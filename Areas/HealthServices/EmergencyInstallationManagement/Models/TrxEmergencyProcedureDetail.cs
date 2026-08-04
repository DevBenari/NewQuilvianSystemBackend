using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("TrxEmergencyProcedureDetail", Schema = "public")]
    public class TrxEmergencyProcedureDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid PatientProcedureId { get; set; }

        public Guid? EmergencyResuscitationId { get; set; }

        public Guid? EmergencyObservationId { get; set; }

        public EmergencyProcedureDetailType DetailType { get; set; }
            = EmergencyProcedureDetailType.General;

        [MaxLength(250)]
        public string? SkinTestResult { get; set; }

        [MaxLength(250)]
        public string? TetanusToxoidResult { get; set; }

        public decimal? AntiTetanusSerumAmount { get; set; }

        [MaxLength(50)]
        public string? AntiTetanusSerumUnit { get; set; }

        [MaxLength(100)]
        public string? MedicationRoute { get; set; }

        public DateTime? MedicationDateTime { get; set; }

        [MaxLength(1000)]
        public string? EmergencySpecificResult { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxEmergencyVisit? EmergencyVisit { get; set; }

        public TrxPatientProcedure? PatientProcedure { get; set; }

        public TrxEmergencyResuscitation? EmergencyResuscitation { get; set; }

        public TrxEmergencyObservation? EmergencyObservation { get; set; }
    }
}
