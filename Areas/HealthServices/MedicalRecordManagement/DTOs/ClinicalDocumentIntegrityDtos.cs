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

        /// <summary>
        /// Nomor perawatan rawat inap yang menaungi dokumen ini — <c>BE-RWI-092</c>. Kosong
        /// untuk dokumen poliklinik, medical check-up, dan IGD.
        /// </summary>
        public string? EpisodeNumber { get; set; }

        /// <summary>
        /// Waktu klinis dokumen, yaitu saat pemeriksaannya benar-benar terjadi menurut
        /// penulisnya — <c>BE-RWI-092</c>.
        /// </summary>
        /// <remarks>
        /// <b>Sengaja terpisah dari <see cref="CreatedAt"/>.</b> Konsep SOAP shift malam dapat
        /// bertanggal klinis 02.00 tetapi baru tersimpan 06.30. Menampilkan satu angka saja
        /// membuat dokter mencari catatan pada jam yang salah.
        /// </remarks>
        public DateTime? ClinicalDateTime { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Satu baris pada daftar "Catatan Saya" — catatan <b>terkunci</b> milik penulis yang sedang
    /// masuk: <c>Signed</c> maupun <c>LockedUnsigned</c> — <c>BE-RWI-092</c>,
    /// <c>FR-DOK-079</c>, <c>RWI-DEC-127</c>, <c>RWI-DEC-142</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa daftar ini ada.</b> <c>RWI-DEC-111</c> membuat kartu pasien hilang dari daftar
    /// dokter begitu penugasannya berakhir. Itu benar untuk akses pasien, tetapi menutup satu
    /// hal yang sah: dokter mengoreksi catatannya sendiri. Tanpa daftar ini, dr. Yoga yang salah
    /// mengetik dosis pada SOAP pukul 02.00 Selasa tidak punya satu pun cara menemukan catatan
    /// itu pada hari Kamis — pasiennya sudah tidak muncul di daftarnya.
    /// </para>
    /// <para>
    /// <b>Identitas pasien yang dibawa MINIMUM</b> — <c>RWI-DEC-127</c> butir (2): nama, nomor
    /// rekam medis, dan nomor perawatan. Cukup untuk mengenali catatan mana yang dimaksud, dan
    /// tidak lebih. Nol kolom klinis pasien, nol diagnosis, nol resep.
    /// </para>
    /// </remarks>
    public class AuthoredDocumentItem
    {
        public Guid IntegrityId { get; set; }

        public ClinicalDocumentKind DocumentKind { get; set; }

        public string DocumentKindName { get; set; } = string.Empty;

        public Guid DocumentId { get; set; }

        public ClinicalDocumentIntegrityStatus IntegrityStatus { get; set; }

        public string IntegrityStatusName { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        /// <summary>Identitas minimum — <c>RWI-DEC-127</c> butir (2).</summary>
        public string? PatientName { get; set; }

        /// <summary>Identitas minimum — <c>RWI-DEC-127</c> butir (2).</summary>
        public string? MedicalRecordNumber { get; set; }

        public Guid EncounterId { get; set; }

        public string? EncounterNumber { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi catatan ini. Kosong untuk catatan di luar rawat
        /// inap. Dipakai layar untuk mengelompokkan daftar di bawah episode —
        /// <c>BE-RWI-092</c> kriteria 4.
        /// </summary>
        public Guid? InpEpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        /// <summary>Waktu klinis catatan, saat pemeriksaannya benar-benar terjadi.</summary>
        public DateTime? ClinicalDateTime { get; set; }

        /// <summary>Waktu tanda tangan. Kosong pada catatan <c>LockedUnsigned</c>.</summary>
        public DateTime? SignedAt { get; set; }

        /// <summary>Waktu penguncian, apa pun sebabnya.</summary>
        public DateTime? LockedAt { get; set; }

        public ClinicalDocumentLockTrigger? LockTrigger { get; set; }

        public string? LockTriggerName { get; set; }

        public int AddendumCount { get; set; }

        /// <summary>
        /// Benar bila catatan ini masih dapat menerima addendum dari penulisnya —
        /// <c>BE-RWI-093</c>. Dihitung server supaya layar tidak menafsirkan status sendiri.
        /// </summary>
        public bool CanAddAddendum { get; set; }

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
