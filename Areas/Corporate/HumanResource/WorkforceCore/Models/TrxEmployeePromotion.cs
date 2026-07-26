using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("TrxEmployeePromotion", Schema = "public")]
    public class TrxEmployeePromotion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? PromotionReasonId { get; set; }
        [Required, MaxLength(50)] public string PromotionNumber { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string PromotionStatus { get; set; } = "Draft";
        [Required] public DateTime EffectiveDate { get; set; }
        public Guid? FromPositionId { get; set; }
        [Required] public Guid ToPositionId { get; set; }
        public Guid? FromJobLevelId { get; set; }
        public Guid? ToJobLevelId { get; set; }
        public Guid? FromEmployeeGradeId { get; set; }
        public Guid? ToEmployeeGradeId { get; set; }
        public Guid? FromSalaryGradeId { get; set; }
        public Guid? ToSalaryGradeId { get; set; }
        public Guid? ToSalaryStructureId { get; set; }
        public decimal? NewBaseSalary { get; set; }
        [MaxLength(500)] public string? ReasonText { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        [MaxLength(500)] public string? Description { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstPromotionReason? PromotionReason { get; set; }
        public MstPosition? FromPosition { get; set; }
        public MstPosition? ToPosition { get; set; }
        public MstJobLevel? FromJobLevel { get; set; }
        public MstJobLevel? ToJobLevel { get; set; }
        public MstEmployeeGrade? FromEmployeeGrade { get; set; }
        public MstEmployeeGrade? ToEmployeeGrade { get; set; }
        public MstSalaryGrade? FromSalaryGrade { get; set; }
        public MstSalaryGrade? ToSalaryGrade { get; set; }
        public MstSalaryStructure? ToSalaryStructure { get; set; }
    }
}
