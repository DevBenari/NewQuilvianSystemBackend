using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Employee.Models
{
    [Table("EmpOrganizationAssignment", Schema = "public")]
    public class EmpOrganizationAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public MstEmployee? Employee { get; set; }

        public MstDepartment? Department { get; set; }

        public MstPosition? Position { get; set; }
    }
}
