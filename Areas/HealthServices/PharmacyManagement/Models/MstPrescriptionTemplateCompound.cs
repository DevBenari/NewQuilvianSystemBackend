using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("MstPrescriptionTemplateCompound", Schema = "public")]
    public class MstPrescriptionTemplateCompound : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PrescriptionTemplateId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompoundName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CompoundForm { get; set; }

        public decimal TotalPackage { get; set; } = 1;

        public Guid? PackageUnitMeasurementId { get; set; }

        public decimal DosePerUse { get; set; } = 1;

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

        [MaxLength(1000)]
        public string? CompoundingInstruction { get; set; }

        [MaxLength(500)]
        public string? AdministrationInstruction { get; set; }

        [MaxLength(500)]
        public string? DoctorNote { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPrescriptionTemplate? PrescriptionTemplate { get; set; }

        public MstMeasurement? PackageUnitMeasurement { get; set; }

        public MstMeasurement? DoseUnitMeasurement { get; set; }

        public ICollection<MstPrescriptionTemplateCompoundItem> Items { get; set; }
            = new List<MstPrescriptionTemplateCompoundItem>();
    }
}
