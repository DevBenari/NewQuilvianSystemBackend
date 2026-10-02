using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

/// <summary>
/// Data induk mata uang milik Finance (FIN-DES-003, FIN-DEC-018). Dipakai FinExchangeRate dan
/// rujukan CurrencyCode pada MstBankAccount.
/// </summary>
[Table("MstCurrency", Schema = "public")]
public sealed class MstCurrency : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(3)] public string CurrencyCode { get; set; } = string.Empty;

    [Required, MaxLength(100)] public string CurrencyName { get; set; } = string.Empty;

    [MaxLength(10)] public string? Symbol { get; set; }

    public int DecimalPlaces { get; set; } = 2;

    /// <summary>Hanya satu baris boleh true — ditegakkan partial unique index (FR-FIN-003).</summary>
    public bool IsBaseCurrency { get; set; } = false;

    public bool IsActive { get; set; } = true;
}
