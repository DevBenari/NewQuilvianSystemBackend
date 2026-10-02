using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    // =====================================================================
    // Parameter — ruas isian laporan Patologi Anatomi
    // =====================================================================

    /// <summary>
    /// Penyaring daftar parameter untuk layar pengelolaan kepala instalasi. Memuat yang nonaktif
    /// juga; tanpa itu parameter yang dinonaktifkan tidak akan pernah dapat diaktifkan kembali.
    /// </summary>
    public class LabPathologyParameterPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public bool? IsActive { get; set; }

        /// <summary>Pencarian bebas pada kode dan label parameter.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Penyaring daftar pilihan parameter. Berbeda dari
    /// <see cref="LabPathologyParameterPagedQuery"/>: jalur ini <b>hanya</b> mengembalikan
    /// parameter aktif, karena keberlakuan baru nol boleh menunjuk parameter yang sudah ditarik
    /// (<c>VAL-99</c>).
    /// </summary>
    public class LabPathologyParameterOptionQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 50;

        public string? Search { get; set; }
    }

    /// <summary>Bentuk tampilan satu parameter pada layar pengelolaan.</summary>
    public class LabPathologyParameterResponse
    {
        public Guid Id { get; set; }

        public string ParameterCode { get; set; } = string.Empty;

        public string ParameterName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }

    /// <summary>Bentuk ringan untuk kotak pilihan saat menyusun keberlakuan.</summary>
    public class LabPathologyParameterOptionResponse
    {
        public Guid Id { get; set; }

        public string ParameterCode { get; set; } = string.Empty;

        public string ParameterName { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }

    /// <summary>Menambah parameter baru. Parameter baru selalu lahir aktif.</summary>
    public class CreateLabPathologyParameterRequest
    {
        /// <summary>Kode ruas, dinormalkan menjadi huruf kapital dan wajib unik (<c>VAL-101</c>).</summary>
        [Required]
        [MaxLength(32)]
        public string ParameterCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ParameterName { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Mengubah label, urutan, dan status aktif satu parameter.
    ///
    /// Kode parameter tidak ikut berubah: ia menjadi penanda yang dirujuk seeder dan keberlakuan
    /// yang sudah tersimpan, sehingga mengubahnya diam-diam memutus rujukan yang sudah ada —
    /// pola yang sama dengan <c>UpdateLabSpecimenTypeRequest</c>.
    ///
    /// <b>Status aktif ada di sini, bukan pada jalur tersendiri.</b> Kontrak <c>r25</c> bagian
    /// 20.3 menyediakan <c>GET</c>, <c>GET /options</c>, <c>POST</c>, dan <c>PUT /{id}</c> saja —
    /// nol <c>DELETE</c> dan nol jalur aktivasi. Penonaktifan karena itu berjalan lewat sini.
    /// </summary>
    public class UpdateLabPathologyParameterRequest
    {
        [Required]
        [MaxLength(200)]
        public string ParameterName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // =====================================================================
    // Kategori — golongan pemeriksaan Patologi Anatomi
    // =====================================================================

    /// <summary>Penyaring daftar golongan untuk layar pengelolaan.</summary>
    public class LabPathologyCategoryPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public bool? IsActive { get; set; }

        public string? Search { get; set; }
    }

    /// <summary>Penyaring daftar pilihan golongan. Hanya golongan aktif yang dikembalikan.</summary>
    public class LabPathologyCategoryOptionQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 50;

        public string? Search { get; set; }
    }

    /// <summary>Bentuk tampilan satu golongan pada layar pengelolaan.</summary>
    public class LabPathologyCategoryResponse
    {
        public Guid Id { get; set; }

        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Berapa ruas isian yang berlaku bagi golongan ini.
        ///
        /// Ikut ditampilkan karena nilai <c>0</c> adalah keadaan berbahaya yang tidak terlihat
        /// dari mana pun: golongan tanpa keberlakuan menghasilkan formulir kosong, dan
        /// <c>VAL-100</c> baru akan menyebutnya ketika patolog sudah membuka layarnya.
        /// </summary>
        public int ParameterCount { get; set; }
    }

    /// <summary>Bentuk ringan untuk kotak pilihan golongan.</summary>
    public class LabPathologyCategoryOptionResponse
    {
        public Guid Id { get; set; }

        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }

    /// <summary>Menambah golongan baru. Golongan baru selalu lahir aktif dan tanpa keberlakuan.</summary>
    public class CreateLabPathologyCategoryRequest
    {
        [Required]
        [MaxLength(32)]
        public string CategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string CategoryName { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }

    /// <summary>Mengubah nama, urutan, dan status aktif satu golongan. Kode tidak ikut berubah.</summary>
    public class UpdateLabPathologyCategoryRequest
    {
        [Required]
        [MaxLength(128)]
        public string CategoryName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // =====================================================================
    // Keberlakuan — ruas apa saja yang berlaku bagi sebuah golongan
    // =====================================================================

    /// <summary>
    /// Satu ruas yang berlaku bagi sebuah golongan, beserta wajib atau tidaknya.
    ///
    /// <b>Inilah bentuk yang membangun formulir laporan.</b> Layar nol menebak: urutan, label,
    /// dan penanda wajibnya seluruhnya datang dari sini.
    /// </summary>
    public class LabPathologyCategoryParameterResponse
    {
        public Guid LabPathologyParameterId { get; set; }

        public string ParameterCode { get; set; } = string.Empty;

        public string ParameterName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        /// <summary>
        /// Apakah parameternya sendiri masih aktif. Keberlakuan yang menunjuk parameter nonaktif
        /// tetap ditampilkan apa adanya supaya kepala instalasi melihat sebab ruas itu hilang
        /// dari formulir, alih-alih ia lenyap tanpa jejak.
        /// </summary>
        public bool IsParameterActive { get; set; }

        /// <summary>Wajib terisi sebelum laporan dapat difinalkan (<c>INV-34</c>, <c>VAL-95</c>).</summary>
        public bool IsRequired { get; set; }
    }

    /// <summary>Satu baris keberlakuan yang hendak disimpan.</summary>
    public class LabPathologyCategoryParameterItemRequest
    {
        [Required]
        public Guid LabPathologyParameterId { get; set; }

        public bool IsRequired { get; set; } = true;
    }

    /// <summary>
    /// Mengganti <b>seluruh</b> daftar keberlakuan sebuah golongan sekaligus.
    ///
    /// <b>Kenapa mengganti seluruhnya, bukan menambah dan menghapus per baris.</b> Yang disusun
    /// kepala instalasi adalah <i>bentuk formulir</i> — satu benda utuh, bukan kumpulan baris
    /// lepas. Jalur tambah/hapus per baris membuat dua penyunting yang bekerja bersamaan dapat
    /// menghasilkan formulir yang tidak pernah dimaksudkan keduanya. Daftar kosong sah dan
    /// berarti golongan itu nol punya ruas.
    /// </summary>
    public class ReplaceLabPathologyCategoryParametersRequest
    {
        public List<LabPathologyCategoryParameterItemRequest> Items { get; set; } = new();
    }

    // =====================================================================
    // Pemetaan jenis pemeriksaan ke golongan
    // =====================================================================

    /// <summary>Penyaring daftar pemetaan jenis pemeriksaan.</summary>
    public class LabProcedurePathologyCategoryPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        /// <summary>Menyaring pada satu golongan.</summary>
        public Guid? LabPathologyCategoryId { get; set; }

        /// <summary>Pencarian bebas pada kode dan nama jenis pemeriksaan.</summary>
        public string? Search { get; set; }
    }

    /// <summary>Bentuk tampilan satu pemetaan.</summary>
    public class LabProcedurePathologyCategoryResponse
    {
        public Guid Id { get; set; }

        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        public Guid LabPathologyCategoryId { get; set; }

        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;
    }

    /// <summary>Menggolongkan satu jenis pemeriksaan katalog.</summary>
    public class CreateLabProcedurePathologyCategoryRequest
    {
        [Required]
        public Guid ProcedureId { get; set; }

        [Required]
        public Guid LabPathologyCategoryId { get; set; }
    }

    /// <summary>
    /// Memindahkan satu jenis pemeriksaan ke golongan lain.
    ///
    /// Jenis pemeriksaannya tidak ikut berubah: memindahkan baris ini ke pemeriksaan lain sama
    /// artinya dengan mencabut satu penggolongan dan membuat penggolongan lain, dan keduanya
    /// punya jalurnya sendiri.
    /// </summary>
    public class UpdateLabProcedurePathologyCategoryRequest
    {
        [Required]
        public Guid LabPathologyCategoryId { get; set; }
    }

    /// <summary>Penyaring daftar usulan pemetaan.</summary>
    public class LabProcedurePathologyCategorySuggestionQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 50;

        public string? Search { get; set; }
    }

    /// <summary>
    /// Satu <b>usulan</b> penggolongan — dan kata "usulan" di sini menanggung seluruh beban
    /// bentuk ini (<c>LAB-DEC-087</c>).
    ///
    /// <b>Tidak ada yang tersimpan saat bentuk ini dibuat.</b> Ia hasil pencocokan kata kunci
    /// pada nama pemeriksaan, dan pencocokan kata kunci tidak tahu apa-apa soal patologi. Yang
    /// menyimpannya tetap manusia, lewat <c>POST</c> tersendiri. Sesudah pengisian awal selesai,
    /// jalur ini boleh tidak dipakai lagi.
    ///
    /// <see cref="SuggestedCategoryId"/> kosong ketika nol kata kunci cocok — dan itu jawaban
    /// yang sah, bukan kegagalan. Pemeriksaan yang namanya tidak memuat kata kunci mana pun
    /// memang harus digolongkan manusia tanpa bantuan.
    /// </summary>
    public class LabProcedurePathologyCategorySuggestionResponse
    {
        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        public Guid? SuggestedCategoryId { get; set; }

        public string? SuggestedCategoryCode { get; set; }

        public string? SuggestedCategoryName { get; set; }

        /// <summary>
        /// Kata kunci yang membuat usulan ini muncul, ditampilkan apa adanya supaya pemeriksanya
        /// dapat menilai apakah kecocokannya masuk akal — bukan hanya menerima hasilnya.
        /// </summary>
        public string? MatchedKeyword { get; set; }
    }

    // =====================================================================
    // Permukaan baseline yang dilengkapi 2026-09-23 — `LAB-API-v1` r32, `BE-LAB-66`
    // =====================================================================

    /// <summary>
    /// Membalik penanda aktif satu baris data induk Patologi Anatomi
    /// (<c>PATCH /{id}/status</c>).
    ///
    /// <b>Dipakai bersama parameter dan golongan, dan sengaja tidak dipakai pemetaan.</b>
    /// <c>LabProcedurePathologyCategory</c> nol punya <c>IsActive</c> — ia baris pemetaan, bukan
    /// data induk berstatus (<c>r32</c> bagian 27.4).
    /// </summary>
    public class LabPathologyMasterDataStatusRequest
    {
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Ringkasan data induk parameter (<c>GET /summary</c>).
    ///
    /// <see cref="UsedInCategory"/> menghitung parameter yang <b>dipakai sedikitnya satu
    /// golongan</b>. Selisihnya terhadap <see cref="TotalParameter"/> adalah parameter yang nol
    /// pernah dipakai di mana pun — ruas yang terdaftar tetapi nol akan pernah muncul pada satu
    /// formulir pun, dan itu keadaan yang nol terlihat dari daftar mana pun.
    /// </summary>
    public class LabPathologyParameterSummaryResponse
    {
        public int TotalParameter { get; set; }

        public int ActiveParameter { get; set; }

        public int InactiveParameter { get; set; }

        public int UsedInCategory { get; set; }
    }

    /// <summary>
    /// Ringkasan data induk golongan (<c>GET /summary</c>).
    ///
    /// <see cref="WithParameter"/> menghitung golongan yang punya sedikitnya satu keberlakuan.
    /// <b>Selisihnya terhadap <see cref="TotalCategory"/> adalah golongan yang menghasilkan
    /// formulir kosong</b>, dan <c>VAL-100</c> baru menyebutnya ketika patolog sudah membuka
    /// layar hasil — angka ini yang membuatnya terlihat lebih awal.
    /// </summary>
    public class LabPathologyCategorySummaryResponse
    {
        public int TotalCategory { get; set; }

        public int ActiveCategory { get; set; }

        public int InactiveCategory { get; set; }

        public int WithParameter { get; set; }
    }

    /// <summary>
    /// Ringkasan penggolongan jenis pemeriksaan (<c>GET /summary</c>).
    ///
    /// <b><see cref="UnmappedProcedure"/> adalah alasan ringkasan ini ada.</b> Ia angka
    /// pekerjaan yang tersisa bagi kepala instalasi, dan penahan yang <c>FE-LAB-28</c> tunggu:
    /// pesanan Patologi Anatomi yang jenis pemeriksaannya belum digolongkan nol punya bentuk
    /// formulir sama sekali (<c>INV-39</c>).
    ///
    /// Ketiganya memakai definisi jenis pemeriksaan Patologi Anatomi yang <b>sama persis</b>
    /// dengan <c>GET /suggestions</c> — <c>IsLaboratory</c> benar dan <c>LabDiscipline</c>
    /// bernilai <c>AnatomicalPathology</c>. Definisi yang berbeda akan membuat ringkasan dan
    /// daftar usulan saling membantah.
    /// </summary>
    public class LabProcedurePathologyCategorySummaryResponse
    {
        public int TotalProcedure { get; set; }

        public int MappedProcedure { get; set; }

        public int UnmappedProcedure { get; set; }
    }
}
