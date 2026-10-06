using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Daftar serah terima pasca operasi per unit tujuan dan status, baca saja (<c>BE-RWI-177</c>,
/// API 11.5.3). Dipakai bangsal dan Daftar Pantau (<c>BE-RWI-182</c>).
/// </summary>
/// <remarks>
/// "Tertunda" = masih <c>Sent</c> dan menunggu lebih lama dari
/// <c>MstInpatientSetting.PendingSurgicalHandoverAlertMinutes</c> (bawaan 60 menit, <c>RWI-DEC-220</c>
/// butir 6). Contoh: serah terima ke ICU dikirim 08.00; pukul 09.10 baris itu <c>WaitingMinutes = 70</c>,
/// <c>IsOverdue = true</c>, dan bila Budi masih di bangsal, <c>PatientInDestinationUnit = false</c>.
/// </remarks>
public sealed class OperatingRoomHandoverQueryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly InpSettingService _settingService;
    private readonly InpPatientLocationQuery _patientLocation;

    public OperatingRoomHandoverQueryService(ApplicationDbContext dbContext, InpSettingService settingService,
        InpPatientLocationQuery patientLocation)
    {
        _dbContext = dbContext;
        _settingService = settingService;
        _patientLocation = patientLocation;
    }

    public async Task<PagedResult<HandoverQueueItemResponse>> GetPagedAsync(HandoverQueueQuery request,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var setting = await _settingService.GetEffectiveSettingAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var overdueBefore = now.AddMinutes(-setting.PendingSurgicalHandoverAlertMinutes);

        var query = _dbContext.OprHandovers.AsNoTracking()
            .Where(x => !x.IsDelete && x.OprCase != null && !x.OprCase.IsDelete);
        if (request.DestinationUnitId.HasValue)
            query = query.Where(x => x.DestinationUnitId == request.DestinationUnitId.Value);
        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);
        if (request.OverdueOnly == true)
            query = query.Where(x => x.Status == OprHandoverStatus.Sent && x.SentAt <= overdueBefore);

        var totalData = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.SentAt).ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new
            {
                x.Id, x.OprCaseId, x.DestinationUnitId, x.Status, x.SentAt, x.AcceptedAt, x.UpdateDateTime,
                CaseNumber = x.OprCase!.CaseNumber,
                x.OprCase.PatientId,
                PatientName = x.OprCase.Patient != null ? x.OprCase.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.OprCase.Patient != null ? x.OprCase.Patient.MedicalRecordNumber : null,
                DestinationUnitName = _dbContext.Set<MstServiceUnit>()
                    .Where(u => u.Id == x.DestinationUnitId).Select(u => u.ServiceUnitName).FirstOrDefault(),
                SentByName = _dbContext.Users.Where(u => u.Id == x.SentBy).Select(u => u.DisplayName).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var locations = await _patientLocation.GetCurrentLocationsAsync(
            rows.Select(x => x.PatientId).Distinct().ToList(), cancellationToken);

        return new PagedResult<HandoverQueueItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
            Items = [.. rows.Select(x =>
            {
                locations.TryGetValue(x.PatientId, out var location);
                var endedAt = x.Status == OprHandoverStatus.Sent ? now : x.AcceptedAt ?? x.UpdateDateTime ?? now;
                return new HandoverQueueItemResponse
                {
                    HandoverId = x.Id, CaseId = x.OprCaseId, CaseNumber = x.CaseNumber, PatientName = x.PatientName,
                    MedicalRecordNumber = x.MedicalRecordNumber, DestinationUnitId = x.DestinationUnitId,
                    DestinationUnitName = x.DestinationUnitName ?? string.Empty,
                    CurrentUnitName = location?.ServiceUnitName,
                    PatientInDestinationUnit = location != null && location.ServiceUnitId == x.DestinationUnitId,
                    Status = x.Status, SentByName = x.SentByName, SentAt = x.SentAt,
                    WaitingMinutes = Math.Max(0, (int)Math.Floor((endedAt - x.SentAt).TotalMinutes)),
                    IsOverdue = x.Status == OprHandoverStatus.Sent && x.SentAt <= overdueBefore
                };
            })]
        };
    }
}
