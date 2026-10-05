using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;

public partial class BbkBloodOrderService
{
    private const string ClinicalNoteAction = "ClinicalNote";

    public async Task<PagedResult<BloodOrderVerificationItem>> GetInstructionVerificationWorklistAsync(
        BloodInstructionVerificationQuery request, Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var doctorId = await _clinicalContext.ResolveActorDoctorIdAsync(
            _httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);
        if (!doctorId.HasValue)
            throw new UnauthorizedAccessException("Akun pengguna tidak tertaut ke dokter aktif.");
        var query = DetailQuery().Where(x => x.RequestingDoctorId == doctorId && x.OrderStatus != BbkBloodOrderStatus.Cancelled &&
            x.InstructionVerificationStatus == BbkInstructionVerificationStatus.Pending);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.OrderNumber.ToLower().Contains(term) ||
                (x.Patient != null && x.Patient.FullName.ToLower().Contains(term)));
        }
        var (page, size) = NormalizePaging(request.PageNumber, request.PageSize);
        var total = await query.CountAsync(cancellationToken);
        var orders = await query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.Id)
            .Skip((page - 1) * size).Take(size).Select(x => new BloodOrderVerificationItem
            {
                Id = x.Id, OrderNumber = x.OrderNumber, PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : null,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                EncounterId = x.EncounterId, RequestingDoctorId = x.RequestingDoctorId,
                RequestingDoctorName = x.RequestingDoctor != null ? x.RequestingDoctor.FullName : null,
                InputByUserId = x.InputByUserId, InstructionVerificationStatus = x.InstructionVerificationStatus,
                OrderStatus = x.OrderStatus, CreateDateTime = x.CreateDateTime, Version = x.Version
            }).ToListAsync(cancellationToken);
        return new PagedResult<BloodOrderVerificationItem>
        {
            PageNumber = page, PageSize = size, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)size),
            Items = orders
        };
    }

    public async Task<BloodOrderResult> VerifyInstructionAsync(Guid id,
        VerifyBloodInstructionRequest request, Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var order = await TrackedAsync(id, cancellationToken);
        if (order == null) return Failed(BloodOrderOutcome.NotFound, NotFoundMessage);
        var doctorId = await _clinicalContext.ResolveActorDoctorIdAsync(
            _httpContextAccessor.HttpContext?.User, actorUserId, cancellationToken);
        if (actorUserId == Guid.Empty || !doctorId.HasValue || doctorId != order.RequestingDoctorId)
            return Failed(BloodOrderOutcome.Forbidden,
                "Hanya dokter yang memberi instruksi yang dapat memverifikasi pesanan ini.");
        if (order.InstructionVerificationStatus != BbkInstructionVerificationStatus.Pending)
            return Failed(BloodOrderOutcome.VersionConflict, "Pesanan ini sudah diverifikasi.");
        if (order.OrderStatus == BbkBloodOrderStatus.Cancelled)
            return Failed(BloodOrderOutcome.VersionConflict, "Order yang dibatalkan tidak dapat diverifikasi.");
        if (order.Version != request.ExpectedVersion)
            return Failed(BloodOrderOutcome.VersionConflict, ConcurrencyMessage);
        var now = DateTime.UtcNow;
        order.InstructionVerificationStatus = BbkInstructionVerificationStatus.Verified;
        order.InstructionVerifiedAt = now;
        order.InstructionVerifiedByUserId = actorUserId;
        order.UpdateBy = actorUserId;
        order.UpdateDateTime = now;
        order.Version++;
        _dbContext.Set<BbkTransitionHistory>().Add(new BbkTransitionHistory
        {
            Id = Guid.NewGuid(), Scope = BbkTransitionScopes.BloodOrder, EntityId = order.Id,
            Action = "VerifyInstruction", FromStatus = "Pending", ToStatus = "Verified",
            ActorUserId = actorUserId, OccurredAt = now, CreateBy = actorUserId,
            CreateDateTime = now
        });
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            _dbContext.ChangeTracker.Clear();
            return Failed(BloodOrderOutcome.VersionConflict, ConcurrencyMessage);
        }
        return Succeeded(order, "Instruksi pesanan darah berhasil diverifikasi.");
    }

    private static Guid IdempotentOrderId(Guid actorUserId, Guid encounterId, string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(
            $"BbkBloodOrder:{actorUserId:N}:{encounterId:N}:{key.Trim()}"));
        return new Guid(hash.AsSpan(0, 16));
    }

    // Lock milik transaksi PostgreSQL: pengiriman ulang serentak menunggu order pertama.
    private Task LockIdempotentOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var lockKey = BitConverter.ToInt64(orderId.ToByteArray(), 0);
        return _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({lockKey})", cancellationToken);
    }

    private static bool SameOrderRequest(BbkBloodOrder order, CreateBloodOrderRequest request,
        Guid actorUserId, BbkOrderSource source)
        => !order.IsDelete && order.InputByUserId == actorUserId &&
            order.PatientId == request.PatientId && order.EncounterId == request.EncounterId &&
            order.ServiceUnitId == request.ServiceUnitId &&
            order.RequestingDoctorId == request.RequestingDoctorId &&
            order.RequestedBloodGroup == request.RequestedBloodGroup && order.OrderSource == source &&
            order.Lines.Where(x => !x.IsDelete).OrderBy(x => x.Sequence)
                .Select(x => (x.BloodComponentId, x.RequestedQuantity))
                .SequenceEqual(request.Lines.Select(x => (x.BloodComponentId, x.RequestedQuantity)));

    private async Task<bool> SameClinicalNoteAsync(Guid orderId, string? note,
        CancellationToken cancellationToken)
    {
        var stored = await _dbContext.Set<BbkTransitionHistory>().AsNoTracking()
            .Where(x => x.Scope == BbkTransitionScopes.BloodOrder && x.EntityId == orderId &&
                x.Action == ClinicalNoteAction).Select(x => x.ReasonNote)
            .FirstOrDefaultAsync(cancellationToken);
        return string.Equals(stored ?? string.Empty, note?.Trim() ?? string.Empty,
            StringComparison.Ordinal);
    }

    private async Task<bool> SameDuplicateReasonAsync(Guid orderId, string? reason,
        CancellationToken cancellationToken)
    {
        var stored = await _dbContext.Set<BbkTransitionHistory>().AsNoTracking()
            .Where(x => x.Scope == BbkTransitionScopes.BloodOrder && x.EntityId == orderId &&
                x.Action == ConfirmedDuplicateAction).Select(x => x.ReasonNote)
            .FirstOrDefaultAsync(cancellationToken);
        return string.Equals(stored ?? string.Empty, reason?.Trim() ?? string.Empty,
            StringComparison.Ordinal);
    }
}
