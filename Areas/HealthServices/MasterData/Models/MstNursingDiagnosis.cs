using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Katalog Standar Diagnosis Keperawatan Indonesia (SDKI) resmi PPNI.
    /// Menyediakan master terminologi diagnosis terstandar untuk rawat inap (CAP-013, FR-KEP-013).
    /// </summary>
    [Table("MstNursingDiagnosis", Schema = "public")]
    public class MstNursingDiagnosis : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? GroupId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty; // misal: D.0077

        [Required]
        [MaxLength(300)]
        public string Name { get; set; } = string.Empty; // misal: Nyeri Akut

        [MaxLength(100)]
        public string? SubCategory { get; set; } // misal: Nyeri dan Kenyamanan

        public string? Definition { get; set; }

        [Required]
        [MaxLength(50)]
        public string TerminologySystem { get; set; } = "SDKI";

        public bool IsActive { get; set; } = true;

        public MstNursingDiagnosisGroup? Group { get; set; }

        public ICollection<MstNursingDiagnosisEtiology> Etiologies { get; set; } = new List<MstNursingDiagnosisEtiology>();

        public ICollection<MstNursingDiagnosisOutcome> Outcomes { get; set; } = new List<MstNursingDiagnosisOutcome>();

        public ICollection<MstNursingDiagnosisIntervention> Interventions { get; set; } = new List<MstNursingDiagnosisIntervention>();
    }
}
