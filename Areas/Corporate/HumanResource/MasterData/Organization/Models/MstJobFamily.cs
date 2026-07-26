using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models
{
    [Table("MstJobFamily", Schema = "public")]
    public class MstJobFamily : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string JobFamilyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string JobFamilyName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
