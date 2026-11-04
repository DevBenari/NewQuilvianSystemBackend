using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdProbation", Schema = "public")]
    public class HrdProbation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid ContractHistoryId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public DateOnly ReminderFromDate { get; set; }

        public ProbationStatus ProbationStatus { get; set; } = ProbationStatus.Running;

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public Guid? EmployeeSeparationId { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpContractHistory? ContractHistory { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }
        public ICollection<HrdProbationAssessment> Assessments { get; set; } = new List<HrdProbationAssessment>();
    }
}
