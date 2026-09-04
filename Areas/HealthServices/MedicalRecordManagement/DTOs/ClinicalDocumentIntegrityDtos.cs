using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    /// <summary>
    /// Permintaan menandatangani sebuah catatan klinis.
    ///
    /// SENGAJA tidak memuat kata sandi, sidik jari, maupun identitas penanda tangan. Identitas
    /// diambil dari pengguna yang sedang masuk (RM-DEC-021), dan perangkat serta alamat
    /// jaringan diambil server dari permintaan — bila dikirim klien nilainya dapat dipalsukan
    /// dan kehilangan makna sebagai bukti.
    /// </summary>
    public class SignClinicalDocumentRequest
    {
        /// <summary>
        /// Pernyataan sadar dari penanda tangan. Bukan pengesahan ulang, melainkan pengingat
        /// bahwa tindakan ini mengunci catatan.
        /// </summary>
        public bool IsConfirmed { get; set; } = true;
    }

    public class ClinicalDocumentIntegrityResponse
    {
        public Guid Id { get; set; }
        public ClinicalDocumentKind DocumentKind { get; set; }
        public string DocumentKindName { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public Guid PatientId { get; set; }
        public Guid EncounterId { get; set; }

        public ClinicalDocumentIntegrityStatus IntegrityStatus { get; set; }
        public string IntegrityStatusName { get; set; } = string.Empty;

        public Guid AuthorUserId { get; set; }
        public string? AuthorName { get; set; }
        public bool IsAuthorKnown { get; set; }

        public DateTime? SignedAt { get; set; }
        public string? SignatureDeviceInfo { get; set; }

        public DateTime? LockedAt { get; set; }
        public ClinicalDocumentLockTrigger? LockTrigger { get; set; }
        public string? LockTriggerName { get; set; }

        public int AddendumCount { get; set; }

        /// <summary>
        /// Ringkasan yang dapat langsung ditampilkan: apakah catatan masih boleh diubah.
        /// Dihitung server supaya layar tidak perlu menafsirkan status sendiri.
        /// </summary>
        public bool IsMutable { get; set; }
    }

    /// <summary>
    /// Satu baris pada daftar "catatan saya yang belum saya tandatangani".
    ///
    /// Tanpa daftar ini, catatan yang lupa ditandatangani tidak dapat ditemukan, dan seluruhnya
    /// akan berakhir terkunci tanpa tanda tangan saat kunjungan ditutup — hasil yang berlawanan
    /// dengan tujuan RM-DEC-003.
    /// </summary>
    public class UnsignedDocumentResponse
    {
        public Guid IntegrityId { get; set; }
        public ClinicalDocumentKind DocumentKind { get; set; }
        public string DocumentKindName { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }

        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }

        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class CreateClinicalNoteAddendumRequest
    {
        [Required(ErrorMessage = "Isi koreksi wajib diisi.")]
        [MaxLength(4000, ErrorMessage = "Isi koreksi terlalu panjang. Batasnya 4000 huruf.")]
        public string AddendumText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alasan koreksi wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Alasan koreksi terlalu panjang. Batasnya 500 huruf.")]
        public string CorrectionReason { get; set; } = string.Empty;
    }

    public class ClinicalNoteAddendumResponse
    {
        public Guid Id { get; set; }
        public Guid IntegrityId { get; set; }
        public int Sequence { get; set; }

        public Guid AuthorUserId { get; set; }
        public string? AuthorName { get; set; }
        public bool IsSubstituteAuthor { get; set; }
        public Guid? DelegationId { get; set; }

        public string AddendumText { get; set; } = string.Empty;
        public string CorrectionReason { get; set; } = string.Empty;

        public DateTime SignedAt { get; set; }
    }
}
