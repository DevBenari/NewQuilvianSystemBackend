namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs
{
    // =========================================================================
    // Bentuk bersama
    //
    // Mengikuti pola metadata dan ringkasan yang sudah dipakai grup Master Data
    // dan modul Rekam Medis, supaya layar Radiologi dapat memakai komponen
    // penyaring yang sama tanpa penyesuaian khusus.
    // =========================================================================

    /// <summary>Satu pilihan pengurutan pada layar daftar.</summary>
    public class RadSortOptionResponse
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
    public class RadEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>Keterangan satu parameter query, supaya layar tahu apa yang boleh dikirim.</summary>
    public class RadQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    // =========================================================================
    // Rad Order
    // =========================================================================

    public class RadOrderFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>
        /// Sepuluh status pesanan. <c>Draft</c> ikut dikirim supaya angka status lain tidak
        /// bergeser, tetapi ditandai tidak dipakai sesuai <c>RAD-DEC-011</c> — layar sebaiknya
        /// tidak menawarkannya sebagai penyaring.
        /// </summary>
        public List<RadEnumOptionResponse> OrderStatuses { get; set; } = new();

        public List<RadSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();

        /// <summary>
        /// <b>Sengaja tidak ada <c>PageSizeOptions</c>.</b> Daftar radiologi belum memakai
        /// <c>PagedResult</c> — <c>RAD-API-001</c> menargetkannya, tetapi perpindahannya adalah
        /// perubahan yang merusak konsumen dan menjadi task tersendiri. Mengirim pilihan jumlah
        /// baris sekarang berarti menjanjikan penyaring yang tidak diproses daftar.
        /// </summary>
        public List<RadQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }

    public class RadOrderSummaryResponse
    {
        public int TotalPesanan { get; set; }

        public int Diminta { get; set; }
        public int Diterima { get; set; }
        public int Dijadwalkan { get; set; }
        public int SedangDikerjakan { get; set; }
        public int Selesai { get; set; }
        public int Ditahan { get; set; }
        public int Dibatalkan { get; set; }
        public int Ditolak { get; set; }

        /// <summary>
        /// Pesanan yang masih menunggu dikerjakan — diminta, diterima, atau dijadwalkan.
        /// Angka inilah yang menunjukkan beban kerja yang belum tersentuh.
        /// </summary>
        public int BelumDikerjakan { get; set; }
    }

    // =========================================================================
    // Rad Study
    // =========================================================================

    public class RadStudyFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<RadEnumOptionResponse> StudyStatuses { get; set; } = new();

        public List<RadEnumOptionResponse> SafetyCheckStates { get; set; } = new();

        /// <summary>
        /// Empat keadaan aturan keselamatan pada siklus pengesahannya. Hanya <c>Active</c>
        /// yang dinilai gerbang — <c>RAD-DEC-005</c>.
        /// </summary>
        public List<RadEnumOptionResponse> SafetyRuleStatuses { get; set; } = new();

        public List<RadSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();

        /// <summary>
        /// <b>Sengaja tidak ada <c>PageSizeOptions</c>.</b> Daftar radiologi belum memakai
        /// <c>PagedResult</c> — <c>RAD-API-001</c> menargetkannya, tetapi perpindahannya adalah
        /// perubahan yang merusak konsumen dan menjadi task tersendiri. Mengirim pilihan jumlah
        /// baris sekarang berarti menjanjikan penyaring yang tidak diproses daftar.
        /// </summary>
        public List<RadQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }

    public class RadStudySummaryResponse
    {
        public int TotalStudy { get; set; }

        public int Direncanakan { get; set; }
        public int IdentitasTerverifikasi { get; set; }
        public int LolosGerbangKeselamatan { get; set; }
        public int SedangDiambil { get; set; }
        public int CitraSudahDiambil { get; set; }
        public int MutuDiterima { get; set; }
        public int MutuDitolak { get; set; }
        public int Dihentikan { get; set; }

        /// <summary>
        /// Study yang identitasnya sudah diverifikasi tetapi belum lolos gerbang keselamatan.
        /// Inilah antrian yang benar-benar menunggu jawaban butir keselamatan.
        /// </summary>
        public int MenungguGerbangKeselamatan { get; set; }

        /// <summary>
        /// Alat pencitraan aktif yang belum punya satu pun aturan keselamatan berlaku.
        ///
        /// <b>Angka ini wajib ditampilkan dan wajib nol.</b> Gerbang bersifat fail-closed:
        /// setiap alat yang terhitung di sini akan menolak seluruh pemeriksaannya, dan
        /// mengetahuinya di depan jauh lebih baik daripada mengetahuinya ketika pasien sudah
        /// berbaring di meja pemeriksaan.
        /// </summary>
        public int AlatTanpaAturanKeselamatanAktif { get; set; }
    }

    // =========================================================================
    // Rad Safety Rule
    // =========================================================================

    public class RadSafetyRuleFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>
        /// Empat keadaan pada siklus pengesahan. Hanya <c>Active</c> yang dinilai gerbang.
        /// </summary>
        public List<RadEnumOptionResponse> RuleStatuses { get; set; } = new();

        public List<RadSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RadQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        /// <summary>
        /// Aksi yang tersedia pada layar, beserta keadaan asal yang memperbolehkannya.
        ///
        /// Dikirim supaya layar tidak menebak dari nilai status. Ini <b>bantuan tampilan</b>,
        /// bukan pengaman: setiap endpoint tetap memeriksa ulang keadaan dan kewenangan.
        /// </summary>
        public List<RadSafetyRuleActionInfoResponse> Actions { get; set; } = new();
    }

    public class RadSafetyRuleActionInfoResponse
    {
        public string Action { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        /// <summary>Keadaan aturan yang memperbolehkan aksi ini dijalankan.</summary>
        public string FromStatus { get; set; } = string.Empty;

        public string ToStatus { get; set; } = string.Empty;
    }

    public class RadSafetyRuleSummaryResponse
    {
        public int TotalAturan { get; set; }

        public int Draf { get; set; }
        public int MenungguPengesahan { get; set; }
        public int Berlaku { get; set; }
        public int Dihentikan { get; set; }

        /// <summary>Aturan berlaku yang butirnya wajib dijawab sebelum penyinaran.</summary>
        public int BerlakuDanWajib { get; set; }

        /// <summary>
        /// Alat pencitraan aktif yang belum punya satu pun aturan berlaku. Wajib nol sebelum
        /// modul dipakai — lihat <c>GET /coverage</c> untuk daftar alatnya.
        /// </summary>
        public int AlatBelumTercakup { get; set; }
    }

    /// <summary>
    /// Satu alat pencitraan yang <b>belum</b> punya aturan keselamatan berlaku.
    ///
    /// Daftar ini sengaja hanya memuat alat yang bermasalah, bukan seluruh alat. Layar tidak
    /// perlu menyaring apa pun: daftar kosong berarti seluruh alat sudah siap dipakai, dan
    /// setiap baris yang muncul adalah alat yang akan menolak seluruh pemeriksaannya hari ini.
    /// </summary>
    public class RadModalityCoverageResponse
    {
        public Guid ModalityId { get; set; }

        public string ModalityCode { get; set; } = string.Empty;

        public string ModalityName { get; set; } = string.Empty;

        public bool UsesIonisingRadiation { get; set; }

        /// <summary>
        /// Jumlah aturan yang sudah disusun tetapi belum disahkan. Bernilai lebih dari nol
        /// berarti pekerjaannya sudah dimulai dan tinggal menunggu pengesahan; bernilai nol
        /// berarti belum ada yang menyusun sama sekali.
        /// </summary>
        public int DraftOrPendingRuleCount { get; set; }
    }
}
