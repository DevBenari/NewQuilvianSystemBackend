using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Siklus hidup pesanan laboratorium sesuai <c>RJ-BIL-GATE-DEC-003</c>.
    ///
    /// Pesanan tidak pernah menerbitkan fakta kelayakan tagih. Yang menerbitkan hanyalah
    /// penetapan layak pada tingkat sampel, karena kelayakan tagih dinilai per komponen
    /// pemeriksaan sesuai keputusan author <c>RJ-BIL-OQ-008</c>.
    /// </summary>
    public class LabOrderService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LabSpecimenService _labSpecimenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabOrderService(
            ApplicationDbContext dbContext,
            LabSpecimenService labSpecimenService,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _labSpecimenService = labSpecimenService;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<List<LabOrderListResponse>> GetListAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new LabOrderListResponse
                {
                    Id = x.Id,
                    EncounterId = x.EncounterId,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    OrderStatus = x.OrderStatus.ToString(),
                    SpecimenCount = x.Specimens.Count(s => !s.IsDelete),
                    AcceptedSpecimenCount = x.Specimens.Count(s =>
                        !s.IsDelete && s.SpecimenStatus == LabSpecimenStatus.Accepted),
                    IsCancel = x.IsCancel,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<LabOrderDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new LabOrderDetailResponse
                {
                    Id = x.Id,
                    EncounterId = x.EncounterId,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    OrderStatus = x.OrderStatus.ToString(),
                    SpecimenCount = x.Specimens.Count(s => !s.IsDelete),
                    AcceptedSpecimenCount = x.Specimens.Count(s =>
                        !s.IsDelete && s.SpecimenStatus == LabSpecimenStatus.Accepted),
                    IsCancel = x.IsCancel,
                    CreateDateTime = x.CreateDateTime,
                    RequestedAt = x.RequestedAt,
                    CompletedAt = x.CompletedAt,
                    StatusBeforeHold = x.StatusBeforeHold != null ? x.StatusBeforeHold.ToString() : null,
                    Version = x.Version,
                    CancelDateTime = x.CancelDateTime,
                    CancelBy = x.CancelBy == Guid.Empty ? null : x.CancelBy
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<LabOrderDetailResponse> CreateAsync(
            CreateLabOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EncounterId == Guid.Empty)
                throw new ArgumentException("EncounterId wajib diisi.");

            if (request.ProcedureId == Guid.Empty)
                throw new ArgumentException("ProcedureId wajib diisi.");

            var encounterExists = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.EncounterId && !x.IsDelete, cancellationToken);

            if (!encounterExists)
                throw new KeyNotFoundException("Encounter tidak ditemukan.");

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ProcedureId &&
                    x.IsLaboratory &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (procedure == null)
                throw new ArgumentException("Procedure tidak ditemukan, tidak aktif, atau bukan procedure laboratorium.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            // Endpoint pembuatan yang sudah ada sejak sebelum RJ-BIL-BE-003 berarti "pesanan
            // dikirim ke laboratorium", sehingga status awalnya Requested dan bukan Draft.
            // Mengubah artinya menjadi Draft akan mengubah perilaku endpoint lama tanpa manfaat.
            var entity = new LabOrder
            {
                EncounterId = request.EncounterId,
                ProcedureId = request.ProcedureId,
                OrderStatus = LabOrderStatus.Requested,
                RequestedAt = now,
                RequestedByUserId = actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabOrders.Add(entity);

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                "Order.Request",
                fromStatus: null,
                LabOrderStatus.Requested.ToString(),
                reasonCode: null,
                reasonNote: null,
                actorUserId,
                now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrder.Create",
                "Membuat order laboratorium.",
                new { entity.Id, entity.EncounterId, entity.ProcedureId, ActorUserId = actorUserId });

            return MapDetailResponse(entity, procedure);
        }

        /// <summary>
        /// Menandai pesanan mulai dikerjakan. Tidak menerbitkan fakta apa pun; tagihan sudah
        /// terbentuk pada saat sampel dinyatakan layak, bukan di sini.
        /// </summary>
        public Task<LabOrderDetailResponse> StartProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            MoveOrderStatusAsync(
                id,
                new[] { LabOrderStatus.Accepted },
                LabOrderStatus.InProcess,
                "Order.StartProcess",
                note: null,
                cancellationToken);

        public Task<LabOrderDetailResponse> CompleteAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            MoveOrderStatusAsync(
                id,
                new[] { LabOrderStatus.InProcess },
                LabOrderStatus.Completed,
                "Order.Complete",
                note: null,
                cancellationToken);

        public async Task<LabOrderDetailResponse> HoldAsync(
            Guid id,
            HoldLabRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadTrackedAsync(id, cancellationToken);

            if (entity.OrderStatus == LabOrderStatus.OnHold)
                throw new InvalidOperationException("Pesanan laboratorium sudah ditahan.");

            if (entity.OrderStatus is LabOrderStatus.Cancelled or LabOrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    $"Pesanan laboratorium berstatus {entity.OrderStatus} tidak dapat ditahan.");
            }

            var reason = request.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Alasan penahanan wajib diisi.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = entity.OrderStatus;

            entity.StatusBeforeHold = fromStatus;
            entity.OrderStatus = LabOrderStatus.OnHold;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                "Order.Hold",
                fromStatus.ToString(),
                LabOrderStatus.OnHold.ToString(),
                reasonCode: null,
                reasonNote: reason,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return await GetDetailOrThrowAsync(entity.Id, cancellationToken);
        }

        public async Task<LabOrderDetailResponse> ResumeAsync(
            Guid id,
            ResumeLabRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadTrackedAsync(id, cancellationToken);

            if (entity.OrderStatus != LabOrderStatus.OnHold)
                throw new InvalidOperationException("Pesanan laboratorium tidak sedang ditahan.");

            var resumeTo = entity.StatusBeforeHold ?? LabOrderStatus.Requested;

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.OrderStatus = resumeTo;
            entity.StatusBeforeHold = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                "Order.Resume",
                LabOrderStatus.OnHold.ToString(),
                resumeTo.ToString(),
                reasonCode: null,
                reasonNote: string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return await GetDetailOrThrowAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Membatalkan pesanan laboratorium beserta seluruh sampel yang masih berjalan.
        ///
        /// Pembatalan klinis bukan pembatalan finansial. Untuk setiap sampel yang sebelumnya
        /// sudah dinyatakan layak, diterbitkan fakta pembatalan sebagai revisi baru atas fakta
        /// yang sama, sehingga tagihan lama tetap utuh dan Billing yang menentukan koreksinya.
        /// Sampel yang belum pernah layak tidak menghasilkan koreksi apa pun karena tagihannya
        /// memang belum pernah terbentuk.
        /// </summary>
        public async Task<LabOrderCancellationResult> CancelAsync(
            Guid id,
            CancelLabSpecimenRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadTrackedAsync(id, cancellationToken);

            if (entity.OrderStatus == LabOrderStatus.Cancelled || entity.IsCancel)
                throw new InvalidOperationException("Order laboratorium sudah dibatalkan.");

            if (entity.OrderStatus == LabOrderStatus.Completed)
                throw new InvalidOperationException("Order laboratorium yang sudah selesai tidak dapat dibatalkan.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = entity.OrderStatus;
            var reason = string.IsNullOrWhiteSpace(request?.Reason) ? null : request!.Reason!.Trim();

            var previouslyAccepted = await _labSpecimenService.CancelAllForOrderInMemoryAsync(
                entity,
                reason,
                actorUserId,
                now,
                cancellationToken);

            // Status klinis dan status pemenuhan saja. Tidak ada status pembayaran yang
            // disentuh dari sini — sejalan dengan keputusan author 1B pada RJ-BIL-BE-002.
            entity.OrderStatus = LabOrderStatus.Cancelled;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                "Order.Cancel",
                fromStatus.ToString(),
                LabOrderStatus.Cancelled.ToString(),
                reasonCode: null,
                reasonNote: reason,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrder.Cancel",
                "Membatalkan order laboratorium.",
                new
                {
                    entity.Id,
                    entity.EncounterId,
                    PreviouslyAcceptedSpecimens = previouslyAccepted.Count,
                    ActorUserId = actorUserId
                });

            // Penyerahan ke Billing dilakukan setelah perubahan klinis tersimpan. Billing yang
            // tidak dapat dihubungi tidak boleh membatalkan pembatalan klinis yang sudah sah.
            var handoffs = new List<LabBillingHandoffResponse>();

            foreach (var specimen in previouslyAccepted)
            {
                var emission = await _labSpecimenService.EmitClinicalCancellationAsync(
                    specimen,
                    entity,
                    actorUserId,
                    cancellationToken);

                handoffs.Add(MapHandoff(emission));
            }

            var detail = await GetDetailOrThrowAsync(entity.Id, cancellationToken);

            return new LabOrderCancellationResult(detail, handoffs);
        }

        public static LabBillingHandoffResponse MapHandoff(ClinicalFactEmissionResult emission) =>
            new()
            {
                Kind = emission.Kind.ToString(),
                IsClinicallySafe = emission.IsClinicallySafe,
                MilestoneFactId = emission.MilestoneFactId,
                MilestoneFactVersion = emission.MilestoneFactVersion,
                Code = emission.Code,
                Message = emission.Message
            };

        private async Task<LabOrderDetailResponse> MoveOrderStatusAsync(
            Guid id,
            LabOrderStatus[] allowedFrom,
            LabOrderStatus target,
            string action,
            string? note,
            CancellationToken cancellationToken)
        {
            var entity = await LoadTrackedAsync(id, cancellationToken);

            if (Array.IndexOf(allowedFrom, entity.OrderStatus) < 0)
            {
                throw new InvalidOperationException(
                    $"Pesanan berstatus {entity.OrderStatus} tidak dapat dipindahkan ke {target}.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var fromStatus = entity.OrderStatus;

            entity.OrderStatus = target;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            if (target == LabOrderStatus.Completed)
                entity.CompletedAt = now;

            _labSpecimenService.AppendHistory(
                entity,
                specimen: null,
                LabTransitionScope.LabOrder,
                action,
                fromStatus.ToString(),
                target.ToString(),
                reasonCode: null,
                reasonNote: note,
                actorUserId,
                now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return await GetDetailOrThrowAsync(entity.Id, cancellationToken);
        }

        private async Task<LabOrder> LoadTrackedAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.LabOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Order laboratorium tidak ditemukan.");

            return entity;
        }

        private async Task<LabOrderDetailResponse> GetDetailOrThrowAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var detail = await GetDetailAsync(id, cancellationToken);

            if (detail == null)
                throw new KeyNotFoundException("Order laboratorium tidak ditemukan.");

            return detail;
        }

        private async Task SaveWithConcurrencyGuardAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new LabConcurrencyException(
                    "Data laboratorium sudah diubah oleh petugas lain. Muat ulang lalu ulangi tindakan Anda.");
            }
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private static LabOrderDetailResponse MapDetailResponse(LabOrder entity, MstProcedure? procedure)
        {
            return new LabOrderDetailResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ProcedureId = entity.ProcedureId,
                ProcedureCode = procedure?.ProcedureCode ?? string.Empty,
                ProcedureName = procedure?.ProcedureName ?? string.Empty,
                OrderStatus = entity.OrderStatus.ToString(),
                SpecimenCount = 0,
                AcceptedSpecimenCount = 0,
                IsCancel = entity.IsCancel,
                CreateDateTime = entity.CreateDateTime,
                RequestedAt = entity.RequestedAt,
                CompletedAt = entity.CompletedAt,
                StatusBeforeHold = entity.StatusBeforeHold?.ToString(),
                Version = entity.Version,
                CancelDateTime = entity.CancelDateTime,
                CancelBy = entity.CancelBy == Guid.Empty ? null : entity.CancelBy
            };
        }
    }

    /// <summary>
    /// Hasil pembatalan pesanan beserta ringkasan penyerahan fakta pembatalan untuk setiap
    /// sampel yang sebelumnya sudah dinyatakan layak.
    /// </summary>
    public sealed record LabOrderCancellationResult(
        LabOrderDetailResponse Order,
        List<LabBillingHandoffResponse> BillingHandoffs);
}
