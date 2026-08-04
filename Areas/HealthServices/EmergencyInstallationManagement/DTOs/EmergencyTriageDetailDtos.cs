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
}
