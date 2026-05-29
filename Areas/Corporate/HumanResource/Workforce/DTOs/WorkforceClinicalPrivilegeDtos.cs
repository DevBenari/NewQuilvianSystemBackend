using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceClinicalPrivilegeResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? CredentialLicenseId { get; set; }

        public string? CredentialLicenseType { get; set; }

        public string? CredentialLicenseNumber { get; set; }

        public Guid? DepartmentId { get; set; }

        public string? DepartmentCode { get; set; }

        public string? DepartmentName { get; set; }

        public Guid? PositionId { get; set; }

        public string? PositionCode { get; set; }

        public string? PositionName { get; set; }

        public string PrivilegeCode { get; set; } = string.Empty;

        public string PrivilegeName { get; set; } = string.Empty;

        public ClinicalPrivilegeType PrivilegeType { get; set; }

        public string? ClinicalScope { get; set; }

        public string? SpecialtyName { get; set; }

        public string? SubSpecialtyName { get; set; }

        public string? ProcedureGroup { get; set; }

        public string? ProcedureName { get; set; }

        public string? PracticeLocation { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public ClinicalPrivilegeStatus PrivilegeStatus { get; set; }

        public bool IsTemporary { get; set; }

        public bool IsEmergencyPrivilege { get; set; }

        public bool IsSupervisionRequired { get; set; }

        public Guid? SupervisorUserId { get; set; }

        public string? SupervisorUserName { get; set; }

        public Guid? GrantedByUserId { get; set; }

        public string? GrantedByUserName { get; set; }

        public DateTime? GrantedAt { get; set; }

        public string? GrantNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public string? RejectedByUserName { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? RejectedReason { get; set; }

        public Guid? SuspendedByUserId { get; set; }

        public string? SuspendedByUserName { get; set; }

        public DateTime? SuspendedAt { get; set; }

        public string? SuspensionReason { get; set; }

        public Guid? RevokedByUserId { get; set; }

        public string? RevokedByUserName { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedReason { get; set; }

        public DateTime? LastReviewDate { get; set; }

        public DateTime? NextReviewDate { get; set; }

        public string? SupportingFilePath { get; set; }

        public string? SupportingFileContentType { get; set; }

        public bool HasSupportingFile { get; set; }

        public bool IsExpired { get; set; }

        public bool IsCurrentlyValid { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceClinicalPrivilegeListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PendingApprovalData { get; set; }

        public int SuspendedData { get; set; }

        public int RejectedData { get; set; }

        public int RevokedData { get; set; }

        public int ExpiredData { get; set; }

        public int CurrentlyValidData { get; set; }

        public int WithCredentialLicenseData { get; set; }

        public int WithSupportingFileData { get; set; }

        public List<WorkforceClinicalPrivilegeResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceClinicalPrivilegeRequest
    {
        public Guid? CredentialLicenseId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrivilegeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PrivilegeName { get; set; } = string.Empty;

        public ClinicalPrivilegeType PrivilegeType { get; set; } = ClinicalPrivilegeType.CorePrivilege;

        [MaxLength(100)]
        public string? ClinicalScope { get; set; }

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

        public bool IsTemporary { get; set; } = false;

        public bool IsEmergencyPrivilege { get; set; } = false;

        public bool IsSupervisionRequired { get; set; } = false;

        public Guid? SupervisorUserId { get; set; }

        public DateTime? LastReviewDate { get; set; }

        public DateTime? NextReviewDate { get; set; }

        public IFormFile? SupportingFile { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceClinicalPrivilegeRequest
    {
        public Guid? CredentialLicenseId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrivilegeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PrivilegeName { get; set; } = string.Empty;

        public ClinicalPrivilegeType PrivilegeType { get; set; } = ClinicalPrivilegeType.CorePrivilege;

        [MaxLength(100)]
        public string? ClinicalScope { get; set; }

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

        public bool IsTemporary { get; set; } = false;

        public bool IsEmergencyPrivilege { get; set; } = false;

        public bool IsSupervisionRequired { get; set; } = false;

        public Guid? SupervisorUserId { get; set; }

        public DateTime? LastReviewDate { get; set; }

        public DateTime? NextReviewDate { get; set; }

        public IFormFile? SupportingFile { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceClinicalPrivilegeStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class GrantWorkforceClinicalPrivilegeRequest
    {
        [MaxLength(250)]
        public string? GrantNote { get; set; }

        public DateTime? NextReviewDate { get; set; }
    }

    public class RejectWorkforceClinicalPrivilegeRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class SuspendWorkforceClinicalPrivilegeRequest
    {
        [Required]
        [MaxLength(250)]
        public string SuspensionReason { get; set; } = string.Empty;
    }

    public class RevokeWorkforceClinicalPrivilegeRequest
    {
        [Required]
        [MaxLength(250)]
        public string RevokedReason { get; set; } = string.Empty;
    }
}
