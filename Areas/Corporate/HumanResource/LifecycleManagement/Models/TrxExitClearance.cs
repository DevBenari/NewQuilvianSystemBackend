using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxExitClearance", Schema = "public")]
    public class TrxExitClearance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string ClearanceNumber { get; set; } = string.Empty;
        [Required] public Guid EmployeeSeparationId { get; set; }
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OffboardingChecklistId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        [MaxLength(30)] public string ClearanceStatus { get; set; } = "Draft";
        public int TotalItem { get; set; }
        public int ClearedItem { get; set; }
        public decimal ProgressPercentage { get; set; }
        public bool IsDepartmentCleared { get; set; }
        public bool IsAssetCleared { get; set; }
        public bool IsFinanceCleared { get; set; }
        public bool IsPayrollCleared { get; set; }
        public bool IsItAccessCleared { get; set; }
        public bool IsHrCleared { get; set; }
        public Guid? CompletedByUserId { get; set; }
        [MaxLength(1500)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public TrxEmployeeSeparation? EmployeeSeparation { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOffboardingChecklist? OffboardingChecklist { get; set; }
        public ApplicationUser? CompletedByUser { get; set; }
        public ICollection<TrxAssetReturn> AssetReturns { get; set; } = new List<TrxAssetReturn>();
        public ICollection<TrxAccessRevocation> AccessRevocations { get; set; } = new List<TrxAccessRevocation>();
    }
}
