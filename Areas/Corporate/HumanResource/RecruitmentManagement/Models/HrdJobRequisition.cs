using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdJobRequisition", Schema = "public")]
    public class HrdJobRequisition : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RequisitionNumber { get; set; } = string.Empty;

        [Required]
        public Guid HeadcountRequestId { get; set; }

        [Required]
        public Guid HospitalSiteId { get; set; }

        [Required]
        public Guid OrganizationUnitId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public int RequestedSeats { get; set; }

        public int ApprovedSeats { get; set; }

        public int FulfilledSeats { get; set; }

        public JobRequisitionStatus Status { get; set; } = JobRequisitionStatus.Draft;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdHeadcountRequest? HeadcountRequest { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstPosition? Position { get; set; }
        public ICollection<HrdJobVacancy> Vacancies { get; set; } = new List<HrdJobVacancy>();
    }
}
