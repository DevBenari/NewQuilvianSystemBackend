using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpLeaveBalance", Schema = "public")]
    public class WfpLeaveBalance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public int LeaveYear { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; } = LeaveType.AnnualLeave;

        [Column(TypeName = "numeric(6,2)")]
        public decimal OpeningBalance { get; set; } = 0;

        [Column(TypeName = "numeric(6,2)")]
        public decimal EntitledDays { get; set; } = 0;

        [Column(TypeName = "numeric(6,2)")]
        public decimal UsedDays { get; set; } = 0;

        [Column(TypeName = "numeric(6,2)")]
        public decimal PendingDays { get; set; } = 0;

        [Column(TypeName = "numeric(6,2)")]
        public decimal RemainingDays { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public ICollection<WfpLeaveRequest> LeaveRequests { get; set; }
            = new List<WfpLeaveRequest>();
    }
}
