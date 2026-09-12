using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Master aturan tanggungan perusahaan penjamin (MPY-DEC-008, CAP-36).
    /// Cetakan 1:1 dari MstInsuranceCoverageRule dengan kunci CompanyGuarantorId
    /// dan tambahan dimensi EmployeeGrade (golongan karyawan).
    /// </summary>
    [Table("MstCompanyGuarantorCoverageRule", Schema = "public")]
    public class MstCompanyGuarantorCoverageRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CompanyGuarantorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RuleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RuleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
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

        [MaxLength(50)]
        public string? EmployeeGrade { get; set; }

        [Required]
        [MaxLength(30)]
        public string CoverageStatus { get; set; } = "Covered";

        public decimal CoveragePercent { get; set; } = 100m;
        public decimal? MaxCoverageAmount { get; set; }
        public decimal? CoPaymentPercent { get; set; }
        public decimal? CoPaymentAmount { get; set; }
        public bool IsNeedApproval { get; set; } = false;
        public bool IsNeedGuaranteeLetter { get; set; } = false;
        public bool IsAllowExcessPaymentByPatient { get; set; } = true;
        public decimal? MaxQuantityPerVisit { get; set; }
        public decimal? MaxQuantityPerMonth { get; set; }
        public decimal? MaxAmountPerVisit { get; set; }
        public decimal? MaxAmountPerMonth { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int Priority { get; set; } = 0;

        [MaxLength(500)]
        public string? ApprovalInstruction { get; set; }

        [MaxLength(500)]
        public string? BillingInstruction { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Navigations
        public MstCompanyGuarantor? CompanyGuarantor { get; set; }
        public MstTariff? Tariff { get; set; }
        public MstDrug? Drug { get; set; }
        public MstDrugCategory? DrugCategory { get; set; }
        public MstProcedure? Procedure { get; set; }
        public MstTariffCategory? TariffCategory { get; set; }
        public MstPatientClass? PatientClass { get; set; }
    }
}
