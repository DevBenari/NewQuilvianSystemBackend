using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstInpatientSetting", Schema = "public")]
    public class MstInpatientSetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = "DEFAULT";

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = "Pengaturan Rawat Inap Default";

        public int BedReservationMinutes { get; set; } = 120;

        public int DraftEpisodeExpiryHours { get; set; } = 24;

        public int InitialAssessmentTargetHours { get; set; } = 24;

        public int ProgressNoteVerificationTargetHours { get; set; } = 24;

        public int PendingClosureThresholdHours { get; set; } = 4;

        [Required]
        [MaxLength(20)]
        public string EpisodeNumberPrefix { get; set; } = "RI";

        public bool IsDefault { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
