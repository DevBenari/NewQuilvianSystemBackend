using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Status penyelesaian tagihan pasien pada sebuah invoice, TERPISAH dari status siklus hidup invoice (OPEN/FINAL/CLOSED).
/// Diturunkan murni dari fakta yang sudah dihitung Backend (sisa tagihan pasien kanonik, uang yang benar-benar masuk, dan
/// handoff piutang), tidak disimpan, sehingga data lama otomatis konsisten tanpa migrasi.
///
/// Aturan inti:
/// - "Uang masuk" (alokasi tender/deposit) adalah satu-satunya dasar "Lunas karena pasien membayar". Coverage penjamin dan
///   Benefit RS bukan kas: pengakuan handoff/piutang TIDAK pernah dianggap uang diterima.
/// - CLOSED tidak otomatis "Lunas": CLOSED dapat berasal dari pembayaran pasien, coverage penjamin, Benefit RS, piutang yang
///   dialihkan, atau gabungannya. Labelnya mengikuti fakta tersebut.
/// - Invoice OPEN dengan kewajiban pasien nol belum selesai: ia MENUNGGU penyelesaian kasir (finalisasi belum terjadi).
/// </summary>
public static class BillingInvoiceSettlementOutcomes
{
    /// <summary>Invoice belum memiliki hasil perhitungan tersimpan.</summary>
    public const string NotCalculated = "NOT_CALCULATED";

    /// <summary>Masih ada sisa tagihan pasien dan belum ada uang masuk.</summary>
    public const string PatientDue = "PATIENT_DUE";

    /// <summary>Masih ada sisa tagihan pasien dan sebagian sudah dibayar.</summary>
    public const string PatientPartial = "PATIENT_PARTIAL";

    /// <summary>Sisa pasien nol karena pasien benar-benar membayar (uang masuk).</summary>
    public const string PatientPaid = "PATIENT_PAID";

    /// <summary>Invoice OPEN, kewajiban pasien nol, belum ada uang masuk: menunggu penyelesaian/finalisasi kasir.</summary>
    public const string AwaitingCompletion = "AWAITING_COMPLETION";

    /// <summary>Selesai (FINAL/CLOSED) karena seluruhnya ditanggung penjamin/asuransi (handoff piutang penjamin).</summary>
    public const string CoveredByPayer = "COVERED_BY_PAYER";

    /// <summary>Selesai (FINAL/CLOSED) karena seluruhnya ditanggung Benefit RS (handoff Benefit RS internal).</summary>
    public const string CoveredByBenefit = "COVERED_BY_BENEFIT";

    /// <summary>Selesai (FINAL/CLOSED) karena kewajiban dialihkan menjadi piutang (AR karyawan / piutang pasien).</summary>
    public const string DeferredToReceivable = "DEFERRED_TO_RECEIVABLE";

    /// <summary>Selesai (FINAL/CLOSED) dengan lebih dari satu sumber penyelesaian (kas, penjamin, Benefit RS, piutang).</summary>
    public const string Mixed = "MIXED";

    /// <summary>Selesai (FINAL/CLOSED) tanpa kewajiban maupun coverage tercatat.</summary>
    public const string NoObligation = "NO_OBLIGATION";

    /// <summary>Invoice diselesaikan lewat penghapusan buku.</summary>
    public const string WrittenOff = "SETTLED_BY_WRITE_OFF";
}

/// <summary>Kesesuaian status tersimpan invoice (FINAL/CLOSED) dengan sisa tagihan kanonik; hanya bacaan, tidak menulis apa pun.</summary>
public static class BillingClosureDriftStates
{
    public const string Ok = "OK";

    /// <summary>Tersimpan FINAL tetapi sisa tagihan pasien nol: seharusnya CLOSED (SyncClosureAsync akan menutupnya saat peristiwa berikutnya).</summary>
    public const string ShouldBeClosed = "SHOULD_BE_CLOSED";

    /// <summary>Tersimpan CLOSED tetapi sisa tagihan pasien muncul lagi: seharusnya FINAL (mekanisme reopening).</summary>
    public const string ShouldBeFinal = "SHOULD_BE_FINAL";
}

public readonly record struct BillingSettlementFacts(
    string InvoiceStatus,
    bool HasCalculation,
    decimal PatientPaidAmount,
    decimal PatientOutstandingAmount,
    decimal PayerCoverageAmount,
    decimal InternalBenefitAmount,
    decimal EmployeeReceivableAmount,
    decimal PatientReceivableAmount);

public static class BillingSettlementStatusResolver
{
    private const decimal Tolerance = 0.005m;

    public static string Resolve(in BillingSettlementFacts facts)
    {
        if (facts.InvoiceStatus == BillingInvoiceStatuses.SettledByWriteOff)
            return BillingInvoiceSettlementOutcomes.WrittenOff;
        if (!facts.HasCalculation)
            return BillingInvoiceSettlementOutcomes.NotCalculated;

        if (facts.PatientOutstandingAmount > Tolerance)
        {
            return facts.PatientPaidAmount > Tolerance
                ? BillingInvoiceSettlementOutcomes.PatientPartial
                : BillingInvoiceSettlementOutcomes.PatientDue;
        }

        var cash = facts.PatientPaidAmount > Tolerance;
        var finalized = facts.InvoiceStatus == BillingInvoiceStatuses.Final
            || facts.InvoiceStatus == BillingInvoiceStatuses.Closed;

        // OPEN: finalisasi belum terjadi, jadi coverage belum menjadi penyelesaian. Sisa nol tanpa uang masuk = menunggu.
        if (!finalized)
            return cash ? BillingInvoiceSettlementOutcomes.PatientPaid : BillingInvoiceSettlementOutcomes.AwaitingCompletion;

        var payer = facts.PayerCoverageAmount > Tolerance;
        var benefit = facts.InternalBenefitAmount > Tolerance;
        var receivable = facts.EmployeeReceivableAmount > Tolerance || facts.PatientReceivableAmount > Tolerance;

        var kinds = (cash ? 1 : 0) + (payer ? 1 : 0) + (benefit ? 1 : 0) + (receivable ? 1 : 0);
        if (kinds > 1) return BillingInvoiceSettlementOutcomes.Mixed;
        if (cash) return BillingInvoiceSettlementOutcomes.PatientPaid;
        if (payer) return BillingInvoiceSettlementOutcomes.CoveredByPayer;
        if (benefit) return BillingInvoiceSettlementOutcomes.CoveredByBenefit;
        if (receivable) return BillingInvoiceSettlementOutcomes.DeferredToReceivable;
        return BillingInvoiceSettlementOutcomes.NoObligation;
    }

    public static string ResolveClosureDrift(string invoiceStatus, bool hasCalculation, decimal patientOutstandingAmount)
    {
        if (!hasCalculation) return BillingClosureDriftStates.Ok;
        if (invoiceStatus == BillingInvoiceStatuses.Final && patientOutstandingAmount <= Tolerance)
            return BillingClosureDriftStates.ShouldBeClosed;
        if (invoiceStatus == BillingInvoiceStatuses.Closed && patientOutstandingAmount > Tolerance)
            return BillingClosureDriftStates.ShouldBeFinal;
        return BillingClosureDriftStates.Ok;
    }
}
