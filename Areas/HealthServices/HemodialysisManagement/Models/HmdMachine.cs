using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Mesin cuci darah beserta status laik pakainya — master milik modul.
    /// </summary>
    /// <remarks>
    /// Diberi prefix <c>Hmd</c>, bukan <c>Mst</c>, karena mesin HD hanya bermakna di unit HD
    /// (<c>02-backend-architecture.md</c> bagian 10 butir 1). Setiap perpindahan status menulis
    /// satu baris <see cref="HmdMachineStatusHistory"/>.
    /// </remarks>
    [Table("HmdMachine", Schema = "public")]
    public class HmdMachine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kode mesin sesuai label fisik, unik per unit.</summary>
        [Required]
        [MaxLength(30)]
        public string MachineCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string MachineName { get; set; } = string.Empty;

        [Required]
        public Guid ServiceUnitId { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        [MaxLength(150)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        public HmdMachineStatus MachineStatus { get; set; } = HmdMachineStatus.Ready;

        /// <summary>Mesin khusus, misalnya khusus pasien Hepatitis B.</summary>
        public HmdIsolationRequirement DedicatedFor { get; set; } = HmdIsolationRequirement.None;

        public bool IsSchedulable { get; set; } = true;

        public DateTime? LastStatusChangedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int Version { get; set; }

        public ICollection<HmdMachineStatusHistory> StatusHistories { get; set; } = new List<HmdMachineStatusHistory>();
    }
}
