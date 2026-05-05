using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.Models
{
    [Table("EmpTaxProfile", Schema = "public")]
    public class EmpTaxProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [MaxLength(50)]
        public string? PtkpStatus { get; set; } //Contoh Penerapan : TK/0, TK/1, K/0, K/1, K/2 ...

        public bool IsPph21Active { get; set; } = true;

        public bool IsTaxPaidByCompany { get; set; } = false;

        [MaxLength(200)]
        public string? TaxRegisteredName { get; set; }

        [MaxLength(500)]
        public string? TaxRegisteredAddress { get; set; }

        public DateTime? TaxRegisteredDate { get; set; }

        public MstEmployee? Employee { get; set; }
    }
}
