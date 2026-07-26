using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models
{
    [Table("MstTravelAllowanceRate", Schema = "public")]
    public class MstTravelAllowanceRate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TravelPolicyId { get; set; }

        [Required]
        public Guid TravelExpenseCategoryId { get; set; }

        public Guid? TravelClassId { get; set; }

        public Guid? DestinationZoneId { get; set; }

        public Guid? EmployeeGradeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AllowanceRateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string AllowanceRateName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RateType { get; set; } = "Fixed";
        // Fixed, PerDay, PerNight, PerTrip, PerKilometer, ActualUpToLimit.

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal RateAmount { get; set; } = 0m;

        public decimal? MinimumAmount { get; set; }

        public decimal? MaximumAmount { get; set; }

        public decimal? Percentage { get; set; }

        public bool RequiresReceipt { get; set; } = false;

        public bool IsTaxable { get; set; } = false;

        public int Priority { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstTravelPolicy? TravelPolicy { get; set; }

        public MstTravelExpenseCategory? TravelExpenseCategory { get; set; }

        public MstTravelClass? TravelClass { get; set; }

        public MstTravelDestinationZone? DestinationZone { get; set; }

        public MstEmployeeGrade? EmployeeGrade { get; set; }
    }
}
