using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public class WorkflowAttachmentService
    {
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

        public WorkflowAttachmentService(
            ApplicationDbContext dbContext,
            HumanResourceContextService humanResourceContextService,
            WorkflowFileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _humanResourceContextService = humanResourceContextService;
            _fileStorageService = fileStorageService;
        }

        public WorkflowAttachmentFilterMetadataResponse GetFilterMetadata()
        {
            return new WorkflowAttachmentFilterMetadataResponse
            {
                MaximumFileSizeBytes = _fileStorageService.MaximumFileSizeBytes,
                AllowedExtensions = _fileStorageService.AllowedExtensions.ToList(),
                AttachmentCategoryOptions = new List<WorkflowStringOptionResponse>
                {
                    new() { Value = "RequestDocument", Label = "Dokumen Pengajuan" },
                    new() { Value = "Evidence", Label = "Bukti Pendukung" },
                    new() { Value = "ApprovalEvidence", Label = "Bukti Persetujuan" },
                    new() { Value = "VerificationDocument", Label = "Dokumen Verifikasi" },
                    new() { Value = "RevisionDocument", Label = "Dokumen Revisi" },
                    new() { Value = "General", Label = "Umum" }
                },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<WorkflowServiceResult<PagedResult<WorkflowAttachmentListResponse>>> GetAttachmentsAsync(
            Guid workflowInstanceId,
            Guid? workflowStepInstanceId,
            Guid? approvalActionId,
            Guid? workflowCommentId,
            string? attachmentCategory,
            bool? isConfidential,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty)
            {
                return WorkflowServiceResult<PagedResult<WorkflowAttachmentListResponse>>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<PagedResult<WorkflowAttachmentListResponse>>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var instance = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x => x.Id == workflowInstanceId && !x.IsDelete)
                .Select(x => new
                {
                    x.RequestedByUserId,
                    x.WorkflowStatus
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (instance == null)
            {
                return WorkflowServiceResult<PagedResult<WorkflowAttachmentListResponse>>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow instance tidak ditemukan.");
            }

            var actor = actorResult.Data!;
            var isRequester = instance.RequestedByUserId == actor.UserId;
            var workflowIsTerminal = TerminalWorkflowStatuses.Contains(
                instance.WorkflowStatus);
            var paging = NormalizePaging(pageNumber, pageSize);

            var query = BuildAttachmentQuery()
                .Where(x => x.WorkflowInstanceId == workflowInstanceId);

            if (isRequester)
            {
                query = query.Where(x => x.IsRequesterVisible && !x.IsConfidential);
            }

            if (workflowStepInstanceId.HasValue &&
                workflowStepInstanceId.Value != Guid.Empty)
            {
                query = query.Where(x =>
                    x.WorkflowStepInstanceId == workflowStepInstanceId.Value);
            }

            if (approvalActionId.HasValue && approvalActionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ApprovalActionId == approvalActionId.Value);
            }

            if (workflowCommentId.HasValue && workflowCommentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkflowCommentId == workflowCommentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(attachmentCategory))
            {
                var normalizedCategory = attachmentCategory.Trim();
                query = query.Where(x =>
                    x.AttachmentCategory == normalizedCategory);
            }

            if (isConfidential.HasValue && !isRequester)
            {
                query = query.Where(x => x.IsConfidential == isConfidential.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.FileName.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword) ||
                    x.AttachmentCategory != null &&
                    x.AttachmentCategory.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.UploadedAt)
                .ThenBy(x => x.FileName)
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .Select(x => new WorkflowAttachmentListResponse
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
                    ApprovalActionId = x.ApprovalActionId,
                    ApprovalActionType = x.ApprovalAction != null
                        ? x.ApprovalAction.ActionType
                        : null,
                    WorkflowCommentId = x.WorkflowCommentId,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    FileSizeBytes = x.FileSizeBytes,
                    FileChecksum = x.FileChecksum,
                    AttachmentCategory = x.AttachmentCategory,
                    Description = x.Description,
                    UploadedAt = x.UploadedAt,
                    UploadedByUserId = x.UploadedByUserId,
                    UploadedByWorkforceProfileId = x.UploadedByWorkforceProfileId,
                    UploadedByName = x.UploadedByWorkforceProfile != null
                        ? x.UploadedByWorkforceProfile.DisplayName
                        : x.UploadedByUser != null
                            ? x.UploadedByUser.DisplayName ??
                              x.UploadedByUser.UserName ??
                              x.UploadedByUser.Email ??
                              x.UploadedByUser.UserCode
                            : null,
                    IsRequesterVisible = x.IsRequesterVisible,
                    IsConfidential = x.IsConfidential,
                    DownloadUrl = BuildDownloadUrl(workflowInstanceId, x.Id),
                    CanDelete = x.UploadedByUserId == actor.UserId &&
                                !workflowIsTerminal
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WorkflowAttachmentListResponse>
            {
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)paging.PageSize),
                Items = items
            };

            return WorkflowServiceResult<PagedResult<WorkflowAttachmentListResponse>>.Ok(
                result,
                "Lampiran workflow berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<WorkflowAttachmentListResponse>> UploadAttachmentAsync(
            Guid workflowInstanceId,
            UploadWorkflowAttachmentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowAttachmentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id tidak valid.");
            }

            if (request == null || request.File == null)
            {
                return WorkflowServiceResult<WorkflowAttachmentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "File wajib dilampirkan.");
            }

            var contextCount = CountAttachmentContexts(request);
            if (contextCount > 1)
            {
                return WorkflowServiceResult<WorkflowAttachmentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Lampiran hanya boleh terhubung ke satu konteks: step, action, atau comment.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<WorkflowAttachmentListResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var instance = await _dbContext.Set<TrxWorkflowInstance>()
                .FirstOrDefaultAsync(
                    x => x.Id == workflowInstanceId && !x.IsDelete,
                    cancellationToken);

            if (instance == null)
            {
                return WorkflowServiceResult<WorkflowAttachmentListResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow instance tidak ditemukan.");
            }

            if (TerminalWorkflowStatuses.Contains(instance.WorkflowStatus))
            {
                return WorkflowServiceResult<WorkflowAttachmentListResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Lampiran baru tidak dapat ditambahkan karena workflow sudah selesai atau ditutup.");
            }

            var actor = actorResult.Data!;
            var isRequester = instance.RequestedByUserId == actor.UserId;

            if (isRequester && request.ApprovalActionId.HasValue &&
                request.ApprovalActionId.Value != Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowAttachmentListResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Pemohon tidak dapat menambahkan lampiran langsung pada approval action.");
            }

            var relationValidation = await ValidateAttachmentRelationsAsync(
                workflowInstanceId,
                request.WorkflowStepInstanceId,
                request.ApprovalActionId,
                request.WorkflowCommentId,
                isRequester,
                cancellationToken);

            if (relationValidation != null)
            {
                return WorkflowServiceResult<WorkflowAttachmentListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    relationValidation);
            }

            var storedFileResult = await _fileStorageService.SaveAsync(
                workflowInstanceId,
                request.File,
                cancellationToken);

            if (!storedFileResult.Success || storedFileResult.Data == null)
            {
                return WorkflowServiceResult<WorkflowAttachmentListResponse>.Fail(
                    storedFileResult.StatusCode,
                    storedFileResult.Message);
            }

            var storedFile = storedFileResult.Data;
            var now = DateTime.UtcNow;
            var entity = new TrxWorkflowAttachment
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = workflowInstanceId,
                WorkflowStepInstanceId = NormalizeGuid(request.WorkflowStepInstanceId),
                ApprovalActionId = NormalizeGuid(request.ApprovalActionId),
                WorkflowCommentId = NormalizeGuid(request.WorkflowCommentId),
                UploadedByUserId = actor.UserId,
                UploadedByWorkforceProfileId = actor.WorkforceProfileId,
                FileName = Path.GetFileName(request.File.FileName).Trim(),
                FilePath = storedFile.RelativePath,
                ContentType = storedFile.ContentType,
                FileSizeBytes = storedFile.FileSizeBytes,
                FileChecksum = storedFile.Checksum,
                AttachmentCategory = NormalizeOptionalText(request.AttachmentCategory),
                Description = NormalizeOptionalText(request.Description),
                UploadedAt = now,
                IsRequesterVisible = isRequester || request.IsRequesterVisible,
                IsConfidential = !isRequester && request.IsConfidential,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor.UserId,
                IsDelete = false,
                IsCancel = false
            };

            try
            {
                _dbContext.Set<TrxWorkflowAttachment>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await _fileStorageService.DeletePhysicalFileAsync(
                    storedFile.RelativePath,
                    cancellationToken);
                throw;
            }

            var result = await GetAttachmentByIdAsync(
                workflowInstanceId,
                entity.Id,
                actor,
                instance.WorkflowStatus,
                cancellationToken);

            return WorkflowServiceResult<WorkflowAttachmentListResponse>.Ok(
                result!,
                "Lampiran workflow berhasil diunggah.",
                StatusCodes.Status201Created);
        }

        public async Task<WorkflowServiceResult<WorkflowAttachmentDownloadResponse>> GetDownloadAsync(
            Guid workflowInstanceId,
            Guid attachmentId,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || attachmentId == Guid.Empty)
            {
                return WorkflowServiceResult<WorkflowAttachmentDownloadResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id atau attachment id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<WorkflowAttachmentDownloadResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var attachment = await _dbContext.Set<TrxWorkflowAttachment>()
                .AsNoTracking()
                .Include(x => x.WorkflowInstance)
                .FirstOrDefaultAsync(
                    x => x.Id == attachmentId &&
                         x.WorkflowInstanceId == workflowInstanceId &&
                         !x.IsDelete &&
                         x.IsActive,
                    cancellationToken);

            if (attachment == null || attachment.WorkflowInstance == null)
            {
                return WorkflowServiceResult<WorkflowAttachmentDownloadResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Lampiran workflow tidak ditemukan.");
            }

            var actor = actorResult.Data!;
            var isRequester = attachment.WorkflowInstance.RequestedByUserId == actor.UserId;
            if (isRequester &&
                (!attachment.IsRequesterVisible || attachment.IsConfidential))
            {
                return WorkflowServiceResult<WorkflowAttachmentDownloadResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Lampiran tersebut tidak tersedia untuk pemohon.");
            }

            var physicalPathResult = _fileStorageService.ResolveDownloadPath(
                attachment.FilePath);

            if (!physicalPathResult.Success ||
                string.IsNullOrWhiteSpace(physicalPathResult.Data))
            {
                return WorkflowServiceResult<WorkflowAttachmentDownloadResponse>.Fail(
                    physicalPathResult.StatusCode,
                    physicalPathResult.Message);
            }

            var result = new WorkflowAttachmentDownloadResponse
            {
                PhysicalPath = physicalPathResult.Data,
                FileName = attachment.FileName,
                ContentType = string.IsNullOrWhiteSpace(attachment.ContentType)
                    ? "application/octet-stream"
                    : attachment.ContentType
            };

            return WorkflowServiceResult<WorkflowAttachmentDownloadResponse>.Ok(
                result,
                "Lampiran workflow siap diunduh.");
        }

        public async Task<WorkflowServiceResult<object>> DeleteAttachmentAsync(
            Guid workflowInstanceId,
            Guid attachmentId,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty || attachmentId == Guid.Empty)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow instance id atau attachment id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<object>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var entity = await _dbContext.Set<TrxWorkflowAttachment>()
                .Include(x => x.WorkflowInstance)
                .FirstOrDefaultAsync(
                    x => x.Id == attachmentId &&
                         x.WorkflowInstanceId == workflowInstanceId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null || entity.WorkflowInstance == null)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Lampiran workflow tidak ditemukan.");
            }

            if (entity.UploadedByUserId != actor.UserId)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Lampiran hanya dapat dihapus oleh pengunggahnya.");
            }

            if (TerminalWorkflowStatuses.Contains(entity.WorkflowInstance.WorkflowStatus))
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Lampiran tidak dapat dihapus karena workflow sudah selesai atau ditutup.");
            }

            var relativePath = entity.FilePath;
            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor.UserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor.UserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _fileStorageService.DeletePhysicalFileAsync(
                relativePath,
                cancellationToken);

            return WorkflowServiceResult<object>.Ok(
                new { entity.Id },
                "Lampiran workflow berhasil dihapus.");
        }

        private IQueryable<TrxWorkflowAttachment> BuildAttachmentQuery()
        {
            return _dbContext.Set<TrxWorkflowAttachment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);
        }

        private async Task<WorkflowAttachmentListResponse?> GetAttachmentByIdAsync(
            Guid workflowInstanceId,
            Guid attachmentId,
            HumanResourceUserContextDto actor,
            string workflowStatus,
            CancellationToken cancellationToken)
        {
            var workflowIsTerminal = TerminalWorkflowStatuses.Contains(
                workflowStatus);

            return await BuildAttachmentQuery()
                .Where(x =>
                    x.Id == attachmentId &&
                    x.WorkflowInstanceId == workflowInstanceId)
                .Select(x => new WorkflowAttachmentListResponse
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
                    ApprovalActionId = x.ApprovalActionId,
                    ApprovalActionType = x.ApprovalAction != null
                        ? x.ApprovalAction.ActionType
                        : null,
                    WorkflowCommentId = x.WorkflowCommentId,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    FileSizeBytes = x.FileSizeBytes,
                    FileChecksum = x.FileChecksum,
                    AttachmentCategory = x.AttachmentCategory,
                    Description = x.Description,
                    UploadedAt = x.UploadedAt,
                    UploadedByUserId = x.UploadedByUserId,
                    UploadedByWorkforceProfileId = x.UploadedByWorkforceProfileId,
                    UploadedByName = x.UploadedByWorkforceProfile != null
                        ? x.UploadedByWorkforceProfile.DisplayName
                        : x.UploadedByUser != null
                            ? x.UploadedByUser.DisplayName ??
                              x.UploadedByUser.UserName ??
                              x.UploadedByUser.Email ??
                              x.UploadedByUser.UserCode
                            : null,
                    IsRequesterVisible = x.IsRequesterVisible,
                    IsConfidential = x.IsConfidential,
                    DownloadUrl = BuildDownloadUrl(workflowInstanceId, x.Id),
                    CanDelete = x.UploadedByUserId == actor.UserId &&
                                !workflowIsTerminal
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<string?> ValidateAttachmentRelationsAsync(
            Guid workflowInstanceId,
            Guid? workflowStepInstanceId,
            Guid? approvalActionId,
            Guid? workflowCommentId,
            bool isRequester,
            CancellationToken cancellationToken)
        {
            if (workflowStepInstanceId.HasValue &&
                workflowStepInstanceId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<TrxWorkflowStepInstance>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == workflowStepInstanceId.Value &&
                             x.WorkflowInstanceId == workflowInstanceId &&
                             !x.IsDelete,
                        cancellationToken);

                if (!exists)
                {
                    return "Workflow step instance tidak ditemukan pada workflow tersebut.";
                }
            }

            if (approvalActionId.HasValue && approvalActionId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<TrxApprovalAction>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == approvalActionId.Value &&
                             x.WorkflowInstanceId == workflowInstanceId &&
                             !x.IsDelete,
                        cancellationToken);

                if (!exists)
                {
                    return "Approval action tidak ditemukan pada workflow tersebut.";
                }
            }

            if (workflowCommentId.HasValue && workflowCommentId.Value != Guid.Empty)
            {
                var comment = await _dbContext.Set<TrxWorkflowComment>()
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == workflowCommentId.Value &&
                        x.WorkflowInstanceId == workflowInstanceId &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => new
                    {
                        x.IsRequesterVisible,
                        x.IsInternalComment
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (comment == null)
                {
                    return "Workflow comment tidak ditemukan pada workflow tersebut.";
                }

                if (isRequester &&
                    (!comment.IsRequesterVisible || comment.IsInternalComment))
                {
                    return "Pemohon tidak dapat menambahkan lampiran pada komentar internal.";
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

        private static int CountAttachmentContexts(
            UploadWorkflowAttachmentRequest request)
        {
            var count = 0;

            if (request.WorkflowStepInstanceId.HasValue &&
                request.WorkflowStepInstanceId.Value != Guid.Empty)
            {
                count++;
            }

            if (request.ApprovalActionId.HasValue &&
                request.ApprovalActionId.Value != Guid.Empty)
            {
                count++;
            }

            if (request.WorkflowCommentId.HasValue &&
                request.WorkflowCommentId.Value != Guid.Empty)
            {
                count++;
            }

            return count;
        }

        private static string BuildDownloadUrl(
            Guid workflowInstanceId,
            Guid attachmentId)
        {
            return $"/api/v1/corporate/human-resource/workflow-instances/" +
                   $"{workflowInstanceId}/attachments/{attachmentId}/download";
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
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
