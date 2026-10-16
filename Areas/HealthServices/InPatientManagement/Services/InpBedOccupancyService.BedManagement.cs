using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public partial class InpBedOccupancyService
{
    public async Task<InpBedOccupancyOperationResult> ReserveBedAsync(ReserveBedRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        await _bedAccess.RequireEpisodeAsync(request.EpisodeId, "InpatientBedOccupancy:Create", cancellationToken);
        var meta = await _bedCoordinator.ExecuteAsync("InpatientBedOccupancy:Create", new { operation = "Reserve", request },
            [request.EpisodeId], [request.BedId], [new() { BedId = request.BedId, ExpectedVersion = request.ExpectedBedVersion }],
            async ct => { var result = await ReserveBedCoreAsync(request, actorUserId, ct); RequireSuccess(result); return ("Reservation", result.ReservationId!.Value); }, cancellationToken);
        return InpBedOccupancyOperationResult.Success("Pemesanan tersimpan.", reservationId: meta.ResultEntityId);
    }
    public async Task<InpBedOccupancyOperationResult> CancelReservationAsync(Guid reservationId, CancelReservationRequest? request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Alasan pembatalan wajib, maksimum 500 karakter.");
        var reservation = await _dbContext.Set<InpBedReservation>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == reservationId && !x.IsDelete, cancellationToken)
            ?? throw new InpBedOperationException(404, "NOT_FOUND", "Pemesanan tidak ditemukan.");
        await _bedAccess.RequireEpisodeAsync(reservation.EpisodeId, "InpatientBedOccupancy:Update", cancellationToken);
        var meta = await _bedCoordinator.ExecuteAsync("InpatientBedOccupancy:Update", new { reservationId, request },
            [reservation.EpisodeId], [reservation.BedId], [new() { BedId = reservation.BedId, ExpectedVersion = request.ExpectedBedVersion }],
            async ct => { var result = await CancelReservationCoreAsync(reservationId, request, actorUserId, ct); RequireSuccess(result); return ("Reservation", reservationId); }, cancellationToken);
        return InpBedOccupancyOperationResult.Success("Pembatalan tersimpan.", reservationId: meta.ResultEntityId);
    }
    public async Task<InpBedOccupancyOperationResult> PlacePatientAsync(PlacePatientRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        await _bedAccess.RequireEpisodeAsync(request.EpisodeId, "InpatientBedOccupancy:Create", cancellationToken);
        var meta = await _bedCoordinator.ExecuteAsync("InpatientBedOccupancy:Create", new { operation = "Place", request },
            [request.EpisodeId], [request.BedId], [new() { BedId = request.BedId, ExpectedVersion = request.ExpectedBedVersion }],
            async ct => { var result = await PlacePatientCoreAsync(request, actorUserId, ct); RequireSuccess(result); return ("Placement", result.PlacementId!.Value); }, cancellationToken);
        return InpBedOccupancyOperationResult.Success("Penempatan tersimpan.", placementId: meta.ResultEntityId);
    }
    public async Task<InpBedOccupancyOperationResult> TransferAsync(TransferPatientRequest request, Guid actorUserId, Guid? actorDoctorId, CancellationToken cancellationToken = default)
    {
        await _bedAccess.RequireEpisodeAsync(request.EpisodeId, "InpatientBedOccupancy:Transfer", cancellationToken);
        var source = await _dbContext.Set<InpBedPlacement>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.ExpectedSourcePlacementId && x.EpisodeId == request.EpisodeId, cancellationToken)
            ?? throw new InpBedOperationException(409, "STALE_PLACEMENT", "Penempatan asal tidak sesuai.");
        var meta = await _bedCoordinator.ExecuteAsync("InpatientBedOccupancy:Transfer", request,
            [request.EpisodeId], [source.BedId, request.TargetBedId],
            [new() { BedId = source.BedId, ExpectedVersion = request.ExpectedSourceBedVersion }, new() { BedId = request.TargetBedId, ExpectedVersion = request.ExpectedTargetBedVersion }],
            async ct =>
            {
                var current = await _dbContext.Set<InpBedPlacement>().SingleOrDefaultAsync(x => x.Id == source.Id && x.EndDateTime == null && !x.IsDelete, ct);
                if (current == null || current.Version != request.ExpectedSourcePlacementVersion)
                    throw new InpBedOperationException(409, "STALE_PLACEMENT", "Penempatan asal berubah.");
                var targetClass = await _dbContext.Set<MstBed>().Where(x => x.Id == request.TargetBedId).Select(x => x.Room!.PatientClassId).SingleOrDefaultAsync(ct);
                var comparison = targetClass.HasValue ? await _bedClasses.CompareAsync(current.PatientClassId, targetClass.Value, ct) : "Unknown";
                if (comparison == "Unknown") throw new InpBedOperationException(422, "CLASS_ORDER_UNVERIFIED", "Urutan kelas belum dapat diverifikasi.");
                // BE-RWI-233 (VAL-DNH-13): transfer ke atau dari kelas khusus menerima ketiga kategori
                // pilihan petugas; antarkelas biasa tetap wajib sama dengan perbandingan resmi.
                if (comparison != "Special" &&
                    (request.ManualTransferCategory is < 1 or > 3 || ((InpBedTransferCategory)request.ManualTransferCategory).ToString() != comparison))
                    throw new InpBedOperationException(422, "TRANSFER_CATEGORY_MISMATCH", "Kategori pilihan tidak sesuai perbandingan kelas resmi.");
                var result = await TransferCoreAsync(request, actorUserId, actorDoctorId, ct, comparison); RequireSuccess(result);
                return ("Placement", result.PlacementId!.Value);
            }, cancellationToken);
        if (!meta.IsReplay)
        {
            var destination = await _dbContext.Set<InpBedPlacement>().AsNoTracking().SingleAsync(x => x.Id == meta.ResultEntityId, cancellationToken);
            if (source.ServiceUnitId != destination.ServiceUnitId)
            {
                try { await _transferHandoverService.CreateForTransferAsync(request.EpisodeId, source.Id, destination.Id, actorUserId, cancellationToken); }
                catch (Exception ex) when (ex is not OperationCanceledException) { _logger.LogWarning(ex, "Dokumen serah terima transfer perlu ditindaklanjuti untuk {EpisodeId}.", request.EpisodeId); }
            }
        }
        return InpBedOccupancyOperationResult.Success("Transfer tersimpan.", placementId: meta.ResultEntityId);
    }
    public async Task<InpPlacementCorrectionResult> ApplyPlacementCorrectionAsync(Guid placementId, CorrectPlacementRequest request, Guid actorUserId, CancellationToken cancellationToken = default, Func<CancellationToken, Task>? billingGuard = null)
    {
        var old = await _dbContext.Set<InpBedPlacement>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == placementId && !x.IsDelete, cancellationToken)
            ?? throw new InpBedOperationException(404, "NOT_FOUND", "Penempatan tidak ditemukan.");
        await _bedAccess.RequireEpisodeAsync(old.EpisodeId, "InpatientBedOccupancy:Correct", cancellationToken);
        var ids = new[] { old.BedId, request.CorrectedBedId ?? old.BedId }.Distinct().ToArray();
        var meta = await _bedCoordinator.ExecuteAsync("InpatientBedOccupancy:Correct", new { placementId, request },
            [old.EpisodeId], ids, request.AffectedBedVersions, async ct =>
            {
                if (billingGuard != null) await billingGuard(ct); var result = await ApplyPlacementCorrectionCoreAsync(placementId, request, actorUserId, ct);
                if (result.Status != InpEpisodeOperationStatus.Success)
                    throw new InpBedOperationException(result.Status == InpEpisodeOperationStatus.Conflict ? 409 : result.Status == InpEpisodeOperationStatus.Invalid ? 400 : 422, result.Code ?? "INVALID_INPUT", result.Message);
                return ("Placement", result.PlacementId!.Value);
            }, cancellationToken);
        return InpPlacementCorrectionResult.Ok("Koreksi tersimpan.", meta.ResultEntityId);
    }
    public async Task<BedTransferContextResponse> GetTransferContextAsync(Guid episodeId, CancellationToken ct)
    {
        await _bedAccess.RequireEpisodeAsync(episodeId, "InpatientBedOccupancy:Read", ct);
        var p = await _dbContext.Set<InpBedPlacement>().AsNoTracking().SingleOrDefaultAsync(x => x.EpisodeId == episodeId && x.EndDateTime == null && !x.IsDelete, ct)
            ?? throw new InpBedOperationException(409, "STALE_PLACEMENT", "Episode tidak mempunyai penempatan aktif.");
        await _bedAccess.RequireBedAsync(p.BedId, "InpatientBedOccupancy:Read", ct);
        var bed = await _dbContext.Set<MstBed>().AsNoTracking().SingleAsync(x => x.Id == p.BedId, ct);
        return new() { EpisodeId = episodeId, SourcePlacementId = p.Id, SourcePlacementVersion = p.Version,
            SourceBed = await _bedQueries.ProjectAsync(bed, ct), LocationSnapshotJson = p.LocationSnapshotJson, AsOfUtc = DateTime.UtcNow };
    }
    /// <remarks>
    /// BE-RWI-228 / PLAN-REPAIR-EPS-009: penyaring kamar, kelas, dan pencarian bersifat opsional dan
    /// sama dengan <c>available-beds</c>, sehingga kartu dan angka papan mengikuti penyaring layar.
    /// Tanpa penyaring, jawabannya tidak berubah.
    /// </remarks>
    public async Task<BedBoardResponse> GetBedBoardAsync(Guid? serviceUnitId, Guid? roomId = null, Guid? patientClassId = null,
        string? search = null, CancellationToken cancellationToken = default, bool? isForNewborn = null)
    {
        var m = await _bedQueries.MonitoringAsync(new()
        {
            IsForNewborn = isForNewborn,
            ServiceUnitId = serviceUnitId == Guid.Empty ? null : serviceUnitId,
            RoomId = roomId == Guid.Empty ? null : roomId,
            PatientClassId = patientClassId == Guid.Empty ? null : patientClassId,
            Search = string.IsNullOrWhiteSpace(search) ? null : search
        }, "InpatientBedOccupancy:Read", cancellationToken);
        return new()
        {
            TotalBed = m.Counts.Total, TotalAvailable = m.Counts.Available, TotalOccupied = m.Counts.Occupied,
            TotalReserved = m.Counts.Reserved, TotalUnavailable = m.Counts.Unavailable + m.Counts.Cleaning + m.Counts.WaitingCleaning,
            ServiceUnits = m.ServiceUnits.Select(u => new BedBoardServiceUnitResponse
            {
                ServiceUnitId = u.Id, ServiceUnitName = u.Name, TotalBed = u.Rooms.Sum(r => r.Beds.Count),
                TotalAvailable = u.Rooms.Sum(r => r.Beds.Count(b => b.StatusCode == "Available")),
                TotalOccupied = u.Rooms.Sum(r => r.Beds.Count(b => b.StatusCode == "Occupied")),
                TotalReserved = u.Rooms.Sum(r => r.Beds.Count(b => b.StatusCode == "Reserved")),
                TotalUnavailable = u.Rooms.Sum(r => r.Beds.Count(b => b.StatusCode is "Unavailable" or "Cleaning" or "WaitingCleaning")),
                Rooms = u.Rooms.Select(r => new BedBoardRoomResponse
                {
                    RoomId = r.Id, RoomName = r.Name, RoomCode = r.Beds.FirstOrDefault()?.RawRoomCode,
                    Capacity = r.Beds.FirstOrDefault()?.RawRoomCapacity ?? 0,
                    PatientClassId = r.Beds.FirstOrDefault()?.PatientClassId, PatientClassName = r.Beds.FirstOrDefault()?.PatientClassName,
                    Beds = r.Beds.Select(b => new BedBoardBedResponse
                    {
                        BedId = b.BedId, BedCode = b.BedCode, BedName = b.BedName,
                        BedStatus = b.RawBedStatus,
                        BedStatusName = b.StatusCode, StatusCode = b.StatusCode, StatusLabel = b.StatusLabel, CleaningPhase = b.CleaningPhase,
                        OperationalVersion = b.OperationalVersion, CycleId = b.CycleId, IsBookable = b.IsBookable,
                        IsOccupied = b.StatusCode == "Occupied", IsReserved = b.StatusCode == "Reserved",
                        IsIsolationBed = b.IsIsolationBed, IsForNewborn = b.IsForNewborn,
                        HoldingEpisodeId = b.HoldingContext?.EpisodeId, HoldingEpisodeNumber = b.HoldingContext?.EpisodeNumber,
                        PatientName = b.HoldingContext?.PatientName, ReservationId = b.HoldingContext?.ReservationId,
                        ReservationExpiresAt = b.HoldingContext?.ReservationExpiresAt
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }
    private static void RequireSuccess(InpBedOccupancyOperationResult r)
    {
        if (r.Status == InpEpisodeOperationStatus.Success) return;
        var status = r.Status switch { InpEpisodeOperationStatus.Invalid => 400, InpEpisodeOperationStatus.Forbidden => 403,
            InpEpisodeOperationStatus.NotFound => 404, InpEpisodeOperationStatus.Conflict => 409, _ => 422 };
        var defaultCode = status == 409 ? "PATIENT_ALREADY_ADMITTED" : "BED_NOT_READY";
        throw new InpBedOperationException(status, r.Code ?? r.Failures.FirstOrDefault()?.Code ?? defaultCode, r.Message);
    }
}

