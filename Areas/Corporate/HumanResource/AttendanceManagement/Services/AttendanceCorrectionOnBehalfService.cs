using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    /// <summary>
    /// Service untuk menangani pengajuan koreksi presensi atas nama pegawai (On-Behalf) oleh HR Admin.
    /// Mengimplementasikan keputusan arsitektur [DECISION] HRD-DEC-028 dan guard penutupan periode presensi.
    /// </summary>
    public class AttendanceCorrectionOnBehalfService
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement.OnBehalf";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly AttendanceCorrectionService _correctionService;

        public AttendanceCorrectionOnBehalfService(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            AttendanceCorrectionService correctionService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _correctionService = correctionService;
        }

        public async Task<AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>> CreateOnBehalfAsync(
            CreateAttendanceCorrectionOnBehalfRequest request,
            Guid hrActorUserId,
            CancellationToken cancellationToken = default)
        {
            if (hrActorUserId == Guid.Empty)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login HR initiator tidak valid.");
            }

            if (request.WorkforceProfileId == Guid.Empty || request.AttendanceDailyId == Guid.Empty)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workforce profile pegawai dan attendance daily wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan permohonan koreksi kehadiran wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.OnBehalfReason))
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan pengajuan atas nama pegawai (on-behalf) wajib diisi (misal: kendala akses ESS, staf di lapangan, kedaruratan).");
            }

            if (request.Details == null || request.Details.Count == 0)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Minimal satu butir detail perubahan presensi wajib disertakan.");
            }

            var daily = await _dbContext.Set<HrdAttendanceDaily>()
                .Include(x => x.AttendancePolicy)
                .Include(x => x.WorkforceProfile)
                .FirstOrDefaultAsync(
                    x => x.Id == request.AttendanceDailyId && !x.IsDelete,
                    cancellationToken);

            if (daily == null)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance daily tidak ditemukan.");
            }

            if (daily.WorkforceProfileId != request.WorkforceProfileId)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Attendance daily tidak cocok dengan workforce profile yang dipilih.");
            }

            // [DECISION] HRD-DEC-028 & Acceptance Criteria 1: Guard penutupan periode kehadiran
            var isPeriodClosed = await _dbContext.Set<HrdAttendancePeriod>().AsNoTracking()
                .AnyAsync(p => p.StartDate <= daily.AttendanceDate && p.EndDate >= daily.AttendanceDate &&
                               p.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Closed &&
                               !p.IsDelete, cancellationToken);
            if (isPeriodClosed)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Periode kehadiran untuk tanggal {daily.AttendanceDate:yyyy-MM-dd} telah ditutup (Closed). Pengajuan koreksi kehadiran atas nama staf ditolak tanpa pembukaan resmi periode.");
            }

            // Guard duplicate active request
            var hasDuplicate = await _dbContext.Set<HrdAttendanceCorrectionRequest>().AsNoTracking()
                .AnyAsync(x => x.AttendanceDailyId == daily.Id &&
                               AttendanceCorrectionService.ActiveRequestStatuses.Contains(x.RequestStatus) &&
                               !x.IsDelete, cancellationToken);
            if (hasDuplicate)
            {
                return AttendanceCorrectionServiceResult<AttendanceCorrectionDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Masih terdapat permohonan attendance correction aktif untuk attendance daily tersebut.");
            }

            // Lookup HR initiator's workforce profile if linked
            var hrWorkforceProfileId = await _dbContext.Users.AsNoTracking()
                .Where(x => x.Id == hrActorUserId)
                .Select(x => x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var requestNumber = $"ACR-OB-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            var entity = new HrdAttendanceCorrectionRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = requestNumber,
                WorkforceProfileId = request.WorkforceProfileId,
                AttendanceDailyId = daily.Id,
                AttendanceDate = daily.AttendanceDate,
                CorrectionType = string.IsNullOrWhiteSpace(request.CorrectionType)
                    ? AttendanceValueConstants.CorrectionType.AttendanceTime
                    : request.CorrectionType.Trim(),
                RequestStatus = AttendanceValueConstants.CorrectionRequestStatus.Draft,
                Reason = request.Reason.Trim(),
                RequestReasonId = request.RequestReasonId,
                InitiatedByUserId = hrActorUserId,
                IsOnBehalf = true,
                OnBehalfReason = request.OnBehalfReason.Trim(),
                OnBehalfNotifiedAt = now,
                RequestedByUserId = hrActorUserId,
                RequestedByWorkforceProfileId = hrWorkforceProfileId,
                CreateBy = hrActorUserId,
                CreateDateTime = now,
                IsActive = true
            };

            var sortOrder = 1;
            foreach (var detail in request.Details)
            {
                AttendanceCorrectionFieldCatalog.TryGet(detail.FieldName, out var def);
                var fieldName = def?.FieldName ?? detail.FieldName.Trim();
                var originalVal = AttendanceCorrectionFieldCatalog.GetValue(daily, fieldName);
                entity.Details.Add(new HrdAttendanceCorrectionDetail
                {
                    Id = Guid.NewGuid(),
                    AttendanceCorrectionRequestId = entity.Id,
                    FieldName = fieldName,
                    DataType = def?.DataType ?? "String",
                    OriginalValue = originalVal,
                    RequestedValue = detail.RequestedValue?.Trim(),
                    DetailStatus = "Requested",
                    Reason = detail.Reason?.Trim() ?? request.Reason.Trim(),
                    SortOrder = sortOrder++,
                    IsActive = true,
                    CreateBy = hrActorUserId,
                    CreateDateTime = now
                });
            }

            // Link existing exceptions if provided
            if (request.ExceptionIds != null && request.ExceptionIds.Count > 0)
            {
                var exceptions = await _dbContext.Set<HrdAttendanceException>()
                    .Where(x => request.ExceptionIds.Contains(x.Id) && !x.IsDelete)
                    .ToListAsync(cancellationToken);
                foreach (var exc in exceptions)
                {
                    entity.Exceptions.Add(exc);
                }
            }

            _dbContext.Set<HrdAttendanceCorrectionRequest>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.LogInfoAsync(
                LogCategory,
                "CreateOnBehalf",
                $"Koreksi kehadiran on-behalf berhasil dibuat oleh HR user {hrActorUserId} untuk pegawai {request.WorkforceProfileId}: {entity.RequestNumber}");

            return await _correctionService.GetDetailAsync(entity.Id, null, cancellationToken);
        }
    }
}
