using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IBulkApprovalService
    {
        Task<BulkApproveResultDto> ExecuteBulkApprovalAsync(BulkApproveRequestDto request, Guid currentEmployeeId);
    }

    public class BulkApprovalService : IBulkApprovalService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowApprovalActionService _actionService;
        private readonly IAntiSelfApprovalGuardService _antiSelfApprovalGuard;

        public BulkApprovalService(
            ApplicationDbContext context,
            IWorkflowApprovalActionService actionService,
            IAntiSelfApprovalGuardService antiSelfApprovalGuard)
        {
            _context = context;
            _actionService = actionService;
            _antiSelfApprovalGuard = antiSelfApprovalGuard;
        }

        public async Task<BulkApproveResultDto> ExecuteBulkApprovalAsync(BulkApproveRequestDto request, Guid currentEmployeeId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Validasi PIN Otorisasi (HRD-DEC-187 / VR-WF-07)
            if (string.IsNullOrWhiteSpace(request.PinSignature) || request.PinSignature.Length < 6)
            {
                throw new InvalidOperationException("PIN keamanan otorisasi massal wajib minimal 6 digit sah (HRD-DEC-187)");
            }

            var result = new BulkApproveResultDto
            {
                TotalRequested = request.StepInstanceIds.Count
            };

            foreach (var stepId in request.StepInstanceIds)
            {
                var step = await _context.TrxWorkflowStepInstances
                    .Include(s => s.WorkflowInstance)
                    .FirstOrDefaultAsync(s => s.Id == stepId);

                if (step == null || step.WorkflowInstance == null)
                {
                    result.TotalSkipped++;
                    result.SkippedItems.Add(new BulkApproveSkippedItemDto
                    {
                        StepInstanceId = stepId,
                        WorkflowNumber = "-",
                        Reason = "Tahapan alur kerja tidak ditemukan"
                    });
                    continue;
                }

                var instance = step.WorkflowInstance;

                // 1. Guard Anti Self-Approval
                if (instance.RequestedByEmployeeId == currentEmployeeId)
                {
                    result.TotalSkipped++;
                    result.SkippedItems.Add(new BulkApproveSkippedItemDto
                    {
                        StepInstanceId = stepId,
                        WorkflowNumber = instance.RequestNumber,
                        Reason = "Ditolak dari batch: Pejabat tidak boleh menyetujui permohonan mandiri sendiri (Anti Self-Approval)"
                    });
                    continue;
                }

                // 2. Guard 100% Indikator Hijau (HRD-DEC-187):
                // Tiket dengan status Urgent, Overdue, atau berisiko tinggi wajib ditinjau perorangan
                if (instance.IsOverdue || instance.PriorityLevel == "Urgent")
                {
                    result.TotalSkipped++;
                    result.SkippedItems.Add(new BulkApproveSkippedItemDto
                    {
                        StepInstanceId = stepId,
                        WorkflowNumber = instance.RequestNumber,
                        Reason = "Dikeluarkan dari batch: Memiliki indikator risiko KUNING/MERAH atau prioritas darurat (Wajib buka detail)"
                    });
                    continue;
                }

                try
                {
                    var actionRes = await _actionService.ApproveAsync(stepId, new ApproveActionRequestDto
                    {
                        Note = request.BatchNote ?? "Disetujui melalui persetujuan massal bersyarat (Bulk Approval)",
                        PinSignature = request.PinSignature
                    }, currentEmployeeId);

                    if (actionRes.Success)
                    {
                        result.TotalApproved++;
                        result.ApprovedStepIds.Add(stepId);
                    }
                    else
                    {
                        result.TotalSkipped++;
                        result.SkippedItems.Add(new BulkApproveSkippedItemDto
                        {
                            StepInstanceId = stepId,
                            WorkflowNumber = instance.RequestNumber,
                            Reason = actionRes.Message
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.TotalSkipped++;
                    result.SkippedItems.Add(new BulkApproveSkippedItemDto
                    {
                        StepInstanceId = stepId,
                        WorkflowNumber = instance.RequestNumber,
                        Reason = ex.Message
                    });
                }
            }

            return result;
        }
    }
}
