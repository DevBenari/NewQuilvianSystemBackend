using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IWorkflowCommentAndAttachmentService
    {
        Task<List<WorkflowCommentV2ResponseDto>> GetCommentsAsync(Guid workflowInstanceId);
        Task<WorkflowCommentV2ResponseDto> AddCommentAsync(Guid workflowInstanceId, AddWorkflowCommentRequestDto request, Guid currentEmployeeId);
        Task<List<WorkflowAttachmentV2ResponseDto>> GetAttachmentsAsync(Guid workflowInstanceId);
        Task<WorkflowAttachmentV2ResponseDto> UploadAttachmentAsync(Guid workflowInstanceId, IFormFile file, string? description, Guid currentEmployeeId);
    }

    public class WorkflowCommentAndAttachmentService : IWorkflowCommentAndAttachmentService
    {
        private readonly ApplicationDbContext _context;

        public WorkflowCommentAndAttachmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkflowCommentV2ResponseDto>> GetCommentsAsync(Guid workflowInstanceId)
        {
            return await _context.TrxWorkflowComments
                .Where(c => c.WorkflowInstanceId == workflowInstanceId)
                .OrderBy(c => c.CommentedAt)
                .Select(c => new WorkflowCommentV2ResponseDto
                {
                    Id = c.Id,
                    WorkflowInstanceId = c.WorkflowInstanceId,
                    AuthorEmployeeId = c.CommentByUserId ?? Guid.Empty,
                    AuthorName = "Pengguna Sistem",
                    CommentText = c.CommentText,
                    IsInternalReviewOnly = c.IsInternalComment,
                    CreatedAt = c.CommentedAt
                })
                .ToListAsync();
        }

        public async Task<WorkflowCommentV2ResponseDto> AddCommentAsync(Guid workflowInstanceId, AddWorkflowCommentRequestDto request, Guid currentEmployeeId)
        {
            var instance = await _context.TrxWorkflowInstances.FindAsync(workflowInstanceId);
            if (instance == null) throw new KeyNotFoundException("Tiket workflow tidak ditemukan");

            var comment = new TrxWorkflowComment
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = workflowInstanceId,
                CommentByUserId = currentEmployeeId,
                CommentText = request.CommentText,
                IsInternalComment = request.IsInternalOnly,
                CommentedAt = DateTime.UtcNow
            };

            _context.TrxWorkflowComments.Add(comment);
            await _context.SaveChangesAsync();

            return new WorkflowCommentV2ResponseDto
            {
                Id = comment.Id,
                WorkflowInstanceId = workflowInstanceId,
                AuthorEmployeeId = currentEmployeeId,
                AuthorName = "Pengguna Sistem",
                CommentText = comment.CommentText,
                IsInternalReviewOnly = comment.IsInternalComment,
                CreatedAt = comment.CommentedAt
            };
        }

        public async Task<List<WorkflowAttachmentV2ResponseDto>> GetAttachmentsAsync(Guid workflowInstanceId)
        {
            return await _context.TrxWorkflowAttachments
                .Where(a => a.WorkflowInstanceId == workflowInstanceId)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new WorkflowAttachmentV2ResponseDto
                {
                    Id = a.Id,
                    WorkflowInstanceId = a.WorkflowInstanceId,
                    FileName = a.FileName,
                    FileStoragePath = a.FilePath,
                    FileSizeBytes = a.FileSizeBytes,
                    FileExtension = Path.GetExtension(a.FileName),
                    Description = a.Description,
                    UploadedAt = a.UploadedAt,
                    UploadedByName = "Pengunggah"
                })
                .ToListAsync();
        }

        public async Task<WorkflowAttachmentV2ResponseDto> UploadAttachmentAsync(Guid workflowInstanceId, IFormFile file, string? description, Guid currentEmployeeId)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("Berkas lampiran tidak boleh kosong");
            }

            var instance = await _context.TrxWorkflowInstances.FindAsync(workflowInstanceId);
            if (instance == null) throw new KeyNotFoundException("Tiket workflow tidak ditemukan");

            var ext = Path.GetExtension(file.FileName);
            var safeName = $"WF-ATT-{Guid.NewGuid()}{ext}";
            var storagePath = $"/uploads/workflow/{safeName}";

            var att = new TrxWorkflowAttachment
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = workflowInstanceId,
                FileName = file.FileName,
                FilePath = storagePath,
                FileSizeBytes = file.Length,
                ContentType = file.ContentType,
                Description = description,
                UploadedByUserId = currentEmployeeId,
                UploadedAt = DateTime.UtcNow
            };

            _context.TrxWorkflowAttachments.Add(att);
            await _context.SaveChangesAsync();

            return new WorkflowAttachmentV2ResponseDto
            {
                Id = att.Id,
                WorkflowInstanceId = workflowInstanceId,
                FileName = att.FileName,
                FileStoragePath = att.FilePath,
                FileSizeBytes = att.FileSizeBytes,
                FileExtension = Path.GetExtension(att.FileName),
                Description = att.Description,
                UploadedAt = att.UploadedAt,
                UploadedByName = "Pengunggah"
            };
        }
    }
}
