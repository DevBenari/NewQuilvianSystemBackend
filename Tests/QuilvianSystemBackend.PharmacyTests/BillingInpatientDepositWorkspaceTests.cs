using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Pengujian unit dan acceptance kriteria BE-01 s/d BE-08 untuk Integrated Inpatient Deposit Workspace.
/// Mencakup 17 skenario verifikasi sesuai kebutuhan spesifikasi.
/// CATATAN: Pengujian ini disiapkan sebagai sumber verifikasi source-level dan tidak dieksekusi secara otomatis.
/// </summary>
public class BillingInpatientDepositWorkspaceTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingDepositService _depositService;
    private readonly BillingPatientFundsController _patientFundsController;
    private readonly PatientEncounterController _encounterController;
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _transferPaymentMethodId = Guid.NewGuid();
    private readonly Guid _cashPaymentMethodId = Guid.NewGuid();
    private readonly Guid _insurancePaymentMethodId = Guid.NewGuid();

    public BillingInpatientDepositWorkspaceTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _actorUserId.ToString()),
            new Claim("user_id", _actorUserId.ToString())
        };
        httpContextAccessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, httpContextAccessor);
        var numberSeries = new BillingNumberSeriesService(_dbContext);
        var cashierShiftService = new CashierShiftService(_dbContext, numberSeries, loggerService);
        var allocationService = new BillingAllocationService(_dbContext);

        _depositService = new BillingDepositService(_dbContext, numberSeries, cashierShiftService, loggerService);
        _patientFundsController = new BillingPatientFundsController(_depositService, allocationService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextAccessor.HttpContext
            }
        };

        _encounterController = new PatientEncounterController(_dbContext, httpContextAccessor)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextAccessor.HttpContext
            }
        };

        SeedCommonData();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    private void SeedCommonData()
    {
        var patient = new Patient
        {
            Id = _patientId,
            FullName = "Pasien Uji Rawat Inap",
            MedicalRecordNumber = "RM-INP-001",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = "M",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.Patients.Add(patient);

        var transferMethod = new MstPaymentMethod
        {
            Id = _transferPaymentMethodId,
            PaymentMethodCode = "PM-TRF",
            PaymentMethodName = "Bank Transfer",
            PaymentMethodType = "Transfer",
            PaymentGroupName = "Non-Cash",
            IsActive = true,
            IsAvailableForBilling = true,
            IsCash = false,
            IsInsurance = false,
            IsCompanyGuarantor = false,
            IsMembership = false,
            IsNeedReferenceNumber = false,
            IsNeedApproval = false,
            IsNeedAttachment = false,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };

        var cashMethod = new MstPaymentMethod
        {
            Id = _cashPaymentMethodId,
            PaymentMethodCode = "PM-CASH",
            PaymentMethodName = "Tunai Kasir",
            PaymentMethodType = "Cash",
            PaymentGroupName = "Cash",
            IsActive = true,
            IsAvailableForBilling = true,
            IsCash = true,
            IsInsurance = false,
            IsCompanyGuarantor = false,
            IsMembership = false,
            IsNeedReferenceNumber = false,
            IsNeedApproval = false,
            IsNeedAttachment = false,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };

        var insuranceMethod = new MstPaymentMethod
        {
            Id = _insurancePaymentMethodId,
            PaymentMethodCode = "PM-INS",
            PaymentMethodName = "BPJS Kesehatan",
            PaymentMethodType = "Insurance",
            PaymentGroupName = "Payer",
            IsActive = true,
            IsAvailableForBilling = true,
            IsCash = false,
            IsInsurance = true,
            IsCompanyGuarantor = false,
            IsMembership = false,
            IsNeedReferenceNumber = false,
            IsNeedApproval = false,
            IsNeedAttachment = false,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };

        _dbContext.MstPaymentMethods.AddRange(transferMethod, cashMethod, insuranceMethod);
        _dbContext.SaveChanges();
    }

    private RegPatientEncounter CreateEncounter(
        EncounterType type = EncounterType.Inpatient,
        EncounterStatus status = EncounterStatus.InProgress,
        string encounterNumber = "ENC-TEST-001")
    {
        var encounter = new RegPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = encounterNumber,
            PatientId = _patientId,
            EncounterType = type,
            EncounterStatus = status,
            EncounterDate = DateTime.UtcNow.Date,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.RegPatientEncounters.Add(encounter);
        _dbContext.SaveChanges();
        return encounter;
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 1: Filter kunjungan mengembalikan hanya Inpatient
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario01_FilterEncounterOptions_ReturnsOnlyInpatient()
    {
        CreateEncounter(EncounterType.Inpatient, EncounterStatus.InProgress, "ENC-INP-01");
        CreateEncounter(EncounterType.Outpatient, EncounterStatus.InProgress, "ENC-OUT-01");
        CreateEncounter(EncounterType.Emergency, EncounterStatus.InProgress, "ENC-EMG-01");

        var result = await _encounterController.GetEncounterOptionsForAdmin(
            encounterType: EncounterType.Inpatient,
            pageNumber: 1,
            pageSize: 25) as OkObjectResult;

        Assert.NotNull(result);
        var apiResponse = result.Value as ApiResponse<PatientEncounterOptionPagedResponse>;
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Data);
        Assert.All(apiResponse.Data.Items, item => Assert.Equal(EncounterType.Inpatient, item.EncounterType));
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 2: Pagination tidak memasukkan Rawat Jalan, IGD, MCU, atau jenis lainnya
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario02_Pagination_DoesNotIncludeNonInpatientInTotalData()
    {
        CreateEncounter(EncounterType.Inpatient, EncounterStatus.InProgress, "ENC-INP-PAG-01");
        CreateEncounter(EncounterType.Inpatient, EncounterStatus.InProgress, "ENC-INP-PAG-02");
        CreateEncounter(EncounterType.Outpatient, EncounterStatus.InProgress, "ENC-OUT-PAG-01");
        CreateEncounter(EncounterType.Emergency, EncounterStatus.InProgress, "ENC-EMG-PAG-01");
        CreateEncounter(EncounterType.MedicalCheckup, EncounterStatus.InProgress, "ENC-MCU-PAG-01");

        var result = await _encounterController.GetEncounterOptionsForAdmin(
            encounterType: EncounterType.Inpatient,
            pageNumber: 1,
            pageSize: 10) as OkObjectResult;

        Assert.NotNull(result);
        var apiResponse = result.Value as ApiResponse<PatientEncounterOptionPagedResponse>;
        Assert.NotNull(apiResponse);
        Assert.Equal(2, apiResponse.Data!.TotalData);
        Assert.Equal(2, apiResponse.Data.Items.Count);
        Assert.DoesNotContain(apiResponse.Data.Items, x => x.EncounterType != EncounterType.Inpatient);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 3: Kunjungan Rawat Inap tanpa deposit menampilkan saldo 0
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario03_InpatientEncounterWithoutDeposit_ReturnsEmptyStateZeroBalance()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient, EncounterStatus.InProgress, "ENC-NO-DEP-01");

        var response = await _depositService.GetDepositWorkspaceAsync(
            encounter.Id,
            pageNumber: 1,
            pageSize: 10,
            movementType: null,
            startDate: null,
            endDate: null,
            search: null,
            cancellationToken: CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(response.HasDepositAccount);
        Assert.Null(response.DepositAccountId);
        Assert.Equal(0m, response.AvailableBalance);
        Assert.Equal(0m, response.TotalTopUpGross);
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalData);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 4: Top-up pertama membuat satu account dan satu movement
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario04_FirstTopUp_CreatesOneAccountAndOneMovement()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);
        var request = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 5_000_000m,
            Reason = "Deposit awal rawat inap",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            ExpectedRowVersion = null
        };
        var idempotencyKey = Guid.NewGuid();

        var result = await _depositService.TopUpAsync(
            encounter.Id, request, idempotencyKey, _actorUserId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(5_000_000m, result.SuccessfulAmount);

        var account = await _dbContext.BilDepositAccounts
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.EncounterId == encounter.Id);

        Assert.NotNull(account);
        Assert.Equal(5_000_000m, account.AvailableBalance);
        Assert.Single(account.Movements);
        Assert.Equal(BillingDepositMovementTypes.TopUp, account.Movements.First().MovementType);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 5: Top-up berikutnya menambahkan movement pada account yang sama
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario05_SubsequentTopUp_AppendsToSameAccount()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);
        var req1 = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 3_000_000m,
            Reason = "Deposit ke-1",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            ExpectedRowVersion = null
        };
        await _depositService.TopUpAsync(encounter.Id, req1, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        var accountAfter1 = await _dbContext.BilDepositAccounts.SingleAsync(x => x.EncounterId == encounter.Id);

        var req2 = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 2_000_000m,
            Reason = "Deposit ke-2",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            ExpectedRowVersion = accountAfter1.RowVersion
        };
        await _depositService.TopUpAsync(encounter.Id, req2, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        var accountAfter2 = await _dbContext.BilDepositAccounts
            .Include(x => x.Movements)
            .SingleAsync(x => x.EncounterId == encounter.Id);

        Assert.Equal(accountAfter1.Id, accountAfter2.Id);
        Assert.Equal(5_000_000m, accountAfter2.AvailableBalance);
        Assert.Equal(2, accountAfter2.Movements.Count);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 6: Satu pasien dengan dua kunjungan Rawat Inap memiliki saldo deposit terpisah
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario06_SamePatientTwoEncounters_HaveSeparateDepositAccounts()
    {
        var enc1 = CreateEncounter(EncounterType.Inpatient, EncounterStatus.InProgress, "ENC-PAT-01");
        var enc2 = CreateEncounter(EncounterType.Inpatient, EncounterStatus.InProgress, "ENC-PAT-02");

        var req1 = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 4_000_000m,
            Reason = "Topup Kunjungan 1",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };
        await _depositService.TopUpAsync(enc1.Id, req1, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        var req2 = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 7_000_000m,
            Reason = "Topup Kunjungan 2",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };
        await _depositService.TopUpAsync(enc2.Id, req2, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        var acc1 = await _dbContext.BilDepositAccounts.SingleAsync(x => x.EncounterId == enc1.Id);
        var acc2 = await _dbContext.BilDepositAccounts.SingleAsync(x => x.EncounterId == enc2.Id);

        Assert.NotEqual(acc1.Id, acc2.Id);
        Assert.Equal(4_000_000m, acc1.AvailableBalance);
        Assert.Equal(7_000_000m, acc2.AvailableBalance);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 7: Nominal nol atau negatif ditolak
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario07_ZeroOrNegativeAmount_ThrowsValidationException()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);

        var zeroReq = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 0m,
            Reason = "Uji nol",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            _depositService.TopUpAsync(encounter.Id, zeroReq, Guid.NewGuid(), _actorUserId, CancellationToken.None));

        var negativeReq = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = -100_000m,
            Reason = "Uji negatif",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            _depositService.TopUpAsync(encounter.Id, negativeReq, Guid.NewGuid(), _actorUserId, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 8: Deposit untuk kunjungan non-Inpatient ditolak dari backend
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario08_NonInpatientDeposit_RejectedWithValidationException()
    {
        var outpatientEncounter = CreateEncounter(EncounterType.Outpatient, EncounterStatus.InProgress, "ENC-RJ-001");
        var req = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 1_000_000m,
            Reason = "Uji non inpatient",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            _depositService.TopUpAsync(outpatientEncounter.Id, req, Guid.NewGuid(), _actorUserId, CancellationToken.None));

        Assert.Contains("rawat inap", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 9: Request top-up berulang tidak menggandakan saldo (idempotency)
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario09_ReplayedTopUp_DoesNotDuplicateBalance()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);
        var idempotencyKey = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();

        var req = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 2_500_000m,
            Reason = "Topup idempotent",
            CorrelationId = correlationId,
            CausationId = causationId
        };

        var firstCall = await _depositService.TopUpAsync(
            encounter.Id, req, idempotencyKey, _actorUserId, CancellationToken.None);
        Assert.False(firstCall.IsReplay);

        // Panggil ulang dengan key dan payload yang sama persis
        var secondCall = await _depositService.TopUpAsync(
            encounter.Id, req, idempotencyKey, _actorUserId, CancellationToken.None);
        Assert.True(secondCall.IsReplay);

        var account = await _dbContext.BilDepositAccounts
            .Include(x => x.Movements)
            .SingleAsync(x => x.EncounterId == encounter.Id);

        Assert.Equal(2_500_000m, account.AvailableBalance);
        Assert.Single(account.Movements);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 10: Dua transaksi bersamaan tidak menyebabkan lost update (RowVersion)
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario10_RowVersionMismatch_ThrowsConflictException()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);
        var req1 = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 1_000_000m,
            Reason = "Topup 1",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };
        await _depositService.TopUpAsync(encounter.Id, req1, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        var staleRowVersion = Guid.NewGuid(); // Versi tidak cocok

        var req2 = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 1_000_000m,
            Reason = "Topup 2 dengan versi stale",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            ExpectedRowVersion = staleRowVersion
        };

        await Assert.ThrowsAsync<BillingDepositConflictException>(() =>
            _depositService.TopUpAsync(encounter.Id, req2, Guid.NewGuid(), _actorUserId, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 11: Metode pembayaran terlarang ditolak
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario11_ForbiddenPaymentMethod_RejectedWithValidationException()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);
        var req = new DepositTopUpRequest
        {
            PaymentMethodId = _insurancePaymentMethodId, // Asuransi / Payer tidak boleh jadi deposit
            Amount = 1_000_000m,
            Reason = "Uji metode asuransi",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            _depositService.TopUpAsync(encounter.Id, req, Guid.NewGuid(), _actorUserId, CancellationToken.None));

        Assert.Contains("penjamin", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 12: Top-up tunai tanpa shift aktif ditolak
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario12_CashTopUpWithoutActiveShift_Rejected()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);
        var req = new DepositTopUpRequest
        {
            PaymentMethodId = _cashPaymentMethodId,
            Amount = 1_000_000m,
            Reason = "Uji tunai tanpa shift",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        // User tidak memiliki shift aktif di database
        await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            _depositService.TopUpAsync(encounter.Id, req, Guid.NewGuid(), _actorUserId, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 13: History tampil dengan nilai BalanceAfter yang benar (termasuk mutasi)
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario13_MovementHistory_CalculatesCorrectRunningBalance()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);
        var now = DateTimeOffset.UtcNow;

        var account = new BilDepositAccount
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            AccountNumber = "DEP-HIST-001",
            AvailableBalance = 6_500_000m,
            Status = BillingDepositAccountStatuses.Active,
            RowVersion = Guid.NewGuid()
        };
        _dbContext.BilDepositAccounts.Add(account);

        var mov1 = new BilDepositMovement
        {
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.TopUp,
            Amount = 10_000_000m,
            OccurredAt = now.AddHours(-3),
            Reason = "Top Up 10jt",
            PayloadHash = "hash1"
        };
        var mov2 = new BilDepositMovement
        {
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.Allocation,
            Amount = 4_000_000m,
            OccurredAt = now.AddHours(-2),
            Reason = "Alokasi tagihan 4jt",
            PayloadHash = "hash2"
        };
        var mov3 = new BilDepositMovement
        {
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.Release,
            Amount = 500_000m,
            OccurredAt = now.AddHours(-1),
            Reason = "Pembatalan alokasi tagihan 500rb",
            ReversesMovementId = mov2.Id,
            PayloadHash = "hash3"
        };
        _dbContext.BilDepositMovements.AddRange(mov1, mov2, mov3);
        _dbContext.SaveChanges();

        var workspace = await _depositService.GetDepositWorkspaceAsync(
            encounter.Id, 1, 10, null, null, null, null, CancellationToken.None);

        Assert.Equal(3, workspace.Items.Count);
        // mov1: +10jt -> BalanceAfter: 10jt
        Assert.Equal(10_000_000m, workspace.Items[0].BalanceAfter);
        // mov2: -4jt -> BalanceAfter: 6jt
        Assert.Equal(6_000_000m, workspace.Items[1].BalanceAfter);
        // mov3: +500rb -> BalanceAfter: 6.5jt
        Assert.Equal(6_500_000m, workspace.Items[2].BalanceAfter);
        Assert.Equal(account.AvailableBalance, workspace.Items[2].BalanceAfter);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 14: Pagination/filter movement tidak merusak running balance
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario14_PaginationAndFilter_PreservesAccurateBalanceAfter()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);
        var now = DateTimeOffset.UtcNow;

        var account = new BilDepositAccount
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            AccountNumber = "DEP-RUN-001",
            AvailableBalance = 7_000_000m,
            Status = BillingDepositAccountStatuses.Active,
            RowVersion = Guid.NewGuid()
        };
        _dbContext.BilDepositAccounts.Add(account);

        var mov1 = new BilDepositMovement
        {
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.TopUp,
            Amount = 10_000_000m,
            OccurredAt = now.AddHours(-3),
            Reason = "Top Up #1",
            PayloadHash = "hash1"
        };
        var mov2 = new BilDepositMovement
        {
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.Allocation,
            Amount = 5_000_000m,
            OccurredAt = now.AddHours(-2),
            Reason = "Alokasi",
            PayloadHash = "hash2"
        };
        var mov3 = new BilDepositMovement
        {
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.TopUp,
            Amount = 2_000_000m,
            OccurredAt = now.AddHours(-1),
            Reason = "Top Up #2",
            PayloadHash = "hash3"
        };
        _dbContext.BilDepositMovements.AddRange(mov1, mov2, mov3);
        _dbContext.SaveChanges();

        // Filter HANYA TOP_UP
        var filteredWorkspace = await _depositService.GetDepositWorkspaceAsync(
            encounter.Id, 1, 10, BillingDepositMovementTypes.TopUp, null, null, null, CancellationToken.None);

        Assert.Equal(2, filteredWorkspace.Items.Count);
        // mov1 (TopUp): BalanceAfter tetap 10jt
        Assert.Equal(10_000_000m, filteredWorkspace.Items[0].BalanceAfter);
        // mov3 (TopUp): BalanceAfter tetap 7jt (bukan 12jt karena mutasi alokasi di tengah tetap dihitung)
        Assert.Equal(7_000_000m, filteredWorkspace.Items[1].BalanceAfter);

        // Halaman kedua dengan pageSize 1
        var page2Workspace = await _depositService.GetDepositWorkspaceAsync(
            encounter.Id, 2, 1, null, null, null, null, CancellationToken.None);

        Assert.Single(page2Workspace.Items);
        // Item kedua adalah mov2 (Allocation): BalanceAfter tetap 5jt (10jt - 5jt)
        Assert.Equal(mov2.Id, page2Workspace.Items[0].Id);
        Assert.Equal(5_000_000m, page2Workspace.Items[0].BalanceAfter);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 15: Kunjungan selesai dapat dibaca tanpa mengizinkan transaksi baru
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario15_CompletedEncounter_CanReadWorkspace_DisallowsNewTransactionsIfClosed()
    {
        var completedEncounter = CreateEncounter(EncounterType.Inpatient, EncounterStatus.Completed, "ENC-DONE-01");

        // Workspace tetap dapat dibaca dengan status 200 OK
        var workspace = await _depositService.GetDepositWorkspaceAsync(
            completedEncounter.Id, 1, 10, null, null, null, null, CancellationToken.None);
        Assert.NotNull(workspace);
        Assert.Equal(completedEncounter.Id, workspace.EncounterId);

        // Jika akun sudah CLOSED, top-up baru ditolak
        var account = new BilDepositAccount
        {
            Id = Guid.NewGuid(),
            EncounterId = completedEncounter.Id,
            AccountNumber = "DEP-CLOSED-01",
            AvailableBalance = 0m,
            Status = BillingDepositAccountStatuses.Closed,
            RowVersion = Guid.NewGuid()
        };
        _dbContext.BilDepositAccounts.Add(account);
        _dbContext.SaveChanges();

        var req = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 500_000m,
            Reason = "Uji akun ditutup",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            ExpectedRowVersion = account.RowVersion
        };

        var ex = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            _depositService.TopUpAsync(completedEncounter.Id, req, Guid.NewGuid(), _actorUserId, CancellationToken.None));
        Assert.Contains("ditutup", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 16: Metadata otorisasi endpoint sesuai RBAC
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public void Scenario16_AuthorizationMetadata_EnforcesBillingDepositReadAndCreate()
    {
        var workspaceMethod = typeof(BillingPatientFundsController).GetMethod(nameof(BillingPatientFundsController.GetDepositWorkspace));
        Assert.NotNull(workspaceMethod);

        var accessPermission = workspaceMethod.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(accessPermission);
        Assert.Equal("BillingDeposit", accessPermission.Resource);
        Assert.Equal("Read", accessPermission.Action);

        var topUpMethod = typeof(BillingPatientFundsController).GetMethod(nameof(BillingPatientFundsController.TopUp));
        Assert.NotNull(topUpMethod);

        var topUpPermission = topUpMethod.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(topUpPermission);
        Assert.Equal("BillingDeposit", topUpPermission.Resource);
        Assert.Equal("Create", topUpPermission.Action);
    }

    // ---------------------------------------------------------------------------------------------
    // BE-08 Skenario 17: Integrasi endpoint existing GetByEncounterAsync tidak mengalami regresi
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task Scenario17_ExistingGetByEncounterAsync_MaintainsBackwardCompatibility()
    {
        var encounter = CreateEncounter(EncounterType.Inpatient);
        var req = new DepositTopUpRequest
        {
            PaymentMethodId = _transferPaymentMethodId,
            Amount = 1_500_000m,
            Reason = "Deposit existing compatibility",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };
        await _depositService.TopUpAsync(encounter.Id, req, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        var depositResponse = await _depositService.GetByEncounterAsync(encounter.Id, CancellationToken.None);

        Assert.NotNull(depositResponse);
        Assert.Equal(encounter.Id, depositResponse.EncounterId);
        Assert.Equal(1_500_000m, depositResponse.AvailableBalance);
        Assert.Single(depositResponse.Movements);
    }
}
