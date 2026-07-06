using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDiagnosis", Schema = "public")]
    public class MstDiagnosis : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? DiagnosisChapterId { get; set; }
        // Optional background grouping berdasarkan chapter ICD.

        public Guid? ParentDiagnosisId { get; set; }
        // Contoh: A00 sebagai parent dari A00.0, A00.1, A00.9.

        [Required]
        [MaxLength(50)]
        public string DiagnosisCode { get; set; } = string.Empty;
        // Contoh: A00, A00.0, I10, E11.9

        [Required]
        [MaxLength(500)]
        public string DiagnosisName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DiagnosisType { get; set; } = "ICD10";
        // ICD10, Local, Custom

        [Required]
        [MaxLength(100)]
        public string IcdVersion { get; set; } = "ICD-10";

        public bool IsSelectableForClinicalUse { get; set; } = true;
        // Jika false, data hanya sebagai parent/group dan tidak dipilih dokter.

        public bool IsPrimaryDiagnosisAllowed { get; set; } = true;

        public bool IsSecondaryDiagnosisAllowed { get; set; } = true;


        public bool IsActive { get; set; } = true;

        public MstDiagnosisChapter? DiagnosisChapter { get; set; }

        public MstDiagnosis? ParentDiagnosis { get; set; }

        public ICollection<MstDiagnosis> ChildDiagnoses { get; set; } = new List<MstDiagnosis>();

        public ICollection<MstDiagnosisDrugRecommendation> DrugRecommendations { get; set; } = new List<MstDiagnosisDrugRecommendation>();

        public ICollection<MstDiagnosisProcedureRecommendation> ProcedureRecommendations { get; set; } = new List<MstDiagnosisProcedureRecommendation>();

        public ICollection<MstDiagnosisEducationRecommendation> EducationRecommendations { get; set; } = new List<MstDiagnosisEducationRecommendation>();
    }
}
