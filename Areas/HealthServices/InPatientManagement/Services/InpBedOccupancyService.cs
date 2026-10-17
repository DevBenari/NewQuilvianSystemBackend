using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Memesan, menempatkan, memindahkan, dan melepas tempat tidur; menghitung kedaluwarsa
    /// pemesanan saat dibaca; dan memperbarui salinan status pada <c>MstBed</c>.
    /// </summary>
    /// <remarks>
    /// <b>Satu daftar aturan, dipakai seluruh jalur.</b> Penempatan, perpindahan, dan
    /// penyaringan pencarian tempat tidur memanggil <see cref="EvaluatePlacementEligibilityAsync"/>
    /// yang sama. Menulis daftar aturan kedua khusus perpindahan adalah kesalahan yang paling
    /// mahal di modul ini: dua daftar akan berselisih dalam hitungan minggu, dan jalur
    /// perpindahan justru yang paling sering dipakai petugas yang sedang terburu-buru.
    ///
    /// <para>
    /// <b>Tiga lapis penjagaan <c>INV-INP-02</c>.</b> Pemeriksaan "tempat tidur kosong" di
    /// dalam kode <b>tidak cukup</b>: dua transaksi dapat sama-sama lolos pemeriksaan sebelum
    /// salah satunya menyimpan. Karena itu ada tiga lapis — penguncian baris <c>MstBed</c> di
    /// dalam transaksi, unique index parsial pada penempatan aktif, dan unique index parsial
    /// pada pemesanan aktif. Lapis pertama membuat permintaan kedua menunggu; lapis kedua dan
    /// ketiga adalah jaring pengaman terakhirnya.
    /// </para>
    ///
    /// <para>
    /// <b>Kedaluwarsa pemesanan dihitung saat dibaca.</b> Tidak ada program penjadwal.
    /// Pemesanan yang lewat batas digugurkan pada saat seseorang membaca ketersediaan tempat
    /// tidur, memesan, atau menempatkan pasien — <c>RWI-DEC-007</c>.
    /// </para>
    /// </remarks>
    public partial class InpBedOccupancyService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpBedMutationCoordinator _bedCoordinator;
        private readonly InpBedAccessService _bedAccess;
        private readonly InpBedManagementQueryService _bedQueries;
        private readonly InpBedClassComparisonService _bedClasses;
        private readonly InpSettingService _settingService;
        private readonly InpEpisodeService _episodeService;
        private readonly IInpIntegrationOutboxService _outboxService;

        /// <summary>
        /// Dokumen serah terima klinis transfer antarunit, dibuat sesudah commit (<c>BE-RWI-183</c>,
        /// <c>INV-RWF-33</c>).
        /// </summary>
        private readonly CliTransferHandoverService _transferHandoverService;

        private readonly ILogger<InpBedOccupancyService> _logger;

        /// <remarks>
        /// Arah dependency ke <see cref="InpEpisodeService"/> ditetapkan `BE-RWI-011`:
        /// penempatan pasien wajib memindahkan status episode lewat satu-satunya pintu, yaitu
        /// <c>InpEpisodeService.ApplyStatusChangeAsync</c>. Lihat catatan pada konstruktor
        /// <see cref="InpEpisodeService"/> untuk alasan lengkapnya.
        /// </remarks>
        public InpBedOccupancyService(
            ApplicationDbContext dbContext,
            InpSettingService settingService,
            InpEpisodeService episodeService,
            IInpIntegrationOutboxService outboxService,
            CliTransferHandoverService transferHandoverService,
            ILogger<InpBedOccupancyService> logger, InpBedMutationCoordinator bedCoordinator, InpBedAccessService bedAccess, InpBedManagementQueryService bedQueries, InpBedClassComparisonService bedClasses)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _episodeService = episodeService;
            _outboxService = outboxService;
            _transferHandoverService = transferHandoverService;
            _logger = logger; _bedCoordinator = bedCoordinator; _bedAccess = bedAccess; _bedQueries = bedQueries; _bedClasses = bedClasses;
        }

        // =====================================================================
        // BE-RWI-010 — Pencarian, papan ketersediaan, dan pemesanan
        // =====================================================================

        /// <summary>
        /// Mencari tempat tidur yang benar-benar dapat ditempati.
        /// </summary>
        /// <remarks>
        /// Bila <c>EpisodeId</c> diisi, hasilnya disaring memakai <b>seluruh</b> aturan
        /// Kelayakan Penempatan milik episode itu. Penyaring dan penolak wajib memberi
        /// jawaban yang sama: tempat tidur yang muncul di sini tidak boleh ditolak saat
        /// petugas menekan simpan, dan sebaliknya.
        /// </remarks>
        public async Task<AvailableBedPagedResult> SearchAvailableBedsAsync(
            AvailableBedQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new AvailableBedQuery();
            if (query.EpisodeId.HasValue) await _bedAccess.RequireEpisodeAsync(query.EpisodeId.Value, "InpatientBedOccupancy:Read", cancellationToken);
            var scopedUnits = await _bedAccess.UnitsAsync("InpatientBedOccupancy:Read", cancellationToken);

            await ExpireDueReservationsAsync(cancellationToken);

            var (pageNumber, pageSize) = InpEpisodeService.NormalizePaging(
                query.PageNumber,
                query.PageSize);

            InpEpisode? episode = null;
            MstPatient? patient = null;

            if (query.EpisodeId.HasValue && query.EpisodeId.Value != Guid.Empty)
            {
                episode = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == query.EpisodeId.Value && !x.IsDelete,
                        cancellationToken);

                if (episode != null)
                {
                    patient = await _dbContext.Set<MstPatient>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == episode.PatientId, cancellationToken);
                }
            }

            if (episode == null && query.PatientId.HasValue)
            {
                patient = await _dbContext.Set<MstPatient>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == query.PatientId.Value && !x.IsDelete && x.IsActive, cancellationToken);
                if (patient == null) throw new InpBedOperationException(422, "PATIENT_NOT_FOUND", "Pasien belum dapat diverifikasi.");
                episode = new InpEpisode
                {
                    PatientId = patient.Id, MotherEpisodeId = query.MotherEpisodeId,
                    NewbornCareMode = query.NewbornCareMode,
                    RequiresNeonatalIntensiveCare = query.RequiresIntensiveCare,
                    RequiresIsolation = query.RequiresIsolation
                };
            }
            var motherRoomId = patient?.IsNewborn == true
                ? await InpNewbornAdmissionRules.FindMotherRoomAsync(_dbContext, episode?.MotherEpisodeId, patient.MotherPatientId, cancellationToken)
                : null;

            var registrationEligibleUnits = _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(ServiceUnitRegistrationRules.InpatientRegistrationEligible)
                .Select(x => x.Id);

            IQueryable<MstBed> bedQuery = _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.IsActive &&
                            x.Room != null &&
                            scopedUnits.Contains(x.Room.ServiceUnitId) &&
                            registrationEligibleUnits.Contains(x.Room.ServiceUnitId));

            if (query.RoomId.HasValue && query.RoomId.Value != Guid.Empty)
            {
                bedQuery = bedQuery.Where(x => x.RoomId == query.RoomId.Value);
            }

            if (query.IsIsolationBed.HasValue)
            {
                bedQuery = bedQuery.Where(x => x.IsIsolationBed == query.IsIsolationBed.Value);
            }

            if (query.IsForNewborn.HasValue)
            {
                bedQuery = bedQuery.Where(x => x.IsForNewborn == query.IsForNewborn.Value);
            }

            // BE-RWI-248: saring tempat tidur khusus ODC dan ruangan ODC.
            if (query.IsOdcBed.HasValue)
            {
                bedQuery = bedQuery.Where(x => x.IsOdcBed == query.IsOdcBed.Value);
            }

            if (query.IsOdcRoom.HasValue)
            {
                bedQuery = bedQuery.Where(x => x.Room != null && x.Room.IsOdcRoom == query.IsOdcRoom.Value);
            }

            // BE-RWI-239: saring satu lantai denah.
            if (!string.IsNullOrWhiteSpace(query.FloorCode))
            {
                var floor = query.FloorCode.Trim().ToLower();
                bedQuery = bedQuery.Where(x => x.Room != null && x.Room.FloorCode != null && x.Room.FloorCode.ToLower() == floor);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();

                bedQuery = bedQuery.Where(x =>
                    x.BedCode.ToLower().Contains(keyword) ||
                    x.BedName.ToLower().Contains(keyword) ||
                    (x.Room != null && x.Room.RoomName.ToLower().Contains(keyword)));
            }

            if (query.ServiceUnitId.HasValue && query.ServiceUnitId.Value != Guid.Empty)
            {
                bedQuery = bedQuery.Where(x =>
                    x.Room != null && x.Room.ServiceUnitId == query.ServiceUnitId.Value);
            }

            if (query.PatientClassId.HasValue && query.PatientClassId.Value != Guid.Empty)
            {
                bedQuery = bedQuery.Where(x =>
                    x.Room != null && x.Room.PatientClassId == query.PatientClassId.Value);
            }

            var candidates = await bedQuery
                .Include(x => x.Room)
                .OrderBy(x => x.Room != null ? x.Room.RoomName : string.Empty)
                .ThenBy(x => x.BedName)
                .ToListAsync(cancellationToken);

            var eligible = new List<MstBed>();
            var ineligible = new List<IneligibleBedResponse>();
            var collectIneligible = query.IncludeIneligible && episode != null;
            Dictionary<Guid, BedOperationalResponse> opMap = [];
            BedProjectionBatch? projection = null;

            if (candidates.Count > 0)
            {
                // BE-RWI-228: kesiapan, pemesanan aktif, penempatan aktif, unit, dan kelas sudah
                // dimuat proyeksi status; dipakai ulang di sini alih-alih dibaca ulang dari database.
                projection = await _bedQueries.ProjectBatchAsync(candidates, cancellationToken);
                opMap = projection.Rows.ToDictionary(x => x.BedId);

                foreach (var candidate in candidates)
                {
                    if (candidate.Room == null || candidate.Room.IsDelete || !candidate.Room.IsActive)
                    {
                        continue;
                    }

                    opMap.TryGetValue(candidate.Id, out var operational);
                    projection.Roots.TryGetValue(candidate.Id, out var root);
                    IReadOnlyList<InpBedReservation>? bedReservations =
                        projection.Reservations.TryGetValue(candidate.Id, out var reservation) ? [reservation] : null;
                    IReadOnlyList<InpBedPlacement>? bedPlacements =
                        projection.Placements.TryGetValue(candidate.Id, out var placement) ? [placement] : null;

                    var evaluation = EvaluatePlacementEligibilityCore(
                        episode,
                        patient,
                        candidate,
                        candidate.Room,
                        InpPlacementContext.Search,
                        operational ?? new BedOperationalResponse(),
                        root,
                        bedReservations,
                        bedPlacements, motherRoomId);

                    if (evaluation.IsEligible)
                    {
                        eligible.Add(candidate);
                        continue;
                    }

                    if (collectIneligible)
                    {
                        ineligible.Add(new IneligibleBedResponse
                        {
                            BedId = candidate.Id,
                            BedCode = candidate.BedCode,
                            BedName = candidate.BedName,
                            BedNumber = candidate.BedNumber,
                            RoomId = candidate.RoomId,
                            RoomCode = candidate.Room.RoomCode,
                            RoomName = candidate.Room.RoomName,
                            Failures = evaluation.Failures
                        });
                    }
                }
            }

            var totalData = eligible.Count;

            var items = eligible
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AvailableBedResponse
                {
                    BedId = x.Id,
                    BedCode = x.BedCode,
                    BedName = x.BedName,
                    BedNumber = x.BedNumber,
                    RoomId = x.RoomId,
                    RoomCode = x.Room?.RoomCode,
                    RoomName = x.Room?.RoomName,
                    ServiceUnitId = x.Room?.ServiceUnitId ?? Guid.Empty,
                    PatientClassId = x.Room?.PatientClassId,
                    BedStatus = (int)x.BedStatus,
                    BedStatusName = x.BedStatus.ToString(),
                    IsForNewborn = x.IsForNewborn,
                    IsIsolationBed = x.IsIsolationBed,
                    IsReservable = x.IsReservable,
                    IsOdcBed = x.IsOdcBed
                })
                .ToList();

            if (projection != null)
            {
                foreach (var item in items)
                {
                    item.ServiceUnitName = projection.Units.TryGetValue(item.ServiceUnitId, out var unit)
                        ? unit.ServiceUnitName
                        : null;

                    if (item.PatientClassId.HasValue)
                    {
                        item.PatientClassName = projection.Classes.TryGetValue(item.PatientClassId.Value, out var patientClass)
                            ? patientClass.PatientClassName
                            : null;
                    }
                }
            }

            Guid? sourceClass = null;
            if (episode != null && items.Any(x => x.PatientClassId.HasValue))
            {
                sourceClass = await _dbContext.Set<InpBedPlacement>().AsNoTracking()
                    .Where(x => x.EpisodeId == episode.Id && x.EndDateTime == null && !x.IsDelete)
                    .Select(x => (Guid?)x.PatientClassId)
                    .SingleOrDefaultAsync(cancellationToken);
            }

            foreach (var item in items)
            {
                if (opMap.TryGetValue(item.BedId, out var op))
                {
                    item.OperationalVersion = op.OperationalVersion; item.CycleId = op.CycleId; item.StatusCode = op.StatusCode;
                    item.StatusLabel = op.StatusLabel; item.CleaningPhase = op.CleaningPhase; item.IsBookable = op.IsBookable;
                }
                if (sourceClass.HasValue && item.PatientClassId.HasValue)
                {
                    item.ComparisonResult = await _bedClasses.CompareAsync(sourceClass.Value, item.PatientClassId.Value, cancellationToken);
                    item.AllowedManualCategories = InpBedClassComparisonService.AllowedCategories(item.ComparisonResult);
                }
            }

            return new AvailableBedPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items,
                Ineligible = ineligible
            };
        }

        /// <summary>
        /// Menyusun papan ketersediaan tempat tidur, dikelompokkan per unit layanan lalu per
        /// kamar.
        /// </summary>
        /// <remarks>
        /// Keadaan penghunian dibaca dari catatan penempatan dan pemesanan, bukan dari salinan
        /// <c>MstBed.BedStatus</c>. Salinan itu tetap ditampilkan supaya selisih antara catatan
        /// dan salinan terlihat orang, bukan supaya dipercaya.
        /// </remarks>
        /// <summary>
        /// Memesan tempat tidur untuk satu episode <c>Draft</c>. Pemesanan mengunci tempat
        /// tidur selama <c>MstInpatientSetting.BedReservationMinutes</c>, lalu gugur sendiri.
        /// </summary>
        /// <remarks>
        /// <b>Batas waktunya dibaca ulang setiap pemesanan.</b> Angka yang baru diubah admin
        /// berlaku pada pemesanan berikutnya tanpa aplikasi dinyalakan ulang — <c>RWI-AC-003</c>.
        /// Pemesanan yang sudah terlanjur dibuat tetap memakai batas yang berlaku saat ia
        /// dibuat, karena <c>ExpiresAt</c> disimpan pada barisnya.
        /// </remarks>
        internal async Task<InpBedOccupancyOperationResult> ReserveBedCoreAsync(
            ReserveBedRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.EpisodeId == Guid.Empty || request.BedId == Guid.Empty)
            {
                return InpBedOccupancyOperationResult.Invalid(
                    "Episode dan tempat tidur wajib dipilih.");
            }

            await ExpireDueReservationsAsync(cancellationToken);

            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(
                    x => x.Id == request.EpisodeId && !x.IsDelete,
                    cancellationToken);

            if (episode == null)
            {
                return InpBedOccupancyOperationResult.NotFound(
                    "Episode rawat inap tidak ditemukan.");
            }

            if (episode.EpisodeStatus != InpEpisodeStatus.Draft)
            {
                return InpBedOccupancyOperationResult.BusinessRuleRejected(
                    "Pemesanan tempat tidur hanya dapat dilakukan sebelum pasien ditempatkan.");
            }

            // BE-RWI-012 — INV-INP-10. Pasien yang sudah aktif dirawat tidak dapat memesan bed baru.
            var present = await _episodeService.FindPresentEpisodeAsync(
                episode.PatientId,
                episode.Id,
                cancellationToken);

            if (present != null)
            {
                return InpBedOccupancyOperationResult.Conflict(
                    present.RejectionMessage,
                    "PATIENT_ALREADY_ADMITTED");
            }

            var context = await LoadBedContextAsync(request.BedId, cancellationToken);

            if (context == null)
            {
                return InpBedOccupancyOperationResult.NotFound("Tempat tidur tidak ditemukan.");
            }

            var patient = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == episode.PatientId, cancellationToken);

            var existingOwnReservation = await _dbContext.Set<InpBedReservation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.EpisodeId == episode.Id &&
                        x.ReservationStatus == InpBedReservationStatus.Active && x.ExpiresAt > DateTime.UtcNow &&
                        !x.IsDelete,
                    cancellationToken);

            if (existingOwnReservation != null && existingOwnReservation.BedId != request.BedId)
            {
                return InpBedOccupancyOperationResult.Conflict(
                    "Episode ini sudah memesan tempat tidur lain. Batalkan dulu pemesanan " +
                    "sebelumnya.");
            }

            if (existingOwnReservation != null)
            {
                return InpBedOccupancyOperationResult.Success(
                    "Tempat tidur ini sudah dipesan untuk episode ini.",
                    reservationId: existingOwnReservation.Id);
            }

            var evaluation = await EvaluatePlacementEligibilityAsync(
                episode,
                patient,
                context.Bed,
                context.Room,
                InpPlacementContext.Reservation,
                cancellationToken);

            if (!evaluation.IsEligible)
            {
                return InpBedOccupancyOperationResult.FromEligibility(evaluation);
            }

            var setting = await _settingService.GetEffectiveSettingAsync(cancellationToken);
            var now = DateTime.UtcNow;

            await using var transaction = await InpBedTransactionLease.BeginAsync(_dbContext, cancellationToken);

            try
            {
                await LockBedRowAsync(context.Bed.Id, cancellationToken);

                var reservation = new InpBedReservation
                {
                    Id = Guid.NewGuid(),
                    EpisodeId = episode.Id,
                    BedId = context.Bed.Id,
                    ReservedAt = now,
                    ExpiresAt = now.AddMinutes(setting.BedReservationMinutes),
                    ReservationStatus = InpBedReservationStatus.Active,
                    ReservedByUserId = actorUserId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<InpBedReservation>().Add(reservation);

                // Draft mengikuti kamar yang benar-benar dipesan, termasuk setelah memilih ulang.
                episode.PatientClassId = ResolveBilledPatientClassId(context.Room, episode);
                episode.UpdateDateTime = now;
                episode.UpdateBy = actorUserId;

                await WriteBedStatusCopyAsync(
                    context.Bed.Id,
                    BedStatus.Reserved,
                    actorUserId,
                    now,
                    cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpBedOccupancyOperationResult.Success(
                    "Tempat tidur berhasil dipesan.",
                    reservationId: reservation.Id);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                return InpBedOccupancyOperationResult.Conflict(
                    "Tempat tidur ini sudah dipesan untuk pasien lain.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>Membatalkan pemesanan sebelum dipakai.</summary>
        internal async Task<InpBedOccupancyOperationResult> CancelReservationCoreAsync(
            Guid reservationId,
            CancelReservationRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var reservation = await _dbContext.Set<InpBedReservation>()
                .FirstOrDefaultAsync(x => x.Id == reservationId && !x.IsDelete, cancellationToken);

            if (reservation == null)
            {
                return InpBedOccupancyOperationResult.NotFound("Pemesanan tidak ditemukan.");
            }

            if (reservation.ReservationStatus != InpBedReservationStatus.Active)
            {
                return InpBedOccupancyOperationResult.Conflict(
                    "Pemesanan ini sudah tidak berlaku, sehingga tidak dapat dibatalkan lagi.");
            }

            var now = DateTime.UtcNow;

            await using var transaction = await InpBedTransactionLease.BeginAsync(_dbContext, cancellationToken);

            try
            {
                reservation.CancellationReason = request!.Reason!.Trim(); reservation.CancelledByUserId = actorUserId; reservation.ReservationStatus = InpBedReservationStatus.Cancelled;
                reservation.ReleasedAt = now;
                reservation.IsActive = false;
                reservation.UpdateDateTime = now;
                reservation.UpdateBy = actorUserId;

                await ReleaseBedStatusCopyAsync(
                    reservation.BedId,
                    reservation.EpisodeId,
                    actorUserId,
                    now,
                    cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpBedOccupancyOperationResult.Success(
                    "Pemesanan berhasil dibatalkan.",
                    reservationId: reservation.Id);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Menggugurkan seluruh pemesanan yang sudah melewati batas waktunya, lalu
        /// mengembalikan salinan status tempat tidurnya bila memang sudah tidak dipegang
        /// siapa pun.
        /// </summary>
        /// <remarks>
        /// <b>Contoh berangka.</b> Sdri. Wati memesan <c>BD-RSMMC-00042</c> pukul 09:15 dengan
        /// batas 2 jam. Pembacaan pukul 11:14 masih menemukannya terkunci; pembacaan pukul
        /// 11:16 menemukannya bebas. Tidak ada satu pun proses yang berjalan di antara kedua
        /// pembacaan itu — yang menggugurkannya adalah pembacaan kedua itu sendiri.
        /// </remarks>
        public Task<int> ExpireDueReservationsAsync(CancellationToken cancellationToken = default)
        {
            // Core di dalam coordinator sudah mengevaluasi bed yang terlibat; GET memakai akun sistem sah.
            return _dbContext.Database.CurrentTransaction != null ? Task.FromResult(0) : _bedCoordinator.ExpireScopedAsync("InpatientBedOccupancy:Read", cancellationToken);
        }
        // =====================================================================
        // BE-RWI-011 — Penempatan pasien
        // =====================================================================

        /// <summary>
        /// Menempatkan pasien ke tempat tidur dan mengaktifkan episodenya.
        /// </summary>
        /// <remarks>
        /// Seluruh perubahan berada di dalam <b>satu</b> transaksi: baris penempatan,
        /// pemesanan yang dipakai menjadi <c>Consumed</c>, salinan <c>MstBed.BedStatus</c>
        /// menjadi <c>Occupied</c>, status episode menjadi <c>Admitted</c>, dan baris riwayat
        /// statusnya. Bila salah satu gagal — termasuk penulisan salinan status — tidak ada
        /// satu pun yang tersimpan dan episode tetap <c>Draft</c>.
        ///
        /// <para>
        /// <b>Penolakan tidak menghapus isian admisi.</b> Ketika tempat tidur ternyata sudah
        /// diambil pasien lain, yang gagal hanya penempatannya. Episode tetap <c>Draft</c>
        /// dengan seluruh isiannya utuh, dan petugas cukup memilih tempat tidur lain.
        /// </para>
        ///
        /// <para>
        /// <b>Waktu mulai penempatan</b> adalah waktu penempatan dibuat, untuk jalur datang
        /// langsung dan poliklinik (<c>RWI-AC-147</c>). Jalur serah terima IGD membaca waktu
        /// tiba dari catatan kepergian IGD; jalur itu <c>INP-S09</c> yang di luar scope
        /// revisi ini, dan kolom <c>RegPatientEncounter.OriginEncounterId</c> yang menjadi
        /// syaratnya belum ada pada source hari ini.
        /// </para>
        /// </remarks>
        internal async Task<InpBedOccupancyOperationResult> PlacePatientCoreAsync(
            PlacePatientRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.EpisodeId == Guid.Empty || request.BedId == Guid.Empty)
            {
                return InpBedOccupancyOperationResult.Invalid(
                    "Episode dan tempat tidur wajib dipilih.");
            }

            await ExpireDueReservationsAsync(cancellationToken);

            var episode = await _dbContext.Set<InpEpisode>()
                .Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(
                    x => x.Id == request.EpisodeId && !x.IsDelete,
                    cancellationToken);

            if (episode == null)
            {
                return InpBedOccupancyOperationResult.NotFound(
                    "Episode rawat inap tidak ditemukan.");
            }

            if (episode.EpisodeStatus != InpEpisodeStatus.Draft)
            {
                return InpBedOccupancyOperationResult.Conflict(
                    episode.EpisodeStatus == InpEpisodeStatus.Cancelled
                        ? "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan."
                        : "Pasien sudah ditempatkan sebelumnya.");
            }

            // BE-RWI-012 — INV-INP-10. Diperiksa sebelum penempatan, dan kalimat penolakannya
            // menyebut nomor episode beserta lokasi yang sedang ditempati.
            var present = await _episodeService.FindPresentEpisodeAsync(
                episode.PatientId,
                episode.Id,
                cancellationToken);

            if (present != null)
            {
                return InpBedOccupancyOperationResult.Conflict(
                    present.RejectionMessage,
                    "PATIENT_ALREADY_ADMITTED");
            }

            var context = await LoadBedContextAsync(request.BedId, cancellationToken);

            if (context == null)
            {
                return InpBedOccupancyOperationResult.NotFound("Tempat tidur tidak ditemukan.");
            }

            var patient = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == episode.PatientId, cancellationToken);

            if (patient?.IsNewborn == true && !await _dbContext.Set<InpAdmissionDocument>().AsNoTracking()
                .AnyAsync(d => d.EpisodeId == episode.Id && !d.IsDelete &&
                    d.DocumentType == InpAdmissionDocumentType.GeneralConsent && d.Status == InpAdmissionDocumentStatus.Completed &&
                    d.Signatures.Any(s => !s.IsDelete && s.Slot == InpAdmissionSignatureSlot.PatientOrFamily &&
                        (s.SignerRelationship == InpAdmissionPartyRelationship.Parent || s.SignerRelationship == InpAdmissionPartyRelationship.Guardian)), cancellationToken))
                return InpBedOccupancyOperationResult.BusinessRuleRejected("Persetujuan orang tua atau wali bayi harus tersimpan sebelum mengaktifkan penempatan.", "NEWBORN_CONSENT_REQUIRED");

            var evaluation = await EvaluatePlacementEligibilityAsync(
                episode,
                patient,
                context.Bed,
                context.Room,
                InpPlacementContext.Placement,
                cancellationToken);

            if (!evaluation.IsEligible)
            {
                return InpBedOccupancyOperationResult.FromEligibility(evaluation);
            }

            var now = DateTime.UtcNow;

            await using var transaction = await InpBedTransactionLease.BeginAsync(_dbContext, cancellationToken);

            try
            {
                // Lapis 1 dari tiga lapis INV-INP-02. Permintaan kedua menunggu di sini
                // sampai yang pertama selesai, lalu membaca keadaan yang sudah berubah.
                await LockBedRowAsync(context.Bed.Id, cancellationToken);

                // Keadaan tempat tidur diperiksa ULANG di dalam transaksi, bukan hanya saat
                // pemesanan. Tanpa pemeriksaan ulang ini, penguncian baris tidak ada gunanya.
                var takenByOther = await _dbContext.Set<InpBedPlacement>()
                    .AnyAsync(
                        x =>
                            x.BedId == context.Bed.Id &&
                            x.EpisodeId != episode.Id &&
                            x.EndDateTime == null &&
                            !x.IsDelete,
                        cancellationToken);

                if (takenByOther)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return InpBedOccupancyOperationResult.Conflict(
                        BedTakenMessage(context.Bed.BedCode));
                }

                var reservation = await _dbContext.Set<InpBedReservation>()
                    .FirstOrDefaultAsync(
                        x =>
                            x.BedId == context.Bed.Id &&
                            x.EpisodeId == episode.Id &&
                            x.ReservationStatus == InpBedReservationStatus.Active && x.ExpiresAt > DateTime.UtcNow &&
                            !x.IsDelete,
                        cancellationToken);

                if (reservation != null)
                {
                    reservation.ReservationStatus = InpBedReservationStatus.Consumed;
                    reservation.ReleasedAt = now;
                    reservation.IsActive = false;
                    reservation.UpdateDateTime = now;
                    reservation.UpdateBy = actorUserId;
                }

                var lastSequence = await _dbContext.Set<InpBedPlacement>()
                    .Where(x => x.EpisodeId == episode.Id)
                    .Select(x => (int?)x.SequenceNumber)
                    .MaxAsync(cancellationToken) ?? 0;

                var placement = new InpBedPlacement
                {
                    Id = Guid.NewGuid(),
                    EpisodeId = episode.Id,
                    BedId = context.Bed.Id,
                    RoomId = context.Room.Id,
                    ServiceUnitId = context.Room.ServiceUnitId,
                    PatientClassId = ResolveBilledPatientClassId(context.Room, episode),
                    SequenceNumber = lastSequence + 1,
                    StartDateTime = now,
                    EndDateTime = null,
                    PlacedByUserId = actorUserId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                placement.LocationSnapshotJson = await _bedQueries.SnapshotAsync(placement, cancellationToken); await _bedCoordinator.InvalidateAsync(placement.BedId, actorUserId, null, cancellationToken);
                _dbContext.Set<InpBedPlacement>().Add(placement);

                await WriteBedStatusCopyAsync(
                    context.Bed.Id,
                    BedStatus.Occupied,
                    actorUserId,
                    now,
                    cancellationToken);

                episode.AdmittedAt = now;

                await _episodeService.ApplyStatusChangeAsync(
                    episode,
                    fromStatus: InpEpisodeStatus.Draft,
                    toStatus: InpEpisodeStatus.Admitted,
                    actionType: ActionPlacePatient,
                    actorType: InpStatusChangeActorType.User,
                    changedByUserId: actorUserId,
                    reason: null,
                    now: now,
                    touchEpisode: true,
                    cancellationToken: cancellationToken);

                // BE-RWI-130 / RWI-DEC-156 — Event BED_OCCUPIED saat bed fisik ditempati.
                // BE-RWI-151 / INV-RWF-05: ruang, bed, kelas, dan waktu hunian tidak ikut di
                // pesan; Billing membaca linimasa penempatan langsung.
                await _outboxService.EnqueueEventAsync(
                    eventType: "BED_OCCUPIED",
                    episodeId: episode.Id,
                    encounterId: episode.EncounterId,
                    sourceType: "ROOM_STAY",
                    sourceId: placement.Id,
                    version: placement.Version,
                    occurredAtUtc: placement.StartDateTime,
                    cancellationToken: cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpBedOccupancyOperationResult.Success(
                    "Pasien berhasil ditempatkan.",
                    placementId: placement.Id);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                // Lapis 2 dan 3 dari INV-INP-02. Dua transaksi sama-sama lolos pemeriksaan
                // lalu unique index parsial menolak yang kalah. Tepat satu baris penempatan
                // aktif tersimpan, dan episode yang kalah tetap Draft dengan isian utuh.
                return InpBedOccupancyOperationResult.Conflict(
                    BedTakenMessage(context.Bed.BedCode));
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        // =====================================================================
        // BE-RWI-019 — Perpindahan pasien
        // =====================================================================

        /// <summary>
        /// Memindahkan pasien ke tempat tidur lain: menutup penempatan lama dan membuka
        /// penempatan baru dalam satu tindakan utuh.
        /// </summary>
        /// <param name="actorDoctorId">
        /// Identitas dokter pemohon, dibaca dari klaim <c>doctor_id</c>. Kosong bila pemohon
        /// bukan dokter.
        /// </param>
        /// <remarks>
        /// <b><c>GUARD-INP-01</c> hanya berlaku untuk pemohon berperan dokter.</b> Kepala
        /// ruangan, perawat pelaksana, dan supervisor tetap boleh memindahkan tanpa menjadi
        /// DPJP — itu <c>RWI-DEC-012</c> yang tidak dicabut, dan risikonya sudah diterima
        /// secara sadar sebagai <c>RWI-RISK-001</c>. Yang ditolak adalah dokter yang bukan
        /// DPJP aktif episode itu, dan tidak ada kolom keterangan yang dapat dipakai
        /// melewatinya.
        ///
        /// <para>
        /// <b>Kedelapan aturan dipanggil utuh.</b> Perpindahan memakai
        /// <see cref="EvaluatePlacementEligibilityAsync"/> yang sama persis dengan penempatan,
        /// sehingga kode dan kalimat penolakannya identik. Tidak ada daftar aturan kedua.
        /// </para>
        ///
        /// <para>
        /// <b>Kelas yang ditagihkan mengikuti kamar tujuan</b> (<c>RWI-DEC-013</c>), sehingga
        /// riwayat penempatan dapat menunjukkan 2 hari kelas 2 lalu 2 hari kelas 1.
        /// </para>
        /// </remarks>
        /// <param name="comparisonResult">
        /// Hasil perbandingan kelas resmi yang sudah divalidasi pemanggil, termasuk <c>Special</c>
        /// untuk kelas khusus (BE-RWI-233); disimpan pada snapshot lokasi tujuan.
        /// </param>
        internal async Task<InpBedOccupancyOperationResult> TransferCoreAsync(
            TransferPatientRequest request,
            Guid actorUserId,
            Guid? actorDoctorId,
            CancellationToken cancellationToken = default,
            string? comparisonResult = null)
        {
            if (request == null || request.EpisodeId == Guid.Empty || request.TargetBedId == Guid.Empty)
            {
                return InpBedOccupancyOperationResult.Invalid(
                    "Episode dan tempat tidur tujuan wajib dipilih.");
            }

            // VAL-INT-003 / RWI-DEC-157 — Alasan perubahan kamar wajib minimal 10 karakter
            if (string.IsNullOrWhiteSpace(request.TransferReason) ||
                request.TransferReason.Trim().Length < 10)
            {
                return InpBedOccupancyOperationResult.Invalid(
                    "Alasan perubahan kamar wajib diisi minimal 10 karakter untuk keperluan jejak rekam audit.");
            }

            await ExpireDueReservationsAsync(cancellationToken);

            var episode = await _dbContext.Set<InpEpisode>()
                .Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(
                    x => x.Id == request.EpisodeId && !x.IsDelete,
                    cancellationToken);

            if (episode == null)
            {
                return InpBedOccupancyOperationResult.NotFound(
                    "Episode rawat inap tidak ditemukan.");
            }

            // VAL-INT-002 / RWI-DEC-157 — Validasi status billing kasir: tolak jika CLOSED
            var isBillingClosed = await _dbContext.BilFolios
                .AsNoTracking()
                .AnyAsync(
                    x => x.EncounterId == episode.EncounterId &&
                         x.Status == BillingFolioStatus.Closed &&
                         !x.IsDelete,
                    cancellationToken);

            if (isBillingClosed)
            {
                return InpBedOccupancyOperationResult.BusinessRuleRejected(
                    "Mutasi kamar ditolak: Tagihan kasir pasien sudah berstatus CLOSED. Data hunian kamar tidak dapat diubah kembali. Hubungi bagian Kasir/Keuangan bila diperlukan pembukaan kembali tagihan.");
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.DischargePending)
            {
                return InpBedOccupancyOperationResult.BusinessRuleRejected(
                    "Pasien sudah diputuskan boleh pulang, sehingga tidak dapat dipindahkan lagi.");
            }

            if (episode.EpisodeStatus != InpEpisodeStatus.Admitted)
            {
                return InpBedOccupancyOperationResult.BusinessRuleRejected(
                    "Perpindahan hanya dapat dilakukan selama pasien masih dirawat.");
            }

            if (episode.PhysicallyLeftAt.HasValue)
            {
                return InpBedOccupancyOperationResult.BusinessRuleRejected(
                    "Pasien sudah tercatat meninggalkan ruangan, sehingga tidak dapat dipindahkan.");
            }

            // GUARD-INP-01.
            var isDoctor = actorDoctorId.HasValue && actorDoctorId.Value != Guid.Empty;

            if (isDoctor &&
                !await _episodeService.IsActiveDoctorAsync(episode.Id, actorDoctorId, cancellationToken))
            {
                return InpBedOccupancyOperationResult.Forbidden(
                    "Hanya DPJP episode ini yang dapat memindahkan pasien. Alihkan tanggung " +
                    "jawab DPJP lebih dulu bila diperlukan.");
            }

            var currentPlacement = await _dbContext.Set<InpBedPlacement>()
                .FirstOrDefaultAsync(
                    x => x.EpisodeId == episode.Id && x.EndDateTime == null && !x.IsDelete,
                    cancellationToken);

            if (currentPlacement == null)
            {
                return InpBedOccupancyOperationResult.Conflict(
                    "Pasien belum menempati tempat tidur mana pun, sehingga belum dapat " +
                    "dipindahkan.");
            }

            if (currentPlacement.BedId == request.TargetBedId)
            {
                return InpBedOccupancyOperationResult.Invalid(
                    "Tempat tidur tujuan sama dengan tempat tidur saat ini.");
            }

            var context = await LoadBedContextAsync(request.TargetBedId, cancellationToken);

            if (context == null)
            {
                return InpBedOccupancyOperationResult.NotFound(
                    "Tempat tidur tujuan tidak ditemukan.");
            }

            var patient = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == episode.PatientId, cancellationToken);

            var evaluation = await EvaluatePlacementEligibilityAsync(
                episode,
                patient,
                context.Bed,
                context.Room,
                InpPlacementContext.Transfer,
                cancellationToken);

            if (!evaluation.IsEligible)
            {
                return InpBedOccupancyOperationResult.FromEligibility(evaluation);
            }

            var now = DateTime.UtcNow;
            var reason = request.TransferReason.Trim();
            var previousBedId = currentPlacement.BedId;

            await using var transaction = await InpBedTransactionLease.BeginAsync(_dbContext, cancellationToken);

            try
            {
                await LockBedRowAsync(context.Bed.Id, cancellationToken);

                var takenByOther = await _dbContext.Set<InpBedPlacement>()
                    .AnyAsync(
                        x =>
                            x.BedId == context.Bed.Id &&
                            x.EpisodeId != episode.Id &&
                            x.EndDateTime == null &&
                            !x.IsDelete,
                        cancellationToken);

                if (takenByOther)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return InpBedOccupancyOperationResult.Conflict(
                        BedTakenMessage(context.Bed.BedCode));
                }

                currentPlacement.EndDateTime = now;
                currentPlacement.EndReason = InpBedPlacementEndReason.Transfer;
                currentPlacement.EndedByUserId = actorUserId;
                currentPlacement.TransferReason = reason;
                currentPlacement.ChangeReason = reason;
                currentPlacement.IsSuperseded = true;
                currentPlacement.SupersededAtUtc = now;
                currentPlacement.IsActive = false;
                currentPlacement.UpdateDateTime = now;
                currentPlacement.UpdateBy = actorUserId;

                var lastSequence = await _dbContext.Set<InpBedPlacement>()
                    .Where(x => x.EpisodeId == episode.Id)
                    .Select(x => (int?)x.SequenceNumber)
                    .MaxAsync(cancellationToken) ?? 0;

                var placement = new InpBedPlacement
                {
                    Id = Guid.NewGuid(),
                    EpisodeId = episode.Id,
                    BedId = context.Bed.Id,
                    RoomId = context.Room.Id,
                    ServiceUnitId = context.Room.ServiceUnitId,
                    PatientClassId = ResolveBilledPatientClassId(context.Room, episode),
                    SequenceNumber = lastSequence + 1,
                    Version = currentPlacement.Version + 1, TransferCategory = (InpBedTransferCategory)request.ManualTransferCategory, TransferFromPlacementId = currentPlacement.Id,
                    StartDateTime = now,
                    EndDateTime = null,
                    TransferReason = reason,
                    ChangeReason = reason,
                    IsSuperseded = false,
                    PlacedByUserId = actorUserId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                placement.LocationSnapshotJson = await _bedQueries.SnapshotAsync(placement, cancellationToken, comparisonResult); await _bedCoordinator.InvalidateAsync(placement.BedId, actorUserId, null, cancellationToken);
                _dbContext.Set<InpBedPlacement>().Add(placement);

                await WriteBedStatusCopyAsync(
                    context.Bed.Id,
                    BedStatus.Occupied,
                    actorUserId,
                    now,
                    cancellationToken);

                await ReleaseBedStatusCopyAsync(
                    previousBedId,
                    episode.Id,
                    actorUserId,
                    now,
                    cancellationToken);

                // Kolom unit layanan dan kelas pada episode sengaja TIDAK diubah. Keduanya
                // merekam pilihan saat admisi dibuka; yang berlaku untuk penagihan adalah
                // kolom pada baris penempatan, yang berbeda periode demi periode. Menimpa
                // kolom pada episode akan menghapus jejak kelas awal dan membuat riwayat
                // "2 hari kelas 2 lalu 2 hari kelas 1" tidak lagi dapat dibaca.
                episode.UpdateDateTime = now;
                episode.UpdateBy = actorUserId;

                // BE-RWI-154 / kontrak integrasi-billing 1.1.0 API 3.4: transfer biasa menerbitkan
                // BED_OCCUPIED untuk penempatan baru. OCCUPANCY_CORRECTED kini hanya milik koreksi
                // salah catat (InpPlacementCorrectionService). Bagi Billing artinya sama: hitung
                // ulang tarif kamar dari linimasa penempatan (RWI-DEC-166 butir 3).
                // BE-RWI-151 / INV-RWF-05: isi pesan hanya penanda kejadian (daftar putih).
                await _outboxService.EnqueueEventAsync(
                    eventType: "BED_OCCUPIED",
                    episodeId: episode.Id,
                    encounterId: episode.EncounterId,
                    sourceType: "ROOM_STAY",
                    sourceId: placement.Id,
                    version: placement.Version,
                    occurredAtUtc: placement.StartDateTime,
                    cancellationToken: cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // BE-RWI-183 / INV-RWF-33: transfer ke unit lain membuat dokumen serah terima klinis
                // SESUDAH commit. Gagal tidak pernah membatalkan transfer yang sudah sah; dokumennya
                // dibuat ulang oleh pengecekan saat daftar serah terima dibaca.
                if (_dbContext.Database.CurrentTransaction == null && currentPlacement.ServiceUnitId != placement.ServiceUnitId)
                {
                    try
                    {
                        await _transferHandoverService.CreateForTransferAsync(
                            episode.Id, currentPlacement.Id, placement.Id, actorUserId, cancellationToken);
                    }
                    catch (Exception exception) when (exception is not OperationCanceledException)
                    {
                        _logger.LogWarning(exception,
                            "Dokumen serah terima transfer untuk episode {EpisodeId} gagal dibuat sesudah transfer; " +
                            "transfer tetap sah dan dokumen akan dibuat ulang saat daftar dibaca.",
                            episode.Id);
                    }
                }

                return InpBedOccupancyOperationResult.Success(
                    "Pasien berhasil dipindahkan.",
                    placementId: placement.Id);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                // Perpindahan gagal seluruhnya. Penempatan lama TIDAK jadi ditutup, sehingga
                // pasien tetap berada di tempat tidur semula — INV-INP-07.
                return InpBedOccupancyOperationResult.Conflict(
                    BedTakenMessage(context.Bed.BedCode));
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        // =====================================================================
        // BE-RWI-154 — Koreksi salah catat penempatan (RWI-DEC-157, RWI-DEC-192 butir g)
        // =====================================================================

        /// <summary>
        /// Menerapkan koreksi salah catat kamar, bed, kelas, atau waktu pada satu penempatan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dipanggil <see cref="InpPlacementCorrectionService"/> <b>sesudah</b> gerbang status invoice
        /// (<c>INT-RWF-03</c>) lolos. Baris lama tidak pernah dihapus atau ditimpa isinya: ia diberi
        /// <c>SupersededByCorrectionId</c>, lalu satu baris baru lahir dengan
        /// <c>CorrectsPlacementId</c> dan <c>Version</c> lama + 1 (kontrak state 5.4). Billing hanya
        /// menghitung baris yang <c>SupersededByCorrectionId</c>-nya kosong (<c>INV-RWF-07</c>),
        /// sehingga tarif kamar tidak dobel dan tidak kurang (<c>RSK-RWF-02</c>).
        /// </para>
        /// <para>
        /// <b>Dua kali simpan di dalam satu transaksi.</b> Bila penempatan yang dikoreksi masih
        /// berjalan, baris lama ditutup lebih dulu supaya index unik "satu bed satu penempatan
        /// aktif" (<c>IX_InpBedPlacement_BedId_Active</c>) tidak menolak baris koreksi pada bed yang
        /// sama, misalnya koreksi kelas saja.
        /// </para>
        /// </remarks>
        internal async Task<InpPlacementCorrectionResult> ApplyPlacementCorrectionCoreAsync(
            Guid placementId,
            CorrectPlacementRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var reason = request.Reason.Trim();
            var now = DateTime.UtcNow;

            var target = await _dbContext.Set<InpBedPlacement>()
                .FirstOrDefaultAsync(x => x.Id == placementId && !x.IsDelete, cancellationToken);

            if (target == null)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.NotFound, "Penempatan tidak ditemukan.");
            }

            // Koreksi selalu dilakukan pada baris yang berlaku (kontrak state 5.4).
            if (target.SupersededByCorrectionId.HasValue || target.Version != request.ExpectedVersion)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Conflict,
                    "Data penempatan sudah diubah pengguna lain. Muat ulang lalu coba lagi.",
                    InpPlacementCorrectionCodes.VersionChanged);
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == target.EpisodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.NotFound, "Episode rawat inap tidak ditemukan.");
            }

            var isActivePlacement = target.EndDateTime == null;

            if (request.CorrectedEndDateTime.HasValue && isActivePlacement)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Invalid,
                    "Waktu selesai hanya dapat dikoreksi pada penempatan yang sudah berakhir.");
            }

            var newStart = request.CorrectedStartDateTime.HasValue
                ? ToUtc(request.CorrectedStartDateTime.Value)
                : target.StartDateTime;
            var newEnd = isActivePlacement
                ? (DateTime?)null
                : request.CorrectedEndDateTime.HasValue
                    ? ToUtc(request.CorrectedEndDateTime.Value)
                    : target.EndDateTime;

            if (newStart > now || (newEnd.HasValue && newEnd.Value > now))
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Invalid,
                    "Waktu koreksi tidak boleh melewati waktu sekarang.");
            }

            if (newEnd.HasValue && newEnd.Value <= newStart)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Invalid,
                    "Waktu selesai penempatan harus sesudah waktu mulainya.");
            }

            // VAL-RWF-12 — tidak mendahului admisi dan tidak menimpa penempatan lain.
            if (episode.AdmittedAt.HasValue && newStart < episode.AdmittedAt.Value)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Invalid,
                    "Waktu koreksi bertabrakan dengan penempatan lain pada episode ini.");
            }

            var siblings = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x =>
                    x.EpisodeId == episode.Id &&
                    x.Id != target.Id &&
                    !x.IsDelete &&
                    x.SupersededByCorrectionId == null)
                .Select(x => new { x.StartDateTime, x.EndDateTime })
                .ToListAsync(cancellationToken);

            var correctedEndForOverlap = newEnd ?? DateTime.MaxValue;
            var overlaps = siblings.Any(x =>
                x.StartDateTime < correctedEndForOverlap &&
                (x.EndDateTime ?? DateTime.MaxValue) > newStart);

            if (overlaps)
            {
                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Invalid,
                    "Waktu koreksi bertabrakan dengan penempatan lain pada episode ini.");
            }

            var newBedId = target.BedId;
            var newRoomId = target.RoomId;
            var newServiceUnitId = target.ServiceUnitId;
            var newPatientClassId = target.PatientClassId;
            var bedChanged = request.CorrectedBedId.HasValue && request.CorrectedBedId.Value != target.BedId;

            if (bedChanged)
            {
                var context = await LoadBedContextAsync(request.CorrectedBedId!.Value, cancellationToken);

                if (context?.Room == null)
                {
                    return InpPlacementCorrectionResult.Fail(
                        InpEpisodeOperationStatus.NotFound, "Tempat tidur hasil koreksi tidak ditemukan.");
                }

                if (isActivePlacement)
                {
                    // VAL-RWF-10 — bed yang sedang ditempati wajib lolos kelayakan yang sama
                    // dengan transfer.
                    var patient = await _dbContext.Set<MstPatient>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == episode.PatientId, cancellationToken);

                    var evaluation = await EvaluatePlacementEligibilityAsync(
                        episode,
                        patient,
                        context.Bed,
                        context.Room,
                        InpPlacementContext.Transfer,
                        cancellationToken);

                    if (!evaluation.IsEligible)
                    {
                        return InpPlacementCorrectionResult.Fail(
                            InpEpisodeOperationStatus.BusinessRuleRejected,
                            evaluation.PrimaryMessage,
                            InpPlacementCorrectionCodes.BedNotEligible,
                            evaluation.Failures.ToList());
                    }
                }
                else
                {
                    // Penempatan yang sudah berakhir: keadaan bed hari ini tidak relevan, tetapi
                    // bed itu tidak boleh tercatat ditempati episode lain pada periode yang sama.
                    var historicalEnd = newEnd ?? DateTime.MaxValue;
                    var takenInPeriod = await _dbContext.Set<InpBedPlacement>()
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.BedId == context.Bed.Id &&
                            x.EpisodeId != episode.Id &&
                            !x.IsDelete &&
                            x.SupersededByCorrectionId == null &&
                            x.StartDateTime < historicalEnd &&
                            (x.EndDateTime == null || x.EndDateTime > newStart),
                            cancellationToken);

                    if (takenInPeriod)
                    {
                        return InpPlacementCorrectionResult.Fail(
                            InpEpisodeOperationStatus.BusinessRuleRejected,
                            "Tempat tidur itu tercatat ditempati pasien lain pada periode yang sama.",
                            InpPlacementCorrectionCodes.BedNotEligible);
                    }
                }

                newBedId = context.Bed.Id;
                newRoomId = context.Room.Id;
                newServiceUnitId = context.Room.ServiceUnitId;
                newPatientClassId = ResolveBilledPatientClassId(context.Room, episode);
            }

            if (request.CorrectedPatientClassId.HasValue)
            {
                var classExists = await _dbContext.Set<MstPatientClass>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.CorrectedPatientClassId.Value && !x.IsDelete, cancellationToken);

                if (!classExists)
                {
                    return InpPlacementCorrectionResult.Fail(
                        InpEpisodeOperationStatus.NotFound, "Kelas perawatan hasil koreksi tidak ditemukan.");
                }

                newPatientClassId = request.CorrectedPatientClassId.Value;
            }

            await using var transaction = await InpBedTransactionLease.BeginAsync(_dbContext, cancellationToken);

            try
            {
                if (bedChanged && isActivePlacement)
                {
                    await LockBedRowAsync(newBedId, cancellationToken);

                    var takenByOther = await _dbContext.Set<InpBedPlacement>()
                        .AnyAsync(
                            x =>
                                x.BedId == newBedId &&
                                x.EpisodeId != episode.Id &&
                                x.EndDateTime == null &&
                                !x.IsDelete,
                            cancellationToken);

                    if (takenByOther)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return InpPlacementCorrectionResult.Fail(
                            InpEpisodeOperationStatus.Conflict,
                            "Tempat tidur hasil koreksi sudah ditempati pasien lain.");
                    }
                }

                // Langkah 1 — tutup baris lama yang masih berjalan lebih dulu (lihat remarks).
                if (isActivePlacement)
                {
                    target.EndDateTime = newStart;
                    target.EndReason = InpBedPlacementEndReason.CorrectedEntry;
                    target.EndedByUserId = actorUserId;
                    target.IsActive = false;
                }

                target.UpdateDateTime = now;
                target.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Langkah 2 — baris koreksi, lalu tautan dua arah.
                var lastSequence = await _dbContext.Set<InpBedPlacement>()
                    .Where(x => x.EpisodeId == episode.Id)
                    .Select(x => (int?)x.SequenceNumber)
                    .MaxAsync(cancellationToken) ?? 0;

                var correction = new InpBedPlacement
                {
                    Id = Guid.NewGuid(),
                    EpisodeId = episode.Id,
                    BedId = newBedId,
                    RoomId = newRoomId,
                    ServiceUnitId = newServiceUnitId,
                    PatientClassId = newPatientClassId,
                    SequenceNumber = lastSequence + 1,
                    StartDateTime = newStart,
                    EndDateTime = newEnd,
                    EndReason = isActivePlacement ? null : target.EndReason,
                    TransferReason = target.TransferReason,
                    PhysicallyLeftAt = target.PhysicallyLeftAt,
                    Version = target.Version + 1,
                    ChangeReason = reason,
                    IsSuperseded = !isActivePlacement && target.IsSuperseded,
                    SupersededAtUtc = !isActivePlacement ? target.SupersededAtUtc : null,
                    CorrectsPlacementId = target.Id,
                    PlacedByUserId = actorUserId,
                    EndedByUserId = isActivePlacement ? null : target.EndedByUserId,
                    IsActive = isActivePlacement,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                correction.TransferCategory = target.TransferCategory;
                correction.TransferFromPlacementId = target.TransferFromPlacementId;
                correction.LocationSnapshotJson = bedChanged || newPatientClassId != target.PatientClassId
                    ? await _bedQueries.SnapshotAsync(correction, cancellationToken) : target.LocationSnapshotJson;
                _dbContext.Set<InpBedPlacement>().Add(correction);
                await _dbContext.SaveChangesAsync(cancellationToken);

                target.SupersededByCorrectionId = correction.Id;

                if (bedChanged && isActivePlacement)
                {
                    await _bedCoordinator.InvalidateAsync(newBedId, actorUserId, null, cancellationToken); await WriteBedStatusCopyAsync(newBedId, BedStatus.Occupied, actorUserId, now, cancellationToken);
                    await ReleaseBedStatusCopyAsync(target.BedId, episode.Id, actorUserId, now, cancellationToken);
                }

                episode.UpdateDateTime = now;
                episode.UpdateBy = actorUserId;

                // OCCUPANCY_CORRECTED kini hanya milik koreksi salah catat (kontrak integrasi 4.2).
                await _outboxService.EnqueueEventAsync(
                    eventType: "OCCUPANCY_CORRECTED",
                    episodeId: episode.Id,
                    encounterId: episode.EncounterId,
                    sourceType: "ROOM_STAY",
                    sourceId: correction.Id,
                    version: correction.Version,
                    occurredAtUtc: now,
                    cancellationToken: cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpPlacementCorrectionResult.Ok(
                    "Penempatan berhasil dikoreksi. Baris lama tetap tersimpan sebagai jejak.",
                    correction.Id);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();

                return InpPlacementCorrectionResult.Fail(
                    InpEpisodeOperationStatus.Conflict,
                    "Data penempatan sudah diubah pengguna lain. Muat ulang lalu coba lagi.",
                    InpPlacementCorrectionCodes.VersionChanged);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        /// <summary>Membaca riwayat penempatan satu episode, dari tempat tidur pertama sampai terakhir.</summary>
        public async Task<List<BedPlacementResponse>> GetPlacementsByEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var allowedUnits = await _bedAccess.UnitsAsync(_bedCoordinator.LastOperationName ?? "InpatientBedOccupancy:Read", cancellationToken); var rows = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && allowedUnits.Contains(x.ServiceUnitId))
                .OrderBy(x => x.SequenceNumber)
                .Select(x => new BedPlacementResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                    BedId = x.BedId,
                    BedCode = x.Bed != null ? x.Bed.BedCode : null,
                    BedName = x.Bed != null ? x.Bed.BedName : null,
                    RoomId = x.RoomId,
                    RoomName = x.Room != null ? x.Room.RoomName : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    SequenceNumber = x.SequenceNumber,
                    StartDateTime = x.StartDateTime,
                    EndDateTime = x.EndDateTime,
                    EndReason = x.EndReason != null ? (int)x.EndReason : null,
                    TransferReason = x.TransferReason,
                    PlacedByUserId = x.PlacedByUserId,
                    EndedByUserId = x.EndedByUserId,
                    IsCurrent = x.EndDateTime == null,
                    // BE-RWI-154 / kontrak integrasi-billing 1.1.0 API 3.4 — transfer dan koreksi
                    // salah catat dapat dibedakan pada riwayat.
                    Version = x.Version, TransferCategory = (int?)x.TransferCategory, TransferFromPlacementId = x.TransferFromPlacementId, LocationSnapshotJson = x.LocationSnapshotJson, ContextSource = x.LocationSnapshotJson == null ? "LegacyStoredIds" : "StoredSnapshot", IsEffectiveForBilling = x.SupersededByCorrectionId == null,
                    CorrectsPlacementId = x.CorrectsPlacementId,
                    SupersededByCorrectionId = x.SupersededByCorrectionId,
                    IsCorrection = x.CorrectsPlacementId != null
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                if (row.LocationSnapshotJson != null)
                {
                    using var snapshot = System.Text.Json.JsonDocument.Parse(row.LocationSnapshotJson);
                    var loc = snapshot.RootElement;
                    string? Read(string name) => loc.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String ? v.GetString() : null;
                    row.BedCode = Read("BedCode"); row.BedName = Read("BedName"); row.RoomName = Read("RoomName");
                    row.ServiceUnitName = Read("ServiceUnitName"); row.PatientClassName = Read("PatientClassName");
                }
                row.EndReasonName = row.EndReason.HasValue
                    ? ((InpBedPlacementEndReason)row.EndReason.Value).ToString()
                    : null;
            }

            return rows;
        }

        /// <summary>Membaca satu baris penempatan sebagai balasan endpoint.</summary>
        public async Task<BedPlacementResponse?> GetPlacementAsync(
            Guid placementId,
            CancellationToken cancellationToken = default)
        {
            var allowedUnits = await _bedAccess.UnitsAsync(_bedCoordinator.LastOperationName ?? "InpatientBedOccupancy:Read", cancellationToken); var rows = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x => x.Id == placementId)
                .Select(x => x.EpisodeId)
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return null;
            }

            var placements = await GetPlacementsByEpisodeAsync(rows[0], cancellationToken);

            return placements.FirstOrDefault(x => x.Id == placementId);
        }

        /// <summary>Membaca satu pemesanan sebagai balasan endpoint.</summary>
        public async Task<BedReservationResponse?> GetReservationAsync(
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            var row = await _dbContext.Set<InpBedReservation>()
                .AsNoTracking()
                .Where(x => x.Id == reservationId)
                .Select(x => new BedReservationResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                    BedId = x.BedId,
                    BedCode = x.Bed != null ? x.Bed.BedCode : null,
                    BedName = x.Bed != null ? x.Bed.BedName : null,
                    RoomId = x.Bed != null ? x.Bed.RoomId : null,
                    RoomName = x.Bed != null && x.Bed.Room != null ? x.Bed.Room.RoomName : null,
                    ReservedAt = x.ReservedAt,
                    ExpiresAt = x.ExpiresAt,
                    ReservationStatus = (int)x.ReservationStatus,
                    ReservedByUserId = x.ReservedByUserId,
                    ReleasedAt = x.ReleasedAt, CancellationReason = x.CancellationReason, CancelledByUserId = x.CancelledByUserId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row != null)
            {
                row.ReservationStatusName =
                    ((InpBedReservationStatus)row.ReservationStatus).ToString();
            }

            return row;
        }

        // =====================================================================
        // Kelayakan Penempatan
        // =====================================================================

        /// <summary>
        /// Memeriksa kelayakan satu tempat tidur bagi satu episode, lalu mengembalikan
        /// <b>daftar aturan yang gagal</b> — bukan sekadar boleh atau tidak.
        /// </summary>
        /// <remarks>
        /// Bentuk daftar ini dipilih sejak revision <c>0.1</c> justru supaya aturan baru dapat
        /// ditambahkan tanpa membongkar perintah penempatan maupun perpindahan. Pada revision
        /// <c>0.3</c> bentuk itu terbukti: lima aturan bertambah dan tidak satu baris pun
        /// perintah bisnisnya berubah.
        ///
        /// <para>
        /// <b>Aturan 4, 5, dan 6 dipensiunkan.</b> Aturan 6 dicabut <c>RWI-DEC-101</c> (kontrak
        /// <c>0.8.0</c>); aturan 4 dan 5 — penanda jenis kelamin tempat tidur — dicabut
        /// <c>RWI-DEC-315</c> (BE-RWI-240, kontrak <c>0.13.0</c>): kamar campur diterima tanpa
        /// peringatan, dan tidak ada jalur bed yang membaca jenis kelamin pasien. Ketiga nomor
        /// <b>dibiarkan kosong</b> dan tidak dipakai ulang, supaya nomor aturan 7 dan 8 pada
        /// <c>failures[]</c> tidak bergeser bagi pemanggil yang sudah terbit. Kedua kode penolakan
        /// jenis kelamin lama dipensiunkan dan tidak pernah dikembalikan lagi (API 14.4).
        /// </para>
        ///
        /// <para>
        /// <b>Aturan 9 tidak pernah menyala pada revisi ini.</b> Ia hanya berlaku bila episode
        /// lahir dari serah terima IGD, yang dikenali dari
        /// <c>RegPatientEncounter.OriginEncounterId</c>. Kolom itu <b>belum ada</b> pada source
        /// hari ini — ia dibuat modul IGD lewat <c>IGD-DEC-075</c> — sehingga aturannya tidak
        /// dapat diperiksa dan memang tidak perlu, karena jalur <c>INP-S09</c> di luar scope.
        /// </para>
        /// </remarks>
        public async Task<InpPlacementEligibilityResult> EvaluatePlacementEligibilityAsync(
            InpEpisode? episode,
            MstPatient? patient,
            MstBed bed,
            MstRoom room,
            InpPlacementContext context,
            CancellationToken cancellationToken = default)
        {
            // BE-RWI-228: kesiapan, pemesanan, dan penempatan diambil dari data proyeksi yang sama.
            var projection = await _bedQueries.ProjectBatchAsync([bed], cancellationToken);
            var operational = projection.Rows[0];
            projection.Roots.TryGetValue(bed.Id, out var root);
            IReadOnlyList<InpBedReservation> reservations =
                projection.Reservations.TryGetValue(bed.Id, out var reservation) ? [reservation] : [];
            IReadOnlyList<InpBedPlacement> placements =
                projection.Placements.TryGetValue(bed.Id, out var placement) ? [placement] : [];

            if (patient == null && episode != null)
            {
                patient = episode.Patient ?? await _dbContext.Set<MstPatient>().AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == episode.PatientId && !p.IsDelete, cancellationToken);
            }

            var motherRoomId = patient?.IsNewborn == true
                ? await InpNewbornAdmissionRules.FindMotherRoomAsync(_dbContext, episode?.MotherEpisodeId, patient.MotherPatientId, cancellationToken)
                : null;
            return EvaluatePlacementEligibilityCore(episode, patient, bed, room, context, operational, root, reservations, placements, motherRoomId);
        }

        internal InpPlacementEligibilityResult EvaluatePlacementEligibilityCore(
            InpEpisode? episode,
            MstPatient? patient,
            MstBed bed,
            MstRoom room,
            InpPlacementContext context,
            BedOperationalResponse operational,
            InpBedReadiness? root,
            IReadOnlyList<InpBedReservation>? bedReservations,
            IReadOnlyList<InpBedPlacement>? bedPlacements,
            Guid? motherRoomId = null)
        {
            var result = new InpPlacementEligibilityResult();
            if (!room.PatientClassId.HasValue || room.PatientClassId.Value == Guid.Empty)
            {
                result.Add(1, "ROOM_CLASS_MISSING", "Kamar belum memiliki kelas perawatan. Lengkapi kelas kamar sebelum memesan atau menempatkan pasien.", 422);
            }
            var episodeId = episode?.Id ?? Guid.Empty;
            var ownReservation = episode != null && (bedReservations?.Any(x => x.EpisodeId == episode.Id) ?? false);
            if (!operational.IsBookable && !(ownReservation && operational.StatusCode == "Reserved" && root?.State == InpBedReadinessState.Ready && operational.ConflictCode == null))
                result.Add(1, "BED_NOT_READY", "Bed belum siap atau masih dipegang. Muat ulang kesiapan bed.", 422);

            // --- Aturan 1: tempat tidur aktif dan tidak sedang ditutup -------------------
            if (bed.IsDelete || !bed.IsActive)
            {
                result.Add(1, "BED_INACTIVE", "Tempat tidur ini sedang tidak aktif.", 422);
            }
            else if (IsBedClosed(bed.BedStatus))
            {
                result.Add(
                    1,
                    "BED_CLOSED",
                    $"Tempat tidur sedang tidak dapat dipakai. Keadaan saat ini: " +
                    $"{BedStatusLabel(bed.BedStatus)}.",
                    422);
            }

            if (context == InpPlacementContext.Reservation && !bed.IsReservable)
            {
                result.Add(1, "BED_NOT_RESERVABLE", "Tempat tidur ini tidak dapat dipesan.", 422);
            }

            // --- Aturan 2 dan 3: tempat tidur tidak dipegang episode lain -----------------
            var otherReservation = bedReservations?.Any(x => x.EpisodeId != episodeId) ?? false;

            if (otherReservation)
            {
                result.Add(
                    2,
                    "BED_RESERVED_BY_OTHER",
                    "Tempat tidur ini sudah dipesan untuk pasien lain.",
                    409);
            }

            var otherPlacement = bedPlacements?.Any(x => x.EpisodeId != episodeId) ?? false;

            if (otherPlacement)
            {
                result.Add(
                    2,
                    "BED_OCCUPIED_BY_OTHER",
                    context == InpPlacementContext.Reservation ||
                    context == InpPlacementContext.Search
                        ? "Tempat tidur ini sedang ditempati pasien lain."
                        : BedTakenMessage(bed.BedCode),
                    409);
            }

            // Aturan 3 bukan penolakan. Pemesanan milik episode ini yang masih berlaku dipakai,
            // dan pemesanan yang sudah gugur tidak menghalangi apa pun — RWI-RULE-015.
            if (episodeId != Guid.Empty)
            {
                result.ReusableReservationExists = ownReservation;
            }

            foreach (var failure in InpNewbornAdmissionRules.ValidateBed(episode, patient, bed, room, motherRoomId))
                result.Add(10, failure.Code, failure.Message, 422);

            // Tanpa episode, pemeriksaan berhenti di sini. Aturan berikutnya menyangkut kebutuhan
            // isolasi pasien, dan itu milik episode.
            if (episode == null)
            {
                return result;
            }

            // Nomor 4, 5, dan 6 dipensiunkan (RWI-DEC-101, RWI-DEC-315) dan sengaja dibiarkan
            // kosong, supaya nomor aturan 7 dan 8 tidak bergeser. Tidak ada aturan jenis kelamin.

            // --- Aturan 7 dan 8: kapasitas isolasi terjaga dua arah ----------------------
            if (episode.RequiresIsolation && !bed.IsIsolationBed)
            {
                result.Add(
                    7,
                    "ISOLATION_REQUIRED",
                    "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada " +
                    "tempat tidur isolasi.",
                    422);
            }

            if (!episode.RequiresIsolation && bed.IsIsolationBed)
            {
                result.Add(
                    8,
                    "ISOLATION_BED_RESERVED",
                    "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi.",
                    422);
            }

            // --- Aturan 9: Tempat tidur khusus ODC terjaga dua arah (BE-RWI-249 / VAL-ADM-10, VAL-ADM-11, VAL-ADM-12) ---
            if (context != InpPlacementContext.Transfer)
            {
                var effectivePatient = patient ?? episode.Patient;
                if (effectivePatient != null)
                {
                    var isOdcPatient = effectivePatient.PatientType == PatientType.OneDayCare ||
                                       (int)effectivePatient.PatientType == 4 ||
                                       (int)effectivePatient.PatientType == 7;

                    if (isOdcPatient && !bed.IsOdcBed)
                    {
                        result.Add(
                            9,
                            "ODC_BED_REQUIRED",
                            "Pasien ODC hanya dapat ditempatkan pada tempat tidur khusus One Day Care (ODC).",
                            422);
                    }
                    else if (!isOdcPatient && bed.IsOdcBed)
                    {
                        result.Add(
                            9,
                            "ODC_BED_RESERVED",
                            "Tempat tidur One Day Care (ODC) khusus diperuntukkan bagi pasien ODC.",
                            422);
                    }
                }
            }

            return result;
        }

        // =====================================================================
        // Pelepasan tempat tidur untuk pemanggil lain
        // =====================================================================

        /// <summary>
        /// Menutup penempatan yang masih aktif milik satu episode dan mengembalikan salinan
        /// status tempat tidurnya.
        /// </summary>
        /// <param name="endReason">
        /// Alasan berakhirnya penempatan. <c>PatientDeparted</c> untuk pencatatan kepergian
        /// fisik, <c>EpisodeClosed</c> untuk penutupan episode.
        /// </param>
        /// <returns>
        /// Identitas tempat tidur yang dilepas, atau <c>null</c> bila episode itu memang sudah
        /// tidak memegang tempat tidur.
        /// </returns>
        /// <remarks>
        /// <b>Method ini tidak membuka transaksi sendiri.</b> Ia ikut transaksi pemanggilnya,
        /// karena pelepasan tempat tidur selalu merupakan bagian dari tindakan yang lebih
        /// besar — pencatatan kepergian atau penutupan episode. Melepas tempat tidur di dalam
        /// transaksi terpisah membuka keadaan setengah jadi yang paling merugikan: tempat
        /// tidur sudah bebas dan diambil pasien lain, sementara tindakan yang menyebabkannya
        /// gagal dan pasien lama masih tercatat di sana.
        ///
        /// <para>
        /// Pemanggil <b>wajib</b> memanggil <c>SaveChangesAsync</c> sendiri.
        /// </para>
        /// </remarks>
        public async Task<Guid?> ReleaseActivePlacementAsync(
            Guid episodeId,
            InpBedPlacementEndReason endReason,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            var placement = await _dbContext.Set<InpBedPlacement>()
                .FirstOrDefaultAsync(
                    x => x.EpisodeId == episodeId && x.EndDateTime == null && !x.IsDelete,
                    cancellationToken);

            if (placement == null)
            {
                return null;
            }

            await _bedCoordinator.LockHierarchyAsync([placement.BedId], null, null, null, cancellationToken);
            await _dbContext.Entry(placement).ReloadAsync(cancellationToken);
            if (placement.EndDateTime != null) return null;
            placement.EndDateTime = now;
            placement.EndReason = endReason;
            placement.EndedByUserId = actorUserId;
            placement.IsActive = false;
            placement.UpdateDateTime = now;
            placement.UpdateBy = actorUserId;

            await ReleaseBedStatusCopyAsync(
                placement.BedId,
                episodeId,
                actorUserId,
                now,
                cancellationToken);

            return placement.BedId;
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>Nilai <c>InpStatusHistory.ActionType</c> untuk penempatan pasien.</summary>
        public const string ActionPlacePatient = "PlacePatient";

        /// <summary>
        /// Mengunci baris <c>MstBed</c> di dalam transaksi yang sedang berjalan.
        /// </summary>
        /// <remarks>
        /// <b>Ini lapis pertama penjagaan <c>INV-INP-02</c>.</b> Tanpa penguncian, dua
        /// permintaan dapat membaca keadaan tempat tidur yang sama lalu sama-sama merasa
        /// boleh; keduanya baru bertabrakan di unique index, dan yang kalah menerima galat
        /// basis data alih-alih kalimat yang dapat dibaca.
        ///
        /// <para>
        /// Penguncian hanya dijalankan pada penyedia relasional. Provider InMemory yang
        /// dipakai test tidak mengenal <c>FOR UPDATE</c> maupun transaksi sungguhan, sehingga
        /// pemanggilan ini dilewati di sana. Konsekuensinya, pembuktian bahwa dua transaksi
        /// bersamaan benar-benar menghasilkan tepat satu penempatan aktif harus dijalankan
        /// terhadap PostgreSQL sungguhan, dan dicatat pada laporan task.
        /// </para>
        /// </remarks>
        private async Task LockBedRowAsync(Guid bedId, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsRelational())
            {
                return;
            }

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT 1 FROM public.\"MstBed\" WHERE \"Id\" = {0} FOR UPDATE",
                new object[] { bedId },
                cancellationToken);
        }

        /// <summary>
        /// Menulis salinan status tempat tidur. Hanya tiga nilai yang boleh ditulis modul ini:
        /// <c>Available</c>, <c>Reserved</c>, dan <c>Occupied</c>.
        /// </summary>
        /// <remarks>
        /// Tempat tidur yang sedang <c>Cleaning</c>, <c>Maintenance</c>, <c>Blocked</c>, atau
        /// <c>Inactive</c> tidak pernah ditimpa. Keempatnya tetap wewenang admin master data,
        /// dan menimpanya akan membuat tempat tidur yang sedang diperbaiki kembali muncul
        /// sebagai siap pakai. Batas ini adalah isi <c>INT-INP-03</c>, disetujui pemilik
        /// Master Data lewat <c>RWI-DEC-062</c>.
        /// </remarks>
        private async Task WriteBedStatusCopyAsync(
            Guid bedId,
            BedStatus status,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var bed = await _dbContext.Set<MstBed>()
                .FirstOrDefaultAsync(x => x.Id == bedId, cancellationToken);

            if (bed == null)
            {
                return;
            }

            if (IsBedClosed(bed.BedStatus)) return; bed.BedStatus = status;
            bed.UpdateDateTime = now;
            bed.UpdateBy = actorUserId;
        }

        /// <summary>
        /// Mengembalikan salinan status tempat tidur menjadi <c>Available</c> bila ia sudah
        /// tidak dipegang episode lain.
        /// </summary>
        private async Task ReleaseBedStatusCopyAsync(
            Guid bedId,
            Guid releasingEpisodeId,
            Guid? actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var bed = await _dbContext.Set<MstBed>()
                .FirstOrDefaultAsync(x => x.Id == bedId, cancellationToken);

            if (bed == null)
            {
                return;
            }

            var stillHeld =
                await _dbContext.Set<InpBedPlacement>()
                    .AnyAsync(
                        x =>
                            x.BedId == bedId &&
                            x.EpisodeId != releasingEpisodeId &&
                            x.EndDateTime == null &&
                            !x.IsDelete,
                        cancellationToken)
                || await _dbContext.Set<InpBedReservation>()
                    .AnyAsync(
                        x =>
                            x.BedId == bedId &&
                            x.EpisodeId != releasingEpisodeId &&
                            x.ReservationStatus == InpBedReservationStatus.Active && x.ExpiresAt > DateTime.UtcNow &&
                            !x.IsDelete,
                        cancellationToken);

            if (stillHeld)
            {
                return;
            }

            var released = _dbContext.ChangeTracker.Entries<InpBedPlacement>()
                .Where(x => x.Entity.BedId == bedId && x.Entity.EpisodeId == releasingEpisodeId && x.Entity.EndDateTime != null && x.Entity.SupersededByCorrectionId == null)
                .Select(x => x.Entity).OrderByDescending(x => x.EndDateTime).FirstOrDefault();
            var root = await _bedCoordinator.RootAsync(bedId, actorUserId ?? _bedAccess.ActorId, cancellationToken);
            if (released != null)
            {
                var before = root.State;
                await _bedCoordinator.InvalidateAsync(bedId, actorUserId ?? _bedAccess.ActorId, released.Id, cancellationToken);
                root.Version++;
                _dbContext.Set<InpBedLifecycleEvent>().Add(new() { ReadinessId = root.Id, CycleId = root.CycleId,
                    PlacementId = released.Id, Action = "UsedBedReleased", BeforeState = before, AfterState = root.State,
                    ActorUserId = actorUserId ?? _bedAccess.ActorId, CreateBy = actorUserId ?? _bedAccess.ActorId,
                    OccurredAtUtc = now, RecordedAtUtc = DateTime.UtcNow });
            }
            if (bed.BedStatus is not (BedStatus.Maintenance or BedStatus.Blocked or BedStatus.Inactive or BedStatus.Unknown)) bed.BedStatus = root.State == InpBedReadinessState.Ready ? BedStatus.Available : BedStatus.Cleaning;
            bed.UpdateDateTime = now;
            bed.UpdateBy = actorUserId ?? Guid.Empty;
        }

        private async Task<BedWithRoom?> LoadBedContextAsync(
            Guid bedId,
            CancellationToken cancellationToken)
        {
            var bed = await _dbContext.Set<MstBed>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == bedId && !x.IsDelete, cancellationToken);

            if (bed == null)
            {
                return null;
            }

            var room = await _dbContext.Set<MstRoom>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == bed.RoomId && !x.IsDelete, cancellationToken);

            if (room == null)
            {
                return null;
            }

            return new BedWithRoom { Bed = bed, Room = room };
        }

        /// <summary>
        /// Kelas yang ditagihkan selama satu penempatan: mengikuti kamar yang ditempati bila
        /// kamarnya memiliki kelas. Kamar tanpa kelas ditolak pada pemeriksaan kelayakan.
        /// </summary>
        /// <remarks>
        /// Dasarnya <c>RWI-DEC-013</c>: pasien yang pindah dari kamar kelas 2 ke kamar kelas 1
        /// ditagihkan menurut kamar yang benar-benar ditempatinya, bukan menurut kelas yang
        /// dipilih saat admisi dibuka.
        /// </remarks>
        private static Guid ResolveBilledPatientClassId(MstRoom room, InpEpisode episode)
        {
            return room.PatientClassId.HasValue && room.PatientClassId.Value != Guid.Empty
                ? room.PatientClassId.Value
                : throw new InvalidOperationException("Kamar belum memiliki kelas perawatan. Lengkapi kelas kamar sebelum menempatkan pasien.");
        }

        private static bool IsBedClosed(BedStatus status)
        {
            return status is BedStatus.Cleaning
                or BedStatus.Maintenance
                or BedStatus.Blocked
                or BedStatus.Inactive;
        }

        private static string BedStatusLabel(BedStatus status)
        {
            return status switch
            {
                BedStatus.Available => "Tersedia",
                BedStatus.Occupied => "Terisi",
                BedStatus.Reserved => "Dipesan",
                BedStatus.Cleaning => "Pembersihan",
                BedStatus.Maintenance => "Perbaikan",
                BedStatus.Blocked => "Diblokir",
                BedStatus.Inactive => "Nonaktif",
                _ => "Tidak diketahui"
            };
        }

        private static string BedTakenMessage(string bedCode)
        {
            return $"Tempat tidur {bedCode} sudah ditempati pasien lain. Silakan pilih tempat " +
                   "tidur lain; isian admisi Anda tetap tersimpan.";
        }

        private sealed class BedWithRoom
        {
            public MstBed Bed { get; set; } = null!;

            public MstRoom? Room { get; set; }
        }
    }

    /// <summary>
    /// Jalur yang sedang memanggil Kelayakan Penempatan. Menentukan kalimat penolakan dan
    /// apakah penanda <c>IsReservable</c> ikut diperiksa.
    /// </summary>
    public enum InpPlacementContext
    {
        /// <summary>Penyaringan hasil pencarian tempat tidur.</summary>
        Search = 0,

        /// <summary>Pemesanan tempat tidur.</summary>
        Reservation = 1,

        /// <summary>Penempatan pasien pertama kali.</summary>
        Placement = 2,

        /// <summary>Perpindahan ke tempat tidur lain.</summary>
        Transfer = 3
    }

    /// <summary>
    /// Hasil pemeriksaan Kelayakan Penempatan: daftar aturan yang gagal beserta kode status
    /// yang seharusnya dipakai.
    /// </summary>
    public sealed class InpPlacementEligibilityResult
    {
        public List<PlacementEligibilityFailureResponse> Failures { get; } = new();

        public bool IsEligible => Failures.Count == 0;

        /// <summary>
        /// Benar bila episode ini punya pemesanan yang masih berlaku atas tempat tidur
        /// tersebut. Bukan penolakan — inilah aturan 3, yang memakainya alih-alih menolaknya.
        /// </summary>
        public bool ReusableReservationExists { get; set; }

        /// <summary>
        /// Kode status yang dipakai bila permintaan ditolak. Tabrakan keadaan (409) selalu
        /// mengalahkan penolakan aturan bisnis (422), karena tindakan lanjutan petugas berbeda:
        /// yang pertama berarti pilih tempat tidur lain, yang kedua berarti tempat tidur itu
        /// memang tidak layak bagi pasien ini.
        /// </summary>
        public int StatusCode => Failures.Any(x => x.StatusCode == 409) ? 409 : 422;

        /// <summary>Kalimat utama penolakan, diambil dari aturan yang menentukan kode statusnya.</summary>
        public string PrimaryMessage =>
            Failures.FirstOrDefault(x => x.StatusCode == StatusCode)?.Message
            ?? "Tempat tidur ini tidak dapat dipakai.";

        public void Add(int ruleNumber, string code, string message, int statusCode)
        {
            Failures.Add(new PlacementEligibilityFailureResponse
            {
                RuleNumber = ruleNumber,
                Code = code,
                Message = message,
                StatusCode = statusCode
            });
        }
    }

    /// <summary>Hasil satu tindakan penghunian tempat tidur.</summary>
    public sealed class InpBedOccupancyOperationResult
    {
        private InpBedOccupancyOperationResult(
            InpEpisodeOperationStatus status,
            string message,
            List<PlacementEligibilityFailureResponse>? failures = null,
            Guid? reservationId = null,
            Guid? placementId = null,
            string? code = null)
        {
            Status = status;
            Message = message;
            Failures = failures ?? new List<PlacementEligibilityFailureResponse>();
            ReservationId = reservationId;
            PlacementId = placementId;
            Code = code;
        }

        public InpEpisodeOperationStatus Status { get; }

        public string Message { get; }

        public string? Code { get; }

        /// <summary>
        /// Daftar aturan Kelayakan Penempatan yang gagal. Kosong untuk penolakan yang bukan
        /// berasal dari pemeriksaan kelayakan.
        /// </summary>
        public List<PlacementEligibilityFailureResponse> Failures { get; }

        public Guid? ReservationId { get; }

        public Guid? PlacementId { get; }

        public static InpBedOccupancyOperationResult Success(
            string message,
            Guid? reservationId = null,
            Guid? placementId = null)
            => new(
                InpEpisodeOperationStatus.Success,
                message,
                null,
                reservationId,
                placementId);

        public static InpBedOccupancyOperationResult Invalid(string message, string? code = null)
            => new(InpEpisodeOperationStatus.Invalid, message, code: code);

        public static InpBedOccupancyOperationResult NotFound(string message, string? code = null)
            => new(InpEpisodeOperationStatus.NotFound, message, code: code);

        public static InpBedOccupancyOperationResult Conflict(string message, string? code = null)
            => new(InpEpisodeOperationStatus.Conflict, message, code: code);

        public static InpBedOccupancyOperationResult BusinessRuleRejected(string message, string? code = null)
            => new(InpEpisodeOperationStatus.BusinessRuleRejected, message, code: code);

        public static InpBedOccupancyOperationResult Forbidden(string message, string? code = null)
            => new(InpEpisodeOperationStatus.Forbidden, message, code: code);

        /// <summary>
        /// Menyusun penolakan dari hasil Kelayakan Penempatan, lengkap dengan daftar aturan
        /// yang gagal.
        /// </summary>
        public static InpBedOccupancyOperationResult FromEligibility(
            InpPlacementEligibilityResult evaluation)
        {
            var status = evaluation.StatusCode == 409
                ? InpEpisodeOperationStatus.Conflict
                : InpEpisodeOperationStatus.BusinessRuleRejected;

            return new InpBedOccupancyOperationResult(
                status,
                evaluation.PrimaryMessage,
                evaluation.Failures.ToList(),
                code: evaluation.Failures.FirstOrDefault()?.Code);
        }
    }
}
