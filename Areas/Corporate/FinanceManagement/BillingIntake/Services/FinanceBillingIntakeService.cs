using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;

/// <summary>
/// Membaca handoff Billing, membuat piutang yang sesuai, lalu menandai ACK (FIN-DES-008,
/// 02-backend-architecture.md). Dibangun pada BE-FIN-009 karena tidak ada task roadmap
/// eksplisit yang memilikinya — gap ini dilaporkan pertama kali di BE-FIN-005, ditutup di sini
/// atas keputusan eksplisit pemilik repository (21 September 2026), bukan diselipkan sepihak.
///
/// Cakupan HANYA HandoffType AR (BilArHandoff → FinReceivable). AP/COLLECTION/ADJUSTMENT
/// TIDAK diproses — FinPayable dan FinReceipt/FinReceiptAllocation belum ada task pemilik
/// (BE-FIN-007 bagian 1). Baris intake dengan tipe itu akan tetap NEW tanpa penangan.
///
/// BE-FIN-011: setiap piutang yang berhasil diakui menulis kejadian PENGAKUAN-PIUTANG ke
/// FinAccountingEventOutbox lewat FinanceAccountingOutboxService, di dalam transaksi
/// ProcessArIntakeAsync yang sama (FIN-DES-017, FR-FIN-070).
/// </summary>
public sealed class FinanceBillingIntakeService
{
    private const string LogCategory = "Corporate.FinanceManagement.BillingIntake";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;

    public FinanceBillingIntakeService(ApplicationDbContext dbContext, LoggerService loggerService, FinanceAccountingOutboxService accountingOutboxService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _accountingOutboxService = accountingOutboxService;
    }

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<BillingIntakeResponse>> GetPagedAsync(BillingIntakeQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinBillingHandoffIntakes.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.HandoffType)) query = query.Where(x => x.HandoffType == request.HandoffType);

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => Map(x)).ToListAsync(cancellationToken);
        return new PagedResult<BillingIntakeResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<BillingIntakeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var intake = await _dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
        return Map(intake);
    }

    public async Task<BillingIntakeSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.FinBillingHandoffIntakes.AsNoTracking().Where(x => !x.IsDelete);
        var counts = await query.GroupBy(x => x.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        int Count(string status) => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;

        return new BillingIntakeSummaryResponse
        {
            TotalIntake = counts.Sum(c => c.Count),
            NewCount = Count(FinBillingHandoffIntakeStatuses.New),
            ErrorCount = Count(FinBillingHandoffIntakeStatuses.Error),
            ConsumedCount = Count(FinBillingHandoffIntakeStatuses.Consumed),
            AcknowledgedCount = Count(FinBillingHandoffIntakeStatuses.Acknowledged)
        };
    }

    public Task<BillingIntakeFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new BillingIntakeFilterMetadataResponse
        {
            PageSizeOptions = [10, 25, 50, 100],
            SortableFields = ["createDateTime", "status"],
            StatusOptions =
            [
                FinBillingHandoffIntakeStatuses.New, FinBillingHandoffIntakeStatuses.Consumed,
                FinBillingHandoffIntakeStatuses.Acknowledged, FinBillingHandoffIntakeStatuses.Error
            ]
        });

    // ------------------------------------------------------------------------------------
    // Penemuan fakta baru — belum ada hosted job yang menjadwalkannya (di luar cakupan
    // BE-FIN-009); dipicu manual lewat endpoint sampai ada keputusan penjadwalan otomatis.
    // ------------------------------------------------------------------------------------

    public async Task<int> SyncNewFactsAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        var existingKeys = (await _dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .Where(x => !x.IsDelete && x.HandoffType == FinBillingHandoffTypes.Ar)
            .Select(x => x.SourceHandoffKey)
            .ToListAsync(cancellationToken)).ToHashSet();

        var candidates = await _dbContext.BilArHandoffs.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == BillingHandoffStatuses.Created)
            .ToListAsync(cancellationToken);
        var toCreate = candidates.Where(x => !existingKeys.Contains(x.HandoffKey)).ToList();
        if (toCreate.Count == 0) return 0;

        var now = DateTime.UtcNow;
        foreach (var handoff in toCreate)
        {
            _dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.Ar,
                SourceHandoffId = handoff.Id,
                SourceHandoffKey = handoff.HandoffKey,
                Status = FinBillingHandoffIntakeStatuses.New,
                CorrelationId = handoff.CorrelationId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Race dengan sinkronisasi lain yang berjalan bersamaan — index unik
            // IX_FinBillingHandoffIntake_Identity sudah mencegah duplikat; percobaan ini
            // dianggap tidak menghasilkan baris baru, bukan kegagalan.
            foreach (var entry in _dbContext.ChangeTracker.Entries<FinBillingHandoffIntake>().Where(e => e.State == EntityState.Added).ToList())
                entry.State = EntityState.Detached;
            return 0;
        }
        return toCreate.Count;
    }

    // ------------------------------------------------------------------------------------
    // Pengolahan (FR-FIN-010..013) — satu-satunya jalur yang membuat FinReceivable dari fakta AR.
    // ------------------------------------------------------------------------------------

    public async Task<BillingIntakeResponse> ProcessAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var current = await _dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
        // FR-FIN-013: yang sudah berhasil diolah tidak dapat diulang.
        if (current.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
            throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");

        try
        {
            await ProcessArIntakeAsync(intakeId, actorUserId, cancellationToken);
        }
        catch (Exception exception) when (exception is not (KeyNotFoundException or BillingIntakeValidationException))
        {
            // Transaksi di ProcessArIntakeAsync sudah di-rollback; bersihkan change tracker
            // supaya percobaan yang gagal (mis. FinReceivable yang sempat di-Add) tidak ikut
            // tersimpan saat MarkErrorAsync memanggil SaveChangesAsync berikutnya.
            _dbContext.ChangeTracker.Clear();
            // FR-FIN-011: kegagalan tersimpan dan terlihat, bukan hilang diam-diam.
            await MarkErrorAsync(intakeId, exception.Message, actorUserId, cancellationToken);
        }

        var refreshed = await _dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .SingleAsync(x => x.Id == intakeId, cancellationToken);
        return Map(refreshed);
    }

    private async Task ProcessArIntakeAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BILLING_INTAKE_{intakeId:N}", cancellationToken);

            var intake = await _dbContext.FinBillingHandoffIntakes
                .SingleOrDefaultAsync(x => x.Id == intakeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Fakta masuk tidak ditemukan.");
            if (intake.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
                throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");
            if (intake.HandoffType != FinBillingHandoffTypes.Ar)
                throw new InvalidOperationException(
                    $"Konsumen untuk HandoffType '{intake.HandoffType}' belum dibangun (lihat laporan task BE-FIN-009 bagian 1).");

            var handoff = await _dbContext.BilArHandoffs
                .SingleOrDefaultAsync(x => x.Id == intake.SourceHandoffId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Fakta AR sumber tidak ditemukan di Billing.");

            // FIN-VAL-012: satu fakta AR menghasilkan satu piutang — lapis kedua di atas
            // IX_FinReceivable_SourceHandoffKey untuk pesan yang lebih jelas.
            if (await _dbContext.FinReceivables.AnyAsync(x => !x.IsDelete && x.SourceHandoffKey == handoff.HandoffKey, cancellationToken))
                throw new InvalidOperationException("Piutang untuk fakta ini sudah pernah dibuat.");

            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == handoff.InvoiceId, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var receivable = new FinReceivable
            {
                ReceivableNumber = GenerateReceivableNumber(),
                SourceHandoffKey = handoff.HandoffKey,
                SourceHandoffId = handoff.Id,
                InvoiceId = handoff.InvoiceId,
                // Billing hanya mengenal PAYER/PATIENT_GUARANTOR — nilai string sama persis
                // dengan FinReceivableDebtorTypes, disalin apa adanya (FIN-DES-024: EMPLOYEE_BENEFIT
                // belum punya jalur intake, OPEN DECISION, tidak pernah dihasilkan di sini).
                DebtorType = handoff.DebtorType,
                DebtorReferenceId = handoff.DebtorReferenceId,
                // FIN-DES-011 area: nilai disalin dari handoff, tidak pernah dihitung ulang.
                // BilArHandoff.Amount sudah berupa sisa tanggungan penjamin (FR-FIN-020) —
                // dihitung Billing sebelum handoff dibuat, bukan oleh Finance.
                OriginalAmount = handoff.Amount,
                OutstandingAmount = handoff.Amount,
                // BilArHandoff.DueDate nullable; tidak diatur eksplisit dokumen manapun bila
                // kosong — dipakai tanggal pengakuan hari ini sebagai nilai aman, dicatat di
                // laporan task, bukan keputusan bisnis baru.
                DueDate = handoff.DueDate.HasValue ? DateOnly.FromDateTime(handoff.DueDate.Value.UtcDateTime) : DateOnly.FromDateTime(now.UtcDateTime),
                Status = FinReceivableStatuses.Outstanding,
                ClaimStatus = FinReceivableClaimStatuses.NotRequired,
                RecognizedAt = now,
                CorrelationId = handoff.CorrelationId,
                CausationId = handoff.CausationId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            // Kardinalitas 1..* (02-backend-architecture.md §3.2): satu baris rincian minimal,
            // sebesar OriginalAmount. PatientId sengaja kosong pada task ini — perlu join lewat
            // Registration/Encounter yang belum diverifikasi skemanya di sesi ini (dicatat sebagai
            // penyederhanaan, bukan penghilangan FR-FIN-024 secara permanen).
            receivable.Items.Add(new FinReceivableItem
            {
                EncounterId = invoice?.EncounterId,
                InvoiceId = handoff.InvoiceId,
                Description = invoice is not null ? $"Piutang dari tagihan {invoice.InvoiceNumber}" : "Piutang dari tagihan Billing",
                Amount = handoff.Amount,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
            _dbContext.FinReceivables.Add(receivable);

            // BE-FIN-011, FIN-DES-017: kejadian PENGAKUAN-PIUTANG ditulis DI DALAM transaksi yang
            // sama dengan piutangnya sendiri (FR-FIN-070) — StageEventAsync hanya Add(), commit
            // sesungguhnya terjadi lewat SaveChangesAsync/CommitAsync di bawah, milik method ini.
            await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
            {
                EventTypeCode = FinAccountingEventTypeCodes.PengakuanPiutang,
                SourceTransactionId = receivable.ReceivableNumber,
                EventOccurredAt = now,
                AccountingDate = DateOnly.FromDateTime(now.UtcDateTime),
                Amount = receivable.OriginalAmount,
                CorrelationId = receivable.CorrelationId,
                CausationId = receivable.CausationId,
                ActorUserId = actorUserId
            }, cancellationToken);

            // FIN-BIL-005: Finance wajib mengirim ACK balik ke Billing setelah berhasil.
            handoff.Status = BillingHandoffStatuses.Acknowledged;
            handoff.AcknowledgedAt = now;
            handoff.RowVersion = Guid.NewGuid();

            intake.Status = FinBillingHandoffIntakeStatuses.Acknowledged;
            intake.TargetEntityId = receivable.Id;
            intake.ConsumedAt = now;
            intake.AcknowledgedAt = now;
            intake.ErrorMessage = null;
            intake.UpdateDateTime = DateTime.UtcNow;
            intake.UpdateBy = actorUserId;
            intake.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Intake.Acknowledged", intake.Id, actorUserId, receivable.Id);
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

    private async Task MarkErrorAsync(Guid intakeId, string errorMessage, Guid actorUserId, CancellationToken cancellationToken)
    {
        var intake = await _dbContext.FinBillingHandoffIntakes.SingleAsync(x => x.Id == intakeId, cancellationToken);
        intake.Status = FinBillingHandoffIntakeStatuses.Error;
        intake.ErrorMessage = Truncate(errorMessage, 1000);
        intake.RetryCount += 1;
        intake.UpdateDateTime = DateTime.UtcNow;
        intake.UpdateBy = actorUserId;
        intake.RowVersion = Guid.NewGuid();
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Intake.Error", intake.Id, actorUserId, null);
    }

    // ------------------------------------------------------------------------------------
    // Infrastruktur bersama
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

    // Nomor piutang tidak punya penomor seri resmi di modul ini (BillingNumberSeriesService
    // adalah milik BillingManagement) — dibuat unik lewat Guid, bukan Count/Max/Last+1
    // (QBE-CODE-002/003), sampai ada keputusan skema penomoran resmi untuk Finance.
    private static string GenerateReceivableNumber()
    {
        var candidate = $"AR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string Truncate(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    private Task AuditAsync(string action, Guid intakeId, Guid actorUserId, Guid? receivableId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceBillingIntake.{action}",
            $"Transisi fakta masuk Billing dicatat. IntakeId={intakeId} ReceivableId={receivableId?.ToString() ?? "-"}",
            new { IntakeId = intakeId, ReceivableId = receivableId, ActorUserId = actorUserId });

    private static BillingIntakeResponse Map(FinBillingHandoffIntake x) => new()
    {
        Id = x.Id,
        HandoffType = x.HandoffType,
        SourceHandoffId = x.SourceHandoffId,
        SourceHandoffKey = x.SourceHandoffKey,
        Status = x.Status,
        TargetEntityId = x.TargetEntityId,
        ConsumedAt = x.ConsumedAt,
        AcknowledgedAt = x.AcknowledgedAt,
        RetryCount = x.RetryCount,
        ErrorMessage = x.ErrorMessage,
        CorrelationId = x.CorrelationId,
        RowVersion = x.RowVersion,
        CreateDateTime = x.CreateDateTime
    };
}

public sealed class BillingIntakeValidationException(string message) : Exception(message);
