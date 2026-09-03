using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    /// <summary>
    /// Angka ringkasan katalog komponen darah untuk kartu statistik di halaman index.
    /// </summary>
    /// <remarks>
    /// Dua pencacah terakhir bukan hiasan. Komponen yang masa berlaku bukti kecocokannya
    /// belum ditetapkan <b>tidak dapat diberikan sama sekali</b> — gerbang pemberian menolak
    /// dengan <c>VAL-BD-020b</c>. Menampilkan angkanya di halaman index membuat keadaan itu
    /// terlihat sebelum petugas kebingungan di depan pasien.
    /// </remarks>
    public class BloodComponentSummaryResponse
    {
        public int TotalBloodComponent { get; set; }
        public int ActiveBloodComponent { get; set; }
        public int InactiveBloodComponent { get; set; }

        /// <summary>Komponen aktif yang masa berlaku bukti kecocokannya sudah ditetapkan.</summary>
        public int ValidityConfiguredBloodComponent { get; set; }

        /// <summary>
        /// Komponen aktif yang masa berlakunya masih kosong, sehingga pemberiannya tertahan.
        /// </summary>
        public int ValidityNotConfiguredBloodComponent { get; set; }
    }

    /// <summary>Bentuk balasan satu komponen darah.</summary>
    public class BloodComponentResponse
    {
        public Guid Id { get; set; }

        /// <summary>Kode komponen, unik di seluruh tabel. Contoh <c>PRC</c>.</summary>
        public string ComponentCode { get; set; } = string.Empty;

        public string ComponentName { get; set; } = string.Empty;

        /// <summary>
        /// Masa berlaku bukti kecocokan dalam jam. Kosong berarti pemberian komponen ini
        /// tertahan sampai nilainya ditetapkan (<c>VAL-BD-020b</c>).
        /// </summary>
        public int? CompatibilityEvidenceValidityHours { get; set; }

        /// <summary>
        /// Penanda turunan yang dihitung backend, supaya layar tidak perlu menyimpulkannya
        /// sendiri dari nilai kosong.
        /// </summary>
        public bool IsIssuanceBlockedByMissingValidity { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>Bentuk ringan untuk kotak pilihan pada layar lain.</summary>
    public class BloodComponentOptionResponse
    {
        public Guid Id { get; set; }
        public string ComponentCode { get; set; } = string.Empty;
        public string ComponentName { get; set; } = string.Empty;

        /// <summary>
        /// Ikut dikirim supaya layar order darah dapat memperingatkan lebih awal bahwa
        /// komponen ini belum dapat diberikan, tanpa memanggil endpoint detail satu per satu.
        /// </summary>
        public int? CompatibilityEvidenceValidityHours { get; set; }
    }

    public class CreateBloodComponentRequest
    {
        [Required(ErrorMessage = "Kode komponen wajib diisi.")]
        [MaxLength(20, ErrorMessage = "Kode komponen terlalu panjang. Batasnya 20 huruf.")]
        public string ComponentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama komponen wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Nama komponen terlalu panjang. Batasnya 100 huruf.")]
        public string ComponentName { get; set; } = string.Empty;

        /// <summary>
        /// Boleh dikosongkan saat komponen pertama kali didaftarkan. Kebijakan klinis MMC
        /// untuk angka jamnya masih berjalan (<c>OQ-BD-012</c>), dan menahannya lebih aman
        /// daripada menanam angka tebakan.
        /// </summary>
        [Range(1, 8760, ErrorMessage = "Masa berlaku bukti kecocokan harus antara 1 dan 8760 jam.")]
        public int? CompatibilityEvidenceValidityHours { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Bentuk permintaan mengubah komponen darah. Sengaja memuat <c>ComponentCode</c>, karena
    /// kode yang salah ketik harus dapat dibetulkan selama kode barunya belum dipakai
    /// komponen lain.
    /// </summary>
    public class UpdateBloodComponentRequest : CreateBloodComponentRequest
    {
    }

    public class UpdateBloodComponentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class BloodComponentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        public BloodComponentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BloodComponentSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<BloodComponentQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<BloodComponentFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<BloodComponentFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class BloodComponentDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }

        /// <summary>
        /// Penyaring khas katalog ini: menampilkan hanya komponen yang masa berlakunya belum
        /// ditetapkan, supaya admin dapat menyelesaikan konfigurasi yang tertinggal.
        /// </summary>
        public bool? IsValidityConfigured { get; set; }

        public string SortBy { get; set; } = "componentCode";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class BloodComponentSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class BloodComponentQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class BloodComponentFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }
}
