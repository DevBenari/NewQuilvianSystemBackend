using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Voucher pencairan tunai kasir darurat cito nakes 24/7 ber-token SHA-256 kedaluwarsa 48 jam (BE-HRD-161, BE-HRD-172, BE-HRD-173, HRD-DEC-133, DED-B6).
    /// </summary>
    [Table("TrxCashierDisbursementVoucher", Schema = "public")]
    public class TrxCashierDisbursementVoucher : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string VoucherNumber { get; set; } = string.Empty;

        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VoucherAmount { get; set; }

        [Required, MaxLength(256)]
        public string SecurityTokenHash { get; set; } = string.Empty; // SHA-256 token satu kali pakai untuk kasir

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiredAt { get; set; } // Berlaku 48 jam sejak terbit

        [Required, MaxLength(50)]
        public string VoucherStatus { get; set; } = "Issued";
        // 'Issued', 'ClaimedAtCashier', 'DisbursedViaCashier', 'Expired', 'Cancelled'

        public Guid? CashedByUserId { get; set; } // ID User Kasir RS yang memverifikasi voucher

        public DateTime? CashedAt { get; set; }

        [MaxLength(100)]
        public string? CashierReceiptNumber { get; set; }

        public string? Notes { get; set; }

        public TrxEmployeeRestitutionClaim? Claim { get; set; }
        public MstEmployee? Employee { get; set; }
        public ApplicationUser? CashedByUser { get; set; }
    }
}
