using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>BE-FIN-098, data-dictionary.md §8.2. Form-data: berkas CSV/XLSX + ruas ini — pola sama
/// dengan UploadOpeningItemBatchRequest (FinanceOpeningItemBatchesController).</summary>
public sealed class ImportEmployeeReceivableBatchRequest
{
    [Required] public IFormFile File { get; set; } = null!;
    [Required] public DateOnly CutoverDate { get; set; }
    [Required, MaxLength(200)] public string AccountingReferenceDocument { get; set; } = string.Empty;
    /// <summary>FIN-VAL-(kontrol total): MUST sama persis dengan jumlah OutstandingAmount seluruh baris.</summary>
    [Range(0.01, double.MaxValue)] public decimal ControlTotalAmount { get; set; }
}

public sealed class EmployeeReceivableRowError
{
    public int RowNumber { get; set; }
    public string? NomorKartuLama { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class EmployeeReceivableBatchResponse
{
    public Guid BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int TotalItemCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public DateOnly CutoverDate { get; set; }
}
