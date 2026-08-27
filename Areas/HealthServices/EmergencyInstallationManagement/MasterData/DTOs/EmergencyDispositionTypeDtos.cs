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
}
