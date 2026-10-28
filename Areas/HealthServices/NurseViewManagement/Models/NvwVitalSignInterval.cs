using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models
{
    [Table("NvwVitalSignInterval", Schema = "public")]
    public class NvwVitalSignInterval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public EwsRiskLevel EwsRiskLevel { get; set; }

        [Range(1, 1440)]
        public int IntervalMinutes { get; set; }

        [Required]
        [MaxLength(200)]
        public string RatificationReference { get; set; } = string.Empty;

        public DateOnly RatifiedOn { get; set; }
    }
}
