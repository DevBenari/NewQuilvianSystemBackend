using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
{
    [Table("MstPositionCompetencyRequirement", Schema = "public")]
    public class MstPositionCompetencyRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PositionId { get; set; }

        [Required]
        public Guid CompetencyId { get; set; }

        public bool IsRequired { get; set; } = true;

        public CompetencyLevel MinimumLevel { get; set; } = CompetencyLevel.Basic;

        public bool IsCertificationRequired { get; set; } = false;

        public bool IsTrainingRequired { get; set; } = false;

        [MaxLength(100)]
        public string? AssessmentMethod { get; set; }

        public int? ValidityMonths { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPosition? Position { get; set; }

        public MstCompetency? Competency { get; set; }
    }
}
