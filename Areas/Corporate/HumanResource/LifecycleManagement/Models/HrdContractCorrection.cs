using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdContractCorrection", Schema = "public")]
    public class HrdContractCorrection : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string CorrectionNumber { get; set; } = string.Empty;

        [Required]
        public Guid ContractHistoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BeforeContractNumber { get; set; } = string.Empty;

        [Required]
        public Guid BeforeContractTypeId { get; set; }

        [Required]
        public DateOnly BeforeStartDate { get; set; }

        public DateOnly? BeforeEndDate { get; set; }

        [MaxLength(500)]
        public string? BeforeDocumentPath { get; set; }

        [Required]
        [MaxLength(100)]
        public string AfterContractNumber { get; set; } = string.Empty;

        [Required]
        public Guid AfterContractTypeId { get; set; }

        [Required]
        public DateOnly AfterStartDate { get; set; }

        public DateOnly? AfterEndDate { get; set; }

        [Required]
        [MaxLength(2000)]
        public string ReasonText { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string EvidenceStoragePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string EvidenceFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string EvidenceContentType { get; set; } = string.Empty;

        public ContractCorrectionStatus CorrectionStatus { get; set; } = ContractCorrectionStatus.Submitted;

        [Required]
        public Guid SubmittedByUserId { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public Guid? WorkflowInstanceId { get; set; }

        public Guid? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        [MaxLength(1000)]
        public string? DecisionNote { get; set; }

        public DateTime? AppliedAt { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public WfpContractHistory? ContractHistory { get; set; }
        public MstContractType? BeforeContractType { get; set; }
        public MstContractType? AfterContractType { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? DecidedByUser { get; set; }
    }
}
