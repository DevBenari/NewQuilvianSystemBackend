using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models
{
    [Table("MstLeaveType", Schema = "public")]
    public class MstLeaveType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string LeaveTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string LeaveTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LeaveCategory { get; set; } = "Annual";
        // Annual, Sick, Maternity, Paternity, Marriage, Bereavement,
        // Unpaid, Special, Compensatory, Other.

        public bool IsPaidLeave { get; set; } = true;

        public bool IsBalanceDeducted { get; set; } = true;

        public bool AllowHalfDay { get; set; } = false;

        public bool AllowHourly { get; set; } = false;

        public bool RequiresAttachment { get; set; } = false;

        public bool RequiresMedicalCertificate { get; set; } = false;

        public int? AttachmentRequiredAfterDays { get; set; }

        public int DefaultMinimumNoticeDays { get; set; } = 0;

        public int? DefaultMaximumConsecutiveDays { get; set; }

        [MaxLength(20)]
        public string? ColorCode { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<MstLeavePolicy> LeavePolicies { get; set; }
            = new List<MstLeavePolicy>();
    }
}
