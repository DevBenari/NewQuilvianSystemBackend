using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    /// <summary>
    /// DTO Aturan Pelatihan Wajib Akreditasi Rumah Sakit STARKES KPS 8 (VR-TRN-01, VR-TRN-02)
    /// </summary>
    public class MandatoryTrainingRuleDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string TrainingCategory { get; set; } = "STARKES_MANDATORY";
        public string TargetAudience { get; set; } = "Seluruh Pegawai RS (100%)";
        public int RecurrenceMonths { get; set; } = 24;
        public int GracePeriodDays { get; set; } = 30;
        public bool IsRequiredBeforeIndependentPractice { get; set; } = true;
        public bool IsRequiredForCredentialing { get; set; } = true;
        public bool RequiresPassingResult { get; set; } = true;
        public decimal MinimumPassingScore { get; set; } = 75.00m;
        public decimal OscePassingScore { get; set; } = 80.00m;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO Request Pembuatan / Konfigurasi Aturan Pelatihan Wajib
    /// </summary>
    public class CreateMandatoryTrainingRuleRequestDto
    {
        [Required(ErrorMessage = "Kode aturan pelatihan wajib diisi.")]
        [MaxLength(50)]
        public string RuleCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama aturan pelatihan wajib diisi.")]
        [MaxLength(200)]
        public string RuleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori pelatihan wajib diisi.")]
        public string TrainingCategory { get; set; } = "STARKES_MANDATORY";

        /// <summary>Katalog pelatihan yang diwajibkan. Bila kosong, katalog dicari atau dibuat dari nama aturan.</summary>
        public Guid? TrainingCatalogId { get; set; }

        public string TargetAudience { get; set; } = "Seluruh Pegawai RS (100%)";

        /// <summary>
        /// Masa berlaku sertifikat (Maksimal 24 bulan, VR-TRN-02)
        /// </summary>
        [Range(1, 24, ErrorMessage = "Masa berlaku pelatihan wajib rumah sakit dibatasi maksimal 24 bulan sesuai regulasi akreditasi (VR-TRN-02).")]
        public int RecurrenceMonths { get; set; } = 24;

        [Range(0, 60, ErrorMessage = "Masa tenggang (grace period) maksimal 60 hari.")]
        public int GracePeriodDays { get; set; } = 30;

        public bool IsRequiredBeforeIndependentPractice { get; set; } = true;
        public bool IsRequiredForCredentialing { get; set; } = true;
        public bool RequiresPassingResult { get; set; } = true;

        [Range(50, 100, ErrorMessage = "Ambang kelulusan teori minimal 50.")]
        public decimal MinimumPassingScore { get; set; } = 75.00m;

        [Range(50, 100, ErrorMessage = "Ambang kelulusan OSCE klinis minimal 50.")]
        public decimal OscePassingScore { get; set; } = 80.00m;

        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO Item Dashboard Radar Kedaluwarsa Sertifikat Diklat (VR-TRN-09)
    /// </summary>
    public class CertificateExpiryRadarItemDto
    {
        public Guid CertificateId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string StaffCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Profession { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public string TrainingName { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public int DaysUntilExpiry { get; set; }

        /// <summary>
        /// RADAR_H90, RADAR_H60, RADAR_H30_CRITICAL, OVERDUE_H0
        /// </summary>
        public string RadarZone { get; set; } = "RADAR_H90";

        public string SeverityLevel => RadarZone switch
        {
            "OVERDUE_H0" => "FATAL",
            "RADAR_H30_CRITICAL" => "ERROR",
            "RADAR_H60" => "WARNING",
            _ => "INFO"
        };

        public bool IsBuddySafeguardActive { get; set; } = false;
        public bool IsRkkPrivilegeRestricted { get; set; } = false;
    }

    /// <summary>
    /// DTO Summary Dashboard Radar Kedaluwarsa
    /// </summary>
    public class CertificateExpiryRadarSummaryDto
    {
        public int TotalMonitoredCertificates { get; set; }
        public int ZoneH90Count { get; set; }
        public int ZoneH60Count { get; set; }
        public int ZoneH30CriticalCount { get; set; }
        public int OverdueH0Count { get; set; }
        public List<CertificateExpiryRadarItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO Request Broadcast Notifikasi Peringatan Massal
    /// </summary>
    public class BroadcastExpiryAlertRequestDto
    {
        /// <summary>
        /// Target zona yang akan dibroadcast: ALL, H30_AND_OVERDUE, OVERDUE_ONLY
        /// </summary>
        public string TargetZoneFilter { get; set; } = "H30_AND_OVERDUE";

        public List<string> DeliveryChannels { get; set; } = new() { "ESS", "EMAIL", "WHATSAPP" };

        public string? CustomMessagePrefix { get; set; }
    }

    public class BroadcastExpiryAlertResponseDto
    {
        public int TotalRecipients { get; set; }
        public int SuccessfulDispatches { get; set; }
        public List<string> DeliveryChannels { get; set; } = new();
        public DateTime DispatchedAt { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO Request Penugasan Pengamanan Shift Dinas Buddy System (VR-TRN-11, INT-HR-79, INT-HR-77)
    /// </summary>
    public class AssignShiftBuddySafeguardRequestDto
    {
        [Required(ErrorMessage = "Sertifikat yang kedaluwarsa wajib dipilih.")]
        public Guid ExpiredCertificateId { get; set; }

        public Guid? AssignedBuddyWorkforceId { get; set; }

        public string ReasonNotes { get; set; } = "Sertifikat pelatihan wajib kritis kedaluwarsa. Diberlakukan penugasan dinas buddy system dan penurunan kewenangan RKK Level 3 supervisi.";

        public bool RestrictRkkToLevel3 { get; set; } = true;
    }

    public class ShiftBuddySafeguardResponseDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public bool IsOverdueFlagged { get; set; } = true;
        public int GracePeriodDaysRemaining { get; set; } = 30;
        public bool IsBuddySystemEnforced { get; set; } = true;
        public bool IsRkkRestrictedLevel3 { get; set; } = true;
        public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO Portofolio Sertifikat Pegawai
    /// </summary>
    public class StaffCertificatePortfolioDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string StaffCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalCertificates { get; set; }
        public int ActiveCertificates { get; set; }
        public int ExpiredCertificates { get; set; }
        public bool HasOverdueMandatoryTraining { get; set; }
        public List<CertificateItemDetailDto> Certificates { get; set; } = new();
    }

    public class CertificateItemDetailDto
    {
        public Guid CertificateId { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public string TrainingProgramName { get; set; } = string.Empty;
        public string Category { get; set; } = "STARKES_MANDATORY";
        public DateTime IssuedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsExpired => ExpiredDate.HasValue && ExpiredDate.Value < DateTime.UtcNow;
        public string IssuerName { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = true;
        public string? CertificateUrl { get; set; }
    }
}
