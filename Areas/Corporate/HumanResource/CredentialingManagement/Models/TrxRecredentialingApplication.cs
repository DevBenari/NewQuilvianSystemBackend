using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxRecredentialingApplication", Schema = "public")]
    public class TrxRecredentialingApplication : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? PreviousCredentialingApplicationId { get; set; }

        public Guid? CurrentCredentialLicenseId { get; set; }

        public Guid? CurrentCertificationId { get; set; }

        public Guid? CurrentClinicalPrivilegeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecredentialingNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string RecredentialingStatus { get; set; } = "Draft";

        public int CycleNumber { get; set; } = 1;

        public DateTime DueDate { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public string? ComplianceSnapshotJson { get; set; }

        public string? ChangeSummaryJson { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public Guid? CompletedByUserId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public TrxCredentialingApplication? PreviousCredentialingApplication { get; set; }
        public WfpCredentialLicense? CurrentCredentialLicense { get; set; }
        public WfpCertification? CurrentCertification { get; set; }
        public WfpClinicalPrivilege? CurrentClinicalPrivilege { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? CompletedByUser { get; set; }

    }
}
