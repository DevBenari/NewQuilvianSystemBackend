using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.DTOs;

/// <summary>
/// Parameter query untuk laporan utama pendapatan kasir per shift.
/// </summary>
public sealed class CashierShiftRevenueReportQuery
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public Guid? CashierId { get; set; }
    public string? Location { get; set; }
    public Guid? ShiftId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Parameter query untuk filter dropdown kasir (searchable).
/// </summary>
public sealed class CashierFilterQuery
{
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Parameter query untuk filter dropdown lokasi register (searchable).
/// </summary>
public sealed class LocationFilterQuery
{
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Parameter query untuk filter dropdown shift kasir (searchable).
/// </summary>
public sealed class ShiftFilterQuery
{
    public string? Search { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public Guid? CashierId { get; set; }
    public string? Location { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Parameter query untuk rincian transaksi dalam shift.
/// </summary>
public sealed class ShiftTransactionQuery
{
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Parameter query untuk transaksi Petty Cash dalam shift.
/// </summary>
public sealed class ShiftPettyCashQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Parameter query untuk laporan tagihan dan pembayaran asuransi.
/// </summary>
public sealed class InsuranceReportQuery
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Search { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Parameter query untuk ekspor laporan pendapatan kasir.
/// </summary>
public sealed class CashierShiftExportQuery
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public Guid? CashierId { get; set; }
    public string? Location { get; set; }
    public Guid? ShiftId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Status { get; set; }
    public string Format { get; set; } = "xlsx"; // "xlsx" atau "csv"
}

/// <summary>
/// Opsi dropdown untuk filter dinamis.
/// </summary>
public sealed class FilterOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? CashierCode { get; set; }
    public string? SubLabel { get; set; }
}

/// <summary>
/// Respons utama Laporan Pendapatan Kasir per Shift.
/// </summary>
public sealed class CashierShiftRevenueReportResponse
{
    public CashierShiftRevenueSummaryDto Summary { get; set; } = new();
    public PagedResult<CashierShiftRevenueItemDto> Shifts { get; set; } = new();
    public CashierShiftChartsDto Charts { get; set; } = new();
}

/// <summary>
/// Ringkasan agregat seluruh hasil filter (bukan hanya halaman aktif).
/// </summary>
public sealed class CashierShiftRevenueSummaryDto
{
    public int TotalShifts { get; set; }
    public decimal TotalOpeningCash { get; set; }
    public decimal TotalCashReceived { get; set; }
    public decimal TotalExpectedCash { get; set; }
    public decimal? TotalPhysicalCash { get; set; }
    public decimal? TotalCashVariance { get; set; }
    public decimal InsuranceArAmount { get; set; }
    public decimal InsuranceCollectedAmount { get; set; }
    public decimal UnattributedInsuranceAmount { get; set; }
    public decimal TotalPettyCashDisbursed { get; set; }
    public decimal TotalPettyCashReturned { get; set; }
    public decimal TotalPettyCashNet { get; set; }
    public decimal TotalNonCashReceived { get; set; }
}

/// <summary>
/// Baris per shift kasir pada tabel dan card list.
/// </summary>
public sealed class CashierShiftRevenueItemDto
{
    public Guid ShiftId { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public Guid RegisterId { get; set; }
    public string RegisterName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal OpeningCash { get; set; }
    public decimal CashReceived { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal? PhysicalCash { get; set; }
    public decimal? Variance { get; set; }
    public decimal NonCashReceived { get; set; }
    public decimal? InsuranceAttributedAmount { get; set; }
    public int PettyCashMatchedCount { get; set; }
    public decimal PettyCashMatchedNet { get; set; }
    public string PettyCashAttributionStatus { get; set; } = "NONE"; // MATCHED, NONE, NEEDS_VERIFICATION
}

/// <summary>
/// Data agregat untuk visualisasi grafik.
/// </summary>
public sealed class CashierShiftChartsDto
{
    public List<ShiftRevenueChartItemDto> ShiftRevenues { get; set; } = new();
    public List<PaymentMethodCompositionDto> PaymentComposition { get; set; } = new();
}

/// <summary>
/// Data bar chart per shift.
/// </summary>
public sealed class ShiftRevenueChartItemDto
{
    public string ShiftNumber { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
    public decimal CashReceived { get; set; }
    public decimal NonCashReceived { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// Data komposisi metode pembayaran.
/// </summary>
public sealed class PaymentMethodCompositionDto
{
    public string MethodType { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Rincian transaksi dalam shift kasir.
/// </summary>
public sealed class ShiftTransactionItemDto
{
    public Guid Id { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Pembayaran Tagihan" atau "Top-Up Deposit"
    public string PaymentMethodName { get; set; } = string.Empty;
    public string PaymentMethodType { get; set; } = string.Empty;
    public bool IsCash { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// Rincian transaksi Petty Cash yang dilayani petugas shift.
/// </summary>
public sealed class ShiftPettyCashItemDto
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTimeOffset? DisbursedAt { get; set; }
    public string DisbursedByName { get; set; } = string.Empty;
    public string? ConfirmedByName { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ReturnedAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AttributionStatus { get; set; } = string.Empty; // "Terkait Petugas & Waktu Shift", "Perlu Verifikasi"
}

/// <summary>
/// Rincian tagihan A/R dan pembayaran asuransi.
/// </summary>
public sealed class InsuranceReportItemDto
{
    public Guid Id { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public decimal ArAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AttributionStatus { get; set; } = "BELUM_TERATRIBUSI"; // "TERATRIBUSI" atau "BELUM_TERATRIBUSI"
    public string? ShiftNumber { get; set; }
}
