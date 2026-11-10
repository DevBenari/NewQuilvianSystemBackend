using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdCandidateHiringHandoff", Schema = "public")]
    public class HrdCandidateHiringHandoff : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string HandoffNumber { get; set; } = string.Empty;

        [Required]
        public Guid OfferId { get; set; }

        [Required]
        public Guid CandidateId { get; set; }

        public Guid? WorkforceProfileId { get; set; }

        public Guid? HeadcountRequestSeatId { get; set; }

        public DateTime HandoffDate { get; set; } = DateTime.UtcNow;

        public HandoffStatus Status { get; set; } = HandoffStatus.Generated;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdJobOffer? Offer { get; set; }
        public HrdCandidate? Candidate { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public HrdHeadcountRequestSeat? HeadcountRequestSeat { get; set; }
    }
}
