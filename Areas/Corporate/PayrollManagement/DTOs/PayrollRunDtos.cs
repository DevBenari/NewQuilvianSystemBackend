using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class CreatePayrollRunRequest
{
    [Required] public Guid PayrollPeriodId { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
}

public sealed class PayrollRunDecisionRequest
{
    [MaxLength(2000)] public string? Reason { get; set; }

    /// <summary>Token versi run yang dilihat klien (<c>rowVersion</c>). Kosong = tanpa pemeriksaan; berbeda = 409.</summary>
    [MaxLength(40)] public string? ExpectedRowVersion { get; set; }
}

/// <summary>Badan opsional untuk aksi run tanpa alasan (hitung, ajukan, sahkan, tutup).</summary>
public sealed class PayrollRunActionRequest
{
    /// <summary>Token versi run yang dilihat klien (<c>rowVersion</c>). Kosong = tanpa pemeriksaan; berbeda = 409.</summary>
    [MaxLength(40)] public string? ExpectedRowVersion { get; set; }
}

/// <summary>Mode pengajuan run. Normal = semua penahan harus beres; ReviewOnly = hanya histori PPh yang belum lengkap (PPh sementara).</summary>
public static class PayrollRunSubmissionModes
{
    public const string Normal = "Normal";
    public const string ReviewOnly = "ReviewOnly";
}

/// <summary>
/// PAY-BE-FIX-034: tahap keputusan persetujuan yang eksplisit dan konsisten di daftar maupun detail, diturunkan dari data yang sudah
/// tersimpan (status run + jejak pengesahan bersyarat pada ValidationSummaryJson); TANPA kolom/status baru dan tanpa migration. Tahap
/// <see cref="AdministrativeApprovedTaxPending"/> BUKAN Approved: RunStatus tetap Submitted, sehingga seluruh konsumen hilir (Finance Handoff,
/// konfirmasi potongan AR, instruksi pembayaran, slip final, laporan PPh resmi) tidak membukanya.
/// </summary>
public static class PayrollRunApprovalStages
{
    /// <summary>Bukan tahap persetujuan (belum diajukan atau sudah dibatalkan).</summary>
    public const string None = "NONE";

    /// <summary>Diajukan, menunggu keputusan persetujuan.</summary>
    public const string AwaitingDecision = "AWAITING_DECISION";

    /// <summary>Disetujui administratif, PPh BELUM FINAL: menunggu finalisasi PPh. Bukan pengesahan final dan bukan siap dibayar.</summary>
    public const string AdministrativeApprovedTaxPending = "ADMINISTRATIVE_APPROVED_TAX_PENDING";

    /// <summary>Pengesahan final (Approved dan sesudahnya).</summary>
    public const string FinalApproved = "FINAL_APPROVED";
}

/// <summary>
/// PAY-BE-FIX-033: langkah berikutnya menuju pengesahan FINAL untuk run Submitted. Informasi tampilan; endpoint mutasi tetap memeriksa ulang
/// seluruh syarat. Klien memilih tampilan dari kode ini, bukan dari teks pesan.
/// </summary>
public static class PayrollRunApprovalNextSteps
{
    /// <summary>Bukan run Submitted (atau tidak ada langkah persetujuan).</summary>
    public const string None = "NONE";

    /// <summary>Seluruh syarat terpenuhi: run dapat disahkan final (izin PayrollRun:Approve tetap diperiksa terpisah).</summary>
    public const string ApproveFinal = "APPROVE_FINAL";

    /// <summary>PPh sementara dan run layak disahkan bersyarat (administratif); belum disahkan bersyarat.</summary>
    public const string ApproveConditionally = "APPROVE_CONDITIONALLY";

    /// <summary>
    /// PPh belum final (run review-only), baik sudah maupun belum disahkan bersyarat: histori/penetapan PPh harus diselesaikan secara sah,
    /// run dikembalikan, dihitung ulang, diajukan ulang, lalu disahkan normal. Estimasi tidak pernah menjadi PPh resmi.
    /// </summary>
    public const string ResolveTaxData = "RESOLVE_TAX_DATA";

    /// <summary>Ada penahan selain PPh sementara (penerima bermasalah, sumber berubah, dst.).</summary>
    public const string ResolveOtherBlockers = "RESOLVE_OTHER_BLOCKERS";

    /// <summary>
    /// PAY-BE-FIX-038: PPh masih sementara pada run ini, tetapi histori yang kurang SUDAH dilengkapi Saldo Awal PPh yang disetujui. Yang tersisa
    /// hanya hitung ulang: kembalikan run, hitung ulang, ajukan kembali, lalu sahkan (hasil lama beku, tidak berubah sendiri).
    /// </summary>
    public const string RecalculateAfterTaxHistory = "RECALCULATE_AFTER_TAX_HISTORY";
}

/// <summary>Kode alasan pengajuan review-only belum dapat dilakukan (PAY-BE-FIX-031). Keputusan klien memakai kode, bukan teks pesan.</summary>
public static class PayrollRunReviewBlockCodes
{
    /// <summary>Run dihitung sebelum klasifikasi penahan PPh tersedia; hitung ulang (endpoint Calculate) sebelum dapat diajukan untuk review.</summary>
    public const string RecalculationRequired = "RECALCULATION_REQUIRED_FOR_TAX_REVIEW";

    /// <summary>Ada penahan selain histori PPh yang belum lengkap.</summary>
    public const string OtherBlockers = "OTHER_BLOCKERS";

    /// <summary>Estimasi PPh sementara tidak tersedia untuk sebagian penerima terdampak.</summary>
    public const string EstimateUnavailable = "ESTIMATE_UNAVAILABLE";

    /// <summary>Alasan lain: penerima bermasalah, belum ada yang terhitung, input manual, atau sumber kompensasi berubah.</summary>
    public const string OtherReasons = "OTHER_REASONS";
}

/// <summary>Mode pengesahan run. Normal = pengesahan final (perilaku lama); Conditional = pengesahan bersyarat, PPh belum final (PAY-BE-FIX-032).</summary>
public static class PayrollRunApprovalModes
{
    public const string Normal = "Normal";
    public const string Conditional = "Conditional";
}

/// <summary>
/// Badan aksi Approve (PAY-BE-FIX-032). Tanpa badan atau tanpa <see cref="ApprovalMode"/> = pengesahan normal (perilaku lama). Mode
/// <c>Conditional</c> memakai izin PayrollRun:Approve yang sama dan HANYA mencatat persetujuan administratif: status run tetap Submitted,
/// tidak ada pengunci, konfirmasi potongan ke Finance, pembekuan snapshot, atau pembayaran.
/// </summary>
public sealed class PayrollRunApproveRequest
{
    /// <summary>Token versi run yang dilihat klien (<c>rowVersion</c>). Kosong = tanpa pemeriksaan; berbeda = 409.</summary>
    [MaxLength(40)] public string? ExpectedRowVersion { get; set; }

    /// <summary><c>Normal</c> (bawaan) atau <c>Conditional</c>.</summary>
    [MaxLength(20)] public string? ApprovalMode { get; set; }

    /// <summary>Alasan pengesahan bersyarat; wajib pada mode Conditional.</summary>
    [MaxLength(1500)] public string? Reason { get; set; }

    /// <summary>Pernyataan bahwa sebagian PPh belum final dan pengesahan ini tidak mengesahkan pajak, posting Finance, atau pembayaran.</summary>
    public bool AcknowledgeProvisionalTax { get; set; }
}

/// <summary>
/// Badan aksi Submit (PAY-BE-FIX-030). Tanpa badan atau tanpa <see cref="SubmissionMode"/> = pengajuan normal (perilaku lama). Mode
/// <c>ReviewOnly</c> memakai izin PayrollRun:Submit yang sama dan diperiksa ulang penuh oleh backend; bukan jalan pintas pengesahan.
/// </summary>
public sealed class PayrollRunSubmitRequest
{
    /// <summary>Token versi run yang dilihat klien (<c>rowVersion</c>). Kosong = tanpa pemeriksaan; berbeda = 409.</summary>
    [MaxLength(40)] public string? ExpectedRowVersion { get; set; }

    /// <summary><c>Normal</c> (bawaan) atau <c>ReviewOnly</c>.</summary>
    [MaxLength(20)] public string? SubmissionMode { get; set; }

    /// <summary>Alasan pengajuan untuk review dengan PPh sementara; wajib pada mode ReviewOnly.</summary>
    [MaxLength(1500)] public string? Reason { get; set; }

    /// <summary>Pernyataan pengaju bahwa PPh masih estimasi dan belum final; wajib true pada mode ReviewOnly.</summary>
    public bool AcknowledgeProvisionalTax { get; set; }
}

public sealed class PayrollRunQuery
{
    public Guid? PayrollPeriodId { get; set; }

    /// <summary>Satu status run, atau beberapa status dipisah koma (mis. <c>Draft,CollectingInput,Calculated</c>).</summary>
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PayrollRunListItemResponse
{
    public Guid Id { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public Guid PayrollPeriodId { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string RunType { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public int TotalEmployeeCount { get; set; }
    public int ErrorEmployeeCount { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal TotalNetPay { get; set; }
    public DateTime? CalculatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// PAY-BE-FIX-030: true bila run diajukan untuk review dengan PPh sementara (belum final). Run seperti ini tidak dapat disahkan sampai
    /// dikembalikan, dihitung ulang, dan diajukan ulang. Aditif.
    /// </summary>
    public bool IsTaxProvisional { get; set; }

    /// <summary>BELUM_FINAL bila <see cref="IsTaxProvisional"/>; null selain itu. Aditif.</summary>
    public string? TaxFinalStatus { get; set; }

    /// <summary>
    /// PAY-BE-FIX-034: tahap keputusan persetujuan (<see cref="PayrollRunApprovalStages"/>), sama pada daftar dan detail. Diturunkan dari
    /// data tersimpan; BUKAN status run baru. Aditif.
    /// </summary>
    public string ApprovalStage { get; set; } = PayrollRunApprovalStages.None;

    /// <summary>Total PPh sementara (estimasi) saat diajukan untuk review; null bila bukan pengajuan review. BUKAN TotalTax resmi. Aditif.</summary>
    public decimal? ProvisionalTaxTotal { get; set; }

    /// <summary>PAY-BE-FIX-032: true bila run sudah disahkan bersyarat (administratif; PPh tetap BELUM FINAL, status run tetap Submitted). Aditif.</summary>
    public bool IsConditionallyApproved { get; set; }

    /// <summary>Referensi pengesahan bersyarat (mis. CA-PR-202612-XXXX-1); null bila belum. Aditif.</summary>
    public string? ConditionalApprovalReference { get; set; }

    /// <summary>
    /// Token konkurensi optimistik turunan (tick waktu pembaruan). Kirim kembali sebagai <c>expectedRowVersion</c> pada aksi run
    /// agar aksi atas data yang sudah berubah ditolak dengan 409.
    /// </summary>
    public string RowVersion { get; set; } = string.Empty;
}

/// <summary>Potongan cicilan piutang RS pada satu slip: rencana dari hitung terakhir dan status konfirmasi ke Finance.</summary>
public sealed class PayrollRunInstallmentLineResponse
{
    public Guid InstallmentId { get; set; }
    public string? DeductionPeriod { get; set; }

    /// <summary>Nominal sesuai perjanjian angsuran (terjadwal ditambah tunggakan terbawa).</summary>
    public decimal DueAmount { get; set; }

    /// <summary>Gaji bersih yang tersedia untuk cicilan setelah pajak, iuran, dan potongan lain (dasar batas potong).</summary>
    public decimal? AvailableNetSalary { get; set; }
    public decimal? MaxAllowedDeduction { get; set; }
    public decimal? PlannedDeduction { get; set; }

    /// <summary>BERHASIL | SEBAGIAN | GAGAL (rencana). Kosong bila belum dihitung.</summary>
    public string? PlannedStatus { get; set; }
    public bool IsCalculated { get; set; }

    /// <summary>True setelah run disahkan dan hasilnya diterima Finance (saldo piutang baru berkurang pada titik ini).</summary>
    public bool IsConfirmedToFinance { get; set; }

    /// <summary>Nominal terpotong saat ini menurut Finance (setelah konfirmasi dan pembalikan). Kosong bila belum dikonfirmasi.</summary>
    public decimal? FinanceDeductedAmount { get; set; }

    // ---- Rincian perjanjian dan angsuran (additive; hanya terisi pada detail penerima, dibaca lewat satu join, bukan N+1) ----

    public Guid? PlanId { get; set; }

    /// <summary>Nomor perjanjian angsuran. Nomor piutang dan isi dokumen perjanjian TIDAK dikirim ke endpoint detail umum.</summary>
    public string? PlanNumber { get; set; }
    public int? InstallmentNumber { get; set; }

    /// <summary>MENUNGGU | DISETUJUI | DITOLAK | SELESAI | DIBATALKAN (status perjanjian pada Finance).</summary>
    public string? AgreementStatus { get; set; }
    public DateTimeOffset? AgreementApprovedAt { get; set; }

    /// <summary>Ada jalur berkas perjanjian yang tercatat. Jalur itu sendiri tidak dikirim.</summary>
    public bool HasAgreementDocument { get; set; }

    /// <summary>
    /// DocumentRecorded (jalur berkas tercatat; isi dan tanda tangannya BELUM diverifikasi sistem) | DocumentMissing (tidak ada jalur berkas).
    /// Sistem tidak punya penanda bukti otorisasi pemotongan yang terverifikasi, jadi nilai ini bukan bukti tanda tangan.
    /// </summary>
    public string? AuthorizationEvidenceStatus { get; set; }

    public decimal? ScheduledAmount { get; set; }
    public decimal? CarriedOverAmount { get; set; }

    /// <summary>Status angsuran pada Finance: DIJADWALKAN | TERBAYAR_SEBAGIAN | TERBAYAR | TERTUNGGAK | DIBATALKAN.</summary>
    public string? InstallmentStatus { get; set; }

    /// <summary>Sisa belum terpotong: jatuh tempo dikurangi rencana potongan sebelum konfirmasi, atau sisa menurut Finance sesudahnya.</summary>
    public decimal? RemainingUnpaidAmount { get; set; }

    /// <summary>Batas potongan yang masih tersedia untuk angsuran ini setelah angsuran sebelumnya pada urutan alokasi (batas kumulatif per penerima).</summary>
    public decimal? CapAvailableForInstallment { get; set; }

    /// <summary>MasterDeductionType (persen dari master jenis potongan) | PolicyDefault (nilai bawaan kebijakan yang belum disahkan).</summary>
    public string? MaxAllowedSource { get; set; }

    /// <summary>Urutan alokasi batas potongan di antara angsuran penerima ini (1 = pertama). Urutan sementara, bukan kebijakan yang disahkan.</summary>
    public int? AllocationOrder { get; set; }

    /// <summary>Angsuran sudah ditarik dari Finance ke run sebagai input; baris ini tidak pernah dibuat dari saldo piutang.</summary>
    public string SyncStatus { get; set; } = "Synced";

    /// <summary>Catatan kepatuhan atau konfigurasi pada angsuran ini.</summary>
    public List<string> Notes { get; set; } = new();
}

public class PayrollRunEmployeeResponse
{
    public Guid Id { get; set; }
    public Guid WorkforceProfileId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeePayrollStatus { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal TotalRecurringEarning { get; set; }
    public decimal TotalVariableEarning { get; set; }
    public decimal TotalOvertimeAmount { get; set; }
    public decimal TotalAttendanceAllowance { get; set; }
    public decimal GrossPay { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalEmployeeInsuranceContribution { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetPay { get; set; }
    public string? ValidationMessage { get; set; }
    public PayrollRunInstallmentLineResponse? Installment { get; set; }

    /// <summary>Total pembalikan potongan cicilan yang sudah disetujui untuk pegawai ini.</summary>
    public decimal ReversedAmount { get; set; }
    public bool HasPendingReversal { get; set; }

    /// <summary>
    /// PAY-BE-FIX-030: PPh sementara penerima ini (hanya bila penahannya histori PPh yang belum lengkap). Terpisah dari TotalTax/NetPay resmi.
    /// Null pada penerima lain. Aditif.
    /// </summary>
    public PayrollRunProvisionalTaxResponse? ProvisionalTax { get; set; }

    /// <summary>
    /// PAY-BE-FIX-032: FINAL_VALID | PROVISIONAL_ESTIMATE | INCOMPLETE_NO_ESTIMATE | NOT_APPLICABLE. Kosong bila penerima belum dihitung.
    /// Nilai yang tidak diketahui bukan Rp0: lihat <see cref="OfficialTaxAmount"/>. Aditif.
    /// </summary>
    public string TaxFinalityStatus { get; set; } = string.Empty;

    /// <summary>PPh resmi penerima hanya bila FINAL_VALID (termasuk VALID_ZERO = 0 yang sah); null bila belum final. Aditif.</summary>
    public decimal? OfficialTaxAmount { get; set; }
}

/// <summary>Jejak pengesahan bersyarat (administratif) dari penanda tersimpan pada run. PPh tetap BELUM FINAL.</summary>
public sealed class PayrollRunConditionalApprovalResponse
{
    public string Reference { get; set; } = string.Empty;

    /// <summary>CONDITIONAL.</summary>
    public string ApprovalMode { get; set; } = "CONDITIONAL";
    public DateTime ApprovedAt { get; set; }
    public Guid ApprovedByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int AffectedEmployeeCount { get; set; }

    /// <summary>Jumlah PPh resmi (FINAL_VALID) penerima pada run saat pengesahan bersyarat.</summary>
    public decimal ConfirmedTaxTotal { get; set; }

    /// <summary>Jumlah estimasi PPh sementara; BUKAN kewajiban pajak.</summary>
    public decimal ProvisionalTaxTotal { get; set; }
    public string? PolicyVersion { get; set; }
    public List<int> MissingTaxMonths { get; set; } = new();
    public bool FollowUpRequired { get; set; }

    /// <summary>PENDING_HISTORY_COMPLETION sampai run dikembalikan, dihitung ulang dengan histori lengkap, dan diajukan ulang.</summary>
    public string ReconciliationStatus { get; set; } = string.Empty;
    public string TaxFinalStatus { get; set; } = "BELUM_FINAL";
}

/// <summary>
/// Ringkasan PPh run untuk Finance (PAY-BE-FIX-032). BACA-SAJA dan INFORMATIF: bukan jurnal, bukan utang pajak, bukan instruksi potong.
/// Nominal estimasi tidak pernah menjadi kewajiban final. Belum ada consumer Finance yang terverifikasi; tidak ada pengiriman otomatis.
/// </summary>
public sealed class PayrollRunTaxSummaryResponse
{
    public Guid RunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string PeriodCode { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;

    /// <summary>FINAL bila seluruh penerima FINAL_VALID/NOT_APPLICABLE dan run disahkan final; BELUM_FINAL selain itu.</summary>
    public string TaxFinalStatus { get; set; } = "BELUM_FINAL";

    /// <summary>INFORMATIONAL_ONLY. Posting kewajiban PPh hanya lewat kontrak Finance/Accounting yang belum disepakati.</summary>
    public string UsageMode { get; set; } = "INFORMATIONAL_ONLY";

    /// <summary>Selalu false pada versi ini: tidak ada kontrak posting yang disetujui.</summary>
    public bool IsPostingEligible { get; set; }
    public string UsageNotice { get; set; } = string.Empty;
    public string? ConditionalApprovalReference { get; set; }
    public string ReconciliationStatus { get; set; } = string.Empty;
    public int AffectedEmployeeCount { get; set; }
    public decimal ConfirmedTaxTotal { get; set; }
    public decimal ProvisionalTaxTotal { get; set; }
    public List<string> PolicyVersions { get; set; } = new();
    public List<int> MissingTaxMonths { get; set; } = new();
    public List<PayrollRunTaxSummaryEmployeeResponse> Employees { get; set; } = new();
}

public sealed class PayrollRunTaxSummaryEmployeeResponse
{
    public Guid WorkforceProfileId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int? TaxYear { get; set; }
    public int? TaxMonth { get; set; }
    public string? TaxArticle { get; set; }
    public string? CalculationMode { get; set; }
    public string TaxFinalityStatus { get; set; } = string.Empty;
    public string? ResultStatus { get; set; }

    /// <summary>PPh resmi hanya bila FINAL_VALID; null bila belum final (bukan Rp0).</summary>
    public decimal? ConfirmedTaxAmount { get; set; }

    /// <summary>Estimasi proksi bila tersedia; BUKAN kewajiban pajak.</summary>
    public decimal? ProvisionalTaxAmount { get; set; }
    public string? ProvisionalMethod { get; set; }
    public string? PolicyVersion { get; set; }
    public List<int> MissingTaxMonths { get; set; } = new();
    public string? Explanation { get; set; }
}

/// <summary>
/// PPh SEMENTARA satu penerima. Bukan hasil pajak resmi: tidak masuk TotalTax, tidak menjadi dasar potong/lapor/Finance. Bila
/// <see cref="IsAvailable"/> false tampilkan "Estimasi Tidak Tersedia" beserta <see cref="UnavailableReason"/>, jangan Rp0.
/// </summary>
public sealed class PayrollRunProvisionalTaxResponse
{
    public bool IsAvailable { get; set; }
    public decimal? ProvisionalTaxAmount { get; set; }

    /// <summary>Gaji bersih resmi dikurangi PPh sementara (bila dipotong dari pegawai). Tampilkan dengan label "Estimasi Gaji Bersih".</summary>
    public decimal? EstimatedNetPay { get; set; }

    /// <summary>Gaji bersih resmi saat ini (PPh resmi masih 0 karena penahan); hanya pembanding.</summary>
    public decimal OfficialNetPay { get; set; }

    /// <summary>TER_MONTHLY_PROXY.</summary>
    public string Method { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? PolicyVersion { get; set; }
    public string? PolicyLegalBasis { get; set; }
    public decimal? BasisGross { get; set; }
    public string? TerCategory { get; set; }
    public decimal? TerRatePercent { get; set; }
    public decimal NoNpwpSurchargePercent { get; set; }
    public List<int> MissingTaxMonths { get; set; } = new();

    /// <summary>Selalu BELUM_FINAL.</summary>
    public string TaxFinalStatus { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string? UnavailableReason { get; set; }
}

/// <summary>Ringkasan PPh sementara pada tingkat run (hanya penerima yang penahannya histori PPh belum lengkap).</summary>
public sealed class PayrollRunProvisionalTaxSummaryResponse
{
    /// <summary>TAX_YTD_HISTORY_INCOMPLETE.</summary>
    public string BlockCode { get; set; } = string.Empty;
    public int AffectedEmployeeCount { get; set; }
    public int TaxBlockerCount { get; set; }
    public int EstimateAvailableCount { get; set; }
    public int EstimateUnavailableCount { get; set; }

    /// <summary>Jumlah PPh sementara penerima yang estimasinya tersedia. BUKAN TotalTax resmi.</summary>
    public decimal TotalProvisionalTax { get; set; }

    /// <summary>PAY-BE-FIX-032: jumlah PPh resmi (FINAL_VALID) seluruh penerima run; null-safe 0 bila tidak ada. Dihitung backend, bukan klien. Aditif.</summary>
    public decimal ConfirmedTaxTotal { get; set; }
    public decimal TotalEstimatedNetPay { get; set; }
    public string Method { get; set; } = string.Empty;
    public List<string> PolicyVersions { get; set; } = new();
    public List<int> MissingTaxMonths { get; set; } = new();
    public string TaxFinalStatus { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string NetPayNotice { get; set; } = string.Empty;
}

/// <summary>Jejak pengajuan untuk review dengan PPh sementara (dari penanda tersimpan pada run).</summary>
public sealed class PayrollRunReviewOnlyResponse
{
    public DateTime SubmittedAt { get; set; }
    public Guid SubmittedByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int AffectedEmployeeCount { get; set; }
    public decimal TotalProvisionalTax { get; set; }
    public string? PolicyVersion { get; set; }
    public List<int> MissingTaxMonths { get; set; } = new();
    public string BlockCode { get; set; } = string.Empty;
    public string TaxFinalStatus { get; set; } = string.Empty;
}

/// <summary>Satu baris komponen penghasilan/potongan pada slip penerima (gross-to-net yang dapat ditelusuri).</summary>
public sealed class PayrollRunComponentLineResponse
{
    public Guid Id { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>Earning | Deduction | Tax | Insurance.</summary>
    public string ComponentType { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public bool IsProrated { get; set; }
    public bool IsManual { get; set; }
    public string? Formula { get; set; }
    public int SortOrder { get; set; }

    // Additive: penelusuran sumber dan metode hitung komponen.
    public Guid? SourceId { get; set; }
    public string? CalculationMethod { get; set; }
}

/// <summary>
/// Hasil PPh satu penerima dari snapshot hitung. ResultStatus: NOT_CONFIGURED | CALCULATED | VALID_ZERO | EXEMPT | NOT_APPLICABLE |
/// ERROR. VALID_ZERO, EXEMPT, dan NOT_APPLICABLE adalah nol yang sah; NOT_CONFIGURED dan ERROR bukan hasil. Tanpa NPWP.
/// </summary>
public sealed class PayrollRunTaxResultResponse
{
    /// <summary>HRD_PROFILE | LEGACY_PROFILE_AMOUNT | NONE.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>NOT_CONFIGURED | CONFIGURED | NOT_EFFECTIVE | ERROR.</summary>
    public string ConfigurationStatus { get; set; } = string.Empty;
    public string ResultStatus { get; set; } = string.Empty;

    /// <summary>PPH21 | PPH26.</summary>
    public string? Article { get; set; }
    public string? RecipientCategory { get; set; }
    public string? CalculationMode { get; set; }

    /// <summary>Gross | GrossUp | Net.</summary>
    public string? TaxMethod { get; set; }

    /// <summary>Status PTKP; hanya terisi bila relevan (PPh 21 penerima berstatus pegawai).</summary>
    public string? PtkpStatus { get; set; }
    public string? PolicyVersion { get; set; }

    /// <summary>EmployeeDeduction (dipotong dari pegawai) | EmployerBorne (ditanggung pemberi kerja) | None.</summary>
    public string Treatment { get; set; } = string.Empty;
    public decimal EmployeeAmount { get; set; }
    public decimal EmployerBorneAmount { get; set; }
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public string? DecisionBasis { get; set; }
    public string? ExemptionReason { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    // ---- Penelusuran perhitungan otomatis dan persetujuan nominal manual (aditif; null pada run lama). ----

    /// <summary>Kebijakan tarif PPh yang dipakai (id dan dasar peraturan); nomor versinya ada pada PolicyVersion.</summary>
    public Guid? PolicyId { get; set; }
    public string? PolicyLegalBasis { get; set; }

    /// <summary>PENDING | APPROVED | REJECTED; hanya untuk skema nominal manual.</summary>
    public string? ManualAmountApprovalStatus { get; set; }

    public int? TaxYear { get; set; }
    public int? TaxMonth { get; set; }
    public bool IsLastTaxPeriod { get; set; }

    /// <summary>Bruto kena pajak, premi pemberi kerja yang termasuk bruto, dan iuran pensiun pengurang yang menjadi dasar hitung.</summary>
    public decimal? TaxableGross { get; set; }
    public decimal EmployerPremium { get; set; }
    public decimal EmployeePension { get; set; }

    /// <summary>Langkah hitung (kategori TER, tarif, PKP, dst.) sebagai bukti penelusuran; null untuk nominal manual dan pengecualian.</summary>
    public Dictionary<string, object?>? Detail { get; set; }

    /// <summary>PAY-BE-FIX-030: TAX_YTD_HISTORY_INCOMPLETE bila penahan murni histori PPh yang belum lengkap; null selain itu. Aditif.</summary>
    public string? BlockCode { get; set; }

    /// <summary>PAY-BE-FIX-030: PPh sementara, terpisah dari EmployeeAmount di atas (yang tetap resmi). Aditif.</summary>
    public PayrollRunProvisionalTaxResponse? Provisional { get; set; }
}

public sealed class PayrollRunWageBaseItemResponse
{
    public string Source { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    /// <summary>INCLUDED | EXCLUDED | UNCLASSIFIED.</summary>
    public string Classification { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public sealed class PayrollRunInsuranceLineResponse
{
    public string Code { get; set; } = string.Empty;

    /// <summary>KES | JHT | JP | JKK | JKM | PRIVATE | LEGACY.</summary>
    public string Program { get; set; } = string.Empty;

    /// <summary>Employee | Employer.</summary>
    public string PaidBy { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Hasil iuran satu penerima dari snapshot hitung. <see cref="EmployeeTotal"/> mengurangi gaji bersih; <see cref="EmployerTotal"/> adalah
/// kontribusi pemberi kerja dan TIDAK mengurangi gaji bersih.
/// </summary>
public sealed class PayrollRunInsuranceResultResponse
{
    public string Source { get; set; } = string.Empty;
    public string ConfigurationStatus { get; set; } = string.Empty;
    public string ResultStatus { get; set; } = string.Empty;

    /// <summary>ACTIVE | NOT_PARTICIPATING | NOT_APPLICABLE.</summary>
    public string? ParticipationStatus { get; set; }
    public List<string> Programs { get; set; } = new();

    /// <summary>HISTORIS: nomor versi kebijakan iuran BPJS pada snapshot run lama. Kosong pada run baru.</summary>
    public string? PolicyVersion { get; set; }
    public Guid? PolicyId { get; set; }
    public string? PolicyLegalBasis { get; set; }

    /// <summary>HISTORIS: kelompok risiko JKK pada kebijakan BPJS snapshot run lama.</summary>
    public int? JkkRiskClass { get; set; }
    public decimal WageBase { get; set; }

    /// <summary>HISTORIS: dasar upah iuran BPJS per komponen pada snapshot run lama. Kosong pada run baru.</summary>
    public List<PayrollRunWageBaseItemResponse> WageBaseItems { get; set; } = new();
    public decimal EmployeeTotal { get; set; }
    public decimal EmployerTotal { get; set; }
    public List<PayrollRunInsuranceLineResponse> Lines { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>Rincian satu penerima pada run: komponen, agregat kehadiran/lembur, dan identitas pembayaran yang disamarkan.</summary>
public sealed class PayrollRunEmployeeDetailResponse : PayrollRunEmployeeResponse
{
    public int ScheduledWorkMinutes { get; set; }
    public int ActualWorkMinutes { get; set; }
    public int LateMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal AbsentDays { get; set; }
    public string? BankName { get; set; }

    /// <summary>Nomor rekening disamarkan (empat digit terakhir). Nomor lengkap tidak dikirim ke layar.</summary>
    public string? BankAccountMasked { get; set; }
    public string? TaxStatus { get; set; }
    public bool IsSnapshotFrozen { get; set; }
    public List<PayrollRunComponentLineResponse> Components { get; set; } = new();

    // ---- Additive (rincian penghasilan): identitas dan periode ----
    public Guid RunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public bool IsRunLocked { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string PayrollPeriodCode { get; set; } = string.Empty;
    public string PayrollPeriodName { get; set; } = string.Empty;
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }

    // ---- Status proses: membedakan belum dihitung, hasil sah (termasuk nol sah), hasil bermasalah, dan snapshot final ----

    /// <summary>
    /// NotCalculated (run atau penerima belum dihitung; angka gross-to-net BUKAN hasil) | Calculated | DataIncomplete (terhitung, tetapi
    /// konfigurasi pajak/iuran belum lengkap) | Error (perhitungan gagal; angka tidak sah) | SnapshotFrozen (run disahkan, angka final).
    /// </summary>
    public string CalculationState { get; set; } = "NotCalculated";

    /// <summary>True hanya bila penerima ini sudah dihitung dengan berhasil (atau snapshot-nya beku). Nol pada penerima yang IsCalculated adalah hasil sah.</summary>
    public bool IsCalculated { get; set; }

    /// <summary>Perhitungan sudah dicoba untuk penerima ini tetapi gagal; alasannya ada di <see cref="ValidationIssues"/>.</summary>
    public bool CalculationFailed { get; set; }
    public DateTime? CalculatedAt { get; set; }
    public DateTime? SnapshotFrozenAt { get; set; }

    /// <summary>Alasan perhitungan gagal atau penahan penerima ini (dari hasil hitung dan penahan run yang menyebut penerima ini).</summary>
    public List<string> ValidationIssues { get; set; } = new();

    /// <summary>Peringatan yang tidak menahan hasil (mis. rekening belum terverifikasi, tetapan pajak nol).</summary>
    public List<string> CalculationWarnings { get; set; } = new();

    /// <summary>Selisih agregat terhadap komponen penyusunnya (bruto, total potongan, bersih). Kosong bila konsisten.</summary>
    public List<string> ConsistencyIssues { get; set; } = new();

    // ---- Ringkasan penghasilan dan potongan dari komponen hasil hitung ----
    public decimal TotalFixedAllowance { get; set; }
    public decimal TotalTransportAllowance { get; set; }
    public decimal TotalAttendanceDeduction { get; set; }
    public decimal TotalUnpaidLeaveDeduction { get; set; }

    /// <summary>Jumlah rencana potongan cicilan seluruh angsuran penerima ini.</summary>
    public decimal TotalInstallmentDeduction { get; set; }

    /// <summary>Potongan bertipe Deduction di luar kehadiran, cuti tanpa gaji, dan cicilan (variabel/manual/lainnya).</summary>
    public decimal TotalOtherDeduction { get; set; }

    /// <summary>Kontribusi pemberi kerja: informasi saja, TIDAK mengurangi gaji bersih penerima.</summary>
    public decimal TotalEmployerInsuranceContribution { get; set; }

    // ---- Pajak dan iuran ----

    /// <summary>Asal nominal PPh: konfigurasi HRD (Profil Pajak), tetapan lama profil payroll (cadangan bertanda), atau belum ada sumber yang sah.</summary>
    public string TaxCalculationSource { get; set; } = string.Empty;

    /// <summary>
    /// NotCalculated | Configured (konfigurasi PPh dan iuran HRD lengkap) | NotConfigured (ada kekurangan konfigurasi; lihat
    /// <see cref="TaxConfigurationIssues"/>). Tidak diturunkan dari nominal nol. Dipertahankan untuk kompatibilitas; status rinci ada
    /// pada <see cref="Tax"/> dan <see cref="Insurance"/>.
    /// </summary>
    public string TaxCalculationStatus { get; set; } = "NotCalculated";
    public List<string> TaxConfigurationIssues { get; set; } = new();

    /// <summary>Hasil dan konfigurasi PPh pada saat hitung terakhir (snapshot). Null bila penerima belum dihitung atau dihitung sebelum snapshot ada.</summary>
    public PayrollRunTaxResultResponse? Tax { get; set; }

    /// <summary>
    /// Hasil dan konfigurasi asuransi pada saat hitung terakhir (snapshot). Sisi pegawai dan pemberi kerja dipisahkan. Snapshot run lama dapat
    /// memuat baris BPJS historis; snapshot baru hanya memuat asuransi swasta.
    /// </summary>
    public PayrollRunInsuranceResultResponse? Insurance { get; set; }

    /// <summary>
    /// True bila run ini ditahan dari Finance Handoff produksi sampai pemilik proses menetapkan aturan pengiriman Payroll tanpa iuran BPJS
    /// (PayrollFinanceHandoffGuard). Bukan status persetujuan dan tidak mengubah status run.
    /// </summary>
    public bool IsFinanceHandoffHeld { get; set; }

    /// <summary>Alasan penahan (PENDING_OWNER_DECISION); null bila tidak ditahan.</summary>
    public string? FinanceHandoffHoldReason { get; set; }

    // ---- Cicilan piutang ----

    /// <summary>
    /// Seluruh angsuran sah penerima pada periode ini (satu penerima dapat punya beberapa). <see cref="PayrollRunEmployeeResponse.Installment"/>
    /// tetap berisi angsuran pertama untuk kompatibilitas.
    /// </summary>
    public List<PayrollRunInstallmentLineResponse> Installments { get; set; } = new();
}

/// <summary>Ringkasan pembayaran run (instruksi ke Finance dan hasil bank). Hanya terisi setelah handoff.</summary>
public sealed class PayrollRunPaymentSummaryResponse
{
    public int InstructionCount { get; set; }
    public int ScheduledCount { get; set; }
    public int PaidCount { get; set; }
    public int FailedCount { get; set; }
    public decimal TotalInstructionAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }

    /// <summary>
    /// PAY-BE-11. Jumlah potongan piutang yang SUDAH dikonfirmasi ke Finance (saat run disahkan) untuk penerima yang gajinya belum
    /// dibayar: belum dikirim (<c>BELUM_DIKIRIM</c>) atau menunggu hasil bank (<c>MENUNGGU_BAYAR</c>). Satuannya potongan, bukan
    /// penerima. Informasi, bukan selisih; titik pengakuan potongan tidak berubah.
    /// </summary>
    public int ConfirmedDeductionPaymentPendingCount { get; set; }

    /// <summary>
    /// PAY-BE-11. Jumlah potongan piutang yang sudah dikonfirmasi ke Finance untuk penerima yang pembayaran gajinya gagal
    /// (<c>GAGAL</c>) atau dibalik (<c>DIBALIK</c>): potongan tercatat tetapi gaji tidak sampai ke penerima.
    /// </summary>
    public int ConfirmedDeductionPaymentFailedCount { get; set; }
}

/// <summary>Satu baris riwayat perpindahan status run (jejak audit dari persetujuan dan cap waktu aksi).</summary>
public sealed class PayrollRunStatusHistoryItemResponse
{
    public DateTime OccurredAt { get; set; }

    /// <summary>Submit | Return | Approve | Cancel | ReversalRequest | ReversalApprove | ReversalReject | Close | FinanceHandoff.</summary>
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public string? Comments { get; set; }
}

public sealed class PayrollRunDetailResponse : PayrollRunListItemResponse
{
    /// <summary>
    /// True bila ada sumber kompensasi (penetapan gaji, tunjangan struktur, tunjangan khusus, atau tunjangan transport) milik penerima run ini
    /// yang dibuat, diubah, atau disetujui SETELAH perhitungan terakhir dan sebelum run disahkan. Run perlu dihitung ulang; pengajuan dan
    /// pengesahan ditolak (409) selama masih true. Selalu false untuk run yang belum dihitung atau sudah disahkan (snapshot terkunci). Aditif.
    /// </summary>
    public bool NeedsRecalculation { get; set; }

    /// <summary>Sumber kompensasi yang berubah sesudah perhitungan terakhir (teks siap tampil); kosong bila NeedsRecalculation false. Aditif.</summary>
    public List<string> RecalculationReasons { get; set; } = new();

    public string? Notes { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public DateTime? PaymentDate { get; set; }

    /// <summary>Versi kebijakan yang dipakai hitung terakhir (kosong bila belum dihitung).</summary>
    public string? PolicyVersion { get; set; }

    /// <summary>
    /// True bila run ini ditahan dari Finance Handoff produksi sampai pemilik proses menetapkan aturan pengiriman Payroll tanpa iuran BPJS
    /// (PayrollFinanceHandoffGuard). Bukan status persetujuan dan tidak mengubah status run.
    /// </summary>
    public bool IsFinanceHandoffHeld { get; set; }

    /// <summary>Alasan penahan (PENDING_OWNER_DECISION); null bila tidak ditahan.</summary>
    public string? FinanceHandoffHoldReason { get; set; }

    /// <summary>
    /// Aksi yang sedang boleh dijalankan menurut status run (bantuan tampilan, BUKAN pengaman: setiap aksi memeriksa ulang di backend).
    /// Nilai: Calculate, ImportHrd, Submit, Return, Approve, Cancel, FinanceHandoff, Close, RequestReversal, GeneratePayslips.
    /// </summary>
    public List<string> AvailableActions { get; set; } = new();

    /// <summary>
    /// Alasan run berstatus Calculated belum dapat diajukan, atau berstatus Submitted belum dapat disahkan (teks siap tampil, dari backend).
    /// Kosong berarti tidak ada alasan menahan; run lain selalu kosong. Syaratnya sama dengan yang diperiksa ulang oleh SubmitAsync dan
    /// ApproveAsync, sehingga "Submit"/"Approve" pada <see cref="AvailableActions"/> hanya muncul bila daftar ini kosong. Daftar penahan
    /// rinci tetap ada di <c>Blockers</c>. Bantuan tampilan, bukan pengaman. Aditif.
    /// </summary>
    public List<string> DecisionBlockReasons { get; set; } = new();

    /// <summary>
    /// PAY-BE-FIX-030: true bila run berstatus Calculated dan SATU-SATUNYA penahan yang tersisa adalah histori PPh yang belum lengkap
    /// (TAX_YTD_HISTORY_INCOMPLETE) dengan estimasi tersedia untuk semua penerimanya, serta syarat pengajuan lain terpenuhi. Pengajuan
    /// untuk review TIDAK membuat run dapat disahkan. Bantuan tampilan; backend memeriksa ulang. Aditif.
    /// </summary>
    public bool CanSubmitForReview { get; set; }

    /// <summary>Alasan <see cref="CanSubmitForReview"/> false padahal ada penahan histori PPh (teks siap tampil). Aditif.</summary>
    public List<string> SubmitForReviewBlockReasons { get; set; } = new();

    /// <summary>
    /// PAY-BE-FIX-031: kode terstruktur untuk <see cref="SubmitForReviewBlockReasons"/> (<see cref="PayrollRunReviewBlockCodes"/>). Klien memilih
    /// tampilan (hitung ulang, penahan lain, estimasi tidak tersedia) dari kode ini. Aditif.
    /// </summary>
    public List<string> SubmitForReviewBlockCodes { get; set; } = new();

    /// <summary>
    /// PAY-BE-FIX-031: Calculated dan penahan PPh terlihat, tetapi run dihitung sebelum klasifikasi penahan tersedia (snapshot lama atau
    /// tanpa metadata klasifikasi). Pengajuan review baru dapat dinilai setelah run dihitung ulang lewat endpoint Calculate. Aditif.
    /// </summary>
    public bool ReviewRecalculationRequired { get; set; }

    /// <summary>NotApplicable | RecalculationRequired | Blocked | Eligible. Hanya informasi diagnosis; keputusan ada pada CanSubmitForReview. Aditif.</summary>
    public string TaxReviewState { get; set; } = "NotApplicable";

    /// <summary>
    /// PAY-BE-FIX-030: true hanya bila run Submitted, tidak ada alasan menahan, dan PPh sudah final (bukan PPh sementara). Beda dengan izin
    /// PayrollRun:Approve: izin dan kesiapan bisnis adalah dua syarat terpisah. Aditif.
    /// </summary>
    public bool CanApproveFinal { get; set; }

    /// <summary>Jejak pengajuan review-only; null bila run tidak diajukan dengan PPh sementara. Aditif.</summary>
    public PayrollRunReviewOnlyResponse? ReviewOnlySubmission { get; set; }

    /// <summary>
    /// PAY-BE-FIX-032: true bila run Submitted review-only dapat disahkan bersyarat (administratif): satu-satunya masalah adalah histori PPh
    /// belum lengkap dan syarat lain terpenuhi. BUKAN pengesahan final: <see cref="CanApproveFinal"/> tetap false. Aditif.
    /// </summary>
    public bool CanApproveConditionally { get; set; }

    /// <summary>Alasan <see cref="CanApproveConditionally"/> false (teks siap tampil). Aditif.</summary>
    public List<string> ApproveConditionallyBlockReasons { get; set; } = new();

    /// <summary>NONE | CONDITIONAL. CONDITIONAL tidak pernah berarti pajak final. Aditif.</summary>
    public string TaxApprovalStatus { get; set; } = "NONE";

    /// <summary>
    /// PAY-BE-FIX-033: true bila run Submitted sudah disahkan bersyarat dan masih menunggu pengesahan final. Bukan izin mengesahkan: aksi
    /// "ContinueApproval" pada <see cref="AvailableActions"/> hanya membuka panduan/pemeriksaan; tidak ada perubahan data dan tidak
    /// menggantikan validasi endpoint approve. Aditif.
    /// </summary>
    public bool CanContinueApproval { get; set; }

    /// <summary>PAY-BE-FIX-033: langkah berikutnya menuju pengesahan final (<see cref="PayrollRunApprovalNextSteps"/>). Aditif.</summary>
    public string ApprovalNextStep { get; set; } = PayrollRunApprovalNextSteps.None;

    /// <summary>
    /// PAY-BE-FIX-033: true bila pengesahan final menunggu nilai PPh yang sah (run Submitted dengan PPh sementara). Estimasi tidak pernah
    /// dianggap PPh resmi. Aditif.
    /// </summary>
    public bool TaxResolutionRequired { get; set; }

    /// <summary>
    /// PAY-BE-FIX-038: true bila SEMUA bulan histori yang kurang pada snapshot run (penerima terdampak) kini tercakup Saldo Awal PPh berstatus
    /// APPROVED sumber SAME_EMPLOYER_PRE_CUTOVER. Murni baca; tidak mengubah PPh. PPh run tetap sementara sampai dihitung ulang. Aditif.
    /// </summary>
    public bool TaxHistoryResolved { get; set; }

    /// <summary>
    /// PAY-BE-FIX-033/034: penahan NYATA yang masih ada (teks siap tampil). Untuk run dengan PPh sementara, histori PPh yang belum lengkap TIDAK
    /// dihitung di sini (itu tindak lanjut finalisasi, lihat <see cref="ContinueApprovalSteps"/>): hanya penerima bermasalah, belum terhitung,
    /// sumber berubah, snapshot lama, atau penahan non-PPh. Kosong bila tidak ada penahan selain PPh. Aditif.
    /// </summary>
    public List<string> ContinueApprovalBlockReasons { get; set; } = new();

    /// <summary>PAY-BE-FIX-033: langkah penyelesaian berurutan (teks siap tampil); kosong bila tidak ada tindak lanjut. Aditif.</summary>
    public List<string> ContinueApprovalSteps { get; set; } = new();

    /// <summary>Jejak pengesahan bersyarat; null bila belum. Aditif.</summary>
    public PayrollRunConditionalApprovalResponse? ConditionalApproval { get; set; }

    /// <summary>Ringkasan PPh sementara; null bila tidak ada penerima dengan penahan histori PPh. Aditif.</summary>
    public PayrollRunProvisionalTaxSummaryResponse? ProvisionalTax { get; set; }

    /// <summary>Catatan tidak menahan (mis. rekening belum terverifikasi, input Draft tidak dihitung).</summary>
    public List<string> Warnings { get; set; } = new();
    public PayrollRunPaymentSummaryResponse? PaymentSummary { get; set; }

    /// <summary>Penahan pengajuan: data yang belum lengkap atau tidak valid. Kosong berarti run siap diajukan.</summary>
    public List<string> Blockers { get; set; } = new();

    /// <summary>Angsuran periode ini yang belum masuk run beserta alasannya (mis. pegawai tidak termasuk run).</summary>
    public List<string> InstallmentSyncNotes { get; set; } = new();

    public int InstallmentCount { get; set; }
    public decimal TotalInstallmentDue { get; set; }
    public decimal TotalInstallmentPlanned { get; set; }
    public List<PayrollRunEmployeeResponse> Employees { get; set; } = new();
    public List<PayrollReversalResponse> Reversals { get; set; } = new();
}

public sealed class RequestPayrollReversalRequest
{
    /// <summary>Nominal yang dibalik. Kosong = seluruh potongan cicilan yang terpotong saat ini.</summary>
    [Range(0.01, 100000000000)] public decimal? Amount { get; set; }
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
}

public sealed class PayrollReversalResponse
{
    public Guid Id { get; set; }
    public string ReversalNumber { get; set; } = string.Empty;
    public Guid? PayrollRunEmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>Full | Partial.</summary>
    public string ReversalType { get; set; } = string.Empty;

    /// <summary>Requested | Approved | Rejected.</summary>
    public string ReversalStatus { get; set; } = string.Empty;
    public decimal ReversalAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? PostedAt { get; set; }
}

/// <summary>Ringkasan jumlah payroll run per status (baseline standar transaksi). Hanya jumlah, tanpa nominal gaji.</summary>
public sealed class PayrollRunSummaryResponse
{
    public int TotalRuns { get; set; }

    /// <summary>Run yang menunggu tindakan pengguna: Draft, CollectingInput, Calculated, Submitted.</summary>
    public int AwaitingActionCount { get; set; }

    /// <summary>Run yang sudah di tahap pembayaran: SentToFinance, PartiallyPaid.</summary>
    public int InPaymentCount { get; set; }
    public Dictionary<string, int> CountByStatus { get; set; } = new();
}

/// <summary>Metadata filter daftar payroll run (baseline standar transaksi).</summary>
public sealed class PayrollRunFilterMetadataResponse
{
    public List<string> Statuses { get; set; } = new();
    public List<string> RunTypes { get; set; } = new();
    public int DefaultPageSize { get; set; }
    public List<int> PageSizeOptions { get; set; } = new();
}

public static class PayrollPeriodProcessStatuses
{
    /// <summary>Periode sejak peralihan, tetapi HRD belum membuat payroll run. Jadwal angsuran menunggu.</summary>
    public const string AwaitingPayrollRun = "MENUNGGU_PAYROLL_RUN";

    /// <summary>Periode sebelum peralihan: potongan dicatat Finance lewat Potong Gaji Manual, bukan payroll run.</summary>
    public const string ManualFinance = "MANUAL_FINANCE";
}

public static class PayrollInstallmentProcessStatuses
{
    public const string AwaitingPayrollRun = "MENUNGGU_PAYROLL_RUN";
    public const string ManualFinance = "MANUAL_FINANCE";
    public const string NotInRun = "BELUM_MASUK_RUN";
    public const string NotCalculated = "BELUM_DIHITUNG";
    public const string Planned = "DIRENCANAKAN";
    public const string Confirmed = "TERKONFIRMASI";
}

public sealed class PayrollRunPeriodOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DeductionPeriod { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Satu angsuran periode itu: sesuai perjanjian Finance, dan (bila run ada) rencana atau hasil potongan HRD.</summary>
public sealed class PayrollPeriodInstallmentResponse
{
    public Guid InstallmentId { get; set; }
    public string PlanNumber { get; set; } = string.Empty;
    public string ReceivableNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public int InstallmentNumber { get; set; }
    public decimal ScheduledAmount { get; set; }
    public decimal CarriedOverAmount { get; set; }

    /// <summary>Terjadwal ditambah tunggakan terbawa: nominal yang wajib dipotong periode ini sesuai perjanjian.</summary>
    public decimal DueAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string InstallmentStatus { get; set; } = string.Empty;
    public decimal? PlannedDeduction { get; set; }
    public string? PlannedStatus { get; set; }

    /// <summary><see cref="PayrollInstallmentProcessStatuses"/>.</summary>
    public string ProcessStatus { get; set; } = string.Empty;
}

public sealed class PayrollRunPeriodOverviewResponse
{
    public Guid PayrollPeriodId { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string DeductionPeriod { get; set; } = string.Empty;
    public bool IsLocked { get; set; }

    /// <summary>Periode pertama yang wajib memakai payroll run (AutomaticFromPeriod); kosong bila peralihan belum ditetapkan.</summary>
    public string? AutomaticFromPeriod { get; set; }
    public bool IsAutomaticPeriod { get; set; }

    public Guid? RunId { get; set; }
    public string? RunNumber { get; set; }
    public string? RunStatus { get; set; }

    /// <summary><see cref="PayrollPeriodProcessStatuses"/> atau status run (Draft, Calculated, Submitted, Approved).</summary>
    public string PeriodStatus { get; set; } = string.Empty;

    public int EmployeeCount { get; set; }
    public int InstallmentCount { get; set; }
    public decimal TotalDueAmount { get; set; }
    public decimal TotalPlannedDeduction { get; set; }
    public List<PayrollPeriodInstallmentResponse> Items { get; set; } = new();
}

/// <summary>PAY-BE-23 (PGJ-C1, PRL-DEC-42): saringan rekap lembur terverifikasi yang diimpor ke run.</summary>
public sealed class PayrollRunOvertimeInputQuery
{
    /// <summary>Nama atau nomor pegawai.</summary>
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Satu baris lembur terverifikasi HR yang diimpor ke run (snapshot PrlPayrollOvertimeInput). Nominal sensitif.</summary>
public sealed class PayrollRunOvertimeInputResponse
{
    public Guid Id { get; set; }
    public Guid PayrollRunEmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly OvertimeDate { get; set; }
    public int VerifiedMinutes { get; set; }
    public decimal RateMultiplier { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal OvertimeAmount { get; set; }
}

public sealed class PayrollRunValidationException(string message) : Exception(message);

public sealed class PayrollRunConflictException(string message) : Exception(message);
