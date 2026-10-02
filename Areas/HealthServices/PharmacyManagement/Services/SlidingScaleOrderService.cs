using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Order sliding scale per pasien — <c>BE-RWI-103</c>, <c>FR-DOK-096</c> s.d. <c>FR-DOK-099</c>,
    /// <c>RWI-DEC-146</c>, state matrix 0.6.0 bagian 8.7.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Proses bisnisnya.</b> Dokter yang merawat memasang protokol pada butir resep insulin berdosis
    /// skala, berangkat dari satu versi template yang <b>sudah disahkan</b>. Rentang versi itu
    /// <b>disalin</b> menjadi rentang order versi 1. Dokter boleh menyesuaikan dosis atau rentang, tetapi
    /// wajib beralasan, dan setiap penyesuaian membuat versi order baru.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> dr. Ahmad memesan "SSI-DEWASA" v2 untuk Budi dan memotong dosis separuh dengan
    /// alasan "pasien sensitif insulin": GDS 280 yang biasanya 6 unit menjadi 3 unit, versi order 1
    /// bertanda disesuaikan. Besoknya v3 template disahkan — order Budi tetap memakai rentang yang
    /// tersalin, tidak berubah. dr. Rina membuka order yang sama pada versi 1, sementara dr. Ahmad lebih
    /// dulu menyesuaikan menjadi versi 2; kiriman dr. Rina yang masih membawa nomor versi 1 ditolak
    /// <c>409</c>.
    /// </para>
    /// <para>
    /// <b>Tidak ada jalur baca hasil laboratorium</b> di service ini — <c>RUL-DOK-03</c>. Pelaksanaan
    /// dosis milik sub-modul <c>keperawatan</c> dan membaca order <c>Active</c> beserta versi terbarunya.
    /// </para>
    /// </remarks>
    public class SlidingScaleOrderService
    {
        private const string LogCategory = "HealthServices.Pharmacy.SlidingScaleOrder";
        private const string OrderSequenceKey = "PHM_SLIDING_SCALE_ORDER";
        private const string OrderNumberPrefix = "SSO";
        private const int OrderSequenceDigits = 6;

        private const string PenolakanBukanDokterBerwenang =
            "Anda tidak berwenang menulis resep untuk pasien ini.";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _clinicalContextService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly LoggerService _loggerService;

        public SlidingScaleOrderService(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService clinicalContextService,
            NumberSeriesAllocator numberSeriesAllocator,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _clinicalContextService = clinicalContextService;
            _numberSeriesAllocator = numberSeriesAllocator;
            _loggerService = loggerService;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<List<SlidingScaleOrderListItem>> GetByEpisodeAsync(
            Guid episodeId,
            SlidingScaleOrderStatus? status,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<PhmSlidingScaleOrder>()
                .AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete);

            if (status.HasValue)
                query = query.Where(x => x.OrderStatus == status.Value);

            return await query
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new SlidingScaleOrderListItem
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    PrescriptionId = x.PrescriptionId,
                    PrescriptionItemId = x.PrescriptionItemId,
                    DrugNameSnapshot = x.PrescriptionItem != null ? x.PrescriptionItem.DrugNameSnapshot : null,
                    InpEpisodeId = x.InpEpisodeId,
                    PatientId = x.PatientId,
                    TemplateId = x.TemplateId,
                    TemplateName = x.Template != null ? x.Template.TemplateName : null,
                    OrderStatus = x.OrderStatus,
                    CurrentVersionNumber = x.CurrentVersionNumber,
                    IsCurrentVersionAdjusted = x.Versions
                        .Where(v => !v.IsDelete && v.VersionNumber == x.CurrentVersionNumber)
                        .Select(v => v.IsAdjusted)
                        .FirstOrDefault(),
                    CheckFrequencyCode = x.CheckFrequencyCode,
                    CreateDateTime = x.CreateDateTime,
                    StoppedAt = x.StoppedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<SlidingScaleResult<SlidingScaleOrderResponse>> GetAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            var order = await LoadOrderAsync(orderId, cancellationToken);

            if (order == null)
            {
                return SlidingScaleResult<SlidingScaleOrderResponse>.Fail(
                    StatusCodes.Status404NotFound, "Order sliding scale tidak ditemukan.");
            }

            return SlidingScaleResult<SlidingScaleOrderResponse>.Ok(ToResponse(order), "Order sliding scale berhasil diambil.");
        }

        // =====================================================================
        // Memesan
        // =====================================================================

        /// <summary>
        /// Dokter memesan protokol pada butir insulin draft resep — <c>FR-DOK-096</c>.
        /// </summary>
        public async Task<SlidingScaleResult<SlidingScaleOrderResponse>> CreateAsync(
            CreateSlidingScaleOrderRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.Set<PhmPrescriptionItem>()
                .AsNoTracking()
                .Include(x => x.Prescription)
                .FirstOrDefaultAsync(x => x.Id == request.PrescriptionItemId && !x.IsDelete, cancellationToken);

            if (item?.Prescription == null)
                return Fail(StatusCodes.Status404NotFound, "Butir resep tidak ditemukan.");

            var prescription = item.Prescription;

            if (!prescription.InpEpisodeId.HasValue)
            {
                return Fail(StatusCodes.Status409Conflict,
                    "Butir resep ini tidak dapat dipasangi protokol sliding scale.");
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == prescription.InpEpisodeId.Value && !x.IsDelete, cancellationToken);

            if (episode == null)
                return Fail(StatusCodes.Status404NotFound, "Perawatan rawat inap tidak ditemukan.");

            if (episode.EpisodeStatus is InpEpisodeStatus.Closed or InpEpisodeStatus.Cancelled)
                return Fail(StatusCodes.Status422UnprocessableEntity, "Perawatan pasien ini sudah ditutup.");

            var now = DateTime.UtcNow;

            var doctorId = await ResolveAuthorizedDoctorAsync(user, actorUserId, episode.Id, now, cancellationToken);

            if (!doctorId.HasValue)
                return Fail(StatusCodes.Status403Forbidden, PenolakanBukanDokterBerwenang);

            // VAL-DOK-55b.
            var bukanButirSkala =
                item.DoseKind != PrescriptionDoseKind.SlidingScale ||
                item.IsStopped ||
                prescription.IsCancel ||
                prescription.PrescriptionStatus != PrescriptionStatus.Draft;

            var sudahPunyaOrder = await _dbContext.Set<PhmSlidingScaleOrder>()
                .AsNoTracking()
                .AnyAsync(x => x.PrescriptionItemId == item.Id && !x.IsDelete, cancellationToken);

            if (bukanButirSkala || sudahPunyaOrder)
            {
                return Fail(StatusCodes.Status409Conflict,
                    "Butir resep ini tidak dapat dipasangi protokol sliding scale.");
            }

            var templateVersion = await _dbContext.Set<PhmSlidingScaleTemplateVersion>()
                .AsNoTracking()
                .Include(x => x.Template)
                .Include(x => x.Ranges)
                .FirstOrDefaultAsync(x => x.Id == request.TemplateVersionId && !x.IsDelete, cancellationToken);

            if (templateVersion?.Template == null)
                return Fail(StatusCodes.Status404NotFound, "Versi protokol sliding scale tidak ditemukan.");

            // VAL-DOK-55 — BE-RWI-102 kriteria 6 dan BE-RWI-103 kriteria 1: hanya versi Approved.
            if (templateVersion.VersionStatus != SlidingScaleVersionStatus.Approved)
                return Fail(StatusCodes.Status409Conflict, "Protokol ini belum disahkan sehingga belum dapat dipesan.");

            if (!templateVersion.Template.IsActive || templateVersion.Template.IsDelete)
                return Fail(StatusCodes.Status409Conflict, "Template protokol ini sudah nonaktif sehingga tidak dapat dipesan.");

            var rentangTemplate = SlidingScaleRangeValidator.ToRequests(templateVersion.Ranges);

            IReadOnlyList<SlidingScaleRangeRequest> rentangOrder;
            var disesuaikan = false;

            if (request.Ranges == null || request.Ranges.Count == 0)
            {
                // Kriteria 2: rentang TERSALIN, bukan dirujuk.
                rentangOrder = rentangTemplate;
            }
            else
            {
                var validasi = SlidingScaleRangeValidator.Validate(request.Ranges, templateVersion.GlucoseUnit);

                if (!validasi.IsValid)
                    return Fail(StatusCodes.Status400BadRequest, validasi.ErrorMessage!);

                rentangOrder = validasi.OrderedRanges;
                disesuaikan = !SlidingScaleRangeValidator.AreEquivalent(rentangOrder, rentangTemplate);
            }

            var alasan = string.IsNullOrWhiteSpace(request.AdjustmentReason) ? null : request.AdjustmentReason.Trim();

            // VAL-DOK-55a.
            if (disesuaikan && alasan == null)
                return Fail(StatusCodes.Status400BadRequest, "Alasan penyesuaian wajib diisi.");

            string orderNumber;

            try
            {
                orderNumber = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: OrderSequenceKey,
                        Prefix: OrderNumberPrefix,
                        ResetPolicy: NumberSeriesResetPolicies.Yearly,
                        SequenceDigits: OrderSequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return Fail(StatusCodes.Status422UnprocessableEntity,
                    "Nomor order sliding scale gagal diterbitkan. Hubungi administrator sistem.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var order = new PhmSlidingScaleOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                PrescriptionId = prescription.Id,
                PrescriptionItemId = item.Id,
                EncounterId = prescription.EncounterId,
                InpEpisodeId = episode.Id,
                PatientId = prescription.PatientId,
                TemplateId = templateVersion.TemplateId,
                OrderStatus = SlidingScaleOrderStatus.Active,
                CurrentVersionNumber = 1,
                CheckFrequencyCode = string.IsNullOrWhiteSpace(request.CheckFrequencyCode) ? null : request.CheckFrequencyCode.Trim(),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            var version = new PhmSlidingScaleOrderVersion
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                VersionNumber = 1,
                TemplateVersionId = templateVersion.Id,
                IsAdjusted = disesuaikan,
                AdjustmentReason = disesuaikan ? alasan : null,
                OrderedByDoctorId = doctorId.Value,
                OrderedByUserId = actorUserId,
                OrderedAt = now,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<PhmSlidingScaleOrder>().Add(order);
            _dbContext.Set<PhmSlidingScaleOrderVersion>().Add(version);
            _dbContext.Set<PhmSlidingScaleRange>().AddRange(
                SlidingScaleRangeValidator.BuildRows(rentangOrder, null, version.Id, actorUserId, now));

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Fail(StatusCodes.Status409Conflict,
                    "Butir resep ini tidak dapat dipasangi protokol sliding scale.");
            }

            // AdjustmentReason SENSITIF — tidak masuk payload logger.
            await _loggerService.InfoAsync(LogCategory, "SlidingScaleOrder.Create",
                "Dokter memesan protokol sliding scale.",
                new
                {
                    order.Id,
                    order.OrderNumber,
                    order.PrescriptionItemId,
                    order.InpEpisodeId,
                    version.TemplateVersionId,
                    version.IsAdjusted,
                    version.OrderedByDoctorId
                });

            return await ReloadAsync(order.Id, "Order sliding scale berhasil dibuat.", StatusCodes.Status201Created, cancellationToken);
        }

        // =====================================================================
        // Menyesuaikan
        // =====================================================================

        /// <summary>
        /// Menyesuaikan order → versi order baru — <c>BE-RWI-103</c> kriteria 3 dan 5, <c>FR-DOK-098</c>.
        /// </summary>
        public async Task<SlidingScaleResult<SlidingScaleOrderResponse>> AdjustAsync(
            Guid orderId,
            AdjustSlidingScaleOrderRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.Set<PhmSlidingScaleOrder>()
                .Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);

            if (order == null)
                return Fail(StatusCodes.Status404NotFound, "Order sliding scale tidak ditemukan.");

            // VAL-DOK-55d.
            if (order.OrderStatus == SlidingScaleOrderStatus.Stopped)
                return Fail(StatusCodes.Status409Conflict, "Protokol sudah dihentikan. Pesan protokol baru bila diperlukan.");

            // VAL-DOK-55c — kiriman yang membawa nomor versi basi.
            if (request.ExpectedVersionNumber != order.CurrentVersionNumber)
                return Fail(StatusCodes.Status409Conflict, "Protokol sudah diubah dokter lain. Muat ulang lalu periksa kembali.");

            var alasan = string.IsNullOrWhiteSpace(request.AdjustmentReason) ? null : request.AdjustmentReason.Trim();

            // VAL-DOK-55a.
            if (alasan == null)
                return Fail(StatusCodes.Status400BadRequest, "Alasan penyesuaian wajib diisi.");

            var versiSekarang = order.Versions
                .Where(x => !x.IsDelete)
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefault();

            if (versiSekarang == null)
                return Fail(StatusCodes.Status409Conflict, "Order sliding scale tidak punya versi yang dapat disesuaikan.");

            var satuan = await _dbContext.Set<PhmSlidingScaleTemplateVersion>()
                .AsNoTracking()
                .Where(x => x.Id == versiSekarang.TemplateVersionId)
                .Select(x => x.GlucoseUnit)
                .FirstAsync(cancellationToken);

            var validasi = SlidingScaleRangeValidator.Validate(request.Ranges, satuan);

            if (!validasi.IsValid)
                return Fail(StatusCodes.Status400BadRequest, validasi.ErrorMessage!);

            var episodeStatus = await ReadEpisodeStatusAsync(order.InpEpisodeId, cancellationToken);

            if (episodeStatus is InpEpisodeStatus.Closed or InpEpisodeStatus.Cancelled)
                return Fail(StatusCodes.Status422UnprocessableEntity, "Perawatan pasien ini sudah ditutup.");

            var now = DateTime.UtcNow;

            var doctorId = await ResolveAuthorizedDoctorAsync(user, actorUserId, order.InpEpisodeId, now, cancellationToken);

            if (!doctorId.HasValue)
                return Fail(StatusCodes.Status403Forbidden, PenolakanBukanDokterBerwenang);

            var nomorBaru = order.CurrentVersionNumber + 1;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var version = new PhmSlidingScaleOrderVersion
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                VersionNumber = nomorBaru,
                // Kriteria 4: versi template asal tidak berganti ke versi template terbaru.
                TemplateVersionId = versiSekarang.TemplateVersionId,
                IsAdjusted = true,
                AdjustmentReason = alasan,
                OrderedByDoctorId = doctorId.Value,
                OrderedByUserId = actorUserId,
                OrderedAt = now,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<PhmSlidingScaleOrderVersion>().Add(version);
            _dbContext.Set<PhmSlidingScaleRange>().AddRange(
                SlidingScaleRangeValidator.BuildRows(validasi.OrderedRanges, null, version.Id, actorUserId, now));

            order.CurrentVersionNumber = nomorBaru;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Dua dokter menyesuaikan bersamaan: index unik (OrderId, VersionNumber) menolak yang kedua.
                await transaction.RollbackAsync(cancellationToken);
                return Fail(StatusCodes.Status409Conflict, "Protokol sudah diubah dokter lain. Muat ulang lalu periksa kembali.");
            }

            await _loggerService.InfoAsync(LogCategory, "SlidingScaleOrder.Adjust",
                "Dokter menyesuaikan protokol sliding scale.",
                new { order.Id, order.OrderNumber, version.VersionNumber, version.OrderedByDoctorId });

            return await ReloadAsync(order.Id, "Protokol sliding scale berhasil disesuaikan.", StatusCodes.Status200OK, cancellationToken);
        }

        // =====================================================================
        // Menghentikan
        // =====================================================================

        /// <summary>
        /// Menghentikan order; pelaksanaan berikutnya ditolak — <c>FR-DOK-099</c>.
        /// </summary>
        public async Task<SlidingScaleResult<SlidingScaleOrderResponse>> StopAsync(
            Guid orderId,
            string? reason,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var alasan = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

            if (alasan == null)
                return Fail(StatusCodes.Status400BadRequest, "Alasan penghentian wajib diisi.");

            var order = await _dbContext.Set<PhmSlidingScaleOrder>()
                .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);

            if (order == null)
                return Fail(StatusCodes.Status404NotFound, "Order sliding scale tidak ditemukan.");

            if (order.OrderStatus == SlidingScaleOrderStatus.Stopped)
                return Fail(StatusCodes.Status409Conflict, "Protokol sudah dihentikan. Pesan protokol baru bila diperlukan.");

            var now = DateTime.UtcNow;

            var doctorId = await ResolveAuthorizedDoctorAsync(user, actorUserId, order.InpEpisodeId, now, cancellationToken);

            if (!doctorId.HasValue)
                return Fail(StatusCodes.Status403Forbidden, PenolakanBukanDokterBerwenang);

            ApplyStop(order, alasan, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "SlidingScaleOrder.Stop",
                "Dokter menghentikan protokol sliding scale.",
                new { order.Id, order.OrderNumber, StoppedBy = actorUserId });

            return await ReloadAsync(order.Id, "Protokol sliding scale berhasil dihentikan.", StatusCodes.Status200OK, cancellationToken);
        }

        /// <summary>
        /// Menghentikan order milik satu butir insulin yang dihentikan — state matrix 0.6.0 bagian 8.4
        /// dan 8.7 baris "butir insulinnya dihentikan".
        /// </summary>
        /// <remarks>
        /// <b>TIDAK menyimpan.</b> Dipanggil jalur penghentian butir resep (<c>BE-RWI-100</c>) di dalam
        /// transaksinya, sehingga butir dan protokolnya berhenti bersama atau tidak sama sekali.
        /// Mengembalikan jumlah order yang dihentikan (0 atau 1).
        /// </remarks>
        public async Task<int> StopForPrescriptionItemAsync(
            Guid prescriptionItemId,
            string reason,
            Guid actorUserId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            var orders = await _dbContext.Set<PhmSlidingScaleOrder>()
                .Where(x => x.PrescriptionItemId == prescriptionItemId &&
                            !x.IsDelete &&
                            x.OrderStatus == SlidingScaleOrderStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var order in orders)
                ApplyStop(order, reason, actorUserId, nowUtc);

            return orders.Count;
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        private static void ApplyStop(PhmSlidingScaleOrder order, string reason, Guid actorUserId, DateTime now)
        {
            order.OrderStatus = SlidingScaleOrderStatus.Stopped;
            order.StoppedAt = now;
            order.StoppedByUserId = actorUserId;
            order.StopReason = reason.Length > 500 ? reason[..500] : reason;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;
        }

        /// <summary>
        /// Dokter berwenang menulis resep rawat inap: dokter tertaut akun login yang punya penugasan
        /// aktif pada episode (<c>RWI-DEC-099</c>). Tidak membaca nama peran.
        /// </summary>
        private async Task<Guid?> ResolveAuthorizedDoctorAsync(
            ClaimsPrincipal? user,
            Guid actorUserId,
            Guid episodeId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);

            if (!doctorId.HasValue)
                return null;

            var bertugas = await _clinicalContextService.IsDoctorAssignedAsync(
                episodeId, doctorId.Value, now, cancellationToken);

            return bertugas ? doctorId : null;
        }

        private async Task<InpEpisodeStatus?> ReadEpisodeStatusAsync(Guid episodeId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => (InpEpisodeStatus?)x.EpisodeStatus)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private Task<PhmSlidingScaleOrder?> LoadOrderAsync(Guid orderId, CancellationToken cancellationToken)
        {
            return _dbContext.Set<PhmSlidingScaleOrder>()
                .AsNoTracking()
                .Include(x => x.PrescriptionItem)
                .Include(x => x.Template)
                .Include(x => x.Versions).ThenInclude(v => v.Ranges)
                .Include(x => x.Versions).ThenInclude(v => v.TemplateVersion)
                .Include(x => x.Versions).ThenInclude(v => v.OrderedByDoctor)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken);
        }

        private async Task<SlidingScaleResult<SlidingScaleOrderResponse>> ReloadAsync(
            Guid orderId,
            string message,
            int statusCode,
            CancellationToken cancellationToken)
        {
            var order = await LoadOrderAsync(orderId, cancellationToken);

            return order == null
                ? Fail(StatusCodes.Status404NotFound, "Order sliding scale tidak ditemukan.")
                : SlidingScaleResult<SlidingScaleOrderResponse>.Ok(ToResponse(order), message, statusCode);
        }

        private static SlidingScaleResult<SlidingScaleOrderResponse> Fail(int statusCode, string message)
            => SlidingScaleResult<SlidingScaleOrderResponse>.Fail(statusCode, message);

        private static SlidingScaleOrderResponse ToResponse(PhmSlidingScaleOrder x)
        {
            var versions = x.Versions
                .Where(v => !v.IsDelete)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new SlidingScaleOrderVersionResponse
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    TemplateVersionId = v.TemplateVersionId,
                    TemplateVersionNumber = v.TemplateVersion?.VersionNumber ?? 0,
                    GlucoseUnit = v.TemplateVersion?.GlucoseUnit ?? default,
                    IsAdjusted = v.IsAdjusted,
                    AdjustmentReason = v.AdjustmentReason,
                    OrderedByDoctorId = v.OrderedByDoctorId,
                    OrderedByDoctorName = v.OrderedByDoctor?.FullName,
                    OrderedByUserId = v.OrderedByUserId,
                    OrderedAt = v.OrderedAt,
                    Ranges = v.Ranges
                        .Where(r => !r.IsDelete)
                        .OrderBy(r => r.SortOrder)
                        .Select(SlidingScaleTemplateService.ToRangeResponse)
                        .ToList()
                })
                .ToList();

            var response = new SlidingScaleOrderResponse
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                PrescriptionId = x.PrescriptionId,
                PrescriptionItemId = x.PrescriptionItemId,
                DrugNameSnapshot = x.PrescriptionItem?.DrugNameSnapshot,
                EncounterId = x.EncounterId,
                InpEpisodeId = x.InpEpisodeId,
                PatientId = x.PatientId,
                TemplateId = x.TemplateId,
                TemplateName = x.Template?.TemplateName,
                OrderStatus = x.OrderStatus,
                CurrentVersionNumber = x.CurrentVersionNumber,
                IsCurrentVersionAdjusted = versions.FirstOrDefault(v => v.VersionNumber == x.CurrentVersionNumber)?.IsAdjusted ?? false,
                CheckFrequencyCode = x.CheckFrequencyCode,
                CreateDateTime = x.CreateDateTime,
                StoppedAt = x.StoppedAt,
                StoppedByUserId = x.StoppedByUserId,
                StopReason = x.StopReason,
                Versions = versions
            };

            if (x.OrderStatus == SlidingScaleOrderStatus.Active)
                response.AvailableActions.AddRange(new[] { "Adjust", "Stop" });

            return response;
        }
    }
}
