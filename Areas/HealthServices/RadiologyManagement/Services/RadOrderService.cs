using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Siklus hidup pesanan radiologi sesuai <c>RJ-BIL-GATE-DEC-004</c>.
    ///
    /// Pesanan tidak pernah menerbitkan fakta kelayakan tagih. `Requested`, `Accepted`, dan
    /// `Scheduled` secara tegas **bukan** pemicu tagihan; yang menerbitkan hanyalah study yang
    /// acquisition-nya benar-benar dikerjakan dan menghasilkan citra yang dapat dipakai.
    ///
    /// Pembatalan sebelum acquisition dimulai karena itu tidak menerbitkan koreksi apa pun:
    /// tidak ada yang perlu dikoreksi karena tidak pernah ada yang tertagih.
    /// </summary>
    public class RadOrderService
    {
        private const string LogCategory = "HealthServices.RadiologyManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public RadOrderService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        /* ================================================================ *
         * Pembacaan
         * ================================================================ */

        public async Task<List<RadOrderListResponse>> GetListAsync(
            Guid? encounterId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.RadOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            // Penyaring kunjungan disediakan sejak awal. Modul Laboratorium tidak memilikinya,
            // dan akibatnya layar Billing terpaksa menyaring seluruh pesanan rumah sakit di
            // sisi klien — batas yang tidak perlu diulang di sini.
            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
            {
                query = query.Where(x => x.EncounterId == encounterId.Value);
            }

            return await query
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new RadOrderListResponse
                {
                    Id = x.Id,
                    EncounterId = x.EncounterId,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    ModalityId = x.ModalityId,
                    ModalityCode = x.Modality != null ? x.Modality.ModalityCode : string.Empty,
                    ModalityName = x.Modality != null ? x.Modality.ModalityName : string.Empty,
                    OrderStatus = x.OrderStatus.ToString(),
                    StudyCount = x.Studies.Count(s => !s.IsDelete),
                    UsableStudyCount = x.Studies.Count(s =>
                        !s.IsDelete && s.StudyStatus == RadStudyStatus.QualityAccepted),
                    IsCancel = x.IsCancel,
                    CreateDateTime = x.CreateDateTime,
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<RadOrderDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.RadOrders
                .AsNoTracking()
                .Include(x => x.Procedure)
                .Include(x => x.Modality)
                .Include(x => x.Studies.Where(s => !s.IsDelete))
                    .ThenInclude(s => s.SafetyChecks.Where(c => !c.IsDelete))
                .Include(x => x.Studies.Where(s => !s.IsDelete))
                    .ThenInclude(s => s.Consumptions.Where(c => !c.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return entity == null ? null : MapDetail(entity);
        }

        /* ================================================================ *
         * Pembuatan dan perpindahan status
         * ================================================================ */

        public async Task<RadOperationResult<RadOrderDetailResponse>> CreateAsync(
            CreateRadOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var modalityExists = await _dbContext.MstRadModalities
                .AnyAsync(x => x.Id == request.ModalityId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (!modalityExists)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.ModalityNotFound,
                    "Modalitas tidak ditemukan atau sedang tidak aktif.");
            }

            var entity = new RadOrder
            {
                EncounterId = request.EncounterId,
                ProcedureId = request.ProcedureId,
                ModalityId = request.ModalityId,
                ClinicalIndication = request.ClinicalIndication,
                OrderStatus = RadOrderStatus.Requested,
                RequestedAt = now,
                RequestedByUserId = actorUserId,
                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            _dbContext.RadOrders.Add(entity);
            AddHistory(entity, "Order.Create", null, entity.OrderStatus.ToString(),
                null, null, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var detail = await GetDetailAsync(entity.Id, cancellationToken);
            return RadOperationResult<RadOrderDetailResponse>.Success(detail!);
        }

        public Task<RadOperationResult<RadOrderDetailResponse>> AcceptAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Accept", RadOrderStatus.Accepted,
                new[] { RadOrderStatus.Requested }, request, cancellationToken);

        public Task<RadOperationResult<RadOrderDetailResponse>> ScheduleAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Schedule", RadOrderStatus.Scheduled,
                new[] { RadOrderStatus.Accepted }, request, cancellationToken);

        public Task<RadOperationResult<RadOrderDetailResponse>> StartAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Start", RadOrderStatus.InProgress,
                new[] { RadOrderStatus.Accepted, RadOrderStatus.Scheduled }, request, cancellationToken);

        public Task<RadOperationResult<RadOrderDetailResponse>> CompleteAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Complete", RadOrderStatus.Completed,
                new[] { RadOrderStatus.InProgress }, request, cancellationToken);

        public Task<RadOperationResult<RadOrderDetailResponse>> RejectAsync(
            Guid id, RadOrderTransitionRequest request, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, "Order.Reject", RadOrderStatus.Rejected,
                new[] { RadOrderStatus.Requested }, request, cancellationToken, reasonRequired: true);

        /// <summary>
        /// Membatalkan pesanan.
        ///
        /// Pembatalan menolak berjalan bila ada study yang acquisition-nya sudah dimulai.
        /// Bukan karena tidak boleh dibatalkan, tetapi karena pembatalan pada tingkat pesanan
        /// akan menyembunyikan paparan yang sudah terjadi. Study yang sudah berjalan harus
        /// diselesaikan atau dihentikan pada tingkat study, tempat sebab dan konsumsinya dicatat.
        /// </summary>
        public async Task<RadOperationResult<RadOrderDetailResponse>> CancelAsync(
            Guid id,
            RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.RadOrders
                .Include(x => x.Studies.Where(s => !s.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            var adaStudyBerjalan = order.Studies.Any(x => x.AcquisitionStartedAt != null);

            if (adaStudyBerjalan)
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    "Pesanan ini memiliki study yang acquisition-nya sudah dimulai. " +
                    "Selesaikan atau hentikan study tersebut lebih dulu agar paparan yang " +
                    "sudah terjadi tetap tercatat.");
            }

            return await TransitionAsync(id, "Order.Cancel", RadOrderStatus.Cancelled,
                new[]
                {
                    RadOrderStatus.Draft, RadOrderStatus.Requested, RadOrderStatus.Accepted,
                    RadOrderStatus.Scheduled, RadOrderStatus.OnHold, RadOrderStatus.CancelRequested,
                },
                request, cancellationToken, reasonRequired: true);
        }

        public async Task<RadOperationResult<RadOrderDetailResponse>> HoldAsync(
            Guid id,
            RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var order = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            if (order.OrderStatus == RadOrderStatus.OnHold)
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.InvalidTransition, "Pesanan sudah berstatus OnHold.");
            }

            var from = order.OrderStatus;
            order.StatusBeforeHold = from;
            order.OrderStatus = RadOrderStatus.OnHold;
            Touch(order, actorUserId, now);

            AddHistory(order, "Order.Hold", from.ToString(), order.OrderStatus.ToString(),
                null, request?.Reason, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            var detail = await GetDetailAsync(order.Id, cancellationToken);
            return RadOperationResult<RadOrderDetailResponse>.Success(detail!);
        }

        /// <summary>
        /// Melanjutkan pesanan yang ditahan, kembali ke status sebelum penahanan.
        ///
        /// Status sebelumnya dibaca dari kolomnya sendiri, bukan ditebak dari riwayat. Menebak
        /// akan membuat pesanan yang ditahan dari `Scheduled` kembali sebagai `Accepted`, dan
        /// jadwal yang sudah disepakati dengan pasien hilang tanpa ada yang menyadarinya.
        /// </summary>
        public async Task<RadOperationResult<RadOrderDetailResponse>> ResumeAsync(
            Guid id,
            RadOrderTransitionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var order = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            if (order.OrderStatus != RadOrderStatus.OnHold)
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    "Hanya pesanan berstatus OnHold yang dapat dilanjutkan.");
            }

            var from = order.OrderStatus;
            order.OrderStatus = order.StatusBeforeHold ?? RadOrderStatus.Requested;
            order.StatusBeforeHold = null;
            Touch(order, actorUserId, now);

            AddHistory(order, "Order.Resume", from.ToString(), order.OrderStatus.ToString(),
                null, request?.Reason, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            var detail = await GetDetailAsync(order.Id, cancellationToken);
            return RadOperationResult<RadOrderDetailResponse>.Success(detail!);
        }

        /* ================================================================ *
         * Pembantu
         * ================================================================ */

        private async Task<RadOperationResult<RadOrderDetailResponse>> TransitionAsync(
            Guid id,
            string action,
            RadOrderStatus target,
            IReadOnlyCollection<RadOrderStatus> allowedFrom,
            RadOrderTransitionRequest? request,
            CancellationToken cancellationToken,
            bool reasonRequired = false)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            if (reasonRequired && string.IsNullOrWhiteSpace(request?.Reason))
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.ReasonRequired,
                    "Tindakan ini wajib disertai alasan yang tercatat.");
            }

            var order = await _dbContext.RadOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadOrderDetailResponse>.NotFound(
                    RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            if (!allowedFrom.Contains(order.OrderStatus))
            {
                return RadOperationResult<RadOrderDetailResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    $"Pesanan berstatus {order.OrderStatus} tidak dapat berpindah ke {target}.");
            }

            var from = order.OrderStatus;
            order.OrderStatus = target;

            if (target == RadOrderStatus.Scheduled && request?.ScheduledAt != null)
            {
                order.ScheduledAt = request.ScheduledAt;
            }

            if (target == RadOrderStatus.Completed)
            {
                order.CompletedAt = now;
            }

            if (target is RadOrderStatus.Cancelled or RadOrderStatus.Rejected)
            {
                order.ClosureReason = request?.Reason;
            }

            Touch(order, actorUserId, now);
            AddHistory(order, action, from.ToString(), target.ToString(),
                null, request?.Reason, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            var detail = await GetDetailAsync(order.Id, cancellationToken);
            return RadOperationResult<RadOrderDetailResponse>.Success(detail!);
        }

        private static void Touch(RadOrder order, Guid actorUserId, DateTime now)
        {
            order.UpdateBy = actorUserId;
            order.UpdateDateTime = now;
            order.Version += 1;
        }

        private void AddHistory(
            RadOrder order,
            string action,
            string? fromStatus,
            string toStatus,
            string? reasonCode,
            string? reasonNote,
            Guid actorUserId,
            DateTime now)
        {
            _dbContext.RadTransitionHistories.Add(new RadTransitionHistory
            {
                RadOrderId = order.Id,
                RadStudyId = null,
                EncounterId = order.EncounterId,
                Scope = RadTransitionScope.RadOrder,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ReasonCode = reasonCode,
                ReasonNote = reasonNote,
                ActorUserId = actorUserId,
                OccurredAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now,
            });
        }

        private async Task SaveWithConcurrencyGuardAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new RadConcurrencyException(
                    "Pesanan ini sudah diubah petugas lain. Muat ulang lalu ulangi tindakan Anda.");
            }
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            if (!Guid.TryParse(value, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Identitas petugas tidak dapat ditentukan dari sesi yang sedang berjalan. " +
                    "Tindakan radiologi tidak dijalankan.");
            }

            return userId;
        }

        private static RadOrderDetailResponse MapDetail(RadOrder entity)
        {
            return new RadOrderDetailResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ProcedureId = entity.ProcedureId,
                ProcedureCode = entity.Procedure?.ProcedureCode ?? string.Empty,
                ProcedureName = entity.Procedure?.ProcedureName ?? string.Empty,
                ModalityId = entity.ModalityId,
                ModalityCode = entity.Modality?.ModalityCode ?? string.Empty,
                ModalityName = entity.Modality?.ModalityName ?? string.Empty,
                OrderStatus = entity.OrderStatus.ToString(),
                StudyCount = entity.Studies.Count(x => !x.IsDelete),
                UsableStudyCount = entity.Studies.Count(x =>
                    !x.IsDelete && x.StudyStatus == RadStudyStatus.QualityAccepted),
                IsCancel = entity.IsCancel,
                CreateDateTime = entity.CreateDateTime,
                ClinicalIndication = entity.ClinicalIndication,
                RequestedAt = entity.RequestedAt,
                ScheduledAt = entity.ScheduledAt,
                CompletedAt = entity.CompletedAt,
                StatusBeforeHold = entity.StatusBeforeHold?.ToString(),
                ClosureReason = entity.ClosureReason,
                Version = entity.Version,
                Studies = entity.Studies
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.StudySequence)
                    .Select(RadStudyService.MapStudy)
                    .ToList(),
            };
        }
    }
}
