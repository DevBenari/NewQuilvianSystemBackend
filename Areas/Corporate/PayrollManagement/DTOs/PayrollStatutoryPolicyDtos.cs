using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

// Kontrak kebijakan regulasi penggajian berversi (PPh dan iuran BPJS). Seluruh nilai regulasi dimasukkan HRD dan disahkan Finance;
// tidak ada angka bawaan dari Backend.

/// <summary>Filter daftar kebijakan.</summary>
public sealed class PayrollStatutoryPolicyQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? TaxArticle { get; set; }
    public string? CalculationType { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Aksi alur kebijakan (ajukan, setujui, tolak, pensiunkan). Alasan wajib untuk menolak dan mempensiunkan.</summary>
public sealed class PayrollStatutoryPolicyActionRequest
{
    public string? Reason { get; set; }
}

public sealed class PayrollStatutoryPolicyReviseRequest
{
    public string PolicyVersion { get; set; } = string.Empty;
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
}

/// <summary>Kemampuan pemanggil menurut Akses Role; bantuan tampilan, bukan pengaman (setiap aksi diperiksa ulang di Backend).</summary>
public sealed class PayrollStatutoryPolicyActionsResponse
{
    public bool CanUpdate { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanRetire { get; set; }
    public bool CanDelete { get; set; }
}

public sealed class PayrollStatutoryPolicyListItemResponse
{
    public Guid Id { get; set; }
    public string PolicyVersion { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TaxArticle { get; set; }
    public string? CalculationType { get; set; }
    public string? CategoryLabel { get; set; }
    public string? EffectiveStatus { get; set; }
    public string LegalBasis { get; set; } = string.Empty;
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public string PolicyStatus { get; set; } = string.Empty;

    /// <summary>True bila kebijakan APPROVED dan masa berlakunya mencakup hari ini (bantuan tampilan).</summary>
    public bool IsEffectiveToday { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? StatusReason { get; set; }
}

public sealed class PayrollStatutoryPolicyPagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPage { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

// ---------------- Kebijakan PPh ----------------

/// <summary>Status dan identitas referensi tarif PPh 21 resmi yang dikelola di Backend (read-only bagi pengguna).</summary>
public sealed class PayrollTaxOfficialReferenceResponse
{
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceVersion { get; set; } = string.Empty;
    public string CalculationType { get; set; } = string.Empty;
    public string CalculationTypeLabel { get; set; } = string.Empty;
    public string CalculationMethodLabel { get; set; } = string.Empty;
    public string DefaultLegalBasis { get; set; } = string.Empty;
    public bool IsReady { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public int PtkpCount { get; set; }
    public int TerRateCount { get; set; }
    public int BracketCount { get; set; }
    public List<string> Issues { get; set; } = new();
}

/// <summary>
/// Isi kebijakan PPh yang disunting HRD (buat dan ubah).
/// Pengguna tidak lagi wajib memasukkan tabel PTKP, TER A/B/C, Pasal 17, atau parameter teknis: saat
/// <see cref="UseOfficialReference"/> bernilai true (bawaan), Backend mengisi seluruh tabel regulasi resmi secara server-side.
/// </summary>
public sealed class SavePayrollTaxPolicyRequest
{
    public string PolicyVersion { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    /// <summary>Pasal PPh: PPH21 atau PPH23.</summary>
    public string? TaxArticle { get; set; }

    /// <summary>Jenis perhitungan kebijakan PPh 21 atau PPh 23.</summary>
    public string? CalculationType { get; set; }

    /// <summary>Kategori subjek pajak penerima penghasilan.</summary>
    public string? RecipientTaxCategory { get; set; }

    /// <summary>Objek pemotongan penghasilan (khususnya PPh 23).</summary>
    public string? IncomeObjectType { get; set; }

    /// <summary>Tarif pemotongan persen tunggal (khususnya PPh 23, mis. 2% atau 15%).</summary>
    public decimal? RatePercent { get; set; }

    /// <summary>Dasar Pengenaan Pajak (DPP), mis. Jumlah Bruto tidak termasuk PPN.</summary>
    public string? DppBasis { get; set; }

    public string LegalBasis { get; set; } = string.Empty;
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }

    /// <summary>Bawaan true untuk PPh 21 Pegawai Tetap: Backend memuat referensi tarif resmi PPh 21 secara server-side.</summary>
    public bool UseOfficialReference { get; set; } = true;

    public decimal? PositionCostRatePercent { get; set; }
    public decimal? PositionCostMonthlyCap { get; set; }
    public decimal? PositionCostAnnualCap { get; set; }
    public decimal? NoNpwpSurchargePercent { get; set; }
    public decimal? PkpRoundingUnit { get; set; }
    public bool? RoundTaxToWholeRupiah { get; set; }

    /// <summary>Daftar program dipisah koma (KES, JKK, JKM, ...); diisi otomatis dari referensi resmi bila UseOfficialReference = true.</summary>
    public string? EmployerPremiumPrograms { get; set; }

    /// <summary>Daftar program dipisah koma (JHT, JP); diisi otomatis dari referensi resmi bila UseOfficialReference = true.</summary>
    public string? PensionDeductionPrograms { get; set; }

    public decimal? NonEmployeeBasisPercent { get; set; }
    public string? NonEmployeeMethod { get; set; }
    public decimal? Pph26RatePercent { get; set; }

    public List<PtkpEntry>? Ptkp { get; set; }
    public List<TerRateEntry>? TerRates { get; set; }
    public List<TaxBracketEntry>? Brackets { get; set; }
    public string? Notes { get; set; }
}

public sealed class PayrollTaxPolicyDetailResponse
{
    public PayrollStatutoryPolicyListItemResponse Summary { get; set; } = new();

    public string? TaxArticle { get; set; }
    public string CalculationType { get; set; } = string.Empty;
    public string CalculationTypeLabel { get; set; } = string.Empty;
    public string CalculationMethodLabel { get; set; } = string.Empty;
    public string? RecipientTaxCategory { get; set; }
    public string? IncomeObjectType { get; set; }
    public decimal? RatePercent { get; set; }
    public string? DppBasis { get; set; }
    public string? EffectiveStatus { get; set; }
    public string ReferenceVersion { get; set; } = string.Empty;
    public bool ReferenceReady { get; set; }
    public string ReferenceStatusLabel { get; set; } = string.Empty;

    public decimal PositionCostRatePercent { get; set; }
    public decimal PositionCostMonthlyCap { get; set; }
    public decimal PositionCostAnnualCap { get; set; }
    public decimal NoNpwpSurchargePercent { get; set; }
    public decimal PkpRoundingUnit { get; set; }
    public bool RoundTaxToWholeRupiah { get; set; }
    public string? EmployerPremiumPrograms { get; set; }
    public string? PensionDeductionPrograms { get; set; }
    public decimal? NonEmployeeBasisPercent { get; set; }
    public string? NonEmployeeMethod { get; set; }
    public decimal? Pph26RatePercent { get; set; }

    public List<PtkpEntry> Ptkp { get; set; } = new();
    public List<TerRateEntry> TerRates { get; set; } = new();
    public List<TaxBracketEntry> Brackets { get; set; } = new();
    public string? Notes { get; set; }

    /// <summary>Kekurangan isi yang akan menolak pengajuan; kosong bila kebijakan lengkap.</summary>
    public List<string> CompletenessIssues { get; set; } = new();

    public PayrollStatutoryPolicyActionsResponse Actions { get; set; } = new();
}

// ---------------- Kebijakan iuran BPJS ----------------

/// <summary>Isi kebijakan iuran BPJS yang disunting HRD (buat dan ubah).</summary>
public sealed class SavePayrollBpjsPolicyRequest
{
    public string PolicyVersion { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LegalBasis { get; set; } = string.Empty;
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }

    public int? JkkRiskClass { get; set; }
    public string? JkkRiskBasis { get; set; }

    public decimal HealthCapAmount { get; set; }
    public decimal HealthEmployeeRate { get; set; }
    public decimal HealthEmployerRate { get; set; }
    public decimal JhtEmployeeRate { get; set; }
    public decimal JhtEmployerRate { get; set; }
    public decimal JpCapAmount { get; set; }
    public decimal JpEmployeeRate { get; set; }
    public decimal JpEmployerRate { get; set; }
    public decimal JkkRate { get; set; }
    public decimal JkmRate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Menyalin nilai Pengaturan Penggajian saat ini menjadi draf kebijakan (nilai itu BELUM resmi sampai diverifikasi dan disahkan).</summary>
public sealed class CreatePayrollBpjsPolicyFromSettingRequest
{
    public string PolicyVersion { get; set; } = string.Empty;
    public string LegalBasis { get; set; } = string.Empty;
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
}

public sealed class PayrollBpjsPolicyDetailResponse
{
    public PayrollStatutoryPolicyListItemResponse Summary { get; set; } = new();

    public int? JkkRiskClass { get; set; }
    public string? JkkRiskBasis { get; set; }
    public decimal HealthCapAmount { get; set; }
    public decimal HealthEmployeeRate { get; set; }
    public decimal HealthEmployerRate { get; set; }
    public decimal JhtEmployeeRate { get; set; }
    public decimal JhtEmployerRate { get; set; }
    public decimal JpCapAmount { get; set; }
    public decimal JpEmployeeRate { get; set; }
    public decimal JpEmployerRate { get; set; }
    public decimal JkkRate { get; set; }
    public decimal JkmRate { get; set; }
    public string? Notes { get; set; }

    public List<string> CompletenessIssues { get; set; } = new();
    public PayrollStatutoryPolicyActionsResponse Actions { get; set; } = new();
}

// ---------------- Metadata dan ketersediaan ----------------

public sealed class PayrollStatutoryPolicyOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public sealed class PayrollStatutoryPolicyMetadataResponse
{
    public List<PayrollStatutoryPolicyOptionResponse> StatusOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> TaxArticleOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> TaxCalculationTypeOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> Pph21CategoryOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> Pph23ObjectOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> TaxEffectiveStatusOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> TerCategoryOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> TerPeriodKindOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> NonEmployeeMethodOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> ProgramOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> JkkRiskClassOptions { get; set; } = new();
    public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50 };
    public PayrollTaxOfficialReferenceResponse? OfficialTaxReference { get; set; }
    public bool EnableBpjsInPayroll { get; set; } = true;
    public string? BpjsTestingModeLabel { get; set; }
}

/// <summary>
/// Ketersediaan kebijakan yang disahkan pada tanggal tertentu. Status "Menunggu Verifikasi Kebijakan" berarti belum ada kebijakan
/// APPROVED yang berlaku: PPh otomatis dan iuran BPJS tidak dihitung (penahan, bukan Rp0). Ketika BPJS dinonaktifkan sementara pada
/// mode pengujian, <see cref="EnableBpjsInPayroll"/> bernilai false dan status BPJS ditandai eksplisit sebagai mode pengujian.
/// </summary>
public sealed class PayrollStatutoryPolicyAvailabilityResponse
{
    public DateTime AsOf { get; set; }
    public bool TaxPolicyApproved { get; set; }
    public string? TaxPolicyVersion { get; set; }
    public string TaxPolicyStatusLabel { get; set; } = string.Empty;
    public bool EnableBpjsInPayroll { get; set; } = true;
    public bool BpjsPolicyApproved { get; set; }
    public string? BpjsPolicyVersion { get; set; }
    public string BpjsPolicyStatusLabel { get; set; } = string.Empty;
    public string? BpjsTestingModeNotice { get; set; }

    /// <summary>Jumlah draf/pengajuan kebijakan yang menunggu verifikasi atau pengesahan.</summary>
    public int PendingTaxPolicyCount { get; set; }
    public int PendingBpjsPolicyCount { get; set; }
}
