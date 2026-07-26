using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("TrxEmployeeTransfer", Schema = "public")]
    public class TrxEmployeeTransfer : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? TransferReasonId { get; set; }
        [Required, MaxLength(50)] public string TransferNumber { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string TransferStatus { get; set; } = "Draft";
        [Required] public DateTime EffectiveDate { get; set; }
        public Guid? FromOrganizationAssignmentId { get; set; }
        public Guid? ToOrganizationAssignmentId { get; set; }
        public Guid? FromLegalEntityId { get; set; }
        public Guid? ToLegalEntityId { get; set; }
        public Guid? FromHospitalSiteId { get; set; }
        public Guid? ToHospitalSiteId { get; set; }
        public Guid? FromOrganizationUnitId { get; set; }
        public Guid? ToOrganizationUnitId { get; set; }
        public Guid? FromDepartmentId { get; set; }
        public Guid? ToDepartmentId { get; set; }
        public Guid? FromPositionId { get; set; }
        public Guid? ToPositionId { get; set; }
        public Guid? FromCostCenterId { get; set; }
        public Guid? ToCostCenterId { get; set; }
        public Guid? FromWorkLocationId { get; set; }
        public Guid? ToWorkLocationId { get; set; }
        [MaxLength(500)] public string? ReasonText { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        [MaxLength(500)] public string? Description { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstTransferReason? TransferReason { get; set; }
        public WfpOrganizationAssignment? FromOrganizationAssignment { get; set; }
        public WfpOrganizationAssignment? ToOrganizationAssignment { get; set; }
        public MstLegalEntity? FromLegalEntity { get; set; }
        public MstLegalEntity? ToLegalEntity { get; set; }
        public MstHospitalSite? FromHospitalSite { get; set; }
        public MstHospitalSite? ToHospitalSite { get; set; }
        public MstOrganizationUnit? FromOrganizationUnit { get; set; }
        public MstOrganizationUnit? ToOrganizationUnit { get; set; }
        public MstDepartment? FromDepartment { get; set; }
        public MstDepartment? ToDepartment { get; set; }
        public MstPosition? FromPosition { get; set; }
        public MstPosition? ToPosition { get; set; }
        public MstCostCenter? FromCostCenter { get; set; }
        public MstCostCenter? ToCostCenter { get; set; }
        public MstWorkLocation? FromWorkLocation { get; set; }
        public MstWorkLocation? ToWorkLocation { get; set; }
    }
}
