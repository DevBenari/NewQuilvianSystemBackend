using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelTransportation", Schema = "public")]
    public class TrxTravelTransportation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public Guid? BusinessTravelParticipantId { get; set; }
        public Guid? TravelItineraryId { get; set; }
        public Guid? TravelClassId { get; set; }

        [Required, MaxLength(50)]
        public string TransportationType { get; set; } = "Air";
        // Air, Rail, Sea, Road, Taxi, Rental, PersonalVehicle, Other.

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(100)]
        public string? BookingReference { get; set; }

        [MaxLength(100)]
        public string? TicketNumber { get; set; }

        [Required, MaxLength(250)]
        public string Origin { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string Destination { get; set; } = string.Empty;

        public DateTime? DepartureAt { get; set; }
        public DateTime? ArrivalAt { get; set; }

        public decimal EstimatedAmount { get; set; } = 0m;
        public decimal ActualAmount { get; set; } = 0m;
        public decimal ApprovedAmount { get; set; } = 0m;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(500)]
        public string? TicketFilePath { get; set; }

        [MaxLength(255)]
        public string? TicketFileName { get; set; }

        [Required, MaxLength(30)]
        public string TransportationStatus { get; set; } = "Planned";
        // Planned, Booked, Ticketed, Completed, Cancelled, Refunded.

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public TrxBusinessTravelParticipant? BusinessTravelParticipant { get; set; }
        public TrxTravelItinerary? TravelItinerary { get; set; }
        public MstTravelClass? TravelClass { get; set; }

        public ICollection<TrxTravelExpenseItem> ExpenseItems { get; set; } = new List<TrxTravelExpenseItem>();
    }
}
