using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public interface IBillingChargeSourceAdapter
{
    BillingChargeSourceSnapshot ValidateAndNormalize(UpsertChargeRequest request);
    BillingChargeVoidSnapshot ValidateVoid(BilInvoiceItem item, VoidInvoiceItemRequest request);
    bool IsOrderComplete(BilInvoiceItem item);
}

public sealed record BillingChargeSourceSnapshot(string SourceDomain, string SourceDetailId, string SourceStatus);
public sealed record BillingChargeVoidSnapshot(long SourceVersion, string SourceStatus, string ContractVersion);

public sealed class ContractBillingChargeSourceAdapter : IBillingChargeSourceAdapter
{
    public const string ContractVersion = "BIL-INTEGRATION-0.4";

    private static readonly IReadOnlyDictionary<string, SourceLifecyclePolicy> SourcePolicies =
        new Dictionary<string, SourceLifecyclePolicy>(StringComparer.OrdinalIgnoreCase)
        {
            ["PROCEDURE"] = Policy(
                ["CONFIRMED", "ACCEPTED", "COMPLETED", "PERFORMED"],
                ["CONFIRMED", "ACCEPTED"],
                ["CANCELLED", "VOIDED"]),
            ["LABORATORY"] = Policy(
                ["CONFIRMED", "ACCEPTED", "COMPLETED", "PERFORMED"],
                ["CONFIRMED", "ACCEPTED"],
                ["CANCELLED", "VOIDED"]),
            ["RADIOLOGY"] = Policy(
                ["CONFIRMED", "ACCEPTED", "COMPLETED", "PERFORMED"],
                ["CONFIRMED", "ACCEPTED"],
                ["CANCELLED", "VOIDED"]),
            // Dispense dan usage adalah fakta final producer. Koreksinya harus berupa event/adjustment baru.
            ["PHARMACY"] = Policy(["DISPENSED"], [], []),
            ["CONSUMABLE"] = Policy(["USED"], [], []),
            // Biaya bebas yang diketik langsung oleh kasir pada Menu Pembayaran (BKC-DEC-047):
            // nama/harga bebas, tanpa gerbang approval, tapi tetap boleh dibatalkan kasir sendiri
            // sebelum invoice final - beda dari domain producer klinis di atas yang begitu
            // selesai (dispensed/used) tidak lagi bisa dibatalkan normal.
            // Biaya ad-hoc kasir tidak punya siklus pemenuhan order: begitu dicatat, layanannya
            // sudah terjadi. Tanpa penanda ini, statusnya ADDED selalu masuk NormalVoidFromStatuses
            // sehingga IsOrderComplete permanen false - dan invoice yang seluruh itemnya ADHOC
            // tidak pernah bisa difinalisasi, baik otomatis maupun manual.
            ["ADHOC"] = Policy(["ADDED"], ["ADDED"], ["VOIDED"], completeOnEntry: true)
        };

    public BillingChargeSourceSnapshot ValidateAndNormalize(UpsertChargeRequest request)
    {
        if (!string.Equals(request.ContractVersion?.Trim(), ContractVersion, StringComparison.Ordinal))
            throw new BillingInvoiceValidationException($"ContractVersion harus {ContractVersion}.");
        var domain = Required(request.SourceDomain, "SourceDomain").ToUpperInvariant();
        var detailId = Required(request.SourceDetailId, "SourceDetailId");
        var status = Required(request.SourceStatus, "SourceStatus").ToUpperInvariant();
        if (!SourcePolicies.TryGetValue(domain, out var policy))
            throw new BillingInvoiceValidationException("SourceDomain belum didukung oleh kontrak charge Billing.");
        if (domain == "PHARMACY" && status != "DISPENSED")
            throw new BillingInvoiceValidationException("Jumlah obat yang diserahkan belum final.");
        if (!policy.BillableStatuses.Contains(status))
            throw new BillingInvoiceValidationException("Source belum mencapai status billable yang disetujui.");
        return new BillingChargeSourceSnapshot(domain, detailId, status);
    }

    public BillingChargeVoidSnapshot ValidateVoid(BilInvoiceItem item, VoidInvoiceItemRequest request)
    {
        var contractVersion = Required(request.ContractVersion, "ContractVersion");
        if (!string.Equals(contractVersion, ContractVersion, StringComparison.Ordinal))
            throw new BillingInvoiceValidationException($"ContractVersion harus {ContractVersion}.");

        if (!SourcePolicies.TryGetValue(item.SourceDomain, out var policy))
            throw new BillingInvoiceValidationException("SourceDomain belum didukung oleh kontrak charge Billing.");

        if (!policy.NormalVoidFromStatuses.Contains(item.SourceStatus))
            throw new BillingInvoiceValidationException(
                "Item tidak dapat dibatalkan karena pelayanan atau pembayaran sudah diproses.");

        var sourceStatus = Required(request.SourceStatus, "SourceStatus").ToUpperInvariant();
        if (!policy.VoidStatuses.Contains(sourceStatus))
            throw new BillingInvoiceValidationException(
                "Status pembatalan source tidak sesuai kontrak producer.");

        if (request.SourceVersion <= item.SourceVersion)
            throw new BillingInvoiceConflictException(
                "Versi source pembatalan harus lebih baru dari data Billing saat ini.");

        return new BillingChargeVoidSnapshot(
            request.SourceVersion,
            sourceStatus,
            contractVersion);
    }

    public bool IsOrderComplete(BilInvoiceItem item)
    {
        if (!SourcePolicies.TryGetValue(item.SourceDomain, out var policy))
            throw new BillingInvoiceValidationException("SourceDomain belum didukung oleh kontrak charge Billing.");
        if (policy.CompleteOnEntry) return true;
        return !policy.NormalVoidFromStatuses.Contains(item.SourceStatus);
    }

    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new BillingInvoiceValidationException($"{field} wajib diisi.");
        return value.Trim();
    }
    private static SourceLifecyclePolicy Policy(
        string[] billableStatuses,
        string[] normalVoidFromStatuses,
        string[] voidStatuses,
        bool completeOnEntry = false) => new(
            Set(billableStatuses),
            Set(normalVoidFromStatuses),
            Set(voidStatuses),
            completeOnEntry);

    private static IReadOnlySet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);

    private sealed record SourceLifecyclePolicy(
        IReadOnlySet<string> BillableStatuses,
        IReadOnlySet<string> NormalVoidFromStatuses,
        IReadOnlySet<string> VoidStatuses,
        bool CompleteOnEntry);
}
