using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Models
{
    /// <summary>
    /// Satu perpindahan status atau perubahan catatan membership beserta pelaku, sumber, dan
    /// alasannya (CRM-DEC-013, CRM-DEC-022). Baris tidak pernah diubah maupun dihapus; urutan
    /// memakai <see cref="OccurredAt"/> lalu <see cref="Id"/>, tanpa nomor urut tersimpan.
    /// </summary>
    [Table("PatMembershipStatusHistory", Schema = "public")]
    public class PatMembershipStatusHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientMembershipId { get; set; }

        public MstPatientMembership? PatientMembership { get; set; }

        /// <summary>Kosong pada baris pertama, saat membership didaftarkan.</summary>
        public MembershipStatus? FromStatus { get; set; }

        /// <summary>Sama dengan <see cref="FromStatus"/> untuk perubahan catatan.</summary>
        public MembershipStatus ToStatus { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActionCode { get; set; } = string.Empty;

        public MembershipChangeSource Source { get; set; }

        /// <summary>Kosong bila perubahan dipicu sistem.</summary>
        public Guid? ActorUserId { get; set; }

        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(500)]
        public string? Reason { get; set; }

        /// <summary>Rujukan tagihan atau refund bila ada.</summary>
        public Guid? ReferenceId { get; set; }

        [MaxLength(250)]
        public string? OldNotes { get; set; }

        [MaxLength(250)]
        public string? NewNotes { get; set; }
    }
}
