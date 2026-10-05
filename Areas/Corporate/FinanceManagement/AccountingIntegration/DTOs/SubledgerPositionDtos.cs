namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;

/// <summary>
/// Data representasi posisi saldo subledger per tanggal bisnis (WIB) (BE-FIN-067, FIN-DES-081, FIN-API-1.5 F.6).
/// Murni dihitung dari saldo awal cutover ditambah pergerakan buku mutasi (FinCashMovement, FinReceivableMovement, FinSupplierPayableMovement).
/// </summary>
public sealed class SubledgerPositionResponse
{
    /// <summary>
    /// Tanggal acuan perhitungan posisi saldo (WIB).
    /// </summary>
    public DateOnly AsOfDate { get; set; }

    /// <summary>
    /// Tanggal cutover peralihan sistem yang berlaku.
    /// </summary>
    public DateOnly CutoverDate { get; set; }

    /// <summary>
    /// Waktu eksekusi kalkulasi dilakukan.
    /// </summary>
    public DateTimeOffset CalculatedAt { get; set; }

    /// <summary>
    /// Total penjumlahan posisi seluruh kelompok saldo pada tanggal asOfDate.
    /// </summary>
    public decimal TotalPosition { get; set; }

    /// <summary>
    /// Rincian posisi saldo per kelompok subledger.
    /// </summary>
    public List<SubledgerGroupPositionItem> Groups { get; set; } = new();
}

/// <summary>
/// Rincian posisi saldo satu kelompok subledger (KAS-KASIR, KAS-KECIL, PIUTANG, UTANG-SUPPLIER, UTANG-JASA-MEDIS).
/// </summary>
public sealed class SubledgerGroupPositionItem
{
    /// <summary>
    /// Kode kelompok saldo subledger.
    /// </summary>
    public string BalanceGroup { get; set; } = string.Empty;

    /// <summary>
    /// Nama deskriptif kelompok saldo.
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Nominal saldo awal cutover dari FinOpeningBalance.
    /// </summary>
    public decimal OpeningAmount { get; set; }

    /// <summary>
    /// Total kas masuk bertanggal &lt;= asOfDate (khusus kelompok KAS-KASIR).
    /// </summary>
    public decimal TotalIn { get; set; }

    /// <summary>
    /// Total kas keluar bertanggal &lt;= asOfDate (khusus kelompok KAS-KASIR).
    /// </summary>
    public decimal TotalOut { get; set; }

    /// <summary>
    /// Total akumulasi mutasi saldo bertanggal &lt;= asOfDate (khusus kelompok PIUTANG dan UTANG-SUPPLIER).
    /// </summary>
    public decimal TotalMovementAmount { get; set; }

    /// <summary>
    /// Posisi saldo terhitung akhir per tanggal asOfDate.
    /// </summary>
    public decimal CalculatedPosition { get; set; }

    /// <summary>
    /// Rincian per segmen jika kelompok saldo mendukung segmentasi (misal: PIUTANG per DebtorType).
    /// </summary>
    public List<SubledgerSegmentPositionItem> Segments { get; set; } = new();
}

/// <summary>
/// Rincian posisi saldo per segmen debitur atau tenaga medis.
/// </summary>
public sealed class SubledgerSegmentPositionItem
{
    /// <summary>
    /// Kunci kode segmen (misal: PAYER, PATIENT_GUARANTOR, EMPLOYEE_BENEFIT).
    /// </summary>
    public string SegmentKey { get; set; } = string.Empty;

    /// <summary>
    /// Nama deskriptif segmen.
    /// </summary>
    public string SegmentName { get; set; } = string.Empty;

    /// <summary>
    /// Nominal posisi saldo terhitung untuk segmen ini.
    /// </summary>
    public decimal Amount { get; set; }
}

/// <summary>
/// Data representasi perbandingan selisih rekap kas harian terhadap posisi kas terhitung satu periode (BE-FIN-067, FIN-DEC-125, FIN-API-1.5 F.6).
/// </summary>
public sealed class CashVarianceResponse
{
    /// <summary>
    /// Kode periode akuntansi (format YYYY-MM).
    /// </summary>
    public string AccountingPeriodCode { get; set; } = string.Empty;

    /// <summary>
    /// Tanggal awal periode akuntansi.
    /// </summary>
    public DateOnly PeriodStartDate { get; set; }

    /// <summary>
    /// Tanggal akhir periode akuntansi (WIB).
    /// </summary>
    public DateOnly PeriodEndDate { get; set; }

    /// <summary>
    /// Tanggal cutover peralihan sistem.
    /// </summary>
    public DateOnly CutoverDate { get; set; }

    /// <summary>
    /// Posisi Kas Kasir terhitung dari buku mutasi kas bertanggal &lt;= PeriodEndDate.
    /// </summary>
    public decimal CalculatedCashPosition { get; set; }

    /// <summary>
    /// Saldo akhir kas harian dari FinDailyCashSnapshot terakhir pada rentang periode terkait.
    ///
    /// BE-FIN-090 (FIN-DEC-152, FIN-DES-098) — PERUBAHAN MEMUTUS: ruas ini SEKARANG boleh
    /// <c>null</c>. Sebelumnya selalu berisi dan bernilai <c>0</c> bila rekap tidak ada — nol itu
    /// BUKAN angka rekap, dan menampilkannya sebagai saldo sungguhan adalah cacat yang task ini
    /// perbaiki. <c>null</c> berarti periode itu belum punya rekap kas harian; periksa
    /// <see cref="HasDailyCashSnapshot"/>, JANGAN menyimpulkan dari nilai <c>0</c>.
    /// </summary>
    public decimal? DailyCashClosingBalance { get; set; }

    /// <summary>
    /// Tanggal snapshot kas harian terakhir yang ditemukan dalam periode terkait.
    /// </summary>
    public DateOnly? DailyCashSnapshotDate { get; set; }

    /// <summary>
    /// Status snapshot kas harian terakhir (OPEN atau CLOSED).
    /// </summary>
    public string? DailyCashSnapshotStatus { get; set; }

    /// <summary>
    /// Besaran selisih antara rekap harian dan posisi terhitung (DailyCashClosingBalance - CalculatedCashPosition).
    ///
    /// BE-FIN-090 — PERUBAHAN MEMUTUS: boleh <c>null</c> ketika <see cref="HasDailyCashSnapshot"/>
    /// bernilai salah, dengan alasan yang sama seperti <see cref="DailyCashClosingBalance"/>.
    /// </summary>
    public decimal? VarianceAmount { get; set; }

    /// <summary>
    /// Menandakan apakah terdapat selisih antara rekap kas harian dan posisi kas terhitung.
    ///
    /// BE-FIN-090 — PERUBAHAN MEMUTUS: sebelumnya bernilai <c>true</c> ketika rekap tidak ada
    /// (karena <c>0 - posisi</c> hampir selalu bukan nol). Sekarang SELALU <c>false</c> ketika
    /// <see cref="HasDailyCashSnapshot"/> salah — tidak ada selisih yang dapat dinyatakan tanpa
    /// angka rekap sungguhan untuk dibandingkan. Hanya <c>true</c> bila rekap ADA dan berbeda.
    /// </summary>
    public bool HasVariance { get; set; }

    /// <summary>
    /// BE-FIN-090 (FIN-DEC-152, FIN-DES-098) — ruas BARU. <c>false</c> berarti periode itu belum
    /// punya rekap kas harian (<c>FinDailyCashSnapshot</c> tidak ditemukan pada rentang periode).
    /// Ditambahkan supaya tafsir "belum ada rekap" tertulis eksplisit pada kontrak, bukan
    /// disimpulkan setiap pembaca dari ketiadaan <see cref="DailyCashSnapshotDate"/>.
    /// </summary>
    public bool HasDailyCashSnapshot { get; set; }

    /// <summary>
    /// Daftar mutasi kas FinCashMovement pada rentang periode terkait yang menjelaskan perubahan kas.
    /// </summary>
    public List<CashMovementExplainingItem> ExplainingMovements { get; set; } = new();
}

/// <summary>
/// Baris transaksi mutasi kas yang menjelaskan pergerakan kas pada periode terkait (FIN-DEC-125).
/// </summary>
public sealed class CashMovementExplainingItem
{
    public Guid Id { get; set; }

    public string MovementType { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateOnly BusinessDate { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string SourceReferenceType { get; set; } = string.Empty;

    public string SourceReferenceId { get; set; } = string.Empty;

    public string? PaymentMethodCode { get; set; }

    public Guid? CashierShiftId { get; set; }

    public string? Notes { get; set; }
}
