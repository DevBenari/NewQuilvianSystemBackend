using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimePlanningService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimePlanQueryService _queryService;
        private readonly OvertimePolicyResolverService _policyResolver;
        private readonly OvertimeRateResolverService _rateResolver;
        private readonly OvertimePeriodGuardService _periodGuard;

        public OvertimePlanningService(
            ApplicationDbContext dbContext,
            OvertimePlanQueryService queryService,
            OvertimePolicyResolverService policyResolver,
            OvertimeRateResolverService rateResolver,
            OvertimePeriodGuardService periodGuard)
        {
            _dbContext = dbContext;
            _queryService = queryService;
            _policyResolver = policyResolver;
            _rateResolver = rateResolver;
            _periodGuard = periodGuard;
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanResponse>> CreateAsync(
            CreateOvertimePlanRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var headerError = await ValidateHeaderAsync(null, request.PlanTitle, request.PlanStartDate, request.PlanEndDate,
                request.LegalEntityId, request.HospitalSiteId, request.OrganizationUnitId, request.DepartmentId,
                request.CostCenterId, request.WorkLocationId, request.RosterPeriodId, request.Reason, cancellationToken);
            if (headerError != null) return Fail<OvertimePlanResponse>(headerError);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var now = DateTime.UtcNow;
                var entity = new TrxOvertimePlan
                {
                    Id = Guid.NewGuid(),
                    PlanNumber = await GeneratePlanNumberAsync(request.PlanStartDate, cancellationToken),
                    PlanTitle = request.PlanTitle.Trim(),
                    LegalEntityId = NormalizeGuid(request.LegalEntityId),
                    HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                    OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                    DepartmentId = NormalizeGuid(request.DepartmentId),
                    CostCenterId = NormalizeGuid(request.CostCenterId),
                    WorkLocationId = NormalizeGuid(request.WorkLocationId),
                    RosterPeriodId = NormalizeGuid(request.RosterPeriodId),
                    PlanStartDate = request.PlanStartDate,
                    PlanEndDate = request.PlanEndDate,
                    Reason = request.Reason.Trim(),
                    Notes = NormalizeText(request.Notes),
                    PlanStatus = OvertimeValueConstants.PlanStatus.Draft,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    UpdateBy = actorUserId
                };

                _dbContext.TrxOvertimePlans.Add(entity);

                var staged = new List<TrxOvertimePlanDetail>();
                var sequence = 1;
                foreach (var detailRequest in request.Details ?? new List<CreateOvertimePlanDetailRequest>())
                {
                    var evaluation = await EvaluateDetailAsync(entity, null, detailRequest, staged, cancellationToken);
                    if (!evaluation.Response.CanPersist)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Fail<OvertimePlanResponse>(BuildIssueMessage(evaluation.Response));
                    }

                    var detail = BuildDetailEntity(entity.Id, sequence++, detailRequest, evaluation, actorUserId, now);
                    staged.Add(detail);
                    _dbContext.TrxOvertimePlanDetails.Add(detail);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                var data = await _queryService.GetDetailAsync(entity.Id, cancellationToken);
                return OvertimePlanningServiceResult<OvertimePlanResponse>.Ok(data!, "Rencana lembur berhasil dibuat.", StatusCodes.Status201Created);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanResponse>> UpdateAsync(
            Guid id,
            UpdateOvertimePlanRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.TrxOvertimePlans.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return NotFound<OvertimePlanResponse>();
            if (!CanEdit(entity.PlanStatus)) return Conflict<OvertimePlanResponse>("Rencana lembur hanya dapat diubah saat berstatus Draft atau Validated.");

            var headerError = await ValidateHeaderAsync(id, request.PlanTitle, request.PlanStartDate, request.PlanEndDate,
                request.LegalEntityId, request.HospitalSiteId, request.OrganizationUnitId, request.DepartmentId,
                request.CostCenterId, request.WorkLocationId, request.RosterPeriodId, request.Reason, cancellationToken);
            if (headerError != null) return Fail<OvertimePlanResponse>(headerError);

            entity.PlanTitle = request.PlanTitle.Trim();
            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.DepartmentId = NormalizeGuid(request.DepartmentId);
            entity.CostCenterId = NormalizeGuid(request.CostCenterId);
            entity.WorkLocationId = NormalizeGuid(request.WorkLocationId);
            entity.RosterPeriodId = NormalizeGuid(request.RosterPeriodId);
            entity.PlanStartDate = request.PlanStartDate;
            entity.PlanEndDate = request.PlanEndDate;
            entity.Reason = request.Reason.Trim();
            entity.Notes = NormalizeText(request.Notes);
            entity.PlanStatus = OvertimeValueConstants.PlanStatus.Draft;
            entity.ValidatedAt = null;
            entity.ValidatedByUserId = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            var data = await _queryService.GetDetailAsync(id, cancellationToken);
            return OvertimePlanningServiceResult<OvertimePlanResponse>.Ok(data!, "Rencana lembur berhasil diperbarui.");
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanResponse>> UpdateStatusAsync(
            Guid id,
            UpdateOvertimePlanStatusRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.TrxOvertimePlans.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return NotFound<OvertimePlanResponse>();
            var periodGuard = await CheckPlanPeriodAsync(entity, cancellationToken);
            if (!periodGuard.IsWritable) return Conflict<OvertimePlanResponse>(periodGuard.Message);
            if (!CanEdit(entity.PlanStatus)) return Conflict<OvertimePlanResponse>("Status aktif hanya dapat diubah saat rencana Draft atau Validated.");

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            var data = await _queryService.GetDetailAsync(id, cancellationToken);
            return OvertimePlanningServiceResult<OvertimePlanResponse>.Ok(data!, "Status rencana lembur berhasil diubah.");
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanActionResponse>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.TrxOvertimePlans
                .Include(x => x.Details)
                .ThenInclude(x => x.GeneratedOvertimeRequest)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return NotFound<OvertimePlanActionResponse>();
            var periodGuard = await CheckPlanPeriodAsync(entity, cancellationToken);
            if (!periodGuard.IsWritable) return Conflict<OvertimePlanActionResponse>(periodGuard.Message);
            if (entity.PlanStatus != OvertimeValueConstants.PlanStatus.Draft)
                return Conflict<OvertimePlanActionResponse>("Hanya rencana Draft yang dapat dihapus.");
            if (entity.Details.Any(x => !x.IsDelete && x.GeneratedOvertimeRequest != null && !x.GeneratedOvertimeRequest.IsDelete))
                return Conflict<OvertimePlanActionResponse>("Rencana sudah menghasilkan overtime request dan tidak dapat dihapus.");

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            foreach (var detail in entity.Details.Where(x => !x.IsDelete))
            {
                detail.IsDelete = true;
                detail.IsActive = false;
                detail.DeleteDateTime = now;
                detail.DeleteBy = actorUserId;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimePlanningServiceResult<OvertimePlanActionResponse>.Ok(new OvertimePlanActionResponse
            {
                OvertimePlanId = entity.Id,
                PlanNumber = entity.PlanNumber,
                PreviousStatus = entity.PlanStatus,
                CurrentStatus = entity.PlanStatus,
                AffectedDetailCount = entity.Details.Count,
                ActionAt = now
            }, "Rencana lembur berhasil dihapus.");
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanDetailResponse>> AddDetailAsync(
            Guid planId,
            CreateOvertimePlanDetailRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var plan = await _dbContext.TrxOvertimePlans.FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);
            if (plan == null) return NotFound<OvertimePlanDetailResponse>();
            if (!CanEdit(plan.PlanStatus)) return Conflict<OvertimePlanDetailResponse>("Detail hanya dapat ditambahkan saat rencana Draft atau Validated.");

            var evaluation = await EvaluateDetailAsync(plan, null, request, null, cancellationToken);
            if (!evaluation.Response.CanPersist) return Fail<OvertimePlanDetailResponse>(BuildIssueMessage(evaluation.Response));

            var sequence = (await _dbContext.TrxOvertimePlanDetails
                .Where(x => x.OvertimePlanId == planId && !x.IsDelete)
                .MaxAsync(x => (int?)x.SequenceNumber, cancellationToken) ?? 0) + 1;
            var now = DateTime.UtcNow;
            var detail = BuildDetailEntity(planId, sequence, request, evaluation, actorUserId, now);
            _dbContext.TrxOvertimePlanDetails.Add(detail);
            ResetPlanValidation(plan, actorUserId, now);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var data = await _queryService.GetPlanDetailAsync(planId, detail.Id, cancellationToken);
            return OvertimePlanningServiceResult<OvertimePlanDetailResponse>.Ok(data!, evaluation.Response.CanPublish
                ? "Detail rencana lembur berhasil ditambahkan."
                : "Detail rencana lembur disimpan sebagai Draft dan masih memiliki hasil validasi yang harus diperbaiki.", StatusCodes.Status201Created);
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanDetailResponse>> UpdateDetailAsync(
            Guid planId,
            Guid detailId,
            UpdateOvertimePlanDetailRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var plan = await _dbContext.TrxOvertimePlans.FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);
            if (plan == null) return NotFound<OvertimePlanDetailResponse>();
            if (!CanEdit(plan.PlanStatus)) return Conflict<OvertimePlanDetailResponse>("Detail hanya dapat diubah saat rencana Draft atau Validated.");
            var detail = await _dbContext.TrxOvertimePlanDetails
                .Include(x => x.GeneratedOvertimeRequest)
                .FirstOrDefaultAsync(x => x.Id == detailId && x.OvertimePlanId == planId && !x.IsDelete, cancellationToken);
            if (detail == null) return NotFound<OvertimePlanDetailResponse>("Detail rencana lembur tidak ditemukan.");
            if (detail.GeneratedOvertimeRequest != null && !detail.GeneratedOvertimeRequest.IsDelete)
                return Conflict<OvertimePlanDetailResponse>("Detail yang sudah menghasilkan overtime request tidak dapat diubah.");

            var evaluation = await EvaluateDetailAsync(plan, detailId, request, null, cancellationToken);
            if (!evaluation.Response.CanPersist) return Fail<OvertimePlanDetailResponse>(BuildIssueMessage(evaluation.Response));

            ApplyEvaluation(detail, request, evaluation);
            detail.IsActive = request.IsActive;
            detail.DetailStatus = OvertimeValueConstants.PlanDetailStatus.Draft;
            detail.UpdateDateTime = DateTime.UtcNow;
            detail.UpdateBy = actorUserId;
            ResetPlanValidation(plan, actorUserId, detail.UpdateDateTime.Value);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var data = await _queryService.GetPlanDetailAsync(planId, detailId, cancellationToken);
            return OvertimePlanningServiceResult<OvertimePlanDetailResponse>.Ok(data!, "Detail rencana lembur berhasil diperbarui.");
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanActionResponse>> DeleteDetailAsync(
            Guid planId,
            Guid detailId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var plan = await _dbContext.TrxOvertimePlans.FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);
            if (plan == null) return NotFound<OvertimePlanActionResponse>();
            var periodGuard = await CheckPlanPeriodAsync(plan, cancellationToken);
            if (!periodGuard.IsWritable) return Conflict<OvertimePlanActionResponse>(periodGuard.Message);
            if (!CanEdit(plan.PlanStatus)) return Conflict<OvertimePlanActionResponse>("Detail hanya dapat dihapus saat rencana Draft atau Validated.");
            var detail = await _dbContext.TrxOvertimePlanDetails
                .Include(x => x.GeneratedOvertimeRequest)
                .FirstOrDefaultAsync(x => x.Id == detailId && x.OvertimePlanId == planId && !x.IsDelete, cancellationToken);
            if (detail == null) return NotFound<OvertimePlanActionResponse>("Detail rencana lembur tidak ditemukan.");
            if (detail.GeneratedOvertimeRequest != null && !detail.GeneratedOvertimeRequest.IsDelete)
                return Conflict<OvertimePlanActionResponse>("Detail yang sudah menghasilkan overtime request tidak dapat dihapus.");

            var now = DateTime.UtcNow;
            detail.IsDelete = true;
            detail.IsActive = false;
            detail.DeleteDateTime = now;
            detail.DeleteBy = actorUserId;
            detail.UpdateDateTime = now;
            detail.UpdateBy = actorUserId;
            ResetPlanValidation(plan, actorUserId, now);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimePlanningServiceResult<OvertimePlanActionResponse>.Ok(new OvertimePlanActionResponse
            {
                OvertimePlanId = plan.Id,
                PlanNumber = plan.PlanNumber,
                PreviousStatus = plan.PlanStatus,
                CurrentStatus = plan.PlanStatus,
                AffectedDetailCount = 1,
                ActionAt = now
            }, "Detail rencana lembur berhasil dihapus.");
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanDetailValidationResponse>> PreviewDetailAsync(
            Guid planId,
            OvertimePlanDetailPreviewRequest request,
            CancellationToken cancellationToken = default)
        {
            var plan = await _dbContext.TrxOvertimePlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);
            if (plan == null) return NotFound<OvertimePlanDetailValidationResponse>();
            var evaluation = await EvaluateDetailAsync(plan, NormalizeGuid(request.ExcludeDetailId), request, null, cancellationToken);
            return OvertimePlanningServiceResult<OvertimePlanDetailValidationResponse>.Ok(evaluation.Response, "Preview validasi detail rencana lembur berhasil dibuat.");
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanValidationResponse>> ValidateAsync(
            Guid planId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var plan = await _dbContext.TrxOvertimePlans
                .Include(x => x.Details.Where(d => !d.IsDelete && !d.IsCancel))
                .FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);
            if (plan == null) return NotFound<OvertimePlanValidationResponse>();
            if (!CanEdit(plan.PlanStatus)) return Conflict<OvertimePlanValidationResponse>("Validasi hanya dapat dilakukan pada rencana Draft atau Validated.");

            var result = await ValidateAndPersistAsync(plan, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimePlanningServiceResult<OvertimePlanValidationResponse>.Ok(result,
                result.CanPublish ? "Rencana lembur valid dan siap dipublikasikan." : "Validasi selesai, tetapi masih ada konflik atau ketidaksesuaian policy.");
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanActionResponse>> PublishAsync(
            Guid planId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var plan = await _dbContext.TrxOvertimePlans
                    .Include(x => x.Details.Where(d => !d.IsDelete && !d.IsCancel))
                    .FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);
                if (plan == null) return NotFound<OvertimePlanActionResponse>();
                var periodGuard = await CheckPlanPeriodAsync(plan, cancellationToken);
                if (!periodGuard.IsWritable) return Conflict<OvertimePlanActionResponse>(periodGuard.Message);
                if (!CanEdit(plan.PlanStatus)) return Conflict<OvertimePlanActionResponse>("Rencana hanya dapat dipublikasikan dari status Draft atau Validated.");
                if (!plan.IsActive) return Conflict<OvertimePlanActionResponse>("Rencana nonaktif tidak dapat dipublikasikan.");

                var previous = plan.PlanStatus;
                var validation = await ValidateAndPersistAsync(plan, actorUserId, cancellationToken);
                if (!validation.CanPublish)
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return Conflict<OvertimePlanActionResponse>("Rencana belum dapat dipublikasikan karena masih memiliki validasi blocking.");
                }

                var now = DateTime.UtcNow;
                plan.PlanStatus = OvertimeValueConstants.PlanStatus.Published;
                plan.PublishedAt = now;
                plan.PublishedByUserId = actorUserId;
                plan.UpdateDateTime = now;
                plan.UpdateBy = actorUserId;
                foreach (var detail in plan.Details)
                {
                    detail.DetailStatus = OvertimeValueConstants.PlanDetailStatus.Published;
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;
                }
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return OvertimePlanningServiceResult<OvertimePlanActionResponse>.Ok(new OvertimePlanActionResponse
                {
                    OvertimePlanId = plan.Id,
                    PlanNumber = plan.PlanNumber,
                    PreviousStatus = previous,
                    CurrentStatus = plan.PlanStatus,
                    AffectedDetailCount = plan.Details.Count,
                    ActionAt = now
                }, "Rencana lembur berhasil dipublikasikan.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimePlanningServiceResult<OvertimePlanActionResponse>> CancelAsync(
            Guid planId,
            CancelOvertimePlanRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var plan = await _dbContext.TrxOvertimePlans
                    .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(x => x.GeneratedOvertimeRequest)
                    .FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);
                if (plan == null) return NotFound<OvertimePlanActionResponse>();
                var periodGuard = await CheckPlanPeriodAsync(plan, cancellationToken);
                if (!periodGuard.IsWritable) return Conflict<OvertimePlanActionResponse>(periodGuard.Message);
                if (plan.PlanStatus == OvertimeValueConstants.PlanStatus.Cancelled)
                    return Conflict<OvertimePlanActionResponse>("Rencana lembur sudah dibatalkan.");
                if (plan.PlanStatus == OvertimeValueConstants.PlanStatus.Closed)
                    return Conflict<OvertimePlanActionResponse>("Rencana lembur yang sudah Closed tidak dapat dibatalkan.");

                var generated = plan.Details.Select(x => x.GeneratedOvertimeRequest).Where(x => x != null && !x.IsDelete).ToList();
                var blocking = generated.FirstOrDefault(x => x!.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Draft &&
                    x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.NeedRevision &&
                    x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Cancelled);
                if (blocking != null)
                    return Conflict<OvertimePlanActionResponse>("Rencana tidak dapat dibatalkan karena terdapat overtime request yang sudah diproses lebih lanjut.");

                var generatedRequestIds = generated.Select(x => x!.Id).ToList();
                var generatedRequestDetails = generatedRequestIds.Count == 0
                    ? new List<TrxOvertimeRequestDetail>()
                    : await _dbContext.TrxOvertimeRequestDetails
                        .Where(x => generatedRequestIds.Contains(x.OvertimeRequestId) && !x.IsDelete)
                        .ToListAsync(cancellationToken);

                var previous = plan.PlanStatus;
                var now = DateTime.UtcNow;
                foreach (var overtimeRequest in generated.Where(x => x!.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Cancelled))
                {
                    overtimeRequest!.OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.Cancelled;
                    overtimeRequest.CancelledAt = now;
                    overtimeRequest.CancelledByUserId = actorUserId;
                    overtimeRequest.ApprovalNotes = request.Reason.Trim();
                    overtimeRequest.IsCancel = true;
                    overtimeRequest.CancelDateTime = now;
                    overtimeRequest.CancelBy = actorUserId;
                    overtimeRequest.UpdateDateTime = now;
                    overtimeRequest.UpdateBy = actorUserId;

                    foreach (var requestDetail in generatedRequestDetails.Where(x => x.OvertimeRequestId == overtimeRequest.Id && !x.IsCancel))
                    {
                        requestDetail.DetailStatus = "Cancelled";
                        requestDetail.IsActive = false;
                        requestDetail.IsCancel = true;
                        requestDetail.CancelDateTime = now;
                        requestDetail.CancelBy = actorUserId;
                        requestDetail.UpdateDateTime = now;
                        requestDetail.UpdateBy = actorUserId;
                        requestDetail.Notes = AppendNote(requestDetail.Notes, "Plan cancelled: " + request.Reason.Trim(), 1000);
                    }
                }

                foreach (var detail in plan.Details.Where(x => !x.IsCancel))
                {
                    detail.DetailStatus = OvertimeValueConstants.PlanDetailStatus.Cancelled;
                    detail.IsCancel = true;
                    detail.CancelDateTime = now;
                    detail.CancelBy = actorUserId;
                    detail.IsActive = false;
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;
                    detail.Notes = AppendNote(detail.Notes, "Plan cancelled: " + request.Reason.Trim(), 1000);
                }

                plan.PlanStatus = OvertimeValueConstants.PlanStatus.Cancelled;
                plan.IsCancel = true;
                plan.CancelDateTime = now;
                plan.CancelBy = actorUserId;
                plan.IsActive = false;
                plan.Notes = AppendNote(plan.Notes, "Cancelled: " + request.Reason.Trim(), 2000);
                plan.UpdateDateTime = now;
                plan.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return OvertimePlanningServiceResult<OvertimePlanActionResponse>.Ok(new OvertimePlanActionResponse
                {
                    OvertimePlanId = plan.Id,
                    PlanNumber = plan.PlanNumber,
                    PreviousStatus = previous,
                    CurrentStatus = plan.PlanStatus,
                    AffectedDetailCount = plan.Details.Count,
                    AffectedRequestCount = generated.Count,
                    ActionAt = now
                }, "Rencana lembur berhasil dibatalkan.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimePlanningServiceResult<GenerateOvertimeRequestsResponse>> GenerateRequestsAsync(
            Guid planId,
            GenerateOvertimeRequestsRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var plan = await _dbContext.TrxOvertimePlans
                    .Include(x => x.Details.Where(d => !d.IsDelete && !d.IsCancel))
                    .ThenInclude(x => x.GeneratedOvertimeRequest)
                    .FirstOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken);
                if (plan == null) return NotFound<GenerateOvertimeRequestsResponse>();
                var periodGuard = await CheckPlanPeriodAsync(plan, cancellationToken);
                if (!periodGuard.IsWritable) return Conflict<GenerateOvertimeRequestsResponse>(periodGuard.Message);
                if (plan.PlanStatus != OvertimeValueConstants.PlanStatus.Published &&
                    plan.PlanStatus != OvertimeValueConstants.PlanStatus.PartiallyConverted &&
                    plan.PlanStatus != OvertimeValueConstants.PlanStatus.Converted)
                    return Conflict<GenerateOvertimeRequestsResponse>("Overtime request hanya dapat dibuat dari rencana Published atau yang sedang dikonversi.");

                var selectedIds = (request.DetailIds ?? new List<Guid>()).Where(x => x != Guid.Empty).Distinct().ToHashSet();
                var selected = plan.Details
                    .Where(x => selectedIds.Count == 0 || selectedIds.Contains(x.Id))
                    .OrderBy(x => x.SequenceNumber)
                    .ToList();
                if (selected.Count == 0) return Fail<GenerateOvertimeRequestsResponse>("Tidak ada detail rencana yang dipilih.");

                var now = DateTime.UtcNow;
                var response = new GenerateOvertimeRequestsResponse
                {
                    OvertimePlanId = plan.Id,
                    PlanNumber = plan.PlanNumber,
                    PreviousStatus = plan.PlanStatus,
                    RequestedDetailCount = selected.Count,
                    GeneratedAt = now
                };
                var nextSequenceByDate = new Dictionary<DateOnly, int>();

                foreach (var detail in selected)
                {
                    if (detail.GeneratedOvertimeRequest != null && !detail.GeneratedOvertimeRequest.IsDelete)
                    {
                        if (!request.SkipExisting)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Conflict<GenerateOvertimeRequestsResponse>("Salah satu detail sudah memiliki overtime request. Aktifkan SkipExisting untuk menjalankan proses secara idempotent.");
                        }
                        response.ExistingRequestCount++;
                        response.Items.Add(new OvertimeGeneratedRequestItemResponse
                        {
                            PlanDetailId = detail.Id,
                            WorkforceProfileId = detail.WorkforceProfileId,
                            OvertimeRequestId = detail.GeneratedOvertimeRequest.Id,
                            RequestNumber = detail.GeneratedOvertimeRequest.RequestNumber,
                            RequestStatus = detail.GeneratedOvertimeRequest.OvertimeRequestStatus,
                            WasCreated = false,
                            Message = "Overtime request sudah pernah dibuat untuk detail ini."
                        });
                        continue;
                    }

                    if (!detail.IsActive || detail.DetailStatus != OvertimeValueConstants.PlanDetailStatus.Published || !IsPublishable(detail))
                    {
                        response.SkippedDetailCount++;
                        response.Items.Add(new OvertimeGeneratedRequestItemResponse
                        {
                            PlanDetailId = detail.Id,
                            WorkforceProfileId = detail.WorkforceProfileId,
                            WasCreated = false,
                            Message = "Detail dilewati karena belum valid, nonaktif, atau bukan status Published."
                        });
                        continue;
                    }

                    if (!nextSequenceByDate.TryGetValue(detail.OvertimeDate, out var nextSequence))
                    {
                        nextSequence = await GetNextRequestSequenceAsync(detail.OvertimeDate, cancellationToken);
                    }
                    nextSequenceByDate[detail.OvertimeDate] = nextSequence + 1;
                    var requestNumber = BuildRequestNumber(detail.OvertimeDate, nextSequence);
                    var overtimeRequest = BuildOvertimeRequest(plan, detail, requestNumber, actorUserId, now);
                    var overtimeRequestDetail = BuildOvertimeRequestDetail(detail, actorUserId, now);
                    overtimeRequestDetail.OvertimeRequestId = overtimeRequest.Id;
                    overtimeRequest.Details.Add(overtimeRequestDetail);
                    _dbContext.WfpOvertimeRequests.Add(overtimeRequest);
                    detail.DetailStatus = OvertimeValueConstants.PlanDetailStatus.RequestGenerated;
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;

                    response.CreatedRequestCount++;
                    response.Items.Add(new OvertimeGeneratedRequestItemResponse
                    {
                        PlanDetailId = detail.Id,
                        WorkforceProfileId = detail.WorkforceProfileId,
                        OvertimeRequestId = overtimeRequest.Id,
                        RequestNumber = overtimeRequest.RequestNumber,
                        RequestStatus = overtimeRequest.OvertimeRequestStatus,
                        WasCreated = true,
                        Message = "Overtime request Draft berhasil dibuat."
                    });
                }

                var allActive = plan.Details.Where(x => x.IsActive).ToList();
                var convertedCount = allActive.Count(x => x.DetailStatus == OvertimeValueConstants.PlanDetailStatus.RequestGenerated || x.GeneratedOvertimeRequest != null);
                plan.PlanStatus = convertedCount == 0
                    ? OvertimeValueConstants.PlanStatus.Published
                    : convertedCount == allActive.Count
                        ? OvertimeValueConstants.PlanStatus.Converted
                        : OvertimeValueConstants.PlanStatus.PartiallyConverted;
                plan.UpdateDateTime = now;
                plan.UpdateBy = actorUserId;
                response.CurrentStatus = plan.PlanStatus;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return OvertimePlanningServiceResult<GenerateOvertimeRequestsResponse>.Ok(response,
                    response.CreatedRequestCount > 0
                        ? "Overtime request Draft berhasil dibuat secara idempotent dari rencana lembur."
                        : "Tidak ada overtime request baru yang dibuat.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<OvertimePlanValidationResponse> ValidateAndPersistAsync(
            TrxOvertimePlan plan,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var previous = plan.PlanStatus;
            var detailResults = new List<OvertimePlanDetailValidationResponse>();
            foreach (var detail in plan.Details.OrderBy(x => x.SequenceNumber))
            {
                var request = ToUpdateRequest(detail);
                var evaluation = await EvaluateDetailAsync(plan, detail.Id, request, null, cancellationToken);
                evaluation.Response.DetailId = detail.Id;
                detailResults.Add(evaluation.Response);
                ApplyEvaluation(detail, request, evaluation);
                detail.DetailStatus = evaluation.Response.CanPublish
                    ? OvertimeValueConstants.PlanDetailStatus.Validated
                    : OvertimeValueConstants.PlanDetailStatus.Draft;
                detail.UpdateDateTime = DateTime.UtcNow;
                detail.UpdateBy = actorUserId;
            }

            var issues = new List<OvertimeValidationIssueResponse>();
            if (plan.Details.Count == 0)
                issues.Add(Issue("PLAN_EMPTY", "Error", "Rencana lembur belum memiliki detail employee.", true, "details"));
            if (!plan.IsActive)
                issues.Add(Issue("PLAN_INACTIVE", "Error", "Rencana lembur nonaktif tidak dapat dipublikasikan.", true, "isActive"));
            issues.AddRange(detailResults.SelectMany(x => x.Issues.Where(i => i.IsBlocking))
                .GroupBy(x => new { x.Code, x.Message })
                .Select(g => new OvertimeValidationIssueResponse
                {
                    Code = g.Key.Code,
                    Severity = "Error",
                    Message = g.Count() + " detail: " + g.Key.Message,
                    IsBlocking = true
                }));

            var canPublish = plan.Details.Count > 0 && issues.All(x => !x.IsBlocking) && detailResults.All(x => x.CanPublish);
            var now = DateTime.UtcNow;
            plan.PlanStatus = canPublish ? OvertimeValueConstants.PlanStatus.Validated : OvertimeValueConstants.PlanStatus.Draft;
            plan.ValidatedAt = now;
            plan.ValidatedByUserId = actorUserId;
            plan.UpdateDateTime = now;
            plan.UpdateBy = actorUserId;

            return new OvertimePlanValidationResponse
            {
                OvertimePlanId = plan.Id,
                PlanNumber = plan.PlanNumber,
                PreviousStatus = previous,
                CurrentStatus = plan.PlanStatus,
                CanPublish = canPublish,
                TotalDetail = detailResults.Count,
                ValidDetail = detailResults.Count(x => x.CanPublish),
                ConflictDetail = detailResults.Count(x => !x.CanPublish),
                TotalPlannedMinutes = detailResults.Sum(x => x.RoundedPlannedMinutes),
                ValidatedAt = now,
                Issues = issues,
                DetailValidations = detailResults
            };
        }

        private async Task<DetailEvaluation> EvaluateDetailAsync(
            TrxOvertimePlan plan,
            Guid? excludeDetailId,
            CreateOvertimePlanDetailRequest request,
            IReadOnlyCollection<TrxOvertimePlanDetail>? stagedDetails,
            CancellationToken cancellationToken)
        {
            var response = new OvertimePlanDetailValidationResponse
            {
                DetailId = excludeDetailId,
                WorkforceProfileId = request.WorkforceProfileId,
                OvertimeDate = request.OvertimeDate,
                PlannedStartAt = NormalizeUtc(request.PlannedStartAt),
                PlannedEndAt = NormalizeUtc(request.PlannedEndAt),
                EvaluatedChecks = new List<string> { "workforce", "organizationAssignment", "workScheduleAssignment", "shiftAssignment", "policy", "ratePreview", "planOverlap", "requestOverlap", "dailyWeeklyMonthlyLimit" },
                DeferredChecks = new List<string> { "approvedLeaveDeepMatch", "trainingSessionConflict", "minimumRestAcrossPublishedRoster", "attendanceActualMatch" }
            };

            var snapshot = new DetailSnapshot();
            var profile = await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.WorkforceProfileId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken);
            if (profile == null)
            {
                response.Issues.Add(Issue("WORKFORCE_NOT_FOUND", "Error", "Workforce profile tidak ditemukan atau tidak aktif.", true, "workforceProfileId"));
                response.CanPersist = false;
                return new DetailEvaluation(response, snapshot);
            }
            response.WorkforceProfileCode = profile.ProfileCode;
            response.WorkforceDisplayName = profile.DisplayName;

            var startAt = response.PlannedStartAt;
            var endAt = response.PlannedEndAt;
            if (endAt <= startAt)
                response.Issues.Add(Issue("INVALID_INTERVAL", "Error", "Planned end harus lebih besar dari planned start.", true, "plannedEndAt"));
            var rawMinutes = endAt > startAt ? (int)Math.Ceiling((endAt - startAt).TotalMinutes) : 0;
            response.RawPlannedMinutes = rawMinutes;
            if (rawMinutes > 24 * 60)
                response.Issues.Add(Issue("INTERVAL_TOO_LONG", "Error", "Satu detail planning tidak boleh melebihi 24 jam.", true, "plannedEndAt"));
            if (request.OvertimeDate < plan.PlanStartDate || request.OvertimeDate > plan.PlanEndDate)
                response.Issues.Add(Issue("DATE_OUTSIDE_PLAN", "Error", "Tanggal lembur berada di luar periode plan.", true, "overtimeDate"));

            var dayType = NormalizeToken(request.DayType, OvertimeValueConstants.DayType.All);
            if (dayType == null)
                response.Issues.Add(Issue("INVALID_DAY_TYPE", "Error", "Day type tidak valid.", true, "dayType"));
            var category = NormalizeToken(request.OvertimeCategory, OvertimeValueConstants.OvertimeCategory.All);
            if (category == null)
                response.Issues.Add(Issue("INVALID_CATEGORY", "Error", "Overtime category tidak valid.", true, "overtimeCategory"));
            snapshot.DayType = dayType ?? request.DayType.Trim();
            snapshot.OvertimeCategory = category ?? request.OvertimeCategory.Trim();

            var employee = request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty
                ? await _dbContext.MstEmployees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.EmployeeId && x.WorkforceProfileId == request.WorkforceProfileId && !x.IsDelete && !x.IsCancel, cancellationToken)
                : await _dbContext.MstEmployees.AsNoTracking().FirstOrDefaultAsync(x => x.WorkforceProfileId == request.WorkforceProfileId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken);
            if (request.EmployeeId.HasValue && request.EmployeeId != Guid.Empty && employee == null)
                response.Issues.Add(Issue("EMPLOYEE_MISMATCH", "Error", "Employee tidak sesuai dengan workforce profile.", true, "employeeId"));
            snapshot.EmployeeId = employee?.Id;

            var dateTime = DateTime.SpecifyKind(request.OvertimeDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            WfpOrganizationAssignment? orgAssignment;
            if (request.OrganizationAssignmentId.HasValue && request.OrganizationAssignmentId != Guid.Empty)
            {
                orgAssignment = await _dbContext.WfpOrganizationAssignments.AsNoTracking().FirstOrDefaultAsync(x =>
                    x.Id == request.OrganizationAssignmentId && x.WorkforceProfileId == request.WorkforceProfileId && !x.IsDelete && !x.IsCancel && x.IsActive &&
                    x.EffectiveStartDate.Date <= dateTime.Date && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= dateTime.Date), cancellationToken);
            }
            else
            {
                orgAssignment = await _dbContext.WfpOrganizationAssignments.AsNoTracking()
                    .Where(x => x.WorkforceProfileId == request.WorkforceProfileId && !x.IsDelete && !x.IsCancel && x.IsActive &&
                        x.EffectiveStartDate.Date <= dateTime.Date && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= dateTime.Date))
                    .OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            if (orgAssignment == null)
                response.Issues.Add(Issue("ORGANIZATION_ASSIGNMENT_NOT_FOUND", "Error", "Organization assignment efektif tidak ditemukan.", true, "organizationAssignmentId"));
            else
            {
                snapshot.OrganizationAssignmentId = orgAssignment.Id;
                snapshot.HospitalSiteId = orgAssignment.HospitalSiteId;
                snapshot.OrganizationUnitId = orgAssignment.OrganizationUnitId;
                snapshot.DepartmentId = orgAssignment.DepartmentId;
                snapshot.PositionId = orgAssignment.PositionId;
                snapshot.CostCenterId = orgAssignment.CostCenterId;
                snapshot.WorkLocationId = orgAssignment.WorkLocationId;
                response.OrganizationAssignmentId = orgAssignment.Id;
                if (plan.LegalEntityId.HasValue && orgAssignment.LegalEntityId != plan.LegalEntityId) response.Issues.Add(Issue("LEGAL_ENTITY_SCOPE_MISMATCH", "Error", "Assignment employee tidak berada pada legal entity plan.", true));
                if (plan.HospitalSiteId.HasValue && orgAssignment.HospitalSiteId != plan.HospitalSiteId) response.Issues.Add(Issue("SITE_SCOPE_MISMATCH", "Error", "Assignment employee tidak berada pada hospital site plan.", true));
                if (plan.OrganizationUnitId.HasValue && orgAssignment.OrganizationUnitId != plan.OrganizationUnitId) response.Issues.Add(Issue("ORG_SCOPE_MISMATCH", "Error", "Assignment employee tidak berada pada organization unit plan.", true));
                if (plan.DepartmentId.HasValue && orgAssignment.DepartmentId != plan.DepartmentId) response.Issues.Add(Issue("DEPARTMENT_SCOPE_MISMATCH", "Error", "Assignment employee tidak berada pada department plan.", true));
            }

            var periodGuard = await _periodGuard.CheckDateAsync(
                request.OvertimeDate,
                orgAssignment?.LegalEntityId ?? plan.LegalEntityId,
                orgAssignment?.HospitalSiteId ?? plan.HospitalSiteId,
                orgAssignment?.OrganizationUnitId ?? plan.OrganizationUnitId,
                orgAssignment?.DepartmentId ?? plan.DepartmentId,
                cancellationToken);
            if (!periodGuard.IsWritable)
                response.Issues.Add(Issue("OVERTIME_PERIOD_CLOSED", "Error", periodGuard.Message, true, "overtimeDate", periodGuard.OvertimePeriodId));

            WfpWorkScheduleAssignment? scheduleAssignment = null;
            if (request.WorkScheduleAssignmentId.HasValue && request.WorkScheduleAssignmentId != Guid.Empty)
            {
                scheduleAssignment = await _dbContext.WfpWorkScheduleAssignments.AsNoTracking().FirstOrDefaultAsync(x =>
                    x.Id == request.WorkScheduleAssignmentId && x.WorkforceProfileId == request.WorkforceProfileId && !x.IsDelete && !x.IsCancel && x.IsActive &&
                    x.EffectiveStartDate <= request.OvertimeDate && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= request.OvertimeDate), cancellationToken);
                if (scheduleAssignment == null) response.Issues.Add(Issue("SCHEDULE_ASSIGNMENT_INVALID", "Error", "Work schedule assignment tidak valid untuk tanggal lembur.", true, "workScheduleAssignmentId"));
            }
            else
            {
                scheduleAssignment = await _dbContext.WfpWorkScheduleAssignments.AsNoTracking()
                    .Where(x => x.WorkforceProfileId == request.WorkforceProfileId && !x.IsDelete && !x.IsCancel && x.IsActive &&
                        x.EffectiveStartDate <= request.OvertimeDate && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= request.OvertimeDate))
                    .OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            if (scheduleAssignment == null)
                response.Issues.Add(Issue("SCHEDULE_ASSIGNMENT_NOT_FOUND", "Warning", "Work schedule assignment efektif tidak ditemukan. Detail dapat disimpan tetapi tidak dapat dipublikasikan sebelum jadwal tersedia.", true));
            else
            {
                snapshot.WorkScheduleAssignmentId = scheduleAssignment.Id;
                snapshot.WorkScheduleId = request.WorkScheduleId.HasValue && request.WorkScheduleId != Guid.Empty ? request.WorkScheduleId : scheduleAssignment.WorkScheduleId;
                response.WorkScheduleAssignmentId = scheduleAssignment.Id;
            }

            if (request.ShiftAssignmentId.HasValue && request.ShiftAssignmentId != Guid.Empty)
            {
                var validShiftAssignment = await _dbContext.TrxShiftAssignments.AsNoTracking().AnyAsync(x =>
                    x.Id == request.ShiftAssignmentId && x.WorkforceProfileId == request.WorkforceProfileId && x.ShiftDate == request.OvertimeDate && !x.IsDelete && !x.IsCancel, cancellationToken);
                if (!validShiftAssignment)
                    response.Issues.Add(Issue("SHIFT_ASSIGNMENT_INVALID", "Error", "Shift assignment tidak sesuai dengan workforce dan tanggal lembur.", true, "shiftAssignmentId"));
                else
                {
                    snapshot.ShiftAssignmentId = request.ShiftAssignmentId;
                    response.ShiftAssignmentId = request.ShiftAssignmentId;
                }
            }
            snapshot.RosterPeriodId = NormalizeGuid(request.RosterPeriodId) ?? plan.RosterPeriodId;

            if (snapshot.WorkScheduleId.HasValue)
            {
                var workScheduleValid = await _dbContext.MstWorkSchedules.AsNoTracking().AnyAsync(x =>
                    x.Id == snapshot.WorkScheduleId.Value && !x.IsDelete && !x.IsCancel && x.IsActive,
                    cancellationToken);
                if (!workScheduleValid)
                    response.Issues.Add(Issue("WORK_SCHEDULE_INVALID", "Error", "Work schedule tidak ditemukan atau tidak aktif.", true, "workScheduleId"));
            }

            snapshot.ShiftId = NormalizeGuid(request.ShiftId);
            if (snapshot.ShiftId.HasValue)
            {
                var shift = await _dbContext.MstShifts.AsNoTracking().FirstOrDefaultAsync(x =>
                    x.Id == snapshot.ShiftId.Value && !x.IsDelete && !x.IsCancel && x.IsActive,
                    cancellationToken);
                if (shift == null)
                    response.Issues.Add(Issue("SHIFT_INVALID", "Error", "Shift tidak ditemukan atau tidak aktif.", true, "shiftId"));
                else if (!shift.AllowOvertime)
                    response.Issues.Add(Issue("SHIFT_OVERTIME_NOT_ALLOWED", "Error", "Shift yang dipilih tidak mengizinkan overtime.", true, "shiftId", shift.Id));
                else if (snapshot.WorkScheduleId.HasValue && shift.WorkScheduleId.HasValue && shift.WorkScheduleId != snapshot.WorkScheduleId)
                    response.Issues.Add(Issue("SHIFT_SCHEDULE_MISMATCH", "Error", "Shift tidak berada pada work schedule yang dipilih.", true, "shiftId", shift.Id));
            }

            MstOvertimePolicy? policy = null;
            if (request.OvertimePolicyId.HasValue && request.OvertimePolicyId != Guid.Empty)
            {
                policy = await _dbContext.MstOvertimePolicies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.OvertimePolicyId && !x.IsDelete && !x.IsCancel && x.IsActive &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= dateTime.Date) && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= dateTime.Date), cancellationToken);
                if (policy == null) response.Issues.Add(Issue("POLICY_INVALID", "Error", "Overtime policy tidak aktif atau di luar periode efektif.", true, "overtimePolicyId"));
            }
            else
            {
                var resolution = await _policyResolver.ResolveAsync(new OvertimePolicyResolveRequest
                {
                    WorkforceProfileId = request.WorkforceProfileId,
                    LegalEntityId = orgAssignment?.LegalEntityId ?? plan.LegalEntityId,
                    HospitalSiteId = orgAssignment?.HospitalSiteId ?? plan.HospitalSiteId,
                    OrganizationUnitId = orgAssignment?.OrganizationUnitId ?? plan.OrganizationUnitId,
                    EffectiveDate = dateTime
                }, cancellationToken);
                if (!resolution.IsResolved || resolution.SelectedPolicy == null)
                    response.Issues.Add(Issue("POLICY_NOT_RESOLVED", "Error", resolution.Message, true, "overtimePolicyId"));
                else if (resolution.IsAmbiguous)
                    response.Issues.Add(Issue("POLICY_AMBIGUOUS", "Error", resolution.Message, true, "overtimePolicyId", resolution.SelectedPolicy.Id));
                else
                    policy = await _dbContext.MstOvertimePolicies.AsNoTracking().FirstAsync(x => x.Id == resolution.SelectedPolicy.Id, cancellationToken);
            }

            if (policy != null)
            {
                if (policy.LegalEntityId.HasValue && orgAssignment?.LegalEntityId != policy.LegalEntityId)
                    response.Issues.Add(Issue("POLICY_LEGAL_ENTITY_MISMATCH", "Error", "Overtime policy tidak sesuai dengan legal entity employee.", true, "overtimePolicyId", policy.Id));
                if (policy.HospitalSiteId.HasValue && orgAssignment?.HospitalSiteId != policy.HospitalSiteId)
                    response.Issues.Add(Issue("POLICY_SITE_MISMATCH", "Error", "Overtime policy tidak sesuai dengan hospital site employee.", true, "overtimePolicyId", policy.Id));
                if (policy.OrganizationUnitId.HasValue && orgAssignment?.OrganizationUnitId != policy.OrganizationUnitId)
                    response.Issues.Add(Issue("POLICY_ORG_MISMATCH", "Error", "Overtime policy tidak sesuai dengan organization unit employee.", true, "overtimePolicyId", policy.Id));
                if (policy.EmployeeCategoryId.HasValue && employee?.EmployeeCategoryId != policy.EmployeeCategoryId)
                    response.Issues.Add(Issue("POLICY_EMPLOYEE_CATEGORY_MISMATCH", "Error", "Overtime policy tidak sesuai dengan employee category.", true, "overtimePolicyId", policy.Id));
                if (policy.EmploymentTypeId.HasValue && employee?.EmploymentTypeId != policy.EmploymentTypeId)
                    response.Issues.Add(Issue("POLICY_EMPLOYMENT_TYPE_MISMATCH", "Error", "Overtime policy tidak sesuai dengan employment type.", true, "overtimePolicyId", policy.Id));

                snapshot.OvertimePolicyId = policy.Id;
                response.OvertimePolicyId = policy.Id;
                response.OvertimePolicyCode = policy.OvertimePolicyCode;
                response.OvertimePolicyName = policy.OvertimePolicyName;
                if (!IsCategoryAllowed(policy, snapshot.OvertimeCategory))
                    response.Issues.Add(Issue("CATEGORY_NOT_ALLOWED", "Error", "Kategori lembur tidak diizinkan oleh policy.", true, "overtimeCategory", policy.Id));
                if (!IsDayTypeAllowed(policy, snapshot.DayType))
                    response.Issues.Add(Issue("DAY_TYPE_NOT_ALLOWED", "Error", "Day type tidak diizinkan oleh policy.", true, "dayType", policy.Id));

                var breakMinutes = policy.DeductBreakMinutes
                    ? Math.Max(request.EstimatedBreakMinutes, policy.BreakDeductionMinutes)
                    : Math.Max(0, request.EstimatedBreakMinutes);
                breakMinutes = Math.Min(breakMinutes, rawMinutes);
                var eligible = Math.Max(0, rawMinutes - breakMinutes - Math.Max(0, policy.OvertimeThresholdMinutes));
                var rounded = RoundMinutes(eligible, policy.RoundingIntervalMinutes, policy.RoundingMethod);
                response.EstimatedBreakMinutes = breakMinutes;
                response.EligiblePlannedMinutes = eligible;
                response.RoundedPlannedMinutes = rounded;
                snapshot.EstimatedBreakMinutes = breakMinutes;
                snapshot.PlannedMinutes = rounded;

                if (rounded < policy.MinimumOvertimeMinutes)
                    response.Issues.Add(Issue("BELOW_MINIMUM", "Error", "Menit lembur setelah threshold, break, dan rounding berada di bawah minimum policy.", true));
                if (policy.MaximumOvertimeMinutesPerDay.HasValue && rounded > policy.MaximumOvertimeMinutesPerDay.Value)
                    response.Issues.Add(Issue("DAILY_LIMIT_EXCEEDED", "Error", "Menit lembur melebihi batas harian policy.", true));

                var weekStart = GetWeekStart(request.OvertimeDate);
                var weekEnd = weekStart.AddDays(6);
                var monthStart = new DateOnly(request.OvertimeDate.Year, request.OvertimeDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var weeklyExisting = await SumExistingMinutesAsync(request.WorkforceProfileId, weekStart, weekEnd, excludeDetailId, cancellationToken);
                var monthlyExisting = await SumExistingMinutesAsync(request.WorkforceProfileId, monthStart, monthEnd, excludeDetailId, cancellationToken);
                var weeklyStaged = stagedDetails?
                    .Where(x => x.WorkforceProfileId == request.WorkforceProfileId && x.OvertimeDate >= weekStart && x.OvertimeDate <= weekEnd)
                    .Sum(x => x.PlannedMinutes) ?? 0;
                var monthlyStaged = stagedDetails?
                    .Where(x => x.WorkforceProfileId == request.WorkforceProfileId && x.OvertimeDate >= monthStart && x.OvertimeDate <= monthEnd)
                    .Sum(x => x.PlannedMinutes) ?? 0;
                if (policy.MaximumOvertimeMinutesPerWeek.HasValue && weeklyExisting + weeklyStaged + rounded > policy.MaximumOvertimeMinutesPerWeek.Value)
                    response.Issues.Add(Issue("WEEKLY_LIMIT_EXCEEDED", "Error", "Akumulasi menit lembur melebihi batas mingguan policy.", true));
                if (policy.MaximumOvertimeMinutesPerMonth.HasValue && monthlyExisting + monthlyStaged + rounded > policy.MaximumOvertimeMinutesPerMonth.Value)
                    response.Issues.Add(Issue("MONTHLY_LIMIT_EXCEEDED", "Error", "Akumulasi menit lembur melebihi batas bulanan policy.", true));

                if (rounded > 0 && dayType != null)
                {
                    var rate = await _rateResolver.ResolveAsync(new OvertimeRateResolveRequest
                    {
                        OvertimePolicyId = policy.Id,
                        DayType = dayType,
                        EffectiveDate = dateTime,
                        MinutePosition = 0,
                        EligibleMinutes = rounded,
                        OccurrenceTime = TimeOnly.FromDateTime(startAt)
                    }, cancellationToken);
                    if (!rate.IsResolved || rate.SelectedRate == null)
                        response.Issues.Add(Issue("RATE_NOT_RESOLVED", "Error", rate.Message, true));
                    else if (rate.IsAmbiguous)
                        response.Issues.Add(Issue("RATE_AMBIGUOUS", "Error", rate.Message, true, referenceId: rate.SelectedRate.Id));
                    else
                    {
                        response.PreviewOvertimeRateId = rate.SelectedRate.Id;
                        response.PreviewOvertimeRateCode = rate.SelectedRate.OvertimeRateCode;
                        response.PreviewRateMultiplier = rate.SelectedRate.RateMultiplier;
                    }
                }
            }
            else
            {
                response.EstimatedBreakMinutes = Math.Max(0, request.EstimatedBreakMinutes);
                response.EligiblePlannedMinutes = Math.Max(0, rawMinutes - response.EstimatedBreakMinutes);
                response.RoundedPlannedMinutes = response.EligiblePlannedMinutes;
                snapshot.EstimatedBreakMinutes = response.EstimatedBreakMinutes;
                snapshot.PlannedMinutes = response.RoundedPlannedMinutes;
            }

            var overlapPlan = await _dbContext.TrxOvertimePlanDetails.AsNoTracking().AnyAsync(x =>
                x.WorkforceProfileId == request.WorkforceProfileId && !x.IsDelete && !x.IsCancel && x.IsActive &&
                (!excludeDetailId.HasValue || x.Id != excludeDetailId.Value) && x.PlannedStartAt < endAt && startAt < x.PlannedEndAt,
                cancellationToken);
            var overlapStaged = stagedDetails?.Any(x => x.WorkforceProfileId == request.WorkforceProfileId && x.PlannedStartAt < endAt && startAt < x.PlannedEndAt) == true;
            var overlapRequest = await _dbContext.WfpOvertimeRequests.AsNoTracking().AnyAsync(x =>
                x.WorkforceProfileId == request.WorkforceProfileId && !x.IsDelete && !x.IsCancel && x.IsActive &&
                x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Rejected && x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Cancelled &&
                x.PlannedStartAt.HasValue && x.PlannedEndAt.HasValue && x.PlannedStartAt.Value < endAt && startAt < x.PlannedEndAt.Value,
                cancellationToken);
            if (overlapPlan || overlapStaged || overlapRequest)
            {
                response.HasScheduleConflict = true;
                response.Issues.Add(Issue("OVERTIME_OVERLAP", "Error", "Terdapat planning atau overtime request lain yang waktunya overlap untuk employee ini.", true));
            }

            response.HasWorkHourLimitConflict = response.Issues.Any(x => x.Code == "DAILY_LIMIT_EXCEEDED" || x.Code == "WEEKLY_LIMIT_EXCEEDED" || x.Code == "MONTHLY_LIMIT_EXCEEDED");
            response.IsPolicyCompliant = policy != null && !response.Issues.Any(x => x.IsBlocking && (x.Code.Contains("POLICY") || x.Code.Contains("RATE") || x.Code.Contains("LIMIT") || x.Code.Contains("MINIMUM") || x.Code.Contains("CATEGORY") || x.Code.Contains("DAY_TYPE")));
            response.CanPersist = !response.Issues.Any(x => x.IsBlocking && (x.Code == "WORKFORCE_NOT_FOUND" || x.Code == "EMPLOYEE_MISMATCH" || x.Code == "INVALID_INTERVAL" || x.Code == "INTERVAL_TOO_LONG" || x.Code == "DATE_OUTSIDE_PLAN" || x.Code == "INVALID_DAY_TYPE" || x.Code == "INVALID_CATEGORY" || x.Code == "ORGANIZATION_ASSIGNMENT_NOT_FOUND"));
            response.CanPublish = response.CanPersist && !response.Issues.Any(x => x.IsBlocking) && response.IsPolicyCompliant && !response.HasScheduleConflict;
            return new DetailEvaluation(response, snapshot);
        }

        private async Task<int> SumExistingMinutesAsync(
            Guid workforceProfileId,
            DateOnly start,
            DateOnly end,
            Guid? excludeDetailId,
            CancellationToken cancellationToken)
        {
            var planned = await _dbContext.TrxOvertimePlanDetails.AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && x.OvertimeDate >= start && x.OvertimeDate <= end &&
                    !x.IsDelete && !x.IsCancel && x.IsActive && (!excludeDetailId.HasValue || x.Id != excludeDetailId.Value))
                .SumAsync(x => (int?)x.PlannedMinutes, cancellationToken) ?? 0;

            // Request hasil konversi plan tidak dijumlahkan lagi karena menitnya sudah diwakili oleh plan detail.
            var requested = await _dbContext.WfpOvertimeRequests.AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && x.OvertimeDate >= start && x.OvertimeDate <= end &&
                    !x.SourceOvertimePlanDetailId.HasValue &&
                    !x.IsDelete && !x.IsCancel && x.IsActive &&
                    x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Rejected &&
                    x.OvertimeRequestStatus != OvertimeValueConstants.RequestStatus.Cancelled)
                .SumAsync(x => (int?)x.RequestedMinutes, cancellationToken) ?? 0;
            return planned + requested;
        }

        private static TrxOvertimePlanDetail BuildDetailEntity(
            Guid planId,
            int sequence,
            CreateOvertimePlanDetailRequest request,
            DetailEvaluation evaluation,
            Guid actorUserId,
            DateTime now)
        {
            var entity = new TrxOvertimePlanDetail
            {
                Id = Guid.NewGuid(),
                OvertimePlanId = planId,
                SequenceNumber = sequence,
                WorkforceProfileId = request.WorkforceProfileId,
                CreateDateTime = now,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                IsActive = true,
                DetailStatus = OvertimeValueConstants.PlanDetailStatus.Draft
            };
            ApplyEvaluation(entity, request, evaluation);
            return entity;
        }

        private static void ApplyEvaluation(
            TrxOvertimePlanDetail entity,
            CreateOvertimePlanDetailRequest request,
            DetailEvaluation evaluation)
        {
            var s = evaluation.Snapshot;
            entity.WorkforceProfileId = request.WorkforceProfileId;
            entity.EmployeeId = s.EmployeeId;
            entity.OrganizationAssignmentId = s.OrganizationAssignmentId;
            entity.HospitalSiteId = s.HospitalSiteId;
            entity.OrganizationUnitId = s.OrganizationUnitId;
            entity.DepartmentId = s.DepartmentId;
            entity.PositionId = s.PositionId;
            entity.CostCenterId = s.CostCenterId;
            entity.WorkLocationId = s.WorkLocationId;
            entity.WorkScheduleAssignmentId = s.WorkScheduleAssignmentId;
            entity.RosterPeriodId = s.RosterPeriodId;
            entity.ShiftAssignmentId = s.ShiftAssignmentId;
            entity.WorkScheduleId = s.WorkScheduleId;
            entity.ShiftId = s.ShiftId;
            entity.OvertimePolicyId = s.OvertimePolicyId;
            entity.OvertimeDate = request.OvertimeDate;
            entity.PlannedEndDate = request.PlannedEndDate ?? DateOnly.FromDateTime(evaluation.Response.PlannedEndAt);
            entity.PlannedStartAt = evaluation.Response.PlannedStartAt;
            entity.PlannedEndAt = evaluation.Response.PlannedEndAt;
            entity.PlannedMinutes = s.PlannedMinutes;
            entity.EstimatedBreakMinutes = s.EstimatedBreakMinutes;
            entity.DayType = s.DayType;
            entity.OvertimeCategory = s.OvertimeCategory;
            entity.WorkDescription = request.WorkDescription.Trim();
            entity.Notes = NormalizeText(request.Notes);
            entity.HasScheduleConflict = evaluation.Response.HasScheduleConflict;
            entity.HasLeaveConflict = evaluation.Response.HasLeaveConflict;
            entity.HasTrainingConflict = evaluation.Response.HasTrainingConflict;
            entity.HasMinimumRestConflict = evaluation.Response.HasMinimumRestConflict;
            entity.HasWorkHourLimitConflict = evaluation.Response.HasWorkHourLimitConflict;
            entity.IsPolicyCompliant = evaluation.Response.IsPolicyCompliant;
            entity.ValidationResultJson = SerializeValidation(evaluation.Response);
        }

        private static WfpOvertimeRequest BuildOvertimeRequest(
            TrxOvertimePlan plan,
            TrxOvertimePlanDetail detail,
            string requestNumber,
            Guid actorUserId,
            DateTime now) => new()
        {
            Id = Guid.NewGuid(),
            RequestNumber = requestNumber,
            WorkforceProfileId = detail.WorkforceProfileId,
            EmployeeId = detail.EmployeeId,
            OrganizationAssignmentId = detail.OrganizationAssignmentId,
            HospitalSiteId = detail.HospitalSiteId,
            OrganizationUnitId = detail.OrganizationUnitId,
            DepartmentId = detail.DepartmentId,
            PositionId = detail.PositionId,
            CostCenterId = detail.CostCenterId,
            OvertimePolicyId = detail.OvertimePolicyId,
            SourceOvertimePlanDetailId = detail.Id,
            RequestSource = OvertimeValueConstants.RequestSource.ManagerPlanning,
            WorkScheduleAssignmentId = detail.WorkScheduleAssignmentId,
            RosterPeriodId = detail.RosterPeriodId,
            ShiftAssignmentId = detail.ShiftAssignmentId,
            WorkScheduleId = detail.WorkScheduleId,
            ShiftId = detail.ShiftId,
            OvertimeDate = detail.OvertimeDate,
            PlannedEndDate = detail.PlannedEndDate,
            PlannedStartAt = detail.PlannedStartAt,
            PlannedEndAt = detail.PlannedEndAt,
            RequestedStartTime = TimeOnly.FromDateTime(detail.PlannedStartAt),
            RequestedEndTime = TimeOnly.FromDateTime(detail.PlannedEndAt),
            RequestedMinutes = detail.PlannedMinutes,
            ApprovedMinutes = 0,
            EstimatedBreakMinutes = detail.EstimatedBreakMinutes,
            CurrencyCode = "IDR",
            Reason = plan.Reason,
            WorkDescription = detail.WorkDescription,
            IsUrgent = detail.OvertimeCategory == OvertimeValueConstants.OvertimeCategory.Emergency,
            IsBeforeShift = detail.OvertimeCategory == OvertimeValueConstants.OvertimeCategory.BeforeShift,
            IsAfterShift = detail.OvertimeCategory == OvertimeValueConstants.OvertimeCategory.AfterShift,
            IsRestDay = detail.DayType == OvertimeValueConstants.DayType.RestDay,
            IsHoliday = detail.DayType == OvertimeValueConstants.DayType.Holiday || detail.DayType == OvertimeValueConstants.DayType.SpecialHoliday,
            HasScheduleConflict = detail.HasScheduleConflict,
            HasLeaveConflict = detail.HasLeaveConflict,
            HasTrainingConflict = detail.HasTrainingConflict,
            HasMinimumRestConflict = detail.HasMinimumRestConflict,
            HasWorkHourLimitConflict = detail.HasWorkHourLimitConflict,
            IsPolicyCompliant = detail.IsPolicyCompliant,
            ValidationResultJson = detail.ValidationResultJson,
            OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.Draft,
            IsActive = true,
            CreateDateTime = now,
            CreateBy = actorUserId,
            UpdateBy = actorUserId
        };

        private static TrxOvertimeRequestDetail BuildOvertimeRequestDetail(
            TrxOvertimePlanDetail detail,
            Guid actorUserId,
            DateTime now) => new()
        {
            Id = Guid.NewGuid(),
            SequenceNumber = 1,
            OvertimeDate = detail.OvertimeDate,
            WorkScheduleId = detail.WorkScheduleId,
            ShiftId = detail.ShiftId,
            ShiftAssignmentId = detail.ShiftAssignmentId,
            PlannedStartAt = detail.PlannedStartAt,
            PlannedEndAt = detail.PlannedEndAt,
            RequestedMinutes = detail.PlannedMinutes,
            ApprovedMinutes = 0,
            BreakMinutes = detail.EstimatedBreakMinutes,
            DayType = detail.DayType,
            OvertimeCategory = detail.OvertimeCategory,
            CurrencyCode = "IDR",
            WorkDescription = detail.WorkDescription,
            Notes = detail.Notes,
            DetailStatus = OvertimeValueConstants.RequestStatus.Draft,
            IsActive = true,
            CreateDateTime = now,
            CreateBy = actorUserId,
            UpdateBy = actorUserId
        };

        private Task<OvertimePeriodGuardResult> CheckPlanPeriodAsync(
            TrxOvertimePlan plan,
            CancellationToken cancellationToken) =>
            _periodGuard.CheckRangeAsync(
                plan.PlanStartDate,
                plan.PlanEndDate,
                plan.LegalEntityId,
                plan.HospitalSiteId,
                plan.OrganizationUnitId,
                plan.DepartmentId,
                cancellationToken);

        private async Task<string?> ValidateHeaderAsync(
            Guid? id,
            string planTitle,
            DateOnly start,
            DateOnly end,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? organizationUnitId,
            Guid? departmentId,
            Guid? costCenterId,
            Guid? workLocationId,
            Guid? rosterPeriodId,
            string reason,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(planTitle)) return "Judul rencana wajib diisi.";
            if (string.IsNullOrWhiteSpace(reason)) return "Alasan rencana wajib diisi.";
            if (end < start) return "Tanggal akhir rencana tidak boleh lebih kecil dari tanggal mulai.";
            var guard = await _periodGuard.CheckRangeAsync(
                start,
                end,
                legalEntityId,
                hospitalSiteId,
                organizationUnitId,
                departmentId,
                cancellationToken);
            if (!guard.IsWritable) return guard.Message;
            if (legalEntityId.HasValue && legalEntityId != Guid.Empty && !await _dbContext.MstLegalEntities.AnyAsync(x => x.Id == legalEntityId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken)) return "Legal entity tidak ditemukan atau tidak aktif.";
            if (hospitalSiteId.HasValue && hospitalSiteId != Guid.Empty && !await _dbContext.MstHospitalSites.AnyAsync(x => x.Id == hospitalSiteId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken)) return "Hospital site tidak ditemukan atau tidak aktif.";
            if (organizationUnitId.HasValue && organizationUnitId != Guid.Empty && !await _dbContext.MstOrganizationUnits.AnyAsync(x => x.Id == organizationUnitId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken)) return "Organization unit tidak ditemukan atau tidak aktif.";
            if (departmentId.HasValue && departmentId != Guid.Empty && !await _dbContext.MstDepartments.AnyAsync(x => x.Id == departmentId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken)) return "Department tidak ditemukan atau tidak aktif.";
            if (costCenterId.HasValue && costCenterId != Guid.Empty && !await _dbContext.MstCostCenters.AnyAsync(x => x.Id == costCenterId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken)) return "Cost center tidak ditemukan atau tidak aktif.";
            if (workLocationId.HasValue && workLocationId != Guid.Empty && !await _dbContext.MstWorkLocations.AnyAsync(x => x.Id == workLocationId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken)) return "Work location tidak ditemukan atau tidak aktif.";
            if (rosterPeriodId.HasValue && rosterPeriodId != Guid.Empty && !await _dbContext.TrxRosterPeriods.AnyAsync(x => x.Id == rosterPeriodId && !x.IsDelete && !x.IsCancel, cancellationToken)) return "Roster period tidak ditemukan.";
            return null;
        }

        private async Task<string> GeneratePlanNumberAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var prefix = $"OTP-PLN-{date:yyyyMMdd}-";
            await AcquireNumberLockAsync("OVERTIME_PLAN_" + date.ToString("yyyyMMdd"), cancellationToken);
            var last = await _dbContext.TrxOvertimePlans.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.PlanNumber.StartsWith(prefix))
                .OrderByDescending(x => x.PlanNumber)
                .Select(x => x.PlanNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = ParseSequence(last, prefix) + 1;
            return prefix + next.ToString("D5");
        }

        private async Task<int> GetNextRequestSequenceAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var prefix = $"OTR-{date:yyyyMMdd}-";
            await AcquireNumberLockAsync("OVERTIME_REQUEST_" + date.ToString("yyyyMMdd"), cancellationToken);
            var last = await _dbContext.WfpOvertimeRequests.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.RequestNumber.StartsWith(prefix))
                .OrderByDescending(x => x.RequestNumber)
                .Select(x => x.RequestNumber)
                .FirstOrDefaultAsync(cancellationToken);
            return ParseSequence(last, prefix) + 1;
        }

        private async Task AcquireNumberLockAsync(string key, CancellationToken cancellationToken) =>
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                new object[] { key },
                cancellationToken);

        private static string BuildRequestNumber(DateOnly date, int sequence) => $"OTR-{date:yyyyMMdd}-{sequence:D5}";
        private static int ParseSequence(string? value, string prefix) => value != null && value.StartsWith(prefix) && int.TryParse(value[prefix.Length..], out var result) ? result : 0;
        private static bool CanEdit(string status) => status == OvertimeValueConstants.PlanStatus.Draft || status == OvertimeValueConstants.PlanStatus.Validated;
        private static bool IsPublishable(TrxOvertimePlanDetail detail) => detail.IsPolicyCompliant && !detail.HasScheduleConflict && !detail.HasLeaveConflict && !detail.HasTrainingConflict && !detail.HasMinimumRestConflict && !detail.HasWorkHourLimitConflict;

        private static bool IsCategoryAllowed(MstOvertimePolicy policy, string category) => category switch
        {
            OvertimeValueConstants.OvertimeCategory.BeforeShift => policy.AllowBeforeShift,
            OvertimeValueConstants.OvertimeCategory.AfterShift => policy.AllowAfterShift,
            OvertimeValueConstants.OvertimeCategory.RestDay => policy.AllowRestDay,
            OvertimeValueConstants.OvertimeCategory.Holiday => policy.AllowHoliday,
            _ => true
        };

        private static bool IsDayTypeAllowed(MstOvertimePolicy policy, string dayType) => dayType switch
        {
            OvertimeValueConstants.DayType.RestDay => policy.AllowRestDay,
            OvertimeValueConstants.DayType.Holiday => policy.AllowHoliday,
            OvertimeValueConstants.DayType.SpecialHoliday => policy.AllowHoliday,
            _ => true
        };

        private static int RoundMinutes(int minutes, int interval, string method)
        {
            if (minutes <= 0 || interval <= 1 || method == OvertimeValueConstants.RoundingMethod.None) return Math.Max(0, minutes);
            var quotient = minutes / (double)interval;
            return method switch
            {
                OvertimeValueConstants.RoundingMethod.Up => (int)Math.Ceiling(quotient) * interval,
                OvertimeValueConstants.RoundingMethod.Nearest => (int)Math.Round(quotient, MidpointRounding.AwayFromZero) * interval,
                _ => (int)Math.Floor(quotient) * interval
            };
        }

        private static DateOnly GetWeekStart(DateOnly date)
        {
            var offset = ((int)date.DayOfWeek + 6) % 7;
            return date.AddDays(-offset);
        }

        private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        private static Guid? NormalizeGuid(Guid? value) => value.HasValue && value.Value != Guid.Empty ? value : null;
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string AppendNote(string? existing, string note, int maxLength)
        {
            var combined = string.IsNullOrWhiteSpace(existing)
                ? note.Trim()
                : existing.Trim() + Environment.NewLine + note.Trim();
            return combined.Length <= maxLength ? combined : combined[..maxLength];
        }
        private static string? NormalizeToken(string? value, IReadOnlyCollection<string> allowed) => string.IsNullOrWhiteSpace(value) ? null : allowed.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        private static OvertimeValidationIssueResponse Issue(string code, string severity, string message, bool blocking, string? field = null, Guid? referenceId = null) => new() { Code = code, Severity = severity, Message = message, IsBlocking = blocking, Field = field, ReferenceId = referenceId };
        private static string BuildIssueMessage(OvertimePlanDetailValidationResponse response) => string.Join(" ", response.Issues.Where(x => x.IsBlocking).Select(x => x.Message).Distinct());

        private static string SerializeValidation(OvertimePlanDetailValidationResponse response) =>
            JsonSerializer.Serialize(new
            {
                response.DetailId,
                response.WorkforceProfileId,
                response.WorkforceProfileCode,
                response.WorkforceDisplayName,
                OvertimeDate = response.OvertimeDate.ToString("yyyy-MM-dd"),
                response.PlannedStartAt,
                response.PlannedEndAt,
                response.RawPlannedMinutes,
                response.EstimatedBreakMinutes,
                response.EligiblePlannedMinutes,
                response.RoundedPlannedMinutes,
                response.OrganizationAssignmentId,
                response.WorkScheduleAssignmentId,
                response.ShiftAssignmentId,
                response.OvertimePolicyId,
                response.OvertimePolicyCode,
                response.OvertimePolicyName,
                response.PreviewOvertimeRateId,
                response.PreviewOvertimeRateCode,
                response.PreviewRateMultiplier,
                response.HasScheduleConflict,
                response.HasLeaveConflict,
                response.HasTrainingConflict,
                response.HasMinimumRestConflict,
                response.HasWorkHourLimitConflict,
                response.IsPolicyCompliant,
                response.CanPersist,
                response.CanPublish,
                response.EvaluatedChecks,
                response.DeferredChecks,
                response.Issues
            });

        private static void ResetPlanValidation(TrxOvertimePlan plan, Guid actorUserId, DateTime now)
        {
            plan.PlanStatus = OvertimeValueConstants.PlanStatus.Draft;
            plan.ValidatedAt = null;
            plan.ValidatedByUserId = null;
            plan.UpdateDateTime = now;
            plan.UpdateBy = actorUserId;
        }

        private static UpdateOvertimePlanDetailRequest ToUpdateRequest(TrxOvertimePlanDetail detail) => new()
        {
            WorkforceProfileId = detail.WorkforceProfileId,
            EmployeeId = detail.EmployeeId,
            OrganizationAssignmentId = detail.OrganizationAssignmentId,
            WorkScheduleAssignmentId = detail.WorkScheduleAssignmentId,
            RosterPeriodId = detail.RosterPeriodId,
            ShiftAssignmentId = detail.ShiftAssignmentId,
            WorkScheduleId = detail.WorkScheduleId,
            ShiftId = detail.ShiftId,
            OvertimePolicyId = detail.OvertimePolicyId,
            OvertimeDate = detail.OvertimeDate,
            PlannedEndDate = detail.PlannedEndDate,
            PlannedStartAt = detail.PlannedStartAt,
            PlannedEndAt = detail.PlannedEndAt,
            EstimatedBreakMinutes = detail.EstimatedBreakMinutes,
            DayType = detail.DayType,
            OvertimeCategory = detail.OvertimeCategory,
            WorkDescription = detail.WorkDescription,
            Notes = detail.Notes,
            IsActive = detail.IsActive
        };

        private static OvertimePlanningServiceResult<T> Fail<T>(string message) => OvertimePlanningServiceResult<T>.Fail(StatusCodes.Status400BadRequest, message);
        private static OvertimePlanningServiceResult<T> NotFound<T>(string message = "Rencana lembur tidak ditemukan.") => OvertimePlanningServiceResult<T>.Fail(StatusCodes.Status404NotFound, message);
        private static OvertimePlanningServiceResult<T> Conflict<T>(string message) => OvertimePlanningServiceResult<T>.Fail(StatusCodes.Status409Conflict, message);

        private sealed class DetailSnapshot
        {
            public Guid? EmployeeId { get; set; }
            public Guid? OrganizationAssignmentId { get; set; }
            public Guid? HospitalSiteId { get; set; }
            public Guid? OrganizationUnitId { get; set; }
            public Guid? DepartmentId { get; set; }
            public Guid? PositionId { get; set; }
            public Guid? CostCenterId { get; set; }
            public Guid? WorkLocationId { get; set; }
            public Guid? WorkScheduleAssignmentId { get; set; }
            public Guid? RosterPeriodId { get; set; }
            public Guid? ShiftAssignmentId { get; set; }
            public Guid? WorkScheduleId { get; set; }
            public Guid? ShiftId { get; set; }
            public Guid? OvertimePolicyId { get; set; }
            public int PlannedMinutes { get; set; }
            public int EstimatedBreakMinutes { get; set; }
            public string DayType { get; set; } = OvertimeValueConstants.DayType.Workday;
            public string OvertimeCategory { get; set; } = OvertimeValueConstants.OvertimeCategory.AfterShift;
        }

        private sealed record DetailEvaluation(OvertimePlanDetailValidationResponse Response, DetailSnapshot Snapshot);
    }
}
