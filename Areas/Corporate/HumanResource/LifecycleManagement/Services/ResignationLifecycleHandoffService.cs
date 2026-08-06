using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class ResignationLifecycleHandoffService
    {
        private readonly ApplicationDbContext _dbContext;

        public ResignationLifecycleHandoffService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResignationServiceResult<ResignationHandoffResponse>> HandoffAsync(
            Guid resignationRequestId,
            ResignationHandoffRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var resignation = await _dbContext.TrxResignationRequests
                .FirstOrDefaultAsync(x => x.Id == resignationRequestId && !x.IsDelete, cancellationToken);

            if (resignation == null)
            {
                return ResignationServiceResult<ResignationHandoffResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan resign tidak ditemukan.");
            }

            if (resignation.EmployeeSeparationId.HasValue)
            {
                var existing = await BuildExistingResponseAsync(resignation, cancellationToken);
                await transaction.RollbackAsync(cancellationToken);
                return ResignationServiceResult<ResignationHandoffResponse>.Ok(
                    existing,
                    "Pengajuan resign sudah pernah di-handoff ke separation.");
            }

            if (resignation.RequestStatus != ResignationValueConstants.Status.Approved)
            {
                return ResignationServiceResult<ResignationHandoffResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya pengajuan resign berstatus Approved yang dapat di-handoff.");
            }

            var now = DateTime.UtcNow;
            var separation = new TrxEmployeeSeparation
            {
                Id = Guid.NewGuid(),
                SeparationNumber = GenerateNumber("SEP"),
                WorkforceProfileId = resignation.WorkforceProfileId,
                EmployeeId = resignation.EmployeeId,
                SeparationType = "Resignation",
                RequestReasonId = resignation.RequestReasonId,
                RejectionReasonId = resignation.RejectionReasonId,
                FinalEmploymentStatusId = NormalizeGuid(request.FinalEmploymentStatusId),
                FinalPayrollPeriodId = NormalizeGuid(request.FinalPayrollPeriodId),
                WorkflowDefinitionId = resignation.WorkflowDefinitionId,
                WorkflowInstanceId = resignation.WorkflowInstanceId,
                RequestDate = resignation.RequestDate,
                ApprovedDate = resignation.ApprovedAt,
                EffectiveSeparationDate = resignation.ProposedLastWorkingDate,
                LastWorkingDate = resignation.ProposedLastWorkingDate,
                NoticePeriodDays = resignation.NoticePeriodDays,
                SeparationStatus = "Approved",
                IsEligibleForRehire = request.IsEligibleForRehire,
                ApprovedByUserId = resignation.ApprovedByUserId ?? actorUserId,
                ReasonText = resignation.ResignationReason,
                Notes = NormalizeText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.TrxEmployeeSeparations.Add(separation);

            WfpOffboardingChecklist? checklist = null;
            var createdTaskCount = 0;

            if (request.CreateOffboardingChecklist)
            {
                var template = await ResolveTemplateAsync(
                    resignation,
                    request.OffboardingTemplateId,
                    cancellationToken);

                if (request.OffboardingTemplateId.HasValue && template == null)
                {
                    return ResignationServiceResult<ResignationHandoffResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Offboarding template tidak ditemukan atau tidak aktif.");
                }

                if (template != null)
                {
                    var managerProfileId = await ResolvePrimaryManagerAsync(
                        resignation.WorkforceProfileId,
                        cancellationToken);
                    var templateTasks = await _dbContext.MstOffboardingTemplateTasks
                        .AsNoTracking()
                        .Where(x =>
                            x.OffboardingTemplateId == template.Id &&
                            x.IsActive &&
                            !x.IsDelete)
                        .OrderBy(x => x.SortOrder)
                        .ToListAsync(cancellationToken);

                    checklist = new WfpOffboardingChecklist
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = resignation.WorkforceProfileId,
                        EmployeeId = resignation.EmployeeId,
                        OffboardingTemplateId = template.Id,
                        EmployeeSeparationId = separation.Id,
                        ChecklistNumber = GenerateNumber("OFB"),
                        OffboardingType = "Resignation",
                        PlannedStartDate = now,
                        PlannedCompletionDate = resignation.ProposedLastWorkingDate,
                        ChecklistStatus = "Active",
                        TotalTask = templateTasks.Count,
                        RequiredTask = templateTasks.Count(x => x.IsRequired),
                        CompletedTask = 0,
                        CompletedRequiredTask = 0,
                        ProgressPercentage = 0,
                        ManagerWorkforceProfileId = managerProfileId,
                        Notes = $"Created from resignation request {resignation.RequestNumber}.",
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    };

                    _dbContext.WfpOffboardingChecklists.Add(checklist);

                    foreach (var templateTask in templateTasks)
                    {
                        _dbContext.WfpOffboardingTasks.Add(new WfpOffboardingTask
                        {
                            Id = Guid.NewGuid(),
                            OffboardingChecklistId = checklist.Id,
                            OffboardingTemplateTaskId = templateTask.Id,
                            TaskCode = templateTask.TaskCode,
                            TaskName = templateTask.TaskName,
                            TaskCategory = templateTask.TaskCategory,
                            Description = templateTask.Description,
                            DueDate = resignation.ProposedLastWorkingDate.AddDays(templateTask.DueDayOffset),
                            TaskStatus = "Pending",
                            IsRequired = templateTask.IsRequired,
                            RequiresDocument = templateTask.RequiresDocument,
                            SortOrder = templateTask.SortOrder,
                            IsActive = true,
                            CreateDateTime = now,
                            CreateBy = actorUserId
                        });
                    }

                    createdTaskCount = templateTasks.Count;
                }
            }

            resignation.EmployeeSeparationId = separation.Id;
            resignation.RequestStatus = ResignationValueConstants.Status.HandoffCompleted;
            resignation.UpdateDateTime = now;
            resignation.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ResignationServiceResult<ResignationHandoffResponse>.Ok(
                new ResignationHandoffResponse
                {
                    ResignationRequestId = resignation.Id,
                    EmployeeSeparationId = separation.Id,
                    SeparationNumber = separation.SeparationNumber,
                    OffboardingChecklistId = checklist?.Id,
                    OffboardingChecklistNumber = checklist?.ChecklistNumber,
                    CreatedTaskCount = createdTaskCount,
                    RequestStatus = resignation.RequestStatus
                },
                checklist == null
                    ? "Pengajuan resign berhasil di-handoff ke separation. Checklist belum dibuat karena template tidak tersedia atau tidak dipilih."
                    : "Pengajuan resign berhasil di-handoff ke separation dan offboarding checklist.");
        }

        private async Task<ResignationHandoffResponse> BuildExistingResponseAsync(
            TrxResignationRequest resignation,
            CancellationToken cancellationToken)
        {
            var separation = await _dbContext.TrxEmployeeSeparations
                .AsNoTracking()
                .FirstAsync(x => x.Id == resignation.EmployeeSeparationId, cancellationToken);
            var checklist = await _dbContext.WfpOffboardingChecklists
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeSeparationId == separation.Id &&
                    !x.IsDelete,
                    cancellationToken);

            return new ResignationHandoffResponse
            {
                ResignationRequestId = resignation.Id,
                EmployeeSeparationId = separation.Id,
                SeparationNumber = separation.SeparationNumber,
                OffboardingChecklistId = checklist?.Id,
                OffboardingChecklistNumber = checklist?.ChecklistNumber,
                CreatedTaskCount = checklist?.TotalTask ?? 0,
                RequestStatus = resignation.RequestStatus
            };
        }

        private async Task<MstOffboardingTemplate?> ResolveTemplateAsync(
            TrxResignationRequest resignation,
            Guid? requestedTemplateId,
            CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow;
            var query = _dbContext.MstOffboardingTemplates
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    (x.SeparationType == "Resignation" || x.SeparationType == "General") &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today));

            if (requestedTemplateId.HasValue)
            {
                return await query.FirstOrDefaultAsync(x => x.Id == requestedTemplateId.Value, cancellationToken);
            }

            var organization = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == resignation.WorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EffectiveStartDate <= today &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            return await query
                .OrderByDescending(x =>
                    organization != null &&
                    x.LegalEntityId == organization.LegalEntityId)
                .ThenByDescending(x =>
                    organization != null &&
                    x.HospitalSiteId == organization.HospitalSiteId)
                .ThenByDescending(x =>
                    organization != null &&
                    x.OrganizationUnitId == organization.OrganizationUnitId)
                .ThenByDescending(x =>
                    organization != null &&
                    x.DepartmentId == organization.DepartmentId)
                .ThenByDescending(x =>
                    organization != null &&
                    x.PositionId == organization.PositionId)
                .ThenByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.Version)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<Guid?> ResolvePrimaryManagerAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            return await _dbContext.WfpManagerAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.IsPrimaryManager &&
                    x.EffectiveStartDate <= now &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now))
                .OrderByDescending(x => x.EffectiveStartDate)
                .Select(x => (Guid?)x.ManagerWorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty ? value : null;
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string GenerateNumber(string prefix)
        {
            var value = $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
            return value.Length <= 50 ? value : value[..50];
        }
    }
}
