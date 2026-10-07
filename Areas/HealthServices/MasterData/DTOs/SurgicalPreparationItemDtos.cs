using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    /// <summary>
    /// Angka ringkasan master butir persiapan bedah untuk kartu statistik halaman index
    /// (<c>BE-RWI-173</c>).
    /// </summary>
    /// <remarks>
    /// Pencacah wajib dan tidak wajib dihitung dari butir <b>aktif</b> saja, karena hanya butir
    /// aktif yang ikut tersalin ke versi Catatan Pra-Operasi baru. Contoh: 14 butir aktif, 9 wajib
    /// — artinya perawat bangsal wajib mencentang 9 butir sebelum dapat mengirim.
    /// </remarks>
    public class SurgicalPreparationItemSummaryResponse
    {
        public int TotalSurgicalPreparationItem { get; set; }
        public int ActiveSurgicalPreparationItem { get; set; }
        public int InactiveSurgicalPreparationItem { get; set; }

        /// <summary>Butir aktif yang wajib dikonfirmasi pengirim sebelum mengirim.</summary>
        public int ActiveMandatorySurgicalPreparationItem { get; set; }

        /// <summary>Butir aktif yang tidak wajib.</summary>
        public int ActiveOptionalSurgicalPreparationItem { get; set; }

        /// <summary>Jumlah kelompok yang punya sedikitnya satu butir aktif.</summary>
        public int ActiveGroupCount { get; set; }
    }

    /// <summary>Bentuk balasan satu butir persiapan bedah (kontrak <c>0.10.0</c> API 11.4).</summary>
    public class SurgicalPreparationItemResponse
    {
        public Guid Id { get; set; }

        /// <summary>Kode unik di antara butir yang belum dihapus. Contoh <c>SPI-ID-01</c>.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Contoh <c>Verifikasi pasien</c>.</summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>Contoh <c>Gelang identitas terpasang</c>.</summary>
        public string ItemName { get; set; } = string.Empty;

        public bool IsMandatory { get; set; }

        /// <summary>Urutan tampil butir di dalam kelompoknya pada form pra-operasi.</summary>
        public int SortOrder { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Penanda konkurensi. Dikirim kembali pada <c>PUT /{id}</c>; bila sudah berganti karena
        /// admin lain menyimpan lebih dulu, permintaan ditolak <c>409</c>.
        /// </summary>
        public Guid RowVersion { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid? CreateBy { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }
    }

    /// <summary>Bentuk ringan untuk kotak pilihan dan penyusunan form pra-operasi.</summary>
    public class SurgicalPreparationItemOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Permintaan menambah butir persiapan bedah. Bentuknya mengikuti kontrak API 11.4:
    /// <c>{ Code, GroupName, ItemName, IsMandatory, SortOrder, Description? }</c>.
    /// </summary>
    /// <remarks>
    /// Kode diisi admin, bukan dibangkitkan backend, karena kontrak yang disetujui memuatnya pada
    /// body. Butir baru selalu lahir aktif; menonaktifkannya lewat <c>PATCH /{id}/status</c>.
    /// </remarks>
    public class CreateSurgicalPreparationItemRequest
    {
        [Required(ErrorMessage = "Kode butir wajib diisi.")]
        [MaxLength(30, ErrorMessage = "Kode butir terlalu panjang. Batasnya 30 huruf.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kelompok butir wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Kelompok butir terlalu panjang. Batasnya 100 huruf.")]
        public string GroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama butir wajib diisi.")]
        [MaxLength(200, ErrorMessage = "Nama butir terlalu panjang. Batasnya 200 huruf.")]
        public string ItemName { get; set; } = string.Empty;

        public bool IsMandatory { get; set; } = true;

        [Range(0, 9999, ErrorMessage = "Urutan tampil harus antara 0 dan 9999.")]
        public int SortOrder { get; set; }

        [MaxLength(500, ErrorMessage = "Keterangan terlalu panjang. Batasnya 500 huruf.")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Permintaan mengubah butir. Isinya sama dengan tambah, ditambah <c>RowVersion</c> yang
    /// dibaca dari <c>GET /{id}</c>.
    /// </summary>
    /// <remarks>
    /// Mengubah nama atau sifat wajib TIDAK mengubah versi pra-operasi yang sudah dibuat: nama dan
    /// sifat wajib disalin ke butir versi saat versi itu dibuat. Perubahan baru berlaku pada versi
    /// berikutnya.
    /// </remarks>
    public class UpdateSurgicalPreparationItemRequest : CreateSurgicalPreparationItemRequest
    {
        [Required(ErrorMessage = "RowVersion wajib dikirim agar perubahan admin lain tidak tertimpa.")]
        public Guid RowVersion { get; set; }
    }

    /// <summary>Permintaan mengaktifkan atau menonaktifkan butir.</summary>
    public class UpdateSurgicalPreparationItemStatusRequest
    {
        public bool IsActive { get; set; }

        /// <summary>
        /// Opsional. Bila dikirim dan sudah tidak sama dengan yang tersimpan, permintaan ditolak
        /// <c>409</c>. Kontrak API 11.4 hanya mewajibkan <c>IsActive</c>.
        /// </summary>
        public Guid? RowVersion { get; set; }
    }

    public class SurgicalPreparationItemFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        public SurgicalPreparationItemDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<SurgicalPreparationItemSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();

        /// <summary>
        /// Pilihan kelompok untuk penyaring dan form: empat kelompok baku <c>RWI-DEC-173</c> butir 3,
        /// ditambah kelompok lain yang sudah tersimpan bila admin pernah membuatnya.
        /// </summary>
        public List<string> GroupNameOptions { get; set; } = new();

        public List<SurgicalPreparationItemQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<SurgicalPreparationItemFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<SurgicalPreparationItemFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class SurgicalPreparationItemDefaultFilterResponse
    {
        public string? Search { get; set; }
        public string? GroupName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsMandatory { get; set; }
        public string SortBy { get; set; } = "groupName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class SurgicalPreparationItemSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SurgicalPreparationItemQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class SurgicalPreparationItemFormFieldMetadataResponse
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
