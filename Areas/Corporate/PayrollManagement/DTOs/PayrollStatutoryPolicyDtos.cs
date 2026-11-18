using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

// Kontrak kebijakan regulasi penggajian berversi (PPh dan iuran BPJS). Seluruh nilai regulasi dimasukkan HRD dan disahkan Finance;
// tidak ada angka bawaan dari Backend.

/// <summary>Filter daftar kebijakan.</summary>
public sealed class PayrollStatutoryPolicyQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
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

/// <summary>Isi kebijakan PPh yang disunting HRD (buat dan ubah).</summary>
public sealed class SavePayrollTaxPolicyRequest
{
    public string PolicyVersion { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LegalBasis { get; set; } = string.Empty;
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }

    public decimal PositionCostRatePercent { get; set; }
    public decimal PositionCostMonthlyCap { get; set; }
    public decimal PositionCostAnnualCap { get; set; }
    public decimal NoNpwpSurchargePercent { get; set; }
    public decimal PkpRoundingUnit { get; set; }
    public bool RoundTaxToWholeRupiah { get; set; }

    /// <summary>Daftar program dipisah koma (KES, JKK, JKM, ...); kosong = tidak ada premi pemberi kerja yang termasuk bruto.</summary>
    public string? EmployerPremiumPrograms { get; set; }

    /// <summary>Daftar program dipisah koma (JHT, JP); kosong = tidak ada iuran pensiun pengurang.</summary>
    public string? PensionDeductionPrograms { get; set; }

    public decimal? NonEmployeeBasisPercent { get; set; }
    public string? NonEmployeeMethod { get; set; }
    public decimal? Pph26RatePercent { get; set; }

    public List<PtkpEntry> Ptkp { get; set; } = new();
    public List<TerRateEntry> TerRates { get; set; } = new();
    public List<TaxBracketEntry> Brackets { get; set; } = new();
    public string? Notes { get; set; }
}

public sealed class PayrollTaxPolicyDetailResponse
{
    public PayrollStatutoryPolicyListItemResponse Summary { get; set; } = new();

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
    public List<PayrollStatutoryPolicyOptionResponse> TerCategoryOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> TerPeriodKindOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> NonEmployeeMethodOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> ProgramOptions { get; set; } = new();
    public List<PayrollStatutoryPolicyOptionResponse> JkkRiskClassOptions { get; set; } = new();
    public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50 };
}

/// <summary>
/// Ketersediaan kebijakan yang disahkan pada tanggal tertentu. Status "Menunggu Verifikasi Kebijakan" berarti belum ada kebijakan
/// APPROVED yang berlaku: PPh otomatis dan iuran BPJS tidak dihitung (penahan, bukan Rp0).
/// </summary>
public sealed class PayrollStatutoryPolicyAvailabilityResponse
{
    public DateTime AsOf { get; set; }
    public bool TaxPolicyApproved { get; set; }
    public string? TaxPolicyVersion { get; set; }
    public string TaxPolicyStatusLabel { get; set; } = string.Empty;
    public bool BpjsPolicyApproved { get; set; }
    public string? BpjsPolicyVersion { get; set; }
    public string BpjsPolicyStatusLabel { get; set; } = string.Empty;

    /// <summary>Jumlah draf/pengajuan kebijakan yang menunggu verifikasi atau pengesahan.</summary>
    public int PendingTaxPolicyCount { get; set; }
    public int PendingBpjsPolicyCount { get; set; }
}
