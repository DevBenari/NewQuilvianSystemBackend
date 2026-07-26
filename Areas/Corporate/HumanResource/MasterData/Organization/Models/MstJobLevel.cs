using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models
{
    [Table("MstJobLevel", Schema = "public")]
    public class MstJobLevel : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string JobLevelCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string JobLevelName { get; set; } = string.Empty;

        public int LevelOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstEmployeeGrade> EmployeeGrades { get; set; }
            = new List<MstEmployeeGrade>();
    }
}
