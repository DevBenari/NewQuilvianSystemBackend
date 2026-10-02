using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    // =====================================================================
    // Permintaan
    // =====================================================================

    /// <summary>Mencatat pengambilan sampel dan membuka satu pemeriksaan baru.</summary>
    /// <remarks>
    /// <b>Tidak memuat pengambil dan tidak memuat pemeriksa.</b> Keduanya diturunkan dari
    /// pengguna terautentikasi, supaya tidak ada sampel yang mengaku diambil orang lain.
    /// </remarks>
    public class RecordSampleRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>
        /// Identifier sampel yang tertulis pada tabung. <b>Ditulis petugas</b>, unik di seluruh
        /// tabel — lihat catatan pada
        /// <see cref="Models.BbkBloodGroupSample.SampleIdentifier"/>.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SampleIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Waktu pengambilan. Kosongkan untuk memakai waktu server saat permintaan diterima.
        /// </summary>
        /// <remarks>
        /// Boleh diisi mundur karena sampel kerap dicatat setelah tangannya selesai bekerja,
        /// tetapi <b>tidak boleh</b> diisi maju — sampel yang belum diambil tidak dapat dicatat
        /// sudah diambil.
        /// </remarks>
        public DateTime? TakenAt { get; set; }
    }

    /// <summary>Mencatat hasil ABO dan Rhesus pada pemeriksaan yang sampelnya sudah diambil.</summary>
    public class RecordResultRequest
    {
        /// <summary>
        /// Hasil ABO dan Rhesus. <c>Unknown</c> dan <c>NotDisclosed</c> ditolak — keduanya
        /// menyatakan ketiadaan hasil, bukan hasil pemeriksaan.
        /// </summary>
        [Required]
        public BloodType? AboRhesusResult { get; set; }
    }

    /// <summary>
    /// Menutup keadaan konflik golongan darah dengan menunjuk pemeriksaan ulang tervalidasi.
    /// </summary>
    /// <remarks>
    /// <b>Sengaja tidak punya mode otomatis.</b> Tidak ada field pemilih mayoritas, tidak ada
    /// field pemakai hasil terbaru, dan tidak ada jalan menutup konflik tanpa menyebut satu
    /// pemeriksaan (<c>AC-BD-054</c>, <c>INV-BD-022</c>). Ketiadaan itu bagian dari kontrak,
    /// bukan kelalaian.
    /// </remarks>
    public class ResolveConflictRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>Pemeriksaan ulang tervalidasi yang dinyatakan berlaku (<c>DEC-BD-031</c>).</summary>
        [Required]
        public Guid ResolvingExamId { get; set; }

        /// <summary>Kode alasan terkendali dari <c>MstBloodBankReason</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string ReasonCode { get; set; } = string.Empty;
    }

    // =====================================================================
    // Balasan
    // =====================================================================

    /// <summary>Satu baris pada daftar pemeriksaan golongan darah.</summary>
    public class BloodGroupExamListDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public BloodType? AboRhesusResult { get; set; }
        public string? AboRhesusResultLabel { get; set; }
        public BbkBloodGroupExamStatus ExamStatus { get; set; }
        public string ExamStatusLabel { get; set; } = string.Empty;
        public bool IsValidResult { get; set; }
        public bool IsConflictHeld { get; set; }
        public string? SampleIdentifier { get; set; }
        public DateTime? TakenAt { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>Detail satu pemeriksaan, termasuk sampel dan aksi yang tersedia.</summary>
    public class BloodGroupExamDetailDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public BloodType? AboRhesusResult { get; set; }
        public string? AboRhesusResultLabel { get; set; }
        public BbkBloodGroupExamStatus ExamStatus { get; set; }
        public string ExamStatusLabel { get; set; } = string.Empty;

        public Guid? ExaminedByUserId { get; set; }
        public DateTime? ExaminedAt { get; set; }
        public Guid? ValidatedByUserId { get; set; }
        public DateTime? ValidatedAt { get; set; }

        public bool IsValidResult { get; set; }
        public bool IsConflictHeld { get; set; }
        public int Version { get; set; }

        public List<BloodGroupSampleDto> Samples { get; set; } = new();

        /// <summary>
        /// Aksi yang boleh dicoba pada keadaan sekarang, dinilai backend.
        /// </summary>
        /// <remarks>
        /// Daftar ini menjawab <b>kelayakan status</b> saja — apakah tindakannya masuk akal bagi
        /// pemeriksaan pada keadaan ini. Ia <b>tidak</b> menjawab kewenangan; itu tetap dijaga
        /// butir hak akses pada endpoint masing-masing. Layar yang menyembunyikan tombol
        /// berdasarkan daftar ini tetap tidak boleh dianggap sebagai penjaga.
        /// </remarks>
        public List<string> AvailableActions { get; set; } = new();

        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class BloodGroupSampleDto
    {
        public Guid Id { get; set; }
        public string SampleIdentifier { get; set; } = string.Empty;
        public Guid TakenByUserId { get; set; }
        public DateTime TakenAt { get; set; }
    }

    /// <summary>
    /// Jawaban atas satu pertanyaan: apa golongan darah sah pasien ini sekarang, atau apakah
    /// sedang bertentangan (<c>BD-DOM-21</c>).
    /// </summary>
    /// <remarks>
    /// <b>Turunan, bukan kolom.</b> Nilainya dihitung dari kumpulan pemeriksaan milik pasien dan
    /// tidak pernah membaca <c>MstPatient.BloodType</c> (<c>INV-BD-014</c>).
    ///
    /// Ketika <see cref="IsConflictHeld"/> bernilai benar, <see cref="BloodType"/> <b>selalu</b>
    /// kosong. Pemanggil yang membaca golongan darah tanpa memeriksa penanda konflik karena itu
    /// tetap tidak akan mendapat nilai yang salah — paling buruk ia mendapat nilai kosong.
    /// </remarks>
    public class ValidBloodGroupDto
    {
        public Guid PatientId { get; set; }

        /// <summary>Golongan darah sah, atau kosong bila belum ada atau sedang bertentangan.</summary>
        public BloodType? BloodType { get; set; }

        public string? BloodTypeLabel { get; set; }

        /// <summary>Pemeriksaan yang menjadi sumber nilai di atas.</summary>
        public Guid? SourceExamId { get; set; }

        public DateTime? ValidatedAt { get; set; }

        /// <summary>Benar ketika pasien sedang menahan perbedaan hasil (<c>DEC-BD-026</c>).</summary>
        public bool IsConflictHeld { get; set; }

        /// <summary>Pemeriksaan yang menjadi pihak dalam perbedaan yang belum diselesaikan.</summary>
        public List<Guid> ConflictingExamIds { get; set; } = new();

        /// <summary>
        /// Benar ketika golongan darah sah tersedia dan gerbang klinis boleh dilewati. Dihitung
        /// backend supaya layar tidak perlu menyimpulkannya sendiri.
        /// </summary>
        public bool IsUsableForClinicalDecision { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    /// <summary>Angka ringkasan untuk kartu statistik halaman pemeriksaan.</summary>
    public class BloodGroupExamSummaryResponse
    {
        public int TotalExam { get; set; }
        public int SampleTakenExam { get; set; }
        public int ResultRecordedExam { get; set; }
        public int ValidatedExam { get; set; }

        /// <summary>Jumlah pemeriksaan yang menjadi pihak dalam perbedaan yang belum ditutup.</summary>
        public int ConflictHeldExam { get; set; }

        /// <summary>Jumlah pasien yang sedang tidak punya golongan darah sah karena perbedaan.</summary>
        public int PatientWithHeldConflict { get; set; }
    }

    // =====================================================================
    // Metadata penyaring
    // =====================================================================

    public class BloodGroupExamFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        public BloodGroupExamDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BloodGroupExamOptionItemResponse> ExamStatusOptions { get; set; } = new();
        public List<BloodGroupExamOptionItemResponse> BloodTypeOptions { get; set; } = new();
        public List<BloodGroupExamSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class BloodGroupExamDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public BbkBloodGroupExamStatus? ExamStatus { get; set; }
        public bool? IsConflictHeld { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class BloodGroupExamOptionItemResponse
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class BloodGroupExamSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
