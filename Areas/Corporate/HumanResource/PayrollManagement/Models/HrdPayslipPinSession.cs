using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Sesi gaji sensitif 15 menit yang terbit sesudah PIN sekunder slip gaji terverifikasi (VR-PAY-22, BE-HRD-159).
    /// </summary>
    /// <remarks>
    /// Hanya hash SHA-256 dari token yang disimpan; token mentahnya cuma dipegang peramban pegawai.
    /// Sebelum 10 November 2026 sesi disimpan di memori proses dan hilang saat restart.
    /// </remarks>
    [Table("HrdPayslipPinSession", Schema = "public")]
    public class HrdPayslipPinSession : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        public Guid EmployeeKey { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }
    }
}
