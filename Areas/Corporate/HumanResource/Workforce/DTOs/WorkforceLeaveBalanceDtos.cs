using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceLeaveBalanceResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int LeaveYear { get; set; }

        public LeaveType LeaveType { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal EntitledDays { get; set; }

        public decimal UsedDays { get; set; }

        public decimal PendingDays { get; set; }

        public decimal RemainingDays { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceLeaveBalanceListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public decimal TotalEntitledDays { get; set; }

        public decimal TotalUsedDays { get; set; }

        public decimal TotalPendingDays { get; set; }

        public decimal TotalRemainingDays { get; set; }

        public List<WorkforceLeaveBalanceResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceLeaveBalanceRequest
    {
        [Required]
        public int LeaveYear { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; } = LeaveType.AnnualLeave;

        public decimal OpeningBalance { get; set; } = 0;

        public decimal EntitledDays { get; set; } = 0;

        public decimal UsedDays { get; set; } = 0;

        public decimal PendingDays { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceLeaveBalanceRequest
    {
        [Required]
        public int LeaveYear { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; } = LeaveType.AnnualLeave;

        public decimal OpeningBalance { get; set; } = 0;

        public decimal EntitledDays { get; set; } = 0;

        public decimal UsedDays { get; set; } = 0;

        public decimal PendingDays { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceLeaveBalanceStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
