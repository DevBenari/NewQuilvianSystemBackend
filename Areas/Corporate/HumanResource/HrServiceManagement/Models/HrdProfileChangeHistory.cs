using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    [Table("HrdProfileChangeHistory", Schema = "public")]
    public class HrdProfileChangeHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid HrServiceRequestId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ComponentModified { get; set; } = string.Empty;

        [Required]
        public string OldValuesJson { get; set; } = "{}";

        [Required]
        public string NewValuesJson { get; set; } = "{}";

        [Required]
        public Guid ApprovedByUserId { get; set; }

        public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public TrxHrServiceRequest? HrServiceRequest { get; set; }
        public MstWorkforceProfile? Employee { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
