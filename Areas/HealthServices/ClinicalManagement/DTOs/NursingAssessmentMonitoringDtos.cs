using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    // =========================================================================
    // BE-RWI-058 — lini masa pengkajian satu perawatan
    // =========================================================================

    /// <summary>
    /// Satu titik pengukuran pada lini masa. Setiap titik berasal dari satu pengkajian, dan
    /// nilai lama <b>tidak pernah</b> ditimpa nilai baru.
    /// </summary>
    public class AssessmentMeasurementPointResponse
    {
        public Guid AssessmentId { get; set; }

        public string AssessmentNumber { get; set; } = string.Empty;

        public PatientAssessmentType AssessmentType { get; set; }

        public DateTime MeasuredAt { get; set; }

        /// <summary>Angka pengukuran bila ada, misalnya skala nyeri atau skor risiko jatuh.</summary>
        public int? Score { get; set; }

        /// <summary>Kategori pengukuran dalam bahasa layar, misalnya "Risiko tinggi".</summary>
        public string? Category { get; set; }

        /// <summary>
        /// Banyaknya koreksi yang menempel pada pengkajian asal titik ini. Lebih dari nol berarti
        /// isi aslinya tetap ada dan koreksinya menempel di bawahnya — <c>RWI-DEC-091</c>.
        /// </summary>
        public int AddendumCount { get; set; }

        /// <summary>Nomor urut koreksi terakhir, bila pengkajian ini pernah dikoreksi.</summary>
        public int? LastAddendumSequence { get; set; }
    }

    /// <summary>Satu deret pengukuran, misalnya seluruh penilaian nyeri pada satu perawatan.</summary>
    public class AssessmentMeasurementSeriesResponse
    {
        /// <summary>Kunci teknis deret: <c>pain</c>, <c>fallRisk</c>, atau <c>nutrition</c>.</summary>
        public string Measurement { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public List<AssessmentMeasurementPointResponse> Points { get; set; } = new();
    }

    /// <summary>Satu baris pengkajian pada lini masa, beserta keadaan tenggatnya.</summary>
    public class AssessmentTimelineEntryResponse
    {
        public Guid AssessmentId { get; set; }

        public string AssessmentNumber { get; set; } = string.Empty;

        public PatientAssessmentType AssessmentType { get; set; }

        public string AssessmentTypeName { get; set; } = string.Empty;

        public DateTime AssessmentDateTime { get; set; }

        public PatientAssessmentStatus AssessmentStatus { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? DueAt { get; set; }

        public Guid? PolicyId { get; set; }

        public string? PolicyCode { get; set; }

        public AssessmentDueState DueState { get; set; }

        /// <summary>Selisih keterlambatan dalam menit. Kosong bila tidak terlambat.</summary>
        public int? LateByMinutes { get; set; }

        public Guid? AssessmentByUserId { get; set; }

        public string? AssessmentByUserName { get; set; }

        public int AddendumCount { get; set; }

        public int? LastAddendumSequence { get; set; }

        public bool HasPain { get; set; }

        public int? PainScale { get; set; }

        public FallRiskStatus FallRiskStatus { get; set; }

        public int? FallRiskScore { get; set; }

        public NutritionRiskStatus NutritionRiskStatus { get; set; }

        public int? NutritionRiskScore { get; set; }
    }

    /// <summary>
    /// Lini masa pengkajian satu perawatan — <c>AC-CAP012-02</c>.
    /// </summary>
    /// <remarks>
    /// Menjawab pertanyaan yang tidak dapat dijawab daftar biasa: <i>apakah nyeri pasien
    /// membaik atau memburuk</i>. Karena itu isinya seluruh pengukuran terurut waktu, bukan
    /// hanya nilai terakhirnya.
    /// </remarks>
    public class AssessmentTimelineResponse
    {
        public Guid EpisodeId { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public DateTime? AdmittedAt { get; set; }

        public int TotalAssessment { get; set; }

        /// <summary>
        /// Benar bila master kebijakan batas waktu belum berisi satu pun baris aktif. Layar
        /// memakainya untuk menulis "batas waktu pengkajian belum ditetapkan", bukan "terlambat".
        /// </summary>
        public bool IsPolicyMasterEmpty { get; set; }

        public List<AssessmentTimelineEntryResponse> Entries { get; set; } = new();

        public List<AssessmentMeasurementSeriesResponse> Series { get; set; } = new();
    }

    // =========================================================================
    // BE-RWI-058 — keadaan tenggat satu perawatan
    // =========================================================================

    public class AssessmentDueStatusItemResponse
    {
        public Guid AssessmentId { get; set; }

        public string AssessmentNumber { get; set; } = string.Empty;

        public PatientAssessmentType AssessmentType { get; set; }

        public string AssessmentTypeName { get; set; } = string.Empty;

        public DateTime AssessmentDateTime { get; set; }

        public DateTime? DueAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public AssessmentDueState DueState { get; set; }

        public int? LateByMinutes { get; set; }

        public Guid? PolicyId { get; set; }

        public string? PolicyCode { get; set; }
    }

    /// <summary>Keadaan tenggat seluruh pengkajian pada satu perawatan.</summary>
    public class AssessmentDueStatusResponse
    {
        public Guid EpisodeId { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public DateTime? AdmittedAt { get; set; }

        public bool IsPolicyMasterEmpty { get; set; }

        /// <summary>
        /// Kalimat siap tampil yang membedakan tiga keadaan yang mudah tertukar: belum ada
        /// kebijakan, sudah tepat waktu, dan ada yang terlambat.
        /// </summary>
        public string Explanation { get; set; } = string.Empty;

        public bool HasInitialAssessment { get; set; }

        /// <summary>Tenggat pengkajian awal, walaupun pengkajiannya belum ada sama sekali.</summary>
        public DateTime? InitialAssessmentDueAt { get; set; }

        public AssessmentDueState InitialAssessmentDueState { get; set; }

        public int TotalAssessment { get; set; }

        public int OnTimeCount { get; set; }

        public int PendingCount { get; set; }

        public int LateCount { get; set; }

        public int NotMonitoredCount { get; set; }

        public List<AssessmentDueStatusItemResponse> Items { get; set; } = new();
    }

    // =========================================================================
    // BE-RWI-064 — daftar pantau kepatuhan pengkajian awal
    // =========================================================================

    /// <summary>
    /// Satu baris daftar pantau kepatuhan pengkajian awal.
    /// </summary>
    /// <remarks>
    /// <b>Sengaja tidak memuat satu pun isi klinis.</b> Kepala ruangan memakainya untuk
    /// menemukan pekerjaan yang tertinggal, bukan untuk membaca rekam medis pasien —
    /// <c>FR-KEP-024</c>.
    /// </remarks>
    public class InitialAssessmentComplianceItemResponse
    {
        public Guid EpisodeId { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid ServiceUnitId { get; set; }

        public string ServiceUnitName { get; set; } = string.Empty;

        public string? RoomName { get; set; }

        public string? BedName { get; set; }

        public DateTime? AdmittedAt { get; set; }

        /// <summary>Kosong bila pengkajian awalnya memang belum ada sama sekali.</summary>
        public Guid? AssessmentId { get; set; }

        public string? AssessmentNumber { get; set; }

        public bool HasInitialAssessment { get; set; }

        public DateTime? DueAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public AssessmentDueState DueState { get; set; }

        public int? LateByMinutes { get; set; }
    }

    // =========================================================================
    // BE-RWI-057 — koreksi pengkajian final
    // =========================================================================

    /// <summary>
    /// Permintaan menambah koreksi pada pengkajian yang sudah final.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>BE-RWI-057</c>, <c>RWI-DEC-091</c>. Koreksi <b>tidak</b> menimpa isi pengkajian; ia
    /// tersimpan sebagai addendum bernomor urut pada mesin keutuhan dokumen milik
    /// <c>MedicalRecordManagement</c>, dan status pengkajian tetap <c>Completed</c>.
    /// </para>
    /// <para>
    /// Penulis, waktu, perangkat, dan alamat jaringan <b>tidak</b> diterima dari klien. Bila
    /// boleh dikirim, pembuat koreksi dapat mengaku sebagai orang lain.
    /// </para>
    /// </remarks>
    public class CreateAssessmentAddendumRequest
    {
        /// <summary>Isi koreksinya: apa yang seharusnya tertulis.</summary>
        [Required(ErrorMessage = "Isi koreksi wajib diisi.")]
        [MaxLength(4000, ErrorMessage = "Isi koreksi terlalu panjang. Batasnya 4000 huruf.")]
        public string Content { get; set; } = string.Empty;

        /// <summary>Alasan koreksi. Wajib — <c>VAL-KEP-12</c>.</summary>
        [Required(ErrorMessage = "Alasan perubahan wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Alasan koreksi terlalu panjang. Batasnya 500 huruf.")]
        public string Reason { get; set; } = string.Empty;
    }
}
