using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs
{
    // =========================================================================
    // Permintaan
    // =========================================================================

    /// <summary>
    /// Mendaftarkan alat pencitraan baru.
    ///
    /// Kode alat mengikuti kode modalitas DICOM yang berlaku umum — `CT`, `MR`, `US`, dan
    /// seterusnya. Itu kosakata teknis internasional, bukan kebijakan rumah sakit.
    /// </summary>
    public class CreateRadModalityRequest
    {
        [Required]
        [MaxLength(20)]
        public string ModalityCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ModalityName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Menandai alat yang memakai radiasi pengion.
        ///
        /// Penanda ini <b>tidak</b> menetapkan gerbang keselamatan apa pun dengan sendirinya.
        /// Yang mengikat tetap baris aturan yang benar-benar disusun dan disahkan.
        /// </summary>
        public bool UsesIonisingRadiation { get; set; }

        public bool SupportsContrast { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateRadModalityRequest
    {
        [Required]
        [MaxLength(20)]
        public string ModalityCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ModalityName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool UsesIonisingRadiation { get; set; }

        public bool SupportsContrast { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Menyalakan atau mematikan sebuah alat.
    ///
    /// Dipisahkan dari <see cref="UpdateRadModalityRequest"/> karena tombol aktif di tabel
    /// hanya mengubah satu kolom, dan tidak seharusnya menuntut seluruh isian form dikirim
    /// ulang.
    /// </summary>
    public class UpdateRadModalityStatusRequest
    {
        public bool IsActive { get; set; }
    }

    /// <summary>Penyaring daftar alat pencitraan. Setiap field di sini diproses query.</summary>
    public class RadModalityPagedQuery
    {
        /// <summary>Dicari pada kode, nama, dan keterangan alat.</summary>
        public string? Search { get; set; }

        public bool? IsActive { get; set; }

        public bool? UsesIonisingRadiation { get; set; }

        public bool? SupportsContrast { get; set; }

        /// <summary>
        /// Menyaring alat menurut kesiapan gerbangnya: <c>true</c> hanya yang sudah punya
        /// aturan keselamatan berlaku, <c>false</c> hanya yang belum.
        /// </summary>
        public bool? HasActiveSafetyRule { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    // =========================================================================
    // Jawaban
    // =========================================================================

    /// <summary>
    /// Rincian satu alat, lebih lengkap daripada satu baris daftar — termasuk jejak siapa
    /// mendaftarkan dan siapa terakhir mengubahnya.
    /// </summary>
    public class RadModalityDetailResponse : RadModalityResponse
    {
        public Guid CreateBy { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        /// <summary>
        /// Jumlah aturan keselamatan yang sedang berlaku pada alat ini. Bernilai nol berarti
        /// setiap pemeriksaan pada alat ini akan ditolak gerbang.
        /// </summary>
        public int ActiveSafetyRuleCount { get; set; }
    }

    /// <summary>Bentuk ringan untuk dropdown pada form lain.</summary>
    public class RadModalityOptionResponse
    {
        public Guid Id { get; set; }

        public string ModalityCode { get; set; } = string.Empty;

        public string ModalityName { get; set; } = string.Empty;

        public bool UsesIonisingRadiation { get; set; }

        public bool SupportsContrast { get; set; }

        /// <summary>
        /// Ikut dikirim supaya layar yang memilih alat dapat memperingatkan lebih dulu bahwa
        /// alat itu akan menolak pemeriksaan — bukan membiarkan petugas tahu belakangan.
        /// </summary>
        public bool HasActiveSafetyRule { get; set; }
    }

    public class RadModalityFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<RadSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RadQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }

    public class RadModalitySummaryResponse
    {
        public int TotalAlat { get; set; }

        public int Aktif { get; set; }

        public int Nonaktif { get; set; }

        public int MemakaiRadiasiPengion { get; set; }

        public int MendukungKontras { get; set; }

        /// <summary>
        /// Alat aktif yang belum punya satu pun aturan keselamatan berlaku.
        ///
        /// <b>Wajib nol sebelum modul dipakai.</b> Gerbang bersifat fail-closed, sehingga alat
        /// yang terhitung di sini akan menolak seluruh pemeriksaannya.
        /// </summary>
        public int BelumPunyaAturanKeselamatan { get; set; }
    }
}
