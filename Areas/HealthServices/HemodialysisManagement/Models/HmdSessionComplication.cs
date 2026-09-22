using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Kejadian tidak diinginkan selama sesi beserta penanganannya. Sistem tidak menyimpulkan
    /// diagnosis dari nilai tanda vital; baris ini selalu lahir dari penilaian manusia.
    /// </summary>
    [Table("HmdSessionComplication", Schema = "public")]
    public class HmdSessionComplication : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        public HmdSession? Session { get; set; }

        public HmdComplicationType ComplicationType { get; set; }

        public DateTime DetectedAt { get; set; }

        [Required]
        public Guid DetectedByUserId { get; set; }

        /// <summary>Derajat keparahan sebagaimana dinilai petugas.</summary>
        [MaxLength(50)]
        public string? Severity { get; set; }

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(1000)]
        public string SignsAndSymptoms { get; set; } = string.Empty;

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(1000)]
        public string Intervention { get; set; } = string.Empty;

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? ClinicianInstruction { get; set; }

        public HmdComplicationOutcome Outcome { get; set; }

        public HmdComplicationSessionImpact SessionImpact { get; set; }

        [MaxLength(200)]
        public string? TransferDestination { get; set; }
    }
}
