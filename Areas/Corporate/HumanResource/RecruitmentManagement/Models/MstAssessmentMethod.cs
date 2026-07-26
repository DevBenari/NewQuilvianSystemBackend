using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("MstAssessmentMethod", Schema = "public")]
    public class MstAssessmentMethod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string MethodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string MethodName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string MethodType { get; set; } = "WrittenTest";
        // WrittenTest, OnlineTest, PracticalTest, Psychometric, Personality, ClinicalSkill, CaseStudy, Presentation, Other.

        public Guid? CompetencyId { get; set; }

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(500)]
        public string? ProviderUrl { get; set; }

        public decimal MaximumScore { get; set; } = 100m;
        public decimal PassingScore { get; set; } = 0m;
        public int EstimatedDurationMinutes { get; set; } = 60;
        public int? ResultValidityMonths { get; set; }
        public bool RequiresProctor { get; set; } = false;
        public bool RequiresAttachment { get; set; } = false;
        public bool IsOnlineSupported { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public string? ConfigurationJson { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public MstCompetency? Competency { get; set; }
    }
}
