using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Disiplin yang diresepkan Sp.KFR lewat asesmen medis final — <c>data-dictionary.md</c>
    /// bagian 2.4. Dibentuk saat asesmen medis ditandatangani.
    /// </summary>
    [Table("RhbEpisodeDiscipline", Schema = "public")]
    public class RhbEpisodeDiscipline : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public RhbEpisode? Episode { get; set; }

        [Required]
        public Guid DisciplineId { get; set; }

        public RhbDiscipline? Discipline { get; set; }

        [Required]
        public Guid PrescribedByAssessmentId { get; set; }

        public RhbAssessment? PrescribedByAssessment { get; set; }

        /// <summary>Kewaspadaan medis untuk disiplin ini. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? MedicalPrecautionText { get; set; }
    }
}
