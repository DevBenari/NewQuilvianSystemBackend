using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    [Table("TrxKioskScanSession", Schema = "public")]
    public class TrxKioskScanSession : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string SessionCode { get; set; } = string.Empty;

        public KioskScanSource ScanSource { get; set; } = KioskScanSource.Unknown;

        public KioskScanSessionStatus ScanStatus { get; set; } = KioskScanSessionStatus.Started;

        public Guid? KioskDeviceId { get; set; }

        public Guid? IdentityScannerProfileId { get; set; }

        public Guid? PatientId { get; set; }

        [MaxLength(100)]
        public string? IdentityType { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(100)]
        public string? CardNumber { get; set; }

        [MaxLength(100)]
        public string? MemberNumber { get; set; }

        [MaxLength(100)]
        public string? InsuranceCardNumber { get; set; }

        [MaxLength(200)]
        public string? FullName { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(50)]
        public string? GenderText { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(2000)]
        public string? RawScanText { get; set; }

        [MaxLength(2000)]
        public string? ParsedJson { get; set; }

        [MaxLength(250)]
        public string? FailureReason { get; set; }

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public bool IsPatientFound { get; set; } = false;

        public bool IsManualInput { get; set; } = false;

        public bool IsUsedForRegistration { get; set; } = false;

        public MstKioskDevice? KioskDevice { get; set; }

        public MstIdentityScannerProfile? IdentityScannerProfile { get; set; }

        public MstPatient? Patient { get; set; }
    }
}
