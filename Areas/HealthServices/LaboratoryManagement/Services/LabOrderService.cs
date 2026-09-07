using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
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

        /// <summary>
        /// Keterangan bentuk layar daftar pesanan. Tidak menyentuh database sama sekali.
        /// </summary>
        public LabOrderFilterMetadataResponse GetFilterMetadata() =>
            LabFilterMetadataFactory.LabOrder();

        /// <summary>
        /// Rekap pesanan pada satu rentang waktu, dihitung dari baris yang belum ditandai
        /// terhapus. Rentangnya memakai waktu pesanan dibuat.
        /// </summary>
        public async Task<LabOrderSummaryResponse> GetSummaryAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.CreateDateTime >= startDate &&
                            x.CreateDateTime <= endDate);

            // Satu perjalanan ke database, bukan sebelas. Pencacahan per status dan per
            // disiplin dikerjakan di sisi server lewat satu proyeksi agregat.
            var rekap = await source
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Draft = g.Count(x => x.OrderStatus == LabOrderStatus.Draft),
                    Diminta = g.Count(x => x.OrderStatus == LabOrderStatus.Requested),
                    Diterima = g.Count(x => x.OrderStatus == LabOrderStatus.Accepted),
                    SedangDikerjakan = g.Count(x => x.OrderStatus == LabOrderStatus.InProcess),
                    Selesai = g.Count(x => x.OrderStatus == LabOrderStatus.Completed),
                    Ditahan = g.Count(x => x.OrderStatus == LabOrderStatus.OnHold),
                    PembatalanDiminta = g.Count(x => x.OrderStatus == LabOrderStatus.CancelRequested),
                    Dibatalkan = g.Count(x => x.OrderStatus == LabOrderStatus.Cancelled),
                    PatologiKlinik = g.Count(x => x.Discipline == LabDiscipline.ClinicalPathology),
                    PatologiAnatomi = g.Count(x => x.Discipline == LabDiscipline.AnatomicalPathology),
                    Mikrobiologi = g.Count(x => x.Discipline == LabDiscipline.Microbiology),
                    TanpaDisiplin = g.Count(x => x.Discipline == null)
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new LabOrderSummaryResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalPesanan = rekap?.Total ?? 0,
                Draft = rekap?.Draft ?? 0,
                Diminta = rekap?.Diminta ?? 0,
                Diterima = rekap?.Diterima ?? 0,
                SedangDikerjakan = rekap?.SedangDikerjakan ?? 0,
                Selesai = rekap?.Selesai ?? 0,
                Ditahan = rekap?.Ditahan ?? 0,
                PembatalanDiminta = rekap?.PembatalanDiminta ?? 0,
                Dibatalkan = rekap?.Dibatalkan ?? 0,
                PatologiKlinik = rekap?.PatologiKlinik ?? 0,
                PatologiAnatomi = rekap?.PatologiAnatomi ?? 0,
                Mikrobiologi = rekap?.Mikrobiologi ?? 0,
                TanpaDisiplin = rekap?.TanpaDisiplin ?? 0
            };
        }

        /// <summary>
        /// Daftar pesanan dengan penyaring, pengurutan, dan pagination di sisi server.
        ///
        /// Penyaring <c>EncounterId</c> adalah yang paling menentukan: tanpanya, pemanggil yang
        /// hanya butuh pesanan satu pasien terpaksa menarik seluruh tabel lalu menyaringnya
        /// sendiri — dan pesanan pasien lain ikut terkirim ke browsernya. Itu keadaan yang
        /// sebelumnya benar-benar terjadi pada layar IGD (<c>IGD-DEC-105</c>).
        /// </summary>
        public async Task<PagedResult<LabOrderListResponse>> GetListAsync(
            LabOrderPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var source = _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.EncounterId.HasValue && query.EncounterId.Value != Guid.Empty)
                source = source.Where(x => x.EncounterId == query.EncounterId.Value);

            if (query.OrderStatus.HasValue)
                source = source.Where(x => x.OrderStatus == query.OrderStatus.Value);

            if (query.Discipline.HasValue)
                source = source.Where(x => x.Discipline == query.Discipline.Value);

            if (query.StartDate.HasValue)
                source = source.Where(x => x.CreateDateTime >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                source = source.Where(x => x.CreateDateTime <= query.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.Procedure != null &&
                    (EF.Functions.ILike(x.Procedure.ProcedureCode, $"%{search}%") ||
                     EF.Functions.ILike(x.Procedure.ProcedureName, $"%{search}%")));
            }

            var totalData = await source.CountAsync(cancellationToken);

            // Nama kolom yang tidak dikenal dikembalikan ke bawaan, bukan ditolak. Layar lama
            // yang mengirim kolom yang sudah tidak ada tetap memperoleh daftar yang masuk akal.
            var menaik = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            source = query.SortBy?.Trim().ToLowerInvariant() switch
            {
                "orderstatus" => menaik
                    ? source.OrderBy(x => x.OrderStatus).ThenByDescending(x => x.CreateDateTime)
                    : source.OrderByDescending(x => x.OrderStatus).ThenByDescending(x => x.CreateDateTime),
                _ => menaik
                    ? source.OrderBy(x => x.CreateDateTime)
                    : source.OrderByDescending(x => x.CreateDateTime)
            };

            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            return new PagedResult<LabOrderListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
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
                    Discipline = x.Discipline != null ? x.Discipline.ToString() : null,
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

            // LAB-DEC-025: hanya tiga disiplin yang ada. Angka di luar ketiganya ditolak di
            // sini, bukan disimpan diam-diam sebagai nilai enum yang tidak berarti apa pun.
            if (request.Discipline.HasValue && !Enum.IsDefined(request.Discipline.Value))
                throw new ArgumentException("Disiplin laboratorium tidak dikenal.");

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
                // Disiplin hanya boleh ditetapkan di sini. Setelah baris ini tersimpan, EF
                // menolak setiap upaya mengubahnya (INV-21).
                Discipline = request.Discipline,
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
                new
                {
                    entity.Id,
                    entity.EncounterId,
                    entity.ProcedureId,
                    Discipline = entity.Discipline?.ToString(),
                    ActorUserId = actorUserId
                });

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
                Message = emission.Message,
                MilestoneFactIds = emission.MilestoneFactId.HasValue
                    ? new List<Guid> { emission.MilestoneFactId.Value }
                    : new List<Guid>()
            };

        /// <summary>
        /// Bentuk jawaban untuk keputusan yang menerbitkan fakta <b>per pemeriksaan</b>
        /// (<c>FR-05.1</c>).
        ///
        /// <c>MilestoneFactId</c> tetap diisi identitas fakta pertama supaya pemanggil lama
        /// tidak putus, sementara <c>MilestoneFactIds</c> membawa seluruhnya. Satu wadah berisi
        /// tiga pemeriksaan menerbitkan tiga fakta, dan satu ruas tidak dapat mewakili
        /// ketiganya.
        /// </summary>
        public static LabBillingHandoffResponse MapHandoff(LabFactEmission emission)
        {
            var response = MapHandoff(emission.Perwakilan);

            response.MilestoneFactIds = emission.FactIds.ToList();
            response.MilestoneFactCount = emission.Count;

            return response;
        }

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
                Discipline = entity.Discipline?.ToString(),
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
