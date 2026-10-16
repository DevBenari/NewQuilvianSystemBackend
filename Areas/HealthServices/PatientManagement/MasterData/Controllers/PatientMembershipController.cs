using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
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
        private readonly PatientMembershipStatusResolver _statusResolver;
        private readonly PatientMembershipService _membershipService;

        public PatientMembershipController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            PatientMembershipStatusResolver statusResolver,
            PatientMembershipService membershipService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _statusResolver = statusResolver;
            _membershipService = membershipService;
        }

        [HttpGet("filters/metadata")]
        [HttpGet("kiosk/filters/metadata")]
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

        [HttpGet("admin/filters/metadata")]
        [AccessPermission("PatientMembership", "Read")]
        public Task<IActionResult> GetFilterMetadataForAdmin()
        {
            return GetFilterMetadata();
        }


        [HttpGet("summary")]
        [HttpGet("admin/summary")]
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
                MarketingCreatedPatientMembership = await query.CountAsync(x => x.IsCreatedByMarketing),
                PendingPaymentPatientMembership = await query.CountAsync(x => x.MembershipStatus == MembershipStatus.PendingPayment),
                ScheduledPatientMembership = await query.CountAsync(x => x.MembershipStatus == MembershipStatus.Scheduled),
                NeedsVerificationPatientMembership = await query.CountAsync(x => x.MembershipStatus == MembershipStatus.NeedsVerification),
                CancelledPatientMembership = await query.CountAsync(x => x.MembershipStatus == MembershipStatus.Cancelled),
                ExpiringIn30DaysPatientMembership = await ApplyExpiringWithinDays(query, 30).CountAsync()
            };

            return Ok(ApiResponse<PatientMembershipSummaryResponse>.Ok(
                result,
                "Ringkasan patient membership berhasil diambil."
            ));
        }

        [HttpGet]
        [HttpGet("kiosk")]
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
            [FromQuery] int pageSize = 25,
            [FromQuery] MembershipStatus? membershipStatus = null,
            [FromQuery] int? expiringWithinDays = null)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyRelationFilter(query, patientId, membershipTierId);
            query = ApplyStandardFilter(query, isActive, search);

            if (membershipStatus.HasValue)
            {
                query = query.Where(x => x.MembershipStatus == membershipStatus.Value);
            }

            if (expiringWithinDays is > 0)
            {
                query = ApplyExpiringWithinDays(query, Math.Min(expiringWithinDays.Value, 366));
            }

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

        [HttpGet("admin")]
        [AccessPermission("PatientMembership", "Read")]
        public Task<IActionResult> GetPatientMembershipsForAdmin(
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
            [FromQuery] int pageSize = 25,
            [FromQuery] MembershipStatus? membershipStatus = null,
            [FromQuery] int? expiringWithinDays = null)
        {
            return GetPatientMemberships(startDate, endDate, customPeriod, patientId, membershipTierId, isActive, search, sortBy, sortDirection, pageNumber, pageSize, membershipStatus, expiringWithinDays);
        }


        [HttpGet("options")]
        [HttpGet("kiosk/options")]
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
            query = ApplyStandardFilter(query, null, search);

            if (onlyActive)
            {
                // crm-membership BE-CRM-021: "aktif" = Aktif dalam periode hari ini (aturan tunggal J1).
                var activeIds = _statusResolver.QueryActiveOn(_statusResolver.GetTodayWib()).Select(x => x.Id);
                query = query.Where(x => activeIds.Contains(x.Id));
            }

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

        [HttpGet("admin/options")]
        [AccessPermission("PatientMembership", "Read")]
        public Task<IActionResult> GetPatientMembershipOptionsForAdmin(
            [FromQuery] bool onlyActive = true,
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? membershipTierId = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            return GetPatientMembershipOptions(onlyActive, patientId, membershipTierId, search, pageNumber, pageSize);
        }


        [HttpGet("{id:guid}")]
        [HttpGet("admin/{id:guid}")]
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

        // crm-membership BE-CRM-011 (CRM-DEC-004): pendaftaran baru oleh petugas CRM, tersimpan
        // Menunggu Pembayaran dengan nomor member dari sistem.
        [HttpPost("admin")]
        [AccessAction(
            "Create",
            "Create Patient Membership",
            Description = "Mendaftarkan, memperpanjang, atau meng-upgrade patient membership",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessPermission("PatientMembership", "Create")]
        public async Task<IActionResult> RegisterPatientMembershipForAdmin(
            [FromBody] RegisterPatientMembershipRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _membershipService.RegisterAsync(request, GetCurrentUserId(), cancellationToken);
            return await ToCommandResponseAsync(result, cancellationToken);
        }

        // crm-membership BE-CRM-012 (CRM-DEC-024): perpanjangan membership Aktif {id}.
        [HttpPost("admin/{id:guid}/renewals")]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessPermission("PatientMembership", "Create")]
        public async Task<IActionResult> RenewPatientMembership(
            Guid id,
            [FromBody] RenewPatientMembershipRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _membershipService.RenewAsync(id, request, GetCurrentUserId(), cancellationToken);
            return await ToCommandResponseAsync(result, cancellationToken);
        }

        // crm-membership BE-CRM-012 (CRM-DEC-012): upgrade membership Aktif {id} ke tingkat lebih tinggi.
        [HttpPost("admin/{id:guid}/upgrades")]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessPermission("PatientMembership", "Create")]
        public async Task<IActionResult> UpgradePatientMembership(
            Guid id,
            [FromBody] UpgradePatientMembershipRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _membershipService.UpgradeAsync(id, request, GetCurrentUserId(), cancellationToken);
            return await ToCommandResponseAsync(result, cancellationToken);
        }

        // crm-membership BE-CRM-013 (CRM-DEC-013, 025, 029): pembatalan oleh petugas CRM.
        [HttpPost("admin/{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(
            "Cancel",
            "Cancel Patient Membership",
            Description = "Membatalkan patient membership dengan alasan",
            AccessType = AccessTypes.Update,
            SortOrder = 5
        )]
        [AccessPermission("PatientMembership", "Cancel")]
        public async Task<IActionResult> CancelPatientMembership(
            Guid id,
            [FromBody] CancelPatientMembershipRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _membershipService.CancelAsync(id, request, GetCurrentUserId(), cancellationToken);
            return await ToCommandResponseAsync(result, cancellationToken);
        }

        // crm-membership BE-CRM-013 (CRM-DEC-021): mengakui atau menolak membership lama.
        [HttpPost("admin/{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(
            "Verify",
            "Verify Patient Membership",
            Description = "Mengakui atau menolak patient membership lama",
            AccessType = AccessTypes.Update,
            SortOrder = 6
        )]
        [AccessPermission("PatientMembership", "Verify")]
        public async Task<IActionResult> VerifyLegacyPatientMembership(
            Guid id,
            [FromBody] VerifyLegacyPatientMembershipRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _membershipService.VerifyLegacyAsync(id, request, GetCurrentUserId(), cancellationToken);
            return await ToCommandResponseAsync(result, cancellationToken);
        }

        // crm-membership BE-CRM-013 (CRM-DEC-022): ubah catatan.
        [HttpPatch("admin/{id:guid}/notes")]
        [ProducesResponseType(typeof(ApiResponse<PatientMembershipDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessPermission("PatientMembership", "Update")]
        public async Task<IActionResult> UpdatePatientMembershipNotes(
            Guid id,
            [FromBody] UpdatePatientMembershipNotesRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _membershipService.UpdateNotesAsync(id, request, GetCurrentUserId(), cancellationToken);
            return await ToCommandResponseAsync(result, cancellationToken);
        }

        // crm-membership BE-CRM-013: riwayat status satu membership.
        [HttpGet("admin/{id:guid}/status-histories")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientMembershipStatusHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessPermission("PatientMembership", "Read")]
        public async Task<IActionResult> GetPatientMembershipStatusHistories(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var data = await _membershipService.GetStatusHistoriesAsync(id, cancellationToken);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Membership tidak ditemukan."));
            }

            return Ok(ApiResponse<List<PatientMembershipStatusHistoryResponse>>.Ok(data, "Riwayat status membership berhasil diambil."));
        }

        // crm-membership BE-CRM-014 (CRM-DEC-010): cari pasien beserta status membernya.
        [HttpGet("admin/patient-lookup")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientMembershipLookupResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessPermission("PatientMembership", "Read")]
        public async Task<IActionResult> LookupPatientsForMembership(
            [FromQuery] PatientMembershipLookupQuery query,
            CancellationToken cancellationToken = default)
        {
            var data = await _membershipService.LookupPatientsAsync(query, cancellationToken);

            if (data == null)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Ketik minimal 3 karakter No. RM, NIK, atau No. Telp."));
            }

            return Ok(ApiResponse<PagedResult<PatientMembershipLookupResponse>>.Ok(data, "Data pasien berhasil diambil."));
        }

        // crm-membership BE-CRM-014 (J1): status member pasien pada tanggal WIB tertentu.
        [HttpGet("admin/patients/{patientId:guid}/active")]
        [ProducesResponseType(typeof(ApiResponse<PatientActiveMembershipResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessPermission("PatientMembership", "Read")]
        public async Task<IActionResult> GetPatientActiveMembership(
            Guid patientId,
            [FromQuery] DateTime? date,
            CancellationToken cancellationToken = default)
        {
            var data = await _membershipService.GetPatientActiveMembershipAsync(patientId, date, cancellationToken);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pasien tidak ditemukan."));
            }

            return Ok(ApiResponse<PatientActiveMembershipResponse>.Ok(data, "Status member pasien berhasil diambil."));
        }

        // Gagal: kode dan pesan dari service. Berhasil: detail membership yang terdampak.
        private async Task<IActionResult> ToCommandResponseAsync(
            PatientMembershipCommandResult result,
            CancellationToken cancellationToken)
        {
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            var entity = await BuildBaseQuery().FirstAsync(x => x.Id == result.PatientMembershipId, cancellationToken);
            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });

            return Ok(ApiResponse<PatientMembershipDetailResponse>.Ok(MapDetailResponse(entity, actorNames), result.Message));
        }


        // crm-membership BE-CRM-015 (CRM-DEC-022): ubah penuh dipensiunkan.
        [HttpPut("{id:guid}")]
        [HttpPut("admin/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status410Gone)]
        [AccessAction(
            "Update",
            "Update Patient Membership",
            Description = "Mengubah catatan patient membership",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientMembership", "Update")]
        public IActionResult UpdatePatientMembership(Guid id)
        {
            return StatusCode(StatusCodes.Status410Gone, ApiResponse<object>.Fail(
                StatusCodes.Status410Gone,
                "Fitur ubah membership sudah tidak tersedia. Gunakan ubah catatan, upgrade, atau pembatalan."));
        }

        // crm-membership BE-CRM-015 (CRM-DEC-013): status tidak boleh dibalik tanpa alasan.
        [HttpPatch("{id:guid}/status")]
        [HttpPatch("admin/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status410Gone)]
        [AccessPermission("PatientMembership", "Update")]
        public IActionResult UpdatePatientMembershipStatus(Guid id)
        {
            return StatusCode(StatusCodes.Status410Gone, ApiResponse<object>.Fail(
                StatusCodes.Status410Gone,
                "Ubah status membership sudah tidak tersedia. Gunakan pembatalan atau verifikasi."));
        }

        // crm-membership BE-CRM-015 (CRM-DEC-013): riwayat membership tidak boleh hilang.
        [HttpDelete("{id:guid}")]
        [HttpDelete("admin/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status410Gone)]
        [AccessAction(
            "Delete",
            "Delete Patient Membership",
            Description = "Menghapus patient membership (dipensiunkan, diganti pembatalan)",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("PatientMembership", "Delete")]
        public IActionResult DeletePatientMembership(Guid id)
        {
            return StatusCode(StatusCodes.Status410Gone, ApiResponse<object>.Fail(
                StatusCodes.Status410Gone,
                "Hapus membership sudah tidak tersedia. Gunakan pembatalan dengan alasan."));
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
                var today = AppDateTimeHelper.OperationalDate();

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

        /// <summary>Aktif yang tanggal akhirnya jatuh antara hari ini dan N hari ke depan (kalender WIB).</summary>
        private IQueryable<MstPatientMembership> ApplyExpiringWithinDays(
            IQueryable<MstPatientMembership> query,
            int days)
        {
            var today = _statusResolver.GetTodayWib();
            var until = today.AddDays(days);

            return query.Where(x =>
                x.MembershipStatus == MembershipStatus.Active &&
                x.ExpiredDate != null &&
                x.ExpiredDate >= today &&
                x.ExpiredDate <= until);
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
                RegistrationType = entity.RegistrationType,
                RegistrationTypeName = BuildEnumLabel(entity.RegistrationType),
                PeriodStartDate = entity.PeriodStartDate,
                PaymentDueAt = entity.PaymentDueAt,
                ActivatedAt = entity.ActivatedAt,
                FeeAmountSnapshot = entity.FeeAmountSnapshot,
                PreviousPatientMembershipId = entity.PreviousPatientMembershipId,
                CancellationCategory = entity.CancellationCategory,
                CancellationCategoryName = entity.CancellationCategory.HasValue ? BuildEnumLabel(entity.CancellationCategory.Value) : null,
                RowVersion = entity.RowVersion,
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
                RegistrationType = entity.RegistrationType,
                RegistrationTypeName = BuildEnumLabel(entity.RegistrationType),
                PeriodStartDate = entity.PeriodStartDate,
                PaymentDueAt = entity.PaymentDueAt,
                ActivatedAt = entity.ActivatedAt,
                FeeAmountSnapshot = entity.FeeAmountSnapshot,
                PreviousPatientMembershipId = entity.PreviousPatientMembershipId,
                CancellationCategory = entity.CancellationCategory,
                CancellationCategoryName = entity.CancellationCategory.HasValue ? BuildEnumLabel(entity.CancellationCategory.Value) : null,
                RowVersion = entity.RowVersion,
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
                CancellationReason = entity.CancellationReason,
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
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
