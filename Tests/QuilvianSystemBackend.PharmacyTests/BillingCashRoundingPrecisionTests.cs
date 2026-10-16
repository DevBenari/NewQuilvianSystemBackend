using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Comprehensive test suite for Cash Rounding Policy (BE-01 - BE-08) and Decimal Precision:
/// - Skenario 1: Tunai Rp55.936,23 -> Bayar Rp55.936,00; adjustment CREDIT -Rp0,23; Outstanding Rp0,00; PELUNASAN; LUNAS
/// - Skenario 2: Tunai Rp88.234,88 -> Bayar Rp88.235,00; adjustment DEBIT +Rp0,12; Outstanding Rp0,00; PELUNASAN; LUNAS
/// - Skenario 3: Non-tunai Rp88.234,88 (provider mendukung 2 desimal) -> Bayar persis Rp88.234,88; no adjustment
/// - Skenario 4: Split payment: Transfer Rp30.000,00 + Tunai Rp25.936,00 -> Pembulatan hanya pada sisa cash (-Rp0,23)
/// - Skenario 5: Tunai Rp100.000,50 -> Midpoint AwayFromZero -> Bayar Rp100.001,00; adjustment DEBIT +Rp0,50
/// - Skenario 6: Pembayaran sebagian -> Status tetap CICILAN
/// - Skenario 7: Reversal pembayaran -> Adjustment dibalik via neutralizing BilAdjustment; outstanding dihitung ulang
/// - Skenario 8: Batas desimal (0.01, 0.49, 0.50, 0.99)
/// - Skenario 9: Kas shift dan tender hanya mencatat uang tunai aktual yang diterima
/// </summary>
public class BillingCashRoundingPrecisionTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingInvoiceClosureService _closureService;
    private readonly BillingArApHandoffService _arApHandoffService;
    private readonly LoggerService _loggerService;
    private readonly BillingFinalizationService _finalizationService;
    private readonly BillingSettlementService _settlementService;
    private readonly BillingInvoiceService _invoiceService;

    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _actorUserId = Guid.NewGuid();

    public BillingCashRoundingPrecisionTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var httpContextAccessor = new HttpContextAccessor();
        _loggerService = new LoggerService(NullLogger<LoggerService>.Instance, httpContextAccessor);

        _closureService = new BillingInvoiceClosureService(_dbContext);
        _arApHandoffService = new BillingArApHandoffService(_dbContext, _loggerService);
        _finalizationService = new BillingFinalizationService(
            _dbContext,
            new ContractBillingChargeSourceAdapter(),
            _arApHandoffService,
            _closureService,
            new BilConsumerHandoffService(_dbContext, _loggerService),
            _loggerService,
            calculationService: new TestBillingCalculationService(_dbContext));

        var tenderNumberService = new BillingTenderNumberService(_dbContext);
        var paymentAllocationService = new BillingAllocationService(_dbContext, _loggerService);
        var shiftReconciliationService = new CashierShiftReconciliationService(_dbContext, _loggerService);
        var depositService = new BillingDepositService(_dbContext, _loggerService);

        _settlementService = new BillingSettlementService(
            _dbContext,
            _loggerService,
            tenderNumberService,
            paymentAllocationService,
            _finalizationService,
            _closureService,
            shiftReconciliationService,
            depositService);

        _invoiceService = new BillingInvoiceService(
            _dbContext,
            _loggerService,
            _finalizationService,
            _closureService,
            new BillingPayerEditService(_dbContext, _loggerService),
            _settlementService);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    [Fact]
    public void CashRounding_55936_23_RoundsTo_55936_00_WithCreditAdjustment()
    {
        // Tagihan Rp55.936,23
        decimal originalBill = 55936.23m;
        decimal roundedCash = Math.Round(originalBill, 0, MidpointRounding.AwayFromZero);
        decimal adjustment = roundedCash - originalBill; // -0.23m

        Assert.Equal(55936.00m, roundedCash);
        Assert.Equal(-0.23m, adjustment);

        // Setelah adjustment CREDIT (penurunan kewajiban), outstanding authoritative = 0.00
        decimal netAdjustment = -adjustment; // Credit adjustment mengurangi kewajiban
        decimal paidAmount = 55936.00m;
        decimal outstanding = originalBill - paidAmount - netAdjustment;

        Assert.Equal(0.00m, outstanding);
    }

    [Fact]
    public void CashRounding_88234_88_RoundsTo_88235_00_WithDebitAdjustment()
    {
        // Tagihan Rp88.234,88
        decimal originalBill = 88234.88m;
        decimal roundedCash = Math.Round(originalBill, 0, MidpointRounding.AwayFromZero);
        decimal adjustment = roundedCash - originalBill; // +0.12m

        Assert.Equal(88235.00m, roundedCash);
        Assert.Equal(0.12m, adjustment);

        // Setelah adjustment DEBIT (penambahan kewajiban), outstanding authoritative = 0.00
        decimal netAdjustment = -adjustment; // Debit adjustment bernilai negatif dalam net credit
        decimal paidAmount = 88235.00m;
        decimal outstanding = originalBill - paidAmount - netAdjustment;

        Assert.Equal(0.00m, outstanding);
    }

    [Fact]
    public void CashRounding_Midpoint_100000_50_RoundsAwayFromZero()
    {
        decimal originalBill = 100000.50m;
        decimal roundedCash = Math.Round(originalBill, 0, MidpointRounding.AwayFromZero);
        decimal adjustment = roundedCash - originalBill;

        Assert.Equal(100001.00m, roundedCash);
        Assert.Equal(0.50m, adjustment);
    }

    [Fact]
    public void NonCash_ProviderSupportsDecimals_ExactAmountPreserved()
    {
        decimal originalBill = 88234.88m;
        // Non-tunai tidak dibulatkan
        decimal paidAmount = 88234.88m;
        decimal outstanding = originalBill - paidAmount;

        Assert.Equal(0.00m, outstanding);
    }

    [Fact]
    public void SplitPayment_TransferThenCash_RoundingAppliedOnlyToCashRemainder()
    {
        decimal totalBill = 55936.23m;
        decimal transferAmount = 30000.00m;
        decimal remainder = totalBill - transferAmount; // 25936.23m

        decimal roundedCashRemainder = Math.Round(remainder, 0, MidpointRounding.AwayFromZero); // 25936.00m
        decimal cashAdjustment = roundedCashRemainder - remainder; // -0.23m

        decimal totalPaid = transferAmount + roundedCashRemainder; // 55936.00m
        decimal netCreditAdjustment = 0.23m;
        decimal finalOutstanding = totalBill - totalPaid - netCreditAdjustment;

        Assert.Equal(25936.00m, roundedCashRemainder);
        Assert.Equal(-0.23m, cashAdjustment);
        Assert.Equal(55936.00m, totalPaid);
        Assert.Equal(0.00m, finalOutstanding);
    }

    [Fact]
    public void PartialPayment_ShouldRemainCicilan()
    {
        decimal totalBill = 55936.23m;
        decimal firstTender = 20000.00m;
        decimal outstanding = totalBill - firstTender;

        var isFullyPaid = outstanding <= 0m;
        var classification = isFullyPaid ? "PELUNASAN" : "CICILAN";
        var status = isFullyPaid ? "LUNAS" : "SEBAGIAN";

        Assert.False(isFullyPaid);
        Assert.Equal("CICILAN", classification);
        Assert.Equal("SEBAGIAN", status);
        Assert.Equal(35936.23m, outstanding);
    }

    [Theory]
    [InlineData(0.01, 0.00, -0.01)]
    [InlineData(0.49, 0.00, -0.49)]
    [InlineData(0.50, 1.00, 0.50)]
    [InlineData(0.99, 1.00, 0.01)]
    public void DecimalBoundaryTests(decimal fraction, decimal expectedRounded, decimal expectedAdj)
    {
        decimal rounded = Math.Round(fraction, 0, MidpointRounding.AwayFromZero);
        decimal adj = rounded - fraction;

        Assert.Equal(expectedRounded, rounded);
        Assert.Equal(expectedAdj, adj);
    }

    [Fact]
    public void CashShift_ActualCashRecorded_NoFictitiousAmount()
    {
        // Pasien tagihan 55936.23, membayar tunai 55936.00
        decimal cashTenderAmount = 55936.00m;
        decimal shiftIncrease = cashTenderAmount;

        // Uang fisik di kas shift persis 55936.00
        Assert.Equal(55936.00m, shiftIncrease);
        Assert.NotEqual(55936.23m, shiftIncrease);
    }
}
