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
    /// Pengelolaan data induk organisme Mikrobiologi (<c>LAB-DEC-084</c>, <c>r24</c> bagian
    /// 19.4).
    ///
    /// <b>Taruhan berkas ini lebih besar daripada tampaknya.</b> Daftar kuman yang ejaannya
    /// beragam membuat pola resistensi rumah sakit salah hitung — dan pola resistensi itulah
    /// yang menjadi dasar pemilihan antibiotik empiris sebelum hasil biakan keluar. Daftar yang
    /// kotor berakhir pada terapi yang salah pilih, bukan sekadar laporan yang jelek.
    ///
    /// <b>Nol penghapusan.</b> Isolat yang sudah tercatat menunjuk ke sini; menghapus barisnya
    /// berarti menghapus temuan pasien. Penonaktifan lewat <c>PUT /{id}</c>, dan <c>INV-31</c>
    /// menjamin isolat lama tetap terbaca.
    /// </summary>
    public class LabOrganismService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabOrganismService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<PagedResult<LabOrganismResponse>> GetListAsync(
            LabOrganismPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var source = _dbContext.LabOrganisms.AsNoTracking().Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            var search = LabMicrobiologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.OrganismCode, pattern) ||
                    EF.Functions.ILike(x.OrganismName, pattern) ||
                    (x.Description != null && EF.Functions.ILike(x.Description, pattern)));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.OrganismName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabOrganismResponse
                {
                    Id = x.Id,
                    OrganismCode = x.OrganismCode,
                    OrganismName = x.OrganismName,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    Description = x.Description
                })
                .ToListAsync(cancellationToken);

            return LabMicrobiologyMasterDataText.Page(pageNumber, pageSize, totalData, items);
        }

        /// <summary>
        /// Daftar pilihan saat mencatat isolat. Hanya organisme <b>aktif</b>, sehingga analis nol
        /// dapat memilih kuman yang sudah ditarik dari daftar (<c>VAL-85</c>).
        /// </summary>
        public async Task<PagedResult<LabOrganismOptionResponse>> GetOptionsAsync(
            LabOrganismOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 50 : query.PageSize;

            var source = _dbContext.LabOrganisms
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            var search = LabMicrobiologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.OrganismCode, pattern) ||
                    EF.Functions.ILike(x.OrganismName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.OrganismName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabOrganismOptionResponse
                {
                    Id = x.Id,
                    OrganismCode = x.OrganismCode,
                    OrganismName = x.OrganismName,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return LabMicrobiologyMasterDataText.Page(pageNumber, pageSize, totalData, items);
        }

        public async Task<LabOrganismResponse> CreateAsync(
            CreateLabOrganismRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var code = LabMicrobiologyMasterDataText.NormalizeCode(request.OrganismCode);

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Kode organisme wajib diisi.");

            var name = LabMicrobiologyMasterDataText.Normalize(request.OrganismName);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Nama organisme wajib diisi.");

            // VAL-91. Diperiksa di sini supaya pemanggil menerima pesan yang berarti, sementara
            // index unik parsial tetap menjadi penjaga terakhir bila dua permintaan datang
            // bersamaan.
            var duplicate = await _dbContext.LabOrganisms
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.OrganismCode == code, cancellationToken);

            if (duplicate)
                throw new LabMicrobiologyMasterDataConflictException(LabMicrobiologyMasterDataText.DuplicateMessage);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new LabOrganism
            {
                OrganismCode = code,
                OrganismName = name,
                Description = LabMicrobiologyMasterDataText.Normalize(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabOrganisms.Add(entity);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrganism.Create",
                "Menambah organisme Mikrobiologi.",
                new { entity.Id, entity.OrganismCode, entity.OrganismName, entity.SortOrder, ActorUserId = actorUserId });

            return Map(entity);
        }

        public async Task<LabOrganismResponse> UpdateAsync(
            Guid id,
            UpdateLabOrganismRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabOrganisms
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Organisme tidak ditemukan.");

            var name = LabMicrobiologyMasterDataText.Normalize(request.OrganismName);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Nama organisme wajib diisi.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var deactivating = entity.IsActive && !request.IsActive;

            entity.OrganismName = name;
            entity.Description = LabMicrobiologyMasterDataText.Normalize(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrganism.Update",
                deactivating ? "Menonaktifkan organisme Mikrobiologi." : "Mengubah organisme Mikrobiologi.",
                new { entity.Id, entity.OrganismCode, entity.OrganismName, entity.IsActive, ActorUserId = actorUserId });

            return Map(entity);
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (LabMicrobiologyMasterDataText.IsUniqueViolation(exception))
            {
                throw new LabMicrobiologyMasterDataConflictException(LabMicrobiologyMasterDataText.DuplicateMessage);
            }
        }

        private Guid GetCurrentUserId() =>
            LabMicrobiologyMasterDataText.ResolveActor(_httpContextAccessor);

        private static LabOrganismResponse Map(LabOrganism x) =>
            new()
            {
                Id = x.Id,
                OrganismCode = x.OrganismCode,
                OrganismName = x.OrganismName,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                Description = x.Description
            };
    }

    /// <summary>
    /// Pengelolaan panel uji kepekaan antibiotik (<c>LAB-DEC-084</c>, <c>r24</c> bagian 19.4).
    ///
    /// <b>Ini panel uji, bukan formularium obat.</b> Kesamaan nama dengan data induk farmasi
    /// menyesatkan, dan penautannya adalah keputusan tersendiri yang belum diambil.
    ///
    /// Bentuknya sengaja kembar dengan <see cref="LabOrganismService"/> — keduanya data induk
    /// berkode, bernama, berurutan, dan nol berpenghapusan. Kembarannya dipertahankan
    /// <b>eksplisit</b> alih-alih diabstraksikan menjadi satu service generik: nama kolomnya
    /// berbeda, dan abstraksi yang memaksanya sama akan membuat query EF nol dapat diterjemahkan.
    /// </summary>
    public class LabAntibioticService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabAntibioticService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<PagedResult<LabAntibioticResponse>> GetListAsync(
            LabAntibioticPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var source = _dbContext.LabAntibiotics.AsNoTracking().Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            var search = LabMicrobiologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.AntibioticCode, pattern) ||
                    EF.Functions.ILike(x.AntibioticName, pattern) ||
                    (x.Description != null && EF.Functions.ILike(x.Description, pattern)));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.AntibioticName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabAntibioticResponse
                {
                    Id = x.Id,
                    AntibioticCode = x.AntibioticCode,
                    AntibioticName = x.AntibioticName,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    Description = x.Description
                })
                .ToListAsync(cancellationToken);

            return LabMicrobiologyMasterDataText.Page(pageNumber, pageSize, totalData, items);
        }

        /// <summary>Daftar pilihan panel uji. Hanya antibiotik <b>aktif</b> (<c>VAL-86</c>).</summary>
        public async Task<PagedResult<LabAntibioticOptionResponse>> GetOptionsAsync(
            LabAntibioticOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 50 : query.PageSize;

            var source = _dbContext.LabAntibiotics
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            var search = LabMicrobiologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.AntibioticCode, pattern) ||
                    EF.Functions.ILike(x.AntibioticName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.AntibioticName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabAntibioticOptionResponse
                {
                    Id = x.Id,
                    AntibioticCode = x.AntibioticCode,
                    AntibioticName = x.AntibioticName,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return LabMicrobiologyMasterDataText.Page(pageNumber, pageSize, totalData, items);
        }

        public async Task<LabAntibioticResponse> CreateAsync(
            CreateLabAntibioticRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var code = LabMicrobiologyMasterDataText.NormalizeCode(request.AntibioticCode);

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Kode antibiotik wajib diisi.");

            var name = LabMicrobiologyMasterDataText.Normalize(request.AntibioticName);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Nama antibiotik wajib diisi.");

            // VAL-91.
            var duplicate = await _dbContext.LabAntibiotics
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.AntibioticCode == code, cancellationToken);

            if (duplicate)
                throw new LabMicrobiologyMasterDataConflictException(LabMicrobiologyMasterDataText.DuplicateMessage);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new LabAntibiotic
            {
                AntibioticCode = code,
                AntibioticName = name,
                Description = LabMicrobiologyMasterDataText.Normalize(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabAntibiotics.Add(entity);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabAntibiotic.Create",
                "Menambah antibiotik pada panel uji.",
                new { entity.Id, entity.AntibioticCode, entity.AntibioticName, entity.SortOrder, ActorUserId = actorUserId });

            return Map(entity);
        }

        public async Task<LabAntibioticResponse> UpdateAsync(
            Guid id,
            UpdateLabAntibioticRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabAntibiotics
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Antibiotik tidak ditemukan.");

            var name = LabMicrobiologyMasterDataText.Normalize(request.AntibioticName);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Nama antibiotik wajib diisi.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var deactivating = entity.IsActive && !request.IsActive;

            entity.AntibioticName = name;
            entity.Description = LabMicrobiologyMasterDataText.Normalize(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabAntibiotic.Update",
                deactivating ? "Menonaktifkan antibiotik pada panel uji." : "Mengubah antibiotik pada panel uji.",
                new { entity.Id, entity.AntibioticCode, entity.AntibioticName, entity.IsActive, ActorUserId = actorUserId });

            return Map(entity);
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (LabMicrobiologyMasterDataText.IsUniqueViolation(exception))
            {
                throw new LabMicrobiologyMasterDataConflictException(LabMicrobiologyMasterDataText.DuplicateMessage);
            }
        }

        private Guid GetCurrentUserId() =>
            LabMicrobiologyMasterDataText.ResolveActor(_httpContextAccessor);

        private static LabAntibioticResponse Map(LabAntibiotic x) =>
            new()
            {
                Id = x.Id,
                AntibioticCode = x.AntibioticCode,
                AntibioticName = x.AntibioticName,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                Description = x.Description
            };
    }

    /// <summary>
    /// Pembantu yang dipakai bersama kedua service data induk Mikrobiologi. Dikumpulkan supaya
    /// normalisasi kode dan kalimat penolakannya <b>tidak berbeda</b> antar-resource — perbedaan
    /// kecil di sini membuat satu jalur menerima apa yang jalur lain tolak, dan bocornya
    /// diam-diam.
    /// </summary>
    internal static class LabMicrobiologyMasterDataText
    {
        /// <summary>Kalimat <c>VAL-91</c>, ditulis satu kali supaya kedua jalur berbunyi sama.</summary>
        public const string DuplicateMessage = "Kode ini sudah dipakai data lain, jadi tidak bisa disimpan.";

        public static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// Menormalkan kode menjadi huruf kapital tanpa spasi tepi, mengikuti bentuk
        /// <c>LabSpecimenType</c>. Tanpa ini, "ecoli" dan "ECOLI" lolos sebagai dua kode berbeda.
        /// </summary>
        public static string NormalizeCode(string? value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

        public static bool IsUniqueViolation(DbUpdateException exception) =>
            exception.InnerException?.GetType().Name == "PostgresException" &&
            exception.InnerException.Message.Contains("duplicate key value", StringComparison.OrdinalIgnoreCase);

        public static PagedResult<T> Page<T>(int pageNumber, int pageSize, int totalData, List<T> items) =>
            new()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

        public static Guid ResolveActor(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>Bentrokan kode data induk Mikrobiologi. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabMicrobiologyMasterDataConflictException(string message) : Exception(message);
}
