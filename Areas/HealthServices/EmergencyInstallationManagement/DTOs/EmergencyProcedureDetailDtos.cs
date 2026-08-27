using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyProcedureDetailResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public Guid PatientProcedureId { get; set; }

        /// <summary>Identitas tindakan induk, diambil dari <c>TrxPatientProcedure</c>.</summary>
        /// <remarks>
        /// Baris ini hanya menyimpan <b>rincian khas IGD</b> — skin test, ATS, rute obat.
        /// Nama tindakan, waktu, jumlah, dan pelaksananya dimiliki tindakan induk. Tanpa
        /// keempat kolom ini, tab Tindakan pada layar pengkajian IGD menampilkan baris
        /// tanpa judul dan tiga kolom kosong, karena layar membacanya langsung dari respons
        /// daftar dan tidak membuka tindakan induk satu per satu.
        /// </remarks>
        public string? ProcedureName { get; set; }

        public DateTime? PerformedAt { get; set; }

        public decimal? Quantity { get; set; }

        public string? PerformedByName { get; set; }
        public Guid? EmergencyResuscitationId { get; set; }
        public Guid? EmergencyObservationId { get; set; }
        public EmergencyProcedureDetailType DetailType { get; set; }
        public string? SkinTestResult { get; set; }
        public string? TetanusToxoidResult { get; set; }
        public decimal? AntiTetanusSerumAmount { get; set; }
        public string? AntiTetanusSerumUnit { get; set; }
        public string? MedicationRoute { get; set; }
        public DateTime? MedicationDateTime { get; set; }
        public string? EmergencySpecificResult { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyProcedureDetailRequest
    {
        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid PatientProcedureId { get; set; }

        public Guid? EmergencyResuscitationId { get; set; }

        public Guid? EmergencyObservationId { get; set; }

        public EmergencyProcedureDetailType DetailType { get; set; } = EmergencyProcedureDetailType.General;

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

    }

    public class UpdateEmergencyProcedureDetailRequest : CreateEmergencyProcedureDetailRequest
    {
    }
}
