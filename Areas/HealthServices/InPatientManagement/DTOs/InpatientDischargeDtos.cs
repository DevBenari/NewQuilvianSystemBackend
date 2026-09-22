using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Bentuk permintaan DPJP menyatakan pasien boleh pulang.
    /// </summary>
    /// <remarks>
    /// <b>Tempat tidur belum dilepas pada langkah ini.</b> Episode berpindah ke
    /// <c>DischargePending</c>, pasien tetap muncul pada census, dan salinan status tempat
    /// tidur tidak berubah. Pelepasannya baru terjadi ketika kepergian fisik dicatat
    /// (<c>BE-RWI-027</c>) atau episode ditutup (<c>BE-RWI-025</c>).
    /// </remarks>
    public class DecideDischargeRequest
    {
        /// <summary>
        /// Cara pulang. Nilai yang berlaku pada revisi ini: 1 <c>DoctorApproved</c>,
        /// 2 <c>AgainstMedicalAdvice</c>, 3 <c>Referred</c>.
        /// </summary>
        public int DischargeType { get; set; }

        /// <summary>
        /// Alasan keputusan pulang. Ikut tersimpan pada baris riwayat status episode.
        /// </summary>
        /// <remarks>
        /// <b>Tujuan rujukan sengaja tidak ada di sini.</b> Ia milik resume pulang, dan
        /// diwajibkan pada saat resume ditandatangani — validation matrix bagian 6. Menyediakan
        /// kolomnya di sini akan membuat dua tempat menyimpan nilai yang sama, dan keduanya
        /// akan berselisih pada kasus pertama yang tujuannya berubah.
        /// </remarks>
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan menyusun atau memperbarui resume pulang.
    /// </summary>
    /// <remarks>
    /// Seluruh kolom isi resume bertanda <b>sensitif</b> pada permission matrix bagian 5.4.
    /// Tidak satu pun boleh masuk payload logger, dan tidak satu pun boleh ikut pada endpoint
    /// daftar mana pun.
    /// </remarks>
    public class UpsertDischargeSummaryRequest
    {
        [Required]
        [MaxLength(1000)]
        public string PrimaryDiagnosisText { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? SecondaryDiagnosisText { get; set; }

        [MaxLength(2000)]
        public string? ProcedureSummary { get; set; }

        [MaxLength(2000)]
        public string? DischargeMedicationNote { get; set; }

        [MaxLength(2000)]
        public string? FollowUpInstruction { get; set; }

        [MaxLength(250)]
        public string? ReferralDestination { get; set; }

        /// <summary>
        /// Ringkasan Perawatan. <b>Kolom lama, label baru</b> — <c>RWI-DEC-112</c>. Bentuk dan
        /// panjangnya tidak berubah; yang berubah hanya sebutannya di layar.
        /// </summary>
        [MaxLength(4000)]
        public string? ClinicalSummary { get; set; }

        /// <summary>
        /// Pemeriksaan Penting — hasil penunjang yang membentuk keputusan klinis selama
        /// perawatan. Isian baru <c>BE-RWI-085</c>. <b>SENSITIF.</b>
        /// </summary>
        [MaxLength(4000)]
        public string? ImportantFindingsSummary { get; set; }

        /// <summary>
        /// Kondisi Saat Pulang — keadaan pasien ketika meninggalkan rumah sakit. Isian baru
        /// <c>BE-RWI-085</c>. <b>SENSITIF.</b>
        /// </summary>
        [MaxLength(2000)]
        public string? DischargeConditionNote { get; set; }

        /// <summary>
        /// Edukasi — apa yang sudah dijelaskan kepada pasien dan keluarganya. Isian baru
        /// <c>BE-RWI-085</c>. <b>SENSITIF.</b>
        /// </summary>
        /// <remarks>
        /// <b>Ketiganya tetap opsional.</b> Isi minimal resume berada di bawah gerbang pemilik
        /// klinis — <c>RWI-RULE-032</c>, <c>04-prd-to-mvp.md</c> 22.7 nomor 3. Menjadikannya
        /// wajib di sini berarti menetapkan kebijakan klinis dari dalam kode.
        /// </remarks>
        [MaxLength(2000)]
        public string? EducationSummary { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan DPJP menandatangani resume pulang.
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada kolom penandatangan.</b> Penandatangan diturunkan dari DPJP aktif episode
    /// dan dari pengguna yang terautentikasi, tidak pernah dari isian permintaan. Menerimanya
    /// dari pemanggil membuat <c>GUARD-INP-03</c> dapat dilewati hanya dengan mengirim
    /// identifier dokter lain.
    /// </remarks>
    public class SignDischargeSummaryRequest
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>Resume pulang satu episode.</summary>
    public class DischargeSummaryResponse
    {
        public Guid Id { get; set; }

        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        public string PrimaryDiagnosisText { get; set; } = string.Empty;

        public string? SecondaryDiagnosisText { get; set; }

        public string? ProcedureSummary { get; set; }

        public string? DischargeMedicationNote { get; set; }

        public string? FollowUpInstruction { get; set; }

        public string? ReferralDestination { get; set; }

        public string? ClinicalSummary { get; set; }

        /// <summary>Pemeriksaan Penting. Isian baru <c>BE-RWI-085</c>. <b>SENSITIF.</b></summary>
        public string? ImportantFindingsSummary { get; set; }

        /// <summary>Kondisi Saat Pulang. Isian baru <c>BE-RWI-085</c>. <b>SENSITIF.</b></summary>
        public string? DischargeConditionNote { get; set; }

        /// <summary>Edukasi. Isian baru <c>BE-RWI-085</c>. <b>SENSITIF.</b></summary>
        public string? EducationSummary { get; set; }

        public DateTime? SignedAt { get; set; }

        public Guid? SignedByDoctorId { get; set; }

        public string? SignedByDoctorName { get; set; }

        public bool IsSigned { get; set; }

        public int DischargeType { get; set; }

        public string DischargeTypeName { get; set; } = string.Empty;

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        /// <summary>
        /// Versi resume yang sudah digantikan, urut waktu. Hanya terisi bila pemanggil
        /// meminta <c>includeRevisions=true</c>.
        /// </summary>
        public List<DischargeSummaryRevisionResponse> Revisions { get; set; } = new();
    }

    /// <summary>
    /// Salinan satu versi resume yang sudah digantikan.
    /// </summary>
    /// <remarks>
    /// Baris versi <b>tidak dapat diubah dan tidak dapat dihapus</b>. Tidak ada endpoint
    /// <c>PUT</c> maupun <c>DELETE</c> yang menunjuk baris ini, dan ketiadaannya disengaja —
    /// api contract bagian 8 dan <c>RWI-DEC-057</c>.
    /// </remarks>
    public class DischargeSummaryRevisionResponse
    {
        public Guid Id { get; set; }

        public Guid DischargeSummaryId { get; set; }

        public int RevisionNumber { get; set; }

        public Guid? CorrectionSessionId { get; set; }

        public string PrimaryDiagnosisText { get; set; } = string.Empty;

        public string? SecondaryDiagnosisText { get; set; }

        public string? ProcedureSummary { get; set; }

        public string? DischargeMedicationNote { get; set; }

        public string? FollowUpInstruction { get; set; }

        public string? ReferralDestination { get; set; }

        public string? ClinicalSummary { get; set; }

        /// <summary>Salinan Pemeriksaan Penting versi ini. <b>SENSITIF.</b></summary>
        public string? ImportantFindingsSummary { get; set; }

        /// <summary>Salinan Kondisi Saat Pulang versi ini. <b>SENSITIF.</b></summary>
        public string? DischargeConditionNote { get; set; }

        /// <summary>Salinan Edukasi versi ini. <b>SENSITIF.</b></summary>
        public string? EducationSummary { get; set; }

        public int PreviousDischargeType { get; set; }

        public string PreviousDischargeTypeName { get; set; } = string.Empty;

        public DateTime PreviousSignedAt { get; set; }

        public Guid PreviousSignedByDoctorId { get; set; }

        public string? PreviousSignedByDoctorName { get; set; }

        public DateTime SupersededAt { get; set; }

        public Guid SupersededByUserId { get; set; }
    }

    /// <summary>
    /// Satu sumber klinis yang membentuk sebuah usulan isian resume. <c>BE-RWI-086</c>.
    /// </summary>
    /// <remarks>
    /// <b>Label sumber adalah bagian dari kontrak, bukan hiasan.</b> Dokter yang membaca usulan
    /// perlu tahu dari mana kalimatnya berasal sebelum menandatanganinya — "Sumber: Hasil
    /// Laboratorium 14 Sep 2026" adalah yang membedakan usulan bersumber dari teks yang muncul
    /// entah dari mana.
    /// </remarks>
    public class DischargeSummaryPrefillSourceResponse
    {
        /// <summary>Nama sumbernya sebagaimana dibaca dokter.</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>Keterangan tambahan, misalnya nama dokter atau pelaksananya.</summary>
        public string? Detail { get; set; }

        /// <summary>Kapan datanya tercatat.</summary>
        public DateTime? RecordedAt { get; set; }
    }

    /// <summary>Usulan untuk satu isian resume beserta keadaan sumbernya. <c>BE-RWI-086</c>.</summary>
    public class DischargeSummaryPrefillFieldResponse
    {
        /// <summary>
        /// Teks usulan. <b>Tidak tersimpan ke mana pun</b> sampai dokter menyimpannya sendiri
        /// lewat <c>PUT /{episodeId}/summary</c>.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Keadaan sumbernya: <c>1</c> <c>Available</c>, <c>2</c> <c>Empty</c>, <c>3</c>
        /// <c>Unavailable</c>.
        /// </summary>
        public int SourceStatus { get; set; }

        public string SourceStatusName { get; set; } = string.Empty;

        /// <summary>Keterangan singkat, terutama ketika isiannya kosong atau gagal dibaca.</summary>
        public string? Note { get; set; }

        /// <summary>
        /// Nama jenis galat ketika sumbernya gagal dibaca. <b>Tidak pernah memuat pesan galat
        /// aslinya</b>: pesan itu dapat mengandung nama tabel, kolom, dan potongan data.
        /// </summary>
        public string? ErrorType { get; set; }

        /// <summary>Sumber-sumber yang membentuk usulan ini.</summary>
        public List<DischargeSummaryPrefillSourceResponse> Sources { get; set; } = new();
    }

    /// <summary>
    /// Waktu penyelesaian pembacaan satu sumber. <c>BE-RWI-086</c> acceptance criteria 5.
    /// </summary>
    /// <remarks>
    /// <b>Angkanya dilaporkan apa adanya.</b> <c>NFR-027</c> menyebut batas 5 detik per sumber
    /// sebagai angka <b>usulan desain</b>. Bila pengukuran nyata jauh melampauinya, angkanya
    /// dibawa kembali ke pemilik — bukan diam-diam diubah di roadmap.
    /// </remarks>
    public class DischargeSummaryPrefillTimingResponse
    {
        /// <summary>Nama isian yang diukur.</summary>
        public string Field { get; set; } = string.Empty;

        public long ElapsedMilliseconds { get; set; }

        /// <summary>Salah bila pembacaan sumbernya gagal.</summary>
        public bool IsSuccessful { get; set; }
    }

    /// <summary>
    /// Usulan isian resume pulang dari data klinis yang sudah ada. <c>BE-RWI-086</c>,
    /// kontrak <c>0.9.0</c> bagian 10.3.
    /// </summary>
    /// <remarks>
    /// <b>Memanggil endpoint ini tidak menyimpan apa pun.</b> Yang tersimpan sebagai resume
    /// selalu teks final dokter, lewat <c>PUT /{episodeId}/summary</c> — <c>RWI-DEC-112</c>.
    /// </remarks>
    public class DischargeSummaryPrefillResponse
    {
        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        /// <summary>
        /// Benar bila resume episode ini sudah ditandatangani. Usulan tidak dibentuk untuk
        /// resume yang sudah bertanda tangan — acceptance criteria 4.
        /// </summary>
        public bool IsSummarySigned { get; set; }

        /// <summary>Keterangan umum, terisi ketika usulan sengaja tidak dibentuk.</summary>
        public string? Message { get; set; }

        public DischargeSummaryPrefillFieldResponse? PrimaryDiagnosisText { get; set; }

        public DischargeSummaryPrefillFieldResponse? SecondaryDiagnosisText { get; set; }

        public DischargeSummaryPrefillFieldResponse? ClinicalSummary { get; set; }

        public DischargeSummaryPrefillFieldResponse? ImportantFindingsSummary { get; set; }

        public DischargeSummaryPrefillFieldResponse? ProcedureSummary { get; set; }

        public DischargeSummaryPrefillFieldResponse? DischargeMedicationNote { get; set; }

        public DischargeSummaryPrefillFieldResponse? DischargeConditionNote { get; set; }

        public DischargeSummaryPrefillFieldResponse? FollowUpInstruction { get; set; }

        public DischargeSummaryPrefillFieldResponse? EducationSummary { get; set; }

        /// <summary>Waktu penyelesaian pembacaan setiap sumber.</summary>
        public List<DischargeSummaryPrefillTimingResponse> SourceTimings { get; set; } = new();
    }
}
