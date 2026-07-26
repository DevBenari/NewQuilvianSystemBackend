using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollOvertimeInput", Schema = "public")]
    public class TrxPayrollOvertimeInput : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunEmployeeId { get; set; }

        public Guid? OvertimeRealizationId { get; set; }
        public Guid? OvertimeRequestId { get; set; }

        public DateOnly OvertimeDate { get; set; }

        [Required, MaxLength(30)]
        public string OvertimeStatusSnapshot { get; set; } = "Verified";

        public int RequestedMinutes { get; set; } = 0;
        public int ApprovedMinutes { get; set; } = 0;
        public int ActualMinutes { get; set; } = 0;
        public int EligibleMinutes { get; set; } = 0;
        public int VerifiedMinutes { get; set; } = 0;

        public decimal RateMultiplier { get; set; } = 1m;
        public decimal HourlyRate { get; set; } = 0m;
        public decimal OvertimeAmount { get; set; } = 0m;

        public string? CalculationSnapshotJson { get; set; }

        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
        public Guid? ImportedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public TrxOvertimeRealization? OvertimeRealization { get; set; }
        public WfpOvertimeRequest? OvertimeRequest { get; set; }
        public ApplicationUser? ImportedByUser { get; set; }
    }
}
