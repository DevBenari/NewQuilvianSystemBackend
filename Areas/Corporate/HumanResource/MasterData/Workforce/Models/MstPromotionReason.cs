using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstPromotionReason", Schema = "public")]
    public class MstPromotionReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PromotionReasonCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PromotionReasonName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool RequiresPerformanceReview { get; set; } = true;

        public bool RequiresSalaryReview { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
