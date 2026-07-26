using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
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

        public bool IsClinicalCompetency { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstPositionCompetencyRequirement> PositionRequirements { get; set; }
            = new List<MstPositionCompetencyRequirement>();

        public ICollection<WfpCompetencyAssessment> CompetencyAssessments { get; set; }
            = new List<WfpCompetencyAssessment>();
    }
}
