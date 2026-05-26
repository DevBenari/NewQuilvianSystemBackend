using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpLeaveRequest", Schema = "public")]
    public class WfpLeaveRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? LeaveBalanceId { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; } = LeaveType.AnnualLeave;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "numeric(6,2)")]
        public decimal TotalDays { get; set; } = 0;

        public bool IsHalfDay { get; set; } = false;

        public bool IsDeductBalance { get; set; } = true;

        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public LeaveApprovalStatus ApprovalStatus { get; set; } = LeaveApprovalStatus.PendingApproval;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(250)]
        public string? ApprovalNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public DateTime? RejectedAt { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(100)]
        public string? AttachmentContentType { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public WfpLeaveBalance? LeaveBalance { get; set; }        
    }
}
