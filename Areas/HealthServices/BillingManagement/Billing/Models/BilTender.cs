using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilTender", Schema = "public")]
public sealed class BilTender : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SettlementId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingTenderStatuses.Created;
    [MaxLength(150)] public string? ProviderReference { get; set; }
    [MaxLength(50)] public string? ProviderStatusCode { get; set; }
    // Catatan referensi yang diketik kasir sendiri saat tender dibuat (mis. nomor bukti transfer
    // manual). BERBEDA dari ProviderReference: kolom itu murni hasil rekonsiliasi provider
    // pembayaran (IBillingPaymentProviderAdapter) dan tidak pernah diisi manual - lihat
    // BillingSettlementService.AddTenderAsync. Kolom ini tidak divalidasi/direkonsiliasi apa pun;
    // murni catatan kasir untuk ditelusuri manual selama provider pembayaran belum terintegrasi
    // (BKC-BLK-PROV-001).
    [MaxLength(150)] public string? CashierReferenceNote { get; set; }
    // Nomor Kwitansi dialokasikan SEKALI saat tender ini dibuat (BKC-DEC-057) - satu nomor per
    // pembayaran/tender, BUKAN satu per invoice. Reprint tender yang sama selalu mengembalikan
    // nomor ini apa adanya karena tender tidak pernah dibuat ulang untuk permintaan yang sama
    // (idempotency key). Split payment dengan beberapa tender menghasilkan beberapa nomor
    // Kwitansi berbeda - konsisten dengan pola legacy KasirQuilvian1 (MainKasirDetail.NoKwitansi,
    // satu nomor per baris pembayaran).
    [MaxLength(50)] public string? KwitansiNumber { get; set; }
    public Guid IdempotencyKey { get; set; }
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
    public Guid? CashierShiftId { get; set; }
    public DateTimeOffset? ProviderOccurredAt { get; set; }
    [MaxLength(100)] public string? LastProviderEventId { get; set; }
    [MaxLength(64)] public string? LastProviderPayloadHash { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();
    public BilSettlement Settlement { get; set; } = null!;
}

public static class BillingTenderStatuses
{
    public const string Created = "CREATED";
    public const string Pending = "PENDING";
    public const string Succeeded = "SUCCEEDED";
    public const string Failed = "FAILED";
    public const string Expired = "EXPIRED";
    public const string Reversed = "REVERSED";
}
