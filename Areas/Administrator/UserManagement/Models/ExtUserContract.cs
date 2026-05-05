using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.Models
{
    [Table("ExtUserContract", Schema = "public")]
    public class ExtUserContract : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ExternalUserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ContractName { get; set; }

        [MaxLength(100)]
        public string? ContractType { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal ContractValue { get; set; } = 0;

        [MaxLength(50)]
        public string? PaymentTerm { get; set; }

        [MaxLength(500)]
        public string? ScopeOfWork { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        public bool IsActive { get; set; } = true;

        public MstExternalUser? ExternalUser { get; set; }
    }
}
