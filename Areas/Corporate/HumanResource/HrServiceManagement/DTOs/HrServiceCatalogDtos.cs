using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs
{
    public class HrServiceCategoryDto
    {
        public Guid Id { get; set; }
        public string ServiceCategoryCode { get; set; } = string.Empty;
        public string ServiceCategoryName { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public string? DisplayColor { get; set; }
        public int DefaultSlaHours { get; set; }
        public int SortOrder { get; set; }
        public bool IsEmployeeVisible { get; set; }
        public bool IsManagerVisible { get; set; }
        public bool IsConfidentialByDefault { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public int TotalServiceTypes { get; set; }
    }

    public class CreateHrServiceCategoryRequestDto
    {
        [Required(ErrorMessage = "Kode kategori wajib diisi")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kode kategori 3-50 karakter")]
        public string ServiceCategoryCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama kategori wajib diisi")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Nama kategori 3-200 karakter")]
        public string ServiceCategoryName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? IconName { get; set; }

        [StringLength(20)]
        public string? DisplayColor { get; set; }

        [Range(1, 168, ErrorMessage = "Target SLA default harus antara 1 s.d. 168 jam")]
        public int DefaultSlaHours { get; set; } = 24;

        public int SortOrder { get; set; } = 0;
        public bool IsEmployeeVisible { get; set; } = true;
        public bool IsManagerVisible { get; set; } = true;
        public bool IsConfidentialByDefault { get; set; } = false;

        [StringLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateHrServiceCategoryRequestDto
    {
        [Required(ErrorMessage = "Nama kategori wajib diisi")]
        [StringLength(200, MinimumLength = 3)]
        public string ServiceCategoryName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? IconName { get; set; }

        [StringLength(20)]
        public string? DisplayColor { get; set; }

        [Range(1, 168)]
        public int DefaultSlaHours { get; set; } = 24;

        public int SortOrder { get; set; } = 0;
        public bool IsEmployeeVisible { get; set; } = true;
        public bool IsManagerVisible { get; set; } = true;
        public bool IsConfidentialByDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        public string? Description { get; set; }
    }

    public class HrServiceTypeDto
    {
        public Guid Id { get; set; }
        public Guid HrServiceCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ServiceTypeCode { get; set; } = string.Empty;
        public string ServiceTypeName { get; set; } = string.Empty;
        public string RequestClassification { get; set; } = "General";
        public string DefaultPriority { get; set; } = "Normal";
        public int DefaultSlaHours { get; set; }
        public int? FirstResponseSlaHours { get; set; }
        public int? ResolutionSlaHours { get; set; }
        public string? DefaultAssignedRoleCode { get; set; }
        public string AssignmentSource { get; set; } = "SiteHr";
        public bool RequiresWorkflow { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool AllowsEmployeeComment { get; set; }
        public bool AllowsInternalComment { get; set; }
        public bool AutoCreateDocumentRequest { get; set; }
        public bool IsEmployeeSelectable { get; set; }
        public bool IsConfidential { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string? FormSchemaJson { get; set; }
        public string? ValidationRuleJson { get; set; }
        public Guid? EmployeeDocumentTypeId { get; set; }
        public string? DocumentTypeName { get; set; }
    }

    public class CreateHrServiceTypeRequestDto
    {
        [Required]
        public Guid HrServiceCategoryId { get; set; }

        [Required(ErrorMessage = "Kode tipe layanan wajib diisi")]
        [StringLength(50, MinimumLength = 3)]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama tipe layanan wajib diisi")]
        [StringLength(200, MinimumLength = 3)]
        public string ServiceTypeName { get; set; } = string.Empty;

        [StringLength(50)]
        public string RequestClassification { get; set; } = "General";

        [StringLength(50)]
        public string DefaultPriority { get; set; } = "Normal";

        [Range(1, 168, ErrorMessage = "SLA target harus antara 1 s.d. 168 jam (VR-HRS-03)")]
        public int DefaultSlaHours { get; set; } = 24;

        public int? FirstResponseSlaHours { get; set; } = 4;
        public int? ResolutionSlaHours { get; set; } = 24;

        [StringLength(100)]
        public string? DefaultAssignedRoleCode { get; set; }

        [StringLength(50)]
        public string AssignmentSource { get; set; } = "SiteHr";

        public bool RequiresWorkflow { get; set; } = false;
        public bool RequiresAttachment { get; set; } = false;
        public bool AllowsEmployeeComment { get; set; } = true;
        public bool AllowsInternalComment { get; set; } = true;
        public bool AutoCreateDocumentRequest { get; set; } = false;
        public bool IsEmployeeSelectable { get; set; } = true;
        public bool IsConfidential { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? FormSchemaJson { get; set; }
        public string? ValidationRuleJson { get; set; }
        public Guid? EmployeeDocumentTypeId { get; set; }
    }

    public class UpdateHrServiceTypeRequestDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string ServiceTypeName { get; set; } = string.Empty;

        [Range(1, 168, ErrorMessage = "SLA target harus antara 1 s.d. 168 jam")]
        public int DefaultSlaHours { get; set; } = 24;

        public int? FirstResponseSlaHours { get; set; }
        public int? ResolutionSlaHours { get; set; }

        [StringLength(100)]
        public string? DefaultAssignedRoleCode { get; set; }

        public bool RequiresWorkflow { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool AllowsEmployeeComment { get; set; }
        public bool AllowsInternalComment { get; set; }
        public bool AutoCreateDocumentRequest { get; set; }
        public bool IsEmployeeSelectable { get; set; }
        public bool IsConfidential { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? FormSchemaJson { get; set; }
        public string? ValidationRuleJson { get; set; }
        public Guid? EmployeeDocumentTypeId { get; set; }
    }

    public class EmployeeDocumentTypeDto
    {
        public Guid Id { get; set; }
        public string DocumentTypeCode { get; set; } = string.Empty;
        public string DocumentTypeName { get; set; } = string.Empty;
        public string DocumentCategory { get; set; } = "Employment";
        public string NumberingPattern { get; set; } = string.Empty;
        public int LastSequenceNumber { get; set; }
        public int CurrentYear { get; set; }
        public string? TemplateBodyHtml { get; set; }
        public string? SupportedTokensJson { get; set; }
        public int ValidityPeriodDays { get; set; }
        public bool IsFinancialSensitive { get; set; }
        public bool RequiresCbApproval { get; set; }
        public string AuthorizedSignerTitle { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; }
        public bool RequiresDigitalSignature { get; set; }
        public bool AllowsEmployeeDownload { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEmployeeDocumentTypeRequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string DocumentTypeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string DocumentTypeName { get; set; } = string.Empty;

        [StringLength(50)]
        public string DocumentCategory { get; set; } = "Employment";

        [StringLength(100)]
        public string NumberingPattern { get; set; } = "[KODE]/RSQ-SDM/[BULAN_ROMAWI]/[TAHUN]/[NO_URUT]";

        public string? TemplateBodyHtml { get; set; }
        public string? SupportedTokensJson { get; set; } = "[]";

        [Range(1, 365)]
        public int ValidityPeriodDays { get; set; } = 90;

        public bool IsFinancialSensitive { get; set; } = false;
        public bool RequiresCbApproval { get; set; } = false;

        [StringLength(150)]
        public string AuthorizedSignerTitle { get; set; } = "Manajer Sumber Daya Manusia";

        public bool RequiresApproval { get; set; } = true;
        public bool RequiresDigitalSignature { get; set; } = true;
        public bool AllowsEmployeeDownload { get; set; } = true;
    }
}
