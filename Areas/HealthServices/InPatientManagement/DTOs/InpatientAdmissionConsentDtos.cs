using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Permintaan simpan formulir persetujuan rawat inap dan tanda tangan digital (BE-RWI-205, RWI-DEC-272).
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

        /// <summary>Alamat lengkap penanda tangan persetujuan rawat inap.</summary>
        [MaxLength(500, ErrorMessage = "Alamat penanda tangan maksimal 500 karakter.")]
        public string? SignerAddress { get; set; }

        /// <summary>Daftar 12 klausul persetujuan umum rawat inap yang disetujui (VAL-ADM-04).</summary>
        public List<int> AgreedClauses { get; set; } = new();

        /// <summary>Citra tanda tangan digital format Base64 PNG (VAL-ADM-04).</summary>
        [Required(ErrorMessage = "Tanda tangan digital wajib dibubuhkan.")]
        public string SignatureImageBase64 { get; set; } = string.Empty;

        /// <summary>Waktu pembubuhan tanda tangan.</summary>
        public DateTime? SignedAt { get; set; }

        /// <summary>Sudah menerima panduan rawat inap; kosong = belum diisi (BE-RWI-227, ISS-EPS-008-T1).</summary>
        public bool? GuidanceReceived { get; set; }

        /// <summary>Keterangan penerimaan panduan rawat inap.</summary>
        [MaxLength(300, ErrorMessage = "Keterangan panduan maksimal 300 karakter.")]
        public string? GuidanceNote { get; set; }
    }

    /// <summary>
    /// Edit Persetujuan dari Workspace PPRI — selalu membuat versi baru, versi lama menjadi
    /// <c>Superseded</c> (BE-RWI-227, PLAN-REPAIR-EPS-008 FIX-01).
    /// </summary>
    /// <remarks>
    /// Contoh: NIK Ny. Rina salah satu angka. Petugas mengirim isian lengkap dengan NIK benar,
    /// <c>RowVersion</c> versi 1, dan alasan "NIK penanda tangan salah ketik saat admisi" → versi 2.
    /// <see cref="SignatureImageBase64"/> kosong berarti tanda tangan lama dipakai; wajib diisi bila
    /// <see cref="SignerType"/> berganti (K-01).
    /// </remarks>
    public class InpatientAdmissionConsentRevisionRequest
    {
        [Required(ErrorMessage = "RowVersion persetujuan wajib dikirim.")]
        public Guid? RowVersion { get; set; }

        [Required(ErrorMessage = "Alasan perubahan wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Alasan perubahan maksimal 500 karakter.")]
        public string CorrectionReason { get; set; } = string.Empty;

        /// <summary>Jenis penanda tangan: "Self", "Parent", "Guardian", "Spouse", "Family".</summary>
        [Required(ErrorMessage = "Jenis penanda tangan wajib dipilih.")]
        public string SignerType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama penanda tangan wajib diisi.")]
        [MinLength(3, ErrorMessage = "Nama penanda tangan minimal 3 karakter.")]
        [MaxLength(150, ErrorMessage = "Nama penanda tangan maksimal 150 karakter.")]
        public string SignerName { get; set; } = string.Empty;

        public string? SignerRelationship { get; set; }

        [MaxLength(100, ErrorMessage = "Keterangan hubungan maksimal 100 karakter.")]
        public string? SignerRelationshipText { get; set; }

        [MaxLength(13, ErrorMessage = "Nomor telepon penanda tangan maksimal 13 karakter.")]
        public string? SignerPhoneNumber { get; set; }

        [MaxLength(50, ErrorMessage = "Nomor identitas maksimal 50 karakter.")]
        public string? SignerIdentityNumber { get; set; }

        [MaxLength(500, ErrorMessage = "Alamat penanda tangan maksimal 500 karakter.")]
        public string? SignerAddress { get; set; }

        public bool? GuidanceReceived { get; set; }

        [MaxLength(300, ErrorMessage = "Keterangan panduan maksimal 300 karakter.")]
        public string? GuidanceNote { get; set; }

        /// <summary>Tanda tangan baru (data URL PNG); kosong = memakai tanda tangan versi sebelumnya.</summary>
        public string? SignatureImageBase64 { get; set; }
    }

    /// <summary>Satu baris Riwayat Perubahan persetujuan — satu versi (BE-RWI-227).</summary>
    public class InpatientAdmissionConsentHistoryResponse
    {
        public int VersionNo { get; set; }

        /// <summary>"Aktif", "Digantikan", "Dibatalkan", atau "Konsep".</summary>
        public string Status { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        /// <summary>Waktu versi ini dibuat (UTC).</summary>
        public DateTime ChangedAt { get; set; }

        public string? ChangedByName { get; set; }

        public string? ChangedByPosition { get; set; }

        /// <summary>Kosong pada versi pertama yang dibuat di langkah admisi.</summary>
        public string? CorrectionReason { get; set; }

        public bool IsSignatureReplaced { get; set; }

        public List<InpatientAdmissionConsentChangeResponse> Changes { get; set; } = new();
    }

    /// <summary>Satu isian yang berubah dibanding versi sebelumnya, misalnya "No. Identitas: 3171…98 → 3171…99".</summary>
    public class InpatientAdmissionConsentChangeResponse
    {
        public string Field { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string? Before { get; set; }

        public string? After { get; set; }
    }

    /// <summary>
    /// Respon dokumen persetujuan rawat inap terisi dan bertanda tangan digital (BE-RWI-205, RWI-AC-393, RWI-AC-395).
    /// </summary>
    /// <remarks>
    /// Bentuk yang sama juga dibekukan ke <c>SnapshotJson</c>. Field versi (<see cref="VersionNo"/>,
    /// <see cref="RowVersion"/>, <see cref="LastChangedByName"/>, <see cref="LastChangedAt"/>) selalu
    /// diisi ulang dari kepala dokumen saat dibaca, bukan dari salinan beku (BE-RWI-227).
    /// </remarks>
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

        public string? SignerAddress { get; set; }

        public List<int> AgreedClauses { get; set; } = new();

        public string SignatureImageBase64 { get; set; } = string.Empty;

        public DateTime? SignedAt { get; set; }

        public string Status { get; set; } = "Signed";

        public bool IsSigned { get; set; } = true;

        public bool? GuidanceReceived { get; set; }

        public string? GuidanceNote { get; set; }

        public int VersionNo { get; set; } = 1;

        /// <summary>Token konkurensi versi aktif; wajib dikirim balik saat Edit Persetujuan.</summary>
        public Guid RowVersion { get; set; }

        public string? LastChangedByName { get; set; }

        public DateTime? LastChangedAt { get; set; }
    }
}
