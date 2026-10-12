using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

public sealed class PaymentArReportQuery
{
    public string? Search { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? PayerType { get; set; }
    public Guid? PayerId { get; set; }
    public string? PaymentSource { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "PaymentDate";
    public string? SortDirection { get; set; } = "desc";
}

public sealed class PaymentArReportRowResponse
{
    public string RowKey { get; set; } = string.Empty;
    public Guid ReceiptId { get; set; }
    public Guid BatchId { get; set; }
    public string DebtorName { get; set; } = string.Empty;
    public string PayerType { get; set; } = string.Empty;
    public string PaymentSource { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string BankRecipient { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? CrossEntryNumber { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalTagihan { get; set; }
    public decimal TotalPembayaran { get; set; }
    public decimal SisaPembayaran { get; set; }
}

public sealed class PaymentArReportSummaryResponse
{
    public int TotalTransaksi { get; set; }
    public decimal TotalTagihan { get; set; }
    public decimal TotalBayar { get; set; }
    public decimal TotalSisa { get; set; }
}

public sealed class PaymentArReportPagedResponse
{
    public List<PaymentArReportRowResponse> Items { get; set; } = [];
    public PaymentArReportSummaryResponse Summary { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalData { get; set; }
    public int TotalPage { get; set; }
}

public sealed class PaymentArDetailSummaryResponse
{
    public int JumlahDetail { get; set; }
    public decimal TotalPiutang { get; set; }
    public decimal PiutangTerbayar { get; set; }
    public decimal SisaPembayaran { get; set; }
}

public sealed class PaymentArDetailItemResponse
{
    public Guid ReceivableId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string BillingNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly? BillingDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string PaymentSequence { get; set; } = string.Empty;
    public decimal TotalPiutang { get; set; }
    public decimal PiutangTerbayar { get; set; }
    public decimal SisaPembayaran { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class PaymentArDetailResponse
{
    public Guid ReceiptId { get; set; }
    public Guid BatchId { get; set; }
    public string PaymentKey { get; set; } = string.Empty;
    public string DebtorName { get; set; } = string.Empty;
    public string PayerType { get; set; } = string.Empty;
    public string BankRecipient { get; set; } = string.Empty;
    public string PaymentSource { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string ReceiptNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? CrossEntryNumber { get; set; }
    public DateOnly PaymentDate { get; set; }
    public PaymentArDetailSummaryResponse Summary { get; set; } = new();
    public List<PaymentArDetailItemResponse> Items { get; set; } = [];
}
