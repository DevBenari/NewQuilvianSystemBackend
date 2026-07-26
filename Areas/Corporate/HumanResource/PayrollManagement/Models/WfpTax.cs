using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("WfpTax", Schema = "public")]
    public class WfpTax : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(50)]
        public string? NpwpNumber { get; set; }

        [Required, MaxLength(30)]
        public string TaxStatus { get; set; } = "TK/0";

        [Required, MaxLength(30)]
        public string TaxMethod { get; set; } = "Gross";
        // Gross, GrossUp, Net.

        [Required, MaxLength(3)]
        public string TaxCountryCode { get; set; } = "ID";

        [MaxLength(50)]
        public string? TaxOfficeCode { get; set; }

        public bool IsNpwpRegistered { get; set; } = false;
        public bool IsTaxResident { get; set; } = true;
        public bool HasPreviousEmployer { get; set; } = false;
        public bool IsEmployerBorneTax { get; set; } = false;

        public decimal PreviousEmployerTaxableIncome { get; set; } = 0m;
        public decimal PreviousEmployerTaxPaid { get; set; } = 0m;
        public decimal AnnualNonTaxableIncome { get; set; } = 0m;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string? TaxIdentificationNumber
        {
            get => NpwpNumber;
            set => NpwpNumber = value;
        }

        [NotMapped]
        public string PtkpStatus
        {
            get => TaxStatus;
            set => TaxStatus = value;
        }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
