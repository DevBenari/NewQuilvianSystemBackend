using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Claims;

using ResponseMembershipTierPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.MembershipTierResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/membership-tiers")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Membership Tier",
        AreaName = "Administrator",
        ControllerName = "MembershipTier",
        Description = "Administrator master data membership tier",
        SortOrder = 5
    )]
    [Tags("Administrator / Master Data / Membership Tier")]
    public class MembershipTierController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
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
        [AccessAction(
            "Read",
            "Read Membership Tier",
            Description = "Melihat metadata filter membership tier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new MembershipTierFilterMetadataResponse
            {
                DefaultFilter = new MembershipTierDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<MembershipTierSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "tierCode", Label = "Kode tier" },
                    new() { Value = "tierName", Label = "Nama tier" },
                    new() { Value = "tierType", Label = "Tipe tier" },
                    new() { Value = "cardTitle", Label = "Judul kartu" },
                    new() { Value = "priorityLevel", Label = "Level prioritas" },
                    new() { Value = "validityMonths", Label = "Masa berlaku" },
                    new() { Value = "minimumSpendAmount", Label = "Minimum belanja" },
                    new() { Value = "registrationDiscountPercent", Label = "Diskon registrasi" },
                    new() { Value = "consultationDiscountPercent", Label = "Diskon konsultasi" },
                    new() { Value = "procedureDiscountPercent", Label = "Diskon tindakan" },
                    new() { Value = "laboratoryDiscountPercent", Label = "Diskon laboratorium" },
                    new() { Value = "radiologyDiscountPercent", Label = "Diskon radiologi" },
                    new() { Value = "pharmacyDiscountPercent", Label = "Diskon farmasi" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isSelectableInKiosk", Label = "Tampil di kiosk" },
                    new() { Value = "isSelectableInAdmission", Label = "Tampil di admission" },
                    new() { Value = "isManagedByMarketingOnly", Label = "Khusus marketing" },
                    new() { Value = "priorityQueue", Label = "Prioritas antrian" },
                    new() { Value = "freeAnnualCheckup", Label = "Free annual checkup" },
                    new() { Value = "freeParking", Label = "Free parking" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                TierTypeOptions = BuildEnumOptions<MembershipTierType>(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
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
        [AccessAction(
            "Read",
            "Read Membership Tier",
            Description = "Melihat ringkasan membership tier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new MembershipTierSummaryResponse
            {
                TotalMembershipTier = await query.CountAsync(),
                ActiveMembershipTier = await query.CountAsync(x => x.IsActive),
                InactiveMembershipTier = await query.CountAsync(x => !x.IsActive),
                DefaultTier = await query.CountAsync(x => x.IsDefault),
                RegularTier = await query.CountAsync(x => x.TierType == MembershipTierType.Regular),
                SilverTier = await query.CountAsync(x => x.TierType == MembershipTierType.Silver),
                GoldTier = await query.CountAsync(x => x.TierType == MembershipTierType.Gold),
                PlatinumTier = await query.CountAsync(x => x.TierType == MembershipTierType.Platinum),
                ExecutiveTier = await query.CountAsync(x => x.TierType == MembershipTierType.Executive),
                CorporateTier = await query.CountAsync(x => x.TierType == MembershipTierType.Corporate),
                FamilyTier = await query.CountAsync(x => x.TierType == MembershipTierType.Family),
                OtherTier = await query.CountAsync(x => x.TierType == MembershipTierType.Other),
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Membership Tier",
            Description = "Melihat data membership tier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetMembershipTiers(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] MembershipTierType? tierType,
            [FromQuery] bool? isDefault,
            [FromQuery] bool? isSelectableInKiosk,
            [FromQuery] bool? isSelectableInAdmission,
            [FromQuery] bool? isManagedByMarketingOnly,
            [FromQuery] bool? priorityQueue,
            [FromQuery] bool? freeAnnualCheckup,
            [FromQuery] bool? freeParking,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                search,
                isActive,
                tierType,
                isDefault,
                isSelectableInKiosk,
                isSelectableInAdmission,
                isManagedByMarketingOnly,
                priorityQueue,
                freeAnnualCheckup,
                freeParking
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

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
        [AccessAction(
            "Read",
            "Read Membership Tier",
            Description = "Melihat data pilihan membership tier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetMembershipTierOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] MembershipTierType? tierType = null,
            [FromQuery] bool? isDefault = null,
            [FromQuery] bool? isSelectableInKiosk = null,
            [FromQuery] bool? isSelectableInAdmission = null,
            [FromQuery] bool? isManagedByMarketingOnly = null,
            [FromQuery] bool? priorityQueue = null,
            [FromQuery] bool? freeAnnualCheckup = null,
            [FromQuery] bool? freeParking = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var useOnlyActive = activeOnly ?? onlyActive;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                search,
                useOnlyActive ? true : null,
                tierType,
                isDefault,
                isSelectableInKiosk,
                isSelectableInAdmission,
                isManagedByMarketingOnly,
                priorityQueue,
                freeAnnualCheckup,
                freeParking
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.PriorityLevel)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.TierName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

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
        [AccessAction(
            "Read",
            "Read Membership Tier",
            Description = "Melihat detail membership tier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetMembershipTierById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Membership tier tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<MembershipTierDetailResponse>.Ok(
                data,
                "Detail membership tier berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Membership Tier",
            Description = "Membuat data membership tier",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("MembershipTier", "Create")]
        public async Task<IActionResult> CreateMembershipTier([FromBody] CreateMembershipTierRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            if (request.IsDefault)
            {
                await ClearOtherDefaultTiersAsync(null, now, actorUserId);
            }

            var generatedTierCode = await GenerateTierCodeAsync();

            var entity = new MstMembershipTier
            {
                Id = Guid.NewGuid(),
                TierCode = generatedTierCode,
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
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });

            var result = new MembershipTierCreateResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                TierType = entity.TierType,
                TierTypeName = BuildMembershipTierTypeLabel(entity.TierType),
                PriorityLevel = entity.PriorityLevel,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
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
        [AccessAction(
            "Update",
            "Update Membership Tier",
            Description = "Mengubah data membership tier",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("MembershipTier", "Update")]
        public async Task<IActionResult> UpdateMembershipTier(
            Guid id,
            [FromBody] UpdateMembershipTierRequest request)
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

            if (request.IsDefault && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Membership tier default harus aktif."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

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
            entity.IsDefault = request.IsActive ? request.IsDefault : false;
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
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new MembershipTierUpdateResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                TierType = entity.TierType,
                TierTypeName = BuildMembershipTierTypeLabel(entity.TierType),
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
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

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Membership Tier Status",
            Description = "Mengubah status membership tier",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("MembershipTier", "Update")]
        public async Task<IActionResult> UpdateMembershipTierStatus(
            Guid id,
            [FromBody] UpdateMembershipTierStatusRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;

            if (!request.IsActive)
            {
                entity.IsDefault = false;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new MembershipTierStatusResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<MembershipTierStatusResponse>.Ok(
                result,
                request.IsActive
                    ? "Membership tier berhasil diaktifkan."
                    : "Membership tier berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Membership Tier",
            Description = "Menghapus data membership tier",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("MembershipTier", "Delete")]
        public async Task<IActionResult> DeleteMembershipTier(
            Guid id,
            [FromBody] DeleteMembershipTierRequest? request = null)
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
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = NormalizeNullableString(request.DeleteReason);
            }

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.DeleteBy });

            var result = new MembershipTierDeleteResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
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

        private IQueryable<MstMembershipTier> BuildBaseQuery()
        {
            return _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstMembershipTier> ApplyDateFilter(
            IQueryable<MstMembershipTier> query,
            DateRangeResolveResult dateRange)
        {
            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            return query;
        }

        private static IQueryable<MstMembershipTier> ApplyStandardFilter(
            IQueryable<MstMembershipTier> query,
            string? search,
            bool? isActive,
            MembershipTierType? tierType,
            bool? isDefault,
            bool? isSelectableInKiosk,
            bool? isSelectableInAdmission,
            bool? isManagedByMarketingOnly,
            bool? priorityQueue,
            bool? freeAnnualCheckup,
            bool? freeParking)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedTierTypes = Enum.GetValues(typeof(MembershipTierType))
                    .Cast<MembershipTierType>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildMembershipTierTypeLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.TierCode.ToLower().Contains(keyword) ||
                    x.TierName.ToLower().Contains(keyword) ||
                    (x.CardTitle != null && x.CardTitle.ToLower().Contains(keyword)) ||
                    (x.CardColor != null && x.CardColor.ToLower().Contains(keyword)) ||
                    (x.CardImagePath != null && x.CardImagePath.ToLower().Contains(keyword)) ||
                    (x.BenefitDescription != null && x.BenefitDescription.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    matchedTierTypes.Contains(x.TierType));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (tierType.HasValue)
            {
                query = query.Where(x => x.TierType == tierType.Value);
            }

            if (isDefault.HasValue)
            {
                query = query.Where(x => x.IsDefault == isDefault.Value);
            }

            if (isSelectableInKiosk.HasValue)
            {
                query = query.Where(x => x.IsSelectableInKiosk == isSelectableInKiosk.Value);
            }

            if (isSelectableInAdmission.HasValue)
            {
                query = query.Where(x => x.IsSelectableInAdmission == isSelectableInAdmission.Value);
            }

            if (isManagedByMarketingOnly.HasValue)
            {
                query = query.Where(x => x.IsManagedByMarketingOnly == isManagedByMarketingOnly.Value);
            }

            if (priorityQueue.HasValue)
            {
                query = query.Where(x => x.PriorityQueue == priorityQueue.Value);
            }

            if (freeAnnualCheckup.HasValue)
            {
                query = query.Where(x => x.FreeAnnualCheckup == freeAnnualCheckup.Value);
            }

            if (freeParking.HasValue)
            {
                query = query.Where(x => x.FreeParking == freeParking.Value);
            }

            return query;
        }

        private static IOrderedQueryable<MstMembershipTier> ApplySorting(
            IQueryable<MstMembershipTier> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "tiercode" => isDescending
                    ? query.OrderByDescending(x => x.TierCode)
                    : query.OrderBy(x => x.TierCode),

                "tiername" => isDescending
                    ? query.OrderByDescending(x => x.TierName)
                    : query.OrderBy(x => x.TierName),

                "tiertype" => isDescending
                    ? query.OrderByDescending(x => x.TierType).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.TierType).ThenBy(x => x.TierName),

                "cardtitle" => isDescending
                    ? query.OrderByDescending(x => x.CardTitle).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.CardTitle).ThenBy(x => x.TierName),

                "prioritylevel" => isDescending
                    ? query.OrderByDescending(x => x.PriorityLevel).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.PriorityLevel).ThenBy(x => x.TierName),

                "validitymonths" => isDescending
                    ? query.OrderByDescending(x => x.ValidityMonths).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.ValidityMonths).ThenBy(x => x.TierName),

                "minimumspendamount" => isDescending
                    ? query.OrderByDescending(x => x.MinimumSpendAmount).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.MinimumSpendAmount).ThenBy(x => x.TierName),

                "registrationdiscountpercent" => isDescending
                    ? query.OrderByDescending(x => x.RegistrationDiscountPercent).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.RegistrationDiscountPercent).ThenBy(x => x.TierName),

                "consultationdiscountpercent" => isDescending
                    ? query.OrderByDescending(x => x.ConsultationDiscountPercent).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.ConsultationDiscountPercent).ThenBy(x => x.TierName),

                "procedurediscountpercent" => isDescending
                    ? query.OrderByDescending(x => x.ProcedureDiscountPercent).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.ProcedureDiscountPercent).ThenBy(x => x.TierName),

                "laboratorydiscountpercent" => isDescending
                    ? query.OrderByDescending(x => x.LaboratoryDiscountPercent).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.LaboratoryDiscountPercent).ThenBy(x => x.TierName),

                "radiologydiscountpercent" => isDescending
                    ? query.OrderByDescending(x => x.RadiologyDiscountPercent).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.RadiologyDiscountPercent).ThenBy(x => x.TierName),

                "pharmacydiscountpercent" => isDescending
                    ? query.OrderByDescending(x => x.PharmacyDiscountPercent).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.PharmacyDiscountPercent).ThenBy(x => x.TierName),

                "isdefault" => isDescending
                    ? query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.IsDefault).ThenBy(x => x.TierName),

                "isselectableinkiosk" => isDescending
                    ? query.OrderByDescending(x => x.IsSelectableInKiosk).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.IsSelectableInKiosk).ThenBy(x => x.TierName),

                "isselectableinadmission" => isDescending
                    ? query.OrderByDescending(x => x.IsSelectableInAdmission).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.IsSelectableInAdmission).ThenBy(x => x.TierName),

                "ismanagedbymarketingonly" => isDescending
                    ? query.OrderByDescending(x => x.IsManagedByMarketingOnly).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.IsManagedByMarketingOnly).ThenBy(x => x.TierName),

                "priorityqueue" => isDescending
                    ? query.OrderByDescending(x => x.PriorityQueue).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.PriorityQueue).ThenBy(x => x.TierName),

                "freeannualcheckup" => isDescending
                    ? query.OrderByDescending(x => x.FreeAnnualCheckup).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.FreeAnnualCheckup).ThenBy(x => x.TierName),

                "freeparking" => isDescending
                    ? query.OrderByDescending(x => x.FreeParking).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.FreeParking).ThenBy(x => x.TierName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.TierName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.TierName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.PriorityLevel).ThenByDescending(x => x.TierName)
                    : query.OrderBy(x => x.SortOrder).ThenByDescending(x => x.PriorityLevel).ThenBy(x => x.TierName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateMembershipTierRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TierName))
            {
                return (false, "Nama membership tier wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(MembershipTierType), request.TierType))
            {
                return (false, "Tipe membership tier tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.PriorityLevel < 0)
            {
                return (false, "Priority level tidak boleh kurang dari 0.");
            }

            if (request.ValidityMonths < 1)
            {
                return (false, "Masa berlaku membership minimal 1 bulan.");
            }

            if (request.MinimumSpendAmount < 0)
            {
                return (false, "Minimum spend amount tidak boleh kurang dari 0.");
            }

            if (request.SortOrder < 0)
            {
                return (false, "Urutan tidak boleh kurang dari 0.");
            }

            var discounts = new Dictionary<string, decimal>
            {
                { "Diskon registrasi", request.RegistrationDiscountPercent },
                { "Diskon konsultasi", request.ConsultationDiscountPercent },
                { "Diskon tindakan", request.ProcedureDiscountPercent },
                { "Diskon laboratorium", request.LaboratoryDiscountPercent },
                { "Diskon radiologi", request.RadiologyDiscountPercent },
                { "Diskon farmasi", request.PharmacyDiscountPercent }
            };

            foreach (var discount in discounts)
            {
                if (discount.Value < 0 || discount.Value > 100)
                {
                    return (false, $"{discount.Key} harus berada di antara 0 sampai 100 persen.");
                }
            }

            var normalizedName = request.TierName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.TierName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama membership tier sudah digunakan.");
            }

            return (true, null);
        }

        private async Task ClearOtherDefaultTiersAsync(
            Guid? excludeId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstMembershipTier>()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsDefault);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            var otherDefaultTiers = await query.ToListAsync();

            foreach (var tier in otherDefaultTiers)
            {
                tier.IsDefault = false;
                tier.UpdateDateTime = now;
                tier.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateTierCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstMembershipTier>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.TierCode.StartsWith(TierCodePrefix))
                .Select(x => x.TierCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractTierSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return TierCodePrefix + nextNumber.ToString("D" + TierCodeDigitLength, CultureInfo.InvariantCulture);
        }

        private static int? TryExtractTierSequenceNumber(string tierCode)
        {
            if (string.IsNullOrWhiteSpace(tierCode))
            {
                return null;
            }

            if (!tierCode.StartsWith(TierCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = tierCode[TierCodePrefix.Length..];

            return int.TryParse(numberText, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                ? number
                : null;
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static MembershipTierResponse MapResponse(
            MstMembershipTier entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new MembershipTierResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                TierType = entity.TierType,
                TierTypeName = BuildMembershipTierTypeLabel(entity.TierType),
                CardTitle = entity.CardTitle,
                CardColor = entity.CardColor,
                CardImagePath = entity.CardImagePath,
                PriorityLevel = entity.PriorityLevel,
                IsDefault = entity.IsDefault,
                IsSelectableInKiosk = entity.IsSelectableInKiosk,
                IsSelectableInAdmission = entity.IsSelectableInAdmission,
                IsManagedByMarketingOnly = entity.IsManagedByMarketingOnly,
                RegistrationDiscountPercent = entity.RegistrationDiscountPercent,
                ConsultationDiscountPercent = entity.ConsultationDiscountPercent,
                ProcedureDiscountPercent = entity.ProcedureDiscountPercent,
                LaboratoryDiscountPercent = entity.LaboratoryDiscountPercent,
                RadiologyDiscountPercent = entity.RadiologyDiscountPercent,
                PharmacyDiscountPercent = entity.PharmacyDiscountPercent,
                PriorityQueue = entity.PriorityQueue,
                FreeAnnualCheckup = entity.FreeAnnualCheckup,
                FreeParking = entity.FreeParking,
                ValidityMonths = entity.ValidityMonths,
                MinimumSpendAmount = entity.MinimumSpendAmount,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static MembershipTierDetailResponse MapDetailResponse(
            MstMembershipTier entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new MembershipTierDetailResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                TierType = entity.TierType,
                TierTypeName = BuildMembershipTierTypeLabel(entity.TierType),
                CardTitle = entity.CardTitle,
                CardColor = entity.CardColor,
                CardImagePath = entity.CardImagePath,
                PriorityLevel = entity.PriorityLevel,
                IsDefault = entity.IsDefault,
                IsSelectableInKiosk = entity.IsSelectableInKiosk,
                IsSelectableInAdmission = entity.IsSelectableInAdmission,
                IsManagedByMarketingOnly = entity.IsManagedByMarketingOnly,
                RegistrationDiscountPercent = entity.RegistrationDiscountPercent,
                ConsultationDiscountPercent = entity.ConsultationDiscountPercent,
                ProcedureDiscountPercent = entity.ProcedureDiscountPercent,
                LaboratoryDiscountPercent = entity.LaboratoryDiscountPercent,
                RadiologyDiscountPercent = entity.RadiologyDiscountPercent,
                PharmacyDiscountPercent = entity.PharmacyDiscountPercent,
                PriorityQueue = entity.PriorityQueue,
                FreeAnnualCheckup = entity.FreeAnnualCheckup,
                FreeParking = entity.FreeParking,
                ValidityMonths = entity.ValidityMonths,
                MinimumSpendAmount = entity.MinimumSpendAmount,
                SortOrder = entity.SortOrder,
                BenefitDescription = entity.BenefitDescription,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static MembershipTierOptionResponse MapOptionResponse(MstMembershipTier entity)
        {
            return new MembershipTierOptionResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                TierType = entity.TierType,
                TierTypeName = BuildMembershipTierTypeLabel(entity.TierType),
                CardTitle = entity.CardTitle,
                CardColor = entity.CardColor,
                PriorityLevel = entity.PriorityLevel,
                IsDefault = entity.IsDefault,
                IsSelectableInKiosk = entity.IsSelectableInKiosk,
                IsSelectableInAdmission = entity.IsSelectableInAdmission,
                IsManagedByMarketingOnly = entity.IsManagedByMarketingOnly,
                PriorityQueue = entity.PriorityQueue,
                FreeAnnualCheckup = entity.FreeAnnualCheckup,
                FreeParking = entity.FreeParking,
                ValidityMonths = entity.ValidityMonths,
                MinimumSpendAmount = entity.MinimumSpendAmount,
                SortOrder = entity.SortOrder
            };
        }

        private static List<MembershipTierCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<MembershipTierCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
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

        private static string BuildMembershipTierTypeLabel(MembershipTierType value)
        {
            return SplitPascalCase(value.ToString());
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<MembershipTierQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<MembershipTierQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, judul kartu, warna kartu, tipe tier, benefit, atau deskripsi.", Example = "Gold" },
                new() { Name = "tierType", Type = "enum", Description = "Filter berdasarkan tipe membership tier.", Example = "3" },
                new() { Name = "isDefault", Type = "bool", Description = "Filter tier default.", Example = "true" },
                new() { Name = "isSelectableInKiosk", Type = "bool", Description = "Filter tier yang dapat dipilih di kiosk.", Example = "true" },
                new() { Name = "isSelectableInAdmission", Type = "bool", Description = "Filter tier yang dapat dipilih di admission.", Example = "true" },
                new() { Name = "isManagedByMarketingOnly", Type = "bool", Description = "Filter tier yang hanya dikelola marketing.", Example = "true" },
                new() { Name = "priorityQueue", Type = "bool", Description = "Filter benefit prioritas antrian.", Example = "true" },
                new() { Name = "freeAnnualCheckup", Type = "bool", Description = "Filter benefit free annual checkup.", Example = "true" },
                new() { Name = "freeParking", Type = "bool", Description = "Filter benefit free parking.", Example = "true" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<MembershipTierFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<MembershipTierFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<MembershipTierFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<MembershipTierFormFieldMetadataResponse>
            {
                new() { Name = "tierCode", Label = "Kode Membership Tier", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format MT-RSMMC-00001.", Example = "MT-RSMMC-00001", SortOrder = 1 },
                new() { Name = "tierName", Label = "Nama Membership Tier", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "Gold Member", SortOrder = 2 },
                new() { Name = "tierType", Label = "Tipe Tier", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "tierTypeOptions", Description = "Gunakan opsi enum MembershipTierType.", Example = "3", SortOrder = 3 },
                new() { Name = "cardTitle", Label = "Judul Kartu", Section = "Card", InputType = "text", MaxLength = 100, Example = "Gold Member", SortOrder = 4 },
                new() { Name = "cardColor", Label = "Warna Kartu", Section = "Card", InputType = "text", MaxLength = 50, Example = "#D4AF37", SortOrder = 5 },
                new() { Name = "cardImagePath", Label = "Path Gambar Kartu", Section = "Card", InputType = "text", MaxLength = 500, Example = "/uploads/membership/gold.png", SortOrder = 6 },
                new() { Name = "priorityLevel", Label = "Level Prioritas", Section = "Rule", InputType = "number", Description = "Semakin besar nilainya, semakin tinggi prioritasnya.", Example = "10", SortOrder = 7 },
                new() { Name = "isDefault", Label = "Default Tier", Section = "Rule", InputType = "switch", SortOrder = 8 },
                new() { Name = "isSelectableInKiosk", Label = "Tampil di Kiosk", Section = "Rule", InputType = "switch", SortOrder = 9 },
                new() { Name = "isSelectableInAdmission", Label = "Tampil di Admission", Section = "Rule", InputType = "switch", SortOrder = 10 },
                new() { Name = "isManagedByMarketingOnly", Label = "Hanya Dikelola Marketing", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "registrationDiscountPercent", Label = "Diskon Registrasi (%)", Section = "Discount", InputType = "number", Description = "Nilai 0 sampai 100.", Example = "10", SortOrder = 12 },
                new() { Name = "consultationDiscountPercent", Label = "Diskon Konsultasi (%)", Section = "Discount", InputType = "number", Description = "Nilai 0 sampai 100.", Example = "10", SortOrder = 13 },
                new() { Name = "procedureDiscountPercent", Label = "Diskon Tindakan (%)", Section = "Discount", InputType = "number", Description = "Nilai 0 sampai 100.", Example = "5", SortOrder = 14 },
                new() { Name = "laboratoryDiscountPercent", Label = "Diskon Laboratorium (%)", Section = "Discount", InputType = "number", Description = "Nilai 0 sampai 100.", Example = "5", SortOrder = 15 },
                new() { Name = "radiologyDiscountPercent", Label = "Diskon Radiologi (%)", Section = "Discount", InputType = "number", Description = "Nilai 0 sampai 100.", Example = "5", SortOrder = 16 },
                new() { Name = "pharmacyDiscountPercent", Label = "Diskon Farmasi (%)", Section = "Discount", InputType = "number", Description = "Nilai 0 sampai 100.", Example = "3", SortOrder = 17 },
                new() { Name = "priorityQueue", Label = "Prioritas Antrian", Section = "Benefit", InputType = "switch", SortOrder = 18 },
                new() { Name = "freeAnnualCheckup", Label = "Free Annual Checkup", Section = "Benefit", InputType = "switch", SortOrder = 19 },
                new() { Name = "freeParking", Label = "Free Parking", Section = "Benefit", InputType = "switch", SortOrder = 20 },
                new() { Name = "validityMonths", Label = "Masa Berlaku Bulan", Section = "Validity", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", Description = "Minimal 1 bulan.", Example = "12", SortOrder = 21 },
                new() { Name = "minimumSpendAmount", Label = "Minimum Spend", Section = "Validity", InputType = "number", Description = "Minimal transaksi untuk tier ini.", Example = "1000000", SortOrder = 22 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 23 },
                new() { Name = "benefitDescription", Label = "Deskripsi Benefit", Section = "Additional", InputType = "textarea", MaxLength = 500, SortOrder = 24 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 25 }
            };

            if (isUpdate)
            {
                fields.Add(new MembershipTierFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Status",
                    InputType = "switch",
                    SortOrder = 99
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLowerInvariant();
            var today = DateTime.UtcNow.Date;

            DateTime? start = null;
            DateTime? endExclusive = null;

            switch (period)
            {
                case null:
                case "":
                case "custom":
                    if (startDate.HasValue)
                    {
                        start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                    }

                    if (endDate.HasValue)
                    {
                        endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    }

                    break;

                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
                    break;

                case "last7days":
                    start = today.AddDays(-6);
                    endExclusive = today.AddDays(1);
                    break;

                case "last30days":
                    start = today.AddDays(-29);
                    endExclusive = today.AddDays(1);
                    break;

                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddMonths(1);
                    break;

                case "lastmonth":
                    var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    start = currentMonthStart.AddMonths(-1);
                    endExclusive = currentMonthStart;
                    break;

                default:
                    return DateRangeResolveResult.Invalid($"customPeriod '{customPeriod}' tidak valid.");
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResolveResult.Invalid("startDate tidak boleh lebih besar atau sama dengan endDate.");
            }

            return DateRangeResolveResult.Valid(start, endExclusive);
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

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private sealed class DateRangeResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResolveResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResolveResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResolveResult Invalid(string errorMessage)
            {
                return new DateRangeResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
