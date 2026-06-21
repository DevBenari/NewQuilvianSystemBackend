using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QRCoder;
using System.Security.Claims;

using ResponsePatientPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.PatientResponse>;
using System.Runtime.InteropServices;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using ImageSharpImage = SixLabors.ImageSharp.Image;
using ImageSharpColor = SixLabors.ImageSharp.Color;
using ImageSharpPoint = SixLabors.ImageSharp.Point;
using ImageSharpSize = SixLabors.ImageSharp.Size;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/patients")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Patient",
        AreaName = "HealthServices",
        ControllerName = "Patient",
        Description = "Health service patient management master data patient",
        SortOrder = 1
    )]
    [Tags("Health Services / Patient Management / Master Data / Patient")]
    public class PatientController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";
        private const string KioskReadPolicy = "KioskRead";
        private const string CodePrefix = "PAT-RSMMC-";
        private const int CodeNumberLength = 5;
        private const string PatientQrCodeFolderName = "patient-qrcodes";
        private const string DefaultPublicRequestPath = "/uploads";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public PatientController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
            _configuration = configuration;
        }

        [HttpGet("filters/metadata")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient",
            Description = "Melihat metadata filter patient",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientFilterMetadataResponse
            {
                DefaultFilter = new PatientDefaultFilterResponse(),
                CustomPeriods = new List<PatientCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<PatientSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal diperbarui" },
                    new() { Value = "patientCode", Label = "Kode pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "fullName", Label = "Nama pasien" },
                    new() { Value = "birthDate", Label = "Tanggal lahir" },
                    new() { Value = "patientType", Label = "Tipe pasien" },
                    new() { Value = "patientStatus", Label = "Status pasien" },
                    new() { Value = "registrationSource", Label = "Sumber registrasi" },
                    new() { Value = "isMember", Label = "Member" },
                    new() { Value = "isNewborn", Label = "Newborn" },
                    new() { Value = "isDeceased", Label = "Meninggal" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RelationFilters = new List<PatientRelationFilterResponse>
                {
                    new()
                    {
                        Value = "defaultMembershipTierId",
                        Label = "Membership Tier",
                        Endpoint = "/api/v1/health-services/master-data/membership-tiers/options"
                    }
                },
                PatientTypeOptions = BuildEnumOptions<PatientType>(),
                PatientStatusOptions = BuildEnumOptions<PatientStatus>(),
                RegistrationSourceOptions = BuildEnumOptions<PatientRegistrationSource>(),
                GenderOptions = BuildEnumOptions<Gender>(),
                ReligionOptions = BuildEnumOptions<Religion>(),
                MaritalStatusOptions = BuildEnumOptions<MaritalStatus>(),
                BloodTypeOptions = BuildEnumOptions<BloodType>(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Patient.GetFilterMetadata",
                "Mengambil metadata filter patient.",
                result
            );

            return Ok(ApiResponse<PatientFilterMetadataResponse>.Ok(
                result,
                "Metadata filter patient berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient",
            Description = "Melihat ringkasan patient",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Patient", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new PatientSummaryResponse
            {
                TotalPatient = await query.CountAsync(),
                ActivePatient = await query.CountAsync(x => x.IsActive),
                InactivePatient = await query.CountAsync(x => !x.IsActive),
                GeneralPatient = await query.CountAsync(x => x.PatientType == PatientType.General),
                NewbornPatient = await query.CountAsync(x => x.IsNewborn),
                MemberPatient = await query.CountAsync(x => x.IsMember),
                DeceasedPatient = await query.CountAsync(x => x.IsDeceased),
                MergedPatient = await query.CountAsync(x => x.MergedToPatientId.HasValue),
                WithIdentityNumberPatient = await query.CountAsync(x =>
                    x.IdentityNumber != null &&
                    x.IdentityNumber != string.Empty)
            };

            return Ok(ApiResponse<PatientSummaryResponse>.Ok(
                result,
                "Ringkasan patient berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient",
            Description = "Melihat data patient",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Patient", "Read")]
        public async Task<IActionResult> GetPatients(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? defaultMembershipTierId,
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
            query = ApplyRelationFilter(query, defaultMembershipTierId);
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

            var result = new ResponsePatientPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientPagedResult>.Ok(
                result,
                "Data patient berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient",
            Description = "Melihat data pilihan patient",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetPatientOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] Guid? defaultMembershipTierId = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyRelationFilter(query, defaultMembershipTierId);
            query = ApplyStandardFilter(
                query,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.MedicalRecordNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new PatientOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PatientOptionPagedResponse>.Ok(
                result,
                "Data pilihan patient berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Patient",
            Description = "Melihat detail patient",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Patient", "Read")]
        public async Task<IActionResult> GetPatientById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            NormalizeActorInfo(data);

            return Ok(ApiResponse<PatientDetailResponse>.Ok(
                data,
                "Detail patient berhasil diambil."
            ));
        }

        [HttpPost]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Patient",
            Description = "Membuat data patient",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request,
                mergedToPatientId: null,
                mergeReason: null
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            (string FilePath, string PhysicalPath)? savedQrCode = null;

            try
            {
                var patientCode = await GeneratePatientCodeAsync();
                var medicalRecordNumber = await GenerateMedicalRecordNumberAsync();

                var entity = new MstPatient
                {
                    Id = Guid.NewGuid(),
                    PatientCode = patientCode,
                    MedicalRecordNumber = medicalRecordNumber,
                    PatientType = request.PatientType,
                    PatientStatus = request.PatientStatus,
                    RegistrationSource = request.RegistrationSource,
                    FullName = request.FullName.Trim(),
                    NickName = NormalizeNullableString(request.NickName),
                    BirthPlace = NormalizeNullableString(request.BirthPlace),
                    BirthDate = request.BirthDate,
                    Gender = request.Gender,
                    Religion = request.Religion,
                    MaritalStatus = request.MaritalStatus,
                    BloodType = request.BloodType,
                    IdentityType = NormalizeNullableString(request.IdentityType),
                    IdentityNumber = NormalizeNullableString(request.IdentityNumber),
                    PhoneNumber = NormalizeNullableString(request.PhoneNumber),
                    WhatsAppNumber = NormalizeNullableString(request.WhatsAppNumber),
                    Email = NormalizeLowerNullableString(request.Email),
                    Address = NormalizeNullableString(request.Address),
                    CountryId = NormalizeNullableGuid(request.CountryId),
                    ProvinceId = NormalizeNullableGuid(request.ProvinceId),
                    CityId = NormalizeNullableGuid(request.CityId),
                    DistrictId = NormalizeNullableGuid(request.DistrictId),
                    PostalCodeId = NormalizeNullableGuid(request.PostalCodeId),
                    PhotoPath = NormalizeNullableString(request.PhotoPath),
                    IsMember = request.IsMember,
                    DefaultMembershipTierId = NormalizeNullableGuid(request.DefaultMembershipTierId),
                    ActivePatientMembershipId = NormalizeNullableGuid(request.ActivePatientMembershipId),
                    IsNewborn = request.IsNewborn,
                    MotherPatientId = NormalizeNullableGuid(request.MotherPatientId),
                    BirthOrder = request.BirthOrder,
                    BirthWeightGram = request.BirthWeightGram,
                    BirthLengthCm = request.BirthLengthCm,
                    BirthTime = request.BirthTime,
                    DeliveryMethod = NormalizeNullableString(request.DeliveryMethod),
                    IsDeceased = request.IsDeceased,
                    DeceasedDate = request.DeceasedDate,
                    Notes = NormalizeNullableString(request.Notes),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                WriteDockerInfoLog(
                     "PATIENT_CREATE_STEP",
                     new
                     {
                         TraceId = HttpContext.TraceIdentifier,
                         Step = "BeforeGenerateQrCode",
                         entity.PatientCode,
                         entity.MedicalRecordNumber,
                         entity.FullName,
                         ContentRootPath = _environment.ContentRootPath,
                         WebRootPath = _environment.WebRootPath,
                         CurrentDirectory = Directory.GetCurrentDirectory(),
                         OS = RuntimeInformation.OSDescription,
                         Framework = RuntimeInformation.FrameworkDescription
                     }
                 );

                try
                {
                    savedQrCode = SavePatientQrCodeFile(
                        entity.MedicalRecordNumber,
                        HttpContext.TraceIdentifier
                    );
                }
                catch (Exception qrEx)
                {
                    WriteDockerErrorLog(
                        "PATIENT_CREATE_QR_ERROR",
                        qrEx,
                        new
                        {
                            TraceId = HttpContext.TraceIdentifier,
                            entity.PatientCode,
                            entity.MedicalRecordNumber,
                            entity.FullName
                        }
                    );

                    await _loggerService.ErrorAsync(
                        LogCategory,
                        "Patient.CreatePatient.GenerateQrCode",
                        $"Gagal generate QR Code pasien. TraceId: {HttpContext.TraceIdentifier}, MRN: {entity.MedicalRecordNumber}",
                        qrEx
                    );

                    throw;
                }

                WriteDockerInfoLog(
                    "PATIENT_CREATE_STEP",
                    new
                    {
                        TraceId = HttpContext.TraceIdentifier,
                        Step = "AfterGenerateQrCode",
                        QrCodePath = savedQrCode?.FilePath,
                        QrPhysicalPath = savedQrCode?.PhysicalPath
                    }
                );

                _dbContext.Set<MstPatient>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = new
                {
                    Id = entity.Id,
                    PatientCode = entity.PatientCode,
                    MedicalRecordNumber = entity.MedicalRecordNumber,
                    FullName = entity.FullName,
                    PhotoPath = entity.PhotoPath,
                    QrCodePath = savedQrCode?.FilePath,
                    QrCodePayload = BuildPatientQrPayload(entity.MedicalRecordNumber),
                    PatientType = entity.PatientType,
                    PatientTypeName = BuildEnumLabel(entity.PatientType),
                    PatientStatus = entity.PatientStatus,
                    PatientStatusName = BuildEnumLabel(entity.PatientStatus),
                    IsNewborn = entity.IsNewborn,
                    IsMember = entity.IsMember,
                    IsActive = entity.IsActive
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "Patient.CreatePatient",
                    "Membuat data patient.",
                    result
                );

                return Ok(ApiResponse<object>.Ok(
                    result,
                    "Patient berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    WriteDockerErrorLog(
                        "PATIENT_CREATE_ROLLBACK_ERROR",
                        rollbackEx,
                        new
                        {
                            TraceId = HttpContext.TraceIdentifier
                        }
                    );
                }

                try
                {
                    DeletePhysicalFileIfExists(savedQrCode?.PhysicalPath);
                }
                catch (Exception deleteFileEx)
                {
                    WriteDockerErrorLog(
                        "PATIENT_CREATE_DELETE_QR_ERROR",
                        deleteFileEx,
                        new
                        {
                            TraceId = HttpContext.TraceIdentifier,
                            QrPhysicalPath = savedQrCode?.PhysicalPath
                        }
                    );
                }

                WriteDockerErrorLog(
                    "PATIENT_CREATE_ERROR",
                    ex,
                    new
                    {
                        TraceId = HttpContext.TraceIdentifier,
                        SavedQrCodePath = savedQrCode?.FilePath,
                        SavedQrCodePhysicalPath = savedQrCode?.PhysicalPath,
                        ContentRootPath = _environment.ContentRootPath,
                        WebRootPath = _environment.WebRootPath,
                        CurrentDirectory = Directory.GetCurrentDirectory(),
                        OS = RuntimeInformation.OSDescription,
                        Framework = RuntimeInformation.FrameworkDescription
                    }
                );

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "Patient.CreatePatient",
                    $"Gagal membuat data patient. TraceId: {HttpContext.TraceIdentifier}",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Terjadi kesalahan saat membuat patient. TraceId: {HttpContext.TraceIdentifier}"
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Update",
            "Update Patient",
            Description = "Mengubah data patient",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Patient", "Update")]
        public async Task<IActionResult> UpdatePatient(
            Guid id,
            [FromBody] UpdatePatientRequest request)
        {
            var entity = await _dbContext.Set<MstPatient>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request,
                mergedToPatientId: request.MergedToPatientId,
                mergeReason: request.MergeReason
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.PatientType = request.PatientType;
            entity.PatientStatus = request.PatientStatus;
            entity.RegistrationSource = request.RegistrationSource;
            entity.FullName = request.FullName.Trim();
            entity.NickName = NormalizeNullableString(request.NickName);
            entity.BirthPlace = NormalizeNullableString(request.BirthPlace);
            entity.BirthDate = request.BirthDate;
            entity.Gender = request.Gender;
            entity.Religion = request.Religion;
            entity.MaritalStatus = request.MaritalStatus;
            entity.BloodType = request.BloodType;
            entity.IdentityType = NormalizeNullableString(request.IdentityType);
            entity.IdentityNumber = NormalizeNullableString(request.IdentityNumber);
            entity.PhoneNumber = NormalizeNullableString(request.PhoneNumber);
            entity.WhatsAppNumber = NormalizeNullableString(request.WhatsAppNumber);
            entity.Email = NormalizeLowerNullableString(request.Email);
            entity.Address = NormalizeNullableString(request.Address);
            entity.CountryId = NormalizeNullableGuid(request.CountryId);
            entity.ProvinceId = NormalizeNullableGuid(request.ProvinceId);
            entity.CityId = NormalizeNullableGuid(request.CityId);
            entity.DistrictId = NormalizeNullableGuid(request.DistrictId);
            entity.PostalCodeId = NormalizeNullableGuid(request.PostalCodeId);
            entity.PhotoPath = NormalizeNullableString(request.PhotoPath);
            entity.IsMember = request.IsMember;
            entity.DefaultMembershipTierId = NormalizeNullableGuid(request.DefaultMembershipTierId);
            entity.ActivePatientMembershipId = NormalizeNullableGuid(request.ActivePatientMembershipId);
            entity.IsNewborn = request.IsNewborn;
            entity.MotherPatientId = NormalizeNullableGuid(request.MotherPatientId);
            entity.BirthOrder = request.BirthOrder;
            entity.BirthWeightGram = request.BirthWeightGram;
            entity.BirthLengthCm = request.BirthLengthCm;
            entity.BirthTime = request.BirthTime;
            entity.DeliveryMethod = NormalizeNullableString(request.DeliveryMethod);
            entity.IsDeceased = request.IsDeceased;
            entity.DeceasedDate = request.DeceasedDate;
            entity.MergedToPatientId = NormalizeNullableGuid(request.MergedToPatientId);
            entity.MergeReason = NormalizeNullableString(request.MergeReason);
            entity.Notes = NormalizeNullableString(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Patient.UpdatePatient",
                "Mengubah data patient.",
                new
                {
                    entity.Id,
                    entity.PatientCode,
                    entity.MedicalRecordNumber,
                    entity.FullName,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Patient Status",
            Description = "Mengubah status patient",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Patient", "Update")]
        public async Task<IActionResult> UpdatePatientStatus(
            Guid id,
            [FromBody] UpdatePatientStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPatient>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status patient berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Delete",
            "Delete Patient",
            Description = "Menghapus data patient",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Patient", "Delete")]
        public async Task<IActionResult> DeletePatient(
            Guid id,
            [FromBody] DeletePatientRequest? request = null)
        {
            var entity = await _dbContext.Set<MstPatient>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient tidak ditemukan."
                ));
            }

            var isUsedByIdentityDocument = await _dbContext.Set<MstPatientIdentityDocument>()
                .AnyAsync(x => x.PatientId == id && !x.IsDelete);

            var isUsedByRelationship = await _dbContext.Set<MstPatientRelationship>()
                .AnyAsync(x =>
                    (x.PatientId == id || x.RelatedPatientId == id) &&
                    !x.IsDelete);

            var isUsedByEmergencyContact = await _dbContext.Set<MstPatientEmergencyContact>()
                .AnyAsync(x => x.PatientId == id && !x.IsDelete);

            var isUsedByMembership = await _dbContext.Set<MstPatientMembership>()
                .AnyAsync(x => x.PatientId == id && !x.IsDelete);

            var isUsedAsReference = await _dbContext.Set<MstPatient>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    (x.MotherPatientId == id || x.MergedToPatientId == id));

            if (isUsedByIdentityDocument ||
                isUsedByRelationship ||
                isUsedByEmergencyContact ||
                isUsedByMembership ||
                isUsedAsReference)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient tidak dapat dihapus karena sudah digunakan oleh dokumen identitas, relasi, kontak darurat, membership, atau referensi pasien lain."
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

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Notes = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Patient.DeletePatient",
                "Menghapus data patient.",
                new
                {
                    entity.Id,
                    entity.PatientCode,
                    entity.MedicalRecordNumber,
                    entity.FullName,
                    entity.DeleteDateTime
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient berhasil dihapus."
            ));
        }

        private (string FilePath, string PhysicalPath) SavePatientQrCodeFile(
     string medicalRecordNumber,
     string? traceId = null)
        {
            string step = "Start";
            string? storageRootPath = null;
            string? publicRequestPath = null;
            string? qrFolderName = null;
            string? relativeFolder = null;
            string? absoluteFolder = null;
            string? physicalPath = null;
            string? logoPath = null;
            string? qrPayload = null;
            int qrPngByteLength = 0;

            try
            {
                step = "ValidateMedicalRecordNumber";

                if (string.IsNullOrWhiteSpace(medicalRecordNumber))
                {
                    throw new InvalidOperationException("Nomor rekam medis tidak tersedia untuk membuat QR Code pasien.");
                }

                step = "GetFileStoragePaths";
                var storage = GetFileStoragePaths();

                storageRootPath = storage.RootPath;
                publicRequestPath = storage.PublicRequestPath;

                qrFolderName = SanitizePathSegment(medicalRecordNumber);
                relativeFolder = Path.Combine(PatientQrCodeFolderName, qrFolderName);
                absoluteFolder = Path.Combine(storage.RootPath, relativeFolder);

                WriteDockerInfoLog(
                    "PATIENT_QR_STEP",
                    new
                    {
                        TraceId = traceId,
                        Step = step,
                        MedicalRecordNumber = medicalRecordNumber,
                        StorageRootPath = storageRootPath,
                        PublicRequestPath = publicRequestPath,
                        RelativeFolder = relativeFolder,
                        AbsoluteFolder = absoluteFolder,
                        ContentRootPath = _environment.ContentRootPath,
                        WebRootPath = _environment.WebRootPath,
                        CurrentDirectory = Directory.GetCurrentDirectory()
                    }
                );

                step = "CreateQrDirectory";
                Directory.CreateDirectory(absoluteFolder);

                step = "EnsureQrDirectoryWritable";
                EnsureDirectoryWritable(absoluteFolder);

                var fileName = "qrcode.png";
                physicalPath = Path.Combine(absoluteFolder, fileName);

                step = "ResolveQrLogoPath";
                logoPath = ResolveQrLogoPath();

                WriteDockerInfoLog(
                    "PATIENT_QR_STEP",
                    new
                    {
                        TraceId = traceId,
                        Step = step,
                        LogoPath = logoPath,
                        LogoExists = !string.IsNullOrWhiteSpace(logoPath) && System.IO.File.Exists(logoPath)
                    }
                );

                step = "BuildQrPayload";
                qrPayload = BuildPatientQrPayload(medicalRecordNumber);

                WriteDockerInfoLog(
                    "PATIENT_QR_STEP",
                    new
                    {
                        TraceId = traceId,
                        Step = step,
                        MedicalRecordNumber = medicalRecordNumber,
                        QrPayload = qrPayload
                    }
                );

                step = "GenerateQrCodePngBytes";
                var qrPngBytes = GenerateQrCodePngBytes(
                    qrPayload,
                    logoPath,
                    traceId
                );

                qrPngByteLength = qrPngBytes.Length;

                step = "WriteQrFile";
                System.IO.File.WriteAllBytes(physicalPath, qrPngBytes);

                step = "VerifyQrFileExists";

                if (!System.IO.File.Exists(physicalPath))
                {
                    throw new IOException($"File QR Code tidak ditemukan setelah ditulis: {physicalPath}");
                }

                var publicPath = CombineUrlPath(
                    storage.PublicRequestPath,
                    relativeFolder.Replace("\\", "/"),
                    fileName
                );

                WriteDockerInfoLog(
                    "PATIENT_QR_STEP",
                    new
                    {
                        TraceId = traceId,
                        Step = "QrCodeSaved",
                        PublicPath = publicPath,
                        PhysicalPath = physicalPath,
                        QrPayload = qrPayload,
                        QrPngByteLength = qrPngByteLength
                    }
                );

                return (publicPath, physicalPath);
            }
            catch (Exception ex)
            {
                WriteDockerErrorLog(
                    "PATIENT_QR_ERROR",
                    ex,
                    new
                    {
                        TraceId = traceId,
                        Step = step,
                        MedicalRecordNumber = medicalRecordNumber,
                        QrPayload = qrPayload,
                        StorageRootPath = storageRootPath,
                        PublicRequestPath = publicRequestPath,
                        QrFolderName = qrFolderName,
                        RelativeFolder = relativeFolder,
                        AbsoluteFolder = absoluteFolder,
                        PhysicalPath = physicalPath,
                        LogoPath = logoPath,
                        LogoExists = !string.IsNullOrWhiteSpace(logoPath) && System.IO.File.Exists(logoPath),
                        QrPngByteLength = qrPngByteLength,
                        ContentRootPath = _environment.ContentRootPath,
                        WebRootPath = _environment.WebRootPath,
                        CurrentDirectory = Directory.GetCurrentDirectory(),
                        OS = RuntimeInformation.OSDescription,
                        Framework = RuntimeInformation.FrameworkDescription
                    }
                );

                throw new InvalidOperationException(
                    $"Gagal membuat file QR pasien pada step {step}. MRN: {medicalRecordNumber}",
                    ex
                );
            }
        }

        private static string BuildPatientQrPayload(string medicalRecordNumber)
        {
            if (string.IsNullOrWhiteSpace(medicalRecordNumber))
            {
                throw new InvalidOperationException("Nomor rekam medis tidak tersedia untuk payload QR.");
            }

            var rawNumber = NormalizeMedicalRecordNumberToRawDigits(medicalRecordNumber);

            if (!string.IsNullOrWhiteSpace(rawNumber))
            {
                return FormatMedicalRecordNumber(rawNumber);
            }

            return medicalRecordNumber.Trim();
        }

        private async Task<string> GenerateMedicalRecordNumberAsync()
        {
            var existingNumbers = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.MedicalRecordNumber != null && x.MedicalRecordNumber != string.Empty)
                .Select(x => x.MedicalRecordNumber)
                .ToListAsync();

            var usedNumbers = existingNumbers
                .Select(NormalizeMedicalRecordNumberToRawDigits)
                .Where(x => x != null)
                .Select(x => int.Parse(x!))
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            if (nextNumber > 99999999)
            {
                throw new InvalidOperationException("Nomor rekam medis sudah mencapai batas maksimum 99-99-99-99.");
            }

            var rawNumber = nextNumber.ToString().PadLeft(8, '0');

            return FormatMedicalRecordNumber(rawNumber);
        }

        private static byte[] GenerateQrCodePngBytes(
    string payload,
    string? logoPath,
    string? traceId = null)
        {
            string step = "Start";

            try
            {
                step = "CreateQrGenerator";

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);
                using var qrCode = new PngByteQRCode(qrCodeData);

                step = "GeneratePlainQrBytes";
                var qrCodeBytes = qrCode.GetGraphic(20);

                step = "CheckLogoPath";

                if (string.IsNullOrWhiteSpace(logoPath) || !System.IO.File.Exists(logoPath))
                {
                    WriteDockerInfoLog(
                        "PATIENT_QR_GENERATE_STEP",
                        new
                        {
                            TraceId = traceId,
                            Step = step,
                            Message = "LogoPath kosong/tidak ditemukan. QR dibuat tanpa logo.",
                            LogoPath = logoPath,
                            Payload = payload,
                            PlainQrByteLength = qrCodeBytes.Length
                        }
                    );

                    return qrCodeBytes;
                }

                WriteDockerInfoLog(
                    "PATIENT_QR_GENERATE_STEP",
                    new
                    {
                        TraceId = traceId,
                        Step = "BeforeOverlayLogo",
                        Payload = payload,
                        LogoPath = logoPath,
                        LogoExists = true,
                        LogoFileLength = new FileInfo(logoPath).Length,
                        PlainQrByteLength = qrCodeBytes.Length,
                        OS = RuntimeInformation.OSDescription,
                        Framework = RuntimeInformation.FrameworkDescription
                    }
                );

                step = "LoadQrImage";
                using var qrImage = ImageSharpImage.Load<Rgba32>(qrCodeBytes);

                step = "LoadLogoImage";
                using var sourceLogoImage = ImageSharpImage.Load<Rgba32>(logoPath);

                step = "PrepareLogoSize";

                var qrWidth = qrImage.Width;
                var qrHeight = qrImage.Height;

                // Logo dibuat cukup besar agar warna terlihat, tapi tetap aman discan.
                var logoMaxWidth = Math.Max(1, qrWidth / 4);
                var logoMaxHeight = Math.Max(1, qrHeight / 6);

                using var logoImage = sourceLogoImage.Clone(ctx =>
                    ctx.Resize(new ResizeOptions
                    {
                        Size = new ImageSharpSize(logoMaxWidth, logoMaxHeight),
                        Mode = ResizeMode.Max
                    })
                );

                step = "PrepareLogoCanvas";

                var logoPaddingX = Math.Max(8, logoImage.Width / 5);
                var logoPaddingY = Math.Max(8, logoImage.Height / 4);

                var canvasWidth = logoImage.Width + (logoPaddingX * 2);
                var canvasHeight = logoImage.Height + (logoPaddingY * 2);

                using var logoCanvas = new SixLabors.ImageSharp.Image<Rgba32>(
                    canvasWidth,
                    canvasHeight,
                    ImageSharpColor.White
                );

                var logoXInCanvas = (canvasWidth - logoImage.Width) / 2;
                var logoYInCanvas = (canvasHeight - logoImage.Height) / 2;

                logoCanvas.Mutate(ctx =>
                {
                    ctx.DrawImage(
                        logoImage,
                        new ImageSharpPoint(logoXInCanvas, logoYInCanvas),
                        1f
                    );
                });

                step = "DrawLogoCanvasToQr";

                var qrLogoX = (qrImage.Width - logoCanvas.Width) / 2;
                var qrLogoY = (qrImage.Height - logoCanvas.Height) / 2;

                qrImage.Mutate(ctx =>
                {
                    ctx.DrawImage(
                        logoCanvas,
                        new ImageSharpPoint(qrLogoX, qrLogoY),
                        1f
                    );
                });

                step = "SaveQrWithLogoToPng";

                using var outputStream = new MemoryStream();

                qrImage.Save(outputStream, new PngEncoder
                {
                    ColorType = PngColorType.RgbWithAlpha
                });

                var resultBytes = outputStream.ToArray();

                WriteDockerInfoLog(
                    "PATIENT_QR_GENERATE_STEP",
                    new
                    {
                        TraceId = traceId,
                        Step = "QrWithLogoGenerated",
                        Payload = payload,
                        LogoPath = logoPath,
                        QrWidth = qrImage.Width,
                        QrHeight = qrImage.Height,
                        LogoWidth = logoImage.Width,
                        LogoHeight = logoImage.Height,
                        LogoCanvasWidth = logoCanvas.Width,
                        LogoCanvasHeight = logoCanvas.Height,
                        ResultByteLength = resultBytes.Length
                    }
                );

                return resultBytes;
            }
            catch (Exception ex)
            {
                WriteDockerErrorLog(
                    "PATIENT_QR_GENERATE_ERROR",
                    ex,
                    new
                    {
                        TraceId = traceId,
                        Step = step,
                        Payload = payload,
                        PayloadLength = payload?.Length ?? 0,
                        LogoPath = logoPath,
                        LogoExists = !string.IsNullOrWhiteSpace(logoPath) && System.IO.File.Exists(logoPath),
                        OS = RuntimeInformation.OSDescription,
                        Framework = RuntimeInformation.FrameworkDescription
                    }
                );

                throw new InvalidOperationException(
                    $"Gagal generate PNG QR Code pada step {step}.",
                    ex
                );
            }
        }

        private static string? NormalizeMedicalRecordNumberToRawDigits(string? medicalRecordNumber)
        {
            if (string.IsNullOrWhiteSpace(medicalRecordNumber))
            {
                return null;
            }

            var digits = new string(medicalRecordNumber.Where(char.IsDigit).ToArray());

            return string.IsNullOrWhiteSpace(digits)
                ? null
                : digits;
        }

        private static string FormatMedicalRecordNumber(string rawNumber)
        {
            if (string.IsNullOrWhiteSpace(rawNumber))
            {
                return rawNumber;
            }

            var digits = new string(rawNumber.Where(char.IsDigit).ToArray());

            if (digits.Length == 8)
            {
                return $"{digits[..2]}-{digits.Substring(2, 2)}-{digits.Substring(4, 2)}-{digits.Substring(6, 2)}";
            }

            if (digits.Length == 6)
            {
                return $"{digits[..2]}-{digits.Substring(2, 2)}-{digits.Substring(4, 2)}";
            }

            return rawNumber;
        }

        private string? ResolveQrLogoPath()
        {
            var configuredLogoPath = NormalizeNullableString(_configuration["PatientQrCode:LogoPath"]);
            var storage = GetFileStoragePaths();

            if (!string.IsNullOrWhiteSpace(configuredLogoPath))
            {
                var resolvedConfiguredLogoPath = ResolveConfiguredQrLogoPath(
                    configuredLogoPath,
                    storage.RootPath,
                    storage.PublicRequestPath
                );

                if (!string.IsNullOrWhiteSpace(resolvedConfiguredLogoPath))
                {
                    return resolvedConfiguredLogoPath;
                }
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var candidates = new[]
            {
                Path.Combine(storage.RootPath, "system-assets", "logo-rsmmc.png"),
                Path.Combine(storage.RootPath, "system-assets", "logo_mmc.png"),              
            };

            return candidates.FirstOrDefault(System.IO.File.Exists);
        }

        private string? ResolveConfiguredQrLogoPath(
    string configuredLogoPath,
    string uploadRootPath,
    string publicRequestPath)
        {
            var normalizedLogoPath = configuredLogoPath
                .Replace("\\", "/")
                .Trim();

            if (Path.IsPathRooted(normalizedLogoPath) &&
                System.IO.File.Exists(normalizedLogoPath))
            {
                return normalizedLogoPath;
            }

            var publicPrefix = publicRequestPath
                .Replace("\\", "/")
                .Trim();

            if (!publicPrefix.StartsWith('/'))
            {
                publicPrefix = "/" + publicPrefix;
            }

            publicPrefix = publicPrefix.TrimEnd('/');

            if (normalizedLogoPath.StartsWith(publicPrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                var relativeFromPublicPath = normalizedLogoPath[(publicPrefix.Length + 1)..];

                var resolvedFromPublicPath = Path.Combine(
                    uploadRootPath,
                    relativeFromPublicPath.Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (System.IO.File.Exists(resolvedFromPublicPath))
                {
                    return resolvedFromPublicPath;
                }
            }

            var resolvedFromUploadRoot = Path.Combine(
                uploadRootPath,
                normalizedLogoPath
                    .TrimStart('/')
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (System.IO.File.Exists(resolvedFromUploadRoot))
            {
                return resolvedFromUploadRoot;
            }

            var resolvedFromContentRoot = Path.Combine(
                _environment.ContentRootPath,
                normalizedLogoPath
                    .TrimStart('/')
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (System.IO.File.Exists(resolvedFromContentRoot))
            {
                return resolvedFromContentRoot;
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var resolvedFromWebRoot = Path.Combine(
                webRootPath,
                normalizedLogoPath
                    .TrimStart('/')
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (System.IO.File.Exists(resolvedFromWebRoot))
            {
                return resolvedFromWebRoot;
            }

            return null;
        }

        private (string RootPath, string PublicRequestPath) GetFileStoragePaths()
        {
            var publicRequestPath = _configuration["FileStorage:PublicRequestPath"] ?? DefaultPublicRequestPath;

            if (!publicRequestPath.StartsWith('/'))
            {
                publicRequestPath = "/" + publicRequestPath;
            }

            publicRequestPath = publicRequestPath.TrimEnd('/');

            var configuredRoot = _configuration["FileStorage:UploadRootPath"];

            if (!string.IsNullOrWhiteSpace(configuredRoot))
            {
                Directory.CreateDirectory(configuredRoot);
                return (configuredRoot, publicRequestPath);
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var rootPath = Path.Combine(webRootPath, publicRequestPath.TrimStart('/'));
            Directory.CreateDirectory(rootPath);

            return (rootPath, publicRequestPath);
        }

        private static string CombineUrlPath(params string[] segments)
        {
            return "/" + string.Join(
                "/",
                segments
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Replace("\\", "/").Trim('/'))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
            );
        }

        private static string SanitizePathSegment(string value)
        {
            var sanitized = new string(value
                .Trim()
                .Select(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '-')
                .ToArray());

            while (sanitized.Contains("--", StringComparison.Ordinal))
            {
                sanitized = sanitized.Replace("--", "-");
            }

            return sanitized.Trim('-');
        }

        private static void DeletePhysicalFileIfExists(string? physicalPath)
        {
            if (string.IsNullOrWhiteSpace(physicalPath))
            {
                return;
            }

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        private static void EnsureDirectoryWritable(string directoryPath)
        {
            var probeFilePath = Path.Combine(
                directoryPath,
                $".write-test-{Guid.NewGuid():N}.tmp"
            );

            System.IO.File.WriteAllText(probeFilePath, "ok");
            System.IO.File.Delete(probeFilePath);
        }

        private static void WriteDockerInfoLog(string marker, object? context = null)
        {
            try
            {
                Console.WriteLine($"===== {marker} =====");

                if (context != null)
                {
                    Console.WriteLine(JsonSerializer.Serialize(context));
                }

                Console.WriteLine($"===== END {marker} =====");
            }
            catch
            {
                // Jangan sampai logging membuat proses utama gagal.
            }
        }

        private static void WriteDockerErrorLog(
            string marker,
            Exception exception,
            object? context = null)
        {
            try
            {
                Console.Error.WriteLine($"===== {marker} =====");

                if (context != null)
                {
                    Console.Error.WriteLine(JsonSerializer.Serialize(context));
                }

                Console.Error.WriteLine(exception.ToString());
                Console.Error.WriteLine($"===== END {marker} =====");
            }
            catch
            {
                // Jangan sampai logging membuat proses utama gagal.
            }
        }

        private IQueryable<MstPatient> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Include(x => x.Country)
                .Include(x => x.Province)
                .Include(x => x.City)
                .Include(x => x.District)
                .Include(x => x.PostalCode)
                .Include(x => x.DefaultMembershipTier)
                .Include(x => x.MotherPatient)
                .Include(x => x.MergedToPatient)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatient> ApplyDateFilter(
            IQueryable<MstPatient> query,
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
                        var thisMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        query = query.Where(x =>
                            x.CreateDateTime >= thisMonthStart &&
                            x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        var lastMonthStart = currentMonthStart.AddMonths(-1);

                        query = query.Where(x =>
                            x.CreateDateTime >= lastMonthStart &&
                            x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstPatient> ApplyRelationFilter(
            IQueryable<MstPatient> query,
            Guid? defaultMembershipTierId)
        {
            var normalizedDefaultMembershipTierId = NormalizeNullableGuid(defaultMembershipTierId);

            if (normalizedDefaultMembershipTierId.HasValue)
            {
                query = query.Where(x => x.DefaultMembershipTierId == normalizedDefaultMembershipTierId.Value);
            }

            return query;
        }

        private static IQueryable<MstPatient> ApplyStandardFilter(
            IQueryable<MstPatient> query,
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

                var matchedPatientTypes = Enum.GetValues<PatientType>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x).ToLower().Contains(keyword))
                    .ToList();

                var matchedPatientStatuses = Enum.GetValues<PatientStatus>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x).ToLower().Contains(keyword))
                    .ToList();

                var matchedRegistrationSources = Enum.GetValues<PatientRegistrationSource>()
                    .Where(x =>
                        x.ToString().ToLower().Contains(keyword) ||
                        BuildEnumLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.PatientCode.ToLower().Contains(keyword) ||
                    x.MedicalRecordNumber.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.NickName != null && x.NickName.ToLower().Contains(keyword)) ||
                    (x.IdentityType != null && x.IdentityType.ToLower().Contains(keyword)) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.Country != null && x.Country.CountryName.ToLower().Contains(keyword)) ||
                    (x.Province != null && x.Province.ProvinceName.ToLower().Contains(keyword)) ||
                    (x.City != null && x.City.CityName.ToLower().Contains(keyword)) ||
                    (x.District != null && x.District.DistrictName.ToLower().Contains(keyword)) ||
                    (x.DefaultMembershipTier != null && x.DefaultMembershipTier.TierName.ToLower().Contains(keyword)) ||
                    matchedPatientTypes.Contains(x.PatientType) ||
                    matchedPatientStatuses.Contains(x.PatientStatus) ||
                    matchedRegistrationSources.Contains(x.RegistrationSource));
            }

            return query;
        }

        private static IOrderedQueryable<MstPatient> ApplySorting(
            IQueryable<MstPatient> query,
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

                "patientcode" => isDescending
                    ? query.OrderByDescending(x => x.PatientCode)
                    : query.OrderBy(x => x.PatientCode),

                "medicalrecordnumber" => isDescending
                    ? query.OrderByDescending(x => x.MedicalRecordNumber)
                    : query.OrderBy(x => x.MedicalRecordNumber),

                "fullname" => isDescending
                    ? query.OrderByDescending(x => x.FullName)
                    : query.OrderBy(x => x.FullName),

                "birthdate" => isDescending
                    ? query.OrderByDescending(x => x.BirthDate).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.BirthDate).ThenBy(x => x.FullName),

                "patienttype" => isDescending
                    ? query.OrderByDescending(x => x.PatientType).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.PatientType).ThenBy(x => x.FullName),

                "patientstatus" => isDescending
                    ? query.OrderByDescending(x => x.PatientStatus).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.PatientStatus).ThenBy(x => x.FullName),

                "registrationsource" => isDescending
                    ? query.OrderByDescending(x => x.RegistrationSource).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.RegistrationSource).ThenBy(x => x.FullName),

                "ismember" => isDescending
                    ? query.OrderByDescending(x => x.IsMember).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.IsMember).ThenBy(x => x.FullName),

                "isnewborn" => isDescending
                    ? query.OrderByDescending(x => x.IsNewborn).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.IsNewborn).ThenBy(x => x.FullName),

                "isdeceased" => isDescending
                    ? query.OrderByDescending(x => x.IsDeceased).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.IsDeceased).ThenBy(x => x.FullName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.FullName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.FullName)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.FullName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreatePatientRequest request,
            Guid? mergedToPatientId,
            string? mergeReason)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return (false, "Nama patient wajib diisi.");
            }

            if (!Enum.IsDefined(typeof(PatientType), request.PatientType))
            {
                return (false, "Tipe patient tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!Enum.IsDefined(typeof(PatientStatus), request.PatientStatus))
            {
                return (false, "Status patient tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!Enum.IsDefined(typeof(PatientRegistrationSource), request.RegistrationSource))
            {
                return (false, "Sumber registrasi patient tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.Gender.HasValue && !Enum.IsDefined(typeof(Gender), request.Gender.Value))
            {
                return (false, "Gender tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!Enum.IsDefined(typeof(Religion), request.Religion))
            {
                return (false, "Agama tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!Enum.IsDefined(typeof(MaritalStatus), request.MaritalStatus))
            {
                return (false, "Status pernikahan tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!Enum.IsDefined(typeof(BloodType), request.BloodType))
            {
                return (false, "Golongan darah tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.IsDeceased && !request.DeceasedDate.HasValue)
            {
                return (false, "Tanggal meninggal wajib diisi jika patient ditandai meninggal.");
            }

            if (!request.IsDeceased && request.DeceasedDate.HasValue)
            {
                return (false, "Tanggal meninggal hanya boleh diisi jika patient ditandai meninggal.");
            }

            var identityNumber = NormalizeNullableString(request.IdentityNumber);

            if (!string.IsNullOrWhiteSpace(identityNumber))
            {
                var normalizedIdentityNumber = identityNumber.ToLower();

                var duplicateIdentityQuery = _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IdentityNumber != null &&
                        x.IdentityNumber.ToLower() == normalizedIdentityNumber);

                if (excludeId.HasValue)
                {
                    duplicateIdentityQuery = duplicateIdentityQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateIdentityQuery.AnyAsync())
                {
                    return (false, "Nomor identitas sudah digunakan oleh patient lain.");
                }
            }

            var regionValidation = await ValidateRegionReferencesAsync(
                request.CountryId,
                request.ProvinceId,
                request.CityId,
                request.DistrictId,
                request.PostalCodeId
            );

            if (!regionValidation.IsValid)
            {
                return regionValidation;
            }

            var membershipValidation = await ValidateMembershipReferencesAsync(
                excludeId,
                request.DefaultMembershipTierId,
                request.ActivePatientMembershipId
            );

            if (!membershipValidation.IsValid)
            {
                return membershipValidation;
            }

            var newbornValidation = await ValidateNewbornReferencesAsync(
                excludeId,
                request.IsNewborn,
                request.MotherPatientId
            );

            if (!newbornValidation.IsValid)
            {
                return newbornValidation;
            }

            var mergedValidation = await ValidateMergedPatientReferenceAsync(
                excludeId,
                mergedToPatientId,
                mergeReason
            );

            if (!mergedValidation.IsValid)
            {
                return mergedValidation;
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRegionReferencesAsync(
            Guid? countryId,
            Guid? provinceId,
            Guid? cityId,
            Guid? districtId,
            Guid? postalCodeId)
        {
            var normalizedCountryId = NormalizeNullableGuid(countryId);
            var normalizedProvinceId = NormalizeNullableGuid(provinceId);
            var normalizedCityId = NormalizeNullableGuid(cityId);
            var normalizedDistrictId = NormalizeNullableGuid(districtId);
            var normalizedPostalCodeId = NormalizeNullableGuid(postalCodeId);

            if (normalizedCountryId.HasValue)
            {
                var exists = await _dbContext.MstCountries
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedCountryId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "Country tidak valid atau tidak aktif.");
                }
            }

            if (normalizedProvinceId.HasValue)
            {
                var exists = await _dbContext.MstProvinces
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedProvinceId.Value &&
                        (!normalizedCountryId.HasValue || x.CountryId == normalizedCountryId.Value) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "Province tidak valid, tidak aktif, atau tidak sesuai country.");
                }
            }

            if (normalizedCityId.HasValue)
            {
                var exists = await _dbContext.MstCities
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedCityId.Value &&
                        (!normalizedProvinceId.HasValue || x.ProvinceId == normalizedProvinceId.Value) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "City tidak valid, tidak aktif, atau tidak sesuai province.");
                }
            }

            if (normalizedDistrictId.HasValue)
            {
                var exists = await _dbContext.MstDistricts
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedDistrictId.Value &&
                        (!normalizedCityId.HasValue || x.CityId == normalizedCityId.Value) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "District tidak valid, tidak aktif, atau tidak sesuai city.");
                }
            }

            if (normalizedPostalCodeId.HasValue)
            {
                var exists = await _dbContext.MstPostalCodes
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedPostalCodeId.Value &&
                        (!normalizedDistrictId.HasValue || x.DistrictId == normalizedDistrictId.Value) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "Postal code tidak valid, tidak aktif, atau tidak sesuai district.");
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateMembershipReferencesAsync(
            Guid? patientId,
            Guid? defaultMembershipTierId,
            Guid? activePatientMembershipId)
        {
            var normalizedDefaultMembershipTierId = NormalizeNullableGuid(defaultMembershipTierId);

            if (normalizedDefaultMembershipTierId.HasValue)
            {
                var exists = await _dbContext.Set<MstMembershipTier>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedDefaultMembershipTierId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "Membership tier tidak valid atau tidak aktif.");
                }
            }

            var normalizedActivePatientMembershipId = NormalizeNullableGuid(activePatientMembershipId);

            if (normalizedActivePatientMembershipId.HasValue)
            {
                if (!patientId.HasValue)
                {
                    return (false, "Active patient membership hanya dapat dipilih setelah patient dibuat.");
                }

                var exists = await _dbContext.Set<MstPatientMembership>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedActivePatientMembershipId.Value &&
                        x.PatientId == patientId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "Active patient membership tidak valid, tidak aktif, atau tidak sesuai patient.");
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateNewbornReferencesAsync(
            Guid? patientId,
            bool isNewborn,
            Guid? motherPatientId)
        {
            var normalizedMotherPatientId = NormalizeNullableGuid(motherPatientId);

            if (isNewborn && !normalizedMotherPatientId.HasValue)
            {
                return (false, "Mother patient wajib dipilih untuk patient newborn.");
            }

            if (!isNewborn && normalizedMotherPatientId.HasValue)
            {
                return (false, "Mother patient hanya boleh diisi jika patient ditandai newborn.");
            }

            if (normalizedMotherPatientId.HasValue)
            {
                if (patientId.HasValue && normalizedMotherPatientId.Value == patientId.Value)
                {
                    return (false, "Mother patient tidak boleh sama dengan patient yang sedang diubah.");
                }

                var exists = await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedMotherPatientId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "Mother patient tidak valid atau tidak aktif.");
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateMergedPatientReferenceAsync(
            Guid? patientId,
            Guid? mergedToPatientId,
            string? mergeReason)
        {
            var normalizedMergedToPatientId = NormalizeNullableGuid(mergedToPatientId);

            if (normalizedMergedToPatientId.HasValue)
            {
                if (!patientId.HasValue)
                {
                    return (false, "Merged patient hanya dapat diatur saat update patient.");
                }

                if (normalizedMergedToPatientId.Value == patientId.Value)
                {
                    return (false, "Merged to patient tidak boleh sama dengan patient yang sedang diubah.");
                }

                if (string.IsNullOrWhiteSpace(mergeReason))
                {
                    return (false, "Alasan merge wajib diisi jika patient digabung ke patient lain.");
                }

                var exists = await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedMergedToPatientId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                {
                    return (false, "Merged to patient tidak valid atau tidak aktif.");
                }
            }

            return (true, null);
        }

        private async Task<string> GeneratePatientCodeAsync()
        {
            return await GenerateRunningCodeAsync(
                selector: x => x.PatientCode,
                prefix: CodePrefix
            );
        }       

        private async Task<string> GenerateRunningCodeAsync(
            System.Linq.Expressions.Expression<Func<MstPatient, string>> selector,
            string prefix)
        {
            var existingCodes = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(selector)
                .Where(x => x.StartsWith(prefix))
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return prefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
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

        private static PatientResponse MapResponse(
            MstPatient entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientResponse
            {
                Id = entity.Id,
                PatientCode = entity.PatientCode,
                MedicalRecordNumber = entity.MedicalRecordNumber,
                PatientType = entity.PatientType,
                PatientTypeName = BuildEnumLabel(entity.PatientType),
                PatientStatus = entity.PatientStatus,
                PatientStatusName = BuildEnumLabel(entity.PatientStatus),
                RegistrationSource = entity.RegistrationSource,
                RegistrationSourceName = BuildEnumLabel(entity.RegistrationSource),
                FullName = entity.FullName,
                NickName = entity.NickName,
                BirthPlace = entity.BirthPlace,
                BirthDate = entity.BirthDate,
                Gender = entity.Gender,
                GenderName = entity.Gender.HasValue ? BuildEnumLabel(entity.Gender.Value) : null,
                Religion = entity.Religion,
                ReligionName = BuildEnumLabel(entity.Religion),
                MaritalStatus = entity.MaritalStatus,
                MaritalStatusName = BuildEnumLabel(entity.MaritalStatus),
                BloodType = entity.BloodType,
                BloodTypeName = BuildEnumLabel(entity.BloodType),
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                PhoneNumber = entity.PhoneNumber,
                WhatsAppNumber = entity.WhatsAppNumber,
                Email = entity.Email,
                CountryId = entity.CountryId,
                CountryName = entity.Country?.CountryName,
                ProvinceId = entity.ProvinceId,
                ProvinceName = entity.Province?.ProvinceName,
                CityId = entity.CityId,
                CityName = entity.City?.CityName,
                DistrictId = entity.DistrictId,
                DistrictName = entity.District?.DistrictName,
                PostalCodeId = entity.PostalCodeId,
                PostalCode = entity.PostalCode?.PostalCode,
                PhotoPath = entity.PhotoPath,
                IsMember = entity.IsMember,
                DefaultMembershipTierId = entity.DefaultMembershipTierId,
                DefaultMembershipTierName = entity.DefaultMembershipTier?.TierName,
                ActivePatientMembershipId = entity.ActivePatientMembershipId,
                IsNewborn = entity.IsNewborn,
                MotherPatientId = entity.MotherPatientId,
                MotherMedicalRecordNumber = entity.MotherPatient?.MedicalRecordNumber,
                MotherPatientName = entity.MotherPatient?.FullName,
                BirthOrder = entity.BirthOrder,
                IsDeceased = entity.IsDeceased,
                DeceasedDate = entity.DeceasedDate,
                MergedToPatientId = entity.MergedToPatientId,
                MergedToMedicalRecordNumber = entity.MergedToPatient?.MedicalRecordNumber,
                MergedToPatientName = entity.MergedToPatient?.FullName,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PatientDetailResponse MapDetailResponse(
            MstPatient entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new PatientDetailResponse
            {
                Id = entity.Id,
                PatientCode = entity.PatientCode,
                MedicalRecordNumber = entity.MedicalRecordNumber,
                PatientType = entity.PatientType,
                PatientTypeName = BuildEnumLabel(entity.PatientType),
                PatientStatus = entity.PatientStatus,
                PatientStatusName = BuildEnumLabel(entity.PatientStatus),
                RegistrationSource = entity.RegistrationSource,
                RegistrationSourceName = BuildEnumLabel(entity.RegistrationSource),
                FullName = entity.FullName,
                NickName = entity.NickName,
                BirthPlace = entity.BirthPlace,
                BirthDate = entity.BirthDate,
                Gender = entity.Gender,
                GenderName = entity.Gender.HasValue ? BuildEnumLabel(entity.Gender.Value) : null,
                Religion = entity.Religion,
                ReligionName = BuildEnumLabel(entity.Religion),
                MaritalStatus = entity.MaritalStatus,
                MaritalStatusName = BuildEnumLabel(entity.MaritalStatus),
                BloodType = entity.BloodType,
                BloodTypeName = BuildEnumLabel(entity.BloodType),
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                PhoneNumber = entity.PhoneNumber,
                WhatsAppNumber = entity.WhatsAppNumber,
                Email = entity.Email,
                Address = entity.Address,
                CountryId = entity.CountryId,
                CountryName = entity.Country?.CountryName,
                ProvinceId = entity.ProvinceId,
                ProvinceName = entity.Province?.ProvinceName,
                CityId = entity.CityId,
                CityName = entity.City?.CityName,
                DistrictId = entity.DistrictId,
                DistrictName = entity.District?.DistrictName,
                PostalCodeId = entity.PostalCodeId,
                PostalCode = entity.PostalCode?.PostalCode,
                PhotoPath = entity.PhotoPath,
                IsMember = entity.IsMember,
                DefaultMembershipTierId = entity.DefaultMembershipTierId,
                DefaultMembershipTierName = entity.DefaultMembershipTier?.TierName,
                ActivePatientMembershipId = entity.ActivePatientMembershipId,
                IsNewborn = entity.IsNewborn,
                MotherPatientId = entity.MotherPatientId,
                MotherMedicalRecordNumber = entity.MotherPatient?.MedicalRecordNumber,
                MotherPatientName = entity.MotherPatient?.FullName,
                BirthOrder = entity.BirthOrder,
                BirthWeightGram = entity.BirthWeightGram,
                BirthLengthCm = entity.BirthLengthCm,
                BirthTime = entity.BirthTime,
                DeliveryMethod = entity.DeliveryMethod,
                IsDeceased = entity.IsDeceased,
                DeceasedDate = entity.DeceasedDate,
                MergedToPatientId = entity.MergedToPatientId,
                MergedToMedicalRecordNumber = entity.MergedToPatient?.MedicalRecordNumber,
                MergedToPatientName = entity.MergedToPatient?.FullName,
                MergeReason = entity.MergeReason,
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

        private static PatientOptionResponse MapOptionResponse(MstPatient entity)
        {
            return new PatientOptionResponse
            {
                Id = entity.Id,
                PatientCode = entity.PatientCode,
                MedicalRecordNumber = entity.MedicalRecordNumber,
                FullName = entity.FullName,
                BirthDate = entity.BirthDate,
                Gender = entity.Gender,
                GenderName = entity.Gender.HasValue ? BuildEnumLabel(entity.Gender.Value) : null,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                PhoneNumber = entity.PhoneNumber,
                PatientType = entity.PatientType,
                PatientTypeName = BuildEnumLabel(entity.PatientType),
                PatientStatus = entity.PatientStatus,
                PatientStatusName = BuildEnumLabel(entity.PatientStatus),
                DefaultMembershipTierId = entity.DefaultMembershipTierId,
                DefaultMembershipTierName = entity.DefaultMembershipTier?.TierName,
                IsNewborn = entity.IsNewborn,
                IsMember = entity.IsMember
            };
        }

        private static List<PatientEnumOptionResponse> BuildEnumOptions<TEnum>()
            where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x)
                })
                .ToList();
        }

        private static string BuildEnumLabel<TEnum>(TEnum value) where TEnum : Enum
        {
            return value switch
            {
                // Patient Type
                PatientType.General => "Umum",
                PatientType.Mother => "Ibu",
                PatientType.Newborn => "Bayi Baru Lahir",
                PatientType.Child => "Anak",
                PatientType.Employee => "Karyawan",
                PatientType.Corporate => "Perusahaan",
                PatientType.Other => "Lainnya",

                // Patient Status
                PatientStatus.Active => "Aktif",
                PatientStatus.Inactive => "Tidak Aktif",
                PatientStatus.Deceased => "Meninggal",
                PatientStatus.Blacklisted => "Daftar Hitam",
                PatientStatus.Merged => "Digabung",

                // Registration Source
                PatientRegistrationSource.Unknown => "Tidak Diketahui",
                PatientRegistrationSource.Kiosk => "Kiosk",
                PatientRegistrationSource.OutpatientAdmission => "Pendaftaran Rawat Jalan",
                PatientRegistrationSource.InpatientAdmission => "Pendaftaran Rawat Inap",
                PatientRegistrationSource.EmergencyAdmission => "Pendaftaran IGD",
                PatientRegistrationSource.Marketing => "Marketing",
                PatientRegistrationSource.Migration => "Migrasi",
                PatientRegistrationSource.Other => "Lainnya",

                // Gender
                Gender.Male => "Laki-laki",
                Gender.Female => "Perempuan",

                // Religion
                Religion.Unknown => "Tidak Diketahui",
                Religion.Islam => "Islam",
                Religion.ProtestantChristian => "Kristen Protestan",
                Religion.CatholicChristian => "Katolik",
                Religion.Hindu => "Hindu",
                Religion.Buddhist => "Buddha",
                Religion.Confucian => "Konghucu",
                Religion.Other => "Lainnya",

                // Marital Status
                MaritalStatus.Unknown => "Tidak Diketahui",
                MaritalStatus.Single => "Belum Menikah",
                MaritalStatus.Married => "Menikah",
                MaritalStatus.Divorced => "Cerai Hidup",
                MaritalStatus.Widowed => "Cerai Mati",
                MaritalStatus.Separated => "Berpisah",

                // Blood Type
                BloodType.Unknown => "Tidak Diketahui",
                BloodType.APositive => "A+",
                BloodType.ANegative => "A-",
                BloodType.BPositive => "B+",
                BloodType.BNegative => "B-",
                BloodType.ABPositive => "AB+",
                BloodType.ABNegative => "AB-",
                BloodType.OPositive => "O+",
                BloodType.ONegative => "O-",
                BloodType.NotDisclosed => "Tidak Diungkapkan",

                _ => SplitPascalCase(value.ToString())
            };
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

        private static void NormalizeActorInfo(PatientResponse data)
        {
            if (data.UpdateDateTime.HasValue &&
                data.UpdateDateTime.Value == DateTime.MinValue)
            {
                data.UpdateDateTime = null;
            }

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

        private static string? NormalizeLowerNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant();
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
