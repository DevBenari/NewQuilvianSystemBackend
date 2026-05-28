using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseMembershipTierPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.MembershipTierResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/membership-tiers")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT",
        moduleName: "Health Service Patient Management",
        displayName: "Membership Tier",
        AreaName = "HealthServices",
        ControllerName = "MembershipTier",
        Description = "Health service patient management master data membership tier",
        SortOrder = 5
    )]
    [Tags("Health Services / Patient Management / Membership Tier")]
    public class MembershipTierController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement";

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

            var query = _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TierCode.ToLower().Contains(keyword) ||
                    x.TierName.ToLower().Contains(keyword) ||
                    (x.CardTitle != null && x.CardTitle.ToLower().Contains(keyword)) ||
                    (x.BenefitDescription != null && x.BenefitDescription.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (tierType.HasValue)
                query = query.Where(x => x.TierType == tierType.Value);

            if (isDefault.HasValue)
                query = query.Where(x => x.IsDefault == isDefault.Value);

            if (isSelectableInKiosk.HasValue)
                query = query.Where(x => x.IsSelectableInKiosk == isSelectableInKiosk.Value);

            if (isSelectableInAdmission.HasValue)
                query = query.Where(x => x.IsSelectableInAdmission == isSelectableInAdmission.Value);

            if (isManagedByMarketingOnly.HasValue)
                query = query.Where(x => x.IsManagedByMarketingOnly == isManagedByMarketingOnly.Value);

            if (priorityQueue.HasValue)
                query = query.Where(x => x.PriorityQueue == priorityQueue.Value);

            if (freeAnnualCheckup.HasValue)
                query = query.Where(x => x.FreeAnnualCheckup == freeAnnualCheckup.Value);

            if (freeParking.HasValue)
                query = query.Where(x => x.FreeParking == freeParking.Value);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MembershipTierResponse
                {
                    Id = x.Id,
                    TierCode = x.TierCode,
                    TierName = x.TierName,
                    TierType = x.TierType,
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
                })
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
        [ProducesResponseType(typeof(ApiResponse<List<MembershipTierOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Membership Tier", Description = "Melihat data membership tier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MembershipTier", "Read")]
        public async Task<IActionResult> GetMembershipTierOptions(
            [FromQuery] MembershipTierType? tierType,
            [FromQuery] bool? isSelectableInKiosk,
            [FromQuery] bool? isSelectableInAdmission,
            [FromQuery] bool? priorityQueue,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (tierType.HasValue)
                query = query.Where(x => x.TierType == tierType.Value);

            if (isSelectableInKiosk.HasValue)
                query = query.Where(x => x.IsSelectableInKiosk == isSelectableInKiosk.Value);

            if (isSelectableInAdmission.HasValue)
                query = query.Where(x => x.IsSelectableInAdmission == isSelectableInAdmission.Value);

            if (priorityQueue.HasValue)
                query = query.Where(x => x.PriorityQueue == priorityQueue.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TierCode.ToLower().Contains(keyword) ||
                    x.TierName.ToLower().Contains(keyword) ||
                    (x.CardTitle != null && x.CardTitle.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.PriorityLevel)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.TierName)
                .Select(x => new MembershipTierOptionResponse
                {
                    Id = x.Id,
                    TierCode = x.TierCode,
                    TierName = x.TierName,
                    TierType = x.TierType,
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

            return Ok(ApiResponse<List<MembershipTierOptionResponse>>.Ok(
                data,
                "Data pilihan membership tier berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Membership Tier", Description = "Melihat data membership tier", AccessType = AccessTypes.Read, SortOrder = 1)]
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
        [AccessAction("Create", "Create Membership Tier", Description = "Membuat data membership tier", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MembershipTier", "Create")]
        public async Task<IActionResult> CreateMembershipTier([FromBody] CreateMembershipTierRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                tierCode: request.TierCode,
                tierName: request.TierName,
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
                TierCode = request.TierCode.Trim().ToUpperInvariant(),
                TierName = request.TierName.Trim(),
                TierType = request.TierType,
                CardTitle = NormalizeNullableText(request.CardTitle),
                CardColor = NormalizeNullableText(request.CardColor),
                CardImagePath = NormalizeNullableText(request.CardImagePath),
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
                BenefitDescription = NormalizeNullableText(request.BenefitDescription),
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstMembershipTier>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new MembershipTierCreateResponse
            {
                Id = entity.Id,
                TierCode = entity.TierCode,
                TierName = entity.TierName,
                TierType = entity.TierType,
                PriorityLevel = entity.PriorityLevel,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<MembershipTierCreateResponse>.Ok(
                response,
                "Membership tier berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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
                tierCode: request.TierCode,
                tierName: request.TierName,
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

            entity.TierCode = request.TierCode.Trim().ToUpperInvariant();
            entity.TierName = request.TierName.Trim();
            entity.TierType = request.TierType;
            entity.CardTitle = NormalizeNullableText(request.CardTitle);
            entity.CardColor = NormalizeNullableText(request.CardColor);
            entity.CardImagePath = NormalizeNullableText(request.CardImagePath);
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
            entity.BenefitDescription = NormalizeNullableText(request.BenefitDescription);
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Membership tier berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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
                .AnyAsync(x => x.DefaultMembershipTierId == id && !x.IsDelete);

            if (isUsedByPatientDefaultTier)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Membership tier tidak dapat dihapus karena sudah digunakan sebagai default membership pasien."
                ));
            }

            var isUsedByPatientMembership = await _dbContext.Set<MstPatientMembership>()
                .AnyAsync(x => x.MembershipTierId == id && !x.IsDelete);

            if (isUsedByPatientMembership)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Membership tier tidak dapat dihapus karena sudah digunakan oleh membership pasien."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Membership tier berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string tierCode,
            string tierName,
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
            if (string.IsNullOrWhiteSpace(tierCode))
                return (false, "Kode membership tier wajib diisi.");

            if (string.IsNullOrWhiteSpace(tierName))
                return (false, "Nama membership tier wajib diisi.");

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

            var normalizedCode = tierCode.Trim().ToUpperInvariant();
            var normalizedName = tierName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstMembershipTier>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.TierCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode membership tier sudah digunakan.");

            var duplicateName = await _dbContext.Set<MstMembershipTier>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.TierName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama membership tier sudah digunakan.");

            return (true, null);
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

        private static IQueryable<MstMembershipTier> ApplySorting(
            IQueryable<MstMembershipTier> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
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

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

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

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}