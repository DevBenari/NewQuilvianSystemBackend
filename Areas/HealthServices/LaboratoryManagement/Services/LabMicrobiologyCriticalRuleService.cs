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
    /// <summary>Satu aturan kritis dalam bentuk yang dapat dicocokkan di memori.</summary>
    public readonly record struct LabCriticalRuleSnapshot(
        Guid? LabOrganismId,
        Guid? LabAntibioticId,
        LabSusceptibilityResult? SusceptibilityResult);

    /// <summary>
    /// Kumpulan aturan kritis yang berlaku, beserta penilainya.
    ///
    /// <b><see cref="Available"/> membedakan dua keadaan yang terlihat sama.</b> Nol penanda
    /// menyala dapat berarti <i>"tidak ada yang kritis"</i> atau <i>"belum ada aturannya"</i>,
    /// dan layar wajib dapat menyatakan yang mana (<c>LAB-DEC-103</c> butir 5).
    /// </summary>
    public sealed class LabCriticalRuleSet
    {
        private readonly IReadOnlyList<LabCriticalRuleSnapshot> _rules;

        public LabCriticalRuleSet(IReadOnlyList<LabCriticalRuleSnapshot> rules) => _rules = rules;

        /// <summary>Ada tidaknya aturan kritis yang berlaku sama sekali.</summary>
        public bool Available => _rules.Count > 0;

        /// <summary>
        /// Apakah satu baris antibiogram tergolong kritis.
        ///
        /// <b>Cukup satu aturan cocok.</b> Ruas aturan yang kosong berarti "apa saja", dan
        /// tumpang tindih antaraturan nol menimbulkan keraguan — keduanya sama-sama benar.
        ///
        /// <b>Nol aturan berarti nol kritis</b>, bukan "semuanya kritis". Itu pilihan
        /// fail-closed terhadap kebisingan: alarm yang berbunyi terus-menerus berhenti dibaca
        /// orang, dan layar wajib menyatakan kekosongannya lewat <see cref="Available"/>.
        /// </summary>
        public bool IsCritical(Guid organismId, Guid antibioticId, LabSusceptibilityResult result)
        {
            foreach (var rule in _rules)
            {
                if (rule.LabOrganismId.HasValue && rule.LabOrganismId.Value != organismId)
                    continue;

                if (rule.LabAntibioticId.HasValue && rule.LabAntibioticId.Value != antibioticId)
                    continue;

                if (rule.SusceptibilityResult.HasValue && rule.SusceptibilityResult.Value != result)
                    continue;

                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Data induk aturan kritis Mikrobiologi (<c>LAB-DEC-103</c>, <c>BE-LAB-56</c>).
    ///
    /// <b>Hak tulisnya dipegang wewenang klinis Mikrobiologi (<c>DR-LAB-002</c>)</b>, bukan
    /// kepala instalasi: menetapkan kombinasi mana yang membahayakan pasien sehingga dokter
    /// wajib dihubungi malam itu juga adalah penilaian klinis.
    /// </summary>
    public class LabMicrobiologyCriticalRuleService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabMicrobiologyCriticalRuleService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Menarik seluruh aturan aktif <b>sekali</b>, lalu mencocokkannya di memori terhadap
        /// setiap baris antibiogram — satu antibiogram lazim memuat dua puluh baris lebih.
        /// </summary>
        public async Task<LabCriticalRuleSet> LoadRuleSetAsync(CancellationToken cancellationToken = default)
        {
            var rules = await _dbContext.LabMicrobiologyCriticalRules
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .Select(x => new LabCriticalRuleSnapshot(
                    x.LabOrganismId, x.LabAntibioticId, x.SusceptibilityResult))
                .ToListAsync(cancellationToken);

            return new LabCriticalRuleSet(rules);
        }

        public async Task<PagedResult<LabMicrobiologyCriticalRuleResponse>> GetListAsync(
            LabMicrobiologyCriticalRulePagedQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

            var source = _dbContext.LabMicrobiologyCriticalRules
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

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<LabMicrobiologyCriticalRuleResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
                Items = [.. items.Select(MapToResponse)]
            };
        }

        public async Task<LabMicrobiologyCriticalRuleResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabMicrobiologyCriticalRules
                .AsNoTracking()
                .Include(x => x.LabOrganism)
                .Include(x => x.LabAntibiotic)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Aturan kritis tidak ditemukan.");

            return MapToResponse(entity);
        }

        public async Task<LabMicrobiologyCriticalRuleResponse> CreateAsync(
            CreateLabMicrobiologyCriticalRuleRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            EnsureRuleMeaningful(request.LabOrganismId, request.LabAntibioticId, request.SusceptibilityResult);
            await EnsureMasterDataValidAsync(request.LabOrganismId, request.LabAntibioticId, cancellationToken);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new LabMicrobiologyCriticalRule
            {
                LabOrganismId = request.LabOrganismId,
                LabAntibioticId = request.LabAntibioticId,
                SusceptibilityResult = request.SusceptibilityResult,
                RuleNote = Normalize(request.RuleNote),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabMicrobiologyCriticalRules.Add(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Aturan keselamatan: setiap perubahannya wajib dapat ditelusuri.
            await _loggerService.AuditAsync(
                LogCategory,
                "LabMicrobiologyCriticalRule.Create",
                "Menambah aturan nilai kritis Mikrobiologi.",
                new
                {
                    entity.Id,
                    entity.LabOrganismId,
                    entity.LabAntibioticId,
                    Hasil = entity.SusceptibilityResult?.ToString()
                });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<LabMicrobiologyCriticalRuleResponse> UpdateAsync(
            Guid id,
            UpdateLabMicrobiologyCriticalRuleRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabMicrobiologyCriticalRules
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Aturan kritis tidak ditemukan.");

            EnsureRuleMeaningful(request.LabOrganismId, request.LabAntibioticId, request.SusceptibilityResult);
            await EnsureMasterDataValidAsync(request.LabOrganismId, request.LabAntibioticId, cancellationToken);

            var sebelum = new
            {
                entity.LabOrganismId,
                entity.LabAntibioticId,
                Hasil = entity.SusceptibilityResult?.ToString(),
                entity.IsActive
            };

            entity.LabOrganismId = request.LabOrganismId;
            entity.LabAntibioticId = request.LabAntibioticId;
            entity.SusceptibilityResult = request.SusceptibilityResult;
            entity.RuleNote = Normalize(request.RuleNote);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabMicrobiologyCriticalRule.Update",
                "Mengubah aturan nilai kritis Mikrobiologi.",
                new
                {
                    entity.Id,
                    Sebelum = sebelum,
                    Sesudah = new
                    {
                        entity.LabOrganismId,
                        entity.LabAntibioticId,
                        Hasil = entity.SusceptibilityResult?.ToString(),
                        entity.IsActive
                    }
                });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        /// <summary>Menonaktifkan aturan. Nol baris dihapus.</summary>
        public async Task DeactivateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabMicrobiologyCriticalRules
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Aturan kritis tidak ditemukan.");

            if (!entity.IsActive)
                return;

            entity.IsActive = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabMicrobiologyCriticalRule.Deactivate",
                "Menonaktifkan aturan nilai kritis Mikrobiologi.",
                new { entity.Id });
        }

        // VAL-106. Aturan yang ketiga ruas penilainya kosong berarti SELURUH hasil kritis, dan
        // itu mematikan guna penandanya — alarm yang berbunyi terus-menerus berhenti dibaca.
        private static void EnsureRuleMeaningful(
            Guid? organismId,
            Guid? antibioticId,
            LabSusceptibilityResult? result)
        {
            if (organismId is null && antibioticId is null && result is null)
            {
                throw new ArgumentException(
                    "Aturan kritis harus menyebut sedikitnya organisme, antibiotik, atau hasil kepekaan.");
            }
        }

        private async Task EnsureMasterDataValidAsync(
            Guid? organismId,
            Guid? antibioticId,
            CancellationToken cancellationToken)
        {
            if (organismId.HasValue)
            {
                var ada = await _dbContext.LabOrganisms
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == organismId.Value && !x.IsDelete && x.IsActive, cancellationToken);

                if (!ada)
                    throw new ArgumentException("Organisme yang dipilih tidak berlaku.");
            }

            if (antibioticId.HasValue)
            {
                var ada = await _dbContext.LabAntibiotics
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == antibioticId.Value && !x.IsDelete && x.IsActive, cancellationToken);

                if (!ada)
                    throw new ArgumentException("Antibiotik yang dipilih tidak berlaku.");
            }
        }

        private static LabMicrobiologyCriticalRuleResponse MapToResponse(LabMicrobiologyCriticalRule entity)
        {
            var kuman = entity.LabOrganism?.OrganismName ?? "kuman apa saja";
            var obat = entity.LabAntibiotic?.AntibioticName ?? "antibiotik apa saja";
            var hasil = entity.SusceptibilityResult?.ToString() ?? "hasil apa saja";

            return new LabMicrobiologyCriticalRuleResponse
            {
                Id = entity.Id,
                LabOrganismId = entity.LabOrganismId,
                OrganismName = entity.LabOrganism?.OrganismName,
                LabAntibioticId = entity.LabAntibioticId,
                AntibioticName = entity.LabAntibiotic?.AntibioticName,
                SusceptibilityResult = entity.SusceptibilityResult,
                RuleNote = entity.RuleNote,
                RuleSummary = $"Kritis bila {kuman} diuji terhadap {obat} dengan {hasil}.",
                IsActive = entity.IsActive
            };
        }

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
}
