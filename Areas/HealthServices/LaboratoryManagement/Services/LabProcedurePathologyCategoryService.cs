using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Penggolongan jenis pemeriksaan katalog ke golongan Patologi Anatomi
    /// (<c>LAB-DEC-087</c>, <c>r25</c> bagian 20.3).
    ///
    /// <b>Tabel yang diurus berkas ini menentukan halaman hasil Patologi Anatomi berisi atau
    /// kosong.</b> Formulir laporan dibentuk dari parameter yang berlaku bagi golongan pesanan,
    /// dan golongan itu ditemukan lewat baris di sini. Selama tabelnya kosong, nol pemeriksaan
    /// punya golongan — <c>INV-39</c> melarang sistem menebak — sehingga formulirnya kosong
    /// sama sekali. Itulah keadaan hari pertama, dan <c>VAL-100</c> ada untuk membuatnya
    /// terbaca sebagai penjelasan, bukan sebagai kerusakan.
    ///
    /// <b>Pengisian awalnya bukan pekerjaan programmer</b>, melainkan kepala instalasi bersama
    /// <c>DR-LAB-003</c>. Yang disediakan di sini hanyalah alat bantunya:
    /// <see cref="GetSuggestionsAsync"/> <i>mengusulkan</i>, dan manusia yang menyimpannya.
    /// </summary>
    public class LabProcedurePathologyCategoryService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        /// <summary>
        /// Keenam kata kunci <c>LAB-EVD-003</c>, beserta kode golongan yang diusulkannya.
        ///
        /// <b>Ini satu-satunya tempat keenam kata kunci itu hidup</b>, dan umurnya memang
        /// pendek: sesudah pengisian awal selesai, jalur usulan boleh tidak dipakai lagi
        /// (<c>LAB-DEC-087</c>). Menaruhnya di tempat lain — apalagi pada jalur yang menyimpan —
        /// akan membuat tebakan kata kunci hidup lebih lama daripada kegunaannya.
        ///
        /// Urutannya mengikat: yang lebih khusus diperiksa lebih dulu, supaya
        /// <c>NON GINEKOLOGI</c> tidak keburu direbut kecocokan yang lebih longgar.
        /// </summary>
        private static readonly (string Keyword, string CategoryCode)[] SuggestionKeywords =
        {
            ("NON GINEKOLOGI", "SITO_NONGIN"),
            ("PAPSMEAR", "SITO_GIN"),
            ("LBC", "SITO_GIN"),
            ("HPV", "SITO_GIN"),
            ("IHK", "IHK"),
            ("HISTO", "HISTO")
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabProcedurePathologyCategoryService(
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

        public async Task<PagedResult<LabProcedurePathologyCategoryResponse>> GetListAsync(
            LabProcedurePathologyCategoryPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var source =
                from pemetaan in _dbContext.LabProcedurePathologyCategories.AsNoTracking()
                join procedure in _dbContext.MstProcedures.AsNoTracking()
                    on pemetaan.ProcedureId equals procedure.Id
                join category in _dbContext.LabPathologyCategories.AsNoTracking()
                    on pemetaan.LabPathologyCategoryId equals category.Id
                where !pemetaan.IsDelete
                select new LabProcedurePathologyCategoryResponse
                {
                    Id = pemetaan.Id,
                    ProcedureId = procedure.Id,
                    ProcedureCode = procedure.ProcedureCode,
                    ProcedureName = procedure.ProcedureName,
                    LabPathologyCategoryId = category.Id,
                    CategoryCode = category.CategoryCode,
                    CategoryName = category.CategoryName
                };

            if (query.LabPathologyCategoryId.HasValue)
            {
                source = source.Where(x =>
                    x.LabPathologyCategoryId == query.LabPathologyCategoryId.Value);
            }

            var search = LabPathologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.ProcedureCode, pattern) ||
                    EF.Functions.ILike(x.ProcedureName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.CategoryName)
                .ThenBy(x => x.ProcedureName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<LabProcedurePathologyCategoryResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Usulan penggolongan bagi jenis pemeriksaan Patologi Anatomi yang <b>belum</b>
        /// digolongkan.
        ///
        /// <b>Jalur ini nol menyimpan apa pun.</b> Ia mencocokkan kata kunci pada nama
        /// pemeriksaan lalu menyerahkan hasilnya untuk diperiksa manusia; penyimpanannya lewat
        /// <see cref="CreateAsync"/>. Pemisahan itu bukan kerapian melainkan inti
        /// <c>LAB-DEC-087</c>: pencocokan kata kunci tidak tahu apa-apa soal patologi, dan
        /// tombol "terapkan semua" akan menghapus satu-satunya pemeriksaan yang menjaganya.
        ///
        /// Pemeriksaan yang nol cocok dengan kata kunci mana pun <b>tetap dikembalikan</b>
        /// dengan usulan kosong. Menyembunyikannya akan membuat justru pemeriksaan yang paling
        /// butuh perhatian manusia menjadi yang paling tidak terlihat.
        /// </summary>
        public async Task<PagedResult<LabProcedurePathologyCategorySuggestionResponse>> GetSuggestionsAsync(
            LabProcedurePathologyCategorySuggestionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 50 : query.PageSize;

            var mappedProcedureIds = _dbContext.LabProcedurePathologyCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.ProcedureId);

            var source = _dbContext.MstProcedures
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsLaboratory &&
                    x.LabDiscipline == LabDiscipline.AnatomicalPathology &&
                    !mappedProcedureIds.Contains(x.Id));

            var search = LabPathologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.ProcedureCode, pattern) ||
                    EF.Functions.ILike(x.ProcedureName, pattern));
            }

            var candidates = await source
                .OrderBy(x => x.ProcedureName)
                .Select(x => new { x.Id, x.ProcedureCode, x.ProcedureName })
                .ToListAsync(cancellationToken);

            var categories = await _dbContext.LabPathologyCategories
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .ToListAsync(cancellationToken);

            var categoryByCode = categories.ToDictionary(
                x => x.CategoryCode,
                StringComparer.OrdinalIgnoreCase);

            // Pencocokan dikerjakan di memori, bukan di basis data, dan itu disengaja: bentuk
            // pencocokannya menoleransi spasi (lihat Matches), dan menuliskannya sebagai SQL
            // berarti menanam tebakan kata kunci ke dalam query yang umurnya jauh lebih panjang
            // daripada kegunaannya. Jumlah pemeriksaan Patologi Anatomi pada satu katalog rumah
            // sakit berkisar puluhan, sehingga ongkosnya nol berarti.
            var suggestions = candidates
                .Select(procedure =>
                {
                    var match = FindKeyword($"{procedure.ProcedureCode} {procedure.ProcedureName}");

                    LabPathologyCategory? category = null;

                    if (match != null)
                        categoryByCode.TryGetValue(match.Value.CategoryCode, out category);

                    return new LabProcedurePathologyCategorySuggestionResponse
                    {
                        ProcedureId = procedure.Id,
                        ProcedureCode = procedure.ProcedureCode,
                        ProcedureName = procedure.ProcedureName,
                        SuggestedCategoryId = category?.Id,
                        SuggestedCategoryCode = category?.CategoryCode,
                        SuggestedCategoryName = category?.CategoryName,

                        // Kata kuncinya tetap ditampilkan walaupun golongannya tidak ditemukan.
                        // Justru pasangan itu — ada kecocokan, nol golongan — yang memberi tahu
                        // kepala instalasi bahwa golongannya belum dibuat atau sudah dinonaktifkan.
                        MatchedKeyword = match?.Keyword
                    };
                })
                .ToList();

            var totalData = suggestions.Count;

            var items = suggestions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<LabProcedurePathologyCategorySuggestionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        // =================================================================
        // Menambah dan memindahkan
        // =================================================================

        public async Task<LabProcedurePathologyCategoryResponse> CreateAsync(
            CreateLabProcedurePathologyCategoryRequest request,
            CancellationToken cancellationToken = default)
        {
            var procedure = await _dbContext.MstProcedures
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ProcedureId && !x.IsDelete, cancellationToken);

            if (procedure == null)
                throw new KeyNotFoundException("Jenis pemeriksaan tidak ditemukan.");

            // Penggolongan Patologi Anatomi hanya berlaku bagi pemeriksaan Patologi Anatomi.
            // Tanpa penjagaan ini, pemeriksaan Patologi Klinik dapat memperoleh golongan lalu
            // memunculkan formulir narasi pada layar yang bukan miliknya.
            if (!procedure.IsLaboratory || procedure.LabDiscipline != LabDiscipline.AnatomicalPathology)
            {
                throw new LabPathologyMasterDataValidationException(
                    "Jenis pemeriksaan ini bukan pemeriksaan Patologi Anatomi.");
            }

            await EnsureCategoryIsUsableAsync(request.LabPathologyCategoryId, cancellationToken);

            var duplicate = await _dbContext.LabProcedurePathologyCategories
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.ProcedureId == request.ProcedureId, cancellationToken);

            if (duplicate)
            {
                throw new LabPathologyMasterDataConflictException(
                    "Jenis pemeriksaan ini sudah digolongkan. Ubah golongannya, bukan menambah baru.");
            }

            var actorUserId = GetCurrentUserId();

            var entity = new LabProcedurePathologyCategory
            {
                ProcedureId = request.ProcedureId,
                LabPathologyCategoryId = request.LabPathologyCategoryId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.LabProcedurePathologyCategories.Add(entity);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabProcedurePathologyCategory.Create",
                "Menggolongkan jenis pemeriksaan Patologi Anatomi.",
                new
                {
                    entity.Id,
                    entity.ProcedureId,
                    entity.LabPathologyCategoryId,
                    ActorUserId = actorUserId
                });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<LabProcedurePathologyCategoryResponse> UpdateAsync(
            Guid id,
            UpdateLabProcedurePathologyCategoryRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabProcedurePathologyCategories
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Penggolongan jenis pemeriksaan tidak ditemukan.");

            await EnsureCategoryIsUsableAsync(request.LabPathologyCategoryId, cancellationToken);

            if (entity.LabPathologyCategoryId == request.LabPathologyCategoryId)
                return await GetByIdAsync(entity.Id, cancellationToken);

            var actorUserId = GetCurrentUserId();
            var previousCategoryId = entity.LabPathologyCategoryId;

            entity.LabPathologyCategoryId = request.LabPathologyCategoryId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabProcedurePathologyCategory.Update",
                "Memindahkan jenis pemeriksaan ke golongan Patologi Anatomi lain.",
                new
                {
                    entity.Id,
                    entity.ProcedureId,
                    GolonganSebelumnya = previousCategoryId,
                    GolonganSesudahnya = entity.LabPathologyCategoryId,
                    ActorUserId = actorUserId
                });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private async Task<LabProcedurePathologyCategoryResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await (
                from pemetaan in _dbContext.LabProcedurePathologyCategories.AsNoTracking()
                join procedure in _dbContext.MstProcedures.AsNoTracking()
                    on pemetaan.ProcedureId equals procedure.Id
                join category in _dbContext.LabPathologyCategories.AsNoTracking()
                    on pemetaan.LabPathologyCategoryId equals category.Id
                where pemetaan.Id == id && !pemetaan.IsDelete
                select new LabProcedurePathologyCategoryResponse
                {
                    Id = pemetaan.Id,
                    ProcedureId = procedure.Id,
                    ProcedureCode = procedure.ProcedureCode,
                    ProcedureName = procedure.ProcedureName,
                    LabPathologyCategoryId = category.Id,
                    CategoryCode = category.CategoryCode,
                    CategoryName = category.CategoryName
                }).FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                throw new KeyNotFoundException("Penggolongan jenis pemeriksaan tidak ditemukan.");

            return result;
        }

        private async Task EnsureCategoryIsUsableAsync(
            Guid categoryId,
            CancellationToken cancellationToken)
        {
            var category = await _dbContext.LabPathologyCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == categoryId && !x.IsDelete, cancellationToken);

            if (category == null)
                throw new KeyNotFoundException("Golongan Patologi Anatomi tidak ditemukan.");

            // Golongan nonaktif nol boleh dipakai pada penggolongan baru (VAL-99). Penggolongan
            // lama yang menunjuk golongan yang belakangan dinonaktifkan tetap terbaca.
            if (!category.IsActive)
            {
                throw new LabPathologyMasterDataValidationException(
                    "Golongan ini sudah tidak dipakai lagi. Pilih dari daftar yang tersedia.");
            }
        }

        /// <summary>
        /// Mencari kata kunci pertama yang cocok pada <b>kode dan nama</b> pemeriksaan.
        ///
        /// <b>Kodenya ikut dicari, dan itu bukan kelengkapan melainkan perbaikan cacat.</b>
        /// Mencocokkan hanya pada nama membuat <c>Imunohistokimia ER</c> tertangkap kata kunci
        /// <c>HISTO</c> — sebab kata "Imuno<b>histo</b>kimia" memuatnya — lalu diusulkan ke
        /// golongan Histologi, yang salah. Kodenya, <c>LAB-IHK-ER</c>, memuat <c>IHK</c> dan
        /// diperiksa lebih dulu, sehingga usulannya benar. Cacat ini ditemukan uji `BE-LAB-52`
        /// terhadap katalog sungguhan, bukan diperkirakan.
        ///
        /// Pembandingnya mengabaikan besar kecil huruf <b>dan spasi</b>, sehingga
        /// <c>PAP SMEAR</c> pada katalog tetap tertangkap kata kunci <c>PAPSMEAR</c>. Tanpa itu,
        /// kata kunci yang ditulis menyatu pada artifact akan melewatkan justru bentuk yang
        /// paling lazim dipakai katalog rumah sakit — dan usulan yang melewatkan kasus terlazim
        /// nol menghemat pekerjaan siapa pun.
        /// </summary>
        private static (string Keyword, string CategoryCode)? FindKeyword(string? codeAndName)
        {
            if (string.IsNullOrWhiteSpace(codeAndName))
                return null;

            var haystack = Squash(codeAndName);

            foreach (var candidate in SuggestionKeywords)
            {
                if (haystack.Contains(Squash(candidate.Keyword), StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }

            return null;
        }

        private static string Squash(string value) =>
            new(value.Where(c => !char.IsWhiteSpace(c)).ToArray());

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (LabPathologyMasterDataText.IsUniqueViolation(exception))
            {
                // Penjaga terakhir index unik parsial ProcedureId bila dua permintaan datang
                // bersamaan dan keduanya lolos pemeriksaan awal.
                throw new LabPathologyMasterDataConflictException(
                    "Jenis pemeriksaan ini sudah digolongkan. Ubah golongannya, bukan menambah baru.");
            }
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
