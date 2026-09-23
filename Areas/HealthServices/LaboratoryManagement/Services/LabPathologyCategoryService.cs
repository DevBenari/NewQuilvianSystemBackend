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
    /// Pengelolaan golongan pemeriksaan Patologi Anatomi beserta <b>keberlakuan ruasnya</b>
    /// (<c>LAB-DEC-086</c>, <c>r25</c> bagian 20.3).
    ///
    /// <b>Keberlakuan tinggal di sini, bukan menjadi resource tersendiri.</b> Ia adalah cara
    /// sebuah golongan dipakai, bukan benda yang berdiri sendiri — dan mendirikannya sebagai
    /// resource keempat hanya melahirkan satu pintu lagi yang salah satunya pasti lupa dikunci
    /// (<c>rev 7</c> bagian 9.2).
    ///
    /// <b>Yang paling menentukan pada berkas ini adalah <see cref="ReplaceCategoryParametersAsync"/>.</b>
    /// Ia menyusun bentuk formulir laporan diagnostik; salah di sini berarti patolog melihat
    /// ruas yang bukan miliknya, atau kehilangan ruas yang wajib diisinya.
    /// </summary>
    public class LabPathologyCategoryService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabPathologyCategoryService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        // =================================================================
        // Baca — golongan
        // =================================================================

        public async Task<PagedResult<LabPathologyCategoryResponse>> GetListAsync(
            LabPathologyCategoryPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var source = _dbContext.LabPathologyCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            var search = LabPathologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.CategoryCode, pattern) ||
                    EF.Functions.ILike(x.CategoryName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            // Jumlah ruas dihitung dari keberlakuan yang hidup. Nilai 0 sengaja dapat terlihat
            // dari daftar: golongan tanpa ruas menghasilkan formulir kosong, dan tanpa angka ini
            // keadaannya baru ketahuan ketika patolog sudah membuka layar hasil.
            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabPathologyCategoryResponse
                {
                    Id = x.Id,
                    CategoryCode = x.CategoryCode,
                    CategoryName = x.CategoryName,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    ParameterCount = _dbContext.LabPathologyParameterCategories
                        .Count(p => !p.IsDelete && p.LabPathologyCategoryId == x.Id)
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabPathologyCategoryResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<PagedResult<LabPathologyCategoryOptionResponse>> GetOptionsAsync(
            LabPathologyCategoryOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 50 : query.PageSize;

            var source = _dbContext.LabPathologyCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            var search = LabPathologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.CategoryCode, pattern) ||
                    EF.Functions.ILike(x.CategoryName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabPathologyCategoryOptionResponse
                {
                    Id = x.Id,
                    CategoryCode = x.CategoryCode,
                    CategoryName = x.CategoryName,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabPathologyCategoryOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        // =================================================================
        // Menambah dan mengubah golongan
        // =================================================================

        public async Task<LabPathologyCategoryResponse> CreateAsync(
            CreateLabPathologyCategoryRequest request,
            CancellationToken cancellationToken = default)
        {
            var categoryCode = LabPathologyMasterDataText.NormalizeCode(request.CategoryCode);

            if (string.IsNullOrEmpty(categoryCode))
                throw new ArgumentException("Kode golongan wajib diisi.");

            var categoryName = LabPathologyMasterDataText.Normalize(request.CategoryName);

            if (string.IsNullOrEmpty(categoryName))
                throw new ArgumentException("Nama golongan wajib diisi.");

            await EnsureCodeIsFreeAsync(categoryCode, cancellationToken);

            var actorUserId = GetCurrentUserId();

            var entity = new LabPathologyCategory
            {
                CategoryCode = categoryCode,
                CategoryName = categoryName,
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.LabPathologyCategories.Add(entity);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyCategory.Create",
                "Menambah golongan Patologi Anatomi.",
                new
                {
                    entity.Id,
                    entity.CategoryCode,
                    entity.CategoryName,
                    entity.SortOrder,
                    ActorUserId = actorUserId
                });

            return Map(entity, 0);
        }

        public async Task<LabPathologyCategoryResponse> UpdateAsync(
            Guid id,
            UpdateLabPathologyCategoryRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, cancellationToken);

            var categoryName = LabPathologyMasterDataText.Normalize(request.CategoryName);

            if (string.IsNullOrEmpty(categoryName))
                throw new ArgumentException("Nama golongan wajib diisi.");

            var actorUserId = GetCurrentUserId();
            var deactivating = entity.IsActive && !request.IsActive;

            entity.CategoryName = categoryName;
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyCategory.Update",
                deactivating
                    ? "Menonaktifkan golongan Patologi Anatomi."
                    : "Mengubah golongan Patologi Anatomi.",
                new
                {
                    entity.Id,
                    entity.CategoryCode,
                    entity.CategoryName,
                    entity.SortOrder,
                    entity.IsActive,
                    ActorUserId = actorUserId
                });

            var parameterCount = await _dbContext.LabPathologyParameterCategories
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete && x.LabPathologyCategoryId == entity.Id, cancellationToken);

            return Map(entity, parameterCount);
        }

        // =================================================================
        // Keberlakuan ruas
        // =================================================================

        /// <summary>
        /// Ruas apa saja yang berlaku bagi satu golongan, berurut sesuai tampilnya pada formulir.
        ///
        /// Keberlakuan yang menunjuk parameter <b>nonaktif</b> tetap ikut terbawa beserta
        /// penandanya. Menyembunyikannya akan membuat ruas lenyap dari formulir tanpa satu pun
        /// layar yang dapat menjelaskan sebabnya.
        /// </summary>
        public async Task<List<LabPathologyCategoryParameterResponse>> GetCategoryParametersAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            await FindAsync(categoryId, cancellationToken);

            return await _dbContext.LabPathologyParameterCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.LabPathologyCategoryId == categoryId)
                .Join(
                    _dbContext.LabPathologyParameters.AsNoTracking().Where(p => !p.IsDelete),
                    keberlakuan => keberlakuan.LabPathologyParameterId,
                    parameter => parameter.Id,
                    (keberlakuan, parameter) => new LabPathologyCategoryParameterResponse
                    {
                        LabPathologyParameterId = parameter.Id,
                        ParameterCode = parameter.ParameterCode,
                        ParameterName = parameter.ParameterName,
                        SortOrder = parameter.SortOrder,
                        IsParameterActive = parameter.IsActive,
                        IsRequired = keberlakuan.IsRequired
                    })
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ParameterName)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Mengganti seluruh daftar keberlakuan sebuah golongan sekaligus.
        ///
        /// <b>Baris yang hilang dari permintaan ditandai terhapus, bukan dibuang.</b> Index unik
        /// keberlakuan berbentuk parsial justru supaya pasangan yang dicabut hari ini dapat
        /// dipasang kembali besok — mencabut lalu memasang ulang adalah tindakan wajar bagi
        /// kepala instalasi yang sedang menyusun formulir.
        ///
        /// <b>Parameter nonaktif hanya ditolak bila ia BARU.</b> Pasangan yang sudah ada dan
        /// kebetulan menunjuk parameter yang belakangan dinonaktifkan tetap dipertahankan; ia
        /// bagian bentuk formulir yang sudah terpakai laporan lama (<c>INV-37</c>). Yang dicegah
        /// adalah menambahkan ruas mati ke dalam formulir baru (<c>VAL-99</c>).
        /// </summary>
        public async Task<List<LabPathologyCategoryParameterResponse>> ReplaceCategoryParametersAsync(
            Guid categoryId,
            ReplaceLabPathologyCategoryParametersRequest request,
            CancellationToken cancellationToken = default)
        {
            var category = await FindAsync(categoryId, cancellationToken);

            var requested = request.Items ?? new List<LabPathologyCategoryParameterItemRequest>();

            // Satu parameter nol boleh dikirim dua kali dalam satu permintaan. Menerima lalu
            // memakai yang terakhir akan membuat penanda wajib bergantung urutan kiriman, dan
            // urutan kiriman bukan sesuatu yang disadari penyusun formulir.
            var duplicateIds = requested
                .GroupBy(x => x.LabPathologyParameterId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Count > 0)
            {
                throw new LabPathologyMasterDataValidationException(
                    "Ada isian yang sama dikirim dua kali dalam satu permintaan.");
            }

            var requestedIds = requested.Select(x => x.LabPathologyParameterId).ToList();

            var parameters = await _dbContext.LabPathologyParameters
                .AsNoTracking()
                .Where(x => !x.IsDelete && requestedIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var missing = requestedIds.Where(id => parameters.All(p => p.Id != id)).ToList();

            if (missing.Count > 0)
            {
                throw new LabPathologyMasterDataValidationException(
                    "Ada isian yang tidak ditemukan pada daftar parameter Patologi Anatomi.");
            }

            var existing = await _dbContext.LabPathologyParameterCategories
                .Where(x => !x.IsDelete && x.LabPathologyCategoryId == categoryId)
                .ToListAsync(cancellationToken);

            var existingByParameterId = existing.ToDictionary(x => x.LabPathologyParameterId);

            var inactiveNewcomers = parameters
                .Where(p => !p.IsActive && !existingByParameterId.ContainsKey(p.Id))
                .Select(p => p.ParameterName)
                .ToList();

            if (inactiveNewcomers.Count > 0)
            {
                throw new LabPathologyMasterDataValidationException(
                    $"Isian berikut sudah tidak dipakai lagi, jadi tidak bisa ditambahkan: {string.Join(", ", inactiveNewcomers)}.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var added = 0;
            var updated = 0;
            var removed = 0;

            foreach (var item in requested)
            {
                if (existingByParameterId.TryGetValue(item.LabPathologyParameterId, out var row))
                {
                    if (row.IsRequired == item.IsRequired)
                        continue;

                    row.IsRequired = item.IsRequired;
                    row.UpdateDateTime = now;
                    row.UpdateBy = actorUserId;
                    updated++;

                    continue;
                }

                _dbContext.LabPathologyParameterCategories.Add(new LabPathologyParameterCategory
                {
                    LabPathologyParameterId = item.LabPathologyParameterId,
                    LabPathologyCategoryId = categoryId,
                    IsRequired = item.IsRequired,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });

                added++;
            }

            var requestedIdSet = requestedIds.ToHashSet();

            foreach (var row in existing.Where(x => !requestedIdSet.Contains(x.LabPathologyParameterId)))
            {
                row.IsDelete = true;
                row.DeleteDateTime = now;
                row.DeleteBy = actorUserId;
                removed++;
            }

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabPathologyCategory.ReplaceParameters",
                "Menyusun ulang keberlakuan ruas golongan Patologi Anatomi.",
                new
                {
                    CategoryId = category.Id,
                    category.CategoryCode,
                    Ditambahkan = added,
                    Diubah = updated,
                    Dicabut = removed,
                    TotalSesudahnya = requested.Count,
                    ActorUserId = actorUserId
                });

            return await GetCategoryParametersAsync(categoryId, cancellationToken);
        }

        // =================================================================
        // Permukaan baseline yang dilengkapi 2026-09-23 — `r32`, `BE-LAB-66`
        // =================================================================

        /// <summary>
        /// Detail satu golongan beserta jumlah keberlakuannya (<c>GET /{id}</c>).
        ///
        /// <b>Ketiadaannya adalah kelas kesalahan yang sudah pernah dibayar modul ini</b> —
        /// formulir ubah yang dibuka lewat tautan langsung nol punya jalur memuat barisnya, dan
        /// kegagalannya diam. <c>r6</c> menutupnya untuk alasan penolakan; ini menutupnya untuk
        /// golongan Patologi Anatomi.
        ///
        /// <c>ParameterCount</c> ikut dihitung di sini, bukan dibiarkan <c>0</c>: nilai <c>0</c>
        /// pada halaman detail berarti golongan ini menghasilkan formulir kosong, dan itu justru
        /// keterangan yang paling perlu terlihat saat seseorang membuka golongannya.
        /// </summary>
        public async Task<LabPathologyCategoryResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabPathologyCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Golongan Patologi Anatomi tidak ditemukan.");

            var parameterCount = await _dbContext.LabPathologyParameterCategories
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete && x.LabPathologyCategoryId == id, cancellationToken);

            return Map(entity, parameterCount);
        }

        /// <summary>
        /// Ringkasan data induk golongan (<c>GET /summary</c>).
        ///
        /// <b><c>WithParameter</c> yang membuat ringkasan ini berguna.</b> Selisihnya terhadap
        /// total adalah golongan yang nol punya satu pun ruas — dan golongan seperti itu
        /// menghasilkan formulir laporan yang kosong sama sekali. Tanpa angka ini, keadaannya
        /// baru ketahuan ketika patolog sudah membuka layar hasil dan <c>VAL-100</c> menolaknya.
        /// </summary>
        public async Task<LabPathologyCategorySummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.LabPathologyCategories.AsNoTracking().Where(x => !x.IsDelete);

            var total = await source.CountAsync(cancellationToken);
            var aktif = await source.CountAsync(x => x.IsActive, cancellationToken);

            // Penyaring pada golongannya sendiri BUKAN hiasan. Tanpa itu, keberlakuan yang
            // menunjuk golongan terhapus tetap terhitung "berruas", dan ringkasannya dapat
            // melaporkan angka berruas yang LEBIH BESAR daripada totalnya. Bentuk kesalahan yang
            // sama sudah pernah ditemukan pada `withBreakpoint` (`BE-LAB-65` bagian 6ad.5).
            var idHidup = source.Select(x => x.Id);

            var berruas = await _dbContext.LabPathologyParameterCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete && idHidup.Contains(x.LabPathologyCategoryId))
                .Select(x => x.LabPathologyCategoryId)
                .Distinct()
                .CountAsync(cancellationToken);

            return new LabPathologyCategorySummaryResponse
            {
                TotalCategory = total,
                ActiveCategory = aktif,
                InactiveCategory = total - aktif,
                WithParameter = berruas
            };
        }

        /// <summary>
        /// Membalik penanda aktif satu golongan (<c>PATCH /{id}/status</c>).
        ///
        /// <b>Ini bukan penghapusan, dan bukan pula jalan pintas <c>PUT</c>.</b> <c>PUT</c>
        /// menuntut seluruh ruas dikirim; menonaktifkan satu baris dari halaman daftar lewat
        /// <c>PUT</c> memaksa layar memuat detailnya lebih dulu hanya untuk mengirim balik ruas
        /// yang nol berubah.
        ///
        /// <b>Pemetaan jenis pemeriksaan yang menunjuk golongan ini nol ikut dicabut.</b>
        /// Golongan nonaktif berhenti ditawarkan saat menggolongkan pemeriksaan baru, tetapi
        /// pesanan yang terlanjur memakainya tetap punya bentuk formulir — mencabutnya diam-diam
        /// akan mengosongkan laporan yang sedang berjalan.
        /// </summary>
        public async Task<LabPathologyCategoryResponse> SetStatusAsync(
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
                "LabPathologyCategory.SetStatus",
                isActive
                    ? "Mengaktifkan golongan Patologi Anatomi."
                    : "Menonaktifkan golongan Patologi Anatomi.",
                new
                {
                    entity.Id,
                    entity.CategoryCode,
                    entity.CategoryName,
                    entity.IsActive,
                    ActorUserId = actorUserId
                });

            var parameterCount = await _dbContext.LabPathologyParameterCategories
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete && x.LabPathologyCategoryId == id, cancellationToken);

            return Map(entity, parameterCount);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private async Task<LabPathologyCategory> FindAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.LabPathologyCategories
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Golongan Patologi Anatomi tidak ditemukan.");

            return entity;
        }

        private async Task EnsureCodeIsFreeAsync(
            string categoryCode,
            CancellationToken cancellationToken)
        {
            var duplicate = await _dbContext.LabPathologyCategories
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.CategoryCode == categoryCode, cancellationToken);

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

        private static LabPathologyCategoryResponse Map(LabPathologyCategory x, int parameterCount) =>
            new()
            {
                Id = x.Id,
                CategoryCode = x.CategoryCode,
                CategoryName = x.CategoryName,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                ParameterCount = parameterCount
            };
    }
}
