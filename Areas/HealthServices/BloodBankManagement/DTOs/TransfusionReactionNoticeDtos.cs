using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
public class ReceiveTransfusionReactionNotice
{
    public Guid ClinicalReactionId { get; set; }
    public Guid BloodUnitId { get; set; }
    public Guid PatientId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid? ServiceUnitId { get; set; }
    public string ReactionSummary { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public Guid RecordedByUserId { get; set; }
}
public class ReactionNoticeQuery
{
    public BbkReactionNoticeStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public class AcknowledgeReactionNoticeRequest { [MaxLength(500)] public string? Note { get; set; } }
public class ReactionNoticeDetail
{
    public Guid Id { get; set; }
    public Guid ClinicalReactionId { get; set; }
    public Guid BloodUnitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PmiBagNumber { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
    public Guid? ServiceUnitId { get; set; }
    public string? ServiceUnitName { get; set; }
    public string ReactionSummary { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public BbkReactionNoticeStatus Status { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedByUserId { get; set; }
    public string? AcknowledgeNote { get; set; }
}
