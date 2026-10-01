using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Standar Intervensi Keperawatan Indonesia (SIKI) resmi PPNI.
    /// Dikelompokkan dalam 4 pilar utama: 1 = Observasi, 2 = Terapeutik, 3 = Edukasi, 4 = Kolaborasi.
    /// </summary>
    [Table("MstNursingDiagnosisIntervention", Schema = "public")]
    public class MstNursingDiagnosisIntervention : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NursingDiagnosisId { get; set; }

        [Required]
        public int PillarType { get; set; } // 1: Observasi, 2: Terapeutik, 3: Edukasi, 4: Kolaborasi

        [Required]
        [MaxLength(50)]
        public string InterventionCode { get; set; } = string.Empty; // misal: I.08238

        [Required]
        [MaxLength(300)]
        public string InterventionName { get; set; } = string.Empty; // misal: Manajemen Nyeri

        [Required]
        public string ActionDescription { get; set; } = string.Empty; // Detail tindakan keperawatan

        [Required]
        [MaxLength(50)]
        public string TerminologySystem { get; set; } = "SIKI";

        public bool IsDefaultRecommendation { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public MstNursingDiagnosis? NursingDiagnosis { get; set; }
    }
}
