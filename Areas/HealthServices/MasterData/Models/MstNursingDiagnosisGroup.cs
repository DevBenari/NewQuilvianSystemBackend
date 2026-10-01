using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Kategori / Kelompok besar Standar Diagnosis Keperawatan Indonesia (SDKI),
    /// misalnya: Fisiologis, Psikologis, Perilaku, Relasional, Lingkungan.
    /// </summary>
    [Table("MstNursingDiagnosisGroup", Schema = "public")]
    public class MstNursingDiagnosisGroup : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string GroupCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstNursingDiagnosis> Diagnoses { get; set; } = new List<MstNursingDiagnosis>();
    }
}
