using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
public class CliSurgicalSiteSurveillanceService(ApplicationDbContext db, NursingEpisodeWriteGuard guard,
    NosocomialRecordNumberService numbers, IConfiguration config, LoggerService logger)
{
    private int MaxDay => Math.Clamp(config.GetValue("Clinical:SurgicalSiteSurveillance:MaxDay", 15), 1, 15);
    private IQueryable<CliClinicalInstrumentVersion> Approved() => from v in db.Set<CliClinicalInstrumentVersion>()
        join i in db.Set<CliClinicalInstrument>() on v.InstrumentId equals i.Id
        where !v.IsDelete && !i.IsDelete && i.IsActive && i.InstrumentKind == ClinicalInstrumentKind.SurgicalSiteSurveillanceForm && v.VersionStatus == ClinicalInstrumentVersionStatus.Approved && v.ApprovedByUserId != null
        select v;
    public async Task<SurveillanceFormReadiness> ReadinessAsync(CancellationToken ct)
    { var approved = await Approved().AnyAsync(ct); return new() { HasApprovedVersion = approved, Warning = approved ? null : "Formulir surveilans belum disahkan. Surveilans baru tidak dibentuk." }; }
    public async Task ProcessAsync(CancellationToken ct)
    {
        var version = await Approved().AsNoTracking().OrderByDescending(x => x.ApprovedAt).ThenByDescending(x => x.VersionNumber).FirstOrDefaultAsync(ct);
        if (version != null)
        {
            var cases = await (from c in db.Set<OprCase>().AsNoTracking()
                join ep in db.Set<InpEpisode>().AsNoTracking() on c.EncounterId equals ep.EncounterId
                where !c.IsDelete && c.Status == OprCaseStatus.Completed && !ep.IsDelete && ep.PhysicallyLeftAt == null &&
                    (ep.EpisodeStatus == InpEpisodeStatus.Admitted || ep.EpisodeStatus == InpEpisodeStatus.DischargePending) &&
                    !db.Set<CliSurgicalSiteSurveillance>().Any(x => x.OprCaseId == c.Id)
                select new { Case = c, Episode = ep }).Take(200).ToListAsync(ct);
            foreach (var item in cases)
            {
                await using var tx = await db.Database.BeginTransactionAsync(ct);
                await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"SSI:" + item.Case.Id}, 0))", ct);
                if (await db.Set<CliSurgicalSiteSurveillance>().AnyAsync(x => x.OprCaseId == item.Case.Id, ct)) continue;
                var completed = await db.Set<OprStatusHistory>().AsNoTracking().Where(x => x.OprCaseId == item.Case.Id && x.ToStatus == OprCaseStatus.Completed && !x.IsDelete).OrderByDescending(x => x.OccurredAt).FirstOrDefaultAsync(ct);
                if (completed == null) continue;
                var row = new CliSurgicalSiteSurveillance { OprCaseId = item.Case.Id, InpEpisodeId = item.Episode.Id, PatientId = item.Episode.PatientId, EncounterId = item.Episode.EncounterId,
                    InstrumentVersionId = version.Id, SurgeryCompletedAt = completed.OccurredAt, DayOneDate = HospitalTimeZone.TodayLocal(completed.OccurredAt).AddDays(1), CreateBy = completed.ActorUserId };
                db.Add(row); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("Create", row.Id, completed.ActorUserId);
            }
        }
        var active = await db.Set<CliSurgicalSiteSurveillance>().Where(x => x.Status == CliSurveillanceStatus.Active && !x.IsDelete).Select(x => x.Id).Take(1000).ToListAsync(ct);
        foreach (var id in active)
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct); var row = await LockAsync(id, ct);
            if (row == null || row.Status != CliSurveillanceStatus.Active) continue;
            if (await SynchronizeStatusAsync(row, ct)) { await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("StopOrComplete", row.Id, row.UpdateBy); }
        }
    }
    public async Task<PagedResult<SurveillanceListItem>> ListAsync(SurveillanceQuery request, CancellationToken ct)
    {
        var q = db.Set<CliSurgicalSiteSurveillance>().AsNoTracking().Where(x => !x.IsDelete);
        if (request.EpisodeId.HasValue) q = q.Where(x => x.InpEpisodeId == request.EpisodeId);
        if (request.Status.HasValue) q = q.Where(x => x.Status == request.Status);
        if (request.OnlySuspected == true) q = q.Where(x => x.NosocomialInfectionId != null);
        if (request.ServiceUnitId.HasValue) q = q.Where(x => db.Set<InpEpisode>().Any(e => e.Id == x.InpEpisodeId && e.ServiceUnitId == request.ServiceUnitId));
        var total = await q.CountAsync(ct); var page = Math.Max(1, request.PageNumber); var size = Math.Clamp(request.PageSize, 1, 100);
        var rows = await q.OrderByDescending(x => x.DayOneDate).ThenBy(x => x.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct);
        var patientIds = rows.Select(x => x.PatientId).ToList(); var names = await db.Set<MstPatient>().Where(x => patientIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.FullName, ct);
        return new() { PageNumber = page, PageSize = size, TotalData = total, TotalPage = (int)Math.Ceiling(total / (double)size), Items = rows.Select(x => ListItem(x, names.GetValueOrDefault(x.PatientId))).ToList() };
    }
    public async Task<NursingResult<SurveillanceDetailResponse>> DetailAsync(Guid id, CancellationToken ct)
    {
        var row = await db.Set<CliSurgicalSiteSurveillance>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
        if (row == null) return NursingResult<SurveillanceDetailResponse>.Fail(404, "Surveilans tidak ditemukan.");
        var item = ListItem(row, await db.Set<MstPatient>().Where(x => x.Id == row.PatientId).Select(x => x.FullName).FirstOrDefaultAsync(ct));
        var response = new SurveillanceDetailResponse { Id = item.Id, OprCaseId = item.OprCaseId, EpisodeId = item.EpisodeId, PatientName = item.PatientName, DayOneDate = item.DayOneDate,
            Status = item.Status, NosocomialInfectionId = item.NosocomialInfectionId, StoppedAt = item.StoppedAt, StoppedOnDayNumber = item.StoppedOnDayNumber, Version = item.Version,
            InstrumentVersionId = row.InstrumentVersionId, SummaryResponsesJson = row.SummaryResponsesJson,
            DefinitionJson = await db.Set<CliClinicalInstrumentVersion>().Where(x => x.Id == row.InstrumentVersionId).Select(x => x.DefinitionJson).FirstAsync(ct) };
        var entries = await db.Set<CliSurgicalSiteSurveillanceEntry>().AsNoTracking().Where(x => x.SurveillanceId == id && !x.IsDelete).OrderBy(x => x.DayNumber).ToListAsync(ct);
        response.Entries = entries.Select(Entry).ToList();
        for (var day = 1; day <= MaxDay; day++) { var date = row.DayOneDate.AddDays(day - 1); var temp = await TemperatureAsync(row.EncounterId, date, ct); response.Temperatures.Add(new() { DayNumber = day, Date = date, TemperatureMaxCelsius = temp, FeverIndicator = temp.HasValue ? temp >= 38 : null }); }
        return NursingResult<SurveillanceDetailResponse>.Ok(response, "Formulir surveilans berhasil diambil.");
    }
    public async Task<NursingResult<SurveillanceEntryResponse>> PutEntryAsync(Guid id, int day, PutSurveillanceEntryRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct); var row = await LockAsync(id, ct);
        if (row == null) return NursingResult<SurveillanceEntryResponse>.Fail(404, "Surveilans tidak ditemukan.");
        if (await SynchronizeStatusAsync(row, ct)) { await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return NursingResult<SurveillanceEntryResponse>.Fail(422, "Surveilans sudah berhenti.", "CLI-SSI-001"); }
        if (row.Status != CliSurveillanceStatus.Active) return NursingResult<SurveillanceEntryResponse>.Fail(422, "Surveilans sudah berhenti.", "CLI-SSI-001");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct); if (!access.IsSuccess) return access.Cast<SurveillanceEntryResponse>();
        if (day < 1 || day > MaxDay) return NursingResult<SurveillanceEntryResponse>.Fail(422, "Hari di luar 1–15.", "CLI-SSI-002");
        var date = row.DayOneDate.AddDays(day - 1);
        if (date > HospitalTimeZone.TodayLocal(DateTime.UtcNow)) return NursingResult<SurveillanceEntryResponse>.Fail(422, "Hari surveilans belum tiba.", "CLI-SSI-002");
        if (!await ValidateResponsesAsync(row.InstrumentVersionId, request.ResponsesJson, false, ct)) return NursingResult<SurveillanceEntryResponse>.Fail(422, "Isian tidak sesuai versi formulir.", "CLI-SSI-003");
        var entry = await db.Set<CliSurgicalSiteSurveillanceEntry>().FirstOrDefaultAsync(x => x.SurveillanceId == id && x.DayNumber == day, ct);
        if (entry != null)
        {
            if (entry.RevisionNumber != request.ExpectedRevision) return NursingResult<SurveillanceEntryResponse>.Fail(409, "Revisi berubah. Muat ulang isian.");
            if (string.IsNullOrWhiteSpace(request.CorrectionReason)) return NursingResult<SurveillanceEntryResponse>.Fail(400, "Alasan koreksi wajib diisi.");
            db.Add(new CliSurgicalSiteSurveillanceEntryRevision { EntryId = entry.Id, RevisionNumber = entry.RevisionNumber, PreviousResponsesJson = entry.ResponsesJson, Reason = request.CorrectionReason.Trim(), RevisedByUserId = actor, RevisedAt = DateTime.UtcNow, CreateBy = actor });
            entry.RevisionNumber++; entry.UpdateBy = actor; entry.UpdateDateTime = DateTime.UtcNow;
        }
        else { entry = new() { SurveillanceId = id, DayNumber = day, EntryDate = date, CreateBy = actor }; db.Add(entry); }
        entry.ResponsesJson = request.ResponsesJson; entry.RecordedByUserId = actor;
        entry.TemperatureMaxCelsiusSnapshot = await TemperatureAsync(row.EncounterId, date, ct);
        entry.FeverIndicatorFromVitals = entry.TemperatureMaxCelsiusSnapshot.HasValue ? entry.TemperatureMaxCelsiusSnapshot >= 38 : null;
        row.Version++; row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("Entry", id, actor);
        return NursingResult<SurveillanceEntryResponse>.Ok(Entry(entry), "Isian harian tersimpan.");
    }
    public async Task<NursingResult<SurveillanceDetailResponse>> PutSummaryAsync(Guid id, PutSurveillanceSummaryRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct); var row = await LockAsync(id, ct);
        if (row == null) return NursingResult<SurveillanceDetailResponse>.Fail(404, "Surveilans tidak ditemukan.");
        if (await SynchronizeStatusAsync(row, ct)) { await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return NursingResult<SurveillanceDetailResponse>.Fail(422, "Surveilans sudah berhenti.", "CLI-SSI-001"); }
        if (row.Status != CliSurveillanceStatus.Active) return NursingResult<SurveillanceDetailResponse>.Fail(422, "Surveilans sudah berhenti.", "CLI-SSI-001");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct); if (!access.IsSuccess) return access.Cast<SurveillanceDetailResponse>();
        if (row.Version != request.ExpectedVersion) return NursingResult<SurveillanceDetailResponse>.Fail(409, "Versi berubah.");
        if (row.SummaryResponsesJson != "{}" && string.IsNullOrWhiteSpace(request.Reason)) return NursingResult<SurveillanceDetailResponse>.Fail(400, "Alasan koreksi wajib diisi.");
        if (!await ValidateResponsesAsync(row.InstrumentVersionId, request.SummaryResponsesJson, true, ct)) return NursingResult<SurveillanceDetailResponse>.Fail(422, "Isian ringkasan tidak sesuai versi formulir.", "CLI-SSI-003");
        row.SummaryResponsesJson = request.SummaryResponsesJson; row.Version++; row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("Summary", id, actor); return await DetailAsync(id, ct);
    }
    public async Task<NursingResult<SurveillanceDetailResponse>> FlagSuspectedAsync(Guid id, FlagSurveillanceSuspectedRequest request, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct); var row = await LockAsync(id, ct);
        if (row == null) return NursingResult<SurveillanceDetailResponse>.Fail(404, "Surveilans tidak ditemukan.");
        if (row.NosocomialInfectionId.HasValue) return NursingResult<SurveillanceDetailResponse>.Fail(409, "Surveilans sudah ditandai dicurigai.", "CLI-SSI-004");
        if (string.IsNullOrWhiteSpace(request.Note) || request.OnsetDate == default || request.OnsetDate > HospitalTimeZone.TodayLocal(DateTime.UtcNow)) return NursingResult<SurveillanceDetailResponse>.Fail(400, "Tanggal dan catatan dugaan wajib sah.");
        var ep = await db.Set<InpEpisode>().AsNoTracking().FirstAsync(x => x.Id == row.InpEpisodeId, ct); var onset = HospitalTimeZone.ToUtc(request.OnsetDate, TimeOnly.MinValue);
        var infection = new TrxNosocomialInfection { NosocomialRecordNumber = await numbers.AllocateAsync(DateTime.UtcNow, actor, ct), PatientId = row.PatientId, EncounterId = row.EncounterId,
            ServiceUnitId = ep.ServiceUnitId, InfectionType = NosocomialInfectionType.SurgicalSiteInfection, Status = NosocomialInfectionStatus.Suspected, OnsetDateTime = onset,
            AdmissionDateTimeSnapshot = ep.AdmittedAt, HoursSinceAdmission = ep.AdmittedAt.HasValue ? (int)(onset - ep.AdmittedAt.Value).TotalHours : null,
            OnsetCategory = ep.AdmittedAt.HasValue ? ((onset - ep.AdmittedAt.Value).TotalHours >= 48 ? NosocomialInfectionOnsetCategory.HealthcareAssociated : NosocomialInfectionOnsetCategory.PresentOnAdmission) : NosocomialInfectionOnsetCategory.Unknown,
            ReportedByUserId = actor, ReportedAt = DateTime.UtcNow, CriteriaMet = request.Note, CreateBy = actor };
        db.Add(infection); row.NosocomialInfectionId = infection.Id; row.SuspectedFlaggedAt = DateTime.UtcNow; row.SuspectedFlaggedByUserId = actor; row.ReviewNote = request.Note; row.Version++; row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("FlagSuspected", id, actor); return await DetailAsync(id, ct);
    }
    private async Task<bool> ValidateResponsesAsync(Guid versionId, string json, bool summary, CancellationToken ct)
    {
        try
        {
            using var document = JsonDocument.Parse(json); if (document.RootElement.ValueKind != JsonValueKind.Object) return false;
            var definition = ClinicalInstrumentDefinitionEngine.Parse(await db.Set<CliClinicalInstrumentVersion>().Where(x => x.Id == versionId).Select(x => x.DefinitionJson).FirstAsync(ct), out _);
            if (definition == null) return false;
            definition.Sections = definition.Sections.Where(x => (x.Code?.StartsWith("SUMMARY", StringComparison.OrdinalIgnoreCase) == true) == summary).ToList();
            var codes = definition.Sections.SelectMany(x => x.Items).Select(x => x.Code).ToHashSet(); definition.RequiredItemCodes = definition.RequiredItemCodes.Where(x => codes.Contains(x)).ToList();
            var responses = ClinicalInstrumentDefinitionEngine.ParseResponses(json);
            return ClinicalInstrumentDefinitionEngine.Score(definition, responses).Errors.Count == 0 && ClinicalInstrumentDefinitionEngine.MissingRequiredItems(definition, responses, _ => false).Count == 0;
        }
        catch (JsonException) { return false; }
    }
    private async Task<bool> SynchronizeStatusAsync(CliSurgicalSiteSurveillance row, CancellationToken ct)
    {
        if (row.Status != CliSurveillanceStatus.Active) return false;
        var ep = await db.Set<InpEpisode>().AsNoTracking().FirstAsync(x => x.Id == row.InpEpisodeId, ct);
        var end = HospitalTimeZone.ToUtc(row.DayOneDate.AddDays(MaxDay), TimeOnly.MinValue);
        if (ep.PhysicallyLeftAt is DateTime left && left < end) { row.Status = CliSurveillanceStatus.StoppedOnDeparture; row.StoppedAt = left; row.StoppedOnDayNumber = Math.Clamp(HospitalTimeZone.TodayLocal(left).DayNumber - row.DayOneDate.DayNumber + 1, 0, MaxDay); row.UpdateBy = ep.PhysicallyLeftByUserId ?? ep.UpdateBy; }
        else if (DateTime.UtcNow >= end) { row.Status = CliSurveillanceStatus.Completed; row.StoppedAt = end; row.UpdateBy = row.CreateBy; }
        else return false;
        row.Version++; row.UpdateDateTime = DateTime.UtcNow; return true;
    }
    private Task<decimal?> TemperatureAsync(Guid encounter, DateOnly date, CancellationToken ct)
    {
        var from = HospitalTimeZone.ToUtc(date, TimeOnly.MinValue); var to = HospitalTimeZone.ToUtc(date.AddDays(1), TimeOnly.MinValue);
        return db.Set<TrxPatientVitalSign>().AsNoTracking().Where(x => x.EncounterId == encounter && !x.IsDelete && !x.IsCancel && x.ObservationDateTime >= from && x.ObservationDateTime < to &&
            (x.VitalSignStatus == PatientVitalSignStatus.Recorded || x.VitalSignStatus == PatientVitalSignStatus.Verified || x.VitalSignStatus == PatientVitalSignStatus.Corrected)).MaxAsync(x => x.Temperature, ct);
    }
    private Task<CliSurgicalSiteSurveillance?> LockAsync(Guid id, CancellationToken ct) => db.Set<CliSurgicalSiteSurveillance>().FromSqlInterpolated($"SELECT * FROM public.\"CliSurgicalSiteSurveillance\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(ct);
    private Task LogAsync(string action, Guid id, Guid actor) => logger.InfoAsync("Clinical.SurgicalSiteSurveillance", action, "Surveilans luka operasi berubah.", new { Id = id, ActorUserId = actor });
    private static SurveillanceListItem ListItem(CliSurgicalSiteSurveillance x, string? name) => new() { Id = x.Id, OprCaseId = x.OprCaseId, EpisodeId = x.InpEpisodeId, PatientName = name ?? string.Empty, DayOneDate = x.DayOneDate, Status = x.Status, NosocomialInfectionId = x.NosocomialInfectionId, StoppedAt = x.StoppedAt, StoppedOnDayNumber = x.StoppedOnDayNumber, Version = x.Version };
    private static SurveillanceEntryResponse Entry(CliSurgicalSiteSurveillanceEntry x) => new() { Id = x.Id, DayNumber = x.DayNumber, EntryDate = x.EntryDate, ResponsesJson = x.ResponsesJson, TemperatureMaxCelsiusSnapshot = x.TemperatureMaxCelsiusSnapshot, FeverIndicatorFromVitals = x.FeverIndicatorFromVitals, RevisionNumber = x.RevisionNumber };
}
