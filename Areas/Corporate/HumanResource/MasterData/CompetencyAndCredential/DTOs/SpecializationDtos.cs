using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs
{
    public class SpecializationSummaryResponse
    {
        public int TotalSpecialization { get; set; }
        public int ActiveSpecialization { get; set; }
        public int InactiveSpecialization { get; set; }
        public int ClinicalSpecialization { get; set; }
        public int CredentialingRequiredSpecialization { get; set; }
        public int RootSpecialization { get; set; }
        public int SubSpecialization { get; set; }
    }

    public class SpecializationResponse
    {
        public Guid Id { get; set; }
        public Guid ProfessionId { get; set; }
        public string ProfessionCode { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;
        public Guid? ParentSpecializationId { get; set; }
        public string? ParentSpecializationCode { get; set; }
        public string? ParentSpecializationName { get; set; }
        public string SpecializationCode { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public string SpecializationType { get; set; } = string.Empty;
        public bool IsClinicalSpecialization { get; set; }
        public bool RequiresCredentialing { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int ChildSpecializationCount { get; set; }
        public int ClinicalPrivilegeCatalogCount { get; set; }
        public int CredentialingRequirementCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class SpecializationDetailResponse : SpecializationResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class SpecializationOptionResponse
    {
        public Guid Id { get; set; }
        public Guid ProfessionId { get; set; }
        public string ProfessionName { get; set; } = string.Empty;
        public Guid? ParentSpecializationId { get; set; }
        public string SpecializationCode { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public string SpecializationType { get; set; } = string.Empty;
        public bool IsClinicalSpecialization { get; set; }
    }

    public class SpecializationOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<SpecializationOptionResponse> Items { get; set; } = new();
    }

    public class SpecializationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public SpecializationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<SpecializationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<SpecializationStringOptionResponse> SpecializationTypeOptions { get; set; } = new();
        public List<SpecializationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class SpecializationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? ParentSpecializationId { get; set; }
        public string? SpecializationType { get; set; }
        public bool? IsClinicalSpecialization { get; set; }
        public bool? RequiresCredentialing { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "specializationName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class SpecializationCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SpecializationStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SpecializationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateSpecializationRequest
    {
        [Required]
        public Guid ProfessionId { get; set; }

        public Guid? ParentSpecializationId { get; set; }

        [Required, MaxLength(200)]
        public string SpecializationName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string SpecializationType { get; set; } = "Specialization";

        public bool IsClinicalSpecialization { get; set; } = true;
        public bool RequiresCredentialing { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateSpecializationRequest : CreateSpecializationRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSpecializationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SpecializationCreateResponse
    {
        public Guid Id { get; set; }
        public Guid ProfessionId { get; set; }
        public string SpecializationCode { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public string SpecializationType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
