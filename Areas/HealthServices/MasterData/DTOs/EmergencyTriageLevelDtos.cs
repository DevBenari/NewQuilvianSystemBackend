using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.DTOs
{
    public class EmergencyTriageLevelResponse
    {
        public Guid Id { get; set; }
        public EmergencyTriageSystem TriageSystem { get; set; }
        public int Level { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public int? MaxWaitingMinutes { get; set; }
        public bool AllowsTreatmentBeforeRegistration { get; set; }
        public int Sequence { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyTriageLevelRequest
    {
        public EmergencyTriageSystem TriageSystem { get; set; } = EmergencyTriageSystem.ATS;

        /// <summary>
        /// 1 sampai 5 untuk skala antrean biasa; 0 khusus kategori Hitam yang berada di
        /// luar skala antrean.
        /// </summary>
        [Range(0, 5)]
        public int Level { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ColorName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ColorHex { get; set; }

        public int? MaxWaitingMinutes { get; set; }

        public bool AllowsTreatmentBeforeRegistration { get; set; }

        public int Sequence { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyTriageLevelRequest : CreateEmergencyTriageLevelRequest
    {
    }

    public class EmergencyTriageLevelOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public EmergencyTriageSystem TriageSystem { get; set; }
        public int Sequence { get; set; }
    }
}
