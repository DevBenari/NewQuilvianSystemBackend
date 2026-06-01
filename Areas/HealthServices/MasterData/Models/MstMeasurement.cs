using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstMeasurement", Schema = "public")]
    public class MstMeasurement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string MeasurementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string MeasurementName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MeasurementSymbol { get; set; }

        [Required]
        [MaxLength(50)]
        public string MeasurementType { get; set; } = "General";
        // General, Weight, Volume, Length, Count, Time, Dose, Pharmacy

        [MaxLength(100)]
        public string? MeasurementGroupName { get; set; }
        // contoh: Weight, Volume, Tablet, Injection, Syrup

        public bool IsBaseUnit { get; set; } = false;

        public bool IsDecimalAllowed { get; set; } = true;

        public int DecimalPrecision { get; set; } = 2;

        public bool IsForDrug { get; set; } = false;

        public bool IsForLaboratory { get; set; } = false;

        public bool IsForVitalSign { get; set; } = false;

        public bool IsForGeneralUse { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
