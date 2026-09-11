namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

// Ad-hoc, di luar roadmap, permintaan langsung pengguna: halaman "Riwayat Pembayaran" - daftar
// SEMUA invoice yang sudah punya minimal satu pembayaran (tender SUCCEEDED), lintas pasien, beda
// dari Running Invoice (BillingInvoiceQuery/InvoiceSummaryResponse) yang tidak membawa info
// penjamin/pembayaran sama sekali. Satu baris = satu invoice (bukan satu baris per tender) -
// keputusan pengguna eksplisit: kolom "cicilan ke berapa" menampilkan progres tender TERAKHIR,
// dan aksi "Lihat Kwitansi" menampilkan daftar SEMUA Kwitansi invoice itu (Tenders di bawah).
public sealed class PaymentHistoryQuery
{
    public string? Search { get; set; }
    public string? ServiceType { get; set; }
    public DateTime? VisitDateFrom { get; set; }
    public DateTime? VisitDateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class PaymentHistoryItemResponse
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime? VisitDate { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;

    // Tunai/Asuransi/Penjamin Perusahaan - dari RegPatientEncounterGuarantor.PaymentType aktif.
    public string PatientType { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    // "Tunai" untuk pasien tunai (tidak punya baris guarantor bertipe Insurance/CompanyGuarantor),
    // nama provider asuransi/perusahaan penjamin untuk yang lain - dari PaymentSourceNameSnapshot.
    public string GuarantorName { get; set; } = string.Empty;

    // Null untuk Tunai/CompanyGuarantor - hanya diisi untuk penjamin Insurance, dari
    // MstInsuranceProvider.ClaimMethod (pola yang sama dipakai BE-BKC-023/InsuranceInvoicePayerResponse).
    public string? ClaimMethod { get; set; }

    // Total yang seharusnya ditagih ke pasien (PatientAmount kalkulasi TERAKHIR invoice ini) -
    // BUKAN gross invoice, supaya konsisten dengan nominal yang benar-benar dikumpulkan kasir
    // lewat tender/Kwitansi.
    public decimal TotalBillAmount { get; set; }

    public decimal TotalPaidAmount { get; set; }

    // Lunas bila seluruh TotalBillAmount sudah tertutup oleh tender SUCCEEDED yang terkumpul.
    public bool IsFullyPaid { get; set; }

    // Seluruh tender SUCCEEDED milik invoice ini, diurutkan kronologis - frontend yang
    // menurunkan label "Angsuran N/M" (M = Count di sini), konsisten dengan logika yang sama
    // dipakai Kwitansi (satu sumber kebenaran, bukan dihitung ulang backend maupun frontend
    // dengan cara berbeda).
    public IReadOnlyList<PaymentHistoryTenderResponse> Tenders { get; set; } = [];
}

public sealed class PaymentHistoryTenderResponse
{
    public Guid Id { get; set; }
    public Guid SettlementId { get; set; }
    public string KwitansiNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
}
