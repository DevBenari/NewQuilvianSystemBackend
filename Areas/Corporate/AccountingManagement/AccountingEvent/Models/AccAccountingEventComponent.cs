using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Models
{
    [Table("AccAccountingEventComponent", Schema = "public")]
    public class AccAccountingEventComponent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AccountingEventId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ComponentCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public AccAccountingEvent? AccountingEvent { get; set; }
    }
}
