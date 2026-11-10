using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdInitialContractBatchRow", Schema = "public")]
    public class HrdInitialContractBatchRow : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BatchId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid ContractTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public DateOnly? FixedTermSeriesStartDate { get; set; }

        public bool IsValid { get; set; } = false;

        [MaxLength(500)]
        public string? ValidationMessage { get; set; }

        public Guid? CreatedContractHistoryId { get; set; }

        public HrdInitialContractBatch? Batch { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstContractType? ContractType { get; set; }
        public WfpContractHistory? CreatedContractHistory { get; set; }
    }
}
