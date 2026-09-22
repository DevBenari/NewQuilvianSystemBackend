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
    /// Pemetaan pemeriksaan katalog yang memakai set bakteri (<c>LAB-DEC-125</c>,
    /// <c>BE-LAB-62</c>).
    ///
    /// <b>Selama tabel ini kosong, nol pemeriksaan terpetakan</b> — dan itu keadaan yang pasti
    /// terjadi lebih dulu. Lihat <see cref="UsesSusceptibilitySetAsync"/> untuk perilaku yang
    /// dipilih pada keadaan itu, beserta alasannya.
    /// </summary>
    public class LabProcedureMicrobiologyProfileService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabProcedureMicrobiologyProfileService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Apakah pemeriksaan ini memakai set bakteri.
        ///
        /// <b>Pemeriksaan yang BELUM diprofilkan dianggap MEMAKAI set bakteri.</b> Ini pilihan
        /// sadar, dan alasannya sama dengan jalur jatuh <c>LAB-DEC-111</c>: tabel pemetaan
        /// dimulai kosong, dan menolak seluruh pengisian hasil sampai kepala instalasi selesai
        /// memetakan berarti <b>halaman hasil Mikrobiologi lahir dalam keadaan tidak dapat
        /// dipakai</b>.
        ///
        /// Yang ditolak hanyalah pemeriksaan yang <b>sudah diprofilkan secara tegas</b> sebagai
        /// tidak memakai set bakteri (<c>VAL-118</c>).
        /// </summary>
        public async Task<bool> UsesSusceptibilitySetAsync(
            Guid procedureId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _dbContext.LabProcedureMicrobiologyProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ProcedureId == procedureId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            return profile is null || profile.UsesSusceptibilitySet;
        }

        public async Task<PagedResult<LabProcedureMicrobiologyProfileResponse>> GetListAsync(
            LabProcedureMicrobiologyProfilePagedQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

            var source = _dbContext.LabProcedureMicrobiologyProfiles
                .AsNoTracking()
                .Include(x => x.Procedure)
                .Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            if (query.UsesSusceptibilitySet.HasValue)
                source = source.Where(x => x.UsesSusceptibilitySet == query.UsesSusceptibilitySet.Value);

            var search = query.Search?.Trim();

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                // Pencocokan pada KODE dan NAMA sekaligus. Pencocokan hanya pada nama pernah
                // menggolongkan "Imunohistokimia" ke Histologi — pelajaran BE-LAB-52.
                source = source.Where(x =>
                    EF.Functions.ILike(x.Procedure!.ProcedureCode, pattern) ||
                    EF.Functions.ILike(x.Procedure!.ProcedureName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.Procedure!.ProcedureName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => MapToResponse(x))
                .ToListAsync(cancellationToken);

            return new PagedResult<LabProcedureMicrobiologyProfileResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<LabProcedureMicrobiologyProfileResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabProcedureMicrobiologyProfiles
                .AsNoTracking()
                .Include(x => x.Procedure)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Profil Mikrobiologi tidak ditemukan.");

            return MapToResponse(entity);
        }

        public async Task<LabProcedureMicrobiologyProfileResponse> CreateAsync(
            CreateLabProcedureMicrobiologyProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var procedure = await _dbContext.MstProcedures
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ProcedureId && !x.IsDelete, cancellationToken)
                ?? throw new ArgumentException("Pemeriksaan yang dipilih tidak ditemukan.");

            if (!procedure.IsLaboratory)
                throw new ArgumentException("Tindakan yang dipilih bukan pemeriksaan laboratorium.");

            var duplicate = await _dbContext.LabProcedureMicrobiologyProfiles
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.ProcedureId == request.ProcedureId, cancellationToken);

            if (duplicate)
            {
                throw new LabProcedureMicrobiologyProfileConflictException(
                    "Pemeriksaan ini sudah punya profil Mikrobiologi.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new LabProcedureMicrobiologyProfile
            {
                ProcedureId = request.ProcedureId,
                UsesSusceptibilitySet = request.UsesSusceptibilitySet,
                DefaultCultureType = request.DefaultCultureType,
                DefaultSusceptibilityMethod = request.DefaultSusceptibilityMethod,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabProcedureMicrobiologyProfiles.Add(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabProcedureMicrobiologyProfile.Create",
                "Memetakan profil Mikrobiologi pemeriksaan katalog.",
                new { entity.Id, entity.ProcedureId, entity.UsesSusceptibilitySet });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<LabProcedureMicrobiologyProfileResponse> UpdateAsync(
            Guid id,
            UpdateLabProcedureMicrobiologyProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabProcedureMicrobiologyProfiles
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Profil Mikrobiologi tidak ditemukan.");

            var sebelum = new { entity.UsesSusceptibilitySet, entity.IsActive };

            entity.UsesSusceptibilitySet = request.UsesSusceptibilitySet;
            entity.DefaultCultureType = request.DefaultCultureType;
            entity.DefaultSusceptibilityMethod = request.DefaultSusceptibilityMethod;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabProcedureMicrobiologyProfile.Update",
                "Mengubah profil Mikrobiologi pemeriksaan katalog.",
                new
                {
                    entity.Id,
                    Sebelum = sebelum,
                    Sesudah = new { entity.UsesSusceptibilitySet, entity.IsActive }
                });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        /// <summary>Menonaktifkan pemetaan. Nol baris dihapus.</summary>
        public async Task DeactivateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabProcedureMicrobiologyProfiles
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Profil Mikrobiologi tidak ditemukan.");

            if (!entity.IsActive)
                return;

            entity.IsActive = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabProcedureMicrobiologyProfile.Deactivate",
                "Menonaktifkan profil Mikrobiologi pemeriksaan katalog.",
                new { entity.Id, entity.ProcedureId });
        }

        /// <summary>
        /// Ringkasan pemetaan (<c>GET /summary</c>, baseline master data).
        ///
        /// <b>Kedua pencacah terakhir sengaja dipisah.</b> "Belum diprofilkan" dan "tegas
        /// ditandai tidak memakai set bakteri" adalah dua keadaan yang berbeda akibatnya:
        /// yang pertama tetap menerima isolat, yang kedua ditolak <c>VAL-118</c>. Satu angka
        /// gabungan akan menyembunyikan perbedaan itu.
        /// </summary>
        public async Task<LabProcedureMicrobiologyProfileSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.LabProcedureMicrobiologyProfiles
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            return new LabProcedureMicrobiologyProfileSummaryResponse
            {
                TotalProfile = await source.CountAsync(cancellationToken),
                ActiveProfile = await source.CountAsync(x => x.IsActive, cancellationToken),
                InactiveProfile = await source.CountAsync(x => !x.IsActive, cancellationToken),

                UsesSusceptibilitySet = await source
                    .CountAsync(x => x.IsActive && x.UsesSusceptibilitySet, cancellationToken),

                WithoutSusceptibilitySet = await source
                    .CountAsync(x => x.IsActive && !x.UsesSusceptibilitySet, cancellationToken)
            };
        }

        /// <summary>Feed ringan untuk dropdown (<c>GET /options</c>). Hanya baris aktif secara bawaan.</summary>
        public async Task<PagedResult<LabProcedureMicrobiologyProfileOptionResponse>> GetOptionsAsync(
            string? search = null,
            bool onlyActive = true,
            int pageNumber = 1,
            int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var halaman = pageNumber < 1 ? 1 : pageNumber;
            var ukuran = pageSize is < 1 or > 100 ? 25 : pageSize;

            var source = _dbContext.LabProcedureMicrobiologyProfiles
                .AsNoTracking()
                .Include(x => x.Procedure)
                .Where(x => !x.IsDelete);

            if (onlyActive)
                source = source.Where(x => x.IsActive);

            var kata = search?.Trim();

            if (!string.IsNullOrEmpty(kata))
            {
                source = source.Where(x =>
                    x.Procedure != null &&
                    (EF.Functions.ILike(x.Procedure.ProcedureName, $"%{kata}%") ||
                     EF.Functions.ILike(x.Procedure.ProcedureCode, $"%{kata}%")));
            }

            var total = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.Procedure!.ProcedureName)
                .Skip((halaman - 1) * ukuran)
                .Take(ukuran)
                .Select(x => new LabProcedureMicrobiologyProfileOptionResponse
                {
                    Id = x.Id,
                    ProcedureId = x.ProcedureId,
                    UsesSusceptibilitySet = x.UsesSusceptibilitySet,
                    Label = x.Procedure != null ? x.Procedure.ProcedureName : "-"
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabProcedureMicrobiologyProfileOptionResponse>
            {
                PageNumber = halaman,
                PageSize = ukuran,
                TotalData = total,
                TotalPage = ukuran == 0 ? 0 : (int)Math.Ceiling(total / (double)ukuran),
                Items = items
            };
        }

        /// <summary>
        /// Mengubah status aktif saja (<c>PATCH /{id}/status</c>).
        ///
        /// <b>Menonaktifkan profil BUKAN sama dengan menandainya tidak memakai set bakteri.</b>
        /// Profil nonaktif membuat pemeriksaannya kembali ke perilaku bawaan — tetap menerima
        /// isolat; sedangkan <c>UsesSusceptibilitySet = false</c> menolaknya. Karena itu kedua
        /// ruas itu punya jalur yang berbeda.
        /// </summary>
        public async Task<LabProcedureMicrobiologyProfileResponse> SetStatusAsync(
            Guid id,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabProcedureMicrobiologyProfiles
                .Include(x => x.Procedure)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Profil Mikrobiologi tidak ditemukan.");

            if (entity.IsActive != isActive)
            {
                entity.IsActive = isActive;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = GetCurrentUserId();

                await _dbContext.SaveChangesAsync(cancellationToken);

                await _loggerService.AuditAsync(
                    LogCategory,
                    "LabProcedureMicrobiologyProfile.SetStatus",
                    isActive ? "Mengaktifkan profil Mikrobiologi." : "Menonaktifkan profil Mikrobiologi.",
                    new { entity.Id, entity.ProcedureId, isActive });
            }

            return MapToResponse(entity);
        }

        private static LabProcedureMicrobiologyProfileResponse MapToResponse(LabProcedureMicrobiologyProfile entity)
            => new()
            {
                Id = entity.Id,
                ProcedureId = entity.ProcedureId,
                ProcedureCode = entity.Procedure?.ProcedureCode,
                ProcedureName = entity.Procedure?.ProcedureName,
                UsesSusceptibilitySet = entity.UsesSusceptibilitySet,
                DefaultCultureType = entity.DefaultCultureType,
                DefaultSusceptibilityMethod = entity.DefaultSusceptibilityMethod,
                IsActive = entity.IsActive
            };

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>Pemeriksaan sudah punya profil. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabProcedureMicrobiologyProfileConflictException(string message) : Exception(message);
}
