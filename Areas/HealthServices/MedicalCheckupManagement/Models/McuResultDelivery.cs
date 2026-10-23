using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Catatan satu penyerahan hasil: kepada siapa, isi apa, lewat kanal apa, versi resume mana.
    /// Bila resume kemudian dibuka ulang, penyerahan lama ditandai perlu pemberitahuan koreksi.
    /// </summary>
    [Table("McuResultDelivery", Schema = "public")]
    public class McuResultDelivery : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid McuOrderId { get; set; }

        public McuOrder? McuOrder { get; set; }

        /// <summary>Versi resume terverifikasi yang diserahkan.</summary>
        [Required]
        public Guid McuResumeId { get; set; }

        public McuResume? McuResume { get; set; }

        public McuDeliveryRecipient RecipientType { get; set; }

        public McuDeliveryContent ContentType { get; set; }

        public McuDeliveryChannel Channel { get; set; }

        /// <summary>SENSITIF. Nomor atau alamat tujuan untuk WhatsApp, Email, Telepon.</summary>
        [MaxLength(200)]
        public string? DestinationText { get; set; }

        public DateTime DeliveredAt { get; set; }

        [Required]
        public Guid DeliveredByUserId { get; set; }

        /// <summary>Contoh <i>Telepon diterima peserta</i>.</summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        /// <summary>Diset <c>true</c> saat resume yang diserahkan dibuka ulang.</summary>
        public bool RequiresCorrectionNotice { get; set; }

        public DateTime? CorrectionNoticeRecordedAt { get; set; }
    }
}
