using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Guard siklus hidup AP (P05/P06): duplikat nomor faktur (AC-11), koreksi GR/TF dengan pemeriksaan
/// dependensi (AC-17), penutupan PO yang tidak lolos pada himpunan kosong (AC-16), dan pengikatan kunci
/// idempotensi ke aksi/dokumen (AC-18). Perilaku kunci bersamaan di PostgreSQL TIDAK dibuktikan di sini:
/// basis data uji hanya meniru bentuk fungsi kunci. Ditulis untuk dijalankan pihak berwenang — belum
/// pernah dijalankan pada task yang membuatnya.
/// </summary>
public class FinanceApLifecycleGuardTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceInvoiceExchangeService _exchangeService;
    private readonly FinanceGoodsReceiptService _goodsReceiptService;
    private readonly FinancePurchaseOrderService _purchaseOrderService;
    private readonly PurchasingIdempotencyService _idempotencyService;
    private readonly MstSupplier _supplier;
    private readonly Guid _actorUserId = Guid.NewGuid();

    private static readonly DateOnly InvoiceDate = new(2026, 10, 8);
    private static readonly DateOnly ReceivedDate = new(2026, 10, 10);

    public FinanceApLifecycleGuardTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        _exchangeService = new FinanceInvoiceExchangeService(_dbContext, loggerService);
        _goodsReceiptService = new FinanceGoodsReceiptService(_dbContext, loggerService);
        _purchaseOrderService = new FinancePurchaseOrderService(
            _dbContext, loggerService, FinanceApSeed.BuildApprovalAuthorization(_dbContext));
        _idempotencyService = new PurchasingIdempotencyService(_dbContext);

        _supplier = FinanceApSeed.AddSupplier(_dbContext, "SUP-LIFECYCLE", paymentTermDays: 30);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    private Task<FinInvoiceExchange> CreateExchangeAsync(string invoiceNumber, Guid? purchaseOrderId = null, Guid? goodsReceiptId = null) =>
        _exchangeService.CreateAsync(
            _supplier.Id, purchaseOrderId, goodsReceiptId, invoiceNumber,
            InvoiceDate, ReceivedDate, _actorUserId, CancellationToken.None);

    private FinPurchaseOrder AddPurchaseOrderWithItem(decimal quantity, out FinPurchaseOrderItem item)
    {
        var purchaseOrder = FinanceApSeed.AddPurchaseOrder(_dbContext, _supplier);
        item = new FinPurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = purchaseOrder.Id,
            ProductCategory = "OBAT",
            ProductName = "Parasetamol 500 mg",
            Unit = "BOX",
            Quantity = quantity,
            UnitPrice = 10_000m,
            LineTotal = quantity * 10_000m
        };
        _dbContext.FinPurchaseOrderItems.Add(item);
        _dbContext.SaveChanges();
        return purchaseOrder;
    }

    private Task<FinGoodsReceipt> CreateGoodsReceiptAsync(FinPurchaseOrder purchaseOrder, FinPurchaseOrderItem item, decimal quantity, string batch) =>
        _goodsReceiptService.CreateAsync(
            purchaseOrder.Id, ReceivedDate,
            [new GoodsReceiptItemRequest(item.Id, quantity, null, batch, null)],
            _actorUserId, CancellationToken.None);

    private FinPurchasingInvoice AddInvoice(FinInvoiceExchange exchange, string status)
    {
        var invoice = new FinPurchasingInvoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = $"PI-TEST-{Guid.NewGuid():N}",
            InvoiceExchangeId = exchange.Id,
            SupplierId = exchange.SupplierId,
            SubtotalAmount = 100_000m,
            TotalAmount = 100_000m,
            Status = status,
            ApprovalTier = "TIER_1",
            RequestedByUserId = _actorUserId,
            RequestedAt = DateTimeOffset.UtcNow
        };
        _dbContext.FinPurchasingInvoices.Add(invoice);
        _dbContext.SaveChanges();
        return invoice;
    }

    // -------------------------------------------------------------------------------------
    // AC-11: nomor faktur unik per supplier
    // -------------------------------------------------------------------------------------

    [Fact]
    public async Task Exchange_Create_RejectsSameInvoiceNumberWhileOtherExchangeIsActive()
    {
        await CreateExchangeAsync("INV-DUP-001");

        await Assert.ThrowsAsync<PurchasingConflictException>(() => CreateExchangeAsync("  inv-dup-001 "));
        Assert.Single(_dbContext.FinInvoiceExchanges);
    }

    [Fact]
    public async Task Exchange_Create_RejectsInvoiceNumberAlreadyRecognizedAsPayable()
    {
        FinanceApSeed.AddPayable(_dbContext, _supplier, "INV-MANUAL-777", 500_000m);

        await Assert.ThrowsAsync<PurchasingConflictException>(() => CreateExchangeAsync("INV-MANUAL-777"));
    }

    [Fact]
    public async Task Exchange_Create_AllowsSameInvoiceNumberAfterPreviousExchangeIsCancelled()
    {
        var first = await CreateExchangeAsync("INV-REBUILD-1");
        await _exchangeService.CancelAsync(first.Id, first.RowVersion, _actorUserId, CancellationToken.None);

        var second = await CreateExchangeAsync("INV-REBUILD-1");

        Assert.Equal(FinInvoiceExchangeStatuses.Received, second.Status);
    }

    // -------------------------------------------------------------------------------------
    // Koreksi dan pembatalan Tukar Faktur (dependensi invoice aktif)
    // -------------------------------------------------------------------------------------

    [Fact]
    public async Task Exchange_Update_ChangesFieldsAndRecomputesEstimatedDueDateOnlyWhenReceivedDateChanges()
    {
        var exchange = await CreateExchangeAsync("INV-EDIT-1");
        var originalEstimate = exchange.EstimatedDueDate;

        var sameReceived = await _exchangeService.UpdateAsync(
            exchange.Id, exchange.RowVersion, "INV-EDIT-1B", InvoiceDate.AddDays(1), ReceivedDate, _actorUserId, CancellationToken.None);
        Assert.Equal("INV-EDIT-1B", sameReceived.SupplierInvoiceNumber);
        Assert.Equal(originalEstimate, sameReceived.EstimatedDueDate);

        var newReceived = ReceivedDate.AddDays(5);
        var movedReceived = await _exchangeService.UpdateAsync(
            exchange.Id, sameReceived.RowVersion, "INV-EDIT-1B", InvoiceDate, newReceived, _actorUserId, CancellationToken.None);
        Assert.Equal(newReceived.AddDays(30), movedReceived.EstimatedDueDate);
    }

    [Fact]
    public async Task Exchange_Update_RejectsStaleRowVersion()
    {
        var exchange = await CreateExchangeAsync("INV-EDIT-2");

        await Assert.ThrowsAsync<PurchasingConflictException>(() => _exchangeService.UpdateAsync(
            exchange.Id, Guid.NewGuid(), "INV-EDIT-2", InvoiceDate, ReceivedDate, _actorUserId, CancellationToken.None));
    }

    [Fact]
    public async Task Exchange_UpdateAndCancel_AreRejectedWhileDraftInvoiceExists()
    {
        var exchange = await CreateExchangeAsync("INV-EDIT-3");
        AddInvoice(exchange, FinPurchasingInvoiceStatuses.Draft);

        await Assert.ThrowsAsync<PurchasingValidationException>(() => _exchangeService.UpdateAsync(
            exchange.Id, exchange.RowVersion, "INV-EDIT-3X", InvoiceDate, ReceivedDate, _actorUserId, CancellationToken.None));
        await Assert.ThrowsAsync<PurchasingValidationException>(() => _exchangeService.CancelAsync(
            exchange.Id, exchange.RowVersion, _actorUserId, CancellationToken.None));
    }

    [Fact]
    public async Task Exchange_Cancel_IsAllowedWhenOnlyCancelledInvoiceRemains()
    {
        var exchange = await CreateExchangeAsync("INV-EDIT-4");
        AddInvoice(exchange, FinPurchasingInvoiceStatuses.Cancelled);

        var cancelled = await _exchangeService.CancelAsync(exchange.Id, exchange.RowVersion, _actorUserId, CancellationToken.None);

        Assert.Equal(FinInvoiceExchangeStatuses.Cancelled, cancelled.Status);
    }

    // -------------------------------------------------------------------------------------
    // AC-01/AC-02/AC-17: koreksi GR
    // -------------------------------------------------------------------------------------

    [Fact]
    public async Task GoodsReceipt_Update_ReplacesItemsKeepsHistoryAndRecomputesPurchaseOrderStatus()
    {
        var purchaseOrder = AddPurchaseOrderWithItem(100m, out var poItem);
        var goodsReceipt = await CreateGoodsReceiptAsync(purchaseOrder, poItem, 40m, "B1");

        await _goodsReceiptService.UpdateAsync(
            goodsReceipt.Id, goodsReceipt.RowVersion, ReceivedDate,
            [
                new GoodsReceiptItemRequest(poItem.Id, 60m, null, "B1", null),
                new GoodsReceiptItemRequest(poItem.Id, 40m, null, "B2", null)
            ],
            _actorUserId, CancellationToken.None);

        var detail = await _goodsReceiptService.GetByIdAsync(goodsReceipt.Id, CancellationToken.None);
        Assert.NotNull(detail);
        Assert.Equal(2, detail!.Items.Count);
        Assert.Equal(100m, detail.Items.Sum(x => x.ReceivedQuantity));

        // Baris lama dihapus lunak, bukan dibuang.
        Assert.Equal(1, await _dbContext.FinGoodsReceiptItems.CountAsync(x => x.IsDelete));

        var reloaded = await _dbContext.FinPurchaseOrders.AsNoTracking().SingleAsync(x => x.Id == purchaseOrder.Id);
        Assert.Equal(FinPurchaseOrderStatuses.FullyReceived, reloaded.Status);
    }

    [Fact]
    public async Task GoodsReceipt_Update_RejectsAccumulationBeyondOrderedQuantity()
    {
        var purchaseOrder = AddPurchaseOrderWithItem(100m, out var poItem);
        var first = await CreateGoodsReceiptAsync(purchaseOrder, poItem, 60m, "B1");
        var second = await CreateGoodsReceiptAsync(purchaseOrder, poItem, 30m, "B2");

        // GR kedua dikoreksi menjadi 50 → 60 + 50 = 110 > 100.
        await Assert.ThrowsAsync<PurchasingValidationException>(() => _goodsReceiptService.UpdateAsync(
            second.Id, second.RowVersion, ReceivedDate,
            [new GoodsReceiptItemRequest(poItem.Id, 50m, null, "B2", null)],
            _actorUserId, CancellationToken.None));

        Assert.NotEqual(Guid.Empty, first.Id);
    }

    [Fact]
    public async Task GoodsReceipt_Update_IsRejectedWhileActiveExchangeReferencesIt()
    {
        var purchaseOrder = AddPurchaseOrderWithItem(100m, out var poItem);
        var goodsReceipt = await CreateGoodsReceiptAsync(purchaseOrder, poItem, 40m, "B1");
        await CreateExchangeAsync("INV-GR-REF", purchaseOrder.Id, goodsReceipt.Id);

        await Assert.ThrowsAsync<PurchasingValidationException>(() => _goodsReceiptService.UpdateAsync(
            goodsReceipt.Id, goodsReceipt.RowVersion, ReceivedDate,
            [new GoodsReceiptItemRequest(poItem.Id, 30m, null, "B1", null)],
            _actorUserId, CancellationToken.None));
    }

    [Fact]
    public async Task GoodsReceipt_Cancel_IsAllowedAfterReferencingExchangeIsCancelled()
    {
        var purchaseOrder = AddPurchaseOrderWithItem(100m, out var poItem);
        var goodsReceipt = await CreateGoodsReceiptAsync(purchaseOrder, poItem, 40m, "B1");
        var exchange = await CreateExchangeAsync("INV-GR-CANCEL", purchaseOrder.Id, goodsReceipt.Id);

        await Assert.ThrowsAsync<PurchasingValidationException>(() => _goodsReceiptService.CancelAsync(
            goodsReceipt.Id, goodsReceipt.RowVersion, _actorUserId, CancellationToken.None));

        await _exchangeService.CancelAsync(exchange.Id, exchange.RowVersion, _actorUserId, CancellationToken.None);
        var cancelled = await _goodsReceiptService.CancelAsync(
            goodsReceipt.Id, goodsReceipt.RowVersion, _actorUserId, CancellationToken.None);

        Assert.Equal(FinGoodsReceiptStatuses.Cancelled, cancelled.Status);
    }

    // -------------------------------------------------------------------------------------
    // AC-16: penutupan PO
    // -------------------------------------------------------------------------------------

    [Fact]
    public async Task PurchaseOrder_Close_IsRejectedWhenGoodsReceiptHasNoExchange()
    {
        var purchaseOrder = AddPurchaseOrderWithItem(100m, out var poItem);
        await CreateGoodsReceiptAsync(purchaseOrder, poItem, 100m, "B1");
        var reloaded = await _dbContext.FinPurchaseOrders.SingleAsync(x => x.Id == purchaseOrder.Id);
        Assert.Equal(FinPurchaseOrderStatuses.FullyReceived, reloaded.Status);

        var exception = await Assert.ThrowsAsync<PurchasingValidationException>(() => _purchaseOrderService.CloseAsync(
            purchaseOrder.Id, reloaded.RowVersion, _actorUserId, CancellationToken.None));

        Assert.Contains("belum memiliki Tukar Faktur", exception.Message);
    }

    [Fact]
    public async Task PurchaseOrder_Close_SucceedsWhenEveryGoodsReceiptIsCoveredByApprovedInvoice_AndDoesNotTouchPayables()
    {
        var purchaseOrder = AddPurchaseOrderWithItem(100m, out var poItem);
        var goodsReceipt = await CreateGoodsReceiptAsync(purchaseOrder, poItem, 100m, "B1");
        var exchange = await CreateExchangeAsync("INV-CLOSE-1", purchaseOrder.Id, goodsReceipt.Id);
        AddInvoice(exchange, FinPurchasingInvoiceStatuses.Approved);

        var trackedExchange = await _dbContext.FinInvoiceExchanges.SingleAsync(x => x.Id == exchange.Id);
        trackedExchange.Status = FinInvoiceExchangeStatuses.LinkedToInvoice;
        await _dbContext.SaveChangesAsync();

        var reloaded = await _dbContext.FinPurchaseOrders.SingleAsync(x => x.Id == purchaseOrder.Id);
        var closed = await _purchaseOrderService.CloseAsync(purchaseOrder.Id, reloaded.RowVersion, _actorUserId, CancellationToken.None);

        Assert.Equal(FinPurchaseOrderStatuses.Closed, closed.Status);
        Assert.Empty(_dbContext.FinSupplierPayables);
    }

    // -------------------------------------------------------------------------------------
    // AC-18: kunci idempotensi terikat aksi dan dokumen
    // -------------------------------------------------------------------------------------

    [Fact]
    public async Task Idempotency_ReplaysSameActionAndRejectsKeyReusedForOtherActionOrDocument()
    {
        var key = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        await _idempotencyService.SaveAsync(
            key, FinPurchasingIdempotencyEntityTypes.GoodsReceipt, FinPurchasingIdempotencyActions.Cancel,
            documentId, 200, new { Success = true }, CancellationToken.None);

        var replay = await _idempotencyService.TryReplayAsync(
            key, FinPurchasingIdempotencyEntityTypes.GoodsReceipt, FinPurchasingIdempotencyActions.Cancel, documentId, CancellationToken.None);
        Assert.NotNull(replay);
        Assert.Equal(200, replay!.StatusCode);
        Assert.Contains("\"success\":true", replay.ResponseBody);

        var otherAction = await _idempotencyService.TryReplayAsync(
            key, FinPurchasingIdempotencyEntityTypes.GoodsReceipt, FinPurchasingIdempotencyActions.Create, null, CancellationToken.None);
        Assert.Equal(409, otherAction!.StatusCode);

        var otherEntity = await _idempotencyService.TryReplayAsync(
            key, FinPurchasingIdempotencyEntityTypes.InvoiceExchange, FinPurchasingIdempotencyActions.Cancel, documentId, CancellationToken.None);
        Assert.Equal(409, otherEntity!.StatusCode);

        var otherDocument = await _idempotencyService.TryReplayAsync(
            key, FinPurchasingIdempotencyEntityTypes.GoodsReceipt, FinPurchasingIdempotencyActions.Cancel, Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(409, otherDocument!.StatusCode);
    }

    [Fact]
    public async Task Idempotency_UnknownKeyIsNotReplayed()
    {
        var replay = await _idempotencyService.TryReplayAsync(
            Guid.NewGuid(), FinPurchasingIdempotencyEntityTypes.GoodsReceipt, FinPurchasingIdempotencyActions.Create, null, CancellationToken.None);

        Assert.Null(replay);
    }
}
