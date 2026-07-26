using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    [Table("MstPayrollComponentCategory", Schema = "public")]
    public class MstPayrollComponentCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ComponentCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ComponentCategoryName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ComponentGroup { get; set; } = "Earning";
        // Earning, Deduction, EmployerContribution, Information.

        public bool AffectsGrossPay { get; set; } = true;

        public bool AffectsTaxableIncome { get; set; } = true;

        public bool AffectsTakeHomePay { get; set; } = true;

        public bool IsEmployerCost { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<MstPayrollComponent> PayrollComponents { get; set; }
            = new List<MstPayrollComponent>();
    }
}
