using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models
{
    [Table("MstPerformanceRatingScale", Schema = "public")]
    public class MstPerformanceRatingScale : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ScaleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ScaleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ScaleType { get; set; } = "Numeric";
        // Numeric, Percentage, Descriptive, FivePoint, Custom

        public decimal MinimumScore { get; set; } = 1m;

        public decimal MaximumScore { get; set; } = 5m;

        public decimal? PassingScore { get; set; }

        public int DecimalPlaces { get; set; } = 2;

        public bool IsHigherScoreBetter { get; set; } = true;

        public string? RatingDefinitionJson { get; set; }
        // Contoh level: [{"score":1,"code":"POOR","label":"Kurang"}, ...]

        public bool IsDefault { get; set; } = false;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstPerformanceTemplate> PerformanceTemplates { get; set; }
            = new List<MstPerformanceTemplate>();

        public ICollection<MstPerformanceTemplateDetail> TemplateDetails { get; set; }
            = new List<MstPerformanceTemplateDetail>();
    }
}
