using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models
{
    [Table("MstPerformanceCycle", Schema = "public")]
    public class MstPerformanceCycle : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CycleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CycleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CycleType { get; set; } = "Annual";
        // Annual, Semester, Quarter, Probation, Project, Custom

        public int? PeriodYear { get; set; }

        [Required]
        public DateTime PeriodStartDate { get; set; }

        [Required]
        public DateTime PeriodEndDate { get; set; }

        public DateTime? GoalSettingStartDate { get; set; }

        public DateTime? GoalSettingEndDate { get; set; }

        public DateTime? MidReviewStartDate { get; set; }

        public DateTime? MidReviewEndDate { get; set; }

        public DateTime? FinalReviewStartDate { get; set; }

        public DateTime? FinalReviewEndDate { get; set; }

        public DateTime? CalibrationStartDate { get; set; }

        public DateTime? CalibrationEndDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string CycleStatus { get; set; } = "Draft";
        // Draft, Open, GoalSetting, MidReview, FinalReview, Calibration, Completed, Closed, Cancelled

        public bool IsCurrent { get; set; } = false;

        public bool IsLocked { get; set; } = false;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public ICollection<MstPerformanceTemplate> PerformanceTemplates { get; set; }
            = new List<MstPerformanceTemplate>();
    }
}
