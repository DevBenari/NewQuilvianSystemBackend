using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyVisitResponse
    {
        public Guid Id { get; set; }
        public string EmergencyVisitNumber { get; set; } = string.Empty;
        public Guid? EncounterId { get; set; }
        public Guid? PatientId { get; set; }

        /// <summary>
        /// Nama pasien untuk ditampilkan. Untuk pasien yang belum teridentifikasi, diisi
        /// alias sementara kunjungan supaya layar tidak pernah menampilkan kolom kosong.
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        public string? MedicalRecordNumber { get; set; }

        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public Guid? ArrivalModeId { get; set; }
        public string? ArrivalModeName { get; set; }
        public Guid? CaseTypeId { get; set; }
        public string? CaseTypeName { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? ArrivalLocation { get; set; }
        public string? FoundLocation { get; set; }
        public string? TraumaLocation { get; set; }
        public DateTime? TraumaDateTime { get; set; }
        public bool IsUnknownPatient { get; set; }
        public string? TemporaryPatientAlias { get; set; }
        public bool IsImmediateCareAllowed { get; set; }
        public EmergencyRegistrationStatus RegistrationStatus { get; set; }
        public EmergencyVisitStatus VisitStatus { get; set; }
        public DateTime? RegistrationCompletedAt { get; set; }
        public Guid? RegistrationCompletedByUserId { get; set; }
        public DateTime? TreatmentStartedAt { get; set; }
        public DateTime? VisitCompletedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Terisi hanya bila kunjungan ini dibuat dengan menembus penjagaan satu pasien satu
        /// episode IGD aktif. Layar wajib menandainya, bukan menyembunyikannya.
        /// </summary>
        public string? DuplicateEpisodeOverrideReason { get; set; }

        public Guid? DuplicateEpisodeOverrideByUserId { get; set; }
        public DateTime? DuplicateEpisodeOverrideAt { get; set; }
        public Guid? DuplicateEpisodeOverrideOfVisitId { get; set; }

        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyVisitRequest
    {
        [MaxLength(50)]
        public string? EmergencyVisitNumber { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ArrivalModeId { get; set; }

        public Guid? CaseTypeId { get; set; }

        public DateTime ArrivalDateTime { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(250)]
        public string? ArrivalLocation { get; set; }

        [MaxLength(250)]
        public string? FoundLocation { get; set; }

        [MaxLength(250)]
        public string? TraumaLocation { get; set; }

        public DateTime? TraumaDateTime { get; set; }

        public bool IsUnknownPatient { get; set; }

        [MaxLength(100)]
        public string? TemporaryPatientAlias { get; set; }

        public bool IsImmediateCareAllowed { get; set; }

        public EmergencyRegistrationStatus RegistrationStatus { get; set; } = EmergencyRegistrationStatus.Pending;

        public EmergencyVisitStatus VisitStatus { get; set; } = EmergencyVisitStatus.Arrived;

        public DateTime? RegistrationCompletedAt { get; set; }

        public Guid? RegistrationCompletedByUserId { get; set; }

        public DateTime? TreatmentStartedAt { get; set; }

        public DateTime? VisitCompletedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Alasan menembus penjagaan satu pasien satu episode IGD aktif — <c>IGD-DEC-084</c>.
        /// Diisi <b>hanya</b> ketika petugas sengaja mendaftarkan kunjungan kedua untuk pasien
        /// yang episode IGD sebelumnya belum ditutup, misalnya karena pasien benar-benar datang
        /// lagi dengan keluhan baru.
        /// </summary>
        [MaxLength(1000)]
        public string? DuplicateEpisodeOverrideReason { get; set; }
    }

    public class UpdateEmergencyVisitRequest : CreateEmergencyVisitRequest
    {
    }

    public class UpdateEmergencyVisitRegistrationStatusRequest
    {
        [Required]
        public EmergencyRegistrationStatus RegistrationStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class UpdateEmergencyVisitVisitStatusRequest
    {
        [Required]
        public EmergencyVisitStatus VisitStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Permintaan menyelesaikan kunjungan secara klinis. Waktu selesai tidak diterima dari
    /// pemanggil melainkan diisi waktu server, supaya penutupan tidak dapat dimundurkan.
    /// </summary>
    public class CompleteVisitRequest
    {
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
