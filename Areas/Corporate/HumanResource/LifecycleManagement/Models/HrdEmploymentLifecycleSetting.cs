using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdEmploymentLifecycleSetting", Schema = "public")]
    public class HrdEmploymentLifecycleSetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid HospitalSiteId { get; set; }

        public int ContractReminderDays { get; set; } = 60;

        public int ContractEscalationDays { get; set; } = 14;

        public int ProbationReminderDays { get; set; } = 14;

        public int LateStartFlagDays { get; set; } = 3;

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstHospitalSite? HospitalSite { get; set; }
    }
}
