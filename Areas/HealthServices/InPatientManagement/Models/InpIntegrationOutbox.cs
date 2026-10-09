using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Model entitas untuk menyimpan event integrasi yang harus dipublikasikan ke message broker
    /// atau modul Billing secara transaksional (Transactional Outbox Pattern).
    /// </summary>
    [Table("InpIntegrationOutboxes", Schema = "public")]
    public class InpIntegrationOutbox : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string IdempotencyKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SourceDomain { get; set; } = "INPATIENT";

        [Required]
        [MaxLength(50)]
        public string SourceType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SourceDetailId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public string PayloadJson { get; set; } = string.Empty;

        public OutboxStatus Status { get; set; } = OutboxStatus.Pending;

        public int RetryCount { get; set; } = 0;

        public DateTime? NextRetryAtUtc { get; set; }

        public DateTime? PublishedAtUtc { get; set; }

        public string? LastError { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Awal masa sewa pemrosesan. Pesan <c>Processing</c> yang sewanya lebih tua dari
        /// <c>InpatientIntegrationOutbox:ProcessingLeaseSeconds</c> diambil ulang worker berikutnya
        /// (kontrak <c>integrasi-billing</c> <c>1.1.0</c> state 5.1).
        /// </summary>
        public DateTime? ProcessingStartedAtUtc { get; set; }

        /// <summary>
        /// Rujukan logis ke tanda terima Billing (<c>BilInpatientEventReceipt.Id</c>), tanpa FK
        /// lintas modul. Terisi hanya ketika Billing menyatakan pesan diterima (<c>INV-RWF-04</c>).
        /// </summary>
        public Guid? AcknowledgedReceiptId { get; set; }

        /// <summary>Kelompok putar ulang yang terakhir mengantrekan ulang pesan ini.</summary>
        public Guid? ReplayBatchId { get; set; }

        /// <summary>Waktu pesan diantrekan ulang oleh putar ulang.</summary>
        public DateTime? ReplayedAtUtc { get; set; }

        /// <summary>
        /// Menandai pesan terkirim. <b>Hanya</b> dipanggil setelah Billing mengembalikan tanda terima
        /// yang menyatakan diterima — pesan tanpa tanda terima tidak pernah <c>Published</c>
        /// (<c>INV-RWF-04</c>, <c>BE-RWI-151</c>).
        /// </summary>
        public void MarkPublished(Guid acknowledgedReceiptId)
        {
            Status = OutboxStatus.Published;
            PublishedAtUtc = DateTime.UtcNow;
            AcknowledgedReceiptId = acknowledgedReceiptId;
            ProcessingStartedAtUtc = null;
            LastError = null;
        }

        /// <summary>
        /// Mencatat satu kegagalan pengiriman. Pesan menjadi <c>DeadLetter</c> setelah
        /// <paramref name="maxRetry"/> kali gagal (bawaan 10, konfigurasi
        /// <c>InpatientIntegrationOutbox:MaxRetry</c>).
        /// </summary>
        public void RecordFailure(string error, DateTime nextRetry, int maxRetry = 10)
        {
            RetryCount++;
            LastError = error;
            NextRetryAtUtc = nextRetry;
            ProcessingStartedAtUtc = null;
            if (RetryCount >= maxRetry)
            {
                Status = OutboxStatus.DeadLetter;
            }
            else
            {
                Status = OutboxStatus.Failed;
            }
        }
    }
}
