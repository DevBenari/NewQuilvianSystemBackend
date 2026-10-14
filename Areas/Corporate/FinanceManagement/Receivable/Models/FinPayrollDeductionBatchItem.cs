using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Satu baris hasil potongan gaji pada berkas batch (BE-FIN-117, FIN-DES-106).
/// </summary>
[Table("FinPayrollDeductionBatchItem", Schema = "public")]
public sealed class FinPayrollDeductionBatchItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BatchId { get; set; }
    public FinPayrollDeductionBatch? Batch { get; set; }

    public int RowNumber { get; set; }

    public Guid? InstallmentId { get; set; }
    public FinReceivableInstallment? Installment { get; set; }

    [Required, MaxLength(60)]
    public string InstallmentCode { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>BERHASIL, SEBAGIAN, GAGAL.</summary>
    [Required, MaxLength(10)]
    public string ResultStatus { get; set; } = FinPayrollDeductionItemResultStatuses.Berhasil;

    public decimal DeductedAmount { get; set; }

    public decimal DueAmount { get; set; }

    public decimal? PreviousDeductedAmount { get; set; }

    public decimal? DeltaAmount { get; set; }

    /// <summary>VALID atau INVALID.</summary>
    [Required, MaxLength(10)]
    public string ValidationStatus { get; set; } = FinPayrollDeductionItemValidationStatuses.Valid;

    [MaxLength(500)]
    public string? ValidationMessage { get; set; }

    public DateTimeOffset? AppliedAt { get; set; }
}

public static class FinPayrollDeductionItemResultStatuses
{
    public const string Berhasil = "BERHASIL";
    public const string Sebagian = "SEBAGIAN";
    public const string Gagal = "GAGAL";
}

public static class FinPayrollDeductionItemValidationStatuses
{
    public const string Valid = "VALID";
    public const string Invalid = "INVALID";
}

