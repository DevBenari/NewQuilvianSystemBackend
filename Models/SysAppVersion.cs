using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Models
{
    [Table("SysAppVersion", Schema = "public")]
    public class SysAppVersion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string AppName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string BackendVersion { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ApiVersion { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? FrontendMinimumVersion { get; set; }

        [MaxLength(50)]
        public string? FrontendRecommendedVersion { get; set; }

        [MaxLength(200)]
        public string? ReleaseName { get; set; }

        public string? Description { get; set; }

        public bool IsLatest { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime ReleaseDateTime { get; set; } = DateTime.Now;
    }
}
