using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstRoom", Schema = "public")]
    public class MstRoom : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? PatientClassId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoomCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RoomName { get; set; } = string.Empty;

        public RoomType RoomType { get; set; } = RoomType.Unknown;

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        public int Capacity { get; set; } = 1;

        public bool IsForMale { get; set; } = true;

        public bool IsForFemale { get; set; } = true;

        public bool IsForNewborn { get; set; } = false;

        public bool IsIsolationRoom { get; set; } = false;

        public bool IsIntensiveCare { get; set; } = false;

        public bool IsOdcRoom { get; set; } = false;

        public bool IsAvailableForAdmission { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstPatientClass? PatientClass { get; set; }
    }
}
