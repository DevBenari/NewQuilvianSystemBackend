using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs
{
    public class HmdPrescriptionPagedQuery
    {
        public Guid? EpisodeId { get; set; }
        public HmdPrescriptionStatus? PrescriptionStatus { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Parameter teknis resep HD. Dipakai bersama oleh pembuatan dan penyuntingan draf.</summary>
    public class HmdPrescriptionParameters
    {
        [Required]
        public DateOnly EffectiveDate { get; set; }

        [Range(0, 14)]
        public int FrequencyPerWeek { get; set; }

        [Range(0, 1440)]
        public int TargetDurationMinutes { get; set; }

        [Range(0, 20000)]
        public int? TargetUltrafiltrationMl { get; set; }

        [Range(0, 1000)]
        public int? BloodFlowRate { get; set; }

        [Range(0, 2000)]
        public int? DialysateFlowRate { get; set; }

        [MaxLength(100)]
        public string? DialyzerType { get; set; }

        [MaxLength(200)]
        public string? DialysateComposition { get; set; }

        [MaxLength(200)]
        public string? SodiumBicarbonateProfile { get; set; }

        [Range(typeof(decimal), "30", "42")]
        public decimal? DialysateTemperatureC { get; set; }

        [MaxLength(500)]
        public string? AnticoagulantPlan { get; set; }

        public Guid? VascularAccessId { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }
    }

    public class CreateHmdPrescriptionRequest : HmdPrescriptionParameters
    {
        [Required]
        public Guid EpisodeId { get; set; }
    }

    public class UpdateHmdPrescriptionRequest : HmdPrescriptionParameters
    {
    }

    public class CancelHmdPrescriptionRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class HmdPrescriptionResponse
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public string? EpisodeNumber { get; set; }
        public Guid PrescribingDoctorId { get; set; }
        public string? PrescribingDoctorName { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public int FrequencyPerWeek { get; set; }
        public int TargetDurationMinutes { get; set; }
        public int? TargetUltrafiltrationMl { get; set; }
        public int? BloodFlowRate { get; set; }
        public int? DialysateFlowRate { get; set; }
        public string? DialyzerType { get; set; }
        public string? DialysateComposition { get; set; }
        public string? SodiumBicarbonateProfile { get; set; }
        public decimal? DialysateTemperatureC { get; set; }
        public string? AnticoagulantPlan { get; set; }
        public Guid? VascularAccessId { get; set; }
        public string? VascularAccessSite { get; set; }
        public string? ClinicalNote { get; set; }
        public HmdPrescriptionStatus PrescriptionStatus { get; set; }
        public string PrescriptionStatusName { get; set; } = string.Empty;
        public Guid? SupersededByPrescriptionId { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public Guid? ActivatedByUserId { get; set; }
        public string? CancelReason { get; set; }
        public DateTime CreateDateTime { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }
}
