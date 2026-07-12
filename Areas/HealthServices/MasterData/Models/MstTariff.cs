using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstTariff", Schema = "public")]
    public class MstTariff : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string TariffCode { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string TariffName { get; set; } = string.Empty;

        [Required]
        public Guid TariffCategoryId { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? PatientClassId { get; set; }
        public Guid? ProcedureId { get; set; }
        public Guid? DrugId { get; set; }

        [MaxLength(50)]
        public string? ExternalServiceCode { get; set; }

        [MaxLength(50)]
        public string? ExternalClassCode { get; set; }

        public bool IsSurgeryRelated { get; set; }
        public bool IsRoomCharge { get; set; }
        public bool IsAdministrationFee { get; set; }
        public bool IsRegistrationFee { get; set; }
        public bool IsConsultationFee { get; set; }
        public bool IsPackageTariff { get; set; }
        public bool IsNeedDoctor { get; set; }
        public bool IsNeedApproval { get; set; }

        public decimal NormalPrice { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public bool IsTaxable { get; set; }
        public int SortOrder { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstTariffCategory? TariffCategory { get; set; }
        public MstServiceUnit? ServiceUnit { get; set; }
        public MstClinic? Clinic { get; set; }
        public MstPatientClass? PatientClass { get; set; }
        public MstProcedure? Procedure { get; set; }
        public MstDrug? Drug { get; set; }
    }
}
