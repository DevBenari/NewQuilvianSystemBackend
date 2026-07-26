using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelDocument", Schema = "public")]
    public class TrxTravelDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public Guid? BusinessTravelParticipantId { get; set; }
        public Guid? TravelExpenseClaimId { get; set; }
        public Guid? TravelExpenseItemId { get; set; }

        [Required, MaxLength(50)]
        public string DocumentType { get; set; } = "Other";
        // InvitationLetter, TravelOrder, Passport, Visa, Ticket, HotelVoucher,
        // Receipt, ActivityReport, SettlementEvidence, Other.

        [Required, MaxLength(255)]
        public string DocumentName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? FileName { get; set; }

        [MaxLength(150)]
        public string? ContentType { get; set; }

        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        public long? FileSizeBytes { get; set; }
        public DateOnly? IssueDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public bool IsRequiredDocument { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }

        [Required, MaxLength(30)]
        public string DocumentStatus { get; set; } = "Uploaded";
        // Uploaded, UnderReview, Verified, Rejected, Expired, Deleted.

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public TrxBusinessTravelParticipant? BusinessTravelParticipant { get; set; }
        public TrxTravelExpenseClaim? TravelExpenseClaim { get; set; }
        public TrxTravelExpenseItem? TravelExpenseItem { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
