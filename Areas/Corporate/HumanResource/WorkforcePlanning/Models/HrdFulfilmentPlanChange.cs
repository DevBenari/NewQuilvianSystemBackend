using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdFulfilmentPlanChange", Schema = "public")]
    public class HrdFulfilmentPlanChange : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid HeadcountRequestId { get; set; }

        [Required]
        public Guid SeatId { get; set; }

        public HeadcountFulfilmentSource FromSource { get; set; }

        public HeadcountFulfilmentSource ToSource { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public Guid ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; }

        public HrdHeadcountRequest? HeadcountRequest { get; set; }
        public HrdHeadcountRequestSeat? Seat { get; set; }
    }
}
