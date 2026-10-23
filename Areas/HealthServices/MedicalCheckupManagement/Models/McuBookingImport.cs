using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Satu kali impor booking rombongan perusahaan. Hanya nama berkas yang disimpan, isinya tidak.
    /// </summary>
    [Table("McuBookingImport", Schema = "public")]
    public class McuBookingImport : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary><c>MCU-IMP-000001</c>, dari seri nomor.</summary>
        [Required]
        [MaxLength(30)]
        public string ImportNumber { get; set; } = string.Empty;

        [Required]
        public Guid CompanyGuarantorId { get; set; }

        public MstCompanyGuarantor? CompanyGuarantor { get; set; }

        [Required]
        public Guid PackageId { get; set; }

        public McuPackage? Package { get; set; }

        public DateOnly ExaminationDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        public int TotalRows { get; set; }

        public int CreatedRows { get; set; }

        public int FailedRows { get; set; }

        /// <summary>SENSITIF. Nomor baris dan sebab gagal; dapat memuat nama peserta.</summary>
        [MaxLength(4000)]
        public string? FailureSummary { get; set; }
    }
}
