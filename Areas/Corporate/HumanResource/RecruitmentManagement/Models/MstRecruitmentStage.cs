using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("MstRecruitmentStage", Schema = "public")]
    public class MstRecruitmentStage : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string StageCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string StageName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string StageType { get; set; } = "Screening";
        // Application, Screening, Assessment, Interview, ReferenceCheck, MedicalCheck, Offer, Hiring, Closed.

        public int StageOrder { get; set; } = 0;
        public int TargetCompletionDays { get; set; } = 0;
        public bool IsRequired { get; set; } = true;
        public bool AllowSkip { get; set; } = false;
        public bool IsFinalStage { get; set; } = false;
        public bool IsRejectStage { get; set; } = false;
        public bool IsHireStage { get; set; } = false;
        public bool RequiresApproval { get; set; } = false;
        public bool RequiresDocument { get; set; } = false;
        public bool RequiresScore { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
