using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Tunjangan Pegawai pada Detail Profil Penggajian: penetapan tunjangan tetap KHUSUS satu pegawai atau dokter berbasis Master
/// Tunjangan, ditampilkan bersama tunjangan yang diwarisi dari Struktur Gaji dan tunjangan berbasis kebijakan (transportasi).
///
/// Pemilik data adalah WorkforceProfileId: tidak membaca dan tidak membuat <c>PrlPayroll</c>, sehingga tunjangan dapat disiapkan sebelum
/// profil operasional payroll ada. Master Tunjangan, Struktur Gaji, dan tunjangan transportasi TIDAK diubah dari sini.
///
/// Aturan yang dijaga Backend (klien tidak dipercaya untuk kategori, metode, mata uang, syarat pajak/prorata, atau persetujuan):
/// - jenis tunjangan harus ada, aktif, berulang (IsRecurring), bermetode Fixed atau Percentage, dan bukan tunjangan yang dihitung dari
///   aktivitas/kebijakan (shift, siaga, risiko, transportasi) atau input manual per periode: itu lewat jalurnya masing-masing;
/// - nominal wajib &gt; 0 dan tidak melebihi batas maksimum master; persen di atas 0 dan paling tinggi 100;
/// - tanggal berlaku harus berada dalam masa berlaku master; tidak boleh beririsan dengan penetapan hidup (Draft/PendingApproval/Approved)
///   jenis yang sama pada pegawai yang sama; tidak boleh menduplikasi tunjangan yang sudah ada di Struktur Gaji pegawai;
/// - RequiresApproval master menentukan status awal: butuh persetujuan -> PendingApproval, tidak butuh -> Approved otomatis. Hanya
///   Approved yang dibayarkan mesin payroll (PayrollCalculationService). Persetujuan memakai izin Approve sendiri. Selama
///   Payroll:Policy:RequireDifferentApprover bernilai false (nilai bawaan; keputusan pemisahan pengaju dan pengesah belum disahkan),
///   pengaju boleh menyetujui sendiri, sama dengan penghasilan variabel manual (PAY-BE-20). Bila pemilik proses menyalakannya,
///   pengaju ditolak (403) menyetujui penetapannya sendiri; nilai bawaan tidak diubah oleh kode (PAY-BE-FIX-008);
/// - penetapan yang sudah pernah menjadi komponen payroll run tidak lagi boleh diubah isinya (riwayat tidak dihapus dan snapshot run
///   final tidak tersentuh); koreksinya adalah menonaktifkan lalu membuat penetapan baru.
///
/// Penulisan berjalan di transaksi Serializable dengan advisory lock per pegawai, sehingga dua permintaan bersamaan (klik ganda,
/// dua petugas) tidak dapat sama-sama lolos pemeriksaan tumpang-tindih. Nominal dan persen tidak pernah ditulis ke log.
/// </summary>
public sealed partial class PayrollWorkforceAllowanceService
{
    private const string LogCategory = "Corporate.PayrollManagement.WorkforceAllowance";
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly PayrollPolicyOptions _policy;

    public PayrollWorkforceAllowanceService(
        ApplicationDbContext dbContext, LoggerService loggerService, IOptions<PayrollPolicyOptions> policyOptions)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _policy = policyOptions.Value;
    }

    private sealed record TypeFacts(
        Guid Id,
        Guid? PayrollComponentId,
        string Code,
        string Name,
        string Category,
        string Method,
        string CurrencyCode,
        decimal DefaultAmount,
        decimal DefaultPercentage,
        decimal? MaximumAmount,
        bool IsRecurring,
        bool IsTaxable,
        bool IsProrated,
        bool RequiresApproval,
        DateTime? EffectiveStartDate,
        DateTime? EffectiveEndDate);

    private sealed record IndividualRow(
        Guid Id,
        Guid AllowanceTypeId,
        string TypeCode,
        string TypeName,
        string? TypeCategory,
        string CalculationMethod,
        decimal Amount,
        decimal Percentage,
        string CurrencyCode,
        bool IsTaxable,
        bool IsProrated,
        bool RequiresApproval,
        decimal? MaximumAmount,
        DateTime EffectiveStartDate,
        DateTime? EffectiveEndDate,
        string Status,
        string? StatusReason,
        DateTime? SubmittedAt,
        DateTime? DecidedAt,
        string? Notes,
        bool IsActive,
        DateTime CreateDateTime,
        DateTime? UpdateDateTime,
        string AssignmentSource);

    private sealed record SalaryBasis(
        Guid AssignmentId, Guid SalaryStructureId, decimal BaseSalary, bool Approved, DateTime EffectiveStartDate, DateTime? EffectiveEndDate);

    // ------------------------------------------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------------------------------------------

    public async Task<PagedResult<WorkforceAllowanceItemResponse>> GetPagedAsync(
        Guid workforceProfileId, WorkforceAllowanceQuery query, WorkforceAllowanceCaller caller, CancellationToken cancellationToken)
    {
        await EnsureWorkforceExistsAsync(workforceProfileId, cancellationToken);
        var loaded = await LoadItemsAsync(workforceProfileId, caller, cancellationToken);

        IEnumerable<WorkforceAllowanceItemResponse> rows = loaded.Items;
        if (!string.IsNullOrWhiteSpace(query.Source))
            rows = rows.Where(x => string.Equals(x.SourceType, query.Source.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(query.Status))
            rows = rows.Where(x => string.Equals(x.Status, query.Status.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim();
            rows = rows.Where(x =>
                x.AllowanceTypeName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.AllowanceTypeCode.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = rows.ToList();
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        return new PagedResult<WorkforceAllowanceItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = filtered.Count,
            TotalPage = (int)Math.Ceiling(filtered.Count / (double)pageSize),
            Items = filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
        };
    }

    public async Task<WorkforceAllowanceSummaryResponse> GetSummaryAsync(
        Guid workforceProfileId, WorkforceAllowanceCaller caller, CancellationToken cancellationToken)
    {
        await EnsureWorkforceExistsAsync(workforceProfileId, cancellationToken);
        var loaded = await LoadItemsAsync(workforceProfileId, caller, cancellationToken);
        var effective = loaded.Items.Where(x => x.IsEffectiveNow).ToList();

        var summary = new WorkforceAllowanceSummaryResponse
        {
            WorkforceProfileId = workforceProfileId,
            AmountsVisible = caller.CanViewAmount,
            ActiveCount = effective.Count,
            PendingApprovalCount = loaded.Items.Count(x => x.Status == WorkforceAllowanceStatuses.PendingApproval),
            SalaryStructureCount = loaded.Items.Count(x => x.SourceType == WorkforceAllowanceSourceTypes.SalaryStructure),
            IndividualCount = loaded.Items.Count(x => x.SourceType == WorkforceAllowanceSourceTypes.Individual),
            PolicyCount = loaded.Items.Count(x => x.SourceType == WorkforceAllowanceSourceTypes.Policy),
            HasSalaryAssignment = loaded.HasSalaryAssignment,
            HasTransportAllowance = loaded.HasTransportAllowance
        };

        if (caller.CanViewAmount)
            summary.TotalEstimatedMonthlyAmount = effective.Where(x => x.EstimatedMonthlyAmount.HasValue).Sum(x => x.EstimatedMonthlyAmount!.Value);

        summary.Notes.Add(
            "Tunjangan variabel per periode (insentif, bonus, honor) dan tunjangan berbasis aktivitas seperti shift, siaga, dan risiko "
            + "dimasukkan pada Payroll Run lewat Penghasilan Variabel, bukan di sini.");
        if (!loaded.HasSalaryAssignment)
            summary.Notes.Add("Belum ada penetapan gaji utama yang berlaku. Tunjangan Struktur Gaji dan perkiraan berbasis persentase belum dapat ditampilkan.");
        if (caller.CanViewAmount && effective.Any(x => !x.EstimatedMonthlyAmount.HasValue))
            summary.Notes.Add("Total estimasi belum memuat tunjangan yang nilainya ditentukan kebijakan atau belum punya gaji pokok yang disetujui.");

        return summary;
    }

    public async Task<WorkforceAllowanceItemResponse> GetAsync(
        Guid workforceProfileId, Guid id, WorkforceAllowanceCaller caller, CancellationToken cancellationToken)
    {
        await EnsureWorkforceExistsAsync(workforceProfileId, cancellationToken);
        return await GetItemAsync(workforceProfileId, id, caller, cancellationToken);
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Tulis
    // ------------------------------------------------------------------------------------------------------------------------

    public Task<WorkforceAllowanceItemResponse> CreateAsync(
        Guid workforceProfileId,
        CreateWorkforceAllowanceRequest request,
        Guid actorUserId,
        WorkforceAllowanceCaller caller,
        CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey(workforceProfileId), async () =>
        {
            await EnsureWorkforceExistsAsync(workforceProfileId, cancellationToken);
            var type = await LoadEligibleTypeAsync(request.AllowanceTypeId, cancellationToken);

            var start = PayrollDateNormalizer.ToUtcDate(request.EffectiveStartDate);
            var end = PayrollDateNormalizer.ToUtcDate(request.EffectiveEndDate);
            ValidateDates(type, start, end);
            var (method, amount, percentage) = ResolveValues(type, request.Amount, request.Percentage);
            await EnsureNoOverlapAsync(workforceProfileId, type, excludeId: null, start, end, cancellationToken);
            await EnsureNotInSalaryStructureAsync(workforceProfileId, type, start, end, cancellationToken);

            var now = DateTime.UtcNow;
            var entity = new PrlWorkforceAllowance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
            ApplyTerms(entity, type, method, amount, percentage, start, end, request.Notes);
            StartLifecycle(entity, type, actorUserId, now);

            _dbContext.PrlWorkforceAllowances.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await AuditAsync("WorkforceAllowance.Create", "Penetapan tunjangan pegawai dibuat.", entity, previousStatus: null, actorUserId, reason: null);
            return await GetItemAsync(workforceProfileId, entity.Id, caller, cancellationToken);
        }, cancellationToken);

    public Task<WorkforceAllowanceItemResponse> UpdateAsync(
        Guid workforceProfileId,
        Guid id,
        UpdateWorkforceAllowanceRequest request,
        Guid actorUserId,
        WorkforceAllowanceCaller caller,
        CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey(workforceProfileId), async () =>
        {
            var entity = await FindTrackedAsync(workforceProfileId, id, cancellationToken);
            PayrollVersionToken.EnsureMatches(request.ExpectedRowVersion, entity.CreateDateTime, entity.UpdateDateTime, "Penetapan tunjangan ini");

            if (request.AllowanceTypeId != entity.AllowanceTypeId)
                throw new ArgumentException("Jenis tunjangan tidak dapat diganti. Nonaktifkan penetapan ini lalu buat penetapan baru.");
            if (entity.AllowanceStatus == WorkforceAllowanceStatuses.Inactive)
                throw new PayrollInvalidTransitionException("Penetapan yang sudah dinonaktifkan tidak dapat diubah.");
            if (entity.AssignmentSource == WorkforceAllowanceAssignmentSources.Policy)
                throw new PayrollInvalidTransitionException(
                    "Penetapan dari Kebijakan Tunjangan tidak dapat diubah nilainya. Nonaktifkan lalu tetapkan ulang dari kebijakan.");
            if (await IsUsedInPayrollAsync(entity.Id, cancellationToken))
                throw new PayrollInvalidTransitionException(
                    "Penetapan ini sudah dipakai payroll run, jadi isinya tidak dapat diubah. Nonaktifkan lalu buat penetapan baru.");

            var type = await LoadEligibleTypeAsync(entity.AllowanceTypeId, cancellationToken);
            var start = PayrollDateNormalizer.ToUtcDate(request.EffectiveStartDate);
            var end = PayrollDateNormalizer.ToUtcDate(request.EffectiveEndDate);
            ValidateDates(type, start, end);
            var (method, amount, percentage) = ResolveValues(type, request.Amount, request.Percentage);
            await EnsureNoOverlapAsync(workforceProfileId, type, entity.Id, start, end, cancellationToken);
            await EnsureNotInSalaryStructureAsync(workforceProfileId, type, start, end, cancellationToken);

            var previousStatus = entity.AllowanceStatus;
            var now = DateTime.UtcNow;
            ApplyTerms(entity, type, method, amount, percentage, start, end, request.Notes);
            // Mengubah syarat membatalkan persetujuan sebelumnya: jenis yang butuh persetujuan kembali ke Draft dan harus diajukan ulang;
            // jenis yang tidak butuh persetujuan tetap Approved otomatis.
            ResetDecision(entity);
            if (type.RequiresApproval)
            {
                entity.AllowanceStatus = WorkforceAllowanceStatuses.Draft;
                entity.IsActive = true;
            }
            else
            {
                StartLifecycle(entity, type, actorUserId, now);
            }
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await AuditAsync("WorkforceAllowance.Update", "Penetapan tunjangan pegawai diperbarui.", entity, previousStatus, actorUserId, reason: null);
            return await GetItemAsync(workforceProfileId, entity.Id, caller, cancellationToken);
        }, cancellationToken);

    /// <summary>Draft atau Rejected -> PendingApproval. Hanya untuk jenis yang memerlukan persetujuan.</summary>
    public Task<WorkforceAllowanceItemResponse> SubmitAsync(
        Guid workforceProfileId, Guid id, WorkforceAllowanceActionRequest request, Guid actorUserId, WorkforceAllowanceCaller caller,
        CancellationToken cancellationToken) =>
        TransitionAsync(workforceProfileId, id, request, actorUserId, caller, "WorkforceAllowance.Submit", "Penetapan tunjangan diajukan untuk persetujuan.",
            (entity, now) =>
            {
                if (!entity.RequiresApproval)
                    throw new PayrollInvalidTransitionException("Jenis tunjangan ini tidak memerlukan persetujuan.");
                if (entity.AllowanceStatus is not (WorkforceAllowanceStatuses.Draft or WorkforceAllowanceStatuses.Rejected))
                    throw new PayrollInvalidTransitionException("Hanya penetapan berstatus Draft atau Ditolak yang dapat diajukan.");

                ResetDecision(entity);
                entity.AllowanceStatus = WorkforceAllowanceStatuses.PendingApproval;
                entity.IsActive = true;
                entity.SubmittedAt = now;
                entity.SubmittedByUserId = actorUserId;
            },
            revalidate: false,
            cancellationToken);

    /// <summary>PendingApproval -> Approved. Pemeriksaan jenis aktif, tumpang-tindih, dan duplikasi struktur diulang karena waktu telah berlalu.</summary>
    public Task<WorkforceAllowanceItemResponse> ApproveAsync(
        Guid workforceProfileId, Guid id, WorkforceAllowanceActionRequest request, Guid actorUserId, WorkforceAllowanceCaller caller,
        CancellationToken cancellationToken) =>
        TransitionAsync(workforceProfileId, id, request, actorUserId, caller, "WorkforceAllowance.Approve", "Penetapan tunjangan disetujui.",
            (entity, now) =>
            {
                if (entity.AllowanceStatus != WorkforceAllowanceStatuses.PendingApproval)
                    throw new PayrollInvalidTransitionException("Hanya penetapan berstatus Menunggu Persetujuan yang dapat disetujui.");
                // NEEDS CONFIRMATION (matriks maker-checker kompensasi): pemisahan baru berlaku bila kebijakan menyalakannya.
                if (_policy.RequireDifferentApprover && entity.SubmittedByUserId.HasValue && entity.SubmittedByUserId.Value == actorUserId)
                    throw new PayrollForbiddenActionException(
                        "Pengaju penetapan tunjangan tidak boleh menyetujui penetapan yang sama (kebijakan pemisahan kewenangan).");

                entity.AllowanceStatus = WorkforceAllowanceStatuses.Approved;
                entity.IsActive = true;
                entity.DecidedAt = now;
                entity.DecidedByUserId = actorUserId;
                entity.StatusReason = NormalizeText(request.Reason);
            },
            revalidate: true,
            cancellationToken);

    /// <summary>PendingApproval -> Rejected, alasan wajib.</summary>
    public Task<WorkforceAllowanceItemResponse> RejectAsync(
        Guid workforceProfileId, Guid id, WorkforceAllowanceActionRequest request, Guid actorUserId, WorkforceAllowanceCaller caller,
        CancellationToken cancellationToken) =>
        TransitionAsync(workforceProfileId, id, request, actorUserId, caller, "WorkforceAllowance.Reject", "Penetapan tunjangan ditolak.",
            (entity, now) =>
            {
                if (entity.AllowanceStatus != WorkforceAllowanceStatuses.PendingApproval)
                    throw new PayrollInvalidTransitionException("Hanya penetapan berstatus Menunggu Persetujuan yang dapat ditolak.");
                var reason = NormalizeText(request.Reason) ?? throw new ArgumentException("Alasan penolakan wajib diisi.");

                entity.AllowanceStatus = WorkforceAllowanceStatuses.Rejected;
                entity.IsActive = false;
                entity.DecidedAt = now;
                entity.DecidedByUserId = actorUserId;
                entity.StatusReason = reason;
            },
            revalidate: false,
            cancellationToken);

    /// <summary>Draft, PendingApproval, Approved, atau Rejected -> Inactive, alasan wajib. Riwayat dan komponen payroll lama tetap utuh.</summary>
    public Task<WorkforceAllowanceItemResponse> DeactivateAsync(
        Guid workforceProfileId, Guid id, WorkforceAllowanceActionRequest request, Guid actorUserId, WorkforceAllowanceCaller caller,
        CancellationToken cancellationToken) =>
        TransitionAsync(workforceProfileId, id, request, actorUserId, caller, "WorkforceAllowance.Deactivate", "Penetapan tunjangan dinonaktifkan.",
            (entity, now) =>
            {
                if (entity.AllowanceStatus == WorkforceAllowanceStatuses.Inactive)
                    throw new PayrollInvalidTransitionException("Penetapan ini sudah dinonaktifkan.");
                var reason = NormalizeText(request.Reason) ?? throw new ArgumentException("Alasan penonaktifan wajib diisi.");

                entity.AllowanceStatus = WorkforceAllowanceStatuses.Inactive;
                entity.IsActive = false;
                entity.StatusReason = reason;
            },
            revalidate: false,
            cancellationToken);

    private Task<WorkforceAllowanceItemResponse> TransitionAsync(
        Guid workforceProfileId,
        Guid id,
        WorkforceAllowanceActionRequest request,
        Guid actorUserId,
        WorkforceAllowanceCaller caller,
        string auditAction,
        string auditMessage,
        Action<PrlWorkforceAllowance, DateTime> apply,
        bool revalidate,
        CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey(workforceProfileId), async () =>
        {
            var entity = await FindTrackedAsync(workforceProfileId, id, cancellationToken);
            PayrollVersionToken.EnsureMatches(request.ExpectedRowVersion, entity.CreateDateTime, entity.UpdateDateTime, "Penetapan tunjangan ini");

            var previousStatus = entity.AllowanceStatus;
            var now = DateTime.UtcNow;
            apply(entity, now);

            if (revalidate)
            {
                var type = await LoadEligibleTypeAsync(entity.AllowanceTypeId, cancellationToken);
                var start = PayrollDateNormalizer.ToUtcDate(entity.EffectiveStartDate);
                var end = PayrollDateNormalizer.ToUtcDate(entity.EffectiveEndDate);
                await EnsureNoOverlapAsync(workforceProfileId, type, entity.Id, start, end, cancellationToken);
                await EnsureNotInSalaryStructureAsync(workforceProfileId, type, start, end, cancellationToken);
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await AuditAsync(auditAction, auditMessage, entity, previousStatus, actorUserId, NormalizeText(request.Reason));
            return await GetItemAsync(workforceProfileId, entity.Id, caller, cancellationToken);
        }, cancellationToken);

    // ------------------------------------------------------------------------------------------------------------------------
    // Pemuatan baris dari semua sumber
    // ------------------------------------------------------------------------------------------------------------------------

    private sealed record LoadedItems(List<WorkforceAllowanceItemResponse> Items, bool HasSalaryAssignment, bool HasTransportAllowance);

    private async Task<LoadedItems> LoadItemsAsync(Guid workforceProfileId, WorkforceAllowanceCaller caller, CancellationToken cancellationToken)
    {
        var today = PayrollDateNormalizer.ToUtcDate(DateTime.UtcNow);
        var salary = await LoadSalaryBasisAsync(workforceProfileId, today, cancellationToken);

        var items = new List<WorkforceAllowanceItemResponse>();
        items.AddRange(await LoadStructureItemsAsync(salary, caller, today, cancellationToken));

        var individual = await LoadIndividualRowsAsync(workforceProfileId, null, cancellationToken);
        var usedIds = await LoadUsedIdsAsync(individual.Select(x => x.Id).ToList(), cancellationToken);
        items.AddRange(individual.Select(row => ToIndividualItem(row, usedIds.Contains(row.Id), caller, today, salary)));

        var (transportItems, hasTransport) = await LoadTransportItemsAsync(workforceProfileId, caller, today, cancellationToken);
        items.AddRange(transportItems);

        var ordered = items
            .OrderBy(x => x.SourceType switch
            {
                WorkforceAllowanceSourceTypes.SalaryStructure => 0,
                WorkforceAllowanceSourceTypes.Individual => 1,
                _ => 2
            })
            .ThenBy(x => x.AllowanceTypeName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Id)
            .ToList();

        return new LoadedItems(ordered, salary is not null, hasTransport);
    }

    private async Task<WorkforceAllowanceItemResponse> GetItemAsync(
        Guid workforceProfileId, Guid id, WorkforceAllowanceCaller caller, CancellationToken cancellationToken)
    {
        var today = PayrollDateNormalizer.ToUtcDate(DateTime.UtcNow);
        var rows = await LoadIndividualRowsAsync(workforceProfileId, id, cancellationToken);
        var row = rows.FirstOrDefault() ?? throw new KeyNotFoundException("Penetapan tunjangan tidak ditemukan.");
        var salary = await LoadSalaryBasisAsync(workforceProfileId, today, cancellationToken);
        var used = await LoadUsedIdsAsync(new List<Guid> { id }, cancellationToken);
        return ToIndividualItem(row, used.Contains(id), caller, today, salary);
    }

    private async Task<SalaryBasis?> LoadSalaryBasisAsync(Guid workforceProfileId, DateTime today, CancellationToken cancellationToken)
    {
        // "Approved" pada SalaryBasis berarti "sah dipakai payroll": mengikuti sakelar RequireApprovedSalaryAssignment (PAY-BE-FIX-008).
        // Mati (bawaan): penetapan aktif dan efektif sudah sah. Menyala: harus disetujui (ApprovedAt dan ApprovedByUserId terisi).
        // "Aktif" tidak pernah dianggap sama dengan "disetujui" bila approval disyaratkan.
        var requireApproval = _policy.RequireApprovedSalaryAssignment;
        return await _dbContext.WfpSalaryAssignments.AsNoTracking()
            .Where(a => !a.IsDelete && a.IsActive && a.IsPrimary && a.WorkforceProfileId == workforceProfileId
                && a.EffectiveStartDate <= today && (a.EffectiveEndDate == null || a.EffectiveEndDate >= today))
            // Yang disetujui didahulukan bila ada lebih dari satu kandidat.
            .OrderByDescending(a => a.ApprovedAt != null && a.ApprovedByUserId != null)
            .ThenByDescending(a => a.EffectiveStartDate)
            .Select(a => new SalaryBasis(
                a.Id, a.SalaryStructureId, a.BaseSalary, !requireApproval || (a.ApprovedAt != null && a.ApprovedByUserId != null),
                a.EffectiveStartDate, a.EffectiveEndDate))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<List<IndividualRow>> LoadIndividualRowsAsync(Guid workforceProfileId, Guid? id, CancellationToken cancellationToken) =>
        await _dbContext.PrlWorkforceAllowances.AsNoTracking()
            .Where(x => !x.IsDelete && x.WorkforceProfileId == workforceProfileId && (id == null || x.Id == id))
            .Select(x => new IndividualRow(
                x.Id,
                x.AllowanceTypeId,
                x.AllowanceType != null ? x.AllowanceType.AllowanceTypeCode : string.Empty,
                x.AllowanceType != null ? x.AllowanceType.AllowanceTypeName : string.Empty,
                x.AllowanceType != null ? x.AllowanceType.AllowanceCategory : null,
                x.CalculationMethod,
                x.Amount,
                x.Percentage,
                x.CurrencyCode,
                x.IsTaxable,
                x.IsProrated,
                x.RequiresApproval,
                x.MaximumAmount,
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                x.AllowanceStatus,
                x.StatusReason,
                x.SubmittedAt,
                x.DecidedAt,
                x.Notes,
                x.IsActive,
                x.CreateDateTime,
                x.UpdateDateTime,
                x.AssignmentSource))
            .ToListAsync(cancellationToken);

    /// <summary>Penetapan yang pernah menjadi komponen payroll run (apa pun status run-nya): isinya dikunci.</summary>
    private async Task<HashSet<Guid>> LoadUsedIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return new HashSet<Guid>();
        return (await _dbContext.PrlPayrollEmployeeComponents.AsNoTracking()
                .Where(c => !c.IsDelete && c.SourceType == PayrollEngineComponentSources.WorkforceAllowance
                    && c.SourceId != null && ids.Contains(c.SourceId.Value))
                .Select(c => c.SourceId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet();
    }

    private Task<bool> IsUsedInPayrollAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.PrlPayrollEmployeeComponents.AsNoTracking()
            .AnyAsync(c => !c.IsDelete && c.SourceType == PayrollEngineComponentSources.WorkforceAllowance && c.SourceId == id, cancellationToken);

    private async Task<List<WorkforceAllowanceItemResponse>> LoadStructureItemsAsync(
        SalaryBasis? salary, WorkforceAllowanceCaller caller, DateTime today, CancellationToken cancellationToken)
    {
        if (salary is null) return new List<WorkforceAllowanceItemResponse>();

        var rows = await _dbContext.MstSalaryStructureAllowances.AsNoTracking()
            .Where(x => !x.IsDelete && x.SalaryStructureId == salary.SalaryStructureId && x.AllowanceType != null && !x.AllowanceType.IsDelete)
            .Select(x => new
            {
                x.Id,
                x.AllowanceTypeId,
                Code = x.AllowanceType!.AllowanceTypeCode,
                Name = x.AllowanceType.AllowanceTypeName,
                Category = x.AllowanceType.AllowanceCategory,
                x.CalculationMethod,
                x.Amount,
                x.Percentage,
                x.IsActive,
                x.Notes,
                Maximum = x.AllowanceType.MaximumAmount,
                x.AllowanceType.CurrencyCode,
                x.AllowanceType.IsTaxable,
                x.AllowanceType.IsProrated,
                x.AllowanceType.RequiresApproval,
                x.CreateDateTime,
                x.UpdateDateTime
            })
            .ToListAsync(cancellationToken);

        var approvedBase = salary.Approved ? (decimal?)salary.BaseSalary : null;
        return rows.Select(x =>
        {
            var isPercentage = x.CalculationMethod == SalaryStructureAllowanceCalculationMethods.PercentageOfBaseSalary;
            // Hak dari Struktur Gaji hanya berlaku bila penetapan gaji yang memberinya disetujui; baris yang sekadar aktif tidak dibayarkan.
            var effective = x.IsActive && salary.Approved
                && salary.EffectiveStartDate <= today && (salary.EffectiveEndDate == null || salary.EffectiveEndDate >= today);
            var notPayableReason = !x.IsActive
                ? "Baris tunjangan ini dinonaktifkan pada Struktur Gaji."
                : !salary.Approved
                    ? "Penetapan gaji pegawai belum disetujui HRD, jadi tunjangan dari Struktur Gaji belum dibayarkan."
                    : null;

            return new WorkforceAllowanceItemResponse
            {
                Id = x.Id,
                SourceType = WorkforceAllowanceSourceTypes.SalaryStructure,
                SourceLabel = "Dari Struktur Gaji",
                AllowanceTypeId = x.AllowanceTypeId,
                AllowanceTypeCode = x.Code,
                AllowanceTypeName = x.Name,
                AllowanceCategory = x.Category,
                CalculationMethod = x.CalculationMethod,
                Status = x.IsActive ? "Active" : "Inactive",
                IsEffectiveNow = effective,
                CurrencyCode = x.CurrencyCode,
                IsTaxable = x.IsTaxable,
                IsProrated = x.IsProrated,
                RequiresApproval = false,
                AmountsVisible = caller.CanViewAmount,
                Amount = caller.CanViewAmount && !isPercentage ? x.Amount : null,
                Percentage = caller.CanViewAmount && isPercentage ? x.Percentage : null,
                MaximumAmount = caller.CanViewAmount ? x.Maximum : null,
                EstimatedMonthlyAmount = caller.CanViewAmount ? EstimateMonthly(isPercentage, x.Amount, x.Percentage, x.Maximum, approvedBase) : null,
                EffectiveStartDate = salary.EffectiveStartDate,
                EffectiveEndDate = salary.EffectiveEndDate,
                Notes = x.Notes,
                IsEditable = false,
                NotPayableReason = notPayableReason,
                LockReason = "Tunjangan ini berlaku untuk semua pemilik Struktur Gaji yang sama. Ubah di pengelolaan Struktur Gaji.",
                ManagementTarget = WorkforceAllowanceManagementTargets.SalaryStructure,
                ManagementReferenceId = salary.SalaryStructureId,
                RowVersion = PayrollVersionToken.From(x.CreateDateTime, x.UpdateDateTime)
            };
        }).ToList();
    }

    private async Task<(List<WorkforceAllowanceItemResponse> Items, bool Exists)> LoadTransportItemsAsync(
        Guid workforceProfileId, WorkforceAllowanceCaller caller, DateTime today, CancellationToken cancellationToken)
    {
        var rows = await _dbContext.PrlTransportAllowances.AsNoTracking()
            .Where(x => !x.IsDelete && x.WorkforceProfileId == workforceProfileId)
            .Select(x => new
            {
                x.Id,
                x.AllowanceStatus,
                x.IsActive,
                x.MonthlyAmount,
                x.CurrencyCode,
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                x.Description,
                PolicyName = x.TransportAllowancePolicy != null ? x.TransportAllowancePolicy.PolicyName : null,
                PolicyCode = x.TransportAllowancePolicy != null ? x.TransportAllowancePolicy.PolicyCode : null,
                PolicyMethod = x.TransportAllowancePolicy != null ? x.TransportAllowancePolicy.CalculationMethod : null,
                PolicyFixed = x.TransportAllowancePolicy != null ? x.TransportAllowancePolicy.FixedMonthlyAmount : 0m,
                PolicyLimit = x.TransportAllowancePolicy != null ? x.TransportAllowancePolicy.MonthlyLimitAmount : 0m,
                PolicyTaxable = x.TransportAllowancePolicy == null || x.TransportAllowancePolicy.IsTaxable,
                PolicyProrated = x.TransportAllowancePolicy == null || x.TransportAllowancePolicy.IsProrated,
                x.CreateDateTime,
                x.UpdateDateTime
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(x =>
        {
            var isFixed = string.Equals(x.PolicyMethod, "FixedMonthly", StringComparison.OrdinalIgnoreCase);
            decimal? estimate = null;
            if (caller.CanViewAmount && isFixed)
            {
                var monthly = x.MonthlyAmount > 0m ? x.MonthlyAmount : x.PolicyFixed;
                if (x.PolicyLimit > 0m && monthly > x.PolicyLimit) monthly = x.PolicyLimit;
                if (monthly > 0m) estimate = monthly;
            }

            var active = x.IsActive && string.Equals(x.AllowanceStatus, "Active", StringComparison.OrdinalIgnoreCase);
            return new WorkforceAllowanceItemResponse
            {
                Id = x.Id,
                SourceType = WorkforceAllowanceSourceTypes.Policy,
                SourceLabel = "Berbasis Kebijakan",
                AllowanceTypeCode = x.PolicyCode ?? string.Empty,
                AllowanceTypeName = "Tunjangan Transportasi" + (string.IsNullOrWhiteSpace(x.PolicyName) ? string.Empty : $" ({x.PolicyName})"),
                AllowanceCategory = "Transport",
                CalculationMethod = "PolicyBased",
                Status = active ? "Active" : "Inactive",
                IsEffectiveNow = active
                    && (x.EffectiveStartDate == null || x.EffectiveStartDate <= today)
                    && (x.EffectiveEndDate == null || x.EffectiveEndDate >= today),
                CurrencyCode = x.CurrencyCode,
                IsTaxable = x.PolicyTaxable,
                IsProrated = x.PolicyProrated,
                RequiresApproval = false,
                AmountsVisible = caller.CanViewAmount,
                EstimatedMonthlyAmount = estimate,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                Notes = x.Description,
                IsEditable = false,
                LockReason = "Tunjangan transportasi dihitung kebijakan dan dikelola di Tunjangan Transportasi.",
                ManagementTarget = WorkforceAllowanceManagementTargets.TransportAllowance,
                RowVersion = PayrollVersionToken.From(x.CreateDateTime, x.UpdateDateTime)
            };
        }).ToList();

        return (items, rows.Count > 0);
    }

    private static WorkforceAllowanceItemResponse ToIndividualItem(
        IndividualRow row, bool used, WorkforceAllowanceCaller caller, DateTime today, SalaryBasis? salary)
    {
        var isPercentage = row.CalculationMethod == SalaryStructureAllowanceCalculationMethods.PercentageOfBaseSalary;
        var approvedBase = salary is { Approved: true } ? (decimal?)salary.BaseSalary : null;
        var inactive = row.Status == WorkforceAllowanceStatuses.Inactive;
        // Penetapan berbasis kebijakan (PAY-BE-FIX-022) nilainya milik kebijakan dan tarif Grade Gaji; tidak ada jalan mengubah nominalnya
        // per pegawai. Untuk menggantinya: nonaktifkan, lalu tetapkan ulang dari kebijakan.
        var fromPolicy = row.AssignmentSource == WorkforceAllowanceAssignmentSources.Policy;
        var editable = !used && !inactive && !fromPolicy;

        // Alasan baris belum/tidak dibayarkan mesin payroll, diturunkan dari data (bukan dikarang): status, tanggal efektif, dan gaji pokok.
        var notPayableReason = row.Status switch
        {
            WorkforceAllowanceStatuses.Approved when !row.IsActive => "Penetapan ini dinonaktifkan.",
            WorkforceAllowanceStatuses.Approved when row.EffectiveStartDate > today =>
                "Belum berlaku; mulai " + row.EffectiveStartDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) + ".",
            WorkforceAllowanceStatuses.Approved when row.EffectiveEndDate.HasValue && row.EffectiveEndDate.Value < today =>
                "Masa berlakunya sudah berakhir.",
            WorkforceAllowanceStatuses.Approved when isPercentage && approvedBase is null =>
                "Persentase gaji pokok belum dapat dihitung karena penetapan gaji pegawai belum disetujui HRD.",
            WorkforceAllowanceStatuses.Approved => null,
            WorkforceAllowanceStatuses.PendingApproval => "Menunggu persetujuan; belum dibayarkan.",
            WorkforceAllowanceStatuses.Draft => "Masih draft dan belum diajukan; belum dibayarkan.",
            WorkforceAllowanceStatuses.Rejected => "Ditolak; tidak dibayarkan.",
            WorkforceAllowanceStatuses.Inactive => "Dinonaktifkan; tidak dibayarkan.",
            _ => null
        };

        var actions = new List<string>();
        if (editable && caller.CanUpdate) actions.Add("Update");
        if (row.RequiresApproval && row.Status is WorkforceAllowanceStatuses.Draft or WorkforceAllowanceStatuses.Rejected && caller.CanSubmit)
            actions.Add("Submit");
        if (row.Status == WorkforceAllowanceStatuses.PendingApproval)
        {
            if (caller.CanApprove) actions.Add("Approve");
            if (caller.CanReject) actions.Add("Reject");
        }
        if (!inactive && caller.CanDeactivate) actions.Add("Deactivate");

        return new WorkforceAllowanceItemResponse
        {
            Id = row.Id,
            SourceType = WorkforceAllowanceSourceTypes.Individual,
            SourceLabel = fromPolicy ? "Dari Kebijakan" : "Khusus Pegawai",
            AllowanceTypeId = row.AllowanceTypeId,
            AllowanceTypeCode = row.TypeCode,
            AllowanceTypeName = row.TypeName,
            AllowanceCategory = row.TypeCategory,
            CalculationMethod = row.CalculationMethod,
            Status = row.Status,
            IsEffectiveNow = row.Status == WorkforceAllowanceStatuses.Approved && row.IsActive
                && row.EffectiveStartDate <= today && (row.EffectiveEndDate == null || row.EffectiveEndDate >= today),
            CurrencyCode = row.CurrencyCode,
            IsTaxable = row.IsTaxable,
            IsProrated = row.IsProrated,
            RequiresApproval = row.RequiresApproval,
            AmountsVisible = caller.CanViewAmount,
            Amount = caller.CanViewAmount && !isPercentage ? row.Amount : null,
            Percentage = caller.CanViewAmount && isPercentage ? row.Percentage : null,
            MaximumAmount = caller.CanViewAmount ? row.MaximumAmount : null,
            EstimatedMonthlyAmount = caller.CanViewAmount
                ? EstimateMonthly(isPercentage, row.Amount, row.Percentage, row.MaximumAmount, approvedBase)
                : null,
            EffectiveStartDate = row.EffectiveStartDate,
            EffectiveEndDate = row.EffectiveEndDate,
            StatusReason = row.StatusReason,
            SubmittedAt = row.SubmittedAt,
            DecidedAt = row.DecidedAt,
            Notes = row.Notes,
            UsedInPayroll = used,
            IsEditable = editable,
            NotPayableReason = notPayableReason,
            LockReason = used ? "Sudah dipakai payroll run. Nonaktifkan lalu buat penetapan baru untuk mengubahnya."
                : inactive ? "Penetapan ini sudah dinonaktifkan."
                : fromPolicy ? "Nilai penetapan ini mengikuti Kebijakan Tunjangan. Nonaktifkan lalu tetapkan ulang dari kebijakan untuk mengubahnya."
                : null,
            AvailableActions = actions,
            RowVersion = PayrollVersionToken.From(row.CreateDateTime, row.UpdateDateTime)
        };
    }

    /// <summary>Perkiraan sebulan penuh; null bila persentase tetapi belum ada gaji pokok yang disetujui.</summary>
    private static decimal? EstimateMonthly(bool isPercentage, decimal amount, decimal percentage, decimal? maximum, decimal? approvedBaseSalary)
    {
        decimal monthly;
        if (isPercentage)
        {
            if (approvedBaseSalary is null) return null;
            monthly = approvedBaseSalary.Value * percentage / 100m;
        }
        else
        {
            monthly = amount;
        }

        if (maximum.HasValue && maximum.Value > 0m && monthly > maximum.Value) monthly = maximum.Value;
        return Math.Round(monthly, 2, MidpointRounding.AwayFromZero);
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Validasi terhadap master dan aturan bisnis
    // ------------------------------------------------------------------------------------------------------------------------

    private async Task EnsureWorkforceExistsAsync(Guid workforceProfileId, CancellationToken cancellationToken)
    {
        var exists = workforceProfileId != Guid.Empty
            && await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!exists) throw new KeyNotFoundException("Data pegawai atau dokter tidak ditemukan di HRD.");
    }

    private async Task<PrlWorkforceAllowance> FindTrackedAsync(Guid workforceProfileId, Guid id, CancellationToken cancellationToken) =>
        await _dbContext.PrlWorkforceAllowances
            .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Penetapan tunjangan tidak ditemukan.");

    /// <summary>
    /// Jenis tunjangan yang boleh ditetapkan per pegawai: aktif, berulang, bermetode Fixed atau Percentage, bukan tunjangan berbasis
    /// aktivitas/kebijakan atau input manual, dan komponen payroll-nya (bila ada) valid.
    /// Memakai <see cref="WorkforceAllowanceEligibilityRules"/> yang sama dengan endpoint opsi Master Tunjangan dalam satu query.
    /// </summary>
    private async Task<TypeFacts> LoadEligibleTypeAsync(Guid allowanceTypeId, CancellationToken cancellationToken)
    {
        var row = await _dbContext.MstAllowanceTypes.AsNoTracking()
            .Where(x => x.Id == allowanceTypeId && !x.IsDelete && x.IsActive)
            .Select(x => new
            {
                Facts = new TypeFacts(
                    x.Id, x.PayrollComponentId, x.AllowanceTypeCode, x.AllowanceTypeName, x.AllowanceCategory, x.CalculationMethod,
                    x.CurrencyCode, x.DefaultAmount, x.DefaultPercentage, x.MaximumAmount, x.IsRecurring, x.IsTaxable, x.IsProrated,
                    x.RequiresApproval, x.EffectiveStartDate, x.EffectiveEndDate),
                HasActivePolicy =
                    x.ShiftAllowancePolicies.Any(p => p.IsActive && !p.IsDelete)
                    || x.OnCallAllowancePolicies.Any(p => p.IsActive && !p.IsDelete)
                    || x.HazardAllowancePolicies.Any(p => p.IsActive && !p.IsDelete)
                    || (x.PayrollComponentId != null
                        && _dbContext.PrlTransportAllowancePolicies.Any(p =>
                            p.PayrollComponentId == x.PayrollComponentId && p.IsActive && !p.IsDelete)),
                HasInvalidPayrollComponent =
                    x.PayrollComponentId != null
                    && (x.PayrollComponent == null || !x.PayrollComponent.IsActive || x.PayrollComponent.IsDelete)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException("Jenis tunjangan tidak ditemukan atau tidak aktif di Master Tunjangan.");

        var ineligibleReason = WorkforceAllowanceEligibilityRules.EvaluateIneligibilityReason(
            row.Facts.Name,
            row.Facts.Category,
            row.Facts.Method,
            isActive: true,
            row.Facts.IsRecurring,
            row.HasActivePolicy,
            row.HasInvalidPayrollComponent);

        if (ineligibleReason is not null)
            throw new ArgumentException(ineligibleReason);

        return row.Facts;
    }

    private static void ValidateDates(TypeFacts type, DateTime start, DateTime? end)
    {
        if (end.HasValue && end.Value < start)
            throw new ArgumentException("Tanggal akhir berlaku tidak boleh lebih awal dari tanggal mulai.");

        if (type.EffectiveStartDate.HasValue && start < PayrollDateNormalizer.ToUtcDate(type.EffectiveStartDate.Value))
            throw new ArgumentException(
                $"Tanggal mulai berlaku lebih awal dari masa berlaku jenis tunjangan (mulai {type.EffectiveStartDate.Value:dd MMM yyyy}).");

        if (type.EffectiveEndDate.HasValue)
        {
            var masterEnd = PayrollDateNormalizer.ToUtcDate(type.EffectiveEndDate.Value);
            if (start > masterEnd)
                throw new ArgumentException($"Jenis tunjangan sudah berakhir pada {masterEnd:dd MMM yyyy}.");
            if (!end.HasValue || end.Value > masterEnd)
                throw new ArgumentException(
                    $"Masa berlaku jenis tunjangan berakhir pada {masterEnd:dd MMM yyyy}. Isi tanggal akhir berlaku paling lambat tanggal itu.");
        }
    }

    /// <summary>Nominal atau persen sesuai metode master; nilai kosong memakai bawaan master; batas maksimum dijaga di server.</summary>
    private static (string Method, decimal Amount, decimal Percentage) ResolveValues(TypeFacts type, decimal? amount, decimal? percentage)
    {
        if (string.Equals(type.Method, WorkforceAllowanceEligibilityRules.MasterFixed, StringComparison.OrdinalIgnoreCase))
        {
            var value = Math.Round(amount ?? type.DefaultAmount, 2, MidpointRounding.AwayFromZero);
            if (value <= 0m) throw new ArgumentException("Nominal tunjangan harus lebih dari nol.");
            if (type.MaximumAmount.HasValue && type.MaximumAmount.Value > 0m && value > type.MaximumAmount.Value)
                throw new ArgumentException($"Nominal tunjangan melebihi batas maksimum jenis tunjangan {type.Name}.");
            return (SalaryStructureAllowanceCalculationMethods.FixedAmount, value, 0m);
        }

        var percent = Math.Round(percentage ?? type.DefaultPercentage, 2, MidpointRounding.AwayFromZero);
        if (percent <= 0m || percent > 100m)
            throw new ArgumentException("Persentase tunjangan harus di atas 0 dan paling tinggi 100.");
        return (SalaryStructureAllowanceCalculationMethods.PercentageOfBaseSalary, 0m, percent);
    }

    /// <summary>Satu jenis tunjangan paling banyak satu penetapan hidup pada satu waktu untuk pegawai yang sama.</summary>
    private async Task EnsureNoOverlapAsync(
        Guid workforceProfileId, TypeFacts type, Guid? excludeId, DateTime start, DateTime? end, CancellationToken cancellationToken)
    {
        var query = _dbContext.PrlWorkforceAllowances.AsNoTracking().Where(x =>
            !x.IsDelete && x.WorkforceProfileId == workforceProfileId && x.AllowanceTypeId == type.Id
            && (x.AllowanceStatus == WorkforceAllowanceStatuses.Draft
                || x.AllowanceStatus == WorkforceAllowanceStatuses.PendingApproval
                || x.AllowanceStatus == WorkforceAllowanceStatuses.Approved)
            && (excludeId == null || x.Id != excludeId)
            && (x.EffectiveEndDate == null || x.EffectiveEndDate >= start));
        if (end.HasValue) query = query.Where(x => x.EffectiveStartDate <= end.Value);

        if (await query.AnyAsync(cancellationToken))
            throw new PayrollRunConflictException(
                $"Pegawai ini sudah punya penetapan tunjangan {type.Name} yang masa berlakunya beririsan. Ubah penetapan yang ada atau nonaktifkan lebih dulu.");
    }

    /// <summary>Tunjangan yang sudah ada di Struktur Gaji pegawai pada rentang yang sama tidak boleh ditetapkan lagi per pegawai (tidak dihitung dua kali).</summary>
    private async Task EnsureNotInSalaryStructureAsync(
        Guid workforceProfileId, TypeFacts type, DateTime start, DateTime? end, CancellationToken cancellationToken)
    {
        var assignments = _dbContext.WfpSalaryAssignments.AsNoTracking().Where(a =>
            !a.IsDelete && a.IsActive && a.WorkforceProfileId == workforceProfileId
            && (a.EffectiveEndDate == null || a.EffectiveEndDate >= start));
        if (end.HasValue) assignments = assignments.Where(a => a.EffectiveStartDate <= end.Value);

        var structureIds = await assignments.Select(a => a.SalaryStructureId).Distinct().ToListAsync(cancellationToken);
        if (structureIds.Count == 0) return;

        var duplicated = await _dbContext.MstSalaryStructureAllowances.AsNoTracking().AnyAsync(x =>
            !x.IsDelete && x.IsActive && x.AllowanceTypeId == type.Id && structureIds.Contains(x.SalaryStructureId), cancellationToken);
        if (duplicated)
            throw new PayrollRunConflictException(
                $"Tunjangan {type.Name} sudah berasal dari Struktur Gaji pegawai ini. Ubah di Struktur Gaji atau pilih jenis tunjangan lain; "
                + "tunjangan yang sama tidak boleh dihitung dua kali.");
    }

    private static void ApplyTerms(
        PrlWorkforceAllowance entity, TypeFacts type, string method, decimal amount, decimal percentage, DateTime start, DateTime? end, string? notes)
    {
        entity.AllowanceTypeId = type.Id;
        entity.PayrollComponentId = type.PayrollComponentId;
        entity.CalculationMethod = method;
        entity.Amount = amount;
        entity.Percentage = percentage;
        entity.CurrencyCode = type.CurrencyCode;
        entity.IsTaxable = type.IsTaxable;
        entity.IsProrated = type.IsProrated;
        entity.RequiresApproval = type.RequiresApproval;
        entity.MaximumAmount = type.MaximumAmount;
        entity.EffectiveStartDate = start;
        entity.EffectiveEndDate = end;
        entity.Notes = NormalizeText(notes);
    }

    /// <summary>Status awal: butuh persetujuan -> PendingApproval; tidak butuh -> Approved otomatis (dicatat sebagai keputusan sistem).</summary>
    private static void StartLifecycle(PrlWorkforceAllowance entity, TypeFacts type, Guid actorUserId, DateTime now)
    {
        entity.IsActive = true;
        if (type.RequiresApproval)
        {
            entity.AllowanceStatus = WorkforceAllowanceStatuses.PendingApproval;
            entity.SubmittedAt = now;
            entity.SubmittedByUserId = actorUserId;
            entity.DecidedAt = null;
            entity.DecidedByUserId = null;
            entity.StatusReason = null;
        }
        else
        {
            entity.AllowanceStatus = WorkforceAllowanceStatuses.Approved;
            entity.DecidedAt = now;
            entity.DecidedByUserId = actorUserId;
            entity.StatusReason = "Disetujui otomatis: jenis tunjangan tidak memerlukan persetujuan.";
        }
    }

    private static void ResetDecision(PrlWorkforceAllowance entity)
    {
        entity.SubmittedAt = null;
        entity.SubmittedByUserId = null;
        entity.DecidedAt = null;
        entity.DecidedByUserId = null;
        entity.StatusReason = null;
    }

    private Task AuditAsync(string action, string message, PrlWorkforceAllowance entity, string? previousStatus, Guid actorUserId, string? reason) =>
        // Tanpa nominal dan persen: hanya identitas, status sebelum dan sesudah, aktor, dan alasan.
        _loggerService.AuditAsync(LogCategory, action, message, new
        {
            EntityId = entity.Id,
            entity.WorkforceProfileId,
            entity.AllowanceTypeId,
            PreviousStatus = previousStatus,
            Status = entity.AllowanceStatus,
            Reason = reason,
            ActorUserId = actorUserId
        });

    private static string LockKey(Guid workforceProfileId) => $"PRL_WORKFORCE_ALLOWANCE_{workforceProfileId:N}";

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
