using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs
{
    // =========================================================================
    // Request DTOs
    // =========================================================================

    /// <summary>
    /// Permintaan pengajuan penyesuaian/koreksi hasil jasa medis berjenjang (MDF-DES-017, MF-DEC-007, MDF-VAL-1.0 §6).
    /// </summary>
    public class CreateAdjustmentRequest
    {
        [Required(ErrorMessage = "Arah koreksi wajib diisi ('Addition' atau 'Deduction').")]
        [RegularExpression("^(Addition|Deduction)$", ErrorMessage = "Arah koreksi hanya boleh bernilai 'Addition' atau 'Deduction' (MDF-VAL-1.0 §6.5).")]
        public string Direction { get; set; } = MdfAdjustmentDirections.Addition;

        [Required(ErrorMessage = "Nominal koreksi wajib diisi.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Nominal koreksi harus lebih besar dari 0 (MDF-VAL-1.0 §6.6).")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Alasan koreksi wajib diisi.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Alasan koreksi wajib diisi antara 10 sampai 500 karakter (MDF-VAL-1.0 §6.7).")]
        public string Reason { get; set; } = string.Empty;

        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>
    /// Permintaan persetujuan penyesuaian koreksi jasa medis oleh pejabat berwenang (Maker-Checker).
    /// </summary>
    public class ApproveAdjustmentRequest
    {
        public Guid? ExpectedRowVersion { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Permintaan penolakan pengajuan penyesuaian koreksi jasa medis dengan alasan wajib (MDF-VAL-1.0 §6.10).
    /// </summary>
    public class RejectAdjustmentRequest
    {
        [Required(ErrorMessage = "Alasan penolakan koreksi wajib diisi.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Alasan penolakan koreksi wajib diisi antara 10 sampai 500 karakter (MDF-VAL-1.0 §6.10).")]
        public string RejectionReason { get; set; } = string.Empty;

        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>
    /// Parameter pencarian dan filter riwayat penyesuaian koreksi hasil jasa medis.
    /// </summary>
    public class AdjustmentPagedQuery
    {
        public Guid? ServiceFeeId { get; set; }
        public string? Status { get; set; }
        public string? Direction { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; } = "RequestedAt";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    // =========================================================================
    // Response DTOs
    // =========================================================================

    /// <summary>
    /// Rincian data pengajuan penyesuaian koreksi jasa medis beserta stempel audit dan nilai terkini.
    /// </summary>
    public class ServiceFeeAdjustmentResponse
    {
        public Guid Id { get; set; }
        public string AdjustmentNumber { get; set; } = string.Empty;
        public Guid ServiceFeeId { get; set; }
        public string FeeNumber { get; set; } = string.Empty;
        public string Direction { get; set; } = MdfAdjustmentDirections.Addition;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = MdfAdjustmentStatuses.Pending;
        public Guid RequestedBy { get; set; }
        public string RequestedByName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public Guid RowVersion { get; set; }

        public decimal CurrentGrossAmount { get; set; }
        public decimal CurrentAdjustmentAmount { get; set; }
        public decimal CurrentFinalAmount { get; set; }
    }

    /// <summary>
    /// Metadata konfigurasi filter dan opsi status pengajuan koreksi jasa medis.
    /// </summary>
    public class AdjustmentFilterMetadataResponse
    {
        public List<string> StatusOptions { get; set; } = new()
        {
            MdfAdjustmentStatuses.Pending,
            MdfAdjustmentStatuses.Approved,
            MdfAdjustmentStatuses.Rejected
        };

        public List<string> DirectionOptions { get; set; } = new()
        {
            MdfAdjustmentDirections.Addition,
            MdfAdjustmentDirections.Deduction
        };

        public List<string> SortOptions { get; set; } = new()
        {
            "RequestedAt",
            "Amount",
            "AdjustmentNumber",
            "Status"
        };

        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
    }
}

