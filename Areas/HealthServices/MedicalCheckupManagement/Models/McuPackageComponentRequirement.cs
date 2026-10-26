using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Persyaratan sebuah komponen, contoh <i>Puasa 10–12 jam sebelum pengambilan darah</i>.
    /// Yang wajib dikonfirmasi ditandai petugas saat check-in.
    /// </summary>
    [Table("McuPackageComponentRequirement", Schema = "public")]
    public class McuPackageComponentRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PackageComponentId { get; set; }

        public McuPackageComponent? PackageComponent { get; set; }

        [Required]
        [MaxLength(300)]
        public string RequirementText { get; set; } = string.Empty;

        public bool IsConfirmationRequired { get; set; } = true;
    }
}
