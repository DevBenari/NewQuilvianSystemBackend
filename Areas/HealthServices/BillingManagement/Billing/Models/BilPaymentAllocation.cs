using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilPaymentAllocation", Schema = "public")]
public sealed class BilPaymentAllocation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SettlementId { get; set; }
    [Required, MaxLength(30)] public string TargetType { get; set; } = BillingAllocationTargetTypes.Invoice;
    public Guid TargetId { get; set; }
    public decimal Amount { get; set; }
    public int? CalculationVersion { get; set; }
    public DateTimeOffset AllocatedAt { get; set; }
    public Guid? ReversesAllocationId { get; set; }
    public BilSettlement Settlement { get; set; } = null!;
}

public static class BillingAllocationTargetTypes
{
    /// <summary>Dana yang melunasi sisa kewajiban pasien (K) pada invoice. Semua perhitungan "sudah dibayar" untuk K hanya membaca target ini.</summary>
    public const string Invoice = "INVOICE";

    /// <summary>
    /// Dana yang melunasi kelebihan biaya di atas plafon Benefit Karyawan RS (E) pada kunjungan berpenjamin internal.
    /// TargetId = InvoiceId. Sengaja TIDAK dibaca oleh perhitungan sisa tagihan K: E bukan bagian PatientAmount
    /// kalkulasi, sehingga tidak boleh menutup invoice, mengurangi K, atau menjadi kredit refund.
    /// </summary>
    public const string BenefitExcess = "BENEFIT_EXCESS";
}
