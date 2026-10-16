using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Template Hasil — menu v1 Setup → Template Hasil (<c>BE-RAD-26</c>, <c>RAD-DEC-021</c>).
    /// </summary>
    /// <remarks>
    /// Template hanya isian awal draf; pengesahan dokter radiolog tetap berlaku (<c>RAD-DEC-003</c>).
    /// Kode unik di antara template yang belum dihapus; hapus = hapus lunak.
    /// </remarks>
    public class RadReportTemplateService
    {
        private const string LogCategory = "RadiologyManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public RadReportTemplateService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<List<RadReportTemplateResponse>> GetListAsync(
            RadReportTemplateQuery? query,
            CancellationToken cancellationToken = default)
        {
            query ??= new RadReportTemplateQuery();
            var source = _dbContext.RadReportTemplates.AsNoTracking().Where(x => !x.IsDelete);

            if (query.OnlyActive) source = source.Where(x => x.IsActive);
            if (query.ModalityId.HasValue) source = source.Where(x => x.ModalityId == null || x.ModalityId == query.ModalityId);
            if (query.ProcedureId.HasValue) source = source.Where(x => x.ProcedureId == null || x.ProcedureId == query.ProcedureId);

            var cari = query.Search?.Trim().ToLower();
            if (!string.IsNullOrEmpty(cari))
            {
                source = source.Where(x => x.TemplateName.ToLower().Contains(cari) || x.TemplateCode.ToLower().Contains(cari));
            }

            var rows = await source
                .OrderBy(x => x.SortOrder).ThenBy(x => x.TemplateName)
                .Select(x => new RadReportTemplateResponse
                {
                    Id = x.Id,
                    TemplateCode = x.TemplateCode,
                    TemplateName = x.TemplateName,
                    ModalityId = x.ModalityId,
                    ProcedureId = x.ProcedureId,
                    Findings = x.Findings,
                    Impression = x.Impression,
                    Recommendation = x.Recommendation,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder,
                })
                .ToListAsync(cancellationToken);

            await LengkapiNamaAsync(rows, cancellationToken);
            return rows;
        }

        public Task<RadOperationResult<RadReportTemplateResponse>> CreateAsync(
            RadReportTemplateRequest request, CancellationToken cancellationToken = default) =>
            SimpanAsync(null, request, cancellationToken);

        public Task<RadOperationResult<RadReportTemplateResponse>> UpdateAsync(
            Guid id, RadReportTemplateRequest request, CancellationToken cancellationToken = default) =>
            SimpanAsync(id, request, cancellationToken);

        private async Task<RadOperationResult<RadReportTemplateResponse>> SimpanAsync(
            Guid? id, RadReportTemplateRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var kode = request.TemplateCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var nama = request.TemplateName?.Trim() ?? string.Empty;
            var kesan = request.Impression?.Trim() ?? string.Empty;

            if (kode.Length == 0 || nama.Length == 0 || kesan.Length == 0)
            {
                return RadOperationResult<RadReportTemplateResponse>.Validation(
                    RadErrorCodes.ValidationFailed, "Kode, nama, dan kesan template wajib diisi.");
            }

            if (await _dbContext.RadReportTemplates.AnyAsync(x => !x.IsDelete && x.TemplateCode == kode && x.Id != (id ?? Guid.Empty), cancellationToken))
            {
                return RadOperationResult<RadReportTemplateResponse>.Conflict(
                    RadErrorCodes.ValidationFailed, $"Kode template {kode} sudah dipakai.");
            }

            if (request.ModalityId is Guid modalitas &&
                !await _dbContext.MstRadModalities.AnyAsync(m => m.Id == modalitas && !m.IsDelete, cancellationToken))
            {
                return RadOperationResult<RadReportTemplateResponse>.Validation(RadErrorCodes.ModalityNotFound, "Alat pencitraan tidak ditemukan.");
            }

            if (request.ProcedureId is Guid tindakan &&
                !await _dbContext.MstProcedures.AnyAsync(p => p.Id == tindakan && p.IsRadiology && !p.IsDelete, cancellationToken))
            {
                return RadOperationResult<RadReportTemplateResponse>.Validation(RadErrorCodes.ProcedureNotFound, "Pemeriksaan radiologi tidak ditemukan.");
            }

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            RadReportTemplate? entity;

            if (id is Guid templateId)
            {
                entity = await _dbContext.RadReportTemplates.FirstOrDefaultAsync(x => x.Id == templateId && !x.IsDelete, cancellationToken);
                if (entity == null)
                {
                    return RadOperationResult<RadReportTemplateResponse>.NotFound(RadErrorCodes.TemplateNotFound, "Template hasil tidak ditemukan.");
                }
                entity.UpdateBy = actorUserId;
                entity.UpdateDateTime = now;
            }
            else
            {
                entity = new RadReportTemplate { CreateBy = actorUserId, CreateDateTime = now };
                _dbContext.RadReportTemplates.Add(entity);
            }

            entity.TemplateCode = kode;
            entity.TemplateName = nama;
            entity.ModalityId = request.ModalityId;
            entity.ProcedureId = request.ProcedureId;
            entity.Findings = string.IsNullOrWhiteSpace(request.Findings) ? null : request.Findings.Trim();
            entity.Impression = kesan;
            entity.Recommendation = string.IsNullOrWhiteSpace(request.Recommendation) ? null : request.Recommendation.Trim();
            entity.IsActive = request.IsActive;
            entity.SortOrder = request.SortOrder;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.InfoAsync(LogCategory, id == null ? "RadReportTemplate.Create" : "RadReportTemplate.Update",
                $"Template hasil radiologi {kode} — {nama} disimpan.", new { TemplateId = entity.Id, ActorUserId = actorUserId });

            return RadOperationResult<RadReportTemplateResponse>.Success((await GetListAsync(
                new RadReportTemplateQuery { OnlyActive = false }, cancellationToken)).First(x => x.Id == entity.Id));
        }

        public async Task<RadOperationResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.RadReportTemplates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
            {
                return RadOperationResult<bool>.NotFound(RadErrorCodes.TemplateNotFound, "Template hasil tidak ditemukan.");
            }

            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteBy = actorUserId;
            entity.DeleteDateTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.InfoAsync(LogCategory, "RadReportTemplate.Delete",
                $"Template hasil radiologi {entity.TemplateCode} dihapus.", new { TemplateId = id, ActorUserId = actorUserId });

            return RadOperationResult<bool>.Success(true);
        }

        private async Task LengkapiNamaAsync(List<RadReportTemplateResponse> rows, CancellationToken cancellationToken)
        {
            var modalitasIds = rows.Where(r => r.ModalityId.HasValue).Select(r => r.ModalityId!.Value).Distinct().ToList();
            var tindakanIds = rows.Where(r => r.ProcedureId.HasValue).Select(r => r.ProcedureId!.Value).Distinct().ToList();

            var modalitas = modalitasIds.Count == 0 ? new Dictionary<Guid, string>() : await _dbContext.MstRadModalities
                .AsNoTracking().Where(m => modalitasIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.ModalityName, cancellationToken);
            var tindakan = tindakanIds.Count == 0 ? new Dictionary<Guid, string>() : await _dbContext.MstProcedures
                .AsNoTracking().Where(p => tindakanIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.ProcedureName, cancellationToken);

            foreach (var r in rows)
            {
                if (r.ModalityId.HasValue) r.ModalityName = modalitas.GetValueOrDefault(r.ModalityId.Value);
                if (r.ProcedureId.HasValue) r.ProcedureName = tindakan.GetValueOrDefault(r.ProcedureId.Value);
            }
        }

        private Guid GetCurrentUserId()
        {
            var raw = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }
}
