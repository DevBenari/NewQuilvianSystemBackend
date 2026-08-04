using QuilvianSystemBackend.Enums.HumanResource;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs
{
    public class WfpComplianceAlertLogResponse
    {
        public Guid Id { get; set; }
        public Guid ComplianceAlertId { get; set; }
        public ComplianceAlertLogType LogType { get; set; }
        public string LogTypeName { get; set; } = string.Empty;
        public ComplianceAlertStatus? OldStatus { get; set; }
        public string? OldStatusName { get; set; }
        public ComplianceAlertStatus? NewStatus { get; set; }
        public string? NewStatusName { get; set; }
        public string LogMessage { get; set; } = string.Empty;
        public Guid? PerformedByUserId { get; set; }
        public string? PerformedByUserName { get; set; }
        public DateTime PerformedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WfpComplianceAlertLogDetailResponse : WfpComplianceAlertLogResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class CreateWfpComplianceAlertLogRequest
    {
        [Required]
        public ComplianceAlertLogType LogType { get; set; }

        [Required]
        [MaxLength(2000)]
        public string LogMessage { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
