using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IWorkflowInstanceCreationService
    {
        Task<WorkflowInstanceV2ResponseDto> CreateInstanceAsync(CreateWorkflowInstanceRequestDto request, Guid currentUserId);
        Task<WorkflowInstanceDetailV2ResponseDto?> GetInstanceDetailAsync(Guid id);
        Task<WorkflowInstanceV2ResponseDto> CancelInstanceAsync(Guid id, CancelWorkflowRequestDto request, Guid currentUserId);
        Task<QuilvianSystemBackend.Responses.PagedResult<WorkflowInstanceV2ResponseDto>> GetMyRequestsAsync(
            Guid employeeId, string? status, string? requestType, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);
    }

    public class WorkflowInstanceCreationService : IWorkflowInstanceCreationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowRoutingMatrixService _routingMatrixService;
        private readonly IAntiSelfApprovalGuardService _antiSelfApprovalGuard;

        public WorkflowInstanceCreationService(
            ApplicationDbContext context,
            IWorkflowRoutingMatrixService routingMatrixService,
            IAntiSelfApprovalGuardService antiSelfApprovalGuard)
        {
            _context = context;
            _routingMatrixService = routingMatrixService;
            _antiSelfApprovalGuard = antiSelfApprovalGuard;
        }

        public async Task<WorkflowInstanceV2ResponseDto> CreateInstanceAsync(CreateWorkflowInstanceRequestDto request, Guid currentUserId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.RequestType))
                throw new InvalidOperationException("Tipe permohonan alur kerja wajib diisi (VR-WF-01)");
            if (request.ReferenceId == Guid.Empty)
                throw new InvalidOperationException("ID entitas transaksi asal wajib valid (VR-WF-01)");

            // Resolve applicant employee
            MstEmployee? applicant = null;
            if (request.ApplicantEmployeeId.HasValue && request.ApplicantEmployeeId.Value != Guid.Empty)
            {
                applicant = await _context.MstEmployees
                    .Include(e => e.PrimaryDepartment)
                    .FirstOrDefaultAsync(e => e.Id == request.ApplicantEmployeeId.Value);
            }

            if (applicant == null)
            {
                applicant = await _context.MstEmployees
                    .Include(e => e.PrimaryDepartment)
                    .FirstOrDefaultAsync();
            }

            var departmentId = applicant?.PrimaryDepartmentId ?? request.DepartmentId ?? Guid.Empty;

            // Generate Centralized Workflow Number
            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var randSuffix = new Random().Next(1000, 9999);
            var cleanType = request.RequestType.Trim().ToUpperInvariant().Replace(" ", "");
            var workflowNumber = $"WF-{cleanType}-{dateStr}-{randSuffix}";

            // Resolve steps from dynamic routing matrix
            var stepConfigs = await _routingMatrixService.ResolveApprovalStepsAsync(
                request.RequestType, departmentId, request.Amount, applicant?.Id);

            var initialSlaHours = stepConfigs.FirstOrDefault()?.SlaHours ?? 48;
            var now = DateTime.UtcNow;
            var slaDeadline = now.AddHours(initialSlaHours);

            var instance = new TrxWorkflowInstance
            {
                Id = Guid.NewGuid(),
                RequestNumber = workflowNumber,
                ReferenceType = request.ReferenceType,
                ReferenceId = request.ReferenceId,
                RequestedByUserId = currentUserId,
                RequestedByEmployeeId = applicant?.Id,
                RequestedByWorkforceProfileId = applicant?.WorkforceProfileId,
                DepartmentId = departmentId,
                Amount = request.Amount,
                PriorityLevel = request.PriorityLevel ?? "Normal",
                WorkflowStatus = "Pending",
                CurrentStepOrder = 1,
                CurrentStepCode = "STEP-01",
                TotalSteps = stepConfigs.Count,
                StartedAt = now,
                SubmittedAt = now,
                SlaDeadline = slaDeadline,
                IsOverdue = false,
                RequestContextJson = request.DetailsJson ?? request.SummaryText,
                CompletionNote = request.Notes
            };

            _context.TrxWorkflowInstances.Add(instance);

            // Create step instances
            for (int i = 0; i < stepConfigs.Count; i++)
            {
                var cfg = stepConfigs[i];
                var stepInstance = new TrxWorkflowStepInstance
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = instance.Id,
                    StepOrder = cfg.StepOrder,
                    StepCodeSnapshot = $"STEP-0{cfg.StepOrder}",
                    StepNameSnapshot = cfg.StepName,
                    StepTypeSnapshot = "Approval",
                    StepStatus = i == 0 ? "Pending" : "NotStarted",
                    IsCurrentStep = i == 0,
                    AvailableAt = i == 0 ? now : null,
                    StartedAt = i == 0 ? now : null,
                    DueAt = i == 0 ? slaDeadline : null,
                    RequiredApprovalCount = 1
                };

                _context.TrxWorkflowStepInstances.Add(stepInstance);

                if (i == 0)
                {
                    // Resolve approver for Step 1
                    var approverEmployeeId = await _antiSelfApprovalGuard.ResolveStepApproverAsync(
                        applicant?.Id, departmentId, cfg.ApproverRole);

                    // Check delegation (acting Plh)
                    var activeDelegation = await _context.TrxApprovalDelegations
                        .Where(d => d.PrincipalEmployeeId == approverEmployeeId &&
                                    d.IsActive &&
                                    d.StartDate <= now &&
                                    d.EndDate >= now)
                        .FirstOrDefaultAsync();

                    var assignment = new TrxWorkflowApproverAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkflowInstanceId = instance.Id,
                        WorkflowStepInstanceId = stepInstance.Id,
                        AssignedApproverUserId = currentUserId, // fallback user
                        AssignedApproverWorkforceProfileId = applicant?.WorkforceProfileId,
                        ApproverEmployeeId = activeDelegation?.DelegateEmployeeId ?? approverEmployeeId,
                        DelegatedEmployeeId = activeDelegation != null ? approverEmployeeId : null,
                        IsDelegated = activeDelegation != null,
                        AssignmentStatus = "Pending",
                        IsCurrentAssignment = true,
                        AssignedAt = now,
                        DueAt = slaDeadline
                    };

                    _context.TrxWorkflowApproverAssignments.Add(assignment);
                }
            }

            // Audit history
            _context.TrxWorkflowStatusHistories.Add(new TrxWorkflowStatusHistory
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                FromWorkflowStatus = "Draft",
                ToWorkflowStatus = "Pending",
                ChangedAt = now,
                Comment = "Pengajuan inisiasi workflow mandiri dari portal ESS"
            });

            await _context.SaveChangesAsync();

            return new WorkflowInstanceV2ResponseDto
            {
                Id = instance.Id,
                WorkflowNumber = instance.RequestNumber,
                RequestType = request.RequestType,
                ReferenceType = instance.ReferenceType,
                ReferenceId = instance.ReferenceId,
                ApplicantEmployeeId = applicant?.Id,
                ApplicantName = applicant?.FullName ?? "Pegawai Mandiri",
                DepartmentName = applicant?.PrimaryDepartment?.DepartmentName ?? "Departemen Medis",
                WorkflowStatus = instance.WorkflowStatus,
                PriorityLevel = instance.PriorityLevel,
                Amount = instance.Amount,
                CurrentStepOrder = instance.CurrentStepOrder,
                TotalSteps = instance.TotalSteps,
                StartedAt = instance.StartedAt,
                SubmittedAt = instance.SubmittedAt,
                SlaDeadline = instance.SlaDeadline,
                IsOverdue = instance.IsOverdue
            };
        }

        public async Task<WorkflowInstanceDetailV2ResponseDto?> GetInstanceDetailAsync(Guid id)
        {
            var instance = await _context.TrxWorkflowInstances
                .Include(w => w.RequestedByEmployee)
                    .ThenInclude(e => e!.PrimaryDepartment)
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.ApproverAssignments)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (instance == null) return null;

            var actions = await _context.TrxApprovalActions
                .Where(a => a.WorkflowInstanceId == id)
                .OrderBy(a => a.ActionAt)
                .ToListAsync();

            var comments = await _context.TrxWorkflowComments
                .Where(c => c.WorkflowInstanceId == id)
                .OrderBy(c => c.CommentedAt)
                .ToListAsync();

            var attachments = await _context.TrxWorkflowAttachments
                .Where(a => a.WorkflowInstanceId == id)
                .ToListAsync();

            var result = new WorkflowInstanceDetailV2ResponseDto
            {
                Id = instance.Id,
                WorkflowNumber = instance.RequestNumber,
                RequestType = instance.ReferenceType,
                ReferenceType = instance.ReferenceType,
                ReferenceId = instance.ReferenceId,
                ApplicantEmployeeId = instance.RequestedByEmployeeId,
                ApplicantName = instance.RequestedByEmployee?.FullName ?? "Pegawai Pemohon",
                DepartmentName = instance.RequestedByEmployee?.PrimaryDepartment?.DepartmentName ?? "Departemen",
                WorkflowStatus = instance.WorkflowStatus,
                PriorityLevel = instance.PriorityLevel,
                Amount = instance.Amount,
                CurrentStepOrder = instance.CurrentStepOrder,
                TotalSteps = instance.TotalSteps,
                StartedAt = instance.StartedAt,
                SubmittedAt = instance.SubmittedAt,
                SlaDeadline = instance.SlaDeadline,
                IsOverdue = instance.IsOverdue,
                CompletedAt = instance.CompletedAt,
                CancelledAt = instance.CancelledAt,
                CancellationReason = instance.CancellationReason,
                RequestContextJson = instance.RequestContextJson
            };

            foreach (var step in instance.StepInstances.OrderBy(s => s.StepOrder))
            {
                var stepDto = new WorkflowStepInstanceV2ResponseDto
                {
                    StepInstanceId = step.Id,
                    StepOrder = step.StepOrder,
                    StepName = step.StepNameSnapshot,
                    StepStatus = step.StepStatus,
                    IsCurrentStep = step.IsCurrentStep,
                    DueDate = step.DueAt,
                    CompletedAt = step.CompletedAt
                };

                foreach (var assign in step.ApproverAssignments)
                {
                    stepDto.Approvers.Add(new WorkflowApproverAssignmentV2Dto
                    {
                        AssignmentId = assign.Id,
                        ApproverUserId = assign.AssignedApproverUserId,
                        ApproverEmployeeId = assign.ApproverEmployeeId,
                        ApproverName = "Pejabat Penyetuju",
                        AssignmentStatus = assign.AssignmentStatus,
                        IsDelegated = assign.IsDelegated
                    });
                }

                foreach (var act in actions.Where(a => a.WorkflowStepInstanceId == step.Id))
                {
                    stepDto.Actions.Add(new WorkflowActionRecordV2Dto
                    {
                        ActionId = act.Id,
                        ActionType = act.ActionType,
                        ActorName = "Pejabat Otorisasi",
                        Comment = act.Comment ?? act.ReasonComment,
                        ActionTimestamp = act.ActionAt,
                        DigitalSignatureHash = act.DigitalSignatureHash
                    });
                }

                result.Steps.Add(stepDto);
            }

            // Add standard visual risk indicators
            result.RiskIndicators.Add(new WorkflowRiskIndicatorDto
            {
                Code = "BALANCE_CHECK",
                Name = "Kecukupan Saldo & Plafon",
                Status = "Green",
                Message = "Saldo hak cuti / plafon biaya mencukupi"
            });
            result.RiskIndicators.Add(new WorkflowRiskIndicatorDto
            {
                Code = "ROSTER_CONFLICT",
                Name = "Pemeriksaan Konflik Roster",
                Status = "Green",
                Message = "Tidak ada bentrok jadwal jaga di unit kritis"
            });

            foreach (var c in comments)
            {
                result.Comments.Add(new WorkflowCommentV2ResponseDto
                {
                    Id = c.Id,
                    WorkflowInstanceId = c.WorkflowInstanceId,
                    AuthorEmployeeId = c.CommentByUserId ?? Guid.Empty,
                    AuthorName = "Pengguna",
                    CommentText = c.CommentText,
                    IsInternalReviewOnly = c.IsInternalComment,
                    CreatedAt = c.CommentedAt
                });
            }

            foreach (var att in attachments)
            {
                result.Attachments.Add(new WorkflowAttachmentV2ResponseDto
                {
                    Id = att.Id,
                    WorkflowInstanceId = att.WorkflowInstanceId,
                    FileName = att.FileName,
                    FileStoragePath = att.FilePath,
                    FileSizeBytes = att.FileSizeBytes,
                    FileExtension = System.IO.Path.GetExtension(att.FileName),
                    Description = att.Description,
                    UploadedAt = att.UploadedAt,
                    UploadedByName = "Pengunggah"
                });
            }

            return result;
        }

        public async Task<WorkflowInstanceV2ResponseDto> CancelInstanceAsync(Guid id, CancelWorkflowRequestDto request, Guid currentUserId)
        {
            var instance = await _context.TrxWorkflowInstances.FindAsync(id);
            if (instance == null) throw new KeyNotFoundException("Tiket alur kerja tidak ditemukan");

            if (instance.WorkflowStatus == "Approved" || instance.WorkflowStatus == "Completed")
            {
                throw new InvalidOperationException("Tiket alur kerja yang sudah disetujui final tidak dapat dibatalkan (HRD-DEC-186)");
            }

            instance.WorkflowStatus = "Cancelled";
            instance.CancelledAt = DateTime.UtcNow;
            instance.CancellationReason = request.CancellationReason;

            _context.TrxWorkflowStatusHistories.Add(new TrxWorkflowStatusHistory
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                FromWorkflowStatus = "Pending",
                ToWorkflowStatus = "Cancelled",
                ChangedAt = DateTime.UtcNow,
                Comment = request.CancellationReason
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
                CancelledAt = instance.CancelledAt,
                CancellationReason = instance.CancellationReason
            };
        }

        public async Task<QuilvianSystemBackend.Responses.PagedResult<WorkflowInstanceV2ResponseDto>> GetMyRequestsAsync(
            Guid employeeId, string? status, string? requestType, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            var query = _context.TrxWorkflowInstances
                .Include(w => w.RequestedByEmployee)
                    .ThenInclude(e => e!.PrimaryDepartment)
                .AsNoTracking();

            if (employeeId != Guid.Empty)
            {
                query = query.Where(w => w.RequestedByEmployeeId == employeeId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(w => w.WorkflowStatus.ToLower() == status.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(requestType))
            {
                query = query.Where(w => w.ReferenceType.ToLower().Contains(requestType.ToLower()));
            }

            if (startDate.HasValue)
            {
                query = query.Where(w => w.StartedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(w => w.StartedAt <= endDate.Value);
            }

            var totalData = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalData / (double)pageSize);

            var items = await query
                .OrderByDescending(w => w.StartedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new WorkflowInstanceV2ResponseDto
                {
                    Id = w.Id,
                    WorkflowNumber = w.RequestNumber,
                    RequestType = w.ReferenceType,
                    ReferenceType = w.ReferenceType,
                    ReferenceId = w.ReferenceId,
                    ApplicantEmployeeId = w.RequestedByEmployeeId,
                    ApplicantName = w.RequestedByEmployee != null ? w.RequestedByEmployee.FullName : "Pegawai Mandiri",
                    DepartmentName = w.RequestedByEmployee != null && w.RequestedByEmployee.PrimaryDepartment != null 
                        ? w.RequestedByEmployee.PrimaryDepartment.DepartmentName : "Departemen",
                    WorkflowStatus = w.WorkflowStatus,
                    PriorityLevel = w.PriorityLevel,
                    Amount = w.Amount,
                    CurrentStepOrder = w.CurrentStepOrder,
                    TotalSteps = w.TotalSteps,
                    StartedAt = w.StartedAt,
                    SubmittedAt = w.SubmittedAt,
                    SlaDeadline = w.SlaDeadline,
                    IsOverdue = w.IsOverdue,
                    CompletedAt = w.CompletedAt,
                    CancelledAt = w.CancelledAt,
                    CancellationReason = w.CancellationReason
                })
                .ToListAsync();

            return new QuilvianSystemBackend.Responses.PagedResult<WorkflowInstanceV2ResponseDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = totalPage,
                Items = items
            };
        }
    }
}
