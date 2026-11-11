using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdStaffingWorkloadInput", Schema = "public")]
    public class HrdStaffingWorkloadInput : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationUnitId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public DateOnly PeriodStartDate { get; set; }

        public DateOnly PeriodEndDate { get; set; }

        public decimal WorkloadValue { get; set; }

        [Required]
        [MaxLength(500)]
        public string SourceNote { get; set; } = string.Empty;

        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstPosition? Position { get; set; }
    }
}
