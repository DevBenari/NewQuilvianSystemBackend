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

    // Biaya & Tanggal Bayar
    public decimal TotalBillAmount { get; set; }
    public DateTimeOffset? LastSuccessfulPaymentAt { get; set; }

    // Sisa Pembayaran
    public decimal TotalPaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string PaymentStatus { get; set; } = CashierPaymentStatuses.Unpaid;

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
