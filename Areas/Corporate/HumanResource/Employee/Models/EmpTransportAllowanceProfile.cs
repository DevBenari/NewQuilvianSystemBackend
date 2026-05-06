using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Employee.Models
{
    [Table("EmpTransportAllowanceProfile", Schema = "public")]
    public class EmpTransportAllowanceProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        public bool IsEligible { get; set; } = false;

        public bool IsNightTransportEligible { get; set; } = false;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "None";

        public decimal MonthlyAmount { get; set; } = 0;

        public decimal DailyAmount { get; set; } = 0;

        public decimal NightAmount { get; set; } = 0;

        public bool IsProrated { get; set; } = true;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstEmployee? Employee { get; set; }
    }
}
