using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models
{
    [Table("NvwPatientCallWaitThreshold", Schema = "public")]
    public class NvwPatientCallWaitThreshold : IdentityModel
    {
        public const string GlobalScopeCode = "GLOBAL";

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string ScopeCode { get; set; } = GlobalScopeCode;

        [Range(1, 240)]
        public int WarningAfterMinutes { get; set; }

        [Range(2, 480)]
        public int CriticalAfterMinutes { get; set; }

        [Required]
        [MaxLength(200)]
        public string RatificationReference { get; set; } = string.Empty;

        public DateOnly RatifiedOn { get; set; }
    }
}
