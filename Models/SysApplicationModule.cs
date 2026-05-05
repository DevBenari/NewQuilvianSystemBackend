using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Models
{
    [Table("SysApplicationModule", Schema = "public")]
    public class SysApplicationModule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ModuleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ModuleName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? AreaName { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<SysControllerAccess> Controllers { get; set; } = new List<SysControllerAccess>();
    }
}
