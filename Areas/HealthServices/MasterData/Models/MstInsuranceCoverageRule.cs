using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstInsuranceCoverageRule", Schema = "public")]
    public class MstInsuranceCoverageRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InsuranceProviderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RuleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RuleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ItemType { get; set; } = string.Empty;
        // Tariff, Drug, DrugCategory, Procedure, ServiceCategory

        public Guid? TariffId { get; set; }

        public Guid? DrugId { get; set; }

        public Guid? DrugCategoryId { get; set; }

        public Guid? ProcedureId { get; set; }

        public Guid? TariffCategoryId { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanName { get; set; }

        [MaxLength(100)]
        public string? PatientClassName { get; set; }

        [MaxLength(50)]
        public string CoverageStatus { get; set; } = "Covered";
        // Covered, NotCovered, PartialCovered, NeedApproval

        public decimal CoveragePercent { get; set; } = 100;

        public decimal? MaxCoverageAmount { get; set; }

        public decimal? CoPaymentPercent { get; set; }

        public decimal? CoPaymentAmount { get; set; }

        public bool IsCovered { get; set; } = true;

        public bool IsExcluded { get; set; } = false;

        public bool IsNeedApproval { get; set; } = false;

        public bool IsNeedGuaranteeLetter { get; set; } = false;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        public int? MaxQuantityPerVisit { get; set; }

        public int? MaxQuantityPerMonth { get; set; }

        public decimal? MaxAmountPerVisit { get; set; }

        public decimal? MaxAmountPerMonth { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? ApprovalInstruction { get; set; }

        [MaxLength(250)]
        public string? BillingInstruction { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstInsuranceProvider? InsuranceProvider { get; set; }

        public MstTariff? Tariff { get; set; }

        public MstDrug? Drug { get; set; }

        public MstDrugCategory? DrugCategory { get; set; }

        public MstProcedure? Procedure { get; set; }

        public MstTariffCategory? TariffCategory { get; set; }
    }
}
