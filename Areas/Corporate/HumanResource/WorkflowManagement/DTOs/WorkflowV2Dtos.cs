using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs
{
    // =========================================================================
    // DTOs untuk Tag 1: Corporate - HRD Workflow Inbox & Instances
    // =========================================================================

    public class ApprovalInboxItemV2ResponseDto
    {
        public Guid WorkflowInstanceId { get; set; }
        public Guid StepInstanceId { get; set; }
        public string WorkflowNumber { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public Guid ApplicantEmployeeId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public DateTime? SlaDeadline { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsDelegated { get; set; }
        public string? DelegatedFromEmployeeName { get; set; }
        public string PriorityLevel { get; set; } = "Normal";
        public decimal? Amount { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public string RiskIndicator { get; set; } = "Green"; // Green, Yellow, Red
        public int CurrentStepNumber { get; set; }
        public int TotalSteps { get; set; }
    }

    public class ApprovalInboxSummaryV2ResponseDto
    {
        public int TotalPendingCount { get; set; }
        public int UrgentCount { get; set; }
        public int DelegatedCount { get; set; }
        public int OverdueCount { get; set; }
        public Dictionary<string, int> ByCategoryCounts { get; set; } = new();
    }

    public class CreateWorkflowInstanceRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ReferenceType { get; set; } = string.Empty;

        [Required]
        public Guid ReferenceId { get; set; }

        public Guid? ApplicantEmployeeId { get; set; }

        public Guid? DepartmentId { get; set; }

        public decimal? Amount { get; set; }

        [MaxLength(30)]
        public string PriorityLevel { get; set; } = "Normal";

        [MaxLength(500)]
        public string? SummaryText { get; set; }

        public string? DetailsJson { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class CancelWorkflowRequestDto
    {
        [Required]
        [MinLength(10, ErrorMessage = "Alasan pembatalan minimal 10 karakter")]
        [MaxLength(1000)]
        public string CancellationReason { get; set; } = string.Empty;
    }

    public class WorkflowInstanceV2ResponseDto
    {
        public Guid Id { get; set; }
        public string WorkflowNumber { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
        public Guid ReferenceId { get; set; }
        public Guid? ApplicantEmployeeId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string WorkflowStatus { get; set; } = "Pending";
        public string PriorityLevel { get; set; } = "Normal";
        public decimal? Amount { get; set; }
        public int CurrentStepOrder { get; set; }
        public int TotalSteps { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? SlaDeadline { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
    }

    public class WorkflowInstanceDetailV2ResponseDto : WorkflowInstanceV2ResponseDto
    {
        public string? RequestContextJson { get; set; }
        public List<WorkflowStepInstanceV2ResponseDto> Steps { get; set; } = new();
        public List<WorkflowRiskIndicatorDto> RiskIndicators { get; set; } = new();
        public List<WorkflowCommentV2ResponseDto> Comments { get; set; } = new();
        public List<WorkflowAttachmentV2ResponseDto> Attachments { get; set; } = new();
    }

    public class WorkflowStepInstanceV2ResponseDto
    {
        public Guid StepInstanceId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepStatus { get; set; } = "Pending";
        public bool IsCurrentStep { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<WorkflowApproverAssignmentV2Dto> Approvers { get; set; } = new();
        public List<WorkflowActionRecordV2Dto> Actions { get; set; } = new();
    }

    public class WorkflowApproverAssignmentV2Dto
    {
        public Guid AssignmentId { get; set; }
        public Guid ApproverUserId { get; set; }
        public Guid? ApproverEmployeeId { get; set; }
        public string ApproverName { get; set; } = string.Empty;
        public string AssignmentStatus { get; set; } = "Pending";
        public bool IsDelegated { get; set; }
        public string? DelegatedToName { get; set; }
    }

    public class WorkflowActionRecordV2Dto
    {
        public Guid ActionId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string ActorName { get; set; } = string.Empty;
        public string? OnBehalfOfName { get; set; }
        public string? Comment { get; set; }
        public DateTime ActionTimestamp { get; set; }
        public string? DigitalSignatureHash { get; set; }
    }

    public class WorkflowRiskIndicatorDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Green"; // Green, Yellow, Red
        public string Message { get; set; } = string.Empty;
    }

    // =========================================================================
    // DTOs untuk Tag 2: Corporate - HRD Workflow Approval Actions
    // =========================================================================

    public class ApproveActionRequestDto
    {
        [MaxLength(1000)]
        public string? Note { get; set; }

        [Required(ErrorMessage = "PIN otorisasi atau digital signature wajib disertakan")]
        [MinLength(6, ErrorMessage = "PIN otorisasi minimal 6 digit")]
        public string PinSignature { get; set; } = string.Empty;
    }

    public class RejectActionRequestDto
    {
        [Required(ErrorMessage = "Alasan penolakan wajib diisi (VR-WF-05)")]
        [MinLength(15, ErrorMessage = "Alasan penolakan wajib minimal 15 karakter")]
        [MaxLength(2000)]
        public string RejectReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "PIN otorisasi wajib disertakan")]
        public string PinSignature { get; set; } = string.Empty;
    }

    public class ReturnForRevisionRequestDto
    {
        [Required(ErrorMessage = "Catatan revisi wajib diisi (HRD-DEC-186)")]
        [MinLength(10, ErrorMessage = "Catatan revisi minimal 10 karakter")]
        [MaxLength(2000)]
        public string RevisionNotes { get; set; } = string.Empty;
    }

    public class BulkApproveRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Minimal pilih 1 tiket untuk persetujuan massal")]
        public List<Guid> StepInstanceIds { get; set; } = new();

        [MaxLength(500)]
        public string? BatchNote { get; set; }

        [Required(ErrorMessage = "PIN otorisasi wajib disertakan")]
        [MinLength(6, ErrorMessage = "PIN otorisasi minimal 6 digit")]
        public string PinSignature { get; set; } = string.Empty;
    }

    public class BulkApproveResultDto
    {
        public int TotalRequested { get; set; }
        public int TotalApproved { get; set; }
        public int TotalSkipped { get; set; }
        public List<Guid> ApprovedStepIds { get; set; } = new();
        public List<BulkApproveSkippedItemDto> SkippedItems { get; set; } = new();
    }

    public class BulkApproveSkippedItemDto
    {
        public Guid StepInstanceId { get; set; }
        public string WorkflowNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class ResubmitWorkflowRequestDto
    {
        public string? UpdatedDataJson { get; set; }

        [Required(ErrorMessage = "Komentar pengajuan perbaikan wajib diisi")]
        [MaxLength(1000)]
        public string ResubmitComment { get; set; } = string.Empty;
    }

    public class WorkflowActionV2ResponseDto
    {
        public bool Success { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid WorkflowInstanceId { get; set; }
        public string ResultingWorkflowStatus { get; set; } = string.Empty;
        public string ResultingStepStatus { get; set; } = string.Empty;
        public DateTime ActionTimestamp { get; set; }
    }

    // =========================================================================
    // DTOs untuk Tag 3: Corporate - HRD Workflow Delegations
    // =========================================================================

    public class CreateApprovalDelegationRequestDto
    {
        [Required(ErrorMessage = "Pejabat pengganti (Plh) wajib dipilih")]
        public Guid DelegateEmployeeId { get; set; }

        [Required(ErrorMessage = "Tanggal mulai wewenang Plh wajib diisi")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Tanggal selesai wewenang Plh wajib diisi")]
        public DateTime EndDate { get; set; }

        [MaxLength(500)]
        public string DelegatedRequestTypes { get; set; } = "ALL";

        [Required(ErrorMessage = "Alasan penunjukan Plh wajib diisi")]
        [MinLength(10, ErrorMessage = "Alasan pendelegasian minimal 10 karakter")]
        [MaxLength(1000)]
        public string DelegationReason { get; set; } = string.Empty;
    }

    public class RevokeDelegationRequestDto
    {
        [Required(ErrorMessage = "Alasan pencabutan wewenang Plh wajib diisi")]
        [MinLength(10, ErrorMessage = "Alasan pencabutan minimal 10 karakter")]
        [MaxLength(1000)]
        public string RevokeReason { get; set; } = string.Empty;
    }

    public class ApprovalDelegationV2ResponseDto
    {
        public Guid Id { get; set; }
        public string DelegationNumber { get; set; } = string.Empty;
        public Guid PrincipalEmployeeId { get; set; }
        public string PrincipalName { get; set; } = string.Empty;
        public string PrincipalPosition { get; set; } = string.Empty;
        public Guid DelegateEmployeeId { get; set; }
        public string DelegateName { get; set; } = string.Empty;
        public string DelegatePosition { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DelegatedRequestTypes { get; set; } = "ALL";
        public string DelegationReason { get; set; } = string.Empty;
        public string DelegationStatus { get; set; } = "Active";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevocationReason { get; set; }
    }

    // =========================================================================
    // DTOs untuk Tag 4: Corporate - HRD Workflow Comments & Attachments
    // =========================================================================

    public class AddWorkflowCommentRequestDto
    {
        [Required(ErrorMessage = "Teks komentar klarifikasi wajib diisi")]
        [MaxLength(2000)]
        public string CommentText { get; set; } = string.Empty;

        public bool IsInternalOnly { get; set; } = false;
    }

    public class WorkflowCommentV2ResponseDto
    {
        public Guid Id { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public Guid AuthorEmployeeId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public bool IsInternalReviewOnly { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WorkflowAttachmentV2ResponseDto
    {
        public Guid Id { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileStoragePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
    }

    // =========================================================================
    // DTOs untuk Tag 5: Corporate - HRD Workflow SLA & Matrix Management
    // =========================================================================

    public class WorkflowMatrixStepConfigDto
    {
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string ApproverRole { get; set; } = string.Empty; // e.g. "DirectSupervisor", "HeadOfDepartment", "HRDirector", "FinanceDirector"
        public int SlaHours { get; set; } = 48;
        public bool IsRequired { get; set; } = true;
    }

    public class ConfigureWorkflowMatrixRequestDto
    {
        public Guid? DepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RequestType { get; set; } = string.Empty;

        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Minimal harus memiliki 1 tahapan step")]
        public List<WorkflowMatrixStepConfigDto> Steps { get; set; } = new();
    }

    public class WorkflowMatrixResponseDto
    {
        public Guid Id { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "Seluruh Unit / Global";
        public string RequestType { get; set; } = string.Empty;
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public List<WorkflowMatrixStepConfigDto> Steps { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class OverdueWorkflowItemDto
    {
        public Guid WorkflowInstanceId { get; set; }
        public string WorkflowNumber { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public DateTime SlaDeadline { get; set; }
        public double OverdueHours { get; set; }
        public string CurrentStepName { get; set; } = string.Empty;
        public string CurrentApproverName { get; set; } = string.Empty;
    }

    public class ManualEscalationRequestDto
    {
        [Required(ErrorMessage = "Pejabat atasan target eskalasi wajib dipilih")]
        public Guid TargetApproverEmployeeId { get; set; }

        [Required(ErrorMessage = "Alasan intervensi eskalasi wajib diisi")]
        [MinLength(10, ErrorMessage = "Alasan eskalasi minimal 10 karakter")]
        [MaxLength(1000)]
        public string EscalationReason { get; set; } = string.Empty;
    }

    public class SlaComplianceReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalRequestsProcessed { get; set; }
        public int CompletedWithinSla { get; set; }
        public int CompletedOverdue { get; set; }
        public double CompliancePercentage { get; set; }
        public double AverageResolutionHours { get; set; }
        public List<DepartmentSlaSummaryDto> DepartmentSummaries { get; set; } = new();
    }

    public class DepartmentSlaSummaryDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int WithinSlaCount { get; set; }
        public int OverdueCount { get; set; }
        public double ComplianceRate { get; set; }
    }
}
