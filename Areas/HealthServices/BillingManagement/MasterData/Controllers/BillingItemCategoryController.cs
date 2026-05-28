using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseBillingItemCategoryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs.BillingItemCategoryResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/billing-management/master-data/billing-item-categories")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BILLING_MANAGEMENT",
        moduleName: "Health Service Billing Management",
        displayName: "Billing Item Category",
        AreaName = "HealthServices",
        ControllerName = "BillingItemCategory",
        Description = "Health service billing management master data billing item category",
        SortOrder = 2
    )]
    [Tags("Health Services / Billing Management / Master Data / Billing Item Category")]
    public class BillingItemCategoryController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BillingManagement.MasterData";

        private static readonly HashSet<string> AllowedItemSourceTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Tariff",
            "Procedure",
            "Drug",
            "RoomCharge",
            "Registration",
            "Consultation",
            "Laboratory",
            "Radiology",
            "Pharmacy",
            "Manual",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public BillingItemCategoryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BillingItemCategoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Billing Item Category", Description = "Melihat data billing item category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BillingItemCategory", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new BillingItemCategoryFilterMetadataResponse
            {
                DefaultFilter = new BillingItemCategoryDefaultFilterResponse(),
                SortOptions = new List<BillingItemCategorySortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "billingItemCategoryCode", Label = "Kode kategori billing" },
                    new() { Value = "billingItemCategoryName", Label = "Nama kategori billing" },
                    new() { Value = "billingGroupName", Label = "Group billing" },
                    new() { Value = "itemSourceType", Label = "Sumber item" },
                    new() { Value = "isSystemCategory", Label = "Kategori sistem" },
                    new() { Value = "isEditableInBilling", Label = "Editable di billing" },
                    new() { Value = "isNeedDoctor", Label = "Butuh dokter" },
                    new() { Value = "isNeedApproval", Label = "Butuh approval" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ItemSourceTypeOptions = AllowedItemSourceTypes.OrderBy(x => x).ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "BillingItemCategory.GetFilterMetadata",
                "Mengambil metadata filter billing item category.",
                result
            );

            return Ok(ApiResponse<BillingItemCategoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter billing item category berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BillingItemCategorySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Billing Item Category", Description = "Melihat data billing item category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BillingItemCategory", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstBillingItemCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new BillingItemCategorySummaryResponse
            {
                TotalBillingItemCategory = await query.CountAsync(),
                ActiveBillingItemCategory = await query.CountAsync(x => x.IsActive),
                InactiveBillingItemCategory = await query.CountAsync(x => !x.IsActive),
                RegistrationFeeCategory = await query.CountAsync(x => x.IsRegistrationFee),
                AdministrationFeeCategory = await query.CountAsync(x => x.IsAdministrationFee),
                ConsultationFeeCategory = await query.CountAsync(x => x.IsConsultationFee),
                RoomChargeCategory = await query.CountAsync(x => x.IsRoomCharge),
                ProcedureCategory = await query.CountAsync(x => x.IsProcedure),
                LaboratoryCategory = await query.CountAsync(x => x.IsLaboratory),
                RadiologyCategory = await query.CountAsync(x => x.IsRadiology),
                PharmacyCategory = await query.CountAsync(x => x.IsPharmacy),
                DrugCategory = await query.CountAsync(x => x.IsDrug),
                PackageCategory = await query.CountAsync(x => x.IsPackage),
                DiscountCategory = await query.CountAsync(x => x.IsDiscount),
                TaxCategory = await query.CountAsync(x => x.IsTax),
                DepositCategory = await query.CountAsync(x => x.IsDeposit),
                RefundCategory = await query.CountAsync(x => x.IsRefund),
                CoveredByInsuranceDefaultCategory = await query.CountAsync(x => x.IsCoveredByInsuranceDefault),
                NeedDoctorCategory = await query.CountAsync(x => x.IsNeedDoctor),
                NeedApprovalCategory = await query.CountAsync(x => x.IsNeedApproval),
                EditableInBillingCategory = await query.CountAsync(x => x.IsEditableInBilling),
                SystemCategory = await query.CountAsync(x => x.IsSystemCategory)
            };

            return Ok(ApiResponse<BillingItemCategorySummaryResponse>.Ok(
                result,
                "Ringkasan billing item category berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseBillingItemCategoryPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Billing Item Category", Description = "Melihat data billing item category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BillingItemCategory", "Read")]
        public async Task<IActionResult> GetBillingItemCategories(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? billingGroupName,
            [FromQuery] string? itemSourceType,
            [FromQuery] bool? isRegistrationFee,
            [FromQuery] bool? isAdministrationFee,
            [FromQuery] bool? isConsultationFee,
            [FromQuery] bool? isRoomCharge,
            [FromQuery] bool? isProcedure,
            [FromQuery] bool? isLaboratory,
            [FromQuery] bool? isRadiology,
            [FromQuery] bool? isPharmacy,
            [FromQuery] bool? isDrug,
            [FromQuery] bool? isPackage,
            [FromQuery] bool? isDiscount,
            [FromQuery] bool? isTax,
            [FromQuery] bool? isDeposit,
            [FromQuery] bool? isRefund,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] bool? isNeedDoctor,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isEditableInBilling,
            [FromQuery] bool? isSystemCategory,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilter(
                query,
                search,
                isActive,
                billingGroupName,
                itemSourceType,
                isRegistrationFee,
                isAdministrationFee,
                isConsultationFee,
                isRoomCharge,
                isProcedure,
                isLaboratory,
                isRadiology,
                isPharmacy,
                isDrug,
                isPackage,
                isDiscount,
                isTax,
                isDeposit,
                isRefund,
                isCoveredByInsuranceDefault,
                isNeedDoctor,
                isNeedApproval,
                isEditableInBilling,
                isSystemCategory
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseBillingItemCategoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseBillingItemCategoryPagedResult>.Ok(
                result,
                "Data billing item category berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<BillingItemCategoryOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Billing Item Category", Description = "Melihat data billing item category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BillingItemCategory", "Read")]
        public async Task<IActionResult> GetBillingItemCategoryOptions(
            [FromQuery] string? itemSourceType,
            [FromQuery] string? billingGroupName,
            [FromQuery] bool? isRegistrationFee,
            [FromQuery] bool? isAdministrationFee,
            [FromQuery] bool? isConsultationFee,
            [FromQuery] bool? isRoomCharge,
            [FromQuery] bool? isProcedure,
            [FromQuery] bool? isLaboratory,
            [FromQuery] bool? isRadiology,
            [FromQuery] bool? isPharmacy,
            [FromQuery] bool? isDrug,
            [FromQuery] bool? isPackage,
            [FromQuery] bool? isDiscount,
            [FromQuery] bool? isTax,
            [FromQuery] bool? isDeposit,
            [FromQuery] bool? isRefund,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] bool? isNeedDoctor,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isEditableInBilling,
            [FromQuery] bool? isSystemCategory,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            query = ApplyFilter(
                query,
                search,
                onlyActive ? true : null,
                billingGroupName,
                itemSourceType,
                isRegistrationFee,
                isAdministrationFee,
                isConsultationFee,
                isRoomCharge,
                isProcedure,
                isLaboratory,
                isRadiology,
                isPharmacy,
                isDrug,
                isPackage,
                isDiscount,
                isTax,
                isDeposit,
                isRefund,
                isCoveredByInsuranceDefault,
                isNeedDoctor,
                isNeedApproval,
                isEditableInBilling,
                isSystemCategory
            );

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.BillingItemCategoryName)
                .Select(x => new BillingItemCategoryOptionResponse
                {
                    Id = x.Id,
                    BillingItemCategoryCode = x.BillingItemCategoryCode,
                    BillingItemCategoryName = x.BillingItemCategoryName,
                    BillingGroupName = x.BillingGroupName,
                    ItemSourceType = x.ItemSourceType,
                    IsRegistrationFee = x.IsRegistrationFee,
                    IsAdministrationFee = x.IsAdministrationFee,
                    IsConsultationFee = x.IsConsultationFee,
                    IsRoomCharge = x.IsRoomCharge,
                    IsProcedure = x.IsProcedure,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsPharmacy = x.IsPharmacy,
                    IsDrug = x.IsDrug,
                    IsPackage = x.IsPackage,
                    IsDiscount = x.IsDiscount,
                    IsTax = x.IsTax,
                    IsDeposit = x.IsDeposit,
                    IsRefund = x.IsRefund,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    IsNeedDoctor = x.IsNeedDoctor,
                    IsNeedApproval = x.IsNeedApproval,
                    IsEditableInBilling = x.IsEditableInBilling,
                    IsSystemCategory = x.IsSystemCategory
                })
                .ToListAsync();

            return Ok(ApiResponse<List<BillingItemCategoryOptionResponse>>.Ok(
                data,
                "Data pilihan billing item category berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BillingItemCategoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Billing Item Category", Description = "Melihat data billing item category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BillingItemCategory", "Read")]
        public async Task<IActionResult> GetBillingItemCategoryById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new BillingItemCategoryDetailResponse
                {
                    Id = x.Id,
                    BillingItemCategoryCode = x.BillingItemCategoryCode,
                    BillingItemCategoryName = x.BillingItemCategoryName,
                    BillingGroupName = x.BillingGroupName,
                    ItemSourceType = x.ItemSourceType,
                    IsRegistrationFee = x.IsRegistrationFee,
                    IsAdministrationFee = x.IsAdministrationFee,
                    IsConsultationFee = x.IsConsultationFee,
                    IsRoomCharge = x.IsRoomCharge,
                    IsProcedure = x.IsProcedure,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsPharmacy = x.IsPharmacy,
                    IsDrug = x.IsDrug,
                    IsPackage = x.IsPackage,
                    IsDiscount = x.IsDiscount,
                    IsTax = x.IsTax,
                    IsDeposit = x.IsDeposit,
                    IsRefund = x.IsRefund,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    IsNeedDoctor = x.IsNeedDoctor,
                    IsNeedApproval = x.IsNeedApproval,
                    IsEditableInBilling = x.IsEditableInBilling,
                    IsSystemCategory = x.IsSystemCategory,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Billing item category tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<BillingItemCategoryDetailResponse>.Ok(
                data,
                "Detail billing item category berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BillingItemCategoryCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Billing Item Category", Description = "Membuat data billing item category", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BillingItemCategory", "Create")]
        public async Task<IActionResult> CreateBillingItemCategory([FromBody] CreateBillingItemCategoryRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                billingItemCategoryCode: request.BillingItemCategoryCode,
                billingItemCategoryName: request.BillingItemCategoryName,
                itemSourceType: request.ItemSourceType
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data billing item category tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstBillingItemCategory
            {
                Id = Guid.NewGuid(),
                BillingItemCategoryCode = request.BillingItemCategoryCode.Trim().ToUpperInvariant(),
                BillingItemCategoryName = request.BillingItemCategoryName.Trim(),
                BillingGroupName = NormalizeNullableString(request.BillingGroupName),
                ItemSourceType = NormalizeItemSourceType(request.ItemSourceType),
                IsRegistrationFee = request.IsRegistrationFee,
                IsAdministrationFee = request.IsAdministrationFee,
                IsConsultationFee = request.IsConsultationFee,
                IsRoomCharge = request.IsRoomCharge,
                IsProcedure = request.IsProcedure,
                IsLaboratory = request.IsLaboratory,
                IsRadiology = request.IsRadiology,
                IsPharmacy = request.IsPharmacy,
                IsDrug = request.IsDrug,
                IsPackage = request.IsPackage,
                IsDiscount = request.IsDiscount,
                IsTax = request.IsTax,
                IsDeposit = request.IsDeposit,
                IsRefund = request.IsRefund,
                IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault,
                IsNeedDoctor = request.IsNeedDoctor,
                IsNeedApproval = request.IsNeedApproval,
                IsEditableInBilling = request.IsEditableInBilling,
                IsSystemCategory = request.IsSystemCategory,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableString(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstBillingItemCategory>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new BillingItemCategoryCreateResponse
            {
                Id = entity.Id,
                BillingItemCategoryCode = entity.BillingItemCategoryCode,
                BillingItemCategoryName = entity.BillingItemCategoryName
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "BillingItemCategory.CreateBillingItemCategory",
                "Membuat data billing item category.",
                result
            );

            return Ok(ApiResponse<BillingItemCategoryCreateResponse>.Ok(
                result,
                "Billing item category berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BillingItemCategoryUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Billing Item Category", Description = "Mengubah data billing item category", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BillingItemCategory", "Update")]
        public async Task<IActionResult> UpdateBillingItemCategory(Guid id, [FromBody] UpdateBillingItemCategoryRequest request)
        {
            var entity = await _dbContext.Set<MstBillingItemCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Billing item category tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                billingItemCategoryCode: request.BillingItemCategoryCode,
                billingItemCategoryName: request.BillingItemCategoryName,
                itemSourceType: request.ItemSourceType
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data billing item category tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.BillingItemCategoryCode = request.BillingItemCategoryCode.Trim().ToUpperInvariant();
            entity.BillingItemCategoryName = request.BillingItemCategoryName.Trim();
            entity.BillingGroupName = NormalizeNullableString(request.BillingGroupName);
            entity.ItemSourceType = NormalizeItemSourceType(request.ItemSourceType);
            entity.IsRegistrationFee = request.IsRegistrationFee;
            entity.IsAdministrationFee = request.IsAdministrationFee;
            entity.IsConsultationFee = request.IsConsultationFee;
            entity.IsRoomCharge = request.IsRoomCharge;
            entity.IsProcedure = request.IsProcedure;
            entity.IsLaboratory = request.IsLaboratory;
            entity.IsRadiology = request.IsRadiology;
            entity.IsPharmacy = request.IsPharmacy;
            entity.IsDrug = request.IsDrug;
            entity.IsPackage = request.IsPackage;
            entity.IsDiscount = request.IsDiscount;
            entity.IsTax = request.IsTax;
            entity.IsDeposit = request.IsDeposit;
            entity.IsRefund = request.IsRefund;
            entity.IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault;
            entity.IsNeedDoctor = request.IsNeedDoctor;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.IsEditableInBilling = request.IsEditableInBilling;
            entity.IsSystemCategory = request.IsSystemCategory;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new BillingItemCategoryUpdateResponse
            {
                Id = entity.Id,
                BillingItemCategoryCode = entity.BillingItemCategoryCode,
                BillingItemCategoryName = entity.BillingItemCategoryName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "BillingItemCategory.UpdateBillingItemCategory",
                "Mengubah data billing item category.",
                result
            );

            return Ok(ApiResponse<BillingItemCategoryUpdateResponse>.Ok(
                result,
                "Billing item category berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<BillingItemCategoryStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Billing Item Category", Description = "Mengaktifkan data billing item category", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BillingItemCategory", "Update")]
        public async Task<IActionResult> ActivateBillingItemCategory(Guid id)
        {
            return await UpdateStatusAsync(id, true, "Billing item category berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<BillingItemCategoryStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Billing Item Category", Description = "Menonaktifkan data billing item category", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BillingItemCategory", "Update")]
        public async Task<IActionResult> DeactivateBillingItemCategory(Guid id)
        {
            return await UpdateStatusAsync(id, false, "Billing item category berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BillingItemCategoryDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Billing Item Category", Description = "Menghapus data billing item category", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("BillingItemCategory", "Delete")]
        public async Task<IActionResult> DeleteBillingItemCategory(Guid id)
        {
            var entity = await _dbContext.Set<MstBillingItemCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Billing item category tidak ditemukan."
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

            var result = new BillingItemCategoryDeleteResponse
            {
                Id = entity.Id,
                BillingItemCategoryCode = entity.BillingItemCategoryCode,
                BillingItemCategoryName = entity.BillingItemCategoryName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "BillingItemCategory.DeleteBillingItemCategory",
                "Menghapus data billing item category.",
                result
            );

            return Ok(ApiResponse<BillingItemCategoryDeleteResponse>.Ok(
                result,
                "Billing item category berhasil dihapus."
            ));
        }

        private async Task<IActionResult> UpdateStatusAsync(Guid id, bool isActive, string successMessage)
        {
            var entity = await _dbContext.Set<MstBillingItemCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Billing item category tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = isActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new BillingItemCategoryStatusResponse
            {
                Id = entity.Id,
                BillingItemCategoryCode = entity.BillingItemCategoryCode,
                BillingItemCategoryName = entity.BillingItemCategoryName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "BillingItemCategory.UpdateStatus",
                successMessage,
                result
            );

            return Ok(ApiResponse<BillingItemCategoryStatusResponse>.Ok(
                result,
                successMessage
            ));
        }

        private IQueryable<MstBillingItemCategory> BuildBaseQuery()
        {
            return _dbContext.Set<MstBillingItemCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstBillingItemCategory> ApplyFilter(
            IQueryable<MstBillingItemCategory> query,
            string? search,
            bool? isActive,
            string? billingGroupName,
            string? itemSourceType,
            bool? isRegistrationFee,
            bool? isAdministrationFee,
            bool? isConsultationFee,
            bool? isRoomCharge,
            bool? isProcedure,
            bool? isLaboratory,
            bool? isRadiology,
            bool? isPharmacy,
            bool? isDrug,
            bool? isPackage,
            bool? isDiscount,
            bool? isTax,
            bool? isDeposit,
            bool? isRefund,
            bool? isCoveredByInsuranceDefault,
            bool? isNeedDoctor,
            bool? isNeedApproval,
            bool? isEditableInBilling,
            bool? isSystemCategory)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.BillingItemCategoryCode.ToLower().Contains(keyword) ||
                    x.BillingItemCategoryName.ToLower().Contains(keyword) ||
                    x.ItemSourceType.ToLower().Contains(keyword) ||
                    (x.BillingGroupName != null && x.BillingGroupName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(billingGroupName))
            {
                var billingGroupKeyword = billingGroupName.Trim().ToLower();
                query = query.Where(x => x.BillingGroupName != null && x.BillingGroupName.ToLower().Contains(billingGroupKeyword));
            }

            if (!string.IsNullOrWhiteSpace(itemSourceType))
            {
                var normalizedItemSourceType = itemSourceType.Trim().ToLower();
                query = query.Where(x => x.ItemSourceType.ToLower() == normalizedItemSourceType);
            }

            if (isRegistrationFee.HasValue)
                query = query.Where(x => x.IsRegistrationFee == isRegistrationFee.Value);

            if (isAdministrationFee.HasValue)
                query = query.Where(x => x.IsAdministrationFee == isAdministrationFee.Value);

            if (isConsultationFee.HasValue)
                query = query.Where(x => x.IsConsultationFee == isConsultationFee.Value);

            if (isRoomCharge.HasValue)
                query = query.Where(x => x.IsRoomCharge == isRoomCharge.Value);

            if (isProcedure.HasValue)
                query = query.Where(x => x.IsProcedure == isProcedure.Value);

            if (isLaboratory.HasValue)
                query = query.Where(x => x.IsLaboratory == isLaboratory.Value);

            if (isRadiology.HasValue)
                query = query.Where(x => x.IsRadiology == isRadiology.Value);

            if (isPharmacy.HasValue)
                query = query.Where(x => x.IsPharmacy == isPharmacy.Value);

            if (isDrug.HasValue)
                query = query.Where(x => x.IsDrug == isDrug.Value);

            if (isPackage.HasValue)
                query = query.Where(x => x.IsPackage == isPackage.Value);

            if (isDiscount.HasValue)
                query = query.Where(x => x.IsDiscount == isDiscount.Value);

            if (isTax.HasValue)
                query = query.Where(x => x.IsTax == isTax.Value);

            if (isDeposit.HasValue)
                query = query.Where(x => x.IsDeposit == isDeposit.Value);

            if (isRefund.HasValue)
                query = query.Where(x => x.IsRefund == isRefund.Value);

            if (isCoveredByInsuranceDefault.HasValue)
                query = query.Where(x => x.IsCoveredByInsuranceDefault == isCoveredByInsuranceDefault.Value);

            if (isNeedDoctor.HasValue)
                query = query.Where(x => x.IsNeedDoctor == isNeedDoctor.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (isEditableInBilling.HasValue)
                query = query.Where(x => x.IsEditableInBilling == isEditableInBilling.Value);

            if (isSystemCategory.HasValue)
                query = query.Where(x => x.IsSystemCategory == isSystemCategory.Value);

            return query;
        }

        private static IOrderedQueryable<MstBillingItemCategory> ApplySorting(
            IQueryable<MstBillingItemCategory> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "billingitemcategorycode" => isDescending
                    ? query.OrderByDescending(x => x.BillingItemCategoryCode)
                    : query.OrderBy(x => x.BillingItemCategoryCode),

                "billingitemcategoryname" => isDescending
                    ? query.OrderByDescending(x => x.BillingItemCategoryName)
                    : query.OrderBy(x => x.BillingItemCategoryName),

                "billinggroupname" => isDescending
                    ? query.OrderByDescending(x => x.BillingGroupName)
                    : query.OrderBy(x => x.BillingGroupName),

                "itemsourcetype" => isDescending
                    ? query.OrderByDescending(x => x.ItemSourceType)
                    : query.OrderBy(x => x.ItemSourceType),

                "issystemcategory" => isDescending
                    ? query.OrderByDescending(x => x.IsSystemCategory)
                    : query.OrderBy(x => x.IsSystemCategory),

                "iseditableinbilling" => isDescending
                    ? query.OrderByDescending(x => x.IsEditableInBilling)
                    : query.OrderBy(x => x.IsEditableInBilling),

                "isneeddoctor" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedDoctor)
                    : query.OrderBy(x => x.IsNeedDoctor),

                "isneedapproval" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedApproval)
                    : query.OrderBy(x => x.IsNeedApproval),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.BillingItemCategoryName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.BillingItemCategoryName)
            };
        }

        private static BillingItemCategoryResponse ToResponse(MstBillingItemCategory x)
        {
            return new BillingItemCategoryResponse
            {
                Id = x.Id,
                BillingItemCategoryCode = x.BillingItemCategoryCode,
                BillingItemCategoryName = x.BillingItemCategoryName,
                BillingGroupName = x.BillingGroupName,
                ItemSourceType = x.ItemSourceType,
                IsRegistrationFee = x.IsRegistrationFee,
                IsAdministrationFee = x.IsAdministrationFee,
                IsConsultationFee = x.IsConsultationFee,
                IsRoomCharge = x.IsRoomCharge,
                IsProcedure = x.IsProcedure,
                IsLaboratory = x.IsLaboratory,
                IsRadiology = x.IsRadiology,
                IsPharmacy = x.IsPharmacy,
                IsDrug = x.IsDrug,
                IsPackage = x.IsPackage,
                IsDiscount = x.IsDiscount,
                IsTax = x.IsTax,
                IsDeposit = x.IsDeposit,
                IsRefund = x.IsRefund,
                IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                IsNeedDoctor = x.IsNeedDoctor,
                IsNeedApproval = x.IsNeedApproval,
                IsEditableInBilling = x.IsEditableInBilling,
                IsSystemCategory = x.IsSystemCategory,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string billingItemCategoryCode,
            string billingItemCategoryName,
            string itemSourceType)
        {
            if (string.IsNullOrWhiteSpace(billingItemCategoryCode))
                return (false, "Kode billing item category wajib diisi.");

            if (string.IsNullOrWhiteSpace(billingItemCategoryName))
                return (false, "Nama billing item category wajib diisi.");

            if (string.IsNullOrWhiteSpace(itemSourceType))
                return (false, "Item source type wajib diisi.");

            if (!AllowedItemSourceTypes.Contains(itemSourceType.Trim()))
            {
                return (false, "Item source type tidak valid. Gunakan salah satu: Tariff, Procedure, Drug, RoomCharge, Registration, Consultation, Laboratory, Radiology, Pharmacy, Manual, Other.");
            }

            var normalizedCode = billingItemCategoryCode.Trim().ToUpperInvariant();
            var normalizedName = billingItemCategoryName.Trim().ToLower();

            var duplicateCodeQuery = _dbContext.Set<MstBillingItemCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.BillingItemCategoryCode.ToUpper() == normalizedCode);

            if (excludeId.HasValue)
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateCodeQuery.AnyAsync())
                return (false, "Kode billing item category sudah digunakan.");

            var duplicateNameQuery = _dbContext.Set<MstBillingItemCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.BillingItemCategoryName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama billing item category sudah digunakan.");

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

        private static string NormalizeItemSourceType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedItemSourceTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "Manual";
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
