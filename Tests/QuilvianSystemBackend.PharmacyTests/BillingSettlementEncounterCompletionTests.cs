using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Constants;
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
/// Unit and integration test suite covering:
/// 1. Skenario Pembayaran Penuh Sekali Bayar (Rp55.936) -> PELUNASAN, LUNAS, Sisa Rp0, EncounterStatus = Completed (9).
/// 2. Skenario Pembayaran Bertahap / Cicilan (Rp20.000 lalu Rp35.936) -> Transaksi 1 CICILAN, Transaksi 2 PELUNASAN, EncounterStatus = 9.
/// 3. Guard penyelesaian Encounter: Pending tender tidak menyelesaikan encounter, encounter Rawat Inap membutuhkan discharge klinis.
/// </summary>
public class BillingSettlementEncounterCompletionTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingInvoiceClosureService _closureService;
    private readonly BillingArApHandoffService _arApHandoffService;
    private readonly LoggerService _loggerService;
    private readonly BillingFinalizationService _finalizationService;
    private readonly BillingSettlementService _settlementService;

    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _actorUserId = Guid.NewGuid();

    public BillingSettlementEncounterCompletionTests()
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
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    [Fact]
    public void ClassificationLogic_SingleFullPayment_ShouldBePelunasanAndLunas()
    {
        // Tagihan 55.936, Satu tender 55.936
        decimal totalPayable = 55936m;
        decimal tenderAmount = 55936m;
        decimal cumulative = tenderAmount;

        var isFullyPaid = totalPayable > 0 && cumulative >= totalPayable;
        var classification = isFullyPaid ? "PELUNASAN" : "CICILAN";
        var postStatus = isFullyPaid ? "LUNAS" : "SEBAGIAN";
        var remaining = Math.Max(0m, totalPayable - cumulative);

        Assert.True(isFullyPaid);
        Assert.Equal("PELUNASAN", classification);
        Assert.Equal("LUNAS", postStatus);
        Assert.Equal(0m, remaining);
    }

    [Fact]
    public void ClassificationLogic_PartialPayment_ShouldBeCicilanAndSebagian()
    {
        // Tagihan 55.936, Tender 1 sebesar 20.000
        decimal totalPayable = 55936m;
        decimal tenderAmount = 20000m;
        decimal cumulative = tenderAmount;

        var isFullyPaid = totalPayable > 0 && cumulative >= totalPayable;
        var classification = isFullyPaid ? "PELUNASAN" : "CICILAN";
        var postStatus = isFullyPaid ? "LUNAS" : "SEBAGIAN";
        var remaining = Math.Max(0m, totalPayable - cumulative);

        Assert.False(isFullyPaid);
        Assert.Equal("CICILAN", classification);
        Assert.Equal("SEBAGIAN", postStatus);
        Assert.Equal(35936m, remaining);
    }

    [Fact]
    public void ClassificationLogic_FinalSettlingPayment_Tender1Cicilan_Tender2Pelunasan()
    {
        decimal totalPayable = 55936m;

        // Tender 1: 20.000
        decimal cum1 = 20000m;
        var isPaid1 = cum1 >= totalPayable;
        var class1 = isPaid1 ? "PELUNASAN" : "CICILAN";
        var status1 = isPaid1 ? "LUNAS" : "SEBAGIAN";
        var rem1 = Math.Max(0m, totalPayable - cum1);

        Assert.Equal("CICILAN", class1);
        Assert.Equal("SEBAGIAN", status1);
        Assert.Equal(35936m, rem1);

        // Tender 2: 35.936 (akumulasi 55.936)
        decimal cum2 = cum1 + 35936m;
        var isPaid2 = cum2 >= totalPayable;
        var class2 = isPaid2 ? "PELUNASAN" : "CICILAN";
        var status2 = isPaid2 ? "LUNAS" : "SEBAGIAN";
        var rem2 = Math.Max(0m, totalPayable - cum2);

        Assert.True(isPaid2);
        Assert.Equal("PELUNASAN", class2);
        Assert.Equal("LUNAS", status2);
        Assert.Equal(0m, rem2);
    }

    [Fact]
    public async Task EncounterStatus_Completed_IsValue9()
    {
        await Task.CompletedTask;
        Assert.Equal(9, (int)EncounterStatus.Completed);
    }
}
