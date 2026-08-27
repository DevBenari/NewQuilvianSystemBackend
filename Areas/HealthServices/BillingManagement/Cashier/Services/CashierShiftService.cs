using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;

public sealed class CashierShiftService
{
    private const string LogCategory = "HealthServices.BillingManagement.Cashier";
    private const decimal MaxMoneyAmount = 9999999999999999.99m;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingNumberSeriesService _numberSeries;
    private readonly LoggerService _loggerService;

    public CashierShiftService(
        ApplicationDbContext dbContext,
        BillingNumberSeriesService numberSeries,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _numberSeries = numberSeries;
        _loggerService = loggerService;
    }

    public async Task<CashierShiftResponse> OpenAsync(
        OpenShiftRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        ValidateOpen(request, idempotencyKey, actorUserId);
        var payloadHash = Hash(
            CashierShiftCommandTypes.Open,
            actorUserId.ToString("N"),
            request.RegisterId.ToString("N"),
            Money(request.OpeningCash),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        IDbContextTransaction? transaction = null;

        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"BIL_CASHIER_SHIFT_COMMAND_{idempotencyKey:N}", cancellationToken);
            var replay = await ReplayAsync<CashierShiftResponse>(
                idempotencyKey, CashierShiftCommandTypes.Open, payloadHash, actorUserId, cancellationToken);
            if (replay is not null)
            {
                await CommitAsync(transaction, cancellationToken);
                return replay;
            }

            await AcquireLockAsync($"BIL_CASHIER_{actorUserId:N}", cancellationToken);
            await AcquireLockAsync($"BIL_REGISTER_{request.RegisterId:N}", cancellationToken);
            if (await ActiveShifts().AnyAsync(
                    x => x.CashierId == actorUserId || x.RegisterId == request.RegisterId,
                    cancellationToken))
                throw new CashierShiftConflictException(
                    "Kasir atau register masih memiliki shift aktif.");

            var now = DateTimeOffset.UtcNow;
            var shift = new BilCashierShift
            {
                ShiftNumber = await _numberSeries.AllocateCashierShiftNumberAsync(
                    actorUserId, now, cancellationToken),
                CashierId = actorUserId,
                RegisterId = request.RegisterId,
                OpeningCash = request.OpeningCash,
                SystemCash = 0,
                PhysicalCash = 0,
                Variance = 0,
                Status = CashierShiftStatuses.Open,
                OpenedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilCashierShifts.Add(shift);
            var response = MapShift(shift);
            var command = Command(
                shift,
                CashierShiftCommandTypes.Open,
                actorUserId,
                actorRole,
                "CashierShift.Create",
                idempotencyKey,
                payloadHash,
                request.CorrelationId,
                request.CausationId,
                null,
                shift.Status,
                null,
                response,
                now);
            _dbContext.BilCashierShiftCommands.Add(command);

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
            throw new CashierShiftConflictException(
                "Shift tidak dapat dibuka karena kasir, register, correlation, atau idempotency key sudah diproses.",
                exception);
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

    public async Task<CashierShiftResponse> GetCurrentAsync(
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (actorUserId == Guid.Empty)
            throw new CashierShiftForbiddenException("Identitas kasir tidak valid.");
        var shift = await ActiveShifts().AsNoTracking()
            .SingleOrDefaultAsync(x => x.CashierId == actorUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Shift kasir aktif tidak ditemukan.");
        var pending = await _dbContext.BilCashierShiftHandovers.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.SourceShiftId == shift.Id
                    && x.Status == CashierShiftHandoverStatuses.Pending
                    && !x.IsDelete,
                cancellationToken);
        return MapShift(shift, pending);
    }

    public async Task<CashierShiftResponse> GetByIdAsync(
        Guid shiftId,
        CancellationToken cancellationToken)
    {
        if (shiftId == Guid.Empty)
            throw new CashierShiftValidationException("ShiftId wajib diisi.");
        var shift = await _dbContext.BilCashierShifts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == shiftId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Shift kasir tidak ditemukan.");
        var pending = await _dbContext.BilCashierShiftHandovers.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.SourceShiftId == shift.Id
                    && x.Status == CashierShiftHandoverStatuses.Pending
                    && !x.IsDelete,
                cancellationToken);
        return MapShift(shift, pending);
    }

    public async Task<CashierShiftResponse> HandoverAsync(
        Guid shiftId,
        HandoverShiftRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        ValidateHandover(shiftId, request, idempotencyKey, actorUserId);
        var commandType = actorUserId == request.ReceivingCashierId
            ? CashierShiftCommandTypes.HandoverConfirmed
            : CashierShiftCommandTypes.HandoverInitiated;
        var payloadHash = Hash(
            commandType,
            shiftId.ToString("N"),
            actorUserId.ToString("N"),
            request.ReceivingCashierId.ToString("N"),
            request.ExpectedRowVersion.ToString("N"),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        IDbContextTransaction? transaction = null;

        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"BIL_CASHIER_SHIFT_COMMAND_{idempotencyKey:N}", cancellationToken);
            var replay = await ReplayAsync<CashierShiftResponse>(
                idempotencyKey, commandType, payloadHash, actorUserId, cancellationToken);
            if (replay is not null)
            {
                await CommitAsync(transaction, cancellationToken);
                return replay;
            }

            await AcquireLockAsync($"BIL_CASHIER_SHIFT_{shiftId:N}", cancellationToken);
            var shift = await _dbContext.BilCashierShifts.SingleOrDefaultAsync(
                x => x.Id == shiftId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Shift kasir tidak ditemukan.");
            if (!CashierShiftStatuses.IsActive(shift.Status))
                throw new CashierShiftValidationException(
                    "Hanya shift aktif yang dapat diserahterimakan.");
            EnsureCurrent(shift, request.ExpectedRowVersion);
            var pending = await _dbContext.BilCashierShiftHandovers.SingleOrDefaultAsync(
                x => x.SourceShiftId == shift.Id
                    && x.Status == CashierShiftHandoverStatuses.Pending
                    && !x.IsDelete,
                cancellationToken);
            var now = DateTimeOffset.UtcNow;
            BilCashierShiftCommand command;
            CashierShiftResponse response;

            if (actorUserId == shift.CashierId)
            {
                if (request.ReceivingCashierId == shift.CashierId)
                    throw new CashierShiftValidationException(
                        "Kasir penerima harus berbeda dari kasir yang menyerahkan.");
                if (pending is not null)
                    throw new CashierShiftConflictException(
                        "Shift sudah memiliki handover yang menunggu konfirmasi.");
                await AcquireLockAsync($"BIL_CASHIER_{request.ReceivingCashierId:N}", cancellationToken);
                if (await ActiveShifts().AnyAsync(
                        x => x.CashierId == request.ReceivingCashierId,
                        cancellationToken))
                    throw new CashierShiftConflictException(
                        "Kasir penerima masih memiliki shift aktif.");

                pending = new BilCashierShiftHandover
                {
                    SourceShiftId = shift.Id,
                    SourceShift = shift,
                    OutgoingCashierId = shift.CashierId,
                    IncomingCashierId = request.ReceivingCashierId,
                    Status = CashierShiftHandoverStatuses.Pending,
                    Reason = request.Reason.Trim(),
                    InitiatedAt = now,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.BilCashierShiftHandovers.Add(pending);
                shift.RowVersion = Guid.NewGuid();
                Touch(shift, actorUserId);
                response = MapShift(shift, pending);
                command = Command(
                    shift,
                    commandType,
                    actorUserId,
                    actorRole,
                    "CashierShift.Handover",
                    idempotencyKey,
                    payloadHash,
                    request.CorrelationId,
                    request.CausationId,
                    shift.Status,
                    shift.Status,
                    request.Reason,
                    response,
                    now);
            }
            else if (actorUserId == request.ReceivingCashierId)
            {
                if (pending is null || pending.IncomingCashierId != actorUserId)
                    throw new CashierShiftForbiddenException(
                        "Handover hanya dapat dikonfirmasi oleh kasir penerima yang ditunjuk.");
                await AcquireLockAsync($"BIL_CASHIER_{actorUserId:N}", cancellationToken);
                await AcquireLockAsync($"BIL_REGISTER_{shift.RegisterId:N}", cancellationToken);
                if (await ActiveShifts().AnyAsync(
                        x => x.Id != shift.Id &&
                            (x.CashierId == actorUserId || x.RegisterId == shift.RegisterId),
                        cancellationToken))
                    throw new CashierShiftConflictException(
                        "Kasir penerima atau register memiliki shift aktif lain.");

                var sourceStatus = shift.Status;
                shift.Status = CashierShiftStatuses.HandedOver;
                shift.ClosedAt = now;
                shift.RowVersion = Guid.NewGuid();
                Touch(shift, actorUserId);
                var receivingShift = new BilCashierShift
                {
                    ShiftNumber = await _numberSeries.AllocateCashierShiftNumberAsync(
                        actorUserId, now, cancellationToken),
                    CashierId = actorUserId,
                    RegisterId = shift.RegisterId,
                    OpeningCash = checked(shift.OpeningCash + shift.SystemCash),
                    SystemCash = 0,
                    PhysicalCash = 0,
                    Variance = 0,
                    Status = CashierShiftStatuses.Open,
                    OpenedAt = now,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.BilCashierShifts.Add(receivingShift);
                pending.ReceivingShiftId = receivingShift.Id;
                pending.ReceivingShift = receivingShift;
                pending.Status = CashierShiftHandoverStatuses.Confirmed;
                pending.ConfirmedAt = now;
                pending.RowVersion = Guid.NewGuid();
                pending.UpdateDateTime = DateTime.UtcNow;
                pending.UpdateBy = actorUserId;
                response = MapShift(receivingShift);
                command = Command(
                    shift,
                    commandType,
                    actorUserId,
                    actorRole,
                    "CashierShift.Handover",
                    idempotencyKey,
                    payloadHash,
                    request.CorrelationId,
                    request.CausationId,
                    sourceStatus,
                    shift.Status,
                    request.Reason,
                    response,
                    now);
            }
            else
            {
                throw new CashierShiftForbiddenException(
                    "Handover hanya dapat dilakukan oleh kasir pemilik shift atau kasir penerima.");
            }

            _dbContext.BilCashierShiftCommands.Add(command);
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
            throw new CashierShiftConflictException(
                "Handover tidak dapat disimpan karena state, correlation, atau idempotency key sudah diproses.",
                exception);
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

    public async Task<CashierShiftResponse> CloseAsync(
        Guid shiftId,
        CloseShiftRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        ValidateClose(shiftId, request, idempotencyKey, actorUserId);
        var payloadHash = Hash(
            CashierShiftCommandTypes.Close,
            shiftId.ToString("N"),
            actorUserId.ToString("N"),
            Money(request.PhysicalCash),
            request.ExpectedRowVersion.ToString("N"),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return await ChangeShiftAsync(
            shiftId,
            idempotencyKey,
            CashierShiftCommandTypes.Close,
            payloadHash,
            actorUserId,
            actorRole,
            "CashierShift.Close",
            request.CorrelationId,
            request.CausationId,
            async (shift, now, cancellation) =>
            {
                if (shift.CashierId != actorUserId)
                    throw new CashierShiftForbiddenException(
                        "Hanya kasir pemilik shift yang dapat menutup shift.");
                if (!CashierShiftStatuses.IsActive(shift.Status))
                    throw new CashierShiftValidationException(
                        "Hanya shift aktif yang dapat ditutup.");
                EnsureCurrent(shift, request.ExpectedRowVersion);
                if (await _dbContext.BilCashierShiftHandovers.AsNoTracking().AnyAsync(
                        x => x.SourceShiftId == shift.Id
                            && x.Status == CashierShiftHandoverStatuses.Pending
                            && !x.IsDelete,
                        cancellation))
                    throw new CashierShiftValidationException(
                        "Selesaikan handover yang tertunda sebelum menutup shift.");
                var before = shift.Status;
                shift.PhysicalCash = request.PhysicalCash;
                shift.Variance = request.PhysicalCash - checked(shift.OpeningCash + shift.SystemCash);
                shift.Status = shift.Variance == 0
                    ? CashierShiftStatuses.Closed
                    : CashierShiftStatuses.ClosedWithVariance;
                shift.ClosedAt = now;
                shift.RowVersion = Guid.NewGuid();
                Touch(shift, actorUserId);
                return (before, (string?)null, MapShift(shift));
            },
            cancellationToken);
    }

    public async Task<CashVarianceResponse> ReviewVarianceAsync(
        Guid shiftId,
        ReviewVarianceRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        ValidateReview(shiftId, request, idempotencyKey, actorUserId);
        var payloadHash = Hash(
            CashierShiftCommandTypes.ReviewVariance,
            shiftId.ToString("N"),
            actorUserId.ToString("N"),
            request.ExpectedRowVersion.ToString("N"),
            request.Resolution.Trim(),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        IDbContextTransaction? transaction = null;

        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"BIL_CASHIER_SHIFT_COMMAND_{idempotencyKey:N}", cancellationToken);
            var replay = await ReplayAsync<CashVarianceResponse>(
                idempotencyKey,
                CashierShiftCommandTypes.ReviewVariance,
                payloadHash,
                actorUserId,
                cancellationToken);
            if (replay is not null)
            {
                await CommitAsync(transaction, cancellationToken);
                return replay;
            }

            await AcquireLockAsync($"BIL_CASHIER_SHIFT_{shiftId:N}", cancellationToken);
            var shift = await _dbContext.BilCashierShifts.SingleOrDefaultAsync(
                x => x.Id == shiftId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Shift kasir tidak ditemukan.");
            EnsureCurrent(shift, request.ExpectedRowVersion);
            if (shift.Status != CashierShiftStatuses.ClosedWithVariance)
                throw new CashierShiftValidationException(
                    "Hanya shift CLOSED_WITH_VARIANCE yang dapat direview.");

            var now = DateTimeOffset.UtcNow;
            var before = shift.Status;
            var review = new BilCashVarianceReview
            {
                ShiftId = shift.Id,
                Shift = shift,
                ReviewerId = actorUserId,
                Variance = shift.Variance,
                Resolution = request.Resolution.Trim(),
                Reason = request.Reason.Trim(),
                ReviewedAt = now,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilCashVarianceReviews.Add(review);
            shift.Status = CashierShiftStatuses.Reviewed;
            shift.RowVersion = Guid.NewGuid();
            Touch(shift, actorUserId);
            var response = new CashVarianceResponse
            {
                Id = review.Id,
                ShiftId = shift.Id,
                ReviewerId = review.ReviewerId,
                Variance = review.Variance,
                Resolution = review.Resolution,
                Reason = review.Reason,
                ReviewedAt = review.ReviewedAt,
                Shift = MapShift(shift)
            };
            var command = Command(
                shift,
                CashierShiftCommandTypes.ReviewVariance,
                actorUserId,
                actorRole,
                "CashierShift.Review",
                idempotencyKey,
                payloadHash,
                request.CorrelationId,
                request.CausationId,
                before,
                shift.Status,
                request.Reason,
                response,
                now);
            _dbContext.BilCashierShiftCommands.Add(command);

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
            throw new CashierShiftConflictException(
                "Review variance tidak dapat disimpan karena correlation atau idempotency key sudah diproses.",
                exception);
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

    public async Task<CashierShiftResponse> ReopenAsync(
        Guid shiftId,
        ReopenShiftRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        ValidateReopen(shiftId, request, idempotencyKey, actorUserId);
        var payloadHash = Hash(
            CashierShiftCommandTypes.Reopen,
            shiftId.ToString("N"),
            actorUserId.ToString("N"),
            request.ExpectedRowVersion.ToString("N"),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return await ChangeShiftAsync(
            shiftId,
            idempotencyKey,
            CashierShiftCommandTypes.Reopen,
            payloadHash,
            actorUserId,
            actorRole,
            "CashierShift.Reopen",
            request.CorrelationId,
            request.CausationId,
            async (shift, _, cancellation) =>
            {
                EnsureCurrent(shift, request.ExpectedRowVersion);
                if (shift.Status is not (CashierShiftStatuses.Closed or CashierShiftStatuses.Reviewed))
                    throw new CashierShiftValidationException(
                        "Hanya shift CLOSED atau REVIEWED yang dapat dibuka kembali.");
                await AcquireLockAsync($"BIL_CASHIER_{shift.CashierId:N}", cancellation);
                await AcquireLockAsync($"BIL_REGISTER_{shift.RegisterId:N}", cancellation);
                if (await ActiveShifts().AnyAsync(
                        x => x.Id != shift.Id &&
                            (x.CashierId == shift.CashierId || x.RegisterId == shift.RegisterId),
                        cancellation))
                    throw new CashierShiftConflictException(
                        "Kasir atau register sudah memiliki shift aktif lain.");
                var before = shift.Status;
                shift.Status = CashierShiftStatuses.Reopened;
                shift.ClosedAt = null;
                shift.RowVersion = Guid.NewGuid();
                Touch(shift, actorUserId);
                return (before, (string?)request.Reason.Trim(), MapShift(shift));
            },
            cancellationToken);
    }

    public async Task<BilCashierShift> RequireActiveShiftAsync(
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (actorUserId == Guid.Empty)
            throw new CashierShiftForbiddenException("Identitas kasir tidak valid.");
        var shift = await ActiveShifts().SingleOrDefaultAsync(
            x => x.CashierId == actorUserId, cancellationToken);
        if (shift is null)
            throw new CashierShiftValidationException(
                "Buka shift kasir sebelum menerima uang tunai.");
        await AcquireLockAsync($"BIL_CASHIER_SHIFT_{shift.Id:N}", cancellationToken);
        return shift;
    }

    public async Task<bool> ApplyCashReceiptAsync(
        BilCashierShift shift,
        string sourceType,
        Guid sourceId,
        decimal amount,
        Guid actorUserId,
        Guid correlationId,
        Guid causationId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(shift);
        if (string.IsNullOrWhiteSpace(sourceType) || sourceType.Trim().Length > 40
            || sourceId == Guid.Empty || correlationId == Guid.Empty || causationId == Guid.Empty)
            throw new CashierShiftValidationException("Sumber penerimaan tunai tidak valid.");
        ValidateMoney(amount, "Nominal penerimaan tunai", allowZero: false);
        if (shift.CashierId != actorUserId || !CashierShiftStatuses.IsActive(shift.Status))
            throw new CashierShiftValidationException(
                "Buka shift kasir sebelum menerima uang tunai.");

        var normalizedSource = sourceType.Trim().ToUpperInvariant();
        var localPrior = _dbContext.BilCashierShiftCommands.Local.Any(
            x => x.SourceType == normalizedSource && x.SourceId == sourceId);
        if (localPrior || await _dbContext.BilCashierShiftCommands.AsNoTracking().AnyAsync(
                x => x.SourceType == normalizedSource && x.SourceId == sourceId,
                cancellationToken))
            return false;

        var newSystemCash = checked(shift.SystemCash + amount);
        var expectedClosingCash = checked(shift.OpeningCash + newSystemCash);
        if (newSystemCash > MaxMoneyAmount || expectedClosingCash > MaxMoneyAmount)
            throw new CashierShiftValidationException(
                "Kas sistem shift melebihi batas nominal yang didukung.");
        var status = shift.Status;
        shift.SystemCash = newSystemCash;
        shift.RowVersion = Guid.NewGuid();
        Touch(shift, actorUserId);
        var response = MapShift(shift);
        var command = Command(
            shift,
            CashierShiftCommandTypes.CashReceipt,
            actorUserId,
            "Cashier",
            "CashierShift.CashReceipt",
            null,
            Hash(
                CashierShiftCommandTypes.CashReceipt,
                normalizedSource,
                sourceId.ToString("N"),
                Money(amount),
                actorUserId.ToString("N")),
            correlationId,
            causationId,
            status,
            status,
            null,
            response,
            occurredAt);
        command.SourceType = normalizedSource;
        command.SourceId = sourceId;
        command.Amount = amount;
        _dbContext.BilCashierShiftCommands.Add(command);
        return true;
    }

    public Task AuditCashReceiptAsync(
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var command = _dbContext.BilCashierShiftCommands.Local.SingleOrDefault(
            x => x.SourceId == sourceId && x.CommandType == CashierShiftCommandTypes.CashReceipt);
        return command is null ? Task.CompletedTask : AuditCommandAsync(command, false);
    }

    private async Task<CashierShiftResponse> ChangeShiftAsync(
        Guid shiftId,
        Guid idempotencyKey,
        string commandType,
        string payloadHash,
        Guid actorUserId,
        string actorRole,
        string authority,
        Guid correlationId,
        Guid causationId,
        Func<BilCashierShift, DateTimeOffset, CancellationToken,
            Task<(string BeforeStatus, string? Reason, CashierShiftResponse Response)>> change,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"BIL_CASHIER_SHIFT_COMMAND_{idempotencyKey:N}", cancellationToken);
            var replay = await ReplayAsync<CashierShiftResponse>(
                idempotencyKey, commandType, payloadHash, actorUserId, cancellationToken);
            if (replay is not null)
            {
                await CommitAsync(transaction, cancellationToken);
                return replay;
            }

            await AcquireLockAsync($"BIL_CASHIER_SHIFT_{shiftId:N}", cancellationToken);
            var shift = await _dbContext.BilCashierShifts.SingleOrDefaultAsync(
                x => x.Id == shiftId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Shift kasir tidak ditemukan.");
            var now = DateTimeOffset.UtcNow;
            var changed = await change(shift, now, cancellationToken);
            var command = Command(
                shift,
                commandType,
                actorUserId,
                actorRole,
                authority,
                idempotencyKey,
                payloadHash,
                correlationId,
                causationId,
                changed.BeforeStatus,
                shift.Status,
                changed.Reason,
                changed.Response,
                now);
            _dbContext.BilCashierShiftCommands.Add(command);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditCommandAsync(command, false);
            return changed.Response;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
        }
        catch (DbUpdateException exception)
        {
            await RollbackAsync(transaction);
            throw new CashierShiftConflictException(
                "Perubahan shift tidak dapat disimpan karena state, correlation, atau idempotency key sudah diproses.",
                exception);
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

    private IQueryable<BilCashierShift> ActiveShifts() =>
        _dbContext.BilCashierShifts.Where(
            x => !x.IsDelete &&
                (x.Status == CashierShiftStatuses.Open || x.Status == CashierShiftStatuses.Reopened));

    private async Task<T?> ReplayAsync<T>(
        Guid idempotencyKey,
        string commandType,
        string payloadHash,
        Guid actorUserId,
        CancellationToken cancellationToken)
        where T : class
    {
        var prior = await _dbContext.BilCashierShiftCommands.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (prior is null) return null;
        if (prior.CommandType != commandType
            || prior.PayloadHash != payloadHash
            || prior.ActorUserId != actorUserId)
            throw new CashierShiftConflictException(
                "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
        var response = JsonSerializer.Deserialize<T>(prior.ResponseJson, JsonOptions)
            ?? throw new CashierShiftConflictException(
                "Hasil command idempotent tidak dapat dibaca.");
        switch (response)
        {
            case CashierShiftResponse shift:
                shift.IsReplay = true;
                break;
            case CashVarianceResponse variance:
                variance.IsReplay = true;
                variance.Shift.IsReplay = true;
                break;
        }
        await AuditCommandAsync(prior, true);
        return response;
    }

    private static BilCashierShiftCommand Command<T>(
        BilCashierShift shift,
        string commandType,
        Guid actorUserId,
        string actorRole,
        string authority,
        Guid? idempotencyKey,
        string payloadHash,
        Guid correlationId,
        Guid causationId,
        string? statusBefore,
        string statusAfter,
        string? reason,
        T response,
        DateTimeOffset occurredAt) => new()
        {
            ShiftId = shift.Id,
            Shift = shift,
            CashierId = shift.CashierId,
            RegisterId = shift.RegisterId,
            EntityVersion = shift.RowVersion,
            CommandType = commandType,
            ActorUserId = actorUserId,
            ActorRole = NormalizeRole(actorRole),
            Authority = authority,
            IdempotencyKey = idempotencyKey,
            PayloadHash = payloadHash,
            CorrelationId = correlationId,
            CausationId = causationId,
            StatusBefore = statusBefore,
            StatusAfter = statusAfter,
            OpeningCash = shift.OpeningCash,
            SystemCash = shift.SystemCash,
            PhysicalCash = shift.PhysicalCash,
            Variance = shift.Variance,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            OccurredAt = occurredAt,
            ResponseJson = JsonSerializer.Serialize(response, JsonOptions),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

    private Task AuditCommandAsync(BilCashierShiftCommand command, bool isReplay) =>
        _loggerService.AuditAsync(
            LogCategory,
            $"CashierShift.{command.CommandType}",
            "Transisi shift kasir dicatat secara append-only.",
            new
            {
                command.ShiftId,
                command.CashierId,
                command.RegisterId,
                command.EntityVersion,
                command.CommandType,
                command.ActorUserId,
                command.ActorRole,
                command.Authority,
                command.StatusBefore,
                command.StatusAfter,
                command.OpeningCash,
                command.SystemCash,
                command.PhysicalCash,
                command.Variance,
                command.Amount,
                command.Reason,
                command.SourceType,
                command.SourceId,
                command.CorrelationId,
                command.CausationId,
                command.OccurredAt,
                IsReplay = isReplay
            });

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(
        IDbContextTransaction? transaction,
        CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    private static CashierShiftResponse MapShift(
        BilCashierShift shift,
        BilCashierShiftHandover? pending = null) => new()
        {
            Id = shift.Id,
            ShiftNumber = shift.ShiftNumber,
            CashierId = shift.CashierId,
            RegisterId = shift.RegisterId,
            OpeningCash = shift.OpeningCash,
            SystemCash = shift.SystemCash,
            ExpectedClosingCash = checked(shift.OpeningCash + shift.SystemCash),
            PhysicalCash = shift.PhysicalCash,
            Variance = shift.Variance,
            Status = shift.Status,
            OpenedAt = shift.OpenedAt,
            ClosedAt = shift.ClosedAt,
            RowVersion = shift.RowVersion,
            VarianceRequiresReview = shift.Status == CashierShiftStatuses.ClosedWithVariance,
            PendingHandoverStatus = pending?.Status,
            ReceivingCashierId = pending?.IncomingCashierId
        };

    private static void Touch(BilCashierShift shift, Guid actorUserId)
    {
        shift.UpdateDateTime = DateTime.UtcNow;
        shift.UpdateBy = actorUserId;
    }

    private static void EnsureCurrent(BilCashierShift shift, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || shift.RowVersion != expectedRowVersion)
            throw Stale();
    }

    private static CashierShiftConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private static void ValidateOpen(OpenShiftRequest request, Guid idempotencyKey, Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCommand(idempotencyKey, actorUserId, request.CorrelationId, request.CausationId);
        if (request.RegisterId == Guid.Empty)
            throw new CashierShiftValidationException("RegisterId wajib diisi.");
        ValidateMoney(request.OpeningCash, "Kas pembukaan", allowZero: true);
    }

    private static void ValidateHandover(
        Guid shiftId,
        HandoverShiftRequest request,
        Guid idempotencyKey,
        Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCommand(idempotencyKey, actorUserId, request.CorrelationId, request.CausationId);
        if (shiftId == Guid.Empty || request.ReceivingCashierId == Guid.Empty)
            throw new CashierShiftValidationException("ShiftId dan ReceivingCashierId wajib diisi.");
        ValidateExpectedVersion(request.ExpectedRowVersion);
        ValidateText(request.Reason, "Alasan handover");
    }

    private static void ValidateClose(
        Guid shiftId,
        CloseShiftRequest request,
        Guid idempotencyKey,
        Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCommand(idempotencyKey, actorUserId, request.CorrelationId, request.CausationId);
        if (shiftId == Guid.Empty)
            throw new CashierShiftValidationException("ShiftId wajib diisi.");
        ValidateExpectedVersion(request.ExpectedRowVersion);
        ValidateMoney(request.PhysicalCash, "Kas fisik", allowZero: true);
    }

    private static void ValidateReview(
        Guid shiftId,
        ReviewVarianceRequest request,
        Guid idempotencyKey,
        Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCommand(idempotencyKey, actorUserId, request.CorrelationId, request.CausationId);
        if (shiftId == Guid.Empty)
            throw new CashierShiftValidationException("ShiftId wajib diisi.");
        ValidateExpectedVersion(request.ExpectedRowVersion);
        ValidateText(request.Resolution, "Resolusi variance");
        ValidateText(request.Reason, "Alasan review");
    }

    private static void ValidateReopen(
        Guid shiftId,
        ReopenShiftRequest request,
        Guid idempotencyKey,
        Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCommand(idempotencyKey, actorUserId, request.CorrelationId, request.CausationId);
        if (shiftId == Guid.Empty)
            throw new CashierShiftValidationException("ShiftId wajib diisi.");
        ValidateExpectedVersion(request.ExpectedRowVersion);
        ValidateText(request.Reason, "Alasan reopen");
    }

    private static void ValidateCommand(
        Guid idempotencyKey,
        Guid actorUserId,
        Guid correlationId,
        Guid causationId)
    {
        if (idempotencyKey == Guid.Empty)
            throw new CashierShiftValidationException("Idempotency-Key wajib diisi.");
        if (actorUserId == Guid.Empty)
            throw new CashierShiftForbiddenException("Identitas pengguna tidak valid.");
        if (correlationId == Guid.Empty || causationId == Guid.Empty)
            throw new CashierShiftValidationException(
                "CorrelationId dan CausationId wajib diisi.");
    }

    private static void ValidateExpectedVersion(Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty)
            throw new CashierShiftValidationException("ExpectedRowVersion wajib diisi.");
    }

    private static void ValidateText(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new CashierShiftValidationException($"{name} wajib diisi.");
        if (value.Trim().Length > 500)
            throw new CashierShiftValidationException($"{name} maksimal 500 karakter.");
    }

    private static void ValidateMoney(decimal amount, string name, bool allowZero)
    {
        if (amount < 0 || !allowZero && amount == 0 || amount > MaxMoneyAmount
            || decimal.Round(amount, 2) != amount)
            throw new CashierShiftValidationException(
                $"{name} harus {(allowZero ? "nonnegatif" : "positif")} dan maksimal memiliki dua angka desimal.");
    }

    private static string NormalizeRole(string? actorRole)
    {
        var value = string.IsNullOrWhiteSpace(actorRole) ? "AuthenticatedUser" : actorRole.Trim();
        return value.Length <= 150 ? value : value[..150];
    }

    private static string Money(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Hash(params string[] values) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('|', values))));
}

public sealed class CashierShiftValidationException(string message) : Exception(message);
public sealed class CashierShiftForbiddenException(string message) : Exception(message);

public sealed class CashierShiftConflictException : Exception
{
    public CashierShiftConflictException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
