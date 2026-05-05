using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.Models
{
    [Table("EmpPayrollProfile", Schema = "public")]
    public class EmpPayrollProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        [MaxLength(50)]
        public string? PayrollNumber { get; set; }

        [MaxLength(50)]
        public string? SalaryType { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal BasicSalary { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal FixedAllowance { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal MealAllowance { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal TransportAllowance { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal PositionAllowance { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal OtherAllowance { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal FixedDeduction { get; set; } = 0;

        public bool IsOvertimeEligible { get; set; } = true;

        public bool IsPayrollActive { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public MstEmployee? Employee { get; set; }
    }
}
