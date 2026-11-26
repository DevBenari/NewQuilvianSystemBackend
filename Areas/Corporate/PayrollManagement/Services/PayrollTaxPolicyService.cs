using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Kemampuan pemanggil atas kebijakan regulasi menurut Akses Role (bantuan tampilan; pengaman tetap di Backend).</summary>
public sealed record PayrollStatutoryPolicyCaller(bool CanUpdate, bool CanSubmit, bool CanApprove, bool CanReject, bool CanRetire, bool CanDelete)
{
    public PayrollStatutoryPolicyActionsResponse ToResponse(string status) => new()
    {
        CanUpdate = CanUpdate && StatutoryPolicyStatuses.IsEditable(status),
        CanSubmit = CanSubmit && StatutoryPolicyStatuses.IsEditable(status),
        CanApprove = CanApprove && status == StatutoryPolicyStatuses.PendingApproval,
        CanReject = CanReject && status == StatutoryPolicyStatuses.PendingApproval,
        CanRetire = CanRetire && status == StatutoryPolicyStatuses.Approved,
        CanDelete = CanDelete && StatutoryPolicyStatuses.IsEditable(status)
    };

    /// <summary>
    /// Pada Kebijakan Tarif PPh: alur verifikasi manual (Submit/Approve/Reject) dihapus. Kebijakan berlaku otomatis.
    /// </summary>
    public PayrollStatutoryPolicyActionsResponse ToTaxResponse(string status) => new()
    {
        CanUpdate = CanUpdate && status != StatutoryPolicyStatuses.Retired,
        CanSubmit = false,
        CanApprove = false,
        CanReject = false,
        CanRetire = CanRetire && status == StatutoryPolicyStatuses.Approved,
        CanDelete = CanDelete && status != StatutoryPolicyStatuses.Retired
    };
}

/// <summary>
/// Kebijakan tarif PPh berversi (PrlTaxPolicy): sumber tunggal tarif untuk skema AUTO_REGULATORY.
///
/// Siklus: HRD menyusun DRAFT dan mengajukannya (menyatakan isi sudah diverifikasi terhadap dasar peraturan) → Finance mengesahkan
/// atau menolak → APPROVED dipakai mesin payroll pada periode yang masa berlakunya tercakup. Pengesah selalu pengguna yang berbeda
/// dari pengaju. Kebijakan yang sudah disahkan tidak disunting: perubahan tarif dibuat sebagai revisi (versi baru) dan kebijakan lama
/// dipensiunkan, sehingga payroll historis tetap menunjuk versi yang dipakainya. Dua kebijakan APPROVED tidak boleh beririsan masa
/// berlakunya. Seluruh angka regulasi dimasukkan HRD; tidak ada nilai bawaan dari sistem. Nominal tarif tidak ditulis ke log.
/// </summary>
public sealed class PayrollTaxPolicyService
{
    private const string LogCategory = "Corporate.PayrollManagement.TaxPolicy";
    private const string LockKey = "PAYROLL_TAX_POLICY";
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public PayrollTaxPolicyService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------------------------------------------

    public async Task<PayrollStatutoryPolicyPagedResponse<PayrollStatutoryPolicyListItemResponse>> ListAsync(
        PayrollStatutoryPolicyQuery query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var source = _dbContext.PrlTaxPolicies.AsNoTracking().Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(query.TaxArticle))
        {
            var article = query.TaxArticle.Trim().ToUpperInvariant();
            if (article == TaxArticles.Pph23)
            {
                source = source.Where(x => x.Title.Contains("PPh 23") || x.Title.Contains("PPH23") || (x.Notes != null && x.Notes.Contains("PPH23")));
            }
            else if (article == TaxArticles.Pph21)
            {
                source = source.Where(x => !x.Title.Contains("PPh 23") && !x.Title.Contains("PPH23") && (x.Notes == null || !x.Notes.Contains("PPH23")));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim();
            if (status.Equals(TaxPolicyEffectiveStatuses.Active, StringComparison.OrdinalIgnoreCase) || status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                source = source.Where(x => x.IsActive && x.PolicyStatus == StatutoryPolicyStatuses.Approved && x.EffectiveStartDate <= today && (x.EffectiveEndDate == null || x.EffectiveEndDate >= today));
            }
            else if (status.Equals(TaxPolicyEffectiveStatuses.Scheduled, StringComparison.OrdinalIgnoreCase) || status.Equals("SCHEDULED", StringComparison.OrdinalIgnoreCase))
            {
                source = source.Where(x => x.IsActive && x.EffectiveStartDate > today);
            }
            else if (status.Equals(TaxPolicyEffectiveStatuses.Expired, StringComparison.OrdinalIgnoreCase) || status.Equals("EXPIRED", StringComparison.OrdinalIgnoreCase))
            {
                source = source.Where(x => x.IsActive && x.EffectiveEndDate != null && x.EffectiveEndDate < today);
            }
            else if (status.Equals(TaxPolicyEffectiveStatuses.Inactive, StringComparison.OrdinalIgnoreCase) || status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                source = source.Where(x => !x.IsActive || x.PolicyStatus == StatutoryPolicyStatuses.Retired);
            }
            else
            {
                var upper = status.ToUpperInvariant();
                source = source.Where(x => x.PolicyStatus == upper);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            source = source.Where(x => x.PolicyVersion.ToLower().Contains(term) || x.Title.ToLower().Contains(term) || x.LegalBasis.ToLower().Contains(term));
        }

        var total = await source.CountAsync(cancellationToken);
        var rows = await source
            .OrderByDescending(x => x.EffectiveStartDate)
            .ThenByDescending(x => x.CreateDateTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PayrollStatutoryPolicyPagedResponse<PayrollStatutoryPolicyListItemResponse>
        {
            Items = rows.Select(x => ToListItem(x, today)).ToList(),
            TotalCount = total,
            TotalPage = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PayrollTaxPolicyDetailResponse> GetAsync(Guid id, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.PrlTaxPolicies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kebijakan tarif PPh tidak ditemukan.");
        return ToDetail(entity, caller);
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Tulis
    // ------------------------------------------------------------------------------------------------------------------------

    public Task<PayrollTaxPolicyDetailResponse> CreateAsync(
        SavePayrollTaxPolicyRequest request, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = new PrlTaxPolicy { Id = Guid.NewGuid(), CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId };
            await ApplyAsync(entity, request, cancellationToken);

            var isPph23 = DetermineTaxArticle(entity) == TaxArticles.Pph23;
            if (!isPph23)
            {
                var view = PayrollTaxPolicy.FromEntity(entity);
                var issues = PayrollTaxPolicyEngine.ValidateCompleteness(view);
                if (issues.Count > 0)
                    throw new PayrollRunValidationException("Kebijakan PPh 21 belum lengkap atau tidak valid: " + string.Join(" ", issues));
            }

            var start = entity.EffectiveStartDate;
            var end = entity.EffectiveEndDate;
            var sameScopePolicies = await _dbContext.PrlTaxPolicies.AsNoTracking()
                .Where(x => !x.IsDelete && x.Id != entity.Id && x.PolicyStatus == StatutoryPolicyStatuses.Approved && x.IsActive
                    && (x.EffectiveEndDate == null || x.EffectiveEndDate >= start))
                .ToListAsync(cancellationToken);

            var entityArticle = DetermineTaxArticle(entity);
            var entityCategory = DetermineCalculationType(entity);

            var overlapping = sameScopePolicies.FirstOrDefault(x =>
                DetermineTaxArticle(x) == entityArticle &&
                DetermineCalculationType(x) == entityCategory &&
                (!end.HasValue || x.EffectiveStartDate <= end.Value));

            if (overlapping is not null)
                throw new PayrollRunConflictException(
                    $"Masa berlaku beririsan dengan kebijakan {DetermineCategoryLabel(entityCategory)} yang aktif ({overlapping.PolicyVersion}). Pensiunkan kebijakan itu atau sesuaikan tanggal berlaku.");

            entity.PolicyStatus = StatutoryPolicyStatuses.Approved;
            entity.DecidedAt = DateTime.UtcNow;
            entity.DecidedByUserId = actorUserId;
            entity.StatusReason = null;

            _dbContext.PrlTaxPolicies.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Create", "Menyimpan kebijakan tarif PPh (aktif otomatis).", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollTaxPolicyDetailResponse> UpdateAsync(
        Guid id, SavePayrollTaxPolicyRequest request, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.PolicyStatus == StatutoryPolicyStatuses.Retired || !entity.IsActive)
                throw new PayrollInvalidTransitionException("Kebijakan yang sudah dipensiunkan atau nonaktif tidak dapat diubah.");

            // Validasi status editable bila berstatus draf: StatutoryPolicyStatuses.IsEditable(entity.PolicyStatus)
            await ApplyAsync(entity, request, cancellationToken);

            var isPph23 = DetermineTaxArticle(entity) == TaxArticles.Pph23;
            if (!isPph23)
            {
                var view = PayrollTaxPolicy.FromEntity(entity);
                var issues = PayrollTaxPolicyEngine.ValidateCompleteness(view);
                if (issues.Count > 0)
                    throw new PayrollRunValidationException("Kebijakan PPh 21 belum lengkap atau tidak valid: " + string.Join(" ", issues));
            }

            var start = entity.EffectiveStartDate;
            var end = entity.EffectiveEndDate;
            var sameScopePolicies = await _dbContext.PrlTaxPolicies.AsNoTracking()
                .Where(x => !x.IsDelete && x.Id != entity.Id && x.PolicyStatus == StatutoryPolicyStatuses.Approved && x.IsActive
                    && (x.EffectiveEndDate == null || x.EffectiveEndDate >= start))
                .ToListAsync(cancellationToken);

            var entityArticle = DetermineTaxArticle(entity);
            var entityCategory = DetermineCalculationType(entity);

            var overlapping = sameScopePolicies.FirstOrDefault(x =>
                DetermineTaxArticle(x) == entityArticle &&
                DetermineCalculationType(x) == entityCategory &&
                (!end.HasValue || x.EffectiveStartDate <= end.Value));

            if (overlapping is not null)
                throw new PayrollRunConflictException(
                    $"Masa berlaku beririsan dengan kebijakan {DetermineCategoryLabel(entityCategory)} yang aktif ({overlapping.PolicyVersion}). Pensiunkan kebijakan itu atau sesuaikan tanggal berlaku.");

            entity.PolicyStatus = StatutoryPolicyStatuses.Approved;
            entity.DecidedAt = DateTime.UtcNow;
            entity.DecidedByUserId = actorUserId;
            entity.StatusReason = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Update", "Memperbarui kebijakan tarif PPh (aktif otomatis).", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    /// <summary>Membuat versi revisi dari kebijakan yang ada. Kebijakan revisi langsung aktif otomatis sesuai tanggal efektif.</summary>
    public Task<PayrollTaxPolicyDetailResponse> ReviseAsync(
        Guid id, PayrollStatutoryPolicyReviseRequest request, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var source = await LoadAsync(id, cancellationToken);
            var article = DetermineTaxArticle(source);
            var copy = new SavePayrollTaxPolicyRequest
            {
                PolicyVersion = request.PolicyVersion,
                Title = source.Title,
                TaxArticle = article,
                CalculationType = DetermineCalculationType(source),
                LegalBasis = source.LegalBasis,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                UseOfficialReference = article == TaxArticles.Pph21,
                Notes = $"Revisi dari versi {source.PolicyVersion}."
            };

            var entity = new PrlTaxPolicy { Id = Guid.NewGuid(), CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId };
            await ApplyAsync(entity, copy, cancellationToken);

            entity.PolicyStatus = StatutoryPolicyStatuses.Approved;
            entity.DecidedAt = DateTime.UtcNow;
            entity.DecidedByUserId = actorUserId;
            entity.StatusReason = null;

            _dbContext.PrlTaxPolicies.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Revise", "Membuat revisi kebijakan tarif PPh.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollTaxPolicyDetailResponse> SubmitAsync(
        Guid id, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            entity.PolicyStatus = StatutoryPolicyStatuses.Approved;
            entity.DecidedAt = DateTime.UtcNow;
            entity.DecidedByUserId = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollTaxPolicyDetailResponse> ApproveAsync(
        Guid id, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.SubmittedByUserId.HasValue && entity.SubmittedByUserId.Value == actorUserId)
                throw new PayrollForbiddenActionException("Pengaju tidak boleh mengesahkan kebijakan yang diajukannya sendiri; pengesahan oleh Finance.");

            entity.PolicyStatus = StatutoryPolicyStatuses.Approved;
            entity.DecidedAt = DateTime.UtcNow;
            entity.DecidedByUserId = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollTaxPolicyDetailResponse> RejectAsync(
        Guid id, string? reason, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            entity.PolicyStatus = StatutoryPolicyStatuses.Retired;
            entity.IsActive = false;
            entity.StatusReason = reason;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ToDetail(entity, caller);
        }, cancellationToken);

    /// <summary>Mempensiunkan/menonaktifkan kebijakan yang sudah ada. Run yang sudah menyimpan snapshot versinya tidak berubah.</summary>
    public Task<PayrollTaxPolicyDetailResponse> RetireAsync(
        Guid id, string? reason, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan penonaktifan kebijakan wajib diisi.");

            entity.PolicyStatus = StatutoryPolicyStatuses.Retired;
            entity.IsActive = false;
            entity.RetiredAt = DateTime.UtcNow;
            entity.StatusReason = reason.Trim().Length > 500 ? reason.Trim()[..500] : reason.Trim();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Retire", "Menonaktifkan / mempensiunkan kebijakan tarif PPh.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    /// <summary>Menghapus (soft delete) kebijakan tarif PPh.</summary>
    public Task<bool> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Delete", "Menghapus kebijakan tarif PPh.", entity, actorUserId);
            return true;
        }, cancellationToken);

    // ------------------------------------------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------------------------------------------

    private async Task<PrlTaxPolicy> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.PrlTaxPolicies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Kebijakan tarif PPh tidak ditemukan.");

    private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

    private static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }

    /// <summary>
    /// Memvalidasi masukan kebijakan dan mengisi tabel tarif. Bila <see cref="SavePayrollTaxPolicyRequest.UseOfficialReference"/> true
    /// (bawaan), seluruh tabel PTKP, TER A/B/C, Pasal 17, dan parameter regulasi diambil server-side dari referensi resmi Backend.
    /// </summary>
    private async Task ApplyAsync(PrlTaxPolicy entity, SavePayrollTaxPolicyRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        var version = Clean(request.PolicyVersion, 100);
        if (version is null) errors.Add("Nomor versi kebijakan wajib diisi.");
        var title = Clean(request.Title, 200);
        if (title is null) errors.Add("Nama kebijakan wajib diisi.");

        var calculationType = string.IsNullOrWhiteSpace(request.CalculationType)
            ? TaxPolicyCalculationTypes.PermanentEmployeeTer
            : request.CalculationType.Trim().ToUpperInvariant();
        if (!TaxPolicyCalculationTypes.All.Contains(calculationType))
            errors.Add($"Jenis perhitungan '{calculationType}' tidak didukung.");

        var isPph23 = calculationType.StartsWith("PPH23", StringComparison.OrdinalIgnoreCase) ||
                      (request.TaxArticle != null && request.TaxArticle.Equals(TaxArticles.Pph23, StringComparison.OrdinalIgnoreCase));

        var official = PayrollTaxOfficialReferenceProvider.GetCurrent();
        var legalBasis = Clean(request.LegalBasis, 1000);
        if (legalBasis is null && request.UseOfficialReference && !isPph23)
            legalBasis = official.Status.DefaultLegalBasis;
        if (legalBasis is null) errors.Add("Dasar peraturan wajib diisi.");

        if (request.EffectiveStartDate == default) errors.Add("Tanggal mulai berlaku wajib diisi.");
        if (request.EffectiveEndDate.HasValue && request.EffectiveStartDate != default
            && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date)
            errors.Add("Tanggal akhir berlaku tidak boleh sebelum tanggal mulai.");

        if (version is not null)
        {
            var duplicate = await _dbContext.PrlTaxPolicies.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.Id != entity.Id && x.PolicyVersion == version, cancellationToken);
            if (duplicate) errors.Add($"Nomor versi {version} sudah dipakai kebijakan lain.");
        }

        if (isPph23)
        {
            if (errors.Count > 0) throw new PayrollRunValidationException(string.Join(" ", errors));

            entity.PolicyVersion = version!;
            entity.Title = title!;
            entity.LegalBasis = legalBasis!;
            entity.EffectiveStartDate = Utc(request.EffectiveStartDate);
            entity.EffectiveEndDate = request.EffectiveEndDate.HasValue ? Utc(request.EffectiveEndDate.Value) : null;
            entity.PositionCostRatePercent = 0m;
            entity.PositionCostMonthlyCap = 0m;
            entity.PositionCostAnnualCap = 0m;
            entity.NoNpwpSurchargePercent = 100m;
            entity.PkpRoundingUnit = 1m;
            entity.RoundTaxToWholeRupiah = true;
            entity.EmployerPremiumPrograms = null;
            entity.PensionDeductionPrograms = null;
            entity.NonEmployeeBasisPercent = null;
            entity.NonEmployeeMethod = null;
            entity.Pph26RatePercent = null;
            entity.PtkpJson = "[]";
            entity.TerRatesJson = "[]";
            entity.BracketsJson = "[]";
            entity.Notes = Clean(request.Notes, 1000);
            return;
        }

        if (request.UseOfficialReference)
        {
            if (!official.Status.IsReady)
            {
                var detail = official.Status.Issues.Count > 0 ? " " + string.Join(" ", official.Status.Issues) : string.Empty;
                errors.Add("Referensi tarif PPh 21 resmi di Backend belum siap atau belum terverifikasi sehingga kebijakan tidak dapat disimpan." + detail);
            }

            if (errors.Count > 0) throw new PayrollRunValidationException(string.Join(" ", errors));

            entity.PolicyVersion = version!;
            entity.Title = title!;
            entity.LegalBasis = legalBasis!;
            entity.EffectiveStartDate = Utc(request.EffectiveStartDate);
            entity.EffectiveEndDate = request.EffectiveEndDate.HasValue ? Utc(request.EffectiveEndDate.Value) : null;
            entity.PositionCostRatePercent = official.PositionCostRatePercent;
            entity.PositionCostMonthlyCap = official.PositionCostMonthlyCap;
            entity.PositionCostAnnualCap = official.PositionCostAnnualCap;
            entity.NoNpwpSurchargePercent = official.NoNpwpSurchargePercent;
            entity.PkpRoundingUnit = official.PkpRoundingUnit;
            entity.RoundTaxToWholeRupiah = official.RoundTaxToWholeRupiah;
            entity.EmployerPremiumPrograms = official.EmployerPremiumPrograms;
            entity.PensionDeductionPrograms = official.PensionDeductionPrograms;
            entity.NonEmployeeBasisPercent = null;
            entity.NonEmployeeMethod = null;
            entity.Pph26RatePercent = null;
            entity.PtkpJson = PayrollTaxPolicyEngine.SerializeTable(official.Ptkp);
            entity.TerRatesJson = PayrollTaxPolicyEngine.SerializeTable(official.TerRates);
            entity.BracketsJson = PayrollTaxPolicyEngine.SerializeTable(official.Brackets);
            entity.Notes = Clean(request.Notes, 1000);
            return;
        }

        var positionCostRatePercent = request.PositionCostRatePercent ?? 0m;
        var positionCostMonthlyCap = request.PositionCostMonthlyCap ?? 0m;
        var positionCostAnnualCap = request.PositionCostAnnualCap ?? 0m;
        var noNpwpSurchargePercent = request.NoNpwpSurchargePercent ?? 0m;
        var pkpRoundingUnit = request.PkpRoundingUnit ?? 0m;
        var roundTaxToWholeRupiah = request.RoundTaxToWholeRupiah ?? true;

        if (positionCostRatePercent < 0m || positionCostRatePercent > 100m) errors.Add("Persen biaya jabatan harus antara 0 dan 100.");
        if (positionCostMonthlyCap < 0m || positionCostAnnualCap < 0m) errors.Add("Batas biaya jabatan tidak boleh negatif.");
        if (noNpwpSurchargePercent < 0m || noNpwpSurchargePercent > 1000m) errors.Add("Tambahan persen tanpa NPWP tidak valid.");
        if (pkpRoundingUnit < 0m) errors.Add("Kelipatan pembulatan PKP tidak boleh negatif.");

        var premiumPrograms = PayrollTaxPolicyEngine.ParseProgramList(request.EmployerPremiumPrograms);
        var pensionPrograms = PayrollTaxPolicyEngine.ParseProgramList(request.PensionDeductionPrograms);

        var nonEmployeeMethod = PayrollTaxInsuranceRules.NormalizeCode(request.NonEmployeeMethod);
        if (nonEmployeeMethod is not null && !TaxNonEmployeeMethods.All.Contains(nonEmployeeMethod))
            errors.Add("Cara penerapan tarif bukan pegawai tidak valid. Gunakan PER_PAYMENT atau CUMULATIVE.");
        if (request.NonEmployeeBasisPercent is < 0m or > 100m) errors.Add("Persen dasar pengenaan bukan pegawai harus antara 0 dan 100.");
        if (request.Pph26RatePercent is < 0m or > 100m) errors.Add("Tarif PPh Pasal 26 harus antara 0 dan 100.");

        var ptkp = (request.Ptkp ?? new List<PtkpEntry>())
            .Select(x => new PtkpEntry((x.Status ?? string.Empty).Trim().ToUpperInvariant(), x.AnnualAmount, (x.TerCategory ?? string.Empty).Trim().ToUpperInvariant()))
            .ToList();
        var terRates = (request.TerRates ?? new List<TerRateEntry>())
            .Select(x => new TerRateEntry((x.Category ?? string.Empty).Trim().ToUpperInvariant(), (x.PeriodKind ?? string.Empty).Trim().ToUpperInvariant(), x.LowerBound, x.UpperBound, x.RatePercent))
            .ToList();
        var brackets = (request.Brackets ?? new List<TaxBracketEntry>()).ToList();
        if (ptkp.Count > 200 || terRates.Count > 2000 || brackets.Count > 50) errors.Add("Jumlah baris tabel kebijakan melebihi batas.");

        if (errors.Count > 0) throw new PayrollRunValidationException(string.Join(" ", errors));

        entity.PolicyVersion = version!;
        entity.Title = title!;
        entity.LegalBasis = legalBasis!;
        entity.EffectiveStartDate = Utc(request.EffectiveStartDate);
        entity.EffectiveEndDate = request.EffectiveEndDate.HasValue ? Utc(request.EffectiveEndDate.Value) : null;
        entity.PositionCostRatePercent = positionCostRatePercent;
        entity.PositionCostMonthlyCap = positionCostMonthlyCap;
        entity.PositionCostAnnualCap = positionCostAnnualCap;
        entity.NoNpwpSurchargePercent = noNpwpSurchargePercent;
        entity.PkpRoundingUnit = pkpRoundingUnit;
        entity.RoundTaxToWholeRupiah = roundTaxToWholeRupiah;
        entity.EmployerPremiumPrograms = premiumPrograms.Count == 0 ? null : string.Join(",", premiumPrograms);
        entity.PensionDeductionPrograms = pensionPrograms.Count == 0 ? null : string.Join(",", pensionPrograms);
        entity.NonEmployeeBasisPercent = request.NonEmployeeBasisPercent;
        entity.NonEmployeeMethod = nonEmployeeMethod;
        entity.Pph26RatePercent = request.Pph26RatePercent;
        entity.PtkpJson = PayrollTaxPolicyEngine.SerializeTable(ptkp);
        entity.TerRatesJson = PayrollTaxPolicyEngine.SerializeTable(terRates);
        entity.BracketsJson = PayrollTaxPolicyEngine.SerializeTable(brackets);
        entity.Notes = Clean(request.Notes, 1000);
    }

    private static string DetermineTaxArticle(PrlTaxPolicy x)
    {
        if (x.Title.Contains("PPh 23", StringComparison.OrdinalIgnoreCase) ||
            x.Title.Contains("PPH23", StringComparison.OrdinalIgnoreCase) ||
            (x.Notes != null && x.Notes.Contains("PPH23", StringComparison.OrdinalIgnoreCase)))
        {
            return TaxArticles.Pph23;
        }
        return TaxArticles.Pph21;
    }

    private static string DetermineCalculationType(PrlTaxPolicy x)
    {
        var article = DetermineTaxArticle(x);
        if (article == TaxArticles.Pph23)
        {
            if (x.Title.Contains("Outsourcing", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.Pph23OutsourcingServices;
            if (x.Title.Contains("Manajemen", StringComparison.OrdinalIgnoreCase) || x.Title.Contains("Konsultasi", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.Pph23ManagementConsulting;
            if (x.Title.Contains("Teknik", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.Pph23TechnicalServices;
            if (x.Title.Contains("Sewa", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.Pph23RentAsset;
            if (x.Title.Contains("Royalti", StringComparison.OrdinalIgnoreCase) || x.Title.Contains("Bunga", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.Pph23RoyaltyInterest;
            return TaxPolicyCalculationTypes.Pph23OtherServicesPmk141;
        }

        if (x.Title.Contains("Dokter", StringComparison.OrdinalIgnoreCase) || x.Title.Contains("Tenaga Ahli", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.NonEmployeeExpert;
        if (x.Title.Contains("Honorarium", StringComparison.OrdinalIgnoreCase) || x.Title.Contains("Bukan Pegawai", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.NonEmployeeHonorarium;
        if (x.Title.Contains("Kontrak", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.ContractEmployeeTer;
        if (x.Title.Contains("Harian", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.NonPermanentDaily;
        if (x.Title.Contains("Tidak Tetap", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.NonPermanentMonthly;
        if (x.Title.Contains("Pasal 17", StringComparison.OrdinalIgnoreCase)) return TaxPolicyCalculationTypes.Pph21Pasal17;

        return TaxPolicyCalculationTypes.PermanentEmployeeTer;
    }

    private static string DetermineCategoryLabel(string calculationType) => calculationType switch
    {
        TaxPolicyCalculationTypes.PermanentEmployeeTer => "Pegawai Tetap (TER & Pasal 17)",
        TaxPolicyCalculationTypes.ContractEmployeeTer => "Pegawai Kontrak Kriteria Tetap",
        TaxPolicyCalculationTypes.NonPermanentMonthly => "Pegawai Tidak Tetap Bulanan",
        TaxPolicyCalculationTypes.NonPermanentDaily => "Pegawai Tidak Tetap Harian/Satuan",
        TaxPolicyCalculationTypes.NonEmployeeHonorarium => "Bukan Pegawai (Honorarium/Jasa)",
        TaxPolicyCalculationTypes.NonEmployeeExpert => "Tenaga Ahli (Dokter / Konsultan)",
        TaxPolicyCalculationTypes.Pph21Pasal17 => "Tarif Progresif Umum Pasal 17",
        TaxPolicyCalculationTypes.Pph23OutsourcingServices => "Jasa Outsourcing Badan Usaha (2%)",
        TaxPolicyCalculationTypes.Pph23ManagementConsulting => "Jasa Manajemen & Konsultasi (2%)",
        TaxPolicyCalculationTypes.Pph23TechnicalServices => "Jasa Teknik & Pemeliharaan (2%)",
        TaxPolicyCalculationTypes.Pph23OtherServicesPmk141 => "Jasa Lainnya PMK 141/2015 (2%)",
        TaxPolicyCalculationTypes.Pph23RentAsset => "Sewa Harta Selain Tanah/Bangunan (2%)",
        TaxPolicyCalculationTypes.Pph23RoyaltyInterest => "Royalti & Bunga Pinjaman (15%)",
        _ => "PPh 21 Pegawai Tetap"
    };

    private static string DetermineEffectiveStatus(PrlTaxPolicy x, DateTime today)
    {
        if (!x.IsActive || x.PolicyStatus == StatutoryPolicyStatuses.Retired)
            return TaxPolicyEffectiveStatuses.Inactive;
        if (x.EffectiveStartDate > today)
            return TaxPolicyEffectiveStatuses.Scheduled;
        if (x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < today)
            return TaxPolicyEffectiveStatuses.Expired;
        return TaxPolicyEffectiveStatuses.Active;
    }

    private static PayrollStatutoryPolicyListItemResponse ToListItem(PrlTaxPolicy x, DateTime today)
    {
        var taxArticle = DetermineTaxArticle(x);
        var calcType = DetermineCalculationType(x);
        var categoryLabel = DetermineCategoryLabel(calcType);
        var effectiveStatus = DetermineEffectiveStatus(x, today);

        return new PayrollStatutoryPolicyListItemResponse
        {
            Id = x.Id,
            PolicyVersion = x.PolicyVersion,
            Title = x.Title,
            TaxArticle = taxArticle,
            CalculationType = calcType,
            CategoryLabel = categoryLabel,
            EffectiveStatus = effectiveStatus,
            LegalBasis = x.LegalBasis,
            EffectiveStartDate = x.EffectiveStartDate,
            EffectiveEndDate = x.EffectiveEndDate,
            PolicyStatus = x.PolicyStatus,
            IsEffectiveToday = x.PolicyStatus == StatutoryPolicyStatuses.Approved && x.EffectiveStartDate <= today
                && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today) && x.IsActive,
            SubmittedAt = x.SubmittedAt,
            DecidedAt = x.DecidedAt,
            StatusReason = x.StatusReason
        };
    }

    private static PayrollTaxPolicyDetailResponse ToDetail(PrlTaxPolicy x, PayrollStatutoryPolicyCaller caller)
    {
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var summary = ToListItem(x, today);
        var view = PayrollTaxPolicy.FromEntity(x);
        var official = PayrollTaxOfficialReferenceProvider.GetStatus();
        var isPph23 = summary.TaxArticle == TaxArticles.Pph23;
        var completenessIssues = isPph23 ? new List<string>() : PayrollTaxPolicyEngine.ValidateCompleteness(view);

        return new PayrollTaxPolicyDetailResponse
        {
            Summary = summary,
            TaxArticle = summary.TaxArticle,
            CalculationType = summary.CalculationType ?? string.Empty,
            CalculationTypeLabel = summary.CategoryLabel ?? string.Empty,
            CalculationMethodLabel = isPph23 ? "Pemotongan PPh 23 atas Tagihan Vendor Badan Usaha" : official.CalculationMethodLabel,
            EffectiveStatus = summary.EffectiveStatus,
            RatePercent = isPph23 ? (summary.CalculationType == TaxPolicyCalculationTypes.Pph23RoyaltyInterest ? 15m : 2m) : null,
            DppBasis = isPph23 ? "Jumlah Bruto (tidak termasuk PPN)" : null,
            ReferenceVersion = isPph23 ? "-" : official.ReferenceVersion,
            ReferenceReady = isPph23 || (official.IsReady && completenessIssues.Count == 0),
            ReferenceStatusLabel = isPph23 ? "Kebijakan PPh 23 Aktif" : (completenessIssues.Count == 0 && official.IsReady ? "Referensi Tarif Resmi Terverifikasi" : "Referensi Tarif Belum Lengkap"),
            PositionCostRatePercent = x.PositionCostRatePercent,
            PositionCostMonthlyCap = x.PositionCostMonthlyCap,
            PositionCostAnnualCap = x.PositionCostAnnualCap,
            NoNpwpSurchargePercent = x.NoNpwpSurchargePercent,
            PkpRoundingUnit = x.PkpRoundingUnit,
            RoundTaxToWholeRupiah = x.RoundTaxToWholeRupiah,
            EmployerPremiumPrograms = x.EmployerPremiumPrograms,
            PensionDeductionPrograms = x.PensionDeductionPrograms,
            NonEmployeeBasisPercent = x.NonEmployeeBasisPercent,
            NonEmployeeMethod = x.NonEmployeeMethod,
            Pph26RatePercent = x.Pph26RatePercent,
            Ptkp = view.Ptkp.ToList(),
            TerRates = view.TerRates.ToList(),
            Brackets = view.Brackets.ToList(),
            Notes = x.Notes,
            CompletenessIssues = completenessIssues,
            Actions = caller.ToTaxResponse(x.PolicyStatus)
        };
    }

    /// <summary>Audit tanpa nominal tarif atau isi tabel: hanya identitas, versi, status, dan aktor.</summary>
    private Task AuditAsync(string action, string message, PrlTaxPolicy entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, message, new
        {
            entity.Id,
            entity.PolicyVersion,
            entity.PolicyStatus,
            entity.EffectiveStartDate,
            entity.EffectiveEndDate,
            ActorUserId = actorUserId
        });
}
