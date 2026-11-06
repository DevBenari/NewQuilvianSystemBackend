using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimeRealizationResultDto
    {
        public Guid Id { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public string RealizationStatus { get; set; } = string.Empty;
        public string OvertimeCategory { get; set; } = string.Empty;
        public int ActualMinutes { get; set; }
        public decimal ActualHours { get; set; }
        public bool IsMedicalDirectorEscalation { get; set; }
        public string CompensationType { get; set; } = "FinancialPayout";
        public string Message { get; set; } = string.Empty;
    }

    public class OvertimeRealizationDetailDto
    {
        public Guid Id { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string OvertimeCategory { get; set; } = string.Empty;
        public DateOnly WorkDate { get; set; }
        public DateTime? ActualStartAt { get; set; }
        public DateTime? ActualEndAt { get; set; }
        public int ActualMinutes { get; set; }
        public decimal ActualHours { get; set; }
        public int VerifiedMinutes { get; set; }
        public string CompensationType { get; set; } = "FinancialPayout";
        public string? EmergencyClinicalReferenceNo { get; set; }
        public string? ClinicalActionSummary { get; set; }
        public string? RealizationNotes { get; set; }
        public string RealizationStatus { get; set; } = string.Empty;
        public bool IsMedicalDirectorEscalation { get; set; }
        public Guid? MedicalDirectorApprovedById { get; set; }
        public string? MedicalDirectorApproverName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }

    public class OvertimeRealizationSummaryDto
    {
        public Guid Id { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public string OvertimeCategory { get; set; } = string.Empty;
        public DateOnly WorkDate { get; set; }
        public int ActualMinutes { get; set; }
        public decimal ActualHours { get; set; }
        public int VerifiedMinutes { get; set; }
        public string CompensationType { get; set; } = "FinancialPayout";
        public string RealizationStatus { get; set; } = string.Empty;
        public bool IsMedicalDirectorEscalation { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class CancelOvertimeRealizationCommand
    {
        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;
    }
}
