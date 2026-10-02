using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Pengelolaan alat pencitraan — <c>RAD-DEC-001</c> butir 14 dan <c>RAD-DEC-002</c>.
    ///
    /// Data induk yang sederhana, dengan satu penjaga yang tidak sederhana:
    /// <b>alat yang masih dipakai aturan keselamatan berlaku tidak dapat dinonaktifkan.</b>
    ///
    /// Alasannya bukan kerapian data. Menonaktifkan alat meninggalkan aturan keselamatan yang
    /// menggantung pada alat yang sudah tidak ada, dan pemeriksaan yang terlanjur berjalan
    /// pada alat itu kehilangan jejak kebijakan yang menilainya. Urutan yang benar dibalik:
    /// hentikan aturannya lebih dulu — lewat pengesahan penanggung jawab klinis — baru alatnya
    /// dipensiunkan.
    ///
    /// Penghapusan memakai soft delete. Alat yang pernah dipakai memeriksa pasien tidak boleh
    /// hilang dari jejak, walaupun sudah lama tidak dipakai.
    /// </summary>
    public class RadModalityService
    {
        private const string LogCategory = "HealthServices.RadiologyManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public RadModalityService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        /* ================================================================ *
         * Pembacaan
         * ================================================================ */

        public RadModalityFilterMetadataResponse GetFilterMetadata() => new()
        {
            SortOptions =
            [
                new() { Value = "sortOrder", Label = "Urutan tampilan" },
                new() { Value = "modalityCode", Label = "Kode alat" },
                new() { Value = "modalityName", Label = "Nama alat" },
                new() { Value = "createDateTime", Label = "Tanggal didaftarkan" },
            ],

            SortDirections = ["asc", "desc"],
            PageSizeOptions = [10, 25, 50, 100],

            QueryParameters =
            [
                new()
                {
                    Name = "search",
                    Type = "string",
                    Required = "No",
                    Description = "Dicari pada kode, nama, dan keterangan alat.",
                    Example = "CT",
                },
                new()
                {
                    Name = "isActive",
                    Type = "bool",
                    Required = "No",
                    Description = "Menyaring alat yang masih dipakai saja, atau yang sudah dipensiunkan saja.",
                },
                new()
                {
                    Name = "usesIonisingRadiation",
                    Type = "bool",
                    Required = "No",
                    Description = "Menyaring alat yang memakai radiasi pengion.",
                },
                new()
                {
                    Name = "supportsContrast",
                    Type = "bool",
                    Required = "No",
                    Description = "Menyaring alat yang dapat memakai media kontras.",
                },
                new()
                {
                    Name = "hasActiveSafetyRule",
                    Type = "bool",
                    Required = "No",
                    Description = "Menyaring kesiapan gerbang: bernilai false menampilkan alat yang akan menolak pemeriksaan.",
                },
                new()
                {
                    Name = "sortBy",
                    Type = "string",
                    Required = "No",
                    Description = "Kolom pengurutan. Nilainya diambil dari SortOptions.",
                    Example = "sortOrder",
                },
                new()
                {
                    Name = "sortDirection",
                    Type = "string",
                    Required = "No",
                    Description = "Arah pengurutan, asc atau desc. Bawaannya asc.",
                    Example = "asc",
                },
                new()
                {
                    Name = "pageNumber",
                    Type = "int",
                    Required = "No",
                    Description = "Halaman yang diminta. Bawaannya 1.",
                    Example = "1",
                },
                new()
                {
                    Name = "pageSize",
                    Type = "int",
                    Required = "No",
                    Description = "Jumlah baris per halaman. Bawaannya 25, paling banyak 100.",
                    Example = "25",
                },
            ],
        };

        public async Task<RadModalitySummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var tercakup = AlatSudahTercakupQuery(now);

            var rekap = await _dbContext.MstRadModalities
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Aktif = g.Count(x => x.IsActive),
                    Nonaktif = g.Count(x => !x.IsActive),
                    Pengion = g.Count(x => x.UsesIonisingRadiation),
                    Kontras = g.Count(x => x.SupportsContrast),
                })
                .FirstOrDefaultAsync(cancellationToken);

            var belumSiap = await _dbContext.MstRadModalities
                .AsNoTracking()
                .CountAsync(
                    x => !x.IsDelete && x.IsActive && !tercakup.Contains(x.Id),
                    cancellationToken);

            return new RadModalitySummaryResponse
            {
                TotalAlat = rekap?.Total ?? 0,
                Aktif = rekap?.Aktif ?? 0,
                Nonaktif = rekap?.Nonaktif ?? 0,
                MemakaiRadiasiPengion = rekap?.Pengion ?? 0,
                MendukungKontras = rekap?.Kontras ?? 0,
                BelumPunyaAturanKeselamatan = belumSiap,
            };
        }

        public async Task<PagedResult<RadModalityResponse>> GetPagedAsync(
            RadModalityPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var tercakup = AlatSudahTercakupQuery(now);
            var (pageNumber, pageSize) = NormalkanHalaman(query.PageNumber, query.PageSize);

            var sumber = _dbContext.MstRadModalities
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var pencarian = query.Search?.Trim();

            if (!string.IsNullOrWhiteSpace(pencarian))
            {
                sumber = sumber.Where(x =>
                    x.ModalityCode.Contains(pencarian) ||
                    x.ModalityName.Contains(pencarian) ||
                    (x.Description != null && x.Description.Contains(pencarian)));
            }

            if (query.IsActive.HasValue)
            {
                sumber = sumber.Where(x => x.IsActive == query.IsActive.Value);
            }

            if (query.UsesIonisingRadiation.HasValue)
            {
                sumber = sumber.Where(
                    x => x.UsesIonisingRadiation == query.UsesIonisingRadiation.Value);
            }

            if (query.SupportsContrast.HasValue)
            {
                sumber = sumber.Where(x => x.SupportsContrast == query.SupportsContrast.Value);
            }

            if (query.HasActiveSafetyRule.HasValue)
            {
                sumber = query.HasActiveSafetyRule.Value
                    ? sumber.Where(x => tercakup.Contains(x.Id))
                    : sumber.Where(x => !tercakup.Contains(x.Id));
            }

            var totalData = await sumber.CountAsync(cancellationToken);

            var items = await Urutkan(sumber, query.SortBy, query.SortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RadModalityResponse
                {
                    Id = x.Id,
                    ModalityCode = x.ModalityCode,
                    ModalityName = x.ModalityName,
                    Description = x.Description,
                    UsesIonisingRadiation = x.UsesIonisingRadiation,
                    SupportsContrast = x.SupportsContrast,
                    HasActiveSafetyRule = tercakup.Contains(x.Id),
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder,
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RadModalityResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items,
            };
        }

        /// <summary>
        /// Feed ringan untuk dropdown. Hanya alat yang masih dipakai, kecuali diminta
        /// sebaliknya.
        /// </summary>
        public async Task<List<RadModalityOptionResponse>> GetOptionsAsync(
            string? search = null,
            bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var tercakup = AlatSudahTercakupQuery(now);

            var sumber = _dbContext.MstRadModalities
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                sumber = sumber.Where(x => x.IsActive);
            }

            var pencarian = search?.Trim();

            if (!string.IsNullOrWhiteSpace(pencarian))
            {
                sumber = sumber.Where(x =>
                    x.ModalityCode.Contains(pencarian) ||
                    x.ModalityName.Contains(pencarian));
            }

            return await sumber
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ModalityName)
                .Select(x => new RadModalityOptionResponse
                {
                    Id = x.Id,
                    ModalityCode = x.ModalityCode,
                    ModalityName = x.ModalityName,
                    UsesIonisingRadiation = x.UsesIonisingRadiation,
                    SupportsContrast = x.SupportsContrast,
                    HasActiveSafetyRule = tercakup.Contains(x.Id),
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<RadOperationResult<RadModalityDetailResponse>> GetByIdAsync(
            Guid modalityId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var tercakup = AlatSudahTercakupQuery(now);

            var alat = await _dbContext.MstRadModalities
                .AsNoTracking()
                .Where(x => x.Id == modalityId && !x.IsDelete)
                .Select(x => new RadModalityDetailResponse
                {
                    Id = x.Id,
                    ModalityCode = x.ModalityCode,
                    ModalityName = x.ModalityName,
                    Description = x.Description,
                    UsesIonisingRadiation = x.UsesIonisingRadiation,
                    SupportsContrast = x.SupportsContrast,
                    HasActiveSafetyRule = tercakup.Contains(x.Id),
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder,
                    CreateBy = x.CreateBy,
                    CreateDateTime = x.CreateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    ActiveSafetyRuleCount = x.SafetyRules.Count(r =>
                        !r.IsDelete &&
                        r.RuleStatus == RadSafetyRuleStatus.Active &&
                        r.EffectiveFrom <= now &&
                        (r.EffectiveTo == null || r.EffectiveTo > now)),
                })
                .FirstOrDefaultAsync(cancellationToken);

            return alat == null ? TidakDitemukan<RadModalityDetailResponse>() : RadOperationResult<RadModalityDetailResponse>.Success(alat);
        }

        /* ================================================================ *
         * Perubahan
         * ================================================================ */

        public async Task<RadOperationResult<RadModalityDetailResponse>> CreateAsync(
            CreateRadModalityRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var kode = request.ModalityCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var nama = request.ModalityName?.Trim() ?? string.Empty;

            var isian = ValidasiIsian<RadModalityDetailResponse>(kode, nama);

            if (isian != null)
            {
                return isian;
            }

            if (await KodeSudahDipakaiAsync(kode, null, cancellationToken))
            {
                return KodeKembar<RadModalityDetailResponse>();
            }

            var alat = new MstRadModality
            {
                ModalityCode = kode,
                ModalityName = nama,
                Description = request.Description?.Trim(),
                UsesIonisingRadiation = request.UsesIonisingRadiation,
                SupportsContrast = request.SupportsContrast,
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            _dbContext.MstRadModalities.Add(alat);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir: index unik pada database menolak kode kembar yang lolos
                // pemeriksaan di atas karena dua permintaan berjalan hampir bersamaan.
                return KodeKembar<RadModalityDetailResponse>();
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "RadModality.Create",
                "Alat pencitraan radiologi didaftarkan.",
                new { ModalityId = alat.Id, alat.ModalityCode });

            return await GetByIdAsync(alat.Id, cancellationToken);
        }

        public async Task<RadOperationResult<RadModalityDetailResponse>> UpdateAsync(
            Guid modalityId,
            UpdateRadModalityRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var alat = await LoadAsync(modalityId, cancellationToken);

            if (alat == null)
            {
                return TidakDitemukan<RadModalityDetailResponse>();
            }

            var kode = request.ModalityCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var nama = request.ModalityName?.Trim() ?? string.Empty;

            var isian = ValidasiIsian<RadModalityDetailResponse>(kode, nama);

            if (isian != null)
            {
                return isian;
            }

            if (await KodeSudahDipakaiAsync(kode, modalityId, cancellationToken))
            {
                return KodeKembar<RadModalityDetailResponse>();
            }

            alat.ModalityCode = kode;
            alat.ModalityName = nama;
            alat.Description = request.Description?.Trim();
            alat.UsesIonisingRadiation = request.UsesIonisingRadiation;
            alat.SupportsContrast = request.SupportsContrast;
            alat.SortOrder = request.SortOrder;
            alat.UpdateBy = actorUserId;
            alat.UpdateDateTime = now;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return KodeKembar<RadModalityDetailResponse>();
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "RadModality.Update",
                "Data alat pencitraan radiologi diperbarui.",
                new { ModalityId = alat.Id, alat.ModalityCode });

            return await GetByIdAsync(alat.Id, cancellationToken);
        }

        /// <summary>
        /// Menyalakan atau mematikan sebuah alat.
        ///
        /// <b>Mematikan alat yang masih dipakai aturan keselamatan berlaku ditolak.</b> Aturan
        /// yang menggantung pada alat yang sudah dipensiunkan tidak menjaga siapa pun, dan
        /// menghapusnya diam-diam berarti mencabut kebijakan klinis tanpa sepengetahuan
        /// penanggung jawabnya.
        /// </summary>
        public async Task<RadOperationResult<RadModalityDetailResponse>> SetStatusAsync(
            Guid modalityId,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var alat = await LoadAsync(modalityId, cancellationToken);

            if (alat == null)
            {
                return TidakDitemukan<RadModalityDetailResponse>();
            }

            if (!isActive && await MasihDipakaiAturanBerlakuAsync(modalityId, now, cancellationToken))
            {
                return MasihDipakai<RadModalityDetailResponse>();
            }

            alat.IsActive = isActive;
            alat.UpdateBy = actorUserId;
            alat.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "RadModality.SetStatus",
                isActive
                    ? "Alat pencitraan radiologi diaktifkan kembali."
                    : "Alat pencitraan radiologi dinonaktifkan.",
                new { ModalityId = alat.Id, alat.ModalityCode, alat.IsActive });

            return await GetByIdAsync(alat.Id, cancellationToken);
        }

        /// <summary>
        /// Menandai alat terhapus tanpa menghapusnya secara fisik.
        ///
        /// Penjaganya sama dengan penonaktifan: alat yang masih dipakai aturan berlaku tidak
        /// dapat dihapus.
        /// </summary>
        public async Task<RadOperationResult<bool>> DeleteAsync(
            Guid modalityId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var alat = await LoadAsync(modalityId, cancellationToken);

            if (alat == null)
            {
                return TidakDitemukan<bool>();
            }

            if (await MasihDipakaiAturanBerlakuAsync(modalityId, now, cancellationToken))
            {
                return MasihDipakai<bool>();
            }

            alat.IsDelete = true;
            alat.IsActive = false;
            alat.DeleteBy = actorUserId;
            alat.DeleteDateTime = now;
            alat.UpdateBy = actorUserId;
            alat.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "RadModality.Delete",
                "Alat pencitraan radiologi ditandai terhapus.",
                new { ModalityId = alat.Id, alat.ModalityCode });

            return RadOperationResult<bool>.Success(true);
        }

        /* ================================================================ *
         * Penjaga bersama
         * ================================================================ */

        /// <summary>
        /// Id alat yang punya sedikitnya satu aturan keselamatan benar-benar berlaku saat ini.
        ///
        /// Ditulis sebagai query, bukan sebagai fungsi C# yang menerima satu alat. Fungsi biasa
        /// tidak dapat diterjemahkan Entity Framework menjadi SQL, dan memaksakannya membuat
        /// seluruh tabel ditarik ke memori lebih dulu — pada tabel alat hal itu tidak terasa,
        /// tetapi kebiasaannya menular ke tempat yang jauh lebih besar.
        ///
        /// Penyaringnya sama persis dengan yang dipakai gerbang keselamatan sejak
        /// <c>BE-RAD-06</c>. Kalau keduanya berbeda, layar akan menyatakan sebuah alat sudah
        /// siap sementara gerbangnya tetap menolak.
        /// </summary>
        private IQueryable<Guid> AlatSudahTercakupQuery(DateTime now) =>
            _dbContext.MstRadModalitySafetyRules
                .AsNoTracking()
                .Where(r =>
                    !r.IsDelete &&
                    r.RuleStatus == RadSafetyRuleStatus.Active &&
                    r.EffectiveFrom <= now &&
                    (r.EffectiveTo == null || r.EffectiveTo > now))
                .Select(r => r.ModalityId);

        private Task<bool> MasihDipakaiAturanBerlakuAsync(
            Guid modalityId,
            DateTime now,
            CancellationToken cancellationToken) =>
            _dbContext.MstRadModalitySafetyRules
                .AsNoTracking()
                .AnyAsync(
                    r => !r.IsDelete &&
                         r.ModalityId == modalityId &&
                         r.RuleStatus == RadSafetyRuleStatus.Active &&
                         r.EffectiveFrom <= now &&
                         (r.EffectiveTo == null || r.EffectiveTo > now),
                    cancellationToken);

        private Task<bool> KodeSudahDipakaiAsync(
            string kode,
            Guid? kecualiId,
            CancellationToken cancellationToken) =>
            _dbContext.MstRadModalities
                .AsNoTracking()
                .AnyAsync(
                    x => !x.IsDelete &&
                         x.ModalityCode == kode &&
                         (kecualiId == null || x.Id != kecualiId.Value),
                    cancellationToken);

        private Task<MstRadModality?> LoadAsync(
            Guid modalityId,
            CancellationToken cancellationToken) =>
            _dbContext.MstRadModalities
                .FirstOrDefaultAsync(x => x.Id == modalityId && !x.IsDelete, cancellationToken);

        private static RadOperationResult<T>? ValidasiIsian<T>(string kode, string nama)
        {
            if (string.IsNullOrWhiteSpace(kode))
            {
                return RadOperationResult<T>.Validation(
                    RadErrorCodes.ValidationFailed, "Kode alat wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(nama))
            {
                return RadOperationResult<T>.Validation(
                    RadErrorCodes.ValidationFailed, "Nama alat wajib diisi.");
            }

            return null;
        }

        private static RadOperationResult<T> TidakDitemukan<T>() =>
            RadOperationResult<T>.NotFound(
                RadErrorCodes.ModalityNotFound,
                "Alat pencitraan tidak ditemukan atau sudah dihapus.");

        private static RadOperationResult<T> KodeKembar<T>() =>
            RadOperationResult<T>.Conflict(
                RadErrorCodes.ModalityCodeAlreadyUsed,
                "Kode alat sudah dipakai. Gunakan kode lain.");

        private static RadOperationResult<T> MasihDipakai<T>() =>
            RadOperationResult<T>.Conflict(
                RadErrorCodes.ModalityStillInUse,
                "Alat ini masih dipakai aturan keselamatan yang berlaku. Nonaktifkan " +
                "aturannya lebih dulu.");

        private static (int PageNumber, int PageSize) NormalkanHalaman(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }

        private static IOrderedQueryable<MstRadModality> Urutkan(
            IQueryable<MstRadModality> query,
            string? sortBy,
            string? sortDirection)
        {
            var menurun = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "modalitycode" => menurun
                    ? query.OrderByDescending(x => x.ModalityCode)
                    : query.OrderBy(x => x.ModalityCode),

                "modalityname" => menurun
                    ? query.OrderByDescending(x => x.ModalityName)
                    : query.OrderBy(x => x.ModalityName),

                "createdatetime" => menurun
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                _ => menurun
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ModalityName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ModalityName),
            };
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            if (!Guid.TryParse(value, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Identitas petugas tidak dapat ditentukan dari sesi yang sedang berjalan. " +
                    "Tindakan radiologi tidak dijalankan.");
            }

            return userId;
        }
    }
}
