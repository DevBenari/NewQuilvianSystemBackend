using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwPatientCallService
    {
        public const string ActiveCallIndexName = "IX_NvwPatientCall_BedId_Active";
        public const string IdempotencyKeyIndexName = "IX_NvwPatientCallEvent_IdempotencyKey";

        private const int MaxPressAttempts = 3;
        private const int MinCloseNoteLength = 5;
        private const int MaxCloseNoteLength = 250;

        private readonly ApplicationDbContext _dbContext;
        private readonly NvwBedTabletContextService _tabletContext;
        private readonly NvwPatientCallQueryService _queries;
        private readonly NvwRealtimeService _realtime;

        public NvwPatientCallService(
            ApplicationDbContext dbContext,
            NvwBedTabletContextService tabletContext,
            NvwPatientCallQueryService queries,
            NvwRealtimeService realtime)
        {
            _dbContext = dbContext;
            _tabletContext = tabletContext;
            _queries = queries;
            _realtime = realtime;
        }

        public async Task<NvwResult<NvwTabletCallOutcome>> PressAsync(
            Guid userId,
            string? tokenSessionStamp,
            string? idempotencyKey,
            NvwTabletCallRequest? request,
            CancellationToken cancellationToken)
        {
            var resolved = await _tabletContext.ResolveActiveTabletAsync(userId, tokenSessionStamp, cancellationToken);
            if (!resolved.IsSuccess || resolved.Value == null)
                return TabletUnavailable(resolved);

            var tablet = new TabletRef(resolved.Value.Id, resolved.Value.BedId, resolved.Value.DeviceCode);

            if (!TryParseKey(idempotencyKey, out var key))
                return NvwResult<NvwTabletCallOutcome>.Invalid(NvwErrorCodes.IdempotencyKeyInvalid, NvwMessages.IdempotencyKeyInvalid);

            if (request == null || !Enum.IsDefined(request.Priority))
                return NvwResult<NvwTabletCallOutcome>.Invalid(NvwErrorCodes.CallPriorityUnknown, NvwMessages.CallPriorityUnknown);

            var replay = await ReplayAsync(key, tablet, cancellationToken);
            if (replay != null)
                return replay;

            var bed = await LoadBedAsync(tablet.BedId, cancellationToken);

            var placement = await _dbContext.InpBedPlacements.AsNoTracking()
                .Where(x => x.BedId == tablet.BedId && x.EndDateTime == null && !x.IsDelete)
                .OrderByDescending(x => x.StartDateTime)
                .Select(x => new { x.Id, x.EpisodeId })
                .FirstOrDefaultAsync(cancellationToken);

            if (placement == null)
                return NvwResult<NvwTabletCallOutcome>.Conflict(NvwErrorCodes.BedNotOccupied, NvwMessages.BedNotOccupied);

            for (var attempt = 1; attempt <= MaxPressAttempts; attempt++)
            {
                var now = DateTime.UtcNow;
                var call = await FindActiveCallAsync(tablet.BedId, cancellationToken);
                NvwPatientCallEventType eventType;

                if (call == null)
                {
                    call = new NvwPatientCall
                    {
                        BedId = tablet.BedId,
                        BedTabletId = tablet.Id,
                        InpBedPlacementId = placement.Id,
                        InpEpisodeId = placement.EpisodeId,
                        FloorCode = bed.FloorCode,
                        Priority = request.Priority,
                        CallStatus = NvwPatientCallStatus.Waiting,
                        FirstPressedAt = now,
                        LastPressedAt = now,
                        PressCount = 1,
                        Version = 0,
                        CreateDateTime = now,
                        CreateBy = userId
                    };
                    _dbContext.NvwPatientCalls.Add(call);
                    eventType = NvwPatientCallEventType.Pressed;
                }
                else if (request.Priority == NvwPatientCallPriority.Emergency &&
                         call.Priority == NvwPatientCallPriority.Normal)
                {
                    call.Priority = NvwPatientCallPriority.Emergency;
                    call.EscalatedAt = now;
                    call.LastPressedAt = now;
                    Touch(call, userId, now);
                    eventType = NvwPatientCallEventType.EscalatedToEmergency;
                }
                else
                {
                    call.PressCount++;
                    call.LastPressedAt = now;
                    Touch(call, userId, now);
                    eventType = NvwPatientCallEventType.PressedAgain;
                }

                AddEvent(call.Id, eventType, userId, key, now, null);

                var saved = await SaveAsync(cancellationToken);
                if (saved == SaveOutcome.Saved)
                {
                    await _realtime.NotifyPatientCallChangedAsync(call, eventType, CancellationToken.None);
                    return NvwResult<NvwTabletCallOutcome>.Ok(await OutcomeAsync(call, tablet, bed.BedCode, eventType, cancellationToken));
                }

                _dbContext.ChangeTracker.Clear();

                if (saved == SaveOutcome.DuplicateKey)
                    return await ReplayAsync(key, tablet, cancellationToken) ??
                           NvwResult<NvwTabletCallOutcome>.Conflict(NvwErrorCodes.CallVersionMismatch, NvwMessages.CallVersionMismatch);
            }

            return NvwResult<NvwTabletCallOutcome>.Conflict(NvwErrorCodes.CallVersionMismatch, NvwMessages.CallVersionMismatch);
        }

        public Task<NvwResult<NvwTabletCallOutcome>> CancelByPatientAsync(
            Guid userId,
            string? tokenSessionStamp,
            string? idempotencyKey,
            NvwTabletCallVersionRequest? request,
            CancellationToken cancellationToken) =>
            CloseFromTabletAsync(userId, tokenSessionStamp, idempotencyKey, request, true, cancellationToken);

        public Task<NvwResult<NvwTabletCallOutcome>> ConfirmNurseArrivedAsync(
            Guid userId,
            string? tokenSessionStamp,
            string? idempotencyKey,
            NvwTabletCallVersionRequest? request,
            CancellationToken cancellationToken) =>
            CloseFromTabletAsync(userId, tokenSessionStamp, idempotencyKey, request, false, cancellationToken);

        private async Task<NvwResult<NvwTabletCallOutcome>> CloseFromTabletAsync(
            Guid userId,
            string? tokenSessionStamp,
            string? idempotencyKey,
            NvwTabletCallVersionRequest? request,
            bool isCancel,
            CancellationToken cancellationToken)
        {
            var resolved = await _tabletContext.ResolveActiveTabletAsync(userId, tokenSessionStamp, cancellationToken);
            if (!resolved.IsSuccess || resolved.Value == null)
                return TabletUnavailable(resolved);

            var tablet = new TabletRef(resolved.Value.Id, resolved.Value.BedId, resolved.Value.DeviceCode);

            if (!TryParseKey(idempotencyKey, out var key))
                return NvwResult<NvwTabletCallOutcome>.Invalid(NvwErrorCodes.IdempotencyKeyInvalid, NvwMessages.IdempotencyKeyInvalid);

            var replay = await ReplayAsync(key, tablet, cancellationToken);
            if (replay != null)
                return replay;

            var call = await FindActiveCallAsync(tablet.BedId, cancellationToken);
            if (call == null)
            {
                var hasFinishedCall = await _dbContext.NvwPatientCalls.AsNoTracking()
                    .AnyAsync(x => x.BedId == tablet.BedId && !x.IsDelete, cancellationToken);

                return hasFinishedCall
                    ? NvwResult<NvwTabletCallOutcome>.Conflict(NvwErrorCodes.CallFinished, NvwMessages.CallFinished)
                    : NvwResult<NvwTabletCallOutcome>.NotFound(NvwErrorCodes.NoActiveCall, NvwMessages.NoActiveCall);
            }

            if (isCancel &&
                (call.CallStatus != NvwPatientCallStatus.Waiting || call.Priority != NvwPatientCallPriority.Normal))
                return NvwResult<NvwTabletCallOutcome>.Conflict(NvwErrorCodes.CallCancelNotAllowed, NvwMessages.CallCancelNotAllowed);

            if (request == null || call.Version != request.Version)
                return NvwResult<NvwTabletCallOutcome>.Conflict(NvwErrorCodes.CallVersionMismatch, NvwMessages.CallVersionMismatch);

            var now = DateTime.UtcNow;
            NvwPatientCallEventType eventType;

            if (isCancel)
            {
                call.CallStatus = NvwPatientCallStatus.Cancelled;
                call.CloseReason = NvwPatientCallCloseReason.CancelledByPatient;
                call.ClosedByUserId = null;
                eventType = NvwPatientCallEventType.CancelledByPatient;
            }
            else
            {
                call.CallStatus = NvwPatientCallStatus.Handled;
                call.CloseReason = NvwPatientCallCloseReason.NurseArrived;
                call.IsBedsideConfirmed = true;
                call.ClosedByUserId = call.ClaimedByUserId;
                eventType = NvwPatientCallEventType.NurseArrived;
            }

            call.ClosedAt = now;
            Touch(call, userId, now);
            AddEvent(call.Id, eventType, userId, key, now, call.CloseReason);

            var saved = await SaveAsync(cancellationToken);
            if (saved == SaveOutcome.Saved)
            {
                await _realtime.NotifyPatientCallChangedAsync(call, eventType, CancellationToken.None);
                var bed = await LoadBedAsync(tablet.BedId, cancellationToken);
                return NvwResult<NvwTabletCallOutcome>.Ok(await OutcomeAsync(call, tablet, bed.BedCode, eventType, cancellationToken));
            }

            _dbContext.ChangeTracker.Clear();

            if (saved == SaveOutcome.DuplicateKey)
            {
                var duplicate = await ReplayAsync(key, tablet, cancellationToken);
                if (duplicate != null)
                    return duplicate;
            }

            return NvwResult<NvwTabletCallOutcome>.Conflict(NvwErrorCodes.CallVersionMismatch, NvwMessages.CallVersionMismatch);
        }

        public async Task<NvwResult<NvwConsoleCallResponse>> ClaimAsync(
            Guid callId,
            NvwConsoleCallVersionRequest? request,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var call = await FindCallAsync(callId, cancellationToken);
            var blocked = RejectFinishedOrMissing(call);
            if (blocked != null)
                return blocked;

            if (call!.CallStatus == NvwPatientCallStatus.Claimed)
                return NvwResult<NvwConsoleCallResponse>.Conflict(
                    NvwErrorCodes.CallAlreadyClaimed,
                    string.Format(NvwMessages.CallAlreadyClaimedFormat, await UserNameAsync(call.ClaimedByUserId, cancellationToken)));

            if (request == null || call.Version != request.Version)
                return VersionMismatch();

            var now = DateTime.UtcNow;
            call.CallStatus = NvwPatientCallStatus.Claimed;
            call.ClaimedByUserId = userId;
            call.ClaimedAt = now;
            Touch(call, userId, now);
            AddNurseEvent(call.Id, NvwPatientCallEventType.Claimed, userId, null, null, null, now);

            return await SaveConsoleAsync(call, userId, NvwPatientCallEventType.Claimed, cancellationToken);
        }

        public async Task<NvwResult<NvwConsoleCallResponse>> TakeOverAsync(
            Guid callId,
            NvwConsoleCallVersionRequest? request,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var call = await FindCallAsync(callId, cancellationToken);
            var blocked = RejectFinishedOrMissing(call);
            if (blocked != null)
                return blocked;

            if (call!.CallStatus != NvwPatientCallStatus.Claimed || call.ClaimedByUserId == userId)
                return NvwResult<NvwConsoleCallResponse>.Conflict(NvwErrorCodes.CallTakeOverNotAllowed, NvwMessages.CallTakeOverNotAllowed);

            if (request == null || call.Version != request.Version)
                return VersionMismatch();

            var now = DateTime.UtcNow;
            var previousClaimer = call.ClaimedByUserId;
            call.ClaimedByUserId = userId;
            call.ClaimedAt = now;
            Touch(call, userId, now);
            AddNurseEvent(call.Id, NvwPatientCallEventType.TakenOver, userId, previousClaimer, null, null, now);

            return await SaveConsoleAsync(call, userId, NvwPatientCallEventType.TakenOver, cancellationToken);
        }

        public async Task<NvwResult<NvwConsoleCallResponse>> ReleaseAsync(
            Guid callId,
            NvwConsoleCallVersionRequest? request,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var call = await FindCallAsync(callId, cancellationToken);
            var blocked = RejectFinishedOrMissing(call);
            if (blocked != null)
                return blocked;

            if (call!.CallStatus != NvwPatientCallStatus.Claimed)
                return NvwResult<NvwConsoleCallResponse>.Conflict(NvwErrorCodes.CallTakeOverNotAllowed, NvwMessages.CallTakeOverNotAllowed);

            if (call.ClaimedByUserId != userId)
                return NvwResult<NvwConsoleCallResponse>.Forbidden(NvwErrorCodes.CallReleaseNotOwner, NvwMessages.CallReleaseNotOwner);

            if (request == null || call.Version != request.Version)
                return VersionMismatch();

            var now = DateTime.UtcNow;
            call.CallStatus = NvwPatientCallStatus.Waiting;
            call.ClaimedByUserId = null;
            call.ClaimedAt = null;
            Touch(call, userId, now);
            AddNurseEvent(call.Id, NvwPatientCallEventType.Released, userId, userId, null, null, now);

            return await SaveConsoleAsync(call, userId, NvwPatientCallEventType.Released, cancellationToken);
        }

        public async Task<NvwResult<NvwConsoleCallResponse>> CloseFromConsoleAsync(
            Guid callId,
            NvwConsoleCloseRequest? request,
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (request?.Reason is not (NvwPatientCallCloseReason.TabletUnavailable
                or NvwPatientCallCloseReason.HandledNotConfirmedAtBed
                or NvwPatientCallCloseReason.Other))
                return NvwResult<NvwConsoleCallResponse>.Invalid(NvwErrorCodes.CallCloseReasonInvalid, NvwMessages.CallCloseReasonRequired);

            var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            if ((request.Reason == NvwPatientCallCloseReason.Other && (note == null || note.Length < MinCloseNoteLength)) ||
                (note != null && note.Length > MaxCloseNoteLength))
                return NvwResult<NvwConsoleCallResponse>.Invalid(NvwErrorCodes.CallCloseReasonInvalid, NvwMessages.CallCloseNoteRequired);

            var call = await FindCallAsync(callId, cancellationToken);
            var blocked = RejectFinishedOrMissing(call);
            if (blocked != null)
                return blocked;

            if (call!.Version != request.Version)
                return VersionMismatch();

            var now = DateTime.UtcNow;
            call.CallStatus = NvwPatientCallStatus.Handled;
            call.IsBedsideConfirmed = false;
            call.CloseReason = request.Reason;
            call.CloseNote = note;
            call.ClosedAt = now;
            call.ClosedByUserId = userId;
            Touch(call, userId, now);
            AddNurseEvent(call.Id, NvwPatientCallEventType.ClosedFromConsole, userId, null, request.Reason, note, now);

            return await SaveConsoleAsync(call, userId, NvwPatientCallEventType.ClosedFromConsole, cancellationToken);
        }

        private Task<NvwPatientCall?> FindCallAsync(Guid callId, CancellationToken cancellationToken) =>
            _dbContext.NvwPatientCalls.FirstOrDefaultAsync(x => x.Id == callId && !x.IsDelete, cancellationToken);

        private static NvwResult<NvwConsoleCallResponse>? RejectFinishedOrMissing(NvwPatientCall? call)
        {
            if (call == null)
                return NvwResult<NvwConsoleCallResponse>.NotFound(NvwMessages.CallNotFound);

            if (!NvwPatientCallQueryService.IsActiveStatus(call.CallStatus))
                return NvwResult<NvwConsoleCallResponse>.Conflict(NvwErrorCodes.CallFinished, NvwMessages.CallFinished);

            return null;
        }

        private async Task<string> UserNameAsync(Guid? userId, CancellationToken cancellationToken)
        {
            if (!userId.HasValue)
                return "perawat lain";

            var user = await _dbContext.Users.AsNoTracking()
                .Where(x => x.Id == userId.Value)
                .Select(x => new { x.UserName, x.DisplayName })
                .FirstOrDefaultAsync(cancellationToken);

            return (string.IsNullOrWhiteSpace(user?.DisplayName) ? user?.UserName : user.DisplayName) ?? "perawat lain";
        }

        private void AddNurseEvent(
            Guid callId,
            NvwPatientCallEventType eventType,
            Guid userId,
            Guid? previousClaimerUserId,
            NvwPatientCallCloseReason? closeReason,
            string? note,
            DateTime now)
        {
            _dbContext.NvwPatientCallEvents.Add(new NvwPatientCallEvent
            {
                PatientCallId = callId,
                EventType = eventType,
                ActorKind = NvwPatientCallActorKind.Nurse,
                ActorUserId = userId,
                PreviousClaimerUserId = previousClaimerUserId,
                CloseReason = closeReason,
                Note = note,
                OccurredAt = now,
                CreateDateTime = now,
                CreateBy = userId
            });
        }

        private async Task<NvwResult<NvwConsoleCallResponse>> SaveConsoleAsync(
            NvwPatientCall call,
            Guid userId,
            NvwPatientCallEventType eventType,
            CancellationToken cancellationToken)
        {
            if (await SaveAsync(cancellationToken) != SaveOutcome.Saved)
            {
                _dbContext.ChangeTracker.Clear();
                return VersionMismatch();
            }

            await _realtime.NotifyPatientCallChangedAsync(call, eventType, CancellationToken.None);

            return NvwResult<NvwConsoleCallResponse>.Ok(await _queries.MapConsoleCallAsync(call, userId, cancellationToken));
        }

        private static NvwResult<NvwConsoleCallResponse> VersionMismatch() =>
            NvwResult<NvwConsoleCallResponse>.Conflict(NvwErrorCodes.CallVersionMismatch, NvwMessages.CallVersionMismatch);

        private async Task<NvwResult<NvwTabletCallOutcome>?> ReplayAsync(
            Guid key,
            TabletRef tablet,
            CancellationToken cancellationToken)
        {
            var callId = await _dbContext.NvwPatientCallEvents.AsNoTracking()
                .Where(x => x.IdempotencyKey == key)
                .Select(x => (Guid?)x.PatientCallId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!callId.HasValue)
                return null;

            var call = await _dbContext.NvwPatientCalls.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == callId.Value, cancellationToken);

            if (call == null || call.BedId != tablet.BedId)
                return NvwResult<NvwTabletCallOutcome>.Invalid(NvwErrorCodes.IdempotencyKeyInvalid, NvwMessages.IdempotencyKeyInvalid);

            var bed = await LoadBedAsync(tablet.BedId, cancellationToken);
            return NvwResult<NvwTabletCallOutcome>.Ok(await OutcomeAsync(call, tablet, bed.BedCode, null, cancellationToken));
        }

        private Task<NvwPatientCall?> FindActiveCallAsync(Guid bedId, CancellationToken cancellationToken) =>
            _dbContext.NvwPatientCalls
                .Where(x =>
                    x.BedId == bedId &&
                    !x.IsDelete &&
                    (x.CallStatus == NvwPatientCallStatus.Waiting || x.CallStatus == NvwPatientCallStatus.Claimed))
                .OrderByDescending(x => x.FirstPressedAt)
                .FirstOrDefaultAsync(cancellationToken);

        private async Task<BedRef> LoadBedAsync(Guid bedId, CancellationToken cancellationToken)
        {
            var bed = await _dbContext.MstBeds.AsNoTracking()
                .Where(x => x.Id == bedId)
                .Select(x => new
                {
                    x.BedCode,
                    FloorCode = x.Room != null ? x.Room.FloorCode : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new BedRef(
                bed?.BedCode ?? string.Empty,
                NvwDisplayDeviceService.NormalizeFloorCode(bed?.FloorCode));
        }

        private void AddEvent(
            Guid callId,
            NvwPatientCallEventType eventType,
            Guid userId,
            Guid key,
            DateTime now,
            NvwPatientCallCloseReason? closeReason)
        {
            _dbContext.NvwPatientCallEvents.Add(new NvwPatientCallEvent
            {
                PatientCallId = callId,
                EventType = eventType,
                ActorKind = NvwPatientCallActorKind.BedTablet,
                ActorUserId = userId == Guid.Empty ? null : userId,
                CloseReason = closeReason,
                OccurredAt = now,
                IdempotencyKey = key,
                CreateDateTime = now,
                CreateBy = userId
            });
        }

        private async Task<NvwTabletCallOutcome> OutcomeAsync(
            NvwPatientCall call,
            TabletRef tablet,
            string bedCode,
            NvwPatientCallEventType? eventType,
            CancellationToken cancellationToken) =>
            new()
            {
                Call = await _queries.MapTabletCallAsync(call, cancellationToken),
                DeviceCode = tablet.DeviceCode,
                BedCode = bedCode,
                EventType = eventType
            };

        private async Task<SaveOutcome> SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return SaveOutcome.Saved;
            }
            catch (DbUpdateConcurrencyException)
            {
                return SaveOutcome.VersionConflict;
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception, IdempotencyKeyIndexName))
            {
                return SaveOutcome.DuplicateKey;
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception, ActiveCallIndexName))
            {
                return SaveOutcome.ActiveCallTaken;
            }
        }

        private static void Touch(NvwPatientCall call, Guid userId, DateTime now)
        {
            call.UpdateDateTime = now;
            call.UpdateBy = userId;
            call.Version++;
        }

        private static bool TryParseKey(string? value, out Guid key) =>
            Guid.TryParse(value?.Trim(), out key) && key != Guid.Empty;

        private static bool IsUniqueViolation(DbUpdateException exception, string indexName) =>
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres &&
            string.Equals(postgres.ConstraintName, indexName, StringComparison.Ordinal);

        private static NvwResult<NvwTabletCallOutcome> TabletUnavailable(NvwResult<NvwBedTablet> resolved) =>
            NvwResult<NvwTabletCallOutcome>.Forbidden(
                resolved.Code ?? NvwErrorCodes.BedTabletUnavailable,
                resolved.Message ?? NvwMessages.BedTabletUnavailable);

        private sealed record TabletRef(Guid Id, Guid BedId, string DeviceCode);

        private sealed record BedRef(string BedCode, string FloorCode);

        private enum SaveOutcome
        {
            Saved = 1,
            VersionConflict = 2,
            DuplicateKey = 3,
            ActiveCallTaken = 4
        }
    }
}
