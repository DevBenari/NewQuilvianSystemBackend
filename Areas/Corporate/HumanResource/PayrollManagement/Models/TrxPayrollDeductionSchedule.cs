using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Master jadwal cicilan piutang/pinjaman pegawai tersinkronisasi dengan Keuangan (BE-HRD-161, BE-HRD-163, INT-HR-57, HRD-DEC-128).
    /// </summary>
    [Table("TrxPayrollDeductionSchedule", Schema = "public")]
    public class TrxPayrollDeductionSchedule : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string ScheduleNumber { get; set; } = string.Empty;

        [Required]
        public Guid EmployeeId { get; set; }

        [Required, MaxLength(100)]
        public string DeductionCategory { get; set; } = "KasbonPinjamanRS";
        // 'GantiRugiAsetKasir', 'KasbonPinjamanRS', 'CicilanKoperasi', 'IuranPaguyuban'

        public int PriorityLevel { get; set; } = 2;
        // 1: Ganti Rugi Kasir/Aset RS, 2: Kasbon Pinjaman RS, 3: Cicilan Koperasi, 4: Iuran Paguyuban (DED-B2)

        [MaxLength(100)]
        public string? ExternalReferenceId { get; set; } // ID Piutang di modul Keuangan (Finance AR Code)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalLoanPrincipal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyInstallmentAmount { get; set; }

        public int TotalInstallmentMonths { get; set; } = 1;

        public int PaidInstallmentCount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingPrincipal { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required, MaxLength(50)]
        public string ScheduleStatus { get; set; } = "Active";
        // 'Active', 'Paused', 'Completed', 'Cancelled', 'ClearedAtSeparation'

        public bool AutoCarryOverOnCapExceeded { get; set; } = true;

        public MstEmployee? Employee { get; set; }
        public ICollection<TrxPayrollDeductionItem> DeductionItems { get; set; } = new List<TrxPayrollDeductionItem>();
    }
}
