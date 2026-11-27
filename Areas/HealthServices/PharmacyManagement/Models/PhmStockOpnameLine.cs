using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Detail perhitungan Stock Opname: satu saldo (batch × status) atau satu temuan tanpa saldo.
/// </summary>
/// <remarks>
/// <para>
/// Snapshot disimpan per baris (<see cref="SnapshotQuantity"/>, <see cref="SnapshotVersion"/>,
/// <see cref="SnapshotTakenAt"/>), sehingga baris yang saldonya berubah dapat dimuat ulang sendiri
/// tanpa membuang hasil hitung baris lain. Salinan nama obat, batch, dan satuan menjaga berita
/// acara tetap sama walaupun master diubah kemudian.
/// </para>
/// <para>
/// <see cref="AdjustmentMutationId"/> diisi sekali ketika penyesuaian diposting. Mutasinya memakai
/// <c>CorrelationId = opname-{Id}</c>, yang dijaga indeks unik
/// <c>(SourceDocumentType, CorrelationId)</c> milik kartu stok: posting kedua untuk baris yang sama
/// ditolak basis data.
/// </para>
/// </remarks>
[Table("PhmStockOpnameLine", Schema = "public")]
public class PhmStockOpnameLine : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid StockOpnameId { get; set; }

    /// <summary>Urutan baris pada lembar hitung sesi ini.</summary>
    public int LineNumber { get; set; }

    public StockOpnameLineType LineType { get; set; } = StockOpnameLineType.Balance;

    /// <summary>Saldo yang dihitung; null untuk temuan.</summary>
    public Guid? DrugStockBalanceId { get; set; }

    [Required] public Guid DrugId { get; set; }

    /// <summary>Batch; temuan boleh tanpa batch terdaftar (nomor batch dicatat di <see cref="BatchNumberSnapshot"/>).</summary>
    public Guid? DrugBatchId { get; set; }

    public DrugStockStatus StockStatus { get; set; } = DrugStockStatus.Available;

    [Required, MaxLength(50)]
    public string DrugCodeSnapshot { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string DrugNameSnapshot { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? BatchNumberSnapshot { get; set; }

    public DateOnly? ExpiryDateSnapshot { get; set; }

    [MaxLength(50)]
    public string? StockUnitNameSnapshot { get; set; }

    /// <summary>Saldo fisik sistem saat snapshot; 0 untuk temuan.</summary>
    [Column(TypeName = "numeric(18,3)")]
    public decimal SnapshotQuantity { get; set; }

    /// <summary>Versi saldo saat snapshot; null untuk temuan.</summary>
    public int? SnapshotVersion { get; set; }

    public DateTime SnapshotTakenAt { get; set; }

    /// <summary>Hasil hitung fisik; null berarti belum dihitung, 0 adalah hasil yang sah.</summary>
    [Column(TypeName = "numeric(18,3)")]
    public decimal? PhysicalQuantity { get; set; }

    /// <summary>Stok fisik − snapshot, ditetapkan ketika sesi diajukan.</summary>
    [Column(TypeName = "numeric(18,3)")]
    public decimal? VarianceQuantity { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    /// <summary>Penghitung baris ini; dipakai larangan self-approval.</summary>
    public Guid? CountedByUserId { get; set; }

    public DateTime? CountedAt { get; set; }

    /// <summary>Mutasi penyesuaian hasil baris ini; diisi sekali saat sesi disetujui.</summary>
    public Guid? AdjustmentMutationId { get; set; }

    public PhmStockOpname? StockOpname { get; set; }
    public PhmDrugStockBalance? DrugStockBalance { get; set; }
    public MstDrug? Drug { get; set; }
    public PhmDrugBatch? DrugBatch { get; set; }
    public PhmDrugStockMutation? AdjustmentMutation { get; set; }
}
