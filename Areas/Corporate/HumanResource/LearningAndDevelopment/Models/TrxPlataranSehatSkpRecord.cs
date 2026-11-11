using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    /// <summary>
    /// Tabel Portofolio Perolehan 50 SKP Kemenkes & Plataran Sehat Nakes (INT-HR-80, VR-TRN-22..24)
    /// </summary>
    [Table("trx_plataran_sehat_skp_records", Schema = "public")]
    public class TrxPlataranSehatSkpRecord : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? TrainingCertificateId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActivityName { get; set; } = string.Empty;

        /// <summary>
        /// Kategori SKP: Pembelajaran (Diklat/Seminar), Pelayanan (Tindakan/Visite), Pengabdian (Baksos/Bencana)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SkpCategory { get; set; } = "Pembelajaran";

        [Column(TypeName = "decimal(6,2)")]
        public decimal SkpPoints { get; set; } = 1.00m;

        public DateTime ActivityDate { get; set; }

        [MaxLength(100)]
        public string? OrganizerName { get; set; }

        [MaxLength(100)]
        public string? PlataranSehatCertificateId { get; set; }

        public bool IsVerifiedByPlataranSehat { get; set; } = false;

        public DateTime? PlataranSehatVerifiedAt { get; set; }

        public bool IsSyncedToOppe { get; set; } = false;

        public DateTime? OppeSyncedAt { get; set; }

        [MaxLength(1000)]
        public string? EvidenceDocumentUrl { get; set; }

        public virtual MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
