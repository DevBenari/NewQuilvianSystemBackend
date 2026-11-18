using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using IncidentStatus = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.DeviceIncidentStatus;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    /// <summary>
    /// Kejadian gangguan mesin absen (HRD-DEC-202, state 30.5, VR-PK-20). Petugas IT atau Admin HRD mencatatnya sekali;
    /// pegawai terdampak mengajukan koreksi presensi biasa yang merujuk kejadian ini (AttendanceCorrectionService).
    /// Layanan ini TIDAK PERNAH menulis HrdAttendanceRawLog (INV-A52-07).
    /// </summary>
    public class AttendanceDeviceIncidentService
    {
        public const string RangeMessage = "Jam selesai gangguan harus sesudah jam mulai";
        private static readonly TimeSpan WibOffset = TimeSpan.FromHours(7);

        public static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>
        {
            [IncidentStatus.Active] = "Aktif",
            [IncidentStatus.Cancelled] = "Dibatalkan"
        };

        private readonly ApplicationDbContext _context;

        public AttendanceDeviceIncidentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DeviceIncidentServiceResult<PagedResult<DeviceIncidentResponse>>> ListAsync(DeviceIncidentQueryRequest request, CancellationToken ct = default)
        {
            if (request.FromDate.HasValue && request.ToDate.HasValue && request.ToDate < request.FromDate)
            {
                return DeviceIncidentServiceResult<PagedResult<DeviceIncidentResponse>>.Fail(StatusCodes.Status400BadRequest, "Tanggal selesai tidak boleh sebelum tanggal mulai.");
            }

            var query = _context.HrdAttendanceDeviceIncidents.AsNoTracking().Where(x => !x.IsDelete);
            if (request.AttendanceDeviceId.HasValue) query = query.Where(x => x.AttendanceDeviceId == request.AttendanceDeviceId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status.Trim());
            if (request.FromDate.HasValue)
            {
                var fromUtc = WibMidnightUtc(request.FromDate.Value);
                query = query.Where(x => x.EndAt > fromUtc);
            }

            if (request.ToDate.HasValue)
            {
                var toUtc = WibMidnightUtc(request.ToDate.Value.AddDays(1));
                query = query.Where(x => x.StartAt < toUtc);
            }

            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var total = await query.CountAsync(ct);
            var items = await Project(query.OrderByDescending(x => x.StartAt).ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(ct);
            Finish(items);
            return DeviceIncidentServiceResult<PagedResult<DeviceIncidentResponse>>.Ok(new PagedResult<DeviceIncidentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            }, "Daftar kejadian gangguan mesin berhasil diambil.");
        }

        public async Task<DeviceIncidentServiceResult<DeviceIncidentResponse>> CreateAsync(CreateDeviceIncidentRequest request, Guid actorUserId, CancellationToken ct = default)
        {
            var description = request.Description?.Trim();
            if (string.IsNullOrEmpty(description)) return Fail(StatusCodes.Status400BadRequest, "Uraian gangguan wajib diisi.");
            if (request.StartAt == default || request.EndAt == default) return Fail(StatusCodes.Status400BadRequest, "Jam mulai dan jam selesai gangguan wajib diisi.");

            var startAt = ToUtc(request.StartAt);
            var endAt = ToUtc(request.EndAt);
            if (endAt <= startAt) return Fail(StatusCodes.Status400BadRequest, RangeMessage); // VR-PK-20

            // State 30.5: mesin harus terdaftar (master mesin absen, BE-HRD-346).
            var registered = await _context.MstAttendanceDevices.AsNoTracking().AnyAsync(x => x.Id == request.AttendanceDeviceId && !x.IsDelete, ct);
            if (!registered) return Fail(StatusCodes.Status404NotFound, "Mesin absen tidak terdaftar.");

            var entity = new HrdAttendanceDeviceIncident
            {
                AttendanceDeviceId = request.AttendanceDeviceId,
                StartAt = startAt,
                EndAt = endAt,
                Description = description,
                ReportedByUserId = actorUserId,
                Status = IncidentStatus.Active,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };
            _context.HrdAttendanceDeviceIncidents.Add(entity);
            await _context.SaveChangesAsync(ct);

            return DeviceIncidentServiceResult<DeviceIncidentResponse>.Ok((await LoadAsync(entity.Id, ct))!, "Kejadian gangguan mesin dicatat.");
        }

        public async Task<DeviceIncidentServiceResult<DeviceIncidentResponse>> CancelAsync(Guid id, CancelDeviceIncidentRequest request, Guid actorUserId, CancellationToken ct = default)
        {
            var reason = request.CancelReason?.Trim();
            if (string.IsNullOrEmpty(reason)) return Fail(StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");

            var entity = await _context.HrdAttendanceDeviceIncidents.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null) return Fail(StatusCodes.Status404NotFound, "Kejadian gangguan tidak ditemukan.");
            if (entity.Status != IncidentStatus.Active) return Fail(StatusCodes.Status409Conflict, "Kejadian gangguan sudah dibatalkan.");

            // Koreksi yang sudah merujuk kejadian ini tidak diubah; koreksi baru yang merujuknya ditolak (VR-PK-21).
            entity.Status = IncidentStatus.Cancelled;
            entity.CancelReason = reason;
            entity.CancelBy = actorUserId;
            entity.CancelDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return DeviceIncidentServiceResult<DeviceIncidentResponse>.Ok((await LoadAsync(entity.Id, ct))!, "Kejadian gangguan mesin dibatalkan.");
        }

        /// <summary>Kejadian Active yang bersinggungan dengan tanggal kerja (WIB) itu, untuk dipilih di formulir koreksi.</summary>
        public async Task<List<DeviceIncidentOptionResponse>> GetOptionsAsync(DateOnly attendanceDate, CancellationToken ct = default)
        {
            var fromUtc = WibMidnightUtc(attendanceDate);
            var toUtc = WibMidnightUtc(attendanceDate.AddDays(1));
            var rows = await _context.HrdAttendanceDeviceIncidents.AsNoTracking()
                .Where(x => !x.IsDelete && x.Status == IncidentStatus.Active && x.StartAt < toUtc && x.EndAt > fromUtc)
                .OrderBy(x => x.StartAt)
                .Select(x => new
                {
                    x.Id,
                    x.AttendanceDeviceId,
                    x.StartAt,
                    x.EndAt,
                    x.Description,
                    Code = x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceCode : null,
                    Name = x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceName : null
                })
                .ToListAsync(ct);

            return rows.Select(x =>
            {
                var start = DateTime.SpecifyKind(x.StartAt, DateTimeKind.Utc);
                var end = DateTime.SpecifyKind(x.EndAt, DateTimeKind.Utc);
                return new DeviceIncidentOptionResponse
                {
                    Value = x.Id,
                    AttendanceDeviceId = x.AttendanceDeviceId,
                    StartAt = start,
                    EndAt = end,
                    Label = $"{x.Code} {x.Name} — {start.Add(WibOffset):dd-MM-yyyy HH.mm}–{end.Add(WibOffset):HH.mm} — {x.Description}".Trim()
                };
            }).ToList();
        }

        private async Task<DeviceIncidentResponse?> LoadAsync(Guid id, CancellationToken ct)
        {
            var data = await Project(_context.HrdAttendanceDeviceIncidents.AsNoTracking().Where(x => x.Id == id && !x.IsDelete)).FirstOrDefaultAsync(ct);
            if (data != null) Finish(new List<DeviceIncidentResponse> { data });
            return data;
        }

        private IQueryable<DeviceIncidentResponse> Project(IQueryable<HrdAttendanceDeviceIncident> query) =>
            from x in query
            join u in _context.Users.AsNoTracking() on x.ReportedByUserId equals u.Id into uj
            from u in uj.DefaultIfEmpty()
            select new DeviceIncidentResponse
            {
                Id = x.Id,
                AttendanceDeviceId = x.AttendanceDeviceId,
                AttendanceDeviceCode = x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceCode : null,
                AttendanceDeviceName = x.AttendanceDevice != null ? x.AttendanceDevice.AttendanceDeviceName : null,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                Description = x.Description,
                Status = x.Status,
                ReportedByUserId = x.ReportedByUserId,
                ReportedByName = u != null ? u.DisplayName : null,
                CancelReason = x.CancelReason,
                CreatedAt = x.CreateDateTime,
                ReferencedCorrectionCount = _context.HrdAttendanceCorrectionRequests.Count(c => c.DeviceIncidentId == x.Id && !c.IsDelete)
            };

        private static void Finish(List<DeviceIncidentResponse> rows)
        {
            foreach (var row in rows)
            {
                row.StartAt = DateTime.SpecifyKind(row.StartAt, DateTimeKind.Utc);
                row.EndAt = DateTime.SpecifyKind(row.EndAt, DateTimeKind.Utc);
                row.StatusLabel = StatusLabels.GetValueOrDefault(row.Status, row.Status);
            }
        }

        private static DateTime WibMidnightUtc(DateOnly date) => DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc) - WibOffset;

        /// <summary>Nilai tanpa zona dibaca sebagai jam WIB (sama dengan AppDateTimeHelper.ToUtc).</summary>
        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value - WibOffset, DateTimeKind.Utc)
        };

        private static DeviceIncidentServiceResult<DeviceIncidentResponse> Fail(int status, string message) => DeviceIncidentServiceResult<DeviceIncidentResponse>.Fail(status, message);
    }

    public class DeviceIncidentServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static DeviceIncidentServiceResult<T> Ok(T data, string message) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Message = message, Data = data };

        public static DeviceIncidentServiceResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
