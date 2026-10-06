using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

public sealed partial class NutritionDietService
{
    private async Task<GziInstructionVerificationStatus> ResolveDietInstructionStatusAsync(PrescribeGzDietRequest request, Guid actor, CancellationToken ct)
    {
        if (!request.InstructionVerificationStatus.HasValue) return GziInstructionVerificationStatus.NotRequired;
        if (!Enum.IsDefined(request.InstructionVerificationStatus.Value) || request.InstructionVerificationStatus == GziInstructionVerificationStatus.Verified)
            throw new NutritionUnprocessableException("GIZ-VER-001", "Status terverifikasi hanya diberikan melalui aksi verifikasi.");
        var account = await _dbContext.Users.Where(x => x.Id == actor).Select(x => new { x.WorkforceProfileId, x.DoctorId }).FirstOrDefaultAsync(ct);
        var own = account?.WorkforceProfileId;
        if (!own.HasValue && account?.DoctorId != null) own = await _dbContext.Set<MstDoctor>().Where(x => x.Id == account.DoctorId && !x.IsDelete).Select(x => x.WorkforceProfileId).FirstOrDefaultAsync(ct);
        return own == request.PrescribedByWorkforceId ? GziInstructionVerificationStatus.NotRequired : GziInstructionVerificationStatus.Pending;
    }
    public async Task<PagedResult<GziPatientDietResponse>> GetDietVerificationWorklistAsync(GziNutritionPatientQuery request, InpatientClinicalContextService clinical, CancellationToken ct)
    {
        var doctor = await clinical.ResolveActorDoctorIdAsync(_httpContextAccessor.HttpContext?.User, GetCurrentUserId(), ct);
        if (!doctor.HasValue) throw new NutritionForbiddenException("Akun tidak tertaut dokter aktif.");
        var workforce = await _dbContext.Set<MstDoctor>().Where(x => x.Id == doctor).Select(x => x.WorkforceProfileId).FirstAsync(ct);
        var query = BuildDietQuery().Where(x => x.PrescribedByWorkforceId == workforce && x.InstructionVerificationStatus == GziInstructionVerificationStatus.Pending);
        if (!string.IsNullOrWhiteSpace(request.Search)) { var term = request.Search.Trim().ToLower(); query = query.Where(x => x.Patient != null && x.Patient.FullName.ToLower().Contains(term)); }
        var page = Math.Max(1, request.PageNumber); var size = Math.Clamp(request.PageSize, 1, 100); var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.StartAt).ThenBy(x => x.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return new() { PageNumber = page, PageSize = size, TotalData = total, TotalPage = (int)Math.Ceiling(total / (double)size), Items = rows.Select(MapDiet).ToList() };
    }
    public async Task<NursingResult<GziPatientDietResponse>> VerifyDietInstructionAsync(Guid id, VerifyNutritionInstructionRequest request, InpatientClinicalContextService clinical, CancellationToken ct)
    {
        var actor = GetCurrentUserId();
        await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);
        var row = await _dbContext.GziPatientDiets.FromSqlInterpolated($"SELECT * FROM public.\"GziPatientDiet\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(ct);
        if (row == null) return NursingResult<GziPatientDietResponse>.Fail(404, "Diet tidak ditemukan.");
        var doctor = await clinical.ResolveActorDoctorIdAsync(_httpContextAccessor.HttpContext?.User, actor, ct);
        if (!doctor.HasValue || !await _dbContext.Set<MstDoctor>().AnyAsync(x => x.Id == doctor && x.WorkforceProfileId == row.PrescribedByWorkforceId, ct))
            return NursingResult<GziPatientDietResponse>.Fail(403, "Hanya dokter penetap yang dapat memverifikasi diet.", "GIZ-VER-001");
        if (row.InstructionVerificationStatus != GziInstructionVerificationStatus.Pending) return NursingResult<GziPatientDietResponse>.Fail(409, "Diet sudah diverifikasi atau tidak memerlukan verifikasi.", "GIZ-VER-002");
        if (row.Version != request.ExpectedVersion) return NursingResult<GziPatientDietResponse>.Fail(409, "Versi berubah. Muat ulang diet.", "GIZ012");
        row.InstructionVerificationStatus = GziInstructionVerificationStatus.Verified; row.InstructionVerifiedAt = DateTime.UtcNow;
        row.InstructionVerifiedByUserId = actor; row.UpdateBy = actor; row.UpdateDateTime = row.InstructionVerifiedAt; row.Version++;
        await SaveAsync(ct); await tx.CommitAsync(ct);
        await _loggerService.InfoAsync(LogCategory, "Diet.VerifyInstruction", "Dokter memverifikasi instruksi diet.", new { row.Id, ActorUserId = actor });
        return NursingResult<GziPatientDietResponse>.Ok(MapDiet(await BuildDietQuery().FirstAsync(x => x.Id == id, ct)), "Diet terverifikasi.");
    }
}
