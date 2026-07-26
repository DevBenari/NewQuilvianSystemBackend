using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxAccessRevocation", Schema = "public")]
    public class TrxAccessRevocation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid ExitClearanceId { get; set; }
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        [Required, MaxLength(150)] public string SystemName { get; set; } = string.Empty;
        [MaxLength(100)] public string? AccessType { get; set; }
        [MaxLength(200)] public string? AccountIdentifier { get; set; }
        public DateTime RequestedRevocationAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        [MaxLength(30)] public string RevocationStatus { get; set; } = "Pending";
        public Guid? RevokedByUserId { get; set; }
        [MaxLength(500)] public string? EvidencePath { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public TrxExitClearance? ExitClearance { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public ApplicationUser? RevokedByUser { get; set; }
    }
}
