using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("WfpComplianceAlertLog", Schema = "public")]
    public class WfpComplianceAlertLog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ComplianceAlertId { get; set; }

        public ComplianceAlertLogType LogType { get; set; }

        public ComplianceAlertStatus? OldStatus { get; set; }

        public ComplianceAlertStatus? NewStatus { get; set; }

        [Required]
        [MaxLength(2000)]
        public string LogMessage { get; set; } = string.Empty;

        public Guid? PerformedByUserId { get; set; }

        public DateTime PerformedAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpComplianceAlert? ComplianceAlert { get; set; }
        public ApplicationUser? PerformedByUser { get; set; }

    }
}
