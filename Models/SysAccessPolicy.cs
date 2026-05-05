using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;

namespace QuilvianSystemBackend.Models
{
    [Table("SysAccessPolicy", Schema = "public")]
    public class SysAccessPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }       

        [Required]
        public Guid ControllerAccessId { get; set; }

        [Required]
        public Guid ActionAccessId { get; set; }

        public bool IsAllowed { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public MstDepartment? Department { get; set; }

        public MstPosition? Position { get; set; }

        public SysControllerAccess? ControllerAccess { get; set; }

        public SysActionAccess? ActionAccess { get; set; }
    }
}
