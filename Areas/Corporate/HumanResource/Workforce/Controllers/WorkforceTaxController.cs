using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/taxes")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Tax",
        AreaName = "Corporate",
        ControllerName = "WorkforceTax",
        Description = "Workforce tax management",
        SortOrder = 29
    )]
    [Tags("Corporate / Human Resource / Workforce / Tax")]
    public class WorkforceTaxController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Tax";

        private static readonly string[] TaxStatuses =
        {
            "TK0",
            "TK1",
            "TK2",
            "TK3",
            "K0",
            "K1",
            "K2",
            "K3"
        };

        private static readonly string[] TaxCalculationMethods =
        {
            "Gross",
            "GrossUp",
            "Net"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceTaxController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTaxListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Tax",
            Description = "Melihat tax workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTax", "Read")]
        public async Task<IActionResult> GetTaxes(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.Set<WfpTax>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Select(x => new WorkforceTaxResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    NpwpNumber = x.NpwpNumber,
                    TaxStatus = x.TaxStatus,
                    IsTaxed = x.IsTaxed,
                    TaxCalculationMethod = x.TaxCalculationMethod,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceTaxListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                TaxedData = items.Count(x => x.IsTaxed),
                Items = items
            };

            return Ok(ApiResponse<WorkforceTaxListResponse>.Ok(
                result,
                "Data tax workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTaxResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Create",
            "Create Workforce Tax",
            Description = "Menambah tax workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceTax", "Create")]
        public async Task<IActionResult> CreateTax(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceTaxRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = ValidateTaxRequest(
                request.NpwpNumber,
                request.TaxStatus,
                request.TaxCalculationMethod,
                request.EffectiveStartDate,
                request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tax tidak valid."
                ));
            }

            var duplicate = await _dbContext.Set<WfpTax>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tax workforce profile ini sudah tersedia. Gunakan endpoint update."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpTax
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                NpwpNumber = NormalizeDigitsOnly(request.NpwpNumber),
                TaxStatus = request.TaxStatus.Trim().ToUpperInvariant(),
                IsTaxed = request.IsTaxed,
                TaxCalculationMethod = request.TaxCalculationMethod.Trim(),
                EffectiveStartDate = request.EffectiveStartDate.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpTax>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildTaxResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTax.CreateTax",
                "Tax workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.TaxStatus,
                    entity.IsTaxed,
                    entity.TaxCalculationMethod
                }
            );

            return Ok(ApiResponse<WorkforceTaxResponse>.Ok(
                response!,
                "Tax workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTaxResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Tax",
            Description = "Mengubah tax workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceTax", "Update")]
        public async Task<IActionResult> UpdateTax(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceTaxRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpTax>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tax workforce tidak ditemukan."
                ));
            }

            var validation = ValidateTaxRequest(
                request.NpwpNumber,
                request.TaxStatus,
                request.TaxCalculationMethod,
                request.EffectiveStartDate,
                request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tax tidak valid."
                ));
            }

            entity.NpwpNumber = NormalizeDigitsOnly(request.NpwpNumber);
            entity.TaxStatus = request.TaxStatus.Trim().ToUpperInvariant();
            entity.IsTaxed = request.IsTaxed;
            entity.TaxCalculationMethod = request.TaxCalculationMethod.Trim();
            entity.EffectiveStartDate = request.EffectiveStartDate.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildTaxResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTax.UpdateTax",
                "Tax workforce berhasil diperbarui.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.TaxStatus,
                    entity.IsTaxed,
                    entity.TaxCalculationMethod
                }
            );

            return Ok(ApiResponse<WorkforceTaxResponse>.Ok(
                response!,
                "Tax workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Tax",
            Description = "Mengubah status tax workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceTax", "Update")]
        public async Task<IActionResult> UpdateTaxStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceTaxStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpTax>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tax workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.IsTaxed = request.IsTaxed;

            if (request.EffectiveEndDate.HasValue)
            {
                entity.EffectiveEndDate = request.EffectiveEndDate.Value.Date;
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                entity.Description = NormalizeNullableText(request.Description);
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status tax workforce berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Tax",
            Description = "Menghapus tax workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceTax", "Delete")]
        public async Task<IActionResult> DeleteTax(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpTax>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tax workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsTaxed = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tax workforce berhasil dihapus."
            ));
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceProfileId &&
                    !x.IsDelete);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateTaxRequest(
            string? npwpNumber,
            string? taxStatus,
            string? taxCalculationMethod,
            DateTime effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (string.IsNullOrWhiteSpace(taxStatus))
            {
                return (false, "TaxStatus wajib diisi.");
            }

            var normalizedTaxStatus = taxStatus.Trim().ToUpperInvariant();

            if (!TaxStatuses.Contains(normalizedTaxStatus))
            {
                return (false, "TaxStatus tidak valid. Gunakan TK0/TK1/TK2/TK3/K0/K1/K2/K3.");
            }

            if (string.IsNullOrWhiteSpace(taxCalculationMethod))
            {
                return (false, "TaxCalculationMethod wajib diisi.");
            }

            if (!TaxCalculationMethods.Contains(taxCalculationMethod.Trim()))
            {
                return (false, "TaxCalculationMethod tidak valid. Gunakan Gross, GrossUp, atau Net.");
            }

            if (!string.IsNullOrWhiteSpace(npwpNumber))
            {
                var digits = NormalizeDigitsOnly(npwpNumber);

                if (digits != null && digits.Length > 30)
                {
                    return (false, "NPWP maksimal 30 digit.");
                }
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

            return (true, null);
        }

        private async Task<WorkforceTaxResponse?> BuildTaxResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpTax>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDelete)
                .Select(x => new WorkforceTaxResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null
                        ? x.WorkforceProfile.ProfileCode
                        : string.Empty,
                    DisplayName = x.WorkforceProfile != null
                        ? x.WorkforceProfile.DisplayName
                        : string.Empty,
                    NpwpNumber = x.NpwpNumber,
                    TaxStatus = x.TaxStatus,
                    IsTaxed = x.IsTaxed,
                    TaxCalculationMethod = x.TaxCalculationMethod,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
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

        private static string? NormalizeDigitsOnly(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var digits = new string(value.Where(char.IsDigit).ToArray());

            return string.IsNullOrWhiteSpace(digits)
                ? null
                : digits;
        }
    }
}