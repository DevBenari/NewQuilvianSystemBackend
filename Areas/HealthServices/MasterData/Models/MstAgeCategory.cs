using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstAgeCategory", Schema = "public")]
    public class MstAgeCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string AgeCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string AgeCategoryName { get; set; } = string.Empty;

        [MaxLength(75)]
        public string? AgeCategoryShortName { get; set; }

        public int MinAgeDays { get; set; } = 0;

        public int? MaxAgeDays { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsSelectableInKiosk { get; set; } = true;

        public bool IsSelectableInRegistration { get; set; } = true;

        public bool IsUsedForClinicalRule { get; set; } = true;

        [MaxLength(250)]
        public string? StandardReference { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
