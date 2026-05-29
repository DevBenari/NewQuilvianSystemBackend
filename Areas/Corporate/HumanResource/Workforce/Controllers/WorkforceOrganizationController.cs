using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/organizations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Organization",
        AreaName = "Corporate",
        ControllerName = "WorkforceOrganization",
        Description = "Workforce organization assignment management",
        SortOrder = 20
    )]
    [Tags("Corporate / Human Resource / Workforce / Organization")]
    public class WorkforceOrganizationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Organization";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceOrganizationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Organization",
            Description = "Melihat organization assignment workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOrganization", "Read")]
        public async Task<IActionResult> GetOrganizations(Guid workforceProfileId)
        {
            var profile = await GetProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsActive)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Select(x => new WorkforceOrganizationResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    DepartmentId = x.DepartmentId,
                    DepartmentCode = x.Department != null ? x.Department.DepartmentCode : string.Empty,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
                    PositionId = x.PositionId,
                    PositionCode = x.Position != null ? x.Position.PositionCode : string.Empty,
                    PositionName = x.Position != null ? x.Position.PositionName : string.Empty,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceOrganizationListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                PrimaryData = items.Count(x => x.IsPrimary && x.IsActive),
                Items = items
            };

            return Ok(ApiResponse<WorkforceOrganizationListResponse>.Ok(
                result,
                "Data organization assignment workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Organization",
            Description = "Menambah organization assignment workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceOrganization", "Create")]
        public async Task<IActionResult> CreateOrganization(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceOrganizationRequest request)
        {
            var profile = await _dbContext.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateOrganizationAssignmentRequestAsync(
                workforceProfileId,
                request.DepartmentId,
                request.PositionId,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                excludeAssignmentId: null);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data organization assignment tidak valid."
                ));
            }

            if (request.IsPrimary && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Organization assignment primary harus aktif."
                ));
            }

            var hasAnyAssignment = await _dbContext.Set<WfpOrganizationAssignment>()
                .AnyAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var shouldBePrimary = request.IsPrimary || !hasAnyAssignment;

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var assignment = new WfpOrganizationAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    DepartmentId = request.DepartmentId,
                    PositionId = request.PositionId,
                    IsPrimary = false,
                    IsActive = request.IsActive,
                    EffectiveStartDate = request.EffectiveStartDate.Date,
                    EffectiveEndDate = request.EffectiveEndDate?.Date,
                    Description = NormalizeNullableText(request.Description),
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpOrganizationAssignment>().Add(assignment);

                if (shouldBePrimary)
                {
                    await ApplyPrimaryOrganizationAsync(profile, assignment, now, actorUserId);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildOrganizationAssignmentResponseAsync(assignment.Id);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceOrganization.CreateOrganization",
                    "Organization assignment workforce berhasil dibuat.",
                    new
                    {
                        workforceProfileId,
                        assignment.Id,
                        assignment.DepartmentId,
                        assignment.PositionId,
                        assignment.IsPrimary
                    }
                );

                return Ok(ApiResponse<WorkforceOrganizationResponse>.Ok(
                    response!,
                    "Organization assignment workforce berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceOrganization.CreateOrganization",
                    "Gagal membuat organization assignment workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat organization assignment workforce."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Organization",
            Description = "Mengubah organization assignment workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOrganization", "Update")]
        public async Task<IActionResult> UpdateOrganization(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceOrganizationRequest request)
        {
            var profile = await _dbContext.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var assignment = await _dbContext.Set<WfpOrganizationAssignment>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (assignment == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization assignment workforce tidak ditemukan."
                ));
            }

            if (assignment.IsPrimary && !request.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment tidak boleh dilepas langsung. Jadikan assignment lain sebagai primary terlebih dahulu."
                ));
            }

            if ((assignment.IsPrimary || request.IsPrimary) && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment harus aktif."
                ));
            }

            var validation = await ValidateOrganizationAssignmentRequestAsync(
                workforceProfileId,
                request.DepartmentId,
                request.PositionId,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                excludeAssignmentId: id);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data organization assignment tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                assignment.DepartmentId = request.DepartmentId;
                assignment.PositionId = request.PositionId;
                assignment.IsActive = request.IsActive;
                assignment.EffectiveStartDate = request.EffectiveStartDate.Date;
                assignment.EffectiveEndDate = request.EffectiveEndDate?.Date;
                assignment.Description = NormalizeNullableText(request.Description);
                assignment.UpdateDateTime = now;
                assignment.UpdateBy = actorUserId;

                if (request.IsPrimary)
                {
                    await ApplyPrimaryOrganizationAsync(profile, assignment, now, actorUserId);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildOrganizationAssignmentResponseAsync(assignment.Id);

                return Ok(ApiResponse<WorkforceOrganizationResponse>.Ok(
                    response!,
                    "Organization assignment workforce berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceOrganization.UpdateOrganization",
                    "Gagal mengubah organization assignment workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah organization assignment workforce."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
    "Update",
    "Update Workforce Organization",
    Description = "Mengubah status organization assignment workforce",
    AccessType = AccessTypes.Update,
    SortOrder = 3
)]
        [AccessPermission("WorkforceOrganization", "Update")]
        public async Task<IActionResult> UpdateOrganizationStatus(
    Guid workforceProfileId,
    Guid id,
    [FromBody] UpdateWorkforceOrganizationStatusRequest request)
        {
            var assignment = await _dbContext.Set<WfpOrganizationAssignment>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (assignment == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization assignment workforce tidak ditemukan."
                ));
            }

            if (assignment.IsPrimary && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment tidak boleh dinonaktifkan. Jadikan assignment lain sebagai primary terlebih dahulu."
                ));
            }

            if (assignment.IsPrimary && request.EffectiveEndDate.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment tidak boleh diberi EffectiveEndDate. Jadikan assignment lain sebagai primary terlebih dahulu."
                ));
            }

            if (request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < assignment.EffectiveStartDate.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate."
                ));
            }

            var now = DateTime.UtcNow;

            assignment.IsActive = request.IsActive;
            assignment.EffectiveEndDate = request.IsActive
                ? request.EffectiveEndDate?.Date
                : request.EffectiveEndDate?.Date ?? now.Date;

            assignment.Description = NormalizeNullableText(request.Description) ?? assignment.Description;
            assignment.UpdateDateTime = now;
            assignment.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status organization assignment workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Organization",
            Description = "Menetapkan primary organization assignment workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOrganization", "Update")]
        public async Task<IActionResult> SetPrimaryOrganization(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWorkforceOrganizationPrimaryRequest request)
        {
            if (!request.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "IsPrimary harus bernilai true."
                ));
            }

            var profile = await _dbContext.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var assignment = await _dbContext.Set<WfpOrganizationAssignment>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (assignment == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization assignment workforce tidak ditemukan."
                ));
            }

            if (!assignment.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Assignment tidak aktif tidak bisa dijadikan primary."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await ApplyPrimaryOrganizationAsync(profile, assignment, now, actorUserId);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildOrganizationAssignmentResponseAsync(assignment.Id);

                return Ok(ApiResponse<WorkforceOrganizationResponse>.Ok(
                    response!,
                    "Primary organization workforce berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceOrganization.SetPrimaryOrganization",
                    "Gagal menetapkan primary organization workforce.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menetapkan primary organization workforce."
                    )
                );
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Organization",
            Description = "Menghapus organization assignment workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceOrganization", "Delete")]
        public async Task<IActionResult> DeleteOrganization(Guid workforceProfileId, Guid id)
        {
            var assignment = await _dbContext.Set<WfpOrganizationAssignment>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (assignment == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization assignment workforce tidak ditemukan."
                ));
            }

            if (assignment.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment tidak boleh dihapus. Jadikan assignment lain sebagai primary terlebih dahulu."
                ));
            }

            assignment.IsActive = false;
            assignment.IsDelete = true;
            assignment.DeleteDateTime = DateTime.UtcNow;
            assignment.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Organization assignment workforce berhasil dihapus."
            ));
        }

        private async Task ApplyPrimaryOrganizationAsync(
    MstWorkforceProfile profile,
    WfpOrganizationAssignment assignment,
    DateTime now,
    Guid actorUserId)
        {
            var currentPrimaries = await _dbContext.Set<WfpOrganizationAssignment>()
                .Where(x =>
                    x.WorkforceProfileId == profile.Id &&
                    x.Id != assignment.Id &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in currentPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            assignment.IsPrimary = true;
            assignment.IsActive = true;
            assignment.EffectiveEndDate = null;
            assignment.UpdateDateTime = now;
            assignment.UpdateBy = actorUserId;

            profile.PrimaryDepartmentId = assignment.DepartmentId;
            profile.PrimaryPositionId = assignment.PositionId;
            profile.UpdateDateTime = now;
            profile.UpdateBy = actorUserId;

            if (profile.UserType == UserType.Employee)
            {
                var employee = await _dbContext.Set<MstEmployee>()
                    .FirstOrDefaultAsync(x =>
                        x.WorkforceProfileId == profile.Id &&
                        !x.IsDelete);

                if (employee != null)
                {
                    employee.PrimaryDepartmentId = assignment.DepartmentId;
                    employee.PrimaryPositionId = assignment.PositionId;
                    employee.UpdateDateTime = now;
                    employee.UpdateBy = actorUserId;
                }
            }

            if (profile.UserType == UserType.PermanentDoctor ||
                profile.UserType == UserType.GuestDoctor)
            {
                var doctor = await _dbContext.Set<MstDoctor>()
                    .FirstOrDefaultAsync(x =>
                        x.WorkforceProfileId == profile.Id &&
                        !x.IsDelete);

                if (doctor != null)
                {
                    doctor.PrimaryDepartmentId = assignment.DepartmentId;
                    doctor.PrimaryPositionId = assignment.PositionId;
                    doctor.UpdateDateTime = now;
                    doctor.UpdateBy = actorUserId;
                }
            }

            var linkedUser = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == profile.Id);

            if (linkedUser != null)
            {
                linkedUser.PrimaryDepartmentId = assignment.DepartmentId;
                linkedUser.PrimaryPositionId = assignment.PositionId;
                linkedUser.UpdateDateTime = now;

                await SyncUserPrimaryOrganizationAsync(
                    linkedUser.Id,
                    assignment.DepartmentId,
                    assignment.PositionId,
                    assignment.EffectiveStartDate,
                    actorUserId);
            }
        }

        private async Task SyncUserPrimaryOrganizationAsync(
    Guid userId,
    Guid departmentId,
    Guid positionId,
    DateTime effectiveStartDate,
    Guid actorUserId)
        {
            var now = DateTime.UtcNow;
            var effectiveStartUtc = DateTime.SpecifyKind(
                effectiveStartDate.Date,
                DateTimeKind.Utc
            );

            var currentPrimaries = await _dbContext.Set<ApplicationUserOrganization>()
                .Where(x =>
                    x.UserId == userId &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in currentPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            var existing = await _dbContext.Set<ApplicationUserOrganization>()
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId);

            if (existing == null)
            {
                existing = new ApplicationUserOrganization
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DepartmentId = departmentId,
                    PositionId = positionId,
                    IsPrimary = true,
                    IsActive = true,
                    EffectiveStartDate = effectiveStartUtc,
                    EffectiveEndDate = null,
                    Description = "Synced from workforce primary organization",
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<ApplicationUserOrganization>().Add(existing);
            }
            else
            {
                existing.IsPrimary = true;
                existing.IsActive = true;
                existing.EffectiveStartDate = effectiveStartUtc;
                existing.EffectiveEndDate = null;
                existing.Description = "Synced from workforce primary organization";
                existing.IsDelete = false;
                existing.DeleteDateTime = null;
                existing.DeleteBy = Guid.Empty;
                existing.IsCancel = false;
                existing.CancelDateTime = null;
                existing.CancelBy = Guid.Empty;
                existing.UpdateDateTime = now;
                existing.UpdateBy = actorUserId;
            }
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateOrganizationAssignmentRequestAsync(
            Guid workforceProfileId,
            Guid departmentId,
            Guid positionId,
            DateTime effectiveStartDate,
            DateTime? effectiveEndDate,
            Guid? excludeAssignmentId)
        {
            if (departmentId == Guid.Empty)
            {
                return (false, "Department wajib dipilih.");
            }

            if (positionId == Guid.Empty)
            {
                return (false, "Position wajib dipilih.");
            }

            if (effectiveStartDate == default)
            {
                return (false, "EffectiveStartDate wajib diisi.");
            }

            if (effectiveEndDate.HasValue &&
                effectiveEndDate.Value.Date < effectiveStartDate.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            var departmentExists = await _dbContext.Set<MstDepartment>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == departmentId && x.IsActive && !x.IsDelete);

            if (!departmentExists)
            {
                return (false, "Department tidak valid atau tidak aktif.");
            }

            var positionExists = await _dbContext.Set<MstPosition>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == positionId &&
                    x.DepartmentId == departmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!positionExists)
            {
                return (false, "Position tidak valid, tidak aktif, atau tidak sesuai department.");
            }

            var duplicateExists = await _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.Id != excludeAssignmentId &&
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (duplicateExists)
            {
                return (false, "Workforce profile sudah memiliki assignment aktif pada department dan position tersebut.");
            }

            return (true, null);
        }

        private async Task<WorkforceOrganizationResponse?> BuildOrganizationAssignmentResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceOrganizationResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    DepartmentId = x.DepartmentId,
                    DepartmentCode = x.Department != null ? x.Department.DepartmentCode : string.Empty,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : string.Empty,
                    PositionId = x.PositionId,
                    PositionCode = x.Position != null ? x.Position.PositionCode : string.Empty,
                    PositionName = x.Position != null ? x.Position.PositionName : string.Empty,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();
        }

        private async Task<MstWorkforceProfile?> GetProfileHeaderAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}