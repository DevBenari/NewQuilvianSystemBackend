using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstServiceUnit", Schema = "public")]
    public class MstServiceUnit : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ServiceUnitCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ServiceUnitName { get; set; } = string.Empty;

        public ServiceUnitType ServiceUnitType { get; set; } = ServiceUnitType.Unknown;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        public bool IsAvailableForRegistration { get; set; } = true;

        public bool IsAvailableForKiosk { get; set; } = false;

        public bool IsAvailableForAppointment { get; set; } = false;

        public bool IsQueueRequired { get; set; } = true;

        public bool IsDoctorRequired { get; set; } = false;

        public bool IsScreeningRequired { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
