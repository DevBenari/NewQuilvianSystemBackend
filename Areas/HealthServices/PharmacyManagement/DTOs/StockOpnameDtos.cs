using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

// =====================================================================================
// Stock Opname — hitung selisih (FIN-LGS-084).
//
// Hanya bagian yang dapat berjalan tanpa skema baru: membandingkan hasil hitung fisik dengan
// saldo saat ini per batch × status pada satu lokasi. Tidak ada sesi, hasil hitung, keputusan,
// penyesuaian, atau berita acara yang disimpan; semuanya menunggu tabel Stock Opname
// (lihat FIN-LGS-LAPORAN-FINISHING.md, bagian STOCK OPNAME).
// =====================================================================================

/// <summary>Permintaan hitung selisih. Hasilnya tidak disimpan dan tidak mengubah saldo.</summary>
public class StockOpnameVariancePreviewRequest
{
    /// <summary>Lokasi yang dihitung (wajib; kosong atau tidak terdaftar dijawab 422 <c>PHM036</c>).</summary>
    public Guid? StorageLocationId { get; set; }

    /// <summary>
    /// Waktu lembar hitung dimuat (UTC). Bila diisi, mutasi stok pada batch × status yang sama
    /// sesudah waktu ini menandai baris sebagai "perlu dihitung ulang".
    /// </summary>
    public DateTime? SnapshotTakenAtUtc { get; set; }

    /// <summary>Satu baris per saldo (batch × status) yang dihitung; paling banyak 2000.</summary>
    [MaxLength(2000)]
    public List<StockOpnameCountInput> Counts { get; set; } = [];
}

/// <summary>Satu baris lembar hitung.</summary>
public class StockOpnameCountInput
{
    /// <summary>Saldo yang dihitung (<c>PhmDrugStockBalance.Id</c>), harus berada di lokasi yang dipilih.</summary>
    public Guid? BalanceId { get; set; }

    /// <summary>Saldo sistem yang tercetak di lembar hitung saat dimuat (wajib).</summary>
    public decimal? SnapshotQuantity { get; set; }

    /// <summary>Hasil hitung fisik; null berarti belum dihitung. Nol adalah hasil hitung yang sah.</summary>
    public decimal? PhysicalQuantity { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}

/// <summary>Hasil perbandingan satu baris. Kode yang sama dipakai sesudah persistence tersedia.</summary>
public static class StockOpnameVarianceResults
{
    /// <summary>Belum ada hasil hitung fisik.</summary>
    public const string NotCounted = "NotCounted";

    /// <summary>Stok fisik sama dengan saldo sistem.</summary>
    public const string Match = "Match";

    /// <summary>Stok fisik lebih banyak dari saldo sistem.</summary>
    public const string Surplus = "Surplus";

    /// <summary>Stok fisik lebih sedikit dari saldo sistem.</summary>
    public const string Shortage = "Shortage";

    /// <summary>
    /// Saldo berubah sejak lembar hitung dimuat (jumlah berbeda atau ada mutasi sesudahnya), sehingga
    /// hasil hitung tidak dapat dibandingkan dengan aman. Baris harus dihitung ulang.
    /// </summary>
    public const string NeedsRecount = "NeedsRecount";

    /// <summary>Temuan tanpa saldo (FIN-LGS-091); tidak pernah diposting.</summary>
    public const string Finding = "Finding";
}

public class StockOpnameVarianceLineResponse
{
    public Guid BalanceId { get; set; }
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? StockUnitName { get; set; }
    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>Batch sudah lewat kedaluwarsa (status apa pun).</summary>
    public bool IsExpired { get; set; }

    public DrugStockStatus Status { get; set; }

    /// <summary>Saldo sistem di lembar hitung (dikirim klien).</summary>
    public decimal SnapshotQuantity { get; set; }

    /// <summary>Saldo fisik tersimpan saat ini (dibaca server).</summary>
    public decimal CurrentQuantity { get; set; }

    public bool ChangedSinceSnapshot { get; set; }
    public decimal? PhysicalQuantity { get; set; }

    /// <summary>Stok fisik − saldo saat ini; null bila belum dihitung atau perlu dihitung ulang.</summary>
    public decimal? Variance { get; set; }

    public string Result { get; set; } = StockOpnameVarianceResults.NotCounted;
    public string? Note { get; set; }
}

/// <summary>Ringkasan jumlah baris. Jumlah barang tidak dijumlahkan lintas obat karena satuannya berbeda.</summary>
public class StockOpnameVarianceSummaryResponse
{
    public int LineCount { get; set; }
    public int CountedLineCount { get; set; }
    public int NotCountedLineCount { get; set; }
    public int MatchCount { get; set; }
    public int SurplusCount { get; set; }
    public int ShortageCount { get; set; }
    public int NeedsRecountCount { get; set; }

    /// <summary>Semua baris sudah dihitung dan tidak ada yang perlu dihitung ulang.</summary>
    public bool IsComplete { get; set; }
}

public class StockOpnameVariancePreviewResponse
{
    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;
    public DateTime CalculatedAtUtc { get; set; }

    /// <summary>Selalu false: hasil ini tidak disimpan sampai tabel Stock Opname tersedia.</summary>
    public bool IsPersisted { get; set; }

    public StockOpnameVarianceSummaryResponse Summary { get; set; } = new();
    public List<StockOpnameVarianceLineResponse> Lines { get; set; } = [];
    public List<string> Limitations { get; set; } = [];
}

// =====================================================================================
// Transaksi Stock Opname (FIN-LGS-091): sesi dan detail perhitungan.
//
// Seluruh perintah tulis membawa ExpectedVersion sesi. Sebelum tabel PhmStockOpname dan
// PhmStockOpnameLine dibuat lewat migrasi, setiap endpoint menjawab 503 PHM039 (BLOCKED_SCHEMA)
// dan halaman memakai mode terbatas.
// =====================================================================================

/// <summary>Kesiapan penyimpanan Stock Opname. Halaman memakai mode terbatas bila belum siap.</summary>
public class StockOpnameAvailabilityResponse
{
    public bool IsPersistenceReady { get; set; }

    /// <summary><c>READY</c> atau <c>BLOCKED_SCHEMA</c>.</summary>
    public string Status { get; set; } = StockOpnameAvailabilityStatuses.BlockedSchema;

    public string Message { get; set; } = string.Empty;
}

public static class StockOpnameAvailabilityStatuses
{
    public const string Ready = "READY";
    public const string BlockedSchema = "BLOCKED_SCHEMA";
}

public class StockOpnameListQuery
{
    public Guid? StorageLocationId { get; set; }
    public StockOpnameStatus? Status { get; set; }

    /// <summary>Cari nomor sesi atau nama lokasi.</summary>
    [MaxLength(100)]
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Membuat sesi dan memotret seluruh saldo lokasi (sesuai cakupan status) sebagai baris hitung.</summary>
public class CreateStockOpnameRequest
{
    public Guid? StorageLocationId { get; set; }

    /// <summary>Cakupan status; kosong berarti semua status.</summary>
    public DrugStockStatus? StockStatusScope { get; set; }

    /// <summary>Tanggal opname; kosong berarti hari ini (tanggal server).</summary>
    public DateOnly? OpnameDate { get; set; }

    /// <summary>Sesi yang ditolak dan dihitung ulang lewat sesi baru ini (lokasinya harus sama).</summary>
    public Guid? PreviousStockOpnameId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>Dasar setiap perintah tulis pada sesi.</summary>
public class StockOpnameCommandRequest
{
    /// <summary>Versi sesi yang terakhir dibaca; berbeda berarti sesi sudah diubah orang lain (409 PHM057).</summary>
    public int? ExpectedVersion { get; set; }
}

public class SaveStockOpnameCountsRequest : StockOpnameCommandRequest
{
    /// <summary>Baris yang diubah saja; baris lain tidak tersentuh.</summary>
    [MaxLength(2000)]
    public List<StockOpnameCountEntry> Counts { get; set; } = [];
}

public class StockOpnameCountEntry
{
    public Guid? LineId { get; set; }

    /// <summary>Hasil hitung; null mengosongkan baris (belum dihitung). Nol adalah hasil yang sah.</summary>
    public decimal? PhysicalQuantity { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}

/// <summary>Mencatat barang atau batch yang ditemukan tanpa saldo di lokasi. Tidak menambah stok.</summary>
public class AddStockOpnameFindingRequest : StockOpnameCommandRequest
{
    public Guid? DrugId { get; set; }

    /// <summary>Batch terdaftar milik obat itu; kosong bila batch belum terdaftar.</summary>
    public Guid? DrugBatchId { get; set; }

    /// <summary>Nomor batch tertulis pada kemasan, untuk batch yang belum terdaftar.</summary>
    [MaxLength(100)]
    public string? BatchNumber { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DrugStockStatus? StockStatus { get; set; }

    public decimal? PhysicalQuantity { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}

/// <summary>Keputusan penyetuju. Alasan wajib untuk penolakan.</summary>
public class DecideStockOpnameRequest : StockOpnameCommandRequest
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class StockOpnameSummaryResponse
{
    public Guid Id { get; set; }
    public string OpnameNumber { get; set; } = string.Empty;
    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;
    public DrugStockStatus? StockStatusScope { get; set; }
    public DateOnly OpnameDate { get; set; }
    public StockOpnameStatus Status { get; set; }
    public int LineCount { get; set; }
    public int CountedLineCount { get; set; }
    public int FindingCount { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public int Version { get; set; }
}

public class StockOpnameLineResponse
{
    public Guid Id { get; set; }
    public int LineNumber { get; set; }
    public StockOpnameLineType LineType { get; set; }
    public Guid? DrugStockBalanceId { get; set; }
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? StockUnitName { get; set; }
    public Guid? DrugBatchId { get; set; }
    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
    public DrugStockStatus StockStatus { get; set; }
    public decimal SnapshotQuantity { get; set; }
    public DateTime SnapshotTakenAt { get; set; }

    /// <summary>Saldo saat ini; hanya dihitung selama sesi dihitung atau diajukan.</summary>
    public decimal? CurrentQuantity { get; set; }

    public bool ChangedSinceSnapshot { get; set; }
    public decimal? PhysicalQuantity { get; set; }

    /// <summary>Selisih: tersimpan sesudah diajukan; selama dihitung, perkiraan terhadap snapshot.</summary>
    public decimal? VarianceQuantity { get; set; }

    /// <summary>Kode <see cref="StockOpnameVarianceResults"/>, atau <c>Finding</c> untuk temuan.</summary>
    public string Result { get; set; } = StockOpnameVarianceResults.NotCounted;

    public string? Note { get; set; }
    public Guid? CountedByUserId { get; set; }
    public string? CountedByName { get; set; }
    public DateTime? CountedAt { get; set; }
    public Guid? AdjustmentMutationId { get; set; }
}

public class StockOpnameDetailResponse : StockOpnameSummaryResponse
{
    public Guid? PreviousStockOpnameId { get; set; }
    public string? Notes { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public string? SubmittedByName { get; set; }
    public Guid? DecidedByUserId { get; set; }
    public string? DecidedByName { get; set; }
    public string? DecisionReason { get; set; }

    /// <summary>Ringkasan baris saldo (temuan dihitung terpisah pada <see cref="StockOpnameSummaryResponse.FindingCount"/>).</summary>
    public StockOpnameVarianceSummaryResponse Summary { get; set; } = new();

    public List<StockOpnameLineResponse> Lines { get; set; } = [];

    /// <summary>Selalu true: sesi ini tersimpan di basis data.</summary>
    public bool IsPersisted { get; set; } = true;

    public bool CanEdit { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanDecide { get; set; }
    public bool CanCancel { get; set; }

    /// <summary>Alasan pengguna saat ini tidak dapat memutus (izin, self-approval, status), untuk ditampilkan.</summary>
    public string? DecisionBlockedReason { get; set; }

    /// <summary>Alasan sesi belum dapat diajukan (baris belum dihitung atau perlu dihitung ulang).</summary>
    public string? SubmitBlockedReason { get; set; }
}

/// <summary>Berita acara Stock Opname; hanya untuk sesi berstatus Selesai.</summary>
public class StockOpnameReportResponse
{
    public Guid StockOpnameId { get; set; }

    /// <summary>Nomor berita acara = nomor sesi.</summary>
    public string DocumentNumber { get; set; } = string.Empty;

    public string StorageLocationName { get; set; } = string.Empty;
    public DrugStockStatus? StockStatusScope { get; set; }
    public DateOnly OpnameDate { get; set; }
    public DateTime CompletedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public List<string> CountedByNames { get; set; } = [];
    public Guid ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public string? DecisionReason { get; set; }
    public StockOpnameVarianceSummaryResponse Summary { get; set; } = new();
    public int AdjustmentCount { get; set; }
    public List<StockOpnameLineResponse> DifferenceLines { get; set; } = [];
    public List<StockOpnameLineResponse> Findings { get; set; } = [];
}
