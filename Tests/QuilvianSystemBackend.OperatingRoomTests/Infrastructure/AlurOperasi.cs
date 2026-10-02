using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

/// <summary>
/// Langkah-langkah alur operasi yang dipakai berulang oleh banyak uji, disusun sekali di sini
/// supaya setiap uji hanya memuat hal yang benar-benar hendak dibuktikannya.
/// </summary>
/// <remarks>
/// Seluruh langkah memanggil service sungguhan di bawah aturan klinis penuh. Tidak ada status
/// yang ditulis langsung ke basis data, sehingga setiap kasus yang sampai ke sebuah status
/// benar-benar melewati semua penjagaan menuju status itu.
/// </remarks>
public sealed class AlurOperasi(OperatingRoomHarness harness, OperatingRoomSeed seed)
{
    private int _nomor;
    private int _hari;

    public string Kunci(string label) => $"UJI-{label}-{Interlocked.Increment(ref _nomor)}";

    public async Task<Guid> BuatKasusAsync(Guid? tindakanId = null, string? kunci = null)
    {
        await using var db = harness.NewContext();
        var tindakan = tindakanId ?? await seed.TindakanBelumTerpakaiAsync(db);

        var service = harness.CaseService(OperatingRoomSeed.AkunBedah, seed.DokterBedahId, db);
        var hasil = await service.CreateAsync(new CreateOprCaseRequest
        {
            PatientId = seed.PasienId,
            EncounterId = seed.KunjunganId,
            RequesterDoctorId = seed.DokterBedahId,
            PrimarySurgeonId = seed.DokterBedahId,
            CaseType = OprCaseType.Elective,
            Priority = OprPriority.Routine,
            Indication = "Indikasi uji otomatis",
            EstimatedMinutes = 90,
            Procedures = [new OprCaseProcedureRequest { PatientProcedureId = tindakan, IsPrimary = true }],
            IdempotencyKey = kunci ?? Kunci("BUAT")
        });
        return hasil.Id;
    }

    public List<OprTeamMemberRequest> TimLengkap() =>
    [
        new() { WorkforceId = seed.TenagaBedahId, Role = OprTeamRole.PrimarySurgeon, IsLead = true },
        new() { WorkforceId = seed.TenagaAnestesiId, Role = OprTeamRole.Anesthesiologist },
        new() { WorkforceId = seed.TenagaPerawatInstrumenId, Role = OprTeamRole.ScrubNurse },
        new() { WorkforceId = seed.TenagaPerawatSirkulerId, Role = OprTeamRole.CirculatingNurse }
    ];

    public async Task JadwalkanAsync(Guid kasusId, DateTime? mulai = null, string? kunci = null)
    {
        await using var db = harness.NewContext();
        var versi = await VersiAsync(kasusId);
        // Tiap kasus memakai hari yang berbeda bila waktunya tidak ditentukan, supaya uji yang
        // menjadwalkan lebih dari satu kasus tidak saling bertabrakan di ruang yang sama.
        var awal = mulai ?? DateTime.UtcNow.AddDays(7 + Interlocked.Increment(ref _hari)).Date.AddHours(2);

        await harness.SchedulingService(OperatingRoomSeed.AkunBedah, db).ScheduleAsync(kasusId,
            new ScheduleOprCaseRequest
            {
                RoomId = seed.RuangId,
                StartAt = awal,
                EndAt = awal.AddHours(2),
                BufferBeforeMinutes = 15,
                BufferAfterMinutes = 15,
                ChangeReason = "Penyesuaian jadwal pada uji otomatis",
                TeamMembers = TimLengkap(),
                IdempotencyKey = kunci ?? Kunci("JADWAL"),
                ExpectedVersion = versi
            });
    }

    public async Task ChecklistSignInSelesaiAsync(Guid kasusId)
    {
        await using var db = harness.NewContext();
        await harness.PreparationService(OperatingRoomSeed.AkunBedah, db).SaveChecklistAsync(kasusId,
            OprChecklistPhase.SignIn, new SaveOprChecklistRequest
            {
                TemplateVersion = "UJI-1.0",
                Complete = true,
                Items =
                [
                    new() { Code = "ID", Label = "Identitas pasien dikonfirmasi", IsMandatory = true, IsChecked = true },
                    new() { Code = "SITE", Label = "Lokasi operasi ditandai", IsMandatory = true, IsChecked = true }
                ],
                IdempotencyKey = Kunci("CHECKLIST"),
                ExpectedVersion = await VersiAsync(kasusId)
            });
    }

    public async Task SignOffAsync(Guid kasusId, OprReadinessRole peran, string? kunci = null)
    {
        var akun = peran switch
        {
            OprReadinessRole.PrimarySurgeon => OperatingRoomSeed.AkunBedah,
            OprReadinessRole.Anesthesiologist => OperatingRoomSeed.AkunAnestesi,
            _ => OperatingRoomSeed.AkunPerawat
        };

        await using var db = harness.NewContext();
        await harness.PreparationService(akun, db).CreateSignOffAsync(kasusId,
            new CreateOprReadinessSignOffRequest
            {
                Role = peran,
                IdempotencyKey = kunci ?? Kunci("SIGNOFF"),
                ExpectedVersion = await VersiAsync(kasusId)
            });
    }

    /// <summary>Membawa kasus dari nol sampai berstatus <c>Ready</c> lewat seluruh gerbangnya.</summary>
    public async Task<Guid> KasusSiapAsync()
    {
        var kasusId = await BuatKasusAsync();
        await JadwalkanAsync(kasusId);
        await ChecklistSignInSelesaiAsync(kasusId);
        await SignOffAsync(kasusId, OprReadinessRole.PrimarySurgeon);
        await SignOffAsync(kasusId, OprReadinessRole.Anesthesiologist);
        await SignOffAsync(kasusId, OprReadinessRole.Nurse);
        return kasusId;
    }

    public async Task<Guid> KasusBerjalanAsync()
    {
        var kasusId = await KasusSiapAsync();
        await MulaiAsync(kasusId);
        return kasusId;
    }

    public async Task MulaiAsync(Guid kasusId, string? kunci = null)
    {
        await using var db = harness.NewContext();
        await harness.ExecutionService(OperatingRoomSeed.AkunBedah, db).StartAsync(kasusId,
            new StartOprCaseRequest
            {
                ConfirmedPatientIdentity = true,
                ConfirmedProcedure = true,
                IdempotencyKey = kunci ?? Kunci("MULAI"),
                ExpectedVersion = await VersiAsync(kasusId)
            });
    }

    public async Task<int> VersiAsync(Guid kasusId)
    {
        await using var db = harness.NewContext();
        return (await db.OprCases.AsNoTracking()
            .Where(x => x.Id == kasusId)
            .Select(x => (int?)x.Version)
            .FirstOrDefaultAsync()) ?? 0;
    }

    public async Task<OprCaseStatus> StatusAsync(Guid kasusId)
    {
        await using var db = harness.NewContext();
        return await db.OprCases.AsNoTracking()
            .Where(x => x.Id == kasusId).Select(x => x.Status).FirstAsync();
    }
}
