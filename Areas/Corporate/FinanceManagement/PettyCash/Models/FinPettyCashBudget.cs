using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PettyCash.Models;

/// <summary>
/// Baris periode anggaran kas kecil beserta saldo berjalannya (PC-DES-004, PC-DES-017).
/// Sejak revisi 15 September 2026 (PC-DEC-017), satu PoolCode dapat memiliki BANYAK baris
/// dari waktu ke waktu — satu per periode — bukan lagi satu baris statis selamanya
/// (PC-DES-014, superseded). Paling banyak SATU baris berstatus ACTIVE per PoolCode pada
/// satu waktu (ditegakkan index parsial pada configuration).
/// CurrentBalance MUST NOT ditulis di luar PettyCashBudgetService dan MUST NOT negatif.
/// </summary>
[Table("FinPettyCashBudget", Schema = "public")]
public sealed class FinPettyCashBudget : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)] public string PoolCode { get; set; } = string.Empty;

    [Required, MaxLength(100)] public string PoolName { get; set; } = string.Empty;

    /// <summary>Tanggal mulai berlakunya periode ini (PC-DES-017). Baris warisan sebelum
    /// revisi ini diisi migration dari tanggal <see cref="IdentityModel.CreateDateTime"/>
    /// baris itu sendiri (PC-DES-024).</summary>
    public DateOnly PeriodStart { get; set; }

    /// <summary>Tanggal selesai periode, opsional. NULL berarti periode berjalan sampai
    /// ditutup Finance secara eksplisit — keadaan sah, bukan data yang belum diisi
    /// (PC-DES-017).</summary>
    public DateOnly? PeriodEnd { get; set; }

    /// <summary>Plafon anggaran periode ini, ditetapkan Finance. Baris warisan diisi
    /// migration dari TotalTopUpAmount baris itu sendiri (PC-DES-024).</summary>
    public decimal BudgetAmount { get; set; }

    /// <summary>Saldo berjalan — angka kartu "TOTAL PETTY CASH".</summary>
    public decimal CurrentBalance { get; set; }

    public decimal TotalTopUpAmount { get; set; }

    public decimal TotalDisbursedAmount { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = PettyCashBudgetStatuses.Active;

    /// <summary>Periode penerus yang menerima sisa saldo saat baris ini ditutup
    /// (PC-DES-018). Kosong sampai baris ini benar-benar ditutup.</summary>
    public Guid? SupersededByBudgetId { get; set; }

    public DateTimeOffset? LastMovementAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinPettyCashBudgetMovement> Movements { get; set; } = new List<FinPettyCashBudgetMovement>();
}

public static class PettyCashBudgetStatuses
{
    /// <summary>Warisan pra-revisi 15 September 2026. Tetap ada karena
    /// PettyCashBudgetService masih menulis/membacanya; dipensiunkan penuh saat
    /// BE-BKC-054 menyentuh service tersebut.</summary>
    public const string Active = "ACTIVE";

    /// <summary>Warisan pra-revisi 15 September 2026, lihat catatan pada <see cref="Active"/>.
    /// Baris lama dipetakan migration menjadi <see cref="Closed"/> (PC-DES-024); nilai ini
    /// tetap diterima constraint database untuk kompatibilitas mundur selama transisi.</summary>
    public const string Inactive = "INACTIVE";

    /// <summary>Periode baru yang belum diaktifkan Finance (PC-DES-017).</summary>
    public const string Draft = "DRAFT";

    /// <summary>Periode yang sudah ditutup; sisa saldonya sudah/akan berpindah ke periode
    /// penerus (PC-DES-018).</summary>
    public const string Closed = "CLOSED";
}

