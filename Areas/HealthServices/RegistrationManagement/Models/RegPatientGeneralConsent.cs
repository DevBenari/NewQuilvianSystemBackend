using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// General Consent yang ditandatangani saat pendaftaran Rawat Jalan pasien baru
    /// (<c>RJ-REG-REV-BE-007</c>, <c>RJ-REG-DEC-012</c>).
    /// </summary>
    /// <remarks>
    /// Satu kunjungan paling banyak punya satu consent yang hidup. Consent yang sudah tersimpan
    /// tidak pernah ditimpa: penyimpanan ulang mengembalikan isian yang sudah ada. Contoh: Tn.
    /// Budi, pasien baru, menandatangani sendiri 12 butir di loket pada kunjungan
    /// RJ-2610-0012; baris ini mencatat nama, NIK, tanda tangan, waktu server, dan petugasnya.
    /// </remarks>
    [Table("RegPatientGeneralConsent", Schema = "public")]
    public class RegPatientGeneralConsent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid PatientEncounterId { get; set; }

        [Required]
        public GeneralConsentSignerType SignerType { get; set; }

        [Required]
        [MaxLength(200)]
        public string SignerName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SignerRelationshipText { get; set; }

        /// <summary>NIK atau nomor paspor penanda tangan. Sensitif.</summary>
        [MaxLength(50)]
        public string? SignerIdentityNumber { get; set; }

        [MaxLength(13)]
        public string? SignerPhoneNumber { get; set; }

        [MaxLength(500)]
        public string? SignerAddress { get; set; }

        /// <summary>Nomor butir klausul yang disetujui, urut naik. Selalu 1 sampai 12.</summary>
        [Required]
        public int[] AgreedClauses { get; set; } = Array.Empty<int>();

        /// <summary>Citra tanda tangan digital, data URL PNG. Sensitif.</summary>
        [Required]
        public string SignatureImageBase64 { get; set; } = string.Empty;

        /// <summary>Waktu server saat consent disimpan (UTC).</summary>
        [Required]
        public DateTime SignedAt { get; set; } = DateTime.UtcNow;

        public MstPatient? Patient { get; set; }

        public RegPatientEncounter? PatientEncounter { get; set; }
    }
}
