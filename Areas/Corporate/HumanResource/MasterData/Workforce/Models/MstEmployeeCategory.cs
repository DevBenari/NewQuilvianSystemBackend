using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstEmployeeCategory", Schema = "public")]
    public class MstEmployeeCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? WorkforceTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EmployeeCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string EmployeeCategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsClinical { get; set; } = false;

        public bool RequiresCredentialing { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstWorkforceType? WorkforceType { get; set; }
    }
}
