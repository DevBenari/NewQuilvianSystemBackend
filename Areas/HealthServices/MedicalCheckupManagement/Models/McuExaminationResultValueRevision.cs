using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Nilai lama sebuah isian yang direvisi sesudah resume dibuka ulang, beserta versi resume yang
    /// pernah disetujui dengan nilai itu.
    /// </summary>
    [Table("McuExaminationResultValueRevision", Schema = "public")]
    public class McuExaminationResultValueRevision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ResultValueId { get; set; }

        public McuExaminationResultValue? ResultValue { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? PreviousValueText { get; set; }

        public int ResumeVersionNumber { get; set; }

        [Required]
        public Guid ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
