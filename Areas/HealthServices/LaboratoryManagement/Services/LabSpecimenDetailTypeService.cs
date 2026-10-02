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
    /// Data induk Spesifik Specimen (<c>BE-LAB-55</c>, <c>LAB-DEC-098</c>, <c>129</c>..<c>132</c>).
    ///
    /// <b>Hak tulisnya dipegang kepala instalasi.</b> Petugas memakai jalan keluar
    /// <c>Lainnya</c> beserta keterangannya; hanya kepala instalasi yang menaikkannya menjadi
    /// nilai tetap (<c>LAB-DEC-040</c>). Pemeriksaan duplikat Indonesia/Inggris saja tidak
    /// cukup — <c>cairan kista</c>, <c>Cairan Kista</c>, dan <c>c. kista</c> lolos sebagai
    /// tiga nilai.
    /// </summary>
    public class LabSpecimenDetailTypeService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabSpecimenDetailTypeService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<PagedResult<LabSpecimenDetailTypeResponse>> GetListAsync(
            LabSpecimenDetailTypePagedQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

            var source = BuildQuery(query);

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DetailTypeNameEn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<LabSpecimenDetailTypeResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
                Items = [.. items.Select(MapToResponse)]
            };
        }

        /// <summary>
        /// Ringkasan untuk layar pengisian. <b>Hanya baris aktif</b>, dan wajib disaring per
        /// kelompok — daftar berisi 1.601 baris tanpa penyaring bukan bantuan bagi petugas.
        /// </summary>
        public async Task<List<LabSpecimenDetailTypeOptionResponse>> GetOptionsAsync(
            Guid labSpecimenTypeId,
            CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.LabSpecimenDetailTypes
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.LabSpecimenTypeId == labSpecimenTypeId)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DetailTypeNameEn)
                .ToListAsync(cancellationToken);

            return [.. items.Select(x => new LabSpecimenDetailTypeOptionResponse
            {
                Id = x.Id,
                DetailTypeCode = x.DetailTypeCode,
                DisplayName = ResolveDisplayName(x),
                SubTypeName = x.SubTypeName
            })];
        }

        public async Task<LabSpecimenDetailTypeResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabSpecimenDetailTypes
                .AsNoTracking()
                .Include(x => x.LabSpecimenType)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Spesifik Specimen tidak ditemukan.");

            return MapToResponse(entity);
        }

        public async Task<LabSpecimenDetailTypeResponse> CreateAsync(
            CreateLabSpecimenDetailTypeRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var code = Normalize(request.DetailTypeCode)?.ToUpperInvariant();

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Kode Spesifik Specimen wajib diisi.");

            var nameEn = Normalize(request.DetailTypeNameEn);

            if (string.IsNullOrEmpty(nameEn))
                throw new ArgumentException("Nama Inggris Spesifik Specimen wajib diisi.");

            var jenisAda = await _dbContext.LabSpecimenTypes
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.LabSpecimenTypeId && !x.IsDelete && x.IsActive, cancellationToken);

            if (!jenisAda)
                throw new ArgumentException("Jenis specimen yang dipilih tidak berlaku.");

            var nameId = Normalize(request.DetailTypeNameId);

            // RULE-007. Pemeriksaan duplikasi membandingkan KODE, nama Indonesia, DAN nama
            // Inggris sekaligus — mengabaikan besar-kecil huruf dan spasi di tepi.
            var duplikat = await _dbContext.LabSpecimenDetailTypes
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    (x.DetailTypeCode == code ||
                     x.DetailTypeNameEn.ToLower() == nameEn.ToLower() ||
                     (nameId != null && x.DetailTypeNameId != null &&
                      x.DetailTypeNameId.ToLower() == nameId.ToLower())),
                    cancellationToken);

            if (duplikat)
            {
                throw new LabSpecimenDetailTypeConflictException(
                    "Spesifik Specimen dengan kode atau nama tersebut sudah tersedia.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new LabSpecimenDetailType
            {
                LabSpecimenTypeId = request.LabSpecimenTypeId,
                DetailTypeCode = code,
                DetailTypeNameId = nameId,
                DetailTypeNameEn = nameEn,
                SubTypeName = Normalize(request.SubTypeName),
                SnomedCode = Normalize(request.SnomedCode),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabSpecimenDetailTypes.Add(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabSpecimenDetailType.Create",
                "Menambah Spesifik Specimen.",
                new { entity.Id, entity.DetailTypeCode, entity.LabSpecimenTypeId });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<LabSpecimenDetailTypeResponse> UpdateAsync(
            Guid id,
            UpdateLabSpecimenDetailTypeRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabSpecimenDetailTypes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Spesifik Specimen tidak ditemukan.");

            var nameEn = Normalize(request.DetailTypeNameEn);

            if (string.IsNullOrEmpty(nameEn))
                throw new ArgumentException("Nama Inggris Spesifik Specimen wajib diisi.");

            entity.DetailTypeNameId = Normalize(request.DetailTypeNameId);
            entity.DetailTypeNameEn = nameEn;
            entity.SubTypeName = Normalize(request.SubTypeName);
            entity.SnomedCode = Normalize(request.SnomedCode);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabSpecimenDetailType.Update",
                "Mengubah Spesifik Specimen.",
                new { entity.Id, entity.DetailTypeCode, Diterjemahkan = entity.DetailTypeNameId is not null });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        /// <summary>Menonaktifkan. Nol baris dihapus — rincian lama tetap terbaca (<c>INV-31</c>).</summary>
        public async Task DeactivateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabSpecimenDetailTypes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Spesifik Specimen tidak ditemukan.");

            if (!entity.IsActive)
                return;

            entity.IsActive = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabSpecimenDetailType.Deactivate",
                "Menonaktifkan Spesifik Specimen.",
                new { entity.Id, entity.DetailTypeCode });
        }

        /// <summary>Ringkasan kemajuan penerjemahan — berapa yang sudah, berapa yang belum.</summary>
        public async Task<object> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var total = await _dbContext.LabSpecimenDetailTypes
                .AsNoTracking().CountAsync(x => !x.IsDelete, cancellationToken);

            var aktif = await _dbContext.LabSpecimenDetailTypes
                .AsNoTracking().CountAsync(x => !x.IsDelete && x.IsActive, cancellationToken);

            var belumDiterjemahkan = await _dbContext.LabSpecimenDetailTypes
                .AsNoTracking().CountAsync(x => !x.IsDelete && x.DetailTypeNameId == null, cancellationToken);

            return new
            {
                Total = total,
                Aktif = aktif,
                Nonaktif = total - aktif,
                BelumDiterjemahkan = belumDiterjemahkan,
                SudahDiterjemahkan = total - belumDiterjemahkan
            };
        }

        private IQueryable<LabSpecimenDetailType> BuildQuery(LabSpecimenDetailTypePagedQuery query)
        {
            var source = _dbContext.LabSpecimenDetailTypes
                .AsNoTracking()
                .Include(x => x.LabSpecimenType)
                .Where(x => !x.IsDelete);

            if (query.IncludeInactive != true)
                source = source.Where(x => x.IsActive);

            if (query.LabSpecimenTypeId.HasValue)
                source = source.Where(x => x.LabSpecimenTypeId == query.LabSpecimenTypeId.Value);

            if (!string.IsNullOrWhiteSpace(query.SubTypeName))
                source = source.Where(x => x.SubTypeName == query.SubTypeName.Trim());

            if (query.UntranslatedOnly == true)
                source = source.Where(x => x.DetailTypeNameId == null);

            var search = query.Search?.Trim();

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                // RULE-007: pencarian menerima KEDUA bahasa.
                source = source.Where(x =>
                    EF.Functions.ILike(x.DetailTypeNameEn, pattern) ||
                    (x.DetailTypeNameId != null && EF.Functions.ILike(x.DetailTypeNameId, pattern)) ||
                    EF.Functions.ILike(x.DetailTypeCode, pattern));
            }

            return source;
        }

        private static string ResolveDisplayName(LabSpecimenDetailType entity)
            => string.IsNullOrWhiteSpace(entity.DetailTypeNameId)
                ? entity.DetailTypeNameEn
                : entity.DetailTypeNameId;

        private static LabSpecimenDetailTypeResponse MapToResponse(LabSpecimenDetailType entity)
            => new()
            {
                Id = entity.Id,
                LabSpecimenTypeId = entity.LabSpecimenTypeId,
                SpecimenTypeCode = entity.LabSpecimenType?.SpecimenTypeCode,
                SpecimenTypeName = entity.LabSpecimenType?.SpecimenTypeName,
                DetailTypeCode = entity.DetailTypeCode,
                DetailTypeNameId = entity.DetailTypeNameId,
                DetailTypeNameEn = entity.DetailTypeNameEn,
                DisplayName = ResolveDisplayName(entity),
                IsUntranslated = string.IsNullOrWhiteSpace(entity.DetailTypeNameId),
                SubTypeName = entity.SubTypeName,
                SnomedCode = entity.SnomedCode,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>Kode atau nama sudah dipakai. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabSpecimenDetailTypeConflictException(string message) : Exception(message);
}
