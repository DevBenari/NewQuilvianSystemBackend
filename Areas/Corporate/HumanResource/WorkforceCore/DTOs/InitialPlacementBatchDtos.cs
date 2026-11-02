using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class CreateInitialBatchRequest
    {
        public string BatchName { get; set; } = "Initial Placement Baseline Batch 55 Employees";
        public List<CreateInitialBatchRowDto> Rows { get; set; } = new();
    }

    public class CreateInitialBatchRowDto
    {
        public Guid WorkforceProfileId { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid PositionId { get; set; }
        public Guid? ManagerWorkforceProfileId { get; set; }
        public Guid? SalaryGradeId { get; set; }
        public decimal? BaseSalary { get; set; }
        public DateTime EffectiveStartDate { get; set; }
    }

    public class InitialBatchListResponse
    {
        public Guid Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string BatchName { get; set; } = string.Empty;
        public string BatchStatus { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int ValidatedRows { get; set; }
        public int FailedRows { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class InitialBatchDetailResponse
    {
        public Guid Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string BatchName { get; set; } = string.Empty;
        public string BatchStatus { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int ValidatedRows { get; set; }
        public int FailedRows { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ExecutionSummary { get; set; }

        public List<InitialBatchRowResponse> Rows { get; set; } = new();
    }

    public class InitialBatchRowResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? ManagerWorkforceProfileId { get; set; }
        public string? ManagerName { get; set; }
        public decimal? BaseSalary { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class BatchValidationResultResponse
    {
        public bool IsAllValid { get; set; }
        public int TotalCount { get; set; }
        public int ValidCount { get; set; }
        public int InvalidCount { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }
}
