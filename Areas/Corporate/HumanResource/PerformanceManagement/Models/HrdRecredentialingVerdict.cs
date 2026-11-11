using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Putusan sah sidang pleno rekredensialing: RenewAsIs, Upgrade, Restricted, atau Revoked (VR-OPP-20, BE-HRD-235).
    /// </summary>
    /// <remarks>
    /// Sebelum 10 November 2026 putusan hanya disimpan di memori proses dan hilang saat aplikasi di-restart,
    /// sehingga pengiriman ke SIMRS tidak bisa ditelusuri ke putusan asalnya.
    /// </remarks>
    [Table("HrdRecredentialingVerdict", Schema = "public")]
    public class HrdRecredentialingVerdict : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(50)]
        public string? DoctorCodeSnapshot { get; set; }

        [MaxLength(200)]
        public string? DoctorNameSnapshot { get; set; }

        [Required]
        [MaxLength(30)]
        public string Verdict { get; set; } = "RenewAsIs";

        [MaxLength(2000)]
        public string? CommitteeNotes { get; set; }

        [Required]
        public Guid ApprovedByCommitteeChairmanId { get; set; }

        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Stempel digital SHA-256 atas isi putusan.</summary>
        [Required]
        [MaxLength(64)]
        public string SignatureHash { get; set; } = string.Empty;

        public ICollection<HrdSimrsPrivilegeSyncLog> SyncLogs { get; set; } = new List<HrdSimrsPrivilegeSyncLog>();
    }
}
