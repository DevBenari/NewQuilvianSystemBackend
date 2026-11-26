using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    /// <summary>
    /// Peserta Surat Tugas (BE-HRD-382). Snapshot nama, kode, jabatan, dan unit dibekukan saat disimpan agar naskah
    /// yang terbit tidak berubah bila data induk berubah kemudian.
    /// </summary>
    [Table("HrdAssignmentLetterParticipant", Schema = "public")]
    public class HrdAssignmentLetterParticipant : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AssignmentLetterId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        [MaxLength(100)]
        public string? ParticipantRole { get; set; }

        [Required]
        [MaxLength(200)]
        public string DisplayNameSnapshot { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ProfileCodeSnapshot { get; set; }

        [MaxLength(200)]
        public string? PositionNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? UnitNameSnapshot { get; set; }

        /// <summary>Penugasan dinas luar presensi yang dibuat saat surat diterbitkan.</summary>
        public Guid? OfficialDutyRequestId { get; set; }

        /// <summary>Peringatan benturan jadwal saat terakhir disimpan (tidak memblokir).</summary>
        [MaxLength(500)]
        public string? ScheduleConflictNote { get; set; }

        public bool IsActive { get; set; } = true;

        public HrdAssignmentLetter? AssignmentLetter { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
