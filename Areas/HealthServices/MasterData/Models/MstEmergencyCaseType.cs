using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models
{
    [Table("MstEmergencyCaseType", Schema = "public")]
    public class MstEmergencyCaseType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int Sequence { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<TrxEmergencyVisit> EmergencyVisits { get; set; }
            = new List<TrxEmergencyVisit>();
    }
}
