using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    [Table("MstMembershipTier", Schema = "public")]
    public class MstMembershipTier : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TierCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TierName { get; set; } = string.Empty;

        public MembershipTierType TierType { get; set; } = MembershipTierType.Regular;

        [MaxLength(100)]
        public string? CardTitle { get; set; }

        [MaxLength(50)]
        public string? CardColor { get; set; }

        [MaxLength(500)]
        public string? CardImagePath { get; set; }

        public int PriorityLevel { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public bool IsSelectableInKiosk { get; set; } = false;

        public bool IsSelectableInAdmission { get; set; } = false;

        public bool IsManagedByMarketingOnly { get; set; } = true;

        public decimal RegistrationDiscountPercent { get; set; } = 0;

        public decimal ConsultationDiscountPercent { get; set; } = 0;

        public decimal ProcedureDiscountPercent { get; set; } = 0;

        public decimal LaboratoryDiscountPercent { get; set; } = 0;

        public decimal RadiologyDiscountPercent { get; set; } = 0;

        public decimal PharmacyDiscountPercent { get; set; } = 0;

        public bool PriorityQueue { get; set; } = false;

        public bool FreeAnnualCheckup { get; set; } = false;

        public bool FreeParking { get; set; } = false;

        public int ValidityMonths { get; set; } = 12;

        public decimal MinimumSpendAmount { get; set; } = 0;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? BenefitDescription { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
