using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Permintaan HD masuk dari rawat inap, IGD, dan rawat jalan (<c>BE-HMD-07</c>,
    /// <c>HMD-DEC-008</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Penerimaan tidak pernah membuat sesi.</b> Permintaan yang diterima tetap menunggu
    /// koordinator menjadwalkannya; baru saat sesi dibentuk dan menunjuk permintaan ini, statusnya
    /// berpindah ke <c>Fulfilled</c>.
    /// </para>
    /// <para>
    /// <b>Tiga kewenangan yang dijaga di sini, bukan oleh mesin hak akses:</b> (1) penolakan hanya
    /// oleh akun yang tertaut ke data dokter aktif (<c>HMD-VAL-005</c>) — dibaca dari relasi akun,
    /// bukan nama peran; (2) pembatalan hanya oleh pembuat permintaannya sendiri; (3) penolakan
    /// bersifat final. Butir hak akses <c>HemodialysisOrder : Reject</c> tetap menjadi gerbang
    /// pertamanya pada controller.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Dokter menolak permintaan karena hemodinamik pasien tidak stabil; status
    /// menjadi <c>Rejected</c> beserta alasan, pelaku, dan waktunya. Koordinator yang kemudian
    /// menekan Terima menerima <c>422</c>, karena penolakan klinis tidak dapat dibuka kembali —
    /// bila kondisi pasien berubah, dibuat permintaan baru supaya riwayat penolakannya tetap terbaca.
    /// </para>
    /// </remarks>
    public class HmdOrderService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly InpatientClinicalContextService _clinicalContextService;

        public HmdOrderService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            InpatientClinicalContextService clinicalContextService)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _clinicalContextService = clinicalContextService;
        }

        public static HmdOrderFilterMetadataResponse BuildFilterMetadata() => new()
        {
            OrderStatusOptions = HmdLabels.Options<HmdOrderStatus>(HmdLabels.OrderStatus),
            PriorityOptions = HmdLabels.Options<HmdOrderPriority>(HmdLabels.Priority),
            SortOptions =
            [
                new() { Value = "requestedAt", Label = "Waktu permintaan" },
                new() { Value = "priority", Label = "Prioritas" },
                new() { Value = "orderStatus", Label = "Status" },
                new() { Value = "requestedDate", Label = "Tanggal yang diharapkan" },
                new() { Value = "orderNumber", Label = "Nomor permintaan" }
            ],
            SortDirections = HmdLabels.SortDirections(),
            PageSizeOptions = HmdLabels.PageSizeOptions()
        };

        public async Task<HmdOrderSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
        {
            var rows = _dbContext.HmdOrders.AsNoTracking().Where(x => !x.IsDelete);

            return new HmdOrderSummaryResponse
            {
                TotalOrder = await rows.CountAsync(cancellationToken),
                RequestedOrder = await rows.CountAsync(x => x.OrderStatus == HmdOrderStatus.Requested, cancellationToken),
                AcceptedOrder = await rows.CountAsync(x => x.OrderStatus == HmdOrderStatus.Accepted, cancellationToken),
                OnHoldOrder = await rows.CountAsync(x => x.OrderStatus == HmdOrderStatus.OnHold, cancellationToken),
                RejectedOrder = await rows.CountAsync(x => x.OrderStatus == HmdOrderStatus.Rejected, cancellationToken),
                CancelledOrder = await rows.CountAsync(x => x.OrderStatus == HmdOrderStatus.Cancelled, cancellationToken),
                FulfilledOrder = await rows.CountAsync(x => x.OrderStatus == HmdOrderStatus.Fulfilled, cancellationToken),
                PendingCitoOrder = await rows.CountAsync(x =>
                    x.Priority == HmdOrderPriority.Cito &&
                    (x.OrderStatus == HmdOrderStatus.Requested || x.OrderStatus == HmdOrderStatus.Accepted || x.OrderStatus == HmdOrderStatus.OnHold),
                    cancellationToken)
            };
        }

        public async Task<PagedResult<HmdOrderResponse>> GetListAsync(HmdOrderPagedQuery query, CancellationToken cancellationToken)
        {
            var (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);

            var rows = _dbContext.HmdOrders.AsNoTracking().Where(x => !x.IsDelete);

            if (query.PatientId.HasValue)
                rows = rows.Where(x => x.PatientId == query.PatientId.Value);
            if (query.EncounterId.HasValue)
                rows = rows.Where(x => x.EncounterId == query.EncounterId.Value);
            if (query.InpEpisodeId.HasValue)
                rows = rows.Where(x => x.InpEpisodeId == query.InpEpisodeId.Value);
            if (query.OrderStatus.HasValue)
                rows = rows.Where(x => x.OrderStatus == query.OrderStatus.Value);
            if (query.Priority.HasValue)
                rows = rows.Where(x => x.Priority == query.Priority.Value);
            if (query.StartDate.HasValue)
            {
                var start = query.StartDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                rows = rows.Where(x => x.RequestedAt >= start);
            }
            if (query.EndDate.HasValue)
            {
                var end = query.EndDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                rows = rows.Where(x => x.RequestedAt < end);
            }
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x =>
                    x.OrderNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            var ascending = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            rows = (query.SortBy ?? "requestedAt").ToLowerInvariant() switch
            {
                "priority" => ascending ? rows.OrderBy(x => x.Priority).ThenBy(x => x.RequestedAt) : rows.OrderByDescending(x => x.Priority).ThenBy(x => x.RequestedAt),
                "orderstatus" => ascending ? rows.OrderBy(x => x.OrderStatus) : rows.OrderByDescending(x => x.OrderStatus),
                "requesteddate" => ascending ? rows.OrderBy(x => x.RequestedDate) : rows.OrderByDescending(x => x.RequestedDate),
                "ordernumber" => ascending ? rows.OrderBy(x => x.OrderNumber) : rows.OrderByDescending(x => x.OrderNumber),
                _ => ascending ? rows.OrderBy(x => x.RequestedAt) : rows.OrderByDescending(x => x.RequestedAt)
            };

            var total = await rows.CountAsync(cancellationToken);
            var items = await Project(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            items.ForEach(Label);

            return new PagedResult<HmdOrderResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            };
        }

        public async Task<HmdOrderDetailResponse?> GetDetailAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var row = await _dbContext.HmdOrders.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new HmdOrderDetailResponse
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    EncounterId = x.EncounterId,
                    EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                    InpEpisodeId = x.InpEpisodeId,
                    EpisodeId = x.EpisodeId,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                    RequestedByUserId = x.RequestedByUserId,
                    RequestedByName = _dbContext.Users.Where(u => u.Id == x.RequestedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                    RequestingDoctorId = x.RequestingDoctorId,
                    RequestingDoctorName = x.RequestingDoctor != null ? x.RequestingDoctor.FullName : null,
                    RequestedAt = x.RequestedAt,
                    Priority = x.Priority,
                    RequestedDate = x.RequestedDate,
                    OrderStatus = x.OrderStatus,
                    DecisionAt = x.DecisionAt,
                    DecisionByUserId = x.DecisionByUserId,
                    ClinicalReason = x.ClinicalReason,
                    StatusBeforeHold = x.StatusBeforeHold,
                    DecisionReason = x.DecisionReason,
                    DecisionByName = _dbContext.Users.Where(u => u.Id == x.DecisionByUserId).Select(u => u.DisplayName).FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                return null;

            Label(row);

            row.AvailableActions = row.OrderStatus switch
            {
                HmdOrderStatus.Requested => ["Accept", "Hold", "Reject"],
                HmdOrderStatus.Accepted => ["Hold"],
                HmdOrderStatus.OnHold => ["ReleaseHold", "Reject"],
                _ => []
            };

            if (row.OrderStatus == HmdOrderStatus.Requested && row.RequestedByUserId == actorUserId)
                row.AvailableActions.Add("Cancel");

            return row;
        }

        /// <summary>
        /// Membuat permintaan HD. Pasien dan kunjungan wajib sah, dan alasan klinis wajib diisi
        /// (<c>HMD-VAL-001</c>). Pembuatnya diturunkan dari pengguna yang sedang masuk.
        /// </summary>
        public async Task<HmdResult<HmdOrderDetailResponse>> CreateAsync(
            CreateHmdOrderRequest request,
            HmdActor actor,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.ClinicalReason);
            if (reason == null || !request.EncounterId.HasValue || request.EncounterId.Value == Guid.Empty)
                return HmdResult<HmdOrderDetailResponse>.Invalid(HmdErrorCodes.Val001, HmdMessages.Val001);

            var patientExists = await _dbContext.Set<MstPatient>().AsNoTracking()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete, cancellationToken);
            if (!patientExists)
                return HmdResult<HmdOrderDetailResponse>.Invalid(HmdErrorCodes.Val001, HmdMessages.Val001);

            var encounter = await HmdServiceSupport.CheckEncounterAsync(_dbContext, request.EncounterId.Value, request.PatientId, cancellationToken);
            if (!encounter.IsValid)
                return HmdResult<HmdOrderDetailResponse>.Invalid(HmdErrorCodes.Val001, HmdMessages.Val001);

            if (request.InpEpisodeId.HasValue &&
                !await _dbContext.Set<InpEpisode>().AsNoTracking().AnyAsync(x =>
                    x.Id == request.InpEpisodeId.Value && x.PatientId == request.PatientId && !x.IsDelete, cancellationToken))
            {
                return HmdResult<HmdOrderDetailResponse>.Invalid(
                    HmdErrorCodes.InvalidRequest, "Episode rawat inap tidak ditemukan atau bukan milik pasien ini.");
            }

            if (request.RequestingDoctorId.HasValue &&
                !await _dbContext.Set<MstDoctor>().AsNoTracking().AnyAsync(x =>
                    x.Id == request.RequestingDoctorId.Value && !x.IsDelete && x.IsActive, cancellationToken))
            {
                return HmdResult<HmdOrderDetailResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Dokter peminta tidak ditemukan atau tidak aktif.");
            }

            // Permintaan dari pasien yang sudah punya program HD aktif langsung ditautkan ke
            // programnya, supaya koordinator tidak perlu mencarinya ulang.
            var activeEpisodeId = await _dbContext.HmdEpisodes.AsNoTracking()
                .Where(x => x.PatientId == request.PatientId && !x.IsDelete && x.EpisodeStatus == HmdEpisodeStatus.Active)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var orderNumber = await HmdServiceSupport.AllocateNumberAsync(
                _numberSeriesAllocator, HmdServiceSupport.OrderSequenceKey, HmdServiceSupport.OrderNumberPrefix, actor.UserId, cancellationToken);
            if (orderNumber == null)
                return HmdResult<HmdOrderDetailResponse>.Rule(HmdErrorCodes.NumberAllocationFailed, HmdMessages.NumberAllocationFailed);

            var now = DateTime.UtcNow;
            var order = new HmdOrder
            {
                OrderNumber = orderNumber,
                PatientId = request.PatientId,
                EncounterId = request.EncounterId.Value,
                InpEpisodeId = request.InpEpisodeId,
                EpisodeId = activeEpisodeId,
                RequestedByUserId = actor.UserId,
                RequestingDoctorId = request.RequestingDoctorId,
                RequestedAt = now,
                Priority = request.Priority,
                ClinicalReason = reason,
                RequestedDate = request.RequestedDate,
                OrderStatus = HmdOrderStatus.Requested,
                CreateDateTime = now,
                CreateBy = actor.UserId
            };

            _dbContext.HmdOrders.Add(order);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdOrderDetailResponse>.From(failure);

            return HmdResult<HmdOrderDetailResponse>.Created((await GetDetailAsync(order.Id, actor.UserId, cancellationToken))!);
        }

        /// <summary>Koordinator menerima permintaan. Penerimaan tidak membuat sesi.</summary>
        public async Task<HmdResult<HmdOrderDetailResponse>> AcceptAsync(
            Guid id,
            AcceptHmdOrderRequest request,
            HmdActor actor,
            CancellationToken cancellationToken)
        {
            var order = await FindAsync(id, cancellationToken);
            if (order == null)
                return NotFound();

            if (order.OrderStatus != HmdOrderStatus.Requested)
                return RejectTransition(order.OrderStatus);

            if (request.EpisodeId.HasValue)
            {
                var episodeOk = await _dbContext.HmdEpisodes.AsNoTracking().AnyAsync(x =>
                    x.Id == request.EpisodeId.Value &&
                    x.PatientId == order.PatientId &&
                    !x.IsDelete &&
                    x.EpisodeStatus != HmdEpisodeStatus.Closed,
                    cancellationToken);

                if (!episodeOk)
                {
                    return HmdResult<HmdOrderDetailResponse>.Invalid(
                        HmdErrorCodes.InvalidRequest, "Episode hemodialisa tidak ditemukan, sudah ditutup, atau bukan milik pasien ini.");
                }

                order.EpisodeId = request.EpisodeId.Value;
            }

            ApplyDecision(order, HmdOrderStatus.Accepted, null, actor.UserId);
            return await SaveAndReturnAsync(order, actor.UserId, cancellationToken);
        }

        /// <summary>Menahan permintaan dengan alasan operasional (<c>HMD-VAL-003</c>).</summary>
        public async Task<HmdResult<HmdOrderDetailResponse>> HoldAsync(
            Guid id,
            HoldHmdOrderRequest request,
            HmdActor actor,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdOrderDetailResponse>.Invalid(HmdErrorCodes.Val003, HmdMessages.Val003);

            var order = await FindAsync(id, cancellationToken);
            if (order == null)
                return NotFound();

            if (order.OrderStatus is not (HmdOrderStatus.Requested or HmdOrderStatus.Accepted))
                return RejectTransition(order.OrderStatus);

            order.StatusBeforeHold = order.OrderStatus;
            ApplyDecision(order, HmdOrderStatus.OnHold, reason, actor.UserId);
            return await SaveAndReturnAsync(order, actor.UserId, cancellationToken);
        }

        /// <summary>Melepas penahanan, kembali ke status sebelum ditahan (<c>HMD-VAL-004</c>).</summary>
        public async Task<HmdResult<HmdOrderDetailResponse>> ReleaseHoldAsync(
            Guid id,
            HmdActor actor,
            CancellationToken cancellationToken)
        {
            var order = await FindAsync(id, cancellationToken);
            if (order == null)
                return NotFound();

            if (order.OrderStatus != HmdOrderStatus.OnHold)
                return HmdResult<HmdOrderDetailResponse>.Conflict(HmdErrorCodes.Val004, HmdMessages.Val004);

            var target = order.StatusBeforeHold ?? HmdOrderStatus.Requested;
            order.StatusBeforeHold = null;
            ApplyDecision(order, target, order.DecisionReason, actor.UserId);
            return await SaveAndReturnAsync(order, actor.UserId, cancellationToken);
        }

        /// <summary>
        /// Dokter menolak permintaan dengan alasan klinis. Penolakan bersifat final.
        /// </summary>
        public async Task<HmdResult<HmdOrderDetailResponse>> RejectAsync(
            Guid id,
            RejectHmdOrderRequest request,
            HmdActor actor,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.ClinicalReason);
            if (reason == null)
                return HmdResult<HmdOrderDetailResponse>.Invalid(HmdErrorCodes.Val005, HmdMessages.Val005);

            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(actor.Principal, actor.UserId, cancellationToken);
            if (!doctorId.HasValue)
                return HmdResult<HmdOrderDetailResponse>.Forbidden(HmdErrorCodes.Val005, HmdMessages.Val005);

            var order = await FindAsync(id, cancellationToken);
            if (order == null)
                return NotFound();

            if (order.OrderStatus is not (HmdOrderStatus.Requested or HmdOrderStatus.OnHold))
                return RejectTransition(order.OrderStatus);

            order.StatusBeforeHold = null;
            ApplyDecision(order, HmdOrderStatus.Rejected, reason, actor.UserId);
            return await SaveAndReturnAsync(order, actor.UserId, cancellationToken);
        }

        /// <summary>Pembuat membatalkan permintaannya sendiri, selama belum diterima unit HD (<c>HMD-VAL-006</c>).</summary>
        public async Task<HmdResult<HmdOrderDetailResponse>> CancelAsync(
            Guid id,
            CancelHmdOrderRequest request,
            HmdActor actor,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdOrderDetailResponse>.Invalid(HmdErrorCodes.Val080, HmdMessages.Val080);

            var order = await FindAsync(id, cancellationToken);
            if (order == null)
                return NotFound();

            if (order.RequestedByUserId != actor.UserId)
            {
                return HmdResult<HmdOrderDetailResponse>.Forbidden(
                    HmdErrorCodes.InvalidRequest, "Permintaan hanya dapat dibatalkan oleh pembuatnya sendiri.");
            }

            if (order.OrderStatus is HmdOrderStatus.Accepted or HmdOrderStatus.Fulfilled)
                return HmdResult<HmdOrderDetailResponse>.Conflict(HmdErrorCodes.Val006, HmdMessages.Val006);

            if (order.OrderStatus != HmdOrderStatus.Requested)
                return RejectTransition(order.OrderStatus);

            ApplyDecision(order, HmdOrderStatus.Cancelled, reason, actor.UserId);
            order.CancelDateTime = order.DecisionAt;
            order.CancelBy = actor.UserId;
            return await SaveAndReturnAsync(order, actor.UserId, cancellationToken);
        }

        // =================================================================
        // Penolong
        // =================================================================

        private Task<HmdOrder?> FindAsync(Guid id, CancellationToken cancellationToken) =>
            _dbContext.HmdOrders.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        private static HmdResult<HmdOrderDetailResponse> NotFound() =>
            HmdResult<HmdOrderDetailResponse>.NotFound("Permintaan hemodialisa tidak ditemukan atau sudah dihapus.");

        /// <summary>
        /// Permintaan yang masih dalam alur (<c>Accepted</c>, <c>OnHold</c>) berarti sudah diproses
        /// orang lain — <c>409 HMD-VAL-002</c>. Permintaan yang sudah final (<c>Rejected</c>,
        /// <c>Cancelled</c>, <c>Fulfilled</c>) tidak dapat berpindah ke mana pun — <c>422</c>.
        /// </summary>
        private static HmdResult<HmdOrderDetailResponse> RejectTransition(HmdOrderStatus current) =>
            current is HmdOrderStatus.Rejected or HmdOrderStatus.Cancelled or HmdOrderStatus.Fulfilled
                ? HmdResult<HmdOrderDetailResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    $"Permintaan ini sudah berstatus {HmdLabels.OrderStatus(current).ToLowerInvariant()} dan bersifat final.")
                : HmdResult<HmdOrderDetailResponse>.Conflict(HmdErrorCodes.Val002, HmdMessages.Val002);

        private static void ApplyDecision(HmdOrder order, HmdOrderStatus target, string? reason, Guid actorUserId)
        {
            var now = DateTime.UtcNow;
            order.OrderStatus = target;
            order.DecisionByUserId = actorUserId;
            order.DecisionAt = now;
            order.DecisionReason = reason;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;
            order.Version++;
        }

        private async Task<HmdResult<HmdOrderDetailResponse>> SaveAndReturnAsync(
            HmdOrder order,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdOrderDetailResponse>.From(failure);

            return HmdResult<HmdOrderDetailResponse>.Ok((await GetDetailAsync(order.Id, actorUserId, cancellationToken))!);
        }

        private IQueryable<HmdOrderResponse> Project(IQueryable<HmdOrder> rows) =>
            rows.Select(x => new HmdOrderResponse
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : null,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                InpEpisodeId = x.InpEpisodeId,
                EpisodeId = x.EpisodeId,
                EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                RequestedByUserId = x.RequestedByUserId,
                RequestedByName = _dbContext.Users.Where(u => u.Id == x.RequestedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                RequestingDoctorId = x.RequestingDoctorId,
                RequestingDoctorName = x.RequestingDoctor != null ? x.RequestingDoctor.FullName : null,
                RequestedAt = x.RequestedAt,
                Priority = x.Priority,
                RequestedDate = x.RequestedDate,
                OrderStatus = x.OrderStatus,
                DecisionAt = x.DecisionAt,
                DecisionByUserId = x.DecisionByUserId
            });

        private static void Label(HmdOrderResponse row)
        {
            row.PriorityName = HmdLabels.Priority(row.Priority);
            row.OrderStatusName = HmdLabels.OrderStatus(row.OrderStatus);
        }

        private async Task<HmdResult<bool>?> TrySaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<bool>.Conflict(HmdErrorCodes.Val002, HmdMessages.Val002);
            }
        }
    }
}
