using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

/// <summary>
/// Rekening bank rumah sakit milik Finance (FIN-DES-003, FIN-SC-006). BankId merujuk MstBank
/// existing milik Administrator/MasterData (BE-FIN-002) — Finance MUST NOT membuat master Bank
/// baru sendiri, mengikuti preseden reuse MstSupplier pada FIN-DEC-014.
/// </summary>
[Table("MstBankAccount", Schema = "public")]
public sealed class MstBankAccount : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BankId { get; set; }
    public MstBank? Bank { get; set; }

    /// <summary>Nomor rekening rumah sakit — Sensitif: MUST NOT masuk custom logger.</summary>
    [Required, MaxLength(50)] public string AccountNumber { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string AccountName { get; set; } = string.Empty;

    /// <summary>OPERATIONAL, COLLECTION, atau PAYMENT.</summary>
    [Required, MaxLength(30)] public string AccountType { get; set; } = string.Empty;

    [Required, MaxLength(3)] public string CurrencyCode { get; set; } = "IDR";

    public bool IsActive { get; set; } = true;
}
