using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Penyebab atau faktor yang berhubungan (b.d) untuk suatu diagnosis keperawatan SDKI.
    /// </summary>
    [Table("MstNursingDiagnosisEtiology", Schema = "public")]
    public class MstNursingDiagnosisEtiology : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NursingDiagnosisId { get; set; }

        [Required]
        [MaxLength(300)]
        public string EtiologyName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstNursingDiagnosis? NursingDiagnosis { get; set; }
    }
}
