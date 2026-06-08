using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientInsurancePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.PatientInsuranceResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/patient-insurances")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Patient Insurance",
        AreaName = "HealthServices",
        ControllerName = "PatientInsurance",
        Description = "Health service patient management master data patient insurance",
        SortOrder = 14
    )]
    [Tags("Health Services / Patient Management / Master Data / Patient Insurance")]
    public class PatientInsuranceController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";

        private static readonly HashSet<string> AllowedHolderRelationships = new(StringComparer.OrdinalIgnoreCase)
        {
            "Self", "Spouse", "Child", "Parent", "Sibling", "Guardian", "Employee", "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientInsuranceController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Insurance", Description = "Melihat data patient insurance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientInsuranceFilterMetadataResponse
            {
                DefaultFilter = new PatientInsuranceDefaultFilterResponse(),
                SortOptions = new List<PatientInsuranceSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "No. rekam medis" },
                    new() { Value = "insuranceProviderName", Label = "Nama asuransi" },
                    new() { Value = "policyNumber", Label = "Nomor polis" },
                    new() { Value = "planName", Label = "Nama plan" },
                    new() { Value = "className", Label = "Kelas" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "lastEligibilityCheckAt", Label = "Tanggal cek eligibility" },
                    new() { Value = "isPrimary", Label = "Status utama" },
                    new() { Value = "isEligible", Label = "Status eligible" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                HolderRelationships = AllowedHolderRelationships.OrderBy(x => x).ToList()
            };

            await _loggerService.InfoAsync(LogCategory, "PatientInsurance.GetFilterMetadata", "Mengambil metadata filter patient insurance.", result);

            return Ok(ApiResponse<PatientInsuranceFilterMetadataResponse>.Ok(result, "Metadata filter patient insurance berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Insurance", Description = "Melihat data patient insurance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;
            var recentEligibilityDate = DateTime.UtcNow.AddDays(-30);

            var query = _dbContext.Set<MstPatientInsurance>().AsNoTracking().Where(x => !x.IsDelete);

            var result = new PatientInsuranceSummaryResponse
            {
                TotalPatientInsurance = await query.CountAsync(),
                ActivePatientInsurance = await query.CountAsync(x => x.IsActive),
                InactivePatientInsurance = await query.CountAsync(x => !x.IsActive),
                PrimaryPatientInsurance = await query.CountAsync(x => x.IsPrimary),
                EligiblePatientInsurance = await query.CountAsync(x => x.IsEligible),
                IneligiblePatientInsurance = await query.CountAsync(x => !x.IsEligible),
                NeedGuaranteeLetterPatientInsurance = await query.CountAsync(x => x.IsNeedGuaranteeLetter),
                NeedReferralLetterPatientInsurance = await query.CountAsync(x => x.IsNeedReferralLetter),
                AllowExcessPaymentByPatientInsurance = await query.CountAsync(x => x.IsAllowExcessPaymentByPatient),
                EffectivePatientInsurance = await query.CountAsync(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)),
                ExpiredPatientInsurance = await query.CountAsync(x => x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today),
                WithAnnualLimitPatientInsurance = await query.CountAsync(x => x.AnnualLimitAmount.HasValue),
                WithRemainingLimitPatientInsurance = await query.CountAsync(x => x.RemainingLimitAmount.HasValue),
                WithCoPaymentPatientInsurance = await query.CountAsync(x => x.CoPaymentPercent.HasValue || x.CoPaymentAmount.HasValue),
                RecentlyEligibilityCheckedPatientInsurance = await query.CountAsync(x => x.LastEligibilityCheckAt.HasValue && x.LastEligibilityCheckAt.Value >= recentEligibilityDate)
            };

            return Ok(ApiResponse<PatientInsuranceSummaryResponse>.Ok(result, "Ringkasan patient insurance berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientInsurancePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Insurance", Description = "Melihat data patient insurance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetPatientInsurances(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] string? planName,
            [FromQuery] string? className,
            [FromQuery] string? benefitPlanCode,
            [FromQuery] string? holderRelationship,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isEligible,
            [FromQuery] bool? isNeedGuaranteeLetter,
            [FromQuery] bool? isNeedReferralLetter,
            [FromQuery] bool? isAllowExcessPaymentByPatient,
            [FromQuery] DateTime? effectiveDate,
            [FromQuery] DateTime? lastEligibilityCheckFrom,
            [FromQuery] DateTime? lastEligibilityCheckTo,
            [FromQuery] decimal? minimumAnnualLimitAmount,
            [FromQuery] decimal? maximumAnnualLimitAmount,
            [FromQuery] decimal? minimumRemainingLimitAmount,
            [FromQuery] decimal? maximumRemainingLimitAmount,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilters(query, search, isActive, patientId, insuranceProviderId, planName, className, benefitPlanCode,
                holderRelationship, isPrimary, isEligible, isNeedGuaranteeLetter, isNeedReferralLetter,
                isAllowExcessPaymentByPatient, effectiveDate, lastEligibilityCheckFrom, lastEligibilityCheckTo,
                minimumAnnualLimitAmount, maximumAnnualLimitAmount, minimumRemainingLimitAmount, maximumRemainingLimitAmount);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToResponse).ToList();

            var result = new ResponsePatientInsurancePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientInsurancePagedResult>.Ok(result, "Data patient insurance berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientInsuranceOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Insurance", Description = "Melihat data patient insurance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetPatientInsuranceOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isEligible,
            [FromQuery] DateTime? effectiveDate,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);
            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);
            if (insuranceProviderId.HasValue && insuranceProviderId.Value != Guid.Empty)
                query = query.Where(x => x.InsuranceProviderId == insuranceProviderId.Value);
            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isEligible.HasValue)
                query = query.Where(x => x.IsEligible == isEligible.Value);
            if (effectiveDate.HasValue)
            {
                var selectedDate = effectiveDate.Value.Date;
                query = query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= selectedDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= selectedDate));
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.PolicyNumber.ToLower().Contains(keyword) ||
                    (x.CardNumber != null && x.CardNumber.ToLower().Contains(keyword)) ||
                    (x.MemberNumber != null && x.MemberNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                .ThenBy(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty)
                .Select(x => new PatientInsuranceOptionResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                    PolicyNumber = x.PolicyNumber,
                    CardNumber = x.CardNumber,
                    MemberNumber = x.MemberNumber,
                    PlanName = x.PlanName,
                    ClassName = x.ClassName,
                    BenefitPlanCode = x.BenefitPlanCode,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsPrimary = x.IsPrimary,
                    IsEligible = x.IsEligible,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedReferralLetter = x.IsNeedReferralLetter,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    AnnualLimitAmount = x.AnnualLimitAmount,
                    RemainingLimitAmount = x.RemainingLimitAmount,
                    CoPaymentPercent = x.CoPaymentPercent,
                    CoPaymentAmount = x.CoPaymentAmount
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientInsuranceOptionResponse>>.Ok(data, "Data pilihan patient insurance berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Insurance", Description = "Melihat data patient insurance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetPatientInsuranceById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new PatientInsuranceDetailResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderCode = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderCode : string.Empty,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                    InsuranceGroupName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceGroupName : null,
                    ProviderType = x.InsuranceProvider != null ? x.InsuranceProvider.ProviderType : null,
                    ClaimMethod = x.InsuranceProvider != null ? x.InsuranceProvider.ClaimMethod : null,
                    PolicyNumber = x.PolicyNumber,
                    CardNumber = x.CardNumber,
                    MemberNumber = x.MemberNumber,
                    PlanName = x.PlanName,
                    ClassName = x.ClassName,
                    BenefitPlanCode = x.BenefitPlanCode,
                    HolderName = x.HolderName,
                    HolderRelationship = x.HolderRelationship,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsPrimary = x.IsPrimary,
                    IsEligible = x.IsEligible,
                    LastEligibilityCheckAt = x.LastEligibilityCheckAt,
                    LastEligibilityReferenceNumber = x.LastEligibilityReferenceNumber,
                    EligibilityNote = x.EligibilityNote,
                    AnnualLimitAmount = x.AnnualLimitAmount,
                    RemainingLimitAmount = x.RemainingLimitAmount,
                    CoPaymentPercent = x.CoPaymentPercent,
                    CoPaymentAmount = x.CoPaymentAmount,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedReferralLetter = x.IsNeedReferralLetter,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    CardImagePath = x.CardImagePath,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient insurance tidak ditemukan."));

            return Ok(ApiResponse<PatientInsuranceDetailResponse>.Ok(data, "Detail patient insurance berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Patient Insurance", Description = "Membuat data patient insurance", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientInsurance", "Create")]
        public async Task<IActionResult> CreatePatientInsurance([FromBody] CreatePatientInsuranceRequest request)
        {
            var validation = await ValidateRequestAsync(null, request.PatientId, request.InsuranceProviderId, request.PolicyNumber,
                request.HolderRelationship, request.EffectiveStartDate, request.EffectiveEndDate, request.AnnualLimitAmount,
                request.RemainingLimitAmount, request.CoPaymentPercent, request.CoPaymentAmount);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data patient insurance tidak valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimary)
                await ClearPrimaryInsuranceAsync(request.PatientId, null, now, actorUserId);

            var entity = new MstPatientInsurance
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                InsuranceProviderId = request.InsuranceProviderId,
                PolicyNumber = request.PolicyNumber.Trim(),
                CardNumber = NormalizeNullableText(request.CardNumber),
                MemberNumber = NormalizeNullableText(request.MemberNumber),
                PlanName = NormalizeNullableText(request.PlanName),
                ClassName = NormalizeNullableText(request.ClassName),
                BenefitPlanCode = NormalizeNullableText(request.BenefitPlanCode),
                HolderName = NormalizeNullableText(request.HolderName),
                HolderRelationship = NormalizeNullableText(request.HolderRelationship),
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                IsPrimary = request.IsPrimary,
                IsEligible = request.IsEligible,
                LastEligibilityCheckAt = request.LastEligibilityCheckAt,
                LastEligibilityReferenceNumber = NormalizeNullableText(request.LastEligibilityReferenceNumber),
                EligibilityNote = NormalizeNullableText(request.EligibilityNote),
                AnnualLimitAmount = request.AnnualLimitAmount,
                RemainingLimitAmount = request.RemainingLimitAmount,
                CoPaymentPercent = request.CoPaymentPercent,
                CoPaymentAmount = request.CoPaymentAmount,
                IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = request.IsNeedReferralLetter,
                IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient,
                CardImagePath = NormalizeNullableText(request.CardImagePath),
                Notes = NormalizeNullableText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstPatientInsurance>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildCreateUpdateResponse(entity.Id, true);
            return Ok(ApiResponse<PatientInsuranceCreateResponse>.Ok(response.Create!, "Patient insurance berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Insurance", Description = "Mengubah data patient insurance", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientInsurance", "Update")]
        public async Task<IActionResult> UpdatePatientInsurance(Guid id, [FromBody] UpdatePatientInsuranceRequest request)
        {
            var entity = await _dbContext.Set<MstPatientInsurance>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient insurance tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request.PatientId, request.InsuranceProviderId, request.PolicyNumber,
                request.HolderRelationship, request.EffectiveStartDate, request.EffectiveEndDate, request.AnnualLimitAmount,
                request.RemainingLimitAmount, request.CoPaymentPercent, request.CoPaymentAmount);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data patient insurance tidak valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimary)
                await ClearPrimaryInsuranceAsync(request.PatientId, id, now, actorUserId);

            entity.PatientId = request.PatientId;
            entity.InsuranceProviderId = request.InsuranceProviderId;
            entity.PolicyNumber = request.PolicyNumber.Trim();
            entity.CardNumber = NormalizeNullableText(request.CardNumber);
            entity.MemberNumber = NormalizeNullableText(request.MemberNumber);
            entity.PlanName = NormalizeNullableText(request.PlanName);
            entity.ClassName = NormalizeNullableText(request.ClassName);
            entity.BenefitPlanCode = NormalizeNullableText(request.BenefitPlanCode);
            entity.HolderName = NormalizeNullableText(request.HolderName);
            entity.HolderRelationship = NormalizeNullableText(request.HolderRelationship);
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.IsPrimary = request.IsPrimary;
            entity.IsEligible = request.IsEligible;
            entity.LastEligibilityCheckAt = request.LastEligibilityCheckAt;
            entity.LastEligibilityReferenceNumber = NormalizeNullableText(request.LastEligibilityReferenceNumber);
            entity.EligibilityNote = NormalizeNullableText(request.EligibilityNote);
            entity.AnnualLimitAmount = request.AnnualLimitAmount;
            entity.RemainingLimitAmount = request.RemainingLimitAmount;
            entity.CoPaymentPercent = request.CoPaymentPercent;
            entity.CoPaymentAmount = request.CoPaymentAmount;
            entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
            entity.IsNeedReferralLetter = request.IsNeedReferralLetter;
            entity.IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient;
            entity.CardImagePath = NormalizeNullableText(request.CardImagePath);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildCreateUpdateResponse(entity.Id, false);
            return Ok(ApiResponse<PatientInsuranceUpdateResponse>.Ok(response.Update!, "Patient insurance berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsurancePrimaryStatusResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Patient Insurance", Description = "Mengubah primary patient insurance", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientInsurance", "Update")]
        public async Task<IActionResult> SetPrimaryStatus(Guid id, [FromQuery] bool isPrimary = true)
        {
            var entity = await _dbContext.Set<MstPatientInsurance>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient insurance tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (isPrimary)
                await ClearPrimaryInsuranceAsync(entity.PatientId, id, now, actorUserId);

            entity.IsPrimary = isPrimary;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            var response = await BuildPrimaryStatusResponse(entity.Id);
            return Ok(ApiResponse<PatientInsurancePrimaryStatusResponse>.Ok(response, "Status primary patient insurance berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/eligibility")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceEligibilityResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Patient Insurance", Description = "Mengubah eligibility patient insurance", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientInsurance", "Update")]
        public async Task<IActionResult> UpdateEligibility(Guid id, [FromBody] PatientInsuranceEligibilityRequest request)
        {
            var entity = await _dbContext.Set<MstPatientInsurance>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient insurance tidak ditemukan."));

            if (request.RemainingLimitAmount.HasValue && request.RemainingLimitAmount.Value < 0)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Sisa limit tidak boleh kurang dari 0."));

            entity.IsEligible = request.IsEligible;
            entity.LastEligibilityCheckAt = DateTime.UtcNow;
            entity.LastEligibilityReferenceNumber = NormalizeNullableText(request.LastEligibilityReferenceNumber);
            entity.EligibilityNote = NormalizeNullableText(request.EligibilityNote);
            entity.RemainingLimitAmount = request.RemainingLimitAmount;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            var response = await BuildEligibilityResponse(entity.Id);
            return Ok(ApiResponse<PatientInsuranceEligibilityResponse>.Ok(response, "Eligibility patient insurance berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceStatusResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Patient Insurance", Description = "Mengubah status patient insurance", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientInsurance", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] bool isActive)
        {
            var entity = await _dbContext.Set<MstPatientInsurance>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient insurance tidak ditemukan."));

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            var response = await BuildStatusResponse(entity.Id);
            return Ok(ApiResponse<PatientInsuranceStatusResponse>.Ok(response, "Status patient insurance berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Insurance", Description = "Menghapus data patient insurance", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PatientInsurance", "Delete")]
        public async Task<IActionResult> DeletePatientInsurance(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient insurance tidak ditemukan."));

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            var response = new PatientInsuranceDeleteResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientName = entity.Patient != null ? entity.Patient.FullName : string.Empty,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderName = entity.InsuranceProvider != null ? entity.InsuranceProvider.InsuranceProviderName : string.Empty,
                PolicyNumber = entity.PolicyNumber,
                IsDelete = entity.IsDelete
            };

            return Ok(ApiResponse<PatientInsuranceDeleteResponse>.Ok(response, "Patient insurance berhasil dihapus."));
        }

        private IQueryable<MstPatientInsurance> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatientInsurance>()
                .Include(x => x.Patient)
                .Include(x => x.InsuranceProvider)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatientInsurance> ApplyFilters(
            IQueryable<MstPatientInsurance> query,
            string? search,
            bool? isActive,
            Guid? patientId,
            Guid? insuranceProviderId,
            string? planName,
            string? className,
            string? benefitPlanCode,
            string? holderRelationship,
            bool? isPrimary,
            bool? isEligible,
            bool? isNeedGuaranteeLetter,
            bool? isNeedReferralLetter,
            bool? isAllowExcessPaymentByPatient,
            DateTime? effectiveDate,
            DateTime? lastEligibilityCheckFrom,
            DateTime? lastEligibilityCheckTo,
            decimal? minimumAnnualLimitAmount,
            decimal? maximumAnnualLimitAmount,
            decimal? minimumRemainingLimitAmount,
            decimal? maximumRemainingLimitAmount)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.PolicyNumber.ToLower().Contains(keyword) ||
                    (x.CardNumber != null && x.CardNumber.ToLower().Contains(keyword)) ||
                    (x.MemberNumber != null && x.MemberNumber.ToLower().Contains(keyword)) ||
                    (x.PlanName != null && x.PlanName.ToLower().Contains(keyword)) ||
                    (x.ClassName != null && x.ClassName.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(keyword)) ||
                    (x.HolderName != null && x.HolderName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderCode.ToLower().Contains(keyword)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (insuranceProviderId.HasValue && insuranceProviderId.Value != Guid.Empty) query = query.Where(x => x.InsuranceProviderId == insuranceProviderId.Value);
            if (!string.IsNullOrWhiteSpace(planName)) query = query.Where(x => x.PlanName != null && x.PlanName.ToLower().Contains(planName.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(className)) query = query.Where(x => x.ClassName != null && x.ClassName.ToLower().Contains(className.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(benefitPlanCode)) query = query.Where(x => x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(benefitPlanCode.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(holderRelationship)) query = query.Where(x => x.HolderRelationship != null && x.HolderRelationship == holderRelationship.Trim());
            if (isPrimary.HasValue) query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isEligible.HasValue) query = query.Where(x => x.IsEligible == isEligible.Value);
            if (isNeedGuaranteeLetter.HasValue) query = query.Where(x => x.IsNeedGuaranteeLetter == isNeedGuaranteeLetter.Value);
            if (isNeedReferralLetter.HasValue) query = query.Where(x => x.IsNeedReferralLetter == isNeedReferralLetter.Value);
            if (isAllowExcessPaymentByPatient.HasValue) query = query.Where(x => x.IsAllowExcessPaymentByPatient == isAllowExcessPaymentByPatient.Value);

            if (effectiveDate.HasValue)
            {
                var selectedDate = effectiveDate.Value.Date;
                query = query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= selectedDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= selectedDate));
            }

            if (lastEligibilityCheckFrom.HasValue) query = query.Where(x => x.LastEligibilityCheckAt.HasValue && x.LastEligibilityCheckAt.Value >= lastEligibilityCheckFrom.Value);
            if (lastEligibilityCheckTo.HasValue) query = query.Where(x => x.LastEligibilityCheckAt.HasValue && x.LastEligibilityCheckAt.Value <= lastEligibilityCheckTo.Value);
            if (minimumAnnualLimitAmount.HasValue) query = query.Where(x => x.AnnualLimitAmount.HasValue && x.AnnualLimitAmount.Value >= minimumAnnualLimitAmount.Value);
            if (maximumAnnualLimitAmount.HasValue) query = query.Where(x => x.AnnualLimitAmount.HasValue && x.AnnualLimitAmount.Value <= maximumAnnualLimitAmount.Value);
            if (minimumRemainingLimitAmount.HasValue) query = query.Where(x => x.RemainingLimitAmount.HasValue && x.RemainingLimitAmount.Value >= minimumRemainingLimitAmount.Value);
            if (maximumRemainingLimitAmount.HasValue) query = query.Where(x => x.RemainingLimitAmount.HasValue && x.RemainingLimitAmount.Value <= maximumRemainingLimitAmount.Value);

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid patientId,
            Guid insuranceProviderId,
            string policyNumber,
            string? holderRelationship,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate,
            decimal? annualLimitAmount,
            decimal? remainingLimitAmount,
            decimal? coPaymentPercent,
            decimal? coPaymentAmount)
        {
            if (patientId == Guid.Empty) return (false, "Pasien wajib dipilih.");
            if (insuranceProviderId == Guid.Empty) return (false, "Insurance provider wajib dipilih.");
            if (string.IsNullOrWhiteSpace(policyNumber)) return (false, "Nomor polis wajib diisi.");

            if (!string.IsNullOrWhiteSpace(holderRelationship) && !AllowedHolderRelationships.Contains(holderRelationship.Trim()))
                return (false, "Hubungan pemegang polis tidak valid. Gunakan Self, Spouse, Child, Parent, Sibling, Guardian, Employee, atau Other.");

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue && effectiveEndDate.Value.Date < effectiveStartDate.Value.Date)
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");

            if (annualLimitAmount.HasValue && annualLimitAmount.Value < 0) return (false, "Limit tahunan tidak boleh kurang dari 0.");
            if (remainingLimitAmount.HasValue && remainingLimitAmount.Value < 0) return (false, "Sisa limit tidak boleh kurang dari 0.");
            if (coPaymentPercent.HasValue && (coPaymentPercent.Value < 0 || coPaymentPercent.Value > 100)) return (false, "Co-payment percent harus di antara 0 sampai 100.");
            if (coPaymentAmount.HasValue && coPaymentAmount.Value < 0) return (false, "Co-payment amount tidak boleh kurang dari 0.");

            var patientExists = await _dbContext.Set<MstPatient>().AnyAsync(x => x.Id == patientId && !x.IsDelete);
            if (!patientExists) return (false, "Pasien tidak ditemukan.");

            var providerExists = await _dbContext.Set<MstInsuranceProvider>().AnyAsync(x => x.Id == insuranceProviderId && !x.IsDelete);
            if (!providerExists) return (false, "Insurance provider tidak ditemukan.");

            var normalizedPolicyNumber = policyNumber.Trim().ToLower();
            var duplicatePolicy = await _dbContext.Set<MstPatientInsurance>().AnyAsync(x =>
                !x.IsDelete &&
                x.PatientId == patientId &&
                x.InsuranceProviderId == insuranceProviderId &&
                x.PolicyNumber.ToLower() == normalizedPolicyNumber &&
                (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicatePolicy)
                return (false, "Nomor polis untuk pasien dan insurance provider tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task ClearPrimaryInsuranceAsync(Guid patientId, Guid? excludeId, DateTime now, Guid actorUserId)
        {
            var existingPrimaries = await _dbContext.Set<MstPatientInsurance>()
                .Where(x => !x.IsDelete && x.PatientId == patientId && x.IsPrimary && (!excludeId.HasValue || x.Id != excludeId.Value))
                .ToListAsync();

            foreach (var item in existingPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task<(PatientInsuranceCreateResponse? Create, PatientInsuranceUpdateResponse? Update)> BuildCreateUpdateResponse(Guid id, bool isCreate)
        {
            var data = await BuildBaseQuery().AsNoTracking().FirstAsync(x => x.Id == id);

            if (isCreate)
            {
                return (new PatientInsuranceCreateResponse
                {
                    Id = data.Id,
                    PatientId = data.PatientId,
                    PatientName = data.Patient != null ? data.Patient.FullName : string.Empty,
                    InsuranceProviderId = data.InsuranceProviderId,
                    InsuranceProviderName = data.InsuranceProvider != null ? data.InsuranceProvider.InsuranceProviderName : string.Empty,
                    PolicyNumber = data.PolicyNumber,
                    IsPrimary = data.IsPrimary,
                    IsEligible = data.IsEligible,
                    IsActive = data.IsActive
                }, null);
            }

            return (null, new PatientInsuranceUpdateResponse
            {
                Id = data.Id,
                PatientId = data.PatientId,
                PatientName = data.Patient != null ? data.Patient.FullName : string.Empty,
                InsuranceProviderId = data.InsuranceProviderId,
                InsuranceProviderName = data.InsuranceProvider != null ? data.InsuranceProvider.InsuranceProviderName : string.Empty,
                PolicyNumber = data.PolicyNumber,
                IsPrimary = data.IsPrimary,
                IsEligible = data.IsEligible,
                IsActive = data.IsActive
            });
        }

        private async Task<PatientInsuranceStatusResponse> BuildStatusResponse(Guid id)
        {
            return await BuildBaseQuery().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PatientInsuranceStatusResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                    PolicyNumber = x.PolicyNumber,
                    IsActive = x.IsActive
                })
                .FirstAsync();
        }

        private async Task<PatientInsurancePrimaryStatusResponse> BuildPrimaryStatusResponse(Guid id)
        {
            return await BuildBaseQuery().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PatientInsurancePrimaryStatusResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                    PolicyNumber = x.PolicyNumber,
                    IsPrimary = x.IsPrimary
                })
                .FirstAsync();
        }

        private async Task<PatientInsuranceEligibilityResponse> BuildEligibilityResponse(Guid id)
        {
            return await BuildBaseQuery().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PatientInsuranceEligibilityResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                    PolicyNumber = x.PolicyNumber,
                    IsEligible = x.IsEligible,
                    LastEligibilityCheckAt = x.LastEligibilityCheckAt,
                    LastEligibilityReferenceNumber = x.LastEligibilityReferenceNumber,
                    EligibilityNote = x.EligibilityNote,
                    RemainingLimitAmount = x.RemainingLimitAmount
                })
                .FirstAsync();
        }

        private static PatientInsuranceResponse ToResponse(MstPatientInsurance x)
        {
            return new PatientInsuranceResponse
            {
                Id = x.Id,
                PatientId = x.PatientId,
                PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                InsuranceProviderId = x.InsuranceProviderId,
                InsuranceProviderCode = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderCode : string.Empty,
                InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                InsuranceGroupName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceGroupName : null,
                ProviderType = x.InsuranceProvider != null ? x.InsuranceProvider.ProviderType : null,
                ClaimMethod = x.InsuranceProvider != null ? x.InsuranceProvider.ClaimMethod : null,
                PolicyNumber = x.PolicyNumber,
                CardNumber = x.CardNumber,
                MemberNumber = x.MemberNumber,
                PlanName = x.PlanName,
                ClassName = x.ClassName,
                BenefitPlanCode = x.BenefitPlanCode,
                HolderName = x.HolderName,
                HolderRelationship = x.HolderRelationship,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                IsPrimary = x.IsPrimary,
                IsEligible = x.IsEligible,
                LastEligibilityCheckAt = x.LastEligibilityCheckAt,
                LastEligibilityReferenceNumber = x.LastEligibilityReferenceNumber,
                AnnualLimitAmount = x.AnnualLimitAmount,
                RemainingLimitAmount = x.RemainingLimitAmount,
                CoPaymentPercent = x.CoPaymentPercent,
                CoPaymentAmount = x.CoPaymentAmount,
                IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = x.IsNeedReferralLetter,
                IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                CardImagePath = x.CardImagePath,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static IQueryable<MstPatientInsurance> ApplySorting(IQueryable<MstPatientInsurance> query, string? sortBy, string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").ToLowerInvariant() switch
            {
                "patientname" => isDesc ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : string.Empty) : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),
                "medicalrecordnumber" => isDesc ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty) : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty),
                "insuranceprovidername" => isDesc ? query.OrderByDescending(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty) : query.OrderBy(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty),
                "policynumber" => isDesc ? query.OrderByDescending(x => x.PolicyNumber) : query.OrderBy(x => x.PolicyNumber),
                "planname" => isDesc ? query.OrderByDescending(x => x.PlanName) : query.OrderBy(x => x.PlanName),
                "classname" => isDesc ? query.OrderByDescending(x => x.ClassName) : query.OrderBy(x => x.ClassName),
                "effectivestartdate" => isDesc ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDesc ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "lasteligibilitycheckat" => isDesc ? query.OrderByDescending(x => x.LastEligibilityCheckAt) : query.OrderBy(x => x.LastEligibilityCheckAt),
                "isprimary" => isDesc ? query.OrderByDescending(x => x.IsPrimary) : query.OrderBy(x => x.IsPrimary),
                "iseligible" => isDesc ? query.OrderByDescending(x => x.IsEligible) : query.OrderBy(x => x.IsEligible),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
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
            return Guid.TryParse(userIdText, out var userId) ? userId : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}