using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Enums
{
    public enum EventTypeKind
    {
        [Display(Name = "Transaksi")]
        Transaksi = 1,

        [Display(Name = "Saldo Subledger")]
        SaldoSubledger = 2
    }
}
