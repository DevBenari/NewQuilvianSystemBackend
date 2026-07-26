using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpEmergencyContact", Schema = "public")]
    public class WfpEmergencyContact : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Relationship { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public int PriorityOrder { get; set; } = 1;
        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
