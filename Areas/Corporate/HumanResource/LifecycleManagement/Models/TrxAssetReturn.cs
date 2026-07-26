using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxAssetReturn", Schema = "public")]
    public class TrxAssetReturn : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid ExitClearanceId { get; set; }
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        [Required, MaxLength(100)] public string AssetCode { get; set; } = string.Empty;
        [Required, MaxLength(250)] public string AssetName { get; set; } = string.Empty;
        [MaxLength(100)] public string? AssetCategory { get; set; }
        [MaxLength(150)] public string? SerialNumber { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        [MaxLength(50)] public string ReturnCondition { get; set; } = "Good";
        public decimal? ReplacementCost { get; set; }
        [MaxLength(30)] public string ReturnStatus { get; set; } = "Pending";
        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public TrxExitClearance? ExitClearance { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
