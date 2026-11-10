using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdWorkforcePlanningSetting", Schema = "public")]
    public class HrdWorkforcePlanningSetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid HospitalSiteId { get; set; }

        public int? LongLeaveThresholdDays { get; set; }

        public int? OverdueRequestThresholdDays { get; set; }

        [Required]
        [MaxLength(500)]
        public string ChangeReason { get; set; } = string.Empty;

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstHospitalSite? HospitalSite { get; set; }
    }
}
