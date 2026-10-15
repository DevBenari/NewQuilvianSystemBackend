using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

/// <summary>
/// Master peralihan penanganan potongan cicilan ke mesin payroll (BE-FIN-110, BE-FIN-115, FIN-DEC-222, FIN-DES-108).
/// Menyimpan satu nilai aktif "periode pertama yang ditangani mesin" (AutomaticFromPeriod, format YYYY-MM).
/// </summary>
[Table("MstPayrollDeductionCutover", Schema = "public")]
public sealed class MstPayrollDeductionCutover : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Periode pertama yang ditangani mesin payroll otomatis (format: YYYY-MM).
    /// </summary>
    [Required, MaxLength(7)]
    public string AutomaticFromPeriod { get; set; } = string.Empty;

    /// <summary>
    /// Alasan penetapan atau pembaruan periode peralihan.
    /// </summary>
    [Required, MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;

    /// <summary>
    /// Hanya satu baris aktif pada satu waktu — dijaga unique index parsial.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Penanda versi optimistic concurrency.
    /// </summary>
    [ConcurrencyCheck]
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

