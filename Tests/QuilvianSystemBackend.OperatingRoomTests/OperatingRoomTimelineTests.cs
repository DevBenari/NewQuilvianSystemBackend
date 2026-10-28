using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

/// <summary>
/// Uji spesifikasi engine timeline ruang OK dan buffer sterilisasi otomatis (BE-OPR-012, OPS-DEC-038, OPS-DEC-039).
/// </summary>
public class OperatingRoomTimelineTests
{
    [Fact]
    public async Task Timeline_menampilkan_slot_occupied_buffer_dan_available()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();

        // 2026-10-28 09:00 WIB = 02:00 UTC, selesai 10:30 WIB = 03:30 UTC
        var startUtc = new DateTime(2026, 10, 28, 2, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2026, 10, 28, 3, 30, 0, DateTimeKind.Utc);

        await using (var db = harness.NewContext())
        {
            var versi = await alur.VersiAsync(kasusId);
            await harness.SchedulingService(OperatingRoomSeed.AkunBedah, db).ScheduleAsync(kasusId,
                new ScheduleOprCaseRequest
                {
                    RoomId = seed.RuangId,
                    StartAt = startUtc,
                    EndAt = endUtc,
                    BufferBeforeMinutes = 15,
                    BufferAfterMinutes = 45,
                    TeamMembers = alur.TimLengkap(),
                    IdempotencyKey = alur.Kunci("JADWAL-TIMELINE"),
                    ExpectedVersion = versi
                });
        }

        await using (var db = harness.NewContext())
        {
            var service = harness.SchedulingService(OperatingRoomSeed.AkunBedah, db);
            var timeline = await service.GetRoomsTimelineAsync("2026-10-28", null);

            Assert.NotEmpty(timeline);
            var roomTimeline = timeline.FirstOrDefault(x => x.RoomId == seed.RuangId);
            Assert.NotNull(roomTimeline);

            var occupiedSlot = roomTimeline.Slots.FirstOrDefault(x => x.SlotType == "Occupied");
            Assert.NotNull(occupiedSlot);
            Assert.Equal(kasusId, occupiedSlot.CaseId);

            var bufferSlot = roomTimeline.Slots.FirstOrDefault(x => x.SlotType == "SterilizationBuffer");
            Assert.NotNull(bufferSlot);
            Assert.Equal(45, bufferSlot.DurationMinutes);
            Assert.Equal("Pembersihan & Dekontaminasi Ruangan", bufferSlot.Note);

            var availableSlots = roomTimeline.Slots.Where(x => x.SlotType == "Available").ToList();
            Assert.NotEmpty(availableSlots);
        }
    }

    [Fact]
    public async Task Benturan_jadwal_pada_buffer_sterilisasi_ditolak_409_conflict()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasus1 = await alur.BuatKasusAsync();
        var kasus2 = await alur.BuatKasusAsync();

        // Kasus 1: 09:00 - 10:30 WIB (02:00 - 03:30 UTC), buffer after 45 menit -> kamar steril sampai 11:15 WIB (04:15 UTC)
        var start1 = new DateTime(2026, 10, 29, 2, 0, 0, DateTimeKind.Utc);
        var end1 = new DateTime(2026, 10, 29, 3, 30, 0, DateTimeKind.Utc);

        await using (var db = harness.NewContext())
        {
            var v1 = await alur.VersiAsync(kasus1);
            await harness.SchedulingService(OperatingRoomSeed.AkunBedah, db).ScheduleAsync(kasus1,
                new ScheduleOprCaseRequest
                {
                    RoomId = seed.RuangId,
                    StartAt = start1,
                    EndAt = end1,
                    BufferBeforeMinutes = 15,
                    BufferAfterMinutes = 45,
                    TeamMembers = alur.TimLengkap(),
                    IdempotencyKey = alur.Kunci("JADWAL-1"),
                    ExpectedVersion = v1
                });
        }

        // Kasus 2: Mencoba masuk pukul 10:45 - 12:00 WIB (03:45 - 05:00 UTC) pada ruang yang sama
        // 10:45 WIB mendahului 11:15 WIB (buffer sterilisasi belum selesai) -> harus ditolak OPR003
        var start2 = new DateTime(2026, 10, 29, 3, 45, 0, DateTimeKind.Utc);
        var end2 = new DateTime(2026, 10, 29, 5, 0, 0, DateTimeKind.Utc);

        await using (var db = harness.NewContext())
        {
            var v2 = await alur.VersiAsync(kasus2);
            var ex = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
                harness.SchedulingService(OperatingRoomSeed.AkunBedah, db).ScheduleAsync(kasus2,
                    new ScheduleOprCaseRequest
                    {
                        RoomId = seed.RuangId,
                        StartAt = start2,
                        EndAt = end2,
                        BufferBeforeMinutes = 15,
                        BufferAfterMinutes = 45,
                        TeamMembers = alur.TimLengkap(),
                        IdempotencyKey = alur.Kunci("JADWAL-2"),
                        ExpectedVersion = v2
                    }));

            Assert.Equal("OPR003", ex.Code);
        }
    }

    [Fact]
    public async Task Ruang_tanpa_jadwal_menghasilkan_slot_available_penuh()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);

        await using var db = harness.NewContext();
        var service = harness.SchedulingService(OperatingRoomSeed.AkunBedah, db);

        var timeline = await service.GetRoomsTimelineAsync("2026-11-20", null);

        Assert.NotEmpty(timeline);
        foreach (var room in timeline)
        {
            Assert.Single(room.Slots);
            Assert.Equal("Available", room.Slots[0].SlotType);
        }
    }

    [Fact]
    public async Task LookupBookingByPatient_mengembalikan_detail_jadwal_dan_tim()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();
        var startUtc = new DateTime(2026, 10, 28, 2, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2026, 10, 28, 3, 30, 0, DateTimeKind.Utc);

        await using (var db = harness.NewContext())
        {
            var versi = await alur.VersiAsync(kasusId);
            await harness.SchedulingService(OperatingRoomSeed.AkunBedah, db).ScheduleAsync(kasusId,
                new ScheduleOprCaseRequest
                {
                    RoomId = seed.RuangId,
                    StartAt = startUtc,
                    EndAt = endUtc,
                    BufferBeforeMinutes = 15,
                    BufferAfterMinutes = 45,
                    TeamMembers = alur.TimLengkap(),
                    IdempotencyKey = alur.Kunci("JADWAL-LOOKUP"),
                    ExpectedVersion = versi
                });
        }

        await using (var db = harness.NewContext())
        {
            var service = harness.SchedulingService(OperatingRoomSeed.AkunBedah, db);
            var result = await service.LookupBookingByPatientAsync(seed.PasienId, null);

            Assert.NotNull(result);
            Assert.Equal(kasusId, result.CaseId);
            Assert.Equal(seed.PasienId, result.PatientId);
            Assert.NotNull(result.LeadSurgeon);
            Assert.NotNull(result.Anesthesiologist);
            Assert.Equal("2026-10-28", result.ScheduledDate);
            Assert.Contains("WIB", result.ScheduledTime);
        }
    }

    [Fact]
    public async Task LookupBookingByPatient_tanpa_jadwal_mengembalikan_null()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);

        await using var db = harness.NewContext();
        var service = harness.SchedulingService(OperatingRoomSeed.AkunBedah, db);
        var randomPatientId = Guid.NewGuid();

        var result = await service.LookupBookingByPatientAsync(randomPatientId, null);
        Assert.Null(result);
    }
}
