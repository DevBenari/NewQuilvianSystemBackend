using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Satu komponen paket: pemeriksaan Lab, Radiologi, atau Pemeriksaan MCU.
    /// </summary>
    /// <remarks>
    /// Lab/Rad mengisi <see cref="ProcedureId"/>; Rad juga <see cref="RadModalityId"/>; Pemeriksaan
    /// MCU mengisi <see cref="ExaminationTypeId"/>. Satu prosedur atau satu jenis pemeriksaan hanya
    /// sekali per paket — dijaga unique index.
    /// </remarks>
    [Table("McuPackageComponent", Schema = "public")]
    public class McuPackageComponent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PackageId { get; set; }

        public McuPackage? Package { get; set; }

        public McuComponentType ComponentType { get; set; }

        public Guid? ProcedureId { get; set; }

        public MstProcedure? Procedure { get; set; }

        public Guid? RadModalityId { get; set; }

        public MstRadModality? RadModality { get; set; }

        public Guid? ExaminationTypeId { get; set; }

        public McuExaminationType? ExaminationType { get; set; }

        /// <summary>Nilai pengembalian bila komponen dibatalkan (<c>MCU-DEC-015</c>); ≥ 0.</summary>
        public decimal RefundAmount { get; set; }

        /// <summary>Nilai jasa medis (<c>MCU-DEC-034</c>); ≥ 0.</summary>
        public decimal MedicalFeeAmount { get; set; }

        public ICollection<McuPackageComponentRequirement> Requirements { get; set; } =
            new List<McuPackageComponentRequirement>();
    }
}
