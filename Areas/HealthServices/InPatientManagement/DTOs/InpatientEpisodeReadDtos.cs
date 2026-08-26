using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Penyaring daftar episode. Seluruh kolomnya boleh dikosongkan; yang kosong tidak
    /// menyaring apa pun.
    /// </summary>
    public class InpatientEpisodeListQuery
    {
        /// <summary>Kata kunci nama pasien, nomor rekam medis, atau nomor episode.</summary>
        public string? Search { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? PatientClassId { get; set; }

        public Guid? PatientId { get; set; }

        public int? EpisodeStatus { get; set; }

        /// <summary>Batas bawah rentang tanggal, dibandingkan terhadap waktu admisi dibuka.</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Batas atas rentang tanggal, inklusif sampai akhir hari.</summary>
        public DateTime? EndDate { get; set; }

        public bool? RequiresIsolation { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Satu baris pada daftar episode.
    /// </summary>
    /// <remarks>
    /// <b>Kolom sensitif sengaja tidak ada di sini.</b> Permission matrix bagian 5.4 menandai
    /// <c>InpEpisode.Notes</c> dan <c>InpEpisode.IsolationNote</c> sebagai sensitif, dan
    /// keduanya hanya boleh muncul pada layar detail. Daftar episode dibaca setiap peran yang
    /// punya <c>InpatientEpisode : Read</c> — termasuk kasir dan petugas administrasi — dan
    /// mereka tidak perlu membaca alasan klinis kebutuhan isolasi seorang pasien.
    ///
    /// <para>
    /// <b>Lokasi terkini dibaca dari catatan penempatan.</b> Tidak ada kolom lokasi terakhir
    /// pada <c>InpEpisode</c>, dan tidak boleh ditambahkan walaupun query-nya lebih murah.
    /// Itu larangan arsitektur yang ditulis pada roadmap <c>BE-RWI-009</c> bagian risiko.
    /// </para>
    /// </remarks>
    public class InpatientEpisodeListItemResponse
    {
        public Guid Id { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public Guid PatientClassId { get; set; }

        public string? PatientClassName { get; set; }

        public int EpisodeStatus { get; set; }

        public string EpisodeStatusName { get; set; } = string.Empty;

        public DateTime? AdmittedAt { get; set; }

        public DateTime? DischargeDecidedAt { get; set; }

        public DateTime? PhysicallyLeftAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// Benar bila pasien membutuhkan isolasi. Nilai benar/salahnya boleh tampil pada
        /// daftar; yang tidak boleh tampil adalah <c>IsolationNote</c> yang memuat alasan
        /// klinisnya.
        /// </summary>
        public bool RequiresIsolation { get; set; }

        public string? ActiveDoctorName { get; set; }

        public string? ActiveNurseName { get; set; }

        /// <summary>Nama tempat tidur yang sedang ditempati, dibaca dari <c>InpBedPlacement</c>.</summary>
        public string? CurrentBedName { get; set; }

        public string? CurrentRoomName { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>
    /// Daftar episode bertingkat. Turunan <see cref="PagedResult{T}"/> supaya bentuk
    /// pagination-nya sama persis dengan seluruh endpoint daftar lain di repository ini.
    /// </summary>
    public class InpatientEpisodePagedResult : PagedResult<InpatientEpisodeListItemResponse>
    {
    }

    /// <summary>Jumlah episode pada satu status.</summary>
    public class InpatientEpisodeStatusCountResponse
    {
        public int EpisodeStatus { get; set; }

        public string EpisodeStatusName { get; set; } = string.Empty;

        public int Total { get; set; }
    }

    /// <summary>
    /// Ringkasan jumlah episode per status, memakai penyaring yang sama dengan daftar.
    /// </summary>
    /// <remarks>
    /// Kelima status selalu muncul, termasuk yang jumlahnya nol. Layar yang menampilkan kartu
    /// hitungan karena itu tidak perlu menebak status mana yang hilang dari jawaban.
    /// </remarks>
    public class InpatientEpisodeSummaryResponse
    {
        public int TotalAll { get; set; }

        public List<InpatientEpisodeStatusCountResponse> ByStatus { get; set; } = new();
    }

    /// <summary>Nilai bawaan penyaring daftar episode.</summary>
    public class InpatientEpisodeDefaultFilterResponse
    {
        public string? Search { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? PatientClassId { get; set; }

        public int? EpisodeStatus { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? RequiresIsolation { get; set; }

        public string SortBy { get; set; } = "createDateTime";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Pilihan penyaring beserta nilai bawaannya untuk layar daftar episode.
    /// </summary>
    public class InpatientEpisodeFilterMetadataResponse
    {
        public InpatientEpisodeDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<InpatientSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<InpatientOptionResponse> EpisodeStatusOptions { get; set; } = new();

        /// <summary>Unit layanan bertipe rawat inap yang aktif saja.</summary>
        public List<InpatientOptionResponse> ServiceUnitOptions { get; set; } = new();

        /// <summary>Kelas perawatan yang berlaku untuk rawat inap saja.</summary>
        public List<InpatientOptionResponse> PatientClassOptions { get; set; } = new();

        public string ResetButtonLabel { get; set; } = "Reset";
    }

    /// <summary>
    /// Lokasi pasien saat ini, dibaca dari baris penempatan yang masih aktif.
    /// </summary>
    /// <remarks>
    /// Bernilai <c>null</c> untuk episode yang belum ditempatkan, episode yang sudah ditutup,
    /// dan episode <c>DischargePending</c> yang kepergian fisiknya sudah dicatat. Ketiganya
    /// memang tidak sedang memegang tempat tidur.
    /// </remarks>
    public class InpatientEpisodeCurrentLocationResponse
    {
        public Guid PlacementId { get; set; }

        public Guid BedId { get; set; }

        public string? BedCode { get; set; }

        public string? BedName { get; set; }

        public Guid RoomId { get; set; }

        public string? RoomName { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public Guid PatientClassId { get; set; }

        public string? PatientClassName { get; set; }

        public DateTime StartDateTime { get; set; }
    }

    /// <summary>Perawat penanggung jawab yang sedang berlaku pada satu episode.</summary>
    public class InpatientEpisodeActiveNurseResponse
    {
        public Guid AssignmentId { get; set; }

        public Guid EmployeeId { get; set; }

        public string? EmployeeName { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime StartDateTime { get; set; }
    }
}
