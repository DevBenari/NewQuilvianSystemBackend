using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class OnCallAssignmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OnCallAssignmentService> _logger;

        public OnCallAssignmentService(
            ApplicationDbContext context,
            ILogger<OnCallAssignmentService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<OnCallAssignmentResponse>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 20,
            Guid? rosterPeriodId = null,
            Guid? workforceProfileId = null,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxOnCallAssignments
                .AsNoTracking()
                .Include(o => o.WorkforceProfile)
                .Include(o => o.OnCallType)
                .Include(o => o.CalledByUnit)
                .Where(o => o.IsActive)
                .AsQueryable();

            if (rosterPeriodId.HasValue && rosterPeriodId.Value != Guid.Empty)
            {
                query = query.Where(o => o.RosterPeriodId == rosterPeriodId.Value);
            }

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(o => o.WorkforceProfileId == workforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.AssignmentStatus == status);
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(o => o.StartAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OnCallAssignmentResponse
                {
                    Id = o.Id,
                    RosterPeriodId = o.RosterPeriodId,
                    WorkforceProfileId = o.WorkforceProfileId,
                    WorkforceProfileCode = o.WorkforceProfile != null ? o.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = o.WorkforceProfile != null ? o.WorkforceProfile.DisplayName : string.Empty,
                    OnCallTypeId = o.OnCallTypeId,
                    OnCallTypeName = o.OnCallType != null ? o.OnCallType.OnCallTypeName : string.Empty,
                    StartAt = o.StartAt,
                    EndAt = o.EndAt,
                    OnCallRole = o.OnCallRole,
                    AssignmentStatus = o.AssignmentStatus,
                    ConfirmedAt = o.ConfirmedAt,
                    ActivatedAt = o.ActivatedAt,
                    CompletedAt = o.CompletedAt,
                    IsCalledIn = o.IsCalledIn,
                    CallInTime = o.CallInTime,
                    ArrivalTime = o.ArrivalTime,
                    ResponseMinutes = o.ResponseMinutes,
                    IsSlaMet = o.IsSlaMet,
                    CalledByUnitId = o.CalledByUnitId,
                    CalledByUnitName = o.CalledByUnit != null ? o.CalledByUnit.UnitName : null,
                    CallNotes = o.CallNotes,
                    Notes = o.Notes
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OnCallAssignmentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<SchedulingRequestServiceResult<OnCallAssignmentResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var o = await _context.TrxOnCallAssignments
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.OnCallType)
                .Include(x => x.CalledByUnit)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

            if (o == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan siaga (on-call) dengan ID '{id}' tidak ditemukan.");
            }

            var response = new OnCallAssignmentResponse
            {
                Id = o.Id,
                RosterPeriodId = o.RosterPeriodId,
                WorkforceProfileId = o.WorkforceProfileId,
                WorkforceProfileCode = o.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = o.WorkforceProfile?.DisplayName ?? string.Empty,
                OnCallTypeId = o.OnCallTypeId,
                OnCallTypeName = o.OnCallType?.OnCallTypeName ?? string.Empty,
                StartAt = o.StartAt,
                EndAt = o.EndAt,
                OnCallRole = o.OnCallRole,
                AssignmentStatus = o.AssignmentStatus,
                ConfirmedAt = o.ConfirmedAt,
                ActivatedAt = o.ActivatedAt,
                CompletedAt = o.CompletedAt,
                IsCalledIn = o.IsCalledIn,
                CallInTime = o.CallInTime,
                ArrivalTime = o.ArrivalTime,
                ResponseMinutes = o.ResponseMinutes,
                IsSlaMet = o.IsSlaMet,
                CalledByUnitId = o.CalledByUnitId,
                CalledByUnitName = o.CalledByUnit?.UnitName,
                CallNotes = o.CallNotes,
                Notes = o.Notes
            };

            return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Ok(
                response,
                "Detail penugasan siaga berhasil diambil.");
        }

        public async Task<SchedulingRequestServiceResult<OnCallAssignmentResponse>> CreateAsync(
            CreateOnCallAssignmentRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            // Validasi jam mulai < jam selesai (Acceptance criteria 1 BE-HRD-102)
            if (req.EndAt <= req.StartAt)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Waktu selesai penugasan siaga (EndAt) harus lebih besar dari waktu mulai (StartAt).");
            }

            var period = await _context.TrxRosterPeriods
                .FirstOrDefaultAsync(p => p.Id == req.RosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Periode roster dengan ID '{req.RosterPeriodId}' tidak ditemukan.");
            }

            if (period.IsLocked || period.RosterStatus == "Closed")
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penambahan penugasan on-call (HRD-DEC-102).");
            }

            var workforce = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.WorkforceProfileId && w.IsActive, cancellationToken);

            if (workforce == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai dengan ID '{req.WorkforceProfileId}' tidak ditemukan.");
            }

            var onCallType = await _context.MstOnCallTypes
                .FirstOrDefaultAsync(t => t.Id == req.OnCallTypeId && t.IsActive, cancellationToken);

            if (onCallType == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Tipe on-call dengan ID '{req.OnCallTypeId}' tidak ditemukan.");
            }

            int expectedMinutes = req.ExpectedResponseMinutes.HasValue && req.ExpectedResponseMinutes > 0
                ? req.ExpectedResponseMinutes.Value
                : (onCallType.ResponseTimeMinutes > 0 ? onCallType.ResponseTimeMinutes : 30);

            var assignment = new TrxOnCallAssignment
            {
                Id = Guid.NewGuid(),
                RosterPeriodId = req.RosterPeriodId,
                WorkforceProfileId = req.WorkforceProfileId,
                OnCallTypeId = req.OnCallTypeId,
                ShiftId = req.ShiftId,
                UnitId = req.UnitId ?? period.OrganizationUnitId,
                StartAt = req.StartAt,
                EndAt = req.EndAt,
                OnCallRole = req.OnCallRole.Trim(),
                AssignmentStatus = "Scheduled",
                ExpectedResponseMinutes = expectedMinutes,
                Notes = req.Notes?.Trim(),
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxOnCallAssignments.Add(assignment);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new OnCallAssignmentResponse
            {
                Id = assignment.Id,
                RosterPeriodId = assignment.RosterPeriodId,
                WorkforceProfileId = assignment.WorkforceProfileId,
                WorkforceProfileCode = workforce.ProfileCode,
                WorkforceDisplayName = workforce.DisplayName,
                OnCallTypeId = assignment.OnCallTypeId,
                OnCallTypeName = onCallType.OnCallTypeName,
                StartAt = assignment.StartAt,
                EndAt = assignment.EndAt,
                OnCallRole = assignment.OnCallRole,
                AssignmentStatus = assignment.AssignmentStatus,
                Notes = assignment.Notes
            };

            return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Ok(
                response,
                "Penugasan siaga (on-call) berhasil dijadwalkan.",
                StatusCodes.Status201Created);
        }

        public async Task<SchedulingRequestServiceResult<OnCallAssignmentResponse>> ConfirmAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var assignment = await _context.TrxOnCallAssignments
                .Include(o => o.WorkforceProfile)
                .Include(o => o.OnCallType)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive, cancellationToken);

            if (assignment == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan siaga (on-call) dengan ID '{id}' tidak ditemukan.");
            }

            assignment.AssignmentStatus = "Confirmed";
            assignment.ConfirmedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new OnCallAssignmentResponse
            {
                Id = assignment.Id,
                RosterPeriodId = assignment.RosterPeriodId,
                WorkforceProfileId = assignment.WorkforceProfileId,
                WorkforceProfileCode = assignment.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = assignment.WorkforceProfile?.DisplayName ?? string.Empty,
                OnCallTypeId = assignment.OnCallTypeId,
                OnCallTypeName = assignment.OnCallType?.OnCallTypeName ?? string.Empty,
                StartAt = assignment.StartAt,
                EndAt = assignment.EndAt,
                OnCallRole = assignment.OnCallRole,
                AssignmentStatus = assignment.AssignmentStatus,
                ConfirmedAt = assignment.ConfirmedAt,
                ActivatedAt = assignment.ActivatedAt,
                Notes = assignment.Notes
            };

            return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Ok(
                response,
                "Penugasan siaga (on-call) berhasil dikonfirmasi kesediaannya.");
        }

        /// <summary>
        /// Aktivasi status siaga menjadi 'Active' (BE-HRD-099, HRD-DEC-103, menutup bug HRD-TF-053).
        /// Status tetap 'Active' agar kontrak integrasi Laboratorium (LAB-DEC-111, INT-HR-37) tetap menemukan dokter.
        /// </summary>
        public async Task<SchedulingRequestServiceResult<OnCallAssignmentResponse>> ActivateAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var assignment = await _context.TrxOnCallAssignments
                .Include(o => o.WorkforceProfile)
                .Include(o => o.OnCallType)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive, cancellationToken);

            if (assignment == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan siaga (on-call) dengan ID '{id}' tidak ditemukan.");
            }

            // Status diselaraskan menjadi 'Active' (BUKAN 'Activated') per HRD-DEC-103 & LAB-DEC-111
            assignment.AssignmentStatus = "Active";
            assignment.ActivatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new OnCallAssignmentResponse
            {
                Id = assignment.Id,
                RosterPeriodId = assignment.RosterPeriodId,
                WorkforceProfileId = assignment.WorkforceProfileId,
                WorkforceProfileCode = assignment.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = assignment.WorkforceProfile?.DisplayName ?? string.Empty,
                OnCallTypeId = assignment.OnCallTypeId,
                OnCallTypeName = assignment.OnCallType?.OnCallTypeName ?? string.Empty,
                StartAt = assignment.StartAt,
                EndAt = assignment.EndAt,
                OnCallRole = assignment.OnCallRole,
                AssignmentStatus = assignment.AssignmentStatus,
                ConfirmedAt = assignment.ConfirmedAt,
                ActivatedAt = assignment.ActivatedAt,
                Notes = assignment.Notes
            };

            return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Ok(
                response,
                "Penugasan siaga berhasil diaktifkan dengan status 'Active' (kontrak Lab terjamin).");
        }

        public async Task<SchedulingRequestServiceResult<OnCallAssignmentResponse>> CompleteAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var assignment = await _context.TrxOnCallAssignments
                .Include(o => o.WorkforceProfile)
                .Include(o => o.OnCallType)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive, cancellationToken);

            if (assignment == null)
            {
                return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan siaga (on-call) dengan ID '{id}' tidak ditemukan.");
            }

            assignment.AssignmentStatus = "Completed";
            assignment.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new OnCallAssignmentResponse
            {
                Id = assignment.Id,
                RosterPeriodId = assignment.RosterPeriodId,
                WorkforceProfileId = assignment.WorkforceProfileId,
                WorkforceProfileCode = assignment.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = assignment.WorkforceProfile?.DisplayName ?? string.Empty,
                OnCallTypeId = assignment.OnCallTypeId,
                OnCallTypeName = assignment.OnCallType?.OnCallTypeName ?? string.Empty,
                StartAt = assignment.StartAt,
                EndAt = assignment.EndAt,
                OnCallRole = assignment.OnCallRole,
                AssignmentStatus = assignment.AssignmentStatus,
                ConfirmedAt = assignment.ConfirmedAt,
                ActivatedAt = assignment.ActivatedAt,
                CompletedAt = assignment.CompletedAt,
                Notes = assignment.Notes
            };

            return SchedulingRequestServiceResult<OnCallAssignmentResponse>.Ok(
                response,
                "Penugasan siaga berhasil diselesaikan (Completed).");
        }

        /// <summary>
        /// Pencatatan pemanggilan darurat cito dokter/staf siaga aktif & evaluasi indikator SLA respon (BE-HRD-100, HRD-DEC-103).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<OnCallCallInResponse>> RecordCallInAsync(
            Guid id,
            RecordOnCallCallInRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (req.ArrivalTime < req.CallInTime)
            {
                return SchedulingRequestServiceResult<OnCallCallInResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Waktu kedatangan fisik (ArrivalTime) tidak boleh lebih awal dari waktu panggilan cito (CallInTime).");
            }

            var assignment = await _context.TrxOnCallAssignments
                .Include(o => o.WorkforceProfile)
                .Include(o => o.OnCallType)
                .Include(o => o.CalledByUnit)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive, cancellationToken);

            if (assignment == null)
            {
                return SchedulingRequestServiceResult<OnCallCallInResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan siaga (on-call) dengan ID '{id}' tidak ditemukan.");
            }

            int responseMinutes = (int)Math.Max(0, (req.ArrivalTime - req.CallInTime).TotalMinutes);
            int expectedMinutes = assignment.ExpectedResponseMinutes.HasValue && assignment.ExpectedResponseMinutes > 0
                ? assignment.ExpectedResponseMinutes.Value
                : (assignment.OnCallType != null && assignment.OnCallType.ResponseTimeMinutes > 0
                    ? assignment.OnCallType.ResponseTimeMinutes
                    : 30);

            bool isSlaMet = responseMinutes <= expectedMinutes;
            var callAt = DateTime.SpecifyKind(req.CallInTime, req.CallInTime.Kind == DateTimeKind.Local ? DateTimeKind.Local : DateTimeKind.Utc).ToUniversalTime();
            var arriveAt = DateTime.SpecifyKind(req.ArrivalTime, req.ArrivalTime.Kind == DateTimeKind.Local ? DateTimeKind.Local : DateTimeKind.Utc).ToUniversalTime();

            // HRD-ASM-75: ringkasan diisi dari panggilan Diterima pertama. Call-in kini juga satu catatan Completed, jadi
            // ringkasan hanya ditulis bila belum ada panggilan Diterima/Selesai yang lebih awal (sama dengan OnCallCallService).
            var accepted = QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants.SchedulingRequestValueConstants.OnCallCallStatus.Accepted;
            var completed = QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants.SchedulingRequestValueConstants.OnCallCallStatus.Completed;
            var hasEarlierAccepted = await _context.HrdOnCallCalls.AsNoTracking().AnyAsync(x =>
                x.OnCallAssignmentId == assignment.Id && !x.IsDelete &&
                (x.CallStatus == accepted || x.CallStatus == completed) && x.CalledAt <= callAt, cancellationToken);
            if (!hasEarlierAccepted)
            {
                assignment.IsCalledIn = true;
                assignment.CallInTime = req.CallInTime;
                assignment.ArrivalTime = req.ArrivalTime;
                assignment.ResponseMinutes = responseMinutes;
                assignment.ExpectedResponseMinutes = expectedMinutes;
                assignment.IsSlaMet = isSlaMet;
                assignment.CalledByUnitId = req.CalledByUnitId;
                assignment.CallNotes = req.CallNotes?.Trim();
            }

            // Status penugasan tetap 'Active' saat dipanggil (HRD-DEC-103, menutup bug HRD-TF-053)
            assignment.AssignmentStatus = "Active";
            if (!assignment.ActivatedAt.HasValue)
            {
                assignment.ActivatedAt = req.CallInTime;
            }

            // BE-HRD-352 / kontrak v26: endpoint lama kini juga menulis satu catatan panggilan Completed (HRD-DEC-198),
            // dalam penyimpanan yang sama dengan ringkasan. Unit pemanggil: dari permintaan, lalu unit penugasan, lalu unit roster.
            var callingUnitId = req.CalledByUnitId ?? assignment.UnitId ?? await _context.TrxRosterPeriods
                .AsNoTracking()
                .Where(x => x.Id == assignment.RosterPeriodId)
                .Select(x => x.OrganizationUnitId)
                .FirstOrDefaultAsync(cancellationToken);
            if (callingUnitId.HasValue)
            {
                _context.HrdOnCallCalls.Add(new HrdOnCallCall
                {
                    Id = Guid.NewGuid(),
                    OnCallAssignmentId = assignment.Id,
                    CalledWorkforceProfileId = assignment.WorkforceProfileId,
                    BackupSequence = 0,
                    CallingOrganizationUnitId = callingUnitId.Value,
                    CallReason = string.IsNullOrWhiteSpace(req.CallNotes) ? "Dicatat lewat call-in" : req.CallNotes.Trim()[..Math.Min(500, req.CallNotes.Trim().Length)],
                    CallStatus = QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants.SchedulingRequestValueConstants.OnCallCallStatus.Completed,
                    CalledAt = callAt,
                    RecordedByUserId = currentUserId,
                    AnsweredAt = callAt,
                    AnswerRecordedByUserId = currentUserId,
                    ArrivedAt = arriveAt,
                    CompletedAt = DateTime.UtcNow,
                    CompletedByUserId = currentUserId,
                    ResponseMinutes = responseMinutes,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = currentUserId
                });
            }
            else
            {
                _logger.LogWarning("Call-in penugasan siaga {Id} tanpa unit pemanggil, unit penugasan, maupun unit roster; catatan panggilan tidak ditulis.", assignment.Id);
            }

            await _context.SaveChangesAsync(cancellationToken);

            string slaStatusText = isSlaMet
                ? $"Memenuhi standar SLA ({responseMinutes} mnt <= target {expectedMinutes} mnt)"
                : $"Melampaui target SLA ({responseMinutes} mnt > target {expectedMinutes} mnt)";

            var response = new OnCallCallInResponse
            {
                OnCallAssignmentId = assignment.Id,
                WorkforceProfileId = assignment.WorkforceProfileId,
                WorkforceDisplayName = assignment.WorkforceProfile?.DisplayName ?? string.Empty,
                CallInTime = req.CallInTime,
                ArrivalTime = req.ArrivalTime,
                ResponseMinutes = responseMinutes,
                ExpectedResponseMinutes = expectedMinutes,
                IsSlaMet = isSlaMet,
                CalledByUnitId = req.CalledByUnitId,
                CalledByUnitName = assignment.CalledByUnit?.UnitName,
                CallNotes = assignment.CallNotes,
                Status = assignment.AssignmentStatus,
                Message = $"Perekaman pemanggilan cito berhasil dicatat. Respon waktu: {responseMinutes} menit. Status: {slaStatusText}."
            };

            _logger.LogInformation("Perekaman Call-In sukses untuk on-call {Id}. Respon: {Mins} mnt. SLA: {IsMet}",
                assignment.Id, responseMinutes, isSlaMet);

            return SchedulingRequestServiceResult<OnCallCallInResponse>.Ok(
                response,
                response.Message);
        }

        /// <summary>Daftar cadangan berurutan penugasan siaga (HRD-DEC-205 butir 1). Nomor kontak tidak dikirim di sini.</summary>
        public async Task<SchedulingRequestServiceResult<List<OnCallBackupResponse>>> GetBackupsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!await _context.TrxOnCallAssignments.AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
            {
                return SchedulingRequestServiceResult<List<OnCallBackupResponse>>.Fail(StatusCodes.Status404NotFound, "Penugasan siaga tidak ditemukan.");
            }

            return SchedulingRequestServiceResult<List<OnCallBackupResponse>>.Ok(await ReadBackupsAsync(id, cancellationToken), "Daftar cadangan siaga berhasil diambil.");
        }

        /// <summary>
        /// Menetapkan seluruh daftar cadangan (ganti utuh). VR-PK-08: urutan ganda, pegawai ganda, atau cadangan = siaga utama
        /// ditolak 422. Daftar lama dihapus lunak lalu daftar baru disimpan dalam satu transaksi.
        /// </summary>
        public async Task<SchedulingRequestServiceResult<List<OnCallBackupResponse>>> SetBackupsAsync(
            Guid id,
            SetOnCallBackupsRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            const string duplicateMessage = "Urutan cadangan dan nama cadangan tidak boleh ganda atau sama dengan siaga utama";
            var items = req.Items ?? new List<SetOnCallBackupItemRequest>();
            var assignment = await _context.TrxOnCallAssignments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (assignment == null)
            {
                return SchedulingRequestServiceResult<List<OnCallBackupResponse>>.Fail(StatusCodes.Status404NotFound, "Penugasan siaga tidak ditemukan.");
            }

            if (assignment.AssignmentStatus is "Completed" or "Cancelled")
            {
                return SchedulingRequestServiceResult<List<OnCallBackupResponse>>.Fail(StatusCodes.Status409Conflict, $"Cadangan tidak dapat diubah pada penugasan berstatus {assignment.AssignmentStatus}.");
            }

            if (items.Any(x => x.BackupSequence < 1))
            {
                return SchedulingRequestServiceResult<List<OnCallBackupResponse>>.Fail(StatusCodes.Status400BadRequest, "Urutan cadangan dimulai dari 1.");
            }

            if (items.GroupBy(x => x.BackupSequence).Any(g => g.Count() > 1) ||
                items.GroupBy(x => x.WorkforceProfileId).Any(g => g.Count() > 1) ||
                items.Any(x => x.WorkforceProfileId == assignment.WorkforceProfileId))
            {
                return SchedulingRequestServiceResult<List<OnCallBackupResponse>>.Fail(StatusCodes.Status422UnprocessableEntity, duplicateMessage);
            }

            var ids = items.Select(x => x.WorkforceProfileId).ToList();
            var found = await _context.Set<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstWorkforceProfile>()
                .AsNoTracking()
                .CountAsync(x => ids.Contains(x.Id) && !x.IsDelete && x.IsActive, cancellationToken);
            if (found != ids.Count)
            {
                return SchedulingRequestServiceResult<List<OnCallBackupResponse>>.Fail(StatusCodes.Status400BadRequest, "Pegawai cadangan tidak ditemukan atau tidak aktif.");
            }

            var now = DateTime.UtcNow;
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var existing = await _context.HrdOnCallBackups.Where(x => x.OnCallAssignmentId == id && !x.IsDelete).ToListAsync(cancellationToken);
                foreach (var backup in existing)
                {
                    backup.IsDelete = true;
                    backup.IsActive = false;
                    backup.DeleteDateTime = now;
                    backup.DeleteBy = currentUserId;
                    backup.UpdateDateTime = now;
                    backup.UpdateBy = currentUserId;
                }

                // Hapus lunak disimpan dulu agar indeks unik tersaring IsDelete tidak bentrok dengan daftar baru.
                await _context.SaveChangesAsync(cancellationToken);
                _context.HrdOnCallBackups.AddRange(items.OrderBy(x => x.BackupSequence).Select(x => new HrdOnCallBackup
                {
                    Id = Guid.NewGuid(),
                    OnCallAssignmentId = id,
                    BackupSequence = x.BackupSequence,
                    WorkforceProfileId = x.WorkforceProfileId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = currentUserId
                }));
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _context.ChangeTracker.Clear();
                _logger.LogWarning(ex, "Daftar cadangan siaga {Id} gagal disimpan.", id);
                return SchedulingRequestServiceResult<List<OnCallBackupResponse>>.Fail(StatusCodes.Status409Conflict, "Daftar cadangan baru saja diubah petugas lain. Muat ulang lalu ulangi.");
            }

            return SchedulingRequestServiceResult<List<OnCallBackupResponse>>.Ok(await ReadBackupsAsync(id, cancellationToken), "Daftar cadangan siaga berhasil disimpan.");
        }

        private async Task<List<OnCallBackupResponse>> ReadBackupsAsync(Guid id, CancellationToken cancellationToken) =>
            await _context.HrdOnCallBackups.AsNoTracking()
                .Where(x => x.OnCallAssignmentId == id && !x.IsDelete)
                .OrderBy(x => x.BackupSequence)
                .Select(x => new OnCallBackupResponse
                {
                    Id = x.Id,
                    BackupSequence = x.BackupSequence,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null
                })
                .ToListAsync(cancellationToken);

        /// <summary>
        /// Resolver dokter siaga aktif untuk kebutuhan integrasi Laboratorium (BE-HRD-099, LAB-DEC-111, INT-HR-37).
        /// </summary>
        public async Task<List<Guid>> GetActiveOnCallDoctorIdsAsync(
            Guid? hospitalSiteId,
            DateTime effectiveAt,
            CancellationToken cancellationToken = default)
        {
            var activeDoctorIds = await _context.TrxOnCallAssignments
                .AsNoTracking()
                .Where(o => o.IsActive &&
                            o.AssignmentStatus == "Active" &&
                            o.StartAt <= effectiveAt &&
                            o.EndAt >= effectiveAt)
                .Select(o => o.WorkforceProfileId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return activeDoctorIds;
        }
    }
}
