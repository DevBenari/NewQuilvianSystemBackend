using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstInsuranceCoverageRule", Schema = "public")]
    public class MstInsuranceCoverageRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InsuranceProviderId { get; set; }

        [Required, MaxLength(50)]
        public string RuleCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string RuleName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ItemType { get; set; } = "Tariff";

        public Guid? TariffId { get; set; }
        public Guid? DrugId { get; set; }
        public Guid? DrugCategoryId { get; set; }
        public Guid? ProcedureId { get; set; }
        public Guid? TariffCategoryId { get; set; }
        public Guid? PatientClassId { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanName { get; set; }

        [Required, MaxLength(50)]
        public string CoverageStatus { get; set; } = "Covered";

        public decimal CoveragePercent { get; set; } = 100;
        public decimal? MaxCoverageAmount { get; set; }
        public decimal? CoPaymentPercent { get; set; }
        public decimal? CoPaymentAmount { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; } = true;
        public int? MaxQuantityPerVisit { get; set; }
        public int? MaxQuantityPerMonth { get; set; }
        public decimal? MaxAmountPerVisit { get; set; }
        public decimal? MaxAmountPerMonth { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int Priority { get; set; }

        [MaxLength(250)]
        public string? ApprovalInstruction { get; set; }

        [MaxLength(250)]
        public string? BillingInstruction { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public MstInsuranceProvider? InsuranceProvider { get; set; }
        public MstTariff? Tariff { get; set; }
        public MstDrug? Drug { get; set; }
        public MstDrugCategory? DrugCategory { get; set; }
        public MstProcedure? Procedure { get; set; }
        public MstTariffCategory? TariffCategory { get; set; }
        public MstPatientClass? PatientClass { get; set; }
    }
}
