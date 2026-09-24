using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Models
{
    [Table("AccAccountingEvent", Schema = "public")]
    public class AccAccountingEvent : IdentityModel
    {
        public const string MataUangRupiah = "IDR";

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventNumber { get; set; } = string.Empty;

        public Guid? EventTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SourceModule { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SourceTransactionId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string SourceVersion { get; set; } = "1";

        public DateTimeOffset EventOccurredAt { get; set; }

        public DateTime AccountingDate { get; set; }

        public DateTime DocumentDate { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = MataUangRupiah;

        public AccountingEventStatus EventStatus { get; set; } = AccountingEventStatus.Diterima;

        [MaxLength(50)]
        public string? HoldReasonCode { get; set; }

        public Guid? JournalId { get; set; }

        [Required]
        public string RawPayload { get; set; } = string.Empty;

        public int AttemptCount { get; set; }

        public Guid CorrelationId { get; set; }

        public Guid CausationId { get; set; }

        [MaxLength(500)]
        public string? IgnoreReason { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public AccEventType? EventType { get; set; }

        public AccJournal? Journal { get; set; }

        public ICollection<AccAccountingEventAttempt> Attempts { get; set; } = new List<AccAccountingEventAttempt>();

        public ICollection<AccAccountingEventComponent> Components { get; set; } = new List<AccAccountingEventComponent>();
    }
}
