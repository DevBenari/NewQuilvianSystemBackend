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


        public bool IsActive { get; set; } = true;

        public MstDiagnosis? Diagnosis { get; set; }
    }
}
