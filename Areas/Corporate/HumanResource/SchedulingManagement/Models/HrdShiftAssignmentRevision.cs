using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    /// <summary>
    /// Riwayat Revisi roster terbit: satu baris per perubahan (Ganti Petugas, koreksi mundur,
    /// tambah shift, hapus shift). Hanya-tambah; tidak pernah diubah atau dihapus (HRD-DEC-195, HRD-DEC-217).
    /// </summary>
    [Table("HrdShiftAssignmentRevision", Schema = "public")]
    public class HrdShiftAssignmentRevision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ShiftAssignmentId { get; set; }

        [Required]
        public Guid RosterPeriodId { get; set; }

        /// <summary>
        /// Tanggal shift; shift malam memakai tanggal masuk (HRD-DEC-104).
        /// </summary>
        public DateOnly ShiftDate { get; set; }

        /// <summary>
        /// Replacement, RetroactiveCorrection, AddShift, atau RemoveShift.
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string ChangeType { get; set; } = string.Empty;

        /// <summary>
        /// Petugas tepat sebelum perubahan; kosong untuk AddShift.
        /// </summary>
        public Guid? PreviousWorkforceProfileId { get; set; }

        /// <summary>
        /// Petugas sesudah perubahan; kosong untuk RemoveShift.
        /// </summary>
        public Guid? NewWorkforceProfileId { get; set; }

        public Guid? PreviousShiftId { get; set; }
        public Guid? NewShiftId { get; set; }

        public DateTime? PreviousStartAt { get; set; }
        public DateTime? PreviousEndAt { get; set; }
        public DateTime? NewStartAt { get; set; }
        public DateTime? NewEndAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReasonCategory { get; set; } = string.Empty;

        /// <summary>
        /// Catatan alasan. SENSITIF: bisa memuat kondisi kesehatan; tidak boleh masuk payload log.
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string ReasonNote { get; set; } = string.Empty;

        public Guid? ShiftReplacementId { get; set; }

        [Required]
        public Guid ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public TrxShiftAssignment? ShiftAssignment { get; set; }
        public TrxRosterPeriod? RosterPeriod { get; set; }
        public MstWorkforceProfile? PreviousWorkforceProfile { get; set; }
        public MstWorkforceProfile? NewWorkforceProfile { get; set; }
        public MstShift? PreviousShift { get; set; }
        public MstShift? NewShift { get; set; }
        public TrxShiftReplacement? ShiftReplacement { get; set; }
    }
}
