using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Models
{
    [Table("SysAppVersionBuild", Schema = "public")]
    public class SysAppVersionBuild : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AppVersionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BuildVersion { get; set; } = string.Empty;

        public long BuildNumber { get; set; }

        [Required]
        [MaxLength(64)]
        public string CommitSha { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? CommitMessage { get; set; }

        [MaxLength(200)]
        public string? BranchName { get; set; }

        public DateTime BuildDateTime { get; set; } = DateTime.UtcNow;

        public SysAppVersion AppVersion { get; set; } = null!;
    }
}
