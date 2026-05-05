using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
{
    [Table("MstPosition", Schema = "public")]
    public class MstPosition : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PositionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PositionName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDepartment? Department { get; set; }
    }
}
