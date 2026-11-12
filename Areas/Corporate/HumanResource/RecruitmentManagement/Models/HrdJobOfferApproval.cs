using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdJobOfferApproval", Schema = "public")]
    public class HrdJobOfferApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OfferId { get; set; }

        public int StepNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string ApproverRole { get; set; } = string.Empty;

        public Guid? ApproverUserId { get; set; }

        [MaxLength(150)]
        public string? ApproverName { get; set; }

        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public DateTime? DecisionDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public HrdJobOffer? Offer { get; set; }
    }
}
