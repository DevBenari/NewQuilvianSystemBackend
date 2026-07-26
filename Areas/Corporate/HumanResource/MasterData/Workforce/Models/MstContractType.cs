using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstContractType", Schema = "public")]
    public class MstContractType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ContractTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ContractTypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? DefaultDurationMonths { get; set; }

        public bool IsRenewable { get; set; } = true;

        public bool RequiresEndDate { get; set; } = true;

        public bool IsProbationApplicable { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
