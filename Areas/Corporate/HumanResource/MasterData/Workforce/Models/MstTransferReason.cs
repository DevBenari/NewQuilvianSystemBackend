using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstTransferReason", Schema = "public")]
    public class MstTransferReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TransferReasonCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TransferReasonName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TransferType { get; set; } = "InternalTransfer";
        // InternalTransfer, Rotation, TemporaryAssignment, SiteTransfer, Other.

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool RequiresApproval { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
