using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

/// <summary>
/// Surat kepada Finance bahwa satu tender mencapai keadaan akhirnya (BKC-DEC-106, BKC-DES-037).
/// Aggregate root tersendiri tanpa foreign key ke finalisasi, sehingga pembayaran yang mendahului
/// finalisasi tetap dapat diteruskan ke buku penerimaan Finance tanpa menunggu tagihan final.
/// </summary>
[Table("BilCollectionHandoff", Schema = "public")]
public sealed class BilCollectionHandoff : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Tender yang keadaannya dilaporkan.</summary>
    public Guid TenderId { get; set; }

    /// <summary>Penyelesaian pembayaran induknya.</summary>
    public Guid SettlementId { get; set; }

    /// <summary>Tagihan yang dibayar.</summary>
    public Guid InvoiceId { get; set; }

    /// <summary>Daftar identitas alokasi pembayaran, dipisah koma. Kosong bila tender belum dialokasikan.</summary>
    [MaxLength(1000)]
    public string? PaymentAllocationIds { get; set; }

    /// <summary>Membedakan tunai dari non-tunai, dan menentukan hasil finansial resep.</summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>Rekening atau kanal non-tunai.</summary>
    public Guid? PaymentMethodAccountId { get; set; }

    /// <summary>Disalin apa adanya. Finance MUST NOT menghitung ulang.</summary>
    public decimal Amount { get; set; }

    /// <summary>Bukti yang dipegang pasien. Kosong bila tender belum menghasilkan kwitansi.</summary>
    [MaxLength(50)]
    public string? KwitansiNumber { get; set; }

    /// <summary>Wajib terisi untuk tender tunai; dasar rekonsiliasi kas shift.</summary>
    public Guid? CashierShiftId { get; set; }

    /// <summary>Rujukan penyedia pembayaran untuk non-tunai (sensitif).</summary>
    [MaxLength(150)]
    public string? ProviderReference { get; set; }

    /// <summary>Identitas kejadian penyedia; anti-ganda dari sisi penyedia (sensitif).</summary>
    [MaxLength(100)]
    public string? ProviderEventId { get; set; }

    /// <summary>Waktu uang benar-benar diterima, bukan waktu baris dibuat.</summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Status tagihan pada saat itu. Penentu Finance menahan jurnal atau tidak.</summary>
    [Required, MaxLength(30)]
    public string SourceInvoiceStatus { get; set; } = string.Empty;

    /// <summary>SUCCEEDED atau REVERSED.</summary>
    [Required, MaxLength(30)]
    public string TenderStatus { get; set; } = string.Empty;

    /// <summary>Kunci idempotensi deterministik yang diturunkan dari TenderId dan TenderStatus.</summary>
    public Guid HandoffKey { get; set; }

    /// <summary>Rantai telusur ujung ke ujung.</summary>
    public Guid CorrelationId { get; set; }

    /// <summary>Peristiwa yang menyebabkannya.</summary>
    public Guid CausationId { get; set; }

    /// <summary>Status penyerahan fakta: CREATED atau ACKNOWLEDGED.</summary>
    [Required, MaxLength(30)]
    public string Status { get; set; } = BillingHandoffStatuses.Created;

    /// <summary>Waktu saat Finance mengambil/mengakui surat ini.</summary>
    public DateTimeOffset? AcknowledgedAt { get; set; }

    /// <summary>Kendali konkurensi optimistik.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilTender Tender { get; set; } = null!;
    public BilSettlement Settlement { get; set; } = null!;
    public BilInvoice Invoice { get; set; } = null!;
    public MstPaymentMethod PaymentMethod { get; set; } = null!;
    public BilCashierShift? CashierShift { get; set; }
}
