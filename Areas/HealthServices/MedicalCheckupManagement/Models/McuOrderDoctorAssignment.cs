using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Penugasan dokter MCU pada order. Hanya satu yang berlaku (<see cref="EndedAt"/> kosong) —
    /// dijaga unique index; penggantian menutup yang lama dan wajib beralasan (<c>MCU-DEC-040</c>).
    /// </summary>
    [Table("McuOrderDoctorAssignment", Schema = "public")]
    public class McuOrderDoctorAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid McuOrderId { get; set; }

        public McuOrder? McuOrder { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        public MstDoctor? Doctor { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Admisi (check-in) atau kepala unit (ganti dokter).</summary>
        [Required]
        public Guid AssignedByUserId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public DateTime? EndedAt { get; set; }
    }
}
