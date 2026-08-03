using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveRequestAttachmentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly WorkflowFileStorageService _fileStorageService;
        private readonly LeaveRequestCalculationService _calculationService;

        public LeaveRequestAttachmentService(
            ApplicationDbContext dbContext,
            WorkflowFileStorageService fileStorageService,
            LeaveRequestCalculationService calculationService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
            _calculationService = calculationService;
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestAttachmentResponse>> UploadAsync(
            Guid leaveRequestId,
            Guid actorUserId,
            IFormFile file,
            string? attachmentType,
            bool isRequiredDocument,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
            {
                return LeaveRequestServiceResult<LeaveRequestAttachmentResponse>.Fail(
                    actor.StatusCode,
                    actor.Message);
            }

            var request = await _dbContext.Set<WfpLeaveRequest>()
                .FirstOrDefaultAsync(x =>
                    x.Id == leaveRequestId &&
                    x.WorkforceProfileId == actor.Data.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (request == null)
            {
                return LeaveRequestServiceResult<LeaveRequestAttachmentResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan cuti tidak ditemukan.");
            }

            if (!CanEdit(request.LeaveRequestStatus))
            {
                return LeaveRequestServiceResult<LeaveRequestAttachmentResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attachment hanya dapat diubah pada status Draft atau NeedRevision.");
            }

            var storageReferenceId = request.WorkflowInstanceId ?? request.Id;
            var stored = await _fileStorageService.SaveAsync(
                storageReferenceId,
                file,
                cancellationToken);

            if (!stored.Success || stored.Data == null)
            {
                return LeaveRequestServiceResult<LeaveRequestAttachmentResponse>.Fail(
                    stored.StatusCode,
                    stored.Message);
            }

            var entity = new TrxLeaveRequestAttachment
            {
                Id = Guid.NewGuid(),
                LeaveRequestId = request.Id,
                AttachmentType = NormalizeAttachmentType(attachmentType),
                OriginalFileName = Path.GetFileName(file.FileName),
                FilePath = stored.Data.RelativePath,
                ContentType = stored.Data.ContentType,
                FileSizeBytes = stored.Data.FileSizeBytes,
                FileHash = stored.Data.Checksum,
                IsRequiredDocument = isRequiredDocument,
                VerificationStatus = LeaveRequestValueConstants.AttachmentVerificationStatus.Pending,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            try
            {
                _dbContext.Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await _fileStorageService.DeletePhysicalFileAsync(
                    stored.Data.RelativePath,
                    cancellationToken);
                throw;
            }

            return LeaveRequestServiceResult<LeaveRequestAttachmentResponse>.Ok(
                Map(entity),
                "Attachment pengajuan cuti berhasil diunggah.",
                StatusCodes.Status201Created);
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestFileDownloadResponse>> GetDownloadAsync(
            Guid leaveRequestId,
            Guid attachmentId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
            {
                return LeaveRequestServiceResult<LeaveRequestFileDownloadResponse>.Fail(
                    actor.StatusCode,
                    actor.Message);
            }

            var attachment = await _dbContext.Set<TrxLeaveRequestAttachment>()
                .AsNoTracking()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x =>
                    x.Id == attachmentId &&
                    x.LeaveRequestId == leaveRequestId &&
                    x.LeaveRequest != null &&
                    x.LeaveRequest.WorkforceProfileId == actor.Data.WorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (attachment == null)
            {
                return LeaveRequestServiceResult<LeaveRequestFileDownloadResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attachment tidak ditemukan.");
            }

            var path = _fileStorageService.ResolveDownloadPath(attachment.FilePath);
            if (!path.Success || string.IsNullOrWhiteSpace(path.Data))
            {
                return LeaveRequestServiceResult<LeaveRequestFileDownloadResponse>.Fail(
                    path.StatusCode,
                    path.Message);
            }

            return LeaveRequestServiceResult<LeaveRequestFileDownloadResponse>.Ok(
                new LeaveRequestFileDownloadResponse
                {
                    PhysicalPath = path.Data,
                    DownloadFileName = attachment.OriginalFileName,
                    ContentType = attachment.ContentType ?? "application/octet-stream"
                },
                "Attachment siap diunduh.");
        }

        public async Task<LeaveRequestServiceResult<bool>> DeleteAsync(
            Guid leaveRequestId,
            Guid attachmentId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var actor = await _calculationService.GetActorContextAsync(actorUserId, cancellationToken);
            if (!actor.Success || actor.Data == null)
            {
                return LeaveRequestServiceResult<bool>.Fail(actor.StatusCode, actor.Message);
            }

            var attachment = await _dbContext.Set<TrxLeaveRequestAttachment>()
                .Include(x => x.LeaveRequest)
                .FirstOrDefaultAsync(x =>
                    x.Id == attachmentId &&
                    x.LeaveRequestId == leaveRequestId &&
                    x.LeaveRequest != null &&
                    x.LeaveRequest.WorkforceProfileId == actor.Data.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (attachment == null || attachment.LeaveRequest == null)
            {
                return LeaveRequestServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attachment tidak ditemukan.");
            }

            if (!CanEdit(attachment.LeaveRequest.LeaveRequestStatus))
            {
                return LeaveRequestServiceResult<bool>.Fail(
                    StatusCodes.Status409Conflict,
                    "Attachment hanya dapat dihapus pada status Draft atau NeedRevision.");
            }

            attachment.IsActive = false;
            attachment.IsDelete = true;
            attachment.DeleteDateTime = DateTime.UtcNow;
            attachment.DeleteBy = actorUserId;
            attachment.UpdateDateTime = DateTime.UtcNow;
            attachment.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _fileStorageService.DeletePhysicalFileAsync(
                attachment.FilePath,
                cancellationToken);

            return LeaveRequestServiceResult<bool>.Ok(
                true,
                "Attachment berhasil dihapus.");
        }

        private static bool CanEdit(string status) =>
            status == LeaveRequestValueConstants.Status.Draft ||
            status == LeaveRequestValueConstants.Status.NeedRevision;

        private static string NormalizeAttachmentType(string? value) => value switch
        {
            LeaveRequestValueConstants.AttachmentType.MedicalCertificate => value,
            LeaveRequestValueConstants.AttachmentType.HandoverDocument => value,
            LeaveRequestValueConstants.AttachmentType.Other => value,
            _ => LeaveRequestValueConstants.AttachmentType.SupportingDocument
        };

        internal static LeaveRequestAttachmentResponse Map(TrxLeaveRequestAttachment x) =>
            new()
            {
                Id = x.Id,
                AttachmentType = x.AttachmentType,
                OriginalFileName = x.OriginalFileName,
                ContentType = x.ContentType,
                FileSizeBytes = x.FileSizeBytes,
                VerificationStatus = x.VerificationStatus,
                IsRequiredDocument = x.IsRequiredDocument,
                CreateDateTime = x.CreateDateTime
            };
    }
}
