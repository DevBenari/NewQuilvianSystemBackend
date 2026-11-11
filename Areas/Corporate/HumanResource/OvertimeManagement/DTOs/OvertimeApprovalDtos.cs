namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimeApprovalInboxItemDto
    {
        public Guid Id { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string PlanTitle { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public DateOnly PlanDate { get; set; }
        public string CompensationType { get; set; } = "FinancialPayout";
        public string Reason { get; set; } = string.Empty;
        public int StaffCount { get; set; }
        public decimal TotalEstimatedHours { get; set; }
        public Guid RequestEmployeeUserId { get; set; }
        public string RequestEmployeeName { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = "WaitingApproval";
        public DateTime SubmittedAt { get; set; }
    }

    public class ApproveOvertimeCommand
    {
        public string? Notes { get; set; }
    }

    public class RejectOvertimeCommand
    {
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class MedicalDirectorOvertimeEscalationDto
    {
        public Guid RealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public DateOnly WorkDate { get; set; }
        public int ActualMinutes { get; set; }
        public decimal ActualHours { get; set; }
        public decimal Past7DaysTotalHours { get; set; }
        public string EmergencyClinicalReferenceNo { get; set; } = string.Empty;
        public string ClinicalActionSummary { get; set; } = string.Empty;
        public string UrgencyReason { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }

    public class ApproveMedicalDirectorEscalationCommand
    {
        public string DispensatoryReason { get; set; } = string.Empty;
        public int MandatoryRestHoursGranted { get; set; } = 11;
        public string? Notes { get; set; }
    }

    public class OvertimeApprovalResultDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
