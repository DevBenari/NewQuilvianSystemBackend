using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Bagian sesi koreksi dan hubungan episode bayi dengan episode ibunya. Diisi task
    /// <c>BE-RWI-030</c> dan <c>BE-RWI-031</c>.
    /// </summary>
    /// <remarks>
    /// <b>Sesi koreksi bukan status episode keenam.</b> Godaan menjadikan "sedang dikoreksi"
    /// sebagai status akan muncul karena ia menyederhanakan layar. `blueprint-manifest.md`
    /// bagian 8 butir 5 menguncinya sebagai konsep tersendiri: menambah status melanggar
    /// <c>RWI-DEC-009</c> yang mengunci lima nilai, dan <c>RWI-AC-004</c> yang menghitungnya.
    ///
    /// <para>
    /// Selama sesi terbuka, status episode tetap <c>Closed</c>. Tempat tidur tidak
    /// dikembalikan, pasien tidak muncul pada census, dan lama dirawat tidak bertambah — sebab
    /// ketiganya dihitung dari status dan dari baris penempatan, bukan dari keberadaan sesi.
    /// </para>
    /// </remarks>
    public partial class InpEpisodeService
    {
        // =====================================================================
        // BE-RWI-030 — Sesi koreksi
        // =====================================================================

        /// <summary>
        /// Supervisor membuka sesi koreksi pada episode yang sudah ditutup.
        /// </summary>
        /// <param name="actorIsSupervisor">
        /// Benar bila pelakunya supervisor. Kepala ruangan dan DPJP sama-sama ditolak 403 —
        /// <c>RWI-RULE-020</c>.
        /// </param>
        public async Task<InpCorrectionSessionOperationResult> OpenCorrectionSessionAsync(
            Guid episodeId,
            OpenCorrectionSessionRequest request,
            Guid actorUserId,
            bool actorIsSupervisor,
            CancellationToken cancellationToken = default)
        {
            if (request == null || !HasMeaningfulReason(request.OpenReason))
            {
                return InpCorrectionSessionOperationResult.Invalid(
                    "Alasan membuka kembali episode wajib diisi.");
            }

            if (!actorIsSupervisor)
            {
                return InpCorrectionSessionOperationResult.Forbidden(
                    "Hanya supervisor yang dapat membuka kembali episode.");
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpCorrectionSessionOperationResult.NotFound(
                    "Episode rawat inap tidak ditemukan.");
            }

            if (episode.EpisodeStatus != InpEpisodeStatus.Closed)
            {
                return InpCorrectionSessionOperationResult.BusinessRuleRejected(
                    "Sesi koreksi hanya untuk episode yang sudah ditutup.");
            }

            var hasOpenSession = await _dbContext.Set<InpCorrectionSession>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.EpisodeId == episodeId && x.ClosedAt == null && !x.IsDelete,
                    cancellationToken);

            if (hasOpenSession)
            {
                return InpCorrectionSessionOperationResult.Conflict(
                    "Episode ini sedang dalam sesi koreksi yang belum ditutup.");
            }

            var now = DateTime.UtcNow;

            var lastSequence = await _dbContext.Set<InpCorrectionSession>()
                .Where(x => x.EpisodeId == episodeId)
                .Select(x => (int?)x.SequenceNumber)
                .MaxAsync(cancellationToken) ?? 0;

            var session = new InpCorrectionSession
            {
                Id = Guid.NewGuid(),
                EpisodeId = episodeId,
                SequenceNumber = lastSequence + 1,
                OpenedAt = now,
                OpenedByUserId = actorUserId,
                OpenReason = request.OpenReason.Trim(),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<InpCorrectionSession>().Add(session);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Unique index parsial IX_InpCorrectionSession_EpisodeId_Open menolak sesi
                // kedua bila dua supervisor membukanya pada saat hampir bersamaan.
                return InpCorrectionSessionOperationResult.Conflict(
                    "Episode ini sedang dalam sesi koreksi yang belum ditutup.");
            }

            return InpCorrectionSessionOperationResult.Success(
                session.Id,
                "Sesi koreksi berhasil dibuka. Status episode tetap tertutup.");
        }

        /// <summary>
        /// Menutup sesi koreksi beserta daftar perubahan yang dikerjakan di dalamnya.
        /// </summary>
        public async Task<InpCorrectionSessionOperationResult> CloseCorrectionSessionAsync(
            Guid episodeId,
            Guid sessionId,
            CloseCorrectionSessionRequest request,
            Guid actorUserId,
            bool actorIsSupervisor,
            CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ChangedFieldSummary))
            {
                return InpCorrectionSessionOperationResult.Invalid(
                    "Tuliskan apa saja yang diubah sebelum menutup sesi koreksi.");
            }

            if (!actorIsSupervisor)
            {
                return InpCorrectionSessionOperationResult.Forbidden(
                    "Hanya supervisor yang dapat menutup sesi koreksi.");
            }

            var session = await _dbContext.Set<InpCorrectionSession>()
                .FirstOrDefaultAsync(
                    x => x.Id == sessionId && x.EpisodeId == episodeId && !x.IsDelete,
                    cancellationToken);

            if (session == null)
            {
                return InpCorrectionSessionOperationResult.NotFound(
                    "Sesi koreksi tidak ditemukan.");
            }

            if (session.ClosedAt.HasValue)
            {
                return InpCorrectionSessionOperationResult.Conflict(
                    "Sesi koreksi ini sudah ditutup.");
            }

            var now = DateTime.UtcNow;

            session.ClosedAt = now;
            session.ClosedByUserId = actorUserId;
            session.ChangedFieldSummary = request.ChangedFieldSummary.Trim();
            session.IsActive = false;
            session.UpdateDateTime = now;
            session.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return InpCorrectionSessionOperationResult.Success(
                session.Id,
                "Sesi koreksi berhasil ditutup.");
        }

        /// <summary>Membaca seluruh sesi koreksi satu episode, urut nomor urut.</summary>
        public async Task<List<InpatientCorrectionSessionResponse>> GetCorrectionSessionsAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<InpCorrectionSession>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.SequenceNumber)
                .Select(x => new InpatientCorrectionSessionResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                    SequenceNumber = x.SequenceNumber,
                    OpenedAt = x.OpenedAt,
                    OpenedByUserId = x.OpenedByUserId,
                    OpenReason = x.OpenReason,
                    ClosedAt = x.ClosedAt,
                    ClosedByUserId = x.ClosedByUserId,
                    ChangedFieldSummary = x.ChangedFieldSummary,
                    IsOpen = x.ClosedAt == null
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>Membaca satu sesi koreksi sebagai balasan endpoint.</summary>
        public async Task<InpatientCorrectionSessionResponse?> GetCorrectionSessionAsync(
            Guid episodeId,
            Guid sessionId,
            CancellationToken cancellationToken = default)
        {
            var sessions = await GetCorrectionSessionsAsync(episodeId, cancellationToken);

            return sessions.FirstOrDefault(x => x.Id == sessionId);
        }

        // =====================================================================
        // BE-RWI-031 — Hubungan episode bayi dan ibu
        // =====================================================================

        /// <summary>
        /// Memeriksa rujukan episode ibu. Mengembalikan <c>null</c> bila lolos, atau bila
        /// memang tidak diisi.
        /// </summary>
        /// <remarks>
        /// Empat aturan pada validation matrix bagian 5B:
        ///
        /// <list type="number">
        /// <item><description>boleh kosong — sebagian besar episode memang bukan bayi rawat gabung;</description></item>
        /// <item><description>tidak boleh menunjuk dirinya sendiri;</description></item>
        /// <item><description>tidak boleh milik pasien yang sama;</description></item>
        /// <item><description>episode ibu harus ada dan belum <c>Closed</c> maupun <c>Cancelled</c>.</description></item>
        /// </list>
        ///
        /// <para>
        /// <b>Aturan ketiga adalah yang paling mudah terlewat.</b> Tanpa ia, seorang pasien
        /// dapat tercatat sebagai ibu dari dirinya sendiri lewat dua episode berbeda — dan
        /// pertanyaan "bayi siapa yang ada di boks kamar ini" mulai menjawab hal yang mustahil.
        /// </para>
        /// </remarks>
        public async Task<InpEpisodeOperationResult?> ValidateMotherEpisodeAsync(
            Guid? motherEpisodeId,
            Guid childEpisodeId,
            Guid childPatientId,
            CancellationToken cancellationToken = default)
        {
            if (!motherEpisodeId.HasValue || motherEpisodeId.Value == Guid.Empty)
            {
                return null;
            }

            if (motherEpisodeId.Value == childEpisodeId)
            {
                return InpEpisodeOperationResult.Invalid(
                    "Episode tidak dapat menunjuk dirinya sendiri sebagai episode ibu.");
            }

            var mother = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == motherEpisodeId.Value && !x.IsDelete)
                .Select(x => new { x.Id, x.PatientId, x.EpisodeStatus })
                .FirstOrDefaultAsync(cancellationToken);

            if (mother == null ||
                mother.EpisodeStatus == InpEpisodeStatus.Closed ||
                mother.EpisodeStatus == InpEpisodeStatus.Cancelled)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Episode ibu tidak ditemukan atau sudah selesai.");
            }

            if (mother.PatientId == childPatientId)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Episode ibu harus milik pasien yang berbeda.");
            }

            return null;
        }

        /// <summary>
        /// Menjawab bayi siapa yang berada di boks bayi kamar tertentu.
        /// </summary>
        /// <remarks>
        /// Dibaca dari penempatan yang masih aktif pada tempat tidur bertanda
        /// <c>IsForNewborn</c>. Rujukan ke episode ibu ikut dikembalikan bila memang terisi —
        /// itulah yang membuat pertanyaan "bayi siapa" dapat dijawab, bukan sekadar "bayi
        /// mana".
        /// </remarks>
        public async Task<List<CensusItemResponse>> GetNewbornOccupantsAsync(
            Guid roomId,
            CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x =>
                    x.RoomId == roomId &&
                    x.EndDateTime == null &&
                    !x.IsDelete &&
                    x.Bed != null &&
                    x.Bed.IsForNewborn &&
                    x.Episode != null &&
                    !x.Episode.IsDelete)
                .OrderBy(x => x.StartDateTime)
                .Select(x => new CensusItemResponse
                {
                    EpisodeId = x.EpisodeId,
                    EpisodeNumber = x.Episode!.EpisodeNumber,
                    PatientId = x.Episode.PatientId,
                    PatientName = x.Episode.Patient != null ? x.Episode.Patient.FullName : null,
                    MedicalRecordNumber = x.Episode.Patient != null
                        ? x.Episode.Patient.MedicalRecordNumber
                        : null,
                    EpisodeStatus = (int)x.Episode.EpisodeStatus,
                    BedId = x.BedId,
                    BedCode = x.Bed!.BedCode,
                    BedName = x.Bed.BedName,
                    RoomId = x.RoomId,
                    RoomName = x.Room != null ? x.Room.RoomName : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    AdmittedAt = x.Episode.AdmittedAt,
                    PlacementStartDateTime = x.StartDateTime,
                    MotherEpisodeId = x.Episode.MotherEpisodeId,
                    MotherEpisodeNumber = x.Episode.MotherEpisode != null
                        ? x.Episode.MotherEpisode.EpisodeNumber
                        : null,
                    MotherPatientName =
                        x.Episode.MotherEpisode != null && x.Episode.MotherEpisode.Patient != null
                            ? x.Episode.MotherEpisode.Patient.FullName
                            : null
                })
                .ToListAsync(cancellationToken);

            var today = DateTime.UtcNow;

            foreach (var item in items)
            {
                item.EpisodeStatusName = ((InpEpisodeStatus)item.EpisodeStatus).ToString();
                item.LengthOfStayDays = InpCensusQueryService.CalculateLengthOfStayDays(
                    item.AdmittedAt ?? item.PlacementStartDateTime,
                    today);
            }

            return items;
        }
    }

    /// <summary>Hasil satu tindakan pada sesi koreksi.</summary>
    public sealed class InpCorrectionSessionOperationResult
    {
        private InpCorrectionSessionOperationResult(
            InpEpisodeOperationStatus status,
            string message,
            Guid? sessionId = null)
        {
            Status = status;
            Message = message;
            SessionId = sessionId;
        }

        public InpEpisodeOperationStatus Status { get; }

        public string Message { get; }

        public Guid? SessionId { get; }

        public static InpCorrectionSessionOperationResult Success(Guid sessionId, string message)
            => new(InpEpisodeOperationStatus.Success, message, sessionId);

        public static InpCorrectionSessionOperationResult Invalid(string message)
            => new(InpEpisodeOperationStatus.Invalid, message);

        public static InpCorrectionSessionOperationResult NotFound(string message)
            => new(InpEpisodeOperationStatus.NotFound, message);

        public static InpCorrectionSessionOperationResult Conflict(string message)
            => new(InpEpisodeOperationStatus.Conflict, message);

        public static InpCorrectionSessionOperationResult BusinessRuleRejected(string message)
            => new(InpEpisodeOperationStatus.BusinessRuleRejected, message);

        public static InpCorrectionSessionOperationResult Forbidden(string message)
            => new(InpEpisodeOperationStatus.Forbidden, message);
    }
}
