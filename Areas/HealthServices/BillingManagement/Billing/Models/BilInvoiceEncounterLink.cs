using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

/// <summary>
/// Alasan penautan kunjungan asal ke invoice rawat inap (BKC-DEC-155, BKC-DES-078, BIL-INT-021).
/// </summary>
public static class BillingEncounterLinkReasons
{
    public const string SurgeryOrigin = "SURGERY_ORIGIN";
    public const string EmergencyOrigin = "EMERGENCY_ORIGIN";
    public const string OutpatientOrigin = "OUTPATIENT_ORIGIN";

    public static readonly IReadOnlyList<string> All = new[]
    {
        SurgeryOrigin,
        EmergencyOrigin,
        OutpatientOrigin
    };
}

public sealed class BilInvoiceEncounterLink : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RanapInvoiceId { get; set; }
    public Guid LinkedEncounterId { get; set; }
    public string LinkReason { get; set; } = BillingEncounterLinkReasons.SurgeryOrigin;
    public Guid? SourceReferralId { get; set; }
    public Guid ReceiptId { get; set; }
    public DateTimeOffset LinkedAt { get; set; }
    public BilInvoice RanapInvoice { get; set; } = null!;
    public RegPatientEncounter LinkedEncounter { get; set; } = null!;
    public InpAdmissionReferral? SourceReferral { get; set; }
    public BilInpatientEventReceipt Receipt { get; set; } = null!;
}
