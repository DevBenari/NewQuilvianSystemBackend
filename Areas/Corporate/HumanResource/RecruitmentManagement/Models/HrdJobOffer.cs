using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdJobOffer", Schema = "public")]
    public class HrdJobOffer : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string OfferNumber { get; set; } = string.Empty;

        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public Guid SalaryGradeId { get; set; }

        public decimal OfferedBasicSalary { get; set; }

        public decimal MaximumGradeSalary { get; set; }

        public JobOfferApprovalTier ApprovalTier { get; set; } = JobOfferApprovalTier.Tier13Standard;

        [MaxLength(150)]
        public string OfferedPositionTitle { get; set; } = string.Empty;

        public DateOnly ProposedJoinDate { get; set; }

        public DateTime OfferDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        public JobOfferStatus Status { get; set; } = JobOfferStatus.Draft;

        public DateTime? CandidateResponseDate { get; set; }

        [MaxLength(500)]
        public string? CandidateResponseNotes { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdCandidateApplication? Application { get; set; }
        public MstSalaryGrade? SalaryGrade { get; set; }
        public ICollection<HrdJobOfferApproval> Approvals { get; set; } = new List<HrdJobOfferApproval>();
        public HrdPreEmploymentMedicalCheck? MedicalCheck { get; set; }
        public HrdCandidateHiringHandoff? HiringHandoff { get; set; }
    }
}
