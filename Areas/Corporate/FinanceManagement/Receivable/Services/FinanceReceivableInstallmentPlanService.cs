using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Perjanjian pembayaran bertahap atas piutang manfaat karyawan (BE-FIN-093, FIN-DES-099,
/// FIN-DEC-165/166). Mengajukan, menyetujui (maker-checker keras, membangkitkan jadwal angsuran),
/// menolak, dan membatalkan. Menyetujui dan menolak adalah tindakan "pihak berwenang lain" —
/// pengaju MUST NOT menyetujui pengajuannya sendiri, ditegakkan ganda: service (pesan jelas) dan
/// CK_FinReceivableInstallmentPlan_MakerChecker (keras, tidak dapat dilewati walau administrator
/// memberi satu peran kedua hak Create dan Approve — L.2.1, L.8.1).
///
/// BE-FIN-093 TIDAK mencakup: penerimaan hasil potongan gaji dari HR (slice S3, tertahan
/// FIN-OQ-091) dan transisi otomatis DISETUJUI -&gt; SELESAI yang bergantung padanya (L.1.7).
/// Keduanya milik BE-FIN-095, yang belum ada task pemilik eksplisit pada repository ini.
///
/// BE-FIN-094: ApproveAsync memanggil FinanceReceivablePayrollSyncService DI DALAM transaksi yang
/// sama (INT-FIN-HR-001 §P.2 — "ditulis otomatis di dalam transaksi persetujuan"). Kegagalan
/// sinkron payroll (periode/komponen HR belum dikonfigurasi) TIDAK membatalkan persetujuan —
/// keputusan desain task ini, dicatat laporan, bukan di kontrak asli.
/// </summary>
public sealed class FinanceReceivableInstallmentPlanService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.InstallmentPlan";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceReceivablePayrollSyncService _payrollSyncService;

    public FinanceReceivableInstallmentPlanService(
        ApplicationDbContext dbContext, LoggerService loggerService, FinanceReceivablePayrollSyncService payrollSyncService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _payrollSyncService = payrollSyncService;
    }

    // ------------------------------------------------------------------------------------
    // Pengajuan (FR-FIN-192..195)
    // ------------------------------------------------------------------------------------

    public async Task<InstallmentPlanResponse> CreateAsync(
        Guid receivableId, CreateInstallmentPlanRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            // L.2.3: dua pengajuan hampir bersamaan atas piutang yang sama MUST tertolak unique
            // index, bukan hanya pemeriksaan service — lock di sini membuat pemeriksaan di bawah
            // dan index itu konsisten terhadap race yang sama.
            await AcquireLockAsync($"FIN_INSTALLMENT_PLAN_RECEIVABLE_{receivableId:N}", cancellationToken);

            var receivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => x.Id == receivableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Kartu piutang tidak ditemukan.");

            // FIN-VAL-230: hanya piutang manfaat karyawan.
            if (receivable.DebtorType != FinReceivableDebtorTypes.EmployeeBenefit)
                throw new InstallmentPlanValidationException(
                    $"Perjanjian angsuran hanya untuk piutang manfaat karyawan. Piutang ini berjenis {receivable.DebtorType}.");

            // FIN-VAL-236: piutang lunas/dihapus-buku/dibatalkan tidak dapat diangsur.
            if (receivable.Status is FinReceivableStatuses.Settled or FinReceivableStatuses.WrittenOff or FinReceivableStatuses.Cancelled)
                throw new InstallmentPlanValidationException(
                    $"Piutang ini berstatus {receivable.Status} sehingga tidak dapat diangsur.");

            // FIN-VAL-235: periode gaji pertama tidak boleh yang sudah berjalan atau sudah lewat.
            var currentPeriod = CurrentPeriod();
            if (string.CompareOrdinal(request.FirstDeductionPeriod, currentPeriod) <= 0)
                throw new InstallmentPlanValidationException(
                    $"Periode gaji pertama tidak boleh periode yang sudah berjalan atau sudah lewat. Pilih {AddMonths(currentPeriod, 1)} atau sesudahnya.");

            // FIN-VAL-231 lapis service (pesan jelas) — lapis keras adalah unique index terfilter
            // IX_FinReceivableInstallmentPlan_ReceivableId_Active, ditangkap DbUpdateException di bawah.
            var hasActivePlan = await _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.ReceivableId == receivableId
                    && (x.Status == FinReceivableInstallmentPlanStatuses.Menunggu || x.Status == FinReceivableInstallmentPlanStatuses.Disetujui),
                    cancellationToken);
            if (hasActivePlan)
                throw new InstallmentPlanConflictException(
                    "Piutang ini sudah punya perjanjian angsuran yang berjalan. Batalkan dulu perjanjian itu sebelum membuat yang baru.");

            var now = DateTimeOffset.UtcNow;
            var plan = new FinReceivableInstallmentPlan
            {
                PlanNumber = GeneratePlanNumber(),
                ReceivableId = receivableId,
                InstallmentCount = request.InstallmentCount,
                InstallmentAmount = request.InstallmentAmount,
                // Dihitung server, tidak pernah dipercayakan ke klien — request tidak memuat ruas
                // Total (O.2.1). CK_FinReceivableInstallmentPlan_Total karena itu selalu cocok
                // lewat jalur ini; constraint tetap dipasang sebagai pertahanan berlapis.
                TotalAgreedAmount = request.InstallmentCount * request.InstallmentAmount,
                FirstDeductionPeriod = request.FirstDeductionPeriod,
                Status = FinReceivableInstallmentPlanStatuses.Menunggu,
                AgreementDocumentPath = request.AgreementDocumentPath,
                RequestedBy = actorUserId,
                RequestedAt = now,
                Notes = request.Notes,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.FinReceivableInstallmentPlans.Add(plan);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                throw new InstallmentPlanConflictException(
                    "Piutang ini sudah punya perjanjian angsuran yang berjalan. Batalkan dulu perjanjian itu sebelum membuat yang baru.", exception);
            }

            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Plan.Requested", plan.Id, actorUserId);

            return await MapAsync(plan, receivable, cancellationToken);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<List<InstallmentPlanResponse>> GetHistoryAsync(Guid receivableId, CancellationToken cancellationToken)
    {
        var receivable = await _dbContext.FinReceivables.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == receivableId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kartu piutang tidak ditemukan.");

        var plans = await _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
            .Where(x => !x.IsDelete && x.ReceivableId == receivableId)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);

        var names = await GetUserNamesAsync(
            plans.SelectMany(x => new Guid?[] { x.RequestedBy, x.ApprovedBy }), cancellationToken);

        return plans.Select(x => Map(x, receivable.ReceivableNumber, names)).ToList();
    }

    public async Task<PagedResult<InstallmentPlanListResponse>> GetPagedAsync(InstallmentPlanQuery request, CancellationToken cancellationToken)
    {
        var query =
            from plan in _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
            join receivable in _dbContext.FinReceivables.AsNoTracking() on plan.ReceivableId equals receivable.Id
            where !plan.IsDelete
            select new { plan, receivable };

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.plan.Status == request.Status);
        if (request.BenefitOwnerId.HasValue)
            query = query.Where(x => x.receivable.BenefitOwnerId == request.BenefitOwnerId.Value);
        if (!string.IsNullOrWhiteSpace(request.DeductionPeriod))
            query = query.Where(x => x.plan.FirstDeductionPeriod == request.DeductionPeriod);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.plan.PlanNumber, $"%{term}%") || EF.Functions.ILike(x.receivable.ReceivableNumber, $"%{term}%"));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "status" => descending ? query.OrderByDescending(x => x.plan.Status) : query.OrderBy(x => x.plan.Status),
            "planumber" or "plannumber" => descending ? query.OrderByDescending(x => x.plan.PlanNumber) : query.OrderBy(x => x.plan.PlanNumber),
            _ => descending ? query.OrderByDescending(x => x.plan.RequestedAt) : query.OrderBy(x => x.plan.RequestedAt)
        };

        var total = await query.CountAsync(cancellationToken);
        var page = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var names = await GetUserNamesAsync(
            page.SelectMany(x => new Guid?[] { x.plan.RequestedBy, x.plan.ApprovedBy }), cancellationToken);

        return new PagedResult<InstallmentPlanListResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = page.Select(x => MapList(x.plan, x.receivable, names)).ToList()
        };
    }

    public async Task<InstallmentPlanDetailResponse> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Perjanjian angsuran tidak ditemukan.");

        var receivable = await _dbContext.FinReceivables.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == plan.ReceivableId, cancellationToken);

        var installments = await _dbContext.FinReceivableInstallments.AsNoTracking()
            .Where(x => !x.IsDelete && x.PlanId == id)
            .OrderBy(x => x.InstallmentNumber)
            .ToListAsync(cancellationToken);

        var names = await GetUserNamesAsync(new Guid?[] { plan.RequestedBy, plan.ApprovedBy }, cancellationToken);
        var baseResponse = Map(plan, receivable?.ReceivableNumber, names);

        return new InstallmentPlanDetailResponse
        {
            Id = baseResponse.Id,
            PlanNumber = baseResponse.PlanNumber,
            ReceivableId = baseResponse.ReceivableId,
            ReceivableNumber = baseResponse.ReceivableNumber,
            InstallmentCount = baseResponse.InstallmentCount,
            InstallmentAmount = baseResponse.InstallmentAmount,
            TotalAgreedAmount = baseResponse.TotalAgreedAmount,
            FirstDeductionPeriod = baseResponse.FirstDeductionPeriod,
            Status = baseResponse.Status,
            RequestedByName = baseResponse.RequestedByName,
            RequestedAt = baseResponse.RequestedAt,
            ApprovedByName = baseResponse.ApprovedByName,
            ApprovedAt = baseResponse.ApprovedAt,
            RejectionReason = baseResponse.RejectionReason,
            CancelReason = baseResponse.CancelReason,
            Notes = baseResponse.Notes,
            RowVersion = baseResponse.RowVersion,
            Installments = installments.Select(MapInstallment).ToList()
        };
    }

    // ------------------------------------------------------------------------------------
    // Setujui — membangkitkan seluruh jadwal angsuran (FR-FIN-196, FIN-VAL-233/234)
    // ------------------------------------------------------------------------------------

    public async Task<InstallmentPlanDetailResponse> ApproveAsync(
        Guid id, ApproveInstallmentPlanRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_INSTALLMENT_PLAN_{id:N}", cancellationToken);

            var plan = await _dbContext.FinReceivableInstallmentPlans
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Perjanjian angsuran tidak ditemukan.");

            if (plan.Status != FinReceivableInstallmentPlanStatuses.Menunggu)
                throw new InstallmentPlanConflictException(
                    $"Perjanjian berstatus {plan.Status} tidak dapat disetujui. Hanya perjanjian MENUNGGU yang dapat diputuskan.");

            // FIN-VAL-234 lapis service — lapis keras CK_FinReceivableInstallmentPlan_MakerChecker
            // tetap berlaku walau lolos di sini (L.2.1, L.8.1: tidak dapat dilewati administrator).
            if (actorUserId == plan.RequestedBy)
                throw new InstallmentPlanValidationException(
                    "Anda tidak dapat menyetujui perjanjian yang Anda ajukan sendiri. Mintakan persetujuan kepada petugas berwenang lain.");

            var receivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => x.Id == plan.ReceivableId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Kartu piutang pemilik perjanjian ini tidak ditemukan.");

            // FIN-VAL-233: total yang disepakati MUST sama dengan sisa piutang SAAT DISETUJUI.
            if (plan.TotalAgreedAmount != receivable.OutstandingAmount)
                throw new InstallmentPlanValidationException(
                    $"Sisa piutang sudah berubah menjadi {receivable.OutstandingAmount:N2} sejak perjanjian diajukan. Ajukan ulang dengan angka yang benar.");

            var now = DateTimeOffset.UtcNow;
            var period = plan.FirstDeductionPeriod;
            var createdInstallments = new List<FinReceivableInstallment>(plan.InstallmentCount);
            for (var number = 1; number <= plan.InstallmentCount; number++)
            {
                // L.1.4: pembulatan sisa jatuh pada baris TERAKHIR, bukan dibagi rata — jumlah
                // seluruh baris jadwal MUST sama persis dengan TotalAgreedAmount.
                var scheduled = number < plan.InstallmentCount
                    ? plan.InstallmentAmount
                    : plan.TotalAgreedAmount - plan.InstallmentAmount * (plan.InstallmentCount - 1);

                var installment = new FinReceivableInstallment
                {
                    PlanId = plan.Id,
                    InstallmentNumber = number,
                    DeductionPeriod = period,
                    ScheduledAmount = scheduled,
                    CarriedOverAmount = 0m,
                    PaidAmount = 0m,
                    OutstandingAmount = scheduled,
                    Status = FinReceivableInstallmentStatuses.Dijadwalkan,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.FinReceivableInstallments.Add(installment);
                createdInstallments.Add(installment);

                period = AddMonths(period, 1);
            }

            plan.Status = FinReceivableInstallmentPlanStatuses.Disetujui;
            plan.ApprovedBy = actorUserId;
            plan.ApprovedAt = now;
            if (!string.IsNullOrWhiteSpace(request.Notes)) plan.Notes = request.Notes;
            plan.UpdateDateTime = DateTime.UtcNow;
            plan.UpdateBy = actorUserId;
            plan.RowVersion = Guid.NewGuid();

            // BE-FIN-094, INT-FIN-HR-001 §P.2: diterbitkan ke HR Payroll di dalam transaksi yang
            // sama. Kegagalan sinkron (periode/komponen HR belum dikonfigurasi) dicatat, TIDAK
            // membatalkan persetujuan — lihat ringkasan kelas.
            if (receivable.BenefitOwnerId.HasValue)
            {
                var syncResult = await _payrollSyncService.SyncAsync(
                    receivable.BenefitOwnerId.Value, createdInstallments, actorUserId, cancellationToken);
                if (syncResult.SkippedCount > 0)
                {
                    await AuditAsync("Plan.PayrollSyncGap", plan.Id, actorUserId);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Plan.Approved", plan.Id, actorUserId);

            return await GetDetailAsync(id, cancellationToken);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Tolak (FR-FIN-197)
    // ------------------------------------------------------------------------------------

    public async Task<InstallmentPlanResponse> RejectAsync(
        Guid id, RejectInstallmentPlanRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.FinReceivableInstallmentPlans
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Perjanjian angsuran tidak ditemukan.");

        if (plan.Status != FinReceivableInstallmentPlanStatuses.Menunggu)
            throw new InstallmentPlanConflictException(
                $"Perjanjian berstatus {plan.Status} tidak dapat ditolak. Hanya perjanjian MENUNGGU yang dapat diputuskan.");

        var now = DateTimeOffset.UtcNow;
        plan.Status = FinReceivableInstallmentPlanStatuses.Ditolak;
        plan.RejectedBy = actorUserId;
        plan.RejectedAt = now;
        plan.RejectionReason = request.RejectionReason;
        plan.UpdateDateTime = DateTime.UtcNow;
        plan.UpdateBy = actorUserId;
        plan.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Plan.Rejected", plan.Id, actorUserId);

        var receivable = await _dbContext.FinReceivables.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == plan.ReceivableId, cancellationToken);
        return await MapAsync(plan, receivable, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Batalkan (FR-FIN-198/199) — dari MENUNGGU (pengaju menarik) atau DISETUJUI (penyetuju
    // membatalkan; angsuran yang belum terbayar ikut dibatalkan).
    // ------------------------------------------------------------------------------------

    public async Task<InstallmentPlanResponse> CancelAsync(
        Guid id, CancelInstallmentPlanRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_INSTALLMENT_PLAN_{id:N}", cancellationToken);

            var plan = await _dbContext.FinReceivableInstallmentPlans
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Perjanjian angsuran tidak ditemukan.");

            if (plan.Status is not (FinReceivableInstallmentPlanStatuses.Menunggu or FinReceivableInstallmentPlanStatuses.Disetujui))
                throw new InstallmentPlanConflictException(
                    $"Perjanjian berstatus {plan.Status} tidak dapat dibatalkan.");

            var now = DateTimeOffset.UtcNow;

            if (plan.Status == FinReceivableInstallmentPlanStatuses.Disetujui)
            {
                // Angsuran yang belum terbayar penuh ikut dibatalkan. TERBAYAR tidak disentuh —
                // koreksi atasnya memakai pembalikan, bukan pembatalan (O.2 state-transition-matrix).
                var openInstallments = await _dbContext.FinReceivableInstallments
                    .Where(x => !x.IsDelete && x.PlanId == plan.Id && x.Status != FinReceivableInstallmentStatuses.Terbayar
                        && x.Status != FinReceivableInstallmentStatuses.Dibatalkan)
                    .ToListAsync(cancellationToken);

                foreach (var installment in openInstallments)
                {
                    installment.Status = FinReceivableInstallmentStatuses.Dibatalkan;
                    installment.UpdateDateTime = DateTime.UtcNow;
                    installment.UpdateBy = actorUserId;
                    installment.RowVersion = Guid.NewGuid();
                }
            }

            plan.Status = FinReceivableInstallmentPlanStatuses.Dibatalkan;
            plan.CancelReason = request.CancelReason;
            plan.UpdateDateTime = DateTime.UtcNow;
            plan.UpdateBy = actorUserId;
            plan.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Plan.Cancelled", plan.Id, actorUserId);

            var receivable = await _dbContext.FinReceivables.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == plan.ReceivableId, cancellationToken);
            return await MapAsync(plan, receivable, cancellationToken);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Infrastruktur bersama — pola sama persis dengan FinanceBillingIntakeService.
    // ------------------------------------------------------------------------------------

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    /// <summary>Pola ANG-yyyyMMdd-&lt;guid&gt; (kamus data), Guid bukan Count/Max/Last+1 (QBE-CODE-002/003).</summary>
    private static string GeneratePlanNumber()
    {
        var candidate = $"ANG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string CurrentPeriod()
    {
        var today = FinanceBusinessDate.Today();
        return $"{today.Year:D4}-{today.Month:D2}";
    }

    /// <summary>Menambah N bulan pada periode bentuk YYYY-MM, mempertahankan format yang sama.</summary>
    private static string AddMonths(string period, int monthsToAdd)
    {
        var year = int.Parse(period[..4]);
        var month = int.Parse(period[5..7]);
        var total = (year * 12 + (month - 1)) + monthsToAdd;
        var nextYear = total / 12;
        var nextMonth = total % 12 + 1;
        return $"{nextYear:D4}-{nextMonth:D2}";
    }

    private Task AuditAsync(string action, Guid planId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"InstallmentPlan.{action}",
            $"Transisi perjanjian angsuran dicatat. PlanId={planId}",
            new { PlanId = planId, ActorUserId = actorUserId });

    private async Task<Dictionary<Guid, string?>> GetUserNamesAsync(IEnumerable<Guid?> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Where(x => x.HasValue && x.Value != Guid.Empty).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string?>();

        return await _dbContext.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    private async Task<InstallmentPlanResponse> MapAsync(FinReceivableInstallmentPlan plan, FinReceivable? receivable, CancellationToken cancellationToken)
    {
        var names = await GetUserNamesAsync(new Guid?[] { plan.RequestedBy, plan.ApprovedBy }, cancellationToken);
        return Map(plan, receivable?.ReceivableNumber, names);
    }

    private static InstallmentPlanResponse Map(FinReceivableInstallmentPlan x, string? receivableNumber, Dictionary<Guid, string?> names) => new()
    {
        Id = x.Id,
        PlanNumber = x.PlanNumber,
        ReceivableId = x.ReceivableId,
        ReceivableNumber = receivableNumber,
        InstallmentCount = x.InstallmentCount,
        InstallmentAmount = x.InstallmentAmount,
        TotalAgreedAmount = x.TotalAgreedAmount,
        FirstDeductionPeriod = x.FirstDeductionPeriod,
        Status = x.Status,
        RequestedByName = names.GetValueOrDefault(x.RequestedBy),
        RequestedAt = x.RequestedAt,
        ApprovedByName = x.ApprovedBy.HasValue ? names.GetValueOrDefault(x.ApprovedBy.Value) : null,
        ApprovedAt = x.ApprovedAt,
        RejectionReason = x.RejectionReason,
        CancelReason = x.CancelReason,
        Notes = x.Notes,
        RowVersion = x.RowVersion
    };

    private static InstallmentPlanListResponse MapList(FinReceivableInstallmentPlan x, FinReceivable receivable, Dictionary<Guid, string?> names) => new()
    {
        Id = x.Id,
        PlanNumber = x.PlanNumber,
        ReceivableId = x.ReceivableId,
        ReceivableNumber = receivable.ReceivableNumber,
        InstallmentCount = x.InstallmentCount,
        InstallmentAmount = x.InstallmentAmount,
        TotalAgreedAmount = x.TotalAgreedAmount,
        FirstDeductionPeriod = x.FirstDeductionPeriod,
        Status = x.Status,
        RequestedByName = names.GetValueOrDefault(x.RequestedBy),
        RequestedAt = x.RequestedAt,
        ApprovedByName = x.ApprovedBy.HasValue ? names.GetValueOrDefault(x.ApprovedBy.Value) : null,
        ApprovedAt = x.ApprovedAt,
        RejectionReason = x.RejectionReason,
        CancelReason = x.CancelReason,
        Notes = x.Notes,
        RowVersion = x.RowVersion,
        BenefitOwnerId = receivable.BenefitOwnerId,
        BenefitRelationship = receivable.BenefitRelationship,
        OutstandingAmount = receivable.OutstandingAmount
    };

    private static InstallmentResponse MapInstallment(FinReceivableInstallment x) => new()
    {
        Id = x.Id,
        InstallmentNumber = x.InstallmentNumber,
        DeductionPeriod = x.DeductionPeriod,
        ScheduledAmount = x.ScheduledAmount,
        CarriedOverAmount = x.CarriedOverAmount,
        PaidAmount = x.PaidAmount,
        OutstandingAmount = x.OutstandingAmount,
        Status = x.Status,
        LastResultAt = x.LastResultAt
    };
}

public sealed class InstallmentPlanValidationException(string message) : Exception(message);
public sealed class InstallmentPlanConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
