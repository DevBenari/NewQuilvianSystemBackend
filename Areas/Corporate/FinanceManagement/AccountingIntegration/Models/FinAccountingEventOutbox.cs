using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

/// <summary>
/// Kotak keluar kejadian Finance -> Accounting (transactional outbox, FIN-DES-017). Ditulis di
/// transaksi yang sama dengan fakta bisnis yang melahirkannya, sehingga tidak mungkin ada fakta
/// tanpa kejadian atau sebaliknya (FR-FIN-070). Dua lapis unique (EventNumber; kombinasi
/// SourceModule+SourceTransactionId+EventTypeCode+SourceVersion) mencegah jurnal ganda
/// (FIN-DES-019). PayloadJson/ComponentsJson MUST NOT memuat data pasien maupun DoctorId
/// (aturan bisnis #6, FR-FIN-073). Pengiriman (worker, penanganan balasan) di luar lingkup task
/// ini — endpoint Accounting belum dibangun (FIN-CAP-018).
/// </summary>
[Table("FinAccountingEventOutbox", Schema = "public")]
public sealed class FinAccountingEventOutbox : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string EventNumber { get; set; } = string.Empty;

    /// <summary>Salah satu dari 17 kode yang diusulkan FIN-DEC-002 — belum diratifikasi Accounting,
    /// sehingga tidak dibatasi check constraint di sisi Finance.</summary>
    [Required, MaxLength(50)] public string EventTypeCode { get; set; } = string.Empty;

    [Required, MaxLength(30)] public string SourceModule { get; set; } = "Finance";

    [Required, MaxLength(50)] public string SourceTransactionId { get; set; } = string.Empty;

    /// <summary>Dinaikkan saat koreksi, bukan dipakai ulang (FIN-DES-019, FR-FIN-072).</summary>
    [Required, MaxLength(20)] public string SourceVersion { get; set; } = "1";

    public DateTimeOffset EventOccurredAt { get; set; }

    public DateOnly AccountingDate { get; set; }

    public decimal Amount { get; set; }

    [Required, MaxLength(3)] public string CurrencyCode { get; set; } = "IDR";

    /// <summary>Rujukan badan hukum shared — bukan FK.</summary>
    public Guid LegalEntityId { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }

    /// <summary>Daftar { ComponentCode, Amount } opsional — tanpa itu seluruh nilai dianggap TOTAL.</summary>
    public string? ComponentsJson { get; set; }

    /// <summary>Salinan persis pesan yang dikirim. Sensitif secara kepatuhan: MUST NOT memuat
    /// nama/nomor rekam medis/nomor kunjungan pasien maupun DoctorId (FR-FIN-073).</summary>
    [Required] public string PayloadJson { get; set; } = string.Empty;

    [Required, MaxLength(30)] public string DeliveryStatus { get; set; } = FinAccountingEventDeliveryStatuses.Pending;

    /// <summary>Terisi hanya untuk status HELD.</summary>
    [MaxLength(300)] public string? HoldReason { get; set; }

    public int AttemptCount { get; set; } = 0;

    public DateTimeOffset? LastAttemptAt { get; set; }
    public int? LastResponseCode { get; set; }

    [MaxLength(50)] public string? AccountingReceiptNumber { get; set; }
    [MaxLength(50)] public string? AccountingJournalNumber { get; set; }

    /// <summary>Optimistic concurrency (FIN-DES-005) — di-set ulang Guid.NewGuid() setiap perubahan.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinAccountingEventAttempt> Attempts { get; set; } = new List<FinAccountingEventAttempt>();
}

public static class FinAccountingEventSourceModules
{
    public const string Finance = "Finance";
}

public static class FinAccountingEventDeliveryStatuses
{
    public const string Pending = "PENDING";
    public const string HeldForFinalization = "HELD_FOR_FINALIZATION";
    public const string Sent = "SENT";
    public const string Acknowledged = "ACKNOWLEDGED";
    public const string Held = "HELD";
    public const string Failed = "FAILED";
}

/// <summary>Katalog 17 kode yang diusulkan FIN-DEC-002 (integration-contract.md §5.4) — starting
/// point yang menunggu ratifikasi Accounting, bukan daftar yang ditegakkan check constraint.</summary>
public static class FinAccountingEventTypeCodes
{
    // Kode alias standar V2
    public const string ArCreated = "AR_CREATED";
    public const string ArPayment = "AR_PAYMENT";
    public const string ArWriteOff = "AR_WRITEOFF";
    public const string ApCreated = "AP_CREATED";
    public const string ApPayment = "AP_PAYMENT";

    // Katalog 17 kode yang diusulkan FIN-DEC-002
    public const string PengakuanPiutang = "PENGAKUAN-PIUTANG";
    public const string PenerimaanPiutang = "PENERIMAAN-PIUTANG";
    public const string PenyesuaianPiutang = "PENYESUAIAN-PIUTANG";
    public const string PemutihanPiutang = "PEMUTIHAN-PIUTANG";
    public const string PengakuanHutangSupplier = "PENGAKUAN-HUTANG-SUPPLIER";
    public const string PembayaranHutangSupplier = "PEMBAYARAN-HUTANG-SUPPLIER";
    public const string PengakuanHutangDokter = "PENGAKUAN-HUTANG-DOKTER";
    public const string PembayaranHutangDokter = "PEMBAYARAN-HUTANG-DOKTER";
    public const string PenyesuaianHutang = "PENYESUAIAN-HUTANG";
    public const string SetoranBank = "SETORAN-BANK";
    public const string PettyCashTopUp = "PETTY-CASH-TOP-UP";
    public const string PettyCashDisbursement = "PETTY-CASH-DISBURSEMENT";
    public const string PettyCashReturn = "PETTY-CASH-RETURN";
    public const string PettyCashReversal = "PETTY-CASH-REVERSAL";
    public const string PettyCashAdjustment = "PETTY-CASH-ADJUSTMENT";
    public const string PenerimaanKasir = "PENERIMAAN-KASIR";
    public const string PembalikanPenerimaanKasir = "PEMBALIKAN-PENERIMAAN-KASIR";
}
