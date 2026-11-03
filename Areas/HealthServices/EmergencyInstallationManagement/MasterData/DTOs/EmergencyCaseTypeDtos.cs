using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.DTOs
{
    public class EmergencyCaseTypeResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyCaseTypeRequest
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int Sequence { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmergencyCaseTypeRequest : CreateEmergencyCaseTypeRequest
    {
    }

    public class EmergencyCaseTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Sequence { get; set; }
    }

    public class EmergencyCaseTypeSummaryResponse
    {
        public int TotalCaseType { get; set; }
        public int ActiveCaseType { get; set; }
        public int InactiveCaseType { get; set; }
    }

    public class EmergencyCaseTypeDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "sequence";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmergencyCaseTypeFilterMetadataResponse
    {
        public EmergencyCaseTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> SortableFields { get; set; } = new() { "sequence", "code", "name", "createdatetime" };
    }
}
