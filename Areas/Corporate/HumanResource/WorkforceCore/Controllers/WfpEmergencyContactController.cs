using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using EmergencyContactPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs.WfpEmergencyContactResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/emergency-contacts")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Emergency Contact",
        AreaName = "Corporate",
        ControllerName = "WfpEmergencyContact",
        Description = "Corporate human resource workforce emergency contact",
        SortOrder = 8)]
    [Tags("Corporate / Human Resource / Workforce Core / Emergency Contact")]
    public class WfpEmergencyContactController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpEmergencyContactController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Emergency Contact", Description = "Melihat metadata emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmergencyContact", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpEmergencyContactFilterMetadataResponse
            {
                DefaultFilter = new WfpEmergencyContactDefaultFilterResponse(),
                CustomPeriods = new List<WfpEmergencyContactStringOptionResponse>
                {
                    new() { Value = "custom", Label = "Custom" },
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "last30days", Label = "30 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" }
                },
                RelationshipOptions = new List<WfpEmergencyContactStringOptionResponse>
                {
                    new() { Value = "Spouse", Label = "Pasangan" },
                    new() { Value = "Parent", Label = "Orang tua" },
                    new() { Value = "Child", Label = "Anak" },
                    new() { Value = "Sibling", Label = "Saudara" },
                    new() { Value = "Guardian", Label = "Wali" },
                    new() { Value = "Friend", Label = "Teman" },
                    new() { Value = "Other", Label = "Lainnya" }
                },
                SortOptions = new List<WfpEmergencyContactStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "priorityOrder", Label = "Prioritas" },
                    new() { Value = "fullName", Label = "Nama" },
                    new() { Value = "relationship", Label = "Hubungan" },
                    new() { Value = "isPrimary", Label = "Kontak utama" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpEmergencyContactFilterMetadataResponse>.Ok(
                result,
                "Metadata filter emergency contact berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Emergency Contact", Description = "Melihat ringkasan emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmergencyContact", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId))
                return WorkforceProfileNotFound();

            var query = _dbContext.Set<WfpEmergencyContact>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpEmergencyContactSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                PrimaryData = await query.CountAsync(x => x.IsPrimary && x.IsActive),
                WithWhatsAppData = await query.CountAsync(x =>
                    x.WhatsAppNumber != null &&
                    x.WhatsAppNumber != string.Empty)
            };

            return Ok(ApiResponse<WfpEmergencyContactSummaryResponse>.Ok(
                result,
                "Ringkasan emergency contact berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Emergency Contact", Description = "Melihat emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmergencyContact", "Read")]
        public async Task<IActionResult> GetEmergencyContacts(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] string? relationship,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "priorityOrder",
            [FromQuery] string? sortDirection = "asc",
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

            if (!string.IsNullOrWhiteSpace(relationship))
                query = query.Where(x => x.Relationship == relationship.Trim());

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.FullName.ToLower().Contains(keyword) ||
                    x.Relationship.ToLower().Contains(keyword) ||
                    x.PhoneNumber.Contains(keyword) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await BuildActorNameMapAsync(
                entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy }));

            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<EmergencyContactPagedResult>.Ok(
                new EmergencyContactPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data emergency contact berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Workforce Emergency Contact", Description = "Melihat pilihan emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmergencyContact", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int take = 100)
        {
            take = Math.Clamp(take, 1, 200);
            var query = BuildBaseQuery(workforceProfileId);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.FullName.ToLower().Contains(keyword) ||
                    x.Relationship.ToLower().Contains(keyword) ||
                    x.PhoneNumber.Contains(keyword));
            }

            var result = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.PriorityOrder)
                .ThenBy(x => x.FullName)
                .Take(take)
                .Select(x => new WfpEmergencyContactOptionResponse
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Relationship = x.Relationship,
                    PhoneNumber = x.PhoneNumber,
                    PriorityOrder = x.PriorityOrder,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<WfpEmergencyContactOptionResponse>>.Ok(
                result,
                "Pilihan emergency contact berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Emergency Contact", Description = "Melihat detail emergency contact", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpEmergencyContact", "Read")]
        public async Task<IActionResult> GetById(Guid workforceProfileId, Guid id)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Emergency contact tidak ditemukan."));

            var actorNames = await BuildActorNameMapAsync(
                new[] { entity.CreateBy, entity.UpdateBy });

            return Ok(ApiResponse<WfpEmergencyContactResponse>.Ok(
                MapResponse(entity, actorNames),
                "Detail emergency contact berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Workforce Emergency Contact", Description = "Membuat emergency contact", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WfpEmergencyContact", "Create")]
        public async Task<IActionResult> Create(
            Guid workforceProfileId,
            [FromBody] CreateWfpEmergencyContactRequest request)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId))
                return WorkforceProfileNotFound();

            var validation = ValidateRequest(request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsPrimary)
                {
                    await ClearCurrentPrimaryAsync(
                        workforceProfileId,
                        null,
                        actorUserId,
                        now);
                }

                var entity = new WfpEmergencyContact
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    FullName = request.FullName.Trim(),
                    Relationship = request.Relationship.Trim(),
                    PhoneNumber = NormalizePhone(request.PhoneNumber) ?? string.Empty,
                    WhatsAppNumber = NormalizePhone(request.WhatsAppNumber),
                    Email = Normalize(request.Email)?.ToLowerInvariant(),
                    Address = Normalize(request.Address),
                    PriorityOrder = request.PriorityOrder,
                    IsPrimary = request.IsPrimary,
                    IsActive = request.IsActive,
                    Description = Normalize(request.Description),
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpEmergencyContact>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WfpEmergencyContact.Create",
                    "Emergency contact berhasil dibuat.",
                    new { entity.Id, entity.WorkforceProfileId, entity.FullName, entity.IsPrimary });

                return Ok(ApiResponse<object>.Ok(
                    new { entity.Id },
                    "Emergency contact berhasil dibuat."));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Workforce Emergency Contact", Description = "Mengubah emergency contact", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpEmergencyContact", "Update")]
        public async Task<IActionResult> Update(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpEmergencyContactRequest request)
        {
            var entity = await FindEntityAsync(workforceProfileId, id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Emergency contact tidak ditemukan."));

            var validation = ValidateRequest(request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsPrimary)
                {
                    await ClearCurrentPrimaryAsync(
                        workforceProfileId,
                        id,
                        actorUserId,
                        now);
                }

                entity.FullName = request.FullName.Trim();
                entity.Relationship = request.Relationship.Trim();
                entity.PhoneNumber = NormalizePhone(request.PhoneNumber) ?? string.Empty;
                entity.WhatsAppNumber = NormalizePhone(request.WhatsAppNumber);
                entity.Email = Normalize(request.Email)?.ToLowerInvariant();
                entity.Address = Normalize(request.Address);
                entity.PriorityOrder = request.PriorityOrder;
                entity.IsPrimary = request.IsPrimary;
                entity.IsActive = request.IsActive;
                entity.Description = Normalize(request.Description);
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Emergency contact berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Emergency Contact", Description = "Mengubah status emergency contact", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpEmergencyContact", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpEmergencyContactStatusRequest request)
        {
            var entity = await FindEntityAsync(workforceProfileId, id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Emergency contact tidak ditemukan."));

            var wasPrimary = entity.IsPrimary;
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;

            if (!request.IsActive)
                entity.IsPrimary = false;

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            if (wasPrimary && !request.IsActive)
                await PromoteNextPrimaryIfNeededAsync(workforceProfileId, actorUserId, now);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status emergency contact berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/primary")]
        [AccessAction("Update", "Update Workforce Emergency Contact", Description = "Mengatur kontak utama", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpEmergencyContact", "Update")]
        public async Task<IActionResult> SetPrimary(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWfpEmergencyContactPrimaryRequest request)
        {
            var entity = await FindEntityAsync(workforceProfileId, id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Emergency contact tidak ditemukan."));

            if (request.IsPrimary && !entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    400,
                    "Emergency contact tidak aktif tidak dapat dijadikan primary."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsPrimary)
                {
                    await ClearCurrentPrimaryAsync(
                        workforceProfileId,
                        id,
                        actorUserId,
                        now);
                }

                entity.IsPrimary = request.IsPrimary;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Kontak utama berhasil diperbarui."));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Emergency Contact", Description = "Menghapus emergency contact", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WfpEmergencyContact", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id)
        {
            var entity = await FindEntityAsync(workforceProfileId, id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Emergency contact tidak ditemukan."));

            var wasPrimary = entity.IsPrimary;
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            if (wasPrimary)
                await PromoteNextPrimaryIfNeededAsync(workforceProfileId, actorUserId, now);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Emergency contact berhasil dihapus."));
        }

        private IQueryable<WfpEmergencyContact> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpEmergencyContact>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IOrderedQueryable<WfpEmergencyContact> ApplySorting(
            IQueryable<WfpEmergencyContact> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "priorityOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "fullname" => desc
                    ? query.OrderByDescending(x => x.FullName)
                    : query.OrderBy(x => x.FullName),

                "relationship" => desc
                    ? query.OrderByDescending(x => x.Relationship).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.Relationship).ThenBy(x => x.FullName),

                "isprimary" => desc
                    ? query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.PriorityOrder)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.PriorityOrder),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.PriorityOrder)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.PriorityOrder),

                _ => desc
                    ? query.OrderByDescending(x => x.PriorityOrder).ThenBy(x => x.FullName)
                    : query.OrderBy(x => x.PriorityOrder).ThenBy(x => x.FullName)
            };
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequest(
            CreateWfpEmergencyContactRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return (false, "Nama emergency contact wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.Relationship))
                return (false, "Hubungan emergency contact wajib diisi.");

            if (string.IsNullOrWhiteSpace(NormalizePhone(request.PhoneNumber)))
                return (false, "Nomor telepon emergency contact wajib diisi.");

            if (request.PriorityOrder <= 0)
                return (false, "Priority order minimal 1.");

            if (request.IsPrimary && !request.IsActive)
                return (false, "Emergency contact primary harus berstatus aktif.");

            return (true, null);
        }

        private async Task ClearCurrentPrimaryAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            Guid actorUserId,
            DateTime now)
        {
            var primaries = await _dbContext.Set<WfpEmergencyContact>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsPrimary &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.Id != excludeId)
                .ToListAsync();

            foreach (var item in primaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task PromoteNextPrimaryIfNeededAsync(
            Guid workforceProfileId,
            Guid actorUserId,
            DateTime now)
        {
            var hasPrimary = await _dbContext.Set<WfpEmergencyContact>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsPrimary &&
                    x.IsActive &&
                    !x.IsDelete);

            if (hasPrimary)
                return;

            var next = await _dbContext.Set<WfpEmergencyContact>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderBy(x => x.PriorityOrder)
                .ThenBy(x => x.CreateDateTime)
                .FirstOrDefaultAsync();

            if (next == null)
                return;

            next.IsPrimary = true;
            next.UpdateDateTime = now;
            next.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
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

        private async Task<WfpEmergencyContact?> FindEntityAsync(
            Guid workforceProfileId,
            Guid id)
        {
            return await _dbContext.Set<WfpEmergencyContact>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static WfpEmergencyContactResponse MapResponse(
            WfpEmergencyContact x,
            IReadOnlyDictionary<Guid, string> actorNames)
        {
            return new WfpEmergencyContactResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                FullName = x.FullName,
                Relationship = x.Relationship,
                PhoneNumber = x.PhoneNumber,
                WhatsAppNumber = x.WhatsAppNumber,
                Email = x.Email,
                Address = x.Address,
                PriorityOrder = x.PriorityOrder,
                IsPrimary = x.IsPrimary,
                IsActive = x.IsActive,
                Description = x.Description,
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
            var validIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => validIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode);
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string> map,
            Guid id) =>
            id == Guid.Empty ? null : map.GetValueOrDefault(id);

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue("user_id") ??
                        User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizePhone(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? null
                : new string(value.Where(char.IsDigit).ToArray());

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
                    "thismonth" => (
                        new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                    _ => (null, null)
                };
            }

            return (
                startDate.HasValue
                    ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                    : null,
                endDate.HasValue
                    ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc)
                    : null);
        }
    }
}
