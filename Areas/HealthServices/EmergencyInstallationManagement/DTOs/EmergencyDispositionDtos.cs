using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyDispositionResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public Guid DispositionTypeId { get; set; }

        /// <summary>
        /// Nama dan sifat jenis tindak lanjut, disalin dari master saat balasan dibentuk.
        ///
        /// Layar menampilkan nama kepada petugas, bukan identifier. Dua penanda kewajiban
        /// ikut dikirim supaya formulir dapat menampilkan isian unit tujuan atau fasilitas
        /// rujukan hanya ketika jenisnya memang mensyaratkan — tanpa frontend menyalin
        /// aturan yang sudah dipegang backend.
        /// </summary>
        public string? DispositionTypeCode { get; set; }
        public string? DispositionTypeName { get; set; }
        public bool RequiresDestinationServiceUnit { get; set; }
        public bool RequiresReferralFacility { get; set; }
        public bool ClosesEmergencyVisit { get; set; }

        public EmergencyDispositionStatus DispositionStatus { get; set; }
        public DateTime DecidedAt { get; set; }
        public Guid? DecidedByDoctorId { get; set; }
        public string? DecidedByDoctorName { get; set; }
        public Guid? ConfirmedByUserId { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public Guid? DestinationServiceUnitId { get; set; }
        public string? DestinationServiceUnitName { get; set; }
        public string? DestinationFacilityName { get; set; }
        public string? ReferralNumber { get; set; }
        public string? DispositionReason { get; set; }
        public string? PatientConditionAtDisposition { get; set; }
        public string? FollowUpInstruction { get; set; }
        public string? RefusalReason { get; set; }
        public bool IsPatientDeceased { get; set; }
        public DateTime? DeathDateTime { get; set; }
        public string? DeathLocation { get; set; }
        public string? SuspectedCauseOfDeath { get; set; }
        public bool IsVisumRequested { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyDispositionRequest
    {
        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid DispositionTypeId { get; set; }

        public EmergencyDispositionStatus DispositionStatus { get; set; } = EmergencyDispositionStatus.Draft;

        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

        public Guid? DecidedByDoctorId { get; set; }

        public Guid? ConfirmedByUserId { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public DateTime? ExecutedAt { get; set; }

        public Guid? DestinationServiceUnitId { get; set; }

        [MaxLength(250)]
        public string? DestinationFacilityName { get; set; }

        [MaxLength(100)]
        public string? ReferralNumber { get; set; }

        [MaxLength(2000)]
        public string? DispositionReason { get; set; }

        [MaxLength(2000)]
        public string? PatientConditionAtDisposition { get; set; }

        [MaxLength(2000)]
        public string? FollowUpInstruction { get; set; }

        [MaxLength(1000)]
        public string? RefusalReason { get; set; }

        public bool IsPatientDeceased { get; set; }

        public DateTime? DeathDateTime { get; set; }

        [MaxLength(250)]
        public string? DeathLocation { get; set; }

        [MaxLength(1000)]
        public string? SuspectedCauseOfDeath { get; set; }

        public bool IsVisumRequested { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyDispositionRequest : CreateEmergencyDispositionRequest
    {
    }

    public class UpdateEmergencyDispositionDispositionStatusRequest
    {
        [Required]
        public EmergencyDispositionStatus DispositionStatus { get; set; }

        /// <summary>
        /// Catatan perubahan status. <b>Wajib diisi ketika status diubah menjadi Cancelled</b>:
        /// keputusan tindak lanjut yang dicabut harus dapat ditelusuri alasannya, karena
        /// keputusan itu menentukan ke mana pasien pergi setelah meninggalkan IGD.
        ///
        /// Alasan pembatalan sengaja tidak diberi kolom tersendiri. Kolom baru berarti migrasi
        /// pada basis data yang dipakai bersama, sedangkan Cancelled adalah status terminal —
        /// tidak ada transisi keluar darinya, sehingga catatan yang ditulis saat pembatalan
        /// tidak akan tertimpa perubahan status berikutnya.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
