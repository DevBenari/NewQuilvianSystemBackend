using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.DTOs
{
    public class EmergencyDispositionTypeResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool RequiresDestinationServiceUnit { get; set; }
        public bool RequiresReferralFacility { get; set; }
        public bool ClosesEmergencyVisit { get; set; }
        public bool RequiresAdmissionRequest { get; set; }
        public int Sequence { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyDispositionTypeRequest
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public bool RequiresDestinationServiceUnit { get; set; }

        public bool RequiresReferralFacility { get; set; }

        public bool ClosesEmergencyVisit { get; set; } = true;

        public bool RequiresAdmissionRequest { get; set; }

        public int Sequence { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmergencyDispositionTypeRequest : CreateEmergencyDispositionTypeRequest
    {
    }

    public class EmergencyDispositionTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Sequence { get; set; }
    }

    public class EmergencyDispositionTypeSummaryResponse
    {
        public int TotalDispositionType { get; set; }
        public int ActiveDispositionType { get; set; }
        public int InactiveDispositionType { get; set; }
        public int RequiresDestinationServiceUnit { get; set; }
        public int RequiresReferralFacility { get; set; }
        public int ClosesEmergencyVisit { get; set; }
        public int RequiresAdmissionRequest { get; set; }
    }

    public class EmergencyDispositionTypeDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool? RequiresDestinationServiceUnit { get; set; }
        public bool? RequiresReferralFacility { get; set; }
        public bool? ClosesEmergencyVisit { get; set; }
        public bool? RequiresAdmissionRequest { get; set; }
        public string SortBy { get; set; } = "sequence";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmergencyDispositionTypeFilterMetadataResponse
    {
        public EmergencyDispositionTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> SortableFields { get; set; } = new() { "sequence", "code", "name", "createdatetime" };
    }
}
