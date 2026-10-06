using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

public sealed class BilInvoiceEncounterLink : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RanapInvoiceId { get; set; }
    public Guid LinkedEncounterId { get; set; }
    public string LinkReason { get; set; } = "SURGERY_ORIGIN";
    public Guid? SourceReferralId { get; set; }
    public Guid ReceiptId { get; set; }
    public DateTimeOffset LinkedAt { get; set; }
    public BilInvoice RanapInvoice { get; set; } = null!;
    public RegPatientEncounter LinkedEncounter { get; set; } = null!;
    public InpAdmissionReferral? SourceReferral { get; set; }
    public BilInpatientEventReceipt Receipt { get; set; } = null!;
}
