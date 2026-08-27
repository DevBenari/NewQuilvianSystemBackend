using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.DTOs
{
    public class EmergencyArrivalModeResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsAmbulance { get; set; }
        public bool IsReferral { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyArrivalModeRequest
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsAmbulance { get; set; }

        public bool IsReferral { get; set; }

        public int Sequence { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyArrivalModeRequest : CreateEmergencyArrivalModeRequest
    {
    }

    public class EmergencyArrivalModeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Sequence { get; set; }
    }
}
