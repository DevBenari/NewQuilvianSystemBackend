using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>Hasil satu butir pemeriksaan kesiapan unit.</summary>
    [Table("HmdUnitReadinessDetail", Schema = "public")]
    public class HmdUnitReadinessDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UnitReadinessId { get; set; }

        public HmdUnitReadiness? UnitReadiness { get; set; }

        [Required]
        public Guid ReadinessItemId { get; set; }

        public HmdReadinessItem? ReadinessItem { get; set; }

        public HmdChecklistResult Result { get; set; } = HmdChecklistResult.NotChecked;

        /// <summary>Tanggal hasil, untuk butir yang punya masa berlaku.</summary>
        public DateOnly? ResultDate { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
