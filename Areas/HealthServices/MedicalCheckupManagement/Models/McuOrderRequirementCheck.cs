using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Penandaan satu persyaratan komponen saat check-in, contoh puasa: terpenuhi, lanjut dengan
    /// catatan, atau komponennya dijadwal ulang.
    /// </summary>
    [Table("McuOrderRequirementCheck", Schema = "public")]
    public class McuOrderRequirementCheck : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid McuOrderId { get; set; }

        public McuOrder? McuOrder { get; set; }

        [Required]
        public Guid McuOrderComponentId { get; set; }

        public McuOrderComponent? McuOrderComponent { get; set; }

        [Required]
        [MaxLength(300)]
        public string RequirementTextSnapshot { get; set; } = string.Empty;

        public McuRequirementOutcome Outcome { get; set; }

        /// <summary>
        /// SENSITIF. Wajib bila lanjut dengan catatan, contoh <i>Mengaku sudah sarapan pukul 06.00</i>.
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        [Required]
        public Guid CheckedByUserId { get; set; }

        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}
