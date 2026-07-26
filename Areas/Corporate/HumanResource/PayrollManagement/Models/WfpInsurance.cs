using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("WfpInsurance", Schema = "public")]
    public class WfpInsurance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public bool IsBpjsKesehatanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; } = false;

        [MaxLength(200)]
        public string? PrivateInsuranceProvider { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public decimal BpjsHealthEmployeeRate { get; set; } = 0m;
        public decimal BpjsHealthEmployerRate { get; set; } = 0m;
        public decimal BpjsEmploymentEmployeeRate { get; set; } = 0m;
        public decimal BpjsEmploymentEmployerRate { get; set; } = 0m;
        public decimal PrivateInsuranceEmployeeContribution { get; set; } = 0m;
        public decimal PrivateInsuranceEmployerContribution { get; set; } = 0m;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
