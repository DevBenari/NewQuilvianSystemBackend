using Microsoft.AspNetCore.Http;
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
/// Status pembayaran invoice harus menghitung pemakaian deposit RANAP. Pemakaian deposit lahir sebagai
/// settlement SETTLED TANPA tender, sehingga total dibayar yang hanya menjumlah tender membuat invoice yang
/// baru dibayar deposit berstatus BELUM_LUNAS dan invoice deposit + tender tidak pernah berstatus LUNAS.
/// Skenario acuan: tagihan Rp22.571.734, deposit Rp20.000.000, sisa Rp2.571.734 dibayar QRIS Rp1.500.000
/// lalu Tunai Rp1.071.734.
/// CATATAN: pengujian ini disiapkan sebagai sumber verifikasi source-level dan belum dijalankan.
/// </summary>
public class BillingInvoiceDepositPaymentStatusTests : IDisposable
{
    private const decimal PatientAmount = 22_571_734m;
    private const decimal DepositAmount = 20_000_000m;
    private const decimal QrisAmount = 1_500_000m;
    private const decimal CashAmount = 1_071_734m;

    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingInvoiceService _service;
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _qrisMethodId = Guid.NewGuid();
    private readonly Guid _cashMethodId = Guid.NewGuid();
    private readonly DateTimeOffset _t0 = new(2026, 10, 22, 8, 0, 0, TimeSpan.Zero);

    public BillingInvoiceDepositPaymentStatusTests()
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

        _dbContext.MstPatients.Add(new MstPatient
        {
            Id = _patientId,
            MedicalRecordNumber = "MRN-DEP-001",
            FullName = "Pasien Uji Deposit",
            BirthDate = DateTime.UtcNow.AddYears(-40),
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

    private BilInvoice NewInvoice(decimal patientAmount)
    {
        var encounter = new RegPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = "ENC-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            PatientId = _patientId,
            EncounterStatus = EncounterStatus.InProgress,
            EncounterDate = DateTime.UtcNow,
            IsActive = true,
            IsDelete = false
        };
        _dbContext.RegPatientEncounters.Add(encounter);

        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-DEP-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 1,
            RowVersion = Guid.NewGuid(),
            VisitDate = DateTime.UtcNow.Date,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        _dbContext.BilCalculationVersions.Add(new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = patientAmount,
            PatientAmount = patientAmount,
            CalculatedAt = _t0,
            Reason = "Uji",
            BreakdownSnapshot = "{}",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        });
        _dbContext.SaveChanges();
        return invoice;
    }

    private BilSettlement NewSettlement(Guid invoiceId, decimal requested, decimal successful)
    {
        var settlement = new BilSettlement
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Purpose = BillingSettlementPurposes.InvoicePayment,
            RequestedAmount = requested,
            SuccessfulAmount = successful,
            AllocatedAmount = successful,
            Status = BillingSettlementStatuses.Settled,
            IdempotencyKey = Guid.NewGuid(),
            PayloadHash = "hash",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            StartedAt = _t0,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilSettlements.Add(settlement);
        _dbContext.SaveChanges();
        return settlement;
    }

    private BilTender NewTender(Guid settlementId, Guid methodId, decimal amount, DateTimeOffset attemptedAt, string kwitansi)
    {
        var tender = new BilTender
        {
            Id = Guid.NewGuid(),
            SettlementId = settlementId,
            PaymentMethodId = methodId,
            Amount = amount,
            Status = BillingTenderStatuses.Succeeded,
            KwitansiNumber = kwitansi,
            IdempotencyKey = Guid.NewGuid(),
            PayloadHash = "hash",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            AttemptedAt = attemptedAt,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilTenders.Add(tender);
        _dbContext.SaveChanges();
        return tender;
    }

    private BilPaymentAllocation NewAllocation(Guid settlementId, Guid invoiceId, decimal amount, Guid? reverses = null)
    {
        var allocation = new BilPaymentAllocation
        {
            Id = Guid.NewGuid(),
            SettlementId = settlementId,
            TargetType = BillingAllocationTargetTypes.Invoice,
            TargetId = invoiceId,
            Amount = amount,
            AllocatedAt = _t0,
            ReversesAllocationId = reverses,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilPaymentAllocations.Add(allocation);
        _dbContext.SaveChanges();
        return allocation;
    }

    /// <summary>Pemakaian deposit: settlement SETTLED tanpa tender plus alokasi ke invoice.</summary>
    private BilPaymentAllocation AddDepositUsage(BilInvoice invoice, decimal amount)
    {
        var settlement = NewSettlement(invoice.Id, amount, amount);
        return NewAllocation(settlement.Id, invoice.Id, amount);
    }

    /// <summary>Sisa dibayar lewat satu settlement berisi QRIS lalu Tunai, masing-masing teralokasi.</summary>
    private void AddRemainderTenders(BilInvoice invoice)
    {
        var settlement = NewSettlement(invoice.Id, QrisAmount + CashAmount, QrisAmount + CashAmount);
        NewTender(settlement.Id, _qrisMethodId, QrisAmount, _t0.AddHours(2), "KW-001");
        NewTender(settlement.Id, _cashMethodId, CashAmount, _t0.AddHours(3), "KW-002");
        NewAllocation(settlement.Id, invoice.Id, QrisAmount + CashAmount);
    }

    // ----------------------------------------------------------------------------------------- detail

    [Fact]
    public async Task Detail_Invoice_Yang_Baru_Dibayar_Sebagian_Oleh_Deposit_Berstatus_SEBAGIAN()
    {
        var invoice = NewInvoice(PatientAmount);
        AddDepositUsage(invoice, DepositAmount);

        var detail = await _service.GetDetailAsync(invoice.Id, CancellationToken.None);

        Assert.Equal(DepositAmount, detail.TotalPaidAmount);
        Assert.Equal(PatientAmount - DepositAmount, detail.OutstandingAmount);
        Assert.Equal("SEBAGIAN", detail.PaymentStatus);
        Assert.False(detail.IsFullyPaid);
    }

    [Fact]
    public async Task Detail_Invoice_Lunas_Setelah_Sisa_Dibayar_Tender()
    {
        var invoice = NewInvoice(PatientAmount);
        AddDepositUsage(invoice, DepositAmount);
        AddRemainderTenders(invoice);

        var detail = await _service.GetDetailAsync(invoice.Id, CancellationToken.None);

        Assert.Equal(PatientAmount, detail.TotalPaidAmount);
        Assert.Equal(0m, detail.OutstandingAmount);
        Assert.Equal("LUNAS", detail.PaymentStatus);
        Assert.True(detail.IsFullyPaid);
    }

    [Fact]
    public async Task Detail_Alokasi_Deposit_Yang_Dipulihkan_Tidak_Lagi_Dihitung_Dibayar()
    {
        var invoice = NewInvoice(PatientAmount);
        var usage = AddDepositUsage(invoice, DepositAmount);
        NewAllocation(usage.SettlementId, invoice.Id, DepositAmount, reverses: usage.Id);

        var detail = await _service.GetDetailAsync(invoice.Id, CancellationToken.None);

        Assert.Equal(0m, detail.TotalPaidAmount);
        Assert.Equal("BELUM_LUNAS", detail.PaymentStatus);
    }

    [Fact]
    public async Task Detail_Invoice_Yang_Hanya_Dibayar_Tender_Tidak_Berubah()
    {
        var invoice = NewInvoice(100_000m);
        var settlement = NewSettlement(invoice.Id, 40_000m, 40_000m);
        NewTender(settlement.Id, _cashMethodId, 40_000m, _t0, "KW-T1");
        NewAllocation(settlement.Id, invoice.Id, 40_000m);

        var detail = await _service.GetDetailAsync(invoice.Id, CancellationToken.None);

        Assert.Equal(40_000m, detail.TotalPaidAmount);
        Assert.Equal(60_000m, detail.OutstandingAmount);
        Assert.Equal("SEBAGIAN", detail.PaymentStatus);
    }

    // ---------------------------------------------------------------------------- riwayat lintas pasien

    private async Task<PaymentHistoryItemResponse> HistoryItem(BilInvoice invoice)
    {
        var page = await _service.GetPaymentHistoryAsync(new PaymentHistoryQuery(), CancellationToken.None);
        return Assert.Single(page.Items, x => x.Id == invoice.Id);
    }

    [Fact]
    public async Task Riwayat_Invoice_Deposit_Plus_Tender_Berstatus_LUNAS_Dengan_Sisa_Nol()
    {
        var invoice = NewInvoice(PatientAmount);
        AddDepositUsage(invoice, DepositAmount);
        AddRemainderTenders(invoice);

        var item = await HistoryItem(invoice);

        Assert.Equal(PatientAmount, item.TotalPaidAmount);
        Assert.Equal(0m, item.OutstandingAmount);
        Assert.True(item.IsFullyPaid);
        Assert.Equal("LUNAS", item.PaymentStatus);
    }

    [Fact]
    public async Task Riwayat_Klasifikasi_Tender_Memperhitungkan_Deposit_Yang_Sudah_Masuk()
    {
        var invoice = NewInvoice(PatientAmount);
        AddDepositUsage(invoice, DepositAmount);
        AddRemainderTenders(invoice);

        var item = await HistoryItem(invoice);

        Assert.Equal(2, item.Tenders.Count);
        Assert.Equal("KW-001", item.Tenders[0].KwitansiNumber);
        Assert.Equal("CICILAN", item.Tenders[0].TransactionClassification);
        Assert.Equal("SEBAGIAN", item.Tenders[0].PostTransactionStatus);
        Assert.Equal(CashAmount, item.Tenders[0].RemainingOutstanding);

        Assert.Equal("KW-002", item.Tenders[1].KwitansiNumber);
        Assert.Equal("PELUNASAN", item.Tenders[1].TransactionClassification);
        Assert.Equal("LUNAS", item.Tenders[1].PostTransactionStatus);
        Assert.Equal(0m, item.Tenders[1].RemainingOutstanding);
    }

    [Fact]
    public async Task Riwayat_Deposit_Plus_Tender_Sebagian_Berstatus_SEBAGIAN_Dengan_Sisa_Yang_Benar()
    {
        var invoice = NewInvoice(PatientAmount);
        AddDepositUsage(invoice, DepositAmount);
        var settlement = NewSettlement(invoice.Id, QrisAmount + CashAmount, QrisAmount);
        NewTender(settlement.Id, _qrisMethodId, QrisAmount, _t0.AddHours(2), "KW-001");
        NewAllocation(settlement.Id, invoice.Id, QrisAmount);

        var item = await HistoryItem(invoice);

        Assert.Equal(DepositAmount + QrisAmount, item.TotalPaidAmount);
        Assert.Equal(CashAmount, item.OutstandingAmount);
        Assert.False(item.IsFullyPaid);
        Assert.Equal("SEBAGIAN", item.PaymentStatus);
    }

    [Fact]
    public async Task Riwayat_Invoice_Tanpa_Deposit_Tetap_Sama_Seperti_Sebelumnya()
    {
        var invoice = NewInvoice(100_000m);
        var settlement = NewSettlement(invoice.Id, 100_000m, 100_000m);
        NewTender(settlement.Id, _cashMethodId, 100_000m, _t0, "KW-T1");
        NewAllocation(settlement.Id, invoice.Id, 100_000m);

        var item = await HistoryItem(invoice);

        Assert.Equal(100_000m, item.TotalPaidAmount);
        Assert.Equal(0m, item.OutstandingAmount);
        Assert.Equal("LUNAS", item.PaymentStatus);
        var tender = Assert.Single(item.Tenders);
        Assert.Equal("PELUNASAN", tender.TransactionClassification);
    }
}
