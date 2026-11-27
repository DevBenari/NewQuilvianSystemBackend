using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu upaya kirim hasil kepada pasien, <i>Terkirim</i> atau <i>Gagal</i> (<c>LAB-DC-076</c>,
    /// <c>LAB-DEC-283</c>). Menggantikan rancangan <c>LabResultDelivery</c>
    /// (<c>02-backend-architecture.md</c> bagian 13) dengan mempertahankan intinya: angka
    /// <i>Terkirim ke Pasien</i> adalah <b>turunan</b> dari baris ini, bukan kolom (<c>INV-76</c>), dan
    /// nomor tujuan disimpan sebagai snapshot.
    /// </summary>
    /// <remarks>Tidak pernah diubah atau dihapus (<c>INV-75</c>).</remarks>
    [Table("LabPatientResultDelivery", Schema = "public")]
    public class LabPatientResultDelivery : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        /// <summary>Versi yang dikirim — versi berlaku saat dicatat (<c>VAL-170</c>).</summary>
        public Guid VersionId { get; set; }

        public LabPatientResultDeliveryChannel Channel { get; set; } = LabPatientResultDeliveryChannel.WhatsAppManual;

        public LabPatientResultDeliveryOutcome Outcome { get; set; }

        public LabPatientResultDeliveryFailureReason? FailureReason { get; set; }

        /// <summary>Keterangan alasan <i>Lainnya</i>; teks bebas — jangan dicatat ke log.</summary>
        [MaxLength(500)]
        public string? FailureNote { get; set; }

        /// <summary>
        /// Nomor WhatsApp ternormalkan <c>628…</c> saat dikirim (<c>INV-77</c>). <b>Sensitif</b>; tidak
        /// ikut berubah bila data induk pasien diubah.
        /// </summary>
        [MaxLength(20)]
        public string DestinationSnapshot { get; set; } = string.Empty;

        /// <summary>Pencatat — selalu orang (<c>02-backend-architecture.md</c> 13.7).</summary>
        public Guid RecordedByUserId { get; set; }

        public DateTime RecordedAt { get; set; }

        /// <summary>Penangkal klik ganda dari layar (<c>VAL-172</c>).</summary>
        public Guid ClientRequestId { get; set; }

        public LabPatientResultDocument? Document { get; set; }

        public LabPatientResultVersion? ResultVersion { get; set; }
    }
}
