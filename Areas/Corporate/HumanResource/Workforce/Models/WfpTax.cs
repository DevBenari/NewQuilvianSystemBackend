using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpTax", Schema = "public")]
    public class WfpTax : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(30)]
        public string? NpwpNumber { get; set; }

        [MaxLength(50)]
        public string TaxStatus { get; set; } = "TK0";
        // TK0, TK1, K0, K1, K2, K3

        public bool IsTaxed { get; set; } = true;

        [MaxLength(50)]
        public string TaxCalculationMethod { get; set; } = "Gross";
        // Gross, GrossUp, Net

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}