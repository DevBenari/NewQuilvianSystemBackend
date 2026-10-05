using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

/// <summary>
/// Pemetaan akun control subledger ke kode akun Chart of Accounts (COA) Accounting (R14.4, FIN-DES-080, FIN-DEC-113).
/// Menentukan kode akun control yang digunakan untuk setiap kelompok saldo (BalanceGroup) dan segmen opsional (SegmentKey).
/// </summary>
[Table("FinSubledgerControlAccountMap", Schema = "public")]
public sealed class FinSubledgerControlAccountMap : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)]
    public string BalanceGroup { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? SegmentKey { get; set; }

    [Required, MaxLength(50)]
    public string ControlAccountCode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [MaxLength(300)]
    public string? Notes { get; set; }
}

public static class FinSubledgerBalanceGroups
{
    public const string KasKasir = "KAS-KASIR";
    public const string KasKecil = "KAS-KECIL";
    public const string Piutang = "PIUTANG";
    public const string UtangSupplier = "UTANG-SUPPLIER";
    public const string UtangJasaMedis = "UTANG-JASA-MEDIS";

    public static readonly string[] All = [KasKasir, KasKecil, Piutang, UtangSupplier, UtangJasaMedis];
}
