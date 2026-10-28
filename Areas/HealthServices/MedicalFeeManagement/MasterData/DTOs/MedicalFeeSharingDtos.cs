using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.DTOs
{
    // =========================================================================
    // Request DTOs — Kesepakatan Tarif Sharing
    // =========================================================================

    /// <summary>
    /// Request untuk menyusun kesepakatan tarif sharing baru — MDF-API-1.0 §3.
    /// </summary>
    public class CreateSharingAgreementRequest
    {
        [Required(ErrorMessage = "Nomor kesepakatan (AgreementNumber) wajib diisi.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "AgreementNumber harus 3-50 karakter.")]
        public string AgreementNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rujukan kontrak HR (SourceContractHistoryId) wajib diisi.")]
        public Guid SourceContractHistoryId { get; set; }

        [Required(ErrorMessage = "Jenis penerima (PayeeType) wajib diisi.")]
        [RegularExpression("^(Doctor|Workforce)$", ErrorMessage = "PayeeType harus 'Doctor' atau 'Workforce'.")]
        public string PayeeType { get; set; } = MdfPayeeTypes.Doctor;

        [Required(ErrorMessage = "Rujukan penerima (PayeeReferenceId) wajib diisi.")]
        public Guid PayeeReferenceId { get; set; }

        [Required(ErrorMessage = "Tanggal awal berlaku (EffectiveStart) wajib diisi.")]
        public DateOnly EffectiveStart { get; set; }

        public DateOnly? EffectiveEnd { get; set; }

        [MaxLength(1000, ErrorMessage = "Catatan maksimal 1000 karakter.")]
        public string? Notes { get; set; }

        /// <summary>Daftar aturan tarif awal yang disertakan saat pembuatan (opsional).</summary>
        public List<CreateSharingRuleItemRequest> Rules { get; set; } = new();
    }

    /// <summary>
    /// Item aturan tarif saat pembuatan kesepakatan baru.
    /// </summary>
    public class CreateSharingRuleItemRequest
    {
        public Guid? TariffId { get; set; }
        public Guid? TariffCategoryId { get; set; }

        [Required(ErrorMessage = "Peran pelaksana (RoleId) wajib diisi.")]
        public Guid RoleId { get; set; }

        [Range(0.00, 100.00, ErrorMessage = "SharingPercentage harus antara 0.00 dan 100.00.")]
        public decimal SharingPercentage { get; set; }

        [Required(ErrorMessage = "Tanggal awal berlaku aturan wajib diisi.")]
        public DateOnly EffectiveStart { get; set; }

        public DateOnly? EffectiveEnd { get; set; }

        [MaxLength(500, ErrorMessage = "Catatan aturan maksimal 500 karakter.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request untuk mengubah kepala kesepakatan tarif sharing.
    /// </summary>
    public class UpdateSharingAgreementRequest
    {
        [Required(ErrorMessage = "Nomor kesepakatan (AgreementNumber) wajib diisi.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "AgreementNumber harus 3-50 karakter.")]
        public string AgreementNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal awal berlaku (EffectiveStart) wajib diisi.")]
        public DateOnly EffectiveStart { get; set; }

        public DateOnly? EffectiveEnd { get; set; }

        [MaxLength(1000, ErrorMessage = "Catatan maksimal 1000 karakter.")]
        public string? Notes { get; set; }

        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>
    /// Request untuk mengaktifkan kesepakatan tarif sharing.
    /// </summary>
    public class ActivateSharingAgreementRequest
    {
        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>
    /// Request untuk menghentikan kesepakatan tarif sharing.
    /// </summary>
    public class TerminateSharingAgreementRequest
    {
        public DateOnly? TerminationDate { get; set; }

        [Required(ErrorMessage = "Alasan penghentian kesepakatan wajib diisi.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Alasan penghentian harus 3-500 karakter.")]
        public string Reason { get; set; } = string.Empty;

        public Guid? ExpectedRowVersion { get; set; }
    }

    // =========================================================================
    // Request DTOs — Baris Aturan Tarif
    // =========================================================================

    /// <summary>
    /// Request untuk menambahkan satu baris aturan tarif baru ke kesepakatan.
    /// </summary>
    public class AddSharingRuleRequest
    {
        public Guid? TariffId { get; set; }
        public Guid? TariffCategoryId { get; set; }

        [Required(ErrorMessage = "Peran pelaksana (RoleId) wajib diisi.")]
        public Guid RoleId { get; set; }

        [Range(0.00, 100.00, ErrorMessage = "SharingPercentage harus antara 0.00 dan 100.00.")]
        public decimal SharingPercentage { get; set; }

        [Required(ErrorMessage = "Tanggal awal berlaku aturan wajib diisi.")]
        public DateOnly EffectiveStart { get; set; }

        public DateOnly? EffectiveEnd { get; set; }

        [MaxLength(500, ErrorMessage = "Catatan aturan maksimal 500 karakter.")]
        public string? Notes { get; set; }

        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>
    /// Request untuk mengganti baris aturan tarif dengan versi baru — MDF-DES-009, MDF-API-1.0 §3.
    /// </summary>
    public class SupersedeSharingRuleRequest
    {
        [Range(0.00, 100.00, ErrorMessage = "SharingPercentage harus antara 0.00 dan 100.00.")]
        public decimal SharingPercentage { get; set; }

        [Required(ErrorMessage = "Tanggal awal berlaku tarif baru (EffectiveStart) wajib diisi.")]
        public DateOnly EffectiveStart { get; set; }

        [MaxLength(500, ErrorMessage = "Alasan perubahan tarif maksimal 500 karakter.")]
        public string? Reason { get; set; }

        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>
    /// Parameter pencarian dan pagination untuk daftar kesepakatan tarif sharing.
    /// </summary>
    public class SharingAgreementPagedQuery
    {
        public string? Search { get; set; }
        public string? PayeeType { get; set; }
        public Guid? PayeeReferenceId { get; set; }
        public string? Status { get; set; }
        public DateOnly? EffectiveOn { get; set; }
        public string? SortBy { get; set; } = "AgreementNumber";
        public string? SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    // =========================================================================
    // Response DTOs
    // =========================================================================

    /// <summary>
    /// Ringkasan kepala kesepakatan tarif sharing untuk tampilan daftar tabel.
    /// </summary>
    public class SharingAgreementHeaderResponse
    {
        public Guid Id { get; set; }
        public string AgreementNumber { get; set; } = string.Empty;
        public Guid SourceContractHistoryId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string PayeeType { get; set; } = string.Empty;
        public Guid PayeeReferenceId { get; set; }
        public string PayeeName { get; set; } = string.Empty;
        public string? PayeeCode { get; set; }
        public DateOnly EffectiveStart { get; set; }
        public DateOnly? EffectiveEnd { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int RulesCount { get; set; }
        public Guid RowVersion { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid UpdateBy { get; set; }
    }

    /// <summary>
    /// Detail lengkap satu kesepakatan tarif sharing beserta seluruh baris aturan tarifnya.
    /// </summary>
    public class SharingAgreementDetailResponse : SharingAgreementHeaderResponse
    {
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public string? ContractStatus { get; set; }
        public List<SharingRuleResponse> Rules { get; set; } = new();
    }

    /// <summary>
    /// Representasi satu baris aturan tarif sharing.
    /// </summary>
    public class SharingRuleResponse
    {
        public Guid Id { get; set; }
        public Guid AgreementId { get; set; }
        public Guid? TariffId { get; set; }
        public string? TariffCode { get; set; }
        public string? TariffName { get; set; }
        public Guid? TariffCategoryId { get; set; }
        public string? TariffCategoryName { get; set; }
        public Guid RoleId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public decimal SharingPercentage { get; set; }
        public DateOnly EffectiveStart { get; set; }
        public DateOnly? EffectiveEnd { get; set; }
        public Guid? SupersededByRuleId { get; set; }
        public bool IsSuperseded => SupersededByRuleId.HasValue;
        public string? Notes { get; set; }

        /// <summary>
        /// Tingkat kekhususan baris aturan (1: Layanan+Kategori, 2: Layanan saja, 3: Kategori saja, 4: Umum).
        /// </summary>
        public int SpecificityLevel
        {
            get
            {
                if (TariffId.HasValue && TariffCategoryId.HasValue) return 1;
                if (TariffId.HasValue) return 2;
                if (TariffCategoryId.HasValue) return 3;
                return 4;
            }
        }
    }

    /// <summary>
    /// Opsi dropdown untuk memilih kesepakatan tarif.
    /// </summary>
    public class SharingAgreementOptionResponse
    {
        public Guid Id { get; set; }
        public string AgreementNumber { get; set; } = string.Empty;
        public string PayeeType { get; set; } = string.Empty;
        public string PayeeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Label => $"{AgreementNumber} — {PayeeName} ({Status})";
        public string Value => Id.ToString();
    }

    /// <summary>
    /// Statistik ringkasan data kesepakatan tarif sharing.
    /// </summary>
    public class SharingAgreementSummaryResponse
    {
        public int TotalAgreements { get; set; }
        public int ActiveAgreements { get; set; }
        public int DraftAgreements { get; set; }
        public int TerminatedAgreements { get; set; }
        public int ExpiredAgreements { get; set; }
        public int DoctorAgreements { get; set; }
        public int WorkforceAgreements { get; set; }
    }

    // =========================================================================
    // Filter Metadata Response DTOs
    // =========================================================================

    public class SharingAgreementFilterMetadataResponse
    {
        public SharingAgreementDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<SharingAgreementSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> StatusOptions { get; set; } = new()
        {
            MdfSharingAgreementStatuses.Draft,
            MdfSharingAgreementStatuses.Active,
            MdfSharingAgreementStatuses.Expired,
            MdfSharingAgreementStatuses.Terminated
        };
        public List<string> PayeeTypeOptions { get; set; } = new()
        {
            MdfPayeeTypes.Doctor,
            MdfPayeeTypes.Workforce
        };
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class SharingAgreementDefaultFilterResponse
    {
        public string SortBy { get; set; } = "AgreementNumber";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string? Status { get; set; }
        public string? PayeeType { get; set; }
    }

    public class SharingAgreementSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
