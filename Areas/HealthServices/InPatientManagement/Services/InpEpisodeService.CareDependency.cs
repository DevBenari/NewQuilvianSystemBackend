using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Pencatatan High/Low Dependency per episode beserta riwayatnya (<c>BE-RWI-234</c>,
    /// RWI-DEC-302, GUARD-DNH-02, VAL-DNH-14–17).
    /// </summary>
    /// <remarks>
    /// <b>Sistem hanya mencatat.</b> Kriteria klinis HD/LD milik SOP keperawatan (RWI-OQ-141), dan
    /// nilainya tidak dibaca kelayakan bed, kategori transfer, maupun tarif (INV-DNH-05).
    ///
    /// <para>
    /// <b>Siapa yang boleh mencatat</b> diputuskan dari data episode, bukan dari nama peran:
    /// selagi <c>Draft</c> pemegang hak <c>SetCareDependency</c> mencatat catatan awal
    /// (AdmissionNote); setelah pasien dirawat hanya perawat penanggung jawab aktif
    /// (NurseAssessment) atau DPJP aktif (DoctorDecision) episode itu. SuperAdmin tidak melewati
    /// penjaga ini.
    /// </para>
    /// </remarks>
    public partial class InpEpisodeService
    {
        /// <summary>
        /// Mencatat atau mengubah High/Low Dependency. Nilai yang sama dengan nilai saat ini tetap
        /// dicatat sebagai penilaian ulang (satu baris riwayat baru).
        /// </summary>
        /// <example>
        /// Ny. Sari diadmisi dengan catatan awal Low dari petugas admisi; hari kedua perawat
        /// penanggung jawab mencatat High; hari keempat DPJP mencatat Low. Riwayat memuat tiga
        /// baris, dan episode bernilai Low.
        /// </example>
        public async Task<InpEpisodeOperationResult> SetCareDependencyAsync(
            Guid episodeId,
            SetCareDependencyRequest request,
            Guid actorUserId,
            Guid? actorEmployeeId,
            Guid? actorDoctorId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.Invalid,
                    "Isian tingkat ketergantungan belum dikirim.", "INVALID_INPUT");
            }

            // VAL-DNH-14.
            if (request.Level is not ((int)InpCareDependencyLevel.High or (int)InpCareDependencyLevel.Low))
            {
                return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.Invalid,
                    "Pilih High Dependency atau Low Dependency.", "CARE_DEPENDENCY_LEVEL_INVALID");
            }

            var note = NormalizeText(request.Note);
            if (note != null && note.Length > 500)
            {
                return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.Invalid,
                    "Catatan tingkat ketergantungan maksimum 500 karakter.", "INVALID_INPUT");
            }

            var episode = await LoadEpisodeForWriteAsync(episodeId, cancellationToken);
            if (episode == null)
            {
                return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.NotFound,
                    "Episode rawat inap tidak ditemukan.", "NOT_FOUND");
            }

            // Draft yang lewat batas digugurkan lebih dulu, di luar transaksi pencatatan ini.
            if (await ExpireDraftIfDueAsync(episode, cancellationToken))
            {
                return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.BusinessRuleRejected,
                    "Admisi ini sudah gugur karena ditinggalkan melewati batas waktu, sehingga hanya dapat dibaca.",
                    "EPISODE_READ_ONLY");
            }

            await using var transaction = await InpBedTransactionLease.BeginAsync(_dbContext, cancellationToken);

            // Kunci baris episode supaya dua pencatatan bersamaan berurutan; yang kedua membaca
            // nilai terbaru dan ditolak 409 bila ExpectedSetAt-nya sudah basi (DNH-AT-908).
            if (_dbContext.Database.IsRelational())
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT 1 FROM public.\"InpEpisode\" WHERE \"Id\" = {0} FOR UPDATE",
                    new object[] { episode.Id },
                    cancellationToken);
                await _dbContext.Entry(episode).ReloadAsync(cancellationToken);
            }

            if (episode.IsDelete)
            {
                return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.NotFound,
                    "Episode rawat inap tidak ditemukan.", "NOT_FOUND");
            }

            // VAL-DNH-15 / transisi 12.5: episode Closed dan Cancelled hanya dapat dibaca.
            if (episode.EpisodeStatus is InpEpisodeStatus.Closed or InpEpisodeStatus.Cancelled)
            {
                return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.BusinessRuleRejected,
                    "Episode sudah ditutup atau dibatalkan, sehingga tingkat ketergantungan hanya dapat dibaca.",
                    "EPISODE_READ_ONLY");
            }

            // GUARD-DNH-02.
            InpCareDependencySource source;
            if (episode.EpisodeStatus == InpEpisodeStatus.Draft)
            {
                source = InpCareDependencySource.AdmissionNote;
            }
            else if (actorEmployeeId.HasValue && actorEmployeeId.Value != Guid.Empty &&
                     await _dbContext.Set<InpNurseAssignment>().AnyAsync(x =>
                         x.EpisodeId == episode.Id &&
                         x.EmployeeId == actorEmployeeId.Value &&
                         x.EndDateTime == null &&
                         !x.IsDelete, cancellationToken))
            {
                source = InpCareDependencySource.NurseAssessment;
            }
            else if (await IsActiveDoctorAsync(episode.Id, actorDoctorId, cancellationToken))
            {
                source = InpCareDependencySource.DoctorDecision;
            }
            else
            {
                return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.Forbidden,
                    "Setelah pasien dirawat, tingkat ketergantungan hanya dapat dicatat perawat penanggung jawab aktif atau DPJP aktif episode ini.",
                    "CARE_DEPENDENCY_ACTOR_NOT_ASSIGNED");
            }

            // VAL-DNH-16: nilai yang dilihat layar harus masih terbaru; keduanya kosong dianggap sama.
            if (!SameRecordedInstant(request.ExpectedSetAt, episode.CareDependencySetAt))
            {
                return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.Conflict,
                    "Tingkat ketergantungan sudah diubah petugas lain sejak layar dibuka. Muat ulang lalu ulangi bila masih perlu.",
                    "STALE_CARE_DEPENDENCY");
            }

            // Presisi mikrodetik sama dengan timestamptz, supaya nilai yang dibaca ulang layar persis
            // sama dengan yang tersimpan.
            var now = DateTime.UtcNow;
            now = new DateTime(now.Ticks - now.Ticks % 10, DateTimeKind.Utc);
            var level = (InpCareDependencyLevel)request.Level;

            _dbContext.Set<InpCareDependencyHistory>().Add(new InpCareDependencyHistory
            {
                EpisodeId = episode.Id,
                PreviousLevel = episode.CareDependencyLevel,
                NewLevel = level,
                Source = source,
                ActorUserId = actorUserId,
                RecordedAtUtc = now,
                Note = note,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            episode.CareDependencyLevel = level;
            episode.CareDependencySource = source;
            episode.CareDependencySetByUserId = actorUserId;
            episode.CareDependencySetAt = now;
            episode.CareDependencyNote = note;
            episode.UpdateDateTime = now;
            episode.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return InpEpisodeOperationResult.Success(episode,
                level == InpCareDependencyLevel.High
                    ? "High Dependency berhasil dicatat."
                    : "Low Dependency berhasil dicatat.");
        }

        /// <summary>
        /// Riwayat High/Low Dependency satu episode, terbaru dulu. <c>null</c> bila episode tidak ada.
        /// </summary>
        public async Task<PagedResult<CareDependencyHistoryResponse>?> GetCareDependencyHistoryAsync(
            Guid episodeId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.Set<InpEpisode>().AsNoTracking()
                .AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
            if (!exists) return null;

            var (page, size) = NormalizePaging(pageNumber, pageSize);
            var source = _dbContext.Set<InpCareDependencyHistory>().AsNoTracking()
                .Where(x => x.EpisodeId == episodeId);
            var total = await source.CountAsync(cancellationToken);
            var rows = await source
                .OrderByDescending(x => x.RecordedAtUtc)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new
                {
                    x.Id,
                    x.PreviousLevel,
                    x.NewLevel,
                    x.Source,
                    ActorName = x.ActorUser != null ? x.ActorUser.DisplayName ?? x.ActorUser.UserName : null,
                    x.RecordedAtUtc,
                    x.Note
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<CareDependencyHistoryResponse>
            {
                PageNumber = page,
                PageSize = size,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)size),
                Items = rows.Select(x => new CareDependencyHistoryResponse
                {
                    Id = x.Id,
                    PreviousLevel = x.PreviousLevel.HasValue ? (int)x.PreviousLevel.Value : null,
                    PreviousLevelName = x.PreviousLevel?.ToString(),
                    NewLevel = (int)x.NewLevel,
                    NewLevelName = x.NewLevel.ToString(),
                    Source = (int)x.Source,
                    SourceName = x.Source.ToString(),
                    ActorName = x.ActorName,
                    RecordedAtUtc = x.RecordedAtUtc,
                    Note = x.Note
                }).ToList()
            };
        }

        /// <summary>
        /// Membandingkan waktu pencatatan sampai presisi mikrodetik (presisi <c>timestamptz</c>).
        /// </summary>
        private static bool SameRecordedInstant(DateTime? expected, DateTime? current)
        {
            if (!expected.HasValue || !current.HasValue) return expected.HasValue == current.HasValue;
            var a = expected.Value.Kind == DateTimeKind.Local ? expected.Value.ToUniversalTime() : expected.Value;
            var b = current.Value.Kind == DateTimeKind.Local ? current.Value.ToUniversalTime() : current.Value;
            return Math.Abs((a - b).Ticks) < 10;
        }
    }
}
