using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    /// <summary>
    /// Kejadian gangguan mesin absen yang menjadi bukti bersama koreksi presensi (HRD-DEC-202).
    /// Mencatat kejadian tidak pernah membuat baris log mesin.
    /// </summary>
    [Table("HrdAttendanceDeviceIncident", Schema = "public")]
    public class HrdAttendanceDeviceIncident : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AttendanceDeviceId { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid ReportedByUserId { get; set; }

        /// <summary>
        /// Active atau Cancelled.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        public MstAttendanceDevice? AttendanceDevice { get; set; }
    }
}
