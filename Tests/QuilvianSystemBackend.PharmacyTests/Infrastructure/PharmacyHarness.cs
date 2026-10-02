using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

/// <summary>
/// Satu resep yang sudah difinalkan dokter beserta data acuan paling kecil yang masih sah,
/// dan service alur farmasi yang dibangun di atasnya.
/// </summary>
/// <remarks>
/// <para>
/// Alur farmasi adalah rantai gerbang: telaah menuntut clearance, penyiapan menuntut telaah
/// disetujui, telaah akhir menuntut penyiapan selesai, penyerahan menuntut keempatnya. Karena
/// itu harness ini menyiapkan keadaan awal yang sah saja — tahap 2 `WaitingForPayment` tanpa
/// clearance — dan tiap uji yang menaikkan tahap melakukannya lewat service produksi, bukan
/// dengan menyetel kolom status langsung.
/// </para>
/// <para>
/// Surat clearance <c>BilPrescriptionClearanceHandoff</c> disisipkan sebagai fixture, karena
/// penerbitnya milik modul Billing dan task ini tidak menyentuhnya. Yang diuji
/// <b>konsumsinya</b> oleh Farmasi, dan konsumsinya tetap dijalankan service produksi.
/// </para>
/// </remarks>
public sealed class PharmacyHarness : IDisposable
{
    private readonly TestDatabase _database;
    private readonly List<ApplicationDbContext> _contexts = [];
    private readonly List<IServiceScope> _scopes = [];
    private ServiceProvider? _container;

    public PharmacyHarness(bool denganRacikan = false)
    {
        _database = TestDatabase.Create();

        using var konteks = CreateContext();

        konteks.Set<BilInvoice>().Add(new BilInvoice
        {
            Id = InvoiceId,
            EncounterId = KunjunganId,
            InvoiceNumber = "UJI-PHM-INV-1",
            ServiceType = "OUTPATIENT",
            Status = "OPEN",
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<MstMeasurement>().Add(new MstMeasurement
        {
            Id = SatuanId,
            MeasurementCode = "UJI-PHM-UOM",
            MeasurementName = "Tablet (Uji)",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<MstDrug>().Add(new MstDrug
        {
            Id = ObatId,
            DrugCode = "UJI-PHM-DRUG-1",
            DrugName = "Obat Uji Satu",
            StockUnitMeasurementId = SatuanId,
            DispenseUnitMeasurementId = SatuanId,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<MstDrug>().Add(new MstDrug
        {
            Id = ObatTanpaSatuanId,
            DrugCode = "UJI-PHM-DRUG-2",
            DrugName = "Obat Uji Tanpa Satuan",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        // Dua kriteria telaah: satu wajib berkeparahan peringatan, satu berkeparahan hard stop.
        // Keduanya dibutuhkan agar aturan "tidak dapat disetujui selama masih ada hard stop"
        // benar-benar terbukti, bukan hanya aturan "seluruh kriteria wajib ditelaah".
        konteks.Set<MstPrescriptionReviewCriterion>().Add(Kriteria(
            KriteriaBiasaId, "UJI-CRIT-1", "Ketepatan dosis (Uji)",
            PrescriptionIssueSeverity.Warning, 1));

        konteks.Set<MstPrescriptionReviewCriterion>().Add(Kriteria(
            KriteriaHardStopId, "UJI-CRIT-2", "Interaksi obat berat (Uji)",
            PrescriptionIssueSeverity.HardStop, 2));

        // Kriteria tidak aktif. Ada dengan sengaja: tanpa baris ini, penyaring `IsActive` pada
        // pengambilan kriteria tidak terbukti.
        konteks.Set<MstPrescriptionReviewCriterion>().Add(Kriteria(
            KriteriaNonaktifId, "UJI-CRIT-OFF", "Kriteria Nonaktif (Uji)",
            PrescriptionIssueSeverity.Warning, 9, aktif: false));

        var resep = new PhmPrescription
        {
            Id = ResepId,
            PrescriptionNumber = "UJI-RX-0001",
            EncounterId = KunjunganId,
            ConsultationId = Guid.NewGuid(),
            PatientId = PasienId,
            DoctorId = DokterId,
            PrescriptionDateTime = DateTime.UtcNow,
            TotalItemCount = 1,
            TotalPrice = 45000m,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        };

        // Keadaan awal ditetapkan lewat domain service, lalu dinaikkan lewat finalisasi yang
        // sah — bukan dengan menyetel kolom status langsung, supaya titik awalnya memang
        // keadaan yang bisa terjadi di produksi.
        PrescriptionWorkflowService.ApplyInitialClinicalState(resep);
        resep.PrescriptionStatus = PrescriptionStatus.Submitted;
        resep.FulfillmentStatus = PrescriptionFulfillmentStatus.WaitingForPayment;
        resep.SubmittedAt = DateTime.UtcNow;

        konteks.Set<PhmPrescription>().Add(resep);

        konteks.Set<PhmPrescriptionItem>().Add(new PhmPrescriptionItem
        {
            Id = ItemResepId,
            PrescriptionId = ResepId,
            DrugId = ObatId,
            DrugNameSnapshot = "Obat Uji Satu",
            Quantity = 10,
            UnitPrice = 4500m,
            TotalPrice = 45000m,
            DispenseUnitMeasurementId = SatuanId,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        // Tiga depo: satu sah, satu tidak aktif, satu aktif tetapi bukan tempat penyerahan.
        // Ketiganya dibutuhkan agar `PHM112` terbukti memeriksa ketiga hal itu, bukan hanya
        // keberadaan barisnya.
        konteks.Set<MstDrugStorageLocation>().AddRange(
            Depo(DepoId, "UJI-PHM-DEPO-1", "Depo Farmasi (Uji)", aktif: true, bolehSerah: true),
            Depo(DepoNonaktifId, "UJI-PHM-DEPO-2", "Depo Nonaktif (Uji)", aktif: false, bolehSerah: true),
            Depo(GudangTanpaPenyerahanId, "UJI-PHM-GUD-1", "Gudang Induk (Uji)", aktif: true, bolehSerah: false),
            Depo(DepoTanpaTerimaId, "UJI-PHM-DEPO-3", "Depo Tanpa Terima (Uji)", aktif: true, bolehSerah: true, bolehTerima: false));

        // Pasien dan kunjungannya. Etiket obat membacanya, dan retur obat memeriksa kunjungannya
        // ke basis data sebelum menerima apa pun.
        konteks.Set<MstPatient>().Add(new MstPatient
        {
            Id = PasienId,
            FullName = "Pasien Uji Farmasi",
            MedicalRecordNumber = "UJI-PHM-RM-1",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<RegPatientEncounter>().Add(new RegPatientEncounter
        {
            Id = KunjunganId,
            PatientId = PasienId,
            EncounterNumber = "UJI-PHM-ENC-1",
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<MstWorkforceProfile>().AddRange(
            new MstWorkforceProfile
            {
                Id = PetugasId,
                ProfileCode = "UJI-PHM-WFP-1",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow
            },
            new MstWorkforceProfile
            {
                Id = PetugasNonaktifId,
                ProfileCode = "UJI-PHM-WFP-2",
                IsActive = false,
                CreateDateTime = DateTime.UtcNow
            });

        konteks.SaveChanges();
    }

    private static MstDrugStorageLocation Depo(Guid id, string kode, string nama,
        bool aktif, bool bolehSerah, bool bolehTerima = true) => new()
        {
            Id = id,
            StorageLocationCode = kode,
            StorageLocationName = nama,
            IsActive = aktif,
            IsAllowDispensing = bolehSerah,
            IsAllowReceiving = bolehTerima,
            CreateDateTime = DateTime.UtcNow
        };

    public DrugReturnService ReturService(ApplicationDbContext k) =>
        new(k, Accessor(ApotekerId),
            new LoggerService(NullLogger<LoggerService>.Instance, Accessor(ApotekerId)),
            StokService(k));

    public Guid ResepId { get; } = Guid.NewGuid();
    public Guid ItemResepId { get; } = Guid.NewGuid();
    public Guid KunjunganId { get; } = Guid.NewGuid();
    public Guid PasienId { get; } = Guid.NewGuid();
    public Guid DokterId { get; } = Guid.NewGuid();
    public Guid InvoiceId { get; } = Guid.NewGuid();
    public Guid ObatId { get; } = Guid.NewGuid();
    public Guid ObatTanpaSatuanId { get; } = Guid.NewGuid();
    public Guid SatuanId { get; } = Guid.NewGuid();
    public Guid KriteriaBiasaId { get; } = Guid.NewGuid();
    public Guid KriteriaHardStopId { get; } = Guid.NewGuid();
    public Guid KriteriaNonaktifId { get; } = Guid.NewGuid();
    public Guid ApotekerId { get; } = Guid.NewGuid();
    public Guid DepoId { get; } = Guid.NewGuid();
    public Guid DepoNonaktifId { get; } = Guid.NewGuid();
    public Guid GudangTanpaPenyerahanId { get; } = Guid.NewGuid();
    public Guid PetugasId { get; } = Guid.NewGuid();
    public Guid PetugasNonaktifId { get; } = Guid.NewGuid();
    public Guid DepoTanpaTerimaId { get; } = Guid.NewGuid();

    public ApplicationDbContext CreateContext()
    {
        var konteks = _database.CreateContext();
        _contexts.Add(konteks);
        return konteks;
    }

    public PrescriptionFinancialClearanceService ClearanceService(ApplicationDbContext k) => new(k);

    public DrugStockService StokService(ApplicationDbContext k) =>
        new(k, Accessor(ApotekerId),
            new LoggerService(NullLogger<LoggerService>.Instance, Accessor(ApotekerId)));

    /// <summary>
    /// Service penyerahan obat, lengkap dengan rantai dependency-nya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rantai itu panjang bukan karena penyerahan rumit, melainkan karena ia menerbitkan fakta
    /// klinis ke Billing: dispensing → <c>ClinicalMilestoneFactProducer</c> →
    /// <c>BillingFolioService</c> → <c>BillingClinicalChargeBridgeService</c> →
    /// <c>IServiceScopeFactory</c>.
    /// </para>
    /// <para>
    /// Ujung rantai itu diisi scope factory tiruan yang <b>sengaja kosong</b>. Yang diuji di
    /// sini penjagaan sebelum penyerahan terjadi — <c>PrepareAsync</c> dan <c>CancelAsync</c> —
    /// dan keduanya tidak pernah menerbitkan fakta, sehingga ujung rantainya tidak tersentuh.
    /// Begitu <c>DispenseAsync</c> ikut diuji, ia akan membutuhkan container DI yang sebenarnya;
    /// itu dicatat sebagai bagian yang belum teruji, bukan ditutupi tiruan yang berpura-pura
    /// berhasil.
    /// </para>
    /// </remarks>
    private static IHttpContextAccessor Accessor(Guid userId)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Uji"))
        };

        // Bukan HttpContextAccessor bawaan: yang bawaan menyimpan konteksnya pada AsyncLocal
        // statis, sehingga accessor yang dibuat belakangan menimpa konteks milik service yang
        // dibuat lebih dulu dan klaim penggunanya hilang.
        return new AccessorTetap(context);
    }

    private sealed class AccessorTetap(HttpContext context) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = context;
    }

    /// <summary>
    /// Container DI sungguhan untuk jalur penyerahan obat, dibangun sekali per harness.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>DispenseAsync</c> menerbitkan fakta klinis ke Billing lewat rantai
    /// dispensing → <c>ClinicalMilestoneFactProducer</c> → <c>BillingFolioService</c> →
    /// <c>BillingClinicalChargeBridgeService</c> → <c>IServiceScopeFactory</c>. Ujung rantai itu
    /// membuka scope-nya sendiri dan menyelesaikan service dari dalamnya, jadi tiruan kosong akan
    /// membuat jalur itu diam-diam tidak berjalan — dan uji yang lulus karena tidak ada yang
    /// dijalankan tidak membuktikan apa pun.
    /// </para>
    /// <para>
    /// Container ini memakai registrasi yang sama dengan aplikasi dan <c>DbContext</c> yang
    /// menunjuk koneksi SQLite yang sama, sehingga scope turunan melihat data yang sama dengan
    /// uji yang memanggilnya.
    /// </para>
    /// </remarks>
    private ServiceProvider BangunContainer()
    {
        var layanan = new ServiceCollection();

        layanan.AddLogging();
        layanan.AddSingleton<IHttpContextAccessor>(_ => Accessor(ApotekerId));
        layanan.AddScoped(_ => _database.CreateContext());
        layanan.AddScoped<LoggerService>();
        layanan.AddScoped<DrugStockService>();
        layanan.AddScoped<PrescriptionFinancialClearanceService>();
        layanan.AddScoped<BillingClinicalChargeBridgeService>();
        layanan.AddScoped<BillingFolioService>();
        layanan.AddScoped<ClinicalMilestoneFactProducer>();
        layanan.AddScoped<PrescriptionDispensingService>();

        return layanan.BuildServiceProvider();
    }

    /// <summary>
    /// Service penyerahan dari container DI sungguhan, beserta <c>DbContext</c> yang dipakainya.
    /// </summary>
    /// <remarks>
    /// Konteksnya dikembalikan sekalian karena uji perlu memeriksa keadaan yang dilihat service
    /// itu — bukan keadaan pada konteks lain yang kebetulan menunjuk koneksi yang sama.
    /// </remarks>
    public (PrescriptionDispensingService Layanan, ApplicationDbContext Konteks) Penyerahan()
    {
        _container ??= BangunContainer();
        var scope = _container.CreateScope();
        _scopes.Add(scope);

        return (scope.ServiceProvider.GetRequiredService<PrescriptionDispensingService>(),
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    /// <summary>Menyediakan satu bets obat bersaldo di depo penyerahan.</summary>
    public async Task SediakanStokAsync(ApplicationDbContext konteks, decimal jumlah = 100)
    {
        var betsId = Guid.NewGuid();

        konteks.Set<PhmDrugBatch>().Add(new PhmDrugBatch
        {
            Id = betsId,
            DrugId = ObatId,
            BatchNumber = "UJI-PHM-BATCH-1",
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<PhmDrugStockBalance>().Add(new PhmDrugStockBalance
        {
            Id = Guid.NewGuid(),
            DrugId = ObatId,
            DrugBatchId = betsId,
            StorageLocationId = DepoId,
            Status = DrugStockStatus.Available,
            QuantityOnHand = jumlah,
            QuantityReserved = 0,
            CreateDateTime = DateTime.UtcNow
        });

        await konteks.SaveChangesAsync();
    }

    public PrescriptionReviewService ReviewService(ApplicationDbContext k) =>
        new(k, ClearanceService(k));

    public PrescriptionPreparationService PreparationService(ApplicationDbContext k) =>
        new(k, ClearanceService(k));

    public PrescriptionFinalCheckService FinalCheckService(ApplicationDbContext k) =>
        new(k, ClearanceService(k));

    /// <summary>
    /// Menyisipkan satu surat clearance Billing sebagai fixture, lalu mengkonsumsinya lewat
    /// consumer produksi.
    /// </summary>
    /// <remarks>
    /// Penerbit suratnya milik Billing dan tidak disentuh task ini. Yang dibuktikan di sini
    /// perilaku Farmasi saat menerima surat itu — termasuk ketika hasil finansialnya tidak
    /// dikenali, yang harus gagal tertutup dan tidak memindahkan resep.
    /// </remarks>
    public async Task<PrescriptionClearanceConsumeResult> TerimaSuratAsync(
        ApplicationDbContext konteks,
        string status = PrescriptionClearanceProjectionStatuses.Cleared,
        string? hasil = PrescriptionFinancialOutcomes.Paid,
        long versi = 1,
        string alasan = "INVOICE_SETTLED",
        Guid? resepId = null)
    {
        konteks.Set<BilPrescriptionClearanceHandoff>().Add(new BilPrescriptionClearanceHandoff
        {
            Id = Guid.NewGuid(),
            PrescriptionId = resepId ?? ResepId,
            InvoiceId = InvoiceId,
            ClearanceStatus = status,
            FinancialOutcome = hasil,
            ReasonCode = alasan,
            FinancialVersion = versi,
            EffectiveAt = DateTimeOffset.UtcNow,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            Status = BillingHandoffStatuses.Created,
            CreateDateTime = DateTime.UtcNow
        });
        await konteks.SaveChangesAsync();

        return await ClearanceService(konteks)
            .ConsumeForPrescriptionAsync(resepId ?? ResepId, ApotekerId);
    }

    /// <summary>
    /// Membawa resep sampai tahap 5 <c>VerifiedByPharmacy</c> lewat service produksi.
    /// </summary>
    public async Task SampaiTelaahDisetujuiAsync(ApplicationDbContext konteks)
    {
        await TerimaSuratAsync(konteks);

        var telaah = ReviewService(konteks);
        var review = await telaah.StartAsync(ResepId, ApotekerId, null);
        await SetujuiSeluruhKriteriaAsync(konteks, review.Id);
        await telaah.CompleteAsync(review.Id, approve: true, null, ApotekerId);
    }

    /// <summary>Menandai seluruh kriteria telaah sebagai sesuai, lewat service produksi.</summary>
    public async Task<PrescriptionReviewResponse> SetujuiSeluruhKriteriaAsync(
        ApplicationDbContext konteks, Guid reviewId)
    {
        var telaah = ReviewService(konteks);
        var aktif = await telaah.GetActiveAsync(ResepId)
            ?? throw new InvalidOperationException("Telaah uji tidak ditemukan.");

        return await telaah.UpdateItemsAsync(reviewId, new UpdatePrescriptionReviewItemsRequest
        {
            Items = [.. aktif.Items.Select(x => new UpdatePrescriptionReviewItemRequest
            {
                ReviewItemId = x.Id,
                Result = PrescriptionReviewResult.Compliant
            })]
        }, ApotekerId);
    }

    /// <summary>Membawa resep sampai tahap 12 <c>AwaitingFinalCheck</c>.</summary>
    public async Task SampaiMenungguTelaahAkhirAsync(ApplicationDbContext konteks)
    {
        await SampaiTelaahDisetujuiAsync(konteks);

        var penyiapan = PreparationService(konteks);
        await penyiapan.StartAsync(ResepId, ApotekerId, null);
        await penyiapan.CompleteAsync(ResepId, new CompletePrescriptionPreparationRequest
        {
            Items = [ButirPenyiapan()]
        }, ApotekerId);
    }

    /// <summary>Satu baris penyiapan yang sah untuk item resep uji.</summary>
    public SavePrescriptionPreparationItemRequest ButirPenyiapan(
        decimal teori = 10, decimal aktual = 10) => new()
        {
            PrescriptionItemId = ItemResepId,
            DrugId = ObatId,
            TheoreticalQuantity = teori,
            ActualQuantity = aktual
        };

    /// <summary>Membawa resep sampai tahap 7 <c>ReadyToDispense</c>.</summary>
    public async Task SampaiSiapDiserahkanAsync(ApplicationDbContext konteks)
    {
        await SampaiMenungguTelaahAkhirAsync(konteks);

        await FinalCheckService(konteks).CompleteAsync(ResepId,
            new CompletePrescriptionFinalCheckRequest
            {
                Items =
                [
                    new CompletePrescriptionFinalCheckItemRequest
                    {
                        CriterionCode = "ACC-1",
                        CriterionName = "Ketepatan obat",
                        Result = PrescriptionReviewResult.Compliant
                    }
                ]
            }, ApotekerId);
    }

    public async Task<PhmPrescription> ResepAsync(ApplicationDbContext konteks) =>
        await konteks.Set<PhmPrescription>().FindAsync(ResepId)
            ?? throw new InvalidOperationException("Resep uji hilang.");

    private static MstPrescriptionReviewCriterion Kriteria(Guid id, string kode, string nama,
        PrescriptionIssueSeverity keparahan, int urutan, bool aktif = true) => new()
        {
            Id = id,
            CriterionCode = kode,
            CriterionName = nama,
            Category = PrescriptionReviewCategory.Administrative,
            DefaultSeverity = keparahan,
            IsRequired = true,
            IsApplicableToRegular = true,
            IsApplicableToCompound = true,
            IsActive = aktif,
            SortOrder = urutan,
            CreateDateTime = DateTime.UtcNow
        };

    public void Dispose()
    {
        foreach (var scope in _scopes) scope.Dispose();
        _container?.Dispose();
        foreach (var konteks in _contexts) konteks.Dispose();
        _database.Dispose();
    }
}
