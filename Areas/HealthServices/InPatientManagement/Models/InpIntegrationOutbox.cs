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

        public void MarkPublished()
        {
            Status = OutboxStatus.Published;
            PublishedAtUtc = DateTime.UtcNow;
            LastError = null;
        }

        public void RecordFailure(string error, DateTime nextRetry)
        {
            RetryCount++;
            LastError = error;
            NextRetryAtUtc = nextRetry;
            if (RetryCount >= 10)
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
