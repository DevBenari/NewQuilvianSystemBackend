using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstClinic", Schema = "public")]
    public class MstClinic : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ClinicCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ClinicName { get; set; } = string.Empty;

        public ClinicType ClinicType { get; set; } = ClinicType.Unknown;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(50)]
        public string? RoomName { get; set; }

        public bool IsAvailableForRegistration { get; set; } = true;

        public bool IsAvailableForKiosk { get; set; } = true;

        public bool IsAvailableForAppointment { get; set; } = true;

        public bool IsDoctorRequired { get; set; } = true;

        public bool IsScreeningRequired { get; set; } = true;

        public bool IsQueueRequired { get; set; } = true;

        public int DefaultEstimatedServiceMinutes { get; set; } = 15;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstServiceUnit? ServiceUnit { get; set; }
    }
}
