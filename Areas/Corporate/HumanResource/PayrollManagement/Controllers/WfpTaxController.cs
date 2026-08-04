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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/tax-profile")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL_MANAGEMENT",
        moduleName: "Human Resource Payroll Management",
        displayName: "Workforce Tax Profile",
        AreaName = "Corporate",
        ControllerName = "WorkforceTaxProfile",
        Description = "Corporate human resource workforce tax profile",
        SortOrder = 2
    )]
    [Tags("Corporate / Human Resource / Payroll Management / Tax Profile")]
    public class WfpTaxController : ControllerBase
    {
        private static readonly HashSet<string> AllowedTaxMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "Gross",
            "GrossUp",
            "Net"
        };

        private static readonly HashSet<string> AllowedTaxStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "TK/0", "TK/1", "TK/2", "TK/3",
            "K/0", "K/1", "K/2", "K/3",
            "K/I/0", "K/I/1", "K/I/2", "K/I/3"
        };

        private const string LogCategory = "Corporate.HumanResource.PayrollManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpTaxController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Tax Profile", Description = "Melihat metadata filter profil pajak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTaxProfile", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpTaxFilterMetadataResponse
            {
                DefaultFilter = new WfpTaxDefaultFilterResponse(),
                TaxStatusOptions = AllowedTaxStatuses
                    .OrderBy(x => x)
                    .Select(x => new WfpTaxStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),
                TaxMethodOptions = AllowedTaxMethods
                    .OrderBy(x => x)
                    .Select(x => new WfpTaxStringOptionResponse
                    {
                        Value = x,
                        Label = x == "GrossUp" ? "Gross Up" : x
                    })
                    .ToList(),
                SortOptions = new List<WfpTaxSortOptionResponse>
                {
                    new() { Value = "taxStatus", Label = "Status pajak" },
                    new() { Value = "taxMethod", Label = "Metode pajak" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpTaxFilterMetadataResponse>.Ok(
                result,
                "Metadata filter profil pajak workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Tax Profile", Description = "Melihat ringkasan profil pajak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTaxProfile", "Read")]
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

            var query = _dbContext.Set<WfpTax>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpTaxSummaryResponse
            {
                TotalTaxProfile = await query.CountAsync(cancellationToken),
                ActiveTaxProfile = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveTaxProfile = await query.CountAsync(x => !x.IsActive, cancellationToken),
                NpwpRegisteredProfile = await query.CountAsync(x => x.IsNpwpRegistered, cancellationToken),
                TaxResidentProfile = await query.CountAsync(x => x.IsTaxResident, cancellationToken),
                PreviousEmployerProfile = await query.CountAsync(x => x.HasPreviousEmployer, cancellationToken),
                EmployerBorneTaxProfile = await query.CountAsync(x => x.IsEmployerBorneTax, cancellationToken)
            };

            return Ok(ApiResponse<WfpTaxSummaryResponse>.Ok(
                result,
                "Ringkasan profil pajak workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpTaxResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Tax Profile", Description = "Melihat data profil pajak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTaxProfile", "Read")]
        public async Task<IActionResult> GetTaxProfiles(
            Guid workforceProfileId,
            [FromQuery] string? taxStatus,
            [FromQuery] string? taxMethod,
            [FromQuery] bool? isNpwpRegistered,
            [FromQuery] bool? isTaxResident,
            [FromQuery] bool? hasPreviousEmployer,
            [FromQuery] bool? isEmployerBorneTax,
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
                taxStatus,
                taxMethod,
                isNpwpRegistered,
                isTaxResident,
                hasPreviousEmployer,
                isEmployerBorneTax,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WfpTaxResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows.Select(MapResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<WfpTaxResponse>>.Ok(
                result,
                "Data profil pajak workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Tax Profile", Description = "Melihat detail profil pajak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTaxProfile", "Read")]
        public async Task<IActionResult> GetTaxProfileById(
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
                    "Profil pajak workforce tidak ditemukan."));
            }

            var result = MapDetailResponse(entity);

            return Ok(ApiResponse<WfpTaxDetailResponse>.Ok(
                result,
                "Detail profil pajak workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Workforce Tax Profile", Description = "Membuat profil pajak workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceTaxProfile", "Create")]
        public async Task<IActionResult> CreateTaxProfile(
            Guid workforceProfileId,
            [FromBody] CreateWfpTaxRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            if (await _dbContext.Set<WfpTax>().AnyAsync(
                    x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete,
                    cancellationToken))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Profil pajak untuk workforce ini sudah tersedia."));
            }

            var validation = await ValidateRequestAsync(request, null, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil pajak tidak valid."));
            }

            var entity = new WfpTax
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            ApplyRequest(entity, request);

            _dbContext.Set<WfpTax>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTaxProfile.Create",
                "Membuat profil pajak workforce.",
                new { entity.Id, entity.WorkforceProfileId });

            return await GetTaxProfileById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpTaxDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Tax Profile", Description = "Mengubah profil pajak workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceTaxProfile", "Update")]
        public async Task<IActionResult> UpdateTaxProfile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpTaxRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTax>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil pajak workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(request, id, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil pajak tidak valid."));
            }

            ApplyRequest(entity, request);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetTaxProfileById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Tax Profile", Description = "Mengubah status profil pajak workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceTaxProfile", "Update")]
        public async Task<IActionResult> UpdateTaxProfileStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpTaxStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTax>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil pajak workforce tidak ditemukan."));
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
                "Status profil pajak workforce berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Workforce Tax Profile", Description = "Menghapus profil pajak workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceTaxProfile", "Delete")]
        public async Task<IActionResult> DeleteTaxProfile(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpTax>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil pajak workforce tidak ditemukan."));
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
                "Profil pajak workforce berhasil dihapus."));
        }

        private IQueryable<WfpTax> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpTax>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpTax> ApplyFilter(
            IQueryable<WfpTax> query,
            string? taxStatus,
            string? taxMethod,
            bool? isNpwpRegistered,
            bool? isTaxResident,
            bool? hasPreviousEmployer,
            bool? isEmployerBorneTax,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(taxStatus))
                query = query.Where(x => x.TaxStatus == taxStatus.Trim());

            if (!string.IsNullOrWhiteSpace(taxMethod))
                query = query.Where(x => x.TaxMethod == taxMethod.Trim());

            if (isNpwpRegistered.HasValue)
                query = query.Where(x => x.IsNpwpRegistered == isNpwpRegistered.Value);

            if (isTaxResident.HasValue)
                query = query.Where(x => x.IsTaxResident == isTaxResident.Value);

            if (hasPreviousEmployer.HasValue)
                query = query.Where(x => x.HasPreviousEmployer == hasPreviousEmployer.Value);

            if (isEmployerBorneTax.HasValue)
                query = query.Where(x => x.IsEmployerBorneTax == isEmployerBorneTax.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.NpwpNumber != null && x.NpwpNumber.ToLower().Contains(keyword)) ||
                    x.TaxStatus.ToLower().Contains(keyword) ||
                    x.TaxMethod.ToLower().Contains(keyword) ||
                    (x.TaxOfficeCode != null && x.TaxOfficeCode.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpTax> ApplySorting(
            IQueryable<WfpTax> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "taxstatus" => isDescending
                    ? query.OrderByDescending(x => x.TaxStatus)
                    : query.OrderBy(x => x.TaxStatus),
                "taxmethod" => isDescending
                    ? query.OrderByDescending(x => x.TaxMethod)
                    : query.OrderBy(x => x.TaxMethod),
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

        private WfpTaxResponse MapResponse(WfpTax entity)
        {
            return new WfpTaxResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                NpwpNumber = entity.NpwpNumber,
                TaxStatus = entity.TaxStatus,
                TaxMethod = entity.TaxMethod,
                TaxCountryCode = entity.TaxCountryCode,
                TaxOfficeCode = entity.TaxOfficeCode,
                IsNpwpRegistered = entity.IsNpwpRegistered,
                IsTaxResident = entity.IsTaxResident,
                HasPreviousEmployer = entity.HasPreviousEmployer,
                IsEmployerBorneTax = entity.IsEmployerBorneTax,
                PreviousEmployerTaxableIncome = entity.PreviousEmployerTaxableIncome,
                PreviousEmployerTaxPaid = entity.PreviousEmployerTaxPaid,
                AnnualNonTaxableIncome = entity.AnnualNonTaxableIncome,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserDisplayName(entity.CreateBy)
            };
        }

        private WfpTaxDetailResponse MapDetailResponse(WfpTax entity)
        {
            var response = MapResponse(entity);

            return new WfpTaxDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                WorkforceProfileCode = response.WorkforceProfileCode,
                WorkforceDisplayName = response.WorkforceDisplayName,
                NpwpNumber = response.NpwpNumber,
                TaxStatus = response.TaxStatus,
                TaxMethod = response.TaxMethod,
                TaxCountryCode = response.TaxCountryCode,
                TaxOfficeCode = response.TaxOfficeCode,
                IsNpwpRegistered = response.IsNpwpRegistered,
                IsTaxResident = response.IsTaxResident,
                HasPreviousEmployer = response.HasPreviousEmployer,
                IsEmployerBorneTax = response.IsEmployerBorneTax,
                PreviousEmployerTaxableIncome = response.PreviousEmployerTaxableIncome,
                PreviousEmployerTaxPaid = response.PreviousEmployerTaxPaid,
                AnnualNonTaxableIncome = response.AnnualNonTaxableIncome,
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
            CreateWfpTaxRequest request,
            Guid? currentId,
            CancellationToken cancellationToken)
        {
            if (!AllowedTaxStatuses.Contains(request.TaxStatus.Trim()))
                return (false, "TaxStatus tidak valid.");

            if (!AllowedTaxMethods.Contains(request.TaxMethod.Trim()))
                return (false, "TaxMethod tidak valid. Gunakan Gross, GrossUp, atau Net.");

            if (string.IsNullOrWhiteSpace(request.TaxCountryCode) ||
                request.TaxCountryCode.Trim().Length != 2)
            {
                return (false, "TaxCountryCode harus menggunakan kode negara dua karakter.");
            }

            if (request.IsNpwpRegistered && string.IsNullOrWhiteSpace(request.NpwpNumber))
                return (false, "Nomor NPWP wajib diisi ketika NPWP terdaftar.");

            if (request.PreviousEmployerTaxableIncome < 0 ||
                request.PreviousEmployerTaxPaid < 0 ||
                request.AnnualNonTaxableIncome < 0)
            {
                return (false, "Nilai perpajakan tidak boleh negatif.");
            }

            if (!request.HasPreviousEmployer &&
                (request.PreviousEmployerTaxableIncome > 0 || request.PreviousEmployerTaxPaid > 0))
            {
                return (false, "Data pemberi kerja sebelumnya hanya boleh diisi jika HasPreviousEmployer aktif.");
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            var npwpNumber = NormalizeNullableText(request.NpwpNumber);
            if (npwpNumber != null &&
                await _dbContext.Set<WfpTax>().AnyAsync(
                    x => !x.IsDelete &&
                         x.NpwpNumber == npwpNumber &&
                         (!currentId.HasValue || x.Id != currentId.Value),
                    cancellationToken))
            {
                return (false, "Nomor NPWP sudah digunakan oleh profil lain.");
            }

            return (true, null);
        }

        private void ApplyRequest(WfpTax entity, CreateWfpTaxRequest request)
        {
            entity.NpwpNumber = NormalizeNullableText(request.NpwpNumber);
            entity.TaxStatus = NormalizeAllowedValue(AllowedTaxStatuses, request.TaxStatus);
            entity.TaxMethod = NormalizeAllowedValue(AllowedTaxMethods, request.TaxMethod);
            entity.TaxCountryCode = request.TaxCountryCode.Trim().ToUpperInvariant();
            entity.TaxOfficeCode = NormalizeNullableText(request.TaxOfficeCode);
            entity.IsNpwpRegistered = request.IsNpwpRegistered;
            entity.IsTaxResident = request.IsTaxResident;
            entity.HasPreviousEmployer = request.HasPreviousEmployer;
            entity.IsEmployerBorneTax = request.IsEmployerBorneTax;
            entity.PreviousEmployerTaxableIncome = request.PreviousEmployerTaxableIncome;
            entity.PreviousEmployerTaxPaid = request.PreviousEmployerTaxPaid;
            entity.AnnualNonTaxableIncome = request.AnnualNonTaxableIncome;
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

        private static string NormalizeAllowedValue(
            IEnumerable<string> allowedValues,
            string value)
        {
            return allowedValues.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
