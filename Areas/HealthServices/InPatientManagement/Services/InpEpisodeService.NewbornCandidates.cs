using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public partial class InpEpisodeService
{
    /// <summary>
    /// Kandidat ibu berasal dari kasus operasi persalinan, bukan seluruh pasien perempuan.
    /// Lokasi sekarang hanya informasi kontekstual dan tidak diwajibkan merupakan kamar operasi.
    /// </summary>
    public async Task<NewbornMotherCandidatePagedResult> GetNewbornMotherCandidatesAsync(
        NewbornMotherCandidateQuery query,
        CancellationToken cancellationToken = default)
    {
        query ??= new NewbornMotherCandidateQuery();
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var search = query.Search?.Trim().ToLowerInvariant();
        var fromDate = query.FromDate.HasValue
            ? DateTime.SpecifyKind(query.FromDate.Value.Date, DateTimeKind.Utc)
            : (DateTime?)null;
        var toDateExclusive = query.ToDate.HasValue
            ? DateTime.SpecifyKind(
                query.ToDate.Value.Date == DateTime.MaxValue.Date
                    ? DateTime.MaxValue
                    : query.ToDate.Value.Date.AddDays(1),
                DateTimeKind.Utc)
            : (DateTime?)null;

        var cases = _dbContext.Set<OprCase>()
            .AsNoTracking()
            .Where(x =>
                x.SurgicalServiceType == OprSurgicalServiceType.Obstetric &&
                (x.Status == OprCaseStatus.InProgress || x.Status == OprCaseStatus.Completed) &&
                !x.IsDelete &&
                !x.IsCancel &&
                x.Patient != null &&
                x.Patient.IsActive &&
                !x.Patient.IsDelete &&
                x.Encounter != null &&
                !x.Encounter.IsDelete &&
                !x.Encounter.IsCancel &&
                _dbContext.Set<RegPatientEncounterGuarantor>().Any(p =>
                    p.EncounterId == x.EncounterId && p.IsActive && !p.IsDelete && !p.IsCancel));

        if (fromDate.HasValue)
        {
            cases = cases.Where(x => x.RequestedAt >= fromDate.Value);
        }

        if (toDateExclusive.HasValue)
        {
            cases = cases.Where(x => x.RequestedAt < toDateExclusive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            cases = cases.Where(x =>
                x.CaseNumber.ToLower().Contains(search) ||
                x.Patient!.FullName.ToLower().Contains(search) ||
                x.Patient.MedicalRecordNumber.ToLower().Contains(search) ||
                x.Encounter!.EncounterNumber.ToLower().Contains(search));
        }

        var totalData = await cases.CountAsync(cancellationToken);
        var page = await cases
            .OrderByDescending(x => x.RequestedAt)
            .ThenByDescending(x => x.CreateDateTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.CaseNumber,
                x.RequestedAt,
                x.Status,
                x.PatientId,
                MotherName = x.Patient!.FullName,
                MotherMedicalRecordNumber = x.Patient.MedicalRecordNumber,
                x.EncounterId,
                MotherEncounterNumber = x.Encounter!.EncounterNumber
            })
            .ToListAsync(cancellationToken);

        var encounterIds = page.Select(x => x.EncounterId).ToList();
        var patientIds = page.Select(x => x.PatientId).ToList();
        var caseIds = page.Select(x => x.Id).ToList();

        var paymentRows = await _dbContext.Set<RegPatientEncounterGuarantor>()
            .AsNoTracking()
            .Where(x => encounterIds.Contains(x.EncounterId) && x.IsActive && !x.IsDelete && !x.IsCancel)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Priority)
            .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
            .Select(x => new
            {
                x.EncounterId,
                x.Id,
                x.PaymentType,
                x.PaymentSourceNameSnapshot,
                VersionAt = x.UpdateDateTime ?? x.CreateDateTime
            })
            .ToListAsync(cancellationToken);
        var payments = paymentRows
            .GroupBy(x => x.EncounterId)
            .ToDictionary(x => x.Key, x => x.First());

        var episodeRows = await _dbContext.Set<InpEpisode>()
            .AsNoTracking()
            .Where(x =>
                patientIds.Contains(x.PatientId) &&
                (x.EpisodeStatus == InpEpisodeStatus.Draft ||
                 x.EpisodeStatus == InpEpisodeStatus.Admitted ||
                 x.EpisodeStatus == InpEpisodeStatus.DischargePending) &&
                !x.IsDelete &&
                !x.IsCancel)
            .OrderByDescending(x => x.CreateDateTime)
            .Select(x => new { x.Id, x.PatientId, x.EpisodeNumber })
            .ToListAsync(cancellationToken);
        var episodes = episodeRows.GroupBy(x => x.PatientId).ToDictionary(x => x.Key, x => x.First());

        var episodeIds = episodes.Values.Select(x => x.Id).ToList();
        var placementRows = await _dbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .Where(x => episodeIds.Contains(x.EpisodeId) && x.EndDateTime == null && !x.IsDelete)
            .OrderByDescending(x => x.SequenceNumber)
            .Select(x => new
            {
                x.EpisodeId,
                x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                x.RoomId,
                RoomName = x.Room != null ? x.Room.RoomName : null
            })
            .ToListAsync(cancellationToken);
        var placements = placementRows.GroupBy(x => x.EpisodeId).ToDictionary(x => x.Key, x => x.First());

        var babyPatients = await _dbContext.Set<MstPatient>()
            .AsNoTracking()
            .Where(x => x.DeliveryCaseId.HasValue && caseIds.Contains(x.DeliveryCaseId.Value) && !x.IsDelete)
            .OrderBy(x => x.BirthOrder)
            .Select(x => new
            {
                x.Id,
                DeliveryCaseId = x.DeliveryCaseId!.Value,
                x.FullName,
                x.MedicalRecordNumber,
                x.BirthOrder
            })
            .ToListAsync(cancellationToken);

        var babyPatientIds = babyPatients.Select(b => b.Id).ToList();
        var babyEpisodeRows = await _dbContext.Set<InpEpisode>()
            .AsNoTracking()
            .Where(x => babyPatientIds.Contains(x.PatientId) && !x.IsDelete && !x.IsCancel)
            .OrderByDescending(x => x.CreateDateTime)
            .Select(x => new
            {
                x.Id,
                x.PatientId,
                x.EpisodeNumber,
                x.EpisodeStatus
            })
            .ToListAsync(cancellationToken);
        var babyEpisodes = babyEpisodeRows
            .GroupBy(x => x.PatientId)
            .ToDictionary(x => x.Key, x => x.First());

        var babyEpisodeIds = babyEpisodes.Values.Select(x => x.Id).ToList();
        var babyPlacementRows = await _dbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .Where(x => babyEpisodeIds.Contains(x.EpisodeId) && x.EndDateTime == null && !x.IsDelete)
            .OrderByDescending(x => x.SequenceNumber)
            .Select(x => new
            {
                x.EpisodeId,
                RoomName = x.Room != null ? x.Room.RoomName : null,
                BedCode = x.Bed != null ? x.Bed.BedCode : null
            })
            .ToListAsync(cancellationToken);
        var babyPlacements = babyPlacementRows
            .GroupBy(x => x.EpisodeId)
            .ToDictionary(x => x.Key, x => x.First());

        var babyCounts = babyPatients
            .GroupBy(x => x.DeliveryCaseId)
            .ToDictionary(g => g.Key, g => g.Count());

        var sequences = await _dbContext.Set<PatNewbornBirthSequence>()
            .AsNoTracking()
            .Where(x => caseIds.Contains(x.DeliveryCaseId) && !x.IsDelete)
            .ToDictionaryAsync(x => x.DeliveryCaseId, x => x.LastAllocatedBirthOrder, cancellationToken);

        var items = new List<NewbornMotherCandidateResponse>(page.Count);
        foreach (var item in page)
        {
            if (!payments.TryGetValue(item.EncounterId, out var payment))
            {
                continue;
            }

            episodes.TryGetValue(item.PatientId, out var episode);
            var placement = episode != null && placements.TryGetValue(episode.Id, out var currentPlacement)
                ? currentPlacement
                : null;
            var registeredCount = babyCounts.GetValueOrDefault(item.Id);
            var nextOrder = Math.Max(registeredCount, sequences.GetValueOrDefault(item.Id)) + 1;

            var caseBabies = babyPatients
                .Where(b => b.DeliveryCaseId == item.Id)
                .Select(b =>
                {
                    babyEpisodes.TryGetValue(b.Id, out var ep);
                    var bPlacement = ep != null && babyPlacements.TryGetValue(ep.Id, out var bp) ? bp : null;
                    var isCurrentlyInRoom = ep != null && ep.EpisodeStatus == InpEpisodeStatus.Admitted && bPlacement != null;
                    return new NewbornRegisteredBabyDto
                    {
                        PatientId = b.Id,
                        FullName = b.FullName,
                        MedicalRecordNumber = b.MedicalRecordNumber,
                        BirthOrder = b.BirthOrder ?? 0,
                        EpisodeId = ep?.Id,
                        EpisodeNumber = ep?.EpisodeNumber,
                        EpisodeStatus = ep != null ? (int)ep.EpisodeStatus : null,
                        EpisodeStatusName = ep?.EpisodeStatus.ToString(),
                        RoomName = bPlacement?.RoomName,
                        BedCode = bPlacement?.BedCode,
                        IsCurrentlyInRoom = isCurrentlyInRoom
                    };
                })
                .ToList();

            var hasActiveBaby = caseBabies.Any(b => b.IsCurrentlyInRoom || b.EpisodeStatus == (int)InpEpisodeStatus.Admitted || b.EpisodeStatus == (int)InpEpisodeStatus.Draft);

            items.Add(new NewbornMotherCandidateResponse
            {
                DeliveryCaseId = item.Id,
                DeliveryCaseNumber = item.CaseNumber,
                RequestedAt = item.RequestedAt,
                DeliveryCaseStatus = (int)item.Status,
                DeliveryCaseStatusName = item.Status.ToString(),
                MotherPatientId = item.PatientId,
                MotherName = item.MotherName,
                MotherMedicalRecordNumber = item.MotherMedicalRecordNumber,
                MotherEncounterId = item.EncounterId,
                MotherEncounterNumber = item.MotherEncounterNumber,
                PaymentSourceId = payment.Id,
                PaymentSourceExpectedAt = payment.VersionAt,
                PaymentType = (int)payment.PaymentType,
                PaymentSourceName = payment.PaymentSourceNameSnapshot,
                MotherEpisodeId = episode?.Id,
                MotherEpisodeNumber = episode?.EpisodeNumber,
                CurrentServiceUnitId = placement?.ServiceUnitId,
                CurrentServiceUnitName = placement?.ServiceUnitName,
                CurrentRoomId = placement?.RoomId,
                CurrentRoomName = placement?.RoomName,
                RegisteredBabyCount = registeredCount,
                NextBirthOrderPreview = nextOrder,
                HasActiveInpatientBaby = hasActiveBaby,
                RegisteredBabies = caseBabies
            });
        }

        return new NewbornMotherCandidatePagedResult
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
            Items = items
        };
    }
}
