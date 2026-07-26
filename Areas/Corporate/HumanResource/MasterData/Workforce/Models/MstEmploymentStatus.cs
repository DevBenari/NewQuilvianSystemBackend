using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models
{
    [Table("MstEmploymentStatus", Schema = "public")]
    public class MstEmploymentStatus : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string EmploymentStatusCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string EmploymentStatusName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActiveEmployment { get; set; } = true;

        public bool IsSchedulable { get; set; } = true;

        public bool IsPayrollEligible { get; set; } = true;

        public bool IsTerminalStatus { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
