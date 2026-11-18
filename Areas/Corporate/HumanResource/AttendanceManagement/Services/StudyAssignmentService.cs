using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using StudyStatus = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.StudyAssignmentStatus;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    /// <summary>
    /// Tugas belajar penuh waktu (HRD-DEC-201, state 30.4, VR-PK-18, VR-PK-19). Admin HRD mencatat dari SK; selama masa
    /// studinya pegawai diblokir dari roster (B8, RosterValidationService) dan rekap hariannya "Tugas Belajar"
    /// (AttendanceProcessingService). Saldo cuti tidak disentuh. Setiap catat/ubah/batal menjadwalkan hitung ulang rekap
    /// harian yang sudah ada pada rentang terdampak (INT-HR-102).
    /// </summary>
    public class StudyAssignmentService
    {
        public const string OverlapMessage = "Pegawai sudah punya tugas belajar aktif pada rentang ini";
        public const string RangeMessage = "Tanggal selesai harus sesudah tanggal mulai";
        public const string ReasonMessage = "Alasan perubahan wajib diisi";

        public static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>
        {
            [StudyStatus.Active] = "Aktif",
            [StudyStatus.Cancelled] = "Dibatalkan"
        };

        private readonly ApplicationDbContext _context;
        private readonly AttendanceSchedulerService _schedulerService;
        private readonly ILogger<StudyAssignmentService> _logger;

        public StudyAssignmentService(ApplicationDbContext context, AttendanceSchedulerService schedulerService, ILogger<StudyAssignmentService> logger)
        {
            _context = context;
            _schedulerService = schedulerService;
            _logger = logger;
        }

        public async Task<StudyAssignmentServiceResult<PagedResult<StudyAssignmentResponse>>> ListAsync(StudyAssignmentQueryRequest request, CancellationToken ct = default)
        {
            var query = _context.HrdStudyAssignments.AsNoTracking().Where(x => !x.IsDelete);
            if (request.WorkforceProfileId.HasValue) query = query.Where(x => x.WorkforceProfileId == request.WorkforceProfileId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status.Trim());
            if (request.ActiveOn.HasValue)
            {
                var d = request.ActiveOn.Value;
                query = query.Where(x => x.Status == StudyStatus.Active && x.StartDate <= d && x.EndDate >= d);
            }

            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var total = await query.CountAsync(ct);
            var items = await Project(query.OrderByDescending(x => x.StartDate).ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(ct);
            Label(items);
            return StudyAssignmentServiceResult<PagedResult<StudyAssignmentResponse>>.Ok(new PagedResult<StudyAssignmentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            }, "Daftar tugas belajar berhasil diambil.");
        }

        public async Task<StudyAssignmentServiceResult<StudyAssignmentResponse>> GetAsync(Guid id, CancellationToken ct = default)
        {
            var data = await LoadAsync(id, ct);
            return data == null
                ? Fail<StudyAssignmentResponse>(StatusCodes.Status404NotFound, "Tugas belajar tidak ditemukan.")
                : StudyAssignmentServiceResult<StudyAssignmentResponse>.Ok(data, "Detail tugas belajar berhasil diambil.");
        }

        public async Task<StudyAssignmentServiceResult<StudyAssignmentResponse>> CreateAsync(CreateStudyAssignmentRequest request, Guid actorUserId, CancellationToken ct = default)
        {
            var (decree, institution, program, error) = Normalize(request.DecreeNumber, request.InstitutionName, request.ProgramName, request.StartDate, request.EndDate);
            if (error != null) return Fail<StudyAssignmentResponse>(StatusCodes.Status400BadRequest, error);

            var employeeActive = await _context.MstWorkforceProfiles.AsNoTracking()
                .Where(x => x.Id == request.WorkforceProfileId && !x.IsDelete)
                .Select(x => (bool?)x.IsActive)
                .FirstOrDefaultAsync(ct);
            if (employeeActive != true) return Fail<StudyAssignmentResponse>(StatusCodes.Status404NotFound, "Pegawai tidak ditemukan atau tidak aktif.");

            if (await OverlapsAsync(request.WorkforceProfileId, request.StartDate, request.EndDate, null, ct))
            {
                return Fail<StudyAssignmentResponse>(StatusCodes.Status422UnprocessableEntity, OverlapMessage); // VR-PK-18
            }

            var entity = new HrdStudyAssignment
            {
                WorkforceProfileId = request.WorkforceProfileId,
                DecreeNumber = decree!,
                InstitutionName = institution!,
                ProgramName = program,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = StudyStatus.Active,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };
            _context.HrdStudyAssignments.Add(entity);
            await _context.SaveChangesAsync(ct);

            return await DoneAsync(entity, entity.StartDate, entity.EndDate, actorUserId, "Tugas belajar dicatat.", ct);
        }

        public async Task<StudyAssignmentServiceResult<StudyAssignmentResponse>> UpdateAsync(Guid id, UpdateStudyAssignmentRequest request, Guid actorUserId, CancellationToken ct = default)
        {
            var reason = request.ChangeReason?.Trim();
            if (string.IsNullOrEmpty(reason)) return Fail<StudyAssignmentResponse>(StatusCodes.Status400BadRequest, ReasonMessage); // VR-PK-19

            var (decree, institution, program, error) = Normalize(request.DecreeNumber, request.InstitutionName, request.ProgramName, request.StartDate, request.EndDate);
            if (error != null) return Fail<StudyAssignmentResponse>(StatusCodes.Status400BadRequest, error);

            var entity = await _context.HrdStudyAssignments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null) return Fail<StudyAssignmentResponse>(StatusCodes.Status404NotFound, "Tugas belajar tidak ditemukan.");
            if (entity.Status != StudyStatus.Active) return Fail<StudyAssignmentResponse>(StatusCodes.Status409Conflict, "Tugas belajar yang dibatalkan tidak dapat diubah.");

            if (await OverlapsAsync(entity.WorkforceProfileId, request.StartDate, request.EndDate, entity.Id, ct))
            {
                return Fail<StudyAssignmentResponse>(StatusCodes.Status422UnprocessableEntity, OverlapMessage); // VR-PK-18
            }

            // Rentang terdampak = gabungan rentang lama dan baru: hari yang keluar dari masa studi kembali dihitung biasa.
            var from = entity.StartDate < request.StartDate ? entity.StartDate : request.StartDate;
            var to = entity.EndDate > request.EndDate ? entity.EndDate : request.EndDate;

            entity.DecreeNumber = decree!;
            entity.InstitutionName = institution!;
            entity.ProgramName = program;
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.LastChangeReason = reason;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return await DoneAsync(entity, from, to, actorUserId, "Tugas belajar diubah.", ct);
        }

        public async Task<StudyAssignmentServiceResult<StudyAssignmentResponse>> CancelAsync(Guid id, CancelStudyAssignmentRequest request, Guid actorUserId, CancellationToken ct = default)
        {
            var reason = request.CancelReason?.Trim();
            if (string.IsNullOrEmpty(reason)) return Fail<StudyAssignmentResponse>(StatusCodes.Status400BadRequest, ReasonMessage); // VR-PK-19

            var entity = await _context.HrdStudyAssignments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null) return Fail<StudyAssignmentResponse>(StatusCodes.Status404NotFound, "Tugas belajar tidak ditemukan.");
            if (entity.Status != StudyStatus.Active) return Fail<StudyAssignmentResponse>(StatusCodes.Status409Conflict, "Tugas belajar sudah dibatalkan.");

            entity.Status = StudyStatus.Cancelled;
            entity.LastChangeReason = reason;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.CancelBy = actorUserId;
            entity.CancelDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return await DoneAsync(entity, entity.StartDate, entity.EndDate, actorUserId, "Tugas belajar dibatalkan.", ct);
        }

        // ---------------------------------------------------------------- Pembantu

        private static (string? Decree, string? Institution, string? Program, string? Error) Normalize(
            string? decreeNumber, string? institutionName, string? programName, DateOnly start, DateOnly end)
        {
            var decree = decreeNumber?.Trim();
            var institution = institutionName?.Trim();
            var program = string.IsNullOrWhiteSpace(programName) ? null : programName.Trim();
            if (string.IsNullOrEmpty(decree)) return (null, null, null, "Nomor SK tugas belajar wajib diisi.");
            if (string.IsNullOrEmpty(institution)) return (null, null, null, "Lembaga pendidikan wajib diisi.");
            if (start == default || end == default) return (null, null, null, "Tanggal mulai dan tanggal selesai wajib diisi.");
            if (end < start) return (null, null, null, RangeMessage); // VR-PK-19
            return (decree, institution, program, null);
        }

        private Task<bool> OverlapsAsync(Guid profileId, DateOnly start, DateOnly end, Guid? exceptId, CancellationToken ct) =>
            _context.HrdStudyAssignments.AsNoTracking().AnyAsync(x =>
                x.WorkforceProfileId == profileId && !x.IsDelete && x.Status == StudyStatus.Active &&
                (exceptId == null || x.Id != exceptId) && x.StartDate <= end && x.EndDate >= start, ct);

        private async Task<StudyAssignmentServiceResult<StudyAssignmentResponse>> DoneAsync(
            HrdStudyAssignment entity, DateOnly from, DateOnly to, Guid actorUserId, string message, CancellationToken ct)
        {
            var scheduled = await ScheduleReprocessAsync(entity, from, to, actorUserId, ct);
            var data = (await LoadAsync(entity.Id, ct))!;
            data.ReprocessScheduled = scheduled;
            return StudyAssignmentServiceResult<StudyAssignmentResponse>.Ok(data, message);
        }

        /// <summary>
        /// INT-HR-102: rekap harian pegawai yang sudah ada dan tidak terkunci pada rentang terdampak dijadwalkan dihitung ulang,
        /// satu job per rekap (pola pelepasan scan mesin). Tanggal tanpa rekap tidak diproses di sini agar tidak lahir rekap baru;
        /// pemrosesan harian membaca tugas belajar saat tanggal itu diproses. Kegagalan tidak membatalkan perubahan.
        /// </summary>
        private async Task<int> ScheduleReprocessAsync(HrdStudyAssignment entity, DateOnly from, DateOnly to, Guid actorUserId, CancellationToken ct)
        {
            var scheduled = 0;
            try
            {
                var dailies = await _context.HrdAttendanceDailies.AsNoTracking()
                    .Where(d => !d.IsDelete && !d.IsLocked && d.WorkforceProfileId == entity.WorkforceProfileId && d.AttendanceDate >= from && d.AttendanceDate <= to)
                    .OrderBy(d => d.AttendanceDate)
                    .Select(d => new { d.Id, d.AttendanceDate, d.ProcessingVersion })
                    .ToListAsync(ct);

                foreach (var daily in dailies)
                {
                    var result = await _schedulerService.EnqueueAsync(new EnqueueAttendanceProcessingJobRequest
                    {
                        StartDate = daily.AttendanceDate,
                        EndDate = daily.AttendanceDate,
                        WorkforceProfileId = entity.WorkforceProfileId,
                        ForceReprocess = true,
                        Priority = 100,
                        CorrelationId = $"ATT-STUDY-{entity.Id:N}-{daily.Id:N}-v{daily.ProcessingVersion}",
                        Notes = "Hitung ulang sesudah tugas belajar dicatat/diubah/dibatalkan (HRD-DEC-201, INT-HR-102)."
                    }, actorUserId, ct);
                    if (result.Success) scheduled++;
                    else _logger.LogWarning("Hitung ulang rekap {DailyId} untuk tugas belajar {StudyId} tidak terjadwal: {Message}", daily.Id, entity.Id, result.Message);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Hitung ulang untuk tugas belajar {StudyId} gagal dijadwalkan; perubahan tetap berlaku.", entity.Id);
            }

            return scheduled;
        }

        private async Task<StudyAssignmentResponse?> LoadAsync(Guid id, CancellationToken ct)
        {
            var data = await Project(_context.HrdStudyAssignments.AsNoTracking().Where(x => x.Id == id && !x.IsDelete)).FirstOrDefaultAsync(ct);
            if (data != null) Label(new List<StudyAssignmentResponse> { data });
            return data;
        }

        private IQueryable<StudyAssignmentResponse> Project(IQueryable<HrdStudyAssignment> query) =>
            from x in query
            join c in _context.Users.AsNoTracking() on x.CreateBy equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            join u in _context.Users.AsNoTracking() on x.UpdateBy equals u.Id into uj
            from u in uj.DefaultIfEmpty()
            select new StudyAssignmentResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : null,
                EmployeeName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : null,
                DecreeNumber = x.DecreeNumber,
                InstitutionName = x.InstitutionName,
                ProgramName = x.ProgramName,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                LastChangeReason = x.LastChangeReason,
                CreatedAt = x.CreateDateTime,
                CreatedBy = x.CreateBy,
                CreatedByName = c != null ? c.DisplayName : null,
                UpdatedAt = x.UpdateDateTime,
                UpdatedBy = x.UpdateDateTime != null ? (Guid?)x.UpdateBy : null,
                UpdatedByName = x.UpdateDateTime != null && u != null ? u.DisplayName : null
            };

        private static void Label(List<StudyAssignmentResponse> rows)
        {
            foreach (var row in rows) row.StatusLabel = StatusLabels.GetValueOrDefault(row.Status, row.Status);
        }

        private static StudyAssignmentServiceResult<T> Fail<T>(int status, string message) => StudyAssignmentServiceResult<T>.Fail(status, message);
    }

    public class StudyAssignmentServiceResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        public static StudyAssignmentServiceResult<T> Ok(T data, string message) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Message = message, Data = data };

        public static StudyAssignmentServiceResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
