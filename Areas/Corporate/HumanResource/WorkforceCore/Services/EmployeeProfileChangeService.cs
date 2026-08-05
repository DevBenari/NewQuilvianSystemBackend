using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public class EmployeeProfileChangeService
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";
        private const int MaximumEvidenceFileSizeBytes = 10 * 1024 * 1024;
        private const string RequestNumberPrefix = "EPC-RSMMC-";
        private const string AdvisoryLockName = "HR_EMPLOYEE_PROFILE_CHANGE_REQUEST_NUMBER";

        private static readonly string[] RequestStatuses =
        {
            "Draft",
            "Submitted",
            "UnderVerification",
            "NeedRevision",
            "Approved",
            "Rejected",
            "Cancelled",
            "Applied"
        };

        private static readonly string[] RequestCategories =
        {
            "Profile",
            "PersonalData",
            "Contact",
            "Address",
            "Identity",
            "EmergencyContact"
        };

        private static readonly string[] VerificationStatuses =
        {
            "Pending",
            "Verified",
            "Rejected",
            "NeedRevision"
        };

        private static readonly string[] VerificationTypes =
        {
            "HR",
            "Document",
            "Identity",
            "Supervisor"
        };

        private static readonly HashSet<string> AllowedEvidenceExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf",
                ".jpg",
                ".jpeg",
                ".png"
            };

        private static readonly Dictionary<string, AllowedFieldDefinition> AllowedFields =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [BuildFieldKey("MstWorkforceProfile", "DisplayName")] =
                    new("MstWorkforceProfile", "Profile", "DisplayName", "Nama tampilan", "String", true),
                [BuildFieldKey("MstWorkforceProfile", "Email")] =
                    new("MstWorkforceProfile", "Contact", "Email", "Email workforce", "String", true),
                [BuildFieldKey("MstWorkforceProfile", "PhoneNumber")] =
                    new("MstWorkforceProfile", "Contact", "PhoneNumber", "Nomor telepon workforce", "String", true),
                [BuildFieldKey("MstWorkforceProfile", "WhatsAppNumber")] =
                    new("MstWorkforceProfile", "Contact", "WhatsAppNumber", "Nomor WhatsApp workforce", "String", true),

                [BuildFieldKey("MstEmployee", "FullName")] =
                    new("MstEmployee", "Profile", "FullName", "Nama lengkap", "String", true),
                [BuildFieldKey("MstEmployee", "NickName")] =
                    new("MstEmployee", "Profile", "NickName", "Nama panggilan", "String", false),
                [BuildFieldKey("MstEmployee", "BirthPlace")] =
                    new("MstEmployee", "PersonalData", "BirthPlace", "Tempat lahir", "String", true),
                [BuildFieldKey("MstEmployee", "BirthDate")] =
                    new("MstEmployee", "PersonalData", "BirthDate", "Tanggal lahir", "Date", true),
                [BuildFieldKey("MstEmployee", "Gender")] =
                    new("MstEmployee", "PersonalData", "Gender", "Jenis kelamin", "Enum", true),
                [BuildFieldKey("MstEmployee", "Religion")] =
                    new("MstEmployee", "PersonalData", "Religion", "Agama", "Enum", true),
                [BuildFieldKey("MstEmployee", "MaritalStatus")] =
                    new("MstEmployee", "PersonalData", "MaritalStatus", "Status pernikahan", "Enum", true),
                [BuildFieldKey("MstEmployee", "BloodType")] =
                    new("MstEmployee", "PersonalData", "BloodType", "Golongan darah", "Enum", true),
                [BuildFieldKey("MstEmployee", "IdentityType")] =
                    new("MstEmployee", "Identity", "IdentityType", "Jenis identitas", "String", true),
                [BuildFieldKey("MstEmployee", "IdentityNumber")] =
                    new("MstEmployee", "Identity", "IdentityNumber", "Nomor identitas", "String", true),
                [BuildFieldKey("MstEmployee", "PhoneNumber")] =
                    new("MstEmployee", "Contact", "PhoneNumber", "Nomor telepon", "String", true),
                [BuildFieldKey("MstEmployee", "WhatsAppNumber")] =
                    new("MstEmployee", "Contact", "WhatsAppNumber", "Nomor WhatsApp", "String", true),
                [BuildFieldKey("MstEmployee", "Email")] =
                    new("MstEmployee", "Contact", "Email", "Email", "String", true),
                [BuildFieldKey("MstEmployee", "Address")] =
                    new("MstEmployee", "Address", "Address", "Alamat", "String", true),
                [BuildFieldKey("MstEmployee", "CountryId")] =
                    new("MstEmployee", "Address", "CountryId", "Negara", "Guid", true),
                [BuildFieldKey("MstEmployee", "ProvinceId")] =
                    new("MstEmployee", "Address", "ProvinceId", "Provinsi", "Guid", true),
                [BuildFieldKey("MstEmployee", "CityId")] =
                    new("MstEmployee", "Address", "CityId", "Kota", "Guid", true),
                [BuildFieldKey("MstEmployee", "DistrictId")] =
                    new("MstEmployee", "Address", "DistrictId", "Kecamatan", "Guid", true),
                [BuildFieldKey("MstEmployee", "PostalCodeId")] =
                    new("MstEmployee", "Address", "PostalCodeId", "Kode pos", "Guid", true),
                [BuildFieldKey("MstEmployee", "EmergencyContactName")] =
                    new("MstEmployee", "EmergencyContact", "EmergencyContactName", "Nama kontak darurat", "String", true),
                [BuildFieldKey("MstEmployee", "EmergencyContactRelation")] =
                    new("MstEmployee", "EmergencyContact", "EmergencyContactRelation", "Hubungan kontak darurat", "String", true),
                [BuildFieldKey("MstEmployee", "EmergencyContactPhone")] =
                    new("MstEmployee", "EmergencyContact", "EmergencyContactPhone", "Telepon kontak darurat", "String", true),
                [BuildFieldKey("MstEmployee", "EmergencyContactAddress")] =
                    new("MstEmployee", "EmergencyContact", "EmergencyContactAddress", "Alamat kontak darurat", "String", false)
            };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public EmployeeProfileChangeService(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _configuration = configuration;
            _environment = environment;
        }

        public EmployeeProfileChangeFilterMetadataResponse GetFilterMetadata()
        {
            return new EmployeeProfileChangeFilterMetadataResponse
            {
                DefaultFilter = new EmployeeProfileChangeDefaultFilterResponse(),
                CustomPeriods = new List<EmployeeProfileChangeStringOptionResponse>
                {
                    new() { Value = "custom", Label = "Custom" },
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "last30days", Label = "30 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" }
                },
                RequestStatuses = RequestStatuses
                    .Select(x => new EmployeeProfileChangeStringOptionResponse
                    {
                        Value = x,
                        Label = BuildStatusLabel(x)
                    })
                    .ToList(),
                RequestCategories = RequestCategories
                    .Select(x => new EmployeeProfileChangeStringOptionResponse
                    {
                        Value = x,
                        Label = BuildCategoryLabel(x)
                    })
                    .ToList(),
                VerificationStatuses = VerificationStatuses
                    .Select(x => new EmployeeProfileChangeStringOptionResponse
                    {
                        Value = x,
                        Label = BuildStatusLabel(x)
                    })
                    .ToList(),
                VerificationTypes = VerificationTypes
                    .Select(x => new EmployeeProfileChangeStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),
                SortOptions = new List<EmployeeProfileChangeStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "requestNumber", Label = "Nomor permintaan" },
                    new() { Value = "workforceDisplayName", Label = "Nama tenaga kerja" },
                    new() { Value = "requestStatus", Label = "Status permintaan" },
                    new() { Value = "submittedAt", Label = "Tanggal submit" },
                    new() { Value = "approvedAt", Label = "Tanggal approval" },
                    new() { Value = "appliedAt", Label = "Tanggal diterapkan" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                AllowedFields = AllowedFields.Values
                    .OrderBy(x => x.TargetEntityName)
                    .ThenBy(x => x.FieldGroup)
                    .ThenBy(x => x.FieldName)
                    .Select(x => new EmployeeProfileChangeAllowedFieldResponse
                    {
                        TargetEntityName = x.TargetEntityName,
                        FieldGroup = x.FieldGroup,
                        FieldName = x.FieldName,
                        Label = x.Label,
                        ValueType = x.ValueType,
                        RequiresVerificationDefault = x.RequiresVerificationDefault
                    })
                    .ToList(),
                Transitions = new List<EmployeeProfileChangeTransitionResponse>
                {
                    new() { FromStatus = "Draft", AllowedActions = new() { "Update", "Submit", "Cancel", "Delete" } },
                    new() { FromStatus = "Submitted", AllowedActions = new() { "StartVerification", "Reject", "RequestRevision", "Cancel" } },
                    new() { FromStatus = "UnderVerification", AllowedActions = new() { "Verify", "Reject", "RequestRevision", "Approve", "Cancel" } },
                    new() { FromStatus = "NeedRevision", AllowedActions = new() { "Update", "Submit", "Cancel", "Delete" } },
                    new() { FromStatus = "Approved", AllowedActions = new() { "Apply" } },
                    new() { FromStatus = "Rejected", AllowedActions = new() },
                    new() { FromStatus = "Cancelled", AllowedActions = new() { "Delete" } },
                    new() { FromStatus = "Applied", AllowedActions = new() }
                },
                AllowedEvidenceExtensions = AllowedEvidenceExtensions
                    .OrderBy(x => x)
                    .ToList()
            };
        }

        public async Task<EmployeeProfileChangeSummaryResponse> GetSummaryAsync(
            Guid? workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
                query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);

            return new EmployeeProfileChangeSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                DraftData = await query.CountAsync(x => x.RequestStatus == "Draft", cancellationToken),
                SubmittedData = await query.CountAsync(x => x.RequestStatus == "Submitted", cancellationToken),
                UnderVerificationData = await query.CountAsync(x => x.RequestStatus == "UnderVerification", cancellationToken),
                NeedRevisionData = await query.CountAsync(x => x.RequestStatus == "NeedRevision", cancellationToken),
                ApprovedData = await query.CountAsync(x => x.RequestStatus == "Approved", cancellationToken),
                RejectedData = await query.CountAsync(x => x.RequestStatus == "Rejected", cancellationToken),
                CancelledData = await query.CountAsync(x => x.RequestStatus == "Cancelled", cancellationToken),
                AppliedData = await query.CountAsync(x => x.RequestStatus == "Applied", cancellationToken)
            };
        }

        public async Task<PagedResult<EmployeeProfileChangeListResponse>> GetPagedAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? period,
            Guid? workforceProfileId,
            string? requestStatus,
            string? requestCategory,
            Guid? requestedByUserId,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            NormalizePaging(ref pageNumber, ref pageSize);

            var query = _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x => !x.IsDelete);

            var dateRange = ResolveDateRange(startDate, endDate, period);

            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
                query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);

            if (!string.IsNullOrWhiteSpace(requestStatus))
                query = query.Where(x => x.RequestStatus == NormalizeStatus(requestStatus));

            if (!string.IsNullOrWhiteSpace(requestCategory))
                query = query.Where(x => x.RequestCategory == NormalizeCategory(requestCategory));

            if (requestedByUserId.HasValue && requestedByUserId.Value != Guid.Empty)
                query = query.Where(x => x.RequestedByUserId == requestedByUserId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    x.RequestCategory.ToLower().Contains(keyword) ||
                    x.RequestStatus.ToLower().Contains(keyword) ||
                    (x.RequestReasonText != null && x.RequestReasonText.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);

            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EmployeeProfileChangeListResponse
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    RequestReasonId = x.RequestReasonId,
                    RequestCategory = x.RequestCategory,
                    RequestStatus = x.RequestStatus,
                    RequestReasonText = x.RequestReasonText,
                    RequestedByUserId = x.RequestedByUserId,
                    SubmittedAt = x.SubmittedAt,
                    ApprovedAt = x.ApprovedAt,
                    RejectedAt = x.RejectedAt,
                    AppliedAt = x.AppliedAt,
                    CurrentStepOrder = x.CurrentStepOrder,
                    DetailCount = x.Details.Count(d => !d.IsDelete),
                    PendingVerificationCount = x.Verifications.Count(v => !v.IsDelete && v.VerificationStatus == "Pending"),
                    VerifiedVerificationCount = x.Verifications.Count(v => !v.IsDelete && v.VerificationStatus == "Verified"),
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
                })
                .ToListAsync(cancellationToken);

            var actorIds = rows
                .SelectMany(x => new Guid?[]
                {
                    x.RequestedByUserId,
                    x.CreateBy,
                    x.UpdateBy
                })
                .Where(x => x.HasValue && x.Value != Guid.Empty)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var actorNames = await BuildActorNameMapAsync(actorIds, cancellationToken);

            foreach (var row in rows)
            {
                row.RequestedByUserName = GetActorName(actorNames, row.RequestedByUserId);
                row.CreateByName = GetActorName(actorNames, row.CreateBy);
                row.UpdateByName = GetActorName(actorNames, row.UpdateBy);
            }

            return new PagedResult<EmployeeProfileChangeListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows
            };
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await BuildDetailQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Permintaan perubahan profil tidak ditemukan.");
            }

            var actorIds = new List<Guid?>
            {
                entity.RequestedByUserId,
                entity.ApprovedByUserId,
                entity.RejectedByUserId,
                entity.AppliedByUserId,
                entity.CreateBy,
                entity.UpdateBy
            };

            actorIds.AddRange(entity.Verifications.Select(x => x.VerifiedByUserId));

            var actorNames = await BuildActorNameMapAsync(
                actorIds
                    .Where(x => x.HasValue && x.Value != Guid.Empty)
                    .Select(x => x!.Value),
                cancellationToken);

            return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Ok(
                MapResponse(entity, actorNames),
                "Detail permintaan perubahan profil berhasil diambil.");
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> CreateDraftAsync(
            CreateEmployeeProfileChangeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateHeaderAndDetailsAsync(
                request.WorkforceProfileId,
                request.WorkflowDefinitionId,
                request.RequestReasonId,
                request.RequestCategory,
                request.Details,
                cancellationToken);

            if (!validation.Success)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            var hasOpenRequest = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == request.WorkforceProfileId &&
                    !x.IsDelete &&
                    (x.RequestStatus == "Draft" ||
                     x.RequestStatus == "Submitted" ||
                     x.RequestStatus == "UnderVerification" ||
                     x.RequestStatus == "NeedRevision" ||
                     x.RequestStatus == "Approved"),
                    cancellationToken);

            if (hasOpenRequest)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Workforce profile masih memiliki permintaan perubahan profil yang belum selesai.");
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await AcquireRequestNumberLockAsync(cancellationToken);

                var entity = new TrxEmployeeProfileChangeRequest
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = request.WorkforceProfileId,
                    WorkflowDefinitionId = NormalizeGuid(request.WorkflowDefinitionId),
                    RequestReasonId = NormalizeGuid(request.RequestReasonId),
                    RequestNumber = await GenerateRequestNumberAsync(cancellationToken),
                    RequestCategory = NormalizeCategory(request.RequestCategory),
                    RequestStatus = "Draft",
                    RequestReasonText = NormalizeText(request.RequestReasonText),
                    RequestedByUserId = actorUserId,
                    CurrentStepOrder = 0,
                    Description = NormalizeText(request.Description),
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<TrxEmployeeProfileChangeRequest>().Add(entity);

                var detailsResult = await BuildDetailEntitiesAsync(
                    entity.Id,
                    request.WorkforceProfileId,
                    request.Details,
                    actorUserId,
                    now,
                    cancellationToken);

                if (!detailsResult.Success || detailsResult.Data == null)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                        detailsResult.StatusCode,
                        detailsResult.Message);
                }

                _dbContext.Set<TrxEmployeeProfileChangeDetail>().AddRange(detailsResult.Data);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "EmployeeProfileChange.CreateDraft",
                    "Membuat draft perubahan profil pegawai.",
                    new
                    {
                        entity.Id,
                        entity.RequestNumber,
                        entity.WorkforceProfileId,
                        DetailCount = detailsResult.Data.Count
                    });

                return await GetByIdAsync(entity.Id, cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> UpdateDraftAsync(
            Guid id,
            UpdateEmployeeProfileChangeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .Include(x => x.Details)
                .Include(x => x.Verifications)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Permintaan perubahan profil tidak ditemukan.");
            }

            if (entity.RequestStatus != "Draft" && entity.RequestStatus != "NeedRevision")
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Permintaan hanya dapat diubah pada status Draft atau NeedRevision.");
            }

            var validation = await ValidateHeaderAndDetailsAsync(
                entity.WorkforceProfileId,
                request.WorkflowDefinitionId,
                request.RequestReasonId,
                request.RequestCategory,
                request.Details,
                cancellationToken);

            if (!validation.Success)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    validation.StatusCode,
                    validation.Message);
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                SoftDeleteChildren(entity.Details, actorUserId, now);
                SoftDeleteChildren(entity.Verifications, actorUserId, now);

                var detailsResult = await BuildDetailEntitiesAsync(
                    entity.Id,
                    entity.WorkforceProfileId,
                    request.Details,
                    actorUserId,
                    now,
                    cancellationToken);

                if (!detailsResult.Success || detailsResult.Data == null)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                        detailsResult.StatusCode,
                        detailsResult.Message);
                }

                _dbContext.Set<TrxEmployeeProfileChangeDetail>().AddRange(detailsResult.Data);

                entity.WorkflowDefinitionId = NormalizeGuid(request.WorkflowDefinitionId);
                entity.RequestReasonId = NormalizeGuid(request.RequestReasonId);
                entity.RequestCategory = NormalizeCategory(request.RequestCategory);
                entity.RequestReasonText = NormalizeText(request.RequestReasonText);
                entity.Description = NormalizeText(request.Description);
                entity.CurrentStepOrder = 0;
                entity.SubmittedAt = null;
                entity.ApprovedAt = null;
                entity.ApprovedByUserId = null;
                entity.RejectedAt = null;
                entity.RejectedByUserId = null;
                entity.AppliedAt = null;
                entity.AppliedByUserId = null;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetByIdAsync(entity.Id, cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> SubmitAsync(
            Guid id,
            string? note,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .Include(x => x.Details)
                .Include(x => x.Verifications)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFoundResult();

            if (entity.RequestStatus != "Draft" && entity.RequestStatus != "NeedRevision")
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Permintaan hanya dapat disubmit dari status Draft atau NeedRevision.");
            }

            var activeDetails = entity.Details
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ToList();

            if (activeDetails.Count == 0)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Permintaan belum memiliki detail perubahan.");
            }

            var refreshResult = await RefreshAndValidateDetailOldValuesAsync(
                entity.WorkforceProfileId,
                activeDetails,
                actorUserId,
                cancellationToken);

            if (!refreshResult.Success)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    refreshResult.StatusCode,
                    refreshResult.Message);
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                SoftDeleteChildren(entity.Verifications, actorUserId, now);

                foreach (var detail in activeDetails.Where(x => x.RequiresVerification))
                {
                    _dbContext.Set<TrxEmployeeProfileChangeVerification>().Add(
                        new TrxEmployeeProfileChangeVerification
                        {
                            Id = Guid.NewGuid(),
                            ProfileChangeRequestId = entity.Id,
                            ProfileChangeDetailId = detail.Id,
                            VerificationType = ResolveVerificationType(detail.FieldGroup),
                            VerificationStatus = "Pending",
                            IsFinalVerification = false,
                            CreateDateTime = now,
                            CreateBy = actorUserId,
                            IsDelete = false,
                            IsCancel = false
                        });
                }

                _dbContext.Set<TrxEmployeeProfileChangeVerification>().Add(
                    new TrxEmployeeProfileChangeVerification
                    {
                        Id = Guid.NewGuid(),
                        ProfileChangeRequestId = entity.Id,
                        ProfileChangeDetailId = null,
                        VerificationType = "HR",
                        VerificationStatus = "Pending",
                        IsFinalVerification = true,
                        VerificationNote = NormalizeText(note),
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        IsDelete = false,
                        IsCancel = false
                    });

                foreach (var detail in activeDetails)
                {
                    detail.DetailStatus = detail.RequiresVerification ? "Pending" : "Verified";
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;
                }

                entity.RequestStatus = "Submitted";
                entity.SubmittedAt = now;
                entity.CurrentStepOrder = 1;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetByIdAsync(entity.Id, cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> StartVerificationAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindRequestForUpdateAsync(id, cancellationToken);

            if (entity == null)
                return NotFoundResult();

            if (entity.RequestStatus != "Submitted")
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Verifikasi hanya dapat dimulai dari status Submitted.");
            }

            entity.RequestStatus = "UnderVerification";
            entity.CurrentStepOrder = 2;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> DecideVerificationAsync(
            Guid requestId,
            Guid verificationId,
            EmployeeProfileChangeVerificationDecisionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var normalizedDecision = NormalizeVerificationStatus(request.VerificationStatus);

            if (!VerificationStatuses.Contains(normalizedDecision, StringComparer.OrdinalIgnoreCase) ||
                normalizedDecision == "Pending")
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Keputusan verifikasi harus Verified, Rejected, atau NeedRevision.");
            }

            var fileValidation = ValidateEvidenceFile(request.EvidenceFile);

            if (!fileValidation.Success)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    fileValidation.StatusCode,
                    fileValidation.Message);
            }

            var entity = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .Include(x => x.Details)
                .Include(x => x.Verifications)
                .FirstOrDefaultAsync(x => x.Id == requestId && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFoundResult();

            if (entity.RequestStatus != "UnderVerification")
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Keputusan verifikasi hanya dapat dilakukan pada status UnderVerification.");
            }

            var verification = entity.Verifications
                .FirstOrDefault(x => x.Id == verificationId && !x.IsDelete);

            if (verification == null)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data verifikasi tidak ditemukan.");
            }

            if (verification.IsFinalVerification && normalizedDecision == "Verified")
            {
                var hasUnverifiedDetail = entity.Verifications.Any(x =>
                    !x.IsDelete &&
                    !x.IsFinalVerification &&
                    x.VerificationStatus != "Verified");

                if (hasUnverifiedDetail)
                {
                    return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Verifikasi final belum dapat disetujui karena masih ada detail yang belum terverifikasi.");
                }
            }

            string? newEvidencePath = null;
            var oldEvidencePath = verification.EvidenceFilePath;

            if (request.EvidenceFile != null)
            {
                newEvidencePath = await SaveEvidenceFileAsync(
                    entity.WorkforceProfileId,
                    entity.Id,
                    request.EvidenceFile,
                    cancellationToken);
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                verification.VerificationStatus = normalizedDecision;
                verification.VerifiedByUserId = actorUserId;
                verification.VerifiedAt = now;
                verification.VerificationNote = NormalizeText(request.VerificationNote);

                if (newEvidencePath != null)
                    verification.EvidenceFilePath = newEvidencePath;

                verification.UpdateDateTime = now;
                verification.UpdateBy = actorUserId;

                if (verification.ProfileChangeDetailId.HasValue)
                {
                    var detail = entity.Details.FirstOrDefault(x =>
                        x.Id == verification.ProfileChangeDetailId.Value &&
                        !x.IsDelete);

                    if (detail != null)
                    {
                        detail.DetailStatus = normalizedDecision;
                        detail.UpdateDateTime = now;
                        detail.UpdateBy = actorUserId;
                    }
                }

                if (normalizedDecision == "Rejected")
                {
                    entity.RequestStatus = "Rejected";
                    entity.RejectedAt = now;
                    entity.RejectedByUserId = actorUserId;
                }
                else if (normalizedDecision == "NeedRevision")
                {
                    entity.RequestStatus = "NeedRevision";
                    entity.CurrentStepOrder = 0;
                }

                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (newEvidencePath != null && !string.IsNullOrWhiteSpace(oldEvidencePath))
                    DeletePhysicalFile(oldEvidencePath);

                return await GetByIdAsync(entity.Id, cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);

                if (newEvidencePath != null)
                    DeletePhysicalFile(newEvidencePath);

                throw;
            }
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> ApproveAsync(
            Guid id,
            string? note,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .Include(x => x.Verifications)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFoundResult();

            if (entity.RequestStatus != "UnderVerification")
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Permintaan hanya dapat disetujui dari status UnderVerification.");
            }

            var activeVerifications = entity.Verifications.Where(x => !x.IsDelete).ToList();

            if (activeVerifications.Count == 0 ||
                activeVerifications.Any(x => x.VerificationStatus != "Verified") ||
                !activeVerifications.Any(x => x.IsFinalVerification))
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Seluruh verifikasi detail dan verifikasi final harus berstatus Verified.");
            }

            var now = DateTime.UtcNow;

            entity.RequestStatus = "Approved";
            entity.ApprovedAt = now;
            entity.ApprovedByUserId = actorUserId;
            entity.CurrentStepOrder = 3;
            entity.Description = AppendNote(entity.Description, note, "Approval");
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> RejectAsync(
            Guid id,
            string reason,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindRequestForUpdateAsync(id, cancellationToken);

            if (entity == null)
                return NotFoundResult();

            if (entity.RequestStatus != "Submitted" && entity.RequestStatus != "UnderVerification")
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Permintaan hanya dapat ditolak dari status Submitted atau UnderVerification.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan penolakan wajib diisi.");
            }

            var now = DateTime.UtcNow;

            entity.RequestStatus = "Rejected";
            entity.RejectedAt = now;
            entity.RejectedByUserId = actorUserId;
            entity.Description = AppendNote(entity.Description, reason, "Rejected");
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> RequestRevisionAsync(
            Guid id,
            string reason,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindRequestForUpdateAsync(id, cancellationToken);

            if (entity == null)
                return NotFoundResult();

            if (entity.RequestStatus != "Submitted" && entity.RequestStatus != "UnderVerification")
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Revisi hanya dapat diminta dari status Submitted atau UnderVerification.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan revisi wajib diisi.");
            }

            var now = DateTime.UtcNow;

            entity.RequestStatus = "NeedRevision";
            entity.CurrentStepOrder = 0;
            entity.Description = AppendNote(entity.Description, reason, "NeedRevision");
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeApplyResponse>> ApplyAsync(
            Guid id,
            ApplyEmployeeProfileChangeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeApplyResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Permintaan perubahan profil tidak ditemukan.");
            }

            if (entity.RequestStatus != "Approved")
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeApplyResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Perubahan hanya dapat diterapkan dari status Approved.");
            }

            var activeDetails = entity.Details
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ToList();

            if (activeDetails.Count == 0)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeApplyResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Permintaan tidak memiliki detail perubahan.");
            }

            var workforceProfile = await _dbContext.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(x =>
                    x.Id == entity.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            var employee = await _dbContext.Set<MstEmployee>()
                .FirstOrDefaultAsync(x =>
                    x.WorkforceProfileId == entity.WorkforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (workforceProfile == null || employee == null)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeApplyResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Workforce profile atau employee tidak ditemukan.");
            }

            var planResult = await BuildApplyPlanAsync(
                entity,
                activeDetails,
                workforceProfile,
                employee,
                request.EnforceOldValueMatch,
                cancellationToken);

            if (!planResult.Success || planResult.Data == null)
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeApplyResponse>.Fail(
                    planResult.StatusCode,
                    planResult.Message);
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var item in planResult.Data)
                    item.Property.SetValue(item.Target, item.ConvertedValue);

                SynchronizeWorkforceProfile(employee, workforceProfile, planResult.Data);

                workforceProfile.UpdateDateTime = now;
                workforceProfile.UpdateBy = actorUserId;
                employee.UpdateDateTime = now;
                employee.UpdateBy = actorUserId;

                foreach (var detail in activeDetails)
                {
                    detail.DetailStatus = "Applied";
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;
                }

                entity.RequestStatus = "Applied";
                entity.AppliedAt = now;
                entity.AppliedByUserId = actorUserId;
                entity.CurrentStepOrder = 4;
                entity.Description = AppendNote(entity.Description, request.Note, "Applied");
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var response = new EmployeeProfileChangeApplyResponse
                {
                    RequestId = entity.Id,
                    RequestNumber = entity.RequestNumber,
                    RequestStatus = entity.RequestStatus,
                    AppliedAt = now,
                    AppliedDetailCount = activeDetails.Count,
                    AppliedFields = activeDetails
                        .Select(x => $"{x.TargetEntityName}.{x.FieldName}")
                        .ToList()
                };

                await _loggerService.InfoAsync(
                    LogCategory,
                    "EmployeeProfileChange.Apply",
                    "Menerapkan perubahan profil pegawai.",
                    response);

                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeApplyResponse>.Ok(
                    response,
                    "Perubahan profil berhasil diterapkan.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>> CancelAsync(
            Guid id,
            string? note,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindRequestForUpdateAsync(id, cancellationToken);

            if (entity == null)
                return NotFoundResult();

            var allowedStatuses = new[] { "Draft", "Submitted", "UnderVerification", "NeedRevision" };

            if (!allowedStatuses.Contains(entity.RequestStatus, StringComparer.OrdinalIgnoreCase))
            {
                return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Permintaan pada status saat ini tidak dapat dibatalkan.");
            }

            var now = DateTime.UtcNow;

            entity.RequestStatus = "Cancelled";
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.Description = AppendNote(entity.Description, note, "Cancelled");
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EmployeeProfileChangeServiceResult<object>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .Include(x => x.Details)
                .Include(x => x.Verifications)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Permintaan perubahan profil tidak ditemukan.");
            }

            if (entity.RequestStatus != "Draft" &&
                entity.RequestStatus != "NeedRevision" &&
                entity.RequestStatus != "Cancelled")
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Permintaan hanya dapat dihapus pada status Draft, NeedRevision, atau Cancelled.");
            }

            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            SoftDeleteChildren(entity.Details, actorUserId, now);
            SoftDeleteChildren(entity.Verifications, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return EmployeeProfileChangeServiceResult<object>.Ok(
                null,
                "Permintaan perubahan profil berhasil dihapus.");
        }

        public async Task<EmployeeProfileChangeServiceResult<(string PhysicalPath, string FileName, string ContentType)>> GetEvidenceFileAsync(
            Guid requestId,
            Guid verificationId,
            CancellationToken cancellationToken = default)
        {
            var verification = await _dbContext.Set<TrxEmployeeProfileChangeVerification>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == verificationId &&
                    x.ProfileChangeRequestId == requestId &&
                    !x.IsDelete,
                    cancellationToken);

            if (verification == null)
            {
                return EmployeeProfileChangeServiceResult<(string, string, string)>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data verifikasi tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(verification.EvidenceFilePath))
            {
                return EmployeeProfileChangeServiceResult<(string, string, string)>.Fail(
                    StatusCodes.Status404NotFound,
                    "File bukti verifikasi belum tersedia.");
            }

            var physicalPath = ResolvePhysicalPath(verification.EvidenceFilePath);

            if (!File.Exists(physicalPath))
            {
                return EmployeeProfileChangeServiceResult<(string, string, string)>.Fail(
                    StatusCodes.Status404NotFound,
                    "File bukti verifikasi tidak ditemukan pada storage.");
            }

            var extension = Path.GetExtension(physicalPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            return EmployeeProfileChangeServiceResult<(string, string, string)>.Ok(
                (physicalPath, Path.GetFileName(physicalPath), contentType),
                "File bukti verifikasi berhasil ditemukan.");
        }

        private IQueryable<TrxEmployeeProfileChangeRequest> BuildDetailQuery()
        {
            return _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .Include(x => x.Verifications.Where(v => !v.IsDelete))
                    .ThenInclude(x => x.ProfileChangeDetail)
                .Where(x => !x.IsDelete);
        }

        private async Task<EmployeeProfileChangeServiceResult<object>> ValidateHeaderAndDetailsAsync(
            Guid workforceProfileId,
            Guid? workflowDefinitionId,
            Guid? requestReasonId,
            string requestCategory,
            IReadOnlyCollection<CreateEmployeeProfileChangeDetailRequest> details,
            CancellationToken cancellationToken)
        {
            if (workforceProfileId == Guid.Empty)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workforce profile wajib dipilih.");
            }

            var workforceExists = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (!workforceExists)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan atau sudah tidak aktif.");
            }

            var employeeExists = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (!employeeExists)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workforce profile belum terhubung dengan employee aktif.");
            }

            var normalizedCategory = NormalizeCategory(requestCategory);

            if (!RequestCategories.Contains(normalizedCategory, StringComparer.OrdinalIgnoreCase))
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Request category tidak valid.");
            }

            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty)
            {
                var workflowExists = await _dbContext.Set<MstWorkflowDefinition>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == workflowDefinitionId.Value &&
                        x.IsActive &&
                        !x.IsDelete &&
                        x.WorkflowStatus == "Active",
                        cancellationToken);

                if (!workflowExists)
                {
                    return EmployeeProfileChangeServiceResult<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Workflow definition tidak ditemukan atau belum aktif.");
                }
            }

            if (requestReasonId.HasValue && requestReasonId.Value != Guid.Empty)
            {
                var reasonExists = await _dbContext.Set<MstRequestReason>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == requestReasonId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (!reasonExists)
                {
                    return EmployeeProfileChangeServiceResult<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Request reason tidak ditemukan atau sudah tidak aktif.");
                }
            }

            if (details == null || details.Count == 0)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Minimal satu detail perubahan wajib diisi.");
            }

            var duplicateFields = details
                .GroupBy(x => BuildFieldKey(x.TargetEntityName, x.FieldName), StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicateFields.Count > 0)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Field perubahan tidak boleh duplikat: {string.Join(", ", duplicateFields)}.");
            }

            return EmployeeProfileChangeServiceResult<object>.Ok(null, "Validasi berhasil.");
        }

        private async Task<EmployeeProfileChangeServiceResult<List<TrxEmployeeProfileChangeDetail>>> BuildDetailEntitiesAsync(
            Guid requestId,
            Guid workforceProfileId,
            IReadOnlyCollection<CreateEmployeeProfileChangeDetailRequest> requests,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var workforceProfile = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            var employee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (workforceProfile == null || employee == null)
            {
                return EmployeeProfileChangeServiceResult<List<TrxEmployeeProfileChangeDetail>>.Fail(
                    StatusCodes.Status409Conflict,
                    "Workforce profile atau employee tidak ditemukan.");
            }

            var result = new List<TrxEmployeeProfileChangeDetail>();
            var index = 0;

            foreach (var request in requests.OrderBy(x => x.SortOrder))
            {
                index++;

                var targetResult = ResolveTarget(
                    workforceProfile,
                    employee,
                    request.TargetEntityName,
                    request.TargetEntityId);

                if (!targetResult.Success || targetResult.Target == null)
                {
                    return EmployeeProfileChangeServiceResult<List<TrxEmployeeProfileChangeDetail>>.Fail(
                        targetResult.StatusCode,
                        targetResult.Message);
                }

                var fieldKey = BuildFieldKey(targetResult.TargetName, request.FieldName);

                if (!AllowedFields.TryGetValue(fieldKey, out var definition))
                {
                    return EmployeeProfileChangeServiceResult<List<TrxEmployeeProfileChangeDetail>>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Field {targetResult.TargetName}.{request.FieldName} tidak diizinkan untuk profile change.");
                }

                var property = targetResult.Target.GetType().GetProperty(
                    definition.FieldName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property == null || !property.CanWrite)
                {
                    return EmployeeProfileChangeServiceResult<List<TrxEmployeeProfileChangeDetail>>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Field {definition.FieldName} tidak tersedia pada entity target.");
                }

                var conversion = ConvertAndValidatePropertyValue(
                    targetResult.Target,
                    property,
                    request.NewValue);

                if (!conversion.Success)
                {
                    return EmployeeProfileChangeServiceResult<List<TrxEmployeeProfileChangeDetail>>.Fail(
                        conversion.StatusCode,
                        conversion.Message);
                }

                var foreignKeyValidation = await ValidateForeignKeyValueAsync(
                    targetResult.Target.GetType(),
                    property.Name,
                    conversion.ConvertedValue,
                    cancellationToken);

                if (!foreignKeyValidation.Success)
                {
                    return EmployeeProfileChangeServiceResult<List<TrxEmployeeProfileChangeDetail>>.Fail(
                        foreignKeyValidation.StatusCode,
                        foreignKeyValidation.Message);
                }

                var uniqueness = await ValidateUniqueEmployeeValueAsync(
                    employee,
                    targetResult.TargetName,
                    property.Name,
                    conversion.ConvertedValue,
                    cancellationToken);

                if (!uniqueness.Success)
                {
                    return EmployeeProfileChangeServiceResult<List<TrxEmployeeProfileChangeDetail>>.Fail(
                        uniqueness.StatusCode,
                        uniqueness.Message);
                }

                result.Add(new TrxEmployeeProfileChangeDetail
                {
                    Id = Guid.NewGuid(),
                    ProfileChangeRequestId = requestId,
                    FieldGroup = string.IsNullOrWhiteSpace(request.FieldGroup)
                        ? definition.FieldGroup
                        : request.FieldGroup.Trim(),
                    FieldName = property.Name,
                    OldValue = SerializeValue(property.GetValue(targetResult.Target)),
                    NewValue = SerializeValue(conversion.ConvertedValue),
                    ValueType = ResolveValueType(property.PropertyType),
                    TargetEntityName = targetResult.TargetName,
                    TargetEntityId = targetResult.TargetId,
                    RequiresVerification = request.RequiresVerification,
                    DetailStatus = "Pending",
                    SortOrder = request.SortOrder > 0 ? request.SortOrder : index,
                    Description = NormalizeText(request.Description),
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                });
            }

            return EmployeeProfileChangeServiceResult<List<TrxEmployeeProfileChangeDetail>>.Ok(
                result,
                "Detail perubahan berhasil dibentuk.");
        }

        private async Task<EmployeeProfileChangeServiceResult<object>> RefreshAndValidateDetailOldValuesAsync(
            Guid workforceProfileId,
            IReadOnlyCollection<TrxEmployeeProfileChangeDetail> details,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var workforceProfile = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete, cancellationToken);

            var employee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (workforceProfile == null || employee == null)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Workforce profile atau employee tidak ditemukan.");
            }

            foreach (var detail in details)
            {
                var targetResult = ResolveTarget(
                    workforceProfile,
                    employee,
                    detail.TargetEntityName,
                    detail.TargetEntityId);

                if (!targetResult.Success || targetResult.Target == null)
                {
                    return EmployeeProfileChangeServiceResult<object>.Fail(
                        targetResult.StatusCode,
                        targetResult.Message);
                }

                var fieldKey = BuildFieldKey(targetResult.TargetName, detail.FieldName);

                if (!AllowedFields.TryGetValue(fieldKey, out var definition))
                {
                    return EmployeeProfileChangeServiceResult<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Field {targetResult.TargetName}.{detail.FieldName} tidak diizinkan.");
                }

                var property = targetResult.Target.GetType().GetProperty(
                    definition.FieldName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property == null)
                {
                    return EmployeeProfileChangeServiceResult<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Field {detail.FieldName} tidak ditemukan.");
                }

                var conversion = ConvertAndValidatePropertyValue(
                    targetResult.Target,
                    property,
                    detail.NewValue);

                if (!conversion.Success)
                {
                    return EmployeeProfileChangeServiceResult<object>.Fail(
                        conversion.StatusCode,
                        conversion.Message);
                }

                detail.OldValue = SerializeValue(property.GetValue(targetResult.Target));
                detail.NewValue = SerializeValue(conversion.ConvertedValue);
                detail.ValueType = ResolveValueType(property.PropertyType);
                detail.TargetEntityName = targetResult.TargetName;
                detail.TargetEntityId = targetResult.TargetId;
                detail.UpdateDateTime = DateTime.UtcNow;
                detail.UpdateBy = actorUserId;
            }

            return EmployeeProfileChangeServiceResult<object>.Ok(null, "Detail valid.");
        }

        private async Task<EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>> BuildApplyPlanAsync(
            TrxEmployeeProfileChangeRequest request,
            IReadOnlyCollection<TrxEmployeeProfileChangeDetail> details,
            MstWorkforceProfile workforceProfile,
            MstEmployee employee,
            bool enforceOldValueMatch,
            CancellationToken cancellationToken)
        {
            var plan = new List<ApplyPlanItem>();

            foreach (var detail in details)
            {
                if (detail.RequiresVerification && detail.DetailStatus != "Verified")
                {
                    return EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Detail {detail.FieldName} belum berstatus Verified.");
                }

                var targetResult = ResolveTarget(
                    workforceProfile,
                    employee,
                    detail.TargetEntityName,
                    detail.TargetEntityId);

                if (!targetResult.Success || targetResult.Target == null)
                {
                    return EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>.Fail(
                        targetResult.StatusCode,
                        targetResult.Message);
                }

                var fieldKey = BuildFieldKey(targetResult.TargetName, detail.FieldName);

                if (!AllowedFields.TryGetValue(fieldKey, out var definition))
                {
                    return EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Field {targetResult.TargetName}.{detail.FieldName} tidak diizinkan.");
                }

                var property = targetResult.Target.GetType().GetProperty(
                    definition.FieldName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property == null || !property.CanWrite)
                {
                    return EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Field {detail.FieldName} tidak dapat diterapkan.");
                }

                var currentValue = SerializeValue(property.GetValue(targetResult.Target));

                if (enforceOldValueMatch && !AreSerializedValuesEqual(currentValue, detail.OldValue))
                {
                    return EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Nilai field {detail.FieldName} telah berubah sejak permintaan dibuat. Lakukan revisi sebelum apply.");
                }

                var conversion = ConvertAndValidatePropertyValue(
                    targetResult.Target,
                    property,
                    detail.NewValue);

                if (!conversion.Success)
                {
                    return EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>.Fail(
                        conversion.StatusCode,
                        conversion.Message);
                }

                var foreignKeyValidation = await ValidateForeignKeyValueAsync(
                    targetResult.Target.GetType(),
                    property.Name,
                    conversion.ConvertedValue,
                    cancellationToken);

                if (!foreignKeyValidation.Success)
                {
                    return EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>.Fail(
                        foreignKeyValidation.StatusCode,
                        foreignKeyValidation.Message);
                }

                var uniqueness = await ValidateUniqueEmployeeValueAsync(
                    employee,
                    targetResult.TargetName,
                    property.Name,
                    conversion.ConvertedValue,
                    cancellationToken);

                if (!uniqueness.Success)
                {
                    return EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>.Fail(
                        uniqueness.StatusCode,
                        uniqueness.Message);
                }

                plan.Add(new ApplyPlanItem(
                    targetResult.Target,
                    targetResult.TargetName,
                    property,
                    conversion.ConvertedValue));
            }

            return EmployeeProfileChangeServiceResult<List<ApplyPlanItem>>.Ok(
                plan,
                "Rencana apply valid.");
        }

        private async Task<EmployeeProfileChangeServiceResult<object>> ValidateForeignKeyValueAsync(
            Type targetEntityType,
            string propertyName,
            object? convertedValue,
            CancellationToken cancellationToken)
        {
            if (convertedValue is not Guid guidValue || guidValue == Guid.Empty)
                return EmployeeProfileChangeServiceResult<object>.Ok(null, "Valid.");

            var entityType = _dbContext.Model.FindEntityType(targetEntityType);
            var foreignKey = entityType?.GetForeignKeys()
                .FirstOrDefault(x => x.Properties.Any(p =>
                    p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)));

            if (foreignKey == null)
                return EmployeeProfileChangeServiceResult<object>.Ok(null, "Valid.");

            var principalType = foreignKey.PrincipalEntityType.ClrType;
            var principal = await _dbContext.FindAsync(
                principalType,
                new object?[] { guidValue },
                cancellationToken);

            if (principal == null)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Referensi untuk field {propertyName} tidak ditemukan.");
            }

            var isDeleteProperty = principalType.GetProperty("IsDelete");
            var isActiveProperty = principalType.GetProperty("IsActive");

            if (isDeleteProperty?.GetValue(principal) is bool isDelete && isDelete)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Referensi untuk field {propertyName} sudah dihapus.");
            }

            if (isActiveProperty?.GetValue(principal) is bool isActive && !isActive)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Referensi untuk field {propertyName} sudah tidak aktif.");
            }

            return EmployeeProfileChangeServiceResult<object>.Ok(null, "Valid.");
        }

        private async Task<EmployeeProfileChangeServiceResult<object>> ValidateUniqueEmployeeValueAsync(
            MstEmployee employee,
            string targetEntityName,
            string fieldName,
            object? convertedValue,
            CancellationToken cancellationToken)
        {
            if (convertedValue == null)
                return EmployeeProfileChangeServiceResult<object>.Ok(null, "Valid.");

            if (targetEntityName.Equals("MstEmployee", StringComparison.OrdinalIgnoreCase))
            {
                if (fieldName.Equals("IdentityNumber", StringComparison.OrdinalIgnoreCase))
                {
                    var value = convertedValue.ToString() ?? string.Empty;
                    var exists = await _dbContext.Set<MstEmployee>()
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.Id != employee.Id &&
                            x.IdentityNumber == value &&
                            !x.IsDelete,
                            cancellationToken);

                    if (exists)
                    {
                        return EmployeeProfileChangeServiceResult<object>.Fail(
                            StatusCodes.Status409Conflict,
                            "Nomor identitas sudah digunakan employee lain.");
                    }
                }

                if (fieldName.Equals("Email", StringComparison.OrdinalIgnoreCase))
                {
                    var value = convertedValue.ToString()?.Trim().ToLowerInvariant() ?? string.Empty;
                    var exists = await _dbContext.Set<MstEmployee>()
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.Id != employee.Id &&
                            x.Email.ToLower() == value &&
                            !x.IsDelete,
                            cancellationToken);

                    if (exists)
                    {
                        return EmployeeProfileChangeServiceResult<object>.Fail(
                            StatusCodes.Status409Conflict,
                            "Email sudah digunakan employee lain.");
                    }
                }
            }

            return EmployeeProfileChangeServiceResult<object>.Ok(null, "Valid.");
        }

        private static ConversionResult ConvertAndValidatePropertyValue(
            object target,
            PropertyInfo property,
            string? rawValue)
        {
            object? convertedValue;

            try
            {
                convertedValue = ConvertStringValue(rawValue, property.PropertyType);
            }
            catch (Exception ex)
            {
                return ConversionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Nilai field {property.Name} tidak valid: {ex.Message}");
            }

            var validationContext = new ValidationContext(target)
            {
                MemberName = property.Name
            };

            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateProperty(
                    convertedValue,
                    validationContext,
                    validationResults))
            {
                return ConversionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    validationResults.FirstOrDefault()?.ErrorMessage ??
                    $"Nilai field {property.Name} tidak valid.");
            }

            return ConversionResult.Ok(convertedValue);
        }

        private static object? ConvertStringValue(string? rawValue, Type propertyType)
        {
            var nullableType = Nullable.GetUnderlyingType(propertyType);
            var targetType = nullableType ?? propertyType;
            var isNullable = nullableType != null || !propertyType.IsValueType;

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                if (targetType == typeof(string))
                    return isNullable ? null : string.Empty;

                if (isNullable)
                    return null;

                throw new InvalidOperationException("Nilai wajib diisi.");
            }

            var value = rawValue.Trim();

            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(Guid))
                return Guid.Parse(value);

            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParseExact(
                        value,
                        new[] { "yyyy-MM-dd", "O", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ" },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var date))
                {
                    return date;
                }

                return DateTime.Parse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            }

            if (targetType == typeof(bool))
                return bool.Parse(value);

            if (targetType == typeof(int))
                return int.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(long))
                return long.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(decimal))
                return decimal.Parse(value, CultureInfo.InvariantCulture);

            if (targetType == typeof(double))
                return double.Parse(value, CultureInfo.InvariantCulture);

            if (targetType.IsEnum)
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var enumNumber))
                    return Enum.ToObject(targetType, enumNumber);

                return Enum.Parse(targetType, value, true);
            }

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private static string? SerializeValue(object? value)
        {
            if (value == null)
                return null;

            return value switch
            {
                DateTime date => date.TimeOfDay == TimeSpan.Zero
                    ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : date.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                Guid guid => guid.ToString(),
                bool boolean => boolean ? "true" : "false",
                decimal number => number.ToString(CultureInfo.InvariantCulture),
                double number => number.ToString(CultureInfo.InvariantCulture),
                float number => number.ToString(CultureInfo.InvariantCulture),
                Enum enumValue => enumValue.ToString(),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture)
            };
        }

        private static bool AreSerializedValuesEqual(string? left, string? right)
        {
            return string.Equals(
                left?.Trim(),
                right?.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private static TargetResolution ResolveTarget(
            MstWorkforceProfile workforceProfile,
            MstEmployee employee,
            string? targetEntityName,
            Guid? targetEntityId)
        {
            var normalizedName = NormalizeTargetEntityName(targetEntityName);

            if (normalizedName == "MstWorkforceProfile")
            {
                if (targetEntityId.HasValue &&
                    targetEntityId.Value != Guid.Empty &&
                    targetEntityId.Value != workforceProfile.Id)
                {
                    return TargetResolution.Fail(
                        StatusCodes.Status400BadRequest,
                        "TargetEntityId tidak sesuai dengan workforce profile permintaan.");
                }

                return TargetResolution.Ok(
                    workforceProfile,
                    "MstWorkforceProfile",
                    workforceProfile.Id);
            }

            if (normalizedName == "MstEmployee")
            {
                if (targetEntityId.HasValue &&
                    targetEntityId.Value != Guid.Empty &&
                    targetEntityId.Value != employee.Id)
                {
                    return TargetResolution.Fail(
                        StatusCodes.Status400BadRequest,
                        "TargetEntityId tidak sesuai dengan employee permintaan.");
                }

                return TargetResolution.Ok(employee, "MstEmployee", employee.Id);
            }

            return TargetResolution.Fail(
                StatusCodes.Status400BadRequest,
                "Target entity hanya mendukung MstWorkforceProfile atau MstEmployee.");
        }

        private static void SynchronizeWorkforceProfile(
            MstEmployee employee,
            MstWorkforceProfile workforceProfile,
            IReadOnlyCollection<ApplyPlanItem> plan)
        {
            var changedEmployeeFields = plan
                .Where(x => x.TargetName == "MstEmployee")
                .Select(x => x.Property.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (changedEmployeeFields.Contains("FullName"))
                workforceProfile.DisplayName = employee.FullName;

            if (changedEmployeeFields.Contains("Email"))
                workforceProfile.Email = employee.Email;

            if (changedEmployeeFields.Contains("PhoneNumber"))
                workforceProfile.PhoneNumber = employee.PhoneNumber;

            if (changedEmployeeFields.Contains("WhatsAppNumber"))
                workforceProfile.WhatsAppNumber = employee.WhatsAppNumber;
        }

        private static string ResolveVerificationType(string? fieldGroup)
        {
            return fieldGroup?.Trim().ToLowerInvariant() switch
            {
                "identity" => "Identity",
                "document" => "Document",
                _ => "HR"
            };
        }

        private EmployeeProfileChangeResponse MapResponse(
            TrxEmployeeProfileChangeRequest entity,
            IReadOnlyDictionary<Guid, string> actorNames)
        {
            var response = new EmployeeProfileChangeResponse
            {
                Id = entity.Id,
                RequestNumber = entity.RequestNumber,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                WorkflowDefinitionId = entity.WorkflowDefinitionId,
                RequestReasonId = entity.RequestReasonId,
                RequestCategory = entity.RequestCategory,
                RequestStatus = entity.RequestStatus,
                RequestReasonText = entity.RequestReasonText,
                RequestedByUserId = entity.RequestedByUserId,
                RequestedByUserName = GetActorName(actorNames, entity.RequestedByUserId),
                SubmittedAt = entity.SubmittedAt,
                ApprovedAt = entity.ApprovedAt,
                ApprovedByUserId = entity.ApprovedByUserId,
                ApprovedByUserName = GetActorName(actorNames, entity.ApprovedByUserId),
                RejectedAt = entity.RejectedAt,
                RejectedByUserId = entity.RejectedByUserId,
                RejectedByUserName = GetActorName(actorNames, entity.RejectedByUserId),
                AppliedAt = entity.AppliedAt,
                AppliedByUserId = entity.AppliedByUserId,
                AppliedByUserName = GetActorName(actorNames, entity.AppliedByUserId),
                CurrentStepOrder = entity.CurrentStepOrder,
                DetailCount = entity.Details.Count(x => !x.IsDelete),
                PendingVerificationCount = entity.Verifications.Count(x =>
                    !x.IsDelete &&
                    x.VerificationStatus == "Pending"),
                VerifiedVerificationCount = entity.Verifications.Count(x =>
                    !x.IsDelete &&
                    x.VerificationStatus == "Verified"),
                Description = entity.Description,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy),
                Details = entity.Details
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.FieldName)
                    .Select(x => new EmployeeProfileChangeDetailResponse
                    {
                        Id = x.Id,
                        FieldGroup = x.FieldGroup,
                        FieldName = x.FieldName,
                        OldValue = x.OldValue,
                        NewValue = x.NewValue,
                        ValueType = x.ValueType,
                        TargetEntityName = x.TargetEntityName,
                        TargetEntityId = x.TargetEntityId,
                        RequiresVerification = x.RequiresVerification,
                        DetailStatus = x.DetailStatus,
                        SortOrder = x.SortOrder,
                        Description = x.Description
                    })
                    .ToList(),
                Verifications = entity.Verifications
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.IsFinalVerification)
                    .ThenBy(x => x.CreateDateTime)
                    .Select(x => new EmployeeProfileChangeVerificationResponse
                    {
                        Id = x.Id,
                        ProfileChangeRequestId = x.ProfileChangeRequestId,
                        ProfileChangeDetailId = x.ProfileChangeDetailId,
                        DetailFieldName = x.ProfileChangeDetail?.FieldName,
                        VerificationType = x.VerificationType,
                        VerificationStatus = x.VerificationStatus,
                        VerifiedByUserId = x.VerifiedByUserId,
                        VerifiedByUserName = GetActorName(actorNames, x.VerifiedByUserId),
                        VerifiedAt = x.VerifiedAt,
                        IsFinalVerification = x.IsFinalVerification,
                        VerificationNote = x.VerificationNote,
                        EvidenceFilePath = x.EvidenceFilePath,
                        EvidenceFileName = string.IsNullOrWhiteSpace(x.EvidenceFilePath)
                            ? null
                            : Path.GetFileName(x.EvidenceFilePath),
                        EvidenceDownloadUrl = string.IsNullOrWhiteSpace(x.EvidenceFilePath)
                            ? null
                            : $"/api/v1/corporate/human-resource/employee-profile-changes/{entity.Id}/verifications/{x.Id}/evidence"
                    })
                    .ToList()
            };

            return response;
        }

        private static IOrderedQueryable<TrxEmployeeProfileChangeRequest> ApplySorting(
            IQueryable<TrxEmployeeProfileChangeRequest> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "requestnumber" => desc
                    ? query.OrderByDescending(x => x.RequestNumber)
                    : query.OrderBy(x => x.RequestNumber),
                "workforcedisplayname" => desc
                    ? query.OrderByDescending(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty)
                    : query.OrderBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty),
                "requeststatus" => desc
                    ? query.OrderByDescending(x => x.RequestStatus).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.RequestStatus).ThenByDescending(x => x.CreateDateTime),
                "submittedat" => desc
                    ? query.OrderByDescending(x => x.SubmittedAt)
                    : query.OrderBy(x => x.SubmittedAt),
                "approvedat" => desc
                    ? query.OrderByDescending(x => x.ApprovedAt)
                    : query.OrderBy(x => x.ApprovedAt),
                "appliedat" => desc
                    ? query.OrderByDescending(x => x.AppliedAt)
                    : query.OrderBy(x => x.AppliedAt),
                _ => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private async Task AcquireRequestNumberLockAsync(CancellationToken cancellationToken)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                $"SELECT pg_advisory_xact_lock(hashtext('{AdvisoryLockName}'))",
                cancellationToken);
        }

        private async Task<string> GenerateRequestNumberAsync(CancellationToken cancellationToken)
        {
            var dateCode = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefix = $"{RequestNumberPrefix}{dateCode}-";

            var existingNumbers = await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.RequestNumber.StartsWith(prefix))
                .Select(x => x.RequestNumber)
                .ToListAsync(cancellationToken);

            var maxNumber = existingNumbers
                .Select(x => x[prefix.Length..])
                .Select(x => int.TryParse(x, out var value) ? value : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{maxNumber + 1:D4}";
        }

        private async Task<TrxEmployeeProfileChangeRequest?> FindRequestForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxEmployeeProfileChangeRequest>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        }

        private static void SoftDeleteChildren<T>(
            IEnumerable<T> entities,
            Guid actorUserId,
            DateTime now)
            where T : IdentityModel
        {
            foreach (var entity in entities.Where(x => !x.IsDelete))
            {
                entity.IsDelete = true;
                entity.DeleteDateTime = now;
                entity.DeleteBy = actorUserId;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
            }
        }

        private async Task<Dictionary<Guid, string>> BuildActorNameMapAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken)
        {
            var validIds = ids
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (validIds.Count == 0)
                return new Dictionary<Guid, string>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => validIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode,
                    cancellationToken);
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string> actorNames,
            Guid? actorUserId)
        {
            if (!actorUserId.HasValue || actorUserId.Value == Guid.Empty)
                return null;

            return actorNames.GetValueOrDefault(actorUserId.Value);
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string> actorNames,
            Guid actorUserId)
        {
            return actorUserId == Guid.Empty
                ? null
                : actorNames.GetValueOrDefault(actorUserId);
        }

        private EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse> NotFoundResult()
        {
            return EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>.Fail(
                StatusCodes.Status404NotFound,
                "Permintaan perubahan profil tidak ditemukan.");
        }

        private static string BuildFieldKey(string? targetEntityName, string? fieldName)
        {
            return $"{NormalizeTargetEntityName(targetEntityName)}::{fieldName?.Trim()}";
        }

        private static string NormalizeTargetEntityName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim();

            if (normalized.Equals("MstWorkforceProfile", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("WorkforceProfile", StringComparison.OrdinalIgnoreCase))
            {
                return "MstWorkforceProfile";
            }

            if (normalized.Equals("MstEmployee", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                return "MstEmployee";
            }

            return normalized;
        }

        private static string NormalizeStatus(string value)
        {
            var normalized = value.Trim();

            return RequestStatuses.FirstOrDefault(x =>
                       x.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                   ?? normalized;
        }

        private static string NormalizeCategory(string value)
        {
            var normalized = value.Trim();

            return RequestCategories.FirstOrDefault(x =>
                       x.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                   ?? normalized;
        }

        private static string NormalizeVerificationStatus(string value)
        {
            var normalized = value.Trim();

            return VerificationStatuses.FirstOrDefault(x =>
                       x.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                   ?? normalized;
        }

        private static string BuildStatusLabel(string value)
        {
            return value switch
            {
                "Draft" => "Draft",
                "Submitted" => "Diajukan",
                "UnderVerification" => "Dalam verifikasi",
                "NeedRevision" => "Perlu revisi",
                "Approved" => "Disetujui",
                "Rejected" => "Ditolak",
                "Cancelled" => "Dibatalkan",
                "Applied" => "Diterapkan",
                "Pending" => "Menunggu",
                "Verified" => "Terverifikasi",
                _ => value
            };
        }

        private static string BuildCategoryLabel(string value)
        {
            return value switch
            {
                "Profile" => "Profil",
                "PersonalData" => "Data pribadi",
                "Contact" => "Kontak",
                "Address" => "Alamat",
                "Identity" => "Identitas",
                "EmergencyContact" => "Kontak darurat",
                _ => value
            };
        }

        private static string ResolveValueType(Type propertyType)
        {
            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (targetType == typeof(string))
                return "String";
            if (targetType == typeof(Guid))
                return "Guid";
            if (targetType == typeof(DateTime))
                return "Date";
            if (targetType == typeof(bool))
                return "Boolean";
            if (targetType.IsEnum)
                return "Enum";
            if (targetType == typeof(decimal) || targetType == typeof(double) || targetType == typeof(float))
                return "Decimal";
            if (targetType == typeof(int) || targetType == typeof(long))
                return "Integer";

            return targetType.Name;
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? AppendNote(string? current, string? note, string prefix)
        {
            if (string.IsNullOrWhiteSpace(note))
                return current;

            var entry = $"[{prefix} {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] {note.Trim()}";

            if (string.IsNullOrWhiteSpace(current))
                return entry.Length <= 500 ? entry : entry[..500];

            var combined = $"{current.Trim()} | {entry}";
            return combined.Length <= 500 ? combined : combined[..500];
        }

        private static void NormalizePaging(ref int pageNumber, ref int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? period)
        {
            var today = DateTime.UtcNow.Date;
            var selected = period?.Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(selected) && selected != "custom")
            {
                return selected switch
                {
                    "today" => (today, today.AddDays(1)),
                    "last7days" => (today.AddDays(-6), today.AddDays(1)),
                    "last30days" => (today.AddDays(-29), today.AddDays(1)),
                    "thismonth" =>
                    (
                        new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)
                    ),
                    _ => (null, null)
                };
            }

            return
            (
                startDate.HasValue
                    ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                    : null,
                endDate.HasValue
                    ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc)
                    : null
            );
        }

        private EmployeeProfileChangeServiceResult<object> ValidateEvidenceFile(IFormFile? file)
        {
            if (file == null)
                return EmployeeProfileChangeServiceResult<object>.Ok(null, "Valid.");

            if (file.Length <= 0)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "File bukti kosong.");
            }

            if (file.Length > MaximumEvidenceFileSizeBytes)
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Ukuran file bukti maksimal 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedEvidenceExtensions.Contains(extension))
            {
                return EmployeeProfileChangeServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Format file bukti harus PDF, JPG, JPEG, atau PNG.");
            }

            return EmployeeProfileChangeServiceResult<object>.Ok(null, "Valid.");
        }

        private async Task<string> SaveEvidenceFileAsync(
            Guid workforceProfileId,
            Guid requestId,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var uploadRoot = ResolveUploadRoot();
            var relativeDirectory = Path.Combine(
                "human-resource",
                "workforce-core",
                "employee-profile-changes",
                workforceProfileId.ToString(),
                requestId.ToString(),
                "verification-evidence");

            var physicalDirectory = Path.Combine(uploadRoot, relativeDirectory);
            Directory.CreateDirectory(physicalDirectory);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storedName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(physicalDirectory, storedName);

            await using var stream = new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

            await file.CopyToAsync(stream, cancellationToken);

            var publicRequestPath = _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";
            publicRequestPath = "/" + publicRequestPath.Trim().Trim('/');

            return $"{publicRequestPath}/{relativeDirectory.Replace("\\", "/")}/{storedName}";
        }

        private string ResolveUploadRoot()
        {
            var uploadRoot = _configuration["FileStorage:UploadRootPath"];

            if (string.IsNullOrWhiteSpace(uploadRoot))
                uploadRoot = Path.Combine(_environment.ContentRootPath, "uploads");

            if (!Path.IsPathRooted(uploadRoot))
                uploadRoot = Path.Combine(_environment.ContentRootPath, uploadRoot);

            return uploadRoot;
        }

        private string ResolvePhysicalPath(string storedPath)
        {
            var uploadRoot = ResolveUploadRoot();
            var publicRequestPath = _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";
            var normalizedRequestPath = "/" + publicRequestPath.Trim().Trim('/');
            var relativePath = storedPath.Replace("\\", "/");

            if (relativePath.StartsWith(normalizedRequestPath, StringComparison.OrdinalIgnoreCase))
                relativePath = relativePath[normalizedRequestPath.Length..];

            return Path.Combine(
                uploadRoot,
                relativePath
                    .TrimStart('/')
                    .Replace("/", Path.DirectorySeparatorChar.ToString()));
        }

        private void DeletePhysicalFile(string? storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                return;

            try
            {
                var physicalPath = ResolvePhysicalPath(storedPath);

                if (File.Exists(physicalPath))
                    File.Delete(physicalPath);
            }
            catch
            {
                // File cleanup does not cancel the database transaction.
            }
        }

        private sealed record AllowedFieldDefinition(
            string TargetEntityName,
            string FieldGroup,
            string FieldName,
            string Label,
            string ValueType,
            bool RequiresVerificationDefault);

        private sealed record ApplyPlanItem(
            object Target,
            string TargetName,
            PropertyInfo Property,
            object? ConvertedValue);

        private sealed class ConversionResult
        {
            public bool Success { get; private init; }
            public int StatusCode { get; private init; }
            public string Message { get; private init; } = string.Empty;
            public object? ConvertedValue { get; private init; }

            public static ConversionResult Ok(object? value) =>
                new()
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    ConvertedValue = value
                };

            public static ConversionResult Fail(int statusCode, string message) =>
                new()
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = message
                };
        }

        private sealed class TargetResolution
        {
            public bool Success { get; private init; }
            public int StatusCode { get; private init; }
            public string Message { get; private init; } = string.Empty;
            public object? Target { get; private init; }
            public string TargetName { get; private init; } = string.Empty;
            public Guid TargetId { get; private init; }

            public static TargetResolution Ok(object target, string targetName, Guid targetId) =>
                new()
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    Target = target,
                    TargetName = targetName,
                    TargetId = targetId
                };

            public static TargetResolution Fail(int statusCode, string message) =>
                new()
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = message
                };
        }
    }
}
