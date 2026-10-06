using QuilvianSystemBackend.Responses;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>Nilai <c>category</c> pada Data Tagihan. Dipetakan ke FinReceivable.DebtorType.</summary>
public static class BillingDataCategories
{
    /// <summary>Perusahaan/Asuransi → PAYER.</summary>
    public const string Company = "company";

    /// <summary>Karyawan → EMPLOYEE_BENEFIT. Jalur intake-nya belum ada (FIN-DEC-006), jadi hasilnya kosong.</summary>
    public const string Employee = "employee";

    /// <summary>Pasien Umum → PATIENT_GUARANTOR.</summary>
    public const string GeneralPatient = "generalPatient";
}

public static class BillingDataStatuses
{
    public const string All = "all";
    public const string NotCreated = "notCreated";
    public const string Created = "created";
}

public static class BillingDataPatientTypes
{
    public const string All = "all";
    public const string Outpatient = "outpatient";
    public const string Inpatient = "inpatient";
    public const string Emergency = "emergency";

    /// <summary>Hanya muncul pada respons (Medical Checkup, Telemedicine, Unknown); bukan nilai filter.</summary>
    public const string Other = "other";
}

public static class BillingDataPayerKinds
{
    public const string Insurance = "insurance";
    public const string Company = "company";
}

public static class BillingDataPeriods
{
    public const string All = "all";
    public const string Today = "today";
    public const string Yesterday = "yesterday";
    public const string ThisWeek = "thisWeek";
    public const string ThisMonth = "thisMonth";
    public const string LastMonth = "lastMonth";
}

/// <summary>
/// Alasan sebuah baris Data Tagihan belum bisa dibuatkan Batch Tagihan. Null berarti bisa.
/// Hanya petunjuk tampilan; <c>POST receivable-invoice-batches</c> tetap memeriksa ulang di server.
/// </summary>
public static class BillingDataCreateBlockedReasons
{
    public const string NotPayer = "NOT_PAYER";
    public const string NoDebtorReference = "NO_DEBTOR_REFERENCE";
    public const string AlreadyInBatch = "ALREADY_IN_BATCH";
    public const string InCancelledBatch = "IN_CANCELLED_BATCH";
    public const string StatusNotEligible = "STATUS_NOT_ELIGIBLE";
}

/// <summary>
/// Parameter GET /receivables/billing-data. Semua opsional; bila kosong: kategori company, semua status,
/// semua jenis pasien, semua periode.
/// </summary>
public sealed class BillingDataQuery
{
    /// <summary>company | employee | generalPatient.</summary>
    public string? Category { get; set; }

    /// <summary>Mencari No. Bill, No. Registrasi, nama pasien, No. RM; untuk kategori company juga nama asuransi/penjamin.</summary>
    [MaxLength(200)] public string? Search { get; set; }

    /// <summary>
    /// Pilihan satu penjamin/pasien. Untuk company: Id asuransi (MstInsuranceProvider.Id) ATAU Id perusahaan
    /// penjamin (MstCompanyGuarantor.Id), keduanya dari GET billing-data/payer-options. Untuk employee dan
    /// generalPatient: Id pasien (MstPatient.Id).
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>all | notCreated | created.</summary>
    public string? BillingStatus { get; set; }

    /// <summary>all | outpatient | inpatient | emergency.</summary>
    public string? PatientType { get; set; }

    /// <summary>all | today | yesterday | thisWeek | thisMonth | lastMonth. Diabaikan bila StartDate/EndDate dikirim.</summary>
    public string? Period { get; set; }

    /// <summary>Tanggal awal (inklusif, kalender WIB). Bila dikirim, mengalahkan Period.</summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>Tanggal akhir (inklusif sampai akhir hari WIB). Bila dikirim, mengalahkan Period.</summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>date (bawaan) | invoiceNumber | encounterNumber | medicalRecordNumber | patientName | amount. Nilai lain jatuh ke date.</summary>
    public string SortBy { get; set; } = "date";
    public string SortDirection { get; set; } = "desc";

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>
/// Satu baris Data Tagihan = satu rincian piutang (FinReceivableItem). Saat ini satu piutang memiliki tepat satu
/// rincian, jadi satu baris = satu piutang = satu tagihan Billing.
/// </summary>
public sealed class BillingDataItemResponse
{
    /// <summary>Id piutang (FinReceivable.Id). Inilah yang dikirim ke POST receivable-invoice-batches.</summary>
    public Guid Id { get; set; }
    public Guid ReceivableItemId { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;

    /// <summary>Tanggal kunjungan; bila kunjungannya tidak terbaca (item migrasi lama), tanggal pengakuan piutang.</summary>
    public DateTimeOffset BillingDate { get; set; }

    /// <summary>No. Bill. Kosong untuk item migrasi tagihan lama.</summary>
    public string? InvoiceNumber { get; set; }
    public string? EncounterNumber { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string? PatientName { get; set; }

    /// <summary>outpatient | inpatient | emergency | other; null bila kunjungan tidak terbaca.</summary>
    public string? PatientType { get; set; }

    public Guid? InvoiceId { get; set; }
    public Guid? EncounterId { get; set; }
    public Guid? PatientId { get; set; }

    /// <summary>PAYER | PATIENT_GUARANTOR | EMPLOYEE_BENEFIT.</summary>
    public string DebtorType { get; set; } = string.Empty;
    public Guid? DebtorReferenceId { get; set; }

    /// <summary>Nama penjamin: asuransi, perusahaan penjamin, atau nama penjamin pada kunjungan; nama pasien untuk kategori lain.</summary>
    public string? PayerName { get; set; }

    /// <summary>Id penjamin yang dipakai saringan EntityId (Id asuransi atau Id perusahaan). Hanya untuk kategori company.</summary>
    public Guid? PayerId { get; set; }

    /// <summary>insurance | company; null bila penjamin tidak dapat ditentukan atau bukan kategori company.</summary>
    public string? PayerKind { get; set; }

    /// <summary>Jumlah tagihan = nominal rincian piutang, yaitu nominal yang masuk ke Finance/AR (sudah dihitung Billing).</summary>
    public decimal BillingAmount { get; set; }
    public string ReceivableStatus { get; set; } = string.Empty;

    /// <summary>created | notCreated.</summary>
    public string BillingStatus { get; set; } = BillingDataStatuses.NotCreated;
    public Guid? InvoiceBatchId { get; set; }
    public string? InvoiceBatchNumber { get; set; }
    public string? InvoiceBatchStatus { get; set; }

    public bool CanCreateInvoice { get; set; }

    /// <summary>Lihat <see cref="BillingDataCreateBlockedReasons"/>; null bila CanCreateInvoice true.</summary>
    public string? CreateBlockedReason { get; set; }
}

/// <summary>Ringkasan atas SELURUH hasil saringan (bukan hanya halaman aktif).</summary>
public sealed class BillingDataSummaryResponse
{
    /// <summary>Jumlah seluruh BillingAmount pada hasil saringan.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Jumlah pasien unik (PatientId berbeda) pada hasil saringan. Baris tanpa PatientId tidak dihitung.</summary>
    public int PatientCount { get; set; }

    /// <summary>Penjamin/pasien terpilih, atau "Semua Perusahaan" / "Semua Pasien" / "Semua Karyawan".</summary>
    public string EntityName { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;
}

/// <summary>Parameter GET /receivables/billing-data/payer-options (isi dropdown pilihan asuransi/perusahaan).</summary>
public sealed class BillingDataPayerOptionQuery
{
    [MaxLength(200)] public string? Search { get; set; }

    /// <summary>Jumlah maksimum pilihan (1-50, bawaan 20). Gunakan Search untuk mempersempit; daftar tidak dikirim utuh.</summary>
    [Range(1, 50)] public int Limit { get; set; } = 20;
}

/// <summary>Satu pilihan penjamin. Id-nya dikirim sebagai EntityId pada GET billing-data.</summary>
public sealed class BillingDataPayerOptionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>insurance | company.</summary>
    public string Kind { get; set; } = string.Empty;
}

public sealed class BillingDataPagedResponse : PagedResult<BillingDataItemResponse>
{
    public BillingDataSummaryResponse Summary { get; set; } = new();

    /// <summary>Penjelasan bila hasil kosong karena sumber datanya memang belum ada (kategori employee). Null pada kasus lain.</summary>
    public string? Notice { get; set; }
}
