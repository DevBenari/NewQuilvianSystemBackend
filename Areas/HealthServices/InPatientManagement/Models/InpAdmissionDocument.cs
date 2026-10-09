using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Kepala satu <b>versi</b> dokumen admisi Workspace PPRI — kamus data 20.2 (<c>BE-RWI-192</c>,
    /// migration <c>E10</c>, <c>RWI-DEC-228</c>, <c>240</c>, <c>263</c>).
    /// </summary>
    /// <remarks>
    /// Setiap versi koreksi adalah baris sendiri yang menunjuk versi sebelumnya lewat
    /// <see cref="PreviousVersionId"/>, sehingga tanda tangan, salinan beku, dan isi versi lama
    /// tidak pernah ditimpa. Identitas pasien tidak disimpan sebagai kolom: ia dibekukan di
    /// <see cref="SnapshotJson"/> saat dokumen dikunci (<c>INV-RWA-05</c>, <c>INV-RWA-10</c>).
    ///
    /// <para>
    /// Contoh: Selisih Biaya Tn. Budi versi 1 <c>Completed</c> pukul 10.30. Pukul 13.00 dibuat versi
    /// koreksi beralasan "koreksi alamat deklarer": satu transaksi mengubah versi 1 menjadi
    /// <c>Superseded</c>, lalu membuat versi 2 <c>Draft</c> yang menunjuk versi 1. Unique index dokumen
    /// aktif tetap berisi satu baris.
    /// </para>
    /// </remarks>
    public class InpAdmissionDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EpisodeId { get; set; }

        /// <summary>
        /// Diisi server dari episode. Dipakai mencari Nilai Kepercayaan <c>Completed</c> terakhir
        /// pasien lintas episode (<c>RWI-DEC-242</c>).
        /// </summary>
        public Guid PatientId { get; set; }

        public InpAdmissionDocumentType DocumentType { get; set; }

        public InpAdmissionDocumentStatus Status { get; set; } = InpAdmissionDocumentStatus.Draft;

        /// <summary>
        /// Versi di dalam satu rantai koreksi. Bukan nomor bisnis; rantai baru sesudah pembatalan
        /// mulai dari 1.
        /// </summary>
        public int VersionNo { get; set; } = 1;

        /// <summary>Versi yang dikoreksi; satu versi hanya punya satu pengganti.</summary>
        public Guid? PreviousVersionId { get; set; }

        /// <summary>Wajib bila <see cref="VersionNo"/> &gt; 1; 10–500 karakter. SENSITIF.</summary>
        public string? CorrectionReason { get; set; }

        /// <summary>Kota penandatanganan; bawaan dari pengaturan saat dibuat.</summary>
        public string? SigningCity { get; set; }

        /// <summary>"Tanggal" pada formulir; tanggal surat pada Pelunasan Deposit.</summary>
        public DateTime? StatementDate { get; set; }

        /// <summary>"Keterangan" atau "Catatan" formulir. SENSITIF.</summary>
        public string? Note { get; set; }

        /// <summary>Bentuk <see cref="SnapshotJson"/>; <c>1</c> pada revision ini.</summary>
        public int? SnapshotFormatVersion { get; set; }

        /// <summary>Salinan beku saat dikunci (kamus data 20.2.1); dikosongkan saat buka kunci. SENSITIF.</summary>
        public string? SnapshotJson { get; set; }

        public DateTime? LockedAt { get; set; }

        public Guid? LockedByUserId { get; set; }

        /// <summary>Waktu slot wajib terakhir terisi.</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>Waktu versi koreksi dibuat.</summary>
        public DateTime? SupersededAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        /// <summary>Buang konsep 1–500 karakter; batal dokumen 10–500 karakter. SENSITIF.</summary>
        public string? CancelledReason { get; set; }

        /// <summary>Dari header <c>Idempotency-Key</c> simpan konsep atau versi koreksi.</summary>
        public string? IdempotencyKey { get; set; }

        /// <summary>Token konkurensi; berganti setiap kali dokumen atau anaknya diubah.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public InpEpisode? Episode { get; set; }

        public MstPatient? Patient { get; set; }

        public InpAdmissionDocument? PreviousVersion { get; set; }

        public InpAdmissionDocumentParty? Party { get; set; }

        public InpAdmissionPrivacyRequest? PrivacyRequest { get; set; }

        public InpAdmissionCostDifferenceStatement? CostDifferenceStatement { get; set; }

        public InpAdmissionDepositStatement? DepositStatement { get; set; }

        public ICollection<InpAdmissionDocumentSignature> Signatures { get; set; } = new List<InpAdmissionDocumentSignature>();

        public ICollection<InpAdmissionHandoverItem> HandoverItems { get; set; } = new List<InpAdmissionHandoverItem>();

        public ICollection<InpAdmissionPrivacyEntry> PrivacyEntries { get; set; } = new List<InpAdmissionPrivacyEntry>();

        public ICollection<InpAdmissionBeliefItem> BeliefItems { get; set; } = new List<InpAdmissionBeliefItem>();
    }
}
