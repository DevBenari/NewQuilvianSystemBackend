using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Services
{
    /// <summary>
    /// Hari libur rumah sakit pada kalender kerja, dikelola Admin HRD (HRD-DEC-210). Hanya master data: menambah atau
    /// mengubah hari libur tidak menyentuh roster yang sudah terbit (butir 5). Pemrosesan kehadiran membacanya lewat
    /// AttendanceScheduleResolverService, dan status Holiday hanya berlaku untuk jadwal tetap (BE-HRD-343).
    /// </summary>
    public class HolidayService
    {
        private const string OutsideCalendarMessage = "Tanggal libur harus berada di dalam kalender kerja yang dipilih";

        private static readonly (string Value, string Label)[] HolidayTypes =
        {
            ("National", "Libur nasional"),
            ("CollectiveLeave", "Cuti bersama"),
            ("Hospital", "Libur internal rumah sakit"),
            ("Religious", "Hari raya keagamaan"),
            ("Regional", "Libur daerah"),
            ("Other", "Lainnya")
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HolidayService> _logger;

        public HolidayService(ApplicationDbContext dbContext, ILogger<HolidayService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<HolidayFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken = default)
        {
            var calendars = await _dbContext.Set<MstWorkCalendar>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderByDescending(x => x.CalendarYear)
                .ThenBy(x => x.WorkCalendarName)
                .Select(x => new { x.Id, x.WorkCalendarCode, x.WorkCalendarName, x.CalendarYear, x.StartDate, x.EndDate, x.IsDefault })
                .ToListAsync(cancellationToken);

            return new HolidayFilterMetadataResponse
            {
                WorkCalendarOptions = calendars.Select(x => new HolidayWorkCalendarOptionResponse
                {
                    Id = x.Id,
                    Code = x.WorkCalendarCode,
                    Name = x.WorkCalendarName,
                    CalendarYear = x.CalendarYear,
                    StartDate = DateOnly.FromDateTime(x.StartDate),
                    EndDate = DateOnly.FromDateTime(x.EndDate),
                    IsDefault = x.IsDefault
                }).ToList(),
                HolidayTypeOptions = HolidayTypes.Select(x => new HolidayOptionResponse { Value = x.Value, Label = x.Label }).ToList(),
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<PagedResult<HolidayResponse>> GetPagedAsync(HolidayQueryRequest request, CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var query = BaseQuery();

            if (request.WorkCalendarId.HasValue) query = query.Where(x => x.WorkCalendarId == request.WorkCalendarId.Value);
            if (request.Year.HasValue)
            {
                var yearStart = new DateTime(request.Year.Value, 1, 1);
                var yearEnd = new DateTime(request.Year.Value, 12, 31);
                query = query.Where(x => x.StartDate <= yearEnd && x.EndDate >= yearStart);
            }

            if (!string.IsNullOrWhiteSpace(request.HolidayType)) query = query.Where(x => x.HolidayType == request.HolidayType.Trim());
            if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();
                query = query.Where(x => x.HolidayCode.ToLower().Contains(keyword) || x.HolidayName.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.HolidayName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return new PagedResult<HolidayResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = await MapAsync(rows, cancellationToken)
            };
        }

        public async Task<HolidayResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity == null ? null : (await MapAsync(new List<MstHoliday> { entity }, cancellationToken)).Single();
        }

        public async Task<HolidayServiceResult> CreateAsync(CreateHolidayRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var error = await ValidateAsync(request, null, cancellationToken);
            if (error != null) return HolidayServiceResult.Fail(error.Value.Status, error.Value.Message);

            var now = DateTime.UtcNow;
            var entity = new MstHoliday
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
            Apply(entity, request);
            _dbContext.Set<MstHoliday>().Add(entity);
            return await SaveAndReturnAsync(entity.Id, "Hari libur berhasil ditambahkan.", cancellationToken);
        }

        public async Task<HolidayServiceResult> UpdateAsync(Guid id, UpdateHolidayRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstHoliday>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return HolidayServiceResult.Fail(StatusCodes.Status404NotFound, "Hari libur tidak ditemukan.");

            var error = await ValidateAsync(request, id, cancellationToken);
            if (error != null) return HolidayServiceResult.Fail(error.Value.Status, error.Value.Message);

            Apply(entity, request);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            return await SaveAndReturnAsync(entity.Id, "Hari libur berhasil diperbarui.", cancellationToken);
        }

        public async Task<HolidayServiceResult> UpdateStatusAsync(Guid id, bool isActive, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstHoliday>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return HolidayServiceResult.Fail(StatusCodes.Status404NotFound, "Hari libur tidak ditemukan.");

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            return await SaveAndReturnAsync(entity.Id, isActive ? "Hari libur berhasil diaktifkan." : "Hari libur berhasil dinonaktifkan.", cancellationToken);
        }

        public async Task<HolidayServiceResult> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstHoliday>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return HolidayServiceResult.Fail(StatusCodes.Status404NotFound, "Hari libur tidak ditemukan.");

            var response = (await MapAsync(new List<MstHoliday> { entity }, cancellationToken)).Single();
            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            response.IsActive = false;
            return HolidayServiceResult.Ok(response, "Hari libur berhasil dihapus.");
        }

        private async Task<HolidayServiceResult> SaveAndReturnAsync(Guid id, string message, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                // Indeks unik (WorkCalendarId, HolidayCode) menolak simpan bersamaan yang lolos pemeriksaan awal.
                _logger.LogWarning(ex, "Simpan hari libur ditolak basis data.");
                _dbContext.ChangeTracker.Clear();
                return HolidayServiceResult.Fail(StatusCodes.Status409Conflict, "Kode hari libur sudah dipakai pada kalender kerja ini.");
            }

            return HolidayServiceResult.Ok((await GetByIdAsync(id, cancellationToken))!, message);
        }

        private async Task<(int Status, string Message)?> ValidateAsync(CreateHolidayRequest request, Guid? excludeId, CancellationToken cancellationToken)
        {
            var code = N(request.HolidayCode);
            if (code == null) return (StatusCodes.Status400BadRequest, "Kode hari libur wajib diisi.");
            if (N(request.HolidayName) == null) return (StatusCodes.Status400BadRequest, "Nama hari libur wajib diisi.");
            if (!HolidayTypes.Any(x => x.Value == N(request.HolidayType))) return (StatusCodes.Status400BadRequest, "Jenis hari libur tidak valid.");
            if (request.StartDate == default || request.EndDate == default) return (StatusCodes.Status400BadRequest, "Tanggal mulai dan selesai wajib diisi.");

            var calendar = await _dbContext.Set<MstWorkCalendar>()
                .AsNoTracking()
                .Where(x => x.Id == request.WorkCalendarId && !x.IsDelete && x.IsActive)
                .Select(x => new { x.StartDate, x.EndDate })
                .FirstOrDefaultAsync(cancellationToken);
            if (calendar == null) return (StatusCodes.Status400BadRequest, "Kalender kerja tidak ditemukan atau tidak aktif.");

            // VR-PK-25: EndDate < StartDate, atau tanggal di luar rentang kalender kerja.
            var calendarStart = DateOnly.FromDateTime(calendar.StartDate);
            var calendarEnd = DateOnly.FromDateTime(calendar.EndDate);
            if (request.EndDate < request.StartDate || request.StartDate < calendarStart || request.EndDate > calendarEnd)
            {
                return (StatusCodes.Status400BadRequest, OutsideCalendarMessage);
            }

            if (await _dbContext.Set<MstHoliday>().AnyAsync(x =>
                    !x.IsDelete && x.WorkCalendarId == request.WorkCalendarId && x.HolidayCode == code && (!excludeId.HasValue || x.Id != excludeId),
                    cancellationToken))
            {
                return (StatusCodes.Status409Conflict, "Kode hari libur sudah dipakai pada kalender kerja ini.");
            }

            return null;
        }

        private static void Apply(MstHoliday entity, CreateHolidayRequest request)
        {
            entity.WorkCalendarId = request.WorkCalendarId;
            entity.HolidayCode = N(request.HolidayCode)!;
            entity.HolidayName = N(request.HolidayName)!;
            entity.StartDate = request.StartDate.ToDateTime(TimeOnly.MinValue);
            entity.EndDate = request.EndDate.ToDateTime(TimeOnly.MinValue);
            entity.HolidayType = N(request.HolidayType)!;
            entity.IsNationalHoliday = request.IsNationalHoliday;
            entity.Description = N(request.Description);
        }

        private IQueryable<MstHoliday> BaseQuery() =>
            _dbContext.Set<MstHoliday>().AsNoTracking().Where(x => !x.IsDelete);

        private async Task<List<HolidayResponse>> MapAsync(List<MstHoliday> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0) return new List<HolidayResponse>();
            var actorIds = rows.Select(x => x.CreateBy).Where(x => x != Guid.Empty).Distinct().ToList();
            var actors = await _dbContext.Users.AsNoTracking()
                .Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
            var calendarIds = rows.Select(x => x.WorkCalendarId).Distinct().ToList();
            var calendars = await _dbContext.Set<MstWorkCalendar>().AsNoTracking()
                .Where(x => calendarIds.Contains(x.Id))
                .Select(x => new { x.Id, x.WorkCalendarCode, x.WorkCalendarName })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            return rows.Select(x => new HolidayResponse
            {
                Id = x.Id,
                WorkCalendarId = x.WorkCalendarId,
                WorkCalendarCode = calendars.GetValueOrDefault(x.WorkCalendarId)?.WorkCalendarCode,
                WorkCalendarName = calendars.GetValueOrDefault(x.WorkCalendarId)?.WorkCalendarName,
                HolidayCode = x.HolidayCode,
                HolidayName = x.HolidayName,
                StartDate = DateOnly.FromDateTime(x.StartDate),
                EndDate = DateOnly.FromDateTime(x.EndDate),
                HolidayType = x.HolidayType,
                HolidayTypeLabel = HolidayTypes.FirstOrDefault(t => t.Value == x.HolidayType).Label ?? x.HolidayType,
                IsNationalHoliday = x.IsNationalHoliday,
                IsPaidHoliday = x.IsPaidHoliday,
                IsRecurringAnnually = x.IsRecurringAnnually,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = actors.GetValueOrDefault(x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
            }).ToList();
        }

        private static string? N(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public class HolidayServiceResult
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public HolidayResponse? Data { get; private set; }

        public static HolidayServiceResult Ok(HolidayResponse data, string message) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Message = message, Data = data };

        public static HolidayServiceResult Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
