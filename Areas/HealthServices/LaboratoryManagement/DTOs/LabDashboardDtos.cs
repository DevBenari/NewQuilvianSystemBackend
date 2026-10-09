namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Kartu <i>hari ini</i> dan <i>fokus operasional</i> Beranda Laboratorium
    /// (<c>LAB-API-v1</c> <c>r44</c> 39.3, BR-140).
    ///
    /// <b>Hanya angka.</b> Tidak satu pun ruas membawa identitas pasien, hasil, maupun harga —
    /// karena itu jawaban ini boleh terbaca setiap pemegang <c>LabOrder : Read</c>
    /// (<c>LAB-DEC-211</c>).
    /// </summary>
    public class LabDashboardTodayResponse
    {
        /// <summary>Tanggal WIB yang dihitung sebagai <i>hari ini</i>, berbentuk <c>YYYY-MM-DD</c>.</summary>
        public string OperationalDate { get; set; } = string.Empty;

        /// <summary>Waktu server menghitung (UTC) — dipakai layar sebagai "data dimuat pukul …".</summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>Pesanan yang diminta hari ini, <b>termasuk</b> yang sudah dibatalkan.</summary>
        public int TodayOrderCount { get; set; }

        /// <summary>Pesanan hari ini yang belum <c>Completed</c> dan tidak <c>Cancelled</c> (<c>LAB-DEC-205</c>).</summary>
        public int WaitingOrderCount { get; set; }

        public int CompletedOrderCount { get; set; }

        /// <summary>
        /// Persentase selesai, bilangan bulat 0–100, pembulatan setengah ke atas; <c>0</c> bila belum
        /// ada pesanan. Satu angka untuk kartu <i>Selesai</i> dan <i>Tingkat penyelesaian</i>
        /// (<c>LAB-REQ-021</c> butir 6) — layar tidak menghitungnya ulang.
        /// </summary>
        public int CompletionPercent { get; set; }

        /// <summary>Pesanan hari ini yang memuat permintaan atau pemeriksaan cito (<c>r44</c> 39.6).</summary>
        public int CitoOrderCount { get; set; }

        /// <summary>Seluruh pesanan sepanjang waktu, termasuk yang dibatalkan (<c>LAB-REQ-021</c> butir 5).</summary>
        public int TotalRecordedOrderCount { get; set; }

        /// <summary>
        /// Isi Antrean Validasi tahap <i>Menunggu Validasi</i> tanpa penyaring (<c>r44</c> 39.7) —
        /// dihitung dengan kueri antrean itu sendiri, bukan salinannya.
        /// </summary>
        public int AwaitingValidationCount { get; set; }
    }

    /// <summary>
    /// Satu baris tabel <i>Pesanan terbaru</i> Beranda (<c>r44</c> 39.5, <c>LAB-DEC-209</c>).
    ///
    /// Nama pasien dan No. RM setara Daftar Pasien Lab; NIK, tanggal lahir, alamat, telepon, hasil,
    /// dan harga sengaja tidak ada.
    /// </summary>
    public class LabDashboardRecentOrderResponse
    {
        public Guid LabOrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string? MedicalRecordNumber { get; set; }

        public string? PatientName { get; set; }

        /// <summary>
        /// Nama permintaan pemeriksaan yang tidak dibatalkan, urut waktu dibuat. Pesanan lama tanpa
        /// baris permintaan memakai nama pemeriksaan utama pesanan.
        /// </summary>
        public List<string> ProcedureNames { get; set; } = new();

        /// <summary>Nama enum disiplin pesanan; kosong bila belum tergolong.</summary>
        public string? Discipline { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        /// <summary>Waktu diminta — <c>RequestedAt</c>, jatuh ke waktu baris dibuat bagi pesanan lama.</summary>
        public DateTime RequestedAt { get; set; }
    }

    /// <summary>
    /// Penyaring ringkasan tahunan Beranda (<c>r44</c> 39.4). Satu pemilih tahun berlaku bagi ketiga
    /// grafik (<c>LAB-DEC-206</c>).
    /// </summary>
    public class LabDashboardYearlyQuery
    {
        /// <summary>Kosong = tahun berjalan WIB. Di luar 2000 sampai tahun berjalan → <c>VAL-154</c>.</summary>
        public int? Year { get; set; }
    }

    /// <summary>
    /// Grafik pemeriksaan per disiplin, sebaran, kartu <i>Jenis laboratorium</i>, dan tren pesanan
    /// bulanan (<c>r44</c> 39.4). Hanya angka.
    /// </summary>
    public class LabDashboardYearlyResponse
    {
        public int Year { get; set; }

        public int PreviousYear { get; set; }

        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// <b>Selalu tiga butir</b> dengan urutan tetap — Patologi Klinik, Patologi Anatomi,
        /// Mikrobiologi — termasuk yang bernilai nol, supaya layar tidak menebak disiplin yang hilang.
        /// </summary>
        public List<LabDashboardDisciplineCountResponse> Disciplines { get; set; } = new();

        /// <summary>Pemeriksaan yang disiplin pesanan dan katalognya sama-sama kosong.</summary>
        public int UnclassifiedExaminationCount { get; set; }

        /// <summary>Banyaknya dari tiga disiplin yang punya pemeriksaan (0–3).</summary>
        public int ActiveDisciplineCount { get; set; }

        /// <summary><b>Selalu dua belas butir</b>, bulan 1 sampai 12; bulan tanpa pesanan bernilai nol.</summary>
        public List<LabDashboardMonthlyOrderResponse> MonthlyOrders { get; set; } = new();
    }

    public class LabDashboardDisciplineCountResponse
    {
        /// <summary>Nama enum <c>LabDiscipline</c>.</summary>
        public string Discipline { get; set; } = string.Empty;

        /// <summary>Pemeriksaan tidak batal pada pesanan tidak batal yang diminta di tahun terpilih (<c>r44</c> 39.8).</summary>
        public int ExaminationCount { get; set; }
    }

    public class LabDashboardMonthlyOrderResponse
    {
        /// <summary>Bulan WIB, 1 sampai 12.</summary>
        public int Month { get; set; }

        /// <summary>Pesanan bukan <c>Cancelled</c>/<c>Draft</c> yang diminta pada bulan ini di tahun terpilih.</summary>
        public int OrderCount { get; set; }

        /// <summary>Sama, pada bulan yang sama di tahun sebelumnya.</summary>
        public int PreviousYearOrderCount { get; set; }
    }
}
