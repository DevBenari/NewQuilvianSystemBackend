using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Models
{
    [Table("AccSubledgerBalance", Schema = "public")]
    public class AccSubledgerBalance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        public Guid AccountingPeriodId { get; set; }

        [Required]
        public Guid ChartOfAccountId { get; set; }

        public decimal Balance { get; set; }

        public DateTime AsOfDate { get; set; }

        public int SourceVersionNumber { get; set; }

        [Required]
        public Guid AccountingEventId { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public AccAccountingPeriod? AccountingPeriod { get; set; }

        public AccChartOfAccount? ChartOfAccount { get; set; }

        public AccAccountingEvent? AccountingEvent { get; set; }
    }
}
