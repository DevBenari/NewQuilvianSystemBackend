using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstBed", Schema = "public")]
    public class MstBed : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RoomId { get; set; }

        [Required]
        [MaxLength(50)]
        public string BedCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string BedName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? BedNumber { get; set; }

        public BedStatus BedStatus { get; set; } = BedStatus.Available;

        public bool IsForMale { get; set; } = true;

        public bool IsForFemale { get; set; } = true;

        public bool IsForNewborn { get; set; } = false;

        public bool IsIsolationBed { get; set; } = false;

        public bool IsIntensiveCareBed { get; set; } = false;

        public bool IsOdcBed { get; set; } = false;

        public bool IsReservable { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstRoom? Room { get; set; }
    }
}
