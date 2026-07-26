using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstEmploymentType", Schema = "public")]
    public class MstEmploymentType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string EmploymentTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string EmploymentTypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsPermanent { get; set; } = false;

        public bool IsContractBased { get; set; } = false;

        public bool RequiresContractEndDate { get; set; } = false;

        public bool IsPayrollEligible { get; set; } = true;

        public bool IsBenefitEligible { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
