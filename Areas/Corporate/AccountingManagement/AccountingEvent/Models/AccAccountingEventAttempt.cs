using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Models
{
    [Table("AccAccountingEventAttempt", Schema = "public")]
    public class AccAccountingEventAttempt : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AccountingEventId { get; set; }

        public int AttemptNumber { get; set; }

        public DateTimeOffset AttemptedAt { get; set; }

        public bool IsSuccess { get; set; }

        [MaxLength(1000)]
        public string? FailureMessage { get; set; }

        public AccAccountingEvent? AccountingEvent { get; set; }
    }
}
