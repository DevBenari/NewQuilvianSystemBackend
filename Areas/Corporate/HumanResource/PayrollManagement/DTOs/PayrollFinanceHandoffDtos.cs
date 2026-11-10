using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    /// <summary>
    /// Baris rincian instruksi pencairan gaji pegawai ke rekening bank tujuan (INT-HR-53, BE-HRD-158).
    /// </summary>
    public class PayrollDisbursementLineDto
    {
        public Guid DetailId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeNik { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DestinationBankCode { get; set; } = string.Empty;
        public string DestinationBankAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountHolder { get; set; } = string.Empty;
        public decimal TransferAmount { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payload paket serah terima final penggajian berintegritas Checksum SHA-256 ke Modul Finance (INT-HR-53).
    /// </summary>
    public class PayrollFinanceHandoffPayload
    {
        public Guid HandoffBatchId { get; set; }
        public Guid PayrollRunId { get; set; }
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "BankTransferBatch";
        public string SourceBankAccountId { get; set; } = "RS-OPERASIONAL-UTAMA";
        public int TotalEmployees { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalDeductionsAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public string Currency { get; set; } = "IDR";
        public DateTime HandoffTimestamp { get; set; }
        public Guid HandedOverByUserId { get; set; }
        public string IntegrityChecksum { get; set; } = string.Empty;
        public List<PayrollDisbursementLineDto> DisbursementLines { get; set; } = new();
    }

    /// <summary>
    /// DTO hasil eksekusi pengiriman berkas serah terima ke Modul Finance.
    /// </summary>
    public class FinanceHandoffBatchResultDto
    {
        public Guid HandoffBatchId { get; set; }
        public Guid PayrollRunId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string HandoffStatus { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public decimal TotalDisbursementAmount { get; set; }
        public string ChecksumSha256 { get; set; } = string.Empty;
        public DateTime TransferredAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO pemantauan status penerimaan berkas di Departemen Keuangan (Finance).
    /// </summary>
    public class FinanceHandoffStatusDto
    {
        public Guid HandoffBatchId { get; set; }
        public Guid PayrollRunId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string HandoffStatus { get; set; } = string.Empty;
        public DateTime TransferredAt { get; set; }
        public string? FinanceReferenceNo { get; set; }
        public string? FinanceNotes { get; set; }
        public string ChecksumSha256 { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public decimal TotalDisbursementAmount { get; set; }
        public bool IsLocked { get; set; }
        public string RunStatus { get; set; } = string.Empty;
    }
}
