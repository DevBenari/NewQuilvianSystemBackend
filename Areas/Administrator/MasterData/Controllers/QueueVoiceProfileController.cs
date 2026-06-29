using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Text.RegularExpressions;

using ResponseQueueVoiceProfilePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.QueueVoiceProfileResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/queue-voice-profiles")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Queue Voice Profile",
        AreaName = "Administrator",
        ControllerName = "QueueVoiceProfile",
        Description = "Administrator master data profil suara panggilan antrean",
        SortOrder = 105
    )]
    [Tags("Administrator / Master Data / Queue Voice Profile")]
    public class QueueVoiceProfileController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private const string CodePrefix = "QVP-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public QueueVoiceProfileController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Queue Voice Profile",
            Description = "Melihat metadata filter profil suara panggilan antrean",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("QueueVoiceProfile", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var genderOptions = BuildGenderOptions();

            var result = new QueueVoiceProfileFilterMetadataResponse
            {
                DefaultFilter = new QueueVoiceProfileDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<QueueVoiceProfileSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "voiceCode", Label = "Kode voice" },
                    new() { Value = "voiceName", Label = "Nama voice" },
                    new() { Value = "gender", Label = "Gender" },
                    new() { Value = "language", Label = "Bahasa" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EnumOptions = BuildEnumMetadataOptions(genderOptions),
                GenderOptions = genderOptions,
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "QueueVoiceProfile.GetFilterMetadata",
                "Mengambil metadata filter profil suara panggilan antrean.",
                result
            );

            return Ok(ApiResponse<QueueVoiceProfileFilterMetadataResponse>.Ok(
                result,
                "Metadata filter profil suara berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Queue Voice Profile",
            Description = "Melihat ringkasan profil suara panggilan antrean",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("QueueVoiceProfile", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new QueueVoiceProfileSummaryResponse
            {
                TotalProfile = await query.CountAsync(),
                ActiveProfile = await query.CountAsync(x => x.IsActive),
                InactiveProfile = await query.CountAsync(x => !x.IsActive),
                DefaultProfile = await query.CountAsync(x => x.IsDefault),
                FemaleProfile = await query.CountAsync(x => x.Gender.ToLower() == "female"),
                MaleProfile = await query.CountAsync(x => x.Gender.ToLower() == "male"),
                UnknownGenderProfile = await query.CountAsync(x => x.Gender.ToLower() == "unknown")
            };

            return Ok(ApiResponse<QueueVoiceProfileSummaryResponse>.Ok(
                result,
                "Ringkasan profil suara berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseQueueVoiceProfilePagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Queue Voice Profile",
            Description = "Melihat data profil suara panggilan antrean",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("QueueVoiceProfile", "Read")]
        public async Task<IActionResult> GetQueueVoiceProfiles(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? gender,
            [FromQuery] string? language,
            [FromQuery] bool? isDefault,
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
            query = ApplyStandardFilter(query, search, isActive, gender, language, isDefault);

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

            var result = new ResponseQueueVoiceProfilePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseQueueVoiceProfilePagedResult>.Ok(
                result,
                "Data profil suara berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Queue Voice Profile",
            Description = "Melihat data pilihan profil suara panggilan antrean",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("QueueVoiceProfile", "Read")]
        public async Task<IActionResult> GetQueueVoiceProfileOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? gender = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(
                query,
                search,
                onlyActive ? true : null,
                gender,
                language: null,
                isDefault: null
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.VoiceName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new QueueVoiceProfileOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<QueueVoiceProfileOptionPagedResponse>.Ok(
                result,
                "Data pilihan profil suara berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Queue Voice Profile Detail",
            Description = "Melihat detail profil suara panggilan antrean",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("QueueVoiceProfile", "Read")]
        public async Task<IActionResult> GetQueueVoiceProfileById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil suara tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            if (!data.CreateBy.HasValue || data.CreateBy.Value == Guid.Empty)
            {
                data.CreateBy = null;
                data.CreateByName = null;
            }

            if (!data.UpdateBy.HasValue || data.UpdateBy.Value == Guid.Empty)
            {
                data.UpdateBy = null;
                data.UpdateByName = null;
            }

            return Ok(ApiResponse<QueueVoiceProfileDetailResponse>.Ok(
                data,
                "Detail profil suara berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Queue Voice Profile",
            Description = "Membuat profil suara panggilan antrean",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("QueueVoiceProfile", "Create")]
        public async Task<IActionResult> CreateQueueVoiceProfile([FromBody] CreateQueueVoiceProfileRequest request)
        {
            var voiceCode = string.IsNullOrWhiteSpace(request.VoiceCode)
                ? await GenerateCodeAsync()
                : NormalizeCode(request.VoiceCode);

            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request,
                normalizedVoiceCode: voiceCode
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil suara tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultProfilesAsync(
                        exceptId: null,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                var entity = new MstQueueVoiceProfile
                {
                    Id = Guid.NewGuid(),
                    VoiceCode = voiceCode,
                    VoiceName = request.VoiceName.Trim(),
                    Gender = NormalizeGender(request.Gender),
                    Language = NormalizeLanguage(request.Language),
                    ModelPath = request.ModelPath.Trim(),
                    LengthScale = Clamp(request.LengthScale, 0.70m, 1.50m),
                    NoiseScale = Clamp(request.NoiseScale, 0.10m, 1.50m),
                    NoiseW = Clamp(request.NoiseW, 0.10m, 1.50m),
                    Volume = Clamp(request.Volume, 0.50m, 2.00m),
                    IsDefault = request.IsDefault,
                    SortOrder = request.SortOrder,
                    Description = NormalizeNullableString(request.Description),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstQueueVoiceProfile>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
                var result = new QueueVoiceProfileCreateResponse
                {
                    Id = entity.Id,
                    VoiceCode = entity.VoiceCode,
                    VoiceName = entity.VoiceName,
                    Gender = entity.Gender,
                    GenderName = BuildGenderLabel(entity.Gender),
                    IsDefault = entity.IsDefault,
                    IsActive = entity.IsActive,
                    CreateDateTime = entity.CreateDateTime,
                    CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                    CreateByName = GetActorName(actorNames, entity.CreateBy)
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "QueueVoiceProfile.CreateQueueVoiceProfile",
                    "Membuat profil suara panggilan antrean.",
                    result
                );

                return Ok(ApiResponse<QueueVoiceProfileCreateResponse>.Ok(
                    result,
                    "Profil suara berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "QueueVoiceProfile.CreateQueueVoiceProfile",
                    "Gagal membuat profil suara panggilan antrean.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat profil suara."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Queue Voice Profile",
            Description = "Mengubah profil suara panggilan antrean",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("QueueVoiceProfile", "Update")]
        public async Task<IActionResult> UpdateQueueVoiceProfile(
            Guid id,
            [FromBody] UpdateQueueVoiceProfileRequest request)
        {
            var entity = await _dbContext.Set<MstQueueVoiceProfile>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil suara tidak ditemukan."
                ));
            }

            if (request.IsDefault && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Profil default harus aktif."
                ));
            }

            var voiceCode = string.IsNullOrWhiteSpace(request.VoiceCode)
                ? entity.VoiceCode
                : NormalizeCode(request.VoiceCode);

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request,
                normalizedVoiceCode: voiceCode
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil suara tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultProfilesAsync(
                        exceptId: id,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                entity.VoiceCode = voiceCode;
                entity.VoiceName = request.VoiceName.Trim();
                entity.Gender = NormalizeGender(request.Gender);
                entity.Language = NormalizeLanguage(request.Language);
                entity.ModelPath = request.ModelPath.Trim();
                entity.LengthScale = Clamp(request.LengthScale, 0.70m, 1.50m);
                entity.NoiseScale = Clamp(request.NoiseScale, 0.10m, 1.50m);
                entity.NoiseW = Clamp(request.NoiseW, 0.10m, 1.50m);
                entity.Volume = Clamp(request.Volume, 0.50m, 2.00m);
                entity.IsDefault = request.IsActive ? request.IsDefault : false;
                entity.IsActive = request.IsActive;
                entity.SortOrder = request.SortOrder;
                entity.Description = NormalizeNullableString(request.Description);
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
                var result = new QueueVoiceProfileUpdateResponse
                {
                    Id = entity.Id,
                    VoiceCode = entity.VoiceCode,
                    VoiceName = entity.VoiceName,
                    Gender = entity.Gender,
                    GenderName = BuildGenderLabel(entity.Gender),
                    IsDefault = entity.IsDefault,
                    IsActive = entity.IsActive,
                    UpdateDateTime = entity.UpdateDateTime,
                    UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                    UpdateByName = GetActorName(actorNames, entity.UpdateBy)
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "QueueVoiceProfile.UpdateQueueVoiceProfile",
                    "Mengubah profil suara panggilan antrean.",
                    result
                );

                return Ok(ApiResponse<QueueVoiceProfileUpdateResponse>.Ok(
                    result,
                    "Profil suara berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "QueueVoiceProfile.UpdateQueueVoiceProfile",
                    "Gagal mengubah profil suara panggilan antrean.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat memperbarui profil suara."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Queue Voice Profile Status",
            Description = "Mengubah status profil suara panggilan antrean",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("QueueVoiceProfile", "Update")]
        public async Task<IActionResult> UpdateQueueVoiceProfileStatus(
            Guid id,
            [FromBody] UpdateQueueVoiceProfileStatusRequest request)
        {
            var entity = await _dbContext.Set<MstQueueVoiceProfile>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil suara tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            if (!request.IsActive)
            {
                entity.IsDefault = false;
            }

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                entity.Description = request.Reason.Trim();
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new QueueVoiceProfileUpdateResponse
            {
                Id = entity.Id,
                VoiceCode = entity.VoiceCode,
                VoiceName = entity.VoiceName,
                Gender = entity.Gender,
                GenderName = BuildGenderLabel(entity.Gender),
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "QueueVoiceProfile.UpdateQueueVoiceProfileStatus",
                "Mengubah status profil suara panggilan antrean.",
                result
            );

            return Ok(ApiResponse<QueueVoiceProfileUpdateResponse>.Ok(
                result,
                "Status profil suara berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Queue Voice Profile",
            Description = "Menghapus profil suara panggilan antrean",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("QueueVoiceProfile", "Delete")]
        public async Task<IActionResult> DeleteQueueVoiceProfile(
            Guid id,
            [FromBody] DeleteQueueVoiceProfileRequest? request = null)
        {
            var entity = await _dbContext.Set<MstQueueVoiceProfile>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil suara tidak ditemukan."
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
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            var result = new QueueVoiceProfileDeleteResponse
            {
                Id = entity.Id,
                VoiceCode = entity.VoiceCode,
                VoiceName = entity.VoiceName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "QueueVoiceProfile.DeleteQueueVoiceProfile",
                "Menghapus profil suara panggilan antrean.",
                result
            );

            return Ok(ApiResponse<QueueVoiceProfileDeleteResponse>.Ok(
                result,
                "Profil suara berhasil dihapus."
            ));
        }

        private IQueryable<MstQueueVoiceProfile> BuildBaseQuery()
        {
            return _dbContext.Set<MstQueueVoiceProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstQueueVoiceProfile> ApplyDateFilter(
            IQueryable<MstQueueVoiceProfile> query,
            ResolvedDateRange dateRange)
        {
            if (dateRange.StartDateUtc.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.StartDateUtc.Value);
            }

            if (dateRange.EndDateUtcExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndDateUtcExclusive.Value);
            }

            return query;
        }

        private static IQueryable<MstQueueVoiceProfile> ApplyStandardFilter(
            IQueryable<MstQueueVoiceProfile> query,
            string? search,
            bool? isActive,
            string? gender,
            string? language,
            bool? isDefault)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(gender))
            {
                var normalizedGender = NormalizeGender(gender);
                query = query.Where(x => x.Gender.ToLower() == normalizedGender.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                var normalizedLanguage = language.Trim().ToLower();
                query = query.Where(x => x.Language.ToLower() == normalizedLanguage);
            }

            if (isDefault.HasValue)
            {
                query = query.Where(x => x.IsDefault == isDefault.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.VoiceCode.ToLower().Contains(keyword) ||
                    x.VoiceName.ToLower().Contains(keyword) ||
                    x.Gender.ToLower().Contains(keyword) ||
                    x.Language.ToLower().Contains(keyword) ||
                    x.ModelPath.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstQueueVoiceProfile> ApplySorting(
            IQueryable<MstQueueVoiceProfile> query,
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

                "voicecode" => isDescending
                    ? query.OrderByDescending(x => x.VoiceCode)
                    : query.OrderBy(x => x.VoiceCode),

                "voicename" => isDescending
                    ? query.OrderByDescending(x => x.VoiceName)
                    : query.OrderBy(x => x.VoiceName),

                "gender" => isDescending
                    ? query.OrderByDescending(x => x.Gender).ThenBy(x => x.VoiceName)
                    : query.OrderBy(x => x.Gender).ThenBy(x => x.VoiceName),

                "language" => isDescending
                    ? query.OrderByDescending(x => x.Language).ThenBy(x => x.VoiceName)
                    : query.OrderBy(x => x.Language).ThenBy(x => x.VoiceName),

                "isdefault" => isDescending
                    ? query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.VoiceName)
                    : query.OrderBy(x => x.IsDefault).ThenBy(x => x.VoiceName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.VoiceName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.VoiceName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.VoiceName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.VoiceName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateQueueVoiceProfileRequest request,
            string normalizedVoiceCode)
        {
            if (string.IsNullOrWhiteSpace(normalizedVoiceCode))
            {
                return (false, "Kode voice wajib diisi atau biarkan kosong agar dibuat otomatis.");
            }

            if (!Regex.IsMatch(normalizedVoiceCode, "^[a-zA-Z0-9_-]{3,80}$"))
            {
                return (false, "Kode voice hanya boleh berisi huruf, angka, underscore, atau strip dengan panjang 3 sampai 80 karakter.");
            }

            if (string.IsNullOrWhiteSpace(request.VoiceName))
            {
                return (false, "Nama voice wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.ModelPath))
            {
                return (false, "Model path wajib diisi.");
            }

            var normalizedGender = NormalizeGender(request.Gender);
            if (normalizedGender == "Unknown" && !string.IsNullOrWhiteSpace(request.Gender))
            {
                return (false, "Gender tidak valid. Gunakan Female, Male, atau Unknown.");
            }

            var duplicateCodeQuery = _dbContext.Set<MstQueueVoiceProfile>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.VoiceCode.ToLower() == normalizedVoiceCode.ToLower());

            if (excludeId.HasValue)
            {
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateCodeQuery.AnyAsync())
            {
                return (false, "Kode voice sudah digunakan.");
            }

            var normalizedName = request.VoiceName.Trim().ToLower();
            var duplicateNameQuery = _dbContext.Set<MstQueueVoiceProfile>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.VoiceName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama voice sudah digunakan.");
            }

            if (request.LengthScale < 0.70m || request.LengthScale > 1.50m)
            {
                return (false, "Length scale harus berada di antara 0.70 sampai 1.50.");
            }

            if (request.NoiseScale < 0.10m || request.NoiseScale > 1.50m)
            {
                return (false, "Noise scale harus berada di antara 0.10 sampai 1.50.");
            }

            if (request.NoiseW < 0.10m || request.NoiseW > 1.50m)
            {
                return (false, "Noise W harus berada di antara 0.10 sampai 1.50.");
            }

            if (request.Volume < 0.50m || request.Volume > 2.00m)
            {
                return (false, "Volume harus berada di antara 0.50 sampai 2.00.");
            }

            return (true, null);
        }

        private async Task UnsetOtherDefaultProfilesAsync(
            Guid? exceptId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstQueueVoiceProfile>()
                .Where(x =>
                    x.IsDefault &&
                    !x.IsDelete);

            if (exceptId.HasValue)
            {
                query = query.Where(x => x.Id != exceptId.Value);
            }

            var profiles = await query.ToListAsync();

            foreach (var profile in profiles)
            {
                profile.IsDefault = false;
                profile.UpdateDateTime = now;
                profile.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstQueueVoiceProfile>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.VoiceCode.StartsWith(CodePrefix))
                .Select(x => x.VoiceCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
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

        private static QueueVoiceProfileResponse MapResponse(
            MstQueueVoiceProfile entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new QueueVoiceProfileResponse
            {
                Id = entity.Id,
                VoiceCode = entity.VoiceCode,
                VoiceName = entity.VoiceName,
                Gender = entity.Gender,
                GenderName = BuildGenderLabel(entity.Gender),
                Language = entity.Language,
                ModelPath = entity.ModelPath,
                LengthScale = entity.LengthScale,
                NoiseScale = entity.NoiseScale,
                NoiseW = entity.NoiseW,
                Volume = entity.Volume,
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static QueueVoiceProfileDetailResponse MapDetailResponse(
            MstQueueVoiceProfile entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new QueueVoiceProfileDetailResponse
            {
                Id = entity.Id,
                VoiceCode = entity.VoiceCode,
                VoiceName = entity.VoiceName,
                Gender = entity.Gender,
                GenderName = BuildGenderLabel(entity.Gender),
                Language = entity.Language,
                ModelPath = entity.ModelPath,
                LengthScale = entity.LengthScale,
                NoiseScale = entity.NoiseScale,
                NoiseW = entity.NoiseW,
                Volume = entity.Volume,
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static QueueVoiceProfileOptionResponse MapOptionResponse(MstQueueVoiceProfile entity)
        {
            return new QueueVoiceProfileOptionResponse
            {
                Id = entity.Id,
                VoiceCode = entity.VoiceCode,
                VoiceName = entity.VoiceName,
                Gender = entity.Gender,
                GenderName = BuildGenderLabel(entity.Gender),
                Language = entity.Language,
                ModelPath = entity.ModelPath,
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder
            };
        }

        private static List<QueueVoiceProfileGenderOptionResponse> BuildGenderOptions()
        {
            return new List<QueueVoiceProfileGenderOptionResponse>
            {
                new() { Value = "Female", Name = "Female", Label = "Female" },
                new() { Value = "Male", Name = "Male", Label = "Male" },
                new() { Value = "Unknown", Name = "Unknown", Label = "Unknown" }
            };
        }

        private static List<QueueVoiceProfileEnumMetadataResponse> BuildEnumMetadataOptions(
            List<QueueVoiceProfileGenderOptionResponse> genderOptions)
        {
            return new List<QueueVoiceProfileEnumMetadataResponse>
            {
                new()
                {
                    EnumName = "QueueVoiceGender",
                    FieldName = "gender",
                    OptionsSource = "genderOptions",
                    Description = "Gender suara profil antrean.",
                    Options = genderOptions
                        .Select(x => new QueueVoiceProfileEnumOptionResponse
                        {
                            Value = x.Value,
                            Name = x.Name,
                            Label = x.Label
                        })
                        .ToList()
                }
            };
        }

        private static List<QueueVoiceProfileCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<QueueVoiceProfileCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat sejak 7 hari terakhir termasuk hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static List<QueueVoiceProfileQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<QueueVoiceProfileQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal mulai filter createDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter createDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "today, last7days, thismonth, lastmonth, atau custom.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama, gender, bahasa, model path, atau deskripsi.", Example = "female" },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif." },
                new() { Name = "gender", Type = "string", Description = "Filter gender suara.", Example = "Female" },
                new() { Name = "language", Type = "string", Description = "Filter bahasa.", Example = "id-ID" },
                new() { Name = "isDefault", Type = "bool?", Description = "Filter profil default." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<QueueVoiceProfileFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFormFieldMetadata(isUpdate: false);
        }

        private static List<QueueVoiceProfileFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildFormFieldMetadata(isUpdate: true);
            fields.Add(new QueueVoiceProfileFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status Aktif",
                Section = "Status",
                InputType = "switch",
                IsRequiredOnCreate = false,
                IsRequiredOnUpdate = true,
                RequiredType = "RequiredOnUpdate",
                Description = "Menentukan apakah profil suara aktif.",
                SortOrder = 100
            });
            return fields;
        }

        private static List<QueueVoiceProfileFormFieldMetadataResponse> BuildFormFieldMetadata(bool isUpdate)
        {
            return new List<QueueVoiceProfileFormFieldMetadataResponse>
            {
                new() { Name = "voiceCode", Label = "Kode Voice", Section = "Informasi Utama", InputType = "text", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional", MaxLength = 80, Description = "Boleh dikosongkan saat create agar dibuat otomatis. Jika dipakai sebagai DefaultVoiceCode appsettings, isi sesuai kode yang digunakan service.", Example = "id_ID_female_default", SortOrder = 1 },
                new() { Name = "voiceName", Label = "Nama Voice", Section = "Informasi Utama", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Always", MaxLength = 150, Description = "Nama profil suara yang ditampilkan di master data.", Example = "Indonesia Female Default", SortOrder = 2 },
                new() { Name = "gender", Label = "Gender", Section = "Informasi Utama", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Always", MaxLength = 20, OptionsSource = "genderOptions", Description = "Gender suara.", Example = "Female", SortOrder = 3 },
                new() { Name = "language", Label = "Bahasa", Section = "Informasi Utama", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Always", MaxLength = 20, Description = "Kode bahasa/locale voice.", Example = "id-ID", SortOrder = 4 },
                new() { Name = "modelPath", Label = "Model Path", Section = "Piper", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Always", MaxLength = 500, Description = "Path model .onnx relatif dari root aplikasi/container.", Example = "Storage/PiperVoices/id_ID/female/default.onnx", SortOrder = 5 },
                new() { Name = "lengthScale", Label = "Length Scale", Section = "Tuning Suara", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Always", MinValue = 0.70m, MaxValue = 1.50m, Description = "Semakin besar semakin lambat. Rekomendasi RS 1.05 sampai 1.18.", Example = "1.08", SortOrder = 6 },
                new() { Name = "noiseScale", Label = "Noise Scale", Section = "Tuning Suara", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Always", MinValue = 0.10m, MaxValue = 1.50m, Description = "Mengatur variasi/naturalness suara.", Example = "0.65", SortOrder = 7 },
                new() { Name = "noiseW", Label = "Noise W", Section = "Tuning Suara", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Always", MinValue = 0.10m, MaxValue = 1.50m, Description = "Mengatur ekspresi/intonasi model Piper.", Example = "0.80", SortOrder = 8 },
                new() { Name = "volume", Label = "Volume", Section = "Tuning Suara", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Always", MinValue = 0.50m, MaxValue = 2.00m, Description = "Penguatan volume setelah audio dibuat.", Example = "1.15", SortOrder = 9 },
                new() { Name = "isDefault", Label = "Default", Section = "Status", InputType = "switch", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional", Description = "Jika aktif, profil default lain otomatis dilepas.", SortOrder = 10 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Status", InputType = "number", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional", Description = "Urutan tampil data.", Example = "0", SortOrder = 11 },
                new() { Name = "description", Label = "Deskripsi", Section = "Catatan", InputType = "textarea", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional", MaxLength = 250, Description = "Catatan profil suara.", Example = "Model male Indonesia belum tersedia.", SortOrder = 12 }
            };
        }

        private static string BuildGenderLabel(string? value)
        {
            return NormalizeGender(value) switch
            {
                "Female" => "Female",
                "Male" => "Male",
                "Unknown" => "Unknown",
                _ => "Unknown"
            };
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

        private static ResolvedDateRange ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var normalizedPeriod = (customPeriod ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(normalizedPeriod))
            {
                return BuildCustomRange(startDate, endDate);
            }

            if (normalizedPeriod == "custom")
            {
                return BuildCustomRange(startDate, endDate);
            }

            if (startDate.HasValue || endDate.HasValue)
            {
                startDate = null;
                endDate = null;
            }

            var today = DateTime.UtcNow.Date;

            return normalizedPeriod switch
            {
                "today" => new ResolvedDateRange(true, today, today.AddDays(1), null),
                "last7days" => new ResolvedDateRange(true, today.AddDays(-6), today.AddDays(1), null),
                "thismonth" => BuildMonthRange(today.Year, today.Month),
                "lastmonth" => BuildMonthRange(today.AddMonths(-1).Year, today.AddMonths(-1).Month),
                _ => new ResolvedDateRange(false, null, null, "Custom period tidak valid.")
            };
        }

        private static ResolvedDateRange BuildCustomRange(
            DateTime? startDate,
            DateTime? endDate)
        {
            DateTime? startUtc = null;
            DateTime? endUtcExclusive = null;

            if (startDate.HasValue)
            {
                startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            }

            if (endDate.HasValue)
            {
                endUtcExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            }

            if (startUtc.HasValue && endUtcExclusive.HasValue && startUtc.Value >= endUtcExclusive.Value)
            {
                return new ResolvedDateRange(false, null, null, "Tanggal mulai tidak boleh lebih besar atau sama dengan tanggal akhir.");
            }

            return new ResolvedDateRange(true, startUtc, endUtcExclusive, null);
        }

        private static ResolvedDateRange BuildMonthRange(int year, int month)
        {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            return new ResolvedDateRange(true, start, start.AddMonths(1), null);
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

        private static string NormalizeCode(string? value)
        {
            var normalized = Regex.Replace(value ?? string.Empty, "[^a-zA-Z0-9_-]", "_").Trim('_');
            return normalized;
        }

        private static string NormalizeGender(string? value)
        {
            var normalized = (value ?? string.Empty).Trim();

            if (string.Equals(normalized, "Male", StringComparison.OrdinalIgnoreCase))
            {
                return "Male";
            }

            if (string.Equals(normalized, "Female", StringComparison.OrdinalIgnoreCase))
            {
                return "Female";
            }

            if (string.Equals(normalized, "Unknown", StringComparison.OrdinalIgnoreCase))
            {
                return "Unknown";
            }

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return "Female";
            }

            return "Unknown";
        }

        private static string NormalizeLanguage(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "id-ID"
                : value.Trim();
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
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

        private sealed record ResolvedDateRange(
            bool IsValid,
            DateTime? StartDateUtc,
            DateTime? EndDateUtcExclusive,
            string? ErrorMessage
        );
    }
}
