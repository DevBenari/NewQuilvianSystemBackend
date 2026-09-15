using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs
{
    /* ------------------------------------------------------------------ *
     * Permintaan
     * ------------------------------------------------------------------ */

    /// <summary>
    /// Menulis draf bacaan pertama atas satu study.
    /// </summary>
    public class CreateRadReportDraftRequest
    {
        /// <summary>
        /// Peran penulis yang dinyatakan pemanggil.
        ///
        /// <para>
        /// Nilainya <b>tidak dipercaya begitu saja</b>. Menyatakan diri
        /// <see cref="RadReportAuthorRole.Radiologist"/> hanya diterima bila penulisnya benar-benar
        /// memegang hak akses penanda <c>RadReport : ActAsRadiologist</c>; tanpa itu permintaan
        /// ditolak, bukan diturunkan diam-diam. Pernyataan yang lebih rendah dari kewenangan
        /// sebenarnya — misalnya radiolog yang menandai drafnya sebagai hasil bantuan AI —
        /// diterima apa adanya karena hanya membuat aturan pengesahan makin ketat.
        /// </para>
        ///
        /// <para>
        /// Kosong berarti "ikuti kewenangan saya": penulis yang memegang penanda dibekukan
        /// sebagai <see cref="RadReportAuthorRole.Radiologist"/>, sedangkan yang tidak
        /// memegangnya wajib menyebut perannya karena sistem tidak dapat membedakan residen
        /// dari radiografer sendiri.
        /// </para>
        /// </summary>
        public RadReportAuthorRole? AuthorRole { get; set; }

        /// <summary><b>Sensitif.</b> Uraian temuan pada citra. Boleh kosong.</summary>
        public string? Findings { get; set; }

        /// <summary>
        /// <b>Sensitif.</b> Kesimpulan bacaan. Wajib diisi — inilah yang benar-benar dibaca
        /// dokter pengirim.
        /// </summary>
        [Required]
        public string Impression { get; set; } = string.Empty;

        /// <summary><b>Sensitif.</b> Saran tindak lanjut. Boleh kosong.</summary>
        public string? Recommendation { get; set; }
    }

    /// <summary>
    /// Mengubah draf yang belum disahkan. Hanya penulis draf itu sendiri yang boleh memakainya.
    /// </summary>
    /// <remarks>
    /// Peran penulis <b>tidak</b> dapat diubah lewat jalur ini. Ia dibekukan saat draf lahir dan
    /// menjadi dasar aturan pengesahan; membiarkannya disunting sama dengan membiarkan penulis
    /// memilih sendiri siapa yang boleh mengesahkan drafnya.
    /// </remarks>
    public class UpdateRadReportDraftRequest
    {
        /// <summary><b>Sensitif.</b></summary>
        public string? Findings { get; set; }

        /// <summary><b>Sensitif.</b> Tetap wajib terisi setelah diubah.</summary>
        [Required]
        public string Impression { get; set; } = string.Empty;

        /// <summary><b>Sensitif.</b></summary>
        public string? Recommendation { get; set; }
    }

    /// <summary>
    /// Menulis draf koreksi atas bacaan yang sudah dirilis.
    /// </summary>
    /// <remarks>
    /// Isinya ditulis <b>utuh</b>, bukan hanya bagian yang berubah. Koreksi menghasilkan versi
    /// baru yang berdiri sendiri dan harus dapat dibaca tanpa menggabungkannya dengan versi
    /// sebelumnya — pembaca riwayat tidak boleh dipaksa menyusun sendiri bacaan yang berlaku
    /// dari potongan-potongan perubahan.
    /// </remarks>
    public class CreateRadReportAmendmentRequest
    {
        /// <summary>
        /// <b>Wajib diisi.</b> Mengapa bacaan sebelumnya perlu diperbaiki.
        ///
        /// <para>
        /// Tanpa ini, pembaca riwayat melihat dua bacaan berbeda atas satu pemeriksaan tanpa
        /// tahu mana yang keliru dan mengapa — dan itu justru pertanyaan pertama ketika sebuah
        /// keputusan klinis ditinjau ulang. <b>Sensitif</b>, haram masuk application log.
        /// </para>
        /// </summary>
        [Required]
        public string AmendmentReason { get; set; } = string.Empty;

        /// <summary>
        /// Peran penulis koreksi, diperlakukan sama persis seperti pada draf pertama —
        /// <c>FR-RAD-023</c>.
        /// </summary>
        public RadReportAuthorRole? AuthorRole { get; set; }

        /// <summary><b>Sensitif.</b> Uraian temuan versi koreksi ini. Boleh kosong.</summary>
        public string? Findings { get; set; }

        /// <summary><b>Sensitif.</b> Kesimpulan versi koreksi ini. Wajib diisi.</summary>
        [Required]
        public string Impression { get; set; } = string.Empty;

        /// <summary><b>Sensitif.</b> Saran tindak lanjut. Boleh kosong.</summary>
        public string? Recommendation { get; set; }
    }

    /// <summary>
    /// Penyaring, pengurutan, dan halaman untuk daftar bacaan.
    /// </summary>
    public class RadReportPagedQuery
    {
        /// <summary>Dicari pada nomor bacaan dan nomor pemeriksaan.</summary>
        public string? Search { get; set; }

        public Guid? RadStudyId { get; set; }

        public Guid? RadOrderId { get; set; }

        public Guid? EncounterId { get; set; }

        public RadReportStatus? ReportStatus { get; set; }

        /// <summary>
        /// Menyaring bacaan yang belum sampai ke dokter pengirim — <c>Pending</c>,
        /// <c>Drafted</c>, dan <c>Validated</c>. Inilah beban kerja yang masih menggantung.
        /// </summary>
        public bool? BelumDirilis { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /* ------------------------------------------------------------------ *
     * Tanggapan
     * ------------------------------------------------------------------ */

    /// <summary>
    /// Satu versi isi bacaan beserta jejak penulis, pengesah, dan waktunya.
    /// </summary>
    public class RadReportVersionResponse
    {
        public Guid Id { get; set; }

        public int VersionNumber { get; set; }

        /// <summary>Kosong berarti versi pertama.</summary>
        public Guid? PreviousVersionId { get; set; }

        public string VersionStatus { get; set; } = string.Empty;

        public bool IsAmendment { get; set; }

        public string? Findings { get; set; }

        public string Impression { get; set; } = string.Empty;

        public string? Recommendation { get; set; }

        public Guid AuthorUserId { get; set; }

        /// <summary>Peran penulis yang dibekukan saat draf ini ditulis.</summary>
        public string AuthorRoleSnapshot { get; set; } = string.Empty;

        public DateTime DraftedAt { get; set; }

        public Guid? ValidatorUserId { get; set; }

        public DateTime? ValidatedAt { get; set; }

        public DateTime? ReleasedAt { get; set; }

        public string? AmendmentReason { get; set; }
    }

    /// <summary>
    /// Bacaan beserta versi yang sedang berlaku dan seluruh riwayat versinya.
    /// </summary>
    public class RadReportDetailResponse
    {
        public Guid Id { get; set; }

        public Guid RadStudyId { get; set; }

        public Guid RadOrderId { get; set; }

        public Guid EncounterId { get; set; }

        public string ReportNumber { get; set; } = string.Empty;

        public string ReportStatus { get; set; } = string.Empty;

        /// <summary><c>0</c> berarti belum ada draf sama sekali.</summary>
        public int CurrentVersionNumber { get; set; }

        public DateTime? FirstReleasedAt { get; set; }

        public DateTime? LastReleasedAt { get; set; }

        /// <summary>Versi yang sedang berlaku. Kosong selama bacaan masih menunggu draf.</summary>
        public RadReportVersionResponse? CurrentVersion { get; set; }

        /// <summary>
        /// Nomor versi yang <b>sedang dikerjakan</b>.
        ///
        /// <para>
        /// Sama dengan <see cref="CurrentVersionNumber"/> ketika tidak ada koreksi yang sedang
        /// disusun. Selama koreksi berjalan, angka ini lebih besar satu — dan perbedaan itulah
        /// yang memberi tahu layar bahwa <b>ada koreksi yang belum sah</b>, sementara yang boleh
        /// dipakai mengambil keputusan tetap <see cref="CurrentVersion"/>.
        /// </para>
        /// </summary>
        public int WorkingVersionNumber { get; set; }

        /// <summary>Seluruh versi, terbaru lebih dulu. Tidak pernah berkurang.</summary>
        public List<RadReportVersionResponse> Versions { get; set; } = [];

        /// <summary>
        /// Aksi yang masuk akal dijalankan atas bacaan ini menurut keadaannya saat ini.
        ///
        /// <para>
        /// <b>Ini bantuan tampilan, bukan pengaman.</b> Daftar ini diturunkan dari keadaan
        /// bacaan saja — ia tidak tahu siapa yang sedang melihat. Tombol yang muncul di sini
        /// belum tentu boleh ditekan orang yang melihatnya: setiap endpoint aksi tetap memeriksa
        /// ulang kelayakan status <b>dan</b> kewenangan pelakunya di backend. Menyembunyikan
        /// tombol bukan authorization.
        /// </para>
        /// </summary>
        public List<string> AvailableActions { get; set; } = [];
    }

    /// <summary>
    /// Satu baris pada daftar bacaan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Uraian temuan dan saran tindak lanjut sengaja tidak dibawa di sini.</b>
    /// <c>Findings</c> dapat mencapai 8.000 huruf dan <c>Recommendation</c> 2.000 huruf;
    /// keduanya dibuka lewat <c>GET /{id}</c> ketika memang hendak dibaca.
    /// </para>
    /// <para>
    /// <b><c>Impression</c> ikut, tetapi hanya pada pembacaan yang berpusat pada pasien</b> —
    /// <c>GET /by-encounter/{encounterId}</c> dan <c>GET /by-patient/{patientId}</c>. Pada
    /// keduanya pembacanya adalah dokter yang sedang menangani pasien itu, dan kesimpulan bacaan
    /// justru satu-satunya hal yang ia datangi. Memaksanya membuka satu per satu hanya untuk
    /// membaca kesimpulan berarti menyembunyikan hal yang paling dibutuhkan di balik langkah
    /// yang tidak menambah keamanan apa pun.
    /// </para>
    /// <para>
    /// <b>Pada <c>GET /</c> yang berhalaman, <c>Impression</c> tetap kosong.</b> Daftar itu papan
    /// kerja radiologi — sering terbuka lebar di monitor bersama dan hasil pencarian, dan
    /// pembacanya belum tentu sedang menangani pasien yang barisnya kebetulan terlihat.
    /// Pemisahan ini disengaja; lihat <c>MapList</c>.
    /// </para>
    /// </remarks>
    public class RadReportListResponse
    {
        public Guid Id { get; set; }

        public string ReportNumber { get; set; } = string.Empty;

        public Guid RadStudyId { get; set; }

        public string? StudyNumber { get; set; }

        public Guid RadOrderId { get; set; }

        public Guid EncounterId { get; set; }

        public string ReportStatus { get; set; } = string.Empty;

        /// <summary>Teks siap tampil dalam Bahasa Indonesia.</summary>
        public string ReportStatusLabel { get; set; } = string.Empty;

        public int CurrentVersionNumber { get; set; }

        /// <summary>Peran penulis versi yang sedang berlaku, dibekukan saat draf ditulis.</summary>
        public string? AuthorRoleSnapshot { get; set; }

        public Guid? AuthorUserId { get; set; }

        public Guid? ValidatorUserId { get; set; }

        public DateTime? DraftedAt { get; set; }

        public DateTime? FirstReleasedAt { get; set; }

        public DateTime? LastReleasedAt { get; set; }

        /// <summary>
        /// <b>Sensitif.</b> Kesimpulan versi yang <b>sedang berlaku</b>.
        ///
        /// <para>
        /// Terisi hanya pada <c>GET /by-encounter/{encounterId}</c> dan
        /// <c>GET /by-patient/{patientId}</c>; kosong pada daftar berhalaman. Kosong di sini
        /// karena itu <b>tidak berarti bacaannya tanpa kesimpulan</b> — kesimpulan wajib diisi
        /// pada setiap draf — melainkan berarti daftar ini memang tidak membawanya.
        /// </para>
        /// </summary>
        public string? Impression { get; set; }
    }

    /// <summary>
    /// Pilihan penyaring, pengurutan, dan aksi untuk layar daftar bacaan.
    /// </summary>
    public class RadReportFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<RadEnumOptionResponse> ReportStatuses { get; set; } = new();

        public List<RadEnumOptionResponse> VersionStatuses { get; set; } = new();

        /// <summary>
        /// Empat peran penulis. Hanya <c>Radiologist</c> yang boleh mengesahkan drafnya
        /// sendiri — <c>RAD-DEC-003</c>.
        /// </summary>
        public List<RadEnumOptionResponse> AuthorRoles { get; set; } = new();

        public List<RadSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<RadQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        /// <summary>
        /// Perpindahan status yang mungkin, diturunkan dari <c>RAD-STATE-001</c> bagian 3
        /// supaya layar tidak menuliskan ulang aturannya di sisi klien.
        /// </summary>
        public List<RadReportActionInfoResponse> Actions { get; set; } = new();
    }

    /// <summary>Satu perpindahan status beserta asal dan tujuannya.</summary>
    public class RadReportActionInfoResponse
    {
        public string Action { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string FromStatus { get; set; } = string.Empty;
        public string ToStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Rekap jumlah bacaan per keadaan.
    /// </summary>
    public class RadReportSummaryResponse
    {
        public int TotalBacaan { get; set; }

        public int MenungguDraf { get; set; }
        public int Draf { get; set; }
        public int SudahDisahkan { get; set; }
        public int SudahDirilis { get; set; }

        public int KoreksiDraf { get; set; }
        public int KoreksiDisahkan { get; set; }
        public int KoreksiDirilis { get; set; }

        /// <summary>
        /// Bacaan yang belum sampai ke dokter pengirim — menunggu draf, masih draf, atau sudah
        /// disahkan tetapi belum dirilis. <b>Angka inilah yang menunjukkan pasien yang hasilnya
        /// masih ditunggu.</b>
        /// </summary>
        public int BelumDirilis { get; set; }
    }
}
