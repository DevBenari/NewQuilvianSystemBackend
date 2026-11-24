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

        /// <summary>
        /// Usia pensiun normal kebijakan rumah sakit (BE-HRD-368). <c>null</c> berarti kebijakan belum
        /// ditetapkan: kandidat pensiun otomatis diblokir (<c>POLICY_NOT_CONFIGURED</c>), kasus pensiun
        /// yang sudah ada tetap dapat dibaca dan diproses.
        /// </summary>
        public int? NormalRetirementAge { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstHospitalSite? HospitalSite { get; set; }
    }
}
