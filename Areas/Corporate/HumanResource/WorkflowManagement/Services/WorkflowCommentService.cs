using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public class WorkflowCommentService
    {
        private static readonly HashSet<string> AllowedCommentTypes = new(
            new[]
            {
                WorkflowValueConstants.CommentType.General,
                WorkflowValueConstants.CommentType.Requester,
                WorkflowValueConstants.CommentType.Approver,
                WorkflowValueConstants.CommentType.Internal,
                WorkflowValueConstants.CommentType.Revision,
                WorkflowValueConstants.CommentType.Rejection,
                WorkflowValueConstants.CommentType.System
            },
            StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> TerminalWorkflowStatuses = new(
            new[]
            {
                WorkflowValueConstants.WorkflowStatus.Completed,
                WorkflowValueConstants.WorkflowStatus.Rejected,
                WorkflowValueConstants.WorkflowStatus.Cancelled,
                WorkflowValueConstants.WorkflowStatus.Withdrawn
            },
            StringComparer.OrdinalIgnoreCase);

        private readonly ApplicationDbContext _dbContext;
        private readonly HumanResourceContextService _humanResourceContextService;
        private readonly WorkflowFileStorageService _fileStorageService;

        public WorkflowCommentService(
            ApplicationDbContext dbContext,
            HumanResourceContextService humanResourceContextService,
            WorkflowFileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _humanResourceContextService = humanResourceContextService;
            _fileStorageService = fileStorageService;
        }

        public WorkflowCommentFilterMetadataResponse GetFilterMetadata()
        {
            return new WorkflowCommentFilterMetadataResponse
            {
                CommentTypeOptions = new List<WorkflowStringOptionResponse>
                {
                    new() { Value = WorkflowValueConstants.CommentType.General, Label = "Umum" },
                    new() { Value = WorkflowValueConstants.CommentType.Requester, Label = "Pemohon" },
                    new() { Value = WorkflowValueConstants.CommentType.Approver, Label = "Approver" },
                    new() { Value = WorkflowValueConstants.CommentType.Internal, Label = "Internal" },
                    new() { Value = WorkflowValueConstants.CommentType.Revision, Label = "Revisi" },
                    new() { Value = WorkflowValueConstants.CommentType.Rejection, Label = "Penolakan" },
                    new() { Value = WorkflowValueConstants.CommentType.System, Label = "Sistem" }
                },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<WorkflowServiceResult<PagedResult<WorkflowCommentListResponse>>> GetCommentsAsync(
            Guid workflowInstanceId,
            Guid? workflowStepInstanceId,
            Guid? parentCommentId,
            string? commentType,
            bool? isInternalComment,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty)
            {
                return WorkflowServiceResult<PagedResult<WorkflowCommentListResponse>>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<PagedResult<WorkflowCommentListResponse>>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var instance = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x => x.Id == workflowInstanceId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.RequestedByUserId,
                    x.WorkflowStatus
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (instance == null)
            {
                return WorkflowServiceResult<PagedResult<WorkflowCommentListResponse>>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow instance tidak ditemukan.");
            }

            var actor = actorResult.Data!;
            var isRequester = instance.RequestedByUserId == actor.UserId;
            var workflowIsTerminal = TerminalWorkflowStatuses.Contains(
                instance.WorkflowStatus);
            var paging = NormalizePaging(pageNumber, pageSize);

            var query = BuildCommentQuery()
                .Where(x => x.WorkflowInstanceId == workflowInstanceId);

            if (isRequester)
            {
                query = query.Where(x => x.IsRequesterVisible && !x.IsInternalComment);
            }

            if (workflowStepInstanceId.HasValue && workflowStepInstanceId.Value != Guid.Empty)
            {
                query = query.Where(x =>
                    x.WorkflowStepInstanceId == workflowStepInstanceId.Value);
            }

            if (parentCommentId.HasValue && parentCommentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ParentCommentId == parentCommentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(commentType))
            {
                var normalizedType = commentType.Trim();
                query = query.Where(x => x.CommentType == normalizedType);
            }

            if (isInternalComment.HasValue && !isRequester)
            {
                query = query.Where(x =>
                    x.IsInternalComment == isInternalComment.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.CommentText.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(x => x.CommentedAt)
                .ThenBy(x => x.Id)
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .Select(x => new WorkflowCommentListResponse
                {
                    Id = x.Id,
                    WorkflowInstanceId = x.WorkflowInstanceId,
                    WorkflowStepInstanceId = x.WorkflowStepInstanceId,
                    WorkflowStepCode = x.WorkflowStepInstance != null
                        ? x.WorkflowStepInstance.StepCodeSnapshot
                        : null,
                    WorkflowStepName = x.WorkflowStepInstance != null
                        ? x.WorkflowStepInstance.StepNameSnapshot
                        : null,
                    ParentCommentId = x.ParentCommentId,
                    CommentType = x.CommentType,
                    CommentText = x.CommentText,
                    CommentedAt = x.CommentedAt,
                    CommentByUserId = x.CommentByUserId,
                    CommentByWorkforceProfileId = x.CommentByWorkforceProfileId,
                    CommentByName = x.CommentByWorkforceProfile != null
                        ? x.CommentByWorkforceProfile.DisplayName
                        : x.CommentByUser != null
                            ? x.CommentByUser.DisplayName ??
                              x.CommentByUser.UserName ??
                              x.CommentByUser.Email ??
                              x.CommentByUser.UserCode
                            : null,
                    IsRequesterVisible = x.IsRequesterVisible,
                    IsInternalComment = x.IsInternalComment,
                    IsSystemGenerated = x.IsSystemGenerated,
                    ReplyCount = x.Replies.Count(reply => !reply.IsDelete && reply.IsActive),
                    AttachmentCount = x.Attachments.Count(attachment =>
                        !attachment.IsDelete && attachment.IsActive),
                    CanEdit = !x.IsSystemGenerated &&
                              x.CommentByUserId == actor.UserId &&
                              !workflowIsTerminal,
                    CanDelete = !x.IsSystemGenerated &&
                                x.CommentByUserId == actor.UserId &&
                                !workflowIsTerminal &&
                                !x.Replies.Any(reply => !reply.IsDelete && reply.IsActive)
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WorkflowCommentListResponse>
            {
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)paging.PageSize),
                Items = items
            };

            return WorkflowServiceResult<PagedResult<WorkflowCommentListResponse>>.Ok(
                result,
                "Komentar workflow berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<WorkflowCommentListResponse>> CreateCommentAsync(
            Guid workflowInstanceId,
            CreateWorkflowCommentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id tidak valid.");
            }

            if (request == null || string.IsNullOrWhiteSpace(request.CommentText))
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Isi komentar wajib diisi.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var instance = await _dbContext.Set<TrxWorkflowInstance>()
                .FirstOrDefaultAsync(
                    x => x.Id == workflowInstanceId && !x.IsDelete,
                    cancellationToken);

            if (instance == null)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow instance tidak ditemukan.");
            }

            if (TerminalWorkflowStatuses.Contains(instance.WorkflowStatus))
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Komentar baru tidak dapat ditambahkan karena workflow sudah selesai atau ditutup.");
            }

            var relationValidation = await ValidateCommentRelationsAsync(
                workflowInstanceId,
                request.WorkflowStepInstanceId,
                request.ParentCommentId,
                cancellationToken);

            if (relationValidation != null)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    relationValidation);
            }

            var actor = actorResult.Data!;
            var isRequester = instance.RequestedByUserId == actor.UserId;
            var commentType = NormalizeCommentType(request.CommentType, isRequester);

            if (commentType == null)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CommentType tidak dikenali oleh Workflow Engine.");
            }

            var now = DateTime.UtcNow;
            var entity = new TrxWorkflowComment
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = workflowInstanceId,
                WorkflowStepInstanceId = NormalizeGuid(request.WorkflowStepInstanceId),
                ParentCommentId = NormalizeGuid(request.ParentCommentId),
                CommentByUserId = actor.UserId,
                CommentByWorkforceProfileId = actor.WorkforceProfileId,
                CommentType = commentType,
                CommentText = request.CommentText.Trim(),
                CommentedAt = now,
                IsRequesterVisible = isRequester || request.IsRequesterVisible,
                IsInternalComment = !isRequester && request.IsInternalComment,
                IsSystemGenerated = false,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor.UserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxWorkflowComment>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = await GetCommentByIdAsync(
                workflowInstanceId,
                entity.Id,
                actor,
                instance.WorkflowStatus,
                cancellationToken);

            return WorkflowServiceResult<WorkflowCommentListResponse>.Ok(
                result!,
                "Komentar workflow berhasil ditambahkan.",
                StatusCodes.Status201Created);
        }

        public async Task<WorkflowServiceResult<WorkflowCommentListResponse>> UpdateCommentAsync(
            Guid workflowInstanceId,
            Guid commentId,
            UpdateWorkflowCommentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || commentId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id atau comment id tidak valid.");
            }

            if (request == null || string.IsNullOrWhiteSpace(request.CommentText))
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Isi komentar wajib diisi.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var entity = await _dbContext.Set<TrxWorkflowComment>()
                .Include(x => x.WorkflowInstance)
                .FirstOrDefaultAsync(
                    x => x.Id == commentId &&
                         x.WorkflowInstanceId == workflowInstanceId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null || entity.WorkflowInstance == null)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Komentar workflow tidak ditemukan.");
            }

            if (entity.IsSystemGenerated)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Komentar yang dibuat sistem tidak dapat diubah.");
            }

            if (entity.CommentByUserId != actor.UserId)
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Komentar hanya dapat diubah oleh pembuatnya.");
            }

            if (TerminalWorkflowStatuses.Contains(entity.WorkflowInstance.WorkflowStatus))
            {
                return WorkflowServiceResult<WorkflowCommentListResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Komentar tidak dapat diubah karena workflow sudah selesai atau ditutup.");
            }

            var isRequester = entity.WorkflowInstance.RequestedByUserId == actor.UserId;
            var now = DateTime.UtcNow;

            entity.CommentText = request.CommentText.Trim();
            entity.IsRequesterVisible = isRequester || request.IsRequesterVisible;
            entity.IsInternalComment = !isRequester && request.IsInternalComment;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor.UserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = await GetCommentByIdAsync(
                workflowInstanceId,
                entity.Id,
                actor,
                entity.WorkflowInstance.WorkflowStatus,
                cancellationToken);

            return WorkflowServiceResult<WorkflowCommentListResponse>.Ok(
                result!,
                "Komentar workflow berhasil diperbarui.");
        }

        public async Task<WorkflowServiceResult<object>> DeleteCommentAsync(
            Guid workflowInstanceId,
            Guid commentId,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || commentId == Guid.Empty)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id atau comment id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<object>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var entity = await _dbContext.Set<TrxWorkflowComment>()
                .Include(x => x.WorkflowInstance)
                .Include(x => x.Replies.Where(reply => !reply.IsDelete && reply.IsActive))
                .Include(x => x.Attachments.Where(attachment =>
                    !attachment.IsDelete && attachment.IsActive))
                .FirstOrDefaultAsync(
                    x => x.Id == commentId &&
                         x.WorkflowInstanceId == workflowInstanceId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null || entity.WorkflowInstance == null)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Komentar workflow tidak ditemukan.");
            }

            if (entity.IsSystemGenerated)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Komentar yang dibuat sistem tidak dapat dihapus.");
            }

            if (entity.CommentByUserId != actor.UserId)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Komentar hanya dapat dihapus oleh pembuatnya.");
            }

            if (TerminalWorkflowStatuses.Contains(entity.WorkflowInstance.WorkflowStatus))
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Komentar tidak dapat dihapus karena workflow sudah selesai atau ditutup.");
            }

            if (entity.Replies.Any())
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Komentar tidak dapat dihapus karena sudah memiliki balasan aktif.");
            }

            var attachmentPaths = entity.Attachments
                .Select(x => x.FilePath)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor.UserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor.UserId;

            foreach (var attachment in entity.Attachments)
            {
                attachment.IsDelete = true;
                attachment.IsActive = false;
                attachment.DeleteDateTime = now;
                attachment.DeleteBy = actor.UserId;
                attachment.UpdateDateTime = now;
                attachment.UpdateBy = actor.UserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var attachmentPath in attachmentPaths)
            {
                await _fileStorageService.DeletePhysicalFileAsync(
                    attachmentPath,
                    cancellationToken);
            }

            return WorkflowServiceResult<object>.Ok(
                new { entity.Id },
                "Komentar workflow berhasil dihapus.");
        }

        private IQueryable<TrxWorkflowComment> BuildCommentQuery()
        {
            return _dbContext.Set<TrxWorkflowComment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);
        }

        private async Task<WorkflowCommentListResponse?> GetCommentByIdAsync(
            Guid workflowInstanceId,
            Guid commentId,
            HumanResourceUserContextDto actor,
            string workflowStatus,
            CancellationToken cancellationToken)
        {
            var workflowIsTerminal = TerminalWorkflowStatuses.Contains(
                workflowStatus);

            return await BuildCommentQuery()
                .Where(x =>
                    x.Id == commentId &&
                    x.WorkflowInstanceId == workflowInstanceId)
                .Select(x => new WorkflowCommentListResponse
                {
                    Id = x.Id,
                    WorkflowInstanceId = x.WorkflowInstanceId,
                    WorkflowStepInstanceId = x.WorkflowStepInstanceId,
                    WorkflowStepCode = x.WorkflowStepInstance != null
                        ? x.WorkflowStepInstance.StepCodeSnapshot
                        : null,
                    WorkflowStepName = x.WorkflowStepInstance != null
                        ? x.WorkflowStepInstance.StepNameSnapshot
                        : null,
                    ParentCommentId = x.ParentCommentId,
                    CommentType = x.CommentType,
                    CommentText = x.CommentText,
                    CommentedAt = x.CommentedAt,
                    CommentByUserId = x.CommentByUserId,
                    CommentByWorkforceProfileId = x.CommentByWorkforceProfileId,
                    CommentByName = x.CommentByWorkforceProfile != null
                        ? x.CommentByWorkforceProfile.DisplayName
                        : x.CommentByUser != null
                            ? x.CommentByUser.DisplayName ??
                              x.CommentByUser.UserName ??
                              x.CommentByUser.Email ??
                              x.CommentByUser.UserCode
                            : null,
                    IsRequesterVisible = x.IsRequesterVisible,
                    IsInternalComment = x.IsInternalComment,
                    IsSystemGenerated = x.IsSystemGenerated,
                    ReplyCount = x.Replies.Count(reply => !reply.IsDelete && reply.IsActive),
                    AttachmentCount = x.Attachments.Count(attachment =>
                        !attachment.IsDelete && attachment.IsActive),
                    CanEdit = !x.IsSystemGenerated &&
                              x.CommentByUserId == actor.UserId &&
                              !workflowIsTerminal,
                    CanDelete = !x.IsSystemGenerated &&
                                x.CommentByUserId == actor.UserId &&
                                !workflowIsTerminal &&
                                !x.Replies.Any(reply => !reply.IsDelete && reply.IsActive)
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<string?> ValidateCommentRelationsAsync(
            Guid workflowInstanceId,
            Guid? workflowStepInstanceId,
            Guid? parentCommentId,
            CancellationToken cancellationToken)
        {
            if (workflowStepInstanceId.HasValue &&
                workflowStepInstanceId.Value != Guid.Empty)
            {
                var stepExists = await _dbContext.Set<TrxWorkflowStepInstance>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == workflowStepInstanceId.Value &&
                             x.WorkflowInstanceId == workflowInstanceId &&
                             !x.IsDelete,
                        cancellationToken);

                if (!stepExists)
                {
                    return "Workflow step instance tidak ditemukan pada workflow tersebut.";
                }
            }

            if (parentCommentId.HasValue && parentCommentId.Value != Guid.Empty)
            {
                var parentExists = await _dbContext.Set<TrxWorkflowComment>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == parentCommentId.Value &&
                             x.WorkflowInstanceId == workflowInstanceId &&
                             !x.IsDelete &&
                             x.IsActive,
                        cancellationToken);

                if (!parentExists)
                {
                    return "Parent comment tidak ditemukan pada workflow tersebut.";
                }
            }

            return null;
        }

        private async Task<WorkflowServiceResult<HumanResourceUserContextDto>> GetActorContextAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var actor = await _humanResourceContextService.GetCurrentAsync(
                    cancellationToken);

                return WorkflowServiceResult<HumanResourceUserContextDto>.Ok(
                    actor,
                    "Konteks user berhasil diambil.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<HumanResourceUserContextDto>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }
        }

        private static string? NormalizeCommentType(
            string? commentType,
            bool isRequester)
        {
            if (isRequester)
            {
                return WorkflowValueConstants.CommentType.Requester;
            }

            var normalized = string.IsNullOrWhiteSpace(commentType)
                ? WorkflowValueConstants.CommentType.General
                : commentType.Trim();

            return AllowedCommentTypes.Contains(normalized)
                ? AllowedCommentTypes.First(x =>
                    string.Equals(x, normalized, StringComparison.OrdinalIgnoreCase))
                : null;
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            return (
                pageNumber < 1 ? 1 : pageNumber,
                pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }
    }
}
