using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    /// <summary>
    /// Permintaan untuk menyatakan hasil pemeriksaan kecocokan satu kantong terhadap
    /// pasien tujuan yang sedang menerima alokasi kantong tersebut.
    /// </summary>
    /// <remarks>
    /// PatientId tidak diterima dari client. Pasien tujuan selalu diturunkan dari
    /// alokasi aktif sehingga client tidak dapat mengganti pasien secara arbitrer.
    /// </remarks>
    public sealed class RecordEvidenceRequest
    {
        /// <summary>
        /// Hasil pemeriksaan yang dinyatakan validator:
        /// Compatible atau Incompatible.
        /// </summary>
        public BbkCompatibilityResult? EvidenceResult { get; set; }

        /// <summary>
        /// Waktu pemeriksaan yang dinyatakan valid oleh validator.
        /// Wajib menggunakan UTC.
        /// </summary>
        public DateTime CheckedAt { get; set; }

        /// <summary>
        /// Token optimistis kantong. Bila diberikan dan berbeda dengan nilai terkini,
        /// tindakan ditolak sebagai bentrok konkurensi.
        /// </summary>
        public int? Version { get; set; }
    }

    /// <summary>
    /// Satu bukti pemeriksaan kecocokan yang tersimpan pada kantong.
    /// </summary>
    public sealed class CompatibilityEvidenceDto
    {
        public Guid Id { get; set; }
        public Guid BloodUnitId { get; set; }
        public Guid PatientId { get; set; }

        public BbkCompatibilityResult? EvidenceResult { get; set; }
        public string EvidenceResultLabel { get; set; } = string.Empty;

        public Guid ValidatedByUserId { get; set; }
        public DateTime CheckedAt { get; set; }

        /// <summary>
        /// Bukti sudah digugurkan akibat pengalihan kantong.
        /// Bukti tetap tersimpan sebagai riwayat.
        /// </summary>
        public bool IsSuperseded { get; set; }

        /// <summary>
        /// Sebab bukti ini gugur, misalnya karena kantong dialihkan ke pasien lain
        /// (<c>DEC-BD-028</c>). Kosong selama bukti masih berlaku.
        /// </summary>
        public string? SupersededReason { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>
    /// Request pemberian kantong melalui jalur normal.
    /// Jalur darurat mempunyai endpoint dan request tersendiri pada BE-BD-008.
    /// </summary>
    public sealed class IssueUnitRequest
    {
        /// <summary>
        /// Token optimistis kantong untuk mendeteksi perubahan setelah layar dibuka.
        /// </summary>
        public int? Version { get; set; }
    }

    /// <summary>
    /// Hasil evaluasi internal gerbang pemberian normal.
    /// Tidak digunakan sebagai cara melewati gerbang; service Issue harus tetap
    /// mengevaluasi gerbang ini pada saat tindakan dilakukan.
    /// </summary>
    /// <remarks>
    /// <c>ValidUntil</c> hanya terisi pada dua keadaan yang memang menghitungnya: gerbang terbuka
    /// dan bukti kedaluwarsa (<c>VAL-BD-020</c>). Kosong berarti tidak dihitung, bukan berlaku
    /// tanpa batas (<c>BE-BD-021</c>).
    /// </remarks>
    public sealed record BloodUnitIssuanceGateResult(
        bool IsOpen,
        string? ValidationCode,
        string Message,
        Guid? PatientId = null,
        Guid? CompatibilityEvidenceId = null,
        DateTime? ValidUntil = null);

    /// <summary>
    /// Proyeksi gerbang pemberian normal pada detail kantong (<c>BE-BD-021</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hasil <c>EvaluateIssuanceGateAsync</c> apa adanya saat detail dibaca. <b>Petunjuk, bukan
    /// izin</b>: <c>issue</c> tetap menilai ulang gerbangnya saat tindakan dilakukan, sehingga
    /// keadaan yang berubah sesudah detail dibaca tetap ditolak dengan kode yang sama.
    /// </para>
    /// <para>
    /// Penilaian berhenti pada penolakan pertama. Bila lokasi sudah menolak (<c>VAL-BD-065</c>),
    /// bukti tidak dinilai di sini; keadaannya terbaca pada
    /// <see cref="BloodUnitEmergencyBypassDto.EvidenceGateClosed"/>.
    /// </para>
    /// </remarks>
    public sealed class BloodUnitIssuanceGateDto
    {
        public bool IsOpen { get; set; }

        /// <summary>Kode penolakan <c>VAL-BD-*</c>. Kosong ketika gerbang terbuka.</summary>
        public string? ValidationCode { get; set; }

        public string Message { get; set; } = string.Empty;

        /// <summary>Bukti yang dinilai gerbang, bila penilaian sampai pada bukti pasien tujuan.</summary>
        public Guid? CompatibilityEvidenceId { get; set; }

        /// <summary>
        /// Batas berlaku bukti yang dinilai (UTC). Hanya terisi ketika gerbang terbuka atau bukti
        /// kedaluwarsa (<c>VAL-BD-020</c>); kosong berarti tidak dihitung.
        /// </summary>
        public DateTime? ValidUntil { get; set; }
    }
}