using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            Guid userId, string? status, string? requestType, bool? isUrgent, bool? isDelegated, int pageNumber, int pageSize);

        Task<ApprovalInboxSummaryV2ResponseDto> GetInboxSummaryAsync(Guid userId);

        Task<WorkflowMyActionDto?> GetMyActionAsync(Guid workflowInstanceId, Guid userId, CancellationToken cancellationToken = default);

        Task<WorkflowActionableAssignment?> FindActionableAssignmentAsync(Guid stepInstanceId, Guid userId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// BE-HRD-444 (kontrak v29 §35.2): penugasan engine terbuka pada satu tahap yang boleh dijalankan pengguna login,
    /// dipilih dengan aturan yang sama dengan antrean dan <c>myAction</c>.
    /// </summary>
    public sealed class WorkflowActionableAssignment
    {
        public Guid AssignmentId { get; init; }
        public Guid StepInstanceId { get; init; }
        public Guid WorkflowInstanceId { get; init; }
        public Guid WorkflowDefinitionId { get; init; }
        public Guid WorkflowStepId { get; init; }
        public string? DefinitionRequestType { get; init; }
        public string? StepType { get; init; }
        public string RequestNumber { get; init; } = string.Empty;
        public string ReferenceType { get; init; } = string.Empty;
        public Guid RequestedByUserId { get; init; }
        public bool IsDelegated { get; init; }
    }

    /// <summary>
    /// BE-HRD-443 (kontrak v29 §35.1, §35.6; INT-HR-111, INT-HR-113; HRD-DEC-229, HRD-DEC-234): Kotak Masuk Persetujuan
    /// Terpadu membaca penugasan engine <see cref="WorkflowService"/> yang terbuka pada tahap aktif, milik pengguna login
    /// atau milik pejabat yang mendelegasikan kepadanya secara aktif. Penugasan engine V2 lama (<c>ApproverEmployeeId</c>
    /// terisi) tidak ditampilkan. Antrean, ringkasan, dan blok <c>myAction</c> dihitung dari himpunan yang sama.
    /// </summary>
    public class ApprovalInboxQueryService : IApprovalInboxQueryService
    {
        // Himpunan status terbuka sama dengan Kotak Masuk lama (ApprovalInboxService.OpenAssignmentStatuses).
        private static readonly string[] OpenAssignmentStatuses =
        {
            WorkflowValueConstants.AssignmentStatus.Pending,
            WorkflowValueConstants.AssignmentStatus.Available,
            WorkflowValueConstants.AssignmentStatus.InProgress,
            WorkflowValueConstants.AssignmentStatus.Delegated
        };

        private const double UrgentThresholdHours = 4.0;

        internal const string BulkSkipNotAllowed = "Jenis permohonan ini harus disetujui satu per satu.";
        internal const string BulkSkipOwnRequest = "Pejabat tidak boleh menyetujui permohonan sendiri.";
        internal const string BulkSkipUrgent = "Permohonan mendesak atau lewat SLA wajib ditinjau satu per satu.";

        private readonly ApplicationDbContext _context;

        public ApprovalInboxQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<QuilvianSystemBackend.Responses.PagedResult<ApprovalInboxItemV2ResponseDto>> GetInboxAsync(
            Guid userId, string? status, string? requestType, bool? isUrgent, bool? isDelegated, int pageNumber, int pageSize)
        {
            // Kontrak v29 §35.1 tidak lagi memakai filter status: antrean hanya berisi penugasan terbuka.
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : pageSize;

            var now = DateTime.UtcNow;
            var items = (await LoadOpenRowsAsync(userId, null, requestType, now, CancellationToken.None))
                .Select(row => ToInboxItem(row, userId, now))
                .Where(x => !isUrgent.HasValue || x.IsUrgent == isUrgent.Value)
                .Where(x => !isDelegated.HasValue || x.IsDelegated == isDelegated.Value)
                .OrderByDescending(x => x.IsUrgent)
                .ThenBy(x => x.SlaDeadline ?? DateTime.MaxValue)
                .ThenBy(x => x.SubmittedAt)
                .ToList();

            return new QuilvianSystemBackend.Responses.PagedResult<ApprovalInboxItemV2ResponseDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = items.Count,
                TotalPage = (int)Math.Ceiling(items.Count / (double)pageSize),
                Items = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };
        }

        public async Task<ApprovalInboxSummaryV2ResponseDto> GetInboxSummaryAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var items = (await LoadOpenRowsAsync(userId, null, null, now, CancellationToken.None))
                .Select(row => ToInboxItem(row, userId, now))
                .ToList();

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

        public async Task<WorkflowMyActionDto?> GetMyActionAsync(
            Guid workflowInstanceId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty)
            {
                return null;
            }

            var now = DateTime.UtcNow;
            var row = (await LoadOpenRowsAsync(userId, workflowInstanceId, null, now, cancellationToken))
                .FirstOrDefault();

            if (row == null)
            {
                return null;
            }

            var rejectionReasons = await WorkflowRejectionReasonOptions.QueryAsync(
                _context,
                row.DefinitionRequestType,
                row.WorkflowDefinitionId,
                row.WorkflowStepId,
                now.Date,
                cancellationToken);

            return new WorkflowMyActionDto
            {
                AssignmentId = row.AssignmentId,
                StepInstanceId = row.StepInstanceId,
                IsDelegated = IsDelegatedRow(row),
                OnBehalfOfName = ResolveOnBehalfOfName(row),
                AvailableActions = new List<string>
                {
                    WorkflowService.ResolveApprovalActionType(row.StepType),
                    WorkflowValueConstants.ActionType.Reject,
                    WorkflowValueConstants.ActionType.RequestRevision
                },
                RejectionReasons = rejectionReasons
            };
        }

        public async Task<WorkflowActionableAssignment?> FindActionableAssignmentAsync(
            Guid stepInstanceId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (stepInstanceId == Guid.Empty)
            {
                return null;
            }

            var row = (await LoadOpenRowsAsync(userId, null, null, DateTime.UtcNow, cancellationToken, stepInstanceId))
                .FirstOrDefault();

            return row == null
                ? null
                : new WorkflowActionableAssignment
                {
                    AssignmentId = row.AssignmentId,
                    StepInstanceId = row.StepInstanceId,
                    WorkflowInstanceId = row.WorkflowInstanceId,
                    WorkflowDefinitionId = row.WorkflowDefinitionId,
                    WorkflowStepId = row.WorkflowStepId,
                    DefinitionRequestType = row.DefinitionRequestType,
                    StepType = row.StepType,
                    RequestNumber = row.RequestNumber,
                    ReferenceType = row.ReferenceType,
                    RequestedByUserId = row.RequestedByUserId,
                    IsDelegated = IsDelegatedRow(row)
                };
        }

        /// <summary>
        /// BE-HRD-443 (kontrak v29 §35.5, VR-WF-32): alasan sebuah butir dilewati Setujui Massal, atau <c>null</c> bila layak.
        /// Syarat "pengguna memegang penugasan" sudah dipenuhi oleh himpunan antrean, dan hasil engine dinilai saat aksi.
        /// </summary>
        internal static string? ResolveBulkApprovalSkipReason(bool allowBulkApproval, bool isOwnRequest, bool isUrgentOrOverdue)
        {
            if (!allowBulkApproval)
            {
                return BulkSkipNotAllowed;
            }

            if (isOwnRequest)
            {
                return BulkSkipOwnRequest;
            }

            return isUrgentOrOverdue ? BulkSkipUrgent : null;
        }

        /// <summary>
        /// Penugasan engine terbuka pada tahap aktif yang boleh diputuskan <paramref name="userId"/>: atas namanya sendiri,
        /// atau atas nama pejabat yang mendelegasikan kepadanya secara aktif dengan cakupan definisi/tahap yang cocok
        /// (aturan sama dengan <see cref="WorkflowDelegationResolver"/>). Baris Plh atas permohonan pengguna sendiri tidak
        /// dimuat (VR-WF-02). Satu baris per tahap: penugasan sendiri didahulukan.
        /// </summary>
        private async Task<List<OpenAssignmentRow>> LoadOpenRowsAsync(
            Guid userId,
            Guid? workflowInstanceId,
            string? requestType,
            DateTime now,
            CancellationToken cancellationToken,
            Guid? stepInstanceId = null)
        {
            if (userId == Guid.Empty)
            {
                return new List<OpenAssignmentRow>();
            }

            var delegations = await _context.TrxApprovalDelegations
                .AsNoTracking()
                .WhereActiveAt(now)
                .Where(x => x.DelegateUserId == userId && x.DelegatorUserId != userId)
                .Select(x => new DelegationScope
                {
                    DelegatorUserId = x.DelegatorUserId,
                    AppliesToAllWorkflows = x.AppliesToAllWorkflows,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowStepId = x.WorkflowStepId,
                    EffectiveStartAt = x.EffectiveStartAt,
                    DelegatorDisplayName = x.DelegatorUser != null ? x.DelegatorUser.DisplayName : null,
                    DelegatorUserName = x.DelegatorUser != null ? x.DelegatorUser.UserName : null
                })
                .ToListAsync(cancellationToken);

            var holderUserIds = delegations
                .Select(x => x.DelegatorUserId)
                .Append(userId)
                .Distinct()
                .ToList();

            var query = _context.TrxWorkflowApproverAssignments
                .AsNoTracking()
                .Where(a =>
                    a.IsActive &&
                    !a.IsDelete &&
                    !a.IsCancel &&
                    a.IsCurrentAssignment &&
                    a.ApproverEmployeeId == null &&
                    OpenAssignmentStatuses.Contains(a.AssignmentStatus) &&
                    holderUserIds.Contains(a.AssignedApproverUserId) &&
                    a.WorkflowInstance != null &&
                    !a.WorkflowInstance.IsDelete &&
                    a.WorkflowInstance.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress &&
                    a.WorkflowStepInstance != null &&
                    !a.WorkflowStepInstance.IsDelete &&
                    a.WorkflowStepInstance.IsCurrentStep &&
                    a.WorkflowStepInstance.StepStatus == WorkflowValueConstants.StepStatus.InProgress);

            if (workflowInstanceId.HasValue)
            {
                query = query.Where(a => a.WorkflowInstanceId == workflowInstanceId.Value);
            }

            if (stepInstanceId.HasValue)
            {
                query = query.Where(a => a.WorkflowStepInstanceId == stepInstanceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(requestType))
            {
                var normalizedType = requestType.Trim().ToUpper();
                query = query.Where(a => a.WorkflowInstance!.ReferenceType.ToUpper() == normalizedType);
            }

            var rows = await query
                .Select(a => new OpenAssignmentRow
                {
                    AssignmentId = a.Id,
                    StepInstanceId = a.WorkflowStepInstanceId,
                    AssignmentOrder = a.AssignmentOrder,
                    AssignedApproverUserId = a.AssignedApproverUserId,
                    IsDelegatedAssignment = a.IsDelegated,
                    OriginalApproverDisplayName = a.OriginalApproverUser != null ? a.OriginalApproverUser.DisplayName : null,
                    OriginalApproverUserName = a.OriginalApproverUser != null ? a.OriginalApproverUser.UserName : null,
                    AssignmentDueAt = a.DueAt,
                    WorkflowStepId = a.WorkflowStepInstance!.WorkflowStepId,
                    StepType = a.WorkflowStepInstance.StepTypeSnapshot,
                    WorkflowInstanceId = a.WorkflowInstanceId,
                    WorkflowDefinitionId = a.WorkflowInstance!.WorkflowDefinitionId,
                    WorkflowName = a.WorkflowInstance.WorkflowDefinition != null ? a.WorkflowInstance.WorkflowDefinition.WorkflowName : null,
                    DefinitionRequestType = a.WorkflowInstance.WorkflowDefinition != null ? a.WorkflowInstance.WorkflowDefinition.RequestType : null,
                    AllowBulkApproval = a.WorkflowInstance.WorkflowDefinition != null && a.WorkflowInstance.WorkflowDefinition.AllowBulkApproval,
                    RequestNumber = a.WorkflowInstance.RequestNumber,
                    ReferenceType = a.WorkflowInstance.ReferenceType,
                    ExternalReferenceNumber = a.WorkflowInstance.ExternalReferenceNumber,
                    RequestedByUserId = a.WorkflowInstance.RequestedByUserId,
                    RequestedByEmployeeId = a.WorkflowInstance.RequestedByEmployeeId,
                    EmployeeName = a.WorkflowInstance.RequestedByEmployee != null ? a.WorkflowInstance.RequestedByEmployee.FullName : null,
                    DepartmentName = a.WorkflowInstance.RequestedByEmployee != null && a.WorkflowInstance.RequestedByEmployee.PrimaryDepartment != null
                        ? a.WorkflowInstance.RequestedByEmployee.PrimaryDepartment.DepartmentName
                        : null,
                    WorkforceProfileName = a.WorkflowInstance.RequestedByWorkforceProfile != null ? a.WorkflowInstance.RequestedByWorkforceProfile.DisplayName : null,
                    RequesterDisplayName = a.WorkflowInstance.RequestedByUser != null ? a.WorkflowInstance.RequestedByUser.DisplayName : null,
                    RequesterUserName = a.WorkflowInstance.RequestedByUser != null ? a.WorkflowInstance.RequestedByUser.UserName : null,
                    SubmittedAt = a.WorkflowInstance.SubmittedAt ?? a.WorkflowInstance.StartedAt,
                    InstanceSlaDeadline = a.WorkflowInstance.SlaDeadline,
                    InstanceIsOverdue = a.WorkflowInstance.IsOverdue,
                    PriorityLevel = a.WorkflowInstance.PriorityLevel,
                    Amount = a.WorkflowInstance.Amount,
                    CurrentStepOrder = a.WorkflowInstance.CurrentStepOrder,
                    TotalSteps = a.WorkflowInstance.TotalSteps
                })
                .ToListAsync(cancellationToken);

            var visible = new List<OpenAssignmentRow>();
            foreach (var row in rows)
            {
                if (row.AssignedApproverUserId == userId)
                {
                    visible.Add(row);
                    continue;
                }

                if (row.RequestedByUserId == userId)
                {
                    continue;
                }

                row.ActingDelegation = delegations
                    .Where(x => x.DelegatorUserId == row.AssignedApproverUserId && x.Covers(row.WorkflowDefinitionId, row.WorkflowStepId))
                    .OrderByDescending(x => x.WorkflowStepId.HasValue)
                    .ThenByDescending(x => x.WorkflowDefinitionId.HasValue)
                    .ThenByDescending(x => x.EffectiveStartAt)
                    .FirstOrDefault();

                if (row.ActingDelegation != null)
                {
                    visible.Add(row);
                }
            }

            return visible
                .GroupBy(x => x.StepInstanceId)
                .Select(g => g
                    .OrderByDescending(x => x.ActingDelegation == null)
                    .ThenBy(x => x.AssignmentOrder)
                    .First())
                .ToList();
        }

        private static ApprovalInboxItemV2ResponseDto ToInboxItem(OpenAssignmentRow row, Guid userId, DateTime now)
        {
            var deadline = row.AssignmentDueAt ?? row.InstanceSlaDeadline;
            var overdue = row.InstanceIsOverdue || (deadline.HasValue && deadline.Value < now);
            var urgent = overdue || (deadline.HasValue && (deadline.Value - now).TotalHours <= UrgentThresholdHours);
            var isDelegated = IsDelegatedRow(row);

            var risk = "Green";
            if (overdue)
            {
                risk = "Red";
            }
            else if (urgent || string.Equals(row.PriorityLevel, "Urgent", StringComparison.OrdinalIgnoreCase))
            {
                risk = "Yellow";
            }

            var documentNumber = FirstFilled(row.ExternalReferenceNumber, row.RequestNumber);
            var workflowName = FirstFilled(row.WorkflowName, row.ReferenceType);

            return new ApprovalInboxItemV2ResponseDto
            {
                AssignmentId = row.AssignmentId,
                WorkflowInstanceId = row.WorkflowInstanceId,
                StepInstanceId = row.StepInstanceId,
                WorkflowNumber = row.RequestNumber,
                ReferenceNumber = row.ExternalReferenceNumber,
                RequestType = row.ReferenceType,
                ApplicantEmployeeId = row.RequestedByEmployeeId ?? Guid.Empty,
                ApplicantName = FirstFilled(
                    row.EmployeeName,
                    row.WorkforceProfileName,
                    row.RequesterDisplayName,
                    row.RequesterUserName) ?? "Pemohon",
                DepartmentName = FirstFilled(row.DepartmentName) ?? "-",
                SubmittedAt = row.SubmittedAt,
                SlaDeadline = deadline,
                IsOverdue = overdue,
                IsUrgent = urgent,
                IsDelegated = isDelegated,
                DelegatedFromEmployeeName = isDelegated ? ResolveOnBehalfOfName(row) : null,
                PriorityLevel = row.PriorityLevel,
                Amount = row.Amount,
                SummaryText = string.IsNullOrWhiteSpace(workflowName)
                    ? documentNumber ?? string.Empty
                    : $"{documentNumber} — {workflowName}",
                RiskIndicator = risk,
                CurrentStepNumber = row.CurrentStepOrder,
                TotalSteps = row.TotalSteps,
                CanBulkApprove = ResolveBulkApprovalSkipReason(
                    row.AllowBulkApproval,
                    row.RequestedByUserId == userId,
                    urgent || overdue) == null
            };
        }

        private static bool IsDelegatedRow(OpenAssignmentRow row)
        {
            return row.ActingDelegation != null || row.IsDelegatedAssignment;
        }

        private static string? ResolveOnBehalfOfName(OpenAssignmentRow row)
        {
            if (row.ActingDelegation != null)
            {
                return FirstFilled(row.ActingDelegation.DelegatorDisplayName, row.ActingDelegation.DelegatorUserName);
            }

            return row.IsDelegatedAssignment
                ? FirstFilled(row.OriginalApproverDisplayName, row.OriginalApproverUserName)
                : null;
        }

        private static string? FirstFilled(params string?[] values)
        {
            return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim();
        }

        private sealed class DelegationScope
        {
            public Guid DelegatorUserId { get; set; }
            public bool AppliesToAllWorkflows { get; set; }
            public Guid? WorkflowDefinitionId { get; set; }
            public Guid? WorkflowStepId { get; set; }
            public DateTime EffectiveStartAt { get; set; }
            public string? DelegatorDisplayName { get; set; }
            public string? DelegatorUserName { get; set; }

            // Sama dengan WorkflowDelegationResolver.WhereCovers, dievaluasi di memori untuk banyak penugasan sekaligus.
            public bool Covers(Guid workflowDefinitionId, Guid workflowStepId)
            {
                return (AppliesToAllWorkflows ||
                        !WorkflowDefinitionId.HasValue ||
                        WorkflowDefinitionId == workflowDefinitionId) &&
                       (!WorkflowStepId.HasValue || WorkflowStepId == workflowStepId);
            }
        }

        private sealed class OpenAssignmentRow
        {
            public Guid AssignmentId { get; set; }
            public Guid StepInstanceId { get; set; }
            public int AssignmentOrder { get; set; }
            public Guid AssignedApproverUserId { get; set; }
            public bool IsDelegatedAssignment { get; set; }
            public string? OriginalApproverDisplayName { get; set; }
            public string? OriginalApproverUserName { get; set; }
            public DateTime? AssignmentDueAt { get; set; }
            public Guid WorkflowStepId { get; set; }
            public string? StepType { get; set; }
            public Guid WorkflowInstanceId { get; set; }
            public Guid WorkflowDefinitionId { get; set; }
            public string? WorkflowName { get; set; }
            public string? DefinitionRequestType { get; set; }
            public bool AllowBulkApproval { get; set; }
            public string RequestNumber { get; set; } = string.Empty;
            public string ReferenceType { get; set; } = string.Empty;
            public string? ExternalReferenceNumber { get; set; }
            public Guid RequestedByUserId { get; set; }
            public Guid? RequestedByEmployeeId { get; set; }
            public string? EmployeeName { get; set; }
            public string? DepartmentName { get; set; }
            public string? WorkforceProfileName { get; set; }
            public string? RequesterDisplayName { get; set; }
            public string? RequesterUserName { get; set; }
            public DateTime SubmittedAt { get; set; }
            public DateTime? InstanceSlaDeadline { get; set; }
            public bool InstanceIsOverdue { get; set; }
            public string PriorityLevel { get; set; } = "Normal";
            public decimal? Amount { get; set; }
            public int CurrentStepOrder { get; set; }
            public int TotalSteps { get; set; }
            public DelegationScope? ActingDelegation { get; set; }
        }
    }
}
