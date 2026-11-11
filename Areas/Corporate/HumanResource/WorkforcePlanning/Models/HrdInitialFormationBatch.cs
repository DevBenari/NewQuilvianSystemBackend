using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdInitialFormationBatch", Schema = "public")]
    public class HrdInitialFormationBatch : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public Guid HospitalSiteId { get; set; }

        [Required]
        [MaxLength(200)]
        public string SourceDocumentReference { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? SourceDocumentPath { get; set; }

        [MaxLength(255)]
        public string? SourceDocumentFileName { get; set; }

        public InitialFormationBatchStatus BatchStatus { get; set; } = InitialFormationBatchStatus.Draft;

        public int TotalRows { get; set; } = 0;

        public int ValidRows { get; set; } = 0;

        public int InvalidRows { get; set; } = 0;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? AppliedAt { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstHospitalSite? HospitalSite { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }

        public ICollection<HrdInitialFormationBatchRow> Rows { get; set; } = new List<HrdInitialFormationBatchRow>();
    }

    [Table("HrdInitialFormationBatchRow", Schema = "public")]
    public class HrdInitialFormationBatchRow : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BatchId { get; set; }

        [Required]
        public Guid OrganizationUnitId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public decimal FormationQuantity { get; set; }

        public DateOnly EffectiveStartDate { get; set; }

        public bool IsValid { get; set; } = false;

        [MaxLength(500)]
        public string? ValidationMessage { get; set; }

        public Guid? CreatedFormationId { get; set; }

        public HrdInitialFormationBatch? Batch { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstPosition? Position { get; set; }
        public HrdPositionFormation? CreatedFormation { get; set; }
    }
}
