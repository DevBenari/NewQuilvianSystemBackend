using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_CREDENTIALING",
        moduleName: "Human Resource Credentialing",
        displayName: "Workforce Certification",
        AreaName = "Corporate",
        ControllerName = "WorkforceCertification",
        Description = "Corporate human resource workforce certification",
        SortOrder = 1)]
    [Tags("Corporate / Human Resource / Credentialing Management / Certification")]
    public class WfpCertificationController : ControllerBase
    {
        private static readonly HashSet<string> AllowedVerificationStatuses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Pending", "Verified", "Rejected", "Expired"
            };

        private const string LogCategory = "Corporate.HumanResource.Credentialing";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly WfpCertificationFileStorageService _fileStorage;

        public WfpCertificationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            WfpCertificationFileStorageService fileStorage)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _fileStorage = fileStorage;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpCertificationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Melihat metadata filter sertifikasi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> GetFilterMetadata(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var typeOptions = await _dbContext.Set<MstCertificationType>()
                .AsNoTracking()
                .Include(x => x.Profession)
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.CertificationTypeName)
                .Take(500)
                .Select(x => new WfpCertificationTypeOptionResponse
                {
                    Id = x.Id,
                    Code = x.CertificationTypeCode,
                    Name = x.CertificationTypeName,
                    ProfessionId = x.ProfessionId,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    IssuingAuthority = x.IssuingAuthority,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    IsRenewable = x.IsRenewable,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification,
                    Label = x.CertificationTypeCode + " - " + x.CertificationTypeName
                })
                .ToListAsync(cancellationToken);

            var result = new WfpCertificationFilterMetadataResponse
            {
                DefaultFilter = new WfpCertificationDefaultFilterResponse(),
                CertificationTypeOptions = typeOptions,
                VerificationStatusOptions = AllowedVerificationStatuses
                    .OrderBy(x => x)
                    .Select(x => new WfpCertificationStringOptionResponse
                    {
                        Value = x,
                        Label = BuildVerificationStatusLabel(x)
                    })
                    .ToList(),
                SortOptions = new List<WfpCertificationSortOptionResponse>
                {
                    new() { Value = "issueDate", Label = "Tanggal terbit" },
                    new() { Value = "expiredDate", Label = "Tanggal kedaluwarsa" },
                    new() { Value = "certificationType", Label = "Jenis sertifikasi" },
                    new() { Value = "certificationName", Label = "Nama sertifikasi" },
                    new() { Value = "verificationStatus", Label = "Status verifikasi" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpCertificationFilterMetadataResponse>.Ok(
                result,
                "Metadata filter sertifikasi workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpCertificationSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Melihat ringkasan sertifikasi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var today = DateTime.UtcNow.Date;
            var expiringLimit = today.AddDays(90);
            var query = _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpCertificationSummaryResponse
            {
                TotalCertification = await query.CountAsync(cancellationToken),
                ActiveCertification = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveCertification = await query.CountAsync(x => !x.IsActive, cancellationToken),
                VerifiedCertification = await query.CountAsync(x => x.IsVerified, cancellationToken),
                UnverifiedCertification = await query.CountAsync(x => !x.IsVerified, cancellationToken),
                LifetimeCertification = await query.CountAsync(x => x.IsLifetime, cancellationToken),
                ExpiredCertification = await query.CountAsync(x =>
                    !x.IsLifetime && x.ExpiredDate.HasValue && x.ExpiredDate.Value < today,
                    cancellationToken),
                ExpiringSoonCertification = await query.CountAsync(x =>
                    x.IsActive &&
                    !x.IsLifetime &&
                    x.ExpiredDate.HasValue &&
                    x.ExpiredDate.Value >= today &&
                    x.ExpiredDate.Value <= expiringLimit,
                    cancellationToken)
            };

            return Ok(ApiResponse<WfpCertificationSummaryResponse>.Ok(
                result,
                "Ringkasan sertifikasi workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpCertificationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Melihat data sertifikasi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> GetCertifications(
            Guid workforceProfileId,
            [FromQuery] Guid? certificationTypeId,
            [FromQuery] string? verificationStatus,
            [FromQuery] bool? isLifetime,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isExpired,
            [FromQuery] bool? isActive,
            [FromQuery] int? expiringWithinDays,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "issueDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(workforceProfileId),
                certificationTypeId,
                verificationStatus,
                isLifetime,
                isVerified,
                isExpired,
                isActive,
                expiringWithinDays,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var actorNames = await GetActorNameMapAsync(
                entities.SelectMany(x => new[] { x.CreateBy, x.VerifiedByUserId ?? Guid.Empty }),
                cancellationToken);

            var result = new PagedResult<WfpCertificationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<PagedResult<WfpCertificationResponse>>.Ok(
                result,
                "Data sertifikasi workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpCertificationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Melihat detail sertifikasi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> GetCertificationById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Sertifikasi workforce tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(
                new[] { entity.CreateBy, entity.UpdateBy, entity.VerifiedByUserId ?? Guid.Empty },
                cancellationToken);

            return Ok(ApiResponse<WfpCertificationDetailResponse>.Ok(
                MapDetailResponse(entity, actorNames),
                "Detail sertifikasi workforce berhasil diambil."));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WfpCertificationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Certification", Description = "Membuat sertifikasi workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceCertification", "Create")]
        public async Task<IActionResult> CreateCertification(
            Guid workforceProfileId,
            [FromForm] CreateWfpCertificationRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var validation = await ValidateRequestAsync(workforceProfileId, null, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var master = await FindCertificationTypeAsync(request.CertificationTypeId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var isVerified = request.IsVerified;

            StoredCertificationFile? storedFile = null;
            try
            {
                if (request.File != null)
                    storedFile = await _fileStorage.SaveAsync(request.File, cancellationToken);

                var entity = new WfpCertification
                {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CertificationTypeId = NormalizeNullableGuid(request.CertificationTypeId),
                CredentialingRequirementId = NormalizeNullableGuid(request.CredentialingRequirementId),
                RequirementCode = NormalizeNullableText(request.RequirementCode),
                CertificationType = master?.CertificationTypeName ?? request.CertificationType.Trim(),
                CertificationName = request.CertificationName.Trim(),
                Issuer = NormalizeNullableText(request.Issuer) ?? master?.IssuingAuthority,
                CertificateNumber = NormalizeNullableText(request.CertificateNumber),
                IssueDate = NormalizeUtcDate(request.IssueDate),
                ExpiredDate = request.IsLifetime ? null : NormalizeNullableUtcDate(request.ExpiredDate),
                IsLifetime = request.IsLifetime,
                FilePath = storedFile?.FilePath,
                FileContentType = storedFile?.ContentType,
                IsVerified = isVerified,
                VerificationStatus = isVerified
                    ? "Verified"
                    : NormalizeVerificationStatus(request.VerificationStatus),
                VerifiedAt = isVerified ? now : null,
                VerifiedByUserId = isVerified ? actorUserId : null,
                BlocksSchedulingWhenInvalid = request.BlocksSchedulingWhenInvalid,
                BlocksClinicalServiceWhenInvalid = request.BlocksClinicalServiceWhenInvalid,
                IsActive = request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
                };

                _dbContext.Set<WfpCertification>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCertification.CreateCertification",
                "Membuat sertifikasi workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.CertificationType, entity.CertificateNumber });

                return await GetCertificationById(workforceProfileId, entity.Id, cancellationToken);
            }
            catch (CertificationFileValidationException exception)
            {
                if (storedFile != null)
                    await _fileStorage.DeleteAsync(storedFile.FilePath, cancellationToken);
                return BadRequest(ApiResponse<object>.Fail(400, exception.Message));
            }
            catch
            {
                if (storedFile != null)
                    await _fileStorage.DeleteAsync(storedFile.FilePath, cancellationToken);
                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WfpCertificationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Certification", Description = "Mengubah sertifikasi workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCertification", "Update")]
        public async Task<IActionResult> UpdateCertification(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWfpCertificationRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Sertifikasi workforce tidak ditemukan."));

            var validation = await ValidateRequestAsync(workforceProfileId, id, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var master = await FindCertificationTypeAsync(request.CertificationTypeId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.CertificationTypeId = NormalizeNullableGuid(request.CertificationTypeId);
            entity.CredentialingRequirementId = NormalizeNullableGuid(request.CredentialingRequirementId);
            entity.RequirementCode = NormalizeNullableText(request.RequirementCode);
            entity.CertificationType = master?.CertificationTypeName ?? request.CertificationType.Trim();
            entity.CertificationName = request.CertificationName.Trim();
            entity.Issuer = NormalizeNullableText(request.Issuer) ?? master?.IssuingAuthority;
            entity.CertificateNumber = NormalizeNullableText(request.CertificateNumber);
            entity.IssueDate = NormalizeUtcDate(request.IssueDate);
            entity.ExpiredDate = request.IsLifetime ? null : NormalizeNullableUtcDate(request.ExpiredDate);
            entity.IsLifetime = request.IsLifetime;
            StoredCertificationFile? storedFile = null;
            var oldFilePath = entity.FilePath;
            try
            {
                if (request.File != null)
                {
                    storedFile = await _fileStorage.SaveAsync(request.File, cancellationToken);
                    entity.FilePath = storedFile.FilePath;
                    entity.FileContentType = storedFile.ContentType;
                }
                // Keep the existing file reference when only certification metadata changes.
            entity.IsVerified = request.IsVerified;
            entity.VerificationStatus = request.IsVerified
                ? "Verified"
                : NormalizeVerificationStatus(request.VerificationStatus);
            entity.VerifiedAt = request.IsVerified ? entity.VerifiedAt ?? now : null;
            entity.VerifiedByUserId = request.IsVerified ? entity.VerifiedByUserId ?? actorUserId : null;
            entity.BlocksSchedulingWhenInvalid = request.BlocksSchedulingWhenInvalid;
            entity.BlocksClinicalServiceWhenInvalid = request.BlocksClinicalServiceWhenInvalid;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                if (storedFile != null && !string.Equals(oldFilePath, storedFile.FilePath, StringComparison.Ordinal))
                    await _fileStorage.DeleteAsync(oldFilePath, cancellationToken);
                return await GetCertificationById(workforceProfileId, id, cancellationToken);
            }
            catch (CertificationFileValidationException exception)
            {
                if (storedFile != null)
                    await _fileStorage.DeleteAsync(storedFile.FilePath, cancellationToken);
                return BadRequest(ApiResponse<object>.Fail(400, exception.Message));
            }
            catch
            {
                if (storedFile != null)
                    await _fileStorage.DeleteAsync(storedFile.FilePath, cancellationToken);
                throw;
            }
        }

        [HttpGet("{id:guid}/file")]
        [AccessAction("Read", "Read Workforce Certification File", Description = "Mengunduh berkas sertifikasi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> GetCertificationFile(
            Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null || string.IsNullOrWhiteSpace(entity.FilePath))
                return NotFound(ApiResponse<object>.Fail(404, "Berkas sertifikasi tidak ditemukan."));

            var physicalPath = _fileStorage.GetPhysicalPath(entity.FilePath);
            if (!System.IO.File.Exists(physicalPath))
                return NotFound(ApiResponse<object>.Fail(404, "Berkas sertifikasi tidak ditemukan."));

            return PhysicalFile(physicalPath, entity.FileContentType ?? "application/octet-stream", "sertifikasi" + Path.GetExtension(physicalPath));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WfpCertificationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Certification", Description = "Mengubah status sertifikasi workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCertification", "Update")]
        public async Task<IActionResult> UpdateCertificationStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpCertificationStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Sertifikasi workforce tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetCertificationById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WfpCertificationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Workforce Certification", Description = "Memverifikasi sertifikasi workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCertification", "Update")]
        public async Task<IActionResult> VerifyCertification(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWfpCertificationRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Sertifikasi workforce tidak ditemukan."));

            if (!AllowedVerificationStatuses.Contains(request.VerificationStatus.Trim()))
                return BadRequest(ApiResponse<object>.Fail(400, "VerificationStatus tidak valid."));

            if (request.IsVerified && !entity.IsLifetime && entity.ExpiredDate.HasValue && entity.ExpiredDate.Value < DateTime.UtcNow.Date)
                return BadRequest(ApiResponse<object>.Fail(400, "Sertifikasi yang sudah kedaluwarsa tidak dapat diverifikasi sebagai valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = request.IsVerified;
            entity.VerificationStatus = request.IsVerified
                ? "Verified"
                : NormalizeVerificationStatus(request.VerificationStatus);
            entity.VerifiedAt = request.IsVerified ? now : null;
            entity.VerifiedByUserId = request.IsVerified ? actorUserId : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetCertificationById(workforceProfileId, id, cancellationToken);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Certification", Description = "Menghapus sertifikasi workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceCertification", "Delete")]
        public async Task<IActionResult> DeleteCertification(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Sertifikasi workforce tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _fileStorage.DeleteAsync(entity.FilePath, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCertification.DeleteCertification",
                "Menghapus sertifikasi workforce.",
                new { entity.Id, entity.WorkforceProfileId });

            return Ok(ApiResponse<object>.Ok(null, "Sertifikasi workforce berhasil dihapus."));
        }

        private IQueryable<WfpCertification> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.CertificationTypeMaster)
                    .ThenInclude(x => x!.Profession)
                .Include(x => x.VerifiedByUser)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpCertification> ApplyFilter(
            IQueryable<WfpCertification> query,
            Guid? certificationTypeId,
            string? verificationStatus,
            bool? isLifetime,
            bool? isVerified,
            bool? isExpired,
            bool? isActive,
            int? expiringWithinDays,
            string? search)
        {
            var today = DateTime.UtcNow.Date;

            if (certificationTypeId.HasValue && certificationTypeId.Value != Guid.Empty)
                query = query.Where(x => x.CertificationTypeId == certificationTypeId.Value);

            if (!string.IsNullOrWhiteSpace(verificationStatus))
            {
                var normalized = verificationStatus.Trim().ToLower();
                query = query.Where(x => x.VerificationStatus.ToLower() == normalized);
            }

            if (isLifetime.HasValue)
                query = query.Where(x => x.IsLifetime == isLifetime.Value);

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (isExpired.HasValue)
            {
                query = isExpired.Value
                    ? query.Where(x => !x.IsLifetime && x.ExpiredDate.HasValue && x.ExpiredDate.Value < today)
                    : query.Where(x => x.IsLifetime || !x.ExpiredDate.HasValue || x.ExpiredDate.Value >= today);
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (expiringWithinDays.HasValue && expiringWithinDays.Value >= 0)
            {
                var limit = today.AddDays(Math.Min(expiringWithinDays.Value, 3650));
                query = query.Where(x =>
                    !x.IsLifetime &&
                    x.ExpiredDate.HasValue &&
                    x.ExpiredDate.Value >= today &&
                    x.ExpiredDate.Value <= limit);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.CertificationType.ToLower().Contains(keyword) ||
                    x.CertificationName.ToLower().Contains(keyword) ||
                    (x.CertificateNumber != null && x.CertificateNumber.ToLower().Contains(keyword)) ||
                    (x.Issuer != null && x.Issuer.ToLower().Contains(keyword)) ||
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    (x.CertificationTypeMaster != null && x.CertificationTypeMaster.CertificationTypeCode.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpCertification> ApplySorting(
            IQueryable<WfpCertification> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "issueDate").Trim().ToLowerInvariant() switch
            {
                "expireddate" => desc ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate),
                "certificationtype" => desc ? query.OrderByDescending(x => x.CertificationType) : query.OrderBy(x => x.CertificationType),
                "certificationname" => desc ? query.OrderByDescending(x => x.CertificationName) : query.OrderBy(x => x.CertificationName),
                "verificationstatus" => desc ? query.OrderByDescending(x => x.VerificationStatus) : query.OrderBy(x => x.VerificationStatus),
                "isverified" => desc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.IssueDate) : query.OrderBy(x => x.IssueDate)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? currentId,
            CreateWfpCertificationRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CertificationType))
                return (false, "Jenis sertifikasi wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.CertificationName))
                return (false, "Nama sertifikasi wajib diisi.");

            if (request.IssueDate == default)
                return (false, "Tanggal terbit wajib diisi.");

            if (!AllowedVerificationStatuses.Contains(request.VerificationStatus.Trim()))
                return (false, "VerificationStatus tidak valid. Gunakan Pending, Verified, Rejected, atau Expired.");

            var master = await FindCertificationTypeAsync(request.CertificationTypeId, cancellationToken);
            if (request.CertificationTypeId.HasValue && master == null)
                return (false, "Master jenis sertifikasi tidak ditemukan atau tidak aktif.");

            if (request.IsLifetime && request.ExpiredDate.HasValue)
                return (false, "ExpiredDate harus kosong jika sertifikasi berlaku seumur hidup.");

            if (!request.IsLifetime && !request.ExpiredDate.HasValue && master?.RequiresExpiryDate == true)
                return (false, "Tanggal kedaluwarsa wajib diisi untuk jenis sertifikasi ini.");

            if (request.ExpiredDate.HasValue && request.ExpiredDate.Value.Date < request.IssueDate.Date)
                return (false, "Tanggal kedaluwarsa tidak boleh lebih kecil dari tanggal terbit.");

            if (request.IsVerified && master?.RequiresDocument == true && request.File == null)
            {
                var existingFile = currentId.HasValue && await _dbContext.Set<WfpCertification>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == currentId.Value && !string.IsNullOrWhiteSpace(x.FilePath), cancellationToken);
                if (!existingFile)
                    return (false, "Dokumen sertifikasi wajib tersedia sebelum data diverifikasi.");
            }

            var requirementId = NormalizeNullableGuid(request.CredentialingRequirementId);
            if (requirementId.HasValue)
            {
                var requirementExists = await _dbContext.Set<MstCredentialingRequirement>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == requirementId.Value && x.IsActive && !x.IsDelete, cancellationToken);

                if (!requirementExists)
                    return (false, "Credentialing requirement tidak ditemukan atau tidak aktif.");
            }

            var normalizedType = master?.CertificationTypeName ?? request.CertificationType.Trim();
            var normalizedNumber = NormalizeNullableText(request.CertificateNumber);
            if (normalizedNumber != null)
            {
                var duplicate = await _dbContext.Set<WfpCertification>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.CertificationType == normalizedType &&
                        x.CertificateNumber == normalizedNumber &&
                        x.Id != currentId &&
                        !x.IsDelete,
                        cancellationToken);

                if (duplicate)
                    return (false, "Jenis dan nomor sertifikat yang sama sudah tersedia untuk workforce ini.");
            }

            return (true, null);
        }

        private async Task<MstCertificationType?> FindCertificationTypeAsync(
            Guid? id,
            CancellationToken cancellationToken)
        {
            id = NormalizeNullableGuid(id);
            if (!id.HasValue)
                return null;

            return await _dbContext.Set<MstCertificationType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id.Value && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private async Task<WfpCertification?> FindEntityAsync(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);
        }

        private async Task<bool> WorkforceProfileExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return id != Guid.Empty && await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private IActionResult WorkforceProfileNotFound() =>
            NotFound(ApiResponse<object>.Fail(404, "Profil tenaga kerja tidak ditemukan."));

        private static WfpCertificationResponse MapResponse(
            WfpCertification x,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var today = DateTime.UtcNow.Date;
            var days = x.IsLifetime || !x.ExpiredDate.HasValue
                ? null
                : (int?)(x.ExpiredDate.Value.Date - today).TotalDays;

            return new WfpCertificationResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                CertificationTypeId = x.CertificationTypeId,
                CertificationTypeCode = x.CertificationTypeMaster?.CertificationTypeCode,
                CertificationTypeMasterName = x.CertificationTypeMaster?.CertificationTypeName,
                ProfessionId = x.CertificationTypeMaster?.ProfessionId,
                ProfessionCode = x.CertificationTypeMaster?.Profession?.ProfessionCode,
                ProfessionName = x.CertificationTypeMaster?.Profession?.ProfessionName,
                CredentialingRequirementId = x.CredentialingRequirementId,
                RequirementCode = x.RequirementCode,
                CertificationType = x.CertificationType,
                CertificationName = x.CertificationName,
                Issuer = x.Issuer,
                CertificateNumber = x.CertificateNumber,
                IssueDate = x.IssueDate,
                ExpiredDate = x.ExpiredDate,
                IsLifetime = x.IsLifetime,
                IsExpired = !x.IsLifetime && x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today,
                DaysUntilExpiry = days,
                // Do not expose the server-side storage key; clients use the authorized file endpoint.
                FilePath = null,
                FileUrl = string.IsNullOrWhiteSpace(x.FilePath)
                    ? null
                    : $"/api/v1/corporate/human-resource/workforce-profiles/{x.WorkforceProfileId}/certifications/{x.Id}/file",
                FileContentType = x.FileContentType,
                HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                IsVerified = x.IsVerified,
                VerificationStatus = x.VerificationStatus,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = GetActorName(actorNames, x.VerifiedByUserId ?? Guid.Empty),
                BlocksSchedulingWhenInvalid = x.BlocksSchedulingWhenInvalid,
                BlocksClinicalServiceWhenInvalid = x.BlocksClinicalServiceWhenInvalid,
                IsActive = x.IsActive,
                Description = x.Description,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy)
            };
        }

        private static WfpCertificationDetailResponse MapDetailResponse(
            WfpCertification x,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var value = MapResponse(x, actorNames);
            return new WfpCertificationDetailResponse
            {
                Id = value.Id,
                WorkforceProfileId = value.WorkforceProfileId,
                WorkforceProfileCode = value.WorkforceProfileCode,
                WorkforceDisplayName = value.WorkforceDisplayName,
                CertificationTypeId = value.CertificationTypeId,
                CertificationTypeCode = value.CertificationTypeCode,
                CertificationTypeMasterName = value.CertificationTypeMasterName,
                ProfessionId = value.ProfessionId,
                ProfessionCode = value.ProfessionCode,
                ProfessionName = value.ProfessionName,
                CredentialingRequirementId = value.CredentialingRequirementId,
                RequirementCode = value.RequirementCode,
                CertificationType = value.CertificationType,
                CertificationName = value.CertificationName,
                Issuer = value.Issuer,
                CertificateNumber = value.CertificateNumber,
                IssueDate = value.IssueDate,
                ExpiredDate = value.ExpiredDate,
                IsLifetime = value.IsLifetime,
                IsExpired = value.IsExpired,
                DaysUntilExpiry = value.DaysUntilExpiry,
                FilePath = value.FilePath,
                FileUrl = value.FileUrl,
                FileContentType = value.FileContentType,
                HasFile = value.HasFile,
                IsVerified = value.IsVerified,
                VerificationStatus = value.VerificationStatus,
                VerifiedAt = value.VerifiedAt,
                VerifiedByUserId = value.VerifiedByUserId,
                VerifiedByUserName = value.VerifiedByUserName,
                BlocksSchedulingWhenInvalid = value.BlocksSchedulingWhenInvalid,
                BlocksClinicalServiceWhenInvalid = value.BlocksClinicalServiceWhenInvalid,
                IsActive = value.IsActive,
                Description = value.Description,
                CreateDateTime = value.CreateDateTime,
                CreateBy = value.CreateBy,
                CreateByName = value.CreateByName,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            };
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds,
            CancellationToken cancellationToken)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId) =>
            actorId == Guid.Empty
                ? null
                : actorNames.TryGetValue(actorId, out var value) ? value : null;

        private Guid GetCurrentUserId()
        {
            var text = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(text, out var id) ? id : Guid.Empty;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private static string? NormalizeNullableText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static DateTime NormalizeUtcDate(DateTime value) =>
            DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

        private static DateTime? NormalizeNullableUtcDate(DateTime? value) =>
            value.HasValue ? NormalizeUtcDate(value.Value) : null;

        private static string NormalizeVerificationStatus(string value)
        {
            var normalized = AllowedVerificationStatuses.FirstOrDefault(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
            return normalized ?? value.Trim();
        }

        private static string BuildVerificationStatusLabel(string value) => value switch
        {
            "Pending" => "Menunggu Verifikasi",
            "Verified" => "Terverifikasi",
            "Rejected" => "Ditolak",
            "Expired" => "Kedaluwarsa",
            _ => value
        };
    }
}
