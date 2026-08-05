using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("TrxOvertimePeriod", Schema = "public")]
    public class TrxOvertimePeriod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string PeriodCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string PeriodName { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required, MaxLength(30)]
        public string PeriodStatus { get; set; } = "Open";

        public bool RequireAttendanceFinal { get; set; } = true;
        public bool RequireVerificationComplete { get; set; } = true;
        public bool RequireSettlementComplete { get; set; } = true;
        public DateTime? ScheduledCloseAt { get; set; }

        public DateTime? LastValidatedAt { get; set; }
        public DateTime? LastReconciledAt { get; set; }

        [Column(TypeName = "jsonb")]
        public string? ValidationSnapshotJson { get; set; }

        [Column(TypeName = "jsonb")]
        public string? ReconciliationSnapshotJson { get; set; }

        public DateTime? ClosedAt { get; set; }
        public Guid? ClosedByUserId { get; set; }

        [MaxLength(1000)]
        public string? CloseReason { get; set; }

        public DateTime? ReopenedAt { get; set; }
        public Guid? ReopenedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ReopenReason { get; set; }

        public int ReopenCount { get; set; } = 0;
        public int CloseVersion { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }
        public ApplicationUser? ReopenedByUser { get; set; }

        public ICollection<TrxOvertimeSchedulerJob> SchedulerJobs { get; set; }
            = new List<TrxOvertimeSchedulerJob>();
    }
}
