using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Layanan pengelolaan master data Standar Diagnosis Keperawatan Indonesia (SDKI),
    /// Standar Luaran Keperawatan Indonesia (SLKI), dan Standar Intervensi Keperawatan Indonesia (SIKI).
    /// </summary>
    public class NursingDiagnosisService
    {
        private readonly ApplicationDbContext _dbContext;

        public NursingDiagnosisService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NursingDiagnosisSummaryResponse> GetSummaryAsync(CancellationToken ct = default)
        {
            var totalDiagnosis = await _dbContext.MstNursingDiagnoses.CountAsync(x => !x.IsDelete, ct);
            var activeDiagnosis = await _dbContext.MstNursingDiagnoses.CountAsync(x => !x.IsDelete && x.IsActive, ct);
            var inactiveDiagnosis = totalDiagnosis - activeDiagnosis;
            var totalGroups = await _dbContext.MstNursingDiagnosisGroups.CountAsync(x => !x.IsDelete && x.IsActive, ct);
            var totalInterventions = await _dbContext.MstNursingDiagnosisInterventions.CountAsync(x => !x.IsDelete && x.IsActive, ct);
            var totalOutcomes = await _dbContext.MstNursingDiagnosisOutcomes.CountAsync(x => !x.IsDelete && x.IsActive, ct);

            return new NursingDiagnosisSummaryResponse
            {
                TotalDiagnosis = totalDiagnosis,
                ActiveDiagnosis = activeDiagnosis,
                InactiveDiagnosis = inactiveDiagnosis,
                TotalGroups = totalGroups,
                TotalInterventions = totalInterventions,
                TotalOutcomes = totalOutcomes
            };
        }

        public async Task<PagedResult<NursingDiagnosisListItemResponse>> GetPagedListAsync(
            string? search,
            Guid? groupId,
            bool? isActive,
            int page = 1,
            int perPage = 25,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 25;
            if (perPage > 100) perPage = 100;

            var query = _dbContext.MstNursingDiagnoses
                .AsNoTracking()
                .Include(x => x.Group)
                .Include(x => x.Etiologies.Where(e => !e.IsDelete))
                .Include(x => x.Outcomes.Where(o => !o.IsDelete))
                .Include(x => x.Interventions.Where(i => !i.IsDelete))
                .Where(x => !x.IsDelete);

            if (groupId.HasValue && groupId.Value != Guid.Empty)
            {
                query = query.Where(x => x.GroupId == groupId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(s) ||
                    x.Name.ToLower().Contains(s) ||
                    (x.SubCategory != null && x.SubCategory.ToLower().Contains(s)));
            }

            var totalRows = await query.CountAsync(ct);
            var items = await query
                .OrderBy(x => x.Code)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(x => new NursingDiagnosisListItemResponse
                {
                    Id = x.Id,
                    GroupId = x.GroupId,
                    GroupCode = x.Group != null ? x.Group.GroupCode : null,
                    GroupName = x.Group != null ? x.Group.GroupName : null,
                    Code = x.Code,
                    Name = x.Name,
                    SubCategory = x.SubCategory,
                    Definition = x.Definition,
                    TerminologySystem = x.TerminologySystem,
                    IsActive = x.IsActive,
                    EtiologyCount = x.Etiologies.Count(e => !e.IsDelete),
                    OutcomeCount = x.Outcomes.Count(o => !o.IsDelete),
                    InterventionCount = x.Interventions.Count(i => !i.IsDelete)
                })
                .ToListAsync(ct);

            return new PagedResult<NursingDiagnosisListItemResponse>
            {
                PageNumber = page,
                PageSize = perPage,
                TotalData = totalRows,
                TotalPage = (int)Math.Ceiling(totalRows / (double)perPage),
                Items = items
            };
        }

        public async Task<List<NursingDiagnosisOptionResponse>> GetOptionsAsync(
            string? search,
            int limit = 50,
            CancellationToken ct = default)
        {
            if (limit < 1) limit = 20;
            if (limit > 100) limit = 100;

            var query = _dbContext.MstNursingDiagnoses
                .AsNoTracking()
                .Include(x => x.Group)
                .Where(x => !x.IsDelete && x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(s) ||
                    x.Name.ToLower().Contains(s));
            }

            return await query
                .OrderBy(x => x.Code)
                .Take(limit)
                .Select(x => new NursingDiagnosisOptionResponse
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    GroupName = x.Group != null ? x.Group.GroupName : null
                })
                .ToListAsync(ct);
        }

        public async Task<NursingDiagnosisDetailResponse?> GetDetailByIdAsync(Guid id, CancellationToken ct = default)
        {
            var diag = await _dbContext.MstNursingDiagnoses
                .AsNoTracking()
                .Include(x => x.Group)
                .Include(x => x.Etiologies.Where(e => !e.IsDelete))
                .Include(x => x.Outcomes.Where(o => !o.IsDelete))
                .Include(x => x.Interventions.Where(i => !i.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (diag == null) return null;

            return new NursingDiagnosisDetailResponse
            {
                Id = diag.Id,
                GroupId = diag.GroupId,
                GroupCode = diag.Group?.GroupCode,
                GroupName = diag.Group?.GroupName,
                Code = diag.Code,
                Name = diag.Name,
                SubCategory = diag.SubCategory,
                Definition = diag.Definition,
                TerminologySystem = diag.TerminologySystem,
                IsActive = diag.IsActive,
                EtiologyCount = diag.Etiologies.Count(e => !e.IsDelete),
                OutcomeCount = diag.Outcomes.Count(o => !o.IsDelete),
                InterventionCount = diag.Interventions.Count(i => !i.IsDelete),
                CreateDateTime = diag.CreateDateTime,
                UpdateDateTime = diag.UpdateDateTime,
                Etiologies = diag.Etiologies
                    .Where(e => !e.IsDelete)
                    .Select(e => new NursingDiagnosisEtiologyResponse
                    {
                        Id = e.Id,
                        NursingDiagnosisId = e.NursingDiagnosisId,
                        EtiologyName = e.EtiologyName,
                        Description = e.Description,
                        IsActive = e.IsActive
                    })
                    .ToList(),
                Outcomes = diag.Outcomes
                    .Where(o => !o.IsDelete)
                    .Select(o => new NursingDiagnosisOutcomeResponse
                    {
                        Id = o.Id,
                        NursingDiagnosisId = o.NursingDiagnosisId,
                        OutcomeCode = o.OutcomeCode,
                        OutcomeName = o.OutcomeName,
                        Expectation = o.Expectation,
                        TerminologySystem = o.TerminologySystem,
                        IsActive = o.IsActive
                    })
                    .ToList(),
                Interventions = diag.Interventions
                    .Where(i => !i.IsDelete)
                    .OrderBy(i => i.PillarType)
                    .Select(i => new NursingDiagnosisInterventionResponse
                    {
                        Id = i.Id,
                        NursingDiagnosisId = i.NursingDiagnosisId,
                        PillarType = i.PillarType,
                        InterventionCode = i.InterventionCode,
                        InterventionName = i.InterventionName,
                        ActionDescription = i.ActionDescription,
                        TerminologySystem = i.TerminologySystem,
                        IsDefaultRecommendation = i.IsDefaultRecommendation,
                        IsActive = i.IsActive
                    })
                    .ToList()
            };
        }

        public async Task<NursingDiagnosisBundleResponse?> GetBundleByIdAsync(Guid id, CancellationToken ct = default)
        {
            var diag = await _dbContext.MstNursingDiagnoses
                .AsNoTracking()
                .Include(x => x.Etiologies.Where(e => !e.IsDelete && e.IsActive))
                .Include(x => x.Outcomes.Where(o => !o.IsDelete && o.IsActive))
                .Include(x => x.Interventions.Where(i => !i.IsDelete && i.IsActive))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (diag == null) return null;

            var observations = diag.Interventions
                .Where(i => i.PillarType == 1)
                .Select(i => new NursingDiagnosisInterventionResponse
                {
                    Id = i.Id,
                    NursingDiagnosisId = i.NursingDiagnosisId,
                    PillarType = i.PillarType,
                    InterventionCode = i.InterventionCode,
                    InterventionName = i.InterventionName,
                    ActionDescription = i.ActionDescription,
                    TerminologySystem = i.TerminologySystem,
                    IsDefaultRecommendation = i.IsDefaultRecommendation,
                    IsActive = i.IsActive
                })
                .ToList();

            var therapeutics = diag.Interventions
                .Where(i => i.PillarType == 2)
                .Select(i => new NursingDiagnosisInterventionResponse
                {
                    Id = i.Id,
                    NursingDiagnosisId = i.NursingDiagnosisId,
                    PillarType = i.PillarType,
                    InterventionCode = i.InterventionCode,
                    InterventionName = i.InterventionName,
                    ActionDescription = i.ActionDescription,
                    TerminologySystem = i.TerminologySystem,
                    IsDefaultRecommendation = i.IsDefaultRecommendation,
                    IsActive = i.IsActive
                })
                .ToList();

            var educations = diag.Interventions
                .Where(i => i.PillarType == 3)
                .Select(i => new NursingDiagnosisInterventionResponse
                {
                    Id = i.Id,
                    NursingDiagnosisId = i.NursingDiagnosisId,
                    PillarType = i.PillarType,
                    InterventionCode = i.InterventionCode,
                    InterventionName = i.InterventionName,
                    ActionDescription = i.ActionDescription,
                    TerminologySystem = i.TerminologySystem,
                    IsDefaultRecommendation = i.IsDefaultRecommendation,
                    IsActive = i.IsActive
                })
                .ToList();

            var collaborations = diag.Interventions
                .Where(i => i.PillarType == 4)
                .Select(i => new NursingDiagnosisInterventionResponse
                {
                    Id = i.Id,
                    NursingDiagnosisId = i.NursingDiagnosisId,
                    PillarType = i.PillarType,
                    InterventionCode = i.InterventionCode,
                    InterventionName = i.InterventionName,
                    ActionDescription = i.ActionDescription,
                    TerminologySystem = i.TerminologySystem,
                    IsDefaultRecommendation = i.IsDefaultRecommendation,
                    IsActive = i.IsActive
                })
                .ToList();

            // Rangkum template 4 pilar
            var planBuilder = new StringBuilder();
            planBuilder.AppendLine("[Observasi]:");
            foreach (var o in observations) planBuilder.AppendLine($"- {o.ActionDescription}");
            if (observations.Count == 0) planBuilder.AppendLine("- Monitor tanda vital dan kondisi umum pasien secara berkala");

            planBuilder.AppendLine("[Terapeutik]:");
            foreach (var t in therapeutics) planBuilder.AppendLine($"- {t.ActionDescription}");
            if (therapeutics.Count == 0) planBuilder.AppendLine("- Atur posisi nyaman dan fasilitasi lingkungan tenang");

            planBuilder.AppendLine("[Edukasi]:");
            foreach (var e in educations) planBuilder.AppendLine($"- {e.ActionDescription}");
            if (educations.Count == 0) planBuilder.AppendLine("- Edukasi pasien dan keluarga mengenai pembatasan aktivitas");

            planBuilder.AppendLine("[Kolaborasi]:");
            foreach (var c in collaborations) planBuilder.AppendLine($"- {c.ActionDescription}");
            if (collaborations.Count == 0) planBuilder.AppendLine("- Kolaborasi terapi farmakologi sesuai advis DPJP");

            return new NursingDiagnosisBundleResponse
            {
                DiagnosisId = diag.Id,
                DiagnosisCode = diag.Code,
                DiagnosisName = diag.Name,
                SubCategory = diag.SubCategory,
                RecommendedEtiologies = diag.Etiologies.Select(e => e.EtiologyName).ToList(),
                Outcomes = diag.Outcomes.Select(o => new NursingDiagnosisOutcomeResponse
                {
                    Id = o.Id,
                    NursingDiagnosisId = o.NursingDiagnosisId,
                    OutcomeCode = o.OutcomeCode,
                    OutcomeName = o.OutcomeName,
                    Expectation = o.Expectation,
                    TerminologySystem = o.TerminologySystem,
                    IsActive = o.IsActive
                }).ToList(),
                Observations = observations,
                Therapeutics = therapeutics,
                Educations = educations,
                Collaborations = collaborations,
                FormattedSoapAssessment = $"- {diag.Name} ({diag.Code})",
                FormattedSoapPlanTemplate = planBuilder.ToString().TrimEnd()
            };
        }

        public async Task<NursingDiagnosisDetailResponse> CreateDiagnosisAsync(
            CreateNursingDiagnosisRequest req,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var exists = await _dbContext.MstNursingDiagnoses
                .AnyAsync(x => x.Code.ToLower() == req.Code.Trim().ToLower() && !x.IsDelete, ct);

            if (exists)
            {
                throw new InvalidOperationException($"Diagnosis dengan kode '{req.Code}' sudah terdaftar.");
            }

            var now = DateTime.UtcNow;
            var diag = new MstNursingDiagnosis
            {
                Id = Guid.NewGuid(),
                GroupId = req.GroupId,
                Code = req.Code.Trim().ToUpperInvariant(),
                Name = req.Name.Trim(),
                SubCategory = req.SubCategory?.Trim(),
                Definition = req.Definition?.Trim(),
                TerminologySystem = string.IsNullOrWhiteSpace(req.TerminologySystem) ? "SDKI" : req.TerminologySystem.Trim(),
                IsActive = req.IsActive,
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            _dbContext.MstNursingDiagnoses.Add(diag);
            await _dbContext.SaveChangesAsync(ct);

            return (await GetDetailByIdAsync(diag.Id, ct))!;
        }

        public async Task<NursingDiagnosisDetailResponse> UpdateDiagnosisAsync(
            Guid id,
            UpdateNursingDiagnosisRequest req,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var diag = await _dbContext.MstNursingDiagnoses
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (diag == null)
            {
                throw new KeyNotFoundException("Diagnosis keperawatan tidak ditemukan.");
            }

            var codeExists = await _dbContext.MstNursingDiagnoses
                .AnyAsync(x => x.Id != id && x.Code.ToLower() == req.Code.Trim().ToLower() && !x.IsDelete, ct);

            if (codeExists)
            {
                throw new InvalidOperationException($"Diagnosis dengan kode '{req.Code}' sudah digunakan.");
            }

            diag.GroupId = req.GroupId;
            diag.Code = req.Code.Trim().ToUpperInvariant();
            diag.Name = req.Name.Trim();
            diag.SubCategory = req.SubCategory?.Trim();
            diag.Definition = req.Definition?.Trim();
            diag.TerminologySystem = string.IsNullOrWhiteSpace(req.TerminologySystem) ? "SDKI" : req.TerminologySystem.Trim();
            diag.IsActive = req.IsActive;
            diag.UpdateBy = actorUserId;
            diag.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            return (await GetDetailByIdAsync(diag.Id, ct))!;
        }

        public async Task<bool> DeleteDiagnosisAsync(Guid id, Guid actorUserId, CancellationToken ct = default)
        {
            var diag = await _dbContext.MstNursingDiagnoses
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (diag == null) return false;

            diag.IsDelete = true;
            diag.DeleteBy = actorUserId;
            diag.DeleteDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<NursingDiagnosisInterventionResponse> AddInterventionAsync(
            CreateNursingDiagnosisInterventionRequest req,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var diagExists = await _dbContext.MstNursingDiagnoses
                .AnyAsync(x => x.Id == req.NursingDiagnosisId && !x.IsDelete, ct);

            if (!diagExists)
            {
                throw new KeyNotFoundException("Diagnosis keperawatan tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            var entity = new MstNursingDiagnosisIntervention
            {
                Id = Guid.NewGuid(),
                NursingDiagnosisId = req.NursingDiagnosisId,
                PillarType = req.PillarType,
                InterventionCode = req.InterventionCode.Trim(),
                InterventionName = req.InterventionName.Trim(),
                ActionDescription = req.ActionDescription.Trim(),
                TerminologySystem = string.IsNullOrWhiteSpace(req.TerminologySystem) ? "SIKI" : req.TerminologySystem.Trim(),
                IsDefaultRecommendation = req.IsDefaultRecommendation,
                IsActive = req.IsActive,
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            _dbContext.MstNursingDiagnosisInterventions.Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            return new NursingDiagnosisInterventionResponse
            {
                Id = entity.Id,
                NursingDiagnosisId = entity.NursingDiagnosisId,
                PillarType = entity.PillarType,
                InterventionCode = entity.InterventionCode,
                InterventionName = entity.InterventionName,
                ActionDescription = entity.ActionDescription,
                TerminologySystem = entity.TerminologySystem,
                IsDefaultRecommendation = entity.IsDefaultRecommendation,
                IsActive = entity.IsActive
            };
        }

        public async Task<NursingDiagnosisOutcomeResponse> AddOutcomeAsync(
            CreateNursingDiagnosisOutcomeRequest req,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var diagExists = await _dbContext.MstNursingDiagnoses
                .AnyAsync(x => x.Id == req.NursingDiagnosisId && !x.IsDelete, ct);

            if (!diagExists)
            {
                throw new KeyNotFoundException("Diagnosis keperawatan tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            var entity = new MstNursingDiagnosisOutcome
            {
                Id = Guid.NewGuid(),
                NursingDiagnosisId = req.NursingDiagnosisId,
                OutcomeCode = req.OutcomeCode.Trim(),
                OutcomeName = req.OutcomeName.Trim(),
                Expectation = req.Expectation?.Trim(),
                TerminologySystem = string.IsNullOrWhiteSpace(req.TerminologySystem) ? "SLKI" : req.TerminologySystem.Trim(),
                IsActive = req.IsActive,
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            _dbContext.MstNursingDiagnosisOutcomes.Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            return new NursingDiagnosisOutcomeResponse
            {
                Id = entity.Id,
                NursingDiagnosisId = entity.NursingDiagnosisId,
                OutcomeCode = entity.OutcomeCode,
                OutcomeName = entity.OutcomeName,
                Expectation = entity.Expectation,
                TerminologySystem = entity.TerminologySystem,
                IsActive = entity.IsActive
            };
        }

        public async Task<NursingDiagnosisEtiologyResponse> AddEtiologyAsync(
            CreateNursingDiagnosisEtiologyRequest req,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var diagExists = await _dbContext.MstNursingDiagnoses
                .AnyAsync(x => x.Id == req.NursingDiagnosisId && !x.IsDelete, ct);

            if (!diagExists)
            {
                throw new KeyNotFoundException("Diagnosis keperawatan tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            var entity = new MstNursingDiagnosisEtiology
            {
                Id = Guid.NewGuid(),
                NursingDiagnosisId = req.NursingDiagnosisId,
                EtiologyName = req.EtiologyName.Trim(),
                Description = req.Description?.Trim(),
                IsActive = req.IsActive,
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            _dbContext.MstNursingDiagnosisEtiologies.Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            return new NursingDiagnosisEtiologyResponse
            {
                Id = entity.Id,
                NursingDiagnosisId = entity.NursingDiagnosisId,
                EtiologyName = entity.EtiologyName,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }
    }
}
