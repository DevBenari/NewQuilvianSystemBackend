using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollPayslip", Schema = "public")]
    public class TrxPayrollPayslip : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunId { get; set; }

        [Required]
        public Guid PayrollRunEmployeeId { get; set; }

        [Required, MaxLength(50)]
        public string PayslipNumber { get; set; } = string.Empty;

        public int VersionNumber { get; set; } = 1;

        [Required, MaxLength(30)]
        public string PayslipStatus { get; set; } = "Draft";
        // Draft, Generated, Published, Downloaded, Superseded, Cancelled.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public DateTime PeriodStartDateSnapshot { get; set; }
        public DateTime PeriodEndDateSnapshot { get; set; }

        [MaxLength(50)]
        public string? EmployeeNumberSnapshot { get; set; }

        [Required, MaxLength(200)]
        public string EmployeeNameSnapshot { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? DepartmentNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? PositionNameSnapshot { get; set; }

        public decimal BaseSalary { get; set; } = 0m;
        public decimal TotalEarning { get; set; } = 0m;
        public decimal TotalDeduction { get; set; } = 0m;
        public decimal TotalTax { get; set; } = 0m;
        public decimal GrossPay { get; set; } = 0m;
        public decimal NetPay { get; set; } = 0m;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }

        [MaxLength(100)]
        public string? ContentType { get; set; }

        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        public string? PayslipSnapshotJson { get; set; }

        public DateTime? GeneratedAt { get; set; }
        public Guid? GeneratedByUserId { get; set; }
        public DateTime? PublishedAt { get; set; }
        public Guid? PublishedByUserId { get; set; }
        public DateTime? FirstDownloadedAt { get; set; }
        public DateTime? LastDownloadedAt { get; set; }
        public int DownloadCount { get; set; } = 0;

        public bool IsEmployeeVisible { get; set; } = false;
        public bool IsSnapshotFrozen { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public TrxPayrollRun? PayrollRun { get; set; }
        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public ApplicationUser? GeneratedByUser { get; set; }
        public ApplicationUser? PublishedByUser { get; set; }
    }
}
