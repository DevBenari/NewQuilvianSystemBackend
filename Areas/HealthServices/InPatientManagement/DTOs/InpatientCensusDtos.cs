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

        /// <summary>
        /// Menyaring daftar menurut DPJP. <b>Diabaikan seluruhnya</b> bila
        /// <see cref="AssignedToMe"/> bernilai benar — <c>FR-DOK-070</c>.
        /// </summary>
        public Guid? DoctorId { get; set; }

        /// <summary>
        /// Benar bila pemanggil hanya ingin melihat pasien yang ia sendiri punya penugasan
        /// aktif atasnya. Ditambahkan <c>BE-RWI-081</c>.
        /// </summary>
        /// <remarks>
        /// <b>Identitas dokternya tidak pernah diambil dari sini.</b> Ia selalu diturunkan dari
        /// akun yang sedang masuk. Akun dr. Ahmad yang mengirim <c>doctorId</c> milik dr. Rina
        /// bersama <c>assignedToMe=true</c> tetap menerima daftar dr. Ahmad —
        /// <c>RWI-DEC-111</c>. Menerima <c>doctorId</c> di jalur ini akan mengubah penyaring
        /// kenyamanan menjadi lubang baca data pasien.
        ///
        /// <para>
        /// <b>Berlaku untuk seluruh peran penugasan,</b> bukan DPJP saja. Konsulen dan dokter
        /// jaga boleh menulis catatan klinis, sehingga daftar "pasien saya" wajib memuat pasien
        /// yang mereka tangani — <c>INV-INP-13</c>.
        /// </para>
        /// </remarks>
        public bool AssignedToMe { get; set; }

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

        /// <summary>
        /// Peran penugasan <b>pemanggil</b> atas pasien ini: <c>1</c> DPJP, <c>2</c> konsulen,
        /// <c>3</c> dokter jaga. Kosong bila daftar tidak diminta dengan
        /// <c>assignedToMe=true</c>. Ditambahkan <c>BE-RWI-081</c>.
        /// </summary>
        /// <remarks>
        /// Berbeda dari <see cref="DoctorId"/> dan <see cref="DoctorName"/> yang selalu berarti
        /// DPJP episode. Pada baris pasien yang dokter login tangani sebagai konsulen, kolom
        /// DPJP tetap menyebut nama dokter lain — dan itulah yang benar.
        /// </remarks>
        public int? MyAssignmentRole { get; set; }

        /// <summary>
        /// Tujuan penugasan pemanggil atas pasien ini: <c>0</c> penugasan biasa, <c>1</c>
        /// penugasan singkat penulisan catatan terlambat. Kosong bila daftar tidak diminta
        /// dengan <c>assignedToMe=true</c>. Ditambahkan <c>BE-RWI-081</c>.
        /// </summary>
        public int? MyAssignmentPurpose { get; set; }

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
        /// <summary>
        /// Terisi bila daftar kosong karena keadaan yang perlu dijelaskan, bukan karena memang
        /// tidak ada pasien. Ditambahkan <c>BE-RWI-081</c> untuk <c>FR-RI-192</c>.
        /// </summary>
        /// <remarks>
        /// <b>Akun tanpa data dokter menerima <c>200</c> berdaftar kosong, bukan <c>403</c>.</b>
        /// Hak baca census tetap sah; yang tidak ada adalah kaitan akun itu dengan seorang
        /// dokter. Menjawabnya <c>403</c> menyamarkan masalah data induk menjadi masalah
        /// kewenangan, dan petugas yang menghadapinya akan meminta hak akses yang sebenarnya
        /// sudah ia punya.
        /// </remarks>
        public string? EmptyReason { get; set; }
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

        /// <summary>
        /// Jumlah hal yang menunggu tindakan dokter login pada pasien-pasien daftar ini.
        /// Kosong bila ringkasan tidak diminta dengan <c>assignedToMe=true</c>. Ditambahkan
        /// <c>BE-RWI-081</c>.
        /// </summary>
        /// <remarks>
        /// <b>Cakupan saat ini baru sebagian.</b> Angkanya memuat entri CPPT yang menunggu
        /// verifikasi dokter itu. Bagian kedua yang dirancang <c>02-backend-architecture.md</c>
        /// bagian 11.5.6 — pesanan tindakan yang menunggu verifikasi instruksinya — menunggu
        /// <c>PatientProcedureOrderService</c> yang dibuat <c>BE-RWI-097</c> pada sub-modul
        /// <c>dokter-rawat-inap</c>, dan bagian Lab/Radiologi menunggu persetujuan pemiliknya.
        /// Selama itu belum ada, angkanya dihitung dari sumber yang benar-benar tersedia dan
        /// tidak pernah ditebak.
        /// </remarks>
        public int? NeedsReviewCount { get; set; }
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
