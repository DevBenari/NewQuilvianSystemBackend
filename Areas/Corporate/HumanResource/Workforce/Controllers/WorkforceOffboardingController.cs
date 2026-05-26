using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/offboarding")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Offboarding",
        AreaName = "Corporate",
        ControllerName = "WorkforceOffboarding",
        Description = "Corporate human resource workforce offboarding and exit clearance",
        SortOrder = 10
    )]
    [Tags("Corporate / Human Resource / Workforce / Offboarding")]
    public class WorkforceOffboardingController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceOffboardingController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Offboarding",
            Description = "Melihat data offboarding workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOffboarding", "Read")]
        public async Task<IActionResult> GetOffboardingChecklists(Guid workforceProfileId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.WfpOffboardingChecklists
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new WorkforceOffboardingChecklistResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    OffboardingType = x.OffboardingType,
                    EffectiveEndDate = x.EffectiveEndDate,
                    StartDate = x.StartDate,
                    CompletedDate = x.CompletedDate,
                    Status = x.Status,
                    Reason = x.Reason,
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

            var result = new WorkforceOffboardingChecklistListResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                DraftData = items.Count(x => x.Status == OffboardingStatus.Draft),
                InProgressData = items.Count(x => x.Status == OffboardingStatus.InProgress),
                CompletedData = items.Count(x => x.Status == OffboardingStatus.Completed),
                CancelledData = items.Count(x => x.Status == OffboardingStatus.Cancelled),
                Items = items
            };

            return Ok(ApiResponse<WorkforceOffboardingChecklistListResponse>.Ok(
                result,
                "Data offboarding workforce berhasil diambil."
            ));
        }

        [HttpGet("{offboardingChecklistId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Offboarding",
            Description = "Melihat detail offboarding workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOffboarding", "Read")]
        public async Task<IActionResult> GetOffboardingChecklistById(
            Guid workforceProfileId,
            Guid offboardingChecklistId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var checklist = await _dbContext.WfpOffboardingChecklists
                .AsNoTracking()
                .Where(x =>
                    x.Id == offboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new WorkforceOffboardingChecklistResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    OffboardingType = x.OffboardingType,
                    EffectiveEndDate = x.EffectiveEndDate,
                    StartDate = x.StartDate,
                    CompletedDate = x.CompletedDate,
                    Status = x.Status,
                    Reason = x.Reason,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    Tasks = x.Tasks
                        .Where(t => !t.IsDelete)
                        .OrderBy(t => t.SortOrder)
                        .ThenBy(t => t.TaskName)
                        .Select(t => new WorkforceOffboardingTaskResponse
                        {
                            Id = t.Id,
                            OffboardingChecklistId = t.OffboardingChecklistId,
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
                    "Checklist offboarding tidak ditemukan."
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

            return Ok(ApiResponse<WorkforceOffboardingChecklistResponse>.Ok(
                checklist,
                "Detail offboarding workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Offboarding",
            Description = "Membuat offboarding workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceOffboarding", "Create")]
        public async Task<IActionResult> CreateOffboardingChecklist(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceOffboardingChecklistRequest request)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            if (request.OffboardingType == OffboardingType.Unknown)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "OffboardingType wajib dipilih."
                ));
            }

            if (request.StartDate == default)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "StartDate wajib diisi."
                ));
            }

            if (request.EffectiveEndDate == default)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate wajib diisi."
                ));
            }

            if (request.EffectiveEndDate.Date < request.StartDate.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari StartDate."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Reason wajib diisi."
                ));
            }

            var hasActiveChecklist = await _dbContext.WfpOffboardingChecklists
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.Status != OffboardingStatus.Completed &&
                    x.Status != OffboardingStatus.Cancelled);

            if (hasActiveChecklist)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Masih ada checklist offboarding aktif yang belum selesai."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var checklist = new WfpOffboardingChecklist
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    OffboardingType = request.OffboardingType,
                    EffectiveEndDate = request.EffectiveEndDate.Date,
                    StartDate = request.StartDate.Date,
                    CompletedDate = null,
                    Status = OffboardingStatus.InProgress,
                    Reason = request.Reason.Trim(),
                    Notes = NormalizeNullableText(request.Notes),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.WfpOffboardingChecklists.Add(checklist);
                await _dbContext.SaveChangesAsync();

                if (request.GenerateDefaultTasks)
                {
                    var defaultTasks = BuildDefaultTasks(
                        checklist.Id,
                        profile.UserType,
                        request.OffboardingType,
                        now,
                        actorUserId
                    );

                    _dbContext.WfpOffboardingTasks.AddRange(defaultTasks);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceOffboarding.CreateOffboardingChecklist",
                    "Offboarding workforce berhasil dibuat.",
                    new
                    {
                        checklist.Id,
                        checklist.WorkforceProfileId,
                        checklist.OffboardingType,
                        checklist.Status
                    }
                );

                return await GetOffboardingChecklistById(workforceProfileId, checklist.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceOffboarding.CreateOffboardingChecklist",
                    "Gagal membuat offboarding workforce.",
                    ex,
                    new { workforceProfileId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal membuat offboarding workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpPut("{offboardingChecklistId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Offboarding",
            Description = "Mengubah offboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOffboarding", "Update")]
        public async Task<IActionResult> UpdateOffboardingChecklist(
            Guid workforceProfileId,
            Guid offboardingChecklistId,
            [FromBody] UpdateWorkforceOffboardingChecklistRequest request)
        {
            var entity = await _dbContext.WfpOffboardingChecklists
                .FirstOrDefaultAsync(x =>
                    x.Id == offboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist offboarding tidak ditemukan."
                ));
            }

            if (request.OffboardingType == OffboardingType.Unknown)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "OffboardingType wajib dipilih."
                ));
            }

            if (request.EffectiveEndDate.Date < request.StartDate.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari StartDate."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Reason wajib diisi."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.OffboardingType = request.OffboardingType;
            entity.EffectiveEndDate = request.EffectiveEndDate.Date;
            entity.StartDate = request.StartDate.Date;
            entity.Status = request.Status;
            entity.CompletedDate = request.Status == OffboardingStatus.Completed
                ? entity.CompletedDate ?? now.Date
                : null;
            entity.Reason = request.Reason.Trim();
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(entity.Id, actorUserId);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceOffboarding.UpdateOffboardingChecklist",
                "Offboarding workforce berhasil diubah.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.Status
                }
            );

            return await GetOffboardingChecklistById(workforceProfileId, entity.Id);
        }

        [HttpPatch("{offboardingChecklistId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Offboarding Status",
            Description = "Mengubah status offboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceOffboarding", "Update")]
        public async Task<IActionResult> UpdateOffboardingChecklistStatus(
            Guid workforceProfileId,
            Guid offboardingChecklistId,
            [FromBody] UpdateWorkforceOffboardingChecklistStatusRequest request)
        {
            var entity = await _dbContext.WfpOffboardingChecklists
                .FirstOrDefaultAsync(x =>
                    x.Id == offboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist offboarding tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.Status = request.Status;
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.CompletedDate = request.Status == OffboardingStatus.Completed
                ? request.CompletedDate?.Date ?? now.Date
                : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetOffboardingChecklistById(workforceProfileId, entity.Id);
        }

        [HttpDelete("{offboardingChecklistId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Offboarding",
            Description = "Menghapus offboarding workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceOffboarding", "Delete")]
        public async Task<IActionResult> DeleteOffboardingChecklist(
            Guid workforceProfileId,
            Guid offboardingChecklistId)
        {
            var entity = await _dbContext.WfpOffboardingChecklists
                .Include(x => x.Tasks)
                .FirstOrDefaultAsync(x =>
                    x.Id == offboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist offboarding tidak ditemukan."
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
                "Checklist offboarding berhasil dihapus."
            ));
        }

        [HttpPost("{offboardingChecklistId:guid}/tasks")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Offboarding Task",
            Description = "Menambah task offboarding workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceOffboarding", "Create")]
        public async Task<IActionResult> CreateTask(
            Guid workforceProfileId,
            Guid offboardingChecklistId,
            [FromBody] CreateWorkforceOffboardingTaskRequest request)
        {
            var checklist = await _dbContext.WfpOffboardingChecklists
                .FirstOrDefaultAsync(x =>
                    x.Id == offboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (checklist == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist offboarding tidak ditemukan."
                ));
            }

            var taskCode = NormalizeRequiredCode(request.TaskCode);

            if (string.IsNullOrWhiteSpace(taskCode))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TaskCode wajib diisi."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.TaskName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TaskName wajib diisi."
                ));
            }

            var taskCodeExists = await _dbContext.WfpOffboardingTasks
                .AnyAsync(x =>
                    x.OffboardingChecklistId == offboardingChecklistId &&
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

            var task = new WfpOffboardingTask
            {
                Id = Guid.NewGuid(),
                OffboardingChecklistId = offboardingChecklistId,
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

            _dbContext.WfpOffboardingTasks.Add(task);
            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(offboardingChecklistId, actorUserId);

            return await GetOffboardingChecklistById(workforceProfileId, offboardingChecklistId);
        }

        [HttpPut("{offboardingChecklistId:guid}/tasks/{taskId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Offboarding Task",
            Description = "Mengubah task offboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 7
        )]
        [AccessPermission("WorkforceOffboarding", "Update")]
        public async Task<IActionResult> UpdateTask(
            Guid workforceProfileId,
            Guid offboardingChecklistId,
            Guid taskId,
            [FromBody] UpdateWorkforceOffboardingTaskRequest request)
        {
            var checklistExists = await _dbContext.WfpOffboardingChecklists
                .AnyAsync(x =>
                    x.Id == offboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (!checklistExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist offboarding tidak ditemukan."
                ));
            }

            var task = await _dbContext.WfpOffboardingTasks
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.OffboardingChecklistId == offboardingChecklistId &&
                    !x.IsDelete);

            if (task == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Task offboarding tidak ditemukan."
                ));
            }

            var taskCode = NormalizeRequiredCode(request.TaskCode);

            if (string.IsNullOrWhiteSpace(taskCode))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TaskCode wajib diisi."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.TaskName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TaskName wajib diisi."
                ));
            }

            var duplicateExists = await _dbContext.WfpOffboardingTasks
                .AnyAsync(x =>
                    x.Id != taskId &&
                    x.OffboardingChecklistId == offboardingChecklistId &&
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

            await RecalculateChecklistStatusAsync(offboardingChecklistId, actorUserId);

            return await GetOffboardingChecklistById(workforceProfileId, offboardingChecklistId);
        }

        [HttpPatch("{offboardingChecklistId:guid}/tasks/{taskId:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Complete Workforce Offboarding Task",
            Description = "Menyelesaikan task offboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 8
        )]
        [AccessPermission("WorkforceOffboarding", "Update")]
        public async Task<IActionResult> CompleteTask(
            Guid workforceProfileId,
            Guid offboardingChecklistId,
            Guid taskId,
            [FromBody] CompleteWorkforceOffboardingTaskRequest request)
        {
            var checklistExists = await _dbContext.WfpOffboardingChecklists
                .AnyAsync(x =>
                    x.Id == offboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (!checklistExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist offboarding tidak ditemukan."
                ));
            }

            var task = await _dbContext.WfpOffboardingTasks
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.OffboardingChecklistId == offboardingChecklistId &&
                    !x.IsDelete);

            if (task == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Task offboarding tidak ditemukan."
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

            await RecalculateChecklistStatusAsync(offboardingChecklistId, actorUserId);

            return await GetOffboardingChecklistById(workforceProfileId, offboardingChecklistId);
        }

        [HttpPatch("{offboardingChecklistId:guid}/tasks/{taskId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Offboarding Task Status",
            Description = "Mengubah status aktif task offboarding workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 9
        )]
        [AccessPermission("WorkforceOffboarding", "Update")]
        public async Task<IActionResult> UpdateTaskStatus(
            Guid workforceProfileId,
            Guid offboardingChecklistId,
            Guid taskId,
            [FromBody] UpdateWorkforceOffboardingTaskStatusRequest request)
        {
            var checklistExists = await _dbContext.WfpOffboardingChecklists
                .AnyAsync(x =>
                    x.Id == offboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (!checklistExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist offboarding tidak ditemukan."
                ));
            }

            var task = await _dbContext.WfpOffboardingTasks
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.OffboardingChecklistId == offboardingChecklistId &&
                    !x.IsDelete);

            if (task == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Task offboarding tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            task.IsActive = request.IsActive;
            task.Notes = NormalizeNullableText(request.Notes) ?? task.Notes;
            task.UpdateDateTime = now;
            task.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(offboardingChecklistId, actorUserId);

            return await GetOffboardingChecklistById(workforceProfileId, offboardingChecklistId);
        }

        [HttpDelete("{offboardingChecklistId:guid}/tasks/{taskId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOffboardingChecklistResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Offboarding Task",
            Description = "Menghapus task offboarding workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 10
        )]
        [AccessPermission("WorkforceOffboarding", "Delete")]
        public async Task<IActionResult> DeleteTask(
            Guid workforceProfileId,
            Guid offboardingChecklistId,
            Guid taskId)
        {
            var checklistExists = await _dbContext.WfpOffboardingChecklists
                .AnyAsync(x =>
                    x.Id == offboardingChecklistId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (!checklistExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Checklist offboarding tidak ditemukan."
                ));
            }

            var task = await _dbContext.WfpOffboardingTasks
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.OffboardingChecklistId == offboardingChecklistId &&
                    !x.IsDelete);

            if (task == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Task offboarding tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            task.IsDelete = true;
            task.DeleteDateTime = now;
            task.DeleteBy = actorUserId;
            task.IsActive = false;

            await _dbContext.SaveChangesAsync();

            await RecalculateChecklistStatusAsync(offboardingChecklistId, actorUserId);

            return await GetOffboardingChecklistById(workforceProfileId, offboardingChecklistId);
        }

        private async Task RecalculateChecklistStatusAsync(Guid offboardingChecklistId, Guid actorUserId)
        {
            var checklist = await _dbContext.WfpOffboardingChecklists
                .FirstOrDefaultAsync(x => x.Id == offboardingChecklistId && !x.IsDelete);

            if (checklist == null)
            {
                return;
            }

            if (checklist.Status == OffboardingStatus.Cancelled ||
                checklist.Status == OffboardingStatus.OnHold)
            {
                return;
            }

            var now = DateTime.UtcNow;

            var requiredTasks = await _dbContext.WfpOffboardingTasks
                .Where(x =>
                    x.OffboardingChecklistId == offboardingChecklistId &&
                    x.IsRequired &&
                    x.IsActive &&
                    !x.IsDelete)
                .ToListAsync();

            if (!requiredTasks.Any())
            {
                checklist.Status = OffboardingStatus.InProgress;
                checklist.CompletedDate = null;
            }
            else if (requiredTasks.All(x => x.IsCompleted))
            {
                checklist.Status = OffboardingStatus.Completed;
                checklist.CompletedDate = checklist.CompletedDate ?? now.Date;
            }
            else
            {
                checklist.Status = OffboardingStatus.InProgress;
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

        private static List<WfpOffboardingTask> BuildDefaultTasks(
            Guid offboardingChecklistId,
            UserType userType,
            OffboardingType offboardingType,
            DateTime now,
            Guid actorUserId)
        {
            var seedTasks = new List<DefaultOffboardingTaskSeed>
            {
                new("ACCOUNT_REVOCATION", "Nonaktifkan atau cabut akses akun login", OffboardingTaskCategory.AccountRevocation, true, 10),
                new("FINGERPRINT_REVOCATION", "Cabut akses fingerprint", OffboardingTaskCategory.FingerprintRevocation, false, 20),
                new("ACCESS_CARD_RETURN", "Pengembalian access card / ID card", OffboardingTaskCategory.AccessCardReturn, true, 30),
                new("ASSET_RETURN", "Pengembalian asset kerja", OffboardingTaskCategory.AssetReturn, true, 40),
                new("DOCUMENT_HANDOVER", "Serah terima dokumen atau pekerjaan", OffboardingTaskCategory.DocumentHandover, true, 50),
                new("DEPARTMENT_CLEARANCE", "Clearance dari department / unit kerja", OffboardingTaskCategory.DepartmentClearance, true, 60)
            };

            if (userType == UserType.Employee)
            {
                seedTasks.Add(new("PAYROLL_CLEARANCE", "Clearance payroll terakhir", OffboardingTaskCategory.PayrollClearance, true, 70));
                seedTasks.Add(new("FINANCE_CLEARANCE", "Clearance pinjaman / tanggungan finance", OffboardingTaskCategory.FinanceClearance, true, 80));
            }

            if (userType == UserType.PermanentDoctor ||
                userType == UserType.GuestDoctor)
            {
                seedTasks.Add(new("MEDICAL_CREDENTIAL_CLOSE", "Penutupan credential / clinical privilege", OffboardingTaskCategory.MedicalCredentialClose, true, 70));
                seedTasks.Add(new("SCHEDULE_APPOINTMENT_CLOSE", "Pastikan jadwal praktik dan appointment ditutup", OffboardingTaskCategory.DepartmentClearance, true, 80));
            }

            if (userType == UserType.ExternalUser)
            {
                seedTasks.Add(new("EXTERNAL_ACCESS_CLOSE", "Validasi seluruh akses external user sudah ditutup", OffboardingTaskCategory.AccountRevocation, true, 70));
            }

            if (offboardingType == OffboardingType.Termination)
            {
                seedTasks.Add(new("TERMINATION_DOCUMENT", "Lengkapi dokumen administrasi termination", OffboardingTaskCategory.DocumentHandover, true, 90));
            }

            return seedTasks
                .OrderBy(x => x.SortOrder)
                .Select(x => new WfpOffboardingTask
                {
                    Id = Guid.NewGuid(),
                    OffboardingChecklistId = offboardingChecklistId,
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
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
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

        private record DefaultOffboardingTaskSeed(
            string TaskCode,
            string TaskName,
            OffboardingTaskCategory TaskCategory,
            bool IsRequired,
            int SortOrder
        );
    }
}