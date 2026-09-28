using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>Master butir pemeriksaan kesiapan unit HD per shift.</summary>
    [Table("HmdReadinessItem", Schema = "public")]
    public class HmdReadinessItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        public HmdReadinessCategory Category { get; set; }

        public bool IsMandatory { get; set; } = true;

        /// <summary>
        /// Butir yang hasilnya punya masa berlaku, misalnya pemeriksaan pengolahan air. Masa
        /// berlakunya dibaca dari <c>HmdSetting.WaterResultValidityHours</c>.
        /// </summary>
        public bool RequiresResultDate { get; set; }

        /// <summary>Urutan pemeriksaan butir.</summary>
        public int CheckSequence { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
