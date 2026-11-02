using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

/// <summary>
/// Nominal satu refund case atas satu baris tagihan (<c>MCU-DEC-043</c>, <c>BE-MCU-026</c>).
/// Sisa yang dapat direfund pada sebuah baris = nilai baris − jumlah <see cref="Amount"/> dari case yang
/// tidak ditolak; dengan itu satu baris dapat direfund berturut-turut tanpa pernah melebihi nilainya.
/// </summary>
/// <remarks>
/// Case lama yang lahir sebelum tabel ini ada tidak punya rincian; Billing menganggapnya merefund penuh
/// baris-baris pada <see cref="BilRefundCase.SelectedBillingItemIdsJson"/> — sama dengan aturan layar refund
/// sebelum perluasan ini.
/// </remarks>
[Table("BilRefundCaseItem", Schema = "public")]
public sealed class BilRefundCaseItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RefundCaseId { get; set; }
    public Guid InvoiceItemId { get; set; }
    public decimal Amount { get; set; }

    public BilRefundCase RefundCase { get; set; } = null!;
    public BilInvoiceItem InvoiceItem { get; set; } = null!;
}
