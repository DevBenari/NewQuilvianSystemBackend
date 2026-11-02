using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Helpers;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwFloorBoardQueryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpBedManagementQueryService _bedQueries;
        private readonly NvwVitalSignIntervalService _intervals;

        public NvwFloorBoardQueryService(
            ApplicationDbContext dbContext,
            InpBedManagementQueryService bedQueries,
            NvwVitalSignIntervalService intervals)
        {
            _dbContext = dbContext;
            _bedQueries = bedQueries;
            _intervals = intervals;
        }

        public static string BoardStatusLabel(string statusCode) => statusCode switch
        {
            "Occupied" => "Terisi",
            "Reserved" => "Dipesan — pasien akan datang",
            "WaitingCleaning" or "Cleaning" => "Dibersihkan",
            "Available" => "Kosong",
            _ => "Tidak tersedia"
        };

        public async Task<NvwFloorBoardResponse> GetBoardAsync(string deviceFloorCode, CancellationToken cancellationToken)
        {
            var floorCode = NvwDisplayDeviceService.NormalizeFloorCode(deviceFloorCode);
            var nowUtc = DateTime.UtcNow;
            var intervalSet = await _intervals.GetActiveAsync(cancellationToken);

            var board = new NvwFloorBoardResponse
            {
                FloorCode = floorCode,
                FloorName = FloorPlanRoomNaming.FloorName(floorCode),
                AsOfUtc = nowUtc,
                IntervalsConfigured = intervalSet.IsConfigured
            };

            var rooms = await _dbContext.MstRooms.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.FloorCode != null && x.FloorCode.Trim().ToUpper() == floorCode)
                .ToListAsync(cancellationToken);

            if (rooms.Count == 0)
                return board;

            var roomIds = rooms.Select(x => x.Id).ToList();
            var beds = await _dbContext.MstBeds.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && roomIds.Contains(x.RoomId))
                .ToListAsync(cancellationToken);

            var roomById = rooms.ToDictionary(x => x.Id);
            foreach (var bed in beds)
                bed.Room = roomById[bed.RoomId];

            var projection = await _bedQueries.ProjectBatchAsync(beds, cancellationToken);

            var episodeByBed = projection.Placements.ToDictionary(x => x.Key, x => x.Value.EpisodeId);
            var episodeIds = episodeByBed.Values.Distinct().ToList();

            var occupants = await LoadOccupantsAsync(episodeIds, cancellationToken);
            var nurses = await LoadResponsibleNursesAsync(episodeIds, cancellationToken);
            var vitalSigns = await LoadLatestVitalSignsAsync(episodeIds, cancellationToken);
            var highFallRisk = await LoadHighFallRiskEpisodesAsync(episodeIds, cancellationToken);
            var allergicPatients = await LoadAllergicPatientsAsync(
                occupants.Values.Select(x => x.PatientId).Distinct().ToList(),
                cancellationToken);
            var intervalByLevel = intervalSet.Items.ToDictionary(x => x.EwsRiskLevel, x => x.IntervalMinutes);

            board.Wards = projection.Rows
                .GroupBy(x => x.ServiceUnitId)
                .Select(ward => new NvwWardBoardResponse
                {
                    ServiceUnitId = ward.Key,
                    ServiceUnitName = projection.Units.TryGetValue(ward.Key, out var unit) ? unit.ServiceUnitName : string.Empty,
                    RosterAvailable = false,
                    Beds = ward
                        .Select(row => BuildCard(row, episodeByBed, occupants, nurses, vitalSigns, highFallRisk, allergicPatients, intervalByLevel, nowUtc))
                        .OrderByDescending(card => card.IsCritical)
                        .ThenBy(card => card.BedCode, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                })
                .OrderBy(ward => ward.ServiceUnitName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return board;
        }

        private static NvwBedCardResponse BuildCard(
            BedOperationalResponse row,
            IReadOnlyDictionary<Guid, Guid> episodeByBed,
            IReadOnlyDictionary<Guid, Occupant> occupants,
            IReadOnlyDictionary<Guid, string?> nurses,
            IReadOnlyDictionary<Guid, LatestVitalSign> vitalSigns,
            IReadOnlySet<Guid> highFallRisk,
            IReadOnlySet<Guid> allergicPatients,
            IReadOnlyDictionary<EwsRiskLevel, int> intervalByLevel,
            DateTime nowUtc)
        {
            var card = new NvwBedCardResponse
            {
                BedId = row.BedId,
                BedCode = row.BedCode,
                BedName = row.BedName,
                RoomName = row.RoomName,
                StatusCode = row.StatusCode,
                StatusLabel = BoardStatusLabel(row.StatusCode),
                IsNewbornBed = row.IsForNewborn
            };

            if (!episodeByBed.TryGetValue(row.BedId, out var episodeId) || !occupants.TryGetValue(episodeId, out var occupant))
                return card;

            card.Patient = occupant.Masked;
            card.RequiresIsolation = occupant.RequiresIsolation;
            card.IsolationMismatch = occupant.RequiresIsolation != row.IsIsolationBed;
            card.HasActiveAllergy = allergicPatients.Contains(occupant.PatientId);
            card.HasHighFallRisk = highFallRisk.Contains(episodeId);

            if (nurses.TryGetValue(episodeId, out var nurseName) && !string.IsNullOrWhiteSpace(nurseName))
            {
                card.ResponsibleNurseName = nurseName;
                card.HasResponsibleNurse = true;
            }

            if (vitalSigns.TryGetValue(episodeId, out var vitalSign))
            {
                card.VitalSign = BuildVitalSign(vitalSign, intervalByLevel, nowUtc);
                card.IsCritical = card.VitalSign.IsCritical;
            }

            return card;
        }

        private static NvwVitalSignCardResponse BuildVitalSign(
            LatestVitalSign vitalSign,
            IReadOnlyDictionary<EwsRiskLevel, int> intervalByLevel,
            DateTime nowUtc)
        {
            var ageMinutes = Math.Max(0, (int)Math.Floor((nowUtc - vitalSign.ObservedAtUtc).TotalMinutes));
            var isOverdue = intervalByLevel.TryGetValue(vitalSign.EwsRiskLevel, out var intervalMinutes) && ageMinutes > intervalMinutes;

            return new NvwVitalSignCardResponse
            {
                ObservedAtUtc = vitalSign.ObservedAtUtc,
                AgeMinutes = ageMinutes,
                EarlyWarningScore = vitalSign.EarlyWarningScore,
                EwsRiskLevel = vitalSign.EwsRiskLevel.ToString(),
                IsCritical = vitalSign.IsCritical || vitalSign.EwsRiskLevel == EwsRiskLevel.Critical,
                Systolic = vitalSign.Systolic,
                Diastolic = vitalSign.Diastolic,
                PulseRate = vitalSign.PulseRate,
                RespiratoryRate = vitalSign.RespiratoryRate,
                Temperature = vitalSign.Temperature,
                OxygenSaturation = vitalSign.OxygenSaturation,
                IsOverdue = isOverdue,
                OverdueMinutes = isOverdue ? ageMinutes - intervalMinutes : null
            };
        }

        private async Task<Dictionary<Guid, Occupant>> LoadOccupantsAsync(List<Guid> episodeIds, CancellationToken cancellationToken)
        {
            if (episodeIds.Count == 0)
                return new Dictionary<Guid, Occupant>();

            var rows = await _dbContext.InpEpisodes.AsNoTracking()
                .Where(x => episodeIds.Contains(x.Id) && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.PatientId,
                    x.RequiresIsolation,
                    FullName = x.Patient != null ? x.Patient.FullName : null,
                    Gender = x.Patient != null ? x.Patient.Gender : null,
                    BirthDate = x.Patient != null ? x.Patient.BirthDate : null
                })
                .ToListAsync(cancellationToken);

            var today = AppDateTimeHelper.OperationalDate();

            return rows.ToDictionary(
                x => x.Id,
                x => new Occupant(
                    x.PatientId,
                    x.RequiresIsolation,
                    new NvwMaskedPatientResponse
                    {
                        Initials = NvwPatientMasking.Initials(x.FullName),
                        GenderCode = NvwPatientMasking.GenderCode(x.Gender),
                        AgeLabel = NvwPatientMasking.AgeLabel(x.BirthDate, today)
                    }));
        }

        private async Task<Dictionary<Guid, string?>> LoadResponsibleNursesAsync(List<Guid> episodeIds, CancellationToken cancellationToken)
        {
            if (episodeIds.Count == 0)
                return new Dictionary<Guid, string?>();

            var rows = await _dbContext.InpNurseAssignments.AsNoTracking()
                .Where(x => episodeIds.Contains(x.EpisodeId) && x.IsActive && x.EndDateTime == null && !x.IsDelete)
                .OrderByDescending(x => x.SequenceNumber)
                .Select(x => new
                {
                    x.EpisodeId,
                    Name = x.Employee != null ? x.Employee.FullName : null
                })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.EpisodeId)
                .ToDictionary(x => x.Key, x => x.First().Name);
        }

        private async Task<Dictionary<Guid, LatestVitalSign>> LoadLatestVitalSignsAsync(List<Guid> episodeIds, CancellationToken cancellationToken)
        {
            if (episodeIds.Count == 0)
                return new Dictionary<Guid, LatestVitalSign>();

            var validVitalSigns = _dbContext.TrxPatientVitalSigns
                .Where(x =>
                    x.InpEpisodeId != null &&
                    episodeIds.Contains(x.InpEpisodeId.Value) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    (x.VitalSignStatus == PatientVitalSignStatus.Recorded ||
                     x.VitalSignStatus == PatientVitalSignStatus.Verified ||
                     x.VitalSignStatus == PatientVitalSignStatus.Corrected));

            var rows = await validVitalSigns
                .AsNoTracking()
                .Where(x => !validVitalSigns.Any(y => y.InpEpisodeId == x.InpEpisodeId && y.ObservationDateTime > x.ObservationDateTime))
                .Select(x => new LatestVitalSign(
                    x.InpEpisodeId!.Value,
                    x.ObservationDateTime,
                    x.EarlyWarningScore,
                    x.EwsRiskLevel,
                    x.IsCritical,
                    x.BloodPressureSystolic,
                    x.BloodPressureDiastolic,
                    x.PulseRate,
                    x.RespiratoryRate,
                    x.Temperature,
                    x.OxygenSaturation))
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.EpisodeId)
                .ToDictionary(x => x.Key, x => x.First());
        }

        private async Task<HashSet<Guid>> LoadHighFallRiskEpisodesAsync(List<Guid> episodeIds, CancellationToken cancellationToken)
        {
            if (episodeIds.Count == 0)
                return new HashSet<Guid>();

            var completedFallRisk = _dbContext.TrxPatientAssessments
                .Where(x =>
                    x.InpEpisodeId != null &&
                    episodeIds.Contains(x.InpEpisodeId.Value) &&
                    x.AssessmentType == PatientAssessmentType.FallRisk &&
                    x.AssessmentStatus == PatientAssessmentStatus.Completed &&
                    !x.IsDelete &&
                    x.IsActive);

            var rows = await completedFallRisk
                .AsNoTracking()
                .Where(x => !completedFallRisk.Any(y => y.InpEpisodeId == x.InpEpisodeId && y.AssessmentDateTime > x.AssessmentDateTime))
                .Where(x => x.FallRiskStatus == FallRiskStatus.HighRisk)
                .Select(x => x.InpEpisodeId!.Value)
                .ToListAsync(cancellationToken);

            return rows.ToHashSet();
        }

        private async Task<HashSet<Guid>> LoadAllergicPatientsAsync(List<Guid> patientIds, CancellationToken cancellationToken)
        {
            if (patientIds.Count == 0)
                return new HashSet<Guid>();

            var rows = await _dbContext.TrxPatientAllergies.AsNoTracking()
                .Where(x =>
                    patientIds.Contains(x.PatientId) &&
                    x.AllergyStatus == PatientAllergyStatus.Active &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => x.PatientId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return rows.ToHashSet();
        }

        private sealed record Occupant(Guid PatientId, bool RequiresIsolation, NvwMaskedPatientResponse Masked);

        private sealed record LatestVitalSign(
            Guid EpisodeId,
            DateTime ObservedAtUtc,
            int? EarlyWarningScore,
            EwsRiskLevel EwsRiskLevel,
            bool IsCritical,
            int? Systolic,
            int? Diastolic,
            int? PulseRate,
            int? RespiratoryRate,
            decimal? Temperature,
            decimal? OxygenSaturation);
    }
}
