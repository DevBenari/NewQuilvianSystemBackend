using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("MstRecruitmentSource", Schema = "public")]
    public class MstRecruitmentSource : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string SourceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string SourceName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string SourceType { get; set; } = "Direct";
        // Direct, Referral, JobPortal, SocialMedia, Agency, Campus, Internal, Other.

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(500)]
        public string? SourceUrl { get; set; }

        public bool IsEmployeeReferral { get; set; } = false;
        public bool IsExternalSource { get; set; } = false;
        public bool RequiresCostTracking { get; set; } = false;
        public decimal? DefaultCostAmount { get; set; }

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
