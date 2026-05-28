using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDrugCategory", Schema = "public")]
    public class MstDrugCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string DrugCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DrugCategoryName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DrugGroupName { get; set; }

        [MaxLength(50)]
        public string DrugCategoryType { get; set; } = "General";
        // General, Antibiotic, Analgesic, Antipyretic, Antihypertensive, Antidiabetic, Vitamin, Vaccine, Consumable, Other

        public bool IsAntibiotic { get; set; } = false;
        public bool IsNarcotic { get; set; } = false;
        public bool IsPsychotropic { get; set; } = false;
        public bool IsHighAlert { get; set; } = false;
        public bool IsChronicDiseaseDrug { get; set; } = false;
        public bool IsVaccine { get; set; } = false;
        public bool IsConsumable { get; set; } = false;
        public bool IsCoveredByInsuranceDefault { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
