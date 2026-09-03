using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    /// <summary>
    /// Angka ringkasan master lokasi penyimpanan darah untuk kartu statistik halaman index.
    /// </summary>
    /// <remarks>
    /// <c>ActiveBloodStorageLocation</c> bukan angka biasa. Ketika ia bernilai nol, seluruh
    /// alur Bank Darah berhenti: tidak ada kantong yang dapat disimpan, dialokasikan, maupun
    /// diberikan (<c>INV-BD-025</c>). Angka itu ditampilkan di halaman index supaya keadaan
    /// tersebut terlihat sebelum ada pasien yang menunggu.
    /// </remarks>
    public class BloodStorageLocationSummaryResponse
    {
        public int TotalBloodStorageLocation { get; set; }
        public int ActiveBloodStorageLocation { get; set; }
        public int InactiveBloodStorageLocation { get; set; }

        /// <summary>
        /// Bernilai benar ketika tidak ada satu pun lokasi aktif. Dihitung backend supaya
        /// layar tidak perlu menyimpulkannya sendiri dari angka nol.
        /// </summary>
        public bool IsBloodBankHaltedByEmptyActiveLocation { get; set; }
    }

    /// <summary>Bentuk balasan satu lokasi penyimpanan darah.</summary>
    public class BloodStorageLocationResponse
    {
        public Guid Id { get; set; }

        /// <summary>Kode lokasi, unik di seluruh tabel. Contoh <c>KLK-BSR</c>.</summary>
        public string StorageLocationCode { get; set; } = string.Empty;

        public string StorageLocationName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Lokasi nonaktif tidak dapat menjadi tujuan penyimpanan maupun perpindahan, dan
        /// menahan alokasi kantong yang masih tercatat di dalamnya (<c>DEC-BD-037</c>).
        /// </summary>
        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>
    /// Bentuk ringan untuk kotak pilihan lokasi penyimpanan pada layar kantong darah.
    /// </summary>
    /// <remarks>
    /// Feed ini <b>hanya</b> memuat lokasi aktif, dan itu disengaja: menyaringnya di backend
    /// membuat layar tidak dapat menawarkan lokasi nonaktif walaupun penulis layarnya lupa
    /// menyaring. Ini lapisan pertama penegakan <c>INV-BD-027</c>; lapisan yang mengikat tetap
    /// pemeriksaan di jalur penyimpanan, yang dikerjakan <c>BE-BD-015</c>.
    /// </remarks>
    public class BloodStorageLocationOptionResponse
    {
        public Guid Id { get; set; }
        public string StorageLocationCode { get; set; } = string.Empty;
        public string StorageLocationName { get; set; } = string.Empty;
    }

    public class CreateBloodStorageLocationRequest
    {
        [Required(ErrorMessage = "Kode lokasi penyimpanan wajib diisi.")]
        [MaxLength(30, ErrorMessage = "Kode lokasi penyimpanan terlalu panjang. Batasnya 30 huruf.")]
        public string StorageLocationCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama lokasi penyimpanan wajib diisi.")]
        [MaxLength(150, ErrorMessage = "Nama lokasi penyimpanan terlalu panjang. Batasnya 150 huruf.")]
        public string StorageLocationName { get; set; } = string.Empty;

        [MaxLength(250, ErrorMessage = "Keterangan terlalu panjang. Batasnya 250 huruf.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Bentuk permintaan mengubah lokasi penyimpanan. Sengaja memuat
    /// <c>StorageLocationCode</c>, karena kode yang salah ketik harus dapat dibetulkan selama
    /// kode barunya belum dipakai lokasi lain.
    /// </summary>
    public class UpdateBloodStorageLocationRequest : CreateBloodStorageLocationRequest
    {
    }

    public class UpdateBloodStorageLocationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class BloodStorageLocationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        public BloodStorageLocationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BloodStorageLocationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<BloodStorageLocationQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<BloodStorageLocationFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<BloodStorageLocationFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class BloodStorageLocationDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "storageLocationCode";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class BloodStorageLocationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class BloodStorageLocationQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class BloodStorageLocationFormFieldMetadataResponse
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
