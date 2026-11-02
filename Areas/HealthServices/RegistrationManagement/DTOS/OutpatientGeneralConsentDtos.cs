using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Isian General Consent Rawat Jalan dari langkah pendaftaran pasien baru
    /// (<c>RJ-REG-REV-BE-007</c>, <c>RJ-REG-DEC-012</c>).
    /// </summary>
    /// <remarks>
    /// Waktu tanda tangan dan petugas diisi server, bukan dari klien. Contoh: Ny. Rina menandatangani
    /// untuk anaknya → <c>signerType</c> <c>"Parent"</c>, <c>signerRelationshipText</c> <c>"Ibu
    /// Kandung"</c>, <c>agreedClauses</c> 1–12, <c>signatureImageBase64</c>
    /// <c>"data:image/png;base64,..."</c>.
    /// </remarks>
    public class OutpatientGeneralConsentSaveRequest
    {
        /// <summary>"Self", "Parent", "Guardian", "Spouse", atau "Family".</summary>
        [Required(ErrorMessage = "Jenis penanda tangan wajib dipilih.")]
        public string SignerType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama penanda tangan wajib diisi.")]
        [MinLength(3, ErrorMessage = "Nama penanda tangan minimal 3 karakter.")]
        [MaxLength(200, ErrorMessage = "Nama penanda tangan maksimal 200 karakter.")]
        public string SignerName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Keterangan hubungan maksimal 100 karakter.")]
        public string? SignerRelationshipText { get; set; }

        [MaxLength(50, ErrorMessage = "Nomor identitas maksimal 50 karakter.")]
        public string? SignerIdentityNumber { get; set; }

        /// <summary>Hanya angka, maksimal 13 digit.</summary>
        [MaxLength(13, ErrorMessage = "Nomor telepon penanda tangan maksimal 13 karakter.")]
        public string? SignerPhoneNumber { get; set; }

        [MaxLength(500, ErrorMessage = "Alamat penanda tangan maksimal 500 karakter.")]
        public string? SignerAddress { get; set; }

        /// <summary>Wajib berisi tepat butir 1 sampai 12.</summary>
        public List<int> AgreedClauses { get; set; } = new();

        /// <summary>Citra tanda tangan digital, data URL PNG.</summary>
        [Required(ErrorMessage = "Tanda tangan digital wajib dibubuhkan.")]
        public string SignatureImageBase64 { get; set; } = string.Empty;
    }

    /// <summary>General Consent Rawat Jalan yang tersimpan pada satu kunjungan.</summary>
    public class OutpatientGeneralConsentResponse
    {
        public Guid ConsentId { get; set; }

        public Guid EncounterId { get; set; }

        public string EncounterNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string SignerType { get; set; } = string.Empty;

        public string SignerTypeName { get; set; } = string.Empty;

        public string SignerName { get; set; } = string.Empty;

        public string? SignerRelationshipText { get; set; }

        public string? SignerIdentityNumber { get; set; }

        public string? SignerPhoneNumber { get; set; }

        public string? SignerAddress { get; set; }

        public List<int> AgreedClauses { get; set; } = new();

        public string SignatureImageBase64 { get; set; } = string.Empty;

        /// <summary>Waktu server saat consent disimpan (UTC).</summary>
        public DateTime SignedAt { get; set; }

        /// <summary>Petugas pendaftaran yang menyimpan consent.</summary>
        public string? OfficerName { get; set; }
    }
}
