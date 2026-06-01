using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstMeasurementConversion", Schema = "public")]
    public class MstMeasurementConversion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FromMeasurementId { get; set; }

        [Required]
        public Guid ToMeasurementId { get; set; }

        [Required]
        public decimal ConversionFactor { get; set; } = 1;
        // contoh: 1 KG = 1000 G, maka From KG, To G, ConversionFactor 1000

        public bool IsBidirectional { get; set; } = true;

        public bool IsStandardConversion { get; set; } = true;

        [MaxLength(100)]
        public string? ConversionGroupName { get; set; }

        [MaxLength(250)]
        public string? FormulaNote { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstMeasurement? FromMeasurement { get; set; }

        public MstMeasurement? ToMeasurement { get; set; }
    }
}