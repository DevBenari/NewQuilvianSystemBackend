using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Models
{
    [Table("SysControllerAccess", Schema = "public")]
    public class SysControllerAccess : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ModuleId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ControllerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? RoutePath { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public SysApplicationModule? Module { get; set; }

        public ICollection<SysActionAccess> Actions { get; set; } = new List<SysActionAccess>();
    }
}
