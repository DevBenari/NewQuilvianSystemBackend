using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models
{
    [Table("MstSanctionType", Schema = "public")]
    public class MstSanctionType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string SanctionTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string SanctionTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string SanctionLevel { get; set; } = "Warning";

        public int? DefaultDurationDays { get; set; }
        public bool IsFinalSanction { get; set; }
        public bool AllowsAppeal { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
