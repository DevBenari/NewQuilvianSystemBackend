using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.BillingManagement;

/// <summary>
/// BE-BKC-037 — siklus hidup voucher kas kecil penuh. Menutup BIL-AT-064 (alur penuh), bagian
/// domain BIL-AT-067,072,073 (pembatalan oleh bukan-pemohon), 047 (penjaga persetujuan), 046/048
/// (penjaga pencairan), 049-052 (penolakan/pembatalan/immutability REJECTED), dan idempotency
/// replay yang dapat dibuktikan tanpa PostgreSQL. Kunci pg_advisory_xact_lock dan concurrency
/// pencairan ganda (BIL-AT-071) sendiri hanya dapat dibuktikan pada provider relational.
/// </summary>
public sealed class PettyCashVoucherServiceTests
{
    private static readonly Guid Actor = Guid.NewGuid();
    private const string ActorRole = "Kepala Kasir";

    private static (PettyCashVoucherService Service, PettyCashBudgetService Budget) CreateServices(ApplicationDbContext db)
    {
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        var numberSeries = new BillingNumberSeriesService(db, Options.Create(new BillingInvoiceNumberOptions()));
        var categoryService = new PettyCashCategoryService(db, logger);
        var budgetService = new PettyCashBudgetService(db, logger);
        var voucherService = new PettyCashVoucherService(db, numberSeries, categoryService, budgetService, logger);
        return (voucherService, budgetService);
    }

    private static async Task<BilPettyCashBudget> SeedBudgetAsync(ApplicationDbContext db, decimal currentBalance)
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

    private static async Task<MstPettyCashCategory> SeedActiveCategoryAsync(ApplicationDbContext db)
    {
        var category = new MstPettyCashCategory
        {
            CategoryCode = "TRANSPORT",
            CategoryName = "Transport",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Actor
        };
        db.MstPettyCashCategories.Add(category);
        await db.SaveChangesAsync(CancellationToken.None);
        return category;
    }

    private static CreatePettyCashVoucherRequest CreateRequest(Guid categoryId, decimal amount = 300_000m) => new()
    {
        RecipientName = "Budi Santoso",
        CategoryId = categoryId,
        Amount = amount,
        Purpose = "Ongkos kirim dokumen"
    };

    // ---------------------------------------------------------------------------------------
    // BIL-AT-064 — alur penuh pengajuan sampai Selesai
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task FullLifecycle_SubmitApproveDisburseAttachProof_MovesThroughEveryStatus()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, budget) = CreateServices(db);
        await SeedBudgetAsync(db, 5_000_000m);
        var category = await SeedActiveCategoryAsync(db);

        var created = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        Assert.Equal(PettyCashVoucherStatuses.WaitingApproval, created.Status);
        Assert.StartsWith("PTC-", created.VoucherNumber, StringComparison.Ordinal);
        Assert.Equal(["APPROVE", "REJECT", "CANCEL"], created.AvailableActions);

        var approved = await service.ApproveAsync(
            created.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = created.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        Assert.Equal(PettyCashVoucherStatuses.Approved, approved.Status);
        var currentAfterApprove = await budget.GetCurrentAsync(CancellationToken.None);
        Assert.Equal(5_000_000m, currentAfterApprove.CurrentBalance);
        Assert.Equal(300_000m, currentAfterApprove.ReservedAmount);

        var disbursed = await service.DisburseAsync(
            created.Id, new DisbursePettyCashVoucherRequest { ExpectedRowVersion = approved.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        Assert.Equal(PettyCashVoucherStatuses.CashReceived, disbursed.Status);
        var currentAfterDisburse = await budget.GetCurrentAsync(CancellationToken.None);
        Assert.Equal(4_700_000m, currentAfterDisburse.CurrentBalance);
        Assert.Equal(0m, currentAfterDisburse.ReservedAmount);

        var completed = await service.AttachProofAsync(
            created.Id, new AttachPettyCashProofRequest { ExpectedRowVersion = disbursed.RowVersion, ProofReferenceNumber = "NT-8891" },
            Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        Assert.Equal(PettyCashVoucherStatuses.Completed, completed.Status);
        Assert.Equal("NT-8891", completed.ProofReferenceNumber);
        Assert.NotNull(completed.CompletedAt);

        var detail = await service.GetByIdAsync(created.Id, CancellationToken.None);
        Assert.Equal(4, detail.Commands.Count);
        Assert.Equal(["SUBMIT", "APPROVE", "DISBURSE", "ATTACH_PROOF"], detail.Commands.Select(c => c.CommandType).ToList());
    }

    [Fact]
    public async Task AttachProof_Correction_KeepsCompletedAndRecordsProofCorrected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var created = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var approved = await service.ApproveAsync(created.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = created.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var disbursed = await service.DisburseAsync(created.Id, new DisbursePettyCashVoucherRequest { ExpectedRowVersion = approved.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var completed = await service.AttachProofAsync(
            created.Id, new AttachPettyCashProofRequest { ExpectedRowVersion = disbursed.RowVersion, ProofReferenceNumber = "NT-0001" }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        var corrected = await service.AttachProofAsync(
            created.Id, new AttachPettyCashProofRequest { ExpectedRowVersion = completed.RowVersion, ProofReferenceNumber = "NT-0002" }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        Assert.Equal(PettyCashVoucherStatuses.Completed, corrected.Status);
        Assert.Equal("NT-0002", corrected.ProofReferenceNumber);
        var lastCommand = (await service.GetByIdAsync(created.Id, CancellationToken.None)).Commands.Last();
        Assert.Equal("PROOF_CORRECTED", lastCommand.CommandType);
    }

    // ---------------------------------------------------------------------------------------
    // Create — BIL-VAL-044/045/058
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Create_InactiveCategory_ThrowsValidation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        category.IsActive = false;
        await db.SaveChangesAsync(CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashVoucherValidationException>(() =>
            service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));
    }

    [Fact]
    public async Task Create_ZeroAmount_ThrowsBadRequest()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);

        await Assert.ThrowsAsync<PettyCashVoucherBadRequestException>(() =>
            service.CreateAsync(CreateRequest(category.Id, 0m), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));
    }

    [Fact]
    public async Task Create_SameIdempotencyKeyTwice_ReturnsReplayWithoutSecondVoucher()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var key = Guid.NewGuid();
        var request = CreateRequest(category.Id);

        var first = await service.CreateAsync(request, key, Actor, ActorRole, CancellationToken.None);
        var second = await service.CreateAsync(request, key, Actor, ActorRole, CancellationToken.None);

        Assert.False(first.IsReplay);
        Assert.True(second.IsReplay);
        Assert.Equal(first.Id, second.Id);
        Assert.Single(await db.BilPettyCashVouchers.ToListAsync());
    }

    // ---------------------------------------------------------------------------------------
    // Approve — BIL-VAL-047, PC-DES-005
    // ---------------------------------------------------------------------------------------

    // Contoh berangka dari state-transition-matrix.md: saldo 5.000.000, voucher A 300.000
    // disetujui (komitmen 300.000, sisa bebas 4.700.000). Voucher B 4.800.000 melebihi sisa
    // bebas walau di bawah saldo mentah 5.000.000 - harus ditolak (PC-DES-005).
    [Fact]
    public async Task Approve_ExceedsAvailableAfterCommitment_ThrowsValidation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 5_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var voucherA = await service.CreateAsync(CreateRequest(category.Id, 300_000m), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        await service.ApproveAsync(voucherA.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = voucherA.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        var voucherB = await service.CreateAsync(CreateRequest(category.Id, 4_800_000m), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var exception = await Assert.ThrowsAsync<PettyCashVoucherValidationException>(() =>
            service.ApproveAsync(voucherB.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = voucherB.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));
        Assert.Contains("4,700,000", exception.Message, StringComparison.Ordinal);

        var reread = await service.GetByIdAsync(voucherB.Id, CancellationToken.None);
        Assert.Equal(PettyCashVoucherStatuses.WaitingApproval, reread.Voucher.Status);
    }

    [Fact]
    public async Task Approve_WrongStatus_ThrowsValidation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var voucher = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var approved = await service.ApproveAsync(voucher.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = voucher.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashVoucherValidationException>(() =>
            service.ApproveAsync(voucher.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = approved.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------
    // Reject — BIL-VAL-049, immutability BIL-VAL-052
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Reject_EmptyReason_ThrowsValidation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var voucher = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashVoucherValidationException>(() =>
            service.RejectAsync(voucher.Id, new RejectPettyCashVoucherRequest { ExpectedRowVersion = voucher.RowVersion, RejectionReason = "  " }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));
    }

    [Fact]
    public async Task Reject_ThenAnyFurtherAction_ThrowsValidation_BecauseTerminalAndImmutable()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var voucher = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var rejected = await service.RejectAsync(
            voucher.Id, new RejectPettyCashVoucherRequest { ExpectedRowVersion = voucher.RowVersion, RejectionReason = "Tidak sesuai kebijakan" }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        Assert.Equal(PettyCashVoucherStatuses.Rejected, rejected.Status);
        Assert.Equal("Ditolak", rejected.StatusLabel);
        Assert.Empty(rejected.AvailableActions);

        await Assert.ThrowsAsync<PettyCashVoucherValidationException>(() =>
            service.ApproveAsync(voucher.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = rejected.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------
    // Cancel — BIL-VAL-050 (403 bukan pemohon, 422 sudah diputuskan)
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Cancel_ByNonRequester_ThrowsForbidden()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var requester = Actor;
        var voucher = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), requester, ActorRole, CancellationToken.None);

        var someoneElse = Guid.NewGuid();
        await Assert.ThrowsAsync<PettyCashVoucherForbiddenException>(() =>
            service.CancelAsync(voucher.Id, new CancelPettyCashVoucherRequest { ExpectedRowVersion = voucher.RowVersion, Reason = "Salah input" }, Guid.NewGuid(), someoneElse, ActorRole, CancellationToken.None));
    }

    [Fact]
    public async Task Cancel_ByRequester_SetsIsCancelledButKeepsStatusWaitingApproval()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var voucher = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        var cancelled = await service.CancelAsync(
            voucher.Id, new CancelPettyCashVoucherRequest { ExpectedRowVersion = voucher.RowVersion, Reason = "Salah input" }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        Assert.Equal(PettyCashVoucherStatuses.WaitingApproval, cancelled.Status);
        Assert.True(cancelled.IsCancelled);
    }

    [Fact]
    public async Task Cancel_AfterApproved_ThrowsValidation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var voucher = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var approved = await service.ApproveAsync(voucher.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = voucher.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashVoucherValidationException>(() =>
            service.CancelAsync(voucher.Id, new CancelPettyCashVoucherRequest { ExpectedRowVersion = approved.RowVersion, Reason = "Terlambat" }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------
    // Disburse — BIL-VAL-046/048/057 (lewat PettyCashBudgetService)
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Disburse_WithoutApproval_ThrowsValidation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var voucher = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashVoucherValidationException>(() =>
            service.DisburseAsync(voucher.Id, new DisbursePettyCashVoucherRequest { ExpectedRowVersion = voucher.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));
    }

    // BIL-VAL-048: saldo dikoreksi turun setelah persetujuan, sebelum pencairan.
    [Fact]
    public async Task Disburse_BudgetReducedAfterApproval_ThrowsInsufficientBalance()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, budget) = CreateServices(db);
        var budgetRow = await SeedBudgetAsync(db, 5_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var voucher = await service.CreateAsync(CreateRequest(category.Id, 300_000m), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var approved = await service.ApproveAsync(voucher.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = voucher.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        await budget.AdjustAsync(
            new PettyCashBudgetAdjustmentRequest
            {
                Amount = 4_900_000m, Reason = "Penghitungan ulang fisik", Direction = "DECREASE", ExpectedRowVersion = budgetRow.RowVersion
            }, Guid.NewGuid(), Actor, CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashBudgetInsufficientBalanceException>(() =>
            service.DisburseAsync(voucher.Id, new DisbursePettyCashVoucherRequest { ExpectedRowVersion = approved.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));

        var reread = await service.GetByIdAsync(voucher.Id, CancellationToken.None);
        Assert.Equal(PettyCashVoucherStatuses.Approved, reread.Voucher.Status);
    }

    // ---------------------------------------------------------------------------------------
    // RowVersion concurrency
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Approve_StaleRowVersion_ThrowsConflict()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        var voucher = await service.CreateAsync(CreateRequest(category.Id), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        await Assert.ThrowsAsync<PettyCashVoucherConflictException>(() =>
            service.ApproveAsync(voucher.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = Guid.NewGuid() }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None));
    }

    // ---------------------------------------------------------------------------------------
    // Read: Summary
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task GetSummaryAsync_CountsPerStatusAndWaitingAmount()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (service, _) = CreateServices(db);
        await SeedBudgetAsync(db, 1_000_000m);
        var category = await SeedActiveCategoryAsync(db);
        await service.CreateAsync(CreateRequest(category.Id, 100_000m), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var second = await service.CreateAsync(CreateRequest(category.Id, 200_000m), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        await service.RejectAsync(second.Id, new RejectPettyCashVoucherRequest { ExpectedRowVersion = second.RowVersion, RejectionReason = "Tidak relevan" }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        var summary = await service.GetSummaryAsync(CancellationToken.None);

        Assert.Equal(1, summary.WaitingApprovalCount);
        Assert.Equal(1, summary.RejectedCount);
        Assert.Equal(100_000m, summary.TotalWaitingApprovalAmount);
    }

    [Fact]
    public void AddBillingManagement_RegistersPettyCashVoucherService()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => IsolatedBillingDbContextFactory.Create());
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<ILogger<LoggerService>>(NullLogger<LoggerService>.Instance);
        services.AddScoped<LoggerService>();
        services.AddBillingManagement();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<PettyCashVoucherService>());
    }
}
