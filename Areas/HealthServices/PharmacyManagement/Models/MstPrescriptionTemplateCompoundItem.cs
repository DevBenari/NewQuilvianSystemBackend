using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("MstPrescriptionTemplateCompoundItem", Schema = "public")]
    public class MstPrescriptionTemplateCompoundItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PrescriptionTemplateCompoundId { get; set; }

        [Required]
        public Guid DrugId { get; set; }

        public decimal AmountPerPackage { get; set; } = 1;

        public decimal TotalQuantity { get; set; } = 1;

        public Guid? QuantityUnitMeasurementId { get; set; }

        [MaxLength(500)]
        public string? IngredientInstruction { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPrescriptionTemplateCompound? PrescriptionTemplateCompound { get; set; }

        public MstDrug? Drug { get; set; }

        public MstMeasurement? QuantityUnitMeasurement { get; set; }
    }
}
