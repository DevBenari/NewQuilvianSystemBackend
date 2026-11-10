using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdStaffingStandard", Schema = "public")]
    public class HrdStaffingStandard : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string StandardNumber { get; set; } = string.Empty;

        [Required]
        public Guid HospitalSiteId { get; set; }

        [Required]
        public Guid OrganizationUnitId { get; set; }

        public int VersionNumber { get; set; }

        public DateOnly EffectiveStartDate { get; set; }

        public DateOnly? EffectiveEndDate { get; set; }

        public StaffingStandardStatus StandardStatus { get; set; } = StaffingStandardStatus.Draft;

        [MaxLength(1000)]
        public string? Justification { get; set; }

        public Guid? SupersedesStandardId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? ActivatedAt { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public HrdStaffingStandard? SupersedesStandard { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }

        public ICollection<HrdStaffingStandardLine> Lines { get; set; } = new List<HrdStaffingStandardLine>();
        public ICollection<HrdStaffingSkillMixRule> SkillMixRules { get; set; } = new List<HrdStaffingSkillMixRule>();
    }

    [Table("HrdStaffingStandardLine", Schema = "public")]
    public class HrdStaffingStandardLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid StaffingStandardId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public Guid? ProfessionId { get; set; }

        public StaffingCalculationMethod CalculationMethod { get; set; }

        public decimal? MinimumHeadcount { get; set; }

        public decimal? RatioStaffQuantity { get; set; }

        public decimal? RatioWorkloadQuantity { get; set; }

        [MaxLength(50)]
        public string? WorkloadUnit { get; set; }

        public int? ShiftsPerDay { get; set; }

        public decimal? StaffPerShift { get; set; }

        public decimal? ServiceHoursPerDay { get; set; }

        public decimal? ReliefFactor { get; set; }

        public StaffingRoundingMethod RoundingMethod { get; set; }

        public bool CountsOutsourced { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public HrdStaffingStandard? StaffingStandard { get; set; }
        public MstPosition? Position { get; set; }
        public MstProfession? Profession { get; set; }
    }

    [Table("HrdStaffingSkillMixRule", Schema = "public")]
    public class HrdStaffingSkillMixRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid StaffingStandardId { get; set; }

        public Guid? ShiftId { get; set; }

        public Guid? ProfessionId { get; set; }

        [Required]
        public Guid CompetencyId { get; set; }

        [MaxLength(50)]
        public string? MinimumCompetencyLevel { get; set; }

        public decimal MinimumQualifiedHeadcount { get; set; }

        public bool IsMandatory { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public HrdStaffingStandard? StaffingStandard { get; set; }
        public MstShift? Shift { get; set; }
        public MstProfession? Profession { get; set; }
        public MstCompetency? Competency { get; set; }
    }
}
