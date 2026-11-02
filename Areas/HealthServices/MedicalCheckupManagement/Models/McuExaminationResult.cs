using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Hasil satu komponen Pemeriksaan MCU (komponen ber-<c>ComponentType</c> 3). Satu komponen
    /// paling banyak satu hasil; nilai per isian ada di <see cref="McuExaminationResultValue"/>.
    /// </summary>
    [Table("McuExaminationResult", Schema = "public")]
    public class McuExaminationResult : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid McuOrderComponentId { get; set; }

        public McuOrderComponent? McuOrderComponent { get; set; }

        /// <summary>SENSITIF. Contoh <i>EKG: irama sinus, kesan normal</i>.</summary>
        [MaxLength(2000)]
        public string? ConclusionText { get; set; }

        [Required]
        public Guid FilledByUserId { get; set; }

        public DateTime FilledAt { get; set; } = DateTime.UtcNow;

        public ICollection<McuExaminationResultValue> Values { get; set; } = new List<McuExaminationResultValue>();
    }
}
