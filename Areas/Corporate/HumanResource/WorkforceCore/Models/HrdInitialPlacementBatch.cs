using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("HrdInitialPlacementBatch", Schema = "public")]
    public class HrdInitialPlacementBatch : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string BatchName { get; set; } = "Initial Placement Baseline Batch";

        [Required]
        [MaxLength(30)]
        public string BatchStatus { get; set; } = "Draft"; // Draft, Validated, Submitted, Approved, Rejected, Executed, Failed

        public int TotalRows { get; set; } = 0;
        public int ValidatedRows { get; set; } = 0;
        public int FailedRows { get; set; } = 0;

        public Guid? SubmittedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(1000)]
        public string? ExecutionSummary { get; set; }

        public ICollection<HrdInitialPlacementBatchRow> Rows { get; set; } = new List<HrdInitialPlacementBatchRow>();
    }

    [Table("HrdInitialPlacementBatchRow", Schema = "public")]
    public class HrdInitialPlacementBatchRow : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BatchId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ManagerWorkforceProfileId { get; set; }
        public Guid? SalaryGradeId { get; set; }
        public decimal? BaseSalary { get; set; }

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public bool IsValid { get; set; } = false;

        [MaxLength(1000)]
        public string? ValidationMessage { get; set; }

        public DateTime? ProcessedAt { get; set; }
        public Guid? GeneratedOrderId { get; set; }

        public HrdInitialPlacementBatch? Batch { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstWorkforceProfile? ManagerWorkforceProfile { get; set; }
    }
}
