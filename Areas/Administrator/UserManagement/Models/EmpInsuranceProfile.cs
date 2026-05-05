using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.Models
{
    [Table("EmpInsuranceProfile", Schema = "public")]    
    public class EmpInsuranceProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        [MaxLength(50)]
        public string? BpjsHealthNumber { get; set; }

        public DateTime? BpjsHealthRegisteredDate { get; set; }

        public bool IsBpjsHealthActive { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsEmploymentNumber { get; set; }

        public DateTime? BpjsEmploymentRegisteredDate { get; set; }

        public bool IsBpjsEmploymentActive { get; set; } = false;

        [MaxLength(100)]
        public string? PrivateInsuranceName { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public DateTime? PrivateInsuranceStartDate { get; set; }

        public DateTime? PrivateInsuranceEndDate { get; set; }

        public MstEmployee? Employee { get; set; }
    }
}
