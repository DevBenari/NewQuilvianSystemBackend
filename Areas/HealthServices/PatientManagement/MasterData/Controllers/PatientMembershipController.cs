using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientMembershipPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.PatientMembershipResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/patient-memberships")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT",
        moduleName: "Health Service Patient Management",
        displayName: "Patient Membership",
        AreaName = "HealthServices",
        ControllerName = "PatientMembership",
        Description = "Health service patient management master data patient membership",
        SortOrder = 6
    )]
    [Tags("Health Services / Patient Management / Patient Membership")]
    public class PatientMembershipController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientMembershipController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Membership", Description = "Melihat data patient membership", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMembership", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientMembershipFilterMetadataResponse
            {
                DefaultFilter = new PatientMembershipDefaultFilterResponse(),
                SortOptions = new List<PatientMembershipSortOptionResponse>
                {
                    new() { Value = "joinDate", Label = "Tanggal bergabung" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "memberNumber", Label = "Nomor member" },
                    new() { Value = "patientFullName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "tierName", Label = "Nama tier" },
                    new() { Value = "membershipStatus", Label = "Status membership" },
                    new() { Value = "expiredDate", Label = "Tanggal expired" },
                    new() { Value = "pointBalance", Label = "Point balance" },
                    new() { Value = "totalSpendAmount", Label = "Total spend" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                MembershipStatusOptions = BuildEnumOptions<MembershipStatus>(),
                MembershipTierTypeOptions = BuildEnumOptions<MembershipTierType>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientMembership.GetFilterMetadata",
                "Mengambil metadata filter patient membership.",
                result
            );

            return Ok(ApiResponse<PatientMembershipFilterMetadataResponse>.Ok(
                result,
                "Metadata filter patient membership berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Membership", Description = "Melihat data patient membership", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMembership", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var now = DateTime.UtcNow;

            var query = _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new PatientMembershipSummaryResponse
            {
                TotalPatientMembership = await query.CountAsync(),
                ActivePatientMembership = await query.CountAsync(x => x.IsActive),
                InactivePatientMembership = await query.CountAsync(x => !x.IsActive),
                PrimaryPatientMembership = await query.CountAsync(x => x.IsPrimary),
                ExpiredPatientMembership = await query.CountAsync(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value < now),
                AutoCreatedPatientMembership = await query.CountAsync(x => x.IsAutoCreated),
                KioskCreatedPatientMembership = await query.CountAsync(x => x.IsCreatedFromKiosk),
                AdmissionCreatedPatientMembership = await query.CountAsync(x => x.IsCreatedFromAdmission),
                MarketingCreatedPatientMembership = await query.CountAsync(x => x.IsCreatedByMarketing)
            };

            return Ok(ApiResponse<PatientMembershipSummaryResponse>.Ok(
                result,
                "Ringkasan patient membership berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientMembershipPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Membership", Description = "Melihat data patient membership", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMembership", "Read")]
        public async Task<IActionResult> GetPatientMemberships(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? membershipTierId,
            [FromQuery] MembershipStatus? membershipStatus,
            [FromQuery] MembershipTierType? tierType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isAutoCreated,
            [FromQuery] bool? isCreatedFromKiosk,
            [FromQuery] bool? isCreatedFromAdmission,
            [FromQuery] bool? isCreatedByMarketing,
            [FromQuery] bool? isExpired,
            [FromQuery] DateTime? joinDateFrom,
            [FromQuery] DateTime? joinDateTo,
            [FromQuery] DateTime? expiredDateFrom,
            [FromQuery] DateTime? expiredDateTo,
            [FromQuery] string? sortBy = "joinDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var now = DateTime.UtcNow;

            var query = _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.MemberNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.MembershipTier != null && x.MembershipTier.TierCode.ToLower().Contains(keyword)) ||
                    (x.MembershipTier != null && x.MembershipTier.TierName.ToLower().Contains(keyword)) ||
                    (x.UpgradeDowngradeReason != null && x.UpgradeDowngradeReason.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (membershipTierId.HasValue && membershipTierId.Value != Guid.Empty)
                query = query.Where(x => x.MembershipTierId == membershipTierId.Value);

            if (membershipStatus.HasValue)
                query = query.Where(x => x.MembershipStatus == membershipStatus.Value);

            if (tierType.HasValue)
                query = query.Where(x => x.MembershipTier != null && x.MembershipTier.TierType == tierType.Value);

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isAutoCreated.HasValue)
                query = query.Where(x => x.IsAutoCreated == isAutoCreated.Value);

            if (isCreatedFromKiosk.HasValue)
                query = query.Where(x => x.IsCreatedFromKiosk == isCreatedFromKiosk.Value);

            if (isCreatedFromAdmission.HasValue)
                query = query.Where(x => x.IsCreatedFromAdmission == isCreatedFromAdmission.Value);

            if (isCreatedByMarketing.HasValue)
                query = query.Where(x => x.IsCreatedByMarketing == isCreatedByMarketing.Value);

            if (isExpired.HasValue)
            {
                query = isExpired.Value
                    ? query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value < now)
                    : query.Where(x => !x.ExpiredDate.HasValue || x.ExpiredDate.Value >= now);
            }

            if (joinDateFrom.HasValue)
                query = query.Where(x => x.JoinDate >= joinDateFrom.Value.Date);

            if (joinDateTo.HasValue)
            {
                var dateTo = joinDateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.JoinDate < dateTo);
            }

            if (expiredDateFrom.HasValue)
                query = query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value >= expiredDateFrom.Value.Date);

            if (expiredDateTo.HasValue)
            {
                var dateTo = expiredDateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value < dateTo);
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PatientMembershipResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientFullName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MembershipTierId = x.MembershipTierId,
                    TierCode = x.MembershipTier != null ? x.MembershipTier.TierCode : string.Empty,
                    TierName = x.MembershipTier != null ? x.MembershipTier.TierName : string.Empty,
                    TierType = x.MembershipTier != null ? x.MembershipTier.TierType : MembershipTierType.Regular,
                    MemberNumber = x.MemberNumber,
                    MembershipStatus = x.MembershipStatus,
                    JoinDate = x.JoinDate,
                    ExpiredDate = x.ExpiredDate,
                    IsPrimary = x.IsPrimary,
                    IsAutoCreated = x.IsAutoCreated,
                    IsCreatedFromKiosk = x.IsCreatedFromKiosk,
                    IsCreatedFromAdmission = x.IsCreatedFromAdmission,
                    IsCreatedByMarketing = x.IsCreatedByMarketing,
                    PointBalance = x.PointBalance,
                    TotalSpendAmount = x.TotalSpendAmount,
                    LastUpgradeDate = x.LastUpgradeDate,
                    LastDowngradeDate = x.LastDowngradeDate,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponsePatientMembershipPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientMembershipPagedResult>.Ok(
                result,
                "Data patient membership berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientMembershipOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Membership", Description = "Melihat data patient membership", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMembership", "Read")]
        public async Task<IActionResult> GetPatientMembershipOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? membershipTierId,
            [FromQuery] MembershipStatus? membershipStatus,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (membershipTierId.HasValue && membershipTierId.Value != Guid.Empty)
                query = query.Where(x => x.MembershipTierId == membershipTierId.Value);

            if (membershipStatus.HasValue)
                query = query.Where(x => x.MembershipStatus == membershipStatus.Value);

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.MemberNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.MembershipTier != null && x.MembershipTier.TierName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.JoinDate)
                .ThenBy(x => x.MemberNumber)
                .Select(x => new PatientMembershipOptionResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientFullName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    MembershipTierId = x.MembershipTierId,
                    TierName = x.MembershipTier != null ? x.MembershipTier.TierName : string.Empty,
                    MemberNumber = x.MemberNumber,
                    MembershipStatus = x.MembershipStatus,
                    IsPrimary = x.IsPrimary,
                    JoinDate = x.JoinDate,
                    ExpiredDate = x.ExpiredDate
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientMembershipOptionResponse>>.Ok(
                data,
                "Data pilihan patient membership berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Membership", Description = "Melihat data patient membership", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientMembership", "Read")]
        public async Task<IActionResult> GetPatientMembershipById(Guid id)
        {
            var data = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new PatientMembershipDetailResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientFullName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MembershipTierId = x.MembershipTierId,
                    TierCode = x.MembershipTier != null ? x.MembershipTier.TierCode : string.Empty,
                    TierName = x.MembershipTier != null ? x.MembershipTier.TierName : string.Empty,
                    TierType = x.MembershipTier != null ? x.MembershipTier.TierType : MembershipTierType.Regular,
                    MemberNumber = x.MemberNumber,
                    MembershipStatus = x.MembershipStatus,
                    JoinDate = x.JoinDate,
                    ExpiredDate = x.ExpiredDate,
                    IsPrimary = x.IsPrimary,
                    IsAutoCreated = x.IsAutoCreated,
                    IsCreatedFromKiosk = x.IsCreatedFromKiosk,
                    IsCreatedFromAdmission = x.IsCreatedFromAdmission,
                    IsCreatedByMarketing = x.IsCreatedByMarketing,
                    PointBalance = x.PointBalance,
                    TotalSpendAmount = x.TotalSpendAmount,
                    LastUpgradeDate = x.LastUpgradeDate,
                    LastDowngradeDate = x.LastDowngradeDate,
                    UpgradeDowngradeReason = x.UpgradeDowngradeReason,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient membership tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientMembershipDetailResponse>.Ok(
                data,
                "Detail patient membership berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Patient Membership", Description = "Membuat data patient membership", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientMembership", "Create")]
        public async Task<IActionResult> CreatePatientMembership([FromBody] CreatePatientMembershipRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                patientId: request.PatientId,
                membershipTierId: request.MembershipTierId,
                memberNumber: request.MemberNumber,
                joinDate: request.JoinDate,
                expiredDate: request.ExpiredDate,
                pointBalance: request.PointBalance,
                totalSpendAmount: request.TotalSpendAmount,
                lastUpgradeDate: request.LastUpgradeDate,
                lastDowngradeDate: request.LastDowngradeDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient membership tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimary)
            {
                await ClearOtherPrimaryMembershipsAsync(request.PatientId, null, now, actorUserId);
            }

            var entity = new MstPatientMembership
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                MembershipTierId = request.MembershipTierId,
                MemberNumber = request.MemberNumber.Trim().ToUpperInvariant(),
                MembershipStatus = request.MembershipStatus,
                JoinDate = NormalizeDateTime(request.JoinDate),
                ExpiredDate = NormalizeNullableDateTime(request.ExpiredDate),
                IsPrimary = request.IsPrimary,
                IsAutoCreated = request.IsAutoCreated,
                IsCreatedFromKiosk = request.IsCreatedFromKiosk,
                IsCreatedFromAdmission = request.IsCreatedFromAdmission,
                IsCreatedByMarketing = request.IsCreatedByMarketing,
                PointBalance = request.PointBalance,
                TotalSpendAmount = request.TotalSpendAmount,
                LastUpgradeDate = NormalizeNullableDateTime(request.LastUpgradeDate),
                LastDowngradeDate = NormalizeNullableDateTime(request.LastDowngradeDate),
                UpgradeDowngradeReason = NormalizeNullableText(request.UpgradeDowngradeReason),
                Notes = NormalizeNullableText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPatientMembership>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await SyncPatientMembershipReferenceAsync(entity.PatientId, entity.Id, entity.MembershipTierId, entity.IsPrimary, now, actorUserId);
            await _dbContext.SaveChangesAsync();

            var response = new PatientMembershipCreateResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                MembershipTierId = entity.MembershipTierId,
                MemberNumber = entity.MemberNumber,
                MembershipStatus = entity.MembershipStatus,
                IsPrimary = entity.IsPrimary,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<PatientMembershipCreateResponse>.Ok(
                response,
                "Patient membership berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Membership", Description = "Mengubah data patient membership", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientMembership", "Update")]
        public async Task<IActionResult> UpdatePatientMembership(Guid id, [FromBody] UpdatePatientMembershipRequest request)
        {
            var entity = await _dbContext.Set<MstPatientMembership>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient membership tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                patientId: request.PatientId,
                membershipTierId: request.MembershipTierId,
                memberNumber: request.MemberNumber,
                joinDate: request.JoinDate,
                expiredDate: request.ExpiredDate,
                pointBalance: request.PointBalance,
                totalSpendAmount: request.TotalSpendAmount,
                lastUpgradeDate: request.LastUpgradeDate,
                lastDowngradeDate: request.LastDowngradeDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient membership tidak valid."
                ));
            }

            var oldPatientId = entity.PatientId;
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimary)
            {
                await ClearOtherPrimaryMembershipsAsync(request.PatientId, id, now, actorUserId);
            }

            entity.PatientId = request.PatientId;
            entity.MembershipTierId = request.MembershipTierId;
            entity.MemberNumber = request.MemberNumber.Trim().ToUpperInvariant();
            entity.MembershipStatus = request.MembershipStatus;
            entity.JoinDate = NormalizeDateTime(request.JoinDate);
            entity.ExpiredDate = NormalizeNullableDateTime(request.ExpiredDate);
            entity.IsPrimary = request.IsPrimary;
            entity.IsAutoCreated = request.IsAutoCreated;
            entity.IsCreatedFromKiosk = request.IsCreatedFromKiosk;
            entity.IsCreatedFromAdmission = request.IsCreatedFromAdmission;
            entity.IsCreatedByMarketing = request.IsCreatedByMarketing;
            entity.PointBalance = request.PointBalance;
            entity.TotalSpendAmount = request.TotalSpendAmount;
            entity.LastUpgradeDate = NormalizeNullableDateTime(request.LastUpgradeDate);
            entity.LastDowngradeDate = NormalizeNullableDateTime(request.LastDowngradeDate);
            entity.UpgradeDowngradeReason = NormalizeNullableText(request.UpgradeDowngradeReason);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SyncPatientMembershipReferenceAsync(entity.PatientId, entity.Id, entity.MembershipTierId, entity.IsPrimary, now, actorUserId);

            if (oldPatientId != entity.PatientId)
            {
                await RecalculatePatientMembershipReferenceAsync(oldPatientId, now, actorUserId);
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient membership berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Membership", Description = "Menghapus data patient membership", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PatientMembership", "Delete")]
        public async Task<IActionResult> DeletePatientMembership(Guid id)
        {
            var entity = await _dbContext.Set<MstPatientMembership>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient membership tidak ditemukan."
                ));
            }

            var patientId = entity.PatientId;
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;

            await RecalculatePatientMembershipReferenceAsync(patientId, now, actorUserId);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient membership berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid patientId,
            Guid membershipTierId,
            string memberNumber,
            DateTime joinDate,
            DateTime? expiredDate,
            int pointBalance,
            decimal totalSpendAmount,
            DateTime? lastUpgradeDate,
            DateTime? lastDowngradeDate)
        {
            if (patientId == Guid.Empty)
                return (false, "Pasien wajib dipilih.");

            if (membershipTierId == Guid.Empty)
                return (false, "Membership tier wajib dipilih.");

            if (string.IsNullOrWhiteSpace(memberNumber))
                return (false, "Nomor member wajib diisi.");

            if (pointBalance < 0)
                return (false, "Point balance tidak boleh kurang dari 0.");

            if (totalSpendAmount < 0)
                return (false, "Total spend amount tidak boleh kurang dari 0.");

            var normalizedJoinDate = NormalizeDateTime(joinDate);
            var normalizedExpiredDate = NormalizeNullableDateTime(expiredDate);
            var normalizedLastUpgradeDate = NormalizeNullableDateTime(lastUpgradeDate);
            var normalizedLastDowngradeDate = NormalizeNullableDateTime(lastDowngradeDate);

            if (normalizedExpiredDate.HasValue && normalizedExpiredDate.Value < normalizedJoinDate)
                return (false, "Tanggal expired tidak boleh lebih kecil dari tanggal join.");

            if (normalizedLastUpgradeDate.HasValue && normalizedLastUpgradeDate.Value < normalizedJoinDate)
                return (false, "Tanggal upgrade terakhir tidak boleh lebih kecil dari tanggal join.");

            if (normalizedLastDowngradeDate.HasValue && normalizedLastDowngradeDate.Value < normalizedJoinDate)
                return (false, "Tanggal downgrade terakhir tidak boleh lebih kecil dari tanggal join.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == patientId && x.IsActive && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak valid atau tidak aktif.");

            var tierExists = await _dbContext.Set<MstMembershipTier>()
                .AnyAsync(x => x.Id == membershipTierId && x.IsActive && !x.IsDelete);

            if (!tierExists)
                return (false, "Membership tier tidak valid atau tidak aktif.");

            var normalizedMemberNumber = memberNumber.Trim().ToUpperInvariant();

            var duplicateMemberNumber = await _dbContext.Set<MstPatientMembership>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.MemberNumber.ToUpper() == normalizedMemberNumber &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateMemberNumber)
                return (false, "Nomor member sudah digunakan.");

            var duplicateTierForPatient = await _dbContext.Set<MstPatientMembership>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.MembershipTierId == membershipTierId &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateTierForPatient)
                return (false, "Pasien sudah memiliki membership tier tersebut.");

            return (true, null);
        }

        private async Task ClearOtherPrimaryMembershipsAsync(Guid patientId, Guid? excludeId, DateTime now, Guid actorUserId)
        {
            var query = _dbContext.Set<MstPatientMembership>()
                .Where(x => !x.IsDelete && x.PatientId == patientId && x.IsPrimary);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            var otherMemberships = await query.ToListAsync();

            foreach (var membership in otherMemberships)
            {
                membership.IsPrimary = false;
                membership.UpdateDateTime = now;
                membership.UpdateBy = actorUserId;
            }
        }

        private async Task SyncPatientMembershipReferenceAsync(
            Guid patientId,
            Guid membershipId,
            Guid membershipTierId,
            bool isPrimary,
            DateTime now,
            Guid actorUserId)
        {
            if (!isPrimary)
                return;

            var patient = await _dbContext.Set<MstPatient>()
                .FirstOrDefaultAsync(x => x.Id == patientId && !x.IsDelete);

            if (patient == null)
                return;

            patient.IsMember = true;
            patient.DefaultMembershipTierId = membershipTierId;
            patient.ActivePatientMembershipId = membershipId;
            patient.UpdateDateTime = now;
            patient.UpdateBy = actorUserId;
        }

        private async Task RecalculatePatientMembershipReferenceAsync(Guid patientId, DateTime now, Guid actorUserId)
        {
            var patient = await _dbContext.Set<MstPatient>()
                .FirstOrDefaultAsync(x => x.Id == patientId && !x.IsDelete);

            if (patient == null)
                return;

            var activePrimaryMembership = await _dbContext.Set<MstPatientMembership>()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.IsActive &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .OrderByDescending(x => x.JoinDate)
                .FirstOrDefaultAsync();

            if (activePrimaryMembership != null)
            {
                patient.IsMember = true;
                patient.DefaultMembershipTierId = activePrimaryMembership.MembershipTierId;
                patient.ActivePatientMembershipId = activePrimaryMembership.Id;
            }
            else
            {
                patient.IsMember = false;
                patient.DefaultMembershipTierId = null;
                patient.ActivePatientMembershipId = null;
            }

            patient.UpdateDateTime = now;
            patient.UpdateBy = actorUserId;
        }

        private static IQueryable<MstPatientMembership> ApplySorting(
            IQueryable<MstPatientMembership> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "joinDate").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "membernumber" => isDesc
                    ? query.OrderByDescending(x => x.MemberNumber)
                    : query.OrderBy(x => x.MemberNumber),

                "patientfullname" => isDesc
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : "")
                    : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : ""),

                "medicalrecordnumber" => isDesc
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : "")
                    : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : ""),

                "tiername" => isDesc
                    ? query.OrderByDescending(x => x.MembershipTier != null ? x.MembershipTier.TierName : "")
                    : query.OrderBy(x => x.MembershipTier != null ? x.MembershipTier.TierName : ""),

                "membershipstatus" => isDesc
                    ? query.OrderByDescending(x => x.MembershipStatus)
                    : query.OrderBy(x => x.MembershipStatus),

                "expireddate" => isDesc
                    ? query.OrderByDescending(x => x.ExpiredDate)
                    : query.OrderBy(x => x.ExpiredDate),

                "pointbalance" => isDesc
                    ? query.OrderByDescending(x => x.PointBalance)
                    : query.OrderBy(x => x.PointBalance),

                "totalspendamount" => isDesc
                    ? query.OrderByDescending(x => x.TotalSpendAmount)
                    : query.OrderBy(x => x.TotalSpendAmount),

                "isprimary" => isDesc
                    ? query.OrderByDescending(x => x.IsPrimary)
                    : query.OrderBy(x => x.IsPrimary),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.JoinDate).ThenByDescending(x => x.MemberNumber)
                    : query.OrderBy(x => x.JoinDate).ThenBy(x => x.MemberNumber)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientMembershipEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientMembershipEnumOptionResponse
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

        private static DateTime NormalizeDateTime(DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        private static DateTime? NormalizeNullableDateTime(DateTime? value)
        {
            if (!value.HasValue)
                return null;

            return NormalizeDateTime(value.Value);
        }
    }
}
