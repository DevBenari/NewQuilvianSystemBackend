using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class EvaluateVarianceResultDto
    {
        public Guid RunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int TotalEmployeesChecked { get; set; }
        public int TotalAlertsGenerated { get; set; }
        public int TotalCriticalAlerts { get; set; }
        public int TotalHighAlerts { get; set; }
        public int TotalWarningAlerts { get; set; }
        public int TotalPendingReviewAlerts { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<PayrollVarianceAlertDto> Alerts { get; set; } = new();
    }

    public class PayrollVarianceAlertDto
    {
        public Guid Id { get; set; }
        public Guid PayrollRunId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public decimal PreviousNetPay { get; set; }
        public decimal CurrentNetPay { get; set; }
        public decimal VarianceAmount { get; set; }
        public decimal VariancePercentage { get; set; }
        public string AlertSeverity { get; set; } = "Warning";
        public string AlertReason { get; set; } = string.Empty;
        public string ResolutionStatus { get; set; } = "PendingReview";
        public string? JustificationNotes { get; set; }
        public Guid? ResolvedByUserId { get; set; }
        public string? ResolvedByUserName { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class ResolveVarianceAlertCommand
    {
        [Required(ErrorMessage = "Catatan alasan justifikasi audit wajib diisi.")]
        [MinLength(20, ErrorMessage = "Alasan justifikasi variansi wajib minimal 20 karakter untuk audit compliance (VR-PAY-15).")]
        [MaxLength(2000, ErrorMessage = "Alasan justifikasi maksimal 2000 karakter.")]
        public string JustificationNotes { get; set; } = string.Empty;
    }

    public class BatchVarianceSubmissionCheckDto
    {
        public Guid RunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public bool CanBeSubmitted { get; set; }
        public int PendingAlertsCount { get; set; }
        public List<string> PendingAlertSummaries { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
