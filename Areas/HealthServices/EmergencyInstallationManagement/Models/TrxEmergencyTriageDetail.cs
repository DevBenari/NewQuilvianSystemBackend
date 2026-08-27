using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("TrxEmergencyTriageDetail", Schema = "public")]
    public class TrxEmergencyTriageDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyTriageId { get; set; }

        public Guid? TriageIndicatorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string IndicatorCodeSnapshot { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string IndicatorNameSnapshot { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IndicatorGroupSnapshot { get; set; }

        [MaxLength(500)]
        public string? ObservedValue { get; set; }

        public decimal? Score { get; set; }

        public bool IsMatched { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public int Sequence { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxEmergencyTriage? EmergencyTriage { get; set; }

        public MstEmergencyTriageIndicator? TriageIndicator { get; set; }
    }
}
