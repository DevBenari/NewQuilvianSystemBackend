using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    /// <summary>
    /// Perintah callback konfirmasi pencairan bank dari Modul Keuangan (INT-HR-54, BE-HRD-159, HRD-DEC-126).
    /// </summary>
    public class FinancePaymentConfirmationCommand
    {
        [Required]
        public Guid HandoffBatchId { get; set; }

        [Required]
        public Guid PayrollRunId { get; set; }

        [Required]
        public string Status { get; set; } = "PaymentConfirmed";

        [Required, MaxLength(100)]
        public string BankDisbursementReference { get; set; } = string.Empty;

        public DateTime ActualDisbursedAt { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(100)]
        public string ConfirmedByFinanceOfficer { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Perintah callback penolakan berkas pencairan gaji dari Modul Keuangan (INT-HR-54, PAY-B7, VR-PAY-16).
    /// </summary>
    public class FinanceRejectionCallbackCommand
    {
        [Required]
        public Guid HandoffBatchId { get; set; }

        [Required]
        public Guid PayrollRunId { get; set; }

        [Required]
        public string Status { get; set; } = "PaymentRejected";

        [Required, MinLength(15, ErrorMessage = "Alasan penolakan dari Finance wajib diisi minimal 15 karakter.")]
        [MaxLength(1000)]
        public string RejectionReason { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string RejectedByFinanceOfficer { get; set; } = string.Empty;

        public DateTime RejectedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Hasil pemrosesan callback konfirmasi pembayaran bank Finance.
    /// </summary>
    public class FinanceConfirmationResultDto
    {
        public Guid HandoffBatchId { get; set; }
        public Guid PayrollRunId { get; set; }
        public string Status { get; set; } = "Completed";
        public int PayslipsReleasedCount { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Hasil pemrosesan callback penolakan pembayaran bank Finance (rollback terkendali).
    /// </summary>
    public class FinanceRejectionResultDto
    {
        public Guid HandoffBatchId { get; set; }
        public Guid PayrollRunId { get; set; }
        public string Status { get; set; } = "RejectedByFinance";
        public string RollbackStatus { get; set; } = "NeedCorrection";
        public DateTime ProcessedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Ringkasan slip gaji pegawai mandiri pada portal ESS.
    /// </summary>
    public class SelfServicePayslipSummaryDto
    {
        public Guid PayslipId { get; set; }
        public Guid PayrollRunId { get; set; }
        public string PayslipNumber { get; set; } = string.Empty;
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public decimal NetPay { get; set; }
        public decimal GrossPay { get; set; }
        public string PayslipStatus { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public bool IsDownloaded { get; set; }
        public int DownloadCount { get; set; }
    }

    /// <summary>
    /// Detail lengkap rincian slip gaji pegawai mandiri pasca verifikasi PIN 6-digit.
    /// </summary>
    public class SelfServicePayslipDetailDto
    {
        public Guid PayslipId { get; set; }
        public string PayslipNumber { get; set; } = string.Empty;
        public string EmployeeNik { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public decimal BaseSalary { get; set; }
        public decimal TotalRecurringEarning { get; set; }
        public decimal TotalVariableEarning { get; set; }
        public decimal TotalOvertimeAmount { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetPay { get; set; }
        public DateTime? PublishedAt { get; set; }
        public List<PayslipItemLineDto> Earnings { get; set; } = new();
        public List<PayslipItemLineDto> Deductions { get; set; } = new();
    }

    /// <summary>
    /// Butir baris penerimaan atau potongan pada slip gaji.
    /// </summary>
    public class PayslipItemLineDto
    {
        public string ComponentCode { get; set; } = string.Empty;
        public string ComponentName { get; set; } = string.Empty;
        public string ComponentCategory { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Permintaan verifikasi PIN sekunder 6-digit ESS (VR-PAY-22, HRD-ASM-30).
    /// </summary>
    public class VerifyPayslipPinRequest
    {
        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN sekunder slip gaji wajib terdiri tepat dari 6 digit numerik.")]
        public string Pin { get; set; } = string.Empty;
    }

    /// <summary>
    /// Hasil verifikasi autentikasi PIN sekunder 6-digit ESS.
    /// </summary>
    public class PayslipPinVerificationResultDto
    {
        public bool IsSuccess { get; set; }
        public string? PinToken { get; set; }
        public int RemainingAttempts { get; set; }
        public int LockoutMinutesRemaining { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
