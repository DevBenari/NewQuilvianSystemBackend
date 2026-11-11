using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    /// <summary>
    /// Permintaan penerbitan voucher kasir darurat medis cito nakes 24/7 ber-token SHA-256 (BE-HRD-172, HRD-DEC-133, VR-DED-13).
    /// </summary>
    public class IssueCashierVoucherRequestDto
    {
        [Required]
        public Guid ClaimId { get; set; }

        public string? Notes { get; set; }
    }

    /// <summary>
    /// Respons data voucher pencairan loket kasir darurat RS (BE-HRD-172, BE-HRD-173, DED-B6, DED-W3).
    /// </summary>
    public class CashierVoucherResponseDto
    {
        public Guid Id { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public Guid ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string ClaimCategory { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNik { get; set; }
        public decimal VoucherAmount { get; set; }
        public string? SecurityToken { get; set; } // Plaintext token (hanya ditampilkan saat penerbitan/verifikasi kasir)
        public string SecurityTokenHash { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public string VoucherStatus { get; set; } = "Issued";
        public Guid? CashedByUserId { get; set; }
        public string? CashedByName { get; set; }
        public DateTime? CashedAt { get; set; }
        public string? CashierReceiptNumber { get; set; }
        public string? Notes { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoonWarning { get; set; } // Peringatan DED-W3 (tersisa <= 12 jam / > 36 jam berlalu)
        public double RemainingHours { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permintaan verifikasi token dan validitas voucher di loket kasir RS 24/7 (BE-HRD-173, INT-HR-59).
    /// </summary>
    public class VerifyCashierVoucherRequestDto
    {
        [Required]
        public string VoucherNumber { get; set; } = string.Empty;

        [Required]
        public string SecurityToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permintaan kliring dan pencairan tunai voucher di loket kasir RS 24/7 (BE-HRD-173, DED-B6, VR-DED-06).
    /// </summary>
    public class ClaimCashierVoucherRequestDto
    {
        [Required]
        public string VoucherNumber { get; set; } = string.Empty;

        [Required]
        public string SecurityToken { get; set; } = string.Empty;

        [Required, MinLength(3)]
        public string CashierReceiptNumber { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    /// <summary>
    /// Filter pencarian data voucher kasir darurat.
    /// </summary>
    public class CashierVoucherFilterDto
    {
        public Guid? EmployeeId { get; set; }
        public string? VoucherStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
