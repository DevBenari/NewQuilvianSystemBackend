using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

/// <summary>
/// Penjagaan versi dan pengulangan permintaan (`BE-OPR-003/004/005`, `OPR012`, `OPR013`).
/// </summary>
/// <remarks>
/// Dua petugas yang bekerja atas kasus yang sama, dan satu petugas yang menekan tombol dua
/// kali karena jaringannya lambat, adalah dua kejadian berbeda dengan akibat yang berbeda.
/// Yang pertama harus ditolak supaya perubahan orang lain tidak hilang diam-diam; yang kedua
/// harus diterima tanpa menghasilkan catatan kedua.
/// </remarks>
public class ConcurrencyAndIdempotencyTests
{
    [Fact]
    public async Task Versi_yang_sudah_basi_ditolak()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();
        await alur.JadwalkanAsync(kasusId);
        await alur.ChecklistSignInSelesaiAsync(kasusId);

        var versiBasi = 0;

        var galat = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            harness.PreparationService(OperatingRoomSeed.AkunBedah).CreateSignOffAsync(kasusId,
                new CreateOprReadinessSignOffRequest
                {
                    Role = OprReadinessRole.PrimarySurgeon,
                    IdempotencyKey = alur.Kunci("BASI"),
                    ExpectedVersion = versiBasi
                }));

        Assert.Equal("OPR012", galat.Code);
    }

    [Fact]
    public async Task Pengulangan_dengan_kunci_dan_isi_sama_tidak_menghasilkan_kasus_kedua()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        await using var awal = harness.NewContext();
        var tindakan = await seed.TindakanBelumTerpakaiAsync(awal);

        var kunci = alur.Kunci("ULANG");
        var pertama = await alur.BuatKasusAsync(tindakan, kunci);
        var kedua = await alur.BuatKasusAsync(tindakan, kunci);

        Assert.Equal(pertama, kedua);

        await using var db = harness.NewContext();
        Assert.Equal(1, await db.OprCases.CountAsync(x => !x.IsDelete));
    }

    [Fact]
    public async Task Kunci_yang_sama_dengan_isi_berbeda_ditolak()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kunci = alur.Kunci("BEDA");
        await alur.BuatKasusAsync(kunci: kunci);

        await using var db = harness.NewContext();
        var tindakanLain = await seed.TindakanBelumTerpakaiAsync(db);

        var galat = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            harness.CaseService(OperatingRoomSeed.AkunBedah, seed.DokterBedahId).CreateAsync(
                new CreateOprCaseRequest
                {
                    PatientId = seed.PasienId,
                    EncounterId = seed.KunjunganId,
                    RequesterDoctorId = seed.DokterBedahId,
                    PrimarySurgeonId = seed.DokterBedahId,
                    CaseType = OprCaseType.Elective,
                    Priority = OprPriority.Urgent,
                    Indication = "Indikasi yang berbeda dari permintaan pertama",
                    EstimatedMinutes = 120,
                    Procedures = [new OprCaseProcedureRequest { PatientProcedureId = tindakanLain, IsPrimary = true }],
                    IdempotencyKey = kunci
                }));

        Assert.Equal("OPR013", galat.Code);
    }

    [Fact]
    public async Task Sign_off_yang_diulang_tidak_menghasilkan_riwayat_kedua()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();
        await alur.JadwalkanAsync(kasusId);
        await alur.ChecklistSignInSelesaiAsync(kasusId);

        var kunci = alur.Kunci("SOULANG");
        await alur.SignOffAsync(kasusId, OprReadinessRole.PrimarySurgeon, kunci);
        await alur.SignOffAsync(kasusId, OprReadinessRole.PrimarySurgeon, kunci);

        await using var db = harness.NewContext();
        var jumlah = await db.OprStatusHistories.CountAsync(x =>
            x.OprCaseId == kasusId && x.CorrelationId == kunci && !x.IsDelete);

        Assert.Equal(1, jumlah);
    }

    [Fact]
    public async Task Peran_yang_sama_tidak_boleh_sign_off_dua_kali()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();
        await alur.JadwalkanAsync(kasusId);
        await alur.ChecklistSignInSelesaiAsync(kasusId);
        await alur.SignOffAsync(kasusId, OprReadinessRole.PrimarySurgeon);

        var galat = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            alur.SignOffAsync(kasusId, OprReadinessRole.PrimarySurgeon));

        Assert.Equal("OPR006", galat.Code);
    }

    [Fact]
    public async Task Dua_kasus_tidak_dapat_memakai_ruang_pada_waktu_yang_beririsan()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var mulai = DateTime.UtcNow.AddDays(10).Date.AddHours(2);

        var pertama = await alur.BuatKasusAsync();
        await alur.JadwalkanAsync(pertama, mulai);

        var kedua = await alur.BuatKasusAsync();

        var galat = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            alur.JadwalkanAsync(kedua, mulai.AddMinutes(30)));

        Assert.Equal("OPR003", galat.Code);
    }

    [Fact]
    public async Task Penjadwalan_ulang_menyimpan_jadwal_lama_sebagai_riwayat()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();
        var awal = DateTime.UtcNow.AddDays(12).Date.AddHours(2);
        await alur.JadwalkanAsync(kasusId, awal);
        await alur.JadwalkanAsync(kasusId, awal.AddDays(1));

        await using var db = harness.NewContext();
        var jadwal = await db.OprSchedules.AsNoTracking()
            .Where(x => x.OprCaseId == kasusId && !x.IsDelete).ToListAsync();

        Assert.Equal(2, jadwal.Count);
        Assert.Single(jadwal.Where(x => x.IsCurrent));
        Assert.Contains(jadwal, x => !x.IsCurrent && x.StartAt == awal);
    }

    [Fact]
    public async Task Tindakan_yang_sudah_dipakai_kasus_lain_ditolak()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        await using var db = harness.NewContext();
        var tindakan = await seed.TindakanBelumTerpakaiAsync(db);

        await alur.BuatKasusAsync(tindakan);

        var galat = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            alur.BuatKasusAsync(tindakan));

        Assert.Equal("OPR002", galat.Code);
    }
}
