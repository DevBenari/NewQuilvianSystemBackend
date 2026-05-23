using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
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

        public bool IsActive { get; set; } = true;

        public MstPosition? Position { get; set; }

        public MstCompetency? Competency { get; set; }
    }
}
