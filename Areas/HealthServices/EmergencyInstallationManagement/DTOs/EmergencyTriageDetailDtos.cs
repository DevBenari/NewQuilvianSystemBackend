using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyTriageDetailResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyTriageId { get; set; }
        public Guid? TriageIndicatorId { get; set; }
        public string IndicatorCodeSnapshot { get; set; } = string.Empty;
        public string IndicatorNameSnapshot { get; set; } = string.Empty;
        public string? IndicatorGroupSnapshot { get; set; }
        public string? ObservedValue { get; set; }
        public decimal? Score { get; set; }
        public bool IsMatched { get; set; }
        public string? Notes { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyTriageDetailRequest
    {
        [Required]
        public Guid EmergencyTriageId { get; set; }

        public Guid? TriageIndicatorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string IndicatorCodeSnapshot { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string IndicatorNameSnapshot { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IndicatorGroupSnapshot { get; set; }

        [MaxLength(500)]
        public string? ObservedValue { get; set; }

        public decimal? Score { get; set; }

        public bool IsMatched { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public int Sequence { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyTriageDetailRequest : CreateEmergencyTriageDetailRequest
    {
    }

    public class EmergencyTriageDetailOptionResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyTriageId { get; set; }
        public string IndicatorCodeSnapshot { get; set; } = string.Empty;
        public string IndicatorNameSnapshot { get; set; } = string.Empty;
        public int Sequence { get; set; }
    }

    public class EmergencyTriageDetailSummaryResponse
    {
        public int TotalTriageDetail { get; set; }
        public int ActiveTriageDetail { get; set; }
        public int InactiveTriageDetail { get; set; }
    }

    public class EmergencyTriageDetailDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? EmergencyTriageId { get; set; }
        public Guid? TriageIndicatorId { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "sequence";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmergencyTriageDetailFilterMetadataResponse
    {
        public EmergencyTriageDetailDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> SortableFields { get; set; } = new() { "sequence", "createdatetime" };
    }
}
