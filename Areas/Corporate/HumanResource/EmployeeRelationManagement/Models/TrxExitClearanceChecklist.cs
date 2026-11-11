using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("trx_exit_clearance_checklists", Schema = "public")]
    public class TrxExitClearanceChecklist : IdentityModel
    {
        [Key]
        [Column("checklist_id")]
        public Guid ChecklistId { get; set; } = Guid.NewGuid();

        [NotMapped]
        public Guid Id
        {
            get => ChecklistId;
            set => ChecklistId = value;
        }

        [Required]
        [Column("clearance_process_id")]
        public Guid ClearanceProcessId { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("department_type")]
        public string DepartmentType { get; set; } = string.Empty; // WorkUnitHandover, GeneralAffairsAsset, PharmacyMedicalDevice, FinanceCooperative, InformationTechnology

        [Required]
        [MaxLength(255)]
        [Column("item_name")]
        public string ItemName { get; set; } = string.Empty;

        [Column("is_returned")]
        public bool IsReturned { get; set; } = false;

        [Column("outstanding_liability_amount", TypeName = "numeric(14,2)")]
        public decimal OutstandingLiabilityAmount { get; set; } = 0m;

        [Column("approver_notes")]
        public string? ApproverNotes { get; set; }

        [Column("approved_by_user_id")]
        public Guid? ApprovedByUserId { get; set; }

        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "Pending"; // Pending, Approved, RejectedOutstanding

        public TrxExitClearanceProcess? ClearanceProcess { get; set; }
    }
}
