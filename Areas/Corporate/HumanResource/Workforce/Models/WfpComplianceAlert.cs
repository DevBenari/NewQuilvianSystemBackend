using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpComplianceAlert", Schema = "public")]
    public class WfpComplianceAlert : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SourceEntityName { get; set; } = string.Empty;

        [Required]
        public Guid SourceEntityId { get; set; }

        public ComplianceAlertType AlertType { get; set; } = ComplianceAlertType.Unknown;

        [Required]
        [MaxLength(200)]
        public string AlertTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string AlertMessage { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        public ComplianceAlertStatus AlertStatus { get; set; } = ComplianceAlertStatus.Open;

        public ComplianceAlertSeverityLevel SeverityLevel { get; set; } = ComplianceAlertSeverityLevel.Low;

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public ApplicationUser? ResolvedByUser { get; set; }

        public ICollection<WfpComplianceAlertLog> Logs { get; set; } = new List<WfpComplianceAlertLog>();
    }

    [Table("WfpComplianceAlertLog", Schema = "public")]
    public class WfpComplianceAlertLog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ComplianceAlertId { get; set; }

        public ComplianceAlertLogType LogType { get; set; } = ComplianceAlertLogType.Created;

        public ComplianceAlertStatus? OldStatus { get; set; }

        public ComplianceAlertStatus? NewStatus { get; set; }

        [MaxLength(1000)]
        public string? LogMessage { get; set; }

        public Guid? PerformedByUserId { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpComplianceAlert? ComplianceAlert { get; set; }

        public ApplicationUser? PerformedByUser { get; set; }
    }
}
