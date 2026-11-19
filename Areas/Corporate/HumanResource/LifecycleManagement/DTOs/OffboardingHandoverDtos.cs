using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public class HandoverSummaryResponse
    {
        public int TotalData { get; set; }
        public int Active { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int PendingVerification { get; set; }
    }

    public class HandoverChecklistListResponse
    {
        public Guid Id { get; set; }
        public string ChecklistNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNip { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string OffboardingType { get; set; } = string.Empty;
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string ChecklistStatus { get; set; } = string.Empty;
        public int TotalTask { get; set; }
        public int RequiredTask { get; set; }
        public int CompletedTask { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string? ManagerName { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class HandoverChecklistDetailResponse : HandoverChecklistListResponse
    {
        public Guid? EmployeeSeparationId { get; set; }
        public string? SeparationNumber { get; set; }
        public Guid? ManagerWorkforceProfileId { get; set; }
        public string? Notes { get; set; }
        public List<HandoverTaskResponse> Tasks { get; set; } = new();
    }

    public class HandoverTaskResponse
    {
        public Guid Id { get; set; }
        public Guid OffboardingChecklistId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public string TaskCategory { get; set; } = "General";
        public string? Description { get; set; }
        public Guid? AssignedToWorkforceProfileId { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CompletedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedByName { get; set; }
        public string TaskStatus { get; set; } = "Pending";
        public bool IsRequired { get; set; }
        public bool RequiresDocument { get; set; }
        public string? DocumentPath { get; set; }
        public string? OriginalFileName { get; set; }
        public int SortOrder { get; set; }
        public string? CompletionNote { get; set; }
        public string? VerificationNote { get; set; }
    }

    public class AddHandoverTaskRequest
    {
        [Required, MaxLength(250)] public string TaskName { get; set; } = string.Empty;
        [MaxLength(50)] public string TaskCategory { get; set; } = "WorkUnitHandover";
        [MaxLength(1000)] public string? Description { get; set; }
        public Guid? AssignedToWorkforceProfileId { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool RequiresDocument { get; set; } = false;
        public int SortOrder { get; set; } = 1;
    }

    public class SubmitHandoverTaskRequest
    {
        [MaxLength(1000)] public string? CompletionNote { get; set; }
        [MaxLength(500)] public string? DocumentPath { get; set; }
        [MaxLength(250)] public string? OriginalFileName { get; set; }
    }

    public class VerifyHandoverTaskRequest
    {
        public bool IsApproved { get; set; } = true;
        [MaxLength(1000)] public string? VerificationNote { get; set; }
    }
}
