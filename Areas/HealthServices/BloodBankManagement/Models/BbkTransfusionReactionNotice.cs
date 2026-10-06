using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

[Table("BbkTransfusionReactionNotice", Schema = "public")]
public class BbkTransfusionReactionNotice : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClinicalReactionId { get; set; }

    public Guid BloodUnitId { get; set; }

    public Guid PatientId { get; set; }

    public Guid EncounterId { get; set; }

    public Guid? ServiceUnitId { get; set; }

    [MaxLength(250)]
    public string ReactionSummarySnapshot { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public BbkReactionNoticeStatus Status { get; set; } = BbkReactionNoticeStatus.New;

    public Guid? AcknowledgedByUserId { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    [MaxLength(500)]
    public string? AcknowledgeNote { get; set; }
}
