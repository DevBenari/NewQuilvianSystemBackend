using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelAccommodation", Schema = "public")]
    public class TrxTravelAccommodation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public Guid? BusinessTravelParticipantId { get; set; }
        public Guid? DestinationZoneId { get; set; }
        public Guid? TravelClassId { get; set; }

        [Required, MaxLength(200)]
        public string AccommodationName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AccommodationAddress { get; set; }

        [MaxLength(100)]
        public string? BookingReference { get; set; }

        [MaxLength(100)]
        public string? RoomType { get; set; }

        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public int NightCount { get; set; } = 0;

        public decimal EstimatedAmount { get; set; } = 0m;
        public decimal ActualAmount { get; set; } = 0m;
        public decimal ApprovedAmount { get; set; } = 0m;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required, MaxLength(30)]
        public string AccommodationStatus { get; set; } = "Planned";
        // Planned, Booked, CheckedIn, CheckedOut, Cancelled, Refunded.

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public TrxBusinessTravelParticipant? BusinessTravelParticipant { get; set; }
        public MstTravelDestinationZone? DestinationZone { get; set; }
        public MstTravelClass? TravelClass { get; set; }

        public ICollection<TrxTravelExpenseItem> ExpenseItems { get; set; } = new List<TrxTravelExpenseItem>();
    }
}
