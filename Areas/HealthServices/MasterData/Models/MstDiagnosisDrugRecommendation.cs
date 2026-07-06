using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDiagnosisDrugRecommendation", Schema = "public")]
    public class MstDiagnosisDrugRecommendation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DiagnosisId { get; set; }

        [Required]
        public Guid DrugId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecommendationType { get; set; } = "Supportive";
        // FirstLine, Alternative, Symptomatic, Supportive, Conditional

        [MaxLength(500)]
        public string? IndicationText { get; set; }
        // Contoh: diberikan bila demam atau nyeri.

        [MaxLength(250)]
        public string? DoseText { get; set; }
        // Contoh: 500 mg

        [MaxLength(100)]
        public string? Route { get; set; }
        // Contoh: Oral, IV, IM

        [MaxLength(100)]
        public string? Frequency { get; set; }
        // Contoh: 3x sehari

        [MaxLength(100)]
        public string? DurationText { get; set; }
        // Contoh: 3 hari

        [MaxLength(500)]
        public string? CautionNote { get; set; }
        // Catatan kehati-hatian singkat untuk dokter.


        public bool IsActive { get; set; } = true;

        public MstDiagnosis? Diagnosis { get; set; }

        public MstDrug? Drug { get; set; }
    }
}
