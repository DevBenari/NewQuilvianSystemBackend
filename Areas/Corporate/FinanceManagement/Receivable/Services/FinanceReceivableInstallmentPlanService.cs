using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Perjanjian pembayaran bertahap atas piutang manfaat karyawan (BE-FIN-093, FIN-DES-099,
/// FIN-DEC-165/166). Mengajukan, menyetujui (membangkitkan jadwal angsuran), menolak, dan membatalkan.
/// Menyetujui dan menolak dilakukan HRD. Aturan maker-checker kini berbasis izin: pemutus yang sama dengan pengaju
/// hanya lolos dengan HrdInstallmentPlanApproval : SelfApprove (lihat EnsureCanDecideAsync). Constraint database
/// CK_FinReceivableInstallmentPlan_MakerChecker ("ApprovedBy" &lt;&gt; "RequestedBy") dihapus oleh migration
/// DropInstallmentPlanMakerCheckerConstraint; selama migration itu belum diterapkan ia tetap menolak PERSETUJUAN sendiri di
/// database. IsSelfApprovalPersistenceReadyAsync membaca keadaan itu langsung dari katalog database (bukan flag konfigurasi),
/// sehingga tidak bisa menyimpang dari skema yang sebenarnya.
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

    private const string MakerCheckerConstraintName = "CK_FinReceivableInstallmentPlan_MakerChecker";

    public FinanceReceivableInstallmentPlanService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceReceivablePayrollSyncService payrollSyncService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _payrollSyncService = payrollSyncService;
    }

    /// <summary>
    /// Apakah database SAAT INI dapat menyimpan ApprovedBy = RequestedBy. Dibaca langsung dari katalog PostgreSQL (hanya
    /// SELECT, tidak mengubah apa pun): siap bila CK_FinReceivableInstallmentPlan_MakerChecker sudah tidak ada pada tabel.
    /// Gagal membaca katalog dianggap TIDAK siap (fail-closed), sehingga tombol/keputusan yang pasti ditolak database tidak
    /// pernah ditawarkan. Tidak ada flag konfigurasi: skema, bukan niat, yang menentukan.
    ///
    /// Perubahan skema yang menyiapkannya: migration DropInstallmentPlanMakerCheckerConstraint men-DROP constraint tersebut.
    /// Sampai migration itu DITERAPKAN pada database ini (dotnet ef database update), hasilnya tetap "belum siap". Larangan
    /// pengaju menyetujui sendiri bagi akun tanpa SelfApprove tetap ditegakkan service (EnsureCanDecideAsync), bukan database.
    /// </summary>
    public async Task<bool> IsSelfApprovalPersistenceReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var definitions = await _dbContext.Database.SqlQueryRaw<string>(
                """
                SELECT pg_get_constraintdef(c.oid) AS "Value"
                FROM pg_constraint c
                JOIN pg_class t ON t.oid = c.conrelid
                JOIN pg_namespace n ON n.oid = t.relnamespace
                WHERE c.conname = 'CK_FinReceivableInstallmentPlan_MakerChecker'
                  AND t.relname = 'FinReceivableInstallmentPlan'
                  AND n.nspname = 'public'
                """).ToListAsync(cancellationToken);
            return definitions.Count == 0;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await _loggerService.WarningAsync(LogCategory, "InstallmentPlan.SelfApprovalReadiness",
                $"Status constraint {MakerCheckerConstraintName} tidak dapat dibaca; persetujuan pengajuan sendiri dianggap belum siap. {exception.GetType().Name}");
            return false;
        }
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
        if (request.BenefitOwnerWorkforceProfileId.HasValue)
            query = query.Where(x => x.receivable.BenefitOwnerWorkforceProfileId == request.BenefitOwnerWorkforceProfileId.Value);
        if (!string.IsNullOrWhiteSpace(request.DeductionPeriod))
            query = query.Where(x => x.plan.FirstDeductionPeriod == request.DeductionPeriod);
        if (request.StartDate.HasValue)
        {
            var fromUtc = FinanceBusinessDate.GetStartOfDayUtc(request.StartDate.Value);
            query = query.Where(x => x.plan.RequestedAt >= fromUtc);
        }
        if (request.EndDate.HasValue)
        {
            var toExclusiveUtc = FinanceBusinessDate.GetStartOfDayUtc(request.EndDate.Value.AddDays(1));
            query = query.Where(x => x.plan.RequestedAt < toExclusiveUtc);
        }
        if (request.HasArrears.HasValue)
        {
            var wantArrears = request.HasArrears.Value;
            query = query.Where(x => _dbContext.FinReceivableInstallments
                .Any(i => !i.IsDelete && i.PlanId == x.plan.Id && i.Status == FinReceivableInstallmentStatuses.Tertunggak) == wantArrears);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();

            // Pemilik benefit: nama atau NIP / nomor dokter. Pola yang sama dengan daftar piutang (identitas Workforce
            // untuk karyawan maupun dokter tetap, ditambah jalur lama lewat MstEmployee.Id).
            var matchingOwnerEmployees = _dbContext.MstEmployees.AsNoTracking()
                .Where(e => !e.IsDelete &&
                    (e.FullName.ToUpper().Contains(search) || e.EmployeeNumber.ToUpper().Contains(search)));
            var matchingOwnerEmployeeIds = matchingOwnerEmployees.Select(e => e.Id);
            var matchingOwnerEmployeeProfileIds = matchingOwnerEmployees.Select(e => e.WorkforceProfileId);
            var matchingOwnerDoctorProfileIds = _dbContext.MstDoctors.AsNoTracking()
                .Where(d => !d.IsDelete &&
                    (d.FullName.ToUpper().Contains(search) || d.DoctorNumber.ToUpper().Contains(search)))
                .Select(d => d.WorkforceProfileId);

            query = query.Where(x =>
                x.plan.PlanNumber.ToUpper().Contains(search) ||
                x.receivable.ReceivableNumber.ToUpper().Contains(search) ||
                (x.receivable.BenefitOwnerWorkforceProfileId.HasValue &&
                    (matchingOwnerEmployeeProfileIds.Contains(x.receivable.BenefitOwnerWorkforceProfileId.Value) ||
                     matchingOwnerDoctorProfileIds.Contains(x.receivable.BenefitOwnerWorkforceProfileId.Value))) ||
                (x.receivable.BenefitOwnerId.HasValue && matchingOwnerEmployeeIds.Contains(x.receivable.BenefitOwnerId.Value)));
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

        // Nama pemilik dan agregat angsuran untuk satu halaman diambil dalam kueri tetap (tidak ada N+1).
        var owners = await FinanceBenefitOwnerDirectory.LoadAsync(
            _dbContext,
            page.Select(x => ((Guid?)x.receivable.BenefitOwnerId, x.receivable.BenefitOwnerWorkforceProfileId)),
            cancellationToken);

        var planIds = page.Select(x => x.plan.Id).ToList();
        var installmentRows = planIds.Count == 0
            ? new List<InstallmentAggregateRow>()
            : await _dbContext.FinReceivableInstallments.AsNoTracking()
                .Where(i => !i.IsDelete && planIds.Contains(i.PlanId))
                .Select(i => new InstallmentAggregateRow(i.PlanId, i.PaidAmount, i.OutstandingAmount, i.Status))
                .ToListAsync(cancellationToken);
        var aggregates = installmentRows.GroupBy(x => x.PlanId).ToDictionary(g => g.Key, g => g.ToList());

        return new PagedResult<InstallmentPlanListResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = Math.Max(1, (int)Math.Ceiling(total / (double)request.PageSize)),
            Items = page.Select(x => MapList(
                x.plan, x.receivable, names,
                owners.Find(x.receivable.BenefitOwnerId, x.receivable.BenefitOwnerWorkforceProfileId),
                aggregates.GetValueOrDefault(x.plan.Id))).ToList()
        };
    }

    /// <summary>
    /// Ringkasan seluruh perjanjian (tidak mengikuti saringan daftar). "Tertunggak" = perjanjian DISETUJUI yang punya
    /// sedikitnya satu angsuran TERTUNGGAK; potongan gaji yang gagal tidak pernah dihitung sebagai terbayar.
    /// </summary>
    public async Task<InstallmentPlanSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var byStatus = await _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
            .Where(x => !x.IsDelete)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var arrearsCount = await _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == FinReceivableInstallmentPlanStatuses.Disetujui)
            .CountAsync(plan => _dbContext.FinReceivableInstallments
                .Any(i => !i.IsDelete && i.PlanId == plan.Id && i.Status == FinReceivableInstallmentStatuses.Tertunggak),
                cancellationToken);

        int CountOf(string status) => byStatus.Where(x => x.Status == status).Select(x => x.Count).FirstOrDefault();

        return new InstallmentPlanSummaryResponse
        {
            PendingCount = CountOf(FinReceivableInstallmentPlanStatuses.Menunggu),
            RunningCount = CountOf(FinReceivableInstallmentPlanStatuses.Disetujui),
            ArrearsCount = arrearsCount,
            CompletedCount = CountOf(FinReceivableInstallmentPlanStatuses.Selesai)
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

        var names = await GetUserNamesAsync(new Guid?[] { plan.RequestedBy, plan.ApprovedBy, plan.RejectedBy }, cancellationToken);
        var baseResponse = Map(plan, receivable?.ReceivableNumber, names);

        WorkforceBenefitOwner? owner = null;
        if (receivable is not null && (receivable.BenefitOwnerId.HasValue || receivable.BenefitOwnerWorkforceProfileId.HasValue))
        {
            var owners = await FinanceBenefitOwnerDirectory.LoadAsync(
                _dbContext,
                new[] { ((Guid?)receivable.BenefitOwnerId, receivable.BenefitOwnerWorkforceProfileId) },
                cancellationToken);
            owner = owners.Find(receivable.BenefitOwnerId, receivable.BenefitOwnerWorkforceProfileId);
        }

        // Konteks transaksi untuk pemutus: nomor billing dan pasien penerima layanan. Nomor RM di sini milik PASIEN
        // (bisa tanggungan), bukan milik pemilik benefit.
        string? invoiceNumber = null;
        DateTimeOffset? invoiceDate = null;
        var patients = new List<InstallmentPlanPatientResponse>();
        if (receivable is not null)
        {
            if (receivable.InvoiceId.HasValue)
            {
                var invoiceId = receivable.InvoiceId.Value;
                var invoice = await _dbContext.BilInvoices.AsNoTracking()
                    .Where(x => x.Id == invoiceId)
                    .Select(x => new { x.InvoiceNumber, x.InvoiceDate })
                    .FirstOrDefaultAsync(cancellationToken);
                invoiceNumber = invoice?.InvoiceNumber;
                invoiceDate = invoice?.InvoiceDate;
            }

            var receivableId = receivable.Id;
            patients = await (
                from item in _dbContext.FinReceivableItems.AsNoTracking()
                join p in _dbContext.MstPatients.AsNoTracking() on item.PatientId equals (Guid?)p.Id
                where !item.IsDelete && item.ReceivableId == receivableId
                select new InstallmentPlanPatientResponse
                {
                    PatientName = p.FullName,
                    MedicalRecordNumber = p.MedicalRecordNumber,
                    Amount = item.Amount
                }).ToListAsync(cancellationToken);
        }

        var isPending = plan.Status == FinReceivableInstallmentPlanStatuses.Menunggu;
        var (syncStatus, syncedCount, expectedCount) = await ReadPayrollSyncAsync(installments, cancellationToken);

        return new InstallmentPlanDetailResponse
        {
            BenefitRelationship = receivable?.BenefitRelationship,
            BenefitOwnerName = owner?.StaffName,
            BenefitOwnerEmployeeNumber = owner?.StaffNumber,
            BenefitOwnerType = owner?.OwnerType,
            BenefitOwnerWorkforceProfileId = owner?.WorkforceProfileId ?? receivable?.BenefitOwnerWorkforceProfileId,
            ReceivableStatus = receivable?.Status,
            ReceivableOriginalAmount = receivable?.OriginalAmount ?? 0m,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            Patients = patients,
            RejectedByName = plan.RejectedBy.HasValue ? names.GetValueOrDefault(plan.RejectedBy.Value) : null,
            RejectedAt = plan.RejectedAt,
            ApprovalBlockers = isPending ? GetApprovalBlockers(plan, receivable) : new List<string>(),
            SchedulePreview = isPending
                ? BuildScheduleRows(plan).Select(row => new InstallmentResponse
                {
                    InstallmentNumber = row.Number,
                    DeductionPeriod = row.Period,
                    ScheduledAmount = row.Amount,
                    OutstandingAmount = row.Amount,
                    Status = FinReceivableInstallmentStatuses.Dijadwalkan
                }).ToList()
                : new List<InstallmentResponse>(),
            PayrollSyncStatus = syncStatus,
            PayrollSyncedCount = syncedCount,
            PayrollExpectedCount = expectedCount,
            SelfApprovalPersistenceReady = await IsSelfApprovalPersistenceReadyAsync(cancellationToken),
            ReceivableOutstandingAmount = receivable?.OutstandingAmount ?? 0m,
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
            RequestedByUserId = baseResponse.RequestedByUserId,
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
        Guid id,
        ApproveInstallmentPlanRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken,
        bool canDecideOwnRequest = false)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_INSTALLMENT_PLAN_{id:N}", cancellationToken);

            var plan = await _dbContext.FinReceivableInstallmentPlans
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Perjanjian angsuran tidak ditemukan.");

            if (actorUserId == Guid.Empty)
                throw new InstallmentPlanValidationException("Identitas pengguna yang menyetujui tidak dapat ditentukan.");

            if (plan.Status != FinReceivableInstallmentPlanStatuses.Menunggu)
                throw new InstallmentPlanConflictException(
                    $"Perjanjian berstatus {plan.Status} tidak dapat disetujui. Hanya perjanjian MENUNGGU yang dapat diputuskan.");

            if (request.RowVersion.HasValue && request.RowVersion.Value != plan.RowVersion)
                throw new InstallmentPlanConflictException(
                    "Perjanjian telah diperbarui oleh pengguna lain sejak Anda membukanya. Muat ulang sebelum memutuskan.");

            // FIN-VAL-234 diganti kebijakan berbasis izin: pemutus yang sama dengan pengaju hanya lolos bila memegang
            // HrdInstallmentPlanApproval : SelfApprove (diputuskan controller lewat HasAccessAsync) DAN skema database sudah
            // mendukung. ApprovedBy selalu aktor sebenarnya; tidak pernah dipalsukan agar lolos constraint.
            await EnsureCanDecideAsync(plan, actorUserId, canDecideOwnRequest, isApproval: true, cancellationToken);

            var receivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => x.Id == plan.ReceivableId && !x.IsDelete, cancellationToken);

            // Kelayakan diperiksa ULANG pada saat memutuskan: piutang masih manfaat karyawan yang berjalan, sisa tidak berubah
            // (FIN-VAL-233: total disepakati MUST sama dengan sisa SAAT DISETUJUI), jumlah/nominal sah, dan periode potong
            // pertama masih di depan bulan berjalan.
            var blockers = GetApprovalBlockers(plan, receivable);
            if (blockers.Count > 0 || receivable is null)
                throw new InstallmentPlanValidationException(string.Join(" ", blockers));

            var now = DateTimeOffset.UtcNow;
            var createdInstallments = new List<FinReceivableInstallment>(plan.InstallmentCount);
            foreach (var row in BuildScheduleRows(plan))
            {
                var installment = new FinReceivableInstallment
                {
                    PlanId = plan.Id,
                    InstallmentNumber = row.Number,
                    DeductionPeriod = row.Period,
                    ScheduledAmount = row.Amount,
                    CarriedOverAmount = 0m,
                    PaidAmount = 0m,
                    OutstandingAmount = row.Amount,
                    Status = FinReceivableInstallmentStatuses.Dijadwalkan,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.FinReceivableInstallments.Add(installment);
                createdInstallments.Add(installment);
            }

            plan.Status = FinReceivableInstallmentPlanStatuses.Disetujui;
            plan.ApprovedBy = actorUserId;
            plan.ApprovedAt = now;
            // Catatan pemutus ditambahkan; catatan pengajuan Finance tidak ditimpa (belum ada kolom catatan keputusan terpisah).
            if (!string.IsNullOrWhiteSpace(request.Notes)) plan.Notes = ComposeDecisionNotes(plan.Notes, request.Notes);
            plan.UpdateDateTime = DateTime.UtcNow;
            plan.UpdateBy = actorUserId;
            plan.RowVersion = Guid.NewGuid();

            // BE-FIN-094, INT-FIN-HR-001 §P.2: diterbitkan ke HR Payroll di dalam transaksi yang
            // sama. Kegagalan sinkron (periode/komponen HR belum dikonfigurasi) dicatat, TIDAK
            // membatalkan persetujuan — lihat ringkasan kelas.
            if (receivable.BenefitOwnerId.HasValue || receivable.BenefitOwnerWorkforceProfileId.HasValue)
            {
                var syncResult = await _payrollSyncService.SyncAsync(
                    receivable.BenefitOwnerId, receivable.BenefitOwnerWorkforceProfileId, createdInstallments, actorUserId, cancellationToken);
                if (syncResult.SkippedCount > 0)
                {
                    await AuditAsync("Plan.PayrollSyncGap", plan.Id, actorUserId);
                }
            }

            await SaveWithConcurrencyGuardAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Plan.Approved", plan.Id, actorUserId);
            if (actorUserId == plan.RequestedBy) await AuditAsync("Plan.SelfDecision", plan.Id, actorUserId);

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

    public async Task<InstallmentPlanDetailResponse> RejectAsync(
        Guid id,
        RejectInstallmentPlanRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken,
        bool canDecideOwnRequest = false)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            // Kunci yang sama dengan ApproveAsync/CancelAsync: setuju dan tolak yang bersamaan tidak boleh keduanya berhasil.
            await AcquireLockAsync($"FIN_INSTALLMENT_PLAN_{id:N}", cancellationToken);

            if (actorUserId == Guid.Empty)
                throw new InstallmentPlanValidationException("Identitas pengguna yang menolak tidak dapat ditentukan.");
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
                throw new InstallmentPlanValidationException("Alasan penolakan wajib diisi.");

            var plan = await _dbContext.FinReceivableInstallmentPlans
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Perjanjian angsuran tidak ditemukan.");

            if (plan.Status != FinReceivableInstallmentPlanStatuses.Menunggu)
                throw new InstallmentPlanConflictException(
                    $"Perjanjian berstatus {plan.Status} tidak dapat ditolak. Hanya perjanjian MENUNGGU yang dapat diputuskan.");

            if (request.RowVersion.HasValue && request.RowVersion.Value != plan.RowVersion)
                throw new InstallmentPlanConflictException(
                    "Perjanjian telah diperbarui oleh pengguna lain sejak Anda membukanya. Muat ulang sebelum memutuskan.");

            // Kebijakan yang sama dengan persetujuan. Constraint database hanya mengunci ApprovedBy, sehingga menolak
            // pengajuan sendiri dapat tersimpan begitu izin SelfApprove dipegang.
            await EnsureCanDecideAsync(plan, actorUserId, canDecideOwnRequest, isApproval: false, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            plan.Status = FinReceivableInstallmentPlanStatuses.Ditolak;
            plan.RejectedBy = actorUserId;
            plan.RejectedAt = now;
            plan.RejectionReason = request.RejectionReason.Trim();
            plan.UpdateDateTime = DateTime.UtcNow;
            plan.UpdateBy = actorUserId;
            plan.RowVersion = Guid.NewGuid();

            await SaveWithConcurrencyGuardAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Plan.Rejected", plan.Id, actorUserId);
            if (actorUserId == plan.RequestedBy) await AuditAsync("Plan.SelfDecision", plan.Id, actorUserId);

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
    // Retry sinkron payroll — pemulihan bila payroll HR belum menerima jadwal saat persetujuan (periode/komponen HR belum
    // dikonfigurasi). Memakai SyncAsync yang sama dan idempoten (upsert per angsuran), jadi tidak menggandakan input.
    // Tidak mengubah status perjanjian, jadwal, ataupun saldo piutang.
    // ------------------------------------------------------------------------------------

    public async Task<InstallmentPayrollSyncResultResponse> RetryPayrollSyncAsync(
        Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_INSTALLMENT_PLAN_{id:N}", cancellationToken);

            if (actorUserId == Guid.Empty)
                throw new InstallmentPlanValidationException("Identitas pengguna yang menjalankan sinkron tidak dapat ditentukan.");

            var plan = await _dbContext.FinReceivableInstallmentPlans
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Perjanjian angsuran tidak ditemukan.");

            if (plan.Status != FinReceivableInstallmentPlanStatuses.Disetujui)
                throw new InstallmentPlanConflictException(
                    $"Perjanjian berstatus {plan.Status}; sinkron payroll hanya untuk perjanjian DISETUJUI.");

            var receivable = await _dbContext.FinReceivables.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == plan.ReceivableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Kartu piutang pemilik perjanjian ini tidak ditemukan.");

            if (!receivable.BenefitOwnerId.HasValue && !receivable.BenefitOwnerWorkforceProfileId.HasValue)
                throw new InstallmentPlanValidationException("Pemilik benefit piutang ini tidak dikenali sehingga jadwal tidak dapat dikirim ke payroll.");

            // Hanya angsuran DIJADWALKAN yang belum tersentuh hasil payroll. Angsuran yang sudah terbayar sebagian, tertunggak,
            // terbayar, atau dibatalkan TIDAK dikirim ulang, supaya nominalnya tidak dipotong dua kali.
            var pendingInstallments = await _dbContext.FinReceivableInstallments
                .Where(x => !x.IsDelete && x.PlanId == plan.Id && x.Status == FinReceivableInstallmentStatuses.Dijadwalkan)
                .OrderBy(x => x.InstallmentNumber)
                .ToListAsync(cancellationToken);

            var syncResult = new FinanceReceivablePayrollSyncResult();
            if (pendingInstallments.Count > 0)
            {
                syncResult = await _payrollSyncService.SyncAsync(
                    receivable.BenefitOwnerId, receivable.BenefitOwnerWorkforceProfileId, pendingInstallments, actorUserId, cancellationToken);
            }

            await SaveWithConcurrencyGuardAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync(syncResult.SkippedCount > 0 ? "Plan.PayrollSyncRetryGap" : "Plan.PayrollSyncRetried", plan.Id, actorUserId);

            return new InstallmentPayrollSyncResultResponse
            {
                SyncedCount = syncResult.SyncedCount,
                SkippedCount = syncResult.SkippedCount,
                SkipReasons = syncResult.SkipReasons,
                Plan = await GetDetailAsync(id, cancellationToken)
            };
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

    private const string PayrollInputSourceType = "FinanceReceivableInstallment";
    private static readonly Regex PeriodPattern = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Alasan perjanjian MENUNGGU tidak dapat disetujui menurut data TERKINI. Dipakai ApproveAsync (menolak keputusan) dan
    /// GetDetailAsync (memberi tahu pemutus sebelum menekan tombol) sehingga keduanya selalu sepakat.
    /// </summary>
    private static List<string> GetApprovalBlockers(FinReceivableInstallmentPlan plan, FinReceivable? receivable)
    {
        var blockers = new List<string>();
        if (receivable is null || receivable.IsDelete)
        {
            blockers.Add("Kartu piutang pemilik perjanjian ini tidak ditemukan.");
            return blockers;
        }

        if (receivable.DebtorType != FinReceivableDebtorTypes.EmployeeBenefit)
            blockers.Add($"Piutang berjenis {receivable.DebtorType}; perjanjian angsuran hanya untuk piutang manfaat karyawan.");

        if (receivable.Status is FinReceivableStatuses.Settled or FinReceivableStatuses.WrittenOff or FinReceivableStatuses.Cancelled)
            blockers.Add($"Piutang berstatus {receivable.Status} sehingga tidak dapat diangsur.");

        if (receivable.OutstandingAmount <= 0m)
            blockers.Add("Sisa piutang sudah nol.");
        else if (plan.TotalAgreedAmount != receivable.OutstandingAmount)
            blockers.Add(
                $"Sisa piutang sudah berubah menjadi {receivable.OutstandingAmount:N2} sejak perjanjian diajukan " +
                $"(disepakati {plan.TotalAgreedAmount:N2}). Ajukan ulang dengan angka yang benar.");

        if (plan.InstallmentCount is < 2 or > 60 || plan.InstallmentAmount <= 0m)
            blockers.Add("Jumlah angsuran harus 2 sampai 60 dan nominal angsuran harus lebih dari nol.");

        var currentPeriod = CurrentPeriod();
        if (!PeriodPattern.IsMatch(plan.FirstDeductionPeriod ?? string.Empty)
            || string.CompareOrdinal(plan.FirstDeductionPeriod, currentPeriod) <= 0)
        {
            blockers.Add(
                $"Periode potong gaji pertama ({plan.FirstDeductionPeriod}) sudah berjalan atau terlewat. " +
                $"Ajukan ulang dengan periode {AddMonths(currentPeriod, 1)} atau sesudahnya.");
        }

        return blockers;
    }

    /// <summary>
    /// Satu-satunya perhitungan jadwal, dipakai persetujuan DAN pratinjau detail. L.1.4: pembulatan sisa jatuh pada baris
    /// TERAKHIR — jumlah seluruh baris MUST sama persis dengan TotalAgreedAmount.
    /// </summary>
    private static List<(int Number, string Period, decimal Amount)> BuildScheduleRows(FinReceivableInstallmentPlan plan)
    {
        var rows = new List<(int Number, string Period, decimal Amount)>(plan.InstallmentCount);
        var period = plan.FirstDeductionPeriod;
        for (var number = 1; number <= plan.InstallmentCount; number++)
        {
            var scheduled = number < plan.InstallmentCount
                ? plan.InstallmentAmount
                : plan.TotalAgreedAmount - plan.InstallmentAmount * (plan.InstallmentCount - 1);
            rows.Add((number, period, scheduled));
            period = AddMonths(period, 1);
        }
        return rows;
    }

    /// <summary>Catatan keputusan ditambahkan setelah catatan pengajuan Finance (maks 500 karakter); catatan keputusan dipertahankan utuh lebih dulu.</summary>
    private static string ComposeDecisionNotes(string? existing, string decisionNote)
    {
        const int maxLength = 500;
        var decision = $"Catatan HRD: {decisionNote.Trim()}";
        if (decision.Length >= maxLength) return decision[..maxLength];
        if (string.IsNullOrWhiteSpace(existing)) return decision;

        var room = maxLength - decision.Length - 3;
        if (room <= 0) return decision;

        var financeNote = existing.Length <= room ? existing : existing[..(room - 1)] + "…";
        return $"{financeNote} | {decision}";
    }

    /// <summary>
    /// Status penerimaan jadwal oleh payroll, dari baris input variabel payroll yang TERSIMPAN (SourceType + SourceId angsuran).
    /// Bukan dari nilai balik sesaat saat persetujuan, jadi tetap benar ketika dibuka kemudian.
    /// </summary>
    private async Task<(string Status, int Synced, int Expected)> ReadPayrollSyncAsync(
        IReadOnlyCollection<FinReceivableInstallment> installments, CancellationToken cancellationToken)
    {
        var expectedIds = installments
            .Where(x => x.Status != FinReceivableInstallmentStatuses.Dibatalkan)
            .Select(x => x.Id)
            .ToList();
        if (expectedIds.Count == 0) return (FinanceInstallmentPayrollSyncStatuses.NoSchedule, 0, 0);

        var syncedIds = await _dbContext.TrxPayrollVariableInputs.AsNoTracking()
            .Where(x => !x.IsDelete
                && x.SourceType == PayrollInputSourceType
                && x.SourceId != null
                && expectedIds.Contains(x.SourceId.Value))
            .Select(x => x.SourceId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var synced = syncedIds.Count;
        var status = synced == 0
            ? FinanceInstallmentPayrollSyncStatuses.NotSynced
            : synced < expectedIds.Count
                ? FinanceInstallmentPayrollSyncStatuses.Partial
                : FinanceInstallmentPayrollSyncStatuses.Synced;
        return (status, synced, expectedIds.Count);
    }

    private async Task SaveWithConcurrencyGuardAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InstallmentPlanConflictException(
                "Perjanjian telah diperbarui oleh pengguna lain. Muat ulang sebelum mencoba lagi.", exception);
        }
    }

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

    /// <summary>
    /// Satu-satunya kebijakan "siapa boleh memutuskan pengajuan siapa" untuk setujui dan tolak. Pemutus berbeda dari pengaju
    /// selalu lolos (izin Approve/Reject sudah ditegakkan controller). Pemutus yang sama dengan pengaju butuh SelfApprove;
    /// persetujuan sendiri tambahan butuh skema database yang sudah diselaraskan (IsSelfApprovalPersistenceReadyAsync).
    /// </summary>
    private async Task EnsureCanDecideAsync(
        FinReceivableInstallmentPlan plan, Guid actorUserId, bool canDecideOwnRequest, bool isApproval, CancellationToken cancellationToken)
    {
        if (actorUserId != plan.RequestedBy) return;

        if (!canDecideOwnRequest)
            throw new InstallmentPlanForbiddenException(
                "Anda adalah pengaju perjanjian ini. Memutuskan pengajuan sendiri memerlukan hak akses khusus " +
                "\"Decide Own Installment Plan Request\" (HrdInstallmentPlanApproval : SelfApprove) yang diberikan administrator " +
                "lewat Manajemen Role. Atau mintakan keputusan kepada petugas HRD lain.");

        if (isApproval && !await IsSelfApprovalPersistenceReadyAsync(cancellationToken))
            throw new InstallmentPlanValidationException(
                "Persetujuan atas pengajuan sendiri belum dapat disimpan: constraint database " +
                "CK_FinReceivableInstallmentPlan_MakerChecker masih aktif (migration DropInstallmentPlanMakerCheckerConstraint belum diterapkan). " +
                "Mintakan persetujuan kepada petugas HRD lain, atau hubungi administrator sistem. Penolakan tetap dapat dilakukan.");
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
        RequestedByUserId = x.RequestedBy,
        RequestedAt = x.RequestedAt,
        ApprovedByName = x.ApprovedBy.HasValue ? names.GetValueOrDefault(x.ApprovedBy.Value) : null,
        ApprovedAt = x.ApprovedAt,
        RejectionReason = x.RejectionReason,
        CancelReason = x.CancelReason,
        Notes = x.Notes,
        RowVersion = x.RowVersion
    };

    private static InstallmentPlanListResponse MapList(
        FinReceivableInstallmentPlan x, FinReceivable receivable, Dictionary<Guid, string?> names,
        WorkforceBenefitOwner? owner, List<InstallmentAggregateRow>? installments) => new()
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
        RequestedByUserId = x.RequestedBy,
        RequestedAt = x.RequestedAt,
        ApprovedByName = x.ApprovedBy.HasValue ? names.GetValueOrDefault(x.ApprovedBy.Value) : null,
        ApprovedAt = x.ApprovedAt,
        RejectionReason = x.RejectionReason,
        CancelReason = x.CancelReason,
        Notes = x.Notes,
        RowVersion = x.RowVersion,
        BenefitOwnerId = receivable.BenefitOwnerId,
        BenefitOwnerWorkforceProfileId = receivable.BenefitOwnerWorkforceProfileId,
        BenefitRelationship = receivable.BenefitRelationship,
        BenefitOwnerName = owner?.StaffName,
        BenefitOwnerEmployeeNumber = owner?.StaffNumber,
        BenefitOwnerType = owner?.OwnerType,
        OutstandingAmount = receivable.OutstandingAmount,
        // Angsuran sebagian yang kemudian dibatalkan tetap membawa potongan yang sudah benar-benar terjadi.
        PaidAmount = installments?.Sum(i => i.PaidAmount) ?? 0m,
        ArrearsInstallmentCount = installments?.Count(i => i.Status == FinReceivableInstallmentStatuses.Tertunggak) ?? 0,
        ArrearsAmount = installments?.Where(i => i.Status == FinReceivableInstallmentStatuses.Tertunggak).Sum(i => i.OutstandingAmount) ?? 0m
    };

    /// <summary>Baris angsuran minimal untuk agregat daftar (terbayar dan tunggakan) tanpa memuat entitas penuh.</summary>
    private sealed record InstallmentAggregateRow(Guid PlanId, decimal PaidAmount, decimal OutstandingAmount, string Status);

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
/// <summary>Aktor tidak berwenang atas keputusan ini (izin khusus belum diberikan). Dipetakan controller ke 403.</summary>
public sealed class InstallmentPlanForbiddenException(string message) : Exception(message);
public sealed class InstallmentPlanConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
