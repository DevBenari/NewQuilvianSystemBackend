using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// BE-FIN-120 — Persetujuan pembayaran harus sesuai jenjang nominal (FIN-VAL-052, AP-UAT-15).
///
/// Ambang Rp 50.000.000 (FIN-DEC-052): di bawahnya TIER_1 (Supervisor atau Manajer Finance boleh
/// menyetujui), mulai dari ambang TIER_2 (hanya Manajer Finance). Contoh: pembayaran Rp 60.000.000
/// yang dicoba disetujui Supervisor Finance harus ditolak 403 dan tetap berstatus SUBMITTED.
/// </summary>
public class FinancePaymentApprovalTierTests : IDisposable
{
    private const decimal Tier1Amount = 10_000_000m;
    private const decimal Tier2Amount = 60_000_000m;

    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly FinancePaymentService _service;
    private readonly MstSupplier _supplier;
    private readonly MstBankAccount _bankAccount;

    public FinancePaymentApprovalTierTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

        _service = new FinancePaymentService(
            _dbContext,
            loggerService,
            null!,
            null!,
            null!,
            FinanceApSeed.BuildApprovalAuthorization(_dbContext));

        _supplier = FinanceApSeed.AddSupplier(_dbContext, "SUP-A");
        _bankAccount = FinanceApSeed.AddBankAccount(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    private async Task<FinPayment> CreateSubmittedPaymentAsync(decimal amount, Guid requesterId)
    {
        var payable = FinanceApSeed.AddPayable(_dbContext, _supplier, $"INV-{Guid.NewGuid():N}", amount);

        var draft = await _service.CreateDraftAsync(
            FinPaymentTypes.Supplier, _supplier.Id, _bankAccount.Id, FinPaymentMethods.Transfer, null,
            new List<PaymentAllocationRequest>
            {
                new(FinPaymentAllocationPayableTypes.Supplier, payable.Id, amount)
            },
            null, requesterId, CancellationToken.None);

        return await _service.SubmitAsync(draft.Id, draft.RowVersion, requesterId, CancellationToken.None);
    }

    private string ReadStatus(Guid paymentId)
    {
        using var verification = _database.CreateContext();
        return verification.FinPayments.Single(x => x.Id == paymentId).Status;
    }

    [Fact]
    public async Task Approve_Tier2Payment_IsRejectedForSupervisor()
    {
        var requesterId = FinanceApSeed.AddUserWithRole(_dbContext, null);
        var supervisorId = FinanceApSeed.AddUserWithRole(_dbContext, FinanceApprovalRoles.Supervisor);
        var payment = await CreateSubmittedPaymentAsync(Tier2Amount, requesterId);
        var paymentId = payment.Id;
        var rowVersion = payment.RowVersion;

        Assert.Equal(ApprovalTiers.Tier2, payment.ApprovalTier);

        await Assert.ThrowsAsync<PaymentForbiddenException>(() =>
            _service.ApproveAsync(paymentId, rowVersion, supervisorId, CancellationToken.None));

        Assert.Equal(FinPaymentStatuses.Submitted, ReadStatus(paymentId));
    }

    [Fact]
    public async Task Approve_Tier2Payment_IsAllowedForManager()
    {
        var requesterId = FinanceApSeed.AddUserWithRole(_dbContext, null);
        var managerId = FinanceApSeed.AddUserWithRole(_dbContext, FinanceApprovalRoles.Manager);
        var payment = await CreateSubmittedPaymentAsync(Tier2Amount, requesterId);

        var approved = await _service.ApproveAsync(payment.Id, payment.RowVersion, managerId, CancellationToken.None);

        Assert.Equal(FinPaymentStatuses.Approved, approved.Status);
        Assert.Equal(managerId, approved.ApprovedBy);
    }

    [Fact]
    public async Task Approve_Tier1Payment_IsAllowedForSupervisor()
    {
        var requesterId = FinanceApSeed.AddUserWithRole(_dbContext, null);
        var supervisorId = FinanceApSeed.AddUserWithRole(_dbContext, FinanceApprovalRoles.Supervisor);
        var payment = await CreateSubmittedPaymentAsync(Tier1Amount, requesterId);

        Assert.Equal(ApprovalTiers.Tier1, payment.ApprovalTier);

        var approved = await _service.ApproveAsync(payment.Id, payment.RowVersion, supervisorId, CancellationToken.None);

        Assert.Equal(FinPaymentStatuses.Approved, approved.Status);
    }

    [Fact]
    public async Task Approve_Tier1Payment_IsRejectedForUserWithoutFinanceApprovalRole()
    {
        var requesterId = FinanceApSeed.AddUserWithRole(_dbContext, null);
        var outsiderId = FinanceApSeed.AddUserWithRole(_dbContext, null);
        var payment = await CreateSubmittedPaymentAsync(Tier1Amount, requesterId);
        var paymentId = payment.Id;
        var rowVersion = payment.RowVersion;

        await Assert.ThrowsAsync<PaymentForbiddenException>(() =>
            _service.ApproveAsync(paymentId, rowVersion, outsiderId, CancellationToken.None));

        Assert.Equal(FinPaymentStatuses.Submitted, ReadStatus(paymentId));
    }

    [Fact]
    public async Task Approve_ByRequester_IsStillRejectedAsSelfApproval_EvenForManager()
    {
        var managerRequesterId = FinanceApSeed.AddUserWithRole(_dbContext, FinanceApprovalRoles.Manager);
        var payment = await CreateSubmittedPaymentAsync(Tier1Amount, managerRequesterId);
        var paymentId = payment.Id;
        var rowVersion = payment.RowVersion;

        await Assert.ThrowsAsync<PaymentValidationException>(() =>
            _service.ApproveAsync(paymentId, rowVersion, managerRequesterId, CancellationToken.None));

        Assert.Equal(FinPaymentStatuses.Submitted, ReadStatus(paymentId));
    }
}
