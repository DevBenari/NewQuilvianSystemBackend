namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Port in-process untuk menanyakan persentase jasa medis ke Medical Fee (FR-BKC-287, BKC-DEC-169, BKC-DES-093, BIL-INT-023).
/// Kontrak ini sengaja diimplementasikan oleh modul Medical Fee (BE-MDF-016).
/// Implementasi wajib tidak melempar exception: seluruh kegagalan diubah menjadi status Unavailable.
/// </summary>
public interface IBillingDoctorShareResolver
{
    /// <summary>
    /// Menyelesaikan persentase bagi hasil dokter/nakes berdasarkan kueri layanan berjasa.
    /// </summary>
    Task<BillingDoctorShareResolution> ResolveAsync(
        BillingDoctorShareQuery query,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Parameter kueri resolusi persentase bagi hasil dokter (02-backend-architecture.md §25.5).
/// </summary>
public sealed record BillingDoctorShareQuery(
    string PerformerType,
    Guid PerformerReferenceId,
    Guid TariffId,
    DateOnly ServiceDate,
    string? RoleCode = null);

/// <summary>
/// Hasil resolusi persentase bagian dokter dari Medical Fee (02-backend-architecture.md §25.5).
/// </summary>
public sealed record BillingDoctorShareResolution(
    string Outcome,
    decimal? SharingPercentage = null,
    Guid? SharingRuleId = null)
{
    public const string OutcomeResolved = BillingDoctorShareOutcomes.Resolved;
    public const string OutcomeNoRule = BillingDoctorShareOutcomes.NoRule;
    public const string OutcomeUnavailable = BillingDoctorShareOutcomes.Unavailable;

    public static BillingDoctorShareResolution Resolved(decimal sharingPercentage, Guid sharingRuleId) =>
        new(BillingDoctorShareOutcomes.Resolved, sharingPercentage, sharingRuleId);

    public static BillingDoctorShareResolution NoRule() =>
        new(BillingDoctorShareOutcomes.NoRule);

    public static BillingDoctorShareResolution Unavailable() =>
        new(BillingDoctorShareOutcomes.Unavailable);
}

/// <summary>
/// Konstanta status luaran resolusi bagi hasil dokter (02-backend-architecture.md §25.5).
/// </summary>
public static class BillingDoctorShareOutcomes
{
    public const string Resolved = "Resolved";
    public const string NoRule = "NoRule";
    public const string Unavailable = "Unavailable";
}

