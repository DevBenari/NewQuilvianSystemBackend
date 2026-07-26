using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelItinerary", Schema = "public")]
    public class TrxTravelItinerary : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public Guid? BusinessTravelParticipantId { get; set; }
        public Guid? DestinationZoneId { get; set; }

        public int SequenceNumber { get; set; } = 1;
        public DateOnly ItineraryDate { get; set; }
        public DateTime? PlannedStartAt { get; set; }
        public DateTime? PlannedEndAt { get; set; }
        public DateTime? ActualStartAt { get; set; }
        public DateTime? ActualEndAt { get; set; }

        [Required, MaxLength(250)]
        public string Origin { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string Destination { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string ActivityDescription { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TransportMode { get; set; }

        [MaxLength(500)]
        public string? VenueAddress { get; set; }

        [MaxLength(150)]
        public string? ContactPersonName { get; set; }

        [MaxLength(100)]
        public string? ContactPersonPhone { get; set; }

        [Required, MaxLength(30)]
        public string ItineraryStatus { get; set; } = "Planned";
        // Planned, Confirmed, InProgress, Completed, Cancelled.

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public TrxBusinessTravelParticipant? BusinessTravelParticipant { get; set; }
        public MstTravelDestinationZone? DestinationZone { get; set; }

        public ICollection<TrxTravelTransportation> Transportations { get; set; } = new List<TrxTravelTransportation>();
        public ICollection<TrxTravelExpenseItem> ExpenseItems { get; set; } = new List<TrxTravelExpenseItem>();
    }
}
