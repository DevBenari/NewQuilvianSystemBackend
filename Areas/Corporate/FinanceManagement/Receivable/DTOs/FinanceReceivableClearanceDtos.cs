using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>BE-FIN-096, FIN-API-1.9 O.4. FIN-VAL-245: maksimum 100 pegawai per permintaan.</summary>
public sealed class ClearanceBatchQuery
{
    [Required, MaxLength(100, ErrorMessage = "Maksimum 100 pegawai per permintaan. Pecah menjadi beberapa permintaan.")]
    public List<Guid> BenefitOwnerIds { get; set; } = new();
}

/// <summary>BE-FIN-096, FIN-DES-101. IsCleared/OutstandingAmount MUST selalu dihitung saat
/// diminta, tidak pernah dibaca dari kolom tersimpan — class ini bukan entity, murni hasil hitung.</summary>
public sealed class EmployeeClearanceResponse
{
    public Guid BenefitOwnerId { get; set; }
    public bool IsCleared { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int OpenReceivableCount { get; set; }
    public DateOnly? OldestDueDate { get; set; }
    public DateTimeOffset CalculatedAt { get; set; }
}
