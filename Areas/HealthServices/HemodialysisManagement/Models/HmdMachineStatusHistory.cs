using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Satu perpindahan status mesin beserta alasan dan pelakunya (<c>FEAT-027</c>). Tidak ada
    /// perpindahan status mesin yang boleh terjadi tanpa baris ini.
    /// </summary>
    [Table("HmdMachineStatusHistory", Schema = "public")]
    public class HmdMachineStatusHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MachineId { get; set; }

        public HmdMachine? Machine { get; set; }

        /// <summary>Kosong pada baris pertama, saat mesin didaftarkan.</summary>
        public HmdMachineStatus? FromStatus { get; set; }

        public HmdMachineStatus ToStatus { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public Guid ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
