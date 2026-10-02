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
    /// Pengelolaan data induk ruas isian laporan Patologi Anatomi (<c>LAB-DEC-086</c>,
    /// <c>r25</c> bagian 20.3).
    ///
    /// <b>Nol penghapusan, dan alasannya lebih keras daripada pada data induk lain.</b> Pada
    /// jenis specimen, menghapus baris berarti kehilangan penggolongan sebuah wadah. Di sini,
    /// menghapus parameter berarti menghapus <b>isi laporan diagnostik pasien</b> — nilai yang
    /// tersimpan menunjuk parameter, dan parameter yang lenyap membuat nilainya kehilangan
    /// nama. Parameter karena itu dinonaktifkan, dan nilai lama tetap terbaca utuh
    /// (<c>INV-37</c>).
    ///
    /// Penonaktifan berjalan lewat <see cref="UpdateAsync"/>, bukan jalur tersendiri: kontrak
    /// <c>r25</c> bagian 20.3 menyediakan tepat empat jalur bagi resource ini.
    /// </summary>
    public class LabPathologyParameterService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabPathologyParameterService(
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

        public async Task<PagedResult<LabPathologyParameterResponse>> GetListAsync(
            LabPathologyParameterPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var source = _dbContext.LabPathologyParameters
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            var search = LabPathologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.ParameterCode, pattern) ||
                    EF.Functions.ILike(x.ParameterName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ParameterName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabPathologyParameterResponse
                {
                    Id = x.Id,
                    ParameterCode = x.ParameterCode,
                    ParameterName = x.ParameterName,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabPathologyParameterResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Daftar pilihan saat menyusun keberlakuan. Hanya parameter <b>aktif</b>: keberlakuan
        /// baru nol boleh menunjuk parameter yang sudah ditarik dari peredaran (<c>VAL-99</c>).
        /// </summary>
        public async Task<PagedResult<LabPathologyParameterOptionResponse>> GetOptionsAsync(
            LabPathologyParameterOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 50 : query.PageSize;

            var source = _dbContext.LabPathologyParameters
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            var search = LabPathologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.ParameterCode, pattern) ||
                    EF.Functions.ILike(x.ParameterName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ParameterName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabPathologyParameterOptionResponse
                {
                    Id = x.Id,
                    ParameterCode = x.ParameterCode,
                    ParameterName = x.ParameterName,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabPathologyParameterOptionResponse>
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

        public async Task<LabPathologyParameterResponse> CreateAsync(
            CreateLabPathologyParameterRequest request,
            CancellationToken cancellationToken = default)
        {
            var parameterCode = LabPathologyMasterDataText.NormalizeCode(request.ParameterCode);

            if (string.IsNullOrEmpty(parameterCode))
                throw new ArgumentException("Kode parameter wajib diisi.");

            var parameterName = LabPathologyMasterDataText.Normalize(request.ParameterName);

            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentException("Label parameter wajib diisi.");

            // VAL-101. Diperiksa di sini supaya pemanggil menerima pesan yang berarti, sementara
            // index unik parsial tetap menjadi penjaga terakhir bila dua permintaan datang
            // bersamaan.
            await EnsureCodeIsFreeAsync(parameterCode, cancellationToken);

            var actorUserId = GetCurrentUserId();

            var entity = new LabPathologyParameter
            {
                ParameterCode = parameterCode,
                ParameterName = parameterName,
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.LabPathologyParameters.Add(entity);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyParameter.Create",
                "Menambah parameter Patologi Anatomi.",
                new
                {
                    entity.Id,
                    entity.ParameterCode,
                    entity.ParameterName,
                    entity.SortOrder,
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Mengubah
        // =================================================================

        public async Task<LabPathologyParameterResponse> UpdateAsync(
            Guid id,
            UpdateLabPathologyParameterRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, cancellationToken);

            var parameterName = LabPathologyMasterDataText.Normalize(request.ParameterName);

            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentException("Label parameter wajib diisi.");

            var actorUserId = GetCurrentUserId();
            var activationChanged = entity.IsActive != request.IsActive;

            entity.ParameterName = parameterName;
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyParameter.Update",
                activationChanged && !request.IsActive
                    ? "Menonaktifkan parameter Patologi Anatomi."
                    : "Mengubah parameter Patologi Anatomi.",
                new
                {
                    entity.Id,
                    entity.ParameterCode,
                    entity.ParameterName,
                    entity.SortOrder,
                    entity.IsActive,
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Permukaan baseline yang dilengkapi 2026-09-23 — `r32`, `BE-LAB-66`
        // =================================================================

        /// <summary>
        /// Detail satu parameter (<c>GET /{id}</c>).
        ///
        /// <b>Ketiadaannya adalah kelas kesalahan yang sudah pernah dibayar modul ini.</b>
        /// Sesudah <c>FE-LAB-03</c>, formulir ubah yang dibuka lewat tautan langsung atau sesudah
        /// halaman disegarkan nol punya jalur memuat barisnya, dan kegagalannya diam — layar
        /// hanya tampak kosong. <c>r6</c> menutupnya untuk alasan penolakan, <c>r31</c> untuk
        /// kedua data induk Mikrobiologi; ini menutupnya untuk Patologi Anatomi.
        /// </summary>
        public async Task<LabPathologyParameterResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabPathologyParameters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Parameter Patologi Anatomi tidak ditemukan.");

            return Map(entity);
        }

        /// <summary>
        /// Ringkasan data induk parameter (<c>GET /summary</c>).
        ///
        /// <c>UsedInCategory</c> dihitung dari keberlakuan yang <b>hidup</b>, bukan dari seluruh
        /// barisnya. Selisihnya terhadap total adalah parameter yang terdaftar tetapi nol akan
        /// pernah muncul pada satu formulir pun — keadaan yang nol terlihat dari daftar mana pun,
        /// sebab daftar parameter tidak tahu apa-apa soal golongan yang memakainya.
        /// </summary>
        public async Task<LabPathologyParameterSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.LabPathologyParameters.AsNoTracking().Where(x => !x.IsDelete);

            var total = await source.CountAsync(cancellationToken);
            var aktif = await source.CountAsync(x => x.IsActive, cancellationToken);

            // Penyaring pada parameternya sendiri BUKAN hiasan. Tanpa itu, keberlakuan yang
            // menunjuk parameter terhapus tetap terhitung "terpakai", dan ringkasannya dapat
            // melaporkan angka terpakai yang LEBIH BESAR daripada totalnya. Bentuk kesalahan yang
            // sama sudah pernah ditemukan pada `withBreakpoint` (`BE-LAB-65` bagian 6ad.5).
            var idHidup = source.Select(x => x.Id);

            var terpakai = await _dbContext.LabPathologyParameterCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete && idHidup.Contains(x.LabPathologyParameterId))
                .Select(x => x.LabPathologyParameterId)
                .Distinct()
                .CountAsync(cancellationToken);

            return new LabPathologyParameterSummaryResponse
            {
                TotalParameter = total,
                ActiveParameter = aktif,
                InactiveParameter = total - aktif,
                UsedInCategory = terpakai
            };
        }

        /// <summary>
        /// Membalik penanda aktif satu parameter (<c>PATCH /{id}/status</c>).
        ///
        /// <b>Ini bukan penghapusan, dan bukan pula jalan pintas <c>PUT</c>.</b> <c>PUT</c>
        /// menuntut seluruh ruas dikirim; menonaktifkan satu baris dari halaman daftar karena itu
        /// akan memaksa layar memuat detailnya lebih dulu hanya untuk mengirim balik ruas yang
        /// nol berubah — dan setiap ruas yang ikut terkirim adalah ruas yang dapat tertimpa nilai
        /// basi.
        ///
        /// <b>Keberlakuan yang sudah tersusun nol ikut dicabut.</b> Parameter yang dinonaktifkan
        /// berhenti ditawarkan saat menyusun keberlakuan baru (<c>VAL-99</c>), tetapi golongan
        /// yang terlanjur memakainya tetap utuh — mencabutnya diam-diam akan mengubah bentuk
        /// formulir laporan tanpa satu pun manusia memutuskannya.
        /// </summary>
        public async Task<LabPathologyParameterResponse> SetStatusAsync(
            Guid id,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, cancellationToken);

            var actorUserId = GetCurrentUserId();

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyParameter.SetStatus",
                isActive
                    ? "Mengaktifkan parameter Patologi Anatomi."
                    : "Menonaktifkan parameter Patologi Anatomi.",
                new
                {
                    entity.Id,
                    entity.ParameterCode,
                    entity.ParameterName,
                    entity.IsActive,
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private async Task<LabPathologyParameter> FindAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.LabPathologyParameters
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Parameter Patologi Anatomi tidak ditemukan.");

            return entity;
        }

        private async Task EnsureCodeIsFreeAsync(
            string parameterCode,
            CancellationToken cancellationToken)
        {
            var duplicate = await _dbContext.LabPathologyParameters
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.ParameterCode == parameterCode, cancellationToken);

            if (duplicate)
            {
                throw new LabPathologyMasterDataConflictException(
                    "Kode ini sudah dipakai data lain, jadi tidak bisa disimpan.");
            }
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (LabPathologyMasterDataText.IsUniqueViolation(exception))
            {
                // Penjaga terakhir VAL-101 bila dua permintaan datang bersamaan dan keduanya
                // lolos pemeriksaan awal.
                throw new LabPathologyMasterDataConflictException(
                    "Kode ini sudah dipakai data lain, jadi tidak bisa disimpan.");
            }
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private static LabPathologyParameterResponse Map(LabPathologyParameter x) =>
            new()
            {
                Id = x.Id,
                ParameterCode = x.ParameterCode,
                ParameterName = x.ParameterName,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            };
    }

    /// <summary>
    /// Pembantu teks dan pengenalan bentrokan yang dipakai bersama ketiga service data induk
    /// Patologi Anatomi. Dikumpulkan di satu tempat supaya normalisasi kodenya tidak berbeda
    /// antar-resource — perbedaan kecil di sini membuat <c>HISTO</c> dan <c>histo</c> lolos
    /// sebagai dua kode berbeda pada salah satu jalur saja, dan bocornya diam-diam.
    /// </summary>
    internal static class LabPathologyMasterDataText
    {
        public static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public static string NormalizeCode(string? value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

        public static bool IsUniqueViolation(DbUpdateException exception) =>
            exception.InnerException?.GetType().Name == "PostgresException" &&
            exception.InnerException.Message.Contains("duplicate key value", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Pelanggaran aturan isi data induk Patologi Anatomi. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabPathologyMasterDataValidationException(string message) : Exception(message);

    /// <summary>Bentrokan dengan baris yang sudah ada. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabPathologyMasterDataConflictException(string message) : Exception(message);
}
