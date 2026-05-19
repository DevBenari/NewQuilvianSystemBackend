using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpTransportAllowanceTransaction", Schema = "public")]
    public class WfpTransportAllowanceTransaction : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? TransportAllowanceId { get; set; }

        public Guid? TransportAllowancePolicyId { get; set; }

        public Guid? AttendanceId { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string PeriodYearMonth { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AllowanceType { get; set; } = "Regular";

        public decimal Amount { get; set; }

        public bool IsGeneratedFromAttendance { get; set; } = false;

        public bool IsNightShift { get; set; } = false;

        [Required]
        [MaxLength(50)]
        public string TransactionStatus { get; set; } = "Draft";

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public WfpTransportAllowance? TransportAllowance { get; set; }

        public WfpTransportAllowancePolicy? TransportAllowancePolicy { get; set; }

        public EmpAttendance? Attendance { get; set; }
    }
}