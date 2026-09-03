using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Radiology;

/// <summary>
/// Acceptance criteria <c>RJ-BIL-BE-004</c> terhadap database sungguhan.
///
/// Skenario yang dibuktikan:
///   1. Aturan keselamatan yang belum ditetapkan menolak acquisition — fail-closed.
///   2. Identitas yang belum diverifikasi menolak acquisition.
///   3. Butir wajib yang belum dijawab menolak acquisition.
///   4. Butir wajib yang dijawab gagal menolak acquisition.
///   5. Gerbang yang tuntas meloloskan acquisition.
///   6. Requested, Accepted, dan Scheduled tidak membentuk tagihan apa pun.
///   7. Study yang dapat dipakai membentuk tepat satu fakta kelayakan tagih.
///   8. Study yang kualitasnya ditolak tidak membentuk tagihan sama sekali.
///   9. Pengulangan membuat study baru dan mempertahankan study aslinya utuh.
///  10. Pengulangan karena kebutuhan klinis baru menuntut pesanan tambahan.
///  11. Acquisition yang dihentikan mencatat sebab dan konsumsi tanpa menagih.
///  12. Pesanan tidak dapat dibatalkan ketika ada study yang sudah disinari.
/// </summary>
public sealed class RadiologyStudyLifecycleTests
    : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
{
    private readonly BillingTestDatabaseFixture _fixture;
    private readonly List<EncounterSeed> _seeds = new();
    private readonly List<Guid> _procedureIds = new();
    private readonly List<Guid> _modalityIds = new();
    private readonly List<Guid> _requirementIds = new();
    private readonly List<Guid> _ruleIds = new();

    private Guid _actorUserId = Guid.NewGuid();

    public RadiologyStudyLifecycleTests(BillingTestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var seed in _seeds)
            await _fixture.CleanupEncounterAsync(seed);

        await using var context = _fixture.CreateContext();

        await context.MstRadModalitySafetyRules
            .Where(x => _ruleIds.Contains(x.Id)).ExecuteDeleteAsync();

        await context.MstRadSafetyRequirements
            .Where(x => _requirementIds.Contains(x.Id)).ExecuteDeleteAsync();

        await context.MstRadModalities
            .Where(x => _modalityIds.Contains(x.Id)).ExecuteDeleteAsync();

        await context.Set<MstProcedure>()
            .Where(x => _procedureIds.Contains(x.Id)).ExecuteDeleteAsync();
    }

    /* ================================================================== *
     * Kriteria 1 — acquisition ditolak tanpa gerbang
     * ================================================================== */

    [Fact]
    public async Task AturanKeselamatanBelumDitetapkan_AcquisitionDitolak()
    {
        // Fail-closed yang dituntut RJ-BIL-DEC-014. Modalitas tanpa satu pun aturan aktif
        // menolak acquisition, bukan meloloskannya.
        var konteks = await SiapkanAsync(denganAturan: false);

        var study = await BuatStudyAsync(konteks);
        await VerifikasiIdentitasAsync(study.Id);

        var hasil = await JalankanAsync(s => s.StartAcquisitionAsync(study.Id));

        Assert.Equal(RadOperationResultKind.PolicyNotConfigured, hasil.Kind);
        Assert.Equal(RadErrorCodes.SafetyPolicyNotConfigured, hasil.ErrorCode);
    }

    [Fact]
    public async Task IdentitasBelumDiverifikasi_AcquisitionDitolak()
    {
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);

        var hasil = await JalankanAsync(s => s.StartAcquisitionAsync(study.Id));

        Assert.Equal(RadOperationResultKind.SafetyBlocked, hasil.Kind);
        Assert.Equal(RadErrorCodes.IdentityNotVerified, hasil.ErrorCode);
    }

    [Fact]
    public async Task ButirWajibBelumDijawab_AcquisitionDitolak()
    {
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);
        await VerifikasiIdentitasAsync(study.Id);

        var hasil = await JalankanAsync(s => s.ClearSafetyAsync(study.Id));

        Assert.Equal(RadOperationResultKind.SafetyBlocked, hasil.Kind);
        Assert.Equal(RadErrorCodes.SafetyGateNotCleared, hasil.ErrorCode);
        Assert.Contains(konteks.KodeButirWajib, hasil.ErrorMessage);
    }

    [Fact]
    public async Task ButirWajibDijawabGagal_AcquisitionDitolak()
    {
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);
        await VerifikasiIdentitasAsync(study.Id);

        await JalankanAsync(s => s.DecideSafetyCheckAsync(study.Id, new RadSafetyCheckDecisionRequest
        {
            SafetyRequirementId = konteks.RequirementWajibId,
            CheckState = RadSafetyCheckState.Failed,
            Note = "Pasien menyatakan sedang hamil.",
        }));

        var hasil = await JalankanAsync(s => s.ClearSafetyAsync(study.Id));

        Assert.Equal(RadOperationResultKind.SafetyBlocked, hasil.Kind);
        Assert.Contains("tidak aman", hasil.ErrorMessage);
    }

    [Fact]
    public async Task GerbangTuntas_AcquisitionBerjalan()
    {
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);

        await LoloskanGerbangAsync(study.Id, konteks);

        var hasil = await JalankanAsync(s => s.StartAcquisitionAsync(study.Id));

        Assert.True(hasil.IsSuccess);
        Assert.Equal(nameof(RadStudyStatus.AcquisitionStarted), hasil.Value!.StudyStatus);
        Assert.NotNull(hasil.Value.AcquisitionStartedAt);
    }

    /* ================================================================== *
     * Kriteria 2 — hanya study yang dapat dipakai yang menagih
     * ================================================================== */

    [Fact]
    public async Task RequestedAcceptedScheduled_TidakMembentukTagihan()
    {
        var konteks = await SiapkanAsync();

        // Pesanan sudah dibuat, diterima, dan dijadwalkan; belum ada satu pun acquisition.
        await JalankanOrderAsync(s => s.AcceptAsync(konteks.OrderId, new RadOrderTransitionRequest()));
        await JalankanOrderAsync(s => s.ScheduleAsync(konteks.OrderId, new RadOrderTransitionRequest
        {
            ScheduledAt = DateTime.UtcNow.AddHours(2),
        }));

        Assert.Equal(0, await JumlahBarisTagihanAsync(konteks.Seed.EncounterId));
    }

    [Fact]
    public async Task StudyYangDapatDipakai_MembentukTepatSatuBarisTagihan()
    {
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);

        await LoloskanGerbangAsync(study.Id, konteks);
        await JalankanAsync(s => s.StartAcquisitionAsync(study.Id));
        await JalankanAsync(s => s.CompleteAcquisitionAsync(study.Id));

        var hasil = await JalankanAksiAsync(s => s.DecideQualityAsync(study.Id,
            new RadAcquisitionQualityRequest { IsUsable = true }));

        Assert.True(hasil.IsSuccess);
        Assert.Equal(nameof(RadStudyStatus.QualityAccepted), hasil.Value!.Study.StudyStatus);
        Assert.NotNull(hasil.Value.Handoff);
        Assert.Equal(1, await JumlahBarisTagihanAsync(konteks.Seed.EncounterId));
    }

    [Fact]
    public async Task StudyYangKualitasnyaDitolak_TidakMembentukTagihan()
    {
        // GATE-DEC-004: kegagalan kualitas masuk alur pengecualian, bukan otomatis tagihan penuh.
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);

        await LoloskanGerbangAsync(study.Id, konteks);
        await JalankanAsync(s => s.StartAcquisitionAsync(study.Id));
        await JalankanAsync(s => s.CompleteAcquisitionAsync(study.Id));

        var hasil = await JalankanAksiAsync(s => s.DecideQualityAsync(study.Id,
            new RadAcquisitionQualityRequest { IsUsable = false, QualityNote = "Citra kabur." }));

        Assert.True(hasil.IsSuccess);
        Assert.Equal(nameof(RadStudyStatus.QualityRejected), hasil.Value!.Study.StudyStatus);
        Assert.Null(hasil.Value.Handoff);
        Assert.Equal(0, await JumlahBarisTagihanAsync(konteks.Seed.EncounterId));
    }

    [Fact]
    public async Task AcquisitionYangDihentikan_MencatatSebabDanKonsumsiTanpaMenagih()
    {
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);

        await LoloskanGerbangAsync(study.Id, konteks);
        await JalankanAsync(s => s.StartAcquisitionAsync(study.Id));

        // Kontras sudah disuntikkan sebelum acquisition dihentikan. Ia tetap terpakai.
        await JalankanKonsumsiAsync(s => s.RecordConsumptionAsync(study.Id, new RadConsumptionRequest
        {
            ItemType = RadConsumptionItemType.Contrast,
            ItemCode = "CONTRAST-100",
            ItemName = "Media kontras 100 ml",
            Quantity = 100m,
            Unit = "ml",
            ConsumedDespiteFailure = true,
        }));

        var hasil = await JalankanAsync(s => s.AbortAcquisitionAsync(study.Id,
            new RadAbortAcquisitionRequest
            {
                AbortCause = RadAbortCause.PatientCondition,
                AbortReason = "Pasien mual berat, pemeriksaan dihentikan.",
                PerformedPortionNote = "Satu sekuens selesai dari tiga.",
            }));

        Assert.True(hasil.IsSuccess);
        Assert.Equal(nameof(RadStudyStatus.Aborted), hasil.Value!.StudyStatus);
        Assert.Equal(nameof(RadAbortCause.PatientCondition), hasil.Value.AbortCause);
        Assert.Single(hasil.Value.Consumptions);
        Assert.True(hasil.Value.Consumptions[0].ConsumedDespiteFailure);

        // Tidak ada tagihan otomatis. Billing yang menilai konsumsi yang terlanjur terpakai.
        Assert.Equal(0, await JumlahBarisTagihanAsync(konteks.Seed.EncounterId));
    }

    /* ================================================================== *
     * Kriteria 3 — pengulangan mempertahankan study aslinya
     * ================================================================== */

    [Fact]
    public async Task Pengulangan_MempertahankanStudyAsliUtuh()
    {
        var konteks = await SiapkanAsync();
        var asli = await BuatStudyAsync(konteks);

        await LoloskanGerbangAsync(asli.Id, konteks);
        await JalankanAsync(s => s.StartAcquisitionAsync(asli.Id));
        await JalankanAsync(s => s.CompleteAcquisitionAsync(asli.Id));
        await JalankanAksiAsync(s => s.DecideQualityAsync(asli.Id,
            new RadAcquisitionQualityRequest { IsUsable = false, QualityNote = "Gerakan pasien." }));

        var sebelum = await AmbilStudyAsync(asli.Id);

        var hasil = await JalankanAsync(s => s.RepeatStudyAsync(asli.Id, new RadRepeatStudyRequest
        {
            RepeatCause = RadRepeatCause.InternalHospitalError,
            RepeatReason = "Posisi pasien salah saat pengambilan pertama.",
        }));

        Assert.True(hasil.IsSuccess);

        var sesudah = await AmbilStudyAsync(asli.Id);
        var ulangan = await AmbilStudyAsync(hasil.Value!.Id);

        // Study asli tidak disentuh sama sekali: status, waktu, dan penilaiannya tetap.
        Assert.Equal(sebelum.StudyStatus, sesudah.StudyStatus);
        Assert.Equal(sebelum.AcquisitionStartedAt, sesudah.AcquisitionStartedAt);
        Assert.Equal(sebelum.IsUsable, sesudah.IsUsable);
        Assert.Equal(sebelum.StudyNumber, sesudah.StudyNumber);

        // Study ulangan menunjuk ke aslinya beserta sebabnya.
        Assert.Equal(asli.Id, ulangan.RepeatOfStudyId);
        Assert.Equal(RadRepeatCause.InternalHospitalError, ulangan.RepeatCause);
        Assert.NotEqual(sebelum.StudyNumber, ulangan.StudyNumber);
        Assert.True(ulangan.StudySequence > sesudah.StudySequence);
    }

    [Fact]
    public async Task PengulanganKarenaKebutuhanKlinisBaru_MenuntutPesananTambahan()
    {
        // GATE-DEC-004 menuntut mekanisme order untuk kebutuhan klinis baru, bukan alasan bebas.
        var konteks = await SiapkanAsync();
        var asli = await BuatStudyAsync(konteks);

        await LoloskanGerbangAsync(asli.Id, konteks);
        await JalankanAsync(s => s.StartAcquisitionAsync(asli.Id));
        await JalankanAsync(s => s.CompleteAcquisitionAsync(asli.Id));
        await JalankanAksiAsync(s => s.DecideQualityAsync(asli.Id,
            new RadAcquisitionQualityRequest { IsUsable = true }));

        var hasil = await JalankanAsync(s => s.RepeatStudyAsync(asli.Id, new RadRepeatStudyRequest
        {
            RepeatCause = RadRepeatCause.NewClinicalRequirement,
            RepeatReason = "Dokter meminta potongan tambahan.",
        }));

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal(RadErrorCodes.RepeatAuthorizationRequired, hasil.ErrorCode);
    }

    [Fact]
    public async Task StudyYangBelumPernahDikerjakan_TidakDapatDiulang()
    {
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);

        var hasil = await JalankanAsync(s => s.RepeatStudyAsync(study.Id, new RadRepeatStudyRequest
        {
            RepeatCause = RadRepeatCause.InternalHospitalError,
            RepeatReason = "Salah pilih.",
        }));

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal(RadErrorCodes.RepeatSourceInvalid, hasil.ErrorCode);
    }

    /* ================================================================== *
     * Penjaga tambahan
     * ================================================================== */

    [Fact]
    public async Task PesananTidakDapatDibatalkanKetikaAdaStudyYangSudahDisinari()
    {
        // Membatalkan pada tingkat pesanan akan menyembunyikan paparan yang sudah terjadi.
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);

        await LoloskanGerbangAsync(study.Id, konteks);
        await JalankanAsync(s => s.StartAcquisitionAsync(study.Id));

        var hasil = await JalankanOrderAsync(s => s.CancelAsync(konteks.OrderId,
            new RadOrderTransitionRequest { Reason = "Pasien pulang." }));

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal(RadErrorCodes.InvalidTransition, hasil.ErrorCode);
    }

    [Fact]
    public async Task JawabanKeselamatanTidakDapatDiubahSetelahAcquisitionDimulai()
    {
        // Mengubah catatan keselamatan setelah pasien disinari berarti menulis ulang sejarah.
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);

        await LoloskanGerbangAsync(study.Id, konteks);
        await JalankanAsync(s => s.StartAcquisitionAsync(study.Id));

        var hasil = await JalankanAsync(s => s.DecideSafetyCheckAsync(study.Id,
            new RadSafetyCheckDecisionRequest
            {
                SafetyRequirementId = konteks.RequirementWajibId,
                CheckState = RadSafetyCheckState.Failed,
            }));

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
    }

    [Fact]
    public async Task RiwayatTransisiMencatatSetiapLangkah()
    {
        var konteks = await SiapkanAsync();
        var study = await BuatStudyAsync(konteks);

        await LoloskanGerbangAsync(study.Id, konteks);
        await JalankanAsync(s => s.StartAcquisitionAsync(study.Id));

        await using var context = _fixture.CreateContext();
        var service = BuatStudyService(context);

        var riwayat = await service.GetHistoryAsync(konteks.OrderId);

        Assert.Contains(riwayat, x => x.Action == "Order.Create");
        Assert.Contains(riwayat, x => x.Action == "Study.Create");
        Assert.Contains(riwayat, x => x.Action == "Study.VerifyPatient");
        Assert.Contains(riwayat, x => x.Action == "Study.ClearSafety");
        Assert.Contains(riwayat, x => x.Action == "Study.StartAcquisition");
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private sealed record KonteksUji(
        EncounterSeed Seed,
        Guid OrderId,
        Guid ModalityId,
        Guid RequirementWajibId,
        string KodeButirWajib);

    private async Task<KonteksUji> SiapkanAsync(bool denganAturan = true)
    {
        var seed = await _fixture.SeedEncounterAsync();
        _seeds.Add(seed);
        _actorUserId = seed.ActorUserId;

        var suffix = Guid.NewGuid().ToString("N")[..10];

        await using (var context = _fixture.CreateContext())
        {
            // Tarif sengaja tidak dibuat. Radiologi tidak mengirim snapshot tarif sama sekali:
            // GATE-DEC-004 menempatkan perhitungan biaya pada Billing, dan fakta yang dikirim
            // modul ini hanya menyatakan bahwa pemeriksaannya benar-benar terjadi.
            var procedure = new MstProcedure
            {
                Id = Guid.NewGuid(),
                ProcedureCode = $"RD{suffix}",
                ProcedureName = $"Pemeriksaan Radiologi {suffix}",
                ProcedureType = "Radiology",
                IsActive = true,
            };

            var modality = new MstRadModality
            {
                Id = Guid.NewGuid(),
                ModalityCode = $"MD{suffix}",
                ModalityName = $"Modalitas Test {suffix}",
                UsesIonisingRadiation = true,
                IsActive = true,
            };

            var requirement = new MstRadSafetyRequirement
            {
                Id = Guid.NewGuid(),
                RequirementCode = $"REQ{suffix}",
                RequirementName = "Skrining kehamilan",
                Category = "Radiation",
                IsActive = true,
            };

            context.Add(procedure);
            context.MstRadModalities.Add(modality);
            context.MstRadSafetyRequirements.Add(requirement);

            if (denganAturan)
            {
                var rule = new MstRadModalitySafetyRule
                {
                    Id = Guid.NewGuid(),
                    ModalityId = modality.Id,
                    ProcedureId = null,
                    SafetyRequirementId = requirement.Id,
                    IsMandatory = true,
                    IsActive = true,
                    RuleVersion = 1,
                    EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                };

                context.MstRadModalitySafetyRules.Add(rule);
                _ruleIds.Add(rule.Id);
            }

            await context.SaveChangesAsync();

            _procedureIds.Add(procedure.Id);
            _modalityIds.Add(modality.Id);
            _requirementIds.Add(requirement.Id);

            var order = await BuatOrderAsync(seed.EncounterId, procedure.Id, modality.Id);

            return new KonteksUji(seed, order, modality.Id, requirement.Id,
                requirement.RequirementCode);
        }
    }

    private async Task<Guid> BuatOrderAsync(Guid encounterId, Guid procedureId, Guid modalityId)
    {
        await using var context = _fixture.CreateContext();
        var service = BuatOrderService(context);

        var hasil = await service.CreateAsync(new CreateRadOrderRequest
        {
            EncounterId = encounterId,
            ProcedureId = procedureId,
            ModalityId = modalityId,
            ClinicalIndication = "Indikasi test.",
        });

        Assert.True(hasil.IsSuccess);
        return hasil.Value!.Id;
    }

    private async Task<RadStudyResponse> BuatStudyAsync(KonteksUji konteks)
    {
        var hasil = await JalankanAsync(s =>
            s.CreateStudyAsync(konteks.OrderId, new CreateRadStudyRequest()));

        Assert.True(hasil.IsSuccess);
        return hasil.Value!;
    }

    private async Task VerifikasiIdentitasAsync(Guid studyId)
    {
        var hasil = await JalankanAsync(s => s.VerifyPatientAsync(studyId));
        Assert.True(hasil.IsSuccess);
    }

    private async Task LoloskanGerbangAsync(Guid studyId, KonteksUji konteks)
    {
        await VerifikasiIdentitasAsync(studyId);

        var jawab = await JalankanAsync(s => s.DecideSafetyCheckAsync(studyId,
            new RadSafetyCheckDecisionRequest
            {
                SafetyRequirementId = konteks.RequirementWajibId,
                CheckState = RadSafetyCheckState.Passed,
            }));

        Assert.True(jawab.IsSuccess);

        var lolos = await JalankanAsync(s => s.ClearSafetyAsync(studyId));
        Assert.True(lolos.IsSuccess);
    }

    private async Task<RadStudy> AmbilStudyAsync(Guid studyId)
    {
        await using var context = _fixture.CreateContext();

        return await context.RadStudies
            .AsNoTracking()
            .FirstAsync(x => x.Id == studyId);
    }

    private async Task<int> JumlahBarisTagihanAsync(Guid encounterId)
    {
        await using var context = _fixture.CreateContext();

        var folioIds = await context.BilFolios
            .Where(x => x.EncounterId == encounterId)
            .Select(x => x.Id)
            .ToListAsync();

        if (folioIds.Count == 0) return 0;

        return await context.BilChargeLines
            .CountAsync(x =>
                folioIds.Contains(x.FolioId) &&
                x.SourceContext == "Radiology" &&
                x.CalculationStatus != BillingChargeCalculationStatus.Superseded);
    }

    private RadStudyService BuatStudyService(ApplicationDbContext context) =>
        new(
            context,
            new ClinicalMilestoneFactProducer(
                context,
                new BillingFolioService(context),
                BillingTestDatabaseFixture.CreateLoggerService()),
            BillingTestDatabaseFixture.CreateHttpContextAccessor(_actorUserId),
            BillingTestDatabaseFixture.CreateLoggerService());

    private RadOrderService BuatOrderService(ApplicationDbContext context) =>
        new(
            context,
            BillingTestDatabaseFixture.CreateHttpContextAccessor(_actorUserId),
            BillingTestDatabaseFixture.CreateLoggerService());

    private async Task<RadOperationResult<RadStudyResponse>> JalankanAsync(
        Func<RadStudyService, Task<RadOperationResult<RadStudyResponse>>> aksi)
    {
        await using var context = _fixture.CreateContext();
        return await aksi(BuatStudyService(context));
    }

    private async Task<RadOperationResult<RadStudyActionResult>> JalankanAksiAsync(
        Func<RadStudyService, Task<RadOperationResult<RadStudyActionResult>>> aksi)
    {
        await using var context = _fixture.CreateContext();
        return await aksi(BuatStudyService(context));
    }

    private async Task<RadOperationResult<RadConsumptionResponse>> JalankanKonsumsiAsync(
        Func<RadStudyService, Task<RadOperationResult<RadConsumptionResponse>>> aksi)
    {
        await using var context = _fixture.CreateContext();
        return await aksi(BuatStudyService(context));
    }

    private async Task<RadOperationResult<RadOrderDetailResponse>> JalankanOrderAsync(
        Func<RadOrderService, Task<RadOperationResult<RadOrderDetailResponse>>> aksi)
    {
        await using var context = _fixture.CreateContext();
        return await aksi(BuatOrderService(context));
    }
}
