using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models
{
    [Table("NvwDisplayDevice", Schema = "public")]
    public class NvwDisplayDevice : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string DeviceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DeviceName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string FloorCode { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? LocationName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public NvwDisplayDeviceStatus DeviceStatus { get; set; } = NvwDisplayDeviceStatus.PendingLogin;

        public Guid? LoginUserId { get; set; }

        public ApplicationUser? LoginUser { get; set; }

        public DateTime? LoginIssuedAt { get; set; }

        public Guid? LoginIssuedBy { get; set; }

        public DateTime? LastOnlineAt { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        public Guid? DeactivatedBy { get; set; }

        [MaxLength(500)]
        public string? DeactivationReason { get; set; }

        public int Version { get; set; }
    }
}
