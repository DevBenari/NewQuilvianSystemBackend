using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDoctorServiceRule", Schema = "public")]
    public class MstDoctorServiceRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RuleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RuleName { get; set; } = string.Empty;

        public DoctorServiceRuleType RuleType { get; set; } = DoctorServiceRuleType.GeneralService;

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? TariffCategoryId { get; set; }

        public Guid? TariffId { get; set; }

        public Guid? ProcedureId { get; set; }

        public Guid? PatientClassId { get; set; }

        public bool IsAllowWalkIn { get; set; } = true;

        public bool IsAllowAppointment { get; set; } = true;

        public bool IsAllowKioskRegistration { get; set; } = true;

        public bool IsAllowTelemedicine { get; set; } = false;

        public bool IsNeedReferral { get; set; } = false;

        public bool IsNeedApproval { get; set; } = false;

        public bool IsPrimaryForClinic { get; set; } = false;

        public bool IsDefaultForClinic { get; set; } = false;

        public int DailyQuotaLimit { get; set; } = 0;

        public int PriorityLevel { get; set; } = 0;

        public DoctorServiceRuleStatus RuleStatus { get; set; } = DoctorServiceRuleStatus.Active;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstTariffCategory? TariffCategory { get; set; }

        public MstTariff? Tariff { get; set; }

        public MstProcedure? Procedure { get; set; }

        public MstPatientClass? PatientClass { get; set; }
    }
}
