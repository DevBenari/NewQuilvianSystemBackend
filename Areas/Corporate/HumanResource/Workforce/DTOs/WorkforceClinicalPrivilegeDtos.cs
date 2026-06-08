using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public enum WorkforceClinicalScope
    {
        Unknown = 0,
        Department = 1,
        Service = 2,
        Procedure = 3,
        Specialty = 4,
        Unit = 5,
        Telemedicine = 6,
        Other = 99
    }

    public class WorkforceClinicalPrivilegeSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalClinicalPrivilege { get; set; }
        public int ActiveClinicalPrivilege { get; set; }
        public int InactiveClinicalPrivilege { get; set; }
        public int PendingApprovalClinicalPrivilege { get; set; }
        public int GrantedClinicalPrivilege { get; set; }
        public int RejectedClinicalPrivilege { get; set; }
        public int SuspendedClinicalPrivilege { get; set; }
        public int RevokedClinicalPrivilege { get; set; }
        public int ExpiredClinicalPrivilege { get; set; }
        public int CurrentlyValidClinicalPrivilege { get; set; }
        public int TemporaryClinicalPrivilege { get; set; }
        public int EmergencyClinicalPrivilege { get; set; }
        public int SupervisionRequiredClinicalPrivilege { get; set; }
        public int ClinicalPrivilegeWithCredentialLicense { get; set; }
        public int ClinicalPrivilegeWithSupportingFile { get; set; }
    }

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

        /// <summary>
        /// Kode otomatis clinical privilege. Format: CLP-RSMMC-00001.
        /// </summary>
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
        public bool HasSupportingFile { get; set; }
        public string? SupportingFilePath { get; set; }
        public string? SupportingFileContentType { get; set; }
        public string? SupportingFileName { get; set; }
        public string? SupportingFilePreviewUrl { get; set; }
        public string? SupportingFileDownloadUrl { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCurrentlyValid { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WorkforceClinicalPrivilegeDetailResponse : WorkforceClinicalPrivilegeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WorkforceClinicalPrivilegeOptionResponse
    {
        public Guid Id { get; set; }
        public string PrivilegeCode { get; set; } = string.Empty;
        public string PrivilegeName { get; set; } = string.Empty;
        public ClinicalPrivilegeType PrivilegeType { get; set; }
        public string? ClinicalScope { get; set; }
        public string? SpecialtyName { get; set; }
        public string? ProcedureGroup { get; set; }
        public string? ProcedureName { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public ClinicalPrivilegeStatus PrivilegeStatus { get; set; }
        public bool HasSupportingFile { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCurrentlyValid { get; set; }
    }

    public class WorkforceClinicalPrivilegeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceClinicalPrivilegeOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceClinicalPrivilegeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeZoneInfo { get; set; } = "Date filter memakai CreateDateTime dalam UTC.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public string CodeInfo { get; set; } = "PrivilegeCode dibuat otomatis oleh backend dengan format CLP-RSMMC-00001 dan tidak dikirim dari POST/PUT.";
        public string RelationFilterInfo { get; set; } = "Filter relasi utama dibatasi 2 dropdown: CredentialLicenseId dan DepartmentId. Position tetap tampil di form, tetapi tidak dijadikan filter utama agar UI tidak terlalu kecil.";
        public WorkforceClinicalPrivilegeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceClinicalPrivilegeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceClinicalPrivilegeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceClinicalPrivilegeEnumOptionResponse> PrivilegeTypeOptions { get; set; } = new();
        public List<WorkforceClinicalPrivilegeEnumOptionResponse> PrivilegeStatusOptions { get; set; } = new();
        public List<WorkforceClinicalPrivilegeEnumOptionResponse> ClinicalScopeOptions { get; set; } = new();
        public WorkforceClinicalPrivilegeFrontendGuideResponse FrontendGuide { get; set; } = new();
        public List<string> PreviewSupportedContentTypes { get; set; } = new()
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };
    }

    public class WorkforceClinicalPrivilegeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public ClinicalPrivilegeType? PrivilegeType { get; set; }
        public ClinicalPrivilegeStatus? PrivilegeStatus { get; set; }
        public Guid? CredentialLicenseId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceClinicalPrivilegeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceClinicalPrivilegeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceClinicalPrivilegeEnumOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public int NumericValue { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkforceClinicalPrivilegeFrontendGuideResponse
    {
        public string PrivilegeNameLabel { get; set; } = "Nama Clinical Privilege";
        public string PrivilegeTypeLabel { get; set; } = "Tipe Clinical Privilege";
        public string ClinicalScopeLabel { get; set; } = "Ruang Lingkup Clinical Privilege";
        public string CredentialLicenseHelpText { get; set; } = "CredentialLicenseId wajib dipilih untuk privilege reguler. Untuk emergency privilege boleh kosong jika IsEmergencyPrivilege = true.";
        public string PrivilegeTypeHelpText { get; set; } = "Pilih tipe privilege dari enum ClinicalPrivilegeType. Default: CorePrivilege.";
        public string ClinicalScopeHelpText { get; set; } = "Pilih scope agar frontend dan user memahami privilege ini berlaku untuk department, service, procedure, specialty, unit, atau telemedicine.";
        public string LifecycleHelpText { get; set; } = "Create akan membuat status PendingApproval. Gunakan endpoint grant, reject, suspend, atau revoke untuk perubahan status bisnis agar audit trail aman.";
        public string DateFormatHelpText { get; set; } = "Untuk Swagger gunakan format date-time ISO, contoh: 2026-06-05T00:00:00Z.";
    }

    public class CreateWorkforceClinicalPrivilegeRequest
    {
        /// <summary>
        /// Credential/license tenaga medis yang menjadi dasar privilege. Wajib untuk privilege reguler, boleh kosong untuk emergency privilege.
        /// </summary>
        public Guid? CredentialLicenseId { get; set; }

        /// <summary>
        /// Department tempat privilege berlaku. Contoh: IGD, Poliklinik, Bedah, ICU.
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Position/jabatan yang terkait privilege. Jika dikirim bersama DepartmentId, position harus berada di department tersebut.
        /// </summary>
        public Guid? PositionId { get; set; }

        /// <summary>
        /// Nama clinical privilege. Contoh: Pemeriksaan Pasien Rawat Jalan, Tindakan Minor Surgery, Interpretasi EKG.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "PrivilegeName", Description = "Nama clinical privilege. Contoh: Pemeriksaan Pasien Rawat Jalan, Tindakan Minor Surgery, Interpretasi EKG.")]
        public string PrivilegeName { get; set; } = string.Empty;

        /// <summary>
        /// Tipe privilege dari enum ClinicalPrivilegeType. Default: CorePrivilege.
        /// </summary>
        public ClinicalPrivilegeType PrivilegeType { get; set; } = ClinicalPrivilegeType.CorePrivilege;

        /// <summary>
        /// Scope privilege. Nilai: Department, Service, Procedure, Specialty, Unit, Telemedicine, Other.
        /// </summary>
        public WorkforceClinicalScope? ClinicalScope { get; set; }

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

    public class UpdateWorkforceClinicalPrivilegeRequest : CreateWorkforceClinicalPrivilegeRequest
    {
        /// <summary>
        /// Jika true dan SupportingFile dikirim, file lama akan diganti. Jika true tanpa SupportingFile, file lama akan dihapus dari data record.
        /// </summary>
        public bool ReplaceExistingFile { get; set; } = false;
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

    public class DeleteWorkforceClinicalPrivilegeFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
