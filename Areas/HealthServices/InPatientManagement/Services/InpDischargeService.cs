using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Keputusan pulang, resume pulang, dan penyimpanan versi resume yang sudah digantikan.
    /// </summary>
    /// <remarks>
    /// <b>Yang sudah terisi.</b> Task <c>BE-RWI-020</c> mengisi keputusan pulang beserta
    /// <c>GUARD-INP-02</c>, <c>BE-RWI-021</c> mengisi resume beserta <c>GUARD-INP-03</c>, dan
    /// <c>BE-RWI-022</c> mengisi penyalinan versi. Perilaku berikutnya diisi task selanjutnya:
    ///
    /// <list type="bullet">
    /// <item><description><c>BE-RWI-023</c> — penandaan butir daftar periksa administrasi</description></item>
    /// <item><description><c>BE-RWI-024</c> — penandaan kelayakan keuangan oleh kasir</description></item>
    /// <item><description><c>BE-RWI-025</c> dan <c>BE-RWI-026</c> — lima syarat penutupan dan jalan keluar supervisor</description></item>
    /// <item><description><c>BE-RWI-027</c> — pencatatan kepergian fisik pasien</description></item>
    /// </list>
    ///
    /// <para>
    /// <b>Tempat tidur tidak dilepas oleh service ini pada revisi ini.</b> Keputusan pulang
    /// memindahkan episode ke <c>DischargePending</c> dan berhenti di situ: pasien masih di
    /// ruangan, masih muncul pada census, dan salinan <c>MstBed.BedStatus</c> tidak berubah.
    /// Pelepasannya menunggu pencatatan kepergian fisik atau penutupan episode.
    /// </para>
    /// </remarks>
    public class InpDischargeService
    {
        /// <summary>Nilai <c>ActionType</c> untuk keputusan pasien boleh pulang.</summary>
        public const string ActionDecideDischarge = "DecideDischarge";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpEpisodeService _episodeService;

        public InpDischargeService(
            ApplicationDbContext dbContext,
            InpEpisodeService episodeService)
        {
            _dbContext = dbContext;
            _episodeService = episodeService;
        }

        // =====================================================================
        // BE-RWI-020 — Keputusan pasien boleh pulang
        // =====================================================================

        /// <summary>
        /// DPJP aktif menyatakan pasien boleh pulang beserta cara pulangnya.
        /// </summary>
        /// <param name="actorDoctorId">
        /// Identitas dokter pemohon, dibaca dari klaim <c>doctor_id</c>. Kosong bila pemohon
        /// bukan dokter.
        /// </param>
        /// <remarks>
        /// <b><c>GUARD-INP-02</c>.</b> Hanya DPJP aktif episode itu yang boleh memutuskan.
        /// Dokter jaga, kepala ruangan, dan supervisor sama-sama ditolak 403 — keputusan
        /// pulang adalah keputusan klinis yang melekat pada penanggung jawab pelayanan, bukan
        /// pada jabatan.
        ///
        /// <para>
        /// <b>Dua cara pulang yang belum tersedia.</b> Meninggal dan kabur sengaja tidak punya
        /// nilai enum pada revisi ini. Sisi klinis keduanya masih terbuka pada
        /// <c>RWI-OQ-039</c> dan <c>RWI-DEC-059</c>, menunggu pemilik klinis. Nomor 4 dan 5
        /// dikosongkan supaya penambahannya kelak tidak mengubah angka yang sudah tersimpan.
        /// </para>
        ///
        /// <para>
        /// <b>Tempat tidur tetap terisi.</b> Salinan <c>MstBed.BedStatus</c> tidak disentuh
        /// method ini, dan pasien tetap muncul pada census. Ini disengaja: pasien yang sudah
        /// diizinkan pulang biasanya masih berada di kamarnya selama beberapa jam, dan
        /// menganggap tempat tidurnya kosong akan membuat pasien berikutnya ditempatkan di
        /// atasnya.
        /// </para>
        /// </remarks>
        public async Task<InpEpisodeOperationResult> DecideDischargeAsync(
            Guid episodeId,
            DecideDischargeRequest request,
            Guid actorUserId,
            Guid? actorDoctorId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return InpEpisodeOperationResult.Invalid("Isian keputusan pulang belum dikirim.");
            }

            if (request.DischargeType == (int)InpDischargeType.Unknown)
            {
                return InpEpisodeOperationResult.Invalid("Cara pulang wajib dipilih.");
            }

            if (!Enum.IsDefined(typeof(InpDischargeType), request.DischargeType))
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Cara pulang yang dipilih belum tersedia pada versi ini.");
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            switch (episode.EpisodeStatus)
            {
                case InpEpisodeStatus.Draft:
                    return InpEpisodeOperationResult.BusinessRuleRejected(
                        "Pasien belum menempati tempat tidur. Selesaikan penempatan lebih dulu.",
                        episode);

                case InpEpisodeStatus.DischargePending:
                    return InpEpisodeOperationResult.BusinessRuleRejected(
                        "Pasien sudah diputuskan boleh pulang sebelumnya.",
                        episode);

                case InpEpisodeStatus.Closed:
                    return InpEpisodeOperationResult.Conflict(
                        "Episode sudah ditutup. Pasien yang kembali dirawat memerlukan admisi baru.",
                        episode);

                case InpEpisodeStatus.Cancelled:
                    return InpEpisodeOperationResult.Conflict(
                        "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan.",
                        episode);
            }

            // GUARD-INP-02.
            if (!await _episodeService.IsActiveDoctorAsync(episode.Id, actorDoctorId, cancellationToken))
            {
                return InpEpisodeOperationResult.Forbidden(
                    "Hanya DPJP episode ini yang dapat menyatakan pasien boleh pulang.");
            }

            var now = DateTime.UtcNow;
            var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                episode.DischargeType = (InpDischargeType)request.DischargeType;
                episode.DischargeDecidedAt = now;

                await _episodeService.ApplyStatusChangeAsync(
                    episode,
                    fromStatus: InpEpisodeStatus.Admitted,
                    toStatus: InpEpisodeStatus.DischargePending,
                    actionType: ActionDecideDischarge,
                    actorType: InpStatusChangeActorType.User,
                    changedByUserId: actorUserId,
                    reason: reason,
                    now: now,
                    touchEpisode: true,
                    cancellationToken: cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpEpisodeOperationResult.Success(
                    episode,
                    "Pasien dinyatakan boleh pulang. Tempat tidur belum dilepas.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        // =====================================================================
        // BE-RWI-021 dan BE-RWI-022 — Resume pulang, tanda tangan, dan versinya
        // =====================================================================

        /// <summary>
        /// Membaca resume pulang satu episode, dengan pilihan menyertakan seluruh versi
        /// sebelumnya.
        /// </summary>
        public async Task<DischargeSummaryResponse?> GetSummaryAsync(
            Guid episodeId,
            bool includeRevisions = false,
            CancellationToken cancellationToken = default)
        {
            var summary = await _dbContext.Set<InpDischargeSummary>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .Select(x => new DischargeSummaryResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                    PrimaryDiagnosisText = x.PrimaryDiagnosisText,
                    SecondaryDiagnosisText = x.SecondaryDiagnosisText,
                    ProcedureSummary = x.ProcedureSummary,
                    DischargeMedicationNote = x.DischargeMedicationNote,
                    FollowUpInstruction = x.FollowUpInstruction,
                    ReferralDestination = x.ReferralDestination,
                    ClinicalSummary = x.ClinicalSummary,
                    SignedAt = x.SignedAt,
                    SignedByDoctorId = x.SignedByDoctorId,
                    SignedByDoctorName = x.SignedByDoctor != null ? x.SignedByDoctor.FullName : null,
                    IsSigned = x.SignedAt != null,
                    DischargeType = x.Episode != null ? (int)x.Episode.DischargeType : 0,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (summary == null)
            {
                return null;
            }

            summary.DischargeTypeName = ((InpDischargeType)summary.DischargeType).ToString();

            if (!includeRevisions)
            {
                return summary;
            }

            summary.Revisions = await _dbContext.Set<InpDischargeSummaryRevision>()
                .AsNoTracking()
                .Where(x => x.DischargeSummaryId == summary.Id && !x.IsDelete)
                .OrderBy(x => x.RevisionNumber)
                .Select(x => new DischargeSummaryRevisionResponse
                {
                    Id = x.Id,
                    DischargeSummaryId = x.DischargeSummaryId,
                    RevisionNumber = x.RevisionNumber,
                    CorrectionSessionId = x.CorrectionSessionId,
                    PrimaryDiagnosisText = x.PrimaryDiagnosisText,
                    SecondaryDiagnosisText = x.SecondaryDiagnosisText,
                    ProcedureSummary = x.ProcedureSummary,
                    DischargeMedicationNote = x.DischargeMedicationNote,
                    FollowUpInstruction = x.FollowUpInstruction,
                    ReferralDestination = x.ReferralDestination,
                    ClinicalSummary = x.ClinicalSummary,
                    PreviousDischargeType = (int)x.PreviousDischargeType,
                    PreviousSignedAt = x.PreviousSignedAt,
                    PreviousSignedByDoctorId = x.PreviousSignedByDoctorId,
                    PreviousSignedByDoctorName = x.PreviousSignedByDoctor != null
                        ? x.PreviousSignedByDoctor.FullName
                        : null,
                    SupersededAt = x.SupersededAt,
                    SupersededByUserId = x.SupersededByUserId
                })
                .ToListAsync(cancellationToken);

            foreach (var revision in summary.Revisions)
            {
                revision.PreviousDischargeTypeName =
                    ((InpDischargeType)revision.PreviousDischargeType).ToString();
            }

            return summary;
        }

        /// <summary>
        /// Menyusun resume pulang bila belum ada, atau memperbaruinya bila sudah ada.
        /// </summary>
        /// <param name="actorIsSupervisor">
        /// Benar bila pelakunya supervisor. Hanya supervisor yang dapat mengubah resume yang
        /// sudah ditandatangani, dan itu pun hanya di dalam sesi koreksi yang terbuka.
        /// </param>
        /// <remarks>
        /// <b>Dua jalur yang sangat berbeda, dan membedakannya adalah inti task ini.</b>
        ///
        /// <list type="number">
        /// <item><description>
        /// Resume <b>belum</b> ditandatangani: isinya ditimpa biasa. <b>Tidak</b> ada versi
        /// yang disimpan. Menyimpan versi untuk setiap penyuntingan akan membanjiri tabel
        /// versi dengan draf setengah jadi, dan riwayat amandemen kehilangan artinya —
        /// justru kebalikan dari yang diminta <c>RWI-DEC-057</c>.
        /// </description></item>
        /// <item><description>
        /// Resume <b>sudah</b> ditandatangani: ini amandemen rekam medis, bukan penyuntingan.
        /// Ia hanya diterima bila ada sesi koreksi yang terbuka, dan salinan versi
        /// sebelumnya — beserta nama penandatangan lamanya — disimpan lebih dulu di dalam
        /// transaksi yang sama.
        /// </description></item>
        /// </list>
        ///
        /// <para>
        /// <b>Delta terhadap state-transition-matrix bagian 5.</b> Matriks itu memuat satu
        /// baris yang mengizinkan DPJP aktif mengubah resume yang sudah ditandatangani selama
        /// episode belum <c>Closed</c>, dengan tanda tangan diperbarui. Roadmap
        /// <c>BE-RWI-021</c> acceptance criteria 3 justru menuntut sebaliknya: resume yang
        /// sudah ditandatangani <b>tidak</b> dapat diubah lewat endpoint biasa. Implementasi
        /// ini mengikuti roadmap, dan delta-nya dicatat pada laporan task untuk diputuskan
        /// pemilik kontrak.
        /// </para>
        /// </remarks>
        public async Task<InpDischargeSummaryOperationResult> UpsertSummaryAsync(
            Guid episodeId,
            UpsertDischargeSummaryRequest request,
            Guid actorUserId,
            Guid? actorDoctorId,
            bool actorIsSupervisor,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return InpDischargeSummaryOperationResult.Invalid("Isian resume belum dikirim.");
            }

            if (string.IsNullOrWhiteSpace(request.PrimaryDiagnosisText))
            {
                return InpDischargeSummaryOperationResult.Invalid("Diagnosis utama wajib diisi.");
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpDischargeSummaryOperationResult.NotFound(
                    "Episode rawat inap tidak ditemukan.");
            }

            var existing = await _dbContext.Set<InpDischargeSummary>()
                .FirstOrDefaultAsync(x => x.EpisodeId == episodeId && !x.IsDelete, cancellationToken);

            var openCorrectionSession = await GetOpenCorrectionSessionAsync(
                episodeId,
                cancellationToken);

            var isAmendment = existing != null && existing.SignedAt.HasValue;

            if (isAmendment)
            {
                if (openCorrectionSession == null)
                {
                    return InpDischargeSummaryOperationResult.Conflict(
                        "Resume ini sudah ditandatangani, sehingga hanya dapat diubah lewat " +
                        "sesi koreksi yang dibuka supervisor.");
                }

                if (!actorIsSupervisor)
                {
                    return InpDischargeSummaryOperationResult.Forbidden(
                        "Hanya supervisor yang dapat mengubah resume yang sudah ditandatangani.");
                }
            }
            else
            {
                if (episode.EpisodeStatus != InpEpisodeStatus.DischargePending)
                {
                    return InpDischargeSummaryOperationResult.BusinessRuleRejected(
                        "Resume pulang hanya dapat disusun setelah DPJP menyatakan pasien " +
                        "boleh pulang.");
                }

                // GUARD-INP-03 berlaku sejak penyusunan, bukan hanya saat penandatanganan.
                // Resume adalah catatan resmi milik DPJP; membiarkan orang lain menyusunnya
                // lalu meminta DPJP menandatangani apa adanya adalah tanda tangan yang
                // kehilangan artinya.
                if (!await _episodeService.IsActiveDoctorAsync(episode.Id, actorDoctorId, cancellationToken))
                {
                    return InpDischargeSummaryOperationResult.Forbidden(
                        "Hanya DPJP episode ini yang dapat menyusun resume pulang.");
                }
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                InpDischargeSummary summary;

                if (existing == null)
                {
                    summary = new InpDischargeSummary
                    {
                        Id = Guid.NewGuid(),
                        EpisodeId = episodeId,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    };

                    _dbContext.Set<InpDischargeSummary>().Add(summary);
                }
                else
                {
                    summary = existing;

                    if (isAmendment)
                    {
                        await AddRevisionSnapshotAsync(
                            summary,
                            openCorrectionSession!.Id,
                            actorUserId,
                            episode.DischargeType,
                            now,
                            cancellationToken);
                    }

                    summary.UpdateDateTime = now;
                    summary.UpdateBy = actorUserId;
                }

                ApplySummaryContent(summary, request);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpDischargeSummaryOperationResult.Success(
                    summary.Id,
                    isAmendment
                        ? "Resume berhasil diubah. Versi sebelumnya tersimpan."
                        : "Resume pulang berhasil disimpan.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                // INV-INP-05 — satu episode paling banyak satu resume. Unique index pada
                // InpDischargeSummary.EpisodeId adalah penjaga terakhirnya.
                return InpDischargeSummaryOperationResult.Conflict(
                    "Episode ini sudah punya resume pulang.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// DPJP aktif menandatangani resume pulang.
        /// </summary>
        /// <remarks>
        /// <b><c>GUARD-INP-03</c>.</b> Tanda tangan hanya berarti bila ia benar-benar berasal
        /// dari penanggung jawab pelayanan. Dokter mana pun yang bukan DPJP aktif ditolak 403,
        /// dan penandatangan tidak pernah dibaca dari isian permintaan.
        ///
        /// <para>
        /// Dua isian wajib diperiksa di sini, bukan saat penyusunan: diagnosis utama, dan
        /// tujuan rujukan bila cara pulangnya rujukan. Keduanya boleh kosong selama resume
        /// masih disusun; yang tidak boleh adalah resume tertandatangani yang kosong.
        /// </para>
        /// </remarks>
        public async Task<InpDischargeSummaryOperationResult> SignSummaryAsync(
            Guid episodeId,
            SignDischargeSummaryRequest? request,
            Guid actorUserId,
            Guid? actorDoctorId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpDischargeSummaryOperationResult.NotFound(
                    "Episode rawat inap tidak ditemukan.");
            }

            var summary = await _dbContext.Set<InpDischargeSummary>()
                .FirstOrDefaultAsync(x => x.EpisodeId == episodeId && !x.IsDelete, cancellationToken);

            if (summary == null)
            {
                return InpDischargeSummaryOperationResult.NotFound(
                    "Resume pulang belum disusun.");
            }

            // GUARD-INP-03.
            if (!await _episodeService.IsActiveDoctorAsync(episode.Id, actorDoctorId, cancellationToken))
            {
                return InpDischargeSummaryOperationResult.Forbidden(
                    "Hanya DPJP episode ini yang dapat menandatangani resume.");
            }

            if (string.IsNullOrWhiteSpace(summary.PrimaryDiagnosisText))
            {
                return InpDischargeSummaryOperationResult.Invalid(
                    "Diagnosis utama wajib diisi sebelum resume ditandatangani.");
            }

            if (episode.DischargeType == InpDischargeType.Referred &&
                string.IsNullOrWhiteSpace(summary.ReferralDestination))
            {
                return InpDischargeSummaryOperationResult.Invalid(
                    "Tujuan rujukan wajib diisi untuk pasien yang dirujuk.");
            }

            var now = DateTime.UtcNow;

            summary.SignedAt = now;
            summary.SignedByDoctorId = actorDoctorId;
            summary.UpdateDateTime = now;
            summary.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return InpDischargeSummaryOperationResult.Success(
                summary.Id,
                "Resume pulang berhasil ditandatangani.");
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Menyimpan salinan isi resume yang sedang berlaku sebagai satu versi, sebelum isinya
        /// digantikan.
        /// </summary>
        /// <remarks>
        /// Salinan memuat seluruh isi lama, cara pulang lama, nama penandatangan lama beserta
        /// waktunya, siapa yang menggantikan, dan sesi koreksi yang menyebabkannya. Barisnya
        /// <b>tidak dapat diubah maupun dihapus</b>: tidak ada endpoint <c>PUT</c> maupun
        /// <c>DELETE</c> yang menunjuknya, dan ketiadaan itu disengaja — api contract bagian 8.
        /// </remarks>
        private async Task AddRevisionSnapshotAsync(
            InpDischargeSummary summary,
            Guid correctionSessionId,
            Guid actorUserId,
            InpDischargeType previousDischargeType,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var lastRevisionNumber = await _dbContext.Set<InpDischargeSummaryRevision>()
                .Where(x => x.DischargeSummaryId == summary.Id)
                .Select(x => (int?)x.RevisionNumber)
                .MaxAsync(cancellationToken) ?? 0;

            var revision = new InpDischargeSummaryRevision
            {
                Id = Guid.NewGuid(),
                DischargeSummaryId = summary.Id,
                RevisionNumber = lastRevisionNumber + 1,
                CorrectionSessionId = correctionSessionId,
                PrimaryDiagnosisText = summary.PrimaryDiagnosisText,
                SecondaryDiagnosisText = summary.SecondaryDiagnosisText,
                ProcedureSummary = summary.ProcedureSummary,
                DischargeMedicationNote = summary.DischargeMedicationNote,
                FollowUpInstruction = summary.FollowUpInstruction,
                ReferralDestination = summary.ReferralDestination,
                ClinicalSummary = summary.ClinicalSummary,
                PreviousDischargeType = previousDischargeType,
                PreviousSignedAt = summary.SignedAt ?? now,
                PreviousSignedByDoctorId = summary.SignedByDoctorId ?? Guid.Empty,
                SupersededAt = now,
                SupersededByUserId = actorUserId,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<InpDischargeSummaryRevision>().Add(revision);
        }

        /// <summary>
        /// Membaca sesi koreksi yang masih terbuka pada satu episode.
        /// </summary>
        /// <remarks>
        /// <b>Endpoint pembuka dan penutup sesi koreksi belum ada.</b> Keduanya milik
        /// <c>BE-RWI-030</c>. Yang dibutuhkan task ini hanyalah <b>pembacaan</b> keberadaan
        /// sesi terbuka, dan itu dapat dilakukan sekarang karena tabelnya sudah dibuat
        /// <c>BE-RWI-003</c>. Sampai <c>BE-RWI-030</c> rilis, satu-satunya cara sesi koreksi
        /// lahir adalah lewat penyisipan baris langsung — dan itulah yang dipakai test.
        /// </remarks>
        private Task<InpCorrectionSession?> GetOpenCorrectionSessionAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            return _dbContext.Set<InpCorrectionSession>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && x.ClosedAt == null && !x.IsDelete)
                .OrderByDescending(x => x.SequenceNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static void ApplySummaryContent(
            InpDischargeSummary summary,
            UpsertDischargeSummaryRequest request)
        {
            summary.PrimaryDiagnosisText = request.PrimaryDiagnosisText.Trim();
            summary.SecondaryDiagnosisText = NormalizeText(request.SecondaryDiagnosisText);
            summary.ProcedureSummary = NormalizeText(request.ProcedureSummary);
            summary.DischargeMedicationNote = NormalizeText(request.DischargeMedicationNote);
            summary.FollowUpInstruction = NormalizeText(request.FollowUpInstruction);
            summary.ReferralDestination = NormalizeText(request.ReferralDestination);
            summary.ClinicalSummary = NormalizeText(request.ClinicalSummary);
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    /// <summary>Hasil satu tindakan pada resume pulang.</summary>
    public sealed class InpDischargeSummaryOperationResult
    {
        private InpDischargeSummaryOperationResult(
            InpEpisodeOperationStatus status,
            string message,
            Guid? summaryId = null)
        {
            Status = status;
            Message = message;
            SummaryId = summaryId;
        }

        public InpEpisodeOperationStatus Status { get; }

        public string Message { get; }

        public Guid? SummaryId { get; }

        public static InpDischargeSummaryOperationResult Success(Guid summaryId, string message)
            => new(InpEpisodeOperationStatus.Success, message, summaryId);

        public static InpDischargeSummaryOperationResult Invalid(string message)
            => new(InpEpisodeOperationStatus.Invalid, message);

        public static InpDischargeSummaryOperationResult NotFound(string message)
            => new(InpEpisodeOperationStatus.NotFound, message);

        public static InpDischargeSummaryOperationResult Conflict(string message)
            => new(InpEpisodeOperationStatus.Conflict, message);

        public static InpDischargeSummaryOperationResult BusinessRuleRejected(string message)
            => new(InpEpisodeOperationStatus.BusinessRuleRejected, message);

        public static InpDischargeSummaryOperationResult Forbidden(string message)
            => new(InpEpisodeOperationStatus.Forbidden, message);
    }
}
