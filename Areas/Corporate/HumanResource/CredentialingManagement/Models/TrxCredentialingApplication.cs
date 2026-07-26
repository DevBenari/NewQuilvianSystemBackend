using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxCredentialingApplication", Schema = "public")]
    public class TrxCredentialingApplication : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? ProfessionId { get; set; }

        public Guid? SpecializationId { get; set; }

        public Guid? CredentialingRequirementId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApplicationNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ApplicationType { get; set; } = "InitialCredentialing";

        [Required]
        [MaxLength(30)]
        public string ApplicationStatus { get; set; } = "Draft";

        public DateTime ApplicationDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public bool IsLicenseCompliant { get; set; } = false;

        public bool IsCertificationCompliant { get; set; } = false;

        public bool IsContractCompliant { get; set; } = false;

        public bool IsPrivilegeCompliant { get; set; } = false;

        public bool BlocksScheduling { get; set; } = true;

        public bool BlocksClinicalService { get; set; } = true;

        public string? WorkforceSnapshotJson { get; set; }

        public string? RequirementSnapshotJson { get; set; }

        public string? ComplianceResultJson { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstDoctor? Doctor { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstCredentialingRequirement? CredentialingRequirement { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }

        public ICollection<TrxCredentialingDocument> Documents { get; set; } = new List<TrxCredentialingDocument>();
        public ICollection<TrxCredentialingVerification> Verifications { get; set; } = new List<TrxCredentialingVerification>();
        public ICollection<TrxCredentialingCommitteeReview> CommitteeReviews { get; set; } = new List<TrxCredentialingCommitteeReview>();
        public ICollection<TrxCredentialingDecision> Decisions { get; set; } = new List<TrxCredentialingDecision>();
    }
}
