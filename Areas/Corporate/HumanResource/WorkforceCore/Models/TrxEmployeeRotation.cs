using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("TrxEmployeeRotation", Schema = "public")]
    public class TrxEmployeeRotation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? TransferReasonId { get; set; }
        [Required, MaxLength(50)] public string RotationNumber { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string RotationStatus { get; set; } = "Draft";
        [Required] public DateTime EffectiveDate { get; set; }
        public Guid? FromOrganizationUnitId { get; set; }
        [Required] public Guid ToOrganizationUnitId { get; set; }
        public Guid? FromDepartmentId { get; set; }
        public Guid? ToDepartmentId { get; set; }
        public Guid? FromPositionId { get; set; }
        public Guid? ToPositionId { get; set; }
        public Guid? FromWorkLocationId { get; set; }
        public Guid? ToWorkLocationId { get; set; }
        [MaxLength(500)] public string? ReasonText { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        [MaxLength(500)] public string? Description { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstTransferReason? TransferReason { get; set; }
        public MstOrganizationUnit? FromOrganizationUnit { get; set; }
        public MstOrganizationUnit? ToOrganizationUnit { get; set; }
        public MstDepartment? FromDepartment { get; set; }
        public MstDepartment? ToDepartment { get; set; }
        public MstPosition? FromPosition { get; set; }
        public MstPosition? ToPosition { get; set; }
        public MstWorkLocation? FromWorkLocation { get; set; }
        public MstWorkLocation? ToWorkLocation { get; set; }
    }
}
