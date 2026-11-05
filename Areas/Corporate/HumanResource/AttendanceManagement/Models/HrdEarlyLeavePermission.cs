using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    /// <summary>
    /// Permohonan izin pulang cepat administratif non-deductible (BE-HRD-104, BE-HRD-108, HRD-DEC-107, menutup HRD-Q-47).
    /// </summary>
    [Table("HrdEarlyLeavePermission", Schema = "public")]
    public class HrdEarlyLeavePermission : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AttendanceDailyId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public DateTime RequestedEarlyLeaveAt { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public Guid? ApprovedByWorkforceId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Penanda bahwa izin pulang cepat tidak memotong kuota cuti tahunan (Non-Deductible per HRD-DEC-107).
        /// </summary>
        public bool IsDeductible { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(AttendanceDailyId))]
        public HrdAttendanceDaily? AttendanceDaily { get; set; }

        [ForeignKey(nameof(WorkforceProfileId))]
        public MstWorkforceProfile? WorkforceProfile { get; set; }

        [ForeignKey(nameof(ApprovedByWorkforceId))]
        public MstWorkforceProfile? ApprovedByWorkforce { get; set; }
    }
}
