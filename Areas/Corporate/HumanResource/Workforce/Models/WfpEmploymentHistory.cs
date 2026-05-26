using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpEmploymentHistory", Schema = "public")]
    public class WfpEmploymentHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public EmploymentHistoryType HistoryType { get; set; } = EmploymentHistoryType.Unknown;

        public Guid? OldDepartmentId { get; set; }

        public Guid? NewDepartmentId { get; set; }

        public Guid? OldPositionId { get; set; }

        public Guid? NewPositionId { get; set; }

        [MaxLength(100)]
        public string? OldStatus { get; set; }

        [MaxLength(100)]
        public string? NewStatus { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public MstDepartment? OldDepartment { get; set; }

        public MstDepartment? NewDepartment { get; set; }

        public MstPosition? OldPosition { get; set; }

        public MstPosition? NewPosition { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }
    }

    [Table("WfpContractHistory", Schema = "public")]
    public class WfpContractHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        public ContractHistoryType ContractType { get; set; } = ContractHistoryType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ContractHistoryStatus ContractStatus { get; set; } = ContractHistoryStatus.Draft;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
