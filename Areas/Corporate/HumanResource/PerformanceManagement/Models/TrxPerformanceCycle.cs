using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Siklus Periode Evaluasi Kinerja Rumah Sakit (HRD-DEC-164, PRF-B1, VR-PRF-01..02)
    /// </summary>
    [Table("trx_performance_cycles", Schema = "public")]
    public class TrxPerformanceCycle : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string CycleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CycleTitle { get; set; } = string.Empty;

        public int EvaluationYear { get; set; }

        /// <summary>
        /// ANNUAL, SEMESTER_1, SEMESTER_2, SPECIAL_PROBATION
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string CycleType { get; set; } = "ANNUAL";

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime SelfAppraisalDeadline { get; set; }

        public DateTime ManagerAppraisalDeadline { get; set; }

        public DateTime CalibrationDeadline { get; set; }

        /// <summary>
        /// Draft, Active, InReview, Calibrating, Closed, Cancelled
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Draft";

        public bool AllowEmployeeViewFinalResult { get; set; } = false;

        public bool RequireSelfAssessment { get; set; } = true;

        public bool EnablePeerFeedback { get; set; } = false;

        public int TotalEmployee { get; set; } = 0;

        public int CompletedEmployee { get; set; } = 0;

        // Compatibility aliases for legacy properties
        [NotMapped]
        public string CycleName { get => CycleTitle; set => CycleTitle = value; }

        [NotMapped]
        public int CycleYear { get => EvaluationYear; set => EvaluationYear = value; }

        [NotMapped]
        public DateTime CycleStartDate { get => StartDate; set => StartDate = value; }

        [NotMapped]
        public DateTime CycleEndDate { get => EndDate; set => EndDate = value; }

        [NotMapped]
        public string CycleStatus { get => Status; set => Status = value; }

        [NotMapped] public string? Description { get => CycleConfigurationJson; set => CycleConfigurationJson = value; }


        public Guid? MasterPerformanceCycleId { get; set; }
        public Guid? PerformanceTemplateId { get; set; }
        public Guid? RatingScaleId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }

        public string? CycleConfigurationJson { get; set; }
        public string? PopulationSnapshotJson { get; set; }
        public DateTime? PublishedAt { get; set; }
        public Guid? PublishedByUserId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public Guid? ClosedByUserId { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime? GoalSettingDeadline { get; set; }
        public DateTime? SelfAssessmentDeadline { get; set; }
        public DateTime? ManagerAssessmentDeadline { get; set; }

        public MstPerformanceCycle? MasterPerformanceCycle { get; set; }
        public MstPerformanceTemplate? PerformanceTemplate { get; set; }
        public MstPerformanceRatingScale? RatingScale { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? PublishedByUser { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }

        public ICollection<TrxEmployeeGoal> EmployeeGoals { get; set; } = new List<TrxEmployeeGoal>();
        public ICollection<TrxEmployeeKpiTarget> KpiTargets { get; set; } = new List<TrxEmployeeKpiTarget>();
        public ICollection<WfpPerformanceReview> PerformanceReviews { get; set; } = new List<WfpPerformanceReview>();
        public ICollection<TrxSelfAssessment> SelfAssessments { get; set; } = new List<TrxSelfAssessment>();
        public ICollection<TrxManagerAssessment> ManagerAssessments { get; set; } = new List<TrxManagerAssessment>();
        public ICollection<TrxCalibrationSession> CalibrationSessions { get; set; } = new List<TrxCalibrationSession>();

        // Modern S-C3 navigation properties
        public virtual ICollection<TrxPerformanceGoal> PerformanceGoals { get; set; } = new List<TrxPerformanceGoal>();
        public virtual ICollection<TrxPerformanceAppraisal> PerformanceAppraisals { get; set; } = new List<TrxPerformanceAppraisal>();
        public virtual ICollection<TrxPerformanceCalibrationSession> PerformanceCalibrationSessions { get; set; } = new List<TrxPerformanceCalibrationSession>();
    }
}
