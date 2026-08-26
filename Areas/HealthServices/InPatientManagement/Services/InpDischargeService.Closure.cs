using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Bagian daftar periksa administrasi, kelayakan keuangan, kesiapan penutupan, penutupan
    /// episode, jalan keluar supervisor, dan pencatatan kepergian fisik. Diisi task
    /// <c>BE-RWI-023</c> sampai <c>BE-RWI-027</c>.
    /// </summary>
    /// <remarks>
    /// <b>Kelima syarat penutupan berbentuk daftar, bukan boolean.</b> Petugas yang gagal
    /// menutup episode perlu tahu <b>syarat mana</b> yang belum terpenuhi, bukan bahwa
    /// tombolnya mati. Bentuk daftar ini dikunci api contract dan <c>RWI-RULE-010</c>.
    ///
    /// <para>
    /// <b>Jalan keluar supervisor menembus satu syarat saja.</b> Hanya kelayakan keuangan.
    /// Keempat syarat lainnya tetap menahan, dan tidak ada satu pun peran yang dapat
    /// melewatinya. Jalan keluar yang menembus semuanya sekaligus akan menjadi jalur normal
    /// dalam hitungan minggu, dan kelima syarat kehilangan artinya.
    /// </para>
    /// </remarks>
    public partial class InpDischargeService
    {
        /// <summary>Nilai <c>ActionType</c> untuk penutupan episode.</summary>
        public const string ActionCloseEpisode = "CloseEpisode";

        /// <summary>Nilai <c>ActionType</c> untuk penutupan menembus gerbang keuangan.</summary>
        public const string ActionCloseEpisodeWithOverride = "CloseEpisodeWithOverride";

        // =====================================================================
        // BE-RWI-023 — Daftar periksa administrasi
        // =====================================================================

        /// <summary>
        /// Menyusun daftar periksa administrasi satu episode beserta status penandaannya.
        /// </summary>
        /// <remarks>
        /// Daftar memuat seluruh butir yang <b>masih aktif</b>, ditambah butir yang sudah
        /// dinonaktifkan tetapi <b>pernah ditandai</b> pada episode ini. Butir yang
        /// dinonaktifkan tidak lagi menahan penutupan, tetapi penandaan lamanya tetap terbaca
        /// — <c>RWI-DEC-033</c>.
        ///
        /// <para>
        /// <b>Kenapa penandaan lama tidak boleh hilang.</b> Admin menonaktifkan butir
        /// "Surat keterangan dirawat" pada bulan Maret. Episode yang ditutup pada Februari
        /// sudah menandainya, dan penandaan itu adalah bukti bahwa surat tersebut memang
        /// diserahkan. Menghilangkannya dari layar membuat episode lama terlihat seolah tidak
        /// pernah lengkap.
        /// </para>
        /// </remarks>
        public async Task<ClearanceChecklistResponse?> GetClearanceChecklistAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.EpisodeNumber })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
            {
                return null;
            }

            var marks = await _dbContext.Set<InpClearanceMark>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .Select(x => new
                {
                    x.ClearanceItemId,
                    x.MarkedAt,
                    x.MarkedByUserId,
                    x.Note
                })
                .ToListAsync(cancellationToken);

            var markedItemIds = marks.Select(x => x.ClearanceItemId).ToList();

            var items = await _dbContext.Set<MstInpatientClearanceItem>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && (x.IsActive || markedItemIds.Contains(x.Id)))
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ItemName)
                .Select(x => new ClearanceChecklistItemResponse
                {
                    ItemId = x.Id,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    Description = x.Description,
                    IsMandatory = x.IsMandatory,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                var mark = marks.FirstOrDefault(x => x.ClearanceItemId == item.ItemId);

                item.IsMarked = mark != null;
                item.MarkedAt = mark?.MarkedAt;
                item.MarkedByUserId = mark?.MarkedByUserId;
                item.Note = mark?.Note;

                // Menahan hanya bila ketiganya benar: wajib, masih aktif, dan belum ditandai.
                item.IsBlocking = item.IsMandatory && item.IsActive && !item.IsMarked;
            }

            return new ClearanceChecklistResponse
            {
                EpisodeId = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                TotalItem = items.Count,
                TotalMarked = items.Count(x => x.IsMarked),
                TotalBlocking = items.Count(x => x.IsBlocking),
                Items = items
            };
        }

        /// <summary>
        /// Menandai satu butir daftar periksa administrasi. Penandaan menyimpan pelaku dan
        /// waktunya, dan hanya terjadi sekali per butir per episode.
        /// </summary>
        /// <remarks>
        /// <b>Butir obat pulang ditandai manual.</b> Modul Farmasi di luar scope revisi ini
        /// (<c>DEC-INP-001</c>), sehingga tidak ada penandaan otomatis yang menebak apakah obat
        /// pulang sudah diserahkan. Penandaan yang menebak lebih berbahaya daripada penandaan
        /// manual: ia terlihat seperti bukti padahal bukan.
        /// </remarks>
        public async Task<InpDischargeSummaryOperationResult> MarkClearanceItemAsync(
            Guid episodeId,
            Guid clearanceItemId,
            MarkClearanceItemRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpDischargeSummaryOperationResult.NotFound(
                    "Episode rawat inap tidak ditemukan.");
            }

            var closedGuard = await GuardEpisodeNotClosedAsync(episode, cancellationToken);

            if (closedGuard != null)
            {
                return closedGuard;
            }

            var item = await _dbContext.Set<MstInpatientClearanceItem>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == clearanceItemId && !x.IsDelete,
                    cancellationToken);

            if (item == null)
            {
                return InpDischargeSummaryOperationResult.NotFound(
                    "Butir administrasi tidak ditemukan.");
            }

            if (!item.IsActive)
            {
                return InpDischargeSummaryOperationResult.BusinessRuleRejected(
                    "Butir administrasi ini sudah tidak aktif dan tidak dapat ditandai lagi.");
            }

            var existing = await _dbContext.Set<InpClearanceMark>()
                .FirstOrDefaultAsync(
                    x => x.EpisodeId == episodeId && x.ClearanceItemId == clearanceItemId && !x.IsDelete,
                    cancellationToken);

            var now = DateTime.UtcNow;
            var note = NormalizeText(request?.Note);

            if (existing != null)
            {
                // Penandaan ulang memperbarui catatan dan pelakunya, bukan melahirkan baris
                // kedua. Unique index (EpisodeId, ClearanceItemId) adalah penjaga terakhirnya.
                existing.Note = note;
                existing.MarkedAt = now;
                existing.MarkedByUserId = actorUserId;
                existing.UpdateDateTime = now;
                existing.UpdateBy = actorUserId;
            }
            else
            {
                _dbContext.Set<InpClearanceMark>().Add(new InpClearanceMark
                {
                    Id = Guid.NewGuid(),
                    EpisodeId = episodeId,
                    ClearanceItemId = clearanceItemId,
                    MarkedAt = now,
                    MarkedByUserId = actorUserId,
                    Note = note,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return InpDischargeSummaryOperationResult.Success(
                episodeId,
                $"Butir {item.ItemName} berhasil ditandai.");
        }

        // =====================================================================
        // BE-RWI-024 — Kelayakan keuangan
        // =====================================================================

        /// <summary>Membaca kelayakan keuangan satu episode beserta riwayat penandaannya.</summary>
        public async Task<FinancialClearanceResponse?> GetFinancialClearanceAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.EpisodeNumber })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
            {
                return null;
            }

            var history = await _dbContext.Set<InpFinancialClearance>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.SequenceNumber)
                .Select(x => new FinancialClearanceEntryResponse
                {
                    Id = x.Id,
                    SequenceNumber = x.SequenceNumber,
                    ClearanceStatus = (int)x.ClearanceStatus,
                    MarkedAt = x.MarkedAt,
                    MarkedByUserId = x.MarkedByUserId,
                    Note = x.Note,
                    IsManualMarking = x.IsManualMarking
                })
                .ToListAsync(cancellationToken);

            foreach (var entry in history)
            {
                entry.ClearanceStatusName =
                    ((InpFinancialClearanceStatus)entry.ClearanceStatus).ToString();
            }

            var current = history.Count == 0
                ? InpFinancialClearanceStatus.Pending
                : (InpFinancialClearanceStatus)history[^1].ClearanceStatus;

            return new FinancialClearanceResponse
            {
                EpisodeId = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                CurrentStatus = (int)current,
                CurrentStatusName = current.ToString(),
                IsCleared = current == InpFinancialClearanceStatus.Cleared,
                History = history
            };
        }

        /// <summary>
        /// Kasir atau petugas billing menandai kelayakan keuangan episode.
        /// </summary>
        /// <param name="actorIsCashierOrBilling">
        /// Benar bila pelakunya kasir atau billing. Petugas admisi, perawat, dan dokter
        /// sama-sama ditolak 403 — <c>RWI-RULE-028</c>.
        /// </param>
        /// <remarks>
        /// <b><c>RWI-RISK-003</c> diterima secara sadar.</b> Penandaan ini <b>manual</b>.
        /// Nilainya bergantung pada disiplin petugas kasir, bukan pada angka tagihan yang
        /// sebenarnya, karena `BillingManagement` belum punya kemampuan transaksi. Setiap baris
        /// karena itu ditandai <c>IsManualMarking</c> dan wajib ditampilkan apa adanya pada
        /// layar dan laporan.
        ///
        /// <para>
        /// Ketika `BillingManagement` operasional, topik ini kembali sebagai Amendment Pass.
        /// Sampai saat itu, kelayakan keuangan yang bernilai <c>Cleared</c> berarti "seorang
        /// kasir menyatakan lunas", bukan "sistem menghitung tidak ada sisa tagihan".
        /// </para>
        ///
        /// <para>
        /// Riwayatnya bersifat menambah, tidak menimpa: nilai dapat berpindah bolak-balik
        /// antara ketiganya selama episode belum ditutup, dan setiap perpindahan tersimpan.
        /// </para>
        /// </remarks>
        public async Task<InpDischargeSummaryOperationResult> MarkFinancialClearanceAsync(
            Guid episodeId,
            MarkFinancialClearanceRequest request,
            Guid actorUserId,
            bool actorIsCashierOrBilling,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return InpDischargeSummaryOperationResult.Invalid(
                    "Isian kelayakan keuangan belum dikirim.");
            }

            if (!actorIsCashierOrBilling)
            {
                return InpDischargeSummaryOperationResult.Forbidden(
                    "Hanya petugas kasir atau billing yang dapat menandai kelayakan keuangan.");
            }

            if (!Enum.IsDefined(typeof(InpFinancialClearanceStatus), request.ClearanceStatus))
            {
                return InpDischargeSummaryOperationResult.BusinessRuleRejected(
                    "Nilai kelayakan keuangan tidak dikenali.");
            }

            if (string.IsNullOrWhiteSpace(request.Note))
            {
                return InpDischargeSummaryOperationResult.Invalid(
                    "Catatan wajib diisi saat menandai kelayakan keuangan.");
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpDischargeSummaryOperationResult.NotFound(
                    "Episode rawat inap tidak ditemukan.");
            }

            var closedGuard = await GuardEpisodeNotClosedAsync(episode, cancellationToken);

            if (closedGuard != null)
            {
                return closedGuard;
            }

            var now = DateTime.UtcNow;

            var lastSequence = await _dbContext.Set<InpFinancialClearance>()
                .Where(x => x.EpisodeId == episodeId)
                .Select(x => (int?)x.SequenceNumber)
                .MaxAsync(cancellationToken) ?? 0;

            var entry = new InpFinancialClearance
            {
                Id = Guid.NewGuid(),
                EpisodeId = episodeId,
                SequenceNumber = lastSequence + 1,
                ClearanceStatus = (InpFinancialClearanceStatus)request.ClearanceStatus,
                MarkedAt = now,
                MarkedByUserId = actorUserId,
                Note = request.Note.Trim(),
                IsManualMarking = true,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<InpFinancialClearance>().Add(entry);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return InpDischargeSummaryOperationResult.Success(
                entry.Id,
                "Kelayakan keuangan berhasil ditandai.");
        }

        // =====================================================================
        // BE-RWI-025 — Kelima syarat penutupan dan penutupan episode
        // =====================================================================

        /// <summary>
        /// Memeriksa kelima syarat penutupan dan mengembalikan keadaannya satu per satu.
        /// </summary>
        /// <remarks>
        /// Kelima syaratnya mengikuti <c>RWI-RULE-010</c> apa adanya:
        ///
        /// <list type="number">
        /// <item><description>keputusan pulang dari DPJP sudah ada;</description></item>
        /// <item><description>resume pulang sudah ada dan tertandatangani;</description></item>
        /// <item><description>seluruh butir wajib daftar periksa administrasi sudah ditandai;</description></item>
        /// <item><description>kelayakan keuangan bernilai <c>Cleared</c>;</description></item>
        /// <item><description>keadaan tempat tidur pasien sudah jelas.</description></item>
        /// </list>
        ///
        /// <para>
        /// <b>Syarat kelima perlu penjelasan.</b> <c>RWI-RULE-010</c> menuliskannya sebagai
        /// "tempat tidur aktif ditemukan". Sejak <c>RWI-DEC-055</c> melonggarkan
        /// <c>INV-INP-01</c>, episode <c>DischargePending</c> yang kepergiannya sudah dicatat
        /// memang <b>tidak lagi</b> memegang tempat tidur — dan episode itu tetap harus dapat
        /// ditutup. Syarat kelima karena itu dibaca sebagai: episode memegang penempatan aktif
        /// yang akan dilepas saat penutupan, <b>atau</b> kepergiannya sudah dicatat. Yang
        /// gagal hanyalah episode yang tidak punya keduanya — keadaan yang seharusnya mustahil,
        /// dan justru karena itu perlu terlihat bila terjadi.
        /// </para>
        /// </remarks>
        public async Task<ClosureReadinessResponse?> EvaluateClosureReadinessAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return null;
            }

            var conditions = await BuildClosureConditionsAsync(episode, cancellationToken);

            return new ClosureReadinessResponse
            {
                EpisodeId = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                EpisodeStatus = (int)episode.EpisodeStatus,
                EpisodeStatusName = episode.EpisodeStatus.ToString(),
                IsReady = conditions.All(x => x.IsSatisfied),
                IsReadyWithOverride = conditions.All(x => x.IsSatisfied || x.CanBeOverridden),
                Conditions = conditions
            };
        }

        /// <summary>
        /// Menutup episode: melepas tempat tidur, menutup penugasan DPJP dan perawat, lalu
        /// memindahkan status menjadi <c>Closed</c> — seluruhnya di dalam satu transaksi.
        /// </summary>
        public Task<InpEpisodeOperationResult> CloseEpisodeAsync(
            Guid episodeId,
            CloseEpisodeRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            return CloseEpisodeInternalAsync(
                episodeId,
                NormalizeText(request?.Note),
                actorUserId,
                isOverride: false,
                actorIsSupervisor: false,
                cancellationToken);
        }

        // =====================================================================
        // BE-RWI-026 — Jalan keluar supervisor
        // =====================================================================

        /// <summary>
        /// Supervisor menutup episode menembus gerbang keuangan.
        /// </summary>
        /// <remarks>
        /// <b>Menembus satu syarat saja.</b> Keempat syarat lainnya tetap menahan: keputusan
        /// pulang, resume tertandatangani, butir wajib administrasi, dan keadaan tempat tidur.
        /// Mencoba menembus dengan resume yang belum ditandatangani tetap ditolak 422.
        ///
        /// <para>
        /// Setiap penutupan lewat jalur ini menandai <c>IsClosedWithoutFinancialClearance</c>,
        /// menyimpan alasannya, dan memunculkan episodenya pada daftar pantau penutupan
        /// menembus gerbang keuangan. Jalan keluar yang tidak meninggalkan jejak akan menjadi
        /// jalur normal dalam hitungan minggu.
        /// </para>
        /// </remarks>
        public async Task<InpEpisodeOperationResult> CloseWithOverrideAsync(
            Guid episodeId,
            CloseEpisodeOverrideRequest request,
            Guid actorUserId,
            bool actorIsSupervisor,
            CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reason) ||
                !request.Reason.Any(char.IsLetterOrDigit))
            {
                return InpEpisodeOperationResult.Invalid(
                    "Alasan penutupan tanpa kelayakan keuangan wajib diisi.");
            }

            if (!actorIsSupervisor)
            {
                return InpEpisodeOperationResult.Forbidden(
                    "Hanya supervisor yang dapat menutup episode tanpa kelayakan keuangan.");
            }

            return await CloseEpisodeInternalAsync(
                episodeId,
                request.Reason.Trim(),
                actorUserId,
                isOverride: true,
                actorIsSupervisor: true,
                cancellationToken);
        }

        // =====================================================================
        // BE-RWI-027 — Kepergian fisik pasien
        // =====================================================================

        /// <summary>
        /// Mencatat pasien sudah meninggalkan ruangan. Melepas tempat tidur seketika
        /// <b>tanpa</b> menutup episode.
        /// </summary>
        /// <remarks>
        /// <b>Kepergian fisik bukan perubahan status, sehingga ia tidak menulis
        /// <c>InpStatusHistory</c>.</b> Episode tetap <c>DischargePending</c> dan tetap wajib
        /// ditutup. <c>RWI-DEC-009</c> mengunci lima nilai status, dan kepergian fisik sengaja
        /// tidak dijadikan status keenam — ia fakta yang dicatat, bukan tahapan yang dilalui.
        /// Jejaknya tersimpan pada baris penempatan yang ditutup dengan alasan
        /// <c>PatientDeparted</c>, ditambah dua kolom pada episode.
        ///
        /// <para>
        /// <b>Yang sengaja tidak divalidasi.</b> Sistem tidak memeriksa apakah butir
        /// administrasi atau kelayakan keuangan sudah selesai. Kepergian fisik adalah fakta,
        /// bukan izin — pasien yang sudah pulang tetap harus dicatat pulang walaupun
        /// administrasinya belum beres.
        /// </para>
        ///
        /// <para>
        /// <b>Tidak dapat dibatalkan.</b> <c>RWI-RULE-036</c> menetapkan tidak ada pembatalan.
        /// Pasien yang ternyata belum jadi pulang menjalani admisi baru.
        /// </para>
        /// </remarks>
        public async Task<InpEpisodeOperationResult> RecordPatientDepartureAsync(
            Guid episodeId,
            RecordDepartureRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            if (episode.EpisodeStatus != InpEpisodeStatus.DischargePending)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Kepergian hanya dapat dicatat setelah DPJP menyatakan pasien boleh pulang.",
                    episode);
            }

            if (episode.PhysicallyLeftAt.HasValue)
            {
                return InpEpisodeOperationResult.Conflict(
                    $"Kepergian pasien sudah dicatat pada pukul " +
                    $"{episode.PhysicallyLeftAt.Value:HH:mm}.",
                    episode);
            }

            var now = DateTime.UtcNow;
            var departedAt = request?.DepartedAt ?? now;

            if (departedAt > now)
            {
                return InpEpisodeOperationResult.Invalid(
                    "Waktu kepergian tidak boleh melewati waktu sekarang.");
            }

            if (episode.DischargeDecidedAt.HasValue && departedAt < episode.DischargeDecidedAt.Value)
            {
                return InpEpisodeOperationResult.Invalid(
                    "Waktu kepergian tidak boleh mendahului keputusan pulang.");
            }

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                await _bedOccupancyService.ReleaseActivePlacementAsync(
                    episode.Id,
                    InpBedPlacementEndReason.PatientDeparted,
                    actorUserId,
                    departedAt,
                    cancellationToken);

                episode.PhysicallyLeftAt = departedAt;
                episode.PhysicallyLeftByUserId = actorUserId;
                episode.UpdateDateTime = now;
                episode.UpdateBy = actorUserId;

                // Sengaja TIDAK memanggil ApplyStatusChangeAsync. Status episode tidak berubah,
                // dan RWI-RULE-031 aturan 3 mewajibkan riwayat untuk perubahan status — bukan
                // untuk setiap tindakan.
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpEpisodeOperationResult.Success(
                    episode,
                    "Kepergian pasien berhasil dicatat. Tempat tidur sudah dilepas; episode " +
                    "tetap perlu ditutup.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Menyusun kelima syarat penutupan beserta keadaannya masing-masing.
        /// </summary>
        private async Task<List<ClosureConditionResponse>> BuildClosureConditionsAsync(
            InpEpisode episode,
            CancellationToken cancellationToken)
        {
            var summary = await _dbContext.Set<InpDischargeSummary>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episode.Id && !x.IsDelete)
                .Select(x => new { x.SignedAt })
                .FirstOrDefaultAsync(cancellationToken);

            var checklist = await GetClearanceChecklistAsync(episode.Id, cancellationToken);
            var blockingItems = checklist?.Items.Where(x => x.IsBlocking).ToList()
                ?? new List<ClearanceChecklistItemResponse>();

            var financial = await GetFinancialClearanceAsync(episode.Id, cancellationToken);

            var hasActivePlacement = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.EpisodeId == episode.Id && x.EndDateTime == null && !x.IsDelete,
                    cancellationToken);

            return new List<ClosureConditionResponse>
            {
                new()
                {
                    Number = 1,
                    Code = "DISCHARGE_DECIDED",
                    Label = "Keputusan pulang dari DPJP sudah ada",
                    IsSatisfied = episode.EpisodeStatus == InpEpisodeStatus.DischargePending,
                    UnmetMessage = episode.EpisodeStatus == InpEpisodeStatus.DischargePending
                        ? null
                        : "Episode hanya dapat ditutup setelah DPJP menyatakan pasien boleh pulang.",
                    CanBeOverridden = false
                },
                new()
                {
                    Number = 2,
                    Code = "SUMMARY_SIGNED",
                    Label = "Resume pulang sudah ditandatangani DPJP",
                    IsSatisfied = summary?.SignedAt != null,
                    UnmetMessage = summary?.SignedAt != null
                        ? null
                        : "Resume pulang belum ditandatangani DPJP.",
                    CanBeOverridden = false
                },
                new()
                {
                    Number = 3,
                    Code = "CLEARANCE_COMPLETE",
                    Label = "Seluruh butir wajib administrasi sudah ditandai",
                    IsSatisfied = blockingItems.Count == 0,
                    UnmetMessage = blockingItems.Count == 0
                        ? null
                        : "Masih ada butir administrasi yang belum ditandai: " +
                          string.Join(", ", blockingItems.Select(x => x.ItemName)) + ".",
                    CanBeOverridden = false
                },
                new()
                {
                    Number = 4,
                    Code = "FINANCIAL_CLEARED",
                    Label = "Kelayakan keuangan dinyatakan lunas kasir",
                    IsSatisfied = financial?.IsCleared == true,
                    UnmetMessage = financial?.IsCleared == true
                        ? null
                        : "Kelayakan keuangan belum dinyatakan lunas oleh kasir.",
                    // Satu-satunya syarat yang dapat ditembus supervisor.
                    CanBeOverridden = true
                },
                new()
                {
                    Number = 5,
                    Code = "BED_STATE_RESOLVED",
                    Label = "Keadaan tempat tidur pasien sudah jelas",
                    IsSatisfied = hasActivePlacement || episode.PhysicallyLeftAt.HasValue,
                    UnmetMessage = hasActivePlacement || episode.PhysicallyLeftAt.HasValue
                        ? null
                        : "Episode ini tidak memegang tempat tidur dan kepergian pasiennya " +
                          "belum dicatat. Betulkan catatan penempatannya lebih dulu.",
                    CanBeOverridden = false
                }
            };
        }

        /// <summary>
        /// Isi penutupan yang dipakai bersama oleh penutupan biasa dan jalan keluar supervisor.
        /// </summary>
        /// <remarks>
        /// Keduanya memakai jalur yang sama persis; yang berbeda hanya apakah syarat kelayakan
        /// keuangan boleh dilewati. Menulis dua jalur penutupan yang terpisah akan membuat
        /// keduanya berselisih pada perubahan berikutnya — dan yang paling mungkin terlewat
        /// adalah jalur yang lebih jarang dipakai, yaitu jalan keluar supervisor.
        /// </remarks>
        private async Task<InpEpisodeOperationResult> CloseEpisodeInternalAsync(
            Guid episodeId,
            string? reason,
            Guid actorUserId,
            bool isOverride,
            bool actorIsSupervisor,
            CancellationToken cancellationToken)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Closed)
            {
                return InpEpisodeOperationResult.Conflict("Episode sudah ditutup.", episode);
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Cancelled)
            {
                return InpEpisodeOperationResult.Conflict(
                    "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan.",
                    episode);
            }

            var conditions = await BuildClosureConditionsAsync(episode, cancellationToken);

            var unmet = conditions
                .Where(x => !x.IsSatisfied)
                .Where(x => !(isOverride && x.CanBeOverridden))
                .ToList();

            if (unmet.Count > 0)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    string.Join(" ", unmet.Select(x => x.UnmetMessage)),
                    episode);
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                await _bedOccupancyService.ReleaseActivePlacementAsync(
                    episode.Id,
                    InpBedPlacementEndReason.EpisodeClosed,
                    actorUserId,
                    now,
                    cancellationToken);

                await CloseActiveAssignmentsAsync(episode.Id, actorUserId, now, cancellationToken);

                episode.ClosedAt = now;

                if (isOverride)
                {
                    episode.IsClosedWithoutFinancialClearance = true;
                    episode.ClosedWithoutClearanceReason = reason;
                }

                await _episodeService.ApplyStatusChangeAsync(
                    episode,
                    fromStatus: InpEpisodeStatus.DischargePending,
                    toStatus: InpEpisodeStatus.Closed,
                    actionType: isOverride ? ActionCloseEpisodeWithOverride : ActionCloseEpisode,
                    actorType: InpStatusChangeActorType.User,
                    changedByUserId: actorUserId,
                    reason: reason,
                    now: now,
                    touchEpisode: true,
                    cancellationToken: cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _ = actorIsSupervisor;

                return InpEpisodeOperationResult.Success(
                    episode,
                    isOverride
                        ? "Episode ditutup tanpa kelayakan keuangan. Penutupan ini tercatat " +
                          "pada daftar pantau."
                        : "Episode berhasil ditutup dan tempat tidur sudah dilepas.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Menutup penugasan DPJP dan perawat yang masih aktif saat episode ditutup.
        /// </summary>
        /// <remarks>
        /// Tanpa ini, riwayat penugasan berakhir menggantung: DPJP terlihat masih bertanggung
        /// jawab atas pasien yang sudah pulang berbulan-bulan lalu, dan pertanyaan "siapa DPJP
        /// pada tanggal tertentu" menjawab benar untuk tanggal yang salah.
        /// </remarks>
        private async Task CloseActiveAssignmentsAsync(
            Guid episodeId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var doctorAssignments = await _dbContext.Set<InpDoctorAssignment>()
                .Where(x => x.EpisodeId == episodeId && x.EndDateTime == null && !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var assignment in doctorAssignments)
            {
                assignment.EndDateTime = now;
                assignment.IsActive = false;
                assignment.UpdateDateTime = now;
                assignment.UpdateBy = actorUserId;
            }

            var nurseAssignments = await _dbContext.Set<InpNurseAssignment>()
                .Where(x => x.EpisodeId == episodeId && x.EndDateTime == null && !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var assignment in nurseAssignments)
            {
                assignment.EndDateTime = now;
                assignment.IsActive = false;
                assignment.UpdateDateTime = now;
                assignment.UpdateBy = actorUserId;
            }
        }

        /// <summary>
        /// Menolak perubahan pada episode yang sudah ditutup, kecuali ada sesi koreksi terbuka.
        /// Mengembalikan <c>null</c> bila perubahan boleh diteruskan.
        /// </summary>
        /// <remarks><c>INV-INP-06</c>.</remarks>
        private async Task<InpDischargeSummaryOperationResult?> GuardEpisodeNotClosedAsync(
            InpEpisode episode,
            CancellationToken cancellationToken)
        {
            if (episode.EpisodeStatus == InpEpisodeStatus.Cancelled)
            {
                return InpDischargeSummaryOperationResult.Conflict(
                    "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan.");
            }

            if (episode.EpisodeStatus != InpEpisodeStatus.Closed)
            {
                return null;
            }

            var hasOpenSession = await _dbContext.Set<InpCorrectionSession>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.EpisodeId == episode.Id && x.ClosedAt == null && !x.IsDelete,
                    cancellationToken);

            return hasOpenSession
                ? null
                : InpDischargeSummaryOperationResult.Conflict("Episode sudah ditutup.");
        }
    }
}
