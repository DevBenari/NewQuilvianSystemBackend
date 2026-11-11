using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdPositionFormation", Schema = "public")]
    public class HrdPositionFormation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid HospitalSiteId { get; set; }

        [Required]
        public Guid OrganizationUnitId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public int VersionNumber { get; set; }

        public decimal FormationQuantity { get; set; }

        public DateOnly EffectiveStartDate { get; set; }

        public DateOnly? EffectiveEndDate { get; set; }

        public PositionFormationSourceType SourceType { get; set; }

        public Guid SourceDocumentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SourceDocumentNumber { get; set; } = string.Empty;

        public Guid ApprovedByUserId { get; set; }

        public DateTime ApprovedAt { get; set; }

        public Guid? PreviousFormationId { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstPosition? Position { get; set; }
        public HrdPositionFormation? PreviousFormation { get; set; }
    }
}
