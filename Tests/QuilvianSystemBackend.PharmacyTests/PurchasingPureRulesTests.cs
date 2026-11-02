using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Aturan murni Purchasing/AP tanpa basis data: kesiapan penutupan PO (AC-16) dan perencanaan alokasi
/// invoice ke baris GR (AC-04, AC-06; P04 belum aktif). Ditulis untuk dijalankan pihak berwenang —
/// belum pernah dijalankan pada task yang membuatnya.
/// </summary>
public class PurchasingPureRulesTests
{
    // -------------------------------------------------------------------------------------
    // PurchaseOrderClosurePolicy
    // -------------------------------------------------------------------------------------

    private static ClosureGoodsReceipt Gr(string number, string status = FinGoodsReceiptStatuses.Received) =>
        new(Guid.NewGuid(), number, status);

    [Fact]
    public void Closure_RejectsPurchaseOrderNotFullyReceived()
    {
        var result = PurchaseOrderClosurePolicy.Assess(FinPurchaseOrderStatuses.PartiallyReceived, [], [], []);

        Assert.False(result.CanClose);
        Assert.Contains(result.BlockingReasons, x => x.Contains("FULLY_RECEIVED"));
    }

    [Fact]
    public void Closure_RejectsAlreadyClosedPurchaseOrder()
    {
        var result = PurchaseOrderClosurePolicy.Assess(FinPurchaseOrderStatuses.Closed, [], [], []);

        Assert.False(result.CanClose);
        Assert.Contains(result.BlockingReasons, x => x.Contains("sudah ditutup"));
    }

    [Fact]
    public void Closure_DoesNotPassOnEmptyExchangeSet_WhenGoodsReceiptExists()
    {
        // AC-16: tidak ada Tukar Faktur sama sekali bukan berarti "semua selesai".
        var goodsReceipt = Gr("GR-1");

        var result = PurchaseOrderClosurePolicy.Assess(FinPurchaseOrderStatuses.FullyReceived, [goodsReceipt], [], []);

        Assert.False(result.CanClose);
        Assert.Contains(result.BlockingReasons, x => x.Contains("GR-1") && x.Contains("belum memiliki Tukar Faktur"));
    }

    [Fact]
    public void Closure_RejectsPurchaseOrderWithoutAnyActiveGoodsReceipt()
    {
        var cancelled = Gr("GR-X", FinGoodsReceiptStatuses.Cancelled);

        var result = PurchaseOrderClosurePolicy.Assess(FinPurchaseOrderStatuses.FullyReceived, [cancelled], [], []);

        Assert.False(result.CanClose);
        Assert.Contains(result.BlockingReasons, x => x.Contains("tidak memiliki Tanda Terima Barang aktif"));
    }

    [Fact]
    public void Closure_RejectsExchangeStillReceived()
    {
        var goodsReceipt = Gr("GR-1");
        var exchange = new ClosureExchange(Guid.NewGuid(), "TF-1", goodsReceipt.Id, FinInvoiceExchangeStatuses.Received);

        var result = PurchaseOrderClosurePolicy.Assess(FinPurchaseOrderStatuses.FullyReceived, [goodsReceipt], [exchange], []);

        Assert.False(result.CanClose);
        Assert.Contains(result.BlockingReasons, x => x.Contains("TF-1") && x.Contains("belum menjadi Purchasing Invoice"));
    }

    [Fact]
    public void Closure_AcceptsWhenEveryActiveGoodsReceiptIsCoveredByApprovedInvoice()
    {
        var first = Gr("GR-1");
        var second = Gr("GR-2");
        var cancelled = Gr("GR-3", FinGoodsReceiptStatuses.Cancelled);
        var exchangeOne = new ClosureExchange(Guid.NewGuid(), "TF-1", first.Id, FinInvoiceExchangeStatuses.LinkedToInvoice);
        var exchangeTwo = new ClosureExchange(Guid.NewGuid(), "TF-2", second.Id, FinInvoiceExchangeStatuses.LinkedToInvoice);
        var cancelledExchange = new ClosureExchange(Guid.NewGuid(), "TF-OLD", first.Id, FinInvoiceExchangeStatuses.Cancelled);
        var invoices = new[]
        {
            new ClosureInvoice(exchangeOne.Id, "PI-1", FinPurchasingInvoiceStatuses.Approved),
            new ClosureInvoice(exchangeTwo.Id, "PI-2", FinPurchasingInvoiceStatuses.Approved)
        };

        var result = PurchaseOrderClosurePolicy.Assess(
            FinPurchaseOrderStatuses.FullyReceived,
            [first, second, cancelled],
            [exchangeOne, exchangeTwo, cancelledExchange],
            invoices);

        Assert.True(result.CanClose);
        Assert.Empty(result.BlockingReasons);
    }

    [Fact]
    public void Closure_RejectsLinkedExchangeWithoutApprovedInvoice()
    {
        var goodsReceipt = Gr("GR-1");
        var exchange = new ClosureExchange(Guid.NewGuid(), "TF-1", goodsReceipt.Id, FinInvoiceExchangeStatuses.LinkedToInvoice);

        var result = PurchaseOrderClosurePolicy.Assess(FinPurchaseOrderStatuses.FullyReceived, [goodsReceipt], [exchange], []);

        Assert.False(result.CanClose);
        Assert.Contains(result.BlockingReasons, x => x.Contains("tidak memiliki Purchasing Invoice yang disetujui"));
    }

    // -------------------------------------------------------------------------------------
    // PurchasingInvoiceAllocationPlanner
    // -------------------------------------------------------------------------------------

    private static AllocationCandidateGoodsReceiptItem Candidate(
        Guid goodsReceiptId, Guid purchaseOrderId, decimal received, decimal invoiced = 0m,
        string status = FinGoodsReceiptStatuses.Received) =>
        new(Guid.NewGuid(), goodsReceiptId, purchaseOrderId, Guid.NewGuid(), status, received, invoiced);

    [Fact]
    public void Allocation_SplitsOneInvoiceItemAcrossTwoGoodsReceiptsOfSamePurchaseOrder()
    {
        var purchaseOrderId = Guid.NewGuid();
        var firstGr = Guid.NewGuid();
        var secondGr = Guid.NewGuid();
        var first = Candidate(firstGr, purchaseOrderId, received: 60m);
        var second = Candidate(secondGr, purchaseOrderId, received: 40m);
        var invoiceItemId = Guid.NewGuid();

        var plan = PurchasingInvoiceAllocationPlanner.Plan(
            [firstGr, secondGr],
            [first, second],
            [
                new AllocationRequest(invoiceItemId, 100m, 10_000m, first.GoodsReceiptItemId, 60m),
                new AllocationRequest(invoiceItemId, 100m, 10_000m, second.GoodsReceiptItemId, 40m)
            ]);

        Assert.True(plan.IsValid);
        Assert.Equal(2, plan.Allocations.Count);
        Assert.Equal(1_000_000m, plan.Allocations.Sum(x => x.Amount));
    }

    [Fact]
    public void Allocation_RejectsQuantityBeyondUninvoicedRemainder()
    {
        var purchaseOrderId = Guid.NewGuid();
        var goodsReceiptId = Guid.NewGuid();
        var candidate = Candidate(goodsReceiptId, purchaseOrderId, received: 100m, invoiced: 70m);
        var invoiceItemId = Guid.NewGuid();

        var plan = PurchasingInvoiceAllocationPlanner.Plan(
            [goodsReceiptId], [candidate],
            [new AllocationRequest(invoiceItemId, 40m, 5_000m, candidate.GoodsReceiptItemId, 40m)]);

        Assert.False(plan.IsValid);
        Assert.Contains(plan.Errors, x => x.Contains("melebihi sisa yang belum ditagih"));
        Assert.Empty(plan.Allocations);
    }

    [Fact]
    public void Allocation_RejectsGoodsReceiptItemOutsideBundle()
    {
        var purchaseOrderId = Guid.NewGuid();
        var bundleGr = Guid.NewGuid();
        var otherGr = Guid.NewGuid();
        var outside = Candidate(otherGr, purchaseOrderId, received: 10m);
        var invoiceItemId = Guid.NewGuid();

        var plan = PurchasingInvoiceAllocationPlanner.Plan(
            [bundleGr], [outside],
            [new AllocationRequest(invoiceItemId, 10m, 1_000m, outside.GoodsReceiptItemId, 10m)]);

        Assert.False(plan.IsValid);
        Assert.Contains(plan.Errors, x => x.Contains("bukan bagian dari Tanda Terima Barang"));
    }

    [Fact]
    public void Allocation_RejectsBundleSpanningTwoPurchaseOrders()
    {
        var firstGr = Guid.NewGuid();
        var secondGr = Guid.NewGuid();
        var first = Candidate(firstGr, Guid.NewGuid(), received: 5m);
        var second = Candidate(secondGr, Guid.NewGuid(), received: 5m);
        var invoiceItemId = Guid.NewGuid();

        var plan = PurchasingInvoiceAllocationPlanner.Plan(
            [firstGr, secondGr], [first, second],
            [
                new AllocationRequest(invoiceItemId, 10m, 1_000m, first.GoodsReceiptItemId, 5m),
                new AllocationRequest(invoiceItemId, 10m, 1_000m, second.GoodsReceiptItemId, 5m)
            ]);

        Assert.False(plan.IsValid);
        Assert.Contains(plan.Errors, x => x.Contains("satu Purchase Order"));
    }

    [Fact]
    public void Allocation_RejectsInvoiceItemThatIsNotFullyAllocated()
    {
        var purchaseOrderId = Guid.NewGuid();
        var goodsReceiptId = Guid.NewGuid();
        var candidate = Candidate(goodsReceiptId, purchaseOrderId, received: 100m);
        var invoiceItemId = Guid.NewGuid();

        var plan = PurchasingInvoiceAllocationPlanner.Plan(
            [goodsReceiptId], [candidate],
            [new AllocationRequest(invoiceItemId, 100m, 1_000m, candidate.GoodsReceiptItemId, 60m)]);

        Assert.False(plan.IsValid);
        Assert.Contains(plan.Errors, x => x.Contains("harus sama dengan kuantitas"));
    }

    [Fact]
    public void Allocation_RejectsCancelledGoodsReceipt()
    {
        var purchaseOrderId = Guid.NewGuid();
        var goodsReceiptId = Guid.NewGuid();
        var candidate = Candidate(goodsReceiptId, purchaseOrderId, received: 10m, status: FinGoodsReceiptStatuses.Cancelled);
        var invoiceItemId = Guid.NewGuid();

        var plan = PurchasingInvoiceAllocationPlanner.Plan(
            [goodsReceiptId], [candidate],
            [new AllocationRequest(invoiceItemId, 10m, 1_000m, candidate.GoodsReceiptItemId, 10m)]);

        Assert.False(plan.IsValid);
        Assert.Contains(plan.Errors, x => x.Contains("tidak berstatus RECEIVED"));
    }

    [Fact]
    public void Allocation_ValueFollowsInvoiceUnitPrice_SoBoxAndTabletStayEquivalent()
    {
        // AC-06: 1 box = 10 tablet. Rp100.000/box ≡ Rp10.000/tablet — nilai alokasi mengikuti harga satuan
        // baris invoice pada satuan yang sama dengan kuantitas alokasinya.
        var purchaseOrderId = Guid.NewGuid();
        var goodsReceiptId = Guid.NewGuid();
        var candidate = Candidate(goodsReceiptId, purchaseOrderId, received: 20m);

        var perBox = PurchasingInvoiceAllocationPlanner.Plan(
            [goodsReceiptId], [candidate],
            [new AllocationRequest(Guid.NewGuid(), 2m, 100_000m, candidate.GoodsReceiptItemId, 2m)]);
        var perTablet = PurchasingInvoiceAllocationPlanner.Plan(
            [goodsReceiptId], [candidate],
            [new AllocationRequest(Guid.NewGuid(), 20m, 10_000m, candidate.GoodsReceiptItemId, 20m)]);

        Assert.Equal(perBox.Allocations.Sum(x => x.Amount), perTablet.Allocations.Sum(x => x.Amount));
    }
}
