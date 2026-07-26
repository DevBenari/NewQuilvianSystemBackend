using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Enums.HumanResource;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("WfpClinicalPrivilege", Schema = "public")]
    public class WfpClinicalPrivilege : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? CredentialLicenseId { get; set; }

        public Guid? ClinicalPrivilegeCatalogId { get; set; }

        public Guid? CredentialingDecisionId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public Guid? SupervisorUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PrivilegeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string PrivilegeName { get; set; } = string.Empty;

        public ClinicalPrivilegeType PrivilegeType { get; set; }

        [MaxLength(1000)]
        public string? ClinicalScope { get; set; }

        [MaxLength(200)]
        public string? SpecialtyName { get; set; }

        [MaxLength(200)]
        public string? SubSpecialtyName { get; set; }

        [MaxLength(200)]
        public string? ProcedureGroup { get; set; }

        [MaxLength(250)]
        public string? ProcedureName { get; set; }

        [MaxLength(250)]
        public string? PracticeLocation { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public ClinicalPrivilegeStatus PrivilegeStatus { get; set; }

        public bool IsTemporary { get; set; } = false;

        public bool IsEmergencyPrivilege { get; set; } = false;

        public bool IsSupervisionRequired { get; set; } = false;

        [MaxLength(2000)]
        public string? Restrictions { get; set; }

        public bool IsSchedulingBlocked { get; set; } = false;

        public bool IsClinicalServiceBlocked { get; set; } = false;

        [MaxLength(1000)]
        public string? SupportingFilePath { get; set; }

        [MaxLength(150)]
        public string? SupportingFileContentType { get; set; }

        public DateTime? GrantedAt { get; set; }

        public Guid? GrantedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }

        public DateTime? RejectedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        public DateTime? SuspendedAt { get; set; }

        public Guid? SuspendedByUserId { get; set; }

        [MaxLength(1000)]
        public string? SuspensionReason { get; set; }

        public DateTime? RevokedAt { get; set; }

        public Guid? RevokedByUserId { get; set; }

        [MaxLength(1000)]
        public string? RevocationReason { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpCredentialLicense? CredentialLicense { get; set; }
        public MstClinicalPrivilegeCatalog? ClinicalPrivilegeCatalog { get; set; }
        public TrxCredentialingDecision? CredentialingDecision { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public ApplicationUser? SupervisorUser { get; set; }
        public ApplicationUser? GrantedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
        public ApplicationUser? SuspendedByUser { get; set; }
        public ApplicationUser? RevokedByUser { get; set; }

        public ICollection<TrxClinicalPrivilegeRequest> Requests { get; set; } = new List<TrxClinicalPrivilegeRequest>();
        public ICollection<TrxClinicalPrivilegeSuspension> Suspensions { get; set; } = new List<TrxClinicalPrivilegeSuspension>();
        public ICollection<TrxClinicalPrivilegeRevocation> Revocations { get; set; } = new List<TrxClinicalPrivilegeRevocation>();
    }
}
