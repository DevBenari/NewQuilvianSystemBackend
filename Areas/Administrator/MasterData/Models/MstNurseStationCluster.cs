using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstNurseStationCluster", Schema = "public")]
    public class MstNurseStationCluster : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ClusterCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ClusterName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(50)]
        public string? RoomName { get; set; }

        public bool IsAvailableForRegistrationQueue { get; set; } = true;

        public bool IsAvailableForScreening { get; set; } = true;

        public bool IsAvailableForDisplay { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstServiceUnit? ServiceUnit { get; set; }
    }

}
