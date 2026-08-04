using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models
{
    [Table("MstDisciplinaryActionType", Schema = "public")]
    public class MstDisciplinaryActionType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ActionTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ActionTypeName { get; set; } = string.Empty;

        [MaxLength(40)]
        public string? DefaultActionLevel { get; set; }

        public int? DefaultEffectiveDays { get; set; }
        public bool RequiresApproval { get; set; } = true;
        public bool AllowsAppeal { get; set; } = true;
        public bool IsConfidential { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
