using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdCandidateEvaluation", Schema = "public")]
    public class HrdCandidateEvaluation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ApplicationId { get; set; }

        public EvaluationType EvaluationType { get; set; }

        [Required]
        public Guid EvaluatorUserId { get; set; }

        [MaxLength(150)]
        public string EvaluatorName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string EvaluatorRole { get; set; } = string.Empty;

        public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;

        public decimal Score { get; set; }

        public EvaluationRecommendation Recommendation { get; set; }

        public bool IsClinicalVetoApplied { get; set; }

        [MaxLength(500)]
        public string? VetoReason { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public HrdCandidateApplication? Application { get; set; }
    }
}
