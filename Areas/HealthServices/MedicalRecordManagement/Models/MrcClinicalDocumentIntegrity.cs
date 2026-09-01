using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models
{
    /// <summary>
    /// Menyimpan keadaan keutuhan satu dokumen klinis: siapa penulisnya, sudah ditandatangani
    /// atau belum, sudah terkunci atau belum, dan karena apa terkuncinya.
    ///
    /// Tabel ini TIDAK menyimpan isi klinis apa pun — hanya keterangan tentang dokumennya.
    /// Bayangkan buku tamu di depan ruang arsip: ia tidak mengubah isi map mana pun, hanya
    /// mencatat keadaan setiap map.
    ///
    /// Rujukan ke dokumen bersifat polimorfik lewat pasangan
    /// <see cref="DocumentKind"/> + <see cref="DocumentId"/>, bukan foreign key. Akibatnya basis
    /// data tidak dapat menjamin dokumennya benar-benar ada; penjaminannya berada di
    /// ClinicalDocumentIntegrityService. Ini harga yang dibayar agar tiga belas tabel klinis
    /// yang sedang dipakai IGD, antrean dokter, dan farmasi tidak perlu diubah sama sekali
    /// (RM-DEC-013).
    /// </summary>
    [Table("MrcClinicalDocumentIntegrity", Schema = "public")]
    public class MrcClinicalDocumentIntegrity : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Jenis dokumen. Unik bersama <see cref="DocumentId"/>.</summary>
        public ClinicalDocumentKind DocumentKind { get; set; }

        /// <summary>
        /// Nilai Id pada tabel dokumen yang bersangkutan. Rujukan polimorfik, bukan foreign key.
        /// </summary>
        public Guid DocumentId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        public ClinicalDocumentIntegrityStatus IntegrityStatus { get; set; }
            = ClinicalDocumentIntegrityStatus.Draft;

        /// <summary>
        /// Penulis dokumen. TIDAK PERNAH boleh berubah setelah baris dibuat.
        ///
        /// Inilah yang menutup RM-CAP-012: kepemilikan penulis dipindahkan ke tabel yang tidak
        /// dapat disentuh permintaan ubah dokumen. Kolom ini tidak boleh masuk ke DTO ubah
        /// mana pun.
        /// </summary>
        [Required]
        public Guid AuthorUserId { get; set; }

        /// <summary>
        /// Bernilai salah untuk baris hasil pengisian data lama yang penulisnya tidak tercatat.
        /// Ditampilkan apa adanya pada laporan kelengkapan, tidak disembunyikan.
        /// </summary>
        public bool IsAuthorKnown { get; set; } = true;

        public DateTime? SignedAt { get; set; }

        /// <summary>
        /// Selalu sama dengan <see cref="AuthorUserId"/>. Disimpan terpisah agar terbaca
        /// eksplisit pada laporan dan penelusuran.
        /// </summary>
        public Guid? SignedByUserId { get; set; }

        /// <summary>
        /// Peramban dan perangkat saat menandatangani (RM-DEC-021). Diambil server dari
        /// permintaan, bukan dari kiriman klien — bila dikirim klien nilainya dapat dipalsukan.
        /// </summary>
        [MaxLength(250)]
        public string? SignatureDeviceInfo { get; set; }

        [MaxLength(64)]
        public string? SignatureIpAddress { get; set; }

        public DateTime? LockedAt { get; set; }

        public ClinicalDocumentLockTrigger? LockTrigger { get; set; }

        /// <summary>Waktu kunjungan ditutup, bila penguncian dipicu penutupan kunjungan.</summary>
        public DateTime? LockedEncounterClosedAt { get; set; }

        /// <summary>Alasan pembatalan dokumen. SENSITIF — dapat memuat keterangan klinis.</summary>
        [MaxLength(250)]
        public string? CancelledReason { get; set; }

        /// <summary>
        /// Jumlah addendum. Disimpan agar daftar tidak perlu menghitung ulang setiap kali.
        /// </summary>
        public int AddendumCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstPatient? Patient { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public ICollection<MrcClinicalNoteAddendum> Addendums { get; set; }
            = new List<MrcClinicalNoteAddendum>();
    }
}
