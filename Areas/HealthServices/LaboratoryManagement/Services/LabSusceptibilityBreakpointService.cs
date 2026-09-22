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
    /// Data induk rentang breakpoint uji kepekaan (<c>LAB-DEC-122</c>, <c>BE-LAB-60</c>).
    ///
    /// <b>Isinya menentukan interpretasi yang dilaporkan kepada dokter.</b> Menggeser satu
    /// batas mengubah sebagian hasil dari <c>R</c> menjadi <c>I</c> tanpa satu pun hasil
    /// disunting — itu sebabnya hak tulisnya dipegang wewenang klinis Mikrobiologi, dan
    /// setiap perubahan dicatat jejak audit.
    /// </summary>
    public class LabSusceptibilityBreakpointService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabSusceptibilityBreakpointService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<PagedResult<LabSusceptibilityBreakpointResponse>> GetListAsync(
            LabSusceptibilityBreakpointPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

            var source = _dbContext.LabSusceptibilityBreakpoints
                .AsNoTracking()
                .Include(x => x.LabOrganism)
                .Include(x => x.LabAntibiotic)
                .Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            if (query.LabOrganismId.HasValue)
                source = source.Where(x => x.LabOrganismId == query.LabOrganismId.Value);

            if (query.LabAntibioticId.HasValue)
                source = source.Where(x => x.LabAntibioticId == query.LabAntibioticId.Value);

            var search = query.Search?.Trim();

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.LabOrganism!.OrganismName, pattern) ||
                    EF.Functions.ILike(x.LabAntibiotic!.AntibioticName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.LabOrganism!.OrganismName)
                .ThenBy(x => x.LabAntibiotic!.AntibioticName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => MapToResponse(x))
                .ToListAsync(cancellationToken);

            return new PagedResult<LabSusceptibilityBreakpointResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<LabSusceptibilityBreakpointResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabSusceptibilityBreakpoints
                .AsNoTracking()
                .Include(x => x.LabOrganism)
                .Include(x => x.LabAntibiotic)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Rentang breakpoint tidak ditemukan.");

            return MapToResponse(entity);
        }

        public async Task<LabSusceptibilityBreakpointResponse> CreateAsync(
            CreateLabSusceptibilityBreakpointRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            await EnsureMasterDataExistsAsync(request.LabOrganismId, request.LabAntibioticId, cancellationToken);
            EnsureRangeValid(request.LowerMm, request.UpperMm);

            // VAL-119. Dua rentang aktif atas pasangan yang sama membuat hitungan interpretasi
            // bergantung baris mana yang kebetulan terbaca lebih dulu.
            var duplicate = await _dbContext.LabSusceptibilityBreakpoints
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.LabOrganismId == request.LabOrganismId &&
                    x.LabAntibioticId == request.LabAntibioticId,
                    cancellationToken);

            if (duplicate)
            {
                throw new LabSusceptibilityBreakpointConflictException(
                    "Breakpoint untuk pasangan organisme dan antibiotik ini sudah ada.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new LabSusceptibilityBreakpoint
            {
                LabOrganismId = request.LabOrganismId,
                LabAntibioticId = request.LabAntibioticId,
                LowerMm = request.LowerMm,
                UpperMm = request.UpperMm,
                GuidelineVersion = Normalize(request.GuidelineVersion),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabSusceptibilityBreakpoints.Add(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabSusceptibilityBreakpoint.Create",
                "Menambah rentang breakpoint uji kepekaan.",
                new { entity.Id, entity.LabOrganismId, entity.LabAntibioticId, entity.LowerMm, entity.UpperMm });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<LabSusceptibilityBreakpointResponse> UpdateAsync(
            Guid id,
            UpdateLabSusceptibilityBreakpointRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabSusceptibilityBreakpoints
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Rentang breakpoint tidak ditemukan.");

            EnsureRangeValid(request.LowerMm, request.UpperMm);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var sebelum = new { entity.LowerMm, entity.UpperMm, entity.IsActive };

            entity.LowerMm = request.LowerMm;
            entity.UpperMm = request.UpperMm;
            entity.GuidelineVersion = Normalize(request.GuidelineVersion);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Perubahan rentang adalah perubahan aturan keselamatan: nilai lama WAJIB ikut
            // tercatat, sebab pertanyaan "kenapa hasil bulan lalu berbeda" hanya dapat
            // dijawab dari sini.
            await _loggerService.AuditAsync(
                LogCategory,
                "LabSusceptibilityBreakpoint.Update",
                "Mengubah rentang breakpoint uji kepekaan.",
                new
                {
                    entity.Id,
                    Sebelum = sebelum,
                    Sesudah = new { entity.LowerMm, entity.UpperMm, entity.IsActive }
                });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        /// <summary>
        /// Menonaktifkan rentang. <b>Nol baris dihapus</b> — baris hasil lama menyimpan
        /// snapshot-nya, dan riwayat perubahan rentang adalah riwayat keselamatan.
        /// </summary>
        public async Task DeactivateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabSusceptibilityBreakpoints
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Rentang breakpoint tidak ditemukan.");

            if (!entity.IsActive)
                return;

            entity.IsActive = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabSusceptibilityBreakpoint.Deactivate",
                "Menonaktifkan rentang breakpoint uji kepekaan.",
                new { entity.Id, entity.LabOrganismId, entity.LabAntibioticId });
        }

        /// <summary>
        /// Ringkasan data induk breakpoint (<c>GET /summary</c>, baseline master data).
        ///
        /// <b>Dihitung dari baris yang belum ditandai terhapus.</b> Tiga pencacah terakhir
        /// menjawab pertanyaan yang benar-benar ditanyakan wewenang klinis ketika membuka
        /// layar ini: berapa kombinasi yang sudah tertutup, dan berapa yang masih menganga.
        /// </summary>
        public async Task<LabSusceptibilityBreakpointSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.LabSusceptibilityBreakpoints
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            return new LabSusceptibilityBreakpointSummaryResponse
            {
                TotalBreakpoint = await source.CountAsync(cancellationToken),
                ActiveBreakpoint = await source.CountAsync(x => x.IsActive, cancellationToken),
                InactiveBreakpoint = await source.CountAsync(x => !x.IsActive, cancellationToken),

                CoveredOrganism = await source
                    .Where(x => x.IsActive)
                    .Select(x => x.LabOrganismId)
                    .Distinct()
                    .CountAsync(cancellationToken),

                CoveredAntibiotic = await source
                    .Where(x => x.IsActive)
                    .Select(x => x.LabAntibioticId)
                    .Distinct()
                    .CountAsync(cancellationToken),

                // Kandungan cakram melekat pada ANTIBIOTIK, bukan pada rentangnya — satu
                // antibiotik memakai cakram yang sama pada kuman mana pun.
                MissingDiscContent = await source
                    .CountAsync(x => x.IsActive && x.LabAntibiotic!.DiscContentUg == null, cancellationToken)
            };
        }

        /// <summary>
        /// Feed ringan untuk dropdown (<c>GET /options</c>).
        ///
        /// <b>Hanya baris aktif secara bawaan</b>, dan payload-nya jauh lebih ringkas daripada
        /// list utama — pemakainya hanya perlu mengenali kombinasinya, bukan membaca rentang
        /// maupun versi pedomannya.
        /// </summary>
        public async Task<PagedResult<LabSusceptibilityBreakpointOptionResponse>> GetOptionsAsync(
            string? search = null,
            bool onlyActive = true,
            int pageNumber = 1,
            int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var halaman = pageNumber < 1 ? 1 : pageNumber;
            var ukuran = pageSize is < 1 or > 100 ? 25 : pageSize;

            var source = _dbContext.LabSusceptibilityBreakpoints
                .AsNoTracking()
                .Include(x => x.LabOrganism)
                .Include(x => x.LabAntibiotic)
                .Where(x => !x.IsDelete);

            if (onlyActive)
                source = source.Where(x => x.IsActive);

            var kata = search?.Trim();

            if (!string.IsNullOrEmpty(kata))
            {
                source = source.Where(x =>
                    (x.LabOrganism != null && EF.Functions.ILike(x.LabOrganism.OrganismName, $"%{kata}%")) ||
                    (x.LabAntibiotic != null && EF.Functions.ILike(x.LabAntibiotic.AntibioticName, $"%{kata}%")));
            }

            var total = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.LabOrganism!.OrganismName)
                .ThenBy(x => x.LabAntibiotic!.AntibioticName)
                .Skip((halaman - 1) * ukuran)
                .Take(ukuran)
                .Select(x => new LabSusceptibilityBreakpointOptionResponse
                {
                    Id = x.Id,
                    LabOrganismId = x.LabOrganismId,
                    LabAntibioticId = x.LabAntibioticId,
                    Label =
                        (x.LabOrganism != null ? x.LabOrganism.OrganismName : "-") +
                        " — " +
                        (x.LabAntibiotic != null ? x.LabAntibiotic.AntibioticName : "-")
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabSusceptibilityBreakpointOptionResponse>
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
        /// <b>Jalur tersendiri, terpisah dari <c>PUT</c>.</b> Menonaktifkan rentang yang keliru
        /// dan mengubah angka rentangnya adalah dua tindakan yang berbeda akibatnya; satu
        /// jalur untuk keduanya membuat keduanya sama mudahnya terjadi tanpa sengaja.
        ///
        /// <b>Nol menyentuh hasil yang sudah tersimpan</b> — snapshot pada baris kepekaan
        /// menjaganya (<c>AC-185</c>).
        /// </summary>
        public async Task<LabSusceptibilityBreakpointResponse> SetStatusAsync(
            Guid id,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabSusceptibilityBreakpoints
                .Include(x => x.LabOrganism)
                .Include(x => x.LabAntibiotic)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Rentang breakpoint tidak ditemukan.");

            if (entity.IsActive != isActive)
            {
                entity.IsActive = isActive;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = GetCurrentUserId();

                await _dbContext.SaveChangesAsync(cancellationToken);

                await _loggerService.AuditAsync(
                    LogCategory,
                    "LabSusceptibilityBreakpoint.SetStatus",
                    isActive ? "Mengaktifkan rentang breakpoint." : "Menonaktifkan rentang breakpoint.",
                    new { entity.Id, entity.LabOrganismId, entity.LabAntibioticId, isActive });
            }

            return new LabSusceptibilityBreakpointResponse
            {
                Id = entity.Id,
                LabOrganismId = entity.LabOrganismId,
                OrganismName = entity.LabOrganism?.OrganismName,
                LabAntibioticId = entity.LabAntibioticId,
                AntibioticName = entity.LabAntibiotic?.AntibioticName,
                DiscContentUg = entity.LabAntibiotic?.DiscContentUg,
                LowerMm = entity.LowerMm,
                UpperMm = entity.UpperMm,
                GuidelineVersion = entity.GuidelineVersion,
                IsActive = entity.IsActive
            };
        }

        private async Task EnsureMasterDataExistsAsync(
            Guid organismId,
            Guid antibioticId,
            CancellationToken cancellationToken)
        {
            var organismAda = await _dbContext.LabOrganisms
                .AsNoTracking()
                .AnyAsync(x => x.Id == organismId && !x.IsDelete && x.IsActive, cancellationToken);

            if (!organismAda)
                throw new ArgumentException("Organisme yang dipilih tidak berlaku.");

            var antibiotikAda = await _dbContext.LabAntibiotics
                .AsNoTracking()
                .AnyAsync(x => x.Id == antibioticId && !x.IsDelete && x.IsActive, cancellationToken);

            if (!antibiotikAda)
                throw new ArgumentException("Antibiotik yang dipilih tidak berlaku.");
        }

        // VAL-115. Rentang terbalik membuat hitungan interpretasi menghasilkan Intermediate
        // yang mustahil dicapai — nol zona dapat berada di antara batas bawah yang lebih besar
        // daripada batas atas.
        private static void EnsureRangeValid(int lowerMm, int upperMm)
        {
            if (lowerMm < 0 || upperMm < 0)
                throw new ArgumentException("Batas zona tidak boleh kurang dari nol.");

            if (lowerMm > upperMm)
                throw new ArgumentException("Batas bawah tidak boleh lebih besar daripada batas atas.");
        }

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static LabSusceptibilityBreakpointResponse MapToResponse(LabSusceptibilityBreakpoint entity)
            => new()
            {
                Id = entity.Id,
                LabOrganismId = entity.LabOrganismId,
                OrganismName = entity.LabOrganism?.OrganismName,
                LabAntibioticId = entity.LabAntibioticId,
                AntibioticName = entity.LabAntibiotic?.AntibioticName,
                DiscContentUg = entity.LabAntibiotic?.DiscContentUg,
                LowerMm = entity.LowerMm,
                UpperMm = entity.UpperMm,
                GuidelineVersion = entity.GuidelineVersion,
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

    /// <summary>Pasangan organisme dan antibiotik sudah punya rentang. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabSusceptibilityBreakpointConflictException(string message) : Exception(message);
}
