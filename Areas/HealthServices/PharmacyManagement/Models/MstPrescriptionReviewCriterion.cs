using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("MstPrescriptionReviewCriterion", Schema = "public")]
    public class MstPrescriptionReviewCriterion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(80)]
        public string CriterionCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string CriterionName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public PrescriptionReviewCategory Category { get; set; }
        public PrescriptionIssueSeverity DefaultSeverity { get; set; } = PrescriptionIssueSeverity.Warning;
        public bool IsRequired { get; set; } = true;
        public bool IsApplicableToRegular { get; set; } = true;
        public bool IsApplicableToCompound { get; set; } = true;
        public bool IsSystemCheckSupported { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }

        public ICollection<TrxPrescriptionReviewItem> ReviewItems { get; set; }
            = new List<TrxPrescriptionReviewItem>();
    }
}
