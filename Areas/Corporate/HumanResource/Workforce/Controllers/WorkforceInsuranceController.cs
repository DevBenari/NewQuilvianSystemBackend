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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/insurances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Insurance",
        AreaName = "Corporate",
        ControllerName = "WorkforceInsurance",
        Description = "Workforce insurance management",
        SortOrder = 30
    )]
    [Tags("Corporate / Human Resource / Workforce / Insurance")]
    public class WorkforceInsuranceController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Insurance";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceInsuranceController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Insurance",
            Description = "Melihat insurance workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceInsurance", "Read")]
        public async Task<IActionResult> GetInsurances(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.Set<WfpInsurance>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Select(x => new WorkforceInsuranceResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    IsBpjsKesehatanEnabled = x.IsBpjsKesehatanEnabled,
                    BpjsKesehatanNumber = x.BpjsKesehatanNumber,
                    IsBpjsKetenagakerjaanEnabled = x.IsBpjsKetenagakerjaanEnabled,
                    BpjsKetenagakerjaanNumber = x.BpjsKetenagakerjaanNumber,
                    IsPrivateInsuranceEnabled = x.IsPrivateInsuranceEnabled,
                    PrivateInsuranceProvider = x.PrivateInsuranceProvider,
                    PrivateInsuranceNumber = x.PrivateInsuranceNumber,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceInsuranceListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                BpjsKesehatanData = items.Count(x => x.IsBpjsKesehatanEnabled),
                BpjsKetenagakerjaanData = items.Count(x => x.IsBpjsKetenagakerjaanEnabled),
                PrivateInsuranceData = items.Count(x => x.IsPrivateInsuranceEnabled),
                Items = items
            };

            return Ok(ApiResponse<WorkforceInsuranceListResponse>.Ok(
                result,
                "Data insurance workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Create",
            "Create Workforce Insurance",
            Description = "Menambah insurance workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceInsurance", "Create")]
        public async Task<IActionResult> CreateInsurance(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceInsuranceRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = ValidateInsuranceRequest(
                request.IsBpjsKesehatanEnabled,
                request.BpjsKesehatanNumber,
                request.IsBpjsKetenagakerjaanEnabled,
                request.BpjsKetenagakerjaanNumber,
                request.IsPrivateInsuranceEnabled,
                request.PrivateInsuranceProvider,
                request.PrivateInsuranceNumber,
                request.EffectiveStartDate,
                request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance tidak valid."
                ));
            }

            var duplicate = await _dbContext.Set<WfpInsurance>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Insurance workforce profile ini sudah tersedia. Gunakan endpoint update."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpInsurance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                IsBpjsKesehatanEnabled = request.IsBpjsKesehatanEnabled,
                BpjsKesehatanNumber = NormalizeNullableText(request.BpjsKesehatanNumber),
                IsBpjsKetenagakerjaanEnabled = request.IsBpjsKetenagakerjaanEnabled,
                BpjsKetenagakerjaanNumber = NormalizeNullableText(request.BpjsKetenagakerjaanNumber),
                IsPrivateInsuranceEnabled = request.IsPrivateInsuranceEnabled,
                PrivateInsuranceProvider = NormalizeNullableText(request.PrivateInsuranceProvider),
                PrivateInsuranceNumber = NormalizeNullableText(request.PrivateInsuranceNumber),
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpInsurance>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildInsuranceResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceInsurance.CreateInsurance",
                "Insurance workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.IsBpjsKesehatanEnabled,
                    entity.IsBpjsKetenagakerjaanEnabled,
                    entity.IsPrivateInsuranceEnabled
                }
            );

            return Ok(ApiResponse<WorkforceInsuranceResponse>.Ok(
                response!,
                "Insurance workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceInsuranceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Insurance",
            Description = "Mengubah insurance workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceInsurance", "Update")]
        public async Task<IActionResult> UpdateInsurance(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceInsuranceRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpInsurance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance workforce tidak ditemukan."
                ));
            }

            var validation = ValidateInsuranceRequest(
                request.IsBpjsKesehatanEnabled,
                request.BpjsKesehatanNumber,
                request.IsBpjsKetenagakerjaanEnabled,
                request.BpjsKetenagakerjaanNumber,
                request.IsPrivateInsuranceEnabled,
                request.PrivateInsuranceProvider,
                request.PrivateInsuranceNumber,
                request.EffectiveStartDate,
                request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance tidak valid."
                ));
            }

            entity.IsBpjsKesehatanEnabled = request.IsBpjsKesehatanEnabled;
            entity.BpjsKesehatanNumber = NormalizeNullableText(request.BpjsKesehatanNumber);
            entity.IsBpjsKetenagakerjaanEnabled = request.IsBpjsKetenagakerjaanEnabled;
            entity.BpjsKetenagakerjaanNumber = NormalizeNullableText(request.BpjsKetenagakerjaanNumber);
            entity.IsPrivateInsuranceEnabled = request.IsPrivateInsuranceEnabled;
            entity.PrivateInsuranceProvider = NormalizeNullableText(request.PrivateInsuranceProvider);
            entity.PrivateInsuranceNumber = NormalizeNullableText(request.PrivateInsuranceNumber);
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildInsuranceResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceInsurance.UpdateInsurance",
                "Insurance workforce berhasil diperbarui.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.IsBpjsKesehatanEnabled,
                    entity.IsBpjsKetenagakerjaanEnabled,
                    entity.IsPrivateInsuranceEnabled
                }
            );

            return Ok(ApiResponse<WorkforceInsuranceResponse>.Ok(
                response!,
                "Insurance workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Insurance",
            Description = "Mengubah status insurance workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceInsurance", "Update")]
        public async Task<IActionResult> UpdateInsuranceStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceInsuranceStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpInsurance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;

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
                "Status insurance workforce berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Insurance",
            Description = "Menghapus insurance workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceInsurance", "Delete")]
        public async Task<IActionResult> DeleteInsurance(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpInsurance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Insurance workforce berhasil dihapus."
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

        private static (bool IsValid, string? ErrorMessage) ValidateInsuranceRequest(
            bool isBpjsKesehatanEnabled,
            string? bpjsKesehatanNumber,
            bool isBpjsKetenagakerjaanEnabled,
            string? bpjsKetenagakerjaanNumber,
            bool isPrivateInsuranceEnabled,
            string? privateInsuranceProvider,
            string? privateInsuranceNumber,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (isBpjsKesehatanEnabled &&
                string.IsNullOrWhiteSpace(bpjsKesehatanNumber))
            {
                return (false, "Nomor BPJS Kesehatan wajib diisi jika BPJS Kesehatan aktif.");
            }

            if (isBpjsKetenagakerjaanEnabled &&
                string.IsNullOrWhiteSpace(bpjsKetenagakerjaanNumber))
            {
                return (false, "Nomor BPJS Ketenagakerjaan wajib diisi jika BPJS Ketenagakerjaan aktif.");
            }

            if (isPrivateInsuranceEnabled)
            {
                if (string.IsNullOrWhiteSpace(privateInsuranceProvider))
                {
                    return (false, "Provider asuransi private wajib diisi jika private insurance aktif.");
                }

                if (string.IsNullOrWhiteSpace(privateInsuranceNumber))
                {
                    return (false, "Nomor asuransi private wajib diisi jika private insurance aktif.");
                }
            }

            if (!string.IsNullOrWhiteSpace(bpjsKesehatanNumber) &&
                bpjsKesehatanNumber.Trim().Length > 50)
            {
                return (false, "Nomor BPJS Kesehatan maksimal 50 karakter.");
            }

            if (!string.IsNullOrWhiteSpace(bpjsKetenagakerjaanNumber) &&
                bpjsKetenagakerjaanNumber.Trim().Length > 50)
            {
                return (false, "Nomor BPJS Ketenagakerjaan maksimal 50 karakter.");
            }

            if (!string.IsNullOrWhiteSpace(privateInsuranceProvider) &&
                privateInsuranceProvider.Trim().Length > 100)
            {
                return (false, "Provider asuransi private maksimal 100 karakter.");
            }

            if (!string.IsNullOrWhiteSpace(privateInsuranceNumber) &&
                privateInsuranceNumber.Trim().Length > 100)
            {
                return (false, "Nomor asuransi private maksimal 100 karakter.");
            }

            if (effectiveStartDate.HasValue &&
                effectiveEndDate.HasValue &&
                effectiveEndDate.Value.Date < effectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            return (true, null);
        }

        private async Task<WorkforceInsuranceResponse?> BuildInsuranceResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpInsurance>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDelete)
                .Select(x => new WorkforceInsuranceResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null
                        ? x.WorkforceProfile.ProfileCode
                        : string.Empty,
                    DisplayName = x.WorkforceProfile != null
                        ? x.WorkforceProfile.DisplayName
                        : string.Empty,
                    IsBpjsKesehatanEnabled = x.IsBpjsKesehatanEnabled,
                    BpjsKesehatanNumber = x.BpjsKesehatanNumber,
                    IsBpjsKetenagakerjaanEnabled = x.IsBpjsKetenagakerjaanEnabled,
                    BpjsKetenagakerjaanNumber = x.BpjsKetenagakerjaanNumber,
                    IsPrivateInsuranceEnabled = x.IsPrivateInsuranceEnabled,
                    PrivateInsuranceProvider = x.PrivateInsuranceProvider,
                    PrivateInsuranceNumber = x.PrivateInsuranceNumber,
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
    }
}