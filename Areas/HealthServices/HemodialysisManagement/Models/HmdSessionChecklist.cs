using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Hasil pemeriksaan satu butir checklist Pra-HD pada satu sesi.
    /// </summary>
    /// <remarks>
    /// Pelewatan butir hanya sah bila butir masternya bertanda <c>IsOverridable = true</c>.
    /// Selama badan klinis belum memutuskan, seluruh butir bertanda <c>false</c> (<c>HMD-ASM-001</c>).
    /// </remarks>
    [Table("HmdSessionChecklist", Schema = "public")]
    public class HmdSessionChecklist : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        public HmdSession? Session { get; set; }

        [Required]
        public Guid ChecklistItemId { get; set; }

        public HmdChecklistItem? ChecklistItem { get; set; }

        public HmdChecklistResult Result { get; set; } = HmdChecklistResult.NotChecked;

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public bool IsOverridden { get; set; }

        /// <summary>Alasan pelewatan; wajib bila dilewati. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? OverrideReason { get; set; }

        public Guid? OverriddenByUserId { get; set; }

        public DateTime? OverriddenAt { get; set; }
    }
}
