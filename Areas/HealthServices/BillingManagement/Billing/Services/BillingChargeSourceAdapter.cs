using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public interface IBillingChargeSourceAdapter
{
    BillingChargeSourceSnapshot ValidateAndNormalize(UpsertChargeRequest request);
    BillingChargeVoidSnapshot ValidateVoid(BilInvoiceItem item, VoidInvoiceItemRequest request);
    bool IsOrderComplete(BilInvoiceItem item);
    bool IsNormallyVoidable(string sourceDomain, string sourceStatus);
    bool IsRoomStayDomain(string? sourceDomain);
    bool IsRoomCorrection(string? sourceDomain, string? sourceStatus);
}

public sealed record BillingChargeSourceSnapshot(string SourceDomain, string SourceDetailId, string SourceStatus);
public sealed record BillingChargeVoidSnapshot(long SourceVersion, string SourceStatus, string ContractVersion);

public sealed class ContractBillingChargeSourceAdapter : IBillingChargeSourceAdapter
{
    public const string ContractVersion = "BIL-INTEGRATION-0.4";
    public const string IntegrationContractVersion12 = "BIL-INTEGRATION-1.2";

    /// <summary>
    /// Kontrak jembatan Rawat Jalan → invoice canonical (<c>RJ-E2E-CONTRACT-001</c>). Hanya versi
    /// ini yang membuka obat tahap 1 (<c>PHARMACY</c>/<c>PRESCRIBED</c>, <c>RJ-E2E-DEC-005</c>) dan
    /// domain <c>CONSULTATION</c> (<c>RJ-E2E-DEC-001</c>). Pemanggil lama dengan 0.4/1.2 tetap
    /// terikat aturan lama sehingga perilakunya tidak berubah diam-diam.
    /// </summary>
    public const string IntegrationContractVersion13 = "BIL-INTEGRATION-1.3";

    private static readonly IReadOnlySet<string> Contract13OnlyDomains =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CONSULTATION" };

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
            // RJ-E2E-DEC-005: obat ditagih dua tahap. PRESCRIBED = tahap 1 saat resep difinalkan
            // dokter, supaya gerbang "lunas sebelum serah" farmasi dapat dilalui; masih boleh
            // dibatalkan normal selama belum diproses farmasi. DISPENSED = jumlah aktual yang
            // diserahkan dan tetap final — koreksinya berupa adjustment. PRESCRIBED hanya sah
            // pada kontrak 1.3 (lihat ValidateAndNormalize).
            ["PHARMACY"] = Policy(["PRESCRIBED", "DISPENSED"], ["PRESCRIBED"], ["CANCELLED"]),
            // RJ-E2E-DEC-001/010: jasa konsultasi ditagih saat konsultasi Completed. Konsultasi
            // yang sudah selesai tidak dibuka ulang, sehingga koreksinya selalu adjustment.
            ["CONSULTATION"] = Policy(["COMPLETED"], [], []),
            ["CONSUMABLE"] = Policy(["USED"], [], []),
            // Biaya bebas yang diketik langsung oleh kasir pada Menu Pembayaran (BKC-DEC-047):
            // nama/harga bebas, tanpa gerbang approval, tapi tetap boleh dibatalkan kasir sendiri
            // sebelum invoice final - beda dari domain producer klinis di atas yang begitu
            // selesai (dispensed/used) tidak lagi bisa dibatalkan normal.
            // Biaya ad-hoc kasir tidak punya siklus pemenuhan order: begitu dicatat, layanannya
            // sudah terjadi. Tanpa penanda ini, statusnya ADDED selalu masuk NormalVoidFromStatuses
            // sehingga IsOrderComplete permanen false - dan invoice yang seluruh itemnya ADHOC
            // tidak pernah bisa difinalisasi, baik otomatis maupun manual.
            ["ADHOC"] = Policy(["ADDED"], ["ADDED"], ["VOIDED"], completeOnEntry: true),

            // BKC-DEC-059: entri dari katalog tarif. Siklus hidupnya identik dengan ADHOC - harga
            // dan deskripsinya ditetapkan server saat item dicatat, bukan menunggu pemenuhan order
            // dari modul klinis - tetapi dipisahkan sebagai SourceDomain sendiri supaya item yang
            // berasal dari MstTariff dapat dibedakan dari entri bebas kasir pada audit dan laporan.
            ["ADHOC_CATALOG"] = Policy(["ADDED"], ["ADDED"], ["VOIDED"], completeOnEntry: true),

            // BKC-DEC-112, BKC-DES-042, BIL-INT-015: Domain hunian tempat tidur rawat inap (Inpatient Room Stay)
            // Status billable: OCCUPIED (sedang menempati), TRANSFERRED (pindah bed), CORRECTED / ROOM_CORRECTION (koreksi penempatan), RELEASED (pulang/keluar)
            ["ROOM_STAY"] = Policy(
                ["OCCUPIED", "TRANSFERRED", "CORRECTED", "ROOM_CORRECTION", "RELEASED"],
                ["OCCUPIED", "TRANSFERRED", "CORRECTED", "ROOM_CORRECTION"],
                ["CORRECTED", "ROOM_CORRECTION", "CANCELLED", "VOIDED"]),
            ["INPATIENT"] = Policy(
                ["OCCUPIED", "TRANSFERRED", "CORRECTED", "ROOM_CORRECTION", "RELEASED"],
                ["OCCUPIED", "TRANSFERRED", "CORRECTED", "ROOM_CORRECTION"],
                ["CORRECTED", "ROOM_CORRECTION", "CANCELLED", "VOIDED"]),

            // BKC-DEC-117, BKC-DES-048, BIL-INT-017: Multi-domain non-destruktif konsolidasi alihan IGD
            // Item pelayanan IGD tetap mempertahankan SourceDomain = "EMERGENCY" pada invoice ranap gabungan
            ["EMERGENCY"] = Policy(
                ["CONFIRMED", "ACCEPTED", "COMPLETED", "PERFORMED", "DISPENSED", "ADDED"],
                ["CONFIRMED", "ACCEPTED", "ADDED"],
                ["CANCELLED", "VOIDED"])
        };

    public BillingChargeSourceSnapshot ValidateAndNormalize(UpsertChargeRequest request)
    {
        var contractVer = request.ContractVersion?.Trim();
        if (!IsSupportedContractVersion(contractVer))
            throw new BillingInvoiceValidationException(SupportedContractVersionMessage);
        var isContract13 = string.Equals(contractVer, IntegrationContractVersion13, StringComparison.Ordinal);
        var domain = Required(request.SourceDomain, "SourceDomain").ToUpperInvariant();
        var detailId = Required(request.SourceDetailId, "SourceDetailId");
        var status = Required(request.SourceStatus, "SourceStatus").ToUpperInvariant();
        if (!SourcePolicies.TryGetValue(domain, out var policy)
            || (Contract13OnlyDomains.Contains(domain) && !isContract13))
            throw new BillingInvoiceValidationException("SourceDomain belum didukung oleh kontrak charge Billing.");
        // Sebelum 1.3 hanya jumlah yang benar-benar diserahkan yang boleh ditagih (aturan lama
        // dipertahankan apa adanya); obat tahap 1 PRESCRIBED baru terbuka pada 1.3.
        if (domain == "PHARMACY" && status != "DISPENSED" && !(isContract13 && status == "PRESCRIBED"))
            throw new BillingInvoiceValidationException("Jumlah obat yang diserahkan belum final.");
        if (!policy.BillableStatuses.Contains(status))
            throw new BillingInvoiceValidationException("Source belum mencapai status billable yang disetujui.");
        return new BillingChargeSourceSnapshot(domain, detailId, status);
    }

    public BillingChargeVoidSnapshot ValidateVoid(BilInvoiceItem item, VoidInvoiceItemRequest request)
    {
        var contractVersion = Required(request.ContractVersion, "ContractVersion");
        if (!IsSupportedContractVersion(contractVersion))
            throw new BillingInvoiceValidationException(SupportedContractVersionMessage);

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
        // RJ-E2E-DEC-005 / V2.7.3: obat tahap 1 memang ditagih sebelum diserahkan — pelunasannya
        // adalah syarat farmasi menyerahkan obat. Menganggapnya order yang belum selesai akan
        // menahan finalisasi, dan clearance farmasi baru terbit setelah invoice final dan lunas:
        // deadlock yang justru hendak diputus. PRESCRIBED tetap boleh di-void normal.
        if (string.Equals(item.SourceDomain, "PHARMACY", StringComparison.Ordinal)
            && string.Equals(item.SourceStatus, "PRESCRIBED", StringComparison.Ordinal))
            return true;
        return !policy.NormalVoidFromStatuses.Contains(item.SourceStatus);
    }

    /// <summary>
    /// Item masih boleh di-void normal (belum dikerjakan, mis. <c>ACCEPTED</c>/<c>PRESCRIBED</c>).
    /// Aturannya sama dengan <see cref="ValidateVoid"/>; dipakai jembatan Rawat Jalan untuk memilih
    /// void atau adjustment kredit saat pembatalan klinis (V2.7.5).
    /// </summary>
    public bool IsNormallyVoidable(string sourceDomain, string sourceStatus) =>
        SourcePolicies.TryGetValue(sourceDomain, out var policy)
        && policy.NormalVoidFromStatuses.Contains(sourceStatus);

    public bool IsRoomStayDomain(string? sourceDomain) =>
        !string.IsNullOrWhiteSpace(sourceDomain) &&
        (string.Equals(sourceDomain.Trim(), "ROOM_STAY", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(sourceDomain.Trim(), "INPATIENT", StringComparison.OrdinalIgnoreCase));

    public bool IsRoomCorrection(string? sourceDomain, string? sourceStatus) =>
        IsRoomStayDomain(sourceDomain) &&
        !string.IsNullOrWhiteSpace(sourceStatus) &&
        (string.Equals(sourceStatus.Trim(), "CORRECTED", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(sourceStatus.Trim(), "ROOM_CORRECTION", StringComparison.OrdinalIgnoreCase));

    private static readonly string SupportedContractVersionMessage =
        $"ContractVersion harus {ContractVersion}, {IntegrationContractVersion12}, atau {IntegrationContractVersion13}.";

    private static bool IsSupportedContractVersion(string? contractVersion) =>
        string.Equals(contractVersion, ContractVersion, StringComparison.Ordinal)
        || string.Equals(contractVersion, IntegrationContractVersion12, StringComparison.Ordinal)
        || string.Equals(contractVersion, IntegrationContractVersion13, StringComparison.Ordinal);

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
