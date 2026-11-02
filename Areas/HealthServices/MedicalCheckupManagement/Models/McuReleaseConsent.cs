using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Persetujuan peserta agar hasil lengkapnya boleh diserahkan ke perusahaan. Satu persetujuan
    /// berlaku per order (<see cref="RevokedAt"/> kosong) — dijaga unique index.
    /// </summary>
    [Table("McuReleaseConsent", Schema = "public")]
    public class McuReleaseConsent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid McuOrderId { get; set; }

        public McuOrder? McuOrder { get; set; }

        /// <summary>Wajib sama dengan perusahaan order (dijaga service).</summary>
        [Required]
        public Guid CompanyGuarantorId { get; set; }

        public MstCompanyGuarantor? CompanyGuarantor { get; set; }

        public DateTime ConsentGivenAt { get; set; }

        [Required]
        public Guid RecordedByUserId { get; set; }

        public McuConsentEvidenceType EvidenceType { get; set; }

        /// <summary>Nomor formulir kertas.</summary>
        [MaxLength(200)]
        public string? EvidenceReference { get; set; }

        public DateTime? RevokedAt { get; set; }

        public Guid? RevokedByUserId { get; set; }

        [MaxLength(500)]
        public string? RevokeReason { get; set; }
    }
}
