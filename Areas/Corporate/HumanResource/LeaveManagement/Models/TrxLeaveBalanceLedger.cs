using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    /// <summary>
    /// Buku besar mutasi saldo cuti pegawai rumah sakit secara atomik dan audit-tracked (BE-HRD-117, BE-HRD-118, HRD-DEC-110).
    /// </summary>
    [Table("TrxLeaveBalanceLedger", Schema = "public")]
    public class TrxLeaveBalanceLedger : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        [Required]
        public int PeriodYear { get; set; }

        /// <summary>
        /// Jenis mutasi transaksi: Entitlement, CarryForward, Taken, CancellationRestore, RecallRestore, Forfeiture, Adjustment
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TransactionType { get; set; } = string.Empty;

        /// <summary>
        /// Nilai mutasi hari: positif (+) untuk penambahan, negatif (-) untuk pengurangan/debit.
        /// </summary>
        [Column(TypeName = "decimal(4,1)")]
        public decimal AmountDays { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal BalanceBefore { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal BalanceAfter { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public Guid? ReferenceRequestId { get; set; }

        public Guid? ReferenceRecallId { get; set; }

        public Guid? ReferenceAdjustmentId { get; set; }

        /// <summary>
        /// Tanggal batas kedaluwarsa saldo (khusus CarryForward: 30 Juni tahun berjalan per HRD-DEC-110).
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Flag penanda saldo telah hangus akibat kedaluwarsa.
        /// </summary>
        public bool IsForfeited { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(WorkforceProfileId))]
        public MstWorkforceProfile? WorkforceProfile { get; set; }

        [ForeignKey(nameof(LeaveTypeId))]
        public MstLeaveType? LeaveType { get; set; }

        [ForeignKey(nameof(ReferenceRequestId))]
        public WfpLeaveRequest? ReferenceRequest { get; set; }

        [ForeignKey(nameof(ReferenceRecallId))]
        public TrxLeaveRecallRequest? ReferenceRecall { get; set; }

        [ForeignKey(nameof(ReferenceAdjustmentId))]
        public TrxLeaveAdjustment? ReferenceAdjustment { get; set; }
    }
}
