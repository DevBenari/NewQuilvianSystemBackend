using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
{
    [Table("MstCompetency", Schema = "public")]
    public class MstCompetency : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string CompetencyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; } = CompetencyCategory.Other;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstPositionCompetencyRequirement> PositionRequirements { get; set; } = new List<MstPositionCompetencyRequirement>();

        public ICollection<WfpCompetencyAssessment> CompetencyAssessments { get; set; } = new List<WfpCompetencyAssessment>();
    }
}
