using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Dtos;
using QuilvianSystemBackend.Responses;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>Nilai <c>category</c> pada Data Tagihan. Dipetakan ke FinReceivable.DebtorType.</summary>
public static class BillingDataCategories
{
    /// <summary>
    /// Perusahaan/Asuransi → PAYER pihak luar (IsInternalBenefit = false). Porsi Benefit Karyawan RS TIDAK termasuk di sini:
    /// itu biaya rumah sakit sendiri, bukan tagihan ke pihak luar (lihat <see cref="InternalBenefit"/>).
    /// </summary>
    public const string Company = "company";

    /// <summary>
    /// Karyawan → EMPLOYEE_BENEFIT: kewajiban pribadi karyawan yang secara eksplisit ditangguhkan menjadi AR Karyawan saat
    /// finalisasi Benefit RS. Kosong bila tidak ada penangguhan; itu keadaan yang sah, bukan jalur yang belum tersedia.
    /// </summary>
    public const string Employee = "employee";

    /// <summary>
    /// Benefit RS Internal → PAYER dengan IsInternalBenefit = true: porsi biaya yang ditanggung rumah sakit lewat program
    /// Benefit Karyawan RS. Diselesaikan lewat Penutupan Benefit (Finance Benefit Settlement), bukan penerimaan uang.
    /// </summary>
    public const string InternalBenefit = "internalBenefit";

    /// <summary>Pasien Umum → PATIENT_GUARANTOR.</summary>
    public const string GeneralPatient = "generalPatient";
}

public static class BillingDataStatuses
{
    public const string All = "all";
    public const string NotCreated = "notCreated";
    public const string Created = "created";
}

/// <summary>
/// Status penyelesaian piutang menurut DATA SUMBERNYA, bukan menurut keanggotaan Batch Tagihan.
/// Opsional dan additif: <c>billingStatus</c> (created/notCreated) tetap bermakna keanggotaan Batch Tagihan.
/// Perusahaan/Asuransi memakai <see cref="Invoiced"/>/<see cref="AwaitingInvoice"/>; Benefit RS Internal memakai
/// status Penutupan Benefit; Karyawan memakai status perjanjian angsuran; Pasien Umum memakai
/// <see cref="Paid"/>/<see cref="Outstanding"/>.
/// </summary>
public static class BillingDataResolutionStatuses
{
    public const string All = "all";

    // company
    public const string Invoiced = "invoiced";
    public const string AwaitingInvoice = "awaitingInvoice";

    // internalBenefit
    public const string AwaitingSettlement = "awaitingSettlement";
    public const string SettlementDraft = "settlementDraft";
    public const string Settled = "settled";

    // employee
    public const string NoPlan = "noPlan";
    public const string PlanPending = "planPending";
    public const string PlanApproved = "planApproved";
    public const string Paid = "paid";

    // generalPatient
    public const string Outstanding = "outstanding";
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

    /// <summary>Porsi Benefit RS internal: biaya rumah sakit sendiri, diselesaikan lewat Penutupan Benefit, bukan Batch Tagihan AR.</summary>
    public const string InternalBenefit = "INTERNAL_BENEFIT";
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
    /// <summary>company | employee | generalPatient | internalBenefit.</summary>
    public string? Category { get; set; }

    /// <summary>
    /// Mencari No. Bill, No. Registrasi, nama pasien, No. RM; untuk kategori company juga nama asuransi/penjamin.
    /// Khusus kategori employee: dibatasi hanya mencari nama karyawan dan No. RM karyawan.
    /// </summary>
    [MaxLength(200)] public string? Search { get; set; }

    /// <summary>
    /// Pilihan satu penjamin/karyawan/pasien. Untuk company: Id asuransi (MstInsuranceProvider.Id) ATAU Id perusahaan
    /// penjamin (MstCompanyGuarantor.Id), keduanya dari GET billing-data/payer-options. Untuk employee: Id identitas
    /// karyawan / pemilik benefit (WorkforceProfileId atau MstEmployee.Id) dari GET billing-data/employee-options.
    /// Untuk generalPatient dan internalBenefit: Id pasien (MstPatient.Id).
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// all | notCreated | created. Bermakna keanggotaan Batch Tagihan, jadi hanya berguna pada kategori company.
    /// Untuk kategori lain pakai <see cref="ResolutionStatus"/>.
    /// </summary>
    public string? BillingStatus { get; set; }

    /// <summary>
    /// Opsional. Saringan status penyelesaian menurut data sumber (lihat <see cref="BillingDataResolutionStatuses"/>).
    /// internalBenefit: all | awaitingSettlement | settlementDraft | settled.
    /// employee: all | noPlan | planPending | planApproved | paid.
    /// Kategori lain hanya menerima all. Nilai yang bukan milik kategorinya → 400.
    /// </summary>
    public string? ResolutionStatus { get; set; }

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
    [Range(1, 1000)] public int PageSize { get; set; } = 25;
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

    /// <summary>Nomor identitas/NIP karyawan pasien bila tersedia (kategori employee).</summary>
    public string? EmployeeId { get; set; }

    /// <summary>outpatient | inpatient | emergency | other; null bila kunjungan tidak terbaca.</summary>
    public string? PatientType { get; set; }

    public Guid? InvoiceId { get; set; }
    public Guid? EncounterId { get; set; }
    public Guid? PatientId { get; set; }

    /// <summary>PAYER | PATIENT_GUARANTOR | EMPLOYEE_BENEFIT.</summary>
    public string DebtorType { get; set; } = string.Empty;
    public Guid? DebtorReferenceId { get; set; }

    /// <summary>True untuk PAYER porsi Benefit Karyawan RS (biaya rumah sakit sendiri, bukan tagihan ke pihak luar).</summary>
    public bool IsInternalBenefit { get; set; }

    /// <summary>Asal piutang karyawan (EMPLOYEE_BENEFIT): OVER_ALLOWANCE, PATIENT_PORTION, atau MIXED.</summary>
    public string? EmployeeArOrigin { get; set; }

    /// <summary>Nama dan nomor pegawai pemilik benefit (identitas canonical: WorkforceProfileId), bukan nama pasien.</summary>
    public string? BenefitOwnerName { get; set; }
    public string? BenefitOwnerStaffNumber { get; set; }

    /// <summary>Nama penjamin: asuransi, perusahaan penjamin, atau nama penjamin pada kunjungan; nama pasien untuk kategori lain.</summary>
    public string? PayerName { get; set; }

    /// <summary>Id penjamin yang dipakai saringan EntityId (Id asuransi atau Id perusahaan). Hanya untuk kategori company.</summary>
    public Guid? PayerId { get; set; }

    /// <summary>insurance | company; null bila penjamin tidak dapat ditentukan atau bukan kategori company.</summary>
    public string? PayerKind { get; set; }

    /// <summary>Tanggal masuk / registrasi kunjungan (RegPatientEncounter.EncounterDate / RegisteredAt).</summary>
    public DateTimeOffset? EncounterStartAt { get; set; }

    /// <summary>Tanggal keluar / selesai pelayanan kunjungan (RegPatientEncounter.CompletedAt). Null bila belum selesai.</summary>
    public DateTimeOffset? EncounterEndAt { get; set; }

    /// <summary>Nominal awal piutang (FinReceivable.OriginalAmount).</summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>Sisa saldo piutang (FinReceivable.OutstandingAmount).</summary>
    public decimal OutstandingAmount { get; set; }

    /// <summary>Tanggal jatuh tempo piutang (FinReceivable.DueDate).</summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>Jumlah tagihan = nominal rincian piutang, yaitu nominal yang masuk ke Finance/AR (sudah dihitung Billing).</summary>
    public decimal BillingAmount { get; set; }
    public string ReceivableStatus { get; set; } = string.Empty;

    /// <summary>created | notCreated. Keanggotaan Batch Tagihan; hanya bermakna bila <see cref="BillingStatusApplicable"/> true.</summary>
    public string BillingStatus { get; set; } = BillingDataStatuses.NotCreated;

    /// <summary>
    /// True hanya untuk PAYER pihak luar (Perusahaan/Asuransi), satu-satunya jenis yang boleh punya Batch Tagihan.
    /// False berarti <see cref="BillingStatus"/> notCreated BUKAN pekerjaan yang tertunda: Benefit RS Internal diselesaikan
    /// lewat Penutupan Benefit, Karyawan lewat perjanjian angsuran, Pasien Umum lewat penerimaan.
    /// </summary>
    public bool BillingStatusApplicable { get; set; }

    /// <summary>Status penyelesaian menurut data sumber; lihat <see cref="BillingDataResolutionStatuses"/>.</summary>
    public string ResolutionStatus { get; set; } = string.Empty;

    /// <summary>Waktu pengakuan piutang. Dasar pemilihan periode Penutupan Benefit (bukan tanggal invoice).</summary>
    public DateTimeOffset RecognizedAt { get; set; }

    public Guid? InvoiceBatchId { get; set; }
    public string? InvoiceBatchNumber { get; set; }
    public string? InvoiceBatchStatus { get; set; }

    // --- Benefit RS Internal (IsInternalBenefit): Penutupan Benefit yang memuat piutang ini, bukan yang DIBATALKAN ---
    public Guid? SettlementId { get; set; }
    public string? SettlementNumber { get; set; }

    /// <summary>DRAF | DITERBITKAN; null bila belum masuk Penutupan Benefit mana pun.</summary>
    public string? SettlementStatus { get; set; }

    /// <summary>
    /// Kode periode (yyyy-MM) untuk menyiapkan Penutupan Benefit dari piutang ini. Dihitung dari <see cref="RecognizedAt"/>
    /// menurut batas bulan UTC, persis seperti pemilihan periode FinanceBenefitSettlementService.ParsePeriod.
    /// Hanya terisi untuk Benefit RS Internal.
    /// </summary>
    public string? SettlementPeriodCode { get; set; }

    // --- Karyawan (EMPLOYEE_BENEFIT): perjanjian angsuran terkini (MENUNGGU, DISETUJUI, atau SELESAI) ---
    public Guid? InstallmentPlanId { get; set; }
    public string? InstallmentPlanNumber { get; set; }
    public string? InstallmentPlanStatus { get; set; }

    public bool CanCreateInvoice { get; set; }

    /// <summary>Lihat <see cref="BillingDataCreateBlockedReasons"/>; null bila CanCreateInvoice true.</summary>
    public string? CreateBlockedReason { get; set; }

    // --- Alias Kompatibilitas UI / V1 ---
    /// <summary>Alias kompatibilitas UI untuk Id piutang/tagihan.</summary>
    public Guid BillingId => Id;

    /// <summary>Alias kompatibilitas UI untuk BillingDate.</summary>
    public DateTimeOffset Tanggal => BillingDate;

    /// <summary>Alias kompatibilitas UI untuk tanggal masuk.</summary>
    public DateTimeOffset? TanggalMasuk => EncounterStartAt ?? BillingDate;

    /// <summary>Alias kompatibilitas UI untuk tanggal keluar.</summary>
    public DateTimeOffset? TanggalKeluar => EncounterEndAt;

    /// <summary>Alias kompatibilitas UI untuk InvoiceNumber.</summary>
    public string? NoBill => InvoiceNumber;

    /// <summary>Alias kompatibilitas UI untuk EncounterNumber.</summary>
    public string? NoRegistrasi => EncounterNumber;

    /// <summary>Alias kompatibilitas UI untuk MedicalRecordNumber.</summary>
    public string? NoRM => MedicalRecordNumber;

    /// <summary>Alias kompatibilitas UI untuk PatientName.</summary>
    public string? NamaPasien => PatientName;

    /// <summary>Alias kompatibilitas UI untuk EmployeeId.</summary>
    public string? IdKaryawan => EmployeeId;

    /// <summary>Alias kompatibilitas UI untuk BillingAmount.</summary>
    public decimal JumlahTagihan => BillingAmount;

    /// <summary>Alias kompatibilitas UI untuk OriginalAmount.</summary>
    public decimal TotalPiutang => OriginalAmount != 0 ? OriginalAmount : BillingAmount;

    /// <summary>Alias kompatibilitas UI untuk OutstandingAmount.</summary>
    public decimal SisaPiutang => OutstandingAmount;
}

/// <summary>Metadata kelompok tagihan (misal No. RM dan ID Karyawan untuk employee/pasien).</summary>
public sealed class BillingDataGroupMeta
{
    public string? NoRM { get; set; }
    public string? EmployeeId { get; set; }

    /// <summary>Pemilik benefit (kategori employee dan internalBenefit): pegawai pemegang plafon, bukan pasien penerima manfaat.</summary>
    public string? BenefitOwnerName { get; set; }
    public string? BenefitOwnerStaffNumber { get; set; }
}

/// <summary>Subtotal billing amount per tanggal kalender di dalam kelompok yang sama.</summary>
public sealed class BillingDataDateTotalResponse
{
    /// <summary>Format tanggal: yyyy-MM-dd.</summary>
    public string Date { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

/// <summary>Satu kelompok baris tagihan pada Data Tagihan (perusahaan, karyawan, atau pasien umum).</summary>
public sealed class BillingDataGroupResponse
{
    /// <summary>Identifier stabil grup (PayerId GUID string / EmployeeId / PatientId GUID string).</summary>
    public string GroupId { get; set; } = string.Empty;

    /// <summary>Nama kelompok untuk tampilan UI (nama perusahaan/asuransi, nama karyawan, atau nama pasien).</summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>Metadata tambahan untuk header grup.</summary>
    public BillingDataGroupMeta GroupMeta { get; set; } = new();

    /// <summary>Daftar tagihan yang termasuk ke dalam grup ini pada halaman aktif.</summary>
    public List<BillingDataItemResponse> Items { get; set; } = new();

    /// <summary>Subtotal per tanggal di dalam grup ini.</summary>
    public List<BillingDataDateTotalResponse> DateTotals { get; set; } = new();

    /// <summary>Total nominal tagihan grup dari SELURUH DATA FILTERED (bukan hanya halaman aktif).</summary>
    public decimal GroupTotal { get; set; }

    /// <summary>Jumlah transaksi tagihan grup dari seluruh data filtered.</summary>
    public int ItemCount { get; set; }

    /// <summary>Jumlah pasien unik pada grup ini dari seluruh data filtered.</summary>
    public int PatientCount { get; set; }
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

/// <summary>Parameter GET /receivables/billing-data/employee-options (isi pilihan karyawan pada Piutang Karyawan).</summary>
public sealed class BillingDataEmployeeOptionQuery
{
    [MaxLength(200)] public string? Search { get; set; }

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;

    [Range(1, 100)] public int PageSize { get; set; } = 25;

    public int? Limit { get; set; }
}

/// <summary>Satu pilihan karyawan yang terkait piutang EMPLOYEE_BENEFIT.</summary>
public sealed class BillingDataEmployeeOptionResponse
{
    /// <summary>Id penanda identitas karyawan (WorkforceProfileId).</summary>
    public Guid Id { get; set; }

    /// <summary>Nama karyawan.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>No. RM karyawan bila terverifikasi; null bila karyawan belum pernah memiliki rekam medis.</summary>
    public string? MedicalRecordNumber { get; set; }

    /// <summary>Alias kompatibilitas UI untuk nama karyawan.</summary>
    public string Label => Name;

    /// <summary>Keterangan No. RM untuk baris kedua dropdown ("No. RM xx").</summary>
    public string? Description => !string.IsNullOrWhiteSpace(MedicalRecordNumber) ? $"No. RM {MedicalRecordNumber}" : null;
}

public sealed class BillingDataPagedResponse : PagedResult<BillingDataItemResponse>
{
    /// <summary>Daftar grup tagihan dengan subtotal tanggal dan subtotal final grup.</summary>
    public List<BillingDataGroupResponse> Groups { get; set; } = new();

    public BillingDataSummaryResponse Summary { get; set; } = new();

    /// <summary>Penjelasan bila hasil kosong karena sumber datanya memang belum ada (kategori employee). Null pada kasus lain.</summary>
    public string? Notice { get; set; }

    /// <summary>
    /// Fakta AR dari Billing yang belum menjadi piutang. Hanya dibaca (hitungan), TIDAK memicu pemrosesan apa pun.
    /// Pemrosesannya lewat POST billing-intake/sync dan POST billing-intake/process-pending-ar.
    /// </summary>
    public PendingArFactsResponse PendingFacts { get; set; } = new();
}
