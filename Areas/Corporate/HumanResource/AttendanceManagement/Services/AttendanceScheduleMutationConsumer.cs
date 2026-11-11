using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public class RetroactiveShiftCorrectedPayload
    {
        public Guid ShiftAssignmentId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public Guid? OldShiftId { get; set; }
        public Guid NewShiftId { get; set; }
        public Guid TriggeredByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    public class AttendanceScheduleMutationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? AttendanceDailyId { get; set; }
        public bool IsPeriodClosed { get; set; }
    }

    /// <summary>
    /// Event consumer rekalkulasi kehadiran harian akibat koreksi mundur/mutasi jadwal dinas roster (BE-HRD-112, HRD-DEC-102, INT-HR-41).
    /// </summary>
    public class AttendanceScheduleMutationConsumer
    {
        private readonly ApplicationDbContext _context;
        private readonly AttendanceProcessingService _processingService;
        private readonly ILogger<AttendanceScheduleMutationConsumer> _logger;

        public AttendanceScheduleMutationConsumer(
            ApplicationDbContext context,
            AttendanceProcessingService processingService,
            ILogger<AttendanceScheduleMutationConsumer> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AttendanceScheduleMutationResult> ConsumeRetroactiveShiftCorrectionAsync(
            RetroactiveShiftCorrectedPayload payload,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Event INT-HR-41 diterima: Menghitung ulang presensi pegawai {WorkforceProfileId} pada tanggal {ShiftDate} (Shift baru: {NewShiftId})",
                payload.WorkforceProfileId, payload.ShiftDate, payload.NewShiftId);

            // Guard HRD-DEC-028 / VR-ATT-15: Periode kehadiran tertutup
            var isPeriodClosed = await _context.Set<HrdAttendancePeriod>().AsNoTracking()
                .AnyAsync(p => p.StartDate <= payload.ShiftDate && p.EndDate >= payload.ShiftDate &&
                               p.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Closed &&
                               !p.IsDelete, cancellationToken);

            if (isPeriodClosed)
            {
                var msg = $"Periode kehadiran untuk tanggal {payload.ShiftDate:yyyy-MM-dd} telah ditutup (Closed). Rekalkulasi otomatis diabaikan.";
                _logger.LogWarning("[INT-HR-41] {Message}", msg);
                return new AttendanceScheduleMutationResult
                {
                    Success = false,
                    IsPeriodClosed = true,
                    Message = msg
                };
            }

            var daily = await _context.Set<HrdAttendanceDaily>()
                .FirstOrDefaultAsync(d => d.WorkforceProfileId == payload.WorkforceProfileId &&
                                          d.AttendanceDate == payload.ShiftDate &&
                                          !d.IsDelete, cancellationToken);

            if (daily != null)
            {
                daily.ShiftId = payload.NewShiftId;
                await _context.SaveChangesAsync(cancellationToken);

                var reprocessReq = new ReprocessAttendanceDailyRequest
                {
                    Reason = $"[INT-HR-41] Rekalkulasi otomatis akibat mutasi roster: {payload.Reason}"
                };

                var processResult = await _processingService.ReprocessDailyAsync(
                    daily.Id,
                    reprocessReq,
                    payload.TriggeredByUserId,
                    cancellationToken);

                _logger.LogInformation(
                    "[INT-HR-41] ReprocessDailyAsync selesai untuk daily {DailyId} dengan status {Status}",
                    daily.Id, processResult.StatusCode);

                return new AttendanceScheduleMutationResult
                {
                    Success = processResult.Success,
                    AttendanceDailyId = daily.Id,
                    Message = processResult.Message
                };
            }
            else
            {
                var singleReq = new ProcessAttendanceSingleRequest
                {
                    WorkforceProfileId = payload.WorkforceProfileId,
                    WorkDate = payload.ShiftDate,
                    ForceReprocess = true,
                    TriggerSource = AttendanceValueConstants.ProcessingTriggerSource.System
                };

                var singleResult = await _processingService.ProcessSingleAsync(
                    singleReq,
                    payload.TriggeredByUserId,
                    cancellationToken);

                _logger.LogInformation(
                    "[INT-HR-41] ProcessSingleAsync selesai untuk pegawai {WorkforceProfileId} pada tanggal {ShiftDate} dengan status {Status}",
                    payload.WorkforceProfileId, payload.ShiftDate, singleResult.StatusCode);

                return new AttendanceScheduleMutationResult
                {
                    Success = singleResult.Success,
                    Message = singleResult.Message
                };
            }
        }
    }
}
