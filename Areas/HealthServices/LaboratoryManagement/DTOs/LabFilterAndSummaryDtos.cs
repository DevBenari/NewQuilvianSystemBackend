namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    // =========================================================================
    // Bentuk bersama
    // =========================================================================

    /// <summary>Satu kolom yang boleh dipakai mengurutkan daftar.</summary>
    public class LabSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Satu pilihan yang berasal dari enum.
    ///
    /// <see cref="Value"/> adalah angka yang dikirim balik ke API, <see cref="Name"/> nama
    /// teknisnya, dan <see cref="Label"/> teks siap tampil dalam Bahasa Indonesia.
    /// </summary>
    public class LabEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>Keterangan satu parameter query, supaya layar tahu apa yang boleh dikirim.</summary>
    public class LabQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    // =========================================================================
    // Lab Order
    // =========================================================================

    public class LabOrderFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabEnumOptionResponse> OrderStatuses { get; set; } = new();
        public List<LabEnumOptionResponse> Disciplines { get; set; } = new();
        public List<LabSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        /// <summary>
        /// <c>GET /lab-orders</c> menyaring, mengurutkan, dan mem-paging di sisi server sejak
        /// <c>BE-LAB-18</c>. Sebelumnya ia mengembalikan seluruh isi tabel tanpa satu pun
        /// parameter, sehingga layar yang hanya butuh pesanan satu pasien terpaksa menyaring
        /// sendiri di browser dan ikut menerima pesanan pasien lain (<c>IGD-DEC-105</c>).
        ///
        /// Setiap parameter yang diumumkan pada <c>QueryParameters</c> benar-benar diproses
        /// daftarnya. Metadata yang menjanjikan penyaring yang tidak diproses adalah cacat
        /// kontrak, bukan sekadar dokumentasi yang usang.
        /// </summary>
        public bool SupportsServerSideFiltering { get; set; } = true;

        public bool SupportsServerSidePaging { get; set; } = true;
    }

    public class LabOrderSummaryResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalPesanan { get; set; }

        public int Draft { get; set; }
        public int Diminta { get; set; }
        public int Diterima { get; set; }
        public int SedangDikerjakan { get; set; }
        public int Selesai { get; set; }
        public int Ditahan { get; set; }
        public int PembatalanDiminta { get; set; }
        public int Dibatalkan { get; set; }

        public int PatologiKlinik { get; set; }
        public int PatologiAnatomi { get; set; }
        public int Mikrobiologi { get; set; }

        /// <summary>Pesanan yang disiplinnya belum terisi. Data lama sebelum `BE-LAB-01`.</summary>
        public int TanpaDisiplin { get; set; }
    }

    // =========================================================================
    // Lab Specimen
    // =========================================================================

    public class LabSpecimenFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabEnumOptionResponse> SpecimenStatuses { get; set; } = new();
        public List<LabEnumOptionResponse> RecollectionCauses { get; set; } = new();
        public List<LabSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        /// <summary>
        /// Daftar wadah selalu ber-scope satu pesanan lewat
        /// <c>GET /lab-specimens/by-order/{labOrderId}</c>. Tidak ada daftar global, dan itu
        /// disengaja: wadah tanpa pesanannya tidak berarti apa-apa bagi petugas.
        /// </summary>
        public bool SupportsServerSideFiltering { get; set; } = false;

        public bool SupportsServerSidePaging { get; set; } = false;

        /// <summary>Wadah tidak pernah dihapus; ia ditolak atau dibatalkan, dan jejaknya tetap.</summary>
        public bool IsDeletable { get; set; } = false;
    }

    public class LabSpecimenSummaryResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalWadah { get; set; }

        public int Direncanakan { get; set; }
        public int Diambil { get; set; }
        public int Diterima { get; set; }
        public int DinyatakanLayak { get; set; }
        public int Ditolak { get; set; }
        public int PerluAmbilUlang { get; set; }
        public int Dibatalkan { get; set; }
        public int Ditahan { get; set; }

        /// <summary>
        /// Penolakan yang berakar pada kesalahan internal rumah sakit. Angka inilah yang dibaca
        /// saat menilai apakah biaya pengambilan ulang boleh dibebankan kepada pasien.
        /// </summary>
        public int KesalahanInternalRumahSakit { get; set; }

        public int KondisiPasienAtauSampel { get; set; }
        public int SebabEksternal { get; set; }
    }

    // =========================================================================
    // Lab Value Bound
    // =========================================================================

    public class LabValueBoundFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabEnumOptionResponse> ResultForms { get; set; } = new();
        public List<LabEnumOptionResponse> GenderScopes { get; set; } = new();
        public List<LabSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;
        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary>
        /// Batas nilai tidak pernah dihapus; ia dinonaktifkan lewat
        /// <c>PUT /{id}/deactivate</c>, dan batas aktif terakhir milik sebuah pemeriksaan tidak
        /// dapat dinonaktifkan sama sekali (<c>VAL-30</c>).
        /// </summary>
        public bool IsDeletable { get; set; } = false;

        /// <summary>
        /// PERINGATAN KESELAMATAN. Batas kritis <b>tidak</b> dapat diubah lewat
        /// <c>PUT /{id}</c>; ia hanya berubah lewat pengajuan yang disetujui pihak klinis
        /// (<c>VAL-28</c>, <c>LAB-DEC-023</c>).
        ///
        /// Dinyatakan di metadata supaya layar tidak menyediakan tombol simpan langsung untuk
        /// batas kritis. Menurut <c>LAB-FE-011</c>, menyediakan jalan yang pasti ditolak tetap
        /// pelanggaran — pengguna tidak boleh dibiarkan mengira jalan itu ada.
        /// </summary>
        public bool CriticalBoundRequiresApproval { get; set; } = true;
    }

    public class LabValueBoundSummaryResponse
    {
        public int TotalBatasNilai { get; set; }
        public int Aktif { get; set; }
        public int Nonaktif { get; set; }

        public int BentukAngka { get; set; }
        public int BentukPilihan { get; set; }

        public int TotalPilihanHasil { get; set; }

        /// <summary>Batas nilai yang punya pengajuan perubahan batas kritis belum diputuskan.</summary>
        public int MenungguPersetujuanBatasKritis { get; set; }

        /// <summary>Jenis pemeriksaan berbeda yang sudah punya sekurang-kurangnya satu batas nilai.</summary>
        public int JumlahPemeriksaanBerbeda { get; set; }
    }

    // =========================================================================
    // Lab Critical Bound Approval
    // =========================================================================

    public class LabCriticalBoundApprovalFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabEnumOptionResponse> RequestStatuses { get; set; } = new();
        public List<LabSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = false;
        public bool SupportsServerSidePaging { get; set; } = false;

        /// <summary>
        /// Route grup ini bersarang di bawah satu batas nilai, sehingga metadata maupun rekapnya
        /// selalu ber-scope batas nilai itu — tidak pernah global.
        /// </summary>
        public bool IsScopedToSingleValueBound { get; set; } = true;

        /// <summary>
        /// PERINGATAN KESELAMATAN. Pengaju tidak boleh menyetujui pengajuannya sendiri
        /// (<c>VAL-33</c>). Aturan ini ditegakkan di dalam service, bukan oleh konfigurasi
        /// permission, karena sistem permission yang ada tidak pernah membandingkan pelaku
        /// sebelumnya (<c>CAP-16</c>).
        ///
        /// Dinyatakan di metadata supaya layar menyembunyikan tombol setujui bagi pengajunya.
        /// </summary>
        public bool SelfApprovalForbidden { get; set; } = true;

        /// <summary>Hanya satu pengajuan yang belum diputuskan boleh berdiri per batas nilai (<c>VAL-32</c>).</summary>
        public bool SinglePendingRequestOnly { get; set; } = true;
    }

    public class LabCriticalBoundApprovalSummaryResponse
    {
        /// <summary>Batas nilai yang menjadi lingkup rekap ini.</summary>
        public Guid ValueBoundId { get; set; }

        public int TotalPengajuan { get; set; }

        public int Diajukan { get; set; }
        public int Disetujui { get; set; }
        public int Ditolak { get; set; }
        public int Ditarik { get; set; }

        /// <summary>
        /// Ada pengajuan yang belum diputuskan. Selama bernilai benar, batas kritis batas nilai
        /// ini sedang menunggu keputusan pihak klinis dan tidak dapat diajukan ulang.
        /// </summary>
        public bool AdaPengajuanBelumDiputuskan { get; set; }
    }

    // =========================================================================
    // Lab Rejection Reason
    // =========================================================================

    public class LabRejectionReasonFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;
        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary>
        /// Alasan penolakan tidak pernah dihapus; ia dinonaktifkan lewat
        /// <c>PUT /{id}/activation</c>. Alasan yang pernah dipakai menempel pada riwayat
        /// penolakan sampel, dan alasan aktif terakhir tidak dapat dinonaktifkan
        /// (<c>VAL-38</c>).
        /// </summary>
        public bool IsDeletable { get; set; } = false;

        /// <summary>
        /// PERINGATAN KEWENANGAN. Penanda kesalahan internal dan penanda wajib catatan hanya
        /// dapat disetel pemegang <c>LabRejectionReason : SystemFlag</c> lewat
        /// <c>PUT /{id}/system-flags</c>; upaya mengubahnya lewat <c>PUT /{id}</c> biasa ditolak
        /// <c>403</c> (<c>VAL-37</c>).
        ///
        /// Dinyatakan di metadata supaya layar menampilkan kedua kolom itu terkunci sejak awal.
        /// Menurut <c>LAB-FE-012</c>, pengguna harus tahu sebelum mencoba, bukan setelah gagal
        /// menyimpan.
        /// </summary>
        public List<string> SystemFlagFields { get; set; } = new();
    }

    public class LabSpecimenTypeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;
        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary>
        /// Jenis specimen tidak pernah dihapus; ia dinonaktifkan lewat
        /// <c>PUT /{id}/activation</c>. Jenis yang pernah dipakai menempel pada wadah yang
        /// sudah tersimpan, dan baris <c>Lainnya</c> yang aktif tidak dapat dinonaktifkan
        /// selama ia satu-satunya (<c>VAL-63</c>).
        /// </summary>
        public bool IsDeletable { get; set; } = false;

        /// <summary>
        /// Penanda <c>Lainnya</c> tidak dapat disetel dari layar mana pun. Ia lahir dari data
        /// awal, dan hanya satu baris aktif yang boleh memilikinya (<c>VAL-62</c>).
        /// </summary>
        public bool IsOtherBucketEditable { get; set; } = false;
    }

    public class LabSpecimenTypeSummaryResponse
    {
        public int TotalJenis { get; set; }
        public int Aktif { get; set; }
        public int Nonaktif { get; set; }

        /// <summary>
        /// Berapa banyak baris berpenanda <c>Lainnya</c> yang aktif. Nilai sehatnya selalu
        /// <c>1</c>; <c>0</c> berarti jalan keluar bagi jenis yang belum terdaftar sedang
        /// tertutup, dan penerimaan sampel aneh akan tertahan.
        /// </summary>
        public int JalanKeluarLainnyaAktif { get; set; }
    }

    public class LabRejectionReasonSummaryResponse
    {
        public int TotalAlasan { get; set; }
        public int Aktif { get; set; }
        public int Nonaktif { get; set; }

        /// <summary>
        /// Alasan yang ditandai kesalahan internal rumah sakit. Angka ini menentukan berapa
        /// banyak sebab penolakan yang biayanya ditanggung rumah sakit, bukan pasien.
        /// </summary>
        public int KesalahanInternalRumahSakit { get; set; }

        public int WajibDisertaiCatatan { get; set; }
    }

    /// <summary>
    /// Bentuk layar data induk breakpoint (<c>LAB-API-v1</c> <c>r30</c>, baseline master data).
    ///
    /// <b>Dibuat karena layarnya memang dikonsumsi</b> — <c>FE-LAB-34</c>. <c>QBE-OPT-001</c>
    /// menyediakan metadata hanya bila ada yang membacanya, dan di sini ada.
    /// </summary>
    public class LabSusceptibilityBreakpointFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;

        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary>
        /// <b>Benar.</b> Rentang breakpoint boleh dinonaktifkan ketika versi pedoman berganti —
        /// dan menonaktifkan, bukan menghapus, supaya hasil lama tetap dapat dibaca beserta
        /// rentang yang berlaku saat ia dibuat (<c>LAB-DEC-122</c>).
        /// </summary>
        public bool IsDeletable { get; set; } = true;
    }

    /// <summary>Bentuk layar pemetaan profil Mikrobiologi katalog.</summary>
    public class LabProcedureMicrobiologyProfileFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public List<LabEnumOptionResponse> CultureTypeOptions { get; set; } = new();

        public List<LabEnumOptionResponse> SusceptibilityMethodOptions { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;

        public bool SupportsServerSidePaging { get; set; } = true;

        public bool IsDeletable { get; set; } = true;
    }

    /// <summary>Ringkasan pemetaan profil Mikrobiologi katalog.</summary>
    public class LabProcedureMicrobiologyProfileSummaryResponse
    {
        public int TotalProfile { get; set; }

        public int ActiveProfile { get; set; }

        public int InactiveProfile { get; set; }

        /// <summary>Pemeriksaan yang tegas ditandai MEMAKAI set bakteri.</summary>
        public int UsesSusceptibilitySet { get; set; }

        /// <summary>
        /// Pemeriksaan yang tegas ditandai TIDAK memakai set bakteri.
        ///
        /// Angka inilah yang benar-benar menegakkan <c>VAL-118</c>: pemeriksaan yang belum
        /// diprofilkan tetap menerima isolat, sehingga yang membatasi hanya baris bertanda
        /// tegas.
        /// </summary>
        public int WithoutSusceptibilitySet { get; set; }
    }

    /// <summary>Baris ringan untuk dropdown profil Mikrobiologi katalog.</summary>
    public class LabProcedureMicrobiologyProfileOptionResponse
    {
        public Guid Id { get; set; }

        public Guid ProcedureId { get; set; }

        public string Label { get; set; } = string.Empty;

        public bool UsesSusceptibilitySet { get; set; }
    }

    /// <summary>Mengubah status aktif profil saja.</summary>
    public class LabProcedureMicrobiologyProfileStatusRequest
    {
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Bentuk layar data induk organisme Mikrobiologi (<c>FE-LAB-24</c>).
    ///
    /// <c>IsDeletable</c> bernilai <b><c>false</c></b>, dan itu satu-satunya hal yang membuat
    /// metadata ini berbeda dari kembarannya. <c>r24</c> bagian 19.4 menolak <c>DELETE</c> atas
    /// alasan klinis, dan <c>AC-117</c> menuntut layarnya nol menampilkan tombol Hapus. Layar
    /// membaca penanda ini alih-alih menebak dari ada-tidaknya endpoint.
    /// </summary>
    public class LabOrganismFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;

        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary><b>Salah, dan disengaja.</b> Menghapus organisme berarti menghapus temuan pasien.</summary>
        public bool IsDeletable { get; set; }
    }

    /// <summary>Bentuk layar panel uji antibiotik (<c>FE-LAB-24</c>).</summary>
    public class LabAntibioticFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;

        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary><b>Salah, dan disengaja.</b> Sama seperti organisme — baris lama tetap dirujuk hasil.</summary>
        public bool IsDeletable { get; set; }
    }

    // =====================================================================
    // Bentuk layar tiga data induk Patologi Anatomi — `r32`, `BE-LAB-66`, dipakai `FE-LAB-27`
    // =====================================================================

    /// <summary>
    /// Bentuk layar data induk parameter Patologi Anatomi (<c>FE-LAB-27</c>).
    ///
    /// <c>IsDeletable</c> bernilai <b><c>false</c></b>: parameter yang dihapus menarik ruas dari
    /// laporan pasien yang sudah tersimpan (<c>r32</c> bagian 27.5, <c>AC-143</c>).
    /// </summary>
    public class LabPathologyParameterFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;

        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary><b>Salah, dan disengaja.</b> Ruas yang ditarik akan mengosongkan laporan lama.</summary>
        public bool IsDeletable { get; set; }
    }

    /// <summary>
    /// Bentuk layar data induk golongan Patologi Anatomi (<c>FE-LAB-27</c>).
    ///
    /// <c>IsDeletable</c> bernilai <b><c>false</c></b>: golongan yang dihapus membuat pesanan
    /// lama nol punya bentuk formulir.
    /// </summary>
    public class LabPathologyCategoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;

        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary><b>Salah, dan disengaja.</b> Pesanan lama menunjuk golongannya.</summary>
        public bool IsDeletable { get; set; }
    }

    /// <summary>
    /// Bentuk layar penggolongan jenis pemeriksaan Patologi Anatomi (<c>FE-LAB-27</c>).
    ///
    /// <b>Dua penanda di sini berbeda dari kedua kembarannya, dan keduanya disengaja.</b>
    /// <see cref="SupportsStatusToggle"/> bernilai <c>false</c> sebab baris pemetaan nol punya
    /// status untuk dibalik, dan <see cref="HasOptionsEndpoint"/> bernilai <c>false</c> sebab nol
    /// satu pun layar memilih sebuah pemetaan dari kotak pilihan (<c>r32</c> bagian 27.4).
    /// Keduanya dinyatakan di sini supaya layar <b>membacanya</b> alih-alih menyimpulkan dari
    /// endpoint yang menjawab <c>404</c>.
    /// </summary>
    public class LabProcedurePathologyCategoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;

        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary><b>Salah, dan disengaja.</b> Pencabutan penggolongan lewat <c>PUT /{id}</c>.</summary>
        public bool IsDeletable { get; set; }

        /// <summary><b>Salah, dan disengaja.</b> Baris pemetaan nol punya <c>IsActive</c>.</summary>
        public bool SupportsStatusToggle { get; set; }

        /// <summary><b>Salah, dan disengaja.</b> Yang dipilih layar adalah pemeriksaan dan golongannya, bukan pemetaannya.</summary>
        public bool HasOptionsEndpoint { get; set; }
    }
}
