using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Satu-satunya pintu perubahan status episode, penugasan DPJP, dan penugasan perawat.
    /// </summary>
    /// <remarks>
    /// <b>Yang sudah terisi.</b> Task <c>BE-RWI-007</c> mengisi pembukaan admisi beserta
    /// <c>ApplyStatusChangeAsync</c>, dan <c>BE-RWI-008</c> mengisi perbaikan isian,
    /// pembatalan, serta kedaluwarsa episode <c>Draft</c>. Perilaku berikutnya diisi task
    /// selanjutnya, satu per satu:
    ///
    /// <list type="bullet">
    /// <item><description><c>BE-RWI-012</c> — penjagaan INV-INP-10, satu pasien satu episode yang hadir</description></item>
    /// <item><description><c>BE-RWI-014</c> — penetapan kebutuhan isolasi beserta penjaga kewenangannya</description></item>
    /// <item><description><c>BE-RWI-017</c> dan <c>BE-RWI-018</c> — penugasan DPJP dan perawat</description></item>
    /// <item><description><c>BE-RWI-025</c> s.d. <c>BE-RWI-026</c> — penutupan episode dan jalan keluar supervisor</description></item>
    /// </list>
    ///
    /// Tiga batas desain yang terkunci dan tidak boleh dilanggar saat method berikutnya
    /// diisi: status episode hanya boleh berubah lewat <see cref="ApplyStatusChangeAsync"/>,
    /// method itu selalu menulis <c>InpStatusHistory</c> di dalam transaksi yang sama, dan
    /// penjaga kewenangan DPJP berada di service ini — bukan di mesin hak akses.
    ///
    /// <para>
    /// <b>Kedaluwarsa dihitung saat dibaca.</b> Episode <c>Draft</c> yang ditinggalkan tidak
    /// dibatalkan program penjadwal. Ia dibatalkan pada saat seseorang membacanya lewat
    /// service ini. Akibatnya: laporan yang menghitung baris langsung dari tabel tanpa
    /// melewati service akan salah hitung, karena baris <c>Draft</c> basi masih ada di tabel
    /// sampai ada yang membacanya. Ini konsekuensi yang disengaja dari <c>RWI-RULE-022</c>.
    /// </para>
    /// </remarks>
    public partial class InpEpisodeService
    {
        /// <summary>
        /// Nilai <c>InpStatusHistory.ActionType</c> untuk admisi yang memakai kunjungan yang
        /// sudah ada sebagai jangkar.
        /// </summary>
        public const string ActionOpenAdmission = "OpenAdmission";

        /// <summary>
        /// Nilai <c>InpStatusHistory.ActionType</c> untuk admisi jalur pasien datang langsung,
        /// yaitu admisi yang membuat sendiri kunjungan jangkarnya.
        /// </summary>
        /// <remarks>
        /// Penanda ini bukan hiasan. Ia adalah satu-satunya bukti tahan lama bahwa kunjungan
        /// jangkar lahir bersama episodenya, dan bukti itulah yang menentukan apakah
        /// kunjungan tersebut boleh ikut dibatalkan saat admisinya batal. Kunjungan yang
        /// ditunjuk petugas — milik alur pendaftaran — tidak pernah ikut dibatalkan modul
        /// ini. Baris riwayat tidak dapat diubah maupun dihapus, sehingga penanda ini tetap
        /// benar sepanjang umur episode.
        /// </remarks>
        public const string ActionOpenAdmissionWithEncounter = "OpenAdmissionWithEncounter";

        /// <summary>Nilai <c>ActionType</c> untuk pembatalan yang dilakukan orang.</summary>
        public const string ActionCancelAdmission = "CancelAdmission";

        /// <summary>Nilai <c>ActionType</c> untuk pembatalan yang dihitung sistem.</summary>
        public const string ActionExpireDraft = "ExpireDraft";

        /// <summary>
        /// Alasan yang dipakai sistem saat membatalkan episode <c>Draft</c> yang telantar.
        /// Ditulis sistem, bukan diisi orang, sesuai <c>RWI-RULE-022</c>.
        /// </summary>
        public const string SystemExpiryReason =
            "Kedaluwarsa, tidak pernah diaktifkan. Dibatalkan sistem.";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpSettingService _settingService;
        private readonly InpEpisodeNumberService _episodeNumberService;

        /// <remarks>
        /// <b>Arah dependency dibalik pada `BE-RWI-011`.</b> Sampai `BE-RWI-008`, service ini
        /// menerima <c>InpBedOccupancyService</c> tanpa pernah memakainya. Sejak penempatan
        /// pasien dibuka, <c>InpBedOccupancyService.PlacePatientAsync</c> wajib memanggil
        /// <see cref="ApplyStatusChangeAsync"/> — satu-satunya pintu perubahan status —
        /// sehingga arah pemakaiannya menjadi <c>InpBedOccupancyService</c> ke service ini.
        /// Mempertahankan kedua arah sekaligus menghasilkan dependency melingkar yang
        /// ditolak container saat aplikasi dinyalakan. Delta terhadap class diagram
        /// `02-backend-architecture.md` bagian 3.4 dicatat pada laporan `BE-RWI-011`.
        /// </remarks>
        public InpEpisodeService(
            ApplicationDbContext dbContext,
            InpSettingService settingService,
            InpEpisodeNumberService episodeNumberService)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _episodeNumberService = episodeNumberService;
        }

        // =====================================================================
        // BE-RWI-007 — Membuka admisi
        // =====================================================================

        /// <summary>
        /// Membuka admisi rawat inap. Episode lahir berstatus <c>Draft</c> dengan nomor
        /// berawalan dari master, menempel pada tepat satu kunjungan, dan sudah punya DPJP
        /// sejak detik pertama.
        /// </summary>
        /// <remarks>
        /// Seluruh penulisan berada di dalam satu transaksi: kunjungan bila memang dibuat,
        /// episode, penugasan DPJP pertama, dan baris riwayat status pertama. Bila salah satu
        /// gagal, tidak ada satu pun yang tersimpan.
        ///
        /// <para>
        /// <b>Contoh.</b> Ibu Rina datang pukul 07:00 untuk operasi terencana dan hari itu
        /// tidak melewati poliklinik sama sekali. Petugas membuka admisi tanpa menunjuk
        /// kunjungan. Sistem membuat kunjungan bertipe rawat inap atas nama Ibu Rina, lalu
        /// episodenya menempel di situ. Petugas tidak perlu mendaftarkan kunjungan poliklinik
        /// yang sebenarnya tidak terjadi, sehingga laporan kunjungan poliklinik tetap bersih.
        /// </para>
        /// </remarks>
        public async Task<InpEpisodeOperationResult> OpenAdmissionAsync(
            OpenAdmissionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return InpEpisodeOperationResult.Invalid("Isian admisi belum dikirim.");
            }

            if (request.PatientId == Guid.Empty)
            {
                return InpEpisodeOperationResult.Invalid("Pasien belum dipilih.");
            }

            if (request.DoctorId == Guid.Empty)
            {
                return InpEpisodeOperationResult.Invalid("Dokter penanggung jawab belum dipilih.");
            }

            var patient = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.PatientId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (patient == null)
            {
                return InpEpisodeOperationResult.Invalid("Pasien belum dipilih.");
            }

            var doctorExists = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.DoctorId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (!doctorExists)
            {
                return InpEpisodeOperationResult.Invalid(
                    "Dokter penanggung jawab yang dipilih tidak ditemukan atau tidak aktif.");
            }

            var contextCheck = await ValidateServiceUnitAndPatientClassAsync(
                request.ServiceUnitId,
                request.PatientClassId,
                cancellationToken);

            if (contextCheck != null)
            {
                return contextCheck;
            }

            TrxPatientEncounter? encounter = null;
            var encounterCreatedByAdmission = false;

            if (request.EncounterId.HasValue && request.EncounterId.Value != Guid.Empty)
            {
                var requestedEncounterId = request.EncounterId.Value;

                var existingEncounter = await _dbContext.Set<TrxPatientEncounter>()
                    .FirstOrDefaultAsync(
                        x => x.Id == requestedEncounterId && !x.IsDelete,
                        cancellationToken);

                if (existingEncounter == null)
                {
                    return InpEpisodeOperationResult.NotFound(
                        "Kunjungan yang dipilih tidak ditemukan.");
                }

                if (existingEncounter.EncounterType != EncounterType.Inpatient)
                {
                    return InpEpisodeOperationResult.BusinessRuleRejected(
                        "Kunjungan yang dipilih bukan kunjungan rawat inap.");
                }

                if (existingEncounter.PatientId != request.PatientId)
                {
                    return InpEpisodeOperationResult.BusinessRuleRejected(
                        "Kunjungan yang dipilih bukan milik pasien ini.");
                }

                // INV-INP-04 — satu kunjungan menampung paling banyak satu episode. Index
                // unik IX_InpEpisode_EncounterId adalah penjaga terakhirnya; pemeriksaan di
                // sini ada supaya petugas menerima pesan yang dapat dibacanya, bukan galat
                // basis data.
                var encounterAlreadyUsed = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.EncounterId == requestedEncounterId && !x.IsDelete,
                        cancellationToken);

                if (encounterAlreadyUsed)
                {
                    return InpEpisodeOperationResult.Conflict(
                        "Kunjungan ini sudah punya episode rawat inap.");
                }

                encounter = existingEncounter;
            }

            // BE-RWI-031 — hubungan bayi dan ibu. Diperiksa sebelum apa pun ditulis, supaya
            // rujukan yang keliru tidak pernah tersimpan walaupun sesaat.
            var motherCheck = await ValidateMotherEpisodeAsync(
                request.MotherEpisodeId,
                Guid.Empty,
                request.PatientId,
                cancellationToken);

            if (motherCheck != null)
            {
                return motherCheck;
            }

            // RWI-AC-006 turunan: admisi Draft ganda adalah peringatan, bukan penolakan.
            // Petugas boleh melanjutkan, atau membatalkan yang lama lebih dulu.
            var warnings = new List<string>();

            var otherDraft = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x =>
                    x.PatientId == request.PatientId &&
                    x.EpisodeStatus == InpEpisodeStatus.Draft &&
                    !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new { x.EpisodeNumber, x.CreateDateTime })
                .FirstOrDefaultAsync(cancellationToken);

            if (otherDraft != null)
            {
                warnings.Add(
                    $"Pasien ini punya admisi lain yang sedang disiapkan, yaitu " +
                    $"{otherDraft.EpisodeNumber} sejak {otherDraft.CreateDateTime:dd MMMM yyyy HH:mm}. " +
                    "Lanjutkan bila memang admisi terpisah, atau batalkan yang lama lebih dulu.");
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                if (encounter == null)
                {
                    encounter = BuildInpatientEncounter(request, actorUserId, now);
                    encounterCreatedByAdmission = true;

                    _dbContext.Set<TrxPatientEncounter>().Add(encounter);
                }

                var episode = new InpEpisode
                {
                    Id = Guid.NewGuid(),
                    EpisodeNumber = await _episodeNumberService.GenerateAsync(cancellationToken),
                    EncounterId = encounter.Id,
                    PatientId = request.PatientId,
                    ServiceUnitId = request.ServiceUnitId,
                    PatientClassId = request.PatientClassId,
                    EpisodeStatus = InpEpisodeStatus.Draft,
                    MotherEpisodeId = request.MotherEpisodeId == Guid.Empty
                        ? null
                        : request.MotherEpisodeId,
                    Notes = NormalizeText(request.Notes),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<InpEpisode>().Add(episode);

                // INV-INP-03 — DPJP ada sejak detik pertama, bukan dilengkapi kemudian.
                var doctorAssignment = new InpDoctorAssignment
                {
                    Id = Guid.NewGuid(),
                    EpisodeId = episode.Id,
                    DoctorId = request.DoctorId,
                    SequenceNumber = 1,
                    StartDateTime = now,
                    EndDateTime = null,
                    AssignedByUserId = actorUserId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<InpDoctorAssignment>().Add(doctorAssignment);
                episode.DoctorAssignments.Add(doctorAssignment);

                await ApplyStatusChangeAsync(
                    episode,
                    fromStatus: null,
                    toStatus: InpEpisodeStatus.Draft,
                    actionType: encounterCreatedByAdmission
                        ? ActionOpenAdmissionWithEncounter
                        : ActionOpenAdmission,
                    actorType: InpStatusChangeActorType.User,
                    changedByUserId: actorUserId,
                    reason: null,
                    now: now,
                    touchEpisode: false,
                    cancellationToken: cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpEpisodeOperationResult.Success(
                    episode,
                    "Admisi berhasil dibuka.",
                    warnings);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                // Dua petugas membuka admisi pada kunjungan yang sama pada saat hampir
                // bersamaan. Keduanya lolos pemeriksaan di atas, lalu index unik menolak yang
                // kalah. INV-INP-04 tetap tidak pernah dilanggar.
                return InpEpisodeOperationResult.Conflict(
                    "Kunjungan ini sudah punya episode rawat inap.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        // =====================================================================
        // BE-RWI-008 — Mengubah, membatalkan, dan menggugurkan admisi Draft
        // =====================================================================

        /// <summary>
        /// Membetulkan isian admisi selagi episode masih <c>Draft</c>. Episode yang sudah
        /// berjalan tidak dapat diubah lewat jalur ini.
        /// </summary>
        public async Task<InpEpisodeOperationResult> UpdateAdmissionAsync(
            Guid episodeId,
            UpdateAdmissionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return InpEpisodeOperationResult.Invalid("Isian admisi belum dikirim.");
            }

            var episode = await LoadEpisodeForWriteAsync(episodeId, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            // Pembacaan inilah yang menggugurkan Draft telantar. Episode yang sudah lewat
            // batas dibaca sebagai Cancelled, dan permintaan perubahan ini ditolak karenanya.
            if (await ExpireDraftIfDueAsync(episode, cancellationToken))
            {
                return InpEpisodeOperationResult.Conflict(
                    "Admisi ini sudah gugur karena ditinggalkan melewati batas waktu, " +
                    "sehingga tidak dapat diubah lagi.",
                    episode);
            }

            if (episode.EpisodeStatus != InpEpisodeStatus.Draft)
            {
                return InpEpisodeOperationResult.Conflict(
                    "Isian admisi hanya dapat diubah selama pasien belum ditempatkan.",
                    episode);
            }

            var contextCheck = await ValidateServiceUnitAndPatientClassAsync(
                request.ServiceUnitId,
                request.PatientClassId,
                cancellationToken);

            if (contextCheck != null)
            {
                return contextCheck;
            }

            var motherCheck = await ValidateMotherEpisodeAsync(
                request.MotherEpisodeId,
                episode.Id,
                episode.PatientId,
                cancellationToken);

            if (motherCheck != null)
            {
                return motherCheck;
            }

            var now = DateTime.UtcNow;

            episode.ServiceUnitId = request.ServiceUnitId;
            episode.PatientClassId = request.PatientClassId;
            episode.MotherEpisodeId = request.MotherEpisodeId == Guid.Empty
                ? null
                : request.MotherEpisodeId;
            episode.Notes = NormalizeText(request.Notes);
            episode.UpdateDateTime = now;
            episode.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return InpEpisodeOperationResult.Success(episode, "Isian admisi berhasil diubah.");
        }

        /// <summary>
        /// Membatalkan admisi yang tidak jadi berjalan. Pelepasan pemesanan dan penempatan
        /// adalah bagian dari pembatalan, bukan langkah terpisah yang dikerjakan petugas
        /// sesudahnya.
        /// </summary>
        /// <param name="actorIsSupervisorOrWardHead">
        /// Benar bila pelakunya supervisor atau kepala ruangan. Hanya mereka yang boleh
        /// membatalkan episode yang sudah <c>Admitted</c>; petugas admisi berhenti di
        /// <c>Draft</c>. Penjaga ini berada di service, bukan di mesin hak akses, karena
        /// keduanya memakai butir hak akses yang sama yaitu <c>InpatientEpisode : Update</c>.
        /// </param>
        public async Task<InpEpisodeOperationResult> CancelAdmissionAsync(
            Guid episodeId,
            CancelAdmissionRequest request,
            Guid actorUserId,
            bool actorIsSupervisorOrWardHead,
            CancellationToken cancellationToken = default)
        {
            if (request == null || !HasMeaningfulReason(request.Reason))
            {
                return InpEpisodeOperationResult.Invalid(
                    "Alasan pembatalan wajib diisi dengan kalimat yang dapat dibaca.");
            }

            var episode = await LoadEpisodeForWriteAsync(episodeId, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            if (await ExpireDraftIfDueAsync(episode, cancellationToken))
            {
                return InpEpisodeOperationResult.Conflict(
                    "Admisi ini sudah gugur sendiri karena ditinggalkan melewati batas waktu.",
                    episode);
            }

            switch (episode.EpisodeStatus)
            {
                case InpEpisodeStatus.Draft:
                    break;

                case InpEpisodeStatus.Admitted:
                    if (!actorIsSupervisorOrWardHead)
                    {
                        return InpEpisodeOperationResult.Forbidden(
                            "Pembatalan episode yang sudah berjalan hanya dapat dilakukan " +
                            "supervisor atau kepala ruangan.");
                    }

                    // Batas "belum ada catatan klinis" pada RWI-RULE-004 belum dapat
                    // diperiksa di sini: keenam jenis catatan klinis itu milik modul
                    // ClinicalManagement dan PharmacyManagement, dan pemeriksaannya belum
                    // diberi wewenang task ini. Lihat laporan BE-RWI-008 bagian risiko.
                    break;

                case InpEpisodeStatus.DischargePending:
                    return InpEpisodeOperationResult.BusinessRuleRejected(
                        "Episode yang sudah diputuskan pulang tidak dapat dibatalkan.",
                        episode);

                case InpEpisodeStatus.Closed:
                    return InpEpisodeOperationResult.Conflict(
                        "Episode sudah ditutup. Pasien yang kembali dirawat memerlukan admisi baru.",
                        episode);

                default:
                    return InpEpisodeOperationResult.Conflict(
                        "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan.",
                        episode);
            }

            var now = DateTime.UtcNow;
            var reason = request.Reason.Trim();

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                await CancelEpisodeInternalAsync(
                    episode,
                    reason,
                    actionType: ActionCancelAdmission,
                    actorType: InpStatusChangeActorType.User,
                    changedByUserId: actorUserId,
                    now: now,
                    cancellationToken: cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpEpisodeOperationResult.Success(episode, "Admisi berhasil dibatalkan.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Membaca satu episode beserta perhitungan kedaluwarsa <c>Draft</c>-nya. Dipakai
        /// controller setelah tindakan, dan menjadi titik pembacaan yang menggugurkan admisi
        /// telantar tanpa program penjadwal.
        /// </summary>
        public async Task<InpEpisodeOperationResult> GetEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await LoadEpisodeForWriteAsync(episodeId, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            await ExpireDraftIfDueAsync(episode, cancellationToken);

            return InpEpisodeOperationResult.Success(episode, "Episode berhasil diambil.");
        }

        // =====================================================================
        // Satu pintu perubahan status
        // =====================================================================

        /// <summary>
        /// Satu-satunya tempat <c>InpEpisode.EpisodeStatus</c> boleh berubah. Selalu menulis
        /// satu baris <c>InpStatusHistory</c>, dan barisnya masuk ke perubahan yang sama
        /// dengan perubahan statusnya sendiri.
        /// </summary>
        /// <remarks>
        /// <b>Kenapa harus satu pintu.</b> Bila satu controller saja menyetel
        /// <c>EpisodeStatus</c> langsung, riwayat status berlubang. Laporan pengecualian,
        /// daftar pantau, dan pembuktian belum adanya catatan klinis saat pembatalan semuanya
        /// dibaca dari tabel riwayat itu — dan tidak satu pun dari mereka dapat mengetahui
        /// bahwa ada perpindahan yang tidak tercatat. Aturan ini ditegakkan lewat review,
        /// bukan lewat harapan.
        ///
        /// <para>
        /// <c>SequenceNumber</c> dihitung dari nomor urut terakhir milik episode yang sama,
        /// di dalam transaksi yang sama, dan dijaga index unik
        /// <c>(EpisodeId, SequenceNumber)</c>. Ini nomor urut riwayat, bukan nomor bisnis
        /// yang dilihat pengguna, sehingga ia bukan alokasi kode yang diatur QBE-CODE-003;
        /// nomor bisnis modul ini tetap dibentuk <see cref="InpEpisodeNumberService"/>.
        /// </para>
        /// </remarks>
        public async Task ApplyStatusChangeAsync(
            InpEpisode episode,
            InpEpisodeStatus? fromStatus,
            InpEpisodeStatus toStatus,
            string actionType,
            InpStatusChangeActorType actorType,
            Guid? changedByUserId,
            string? reason,
            DateTime now,
            bool touchEpisode,
            CancellationToken cancellationToken)
        {
            var persistedSequence = await _dbContext.Set<InpStatusHistory>()
                .Where(x => x.EpisodeId == episode.Id)
                .Select(x => (int?)x.SequenceNumber)
                .MaxAsync(cancellationToken) ?? 0;

            var pendingSequence = episode.StatusHistories.Count == 0
                ? 0
                : episode.StatusHistories.Max(x => x.SequenceNumber);

            var history = new InpStatusHistory
            {
                Id = Guid.NewGuid(),
                EpisodeId = episode.Id,
                SequenceNumber = Math.Max(persistedSequence, pendingSequence) + 1,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ActionType = actionType,
                ActorType = actorType,
                ChangedByUserId = changedByUserId,
                ChangedAt = now,
                Reason = reason,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = changedByUserId ?? Guid.Empty
            };

            episode.EpisodeStatus = toStatus;

            if (touchEpisode)
            {
                episode.UpdateDateTime = now;
                episode.UpdateBy = changedByUserId ?? Guid.Empty;
            }

            _dbContext.Set<InpStatusHistory>().Add(history);
            episode.StatusHistories.Add(history);
        }

        // =====================================================================
        // Kedaluwarsa Draft — dihitung saat dibaca, tanpa program penjadwal
        // =====================================================================

        /// <summary>
        /// Membatalkan episode <c>Draft</c> yang tidak disentuh melewati
        /// <c>DraftEpisodeExpiryHours</c>, lalu menyimpannya. Mengembalikan <c>true</c> bila
        /// pembacaan inilah yang menggugurkannya.
        /// </summary>
        /// <remarks>
        /// Batas jamnya dibaca ulang setiap pemanggilan lewat <see cref="InpSettingService"/>,
        /// sehingga angka yang baru diubah admin berlaku pada pembacaan berikutnya tanpa
        /// aplikasi dinyalakan ulang.
        ///
        /// <para>
        /// "Tidak disentuh" dihitung dari perubahan terakhir pada barisnya, yaitu
        /// <c>UpdateDateTime</c> bila ada, dan <c>CreateDateTime</c> bila episode belum pernah
        /// diubah sama sekali.
        /// </para>
        ///
        /// <para>
        /// <b>Kenapa disimpan di sini, bukan diserahkan ke pemanggil.</b> Kedaluwarsa yang
        /// hanya dihitung tanpa disimpan akan dihitung ulang pada setiap pembacaan berikutnya,
        /// dan pembatalannya tidak pernah benar-benar terjadi: pemesanan tidak dilepas,
        /// kunjungan tidak ditandai batal, dan tidak ada satu pun baris riwayat yang lahir.
        /// Sebagian besar pemanggilnya justru menolak permintaannya setelah itu, sehingga
        /// tidak ada satu pun yang akan sempat menyimpannya.
        /// </para>
        /// </remarks>
        private async Task<bool> ExpireDraftIfDueAsync(
            InpEpisode episode,
            CancellationToken cancellationToken)
        {
            if (episode.EpisodeStatus != InpEpisodeStatus.Draft)
            {
                return false;
            }

            var setting = await _settingService.GetEffectiveSettingAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var lastTouchedAt = episode.UpdateDateTime ?? episode.CreateDateTime;
            var expiresAt = lastTouchedAt.AddHours(setting.DraftEpisodeExpiryHours);

            if (now <= expiresAt)
            {
                return false;
            }

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                await CancelEpisodeInternalAsync(
                    episode,
                    SystemExpiryReason,
                    actionType: ActionExpireDraft,
                    actorType: InpStatusChangeActorType.System,
                    changedByUserId: null,
                    now: now,
                    cancellationToken: cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Isi pembatalan yang dipakai bersama oleh pembatalan manusia dan kedaluwarsa
        /// sistem. Melepas pemesanan dan penempatan, membatalkan kunjungan yang lahir bersama
        /// episode, lalu memindahkan status lewat satu pintu.
        /// </summary>
        private async Task CancelEpisodeInternalAsync(
            InpEpisode episode,
            string reason,
            string actionType,
            InpStatusChangeActorType actorType,
            Guid? changedByUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var fromStatus = episode.EpisodeStatus;

            await ReleaseBedHoldsAsync(episode, changedByUserId, now, cancellationToken);

            if (await WasEncounterCreatedByAdmissionAsync(episode.Id, cancellationToken))
            {
                await CancelAnchorEncounterAsync(
                    episode.EncounterId,
                    reason,
                    changedByUserId,
                    now,
                    cancellationToken);
            }

            episode.CancelReason = reason;
            episode.IsCancel = true;
            episode.CancelDateTime = now;
            episode.CancelBy = changedByUserId ?? Guid.Empty;
            episode.IsActive = false;

            await ApplyStatusChangeAsync(
                episode,
                fromStatus: fromStatus,
                toStatus: InpEpisodeStatus.Cancelled,
                actionType: actionType,
                actorType: actorType,
                changedByUserId: changedByUserId,
                reason: reason,
                now: now,
                touchEpisode: true,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Melepas pemesanan dan penempatan yang masih hidup milik episode, sebagai bagian
        /// dari pembatalan. Bila pelepasan gagal, pembatalannya ikut gagal dan episode tetap
        /// seperti semula — itulah yang menutup risiko kamar yang terlihat penuh padahal
        /// pasiennya tidak pernah ada.
        /// </summary>
        /// <remarks>
        /// <b>Salinan status tempat tidur ikut dikembalikan.</b> Sampai `BE-RWI-008`, method
        /// ini hanya menutup baris pemesanan dan penempatan, sehingga <c>MstBed.BedStatus</c>
        /// tetap bernilai <c>Reserved</c> atau <c>Occupied</c> untuk pasien yang admisinya
        /// sudah batal — tempat tidur yang sesungguhnya kosong tetap terlihat terpakai pada
        /// layar master. `BE-RWI-011` menutup celah itu di sini, di dalam transaksi yang sama
        /// dengan pembatalannya. Ini arah tulis <c>INT-INP-03</c> yang disetujui
        /// <c>RWI-DEC-062</c>: modul ini hanya boleh menulis <c>Available</c>,
        /// <c>Reserved</c>, dan <c>Occupied</c>.
        /// </remarks>
        private async Task ReleaseBedHoldsAsync(
            InpEpisode episode,
            Guid? actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var touchedBedIds = new List<Guid>();

            var reservations = await _dbContext.Set<InpBedReservation>()
                .Where(x =>
                    x.EpisodeId == episode.Id &&
                    x.ReservationStatus == InpBedReservationStatus.Active &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var reservation in reservations)
            {
                reservation.ReservationStatus = InpBedReservationStatus.Cancelled;
                reservation.ReleasedAt = now;
                reservation.IsActive = false;
                reservation.UpdateDateTime = now;
                reservation.UpdateBy = actorUserId ?? Guid.Empty;

                touchedBedIds.Add(reservation.BedId);
            }

            var placements = await _dbContext.Set<InpBedPlacement>()
                .Where(x =>
                    x.EpisodeId == episode.Id &&
                    x.EndDateTime == null &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var placement in placements)
            {
                placement.EndDateTime = now;
                placement.EndReason = InpBedPlacementEndReason.AdmissionCancelled;
                placement.EndedByUserId = actorUserId;
                placement.IsActive = false;
                placement.UpdateDateTime = now;
                placement.UpdateBy = actorUserId ?? Guid.Empty;

                touchedBedIds.Add(placement.BedId);
            }

            await RestoreBedStatusCopyAsync(
                touchedBedIds,
                episode.Id,
                actorUserId,
                now,
                cancellationToken);
        }

        /// <summary>
        /// Mengembalikan salinan status tempat tidur menjadi <c>Available</c> untuk tempat
        /// tidur yang sudah tidak dipegang pemesanan maupun penempatan siapa pun.
        /// </summary>
        /// <remarks>
        /// Tempat tidur yang sedang <c>Cleaning</c>, <c>Maintenance</c>, <c>Blocked</c>, atau
        /// <c>Inactive</c> <b>tidak</b> disentuh. Keempat nilai itu tetap wewenang admin
        /// master data, dan menimpanya berarti tempat tidur yang sedang diperbaiki kembali
        /// muncul sebagai siap pakai — persis kejadian yang dicegah <c>RWI-DEC-062</c>.
        ///
        /// <para>
        /// <b>Kenapa episode pemanggil dikeluarkan dari pemeriksaan.</b> Baris pemesanan dan
        /// penempatan milik episode ini baru saja ditutup <b>di memori</b> dan belum disimpan,
        /// sehingga query ke database masih membacanya sebagai aktif. Bila ia ikut dihitung,
        /// tidak ada satu pun tempat tidur yang pernah kembali <c>Available</c>. Pemegang milik
        /// episode <b>lain</b> tidak punya masalah itu, karena tidak ada perubahannya yang
        /// tertahan di memori pada saat ini.
        /// </para>
        /// </remarks>
        private async Task RestoreBedStatusCopyAsync(
            IEnumerable<Guid> bedIds,
            Guid releasingEpisodeId,
            Guid? actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var distinctBedIds = bedIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (distinctBedIds.Count == 0)
            {
                return;
            }

            var beds = await _dbContext.Set<MstBed>()
                .Where(x => distinctBedIds.Contains(x.Id) && !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var bed in beds)
            {
                if (bed.BedStatus != BedStatus.Occupied && bed.BedStatus != BedStatus.Reserved)
                {
                    continue;
                }

                var stillHeld =
                    await _dbContext.Set<InpBedPlacement>()
                        .AnyAsync(
                            x =>
                                x.BedId == bed.Id &&
                                x.EpisodeId != releasingEpisodeId &&
                                x.EndDateTime == null &&
                                !x.IsDelete,
                            cancellationToken)
                    || await _dbContext.Set<InpBedReservation>()
                        .AnyAsync(
                            x =>
                                x.BedId == bed.Id &&
                                x.EpisodeId != releasingEpisodeId &&
                                x.ReservationStatus == InpBedReservationStatus.Active &&
                                !x.IsDelete,
                            cancellationToken);

                if (stillHeld)
                {
                    continue;
                }

                bed.BedStatus = BedStatus.Available;
                bed.UpdateDateTime = now;
                bed.UpdateBy = actorUserId ?? Guid.Empty;
            }
        }

        /// <summary>
        /// Membatalkan kunjungan yang lahir bersama episode, supaya ia tidak muncul sebagai
        /// kunjungan rawat inap yang benar-benar terjadi pada laporan kunjungan.
        /// </summary>
        /// <remarks>
        /// Hanya dijalankan untuk kunjungan yang memang dibuat proses admisi. Kunjungan yang
        /// ditunjuk petugas adalah milik alur pendaftaran dan tidak pernah dibatalkan modul
        /// ini.
        /// </remarks>
        private async Task CancelAnchorEncounterAsync(
            Guid encounterId,
            string reason,
            Guid? actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var encounter = await _dbContext.Set<TrxPatientEncounter>()
                .FirstOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);

            if (encounter == null || encounter.EncounterStatus == EncounterStatus.Cancelled)
            {
                return;
            }

            encounter.EncounterStatus = EncounterStatus.Cancelled;
            encounter.CancelledAt = now;
            encounter.CancelledByUserId = actorUserId;
            encounter.CancelReason = Truncate(reason, 250);
            encounter.IsCancel = true;
            encounter.CancelDateTime = now;
            encounter.CancelBy = actorUserId ?? Guid.Empty;
            encounter.IsActive = false;
            encounter.UpdateDateTime = now;
            encounter.UpdateBy = actorUserId ?? Guid.Empty;
        }

        /// <summary>
        /// Membaca penanda provenance dari baris riwayat pertama: benar bila kunjungan
        /// jangkar dibuat sendiri oleh proses admisi.
        /// </summary>
        public async Task<bool> WasEncounterCreatedByAdmissionAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<InpStatusHistory>()
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.EpisodeId == episodeId &&
                        x.ActionType == ActionOpenAdmissionWithEncounter,
                    cancellationToken);
        }

        // =====================================================================
        // Penyusunan balasan
        // =====================================================================

        /// <summary>
        /// Menyusun balasan satu episode beserta nama-nama yang dibaca dari master. Nama
        /// tidak pernah disalin ke tabel modul ini; ia dibaca lewat projection saat query,
        /// sesuai batas integrasi modul.
        /// </summary>
        /// <remarks>
        /// Controller tidak menyentuh <c>ApplicationDbContext</c> sendiri. Seluruh pembacaan
        /// lewat service pemiliknya, sesuai QBE-SVC-001.
        /// </remarks>
        public async Task<InpatientEpisodeDetailResponse?> GetDetailResponseAsync(
            Guid episodeId,
            List<string>? warnings = null,
            CancellationToken cancellationToken = default)
        {
            var detail = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new InpatientEpisodeDetailResponse
                {
                    Id = x.Id,
                    EpisodeNumber = x.EpisodeNumber,
                    EncounterId = x.EncounterId,
                    EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    EpisodeStatus = (int)x.EpisodeStatus,
                    AdmittedAt = x.AdmittedAt,
                    DischargeDecidedAt = x.DischargeDecidedAt,
                    PhysicallyLeftAt = x.PhysicallyLeftAt,
                    ClosedAt = x.ClosedAt,
                    RequiresIsolation = x.RequiresIsolation,
                    IsolationNote = x.IsolationNote,
                    IsolationSource = x.IsolationSource != null ? (int)x.IsolationSource : null,
                    IsolationSetByUserId = x.IsolationSetByUserId,
                    IsolationSetByDoctorId = x.IsolationSetByDoctorId,
                    IsolationSetAt = x.IsolationSetAt,
                    DischargeType = (int)x.DischargeType,
                    IsClosedWithoutFinancialClearance = x.IsClosedWithoutFinancialClearance,
                    ClosedWithoutClearanceReason = x.ClosedWithoutClearanceReason,
                    MotherEpisodeId = x.MotherEpisodeId,
                    MotherEpisodeNumber = x.MotherEpisode != null
                        ? x.MotherEpisode.EpisodeNumber
                        : null,
                    MotherPatientName = x.MotherEpisode != null && x.MotherEpisode.Patient != null
                        ? x.MotherEpisode.Patient.FullName
                        : null,
                    CancelReason = x.CancelReason,
                    Notes = x.Notes,
                    ActiveDoctor = x.DoctorAssignments
                        .Where(d => d.EndDateTime == null && !d.IsDelete)
                        .OrderByDescending(d => d.SequenceNumber)
                        .Select(d => new InpatientEpisodeActiveDoctorResponse
                        {
                            AssignmentId = d.Id,
                            DoctorId = d.DoctorId,
                            DoctorName = d.Doctor != null ? d.Doctor.FullName : null,
                            SequenceNumber = d.SequenceNumber,
                            StartDateTime = d.StartDateTime
                        })
                        .FirstOrDefault(),
                    ActiveNurse = x.NurseAssignments
                        .Where(n => n.EndDateTime == null && !n.IsDelete)
                        .OrderByDescending(n => n.SequenceNumber)
                        .Select(n => new InpatientEpisodeActiveNurseResponse
                        {
                            AssignmentId = n.Id,
                            EmployeeId = n.EmployeeId,
                            EmployeeName = n.Employee != null ? n.Employee.FullName : null,
                            SequenceNumber = n.SequenceNumber,
                            StartDateTime = n.StartDateTime
                        })
                        .FirstOrDefault(),
                    // Lokasi terkini dibaca dari catatan penempatan, tidak pernah dari kolom
                    // pada episode. Larangan itu ditulis pada roadmap BE-RWI-009 bagian risiko
                    // dan dikunci blueprint-manifest.md bagian 8.
                    CurrentLocation = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .OrderByDescending(p => p.SequenceNumber)
                        .Select(p => new InpatientEpisodeCurrentLocationResponse
                        {
                            PlacementId = p.Id,
                            BedId = p.BedId,
                            BedCode = p.Bed != null ? p.Bed.BedCode : null,
                            BedName = p.Bed != null ? p.Bed.BedName : null,
                            RoomId = p.RoomId,
                            RoomName = p.Room != null ? p.Room.RoomName : null,
                            ServiceUnitId = p.ServiceUnitId,
                            ServiceUnitName = p.ServiceUnit != null ? p.ServiceUnit.ServiceUnitName : null,
                            PatientClassId = p.PatientClassId,
                            PatientClassName = p.PatientClass != null ? p.PatientClass.PatientClassName : null,
                            StartDateTime = p.StartDateTime
                        })
                        .FirstOrDefault(),
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (detail == null)
            {
                return null;
            }

            // Nama status dibentuk setelah baris dibaca, bukan di dalam projection. Pembentukan
            // nama enum di dalam query menjadi beban penyedia database, dan tidak setiap
            // penyedia dapat menerjemahkannya.
            detail.EpisodeStatusName = ((InpEpisodeStatus)detail.EpisodeStatus).ToString();
            detail.DischargeTypeName = ((InpDischargeType)detail.DischargeType).ToString();
            detail.IsolationSourceName = detail.IsolationSource.HasValue
                ? ((InpIsolationSource)detail.IsolationSource.Value).ToString()
                : null;

            detail.IsEncounterCreatedByAdmission =
                await WasEncounterCreatedByAdmissionAsync(episodeId, cancellationToken);

            if (warnings != null && warnings.Count > 0)
            {
                detail.Warnings.AddRange(warnings);
            }

            return detail;
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Menyusun kunjungan bertipe rawat inap untuk jalur pasien datang langsung.
        /// </summary>
        /// <remarks>
        /// <b>Nomor kunjungan.</b> Nomornya dibentuk dari awalan kunjungan yang sudah dipakai
        /// pendaftaran, ditambah waktu sampai detik dan enam huruf/angka acak — pola yang
        /// sama dengan <see cref="InpEpisodeNumberService"/>. Alokator lama milik pendaftaran
        /// menyisir seluruh baris lalu memakai celah pertama, dan cara itu dilarang
        /// QBE-CODE-003 untuk kode baru karena dua permintaan bersamaan membaca angka yang
        /// sama. Index unik pada <c>TrxPatientEncounter.EncounterNumber</c> menjadi penjaga
        /// terakhirnya. Bentuk nomor ini berbeda dari nomor kunjungan pendaftaran, dan
        /// bedanya dicatat pada laporan task untuk ditinjau pemilik modul Registrasi.
        ///
        /// <para>
        /// Kelas pasien tidak dipaksa menjadi <c>RAWAT JALAN</c> di sini. Pemaksaan itu ada
        /// pada <c>PatientEncounterController.ResolvePatientClassAsync</c> dan hanya berlaku
        /// ketika <c>EncounterType</c> bernilai <c>Outpatient</c>, sehingga kunjungan rawat
        /// inap tidak tersentuh olehnya.
        /// </para>
        /// </remarks>
        private static TrxPatientEncounter BuildInpatientEncounter(
            OpenAdmissionRequest request,
            Guid actorUserId,
            DateTime now)
        {
            return new TrxPatientEncounter
            {
                Id = Guid.NewGuid(),
                EncounterNumber = BuildInpatientEncounterNumber(now),
                PatientId = request.PatientId,
                ServiceUnitId = request.ServiceUnitId,
                PatientClassId = request.PatientClassId,
                DoctorId = request.DoctorId,
                EncounterDate = now,
                EncounterType = EncounterType.Inpatient,
                VisitType = VisitType.NewVisit,
                RegistrationSource = EncounterRegistrationSource.FrontDesk,
                EncounterStatus = EncounterStatus.Registered,
                PaymentType = EncounterPaymentType.Cash,
                IsWalkIn = true,
                IsQueueRequired = false,
                IsScreeningRequired = false,
                IsDoctorRequired = true,
                RegisteredAt = now,
                RegisteredByUserId = actorUserId,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
        }

        private static string BuildInpatientEncounterNumber(DateTime now)
        {
            var random = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

            return $"ENC-RSMMC-{now:yyMMddHHmmss}-{random}";
        }

        /// <summary>
        /// Memeriksa unit layanan dan kelas perawatan. Mengembalikan <c>null</c> bila
        /// keduanya lolos.
        /// </summary>
        private async Task<InpEpisodeOperationResult?> ValidateServiceUnitAndPatientClassAsync(
            Guid serviceUnitId,
            Guid patientClassId,
            CancellationToken cancellationToken)
        {
            if (serviceUnitId == Guid.Empty)
            {
                return InpEpisodeOperationResult.Invalid("Unit layanan belum dipilih.");
            }

            if (patientClassId == Guid.Empty)
            {
                return InpEpisodeOperationResult.Invalid("Kelas perawatan belum dipilih.");
            }

            var serviceUnit = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == serviceUnitId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (serviceUnit == null)
            {
                return InpEpisodeOperationResult.Invalid(
                    "Unit layanan yang dipilih tidak ditemukan atau tidak aktif.");
            }

            if (serviceUnit.ServiceUnitType != ServiceUnitType.Inpatient)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Unit layanan yang dipilih bukan unit rawat inap.");
            }

            var patientClass = await _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == patientClassId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (patientClass == null)
            {
                return InpEpisodeOperationResult.Invalid(
                    "Kelas perawatan yang dipilih tidak ditemukan atau tidak aktif.");
            }

            if (!patientClass.IsForInpatient)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Kelas perawatan yang dipilih tidak berlaku untuk rawat inap.");
            }

            return null;
        }

        private Task<InpEpisode?> LoadEpisodeForWriteAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            return _dbContext.Set<InpEpisode>()
                .Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
        }

        /// <summary>
        /// Alasan yang hanya berisi tanda baca atau spasi ditolak, sesuai <c>RWI-AC-008</c>.
        /// </summary>
        private static bool HasMeaningfulReason(string? reason)
        {
            return !string.IsNullOrWhiteSpace(reason) && reason.Any(char.IsLetterOrDigit);
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value[..maxLength];
        }

    }

    /// <summary>
    /// Hasil satu tindakan pada episode, beserta kode status HTTP yang harus dipakai
    /// controller. Bentuk ini mengikuti pola <c>InpatientSettingUpdateResult</c> yang sudah
    /// dipakai layar master Rawat Inap.
    /// </summary>
    public enum InpEpisodeOperationStatus
    {
        /// <summary>Berhasil — 200.</summary>
        Success = 0,

        /// <summary>Isian tidak lengkap atau tidak masuk akal — 400.</summary>
        Invalid = 1,

        /// <summary>Yang dimaksud tidak ditemukan — 404.</summary>
        NotFound = 2,

        /// <summary>Bertabrakan dengan keadaan sekarang — 409.</summary>
        Conflict = 3,

        /// <summary>Aturan bisnis menolak — 422.</summary>
        BusinessRuleRejected = 4,

        /// <summary>Pelakunya tidak berwenang untuk tindakan ini — 403.</summary>
        Forbidden = 5
    }

    /// <summary>Hasil satu tindakan pada episode.</summary>
    public sealed class InpEpisodeOperationResult
    {
        private InpEpisodeOperationResult(
            InpEpisodeOperationStatus status,
            InpEpisode? episode,
            string message,
            List<string>? warnings = null)
        {
            Status = status;
            Episode = episode;
            Message = message;
            Warnings = warnings ?? new List<string>();
        }

        public InpEpisodeOperationStatus Status { get; }

        public InpEpisode? Episode { get; }

        public string Message { get; }

        public List<string> Warnings { get; }

        public static InpEpisodeOperationResult Success(
            InpEpisode episode,
            string message,
            List<string>? warnings = null)
            => new(InpEpisodeOperationStatus.Success, episode, message, warnings);

        public static InpEpisodeOperationResult Invalid(string message)
            => new(InpEpisodeOperationStatus.Invalid, null, message);

        public static InpEpisodeOperationResult NotFound(string message)
            => new(InpEpisodeOperationStatus.NotFound, null, message);

        public static InpEpisodeOperationResult Conflict(string message, InpEpisode? episode = null)
            => new(InpEpisodeOperationStatus.Conflict, episode, message);

        public static InpEpisodeOperationResult BusinessRuleRejected(
            string message,
            InpEpisode? episode = null)
            => new(InpEpisodeOperationStatus.BusinessRuleRejected, episode, message);

        public static InpEpisodeOperationResult Forbidden(string message)
            => new(InpEpisodeOperationStatus.Forbidden, null, message);
    }
}
