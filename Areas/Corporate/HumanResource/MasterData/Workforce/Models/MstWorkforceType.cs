using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstWorkforceType", Schema = "public")]
    public class MstWorkforceType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string WorkforceTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string WorkforceTypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsInternal { get; set; } = true;

        public bool IsClinical { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<MstEmployeeCategory> EmployeeCategories { get; set; }
            = new List<MstEmployeeCategory>();
    }
}
