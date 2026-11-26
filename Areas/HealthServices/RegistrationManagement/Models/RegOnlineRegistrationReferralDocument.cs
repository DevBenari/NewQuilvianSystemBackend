using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Satu berkas surat rujukan yang diunggah pasien di halaman Pendaftaran Pasien Online
    /// (kiosk Amendment PO, <c>KSK-DEC-035</c>). Berkas fisik tersimpan di root penyimpanan privat
    /// yang sama dengan dokumen rujukan kunjungan. Saat check-in, satu
    /// <see cref="RegEncounterReferralDocument"/> dibuat dengan <see cref="StoragePath"/> yang sama,
    /// tanpa menyalin berkas (<c>KSK-DSN-PO-010</c>).
    /// </summary>
    [Table("RegOnlineRegistrationReferralDocument", Schema = "public")]
    public class RegOnlineRegistrationReferralDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnlineRegistrationId { get; set; }

        /// <summary>Nama berkas dari perangkat pasien. Sensitif.</summary>
        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        /// <summary>Relatif terhadap root privat: <c>online-registration-referrals/{OnlineRegistrationId:N}/{nama-acak}.{ext}</c>.</summary>
        [Required]
        [MaxLength(500)]
        public string StoragePath { get; set; } = string.Empty;

        public int PageOrder { get; set; }

        /// <summary>Dokumen rujukan kunjungan yang dibuat untuk berkas ini saat check-in.</summary>
        public Guid? EncounterReferralDocumentId { get; set; }

        public RegOnlineRegistration? OnlineRegistration { get; set; }

        public RegEncounterReferralDocument? EncounterReferralDocument { get; set; }
    }
}
