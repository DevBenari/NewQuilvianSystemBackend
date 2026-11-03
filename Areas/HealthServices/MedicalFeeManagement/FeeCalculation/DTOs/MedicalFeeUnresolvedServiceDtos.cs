using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs
{
    // =========================================================================
    // Request DTOs
    // =========================================================================

    /// <summary>
    /// Parameter pencarian, penyaringan, dan pengurutan daftar layanan belum dapat dihitung (MdfUnresolvedService).
    /// </summary>
    public class UnresolvedServicePagedQuery
    {
        public Guid? PeriodId { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public string? SourceDomain { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; } = "DetectedAt";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Permintaan pengesampingan layanan belum dapat dihitung (MDF-DES-014, MDF-VAL-1.0 §7.1).
    /// Menuntut catatan resmi pengesampingan minimal 10 karakter.
    /// </summary>
    public class WaiveUnresolvedServiceRequest
    {
        [Required(ErrorMessage = "Catatan pengesampingan (ResolutionNote) wajib diisi.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Catatan pengesampingan harus 10-500 karakter.")]
        public string ResolutionNote { get; set; } = string.Empty;
    }

    // =========================================================================
    // Response DTOs
    // =========================================================================

    /// <summary>
    /// Data item layanan belum dapat dihitung beserta alasan dan referensi sumbernya.
    /// </summary>
    public class UnresolvedServiceItemResponse
    {
        public Guid Id { get; set; }
        public Guid PeriodId { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string SourceDomain { get; set; } = string.Empty;
        public string SourceDetailId { get; set; } = string.Empty;
        public Guid? InvoiceItemId { get; set; }
        public Guid? TariffId { get; set; }
        public string? TariffName { get; set; }
        public DateOnly ServiceDate { get; set; }
        public decimal? BaseAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? ReasonDetail { get; set; }
        public DateTime DetectedAt { get; set; }
        public string Status { get; set; } = MdfUnresolvedStatuses.Open;
        public Guid? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }
    }

    /// <summary>
    /// Ringkasan statistik dan rekapitulasi layanan belum dapat dihitung per alasan dan per modul sumber.
    /// </summary>
    public class UnresolvedSummaryResponse
    {
        public int Total { get; set; }
        public int OpenCount { get; set; }
        public int ResolvedCount { get; set; }
        public int WaivedCount { get; set; }
        public Dictionary<string, int> ByReason { get; set; } = new();
        public Dictionary<string, int> BySourceDomain { get; set; } = new();
    }

    /// <summary>
    /// Metadata konfigurasi filter, sort, dan pagination untuk tabel frontend layanan unresolved.
    /// </summary>
    public class UnresolvedFilterMetadataResponse
    {
        public UnresolvedDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<UnresolvedSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> StatusOptions { get; set; } = new()
        {
            MdfUnresolvedStatuses.Open,
            MdfUnresolvedStatuses.Resolved,
            MdfUnresolvedStatuses.Waived
        };
        public List<string> ReasonOptions { get; set; } = new()
        {
            MdfUnresolvedReasons.PerformerMissing,
            MdfUnresolvedReasons.AgreementMissing,
            MdfUnresolvedReasons.RuleMissing,
            MdfUnresolvedReasons.ContractExpired,
            MdfUnresolvedReasons.RoleUnmapped,
            MdfUnresolvedReasons.SourceUnsupported
        };
        public List<string> SourceDomainOptions { get; set; } = new()
        {
            MdfServiceSourceDomains.OperatingRoom,
            MdfServiceSourceDomains.Procedure,
            MdfServiceSourceDomains.Laboratory,
            MdfServiceSourceDomains.Adhoc,
            MdfServiceSourceDomains.AdhocCatalog
        };
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class UnresolvedDefaultFilterResponse
    {
        public string SortBy { get; set; } = "DetectedAt";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class UnresolvedSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
