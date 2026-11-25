using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class OffboardingHandoverService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly QuilvianSystemBackend.Services.Security.AccessPermissionService _accessPermissionService;

        public OffboardingHandoverService(
            ApplicationDbContext dbContext,
            QuilvianSystemBackend.Services.Security.AccessPermissionService accessPermissionService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        }

        public async Task<HandoverSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var checklists = await _dbContext.WfpOffboardingChecklists
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .Select(x => new { x.ChecklistStatus, x.ProgressPercentage })
                .ToListAsync(cancellationToken);

            var pendingTasks = await _dbContext.WfpOffboardingTasks
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.TaskStatus == "Submitted")
                .CountAsync(cancellationToken);

            return new HandoverSummaryResponse
            {
                TotalData = checklists.Count,
                Active = checklists.Count(x => x.ChecklistStatus == "Active"),
                InProgress = checklists.Count(x => x.ChecklistStatus == "InProgress" || (x.ChecklistStatus == "Active" && x.ProgressPercentage > 0 && x.ProgressPercentage < 100)),
                Completed = checklists.Count(x => x.ChecklistStatus == "Completed" || x.ProgressPercentage >= 100),
                PendingVerification = pendingTasks
            };
        }

        public async Task<PagedResult<HandoverChecklistListResponse>> GetPagedAsync(
            string? status,
            string? search,
            int pageNumber = 1,
            int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.WfpOffboardingChecklists
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(x => x.ManagerWorkforceProfile)
                .Where(x => !x.IsDelete && x.IsActive);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.ChecklistStatus.ToLower() == status.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ChecklistNumber.ToLower().Contains(term) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(term)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.ProfileCode.ToLower().Contains(term)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.Employee != null && x.WorkforceProfile.Employee.EmployeeNumber.ToLower().Contains(term)));
            }

            var totalItems = await query.CountAsync(cancellationToken);
            var safePageNumber = Math.Max(1, pageNumber);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .Select(x => new HandoverChecklistListResponse
                {
                    Id = x.Id,
                    ChecklistNumber = x.ChecklistNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeId = x.EmployeeId ?? (x.WorkforceProfile != null && x.WorkforceProfile.Employee != null ? x.WorkforceProfile.Employee.Id : null),
                    EmployeeName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    EmployeeNip = x.WorkforceProfile != null && x.WorkforceProfile.Employee != null ? x.WorkforceProfile.Employee.EmployeeNumber : (x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty),
                    DepartmentName = x.WorkforceProfile != null && x.WorkforceProfile.PrimaryDepartment != null ? x.WorkforceProfile.PrimaryDepartment.DepartmentName : string.Empty,
                    OffboardingType = x.OffboardingType,
                    PlannedStartDate = x.PlannedStartDate,
                    PlannedCompletionDate = x.PlannedCompletionDate,
                    ActualCompletionDate = x.ActualCompletionDate,
                    ChecklistStatus = x.ChecklistStatus,
                    TotalTask = x.TotalTask,
                    RequiredTask = x.RequiredTask,
                    CompletedTask = x.CompletedTask,
                    ProgressPercentage = x.ProgressPercentage,
                    ManagerName = x.ManagerWorkforceProfile != null ? x.ManagerWorkforceProfile.DisplayName : null,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<HandoverChecklistListResponse>
            {
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalData = totalItems,
                TotalPage = (int)Math.Ceiling(totalItems / (double)safePageSize),
                Items = items
            };
        }

        public async Task<HandoverChecklistDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var x = await _dbContext.WfpOffboardingChecklists
                .AsNoTracking()
                .Include(c => c.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .Include(c => c.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(c => c.ManagerWorkforceProfile)
                .Include(c => c.EmployeeSeparation)
                .Include(c => c.Tasks)
                    .ThenInclude(t => t.AssignedToWorkforceProfile)
                .Include(c => c.Tasks)
                    .ThenInclude(t => t.CompletedByUser)
                .Include(c => c.Tasks)
                    .ThenInclude(t => t.VerifiedByUser)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (x == null) return null;

            var taskResponses = x.Tasks
                .Where(t => !t.IsDelete && t.IsActive)
                .OrderBy(t => t.SortOrder)
                .Select(t => new HandoverTaskResponse
                {
                    Id = t.Id,
                    OffboardingChecklistId = t.OffboardingChecklistId,
                    TaskCode = t.TaskCode,
                    TaskName = t.TaskName,
                    TaskCategory = t.TaskCategory,
                    Description = t.Description,
                    AssignedToWorkforceProfileId = t.AssignedToWorkforceProfileId,
                    AssignedToName = t.AssignedToWorkforceProfile != null ? t.AssignedToWorkforceProfile.DisplayName : null,
                    DueDate = t.DueDate,
                    CompletedAt = t.CompletedAt,
                    CompletedByName = t.CompletedByUser != null ? t.CompletedByUser.DisplayName : null,
                    VerifiedAt = t.VerifiedAt,
                    VerifiedByName = t.VerifiedByUser != null ? t.VerifiedByUser.DisplayName : null,
                    TaskStatus = t.TaskStatus,
                    IsRequired = t.IsRequired,
                    RequiresDocument = t.RequiresDocument,
                    DocumentPath = t.DocumentPath,
                    OriginalFileName = t.OriginalFileName,
                    SortOrder = t.SortOrder,
                    CompletionNote = t.CompletionNote,
                    VerificationNote = t.VerificationNote
                })
                .ToList();

            var profile = x.WorkforceProfile;
            var emp = profile?.Employee;

            return new HandoverChecklistDetailResponse
            {
                Id = x.Id,
                ChecklistNumber = x.ChecklistNumber,
                WorkforceProfileId = x.WorkforceProfileId,
                EmployeeId = x.EmployeeId ?? emp?.Id,
                EmployeeName = profile?.DisplayName ?? string.Empty,
                EmployeeNip = emp?.EmployeeNumber ?? profile?.ProfileCode ?? string.Empty,
                DepartmentName = profile?.PrimaryDepartment?.DepartmentName ?? string.Empty,
                OffboardingType = x.OffboardingType,
                PlannedStartDate = x.PlannedStartDate,
                PlannedCompletionDate = x.PlannedCompletionDate,
                ActualCompletionDate = x.ActualCompletionDate,
                ChecklistStatus = x.ChecklistStatus,
                TotalTask = x.TotalTask,
                RequiredTask = x.RequiredTask,
                CompletedTask = x.CompletedTask,
                ProgressPercentage = x.ProgressPercentage,
                ManagerName = x.ManagerWorkforceProfile != null ? x.ManagerWorkforceProfile.DisplayName : null,
                CreateDateTime = x.CreateDateTime,
                EmployeeSeparationId = x.EmployeeSeparationId,
                SeparationNumber = x.EmployeeSeparation != null ? x.EmployeeSeparation.SeparationNumber : null,
                ManagerWorkforceProfileId = x.ManagerWorkforceProfileId,
                Notes = x.Notes,
                Tasks = taskResponses
            };
        }

        public async Task<HandoverTaskResponse> AddTaskAsync(
            Guid checklistId,
            AddHandoverTaskRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var checklist = await _dbContext.WfpOffboardingChecklists
                .Include(c => c.Tasks)
                .FirstOrDefaultAsync(c => c.Id == checklistId && !c.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Berkas checklist serah terima tidak ditemukan.");

            var now = DateTime.UtcNow;
            var seq = checklist.Tasks.Count(t => !t.IsDelete);
            var taskCode = $"TSK-{seq + 1:D2}";
            while (checklist.Tasks.Any(t => t.TaskCode == taskCode && !t.IsDelete))
            {
                seq++;
                taskCode = $"TSK-{seq + 1:D2}";
            }

            var task = new WfpOffboardingTask
            {
                Id = Guid.NewGuid(),
                OffboardingChecklistId = checklistId,
                TaskCode = taskCode,
                TaskName = request.TaskName.Trim(),
                TaskCategory = string.IsNullOrWhiteSpace(request.TaskCategory) ? "WorkUnitHandover" : request.TaskCategory.Trim(),
                Description = request.Description?.Trim(),
                AssignedToWorkforceProfileId = request.AssignedToWorkforceProfileId,
                DueDate = request.DueDate,
                TaskStatus = "Pending",
                IsRequired = request.IsRequired,
                RequiresDocument = request.RequiresDocument,
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.WfpOffboardingTasks.Add(task);
            RecalculateProgress(checklist);
            checklist.UpdateDateTime = now;
            checklist.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new HandoverTaskResponse
            {
                Id = task.Id,
                OffboardingChecklistId = task.OffboardingChecklistId,
                TaskCode = task.TaskCode,
                TaskName = task.TaskName,
                TaskCategory = task.TaskCategory,
                Description = task.Description,
                AssignedToWorkforceProfileId = task.AssignedToWorkforceProfileId,
                DueDate = task.DueDate,
                TaskStatus = task.TaskStatus,
                IsRequired = task.IsRequired,
                RequiresDocument = task.RequiresDocument,
                SortOrder = task.SortOrder
            };
        }

        /// <summary>
        /// True bila <paramref name="actorUserId"/> adalah atasan utama aktif (hari ini, tanggal server) dari
        /// salah satu profil pada <paramref name="workforceProfileIds"/>.
        /// </summary>
        private async Task<bool> IsActivePrimaryManagerAsync(
            Guid actorUserId,
            IReadOnlyCollection<Guid> workforceProfileIds,
            CancellationToken cancellationToken)
        {
            if (actorUserId == Guid.Empty || workforceProfileIds.Count == 0)
            {
                return false;
            }

            var actorProfileId = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == actorUserId)
                .Select(x => x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!actorProfileId.HasValue || actorProfileId.Value == Guid.Empty)
            {
                return false;
            }

            var today = DateTime.UtcNow.Date;
            return await _dbContext.WfpManagerAssignments
                .AsNoTracking()
                .AnyAsync(x =>
                    workforceProfileIds.Contains(x.WorkforceProfileId) &&
                    x.ManagerWorkforceProfileId == actorProfileId.Value &&
                    x.IsPrimaryManager &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EffectiveStartDate <= today &&
                    (x.EffectiveEndDate == null || x.EffectiveEndDate >= today),
                    cancellationToken);
        }

        public async Task<HandoverTaskResponse> SubmitTaskAsync(
            Guid checklistId,
            Guid taskId,
            SubmitHandoverTaskRequest request,
            Guid actorUserId,
            System.Security.Claims.ClaimsPrincipal? user = null,
            CancellationToken cancellationToken = default)
        {
            var task = await _dbContext.WfpOffboardingTasks
                .Include(t => t.OffboardingChecklist)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.OffboardingChecklistId == checklistId && !t.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Item tugas serah terima tidak ditemukan.");

            if (task.TaskStatus == "Completed")
            {
                throw new InvalidOperationException("Item tugas serah terima sudah berstatus 'Completed' dan tidak dapat diserahkan kembali.");
            }

            // BE-HRD-365: hanya pemilik tugas yang menyerahkan. Tugas tanpa penanggung jawab diserahkan oleh
            // pegawai yang keluar sendiri.
            var actorProfileId = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == actorUserId)
                .Select(x => x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            var isAssignee =
                (task.AssignedToUserId.HasValue && task.AssignedToUserId.Value == actorUserId) ||
                (task.AssignedToWorkforceProfileId.HasValue && actorProfileId.HasValue && task.AssignedToWorkforceProfileId.Value == actorProfileId.Value);
            var isUnassignedOwner =
                !task.AssignedToUserId.HasValue && !task.AssignedToWorkforceProfileId.HasValue &&
                task.OffboardingChecklist != null && actorProfileId.HasValue &&
                task.OffboardingChecklist.WorkforceProfileId == actorProfileId.Value;

            if (!isAssignee && !isUnassignedOwner)
            {
                throw new UnauthorizedAccessException("Tugas serah terima hanya dapat diserahkan oleh penanggung jawab tugas tersebut.");
            }

            var now = DateTime.UtcNow;
            task.TaskStatus = "Submitted";
            task.CompletedAt = now;
            task.CompletedByUserId = actorUserId;
            if (request != null)
            {
                task.CompletionNote = request.CompletionNote?.Trim();
                if (!string.IsNullOrWhiteSpace(request.DocumentPath))
                {
                    task.DocumentPath = request.DocumentPath.Trim();
                    task.OriginalFileName = request.OriginalFileName?.Trim();
                }
            }
            task.UpdateDateTime = now;
            task.UpdateBy = actorUserId;

            if (task.OffboardingChecklist != null)
            {
                task.OffboardingChecklist.ChecklistStatus = "InProgress";
                task.OffboardingChecklist.UpdateDateTime = now;
                task.OffboardingChecklist.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new HandoverTaskResponse
            {
                Id = task.Id,
                OffboardingChecklistId = task.OffboardingChecklistId,
                TaskCode = task.TaskCode,
                TaskName = task.TaskName,
                TaskCategory = task.TaskCategory,
                Description = task.Description,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                TaskStatus = task.TaskStatus,
                IsRequired = task.IsRequired,
                RequiresDocument = task.RequiresDocument,
                DocumentPath = task.DocumentPath,
                OriginalFileName = task.OriginalFileName,
                SortOrder = task.SortOrder,
                CompletionNote = task.CompletionNote
            };
        }

        public async Task<HandoverTaskResponse> VerifyTaskAsync(
            Guid checklistId,
            Guid taskId,
            VerifyHandoverTaskRequest request,
            Guid actorUserId,
            System.Security.Claims.ClaimsPrincipal? user = null,
            CancellationToken cancellationToken = default)
        {
            var checklist = await _dbContext.WfpOffboardingChecklists
                .Include(c => c.Tasks)
                .FirstOrDefaultAsync(c => c.Id == checklistId && !c.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Berkas checklist serah terima tidak ditemukan.");

            var task = checklist.Tasks.FirstOrDefault(t => t.Id == taskId && !t.IsDelete)
                ?? throw new KeyNotFoundException("Item tugas serah terima tidak ditemukan.");

            if (!string.Equals(task.TaskStatus, "Submitted", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Hanya item tugas serah terima berstatus 'Submitted' yang dapat diverifikasi (status saat ini: '{task.TaskStatus}').");
            }

            // BE-HRD-365: penyerah tidak memverifikasi tugasnya sendiri; verifikator adalah atasan langsung aktif
            // pegawai yang keluar, atau pemegang hak VerifyAllTasks (Akses Role).
            if (task.CompletedByUserId.HasValue && task.CompletedByUserId.Value == actorUserId)
            {
                throw new UnauthorizedAccessException("Penyerah tugas tidak boleh memverifikasi tugasnya sendiri.");
            }

            var canVerifyAll = user != null && await _accessPermissionService.HasAccessAsync(
                user, "OffboardingHandoverAdministration", LifecycleExitPermissions.VerifyAllHandoverTasks);
            if (!canVerifyAll &&
                !await IsActivePrimaryManagerAsync(actorUserId, new[] { checklist.WorkforceProfileId }, cancellationToken))
            {
                throw new UnauthorizedAccessException("Tugas serah terima hanya dapat diverifikasi oleh atasan langsung pegawai yang keluar atau petugas berhak.");
            }

            var now = DateTime.UtcNow;
            if (request.IsApproved)
            {
                task.TaskStatus = "Completed";
                task.VerifiedAt = now;
                task.VerifiedByUserId = actorUserId;
            }
            else
            {
                task.TaskStatus = "Returned";
                task.VerifiedAt = now;
                task.VerifiedByUserId = actorUserId;
            }

            task.VerificationNote = request.VerificationNote?.Trim();
            task.UpdateDateTime = now;
            task.UpdateBy = actorUserId;

            RecalculateProgress(checklist);
            checklist.UpdateDateTime = now;
            checklist.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new HandoverTaskResponse
            {
                Id = task.Id,
                OffboardingChecklistId = task.OffboardingChecklistId,
                TaskCode = task.TaskCode,
                TaskName = task.TaskName,
                TaskCategory = task.TaskCategory,
                Description = task.Description,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                VerifiedAt = task.VerifiedAt,
                TaskStatus = task.TaskStatus,
                IsRequired = task.IsRequired,
                RequiresDocument = task.RequiresDocument,
                DocumentPath = task.DocumentPath,
                OriginalFileName = task.OriginalFileName,
                SortOrder = task.SortOrder,
                CompletionNote = task.CompletionNote,
                VerificationNote = task.VerificationNote
            };
        }

        public async Task<HandoverChecklistDetailResponse> CompleteChecklistAsync(
            Guid checklistId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var checklist = await _dbContext.WfpOffboardingChecklists
                .Include(c => c.Tasks)
                .Include(c => c.EmployeeSeparation)
                .FirstOrDefaultAsync(c => c.Id == checklistId && !c.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Berkas checklist serah terima tidak ditemukan.");

            var activeTasks = checklist.Tasks.Where(t => !t.IsDelete && t.IsActive).ToList();
            if (activeTasks.Count == 0)
            {
                throw new InvalidOperationException("Checklist serah terima belum memiliki item tugas sehingga belum dapat diselesaikan.");
            }

            var uncompletedRequired = activeTasks.Where(t => t.IsRequired && t.TaskStatus != "Completed").ToList();

            if (uncompletedRequired.Count > 0)
            {
                throw new InvalidOperationException($"Masih ada {uncompletedRequired.Count} item serah terima wajib yang belum selesai diverifikasi.");
            }

            var now = DateTime.UtcNow;
            checklist.ChecklistStatus = "Completed";
            checklist.ActualCompletionDate = now;
            checklist.ProgressPercentage = 100;
            checklist.UpdateDateTime = now;
            checklist.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (await GetByIdAsync(checklistId, cancellationToken))!;
        }

        private static void RecalculateProgress(WfpOffboardingChecklist checklist)
        {
            var activeTasks = checklist.Tasks.Where(t => !t.IsDelete && t.IsActive).ToList();
            checklist.TotalTask = activeTasks.Count;
            checklist.RequiredTask = activeTasks.Count(t => t.IsRequired);
            checklist.CompletedTask = activeTasks.Count(t => t.TaskStatus == "Completed");
            checklist.CompletedRequiredTask = activeTasks.Count(t => t.IsRequired && t.TaskStatus == "Completed");

            if (checklist.RequiredTask > 0)
            {
                checklist.ProgressPercentage = Math.Round((decimal)checklist.CompletedRequiredTask / checklist.RequiredTask * 100, 2);
            }
            else if (checklist.TotalTask > 0)
            {
                checklist.ProgressPercentage = Math.Round((decimal)checklist.CompletedTask / checklist.TotalTask * 100, 2);
            }
            else
            {
                checklist.ProgressPercentage = 0;
            }

            if (checklist.ProgressPercentage >= 100 && checklist.ChecklistStatus != "Completed")
            {
                checklist.ChecklistStatus = "Completed";
                checklist.ActualCompletionDate = DateTime.UtcNow;
            }
            else if (checklist.ProgressPercentage > 0 && checklist.ChecklistStatus == "Draft")
            {
                checklist.ChecklistStatus = "InProgress";
            }
        }
    }
}
