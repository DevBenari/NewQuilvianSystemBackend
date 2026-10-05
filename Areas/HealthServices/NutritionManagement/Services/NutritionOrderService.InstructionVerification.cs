using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

public sealed partial class NutritionOrderService
{
    private async Task<GziInstructionVerificationStatus> ResolveInstructionStatusAsync(
        Guid doctorId, Guid actorUserId, GziInstructionVerificationStatus? requested,
        CancellationToken cancellationToken)
    {
        if (!requested.HasValue) return GziInstructionVerificationStatus.NotRequired;
        if (!Enum.IsDefined(requested.Value) || requested == GziInstructionVerificationStatus.Verified)
            throw new NutritionUnprocessableException("VAL-RWF-64",
                "Status terverifikasi hanya dapat diberikan melalui aksi verifikasi.");
        var actorDoctor = await _clinicalContext.ResolveActorDoctorIdAsync(
            _httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);
        return actorDoctor == doctorId ? GziInstructionVerificationStatus.NotRequired
            : GziInstructionVerificationStatus.Pending;
    }

    public async Task<PagedResult<NutritionOrderVerificationItem>> GetInstructionVerificationWorklistAsync(
        GziOrderPagedQuery request, CancellationToken cancellationToken = default)
    {
        var doctorId = await _clinicalContext.ResolveActorDoctorIdAsync(
            _httpContextAccessor.HttpContext?.User, GetCurrentUserId(), cancellationToken);
        if (!doctorId.HasValue)
            throw new NutritionForbiddenException("Akun pengguna tidak tertaut ke dokter aktif.");
        var query = _dbContext.GziNutritionOrders.AsNoTracking()
            .Include(x => x.Patient).Include(x => x.RequesterDoctor)
            .Where(x => !x.IsDelete && !x.IsCancel && x.Status != GziOrderStatus.Cancelled && x.RequesterDoctorId == doctorId &&
                x.InstructionVerificationStatus == GziInstructionVerificationStatus.Pending);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.OrderNumber.ToLower().Contains(term) ||
                (x.Patient != null && x.Patient.FullName.ToLower().Contains(term)));
        }
        var page = Math.Max(1, request.PageNumber);
        var size = Math.Clamp(request.PageSize, 1, 100);
        var total = await query.CountAsync(cancellationToken);
        var orders = await query.OrderBy(x => x.RequestedAt).ThenBy(x => x.Id)
            .Skip((page - 1) * size).Take(size).Select(x => new NutritionOrderVerificationItem
            {
                Id = x.Id, OrderNumber = x.OrderNumber, PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                EncounterId = x.EncounterId, RequesterDoctorId = x.RequesterDoctorId,
                RequesterDoctorName = x.RequesterDoctor != null ? x.RequesterDoctor.FullName : string.Empty,
                InstructionVerificationStatus = x.InstructionVerificationStatus,
                Status = x.Status, RequestedAt = x.RequestedAt, Version = x.Version
            }).ToListAsync(cancellationToken);
        return new PagedResult<NutritionOrderVerificationItem>
        {
            PageNumber = page, PageSize = size, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)size),
            Items = orders
        };
    }

    public async Task<GziOrderDetailResponse> VerifyInstructionAsync(Guid id,
        VerifyNutritionInstructionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await LoadOrderAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Order konsultasi gizi tidak ditemukan.");
        var actor = GetCurrentUserId();
        var doctorId = await _clinicalContext.ResolveActorDoctorIdAsync(
            _httpContextAccessor.HttpContext?.User, actor, cancellationToken);
        if (!doctorId.HasValue || doctorId != entity.RequesterDoctorId)
            throw new NutritionForbiddenException(
                "Hanya dokter yang memberi instruksi yang dapat memverifikasi pesanan ini.");
        if (entity.InstructionVerificationStatus != GziInstructionVerificationStatus.Pending)
            throw new NutritionConflictException("VAL-RWF-64", "Pesanan ini sudah diverifikasi.");
        if (entity.IsCancel || entity.Status == GziOrderStatus.Cancelled)
            throw new NutritionConflictException("GIZ004", "Order yang dibatalkan tidak dapat diverifikasi.");
        EnsureVersion(entity.Version, request.ExpectedVersion);
        entity.InstructionVerificationStatus = GziInstructionVerificationStatus.Verified;
        entity.InstructionVerifiedAt = DateTime.UtcNow;
        entity.InstructionVerifiedByUserId = actor;
        entity.UpdateBy = actor;
        entity.UpdateDateTime = entity.InstructionVerifiedAt;
        entity.Version++;
        _dbContext.GziNutritionOrderHistories.Add(NewHistory(entity.Id, entity.Status,
            entity.Status, "VerifyInstruction", "Pending → Verified", $"VERIFY:{entity.Id:N}:{request.ExpectedVersion}",
            Hash($"{entity.Id:N}|{request.ExpectedVersion}|VerifyInstruction"), actor,
            entity.InstructionVerifiedAt.Value));
        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "NutritionOrder.VerifyInstruction",
            "Dokter peminta memverifikasi instruksi konsultasi gizi.",
            new { entity.Id, ActorUserId = actor, entity.RequesterDoctorId,
                FromStatus = GziInstructionVerificationStatus.Pending, entity.InstructionVerificationStatus });
        return MapDetail(entity);
    }
}
