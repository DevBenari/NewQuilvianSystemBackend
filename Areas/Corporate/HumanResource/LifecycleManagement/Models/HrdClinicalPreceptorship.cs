using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdClinicalPreceptorship", Schema = "public")]
    public class HrdClinicalPreceptorship : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnboardingCaseId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid OrganizationUnitId { get; set; }

        public PreceptorshipStatus PreceptorshipStatus { get; set; } = PreceptorshipStatus.PreceptorAssigned;

        public DateOnly? AccompaniedSince { get; set; }

        public DateOnly? IndependentOn { get; set; }

        public Guid? DeclaredByUserId { get; set; }

        [MaxLength(500)]
        public string? EvidenceStoragePath { get; set; }

        [MaxLength(250)]
        public string? EvidenceFileName { get; set; }

        [MaxLength(150)]
        public string? EvidenceContentType { get; set; }

        [MaxLength(2000)]
        public string? AssessmentNote { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdOnboardingCase? OnboardingCase { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public ApplicationUser? DeclaredByUser { get; set; }
        public ICollection<HrdClinicalPreceptorshipPreceptor> Preceptors { get; set; } = new List<HrdClinicalPreceptorshipPreceptor>();
    }
}
