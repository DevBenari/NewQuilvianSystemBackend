using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs
{
    public class RegisterProfessionalLicenseDto : IValidatableObject
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LicenseType { get; set; } = "SIP"; // "STR" atau "SIP"

        [Required]
        [MaxLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Issuer { get; set; } = "Kementerian Kesehatan RI / Dinas Kesehatan";

        [MaxLength(250)]
        public string? PracticeLocation { get; set; } = "Rumah Sakit Quilvian Utama";

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        /// <summary>
        /// STR Seumur Hidup sesuai amanat UU Kesehatan No. 17 Tahun 2023
        /// </summary>
        public bool IsLifetimeStr { get; set; } = false;

        /// <summary>
        /// Surat Izin Praktik (SIP) ke-1, ke-2, atau ke-3 (maksimal 3 faskes sesuai regulasi)
        /// </summary>
        [RegularExpression(@"^(SIP-1|SIP-2|SIP-3|1|2|3)$", ErrorMessage = "Nomor faskes praktik SIP wajib SIP-1, SIP-2, atau SIP-3.")]
        public string? SipFaskesPracticeNumber { get; set; } = "SIP-1";

        [MaxLength(200)]
        public string? SipFaskesName { get; set; } = "Rumah Sakit Quilvian Utama";

        [MaxLength(1000)]
        public string? Description { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var isStr = LicenseType.Equals("STR", StringComparison.OrdinalIgnoreCase);
            var isSip = LicenseType.Equals("SIP", StringComparison.OrdinalIgnoreCase);

            if (isStr)
            {
                // Jika STR bukan seumur hidup, tanggal kedaluwarsa wajib ada
                if (!IsLifetimeStr && (!ExpiredDate.HasValue || ExpiredDate.Value <= IssueDate))
                {
                    yield return new ValidationResult(
                        "Tanggal kedaluwarsa STR wajib diisi dan harus lebih besar dari tanggal terbit jika bukan STR Seumur Hidup.",
                        new[] { nameof(ExpiredDate) });
                }
            }
            else if (isSip)
            {
                // SIP wajib memiliki tanggal kedaluwarsa 5 tahun dan tidak boleh seumur hidup
                if (IsLifetimeStr)
                {
                    yield return new ValidationResult(
                        "Surat Izin Praktik (SIP) dilarang berstatus seumur hidup. Sesuai regulasi UU Praktik Kedokteran, SIP wajib berjangka waktu 5 tahun.",
                        new[] { nameof(IsLifetimeStr) });
                }

                if (!ExpiredDate.HasValue || ExpiredDate.Value <= IssueDate)
                {
                    yield return new ValidationResult(
                        "Tanggal kedaluwarsa SIP wajib diisi (berlaku 5 tahun).",
                        new[] { nameof(ExpiredDate) });
                }
            }
        }
    }

    public class PsvVerificationRequestDto
    {
        [Required]
        public Guid LicenseId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PsvVerificationSource { get; set; } = "Kemenkes SatuSehat SDMK"; // SatuSehat SDMK, KKI, KTKI

        [Required]
        [MaxLength(500)]
        public string PsvVerificationUrl { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? AuditorNotes { get; set; }

        public bool IsValidOnSource { get; set; } = true;
    }

    public class PsvVerificationResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid LicenseId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public bool IsPsvVerified { get; set; }
        public DateTime? PsvVerifiedAt { get; set; }
        public string? PsvVerificationSource { get; set; }
        public string? PsvVerificationUrl { get; set; }
        public string AuditorUserName { get; set; } = string.Empty;
    }

    public class ProfessionalLicenseDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string? PracticeLocation { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public bool IsLifetimeStr { get; set; }
        public bool IsPsvVerified { get; set; }
        public DateTime? PsvVerifiedAt { get; set; }
        public string? PsvVerificationSource { get; set; }
        public string? PsvVerificationUrl { get; set; }
        public string? SipFaskesPracticeNumber { get; set; }
        public string? SipFaskesName { get; set; }
        public string ExpiryRadarStatus { get; set; } = "Normal"; // Normal, H90, H60, H30, H7, ExpiredH0
        public int DaysRemaining { get; set; }
        public bool BlocksClinicalServiceWhenInvalid { get; set; }
        public bool BlocksSchedulingWhenInvalid { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevocationReason { get; set; }
        public bool IsActive { get; set; }
    }

    public class RenewProfessionalLicenseDto
    {
        [Required]
        [MaxLength(100)]
        public string NewLicenseNumber { get; set; } = string.Empty;

        [Required]
        public DateTime NewIssueDate { get; set; }

        [Required]
        public DateTime NewExpiredDate { get; set; }

        [MaxLength(200)]
        public string? NewIssuer { get; set; }

        [MaxLength(1000)]
        public string? RenewalNotes { get; set; }
    }

    public class LicenseRadarAlertDto
    {
        public Guid LicenseId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Profession { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime ExpiredDate { get; set; }
        public int DaysRemaining { get; set; }
        public string RadarLevel { get; set; } = "Normal"; // H90, H60, H30, H7, ExpiredH0
        public string ActionRequired { get; set; } = string.Empty;
        public bool IsSimrsBlocked { get; set; }
        public bool IsSchedulingBlocked { get; set; }
    }
}
