namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

/// <summary>
/// Ringkasan tagihan satu kunjungan untuk workspace dokter (<c>RJ-E2E-DEC-008</c>,
/// <c>contracts/api-contract.md</c> V2). Sengaja tanpa harga per item: yang boleh dibaca dokter
/// hanya status, total, dan bagian penjamin/pasien.
/// </summary>
public sealed class EncounterBillingSummaryResponse
{
    public Guid EncounterId { get; set; }

    /// <summary><c>NO_INVOICE</c>, <c>OPEN</c>, <c>FINAL</c>, <c>CLOSED</c>, <c>SETTLED_BY_WRITE_OFF</c>.</summary>
    public string BillingStatus { get; set; } = EncounterBillingSummaryStatuses.NoInvoice;

    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }

    /// <summary>Jumlah item invoice aktif.</summary>
    public int ServiceCount { get; set; }

    public decimal GrossAmount { get; set; }

    /// <summary><c>PrimaryAmount</c> kalkulasi Billing.</summary>
    public decimal PayerAmount { get; set; }

    public decimal PatientAmount { get; set; }
    public string PaymentSourceLabel { get; set; } = string.Empty;
    public DateTimeOffset? LastUpdatedAt { get; set; }

    /// <summary>Pelayanan kunjungan ini yang belum sampai ke invoice dan masih dicoba otomatis.</summary>
    public int PendingSyncCount { get; set; }

    /// <summary>Pelayanan atau fakta kunjungan ini yang menunggu penanganan manual.</summary>
    public int ReconciliationCount { get; set; }

    public List<EncounterBillingSummaryServiceResponse> Services { get; set; } = [];
}

/// <summary>Satu pelayanan pada ringkasan — tanpa <c>UnitPrice</c>/<c>TotalPrice</c>.</summary>
public sealed class EncounterBillingSummaryServiceResponse
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string ServiceStatusLabel { get; set; } = string.Empty;

    /// <summary><c>RECORDED</c>, <c>PENDING</c>, <c>RECONCILIATION</c>, <c>NOT_BILLED</c>.</summary>
    public string BillingState { get; set; } = EncounterBillingSummaryBillingStates.Recorded;
}

public static class EncounterBillingSummaryStatuses
{
    public const string NoInvoice = "NO_INVOICE";
}

public static class EncounterBillingSummaryBillingStates
{
    public const string Recorded = "RECORDED";
    public const string Pending = "PENDING";
    public const string Reconciliation = "RECONCILIATION";
    public const string NotBilled = "NOT_BILLED";
}
