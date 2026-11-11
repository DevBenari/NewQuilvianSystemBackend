using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("trx_exit_clearance_processes", Schema = "public")]
    public class TrxExitClearanceProcess : IdentityModel
    {
        [Key]
        [Column("clearance_process_id")]
        public Guid ClearanceProcessId { get; set; } = Guid.NewGuid();

        [NotMapped]
        public Guid Id
        {
            get => ClearanceProcessId;
            set => ClearanceProcessId = value;
        }

        [Required]
        [Column("separation_id")]
        public Guid SeparationId { get; set; }

        [Required]
        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Column("initiated_date")]
        public DateTime InitiatedDate { get; set; }

        [Column("target_completion_date")]
        public DateTime TargetCompletionDate { get; set; }

        [Column("actual_completion_date")]
        public DateTime? ActualCompletionDate { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("overall_clearance_status")]
        public string OverallClearanceStatus { get; set; } = "InProgress"; // InProgress, Completed, BlockedOutstanding

        [Column("is_final_payroll_unlocked")]
        public bool IsFinalPayrollUnlocked { get; set; } = false; // INT-HR-87 / VR-EMP-25

        [Column("finalization_notes")]
        public string? FinalizationNotes { get; set; }

        public ICollection<TrxExitClearanceChecklist> Checklists { get; set; } = new List<TrxExitClearanceChecklist>();
    }
}
