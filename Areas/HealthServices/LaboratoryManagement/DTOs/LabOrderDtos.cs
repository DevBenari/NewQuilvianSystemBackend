using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring daftar pesanan laboratorium.
    ///
    /// <b>Mengapa ini ada.</b> Sebelum penyaring ini, <c>GET /lab-orders</c> mengembalikan
    /// seluruh isi tabel tanpa satu pun parameter. Akibatnya modul IGD terpaksa menarik seluruh
    /// pesanan rumah sakit lalu menyaringnya di dalam browser hanya untuk menampilkan pesanan
    /// satu pasien — keterbatasan yang sudah dicatat terbuka pada
    /// <c>emergency-assessment-slice.jsx</c> sebagai <c>IGD-DEC-105</c>, dan perbaikannya
    /// memang milik Laboratorium.
    ///
    /// Seluruh ruas di bawah bersifat opsional. Permintaan tanpa satu pun ruas tetap sah dan
    /// mengembalikan halaman pertama.
    /// </summary>
    public class LabOrderPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        /// <summary>
        /// Menyaring per kunjungan pasien. Inilah ruas yang membuat IGD tidak perlu lagi
        /// menarik seluruh tabel; pesanan pasien lain tidak pernah ikut terkirim.
        /// </summary>
        public Guid? EncounterId { get; set; }

        /// <summary>Menyaring per status operasional pesanan.</summary>
        public LabOrderStatus? OrderStatus { get; set; }

        /// <summary>Menyaring per disiplin: Patologi Klinik, Patologi Anatomi, atau Mikrobiologi.</summary>
        public LabDiscipline? Discipline { get; set; }

        /// <summary>Menyaring pesanan yang dibuat sejak tanggal ini.</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Menyaring pesanan yang dibuat sampai tanggal ini.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Pencarian bebas pada kode dan nama jenis pemeriksaan.</summary>
        public string? Search { get; set; }

        /// <summary>
        /// Kolom pengurutan: <c>createDateTime</c> atau <c>orderStatus</c>. Nilai yang tidak
        /// dikenal dikembalikan ke bawaan, bukan ditolak, supaya layar lama tidak mendadak
        /// gagal hanya karena mengirim nama kolom yang sudah tidak ada.
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>Arah pengurutan: <c>asc</c> atau <c>desc</c>. Bawaannya <c>desc</c>.</summary>
        public string? SortDirection { get; set; }
    }

    /// <summary>
    /// Permintaan memesan <b>beberapa pemeriksaan sekaligus</b>, yang dipecah sistem menjadi
    /// satu pesanan per disiplin (<c>LAB-DEC-055</c>, <c>LAB-DEC-056</c>).
    ///
    /// <b>Kenapa permintaan tersendiri, bukan memperluas <see cref="CreateLabOrderRequest"/>.</b>
    /// Endpoint lama mengembalikan <b>satu</b> pesanan. Membuatnya kadang mengembalikan dua
    /// adalah perubahan bentuk respons bagi pemanggil yang sudah ada — dan `BE-LAB-21` baru saja
    /// menunjukkan berapa mahal harga perubahan diam-diam pada endpoint yang sedang dipakai.
    ///
    /// <b>Disiplin tidak ditanyakan.</b> Ia diturunkan dari penggolongan katalog setiap
    /// pemeriksaan (<c>LAB-DEC-048</c> butir 6); menanyakannya lagi berarti meminta petugas
    /// mengulang jawaban yang sudah ada di data.
    /// </summary>
    public class CreateLabOrderByExaminationsRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pesanan. Diteruskan apa adanya ke setiap pesanan
        /// yang terbentuk, mengikuti <see cref="CreateLabOrderRequest.InpEpisodeId"/>.
        /// </summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Jenis pemeriksaan yang dipilih petugas. Wajib memuat sekurang-kurangnya satu
        /// (<c>VAL-64</c>), dan tidak boleh memuat jenis yang sama dua kali (<c>VAL-65</c>).
        /// </summary>
        public List<Guid> Examinations { get; set; } = new();

        /// <summary>
        /// Bagian dari <see cref="Examinations"/> yang ditandai cito.
        ///
        /// Penanda cito melekat pada pemeriksaan, bukan pada pesanan (<c>LAB-DEC-026</c>) — satu
        /// pesanan boleh memuat cito dan biasa sekaligus. Penunjuk yang tidak ada pada
        /// <see cref="Examinations"/> diabaikan.
        /// </summary>
        public List<Guid> CitoExaminations { get; set; } = new();
    }

    public class CreateLabOrderRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>
        /// Disiplin yang menaungi pesanan — Patologi Klinik, Patologi Anatomi, atau
        /// Mikrobiologi (<c>LAB-DEC-025</c>).
        ///
        /// Sengaja tidak wajib. `LAB-API-v1` r3 mengunci `POST /lab-orders` tetap berlaku apa
        /// adanya, sehingga pemanggil lama yang belum mengirim ruas ini tidak boleh mendadak
        /// ditolak. Mewajibkannya adalah perubahan kontrak tersendiri.
        /// </summary>
        public LabDiscipline? Discipline { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pesanan. Boleh kosong; bila terisi tetapi tidak
        /// cocok dengan perawatan milik kunjungannya, permintaan ditolak <c>400</c>.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-052</c>, <c>VAL-DOK-22</c>, <c>INV-DOK-12</c>. Inilah yang membuat pesanan
        /// perawatan A tidak dapat diproses sebagai milik perawatan B. Tanpa penanda ini, satu
        /// pasien yang dirawat dua kali dalam sebulan memiliki dua rangkaian pesanan yang
        /// bercampur pada layar dokter.
        /// </remarks>
        public Guid? InpEpisodeId { get; set; }
    }

    /// <summary>
    /// Permintaan konfirmasi pesanan laboratorium (<c>LAB-API-v1</c> <c>r12</c> §7.1,
    /// <c>LAB-DEC-061</c>).
    ///
    /// <para>
    /// <b>Hanya satu ruas, dan itu disengaja.</b> Tidak ada ruas konfirmator dan tidak ada ruas
    /// waktu konfirmasi. Keduanya diturunkan server dari pengguna yang sedang login dan dari jam
    /// server: ruas yang dapat dikirim pemanggil adalah ruas yang dapat dipalsukan pemanggil,
    /// sedangkan nama konfirmator adalah pertanyaan audit, bukan pertanyaan tampilan.
    /// Ketiadaan kedua ruas itu adalah bentuk penegakan <c>T-95b</c> — bukan kelalaian.
    /// </para>
    /// </summary>
    public class ConfirmLabOrderRequest
    {
        /// <summary>
        /// Dokter pemeriksa yang dipilih saat konfirmasi. Wajib terisi (<c>VAL-72</c>) serta
        /// wajib ada dan masih aktif (<c>VAL-73</c>).
        ///
        /// <para>
        /// Sengaja bertipe <see cref="Guid"/>, bukan <c>Guid?</c>. Ruas wajib bertipe nullable
        /// akan ditolak model binding sebagai <c>400</c>, sementara matriks validasi menetapkan
        /// <c>422</c> untuk keadaan "dokter pemeriksa belum dipilih". Nilai kosong karena itu
        /// ditangkap di service, supaya kodenya benar-benar <c>422</c>.
        /// </para>
        /// </summary>
        public Guid ExaminerDoctorId { get; set; }
    }

    /// <summary>
    /// Permintaan pembatalan pesanan laboratorium (<c>LAB-API-v1</c> <c>r12</c> §7.2,
    /// <c>LAB-DEC-063</c>).
    ///
    /// <para>
    /// Menggantikan <c>CancelLabSpecimenRequest</c> yang sebelumnya dipakai endpoint pembatalan
    /// pesanan sebagai badan <b>opsional</b>. Sejak <c>r12</c> alasannya <b>wajib</b>
    /// (<c>VAL-74</c>): pesanan yang dibatalkan tanpa alasan tidak dapat ditelusuri kelak, dan
    /// yang bertanya biasanya bukan petugas yang membatalkannya.
    /// </para>
    ///
    /// <para>
    /// <b>Nol kolom baru dibutuhkan.</b> Alasannya disimpan sebagai <c>ReasonNote</c> pada
    /// <c>LabTransitionHistory</c> — jejak audit, tempat yang memang seharusnya — persis seperti
    /// sebelum aturan ini ada.
    /// </para>
    /// </summary>
    public class CancelLabOrderRequest
    {
        /// <summary>
        /// Alasan pembatalan. Wajib terisi dan tidak boleh hanya berisi spasi (<c>VAL-74</c>).
        ///
        /// <para>
        /// Sengaja <b>tanpa</b> atribut <c>[Required]</c>. Ruas wajib beratribut itu ditolak
        /// model binding sebagai <c>400</c>, sementara matriks validasi menetapkan <c>422</c>
        /// untuk alasan yang kosong. Pemeriksaannya karena itu berada di service — sama dengan
        /// alasan <c>ExaminerDoctorId</c> pada <see cref="ConfirmLabOrderRequest"/>.
        /// </para>
        /// </summary>
        [MaxLength(1000)]
        public string? CancelReason { get; set; }
    }

    public class LabOrderListResponse
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Nomor pesanan yang dapat dibaca, dicetak, dan disebut lewat telepon
        /// (<c>LAB-DEC-072</c>, <c>LAB-API-v1</c> <c>r16</c>). Bentuknya <c>LAB-RSMMC-000001</c>.
        ///
        /// Ruas <b>tampil</b>, bukan penunjuk — layar tidak boleh menampilkan GUID, dan daftar
        /// yang hanya membawa penunjuk memaksa layar memanggil endpoint kedua per baris.
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// Nomor yang tercetak pada lembar hasil — <c>26-1129</c> — per disiplin per tahun
        /// (<c>LAB-DEC-117</c>, <c>LAB-API-v1</c> <c>r27</c>).
        ///
        /// <b>Ia berdiri di samping <see cref="OrderNumber"/>, bukan menggantikannya</b>
        /// (<c>AC-180</c>). Kosong pada pesanan yang lahir sebelum kolomnya ada, dan pada
        /// pesanan yang disiplinnya belum diketahui.
        /// </summary>
        public string? LabReportNumber { get; set; }

        public Guid EncounterId { get; set; }

        /// <summary>Perawatan rawat inap yang menaungi pesanan, bila ada.</summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Benar ketika hasil pemeriksaan sudah final dan sah dipakai sebagai dasar keputusan
        /// klinis.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-052</c>, <c>VAL-DOK-30</c>. Hasil yang belum final <b>wajib</b> ditandai
        /// dan tidak boleh disajikan sebagai hasil sah. Hasil basi di layar dokter adalah risiko
        /// keselamatan, bukan masalah tampilan.
        /// </remarks>
        public bool IsResultFinal { get; set; }

        /// <summary>Keterangan singkat ketersediaan hasil, siap ditampilkan apa adanya.</summary>
        public string ResultAvailabilityNote { get; set; } = string.Empty;

        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        /// <summary>
        /// Status operasional pesanan. Bukan status pembayaran — Laboratorium tidak memiliki
        /// status finansial apa pun.
        /// </summary>
        public string OrderStatus { get; set; } = string.Empty;

        public int SpecimenCount { get; set; }

        /// <summary>
        /// Jumlah sampel yang sudah dinyatakan layak, yaitu jumlah komponen pemeriksaan yang
        /// sudah memenuhi milestone kelayakan tagih.
        /// </summary>
        public int AcceptedSpecimenCount { get; set; }

        public bool IsCancel { get; set; }

        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Waktu pesanan dikonfirmasi (<c>LAB-API-v1</c> <c>r13</c>, <c>LAB-DEC-061</c>).
        /// Kosong selama pesanan belum pernah dikonfirmasi.
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }

        /// <summary>
        /// Nama konfirmator, <b>siap ditampilkan</b>.
        ///
        /// <para>
        /// Ada di daftar — bukan hanya di detail — karena kolom Konfirmasi menampilkan nama
        /// orang, dan layar tidak boleh menampilkan penunjuk (<c>no-uuid-display</c>). Daftar
        /// yang hanya membawa penunjuk memaksa layar memanggil endpoint kedua per baris hanya
        /// untuk menerjemahkannya. Pembagian yang sama sudah berlaku untuk
        /// <c>RequestedByName</c> sejak <c>r3</c> — ruas itu tinggal di
        /// <c>LabOrderDetailResponse</c>, sehingga tidak dapat dirujuk sebagai <c>cref</c> dari
        /// kelas dasar ini.
        /// </para>
        /// </summary>
        public string? ConfirmedByName { get; set; }

        /// <summary>
        /// Nama dokter pemeriksa, <b>siap ditampilkan</b> (<c>AC-95</c>). Kosong selama pesanan
        /// belum dikonfirmasi.
        /// </summary>
        public string? ExaminerDoctorName { get; set; }
    }

    /// <summary>
    /// Satu pemeriksaan yang <b>benar-benar dipesan</b> pada sebuah pesanan
    /// (<c>LAB-API-v1</c> <c>r15</c>, <c>BR-47</c>).
    ///
    /// <para>
    /// <b>Kenapa kelas ini ada.</b> <c>LabOrder.ProcedureId</c> hanyalah penunjuk <b>wakil</b> —
    /// satu pesanan hasil <c>POST /lab-orders/by-examinations</c> dapat memuat beberapa
    /// pemeriksaan sedisiplin sekaligus. Sebelum <c>r15</c>, daftar sebenarnya tersimpan pada
    /// <c>LabOrderedProcedure</c> tetapi <b>tidak dikembalikan DTO maupun endpoint mana pun</b>,
    /// sehingga tidak ada konsumen yang dapat mengetahui isi sebuah pesanan.
    /// </para>
    ///
    /// <para>
    /// <b>Nol penunjuk dikirim, dan itu disengaja.</b> Konsumen pertamanya adalah dokumen cetak
    /// — keluaran <b>baca</b> yang tidak melakukan aksi apa pun terhadap baris pemeriksaan.
    /// Mengirim <c>ProcedureId</c> berarti mengirim nilai yang tidak boleh ditampilkan
    /// (<c>no-uuid-display</c>) ke konsumen yang tidak membutuhkannya; alasan yang sama dipakai
    /// <c>r14</c> bagian 9.3. Layar yang kelak benar-benar <i>mengubah</i> baris terpesan
    /// menambahkan penunjuknya lewat amandemen milik layar itu.
    /// </para>
    /// </summary>
    public class LabOrderedProcedureResponse
    {
        /// <summary>Kode jenis pemeriksaan <b>pada saat dipesan</b>.</summary>
        public string? ProcedureCode { get; set; }

        /// <summary>Nama jenis pemeriksaan <b>pada saat dipesan</b>, siap ditampilkan.</summary>
        public string? ProcedureName { get; set; }

        /// <summary>
        /// <c>Routine</c> atau <c>Cito</c>. Kesegeraan melekat pada pemeriksaan, bukan pada
        /// pesanan (<c>LAB-DEC-026</c>) — satu pesanan boleh memuat keduanya sekaligus.
        /// </summary>
        public string Urgency { get; set; } = string.Empty;

        /// <summary>
        /// <c>Ordered</c>, <c>Fulfilled</c>, atau <c>Cancelled</c>.
        ///
        /// Baris yang dibatalkan <b>ikut dikembalikan</b> beserta statusnya, bukan disaring di
        /// sini. Menyaringnya di sumber berarti konsumen tidak dapat membedakan pemeriksaan yang
        /// tidak pernah dipesan dari yang dipesan lalu dibatalkan — dan pada dokumen resmi
        /// keduanya bermakna berbeda.
        /// </summary>
        public string OrderedStatus { get; set; } = string.Empty;
    }

    public class LabOrderDetailResponse : LabOrderListResponse
    {
        /// <summary>
        /// Pemeriksaan yang benar-benar dipesan pada pesanan ini (<c>LAB-API-v1</c> <c>r15</c>).
        ///
        /// <para>
        /// <b>Kosong adalah keadaan sah, bukan galat.</b> Pesanan yang dibuat lewat jalur lama
        /// <c>POST /lab-orders</c> berpemeriksaan tunggal dan nol memiliki baris terpesan —
        /// keadaan yang sudah diakui <c>LabSpecimenService</c> sejak <c>BE-LAB-28</c>, yang
        /// menegakkan <c>VAL-69</c> hanya bila pesanannya memiliki baris itu.
        /// </para>
        ///
        /// <para>
        /// <b>Aturan bagi pembacanya</b>, ditulis di sini supaya tidak ditafsirkan
        /// sendiri-sendiri: bila daftar ini <b>kosong</b>,
        /// <see cref="LabOrderListResponse.ProcedureName"/> <b>adalah</b> isi lengkap pesanan.
        /// Bila <b>terisi</b>, ruas itu hanyalah wakil dan <b>tidak boleh</b> dipakai sebagai
        /// isi pesanan; yang berlaku adalah daftar ini.
        /// </para>
        /// </summary>
        public List<LabOrderedProcedureResponse> OrderedProcedures { get; set; } = new();

        /// <summary>
        /// Penunjuk konfirmator (<c>LAB-API-v1</c> <c>r13</c>).
        ///
        /// Ada di detail, bukan di daftar, karena yang membutuhkannya adalah aksi lanjutan —
        /// dan aksi membutuhkan nilai yang dapat dikirim balik, sedangkan nama bukan nilai yang
        /// dapat dikirim balik.
        /// </summary>
        public Guid? ConfirmedByUserId { get; set; }

        /// <summary>
        /// Penunjuk dokter pemeriksa (<c>LAB-API-v1</c> <c>r13</c>). Dibutuhkan layar yang kelak
        /// mengubah pilihannya; namanya untuk ditampilkan ada pada
        /// <see cref="LabOrderListResponse.ExaminerDoctorName"/>.
        /// </summary>
        public Guid? ExaminerDoctorId { get; set; }

        /// <summary>
        /// Disiplin pesanan (<c>LAB-API-v1</c> r3, <c>LAB-DEC-025</c>). Kosong hanya untuk
        /// pesanan yang dibuat sebelum kolom disiplin ada.
        /// </summary>
        public string? Discipline { get; set; }

        public DateTime? RequestedAt { get; set; }

        /// <summary>
        /// Dokter atau petugas yang meminta pemeriksaan ini.
        ///
        /// <para>
        /// <b>Dibutuhkan layar untuk menegakkan <c>VAL-03</c> sebelum tombolnya ditekan.</b>
        /// Kesegeraan hanya boleh ditandai pemesannya sendiri; tanpa ruas ini layar tidak punya
        /// cara mengetahui siapa pemesannya, sehingga tombol Tandai Cito tampil kepada setiap
        /// dokter dan baru ditolak <c>403</c> sesudah ditekan.
        /// </para>
        /// </summary>
        public Guid? RequestedByUserId { get; set; }

        /// <summary>
        /// Nama pemesan, siap ditampilkan. Penunjuknya sendiri tidak boleh muncul di layar
        /// (<c>no-uuid-display</c>); yang dibaca petugas adalah ruas ini.
        /// </summary>
        public string? RequestedByName { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? StatusBeforeHold { get; set; }

        public int Version { get; set; }

        public DateTime? CancelDateTime { get; set; }

        public Guid? CancelBy { get; set; }
    }
}
