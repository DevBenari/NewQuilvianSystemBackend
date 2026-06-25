using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpClinicalPrivilege", Schema = "public")]
    public class WfpClinicalPrivilege : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? CredentialLicenseId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrivilegeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PrivilegeName { get; set; } = string.Empty;

        public ClinicalPrivilegeType PrivilegeType { get; set; }
            = ClinicalPrivilegeType.CorePrivilege;

        [MaxLength(100)]
        public string? ClinicalScope { get; set; }
        // Department, Service, Procedure, Specialty, Unit, Telemedicine

        [MaxLength(150)]
        public string? SpecialtyName { get; set; }

        [MaxLength(150)]
        public string? SubSpecialtyName { get; set; }

        [MaxLength(150)]
        public string? ProcedureGroup { get; set; }

        [MaxLength(200)]
        public string? ProcedureName { get; set; }

        [MaxLength(200)]
        public string? PracticeLocation { get; set; }

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public ClinicalPrivilegeStatus PrivilegeStatus { get; set; }
            = ClinicalPrivilegeStatus.PendingApproval;

        public bool IsTemporary { get; set; } = false;

        public bool IsEmergencyPrivilege { get; set; } = false;

        public bool IsSupervisionRequired { get; set; } = false;

        public Guid? SupervisorUserId { get; set; }

        public Guid? GrantedByUserId { get; set; }

        public DateTime? GrantedAt { get; set; }

        [MaxLength(250)]
        public string? GrantNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public DateTime? RejectedAt { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public Guid? SuspendedByUserId { get; set; }

        public DateTime? SuspendedAt { get; set; }

        [MaxLength(250)]
        public string? SuspensionReason { get; set; }

        public Guid? RevokedByUserId { get; set; }

        public DateTime? RevokedAt { get; set; }

        [MaxLength(250)]
        public string? RevokedReason { get; set; }

        public DateTime? LastReviewDate { get; set; }

        public DateTime? NextReviewDate { get; set; }

        [MaxLength(500)]
        public string? SupportingFilePath { get; set; }

        [MaxLength(100)]
        public string? SupportingFileContentType { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }

        [NotMapped]
        public bool IsExpired =>
            EffectiveEndDate.HasValue &&
            AppDateTimeHelper.OperationalDate() > EffectiveEndDate.Value.Date;

        [NotMapped]
        public bool IsCurrentlyValid =>
            IsActive &&
            !IsDelete &&
            PrivilegeStatus == ClinicalPrivilegeStatus.Active &&
            (!EffectiveEndDate.HasValue || AppDateTimeHelper.OperationalDate() <= EffectiveEndDate.Value.Date);

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public WfpCredentialLicense? CredentialLicense { get; set; }

        public MstDepartment? Department { get; set; }

        public MstPosition? Position { get; set; }

        public ApplicationUser? SupervisorUser { get; set; }

        public ApplicationUser? GrantedByUser { get; set; }

        public ApplicationUser? RejectedByUser { get; set; }

        public ApplicationUser? SuspendedByUser { get; set; }

        public ApplicationUser? RevokedByUser { get; set; }
    }
}
