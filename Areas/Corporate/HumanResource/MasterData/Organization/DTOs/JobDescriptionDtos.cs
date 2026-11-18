using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs
{
    public class JobDescriptionItemDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SectionType { get; set; } = "MainDuty";
        // MainDuty, Responsibility, Authority, WorkRelation

        [Range(1, int.MaxValue)]
        public int Sequence { get; set; } = 1;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;
    }

    public class JobDescriptionResponse
    {
        public Guid Id { get; set; }
        public Guid PositionId { get; set; }
        public string PositionCode { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string JobDescriptionCode { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class JobDescriptionDetailResponse : JobDescriptionResponse
    {
        public List<JobDescriptionItemDto> Items { get; set; } = new();
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class JobDescriptionHistoryResponse
    {
        public Guid Id { get; set; }
        public string JobDescriptionCode { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class JobDescriptionSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int DraftData { get; set; }
        public int ArchivedData { get; set; }
        public int TotalPositions { get; set; }
        public int PositionsWithActiveJobDescription { get; set; }
    }

    public class JobDescriptionStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class JobDescriptionFilterMetadataResponse
    {
        public List<JobDescriptionStringOptionResponse> StatusOptions { get; set; } = new();
        public List<JobDescriptionStringOptionResponse> SectionTypeOptions { get; set; } = new();
    }

    public class CreateJobDescriptionDraftRequest
    {
        [Required]
        public Guid PositionId { get; set; }

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Purpose { get; set; } = string.Empty;

        public List<JobDescriptionItemDto> Items { get; set; } = new();
    }

    public class UpdateJobDescriptionDraftRequest
    {
        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Purpose { get; set; } = string.Empty;

        public List<JobDescriptionItemDto> Items { get; set; } = new();
    }

    public class ActivateJobDescriptionRequest
    {
        public DateTime? EffectiveStartDate { get; set; }
    }
}
