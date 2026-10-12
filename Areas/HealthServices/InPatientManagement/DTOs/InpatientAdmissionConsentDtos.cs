using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Permintaan simpan formulir persetujuan rawat inap & tanda tangan digital (BE-RWI-205, RWI-DEC-272).
    /// </summary>
    public class InpatientAdmissionConsentCreateDto
    {
        /// <summary>Kategori pasien saat admisi: "Umum", "BayiBaruLahir", "Pegawai" (RWI-DEC-270).</summary>
        public string? PatientCategory { get; set; }

        /// <summary>Jenis penanda tangan: "Self", "Guardian", "Parent", "Spouse", "Family".</summary>
        [Required(ErrorMessage = "Jenis penanda tangan wajib dipilih.")]
        public string SignerType { get; set; } = string.Empty;

        /// <summary>Nama lengkap penanda tangan persetujuan.</summary>
        [Required(ErrorMessage = "Nama penanda tangan wajib diisi.")]
        [MinLength(3, ErrorMessage = "Nama penanda tangan minimal 3 karakter.")]
        [MaxLength(200, ErrorMessage = "Nama penanda tangan maksimal 200 karakter.")]
        public string SignerName { get; set; } = string.Empty;

        /// <summary>Hubungan penanda tangan terstruktur (Self, Parent, Guardian, Spouse, Child, Other).</summary>
        public string? SignerRelationship { get; set; }

        /// <summary>Keterangan hubungan bebas jika Other atau detail wali.</summary>
        [MaxLength(100, ErrorMessage = "Keterangan hubungan maksimal 100 karakter.")]
        public string? SignerRelationshipText { get; set; }

        /// <summary>Nomor HP penanda tangan (maksimal 13 digit numerik).</summary>
        [MaxLength(13, ErrorMessage = "Nomor telepon penanda tangan maksimal 13 karakter.")]
        public string? SignerPhoneNumber { get; set; }

        /// <summary>Nomor identitas KTP/SIM/Paspor penanda tangan.</summary>
        [MaxLength(50, ErrorMessage = "Nomor identitas maksimal 50 karakter.")]
        public string? SignerIdentityNumber { get; set; }

        /// <summary>Daftar 12 klausul persetujuan umum rawat inap yang disetujui (VAL-ADM-04).</summary>
        public List<int> AgreedClauses { get; set; } = new();

        /// <summary>Citra tanda tangan digital format Base64 PNG (VAL-ADM-04).</summary>
        [Required(ErrorMessage = "Tanda tangan digital wajib dibubuhkan.")]
        public string SignatureImageBase64 { get; set; } = string.Empty;

        /// <summary>Waktu pembubuhan tanda tangan.</summary>
        public DateTime? SignedAt { get; set; }
    }

    /// <summary>
    /// Respon dokumen persetujuan rawat inap terisi dan bertanda tangan digital (BE-RWI-205, RWI-AC-393, RWI-AC-395).
    /// </summary>
    public class InpatientAdmissionConsentResponse
    {
        public Guid ConsentId { get; set; }

        public Guid EpisodeId { get; set; }

        public string DocumentNumber { get; set; } = string.Empty;

        public string? PatientCategory { get; set; }

        public string SignerType { get; set; } = string.Empty;

        public string SignerName { get; set; } = string.Empty;

        public string? SignerRelationship { get; set; }

        public string? SignerRelationshipText { get; set; }

        public string? SignerPhoneNumber { get; set; }

        public string? SignerIdentityNumber { get; set; }

        public List<int> AgreedClauses { get; set; } = new();

        public string SignatureImageBase64 { get; set; } = string.Empty;

        public DateTime? SignedAt { get; set; }

        public string Status { get; set; } = "Signed";

        public bool IsSigned { get; set; } = true;
    }
}
