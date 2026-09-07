using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;

// BE-BKC-037 / PC-DES-001,003,005-013: seluruh perpindahan status voucher, jejak perintah
// append-only (meniru BilCashierShiftCommand), dan penegakan idempotency lewat replay ResponseJson.
// MUST NOT menulis BilPettyCashBudget.CurrentBalance sendiri - memanggil
// PettyCashBudgetService.ApplyDisbursementAsync di dalam transaction yang sama (PC-DES-004).
public sealed class PettyCashVoucherService
{
    private const string LogCategory = "HealthServices.BillingManagement.PettyCash";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingNumberSeriesService _numberSeries;
    private readonly PettyCashCategoryService _categoryService;
    private readonly PettyCashBudgetService _budgetService;
    private readonly LoggerService _loggerService;

    public PettyCashVoucherService(
        ApplicationDbContext dbContext,
        BillingNumberSeriesService numberSeries,
        PettyCashCategoryService categoryService,
        PettyCashBudgetService budgetService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _numberSeries = numberSeries;
        _categoryService = categoryService;
        _budgetService = budgetService;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------------------------
    // Read
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<PettyCashVoucherResponse>> GetPagedAsync(PettyCashVoucherQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.BilPettyCashVouchers.AsNoTracking().Include(x => x.Category).Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToUpperInvariant();
            query = query.Where(x => x.Status == status);
        }
        if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        if (request.StartDate.HasValue) query = query.Where(x => x.SubmittedAt >= request.StartDate.Value);
        if (request.EndDate.HasValue) query = query.Where(x => x.SubmittedAt <= request.EndDate.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToUpper();
            query = query.Where(x => x.VoucherNumber.ToUpper().Contains(keyword)
                || x.RecipientName.ToUpper().Contains(keyword)
                || x.Purpose.ToUpper().Contains(keyword));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "amount" => descending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.SubmittedAt) : query.OrderBy(x => x.SubmittedAt)
        };

        var total = await query.CountAsync(cancellationToken);
        var vouchers = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var names = await ResolveActorNamesAsync(vouchers.SelectMany(ActorIds), cancellationToken);
        var items = vouchers.Select(x => Map(x, x.Category.CategoryCode, x.Category.CategoryName, names)).ToList();

        return new PagedResult<PettyCashVoucherResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<PettyCashVoucherDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var voucher = await _dbContext.BilPettyCashVouchers.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Commands.OrderBy(c => c.OccurredAt))
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Voucher kas kecil tidak ditemukan.");

        var actorIds = ActorIds(voucher).Concat(voucher.Commands.Select(c => c.ActorUserId)).Distinct();
        var names = await ResolveActorNamesAsync(actorIds, cancellationToken);

        return new PettyCashVoucherDetailResponse
        {
            Voucher = Map(voucher, voucher.Category.CategoryCode, voucher.Category.CategoryName, names),
            Commands = voucher.Commands.Select(c => new PettyCashVoucherCommandResponse
            {
                Id = c.Id,
                CommandType = c.CommandType,
                ActorUserId = c.ActorUserId,
                ActorName = names.GetValueOrDefault(c.ActorUserId),
                ActorRole = c.ActorRole,
                StatusBefore = c.StatusBefore,
                StatusAfter = c.StatusAfter,
                Reason = c.Reason,
                OccurredAt = c.OccurredAt
            }).ToList()
        };
    }

    public async Task<PettyCashVoucherSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.BilPettyCashVouchers.AsNoTracking().Where(x => !x.IsDelete);
        var counts = await query.GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var waitingAmount = await query
            .Where(x => x.Status == PettyCashVoucherStatuses.WaitingApproval)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
        var cancelledCount = await query.CountAsync(x => x.IsCancel, cancellationToken);

        int Count(string status) => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;
        return new PettyCashVoucherSummaryResponse
        {
            WaitingApprovalCount = Count(PettyCashVoucherStatuses.WaitingApproval),
            ApprovedCount = Count(PettyCashVoucherStatuses.Approved),
            CashReceivedCount = Count(PettyCashVoucherStatuses.CashReceived),
            CompletedCount = Count(PettyCashVoucherStatuses.Completed),
            RejectedCount = Count(PettyCashVoucherStatuses.Rejected),
            CancelledCount = cancelledCount,
            TotalWaitingApprovalAmount = waitingAmount
        };
    }

    public Task<PettyCashVoucherFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new PettyCashVoucherFilterMetadataResponse
        {
            DefaultFilter = new PettyCashVoucherDefaultFilterResponse(),
            PageSizeOptions = [10, 25, 50, 100],
            StatusOptions =
            [
                new() { Value = PettyCashVoucherStatuses.WaitingApproval, Label = "Menunggu Persetujuan" },
                new() { Value = PettyCashVoucherStatuses.Approved, Label = "Disetujui" },
                new() { Value = PettyCashVoucherStatuses.CashReceived, Label = "Uang Diterima" },
                new() { Value = PettyCashVoucherStatuses.Completed, Label = "Selesai" },
                new() { Value = PettyCashVoucherStatuses.Rejected, Label = "Ditolak" }
            ]
        });

    // ------------------------------------------------------------------------------------
    // Write
    // ------------------------------------------------------------------------------------

    public async Task<PettyCashVoucherResponse> CreateAsync(
        CreatePettyCashVoucherRequest request, Guid idempotencyKey, Guid actorUserId, string actorRole, CancellationToken cancellationToken)
    {
        ValidateCreate(request, idempotencyKey, actorUserId);
        var payloadHash = Hash(
            "CREATE", request.RecipientName.Trim(), request.CategoryId.ToString("N"), Money(request.Amount), request.Purpose.Trim());
        IDbContextTransaction? transaction = null;

        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireVoucherCommandLockAsync(idempotencyKey, cancellationToken);
            var replay = await ReplayAsync(idempotencyKey, payloadHash, actorUserId, cancellationToken);
            if (replay is not null)
            {
                await CommitAsync(transaction, cancellationToken);
                return replay;
            }

            var category = await ValidateActiveCategoryAsync(request.CategoryId, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            var voucher = new BilPettyCashVoucher
            {
                VoucherNumber = await _numberSeries.AllocatePettyCashVoucherNumberAsync(actorUserId, now, cancellationToken),
                RecipientName = request.RecipientName.Trim(),
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Purpose = request.Purpose.Trim(),
                Status = PettyCashVoucherStatuses.WaitingApproval,
                RequestedBy = actorUserId,
                SubmittedAt = now,
                IdempotencyKey = idempotencyKey,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilPettyCashVouchers.Add(voucher);

            var names = await ResolveActorNamesAsync([actorUserId], cancellationToken);
            var response = Map(voucher, category.CategoryCode, category.CategoryName, names);
            var correlationId = Guid.NewGuid();
            var command = Command(
                voucher, voucher.RowVersion, PettyCashVoucherCommandTypes.Submit, actorUserId, actorRole,
                idempotencyKey, payloadHash, correlationId, correlationId, null, voucher.Status, null, response, now);
            _dbContext.BilPettyCashVoucherCommands.Add(command);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditCommandAsync(command, false);
            return response;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
        }
        catch (DbUpdateException exception)
        {
            await RollbackAsync(transaction);
            // BIL-VAL-058
            throw new PettyCashVoucherConflictException("Nomor voucher gagal dibuat. Coba lagi.", exception);
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

    public Task<PettyCashVoucherResponse> ApproveAsync(
        Guid voucherId, ApprovePettyCashVoucherRequest request, Guid idempotencyKey, Guid actorUserId, string actorRole, CancellationToken cancellationToken)
    {
        ValidateCommand(idempotencyKey, actorUserId);
        ValidateExpectedVersion(request.ExpectedRowVersion);
        var payloadHash = Hash(PettyCashVoucherCommandTypes.Approve, voucherId.ToString("N"), request.ExpectedRowVersion.ToString("N"));
        return ChangeVoucherAsync(voucherId, idempotencyKey, payloadHash, actorUserId, actorRole, null,
            async (voucher, now, correlationId, ct) =>
            {
                EnsureCurrent(voucher, request.ExpectedRowVersion);
                if (voucher.Status != PettyCashVoucherStatuses.WaitingApproval)
                    throw new PettyCashVoucherValidationException("Hanya voucher berstatus Menunggu Persetujuan yang dapat disetujui.");
                var before = voucher.Status;

                // PC-DES-005: kunci penasihat kolam yang sama diambil di sini supaya dua
                // persetujuan bersamaan tidak sama-sama lolos memeriksa komitmen sebelum
                // salah satunya benar-benar tercatat.
                if (_dbContext.Database.IsRelational()) await _budgetService.AcquireLockAsync(ct);
                var current = await _budgetService.GetCurrentAsync(ct);
                if (voucher.Amount > current.AvailableAmount)
                    // BIL-VAL-047
                    throw new PettyCashVoucherValidationException(
                        $"Nominal voucher melebihi sisa anggaran kas kecil yang tersedia. Sisa yang bisa dipakai saat ini Rp {current.AvailableAmount.ToString("N0", CultureInfo.InvariantCulture)}.");

                voucher.Status = PettyCashVoucherStatuses.Approved;
                voucher.DecidedBy = actorUserId;
                voucher.DecidedAt = now;
                voucher.RowVersion = Guid.NewGuid();
                return (before, PettyCashVoucherCommandTypes.Approve);
            }, cancellationToken);
    }

    public Task<PettyCashVoucherResponse> RejectAsync(
        Guid voucherId, RejectPettyCashVoucherRequest request, Guid idempotencyKey, Guid actorUserId, string actorRole, CancellationToken cancellationToken)
    {
        ValidateCommand(idempotencyKey, actorUserId);
        ValidateExpectedVersion(request.ExpectedRowVersion);
        // BIL-VAL-049
        ValidateReasonText(request.RejectionReason, "Alasan penolakan");
        var payloadHash = Hash(
            PettyCashVoucherCommandTypes.Reject, voucherId.ToString("N"), request.ExpectedRowVersion.ToString("N"), request.RejectionReason.Trim());
        return ChangeVoucherAsync(voucherId, idempotencyKey, payloadHash, actorUserId, actorRole, request.RejectionReason,
            (voucher, now, correlationId, ct) =>
            {
                EnsureCurrent(voucher, request.ExpectedRowVersion);
                if (voucher.Status != PettyCashVoucherStatuses.WaitingApproval)
                    throw new PettyCashVoucherValidationException("Voucher yang sudah diputuskan tidak dapat ditolak.");
                var before = voucher.Status;
                voucher.Status = PettyCashVoucherStatuses.Rejected;
                voucher.DecidedBy = actorUserId;
                voucher.DecidedAt = now;
                voucher.RejectionReason = request.RejectionReason.Trim();
                voucher.RowVersion = Guid.NewGuid();
                return Task.FromResult((before, PettyCashVoucherCommandTypes.Reject));
            }, cancellationToken);
    }

    public Task<PettyCashVoucherResponse> CancelAsync(
        Guid voucherId, CancelPettyCashVoucherRequest request, Guid idempotencyKey, Guid actorUserId, string actorRole, CancellationToken cancellationToken)
    {
        ValidateCommand(idempotencyKey, actorUserId);
        ValidateExpectedVersion(request.ExpectedRowVersion);
        ValidateReasonText(request.Reason, "Alasan pembatalan");
        var payloadHash = Hash(
            PettyCashVoucherCommandTypes.Cancel, voucherId.ToString("N"), request.ExpectedRowVersion.ToString("N"), request.Reason.Trim());
        return ChangeVoucherAsync(voucherId, idempotencyKey, payloadHash, actorUserId, actorRole, request.Reason,
            (voucher, now, correlationId, ct) =>
            {
                EnsureCurrent(voucher, request.ExpectedRowVersion);
                // BIL-VAL-050 (bagian 403)
                if (voucher.RequestedBy != actorUserId)
                    throw new PettyCashVoucherForbiddenException("Hanya pemohon voucher ini yang dapat membatalkannya.");
                // BIL-VAL-050 (bagian 422)
                if (voucher.Status != PettyCashVoucherStatuses.WaitingApproval)
                    throw new PettyCashVoucherValidationException("Voucher yang sudah diputuskan tidak dapat dibatalkan.");
                var before = voucher.Status;
                voucher.IsCancel = true;
                voucher.CancelDateTime = DateTime.UtcNow;
                voucher.CancelBy = actorUserId;
                voucher.RowVersion = Guid.NewGuid();
                return Task.FromResult((before, PettyCashVoucherCommandTypes.Cancel));
            }, cancellationToken);
    }

    public Task<PettyCashVoucherResponse> DisburseAsync(
        Guid voucherId, DisbursePettyCashVoucherRequest request, Guid idempotencyKey, Guid actorUserId, string actorRole, CancellationToken cancellationToken)
    {
        ValidateCommand(idempotencyKey, actorUserId);
        ValidateExpectedVersion(request.ExpectedRowVersion);
        var payloadHash = Hash(PettyCashVoucherCommandTypes.Disburse, voucherId.ToString("N"), request.ExpectedRowVersion.ToString("N"));
        return ChangeVoucherAsync(voucherId, idempotencyKey, payloadHash, actorUserId, actorRole, null,
            async (voucher, now, correlationId, ct) =>
            {
                EnsureCurrent(voucher, request.ExpectedRowVersion);
                // BIL-VAL-046
                if (voucher.Status != PettyCashVoucherStatuses.Approved)
                    throw new PettyCashVoucherValidationException("Voucher ini belum disetujui, jadi uangnya belum bisa diserahkan.");
                var before = voucher.Status;

                // PC-DES-004: satu-satunya penulis saldo dipanggil dari dalam transaction yang
                // sama; BIL-VAL-048 (saldo tidak cukup) dan BIL-VAL-057 (sudah pernah dicairkan)
                // ditegakkan di dalam ApplyDisbursementAsync, bukan diduplikasi di sini.
                await _budgetService.ApplyDisbursementAsync(voucher, actorUserId, correlationId, ct);

                voucher.Status = PettyCashVoucherStatuses.CashReceived;
                voucher.DisbursedBy = actorUserId;
                voucher.DisbursedAt = now;
                voucher.RowVersion = Guid.NewGuid();
                return (before, PettyCashVoucherCommandTypes.Disburse);
            }, cancellationToken);
    }

    public Task<PettyCashVoucherResponse> AttachProofAsync(
        Guid voucherId, AttachPettyCashProofRequest request, Guid idempotencyKey, Guid actorUserId, string actorRole, CancellationToken cancellationToken)
    {
        ValidateCommand(idempotencyKey, actorUserId);
        ValidateExpectedVersion(request.ExpectedRowVersion);
        if (string.IsNullOrWhiteSpace(request.ProofReferenceNumber))
            // BIL-VAL-051 (bagian 400)
            throw new PettyCashVoucherBadRequestException("Nomor nota atau kwitansi wajib diisi.");
        if (request.ProofReferenceNumber.Trim().Length > 60)
            throw new PettyCashVoucherBadRequestException("Nomor nota atau kwitansi maksimal 60 karakter.");
        var payloadHash = Hash(
            PettyCashVoucherCommandTypes.AttachProof, voucherId.ToString("N"), request.ExpectedRowVersion.ToString("N"), request.ProofReferenceNumber.Trim());
        return ChangeVoucherAsync(voucherId, idempotencyKey, payloadHash, actorUserId, actorRole, null,
            (voucher, now, correlationId, ct) =>
            {
                EnsureCurrent(voucher, request.ExpectedRowVersion);
                // BIL-VAL-051 (bagian 422)
                if (voucher.Status is not (PettyCashVoucherStatuses.CashReceived or PettyCashVoucherStatuses.Completed))
                    throw new PettyCashVoucherValidationException("Bukti nota hanya dapat dimasukkan setelah uang diserahkan.");
                var before = voucher.Status;
                var isCorrection = voucher.Status == PettyCashVoucherStatuses.Completed;
                voucher.ProofReferenceNumber = request.ProofReferenceNumber.Trim();
                voucher.ProofSubmittedBy = actorUserId;
                voucher.ProofSubmittedAt = now;
                if (!isCorrection)
                {
                    voucher.Status = PettyCashVoucherStatuses.Completed;
                    voucher.CompletedAt = now;
                }
                voucher.RowVersion = Guid.NewGuid();
                return Task.FromResult((before, isCorrection
                    ? PettyCashVoucherCommandTypes.ProofCorrected
                    : PettyCashVoucherCommandTypes.AttachProof));
            }, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Infrastruktur bersama
    // ------------------------------------------------------------------------------------

    private async Task<PettyCashVoucherResponse> ChangeVoucherAsync(
        Guid voucherId,
        Guid idempotencyKey,
        string payloadHash,
        Guid actorUserId,
        string actorRole,
        string? reason,
        Func<BilPettyCashVoucher, DateTimeOffset, Guid, CancellationToken, Task<(string StatusBefore, string CommandType)>> change,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireVoucherCommandLockAsync(idempotencyKey, cancellationToken);
            var replay = await ReplayAsync(idempotencyKey, payloadHash, actorUserId, cancellationToken);
            if (replay is not null)
            {
                await CommitAsync(transaction, cancellationToken);
                return replay;
            }

            await AcquireLockAsync($"BIL_PETTY_CASH_VOUCHER_{voucherId:N}", cancellationToken);
            var voucher = await _dbContext.BilPettyCashVouchers.Include(x => x.Category)
                .SingleOrDefaultAsync(x => x.Id == voucherId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Voucher kas kecil tidak ditemukan.");
            // BIL-VAL-052 - lapis kedua; struktural karena tidak ada endpoint yang menerima
            // voucher REJECTED sebagai target (PC-DES-013).
            if (voucher.Status == PettyCashVoucherStatuses.Rejected)
                throw new PettyCashVoucherValidationException(
                    "Voucher yang sudah ditolak tidak dapat diubah. Buat voucher baru bila pengeluarannya masih diperlukan.");

            var entityVersionBefore = voucher.RowVersion;
            var now = DateTimeOffset.UtcNow;
            var correlationId = Guid.NewGuid();
            var (statusBefore, commandType) = await change(voucher, now, correlationId, cancellationToken);

            var names = await ResolveActorNamesAsync(ActorIds(voucher), cancellationToken);
            var response = Map(voucher, voucher.Category.CategoryCode, voucher.Category.CategoryName, names);
            var command = Command(
                voucher, entityVersionBefore, commandType, actorUserId, actorRole,
                idempotencyKey, payloadHash, correlationId, correlationId, statusBefore, voucher.Status, reason, response, now);
            _dbContext.BilPettyCashVoucherCommands.Add(command);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditCommandAsync(command, false);
            return response;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
        }
        catch (DbUpdateException exception)
        {
            await RollbackAsync(transaction);
            throw new PettyCashVoucherConflictException(
                "Perubahan voucher tidak dapat disimpan karena state atau idempotency key sudah diproses.", exception);
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

    private async Task<Dictionary<Guid, string?>> ResolveActorNamesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var distinct = ids.Where(id => id != Guid.Empty).Distinct().ToList();
        if (distinct.Count == 0) return new Dictionary<Guid, string?>();
        return await _dbContext.Users.AsNoTracking()
            .Where(x => distinct.Contains(x.Id))
            .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    private static IEnumerable<Guid> ActorIds(BilPettyCashVoucher voucher)
    {
        yield return voucher.RequestedBy;
        if (voucher.DecidedBy.HasValue) yield return voucher.DecidedBy.Value;
        if (voucher.DisbursedBy.HasValue) yield return voucher.DisbursedBy.Value;
        if (voucher.ProofSubmittedBy.HasValue) yield return voucher.ProofSubmittedBy.Value;
    }

    private async Task<PettyCashCategoryResponse> ValidateActiveCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        PettyCashCategoryResponse category;
        try { category = await _categoryService.GetByIdAsync(categoryId, cancellationToken); }
        catch (KeyNotFoundException)
        {
            // BIL-VAL-045
            throw new PettyCashVoucherValidationException("Kategori yang dipilih sudah tidak aktif. Pilih kategori lain.");
        }
        if (!category.IsActive)
            throw new PettyCashVoucherValidationException("Kategori yang dipilih sudah tidak aktif. Pilih kategori lain.");
        return category;
    }

    // Tanpa perbandingan CommandType tersimpan - AttachProof/ProofCorrected memakai label hash
    // yang sama ("ATTACH_PROOF") walau CommandType yang benar-benar tercatat bisa berbeda
    // (koreksi nomor nota). PayloadHash + ActorUserId sudah cukup memverifikasi isi permintaan.
    private async Task<PettyCashVoucherResponse?> ReplayAsync(
        Guid idempotencyKey, string payloadHash, Guid actorUserId, CancellationToken cancellationToken)
    {
        var prior = await _dbContext.BilPettyCashVoucherCommands.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (prior is null) return null;
        if (prior.PayloadHash != payloadHash || prior.ActorUserId != actorUserId)
            throw new PettyCashVoucherConflictException("Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
        var response = JsonSerializer.Deserialize<PettyCashVoucherResponse>(prior.ResponseJson, JsonOptions)
            ?? throw new PettyCashVoucherConflictException("Hasil command idempotent tidak dapat dibaca.");
        response.IsReplay = true;
        await AuditCommandAsync(prior, true);
        return response;
    }

    private static BilPettyCashVoucherCommand Command(
        BilPettyCashVoucher voucher,
        Guid entityVersion,
        string commandType,
        Guid actorUserId,
        string actorRole,
        Guid idempotencyKey,
        string payloadHash,
        Guid correlationId,
        Guid causationId,
        string? statusBefore,
        string statusAfter,
        string? reason,
        PettyCashVoucherResponse response,
        DateTimeOffset occurredAt) => new()
        {
            VoucherId = voucher.Id,
            Voucher = voucher,
            CommandType = commandType,
            ActorUserId = actorUserId,
            ActorRole = NormalizeRole(actorRole),
            EntityVersion = entityVersion,
            StatusBefore = statusBefore,
            StatusAfter = statusAfter,
            Amount = voucher.Amount,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            IdempotencyKey = idempotencyKey,
            PayloadHash = payloadHash,
            CorrelationId = correlationId,
            CausationId = causationId,
            OccurredAt = occurredAt,
            ResponseJson = JsonSerializer.Serialize(response, JsonOptions),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

    // Privasi: RecipientName, Purpose, RejectionReason, dan ResponseJson (mengandung keduanya)
    // MUST NOT masuk log aplikasi (02-backend-architecture.md § Security, privacy...). Reason
    // command (RejectionReason/alasan batal) juga dikecualikan.
    private Task AuditCommandAsync(BilPettyCashVoucherCommand command, bool isReplay) =>
        _loggerService.AuditAsync(LogCategory, $"PettyCashVoucher.{command.CommandType}", "Transisi voucher kas kecil dicatat secara append-only.", new
        {
            command.VoucherId,
            command.CommandType,
            command.ActorUserId,
            command.ActorRole,
            command.EntityVersion,
            command.StatusBefore,
            command.StatusAfter,
            command.Amount,
            command.CorrelationId,
            command.CausationId,
            command.OccurredAt,
            IsReplay = isReplay
        });

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private Task AcquireVoucherCommandLockAsync(Guid idempotencyKey, CancellationToken cancellationToken) =>
        AcquireLockAsync($"BIL_PETTY_CASH_VOUCHER_COMMAND_{idempotencyKey:N}", cancellationToken);

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    private static void EnsureCurrent(BilPettyCashVoucher voucher, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || voucher.RowVersion != expectedRowVersion) throw Stale();
    }

    private static PettyCashVoucherConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private static void ValidateCreate(CreatePettyCashVoucherRequest request, Guid idempotencyKey, Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCommand(idempotencyKey, actorUserId);
        // BIL-VAL-044
        if (string.IsNullOrWhiteSpace(request.RecipientName) || request.RecipientName.Trim().Length > 150
            || request.CategoryId == Guid.Empty
            || request.Amount <= 0 || decimal.Round(request.Amount, 2) != request.Amount
            || string.IsNullOrWhiteSpace(request.Purpose) || request.Purpose.Trim().Length > 500)
            throw new PettyCashVoucherBadRequestException(
                "Nama Penerima, Kategori, Nominal Voucher, dan Tujuan wajib diisi. Nominal harus lebih besar dari nol.");
    }

    private static void ValidateCommand(Guid idempotencyKey, Guid actorUserId)
    {
        if (idempotencyKey == Guid.Empty) throw new PettyCashVoucherBadRequestException("Idempotency-Key wajib diisi.");
        if (actorUserId == Guid.Empty) throw new PettyCashVoucherForbiddenException("Identitas pengguna tidak valid.");
    }

    private static void ValidateExpectedVersion(Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty) throw new PettyCashVoucherBadRequestException("ExpectedRowVersion wajib diisi.");
    }

    private static void ValidateReasonText(string? value, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new PettyCashVoucherValidationException($"{label} wajib diisi.");
        if (value.Trim().Length > 500) throw new PettyCashVoucherValidationException($"{label} maksimal 500 karakter.");
    }

    private static string NormalizeRole(string? actorRole)
    {
        var value = string.IsNullOrWhiteSpace(actorRole) ? "AuthenticatedUser" : actorRole.Trim();
        return value.Length <= 150 ? value : value[..150];
    }

    private static string Money(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Hash(params string[] values) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('|', values))));

    private static string StatusLabel(string status) => status switch
    {
        PettyCashVoucherStatuses.WaitingApproval => "Menunggu Persetujuan",
        PettyCashVoucherStatuses.Approved => "Disetujui",
        PettyCashVoucherStatuses.CashReceived => "Uang Diterima",
        PettyCashVoucherStatuses.Completed => "Selesai",
        PettyCashVoucherStatuses.Rejected => "Ditolak",
        _ => status
    };

    private static List<string> AvailableActions(BilPettyCashVoucher voucher) => voucher.Status switch
    {
        PettyCashVoucherStatuses.WaitingApproval => ["APPROVE", "REJECT", "CANCEL"],
        PettyCashVoucherStatuses.Approved => ["DISBURSE"],
        PettyCashVoucherStatuses.CashReceived => ["ATTACH_PROOF"],
        PettyCashVoucherStatuses.Completed => ["ATTACH_PROOF"],
        _ => []
    };

    private static PettyCashVoucherResponse Map(
        BilPettyCashVoucher voucher, string categoryCode, string categoryName, IReadOnlyDictionary<Guid, string?> names) => new()
        {
            Id = voucher.Id,
            VoucherNumber = voucher.VoucherNumber,
            RecipientName = voucher.RecipientName,
            CategoryId = voucher.CategoryId,
            CategoryCode = categoryCode,
            CategoryName = categoryName,
            Amount = voucher.Amount,
            Purpose = voucher.Purpose,
            Status = voucher.Status,
            StatusLabel = StatusLabel(voucher.Status),
            IsCancelled = voucher.IsCancel,
            SubmittedAt = voucher.SubmittedAt,
            DecidedAt = voucher.DecidedAt,
            DisbursedAt = voucher.DisbursedAt,
            ProofSubmittedAt = voucher.ProofSubmittedAt,
            CompletedAt = voucher.CompletedAt,
            RequestedBy = voucher.RequestedBy,
            RequestedByName = names.GetValueOrDefault(voucher.RequestedBy),
            DecidedBy = voucher.DecidedBy,
            DecidedByName = voucher.DecidedBy.HasValue ? names.GetValueOrDefault(voucher.DecidedBy.Value) : null,
            DisbursedBy = voucher.DisbursedBy,
            DisbursedByName = voucher.DisbursedBy.HasValue ? names.GetValueOrDefault(voucher.DisbursedBy.Value) : null,
            RejectionReason = voucher.RejectionReason,
            ProofReferenceNumber = voucher.ProofReferenceNumber,
            AvailableActions = AvailableActions(voucher),
            RowVersion = voucher.RowVersion
        };
}

public sealed class PettyCashVoucherValidationException(string message) : Exception(message);
public sealed class PettyCashVoucherBadRequestException(string message) : Exception(message);
public sealed class PettyCashVoucherForbiddenException(string message) : Exception(message);

public sealed class PettyCashVoucherConflictException : Exception
{
    public PettyCashVoucherConflictException(string message, Exception? innerException = null) : base(message, innerException) { }
}
