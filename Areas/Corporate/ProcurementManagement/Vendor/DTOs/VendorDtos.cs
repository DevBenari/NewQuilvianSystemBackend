using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;

// =====================================================================================
// BE-PBJ-VD-003 — DTO daftar, detail, dan kategori vendor (PBJ-VD-API 1.0 §Vendor).
// Kunci vendor di URL adalah supplierId (MstSupplier.Id). Identitas vendor dibaca dari
// Master Data; nomor rekening utuh tidak pernah dikirim (PBJ-DEC-015).
// =====================================================================================

/// <summary>Nilai status vendor yang dihitung (PBJ-VD-STATE bagian 1).</summary>
public static class VendorStatusValues
{
    public const string Active = "ACTIVE";
    public const string Restricted = "RESTRICTED";
    public const string Blocked = "BLOCKED";
    public const string Inactive = "INACTIVE";

    /// <summary>
    /// Nilai saringan gabungan, bukan status: Dibatasi + Diblokir + Nonaktif di Master Data. Dipakai layar Status Vendor
    /// (FE-PBJ-VD-06; delta BE-PBJ-VD-003 D-10 untuk FE-PBJ-VD-004).
    /// </summary>
    public const string NonActive = "NON_ACTIVE";

    public static string Label(string value) => value switch
    {
        Active => "Aktif",
        Restricted => "Dibatasi",
        Blocked => "Diblokir",
        Inactive => "Nonaktif di Master Data",
        NonActive => "Semua selain Aktif",
        _ => value
    };
}

/// <summary>
/// Keadaan dokumen satu vendor, diambil dari keadaan terburuk dokumen terverifikasi terakhir per jenis
/// (PBJ-VD-STATE bagian 2). Contoh: Izin PBF kedaluwarsa kemarin → <c>EXPIRED</c> walaupun NIB berlaku.
/// </summary>
public static class VendorDocumentStateValues
{
    public const string Expired = "EXPIRED";
    public const string ExpiringSoon = "EXPIRING_SOON";
    public const string Pending = "PENDING";
    public const string Valid = "VALID";
    public const string None = "NONE";

    public static string Label(string value) => value switch
    {
        Expired => "Ada yang kedaluwarsa",
        ExpiringSoon => "Segera kedaluwarsa",
        Pending => "Menunggu verifikasi",
        Valid => "Berlaku",
        None => "Belum ada dokumen",
        _ => value
    };
}

public sealed class VendorQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }

    /// <summary>Cari kode, nama, atau nama resmi vendor.</summary>
    public string? Search { get; set; }

    public Guid? ItemCategoryId { get; set; }

    /// <summary>
    /// <c>ACTIVE</c>, <c>RESTRICTED</c>, <c>BLOCKED</c>, <c>INACTIVE</c>, atau gabungan <c>NON_ACTIVE</c>
    /// (semua selain Aktif).
    /// </summary>
    [MaxLength(20)] public string? VendorStatus { get; set; }

    /// <summary><c>EXPIRED</c>, <c>EXPIRING_SOON</c>, <c>PENDING</c>, <c>VALID</c>, <c>NONE</c>.</summary>
    [MaxLength(20)] public string? DocumentState { get; set; }

    /// <summary>
    /// Nama bank persis seperti di Master Data (tanpa beda huruf besar/kecil), dari <c>bankOptions</c> metadata.
    /// Dipakai layar Rekening Vendor (FE-PBJ-VD-05; delta BE-PBJ-VD-003 untuk FE-PBJ-VD-002).
    /// </summary>
    [MaxLength(100)] public string? BankName { get; set; }

    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>Atur kategori vendor. Contoh: <c>{ "itemCategoryIds": ["…Obat…", "…Alkes…"], "expectedRowVersion": "…" }</c>.</summary>
public sealed class UpdateVendorCategoriesRequest
{
    [Required] public List<Guid> ItemCategoryIds { get; set; } = new();

    /// <summary>Wajib bila profil Pengadaan vendor sudah ada (PBJ-VAL-090); kosong saat profil pertama dibuat.</summary>
    public Guid? ExpectedRowVersion { get; set; }
}

/// <summary>
/// Tetapkan vendor Dibatasi (PBJ-DEC-047). Contoh: <c>{ "reason": "Terlambat kirim tiga kali berturut-turut",
/// "restrictedUntil": "2027-06-30", "expectedRowVersion": "…" }</c>. Tanggal berakhir kosong = sampai dicabut.
/// </summary>
public sealed class RestrictVendorRequest
{
    /// <summary>Wajib (PBJ-VAL-105).</summary>
    [MaxLength(1000)] public string? Reason { get; set; }

    /// <summary>Opsional; tidak boleh sebelum hari ini (PBJ-VAL-105).</summary>
    public DateOnly? RestrictedUntil { get; set; }

    /// <summary>Wajib bila profil Pengadaan vendor sudah ada (PBJ-VAL-090).</summary>
    public Guid? ExpectedRowVersion { get; set; }
}

/// <summary>Cabut status Dibatasi. Alasan wajib minimal 10 karakter (PBJ-VAL-104).</summary>
public sealed class LiftRestrictionRequest
{
    [MaxLength(1000)] public string? Reason { get; set; }
    [Required] public Guid ExpectedRowVersion { get; set; }
}

public sealed class VendorCategoryResponse
{
    public Guid ItemCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class VendorListItemResponse
{
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string VendorStatus { get; set; } = VendorStatusValues.Active;
    public string VendorStatusLabel { get; set; } = string.Empty;
    public List<VendorCategoryResponse> Categories { get; set; } = new();
    public string DocumentState { get; set; } = VendorDocumentStateValues.None;
    public string DocumentStateLabel { get; set; } = string.Empty;
    public int ExpiredDocumentCount { get; set; }
    public int ExpiringSoonDocumentCount { get; set; }
    public decimal? LatestFinalScore { get; set; }
    public DateTimeOffset? LatestEvaluationFinalizedAt { get; set; }
    public bool HasProcurementProfile { get; set; }

    /// <summary>Rekening tersamarkan untuk layar Rekening Vendor (PBJ-DEC-015); nomor utuh tidak pernah dikirim.</summary>
    public string? BankName { get; set; }
    public string? MaskedBankAccount { get; set; }
    public string? BankAccountName { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class VendorDocumentSummaryResponse
{
    public string DocumentState { get; set; } = VendorDocumentStateValues.None;
    public string DocumentStateLabel { get; set; } = string.Empty;
    public int VerifiedDocumentTypeCount { get; set; }
    public int PendingDocumentCount { get; set; }
    public int ExpiredDocumentCount { get; set; }
    public int ExpiringSoonDocumentCount { get; set; }
    public int ExpiryWarningDays { get; set; }
}

/// <summary>Rekening tersamarkan (PBJ-DEC-015). Contoh: <c>maskedBankAccount = "****7890"</c>.</summary>
public sealed class VendorBankAccountResponse
{
    public string? BankName { get; set; }
    public string? MaskedBankAccount { get; set; }
    public string? BankAccountName { get; set; }
    public string BankAccountVerification { get; set; } = "Tidak tersedia di Master Data";
}

public sealed class VendorLatestEvaluationResponse
{
    public Guid EvaluationId { get; set; }
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public int PeriodYear { get; set; }
    public string PeriodType { get; set; } = string.Empty;
    public int PeriodNo { get; set; }
    public decimal? FinalScore { get; set; }
    public string? Recommendation { get; set; }
    public DateTimeOffset? FinalizedAt { get; set; }
}

public sealed class VendorDetailResponse
{
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string SupplierType { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? BusinessLicenseNumber { get; set; }
    public string? ContactPersonName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? CityName { get; set; }
    public string? ProvinceName { get; set; }

    public string VendorStatus { get; set; } = VendorStatusValues.Active;
    public string VendorStatusLabel { get; set; } = string.Empty;
    public bool IsSupplierActive { get; set; }
    public bool IsBlacklisted { get; set; }
    public string? BlacklistReason { get; set; }

    /// <summary>Dibatasi yang masih berlaku hari ini; batas yang sudah lewat dibaca sebagai tidak dibatasi (PBJ-DEC-047).</summary>
    public bool IsRestricted { get; set; }

    public DateOnly? RestrictedUntil { get; set; }
    public string? RestrictionReason { get; set; }
    public DateTimeOffset? RestrictedAt { get; set; }
    public string? RestrictedByName { get; set; }

    /// <summary>Kosong bila vendor belum punya data Pengadaan (profil lahir saat kategori pertama diatur).</summary>
    public Guid? ProfileId { get; set; }

    public Guid? RowVersion { get; set; }
    public List<VendorCategoryResponse> Categories { get; set; } = new();
    public VendorDocumentSummaryResponse Documents { get; set; } = new();
    public VendorBankAccountResponse BankAccount { get; set; } = new();
    public List<VendorLatestEvaluationResponse> LatestEvaluations { get; set; } = new();
}

public sealed class VendorSummaryResponse
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Restricted { get; set; }
    public int Blocked { get; set; }
    public int Inactive { get; set; }

    /// <summary>Jumlah dokumen terverifikasi terakhir per jenis yang sudah kedaluwarsa.</summary>
    public int ExpiredDocuments { get; set; }

    /// <summary>Jumlah dokumen terverifikasi terakhir per jenis yang berakhir dalam <see cref="ExpiryWarningDays"/> hari.</summary>
    public int ExpiringSoonDocuments { get; set; }

    public int ExpiryWarningDays { get; set; }
}

public sealed class VendorValueOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class VendorCategoryOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class VendorDefaultFilterResponse
{
    public Guid? ItemCategoryId { get; set; }
    public string? VendorStatus { get; set; }
    public string? DocumentState { get; set; }
    public string? BankName { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class VendorFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public VendorDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<VendorValueOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<VendorValueOptionResponse> VendorStatusOptions { get; set; } = new();
    public List<VendorValueOptionResponse> DocumentStateOptions { get; set; } = new();
    public List<VendorCategoryOptionResponse> ItemCategoryOptions { get; set; } = new();

    /// <summary>Nama bank yang tercatat pada supplier Master Data (urut abjad), untuk saringan <c>bankName</c>.</summary>
    public List<VendorValueOptionResponse> BankOptions { get; set; } = new();
    public int ExpiryWarningDays { get; set; }
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
