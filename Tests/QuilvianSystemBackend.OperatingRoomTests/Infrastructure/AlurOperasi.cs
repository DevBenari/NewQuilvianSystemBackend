using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

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

    /// <summary>
    /// Menyediakan dua gerbang kesiapan yang bukan milik alur Operasi: persetujuan tindakan
    /// pasien dan catatan pra-operasi dari bangsal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Keduanya menyusul sebagai syarat <c>Ready</c> dari integration
    /// (<c>EvaluateReadinessAsync</c> membaca <c>TrxPatientConsent</c> dan
    /// <c>OprWardPreOpService.ReadGateAsync</c>). Tanpa keduanya kasus berhenti di
    /// <c>Scheduled</c>, dan uji yang menuntut kasus siap gagal bukan karena aturan Operasi
    /// melainkan karena prasyarat milik modul lain belum ada.
    /// </para>
    /// <para>
    /// Barisnya ditanam langsung karena keduanya <b>bukan yang sedang dibuktikan</b> di sini:
    /// persetujuan pasien milik Clinical, catatan pra-operasi milik bangsal. Seluruh langkah
    /// Operasi sendiri tetap melewati service sungguhan. Penjagaan kedua gerbang ini diuji
    /// terpisah, lewat kasus yang sengaja dibiarkan tanpa keduanya.
    /// </para>
    /// </remarks>
    public async Task SediakanGerbangLuarAsync(Guid kasusId)
    {
        await using var db = harness.NewContext();

        var kasus = await db.OprCases.AsNoTracking().SingleAsync(x => x.Id == kasusId);

        foreach (var jenis in new[] { PatientConsentType.Surgery, PatientConsentType.Anesthesia })
        {
            db.Set<TrxPatientConsent>().Add(new TrxPatientConsent
            {
                Id = Guid.NewGuid(),
                ConsentNumber = $"UJI-{jenis}-{Guid.NewGuid():N}"[..30],
                PatientId = kasus.PatientId,
                EncounterId = kasus.EncounterId,
                ConsentType = jenis,
                ConsentStatus = PatientConsentStatus.Signed,
                ConsentTitle = $"Persetujuan {jenis} (uji)",
                SignedAt = DateTime.UtcNow.AddHours(-1),
                CreateDateTime = DateTime.UtcNow
            });
        }

        db.OprWardPreOpNotes.Add(new OprWardPreOpNote
        {
            Id = Guid.NewGuid(),
            OprCaseId = kasusId,
            VersionNumber = 1,
            Status = OprWardPreOpStatus.Confirmed,
            MarkingLaterality = kasus.Laterality,
            SiteMarkingConfirmed = true,
            ConfirmedAt = DateTime.UtcNow.AddMinutes(-30),
            CreateDateTime = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Menempatkan pasien pada satu tempat tidur aktif di unit tujuan.
    /// </summary>
    /// <remarks>
    /// <c>VAL-RWF-85</c> dari integration: menerima serah terima menuntut pasien sudah menempati
    /// bed aktif di unit tujuan, dibaca <c>InpPatientLocationQuery</c> dari episode rawat inap
    /// beserta penempatan bed-nya. Keduanya milik Rawat Inap, bukan Operasi, sehingga ditanam
    /// langsung — yang sedang dibuktikan penjagaan serah terima Operasi, bukan alur transfer
    /// pasien.
    /// </remarks>
    public async Task SediakanPasienDiUnitAsync(Guid unitId)
    {
        await using var db = harness.NewContext();

        // FK tetap ditegakkan pada basis data uji Operasi — berbeda dari Farmasi dan Gizi — jadi
        // kelas rawat, kamar, dan tempat tidurnya harus benar-benar ada.
        var kelasId = Guid.NewGuid();
        var kamarId = Guid.NewGuid();
        var bedId = Guid.NewGuid();
        var episodeId = Guid.NewGuid();
        var unik = Guid.NewGuid().ToString("N")[..8];

        db.MstPatientClasses.Add(new MstPatientClass
        {
            Id = kelasId,
            PatientClassCode = $"UJI-KLS-{unik}",
            PatientClassName = "Kelas Uji",
            CreateDateTime = DateTime.UtcNow
        });

        db.MstRooms.Add(new MstRoom
        {
            Id = kamarId,
            ServiceUnitId = unitId,
            RoomCode = $"UJI-KMR-{unik}",
            RoomName = "Kamar Uji",
            Capacity = 1,
            CreateDateTime = DateTime.UtcNow
        });

        db.MstBeds.Add(new MstBed
        {
            Id = bedId,
            RoomId = kamarId,
            BedCode = $"UJI-BED-{unik}",
            BedName = "Tempat Tidur Uji",
            CreateDateTime = DateTime.UtcNow
        });

        db.Set<InpEpisode>().Add(new InpEpisode
        {
            Id = episodeId,
            EpisodeNumber = $"UJI-EP-{unik}",
            EncounterId = seed.KunjunganId,
            PatientId = seed.PasienId,
            ServiceUnitId = unitId,
            PatientClassId = kelasId,
            EpisodeStatus = InpEpisodeStatus.Admitted,
            AdmittedAt = DateTime.UtcNow.AddHours(-4),
            CreateDateTime = DateTime.UtcNow
        });

        db.Set<InpBedPlacement>().Add(new InpBedPlacement
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            BedId = bedId,
            RoomId = kamarId,
            ServiceUnitId = unitId,
            PatientClassId = kelasId,
            PlacedByUserId = OperatingRoomSeed.AkunPerawat,
            SequenceNumber = 1,
            StartDateTime = DateTime.UtcNow.AddHours(-4),
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    /// <summary>Membawa kasus dari nol sampai berstatus <c>Ready</c> lewat seluruh gerbangnya.</summary>
    public async Task<Guid> KasusSiapAsync()
    {
        var kasusId = await BuatKasusAsync();
        await JadwalkanAsync(kasusId);
        await SediakanGerbangLuarAsync(kasusId);
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
