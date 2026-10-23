namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

// Riwayat pembayaran GABUNGAN satu kunjungan, dipakai tab "Riwayat Pembayaran" pada Menu Pembayaran.
//
// Berbeda dari halaman "Riwayat Pembayaran" lintas pasien (PaymentHistoryItemResponse: satu baris per
// invoice, hanya tender), daftar ini satu baris per KEJADIAN uang pada kunjungan itu, dari tiga sumber
// yang sudah ada di ledger:
//   - tender (Tunai/QRIS/Transfer Bank) pada SEMUA invoice kunjungan,
//   - top-up deposit (termasuk Deposit Awal),
//   - pemakaian deposit ke invoice (alokasi).
// Pemakaian deposit tidak punya tender, jadi sebelumnya tidak pernah muncul di riwayat meski sudah
// mengurangi sisa tagihan.
public static class EncounterPaymentHistoryEntryTypes
{
    public const string TenderPayment = "TENDER_PAYMENT";
    public const string DepositTopUp = "DEPOSIT_TOP_UP";
    public const string DepositAllocation = "DEPOSIT_ALLOCATION";
}

public sealed class EncounterPaymentHistoryResponse
{
    public Guid EncounterId { get; set; }

    // Kronologis (terlama dulu), tanpa paginasi: jumlah kejadian uang per kunjungan kecil.
    public IReadOnlyList<EncounterPaymentHistoryEntryResponse> Entries { get; set; } = [];

    public EncounterPaymentHistorySummaryResponse Summary { get; set; } = new();
}

public sealed class EncounterPaymentHistorySummaryResponse
{
    // Hanya tender berstatus SUCCEEDED.
    public decimal TotalTenderReceived { get; set; }

    // Top-up deposit yang belum dibalik.
    public decimal TotalDepositTopUp { get; set; }

    // Pemakaian deposit ke invoice yang belum dipulihkan.
    public decimal TotalDepositUsed { get; set; }

    public int EntryCount { get; set; }
}

public sealed class EncounterPaymentHistoryEntryResponse
{
    // Id tender atau Id mutasi deposit, sesuai EntryType.
    public Guid Id { get; set; }

    // Lihat EncounterPaymentHistoryEntryTypes.
    public string EntryType { get; set; } = string.Empty;
    public string EntryTypeName { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    // Nomor Kwitansi untuk tender; nomor transaksi deposit (DEP-...) untuk mutasi deposit.
    public string DocumentNumber { get; set; } = string.Empty;

    // Hanya tender yang sudah bernomor Kwitansi yang dapat dicetak kwitansinya.
    public bool HasKwitansi { get; set; }

    // Null untuk top-up deposit (tidak terkait invoice).
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid? SettlementId { get; set; }

    public Guid? PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;

    // "Bank - nomor rekening (a.n. pemilik)" bila metode memakai rekening/channel.
    public string? PaymentChannel { get; set; }
    public string? ProviderReferenceMasked { get; set; }

    public decimal Amount { get; set; }

    // Tender: status tender (SUCCEEDED/PENDING/FAILED/EXPIRED/REVERSED/CREATED).
    // Mutasi deposit: SUCCEEDED, atau REVERSED bila sudah dibalik/dipulihkan di ledger.
    public string Status { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;

    // Catatan kasir (tender) atau alasan (mutasi deposit).
    public string? Note { get; set; }
    public string? CashierName { get; set; }

    // Hanya untuk tender: klasifikasi terhadap target settlement-nya. PELUNASAN bila tender ini yang
    // memenuhi seluruh target settlement, CICILAN bila belum. Null untuk mutasi deposit.
    public string? TransactionClassification { get; set; }
    public string? PostTransactionStatus { get; set; }
    public decimal? RemainingOutstanding { get; set; }
}
