using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("TrxOvertimePlan", Schema = "public")]
    public class TrxOvertimePlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string PlanNumber { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string PlanTitle { get; set; } = string.Empty;

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? RosterPeriodId { get; set; }

        public DateOnly PlanStartDate { get; set; }
        public DateOnly PlanEndDate { get; set; }

        [Required, MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Required, MaxLength(40)]
        public string PlanStatus { get; set; } = "Draft";

        public DateTime? ValidatedAt { get; set; }
        public Guid? ValidatedByUserId { get; set; }
        public DateTime? PublishedAt { get; set; }
        public Guid? PublishedByUserId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public Guid? ClosedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public TrxRosterPeriod? RosterPeriod { get; set; }
        public ApplicationUser? ValidatedByUser { get; set; }
        public ApplicationUser? PublishedByUser { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }

        public ICollection<TrxOvertimePlanDetail> Details { get; set; }
            = new List<TrxOvertimePlanDetail>();
    }
}
