using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    [Table("TrxHrServiceRequestAttachment", Schema = "public")]
    public class TrxHrServiceRequestAttachment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid HrServiceRequestId { get; set; }
        public Guid? HrServiceRequestCommentId { get; set; }
        public Guid? UploadedByUserId { get; set; }
        public Guid? UploadedByWorkforceProfileId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        [MaxLength(128)]
        public string? FileChecksum { get; set; }

        [MaxLength(100)]
        public string? DocumentCategory { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsEmployeeVisible { get; set; } = true;
        public bool IsConfidential { get; set; } = false;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public TrxHrServiceRequest? HrServiceRequest { get; set; }
        public TrxHrServiceRequestComment? HrServiceRequestComment { get; set; }
        public ApplicationUser? UploadedByUser { get; set; }
        public MstWorkforceProfile? UploadedByWorkforceProfile { get; set; }
    }
}
