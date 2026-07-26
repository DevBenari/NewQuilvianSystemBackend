using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("MstCandidateStatus", Schema = "public")]
    public class MstCandidateStatus : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string StatusCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string StatusName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string StatusCategory { get; set; } = "Active";
        // Active, Pending, Rejected, Withdrawn, Hired, Closed, Blacklisted.

        public int SortOrder { get; set; } = 0;
        public bool IsInitialStatus { get; set; } = false;
        public bool IsFinalStatus { get; set; } = false;
        public bool IsRejectedStatus { get; set; } = false;
        public bool IsWithdrawnStatus { get; set; } = false;
        public bool IsHiredStatus { get; set; } = false;
        public bool AllowNewApplication { get; set; } = true;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
