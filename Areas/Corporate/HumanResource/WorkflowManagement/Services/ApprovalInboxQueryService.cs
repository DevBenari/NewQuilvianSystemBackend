using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IApprovalInboxQueryService
    {
        Task<QuilvianSystemBackend.Responses.PagedResult<ApprovalInboxItemV2ResponseDto>> GetInboxAsync(
            Guid employeeId, string? status, string? requestType, bool? isUrgent, bool? isDelegated, int pageNumber, int pageSize);

        Task<ApprovalInboxSummaryV2ResponseDto> GetInboxSummaryAsync(Guid employeeId);
    }

    public class ApprovalInboxQueryService : IApprovalInboxQueryService
    {
        private readonly ApplicationDbContext _context;

        public ApprovalInboxQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<QuilvianSystemBackend.Responses.PagedResult<ApprovalInboxItemV2ResponseDto>> GetInboxAsync(
            Guid employeeId, string? status, string? requestType, bool? isUrgent, bool? isDelegated, int pageNumber, int pageSize)
        {
            var now = DateTime.UtcNow;

            // 1. Ambil penugasan tiket di mana user adalah approver langsung
            // ATAU user adalah Plh aktif (WF-B3 / HRD-DEC-184)
            var activeDelegations = await _context.TrxApprovalDelegations
                .Where(d => d.DelegateEmployeeId == employeeId &&
                            d.IsActive &&
                            d.StartDate <= now &&
                            d.EndDate >= now)
                .Select(d => d.PrincipalEmployeeId)
                .ToListAsync();

            var query = _context.TrxWorkflowApproverAssignments
                .Include(a => a.WorkflowInstance)
                    .ThenInclude(w => w!.RequestedByEmployee)
                        .ThenInclude(e => e!.PrimaryDepartment)
                .Include(a => a.WorkflowStepInstance)
                .Where(a => a.AssignmentStatus == "Pending" && a.IsCurrentAssignment)
                .Where(a => a.ApproverEmployeeId == employeeId ||
                            (a.ApproverEmployeeId.HasValue && activeDelegations.Contains(a.ApproverEmployeeId.Value)))
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(requestType))
            {
                query = query.Where(a => a.WorkflowInstance != null &&
                                         a.WorkflowInstance.ReferenceType.ToLower().Contains(requestType.ToLower()));
            }

            if (isDelegated.HasValue)
            {
                query = query.Where(a => a.IsDelegated == isDelegated.Value || 
                                         (a.ApproverEmployeeId.HasValue && activeDelegations.Contains(a.ApproverEmployeeId.Value)));
            }

            var allItems = await query.ToListAsync();

            // Transform to DTO & compute runtime visual indicators
            var mappedList = new List<ApprovalInboxItemV2ResponseDto>();
            foreach (var a in allItems)
            {
                var inst = a.WorkflowInstance;
                if (inst == null) continue;

                var deadline = inst.SlaDeadline ?? a.DueAt;
                var overdue = inst.IsOverdue || (deadline.HasValue && deadline.Value < now);
                var urgent = overdue || (deadline.HasValue && (deadline.Value - now).TotalHours <= 4.0);

                if (isUrgent.HasValue && isUrgent.Value != urgent)
                {
                    continue;
                }

                bool isDelegatedTicket = a.IsDelegated || (a.ApproverEmployeeId.HasValue && activeDelegations.Contains(a.ApproverEmployeeId.Value));
                string? delegatorName = null;
                if (isDelegatedTicket)
                {
                    delegatorName = "Pejabat Definitif Terdelegasi (Plh)";
                }

                string risk = "Green";
                if (overdue) risk = "Red";
                else if (urgent || inst.PriorityLevel == "Urgent") risk = "Yellow";

                mappedList.Add(new ApprovalInboxItemV2ResponseDto
                {
                    WorkflowInstanceId = inst.Id,
                    StepInstanceId = a.WorkflowStepInstanceId,
                    WorkflowNumber = inst.RequestNumber,
                    RequestType = inst.ReferenceType,
                    ApplicantEmployeeId = inst.RequestedByEmployeeId ?? Guid.Empty,
                    ApplicantName = inst.RequestedByEmployee?.FullName ?? "Pegawai Pemohon",
                    DepartmentName = inst.RequestedByEmployee?.PrimaryDepartment?.DepartmentName ?? "Departemen",
                    SubmittedAt = inst.SubmittedAt ?? inst.StartedAt,
                    SlaDeadline = deadline,
                    IsOverdue = overdue,
                    IsUrgent = urgent,
                    IsDelegated = isDelegatedTicket,
                    DelegatedFromEmployeeName = delegatorName,
                    PriorityLevel = inst.PriorityLevel,
                    Amount = inst.Amount,
                    SummaryText = inst.RequestContextJson ?? inst.CompletionNote ?? $"Permohonan {inst.ReferenceType}",
                    RiskIndicator = risk,
                    CurrentStepNumber = inst.CurrentStepOrder,
                    TotalSteps = inst.TotalSteps
                });
            }

            var totalData = mappedList.Count;
            var totalPage = (int)Math.Ceiling(totalData / (double)pageSize);

            var pagedItems = mappedList
                .OrderByDescending(x => x.IsUrgent)
                .ThenBy(x => x.SlaDeadline)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new QuilvianSystemBackend.Responses.PagedResult<ApprovalInboxItemV2ResponseDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = totalPage,
                Items = pagedItems
            };
        }

        public async Task<ApprovalInboxSummaryV2ResponseDto> GetInboxSummaryAsync(Guid employeeId)
        {
            var res = await GetInboxAsync(employeeId, null, null, null, null, 1, 1000);
            var items = res.Items ?? new List<ApprovalInboxItemV2ResponseDto>();

            var summary = new ApprovalInboxSummaryV2ResponseDto
            {
                TotalPendingCount = items.Count,
                UrgentCount = items.Count(x => x.IsUrgent),
                DelegatedCount = items.Count(x => x.IsDelegated),
                OverdueCount = items.Count(x => x.IsOverdue)
            };

            foreach (var group in items.GroupBy(x => x.RequestType))
            {
                summary.ByCategoryCounts[group.Key] = group.Count();
            }

            return summary;
        }
    }
}
