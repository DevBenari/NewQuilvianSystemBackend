using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveRequestAttachment", Schema = "public")]
    public class TrxLeaveRequestAttachment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LeaveRequestId { get; set; }

        [Required, MaxLength(50)]
        public string AttachmentType { get; set; } = "SupportingDocument";

        [Required, MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? ContentType { get; set; }

        public long FileSizeBytes { get; set; } = 0;

        [MaxLength(128)]
        public string? FileHash { get; set; }

        public bool IsRequiredDocument { get; set; } = false;

        [Required, MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";
        // Pending, Verified, Rejected, ReuploadRequired

        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpLeaveRequest? LeaveRequest { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
