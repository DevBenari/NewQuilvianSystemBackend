using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpDisciplinaryAction", Schema = "public")]
    public class WfpDisciplinaryAction : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public DisciplinaryActionType ActionType { get; set; } = DisciplinaryActionType.Unknown;

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; }

        public DisciplinarySeverityLevel SeverityLevel { get; set; } = DisciplinarySeverityLevel.Low;

        [Required]
        [MaxLength(250)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public Guid IssuedByUserId { get; set; }

        public DateTime? EffectiveUntil { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        public DisciplinaryActionStatus ActionStatus { get; set; } = DisciplinaryActionStatus.Draft;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public ApplicationUser? IssuedByUser { get; set; }
    }
}
