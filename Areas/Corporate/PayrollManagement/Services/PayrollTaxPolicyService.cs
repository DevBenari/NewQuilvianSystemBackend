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
        var source = _dbContext.PrlTaxPolicies.AsNoTracking().Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToUpperInvariant();
            source = source.Where(x => x.PolicyStatus == status);
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

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
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
            _dbContext.PrlTaxPolicies.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Create", "Membuat draf kebijakan tarif PPh.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollTaxPolicyDetailResponse> UpdateAsync(
        Guid id, SavePayrollTaxPolicyRequest request, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            if (!StatutoryPolicyStatuses.IsEditable(entity.PolicyStatus))
                throw new PayrollInvalidTransitionException(
                    "Hanya draf atau kebijakan yang ditolak yang dapat diubah. Untuk mengubah kebijakan yang sudah diajukan atau disahkan, buat revisi (versi baru).");

            await ApplyAsync(entity, request, cancellationToken);
            entity.PolicyStatus = StatutoryPolicyStatuses.Draft;
            entity.StatusReason = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Update", "Mengubah draf kebijakan tarif PPh.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    /// <summary>Membuat draf versi baru dari isi kebijakan yang ada (mis. saat tarif berubah). Kebijakan asal tidak diubah.</summary>
    public Task<PayrollTaxPolicyDetailResponse> ReviseAsync(
        Guid id, PayrollStatutoryPolicyReviseRequest request, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var source = await LoadAsync(id, cancellationToken);
            var copy = new SavePayrollTaxPolicyRequest
            {
                PolicyVersion = request.PolicyVersion,
                Title = source.Title,
                LegalBasis = source.LegalBasis,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                PositionCostRatePercent = source.PositionCostRatePercent,
                PositionCostMonthlyCap = source.PositionCostMonthlyCap,
                PositionCostAnnualCap = source.PositionCostAnnualCap,
                NoNpwpSurchargePercent = source.NoNpwpSurchargePercent,
                PkpRoundingUnit = source.PkpRoundingUnit,
                RoundTaxToWholeRupiah = source.RoundTaxToWholeRupiah,
                EmployerPremiumPrograms = source.EmployerPremiumPrograms,
                PensionDeductionPrograms = source.PensionDeductionPrograms,
                NonEmployeeBasisPercent = source.NonEmployeeBasisPercent,
                NonEmployeeMethod = source.NonEmployeeMethod,
                Pph26RatePercent = source.Pph26RatePercent,
                Ptkp = PayrollTaxPolicyEngine.ParseTable<PtkpEntry>(source.PtkpJson).ToList(),
                TerRates = PayrollTaxPolicyEngine.ParseTable<TerRateEntry>(source.TerRatesJson).ToList(),
                Brackets = PayrollTaxPolicyEngine.ParseTable<TaxBracketEntry>(source.BracketsJson).ToList(),
                Notes = $"Revisi dari versi {source.PolicyVersion}."
            };

            var entity = new PrlTaxPolicy { Id = Guid.NewGuid(), CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId };
            await ApplyAsync(entity, copy, cancellationToken);
            _dbContext.PrlTaxPolicies.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Revise", "Membuat revisi draf kebijakan tarif PPh.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollTaxPolicyDetailResponse> SubmitAsync(
        Guid id, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            if (!StatutoryPolicyStatuses.IsEditable(entity.PolicyStatus))
                throw new PayrollInvalidTransitionException("Hanya draf atau kebijakan yang ditolak yang dapat diajukan.");

            var issues = PayrollTaxPolicyEngine.ValidateCompleteness(PayrollTaxPolicy.FromEntity(entity));
            if (issues.Count > 0)
                throw new PayrollRunValidationException("Kebijakan belum lengkap dan tidak dapat diajukan: " + string.Join(" ", issues));

            entity.PolicyStatus = StatutoryPolicyStatuses.PendingApproval;
            entity.SubmittedAt = DateTime.UtcNow;
            entity.SubmittedByUserId = actorUserId;
            entity.DecidedAt = null;
            entity.DecidedByUserId = null;
            entity.StatusReason = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Submit", "Mengajukan kebijakan tarif PPh (diverifikasi HRD) untuk disahkan.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollTaxPolicyDetailResponse> ApproveAsync(
        Guid id, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.PolicyStatus != StatutoryPolicyStatuses.PendingApproval)
                throw new PayrollInvalidTransitionException("Hanya kebijakan yang sudah diajukan yang dapat disahkan.");
            if (entity.SubmittedByUserId.HasValue && entity.SubmittedByUserId.Value == actorUserId)
                throw new PayrollForbiddenActionException("Pengaju tidak boleh mengesahkan kebijakan yang diajukannya sendiri; pengesahan oleh Finance.");

            var start = entity.EffectiveStartDate;
            var end = entity.EffectiveEndDate;
            var overlapQuery = _dbContext.PrlTaxPolicies.AsNoTracking()
                .Where(x => !x.IsDelete && x.Id != entity.Id && x.PolicyStatus == StatutoryPolicyStatuses.Approved
                    && (x.EffectiveEndDate == null || x.EffectiveEndDate >= start));
            if (end.HasValue) overlapQuery = overlapQuery.Where(x => x.EffectiveStartDate <= end.Value);
            var overlapping = await overlapQuery.Select(x => x.PolicyVersion).FirstOrDefaultAsync(cancellationToken);
            if (overlapping is not null)
                throw new PayrollRunConflictException(
                    $"Masa berlaku beririsan dengan kebijakan yang sudah disahkan ({overlapping}). Pensiunkan kebijakan itu atau sesuaikan tanggal berlaku.");

            entity.PolicyStatus = StatutoryPolicyStatuses.Approved;
            entity.DecidedAt = DateTime.UtcNow;
            entity.DecidedByUserId = actorUserId;
            entity.StatusReason = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Approve", "Mengesahkan kebijakan tarif PPh.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollTaxPolicyDetailResponse> RejectAsync(
        Guid id, string? reason, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.PolicyStatus != StatutoryPolicyStatuses.PendingApproval)
                throw new PayrollInvalidTransitionException("Hanya kebijakan yang sudah diajukan yang dapat ditolak.");
            if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan penolakan wajib diisi.");

            entity.PolicyStatus = StatutoryPolicyStatuses.Rejected;
            entity.DecidedAt = DateTime.UtcNow;
            entity.DecidedByUserId = actorUserId;
            entity.StatusReason = reason.Trim().Length > 500 ? reason.Trim()[..500] : reason.Trim();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Reject", "Menolak kebijakan tarif PPh.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    /// <summary>Mempensiunkan kebijakan yang sudah disahkan. Run yang sudah menyimpan snapshot versinya tidak berubah.</summary>
    public Task<PayrollTaxPolicyDetailResponse> RetireAsync(
        Guid id, string? reason, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.PolicyStatus != StatutoryPolicyStatuses.Approved)
                throw new PayrollInvalidTransitionException("Hanya kebijakan yang sudah disahkan yang dapat dipensiunkan.");
            if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan pensiun kebijakan wajib diisi.");

            entity.PolicyStatus = StatutoryPolicyStatuses.Retired;
            entity.RetiredAt = DateTime.UtcNow;
            entity.StatusReason = reason.Trim().Length > 500 ? reason.Trim()[..500] : reason.Trim();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Retire", "Mempensiunkan kebijakan tarif PPh.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    /// <summary>Menghapus (soft delete) draf atau kebijakan yang ditolak yang belum pernah disahkan.</summary>
    public Task<bool> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            var entity = await LoadAsync(id, cancellationToken);
            if (!StatutoryPolicyStatuses.IsEditable(entity.PolicyStatus))
                throw new PayrollInvalidTransitionException("Hanya draf atau kebijakan yang ditolak yang dapat dihapus; kebijakan yang pernah disahkan dipensiunkan, bukan dihapus.");

            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("TaxPolicy.Delete", "Menghapus draf kebijakan tarif PPh.", entity, actorUserId);
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

    /// <summary>Memvalidasi masukan dasar (cukup untuk menyimpan draf) dan menyalinnya ke entity. Kelengkapan penuh diperiksa saat diajukan.</summary>
    private async Task ApplyAsync(PrlTaxPolicy entity, SavePayrollTaxPolicyRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        var version = Clean(request.PolicyVersion, 100);
        if (version is null) errors.Add("Nomor versi kebijakan wajib diisi.");
        var title = Clean(request.Title, 200);
        if (title is null) errors.Add("Judul kebijakan wajib diisi.");
        var legalBasis = Clean(request.LegalBasis, 1000);
        if (legalBasis is null) errors.Add("Dasar peraturan wajib diisi.");
        if (request.EffectiveStartDate == default) errors.Add("Tanggal mulai berlaku wajib diisi.");
        if (request.EffectiveEndDate.HasValue && request.EffectiveStartDate != default
            && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date)
            errors.Add("Tanggal akhir berlaku tidak boleh sebelum tanggal mulai.");

        if (request.PositionCostRatePercent < 0m || request.PositionCostRatePercent > 100m) errors.Add("Persen biaya jabatan harus antara 0 dan 100.");
        if (request.PositionCostMonthlyCap < 0m || request.PositionCostAnnualCap < 0m) errors.Add("Batas biaya jabatan tidak boleh negatif.");
        if (request.NoNpwpSurchargePercent < 0m || request.NoNpwpSurchargePercent > 1000m) errors.Add("Tambahan persen tanpa NPWP tidak valid.");
        if (request.PkpRoundingUnit < 0m) errors.Add("Kelipatan pembulatan PKP tidak boleh negatif.");

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

        if (version is not null)
        {
            var duplicate = await _dbContext.PrlTaxPolicies.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.Id != entity.Id && x.PolicyVersion == version, cancellationToken);
            if (duplicate) errors.Add($"Nomor versi {version} sudah dipakai kebijakan lain.");
        }

        if (errors.Count > 0) throw new PayrollRunValidationException(string.Join(" ", errors));

        entity.PolicyVersion = version!;
        entity.Title = title!;
        entity.LegalBasis = legalBasis!;
        entity.EffectiveStartDate = Utc(request.EffectiveStartDate);
        entity.EffectiveEndDate = request.EffectiveEndDate.HasValue ? Utc(request.EffectiveEndDate.Value) : null;
        entity.PositionCostRatePercent = request.PositionCostRatePercent;
        entity.PositionCostMonthlyCap = request.PositionCostMonthlyCap;
        entity.PositionCostAnnualCap = request.PositionCostAnnualCap;
        entity.NoNpwpSurchargePercent = request.NoNpwpSurchargePercent;
        entity.PkpRoundingUnit = request.PkpRoundingUnit;
        entity.RoundTaxToWholeRupiah = request.RoundTaxToWholeRupiah;
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

    private static PayrollStatutoryPolicyListItemResponse ToListItem(PrlTaxPolicy x, DateTime today) => new()
    {
        Id = x.Id,
        PolicyVersion = x.PolicyVersion,
        Title = x.Title,
        LegalBasis = x.LegalBasis,
        EffectiveStartDate = x.EffectiveStartDate,
        EffectiveEndDate = x.EffectiveEndDate,
        PolicyStatus = x.PolicyStatus,
        IsEffectiveToday = x.PolicyStatus == StatutoryPolicyStatuses.Approved && x.EffectiveStartDate <= today
            && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today),
        SubmittedAt = x.SubmittedAt,
        DecidedAt = x.DecidedAt,
        StatusReason = x.StatusReason
    };

    private static PayrollTaxPolicyDetailResponse ToDetail(PrlTaxPolicy x, PayrollStatutoryPolicyCaller caller)
    {
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var view = PayrollTaxPolicy.FromEntity(x);
        return new PayrollTaxPolicyDetailResponse
        {
            Summary = ToListItem(x, today),
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
            CompletenessIssues = PayrollTaxPolicyEngine.ValidateCompleteness(view),
            Actions = caller.ToResponse(x.PolicyStatus)
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
