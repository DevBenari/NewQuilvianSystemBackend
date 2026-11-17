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
    public Guid? PaymentMethodAccountId { get; set; }
    /// <summary>
    /// Nominal pembayaran NETO yang diterapkan untuk melunasi tagihan. Hanya nilai ini yang boleh dipakai
    /// settlement, alokasi, status pelunasan, SystemCash shift, handoff Finance, dan refund. Uang fisik bruto
    /// yang diserahkan pasien dan kembalian TIDAK pernah masuk ke sini.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Uang fisik yang diserahkan pasien (khusus metode MstPaymentMethod.IsCash). Selalu &gt;= <see cref="Amount"/>.
    /// Null = tender non-tunai ATAU tender tunai lama sebelum kolom ini ada (tidak ada kembalian fiktif untuk data lama).
    /// Untuk tender tunai BARU nilai ini WAJIB dikirim kasir pada setiap pembayaran, termasuk uang pas (keputusan
    /// pemilik modul Billing, 17 November 2026): backend menolak tender tunai tanpa uang diterima.
    /// </summary>
    public decimal? CashTenderedAmount { get; set; }

    /// <summary>
    /// Kembalian yang diserahkan kasir kepada pasien = <see cref="CashTenderedAmount"/> - <see cref="Amount"/>.
    /// Dihitung backend, tidak pernah dipercaya dari input klien. Bukan pendapatan, deposit, piutang, atau refund.
    /// Null = data lama / non-tunai; 0 = tunai pas.
    /// </summary>
    public decimal? CashChangeAmount { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = BillingTenderStatuses.Created;
    [MaxLength(150)] public string? ProviderReference { get; set; }
    [MaxLength(50)] public string? ProviderStatusCode { get; set; }
    [MaxLength(150)] public string? CashierReferenceNote { get; set; }
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

    /// <summary>
    /// Percobaan pembayaran TUNAI yang dibatalkan kasir selagi masih <see cref="Created"/> (belum direkonsiliasi, belum
    /// ada penerimaan kas). Status akhir: riwayat tetap tersimpan, tidak masuk saldo yang dipesan, dan tidak pernah
    /// diubah ke status lain. Beda dengan <see cref="Reversed"/> yang membalik pembayaran yang sudah berhasil.
    /// </summary>
    public const string Cancelled = "CANCELLED";
}
