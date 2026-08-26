using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Laporan operasional modul operasi (BE-OPR-010, OPS-REQ-011, OPS-DEC-024).
/// Seluruh query hanya membaca, memakai `AsNoTracking` dan paging, serta tidak menulis
/// custom audit log karena tidak mengubah data.
/// </summary>
public sealed class OperatingRoomReportService(ApplicationDbContext dbContext)
{
    public async Task<PagedResult<OprOperationReportRow>> GetOperationsAsync(OprReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.OprCases.AsNoTracking().Where(x => !x.IsDelete);
        if (request.From.HasValue) query = query.Where(x => x.RequestedAt >= request.From.Value.ToUniversalTime());
        if (request.To.HasValue) query = query.Where(x => x.RequestedAt <= request.To.Value.ToUniversalTime());
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.CaseType.HasValue) query = query.Where(x => x.CaseType == request.CaseType);
        if (request.Priority.HasValue) query = query.Where(x => x.Priority == request.Priority);
        if (request.PrimarySurgeonId.HasValue) query = query.Where(x => x.PrimarySurgeonId == request.PrimarySurgeonId);
        if (request.RoomId.HasValue)
            query = query.Where(x => x.Schedules.Any(s => s.IsCurrent && !s.IsDelete && s.RoomId == request.RoomId));
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x => x.CaseNumber.ToLower().Contains(search) ||
                (x.Patient != null && x.Patient.FullName.ToLower().Contains(search)));
        }

        var totalData = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.RequestedAt).ThenByDescending(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new
            {
                x.Id, x.CaseNumber, x.CaseType, x.Priority, x.Status, x.Outcome, x.EstimatedMinutes,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                SurgeonName = x.PrimarySurgeon != null ? x.PrimarySurgeon.FullName : string.Empty,
                ProcedureName = x.Procedures.Where(p => p.IsPrimary && !p.IsDelete)
                    .Select(p => p.PatientProcedure != null ? p.PatientProcedure.ProcedureNameSnapshot : string.Empty)
                    .FirstOrDefault(),
                Schedule = x.Schedules.Where(s => s.IsCurrent && !s.IsDelete)
                    .Select(s => new { s.StartAt, s.EndAt, RoomName = s.Room != null ? s.Room.RoomName : string.Empty })
                    .FirstOrDefault(),
                Execution = dbContext.OprExecutionRecords.Where(r => r.OprCaseId == x.Id && !r.IsDelete)
                    .Select(r => new { r.StartedAt, r.FinishedAt }).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OprOperationReportRow>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
            Items = [.. items.Select(x => new OprOperationReportRow
            {
                OprCaseId = x.Id, CaseNumber = x.CaseNumber, PatientName = x.PatientName,
                PrimaryProcedureName = x.ProcedureName ?? string.Empty, PrimarySurgeonName = x.SurgeonName,
                CaseType = x.CaseType, Priority = x.Priority, Status = x.Status, Outcome = x.Outcome,
                RoomName = x.Schedule?.RoomName ?? string.Empty,
                ScheduledStartAt = x.Schedule?.StartAt, ScheduledEndAt = x.Schedule?.EndAt,
                StartedAt = x.Execution?.StartedAt, FinishedAt = x.Execution?.FinishedAt,
                ActualDurationMinutes = x.Execution != null && x.Execution.FinishedAt.HasValue
                    ? (int)Math.Round((x.Execution.FinishedAt.Value - x.Execution.StartedAt).TotalMinutes)
                    : null,
                EstimatedMinutes = x.EstimatedMinutes
            })]
        };
    }

    public async Task<OprUtilizationReport> GetUtilizationAsync(OprUtilizationQuery request,
        CancellationToken cancellationToken = default)
    {
        var from = request.From.ToUniversalTime();
        var to = request.To.ToUniversalTime();
        if (to <= from) throw new ArgumentException("Rentang waktu laporan tidak valid.");

        var schedules = await dbContext.OprSchedules.AsNoTracking()
            .Where(x => x.IsCurrent && !x.IsDelete && x.StartAt < to && x.EndAt > from &&
                (!request.RoomId.HasValue || x.RoomId == request.RoomId.Value) &&
                x.OprCase != null && !x.OprCase.IsDelete)
            .Select(x => new
            {
                x.RoomId,
                RoomName = x.Room != null ? x.Room.RoomName : string.Empty,
                x.StartAt,
                x.EndAt,
                CaseStatus = x.OprCase!.Status,
                Execution = dbContext.OprExecutionRecords
                    .Where(r => r.OprCaseId == x.OprCaseId && !r.IsDelete)
                    .Select(r => new { r.StartedAt, r.FinishedAt }).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var rooms = schedules
            .GroupBy(x => new { x.RoomId, x.RoomName })
            .Select(group =>
            {
                var scheduledMinutes = (int)Math.Round(group.Sum(x => (x.EndAt - x.StartAt).TotalMinutes));
                var actualMinutes = (int)Math.Round(group
                    .Where(x => x.Execution != null && x.Execution.FinishedAt.HasValue)
                    .Sum(x => (x.Execution!.FinishedAt!.Value - x.Execution.StartedAt).TotalMinutes));
                return new OprRoomUtilizationRow
                {
                    RoomId = group.Key.RoomId,
                    RoomName = group.Key.RoomName,
                    ScheduledCases = group.Count(),
                    ScheduledMinutes = scheduledMinutes,
                    ActualMinutes = actualMinutes,
                    RealizationPercent = scheduledMinutes == 0
                        ? 0
                        : Math.Round(actualMinutes * 100m / scheduledMinutes, 2)
                };
            })
            .OrderBy(x => x.RoomName)
            .ToList();

        // Penundaan dan pembatalan dihitung dari histori supaya kasus yang sudah dijadwalkan
        // ulang tetap terhitung pernah ditunda.
        var histories = await dbContext.OprStatusHistories.AsNoTracking()
            .Where(x => !x.IsDelete && x.OccurredAt >= from && x.OccurredAt <= to &&
                (x.ToStatus == OprCaseStatus.Postponed || x.ToStatus == OprCaseStatus.Cancelled))
            .Select(x => new { x.OprCaseId, x.ToStatus })
            .ToListAsync(cancellationToken);

        return new OprUtilizationReport
        {
            From = from,
            To = to,
            Rooms = rooms,
            TotalScheduledCases = schedules.Count,
            CompletedCases = schedules.Count(x => x.CaseStatus == OprCaseStatus.Completed),
            PostponedCases = histories.Where(x => x.ToStatus == OprCaseStatus.Postponed)
                .Select(x => x.OprCaseId).Distinct().Count(),
            CancelledCases = histories.Where(x => x.ToStatus == OprCaseStatus.Cancelled)
                .Select(x => x.OprCaseId).Distinct().Count()
        };
    }

    public async Task<PagedResult<OprMaterialReportRow>> GetMaterialsAsync(OprMaterialReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.OprMaterialUsages.AsNoTracking()
            .Where(x => !x.IsDelete && x.OprCase != null && !x.OprCase.IsDelete);
        if (request.From.HasValue) query = query.Where(x => x.OccurredAt >= request.From.Value.ToUniversalTime());
        if (request.To.HasValue) query = query.Where(x => x.OccurredAt <= request.To.Value.ToUniversalTime());
        if (request.ExternalItemId.HasValue) query = query.Where(x => x.ExternalItemId == request.ExternalItemId);
        if (request.ItemType.HasValue) query = query.Where(x => x.ItemType == request.ItemType);
        if (request.Outcome.HasValue) query = query.Where(x => x.Outcome == request.Outcome);
        if (!string.IsNullOrWhiteSpace(request.BatchNumber))
            query = query.Where(x => x.BatchNumber == request.BatchNumber.Trim());
        if (!string.IsNullOrWhiteSpace(request.SerialNumber))
            query = query.Where(x => x.SerialNumber == request.SerialNumber.Trim());

        var totalData = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAt).ThenBy(x => x.Revision)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new OprMaterialReportRow
            {
                Id = x.Id, OprCaseId = x.OprCaseId,
                CaseNumber = x.OprCase != null ? x.OprCase.CaseNumber : string.Empty,
                ExternalItemId = x.ExternalItemId, ItemType = x.ItemType, Quantity = x.Quantity,
                UnitCode = x.UnitCode, Outcome = x.Outcome, BatchNumber = x.BatchNumber,
                SerialNumber = x.SerialNumber, OccurredAt = x.OccurredAt, Revision = x.Revision
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OprMaterialReportRow>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
            Items = items
        };
    }
}
