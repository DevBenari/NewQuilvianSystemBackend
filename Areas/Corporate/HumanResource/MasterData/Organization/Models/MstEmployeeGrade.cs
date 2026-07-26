using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models
{
    [Table("MstEmployeeGrade", Schema = "public")]
    public class MstEmployeeGrade : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? JobLevelId { get; set; }

        [Required]
        [MaxLength(50)]
        public string GradeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string GradeName { get; set; } = string.Empty;

        public int GradeOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstJobLevel? JobLevel { get; set; }
    }
}
