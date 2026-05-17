using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Models
{
    [Table("AspNetUserFingerprint", Schema = "public")]
    public class ApplicationUserFingerprintCredential : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        public Guid? WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FingerPosition { get; set; } = "Unknown";
        // contoh: RightThumb, LeftThumb, RightIndex, LeftIndex

        [Required]
        [MaxLength(100)]
        public string TemplateFormat { get; set; } = string.Empty;
        // contoh: DigitalPersona.FMD.ANSI, DigitalPersona.FMD.ISO, DigitalPersona.SampleFormat5

        [MaxLength(50)]
        public string? TemplateVersion { get; set; }

        [Required]
        public byte[] TemplateDataEncrypted { get; set; } = Array.Empty<byte>();

        [MaxLength(128)]
        public string? TemplateHash { get; set; }

        [MaxLength(100)]
        public string? DeviceId { get; set; }

        [MaxLength(100)]
        public string? DeviceModel { get; set; }

        [MaxLength(50)]
        public string? SampleFormat { get; set; }

        public int? QualityScore { get; set; }

        public int EnrollmentSampleCount { get; set; } = 1;

        public bool IsPrimary { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public Guid RegisteredByUserId { get; set; }

        [MaxLength(100)]
        public string? RegisteredIpAddress { get; set; }

        [MaxLength(500)]
        public string? RegisteredUserAgent { get; set; }

        public DateTime? RevokedAt { get; set; }

        public Guid? RevokedByUserId { get; set; }

        [MaxLength(250)]
        public string? RevokedReason { get; set; }

        public ApplicationUser? User { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public MstEmployee? Employee { get; set; }

        public MstDoctor? Doctor { get; set; }
    }
}
