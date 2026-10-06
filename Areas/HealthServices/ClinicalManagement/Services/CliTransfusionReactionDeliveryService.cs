using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
public class CliTransfusionReactionDeliveryService(ApplicationDbContext db, IServiceScopeFactory scopes, ILogger<CliTransfusionReactionDeliveryService> logger)
{
    public async Task DeliverAsync(Guid id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var row = await db.Set<CliTransfusionReaction>().FromSqlInterpolated($"SELECT * FROM public.\"CliTransfusionReaction\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(ct);
        if (row == null || row.NoticeDelivery == CliReactionNoticeDelivery.Delivered) return;
        var monitoring = await db.Set<CliTransfusionMonitoring>().AsNoTracking().FirstAsync(x => x.Id == row.MonitoringId, ct);
        var unit = await db.Set<InpEpisode>().Where(x => x.Id == monitoring.InpEpisodeId).Select(x => (Guid?)x.ServiceUnitId).FirstAsync(ct);
        row.NoticeAttemptCount++;
        try
        {
            using var scope = scopes.CreateScope();
            row.BloodBankNoticeId = await scope.ServiceProvider.GetRequiredService<BbkTransfusionReactionNoticeService>().ReceiveAsync(new ReceiveTransfusionReactionNotice { ClinicalReactionId = row.Id,
                BloodUnitId = monitoring.BloodUnitId, PatientId = monitoring.PatientId, EncounterId = monitoring.EncounterId, ServiceUnitId = unit, ReactionSummary = row.ReactionSummary, OccurredAt = row.OccurredAt, RecordedByUserId = row.RecordedByUserId }, ct);
            row.NoticeDelivery = CliReactionNoticeDelivery.Delivered;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { row.NoticeDelivery = CliReactionNoticeDelivery.Failed; logger.LogWarning(ex, "Pengiriman reaksi {ReactionId} gagal; catatan klinis tetap tersimpan dan akan dikirim ulang.", id); }
        row.UpdateBy = row.RecordedByUserId; row.UpdateDateTime = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
}
