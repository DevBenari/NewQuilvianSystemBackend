using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstNurseStationClusterStaff", Schema = "public")]
    public class MstNurseStationClusterStaff : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NurseStationClusterId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool CanCallQueue { get; set; } = true;

        public bool CanStartScreening { get; set; } = true;

        public bool CanTransferQueue { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstNurseStationCluster? NurseStationCluster { get; set; }

        public MstEmployee? Employee { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public ICollection<MstNurseStationClusterStaffClinic> StaffClinics { get; set; } =
            new List<MstNurseStationClusterStaffClinic>();
    }
}
