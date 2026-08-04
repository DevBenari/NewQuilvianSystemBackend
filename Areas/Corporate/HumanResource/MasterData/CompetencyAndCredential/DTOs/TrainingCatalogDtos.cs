using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs
{
    public class TrainingCatalogSummaryResponse
    {
        public int TotalTrainingCatalog { get; set; }
        public int ActiveTrainingCatalog { get; set; }
        public int InactiveTrainingCatalog { get; set; }
        public int MandatoryTrainingCatalog { get; set; }
        public int AssessmentRequiredCatalog { get; set; }
        public int CertificateIssuingCatalog { get; set; }
        public int ExternalTrainingCatalog { get; set; }
    }

    public class TrainingCatalogResponse
    {
        public Guid Id { get; set; }
        public Guid TrainingCategoryId { get; set; }
        public string TrainingCategoryCode { get; set; } = string.Empty;
        public string TrainingCategoryName { get; set; } = string.Empty;
        public Guid? CertificationTypeId { get; set; }
        public string? CertificationTypeCode { get; set; }
        public string? CertificationTypeName { get; set; }
        public string TrainingCode { get; set; } = string.Empty;
        public string TrainingName { get; set; } = string.Empty;
        public string TrainingType { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = string.Empty;
        public string? DefaultProviderName { get; set; }
        public decimal DurationHours { get; set; }
        public int? ValidityMonths { get; set; }
        public bool IsMandatory { get; set; }
        public bool RequiresAssessment { get; set; }
        public decimal? MinimumPassingScore { get; set; }
        public bool IssuesCertificate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int MandatoryTrainingRuleCount { get; set; }
        public int WorkforceTrainingRecordCount { get; set; }
        public int CredentialingRequirementCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class TrainingCatalogDetailResponse : TrainingCatalogResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class TrainingCatalogOptionResponse
    {
        public Guid Id { get; set; }
        public Guid TrainingCategoryId { get; set; }
        public string TrainingCategoryName { get; set; } = string.Empty;
        public string TrainingCode { get; set; } = string.Empty;
        public string TrainingName { get; set; } = string.Empty;
        public string TrainingType { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = string.Empty;
        public decimal DurationHours { get; set; }
        public int? ValidityMonths { get; set; }
        public bool IsMandatory { get; set; }
        public bool RequiresAssessment { get; set; }
        public decimal? MinimumPassingScore { get; set; }
        public bool IssuesCertificate { get; set; }
    }

    public class TrainingCatalogOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<TrainingCatalogOptionResponse> Items { get; set; } = new();
    }

    public class TrainingCatalogFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public TrainingCatalogDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<TrainingCatalogStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<TrainingCatalogStringOptionResponse> TrainingTypeOptions { get; set; } = new();
        public List<TrainingCatalogStringOptionResponse> DeliveryMethodOptions { get; set; } = new();
        public List<TrainingCatalogSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class TrainingCatalogDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? TrainingCategoryId { get; set; }
        public Guid? CertificationTypeId { get; set; }
        public string? TrainingType { get; set; }
        public string? DeliveryMethod { get; set; }
        public bool? IsMandatory { get; set; }
        public bool? RequiresAssessment { get; set; }
        public bool? IssuesCertificate { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "trainingName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class TrainingCatalogStringOptionResponse { public string Value { get; set; } = string.Empty; public string Label { get; set; } = string.Empty; }
    public class TrainingCatalogSortOptionResponse { public string Value { get; set; } = string.Empty; public string Label { get; set; } = string.Empty; }

    public class CreateTrainingCatalogRequest
    {
        [Required] public Guid TrainingCategoryId { get; set; }
        public Guid? CertificationTypeId { get; set; }
        [Required, MaxLength(200)] public string TrainingName { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string TrainingType { get; set; } = "Internal";
        [Required, MaxLength(50)] public string DeliveryMethod { get; set; } = "Classroom";
        [MaxLength(200)] public string? DefaultProviderName { get; set; }
        [Range(typeof(decimal), "0", "999999.99")] public decimal DurationHours { get; set; }
        [Range(1, int.MaxValue)] public int? ValidityMonths { get; set; }
        public bool IsMandatory { get; set; }
        public bool RequiresAssessment { get; set; }
        [Range(typeof(decimal), "0", "100")] public decimal? MinimumPassingScore { get; set; }
        public bool IssuesCertificate { get; set; }
        [MaxLength(1000)] public string? Description { get; set; }
    }

    public class UpdateTrainingCatalogRequest : CreateTrainingCatalogRequest { public bool IsActive { get; set; } = true; }
    public class UpdateTrainingCatalogStatusRequest { public bool IsActive { get; set; } }
    public class TrainingCatalogCreateResponse
    {
        public Guid Id { get; set; }
        public string TrainingCode { get; set; } = string.Empty;
        public string TrainingName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
