using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class NosocomialInfectionResponse
    {
        public Guid Id { get; set; }
        public string NosocomialRecordNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        /// <summary>
        /// Nama pasien untuk ditampilkan. Layar tidak boleh menampilkan identifier sebagai
        /// label, sehingga nama ikut dikirim di sini alih-alih dicari ulang oleh frontend.
        /// </summary>
        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public Guid? EncounterId { get; set; }
        public Guid? EmergencyVisitId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public Guid? DoctorId { get; set; }

        public NosocomialInfectionType InfectionType { get; set; }
        public string? InfectionTypeOther { get; set; }
        public NosocomialInfectionStatus Status { get; set; }
        public NosocomialInfectionOnsetCategory OnsetCategory { get; set; }

        public DateTime OnsetDateTime { get; set; }
        public DateTime? AdmissionDateTimeSnapshot { get; set; }
        public int? HoursSinceAdmission { get; set; }

        public bool IsDeviceAssociated { get; set; }
        public string? DeviceName { get; set; }
        public DateTime? DeviceInsertedAt { get; set; }
        public int? DeviceUsageDays { get; set; }

        public string? CriteriaMet { get; set; }
        public string? CultureSpecimenType { get; set; }
        public DateTime? CultureTakenAt { get; set; }
        public string? CultureResult { get; set; }
        public string? CausativeOrganism { get; set; }
        public string? AntibioticTherapy { get; set; }

        public DateTime ReportedAt { get; set; }
        public Guid? ReportedByUserId { get; set; }
        public string? ReportedByNameSnapshot { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByNameSnapshot { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? RuledOutReason { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateNosocomialInfectionRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }
        public Guid? EmergencyVisitId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? DoctorId { get; set; }

        public NosocomialInfectionType InfectionType { get; set; }
            = NosocomialInfectionType.Unknown;

        [MaxLength(250)]
        public string? InfectionTypeOther { get; set; }

        public NosocomialInfectionOnsetCategory OnsetCategory { get; set; }
            = NosocomialInfectionOnsetCategory.Unknown;

        [Required]
        public DateTime OnsetDateTime { get; set; }

        public DateTime? AdmissionDateTimeSnapshot { get; set; }

        public bool IsDeviceAssociated { get; set; } = false;

        [MaxLength(150)]
        public string? DeviceName { get; set; }

        public DateTime? DeviceInsertedAt { get; set; }

        [Range(0, int.MaxValue)]
        public int? DeviceUsageDays { get; set; }

        [MaxLength(2000)]
        public string? CriteriaMet { get; set; }

        [MaxLength(250)]
        public string? CultureSpecimenType { get; set; }

        public DateTime? CultureTakenAt { get; set; }

        [MaxLength(500)]
        public string? CultureResult { get; set; }

        [MaxLength(250)]
        public string? CausativeOrganism { get; set; }

        [MaxLength(1000)]
        public string? AntibioticTherapy { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateNosocomialInfectionRequest : CreateNosocomialInfectionRequest
    {
    }

    /// <summary>
    /// Perubahan status kejadian. Dipisahkan dari update biasa karena tiap perpindahan
    /// status punya syaratnya sendiri, dan menyatukannya dengan pengubahan isi membuat
    /// syarat itu mudah terlewat.
    /// </summary>
    public class UpdateNosocomialInfectionStatusRequest
    {
        [Required]
        public NosocomialInfectionStatus Status { get; set; }

        /// <summary>Wajib diisi ketika status menjadi <c>RuledOut</c>.</summary>
        [MaxLength(1000)]
        public string? RuledOutReason { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class NosocomialInfectionOptionResponse
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class NosocomialInfectionFilterMetadataResponse
    {
        public List<NosocomialInfectionOptionResponse> InfectionTypes { get; set; } = new();
        public List<NosocomialInfectionOptionResponse> Statuses { get; set; } = new();
        public List<NosocomialInfectionOptionResponse> OnsetCategories { get; set; } = new();
    }
}
