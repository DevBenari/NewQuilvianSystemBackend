using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.DTOs
{
    public class EmergencyTriageIndicatorResponse
    {
        public Guid Id { get; set; }
        public Guid TriageLevelId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? IndicatorGroup { get; set; }
        public int Sequence { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyTriageIndicatorRequest
    {
        [Required]
        public Guid TriageLevelId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IndicatorGroup { get; set; }

        public int Sequence { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmergencyTriageIndicatorRequest : CreateEmergencyTriageIndicatorRequest
    {
    }

    public class EmergencyTriageIndicatorOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Sequence { get; set; }
    }

    public class EmergencyTriageIndicatorSummaryResponse
    {
        public int TotalIndicator { get; set; }
        public int ActiveIndicator { get; set; }
        public int InactiveIndicator { get; set; }
    }

    public class EmergencyTriageIndicatorDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? TriageLevelId { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "sequence";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmergencyTriageIndicatorFilterMetadataResponse
    {
        public EmergencyTriageIndicatorDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> SortableFields { get; set; } = new() { "sequence", "code", "name", "createdatetime" };
    }
}
