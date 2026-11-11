using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan pengelolaan rencana kebutuhan tenaga tahunan dan addendum (BE-HRD-046).
    /// </summary>
    public sealed class HrdManpowerPlanService : IHrdManpowerPlanService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdPositionFormationService _formationService;
        private readonly IHrdStaffingRecommendationService _recommendationService;
        private readonly IHrdWorkforcePlanningCostService _costService;
        private readonly IHrdStaffingGapSnapshotService _snapshotService;
        private readonly IHrdWorkforcePlanningWorkflowIntegrationService _workflowService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        public HrdManpowerPlanService(
            ApplicationDbContext dbContext,
            IHrdPositionFormationService formationService,
            IHrdStaffingRecommendationService recommendationService,
            IHrdWorkforcePlanningCostService costService,
            IHrdStaffingGapSnapshotService snapshotService,
            IHrdWorkforcePlanningWorkflowIntegrationService workflowService,
            NumberSeriesAllocator numberSeriesAllocator)
        {
            _dbContext = dbContext;
            _formationService = formationService;
            _recommendationService = recommendationService;
            _costService = costService;
            _snapshotService = snapshotService;
            _workflowService = workflowService;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

        public async Task<WorkforcePlanningResult<PagedResult<ManpowerPlanListItemResponse>>> GetPagedListAsync(
            ManpowerPlanPagedQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var isCostVisible = _costService.HasCostReadPermission(user);

            var dbQuery = _dbContext.HrdManpowerPlans
                .AsNoTracking()
                .Include(p => p.HospitalSite)
                .Include(p => p.Lines)
                .Where(p => !p.IsDelete);

            if (query.HospitalSiteId.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.PlanYear.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.PlanYear == query.PlanYear.Value);
            }

            if (query.PlanKind.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.PlanKind == query.PlanKind.Value);
            }

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.PlanStatus == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(p => p.PlanNumber.ToLower().Contains(term) ||
                                             (p.Justification != null && p.Justification.ToLower().Contains(term)));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var items = await dbQuery
                .OrderByDescending(p => p.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ManpowerPlanListItemResponse
                {
                    Id = p.Id,
                    PlanNumber = p.PlanNumber,
                    PlanKind = p.PlanKind,
                    HospitalSiteId = p.HospitalSiteId,
                    HospitalSiteName = p.HospitalSite != null ? p.HospitalSite.SiteName : null,
                    PlanYear = p.PlanYear,
                    PeriodStartDate = p.PeriodStartDate,
                    PeriodEndDate = p.PeriodEndDate,
                    ParentPlanId = p.ParentPlanId,
                    AddendumSequence = p.AddendumSequence,
                    PlanStatus = p.PlanStatus,
                    TotalLines = p.Lines.Count(l => !l.IsDelete),
                    TotalEstimatedCost = isCostVisible ? p.TotalEstimatedCost : null,
                    FinanceAdjustedTotal = isCostVisible ? p.FinanceAdjustedTotal : null,
                    CostVisible = isCostVisible,
                    CreatedAt = p.CreateDateTime,
                    RowVersion = p.RowVersion
                })
                .ToListAsync(cancellationToken);

            return WorkforcePlanningResult<PagedResult<ManpowerPlanListItemResponse>>.Ok(
                new PagedResult<ManpowerPlanListItemResponse>
                {
                    Items = items,
                    TotalData = totalCount,
                    TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                "Daftar rencana kebutuhan tenaga berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<ManpowerPlanResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var isCostVisible = _costService.HasCostReadPermission(user);

            var entity = await _dbContext.HrdManpowerPlans
                .AsNoTracking()
                .Include(p => p.HospitalSite)
                .Include(p => p.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.OrganizationUnit)
                .Include(p => p.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.Position)
                .Include(p => p.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.SalaryGrade)
                .FirstOrDefaultAsync(p => !p.IsDelete && p.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Data tidak ditemukan.", 404);
            }

            return WorkforcePlanningResult<ManpowerPlanResponse>.Ok(
                MapToResponse(entity, isCostVisible),
                "Detail rencana kebutuhan tenaga berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<ManpowerPlanResponse>> CreateAnnualPlanAsync(
            CreateAnnualPlanRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            // Validasi: Rencana tahunan kedua untuk RS dan tahun yang sama (VR-WFP-15)
            var activeDuplicate = await _dbContext.HrdManpowerPlans
                .AnyAsync(p => !p.IsDelete &&
                               p.HospitalSiteId == request.HospitalSiteId &&
                               p.PlanYear == request.PlanYear &&
                               p.PlanKind == ManpowerPlanKind.AnnualPlan &&
                               p.PlanStatus != ManpowerPlanStatus.Rejected,
                          cancellationToken);

            if (activeDuplicate)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail(
                    $"Rencana tahunan untuk rumah sakit dan tahun {request.PlanYear} sudah ada. Buat addendum jika ingin melakukan perubahan.", 409);
            }

            string planNumber;
            try
            {
                planNumber = await _numberSeriesAllocator.AllocateAsync(new NumberAllocationRequest(
                    SequenceKey: WorkforcePlanningValueConstants.NumberSeriesKeys.ManpowerPlan,
                    Prefix: "PLN-",
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: 6,
                    ActorUserId: userId,
                    Instant: DateTimeOffset.UtcNow), cancellationToken);
            }
            catch
            {
                planNumber = $"PLN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
            }

            var now = DateTime.UtcNow;
            var plan = new HrdManpowerPlan
            {
                Id = Guid.NewGuid(),
                PlanNumber = planNumber,
                PlanKind = ManpowerPlanKind.AnnualPlan,
                HospitalSiteId = request.HospitalSiteId,
                PlanYear = request.PlanYear,
                PeriodStartDate = request.PeriodStartDate,
                PeriodEndDate = request.PeriodEndDate,
                ReferenceDate = request.ReferenceDate,
                PlanStatus = ManpowerPlanStatus.Draft,
                Justification = request.Justification,
                CurrencyCode = "IDR",
                CreateDateTime = now,
                CreateBy = userId
            };

            foreach (var lineReq in request.Lines)
            {
                plan.Lines.Add(new HrdManpowerPlanLine
                {
                    Id = Guid.NewGuid(),
                    PlanId = plan.Id,
                    OrganizationUnitId = lineReq.OrganizationUnitId,
                    PositionId = lineReq.PositionId,
                    SalaryGradeId = lineReq.SalaryGradeId,
                    ProposedFormationQuantity = lineReq.ProposedFormationQuantity,
                    DeviationReason = lineReq.DeviationReason,
                    EffectiveStartDate = lineReq.EffectiveStartDate,
                    MonthsInPeriod = 12,
                    CreateDateTime = now,
                    CreateBy = userId
                });
            }

            _dbContext.HrdManpowerPlans.Add(plan);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Hitung ulang formasi, rekomendasi, dan biaya
            await InternalRecalculateAsync(plan.Id, user, cancellationToken);

            return await GetByIdAsync(plan.Id, user, cancellationToken);
        }

        public async Task<WorkforcePlanningResult<ManpowerPlanResponse>> CreatePlanAddendumAsync(
            CreatePlanAddendumRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            // Validasi: Addendum hanya dari rencana Approved (VR-WFP-16)
            var parentPlan = await _dbContext.HrdManpowerPlans
                .FirstOrDefaultAsync(p => !p.IsDelete && p.Id == request.ParentPlanId, cancellationToken);

            if (parentPlan == null)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Rencana induk tidak ditemukan.", 404);
            }

            if (parentPlan.PlanStatus != ManpowerPlanStatus.Approved)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Addendum hanya dapat dibuat dari rencana yang telah disetujui (Approved).", 422);
            }

            if (request.Lines == null || request.Lines.Count == 0)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Addendum wajib memuat sekurangnya satu baris perubahan.", 422);
            }

            var nextSeq = await _dbContext.HrdManpowerPlans
                .Where(p => !p.IsDelete && p.ParentPlanId == parentPlan.Id)
                .MaxAsync(p => (int?)p.AddendumSequence, cancellationToken) ?? 0;
            nextSeq += 1;

            string planNumber;
            try
            {
                planNumber = await _numberSeriesAllocator.AllocateAsync(new NumberAllocationRequest(
                    SequenceKey: WorkforcePlanningValueConstants.NumberSeriesKeys.ManpowerPlan,
                    Prefix: "ADD-",
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: 6,
                    ActorUserId: userId,
                    Instant: DateTimeOffset.UtcNow), cancellationToken);
            }
            catch
            {
                planNumber = $"ADD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
            }

            var now = DateTime.UtcNow;
            var addendum = new HrdManpowerPlan
            {
                Id = Guid.NewGuid(),
                PlanNumber = planNumber,
                PlanKind = ManpowerPlanKind.Addendum,
                HospitalSiteId = parentPlan.HospitalSiteId,
                PlanYear = parentPlan.PlanYear,
                PeriodStartDate = parentPlan.PeriodStartDate,
                PeriodEndDate = parentPlan.PeriodEndDate,
                ParentPlanId = parentPlan.Id,
                AddendumSequence = nextSeq,
                ReferenceDate = request.ReferenceDate,
                PlanStatus = ManpowerPlanStatus.Draft,
                Justification = request.Justification,
                CurrencyCode = "IDR",
                CreateDateTime = now,
                CreateBy = userId
            };

            foreach (var lineReq in request.Lines)
            {
                addendum.Lines.Add(new HrdManpowerPlanLine
                {
                    Id = Guid.NewGuid(),
                    PlanId = addendum.Id,
                    OrganizationUnitId = lineReq.OrganizationUnitId,
                    PositionId = lineReq.PositionId,
                    SalaryGradeId = lineReq.SalaryGradeId,
                    ProposedFormationQuantity = lineReq.ProposedFormationQuantity,
                    DeviationReason = lineReq.DeviationReason,
                    EffectiveStartDate = lineReq.EffectiveStartDate,
                    MonthsInPeriod = 12,
                    CreateDateTime = now,
                    CreateBy = userId
                });
            }

            _dbContext.HrdManpowerPlans.Add(addendum);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await InternalRecalculateAsync(addendum.Id, user, cancellationToken);

            return await GetByIdAsync(addendum.Id, user, cancellationToken);
        }

        public async Task<WorkforcePlanningResult<ManpowerPlanResponse>> UpdateDraftAsync(
            Guid id,
            UpdateManpowerPlanRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            var entity = await _dbContext.HrdManpowerPlans
                .Include(p => p.Lines)
                .FirstOrDefaultAsync(p => !p.IsDelete && p.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Data tidak ditemukan.", 404);
            }

            // Validasi: Mengubah rencana Approved (VR-WFP-17)
            if (entity.PlanStatus == ManpowerPlanStatus.Approved)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail(
                    "Rencana yang sudah disetujui (Approved) terkunci dan tidak dapat diubah. Buat addendum untuk melakukan perubahan.", 409);
            }

            if (entity.PlanStatus != ManpowerPlanStatus.Draft &&
                entity.PlanStatus != ManpowerPlanStatus.Returned)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Hanya draf atau rencana yang dikembalikan yang dapat diubah.", 400);
            }

            entity.Justification = request.Justification;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;
            entity.RowVersion = Guid.NewGuid();

            foreach (var existingLine in entity.Lines)
            {
                existingLine.IsDelete = true;
            }

            var now = DateTime.UtcNow;
            foreach (var lineReq in request.Lines)
            {
                entity.Lines.Add(new HrdManpowerPlanLine
                {
                    Id = Guid.NewGuid(),
                    PlanId = entity.Id,
                    OrganizationUnitId = lineReq.OrganizationUnitId,
                    PositionId = lineReq.PositionId,
                    SalaryGradeId = lineReq.SalaryGradeId,
                    ProposedFormationQuantity = lineReq.ProposedFormationQuantity,
                    DeviationReason = lineReq.DeviationReason,
                    EffectiveStartDate = lineReq.EffectiveStartDate,
                    MonthsInPeriod = 12,
                    CreateDateTime = now,
                    CreateBy = userId
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await InternalRecalculateAsync(entity.Id, user, cancellationToken);

            return await GetByIdAsync(entity.Id, user, cancellationToken);
        }

        public async Task<WorkforcePlanningResult<ManpowerPlanResponse>> RecalculateAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.HrdManpowerPlans
                .FirstOrDefaultAsync(p => !p.IsDelete && p.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Data tidak ditemukan.", 404);
            }

            if (entity.PlanStatus != ManpowerPlanStatus.Draft &&
                entity.PlanStatus != ManpowerPlanStatus.Returned)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Hanya draf rencana yang dapat dihitung ulang.", 400);
            }

            await InternalRecalculateAsync(id, user, cancellationToken);
            return await GetByIdAsync(id, user, cancellationToken);
        }

        public async Task<WorkforcePlanningResult<ManpowerPlanResponse>> SubmitAsync(
            Guid id,
            SubmitManpowerPlanRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var entity = await _dbContext.HrdManpowerPlans
                    .Include(p => p.Lines.Where(l => !l.IsDelete))
                    .FirstOrDefaultAsync(p => !p.IsDelete && p.Id == id, cancellationToken);

                if (entity == null)
                {
                    return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Data tidak ditemukan.", 404);
                }

                if (entity.PlanStatus != ManpowerPlanStatus.Draft &&
                    entity.PlanStatus != ManpowerPlanStatus.Returned)
                {
                    return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Hanya draf atau rencana yang dikembalikan yang dapat diajukan.", 400);
                }

                // Validasi baris rencana (VR-WFP-18, VR-WFP-19)
                foreach (var line in entity.Lines)
                {
                    if (line.SalaryGradeId == Guid.Empty)
                    {
                        return WorkforcePlanningResult<ManpowerPlanResponse>.Fail(
                            $"Baris unit {line.OrganizationUnitId} jabatan {line.PositionId} wajib memilih kelas gaji (SalaryGradeId).", 422);
                    }

                    if (line.RecommendedQuantity.HasValue &&
                        line.ProposedFormationQuantity != line.RecommendedQuantity.Value &&
                        string.IsNullOrWhiteSpace(line.DeviationReason))
                    {
                        return WorkforcePlanningResult<ManpowerPlanResponse>.Fail(
                            $"Usulan formasi ({line.ProposedFormationQuantity}) berbeda dari rekomendasi ({line.RecommendedQuantity.Value}) pada suatu baris tanpa mencantumkan alasan penyimpangan.", 422);
                    }
                }

                // Rekam potret gap pengajuan (BE-HRD-039)
                await _snapshotService.CaptureSubmissionSnapshotAsync(
                    entity.HospitalSiteId,
                    null,
                    PositionFormationSourceType.ManpowerPlan,
                    entity.Id,
                    entity.ReferenceDate,
                    userId,
                    cancellationToken);

                // Inisiasi alur workflow persetujuan T10
                var workflowResult = await _workflowService.StartWorkflowAsync(new StartWorkforcePlanningWorkflowRequest
                {
                    WorkflowDefinitionCode = "T10",
                    ReferenceType = WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceManpowerPlan,
                    ReferenceId = entity.Id,
                    DocumentNumber = entity.PlanNumber,
                    SubmitComment = "Pengajuan Rencana Formasi Kebutuhan Tenaga",
                    AutoSubmit = true
                }, cancellationToken);

                if (!workflowResult.Success)
                {
                    return WorkforcePlanningResult<ManpowerPlanResponse>.Fail(
                        $"Gagal mendaftarkan alur persetujuan rencana: {workflowResult.Message}", workflowResult.StatusCode);
                }

                entity.WorkflowInstanceId = workflowResult.Data?.Id;
                entity.PlanStatus = ManpowerPlanStatus.Submitted;
                entity.SubmittedByUserId = userId;
                entity.SubmittedAt = DateTime.UtcNow;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = userId;
                entity.RowVersion = Guid.NewGuid();

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetByIdAsync(entity.Id, user, cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<WorkforcePlanningResult<ManpowerPlanResponse>> FinanceAdjustmentAsync(
            Guid id,
            FinancePlanAdjustmentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            if (!_costService.HasCostReadPermission(user))
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail(
                    "Hanya peninjau Keuangan (Finance) yang berwenang mengoreksi total biaya rencana.", 403);
            }

            var entity = await _dbContext.HrdManpowerPlans
                .FirstOrDefaultAsync(p => !p.IsDelete && p.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Data tidak ditemukan.", 404);
            }

            if (entity.PlanStatus != ManpowerPlanStatus.Submitted)
            {
                return WorkforcePlanningResult<ManpowerPlanResponse>.Fail("Koreksi total biaya hanya dapat dilakukan saat rencana dalam tahap persetujuan.", 400);
            }

            entity.FinanceAdjustedTotal = request.AdjustedTotal;
            entity.FinanceAdjustmentReason = request.Reason;
            entity.FinanceAdjustedByUserId = userId;
            entity.FinanceAdjustedAt = DateTime.UtcNow;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;
            entity.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, user, cancellationToken);
        }

        public async Task<WorkforcePlanningResult<List<StaffingGapSnapshotListItemResponse>>> GetGapSnapshotsAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var snapshots = await _dbContext.HrdStaffingGapSnapshots
                .AsNoTracking()
                .Include(s => s.HospitalSite)
                .Include(s => s.Lines)
                .Where(s => !s.IsDelete &&
                            s.SourceDocumentType == PositionFormationSourceType.ManpowerPlan &&
                            s.SourceDocumentId == id)
                .OrderByDescending(s => s.TakenAt)
                .Select(s => new StaffingGapSnapshotListItemResponse
                {
                    Id = s.Id,
                    SnapshotType = s.SnapshotType,
                    HospitalSiteId = s.HospitalSiteId,
                    HospitalSiteName = s.HospitalSite != null ? s.HospitalSite.SiteName : null,
                    ReferenceDate = s.ReferenceDate,
                    SourceDocumentType = s.SourceDocumentType,
                    SourceDocumentId = s.SourceDocumentId,
                    PeriodMonth = s.PeriodMonth,
                    TakenAt = s.TakenAt,
                    ReconstructedAt = s.ReconstructedAt,
                    TotalLines = s.Lines.Count(l => !l.IsDelete)
                })
                .ToListAsync(cancellationToken);

            return WorkforcePlanningResult<List<StaffingGapSnapshotListItemResponse>>.Ok(
                snapshots,
                "Potret gap rencana berhasil diambil.");
        }

        public ManpowerPlanFilterMetadataResponse GetFilterMetadata()
        {
            return new ManpowerPlanFilterMetadataResponse
            {
                Statuses = Enum.GetNames<ManpowerPlanStatus>().ToList(),
                PlanKinds = Enum.GetNames<ManpowerPlanKind>().ToList()
            };
        }

        private async Task InternalRecalculateAsync(
            Guid planId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var plan = await _dbContext.HrdManpowerPlans
                .Include(p => p.Lines.Where(l => !l.IsDelete))
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null) return;

            decimal totalEstimated = 0m;

            foreach (var line in plan.Lines)
            {
                // 1. Ambil formasi yang berlaku saat ini
                var formation = await _formationService.GetEffectiveFormationForPositionAsync(
                    line.OrganizationUnitId,
                    line.PositionId,
                    plan.ReferenceDate,
                    cancellationToken);

                line.CurrentFormationQuantity = formation?.FormationQuantity ?? 0m;
                line.ChangeQuantity = line.ProposedFormationQuantity - line.CurrentFormationQuantity;

                // 2. Ambil rekomendasi standar kebutuhan tenaga
                var standard = await _dbContext.HrdStaffingStandards
                    .AsNoTracking()
                    .Include(s => s.Lines.Where(l => !l.IsDelete))
                    .FirstOrDefaultAsync(s => !s.IsDelete &&
                                              s.OrganizationUnitId == line.OrganizationUnitId &&
                                              s.StandardStatus == StaffingStandardStatus.Active &&
                                              s.EffectiveStartDate <= plan.ReferenceDate &&
                                              (!s.EffectiveEndDate.HasValue || s.EffectiveEndDate.Value >= plan.ReferenceDate),
                                          cancellationToken);

                if (standard != null)
                {
                    var rec = await _recommendationService.CalculateRecommendationAsync(standard, plan.PeriodStartDate, cancellationToken);
                    var lineRec = rec.Lines.FirstOrDefault(r => r.PositionId == line.PositionId);
                    line.RecommendedQuantity = lineRec?.RecommendedQuantity;
                    line.RecommendationBreakdown = lineRec?.Breakdown;
                }

                // 3. Hitung estimasi biaya
                var cost = await _costService.CalculateEstimatedCostAsync(
                    line.SalaryGradeId,
                    line.ProposedFormationQuantity,
                    line.MonthsInPeriod > 0 ? line.MonthsInPeriod : 12,
                    plan.ReferenceDate,
                    user,
                    cancellationToken);

                line.MonthlyRateSnapshot = cost.MidpointSalary;
                line.EstimatedCost = cost.EstimatedCost;
                line.IncrementalCost = line.ChangeQuantity > 0 && cost.MidpointSalary.HasValue
                    ? line.ChangeQuantity * cost.MidpointSalary.Value * (line.MonthsInPeriod > 0 ? line.MonthsInPeriod : 12)
                    : 0m;
                line.CostStatus = cost.RateStatus == CostRateStatus.Available
                    ? CostCalculationStatus.Calculated
                    : CostCalculationStatus.RateUnavailable;

                if (line.EstimatedCost.HasValue)
                {
                    totalEstimated += line.EstimatedCost.Value;
                }
            }

            plan.TotalEstimatedCost = totalEstimated;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private ManpowerPlanResponse MapToResponse(HrdManpowerPlan entity, bool isCostVisible)
        {
            return new ManpowerPlanResponse
            {
                Id = entity.Id,
                PlanNumber = entity.PlanNumber,
                PlanKind = entity.PlanKind,
                HospitalSiteId = entity.HospitalSiteId,
                HospitalSiteName = entity.HospitalSite?.SiteName,
                PlanYear = entity.PlanYear,
                PeriodStartDate = entity.PeriodStartDate,
                PeriodEndDate = entity.PeriodEndDate,
                ParentPlanId = entity.ParentPlanId,
                AddendumSequence = entity.AddendumSequence,
                ReferenceDate = entity.ReferenceDate,
                PlanStatus = entity.PlanStatus,
                Justification = entity.Justification,
                CurrencyCode = entity.CurrencyCode,
                TotalEstimatedCost = isCostVisible ? entity.TotalEstimatedCost : null,
                FinanceAdjustedTotal = isCostVisible ? entity.FinanceAdjustedTotal : null,
                FinanceAdjustmentReason = isCostVisible ? entity.FinanceAdjustmentReason : null,
                FinanceAdjustedAt = entity.FinanceAdjustedAt,
                CostVisible = isCostVisible,
                WorkflowInstanceId = entity.WorkflowInstanceId,
                SubmittedAt = entity.SubmittedAt,
                ApprovedAt = entity.ApprovedAt,
                RowVersion = entity.RowVersion,
                Lines = entity.Lines
                    .Where(l => !l.IsDelete)
                    .Select(l => new ManpowerPlanLineResponse
                    {
                        Id = l.Id,
                        OrganizationUnitId = l.OrganizationUnitId,
                        OrganizationUnitCode = l.OrganizationUnit?.UnitCode,
                        OrganizationUnitName = l.OrganizationUnit?.UnitName,
                        PositionId = l.PositionId,
                        PositionCode = l.Position?.PositionCode,
                        PositionName = l.Position?.PositionName,
                        SalaryGradeId = l.SalaryGradeId,
                        SalaryGradeName = l.SalaryGrade?.SalaryGradeName,
                        CurrentFormationQuantity = l.CurrentFormationQuantity,
                        RecommendedQuantity = l.RecommendedQuantity,
                        RecommendationBreakdown = l.RecommendationBreakdown,
                        ProposedFormationQuantity = l.ProposedFormationQuantity,
                        ChangeQuantity = l.ChangeQuantity,
                        DeviationReason = l.DeviationReason,
                        EffectiveStartDate = l.EffectiveStartDate,
                        MonthlyRateSnapshot = isCostVisible ? l.MonthlyRateSnapshot : null,
                        MonthsInPeriod = l.MonthsInPeriod,
                        EstimatedCost = isCostVisible ? l.EstimatedCost : null,
                        IncrementalCost = isCostVisible ? l.IncrementalCost : null,
                        CostStatus = l.CostStatus,
                        CostVisible = isCostVisible
                    })
                    .ToList()
            };
        }

        private static Guid GetCurrentUserId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
