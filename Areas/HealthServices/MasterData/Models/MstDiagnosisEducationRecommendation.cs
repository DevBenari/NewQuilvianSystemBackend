using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDiagnosisEducationRecommendation", Schema = "public")]
    public class MstDiagnosisEducationRecommendation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DiagnosisId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EducationType { get; set; } = "General";
        // General, Diet, Activity, MedicationUse, WarningSign, FollowUp, Prevention

        [Required]
        [MaxLength(250)]
        public string EducationTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string EducationText { get; set; } = string.Empty;


        [Required]
        [MaxLength(50)]
        public string ReviewStatus { get; set; } = "DraftFromLiterature";
        // DraftFromLiterature, DoctorReviewed, ActiveForSoap, Inactive

        [MaxLength(50)]
        public string? SourceType { get; set; }
        // PNPK, Fornas, PPK_RS, ManualDoctorInput, Other

        [MaxLength(300)]
        public string? SourceTitle { get; set; }

        [MaxLength(20)]
        public string? SourceYear { get; set; }

        [MaxLength(1000)]
        public string? SourceUrl { get; set; }

        [MaxLength(1000)]
        public string? SourceNote { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(1000)]
        public string? ReviewNote { get; set; }

        public Guid? ActivatedByUserId { get; set; }

        public DateTime? ActivatedAt { get; set; }

        [MaxLength(1000)]
        public string? ActivationNote { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDiagnosis? Diagnosis { get; set; }
    }
}
