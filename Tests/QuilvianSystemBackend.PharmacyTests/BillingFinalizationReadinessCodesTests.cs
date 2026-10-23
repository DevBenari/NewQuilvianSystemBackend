using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Kesiapan finalisasi: setiap alasan pemblokir memiliki kode yang stabil (BIL-FIN-023..026), kalkulasi yang tertinggal
/// ditolak tanpa efek samping, dan kode turunan tidak mengubah perilaku jalur penutupan otomatis.
///
/// Kasus lapangan: item (mis. Barang Habis Pakai Rp5.000) ditambahkan SESUDAH kalkulasi versi 1 disimpan. Versi itu hanya
/// memuat item yang seluruhnya ditanggung, jadi porsi pasien = 0 dan finalisasi harus menolak sampai kasir menghitung ulang.
/// </summary>
public class BillingFinalizationReadinessCodesTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingFinalizationService _finalizationService;

    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _actorUserId = Guid.NewGuid();

    public BillingFinalizationReadinessCodesTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        _finalizationService = new BillingFinalizationService(
            _dbContext,
            new ContractBillingChargeSourceAdapter(),
            new BillingArApHandoffService(_dbContext, loggerService),
            new BillingInvoiceClosureService(_dbContext),
            new BilConsumerHandoffService(_dbContext, loggerService),
            loggerService);

        _dbContext.MstPatients.Add(new MstPatient
        {
            Id = _patientId,
            MedicalRecordNumber = "MRN-READY-001",
            FullName = "Pasien Kesiapan Finalisasi",
            BirthDate = DateTime.UtcNow.AddYears(-35),
            IsActive = true,
            IsDelete = false
        });
        _dbContext.SaveChanges();
    }

    private BilInvoice SeedInvoice(decimal primaryAmount, decimal patientAmount, int calculationVersion = 1)
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
            InvoiceNumber = "INV-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            EncounterId = encounter.Id,
            PatientId = _patientId,
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = calculationVersion,
            InvoiceDate = DateTimeOffset.UtcNow,
            RowVersion = Guid.NewGuid(),
            IsActive = true,
            IsDelete = false
        };
        _dbContext.BilInvoices.Add(invoice);

        if (calculationVersion > 0)
        {
            _dbContext.BilCalculationVersions.Add(new BilCalculationVersion
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                VersionNo = calculationVersion,
                GrossAmount = 710400m,
                TaxAmount = 78144m,
                PrimaryAmount = primaryAmount,
                PatientAmount = patientAmount,
                ExcessAmount = 0m,
                IsLocked = false,
                CalculatedAt = DateTimeOffset.UtcNow,
                Reason = "Kalkulasi pengujian kesiapan finalisasi.",
                BreakdownSnapshot = "{}"
            });
        }

        _dbContext.SaveChanges();
        return invoice;
    }

    private void AddItem(Guid invoiceId, DateTime createdAt)
    {
        _dbContext.BilInvoiceItems.Add(new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            SourceDomain = "PROCEDURE",
            SourceStatus = "COMPLETED",
            Status = BillingInvoiceItemStatuses.Active,
            Amount = 5000m,
            IsActive = true,
            IsDelete = false,
            CreateDateTime = createdAt
        });
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Preview_Item_Ditambah_Setelah_Kalkulasi_Menandai_Stale_Dengan_Kode_Stabil()
    {
        var invoice = SeedInvoice(primaryAmount: 788544m, patientAmount: 0m);
        AddItem(invoice.Id, DateTime.UtcNow.AddMinutes(5));

        var preview = await _finalizationService.PreviewAsync(invoice.Id, CancellationToken.None);

        Assert.False(preview.CalculationCurrent);
        Assert.False(preview.IsReadyForNormalFinalization);
        Assert.Contains(BillingFinalizationBlockingCodes.CalculationStale, preview.BlockingCodes);
        Assert.Equal("BIL-FIN-023", BillingFinalizationBlockingCodes.CalculationStale);
        Assert.Contains("hitung ulang", string.Join(' ', preview.BlockingReasons), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Preview_Setiap_BlockingReason_Memiliki_Kode_Pasangan()
    {
        // Stale + sisa pasien: dua alasan, dua kode (sebelumnya BlockingCodes kosong untuk keduanya).
        var invoice = SeedInvoice(primaryAmount: 0m, patientAmount: 50000m);
        AddItem(invoice.Id, DateTime.UtcNow.AddMinutes(5));

        var preview = await _finalizationService.PreviewAsync(invoice.Id, CancellationToken.None);

        Assert.Equal(preview.BlockingReasons.Count, preview.BlockingCodes.Count);
        Assert.Contains(BillingFinalizationBlockingCodes.CalculationStale, preview.BlockingCodes);
        Assert.Contains(BillingFinalizationBlockingCodes.PatientOutstanding, preview.BlockingCodes);
    }

    [Fact]
    public async Task Preview_Kalkulasi_Terkini_Tanpa_Sisa_Siap_Tanpa_Kode()
    {
        var invoice = SeedInvoice(primaryAmount: 788544m, patientAmount: 0m);
        AddItem(invoice.Id, DateTime.UtcNow.AddMinutes(-5));

        var preview = await _finalizationService.PreviewAsync(invoice.Id, CancellationToken.None);

        Assert.True(preview.CalculationCurrent);
        Assert.True(preview.IsReadyForNormalFinalization);
        Assert.Empty(preview.BlockingCodes);
        Assert.Empty(preview.BlockingReasons);
    }

    [Fact]
    public async Task Preview_Invoice_Tanpa_Kalkulasi_Menandai_CalculationMissing()
    {
        var invoice = SeedInvoice(primaryAmount: 0m, patientAmount: 0m, calculationVersion: 0);

        var preview = await _finalizationService.PreviewAsync(invoice.Id, CancellationToken.None);

        Assert.False(preview.IsReadyForNormalFinalization);
        Assert.Single(preview.BlockingCodes);
        Assert.Equal(BillingFinalizationBlockingCodes.CalculationMissing, preview.BlockingCodes[0]);
    }

    [Fact]
    public async Task Finalize_Kalkulasi_Stale_Ditolak_Dengan_Pesan_Spesifik_Dan_Tanpa_Efek()
    {
        var invoice = SeedInvoice(primaryAmount: 788544m, patientAmount: 0m);
        AddItem(invoice.Id, DateTime.UtcNow.AddMinutes(5));
        var initialRowVersion = invoice.RowVersion;

        var exception = await Assert.ThrowsAsync<BillingFinalizationBlockedException>(() =>
            _finalizationService.FinalizeAsync(
                invoice.Id,
                new FinalizeInvoiceRequest
                {
                    ExpectedRowVersion = initialRowVersion,
                    CorrelationId = Guid.NewGuid(),
                    CausationId = Guid.NewGuid(),
                    FinancingChoice = "EMPLOYEE_BENEFIT",
                    DirectPayAmount = 0m
                },
                Guid.NewGuid(),
                _actorUserId,
                CancellationToken.None));

        // Pesan menyebut penyebab (bukan lagi "Invoice belum siap difinalisasi."), checklist membawa kode stabilnya.
        Assert.Contains("hitung ulang", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(BillingFinalizationBlockingCodes.CalculationStale, exception.Checklist.BlockingCodes);
        Assert.False(exception.Checklist.CalculationCurrent);

        // Tidak ada hitung ulang diam-diam dan tidak ada efek finansial.
        var current = await _dbContext.BilInvoices.AsNoTracking().SingleAsync(x => x.Id == invoice.Id);
        Assert.Equal(BillingInvoiceStatuses.Open, current.Status);
        Assert.Equal(1, current.CurrentCalculationVersion);
        Assert.Equal(initialRowVersion, current.RowVersion);
        Assert.Empty(await _dbContext.BilFinalizationRecords.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
        Assert.Empty(await _dbContext.BilArHandoffs.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
    }

    [Fact]
    public async Task TryCompleteSettled_Sisa_Pasien_Tetap_Memakai_Pesan_Sisa_Bukan_Pesan_Kode_Turunan()
    {
        // Kode PatientOutstanding hanyalah penanda; jalur penutupan otomatis tetap melaporkan sisa tagihan pasien.
        var invoice = SeedInvoice(primaryAmount: 0m, patientAmount: 50000m);

        var (completed, reason) = await _finalizationService.TryCompleteSettledInvoiceAsync(
            invoice.Id,
            _actorUserId,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.False(completed);
        Assert.Contains("sisa tagihan pasien belum lunas", reason, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }
}
