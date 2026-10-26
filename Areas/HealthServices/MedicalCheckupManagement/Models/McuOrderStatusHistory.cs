using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>Satu perpindahan status order, termasuk status awalnya.</summary>
    [Table("McuOrderStatusHistory", Schema = "public")]
    public class McuOrderStatusHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid McuOrderId { get; set; }

        public McuOrder? McuOrder { get; set; }

        /// <summary>Kosong untuk status awal.</summary>
        public McuOrderStatus? FromStatus { get; set; }

        public McuOrderStatus ToStatus { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Kosong bila perpindahan dilakukan sistem, contoh tutup hari.</summary>
        public Guid? ChangedByUserId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
