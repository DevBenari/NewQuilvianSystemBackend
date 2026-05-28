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

using ResponsePaymentMethodPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs.PaymentMethodResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/billing-management/master-data/payment-methods")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Billing Management Master Data",
        displayName: "Payment Method",
        AreaName = "HealthServices",
        ControllerName = "PaymentMethod",
        Description = "Health service billing management master data payment method",
        SortOrder = 21
    )]
    [Tags("Health Services / Billing Management / Master Data / Payment Method")]
    public class PaymentMethodController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BillingManagement.MasterData";

        private static readonly HashSet<string> AllowedPaymentMethodTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Cash",
            "Debit",
            "CreditCard",
            "Transfer",
            "QRIS",
            "Insurance",
            "CompanyGuarantor",
            "Membership",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PaymentMethodController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PaymentMethodFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payment Method", Description = "Melihat data payment method", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PaymentMethod", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PaymentMethodFilterMetadataResponse
            {
                DefaultFilter = new PaymentMethodDefaultFilterResponse(),
                SortOptions = new List<PaymentMethodSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "paymentMethodCode", Label = "Kode payment method" },
                    new() { Value = "paymentMethodName", Label = "Nama payment method" },
                    new() { Value = "paymentMethodType", Label = "Tipe payment method" },
                    new() { Value = "paymentGroupName", Label = "Group payment" },
                    new() { Value = "adminFeeAmount", Label = "Admin fee amount" },
                    new() { Value = "adminFeePercent", Label = "Admin fee percent" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                PaymentMethodTypes = AllowedPaymentMethodTypes.OrderBy(x => x).ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PaymentMethod.GetFilterMetadata",
                "Mengambil metadata filter payment method.",
                result
            );

            return Ok(ApiResponse<PaymentMethodFilterMetadataResponse>.Ok(
                result,
                "Metadata filter payment method berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PaymentMethodSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payment Method", Description = "Melihat data payment method", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PaymentMethod", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstPaymentMethod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new PaymentMethodSummaryResponse
            {
                TotalPaymentMethod = await query.CountAsync(),
                ActivePaymentMethod = await query.CountAsync(x => x.IsActive),
                InactivePaymentMethod = await query.CountAsync(x => !x.IsActive),
                CashPaymentMethod = await query.CountAsync(x => x.IsCash),
                BankTransferPaymentMethod = await query.CountAsync(x => x.IsBankTransfer),
                CardPaymentMethod = await query.CountAsync(x => x.IsCardPayment),
                QrisPaymentMethod = await query.CountAsync(x => x.IsQris),
                InsurancePaymentMethod = await query.CountAsync(x => x.IsInsurance),
                CompanyGuarantorPaymentMethod = await query.CountAsync(x => x.IsCompanyGuarantor),
                MembershipPaymentMethod = await query.CountAsync(x => x.IsMembership),
                NeedReferenceNumberPaymentMethod = await query.CountAsync(x => x.IsNeedReferenceNumber),
                NeedApprovalPaymentMethod = await query.CountAsync(x => x.IsNeedApproval),
                NeedAttachmentPaymentMethod = await query.CountAsync(x => x.IsNeedAttachment),
                AvailableForRegistrationPaymentMethod = await query.CountAsync(x => x.IsAvailableForRegistration),
                AvailableForBillingPaymentMethod = await query.CountAsync(x => x.IsAvailableForBilling),
                AvailableForRefundPaymentMethod = await query.CountAsync(x => x.IsAvailableForRefund)
            };

            return Ok(ApiResponse<PaymentMethodSummaryResponse>.Ok(
                result,
                "Ringkasan payment method berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePaymentMethodPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payment Method", Description = "Melihat data payment method", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PaymentMethod", "Read")]
        public async Task<IActionResult> GetPaymentMethods(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? paymentMethodType,
            [FromQuery] string? paymentGroupName,
            [FromQuery] bool? isCash,
            [FromQuery] bool? isBankTransfer,
            [FromQuery] bool? isCardPayment,
            [FromQuery] bool? isQris,
            [FromQuery] bool? isInsurance,
            [FromQuery] bool? isCompanyGuarantor,
            [FromQuery] bool? isMembership,
            [FromQuery] bool? isNeedReferenceNumber,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isNeedAttachment,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForBilling,
            [FromQuery] bool? isAvailableForRefund,
            [FromQuery] decimal? minimumAdminFeeAmount,
            [FromQuery] decimal? maximumAdminFeeAmount,
            [FromQuery] decimal? minimumAdminFeePercent,
            [FromQuery] decimal? maximumAdminFeePercent,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilters(
                BuildBaseQuery(),
                search,
                isActive,
                paymentMethodType,
                paymentGroupName,
                isCash,
                isBankTransfer,
                isCardPayment,
                isQris,
                isInsurance,
                isCompanyGuarantor,
                isMembership,
                isNeedReferenceNumber,
                isNeedApproval,
                isNeedAttachment,
                isAvailableForRegistration,
                isAvailableForBilling,
                isAvailableForRefund,
                minimumAdminFeeAmount,
                maximumAdminFeeAmount,
                minimumAdminFeePercent,
                maximumAdminFeePercent
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponsePaymentMethodPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePaymentMethodPagedResult>.Ok(
                result,
                "Data payment method berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PaymentMethodOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Payment Method", Description = "Melihat data payment method", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PaymentMethod", "Read")]
        public async Task<IActionResult> GetPaymentMethodOptions(
            [FromQuery] string? paymentMethodType,
            [FromQuery] bool? isAvailableForRegistration,
            [FromQuery] bool? isAvailableForBilling,
            [FromQuery] bool? isAvailableForRefund,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(paymentMethodType))
                query = query.Where(x => x.PaymentMethodType == paymentMethodType.Trim());

            if (isAvailableForRegistration.HasValue)
                query = query.Where(x => x.IsAvailableForRegistration == isAvailableForRegistration.Value);

            if (isAvailableForBilling.HasValue)
                query = query.Where(x => x.IsAvailableForBilling == isAvailableForBilling.Value);

            if (isAvailableForRefund.HasValue)
                query = query.Where(x => x.IsAvailableForRefund == isAvailableForRefund.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PaymentMethodCode.ToLower().Contains(keyword) ||
                    x.PaymentMethodName.ToLower().Contains(keyword) ||
                    x.PaymentMethodType.ToLower().Contains(keyword) ||
                    x.PaymentGroupName != null && x.PaymentGroupName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.PaymentMethodName)
                .Select(x => new PaymentMethodOptionResponse
                {
                    Id = x.Id,
                    PaymentMethodCode = x.PaymentMethodCode,
                    PaymentMethodName = x.PaymentMethodName,
                    PaymentMethodType = x.PaymentMethodType,
                    PaymentGroupName = x.PaymentGroupName,
                    IsCash = x.IsCash,
                    IsBankTransfer = x.IsBankTransfer,
                    IsCardPayment = x.IsCardPayment,
                    IsQris = x.IsQris,
                    IsInsurance = x.IsInsurance,
                    IsCompanyGuarantor = x.IsCompanyGuarantor,
                    IsMembership = x.IsMembership,
                    IsNeedReferenceNumber = x.IsNeedReferenceNumber,
                    IsNeedApproval = x.IsNeedApproval,
                    IsNeedAttachment = x.IsNeedAttachment,
                    IsAvailableForRegistration = x.IsAvailableForRegistration,
                    IsAvailableForBilling = x.IsAvailableForBilling,
                    IsAvailableForRefund = x.IsAvailableForRefund,
                    AdminFeeAmount = x.AdminFeeAmount,
                    AdminFeePercent = x.AdminFeePercent
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PaymentMethodOptionResponse>>.Ok(
                data,
                "Data pilihan payment method berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentMethodDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Payment Method", Description = "Melihat data payment method", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PaymentMethod", "Read")]
        public async Task<IActionResult> GetPaymentMethodById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => ToDetailResponse(x))
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payment method tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PaymentMethodDetailResponse>.Ok(
                data,
                "Detail payment method berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PaymentMethodCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Payment Method", Description = "Membuat data payment method", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PaymentMethod", "Create")]
        public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                paymentMethodCode: request.PaymentMethodCode,
                paymentMethodName: request.PaymentMethodName,
                paymentMethodType: request.PaymentMethodType,
                adminFeeAmount: request.AdminFeeAmount,
                adminFeePercent: request.AdminFeePercent
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payment method tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstPaymentMethod
            {
                Id = Guid.NewGuid(),
                PaymentMethodCode = request.PaymentMethodCode.Trim().ToUpperInvariant(),
                PaymentMethodName = request.PaymentMethodName.Trim(),
                PaymentMethodType = request.PaymentMethodType.Trim(),
                PaymentGroupName = NormalizeNullableText(request.PaymentGroupName),
                IsCash = request.IsCash,
                IsBankTransfer = request.IsBankTransfer,
                IsCardPayment = request.IsCardPayment,
                IsQris = request.IsQris,
                IsInsurance = request.IsInsurance,
                IsCompanyGuarantor = request.IsCompanyGuarantor,
                IsMembership = request.IsMembership,
                IsNeedReferenceNumber = request.IsNeedReferenceNumber,
                IsNeedApproval = request.IsNeedApproval,
                IsNeedAttachment = request.IsNeedAttachment,
                IsAvailableForRegistration = request.IsAvailableForRegistration,
                IsAvailableForBilling = request.IsAvailableForBilling,
                IsAvailableForRefund = request.IsAvailableForRefund,
                BankName = NormalizeNullableText(request.BankName),
                BankAccountNumber = NormalizeNullableText(request.BankAccountNumber),
                BankAccountName = NormalizeNullableText(request.BankAccountName),
                MerchantId = NormalizeNullableText(request.MerchantId),
                TerminalId = NormalizeNullableText(request.TerminalId),
                ExternalPaymentCode = NormalizeNullableText(request.ExternalPaymentCode),
                IntegrationCode = NormalizeNullableText(request.IntegrationCode),
                AdminFeeAmount = request.AdminFeeAmount,
                AdminFeePercent = request.AdminFeePercent,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPaymentMethod>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new PaymentMethodCreateResponse
            {
                Id = entity.Id,
                PaymentMethodCode = entity.PaymentMethodCode,
                PaymentMethodName = entity.PaymentMethodName
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PaymentMethod.CreatePaymentMethod",
                "Membuat data payment method.",
                result
            );

            return Ok(ApiResponse<PaymentMethodCreateResponse>.Ok(
                result,
                "Payment method berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentMethodUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Payment Method", Description = "Mengubah data payment method", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PaymentMethod", "Update")]
        public async Task<IActionResult> UpdatePaymentMethod(Guid id, [FromBody] UpdatePaymentMethodRequest request)
        {
            var entity = await _dbContext.Set<MstPaymentMethod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payment method tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                paymentMethodCode: request.PaymentMethodCode,
                paymentMethodName: request.PaymentMethodName,
                paymentMethodType: request.PaymentMethodType,
                adminFeeAmount: request.AdminFeeAmount,
                adminFeePercent: request.AdminFeePercent
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data payment method tidak valid."
                ));
            }

            entity.PaymentMethodCode = request.PaymentMethodCode.Trim().ToUpperInvariant();
            entity.PaymentMethodName = request.PaymentMethodName.Trim();
            entity.PaymentMethodType = request.PaymentMethodType.Trim();
            entity.PaymentGroupName = NormalizeNullableText(request.PaymentGroupName);
            entity.IsCash = request.IsCash;
            entity.IsBankTransfer = request.IsBankTransfer;
            entity.IsCardPayment = request.IsCardPayment;
            entity.IsQris = request.IsQris;
            entity.IsInsurance = request.IsInsurance;
            entity.IsCompanyGuarantor = request.IsCompanyGuarantor;
            entity.IsMembership = request.IsMembership;
            entity.IsNeedReferenceNumber = request.IsNeedReferenceNumber;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.IsNeedAttachment = request.IsNeedAttachment;
            entity.IsAvailableForRegistration = request.IsAvailableForRegistration;
            entity.IsAvailableForBilling = request.IsAvailableForBilling;
            entity.IsAvailableForRefund = request.IsAvailableForRefund;
            entity.BankName = NormalizeNullableText(request.BankName);
            entity.BankAccountNumber = NormalizeNullableText(request.BankAccountNumber);
            entity.BankAccountName = NormalizeNullableText(request.BankAccountName);
            entity.MerchantId = NormalizeNullableText(request.MerchantId);
            entity.TerminalId = NormalizeNullableText(request.TerminalId);
            entity.ExternalPaymentCode = NormalizeNullableText(request.ExternalPaymentCode);
            entity.IntegrationCode = NormalizeNullableText(request.IntegrationCode);
            entity.AdminFeeAmount = request.AdminFeeAmount;
            entity.AdminFeePercent = request.AdminFeePercent;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var result = new PaymentMethodUpdateResponse
            {
                Id = entity.Id,
                PaymentMethodCode = entity.PaymentMethodCode,
                PaymentMethodName = entity.PaymentMethodName,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<PaymentMethodUpdateResponse>.Ok(
                result,
                "Payment method berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<PaymentMethodStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Payment Method", Description = "Mengaktifkan data payment method", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PaymentMethod", "Update")]
        public async Task<IActionResult> ActivatePaymentMethod(Guid id)
        {
            return await ChangeStatusAsync(id, true, "Payment method berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<PaymentMethodStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Payment Method", Description = "Menonaktifkan data payment method", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PaymentMethod", "Update")]
        public async Task<IActionResult> DeactivatePaymentMethod(Guid id)
        {
            return await ChangeStatusAsync(id, false, "Payment method berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentMethodDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Payment Method", Description = "Menghapus data payment method", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PaymentMethod", "Delete")]
        public async Task<IActionResult> DeletePaymentMethod(Guid id)
        {
            var entity = await _dbContext.Set<MstPaymentMethod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payment method tidak ditemukan."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var result = new PaymentMethodDeleteResponse
            {
                Id = entity.Id,
                PaymentMethodCode = entity.PaymentMethodCode,
                PaymentMethodName = entity.PaymentMethodName,
                IsDelete = entity.IsDelete
            };

            return Ok(ApiResponse<PaymentMethodDeleteResponse>.Ok(
                result,
                "Payment method berhasil dihapus."
            ));
        }

        private async Task<IActionResult> ChangeStatusAsync(Guid id, bool isActive, string message)
        {
            var entity = await _dbContext.Set<MstPaymentMethod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Payment method tidak ditemukan."
                ));
            }

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var result = new PaymentMethodStatusResponse
            {
                Id = entity.Id,
                PaymentMethodCode = entity.PaymentMethodCode,
                PaymentMethodName = entity.PaymentMethodName,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<PaymentMethodStatusResponse>.Ok(result, message));
        }

        private IQueryable<MstPaymentMethod> BuildBaseQuery()
        {
            return _dbContext.Set<MstPaymentMethod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPaymentMethod> ApplyFilters(
            IQueryable<MstPaymentMethod> query,
            string? search,
            bool? isActive,
            string? paymentMethodType,
            string? paymentGroupName,
            bool? isCash,
            bool? isBankTransfer,
            bool? isCardPayment,
            bool? isQris,
            bool? isInsurance,
            bool? isCompanyGuarantor,
            bool? isMembership,
            bool? isNeedReferenceNumber,
            bool? isNeedApproval,
            bool? isNeedAttachment,
            bool? isAvailableForRegistration,
            bool? isAvailableForBilling,
            bool? isAvailableForRefund,
            decimal? minimumAdminFeeAmount,
            decimal? maximumAdminFeeAmount,
            decimal? minimumAdminFeePercent,
            decimal? maximumAdminFeePercent)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PaymentMethodCode.ToLower().Contains(keyword) ||
                    x.PaymentMethodName.ToLower().Contains(keyword) ||
                    x.PaymentMethodType.ToLower().Contains(keyword) ||
                    x.PaymentGroupName != null && x.PaymentGroupName.ToLower().Contains(keyword) ||
                    x.BankName != null && x.BankName.ToLower().Contains(keyword) ||
                    x.ExternalPaymentCode != null && x.ExternalPaymentCode.ToLower().Contains(keyword) ||
                    x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword));
            }

            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(paymentMethodType)) query = query.Where(x => x.PaymentMethodType == paymentMethodType.Trim());
            if (!string.IsNullOrWhiteSpace(paymentGroupName))
            {
                var keyword = paymentGroupName.Trim().ToLower();
                query = query.Where(x => x.PaymentGroupName != null && x.PaymentGroupName.ToLower().Contains(keyword));
            }

            if (isCash.HasValue) query = query.Where(x => x.IsCash == isCash.Value);
            if (isBankTransfer.HasValue) query = query.Where(x => x.IsBankTransfer == isBankTransfer.Value);
            if (isCardPayment.HasValue) query = query.Where(x => x.IsCardPayment == isCardPayment.Value);
            if (isQris.HasValue) query = query.Where(x => x.IsQris == isQris.Value);
            if (isInsurance.HasValue) query = query.Where(x => x.IsInsurance == isInsurance.Value);
            if (isCompanyGuarantor.HasValue) query = query.Where(x => x.IsCompanyGuarantor == isCompanyGuarantor.Value);
            if (isMembership.HasValue) query = query.Where(x => x.IsMembership == isMembership.Value);
            if (isNeedReferenceNumber.HasValue) query = query.Where(x => x.IsNeedReferenceNumber == isNeedReferenceNumber.Value);
            if (isNeedApproval.HasValue) query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);
            if (isNeedAttachment.HasValue) query = query.Where(x => x.IsNeedAttachment == isNeedAttachment.Value);
            if (isAvailableForRegistration.HasValue) query = query.Where(x => x.IsAvailableForRegistration == isAvailableForRegistration.Value);
            if (isAvailableForBilling.HasValue) query = query.Where(x => x.IsAvailableForBilling == isAvailableForBilling.Value);
            if (isAvailableForRefund.HasValue) query = query.Where(x => x.IsAvailableForRefund == isAvailableForRefund.Value);
            if (minimumAdminFeeAmount.HasValue) query = query.Where(x => x.AdminFeeAmount >= minimumAdminFeeAmount.Value);
            if (maximumAdminFeeAmount.HasValue) query = query.Where(x => x.AdminFeeAmount <= maximumAdminFeeAmount.Value);
            if (minimumAdminFeePercent.HasValue) query = query.Where(x => x.AdminFeePercent >= minimumAdminFeePercent.Value);
            if (maximumAdminFeePercent.HasValue) query = query.Where(x => x.AdminFeePercent <= maximumAdminFeePercent.Value);

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string paymentMethodCode,
            string paymentMethodName,
            string paymentMethodType,
            decimal adminFeeAmount,
            decimal adminFeePercent)
        {
            if (string.IsNullOrWhiteSpace(paymentMethodCode))
                return (false, "Kode payment method wajib diisi.");

            if (string.IsNullOrWhiteSpace(paymentMethodName))
                return (false, "Nama payment method wajib diisi.");

            if (string.IsNullOrWhiteSpace(paymentMethodType))
                return (false, "Tipe payment method wajib diisi.");

            if (!AllowedPaymentMethodTypes.Contains(paymentMethodType.Trim()))
                return (false, "Tipe payment method tidak valid. Gunakan Cash, Debit, CreditCard, Transfer, QRIS, Insurance, CompanyGuarantor, Membership, atau Other.");

            if (adminFeeAmount < 0)
                return (false, "Admin fee amount tidak boleh lebih kecil dari 0.");

            if (adminFeePercent < 0 || adminFeePercent > 100)
                return (false, "Admin fee percent harus berada di antara 0 sampai 100.");

            var normalizedCode = paymentMethodCode.Trim().ToUpperInvariant();
            var normalizedName = paymentMethodName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstPaymentMethod>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.PaymentMethodCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode payment method sudah digunakan.");

            var duplicateName = await _dbContext.Set<MstPaymentMethod>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.PaymentMethodName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama payment method sudah digunakan.");

            return (true, null);
        }

        private static IQueryable<MstPaymentMethod> ApplySorting(
            IQueryable<MstPaymentMethod> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "paymentmethodcode" => isDesc ? query.OrderByDescending(x => x.PaymentMethodCode) : query.OrderBy(x => x.PaymentMethodCode),
                "paymentmethodname" => isDesc ? query.OrderByDescending(x => x.PaymentMethodName) : query.OrderBy(x => x.PaymentMethodName),
                "paymentmethodtype" => isDesc ? query.OrderByDescending(x => x.PaymentMethodType) : query.OrderBy(x => x.PaymentMethodType),
                "paymentgroupname" => isDesc ? query.OrderByDescending(x => x.PaymentGroupName) : query.OrderBy(x => x.PaymentGroupName),
                "adminfeeamount" => isDesc ? query.OrderByDescending(x => x.AdminFeeAmount) : query.OrderBy(x => x.AdminFeeAmount),
                "adminfeepercent" => isDesc ? query.OrderByDescending(x => x.AdminFeePercent) : query.OrderBy(x => x.AdminFeePercent),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.PaymentMethodName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.PaymentMethodName)
            };
        }

        private static PaymentMethodResponse ToResponse(MstPaymentMethod x)
        {
            return new PaymentMethodResponse
            {
                Id = x.Id,
                PaymentMethodCode = x.PaymentMethodCode,
                PaymentMethodName = x.PaymentMethodName,
                PaymentMethodType = x.PaymentMethodType,
                PaymentGroupName = x.PaymentGroupName,
                IsCash = x.IsCash,
                IsBankTransfer = x.IsBankTransfer,
                IsCardPayment = x.IsCardPayment,
                IsQris = x.IsQris,
                IsInsurance = x.IsInsurance,
                IsCompanyGuarantor = x.IsCompanyGuarantor,
                IsMembership = x.IsMembership,
                IsNeedReferenceNumber = x.IsNeedReferenceNumber,
                IsNeedApproval = x.IsNeedApproval,
                IsNeedAttachment = x.IsNeedAttachment,
                IsAvailableForRegistration = x.IsAvailableForRegistration,
                IsAvailableForBilling = x.IsAvailableForBilling,
                IsAvailableForRefund = x.IsAvailableForRefund,
                BankName = x.BankName,
                BankAccountNumber = x.BankAccountNumber,
                BankAccountName = x.BankAccountName,
                MerchantId = x.MerchantId,
                TerminalId = x.TerminalId,
                ExternalPaymentCode = x.ExternalPaymentCode,
                IntegrationCode = x.IntegrationCode,
                AdminFeeAmount = x.AdminFeeAmount,
                AdminFeePercent = x.AdminFeePercent,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PaymentMethodDetailResponse ToDetailResponse(MstPaymentMethod x)
        {
            return new PaymentMethodDetailResponse
            {
                Id = x.Id,
                PaymentMethodCode = x.PaymentMethodCode,
                PaymentMethodName = x.PaymentMethodName,
                PaymentMethodType = x.PaymentMethodType,
                PaymentGroupName = x.PaymentGroupName,
                IsCash = x.IsCash,
                IsBankTransfer = x.IsBankTransfer,
                IsCardPayment = x.IsCardPayment,
                IsQris = x.IsQris,
                IsInsurance = x.IsInsurance,
                IsCompanyGuarantor = x.IsCompanyGuarantor,
                IsMembership = x.IsMembership,
                IsNeedReferenceNumber = x.IsNeedReferenceNumber,
                IsNeedApproval = x.IsNeedApproval,
                IsNeedAttachment = x.IsNeedAttachment,
                IsAvailableForRegistration = x.IsAvailableForRegistration,
                IsAvailableForBilling = x.IsAvailableForBilling,
                IsAvailableForRefund = x.IsAvailableForRefund,
                BankName = x.BankName,
                BankAccountNumber = x.BankAccountNumber,
                BankAccountName = x.BankAccountName,
                MerchantId = x.MerchantId,
                TerminalId = x.TerminalId,
                ExternalPaymentCode = x.ExternalPaymentCode,
                IntegrationCode = x.IntegrationCode,
                AdminFeeAmount = x.AdminFeeAmount,
                AdminFeePercent = x.AdminFeePercent,
                SortOrder = x.SortOrder,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
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
