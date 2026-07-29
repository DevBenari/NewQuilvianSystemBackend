using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/insurance-profile")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL_MANAGEMENT",
        moduleName: "Human Resource Payroll Management",
        displayName: "Workforce Insurance Profile",
        AreaName = "Corporate",
        ControllerName = "WorkforceInsuranceProfile",
        Description = "Corporate human resource workforce insurance profile",
        SortOrder = 3
    )]
    [Tags("Corporate / Human Resource / Payroll Management / Insurance Profile")]
    public class WfpInsuranceController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.PayrollManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpInsuranceController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpInsuranceFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Insurance Profile", Description = "Melihat metadata filter profil asuransi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInsuranceProfile", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpInsuranceFilterMetadataResponse
            {
                DefaultFilter = new WfpInsuranceDefaultFilterResponse(),
                SortOptions = new List<WfpInsuranceSortOptionResponse>
                {
                    new() { Value = "bpjsKesehatan", Label = "BPJS Kesehatan" },
                    new() { Value = "bpjsKetenagakerjaan", Label = "BPJS Ketenagakerjaan" },
                    new() { Value = "privateInsurance", Label = "Asuransi swasta" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpInsuranceFilterMetadataResponse>.Ok(
                result,
                "Metadata filter profil asuransi workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpInsuranceSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Insurance Profile", Description = "Melihat ringkasan profil asuransi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInsuranceProfile", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var query = _dbContext.Set<WfpInsurance>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpInsuranceSummaryResponse
            {
                TotalInsuranceProfile = await query.CountAsync(cancellationToken),
                ActiveInsuranceProfile = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveInsuranceProfile = await query.CountAsync(x => !x.IsActive, cancellationToken),
                BpjsKesehatanEnabledProfile = await query.CountAsync(x => x.IsBpjsKesehatanEnabled, cancellationToken),
                BpjsKetenagakerjaanEnabledProfile = await query.CountAsync(x => x.IsBpjsKetenagakerjaanEnabled, cancellationToken),
                PrivateInsuranceEnabledProfile = await query.CountAsync(x => x.IsPrivateInsuranceEnabled, cancellationToken)
            };

            return Ok(ApiResponse<WfpInsuranceSummaryResponse>.Ok(
                result,
                "Ringkasan profil asuransi workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpInsuranceResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Insurance Profile", Description = "Melihat data profil asuransi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInsuranceProfile", "Read")]
        public async Task<IActionResult> GetInsuranceProfiles(
            Guid workforceProfileId,
            [FromQuery] bool? isBpjsKesehatanEnabled,
            [FromQuery] bool? isBpjsKetenagakerjaanEnabled,
            [FromQuery] bool? isPrivateInsuranceEnabled,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(workforceProfileId),
                isBpjsKesehatanEnabled,
                isBpjsKetenagakerjaanEnabled,
                isPrivateInsuranceEnabled,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WfpInsuranceResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows.Select(MapResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<WfpInsuranceResponse>>.Ok(
                result,
                "Data profil asuransi workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpInsuranceDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Insurance Profile", Description = "Melihat detail profil asuransi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceInsuranceProfile", "Read")]
        public async Task<IActionResult> GetInsuranceProfileById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil asuransi workforce tidak ditemukan."));
            }

            return Ok(ApiResponse<WfpInsuranceDetailResponse>.Ok(
                MapDetailResponse(entity),
                "Detail profil asuransi workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpInsuranceDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Workforce Insurance Profile", Description = "Membuat profil asuransi workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceInsuranceProfile", "Create")]
        public async Task<IActionResult> CreateInsuranceProfile(
            Guid workforceProfileId,
            [FromBody] CreateWfpInsuranceRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            if (await _dbContext.Set<WfpInsurance>().AnyAsync(
                    x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete,
                    cancellationToken))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Profil asuransi untuk workforce ini sudah tersedia."));
            }

            var validation = await ValidateRequestAsync(request, null, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil asuransi tidak valid."));
            }

            var entity = new WfpInsurance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            ApplyRequest(entity, request);

            _dbContext.Set<WfpInsurance>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceInsuranceProfile.Create",
                "Membuat profil asuransi workforce.",
                new { entity.Id, entity.WorkforceProfileId });

            return await GetInsuranceProfileById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpInsuranceDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Insurance Profile", Description = "Mengubah profil asuransi workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceInsuranceProfile", "Update")]
        public async Task<IActionResult> UpdateInsuranceProfile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpInsuranceRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpInsurance>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil asuransi workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(request, id, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil asuransi tidak valid."));
            }

            ApplyRequest(entity, request);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetInsuranceProfileById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Insurance Profile", Description = "Mengubah status profil asuransi workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceInsuranceProfile", "Update")]
        public async Task<IActionResult> UpdateInsuranceProfileStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpInsuranceStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpInsurance>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil asuransi workforce tidak ditemukan."));
            }

            if (request.EffectiveEndDate.HasValue &&
                entity.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.Value.Date < entity.EffectiveStartDate.Value.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate."));
            }

            entity.IsActive = request.IsActive;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status profil asuransi workforce berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Workforce Insurance Profile", Description = "Menghapus profil asuransi workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceInsuranceProfile", "Delete")]
        public async Task<IActionResult> DeleteInsuranceProfile(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpInsurance>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil asuransi workforce tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Profil asuransi workforce berhasil dihapus."));
        }

        private IQueryable<WfpInsurance> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpInsurance>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpInsurance> ApplyFilter(
            IQueryable<WfpInsurance> query,
            bool? isBpjsKesehatanEnabled,
            bool? isBpjsKetenagakerjaanEnabled,
            bool? isPrivateInsuranceEnabled,
            bool? isActive,
            string? search)
        {
            if (isBpjsKesehatanEnabled.HasValue)
                query = query.Where(x => x.IsBpjsKesehatanEnabled == isBpjsKesehatanEnabled.Value);

            if (isBpjsKetenagakerjaanEnabled.HasValue)
                query = query.Where(x => x.IsBpjsKetenagakerjaanEnabled == isBpjsKetenagakerjaanEnabled.Value);

            if (isPrivateInsuranceEnabled.HasValue)
                query = query.Where(x => x.IsPrivateInsuranceEnabled == isPrivateInsuranceEnabled.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.BpjsKesehatanNumber != null && x.BpjsKesehatanNumber.ToLower().Contains(keyword)) ||
                    (x.BpjsKetenagakerjaanNumber != null && x.BpjsKetenagakerjaanNumber.ToLower().Contains(keyword)) ||
                    (x.PrivateInsuranceProvider != null && x.PrivateInsuranceProvider.ToLower().Contains(keyword)) ||
                    (x.PrivateInsuranceNumber != null && x.PrivateInsuranceNumber.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpInsurance> ApplySorting(
            IQueryable<WfpInsurance> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "bpjskesehatan" => isDescending
                    ? query.OrderByDescending(x => x.IsBpjsKesehatanEnabled)
                    : query.OrderBy(x => x.IsBpjsKesehatanEnabled),
                "bpjsketenagakerjaan" => isDescending
                    ? query.OrderByDescending(x => x.IsBpjsKetenagakerjaanEnabled)
                    : query.OrderBy(x => x.IsBpjsKetenagakerjaanEnabled),
                "privateinsurance" => isDescending
                    ? query.OrderByDescending(x => x.IsPrivateInsuranceEnabled)
                    : query.OrderBy(x => x.IsPrivateInsuranceEnabled),
                "effectivestartdate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),
                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),
                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private WfpInsuranceResponse MapResponse(WfpInsurance entity)
        {
            return new WfpInsuranceResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                IsBpjsKesehatanEnabled = entity.IsBpjsKesehatanEnabled,
                BpjsKesehatanNumber = entity.BpjsKesehatanNumber,
                IsBpjsKetenagakerjaanEnabled = entity.IsBpjsKetenagakerjaanEnabled,
                BpjsKetenagakerjaanNumber = entity.BpjsKetenagakerjaanNumber,
                IsPrivateInsuranceEnabled = entity.IsPrivateInsuranceEnabled,
                PrivateInsuranceProvider = entity.PrivateInsuranceProvider,
                PrivateInsuranceNumber = entity.PrivateInsuranceNumber,
                BpjsHealthEmployeeRate = entity.BpjsHealthEmployeeRate,
                BpjsHealthEmployerRate = entity.BpjsHealthEmployerRate,
                BpjsEmploymentEmployeeRate = entity.BpjsEmploymentEmployeeRate,
                BpjsEmploymentEmployerRate = entity.BpjsEmploymentEmployerRate,
                PrivateInsuranceEmployeeContribution = entity.PrivateInsuranceEmployeeContribution,
                PrivateInsuranceEmployerContribution = entity.PrivateInsuranceEmployerContribution,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserDisplayName(entity.CreateBy)
            };
        }

        private WfpInsuranceDetailResponse MapDetailResponse(WfpInsurance entity)
        {
            var response = MapResponse(entity);

            return new WfpInsuranceDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                WorkforceProfileCode = response.WorkforceProfileCode,
                WorkforceDisplayName = response.WorkforceDisplayName,
                IsBpjsKesehatanEnabled = response.IsBpjsKesehatanEnabled,
                BpjsKesehatanNumber = response.BpjsKesehatanNumber,
                IsBpjsKetenagakerjaanEnabled = response.IsBpjsKetenagakerjaanEnabled,
                BpjsKetenagakerjaanNumber = response.BpjsKetenagakerjaanNumber,
                IsPrivateInsuranceEnabled = response.IsPrivateInsuranceEnabled,
                PrivateInsuranceProvider = response.PrivateInsuranceProvider,
                PrivateInsuranceNumber = response.PrivateInsuranceNumber,
                BpjsHealthEmployeeRate = response.BpjsHealthEmployeeRate,
                BpjsHealthEmployerRate = response.BpjsHealthEmployerRate,
                BpjsEmploymentEmployeeRate = response.BpjsEmploymentEmployeeRate,
                BpjsEmploymentEmployerRate = response.BpjsEmploymentEmployerRate,
                PrivateInsuranceEmployeeContribution = response.PrivateInsuranceEmployeeContribution,
                PrivateInsuranceEmployerContribution = response.PrivateInsuranceEmployerContribution,
                EffectiveStartDate = response.EffectiveStartDate,
                EffectiveEndDate = response.EffectiveEndDate,
                Description = response.Description,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserDisplayName(entity.UpdateBy)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            CreateWfpInsuranceRequest request,
            Guid? currentId,
            CancellationToken cancellationToken)
        {
            if (request.IsBpjsKesehatanEnabled &&
                string.IsNullOrWhiteSpace(request.BpjsKesehatanNumber))
            {
                return (false, "Nomor BPJS Kesehatan wajib diisi ketika BPJS Kesehatan diaktifkan.");
            }

            if (request.IsBpjsKetenagakerjaanEnabled &&
                string.IsNullOrWhiteSpace(request.BpjsKetenagakerjaanNumber))
            {
                return (false, "Nomor BPJS Ketenagakerjaan wajib diisi ketika BPJS Ketenagakerjaan diaktifkan.");
            }

            if (request.IsPrivateInsuranceEnabled &&
                string.IsNullOrWhiteSpace(request.PrivateInsuranceProvider))
            {
                return (false, "Provider asuransi swasta wajib diisi ketika asuransi swasta diaktifkan.");
            }

            var percentageValues = new[]
            {
                request.BpjsHealthEmployeeRate,
                request.BpjsHealthEmployerRate,
                request.BpjsEmploymentEmployeeRate,
                request.BpjsEmploymentEmployerRate
            };

            if (percentageValues.Any(x => x < 0 || x > 100))
                return (false, "Persentase kontribusi BPJS harus berada pada rentang 0 sampai 100.");

            if (request.PrivateInsuranceEmployeeContribution < 0 ||
                request.PrivateInsuranceEmployerContribution < 0)
            {
                return (false, "Kontribusi asuransi swasta tidak boleh negatif.");
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            var healthNumber = NormalizeNullableText(request.BpjsKesehatanNumber);
            if (healthNumber != null &&
                await _dbContext.Set<WfpInsurance>().AnyAsync(
                    x => !x.IsDelete &&
                         x.BpjsKesehatanNumber == healthNumber &&
                         (!currentId.HasValue || x.Id != currentId.Value),
                    cancellationToken))
            {
                return (false, "Nomor BPJS Kesehatan sudah digunakan oleh profil lain.");
            }

            var employmentNumber = NormalizeNullableText(request.BpjsKetenagakerjaanNumber);
            if (employmentNumber != null &&
                await _dbContext.Set<WfpInsurance>().AnyAsync(
                    x => !x.IsDelete &&
                         x.BpjsKetenagakerjaanNumber == employmentNumber &&
                         (!currentId.HasValue || x.Id != currentId.Value),
                    cancellationToken))
            {
                return (false, "Nomor BPJS Ketenagakerjaan sudah digunakan oleh profil lain.");
            }

            return (true, null);
        }

        private static void ApplyRequest(
            WfpInsurance entity,
            CreateWfpInsuranceRequest request)
        {
            entity.IsBpjsKesehatanEnabled = request.IsBpjsKesehatanEnabled;
            entity.BpjsKesehatanNumber = NormalizeNullableText(request.BpjsKesehatanNumber);
            entity.IsBpjsKetenagakerjaanEnabled = request.IsBpjsKetenagakerjaanEnabled;
            entity.BpjsKetenagakerjaanNumber = NormalizeNullableText(request.BpjsKetenagakerjaanNumber);
            entity.IsPrivateInsuranceEnabled = request.IsPrivateInsuranceEnabled;
            entity.PrivateInsuranceProvider = NormalizeNullableText(request.PrivateInsuranceProvider);
            entity.PrivateInsuranceNumber = NormalizeNullableText(request.PrivateInsuranceNumber);
            entity.BpjsHealthEmployeeRate = request.BpjsHealthEmployeeRate;
            entity.BpjsHealthEmployerRate = request.BpjsHealthEmployerRate;
            entity.BpjsEmploymentEmployeeRate = request.BpjsEmploymentEmployeeRate;
            entity.BpjsEmploymentEmployerRate = request.BpjsEmploymentEmployerRate;
            entity.PrivateInsuranceEmployeeContribution = request.PrivateInsuranceEmployeeContribution;
            entity.PrivateInsuranceEmployerContribution = request.PrivateInsuranceEmployerContribution;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
        }

        private async Task<bool> WorkforceProfileExistsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty &&
                   await _dbContext.MstWorkforceProfiles
                       .AsNoTracking()
                       .AnyAsync(
                           x => x.Id == workforceProfileId &&
                                x.IsActive &&
                                !x.IsDelete,
                           cancellationToken);
        }

        private string? GetUserDisplayName(Guid userId)
        {
            if (userId == Guid.Empty)
                return null;

            return _dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefault();
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

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;
            return (pageNumber, pageSize);
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
