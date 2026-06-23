using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
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
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Patient Membership",
        AreaName = "HealthServices",
        ControllerName = "PatientMembership",
        Description = "Health service patient management master data patient membership",
        SortOrder = 18
    )]
    [Tags("Health Services / Patient Management / Master Data / Patient Membership")]
    public class PatientMembershipController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";
        private const string KioskReadPolicy = "KioskRead";

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
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Membership",
            Description = "Melihat metadata filter patient membership",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientMembershipFilterMetadataResponse
            {
                DefaultFilter = new PatientMembershipDefaultFilterResponse(),
                CustomPeriods = new List<PatientMembershipCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<PatientMembershipSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal diperbarui" },
                    new() { Value = "joinDate", Label = "Tanggal bergabung" },
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
                RelationFilters = new List<PatientMembershipRelationFilterResponse>
                {
                    new()
                    {
                        Value = "patientId",
                        Label = "Patient",
                        Endpoint = "/api/v1/health-services/patient-management/master-data/patients/options"
                    },
                    new()
                    {
                        Value = "membershipTierId",
                        Label = "Membership Tier",
                        Endpoint = "/api/v1/health-services/master-data/membership-tiers/options"
                    }
                },
                MembershipStatusOptions = BuildEnumOptions<MembershipStatus>(),
                MembershipTierTypeOptions = BuildEnumOptions<MembershipTierType>(),
                ResetButtonLabel = "Reset"
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
        [AccessAction(
            "Read",
            "Read Patient Membership",
            Description = "Melihat ringkasan patient membership",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
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
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientMembershipPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Membership",
            Description = "Melihat data patient membership",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetPatientMemberships(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? membershipTierId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyRelationFilter(query, patientId, membershipTierId);
            query = ApplyStandardFilter(query, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

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
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Membership",
            Description = "Melihat data pilihan patient membership",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetPatientMembershipOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? membershipTierId = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyRelationFilter(query, patientId, membershipTierId);
            query = ApplyStandardFilter(query, onlyActive ? true : null, search);

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.JoinDate)
                .ThenBy(x => x.MemberNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new PatientMembershipOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PatientMembershipOptionPagedResponse>.Ok(
                result,
                "Data pilihan patient membership berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Patient Membership",
            Description = "Melihat detail patient membership",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientMembership", "Read")]
        public async Task<IActionResult> GetPatientMembershipById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient membership tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<PatientMembershipDetailResponse>.Ok(
                data,
                "Detail patient membership berhasil diambil."
            ));
        }

        [HttpPost]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Patient Membership",
            Description = "Membuat data patient membership",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        public async Task<IActionResult> CreatePatientMembership([FromBody] CreatePatientMembershipRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                patientId: request.PatientId,
                membershipTierId: request.MembershipTierId,
                memberNumber: request.MemberNumber,
                membershipStatus: request.MembershipStatus,
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
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
                    UpgradeDowngradeReason = NormalizeNullableString(request.UpgradeDowngradeReason),
                    Notes = NormalizeNullableString(request.Notes),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstPatientMembership>().Add(entity);

                if (entity.IsPrimary && entity.IsActive)
                {
                    await SyncPatientMembershipReferenceAsync(entity.PatientId, entity.Id, entity.MembershipTierId, now, actorUserId);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await BuildCreateResponseAsync(entity.Id);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientMembership.CreatePatientMembership",
                    "Membuat data patient membership.",
                    response
                );

                return Ok(ApiResponse<PatientMembershipCreateResponse>.Ok(
                    response,
                    "Patient membership berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientMembership.CreatePatientMembership",
                    "Gagal membuat data patient membership.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat patient membership."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Patient Membership",
            Description = "Mengubah data patient membership",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientMembership", "Update")]
        public async Task<IActionResult> UpdatePatientMembership(
            Guid id,
            [FromBody] UpdatePatientMembershipRequest request)
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
                membershipStatus: request.MembershipStatus,
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

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
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
                entity.UpgradeDowngradeReason = NormalizeNullableString(request.UpgradeDowngradeReason);
                entity.Notes = NormalizeNullableString(request.Notes);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();

                if (entity.IsPrimary && entity.IsActive)
                {
                    await SyncPatientMembershipReferenceAsync(entity.PatientId, entity.Id, entity.MembershipTierId, now, actorUserId);
                }
                else
                {
                    await RecalculatePatientMembershipReferenceAsync(entity.PatientId, now, actorUserId);
                }

                if (oldPatientId != entity.PatientId)
                {
                    await RecalculatePatientMembershipReferenceAsync(oldPatientId, now, actorUserId);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientMembership.UpdatePatientMembership",
                    "Mengubah data patient membership.",
                    new
                    {
                        entity.Id,
                        entity.PatientId,
                        entity.MembershipTierId,
                        entity.MemberNumber,
                        entity.IsActive
                    }
                );

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Patient membership berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientMembership.UpdatePatientMembership",
                    "Gagal mengubah data patient membership.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah patient membership."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Patient Membership Status",
            Description = "Mengubah status patient membership",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientMembership", "Update")]
        public async Task<IActionResult> UpdatePatientMembershipStatus(
            Guid id,
            [FromBody] UpdatePatientMembershipStatusRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await RecalculatePatientMembershipReferenceAsync(entity.PatientId, now, actorUserId);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status patient membership berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Patient Membership",
            Description = "Menghapus data patient membership",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("PatientMembership", "Delete")]
        public async Task<IActionResult> DeletePatientMembership(
            Guid id,
            [FromBody] DeletePatientMembershipRequest? request = null)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var patientId = entity.PatientId;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Notes = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();
            await RecalculatePatientMembershipReferenceAsync(patientId, now, actorUserId);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient membership berhasil dihapus."
            ));
        }

        private IQueryable<MstPatientMembership> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.MembershipTier)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatientMembership> ApplyDateFilter(
            IQueryable<MstPatientMembership> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue &&
                !endDate.HasValue &&
                !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = DateTime.UtcNow.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x =>
                            x.CreateDateTime >= today &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x =>
                            x.CreateDateTime >= today.AddDays(-6) &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x =>
                            x.CreateDateTime >= thisMonthStart &&
                            x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x =>
                            x.CreateDateTime >= lastMonthStart &&
                            x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstPatientMembership> ApplyRelationFilter(
            IQueryable<MstPatientMembership> query,
            Guid? patientId,
            Guid? membershipTierId)
        {
            var normalizedPatientId = NormalizeNullableGuid(patientId);
            if (normalizedPatientId.HasValue)
            {
                query = query.Where(x => x.PatientId == normalizedPatientId.Value);
            }

            var normalizedMembershipTierId = NormalizeNullableGuid(membershipTierId);
            if (normalizedMembershipTierId.HasValue)
            {
                query = query.Where(x => x.MembershipTierId == normalizedMembershipTierId.Value);
            }

            return query;
        }

        private static IQueryable<MstPatientMembership> ApplyStandardFilter(
            IQueryable<MstPatientMembership> query,
            bool? isActive,
            string? search)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedMembershipStatuses = Enum.GetValues<MembershipStatus>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x).ToLower().Contains(keyword))
                    .ToList();

                var matchedTierTypes = Enum.GetValues<MembershipTierType>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.MemberNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.MembershipTier != null && x.MembershipTier.TierCode.ToLower().Contains(keyword)) ||
                    (x.MembershipTier != null && x.MembershipTier.TierName.ToLower().Contains(keyword)) ||
                    (x.UpgradeDowngradeReason != null && x.UpgradeDowngradeReason.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
                    matchedMembershipStatuses.Contains(x.MembershipStatus) ||
                    (x.MembershipTier != null && matchedTierTypes.Contains(x.MembershipTier.TierType)));
            }

            return query;
        }

        private static IOrderedQueryable<MstPatientMembership> ApplySorting(
            IQueryable<MstPatientMembership> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "updatedatetime" => isDescending
                    ? query.OrderByDescending(x => x.UpdateDateTime).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.UpdateDateTime).ThenBy(x => x.CreateDateTime),

                "joindate" => isDescending
                    ? query.OrderByDescending(x => x.JoinDate).ThenByDescending(x => x.MemberNumber)
                    : query.OrderBy(x => x.JoinDate).ThenBy(x => x.MemberNumber),

                "membernumber" => isDescending
                    ? query.OrderByDescending(x => x.MemberNumber)
                    : query.OrderBy(x => x.MemberNumber),

                "patientfullname" => isDescending
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                "medicalrecordnumber" => isDescending
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty)
                    : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty),

                "tiername" => isDescending
                    ? query.OrderByDescending(x => x.MembershipTier != null ? x.MembershipTier.TierName : string.Empty)
                    : query.OrderBy(x => x.MembershipTier != null ? x.MembershipTier.TierName : string.Empty),

                "membershipstatus" => isDescending
                    ? query.OrderByDescending(x => x.MembershipStatus).ThenBy(x => x.MemberNumber)
                    : query.OrderBy(x => x.MembershipStatus).ThenBy(x => x.MemberNumber),

                "expireddate" => isDescending
                    ? query.OrderByDescending(x => x.ExpiredDate).ThenBy(x => x.MemberNumber)
                    : query.OrderBy(x => x.ExpiredDate).ThenBy(x => x.MemberNumber),

                "pointbalance" => isDescending
                    ? query.OrderByDescending(x => x.PointBalance).ThenBy(x => x.MemberNumber)
                    : query.OrderBy(x => x.PointBalance).ThenBy(x => x.MemberNumber),

                "totalspendamount" => isDescending
                    ? query.OrderByDescending(x => x.TotalSpendAmount).ThenBy(x => x.MemberNumber)
                    : query.OrderBy(x => x.TotalSpendAmount).ThenBy(x => x.MemberNumber),

                "isprimary" => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.MemberNumber)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.MemberNumber),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.MemberNumber)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.MemberNumber),

                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.MemberNumber)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.MemberNumber)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid patientId,
            Guid membershipTierId,
            string memberNumber,
            MembershipStatus membershipStatus,
            DateTime joinDate,
            DateTime? expiredDate,
            int pointBalance,
            decimal totalSpendAmount,
            DateTime? lastUpgradeDate,
            DateTime? lastDowngradeDate)
        {
            if (patientId == Guid.Empty)
            {
                return (false, "Pasien wajib dipilih.");
            }

            if (membershipTierId == Guid.Empty)
            {
                return (false, "Membership tier wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(memberNumber))
            {
                return (false, "Nomor member wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(MembershipStatus), membershipStatus))
            {
                return (false, "Status membership tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (pointBalance < 0)
            {
                return (false, "Point balance tidak boleh kurang dari 0.");
            }

            if (totalSpendAmount < 0)
            {
                return (false, "Total spend amount tidak boleh kurang dari 0.");
            }

            var normalizedJoinDate = NormalizeDateTime(joinDate);
            var normalizedExpiredDate = NormalizeNullableDateTime(expiredDate);
            var normalizedLastUpgradeDate = NormalizeNullableDateTime(lastUpgradeDate);
            var normalizedLastDowngradeDate = NormalizeNullableDateTime(lastDowngradeDate);

            if (normalizedExpiredDate.HasValue && normalizedExpiredDate.Value < normalizedJoinDate)
            {
                return (false, "Tanggal expired tidak boleh lebih kecil dari tanggal join.");
            }

            if (normalizedLastUpgradeDate.HasValue && normalizedLastUpgradeDate.Value < normalizedJoinDate)
            {
                return (false, "Tanggal upgrade terakhir tidak boleh lebih kecil dari tanggal join.");
            }

            if (normalizedLastDowngradeDate.HasValue && normalizedLastDowngradeDate.Value < normalizedJoinDate)
            {
                return (false, "Tanggal downgrade terakhir tidak boleh lebih kecil dari tanggal join.");
            }

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == patientId && x.IsActive && !x.IsDelete);

            if (!patientExists)
            {
                return (false, "Pasien tidak valid atau tidak aktif.");
            }

            var tierExists = await _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == membershipTierId && x.IsActive && !x.IsDelete);

            if (!tierExists)
            {
                return (false, "Membership tier tidak valid atau tidak aktif.");
            }

            var normalizedMemberNumber = memberNumber.Trim().ToUpperInvariant();

            var duplicateMemberNumber = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.MemberNumber.ToUpper() == normalizedMemberNumber &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateMemberNumber)
            {
                return (false, "Nomor member sudah digunakan.");
            }

            var duplicateTierForPatient = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.MembershipTierId == membershipTierId &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateTierForPatient)
            {
                return (false, "Pasien sudah memiliki membership tier tersebut.");
            }

            return (true, null);
        }

        private async Task ClearOtherPrimaryMembershipsAsync(
            Guid patientId,
            Guid? excludeId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstPatientMembership>()
                .Where(x => !x.IsDelete && x.PatientId == patientId && x.IsPrimary);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

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
            DateTime now,
            Guid actorUserId)
        {
            var patient = await _dbContext.Set<MstPatient>()
                .FirstOrDefaultAsync(x => x.Id == patientId && !x.IsDelete);

            if (patient == null)
            {
                return;
            }

            patient.IsMember = true;
            patient.DefaultMembershipTierId = membershipTierId;
            patient.ActivePatientMembershipId = membershipId;
            patient.UpdateDateTime = now;
            patient.UpdateBy = actorUserId;
        }

        private async Task RecalculatePatientMembershipReferenceAsync(
            Guid patientId,
            DateTime now,
            Guid actorUserId)
        {
            var patient = await _dbContext.Set<MstPatient>()
                .FirstOrDefaultAsync(x => x.Id == patientId && !x.IsDelete);

            if (patient == null)
            {
                return;
            }

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

        private async Task<PatientMembershipCreateResponse> BuildCreateResponseAsync(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstAsync(x => x.Id == id);

            return new PatientMembershipCreateResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientFullName = entity.Patient?.FullName ?? string.Empty,
                MembershipTierId = entity.MembershipTierId,
                TierName = entity.MembershipTier?.TierName ?? string.Empty,
                MemberNumber = entity.MemberNumber,
                MembershipStatus = entity.MembershipStatus,
                MembershipStatusName = BuildEnumLabel(entity.MembershipStatus),
                IsPrimary = entity.IsPrimary,
                IsActive = entity.IsActive
            };
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

        private static PatientMembershipResponse MapResponse(
            MstPatientMembership entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientMembershipResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientFullName = entity.Patient?.FullName ?? string.Empty,
                MembershipTierId = entity.MembershipTierId,
                TierCode = entity.MembershipTier?.TierCode ?? string.Empty,
                TierName = entity.MembershipTier?.TierName ?? string.Empty,
                TierType = entity.MembershipTier?.TierType ?? MembershipTierType.Regular,
                TierTypeName = entity.MembershipTier != null ? BuildEnumLabel(entity.MembershipTier.TierType) : string.Empty,
                MemberNumber = entity.MemberNumber,
                MembershipStatus = entity.MembershipStatus,
                MembershipStatusName = BuildEnumLabel(entity.MembershipStatus),
                JoinDate = entity.JoinDate,
                ExpiredDate = entity.ExpiredDate,
                IsPrimary = entity.IsPrimary,
                IsAutoCreated = entity.IsAutoCreated,
                IsCreatedFromKiosk = entity.IsCreatedFromKiosk,
                IsCreatedFromAdmission = entity.IsCreatedFromAdmission,
                IsCreatedByMarketing = entity.IsCreatedByMarketing,
                PointBalance = entity.PointBalance,
                TotalSpendAmount = entity.TotalSpendAmount,
                LastUpgradeDate = entity.LastUpgradeDate,
                LastDowngradeDate = entity.LastDowngradeDate,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PatientMembershipDetailResponse MapDetailResponse(
            MstPatientMembership entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new PatientMembershipDetailResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientFullName = entity.Patient?.FullName ?? string.Empty,
                MembershipTierId = entity.MembershipTierId,
                TierCode = entity.MembershipTier?.TierCode ?? string.Empty,
                TierName = entity.MembershipTier?.TierName ?? string.Empty,
                TierType = entity.MembershipTier?.TierType ?? MembershipTierType.Regular,
                TierTypeName = entity.MembershipTier != null ? BuildEnumLabel(entity.MembershipTier.TierType) : string.Empty,
                MemberNumber = entity.MemberNumber,
                MembershipStatus = entity.MembershipStatus,
                MembershipStatusName = BuildEnumLabel(entity.MembershipStatus),
                JoinDate = entity.JoinDate,
                ExpiredDate = entity.ExpiredDate,
                IsPrimary = entity.IsPrimary,
                IsAutoCreated = entity.IsAutoCreated,
                IsCreatedFromKiosk = entity.IsCreatedFromKiosk,
                IsCreatedFromAdmission = entity.IsCreatedFromAdmission,
                IsCreatedByMarketing = entity.IsCreatedByMarketing,
                PointBalance = entity.PointBalance,
                TotalSpendAmount = entity.TotalSpendAmount,
                LastUpgradeDate = entity.LastUpgradeDate,
                LastDowngradeDate = entity.LastDowngradeDate,
                UpgradeDowngradeReason = entity.UpgradeDowngradeReason,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return response;
        }

        private static PatientMembershipOptionResponse MapOptionResponse(MstPatientMembership entity)
        {
            return new PatientMembershipOptionResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientFullName = entity.Patient?.FullName ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                MembershipTierId = entity.MembershipTierId,
                TierName = entity.MembershipTier?.TierName ?? string.Empty,
                MemberNumber = entity.MemberNumber,
                MembershipStatus = entity.MembershipStatus,
                MembershipStatusName = BuildEnumLabel(entity.MembershipStatus),
                IsPrimary = entity.IsPrimary,
                JoinDate = entity.JoinDate,
                ExpiredDate = entity.ExpiredDate
            };
        }

        private static List<PatientMembershipEnumOptionResponse> BuildEnumOptions<TEnum>()
            where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientMembershipEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x)
                })
                .ToList();
        }

        private static string BuildEnumLabel<TEnum>(TEnum value)
            where TEnum : Enum
        {
            return SplitPascalCase(value.ToString());
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
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

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
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
            {
                return null;
            }

            return NormalizeDateTime(value.Value);
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
