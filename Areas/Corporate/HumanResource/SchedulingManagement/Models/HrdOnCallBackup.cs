using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    /// <summary>
    /// Cadangan berurutan untuk satu penugasan siaga utama (HRD-DEC-205).
    /// Cadangan tidak dikirim ke Laboratorium sebagai siaga aktif (HRD-ASM-78).
    /// </summary>
    [Table("HrdOnCallBackup", Schema = "public")]
    public class HrdOnCallBackup : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnCallAssignmentId { get; set; }

        /// <summary>
        /// Urutan cadangan, dimulai dari 1.
        /// </summary>
        public int BackupSequence { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxOnCallAssignment? OnCallAssignment { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
