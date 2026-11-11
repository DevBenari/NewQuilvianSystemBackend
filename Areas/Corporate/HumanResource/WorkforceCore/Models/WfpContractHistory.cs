using QuilvianSystemBackend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpContractHistory", Schema = "public")]
    public class WfpContractHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? PreviousContractHistoryId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? WorkerSourceId { get; set; }
        public Guid? TerminationReasonId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string HistoryType { get; set; } = "Initial";
        // Initial, Renewal, Amendment, Extension, Suspension, Termination, Expiry.

        [Required]
        [MaxLength(50)]
        public string ContractStatus { get; set; } = "Draft";
        // Draft, Active, Suspended, Ended, Expired, Terminated, Cancelled.

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? TerminatedAt { get; set; }
        public int RenewalSequence { get; set; } = 0;
        public bool IsCurrent { get; set; } = false;

        [MaxLength(500)]
        public string? DocumentPath { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? SourceDocumentType { get; set; }

        public Guid? SourceDocumentId { get; set; }

        public DateTime? FixedTermSeriesStartDate { get; set; }

        [MaxLength(250)]
        public string? DocumentFileName { get; set; }

        [MaxLength(150)]
        public string? DocumentContentType { get; set; }

        public DateTime? SignedRecordedAt { get; set; }

        public Guid? SignedRecordedByUserId { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpContractHistory? PreviousContractHistory { get; set; }
        public ICollection<WfpContractHistory> Renewals { get; set; } = new List<WfpContractHistory>();
        public MstContractType? ContractType { get; set; }
        public MstEmploymentType? EmploymentType { get; set; }
        public MstWorkerSource? WorkerSource { get; set; }
        public MstTerminationReason? TerminationReason { get; set; }
    }
}
