using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    /// <summary>
    /// Permintaan kalkulasi pelunasan utang pinjaman pegawai pada pesangon Exit Clearance Sub-20 (BE-HRD-174, HRD-DEC-132, DED-B7, VR-DED-07).
    /// </summary>
    public class ExitClearanceReceivableCalculationRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Range(0, 1000000000)]
        public decimal SeverancePayAmount { get; set; }

        [Range(0, 500000000)]
        public decimal UnusedLeaveCompensationAmount { get; set; }

        [Range(0, 500000000)]
        public decimal OtherAllowancesAmount { get; set; }

        public DateTime EffectiveSeparationDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(100)]
        public string SeparationReason { get; set; } = "Resign"; // 'Resign', 'PHK', 'Pensiun'
    }

    /// <summary>
    /// Rincian jadwal angsuran yang dipotong dari paket pesangon keluar (BE-HRD-174).
    /// </summary>
    public class ExitClearanceScheduleSettlementItemDto
    {
        public Guid ScheduleId { get; set; }
        public string ScheduleNumber { get; set; } = string.Empty;
        public string DeductionCategory { get; set; } = string.Empty;
        public int PriorityLevel { get; set; }
        public string? ExternalReferenceId { get; set; }
        public decimal OriginalRemainingPrincipal { get; set; }
        public decimal DeductedFromSeverance { get; set; }
        public decimal RemainingBalanceAfterSettlement { get; set; }
        public string StatusAfterSettlement { get; set; } = string.Empty;
    }

    /// <summary>
    /// Hasil kalkulasi dan simulasi pelunasan utang pada Exit Clearance (BE-HRD-174, INT-HR-60, DED-W5).
    /// </summary>
    public class ExitClearanceReceivableCalculationResultDto
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNik { get; set; }
        public decimal TotalGrossSeverancePackage { get; set; }
        public decimal TotalOutstandingDebt { get; set; }
        public decimal DebtDeductionCapPercentage { get; set; } = 100.00m; // Pengecualian batas 50% hingga 100% (DED-B7, VR-DED-07)
        public decimal TotalDeductedAmount { get; set; }
        public decimal NetPayableSeverance { get; set; }
        public decimal DeficitResidualDebt { get; set; }
        public bool IsFullySettled { get; set; }
        public string ExitClearanceStatus { get; set; } = "Cleared"; // 'Cleared' atau 'BlockedPendingSettlement' (DED-W5)
        public string? OutstandingReceivableStatementNumber { get; set; } // Nomor surat tagihan resmi ke Finance jika defisit
        public List<ExitClearanceScheduleSettlementItemDto> SettlementDetails { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permintaan konfirmasi eksekusi pelunasan utang Exit Clearance pada database (BE-HRD-174, INT-HR-60).
    /// </summary>
    public class ConfirmExitClearanceSettlementRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Range(0, 1000000000)]
        public decimal SeverancePayAmount { get; set; }

        [Range(0, 500000000)]
        public decimal UnusedLeaveCompensationAmount { get; set; }

        [Range(0, 500000000)]
        public decimal OtherAllowancesAmount { get; set; }

        public DateTime EffectiveSeparationDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(100)]
        public string SeparationReason { get; set; } = "Resign";

        public string? Notes { get; set; }
    }

    /// <summary>
    /// Webhook payload callback realisasi pemotongan gaji bulanan tanggal 25 ke Departemen Keuangan (BE-HRD-174, INT-HR-58, HRD-DEC-129).
    /// </summary>
    public class FinanceRealizedDeductionCallbackDto
    {
        public Guid BatchId { get; set; } = Guid.NewGuid();
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public Guid PayrollRunId { get; set; }
        public DateTime ExecutionDate { get; set; } = DateTime.UtcNow;
        public int TotalEmployeesProcessed { get; set; }
        public decimal TotalDeductionAmountRealized { get; set; }
        public decimal TotalDeferredAmount { get; set; }
        public List<FinanceRealizedDeductionItemCallbackDto> RealizationItems { get; set; } = new();
    }

    /// <summary>
    /// Rincian per item realisasi pemotongan untuk modul Keuangan (Finance AR Code) (INT-HR-58).
    /// </summary>
    public class FinanceRealizedDeductionItemCallbackDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeNik { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string ScheduleNumber { get; set; } = string.Empty;
        public string? ExternalReferenceId { get; set; } // Kode AR Finance
        public string DeductionCategory { get; set; } = string.Empty;
        public int PriorityLevel { get; set; }
        public decimal ScheduledAmount { get; set; }
        public decimal ActualDeductedAmount { get; set; }
        public decimal DeferredAmount { get; set; }
        public decimal RemainingPrincipalAfterExecution { get; set; }
        public string ExecutionStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Respons konfirmasi penerimaan callback oleh Modul Keuangan (INT-HR-58).
    /// </summary>
    public class FinanceCallbackAcknowledgmentDto
    {
        public bool IsSuccess { get; set; }
        public DateTime AcknowledgedAt { get; set; } = DateTime.UtcNow;
        public string CallbackAuditReference { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
