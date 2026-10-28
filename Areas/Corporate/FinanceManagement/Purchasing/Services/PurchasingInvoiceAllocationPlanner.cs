using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>Baris GR yang dapat ditagihkan, dengan kuantitas yang sudah dialokasikan invoice lain yang berlaku.</summary>
public sealed record AllocationCandidateGoodsReceiptItem(
    Guid GoodsReceiptItemId,
    Guid GoodsReceiptId,
    Guid PurchaseOrderId,
    Guid PurchaseOrderItemId,
    string GoodsReceiptStatus,
    decimal ReceivedQuantity,
    decimal InvoicedQuantity);

/// <summary>Satu permintaan alokasi: baris invoice ← baris GR sebanyak <see cref="Quantity"/>.</summary>
public sealed record AllocationRequest(
    Guid InvoiceItemId,
    decimal InvoiceItemQuantity,
    decimal InvoiceItemUnitPrice,
    Guid GoodsReceiptItemId,
    decimal Quantity);

public sealed record PlannedAllocation(Guid InvoiceItemId, Guid GoodsReceiptItemId, decimal Quantity, decimal Amount);

public sealed record AllocationPlan(bool IsValid, IReadOnlyList<string> Errors, IReadOnlyList<PlannedAllocation> Allocations);

/// <summary>
/// P04 — NOT ACTIVATED. Aturan alokasi baris invoice ke baris GR sebagai fungsi murni: tidak membaca atau
/// menulis basis data dan belum dipanggil oleh alur mana pun. Ia ada supaya aturan kuantitas, cakupan, dan
/// pencocokan baris dapat diuji dan ditinjau SEBELUM schema alokasi tersedia, dan supaya penulis service
/// aktivasi nanti tidak menemukan ulang aturan yang berbeda.
///
/// Yang ditegakkan:
///  - pencocokan HANYA lewat ID baris (bukan nama produk);
///  - baris GR harus berasal dari GR dalam bundel Tukar Faktur dan berstatus RECEIVED;
///  - seluruh baris GR berasal dari satu PO (cakupan awal: satu supplier, satu PO, beberapa GR aktif —
///    batas desain awal, bukan kebijakan lintas-PO yang sudah disahkan);
///  - kuantitas > 0, tidak melebihi sisa yang belum ditagih (diterima − dialokasikan invoice lain −
///    yang sudah dipakai permintaan ini);
///  - tiap baris invoice teralokasi PENUH: jumlah alokasinya sama dengan kuantitas baris itu;
///  - satu pasangan baris invoice–baris GR hanya sekali.
///
/// Yang TIDAK ditegakkan di sini dan wajib oleh service aktivasi: kunci per PO
/// (<c>FIN_PURCHASE_ORDER_{id}</c>) dan pembacaan ulang <c>InvoicedQuantity</c> di dalam kunci itu. Tanpa
/// keduanya fungsi ini tidak mencegah dua invoice bersamaan menagih sisa yang sama.
/// </summary>
public static class PurchasingInvoiceAllocationPlanner
{
    public static AllocationPlan Plan(
        IReadOnlyCollection<Guid> bundleGoodsReceiptIds,
        IReadOnlyCollection<AllocationCandidateGoodsReceiptItem> candidates,
        IReadOnlyCollection<AllocationRequest> requests)
    {
        var errors = new List<string>();
        var planned = new List<PlannedAllocation>();

        if (requests.Count == 0)
            errors.Add("Alokasi invoice wajib memuat minimal satu baris.");

        var candidateById = new Dictionary<Guid, AllocationCandidateGoodsReceiptItem>();
        foreach (var candidate in candidates)
            candidateById.TryAdd(candidate.GoodsReceiptItemId, candidate);

        var purchaseOrderIds = candidateById.Values
            .Where(x => bundleGoodsReceiptIds.Contains(x.GoodsReceiptId))
            .Select(x => x.PurchaseOrderId)
            .Distinct()
            .ToList();
        if (purchaseOrderIds.Count > 1)
            errors.Add("Seluruh Tanda Terima Barang dalam satu invoice harus berasal dari satu Purchase Order.");

        var consumedInThisRequest = new Dictionary<Guid, decimal>();
        var seenPairs = new HashSet<(Guid InvoiceItemId, Guid GoodsReceiptItemId)>();

        foreach (var request in requests)
        {
            if (request.Quantity <= 0)
            {
                errors.Add("Kuantitas alokasi harus lebih dari nol.");
                continue;
            }

            if (!seenPairs.Add((request.InvoiceItemId, request.GoodsReceiptItemId)))
            {
                errors.Add("Pasangan baris invoice dan baris Tanda Terima Barang yang sama tidak boleh dialokasikan dua kali.");
                continue;
            }

            if (!candidateById.TryGetValue(request.GoodsReceiptItemId, out var candidate)
                || !bundleGoodsReceiptIds.Contains(candidate.GoodsReceiptId))
            {
                errors.Add($"Baris Tanda Terima Barang {request.GoodsReceiptItemId} bukan bagian dari Tanda Terima Barang pada Tukar Faktur ini.");
                continue;
            }

            if (candidate.GoodsReceiptStatus != FinGoodsReceiptStatuses.Received)
            {
                errors.Add($"Baris Tanda Terima Barang {request.GoodsReceiptItemId} berasal dari GR yang tidak berstatus RECEIVED.");
                continue;
            }

            var usedHere = consumedInThisRequest.GetValueOrDefault(candidate.GoodsReceiptItemId);
            var remaining = candidate.ReceivedQuantity - candidate.InvoicedQuantity - usedHere;
            if (request.Quantity > remaining)
            {
                errors.Add($"Kuantitas alokasi untuk baris Tanda Terima Barang {request.GoodsReceiptItemId} melebihi sisa yang belum ditagih ({Math.Max(0m, remaining)}).");
                continue;
            }

            consumedInThisRequest[candidate.GoodsReceiptItemId] = usedHere + request.Quantity;
            planned.Add(new PlannedAllocation(
                request.InvoiceItemId,
                candidate.GoodsReceiptItemId,
                request.Quantity,
                Math.Round(request.Quantity * request.InvoiceItemUnitPrice, 2, MidpointRounding.AwayFromZero)));
        }

        foreach (var invoiceItem in requests.GroupBy(x => x.InvoiceItemId))
        {
            var invoiceQuantity = invoiceItem.First().InvoiceItemQuantity;
            var allocatedQuantity = invoiceItem.Sum(x => x.Quantity);
            if (allocatedQuantity != invoiceQuantity)
                errors.Add($"Jumlah alokasi baris invoice {invoiceItem.Key} ({allocatedQuantity}) harus sama dengan kuantitas baris tersebut ({invoiceQuantity}).");
        }

        // Rencana yang tidak valid tidak membawa alokasi apa pun supaya tidak ada yang tersimpan setengah-setengah.
        if (errors.Count > 0) planned.Clear();
        return new AllocationPlan(errors.Count == 0, errors, planned);
    }
}
