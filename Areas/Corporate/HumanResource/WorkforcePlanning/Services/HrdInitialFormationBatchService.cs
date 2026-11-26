using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
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
    /// Layanan bisnis pengelolaan batch formasi awal rumah sakit (BE-HRD-033).
    /// Memenuhi HRD-DEC-070, HRD-DEC-064 (T10), FR-HRD-101, VR-WFP-11 s.d. VR-WFP-14, NFR-011, dan NFR-012.
    /// </summary>
    public sealed class HrdInitialFormationBatchService : IHrdInitialFormationBatchService
    {
        private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png"
        };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB (VR-WFP-14)

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberAllocator;
        private readonly IHrdWorkforcePlanningWorkflowIntegrationService _workflowIntegrationService;
        private readonly ILogger<HrdInitialFormationBatchService> _logger;

        public HrdInitialFormationBatchService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberAllocator,
            IHrdWorkforcePlanningWorkflowIntegrationService workflowIntegrationService,
            ILogger<HrdInitialFormationBatchService> logger)
        {
            _dbContext = dbContext;
            _numberAllocator = numberAllocator;
            _workflowIntegrationService = workflowIntegrationService;
            _logger = logger;
        }

        public byte[] GenerateTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template Formasi Awal");

            worksheet.Cell(1, 1).Value = "OrganizationUnitId";
            worksheet.Cell(1, 2).Value = "PositionId";
            worksheet.Cell(1, 3).Value = "FormationQuantity";
            worksheet.Cell(1, 4).Value = "EffectiveStartDate";

            // Baris petunjuk contoh
            worksheet.Cell(2, 1).Value = "00000000-0000-0000-0000-000000000000";
            worksheet.Cell(2, 2).Value = "00000000-0000-0000-0000-000000000000";
            worksheet.Cell(2, 3).Value = 5;
            worksheet.Cell(2, 4).Value = DateTime.UtcNow.ToString("yyyy-MM-dd");

            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<List<InitialFormationBatchRowValidationResponse>> ValidateRowsAsync(
            List<InitialFormationBatchRowRequest> rows,
            Guid? currentBatchId = null,
            CancellationToken cancellationToken = default)
        {
            if (rows == null || rows.Count == 0)
            {
                return new List<InitialFormationBatchRowValidationResponse>();
            }

            var unitIds = rows.Select(r => r.OrganizationUnitId).Where(id => id != Guid.Empty).Distinct().ToList();
            var positionIds = rows.Select(r => r.PositionId).Where(id => id != Guid.Empty).Distinct().ToList();

            var unitMap = await _dbContext.MstOrganizationUnits.AsNoTracking()
                .Where(u => unitIds.Contains(u.Id) && !u.IsDelete)
                .ToDictionaryAsync(u => u.Id, u => u.UnitName, cancellationToken);

            var positionMap = await _dbContext.MstPositions.AsNoTracking()
                .Where(p => positionIds.Contains(p.Id) && !p.IsDelete)
                .ToDictionaryAsync(p => p.Id, p => p.PositionName, cancellationToken);

            // Formasi existing di buku formasi (VR-WFP-11)
            var existingFormations = await _dbContext.HrdPositionFormations.AsNoTracking()
                .Where(f => !f.IsDelete && unitIds.Contains(f.OrganizationUnitId) && positionIds.Contains(f.PositionId))
                .Select(f => new { f.OrganizationUnitId, f.PositionId })
                .Distinct()
                .ToListAsync(cancellationToken);

            var existingFormationSet = new HashSet<string>(
                existingFormations.Select(f => $"{f.OrganizationUnitId}:{f.PositionId}"),
                StringComparer.OrdinalIgnoreCase);

            // Formasi pada batch lain yang sedang aktif (Draft, Submitted, Approved, Applied)
            var otherBatchRowsQuery = _dbContext.HrdInitialFormationBatchRows.AsNoTracking()
                .Where(r => !r.IsDelete &&
                            unitIds.Contains(r.OrganizationUnitId) &&
                            positionIds.Contains(r.PositionId) &&
                            r.Batch!.BatchStatus != InitialFormationBatchStatus.Rejected &&
                            r.Batch.BatchStatus != InitialFormationBatchStatus.Cancelled);

            if (currentBatchId.HasValue && currentBatchId.Value != Guid.Empty)
            {
                otherBatchRowsQuery = otherBatchRowsQuery.Where(r => r.BatchId != currentBatchId.Value);
            }

            var otherBatchRows = await otherBatchRowsQuery
                .Select(r => new { r.OrganizationUnitId, r.PositionId })
                .Distinct()
                .ToListAsync(cancellationToken);

            var otherBatchSet = new HashSet<string>(
                otherBatchRows.Select(r => $"{r.OrganizationUnitId}:{r.PositionId}"),
                StringComparer.OrdinalIgnoreCase);

            var seenInCurrentBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<InitialFormationBatchRowValidationResponse>();

            foreach (var row in rows)
            {
                var pairKey = $"{row.OrganizationUnitId}:{row.PositionId}";
                var unitName = unitMap.TryGetValue(row.OrganizationUnitId, out var uName) ? uName : null;
                var posName = positionMap.TryGetValue(row.PositionId, out var pName) ? pName : null;

                bool isValid = true;
                string? validationMessage = null;

                if (row.OrganizationUnitId == Guid.Empty || unitName == null)
                {
                    isValid = false;
                    validationMessage = "Unit organisasi tidak ditemukan atau tidak aktif.";
                }
                else if (row.PositionId == Guid.Empty || posName == null)
                {
                    isValid = false;
                    validationMessage = "Jabatan tidak ditemukan atau tidak aktif.";
                }
                else if (row.FormationQuantity <= 0)
                {
                    isValid = false;
                    validationMessage = "Jumlah formasi harus lebih besar dari 0.";
                }
                else if (row.EffectiveStartDate == default)
                {
                    isValid = false;
                    validationMessage = "Tanggal mulai berlaku wajib diisi.";
                }
                else if (seenInCurrentBatch.Contains(pairKey))
                {
                    isValid = false;
                    validationMessage = "Duplikasi pasangan unit organisasi dan jabatan dalam satu batch.";
                }
                else if (existingFormationSet.Contains(pairKey))
                {
                    isValid = false;
                    validationMessage = "Pasangan unit dan jabatan ini sudah memiliki formasi di sistem (VR-WFP-11).";
                }
                else if (otherBatchSet.Contains(pairKey))
                {
                    isValid = false;
                    validationMessage = "Pasangan unit dan jabatan sudah terdapat dalam batch lain yang sedang aktif (VR-WFP-11).";
                }

                if (row.OrganizationUnitId != Guid.Empty && row.PositionId != Guid.Empty)
                {
                    seenInCurrentBatch.Add(pairKey);
                }

                results.Add(new InitialFormationBatchRowValidationResponse
                {
                    OrganizationUnitId = row.OrganizationUnitId,
                    OrganizationUnitName = unitName,
                    PositionId = row.PositionId,
                    PositionName = posName,
                    FormationQuantity = row.FormationQuantity,
                    EffectiveStartDate = row.EffectiveStartDate,
                    IsValid = isValid,
                    ValidationMessage = validationMessage
                });
            }

            return results;
        }

        public async Task<PagedResult<InitialFormationBatchListItemResponse>> GetPagedListAsync(
            InitialFormationBatchPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _dbContext.HrdInitialFormationBatches.AsNoTracking()
                .Where(b => !b.IsDelete);

            if (query.HospitalSiteId.HasValue && query.HospitalSiteId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(b => b.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.Status.HasValue)
            {
                baseQuery = baseQuery.Where(b => b.BatchStatus == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                baseQuery = baseQuery.Where(b =>
                    b.BatchNumber.ToLower().Contains(search) ||
                    b.SourceDocumentReference.ToLower().Contains(search) ||
                    (b.Notes != null && b.Notes.ToLower().Contains(search)));
            }

            var totalData = await baseQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, 100);

            var batches = await baseQuery
                .OrderByDescending(b => b.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var siteIds = batches.Select(b => b.HospitalSiteId).Distinct().ToList();
            var sites = await _dbContext.MstHospitalSites.AsNoTracking()
                .Where(s => siteIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.SiteName, cancellationToken);

            var userIds = batches.Where(b => b.SubmittedByUserId.HasValue)
                .Select(b => b.SubmittedByUserId!.Value)
                .Distinct()
                .ToList();

            var users = userIds.Count > 0
                ? await _dbContext.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? string.Empty, cancellationToken)
                : new Dictionary<Guid, string>();

            var batchIds = batches.Select(b => b.Id).ToList();
            var workflowLookup = batchIds.Count > 0
                ? (await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking()
                    .Where(w => !w.IsDelete &&
                                w.ReferenceType == WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch &&
                                batchIds.Contains(w.ReferenceId))
                    .OrderByDescending(w => w.CreateDateTime)
                    .ToListAsync(cancellationToken))
                    .GroupBy(w => w.ReferenceId)
                    .ToDictionary(g => g.Key, g => g.First())
                : new Dictionary<Guid, TrxWorkflowInstance>();

            var items = batches.Select(b =>
            {
                workflowLookup.TryGetValue(b.Id, out var wf);
                var integrityIssue = b.BatchStatus == InitialFormationBatchStatus.Submitted &&
                    (!b.WorkflowInstanceId.HasValue || wf == null || wf.Id != b.WorkflowInstanceId.Value);

                return new InitialFormationBatchListItemResponse
                {
                    Id = b.Id,
                    BatchNumber = b.BatchNumber,
                    HospitalSiteId = b.HospitalSiteId,
                    HospitalSiteName = sites.TryGetValue(b.HospitalSiteId, out var sName) ? sName : null,
                    SourceDocumentReference = b.SourceDocumentReference,
                    SourceDocumentFileName = b.SourceDocumentFileName,
                    BatchStatus = b.BatchStatus,
                    TotalRows = b.TotalRows,
                    ValidRows = b.ValidRows,
                    InvalidRows = b.InvalidRows,
                    SubmittedAt = b.SubmittedAt,
                    SubmittedByUserName = b.SubmittedByUserId.HasValue && users.TryGetValue(b.SubmittedByUserId.Value, out var uName) ? uName : null,
                    AppliedAt = b.AppliedAt,
                    WorkflowInstanceId = b.WorkflowInstanceId ?? wf?.Id,
                    WorkflowNumber = wf?.RequestNumber,
                    WorkflowStatus = wf?.WorkflowStatus,
                    HasWorkflowIntegrityIssue = integrityIssue,
                    CreatedAt = b.CreateDateTime,
                    RowVersion = b.RowVersion
                };
            }).ToList();

            return new PagedResult<InitialFormationBatchListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<InitialFormationBatchResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialFormationBatches.AsNoTracking()
                .Include(b => b.HospitalSite)
                .Include(b => b.Rows)
                    .ThenInclude(r => r.OrganizationUnit)
                .Include(b => b.Rows)
                    .ThenInclude(r => r.Position)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return null;
            }

            var userIds = new List<Guid>();
            if (batch.SubmittedByUserId.HasValue) userIds.Add(batch.SubmittedByUserId.Value);
            if (batch.ApprovedByUserId.HasValue) userIds.Add(batch.ApprovedByUserId.Value);

            var users = userIds.Count > 0
                ? await _dbContext.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? string.Empty, cancellationToken)
                : new Dictionary<Guid, string>();

            TrxWorkflowInstance? workflow = null;
            if (batch.WorkflowInstanceId.HasValue)
            {
                workflow = await LoadTrackedWorkflowDetailAsync(
                    w => w.Id == batch.WorkflowInstanceId.Value && !w.IsDelete,
                    cancellationToken);
            }

            if (workflow == null)
            {
                workflow = await LoadTrackedWorkflowDetailAsync(
                    w => !w.IsDelete &&
                         w.ReferenceType == WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch &&
                         w.ReferenceId == batch.Id,
                    cancellationToken);
            }

            var activeSteps = workflow?.StepInstances
                .Where(s => !s.IsDelete)
                .OrderBy(s => s.StepOrder)
                .ToList() ?? new List<TrxWorkflowStepInstance>();

            var currentStep = activeSteps.FirstOrDefault(s => s.IsCurrentStep)
                ?? activeSteps.FirstOrDefault(s => string.Equals(s.StepStatus, WorkflowValueConstants.StepStatus.InProgress, StringComparison.OrdinalIgnoreCase));

            var approverList = await MapApproversAsync(activeSteps, cancellationToken);
            var historyList = MapWorkflowHistories(workflow, activeSteps);

            var (hasIntegrityIssue, integrityMessage) = EvaluateWorkflowIntegrity(batch, workflow, currentStep);

            return new InitialFormationBatchResponse
            {
                Id = batch.Id,
                BatchNumber = batch.BatchNumber,
                HospitalSiteId = batch.HospitalSiteId,
                HospitalSiteName = batch.HospitalSite?.SiteName,
                SourceDocumentReference = batch.SourceDocumentReference,
                SourceDocumentPath = batch.SourceDocumentPath,
                SourceDocumentFileName = batch.SourceDocumentFileName,
                BatchStatus = batch.BatchStatus,
                TotalRows = batch.TotalRows,
                ValidRows = batch.ValidRows,
                InvalidRows = batch.InvalidRows,
                Notes = batch.Notes,
                WorkflowInstanceId = batch.WorkflowInstanceId,
                WorkflowDefinitionId = batch.WorkflowDefinitionId ?? workflow?.WorkflowDefinitionId,
                WorkflowNumber = workflow?.RequestNumber,
                WorkflowStatus = workflow?.WorkflowStatus,
                CurrentStepOrder = currentStep?.StepOrder ?? (workflow != null && workflow.CurrentStepOrder > 0 ? workflow.CurrentStepOrder : null),
                CurrentStepCode = currentStep?.StepCodeSnapshot ?? workflow?.CurrentStepCode,
                CurrentStepName = currentStep?.StepNameSnapshot,
                CurrentStepStatus = currentStep?.StepStatus,
                HasWorkflowIntegrityIssue = hasIntegrityIssue,
                WorkflowIntegrityMessage = integrityMessage,
                SubmittedAt = batch.SubmittedAt,
                SubmittedByUserName = batch.SubmittedByUserId.HasValue && users.TryGetValue(batch.SubmittedByUserId.Value, out var sUser) ? sUser : null,
                ApprovedAt = batch.ApprovedAt,
                ApprovedByUserName = batch.ApprovedByUserId.HasValue && users.TryGetValue(batch.ApprovedByUserId.Value, out var aUser) ? aUser : null,
                AppliedAt = batch.AppliedAt,
                CreatedAt = batch.CreateDateTime,
                RowVersion = batch.RowVersion,
                Rows = batch.Rows.Where(r => !r.IsDelete).Select(r => new InitialFormationBatchRowResponse
                {
                    Id = r.Id,
                    OrganizationUnitId = r.OrganizationUnitId,
                    OrganizationUnitCode = r.OrganizationUnit?.UnitCode,
                    OrganizationUnitName = r.OrganizationUnit?.UnitName,
                    PositionId = r.PositionId,
                    PositionCode = r.Position?.PositionCode,
                    PositionName = r.Position?.PositionName,
                    FormationQuantity = r.FormationQuantity,
                    EffectiveStartDate = r.EffectiveStartDate,
                    IsValid = r.IsValid,
                    ValidationMessage = r.ValidationMessage,
                    CreatedFormationId = r.CreatedFormationId
                }).ToList(),
                AssignedApprovers = approverList,
                WorkflowHistories = historyList
            };
        }

        private Task<TrxWorkflowInstance?> LoadTrackedWorkflowDetailAsync(
            System.Linq.Expressions.Expression<Func<TrxWorkflowInstance, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.ApproverAssignments)
                        .ThenInclude(a => a.AssignedApproverUser)
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.ApproverAssignments)
                        .ThenInclude(a => a.AssignedApproverWorkforceProfile)
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.ApproverAssignments)
                        .ThenInclude(a => a.OriginalApproverUser)
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.ApproverAssignments)
                        .ThenInclude(a => a.OriginalApproverWorkforceProfile)
                .Include(w => w.StatusHistories)
                    .ThenInclude(h => h.ChangedByUser)
                .Include(w => w.StatusHistories)
                    .ThenInclude(h => h.ChangedByWorkforceProfile)
                .OrderByDescending(w => w.CreateDateTime)
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }

        private async Task<List<InitialFormationBatchWorkflowApproverDto>> MapApproversAsync(
            List<TrxWorkflowStepInstance> steps,
            CancellationToken cancellationToken)
        {
            var profileIds = steps
                .SelectMany(s => s.ApproverAssignments)
                .Where(a => !a.IsDelete && a.AssignedApproverWorkforceProfileId.HasValue)
                .Select(a => a.AssignedApproverWorkforceProfileId!.Value)
                .Distinct()
                .ToList();

            var positionNamesByProfileId = profileIds.Count > 0
                ? await _dbContext.MstWorkforceProfiles
                    .AsNoTracking()
                    .Where(wp => profileIds.Contains(wp.Id) && wp.PrimaryPositionId.HasValue)
                    .Join(
                        _dbContext.MstPositions.AsNoTracking(),
                        wp => wp.PrimaryPositionId!.Value,
                        p => p.Id,
                        (wp, p) => new { wp.Id, p.PositionName })
                    .ToDictionaryAsync(x => x.Id, x => x.PositionName, cancellationToken)
                : new Dictionary<Guid, string>();

            var result = new List<InitialFormationBatchWorkflowApproverDto>();
            foreach (var step in steps)
            {
                foreach (var assign in step.ApproverAssignments.Where(a => !a.IsDelete && a.IsActive).OrderBy(a => a.AssignmentOrder))
                {
                    var name = !string.IsNullOrWhiteSpace(assign.AssignedApproverWorkforceProfile?.DisplayName)
                        ? assign.AssignedApproverWorkforceProfile.DisplayName
                        : !string.IsNullOrWhiteSpace(assign.AssignedApproverUser?.DisplayName)
                            ? assign.AssignedApproverUser.DisplayName
                            : assign.AssignedApproverUser?.UserName ?? assign.AssignedApproverRoleCode ?? "Approver";

                    var posName = assign.AssignedApproverWorkforceProfileId.HasValue &&
                        positionNamesByProfileId.TryGetValue(assign.AssignedApproverWorkforceProfileId.Value, out var pName)
                        ? pName
                        : null;

                    var fullName = !string.IsNullOrWhiteSpace(posName) && !name.Contains(posName, StringComparison.OrdinalIgnoreCase)
                        ? $"{name} ({posName})"
                        : name;

                    var originalName = assign.IsDelegated
                        ? (!string.IsNullOrWhiteSpace(assign.OriginalApproverWorkforceProfile?.DisplayName)
                            ? assign.OriginalApproverWorkforceProfile.DisplayName
                            : assign.OriginalApproverUser?.DisplayName ?? assign.OriginalApproverUser?.UserName)
                        : null;

                    result.Add(new InitialFormationBatchWorkflowApproverDto
                    {
                        AssignmentId = assign.Id,
                        StepOrder = step.StepOrder,
                        StepCode = step.StepCodeSnapshot,
                        StepName = step.StepNameSnapshot,
                        StepStatus = step.StepStatus,
                        IsCurrentStep = step.IsCurrentStep,
                        ApproverUserId = assign.AssignedApproverUserId,
                        ApproverName = fullName,
                        ApproverRoleCode = assign.AssignedApproverRoleCode ?? posName,
                        AssignmentStatus = assign.AssignmentStatus,
                        IsCurrentAssignment = assign.IsCurrentAssignment,
                        IsDelegated = assign.IsDelegated,
                        OriginalApproverName = originalName,
                        DueAt = assign.DueAt,
                        CompletedAt = assign.CompletedAt
                    });
                }
            }

            return result;
        }

        private static List<InitialFormationBatchWorkflowHistoryDto> MapWorkflowHistories(
            TrxWorkflowInstance? workflow,
            List<TrxWorkflowStepInstance> steps)
        {
            if (workflow == null)
            {
                return new List<InitialFormationBatchWorkflowHistoryDto>();
            }

            var stepNamesById = steps.ToDictionary(s => s.Id, s => s.StepNameSnapshot);
            return workflow.StatusHistories
                .Where(h => !h.IsDelete)
                .OrderBy(h => h.SequenceNumber)
                .ThenBy(h => h.ChangedAt)
                .Select(h => new InitialFormationBatchWorkflowHistoryDto
                {
                    SequenceNumber = h.SequenceNumber,
                    StepName = h.WorkflowStepInstanceId.HasValue && stepNamesById.TryGetValue(h.WorkflowStepInstanceId.Value, out var sName) ? sName : null,
                    ActionType = h.ActionType,
                    ActorName = !string.IsNullOrWhiteSpace(h.ChangedByWorkforceProfile?.DisplayName)
                        ? h.ChangedByWorkforceProfile.DisplayName
                        : !string.IsNullOrWhiteSpace(h.ChangedByUser?.DisplayName)
                            ? h.ChangedByUser.DisplayName
                            : h.ChangedByUser?.UserName,
                    FromStatus = h.FromWorkflowStatus,
                    ToStatus = h.ToWorkflowStatus,
                    Comment = h.Comment,
                    ActionAt = h.ChangedAt,
                    IsSystemGenerated = h.IsSystemGenerated
                })
                .ToList();
        }

        private static (bool HasIssue, string? Message) EvaluateWorkflowIntegrity(
            HrdInitialFormationBatch batch,
            TrxWorkflowInstance? workflow,
            TrxWorkflowStepInstance? currentStep)
        {
            if (batch.BatchStatus != InitialFormationBatchStatus.Submitted)
            {
                return (false, null);
            }

            if (workflow == null)
            {
                return (true, "Batch berstatus Menunggu Persetujuan (Submitted), tetapi WorkflowInstanceId kosong dan belum terdapat instans workflow T10 di engine persetujuan. Jalankan pemulihan agar batch dapat dikembalikan ke Draft dan diajukan ulang.");
            }

            if (!batch.WorkflowInstanceId.HasValue || batch.WorkflowInstanceId.Value != workflow.Id)
            {
                return (true, $"Batch belum tertaut pada instans workflow {workflow.RequestNumber} (status engine: {workflow.WorkflowStatus}). Jalankan pemulihan untuk rekonsiliasi atau pengembalian ke Draft.");
            }

            var isEngineActive = string.Equals(workflow.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Submitted, StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(workflow.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.InProgress, StringComparison.OrdinalIgnoreCase);

            if (isEngineActive && (currentStep == null || !currentStep.ApproverAssignments.Any(a => !a.IsDelete && a.IsActive)))
            {
                return (true, $"Workflow {workflow.RequestNumber} berstatus {workflow.WorkflowStatus}, tetapi tahap aktif belum memiliki penugasan approver.");
            }

            return (false, null);
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> CreateDraftAsync(
            CreateInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pembuatan batch tidak boleh kosong.");
            }

            var siteExists = await _dbContext.MstHospitalSites.AsNoTracking()
                .AnyAsync(s => s.Id == request.HospitalSiteId && !s.IsDelete, cancellationToken);

            if (!siteExists)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Rumah sakit tidak ditemukan atau tidak aktif.");
            }

            if (string.IsNullOrWhiteSpace(request.SourceDocumentReference))
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Nomor atau rujukan dokumen sumber SK wajib diisi.");
            }

            string batchNumber;
            try
            {
                batchNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                    SequenceKey: WorkforcePlanningValueConstants.NumberSeriesKeys.InitialFormationBatch,
                    Prefix: "IFB",
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: 6,
                    ActorUserId: currentUserId,
                    Instant: DateTimeOffset.UtcNow), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Alokasi nomor bisnis InitialFormationBatch gagal, menggunakan nomor darurat.");
                batchNumber = $"IFB-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
            }

            var validations = await ValidateRowsAsync(request.Rows, null, cancellationToken);
            var totalRows = validations.Count;
            var validRows = validations.Count(v => v.IsValid);
            var invalidRows = validations.Count(v => !v.IsValid);

            var batch = new HrdInitialFormationBatch
            {
                Id = Guid.NewGuid(),
                BatchNumber = batchNumber,
                HospitalSiteId = request.HospitalSiteId,
                SourceDocumentReference = request.SourceDocumentReference.Trim(),
                Notes = request.Notes?.Trim(),
                BatchStatus = InitialFormationBatchStatus.Draft,
                TotalRows = totalRows,
                ValidRows = validRows,
                InvalidRows = invalidRows,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = currentUserId,
                IsDelete = false
            };

            foreach (var val in validations)
            {
                batch.Rows.Add(new HrdInitialFormationBatchRow
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.Id,
                    OrganizationUnitId = val.OrganizationUnitId,
                    PositionId = val.PositionId,
                    FormationQuantity = val.FormationQuantity,
                    EffectiveStartDate = val.EffectiveStartDate,
                    IsValid = val.IsValid,
                    ValidationMessage = val.ValidationMessage,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = currentUserId,
                    IsDelete = false
                });
            }

            _dbContext.HrdInitialFormationBatches.Add(batch);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Batch formasi awal {BatchNumber} berhasil dibuat sebagai Draf dengan {TotalRows} baris.", batch.BatchNumber, totalRows);

            var response = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Draf batch formasi awal berhasil dibuat.", StatusCodes.Status201Created);
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> UpdateDraftAsync(
            Guid id,
            UpdateInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pembaruan batch tidak boleh kosong.");
            }

            var batch = await _dbContext.HrdInitialFormationBatches
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan.");
            }

            // Kriteria 8: Hanya boleh diubah selagi Draft atau Returned
            if (batch.BatchStatus != InitialFormationBatchStatus.Draft &&
                batch.BatchStatus != InitialFormationBatchStatus.Returned)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Batch berstatus {batch.BatchStatus} tidak dapat diubah. Hanya draf atau batch yang dikembalikan yang dapat diedit.");
            }

            // Konkurensi token (VR-WFP-409)
            if (batch.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data batch telah diubah oleh pengguna lain. Silakan muat ulang halaman.");
            }

            var validations = await ValidateRowsAsync(request.Rows, id, cancellationToken);
            var totalRows = validations.Count;
            var validRows = validations.Count(v => v.IsValid);
            var invalidRows = validations.Count(v => !v.IsValid);

            // Hapus baris lama
            _dbContext.HrdInitialFormationBatchRows.RemoveRange(batch.Rows);
            batch.Rows.Clear();

            // Tambahkan baris baru
            foreach (var val in validations)
            {
                batch.Rows.Add(new HrdInitialFormationBatchRow
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.Id,
                    OrganizationUnitId = val.OrganizationUnitId,
                    PositionId = val.PositionId,
                    FormationQuantity = val.FormationQuantity,
                    EffectiveStartDate = val.EffectiveStartDate,
                    IsValid = val.IsValid,
                    ValidationMessage = val.ValidationMessage,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = currentUserId,
                    IsDelete = false
                });
            }

            batch.SourceDocumentReference = request.SourceDocumentReference.Trim();
            batch.Notes = request.Notes?.Trim();
            batch.TotalRows = totalRows;
            batch.ValidRows = validRows;
            batch.InvalidRows = invalidRows;
            batch.RowVersion = Guid.NewGuid();
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = currentUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Batch formasi awal {BatchNumber} berhasil diperbarui dengan {TotalRows} baris.", batch.BatchNumber, totalRows);

            var response = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Draf batch formasi awal berhasil diperbarui.");
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> UploadSourceDocumentAsync(
            Guid id,
            IFormFile file,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialFormationBatches
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan.");
            }

            if (batch.BatchStatus != InitialFormationBatchStatus.Draft &&
                batch.BatchStatus != InitialFormationBatchStatus.Returned)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Unggah scan dokumen hanya diperbolehkan untuk batch berstatus Draft atau Returned (status saat ini: {batch.BatchStatus}).");
            }

            if (file == null || file.Length == 0)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Berkas scan SK formasi tidak boleh kosong.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedFileExtensions.Contains(extension))
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Format berkas tidak diizinkan. Hanya format berkas PDF, JPG, JPEG, atau PNG yang diperbolehkan (VR-WFP-14).");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Ukuran berkas scan SK melebihi batas maksimum 10 MB (VR-WFP-14).");
            }

            var uploadFolder = Path.Combine(AppContext.BaseDirectory, "Uploads", "WorkforcePlanning", "InitialFormationBatches", id.ToString());
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var safeOriginalName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid():N}_{safeOriginalName}";
            var fullPath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            batch.SourceDocumentFileName = safeOriginalName;
            batch.SourceDocumentPath = fullPath;
            batch.RowVersion = Guid.NewGuid();
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = currentUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Scan SK formasi resmi berhasil diunggah untuk batch {BatchNumber}: {FileName}", batch.BatchNumber, safeOriginalName);

            var response = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Berkas scan SK formasi berhasil diunggah.");
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> GetSourceDocumentStreamAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialFormationBatches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null || string.IsNullOrWhiteSpace(batch.SourceDocumentPath) || !File.Exists(batch.SourceDocumentPath))
            {
                return null;
            }

            var extension = Path.GetExtension(batch.SourceDocumentPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };

            var stream = File.OpenRead(batch.SourceDocumentPath);
            var fileName = batch.SourceDocumentFileName ?? Path.GetFileName(batch.SourceDocumentPath);

            return (stream, contentType, fileName);
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> SubmitAsync(
            Guid id,
            SubmitInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pengajuan batch tidak boleh kosong.");
            }

            var batch = await _dbContext.HrdInitialFormationBatches
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan.");
            }

            if (batch.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict, "Data batch telah diubah oleh pengguna lain.");
            }

            if (batch.BatchStatus != InitialFormationBatchStatus.Draft &&
                batch.BatchStatus != InitialFormationBatchStatus.Returned)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Batch berstatus {batch.BatchStatus} tidak dapat diajukan. Hanya status Draft atau Returned yang sah untuk pengajuan.");
            }

            // Kriteria 4: Validasi scan berkas fisik (VR-WFP-13)
            if (string.IsNullOrWhiteSpace(batch.SourceDocumentPath))
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Dokumen scan SK formasi resmi wajib diunggah sebelum pengajuan (VR-WFP-13).");
            }

            // Kriteria 4: Validasi baris data (VR-WFP-12)
            if (batch.TotalRows == 0)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Batch formasi awal tidak memiliki baris data (VR-WFP-12).");
            }

            if (batch.InvalidRows > 0)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Terdapat {batch.InvalidRows} baris formasi yang tidak valid. Perbaiki seluruh baris sebelum mengajukan (VR-WFP-12).");
            }

            // Kriteria 2: Revalidasi langsung ke database untuk memastikan tidak ada konflik baru (VR-WFP-11)
            var rowRequests = batch.Rows.Where(r => !r.IsDelete).Select(r => new InitialFormationBatchRowRequest
            {
                OrganizationUnitId = r.OrganizationUnitId,
                PositionId = r.PositionId,
                FormationQuantity = r.FormationQuantity,
                EffectiveStartDate = r.EffectiveStartDate
            }).ToList();

            var validations = await ValidateRowsAsync(rowRequests, batch.Id, cancellationToken);
            if (validations.Any(v => !v.IsValid))
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Terdapat baris formasi yang bertentangan dengan formasi aktif atau batch lain (VR-WFP-11).");
            }

            // BE-HRD-FIN-WFP-009: Cegah workflow ganda akibat pengajuan berulang atau duplikasi eksekusi.
            var workflowQuery = _dbContext.Set<TrxWorkflowInstance>()
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.ApproverAssignments)
                .Where(w => !w.IsDelete &&
                            w.ReferenceType == WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch &&
                            w.ReferenceId == batch.Id);

            var existingActiveWorkflow = await workflowQuery
                .OrderByDescending(w => w.CreateDateTime)
                .FirstOrDefaultAsync(w =>
                    w.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Submitted ||
                    w.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress,
                    cancellationToken);

            if (existingActiveWorkflow != null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Sudah terdapat pengajuan workflow aktif ({existingActiveWorkflow.RequestNumber}) untuk batch {batch.BatchNumber}. Jalankan pemulihan sinkronisasi jika status batch belum selaras.");
            }

            // Pastikan belum ada formasi yang terlanjur terbentuk untuk batch ini.
            var hasCreatedFormation = batch.Rows.Any(r => !r.IsDelete && r.CreatedFormationId.HasValue) ||
                await _dbContext.HrdPositionFormations.AsNoTracking()
                    .AnyAsync(f => !f.IsDelete &&
                                   f.SourceType == PositionFormationSourceType.InitialBatch &&
                                   f.SourceDocumentId == batch.Id,
                              cancellationToken);

            if (hasCreatedFormation)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Batch ini sudah memiliki formasi yang terbentuk di Buku Formasi dan tidak dapat diajukan ulang.");
            }

            var activeRows = batch.Rows.Where(r => !r.IsDelete).ToList();
            var totalFte = activeRows.Sum(r => r.FormationQuantity);
            var siteName = await _dbContext.MstHospitalSites.AsNoTracking()
                .Where(s => s.Id == batch.HospitalSiteId)
                .Select(s => s.SiteName)
                .FirstOrDefaultAsync(cancellationToken);

            var firstRow = activeRows.FirstOrDefault();
            string? firstUnitName = null;
            string? firstPosName = null;
            if (firstRow != null)
            {
                firstUnitName = await _dbContext.MstOrganizationUnits.AsNoTracking()
                    .Where(u => u.Id == firstRow.OrganizationUnitId)
                    .Select(u => u.UnitName)
                    .FirstOrDefaultAsync(cancellationToken);
                firstPosName = await _dbContext.MstPositions.AsNoTracking()
                    .Where(p => p.Id == firstRow.PositionId)
                    .Select(p => p.PositionName)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var requestContextJson = JsonSerializer.SerializeToElement(new
            {
                summary = $"Pengajuan Batch Formasi Awal {batch.BatchNumber} ({siteName ?? "-"}) — {activeRows.Count} posisi, total {totalFte:0.##} FTE (SK: {batch.SourceDocumentReference})",
                source = new
                {
                    batchId = batch.Id,
                    batchNumber = batch.BatchNumber
                },
                batchId = batch.Id,
                batchNumber = batch.BatchNumber,
                hospitalSiteId = batch.HospitalSiteId,
                hospitalSiteName = siteName,
                sourceDocumentReference = batch.SourceDocumentReference,
                organizationUnitName = firstUnitName,
                positionName = firstPosName,
                effectiveStartDate = firstRow?.EffectiveStartDate.ToString("yyyy-MM-dd"),
                totalRows = activeRows.Count,
                totalFte
            });

            var submitComment = string.IsNullOrWhiteSpace(request.Notes)
                ? $"Pengajuan Batch Formasi Awal {batch.BatchNumber}"
                : request.Notes.Trim();

            WorkflowServiceResult<WorkflowManagement.DTOs.WorkflowInstanceDetailResponse> workflowResult;

            // Jika pengajuan ulang dari status Returned yang masih terhubung ke instans workflow Returned/RevisionRequested, ajukan ulang instans yang sama.
            var resumableWorkflow = batch.WorkflowInstanceId.HasValue
                ? await _dbContext.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w =>
                        w.Id == batch.WorkflowInstanceId.Value &&
                        !w.IsDelete &&
                        w.ReferenceType == WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch &&
                        w.ReferenceId == batch.Id &&
                        (w.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
                         w.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Returned ||
                         w.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Draft),
                        cancellationToken)
                : null;

            if (resumableWorkflow != null)
            {
                workflowResult = await _workflowIntegrationService.ResubmitWorkflowAsync(
                    resumableWorkflow.Id,
                    submitComment,
                    cancellationToken);
            }
            else
            {
                // BE-HRD-FIN-WFP-008: Gunakan kode definisi workflow T10 existing dan pertahankan ReferenceType WorkforceInitialFormationBatch.
                workflowResult = await _workflowIntegrationService.StartWorkflowAsync(new StartWorkforcePlanningWorkflowRequest
                {
                    WorkflowDefinitionCode = WorkforcePlanningValueConstants.WorkflowDefinitionCodes.ManpowerPlanAndInitialFormation,
                    ReferenceType = WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch,
                    ReferenceId = batch.Id,
                    DocumentNumber = batch.BatchNumber,
                    RequestCorrelationId = batch.Id.ToString("N"),
                    IdempotencyKey = $"ifb-submit:{batch.Id:N}:{batch.RowVersion:N}",
                    RequestContext = requestContextJson,
                    SubmitComment = submitComment,
                    AutoSubmit = true
                }, cancellationToken);
            }

            // BE-HRD-FIN-WFP-009: Hilangkan false success submission. Jangan ubah status menjadi Submitted bila workflow gagal.
            if (!workflowResult.Success || workflowResult.Data == null)
            {
                _dbContext.ChangeTracker.Clear();
                var errorMessage = string.IsNullOrWhiteSpace(workflowResult.Message)
                    ? "Workflow persetujuan T10 gagal dibuat atau diajukan. Status batch tetap dipertahankan."
                    : workflowResult.Message;

                _logger.LogWarning(
                    "Pengajuan batch formasi awal {BatchNumber} ditolak karena workflow T10 gagal dimulai/diajukan ({StatusCode}): {Message}",
                    batch.BatchNumber,
                    workflowResult.StatusCode,
                    errorMessage);

                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    workflowResult.StatusCode >= 400 ? workflowResult.StatusCode : StatusCodes.Status400BadRequest,
                    errorMessage);
            }

            var hasActiveAssignment = workflowResult.Data.Steps.Any(s =>
                s.IsCurrentStep &&
                s.Assignments.Any(a => a.IsCurrentAssignment ||
                    string.Equals(a.AssignmentStatus, WorkflowValueConstants.AssignmentStatus.Pending, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(a.AssignmentStatus, WorkflowValueConstants.AssignmentStatus.Available, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(a.AssignmentStatus, WorkflowValueConstants.AssignmentStatus.InProgress, StringComparison.OrdinalIgnoreCase)));

            if (!hasActiveAssignment)
            {
                _logger.LogWarning(
                    "Workflow instance {WorkflowInstanceId} untuk batch {BatchNumber} tidak memiliki penugasan approver tahap aktif. Menarik kembali instans workflow.",
                    workflowResult.Data.Id,
                    batch.BatchNumber);

                await _workflowIntegrationService.WithdrawWorkflowAsync(
                    workflowResult.Data.Id,
                    currentUserId,
                    "Penarikan otomatis karena tidak ada penugasan approver tahap aktif pada workflow T10.",
                    cancellationToken);

                _dbContext.ChangeTracker.Clear();
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tahap aktif workflow T10 belum memiliki penugasan approver yang valid. Periksa konfigurasi approver dan matrix persetujuan T10.");
            }

            // Reload entity batch setelah transaksi WorkflowService selesai agar tidak terjadi nested transaction / tracking stale.
            _dbContext.ChangeTracker.Clear();
            var trackedBatch = await _dbContext.HrdInitialFormationBatches
                .FirstOrDefaultAsync(b => b.Id == batch.Id && !b.IsDelete, cancellationToken);

            if (trackedBatch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan setelah pembuatan workflow.");
            }

            if (trackedBatch.RowVersion != request.RowVersion)
            {
                await _workflowIntegrationService.WithdrawWorkflowAsync(
                    workflowResult.Data.Id,
                    currentUserId,
                    "Penarikan otomatis akibat konflik konkurensi (RowVersion) saat menyimpan pengajuan batch.",
                    cancellationToken);

                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data batch telah berubah saat pengajuan berlangsung. Pengajuan dibatalkan agar tidak meninggalkan workflow ganda.");
            }

            trackedBatch.WorkflowInstanceId = workflowResult.Data.Id;
            trackedBatch.WorkflowDefinitionId = workflowResult.Data.WorkflowDefinitionId;
            trackedBatch.BatchStatus = InitialFormationBatchStatus.Submitted;
            trackedBatch.SubmittedAt = workflowResult.Data.SubmittedAt ?? DateTime.UtcNow;
            trackedBatch.SubmittedByUserId = currentUserId;
            trackedBatch.RowVersion = Guid.NewGuid();
            trackedBatch.UpdateDateTime = DateTime.UtcNow;
            trackedBatch.UpdateBy = currentUserId;

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                trackedBatch.Notes = request.Notes.Trim();
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menyimpan status Submitted untuk batch {BatchNumber} setelah workflow T10 dibuat. Menarik kembali workflow {WorkflowId}.", batch.BatchNumber, workflowResult.Data.Id);
                await _workflowIntegrationService.WithdrawWorkflowAsync(
                    workflowResult.Data.Id,
                    currentUserId,
                    "Penarikan otomatis karena penyimpanan pembaruan status batch formasi awal gagal.",
                    cancellationToken);

                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Gagal menyimpan pengajuan batch ke database. Perubahan workflow telah ditarik kembali.");
            }

            _logger.LogInformation("Batch formasi awal {BatchNumber} berhasil diajukan (Submitted) ke workflow T10 ({RequestNumber}).", trackedBatch.BatchNumber, workflowResult.Data.RequestNumber);

            var response = await GetByIdAsync(trackedBatch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Batch formasi awal berhasil diajukan ke workflow persetujuan T10.");
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> CancelAsync(
            Guid id,
            CancelInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pembatalan tidak boleh kosong.");
            }

            var batch = await _dbContext.HrdInitialFormationBatches
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan.");
            }

            if (batch.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict, "Data batch telah diubah oleh pengguna lain.");
            }

            // Kriteria 7: Hanya dari Draft atau Returned yang boleh dibatalkan; Applied tidak dapat diubah apa pun
            if (batch.BatchStatus == InitialFormationBatchStatus.Applied)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Batch formasi awal yang sudah diterapkan (Applied) tidak dapat dibatalkan.");
            }

            if (batch.BatchStatus != InitialFormationBatchStatus.Draft &&
                batch.BatchStatus != InitialFormationBatchStatus.Returned)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Batch berstatus {batch.BatchStatus} tidak dapat dibatalkan. Pembatalan hanya sah untuk status Draft atau Returned.");
            }

            batch.BatchStatus = InitialFormationBatchStatus.Cancelled;
            batch.IsCancel = true;
            batch.CancelDateTime = DateTime.UtcNow;
            batch.CancelBy = currentUserId;
            batch.RowVersion = Guid.NewGuid();
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = currentUserId;

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                batch.Notes = string.IsNullOrWhiteSpace(batch.Notes)
                    ? $"[Dibatalkan]: {request.Reason.Trim()}"
                    : $"{batch.Notes}\n[Dibatalkan]: {request.Reason.Trim()}";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Batch formasi awal {BatchNumber} berhasil dibatalkan.", batch.BatchNumber);

            var response = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                response!, "Batch formasi awal berhasil dibatalkan.");
        }

        public async Task<WorkforcePlanningResult<InitialFormationBatchResponse>> RecoverAsync(
            Guid id,
            RecoverInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (currentUserId == Guid.Empty)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status401Unauthorized, "Identitas pengguna tidak valid.");
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length < 10)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Alasan pemulihan batch wajib diisi minimal 10 karakter.");
            }

            var batch = await _dbContext.HrdInitialFormationBatches
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status404NotFound, "Batch formasi awal tidak ditemukan.");
            }

            if (batch.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict, "Data batch telah diperbarui oleh proses atau pengguna lain.");
            }

            if (batch.BatchStatus == InitialFormationBatchStatus.Applied)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict, "Batch formasi awal sudah diterapkan (Applied) dan tidak perlu dipulihkan.");
            }

            if (batch.BatchStatus != InitialFormationBatchStatus.Submitted)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Pemulihan hanya berlaku untuk batch berstatus Submitted (saat ini: {batch.BatchStatus}).");
            }

            // Guard 1: Pastikan batch belum menghasilkan formasi di tabel HrdPositionFormations ataupun pada barisnya
            var hasRowFormation = batch.Rows.Any(r => !r.IsDelete && r.CreatedFormationId.HasValue);
            var hasBookFormation = await _dbContext.HrdPositionFormations.AsNoTracking()
                .AnyAsync(f => !f.IsDelete &&
                               f.SourceType == PositionFormationSourceType.InitialBatch &&
                               f.SourceDocumentId == batch.Id,
                          cancellationToken);

            if (hasRowFormation || hasBookFormation)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Batch ini sudah menghasilkan data formasi pada Buku Formasi sehingga tidak boleh dipulihkan atau dikembalikan ke Draft.");
            }

            // Guard 2: Periksa semua workflow berdasarkan ReferenceType dan ReferenceId (termasuk yang belum selesai dibuat)
            var existingWorkflows = await _dbContext.Set<TrxWorkflowInstance>()
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.ApproverAssignments)
                .Where(w => !w.IsDelete &&
                            w.ReferenceType == WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch &&
                            w.ReferenceId == batch.Id)
                .OrderByDescending(w => w.CreateDateTime)
                .ToListAsync(cancellationToken);

            var actorName = await _dbContext.Users.AsNoTracking()
                .Where(u => u.Id == currentUserId)
                .Select(u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? currentUserId.ToString())
                .FirstOrDefaultAsync(cancellationToken) ?? currentUserId.ToString();

            var now = DateTime.UtcNow;

            // Pastikan batch belum memperoleh keputusan akhir pada engine workflow
            var finalWorkflow = existingWorkflows.FirstOrDefault(w =>
                string.Equals(w.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(w.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(w.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Rejected, StringComparison.OrdinalIgnoreCase));

            if (finalWorkflow != null)
            {
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Workflow {finalWorkflow.RequestNumber} untuk batch {batch.BatchNumber} sudah memiliki keputusan akhir ({finalWorkflow.WorkflowStatus}) dan tidak dapat dikembalikan ke Draft.");
            }

            // Cek apakah sudah tersedia workflow aktif yang sah dan mempunyai penugasan approver tahap aktif
            var validActiveWorkflow = existingWorkflows.FirstOrDefault(w =>
                (string.Equals(w.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Submitted, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(w.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.InProgress, StringComparison.OrdinalIgnoreCase)) &&
                w.StepInstances.Any(s =>
                    !s.IsDelete &&
                    s.IsCurrentStep &&
                    s.ApproverAssignments.Any(a => !a.IsDelete && a.IsActive)));

            if (validActiveWorkflow != null)
            {
                if (batch.WorkflowInstanceId == validActiveWorkflow.Id &&
                    batch.WorkflowDefinitionId == validActiveWorkflow.WorkflowDefinitionId)
                {
                    return WorkforcePlanningResult<InitialFormationBatchResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Batch {batch.BatchNumber} sudah terhubung secara sah ke workflow {validActiveWorkflow.RequestNumber}.");
                }

                batch.WorkflowInstanceId = validActiveWorkflow.Id;
                batch.WorkflowDefinitionId = validActiveWorkflow.WorkflowDefinitionId;
                batch.RowVersion = Guid.NewGuid();
                batch.UpdateDateTime = now;
                batch.UpdateBy = currentUserId;

                var reconcileNote = $"[{now:yyyy-MM-dd HH:mm:ss} UTC][Pemulihan Rekonsiliasi oleh {actorName}]: Menghubungkan ulang ke workflow {validActiveWorkflow.RequestNumber} ({validActiveWorkflow.WorkflowStatus}). Alasan: {request.Reason.Trim()}";
                batch.Notes = string.IsNullOrWhiteSpace(batch.Notes) ? reconcileNote : $"{batch.Notes}\n{reconcileNote}";

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Batch {BatchNumber} direkonsiliasi ke workflow sah {RequestNumber} oleh {ActorUserId}.",
                    batch.BatchNumber,
                    validActiveWorkflow.RequestNumber,
                    currentUserId);

                var reconciledResponse = await GetByIdAsync(batch.Id, cancellationToken);
                return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                    reconciledResponse!,
                    $"Referensi batch {batch.BatchNumber} berhasil direkonsiliasi ke workflow {validActiveWorkflow.RequestNumber}.");
            }

            // Tutup / bersihkan workflow yang belum selesai atau tanpa assignment sebelum mengembalikan batch ke Draft
            foreach (var brokenWorkflow in existingWorkflows.Where(w =>
                !string.Equals(w.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Cancelled, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(w.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Withdrawn, StringComparison.OrdinalIgnoreCase)))
            {
                brokenWorkflow.WorkflowStatus = WorkflowValueConstants.WorkflowStatus.Cancelled;
                brokenWorkflow.CancelledAt = now;
                brokenWorkflow.CancellationReason = $"Dibatalkan saat pemulihan batch {batch.BatchNumber} ke Draft oleh {actorName}: {request.Reason.Trim()}";
                brokenWorkflow.IsActive = false;
                brokenWorkflow.UpdateDateTime = now;
                brokenWorkflow.UpdateBy = currentUserId;
            }

            batch.BatchStatus = InitialFormationBatchStatus.Draft;
            batch.WorkflowInstanceId = null;
            batch.WorkflowDefinitionId = null;
            batch.SubmittedAt = null;
            batch.SubmittedByUserId = null;
            batch.ApprovedAt = null;
            batch.ApprovedByUserId = null;
            batch.AppliedAt = null;
            batch.RowVersion = Guid.NewGuid();
            batch.UpdateDateTime = now;
            batch.UpdateBy = currentUserId;

            var recoveryNote = $"[{now:yyyy-MM-dd HH:mm:ss} UTC][Pemulihan ke Draft oleh {actorName}]: Status batch dikembalikan dari Submitted ke Draft karena tidak ada workflow T10 yang sah. Alasan: {request.Reason.Trim()}";
            batch.Notes = string.IsNullOrWhiteSpace(batch.Notes) ? recoveryNote : $"{batch.Notes}\n{recoveryNote}";

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Batch {BatchNumber} berhasil dipulihkan ke Draft oleh {ActorUserId} tanpa mengubah nomor batch, baris formasi, atau dokumen SK.",
                batch.BatchNumber,
                currentUserId);

            var recoveredResponse = await GetByIdAsync(batch.Id, cancellationToken);
            return WorkforcePlanningResult<InitialFormationBatchResponse>.Ok(
                recoveredResponse!,
                $"Batch {batch.BatchNumber} berhasil dipulihkan ke status Draft dan siap diajukan ulang.");
        }
    }
}
