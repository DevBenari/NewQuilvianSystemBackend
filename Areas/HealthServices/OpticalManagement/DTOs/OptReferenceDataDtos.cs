using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs
{
    // OPT-API-v1 grup "Health Services / Optical Management / Opt Reference Data" (BE-OPT-005).

    /// <summary>Satu nilai enum berlabel Indonesia untuk daftar pilihan di layar.</summary>
    public class OptEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>Seluruh daftar pilihan modul Optik; label tidak di-hardcode di frontend.</summary>
    public class OptMetadataResponse
    {
        public List<OptEnumOptionResponse> ItemCategories { get; set; } = new();
        public List<OptEnumOptionResponse> LensDesigns { get; set; } = new();
        public List<OptEnumOptionResponse> LensSupplies { get; set; } = new();
        public List<OptEnumOptionResponse> WorkshopTypes { get; set; } = new();
        public List<OptEnumOptionResponse> OrderKinds { get; set; } = new();
        public List<OptEnumOptionResponse> OrderStatuses { get; set; } = new();
        public List<OptEnumOptionResponse> OrderItemStatuses { get; set; } = new();
        public List<OptEnumOptionResponse> RefractionExamStatuses { get; set; } = new();
        public List<OptEnumOptionResponse> PrescriptionTypes { get; set; } = new();
        public List<OptEnumOptionResponse> PrescriptionOrigins { get; set; } = new();
        public List<OptEnumOptionResponse> CancellationStatuses { get; set; } = new();
        public List<OptEnumOptionResponse> ComplaintTypes { get; set; } = new();
        public List<OptEnumOptionResponse> WarrantyClaimStatuses { get; set; } = new();
    }

    /// <summary>Penyaring katalog barang Optik.</summary>
    public class OptItemCatalogQuery
    {
        /// <summary>Mencari kode barang, nama barang, atau merk.</summary>
        public string? Search { get; set; }
        public OptItemCategory? ItemCategory { get; set; }
        /// <summary>Warna frame, cocok sebagian, contoh "hitam".</summary>
        public string? Color { get; set; }
        /// <summary>Ukuran frame, cocok sebagian, contoh "52".</summary>
        public string? FrameSize { get; set; }
        public decimal? LensIndex { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Satu barang Optik beserta stok tersedianya di depo Optik.</summary>
    public class OptItemProfileResponse
    {
        public Guid ProfileId { get; set; }
        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        /// <summary>Merk dari master barang (<c>MstDrug.BrandName</c>); tidak digandakan di profil Optik.</summary>
        public string? BrandName { get; set; }
        public OptItemCategory ItemCategory { get; set; }
        public string ItemCategoryLabel { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? FrameSize { get; set; }
        public string? FrameMaterial { get; set; }
        public decimal? LensIndex { get; set; }
        public OptLensDesign? LensDesign { get; set; }
        public string? LensDesignLabel { get; set; }
        public string? LensCoating { get; set; }
        public decimal? ContactLensBaseCurve { get; set; }
        public decimal? ContactLensDiameter { get; set; }
        public string? ContactLensReplacement { get; set; }
        public bool IsActive { get; set; }
        /// <summary>Barang di master sudah tidak aktif; profil tidak bisa dipakai pesanan baru.</summary>
        public bool IsDrugActive { get; set; }
        /// <summary>Jumlah siap dilayankan di depo Optik; <c>null</c> bila depo Optik belum terdaftar.</summary>
        public decimal? AvailableQuantity { get; set; }
    }

    /// <summary>Buat atau ubah profil Optik untuk satu barang (<c>OPT-DEC-031</c>).</summary>
    public class OptItemProfileUpsertRequest
    {
        [Required]
        public OptItemCategory? ItemCategory { get; set; }
        [MaxLength(50)] public string? Color { get; set; }
        [MaxLength(20)] public string? FrameSize { get; set; }
        [MaxLength(50)] public string? FrameMaterial { get; set; }
        [Range(typeof(decimal), "0.01", "99.99")] public decimal? LensIndex { get; set; }
        public OptLensDesign? LensDesign { get; set; }
        [MaxLength(100)] public string? LensCoating { get; set; }
        [Range(typeof(decimal), "0.01", "99.99")] public decimal? ContactLensBaseCurve { get; set; }
        [Range(typeof(decimal), "0.01", "99.99")] public decimal? ContactLensDiameter { get; set; }
        [MaxLength(50)] public string? ContactLensReplacement { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Pengaturan unit Optik.</summary>
    public class OptSettingResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;
        public decimal DownPaymentMinPercent { get; set; }
        public int PickupReminderDays { get; set; }
        public int NotCollectedCloseDays { get; set; }
        public int PrescriptionValidityMonths { get; set; }
        public int WarrantyFitDays { get; set; }
        public int WarrantyLensDefectMonths { get; set; }
        public int WarrantyFrameDefectMonths { get; set; }
        public decimal ContactLensBaseCurveMin { get; set; }
        public decimal ContactLensBaseCurveMax { get; set; }
        public decimal ContactLensDiameterMin { get; set; }
        public decimal ContactLensDiameterMax { get; set; }
        /// <summary>Kirim kembali sebagai <c>ExpectedVersion</c> saat mengubah.</summary>
        public int Version { get; set; }
        public List<OptSettingRevisionResponse> Revisions { get; set; } = new();
    }

    /// <summary>Satu perubahan pengaturan, nilai lama → baru.</summary>
    public class OptSettingRevisionResponse
    {
        public string ChangedFieldsSummary { get; set; } = string.Empty;
        public Guid ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    /// <summary>
    /// Ubah pengaturan unit Optik. Berlaku ke depan: pesanan, resep, dan garansi yang sudah dibuat memakai nilai
    /// yang dibekukan saat dibuat (<c>OPT-DEC-009</c>, <c>OPT-DEC-040</c>, <c>OPT-DEC-041</c>).
    /// </summary>
    public class OptSettingUpdateRequest
    {
        public Guid? ServiceUnitId { get; set; }
        public decimal DownPaymentMinPercent { get; set; }
        public int PickupReminderDays { get; set; }
        public int NotCollectedCloseDays { get; set; }
        public int PrescriptionValidityMonths { get; set; }
        public int WarrantyFitDays { get; set; }
        public int WarrantyLensDefectMonths { get; set; }
        public int WarrantyFrameDefectMonths { get; set; }
        public decimal ContactLensBaseCurveMin { get; set; }
        public decimal ContactLensBaseCurveMax { get; set; }
        public decimal ContactLensDiameterMin { get; set; }
        public decimal ContactLensDiameterMax { get; set; }
        /// <summary>Nilai <c>Version</c> terakhir yang dibaca; berbeda → <c>409</c>.</summary>
        [Required]
        public int? ExpectedVersion { get; set; }
    }
}
