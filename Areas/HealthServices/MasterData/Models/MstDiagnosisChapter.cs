using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDiagnosisChapter", Schema = "public")]
    public class MstDiagnosisChapter : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ChapterCode { get; set; } = string.Empty;
        // Contoh: I, II, III, IV, V

        [Required]
        [MaxLength(250)]
        public string ChapterName { get; set; } = string.Empty;
        // Contoh: Certain infectious and parasitic diseases

        [MaxLength(50)]
        public string? DiagnosisCodeRangeStart { get; set; }
        // Contoh: A00

        [MaxLength(50)]
        public string? DiagnosisCodeRangeEnd { get; set; }
        // Contoh: B99

        [Required]
        [MaxLength(100)]
        public string IcdVersion { get; set; } = "ICD-10";


        public bool IsActive { get; set; } = true;

        public ICollection<MstDiagnosis> Diagnoses { get; set; } = new List<MstDiagnosis>();
    }
}
