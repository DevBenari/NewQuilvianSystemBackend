using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstNurseStationClusterStaffClinic", Schema = "public")]
    public class MstNurseStationClusterStaffClinic : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NurseStationClusterStaffId { get; set; }

        [Required]
        public Guid ClinicId { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstNurseStationClusterStaff? NurseStationClusterStaff { get; set; }

        public MstClinic? Clinic { get; set; }
    }
}
