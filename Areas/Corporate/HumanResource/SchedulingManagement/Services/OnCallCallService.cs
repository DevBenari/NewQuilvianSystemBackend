using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    /// <summary>
    /// Catatan panggilan siaga (HRD-DEC-198, 204, 205; state 35.9.5). Petugas unit pemanggil mencatat panggilan telepon
    /// beserta jawabannya; status penugasan siaga (TrxOnCallAssignment.AssignmentStatus) TIDAK PERNAH diubah di sini, sehingga
    /// daftar dokter siaga aktif untuk Laboratorium (INT-HR-39) tetap. Ringkasan lama penugasan (IsCalledIn, CallInTime,
    /// ArrivalTime, ResponseMinutes, IsSlaMet) diisi dari panggilan Diterima pertama (HRD-ASM-75).
    /// </summary>
    public class OnCallCallService
    {
        public const string NoBackupMessage = "Hubungi Kepala Unit pemilik jadwal siaga";
        private const string NotOnCallMessage = "Pegawai ini tidak sedang siaga pada penugasan tersebut";
        private const string ConflictMessage = "Catatan panggilan ini baru saja diubah petugas lain. Muat ulang lalu ulangi.";
        private static readonly TimeSpan WibOffset = TimeSpan.FromHours(7);

        private readonly ApplicationDbContext _context;
        private readonly ILogger<OnCallCallService> _logger;

        public OnCallCallService(ApplicationDbContext context, ILogger<OnCallCallService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SchedulingRequestServiceResult<List<OnCallCallResponse>>> GetCallsAsync(Guid assignmentId, CancellationToken ct = default)
        {
            if (!await _context.TrxOnCallAssignments.AnyAsync(x => x.Id == assignmentId && !x.IsDelete, ct))
            {
                return SchedulingRequestServiceResult<List<OnCallCallResponse>>.Fail(StatusCodes.Status404NotFound, "Penugasan siaga tidak ditemukan.");
            }

            var calls = await _context.HrdOnCallCalls.AsNoTracking()
                .Where(x => x.OnCallAssignmentId == assignmentId && !x.IsDelete)
                .OrderBy(x => x.CalledAt)
                .ToListAsync(ct);
            return SchedulingRequestServiceResult<List<OnCallCallResponse>>.Ok(await MapAsync(calls, ct), "Catatan panggilan berhasil diambil.");
        }

        public async Task<SchedulingRequestServiceResult<OnCallCallResponse>> CreateCallAsync(
            Guid assignmentId, CreateOnCallCallRequest req, Guid actorUserId, CancellationToken ct = default)
        {
            var reason = req.CallReason?.Trim() ?? string.Empty;
            if (reason.Length == 0) return Fail(StatusCodes.Status400BadRequest, "Alasan panggilan wajib diisi.");
            if (reason.Length > 500) return Fail(StatusCodes.Status400BadRequest, "Alasan panggilan paling panjang 500 karakter.");
            if (req.CalledAt == default) return Fail(StatusCodes.Status400BadRequest, "Jam panggil wajib diisi.");
            if (!await _context.Set<MstOrganizationUnit>().AnyAsync(x => x.Id == req.CallingOrganizationUnitId && !x.IsDelete, ct))
            {
                return Fail(StatusCodes.Status400BadRequest, "Unit pemanggil tidak ditemukan.");
            }

            var assignment = await _context.TrxOnCallAssignments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == assignmentId && !x.IsDelete, ct);
            if (assignment == null) return Fail(StatusCodes.Status404NotFound, "Penugasan siaga tidak ditemukan.");

            // VR-PK-09: penugasan Active dan yang dipanggil = siaga utama (urutan 0) atau cadangan penugasan ini.
            int? sequence = null;
            if (assignment.IsActive && assignment.AssignmentStatus == "Active")
            {
                sequence = req.CalledWorkforceProfileId == assignment.WorkforceProfileId
                    ? 0
                    : await _context.HrdOnCallBackups.AsNoTracking()
                        .Where(x => x.OnCallAssignmentId == assignmentId && !x.IsDelete && x.IsActive && x.WorkforceProfileId == req.CalledWorkforceProfileId)
                        .Select(x => (int?)x.BackupSequence)
                        .FirstOrDefaultAsync(ct);
            }

            if (sequence == null) return Fail(StatusCodes.Status422UnprocessableEntity, NotOnCallMessage);

            var call = new HrdOnCallCall
            {
                Id = Guid.NewGuid(),
                OnCallAssignmentId = assignmentId,
                CalledWorkforceProfileId = req.CalledWorkforceProfileId,
                BackupSequence = sequence.Value,
                CallingOrganizationUnitId = req.CallingOrganizationUnitId,
                CallReason = reason,
                CallStatus = SchedulingRequestValueConstants.OnCallCallStatus.Sent,
                CalledAt = Utc(req.CalledAt),
                RecordedByUserId = actorUserId,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _context.HrdOnCallCalls.Add(call);
            await _context.SaveChangesAsync(ct);
            return await Ok(call, "Panggilan siaga dicatat (Dikirim).", ct);
        }

        public Task<SchedulingRequestServiceResult<OnCallCallResponse>> AcceptAsync(
            Guid assignmentId, Guid callId, AnswerOnCallCallRequest req, Guid actorUserId, CancellationToken ct = default) =>
            TransitionAsync(assignmentId, callId, req.RowVersion, SchedulingRequestValueConstants.OnCallCallStatus.Sent, actorUserId, ct, async (call, timeout) =>
            {
                var answeredAt = Utc(req.AnsweredAt);
                if (req.AnsweredAt == default || answeredAt < call.CalledAt) return (StatusCodes.Status400BadRequest, "Jam jawab tidak boleh kosong atau lebih awal dari jam panggil.");
                // HRD-DEC-198: Dikirim → Diterima hanya bila belum lewat batas jawab.
                if (answeredAt > call.CalledAt.AddMinutes(timeout))
                {
                    return (StatusCodes.Status422UnprocessableEntity, $"Batas jawab {timeout} menit sudah lewat sejak panggilan {Wib(call.CalledAt)}; tandai kedaluwarsa lalu panggil cadangan.");
                }

                call.CallStatus = SchedulingRequestValueConstants.OnCallCallStatus.Accepted;
                call.AnsweredAt = answeredAt;
                call.AnswerRecordedByUserId = actorUserId;
                await FillSummaryOnAcceptAsync(call, ct);
                return null;
            }, "Jawaban bersedia dicatat (Diterima).");

        public Task<SchedulingRequestServiceResult<OnCallCallResponse>> DeclineAsync(
            Guid assignmentId, Guid callId, DeclineOnCallCallRequest req, Guid actorUserId, CancellationToken ct = default) =>
            TransitionAsync(assignmentId, callId, req.RowVersion, SchedulingRequestValueConstants.OnCallCallStatus.Sent, actorUserId, ct, (call, _) =>
            {
                var reason = req.DeclineReason?.Trim() ?? string.Empty;
                if (reason.Length == 0) return Task.FromResult<(int, string)?>((StatusCodes.Status400BadRequest, "Alasan penolakan wajib diisi"));   // VR-PK-11
                if (reason.Length > 500) return Task.FromResult<(int, string)?>((StatusCodes.Status400BadRequest, "Alasan penolakan paling panjang 500 karakter."));
                var answeredAt = Utc(req.AnsweredAt);
                if (req.AnsweredAt == default || answeredAt < call.CalledAt) return Task.FromResult<(int, string)?>((StatusCodes.Status400BadRequest, "Jam jawab tidak boleh kosong atau lebih awal dari jam panggil."));

                call.CallStatus = SchedulingRequestValueConstants.OnCallCallStatus.Declined;
                call.AnsweredAt = answeredAt;
                call.AnswerRecordedByUserId = actorUserId;
                call.DeclineReason = reason;
                return Task.FromResult<(int, string)?>(null);
            }, "Jawaban tidak bisa dicatat (Ditolak).", withNextBackup: true);

        public Task<SchedulingRequestServiceResult<OnCallCallResponse>> ExpireAsync(
            Guid assignmentId, Guid callId, ExpireOnCallCallRequest req, Guid actorUserId, DateTime utcNow, CancellationToken ct = default) =>
            TransitionAsync(assignmentId, callId, req.RowVersion, SchedulingRequestValueConstants.OnCallCallStatus.Sent, actorUserId, ct, (call, timeout) =>
            {
                // VR-PK-10: hanya sesudah batas jawab jenis siaga lewat sejak jam panggil.
                if (utcNow < call.CalledAt.AddMinutes(timeout))
                {
                    return Task.FromResult<(int, string)?>((StatusCodes.Status422UnprocessableEntity, $"Batas jawab {timeout} menit belum lewat sejak panggilan {Wib(call.CalledAt)}"));
                }

                call.CallStatus = SchedulingRequestValueConstants.OnCallCallStatus.Expired;
                call.ExpiredAt = utcNow;
                call.ExpiredByUserId = actorUserId;
                return Task.FromResult<(int, string)?>(null);
            }, "Panggilan ditandai kedaluwarsa.", withNextBackup: true);

        public Task<SchedulingRequestServiceResult<OnCallCallResponse>> CompleteAsync(
            Guid assignmentId, Guid callId, CompleteOnCallCallRequest req, Guid actorUserId, DateTime utcNow, CancellationToken ct = default) =>
            TransitionAsync(assignmentId, callId, req.RowVersion, SchedulingRequestValueConstants.OnCallCallStatus.Accepted, actorUserId, ct, async (call, _) =>
            {
                var arrivedAt = Utc(req.ArrivedAt);
                if (req.ArrivedAt == default || arrivedAt < call.CalledAt) return (StatusCodes.Status400BadRequest, "Jam tiba tidak boleh lebih awal dari jam panggil");   // VR-PK-12

                call.CallStatus = SchedulingRequestValueConstants.OnCallCallStatus.Completed;
                call.ArrivedAt = arrivedAt;
                call.CompletedAt = utcNow;
                call.CompletedByUserId = actorUserId;
                call.ResponseMinutes = (int)Math.Floor((arrivedAt - call.CalledAt).TotalMinutes);
                await FillSummaryOnCompleteAsync(call, ct);
                return null;
            }, "Kedatangan petugas siaga dicatat (Selesai).");

        /// <summary>Cadangan berikutnya (urutan terkecil) yang belum pernah dipanggil pada penugasan ini (HRD-DEC-205 butir 4).</summary>
        public async Task<OnCallNextBackupResponse?> GetNextBackupAsync(Guid assignmentId, CancellationToken ct = default)
        {
            var called = await _context.HrdOnCallCalls.AsNoTracking()
                .Where(x => x.OnCallAssignmentId == assignmentId && !x.IsDelete)
                .Select(x => x.CalledWorkforceProfileId)
                .ToListAsync(ct);
            var next = await _context.HrdOnCallBackups.AsNoTracking()
                .Where(x => x.OnCallAssignmentId == assignmentId && !x.IsDelete && x.IsActive && !called.Contains(x.WorkforceProfileId))
                .OrderBy(x => x.BackupSequence)
                .Select(x => new { x.BackupSequence, x.WorkforceProfileId })
                .FirstOrDefaultAsync(ct);
            if (next == null) return null;

            var profile = await _context.Set<MstWorkforceProfile>().AsNoTracking()
                .Where(x => x.Id == next.WorkforceProfileId)
                .Select(x => new { x.DisplayName, x.PhoneNumber })
                .FirstOrDefaultAsync(ct);
            return new OnCallNextBackupResponse
            {
                BackupSequence = next.BackupSequence,
                WorkforceProfileId = next.WorkforceProfileId,
                DisplayName = profile?.DisplayName,
                PhoneNumber = profile?.PhoneNumber
            };
        }

        private async Task<SchedulingRequestServiceResult<OnCallCallResponse>> TransitionAsync(
            Guid assignmentId,
            Guid callId,
            Guid rowVersion,
            string requiredStatus,
            Guid actorUserId,
            CancellationToken ct,
            Func<HrdOnCallCall, int, Task<(int Status, string Message)?>> apply,
            string successMessage,
            bool withNextBackup = false)
        {
            var call = await _context.HrdOnCallCalls.FirstOrDefaultAsync(x => x.Id == callId && x.OnCallAssignmentId == assignmentId && !x.IsDelete, ct);
            if (call == null) return Fail(StatusCodes.Status404NotFound, "Catatan panggilan tidak ditemukan.");
            if (call.RowVersion != rowVersion) return Fail(StatusCodes.Status409Conflict, ConflictMessage);
            if (call.CallStatus != requiredStatus)
            {
                return Fail(StatusCodes.Status409Conflict, $"Catatan panggilan berstatus {call.CallStatus}; tindakan ini hanya untuk status {requiredStatus}.");
            }

            var timeout = await _context.TrxOnCallAssignments.AsNoTracking()
                .Where(x => x.Id == assignmentId)
                .Select(x => x.OnCallType != null ? x.OnCallType.AnswerTimeoutMinutes : 0)
                .FirstOrDefaultAsync(ct);
            if (timeout <= 0)
            {
                return Fail(StatusCodes.Status422UnprocessableEntity, "Batas jawab jenis siaga belum ditetapkan pada master jenis siaga.");
            }

            var error = await apply(call, timeout);
            if (error != null)
            {
                _context.ChangeTracker.Clear();
                return Fail(error.Value.Status, error.Value.Message);
            }

            call.RowVersion = Guid.NewGuid();
            call.UpdateDateTime = DateTime.UtcNow;
            call.UpdateBy = actorUserId;
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Dua petugas menyimpan catatan yang sama hampir bersamaan: yang kalah ditolak, tidak ada yang tertimpa.
                _context.ChangeTracker.Clear();
                return Fail(StatusCodes.Status409Conflict, ConflictMessage);
            }

            var result = await Ok(call, successMessage, ct);
            if (withNextBackup && result.Data != null)
            {
                result.Data.NextBackup = await GetNextBackupAsync(assignmentId, ct);
                result.Data.NoBackupMessage = result.Data.NextBackup == null ? NoBackupMessage : null;
            }

            return result;
        }

        /// <summary>HRD-ASM-75: panggilan Diterima pertama mengisi ringkasan penugasan. Status penugasan tidak diubah.</summary>
        private async Task FillSummaryOnAcceptAsync(HrdOnCallCall call, CancellationToken ct)
        {
            if (await HasEarlierAcceptedCallAsync(call, ct)) return;
            var assignment = await _context.TrxOnCallAssignments.FirstAsync(x => x.Id == call.OnCallAssignmentId, ct);
            assignment.IsCalledIn = true;
            assignment.CallInTime = call.CalledAt;
            assignment.CalledByUnitId = call.CallingOrganizationUnitId;
            assignment.ArrivalTime = null;
            assignment.ResponseMinutes = null;
            assignment.IsSlaMet = null;
        }

        private async Task FillSummaryOnCompleteAsync(HrdOnCallCall call, CancellationToken ct)
        {
            if (await HasEarlierAcceptedCallAsync(call, ct)) return;
            var assignment = await _context.TrxOnCallAssignments.Include(x => x.OnCallType).FirstAsync(x => x.Id == call.OnCallAssignmentId, ct);
            var target = assignment.ExpectedResponseMinutes is > 0
                ? assignment.ExpectedResponseMinutes.Value
                : assignment.OnCallType?.ResponseTimeMinutes ?? 0;
            assignment.IsCalledIn = true;
            assignment.CallInTime = call.CalledAt;
            assignment.CalledByUnitId = call.CallingOrganizationUnitId;
            assignment.ArrivalTime = call.ArrivedAt;
            assignment.ResponseMinutes = call.ResponseMinutes;
            assignment.IsSlaMet = target > 0 ? call.ResponseMinutes <= target : null;
        }

        private Task<bool> HasEarlierAcceptedCallAsync(HrdOnCallCall call, CancellationToken ct)
        {
            var accepted = SchedulingRequestValueConstants.OnCallCallStatus.Accepted;
            var completed = SchedulingRequestValueConstants.OnCallCallStatus.Completed;
            return _context.HrdOnCallCalls.AsNoTracking().AnyAsync(x =>
                x.OnCallAssignmentId == call.OnCallAssignmentId && x.Id != call.Id && !x.IsDelete &&
                (x.CallStatus == accepted || x.CallStatus == completed) &&
                (x.CalledAt < call.CalledAt || (x.CalledAt == call.CalledAt && x.CreateDateTime < call.CreateDateTime)), ct);
        }

        private async Task<SchedulingRequestServiceResult<OnCallCallResponse>> Ok(HrdOnCallCall call, string message, CancellationToken ct)
        {
            var mapped = (await MapAsync(new List<HrdOnCallCall> { call }, ct)).Single();
            return SchedulingRequestServiceResult<OnCallCallResponse>.Ok(mapped, message);
        }

        private static SchedulingRequestServiceResult<OnCallCallResponse> Fail(int status, string message) =>
            SchedulingRequestServiceResult<OnCallCallResponse>.Fail(status, message);

        private async Task<List<OnCallCallResponse>> MapAsync(List<HrdOnCallCall> calls, CancellationToken ct)
        {
            var profileIds = calls.Select(x => x.CalledWorkforceProfileId).Distinct().ToList();
            var unitIds = calls.Select(x => x.CallingOrganizationUnitId).Distinct().ToList();
            var userIds = calls.Select(x => x.RecordedByUserId).Concat(calls.Where(x => x.AnswerRecordedByUserId.HasValue).Select(x => x.AnswerRecordedByUserId!.Value)).Distinct().ToList();
            var profiles = await _context.Set<MstWorkforceProfile>().AsNoTracking().Where(x => profileIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName, ct);
            var units = await _context.Set<MstOrganizationUnit>().AsNoTracking().Where(x => unitIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.UnitName, ct);
            var users = await _context.Users.AsNoTracking().Where(x => userIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

            return calls.Select(x => new OnCallCallResponse
            {
                Id = x.Id,
                OnCallAssignmentId = x.OnCallAssignmentId,
                CalledWorkforceProfileId = x.CalledWorkforceProfileId,
                CalledDisplayName = profiles.GetValueOrDefault(x.CalledWorkforceProfileId),
                BackupSequence = x.BackupSequence,
                CallingOrganizationUnitId = x.CallingOrganizationUnitId,
                CallingOrganizationUnitName = units.GetValueOrDefault(x.CallingOrganizationUnitId),
                CallReason = x.CallReason,
                CallStatus = x.CallStatus,
                CallStatusLabel = StatusLabel(x.CallStatus),
                CalledAt = x.CalledAt,
                RecordedByUserId = x.RecordedByUserId,
                RecordedByName = users.GetValueOrDefault(x.RecordedByUserId),
                AnsweredAt = x.AnsweredAt,
                AnswerRecordedByUserId = x.AnswerRecordedByUserId,
                AnswerRecordedByName = x.AnswerRecordedByUserId.HasValue ? users.GetValueOrDefault(x.AnswerRecordedByUserId.Value) : null,
                DeclineReason = x.DeclineReason,
                ExpiredAt = x.ExpiredAt,
                ExpiredByUserId = x.ExpiredByUserId,
                ArrivedAt = x.ArrivedAt,
                CompletedAt = x.CompletedAt,
                CompletedByUserId = x.CompletedByUserId,
                ResponseMinutes = x.ResponseMinutes,
                RowVersion = x.RowVersion
            }).ToList();
        }

        private static string StatusLabel(string status) => status switch
        {
            "Sent" => "Dikirim",
            "Accepted" => "Diterima",
            "Declined" => "Ditolak",
            "Expired" => "Kedaluwarsa",
            "Completed" => "Selesai",
            _ => status
        };

        private static DateTime Utc(DateTime value) =>
            value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        private static string Wib(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).Add(WibOffset).ToString("HH.mm");
    }
}
