using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs
{
    public class SubmitDocumentRequestDto
    {
        [Required]
        public Guid EmployeeDocumentTypeId { get; set; }

        public Guid? RequestedForWorkforceProfileId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Purpose { get; set; } = string.Empty;

        [MaxLength(20)]
        public string LanguageCode { get; set; } = "id-ID";

        public int NumberOfCopies { get; set; } = 1;

        public DateTime? NeededByDate { get; set; }

        public string? DeliveryMethod { get; set; } = "Digital";

        public string? AdditionalNotes { get; set; }

        // PERHATIAN: Dilarang keras mengirim nominal gaji secara manual (VR-HRS-06 Anti-Manual Guard).
        // Sistem backend akan menolak atau mengabaikan jika ada payload angka nominal gaji.
    }

    public class CbSupervisorApprovalDto
    {
        [Required]
        public bool IsApproved { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class FinalAuthorizeAndIssueDto
    {
        [Required]
        [MaxLength(150)]
        public string SignerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string SignerTitle { get; set; } = "Manajer Sumber Daya Manusia";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// PIN 4-digit khusus dokumen finansial untuk akses verifikasi pihak ketiga (KPR/Bank).
        /// Jika dikosongkan, sistem akan meng-generate PIN acak 4-digit secara otomatis.
        /// </summary>
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN keamanan finansial wajib 4 digit angka.")]
        public string? CustomSecurityPin { get; set; }
    }

    public class EmployeeDocumentIssuanceDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeDocumentRequestId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public Guid DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string IssuanceNumber { get; set; } = string.Empty;
        public string OfficialDocumentNumber { get; set; } = string.Empty;
        public int VersionNumber { get; set; } = 1;
        public string IssuanceStatus { get; set; } = "Draft"; // Draft, PendingCbApproval, CbApproved, PendingAuthorization, Issued, Rejected, Revoked
        public DateTime IssuedAt { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsDigitallySigned { get; set; }
        public DateTime? DigitallySignedAt { get; set; }
        public string? SignerName { get; set; }
        public string? SignerTitle { get; set; }
        public bool IsFinancialSensitive { get; set; }
        public decimal? SalaryBrutoAmount { get; set; }
        public decimal? SalaryNettoAmount { get; set; }
        public Guid? PayrollPayslipId { get; set; }
        public string? VerificationHash { get; set; }
        public string? VerificationUrl { get; set; }
        public string? QrCodeImagePath { get; set; }
        public string? PdfFilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string? FileChecksum { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevocationReason { get; set; }

        /// <summary>
        /// Ditampilkan HANYA satu kali kepada pemohon / pejabat penerbit saat dokumen berhasil diotorisasi
        /// </summary>
        public string? GeneratedRawSecurityPin { get; set; }
    }

    public class PublicDocumentVerificationDto
    {
        public bool IsValid { get; set; }
        public string VerificationStatusMessage { get; set; } = string.Empty;
        public string IssuanceNumber { get; set; } = string.Empty;
        public string OfficialDocumentNumber { get; set; } = string.Empty;
        public string DocumentTypeName { get; set; } = string.Empty;
        public string EmployeeNameMasked { get; set; } = string.Empty;
        public string EmployeeNumberMasked { get; set; } = string.Empty;
        public string IssuedByHospital { get; set; } = "Rumah Sakit Quilvian Utama";
        public DateTime IssuedAt { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string SignerName { get; set; } = string.Empty;
        public string SignerTitle { get; set; } = string.Empty;
        public bool IsFinancialSensitive { get; set; }
        public bool IsFinancialDataMasked { get; set; } = true;
        public string? MaskedSalaryBruto { get; set; }
        public string? MaskedSalaryNetto { get; set; }
        public bool RequiresPinToUnlock { get; set; }
    }

    public class UnlockFinancialDetailsRequestDto
    {
        [Required]
        public string VerificationHash { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN wajib 4 digit numerik.")]
        public string SecurityPin { get; set; } = string.Empty;
    }

    public class FinancialDetailsUnlockedDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string OfficialDocumentNumber { get; set; } = string.Empty;
        public string EmployeeFullName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public decimal SalaryBrutoAmount { get; set; }
        public decimal SalaryNettoAmount { get; set; }
        public string FormattedSalaryBruto { get; set; } = string.Empty;
        public string FormattedSalaryNetto { get; set; } = string.Empty;
        public string Currency { get; set; } = "IDR";
        public string PayslipPeriodSnapshot { get; set; } = string.Empty;
        public DateTime VerifiedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class RevokeDocumentRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string RevocationReason { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? RevocationBapNumber { get; set; }
    }
}
