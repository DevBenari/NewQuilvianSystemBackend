using QuilvianSystemBackend.Enums.HumanResource;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs
{
    public class WfpClinicalPrivilegeSummaryResponse
    {
        public int TotalPrivilege { get; set; }
        public int ActivePrivilege { get; set; }
        public int InactivePrivilege { get; set; }
        public int GrantedPrivilege { get; set; }
        public int RejectedPrivilege { get; set; }
        public int SuspendedPrivilege { get; set; }
        public int RevokedPrivilege { get; set; }
        public int TemporaryPrivilege { get; set; }
        public int SupervisionRequiredPrivilege { get; set; }
        public int SchedulingBlockedPrivilege { get; set; }
        public int ClinicalServiceBlockedPrivilege { get; set; }
        public int ExpiredPrivilege { get; set; }
        public int ExpiringSoonPrivilege { get; set; }
    }

    public class WfpClinicalPrivilegeResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? CredentialLicenseId { get; set; }
        public string? CredentialLicenseType { get; set; }
        public string? CredentialLicenseNumber { get; set; }
        public Guid? ClinicalPrivilegeCatalogId { get; set; }
        public string? ClinicalPrivilegeCatalogCode { get; set; }
        public string? ClinicalPrivilegeCatalogName { get; set; }
        public string? PrivilegeCategory { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public Guid? SpecializationId { get; set; }
        public string? SpecializationName { get; set; }
        public Guid? CredentialingDecisionId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? SupervisorUserId { get; set; }
        public string? SupervisorUserName { get; set; }
        public string PrivilegeCode { get; set; } = string.Empty;
        public string PrivilegeName { get; set; } = string.Empty;
        public ClinicalPrivilegeType PrivilegeType { get; set; }
        public string PrivilegeTypeName { get; set; } = string.Empty;
        public string? ClinicalScope { get; set; }
        public string? SpecialtyName { get; set; }
        public string? SubSpecialtyName { get; set; }
        public string? ProcedureGroup { get; set; }
        public string? ProcedureName { get; set; }
        public string? PracticeLocation { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsExpired { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public ClinicalPrivilegeStatus PrivilegeStatus { get; set; }
        public string PrivilegeStatusName { get; set; } = string.Empty;
        public bool IsTemporary { get; set; }
        public bool IsEmergencyPrivilege { get; set; }
        public bool IsSupervisionRequired { get; set; }
        public string? Restrictions { get; set; }
        public bool IsSchedulingBlocked { get; set; }
        public bool IsClinicalServiceBlocked { get; set; }
        public string? SupportingFilePath { get; set; }
        public string? SupportingFileContentType { get; set; }
        public bool HasSupportingFile { get; set; }
        public DateTime? GrantedAt { get; set; }
        public Guid? GrantedByUserId { get; set; }
        public string? GrantedByUserName { get; set; }
        public string? ApprovalNotes { get; set; }
        public DateTime? RejectedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByUserName { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? SuspendedAt { get; set; }
        public Guid? SuspendedByUserId { get; set; }
        public string? SuspendedByUserName { get; set; }
        public string? SuspensionReason { get; set; }
        public DateTime? RevokedAt { get; set; }
        public Guid? RevokedByUserId { get; set; }
        public string? RevokedByUserName { get; set; }
        public string? RevocationReason { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpClinicalPrivilegeDetailResponse : WfpClinicalPrivilegeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpClinicalPrivilegeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpClinicalPrivilegeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpClinicalPrivilegeCatalogOptionResponse> CatalogOptions { get; set; } = new();
        public List<WfpClinicalPrivilegeLicenseOptionResponse> CredentialLicenseOptions { get; set; } = new();
        public List<WfpClinicalPrivilegeEnumOptionResponse> PrivilegeTypeOptions { get; set; } = new();
        public List<WfpClinicalPrivilegeEnumOptionResponse> PrivilegeStatusOptions { get; set; } = new();
        public List<WfpClinicalPrivilegeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpClinicalPrivilegeDefaultFilterResponse
    {
        public Guid? ClinicalPrivilegeCatalogId { get; set; }
        public Guid? CredentialLicenseId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public ClinicalPrivilegeType? PrivilegeType { get; set; }
        public ClinicalPrivilegeStatus? PrivilegeStatus { get; set; }
        public bool? IsTemporary { get; set; }
        public bool? IsEmergencyPrivilege { get; set; }
        public bool? IsSupervisionRequired { get; set; }
        public bool? IsSchedulingBlocked { get; set; }
        public bool? IsClinicalServiceBlocked { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsActive { get; set; }
        public int? ExpiringWithinDays { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "effectiveStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpClinicalPrivilegeCatalogOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public Guid? SpecializationId { get; set; }
        public string? SpecializationName { get; set; }
        public bool RequiresSupervision { get; set; }
        public bool AllowsIndependentPractice { get; set; }
        public bool IsHighRisk { get; set; }
        public int? DefaultValidityMonths { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class WfpClinicalPrivilegeLicenseOptionResponse
    {
        public Guid Id { get; set; }
        public string LicenseType { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime ExpiredDate { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsActive { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class WfpClinicalPrivilegeEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpClinicalPrivilegeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpClinicalPrivilegeRequest
    {
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
        public bool IsTemporary { get; set; }
        public bool IsEmergencyPrivilege { get; set; }
        public bool IsSupervisionRequired { get; set; }

        [MaxLength(2000)]
        public string? Restrictions { get; set; }

        public bool IsSchedulingBlocked { get; set; }
        public bool IsClinicalServiceBlocked { get; set; }

        [MaxLength(1000)]
        public string? SupportingFilePath { get; set; }

        [MaxLength(150)]
        public string? SupportingFileContentType { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpClinicalPrivilegeRequest : CreateWfpClinicalPrivilegeRequest
    {
    }

    public class UpdateWfpClinicalPrivilegeStatusRequest
    {
        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }

    public class GrantWfpClinicalPrivilegeRequest
    {
        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }

        public bool IsSchedulingBlocked { get; set; }
        public bool IsClinicalServiceBlocked { get; set; }
    }

    public class RejectWfpClinicalPrivilegeRequest
    {
        [Required]
        [MaxLength(1000)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class SuspendWfpClinicalPrivilegeRequest
    {
        [Required]
        [MaxLength(1000)]
        public string SuspensionReason { get; set; } = string.Empty;

        public bool IsSchedulingBlocked { get; set; } = true;
        public bool IsClinicalServiceBlocked { get; set; } = true;
    }

    public class RevokeWfpClinicalPrivilegeRequest
    {
        [Required]
        [MaxLength(1000)]
        public string RevocationReason { get; set; } = string.Empty;
    }
}
