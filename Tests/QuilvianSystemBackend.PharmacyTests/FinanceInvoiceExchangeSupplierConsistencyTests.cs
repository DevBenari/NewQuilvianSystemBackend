using Microsoft.AspNetCore.Http;
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
/// BE-FIN-121 — Tukar Faktur harus konsisten dengan dokumen yang dirujuknya.
///
/// 1. Supplier Tukar Faktur harus sama dengan supplier PO, baik PO diberikan langsung maupun
///    lewat Tanda Terima Barang (AP-UAT-09).
/// 2. Tanda Terima Barang yang sudah dibatalkan tidak boleh dijadikan dasar Tukar Faktur.
/// 3. Sebaliknya, GR yang sudah dirujuk Tukar Faktur tidak boleh dibatalkan.
/// 4. Tukar Faktur tanpa PO dan tanpa GR tetap sah (FIN-DEC-051).
///
/// Perilaku penguncian (dua permintaan bersamaan) tidak dibuktikan di sini: basis data uji hanya
/// meniru bentuk fungsi kunci PostgreSQL, bukan perilakunya.
/// </summary>
public class FinanceInvoiceExchangeSupplierConsistencyTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceInvoiceExchangeService _invoiceExchangeService;
    private readonly FinanceGoodsReceiptService _goodsReceiptService;
    private readonly MstSupplier _supplierA;
    private readonly MstSupplier _supplierB;
    private readonly Guid _actorUserId = Guid.NewGuid();

    private static readonly DateOnly InvoiceDate = new(2026, 10, 8);
    private static readonly DateOnly ReceivedDate = new(2026, 10, 10);

    public FinanceInvoiceExchangeSupplierConsistencyTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        _invoiceExchangeService = new FinanceInvoiceExchangeService(_dbContext, loggerService);
        _goodsReceiptService = new FinanceGoodsReceiptService(_dbContext, loggerService);

        _supplierA = FinanceApSeed.AddSupplier(_dbContext, "SUP-A", paymentTermDays: 30);
        _supplierB = FinanceApSeed.AddSupplier(_dbContext, "SUP-B", paymentTermDays: 30);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    private Task<FinInvoiceExchange> CreateExchangeAsync(Guid supplierId, Guid? purchaseOrderId, Guid? goodsReceiptId) =>
        _invoiceExchangeService.CreateAsync(
            supplierId, purchaseOrderId, goodsReceiptId, $"INV-{Guid.NewGuid():N}",
            InvoiceDate, ReceivedDate, _actorUserId, CancellationToken.None);

    [Fact]
    public async Task Create_RejectsSupplierDifferentFromReferencedPurchaseOrder()
    {
        var purchaseOrderOfA = FinanceApSeed.AddPurchaseOrder(_dbContext, _supplierA);

        var exception = await Assert.ThrowsAsync<PurchasingValidationException>(() =>
            CreateExchangeAsync(_supplierB.Id, purchaseOrderOfA.Id, null));

        Assert.Contains("harus sama dengan supplier Purchase Order", exception.Message);
        Assert.Empty(_dbContext.FinInvoiceExchanges);
    }

    [Fact]
    public async Task Create_RejectsSupplierDifferentFromPurchaseOrderOfReferencedGoodsReceipt()
    {
        var purchaseOrderOfA = FinanceApSeed.AddPurchaseOrder(_dbContext, _supplierA);
        var goodsReceipt = FinanceApSeed.AddGoodsReceipt(_dbContext, purchaseOrderOfA);

        // PO tidak diberikan; supplier PO dicari lewat GR.
        await Assert.ThrowsAsync<PurchasingValidationException>(() =>
            CreateExchangeAsync(_supplierB.Id, null, goodsReceipt.Id));

        Assert.Empty(_dbContext.FinInvoiceExchanges);
    }

    [Fact]
    public async Task Create_AcceptsMatchingSupplier_ThroughPurchaseOrderAndGoodsReceipt()
    {
        var purchaseOrderOfA = FinanceApSeed.AddPurchaseOrder(_dbContext, _supplierA);
        var goodsReceipt = FinanceApSeed.AddGoodsReceipt(_dbContext, purchaseOrderOfA);

        var exchange = await CreateExchangeAsync(_supplierA.Id, purchaseOrderOfA.Id, goodsReceipt.Id);

        Assert.Equal(FinInvoiceExchangeStatuses.Received, exchange.Status);
        Assert.Equal(_supplierA.Id, exchange.SupplierId);
        // Diterima 10 Oktober 2026 + termin 30 hari = 9 November 2026.
        Assert.Equal(new DateOnly(2026, 11, 9), exchange.EstimatedDueDate);
    }

    [Fact]
    public async Task Create_StillAllowsInvoiceExchangeWithoutPurchaseOrderOrGoodsReceipt()
    {
        var exchange = await CreateExchangeAsync(_supplierA.Id, null, null);

        Assert.Equal(FinInvoiceExchangeStatuses.Received, exchange.Status);
        Assert.Null(exchange.PurchaseOrderId);
        Assert.Null(exchange.GoodsReceiptId);
    }

    [Fact]
    public async Task Create_RejectsCancelledGoodsReceipt()
    {
        var purchaseOrderOfA = FinanceApSeed.AddPurchaseOrder(_dbContext, _supplierA);
        var goodsReceipt = FinanceApSeed.AddGoodsReceipt(_dbContext, purchaseOrderOfA);
        var goodsReceiptId = goodsReceipt.Id;
        var rowVersion = goodsReceipt.RowVersion;

        await _goodsReceiptService.CancelAsync(goodsReceiptId, rowVersion, _actorUserId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<PurchasingValidationException>(() =>
            CreateExchangeAsync(_supplierA.Id, purchaseOrderOfA.Id, goodsReceiptId));

        Assert.Contains("sudah dibatalkan", exception.Message);
        Assert.Empty(_dbContext.FinInvoiceExchanges);
    }

    [Fact]
    public async Task CancelGoodsReceipt_IsRejectedOnceAnInvoiceExchangeReferencesIt()
    {
        var purchaseOrderOfA = FinanceApSeed.AddPurchaseOrder(_dbContext, _supplierA);
        var goodsReceipt = FinanceApSeed.AddGoodsReceipt(_dbContext, purchaseOrderOfA);
        var goodsReceiptId = goodsReceipt.Id;
        var rowVersion = goodsReceipt.RowVersion;

        await CreateExchangeAsync(_supplierA.Id, purchaseOrderOfA.Id, goodsReceiptId);

        var exception = await Assert.ThrowsAsync<PurchasingValidationException>(() =>
            _goodsReceiptService.CancelAsync(goodsReceiptId, rowVersion, _actorUserId, CancellationToken.None));

        Assert.Contains("sudah menjadi Tukar Faktur", exception.Message);

        using var verification = _database.CreateContext();
        Assert.Equal(FinGoodsReceiptStatuses.Received, verification.FinGoodsReceipts.Single(x => x.Id == goodsReceiptId).Status);
    }
}
