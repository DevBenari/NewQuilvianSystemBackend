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
}
