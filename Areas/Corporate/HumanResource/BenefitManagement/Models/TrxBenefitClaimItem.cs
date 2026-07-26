using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    [Table("TrxBenefitClaimItem", Schema = "public")]
    public class TrxBenefitClaimItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid BenefitClaimId { get; set; }

        public Guid? BenefitTypeId { get; set; }

        public int ItemNumber { get; set; } = 1;

        public DateTime? ServiceDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ItemDescription { get; set; } = string.Empty;

        public decimal ClaimedAmount { get; set; } = 0m;

        public decimal EligibleAmount { get; set; } = 0m;

        public decimal NonEligibleAmount { get; set; } = 0m;

        public decimal ApprovedAmount { get; set; } = 0m;

        public decimal PaidAmount { get; set; } = 0m;

        public decimal? PolicyLimitAmount { get; set; }

        [Required]
        [MaxLength(30)]
        public string EligibilityStatus { get; set; } = "Pending";

        [MaxLength(1000)]
        public string? EligibilityReason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBenefitClaim? BenefitClaim { get; set; }
        public MstBenefitType? BenefitType { get; set; }

        public ICollection<TrxBenefitClaimDocument> Documents { get; set; } = new List<TrxBenefitClaimDocument>();
    }
}
