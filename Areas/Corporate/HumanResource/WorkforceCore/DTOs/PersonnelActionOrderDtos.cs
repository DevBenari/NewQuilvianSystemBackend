using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class PersonnelActionOrderListResponse
    {
        public Guid Id { get; set; }
        public string ActionOrderNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public string ActionCategory { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime PostingDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string ReasonText { get; set; } = string.Empty;
        public bool HasSignedDocument { get; set; }
        public string? SignedDocumentFileName { get; set; }
        public string? TargetOrganizationUnitName { get; set; }
        public string? TargetDepartmentName { get; set; }
        public string? TargetPositionName { get; set; }
        public string? TargetManagerName { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PersonnelActionOrderDetailResponse
    {
        public Guid Id { get; set; }
        public string ActionOrderNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public string ActionCategory { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime PostingDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string ReasonText { get; set; } = string.Empty;
        public string? SignedDocumentPath { get; set; }
        public string? SignedDocumentFileName { get; set; }

        public Guid? TargetOrganizationUnitId { get; set; }
        public string? TargetOrganizationUnitName { get; set; }
        public Guid? TargetDepartmentId { get; set; }
        public string? TargetDepartmentName { get; set; }
        public Guid? TargetPositionId { get; set; }
        public string? TargetPositionName { get; set; }
        public Guid? TargetManagerProfileId { get; set; }
        public string? TargetManagerName { get; set; }

        public Guid? LinkedSalaryAssignmentId { get; set; }
        public decimal? TargetBaseSalary { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        public Guid? CancellationRequestedByUserId { get; set; }
        public DateTime? CancellationRequestedAt { get; set; }
        public string? CancellationReason { get; set; }
        public string? CounterOrderNumber { get; set; }

        public DateTime? ExecutedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public List<PersonnelActionOrderAttachmentDto> Attachments { get; set; } = new();
    }

    public class PersonnelActionOrderAttachmentDto
    {
        public Guid Id { get; set; }
        public Guid PersonnelActionOrderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string StoragePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public Guid UploadedByUserId { get; set; }
    }

    public class CreatePersonnelActionOrderRequest
    {
        public Guid WorkforceProfileId { get; set; }
        public string ActionCategory { get; set; } = "Mutation"; // Mutation, Promotion, Rotation, Demotion, TemporaryAssignment
        public DateTime EffectiveDate { get; set; }
        public string ReasonText { get; set; } = string.Empty;
        public Guid? TargetOrganizationUnitId { get; set; }
        public Guid? TargetDepartmentId { get; set; }
        public Guid? TargetPositionId { get; set; }
        public Guid? TargetManagerProfileId { get; set; }
        public decimal? TargetBaseSalary { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdatePersonnelActionOrderRequest
    {
        public string ActionCategory { get; set; } = "Mutation";
        public DateTime EffectiveDate { get; set; }
        public string ReasonText { get; set; } = string.Empty;
        public Guid? TargetOrganizationUnitId { get; set; }
        public Guid? TargetDepartmentId { get; set; }
        public Guid? TargetPositionId { get; set; }
        public Guid? TargetManagerProfileId { get; set; }
        public decimal? TargetBaseSalary { get; set; }
        public string? Notes { get; set; }
    }

    public class RejectOrderRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class CancelOrderRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string? CounterOrderNumber { get; set; }
    }

    public class AttachmentResponse
    {
        public Guid AttachmentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class PersonnelActionOrderSummaryResponse
    {
        public int TotalOrders { get; set; }
        public int DraftCount { get; set; }
        public int PendingApprovalCount { get; set; }
        public int ScheduledCount { get; set; }
        public int AppliedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class PersonnelActionOrderFilterMetadataResponse
    {
        public List<string> ActionCategories { get; set; } = new();
        public List<string> OrderStatuses { get; set; } = new();
    }

    public class PersonnelActionOrderQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? ActionCategory { get; set; }
        public string? OrderStatus { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Search { get; set; }
    }
}
