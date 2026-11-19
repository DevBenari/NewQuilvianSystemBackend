using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public class RetirementFilterMetadataResponse
    {
        public List<string> RetirementStatusOptions { get; set; } = new() { "Planned", "Verified", "Approved", "HandoffCompleted", "Cancelled" };
        public List<string> RetirementTypeOptions { get; set; } = new() { "Normal", "Dini", "Sakit/Medis", "Khusus" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
    }

    public class RetirementSummaryResponse
    {
        public int TotalData { get; set; }
        public int Planned { get; set; }
        public int Verified { get; set; }
        public int Approved { get; set; }
        public int HandoffCompleted { get; set; }
        public int Cancelled { get; set; }
        public int ApproachingRetirementCount { get; set; }
    }

    public class RetirementListResponse
    {
        public Guid Id { get; set; }
        public string RetirementNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNip { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public string RetirementType { get; set; } = "Normal";
        public DateTime NormalRetirementDate { get; set; }
        public DateTime? ActualRetirementDate { get; set; }
        public int? RetirementAge { get; set; }
        public string RetirementStatus { get; set; } = "Planned";
        public Guid? EmployeeSeparationId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class RetirementDetailResponse : RetirementListResponse
    {
        public Guid? BenefitPlanId { get; set; }
        public string? BenefitPlanName { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? HireDate { get; set; }
        public int? ServiceYears { get; set; }
        public string? SeparationNumber { get; set; }
    }

    public class EligibleRetirementEmployeeResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNip { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public int CurrentAge { get; set; }
        public DateTime ProjectedRetirementDate { get; set; }
        public int RemainingMonths { get; set; }
    }

    public class CreateRetirementRequest
    {
        [Required] public Guid WorkforceProfileId { get; set; }
        public string RetirementType { get; set; } = "Normal";
        [Required] public DateTime NormalRetirementDate { get; set; }
        public DateTime? ActualRetirementDate { get; set; }
        public int? RetirementAge { get; set; }
        public Guid? BenefitPlanId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateRetirementRequest
    {
        public string? RetirementType { get; set; }
        public DateTime? ActualRetirementDate { get; set; }
        public int? RetirementAge { get; set; }
        public Guid? BenefitPlanId { get; set; }
        public string? Notes { get; set; }
    }

    public class ApproveRetirementRequest
    {
        public string? Notes { get; set; }
    }

    public class RetirementHandoffRequest
    {
        public bool CreateOffboardingChecklist { get; set; } = true;
        public Guid? OffboardingTemplateId { get; set; }
        public string? Notes { get; set; }
    }
}
