using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    [Table("MstPatientMembership", Schema = "public")]
    public class MstPatientMembership : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid MembershipTierId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MemberNumber { get; set; } = string.Empty;

        public MembershipStatus MembershipStatus { get; set; } = MembershipStatus.Active;

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiredDate { get; set; }

        public bool IsPrimary { get; set; } = true;

        public bool IsAutoCreated { get; set; } = false;

        public bool IsCreatedFromKiosk { get; set; } = false;

        public bool IsCreatedFromAdmission { get; set; } = false;

        public bool IsCreatedByMarketing { get; set; } = false;

        public int PointBalance { get; set; } = 0;

        public decimal TotalSpendAmount { get; set; } = 0;

        public DateTime? LastUpgradeDate { get; set; }

        public DateTime? LastDowngradeDate { get; set; }

        [MaxLength(250)]
        public string? UpgradeDowngradeReason { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPatient? Patient { get; set; }

        public MstMembershipTier? MembershipTier { get; set; }
    }
}
