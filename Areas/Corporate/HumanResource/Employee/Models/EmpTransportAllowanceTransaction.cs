using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Employee.Models
{
    [Table("EmpTransportAllowanceTransaction", Schema = "public")]
    public class EmpTransportAllowanceTransaction : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        public Guid? TransportAllowanceProfileId { get; set; }

        public Guid? TransportAllowancePolicyId { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string PeriodYearMonth { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AllowanceType { get; set; } = "Daily";

        public decimal Amount { get; set; }

        public bool IsGeneratedFromAttendance { get; set; } = false;

        public bool IsNightShift { get; set; } = false;

        public Guid? AttendanceId { get; set; }

        public Guid? ShiftId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TransactionStatus { get; set; } = "Draft";

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstEmployee? Employee { get; set; }

        public EmpTransportAllowanceProfile? TransportAllowanceProfile { get; set; }

        public EmpTransportAllowancePolicy? TransportAllowancePolicy { get; set; }
    }
}
