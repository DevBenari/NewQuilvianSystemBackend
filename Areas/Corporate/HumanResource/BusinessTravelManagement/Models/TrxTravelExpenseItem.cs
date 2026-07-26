using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelExpenseItem", Schema = "public")]
    public class TrxTravelExpenseItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TravelExpenseClaimId { get; set; }

        [Required]
        public Guid TravelExpenseCategoryId { get; set; }

        public Guid? ExpenseCategoryId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? TravelItineraryId { get; set; }
        public Guid? TravelTransportationId { get; set; }
        public Guid? TravelAccommodationId { get; set; }

        public DateOnly ExpenseDate { get; set; }

        [Required, MaxLength(500)]
        public string ExpenseDescription { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? MerchantName { get; set; }

        [MaxLength(100)]
        public string? ReceiptNumber { get; set; }

        public decimal Quantity { get; set; } = 1m;
        public decimal UnitAmount { get; set; } = 0m;
        public decimal ClaimedAmount { get; set; } = 0m;
        public decimal EligibleAmount { get; set; } = 0m;
        public decimal ApprovedAmount { get; set; } = 0m;
        public decimal TaxAmount { get; set; } = 0m;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal ExchangeRate { get; set; } = 1m;
        public decimal BaseCurrencyAmount { get; set; } = 0m;

        public bool HasReceipt { get; set; } = false;
        public bool IsReceiptRequired { get; set; } = true;
        public bool IsPolicyCompliant { get; set; } = false;
        public bool IsTaxable { get; set; } = false;

        [MaxLength(500)]
        public string? ReceiptFilePath { get; set; }

        [MaxLength(255)]
        public string? ReceiptFileName { get; set; }

        [MaxLength(150)]
        public string? ReceiptContentType { get; set; }

        [MaxLength(128)]
        public string? ReceiptChecksum { get; set; }

        [Required, MaxLength(30)]
        public string ItemStatus { get; set; } = "Draft";
        // Draft, Submitted, Verified, Approved, Rejected, NeedRevision.

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTravelExpenseClaim? TravelExpenseClaim { get; set; }
        public MstTravelExpenseCategory? TravelExpenseCategory { get; set; }
        public MstExpenseCategory? ExpenseCategory { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public TrxTravelItinerary? TravelItinerary { get; set; }
        public TrxTravelTransportation? TravelTransportation { get; set; }
        public TrxTravelAccommodation? TravelAccommodation { get; set; }

        public ICollection<TrxTravelDocument> Documents { get; set; } = new List<TrxTravelDocument>();
    }
}
