using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstWorkerSource", Schema = "public")]
    public class MstWorkerSource : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string WorkerSourceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string WorkerSourceName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SourceType { get; set; }
        // InternalRecruitment, Referral, Vendor, Outsourcing, Partnership, Other.

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsExternal { get; set; } = false;

        public bool RequiresVendorInformation { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
