using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstTariff", Schema = "public")]
    public class MstTariff : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TariffCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
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

        [MaxLength(100)]
        public string? ProviderName { get; set; }

        public bool IsSurgeryRelated { get; set; } = false;

        public bool IsRoomCharge { get; set; } = false;

        public bool IsAdministrationFee { get; set; } = false;

        public bool IsRegistrationFee { get; set; } = false;

        public bool IsConsultationFee { get; set; } = false;

        public bool IsPackageTariff { get; set; } = false;

        public bool IsNeedDoctor { get; set; } = false;

        public bool IsNeedApproval { get; set; } = false;

        public decimal NormalPrice { get; set; } = 0;

        public decimal? MemberPrice { get; set; }

        public decimal? InsurancePrice { get; set; }

        public decimal? CompanyPrice { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsTaxable { get; set; } = false;

        public int SortOrder { get; set; } = 0;

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
