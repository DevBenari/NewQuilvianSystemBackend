using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using EmploymentHistoryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs.WfpEmploymentHistoryResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/employment-histories")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Employment History",
        AreaName = "Corporate",
        ControllerName = "WfpEmploymentHistory",
        Description = "Corporate human resource workforce employment history",
        SortOrder = 9)]
    [Tags("Corporate / Human Resource / Workforce Core / Employment History")]
    public class WfpEmploymentHistoryController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";
        private const int MaximumFileSizeBytes = 10 * 1024 * 1024;

        private static readonly string[] AllowedHistoryTypes =
        {
            "Join",
            "StatusChange",
            "Transfer",
            "Promotion",
            "Demotion",
            "Rotation",
            "ContractChange",
            "Separation"
        };

        private static readonly HashSet<string> AllowedFileExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".jpg", ".jpeg", ".png",
                ".doc", ".docx", ".xls", ".xlsx"
            };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public WfpEmploymentHistoryController(
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

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat metadata employment history", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmploymentHistory", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpEmploymentHistoryFilterMetadataResponse
            {
                DefaultFilter = new WfpEmploymentHistoryDefaultFilterResponse(),
                CustomPeriods = new List<WfpEmploymentHistoryStringOptionResponse>
                {
                    new() { Value = "custom", Label = "Custom" },
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "last30days", Label = "30 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" }
                },
                HistoryTypeOptions = AllowedHistoryTypes
                    .Select(x => new WfpEmploymentHistoryStringOptionResponse
                    {
                        Value = x,
                        Label = BuildHistoryTypeLabel(x)
                    })
                    .ToList(),
                SortOptions = new List<WfpEmploymentHistoryStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "effectiveDate", Label = "Tanggal efektif" },
                    new() { Value = "historyType", Label = "Tipe riwayat" },
                    new() { Value = "newDepartmentName", Label = "Department baru" },
                    new() { Value = "newPositionName", Label = "Posisi baru" },
                    new() { Value = "approvedAt", Label = "Tanggal approval" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                FileUploadInfo = new WfpEmploymentHistoryFileUploadInfoResponse()
            };

            return Ok(ApiResponse<WfpEmploymentHistoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter employment history berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat ringkasan employment history", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmploymentHistory", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId))
                return WorkforceProfileNotFound();

            var query = _dbContext.Set<WfpEmploymentHistory>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            var result = new WfpEmploymentHistorySummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                ApprovedData = await query.CountAsync(x =>
                    x.ApprovedAt.HasValue &&
                    x.ApprovedByUserId.HasValue),
                PendingApprovalData = await query.CountAsync(x =>
                    !x.ApprovedAt.HasValue ||
                    !x.ApprovedByUserId.HasValue),
                TransferData = await query.CountAsync(x => x.HistoryType == "Transfer"),
                PromotionData = await query.CountAsync(x => x.HistoryType == "Promotion"),
                SeparationData = await query.CountAsync(x => x.HistoryType == "Separation")
            };

            return Ok(ApiResponse<WfpEmploymentHistorySummaryResponse>.Ok(
                result,
                "Ringkasan employment history berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat employment history", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmploymentHistory", "Read")]
        public async Task<IActionResult> GetEmploymentHistories(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] string? historyType,
            [FromQuery] Guid? newDepartmentId,
            [FromQuery] Guid? newPositionId,
            [FromQuery] bool? isApproved,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId))
                return WorkforceProfileNotFound();

            NormalizePaging(ref pageNumber, ref pageSize);

            var query = BuildBaseQuery(workforceProfileId);
            var range = ResolveDateRange(startDate, endDate, period);

            if (range.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= range.Start.Value);

            if (range.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);

            if (!string.IsNullOrWhiteSpace(historyType))
                query = query.Where(x =>
                    x.HistoryType == NormalizeHistoryType(historyType));

            if (newDepartmentId.HasValue && newDepartmentId.Value != Guid.Empty)
                query = query.Where(x => x.NewDepartmentId == newDepartmentId.Value);

            if (newPositionId.HasValue && newPositionId.Value != Guid.Empty)
                query = query.Where(x => x.NewPositionId == newPositionId.Value);

            if (isApproved.HasValue)
            {
                query = isApproved.Value
                    ? query.Where(x =>
                        x.ApprovedAt.HasValue &&
                        x.ApprovedByUserId.HasValue)
                    : query.Where(x =>
                        !x.ApprovedAt.HasValue ||
                        !x.ApprovedByUserId.HasValue);
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.HistoryType.ToLower().Contains(keyword) ||
                    (x.OldStatus != null && x.OldStatus.ToLower().Contains(keyword)) ||
                    (x.NewStatus != null && x.NewStatus.ToLower().Contains(keyword)) ||
                    (x.Reason != null && x.Reason.ToLower().Contains(keyword)) ||
                    (x.ReferenceType != null && x.ReferenceType.ToLower().Contains(keyword)) ||
                    (x.OldDepartment != null && x.OldDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.NewDepartment != null && x.NewDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.OldPosition != null && x.OldPosition.PositionName.ToLower().Contains(keyword)) ||
                    (x.NewPosition != null && x.NewPosition.PositionName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await BuildActorNameMapAsync(
                entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy }));

            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<EmploymentHistoryPagedResult>.Ok(
                new EmploymentHistoryPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data employment history berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat pilihan employment history", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmploymentHistory", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] string? historyType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] int take = 100)
        {
            take = Math.Clamp(take, 1, 200);
            var query = BuildBaseQuery(workforceProfileId);

            if (!string.IsNullOrWhiteSpace(historyType))
                query = query.Where(x =>
                    x.HistoryType == NormalizeHistoryType(historyType));

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            var result = await query
                .OrderByDescending(x => x.EffectiveDate)
                .Take(take)
                .Select(x => new WfpEmploymentHistoryOptionResponse
                {
                    Id = x.Id,
                    HistoryType = x.HistoryType,
                    EffectiveDate = x.EffectiveDate,
                    NewDepartmentName = x.NewDepartment != null
                        ? x.NewDepartment.DepartmentName
                        : null,
                    NewPositionName = x.NewPosition != null
                        ? x.NewPosition.PositionName
                        : null,
                    NewStatus = x.NewStatus,
                    IsApproved =
                        x.ApprovedAt.HasValue &&
                        x.ApprovedByUserId.HasValue,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<WfpEmploymentHistoryOptionResponse>>.Ok(
                result,
                "Pilihan employment history berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat detail employment history", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmploymentHistory", "Read")]
        public async Task<IActionResult> GetById(Guid workforceProfileId, Guid id)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(
                    404,
                    "Employment history tidak ditemukan."));

            var actorNames = await BuildActorNameMapAsync(
                new[] { entity.CreateBy, entity.UpdateBy });

            return Ok(ApiResponse<WfpEmploymentHistoryResponse>.Ok(
                MapResponse(entity, actorNames),
                "Detail employment history berhasil diambil."));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [AccessAction("Create", "Create Workforce Employment History", Description = "Membuat employment history", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WfpEmploymentHistory", "Create")]
        public async Task<IActionResult> Create(
            Guid workforceProfileId,
            [FromForm] CreateWfpEmploymentHistoryRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId))
                return WorkforceProfileNotFound();

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                null,
                request);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    400,
                    validation.ErrorMessage!));

            var fileValidation = ValidateFile(request.File);

            if (!fileValidation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    400,
                    fileValidation.ErrorMessage!));

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            string? storedFilePath = null;

            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.File != null)
                {
                    storedFilePath = await SaveFileAsync(
                        workforceProfileId,
                        request.File,
                        cancellationToken);
                }

                var entity = new WfpEmploymentHistory
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    HistoryType = NormalizeHistoryType(request.HistoryType),
                    OldStatus = Normalize(request.OldStatus),
                    NewStatus = Normalize(request.NewStatus),
                    OldEmploymentStatusId = NormalizeGuid(request.OldEmploymentStatusId),
                    NewEmploymentStatusId = NormalizeGuid(request.NewEmploymentStatusId),
                    OldEmploymentTypeId = NormalizeGuid(request.OldEmploymentTypeId),
                    NewEmploymentTypeId = NormalizeGuid(request.NewEmploymentTypeId),
                    OldDepartmentId = NormalizeGuid(request.OldDepartmentId),
                    NewDepartmentId = NormalizeGuid(request.NewDepartmentId),
                    OldPositionId = NormalizeGuid(request.OldPositionId),
                    NewPositionId = NormalizeGuid(request.NewPositionId),
                    OldOrganizationUnitId = NormalizeGuid(request.OldOrganizationUnitId),
                    NewOrganizationUnitId = NormalizeGuid(request.NewOrganizationUnitId),
                    OldEmployeeGradeId = NormalizeGuid(request.OldEmployeeGradeId),
                    NewEmployeeGradeId = NormalizeGuid(request.NewEmployeeGradeId),
                    EffectiveDate = request.EffectiveDate.Date,
                    EndDate = request.EndDate?.Date,
                    Reason = Normalize(request.Reason),
                    ReferenceType = Normalize(request.ReferenceType),
                    ReferenceId = NormalizeGuid(request.ReferenceId),
                    FilePath = storedFilePath,
                    FileContentType = request.File?.ContentType,
                    Description = Normalize(request.Description),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpEmploymentHistory>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WfpEmploymentHistory.Create",
                    "Employment history berhasil dibuat.",
                    new
                    {
                        entity.Id,
                        entity.WorkforceProfileId,
                        entity.HistoryType,
                        entity.EffectiveDate
                    });

                return Ok(ApiResponse<object>.Ok(
                    new { entity.Id },
                    "Employment history berhasil dibuat."));
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(storedFilePath))
                    DeletePhysicalFile(storedFilePath);

                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [AccessAction("Update", "Update Workforce Employment History", Description = "Mengubah employment history", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpEmploymentHistory", "Update")]
        public async Task<IActionResult> Update(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWfpEmploymentHistoryRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(
                workforceProfileId,
                id,
                cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(
                    404,
                    "Employment history tidak ditemukan."));

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                id,
                request);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    400,
                    validation.ErrorMessage!));

            var fileValidation = ValidateFile(request.File);

            if (!fileValidation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    400,
                    fileValidation.ErrorMessage!));

            var oldFilePath = entity.FilePath;
            string? newFilePath = null;

            if (request.File != null)
            {
                if (!request.ReplaceExistingFile &&
                    !string.IsNullOrWhiteSpace(entity.FilePath))
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        400,
                        "File sudah tersedia. Aktifkan ReplaceExistingFile untuk mengganti file."));
                }

                newFilePath = await SaveFileAsync(
                    workforceProfileId,
                    request.File,
                    cancellationToken);
            }

            entity.HistoryType = NormalizeHistoryType(request.HistoryType);
            entity.OldStatus = Normalize(request.OldStatus);
            entity.NewStatus = Normalize(request.NewStatus);
            entity.OldEmploymentStatusId = NormalizeGuid(request.OldEmploymentStatusId);
            entity.NewEmploymentStatusId = NormalizeGuid(request.NewEmploymentStatusId);
            entity.OldEmploymentTypeId = NormalizeGuid(request.OldEmploymentTypeId);
            entity.NewEmploymentTypeId = NormalizeGuid(request.NewEmploymentTypeId);
            entity.OldDepartmentId = NormalizeGuid(request.OldDepartmentId);
            entity.NewDepartmentId = NormalizeGuid(request.NewDepartmentId);
            entity.OldPositionId = NormalizeGuid(request.OldPositionId);
            entity.NewPositionId = NormalizeGuid(request.NewPositionId);
            entity.OldOrganizationUnitId = NormalizeGuid(request.OldOrganizationUnitId);
            entity.NewOrganizationUnitId = NormalizeGuid(request.NewOrganizationUnitId);
            entity.OldEmployeeGradeId = NormalizeGuid(request.OldEmployeeGradeId);
            entity.NewEmployeeGradeId = NormalizeGuid(request.NewEmployeeGradeId);
            entity.EffectiveDate = request.EffectiveDate.Date;
            entity.EndDate = request.EndDate?.Date;
            entity.Reason = Normalize(request.Reason);
            entity.ReferenceType = Normalize(request.ReferenceType);
            entity.ReferenceId = NormalizeGuid(request.ReferenceId);
            entity.Description = Normalize(request.Description);
            entity.IsActive = request.IsActive;

            if (newFilePath != null)
            {
                entity.FilePath = newFilePath;
                entity.FileContentType = request.File?.ContentType;
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (newFilePath != null &&
                request.ReplaceExistingFile &&
                !string.IsNullOrWhiteSpace(oldFilePath))
            {
                DeletePhysicalFile(oldFilePath);
            }

            return Ok(ApiResponse<object>.Ok(
                null,
                "Employment history berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/approval")]
        [AccessAction("Update", "Approve Workforce Employment History", Description = "Melakukan approval employment history", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpEmploymentHistory", "Update")]
        public async Task<IActionResult> Approve(
            Guid workforceProfileId,
            Guid id,
            [FromBody] ApproveWfpEmploymentHistoryRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(
                workforceProfileId,
                id,
                cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(
                    404,
                    "Employment history tidak ditemukan."));

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            entity.ApprovedByUserId =
                request.IsApproved ? actorUserId : null;

            entity.ApprovedAt =
                request.IsApproved ? now : null;

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsApproved
                    ? "Employment history berhasil disetujui."
                    : "Approval employment history berhasil dibatalkan."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Employment History", Description = "Mengubah status employment history", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpEmploymentHistory", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpEmploymentHistoryStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(
                workforceProfileId,
                id,
                cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(
                    404,
                    "Employment history tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status employment history berhasil diperbarui."));
        }

        [HttpGet("{id:guid}/file")]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Mengunduh file employment history", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmploymentHistory", "Read")]
        public async Task<IActionResult> DownloadFile(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpEmploymentHistory>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete,
                    cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(
                    404,
                    "Employment history tidak ditemukan."));

            if (string.IsNullOrWhiteSpace(entity.FilePath))
                return NotFound(ApiResponse<object>.Fail(
                    404,
                    "File employment history belum tersedia."));

            var physicalPath = ResolvePhysicalPath(entity.FilePath);

            if (!System.IO.File.Exists(physicalPath))
                return NotFound(ApiResponse<object>.Fail(
                    404,
                    "File employment history tidak ditemukan pada storage."));

            var stream = new FileStream(
                physicalPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            return File(
                stream,
                entity.FileContentType ?? "application/octet-stream",
                Path.GetFileName(physicalPath),
                enableRangeProcessing: true);
        }

        [HttpDelete("{id:guid}/file")]
        [AccessAction("Delete", "Delete Workforce Employment History File", Description = "Menghapus file employment history", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WfpEmploymentHistory", "Delete")]
        public async Task<IActionResult> DeleteFile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] DeleteWfpEmploymentHistoryFileRequest? request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(
                workforceProfileId,
                id,
                cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(
                    404,
                    "Employment history tidak ditemukan."));

            if (request?.DeletePhysicalFile ?? true)
                DeletePhysicalFile(entity.FilePath);

            entity.FilePath = null;
            entity.FileContentType = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "File employment history berhasil dihapus."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Employment History", Description = "Menghapus employment history", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WfpEmploymentHistory", "Delete")]
        public async Task<IActionResult> Delete(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(
                workforceProfileId,
                id,
                cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(
                    404,
                    "Employment history tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Employment history berhasil dihapus."));
        }

        private IQueryable<WfpEmploymentHistory> BuildBaseQuery(
            Guid workforceProfileId)
        {
            return _dbContext.Set<WfpEmploymentHistory>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.OldEmploymentStatus)
                .Include(x => x.NewEmploymentStatus)
                .Include(x => x.OldEmploymentType)
                .Include(x => x.NewEmploymentType)
                .Include(x => x.OldDepartment)
                .Include(x => x.NewDepartment)
                .Include(x => x.OldPosition)
                .Include(x => x.NewPosition)
                .Include(x => x.ApprovedByUser)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IOrderedQueryable<WfpEmploymentHistory> ApplySorting(
            IQueryable<WfpEmploymentHistory> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "effectiveDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "historytype" => desc
                    ? query.OrderByDescending(x => x.HistoryType)
                        .ThenByDescending(x => x.EffectiveDate)
                    : query.OrderBy(x => x.HistoryType)
                        .ThenByDescending(x => x.EffectiveDate),

                "newdepartmentname" => desc
                    ? query.OrderByDescending(x =>
                        x.NewDepartment != null
                            ? x.NewDepartment.DepartmentName
                            : string.Empty)
                    : query.OrderBy(x =>
                        x.NewDepartment != null
                            ? x.NewDepartment.DepartmentName
                            : string.Empty),

                "newpositionname" => desc
                    ? query.OrderByDescending(x =>
                        x.NewPosition != null
                            ? x.NewPosition.PositionName
                            : string.Empty)
                    : query.OrderBy(x =>
                        x.NewPosition != null
                            ? x.NewPosition.PositionName
                            : string.Empty),

                "approvedat" => desc
                    ? query.OrderByDescending(x => x.ApprovedAt)
                    : query.OrderBy(x => x.ApprovedAt),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive)
                        .ThenByDescending(x => x.EffectiveDate)
                    : query.OrderBy(x => x.IsActive)
                        .ThenByDescending(x => x.EffectiveDate),

                _ => desc
                    ? query.OrderByDescending(x => x.EffectiveDate)
                        .ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.EffectiveDate)
                        .ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWfpEmploymentHistoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.HistoryType))
                return (false, "History type wajib diisi.");

            var historyType = NormalizeHistoryType(request.HistoryType);

            if (!AllowedHistoryTypes.Contains(
                    historyType,
                    StringComparer.OrdinalIgnoreCase))
            {
                return (false, $"History type '{request.HistoryType}' tidak valid.");
            }

            if (request.EffectiveDate == default)
                return (false, "Effective date wajib diisi.");

            if (request.EndDate.HasValue &&
                request.EndDate.Value.Date < request.EffectiveDate.Date)
            {
                return (false, "End date tidak boleh lebih kecil dari effective date.");
            }

            var referenceValidation = await ValidateReferenceIdsAsync(request);

            if (!referenceValidation.IsValid)
                return referenceValidation;

            if (request.NewDepartmentId.HasValue &&
                request.NewPositionId.HasValue)
            {
                var positionMatches = await _dbContext.Set<MstPosition>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.NewPositionId.Value &&
                        x.DepartmentId == request.NewDepartmentId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!positionMatches)
                    return (false, "New position tidak sesuai dengan new department.");
            }

            var duplicate = await _dbContext.Set<WfpEmploymentHistory>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.HistoryType == historyType &&
                    x.EffectiveDate == request.EffectiveDate.Date &&
                    x.Id != excludeId &&
                    !x.IsDelete);

            if (duplicate)
            {
                return (
                    false,
                    "Employment history dengan tipe dan tanggal efektif yang sama sudah tersedia.");
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateReferenceIdsAsync(
            CreateWfpEmploymentHistoryRequest request)
        {
            if (!await ExistsIfProvidedAsync<MstEmploymentStatus>(request.OldEmploymentStatusId))
                return (false, "Old employment status tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstEmploymentStatus>(request.NewEmploymentStatusId))
                return (false, "New employment status tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstEmploymentType>(request.OldEmploymentTypeId))
                return (false, "Old employment type tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstEmploymentType>(request.NewEmploymentTypeId))
                return (false, "New employment type tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstDepartment>(request.OldDepartmentId))
                return (false, "Old department tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstDepartment>(request.NewDepartmentId))
                return (false, "New department tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstPosition>(request.OldPositionId))
                return (false, "Old position tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstPosition>(request.NewPositionId))
                return (false, "New position tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstOrganizationUnit>(request.OldOrganizationUnitId))
                return (false, "Old organization unit tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstOrganizationUnit>(request.NewOrganizationUnitId))
                return (false, "New organization unit tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstEmployeeGrade>(request.OldEmployeeGradeId))
                return (false, "Old employee grade tidak ditemukan atau tidak aktif.");

            if (!await ExistsIfProvidedAsync<MstEmployeeGrade>(request.NewEmployeeGradeId))
                return (false, "New employee grade tidak ditemukan atau tidak aktif.");

            return (true, null);
        }

        private async Task<bool> ExistsIfProvidedAsync<TEntity>(Guid? id)
            where TEntity : IdentityModel
        {
            if (!id.HasValue || id.Value == Guid.Empty)
                return true;

            return await _dbContext.Set<TEntity>()
                .AsNoTracking()
                .AnyAsync(x =>
                    EF.Property<Guid>(x, "Id") == id.Value &&
                    EF.Property<bool>(x, "IsActive") &&
                    !EF.Property<bool>(x, "IsDelete"));
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(
            IFormFile? file)
        {
            if (file == null)
                return (true, null);

            if (file.Length <= 0)
                return (false, "File kosong.");

            if (file.Length > MaximumFileSizeBytes)
                return (false, "Ukuran file maksimal 10 MB.");

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedFileExtensions.Contains(extension))
            {
                return (false, "Format file tidak didukung.");
            }

            return (true, null);
        }

        private async Task<string> SaveFileAsync(
            Guid workforceProfileId,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var uploadRoot = _configuration["FileStorage:UploadRootPath"];

            if (string.IsNullOrWhiteSpace(uploadRoot))
                uploadRoot = Path.Combine(_environment.ContentRootPath, "uploads");

            if (!Path.IsPathRooted(uploadRoot))
                uploadRoot = Path.Combine(_environment.ContentRootPath, uploadRoot);

            var relativeDirectory = Path.Combine(
                "human-resource",
                "workforce-core",
                "employment-histories",
                workforceProfileId.ToString());

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

            var publicRequestPath =
                _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";

            publicRequestPath =
                "/" + publicRequestPath.Trim().Trim('/');

            return
                $"{publicRequestPath}/{relativeDirectory.Replace("\\", "/")}/{storedName}";
        }

        private string ResolvePhysicalPath(string storedPath)
        {
            var uploadRoot = _configuration["FileStorage:UploadRootPath"];

            if (string.IsNullOrWhiteSpace(uploadRoot))
                uploadRoot = Path.Combine(_environment.ContentRootPath, "uploads");

            if (!Path.IsPathRooted(uploadRoot))
                uploadRoot = Path.Combine(_environment.ContentRootPath, uploadRoot);

            var publicRequestPath =
                _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";

            var normalizedRequestPath =
                "/" + publicRequestPath.Trim().Trim('/');

            var relativePath = storedPath.Replace("\\", "/");

            if (relativePath.StartsWith(
                    normalizedRequestPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath[normalizedRequestPath.Length..];
            }

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

                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }
            catch
            {
                // Cleanup file fisik tidak menggagalkan transaksi database.
            }
        }

        private async Task<bool> WorkforceProfileExistsAsync(Guid id)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete);
        }

        private IActionResult WorkforceProfileNotFound() =>
            NotFound(ApiResponse<object>.Fail(
                404,
                "Workforce profile tidak ditemukan atau sudah tidak aktif."));

        private async Task<WfpEmploymentHistory?> FindEntityAsync(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpEmploymentHistory>()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete,
                    cancellationToken);
        }

        private static WfpEmploymentHistoryResponse MapResponse(
            WfpEmploymentHistory x,
            IReadOnlyDictionary<Guid, string> actorNames)
        {
            return new WfpEmploymentHistoryResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode =
                    x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName =
                    x.WorkforceProfile?.DisplayName ?? string.Empty,
                HistoryType = x.HistoryType,
                OldStatus = x.OldStatus,
                NewStatus = x.NewStatus,
                OldEmploymentStatusId = x.OldEmploymentStatusId,
                OldEmploymentStatusName =
                    x.OldEmploymentStatus?.EmploymentStatusName,
                NewEmploymentStatusId = x.NewEmploymentStatusId,
                NewEmploymentStatusName =
                    x.NewEmploymentStatus?.EmploymentStatusName,
                OldEmploymentTypeId = x.OldEmploymentTypeId,
                OldEmploymentTypeName =
                    x.OldEmploymentType?.EmploymentTypeName,
                NewEmploymentTypeId = x.NewEmploymentTypeId,
                NewEmploymentTypeName =
                    x.NewEmploymentType?.EmploymentTypeName,
                OldDepartmentId = x.OldDepartmentId,
                OldDepartmentName = x.OldDepartment?.DepartmentName,
                NewDepartmentId = x.NewDepartmentId,
                NewDepartmentName = x.NewDepartment?.DepartmentName,
                OldPositionId = x.OldPositionId,
                OldPositionName = x.OldPosition?.PositionName,
                NewPositionId = x.NewPositionId,
                NewPositionName = x.NewPosition?.PositionName,
                OldOrganizationUnitId = x.OldOrganizationUnitId,
                NewOrganizationUnitId = x.NewOrganizationUnitId,
                OldEmployeeGradeId = x.OldEmployeeGradeId,
                NewEmployeeGradeId = x.NewEmployeeGradeId,
                EffectiveDate = x.EffectiveDate,
                EndDate = x.EndDate,
                Reason = x.Reason,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName =
                    x.ApprovedByUser?.DisplayName ??
                    x.ApprovedByUser?.UserName ??
                    x.ApprovedByUser?.Email,
                ApprovedAt = x.ApprovedAt,
                IsApproved =
                    x.ApprovedAt.HasValue &&
                    x.ApprovedByUserId.HasValue,
                FilePath = x.FilePath,
                FileContentType = x.FileContentType,
                FileName = string.IsNullOrWhiteSpace(x.FilePath)
                    ? null
                    : Path.GetFileName(x.FilePath),
                FileDownloadUrl = string.IsNullOrWhiteSpace(x.FilePath)
                    ? null
                    : $"/api/v1/corporate/human-resource/workforce-profiles/{x.WorkforceProfileId}/employment-histories/{x.Id}/file",
                HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            };
        }

        private async Task<Dictionary<Guid, string>> BuildActorNameMapAsync(
            IEnumerable<Guid> ids)
        {
            var validIds = ids
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => validIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.DisplayName ??
                         x.UserName ??
                         x.Email ??
                         x.UserCode);
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string> map,
            Guid id) =>
            id == Guid.Empty ? null : map.GetValueOrDefault(id);

        private Guid GetCurrentUserId()
        {
            var value =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }

        private static string NormalizeHistoryType(string value)
        {
            var normalized = value.Trim();

            return AllowedHistoryTypes.FirstOrDefault(x =>
                       x.Equals(
                           normalized,
                           StringComparison.OrdinalIgnoreCase))
                   ?? normalized;
        }

        private static string BuildHistoryTypeLabel(string value)
        {
            return value switch
            {
                "Join" => "Bergabung",
                "StatusChange" => "Perubahan status",
                "Transfer" => "Transfer",
                "Promotion" => "Promosi",
                "Demotion" => "Demosi",
                "Rotation" => "Rotasi",
                "ContractChange" => "Perubahan kontrak",
                "Separation" => "Pemisahan",
                _ => value
            };
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();

        private static Guid? NormalizeGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;

        private static void NormalizePaging(
            ref int pageNumber,
            ref int pageSize)
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

            if (!string.IsNullOrWhiteSpace(selected) &&
                selected != "custom")
            {
                return selected switch
                {
                    "today" =>
                        (today, today.AddDays(1)),

                    "last7days" =>
                        (today.AddDays(-6), today.AddDays(1)),

                    "last30days" =>
                        (today.AddDays(-29), today.AddDays(1)),

                    "thismonth" =>
                        (
                            new DateTime(
                                today.Year,
                                today.Month,
                                1,
                                0,
                                0,
                                0,
                                DateTimeKind.Utc),
                            new DateTime(
                                today.Year,
                                today.Month,
                                1,
                                0,
                                0,
                                0,
                                DateTimeKind.Utc)
                                .AddMonths(1)
                        ),

                    _ => (null, null)
                };
            }

            return (
                startDate.HasValue
                    ? DateTime.SpecifyKind(
                        startDate.Value.Date,
                        DateTimeKind.Utc)
                    : null,

                endDate.HasValue
                    ? DateTime.SpecifyKind(
                        endDate.Value.Date.AddDays(1),
                        DateTimeKind.Utc)
                    : null
            );
        }
    }
}
