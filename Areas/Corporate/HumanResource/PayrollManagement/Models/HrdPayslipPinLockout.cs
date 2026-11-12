using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Penghitung kegagalan PIN sekunder slip gaji dan stempel penguncian 30 menit per pegawai
    /// (VR-PAY-22, PAY-B6, BE-HRD-159).
    /// </summary>
    /// <remarks>
    /// Sebelum 10 November 2026 penghitung ini hanya ada di memori proses: penyerang yang terkunci karena
    /// tiga kali salah PIN bisa langsung mencoba lagi sesudah aplikasi di-restart atau bila permintaannya
    /// jatuh ke instance lain. Satu baris per <see cref="EmployeeKey"/>.
    /// </remarks>
    [Table("HrdPayslipPinLockout", Schema = "public")]
    public class HrdPayslipPinLockout : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>ID pegawai sebagaimana dikirim portal ESS (profil tenaga kerja atau pegawai).</summary>
        public Guid EmployeeKey { get; set; }

        public int FailedAttempts { get; set; }

        public DateTime? LockoutExpiresAt { get; set; }

        public DateTime LastFailedAt { get; set; } = DateTime.UtcNow;
    }
}
