using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("HrdPersonnelActionOrderAttachment", Schema = "public")]
    public class HrdPersonnelActionOrderAttachment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PersonnelActionOrderId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = "application/pdf";

        public long FileSize { get; set; }

        [Required]
        [MaxLength(500)]
        public string StoragePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Guid UploadedByUserId { get; set; }

        public HrdPersonnelActionOrder? PersonnelActionOrder { get; set; }
    }
}
