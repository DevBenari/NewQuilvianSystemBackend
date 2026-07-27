using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using FamilyMemberPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs.WfpFamilyMemberResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/family-members")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Family Member",
        AreaName = "Corporate",
        ControllerName = "WfpFamilyMember",
        Description = "Corporate human resource workforce family member",
        SortOrder = 7)]
    [Tags("Corporate / Human Resource / Workforce Core / Family Member")]
    public class WfpFamilyMemberController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpFamilyMemberController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Family Member", Description = "Melihat metadata family member", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpFamilyMember", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpFamilyMemberFilterMetadataResponse
            {
                DefaultFilter = new WfpFamilyMemberDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                RelationshipOptions = new List<WfpFamilyMemberStringOptionResponse>
                {
                    new() { Value = "Spouse", Label = "Pasangan" },
                    new() { Value = "Child", Label = "Anak" },
                    new() { Value = "Father", Label = "Ayah" },
                    new() { Value = "Mother", Label = "Ibu" },
                    new() { Value = "Sibling", Label = "Saudara" },
                    new() { Value = "Guardian", Label = "Wali" },
                    new() { Value = "Other", Label = "Lainnya" }
                },
                GenderOptions = Enum.GetValues<Gender>()
                    .Select(x => new WfpFamilyMemberEnumOptionResponse
                    {
                        Value = Convert.ToInt32(x),
                        Name = x.ToString(),
                        Label = x.ToString()
                    })
                    .ToList(),
                SortOptions = new List<WfpFamilyMemberStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "fullName", Label = "Nama" },
                    new() { Value = "relationship", Label = "Hubungan" },
                    new() { Value = "birthDate", Label = "Tanggal lahir" },
                    new() { Value = "isEmergencyContact", Label = "Kontak darurat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpFamilyMemberFilterMetadataResponse>.Ok(
                result,
                "Metadata filter family member berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Family Member", Description = "Melihat ringkasan family member", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpFamilyMember", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId))
                return WorkforceProfileNotFound();

            var query = _dbContext.Set<WfpFamilyMember>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpFamilyMemberSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                EmergencyContactData = await query.CountAsync(x => x.IsEmergencyContact),
                WithDependentData = await query.CountAsync(x =>
                    _dbContext.Set<WfpDependent>().Any(d =>
                        d.FamilyMemberId == x.Id &&
                        !d.IsDelete))
            };

            return Ok(ApiResponse<WfpFamilyMemberSummaryResponse>.Ok(
                result,
                "Ringkasan family member berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Family Member", Description = "Melihat family member", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpFamilyMember", "Read")]
        public async Task<IActionResult> GetFamilyMembers(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] string? relationship,
            [FromQuery] Gender? gender,
            [FromQuery] bool? isEmergencyContact,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "fullName",
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

            if (gender.HasValue)
                query = query.Where(x => x.Gender == gender.Value);

            if (isEmergencyContact.HasValue)
                query = query.Where(x => x.IsEmergencyContact == isEmergencyContact.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.FullName.ToLower().Contains(keyword) ||
                    x.Relationship.ToLower().Contains(keyword) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(keyword)) ||
                    (x.Occupation != null && x.Occupation.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Contains(keyword)) ||
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

            return Ok(ApiResponse<FamilyMemberPagedResult>.Ok(
                new FamilyMemberPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data family member berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Workforce Family Member", Description = "Melihat pilihan family member", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpFamilyMember", "Read")]
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
                    x.Relationship.ToLower().Contains(keyword));
            }

            var result = await query
                .OrderBy(x => x.FullName)
                .Take(take)
                .Select(x => new WfpFamilyMemberOptionResponse
                {
                    Id = x.Id,
                    Relationship = x.Relationship,
                    FullName = x.FullName,
                    BirthDate = x.BirthDate,
                    IsEmergencyContact = x.IsEmergencyContact,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(ApiResponse<List<WfpFamilyMemberOptionResponse>>.Ok(
                result,
                "Pilihan family member berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Family Member", Description = "Melihat detail family member", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpFamilyMember", "Read")]
        public async Task<IActionResult> GetById(Guid workforceProfileId, Guid id)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Family member tidak ditemukan."));

            var actorNames = await BuildActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });

            return Ok(ApiResponse<WfpFamilyMemberResponse>.Ok(
                MapResponse(entity, actorNames),
                "Detail family member berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Workforce Family Member", Description = "Membuat family member", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WfpFamilyMember", "Create")]
        public async Task<IActionResult> Create(
            Guid workforceProfileId,
            [FromBody] CreateWfpFamilyMemberRequest request)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId))
                return WorkforceProfileNotFound();

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                null,
                request.Relationship,
                request.FullName,
                request.BirthDate,
                request.IdentityNumber);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var entity = new WfpFamilyMember
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                Relationship = request.Relationship.Trim(),
                FullName = request.FullName.Trim(),
                Gender = request.Gender,
                BirthDate = request.BirthDate?.Date,
                IdentityType = Normalize(request.IdentityType),
                IdentityNumber = Normalize(request.IdentityNumber),
                MaritalStatusText = Normalize(request.MaritalStatusText),
                Occupation = Normalize(request.Occupation),
                PhoneNumber = NormalizePhone(request.PhoneNumber),
                Email = Normalize(request.Email)?.ToLowerInvariant(),
                IsEmergencyContact = request.IsEmergencyContact,
                IsActive = request.IsActive,
                Description = Normalize(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpFamilyMember>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WfpFamilyMember.Create",
                "Family member berhasil dibuat.",
                new { entity.Id, entity.WorkforceProfileId, entity.FullName, entity.Relationship });

            return Ok(ApiResponse<object>.Ok(new { entity.Id }, "Family member berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Workforce Family Member", Description = "Mengubah family member", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpFamilyMember", "Update")]
        public async Task<IActionResult> Update(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpFamilyMemberRequest request)
        {
            var entity = await FindEntityAsync(workforceProfileId, id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Family member tidak ditemukan."));

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                id,
                request.Relationship,
                request.FullName,
                request.BirthDate,
                request.IdentityNumber);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            entity.Relationship = request.Relationship.Trim();
            entity.FullName = request.FullName.Trim();
            entity.Gender = request.Gender;
            entity.BirthDate = request.BirthDate?.Date;
            entity.IdentityType = Normalize(request.IdentityType);
            entity.IdentityNumber = Normalize(request.IdentityNumber);
            entity.MaritalStatusText = Normalize(request.MaritalStatusText);
            entity.Occupation = Normalize(request.Occupation);
            entity.PhoneNumber = NormalizePhone(request.PhoneNumber);
            entity.Email = Normalize(request.Email)?.ToLowerInvariant();
            entity.IsEmergencyContact = request.IsEmergencyContact;
            entity.IsActive = request.IsActive;
            entity.Description = Normalize(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Family member berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Family Member", Description = "Mengubah status family member", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpFamilyMember", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpFamilyMemberStatusRequest request)
        {
            var entity = await FindEntityAsync(workforceProfileId, id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Family member tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Status family member berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/emergency-contact")]
        [AccessAction("Update", "Update Workforce Family Member", Description = "Mengubah penanda kontak darurat family member", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WfpFamilyMember", "Update")]
        public async Task<IActionResult> UpdateEmergencyContact(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpFamilyMemberEmergencyContactRequest request)
        {
            var entity = await FindEntityAsync(workforceProfileId, id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Family member tidak ditemukan."));

            entity.IsEmergencyContact = request.IsEmergencyContact;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Penanda kontak darurat berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Family Member", Description = "Menghapus family member", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WfpFamilyMember", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id)
        {
            var entity = await FindEntityAsync(workforceProfileId, id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Family member tidak ditemukan."));

            var hasDependent = await _dbContext.Set<WfpDependent>()
                .AsNoTracking()
                .AnyAsync(x => x.FamilyMemberId == id && !x.IsDelete);

            if (hasDependent)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    400,
                    "Family member masih digunakan sebagai dependent. Nonaktifkan atau hapus dependent terlebih dahulu."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Family member berhasil dihapus."));
        }

        private IQueryable<WfpFamilyMember> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpFamilyMember>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Dependents)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IOrderedQueryable<WfpFamilyMember> ApplySorting(
            IQueryable<WfpFamilyMember> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "fullName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "relationship" => desc ? query.OrderByDescending(x => x.Relationship).ThenBy(x => x.FullName) : query.OrderBy(x => x.Relationship).ThenBy(x => x.FullName),
                "birthdate" => desc ? query.OrderByDescending(x => x.BirthDate).ThenBy(x => x.FullName) : query.OrderBy(x => x.BirthDate).ThenBy(x => x.FullName),
                "isemergencycontact" => desc ? query.OrderByDescending(x => x.IsEmergencyContact).ThenBy(x => x.FullName) : query.OrderBy(x => x.IsEmergencyContact).ThenBy(x => x.FullName),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.FullName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.FullName),
                _ => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            string relationship,
            string fullName,
            DateTime? birthDate,
            string? identityNumber)
        {
            if (string.IsNullOrWhiteSpace(relationship))
                return (false, "Relationship wajib diisi.");

            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Nama family member wajib diisi.");

            if (birthDate.HasValue && birthDate.Value.Date > DateTime.UtcNow.Date)
                return (false, "Tanggal lahir tidak boleh melebihi tanggal hari ini.");

            var normalizedIdentity = Normalize(identityNumber);

            if (!string.IsNullOrWhiteSpace(normalizedIdentity))
            {
                var duplicate = await _dbContext.Set<WfpFamilyMember>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IdentityNumber == normalizedIdentity &&
                        x.Id != excludeId &&
                        !x.IsDelete);

                if (duplicate)
                    return (false, "Nomor identitas family member sudah digunakan.");
            }

            return (true, null);
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

        private async Task<WfpFamilyMember?> FindEntityAsync(Guid workforceProfileId, Guid id)
        {
            return await _dbContext.Set<WfpFamilyMember>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static WfpFamilyMemberResponse MapResponse(
            WfpFamilyMember x,
            IReadOnlyDictionary<Guid, string> actorNames)
        {
            return new WfpFamilyMemberResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                Relationship = x.Relationship,
                FullName = x.FullName,
                Gender = x.Gender,
                BirthDate = x.BirthDate,
                IdentityType = x.IdentityType,
                IdentityNumber = x.IdentityNumber,
                MaritalStatusText = x.MaritalStatusText,
                Occupation = x.Occupation,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                IsEmergencyContact = x.IsEmergencyContact,
                DependentCount = x.Dependents.Count(d => !d.IsDelete),
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

        private async Task<Dictionary<Guid, string>> BuildActorNameMapAsync(IEnumerable<Guid> ids)
        {
            var validIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => validIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string> map, Guid id) =>
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

        private static List<WfpFamilyMemberStringOptionResponse> BuildPeriodOptions() =>
            new()
            {
                new() { Value = "custom", Label = "Custom" },
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "last30days", Label = "30 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" }
            };
    }
}
