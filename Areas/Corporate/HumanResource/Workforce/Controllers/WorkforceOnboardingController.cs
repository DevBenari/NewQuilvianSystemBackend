using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/onboarding")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Onboarding",
        AreaName = "Corporate",
        ControllerName = "WorkforceOnboarding",
        Description = "Corporate human resource workforce onboarding",
        SortOrder = 9
    )]
    [Tags("Corporate / Human Resource / Workforce / Onboarding")]
    public class WorkforceOnboardingController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceOnboardingController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Onboarding",
            Description = "Melihat data onboarding workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOnboarding", "Read")]
        public async Task<IActionResult> GetOnboardingChecklists(Guid workforceProfileId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = _dbContext.WfpOnboardingChecklists
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new WorkforceOnboardingChecklistResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    OnboardingType = x.OnboardingType,
                    StartDate = x.StartDate,
                    TargetCompletionDate = x.TargetCompletionDate,
                    CompletedDate = x.CompletedDate,
                    Status = x.Status,
                    AssignedToUserId = x.AssignedToUserId,
                    AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.DisplayName : null,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    TotalTask = x.Tasks.Count(t => !t.IsDelete),
                    RequiredTask = x.Tasks.Count(t => t.IsRequired && !t.IsDelete),
                    CompletedTask = x.Tasks.Count(t => t.IsCompleted && !t.IsDelete),
                    PendingTask = x.Tasks.Count(t => !t.IsCompleted && !t.IsDelete),
                    RequiredPendingTask = x.Tasks.Count(t => t.IsRequired && !t.IsCompleted && !t.IsDelete),
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            foreach (var item in items)
            {
                item.CompletionPercentage = CalculateCompletionPercentage(
                    item.TotalTask,
                    item.CompletedTask
                );
            }

            var result = new WorkforceOnboardingChecklistListResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                DraftData = items.Count(x => x.Status == OnboardingStatus.Draft),
                InProgressData = items.Count(x => x.Status == OnboardingStatus.InProgress),
                CompletedData = items.Count(x => x.Status == OnboardingStatus.Completed),
                Items = items
            };

            return Ok(ApiResponse<WorkforceOnboardingChecklistListResponse>.Ok(
                result,
                "Data onboarding workforce berhasil diambil."
            ));
        }

        [HttpGet("{onboardingChecklistId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Onboarding",
            Description = "Melihat detail onboarding workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOnboarding", "Read")]
        public async Task<IActionResult> GetOnboardingChecklistById(
            Guid workforceProfileId,
            Guid onboardingChecklistId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var checklist = await _dbContext.WfpOnboardingChecklists
                .AsNoTracking()
                .Where(x =>
                    x.Id == onboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new WorkforceOnboardingChecklistResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    OnboardingType = x.OnboardingType,
                    StartDate = x.StartDate,
                    TargetCompletionDate = x.TargetCompletionDate,
                    CompletedDate = x.CompletedDate,
                    Status = x.Status,
                    AssignedToUserId = x.AssignedToUserId,
                    AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.DisplayName : null,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    Tasks = x.Tasks
                        .Where(t => !t.IsDelete)
                        .OrderBy(t => t.SortOrder)
                        .ThenBy(t => t.TaskName)
                        .Select(t => new WorkforceOnboardingTaskResponse
                        {
                            Id = t.Id,
                            OnboardingChecklistId = t.OnboardingChecklistId,
                            TaskCode = t.TaskCode,
                            TaskName = t.TaskName,
                            TaskCategory = t.TaskCategory,
                            IsRequired = t.IsRequired,
                            IsCompleted = t.IsCompleted,
                            CompletedAt = t.CompletedAt,
                            CompletedByUserId = t.CompletedByUserId,
                            CompletedByUserName = t.CompletedByUser != null ? t.CompletedByUser.DisplayName : null,
                            Notes = t.Notes,
                            SortOrder = t.SortOrder,
                            IsActive = t.IsActive,
                            CreateDateTime = t.CreateDateTime
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (checklist == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist onboarding tidak ditemukan."
                ));
            }

            checklist.TotalTask = checklist.Tasks.Count;
            checklist.RequiredTask = checklist.Tasks.Count(x => x.IsRequired);
            checklist.CompletedTask = checklist.Tasks.Count(x => x.IsCompleted);
            checklist.PendingTask = checklist.Tasks.Count(x => !x.IsCompleted);
            checklist.RequiredPendingTask = checklist.Tasks.Count(x => x.IsRequired && !x.IsCompleted);
            checklist.CompletionPercentage = CalculateCompletionPercentage(
                checklist.TotalTask,
                checklist.CompletedTask
            );

            return Ok(ApiResponse<WorkforceOnboardingChecklistResponse>.Ok(
                checklist,
                "Detail onboarding workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Onboarding",
            Description = "Membuat onboarding workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceOnboarding", "Create")]
        public async Task<IActionResult> CreateOnboardingChecklist(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceOnboardingChecklistRequest request)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            if (request.OnboardingType == OnboardingType.Unknown)
            {
                request.OnboardingType = MapUserTypeToOnboardingType(profile.UserType);
            }

            if (request.StartDate == default)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "StartDate wajib diisi."
                ));
            }

            if (request.TargetCompletionDate == default)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TargetCompletionDate wajib diisi."
                ));
            }

            if (request.TargetCompletionDate.Date < request.StartDate.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TargetCompletionDate tidak boleh lebih kecil dari StartDate."
                ));
            }

            if (request.AssignedToUserId.HasValue)
            {
                var assignedUserExists = await _dbContext.Users.AnyAsync(x =>
                    x.Id == request.AssignedToUserId.Value &&
                    x.IsActive);

                if (!assignedUserExists)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Assigned user tidak ditemukan atau tidak aktif."
                    ));
                }
            }

            var hasActiveChecklist = await _dbContext.WfpOnboardingChecklists
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.Status != OnboardingStatus.Completed &&
                    x.Status != OnboardingStatus.Cancelled);

            if (hasActiveChecklist)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Masih ada checklist onboarding aktif yang belum selesai."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var checklist = new WfpOnboardingChecklist
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    OnboardingType = request.OnboardingType,
                    StartDate = request.StartDate.Date,
                    TargetCompletionDate = request.TargetCompletionDate.Date,
                    CompletedDate = null,
                    Status = OnboardingStatus.InProgress,
                    AssignedToUserId = NormalizeNullableGuid(request.AssignedToUserId),
                    Notes = NormalizeNullableText(request.Notes),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.WfpOnboardingChecklists.Add(checklist);
                await _dbContext.SaveChangesAsync();

                if (request.GenerateDefaultTasks)
                {
                    var defaultTasks = BuildDefaultTasks(checklist.Id, request.OnboardingType, now, actorUserId);

                    _dbContext.WfpOnboardingTasks.AddRange(defaultTasks);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceOnboarding.CreateOnboardingChecklist",
                    "Onboarding workforce berhasil dibuat.",
                    new
                    {
                        checklist.Id,
                        checklist.WorkforceProfileId,
                        checklist.OnboardingType,
                        checklist.Status
                    }
                );

                return await GetOnboardingChecklistById(workforceProfileId, checklist.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceOnboarding.CreateOnboardingChecklist",
                    "Gagal membuat onboarding workforce.",
                    ex,
                    new { workforceProfileId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal membuat onboarding workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpPut("{onboardingChecklistId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Onboarding",
            Description = "Mengubah onboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOnboarding", "Update")]
        public async Task<IActionResult> UpdateOnboardingChecklist(
            Guid workforceProfileId,
            Guid onboardingChecklistId,
            [FromBody] UpdateWorkforceOnboardingChecklistRequest request)
        {
            var entity = await _dbContext.WfpOnboardingChecklists
                .FirstOrDefaultAsync(x =>
                    x.Id == onboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist onboarding tidak ditemukan."
                ));
            }

            if (request.TargetCompletionDate.Date < request.StartDate.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TargetCompletionDate tidak boleh lebih kecil dari StartDate."
                ));
            }

            if (request.AssignedToUserId.HasValue)
            {
                var assignedUserExists = await _dbContext.Users.AnyAsync(x =>
                    x.Id == request.AssignedToUserId.Value &&
                    x.IsActive);

                if (!assignedUserExists)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Assigned user tidak ditemukan atau tidak aktif."
                    ));
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.OnboardingType = request.OnboardingType;
            entity.StartDate = request.StartDate.Date;
            entity.TargetCompletionDate = request.TargetCompletionDate.Date;
            entity.Status = request.Status;
            entity.CompletedDate = request.Status == OnboardingStatus.Completed
                ? entity.CompletedDate ?? now.Date
                : null;
            entity.AssignedToUserId = NormalizeNullableGuid(request.AssignedToUserId);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(entity.Id, actorUserId);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceOnboarding.UpdateOnboardingChecklist",
                "Onboarding workforce berhasil diubah.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.Status
                }
            );

            return await GetOnboardingChecklistById(workforceProfileId, entity.Id);
        }

        [HttpPatch("{onboardingChecklistId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Onboarding Status",
            Description = "Mengubah status onboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceOnboarding", "Update")]
        public async Task<IActionResult> UpdateOnboardingChecklistStatus(
            Guid workforceProfileId,
            Guid onboardingChecklistId,
            [FromBody] UpdateWorkforceOnboardingChecklistStatusRequest request)
        {
            var entity = await _dbContext.WfpOnboardingChecklists
                .FirstOrDefaultAsync(x =>
                    x.Id == onboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist onboarding tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.Status = request.Status;
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.CompletedDate = request.Status == OnboardingStatus.Completed
                ? request.CompletedDate?.Date ?? now.Date
                : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetOnboardingChecklistById(workforceProfileId, entity.Id);
        }

        [HttpDelete("{onboardingChecklistId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Onboarding",
            Description = "Menghapus onboarding workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceOnboarding", "Delete")]
        public async Task<IActionResult> DeleteOnboardingChecklist(
            Guid workforceProfileId,
            Guid onboardingChecklistId)
        {
            var entity = await _dbContext.WfpOnboardingChecklists
                .Include(x => x.Tasks)
                .FirstOrDefaultAsync(x =>
                    x.Id == onboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist onboarding tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;

            foreach (var task in entity.Tasks.Where(x => !x.IsDelete))
            {
                task.IsDelete = true;
                task.DeleteDateTime = now;
                task.DeleteBy = actorUserId;
                task.IsActive = false;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Checklist onboarding berhasil dihapus."
            ));
        }

        [HttpPost("{onboardingChecklistId:guid}/tasks")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Onboarding Task",
            Description = "Menambah task onboarding workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceOnboarding", "Create")]
        public async Task<IActionResult> CreateTask(
            Guid workforceProfileId,
            Guid onboardingChecklistId,
            [FromBody] CreateWorkforceOnboardingTaskRequest request)
        {
            var checklist = await _dbContext.WfpOnboardingChecklists
                .FirstOrDefaultAsync(x =>
                    x.Id == onboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (checklist == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist onboarding tidak ditemukan."
                ));
            }

            var taskCode = NormalizeRequiredCode(request.TaskCode);

            var taskCodeExists = await _dbContext.WfpOnboardingTasks
                .AnyAsync(x =>
                    x.OnboardingChecklistId == onboardingChecklistId &&
                    x.TaskCode.ToLower() == taskCode.ToLower() &&
                    !x.IsDelete);

            if (taskCodeExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TaskCode sudah digunakan pada checklist ini."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var task = new WfpOnboardingTask
            {
                Id = Guid.NewGuid(),
                OnboardingChecklistId = onboardingChecklistId,
                TaskCode = taskCode,
                TaskName = request.TaskName.Trim(),
                TaskCategory = request.TaskCategory,
                IsRequired = request.IsRequired,
                IsCompleted = request.IsCompleted,
                CompletedAt = request.IsCompleted ? now : null,
                CompletedByUserId = request.IsCompleted ? actorUserId : null,
                Notes = NormalizeNullableText(request.Notes),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.WfpOnboardingTasks.Add(task);
            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(onboardingChecklistId, actorUserId);

            return await GetOnboardingChecklistById(workforceProfileId, onboardingChecklistId);
        }

        [HttpPut("{onboardingChecklistId:guid}/tasks/{taskId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Onboarding Task",
            Description = "Mengubah task onboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 7
        )]
        [AccessPermission("WorkforceOnboarding", "Update")]
        public async Task<IActionResult> UpdateTask(
            Guid workforceProfileId,
            Guid onboardingChecklistId,
            Guid taskId,
            [FromBody] UpdateWorkforceOnboardingTaskRequest request)
        {
            var checklistExists = await _dbContext.WfpOnboardingChecklists
                .AnyAsync(x =>
                    x.Id == onboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (!checklistExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist onboarding tidak ditemukan."
                ));
            }

            var task = await _dbContext.WfpOnboardingTasks
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.OnboardingChecklistId == onboardingChecklistId &&
                    !x.IsDelete);

            if (task == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Task onboarding tidak ditemukan."
                ));
            }

            var taskCode = NormalizeRequiredCode(request.TaskCode);

            var duplicateExists = await _dbContext.WfpOnboardingTasks
                .AnyAsync(x =>
                    x.Id != taskId &&
                    x.OnboardingChecklistId == onboardingChecklistId &&
                    x.TaskCode.ToLower() == taskCode.ToLower() &&
                    !x.IsDelete);

            if (duplicateExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TaskCode sudah digunakan pada checklist ini."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            task.TaskCode = taskCode;
            task.TaskName = request.TaskName.Trim();
            task.TaskCategory = request.TaskCategory;
            task.IsRequired = request.IsRequired;
            task.Notes = NormalizeNullableText(request.Notes);
            task.SortOrder = request.SortOrder;
            task.IsActive = request.IsActive;
            task.UpdateDateTime = now;
            task.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(onboardingChecklistId, actorUserId);

            return await GetOnboardingChecklistById(workforceProfileId, onboardingChecklistId);
        }

        [HttpPatch("{onboardingChecklistId:guid}/tasks/{taskId:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Complete Workforce Onboarding Task",
            Description = "Menyelesaikan task onboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 8
        )]
        [AccessPermission("WorkforceOnboarding", "Update")]
        public async Task<IActionResult> CompleteTask(
            Guid workforceProfileId,
            Guid onboardingChecklistId,
            Guid taskId,
            [FromBody] CompleteWorkforceOnboardingTaskRequest request)
        {
            var checklistExists = await _dbContext.WfpOnboardingChecklists
                .AnyAsync(x =>
                    x.Id == onboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (!checklistExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist onboarding tidak ditemukan."
                ));
            }

            var task = await _dbContext.WfpOnboardingTasks
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.OnboardingChecklistId == onboardingChecklistId &&
                    !x.IsDelete);

            if (task == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Task onboarding tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            task.IsCompleted = request.IsCompleted;
            task.CompletedAt = request.IsCompleted ? now : null;
            task.CompletedByUserId = request.IsCompleted ? actorUserId : null;
            task.Notes = NormalizeNullableText(request.Notes) ?? task.Notes;
            task.UpdateDateTime = now;
            task.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(onboardingChecklistId, actorUserId);

            return await GetOnboardingChecklistById(workforceProfileId, onboardingChecklistId);
        }

        [HttpPatch("{onboardingChecklistId:guid}/tasks/{taskId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Onboarding Task Status",
            Description = "Mengubah status aktif task onboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 9
        )]
        [AccessPermission("WorkforceOnboarding", "Update")]
        public async Task<IActionResult> UpdateTaskStatus(
            Guid workforceProfileId,
            Guid onboardingChecklistId,
            Guid taskId,
            [FromBody] UpdateWorkforceOnboardingTaskStatusRequest request)
        {
            var checklistExists = await _dbContext.WfpOnboardingChecklists
                .AnyAsync(x =>
                    x.Id == onboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (!checklistExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist onboarding tidak ditemukan."
                ));
            }

            var task = await _dbContext.WfpOnboardingTasks
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.OnboardingChecklistId == onboardingChecklistId &&
                    !x.IsDelete);

            if (task == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Task onboarding tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            task.IsActive = request.IsActive;
            task.Notes = NormalizeNullableText(request.Notes) ?? task.Notes;
            task.UpdateDateTime = now;
            task.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(onboardingChecklistId, actorUserId);

            return await GetOnboardingChecklistById(workforceProfileId, onboardingChecklistId);
        }

        [HttpDelete("{onboardingChecklistId:guid}/tasks/{taskId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOnboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Onboarding Task",
            Description = "Menghapus task onboarding workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 10
        )]
        [AccessPermission("WorkforceOnboarding", "Delete")]
        public async Task<IActionResult> DeleteTask(
            Guid workforceProfileId,
            Guid onboardingChecklistId,
            Guid taskId)
        {
            var checklistExists = await _dbContext.WfpOnboardingChecklists
                .AnyAsync(x =>
                    x.Id == onboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (!checklistExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist onboarding tidak ditemukan."
                ));
            }

            var task = await _dbContext.WfpOnboardingTasks
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.OnboardingChecklistId == onboardingChecklistId &&
                    !x.IsDelete);

            if (task == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Task onboarding tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            task.IsDelete = true;
            task.DeleteDateTime = now;
            task.DeleteBy = actorUserId;
            task.IsActive = false;

            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(onboardingChecklistId, actorUserId);

            return await GetOnboardingChecklistById(workforceProfileId, onboardingChecklistId);
        }

        private async Task RecalculateChecklistStatusAsync(Guid onboardingChecklistId, Guid actorUserId)
        {
            var checklist = await _dbContext.WfpOnboardingChecklists
                .FirstOrDefaultAsync(x => x.Id == onboardingChecklistId && !x.IsDelete);

            if (checklist == null)
            {
                return;
            }

            if (checklist.Status == OnboardingStatus.Cancelled ||
                checklist.Status == OnboardingStatus.OnHold)
            {
                return;
            }

            var now = DateTime.UtcNow;

            var requiredTasks = await _dbContext.WfpOnboardingTasks
                .Where(x =>
                    x.OnboardingChecklistId == onboardingChecklistId &&
                    x.IsRequired &&
                    x.IsActive &&
                    !x.IsDelete)
                .ToListAsync();

            if (!requiredTasks.Any())
            {
                checklist.Status = OnboardingStatus.InProgress;
                checklist.CompletedDate = null;
            }
            else if (requiredTasks.All(x => x.IsCompleted))
            {
                checklist.Status = OnboardingStatus.Completed;
                checklist.CompletedDate = checklist.CompletedDate ?? now.Date;
            }
            else
            {
                checklist.Status = OnboardingStatus.InProgress;
                checklist.CompletedDate = null;
            }

            checklist.UpdateDateTime = now;
            checklist.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
        }

        private async Task<WorkforceProfileHeader?> GetWorkforceProfileHeaderAsync(Guid workforceProfileId)
        {
            return await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => new WorkforceProfileHeader
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName,
                    UserType = x.UserType
                })
                .FirstOrDefaultAsync();
        }

        private static List<WfpOnboardingTask> BuildDefaultTasks(
            Guid onboardingChecklistId,
            OnboardingType onboardingType,
            DateTime now,
            Guid actorUserId)
        {
            var seedTasks = new List<DefaultOnboardingTaskSeed>
            {
                new("DOCUMENT_BASIC", "Lengkapi dokumen identitas", OnboardingTaskCategory.Document, true, 10),
                new("ACCOUNT_CREATE", "Buat atau verifikasi akun login", OnboardingTaskCategory.Account, true, 20),
                new("FINGERPRINT_REGISTER", "Registrasi fingerprint", OnboardingTaskCategory.Fingerprint, false, 30),
                new("DEPARTMENT_ASSIGNMENT", "Pastikan department dan posisi utama sudah sesuai", OnboardingTaskCategory.DepartmentAssignment, true, 40),
                new("ORIENTATION_HR", "Orientasi HR", OnboardingTaskCategory.Orientation, true, 50),
                new("HEALTH_CHECK", "Pemeriksaan kesehatan / fit to work", OnboardingTaskCategory.HealthCheck, false, 60),
                new("TRAINING_MANDATORY", "Pelatihan mandatory awal", OnboardingTaskCategory.Training, false, 70)
            };

            if (onboardingType == OnboardingType.Employee)
            {
                seedTasks.Add(new("BANK_PAYROLL", "Lengkapi rekening bank dan payroll profile", OnboardingTaskCategory.Payroll, true, 80));
                seedTasks.Add(new("UNIFORM", "Serah terima seragam / atribut kerja", OnboardingTaskCategory.Uniform, false, 90));
            }

            if (onboardingType == OnboardingType.PermanentDoctor ||
                onboardingType == OnboardingType.GuestDoctor)
            {
                seedTasks.Add(new("CREDENTIAL_LICENSE", "Lengkapi credential license dokter", OnboardingTaskCategory.Credentialing, true, 80));
                seedTasks.Add(new("CLINICAL_PRIVILEGE", "Proses clinical privilege", OnboardingTaskCategory.Credentialing, true, 90));
            }

            if (onboardingType == OnboardingType.ExternalUser)
            {
                seedTasks.Add(new("ACCESS_PURPOSE", "Validasi tujuan dan masa akses external user", OnboardingTaskCategory.Account, true, 80));
            }

            return seedTasks
                .OrderBy(x => x.SortOrder)
                .Select(x => new WfpOnboardingTask
                {
                    Id = Guid.NewGuid(),
                    OnboardingChecklistId = onboardingChecklistId,
                    TaskCode = x.TaskCode,
                    TaskName = x.TaskName,
                    TaskCategory = x.TaskCategory,
                    IsRequired = x.IsRequired,
                    IsCompleted = false,
                    CompletedAt = null,
                    CompletedByUserId = null,
                    Notes = null,
                    SortOrder = x.SortOrder,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                })
                .ToList();
        }

        private static OnboardingType MapUserTypeToOnboardingType(UserType userType)
        {
            return userType switch
            {
                UserType.Employee => OnboardingType.Employee,
                UserType.PermanentDoctor => OnboardingType.PermanentDoctor,
                UserType.GuestDoctor => OnboardingType.GuestDoctor,
                UserType.ExternalUser => OnboardingType.ExternalUser,
                _ => OnboardingType.Unknown
            };
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string NormalizeRequiredCode(string value)
        {
            return value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
        }

        private static decimal CalculateCompletionPercentage(int totalTask, int completedTask)
        {
            if (totalTask <= 0)
            {
                return 0;
            }

            return Math.Round((decimal)completedTask / totalTask * 100, 2);
        }

        private class WorkforceProfileHeader
        {
            public Guid Id { get; set; }

            public string ProfileCode { get; set; } = string.Empty;

            public string DisplayName { get; set; } = string.Empty;

            public UserType UserType { get; set; }
        }

        private record DefaultOnboardingTaskSeed(
            string TaskCode,
            string TaskName,
            OnboardingTaskCategory TaskCategory,
            bool IsRequired,
            int SortOrder
        );
    }
}