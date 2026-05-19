using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce/transport-allowance-policies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Transport Allowance Policy",
        AreaName = "Corporate",
        ControllerName = "WorkforceTransportAllowancePolicy",
        Description = "Workforce transport allowance policy management",
        SortOrder = 31
    )]
    [Tags("Corporate / Human Resource / Workforce / Transport Allowance Policy")]
    public class WorkforceTransportAllowancePolicyController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.TransportAllowancePolicy";

        private static readonly string[] AllowanceModes =
        {
            "None",
            "FixedMonthly",
            "DailyAttendance",
            "NightShift",
            "MonthlyAndNightShift",
            "Manual"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceTransportAllowancePolicyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTransportAllowancePolicyListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Transport Allowance Policy",
            Description = "Melihat master policy transport allowance workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTransportAllowancePolicy", "Read")]
        public async Task<IActionResult> GetPolicies(
            [FromQuery] string? search,
            [FromQuery] string? allowanceMode,
            [FromQuery] bool? isActive)
        {
            var query = _dbContext.Set<WfpTransportAllowancePolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PolicyCode.ToLower().Contains(keyword) ||
                    x.PolicyName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(allowanceMode))
            {
                query = query.Where(x => x.AllowanceMode == allowanceMode.Trim());
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var items = await query
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.PolicyName)
                .Select(x => new WorkforceTransportAllowancePolicyResponse
                {
                    Id = x.Id,
                    PolicyCode = x.PolicyCode,
                    PolicyName = x.PolicyName,
                    AllowanceMode = x.AllowanceMode,
                    DefaultMonthlyAmount = x.DefaultMonthlyAmount,
                    DefaultDailyAmount = x.DefaultDailyAmount,
                    DefaultNightAmount = x.DefaultNightAmount,
                    NightStartTime = x.NightStartTime,
                    NightEndTime = x.NightEndTime,
                    RequireAttendance = x.RequireAttendance,
                    ExcludeIfAbsent = x.ExcludeIfAbsent,
                    ExcludeIfLeave = x.ExcludeIfLeave,
                    ExcludeIfHoliday = x.ExcludeIfHoliday,
                    IsTaxable = x.IsTaxable,
                    IsPayrollComponent = x.IsPayrollComponent,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceTransportAllowancePolicyListResponse
            {
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                PayrollComponentData = items.Count(x => x.IsPayrollComponent),
                TaxableData = items.Count(x => x.IsTaxable),
                Items = items
            };

            return Ok(ApiResponse<WorkforceTransportAllowancePolicyListResponse>.Ok(
                result,
                "Data policy transport allowance berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<WorkforceTransportAllowancePolicyOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Transport Allowance Policy",
            Description = "Melihat pilihan policy transport allowance workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTransportAllowancePolicy", "Read")]
        public async Task<IActionResult> GetPolicyOptions(
            [FromQuery] string? allowanceMode,
            [FromQuery] bool onlyActive = true)
        {
            var query = _dbContext.Set<WfpTransportAllowancePolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(allowanceMode))
            {
                query = query.Where(x => x.AllowanceMode == allowanceMode.Trim());
            }

            var items = await query
                .OrderBy(x => x.PolicyName)
                .Select(x => new WorkforceTransportAllowancePolicyOptionResponse
                {
                    Id = x.Id,
                    PolicyCode = x.PolicyCode,
                    PolicyName = x.PolicyName,
                    AllowanceMode = x.AllowanceMode,
                    DefaultMonthlyAmount = x.DefaultMonthlyAmount,
                    DefaultDailyAmount = x.DefaultDailyAmount,
                    DefaultNightAmount = x.DefaultNightAmount
                })
                .ToListAsync();

            return Ok(ApiResponse<List<WorkforceTransportAllowancePolicyOptionResponse>>.Ok(
                items,
                "Data pilihan policy transport allowance berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTransportAllowancePolicyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Transport Allowance Policy",
            Description = "Melihat detail policy transport allowance workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTransportAllowancePolicy", "Read")]
        public async Task<IActionResult> GetPolicyById(Guid id)
        {
            var response = await BuildPolicyResponseAsync(id);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Policy transport allowance tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceTransportAllowancePolicyResponse>.Ok(
                response,
                "Detail policy transport allowance berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTransportAllowancePolicyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Workforce Transport Allowance Policy",
            Description = "Membuat master policy transport allowance workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceTransportAllowancePolicy", "Create")]
        public async Task<IActionResult> CreatePolicy(
            [FromBody] CreateWorkforceTransportAllowancePolicyRequest request)
        {
            var validation = await ValidatePolicyRequestAsync(
                excludeId: null,
                policyCode: request.PolicyCode,
                policyName: request.PolicyName,
                allowanceMode: request.AllowanceMode,
                defaultMonthlyAmount: request.DefaultMonthlyAmount,
                defaultDailyAmount: request.DefaultDailyAmount,
                defaultNightAmount: request.DefaultNightAmount,
                nightStartTime: request.NightStartTime,
                nightEndTime: request.NightEndTime
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data policy transport allowance tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpTransportAllowancePolicy
            {
                Id = Guid.NewGuid(),
                PolicyCode = request.PolicyCode.Trim().ToUpperInvariant(),
                PolicyName = request.PolicyName.Trim(),
                AllowanceMode = request.AllowanceMode.Trim(),
                DefaultMonthlyAmount = request.DefaultMonthlyAmount,
                DefaultDailyAmount = request.DefaultDailyAmount,
                DefaultNightAmount = request.DefaultNightAmount,
                NightStartTime = request.NightStartTime,
                NightEndTime = request.NightEndTime,
                RequireAttendance = request.RequireAttendance,
                ExcludeIfAbsent = request.ExcludeIfAbsent,
                ExcludeIfLeave = request.ExcludeIfLeave,
                ExcludeIfHoliday = request.ExcludeIfHoliday,
                IsTaxable = request.IsTaxable,
                IsPayrollComponent = request.IsPayrollComponent,
                IsActive = request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpTransportAllowancePolicy>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildPolicyResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTransportAllowancePolicy.CreatePolicy",
                "Policy transport allowance berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.PolicyCode,
                    entity.PolicyName,
                    entity.AllowanceMode
                }
            );

            return Ok(ApiResponse<WorkforceTransportAllowancePolicyResponse>.Ok(
                response!,
                "Policy transport allowance berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTransportAllowancePolicyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Transport Allowance Policy",
            Description = "Mengubah master policy transport allowance workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceTransportAllowancePolicy", "Update")]
        public async Task<IActionResult> UpdatePolicy(
            Guid id,
            [FromBody] UpdateWorkforceTransportAllowancePolicyRequest request)
        {
            var entity = await _dbContext.Set<WfpTransportAllowancePolicy>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Policy transport allowance tidak ditemukan."
                ));
            }

            var validation = await ValidatePolicyRequestAsync(
                excludeId: id,
                policyCode: request.PolicyCode,
                policyName: request.PolicyName,
                allowanceMode: request.AllowanceMode,
                defaultMonthlyAmount: request.DefaultMonthlyAmount,
                defaultDailyAmount: request.DefaultDailyAmount,
                defaultNightAmount: request.DefaultNightAmount,
                nightStartTime: request.NightStartTime,
                nightEndTime: request.NightEndTime
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data policy transport allowance tidak valid."
                ));
            }

            entity.PolicyCode = request.PolicyCode.Trim().ToUpperInvariant();
            entity.PolicyName = request.PolicyName.Trim();
            entity.AllowanceMode = request.AllowanceMode.Trim();
            entity.DefaultMonthlyAmount = request.DefaultMonthlyAmount;
            entity.DefaultDailyAmount = request.DefaultDailyAmount;
            entity.DefaultNightAmount = request.DefaultNightAmount;
            entity.NightStartTime = request.NightStartTime;
            entity.NightEndTime = request.NightEndTime;
            entity.RequireAttendance = request.RequireAttendance;
            entity.ExcludeIfAbsent = request.ExcludeIfAbsent;
            entity.ExcludeIfLeave = request.ExcludeIfLeave;
            entity.ExcludeIfHoliday = request.ExcludeIfHoliday;
            entity.IsTaxable = request.IsTaxable;
            entity.IsPayrollComponent = request.IsPayrollComponent;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildPolicyResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTransportAllowancePolicy.UpdatePolicy",
                "Policy transport allowance berhasil diperbarui.",
                new
                {
                    entity.Id,
                    entity.PolicyCode,
                    entity.PolicyName,
                    entity.AllowanceMode
                }
            );

            return Ok(ApiResponse<WorkforceTransportAllowancePolicyResponse>.Ok(
                response!,
                "Policy transport allowance berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Transport Allowance Policy",
            Description = "Mengubah status master policy transport allowance workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceTransportAllowancePolicy", "Update")]
        public async Task<IActionResult> UpdatePolicyStatus(
            Guid id,
            [FromBody] UpdateWorkforceTransportAllowancePolicyStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpTransportAllowancePolicy>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Policy transport allowance tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                entity.Description = NormalizeNullableText(request.Description);
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status policy transport allowance berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Delete",
            "Delete Workforce Transport Allowance Policy",
            Description = "Menghapus master policy transport allowance workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceTransportAllowancePolicy", "Delete")]
        public async Task<IActionResult> DeletePolicy(Guid id)
        {
            var entity = await _dbContext.Set<WfpTransportAllowancePolicy>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Policy transport allowance tidak ditemukan."
                ));
            }

            var isUsed = await _dbContext.Set<WfpTransportAllowance>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.TransportAllowancePolicyId == id &&
                    !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Policy transport allowance tidak bisa dihapus karena sudah digunakan workforce."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Policy transport allowance berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidatePolicyRequestAsync(
            Guid? excludeId,
            string? policyCode,
            string? policyName,
            string? allowanceMode,
            decimal defaultMonthlyAmount,
            decimal defaultDailyAmount,
            decimal defaultNightAmount,
            TimeOnly? nightStartTime,
            TimeOnly? nightEndTime)
        {
            if (string.IsNullOrWhiteSpace(policyCode))
            {
                return (false, "PolicyCode wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(policyName))
            {
                return (false, "PolicyName wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(allowanceMode))
            {
                return (false, "AllowanceMode wajib diisi.");
            }

            var normalizedAllowanceMode = allowanceMode.Trim();

            if (!AllowanceModes.Contains(normalizedAllowanceMode))
            {
                return (false, "AllowanceMode tidak valid.");
            }

            if (defaultMonthlyAmount < 0 ||
                defaultDailyAmount < 0 ||
                defaultNightAmount < 0)
            {
                return (false, "Nominal policy transport tidak boleh negatif.");
            }

            if (normalizedAllowanceMode == "NightShift" ||
                normalizedAllowanceMode == "MonthlyAndNightShift")
            {
                if (!nightStartTime.HasValue || !nightEndTime.HasValue)
                {
                    return (false, "NightStartTime dan NightEndTime wajib diisi untuk mode NightShift.");
                }
            }

            var normalizedPolicyCode = policyCode.Trim().ToUpperInvariant();

            var codeExists = await _dbContext.Set<WfpTransportAllowancePolicy>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id != excludeId &&
                    x.PolicyCode == normalizedPolicyCode &&
                    !x.IsDelete);

            if (codeExists)
            {
                return (false, "PolicyCode sudah digunakan.");
            }

            var normalizedPolicyName = policyName.Trim().ToLower();

            var nameExists = await _dbContext.Set<WfpTransportAllowancePolicy>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id != excludeId &&
                    x.PolicyName.ToLower() == normalizedPolicyName &&
                    !x.IsDelete);

            if (nameExists)
            {
                return (false, "PolicyName sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<WorkforceTransportAllowancePolicyResponse?> BuildPolicyResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpTransportAllowancePolicy>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDelete)
                .Select(x => new WorkforceTransportAllowancePolicyResponse
                {
                    Id = x.Id,
                    PolicyCode = x.PolicyCode,
                    PolicyName = x.PolicyName,
                    AllowanceMode = x.AllowanceMode,
                    DefaultMonthlyAmount = x.DefaultMonthlyAmount,
                    DefaultDailyAmount = x.DefaultDailyAmount,
                    DefaultNightAmount = x.DefaultNightAmount,
                    NightStartTime = x.NightStartTime,
                    NightEndTime = x.NightEndTime,
                    RequireAttendance = x.RequireAttendance,
                    ExcludeIfAbsent = x.ExcludeIfAbsent,
                    ExcludeIfLeave = x.ExcludeIfLeave,
                    ExcludeIfHoliday = x.ExcludeIfHoliday,
                    IsTaxable = x.IsTaxable,
                    IsPayrollComponent = x.IsPayrollComponent,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();
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