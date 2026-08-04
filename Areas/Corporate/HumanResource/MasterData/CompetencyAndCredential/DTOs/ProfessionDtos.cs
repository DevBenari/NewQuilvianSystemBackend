using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs
{
    public class ProfessionSummaryResponse
    {
        public int TotalProfession { get; set; }
        public int ActiveProfession { get; set; }
        public int InactiveProfession { get; set; }
        public int ClinicalProfession { get; set; }
        public int CredentialingRequiredProfession { get; set; }
        public int LicenseRequiredProfession { get; set; }
        public int ProfessionWithSpecialization { get; set; }
    }

    public class ProfessionResponse
    {
        public Guid Id { get; set; }
        public string ProfessionCode { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;
        public string ProfessionGroup { get; set; } = string.Empty;
        public bool IsClinicalProfession { get; set; }
        public bool RequiresCredentialing { get; set; }
        public bool RequiresLicense { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int SpecializationCount { get; set; }
        public int CertificationTypeCount { get; set; }
        public int LicenseTypeCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class ProfessionDetailResponse : ProfessionResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class ProfessionOptionResponse
    {
        public Guid Id { get; set; }
        public string ProfessionCode { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;
        public string ProfessionGroup { get; set; } = string.Empty;
        public bool IsClinicalProfession { get; set; }
        public bool RequiresCredentialing { get; set; }
        public bool RequiresLicense { get; set; }
    }

    public class ProfessionOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<ProfessionOptionResponse> Items { get; set; } = new();
    }

    public class ProfessionFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public ProfessionDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ProfessionCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<ProfessionStringOptionResponse> ProfessionGroupOptions { get; set; } = new();
        public List<ProfessionSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ProfessionDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? ProfessionGroup { get; set; }
        public bool? IsClinicalProfession { get; set; }
        public bool? RequiresCredentialing { get; set; }
        public bool? RequiresLicense { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "professionName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ProfessionCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ProfessionStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ProfessionSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateProfessionRequest
    {
        [Required, MaxLength(200)]
        public string ProfessionName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ProfessionGroup { get; set; } = "General";

        public bool IsClinicalProfession { get; set; }
        public bool RequiresCredentialing { get; set; }
        public bool RequiresLicense { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateProfessionRequest : CreateProfessionRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProfessionStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class ProfessionCreateResponse
    {
        public Guid Id { get; set; }
        public string ProfessionCode { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;
        public string ProfessionGroup { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
