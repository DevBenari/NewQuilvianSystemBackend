using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Riwayat perpindahan status permintaan, episode, program, dan sesi —
    /// <c>data-dictionary.md</c> bagian 2.17.
    /// </summary>
    /// <remarks>
    /// Rujukan ke aggregate bersifat polimorfik (<see cref="AggregateType"/> +
    /// <see cref="AggregateId"/>), tanpa FK. Baris tidak pernah diubah atau dihapus.
    /// </remarks>
    [Table("RhbStatusHistory", Schema = "public")]
    public class RhbStatusHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Contoh: "Request", "Episode", "Program", "Session".</summary>
        [Required]
        [MaxLength(40)]
        public string AggregateType { get; set; } = string.Empty;

        [Required]
        public Guid AggregateId { get; set; }

        [MaxLength(40)]
        public string? FromStatus { get; set; }

        [Required]
        [MaxLength(40)]
        public string ToStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string Action { get; set; } = string.Empty;

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? ReasonText { get; set; }

        [Required]
        public Guid ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
