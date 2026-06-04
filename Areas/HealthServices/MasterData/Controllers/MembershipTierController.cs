using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseMembershipTierPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.MembershipTierResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/membership-tiers")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Membership Tier",
        AreaName = "HealthServices",
        ControllerName = "MembershipTier",
        Description = "Health service master data membership tier",
        SortOrder = 5
    )]
    [Tags("Health Services / Master Data / Membership Tier")]
    public class MembershipTierController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string TierCodePrefix = "MT-RSMMC-";
        private const int TierCodeDigitLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public MembershipTierController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Membership Tier", Description = "Melihat data membership tier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new MembershipTierFilterMetadataResponse
            {
                DefaultFilter = new MembershipTierDefaultFilterResponse(),
                CustomPeriods = new List<MembershipTierCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7Days", Label = "7 hari terakhir" },
                    new() { Value = "thisMonth", Label = "Bulan ini" },
                    new() { Value = "lastMonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<MembershipTierSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "tierCode", Label = "Kode tier" },
                    new() { Value = "tierName", Label = "Nama tier" },
                    new() { Value = "tierType", Label = "Tipe tier" },
                    new() { Value = "priorityLevel", Label = "Level prioritas" },
                    new() { Value = "validityMonths", Label = "Masa berlaku" },
                    new() { Value = "minimumSpendAmount", Label = "Minimum belanja" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                TierTypeOptions = BuildEnumOptions<MembershipTierType>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "MembershipTier.GetFilterMetadata",
                "Mengambil metadata filter membership tier.",
                result
            );

            return Ok(ApiResponse<MembershipTierFilterMetadataResponse>.Ok(
                result,
                "Metadata filter membership tier berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Membership Tier", Description = "Melihat data membership tier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new MembershipTierSummaryResponse
            {
                TotalMembershipTier = await query.CountAsync(),
                ActiveMembershipTier = await query.CountAsync(x => x.IsActive),
                InactiveMembershipTier = await query.CountAsync(x => !x.IsActive),
                DefaultTier = await query.CountAsync(x => x.IsDefault),
                SelectableInKiosk = await query.CountAsync(x => x.IsSelectableInKiosk),
                SelectableInAdmission = await query.CountAsync(x => x.IsSelectableInAdmission),
                MarketingManagedOnly = await query.CountAsync(x => x.IsManagedByMarketingOnly),
                PriorityQueueTier = await query.CountAsync(x => x.PriorityQueue),
                FreeAnnualCheckupTier = await query.CountAsync(x => x.FreeAnnualCheckup),
                FreeParkingTier = await query.CountAsync(x => x.FreeParking)
            };

            return Ok(ApiResponse<MembershipTierSummaryResponse>.Ok(
                result,
                "Ringkasan membership tier berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseMembershipTierPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Membership Tier", Description = "Melihat data membership tier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetMembershipTiers(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TierCode.ToLower().Contains(keyword) ||
                    x.TierName.ToLower().Contains(keyword) ||
                    x.TierType.ToString().ToLower().Contains(keyword) ||
                    (x.CardTitle != null && x.CardTitle.ToLower().Contains(keyword)) ||
                    (x.CardColor != null && x.CardColor.ToLower().Contains(keyword)) ||
                    (x.BenefitDescription != null && x.BenefitDescription.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseMembershipTierPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseMembershipTierPagedResult>.Ok(
                result,
                "Data membership tier berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Membership Tier", Description = "Melihat data membership tier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetMembershipTierOptions(
    [FromQuery] bool onlyActive = true,
    [FromQuery] string? search = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TierCode.ToLower().Contains(keyword) ||
                    x.TierName.ToLower().Contains(keyword) ||
                    x.TierType.ToString().ToLower().Contains(keyword) ||
                    (x.CardTitle != null && x.CardTitle.ToLower().Contains(keyword)) ||
                    (x.CardColor != null && x.CardColor.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.PriorityLevel)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.TierName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MembershipTierOptionResponse
                {
                    Id = x.Id,
                    TierCode = x.TierCode,
                    TierName = x.TierName,
                    TierType = x.TierType,
                    TierTypeName = x.TierType.ToString(),
                    CardTitle = x.CardTitle,
                    CardColor = x.CardColor,
                    PriorityLevel = x.PriorityLevel,
                    IsDefault = x.IsDefault,
                    IsSelectableInKiosk = x.IsSelectableInKiosk,
                    IsSelectableInAdmission = x.IsSelectableInAdmission,
                    PriorityQueue = x.PriorityQueue,
                    ValidityMonths = x.ValidityMonths
                })
                .ToListAsync();

            var result = new MembershipTierOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<MembershipTierOptionPagedResponse>.Ok(
                result,
                "Data pilihan membership tier berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Membership Tier", Description = "Melihat detail membership tier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetMembershipTierById(Guid id)
        {
            var data = await _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new MembershipTierDetailResponse
                {
                    Id = x.Id,
                    TierCode = x.TierCode,
                    TierName = x.TierName,
                    TierType = x.TierType,
                    TierTypeName = x.TierType.ToString(),
                    CardTitle = x.CardTitle,
                    CardColor = x.CardColor,
                    CardImagePath = x.CardImagePath,
                    PriorityLevel = x.PriorityLevel,
                    IsDefault = x.IsDefault,
                    IsSelectableInKiosk = x.IsSelectableInKiosk,
                    IsSelectableInAdmission = x.IsSelectableInAdmission,
                    IsManagedByMarketingOnly = x.IsManagedByMarketingOnly,
                    RegistrationDiscountPercent = x.RegistrationDiscountPercent,
                    ConsultationDiscountPercent = x.ConsultationDiscountPercent,
                    ProcedureDiscountPercent = x.ProcedureDiscountPercent,
                    LaboratoryDiscountPercent = x.LaboratoryDiscountPercent,
                    RadiologyDiscountPercent = x.RadiologyDiscountPercent,
                    PharmacyDiscountPercent = x.PharmacyDiscountPercent,
                    PriorityQueue = x.PriorityQueue,
                    FreeAnnualCheckup = x.FreeAnnualCheckup,
                    FreeParking = x.FreeParking,
                    ValidityMonths = x.ValidityMonths,
                    MinimumSpendAmount = x.MinimumSpendAmount,
                    SortOrder = x.SortOrder,
                    BenefitDescription = x.BenefitDescription,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Membership tier tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<MembershipTierDetailResponse>.Ok(
                data,
                "Detail membership tier berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Membership Tier", Description = "Membuat data membership tier", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MembershipTier", "Create")]
        public async Task<IActionResult> CreateMembershipTier([FromBody] CreateMembershipTierRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                tierName: request.TierName,
                tierType: request.TierType,
                priorityLevel: request.PriorityLevel,
                validityMonths: request.ValidityMonths,
                minimumSpendAmount: request.MinimumSpendAmount,
                registrationDiscountPercent: request.RegistrationDiscountPercent,
                consultationDiscountPercent: request.ConsultationDiscountPercent,
                procedureDiscountPercent: request.ProcedureDiscountPercent,
                laboratoryDiscountPercent: request.LaboratoryDiscountPercent,
                radiologyDiscountPercent: request.RadiologyDiscountPercent,
                pharmacyDiscountPercent: request.PharmacyDiscountPercent
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data membership tier tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsDefault)
            {
                await ClearOtherDefaultTiersAsync(null, now, actorUserId);
            }

            var entity = new MstMembershipTier
            {
                Id = Guid.NewGuid(),
                TierCode = await GenerateTierCodeAsync(),
                TierName = request.TierName.Trim(),
                TierType = request.TierType,
                CardTitle = NormalizeNullableString(request.CardTitle),
                CardColor = NormalizeNullableString(request.CardColor),
                CardImagePath = NormalizeNullableString(request.CardImagePath),
                PriorityLevel = request.PriorityLevel,
                IsDefault = request.IsDefault,
                IsSelectableInKiosk = request.IsSelectableInKiosk,
                IsSelectableInAdmission = request.IsSelectableInAdmission,
                IsManagedByMarketingOnly = request.IsManagedByMarketingOnly,
                RegistrationDiscountPercent = request.RegistrationDiscountPercent,
                ConsultationDiscountPercent = request.ConsultationDiscountPercent,
                ProcedureDiscountPercent = request.ProcedureDiscountPercent,
                LaboratoryDiscountPercent = request.LaboratoryDiscountPercent,
                RadiologyDiscountPercent = request.RadiologyDiscountPercent,
                PharmacyDiscountPercent = request.PharmacyDiscountPercent,
                PriorityQueue = request.PriorityQueue,
                FreeAnnualCheckup = request.FreeAnnualCheckup,
                FreeParking = request.FreeParking,
                ValidityMonths = request.ValidityMonths,
                MinimumSpendAmount = request.MinimumSpendAmount,
                SortOrder = request.SortOrder,
                BenefitDescription = NormalizeNullableString(request.BenefitDescription),
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstMembershipTier>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new MembershipTierCreateResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                TierType = entity.TierType,
                TierTypeName = entity.TierType.ToString(),
                PriorityLevel = entity.PriorityLevel,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "MembershipTier.CreateMembershipTier",
                "Membuat data membership tier.",
                result
            );

            return Ok(ApiResponse<MembershipTierCreateResponse>.Ok(
                result,
                "Membership tier berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Membership Tier", Description = "Mengubah data membership tier", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MembershipTier", "Update")]
        public async Task<IActionResult> UpdateMembershipTier(Guid id, [FromBody] UpdateMembershipTierRequest request)
        {
            var entity = await _dbContext.Set<MstMembershipTier>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Membership tier tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                tierName: request.TierName,
                tierType: request.TierType,
                priorityLevel: request.PriorityLevel,
                validityMonths: request.ValidityMonths,
                minimumSpendAmount: request.MinimumSpendAmount,
                registrationDiscountPercent: request.RegistrationDiscountPercent,
                consultationDiscountPercent: request.ConsultationDiscountPercent,
                procedureDiscountPercent: request.ProcedureDiscountPercent,
                laboratoryDiscountPercent: request.LaboratoryDiscountPercent,
                radiologyDiscountPercent: request.RadiologyDiscountPercent,
                pharmacyDiscountPercent: request.PharmacyDiscountPercent
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data membership tier tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsDefault)
            {
                await ClearOtherDefaultTiersAsync(id, now, actorUserId);
            }

            entity.TierName = request.TierName.Trim();
            entity.TierType = request.TierType;
            entity.CardTitle = NormalizeNullableString(request.CardTitle);
            entity.CardColor = NormalizeNullableString(request.CardColor);
            entity.CardImagePath = NormalizeNullableString(request.CardImagePath);
            entity.PriorityLevel = request.PriorityLevel;
            entity.IsDefault = request.IsDefault;
            entity.IsSelectableInKiosk = request.IsSelectableInKiosk;
            entity.IsSelectableInAdmission = request.IsSelectableInAdmission;
            entity.IsManagedByMarketingOnly = request.IsManagedByMarketingOnly;
            entity.RegistrationDiscountPercent = request.RegistrationDiscountPercent;
            entity.ConsultationDiscountPercent = request.ConsultationDiscountPercent;
            entity.ProcedureDiscountPercent = request.ProcedureDiscountPercent;
            entity.LaboratoryDiscountPercent = request.LaboratoryDiscountPercent;
            entity.RadiologyDiscountPercent = request.RadiologyDiscountPercent;
            entity.PharmacyDiscountPercent = request.PharmacyDiscountPercent;
            entity.PriorityQueue = request.PriorityQueue;
            entity.FreeAnnualCheckup = request.FreeAnnualCheckup;
            entity.FreeParking = request.FreeParking;
            entity.ValidityMonths = request.ValidityMonths;
            entity.MinimumSpendAmount = request.MinimumSpendAmount;
            entity.SortOrder = request.SortOrder;
            entity.BenefitDescription = NormalizeNullableString(request.BenefitDescription);
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new MembershipTierUpdateResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                TierType = entity.TierType,
                TierTypeName = entity.TierType.ToString(),
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "MembershipTier.UpdateMembershipTier",
                "Mengubah data membership tier.",
                result
            );

            return Ok(ApiResponse<MembershipTierUpdateResponse>.Ok(
                result,
                "Membership tier berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Membership Tier", Description = "Menghapus data membership tier", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("MembershipTier", "Delete")]
        public async Task<IActionResult> DeleteMembershipTier(Guid id)
        {
            var entity = await _dbContext.Set<MstMembershipTier>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Membership tier tidak ditemukan."
                ));
            }

            var isUsedByPatientDefaultTier = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.DefaultMembershipTierId == id && !x.IsDelete);

            if (isUsedByPatientDefaultTier)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Membership tier tidak dapat dihapus karena sudah digunakan sebagai default membership pasien."
                ));
            }

            var isUsedByPatientMembership = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .AnyAsync(x => x.MembershipTierId == id && !x.IsDelete);

            if (isUsedByPatientMembership)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Membership tier tidak dapat dihapus karena sudah digunakan oleh membership pasien."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new MembershipTierDeleteResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "MembershipTier.DeleteMembershipTier",
                "Menghapus data membership tier.",
                result
            );

            return Ok(ApiResponse<MembershipTierDeleteResponse>.Ok(
                result,
                "Membership tier berhasil dihapus."
            ));
        }

        private static IQueryable<MstMembershipTier> ApplyDateFilter(
            IQueryable<MstMembershipTier> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var now = DateTime.UtcNow.Date;

            if (!string.IsNullOrWhiteSpace(customPeriod))
            {
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        startDate = now;
                        endDate = now;
                        break;

                    case "last7days":
                        startDate = now.AddDays(-6);
                        endDate = now;
                        break;

                    case "thismonth":
                        startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                        break;

                    case "lastmonth":
                        var firstDayThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        startDate = firstDayThisMonth.AddMonths(-1);
                        endDate = firstDayThisMonth.AddDays(-1);
                        break;
                }
            }

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreateDateTime <= end);
            }

            return query;
        }

        private async Task<string> GenerateTierCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.TierCode.StartsWith(TierCodePrefix))
                .Select(x => x.TierCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractTierSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{TierCodePrefix}{nextNumber.ToString().PadLeft(TierCodeDigitLength, '0')}";
        }

        private static int? TryExtractTierSequenceNumber(string tierCode)
        {
            if (string.IsNullOrWhiteSpace(tierCode))
                return null;

            if (!tierCode.StartsWith(TierCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = tierCode[TierCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private async Task ClearOtherDefaultTiersAsync(Guid? excludeId, DateTime now, Guid actorUserId)
        {
            var query = _dbContext.Set<MstMembershipTier>()
                .Where(x => !x.IsDelete && x.IsDefault);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            var otherDefaultTiers = await query.ToListAsync();

            foreach (var tier in otherDefaultTiers)
            {
                tier.IsDefault = false;
                tier.UpdateDateTime = now;
                tier.UpdateBy = actorUserId;
            }
        }

        private static IOrderedQueryable<MstMembershipTier> ApplySorting(
            IQueryable<MstMembershipTier> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "tiercode" => isDesc
                    ? query.OrderByDescending(x => x.TierCode)
                    : query.OrderBy(x => x.TierCode),

                "tiername" => isDesc
                    ? query.OrderByDescending(x => x.TierName)
                    : query.OrderBy(x => x.TierName),

                "tiertype" => isDesc
                    ? query.OrderByDescending(x => x.TierType)
                    : query.OrderBy(x => x.TierType),

                "prioritylevel" => isDesc
                    ? query.OrderByDescending(x => x.PriorityLevel)
                    : query.OrderBy(x => x.PriorityLevel),

                "validitymonths" => isDesc
                    ? query.OrderByDescending(x => x.ValidityMonths)
                    : query.OrderBy(x => x.ValidityMonths),

                "minimumspendamount" => isDesc
                    ? query.OrderByDescending(x => x.MinimumSpendAmount)
                    : query.OrderBy(x => x.MinimumSpendAmount),

                "isdefault" => isDesc
                    ? query.OrderByDescending(x => x.IsDefault)
                    : query.OrderBy(x => x.IsDefault),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.PriorityLevel).ThenByDescending(x => x.TierName)
                    : query.OrderBy(x => x.SortOrder).ThenByDescending(x => x.PriorityLevel).ThenBy(x => x.TierName)
            };
        }

        private static MembershipTierResponse ToResponse(MstMembershipTier x)
        {
            return new MembershipTierResponse
            {
                Id = x.Id,
                TierCode = x.TierCode,
                TierName = x.TierName,
                TierType = x.TierType,
                TierTypeName = x.TierType.ToString(),
                CardTitle = x.CardTitle,
                CardColor = x.CardColor,
                CardImagePath = x.CardImagePath,
                PriorityLevel = x.PriorityLevel,
                IsDefault = x.IsDefault,
                IsSelectableInKiosk = x.IsSelectableInKiosk,
                IsSelectableInAdmission = x.IsSelectableInAdmission,
                IsManagedByMarketingOnly = x.IsManagedByMarketingOnly,
                RegistrationDiscountPercent = x.RegistrationDiscountPercent,
                ConsultationDiscountPercent = x.ConsultationDiscountPercent,
                ProcedureDiscountPercent = x.ProcedureDiscountPercent,
                LaboratoryDiscountPercent = x.LaboratoryDiscountPercent,
                RadiologyDiscountPercent = x.RadiologyDiscountPercent,
                PharmacyDiscountPercent = x.PharmacyDiscountPercent,
                PriorityQueue = x.PriorityQueue,
                FreeAnnualCheckup = x.FreeAnnualCheckup,
                FreeParking = x.FreeParking,
                ValidityMonths = x.ValidityMonths,
                MinimumSpendAmount = x.MinimumSpendAmount,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string tierName,
            MembershipTierType tierType,
            int priorityLevel,
            int validityMonths,
            decimal minimumSpendAmount,
            decimal registrationDiscountPercent,
            decimal consultationDiscountPercent,
            decimal procedureDiscountPercent,
            decimal laboratoryDiscountPercent,
            decimal radiologyDiscountPercent,
            decimal pharmacyDiscountPercent)
        {
            if (string.IsNullOrWhiteSpace(tierName))
                return (false, "Nama membership tier wajib diisi.");

            if (!Enum.IsDefined(typeof(MembershipTierType), tierType))
                return (false, "Tipe membership tier tidak valid.");

            if (priorityLevel < 0)
                return (false, "Priority level tidak boleh kurang dari 0.");

            if (validityMonths < 1)
                return (false, "Masa berlaku membership minimal 1 bulan.");

            if (minimumSpendAmount < 0)
                return (false, "Minimum spend amount tidak boleh kurang dari 0.");

            var discounts = new Dictionary<string, decimal>
            {
                { "Diskon registrasi", registrationDiscountPercent },
                { "Diskon konsultasi", consultationDiscountPercent },
                { "Diskon tindakan", procedureDiscountPercent },
                { "Diskon laboratorium", laboratoryDiscountPercent },
                { "Diskon radiologi", radiologyDiscountPercent },
                { "Diskon farmasi", pharmacyDiscountPercent }
            };

            foreach (var discount in discounts)
            {
                if (discount.Value < 0 || discount.Value > 100)
                    return (false, $"{discount.Key} harus berada di antara 0 sampai 100 persen.");
            }

            var normalizedName = tierName.Trim().ToLower();

            var duplicateName = await _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.TierName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama membership tier sudah digunakan.");

            return (true, null);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<MembershipTierEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new MembershipTierEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
