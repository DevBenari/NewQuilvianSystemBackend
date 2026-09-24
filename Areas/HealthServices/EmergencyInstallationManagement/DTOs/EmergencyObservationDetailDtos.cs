using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyObservationDetailResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyObservationId { get; set; }
        public Guid? PatientVitalSignId { get; set; }

        /// <summary>
        /// Ringkasan tanda vital yang ditautkan (API 0.6.0 bagian 7.2). Bernilai
        /// <c>null</c> ketika <see cref="PatientVitalSignId"/> kosong, termasuk untuk
        /// baris pemantauan lama yang dicatat sebelum penautan ada.
        /// </summary>
        public EmergencyObservationDetailVitalSignResponse? VitalSign { get; set; }

        public Guid? ProgressNoteId { get; set; }
        public DateTime RecordedAt { get; set; }
        public Guid RecordedByUserId { get; set; }

        /// <summary>
        /// Nama petugas pencatat (API 0.6.0 bagian 7.2). Kosong bila penggunanya tidak
        /// ditemukan; layar tidak menampilkan GUID sebagai gantinya.
        /// </summary>
        public string? RecordedByName { get; set; }
        public string? ClinicalConditionSummary { get; set; }
        public string? InterventionSummary { get; set; }
        public string? PatientResponseSummary { get; set; }
        public decimal? FluidIntakeMl { get; set; }
        public decimal? UrineOutputMl { get; set; }
        public decimal? OtherOutputMl { get; set; }
        public decimal? BleedingEstimatedMl { get; set; }
        public decimal? VomitEstimatedMl { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>
    /// Ringkasan tanda vital yang ditautkan pada satu baris pemantauan observasi.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-046</c>, API 0.6.0 bagian 7.2. Seluruh nilainya diambil apa adanya dari
    /// <c>TrxPatientVitalSign</c> milik ClinicalManagement; IGD menautkan, tidak menyalin dan
    /// tidak menghitung nilai turunan baru (<c>IGD-DEC-122</c>). <c>GcsTotal</c> pun dikirim
    /// seperti tersimpan, bukan hasil penjumlahan ulang.
    /// </remarks>
    public class EmergencyObservationDetailVitalSignResponse
    {
        public Guid Id { get; set; }
        public DateTime ObservationDateTime { get; set; }
        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        public int? PulseRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public int? GcsEye { get; set; }
        public int? GcsVerbal { get; set; }
        public int? GcsMotor { get; set; }
        public int? GcsTotal { get; set; }
        public ConsciousnessStatus ConsciousnessStatus { get; set; }
        public bool IsUsingOxygen { get; set; }
        public OxygenSupportType OxygenSupportType { get; set; }
        public decimal? OxygenFlowRate { get; set; }
        public string? OxygenSupportNote { get; set; }
        public PatientVitalSignStatus VitalSignStatus { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsCritical { get; set; }
    }

    public class CreateEmergencyObservationDetailRequest
    {
        [Required]
        public Guid EmergencyObservationId { get; set; }

        public Guid? PatientVitalSignId { get; set; }

        public Guid? ProgressNoteId { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// USANG pada kontrak 0.6.0 dan diabaikan backend.
        /// </summary>
        /// <remarks>
        /// Field ini tetap diterima supaya pemanggil lama tidak ditolak, tetapi nilainya
        /// tidak pernah dipakai: pelaku pencatat selalu diambil dari pengguna yang
        /// terautentikasi (validation 0.6.0 bagian 9 aturan 9, <c>IGD-DEC-057</c>).
        /// </remarks>
        public Guid RecordedByUserId { get; set; }

        [MaxLength(2000)]
        public string? ClinicalConditionSummary { get; set; }

        [MaxLength(2000)]
        public string? InterventionSummary { get; set; }

        [MaxLength(2000)]
        public string? PatientResponseSummary { get; set; }

        public decimal? FluidIntakeMl { get; set; }

        public decimal? UrineOutputMl { get; set; }

        public decimal? OtherOutputMl { get; set; }

        public decimal? BleedingEstimatedMl { get; set; }

        public decimal? VomitEstimatedMl { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyObservationDetailRequest : CreateEmergencyObservationDetailRequest
    {
    }
}
