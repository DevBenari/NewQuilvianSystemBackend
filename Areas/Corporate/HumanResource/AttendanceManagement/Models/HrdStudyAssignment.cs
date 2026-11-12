using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    /// <summary>
    /// Tugas belajar penuh waktu berdasarkan SK. Selama masanya pegawai tidak dapat dijadwalkan
    /// (blokir B8) dan hari kerjanya berstatus Tugas Belajar (HRD-DEC-201).
    /// </summary>
    [Table("HrdStudyAssignment", Schema = "public")]
    public class HrdStudyAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DecreeNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ProgramName { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        /// <summary>
        /// Active atau Cancelled.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [MaxLength(500)]
        public string? LastChangeReason { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
