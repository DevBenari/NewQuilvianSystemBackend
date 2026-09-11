using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs
{
    // =========================================================================
    // Permintaan
    // =========================================================================

    /// <summary>
    /// Menambah butir keselamatan ke dalam kosakata modul.
    ///
    /// <b>Menambah butir di sini tidak membuatnya berlaku bagi pasien mana pun.</b> Butir baru
    /// hanya menjadi pilihan yang tersedia; yang mengikat adalah aturan keselamatan yang
    /// menyusunnya untuk sebuah alat dan disahkan penanggung jawab klinis.
    /// </summary>
    public class CreateRadSafetyRequirementRequest
    {
        [Required]
        [MaxLength(50)]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RequirementName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Pengelompokan butir, misalnya identitas, radiasi, kontras, implan, atau sedasi.
        /// Hanya untuk penyajian; tidak menentukan perilaku apa pun.
        /// </summary>
        [MaxLength(50)]
        public string? Category { get; set; }

        /// <summary>Menandai butir yang mewajibkan petugas mengisi catatan saat menjawab.</summary>
        public bool RequiresNote { get; set; }

        /// <summary>
        /// Asal-usul butir ini, ditulis apa adanya supaya tidak ada yang mengira baseline
        /// implementasi adalah SOP yang sudah disahkan.
        /// </summary>
        [MaxLength(500)]
        public string? SourceNote { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateRadSafetyRequirementRequest
    {
        [Required]
        [MaxLength(50)]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RequirementName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        public bool RequiresNote { get; set; }

        [MaxLength(500)]
        public string? SourceNote { get; set; }

        public int SortOrder { get; set; }
    }

    public class UpdateRadSafetyRequirementStatusRequest
    {
        public bool IsActive { get; set; }
    }

    /// <summary>Penyaring daftar butir keselamatan. Setiap field di sini diproses query.</summary>
    public class RadSafetyRequirementPagedQuery
    {
        /// <summary>Dicari pada kode, nama, dan keterangan butir.</summary>
        public string? Search { get; set; }

        public string? Category { get; set; }

        public bool? IsActive { get; set; }

        public bool? RequiresNote { get; set; }

        /// <summary>
        /// Menyaring butir menurut pemakaiannya: <c>true</c> hanya butir yang sedang dipakai
        /// aturan keselamatan berlaku, <c>false</c> hanya yang belum dipakai sama sekali.
        /// </summary>
        public bool? IsUsedByActiveRule { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    // =========================================================================
    // Jawaban
    // =========================================================================

    public class RadSafetyRequirementDetailResponse : RadSafetyRequirementResponse
    {
        public int SortOrder { get; set; }

        public Guid CreateBy { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        /// <summary>
        /// Jumlah aturan keselamatan berlaku yang memakai butir ini.
        ///
        /// Bernilai lebih dari nol berarti butir ini sedang benar-benar menahan pemeriksaan,
        /// dan karena itu tidak dapat dinonaktifkan maupun dihapus.
        /// </summary>
        public int ActiveRuleCount { get; set; }
    }

    /// <summary>Bentuk ringan untuk dropdown pada form penyusunan aturan keselamatan.</summary>
    public class RadSafetyRequirementOptionResponse
    {
        public Guid Id { get; set; }

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementName { get; set; } = string.Empty;

        public string? Category { get; set; }

        public bool RequiresNote { get; set; }
    }

    public class RadSafetyRequirementFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>
        /// Kelompok butir yang benar-benar dipakai data saat ini, bukan daftar tetap. Kategori
        /// adalah teks bebas, sehingga daftarnya hanya dapat diturunkan dari isinya.
        /// </summary>
        public List<string> Categories { get; set; } = new();

        public List<RadSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RadQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }

    public class RadSafetyRequirementSummaryResponse
    {
        public int TotalButir { get; set; }

        public int Aktif { get; set; }

        public int Nonaktif { get; set; }

        public int WajibBercatatan { get; set; }

        /// <summary>
        /// Butir yang sedang dipakai sedikitnya satu aturan keselamatan berlaku — inilah butir
        /// yang benar-benar ditanyakan kepada pasien hari ini.
        /// </summary>
        public int DipakaiAturanBerlaku { get; set; }

        /// <summary>
        /// Butir aktif yang belum dipakai satu pun aturan. Bukan kesalahan — kosakata boleh
        /// lebih luas daripada kebijakan — tetapi angkanya membantu admin melihat butir yang
        /// terlanjur dibuat lalu terlupakan.
        /// </summary>
        public int BelumDipakai { get; set; }
    }
}
