using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums.HumanResource;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("WfpComplianceAlert", Schema = "public")]
    public class WfpComplianceAlert : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SourceEntityName { get; set; } = string.Empty;

        public Guid SourceEntityId { get; set; }

        public ComplianceAlertType AlertType { get; set; }

        [Required]
        [MaxLength(250)]
        public string AlertTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string AlertMessage { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public ComplianceAlertStatus AlertStatus { get; set; }

        public ComplianceAlertSeverityLevel SeverityLevel { get; set; }

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByUserId { get; set; }

        public bool BlocksScheduling { get; set; } = false;

        public bool BlocksClinicalService { get; set; } = false;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? ResolvedByUser { get; set; }

        public ICollection<WfpComplianceAlertLog> Logs { get; set; } = new List<WfpComplianceAlertLog>();
    }
}
