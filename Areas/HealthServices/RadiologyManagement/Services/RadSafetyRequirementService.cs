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
    /// Pengelolaan butir keselamatan — <c>RAD-DEC-005</c>.
    ///
    /// <b>Ini kosakata, bukan kebijakan.</b> Menambah butir di sini tidak membuatnya berlaku
    /// bagi seorang pasien pun; ia hanya menjadi pilihan yang tersedia ketika admin menyusun
    /// aturan keselamatan untuk sebuah alat. Yang mengikat adalah aturan yang sudah disahkan
    /// penanggung jawab klinis.
    ///
    /// Penjaganya sejajar dengan alat pencitraan: <b>butir yang sedang dipakai aturan
    /// keselamatan berlaku tidak dapat dinonaktifkan maupun dihapus.</b> Butir yang hilang
    /// sementara aturannya masih berjalan meninggalkan pertanyaan tanpa rumusan — dan
    /// pertanyaan keselamatan yang kehilangan rumusannya lebih buruk daripada pertanyaan yang
    /// tidak pernah ada, karena ia tetap terlihat dijawab.
    /// </summary>
    public class RadSafetyRequirementService
    {
        private const string LogCategory = "HealthServices.RadiologyManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public RadSafetyRequirementService(
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

        public async Task<RadSafetyRequirementFilterMetadataResponse> GetFilterMetadataAsync(
            CancellationToken cancellationToken = default)
        {
            // Kategori adalah teks bebas, sehingga daftarnya hanya dapat diturunkan dari isinya.
            // Menuliskannya sebagai daftar tetap akan basi begitu ada kelompok baru dipakai.
            var kategori = await _dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.Category != null && x.Category != string.Empty)
                .Select(x => x.Category!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            return new RadSafetyRequirementFilterMetadataResponse
            {
                Categories = kategori,

                SortOptions =
                [
                    new() { Value = "sortOrder", Label = "Urutan tampilan" },
                    new() { Value = "requirementCode", Label = "Kode butir" },
                    new() { Value = "requirementName", Label = "Nama butir" },
                    new() { Value = "category", Label = "Kelompok" },
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
                        Description = "Dicari pada kode, nama, dan keterangan butir.",
                        Example = "PREGNANCY",
                    },
                    new()
                    {
                        Name = "category",
                        Type = "string",
                        Required = "No",
                        Description = "Menyaring satu kelompok butir. Nilainya diambil dari Categories.",
                        Example = "Radiation",
                    },
                    new()
                    {
                        Name = "isActive",
                        Type = "bool",
                        Required = "No",
                        Description = "Menyaring butir yang masih dipakai saja, atau yang sudah dipensiunkan saja.",
                    },
                    new()
                    {
                        Name = "requiresNote",
                        Type = "bool",
                        Required = "No",
                        Description = "Menyaring butir yang mewajibkan petugas mengisi catatan.",
                    },
                    new()
                    {
                        Name = "isUsedByActiveRule",
                        Type = "bool",
                        Required = "No",
                        Description = "Menyaring butir yang sedang dipakai aturan keselamatan berlaku.",
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
        }

        public async Task<RadSafetyRequirementSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var dipakai = ButirDipakaiQuery(now);

            var rekap = await _dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Aktif = g.Count(x => x.IsActive),
                    Nonaktif = g.Count(x => !x.IsActive),
                    Bercatatan = g.Count(x => x.RequiresNote),
                })
                .FirstOrDefaultAsync(cancellationToken);

            var terpakai = await _dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete && dipakai.Contains(x.Id), cancellationToken);

            var belumDipakai = await _dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .CountAsync(
                    x => !x.IsDelete && x.IsActive && !dipakai.Contains(x.Id), cancellationToken);

            return new RadSafetyRequirementSummaryResponse
            {
                TotalButir = rekap?.Total ?? 0,
                Aktif = rekap?.Aktif ?? 0,
                Nonaktif = rekap?.Nonaktif ?? 0,
                WajibBercatatan = rekap?.Bercatatan ?? 0,
                DipakaiAturanBerlaku = terpakai,
                BelumDipakai = belumDipakai,
            };
        }

        public async Task<PagedResult<RadSafetyRequirementResponse>> GetPagedAsync(
            RadSafetyRequirementPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var dipakai = ButirDipakaiQuery(now);
            var (pageNumber, pageSize) = NormalkanHalaman(query.PageNumber, query.PageSize);

            var sumber = _dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var pencarian = query.Search?.Trim();

            if (!string.IsNullOrWhiteSpace(pencarian))
            {
                sumber = sumber.Where(x =>
                    x.RequirementCode.Contains(pencarian) ||
                    x.RequirementName.Contains(pencarian) ||
                    (x.Description != null && x.Description.Contains(pencarian)));
            }

            var kategori = query.Category?.Trim();

            if (!string.IsNullOrWhiteSpace(kategori))
            {
                sumber = sumber.Where(x => x.Category == kategori);
            }

            if (query.IsActive.HasValue)
            {
                sumber = sumber.Where(x => x.IsActive == query.IsActive.Value);
            }

            if (query.RequiresNote.HasValue)
            {
                sumber = sumber.Where(x => x.RequiresNote == query.RequiresNote.Value);
            }

            if (query.IsUsedByActiveRule.HasValue)
            {
                sumber = query.IsUsedByActiveRule.Value
                    ? sumber.Where(x => dipakai.Contains(x.Id))
                    : sumber.Where(x => !dipakai.Contains(x.Id));
            }

            var totalData = await sumber.CountAsync(cancellationToken);

            var items = await Urutkan(sumber, query.SortBy, query.SortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RadSafetyRequirementResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    Category = x.Category,
                    Description = x.Description,
                    RequiresNote = x.RequiresNote,
                    SourceNote = x.SourceNote,
                    IsActive = x.IsActive,
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RadSafetyRequirementResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items,
            };
        }

        public async Task<List<RadSafetyRequirementOptionResponse>> GetOptionsAsync(
            string? search = null,
            bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var sumber = _dbContext.MstRadSafetyRequirements
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
                    x.RequirementCode.Contains(pencarian) ||
                    x.RequirementName.Contains(pencarian));
            }

            return await sumber
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.RequirementName)
                .Select(x => new RadSafetyRequirementOptionResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    Category = x.Category,
                    RequiresNote = x.RequiresNote,
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<RadOperationResult<RadSafetyRequirementDetailResponse>> GetByIdAsync(
            Guid requirementId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var butir = await _dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .Where(x => x.Id == requirementId && !x.IsDelete)
                .Select(x => new RadSafetyRequirementDetailResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    Category = x.Category,
                    Description = x.Description,
                    RequiresNote = x.RequiresNote,
                    SourceNote = x.SourceNote,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder,
                    CreateBy = x.CreateBy,
                    CreateDateTime = x.CreateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    ActiveRuleCount = _dbContext.MstRadModalitySafetyRules.Count(r =>
                        !r.IsDelete &&
                        r.SafetyRequirementId == x.Id &&
                        r.RuleStatus == RadSafetyRuleStatus.Active &&
                        r.EffectiveFrom <= now &&
                        (r.EffectiveTo == null || r.EffectiveTo > now)),
                })
                .FirstOrDefaultAsync(cancellationToken);

            return butir == null
                ? TidakDitemukan<RadSafetyRequirementDetailResponse>()
                : RadOperationResult<RadSafetyRequirementDetailResponse>.Success(butir);
        }

        /* ================================================================ *
         * Perubahan
         * ================================================================ */

        public async Task<RadOperationResult<RadSafetyRequirementDetailResponse>> CreateAsync(
            CreateRadSafetyRequirementRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var kode = request.RequirementCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var nama = request.RequirementName?.Trim() ?? string.Empty;

            var isian = ValidasiIsian<RadSafetyRequirementDetailResponse>(kode, nama);

            if (isian != null)
            {
                return isian;
            }

            if (await KodeSudahDipakaiAsync(kode, null, cancellationToken))
            {
                return KodeKembar<RadSafetyRequirementDetailResponse>();
            }

            var butir = new MstRadSafetyRequirement
            {
                RequirementCode = kode,
                RequirementName = nama,
                Description = request.Description?.Trim(),
                Category = request.Category?.Trim(),
                RequiresNote = request.RequiresNote,
                SourceNote = request.SourceNote?.Trim(),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            _dbContext.MstRadSafetyRequirements.Add(butir);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return KodeKembar<RadSafetyRequirementDetailResponse>();
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "RadSafetyRequirement.Create",
                "Butir keselamatan radiologi ditambahkan.",
                new { RequirementId = butir.Id, butir.RequirementCode });

            return await GetByIdAsync(butir.Id, cancellationToken);
        }

        public async Task<RadOperationResult<RadSafetyRequirementDetailResponse>> UpdateAsync(
            Guid requirementId,
            UpdateRadSafetyRequirementRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var butir = await LoadAsync(requirementId, cancellationToken);

            if (butir == null)
            {
                return TidakDitemukan<RadSafetyRequirementDetailResponse>();
            }

            var kode = request.RequirementCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var nama = request.RequirementName?.Trim() ?? string.Empty;

            var isian = ValidasiIsian<RadSafetyRequirementDetailResponse>(kode, nama);

            if (isian != null)
            {
                return isian;
            }

            if (await KodeSudahDipakaiAsync(kode, requirementId, cancellationToken))
            {
                return KodeKembar<RadSafetyRequirementDetailResponse>();
            }

            butir.RequirementCode = kode;
            butir.RequirementName = nama;
            butir.Description = request.Description?.Trim();
            butir.Category = request.Category?.Trim();
            butir.RequiresNote = request.RequiresNote;
            butir.SourceNote = request.SourceNote?.Trim();
            butir.SortOrder = request.SortOrder;
            butir.UpdateBy = actorUserId;
            butir.UpdateDateTime = now;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return KodeKembar<RadSafetyRequirementDetailResponse>();
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "RadSafetyRequirement.Update",
                "Butir keselamatan radiologi diperbarui.",
                new { RequirementId = butir.Id, butir.RequirementCode });

            return await GetByIdAsync(butir.Id, cancellationToken);
        }

        /// <summary>
        /// Menyalakan atau mematikan sebuah butir.
        ///
        /// <b>Mematikan butir yang masih dipakai aturan keselamatan berlaku ditolak.</b>
        /// Aturannya wajib dihentikan lebih dulu lewat pengesahan penanggung jawab klinis.
        /// </summary>
        public async Task<RadOperationResult<RadSafetyRequirementDetailResponse>> SetStatusAsync(
            Guid requirementId,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var butir = await LoadAsync(requirementId, cancellationToken);

            if (butir == null)
            {
                return TidakDitemukan<RadSafetyRequirementDetailResponse>();
            }

            if (!isActive && await MasihDipakaiAsync(requirementId, now, cancellationToken))
            {
                return MasihDipakai<RadSafetyRequirementDetailResponse>();
            }

            butir.IsActive = isActive;
            butir.UpdateBy = actorUserId;
            butir.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "RadSafetyRequirement.SetStatus",
                isActive
                    ? "Butir keselamatan radiologi diaktifkan kembali."
                    : "Butir keselamatan radiologi dinonaktifkan.",
                new { RequirementId = butir.Id, butir.RequirementCode, butir.IsActive });

            return await GetByIdAsync(butir.Id, cancellationToken);
        }

        public async Task<RadOperationResult<bool>> DeleteAsync(
            Guid requirementId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var butir = await LoadAsync(requirementId, cancellationToken);

            if (butir == null)
            {
                return TidakDitemukan<bool>();
            }

            if (await MasihDipakaiAsync(requirementId, now, cancellationToken))
            {
                return MasihDipakai<bool>();
            }

            butir.IsDelete = true;
            butir.IsActive = false;
            butir.DeleteBy = actorUserId;
            butir.DeleteDateTime = now;
            butir.UpdateBy = actorUserId;
            butir.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "RadSafetyRequirement.Delete",
                "Butir keselamatan radiologi ditandai terhapus.",
                new { RequirementId = butir.Id, butir.RequirementCode });

            return RadOperationResult<bool>.Success(true);
        }

        /* ================================================================ *
         * Penjaga bersama
         * ================================================================ */

        /// <summary>
        /// Id butir yang sedang dipakai sedikitnya satu aturan keselamatan berlaku.
        ///
        /// Penyaringnya sama persis dengan yang dipakai gerbang keselamatan sejak
        /// <c>BE-RAD-06</c>.
        /// </summary>
        private IQueryable<Guid> ButirDipakaiQuery(DateTime now) =>
            _dbContext.MstRadModalitySafetyRules
                .AsNoTracking()
                .Where(r =>
                    !r.IsDelete &&
                    r.RuleStatus == RadSafetyRuleStatus.Active &&
                    r.EffectiveFrom <= now &&
                    (r.EffectiveTo == null || r.EffectiveTo > now))
                .Select(r => r.SafetyRequirementId);

        private Task<bool> MasihDipakaiAsync(
            Guid requirementId,
            DateTime now,
            CancellationToken cancellationToken) =>
            _dbContext.MstRadModalitySafetyRules
                .AsNoTracking()
                .AnyAsync(
                    r => !r.IsDelete &&
                         r.SafetyRequirementId == requirementId &&
                         r.RuleStatus == RadSafetyRuleStatus.Active &&
                         r.EffectiveFrom <= now &&
                         (r.EffectiveTo == null || r.EffectiveTo > now),
                    cancellationToken);

        private Task<bool> KodeSudahDipakaiAsync(
            string kode,
            Guid? kecualiId,
            CancellationToken cancellationToken) =>
            _dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .AnyAsync(
                    x => !x.IsDelete &&
                         x.RequirementCode == kode &&
                         (kecualiId == null || x.Id != kecualiId.Value),
                    cancellationToken);

        private Task<MstRadSafetyRequirement?> LoadAsync(
            Guid requirementId,
            CancellationToken cancellationToken) =>
            _dbContext.MstRadSafetyRequirements
                .FirstOrDefaultAsync(x => x.Id == requirementId && !x.IsDelete, cancellationToken);

        private static RadOperationResult<T>? ValidasiIsian<T>(string kode, string nama)
        {
            if (string.IsNullOrWhiteSpace(kode))
            {
                return RadOperationResult<T>.Validation(
                    RadErrorCodes.ValidationFailed, "Kode butir keselamatan wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(nama))
            {
                return RadOperationResult<T>.Validation(
                    RadErrorCodes.ValidationFailed, "Nama butir keselamatan wajib diisi.");
            }

            return null;
        }

        private static RadOperationResult<T> TidakDitemukan<T>() =>
            RadOperationResult<T>.NotFound(
                RadErrorCodes.SafetyRequirementNotFound,
                "Butir keselamatan tidak ditemukan atau sudah dihapus.");

        private static RadOperationResult<T> KodeKembar<T>() =>
            RadOperationResult<T>.Conflict(
                RadErrorCodes.SafetyRequirementCodeAlreadyUsed,
                "Kode butir keselamatan sudah dipakai. Gunakan kode lain.");

        private static RadOperationResult<T> MasihDipakai<T>() =>
            RadOperationResult<T>.Conflict(
                RadErrorCodes.SafetyRequirementStillInUse,
                "Butir ini masih dipakai aturan keselamatan yang berlaku. Nonaktifkan " +
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

        private static IOrderedQueryable<MstRadSafetyRequirement> Urutkan(
            IQueryable<MstRadSafetyRequirement> query,
            string? sortBy,
            string? sortDirection)
        {
            var menurun = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "requirementcode" => menurun
                    ? query.OrderByDescending(x => x.RequirementCode)
                    : query.OrderBy(x => x.RequirementCode),

                "requirementname" => menurun
                    ? query.OrderByDescending(x => x.RequirementName)
                    : query.OrderBy(x => x.RequirementName),

                "category" => menurun
                    ? query.OrderByDescending(x => x.Category).ThenBy(x => x.SortOrder)
                    : query.OrderBy(x => x.Category).ThenBy(x => x.SortOrder),

                _ => menurun
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.RequirementName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.RequirementName),
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
