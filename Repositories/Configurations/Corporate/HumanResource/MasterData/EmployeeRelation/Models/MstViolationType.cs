using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models
{
    [Table("MstViolationType", Schema = "public")]
    public class MstViolationType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ViolationTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ViolationTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string ViolationCategory { get; set; } = "General";

        [Required]
        [MaxLength(40)]
        public string SeverityLevel { get; set; } = "Minor";

        public bool RequiresInvestigation { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
