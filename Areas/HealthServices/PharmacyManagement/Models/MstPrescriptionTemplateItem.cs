using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("MstPrescriptionTemplateItem", Schema = "public")]
    public class MstPrescriptionTemplateItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PrescriptionTemplateId { get; set; }

        [Required]
        public Guid DrugId { get; set; }

        public decimal Dose { get; set; } = 1;

        public Guid? DoseUnitMeasurementId { get; set; }

        [MaxLength(50)]
        public string? FrequencyCode { get; set; }

        [MaxLength(150)]
        public string? FrequencyText { get; set; }

        public decimal? FrequencyPerDay { get; set; }

        public decimal? DurationValue { get; set; }

        [MaxLength(30)]
        public string? DurationUnit { get; set; }

        public bool IsAsNeeded { get; set; }

        [MaxLength(250)]
        public string? AdministrationTime { get; set; }

        [MaxLength(500)]
        public string? Signa { get; set; }

        [MaxLength(500)]
        public string? AdministrationInstruction { get; set; }

        [MaxLength(500)]
        public string? DoctorNote { get; set; }

        public decimal Quantity { get; set; } = 1;

        public Guid? DispenseUnitMeasurementId { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPrescriptionTemplate? PrescriptionTemplate { get; set; }

        public MstDrug? Drug { get; set; }

        public MstMeasurement? DoseUnitMeasurement { get; set; }

        public MstMeasurement? DispenseUnitMeasurement { get; set; }
    }
}
