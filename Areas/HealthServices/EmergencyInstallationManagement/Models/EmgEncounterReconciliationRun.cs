using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgEncounterReconciliationRun", Schema = "public")]
    public class EmgEncounterReconciliationRun : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RunNumber { get; set; } = string.Empty;

        public EmergencyReconciliationRunStatus Status { get; set; }
            = EmergencyReconciliationRunStatus.Executed;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public int CountK1 { get; set; }

        public int CountK1Outpatient { get; set; }

        public int CountK2 { get; set; }

        public int CountK3 { get; set; }

        public int CountK4 { get; set; }

        [Required]
        public Guid ExecutedByUserId { get; set; }

        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        public Guid? ReversedByUserId { get; set; }

        public DateTime? ReversedAt { get; set; }

        [MaxLength(500)]
        public string? ReverseReason { get; set; }

        public ApplicationUser? ExecutedByUser { get; set; }

        public ApplicationUser? ReversedByUser { get; set; }

        public ICollection<EmgEncounterReconciliationItem> Items { get; set; }
            = new List<EmgEncounterReconciliationItem>();
    }
}
