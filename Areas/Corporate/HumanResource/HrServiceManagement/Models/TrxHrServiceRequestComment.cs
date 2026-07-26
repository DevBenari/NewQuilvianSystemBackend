using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    [Table("TrxHrServiceRequestComment", Schema = "public")]
    public class TrxHrServiceRequestComment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid HrServiceRequestId { get; set; }
        public Guid? CommentByUserId { get; set; }
        public Guid? CommentByWorkforceProfileId { get; set; }
        public Guid? ParentCommentId { get; set; }

        [Required]
        [MaxLength(30)]
        public string CommentType { get; set; } = "Message";

        [Required]
        [MaxLength(5000)]
        public string CommentText { get; set; } = string.Empty;

        public bool IsInternalNote { get; set; } = false;
        public bool IsEmployeeVisible { get; set; } = true;
        public bool IsSystemGenerated { get; set; } = false;
        public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public TrxHrServiceRequest? HrServiceRequest { get; set; }
        public ApplicationUser? CommentByUser { get; set; }
        public MstWorkforceProfile? CommentByWorkforceProfile { get; set; }
        public ICollection<TrxHrServiceRequestAttachment> Attachments { get; set; } = new List<TrxHrServiceRequestAttachment>();
    }
}
