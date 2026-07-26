using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstTerminationReason", Schema = "public")]
    public class MstTerminationReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TerminationReasonCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TerminationReasonName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TerminationType { get; set; } = "Other";
        // Voluntary, Involuntary, Retirement, ContractEnd, Deceased, Other.

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsVoluntary { get; set; } = false;

        public bool RequiresExitClearance { get; set; } = true;

        public bool DefaultRehireEligible { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
