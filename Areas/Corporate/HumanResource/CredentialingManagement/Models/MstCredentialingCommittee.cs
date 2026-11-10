using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("MstCredentialingCommittee", Schema = "public")]
    public class MstCredentialingCommittee : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string CommitteeCode { get; set; } = string.Empty; // MEDIK, NURSING, OTHER_HEALTH

        [Required]
        [MaxLength(200)]
        public string CommitteeName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SubCommitteeCredentialName { get; set; } = "Sub-Komite Kredensial";

        public Guid? ChairpersonWorkforceProfileId { get; set; }
        public Guid? SecretaryWorkforceProfileId { get; set; }

        public int MinimumPeerReviewersCount { get; set; } = 2; // Sesuai aturan: minimal 2 Mitra Bestari

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? ChairpersonWorkforceProfile { get; set; }
        public MstWorkforceProfile? SecretaryWorkforceProfile { get; set; }

        public ICollection<TrxCredentialingApplication> CredentialingApplications { get; set; } = new List<TrxCredentialingApplication>();
        public ICollection<TrxSpkRkkIssuance> SpkRkkIssuances { get; set; } = new List<TrxSpkRkkIssuance>();
    }
}
