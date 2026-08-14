using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models
{
    [Table("MstEmergencyTriageLevel", Schema = "public")]
    public class MstEmergencyTriageLevel : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public EmergencyTriageSystem TriageSystem { get; set; }
            = EmergencyTriageSystem.ATS;

        [Range(1, 5)]
        public int Level { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ColorName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ColorHex { get; set; }

        /// <summary>
        /// Target waktu respons dalam menit. Kosong berarti SOP belum menetapkan target,
        /// bukan berarti nol menit. Nol menit berarti harus dilayani seketika.
        /// </summary>
        public int? MaxWaitingMinutes { get; set; }

        public bool AllowsTreatmentBeforeRegistration { get; set; }

        public int Sequence { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstEmergencyTriageIndicator> Indicators { get; set; }
            = new List<MstEmergencyTriageIndicator>();

        public ICollection<TrxEmergencyTriage> Triages { get; set; }
            = new List<TrxEmergencyTriage>();
    }
}
