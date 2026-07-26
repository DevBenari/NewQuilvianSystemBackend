using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxBusinessTravelParticipant", Schema = "public")]
    public class TrxBusinessTravelParticipant : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? TravelClassId { get; set; }
        public Guid? TravelAllowanceRateId { get; set; }

        [Required, MaxLength(30)]
        public string ParticipantRole { get; set; } = "Participant";
        // Requester, Leader, Participant, Companion, Driver, Other.

        public bool IsPrimaryParticipant { get; set; } = false;
        public bool RequiresAccommodation { get; set; } = true;
        public bool RequiresTransportation { get; set; } = true;
        public bool IsAllowanceEligible { get; set; } = true;

        public decimal EstimatedAllowanceAmount { get; set; } = 0m;
        public decimal ApprovedAllowanceAmount { get; set; } = 0m;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required, MaxLength(30)]
        public string ParticipantStatus { get; set; } = "Proposed";
        // Proposed, Approved, Rejected, Confirmed, Withdrawn, Completed.

        public DateTime? ConfirmedAt { get; set; }
        public Guid? ConfirmedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstTravelClass? TravelClass { get; set; }
        public MstTravelAllowanceRate? TravelAllowanceRate { get; set; }
        public ApplicationUser? ConfirmedByUser { get; set; }

        public ICollection<TrxTravelItinerary> Itineraries { get; set; } = new List<TrxTravelItinerary>();
        public ICollection<TrxTravelTransportation> Transportations { get; set; } = new List<TrxTravelTransportation>();
        public ICollection<TrxTravelAccommodation> Accommodations { get; set; } = new List<TrxTravelAccommodation>();
        public ICollection<TrxTravelAdvanceRequest> AdvanceRequests { get; set; } = new List<TrxTravelAdvanceRequest>();
        public ICollection<TrxTravelExpenseClaim> ExpenseClaims { get; set; } = new List<TrxTravelExpenseClaim>();
        public ICollection<TrxTravelDocument> Documents { get; set; } = new List<TrxTravelDocument>();
        public ICollection<TrxTravelAttendanceLink> AttendanceLinks { get; set; } = new List<TrxTravelAttendanceLink>();
    }
}
