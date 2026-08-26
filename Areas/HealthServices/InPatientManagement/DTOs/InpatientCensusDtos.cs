using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>Penyaring census pasien yang sedang dirawat.</summary>
    public class CensusQuery
    {
        /// <summary>Kata kunci nama pasien, nomor rekam medis, nomor episode, atau nama tempat tidur.</summary>
        public string? Search { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? RoomId { get; set; }

        public Guid? PatientClassId { get; set; }

        public Guid? DoctorId { get; set; }

        public bool? RequiresIsolation { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Satu pasien yang sedang dirawat, beserta lokasi, penanggung jawab, dan lama dirawatnya.
    /// </summary>
    /// <remarks>
    /// Census <b>tidak</b> disimpan sebagai tabel. Ia selalu dihitung dari baris penempatan
    /// yang masih aktif, sehingga tidak pernah ada dua versi kebenaran yang perlu disamakan.
    /// Isi klinis — diagnosis, resume, keterangan isolasi — tidak pernah ikut di sini.
    /// </remarks>
    public class CensusItemResponse
    {
        public Guid EpisodeId { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public int EpisodeStatus { get; set; }

        public string EpisodeStatusName { get; set; } = string.Empty;

        public Guid BedId { get; set; }

        public string? BedCode { get; set; }

        public string? BedName { get; set; }

        public Guid RoomId { get; set; }

        public string? RoomName { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public Guid PatientClassId { get; set; }

        public string? PatientClassName { get; set; }

        public string? DoctorName { get; set; }

        public Guid? DoctorId { get; set; }

        public string? NurseName { get; set; }

        public Guid? NurseEmployeeId { get; set; }

        public bool RequiresIsolation { get; set; }

        /// <summary>
        /// Episode ibu, bila baris ini adalah bayi rawat gabung. Kosong untuk sebagian besar
        /// pasien.
        /// </summary>
        /// <remarks>
        /// Census menampilkan ibu dan bayinya sebagai <b>dua baris terpisah</b>. Keduanya
        /// memang dua pasien, dua episode, dan dua tempat tidur — dan hari rawat keduanya
        /// dihitung sendiri-sendiri.
        /// </remarks>
        public Guid? MotherEpisodeId { get; set; }

        public string? MotherEpisodeNumber { get; set; }

        public string? MotherPatientName { get; set; }

        /// <summary>Benar bila tempat tidur yang ditempati adalah boks bayi.</summary>
        public bool IsNewbornBed { get; set; }

        public DateTime? AdmittedAt { get; set; }

        public DateTime PlacementStartDateTime { get; set; }

        /// <summary>
        /// Lama dirawat dalam hari, dihitung dari <b>selisih tanggal</b> dan bernilai paling
        /// sedikit 1. Masuk 21 September 22:30 dan dibaca 22 September 06:00 menghasilkan 1,
        /// bukan 0 — <c>RWI-RULE-019</c>.
        /// </summary>
        public int LengthOfStayDays { get; set; }
    }

    /// <summary>Daftar pasien yang sedang dirawat, bertingkat.</summary>
    public class CensusPagedResult : PagedResult<CensusItemResponse>
    {
    }

    /// <summary>Satu kelompok hitungan pada ringkasan census.</summary>
    public class CensusSummaryGroupResponse
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public int Total { get; set; }
    }

    /// <summary>Ringkasan jumlah pasien dirawat per unit layanan dan per kelas perawatan.</summary>
    public class CensusSummaryResponse
    {
        public int TotalPatient { get; set; }

        public int TotalRequiringIsolation { get; set; }

        public List<CensusSummaryGroupResponse> ByServiceUnit { get; set; } = new();

        public List<CensusSummaryGroupResponse> ByPatientClass { get; set; } = new();
    }

    /// <summary>Nilai bawaan penyaring census.</summary>
    public class CensusDefaultFilterResponse
    {
        public string? Search { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? RoomId { get; set; }

        public Guid? PatientClassId { get; set; }

        public bool? RequiresIsolation { get; set; }

        public string SortBy { get; set; } = "bedName";

        public string SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>Pilihan penyaring census beserta nilai bawaannya.</summary>
    public class CensusFilterMetadataResponse
    {
        public CensusDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<InpatientSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<InpatientOptionResponse> ServiceUnitOptions { get; set; } = new();

        public List<InpatientOptionResponse> PatientClassOptions { get; set; } = new();

        public string ResetButtonLabel { get; set; } = "Reset";
    }
}
