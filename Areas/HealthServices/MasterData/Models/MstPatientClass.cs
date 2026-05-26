using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstPatientClass", Schema = "public")]
    public class MstPatientClass : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PatientClassCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PatientClassName { get; set; } = string.Empty;

        public PatientClassType PatientClassType { get; set; } = PatientClassType.Unknown;

        [MaxLength(50)]
        public string? ExternalClassCode { get; set; }

        [MaxLength(100)]
        public string? ClassAlias { get; set; }

        public int ClassLevel { get; set; } = 0;

        public bool IsForOutpatient { get; set; } = false;

        public bool IsForInpatient { get; set; } = false;

        public bool IsForEmergency { get; set; } = false;

        public bool IsForIntensiveCare { get; set; } = false;

        public bool IsForNewborn { get; set; } = false;

        public bool IsForRoomCharge { get; set; } = false;

        public bool IsForTariffMapping { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        public decimal? DefaultDailyRoomRate { get; set; }

        public decimal? DefaultRegistrationFee { get; set; }

        public decimal? DefaultConsultationFee { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
