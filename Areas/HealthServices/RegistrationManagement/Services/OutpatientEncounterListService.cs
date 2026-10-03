using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>Pengguna tidak terhubung ke dokter, cluster perawat, maupun hak lihat semua (RJDP-VAL-001).</summary>
    public sealed class OutpatientEncounterScopeException : Exception
    {
        public OutpatientEncounterScopeException()
            : base("Akun Anda belum terhubung ke data dokter atau cluster perawat. Hubungi admin untuk pengaturan akses.")
        {
        }
    }

    /// <summary>Saringan atau isian yang tidak valid (RJDP-VAL-007, RJDP-VAL-008).</summary>
    public sealed class OutpatientEncounterValidationException : Exception
    {
        public OutpatientEncounterValidationException(string message) : base(message)
        {
        }
    }

    /// <summary>Kunjungan tidak ada, bukan RJ berklinik, atau di luar cakupan pengguna (RJDP-VAL-003).</summary>
    public sealed class OutpatientEncounterNotFoundException : Exception
    {
        public OutpatientEncounterNotFoundException() : base("Kunjungan tidak ditemukan.")
        {
        }
    }

    /// <summary>
    /// Daftar Pasien Rawat Jalan: daftar, ringkasan, dan metadata filter yang disaring di server
    /// menurut cakupan pengguna (RJ-DOC-DEC-012..014, kontrak <c>RJ-DOC-ENCLIST-001@1.0.0</c>).
    /// </summary>
    public class OutpatientEncounterListService
    {
        public const string CancelAction = "Cancel";
        private const int MaxRangeDays = 31;
        private const int MaxCancelReasonLength = 250;
        private const string LogCategory = "HealthServices.RegistrationManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalActorScopeService _scopeService;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly QueueRealtimeService _queueRealtimeService;
        private readonly LoggerService _loggerService;

        public OutpatientEncounterListService(
            ApplicationDbContext dbContext,
            ClinicalActorScopeService scopeService,
            AccessPermissionService accessPermissionService,
            QueueRealtimeService queueRealtimeService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _scopeService = scopeService;
            _accessPermissionService = accessPermissionService;
            _queueRealtimeService = queueRealtimeService;
            _loggerService = loggerService;
        }

        public async Task<PagedResult<OutpatientEncounterListItemResponse>> GetPagedAsync(
            ClaimsPrincipal user,
            OutpatientEncounterListQuery request,
            CancellationToken cancellationToken = default)
        {
            var scope = await ResolveScopeOrThrowAsync(user, cancellationToken);
            var today = Today();

            var query = ApplyFilters(BuildScopedQuery(scope), request, today, includeStatus: true);

            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var totalData = await query.CountAsync(cancellationToken);

            var rows = await query
                .OrderByDescending(x => x.EncounterDate)
                .ThenByDescending(x => x.RegisteredAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.EncounterNumber,
                    x.EncounterDate,
                    x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    ClinicId = x.ClinicId!.Value,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : string.Empty,
                    x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    PaymentSourceName = x.PaymentSource != null ? x.PaymentSource.PaymentSourceNameSnapshot : null,
                    x.PaymentType,
                    x.EncounterStatus,
                    x.IsCancel,
                    IsCompleted = x.CompletedAt != null,
                    HasActiveConsultation = _dbContext.Set<TrxDoctorConsultation>()
                        .Any(c => c.EncounterId == x.Id && !c.IsDelete && !c.IsCancel)
                })
                .ToListAsync(cancellationToken);

            var canCancelPermission = await _accessPermissionService.HasAccessAsync(
                user, ClinicalActorScopeService.ResourceName, CancelAction);

            var items = rows.Select(x =>
            {
                var blockedReason = GetCancelBlockedReason(x.EncounterStatus, x.IsCancel, x.IsCompleted, x.HasActiveConsultation);
                var isBlocking = !x.IsCancel && !x.IsCompleted && x.EncounterStatus <= OutpatientEncounterRules.LastBlockingStatus;

                return new OutpatientEncounterListItemResponse
                {
                    Id = x.Id,
                    EncounterNumber = x.EncounterNumber,
                    EncounterDate = x.EncounterDate,
                    PatientId = x.PatientId,
                    PatientName = x.PatientName,
                    MedicalRecordNumber = x.MedicalRecordNumber,
                    ClinicId = x.ClinicId,
                    ClinicName = x.ClinicName,
                    DoctorId = x.DoctorId,
                    DoctorName = x.DoctorName,
                    PaymentLabel = !string.IsNullOrWhiteSpace(x.PaymentSourceName)
                        ? x.PaymentSourceName!
                        : GetDisplayName(x.PaymentType),
                    EncounterStatus = (int)x.EncounterStatus,
                    EncounterStatusName = GetDisplayName(x.EncounterStatus),
                    IsCancelled = x.IsCancel,
                    IsHanging = isBlocking && x.EncounterDate < today,
                    HasActiveConsultation = x.HasActiveConsultation,
                    CanCancel = canCancelPermission && blockedReason == null,
                    CancelBlockedReason = blockedReason
                };
            }).ToList();

            return new PagedResult<OutpatientEncounterListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<OutpatientEncounterSummaryResponse> GetSummaryAsync(
            ClaimsPrincipal user,
            OutpatientEncounterListQuery request,
            CancellationToken cancellationToken = default)
        {
            var scope = await ResolveScopeOrThrowAsync(user, cancellationToken);
            var today = Today();
            var scoped = BuildScopedQuery(scope);

            var groups = await ApplyFilters(scoped, request, today, includeStatus: false)
                .GroupBy(x => new { x.EncounterStatus, x.IsCancel, IsCompleted = x.CompletedAt != null })
                .Select(g => new { g.Key.EncounterStatus, g.Key.IsCancel, g.Key.IsCompleted, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var result = new OutpatientEncounterSummaryResponse();

            foreach (var group in groups)
            {
                var isClosed = group.IsCancel ||
                    group.IsCompleted ||
                    group.EncounterStatus is EncounterStatus.Completed or EncounterStatus.Cancelled or EncounterStatus.NoShow;

                if (isClosed)
                    result.Closed += group.Count;
                else if (group.EncounterStatus <= EncounterStatus.WaitingForDoctor)
                    result.Waiting += group.Count;
                else if (group.EncounterStatus == EncounterStatus.InConsultation)
                    result.InConsultation += group.Count;
                else
                    result.ReadyForBilling += group.Count;
            }

            // Menggantung selalu dihitung lintas tanggal, tidak terpengaruh mode (RJ-DOC-FE-007).
            result.Hanging = await scoped
                .Where(OutpatientEncounterRules.IsBlockingState)
                .Where(x => x.EncounterDate < today)
                .CountAsync(cancellationToken);

            return result;
        }

        public async Task<OutpatientEncounterFilterMetadataResponse> GetFilterMetadataAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var scope = await ResolveScopeOrThrowAsync(user, cancellationToken);

            var statusOptions = Enum.GetValues<EncounterStatus>()
                .Select(x => new OutpatientEncounterOptionResponse { Value = ((int)x).ToString(), Label = GetDisplayName(x) })
                .ToList();

            List<OutpatientEncounterOptionResponse> clinicOptions;
            var doctorOptions = new List<OutpatientEncounterOptionResponse>();

            if (scope.CanReadAll)
            {
                clinicOptions = await _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.ClinicName)
                    .Select(x => new OutpatientEncounterOptionResponse { Value = x.Id.ToString(), Label = x.ClinicName })
                    .ToListAsync(cancellationToken);

                doctorOptions = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.FullName)
                    .Select(x => new OutpatientEncounterOptionResponse { Value = x.Id.ToString(), Label = x.FullName })
                    .ToListAsync(cancellationToken);
            }
            else
            {
                var clinicIds = await BuildScopedQuery(scope)
                    .Select(x => x.ClinicId!.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                clinicIds = clinicIds.Union(scope.ClinicIds).ToList();

                clinicOptions = await _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && clinicIds.Contains(x.Id))
                    .OrderBy(x => x.ClinicName)
                    .Select(x => new OutpatientEncounterOptionResponse { Value = x.Id.ToString(), Label = x.ClinicName })
                    .ToListAsync(cancellationToken);
            }

            return new OutpatientEncounterFilterMetadataResponse
            {
                Scope = new OutpatientEncounterScopeResponse { CanReadAll = scope.CanReadAll, Label = scope.Label },
                StatusOptions = statusOptions,
                ClinicOptions = clinicOptions,
                DoctorOptions = doctorOptions
            };
        }

        /// <summary>
        /// Membatalkan satu kunjungan dari Daftar Pasien Rawat Jalan (RJ-DOC-DEC-015..018, 021).
        /// </summary>
        /// <remarks>
        /// Baris kunjungan dikunci <c>FOR UPDATE</c> lalu aturan boleh-batal diperiksa ulang di dalam
        /// transaksi, termasuk konsultasi aktif, supaya dua petugas atau dokter yang bergerak
        /// bersamaan tidak membatalkan kunjungan yang keadaannya sudah berubah. Kolom yang diisi sama
        /// dengan <c>PATCH /patient-encounters/{id}/cancel</c>; <c>EncounterStatus</c> tidak diubah.
        /// Notifikasi antrean dikirim setelah commit; kegagalannya tidak membatalkan pembatalan.
        /// </remarks>
        public async Task<OutpatientEncounterCancelResponse> CancelAsync(
            ClaimsPrincipal user,
            Guid id,
            OutpatientEncounterCancelRequest? request,
            CancellationToken cancellationToken = default)
        {
            var scope = await ResolveScopeOrThrowAsync(user, cancellationToken);

            var reason = request?.CancelReason?.Trim();
            if (string.IsNullOrEmpty(reason) || reason.Length > MaxCancelReasonLength)
                throw new OutpatientEncounterValidationException("Alasan pembatalan wajib diisi, maksimal 250 karakter.");

            var visible = await BuildScopedQuery(scope).AnyAsync(x => x.Id == id, cancellationToken);
            if (!visible)
                throw new OutpatientEncounterNotFoundException();

            var actorUserId = GetUserId(user);
            var now = DateTime.UtcNow;
            List<TrxQueue> cancelledQueues;

            await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                var encounter = await _dbContext.Set<RegPatientEncounter>()
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM public.""RegPatientEncounter""
                        WHERE ""Id"" = {id}
                          AND ""IsDelete"" = false
                        FOR UPDATE")
                    .FirstOrDefaultAsync(cancellationToken);

                if (encounter == null)
                    throw new OutpatientEncounterNotFoundException();

                var hasActiveConsultation = await _dbContext.Set<TrxDoctorConsultation>()
                    .AnyAsync(c => c.EncounterId == id && !c.IsDelete && !c.IsCancel, cancellationToken);

                var blockedReason = GetCancelBlockedReason(
                    encounter.EncounterStatus,
                    encounter.IsCancel,
                    encounter.CompletedAt.HasValue,
                    hasActiveConsultation);

                if (blockedReason != null)
                    throw new OutpatientEncounterValidationException(blockedReason);

                encounter.CancelledAt = now;
                encounter.CancelledByUserId = actorUserId;
                encounter.CancelReason = reason;
                encounter.IsCancel = true;
                encounter.CancelDateTime = now;
                encounter.CancelBy = actorUserId;
                encounter.IsActive = false;
                encounter.UpdateDateTime = now;
                encounter.UpdateBy = actorUserId;

                cancelledQueues = await _dbContext.Set<TrxQueue>()
                    .Where(x => x.EncounterId == id && !x.IsDelete && !x.CompletedAt.HasValue && !x.CancelledAt.HasValue)
                    .ToListAsync(cancellationToken);

                foreach (var queue in cancelledQueues)
                {
                    queue.CancelledAt = now;
                    queue.CancelledByUserId = actorUserId;
                    queue.CancelReason = reason;
                    queue.IsCancel = true;
                    queue.CancelDateTime = now;
                    queue.CancelBy = actorUserId;
                    queue.IsActive = false;
                    queue.UpdateDateTime = now;
                    queue.UpdateBy = actorUserId;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            foreach (var queue in cancelledQueues)
            {
                try
                {
                    await _queueRealtimeService.NotifyQueueCancelledAsync(queue, actorUserId, "Kunjungan rawat jalan dibatalkan.");
                }
                catch (Exception exception)
                {
                    await _loggerService.WarningAsync(LogCategory, "CancelOutpatientEncounter",
                        "Notifikasi antrean batal gagal dikirim.", new { EntityId = queue.Id, Error = exception.GetType().Name });
                }
            }

            // Tanpa alasan batal, nama pasien, maupun data medis.
            await _loggerService.AuditAsync(LogCategory, "CancelOutpatientEncounter", "Kunjungan rawat jalan dibatalkan.",
                new { EntityId = id, Controller = "OutpatientEncounter", Action = "Cancel", Status = "Cancelled", QueueCount = cancelledQueues.Count });

            return new OutpatientEncounterCancelResponse
            {
                Id = id,
                CancelledAt = now,
                CancelledQueueCount = cancelledQueues.Count
            };
        }

        /// <summary>
        /// Alasan sebuah kunjungan tidak dapat dibatalkan, atau <c>null</c> bila statusnya
        /// mengizinkan (RJ-DOC-DEC-016, RJ-DOC-DEC-021). Bunyinya sama dengan validation matrix.
        /// </summary>
        public static string? GetCancelBlockedReason(
            EncounterStatus status,
            bool isCancelled,
            bool isCompleted,
            bool hasActiveConsultation)
        {
            if (isCancelled)
                return "Kunjungan sudah dibatalkan.";

            if (isCompleted || status > OutpatientEncounterRules.LastBlockingStatus)
                return $"Kunjungan dengan status {GetDisplayName(status)} tidak dapat dibatalkan.";

            if (status == EncounterStatus.InConsultation && hasActiveConsultation)
                return "Konsultasi masih aktif. Selesaikan atau batalkan konsultasi lewat workspace dokter.";

            return null;
        }

        internal async Task<ClinicalActorScope> ResolveScopeOrThrowAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var scope = await _scopeService.ResolveAsync(user, cancellationToken);

            if (!scope.HasAnyScope)
                throw new OutpatientEncounterScopeException();

            return scope;
        }

        internal IQueryable<RegPatientEncounter> BuildScopedQuery(ClinicalActorScope scope)
        {
            var query = _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .WhereOutpatientClinicEncounter(_dbContext);

            if (scope.CanReadAll)
                return query;

            var hasDoctor = scope.DoctorId.HasValue;
            var doctorId = scope.DoctorId ?? Guid.Empty;
            var clinicIds = scope.ClinicIds.ToList();

            return query.Where(x =>
                (hasDoctor && x.DoctorId == doctorId) ||
                clinicIds.Contains(x.ClinicId!.Value));
        }

        private static IQueryable<RegPatientEncounter> ApplyFilters(
            IQueryable<RegPatientEncounter> query,
            OutpatientEncounterListQuery request,
            DateTime today,
            bool includeStatus)
        {
            var mode = (request.Mode ?? "today").Trim().ToLowerInvariant();

            if (request.HangingOnly)
            {
                query = query
                    .Where(OutpatientEncounterRules.IsBlockingState)
                    .Where(x => x.EncounterDate < today);
            }
            else if (mode == "active")
            {
                query = query.Where(OutpatientEncounterRules.IsBlockingState);
            }
            else if (mode == "range")
            {
                if (!request.DateFrom.HasValue || !request.DateTo.HasValue)
                    throw new OutpatientEncounterValidationException("Rentang tanggal tidak valid. Maksimal 31 hari.");

                var from = DateTime.SpecifyKind(request.DateFrom.Value.Date, DateTimeKind.Utc);
                var to = DateTime.SpecifyKind(request.DateTo.Value.Date, DateTimeKind.Utc);

                if (from > to || (to - from).TotalDays > MaxRangeDays - 1)
                    throw new OutpatientEncounterValidationException("Rentang tanggal tidak valid. Maksimal 31 hari.");

                query = query.Where(x => x.EncounterDate >= from && x.EncounterDate < to.AddDays(1));
            }
            else if (mode == "today")
            {
                var tomorrow = today.AddDays(1);
                query = query.Where(x => x.EncounterDate >= today && x.EncounterDate < tomorrow);
            }
            else
            {
                throw new OutpatientEncounterValidationException("Saringan tidak valid.");
            }

            if (includeStatus && request.EncounterStatus is { Count: > 0 })
            {
                var statuses = request.EncounterStatus
                    .Where(x => Enum.IsDefined(typeof(EncounterStatus), x))
                    .Select(x => (EncounterStatus)x)
                    .ToList();

                if (statuses.Count != request.EncounterStatus.Count)
                    throw new OutpatientEncounterValidationException("Saringan tidak valid.");

                query = query.Where(x => statuses.Contains(x.EncounterStatus));
            }

            // Bagi pengguna tanpa hak lihat semua, kedua saringan ini hanya mempersempit cakupan.
            if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
            {
                var clinicId = request.ClinicId.Value;
                query = query.Where(x => x.ClinicId == clinicId);
            }

            if (request.DoctorId.HasValue && request.DoctorId.Value != Guid.Empty)
            {
                var doctorId = request.DoctorId.Value;
                query = query.Where(x => x.DoctorId == doctorId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim().ToLower();

                if (keyword.Length > 100)
                    throw new OutpatientEncounterValidationException("Saringan tidak valid.");

                query = query.Where(x =>
                    x.EncounterNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)));
            }

            return query;
        }

        /// <summary>
        /// Tanggal operasional hari ini dalam bentuk yang dipakai kolom <c>EncounterDate</c>:
        /// tanggal WIB yang distempel tengah malam UTC.
        /// </summary>
        private static DateTime Today() =>
            DateTime.SpecifyKind(AppDateTimeHelper.OperationalDate(), DateTimeKind.Utc);

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private static string GetDisplayName<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }
}
