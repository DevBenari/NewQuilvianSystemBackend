using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.BillingManagement;

/// <summary>
/// BE-BKC-036 — kolam anggaran dan saldo berjalan kas kecil. Menutup bagian domain BIL-AT-080
/// dan bagian anggaran BIL-AT-067 yang dapat dibuktikan tanpa PostgreSQL: top-up, koreksi naik/
/// turun (BIL-VAL-054/055), penjaga sisa bebas (PC-DES-005 lewat CalculateReservedAmountAsync),
/// dan pencairan (BIL-VAL-048) lewat ApplyDisbursementAsync yang dipanggil langsung dari harness
/// transaction test — bukan lewat endpoint, karena endpoint disburse ada di BE-BKC-037.
/// Kunci pg_advisory_xact_lock sendiri hanya dapat dibuktikan pada provider relational.
/// </summary>
public sealed class PettyCashBudgetServiceTests
{
    private static readonly Guid Actor = Guid.NewGuid();

    private static PettyCashBudgetService CreateService(ApplicationDbContext db) =>
        new(db, new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()));

    private static async Task<BilPettyCashBudget> SeedBudgetAsync(ApplicationDbContext db, decimal currentBalance = 0m)
    {
        var budget = new BilPettyCashBudget
        {
            PoolCode = "HOSPITAL_MAIN",
            PoolName = "Kas Kecil Rumah Sakit",
            CurrentBalance = currentBalance,
            Status = PettyCashBudgetStatuses.Active,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Actor
        };
        db.BilPettyCashBudgets.Add(budget);
        await db.SaveChangesAsync(CancellationToken.None);
        return budget;
    }

    private static BilPettyCashVoucher ApprovedVoucher(decimal amount) => new()
    {
        VoucherNumber = $"PTC-20260907-{Random.Shared.Next(1000, 9999)}",
        RecipientName = "Budi Santoso",
        CategoryId = Guid.NewGuid(),
        Amount = amount,
        Purpose = "Uji coba",
        Status = PettyCashVoucherStatuses.Approved,
        RequestedBy = Guid.NewGuid(),
        SubmittedAt = DateTimeOffset.UtcNow,
        DecidedBy = Guid.NewGuid(),
        DecidedAt = DateTimeOffset.UtcNow
    };

    // ---------------------------------------------------------------------------------------
    // GetCurrentAsync
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task GetCurrentAsync_ReturnsBalanceReservedAndAvailable()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        await SeedBudgetAsync(db, currentBalance: 5_000_000m);
        db.BilPettyCashVouchers.Add(ApprovedVoucher(300_000m));
        await db.SaveChangesAsync(CancellationToken.None);
        var service = CreateService(db);

        var current = await service.GetCurrentAsync(CancellationToken.None);

        Assert.Equal(5_000_000m, current.CurrentBalance);
        Assert.Equal(300_000m, current.ReservedAmount);
        Assert.Equal(4_700_000m, current.AvailableAmount);
    }

    // ---------------------------------------------------------------------------------------
    // TopUpAsync
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task TopUpAsync_IncreasesBalanceAndWritesLedgerRow()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db, currentBalance: 0m);
        var service = CreateService(db);

        var result = await service.TopUpAsync(
            new PettyCashBudgetTopUpRequest { Amount = 5_000_000m, Reason = "Modal awal kas kecil", ExpectedRowVersion = budget.RowVersion },
            Guid.NewGuid(), Actor, CancellationToken.None);

        Assert.Equal(5_000_000m, result.CurrentBalance);
        Assert.Equal(5_000_000m, result.TotalTopUpAmount);

        var movement = Assert.Single(await db.BilPettyCashBudgetMovements.ToListAsync());
        Assert.Equal(PettyCashBudgetMovementTypes.TopUp, movement.MovementType);
        Assert.Equal(0m, movement.BalanceBefore);
        Assert.Equal(5_000_000m, movement.BalanceAfter);
        Assert.Equal("Modal awal kas kecil", movement.Reason);
    }

    [Fact]
    public async Task TopUpAsync_StaleRowVersion_ThrowsConflict()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        await SeedBudgetAsync(db, currentBalance: 0m);
        var service = CreateService(db);

        await Assert.ThrowsAsync<PettyCashBudgetConflictException>(() => service.TopUpAsync(
            new PettyCashBudgetTopUpRequest { Amount = 100_000m, Reason = "Uji", ExpectedRowVersion = Guid.NewGuid() },
            Guid.NewGuid(), Actor, CancellationToken.None));
    }

    [Fact]
    public async Task TopUpAsync_SameIdempotencyKeyTwice_DoesNotDoubleApply()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db, currentBalance: 0m);
        var service = CreateService(db);
        var key = Guid.NewGuid();
        var request = new PettyCashBudgetTopUpRequest { Amount = 1_000_000m, Reason = "Top-up pertama", ExpectedRowVersion = budget.RowVersion };

        var first = await service.TopUpAsync(request, key, Actor, CancellationToken.None);
        var second = await service.TopUpAsync(request, key, Actor, CancellationToken.None);

        Assert.Equal(1_000_000m, first.CurrentBalance);
        Assert.Equal(1_000_000m, second.CurrentBalance);
        Assert.Single(await db.BilPettyCashBudgetMovements.ToListAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task TopUpAsync_NonPositiveAmount_ThrowsValidation(int amount)
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db);
        var service = CreateService(db);

        await Assert.ThrowsAsync<PettyCashBudgetValidationException>(() => service.TopUpAsync(
            new PettyCashBudgetTopUpRequest { Amount = amount, Reason = "Uji", ExpectedRowVersion = budget.RowVersion },
            Guid.NewGuid(), Actor, CancellationToken.None));
    }

    [Fact]
    public async Task TopUpAsync_EmptyReason_ThrowsValidation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db);
        var service = CreateService(db);

        await Assert.ThrowsAsync<PettyCashBudgetValidationException>(() => service.TopUpAsync(
            new PettyCashBudgetTopUpRequest { Amount = 100_000m, Reason = "  ", ExpectedRowVersion = budget.RowVersion },
            Guid.NewGuid(), Actor, CancellationToken.None));
    }

    [Fact]
    public async Task TopUpAsync_EmptyIdempotencyKey_ThrowsValidation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db);
        var service = CreateService(db);

        await Assert.ThrowsAsync<PettyCashBudgetValidationException>(() => service.TopUpAsync(
            new PettyCashBudgetTopUpRequest { Amount = 100_000m, Reason = "Uji", ExpectedRowVersion = budget.RowVersion },
            Guid.Empty, Actor, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------
    // AdjustAsync — BIL-VAL-054/055
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task AdjustAsync_Increase_RaisesBalanceWithoutTouchingTotalTopUp()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db, currentBalance: 1_000_000m);
        var service = CreateService(db);

        var result = await service.AdjustAsync(
            new PettyCashBudgetAdjustmentRequest { Amount = 200_000m, Reason = "Penghitungan ulang fisik", Direction = "INCREASE", ExpectedRowVersion = budget.RowVersion },
            Guid.NewGuid(), Actor, CancellationToken.None);

        Assert.Equal(1_200_000m, result.CurrentBalance);
        Assert.Equal(0m, result.TotalTopUpAmount);
    }

    [Fact]
    public async Task AdjustAsync_DecreaseWithinFreeBalance_Succeeds()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db, currentBalance: 1_000_000m);
        var service = CreateService(db);

        var result = await service.AdjustAsync(
            new PettyCashBudgetAdjustmentRequest { Amount = 300_000m, Reason = "Penghitungan ulang fisik", Direction = "decrease", ExpectedRowVersion = budget.RowVersion },
            Guid.NewGuid(), Actor, CancellationToken.None);

        Assert.Equal(700_000m, result.CurrentBalance);
    }

    // Contoh berangka dari 02-backend-architecture.md § "Perpindahan status": saldo Rp 5.000.000,
    // satu voucher Rp 4.800.000 sudah APPROVED (komitmen). Finance mencoba koreksi turun Rp
    // 4.900.000 sehingga hasilnya Rp 100.000 — kurang dari reservedAmount Rp 4.800.000. DITOLAK.
    [Fact]
    public async Task AdjustAsync_DecreaseBelowReservedAmount_ThrowsInsufficientBalance()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db, currentBalance: 5_000_000m);
        db.BilPettyCashVouchers.Add(ApprovedVoucher(4_800_000m));
        await db.SaveChangesAsync(CancellationToken.None);
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<PettyCashBudgetInsufficientBalanceException>(() => service.AdjustAsync(
            new PettyCashBudgetAdjustmentRequest { Amount = 4_900_000m, Reason = "Penghitungan ulang fisik", Direction = "DECREASE", ExpectedRowVersion = budget.RowVersion },
            Guid.NewGuid(), Actor, CancellationToken.None));
        Assert.Contains("4,800,000", exception.Message, StringComparison.Ordinal);

        var unchanged = await service.GetCurrentAsync(CancellationToken.None);
        Assert.Equal(5_000_000m, unchanged.CurrentBalance);
    }

    [Fact]
    public async Task AdjustAsync_DecreaseBelowZero_ThrowsInsufficientBalance()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db, currentBalance: 100_000m);
        var service = CreateService(db);

        await Assert.ThrowsAsync<PettyCashBudgetInsufficientBalanceException>(() => service.AdjustAsync(
            new PettyCashBudgetAdjustmentRequest { Amount = 200_000m, Reason = "Uji", Direction = "DECREASE", ExpectedRowVersion = budget.RowVersion },
            Guid.NewGuid(), Actor, CancellationToken.None));
    }

    [Fact]
    public async Task AdjustAsync_InvalidDirection_ThrowsValidation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db, currentBalance: 100_000m);
        var service = CreateService(db);

        await Assert.ThrowsAsync<PettyCashBudgetValidationException>(() => service.AdjustAsync(
            new PettyCashBudgetAdjustmentRequest { Amount = 10_000m, Reason = "Uji", Direction = "SIDEWAYS", ExpectedRowVersion = budget.RowVersion },
            Guid.NewGuid(), Actor, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------
    // CalculateReservedAmountAsync — PC-DES-005
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task CalculateReservedAmountAsync_OnlyCountsApprovedActiveVouchers()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        await SeedBudgetAsync(db);
        db.BilPettyCashVouchers.AddRange(
            ApprovedVoucher(300_000m),
            new BilPettyCashVoucher
            {
                VoucherNumber = "PTC-20260907-0002", RecipientName = "A", CategoryId = Guid.NewGuid(),
                Amount = 999_000m, Purpose = "Uji", Status = PettyCashVoucherStatuses.WaitingApproval,
                RequestedBy = Guid.NewGuid(), SubmittedAt = DateTimeOffset.UtcNow
            },
            new BilPettyCashVoucher
            {
                VoucherNumber = "PTC-20260907-0003", RecipientName = "B", CategoryId = Guid.NewGuid(),
                Amount = 999_000m, Purpose = "Uji", Status = PettyCashVoucherStatuses.Completed,
                RequestedBy = Guid.NewGuid(), SubmittedAt = DateTimeOffset.UtcNow
            },
            new BilPettyCashVoucher
            {
                VoucherNumber = "PTC-20260907-0004", RecipientName = "C", CategoryId = Guid.NewGuid(),
                Amount = 999_000m, Purpose = "Uji", Status = PettyCashVoucherStatuses.Approved,
                RequestedBy = Guid.NewGuid(), SubmittedAt = DateTimeOffset.UtcNow, IsCancel = true
            });
        await db.SaveChangesAsync(CancellationToken.None);
        var service = CreateService(db);

        var reserved = await service.CalculateReservedAmountAsync(CancellationToken.None);

        Assert.Equal(300_000m, reserved);
    }

    // ---------------------------------------------------------------------------------------
    // ApplyDisbursementAsync — dipanggil dari harness transaction, bukan endpoint (BE-BKC-037)
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task ApplyDisbursementAsync_ReducesBalanceAndWritesDisbursementMovement()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        await SeedBudgetAsync(db, currentBalance: 5_000_000m);
        var voucher = ApprovedVoucher(300_000m);
        db.BilPettyCashVouchers.Add(voucher);
        await db.SaveChangesAsync(CancellationToken.None);
        var service = CreateService(db);
        var correlationId = Guid.NewGuid();

        // Harness: pemanggil (nanti PettyCashVoucherService pada BE-BKC-037) bertanggung jawab
        // membuka transaction dan memanggil SaveChangesAsync sendiri; test ini mensimulasikannya.
        await service.ApplyDisbursementAsync(voucher, Actor, correlationId, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        var current = await service.GetCurrentAsync(CancellationToken.None);
        Assert.Equal(4_700_000m, current.CurrentBalance);

        var movement = Assert.Single(await db.BilPettyCashBudgetMovements.ToListAsync());
        Assert.Equal(PettyCashBudgetMovementTypes.Disbursement, movement.MovementType);
        Assert.Equal(voucher.Id, movement.VoucherId);
        Assert.Equal(300_000m, movement.Amount);
        Assert.Null(movement.Reason);
        Assert.Equal(correlationId, movement.CorrelationId);
    }

    [Fact]
    public async Task ApplyDisbursementAsync_AmountExceedsCurrentBalance_ThrowsInsufficientBalance()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        await SeedBudgetAsync(db, currentBalance: 200_000m);
        var voucher = ApprovedVoucher(300_000m);
        db.BilPettyCashVouchers.Add(voucher);
        await db.SaveChangesAsync(CancellationToken.None);
        var service = CreateService(db);

        await Assert.ThrowsAsync<PettyCashBudgetInsufficientBalanceException>(
            () => service.ApplyDisbursementAsync(voucher, Actor, Guid.NewGuid(), CancellationToken.None));

        var current = await service.GetCurrentAsync(CancellationToken.None);
        Assert.Equal(200_000m, current.CurrentBalance);
    }

    [Fact]
    public async Task ApplyDisbursementAsync_VoucherAlreadyDisbursed_ThrowsConflict()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        await SeedBudgetAsync(db, currentBalance: 5_000_000m);
        var voucher = ApprovedVoucher(300_000m);
        db.BilPettyCashVouchers.Add(voucher);
        await db.SaveChangesAsync(CancellationToken.None);
        var service = CreateService(db);
        await service.ApplyDisbursementAsync(voucher, Actor, Guid.NewGuid(), CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashBudgetConflictException>(
            () => service.ApplyDisbursementAsync(voucher, Actor, Guid.NewGuid(), CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------
    // GetMovementsAsync
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task GetMovementsAsync_FiltersByMovementTypeAndOrdersNewestFirst()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var budget = await SeedBudgetAsync(db, currentBalance: 1_500_000m);
        var earlier = DateTimeOffset.UtcNow.AddHours(-2);
        var later = DateTimeOffset.UtcNow.AddHours(-1);
        db.BilPettyCashBudgetMovements.AddRange(
            new BilPettyCashBudgetMovement
            {
                BudgetId = budget.Id, MovementType = PettyCashBudgetMovementTypes.TopUp, Amount = 1_000_000m,
                BalanceBefore = 0m, BalanceAfter = 1_000_000m, Reason = "Top-up 1", ActorUserId = Actor,
                CorrelationId = Guid.NewGuid(), OccurredAt = earlier, CreateDateTime = DateTime.UtcNow, CreateBy = Actor
            },
            new BilPettyCashBudgetMovement
            {
                BudgetId = budget.Id, MovementType = PettyCashBudgetMovementTypes.TopUp, Amount = 500_000m,
                BalanceBefore = 1_000_000m, BalanceAfter = 1_500_000m, Reason = "Top-up 2", ActorUserId = Actor,
                CorrelationId = Guid.NewGuid(), OccurredAt = later, CreateDateTime = DateTime.UtcNow, CreateBy = Actor
            },
            new BilPettyCashBudgetMovement
            {
                BudgetId = budget.Id, MovementType = PettyCashBudgetMovementTypes.Adjustment, Amount = 50_000m,
                BalanceBefore = 1_500_000m, BalanceAfter = 1_450_000m, Reason = "Koreksi", ActorUserId = Actor,
                CorrelationId = Guid.NewGuid(), OccurredAt = later.AddMinutes(30), CreateDateTime = DateTime.UtcNow, CreateBy = Actor
            });
        await db.SaveChangesAsync(CancellationToken.None);
        var service = CreateService(db);

        var page = await service.GetMovementsAsync(new PettyCashBudgetMovementQuery { MovementType = "top_up" }, CancellationToken.None);

        Assert.Equal(2, page.TotalData);
        Assert.Equal("Top-up 2", page.Items[0].Reason);
        Assert.Equal("Top-up 1", page.Items[1].Reason);
    }

    [Fact]
    public void AddBillingManagement_RegistersPettyCashBudgetService()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => IsolatedBillingDbContextFactory.Create());
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<ILogger<LoggerService>>(NullLogger<LoggerService>.Instance);
        services.AddScoped<LoggerService>();
        services.AddBillingManagement();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<PettyCashBudgetService>());
    }
}
