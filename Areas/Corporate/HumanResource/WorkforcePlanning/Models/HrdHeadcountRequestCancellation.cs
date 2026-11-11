using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdHeadcountRequestCancellation", Schema = "public")]
    public class HrdHeadcountRequestCancellation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string CancellationNumber { get; set; } = string.Empty;

        [Required]
        public Guid HeadcountRequestId { get; set; }

        public HeadcountCancellationStatus CancellationStatus { get; set; } = HeadcountCancellationStatus.Submitted;

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public Guid RequestedByUserId { get; set; }

        public Guid? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        public Guid? CreatedFormationId { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public HrdHeadcountRequest? HeadcountRequest { get; set; }
        public HrdPositionFormation? CreatedFormation { get; set; }
    }
}
