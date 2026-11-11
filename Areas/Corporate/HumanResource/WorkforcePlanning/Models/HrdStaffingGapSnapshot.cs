using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdStaffingGapSnapshot", Schema = "public")]
    public class HrdStaffingGapSnapshot : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public StaffingGapSnapshotType SnapshotType { get; set; }

        [Required]
        public Guid HospitalSiteId { get; set; }

        public DateOnly ReferenceDate { get; set; }

        public PositionFormationSourceType? SourceDocumentType { get; set; }

        public Guid? SourceDocumentId { get; set; }

        public DateOnly? PeriodMonth { get; set; }

        public DateTime TakenAt { get; set; }

        public DateTime? ReconstructedAt { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public ICollection<HrdStaffingGapSnapshotLine> Lines { get; set; } = new List<HrdStaffingGapSnapshotLine>();
    }

    [Table("HrdStaffingGapSnapshotLine", Schema = "public")]
    public class HrdStaffingGapSnapshotLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SnapshotId { get; set; }

        [Required]
        public Guid OrganizationUnitId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public decimal? FormationQuantity { get; set; }

        public decimal? FilledFte { get; set; }

        public decimal? ToBeVacatedFte { get; set; }

        public decimal? TemporarilyUnavailableFte { get; set; }

        public decimal? NotYetCompetentFte { get; set; }

        public decimal? OutsourcedFte { get; set; }

        public int? ConcurrentAssignmentCount { get; set; }

        public decimal? PermanentGap { get; set; }

        public decimal? TemporaryGap { get; set; }

        public decimal? CompetencyGap { get; set; }

        public bool PlacementDataComplete { get; set; } = false;

        public bool LeaveDataComplete { get; set; } = false;

        public bool CompetencyDataComplete { get; set; } = false;

        public bool WorkforceTypeDataComplete { get; set; } = false;

        public int? ShortageSummaryCount { get; set; }

        public HrdStaffingGapSnapshot? Snapshot { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstPosition? Position { get; set; }
    }
}
