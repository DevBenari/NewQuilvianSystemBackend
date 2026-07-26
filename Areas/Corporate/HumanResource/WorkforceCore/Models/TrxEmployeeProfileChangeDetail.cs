using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("TrxEmployeeProfileChangeDetail", Schema = "public")]
    public class TrxEmployeeProfileChangeDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProfileChangeRequestId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FieldGroup { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string FieldName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? OldValue { get; set; }

        [MaxLength(1000)]
        public string? NewValue { get; set; }

        [Required]
        [MaxLength(50)]
        public string ValueType { get; set; } = "String";

        [MaxLength(150)]
        public string? TargetEntityName { get; set; }

        public Guid? TargetEntityId { get; set; }
        public bool RequiresVerification { get; set; } = true;

        [Required]
        [MaxLength(50)]
        public string DetailStatus { get; set; } = "Pending";

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public TrxEmployeeProfileChangeRequest? ProfileChangeRequest { get; set; }
    }
}
