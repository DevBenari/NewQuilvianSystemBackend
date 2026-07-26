using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxPreEmploymentMedicalCheck", Schema = "public")]
    public class TrxPreEmploymentMedicalCheck : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateApplicationId { get; set; }
        public Guid? CandidateId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? WorkLocationId { get; set; }

        [MaxLength(200)]
        public string? MedicalProviderName { get; set; }

        [MaxLength(100)]
        public string? ExaminationNumber { get; set; }

        public DateTime? RequestedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? ExaminedAt { get; set; }
        public DateTime? ResultIssuedAt { get; set; }

        [MaxLength(30)]
        public string MedicalCheckStatus { get; set; } = "Requested";

        [MaxLength(30)]
        public string? FitnessResult { get; set; }
        // Fit, FitWithRestriction, TemporarilyUnfit, Unfit, ReviewRequired.

        [MaxLength(2000)]
        public string? WorkRestrictions { get; set; }

        public DateTime? ValidUntil { get; set; }

        [MaxLength(500)]
        public string? ResultDocumentPath { get; set; }

        public Guid? ReviewedByWorkforceProfileId { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        [MaxLength(1500)]
        public string? AdministrativeNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxCandidateApplication? CandidateApplication { get; set; }
        public TrxCandidate? Candidate { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public MstWorkforceProfile? ReviewedByWorkforceProfile { get; set; }
        public ApplicationUser? ReviewedByUser { get; set; }
    }
}
