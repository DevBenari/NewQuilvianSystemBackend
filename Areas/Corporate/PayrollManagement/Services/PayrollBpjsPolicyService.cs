using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Features;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Kebijakan tarif dan batas upah iuran BPJS berversi (PrlBpjsPolicy). Nilai bawaan pada Pengaturan Penggajian (HrdPayrollSetting,
/// mis. JKK 0,89%) BUKAN kebijakan resmi: nilai itu hanya dapat disalin menjadi DRAFT yang wajib dilengkapi klasifikasi risiko JKK
/// beserta dasarnya, diverifikasi HRD (diajukan), lalu disahkan Finance. Hanya kebijakan APPROVED yang berlaku pada periode yang
/// dipakai mesin payroll, dan versinya disimpan pada snapshot iuran setiap penerima.
///
/// Aturan alur, pemisahan pengaju dan pengesah, pelarangan menyunting kebijakan yang sudah disahkan, dan larangan masa berlaku
/// beririsan sama dengan <see cref="PayrollTaxPolicyService"/>. Nominal tarif tidak ditulis ke log.
/// </summary>
public sealed class PayrollBpjsPolicyService
{
    private const string LogCategory = "Corporate.PayrollManagement.BpjsPolicy";
    private const string LockKey = "PAYROLL_BPJS_POLICY";
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly IBpjsFeatureGate _bpjsGate;

    public PayrollBpjsPolicyService(ApplicationDbContext dbContext, LoggerService loggerService, IBpjsFeatureGate bpjsGate)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _bpjsGate = bpjsGate;
    }

    /// <summary>
    /// Fitur BPJS nonaktif = tidak ada kebijakan BPJS baru atau perubahan status kebijakan (pertahanan berlapis: controller sudah
    /// menolak 409). Pembacaan tidak melewatinya. Riwayat kebijakan tidak disentuh.
    /// </summary>
    private void EnsureFeatureEnabled()
    {
        if (!_bpjsGate.IsEnabled) throw new PayrollPolicyNotConfirmedException(_bpjsGate.DisabledMessage);
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------------------------------------------

    public async Task<PayrollStatutoryPolicyPagedResponse<PayrollStatutoryPolicyListItemResponse>> ListAsync(
        PayrollStatutoryPolicyQuery query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var source = _dbContext.PrlBpjsPolicies.AsNoTracking().Where(x => !x.IsDelete);

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

    public async Task<PayrollBpjsPolicyDetailResponse> GetAsync(Guid id, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.PrlBpjsPolicies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kebijakan iuran BPJS tidak ditemukan.");
        return ToDetail(entity, caller);
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Tulis
    // ------------------------------------------------------------------------------------------------------------------------

    public Task<PayrollBpjsPolicyDetailResponse> CreateAsync(
        SavePayrollBpjsPolicyRequest request, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            EnsureFeatureEnabled();
            var entity = new PrlBpjsPolicy { Id = Guid.NewGuid(), CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId };
            await ApplyAsync(entity, request, cancellationToken);
            _dbContext.PrlBpjsPolicies.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("BpjsPolicy.Create", "Membuat draf kebijakan iuran BPJS.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    /// <summary>
    /// Menyalin nilai Pengaturan Penggajian yang aktif menjadi draf kebijakan. Klasifikasi risiko JKK dan dasarnya SENGAJA dikosongkan:
    /// nilai bawaan tidak dianggap resmi, HRD wajib menetapkan kelompok risiko rumah sakit dan dasarnya sebelum draf dapat diajukan.
    /// </summary>
    public Task<PayrollBpjsPolicyDetailResponse> CreateFromSettingAsync(
        CreatePayrollBpjsPolicyFromSettingRequest request, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            EnsureFeatureEnabled();
            var setting = await _dbContext.Set<HrdPayrollSetting>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Pengaturan Penggajian aktif tidak ditemukan sebagai sumber salinan.");

            var copy = new SavePayrollBpjsPolicyRequest
            {
                PolicyVersion = request.PolicyVersion,
                Title = "Draf dari Pengaturan Penggajian (belum diverifikasi)",
                LegalBasis = request.LegalBasis,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                JkkRiskClass = null,
                JkkRiskBasis = null,
                HealthCapAmount = setting.BpjsKesehatanCapAmount,
                HealthEmployeeRate = setting.BpjsKesehatanEmployeeRate,
                HealthEmployerRate = setting.BpjsKesehatanEmployerRate,
                JhtEmployeeRate = setting.BpjsKetenagakerjaanJhtEmployeeRate,
                JhtEmployerRate = setting.BpjsKetenagakerjaanJhtEmployerRate,
                JpCapAmount = setting.BpjsKetenagakerjaanJpCapAmount,
                JpEmployeeRate = setting.BpjsKetenagakerjaanJpEmployeeRate,
                JpEmployerRate = setting.BpjsKetenagakerjaanJpEmployerRate,
                JkkRate = setting.BpjsKetenagakerjaanJkkRate,
                JkmRate = setting.BpjsKetenagakerjaanJkmRate,
                Notes = "Disalin dari nilai bawaan Pengaturan Penggajian; belum diverifikasi terhadap ketentuan BPJS. Tetapkan kelompok risiko JKK dan dasarnya."
            };

            var entity = new PrlBpjsPolicy { Id = Guid.NewGuid(), CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId };
            await ApplyAsync(entity, copy, cancellationToken);
            _dbContext.PrlBpjsPolicies.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("BpjsPolicy.CreateFromSetting", "Membuat draf kebijakan iuran BPJS dari Pengaturan Penggajian.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollBpjsPolicyDetailResponse> UpdateAsync(
        Guid id, SavePayrollBpjsPolicyRequest request, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            EnsureFeatureEnabled();
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
            await AuditAsync("BpjsPolicy.Update", "Mengubah draf kebijakan iuran BPJS.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollBpjsPolicyDetailResponse> ReviseAsync(
        Guid id, PayrollStatutoryPolicyReviseRequest request, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            EnsureFeatureEnabled();
            var source = await LoadAsync(id, cancellationToken);
            var copy = new SavePayrollBpjsPolicyRequest
            {
                PolicyVersion = request.PolicyVersion,
                Title = source.Title,
                LegalBasis = source.LegalBasis,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                JkkRiskClass = source.JkkRiskClass,
                JkkRiskBasis = source.JkkRiskBasis,
                HealthCapAmount = source.HealthCapAmount,
                HealthEmployeeRate = source.HealthEmployeeRate,
                HealthEmployerRate = source.HealthEmployerRate,
                JhtEmployeeRate = source.JhtEmployeeRate,
                JhtEmployerRate = source.JhtEmployerRate,
                JpCapAmount = source.JpCapAmount,
                JpEmployeeRate = source.JpEmployeeRate,
                JpEmployerRate = source.JpEmployerRate,
                JkkRate = source.JkkRate,
                JkmRate = source.JkmRate,
                Notes = $"Revisi dari versi {source.PolicyVersion}."
            };

            var entity = new PrlBpjsPolicy { Id = Guid.NewGuid(), CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId };
            await ApplyAsync(entity, copy, cancellationToken);
            _dbContext.PrlBpjsPolicies.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("BpjsPolicy.Revise", "Membuat revisi draf kebijakan iuran BPJS.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollBpjsPolicyDetailResponse> SubmitAsync(
        Guid id, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            EnsureFeatureEnabled();
            var entity = await LoadAsync(id, cancellationToken);
            if (!StatutoryPolicyStatuses.IsEditable(entity.PolicyStatus))
                throw new PayrollInvalidTransitionException("Hanya draf atau kebijakan yang ditolak yang dapat diajukan.");

            var issues = ValidateCompleteness(entity);
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
            await AuditAsync("BpjsPolicy.Submit", "Mengajukan kebijakan iuran BPJS (diverifikasi HRD) untuk disahkan.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollBpjsPolicyDetailResponse> ApproveAsync(
        Guid id, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            EnsureFeatureEnabled();
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.PolicyStatus != StatutoryPolicyStatuses.PendingApproval)
                throw new PayrollInvalidTransitionException("Hanya kebijakan yang sudah diajukan yang dapat disahkan.");
            if (entity.SubmittedByUserId.HasValue && entity.SubmittedByUserId.Value == actorUserId)
                throw new PayrollForbiddenActionException("Pengaju tidak boleh mengesahkan kebijakan yang diajukannya sendiri; pengesahan oleh Finance.");

            var start = entity.EffectiveStartDate;
            var end = entity.EffectiveEndDate;
            var overlapQuery = _dbContext.PrlBpjsPolicies.AsNoTracking()
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
            await AuditAsync("BpjsPolicy.Approve", "Mengesahkan kebijakan iuran BPJS.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollBpjsPolicyDetailResponse> RejectAsync(
        Guid id, string? reason, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            EnsureFeatureEnabled();
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.PolicyStatus != StatutoryPolicyStatuses.PendingApproval)
                throw new PayrollInvalidTransitionException("Hanya kebijakan yang sudah diajukan yang dapat ditolak.");
            if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan penolakan wajib diisi.");

            entity.PolicyStatus = StatutoryPolicyStatuses.Rejected;
            entity.DecidedAt = DateTime.UtcNow;
            entity.DecidedByUserId = actorUserId;
            entity.StatusReason = Truncate(reason, 500);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("BpjsPolicy.Reject", "Menolak kebijakan iuran BPJS.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<PayrollBpjsPolicyDetailResponse> RetireAsync(
        Guid id, string? reason, Guid actorUserId, PayrollStatutoryPolicyCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            EnsureFeatureEnabled();
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.PolicyStatus != StatutoryPolicyStatuses.Approved)
                throw new PayrollInvalidTransitionException("Hanya kebijakan yang sudah disahkan yang dapat dipensiunkan.");
            if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan pensiun kebijakan wajib diisi.");

            entity.PolicyStatus = StatutoryPolicyStatuses.Retired;
            entity.RetiredAt = DateTime.UtcNow;
            entity.StatusReason = Truncate(reason, 500);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("BpjsPolicy.Retire", "Mempensiunkan kebijakan iuran BPJS.", entity, actorUserId);
            return ToDetail(entity, caller);
        }, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            EnsureFeatureEnabled();
            var entity = await LoadAsync(id, cancellationToken);
            if (!StatutoryPolicyStatuses.IsEditable(entity.PolicyStatus))
                throw new PayrollInvalidTransitionException("Hanya draf atau kebijakan yang ditolak yang dapat dihapus; kebijakan yang pernah disahkan dipensiunkan, bukan dihapus.");

            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("BpjsPolicy.Delete", "Menghapus draf kebijakan iuran BPJS.", entity, actorUserId);
            return true;
        }, cancellationToken);

    // ------------------------------------------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Kelengkapan isi kebijakan: klasifikasi risiko JKK beserta dasarnya wajib, plafon dan tarif wajib terisi. Kosong = dapat diajukan.
    /// Tarif 0 ditolak karena kebijakan resmi tidak berarti iuran nol; kepesertaan dikendalikan lewat Profil Iuran, bukan tarif nol.
    /// </summary>
    public static List<string> ValidateCompleteness(PrlBpjsPolicy x)
    {
        var errors = new List<string>();

        if (x.JkkRiskClass is null or < 1 or > 5) errors.Add("Kelompok risiko JKK (1 sampai 5) wajib ditetapkan.");
        if (string.IsNullOrWhiteSpace(x.JkkRiskBasis)) errors.Add("Dasar penetapan kelompok risiko JKK wajib diisi.");

        if (x.HealthCapAmount <= 0m) errors.Add("Plafon upah BPJS Kesehatan wajib diisi.");
        if (x.JpCapAmount <= 0m) errors.Add("Plafon upah BPJS JP wajib diisi.");

        void RequireRate(decimal value, string label)
        {
            if (value <= 0m || value > 100m) errors.Add($"Tarif {label} wajib diisi (lebih dari 0 dan paling banyak 100).");
        }

        RequireRate(x.HealthEmployeeRate, "BPJS Kesehatan pegawai");
        RequireRate(x.HealthEmployerRate, "BPJS Kesehatan pemberi kerja");
        RequireRate(x.JhtEmployeeRate, "JHT pegawai");
        RequireRate(x.JhtEmployerRate, "JHT pemberi kerja");
        RequireRate(x.JpEmployeeRate, "JP pegawai");
        RequireRate(x.JpEmployerRate, "JP pemberi kerja");
        RequireRate(x.JkkRate, "JKK");
        RequireRate(x.JkmRate, "JKM");

        return errors;
    }

    private async Task<PrlBpjsPolicy> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.PrlBpjsPolicies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Kebijakan iuran BPJS tidak ditemukan.");

    private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

    private static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Truncate(value, maxLength);
    }

    private static string Truncate(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }

    private async Task ApplyAsync(PrlBpjsPolicy entity, SavePayrollBpjsPolicyRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        var version = Clean(request.PolicyVersion, 100);
        if (version is null) errors.Add("Nomor versi kebijakan wajib diisi.");
        var title = Clean(request.Title, 200);
        if (title is null) errors.Add("Judul kebijakan wajib diisi.");
        var legalBasis = Clean(request.LegalBasis, 1000);
        if (legalBasis is null) errors.Add("Dasar penetapan wajib diisi.");
        if (request.EffectiveStartDate == default) errors.Add("Tanggal mulai berlaku wajib diisi.");
        if (request.EffectiveEndDate.HasValue && request.EffectiveStartDate != default
            && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date)
            errors.Add("Tanggal akhir berlaku tidak boleh sebelum tanggal mulai.");

        if (request.JkkRiskClass is < 1 or > 5) errors.Add("Kelompok risiko JKK harus antara 1 dan 5.");
        if (request.HealthCapAmount < 0m || request.JpCapAmount < 0m) errors.Add("Plafon upah tidak boleh negatif.");

        foreach (var (value, label) in new[]
        {
            (request.HealthEmployeeRate, "BPJS Kesehatan pegawai"), (request.HealthEmployerRate, "BPJS Kesehatan pemberi kerja"),
            (request.JhtEmployeeRate, "JHT pegawai"), (request.JhtEmployerRate, "JHT pemberi kerja"),
            (request.JpEmployeeRate, "JP pegawai"), (request.JpEmployerRate, "JP pemberi kerja"),
            (request.JkkRate, "JKK"), (request.JkmRate, "JKM")
        })
        {
            if (value < 0m || value > 100m) errors.Add($"Tarif {label} harus antara 0 dan 100.");
        }

        if (version is not null)
        {
            var duplicate = await _dbContext.PrlBpjsPolicies.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.Id != entity.Id && x.PolicyVersion == version, cancellationToken);
            if (duplicate) errors.Add($"Nomor versi {version} sudah dipakai kebijakan lain.");
        }

        if (errors.Count > 0) throw new PayrollRunValidationException(string.Join(" ", errors));

        entity.PolicyVersion = version!;
        entity.Title = title!;
        entity.LegalBasis = legalBasis!;
        entity.EffectiveStartDate = Utc(request.EffectiveStartDate);
        entity.EffectiveEndDate = request.EffectiveEndDate.HasValue ? Utc(request.EffectiveEndDate.Value) : null;
        entity.JkkRiskClass = request.JkkRiskClass;
        entity.JkkRiskBasis = Clean(request.JkkRiskBasis, 1000);
        entity.HealthCapAmount = request.HealthCapAmount;
        entity.HealthEmployeeRate = request.HealthEmployeeRate;
        entity.HealthEmployerRate = request.HealthEmployerRate;
        entity.JhtEmployeeRate = request.JhtEmployeeRate;
        entity.JhtEmployerRate = request.JhtEmployerRate;
        entity.JpCapAmount = request.JpCapAmount;
        entity.JpEmployeeRate = request.JpEmployeeRate;
        entity.JpEmployerRate = request.JpEmployerRate;
        entity.JkkRate = request.JkkRate;
        entity.JkmRate = request.JkmRate;
        entity.Notes = Clean(request.Notes, 1000);
    }

    private static PayrollStatutoryPolicyListItemResponse ToListItem(PrlBpjsPolicy x, DateTime today) => new()
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

    private static PayrollBpjsPolicyDetailResponse ToDetail(PrlBpjsPolicy x, PayrollStatutoryPolicyCaller caller)
    {
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        return new PayrollBpjsPolicyDetailResponse
        {
            Summary = ToListItem(x, today),
            JkkRiskClass = x.JkkRiskClass,
            JkkRiskBasis = x.JkkRiskBasis,
            HealthCapAmount = x.HealthCapAmount,
            HealthEmployeeRate = x.HealthEmployeeRate,
            HealthEmployerRate = x.HealthEmployerRate,
            JhtEmployeeRate = x.JhtEmployeeRate,
            JhtEmployerRate = x.JhtEmployerRate,
            JpCapAmount = x.JpCapAmount,
            JpEmployeeRate = x.JpEmployeeRate,
            JpEmployerRate = x.JpEmployerRate,
            JkkRate = x.JkkRate,
            JkmRate = x.JkmRate,
            Notes = x.Notes,
            CompletenessIssues = ValidateCompleteness(x),
            Actions = caller.ToResponse(x.PolicyStatus)
        };
    }

    private Task AuditAsync(string action, string message, PrlBpjsPolicy entity, Guid actorUserId) =>
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
