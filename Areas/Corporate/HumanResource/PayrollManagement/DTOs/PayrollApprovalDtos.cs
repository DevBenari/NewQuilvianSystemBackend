using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class SubmitPayrollRunCommand
    {
        [MaxLength(2000)]
        public string? Comments { get; set; }
    }

    public class ReviewPayrollRunCommand
    {
        [MaxLength(2000)]
        public string? Comments { get; set; }
    }

    public class ApprovePayrollRunCommand
    {
        [MaxLength(2000)]
        public string? Comments { get; set; }
    }

    public class RejectPayrollRunCommand
    {
        [Required(ErrorMessage = "Alasan penolakan batch wajib diisi.")]
        [MinLength(15, ErrorMessage = "Alasan penolakan batch wajib minimal 15 karakter resmi untuk audit trail (PAY-B7, VR-PAY-16).")]
        [MaxLength(2000, ErrorMessage = "Alasan penolakan maksimal 2000 karakter.")]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class PayrollApprovalResultDto
    {
        public Guid RunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string ApprovalTierLevel { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;
        public Guid ActorUserId { get; set; }
        public string? ActorUserName { get; set; }
        public DateTime ActionTimestamp { get; set; }
        public bool IsLocked { get; set; }
        public bool IsApproved { get; set; }
        public string? Comments { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PayrollApprovalHistoryDto
    {
        public Guid Id { get; set; }
        public Guid PayrollRunId { get; set; }
        public int StepOrder { get; set; }
        public string ApprovalLevel { get; set; } = string.Empty;
        public string ApprovalStatus { get; set; } = string.Empty;
        public string? ActionType { get; set; }
        public DateTime? ActionAt { get; set; }
        public Guid? ActionByUserId { get; set; }
        public string? ActionByUserName { get; set; }
        public string? Comments { get; set; }
    }
}
