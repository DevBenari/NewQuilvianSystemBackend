using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Hasil pemeriksaan kesiapan unit pada satu tanggal dan shift — aggregate root.
    /// </summary>
    /// <remarks>
    /// Berubah menjadi <c>Ready</c> hanya bila seluruh butir wajib terpenuhi. Berubah menjadi
    /// <c>NotReady</c> di tengah shift <b>tidak</b> menghentikan sesi yang sedang berjalan.
    /// </remarks>
    [Table("HmdUnitReadiness", Schema = "public")]
    public class HmdUnitReadiness : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceUnitId { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public DateOnly ReadinessDate { get; set; }

        public HmdShift Shift { get; set; }

        public HmdReadinessStatus ReadinessStatus { get; set; } = HmdReadinessStatus.Draft;

        public Guid? DeclaredByUserId { get; set; }

        public DateTime? DeclaredAt { get; set; }

        [MaxLength(1000)]
        public string? NotReadyReason { get; set; }

        public int Version { get; set; }

        public ICollection<HmdUnitReadinessDetail> Details { get; set; } = new List<HmdUnitReadinessDetail>();
    }
}
