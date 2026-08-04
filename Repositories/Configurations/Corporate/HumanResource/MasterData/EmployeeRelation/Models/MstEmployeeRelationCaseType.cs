using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models
{
    [Table("MstEmployeeRelationCaseType", Schema = "public")]
    public class MstEmployeeRelationCaseType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string CaseTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CaseTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string CaseCategory { get; set; } = "Disciplinary";

        public bool RequiresInvestigation { get; set; } = true;
        public bool RequiresHearing { get; set; }
        public bool DefaultConfidential { get; set; } = true;
        public int? TargetResolutionDays { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
