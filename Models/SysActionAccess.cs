using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Models
{
    [Table("SysActionAccess", Schema = "public")]
    public class SysActionAccess : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ControllerAccessId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActionName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? HttpMethod { get; set; }

        [MaxLength(250)]
        public string? RoutePath { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public SysControllerAccess? ControllerAccess { get; set; }
    }
}
