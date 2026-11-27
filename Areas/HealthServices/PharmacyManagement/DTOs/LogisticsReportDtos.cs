using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

// =====================================================================================
// Laporan Logistik berbasis jumlah (FIN-LGS-024). Seluruhnya membaca data yang sudah
// tersimpan; tidak ada nilai rupiah karena harga per batch belum disimpan (LGS-CAP-32,
// LGS-OQ-008). Jumlah barang tidak pernah dijumlahkan lintas obat, karena satuannya berbeda.
// =====================================================================================

/// <summary>Periode laporan yang benar-benar dipakai backend (tanggal lokal server, inklusif).</summary>
public class LogisticsReportPeriodResponse
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int DayCount { get; set; }
}

// ------------------------------------------------------------------ Laporan Persediaan

/// <summary>Saringan Laporan Persediaan (keadaan saldo saat ini).</summary>
public class InventoryReportQuery
{
    public Guid? StorageLocationId { get; set; }
    public Guid? DrugId { get; set; }

    /// <summary>Mencari pada nama atau kode obat.</summary>
    [MaxLength(100)] public string? Search { get; set; }

    /// <summary>Ikut menampilkan pasangan obat × lokasi yang seluruh saldonya nol.</summary>
    public bool IncludeEmpty { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Satu obat pada satu lokasi, dengan saldo per status dalam satuan stok obat.</summary>
public class InventoryReportRowResponse
{
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? StockUnitName { get; set; }
    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;

    /// <summary>Saldo fisik berstatus siap pakai.</summary>
    public decimal AvailableOnHand { get; set; }

    /// <summary>Bagian siap pakai yang sedang ditahan proses lain (resep, permintaan).</summary>
    public decimal Reserved { get; set; }

    /// <summary>Siap pakai dikurangi yang ditahan: yang benar-benar dapat dikeluarkan.</summary>
    public decimal AvailableToIssue { get; set; }

    public decimal Quarantine { get; set; }
    public decimal Expired { get; set; }
    public decimal Damaged { get; set; }

    /// <summary>Seluruh saldo fisik pada lokasi itu, semua status.</summary>
    public decimal TotalOnHand { get; set; }

    public int BatchCount { get; set; }

    /// <summary>Kedaluwarsa terdekat di antara batch yang masih bersaldo siap pakai.</summary>
    public DateOnly? NearestExpiryDate { get; set; }
    public int? DaysToNearestExpiry { get; set; }
}

/// <summary>Hitungan baris pada saringan yang sama — bukan penjumlahan jumlah lintas obat.</summary>
public class InventoryReportSummaryResponse
{
    public int RowCount { get; set; }
    public int DrugCount { get; set; }
    public int LocationCount { get; set; }
    public int RowsWithoutAvailable { get; set; }
    public int RowsWithQuarantine { get; set; }
    public int RowsWithExpired { get; set; }
    public int RowsWithDamaged { get; set; }
    public int RowsWithExpiredBatchStillAvailable { get; set; }
}

public class InventoryReportResponse
{
    public DateTime GeneratedAtUtc { get; set; }
    public InventoryReportSummaryResponse Summary { get; set; } = new();
    public PagedResult<InventoryReportRowResponse> Rows { get; set; } = new();
    public List<string> Limitations { get; set; } = [];
}

// ---------------------------------------------------------------- Laporan Mutasi Barang

/// <summary>Saringan Laporan Mutasi; periode kosong berarti 30 hari terakhir, maksimal 366 hari.</summary>
public class MutationReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? StorageLocationId { get; set; }
    public Guid? DrugId { get; set; }
    [MaxLength(100)] public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Rekap pergerakan satu obat pada satu lokasi dalam periode, dalam satuan stok obat.</summary>
public class MutationReportRowResponse
{
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? StockUnitName { get; set; }
    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;

    /// <summary>Saldo pembuka yang dicatat pertama kali dalam periode.</summary>
    public decimal OpeningRecorded { get; set; }

    /// <summary>Barang masuk (penerimaan, transfer masuk, retur).</summary>
    public decimal QuantityIn { get; set; }

    /// <summary>Barang keluar (penyerahan, pemakaian, transfer keluar), sebagai angka positif.</summary>
    public decimal QuantityOut { get; set; }

    /// <summary>Penyesuaian dan koreksi, bertanda (+/−).</summary>
    public decimal AdjustmentNet { get; set; }

    /// <summary>Saldo pembuka + masuk − keluar + penyesuaian dalam periode.</summary>
    public decimal NetChange { get; set; }

    public int MutationCount { get; set; }
    public DateTime LastMutationAt { get; set; }

    /// <summary>Saldo fisik saat laporan dibuat (semua status) — bukan saldo akhir periode.</summary>
    public decimal CurrentOnHand { get; set; }
}

public class MutationReportSummaryResponse
{
    public int MutationCount { get; set; }
    public int InCount { get; set; }
    public int OutCount { get; set; }
    public int AdjustmentCount { get; set; }
    public int OpeningCount { get; set; }
    public int StatusChangeCount { get; set; }
    public int DrugCount { get; set; }
    public int LocationCount { get; set; }
}

public class MutationReportResponse
{
    public LogisticsReportPeriodResponse Period { get; set; } = new();
    public MutationReportSummaryResponse Summary { get; set; } = new();
    public PagedResult<MutationReportRowResponse> Rows { get; set; } = new();
    public List<string> Limitations { get; set; } = [];
}

// ------------------------------------------------------------- Laporan Distribusi Barang

/// <summary>Saringan Laporan Distribusi: penyerahan permintaan unit menurut waktu penyerahan.</summary>
public class DistributionReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? RequestingServiceUnitId { get; set; }
    public Guid? StorageLocationId { get; set; }
    public Guid? DrugId { get; set; }

    /// <summary>Mencari pada nama/kode obat atau nama unit.</summary>
    [MaxLength(100)] public string? Search { get; set; }

    /// <summary>Cakupan yang sama dengan daftar permintaan (LGS-OQ-027 masih terbuka).</summary>
    [EnumDataType(typeof(StockRequestListScope))]
    public StockRequestListScope Scope { get; set; } = StockRequestListScope.All;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Satu unit peminta × obat × satuan permintaan dalam periode.</summary>
public class DistributionReportRowResponse
{
    public Guid RequestingServiceUnitId { get; set; }
    public string RequestingServiceUnitName { get; set; } = string.Empty;
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public Guid MeasurementId { get; set; }
    public string? MeasurementName { get; set; }
    public int RequestCount { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal FulfilledQuantity { get; set; }

    /// <summary>Diminta − diserahkan pada baris yang diserahkan kurang.</summary>
    public decimal ShortfallQuantity { get; set; }

    public int ShortfallLineCount { get; set; }
    public DateTime LastFulfilledAt { get; set; }
}

public class DistributionReportSummaryResponse
{
    public int CompletedRequestCount { get; set; }
    public int PartiallyFulfilledRequestCount { get; set; }
    public int LineCount { get; set; }
    public int UnitCount { get; set; }
    public int DrugCount { get; set; }
}

public class DistributionReportResponse
{
    public LogisticsReportPeriodResponse Period { get; set; } = new();
    public DistributionReportSummaryResponse Summary { get; set; } = new();
    public PagedResult<DistributionReportRowResponse> Rows { get; set; } = new();
    public List<string> Limitations { get; set; } = [];
}

// ---------------------------------------------------------- Laporan Pemakaian per Unit

/// <summary>Saringan Laporan Pemakaian per Unit menurut waktu pemakaian.</summary>
public class UsageByUnitReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>Unit layanan kunjungan pasien tempat barang dipakai.</summary>
    public Guid? ServiceUnitId { get; set; }

    /// <summary>Lokasi stok yang dipotong.</summary>
    public Guid? StorageLocationId { get; set; }

    public Guid? DrugId { get; set; }
    [MaxLength(100)] public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Satu unit layanan × obat × satuan dalam periode.</summary>
public class UsageByUnitReportRowResponse
{
    public Guid ServiceUnitId { get; set; }
    public string ServiceUnitName { get; set; } = string.Empty;
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public Guid MeasurementId { get; set; }
    public string? MeasurementName { get; set; }
    public int UsageDocumentCount { get; set; }
    public int EncounterCount { get; set; }
    public decimal Quantity { get; set; }
    public DateTime LastUsedAt { get; set; }
}

public class UsageByUnitReportSummaryResponse
{
    public int UsageDocumentCount { get; set; }
    public int EncounterCount { get; set; }
    public int UnitCount { get; set; }
    public int DrugCount { get; set; }
}

public class UsageByUnitReportResponse
{
    public LogisticsReportPeriodResponse Period { get; set; } = new();
    public UsageByUnitReportSummaryResponse Summary { get; set; } = new();
    public PagedResult<UsageByUnitReportRowResponse> Rows { get; set; } = new();
    public List<string> Limitations { get; set; } = [];
}

// ------------------------------------------------- Dashboard Penerimaan & Pengeluaran

/// <summary>Saringan Dashboard Penerimaan &amp; Pengeluaran.</summary>
public class StockFlowSummaryQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? StorageLocationId { get; set; }
    public Guid? DrugId { get; set; }
}

/// <summary>Arah mutasi pada dashboard. Perubahan status tidak termasuk masuk/keluar.</summary>
public static class StockFlowDirections
{
    public const string In = "In";
    public const string Out = "Out";
    public const string Adjustment = "Adjustment";
    public const string Opening = "Opening";
}

public class StockFlowTotalsResponse
{
    public int InMutationCount { get; set; }
    public int OutMutationCount { get; set; }
    public int AdjustmentCount { get; set; }
    public int OpeningCount { get; set; }
    public int StatusChangeCount { get; set; }
    public int DrugsReceivedCount { get; set; }
    public int DrugsIssuedCount { get; set; }
}

/// <summary>Jumlah mutasi per arah dan jenis dokumen sumber.</summary>
public class StockFlowSourceRowResponse
{
    public string Direction { get; set; } = string.Empty;

    /// <summary>Jenis dokumen sumber apa adanya (mis. <c>StockRequest</c>); null bila tidak dicatat.</summary>
    public string? SourceDocumentType { get; set; }

    public int MutationCount { get; set; }
    public int DrugCount { get; set; }
}

public class StockFlowDailyRowResponse
{
    public DateOnly Date { get; set; }
    public int InCount { get; set; }
    public int OutCount { get; set; }
    public int AdjustmentCount { get; set; }
}

/// <summary>Obat dengan jumlah masuk/keluar terbesar, dalam satuan stok obatnya sendiri.</summary>
public class StockFlowDrugRowResponse
{
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? StockUnitName { get; set; }
    public decimal Quantity { get; set; }
    public int MutationCount { get; set; }
}

public class StockFlowSummaryResponse
{
    public LogisticsReportPeriodResponse Period { get; set; } = new();
    public StockFlowTotalsResponse Totals { get; set; } = new();
    public List<StockFlowSourceRowResponse> BySource { get; set; } = [];
    public List<StockFlowDailyRowResponse> Daily { get; set; } = [];
    public List<StockFlowDrugRowResponse> TopReceivedDrugs { get; set; } = [];
    public List<StockFlowDrugRowResponse> TopIssuedDrugs { get; set; } = [];

    /// <summary>Benar bila ada mutasi masuk bersumber Goods Receipt pengadaan dalam periode.</summary>
    public bool HasProcurementReceipts { get; set; }

    public List<string> Limitations { get; set; } = [];
}

// ------------------------------------------- Pemeriksaan kelengkapan buku mutasi (FIN-LGS-039)

/// <summary>
/// Saringan pemeriksaan buku mutasi. Lokasi wajib supaya data yang dibaca tetap terbatas.
/// </summary>
public class LedgerConsistencyQuery
{
    /// <summary>Lokasi penyimpanan yang diperiksa (wajib; kosong dijawab 422 <c>PHM036</c>).</summary>
    public Guid? StorageLocationId { get; set; }

    public Guid? DrugId { get; set; }

    /// <summary>Bawaan hanya menampilkan baris yang tidak cocok.</summary>
    public bool OnlyInconsistent { get; set; } = true;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Hasil pemeriksaan satu batch × status pada lokasi yang diperiksa.</summary>
public static class LedgerConsistencyFindings
{
    /// <summary>Saldo tersimpan sama dengan jumlah semua perubahan di buku mutasi.</summary>
    public const string Match = "Match";

    /// <summary>Ada mutasi dan ada saldo, tetapi jumlahnya berbeda.</summary>
    public const string SumMismatch = "SumMismatch";

    /// <summary>Saldo tidak nol, tetapi tidak ada satu pun mutasi yang mencatatnya.</summary>
    public const string NoMutation = "NoMutation";

    /// <summary>Mutasi berjumlah tidak nol, tetapi baris saldonya tidak ada.</summary>
    public const string NoBalance = "NoBalance";
}

public class LedgerConsistencyRowResponse
{
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? StockUnitName { get; set; }
    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DrugStockStatus Status { get; set; }

    /// <summary>Saldo fisik tersimpan (<c>PhmDrugStockBalance.QuantityOnHand</c>).</summary>
    public decimal QuantityOnHand { get; set; }

    /// <summary>Jumlah semua <c>QuantityChange</c> di buku mutasi untuk batch × status ini.</summary>
    public decimal LedgerQuantity { get; set; }

    /// <summary>Saldo tersimpan − jumlah buku mutasi.</summary>
    public decimal Difference { get; set; }

    public int MutationCount { get; set; }
    public DateTime? LastMutationAt { get; set; }
    public string Finding { get; set; } = LedgerConsistencyFindings.Match;
}

public class LedgerConsistencySummaryResponse
{
    /// <summary>Jumlah pasangan batch × status yang punya saldo atau mutasi di lokasi itu.</summary>
    public int CheckedKeyCount { get; set; }

    public int MatchCount { get; set; }
    public int SumMismatchCount { get; set; }
    public int NoMutationCount { get; set; }
    public int NoBalanceCount { get; set; }
    public int MutationCount { get; set; }

    /// <summary>Benar bila semua pasangan cocok: saldo dapat direkonstruksi dari buku mutasi.</summary>
    public bool IsConsistent { get; set; }
}

public class LedgerConsistencyResponse
{
    public DateTime GeneratedAtUtc { get; set; }
    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;
    public LedgerConsistencySummaryResponse Summary { get; set; } = new();
    public PagedResult<LedgerConsistencyRowResponse> Rows { get; set; } = new();
    public List<string> Limitations { get; set; } = [];
}

// ------------------------------------------------- Mutasi Saldo Persediaan (FIN-LGS-056)

/// <summary>Saringan Mutasi Saldo Persediaan. Lokasi wajib (422 <c>PHM036</c>); periode sama dengan laporan lain.</summary>
public class BalanceMovementQuery
{
    public Guid? StorageLocationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? DrugId { get; set; }
    [MaxLength(100)] public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Status saldo historis sebuah baris.</summary>
public static class BalanceMovementStatuses
{
    /// <summary>Buku mutasi cocok dan lokasi sudah lolos UAT buku mutasi (L1–L2).</summary>
    public const string Valid = "Valid";

    /// <summary>Buku mutasi cocok, tetapi lokasi belum lolos UAT; angka tidak boleh dipakai sebagai saldo resmi.</summary>
    public const string Unverified = "Unverified";

    /// <summary>Buku mutasi tidak cocok (S4): saldo awal dan akhir tidak dihitung.</summary>
    public const string NotComputable = "NotComputable";
}

/// <summary>Satu batch (semua status fisik digabung) dalam periode.</summary>
public class BalanceMovementBatchResponse
{
    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>Null bila <see cref="Status"/> = <c>NotComputable</c>.</summary>
    public decimal? OpeningBalance { get; set; }

    public decimal OpeningRecorded { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal AdjustmentNet { get; set; }

    /// <summary>Null bila <see cref="Status"/> = <c>NotComputable</c>.</summary>
    public decimal? ClosingBalance { get; set; }

    public decimal CurrentOnHand { get; set; }
    public int MutationCount { get; set; }
    public int StatusChangeCount { get; set; }
    public int InconsistentKeyCount { get; set; }
    public string Status { get; set; } = BalanceMovementStatuses.NotComputable;
}

/// <summary>Ringkasan satu obat pada lokasi yang diperiksa, dalam satuan stok obat.</summary>
public class BalanceMovementRowResponse
{
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? StockUnitName { get; set; }
    public decimal? OpeningBalance { get; set; }
    public decimal OpeningRecorded { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal AdjustmentNet { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal CurrentOnHand { get; set; }
    public int MutationCount { get; set; }
    public int StatusChangeCount { get; set; }
    public int InconsistentKeyCount { get; set; }
    public string Status { get; set; } = BalanceMovementStatuses.NotComputable;
    public List<BalanceMovementBatchResponse> Batches { get; set; } = [];
}

public class BalanceMovementSummaryResponse
{
    public int RowCount { get; set; }
    public int ValidRowCount { get; set; }
    public int UnverifiedRowCount { get; set; }
    public int NotComputableRowCount { get; set; }

    /// <summary>Benar bila semua batch × status di lokasi (dalam saringan) cocok dengan buku mutasi.</summary>
    public bool IsLedgerConsistent { get; set; }
}

public class BalanceMovementResponse
{
    public LogisticsReportPeriodResponse Period { get; set; } = new();
    public Guid StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;

    /// <summary>Benar bila lokasi tercantum di konfigurasi lokasi yang lolos UAT buku mutasi.</summary>
    public bool IsLocationVerified { get; set; }

    public BalanceMovementSummaryResponse Summary { get; set; } = new();
    public PagedResult<BalanceMovementRowResponse> Rows { get; set; } = new();
    public List<string> Limitations { get; set; } = [];
}

// -------------------------------------------- Rekomendasi Pengisian Ulang (FIN-LGS-057)

/// <summary>Saringan Rekomendasi Pengisian Ulang.</summary>
public class ReplenishmentQuery
{
    public Guid? StorageLocationId { get; set; }
    public Guid? DrugId { get; set; }
    [MaxLength(100)] public string? Search { get; set; }

    /// <summary>Saring menurut <see cref="ReplenishmentStatuses"/>; kosong = semua yang ditampilkan.</summary>
    [MaxLength(30)] public string? Status { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Status satu kebijakan stok pada daftar rekomendasi.</summary>
public static class ReplenishmentStatuses
{
    /// <summary>Kebijakan valid dan siap pakai ≤ titik pesan ulang: ada jumlah usulan.</summary>
    public const string Recommended = "Recommended";

    /// <summary>Syarat kebijakan tidak terpenuhi: tidak ada jumlah usulan.</summary>
    public const string PolicyIncomplete = "PolicyIncomplete";

    /// <summary>Satuan kebijakan ≠ satuan stok obat (R5): dikecualikan, tanpa konversi.</summary>
    public const string UnitMismatch = "UnitMismatch";
}

/// <summary>Alasan kebijakan dinyatakan belum lengkap.</summary>
public static class ReplenishmentIncompleteReasons
{
    public const string NoLocation = "NoLocation";
    public const string LocationInactive = "LocationInactive";
    public const string ReorderPointNotSet = "ReorderPointNotSet";
    public const string MaximumNotAboveReorderPoint = "MaximumNotAboveReorderPoint";
}

public class ReplenishmentRowResponse
{
    public Guid StockPolicyId { get; set; }
    public string StockPolicyCode { get; set; } = string.Empty;
    public Guid DrugId { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;

    /// <summary>Satuan stok obat (bukan satuan kebijakan); dipakai sebagai satuan permintaan.</summary>
    public Guid? StockUnitMeasurementId { get; set; }
    public string? StockUnitName { get; set; }

    public Guid? StorageLocationId { get; set; }
    public string StorageLocationName { get; set; } = string.Empty;
    public bool IsMainWarehouse { get; set; }

    /// <summary>Unit layanan pemilik lokasi; mengisi "unit peminta" pada formulir bila ada.</summary>
    public Guid? RequestingServiceUnitId { get; set; }
    public string? RequestingServiceUnitName { get; set; }

    /// <summary>Saldo fisik berstatus siap pakai (dasar R1, sama dengan layar Hampir Habis).</summary>
    public decimal QuantityAvailable { get; set; }

    public decimal QuantityReserved { get; set; }
    public decimal ReorderPointQuantity { get; set; }
    public decimal MaximumStockQuantity { get; set; }

    /// <summary>Maksimum − siap pakai, dibulatkan ke atas. Null bila tidak dapat dihitung.</summary>
    public decimal? RecommendedQuantity { get; set; }

    public string Status { get; set; } = ReplenishmentStatuses.PolicyIncomplete;
    public List<string> IncompleteReasons { get; set; } = [];

    /// <summary>Benar hanya untuk usulan pada lokasi yang dipenuhi dari gudang internal (bukan gudang induk).</summary>
    public bool CanCreateRequest { get; set; }
}

public class ReplenishmentSummaryResponse
{
    public int ActivePolicyCount { get; set; }
    public int RecommendedCount { get; set; }
    public int PolicyIncompleteCount { get; set; }
    public int UnitMismatchCount { get; set; }

    /// <summary>Kebijakan valid yang siap pakainya masih di atas titik pesan ulang (tidak ditampilkan).</summary>
    public int AboveReorderPointCount { get; set; }
}

public class ReplenishmentResponse
{
    public DateTime GeneratedAtUtc { get; set; }
    public ReplenishmentSummaryResponse Summary { get; set; } = new();
    public PagedResult<ReplenishmentRowResponse> Rows { get; set; } = new();
    public List<string> Limitations { get; set; } = [];
}
