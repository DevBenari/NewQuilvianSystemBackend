using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs
{
    public class LicenseTypeSummaryResponse
    {
        public int TotalLicenseType { get; set; }
        public int ActiveLicenseType { get; set; }
        public int InactiveLicenseType { get; set; }
        public int ExpiryRequiredType { get; set; }
        public int RenewableType { get; set; }
        public int DocumentRequiredType { get; set; }
        public int VerificationRequiredType { get; set; }
    }

    public class LicenseTypeResponse
    {
        public Guid Id { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionCode { get; set; }
        public string? ProfessionName { get; set; }
        public string LicenseTypeCode { get; set; } = string.Empty;
        public string LicenseTypeName { get; set; } = string.Empty;
        public string? IssuingAuthority { get; set; }
        public string? RegulatoryBody { get; set; }
        public int? DefaultValidityMonths { get; set; }
        public bool RequiresExpiryDate { get; set; }
        public bool IsRenewable { get; set; }
        public bool RequiresDocument { get; set; }
        public bool RequiresVerification { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int WorkforceLicenseCount { get; set; }
        public int CredentialingRequirementCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LicenseTypeDetailResponse : LicenseTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LicenseTypeOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public string LicenseTypeCode { get; set; } = string.Empty;
        public string LicenseTypeName { get; set; } = string.Empty;
        public string? IssuingAuthority { get; set; }
        public string? RegulatoryBody { get; set; }
        public int? DefaultValidityMonths { get; set; }
        public bool RequiresExpiryDate { get; set; }
        public bool RequiresDocument { get; set; }
        public bool RequiresVerification { get; set; }
    }

    public class LicenseTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LicenseTypeOptionResponse> Items { get; set; } = new();
    }

    public class LicenseTypeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public LicenseTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LicenseTypeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<LicenseTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LicenseTypeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? ProfessionId { get; set; }
        public bool? RequiresExpiryDate { get; set; }
        public bool? IsRenewable { get; set; }
        public bool? RequiresDocument { get; set; }
        public bool? RequiresVerification { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "licenseTypeName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LicenseTypeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LicenseTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateLicenseTypeRequest
    {
        public Guid? ProfessionId { get; set; }

        [Required, MaxLength(200)]
        public string LicenseTypeName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? IssuingAuthority { get; set; }

        [MaxLength(200)]
        public string? RegulatoryBody { get; set; }

        [Range(1, int.MaxValue)]
        public int? DefaultValidityMonths { get; set; }

        public bool RequiresExpiryDate { get; set; } = true;
        public bool IsRenewable { get; set; } = true;
        public bool RequiresDocument { get; set; } = true;
        public bool RequiresVerification { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateLicenseTypeRequest : CreateLicenseTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLicenseTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class LicenseTypeCreateResponse
    {
        public Guid Id { get; set; }
        public string LicenseTypeCode { get; set; } = string.Empty;
        public string LicenseTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
