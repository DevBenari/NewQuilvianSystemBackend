using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
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
/// Pengujian eligibility encounter dan penjaga mutasi pada manual billing / ad-hoc charges.
/// Memastikan bahwa encounter yang sudah selesai (Completed atau memiliki CompletedAt) tidak dapat menerima
/// tagihan manual baru, baik melalui daftar opsi kunjungan maupun mutasi langsung.
/// </summary>
public class BillingInvoiceEligibilityTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingInvoiceService _service;
    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();
    private readonly Guid _tariffId = Guid.NewGuid();
    private readonly Guid _actorUserId = Guid.NewGuid();

    public BillingInvoiceEligibilityTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var httpContextAccessor = new HttpContextAccessor();
        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, httpContextAccessor);
        var sourceAdapter = new BillingChargeSourceAdapter();
        var numberSeries = new BillingNumberSeriesService(_dbContext);
        var allocationService = new BillingAllocationService(_dbContext);
        var calculationService = new BillingCalculationService(_dbContext, null!, allocationService, loggerService);
        var handoffService = new BilConsumerHandoffService(_dbContext, loggerService);

        _service = new BillingInvoiceService(
            _dbContext,
            sourceAdapter,
            numberSeries,
            calculationService,
            loggerService,
            null!,
            handoffService);

        SeedCommonData();
    }

    private void SeedCommonData()
    {
        _dbContext.MstPatients.Add(new MstPatient
        {
            Id = _patientId,
            MedicalRecordNumber = "MRN-TEST-001",
            FullName = "Pasien Uji Manual Billing",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            IsActive = true,
            IsDelete = false
        });

        _dbContext.MstTariffCategories.Add(new MstTariffCategory
        {
            Id = _categoryId,
            TariffCategoryCode = "CAT-TEST-001",
            TariffCategoryName = "Kategori Uji",
            IsActive = true,
            IsDelete = false
        });

        _dbContext.MstTariffs.Add(new MstTariff
        {
            Id = _tariffId,
            TariffCategoryId = _categoryId,
            TariffCode = "TRF-TEST-001",
            TariffName = "Tarif Uji Manual",
            NormalPrice = 50000m,
            IsActive = true,
            IsDelete = false
        });

        _dbContext.SaveChanges();
    }

    private RegPatientEncounter BuatEncounter(EncounterStatus status, DateTime? completedAt = null, bool isCancel = false)
    {
        var encounter = new RegPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = "ENC-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            PatientId = _patientId,
            EncounterStatus = status,
            CompletedAt = completedAt,
            EncounterDate = DateTime.UtcNow,
            IsActive = true,
            IsDelete = false,
            IsCancel = isCancel
        };

        _dbContext.RegPatientEncounters.Add(encounter);
        _dbContext.SaveChanges();
        return encounter;
    }

    [Fact]
    public async Task A_EncounterStatus_Billing_CompletedAt_Null_Muncul_Di_EncounterOptions()
    {
        var enc = BuatEncounter(EncounterStatus.Billing, completedAt: null);

        var result = await _service.GetActiveEncounterOptionsAsync(enc.EncounterNumber, 10, CancellationToken.None);

        Assert.Contains(result, x => x.Id == enc.Id);
    }

    [Fact]
    public async Task B_EncounterStatus_Completed_CompletedAt_NotNull_TIDAK_Muncul_Di_EncounterOptions()
    {
        var enc = BuatEncounter(EncounterStatus.Completed, completedAt: DateTime.UtcNow);

        var result = await _service.GetActiveEncounterOptionsAsync(enc.EncounterNumber, 10, CancellationToken.None);

        Assert.DoesNotContain(result, x => x.Id == enc.Id);
    }

    [Fact]
    public async Task C_EncounterStatus_Completed_CompletedAt_Null_Tetap_TIDAK_Muncul_Di_EncounterOptions()
    {
        var enc = BuatEncounter(EncounterStatus.Completed, completedAt: null);

        var result = await _service.GetActiveEncounterOptionsAsync(enc.EncounterNumber, 10, CancellationToken.None);

        Assert.DoesNotContain(result, x => x.Id == enc.Id);
    }

    [Fact]
    public async Task D_EncounterStatus_Billing_CompletedAt_NotNull_TIDAK_Muncul_Di_EncounterOptions()
    {
        var enc = BuatEncounter(EncounterStatus.Billing, completedAt: DateTime.UtcNow);

        var result = await _service.GetActiveEncounterOptionsAsync(enc.EncounterNumber, 10, CancellationToken.None);

        Assert.DoesNotContain(result, x => x.Id == enc.Id);
    }

    [Fact]
    public async Task E_EncounterStatus_Cancelled_TIDAK_Muncul_Di_EncounterOptions()
    {
        var enc = BuatEncounter(EncounterStatus.Cancelled, isCancel: true);

        var result = await _service.GetActiveEncounterOptionsAsync(enc.EncounterNumber, 10, CancellationToken.None);

        Assert.DoesNotContain(result, x => x.Id == enc.Id);
    }

    [Fact]
    public async Task F_EncounterStatus_NoShow_TIDAK_Muncul_Di_EncounterOptions()
    {
        var enc = BuatEncounter(EncounterStatus.NoShow);

        var result = await _service.GetActiveEncounterOptionsAsync(enc.EncounterNumber, 10, CancellationToken.None);

        Assert.DoesNotContain(result, x => x.Id == enc.Id);
    }

    [Fact]
    public async Task G_EncounterStatus_Draft_TIDAK_Muncul_Di_EncounterOptions()
    {
        var enc = BuatEncounter(EncounterStatus.Draft);

        var result = await _service.GetActiveEncounterOptionsAsync(enc.EncounterNumber, 10, CancellationToken.None);

        Assert.DoesNotContain(result, x => x.Id == enc.Id);
    }

    [Fact]
    public async Task H_POST_CatalogCharges_Ke_Encounter_Completed_Ditolak_422_Dan_Tidak_Menulis_Item()
    {
        var enc = BuatEncounter(EncounterStatus.Completed, completedAt: DateTime.UtcNow);
        var idempotencyKey = Guid.NewGuid();

        var request = new UpsertChargeRequest
        {
            EncounterId = enc.Id,
            SourceDomain = "ADHOC_CATALOG",
            SourceDetailId = idempotencyKey.ToString("N"),
            SourceVersion = 1,
            SourceStatus = "ADDED",
            OccurredAt = DateTimeOffset.UtcNow,
            CategoryId = _categoryId,
            TariffId = _tariffId,
            DescriptionSnapshot = "Item Uji",
            Quantity = 1,
            UnitPrice = 50000m,
            ContractVersion = "BIL-CATALOG-0.1"
        };

        var ex = await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            _service.UpsertChargeAsync(request, idempotencyKey, _actorUserId, CancellationToken.None));

        Assert.Equal("Kunjungan sudah selesai dan tidak dapat ditambahkan tagihan manual.", ex.Message);

        // Buktikan tidak ada BilInvoiceItem dan BilChargeReceipt yang terbuat
        var itemExists = await _dbContext.BilInvoiceItems.AnyAsync(x => x.SourceDetailId == request.SourceDetailId);
        var receiptExists = await _dbContext.BilChargeReceipts.AnyAsync(x => x.IdempotencyKey == idempotencyKey);
        Assert.False(itemExists);
        Assert.False(receiptExists);
    }

    [Fact]
    public async Task J_Encounter_Completed_Dengan_Invoice_OPEN_Tetap_Ditolak_422()
    {
        var enc = BuatEncounter(EncounterStatus.Completed, completedAt: DateTime.UtcNow);
        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = enc.Id,
            InvoiceNumber = "INV-OPEN-001",
            ServiceType = "OUTPATIENT",
            Status = BillingInvoiceStatuses.Open,
            RowVersion = Guid.NewGuid(),
            VisitDate = enc.EncounterDate,
            CreateDateTime = DateTime.UtcNow
        };
        _dbContext.BilInvoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        var idempotencyKey = Guid.NewGuid();
        var request = new UpsertChargeRequest
        {
            EncounterId = enc.Id,
            SourceDomain = "ADHOC_CATALOG",
            SourceDetailId = idempotencyKey.ToString("N"),
            SourceVersion = 1,
            SourceStatus = "ADDED",
            OccurredAt = DateTimeOffset.UtcNow,
            CategoryId = _categoryId,
            TariffId = _tariffId,
            DescriptionSnapshot = "Item Uji",
            Quantity = 1,
            UnitPrice = 50000m,
            ContractVersion = "BIL-CATALOG-0.1"
        };

        var ex = await Assert.ThrowsAsync<BillingInvoiceValidationException>(() =>
            _service.UpsertChargeAsync(request, idempotencyKey, _actorUserId, CancellationToken.None));

        Assert.Equal("Kunjungan sudah selesai dan tidak dapat ditambahkan tagihan manual.", ex.Message);
    }

    [Fact]
    public async Task K_Replay_IdempotencyKey_Lama_Yang_Sudah_Sukses_Tidak_Menduplikasi_Charge()
    {
        var enc = BuatEncounter(EncounterStatus.Billing);
        var invoiceId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var idempotencyKey = Guid.NewGuid();

        var invoice = new BilInvoice
        {
            Id = invoiceId,
            EncounterId = enc.Id,
            InvoiceNumber = "INV-REPLAY-001",
            ServiceType = "OUTPATIENT",
            Status = BillingInvoiceStatuses.Open,
            RowVersion = Guid.NewGuid(),
            VisitDate = enc.EncounterDate,
            CreateDateTime = DateTime.UtcNow
        };
        var item = new BilInvoiceItem
        {
            Id = itemId,
            InvoiceId = invoiceId,
            CategoryId = _categoryId,
            SourceDomain = "ADHOC_CATALOG",
            SourceDetailId = idempotencyKey.ToString("N"),
            SourceVersion = 1,
            SourceStatus = "ADDED",
            DescriptionSnapshot = "Item Replay",
            Quantity = 1,
            UnitPrice = 50000m,
            Status = BillingInvoiceItemStatuses.Active
        };
        var request = new UpsertChargeRequest
        {
            EncounterId = enc.Id,
            SourceDomain = "ADHOC_CATALOG",
            SourceDetailId = idempotencyKey.ToString("N"),
            SourceVersion = 1,
            SourceStatus = "ADDED",
            OccurredAt = DateTimeOffset.UtcNow,
            CategoryId = _categoryId,
            TariffId = _tariffId,
            DescriptionSnapshot = "Item Replay",
            Quantity = 1,
            UnitPrice = 50000m,
            ContractVersion = "BIL-CATALOG-0.1"
        };

        var sourceAdapter = new BillingChargeSourceAdapter();
        var source = sourceAdapter.ValidateAndNormalize(request);
        var payloadHash = BillingInvoiceService.ComputePayloadHash(request, source);

        var receipt = new BilChargeReceipt
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            SourceDomain = "ADHOC_CATALOG",
            SourceDetailId = idempotencyKey.ToString("N"),
            SourceVersion = 1,
            PayloadHash = payloadHash,
            InvoiceItemId = itemId,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.BilInvoices.Add(invoice);
        _dbContext.BilInvoiceItems.Add(item);
        _dbContext.BilChargeReceipts.Add(receipt);
        await _dbContext.SaveChangesAsync();

        // Ketika encounter kini sudah berstatus Completed:
        enc.EncounterStatus = EncounterStatus.Completed;
        enc.CompletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Replay dengan IdempotencyKey yang sama HARUS berhasil kembali tanpa melempar exception dan tanpa duplikasi
        var replayResult = await _service.UpsertChargeAsync(request, idempotencyKey, _actorUserId, CancellationToken.None);

        Assert.NotNull(replayResult);
        Assert.True(replayResult.IsReplay);
        var totalItems = await _dbContext.BilInvoiceItems.CountAsync(x => x.InvoiceId == invoiceId);
        Assert.Equal(1, totalItems);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }
}
