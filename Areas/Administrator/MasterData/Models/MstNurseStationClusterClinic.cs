using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstNurseStationClusterClinic", Schema = "public")]
    public class MstNurseStationClusterClinic : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NurseStationClusterId { get; set; }

        [Required]
        public Guid ClinicId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstNurseStationCluster? NurseStationCluster { get; set; }

        public MstClinic? Clinic { get; set; }
    }

}
