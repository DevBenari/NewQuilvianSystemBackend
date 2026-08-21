using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public interface IBillingChargeSourceAdapter
{
    BillingChargeSourceSnapshot ValidateAndNormalize(UpsertChargeRequest request);
}

public sealed record BillingChargeSourceSnapshot(string SourceDomain, string SourceDetailId, string SourceStatus);

public sealed class ContractBillingChargeSourceAdapter : IBillingChargeSourceAdapter
{
    public const string ContractVersion = "BIL-INTEGRATION-0.4";

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> BillableStatuses =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["PROCEDURE"] = Set("CONFIRMED", "ACCEPTED", "COMPLETED", "PERFORMED"),
            ["LABORATORY"] = Set("CONFIRMED", "ACCEPTED", "COMPLETED", "PERFORMED"),
            ["RADIOLOGY"] = Set("CONFIRMED", "ACCEPTED", "COMPLETED", "PERFORMED"),
            ["PHARMACY"] = Set("DISPENSED"),
            ["CONSUMABLE"] = Set("USED")
        };

    public BillingChargeSourceSnapshot ValidateAndNormalize(UpsertChargeRequest request)
    {
        if (!string.Equals(request.ContractVersion?.Trim(), ContractVersion, StringComparison.Ordinal))
            throw new BillingInvoiceValidationException($"ContractVersion harus {ContractVersion}.");
        var domain = Required(request.SourceDomain, "SourceDomain").ToUpperInvariant();
        var detailId = Required(request.SourceDetailId, "SourceDetailId");
        var status = Required(request.SourceStatus, "SourceStatus").ToUpperInvariant();
        if (!BillableStatuses.TryGetValue(domain, out var statuses))
            throw new BillingInvoiceValidationException("SourceDomain belum didukung oleh kontrak charge Billing.");
        if (domain == "PHARMACY" && status != "DISPENSED")
            throw new BillingInvoiceValidationException("Jumlah obat yang diserahkan belum final.");
        if (!statuses.Contains(status))
            throw new BillingInvoiceValidationException("Source belum mencapai status billable yang disetujui.");
        return new BillingChargeSourceSnapshot(domain, detailId, status);
    }

    private static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new BillingInvoiceValidationException($"{field} wajib diisi.");
        return value.Trim();
    }
    private static IReadOnlySet<string> Set(params string[] values) => new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
}
