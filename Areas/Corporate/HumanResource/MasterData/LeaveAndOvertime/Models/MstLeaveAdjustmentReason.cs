using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models
{
    [Table("MstLeaveAdjustmentReason", Schema = "public")]
    public class MstLeaveAdjustmentReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? LeaveTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReasonCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ReasonName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ReasonCategory { get; set; }
            = LeaveValueConstants.AdjustmentReasonCategory.ManualAdjustment;

        [Required]
        [MaxLength(20)]
        public string AllowedDirection { get; set; }
            = LeaveValueConstants.AdjustmentAllowedDirection.Both;

        public bool AllowOpeningBalance { get; set; } = false;

        public bool AllowManualAdjustment { get; set; } = true;

        public bool AllowCorrection { get; set; } = true;

        public bool AllowReversal { get; set; } = true;

        public decimal? MaximumAdjustmentDays { get; set; }

        public bool RequiresComment { get; set; } = true;

        public bool RequiresAttachment { get; set; } = false;

        public bool RequiresApproval { get; set; } = true;

        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }

        public int SortOrder { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLeaveType? LeaveType { get; set; }
    }
}
