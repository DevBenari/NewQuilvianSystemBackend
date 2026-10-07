using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public class InpDietOrderAdapter(ApplicationDbContext db, InpatientClinicalContextService clinical, NutritionDietService diets, NursingEpisodeWriteGuard guard)
{
    private async Task<(InpEpisode Episode, Guid WorkforceId, bool IsSelf)> ContextAsync(Guid episodeId, Guid? instructed, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        var ep = await db.Set<InpEpisode>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete && !x.IsCancel, ct)
            ?? throw new InpAncillaryOrderException(404, "Episode tidak ditemukan.");
        if (NursingEpisodeWriteGuard.EpisodeRejection(ep.EpisodeStatus) != null || ep.PhysicallyLeftAt.HasValue) throw new InpAncillaryOrderException(422, "Episode tidak lagi menerima diet.");
        var ownDoctor = await clinical.ResolveActorDoctorIdAsync(user, actor, ct);
        var doctor = instructed ?? ownDoctor;
        if (!doctor.HasValue || doctor == Guid.Empty) throw new InpAncillaryOrderException(400, "Dokter pemberi instruksi wajib dipilih");
        if (!await clinical.IsDoctorAssignedAsync(episodeId, doctor.Value, DateTime.UtcNow, ct)) throw new InpAncillaryOrderException(403, "Dokter tidak berpenugasan aktif.");
        if (!ownDoctor.HasValue) { var access = await guard.EnsureCanWriteAsync(episodeId, user, actor, ct); if (!access.IsSuccess) throw new InpAncillaryOrderException(access.StatusCode, access.ErrorMessage!); }
        var workforce = await db.Set<MstDoctor>().Where(x => x.Id == doctor).Select(x => x.WorkforceProfileId).FirstAsync(ct);
        return (ep, workforce, ownDoctor == doctor);
    }
    public async Task<GziPatientDietResponse> PrescribeAsync(Guid episodeId, PrescribeInpatientDietRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        var ctx = await ContextAsync(episodeId, request.InstructingDoctorId, user, actor, ct);
        return await diets.PrescribeAsync(new PrescribeGzDietRequest { EncounterId = ctx.Episode.EncounterId, PatientId = ctx.Episode.PatientId,
            DietTypeId = request.DietTypeId, FoodFormId = request.FoodFormId, EnergyRequirementKcal = request.EnergyRequirementKcal, Instruction = request.Instruction,
            EffectiveStartAt = request.EffectiveStartAt, ChangeReason = request.ChangeReason, IdempotencyKey = request.IdempotencyKey, PrescribedByWorkforceId = ctx.WorkforceId,
            InstructionVerificationStatus = ctx.IsSelf ? GziInstructionVerificationStatus.NotRequired : GziInstructionVerificationStatus.Pending }, ct);
    }
    public async Task<GziPatientDietResponse> StopAsync(Guid episodeId, Guid dietId, StopInpatientDietRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        var ctx = await ContextAsync(episodeId, request.InstructingDoctorId, user, actor, ct);
        if (!await db.GziPatientDiets.AnyAsync(x => x.Id == dietId && x.EncounterId == ctx.Episode.EncounterId && !x.IsDelete, ct)) throw new InpAncillaryOrderException(404, "Diet bukan milik episode ini.");
        return await diets.StopAsync(dietId, request, ct, ctx.WorkforceId, ctx.IsSelf ? GziInstructionVerificationStatus.NotRequired : GziInstructionVerificationStatus.Pending);
    }
}
