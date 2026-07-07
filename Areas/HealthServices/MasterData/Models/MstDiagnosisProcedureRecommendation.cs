using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDiagnosisProcedureRecommendation", Schema = "public")]
    public class MstDiagnosisProcedureRecommendation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DiagnosisId { get; set; }

        public Guid? ProcedureId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecommendationType { get; set; } = "Procedure";
        // Procedure, Lab, Radiology, Monitoring, Referral, FollowUp

        [Required]
        [MaxLength(250)]
        public string RecommendationName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? InstructionText { get; set; }


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

        public MstProcedure? Procedure { get; set; }
    }
}
