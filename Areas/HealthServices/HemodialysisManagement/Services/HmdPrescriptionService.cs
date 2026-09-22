using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Siklus resep HD: draf, aktivasi dengan penggantian terlacak, dan pembatalan
    /// (<c>BE-HMD-09</c>, <c>HMD-DEC-009</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Resep aktif tidak pernah disunting.</b> Penyuntingan resep <c>Active</c> ditolak
    /// <c>423 HMD-VAL-024</c> dengan anjuran membuat resep baru. Aktivasi resep baru
    /// menggantikan resep aktif lama pada <b>satu transaksi</b>: resep lama menjadi
    /// <c>Superseded</c> dan menunjuk resep penggantinya, baru kemudian resep baru menjadi
    /// <c>Active</c>. Bila salah satu langkah gagal, keduanya dibatalkan dan resep lama tetap
    /// berlaku (<c>HMD-VAL-022</c>).
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Dokter Rahmat menaikkan target penarikan cairan dari 2.000 ml menjadi
    /// 2.500 ml. Ia membuat draf baru bertarget 2.500 ml lalu mengaktifkannya. Resep baru
    /// <c>Active</c>, resep lama <c>Superseded</c>, dan keduanya tetap terbaca lengkap dengan
    /// dokter pembuat dan waktu pengaktifan.
    /// </para>
    /// <para>
    /// Dokter pembuat diturunkan dari akun yang sedang masuk, lalu dibawa sebagai
    /// <c>InstructingDoctorId</c> pada tindakan pasien saat sesi dimulai (<c>HMD-DEC-009</c>).
    /// </para>
    /// </remarks>
    public class HmdPrescriptionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _clinicalContextService;

        public HmdPrescriptionService(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService clinicalContextService)
        {
            _dbContext = dbContext;
            _clinicalContextService = clinicalContextService;
        }

        public async Task<PagedResult<HmdPrescriptionResponse>> GetListAsync(HmdPrescriptionPagedQuery query, CancellationToken cancellationToken)
        {
            var (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);

            var rows = _dbContext.HmdPrescriptions.AsNoTracking().Where(x => !x.IsDelete);
            if (query.EpisodeId.HasValue)
                rows = rows.Where(x => x.EpisodeId == query.EpisodeId.Value);
            if (query.PrescriptionStatus.HasValue)
                rows = rows.Where(x => x.PrescriptionStatus == query.PrescriptionStatus.Value);

            rows = rows.OrderByDescending(x => x.CreateDateTime);

            var total = await rows.CountAsync(cancellationToken);
            var items = await Project(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            items.ForEach(Label);

            return new PagedResult<HmdPrescriptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            };
        }

        public async Task<HmdPrescriptionResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            var row = await Project(_dbContext.HmdPrescriptions.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken);
            if (row != null)
                Label(row);
            return row;
        }

        public async Task<HmdResult<HmdPrescriptionResponse>> CreateAsync(
            CreateHmdPrescriptionRequest request,
            HmdActor actor,
            CancellationToken cancellationToken)
        {
            var episodeStatus = await _dbContext.HmdEpisodes.AsNoTracking()
                .Where(x => x.Id == request.EpisodeId && !x.IsDelete)
                .Select(x => (HmdEpisodeStatus?)x.EpisodeStatus)
                .FirstOrDefaultAsync(cancellationToken);

            if (!episodeStatus.HasValue)
                return HmdResult<HmdPrescriptionResponse>.NotFound("Episode hemodialisa tidak ditemukan atau sudah dihapus.");

            if (episodeStatus.Value != HmdEpisodeStatus.Active)
                return HmdResult<HmdPrescriptionResponse>.Rule(HmdErrorCodes.Val020, HmdMessages.Val020);

            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(actor.Principal, actor.UserId, cancellationToken);
            if (!doctorId.HasValue)
                return HmdResult<HmdPrescriptionResponse>.Forbidden(HmdErrorCodes.ActorNotDoctor, HmdMessages.ActorNotDoctor);

            var accessGuard = await GuardAccessAsync(request.EpisodeId, request.VascularAccessId, cancellationToken);
            if (accessGuard != null)
                return accessGuard;

            var now = DateTime.UtcNow;
            var prescription = new HmdPrescription
            {
                EpisodeId = request.EpisodeId,
                PrescribingDoctorId = doctorId.Value,
                PrescriptionStatus = HmdPrescriptionStatus.Draft,
                CreateDateTime = now,
                CreateBy = actor.UserId
            };
            Apply(prescription, request);

            _dbContext.HmdPrescriptions.Add(prescription);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return HmdResult<HmdPrescriptionResponse>.Created((await GetAsync(prescription.Id, cancellationToken))!);
        }

        /// <summary>Menyunting resep yang masih draf. Resep aktif ditolak <c>423</c>.</summary>
        public async Task<HmdResult<HmdPrescriptionResponse>> UpdateAsync(
            Guid id,
            UpdateHmdPrescriptionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var prescription = await _dbContext.HmdPrescriptions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (prescription == null)
                return NotFound();

            if (prescription.PrescriptionStatus == HmdPrescriptionStatus.Active)
                return HmdResult<HmdPrescriptionResponse>.Locked(HmdErrorCodes.Val024, HmdMessages.Val024);

            if (prescription.PrescriptionStatus != HmdPrescriptionStatus.Draft)
            {
                return HmdResult<HmdPrescriptionResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    $"Resep berstatus {HmdLabels.PrescriptionStatus(prescription.PrescriptionStatus).ToLowerInvariant()} tidak dapat diubah.");
            }

            var accessGuard = await GuardAccessAsync(prescription.EpisodeId, request.VascularAccessId, cancellationToken);
            if (accessGuard != null)
                return accessGuard;

            Apply(prescription, request);
            prescription.UpdateDateTime = DateTime.UtcNow;
            prescription.UpdateBy = actorUserId;
            prescription.Version++;

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdPrescriptionResponse>.From(failure);

            return HmdResult<HmdPrescriptionResponse>.Ok((await GetAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Mengaktifkan resep draf. Resep aktif sebelumnya pada episode yang sama otomatis
        /// digantikan pada transaksi yang sama.
        /// </summary>
        public async Task<HmdResult<HmdPrescriptionResponse>> ActivateAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var episodeId = await _dbContext.HmdPrescriptions.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => (Guid?)x.EpisodeId)
                .FirstOrDefaultAsync(cancellationToken);
            if (!episodeId.HasValue)
                return NotFound();

            // Dua aktivasi pada episode yang sama diantrekan, supaya yang kedua melihat hasil
            // penggantian yang pertama dan tidak pernah ada dua resep aktif.
            await HmdServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, $"HMD_PRESCRIPTION_{episodeId.Value:N}");

            var prescription = await _dbContext.HmdPrescriptions.FirstAsync(x => x.Id == id, cancellationToken);

            if (prescription.PrescriptionStatus != HmdPrescriptionStatus.Draft)
            {
                return HmdResult<HmdPrescriptionResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    $"Resep berstatus {HmdLabels.PrescriptionStatus(prescription.PrescriptionStatus).ToLowerInvariant()} tidak dapat diaktifkan.");
            }

            var episodeActive = await _dbContext.HmdEpisodes.AsNoTracking()
                .AnyAsync(x => x.Id == prescription.EpisodeId && !x.IsDelete && x.EpisodeStatus == HmdEpisodeStatus.Active, cancellationToken);
            if (!episodeActive)
                return HmdResult<HmdPrescriptionResponse>.Rule(HmdErrorCodes.Val020, HmdMessages.Val020);

            // HMD-VAL-021: parameter wajib dan akses vaskular yang masih layak.
            var accessUsable = prescription.VascularAccessId.HasValue &&
                await _dbContext.HmdVascularAccesses.AsNoTracking().AnyAsync(x =>
                    x.Id == prescription.VascularAccessId.Value &&
                    x.EpisodeId == prescription.EpisodeId &&
                    !x.IsDelete &&
                    x.AccessStatus != HmdVascularAccessStatus.NotUsable,
                    cancellationToken);

            if (prescription.FrequencyPerWeek <= 0 ||
                prescription.TargetDurationMinutes <= 0 ||
                !prescription.TargetUltrafiltrationMl.HasValue ||
                !accessUsable)
            {
                return HmdResult<HmdPrescriptionResponse>.Rule(HmdErrorCodes.Val021, HmdMessages.Val021);
            }

            var now = DateTime.UtcNow;

            try
            {
                var previous = await _dbContext.HmdPrescriptions
                    .Where(x => x.EpisodeId == prescription.EpisodeId && x.Id != id && !x.IsDelete &&
                                x.PrescriptionStatus == HmdPrescriptionStatus.Active)
                    .ToListAsync(cancellationToken);

                foreach (var old in previous)
                {
                    old.PrescriptionStatus = HmdPrescriptionStatus.Superseded;
                    old.SupersededByPrescriptionId = prescription.Id;
                    old.UpdateDateTime = now;
                    old.UpdateBy = actorUserId;
                    old.Version++;
                }

                // Langkah pertama disimpan terpisah: PostgreSQL memeriksa unique index bersyarat
                // per pernyataan, sehingga resep lama wajib sudah Superseded sebelum resep baru
                // menjadi Active.
                await _dbContext.SaveChangesAsync(cancellationToken);

                prescription.PrescriptionStatus = HmdPrescriptionStatus.Active;
                prescription.ActivatedAt = now;
                prescription.ActivatedByUserId = actorUserId;
                prescription.UpdateDateTime = now;
                prescription.UpdateBy = actorUserId;
                prescription.Version++;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<HmdPrescriptionResponse>.Conflict(HmdErrorCodes.Val022, HmdMessages.Val022);
            }

            return HmdResult<HmdPrescriptionResponse>.Ok((await GetAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Membatalkan resep draf atau aktif. Resep aktif yang sedang dipakai sesi berjalan ditolak
        /// <c>409 HMD-VAL-023</c>.
        /// </summary>
        public async Task<HmdResult<HmdPrescriptionResponse>> CancelAsync(
            Guid id,
            CancelHmdPrescriptionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdPrescriptionResponse>.Invalid(HmdErrorCodes.Val080, HmdMessages.Val080);

            var prescription = await _dbContext.HmdPrescriptions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (prescription == null)
                return NotFound();

            if (prescription.PrescriptionStatus is not (HmdPrescriptionStatus.Draft or HmdPrescriptionStatus.Active))
            {
                return HmdResult<HmdPrescriptionResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    $"Resep berstatus {HmdLabels.PrescriptionStatus(prescription.PrescriptionStatus).ToLowerInvariant()} tidak dapat dibatalkan.");
            }

            if (prescription.PrescriptionStatus == HmdPrescriptionStatus.Active &&
                await _dbContext.HmdSessions.AsNoTracking().AnyAsync(x =>
                    x.PrescriptionId == id && !x.IsDelete && x.SessionStatus == HmdSessionStatus.InProgress, cancellationToken))
            {
                return HmdResult<HmdPrescriptionResponse>.Conflict(HmdErrorCodes.Val023, HmdMessages.Val023);
            }

            var now = DateTime.UtcNow;
            prescription.PrescriptionStatus = HmdPrescriptionStatus.Cancelled;
            prescription.CancelReason = reason;
            prescription.CancelDateTime = now;
            prescription.CancelBy = actorUserId;
            prescription.UpdateDateTime = now;
            prescription.UpdateBy = actorUserId;
            prescription.Version++;

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdPrescriptionResponse>.From(failure);

            return HmdResult<HmdPrescriptionResponse>.Ok((await GetAsync(id, cancellationToken))!);
        }

        // =================================================================
        // Penolong
        // =================================================================

        private static HmdResult<HmdPrescriptionResponse> NotFound() =>
            HmdResult<HmdPrescriptionResponse>.NotFound("Resep hemodialisa tidak ditemukan atau sudah dihapus.");

        private async Task<HmdResult<HmdPrescriptionResponse>?> GuardAccessAsync(
            Guid episodeId, Guid? vascularAccessId, CancellationToken cancellationToken)
        {
            if (!vascularAccessId.HasValue)
                return null;

            var ok = await _dbContext.HmdVascularAccesses.AsNoTracking()
                .AnyAsync(x => x.Id == vascularAccessId.Value && x.EpisodeId == episodeId && !x.IsDelete, cancellationToken);

            return ok
                ? null
                : HmdResult<HmdPrescriptionResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Akses vaskular yang dipilih bukan milik episode ini.");
        }

        private static void Apply(HmdPrescription target, HmdPrescriptionParameters source)
        {
            target.EffectiveDate = source.EffectiveDate;
            target.FrequencyPerWeek = source.FrequencyPerWeek;
            target.TargetDurationMinutes = source.TargetDurationMinutes;
            target.TargetUltrafiltrationMl = source.TargetUltrafiltrationMl;
            target.BloodFlowRate = source.BloodFlowRate;
            target.DialysateFlowRate = source.DialysateFlowRate;
            target.DialyzerType = HmdServiceSupport.Normalize(source.DialyzerType);
            target.DialysateComposition = HmdServiceSupport.Normalize(source.DialysateComposition);
            target.SodiumBicarbonateProfile = HmdServiceSupport.Normalize(source.SodiumBicarbonateProfile);
            target.DialysateTemperatureC = source.DialysateTemperatureC;
            target.AnticoagulantPlan = HmdServiceSupport.Normalize(source.AnticoagulantPlan);
            target.VascularAccessId = source.VascularAccessId;
            target.ClinicalNote = HmdServiceSupport.Normalize(source.ClinicalNote);
        }

        private static IQueryable<HmdPrescriptionResponse> Project(IQueryable<HmdPrescription> rows) =>
            rows.Select(x => new HmdPrescriptionResponse
            {
                Id = x.Id,
                EpisodeId = x.EpisodeId,
                EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                PrescribingDoctorId = x.PrescribingDoctorId,
                PrescribingDoctorName = x.PrescribingDoctor != null ? x.PrescribingDoctor.FullName : null,
                EffectiveDate = x.EffectiveDate,
                FrequencyPerWeek = x.FrequencyPerWeek,
                TargetDurationMinutes = x.TargetDurationMinutes,
                TargetUltrafiltrationMl = x.TargetUltrafiltrationMl,
                BloodFlowRate = x.BloodFlowRate,
                DialysateFlowRate = x.DialysateFlowRate,
                DialyzerType = x.DialyzerType,
                DialysateComposition = x.DialysateComposition,
                SodiumBicarbonateProfile = x.SodiumBicarbonateProfile,
                DialysateTemperatureC = x.DialysateTemperatureC,
                AnticoagulantPlan = x.AnticoagulantPlan,
                VascularAccessId = x.VascularAccessId,
                VascularAccessSite = x.VascularAccess != null ? x.VascularAccess.AccessSite : null,
                ClinicalNote = x.ClinicalNote,
                PrescriptionStatus = x.PrescriptionStatus,
                SupersededByPrescriptionId = x.SupersededByPrescriptionId,
                ActivatedAt = x.ActivatedAt,
                ActivatedByUserId = x.ActivatedByUserId,
                CancelReason = x.CancelReason,
                CreateDateTime = x.CreateDateTime
            });

        private static void Label(HmdPrescriptionResponse row)
        {
            row.PrescriptionStatusName = HmdLabels.PrescriptionStatus(row.PrescriptionStatus);
            row.AvailableActions = row.PrescriptionStatus switch
            {
                HmdPrescriptionStatus.Draft => ["Update", "Activate", "Cancel"],
                HmdPrescriptionStatus.Active => ["Cancel"],
                _ => []
            };
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
                return HmdResult<bool>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }
        }
    }
}
