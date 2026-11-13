using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Program Peningkatan Kinerja Formal 60 Hari Kalender (HRD-DEC-167, PRF-B4..B6, VR-PRF-08..10)
    /// </summary>
    [Table("HrdPerformanceImprovementPlan", Schema = "public")]
    public class HrdPerformanceImprovementPlan : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PipNumber { get; set; } = string.Empty;

        [Required]
        public Guid PerformanceAppraisalId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid MentorWorkforceProfileId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public string ActionPlanSummary { get; set; } = string.Empty;

        /// <summary>
        /// Draft, Active, Extended, Completed, Failed, Terminated
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// PASSED_NORMAL, EXTENDED_30_DAYS, ESCALATED_HR_LEGAL
        /// </summary>
        [MaxLength(30)]
        public string? FinalVerdict { get; set; }

        public string? VerdictNotes { get; set; }

        // Legacy compatibility properties
        [NotMapped]
        public string PipStatus { get => Status; set => Status = value; }

        [NotMapped]
        public string Reason { get => ActionPlanSummary; set => ActionPlanSummary = value; }

        public Guid? PerformanceCycleId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? ManagerUserId { get; set; }
        public Guid? PerformanceReviewId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? ObjectivesJson { get; set; }
        public string? ActionPlanJson { get; set; }
        public string? SuccessMetricsJson { get; set; }
        public int CheckInFrequencyDays { get; set; } = 30;
        public decimal ProgressPercentage { get; set; } = 0m;
        public string? Outcome { get; set; }
        public string? OutcomeNote { get; set; }
        public bool IsEmployeeAcknowledged { get; set; } = false;
        public DateTime? EmployeeAcknowledgedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public bool IsActive { get; set; } = true;

        public HrdPerformanceCycle? PerformanceCycle { get; set; }
        public virtual HrdPerformanceAppraisal? PerformanceAppraisal { get; set; }
        public virtual MstWorkforceProfile? WorkforceProfile { get; set; }
        public virtual MstWorkforceProfile? MentorWorkforceProfile { get; set; }

        [NotMapped] public Guid MentorStaffId { get => MentorWorkforceProfileId; set => MentorWorkforceProfileId = value; }
        [NotMapped] public string ImprovementObjectives { get => ActionPlanSummary; set => ActionPlanSummary = value; }
        [NotMapped] public string? FinalVerdictNotes { get => VerdictNotes; set => VerdictNotes = value; }
        [NotMapped] public decimal BaselineScore { get; set; } = 60.00m;
        [NotMapped] public int ExtensionCount { get; set; } = 0;

        public virtual ICollection<HrdPerformancePipActionLog> ActionLogs { get; set; } = new List<HrdPerformancePipActionLog>();
    }
}
