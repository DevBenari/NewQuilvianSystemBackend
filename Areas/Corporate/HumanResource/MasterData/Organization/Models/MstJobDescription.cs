using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models
{
    [Table("MstJobDescription", Schema = "public")]
    public class MstJobDescription : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PositionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string JobDescriptionCode { get; set; } = string.Empty;

        public int VersionNumber { get; set; } = 1;

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Draft"; // Draft, Active, Archived

        [Required]
        [MaxLength(2000)]
        public string Purpose { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public MstPosition? Position { get; set; }

        public ICollection<MstJobDescriptionItem> Items { get; set; }
            = new List<MstJobDescriptionItem>();
    }
}
