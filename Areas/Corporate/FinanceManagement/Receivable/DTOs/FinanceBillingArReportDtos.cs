using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>
/// Parameter pencarian dan penyaringan untuk Laporan Billing ke Finance AR (Billing AR Report).
/// Read-only, tidak mengubah data dan tidak memiliki side effect.
/// </summary>
public sealed class BillingArReportQuery
{
    /// <summary>Kata kunci pencarian: nomor bill/invoice, nomor registrasi, no RM, nama pasien, atau penjamin.</summary>
    public string? Search { get; set; }

    /// <summary>ID penjamin (perusahaan atau asuransi).</summary>
    public Guid? PayerId { get; set; }

    /// <summary>Jenis kunjungan: ALL, OP (Rawat Jalan), IP (Rawat Inap), IGD (Emergency).</summary>
    public string? VisitType { get; set; }

    /// <summary>Preset periode: ALL, TODAY, THIS_WEEK, THIS_MONTH, LAST_MONTH, THIS_YEAR, CUSTOM.</summary>
    public string? PeriodPreset { get; set; }

    /// <summary>Tanggal awal (inklusif).</summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>Tanggal akhir (inklusif).</summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>Status Billing: ALL, OPEN, FINAL, CLOSED, SETTLED_BY_WRITE_OFF. Default: ALL (Semua Status Billing).</summary>
    public string? BillingStatus { get; set; } = "ALL";

    /// <summary>Status Integrasi Finance AR: ALL, NOT_HANDED_OFF, PENDING, SUCCESS, FAILED, NOT_APPLICABLE.</summary>
    public string? FinanceHandoffStatus { get; set; } = "ALL";

    /// <summary>Status pembuatan Invoice AR: ALL, NOT_CREATED (Belum Dibuat), DRAFT (Draft), ISSUED (Sudah Dibuat), CANCELLED (Dibatalkan).</summary>
    public string? ArInvoiceStatus { get; set; } = "ALL";

    /// <summary>Halaman aktif (1-based).</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Ukuran halaman: tetap maksimal 30 baris invoice per halaman.</summary>
    public int PageSize { get; set; } = 30;
}

/// <summary>
/// Baris data tagihan Billing per pasien untuk Laporan Billing ke Finance AR.
/// Setiap baris merepresentasikan 1 invoice Billing beserta status siklus hidupnya ke Finance AR dan Invoice AR.
/// </summary>
public sealed class BillingArReportItemResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid BillingInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string BillNumber => InvoiceNumber;

    public Guid? EncounterId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string EncounterNumber => RegistrationNumber;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PatientAgeText { get; set; } = string.Empty;

    public DateTimeOffset? BillingDate { get; set; }
    public DateTime? VisitDate { get; set; }
    public DateTime? AdmissionDate => VisitDate;
    public DateTimeOffset? DischargeDate { get; set; }

    public string ClinicOrRoomName { get; set; } = "-";

    public Guid? PayerId { get; set; }
    public string PayerType { get; set; } = string.Empty; // INSURANCE, COMPANY, OTHER
    public string PayerName { get; set; } = string.Empty;

    public string VisitType { get; set; } = string.Empty; // OP, IP, IGD, OTHER
    public string VisitTypeLabel { get; set; } = string.Empty;

    public decimal CoveredBillingAmount { get; set; }
    public decimal BillingAmount => CoveredBillingAmount;
    public decimal ExcessAmount { get; set; }
    public bool IsEstimated { get; set; }

    // 1. Status Billing
    public string BillingStatus { get; set; } = string.Empty; // OPEN, FINAL, CLOSED, SETTLED_BY_WRITE_OFF
    public string BillingStatusLabel { get; set; } = string.Empty; // On Process, Final, Closed, Write-off
    public string BillingSettlementStatus => BillingStatus;

    // 2. Status Integrasi Finance AR
    public string FinanceHandoffStatus { get; set; } = "NOT_HANDED_OFF"; // NOT_HANDED_OFF, PENDING, SUCCESS, FAILED, NOT_APPLICABLE
    public string FinanceHandoffStatusLabel { get; set; } = "Belum Handoff";
    public Guid? FinanceReceivableId { get; set; }
    public string? FinanceReceivableNumber { get; set; }
    public Guid? ReceivableId => FinanceReceivableId;
    public string? ReceivableNumber => FinanceReceivableNumber;

    // 3. Status Invoice AR
    public string ArInvoiceStatus { get; set; } = "NOT_CREATED"; // NOT_CREATED, DRAFT, ISSUED, CANCELLED
    public string ArInvoiceStatusLabel { get; set; } = "Belum Dibuat";
    public string ArInvoiceCreationStatus => ArInvoiceStatus;
    public Guid? ArInvoiceBatchId { get; set; }
    public string? ArInvoiceNumber { get; set; }
}

/// <summary>
/// Subtotal tagihan per tanggal di dalam kelompok penjamin.
/// </summary>
public sealed class BillingArReportDateTotalResponse
{
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD
    public string DateLabel { get; set; } = string.Empty; // DD/MM/YYYY
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Kelompok data tagihan berdasarkan penjamin (Payer/Company/Insurance) untuk halaman aktif.
/// </summary>
public sealed class BillingArReportGroupResponse
{
    public Guid? PayerId { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string GroupName => PayerName;
    public string PayerType { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal GroupTotal { get; set; }
    public List<BillingArReportDateTotalResponse> DateTotals { get; set; } = new();
    public List<BillingArReportItemResponse> Items { get; set; } = new();
}

/// <summary>
/// Ringkasan keseluruhan atas seluruh data yang memenuhi filter (monitoring counter dan nominal terpisah).
/// </summary>
public sealed class BillingArReportSummaryResponse
{
    public int InvoiceCount { get; set; }
    public int PatientCount { get; set; }

    // 6 Counters
    public int BillingOnProcessCount { get; set; }
    public int BillingClosedCount { get; set; }
    public int HandoffPendingCount { get; set; }
    public int HandoffSuccessCount { get; set; }
    public int InvoiceArNotCreatedCount { get; set; }
    public int InvoiceArCreatedCount { get; set; }

    // 3 Separated Nominals
    public decimal TotalEstimatedAmount { get; set; }
    public decimal TotalFinalBillingAmount { get; set; }
    public decimal TotalFinanceReceivableAmount { get; set; }

    // Total Nominal Global
    public decimal TotalAmount { get; set; }
    public string PayerName { get; set; } = "Semua Perusahaan / Asuransi";
    public string PeriodLabel { get; set; } = "Semua Periode";
    public decimal EntityTotalAmount { get; set; }
    public int EntityPatientCount { get; set; }
}

/// <summary>
/// Respons terpaginasi Laporan Billing ke Finance AR (30 invoice baris per halaman) beserta pengelompokan dan ringkasan global.
/// </summary>
public sealed class BillingArReportPagedResponse
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public int TotalData { get; set; }
    public int TotalPage { get; set; }
    public List<BillingArReportItemResponse> Items { get; set; } = new();
    public List<BillingArReportGroupResponse> Groups { get; set; } = new();
    public BillingArReportSummaryResponse Summary { get; set; } = new();
}
