using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Master Data Tindakan Harian Keperawatan Rawat Inap (Daily Nursing Actions).
    /// Dikelola oleh administrator rumah sakit untuk menentukan butir checklist lembar keperawatan harian per shift.
    /// </summary>
    [Table("MstDailyNursingAction", Schema = "public")]
    public class MstDailyNursingAction : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ActionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string ActionName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DefaultNotes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
