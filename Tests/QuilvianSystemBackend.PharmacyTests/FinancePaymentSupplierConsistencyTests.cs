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
/// BE-FIN-120 — Pembayaran supplier hanya boleh melunasi utang milik penerima pembayaran itu sendiri
/// (AP-UAT-14) dan hanya lewat pembayaran bertipe SUPPLIER (FIN-VAL-096).
///
/// Contoh: utang faktur INV-A-1 milik Supplier A Rp 1.000.000. Pembayaran yang ditujukan ke Supplier B
/// dengan alokasi ke utang itu harus ditolak 422, baik saat draft disusun maupun saat draft diubah.
/// </summary>
public class FinancePaymentSupplierConsistencyTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly FinancePaymentService _service;
    private readonly MstSupplier _supplierA;
    private readonly MstSupplier _supplierB;
    private readonly MstBankAccount _bankAccount;
    private readonly Guid _actorUserId = Guid.NewGuid();

    public FinancePaymentSupplierConsistencyTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

        // Layanan pencatat akuntansi, retur, dan mutasi tidak dipakai oleh jalur yang diuji di sini
        // (susun draft dan ubah draft), jadi tidak perlu dibentuk.
        _service = new FinancePaymentService(
            _dbContext,
            loggerService,
            null!,
            null!,
            null!,
            FinanceApSeed.BuildApprovalAuthorization(_dbContext));

        _supplierA = FinanceApSeed.AddSupplier(_dbContext, "SUP-A");
        _supplierB = FinanceApSeed.AddSupplier(_dbContext, "SUP-B");
        _bankAccount = FinanceApSeed.AddBankAccount(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    private static List<PaymentAllocationRequest> Allocate(FinSupplierPayable payable, decimal amount) =>
        new() { new PaymentAllocationRequest(FinPaymentAllocationPayableTypes.Supplier, payable.Id, amount) };

    [Fact]
    public async Task CreateDraft_RejectsPayableOwnedByAnotherSupplier()
    {
        var payableOfA = FinanceApSeed.AddPayable(_dbContext, _supplierA, "INV-A-1", 1_000_000m);

        var exception = await Assert.ThrowsAsync<PaymentValidationException>(() => _service.CreateDraftAsync(
            FinPaymentTypes.Supplier, _supplierB.Id, _bankAccount.Id, FinPaymentMethods.Transfer, null,
            Allocate(payableOfA, 1_000_000m), null, _actorUserId, CancellationToken.None));

        Assert.Contains("milik supplier lain", exception.Message);
        Assert.Empty(_dbContext.FinPayments);
    }

    [Fact]
    public async Task CreateDraft_AcceptsPayableOwnedByThePayee()
    {
        var payableOfA = FinanceApSeed.AddPayable(_dbContext, _supplierA, "INV-A-1", 1_000_000m);

        var payment = await _service.CreateDraftAsync(
            FinPaymentTypes.Supplier, _supplierA.Id, _bankAccount.Id, FinPaymentMethods.Transfer, null,
            Allocate(payableOfA, 1_000_000m), null, _actorUserId, CancellationToken.None);

        Assert.Equal(FinPaymentStatuses.Draft, payment.Status);
        Assert.Equal(_supplierA.Id, payment.PayeeReferenceId);
        Assert.Equal(1_000_000m, payment.TotalAmount);
    }

    [Fact]
    public async Task CreateDraft_RejectsMedicalServicePaymentAllocatedToSupplierPayable()
    {
        var payableOfA = FinanceApSeed.AddPayable(_dbContext, _supplierA, "INV-A-1", 1_000_000m);

        var exception = await Assert.ThrowsAsync<PaymentBadRequestException>(() => _service.CreateDraftAsync(
            FinPaymentTypes.MedicalService, _supplierA.Id, _bankAccount.Id, FinPaymentMethods.Transfer, null,
            Allocate(payableOfA, 1_000_000m), null, _actorUserId, CancellationToken.None));

        Assert.Contains("Jenis pembayaran tidak cocok", exception.Message);
        Assert.Empty(_dbContext.FinPayments);
    }

    [Fact]
    public async Task UpdateDraft_RejectsPayableOwnedByAnotherSupplier_AndKeepsOriginalAllocation()
    {
        var payableOfA = FinanceApSeed.AddPayable(_dbContext, _supplierA, "INV-A-1", 1_000_000m);
        var payableOfB = FinanceApSeed.AddPayable(_dbContext, _supplierB, "INV-B-1", 2_000_000m);

        var draft = await _service.CreateDraftAsync(
            FinPaymentTypes.Supplier, _supplierA.Id, _bankAccount.Id, FinPaymentMethods.Transfer, null,
            Allocate(payableOfA, 1_000_000m), null, _actorUserId, CancellationToken.None);
        var draftId = draft.Id;
        var rowVersion = draft.RowVersion;

        await Assert.ThrowsAsync<PaymentValidationException>(() => _service.UpdateDraftAsync(
            draftId, rowVersion, _bankAccount.Id, FinPaymentMethods.Transfer, null,
            Allocate(payableOfB, 2_000_000m), null, _actorUserId, CancellationToken.None));

        // Dibaca lewat konteks baru supaya terbukti dari basis data, bukan dari memori konteks lama.
        using var verification = _database.CreateContext();
        var allocations = verification.FinPaymentAllocations.Where(x => x.PaymentId == draftId).ToList();
        Assert.Single(allocations);
        Assert.Equal(payableOfA.Id, allocations[0].SupplierPayableId);
    }
}
