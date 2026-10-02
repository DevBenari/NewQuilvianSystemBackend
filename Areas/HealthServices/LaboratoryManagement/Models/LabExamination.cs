using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu jenis pemeriksaan yang dipesan (<c>LAB-DEC-024</c>, <c>LAB-DEC-026</c>, BR-20).
    ///
    /// <b>Mengapa entity ini ada.</b> Sebelum pemisahan ini, satu baris <see cref="LabSpecimen"/>
    /// merangkap dua peran sekaligus: wadah fisik yang diambil dari pasien, sekaligus jenis
    /// pemeriksaan yang ditagihkan. Kenyataannya keduanya tidak berpasangan satu-satu — satu
    /// tabung darah ungu menopang hemoglobin, leukosit, dan trombosit sekaligus. Selama
    /// keduanya menempel, satu tabung terpaksa dicatat tiga kali, dan pasien menerima tiga
    /// barcode untuk satu kali tusukan jarum (<c>AC-35</c>).
    ///
    /// Entity ini memisahkan peran kedua. <b>Inilah satuan yang ditagihkan</b>, dan kelak
    /// satuan yang memiliki hasil.
    ///
    /// <b>Yang melekat di sini, bukan di pesanan.</b> Kesegeraan dan penanda duplo tinggal pada
    /// baris pemeriksaan (<c>LAB-DEC-026</c>). Satu pesanan boleh memuat pemeriksaan cito dan
    /// biasa sekaligus; memaksanya ke tingkat pesanan akan membuat seluruh isi pesanan ikut
    /// diperlakukan cito dan menenggelamkan yang benar-benar mendesak (<c>AC-40</c>).
    ///
    /// <b>Yang sengaja tidak ada.</b> Tidak satu pun kolom hasil, dan tidak satu pun kolom
    /// finansial. Kolom hasil menunggu slice hasil yang masih tertahan <c>LAB-SIGN-001</c>.
    /// Akibat finansial sepenuhnya milik Billing sesuai <c>RJ-BIL-GATE-DEC-003</c>; salinan
    /// harga di bawah adalah bukti nilai saat kejadian, <b>bukan</b> tagihan.
    /// </summary>
    public class LabExamination : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pesanan induk yang memuat pemeriksaan ini.</summary>
        [Required]
        public Guid LabOrderId { get; set; }

        /// <summary>
        /// Wadah fisik yang menopang pemeriksaan ini. Satu wadah menopang satu atau lebih
        /// pemeriksaan; kelayakan periksa melekat pada wadah, bukan di sini.
        /// </summary>
        [Required]
        public Guid SpecimenId { get; set; }

        /// <summary>Jenis pemeriksaan yang dipesan. Wajib berpenanda <c>IsLaboratory</c>.</summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>Salinan kode jenis pemeriksaan saat kejadian.</summary>
        public string? ProcedureCodeSnapshot { get; set; }

        /// <summary>Salinan nama jenis pemeriksaan saat kejadian.</summary>
        public string? ProcedureNameSnapshot { get; set; }

        /// <summary>
        /// Tarif yang berlaku saat kejadian. Data induknya milik Master Data, dan ditunjuk
        /// tanpa foreign key — mengikuti pola yang sudah dipakai <see cref="LabSpecimen"/>.
        /// </summary>
        public Guid? TariffId { get; set; }

        /// <summary>Salinan kode tarif saat kejadian.</summary>
        public string? TariffCodeSnapshot { get; set; }

        /// <summary>
        /// Salinan harga satuan saat kejadian. <b>Bukan tagihan.</b> Nilainya disimpan supaya
        /// muatan fakta yang dikirim ke Billing dapat direproduksi persis ketika pengiriman
        /// diulang; keputusan menagih tetap milik Billing.
        /// </summary>
        public decimal? UnitPriceSnapshot { get; set; }

        /// <summary>
        /// Keadaan pemeriksaan. Sebagian besar mengikuti wadah penopangnya — lihat
        /// <see cref="LabExaminationStatus"/>.
        /// </summary>
        public LabExaminationStatus ExaminationStatus { get; set; } = LabExaminationStatus.Ordered;

        /// <summary>
        /// Waktu pemeriksaan ini menjadi sah ditagihkan, yaitu saat wadah penopangnya
        /// dinyatakan layak. Kosong selama belum layak.
        /// </summary>
        public DateTime? ChargeEligibleAt { get; set; }

        /// <summary>
        /// Tingkat kesegeraan pemeriksaan ini — biasa atau cito (<c>LAB-DEC-026</c>).
        /// </summary>
        public LabExaminationUrgency Urgency { get; set; } = LabExaminationUrgency.Routine;

        /// <summary>Waktu pemeriksaan ditandai cito atau dikembalikan menjadi biasa.</summary>
        public DateTime? UrgencyMarkedAt { get; set; }

        /// <summary>Dokter yang menandai kesegeraannya.</summary>
        public Guid? UrgencyMarkedByUserId { get; set; }

        /// <summary>
        /// Pemeriksaan dikerjakan ganda (<c>LAB-DEC-026</c>). Dipakai ketika hasil pertama perlu
        /// dikonfirmasi pada pengerjaan yang sama.
        /// </summary>
        public bool IsDuplo { get; set; }

        /// <summary>
        /// Token konkurensi. Menjaga dua permintaan yang datang bersamaan tidak saling menimpa
        /// diam-diam, mengikuti pola <see cref="LabSpecimen.Version"/>.
        /// </summary>
        public int Version { get; set; }

        // =================================================================
        // Pengisian hasil — slice S4a (LAB-DEC-005, LAB-DEC-076)
        //
        // DISIPLIN YANG DIPERTAHANKAN DARI KOMENTAR LabExaminationStatus: kolom-kolom di bawah
        // mencatat APA YANG TERJADI — nilainya, kapan diperiksa, siapa yang mengetik — dan nol
        // MENJANJIKAN apa yang terjadi berikutnya. Tidak ada satu pun status hasil di sini.
        //
        // Validasi, rilis, nilai kritis, dan koreksi tetap tertahan LAB-SIGN-001 (S4), dan
        // ketiga keputusan yang mengaturnya justru yang memberi arti kepada status. Menambahkan
        // statusnya sekarang berarti menjanjikan perilaku yang belum diputuskan pihak klinis.
        //
        // "Hasil sudah diisi" karena itu dibaca dari ResultEnteredAt != null — sebuah fakta,
        // bukan sebuah janji.
        // =================================================================

        /// <summary>
        /// Nilai hasil ketika bentuknya <c>LabResultForm.Numeric</c>.
        ///
        /// Tepat satu dari <see cref="ResultNumeric"/> dan <see cref="ResultOptionId"/> terisi;
        /// bentuknya ditentukan <c>LabValueBound.ResultForm</c> pemeriksaan ini.
        /// </summary>
        public decimal? ResultNumeric { get; set; }

        /// <summary>
        /// Pilihan hasil ketika bentuknya <c>LabResultForm.Choice</c> — menunjuk
        /// <c>LabValueOption</c>.
        ///
        /// Pengetikan bebas tidak diterima pada bentuk ini (<c>AC-28</c>), sehingga yang
        /// disimpan penunjuk pilihan yang sah, bukan teks.
        /// </summary>
        public Guid? ResultOptionId { get; set; }

        /// <summary>
        /// Batas nilai yang <b>berlaku saat hasil diisi</b>, disimpan sebagai penunjuk.
        ///
        /// <b>Tanpa ini, hasil lama berubah artinya ketika batasnya diperbarui.</b> Kalium 3,4
        /// yang hari ini di bawah normal dapat menjadi normal besok bila batasnya digeser — dan
        /// perubahan itu akan berlaku surut pada hasil yang sudah tercetak.
        /// </summary>
        public Guid? ResultValueBoundId { get; set; }

        /// <summary>
        /// Satuan sebagaimana berlaku saat hasil diisi, disalin dari batas nilainya.
        ///
        /// Snapshot, bukan penunjuk, dengan alasan yang sama seperti
        /// <c>ProcedureNameSnapshot</c>: dokumen yang sudah terjadi tidak boleh berubah karena
        /// data induknya diperbarui.
        /// </summary>
        [MaxLength(32)]
        public string? ResultUnitSnapshot { get; set; }

        /// <summary>
        /// <b>Kapan pemeriksaannya dikerjakan</b> — bukan kapan hasilnya diketik.
        ///
        /// Keduanya berbeda dan bedanya bermakna: analis dapat mengerjakan pemeriksaan pukul
        /// 21.10 lalu mengetiknya pukul 08.05 keesokan harinya, persis alasan
        /// <c>LAB-DEC-042</c> memisahkan waktu tiba dari waktu pencatatan pada wadah.
        ///
        /// Inilah kolom yang dituntut pilihan <b>Tanggal Pemeriksaan</b> pada Kategori Periode
        /// (<c>REC3-NEW-002</c>), dan yang ketiadaannya menahannya sampai hari ini.
        /// </summary>
        public DateTime? ExaminedAt { get; set; }

        /// <summary>Kapan hasilnya diketik. Diturunkan server, bukan dikirim pemanggil.</summary>
        public DateTime? ResultEnteredAt { get; set; }

        /// <summary>
        /// Siapa yang mengetik hasilnya.
        ///
        /// <b>Nullable, dan sengaja TANPA foreign key</b> — mengikuti
        /// <see cref="UrgencyMarkedByUserId"/> pada entity yang sama, yang juga nol ber-FK.
        /// Diverifikasi terhadap database 2026-09-17: <c>LabExamination</c> hanya punya tiga
        /// foreign key, dan tidak satu pun menunjuk pengguna.
        ///
        /// Penulisnya tetap wajib mengisi <c>null</c>, bukan <see cref="System.Guid.Empty"/>,
        /// ketika pelakunya bukan orang. Di sini ia tidak akan ditolak database — justru itu
        /// yang membuatnya lebih berbahaya daripada kasus <c>BE-EXT-05</c>: <c>Guid.Empty</c>
        /// akan tersimpan diam-diam sebagai pelaku yang tidak pernah ada, dan nol galat muncul.
        /// </summary>
        public Guid? ResultEnteredByUserId { get; set; }

        // =================================================================
        // Hasil Mikrobiologi berstruktur — slice S4b
        // (LAB-DEC-095, LAB-DEC-097, LAB-DEC-106, LAB-DEC-113, LAB-DEC-114, LAB-DEC-124)
        //
        // DISIPLIN YANG SAMA TETAP BERLAKU: kolom-kolom di bawah mencatat APA YANG TERJADI.
        // FinalizedAt mencatat bahwa seseorang menekan Simpan Final — sebuah fakta. Ia BUKAN
        // status, dan ia BUKAN rilis (LAB-DEC-097).
        //
        // Validasi dan rilis Mikrobiologi adalah S4d, dan S4d tertahan DEC-LAB-011. Nol kolom
        // ValidatedAt maupun ReleasedAt ditambahkan di sini.
        //
        // Isolat dan baris kepekaan antibiotik TIDAK tinggal di sini — keduanya tabel
        // tersendiri pada task BE-LAB-47, dan keduanya melekat pada pemeriksaan ini.
        // =================================================================

        /// <summary>
        /// Status temuan Mikrobiologi — Normal, Positif, atau Negatif (<c>LAB-DEC-113</c>).
        ///
        /// <b>Negatif adalah hasil yang sah</b>, bukan hasil kosong: biakan yang tidak
        /// menumbuhkan apa pun menyingkirkan dugaan infeksi bakteri, dan itu temuan yang
        /// berguna. Pemeriksaan berbentuk ini boleh punya <b>nol</b> isolat (<c>AC-164</c>).
        ///
        /// Terisi hanya ketika bentuk hasilnya <c>LabResultForm.MicrobiologyStructured</c>.
        /// </summary>
        public LabMicrobiologyFinding? MicrobiologyFinding { get; set; }

        /// <summary>
        /// Kualifikasi hasil yang <b>dicetak</b> pada baris <c>HASIL YANG DIPEROLEH</c>
        /// (<c>LAB-DEC-114</c>).
        ///
        /// <b>Nilai tersimpan, bukan kesimpulan.</b> Ia sengaja tidak diturunkan dari
        /// <see cref="ConsultedAt"/>: menurunkannya berarti menyimpulkan bahwa
        /// <i>dikonsultasikan</i> sama dengan <i>definitif</i>, dan bukti nol menyatakan itu.
        /// </summary>
        public LabResultQualifier? ResultQualifier { get; set; }

        /// <summary>
        /// Jenis biakan — menentukan <b>kata</b> pada label cetak (<c>LAB-DEC-124</c>).
        ///
        /// Bebas dari <see cref="SusceptibilityMethod"/>: kultur jamur pun dapat diuji dengan
        /// difusi cakram.
        /// </summary>
        public LabCultureType? CultureType { get; set; }

        /// <summary>
        /// Metode uji kepekaan — menentukan <b>bentuk tabel</b> dan kolom mana yang berlaku
        /// (<c>LAB-DEC-124</c>).
        /// </summary>
        public LabSusceptibilityMethod? SusceptibilityMethod { get; set; }

        /// <summary>
        /// <b>Kapan penulis menyatakan selesai menulis hasil</b> — dan ini BUKAN rilis
        /// (<c>LAB-DEC-097</c>, menyalin pola <c>LAB-DEC-088</c> pada Patologi Anatomi).
        ///
        /// <c>Draft</c> berarti kolom ini masih kosong. Hasil yang sudah Final <b>tetap belum
        /// boleh dikirim ke pasien</b>: pengiriman menunggu rilis, dan rilis Mikrobiologi
        /// adalah <c>S4d</c> yang tertahan <c>DEC-LAB-011</c>.
        ///
        /// <b>Kenapa pembedaan ini dijaga keras.</b> Bila Final diartikan rilis, orang yang
        /// mengisi hasil sekaligus yang mengesahkannya — dan itu melanggar prinsip empat mata
        /// <c>LAB-DEC-003</c> yang ditandatangani <c>DR-LAB-002</c> pada 2026-09-17. Bukti
        /// lapangan menguatkannya: cetakan Patologi Klinik memuat <b>dua baris terpisah</b>,
        /// <c>Otorisasi oleh</c> dan <c>Validasi oleh</c> (<c>LAB-EVD-005</c>).
        /// </summary>
        public DateTime? FinalizedAt { get; set; }

        /// <summary>
        /// Siapa yang menyatakan selesai menulis.
        ///
        /// <b>Nullable dan sengaja TANPA foreign key</b>, mengikuti
        /// <see cref="ResultEnteredByUserId"/> pada entity yang sama. Penulisnya tetap wajib
        /// mengisi <c>null</c>, bukan <see cref="System.Guid.Empty"/>.
        /// </summary>
        public Guid? FinalizedByUserId { get; set; }

        /// <summary>
        /// Berapa kali penulisan dibuka kembali sebelum rilis (<c>LAB-DEC-097</c>).
        ///
        /// <b>Reopen sebelum rilis adalah penyuntingan biasa</b>, bukan koreksi hasil terrilis
        /// — ia <b>tidak</b> menyentuh <c>S6</c> maupun <c>DEC-LAB-014</c>.
        ///
        /// Berdefault <c>0</c> dan <b>bukan</b> nullable: tabel ini sudah berisi data, dan
        /// kolom hitung yang kosong tidak dapat dibedakan dari nol kali dibuka.
        /// </summary>
        public int ReopenCount { get; set; }

        /// <summary>
        /// Siapa yang mengonsultasikan hasil ini — penanda <c>Definitif</c> sebagai
        /// <b>fakta</b>, bukan status dan bukan izin (<c>LAB-DEC-106</c>).
        ///
        /// Mencatat konsultasi <b>nol</b> membuka pengiriman hasil kepada siapa pun. Gerbang
        /// pengirimannya tetap <c>LAB-DEC-067</c>.
        /// </summary>
        public Guid? ConsultedByUserId { get; set; }

        /// <summary>
        /// Kepada siapa hasil ini dikonsultasikan.
        ///
        /// Disimpan sebagai teks, bukan penunjuk: konsultannya sering berada di luar daftar
        /// pengguna sistem — bukti <c>LAB-EVD-005</c> menampilkan seorang Profesor sebagai
        /// Konsultan Mikrobiologi Klinik yang <b>bukan</b> pemegang wewenang klinis terdaftar.
        /// </summary>
        [MaxLength(200)]
        public string? ConsultedToName { get; set; }

        /// <summary>Kapan konsultasinya terjadi. Tidak boleh berada di masa depan (<c>VAL-108</c>).</summary>
        public DateTime? ConsultedAt { get; set; }

        public LabValueOption? ResultOption { get; set; }

        public LabValueBound? ResultValueBound { get; set; }

        public LabOrder? LabOrder { get; set; }

        public LabSpecimen? Specimen { get; set; }

        public MstProcedure? Procedure { get; set; }
    }
}
