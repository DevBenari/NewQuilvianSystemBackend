using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
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
    public class InpBedOccupancyService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpSettingService _settingService;
        private readonly InpEpisodeService _episodeService;

        /// <remarks>
        /// Arah dependency ke <see cref="InpEpisodeService"/> ditetapkan `BE-RWI-011`:
        /// penempatan pasien wajib memindahkan status episode lewat satu-satunya pintu, yaitu
        /// <c>InpEpisodeService.ApplyStatusChangeAsync</c>. Lihat catatan pada konstruktor
        /// <see cref="InpEpisodeService"/> untuk alasan lengkapnya.
        /// </remarks>
        public InpBedOccupancyService(
            ApplicationDbContext dbContext,
            InpSettingService settingService,
            InpEpisodeService episodeService)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _episodeService = episodeService;
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

            IQueryable<MstBed> bedQuery = _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

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

            foreach (var candidate in candidates)
            {
                if (candidate.Room == null || candidate.Room.IsDelete || !candidate.Room.IsActive)
                {
                    continue;
                }

                var evaluation = await EvaluatePlacementEligibilityAsync(
                    episode,
                    patient,
                    candidate,
                    candidate.Room,
                    InpPlacementContext.Search,
                    cancellationToken);

                if (evaluation.IsEligible)
                {
                    eligible.Add(candidate);
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
                    IsForMale = x.IsForMale,
                    IsForFemale = x.IsForFemale,
                    IsForNewborn = x.IsForNewborn,
                    IsIsolationBed = x.IsIsolationBed,
                    IsReservable = x.IsReservable
                })
                .ToList();

            await FillServiceUnitAndClassNamesAsync(items, cancellationToken);

            return new AvailableBedPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
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
        public async Task<BedBoardResponse> GetBedBoardAsync(
            Guid? serviceUnitId,
            CancellationToken cancellationToken = default)
        {
            await ExpireDueReservationsAsync(cancellationToken);
            var activeReservationCutoff = DateTime.UtcNow;

            IQueryable<MstRoom> roomQuery = _dbContext.Set<MstRoom>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
            {
                roomQuery = roomQuery.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            }
            else
            {
                roomQuery = roomQuery.Where(x =>
                    x.ServiceUnit != null &&
                    x.ServiceUnit.ServiceUnitType == ServiceUnitType.Inpatient);
            }

            var rooms = await roomQuery
                .Select(x => new
                {
                    x.Id,
                    x.RoomCode,
                    x.RoomName,
                    x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    x.Capacity
                })
                .ToListAsync(cancellationToken);

            var roomIds = rooms.Select(x => x.Id).ToList();

            var beds = await _dbContext.Set<MstBed>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && roomIds.Contains(x.RoomId))
                .OrderBy(x => x.BedName)
                .Select(x => new
                {
                    x.Id,
                    x.RoomId,
                    x.BedCode,
                    x.BedName,
                    x.BedStatus,
                    x.IsIsolationBed,
                    x.IsForNewborn
                })
                .ToListAsync(cancellationToken);

            var bedIds = beds.Select(x => x.Id).ToList();

            var placements = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x => x.EndDateTime == null && !x.IsDelete && bedIds.Contains(x.BedId))
                .Select(x => new
                {
                    x.BedId,
                    x.EpisodeId,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                    PatientName = x.Episode != null && x.Episode.Patient != null
                        ? x.Episode.Patient.FullName
                        : null
                })
                .ToListAsync(cancellationToken);

            var reservations = await _dbContext.Set<InpBedReservation>()
                .AsNoTracking()
                .Where(x =>
                    x.ReservationStatus == InpBedReservationStatus.Active &&
                    x.ExpiresAt > activeReservationCutoff &&
                    !x.IsDelete &&
                    bedIds.Contains(x.BedId))
                .Select(x => new
                {
                    x.Id,
                    x.BedId,
                    x.EpisodeId,
                    x.ExpiresAt,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                    PatientName = x.Episode != null && x.Episode.Patient != null
                        ? x.Episode.Patient.FullName
                        : null
                })
                .ToListAsync(cancellationToken);

            var board = new BedBoardResponse();

            foreach (var unitGroup in rooms
                .GroupBy(x => new { x.ServiceUnitId, x.ServiceUnitName })
                .OrderBy(x => x.Key.ServiceUnitName))
            {
                var unit = new BedBoardServiceUnitResponse
                {
                    ServiceUnitId = unitGroup.Key.ServiceUnitId,
                    ServiceUnitName = unitGroup.Key.ServiceUnitName
                };

                foreach (var room in unitGroup.OrderBy(x => x.RoomName))
                {
                    var roomResponse = new BedBoardRoomResponse
                    {
                        RoomId = room.Id,
                        RoomCode = room.RoomCode,
                        RoomName = room.RoomName,
                        PatientClassId = room.PatientClassId,
                        PatientClassName = room.PatientClassName,
                        Capacity = room.Capacity
                    };

                    foreach (var bed in beds.Where(x => x.RoomId == room.Id))
                    {
                        var placement = placements.FirstOrDefault(x => x.BedId == bed.Id);
                        var reservation = reservations.FirstOrDefault(x => x.BedId == bed.Id);

                        var bedResponse = new BedBoardBedResponse
                        {
                            BedId = bed.Id,
                            BedCode = bed.BedCode,
                            BedName = bed.BedName,
                            BedStatus = (int)bed.BedStatus,
                            BedStatusName = bed.BedStatus.ToString(),
                            IsOccupied = placement != null,
                            IsReserved = placement == null && reservation != null,
                            IsIsolationBed = bed.IsIsolationBed,
                            IsForNewborn = bed.IsForNewborn,
                            HoldingEpisodeId = placement?.EpisodeId ?? reservation?.EpisodeId,
                            HoldingEpisodeNumber = placement?.EpisodeNumber ?? reservation?.EpisodeNumber,
                            PatientName = placement?.PatientName ?? reservation?.PatientName,
                            ReservationId = placement == null ? reservation?.Id : null,
                            ReservationExpiresAt = placement == null ? reservation?.ExpiresAt : null
                        };

                        roomResponse.Beds.Add(bedResponse);

                        unit.TotalBed++;

                        if (bedResponse.IsOccupied)
                        {
                            unit.TotalOccupied++;
                        }
                        else if (bedResponse.IsReserved)
                        {
                            unit.TotalReserved++;
                        }
                        else if (IsBedClosed(bed.BedStatus))
                        {
                            unit.TotalUnavailable++;
                        }
                        else
                        {
                            unit.TotalAvailable++;
                        }
                    }

                    unit.Rooms.Add(roomResponse);
                }

                board.ServiceUnits.Add(unit);

                board.TotalBed += unit.TotalBed;
                board.TotalAvailable += unit.TotalAvailable;
                board.TotalOccupied += unit.TotalOccupied;
                board.TotalReserved += unit.TotalReserved;
                board.TotalUnavailable += unit.TotalUnavailable;
            }

            return board;
        }

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
        public async Task<InpBedOccupancyOperationResult> ReserveBedAsync(
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
                        x.ReservationStatus == InpBedReservationStatus.Active &&
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

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

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
        public async Task<InpBedOccupancyOperationResult> CancelReservationAsync(
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

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                reservation.ReservationStatus = InpBedReservationStatus.Cancelled;
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
        public async Task<int> ExpireDueReservationsAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var due = await _dbContext.Set<InpBedReservation>()
                .Where(x =>
                    x.ReservationStatus == InpBedReservationStatus.Active &&
                    !x.IsDelete &&
                    x.ExpiresAt <= now)
                .ToListAsync(cancellationToken);

            if (due.Count == 0)
            {
                return 0;
            }

            foreach (var reservation in due)
            {
                reservation.ReservationStatus = InpBedReservationStatus.Expired;
                reservation.ReleasedAt = now;
                reservation.IsActive = false;
                reservation.UpdateDateTime = now;
                reservation.UpdateBy = Guid.Empty;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var reservation in due)
            {
                await ReleaseBedStatusCopyAsync(
                    reservation.BedId,
                    Guid.Empty,
                    null,
                    now,
                    cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return due.Count;
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
        /// revisi ini, dan kolom <c>TrxPatientEncounter.OriginEncounterId</c> yang menjadi
        /// syaratnya belum ada pada source hari ini.
        /// </para>
        /// </remarks>
        public async Task<InpBedOccupancyOperationResult> PlacePatientAsync(
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
                return InpBedOccupancyOperationResult.Conflict(present.RejectionMessage);
            }

            var context = await LoadBedContextAsync(request.BedId, cancellationToken);

            if (context == null)
            {
                return InpBedOccupancyOperationResult.NotFound("Tempat tidur tidak ditemukan.");
            }

            var patient = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == episode.PatientId, cancellationToken);

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

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

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
                            x.ReservationStatus == InpBedReservationStatus.Active &&
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
        public async Task<InpBedOccupancyOperationResult> TransferAsync(
            TransferPatientRequest request,
            Guid actorUserId,
            Guid? actorDoctorId,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.EpisodeId == Guid.Empty || request.TargetBedId == Guid.Empty)
            {
                return InpBedOccupancyOperationResult.Invalid(
                    "Episode dan tempat tidur tujuan wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(request.TransferReason) ||
                !request.TransferReason.Any(char.IsLetterOrDigit))
            {
                return InpBedOccupancyOperationResult.Invalid("Alasan perpindahan wajib diisi.");
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

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

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
                    StartDateTime = now,
                    EndDateTime = null,
                    TransferReason = reason,
                    PlacedByUserId = actorUserId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

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

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

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

        /// <summary>Membaca riwayat penempatan satu episode, dari tempat tidur pertama sampai terakhir.</summary>
        public async Task<List<BedPlacementResponse>> GetPlacementsByEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var rows = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
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
                    IsCurrent = x.EndDateTime == null
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
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
            var rows = await _dbContext.Set<InpBedPlacement>()
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
                    ReleasedAt = x.ReleasedAt
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
        /// <b>Dua pengecualian boks bayi, keduanya berlaku dua arah.</b> Menempatkan <b>ke</b>
        /// boks bayi melewati aturan 4, 5, dan 6 — bayi laki-laki boleh menempati boks di
        /// kamar ibunya. Penghuni yang <b>berada di</b> boks bayi tidak dihitung saat aturan 5
        /// dan 6 memeriksa penghuni kamar — bayi tidak menutup kamar bagi pasien lain.
        /// </para>
        ///
        /// <para>
        /// <b>Aturan 6 diperiksa dari penghuni yang sedang ada</b>, bukan dari penanda pada
        /// <c>MstRoom</c>. Penanda <c>IsForMale</c> dan <c>IsForFemale</c> bernilai benar
        /// secara bawaan untuk setiap kamar, sehingga tidak dapat membedakan kamar yang boleh
        /// campur. Kolom "boleh campur" ditolak tegas oleh <c>RWI-DEC-066</c> dan dikunci
        /// `blueprint-manifest.md` bagian 8 butir 7; menambahkannya bukan keputusan pelaksana.
        /// </para>
        ///
        /// <para>
        /// <b>Aturan 9 tidak pernah menyala pada revisi ini.</b> Ia hanya berlaku bila episode
        /// lahir dari serah terima IGD, yang dikenali dari
        /// <c>TrxPatientEncounter.OriginEncounterId</c>. Kolom itu <b>belum ada</b> pada source
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
            var result = new InpPlacementEligibilityResult();
            var episodeId = episode?.Id ?? Guid.Empty;

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
            var otherReservation = await _dbContext.Set<InpBedReservation>()
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.BedId == bed.Id &&
                        x.EpisodeId != episodeId &&
                        x.ReservationStatus == InpBedReservationStatus.Active &&
                        !x.IsDelete,
                    cancellationToken);

            if (otherReservation)
            {
                result.Add(
                    2,
                    "BED_RESERVED_BY_OTHER",
                    "Tempat tidur ini sudah dipesan untuk pasien lain.",
                    409);
            }

            var otherPlacement = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.BedId == bed.Id &&
                        x.EpisodeId != episodeId &&
                        x.EndDateTime == null &&
                        !x.IsDelete,
                    cancellationToken);

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
                result.ReusableReservationExists = await _dbContext.Set<InpBedReservation>()
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.BedId == bed.Id &&
                            x.EpisodeId == episodeId &&
                            x.ReservationStatus == InpBedReservationStatus.Active &&
                            !x.IsDelete,
                        cancellationToken);
            }

            // Tanpa episode, pemeriksaan berhenti di sini. Empat aturan berikutnya menyangkut
            // pasien dan kebutuhan isolasinya, dan keduanya milik episode.
            if (episode == null)
            {
                return result;
            }

            // --- Aturan 4, 5, dan 6: privasi jenis kelamin --------------------------------
            // Pengecualian pertama: menempatkan KE boks bayi melewati ketiganya.
            if (!bed.IsForNewborn)
            {
                var patientGender = NormalizeGender(patient?.Gender);

                var occupants = await LoadRoomOccupantsAsync(room.Id, episodeId, cancellationToken);

                // Pengecualian kedua: penghuni yang BERADA DI boks bayi tidak dihitung.
                var countedOccupants = occupants.Where(x => !x.BedIsForNewborn).ToList();

                if (patientGender.HasValue)
                {
                    // Aturan 4.
                    var accepted =
                        (patientGender.Value == Gender.Male && bed.IsForMale) ||
                        (patientGender.Value == Gender.Female && bed.IsForFemale);

                    if (!accepted)
                    {
                        result.Add(4, "BED_GENDER_MISMATCH", BedGenderMessage(bed), 422);
                    }

                    // Aturan 6.
                    var conflicting = countedOccupants
                        .FirstOrDefault(x => NormalizeGender(x.Gender) != patientGender.Value);

                    if (conflicting != null)
                    {
                        result.Add(
                            6,
                            "ROOM_GENDER_MIXED",
                            $"Kamar {room.RoomName} sedang dihuni pasien " +
                            $"{GenderLabel(NormalizeGender(conflicting.Gender))}, sehingga tidak " +
                            $"dapat menerima pasien {GenderLabel(patientGender.Value)}.",
                            422);
                    }
                }
                else
                {
                    // Aturan 5. Gagal salah satu saja sudah menolak.
                    if (!bed.IsForMale || !bed.IsForFemale || countedOccupants.Count > 0)
                    {
                        result.Add(
                            5,
                            "PATIENT_GENDER_UNKNOWN",
                            "Jenis kelamin pasien belum tercatat. Pilih tempat tidur yang " +
                            "menerima laki-laki dan perempuan, di kamar yang belum ada " +
                            "penghuninya.",
                            422);
                    }
                }
            }

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

            if (bed == null || IsBedClosed(bed.BedStatus))
            {
                return;
            }

            bed.BedStatus = status;
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

            if (bed == null || IsBedClosed(bed.BedStatus))
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
                            x.ReservationStatus == InpBedReservationStatus.Active &&
                            !x.IsDelete,
                        cancellationToken);

            if (stillHeld)
            {
                return;
            }

            bed.BedStatus = BedStatus.Available;
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

        private async Task<List<RoomOccupant>> LoadRoomOccupantsAsync(
            Guid roomId,
            Guid excludeEpisodeId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x =>
                    x.RoomId == roomId &&
                    x.EpisodeId != excludeEpisodeId &&
                    x.EndDateTime == null &&
                    !x.IsDelete)
                .Select(x => new RoomOccupant
                {
                    BedIsForNewborn = x.Bed != null && x.Bed.IsForNewborn,
                    Gender = x.Episode != null && x.Episode.Patient != null
                        ? x.Episode.Patient.Gender
                        : null
                })
                .ToListAsync(cancellationToken);
        }

        private async Task FillServiceUnitAndClassNamesAsync(
            List<AvailableBedResponse> items,
            CancellationToken cancellationToken)
        {
            if (items.Count == 0)
            {
                return;
            }

            var serviceUnitIds = items
                .Select(x => x.ServiceUnitId)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var patientClassIds = items
                .Where(x => x.PatientClassId.HasValue && x.PatientClassId.Value != Guid.Empty)
                .Select(x => x.PatientClassId!.Value)
                .Distinct()
                .ToList();

            var serviceUnits = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => serviceUnitIds.Contains(x.Id))
                .Select(x => new { x.Id, x.ServiceUnitName })
                .ToListAsync(cancellationToken);

            var patientClasses = await _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => patientClassIds.Contains(x.Id))
                .Select(x => new { x.Id, x.PatientClassName })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.ServiceUnitName = serviceUnits
                    .FirstOrDefault(x => x.Id == item.ServiceUnitId)?.ServiceUnitName;

                if (item.PatientClassId.HasValue)
                {
                    item.PatientClassName = patientClasses
                        .FirstOrDefault(x => x.Id == item.PatientClassId.Value)?.PatientClassName;
                }
            }
        }

        /// <summary>
        /// Kelas yang ditagihkan selama satu penempatan: mengikuti kamar yang ditempati bila
        /// kamarnya memang punya kelas, dan jatuh kembali ke kelas pada episode bila tidak.
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
                : episode.PatientClassId;
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

        private static string BedGenderMessage(MstBed bed)
        {
            if (bed.IsForMale && !bed.IsForFemale)
            {
                return "Tempat tidur ini hanya untuk pasien laki-laki.";
            }

            if (bed.IsForFemale && !bed.IsForMale)
            {
                return "Tempat tidur ini hanya untuk pasien perempuan.";
            }

            return "Tempat tidur ini tidak menerima pasien laki-laki maupun perempuan.";
        }

        /// <summary>
        /// Menormalkan jenis kelamin menjadi dua nilai yang punya arti bagi aturan privasi,
        /// atau <c>null</c> bila memang belum tercatat.
        /// </summary>
        /// <remarks>
        /// <c>Unknown</c> dan <c>NotDisclosed</c> keduanya diperlakukan sebagai belum tercatat.
        /// Yang kedua sering dikira "sudah diisi" karena pasien memang menolak menyebutkan;
        /// untuk aturan privasi kamar keduanya sama saja, karena sistem tetap tidak dapat
        /// membuktikan kamarnya tidak menjadi campur.
        /// </remarks>
        private static Gender? NormalizeGender(Gender? gender)
        {
            return gender is Gender.Male or Gender.Female ? gender : null;
        }

        private static string GenderLabel(Gender? gender)
        {
            return gender switch
            {
                Gender.Male => "laki-laki",
                Gender.Female => "perempuan",
                _ => "yang jenis kelaminnya belum tercatat"
            };
        }

        private sealed class BedWithRoom
        {
            public MstBed Bed { get; set; } = null!;

            public MstRoom? Room { get; set; }
        }

        private sealed class RoomOccupant
        {
            public bool BedIsForNewborn { get; set; }

            public Gender? Gender { get; set; }
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
            Guid? placementId = null)
        {
            Status = status;
            Message = message;
            Failures = failures ?? new List<PlacementEligibilityFailureResponse>();
            ReservationId = reservationId;
            PlacementId = placementId;
        }

        public InpEpisodeOperationStatus Status { get; }

        public string Message { get; }

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

        public static InpBedOccupancyOperationResult Invalid(string message)
            => new(InpEpisodeOperationStatus.Invalid, message);

        public static InpBedOccupancyOperationResult NotFound(string message)
            => new(InpEpisodeOperationStatus.NotFound, message);

        public static InpBedOccupancyOperationResult Conflict(string message)
            => new(InpEpisodeOperationStatus.Conflict, message);

        public static InpBedOccupancyOperationResult BusinessRuleRejected(string message)
            => new(InpEpisodeOperationStatus.BusinessRuleRejected, message);

        public static InpBedOccupancyOperationResult Forbidden(string message)
            => new(InpEpisodeOperationStatus.Forbidden, message);

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
                evaluation.Failures.ToList());
        }
    }
}
