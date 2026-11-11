using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdInitialContractBatch", Schema = "public")]
    public class HrdInitialContractBatch : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public Guid HospitalSiteId { get; set; }

        public InitialContractBatchStatus BatchStatus { get; set; } = InitialContractBatchStatus.Draft;

        public int TotalRows { get; set; } = 0;

        public int ValidRows { get; set; } = 0;

        public int InvalidRows { get; set; } = 0;

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public Guid? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        public DateTime? AppliedAt { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstHospitalSite? HospitalSite { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? DecidedByUser { get; set; }
        public ICollection<HrdInitialContractBatchRow> Rows { get; set; } = new List<HrdInitialContractBatchRow>();
    }
}
