using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdOnboardingCase", Schema = "public")]
    public class HrdOnboardingCase : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string CaseNumber { get; set; } = string.Empty;

        [Required]
        public Guid HiringHandoffId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid HospitalSiteId { get; set; }

        [Required]
        public Guid OrganizationUnitId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public bool IsClinical { get; set; } = false;

        [Required]
        public DateOnly PlannedStartDate { get; set; }

        [Required]
        public Guid CoordinatorUserId { get; set; }

        public OnboardingCaseStatus CaseStatus { get; set; } = OnboardingCaseStatus.InProgress;

        public DateOnly? FirstDayAttendedOn { get; set; }

        public Guid? FirstDayRecordedByUserId { get; set; }

        public DateTime? ReadyForWorkAt { get; set; }

        public Guid? ReadyForWorkByUserId { get; set; }

        public OnboardingCancelReason? CancelReason { get; set; }

        [MaxLength(1000)]
        public string? CancelNote { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public bool HasGeneralTemplate { get; set; } = true;

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdCandidateHiringHandoff? HiringHandoff { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstPosition? Position { get; set; }
        public ApplicationUser? CoordinatorUser { get; set; }
        public ApplicationUser? FirstDayRecordedByUser { get; set; }
        public ApplicationUser? ReadyForWorkByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }

        public ICollection<HrdOnboardingCaseItem> Items { get; set; } = new List<HrdOnboardingCaseItem>();
        public ICollection<HrdOnboardingAccessRequest> AccessRequests { get; set; } = new List<HrdOnboardingAccessRequest>();
        public HrdClinicalPreceptorship? ClinicalPreceptorship { get; set; }
    }
}
