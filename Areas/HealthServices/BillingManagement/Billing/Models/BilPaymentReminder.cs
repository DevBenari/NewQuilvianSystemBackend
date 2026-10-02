using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilPaymentReminder", Schema = "public")]
public sealed class BilPaymentReminder : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public Guid InvoiceId { get; set; }
    [Required] public Guid PatientId { get; set; }
    [Required, MaxLength(30)] public string Channel { get; set; } = "WHATSAPP";
    [Required, MaxLength(30)] public string Status { get; set; } = BilPaymentReminderStatuses.Sent;
    public DateTime? SentAt { get; set; }
    public Guid? SentByUserId { get; set; }
    [MaxLength(50)] public string? MessageTemplateCode { get; set; }
    [MaxLength(100)] public string? ProviderReferenceMasked { get; set; }
    [MaxLength(500)] public string? FailureReason { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice? Invoice { get; set; }
}

public static class BilPaymentReminderStatuses
{
    public const string Sent = "SENT";
    public const string Failed = "FAILED";
    public const string BlockedNoProvider = "BLOCKED_NO_PROVIDER";
}
