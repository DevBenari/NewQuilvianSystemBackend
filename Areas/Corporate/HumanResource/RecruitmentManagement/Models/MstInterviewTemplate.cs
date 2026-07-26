using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("MstInterviewTemplate", Schema = "public")]
    public class MstInterviewTemplate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string InterviewType { get; set; } = "UserInterview";
        // HrInterview, UserInterview, TechnicalInterview, ClinicalInterview, PanelInterview, FinalInterview.

        public Guid? PositionId { get; set; }
        public Guid? JobFamilyId { get; set; }
        public Guid? JobLevelId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? RatingScaleId { get; set; }

        public int MinimumPanelSize { get; set; } = 1;
        public int MaximumPanelSize { get; set; } = 5;
        public int EstimatedDurationMinutes { get; set; } = 60;
        public decimal PassingScore { get; set; } = 0m;
        public bool RequiresPanelConsensus { get; set; } = false;
        public bool RequiresHrParticipation { get; set; } = false;
        public bool RequiresHiringManager { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public string? QuestionDefinitionJson { get; set; }
        public string? EvaluationCriteriaJson { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public MstPosition? Position { get; set; }
        public MstJobFamily? JobFamily { get; set; }
        public MstJobLevel? JobLevel { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstPerformanceRatingScale? RatingScale { get; set; }
    }
}
