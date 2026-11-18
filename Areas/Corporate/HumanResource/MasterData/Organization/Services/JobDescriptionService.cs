using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Services
{
    public class JobDescriptionService : IJobDescriptionService
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData.Organization";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        private static readonly HashSet<string> AllowedSectionTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "MainDuty",
                "Responsibility",
                "Authority",
                "WorkRelation"
            };

        public JobDescriptionService(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        public Task<JobDescriptionFilterMetadataResponse> GetFilterMetadataAsync()
        {
            var result = new JobDescriptionFilterMetadataResponse
            {
                StatusOptions = new List<JobDescriptionStringOptionResponse>
                {
                    new() { Value = "", Label = "Semua Status" },
                    new() { Value = "Draft", Label = "Draft" },
                    new() { Value = "Active", Label = "Aktif" },
                    new() { Value = "Archived", Label = "Diarsipkan" }
                },
                SectionTypeOptions = new List<JobDescriptionStringOptionResponse>
                {
                    new() { Value = "MainDuty", Label = "Tugas Pokok" },
                    new() { Value = "Responsibility", Label = "Tanggung Jawab" },
                    new() { Value = "Authority", Label = "Wewenang" },
                    new() { Value = "WorkRelation", Label = "Hubungan Kerja" }
                }
            };

            return Task.FromResult(result);
        }

        public async Task<JobDescriptionSummaryResponse> GetSummaryAsync()
        {
            var query = _dbContext.MstJobDescriptions
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var totalData = await query.CountAsync();
            var activeData = await query.CountAsync(x => x.Status == "Active");
            var draftData = await query.CountAsync(x => x.Status == "Draft");
            var archivedData = await query.CountAsync(x => x.Status == "Archived");

            var totalPositions = await _dbContext.MstPositions
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete && x.IsActive);

            var coveredPositions = await query
                .Where(x => x.Status == "Active")
                .Select(x => x.PositionId)
                .Distinct()
                .CountAsync();

            return new JobDescriptionSummaryResponse
            {
                TotalData = totalData,
                ActiveData = activeData,
                DraftData = draftData,
                ArchivedData = archivedData,
                TotalPositions = totalPositions,
                PositionsWithActiveJobDescription = coveredPositions
            };
        }

        public async Task<PagedResult<JobDescriptionResponse>> GetDataAsync(
            Guid? positionId,
            Guid? departmentId,
            string? status,
            DateTime? asOfDate,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);

            var query = _dbContext.MstJobDescriptions
                .AsNoTracking()
                .Include(x => x.Position)
                    .ThenInclude(p => p!.Department)
                .Include(x => x.Items)
                .Where(x => !x.IsDelete);

            if (positionId.HasValue && positionId.Value != Guid.Empty)
                query = query.Where(x => x.PositionId == positionId.Value);

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
                query = query.Where(x => x.Position != null && x.Position.DepartmentId == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status == status.Trim());

            if (asOfDate.HasValue)
            {
                var targetDate = asOfDate.Value.Date;
                query = query.Where(x =>
                    x.EffectiveStartDate.Date <= targetDate &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= targetDate));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.JobDescriptionCode.ToLower().Contains(keyword) ||
                    x.Purpose.ToLower().Contains(keyword) ||
                    (x.Position != null && (
                        x.Position.PositionName.ToLower().Contains(keyword) ||
                        x.Position.PositionCode.ToLower().Contains(keyword)
                    )));
            }

            var totalData = await query.CountAsync();

            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var ordered = (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "jobdescriptioncode" => desc ? query.OrderByDescending(x => x.JobDescriptionCode) : query.OrderBy(x => x.JobDescriptionCode),
                "versionnumber" => desc ? query.OrderByDescending(x => x.VersionNumber) : query.OrderBy(x => x.VersionNumber),
                "status" => desc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                "positionname" => desc ? query.OrderByDescending(x => x.Position != null ? x.Position.PositionName : "") : query.OrderBy(x => x.Position != null ? x.Position.PositionName : ""),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate)
            };

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new JobDescriptionResponse
                {
                    Id = x.Id,
                    PositionId = x.PositionId,
                    PositionCode = x.Position != null ? x.Position.PositionCode : string.Empty,
                    PositionName = x.Position != null ? x.Position.PositionName : string.Empty,
                    DepartmentId = x.Position != null ? x.Position.DepartmentId : Guid.Empty,
                    DepartmentName = x.Position != null && x.Position.Department != null ? x.Position.Department.DepartmentName : string.Empty,
                    JobDescriptionCode = x.JobDescriptionCode,
                    VersionNumber = x.VersionNumber,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Status = x.Status,
                    Purpose = x.Purpose,
                    ItemCount = x.Items.Count(i => !i.IsDelete),
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(items.Select(x => x.CreateBy));
            foreach (var item in items)
                item.CreateByName = GetActorName(actorNames, item.CreateBy);

            return new PagedResult<JobDescriptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<JobDescriptionDetailResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _dbContext.MstJobDescriptions
                .AsNoTracking()
                .Include(x => x.Position)
                    .ThenInclude(p => p!.Department)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null) return null;

            return await MapToDetailResponseAsync(entity);
        }

        public async Task<JobDescriptionDetailResponse?> GetEffectiveByPositionAsync(Guid positionId, DateTime? asOfDate)
        {
            var targetDate = (asOfDate ?? DateTime.UtcNow).Date;

            var entity = await _dbContext.MstJobDescriptions
                .AsNoTracking()
                .Include(x => x.Position)
                    .ThenInclude(p => p!.Department)
                .Include(x => x.Items)
                .Where(x =>
                    x.PositionId == positionId &&
                    x.Status == "Active" &&
                    !x.IsDelete &&
                    x.EffectiveStartDate.Date <= targetDate &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= targetDate))
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefaultAsync();

            if (entity == null) return null;

            return await MapToDetailResponseAsync(entity);
        }

        public async Task<List<JobDescriptionHistoryResponse>> GetHistoryByPositionAsync(Guid positionId)
        {
            var list = await _dbContext.MstJobDescriptions
                .AsNoTracking()
                .Where(x => x.PositionId == positionId && !x.IsDelete)
                .OrderByDescending(x => x.VersionNumber)
                .Select(x => new JobDescriptionHistoryResponse
                {
                    Id = x.Id,
                    JobDescriptionCode = x.JobDescriptionCode,
                    VersionNumber = x.VersionNumber,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Status = x.Status,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(list.Select(x => x.CreateBy));
            foreach (var item in list)
                item.CreateByName = GetActorName(actorNames, item.CreateBy);

            return list;
        }

        public async Task<(bool Success, string? ErrorMessage, int StatusCode, JobDescriptionDetailResponse? Result)> CreateDraftAsync(
            CreateJobDescriptionDraftRequest request,
            Guid actorId)
        {
            if (request.PositionId == Guid.Empty)
                return (false, "Position wajib dipilih.", 400, null);

            var position = await _dbContext.MstPositions
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == request.PositionId && !x.IsDelete);

            if (position == null)
                return (false, "Position/Jabatan tidak ditemukan.", 404, null);

            if (string.IsNullOrWhiteSpace(request.Purpose))
                return (false, "Tujuan jabatan wajib diisi.", 400, null);

            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date)
                return (false, "EffectiveEndDate tidak boleh lebih awal dari EffectiveStartDate.", 400, null);

            var existingDraft = await _dbContext.MstJobDescriptions
                .FirstOrDefaultAsync(x => x.PositionId == request.PositionId && x.Status == "Draft" && !x.IsDelete);

            if (existingDraft != null)
            {
                return (false, "Sudah ada draft Uraian Tugas untuk jabatan ini. Selesaikan atau hapus draft tersebut terlebih dahulu.", 409, null);
            }

            var maxVersion = await _dbContext.MstJobDescriptions
                .Where(x => x.PositionId == request.PositionId)
                .MaxAsync(x => (int?)x.VersionNumber) ?? 0;

            var nextVersion = maxVersion + 1;
            var code = $"JD-{position.PositionCode}-V{nextVersion}";

            var now = DateTime.UtcNow;
            var entity = new MstJobDescription
            {
                Id = Guid.NewGuid(),
                PositionId = request.PositionId,
                JobDescriptionCode = code,
                VersionNumber = nextVersion,
                EffectiveStartDate = request.EffectiveStartDate.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Status = "Draft",
                Purpose = request.Purpose.Trim(),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorId,
                IsDelete = false,
                IsCancel = false
            };

            if (request.Items != null && request.Items.Count > 0)
            {
                var seq = 1;
                foreach (var item in request.Items.Where(i => !string.IsNullOrWhiteSpace(i.Content)))
                {
                    var section = NormalizeSectionType(item.SectionType);
                    entity.Items.Add(new MstJobDescriptionItem
                    {
                        Id = Guid.NewGuid(),
                        JobDescriptionId = entity.Id,
                        SectionType = section,
                        Sequence = item.Sequence > 0 ? item.Sequence : seq++,
                        Content = item.Content.Trim(),
                        CreateDateTime = now,
                        CreateBy = actorId,
                        IsDelete = false,
                        IsCancel = false
                    });
                }
            }

            _dbContext.MstJobDescriptions.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "JobDescription.CreateDraft",
                "Membuat draft Uraian Tugas baru.",
                new { entity.Id, entity.PositionId, entity.JobDescriptionCode, entity.VersionNumber, entity.Status, Actor = actorId }
            );

            var detail = await GetByIdAsync(entity.Id);
            return (true, null, 201, detail);
        }

        public async Task<(bool Success, string? ErrorMessage, int StatusCode, JobDescriptionDetailResponse? Result)> UpdateDraftAsync(
            Guid id,
            UpdateJobDescriptionDraftRequest request,
            Guid actorId)
        {
            var entity = await _dbContext.MstJobDescriptions
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return (false, "Uraian tugas tidak ditemukan.", 404, null);

            if (entity.Status != "Draft")
                return (false, "Hanya uraian tugas berstatus Draft yang dapat disunting. Buat versi draft baru untuk perubahan.", 409, null);

            if (string.IsNullOrWhiteSpace(request.Purpose))
                return (false, "Tujuan jabatan wajib diisi.", 400, null);

            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date)
                return (false, "EffectiveEndDate tidak boleh lebih awal dari EffectiveStartDate.", 400, null);

            var now = DateTime.UtcNow;
            entity.EffectiveStartDate = request.EffectiveStartDate.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Purpose = request.Purpose.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorId;

            // Remove existing items and re-add
            _dbContext.MstJobDescriptionItems.RemoveRange(entity.Items);
            entity.Items.Clear();

            if (request.Items != null && request.Items.Count > 0)
            {
                var seq = 1;
                foreach (var item in request.Items.Where(i => !string.IsNullOrWhiteSpace(i.Content)))
                {
                    var section = NormalizeSectionType(item.SectionType);
                    entity.Items.Add(new MstJobDescriptionItem
                    {
                        Id = Guid.NewGuid(),
                        JobDescriptionId = entity.Id,
                        SectionType = section,
                        Sequence = item.Sequence > 0 ? item.Sequence : seq++,
                        Content = item.Content.Trim(),
                        CreateDateTime = now,
                        CreateBy = actorId,
                        IsDelete = false,
                        IsCancel = false
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "JobDescription.UpdateDraft",
                "Memperbarui draft Uraian Tugas.",
                new { entity.Id, entity.JobDescriptionCode, entity.VersionNumber, Actor = actorId }
            );

            var detail = await GetByIdAsync(entity.Id);
            return (true, null, 200, detail);
        }

        public async Task<(bool Success, string? ErrorMessage, int StatusCode, JobDescriptionDetailResponse? Result)> ActivateVersionAsync(
            Guid id,
            ActivateJobDescriptionRequest? request,
            Guid actorId)
        {
            var entity = await _dbContext.MstJobDescriptions
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return (false, "Uraian tugas tidak ditemukan.", 404, null);

            if (entity.Status == "Active")
                return (false, "Uraian tugas ini sudah berstatus Aktif.", 409, null);

            if (string.IsNullOrWhiteSpace(entity.Purpose))
                return (false, "Tujuan jabatan wajib diisi sebelum aktivasi.", 400, null);

            var hasMainDuty = entity.Items.Any(x =>
                !x.IsDelete &&
                string.Equals(x.SectionType, "MainDuty", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(x.Content));

            if (!hasMainDuty)
                return (false, "Minimal harus ada satu Tugas Pokok sebelum aktivasi.", 400, null);

            var activationDate = request?.EffectiveStartDate?.Date ?? entity.EffectiveStartDate.Date;

            await using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;

                // Archive and close any currently Active versions for this position
                var existingActiveVersions = await _dbContext.MstJobDescriptions
                    .Where(x => x.PositionId == entity.PositionId && x.Status == "Active" && x.Id != id && !x.IsDelete)
                    .ToListAsync();

                foreach (var activeVersion in existingActiveVersions)
                {
                    activeVersion.Status = "Archived";
                    if (!activeVersion.EffectiveEndDate.HasValue || activeVersion.EffectiveEndDate.Value.Date >= activationDate)
                    {
                        activeVersion.EffectiveEndDate = activationDate.AddDays(-1);
                    }
                    activeVersion.UpdateDateTime = now;
                    activeVersion.UpdateBy = actorId;
                }

                entity.Status = "Active";
                entity.EffectiveStartDate = activationDate;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorId;

                await _dbContext.SaveChangesAsync();
                await tx.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "JobDescription.ActivateVersion",
                    "Mengaktifkan versi Uraian Tugas.",
                    new { entity.Id, entity.PositionId, entity.JobDescriptionCode, entity.VersionNumber, EffectiveStartDate = activationDate, ClosedVersions = existingActiveVersions.Count, Actor = actorId }
                );

                var detail = await GetByIdAsync(entity.Id);
                return (true, null, 200, detail);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                await _loggerService.ErrorAsync(LogCategory, "JobDescription.ActivateVersion", "Gagal mengaktifkan versi Uraian Tugas: " + ex.Message, ex);
                return (false, "Terjadi kesalahan saat mengaktifkan versi Uraian Tugas.", 500, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, int StatusCode)> DeactivateAsync(Guid id, Guid actorId)
        {
            var entity = await _dbContext.MstJobDescriptions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return (false, "Uraian tugas tidak ditemukan.", 404);

            var now = DateTime.UtcNow;
            entity.Status = "Archived";
            entity.EffectiveEndDate ??= now.Date;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "JobDescription.Deactivate",
                "Menonaktifkan Uraian Tugas.",
                new { entity.Id, entity.JobDescriptionCode, entity.VersionNumber, Actor = actorId }
            );

            return (true, null, 200);
        }

        public async Task<(bool Success, string? ErrorMessage, int StatusCode)> DeleteAsync(Guid id, Guid actorId)
        {
            var entity = await _dbContext.MstJobDescriptions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return (false, "Uraian tugas tidak ditemukan.", 404);

            if (entity.Status == "Active")
                return (false, "Versi Uraian Tugas yang sedang Aktif tidak dapat dihapus. Silakan nonaktifkan atau buat versi baru.", 409);

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "JobDescription.Delete",
                "Menghapus draft/arsip Uraian Tugas.",
                new { entity.Id, entity.JobDescriptionCode, entity.VersionNumber, Actor = actorId }
            );

            return (true, null, 200);
        }

        private async Task<JobDescriptionDetailResponse> MapToDetailResponseAsync(MstJobDescription entity)
        {
            var actorNames = await GetActorNameMapAsync(new Guid?[] { entity.CreateBy, entity.UpdateBy });

            return new JobDescriptionDetailResponse
            {
                Id = entity.Id,
                PositionId = entity.PositionId,
                PositionCode = entity.Position != null ? entity.Position.PositionCode : string.Empty,
                PositionName = entity.Position != null ? entity.Position.PositionName : string.Empty,
                DepartmentId = entity.Position != null ? entity.Position.DepartmentId : Guid.Empty,
                DepartmentName = entity.Position != null && entity.Position.Department != null ? entity.Position.Department.DepartmentName : string.Empty,
                JobDescriptionCode = entity.JobDescriptionCode,
                VersionNumber = entity.VersionNumber,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Status = entity.Status,
                Purpose = entity.Purpose,
                ItemCount = entity.Items.Count(i => !i.IsDelete),
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy),
                Items = entity.Items
                    .Where(i => !i.IsDelete)
                    .OrderBy(i => i.SectionType)
                    .ThenBy(i => i.Sequence)
                    .Select(i => new JobDescriptionItemDto
                    {
                        Id = i.Id,
                        SectionType = i.SectionType,
                        Sequence = i.Sequence,
                        Content = i.Content
                    })
                    .ToList()
            };
        }

        private static string NormalizeSectionType(string type)
        {
            return AllowedSectionTypes.FirstOrDefault(x =>
                x.Equals(type.Trim(), StringComparison.OrdinalIgnoreCase)) ?? "MainDuty";
        }

        private async Task<Dictionary<Guid, string>> GetActorNameMapAsync(IEnumerable<Guid?> actorIds)
        {
            var ids = actorIds
                .Where(x => x.HasValue && x.Value != Guid.Empty)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();
            if (ids.Count == 0) return new Dictionary<Guid, string>();

            return await _dbContext.Users.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string> actorNames, Guid? actorId) =>
            !actorId.HasValue || actorId.Value == Guid.Empty ? null : actorNames.GetValueOrDefault(actorId.Value);
    }
}
