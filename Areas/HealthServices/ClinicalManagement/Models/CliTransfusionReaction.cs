using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliTransfusionReaction", Schema = "public")]
public class CliTransfusionReaction : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MonitoringId { get; set; }

    public CliTransfusionPointType? PointType { get; set; }

    public DateTime OccurredAt { get; set; }

    [MaxLength(250)]
    public string ReactionSummary { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? ReactionDetail { get; set; }

    public Guid RecordedByUserId { get; set; }

    public CliReactionNoticeDelivery NoticeDelivery { get; set; } = CliReactionNoticeDelivery.Pending;

    public int NoticeAttemptCount { get; set; } = 0;

    public Guid? BloodBankNoticeId { get; set; }
}
