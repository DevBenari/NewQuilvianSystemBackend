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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Employment Contract History",
        AreaName = "Corporate",
        ControllerName = "WorkforceEmploymentContractHistory",
        Description = "Corporate human resource workforce employment and contract history",
        SortOrder = 11
    )]
    [Tags("Corporate / Human Resource / Workforce / Employment Contract History")]
    public class WorkforceEmploymentContractHistoryController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceEmploymentContractHistoryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("employment-histories")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Employment History",
            Description = "Melihat data employment history workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Read")]
        public async Task<IActionResult> GetEmploymentHistories(Guid workforceProfileId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.WfpEmploymentHistories
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.EffectiveDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new WorkforceEmploymentHistoryResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    HistoryType = x.HistoryType,

                    OldDepartmentId = x.OldDepartmentId,
                    OldDepartmentCode = x.OldDepartment != null ? x.OldDepartment.DepartmentCode : null,
                    OldDepartmentName = x.OldDepartment != null ? x.OldDepartment.DepartmentName : null,

                    NewDepartmentId = x.NewDepartmentId,
                    NewDepartmentCode = x.NewDepartment != null ? x.NewDepartment.DepartmentCode : null,
                    NewDepartmentName = x.NewDepartment != null ? x.NewDepartment.DepartmentName : null,

                    OldPositionId = x.OldPositionId,
                    OldPositionCode = x.OldPosition != null ? x.OldPosition.PositionCode : null,
                    OldPositionName = x.OldPosition != null ? x.OldPosition.PositionName : null,

                    NewPositionId = x.NewPositionId,
                    NewPositionCode = x.NewPosition != null ? x.NewPosition.PositionCode : null,
                    NewPositionName = x.NewPosition != null ? x.NewPosition.PositionName : null,

                    OldStatus = x.OldStatus,
                    NewStatus = x.NewStatus,
                    EffectiveDate = x.EffectiveDate,
                    EndDate = x.EndDate,
                    Description = x.Description,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                    ApprovedAt = x.ApprovedAt,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceEmploymentHistoryListResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                JoinData = items.Count(x => x.HistoryType == EmploymentHistoryType.Join),
                MutationData = items.Count(x =>
                    x.HistoryType == EmploymentHistoryType.Mutation ||
                    x.HistoryType == EmploymentHistoryType.DepartmentTransfer),
                PromotionData = items.Count(x => x.HistoryType == EmploymentHistoryType.Promotion),
                StatusChangeData = items.Count(x => x.HistoryType == EmploymentHistoryType.StatusChange),
                ResignOrTerminationData = items.Count(x =>
                    x.HistoryType == EmploymentHistoryType.Resign ||
                    x.HistoryType == EmploymentHistoryType.Termination ||
                    x.HistoryType == EmploymentHistoryType.Retirement ||
                    x.HistoryType == EmploymentHistoryType.ContractEnded),
                Items = items
            };

            return Ok(ApiResponse<WorkforceEmploymentHistoryListResponse>.Ok(
                result,
                "Data employment history workforce berhasil diambil."
            ));
        }

        [HttpGet("employment-histories/{historyId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Employment History",
            Description = "Melihat detail employment history workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Read")]
        public async Task<IActionResult> GetEmploymentHistoryById(
            Guid workforceProfileId,
            Guid historyId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var data = await _dbContext.WfpEmploymentHistories
                .AsNoTracking()
                .Where(x =>
                    x.Id == historyId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new WorkforceEmploymentHistoryResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    HistoryType = x.HistoryType,

                    OldDepartmentId = x.OldDepartmentId,
                    OldDepartmentCode = x.OldDepartment != null ? x.OldDepartment.DepartmentCode : null,
                    OldDepartmentName = x.OldDepartment != null ? x.OldDepartment.DepartmentName : null,

                    NewDepartmentId = x.NewDepartmentId,
                    NewDepartmentCode = x.NewDepartment != null ? x.NewDepartment.DepartmentCode : null,
                    NewDepartmentName = x.NewDepartment != null ? x.NewDepartment.DepartmentName : null,

                    OldPositionId = x.OldPositionId,
                    OldPositionCode = x.OldPosition != null ? x.OldPosition.PositionCode : null,
                    OldPositionName = x.OldPosition != null ? x.OldPosition.PositionName : null,

                    NewPositionId = x.NewPositionId,
                    NewPositionCode = x.NewPosition != null ? x.NewPosition.PositionCode : null,
                    NewPositionName = x.NewPosition != null ? x.NewPosition.PositionName : null,

                    OldStatus = x.OldStatus,
                    NewStatus = x.NewStatus,
                    EffectiveDate = x.EffectiveDate,
                    EndDate = x.EndDate,
                    Description = x.Description,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                    ApprovedAt = x.ApprovedAt,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employment history tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceEmploymentHistoryResponse>.Ok(
                data,
                "Detail employment history workforce berhasil diambil."
            ));
        }

        [HttpPost("employment-histories")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Employment History",
            Description = "Membuat employment history workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Create")]
        public async Task<IActionResult> CreateEmploymentHistory(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceEmploymentHistoryRequest request)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateEmploymentHistoryRequestAsync(
                request.HistoryType,
                request.OldDepartmentId,
                request.NewDepartmentId,
                request.OldPositionId,
                request.NewPositionId,
                request.ApprovedByUserId,
                request.EffectiveDate,
                request.EndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpEmploymentHistory
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                HistoryType = request.HistoryType,
                OldDepartmentId = NormalizeNullableGuid(request.OldDepartmentId),
                NewDepartmentId = NormalizeNullableGuid(request.NewDepartmentId),
                OldPositionId = NormalizeNullableGuid(request.OldPositionId),
                NewPositionId = NormalizeNullableGuid(request.NewPositionId),
                OldStatus = NormalizeNullableText(request.OldStatus),
                NewStatus = NormalizeNullableText(request.NewStatus),
                EffectiveDate = request.EffectiveDate.Date,
                EndDate = request.EndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                ApprovedByUserId = NormalizeNullableGuid(request.ApprovedByUserId),
                ApprovedAt = request.ApprovedAt,
                FilePath = NormalizeNullableText(request.FilePath),
                FileContentType = NormalizeNullableText(request.FileContentType),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.WfpEmploymentHistories.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEmploymentContractHistory.CreateEmploymentHistory",
                "Employment history workforce berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.HistoryType
                }
            );

            return await GetEmploymentHistoryById(workforceProfileId, entity.Id);
        }

        [HttpPut("employment-histories/{historyId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Employment History",
            Description = "Mengubah employment history workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Update")]
        public async Task<IActionResult> UpdateEmploymentHistory(
            Guid workforceProfileId,
            Guid historyId,
            [FromBody] UpdateWorkforceEmploymentHistoryRequest request)
        {
            var entity = await _dbContext.WfpEmploymentHistories
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employment history tidak ditemukan."
                ));
            }

            var validation = await ValidateEmploymentHistoryRequestAsync(
                request.HistoryType,
                request.OldDepartmentId,
                request.NewDepartmentId,
                request.OldPositionId,
                request.NewPositionId,
                request.ApprovedByUserId,
                request.EffectiveDate,
                request.EndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.HistoryType = request.HistoryType;
            entity.OldDepartmentId = NormalizeNullableGuid(request.OldDepartmentId);
            entity.NewDepartmentId = NormalizeNullableGuid(request.NewDepartmentId);
            entity.OldPositionId = NormalizeNullableGuid(request.OldPositionId);
            entity.NewPositionId = NormalizeNullableGuid(request.NewPositionId);
            entity.OldStatus = NormalizeNullableText(request.OldStatus);
            entity.NewStatus = NormalizeNullableText(request.NewStatus);
            entity.EffectiveDate = request.EffectiveDate.Date;
            entity.EndDate = request.EndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.ApprovedByUserId = NormalizeNullableGuid(request.ApprovedByUserId);
            entity.ApprovedAt = request.ApprovedAt;
            entity.FilePath = NormalizeNullableText(request.FilePath);
            entity.FileContentType = NormalizeNullableText(request.FileContentType);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEmploymentContractHistory.UpdateEmploymentHistory",
                "Employment history workforce berhasil diubah.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.HistoryType
                }
            );

            return await GetEmploymentHistoryById(workforceProfileId, entity.Id);
        }

        [HttpPatch("employment-histories/{historyId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Employment History Status",
            Description = "Mengubah status employment history workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Update")]
        public async Task<IActionResult> UpdateEmploymentHistoryStatus(
            Guid workforceProfileId,
            Guid historyId,
            [FromBody] UpdateWorkforceEmploymentHistoryStatusRequest request)
        {
            var entity = await _dbContext.WfpEmploymentHistories
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employment history tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetEmploymentHistoryById(workforceProfileId, entity.Id);
        }

        [HttpDelete("employment-histories/{historyId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Employment History",
            Description = "Menghapus employment history workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Delete")]
        public async Task<IActionResult> DeleteEmploymentHistory(
            Guid workforceProfileId,
            Guid historyId)
        {
            var entity = await _dbContext.WfpEmploymentHistories
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employment history tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Employment history berhasil dihapus."
            ));
        }

        [HttpGet("contract-histories")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceContractHistoryListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Contract History",
            Description = "Melihat data contract history workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Read")]
        public async Task<IActionResult> GetContractHistories(Guid workforceProfileId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;

            var items = await _dbContext.WfpContractHistories
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new WorkforceContractHistoryResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    ContractNumber = x.ContractNumber,
                    ContractType = x.ContractType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ContractStatus = x.ContractStatus,
                    IsExpired = x.EndDate.HasValue && x.EndDate.Value.Date < today,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceContractHistoryListResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                ActiveContractData = items.Count(x => x.ContractStatus == ContractHistoryStatus.Active),
                ExpiredContractData = items.Count(x =>
                    x.ContractStatus == ContractHistoryStatus.Expired ||
                    x.IsExpired),
                TerminatedContractData = items.Count(x => x.ContractStatus == ContractHistoryStatus.Terminated),
                DraftContractData = items.Count(x => x.ContractStatus == ContractHistoryStatus.Draft),
                Items = items
            };

            return Ok(ApiResponse<WorkforceContractHistoryListResponse>.Ok(
                result,
                "Data contract history workforce berhasil diambil."
            ));
        }

        [HttpGet("contract-histories/{contractHistoryId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceContractHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Contract History",
            Description = "Melihat detail contract history workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Read")]
        public async Task<IActionResult> GetContractHistoryById(
            Guid workforceProfileId,
            Guid contractHistoryId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;

            var data = await _dbContext.WfpContractHistories
                .AsNoTracking()
                .Where(x =>
                    x.Id == contractHistoryId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new WorkforceContractHistoryResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    ContractNumber = x.ContractNumber,
                    ContractType = x.ContractType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ContractStatus = x.ContractStatus,
                    IsExpired = x.EndDate.HasValue && x.EndDate.Value.Date < today,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Contract history tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceContractHistoryResponse>.Ok(
                data,
                "Detail contract history workforce berhasil diambil."
            ));
        }

        [HttpPost("contract-histories")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceContractHistoryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Contract History",
            Description = "Membuat contract history workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 7
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Create")]
        public async Task<IActionResult> CreateContractHistory(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceContractHistoryRequest request)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateContractHistoryRequestAsync(
                request.ContractNumber,
                request.ContractType,
                request.StartDate,
                request.EndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpContractHistory
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                ContractNumber = request.ContractNumber.Trim(),
                ContractType = request.ContractType,
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate?.Date,
                ContractStatus = request.ContractStatus,
                FilePath = NormalizeNullableText(request.FilePath),
                FileContentType = NormalizeNullableText(request.FileContentType),
                Notes = NormalizeNullableText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.WfpContractHistories.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEmploymentContractHistory.CreateContractHistory",
                "Contract history workforce berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.ContractNumber
                }
            );

            return await GetContractHistoryById(workforceProfileId, entity.Id);
        }

        [HttpPut("contract-histories/{contractHistoryId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceContractHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Contract History",
            Description = "Mengubah contract history workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 8
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Update")]
        public async Task<IActionResult> UpdateContractHistory(
            Guid workforceProfileId,
            Guid contractHistoryId,
            [FromBody] UpdateWorkforceContractHistoryRequest request)
        {
            var entity = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(x =>
                    x.Id == contractHistoryId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Contract history tidak ditemukan."
                ));
            }

            var validation = await ValidateContractHistoryRequestAsync(
                request.ContractNumber,
                request.ContractType,
                request.StartDate,
                request.EndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ContractNumber = request.ContractNumber.Trim();
            entity.ContractType = request.ContractType;
            entity.StartDate = request.StartDate.Date;
            entity.EndDate = request.EndDate?.Date;
            entity.ContractStatus = request.ContractStatus;
            entity.FilePath = NormalizeNullableText(request.FilePath);
            entity.FileContentType = NormalizeNullableText(request.FileContentType);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEmploymentContractHistory.UpdateContractHistory",
                "Contract history workforce berhasil diubah.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.ContractNumber
                }
            );

            return await GetContractHistoryById(workforceProfileId, entity.Id);
        }

        [HttpPatch("contract-histories/{contractHistoryId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceContractHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Contract History Status",
            Description = "Mengubah status contract history workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 9
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Update")]
        public async Task<IActionResult> UpdateContractHistoryStatus(
            Guid workforceProfileId,
            Guid contractHistoryId,
            [FromBody] UpdateWorkforceContractHistoryStatusRequest request)
        {
            var entity = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(x =>
                    x.Id == contractHistoryId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Contract history tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ContractStatus = request.ContractStatus;
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetContractHistoryById(workforceProfileId, entity.Id);
        }

        [HttpDelete("contract-histories/{contractHistoryId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Contract History",
            Description = "Menghapus contract history workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 10
        )]
        [AccessPermission("WorkforceEmploymentContractHistory", "Delete")]
        public async Task<IActionResult> DeleteContractHistory(
            Guid workforceProfileId,
            Guid contractHistoryId)
        {
            var entity = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(x =>
                    x.Id == contractHistoryId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Contract history tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Contract history berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string Message)> ValidateEmploymentHistoryRequestAsync(
            EmploymentHistoryType historyType,
            Guid? oldDepartmentId,
            Guid? newDepartmentId,
            Guid? oldPositionId,
            Guid? newPositionId,
            Guid? approvedByUserId,
            DateTime effectiveDate,
            DateTime? endDate)
        {
            if (historyType == EmploymentHistoryType.Unknown)
            {
                return (false, "HistoryType wajib dipilih.");
            }

            if (effectiveDate == default)
            {
                return (false, "EffectiveDate wajib diisi.");
            }

            if (endDate.HasValue && endDate.Value.Date < effectiveDate.Date)
            {
                return (false, "EndDate tidak boleh lebih kecil dari EffectiveDate.");
            }

            var departmentIds = new[]
            {
                NormalizeNullableGuid(oldDepartmentId),
                NormalizeNullableGuid(newDepartmentId)
            }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

            foreach (var departmentId in departmentIds)
            {
                var exists = await _dbContext.MstDepartments
                    .AnyAsync(x => x.Id == departmentId && !x.IsDelete);

                if (!exists)
                {
                    return (false, "Department tidak ditemukan.");
                }
            }

            var positionIds = new[]
            {
                NormalizeNullableGuid(oldPositionId),
                NormalizeNullableGuid(newPositionId)
            }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

            foreach (var positionId in positionIds)
            {
                var exists = await _dbContext.MstPositions
                    .AnyAsync(x => x.Id == positionId && !x.IsDelete);

                if (!exists)
                {
                    return (false, "Position tidak ditemukan.");
                }
            }

            if (approvedByUserId.HasValue && approvedByUserId.Value != Guid.Empty)
            {
                var userExists = await _dbContext.Users
                    .AnyAsync(x => x.Id == approvedByUserId.Value);

                if (!userExists)
                {
                    return (false, "ApprovedByUser tidak ditemukan.");
                }
            }

            return (true, string.Empty);
        }

        private static Task<(bool IsValid, string Message)> ValidateContractHistoryRequestAsync(
            string contractNumber,
            ContractHistoryType contractType,
            DateTime startDate,
            DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(contractNumber))
            {
                return Task.FromResult((false, "ContractNumber wajib diisi."));
            }

            if (contractType == ContractHistoryType.Unknown)
            {
                return Task.FromResult((false, "ContractType wajib dipilih."));
            }

            if (startDate == default)
            {
                return Task.FromResult((false, "StartDate wajib diisi."));
            }

            if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            {
                return Task.FromResult((false, "EndDate tidak boleh lebih kecil dari StartDate."));
            }

            return Task.FromResult((true, string.Empty));
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

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private class WorkforceProfileHeader
        {
            public Guid Id { get; set; }

            public string ProfileCode { get; set; } = string.Empty;

            public string DisplayName { get; set; } = string.Empty;

            public UserType UserType { get; set; }
        }
    }
}