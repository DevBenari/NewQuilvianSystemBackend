using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

/// <summary>
/// Batas kirim ulang otomatis jalur fakta klinis → Billing (<c>RJ-E2E-DEC-009</c>).
/// Satu baris per jalur. Tanpa baris aktif, pekerja latar tidak mengirim ulang sama sekali dan
/// item langsung masuk antrean rekonsiliasi — fail-closed, mengikuti pola <c>RJ-BIL-DEC-010</c>.
/// </summary>
[Table("MstBillingSyncPolicy", Schema = "public")]
public sealed class MstBillingSyncPolicy : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string PolicyCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string PolicyName { get; set; } = string.Empty;

    /// <summary>0 berarti tidak ada kirim ulang otomatis; seluruh kegagalan langsung ke antrean.</summary>
    public int MaxAttemptCount { get; set; }

    public int BaseDelaySeconds { get; set; }

    public int MaxDelaySeconds { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class BillingSyncPolicyCodes
{
    /// <summary>Penyerahan fakta klinis ke folio.</summary>
    public const string FactDispatch = "FACT_DISPATCH";

    /// <summary>Penerusan efek folio ke invoice canonical.</summary>
    public const string InvoiceSync = "INVOICE_SYNC";
}
