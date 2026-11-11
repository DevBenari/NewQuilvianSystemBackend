using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdCandidateScreening", Schema = "public")]
    public class HrdCandidateScreening : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ApplicationId { get; set; }

        public DateTime ScreeningDate { get; set; } = DateTime.UtcNow;

        public ScreeningStatus Status { get; set; } = ScreeningStatus.Pending;

        public bool IsStrRequired { get; set; }

        public ClinicalScreeningResult ClinicalScreeningResult { get; set; } = ClinicalScreeningResult.NotApplicable;

        [MaxLength(100)]
        public string? StrNumber { get; set; }

        public DateOnly? StrExpiryDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public Guid EvaluatedByUserId { get; set; }

        public HrdCandidateApplication? Application { get; set; }
    }
}
