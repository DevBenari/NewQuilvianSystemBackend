using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.DTOs;

public class ReceivableWriteOffProposalQuery
{
    public string? Search { get; set; }
    public string? ProposalNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Status { get; set; }
    public string? ServiceType { get; set; }
    public string? ReasonCode { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "createdatetime";
    public string SortDirection { get; set; } = "desc";
}

public class EligibleWriteOffInvoiceQuery
{
    public string? Search { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public string? ServiceType { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CreateReceivableWriteOffProposalRequest
{
    public string ProposalType { get; set; } = FinReceivableWriteOffProposalTypes.PerInvoice;
    public string? ServiceType { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public string ReasonCode { get; set; } = FinReceivableWriteOffReasonCodes.Uncollectible;
    public string ReasonDescription { get; set; } = string.Empty;
    public List<CreateReceivableWriteOffProposalItemRequest> Items { get; set; } = new();
}

public class CreateReceivableWriteOffProposalItemRequest
{
    public Guid InvoiceBatchId { get; set; }
    public Guid ReceivableId { get; set; }
    public decimal ProposedAmount { get; set; }
    public string? Note { get; set; }
}

public class UpdateReceivableWriteOffProposalRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string ProposalType { get; set; } = FinReceivableWriteOffProposalTypes.PerInvoice;
    public string? ServiceType { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public string ReasonCode { get; set; } = FinReceivableWriteOffReasonCodes.Uncollectible;
    public string ReasonDescription { get; set; } = string.Empty;
    public List<CreateReceivableWriteOffProposalItemRequest> Items { get; set; } = new();
}

public class SubmitReceivableWriteOffProposalRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

public class ApproveReceivableWriteOffProposalRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public List<ItemApprovalRequest>? ItemApprovals { get; set; }
}

public class ItemApprovalRequest
{
    public Guid ItemId { get; set; }
    public decimal ApprovedAmount { get; set; }
}

public class RejectReceivableWriteOffProposalRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

public class PostReceivableWriteOffProposalRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

public class ReceivableWriteOffProposalResponse
{
    public Guid Id { get; set; }
    public string ProposalNumber { get; set; } = string.Empty;
    public DateOnly ProposalDate { get; set; }
    public string ProposalType { get; set; } = string.Empty;
    public string? ServiceType { get; set; }
    public string? ServiceLabel { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public string? DebtorName { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string ReasonLabel { get; set; } = string.Empty;
    public string ReasonDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalInvoiceAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public decimal TotalProposedAmount { get; set; }
    public decimal TotalApprovedAmount { get; set; }
    public int InvoiceCount { get; set; }
    public int ItemCount { get; set; }
    public Guid RequestedBy { get; set; }
    public string? RequestedByName { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public string? PostedByName { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public string? RejectedByName { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid RowVersion { get; set; }
}

public class ReceivableWriteOffProposalDetailResponse : ReceivableWriteOffProposalResponse
{
    public List<ReceivableWriteOffProposalItemResponse> Items { get; set; } = new();
    public List<ReceivableWriteOffProposalInvoiceSummaryResponse> Invoices { get; set; } = new();
}

public class ReceivableWriteOffProposalItemResponse
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public Guid InvoiceBatchId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid ReceivableId { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string? BillingNumber { get; set; }
    public string? EncounterNumber { get; set; }
    public DateOnly? ServiceDate { get; set; }
    public string? ServiceType { get; set; }
    public string? DoctorName { get; set; }
    public decimal OutstandingSnapshot { get; set; }
    public decimal CurrentOutstandingAmount { get; set; }
    public decimal ProposedAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string? Note { get; set; }
    public Guid? WriteOffId { get; set; }
    public string? WriteOffNumber { get; set; }
}

public class ReceivableWriteOffProposalInvoiceSummaryResponse
{
    public Guid InvoiceBatchId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly? InvoiceDate { get; set; }
    public decimal TotalInvoiceAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public decimal TotalProposedAmount { get; set; }
    public decimal TotalApprovedAmount { get; set; }
    public int MemberCount { get; set; }
}

public class ReceivableWriteOffProposalSummaryResponse
{
    public int TotalProposals { get; set; }
    public int TotalDraft { get; set; }
    public int TotalSubmitted { get; set; }
    public int TotalApproved { get; set; }
    public int TotalPosted { get; set; }
    public int TotalRejected { get; set; }
    public decimal TotalProposedAmount { get; set; }
    public decimal TotalApprovedAmount { get; set; }
    public decimal TotalPostedAmount { get; set; }
}

public class EligibleWriteOffInvoiceResponse
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid DebtorReferenceId { get; set; }
    public string DebtorName { get; set; } = string.Empty;
    public DateOnly? InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? ServiceType { get; set; }
    public string? VisitCode { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public List<EligibleWriteOffInvoiceMemberResponse> Members { get; set; } = new();
}

public class EligibleWriteOffInvoiceMemberResponse
{
    public Guid ReceivableId { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal WrittenOffAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public string? BillingNumber { get; set; }
    public DateOnly? BillingDate { get; set; }
    public string? EncounterNumber { get; set; }
    public string? PatientName { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string? DoctorName { get; set; }
    public string? PatientClassName { get; set; }
    public string? ServiceType { get; set; }
    public DateOnly? ServiceDate { get; set; }
}

public class EligibleWriteOffReasonResponse
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
