using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
public sealed class InpatientBillingStatusResponseDto
{
    public Guid EpisodeId { get; set; }
    public Guid EncounterId { get; set; }
    public bool IsReadable { get; set; }
    public string? ClearanceStatus { get; set; }
    public List<InpClearanceReasonItem> Reasons { get; set; } = [];
    public DateTimeOffset? EvaluatedAt { get; set; }
    public string? InvoiceStatus { get; set; }
    public bool IsClosedWithoutFinancialClearance { get; set; }
}
