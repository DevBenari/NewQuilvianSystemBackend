using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Perhitungan cakupan dan kewajiban mengikuti perubahan item tagihan secara otomatis, di transaksi yang sama.
///
/// Mesin kalkulasi sengaja diganti pencatat (<see cref="RecordingCalculationService"/>): yang diuji di sini adalah
/// KAPAN dan DENGAN TOKEN APA <c>BillingInvoiceService.UpsertChargeAsync</c> memanggil kalkulasi, serta akibatnya
/// bila kalkulasi gagal — bukan isi angka kalkulasinya (itu urusan pengujian mesin kalkulasi).
///
/// Belum dijalankan: pengujian ini ditulis tanpa build maupun test run.
/// </summary>
public class BillingAutoCalculationOnChargeTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly RecordingCalculationService _calculation;
    private readonly BillingInvoiceService _service;
    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();
    private readonly Guid _tariffId = Guid.NewGuid();
    private readonly Guid _actorUserId = Guid.NewGuid();

    public BillingAutoCalculationOnChargeTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        var allocationService = new BillingAllocationService(_dbContext);
        _calculation = new RecordingCalculationService(_dbContext, allocationService, loggerService);

        _service = new BillingInvoiceService(
            _dbContext,
            new BillingChargeSourceAdapter(),
            new BillingNumberSeriesService(_dbContext),
            _calculation,
            loggerService,
            null!,
            new BilConsumerHandoffService(_dbContext, loggerService));

        _dbContext.MstPatients.Add(new MstPatient
        {
            Id = _patientId,
            MedicalRecordNumber = "MRN-AUTOCALC-001",
            FullName = "Pasien Uji Kalkulasi Otomatis",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            IsActive = true,
            IsDelete = false
        });
        _dbContext.MstTariffCategories.Add(new MstTariffCategory
        {
            Id = _categoryId,
            TariffCategoryCode = "CAT-AUTOCALC-001",
            TariffCategoryName = "Kategori Uji",
            IsActive = true,
            IsDelete = false
        });
        _dbContext.MstTariffs.Add(new MstTariff
        {
            Id = _tariffId,
            TariffCategoryId = _categoryId,
            TariffCode = "TRF-AUTOCALC-001",
            TariffName = "Tarif Uji Kalkulasi Otomatis",
            NormalPrice = 50000m,
            IsActive = true,
            IsDelete = false
        });
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    private (RegPatientEncounter Encounter, BilInvoice Invoice) BuatInvoiceOpen()
    {
        var encounter = new RegPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = "ENC-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            PatientId = _patientId,
            EncounterStatus = EncounterStatus.Billing,
            EncounterDate = DateTime.UtcNow,
            IsActive = true,
            IsDelete = false
        };
        _dbContext.RegPatientEncounters.Add(encounter);

        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-AUTOCALC-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
            ServiceType = "OUTPATIENT",
            Status = BillingInvoiceStatuses.Open,
            RowVersion = Guid.NewGuid(),
            VisitDate = encounter.EncounterDate,
            CreateDateTime = DateTime.UtcNow
        };
        _dbContext.BilInvoices.Add(invoice);
        _dbContext.SaveChanges();
        return (encounter, invoice);
    }

    private UpsertChargeRequest BuatRequest(
        Guid encounterId, string sourceDetailId, int sourceVersion = 1, decimal quantity = 1m) => new()
    {
        EncounterId = encounterId,
        SourceDomain = "ADHOC_CATALOG",
        SourceDetailId = sourceDetailId,
        SourceVersion = sourceVersion,
        SourceStatus = "ADDED",
        OccurredAt = DateTimeOffset.UtcNow,
        CategoryId = _categoryId,
        TariffId = _tariffId,
        DescriptionSnapshot = "Item Uji",
        Quantity = quantity,
        UnitPrice = 50000m,
        ContractVersion = "BIL-CATALOG-0.1"
    };

    [Fact]
    public async Task Item_Baru_Memicu_Tepat_Satu_Kalkulasi_Otomatis_Setelah_Item_Tersimpan()
    {
        var (encounter, invoice) = BuatInvoiceOpen();
        var key = Guid.NewGuid();

        var result = await _service.UpsertChargeAsync(
            BuatRequest(encounter.Id, key.ToString("N")), key, _actorUserId, CancellationToken.None);

        Assert.False(result.IsReplay);
        var call = Assert.Single(_calculation.Calls);
        Assert.Equal(invoice.Id, call.InvoiceId);
        // Item dan receipt sudah tersimpan ketika kalkulasi dipanggil, dan token yang dikirim cocok dengan yang
        // tersimpan — kalkulasi memakai mutasi yang sama, bukan data sebelum item masuk.
        Assert.Equal(1, call.ItemCountAtCall);
        Assert.Equal(call.PersistedRowVersion, call.ExpectedRowVersion);
        Assert.False(string.IsNullOrWhiteSpace(call.Reason));
        Assert.Contains("otomatis", call.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.True(await _dbContext.BilChargeReceipts.AsNoTracking().AnyAsync(x => x.IdempotencyKey == key));
    }

    [Fact]
    public async Task Perubahan_Item_Yang_Sudah_Ada_Dengan_Versi_Sumber_Lebih_Baru_Memicu_Kalkulasi()
    {
        var (encounter, invoice) = BuatInvoiceOpen();
        var firstKey = Guid.NewGuid();
        var sourceDetailId = firstKey.ToString("N");
        _dbContext.BilInvoiceItems.Add(new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            CategoryId = _categoryId,
            SourceDomain = "ADHOC_CATALOG",
            SourceDetailId = sourceDetailId,
            SourceVersion = 1,
            SourceStatus = "ADDED",
            DescriptionSnapshot = "Item Uji",
            Quantity = 1m,
            UnitPrice = 50000m,
            Status = BillingInvoiceItemStatuses.Active
        });
        await _dbContext.SaveChangesAsync();

        var secondKey = Guid.NewGuid();
        await _service.UpsertChargeAsync(
            BuatRequest(encounter.Id, sourceDetailId, sourceVersion: 2, quantity: 3m),
            secondKey, _actorUserId, CancellationToken.None);

        var call = Assert.Single(_calculation.Calls);
        Assert.Equal(invoice.Id, call.InvoiceId);
        Assert.Equal(call.PersistedRowVersion, call.ExpectedRowVersion);
    }

    [Fact]
    public async Task Replay_Dengan_IdempotencyKey_Sama_Tidak_Melahirkan_Kalkulasi_Baru()
    {
        var (encounter, invoice) = BuatInvoiceOpen();
        var key = Guid.NewGuid();
        var request = BuatRequest(encounter.Id, key.ToString("N"));

        await _service.UpsertChargeAsync(request, key, _actorUserId, CancellationToken.None);
        Assert.Single(_calculation.Calls);

        var replay = await _service.UpsertChargeAsync(request, key, _actorUserId, CancellationToken.None);

        Assert.True(replay.IsReplay);
        Assert.Single(_calculation.Calls);
        Assert.Equal(1, await _dbContext.BilInvoiceItems.AsNoTracking().CountAsync(x => x.InvoiceId == invoice.Id));
    }

    [Fact]
    public async Task Kalkulasi_Gagal_Membatalkan_Penambahan_Item_Dengan_Pesan_Spesifik()
    {
        var (encounter, invoice) = BuatInvoiceOpen();
        var key = Guid.NewGuid();
        _calculation.FailWith = new BillingCalculationValidationException("Nilai akhir invoice tidak boleh negatif.");

        var exception = await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            _service.UpsertChargeAsync(
                BuatRequest(encounter.Id, key.ToString("N")), key, _actorUserId, CancellationToken.None));

        // Kegagalan tidak dianggap sukses dan tidak ditelan: pesan menyebut perubahan dibatalkan beserta sebabnya.
        Assert.Contains("dibatalkan", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nilai akhir invoice tidak boleh negatif.", exception.Message);

        // Item dan receipt ikut dibatalkan (rollback), sehingga tidak ada tagihan tanpa kalkulasi yang sah.
        Assert.False(await _dbContext.BilInvoiceItems.AsNoTracking()
            .AnyAsync(x => x.InvoiceId == invoice.Id && x.SourceDetailId == key.ToString("N")));
        Assert.False(await _dbContext.BilChargeReceipts.AsNoTracking().AnyAsync(x => x.IdempotencyKey == key));
    }

    [Fact]
    public async Task Konflik_Kalkulasi_Dipetakan_Ke_Konflik_Invoice_Dan_Membatalkan_Perubahan()
    {
        var (encounter, invoice) = BuatInvoiceOpen();
        var key = Guid.NewGuid();
        _calculation.FailWith = new BillingCalculationConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");

        var exception = await Assert.ThrowsAsync<BillingInvoiceConflictException>(() =>
            _service.UpsertChargeAsync(
                BuatRequest(encounter.Id, key.ToString("N")), key, _actorUserId, CancellationToken.None));

        Assert.Contains("dibatalkan", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(await _dbContext.BilInvoiceItems.AsNoTracking()
            .AnyAsync(x => x.InvoiceId == invoice.Id && x.SourceDetailId == key.ToString("N")));
    }

    private sealed record RecordedCall(
        Guid InvoiceId,
        Guid ExpectedRowVersion,
        Guid PersistedRowVersion,
        int ItemCountAtCall,
        string Reason);

    /// <summary>
    /// Pengganti mesin kalkulasi: mencatat setiap pemanggilan beserta keadaan basis data pada saat itu, tanpa
    /// menghitung apa pun. Bila <see cref="FailWith"/> diisi, pemanggilan melempar pengecualian itu.
    /// </summary>
    private sealed class RecordingCalculationService : BillingCalculationService
    {
        private readonly ApplicationDbContext _db;

        public RecordingCalculationService(
            ApplicationDbContext db,
            BillingAllocationService allocationService,
            LoggerService loggerService)
            : base(db, null!, allocationService, loggerService)
        {
            _db = db;
        }

        public List<RecordedCall> Calls { get; } = new();

        public Exception? FailWith { get; set; }

        public override async Task<CalculationResponse> RecalculateAsync(
            Guid invoiceId,
            RecalculateInvoiceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var persistedRowVersion = await _db.BilInvoices.AsNoTracking()
                .Where(x => x.Id == invoiceId)
                .Select(x => x.RowVersion)
                .SingleAsync(cancellationToken);
            var itemCount = await _db.BilInvoiceItems.AsNoTracking()
                .CountAsync(x => x.InvoiceId == invoiceId && !x.IsDelete, cancellationToken);

            Calls.Add(new RecordedCall(
                invoiceId, request.ExpectedRowVersion, persistedRowVersion, itemCount, request.Reason));

            if (FailWith is not null) throw FailWith;
            return new CalculationResponse { InvoiceId = invoiceId };
        }
    }
}
