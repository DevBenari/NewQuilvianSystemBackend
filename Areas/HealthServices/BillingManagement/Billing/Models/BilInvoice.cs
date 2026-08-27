using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilInvoice", Schema = "public")]
public sealed class BilInvoice : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EncounterId { get; set; }
    [Required, MaxLength(50)] public string InvoiceNumber { get; set; } = string.Empty;
    // Nomor Kwitansi dialokasikan SEKALI saat pertama kali diminta (BKC-DEC-054) dan disimpan di
    // sini supaya reprint mengembalikan nomor yang sama, bukan mengonsumsi nomor urut baru.
    [MaxLength(50)] public string? KwitansiNumber { get; set; }
    [Required, MaxLength(30)] public string ServiceType { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Status { get; set; } = BillingInvoiceStatuses.Open;
    public int CurrentCalculationVersion { get; set; }
    public DateTimeOffset? InvoiceDate { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();
    public ICollection<BilInvoiceItem> Items { get; set; } = new List<BilInvoiceItem>();
    public ICollection<BilCalculationVersion> CalculationVersions { get; set; } = new List<BilCalculationVersion>();
    public ICollection<BilDiscountApplication> DiscountApplications { get; set; } = new List<BilDiscountApplication>();
}

public static class BillingInvoiceStatuses
{
    public const string Open = "OPEN";
    public const string Final = "FINAL";
    public const string Closed = "CLOSED";
    public const string SettledByWriteOff = "SETTLED_BY_WRITE_OFF";
}
