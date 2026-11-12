using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IWorkflowResubmitService
    {
        Task<WorkflowInstanceV2ResponseDto> ResubmitAsync(Guid workflowInstanceId, ResubmitWorkflowRequestDto request, Guid currentEmployeeId);
        Task<int> CancelExpiredReturnedRequestsAsync();
    }

    public class WorkflowResubmitService : IWorkflowResubmitService
    {
        private readonly ApplicationDbContext _context;

        public WorkflowResubmitService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WorkflowInstanceV2ResponseDto> ResubmitAsync(Guid workflowInstanceId, ResubmitWorkflowRequestDto request, Guid currentEmployeeId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var instance = await _context.TrxWorkflowInstances
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.ApproverAssignments)
                .FirstOrDefaultAsync(w => w.Id == workflowInstanceId);

            if (instance == null) throw new KeyNotFoundException("Tiket alur kerja tidak ditemukan");

            if (instance.WorkflowStatus != "ReturnedForRevision")
            {
                throw new InvalidOperationException("Hanya tiket berstatus 'ReturnedForRevision' yang dapat diajukan ulang (VR-WF-06)");
            }

            // Validasi batas waktu 7 hari kalender (VR-WF-06)
            var returnHistory = await _context.TrxWorkflowStatusHistories
                .Where(h => h.WorkflowInstanceId == workflowInstanceId && h.ToWorkflowStatus == "ReturnedForRevision")
                .OrderByDescending(h => h.ChangedAt)
                .FirstOrDefaultAsync();

            if (returnHistory != null && (DateTime.UtcNow - returnHistory.ChangedAt).TotalDays > 7.0)
            {
                instance.WorkflowStatus = "Cancelled";
                instance.CancelledAt = DateTime.UtcNow;
                instance.CancellationReason = "Batas waktu pengajuan perbaikan 7 hari kalender telah kedaluwarsa (VR-WF-06)";
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Batas waktu 7 hari kalender untuk memperbaiki draf telah terlewati. Tiket otomatis dibatalkan sistem.");
            }

            var now = DateTime.UtcNow;

            // Perbarui data draf
            if (!string.IsNullOrWhiteSpace(request.UpdatedDataJson))
            {
                instance.RequestContextJson = request.UpdatedDataJson;
            }

            instance.WorkflowStatus = "Pending";
            instance.SlaDeadline = now.AddHours(48);
            instance.IsOverdue = false;

            // Aktifkan kembali tahapan review yang mengembalikan
            var returnedStep = instance.StepInstances.FirstOrDefault(s => s.StepStatus == "ReturnedForRevision");
            if (returnedStep != null)
            {
                returnedStep.StepStatus = "Pending";
                returnedStep.IsCurrentStep = true;
                returnedStep.DueAt = instance.SlaDeadline;

                var assignment = returnedStep.ApproverAssignments.FirstOrDefault(a => a.AssignmentStatus == "Pending" || a.AssignmentStatus == "ReturnedForRevision");
                if (assignment != null)
                {
                    assignment.AssignmentStatus = "Pending";
                    assignment.DueAt = instance.SlaDeadline;
                }
            }

            // Catat ke komentar thread
            _context.TrxWorkflowComments.Add(new TrxWorkflowComment
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                CommentByUserId = currentEmployeeId,
                CommentText = $"[PENGAJUAN ULANG PERBAIKAN]: {request.ResubmitComment}",
                IsInternalComment = false,
                CommentedAt = now
            });

            _context.TrxWorkflowStatusHistories.Add(new TrxWorkflowStatusHistory
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                FromWorkflowStatus = "ReturnedForRevision",
                ToWorkflowStatus = "Pending",
                ChangedAt = now,
                Comment = $"Pengajuan perbaikan draf: {request.ResubmitComment}"
            });

            await _context.SaveChangesAsync();

            return new WorkflowInstanceV2ResponseDto
            {
                Id = instance.Id,
                WorkflowNumber = instance.RequestNumber,
                RequestType = instance.ReferenceType,
                ReferenceType = instance.ReferenceType,
                ReferenceId = instance.ReferenceId,
                WorkflowStatus = instance.WorkflowStatus,
                PriorityLevel = instance.PriorityLevel,
                SlaDeadline = instance.SlaDeadline,
                IsOverdue = false
            };
        }

        public async Task<int> CancelExpiredReturnedRequestsAsync()
        {
            // Worker WF-W6: Pembatalan otomatis tiket yang diabaikan > 7 hari
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var expiredInstances = await _context.TrxWorkflowInstances
                .Where(w => w.WorkflowStatus == "ReturnedForRevision" &&
                            w.LastActionAt.HasValue &&
                            w.LastActionAt.Value < sevenDaysAgo)
                .ToListAsync();

            int count = 0;
            foreach (var inst in expiredInstances)
            {
                inst.WorkflowStatus = "Cancelled";
                inst.CancelledAt = DateTime.UtcNow;
                inst.CancellationReason = "Pembatalan otomatis oleh sistem: Staf tidak mengajukan perbaikan dalam batas 7 hari (WF-W6 / VR-WF-06)";

                _context.TrxWorkflowStatusHistories.Add(new TrxWorkflowStatusHistory
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = inst.Id,
                    FromWorkflowStatus = "ReturnedForRevision",
                    ToWorkflowStatus = "Cancelled",
                    ChangedAt = DateTime.UtcNow,
                    Comment = inst.CancellationReason
                });

                count++;
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return count;
        }
    }
}
