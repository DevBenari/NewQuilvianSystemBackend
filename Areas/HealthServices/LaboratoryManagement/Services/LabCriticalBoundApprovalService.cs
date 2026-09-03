using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Jalur pengajuan dan persetujuan perubahan batas kritis (<c>LAB-DEC-023</c>, BR-19).
    ///
    /// Satu aturan menjadi alasan keberadaan seluruh berkas ini: <b>pengaju tidak boleh
    /// menyetujui pengajuannya sendiri</b> (<c>VAL-33</c>). Ini bukan formalitas administrasi.
    /// Batas kritis menentukan pada angka berapa seorang pasien dianggap terancam; membiarkan
    /// satu orang mengusulkan sekaligus mengesahkan perubahannya sama saja dengan tidak punya
    /// persetujuan sama sekali.
    ///
    /// <b>Aturan itu wajib ada sebagai kode di sini, bukan sebagai konfigurasi permission.</b>
    /// <c>CAP-16</c> sudah membuktikan sistem izin yang ada tidak dapat menegakkannya:
    /// <c>AccessPermissionService.HasAccessAsync</c> menjawab "boleh atau tidak" untuk sebuah
    /// aksi, dan tidak pernah membandingkan siapa pelaku sebelumnya pada baris data yang sama.
    /// Seseorang yang memegang <c>LabCriticalBound : Approve</c> akan lolos pemeriksaan izin
    /// walaupun dialah yang mengajukan.
    ///
    /// Karena itu setiap tindakan di sini menolak pelaku yang tidak dikenali. Identitas yang
    /// kosong akan membuat perbandingan "pengaju versus pemutus" kehilangan artinya, dan sebuah
    /// pengaman yang kehilangan artinya lebih berbahaya daripada pengaman yang tidak ada —
    /// karena ia tetap terlihat bekerja.
    /// </summary>
    public class LabCriticalBoundApprovalService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        /// <summary>
        /// Penanda usulan "tidak ada satu pun pilihan yang kritis". Dibedakan dari kosong,
        /// karena kosong berarti daftar pilihan kritis tidak diusulkan berubah sama sekali.
        /// </summary>
        public const string NoCriticalOptions = "-";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabCriticalBoundApprovalService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        // =================================================================
        // Baca
        // =================================================================

        public async Task<List<LabBoundChangeRequestResponse>> GetListAsync(
            Guid valueBoundId,
            CancellationToken cancellationToken = default)
        {
            await LoadBoundAsync(valueBoundId, tracking: false, cancellationToken);

            return await _dbContext.LabValueBoundChangeRequests
                .AsNoTracking()
                .Include(x => x.ValueBound)
                .ThenInclude(x => x!.Procedure)
                .Where(x => x.ValueBoundId == valueBoundId && !x.IsDelete)
                .OrderByDescending(x => x.RequestedAt)
                .Select(x => MapProjection(x))
                .ToListAsync(cancellationToken);
        }

        // =================================================================
        // Mengajukan
        // =================================================================

        public async Task<LabBoundChangeRequestResponse> SubmitAsync(
            Guid valueBoundId,
            SubmitCriticalBoundChangeRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = RequireActor();
            var bound = await LoadBoundAsync(valueBoundId, tracking: false, cancellationToken);

            // VAL-31. Batas kritis adalah angka keselamatan; perubahannya tanpa alasan tertulis
            // tidak dapat ditelusuri kemudian.
            if (string.IsNullOrWhiteSpace(request.RequestReason))
                throw new LabCriticalBoundValidationException(
                    "Jelaskan alasan perubahan batas kritis ini.");

            var optionCodes = Normalize(request.ProposedCriticalOptionCodes);

            if (!request.ProposedCriticalLow.HasValue &&
                !request.ProposedCriticalHigh.HasValue &&
                optionCodes == null)
            {
                throw new LabCriticalBoundValidationException(
                    "Pengajuan tidak memuat satu pun usulan perubahan batas kritis.");
            }

            // Usulan wajib masuk akal terhadap batas normal yang berlaku, dengan aturan yang
            // sama persis seperti VAL-26 dan VAL-27 pada jalur pengelolaan biasa. Memeriksanya
            // saat diajukan berarti pemutus tidak pernah dihadapkan pada usulan yang mustahil
            // disetujui.
            EnsureProposalFitsResultForm(bound, request, optionCodes);
            EnsureProposedBoundsMakeSense(bound, request.ProposedCriticalLow, request.ProposedCriticalHigh);

            // VAL-32. Dua pengajuan berjalan atas batas nilai yang sama akan membuat urutan
            // penerapannya bergantung pada urutan persetujuan, dan pemutus kedua tidak akan tahu
            // ia sedang menimpa keputusan pertama.
            var adaPengajuanBerjalan = await _dbContext.LabValueBoundChangeRequests
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ValueBoundId == valueBoundId &&
                    !x.IsDelete &&
                    x.RequestStatus == LabBoundChangeStatus.Submitted,
                    cancellationToken);

            if (adaPengajuanBerjalan)
                throw new LabCriticalBoundConflictException(
                    "Masih ada pengajuan yang belum diputuskan untuk batas nilai ini.");

            var now = DateTime.UtcNow;

            var entity = new LabValueBoundChangeRequest
            {
                ValueBoundId = valueBoundId,
                RequestStatus = LabBoundChangeStatus.Submitted,
                ProposedCriticalLow = request.ProposedCriticalLow,
                ProposedCriticalHigh = request.ProposedCriticalHigh,
                ProposedCriticalOptionCodes = optionCodes,
                RequestReason = request.RequestReason.Trim(),
                RequestedByUserId = actorUserId,
                RequestedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabValueBoundChangeRequests.Add(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabCriticalBound.Submit",
                "Mengajukan perubahan batas kritis.",
                new
                {
                    entity.Id,
                    entity.ValueBoundId,
                    entity.ProposedCriticalLow,
                    entity.ProposedCriticalHigh,
                    entity.ProposedCriticalOptionCodes,
                    RequestedByUserId = actorUserId
                });

            return await ReadBackAsync(entity.Id, cancellationToken);
        }

        // =================================================================
        // Menyetujui dan menolak
        // =================================================================

        public async Task<LabBoundChangeRequestResponse> ApproveAsync(
            Guid valueBoundId,
            Guid requestId,
            DecideCriticalBoundChangeRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = RequireActor();
            var entity = await LoadDecidableRequestAsync(valueBoundId, requestId, actorUserId, cancellationToken);

            var bound = await LoadBoundAsync(valueBoundId, tracking: true, cancellationToken);
            var now = DateTime.UtcNow;

            // Diperiksa ulang di sini, bukan hanya saat diajukan. Batas normal boleh berubah
            // lewat PUT biasa kapan saja, termasuk sesudah pengajuan ini dibuat — dan usulan
            // yang tadinya masuk akal bisa menjadi mustahil. Menyetujuinya tanpa memeriksa ulang
            // akan menyimpan batas kritis yang berada di dalam rentang normal, sehingga angka
            // yang sehat pun ikut terhitung kritis.
            EnsureProposedBoundsMakeSense(bound, entity.ProposedCriticalLow, entity.ProposedCriticalHigh);

            // Batas kritis yang baru mulai berlaku tepat di sini, dan tidak di tempat lain mana
            // pun. Setiap perubahannya menerbitkan riwayat beserta penyetujunya, sehingga
            // AC-34 terpenuhi untuk jalur yang memang memerlukan persetujuan.
            if (entity.ProposedCriticalLow.HasValue && entity.ProposedCriticalLow != bound.CriticalLow)
            {
                AppendHistory(
                    bound, nameof(LabValueBound.CriticalLow),
                    Format(bound.CriticalLow), Format(entity.ProposedCriticalLow),
                    entity.RequestedByUserId, actorUserId, entity.RequestReason, now);

                bound.CriticalLow = entity.ProposedCriticalLow;
            }

            if (entity.ProposedCriticalHigh.HasValue && entity.ProposedCriticalHigh != bound.CriticalHigh)
            {
                AppendHistory(
                    bound, nameof(LabValueBound.CriticalHigh),
                    Format(bound.CriticalHigh), Format(entity.ProposedCriticalHigh),
                    entity.RequestedByUserId, actorUserId, entity.RequestReason, now);

                bound.CriticalHigh = entity.ProposedCriticalHigh;
            }

            if (entity.ProposedCriticalOptionCodes != null)
            {
                ApplyCriticalOptions(bound, entity, actorUserId, now);
            }

            bound.UpdateDateTime = now;
            bound.UpdateBy = actorUserId;

            entity.RequestStatus = LabBoundChangeStatus.Approved;
            entity.DecidedByUserId = actorUserId;
            entity.DecidedAt = now;
            entity.DecisionNote = Normalize(request.DecisionNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            await SaveDecisionAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabCriticalBound.Approve",
                "Menyetujui perubahan batas kritis.",
                new
                {
                    entity.Id,
                    entity.ValueBoundId,
                    entity.RequestedByUserId,
                    DecidedByUserId = actorUserId
                });

            return await ReadBackAsync(entity.Id, cancellationToken);
        }

        public async Task<LabBoundChangeRequestResponse> RejectAsync(
            Guid valueBoundId,
            Guid requestId,
            DecideCriticalBoundChangeRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = RequireActor();
            var entity = await LoadDecidableRequestAsync(valueBoundId, requestId, actorUserId, cancellationToken);

            var now = DateTime.UtcNow;

            // Penolakan tidak menyentuh batas nilai sama sekali; yang berlaku tetap yang lama.
            entity.RequestStatus = LabBoundChangeStatus.Rejected;
            entity.DecidedByUserId = actorUserId;
            entity.DecidedAt = now;
            entity.DecisionNote = Normalize(request.DecisionNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.Version++;

            await SaveDecisionAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabCriticalBound.Reject",
                "Menolak perubahan batas kritis.",
                new { entity.Id, entity.ValueBoundId, entity.RequestedByUserId, DecidedByUserId = actorUserId });

            return await ReadBackAsync(entity.Id, cancellationToken);
        }

        // =================================================================
        // Menarik
        // =================================================================

        public async Task<LabBoundChangeRequestResponse> WithdrawAsync(
            Guid valueBoundId,
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = RequireActor();

            var entity = await _dbContext.LabValueBoundChangeRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == requestId && x.ValueBoundId == valueBoundId && !x.IsDelete,
                    cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Pengajuan perubahan batas kritis tidak ditemukan.");

            // VAL-34 diperiksa lebih dulu: pengajuan yang sudah diputuskan tidak dapat ditarik
            // oleh siapa pun, termasuk pengajunya.
            if (entity.RequestStatus != LabBoundChangeStatus.Submitted)
                throw new LabCriticalBoundConflictException("Pengajuan ini sudah diputuskan.");

            // VAL-35
            if (entity.RequestedByUserId != actorUserId)
                throw new LabCriticalBoundForbiddenException(
                    "Hanya pengaju yang boleh menarik pengajuannya.");

            var now = DateTime.UtcNow;

            entity.RequestStatus = LabBoundChangeStatus.Withdrawn;

            // DecidedByUserId sengaja dibiarkan kosong. Ruas itu berarti "pihak berwenang yang
            // memutuskan", dan menarik pengajuan sendiri bukan keputusan pihak berwenang.
            // Mengisinya dengan pengaju akan membuat baris ini terbaca seolah pengaju dan
            // pemutusnya orang yang sama — persis keadaan yang VAL-33 ada untuk mencegahnya.
            // Siapa yang menarik sudah terjawab RequestedByUserId, karena hanya pengaju yang
            // boleh menarik (VAL-35).
            entity.DecidedAt = now;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            // CAP-17. Tanpa kenaikan ini token konkurensi tidak pernah berubah, sehingga klausa
            // WHERE milik EF tetap cocok bagi penulis kedua dan keduanya sama-sama berhasil —
            // penjaga yang terlihat ada padahal tidak pernah menyala.
            entity.Version++;

            await SaveDecisionAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabCriticalBound.Withdraw",
                "Menarik pengajuan perubahan batas kritis.",
                new { entity.Id, entity.ValueBoundId, ActorUserId = actorUserId });

            return await ReadBackAsync(entity.Id, cancellationToken);
        }

        // =================================================================
        // Penegakan invariant keselamatan
        // =================================================================

        /// <summary>
        /// Memuat pengajuan yang boleh diputuskan, sekaligus menegakkan <c>VAL-34</c> dan
        /// <c>VAL-33</c> dalam urutan itu.
        ///
        /// Urutannya disengaja: pengajuan yang sudah diputuskan menghasilkan <c>409</c> tanpa
        /// memandang siapa pelakunya, sehingga jawaban sistem tidak berubah-ubah tergantung
        /// siapa yang bertanya.
        /// </summary>
        private async Task<LabValueBoundChangeRequest> LoadDecidableRequestAsync(
            Guid valueBoundId,
            Guid requestId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.LabValueBoundChangeRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == requestId && x.ValueBoundId == valueBoundId && !x.IsDelete,
                    cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Pengajuan perubahan batas kritis tidak ditemukan.");

            // VAL-34
            if (entity.RequestStatus != LabBoundChangeStatus.Submitted)
                throw new LabCriticalBoundConflictException("Pengajuan ini sudah diputuskan.");

            // VAL-33 — inti seluruh task ini. Ditulis di sini, bukan diserahkan kepada sistem
            // permission, karena sistem itu tidak pernah membandingkan pelaku sebelumnya.
            if (entity.RequestedByUserId == actorUserId)
                throw new LabCriticalBoundForbiddenException(
                    "Pengaju tidak boleh menyetujui pengajuannya sendiri.");

            return entity;
        }

        /// <summary>
        /// Usulan wajib cocok dengan bentuk hasil batas nilainya, dan setiap kode pilihan yang
        /// diusulkan wajib benar-benar ada.
        ///
        /// Yang dicegah pemeriksaan kode pilihan bukan sekadar salah ketik. Penerapan bekerja
        /// dengan menyalakan penanda kritis pada pilihan yang disebut dan <b>memadamkannya</b>
        /// pada yang tidak disebut. Satu kode yang keliru — <c>P5</c> alih-alih <c>P4</c> —
        /// karena itu tidak berakhir sebagai kesalahan, melainkan sebagai pencabutan diam-diam
        /// seluruh penanda kritis yang sudah ada.
        /// </summary>
        private static void EnsureProposalFitsResultForm(
            LabValueBound bound,
            SubmitCriticalBoundChangeRequest request,
            string? optionCodes)
        {
            if (bound.ResultForm == LabResultForm.Numeric)
            {
                if (optionCodes != null)
                    throw new LabCriticalBoundValidationException(
                        "Pemeriksaan berhasil angka tidak punya daftar pilihan, sehingga pilihan kritis tidak dapat diusulkan.");

                return;
            }

            if (request.ProposedCriticalLow.HasValue || request.ProposedCriticalHigh.HasValue)
                throw new LabCriticalBoundValidationException(
                    "Pemeriksaan berhasil pilihan tidak memakai batas kritis berupa angka.");

            if (optionCodes == null || optionCodes == NoCriticalOptions) return;

            var dikenal = bound.Options
                .Where(x => !x.IsDelete)
                .Select(x => x.OptionCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var tidakDikenal = optionCodes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !dikenal.Contains(x))
                .ToList();

            if (tidakDikenal.Count > 0)
                throw new LabCriticalBoundValidationException(
                    $"Kode pilihan berikut tidak ada pada batas nilai ini: {string.Join(", ", tidakDikenal)}.");
        }

        /// <summary>
        /// Menegakkan <c>VAL-26</c> dan <c>VAL-27</c> terhadap batas normal yang berlaku.
        ///
        /// Dipanggil dua kali — saat diajukan dan saat diputuskan — karena batas normal dapat
        /// bergeser di antara keduanya lewat jalur pengelolaan biasa.
        /// </summary>
        private static void EnsureProposedBoundsMakeSense(
            LabValueBound bound,
            decimal? proposedLow,
            decimal? proposedHigh)
        {
            var usulanLow = proposedLow ?? bound.CriticalLow;
            var usulanHigh = proposedHigh ?? bound.CriticalHigh;

            if (usulanLow.HasValue && bound.NormalLow.HasValue && usulanLow > bound.NormalLow)
                throw new LabCriticalBoundValidationException(
                    "Batas kritis bawah harus lebih rendah daripada batas normal bawah.");

            if (usulanHigh.HasValue && bound.NormalHigh.HasValue && usulanHigh < bound.NormalHigh)
                throw new LabCriticalBoundValidationException(
                    "Batas kritis atas harus lebih tinggi daripada batas normal atas.");
        }

        /// <summary>
        /// Mengembalikan identitas pelaku, dan menolak bila ia tidak dikenali.
        ///
        /// Tanpa penolakan ini, dua pelaku yang sama-sama tidak dikenali akan terbaca sebagai
        /// orang yang sama, dan yang lebih berbahaya: pengajuan yang dibuat pengguna sungguhan
        /// dapat disetujui oleh pemanggil tanpa identitas, karena <c>Guid.Empty</c> tidak sama
        /// dengan id pengaju mana pun. <c>VAL-33</c> akan tampak bekerja padahal sudah bocor.
        /// </summary>
        private Guid RequireActor()
        {
            var actorUserId = GetCurrentUserId();

            if (actorUserId == Guid.Empty)
                throw new LabCriticalBoundForbiddenException(
                    "Identitas pengguna tidak dikenali, sehingga tindakan ini tidak dapat dipertanggungjawabkan.");

            return actorUserId;
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private void ApplyCriticalOptions(
            LabValueBound bound,
            LabValueBoundChangeRequest entity,
            Guid actorUserId,
            DateTime now)
        {
            var usulan = entity.ProposedCriticalOptionCodes == NoCriticalOptions
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : entity.ProposedCriticalOptionCodes!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var pilihan = bound.Options.Where(x => !x.IsDelete).ToList();

            var lama = string.Join(",", pilihan.Where(x => x.IsCritical).OrderBy(x => x.SortOrder).Select(x => x.OptionCode));
            var baru = string.Join(",", pilihan.Where(x => usulan.Contains(x.OptionCode)).OrderBy(x => x.SortOrder).Select(x => x.OptionCode));

            if (lama == baru) return;

            AppendHistory(
                bound, "CriticalOptions", lama.Length == 0 ? null : lama, baru.Length == 0 ? null : baru,
                entity.RequestedByUserId, actorUserId, entity.RequestReason, now);

            foreach (var option in pilihan)
            {
                var kritisBaru = usulan.Contains(option.OptionCode);

                if (option.IsCritical == kritisBaru) continue;

                option.IsCritical = kritisBaru;
                option.UpdateDateTime = now;
                option.UpdateBy = actorUserId;
            }
        }

        private void AppendHistory(
            LabValueBound bound,
            string changedField,
            string? oldValue,
            string? newValue,
            Guid actorUserId,
            Guid approvedByUserId,
            string? changeReason,
            DateTime now)
        {
            _dbContext.LabValueBoundHistories.Add(new LabValueBoundHistory
            {
                ValueBoundId = bound.Id,
                ChangedField = changedField,
                // Kedua kolom dibatasi 200 karakter. Daftar pilihan kritis yang panjang dapat
                // melewatinya, dan kegagalan menyimpan riwayat akan menggagalkan persetujuan
                // yang sebenarnya sah.
                OldValue = Truncate(oldValue),
                NewValue = Truncate(newValue),
                // Pelaku adalah pengajunya, bukan pemutusnya. Riwayat harus menjawab siapa yang
                // menghendaki perubahan ini, dan siapa yang mengesahkannya — dua orang berbeda.
                ActorUserId = actorUserId,
                ApprovedByUserId = approvedByUserId,
                ChangeReason = changeReason,
                OccurredAt = now,
                CreateDateTime = now,
                CreateBy = approvedByUserId
            });
        }

        private async Task SaveDecisionAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // CAP-17. Dua pemutus yang memutuskan pengajuan yang sama secara bersamaan tidak
                // boleh sama-sama berhasil; keduanya akan menulis batas kritis yang berbeda.
                throw new LabCriticalBoundConflictException(
                    "Pengajuan ini baru saja diputuskan petugas lain. Muat ulang lalu periksa keputusannya.");
            }
        }

        private async Task<LabValueBound> LoadBoundAsync(
            Guid valueBoundId,
            bool tracking,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.LabValueBounds
                .Include(x => x.Options)
                .Include(x => x.Procedure)
                .AsQueryable();

            if (!tracking) query = query.AsNoTracking();

            var bound = await query.FirstOrDefaultAsync(
                x => x.Id == valueBoundId && !x.IsDelete, cancellationToken);

            if (bound == null)
                throw new KeyNotFoundException("Batas nilai tidak ditemukan.");

            return bound;
        }

        private async Task<LabBoundChangeRequestResponse> ReadBackAsync(
            Guid requestId,
            CancellationToken cancellationToken)
        {
            var hasil = await _dbContext.LabValueBoundChangeRequests
                .AsNoTracking()
                .Include(x => x.ValueBound)
                .ThenInclude(x => x!.Procedure)
                .FirstAsync(x => x.Id == requestId, cancellationToken);

            return MapProjection(hasil);
        }

        private static LabBoundChangeRequestResponse MapProjection(LabValueBoundChangeRequest x) =>
            new()
            {
                Id = x.Id,
                ValueBoundId = x.ValueBoundId,
                ProcedureName = x.ValueBound != null && x.ValueBound.Procedure != null
                    ? x.ValueBound.Procedure.ProcedureName
                    : string.Empty,
                RequestStatus = x.RequestStatus.ToString(),
                CurrentCriticalLow = x.ValueBound != null ? x.ValueBound.CriticalLow : null,
                CurrentCriticalHigh = x.ValueBound != null ? x.ValueBound.CriticalHigh : null,
                ProposedCriticalLow = x.ProposedCriticalLow,
                ProposedCriticalHigh = x.ProposedCriticalHigh,
                ProposedCriticalOptionCodes = x.ProposedCriticalOptionCodes,
                RequestReason = x.RequestReason,
                RequestedByUserId = x.RequestedByUserId,
                RequestedAt = x.RequestedAt,
                DecidedByUserId = x.DecidedByUserId,
                DecidedAt = x.DecidedAt,
                DecisionNote = x.DecisionNote
            };

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? Format(decimal? value) =>
            value?.ToString(CultureInfo.InvariantCulture);

        private static string? Truncate(string? value) =>
            value != null && value.Length > 200 ? value[..200] : value;

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>Pelanggaran aturan isi pengajuan. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabCriticalBoundValidationException(string message) : Exception(message);

    /// <summary>Bentrokan status atau konkurensi. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabCriticalBoundConflictException(string message) : Exception(message);

    /// <summary>Pelaku tidak berhak atas tindakan ini. Dipetakan menjadi <c>403</c>.</summary>
    public sealed class LabCriticalBoundForbiddenException(string message) : Exception(message);
}
