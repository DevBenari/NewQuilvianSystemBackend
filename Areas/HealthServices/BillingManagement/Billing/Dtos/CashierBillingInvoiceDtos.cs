using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public static class CashierBillingPeriodPresets
{
    public const string All = "ALL";
    public const string Today = "TODAY";
    public const string Last7Days = "LAST_7_DAYS";
    public const string Last30Days = "LAST_30_DAYS";
    public const string Custom = "CUSTOM";
}

public static class CashierPaymentStatuses
{
    public const string Unpaid = "UNPAID";
    public const string Partial = "PARTIAL";
    public const string Paid = "PAID";
}

public static class CashierDepositStatuses
{
    public const string NotApplicable = "NOT_APPLICABLE";
    public const string NoDeposit = "NO_DEPOSIT";
    public const string AvailableNotUsed = "AVAILABLE_NOT_USED";
    public const string Used = "USED";
    public const string Exhausted = "EXHAUSTED";
}

/// <summary>Nilai <c>CashierBillingInvoiceListItemResponse.CalculationState</c> (<c>BE-BKC-FIX-020</c>).</summary>
public static class CashierCalculationStates
{
    public const string NotCalculated = "NOT_CALCULATED";
    public const string Stale = "STALE";
    public const string Current = "CURRENT";
}

public static class CashierReminderStatuses
{
    public const string NeverSent = "NEVER_SENT";
    public const string Sent = "SENT";
    public const string Due = "DUE";
    public const string NotApplicable = "NOT_APPLICABLE";
}

public sealed class CashierBillingInvoiceQuery
{
    public string? Search { get; set; }
    public string? PeriodPreset { get; set; }
    public DateTime? VisitDateFrom { get; set; }
    public DateTime? VisitDateTo { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? ServiceType { get; set; }
    public string? PaymentStatus { get; set; }
    public string? DepositStatus { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class CashierBillingInvoiceListItemResponse
{
    // Identitas
    public Guid Id => InvoiceId;
    public Guid InvoiceId { get; set; }
    public Guid EncounterId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    // Pasien
    public Guid PatientId { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;

    // Konteks Layanan
    public string EncounterType { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string PatientType { get; set; } = string.Empty;
    public string GuarantorName { get; set; } = string.Empty;
    public string? ClaimMethod { get; set; }
    public bool HasInsurancePayer { get; set; }
    public DateTime? VisitDate { get; set; }
    public DateTime CreateDateTime { get; set; }

    // Biaya & Tanggal Bayar
    // TotalInvoiceAmount: gross sebelum coverage penjamin (subtotal + admin/room + tax - diskon
    // item). TotalBillAmount: kewajiban pasien setelah coverage (PatientAmount).
    public decimal TotalInvoiceAmount { get; set; }
    public decimal TotalBillAmount { get; set; }
    public DateTimeOffset? LastSuccessfulPaymentAt { get; set; }

    // Sisa Pembayaran
    public decimal TotalPaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string PaymentStatus { get; set; } = CashierPaymentStatuses.Unpaid;

    // Kekinian angka (BE-BKC-FIX-020, K-07, ISSUE-BKC-001 ISS-07). Field tambahan; field lama
    // tidak berubah makna supaya konsumen lama tetap berjalan.

    /// <summary>
    /// <c>CURRENT</c> = hitungan terakhir masih mencakup seluruh item dan diskon; <c>STALE</c> = ada
    /// item/diskon yang berubah sesudah hitungan terakhir (angka hitungan tertinggal);
    /// <c>NOT_CALCULATED</c> = invoice belum pernah dihitung. Aturan kekiniannya sama dengan
    /// penjaga finalisasi. Biaya kamar rawat inap yang masih berjalan tidak ikut menandai STALE.
    /// </summary>
    public string CalculationState { get; set; } = CashierCalculationStates.NotCalculated;

    /// <summary>Waktu hitungan terakhir; kosong bila belum pernah dihitung.</summary>
    public DateTimeOffset? CalculatedAt { get; set; }

    /// <summary>Jumlah Qty × harga item aktif saat ini, sebelum admin, kamar, pajak, diskon, dan penjamin.</summary>
    public decimal RunningGrossAmount { get; set; }

    /// <summary>
    /// <c>true</c> bila <see cref="TotalBillAmount"/>/<see cref="OutstandingAmount"/> bukan hasil hitungan
    /// terkini — perkiraan dari item (pasien tunai belum dihitung), nol karena tanggungan penjamin belum
    /// diketahui, atau hitungan yang tertinggal. Layar tidak boleh menyajikannya sebagai kewajiban pasti.
    /// </summary>
    public bool IsPatientAmountProvisional { get; set; }

    /// <summary>Kunjungan tanpa penjamin (tunai); tanpa hitungan, perkiraannya adalah jumlah item.</summary>
    public bool IsCashPayer { get; set; }

    /// <summary>
    /// Pelayanan yang sudah layak ditagih tetapi belum menjadi item invoice (menunggu diteruskan,
    /// gagal, atau perlu ditinjau) — <c>BillingPendingChargeQuery</c>.
    /// </summary>
    public int PendingChargeCount { get; set; }

    // Info & Status Deposit (RANAP)
    public bool HasDepositAccount { get; set; }
    public decimal DepositAvailableBalance { get; set; }
    public decimal DepositTotalReceived { get; set; }
    public decimal DepositTotalAllocated { get; set; }
    public decimal DepositPolicyShortfallAmount { get; set; }
    public decimal DepositFinalBillShortfallAmount { get; set; }
    public decimal DepositOutstandingTopUp { get; set; }
    public string DepositStatus { get; set; } = CashierDepositStatuses.NotApplicable;

    // Umur Tagihan & Pengingat
    public int BillingAgeDays { get; set; }
    public int ReminderCount { get; set; }
    public DateTime? LastReminderAt { get; set; }
    public string ReminderStatus { get; set; } = CashierReminderStatuses.NeverSent;
    public bool CanSendReminder { get; set; }
}

public sealed class SendPaymentReminderRequest
{
    [MaxLength(30)] public string Channel { get; set; } = "WHATSAPP";
    [MaxLength(50)] public string? MessageTemplateCode { get; set; }
}

public sealed class SendPaymentReminderResponse
{
    public Guid InvoiceId { get; set; }
    public Guid? ReminderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public int ReminderCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsDelivered { get; set; }
}
