using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.PatientResponse>;

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

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient", Description = "Melihat data patient", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Patient", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientFilterMetadataResponse
            {
                DefaultFilter = new PatientDefaultFilterResponse(),
                SortOptions = new List<PatientSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "patientCode", Label = "Kode pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "fullName", Label = "Nama pasien" },
                    new() { Value = "birthDate", Label = "Tanggal lahir" },
                    new() { Value = "patientType", Label = "Tipe pasien" },
                    new() { Value = "patientStatus", Label = "Status pasien" },
                    new() { Value = "registrationSource", Label = "Sumber registrasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                PatientTypeOptions = BuildEnumOptions<PatientType>(),
                PatientStatusOptions = BuildEnumOptions<PatientStatus>(),
                RegistrationSourceOptions = BuildEnumOptions<PatientRegistrationSource>(),
                GenderOptions = BuildEnumOptions<Gender>(),
                ReligionOptions = BuildEnumOptions<Religion>(),
                MaritalStatusOptions = BuildEnumOptions<MaritalStatus>(),
                BloodTypeOptions = BuildEnumOptions<BloodType>()
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
        [AccessAction("Read", "Read Patient", Description = "Melihat data patient", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Patient", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new PatientSummaryResponse
            {
                TotalPatient = await query.CountAsync(),
                ActivePatient = await query.CountAsync(x => x.IsActive),
                InactivePatient = await query.CountAsync(x => !x.IsActive),
                GeneralPatient = await query.CountAsync(x => x.PatientType == PatientType.General),
                NewbornPatient = await query.CountAsync(x => x.IsNewborn),
                MemberPatient = await query.CountAsync(x => x.IsMember),
                DeceasedPatient = await query.CountAsync(x => x.IsDeceased),
                MergedPatient = await query.CountAsync(x => x.MergedToPatientId.HasValue)
            };

            return Ok(ApiResponse<PatientSummaryResponse>.Ok(
                result,
                "Ringkasan patient berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient", Description = "Melihat data patient", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Patient", "Read")]
        public async Task<IActionResult> GetPatients(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] PatientType? patientType,
            [FromQuery] PatientStatus? patientStatus,
            [FromQuery] PatientRegistrationSource? registrationSource,
            [FromQuery] Gender? gender,
            [FromQuery] bool? isMember,
            [FromQuery] bool? isNewborn,
            [FromQuery] bool? isDeceased,
            [FromQuery] bool? isMerged,
            [FromQuery] Guid? countryId,
            [FromQuery] Guid? provinceId,
            [FromQuery] Guid? cityId,
            [FromQuery] Guid? districtId,
            [FromQuery] Guid? defaultMembershipTierId,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyFilters(
                query,
                search,
                isActive,
                patientType,
                patientStatus,
                registrationSource,
                gender,
                isMember,
                isNewborn,
                isDeceased,
                isMerged,
                countryId,
                provinceId,
                cityId,
                districtId,
                defaultMembershipTierId
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PatientResponse
                {
                    Id = x.Id,
                    PatientCode = x.PatientCode,
                    MedicalRecordNumber = x.MedicalRecordNumber,
                    PatientType = x.PatientType,
                    PatientStatus = x.PatientStatus,
                    RegistrationSource = x.RegistrationSource,
                    FullName = x.FullName,
                    NickName = x.NickName,
                    BirthPlace = x.BirthPlace,
                    BirthDate = x.BirthDate,
                    Gender = x.Gender,
                    Religion = x.Religion,
                    MaritalStatus = x.MaritalStatus,
                    BloodType = x.BloodType,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    CountryId = x.CountryId,
                    CountryName = x.Country != null ? x.Country.CountryName : null,
                    ProvinceId = x.ProvinceId,
                    ProvinceName = x.Province != null ? x.Province.ProvinceName : null,
                    CityId = x.CityId,
                    CityName = x.City != null ? x.City.CityName : null,
                    DistrictId = x.DistrictId,
                    DistrictName = x.District != null ? x.District.DistrictName : null,
                    PostalCodeId = x.PostalCodeId,
                    PostalCode = x.PostalCode != null ? x.PostalCode.PostalCode : null,
                    PhotoPath = x.PhotoPath,
                    IsMember = x.IsMember,
                    DefaultMembershipTierId = x.DefaultMembershipTierId,
                    DefaultMembershipTierName = x.DefaultMembershipTier != null ? x.DefaultMembershipTier.TierName : null,
                    ActivePatientMembershipId = x.ActivePatientMembershipId,
                    IsNewborn = x.IsNewborn,
                    MotherPatientId = x.MotherPatientId,
                    MotherMedicalRecordNumber = x.MotherPatient != null ? x.MotherPatient.MedicalRecordNumber : null,
                    MotherPatientName = x.MotherPatient != null ? x.MotherPatient.FullName : null,
                    BirthOrder = x.BirthOrder,
                    IsDeceased = x.IsDeceased,
                    DeceasedDate = x.DeceasedDate,
                    MergedToPatientId = x.MergedToPatientId,
                    MergedToMedicalRecordNumber = x.MergedToPatient != null ? x.MergedToPatient.MedicalRecordNumber : null,
                    MergedToPatientName = x.MergedToPatient != null ? x.MergedToPatient.FullName : null,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
        [ProducesResponseType(typeof(ApiResponse<List<PatientOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient", Description = "Melihat data patient", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Patient", "Read")]
        public async Task<IActionResult> GetPatientOptions(
            [FromQuery] PatientType? patientType,
            [FromQuery] PatientStatus? patientStatus,
            [FromQuery] Gender? gender,
            [FromQuery] bool? isMember,
            [FromQuery] bool? isNewborn,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (patientType.HasValue)
                query = query.Where(x => x.PatientType == patientType.Value);

            if (patientStatus.HasValue)
                query = query.Where(x => x.PatientStatus == patientStatus.Value);

            if (gender.HasValue)
                query = query.Where(x => x.Gender == gender.Value);

            if (isMember.HasValue)
                query = query.Where(x => x.IsMember == isMember.Value);

            if (isNewborn.HasValue)
                query = query.Where(x => x.IsNewborn == isNewborn.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PatientCode.ToLower().Contains(keyword) ||
                    x.MedicalRecordNumber.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.MedicalRecordNumber)
                .Take(100)
                .Select(x => new PatientOptionResponse
                {
                    Id = x.Id,
                    PatientCode = x.PatientCode,
                    MedicalRecordNumber = x.MedicalRecordNumber,
                    FullName = x.FullName,
                    BirthDate = x.BirthDate,
                    Gender = x.Gender,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    PhoneNumber = x.PhoneNumber,
                    PatientType = x.PatientType,
                    PatientStatus = x.PatientStatus,
                    IsNewborn = x.IsNewborn,
                    IsMember = x.IsMember
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientOptionResponse>>.Ok(
                data,
                "Data pilihan patient berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient", Description = "Melihat data patient", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Patient", "Read")]
        public async Task<IActionResult> GetPatientById(Guid id)
        {
            var data = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new PatientDetailResponse
                {
                    Id = x.Id,
                    PatientCode = x.PatientCode,
                    MedicalRecordNumber = x.MedicalRecordNumber,
                    PatientType = x.PatientType,
                    PatientStatus = x.PatientStatus,
                    RegistrationSource = x.RegistrationSource,
                    FullName = x.FullName,
                    NickName = x.NickName,
                    BirthPlace = x.BirthPlace,
                    BirthDate = x.BirthDate,
                    Gender = x.Gender,
                    Religion = x.Religion,
                    MaritalStatus = x.MaritalStatus,
                    BloodType = x.BloodType,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    Address = x.Address,
                    CountryId = x.CountryId,
                    CountryName = x.Country != null ? x.Country.CountryName : null,
                    ProvinceId = x.ProvinceId,
                    ProvinceName = x.Province != null ? x.Province.ProvinceName : null,
                    CityId = x.CityId,
                    CityName = x.City != null ? x.City.CityName : null,
                    DistrictId = x.DistrictId,
                    DistrictName = x.District != null ? x.District.DistrictName : null,
                    PostalCodeId = x.PostalCodeId,
                    PostalCode = x.PostalCode != null ? x.PostalCode.PostalCode : null,
                    PhotoPath = x.PhotoPath,
                    IsMember = x.IsMember,
                    DefaultMembershipTierId = x.DefaultMembershipTierId,
                    DefaultMembershipTierName = x.DefaultMembershipTier != null ? x.DefaultMembershipTier.TierName : null,
                    ActivePatientMembershipId = x.ActivePatientMembershipId,
                    IsNewborn = x.IsNewborn,
                    MotherPatientId = x.MotherPatientId,
                    MotherMedicalRecordNumber = x.MotherPatient != null ? x.MotherPatient.MedicalRecordNumber : null,
                    MotherPatientName = x.MotherPatient != null ? x.MotherPatient.FullName : null,
                    BirthOrder = x.BirthOrder,
                    BirthWeightGram = x.BirthWeightGram,
                    BirthLengthCm = x.BirthLengthCm,
                    BirthTime = x.BirthTime,
                    DeliveryMethod = x.DeliveryMethod,
                    IsDeceased = x.IsDeceased,
                    DeceasedDate = x.DeceasedDate,
                    MergedToPatientId = x.MergedToPatientId,
                    MergedToMedicalRecordNumber = x.MergedToPatient != null ? x.MergedToPatient.MedicalRecordNumber : null,
                    MergedToPatientName = x.MergedToPatient != null ? x.MergedToPatient.FullName : null,
                    MergeReason = x.MergeReason,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientDetailResponse>.Ok(
                data,
                "Detail patient berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Patient", Description = "Membuat data patient", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Patient", "Create")]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request.PatientCode,
                request.MedicalRecordNumber,
                request.FullName,
                request.IdentityNumber,
                request.CountryId,
                request.ProvinceId,
                request.CityId,
                request.DistrictId,
                request.PostalCodeId,
                request.DefaultMembershipTierId,
                request.ActivePatientMembershipId,
                request.IsNewborn,
                request.MotherPatientId,
                mergedToPatientId: null,
                request.IsDeceased,
                request.DeceasedDate
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

            var entity = new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = request.PatientCode.Trim().ToUpperInvariant(),
                MedicalRecordNumber = request.MedicalRecordNumber.Trim().ToUpperInvariant(),
                PatientType = request.PatientType,
                PatientStatus = request.PatientStatus,
                RegistrationSource = request.RegistrationSource,
                FullName = request.FullName.Trim(),
                NickName = NormalizeNullableText(request.NickName),
                BirthPlace = NormalizeNullableText(request.BirthPlace),
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                Religion = request.Religion,
                MaritalStatus = request.MaritalStatus,
                BloodType = request.BloodType,
                IdentityType = NormalizeNullableText(request.IdentityType),
                IdentityNumber = NormalizeNullableText(request.IdentityNumber),
                PhoneNumber = NormalizeNullableText(request.PhoneNumber),
                WhatsAppNumber = NormalizeNullableText(request.WhatsAppNumber),
                Email = NormalizeNullableText(request.Email),
                Address = NormalizeNullableText(request.Address),
                CountryId = NormalizeNullableGuid(request.CountryId),
                ProvinceId = NormalizeNullableGuid(request.ProvinceId),
                CityId = NormalizeNullableGuid(request.CityId),
                DistrictId = NormalizeNullableGuid(request.DistrictId),
                PostalCodeId = NormalizeNullableGuid(request.PostalCodeId),
                PhotoPath = NormalizeNullableText(request.PhotoPath),
                IsMember = request.IsMember,
                DefaultMembershipTierId = NormalizeNullableGuid(request.DefaultMembershipTierId),
                ActivePatientMembershipId = NormalizeNullableGuid(request.ActivePatientMembershipId),
                IsNewborn = request.IsNewborn,
                MotherPatientId = NormalizeNullableGuid(request.MotherPatientId),
                BirthOrder = request.BirthOrder,
                BirthWeightGram = request.BirthWeightGram,
                BirthLengthCm = request.BirthLengthCm,
                BirthTime = request.BirthTime,
                DeliveryMethod = NormalizeNullableText(request.DeliveryMethod),
                IsDeceased = request.IsDeceased,
                DeceasedDate = request.DeceasedDate,
                Notes = NormalizeNullableText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPatient>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new PatientCreateResponse
            {
                Id = entity.Id,
                PatientCode = entity.PatientCode,
                MedicalRecordNumber = entity.MedicalRecordNumber,
                FullName = entity.FullName,
                PatientType = entity.PatientType,
                PatientStatus = entity.PatientStatus,
                IsNewborn = entity.IsNewborn,
                IsMember = entity.IsMember,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<PatientCreateResponse>.Ok(
                response,
                "Patient berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient", Description = "Mengubah data patient", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Patient", "Update")]
        public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] UpdatePatientRequest request)
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
                request.PatientCode,
                request.MedicalRecordNumber,
                request.FullName,
                request.IdentityNumber,
                request.CountryId,
                request.ProvinceId,
                request.CityId,
                request.DistrictId,
                request.PostalCodeId,
                request.DefaultMembershipTierId,
                request.ActivePatientMembershipId,
                request.IsNewborn,
                request.MotherPatientId,
                request.MergedToPatientId,
                request.IsDeceased,
                request.DeceasedDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient tidak valid."
                ));
            }

            entity.PatientCode = request.PatientCode.Trim().ToUpperInvariant();
            entity.MedicalRecordNumber = request.MedicalRecordNumber.Trim().ToUpperInvariant();
            entity.PatientType = request.PatientType;
            entity.PatientStatus = request.PatientStatus;
            entity.RegistrationSource = request.RegistrationSource;
            entity.FullName = request.FullName.Trim();
            entity.NickName = NormalizeNullableText(request.NickName);
            entity.BirthPlace = NormalizeNullableText(request.BirthPlace);
            entity.BirthDate = request.BirthDate;
            entity.Gender = request.Gender;
            entity.Religion = request.Religion;
            entity.MaritalStatus = request.MaritalStatus;
            entity.BloodType = request.BloodType;
            entity.IdentityType = NormalizeNullableText(request.IdentityType);
            entity.IdentityNumber = NormalizeNullableText(request.IdentityNumber);
            entity.PhoneNumber = NormalizeNullableText(request.PhoneNumber);
            entity.WhatsAppNumber = NormalizeNullableText(request.WhatsAppNumber);
            entity.Email = NormalizeNullableText(request.Email);
            entity.Address = NormalizeNullableText(request.Address);
            entity.CountryId = NormalizeNullableGuid(request.CountryId);
            entity.ProvinceId = NormalizeNullableGuid(request.ProvinceId);
            entity.CityId = NormalizeNullableGuid(request.CityId);
            entity.DistrictId = NormalizeNullableGuid(request.DistrictId);
            entity.PostalCodeId = NormalizeNullableGuid(request.PostalCodeId);
            entity.PhotoPath = NormalizeNullableText(request.PhotoPath);
            entity.IsMember = request.IsMember;
            entity.DefaultMembershipTierId = NormalizeNullableGuid(request.DefaultMembershipTierId);
            entity.ActivePatientMembershipId = NormalizeNullableGuid(request.ActivePatientMembershipId);
            entity.IsNewborn = request.IsNewborn;
            entity.MotherPatientId = NormalizeNullableGuid(request.MotherPatientId);
            entity.BirthOrder = request.BirthOrder;
            entity.BirthWeightGram = request.BirthWeightGram;
            entity.BirthLengthCm = request.BirthLengthCm;
            entity.BirthTime = request.BirthTime;
            entity.DeliveryMethod = NormalizeNullableText(request.DeliveryMethod);
            entity.IsDeceased = request.IsDeceased;
            entity.DeceasedDate = request.DeceasedDate;
            entity.MergedToPatientId = NormalizeNullableGuid(request.MergedToPatientId);
            entity.MergeReason = NormalizeNullableText(request.MergeReason);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient", Description = "Menghapus data patient", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Patient", "Delete")]
        public async Task<IActionResult> DeletePatient(Guid id)
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
                .AnyAsync(x => (x.PatientId == id || x.RelatedPatientId == id) && !x.IsDelete);

            var isUsedByEmergencyContact = await _dbContext.Set<MstPatientEmergencyContact>()
                .AnyAsync(x => x.PatientId == id && !x.IsDelete);

            var isUsedByMembership = await _dbContext.Set<MstPatientMembership>()
                .AnyAsync(x => x.PatientId == id && !x.IsDelete);

            var isUsedAsReference = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => !x.IsDelete && (x.MotherPatientId == id || x.MergedToPatientId == id));

            if (isUsedByIdentityDocument || isUsedByRelationship || isUsedByEmergencyContact || isUsedByMembership || isUsedAsReference)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient tidak dapat dihapus karena sudah digunakan oleh dokumen identitas, relasi, kontak darurat, membership, atau referensi pasien lain."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient berhasil dihapus."
            ));
        }

        private static IQueryable<MstPatient> ApplyFilters(
            IQueryable<MstPatient> query,
            string? search,
            bool? isActive,
            PatientType? patientType,
            PatientStatus? patientStatus,
            PatientRegistrationSource? registrationSource,
            Gender? gender,
            bool? isMember,
            bool? isNewborn,
            bool? isDeceased,
            bool? isMerged,
            Guid? countryId,
            Guid? provinceId,
            Guid? cityId,
            Guid? districtId,
            Guid? defaultMembershipTierId)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PatientCode.ToLower().Contains(keyword) ||
                    x.MedicalRecordNumber.ToLower().Contains(keyword) ||
                    x.FullName.ToLower().Contains(keyword) ||
                    (x.NickName != null && x.NickName.ToLower().Contains(keyword)) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (patientType.HasValue)
                query = query.Where(x => x.PatientType == patientType.Value);

            if (patientStatus.HasValue)
                query = query.Where(x => x.PatientStatus == patientStatus.Value);

            if (registrationSource.HasValue)
                query = query.Where(x => x.RegistrationSource == registrationSource.Value);

            if (gender.HasValue)
                query = query.Where(x => x.Gender == gender.Value);

            if (isMember.HasValue)
                query = query.Where(x => x.IsMember == isMember.Value);

            if (isNewborn.HasValue)
                query = query.Where(x => x.IsNewborn == isNewborn.Value);

            if (isDeceased.HasValue)
                query = query.Where(x => x.IsDeceased == isDeceased.Value);

            if (isMerged.HasValue)
                query = isMerged.Value
                    ? query.Where(x => x.MergedToPatientId.HasValue)
                    : query.Where(x => !x.MergedToPatientId.HasValue);

            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.CountryId == countryId.Value);

            if (provinceId.HasValue && provinceId.Value != Guid.Empty)
                query = query.Where(x => x.ProvinceId == provinceId.Value);

            if (cityId.HasValue && cityId.Value != Guid.Empty)
                query = query.Where(x => x.CityId == cityId.Value);

            if (districtId.HasValue && districtId.Value != Guid.Empty)
                query = query.Where(x => x.DistrictId == districtId.Value);

            if (defaultMembershipTierId.HasValue && defaultMembershipTierId.Value != Guid.Empty)
                query = query.Where(x => x.DefaultMembershipTierId == defaultMembershipTierId.Value);

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string patientCode,
            string medicalRecordNumber,
            string fullName,
            string? identityNumber,
            Guid? countryId,
            Guid? provinceId,
            Guid? cityId,
            Guid? districtId,
            Guid? postalCodeId,
            Guid? defaultMembershipTierId,
            Guid? activePatientMembershipId,
            bool isNewborn,
            Guid? motherPatientId,
            Guid? mergedToPatientId,
            bool isDeceased,
            DateTime? deceasedDate)
        {
            if (string.IsNullOrWhiteSpace(patientCode))
                return (false, "Kode patient wajib diisi.");

            if (string.IsNullOrWhiteSpace(medicalRecordNumber))
                return (false, "Nomor rekam medis wajib diisi.");

            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Nama patient wajib diisi.");

            if (isDeceased && !deceasedDate.HasValue)
                return (false, "Tanggal meninggal wajib diisi jika patient ditandai meninggal.");

            var normalizedCode = patientCode.Trim().ToUpperInvariant();
            var normalizedMrn = medicalRecordNumber.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstPatient>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.PatientCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode patient sudah digunakan.");

            var duplicateMedicalRecordNumber = await _dbContext.Set<MstPatient>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.MedicalRecordNumber.ToUpper() == normalizedMrn &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateMedicalRecordNumber)
                return (false, "Nomor rekam medis sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(identityNumber))
            {
                var normalizedIdentityNumber = identityNumber.Trim().ToLower();

                var duplicateIdentityNumber = await _dbContext.Set<MstPatient>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.IdentityNumber != null &&
                        x.IdentityNumber.ToLower() == normalizedIdentityNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateIdentityNumber)
                    return (false, "Nomor identitas sudah digunakan oleh patient lain.");
            }

            var regionValidation = await ValidateRegionReferencesAsync(countryId, provinceId, cityId, districtId, postalCodeId);

            if (!regionValidation.IsValid)
                return regionValidation;

            var normalizedDefaultMembershipTierId = NormalizeNullableGuid(defaultMembershipTierId);

            if (normalizedDefaultMembershipTierId.HasValue)
            {
                var membershipTierExists = await _dbContext.Set<MstMembershipTier>()
                    .AnyAsync(x => x.Id == normalizedDefaultMembershipTierId.Value && x.IsActive && !x.IsDelete);

                if (!membershipTierExists)
                    return (false, "Membership tier tidak valid atau tidak aktif.");
            }

            var normalizedActivePatientMembershipId = NormalizeNullableGuid(activePatientMembershipId);

            if (normalizedActivePatientMembershipId.HasValue)
            {
                var activeMembershipExists = await _dbContext.Set<MstPatientMembership>()
                    .AnyAsync(x =>
                        x.Id == normalizedActivePatientMembershipId.Value &&
                        (!excludeId.HasValue || x.PatientId == excludeId.Value) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!activeMembershipExists)
                    return (false, "Active patient membership tidak valid, tidak aktif, atau tidak sesuai patient.");
            }

            var normalizedMotherPatientId = NormalizeNullableGuid(motherPatientId);

            if (isNewborn && !normalizedMotherPatientId.HasValue)
                return (false, "Mother patient wajib dipilih untuk patient newborn.");

            if (normalizedMotherPatientId.HasValue)
            {
                if (excludeId.HasValue && normalizedMotherPatientId.Value == excludeId.Value)
                    return (false, "Mother patient tidak boleh sama dengan patient yang sedang diubah.");

                var motherExists = await _dbContext.Set<MstPatient>()
                    .AnyAsync(x => x.Id == normalizedMotherPatientId.Value && x.IsActive && !x.IsDelete);

                if (!motherExists)
                    return (false, "Mother patient tidak valid atau tidak aktif.");
            }

            var normalizedMergedToPatientId = NormalizeNullableGuid(mergedToPatientId);

            if (normalizedMergedToPatientId.HasValue)
            {
                if (!excludeId.HasValue)
                    return (false, "Merged patient hanya dapat diatur saat update patient.");

                if (normalizedMergedToPatientId.Value == excludeId.Value)
                    return (false, "Merged to patient tidak boleh sama dengan patient yang sedang diubah.");

                var mergedToPatientExists = await _dbContext.Set<MstPatient>()
                    .AnyAsync(x => x.Id == normalizedMergedToPatientId.Value && x.IsActive && !x.IsDelete);

                if (!mergedToPatientExists)
                    return (false, "Merged to patient tidak valid atau tidak aktif.");
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
                    .AnyAsync(x => x.Id == normalizedCountryId.Value && x.IsActive && !x.IsDelete);

                if (!exists)
                    return (false, "Country tidak valid atau tidak aktif.");
            }

            if (normalizedProvinceId.HasValue)
            {
                var exists = await _dbContext.MstProvinces
                    .AnyAsync(x =>
                        x.Id == normalizedProvinceId.Value &&
                        (!normalizedCountryId.HasValue || x.CountryId == normalizedCountryId.Value) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                    return (false, "Province tidak valid, tidak aktif, atau tidak sesuai country.");
            }

            if (normalizedCityId.HasValue)
            {
                var exists = await _dbContext.MstCities
                    .AnyAsync(x =>
                        x.Id == normalizedCityId.Value &&
                        (!normalizedProvinceId.HasValue || x.ProvinceId == normalizedProvinceId.Value) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                    return (false, "City tidak valid, tidak aktif, atau tidak sesuai province.");
            }

            if (normalizedDistrictId.HasValue)
            {
                var exists = await _dbContext.MstDistricts
                    .AnyAsync(x =>
                        x.Id == normalizedDistrictId.Value &&
                        (!normalizedCityId.HasValue || x.CityId == normalizedCityId.Value) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                    return (false, "District tidak valid, tidak aktif, atau tidak sesuai city.");
            }

            if (normalizedPostalCodeId.HasValue)
            {
                var exists = await _dbContext.MstPostalCodes
                    .AnyAsync(x =>
                        x.Id == normalizedPostalCodeId.Value &&
                        (!normalizedDistrictId.HasValue || x.DistrictId == normalizedDistrictId.Value) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!exists)
                    return (false, "Postal code tidak valid, tidak aktif, atau tidak sesuai district.");
            }

            return (true, null);
        }

        private static IQueryable<MstPatient> ApplySorting(
            IQueryable<MstPatient> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").ToLowerInvariant() switch
            {
                "patientcode" => isDesc ? query.OrderByDescending(x => x.PatientCode) : query.OrderBy(x => x.PatientCode),
                "medicalrecordnumber" => isDesc ? query.OrderByDescending(x => x.MedicalRecordNumber) : query.OrderBy(x => x.MedicalRecordNumber),
                "fullname" => isDesc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
                "birthdate" => isDesc ? query.OrderByDescending(x => x.BirthDate) : query.OrderBy(x => x.BirthDate),
                "patienttype" => isDesc ? query.OrderByDescending(x => x.PatientType) : query.OrderBy(x => x.PatientType),
                "patientstatus" => isDesc ? query.OrderByDescending(x => x.PatientStatus) : query.OrderBy(x => x.PatientStatus),
                "registrationsource" => isDesc ? query.OrderByDescending(x => x.RegistrationSource) : query.OrderBy(x => x.RegistrationSource),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.FullName)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.FullName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientEnumOptionResponse
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }
    }
}