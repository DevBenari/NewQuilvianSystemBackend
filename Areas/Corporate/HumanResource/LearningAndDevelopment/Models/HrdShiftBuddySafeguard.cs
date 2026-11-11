using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    /// <summary>
    /// Status pengamanan shift <i>buddy system</i> untuk staf yang sertifikat pelatihan wajibnya kedaluwarsa
    /// (VR-TRN-10 s.d. VR-TRN-12, INT-HR-77, INT-HR-79, BE-HRD-241).
    /// </summary>
    /// <remarks>
    /// Sebelum 10 November 2026 status ini hanya disimpan di memori proses, sehingga penguncian shift mandiri
    /// seorang perawat bisa "lepas" begitu server di-restart. Satu baris aktif per staf.
    /// </remarks>
    [Table("HrdShiftBuddySafeguard", Schema = "public")]
    public class HrdShiftBuddySafeguard : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(200)]
        public string? StaffNameSnapshot { get; set; }

        public bool IsOverdueFlagged { get; set; } = true;

        public int GracePeriodDaysRemaining { get; set; } = 30;

        public bool IsBuddySystemEnforced { get; set; } = true;

        public bool IsRkkRestrictedLevel3 { get; set; }

        public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReleasedAt { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Message { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
