using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

/// <summary>
/// Kurs harian per mata uang, dipakai untuk mencatat transaksi non-IDR di level operasional
/// (FIN-DEC-018). Satu baris per MstCurrency per RateDate.
/// </summary>
[Table("MstExchangeRate", Schema = "public")]
public sealed class MstExchangeRate : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CurrencyId { get; set; }
    public MstCurrency? Currency { get; set; }

    public DateOnly RateDate { get; set; }

    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public decimal MiddleRate { get; set; }

    [MaxLength(100)] public string? Source { get; set; }
}
