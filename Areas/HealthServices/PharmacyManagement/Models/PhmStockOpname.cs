using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Sesi Stock Opname pada satu lokasi penyimpanan (FIN-LGS-091, rancangan dua tabel FIN-LGS-089).
/// </summary>
/// <remarks>
/// <para>
/// Keputusan disimpan di baris ini, bukan di tabel tersendiri: satu sesi hanya punya satu
/// keputusan final, dan hitung ulang sesudah penolakan memakai sesi baru
/// (<see cref="PreviousStockOpnameId"/>). Pembuat sesi terbaca dari <c>CreateBy</c>.
/// </para>
/// <para>
/// Penyesuaian saldo hasil sesi ini dicatat di kartu stok yang sudah ada
/// (<see cref="PhmDrugStockMutation"/> dengan sumber <c>StockOpname</c> dan
/// <c>SourceDocumentId</c> = <see cref="Id"/>). Berita acara disusun dari sesi berstatus
/// <see cref="StockOpnameStatus.Completed"/> beserta barisnya; nomornya adalah
/// <see cref="OpnameNumber"/>.
/// </para>
/// </remarks>
[Table("PhmStockOpname", Schema = "public")]
public class PhmStockOpname : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nomor sesi dan nomor berita acara, diterbitkan alokator nomor bersama (<c>SOP-000001</c>).</summary>
    [Required, MaxLength(30)]
    public string OpnameNumber { get; set; } = string.Empty;

    [Required] public Guid StorageLocationId { get; set; }

    /// <summary>Salinan nama lokasi saat sesi dibuat, supaya berita acara tidak berubah bila master diubah.</summary>
    [Required, MaxLength(200)]
    public string StorageLocationNameSnapshot { get; set; } = string.Empty;

    /// <summary>Cakupan status stok yang dihitung; null berarti semua status.</summary>
    public DrugStockStatus? StockStatusScope { get; set; }

    public DateOnly OpnameDate { get; set; }

    public StockOpnameStatus Status { get; set; } = StockOpnameStatus.Counting;

    /// <summary>Sesi yang ditolak lalu dihitung ulang lewat sesi ini.</summary>
    public Guid? PreviousStockOpnameId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public Guid? SubmittedByUserId { get; set; }
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Penyetuju atau penolak; wajib bukan pembuat, pengaju, maupun penghitung sesi.</summary>
    public Guid? DecidedByUserId { get; set; }

    public DateTime? DecidedAt { get; set; }

    /// <summary>Alasan keputusan; wajib untuk penolakan.</summary>
    [MaxLength(500)]
    public string? DecisionReason { get; set; }

    /// <summary>Token konkurensi; setiap perubahan sesi atau barisnya menaikkannya.</summary>
    public int Version { get; set; }

    public MstDrugStorageLocation? StorageLocation { get; set; }
    public PhmStockOpname? PreviousStockOpname { get; set; }
    public ICollection<PhmStockOpnameLine> Lines { get; set; } = new List<PhmStockOpnameLine>();
}
