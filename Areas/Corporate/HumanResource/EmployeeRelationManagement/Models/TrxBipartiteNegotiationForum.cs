using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("trx_bipartite_negotiation_forums", Schema = "public")]
    public class TrxBipartiteNegotiationForum : IdentityModel
    {
        [Key]
        [Column("forum_id")]
        public Guid ForumId { get; set; } = Guid.NewGuid();

        [NotMapped]
        public Guid Id
        {
            get => ForumId;
            set => ForumId = value;
        }

        [Required]
        [MaxLength(64)]
        [Column("register_number")]
        public string RegisterNumber { get; set; } = string.Empty;

        [Column("meeting_date")]
        public DateTime MeetingDate { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("dispute_type")]
        public string DisputeType { get; set; } = "RightsDispute"; // RightsDispute, InterestDispute, TerminationDispute, UnionDispute

        [Required]
        [Column("management_representatives")]
        public string ManagementRepresentatives { get; set; } = string.Empty;

        [Required]
        [Column("union_representatives")]
        public string UnionRepresentatives { get; set; } = string.Empty;

        [Required]
        [Column("agenda_description")]
        public string AgendaDescription { get; set; } = string.Empty;

        [Required]
        [Column("minutes_of_meeting")]
        public string MinutesOfMeeting { get; set; } = string.Empty;

        [MaxLength(512)]
        [Column("joint_agreement_document_url")]
        public string? JointAgreementDocumentUrl { get; set; } // Harus .pdf jika status CompletedAgreementReached

        [MaxLength(128)]
        [Column("registration_disnaker_number")]
        public string? RegistrationDisnakerNumber { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "Scheduled"; // Scheduled, CompletedAgreementReached, Deadlocked, Cancelled
    }
}
