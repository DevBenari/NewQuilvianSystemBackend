using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs
{
    // =====================================================================
    // MCU-API-v1 — grup Mcu Package (BE-MCU-005)
    // =====================================================================

    public class McuPackagePagedQuery
    {
        /// <summary>Mencari kode atau nama paket.</summary>
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Pilihan paket aktif untuk booking.</summary>
    public class McuPackageOptionsQuery
    {
        /// <summary>Perusahaan penjamin; kosong = peserta perorangan, harga umum.</summary>
        public Guid? CompanyGuarantorId { get; set; }

        /// <summary>Tanggal pemeriksaan; kosong = hari ini (WIB).</summary>
        public DateOnly? Date { get; set; }
    }

    public class McuPackageListResponse
    {
        public Guid Id { get; set; }
        public string PackageCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public Guid GeneralTariffId { get; set; }
        public string? GeneralTariffName { get; set; }
        public decimal GeneralPrice { get; set; }
        public int ComponentCount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public bool IsActive { get; set; }
    }

    public class McuPackageOptionResponse
    {
        public Guid Id { get; set; }
        public string PackageCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public Guid TariffId { get; set; }

        /// <summary>Harga yang berlaku pada tanggal pemeriksaan.</summary>
        public decimal Price { get; set; }

        /// <summary><c>General</c> (harga umum) atau <c>Company</c> (harga perusahaan).</summary>
        public string PriceSource { get; set; } = string.Empty;
        public string PriceSourceLabel { get; set; } = string.Empty;
        public Guid? CompanyPriceId { get; set; }
        public DateOnly? EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }

        /// <summary>Persyaratan yang disampaikan admisi kepada peserta (<c>MCU-DEC-013</c>).</summary>
        public List<McuPackageOptionRequirementResponse> Requirements { get; set; } = new();
    }

    public class McuPackageOptionRequirementResponse
    {
        public string ComponentName { get; set; } = string.Empty;
        public string RequirementText { get; set; } = string.Empty;
        public bool IsConfirmationRequired { get; set; }
    }

    public class McuPackageDetailResponse
    {
        public Guid Id { get; set; }
        public string PackageCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid GeneralTariffId { get; set; }
        public string? GeneralTariffCode { get; set; }
        public string? GeneralTariffName { get; set; }
        public decimal GeneralPrice { get; set; }
        public bool IsActive { get; set; }

        /// <summary>Jumlah nilai pengembalian seluruh komponen; tidak pernah melebihi <see cref="GeneralPrice"/>.</summary>
        public decimal TotalRefundAmount { get; set; }
        public List<McuPackageComponentResponse> Components { get; set; } = new();
        public List<McuCompanyPriceResponse> CompanyPrices { get; set; } = new();
    }

    public class McuPackageComponentResponse
    {
        public Guid Id { get; set; }
        public McuComponentType ComponentType { get; set; }
        public string ComponentTypeLabel { get; set; } = string.Empty;

        /// <summary>Nama tampil: nama prosedur (Lab/Rad) atau nama jenis Pemeriksaan MCU.</summary>
        public string ComponentName { get; set; } = string.Empty;
        public Guid? ProcedureId { get; set; }
        public string? ProcedureCode { get; set; }
        public Guid? RadModalityId { get; set; }
        public string? RadModalityName { get; set; }
        public Guid? ExaminationTypeId { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal MedicalFeeAmount { get; set; }
        public List<McuPackageRequirementResponse> Requirements { get; set; } = new();
    }

    public class McuPackageRequirementResponse
    {
        public Guid Id { get; set; }
        public string RequirementText { get; set; } = string.Empty;
        public bool IsConfirmationRequired { get; set; }
    }

    public class McuCompanyPriceResponse
    {
        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public Guid CompanyGuarantorId { get; set; }
        public string? CompanyGuarantorName { get; set; }
        public Guid TariffId { get; set; }
        public string? TariffName { get; set; }
        public decimal Price { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }

        /// <summary>Berlaku pada hari ini (WIB).</summary>
        public bool IsCurrentlyEffective { get; set; }
    }

    public class CreateMcuPackageRequest
    {
        /// <summary>Huruf besar tanpa spasi, contoh <c>MCU-BASIC</c>; disimpan dalam huruf besar.</summary>
        public string PackageCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Tarif harga umum; wajib tarif paket aktif (<c>VAL-MCU-03</c>).</summary>
        public Guid GeneralTariffId { get; set; }
        public List<McuPackageComponentRequest> Components { get; set; } = new();
    }

    /// <summary>
    /// Isi paket lengkap. Komponen yang tidak dikirim lagi dihapus dari paket; order yang sudah
    /// memakainya tetap terbaca karena penghapusannya berupa penandaan.
    /// </summary>
    public class UpdateMcuPackageRequest : CreateMcuPackageRequest
    {
    }

    public class McuPackageComponentRequest
    {
        /// <summary>
        /// Komponen tersimpan yang dipertahankan. Kosong = komponen baru. Bila jenis atau
        /// pemeriksaannya diganti, komponen lama dihapus dan komponen baru dibuat.
        /// </summary>
        public Guid? Id { get; set; }
        public McuComponentType ComponentType { get; set; }

        /// <summary>Wajib untuk Laboratorium dan Radiologi.</summary>
        public Guid? ProcedureId { get; set; }

        /// <summary>Wajib untuk Radiologi.</summary>
        public Guid? RadModalityId { get; set; }

        /// <summary>Wajib untuk Pemeriksaan MCU.</summary>
        public Guid? ExaminationTypeId { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal MedicalFeeAmount { get; set; }
        public List<McuPackageRequirementRequest> Requirements { get; set; } = new();
    }

    public class McuPackageRequirementRequest
    {
        public string RequirementText { get; set; } = string.Empty;
        public bool IsConfirmationRequired { get; set; } = true;
    }

    public class McuPackageStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class McuPackageStatusResponse
    {
        public Guid Id { get; set; }
        public string PackageCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateMcuCompanyPriceRequest
    {
        public Guid CompanyGuarantorId { get; set; }

        /// <summary>Tarif khusus; wajib tarif paket aktif (<c>VAL-MCU-03</c>).</summary>
        public Guid TariffId { get; set; }
        public DateOnly EffectiveStartDate { get; set; }

        /// <summary>Kosong = tanpa batas.</summary>
        public DateOnly? EffectiveEndDate { get; set; }
    }

    public class EndMcuCompanyPriceRequest
    {
        /// <summary>Hari terakhir harga berlaku (inklusif).</summary>
        public DateOnly EffectiveEndDate { get; set; }
    }

    public class McuCompanyPriceEndResponse
    {
        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly EffectiveEndDate { get; set; }
    }

    public class McuPackageFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public List<McuOptionItemResponse> StatusOptions { get; set; } = new();
        public List<McuOptionItemResponse> ComponentTypeOptions { get; set; } = new();
        public List<McuOptionItemResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class McuOptionItemResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
