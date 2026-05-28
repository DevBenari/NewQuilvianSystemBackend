using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models
{
    [Table("MstBillingItemCategory", Schema = "public")]
    public class MstBillingItemCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string BillingItemCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string BillingItemCategoryName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BillingGroupName { get; set; }

        [MaxLength(50)]
        public string ItemSourceType { get; set; } = "Manual";
        // Tariff, Procedure, Drug, RoomCharge, Registration, Consultation, Laboratory, Radiology, Pharmacy, Manual, Other

        public bool IsRegistrationFee { get; set; } = false;

        public bool IsAdministrationFee { get; set; } = false;

        public bool IsConsultationFee { get; set; } = false;

        public bool IsRoomCharge { get; set; } = false;

        public bool IsProcedure { get; set; } = false;

        public bool IsLaboratory { get; set; } = false;

        public bool IsRadiology { get; set; } = false;

        public bool IsPharmacy { get; set; } = false;

        public bool IsDrug { get; set; } = false;

        public bool IsPackage { get; set; } = false;

        public bool IsDiscount { get; set; } = false;

        public bool IsTax { get; set; } = false;

        public bool IsDeposit { get; set; } = false;

        public bool IsRefund { get; set; } = false;

        public bool IsCoveredByInsuranceDefault { get; set; } = true;

        public bool IsNeedDoctor { get; set; } = false;

        public bool IsNeedApproval { get; set; } = false;

        public bool IsEditableInBilling { get; set; } = true;

        public bool IsSystemCategory { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
