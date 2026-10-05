using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
public class BbkTransfusionReactionNoticeService(ApplicationDbContext db, LoggerService logger)
{
    public async Task<Guid> ReceiveAsync(ReceiveTransfusionReactionNotice request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"REACTION_NOTICE:" + request.ClinicalReactionId}, 0))", ct);
        var existing = await db.Set<BbkTransfusionReactionNotice>().FirstOrDefaultAsync(x => x.ClinicalReactionId == request.ClinicalReactionId, ct);
        if (existing != null) return existing.Id;
        if (request.ClinicalReactionId == Guid.Empty || string.IsNullOrWhiteSpace(request.ReactionSummary) || !await db.Set<BbkBloodUnit>().AnyAsync(x => x.Id == request.BloodUnitId && x.IssuedToPatientId == request.PatientId, ct))
            throw new InvalidOperationException("Konteks pasien atau kantong pemberitahuan reaksi tidak sah.");
        var row = new BbkTransfusionReactionNotice { ClinicalReactionId = request.ClinicalReactionId, BloodUnitId = request.BloodUnitId, PatientId = request.PatientId, EncounterId = request.EncounterId,
            ServiceUnitId = request.ServiceUnitId, ReactionSummarySnapshot = request.ReactionSummary, OccurredAt = request.OccurredAt, CreateBy = request.RecordedByUserId };
        db.Add(row); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        await logger.InfoAsync("BloodBank.TransfusionReactionNotice", "Receive", "Bank Darah menerima pemberitahuan reaksi.", new { row.Id, row.ClinicalReactionId, ActorUserId = request.RecordedByUserId });
        return row.Id;
    }
    private IQueryable<ReactionNoticeDetail> Query() => from n in db.Set<BbkTransfusionReactionNotice>().AsNoTracking()
        join p in db.Set<MstPatient>().AsNoTracking() on n.PatientId equals p.Id
        join b in db.Set<BbkBloodUnit>().AsNoTracking() on n.BloodUnitId equals b.Id
        where !n.IsDelete
        select new ReactionNoticeDetail { Id = n.Id, ClinicalReactionId = n.ClinicalReactionId, BloodUnitId = n.BloodUnitId, PatientId = n.PatientId, PatientName = p.FullName, PmiBagNumber = b.PmiBagNumber,
            EncounterId = n.EncounterId, ServiceUnitId = n.ServiceUnitId, ServiceUnitName = db.Set<MstServiceUnit>().Where(x => x.Id == n.ServiceUnitId).Select(x => x.ServiceUnitName).FirstOrDefault(),
            ReactionSummary = n.ReactionSummarySnapshot, OccurredAt = n.OccurredAt, ReceivedAt = n.ReceivedAt, Status = n.Status, AcknowledgedAt = n.AcknowledgedAt, AcknowledgedByUserId = n.AcknowledgedByUserId, AcknowledgeNote = n.AcknowledgeNote };
    public async Task<PagedResult<ReactionNoticeDetail>> ListAsync(ReactionNoticeQuery request, CancellationToken ct)
    {
        var q = Query(); if (request.Status.HasValue) q = q.Where(x => x.Status == request.Status); if (request.From.HasValue) q = q.Where(x => x.ReceivedAt >= request.From); if (request.To.HasValue) q = q.Where(x => x.ReceivedAt <= request.To);
        var page = Math.Max(1, request.PageNumber); var size = Math.Clamp(request.PageSize, 1, 100); var count = await q.CountAsync(ct);
        return new() { PageNumber = page, PageSize = size, TotalData = count, TotalPage = (int)Math.Ceiling(count / (double)size), Items = await q.OrderByDescending(x => x.ReceivedAt).ThenBy(x => x.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct) };
    }
    public async Task<NursingResult<ReactionNoticeDetail>> DetailAsync(Guid id, CancellationToken ct)
    { var row = await Query().FirstOrDefaultAsync(x => x.Id == id, ct); return row == null ? NursingResult<ReactionNoticeDetail>.Fail(404, "Pemberitahuan tidak ditemukan.") : NursingResult<ReactionNoticeDetail>.Ok(row, "Pemberitahuan reaksi berhasil diambil."); }
    public async Task<NursingResult<ReactionNoticeDetail>> AcknowledgeAsync(Guid id, AcknowledgeReactionNoticeRequest request, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var row = await db.Set<BbkTransfusionReactionNotice>().FromSqlInterpolated($"SELECT * FROM public.\"BbkTransfusionReactionNotice\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(ct);
        if (row == null) return NursingResult<ReactionNoticeDetail>.Fail(404, "Pemberitahuan tidak ditemukan.");
        if (row.Status == BbkReactionNoticeStatus.Acknowledged) return NursingResult<ReactionNoticeDetail>.Fail(409, "Pemberitahuan sudah ditindaklanjuti.");
        row.Status = BbkReactionNoticeStatus.Acknowledged; row.AcknowledgedAt = DateTime.UtcNow; row.AcknowledgedByUserId = actor; row.AcknowledgeNote = request.Note?.Trim(); row.UpdateBy = actor; row.UpdateDateTime = row.AcknowledgedAt;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await logger.InfoAsync("BloodBank.TransfusionReactionNotice", "Acknowledge", "Petugas menindaklanjuti reaksi transfusi.", new { row.Id, ActorUserId = actor });
        return await DetailAsync(id, ct);
    }
}
