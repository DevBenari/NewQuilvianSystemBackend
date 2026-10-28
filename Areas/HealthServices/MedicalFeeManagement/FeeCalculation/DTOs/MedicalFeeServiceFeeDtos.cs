using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs
{
    // =========================================================================
    // Request DTOs
    // =========================================================================

    /// <summary>
    /// Parameter pencarian, filter, dan pengurutan daftar hasil perhitungan jasa medis (MdfServiceFee).
    /// </summary>
    public class ServiceFeePagedQuery
    {
        public Guid? PeriodId { get; set; }
        public string? PayeeType { get; set; }
        public Guid? PayeeReferenceId { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; } = "GrossAmount";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Permintaan verifikasi satu hasil jasa medis penerima (MDF-STATE-1.0 §2).
    /// </summary>
    public class VerifyServiceFeeRequest
    {
        public Guid? ExpectedRowVersion { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Permintaan pengembalian satu hasil jasa medis dari Verified ke Calculated (MDF-STATE-1.0 §2).
    /// </summary>
    public class ReturnServiceFeeRequest
    {
        public Guid? ExpectedRowVersion { get; set; }

        [Required(ErrorMessage = "Alasan pengembalian hasil jasa wajib diisi.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Alasan pengembalian harus 3-500 karakter.")]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permintaan verifikasi massal seluruh hasil jasa pada satu periode kalkulasi.
    /// </summary>
    public class BatchVerifyServiceFeesRequest
    {
        [Required(ErrorMessage = "ID Periode wajib diisi.")]
        public Guid PeriodId { get; set; }
    }

    // =========================================================================
    // Response DTOs
    // =========================================================================

    /// <summary>
    /// Ringkasan hasil perhitungan jasa medis satu penerima (dokter/staf) untuk satu periode.
    /// </summary>
    public class ServiceFeeHeaderResponse
    {
        public Guid Id { get; set; }
        public string FeeNumber { get; set; } = string.Empty;
        public Guid PeriodId { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PayeeType { get; set; } = MdfPayeeTypes.Doctor;
        public Guid PayeeReferenceId { get; set; }
        public string PayeeName { get; set; } = string.Empty;
        public decimal GrossAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Status { get; set; } = MdfServiceFeeStatuses.Calculated;
        public DateTime CalculatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public int DetailCount { get; set; }
        public int AdjustmentCount { get; set; }
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Rincian perhitungan per baris tindakan/layanan medis (MDF-API-1.0 §5) — "Dari mana angkanya".
    /// </summary>
    public class ServiceFeeDetailBreakdownResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceFeeId { get; set; }
        public string SourceDomain { get; set; } = string.Empty;
        public string SourceDetailId { get; set; } = string.Empty;
        public Guid? InvoiceItemId { get; set; }
        public DateOnly ServiceDate { get; set; }
        public Guid? TariffId { get; set; }
        public string? TariffName { get; set; }
        public Guid RoleId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string? RoleName { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal SharingPercentage { get; set; }
        public decimal CalculatedAmount { get; set; }
        public Guid SharingRuleId { get; set; }
        public string AgreementNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Hasil verifikasi massal hasil jasa pada satu periode.
    /// </summary>
    public class BatchVerifyResultResponse
    {
        public Guid PeriodId { get; set; }
        public int TotalFees { get; set; }
        public int VerifiedCount { get; set; }
        public int AlreadyVerifiedCount { get; set; }
    }

    /// <summary>
    /// Metadata konfigurasi filter dan opsi pengurutan tabel hasil jasa medis.
    /// </summary>
    public class ServiceFeeFilterMetadataResponse
    {
        public ServiceFeeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ServiceFeeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> StatusOptions { get; set; } = new()
        {
            MdfServiceFeeStatuses.Calculated,
            MdfServiceFeeStatuses.Verified,
            MdfServiceFeeStatuses.Approved,
            MdfServiceFeeStatuses.HandedOff
        };
        public List<string> PayeeTypeOptions { get; set; } = new()
        {
            MdfPayeeTypes.Doctor,
            MdfPayeeTypes.Workforce
        };
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class ServiceFeeDefaultFilterResponse
    {
        public string SortBy { get; set; } = "GrossAmount";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ServiceFeeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Key { get => Value; set => Value = value; }
        public string Label { get; set; } = string.Empty;
    }
}
