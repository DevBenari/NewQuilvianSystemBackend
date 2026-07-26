using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models
{
    [Table("MstTravelType", Schema = "public")]
    public class MstTravelType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TravelTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TravelTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TravelScope { get; set; } = "Domestic";
        // Domestic, International, Both.

        public bool RequiresInvitationLetter { get; set; } = false;

        public bool RequiresTravelOrder { get; set; } = true;

        public bool RequiresPassport { get; set; } = false;

        public bool RequiresVisa { get; set; } = false;

        public bool AllowCashAdvance { get; set; } = true;

        public bool AllowPersonalVehicle { get; set; } = false;

        public bool RequireExpenseSettlement { get; set; } = true;

        public int DefaultSettlementDueDays { get; set; } = 7;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<MstTravelPolicy> TravelPolicies { get; set; }
            = new List<MstTravelPolicy>();
    }
}
