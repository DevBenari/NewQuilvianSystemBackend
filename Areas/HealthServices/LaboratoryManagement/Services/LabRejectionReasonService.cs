using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pengelolaan katalog alasan penolakan sampel (<c>LAB-DEC-019</c>, <c>BR-15</c>).
    ///
    /// Satu aturan membentuk seluruh berkas ini: <b>kepala instalasi memiliki penamaan, tidak
    /// memiliki akibat biaya.</b> Nama, keterangan, urutan tampil, dan status aktif sebuah
    /// alasan adalah urusan operasional laboratorium sehari-hari. Dua penanda lain tidak:
    /// penanda kesalahan internal menentukan apakah pengambilan ulang ditanggung rumah sakit
    /// atau boleh dibebankan kepada pasien, dan penanda wajib catatan menentukan kelengkapan
    /// bukti saat penolakan. Menurut <c>LAB-INH-010</c>, akibat finansial bukan wewenang
    /// Laboratorium — karena itu keduanya hanya bergerak lewat
    /// <c>PUT /{id}/system-flags</c> yang menuntut <c>LabRejectionReason : SystemFlag</c>.
    ///
    /// <c>VAL-37</c> ditegakkan di sini, bukan sekadar dengan tidak menyediakan ruasnya:
    /// permintaan ubah tetap menerima kedua penanda, lalu menolaknya dengan <c>403</c> begitu
    /// salah satunya disertakan. Menolak secara terbuka lebih jujur daripada mengabaikan
    /// diam-diam, karena pemanggil yang mengira penanda biayanya sudah berubah padahal tidak
    /// adalah keadaan yang justru berbahaya. Pola ini sama dengan <c>VAL-28</c> pada
    /// <see cref="LabValueBoundService"/>.
    /// </summary>
    public class LabRejectionReasonService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabRejectionReasonService(
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

        public async Task<PagedResult<LabRejectionReasonResponse>> GetListAsync(
            LabRejectionReasonPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var source = _dbContext.MstLabRejectionReasons
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    EF.Functions.ILike(x.ReasonCode, $"%{search}%") ||
                    EF.Functions.ILike(x.ReasonName, $"%{search}%") ||
                    (x.Description != null && EF.Functions.ILike(x.Description, $"%{search}%")));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => Map(x))
                .ToListAsync(cancellationToken);

            return new PagedResult<LabRejectionReasonResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        // =================================================================
        // Menambah
        // =================================================================

        public async Task<LabRejectionReasonResponse> CreateAsync(
            CreateLabRejectionReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var reasonCode = NormalizeCode(request.ReasonCode);

            if (string.IsNullOrEmpty(reasonCode))
                throw new ArgumentException("Kode alasan wajib diisi.");

            var reasonName = Normalize(request.ReasonName);

            if (string.IsNullOrEmpty(reasonName))
                throw new ArgumentException("Nama alasan wajib diisi.");

            // VAL-36. Diperiksa di sini supaya pemanggil menerima pesan yang berarti, sementara
            // index unik database tetap menjadi penjaga terakhir bila dua permintaan datang
            // bersamaan.
            await EnsureCodeIsFreeAsync(reasonCode, cancellationToken);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstLabRejectionReason
            {
                ReasonCode = reasonCode,
                ReasonName = reasonName,
                Description = Normalize(request.Description),
                SortOrder = request.SortOrder,

                // AC-26. Kedua penanda ini sengaja tidak diambil dari permintaan; alasan baru
                // selalu lahir tanpa akibat biaya sampai administrator sistem menyetelnya.
                IsInternalHospitalError = false,
                RequiresNote = false,

                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.MstLabRejectionReasons.Add(entity);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabRejectionReason.Create",
                "Menambah alasan penolakan sampel.",
                new
                {
                    entity.Id,
                    entity.ReasonCode,
                    entity.ReasonName,
                    entity.SortOrder,
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Mengubah nama, keterangan, dan urutan
        // =================================================================

        public async Task<LabRejectionReasonResponse> UpdateAsync(
            Guid id,
            UpdateLabRejectionReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            // VAL-37 diperiksa sebelum baris apa pun disentuh, supaya permintaan yang
            // menyelipkan penanda terkunci ditolak seluruhnya dan tidak menyisakan sebagian
            // perubahan yang terlanjur tersimpan.
            if (request.IsInternalHospitalError.HasValue || request.RequiresNote.HasValue)
            {
                throw new LabRejectionReasonForbiddenException(
                    "Kedua penanda ini hanya dapat diubah administrator sistem, karena menentukan siapa menanggung biaya pengambilan ulang.");
            }

            var entity = await FindAsync(id, cancellationToken);

            var reasonName = Normalize(request.ReasonName);

            if (string.IsNullOrEmpty(reasonName))
                throw new ArgumentException("Nama alasan wajib diisi.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ReasonName = reasonName;
            entity.Description = Normalize(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabRejectionReason.Update",
                "Mengubah alasan penolakan sampel.",
                new
                {
                    entity.Id,
                    entity.ReasonCode,
                    entity.ReasonName,
                    entity.SortOrder,
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Mengaktifkan dan menonaktifkan
        // =================================================================

        public async Task<LabRejectionReasonResponse> SetActivationAsync(
            Guid id,
            SetLabRejectionReasonActivationRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, cancellationToken);

            if (entity.IsActive == request.IsActive)
                return Map(entity);

            if (!request.IsActive)
            {
                // VAL-38. Tabel alasan penolakan yang kosong membuat petugas tidak dapat
                // menolak sampel sama sekali — dan sampel yang tidak layak pun akhirnya tetap
                // diperiksa. Karena itu alasan aktif terakhir tidak boleh dinonaktifkan.
                var otherActive = await _dbContext.MstLabRejectionReasons
                    .AsNoTracking()
                    .AnyAsync(x => !x.IsDelete && x.IsActive && x.Id != entity.Id, cancellationToken);

                if (!otherActive)
                {
                    throw new LabRejectionReasonValidationException(
                        "Sekurang-kurangnya satu alasan penolakan harus tetap aktif.");
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabRejectionReason.SetActivation",
                request.IsActive
                    ? "Mengaktifkan alasan penolakan sampel."
                    : "Menonaktifkan alasan penolakan sampel.",
                new
                {
                    entity.Id,
                    entity.ReasonCode,
                    entity.IsActive,
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Menyetel penanda yang terkunci
        // =================================================================

        /// <summary>
        /// Menyetel penanda kesalahan internal dan penanda wajib catatan.
        ///
        /// Wewenangnya ditegakkan <c>[AccessPermission("LabRejectionReason", "SystemFlag")]</c>
        /// pada controller. Yang dikerjakan di sini adalah pencatatannya: setiap penyetelan
        /// menuliskan nilai lama, nilai baru, pelaku, dan alasannya ke log
        /// (<c>QBE-LOG-001</c>), karena keputusan inilah yang kelak dipakai Billing untuk
        /// menentukan siapa menanggung biaya pengambilan ulang.
        /// </summary>
        public async Task<LabRejectionReasonResponse> SetSystemFlagsAsync(
            Guid id,
            SetLabRejectionReasonSystemFlagsRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, cancellationToken);

            var previousInternalError = entity.IsInternalHospitalError;
            var previousRequiresNote = entity.RequiresNote;

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsInternalHospitalError = request.IsInternalHospitalError;
            entity.RequiresNote = request.RequiresNote;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabRejectionReason.SetSystemFlags",
                "Menyetel penanda sistem pada alasan penolakan sampel.",
                new
                {
                    entity.Id,
                    entity.ReasonCode,
                    PreviousIsInternalHospitalError = previousInternalError,
                    entity.IsInternalHospitalError,
                    PreviousRequiresNote = previousRequiresNote,
                    entity.RequiresNote,
                    ChangeReason = Normalize(request.ChangeReason),
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private async Task<MstLabRejectionReason> FindAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.MstLabRejectionReasons
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Alasan penolakan tidak ditemukan.");

            return entity;
        }

        private async Task EnsureCodeIsFreeAsync(
            string reasonCode,
            CancellationToken cancellationToken)
        {
            var duplicate = await _dbContext.MstLabRejectionReasons
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.ReasonCode == reasonCode, cancellationToken);

            if (duplicate)
            {
                throw new LabRejectionReasonConflictException(
                    "Kode alasan ini sudah dipakai data lain, jadi tidak bisa disimpan.");
            }
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                // Penjaga terakhir VAL-36 bila dua permintaan datang bersamaan dan keduanya
                // lolos pemeriksaan awal.
                throw new LabRejectionReasonConflictException(
                    "Kode alasan ini sudah dipakai data lain, jadi tidak bisa disimpan.");
            }
        }

        private static bool IsUniqueViolation(DbUpdateException exception)
        {
            return exception.InnerException?.GetType().Name == "PostgresException" &&
                   exception.InnerException.Message.Contains("duplicate key value", StringComparison.OrdinalIgnoreCase);
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// Menormalkan kode alasan menjadi huruf kapital tanpa spasi tepi, mengikuti bentuk
        /// baris baseline yang sudah terisi seperti <c>IDENTITY_MISMATCH</c> dan <c>OTHER</c>.
        /// Tanpa normalisasi ini, "other" dan "OTHER" akan lolos sebagai dua kode berbeda.
        /// </summary>
        private static string NormalizeCode(string? value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private static LabRejectionReasonResponse Map(MstLabRejectionReason x) =>
            new()
            {
                Id = x.Id,
                ReasonCode = x.ReasonCode,
                ReasonName = x.ReasonName,
                Description = x.Description,
                IsInternalHospitalError = x.IsInternalHospitalError,
                RequiresNote = x.RequiresNote,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            };
    }

    /// <summary>Pelanggaran aturan isi alasan penolakan. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabRejectionReasonValidationException(string message) : Exception(message);

    /// <summary>Bentrokan dengan baris yang sudah ada. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabRejectionReasonConflictException(string message) : Exception(message);

    /// <summary>Tindakan di luar kewenangan pemanggil. Dipetakan menjadi <c>403</c>.</summary>
    public sealed class LabRejectionReasonForbiddenException(string message) : Exception(message);
}
