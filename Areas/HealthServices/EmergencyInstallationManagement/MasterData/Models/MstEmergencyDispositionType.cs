using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models
{
    [Table("MstEmergencyDispositionType", Schema = "public")]
    public class MstEmergencyDispositionType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public bool RequiresDestinationServiceUnit { get; set; }

        public bool RequiresReferralFacility { get; set; }

        public bool ClosesEmergencyVisit { get; set; } = true;

        public int Sequence { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<TrxEmergencyDisposition> EmergencyDispositions { get; set; }
            = new List<TrxEmergencyDisposition>();
    }
}
