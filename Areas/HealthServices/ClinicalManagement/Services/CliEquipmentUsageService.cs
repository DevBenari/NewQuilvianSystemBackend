using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;

public sealed class CliEquipmentUsageService(ApplicationDbContext db, NursingEpisodeWriteGuard guard,
    InpatientClinicalContextService clinical, IInpBillingClearanceAdapter clearance,
    ClinicalMilestoneFactProducer facts, QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services.BillingClinicalChargeBridgeService bridge, LoggerService logger)
{
    public static decimal CalculateBilledUnits(MstEquipmentChargeUnit unit, DateTime start, DateTime end, decimal? quantity,
        MstEquipmentRoundingRule rounding = MstEquipmentRoundingRule.CeilingWholeUnit)
    {
        if (end < start) throw new ArgumentException("Waktu selesai mendahului waktu mulai.");
        var raw = unit switch
        {
            MstEquipmentChargeUnit.PerUse when quantity > 0 => quantity.Value,
            MstEquipmentChargeUnit.PerHour => (decimal)(end - start).Ticks / TimeSpan.TicksPerHour,
            MstEquipmentChargeUnit.PerDay => (decimal)(end - start).Ticks / TimeSpan.TicksPerDay,
            _ => throw new ArgumentException("Satuan atau jumlah pemakaian tidak sah.")
        };
        if (unit == MstEquipmentChargeUnit.PerUse) return raw;
        return rounding == MstEquipmentRoundingRule.Proportional
            ? (raw == 0 ? 0 : Math.Max(0.01m, decimal.Round(raw, 2, MidpointRounding.AwayFromZero))) : decimal.Ceiling(raw);
    }

    public async Task<NursingResult<List<EquipmentUsageResponse>>> ListAsync(Guid episodeId, CliEquipmentUsageStatus? status, CancellationToken ct)
    {
        var rows = await db.Set<CliEquipmentUsage>().AsNoTracking().Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && (!status.HasValue || x.Status == status))
            .OrderByDescending(x => x.StartedAt).ToListAsync(ct);
        var result = new List<EquipmentUsageResponse>();
        foreach (var row in rows) result.Add(await MapAsync(row, ct));
        return NursingResult<List<EquipmentUsageResponse>>.Ok(result, "Riwayat pemakaian alat berhasil diambil.");
    }

    public async Task<NursingResult<EquipmentUsageResponse>> StartAsync(StartEquipmentUsageRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var episode = await LockEpisodeAsync(request.EpisodeId, ct);
        if (episode == null) return Fail(404, "Episode tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(episode.Id, user, actor, ct);
        if (!access.IsSuccess) return NursingResult<EquipmentUsageResponse>.Fail(access.StatusCode, access.ErrorMessage!, access.StatusCode == 422 ? "CLI-EQP-003" : access.ErrorCode);
        if (episode.PhysicallyLeftAt != null) return Fail(422, "Pasien sudah keluar ruangan.", "CLI-EQP-003");
        if (!await clinical.IsDoctorAssignedAsync(episode.Id, request.ResponsibleDoctorId, DateTime.UtcNow, ct)) return Fail(403, "Dokter penanggung jawab tidak berpenugasan aktif.");
        var equipment = await db.MstMedicalEquipments.FromSqlInterpolated($"SELECT * FROM public.\"MstMedicalEquipment\" WHERE \"Id\" = {request.MedicalEquipmentId} FOR UPDATE").FirstOrDefaultAsync(ct);
        if (equipment == null || equipment.IsDelete || !equipment.IsActive) return Fail(400, "Jenis alat tidak aktif.");
        if (request.StartedAt == default || request.StartedAt > DateTime.UtcNow || request.StartedAt < episode.AdmittedAt ||
            (equipment.ChargeUnit == MstEquipmentChargeUnit.PerUse && !(request.Quantity > 0))) return Fail(400, "Waktu mulai atau jumlah pemakaian tidak sah.");
        var row = new CliEquipmentUsage { InpEpisodeId = episode.Id, EncounterId = episode.EncounterId, PatientId = episode.PatientId,
            MedicalEquipmentId = equipment.Id, ResponsibleDoctorId = request.ResponsibleDoctorId, PerformedByUserId = actor,
            StartedAt = request.StartedAt, Quantity = request.Quantity, ChargeUnitSnapshot = equipment.ChargeUnit,
            RoundingRuleSnapshot = equipment.RoundingRule, Note = request.Note?.Trim(), CreateBy = actor };
        db.Add(row); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        await LogAsync("Start", row, actor);
        return NursingResult<EquipmentUsageResponse>.Ok(await MapAsync(row, ct), "Pemakaian alat dimulai.", 201);
    }

    public async Task<NursingResult<EquipmentUsageResponse>> FinishAsync(Guid id, FinishEquipmentUsageRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var row = await LockAsync(id, ct);
        if (row == null) return Fail(404, "Pemakaian alat tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct);
        if (!access.IsSuccess) return access.Cast<EquipmentUsageResponse>();
        if (row.Version != request.ExpectedVersion) return Fail(409, "Versi berubah. Muat ulang pemakaian.");
        if (row.Status != CliEquipmentUsageStatus.Running) return Fail(409, "Pemakaian sudah berakhir.");
        var episode = await db.Set<InpEpisode>().AsNoTracking().FirstAsync(x => x.Id == row.InpEpisodeId, ct);
        if (request.EndedAt == default || request.EndedAt < row.StartedAt || request.EndedAt > DateTime.UtcNow || request.EndedAt > episode.PhysicallyLeftAt)
            return Fail(422, "Waktu selesai tidak sah.", "CLI-EQP-001");
        row.Quantity = request.Quantity ?? row.Quantity;
        if (row.ChargeUnitSnapshot == MstEquipmentChargeUnit.PerUse && !(row.Quantity > 0)) return Fail(400, "Jumlah pemakaian wajib diisi.");
        Complete(row, request.EndedAt, actor, false);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await tx.DisposeAsync();
        await EmitAsync(row, actor, ct); await LogAsync("Finish", row, actor);
        return NursingResult<EquipmentUsageResponse>.Ok(await MapAsync(row, ct), "Pemakaian alat selesai.");
    }

    public async Task<NursingResult<EquipmentUsageResponse>> CancelAsync(Guid id, CancelEquipmentUsageRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var row = await LockAsync(id, ct);
        if (row == null) return Fail(404, "Pemakaian alat tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct);
        if (!access.IsSuccess) return access.Cast<EquipmentUsageResponse>();
        if (row.Version != request.ExpectedVersion) return Fail(409, "Versi berubah.");
        if (row.Status == CliEquipmentUsageStatus.Cancelled) return Fail(409, "Pemakaian sudah dibatalkan.");
        if (string.IsNullOrWhiteSpace(request.Reason) || !request.Reason.Any(char.IsLetterOrDigit)) return Fail(400, "Alasan pembatalan wajib diisi.");
        var billing = await clearance.GetStatusAsync(row.EncounterId, ct);
        if (!billing.IsReadable || billing.InvoiceStatus != "OPEN") return Fail(422, "Pembatalan hanya dapat dilakukan selama invoice OPEN.", "CLI-EQP-002");
        row.Status = CliEquipmentUsageStatus.Cancelled; row.CancelReason = request.Reason.Trim(); row.CancelBy = actor; row.CancelDateTime = DateTime.UtcNow; row.IsCancel = true;
        Touch(row, actor); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await tx.DisposeAsync();
        await EmitAsync(row, actor, ct); await LogAsync("Cancel", row, actor);
        return NursingResult<EquipmentUsageResponse>.Ok(await MapAsync(row, ct), "Pemakaian alat dibatalkan.");
    }

    public async Task<NursingResult<EquipmentUsageResponse>> CorrectAsync(Guid id, CorrectEquipmentUsageRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var row = await LockAsync(id, ct);
        if (row == null) return Fail(404, "Pemakaian alat tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct);
        if (!access.IsSuccess) return access.Cast<EquipmentUsageResponse>();
        if (row.Version != request.ExpectedVersion) return Fail(409, "Versi berubah.");
        if (row.Status == CliEquipmentUsageStatus.Cancelled) return Fail(409, "Pemakaian dibatalkan tidak dapat dikoreksi.");
        if (string.IsNullOrWhiteSpace(request.Reason) || !request.Reason.Any(char.IsLetterOrDigit)) return Fail(400, "Alasan koreksi wajib diisi.");
        var billing = await clearance.GetStatusAsync(row.EncounterId, ct);
        if (!billing.IsReadable || billing.InvoiceStatus != "OPEN") return Fail(422, "Koreksi hanya dapat dilakukan selama invoice OPEN.", "CLI-EQP-002");
        var episode = await db.Set<InpEpisode>().AsNoTracking().FirstAsync(x => x.Id == row.InpEpisodeId, ct);
        var end = request.EndedAt ?? row.EndedAt;
        if (request.StartedAt == default || request.StartedAt < episode.AdmittedAt || request.StartedAt > DateTime.UtcNow ||
            end < request.StartedAt || end > DateTime.UtcNow || end > episode.PhysicallyLeftAt ||
            (row.Status == CliEquipmentUsageStatus.Running && request.EndedAt.HasValue)) return Fail(422, "Waktu koreksi tidak sah.", "CLI-EQP-001");
        db.Add(new CliEquipmentUsageRevision { EquipmentUsageId = row.Id, RevisionNumber = row.RevisionNumber,
            PreviousStartedAt = row.StartedAt, PreviousEndedAt = row.EndedAt, PreviousQuantity = row.Quantity, PreviousBilledUnits = row.BilledUnits,
            Reason = request.Reason.Trim(), RevisedByUserId = actor, RevisedAt = DateTime.UtcNow, CreateBy = actor });
        row.StartedAt = request.StartedAt; row.EndedAt = end; row.RevisionNumber++; row.RequiresNurseReview = false;
        if (end.HasValue) row.BilledUnits = CalculateBilledUnits(row.ChargeUnitSnapshot, row.StartedAt, end.Value, row.Quantity, row.RoundingRuleSnapshot);
        Touch(row, actor); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await tx.DisposeAsync();
        if (row.Status == CliEquipmentUsageStatus.Completed) await EmitAsync(row, actor, ct);
        await LogAsync("Correct", row, actor);
        return NursingResult<EquipmentUsageResponse>.Ok(await MapAsync(row, ct), "Koreksi pemakaian alat tersimpan.");
    }

    // Caller invokes this in a separate scope after the departure transaction commits.
    public async Task<int> CloseRunningForDepartureAsync(Guid episodeId, DateTime departedAt, Guid actor, CancellationToken ct)
    {
        var ids = await db.Set<CliEquipmentUsage>().AsNoTracking().Where(x => x.InpEpisodeId == episodeId && x.Status == CliEquipmentUsageStatus.Running && !x.IsDelete).Select(x => x.Id).ToListAsync(ct);
        var count = 0;
        foreach (var id in ids)
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var row = await LockAsync(id, ct);
            if (row == null || row.Status != CliEquipmentUsageStatus.Running) continue;
            if (departedAt < row.StartedAt) throw new InvalidOperationException("Waktu keluar mendahului pemakaian alat.");
            Complete(row, departedAt, actor, true); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await tx.DisposeAsync();
            count++; await EmitAsync(row, actor, ct); await LogAsync("AutoClose", row, actor);
        }
        return count;
    }

    public async Task RepairDepartureAndDeliveryAsync(CancellationToken ct)
    {
        var episodes = await (from e in db.Set<InpEpisode>().AsNoTracking() join u in db.Set<CliEquipmentUsage>().AsNoTracking() on e.Id equals u.InpEpisodeId
            where e.PhysicallyLeftAt != null && u.Status == CliEquipmentUsageStatus.Running && !u.IsDelete
            select new { e.Id, e.PhysicallyLeftAt, e.PhysicallyLeftByUserId }).Distinct().Take(100).ToListAsync(ct);
        foreach (var e in episodes) await CloseRunningForDepartureAsync(e.Id, e.PhysicallyLeftAt!.Value, e.PhysicallyLeftByUserId ?? Guid.Empty, ct);
        var pending = await db.Set<CliEquipmentUsage>().AsNoTracking().Where(x => !x.IsDelete && x.Status != CliEquipmentUsageStatus.Running &&
            !db.Set<CliClinicalMilestoneFact>().Any(f => f.SourceContext == "EQUIPMENT_USAGE" && f.SourceAggregateId == x.Id && f.CreateDateTime >= (x.UpdateDateTime ?? x.CreateDateTime)))
            .OrderBy(x => x.UpdateDateTime).Take(100).ToListAsync(ct);
        foreach (var row in pending) await EmitAsync(row, row.UpdateBy, ct);
    }

    private async Task EmitAsync(CliEquipmentUsage row, Guid actor, CancellationToken ct)
    {
        var fact = new ClinicalMilestoneFactRequest { SourceContext = "EQUIPMENT_USAGE", SourceAggregateId = row.Id, EncounterId = row.EncounterId,
            EffectType = "EquipmentUsageCharge", OccurredAt = row.EndedAt ?? row.CancelDateTime ?? DateTime.UtcNow,
            Quantity = row.BilledUnits ?? row.Quantity ?? 0,
            Unit = row.ChargeUnitSnapshot switch { MstEquipmentChargeUnit.PerUse => "PER_USE", MstEquipmentChargeUnit.PerHour => "PER_HOUR", _ => "PER_DAY" },
            RuleSnapshot = JsonSerializer.Serialize(new { row.Version, row.RevisionNumber, row.MedicalEquipmentId, row.StartedAt, row.EndedAt, row.ChargeUnitSnapshot, row.RoundingRuleSnapshot, row.Status }) };
        try
        {
            if (row.Status == CliEquipmentUsageStatus.Cancelled) await facts.EmitClinicalCancellationAsync(fact, actor, ct);
            else await facts.EmitChargeEligibilityAsync(fact, actor, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { await logger.WarningAsync("ClinicalManagement.EquipmentUsage", "BillingDelivery", "Pemakaian tersimpan; penyerahan Billing akan diulang.", new { row.Id, ErrorType = ex.GetType().Name }); }
    }
    private static void Touch(CliEquipmentUsage row, Guid actor) { row.Version++; row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow; }
    private static void Complete(CliEquipmentUsage row, DateTime end, Guid actor, bool auto)
    {
        row.EndedAt = end; row.BilledUnits = CalculateBilledUnits(row.ChargeUnitSnapshot, row.StartedAt, end, row.Quantity, row.RoundingRuleSnapshot);
        row.Status = CliEquipmentUsageStatus.Completed; row.RequiresNurseReview = auto; row.AutoClosedAt = auto ? DateTime.UtcNow : null; Touch(row, actor);
    }
    private Task<InpEpisode?> LockEpisodeAsync(Guid id, CancellationToken ct) => db.Set<InpEpisode>().FromSqlInterpolated($"SELECT * FROM public.\"InpEpisode\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(ct);
    private Task<CliEquipmentUsage?> LockAsync(Guid id, CancellationToken ct) => db.Set<CliEquipmentUsage>().FromSqlInterpolated($"SELECT * FROM public.\"CliEquipmentUsage\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(ct);
    private Task LogAsync(string action, CliEquipmentUsage row, Guid actor) => logger.InfoAsync("ClinicalManagement.EquipmentUsage", action, "Pemakaian alat berubah.", new { row.Id, ActorUserId = actor, row.Version });
    private static NursingResult<EquipmentUsageResponse> Fail(int status, string message, string? code = null) => NursingResult<EquipmentUsageResponse>.Fail(status, message, code);
    private async Task<EquipmentUsageResponse> MapAsync(CliEquipmentUsage row, CancellationToken ct)
    {
        return new() { Id = row.Id, MedicalEquipmentId = row.MedicalEquipmentId, EquipmentName = await db.MstMedicalEquipments.Where(x => x.Id == row.MedicalEquipmentId).Select(x => x.EquipmentName).FirstOrDefaultAsync(ct) ?? "",
            ResponsibleDoctorName = await db.Set<MstDoctor>().Where(x => x.Id == row.ResponsibleDoctorId).Select(x => x.FullName).FirstOrDefaultAsync(ct),
            PerformedByName = await db.Users.Where(x => x.Id == row.PerformedByUserId).Select(x => x.DisplayName).FirstOrDefaultAsync(ct),
            ChargeUnit = row.ChargeUnitSnapshot, StartedAt = row.StartedAt, EndedAt = row.EndedAt, Quantity = row.Quantity, BilledUnits = row.BilledUnits,
            Status = row.Status, RequiresNurseReview = row.RequiresNurseReview, Version = row.Version,
            ChargeState = row.Status == CliEquipmentUsageStatus.Cancelled ? "VOIDED" : await bridge.GetEquipmentChargeStateAsync(row.Id, ct) };
    }
}
