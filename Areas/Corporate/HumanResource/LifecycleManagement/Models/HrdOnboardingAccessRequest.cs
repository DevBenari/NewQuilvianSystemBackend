using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdOnboardingAccessRequest", Schema = "public")]
    public class HrdOnboardingAccessRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnboardingCaseId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public OnboardingAccessRequestType RequestType { get; set; }

        public OnboardingAccessRequestStatus RequestStatus { get; set; } = OnboardingAccessRequestStatus.Requested;

        public Guid? ApplicationUserId { get; set; }

        [MaxLength(500)]
        public string? FailureMessage { get; set; }

        public DateTime? SelfServiceActivatedAt { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        [MaxLength(500)]
        public string? CompletionNote { get; set; }

        public HrdOnboardingCase? OnboardingCase { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public ApplicationUser? CompletedByUser { get; set; }
    }
}
