using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    public enum PriorityReservationOutcome
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        Conflict = 3,
        Forbidden = 4
    }

    /// <summary>
    /// Hasil perintah reservasi. <see cref="ArrivalCode"/> hanya terisi pada buat, kirim ulang, dan
    /// ganti tanggal, hanya hidup di memori, dan tidak pernah disimpan maupun dicatat ke log (INV-RP-05).
    /// </summary>
    public sealed class PriorityReservationCommandResult
    {
        public PriorityReservationOutcome Outcome { get; init; }

        public string Message { get; init; } = string.Empty;

        public RegPriorityReservation? Reservation { get; init; }

        public string? ArrivalCode { get; init; }

        /// <summary>Kode kejadian riwayat yang baru ditulis, untuk kabar realtime.</summary>
        public string EventCode { get; init; } = string.Empty;

        /// <summary>Tanggal kunjungan sebelum diganti, hanya pada ganti tanggal, untuk kabar realtime.</summary>
        public DateTime? PreviousVisitDate { get; init; }

        public bool IsSuccess => Outcome == PriorityReservationOutcome.Success;

        public static PriorityReservationCommandResult Fail(PriorityReservationOutcome outcome, string message) =>
            new() { Outcome = outcome, Message = message };
    }

    /// <summary>
    /// Hasil pemeriksaan sebelum reservasi dilengkapi menjadi kunjungan (INT-RP-03). Klasifikasi
    /// antrean diambil dari snapshot reservasi, bukan dinilai ulang (RJ-PRI-DEC-007, RP-D).
    /// </summary>
    public sealed class PriorityReservationCompletion
    {
        public Guid ReservationId { get; init; }

        public Guid PatientId { get; init; }

        public Guid ExpectedRowVersion { get; init; }

        public OutpatientQueueClassification Classification { get; init; } = OutpatientQueueClassification.Regular;
    }

    public sealed class PriorityReservationCompletionResult
    {
        public PriorityReservationOutcome Outcome { get; init; }

        public string Message { get; init; } = string.Empty;

        public PriorityReservationCompletion? Completion { get; init; }

        public bool IsSuccess => Outcome == PriorityReservationOutcome.Success;

        public static PriorityReservationCompletionResult Fail(PriorityReservationOutcome outcome, string message) =>
            new() { Outcome = outcome, Message = message };
    }

    /// <summary>
    /// Reservasi berubah keadaan di antara pemeriksaan dan penyimpanan kunjungan. Pemanggil wajib
    /// membatalkan seluruh transaksi pendaftaran dan menjawab 409.
    /// </summary>
    public sealed class PriorityReservationConflictException : Exception
    {
        public PriorityReservationConflictException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Reservasi Pasien Prioritas Rawat Jalan (rawat-jalan Amendment RP). Reservasi adalah janji
    /// kedatangan untuk satu tanggal, sebelum poli dan dokter dipilih; ia bukan kunjungan,
    /// sehingga tidak membuat antrean maupun tagihan (RJ-PRI-DEC-009).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kelayakan dinilai lewat <see cref="OutpatientQueueClassificationService"/> pada hari
    /// pembuatan (WIB). Hasilnya disimpan sebagai snapshot. Satu-satunya yang menggantinya adalah
    /// ganti tanggal, dengan penilaian pada hari penggantian (INV-RP-03, RJ-PRI-DEC-035).
    /// </para>
    /// <para>
    /// Kode link dibuat dan di-hash di sini. Kode asli hanya dikembalikan sekali kepada pemanggil;
    /// yang tersimpan hanya hex SHA-256 (INV-RP-05).
    /// </para>
    /// </remarks>
    public class OutpatientPriorityReservationService
    {
        public const string ReservationSequenceKey = "REG_PRIORITY_RESERVATION_NUMBER";
        public const string ReservationNumberPrefix = "RSV";
        private const int ReservationSequenceDigits = 4;
        private const int ArrivalCodeByteLength = 32;
        private const int MinimumSearchLength = 3;
        private const int MaximumPageSize = 100;

        /// <summary>Kunci per pasien yang sama dengan pendaftaran kunjungan (RJ-DOC-REV-BE-007).</summary>
        private const string PatientLockKeyPrefix = "REG_ENCOUNTER_ACTIVE_";

        public const string PatientNotFoundMessage = "Data pasien tidak ditemukan.";
        public const string NotActiveMemberMessage =
            "Pasien bukan member aktif hari ini, sehingga tidak bisa reservasi prioritas. " +
            "Daftarkan lewat Pendaftaran Pasien dengan memilih poli dan dokter.";
        public const string PastDateMessage = "Tanggal kunjungan tidak boleh sebelum hari ini.";
        public const string DisabledMessage = "Reservasi prioritas sedang dinonaktifkan.";
        public const string NumberFailedMessage = "Nomor reservasi gagal diterbitkan. Hubungi administrator sistem.";
        public const string SearchTooShortMessage = "Kata kunci pencarian minimal 3 karakter.";
        public const string InvalidDateRangeMessage = "Tanggal awal tidak boleh setelah tanggal akhir.";
        public const string ReservationNotFoundMessage = "Reservasi prioritas tidak ditemukan.";
        public const string StaleMessage = "Reservasi baru saja diubah petugas lain. Muat ulang lalu coba lagi.";
        public const string ReasonInvalidMessage = "Alasan wajib diisi, 10 sampai 500 karakter.";
        public const string ScreeningIncompleteMessage = "Isi ketiga pertanyaan skrining: batuk, demam, dan risiko jatuh.";
        public const string RescheduleOnVisitDayMessage =
            "Tanggal reservasi tidak bisa diganti pada hari kunjungan. Batalkan reservasi lalu buat reservasi baru.";
        public const string RescheduleSameDateMessage = "Tanggal baru sama dengan tanggal reservasi sekarang.";
        public const string RescheduleDateRequiredMessage = "Tanggal kunjungan baru wajib diisi.";
        private const int MinimumReasonLength = 10;
        private const int MaximumReasonLength = 500;

        private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");

        private readonly ApplicationDbContext _dbContext;
        private readonly PatientMembershipStatusResolver _membershipStatusResolver;
        private readonly OutpatientQueueClassificationService _classificationService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly PriorityReservationOptions _options;
        private readonly TimeProvider _timeProvider;

        public OutpatientPriorityReservationService(
            ApplicationDbContext dbContext,
            PatientMembershipStatusResolver membershipStatusResolver,
            OutpatientQueueClassificationService classificationService,
            NumberSeriesAllocator numberSeriesAllocator,
            AccessPermissionService accessPermissionService,
            IOptions<PriorityReservationOptions> options,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _membershipStatusResolver = membershipStatusResolver;
            _classificationService = classificationService;
            _numberSeriesAllocator = numberSeriesAllocator;
            _accessPermissionService = accessPermissionService;
            _options = options.Value;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        // ------------------------------------------------------------------
        // Kueri
        // ------------------------------------------------------------------

        /// <summary>Daftar reservasi. Mengembalikan pesan galat bila penyaring tidak sah.</summary>
        public async Task<(PagedResult<PriorityReservationListItemResponse>? Result, string? Error)> GetPagedAsync(
            PriorityReservationListQuery query,
            CancellationToken cancellationToken = default)
        {
            var today = _membershipStatusResolver.GetTodayWib();
            var from = ToCalendarDate(query.VisitDateFrom ?? today);
            var to = ToCalendarDate(query.VisitDateTo ?? query.VisitDateFrom ?? today);

            if (from > to)
                return (null, InvalidDateRangeMessage);

            var search = query.Search?.Trim();
            if (!string.IsNullOrEmpty(search) && search.Length < MinimumSearchLength)
                return (null, SearchTooShortMessage);

            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, MaximumPageSize);

            var source = _dbContext.Set<RegPriorityReservation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.VisitDate >= from && x.VisitDate <= to);

            if (query.Status.HasValue)
                source = source.Where(x => x.Status == query.Status.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = BuildContainsPattern(search);
                source = source.Where(x =>
                    EF.Functions.ILike(x.ReservationNumber, pattern, "\\") ||
                    (x.Patient != null && EF.Functions.ILike(x.Patient.FullName, pattern, "\\")) ||
                    (x.Patient != null && EF.Functions.ILike(x.Patient.MedicalRecordNumber, pattern, "\\")));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var rows = await source
                .OrderBy(x => x.VisitDate)
                .ThenBy(x => x.ReservationNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ListItemProjection)
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
                row.StatusName = GetStatusName(row.Status);

            await FillAuditNamesAsync(rows, cancellationToken);

            return (new PagedResult<PriorityReservationListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows
            }, null);
        }

        /// <summary>
        /// Mengisi nama pembuat dan pengubah terakhir untuk satu halaman daftar (kontrak <c>1.1.0</c>).
        /// Pengubah diambil dari riwayat terakhir: petugas memakai namanya, sedangkan perubahan dari
        /// link pasien atau penutupan otomatis memakai nama sumbernya.
        /// </summary>
        private async Task FillAuditNamesAsync(
            List<PriorityReservationListItemResponse> rows,
            CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
                return;

            var ids = rows.Select(x => x.Id).ToList();
            var audits = await _dbContext.Set<RegPriorityReservation>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.CreateBy,
                    LastChange = x.Histories
                        .Where(h => !h.IsDelete)
                        .OrderByDescending(h => h.OccurredAt)
                        .Select(h => new { h.Source, h.ActorUserId })
                        .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var userIds = audits.Values
                .SelectMany(x => new[] { x.CreateBy, x.LastChange?.ActorUserId ?? Guid.Empty })
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var userNames = userIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => userIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            foreach (var row in rows)
            {
                if (!audits.TryGetValue(row.Id, out var audit))
                    continue;

                row.CreatedByName = userNames.GetValueOrDefault(audit.CreateBy);

                if (row.UpdatedAt == null || audit.LastChange == null)
                    continue;

                row.UpdatedByName = audit.LastChange.Source == PriorityReservationEventSource.Staff
                    ? userNames.GetValueOrDefault(audit.LastChange.ActorUserId ?? Guid.Empty)
                    : GetEnumDisplayName(audit.LastChange.Source);
            }
        }

        public async Task<(PriorityReservationSummaryResponse? Result, string? Error)> GetSummaryAsync(
            DateTime? visitDateFrom,
            DateTime? visitDateTo,
            CancellationToken cancellationToken = default)
        {
            var today = _membershipStatusResolver.GetTodayWib();
            var from = ToCalendarDate(visitDateFrom ?? today);
            var to = ToCalendarDate(visitDateTo ?? visitDateFrom ?? today);

            if (from > to)
                return (null, InvalidDateRangeMessage);

            var counts = await _dbContext.Set<RegPriorityReservation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.VisitDate >= from && x.VisitDate <= to)
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

            int Count(PriorityReservationStatus status) => counts.GetValueOrDefault(status);

            return (new PriorityReservationSummaryResponse
            {
                VisitDate = from,
                VisitDateFrom = from,
                VisitDateTo = to,
                AwaitingArrival = Count(PriorityReservationStatus.AwaitingArrival),
                Arrived = Count(PriorityReservationStatus.Arrived),
                Converted = Count(PriorityReservationStatus.Converted),
                Cancelled = Count(PriorityReservationStatus.Cancelled),
                NoShow = Count(PriorityReservationStatus.NoShow),
                RegisteredElsewhere = Count(PriorityReservationStatus.RegisteredElsewhere)
            }, null);
        }

        public async Task<PriorityReservationDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var reservation = await _dbContext.Set<RegPriorityReservation>()
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.PatientMembershipSnapshot)
                .Include(x => x.Histories)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (reservation == null)
                return null;

            var userIds = reservation.Histories
                .Where(x => x.ActorUserId.HasValue)
                .Select(x => x.ActorUserId.GetValueOrDefault())
                .Append(reservation.ScreenedByUserId ?? Guid.Empty)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var userNames = userIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => userIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            var today = _membershipStatusResolver.GetTodayWib();
            var status = reservation.Status;
            var isVisitDay = reservation.VisitDate.Date == today.Date;

            var detail = new PriorityReservationDetailResponse
            {
                Id = reservation.Id,
                ReservationNumber = reservation.ReservationNumber,
                PatientId = reservation.PatientId,
                MedicalRecordNumber = reservation.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = reservation.Patient?.FullName ?? string.Empty,
                BirthDate = reservation.Patient?.BirthDate,
                VisitDate = reservation.VisitDate,
                Status = status,
                StatusName = GetStatusName(status),
                ArrivedAt = reservation.ArrivedAt,
                ArrivalSource = reservation.ArrivalSource,
                IsScreened = reservation.ScreenedAt.HasValue,
                MembershipTierCodeSnapshot = reservation.MembershipTierCodeSnapshot,
                IsPriorityRevoked = reservation.PatientMembershipSnapshot?.MembershipStatus == MembershipStatus.Cancelled,
                EncounterId = reservation.EncounterId,
                RowVersion = reservation.RowVersion,
                Screening = new PriorityReservationScreeningResponse
                {
                    Cough = reservation.ScreeningCough,
                    Fever = reservation.ScreeningFever,
                    FallRisk = reservation.ScreeningFallRisk,
                    ScreenedAt = reservation.ScreenedAt,
                    ScreenedByName = reservation.ScreenedByUserId.HasValue
                        ? userNames.GetValueOrDefault(reservation.ScreenedByUserId.Value)
                        : null
                },
                ClosedAt = reservation.ClosedAt,
                ClosureReason = reservation.ClosureReason,
                ArrivalCodeIssuedAt = reservation.ArrivalCodeIssuedAt,
                CanMarkArrival = status == PriorityReservationStatus.AwaitingArrival && isVisitDay,
                CanRevertArrival = status == PriorityReservationStatus.Arrived,
                CanScreen = status == PriorityReservationStatus.Arrived,
                CanReissueCode = status == PriorityReservationStatus.AwaitingArrival,
                CanComplete = status == PriorityReservationStatus.Arrived && isVisitDay,
                CanCancel = IsOpen(status),
                CanReschedule = status == PriorityReservationStatus.AwaitingArrival && today.Date < reservation.VisitDate.Date,
                Histories = reservation.Histories
                    .OrderBy(x => x.OccurredAt)
                    .Select(x => new PriorityReservationHistoryResponse
                    {
                        EventCode = x.EventCode,
                        EventName = GetEventName(x.EventCode),
                        FromStatus = x.FromStatus,
                        ToStatus = x.ToStatus,
                        Source = x.Source,
                        ActorName = x.ActorUserId.HasValue ? userNames.GetValueOrDefault(x.ActorUserId.Value) : null,
                        Reason = x.Reason,
                        FromVisitDate = x.FromVisitDate,
                        ToVisitDate = x.ToVisitDate,
                        OccurredAt = x.OccurredAt
                    })
                    .ToList()
            };

            return detail;
        }

        /// <summary>
        /// Cek kelayakan tanpa menyimpan apa pun. <c>null</c> berarti pasien tidak ditemukan.
        /// Urutan pemeriksaannya sama dengan <see cref="CreateAsync"/>.
        /// </summary>
        public async Task<PriorityReservationEligibilityResponse?> CheckEligibilityAsync(
            Guid patientId,
            DateTime? visitDate,
            CancellationToken cancellationToken = default)
        {
            if (!await PatientExistsAsync(patientId, cancellationToken))
                return null;

            var today = _membershipStatusResolver.GetTodayWib();
            var date = ToCalendarDate(visitDate ?? today);

            var dateError = ValidateVisitDate(date, today);
            if (dateError != null)
                return NotEligible(PriorityReservationEligibilityResponse.DateOutOfRange, dateError);

            var classification = await _classificationService.ClassifyAsync(patientId, today, cancellationToken);
            if (!classification.IsMember)
                return NotEligible(PriorityReservationEligibilityResponse.NotActiveMember, NotActiveMemberMessage);

            var open = await FindOpenReservationAsync(patientId, date, cancellationToken);
            if (open != null)
            {
                var response = NotEligible(
                    PriorityReservationEligibilityResponse.OpenReservationExists,
                    BuildOpenReservationMessage(open.Value.ReservationNumber, date));
                response.ExistingReservationNumber = open.Value.ReservationNumber;
                response.MembershipTierCode = classification.MembershipTierCode;
                return response;
            }

            if (await HasActiveOutpatientEncounterOnAsync(patientId, date, cancellationToken))
            {
                var response = NotEligible(
                    PriorityReservationEligibilityResponse.ActiveEncounterExists,
                    BuildActiveEncounterMessage(date));
                response.MembershipTierCode = classification.MembershipTierCode;
                return response;
            }

            return new PriorityReservationEligibilityResponse
            {
                IsEligible = true,
                ReasonCode = PriorityReservationEligibilityResponse.Eligible,
                Message = "Pasien dapat dibuatkan reservasi prioritas.",
                MembershipTierCode = classification.MembershipTierCode
            };
        }

        // ------------------------------------------------------------------
        // Perintah
        // ------------------------------------------------------------------

        /// <summary>
        /// Membuat reservasi (VAL-RP-01 s.d. 06). Pemeriksaan pasien terbuka diulang di bawah
        /// kunci per pasien, sehingga dua permintaan bersamaan menghasilkan satu reservasi.
        /// </summary>
        public async Task<PriorityReservationCommandResult> CreateAsync(
            CreatePriorityReservationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Conflict, DisabledMessage);

            if (request.PatientId == Guid.Empty)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, "Pasien wajib dipilih.");

            if (request.VisitDate == default)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, "Tanggal kunjungan wajib diisi.");

            var today = _membershipStatusResolver.GetTodayWib();
            var visitDate = ToCalendarDate(request.VisitDate);

            var dateError = ValidateVisitDate(visitDate, today);
            if (dateError != null)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, dateError);

            if (!await PatientExistsAsync(request.PatientId, cancellationToken))
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.NotFound, PatientNotFoundMessage);

            var classification = await _classificationService.ClassifyAsync(request.PatientId, today, cancellationToken);
            if (!classification.IsMember)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Conflict, NotActiveMemberMessage);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await LockPatientAsync(request.PatientId, cancellationToken);

            var open = await FindOpenReservationAsync(request.PatientId, visitDate, cancellationToken);
            if (open != null)
            {
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    BuildOpenReservationMessage(open.Value.ReservationNumber, visitDate));
            }

            if (await HasActiveOutpatientEncounterOnAsync(request.PatientId, visitDate, cancellationToken))
            {
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    BuildActiveEncounterMessage(visitDate));
            }

            var now = _timeProvider.GetUtcNow();
            string reservationNumber;

            try
            {
                reservationNumber = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: ReservationSequenceKey,
                        Prefix: ReservationNumberPrefix,
                        ResetPolicy: NumberSeriesResetPolicies.Daily,
                        SequenceDigits: ReservationSequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: now),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, NumberFailedMessage);
            }

            var arrivalCode = NewArrivalCode();
            var nowUtc = now.UtcDateTime;

            var reservation = new RegPriorityReservation
            {
                Id = Guid.NewGuid(),
                ReservationNumber = reservationNumber,
                PatientId = request.PatientId,
                VisitDate = visitDate,
                Status = PriorityReservationStatus.AwaitingArrival,
                ArrivalCodeHash = HashArrivalCode(arrivalCode),
                ArrivalCodeIssuedAt = nowUtc,
                IsPrioritySnapshot = classification.IsPriorityQueue,
                PatientMembershipIdSnapshot = classification.PatientMembershipId,
                MembershipTierIdSnapshot = classification.MembershipTierId,
                MembershipTierCodeSnapshot = classification.MembershipTierCode,
                QueuePriorityLevelSnapshot = classification.PriorityLevel,
                QueueAudienceSnapshot = classification.QueueAudience,
                PublicDisplayModeSnapshot = classification.PublicDisplayMode,
                RowVersion = Guid.NewGuid(),
                CreateBy = actorUserId,
                CreateDateTime = nowUtc
            };

            _dbContext.Add(reservation);
            _dbContext.Add(NewHistory(
                reservation.Id,
                new HistoryEntry(
                    RegPriorityReservationHistory.EventCreated,
                    FromStatus: null,
                    PriorityReservationStatus.AwaitingArrival,
                    PriorityReservationEventSource.Staff,
                    actorUserId),
                nowUtc));

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Lapis kedua INV-RP-01: unique index bersyarat menolak reservasi terbuka ganda.
                _dbContext.ChangeTracker.Clear();
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    $"Pasien sudah punya reservasi prioritas untuk {FormatDate(visitDate)}.");
            }

            await transaction.CommitAsync(cancellationToken);

            return new PriorityReservationCommandResult
            {
                Outcome = PriorityReservationOutcome.Success,
                Message = $"Reservasi prioritas {reservationNumber} berhasil dibuat.",
                EventCode = RegPriorityReservationHistory.EventCreated,
                Reservation = reservation,
                ArrivalCode = arrivalCode
            };
        }

        /// <summary>Petugas menandai pasien tiba (RP-B). Hanya pada hari kunjungan (VAL-RP-10).</summary>
        public Task<PriorityReservationCommandResult> MarkArrivalAsync(
            Guid id,
            PriorityReservationRowVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            ExecuteStaffCommandAsync(
                new StaffCommand(
                    id,
                    request.ExpectedRowVersion,
                    [PriorityReservationStatus.AwaitingArrival],
                    RequiresVisitDay: true,
                    (reservation, nowUtc) =>
                    {
                        reservation.Status = PriorityReservationStatus.Arrived;
                        reservation.ArrivedAt = nowUtc;
                        reservation.ArrivalSource = PriorityReservationArrivalSource.Staff;
                        reservation.ArrivalMarkedByUserId = actorUserId;
                        return (RegPriorityReservationHistory.EventArrived, null);
                    },
                    "Pasien ditandai sudah tiba."),
                actorUserId,
                cancellationToken);

        /// <summary>
        /// Mengembalikan ke Menunggu Kedatangan, misalnya pasien ternyata tidak ada di ruang tunggu.
        /// Kode link yang sama aktif lagi karena hash tidak diganti.
        /// </summary>
        public Task<PriorityReservationCommandResult> RevertArrivalAsync(
            Guid id,
            PriorityReservationReasonRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var reason = NormalizeReason(request.Reason);
            if (reason == null)
                return Task.FromResult(PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, ReasonInvalidMessage));

            return ExecuteStaffCommandAsync(
                new StaffCommand(
                    id,
                    request.ExpectedRowVersion,
                    [PriorityReservationStatus.Arrived],
                    RequiresVisitDay: false,
                    (reservation, _) =>
                    {
                        reservation.Status = PriorityReservationStatus.AwaitingArrival;
                        reservation.ArrivedAt = null;
                        reservation.ArrivalSource = null;
                        reservation.ArrivalMarkedByUserId = null;
                        return (RegPriorityReservationHistory.EventArrivalReverted, reason);
                    },
                    "Reservasi dikembalikan ke Menunggu Kedatangan."),
                actorUserId,
                cancellationToken);
        }

        /// <summary>
        /// Menyimpan checklist skrining awal (RJ-PRI-DEC-019). Boleh diulang; nilai terakhir
        /// berlaku dan setiap simpan tercatat di riwayat tanpa isi checklist.
        /// </summary>
        public Task<PriorityReservationCommandResult> SaveScreeningAsync(
            Guid id,
            PriorityReservationScreeningRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.Cough is not { } cough || request.Fever is not { } fever || request.FallRisk is not { } fallRisk)
                return Task.FromResult(PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, ScreeningIncompleteMessage));

            return ExecuteStaffCommandAsync(
                new StaffCommand(
                    id,
                    request.ExpectedRowVersion,
                    [PriorityReservationStatus.Arrived],
                    RequiresVisitDay: false,
                    (reservation, nowUtc) =>
                    {
                        reservation.ScreeningCough = cough;
                        reservation.ScreeningFever = fever;
                        reservation.ScreeningFallRisk = fallRisk;
                        reservation.ScreenedAt = nowUtc;
                        reservation.ScreenedByUserId = actorUserId;
                        return (RegPriorityReservationHistory.EventScreened, null);
                    },
                    "Skrining awal berhasil disimpan."),
                actorUserId,
                cancellationToken);
        }

        /// <summary>
        /// Kirim ulang: menerbitkan kode baru dan mengganti hash, sehingga link lama otomatis
        /// tidak berlaku (RJ-PRI-DEC-022). Kode baru hanya ada di hasil perintah ini.
        /// </summary>
        public async Task<PriorityReservationCommandResult> ReissueArrivalCodeAsync(
            Guid id,
            PriorityReservationRowVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var arrivalCode = NewArrivalCode();

            var result = await ExecuteStaffCommandAsync(
                new StaffCommand(
                    id,
                    request.ExpectedRowVersion,
                    [PriorityReservationStatus.AwaitingArrival],
                    RequiresVisitDay: false,
                    (reservation, nowUtc) =>
                    {
                        reservation.ArrivalCodeHash = HashArrivalCode(arrivalCode);
                        reservation.ArrivalCodeIssuedAt = nowUtc;
                        return (RegPriorityReservationHistory.EventCodeReissued, null);
                    },
                    "Link konfirmasi kedatangan baru berhasil diterbitkan. Link lama tidak berlaku lagi."),
                actorUserId,
                cancellationToken);

            return result.IsSuccess
                ? new PriorityReservationCommandResult
                {
                    Outcome = result.Outcome,
                    Message = result.Message,
                    Reservation = result.Reservation,
                    ArrivalCode = arrivalCode,
                    EventCode = result.EventCode
                }
                : result;
        }

        /// <summary>
        /// Ganti tanggal sebelum hari kunjungan (RJ-PRI-DEC-034 s.d. 036). Urutan pemeriksaan mengikuti
        /// rawat-jalan <c>02-backend-architecture.md</c> GT.4. Status member dinilai ulang pada hari ini
        /// dan snapshot diganti hasilnya; kode link baru terbit sehingga kode lama tidak cocok lagi.
        /// Bila satu pemeriksaan gagal, tidak ada yang berubah. Pemeriksaan tanggal baru diulang di bawah
        /// kunci pasien yang sama dengan buat reservasi, dan unique index menjadi lapis kedua (INV-RP-01).
        /// </summary>
        public async Task<PriorityReservationCommandResult> RescheduleAsync(
            Guid id,
            ReschedulePriorityReservationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Conflict, DisabledMessage);

            if (request.ExpectedRowVersion == Guid.Empty)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, "Versi data reservasi wajib dikirim.");

            if (request.NewVisitDate == default)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, RescheduleDateRequiredMessage);

            var reason = NormalizeReason(request.Reason);
            if (reason == null)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, ReasonInvalidMessage);

            var reservation = await _dbContext.Set<RegPriorityReservation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (reservation == null)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.NotFound, ReservationNotFoundMessage);

            if (reservation.RowVersion != request.ExpectedRowVersion)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Conflict, StaleMessage);

            if (reservation.Status != PriorityReservationStatus.AwaitingArrival)
            {
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    $"Reservasi sedang berstatus {GetStatusName(reservation.Status)}, sehingga tindakan ini tidak bisa dilakukan.");
            }

            var today = _membershipStatusResolver.GetTodayWib();
            var previousVisitDate = ToCalendarDate(reservation.VisitDate);

            if (today.Date >= previousVisitDate.Date)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Conflict, RescheduleOnVisitDayMessage);

            var newVisitDate = ToCalendarDate(request.NewVisitDate);

            if (newVisitDate == previousVisitDate)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, RescheduleSameDateMessage);

            var dateError = ValidateVisitDate(newVisitDate, today);
            if (dateError != null)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, dateError);

            var classification = await _classificationService.ClassifyAsync(reservation.PatientId, today, cancellationToken);
            if (!classification.IsMember)
            {
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    "Pasien bukan member aktif hari ini, sehingga tanggal reservasi tidak bisa diganti. " +
                    $"Reservasi tetap berlaku untuk {FormatDate(previousVisitDate)}.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await LockPatientAsync(reservation.PatientId, cancellationToken);

            var open = await FindOpenReservationAsync(reservation.PatientId, newVisitDate, cancellationToken);
            if (open != null)
            {
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    BuildOpenReservationMessage(open.Value.ReservationNumber, newVisitDate));
            }

            if (await HasActiveOutpatientEncounterOnAsync(reservation.PatientId, newVisitDate, cancellationToken))
            {
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    BuildActiveEncounterMessage(newVisitDate));
            }

            var arrivalCode = NewArrivalCode();
            var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

            reservation.VisitDate = newVisitDate;
            reservation.IsPrioritySnapshot = classification.IsPriorityQueue;
            reservation.PatientMembershipIdSnapshot = classification.PatientMembershipId;
            reservation.MembershipTierIdSnapshot = classification.MembershipTierId;
            reservation.MembershipTierCodeSnapshot = classification.MembershipTierCode;
            reservation.QueuePriorityLevelSnapshot = classification.PriorityLevel;
            reservation.QueueAudienceSnapshot = classification.QueueAudience;
            reservation.PublicDisplayModeSnapshot = classification.PublicDisplayMode;
            reservation.ArrivalCodeHash = HashArrivalCode(arrivalCode);
            reservation.ArrivalCodeIssuedAt = nowUtc;
            reservation.RowVersion = Guid.NewGuid();
            reservation.UpdateBy = actorUserId;
            reservation.UpdateDateTime = nowUtc;

            _dbContext.Add(NewHistory(
                reservation.Id,
                new HistoryEntry(
                    RegPriorityReservationHistory.EventRescheduled,
                    PriorityReservationStatus.AwaitingArrival,
                    PriorityReservationStatus.AwaitingArrival,
                    PriorityReservationEventSource.Staff,
                    actorUserId,
                    reason,
                    FromVisitDate: previousVisitDate,
                    ToVisitDate: newVisitDate),
                nowUtc));

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Conflict, StaleMessage);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Lapis kedua INV-RP-01 bila pemeriksaan di atas terlewati permintaan paralel.
                _dbContext.ChangeTracker.Clear();
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    $"Pasien sudah punya reservasi prioritas untuk {FormatDate(newVisitDate)}.");
            }

            await transaction.CommitAsync(cancellationToken);

            return new PriorityReservationCommandResult
            {
                Outcome = PriorityReservationOutcome.Success,
                Message = $"Tanggal reservasi diganti ke {FormatDate(newVisitDate)}. Kirim link baru ke pasien; link lama tidak berlaku lagi.",
                EventCode = RegPriorityReservationHistory.EventRescheduled,
                Reservation = reservation,
                ArrivalCode = arrivalCode,
                PreviousVisitDate = previousVisitDate
            };
        }

        /// <summary>Admisi membatalkan reservasi terbuka dengan alasan (RJ-PRI-DEC-023).</summary>
        public Task<PriorityReservationCommandResult> CancelAsync(
            Guid id,
            PriorityReservationReasonRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var reason = NormalizeReason(request.Reason);
            if (reason == null)
                return Task.FromResult(PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, ReasonInvalidMessage));

            return ExecuteStaffCommandAsync(
                new StaffCommand(
                    id,
                    request.ExpectedRowVersion,
                    [PriorityReservationStatus.AwaitingArrival, PriorityReservationStatus.Arrived],
                    RequiresVisitDay: false,
                    (reservation, nowUtc) =>
                    {
                        reservation.Status = PriorityReservationStatus.Cancelled;
                        reservation.ClosedAt = nowUtc;
                        reservation.ClosedByUserId = actorUserId;
                        reservation.ClosureReason = reason;
                        return (RegPriorityReservationHistory.EventCancelled, reason);
                    },
                    "Reservasi prioritas berhasil dibatalkan."),
                actorUserId,
                cancellationToken);
        }

        /// <summary>Satu perintah petugas: syarat status, syarat hari kunjungan, dan perubahannya.</summary>
        private sealed record StaffCommand(
            Guid Id,
            Guid ExpectedRowVersion,
            PriorityReservationStatus[] AllowedStatuses,
            bool RequiresVisitDay,
            Func<RegPriorityReservation, DateTime, (string EventCode, string? Reason)> Apply,
            string SuccessMessage);

        /// <summary>
        /// Kerangka bersama perintah petugas: cocokkan <c>RowVersion</c> (VAL-RP-14), periksa status
        /// (VAL-RP-11) dan hari kunjungan (VAL-RP-10), terapkan perubahan, lalu tulis tepat satu baris
        /// riwayat dalam transaksi yang sama (INV-RP-06). <c>RowVersion</c> adalah concurrency token,
        /// sehingga dua petugas yang menyimpan bersamaan menghasilkan satu berhasil dan satu 409.
        /// </summary>
        private async Task<PriorityReservationCommandResult> ExecuteStaffCommandAsync(
            StaffCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (command.ExpectedRowVersion == Guid.Empty)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Invalid, "Versi data reservasi wajib dikirim.");

            var reservation = await _dbContext.Set<RegPriorityReservation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDelete, cancellationToken);

            if (reservation == null)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.NotFound, ReservationNotFoundMessage);

            if (reservation.RowVersion != command.ExpectedRowVersion)
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Conflict, StaleMessage);

            if (!command.AllowedStatuses.Contains(reservation.Status))
            {
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    $"Reservasi sedang berstatus {GetStatusName(reservation.Status)}, sehingga tindakan ini tidak bisa dilakukan.");
            }

            if (command.RequiresVisitDay && reservation.VisitDate.Date != _membershipStatusResolver.GetTodayWib().Date)
            {
                return PriorityReservationCommandResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    $"Kedatangan hanya bisa ditandai pada hari kunjungan, {FormatDate(reservation.VisitDate)}.");
            }

            var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
            var fromStatus = reservation.Status;
            var (eventCode, reason) = command.Apply(reservation, nowUtc);

            reservation.RowVersion = Guid.NewGuid();
            reservation.UpdateBy = actorUserId;
            reservation.UpdateDateTime = nowUtc;

            _dbContext.Add(NewHistory(
                reservation.Id,
                new HistoryEntry(eventCode, fromStatus, reservation.Status, PriorityReservationEventSource.Staff, actorUserId, reason),
                nowUtc));

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                return PriorityReservationCommandResult.Fail(PriorityReservationOutcome.Conflict, StaleMessage);
            }

            await transaction.CommitAsync(cancellationToken);

            return new PriorityReservationCommandResult
            {
                Outcome = PriorityReservationOutcome.Success,
                Message = command.SuccessMessage,
                Reservation = reservation,
                EventCode = eventCode
            };
        }

        // ------------------------------------------------------------------
        // Integrasi dengan pendaftaran kunjungan (INT-RP-03, INT-RP-04)
        // ------------------------------------------------------------------

        /// <summary>
        /// Pemeriksaan sebelum kunjungan dibuat dari reservasi (VAL-RP-15 s.d. 18). Tidak menyimpan
        /// apa pun. Izin <c>OutpatientPriorityReservation : Complete</c> diperiksa di sini karena
        /// endpoint pendaftaran hanya dijaga <c>PatientEncounter : Create</c> (permission-audit-matrix RP-3).
        /// </summary>
        public async Task<PriorityReservationCompletionResult> PrepareCompletionAsync(
            ClaimsPrincipal user,
            Guid reservationId,
            Guid patientId,
            DateTime encounterDate,
            EncounterType encounterType,
            Guid? clinicId,
            CancellationToken cancellationToken = default)
        {
            if (!await _accessPermissionService.HasAccessAsync(user, "OutpatientPriorityReservation", "Complete"))
            {
                return PriorityReservationCompletionResult.Fail(
                    PriorityReservationOutcome.Forbidden,
                    "Anda tidak punya hak melengkapi reservasi prioritas.");
            }

            var reservation = await _dbContext.Set<RegPriorityReservation>()
                .AsNoTracking()
                .Include(x => x.PatientMembershipSnapshot)
                .FirstOrDefaultAsync(x => x.Id == reservationId && !x.IsDelete, cancellationToken);

            if (reservation == null)
                return PriorityReservationCompletionResult.Fail(PriorityReservationOutcome.NotFound, ReservationNotFoundMessage);

            if (reservation.PatientId != patientId)
            {
                return PriorityReservationCompletionResult.Fail(
                    PriorityReservationOutcome.Invalid,
                    "Pasien tidak sama dengan pasien pada reservasi.");
            }

            if (encounterType != EncounterType.Outpatient || !clinicId.HasValue || clinicId.Value == Guid.Empty)
            {
                return PriorityReservationCompletionResult.Fail(
                    PriorityReservationOutcome.Invalid,
                    "Reservasi prioritas hanya dapat dilengkapi menjadi kunjungan Rawat Jalan ke poliklinik.");
            }

            if (reservation.Status != PriorityReservationStatus.Arrived)
            {
                return PriorityReservationCompletionResult.Fail(
                    PriorityReservationOutcome.Conflict,
                    $"Reservasi sedang berstatus {GetStatusName(reservation.Status)}, sehingga tindakan ini tidak bisa dilakukan.");
            }

            var today = _membershipStatusResolver.GetTodayWib();
            if (encounterDate.Date != today.Date || reservation.VisitDate.Date != today.Date)
            {
                return PriorityReservationCompletionResult.Fail(
                    PriorityReservationOutcome.Invalid,
                    $"Kunjungan dari reservasi hanya untuk hari ini, {FormatDate(today)}.");
            }

            var classification = OutpatientQueueClassificationService.FromReservationSnapshot(
                reservation.PatientMembershipIdSnapshot,
                reservation.MembershipTierIdSnapshot,
                reservation.MembershipTierCodeSnapshot,
                reservation.IsPrioritySnapshot,
                reservation.QueuePriorityLevelSnapshot,
                reservation.QueueAudienceSnapshot,
                reservation.PublicDisplayModeSnapshot,
                isSnapshotMembershipCancelled:
                    reservation.PatientMembershipSnapshot?.MembershipStatus == MembershipStatus.Cancelled);

            return new PriorityReservationCompletionResult
            {
                Outcome = PriorityReservationOutcome.Success,
                Completion = new PriorityReservationCompletion
                {
                    ReservationId = reservation.Id,
                    PatientId = reservation.PatientId,
                    ExpectedRowVersion = reservation.RowVersion,
                    Classification = classification
                }
            };
        }

        /// <summary>
        /// Menandai reservasi Menjadi Kunjungan di dalam transaksi pendaftaran milik pemanggil
        /// (INV-RP-04). Tidak memanggil <c>SaveChanges</c>; kunjungan dan reservasi tersimpan bersama.
        /// </summary>
        /// <exception cref="PriorityReservationConflictException">
        /// Reservasi sudah berubah sejak <see cref="PrepareCompletionAsync"/>, misalnya dilengkapi petugas lain.
        /// </exception>
        public async Task<RegPriorityReservation> CompleteInTransactionAsync(
            PriorityReservationCompletion completion,
            Guid encounterId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await LockPatientAsync(completion.PatientId, cancellationToken);

            var reservation = await _dbContext.Set<RegPriorityReservation>()
                .FirstOrDefaultAsync(x => x.Id == completion.ReservationId && !x.IsDelete, cancellationToken);

            if (reservation == null ||
                reservation.RowVersion != completion.ExpectedRowVersion ||
                reservation.Status != PriorityReservationStatus.Arrived)
            {
                throw new PriorityReservationConflictException(StaleMessage);
            }

            var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

            reservation.Status = PriorityReservationStatus.Converted;
            reservation.EncounterId = encounterId;
            reservation.ClosedAt = nowUtc;
            reservation.ClosedByUserId = actorUserId;
            reservation.RowVersion = Guid.NewGuid();
            reservation.UpdateBy = actorUserId;
            reservation.UpdateDateTime = nowUtc;

            _dbContext.Add(NewHistory(
                reservation.Id,
                new HistoryEntry(
                    RegPriorityReservationHistory.EventConverted,
                    PriorityReservationStatus.Arrived,
                    PriorityReservationStatus.Converted,
                    PriorityReservationEventSource.Staff,
                    actorUserId,
                    EncounterId: encounterId),
                nowUtc));

            return reservation;
        }

        /// <summary>
        /// Kunjungan Rawat Jalan berpoliklinik yang terbentuk tanpa reservasi menutup reservasi
        /// terbuka pasien pada tanggal yang sama sebagai Didaftarkan lewat Jalur Lain (RJ-PRI-DEC-027,
        /// INT-RP-04). Ikut transaksi pemanggil; tidak memanggil <c>SaveChanges</c>.
        /// </summary>
        /// <returns>Reservasi yang ditutup (paling banyak satu karena INV-RP-01).</returns>
        public async Task<IReadOnlyList<RegPriorityReservation>> CloseForEncounterAsync(
            Guid patientId,
            DateTime encounterDate,
            Guid encounterId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await LockPatientAsync(patientId, cancellationToken);

            var visitDate = ToCalendarDate(encounterDate);
            var openReservations = await _dbContext.Set<RegPriorityReservation>()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.VisitDate == visitDate &&
                    (x.Status == PriorityReservationStatus.AwaitingArrival ||
                     x.Status == PriorityReservationStatus.Arrived))
                .ToListAsync(cancellationToken);

            var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

            foreach (var reservation in openReservations)
            {
                var fromStatus = reservation.Status;

                reservation.Status = PriorityReservationStatus.RegisteredElsewhere;
                reservation.EncounterId = encounterId;
                reservation.ClosedAt = nowUtc;
                reservation.ClosedByUserId = actorUserId == Guid.Empty ? null : actorUserId;
                reservation.RowVersion = Guid.NewGuid();
                reservation.UpdateBy = actorUserId;
                reservation.UpdateDateTime = nowUtc;

                _dbContext.Add(NewHistory(
                    reservation.Id,
                    new HistoryEntry(
                        RegPriorityReservationHistory.EventRegisteredElsewhere,
                        fromStatus,
                        PriorityReservationStatus.RegisteredElsewhere,
                        PriorityReservationEventSource.System,
                        actorUserId,
                        EncounterId: encounterId),
                    nowUtc));
            }

            return openReservations;
        }

        // ------------------------------------------------------------------
        // Link publik konfirmasi kedatangan (BE-RJP-012, INT-RP-02)
        // ------------------------------------------------------------------

        private const int ArrivalCodeLength = 43;

        /// <summary>Hasil konfirmasi lewat link. <see cref="Changed"/> terisi hanya bila status berubah.</summary>
        public sealed record ArrivalLinkResult(
            PriorityReservationArrivalPublicResponse Response,
            RegPriorityReservation? Changed);

        /// <summary>
        /// Keadaan link untuk halaman publik. <c>null</c> berarti link tidak berlaku: kode salah atau
        /// sudah diganti, reservasi Dibatalkan, Tidak Datang, Didaftarkan lewat Jalur Lain, atau hari
        /// kunjungannya lewat. Semuanya dijawab sama (VAL-RP-07).
        /// </summary>
        public async Task<PriorityReservationArrivalPublicResponse?> GetArrivalStateAsync(
            string? arrivalCode,
            CancellationToken cancellationToken = default)
        {
            var reservation = await FindByArrivalCodeAsync(arrivalCode, tracked: false, cancellationToken);

            return reservation == null ? null : BuildArrivalState(reservation);
        }

        /// <summary>
        /// Pasien menekan "Saya sudah tiba" (RP-B). Hanya pada hari kunjungan; sebelum itu jawabannya
        /// <c>NOT_YET_OPEN</c> tanpa perubahan. Konfirmasi kedua tidak mengubah jam pertama (R-RP-11).
        /// </summary>
        public async Task<ArrivalLinkResult?> ConfirmArrivalByCodeAsync(
            string? arrivalCode,
            CancellationToken cancellationToken = default)
        {
            var reservation = await FindByArrivalCodeAsync(arrivalCode, tracked: true, cancellationToken);
            if (reservation == null)
                return null;

            var today = _membershipStatusResolver.GetTodayWib();
            if (reservation.Status != PriorityReservationStatus.AwaitingArrival || reservation.VisitDate.Date != today.Date)
                return new ArrivalLinkResult(BuildArrivalState(reservation), null);

            // Dibulatkan ke mikrodetik (presisi kolom PostgreSQL) agar jam di respons pertama sama persis
            // dengan jam yang dibaca ulang saat link dibuka lagi (R-RP-11).
            var nowUtc = TruncateToMicroseconds(_timeProvider.GetUtcNow().UtcDateTime);

            reservation.Status = PriorityReservationStatus.Arrived;
            reservation.ArrivedAt = nowUtc;
            reservation.ArrivalSource = PriorityReservationArrivalSource.PatientLink;
            reservation.ArrivalMarkedByUserId = null;
            reservation.RowVersion = Guid.NewGuid();
            reservation.UpdateDateTime = nowUtc;
            // Diubah lewat link pasien, bukan oleh pengguna sistem.
            reservation.UpdateBy = Guid.Empty;

            _dbContext.Add(NewHistory(
                reservation.Id,
                new HistoryEntry(
                    RegPriorityReservationHistory.EventArrived,
                    PriorityReservationStatus.AwaitingArrival,
                    PriorityReservationStatus.Arrived,
                    PriorityReservationEventSource.PatientLink,
                    ActorUserId: null),
                nowUtc));

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Ketukan ganda atau petugas menandai tiba di saat yang sama: jawab keadaan terbaru.
                _dbContext.ChangeTracker.Clear();
                var current = await FindByArrivalCodeAsync(arrivalCode, tracked: false, cancellationToken);
                return current == null ? null : new ArrivalLinkResult(BuildArrivalState(current), null);
            }

            return new ArrivalLinkResult(BuildArrivalState(reservation), reservation);
        }

        private async Task<RegPriorityReservation?> FindByArrivalCodeAsync(
            string? arrivalCode,
            bool tracked,
            CancellationToken cancellationToken)
        {
            if (!IsWellFormedArrivalCode(arrivalCode))
                return null;

            var hash = HashArrivalCode(arrivalCode!);
            var query = _dbContext.Set<RegPriorityReservation>().Where(x => x.ArrivalCodeHash == hash && !x.IsDelete);
            var reservation = tracked
                ? await query.FirstOrDefaultAsync(cancellationToken)
                : await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

            if (reservation == null)
                return null;

            var isLinkValid = reservation.Status switch
            {
                PriorityReservationStatus.AwaitingArrival => reservation.VisitDate.Date >= _membershipStatusResolver.GetTodayWib().Date,
                PriorityReservationStatus.Arrived or PriorityReservationStatus.Converted => true,
                _ => false
            };

            return isLinkValid ? reservation : null;
        }

        private PriorityReservationArrivalPublicResponse BuildArrivalState(RegPriorityReservation reservation)
        {
            var response = new PriorityReservationArrivalPublicResponse
            {
                ReservationNumber = reservation.ReservationNumber,
                VisitDate = reservation.VisitDate
            };

            if (reservation.Status is PriorityReservationStatus.Arrived or PriorityReservationStatus.Converted)
            {
                var arrivedAtWib = reservation.ArrivedAt.HasValue
                    ? TimeZoneInfo.ConvertTime(
                        new DateTimeOffset(DateTime.SpecifyKind(reservation.ArrivedAt.Value, DateTimeKind.Utc)),
                        _membershipStatusResolver.TimeZone)
                    : (DateTimeOffset?)null;

                response.State = PriorityReservationArrivalPublicResponse.StateConfirmed;
                response.ConfirmedAt = reservation.ArrivedAt;
                response.Message =
                    $"Kedatangan Anda sudah tercatat{(arrivedAtWib.HasValue ? $" pukul {arrivedAtWib.Value:HH.mm} WIB" : string.Empty)}. " +
                    $"Silakan menunggu di {_options.ResolvePriorityWaitingRoomName()}; petugas akan menghampiri Anda.";
                return response;
            }

            if (reservation.VisitDate.Date > _membershipStatusResolver.GetTodayWib().Date)
            {
                response.State = PriorityReservationArrivalPublicResponse.StateNotYetOpen;
                response.Message = $"Konfirmasi kedatangan baru bisa dilakukan pada {FormatDate(reservation.VisitDate)}.";
                return response;
            }

            response.State = PriorityReservationArrivalPublicResponse.StateReady;
            response.Message = "Tekan \"Saya sudah tiba\" bila Anda sudah berada di rumah sakit.";
            return response;
        }

        private static DateTime TruncateToMicroseconds(DateTime value) =>
            new(value.Ticks - value.Ticks % 10, value.Kind);

        private static bool IsWellFormedArrivalCode(string? arrivalCode) =>
            arrivalCode is { Length: ArrivalCodeLength } &&
            arrivalCode.All(c => char.IsAsciiLetterOrDigit(c) || c == '-' || c == '_');

        // ------------------------------------------------------------------
        // Proses sistem (INT-RP-08)
        // ------------------------------------------------------------------

        private const int ClosureBatchSize = 200;
        private const int ClosureMaximumBatches = 500;

        /// <summary>
        /// Menutup reservasi terbuka yang tanggal kunjungannya sudah lewat (WIB) sebagai Tidak Datang
        /// (RJ-PRI-DEC-013). Per batch 200 baris dalam transaksi sendiri. Idempoten: baris yang sudah
        /// tertutup tidak terpilih lagi, sehingga menjalankan dua kali tidak menulis riwayat ganda.
        /// </summary>
        /// <returns>Jumlah reservasi yang ditutup.</returns>
        public async Task<int> CloseExpiredAsync(CancellationToken cancellationToken = default)
        {
            var today = _membershipStatusResolver.GetTodayWib();
            var closed = 0;

            for (var batch = 0; batch < ClosureMaximumBatches; batch++)
            {
                var expired = await _dbContext.Set<RegPriorityReservation>()
                    .Where(x =>
                        !x.IsDelete &&
                        x.VisitDate < today &&
                        (x.Status == PriorityReservationStatus.AwaitingArrival ||
                         x.Status == PriorityReservationStatus.Arrived))
                    .OrderBy(x => x.VisitDate)
                    .ThenBy(x => x.Id)
                    .Take(ClosureBatchSize)
                    .ToListAsync(cancellationToken);

                if (expired.Count == 0)
                    break;

                var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

                foreach (var reservation in expired)
                {
                    var fromStatus = reservation.Status;

                    reservation.Status = PriorityReservationStatus.NoShow;
                    reservation.ClosedAt = nowUtc;
                    reservation.RowVersion = Guid.NewGuid();
                    reservation.UpdateDateTime = nowUtc;
                    // Ditutup otomatis oleh sistem, bukan oleh pengguna.
                    reservation.UpdateBy = Guid.Empty;

                    _dbContext.Add(NewHistory(
                        reservation.Id,
                        new HistoryEntry(
                            RegPriorityReservationHistory.EventNoShow,
                            fromStatus,
                            PriorityReservationStatus.NoShow,
                            PriorityReservationEventSource.System,
                            ActorUserId: null),
                        nowUtc));
                }

                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    closed += expired.Count;
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Petugas mengubah salah satu baris di saat yang sama. Batch ini dibatalkan
                    // utuh; putaran berikutnya memilih ulang baris yang masih terbuka.
                    await transaction.RollbackAsync(cancellationToken);
                }
                finally
                {
                    _dbContext.ChangeTracker.Clear();
                }

                if (expired.Count < ClosureBatchSize)
                    break;
            }

            return closed;
        }

        /// <summary>
        /// Menyusun respons terbit: detail, link sekali tampil, teks WA tanpa poli/dokter, dan
        /// No. HP dari master pasien untuk tombol <c>wa.me</c> (RJ-PRI-DEC-011, RJ-PRI-FE-002).
        /// </summary>
        public async Task<PriorityReservationIssuedResponse?> BuildIssuedResponseAsync(
            PriorityReservationCommandResult result,
            CancellationToken cancellationToken = default)
        {
            if (result.Reservation == null || string.IsNullOrEmpty(result.ArrivalCode))
                return null;

            var detail = await GetDetailAsync(result.Reservation.Id, cancellationToken);
            if (detail == null)
                return null;

            var phoneNumber = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == result.Reservation.PatientId)
                .Select(x => x.PhoneNumber)
                .FirstOrDefaultAsync(cancellationToken);

            var link = BuildArrivalLink(result.ArrivalCode);

            return new PriorityReservationIssuedResponse
            {
                Reservation = detail,
                ArrivalLink = link,
                WhatsAppMessageText = result.EventCode == RegPriorityReservationHistory.EventRescheduled
                    ? BuildRescheduleWhatsAppMessage(detail.ReservationNumber, detail.VisitDate, link)
                    : BuildWhatsAppMessage(detail.ReservationNumber, detail.VisitDate, link),
                PatientPhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim()
            };
        }

        // ------------------------------------------------------------------
        // Bantuan bersama
        // ------------------------------------------------------------------

        /// <summary>Hex SHA-256 huruf kecil (64 karakter) dari kode link.</summary>
        public static string HashArrivalCode(string arrivalCode) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(arrivalCode))).ToLowerInvariant();

        /// <summary>32 byte acak, base64url tanpa padding (43 karakter).</summary>
        public static string NewArrivalCode() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(ArrivalCodeByteLength))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        public static bool IsOpen(PriorityReservationStatus status) =>
            status is PriorityReservationStatus.AwaitingArrival or PriorityReservationStatus.Arrived;

        public static string GetStatusName(PriorityReservationStatus status) => GetEnumDisplayName(status);

        public static string FormatDate(DateTime date) =>
            date.ToString("dd MMMM yyyy", IndonesianCulture);

        private string BuildArrivalLink(string arrivalCode)
        {
            var baseUrl = (_options.PublicArrivalBaseUrl ?? string.Empty).Trim().TrimEnd('/');
            return $"{baseUrl}/{arrivalCode}";
        }

        private string BuildWhatsAppMessage(string reservationNumber, DateTime visitDate, string link)
        {
            var visitDateText = visitDate.ToString("dddd, dd MMMM yyyy", IndonesianCulture);

            return
                "Reservasi pasien prioritas Anda sudah tercatat.\n" +
                $"Nomor reservasi: {reservationNumber}\n" +
                $"Tanggal kunjungan: {visitDateText}\n\n" +
                "Saat tiba di rumah sakit pada tanggal tersebut, buka link berikut lalu tekan \"Saya sudah tiba\":\n" +
                $"{link}\n\n" +
                $"Petugas kami akan menghampiri Anda di {_options.ResolvePriorityWaitingRoomName()}.";
        }

        /// <summary>Teks WA sesudah ganti tanggal (RJ-PRI-FE-005): tanggal baru, link baru, link lama tidak berlaku.</summary>
        private string BuildRescheduleWhatsAppMessage(string reservationNumber, DateTime visitDate, string link)
        {
            var visitDateText = visitDate.ToString("dddd, dd MMMM yyyy", IndonesianCulture);

            return
                "Tanggal reservasi pasien prioritas Anda sudah diganti.\n" +
                $"Nomor reservasi: {reservationNumber}\n" +
                $"Tanggal kunjungan baru: {visitDateText}\n\n" +
                "Link yang dikirim sebelumnya tidak berlaku lagi. Saat tiba di rumah sakit pada tanggal tersebut, " +
                "buka link berikut lalu tekan \"Saya sudah tiba\":\n" +
                $"{link}\n\n" +
                $"Petugas kami akan menghampiri Anda di {_options.ResolvePriorityWaitingRoomName()}.";
        }

        private string? ValidateVisitDate(DateTime visitDate, DateTime today)
        {
            if (visitDate < today)
                return PastDateMessage;

            var maxDaysAhead = Math.Max(0, _options.MaxDaysAhead);
            var lastDate = today.AddDays(maxDaysAhead);

            return visitDate > lastDate
                ? $"Reservasi paling jauh {maxDaysAhead} hari ke depan, yaitu sampai {FormatDate(lastDate)}."
                : null;
        }

        private Task<bool> PatientExistsAsync(Guid patientId, CancellationToken cancellationToken) =>
            _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == patientId && !x.IsDelete && x.IsActive, cancellationToken);

        private async Task<(Guid Id, string ReservationNumber)?> FindOpenReservationAsync(
            Guid patientId,
            DateTime visitDate,
            CancellationToken cancellationToken)
        {
            var open = await _dbContext.Set<RegPriorityReservation>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.VisitDate == visitDate &&
                    (x.Status == PriorityReservationStatus.AwaitingArrival ||
                     x.Status == PriorityReservationStatus.Arrived))
                .Select(x => new { x.Id, x.ReservationNumber })
                .FirstOrDefaultAsync(cancellationToken);

            return open == null ? null : (open.Id, open.ReservationNumber);
        }

        /// <summary>
        /// Kunjungan Rawat Jalan berpoliklinik yang masih berjalan pada tanggal itu (VAL-RP-06).
        /// <c>EncounterDate</c> disimpan sebagai tanggal WIB pukul 00.00 UTC.
        /// </summary>
        private Task<bool> HasActiveOutpatientEncounterOnAsync(
            Guid patientId,
            DateTime visitDate,
            CancellationToken cancellationToken)
        {
            var start = DateTime.SpecifyKind(visitDate.Date, DateTimeKind.Utc);
            var end = start.AddDays(1);

            return _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .WhereBlocksRegistration(_dbContext)
                .AnyAsync(x =>
                    x.PatientId == patientId &&
                    x.EncounterDate >= start &&
                    x.EncounterDate < end,
                    cancellationToken);
        }

        private async Task LockPatientAsync(Guid patientId, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"{PatientLockKeyPrefix}{patientId:N}"],
                cancellationToken);
        }

        /// <summary>Isi satu baris riwayat. <c>ActorUserId</c> kosong untuk link pasien dan sistem.</summary>
        private readonly record struct HistoryEntry(
            string EventCode,
            PriorityReservationStatus? FromStatus,
            PriorityReservationStatus ToStatus,
            PriorityReservationEventSource Source,
            Guid? ActorUserId,
            string? Reason = null,
            Guid? EncounterId = null,
            DateTime? FromVisitDate = null,
            DateTime? ToVisitDate = null);

        private static RegPriorityReservationHistory NewHistory(
            Guid reservationId,
            HistoryEntry entry,
            DateTime occurredAtUtc) =>
            new()
            {
                Id = Guid.NewGuid(),
                PriorityReservationId = reservationId,
                EventCode = entry.EventCode,
                FromStatus = entry.FromStatus,
                ToStatus = entry.ToStatus,
                Source = entry.Source,
                ActorUserId = entry.ActorUserId == Guid.Empty ? null : entry.ActorUserId,
                Reason = entry.Reason,
                EncounterId = entry.EncounterId,
                FromVisitDate = entry.FromVisitDate,
                ToVisitDate = entry.ToVisitDate,
                OccurredAt = occurredAtUtc,
                CreateBy = entry.ActorUserId ?? Guid.Empty,
                CreateDateTime = occurredAtUtc
            };

        private static PriorityReservationEligibilityResponse NotEligible(string reasonCode, string message) =>
            new() { IsEligible = false, ReasonCode = reasonCode, Message = message };

        private static string BuildOpenReservationMessage(string reservationNumber, DateTime visitDate) =>
            $"Pasien sudah punya reservasi prioritas {reservationNumber} untuk {FormatDate(visitDate)}.";

        private static string BuildActiveEncounterMessage(DateTime visitDate) =>
            $"Pasien sudah terdaftar kunjungan Rawat Jalan pada {FormatDate(visitDate)}.";

        /// <summary>Alasan yang sah (VAL-RP-12), atau <c>null</c> bila kosong/terlalu pendek/panjang.</summary>
        private static string? NormalizeReason(string? reason)
        {
            var trimmed = reason?.Trim();

            return trimmed is { Length: >= MinimumReasonLength and <= MaximumReasonLength } ? trimmed : null;
        }

        private static DateTime ToCalendarDate(DateTime value) =>
            DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);

        private static string BuildContainsPattern(string search)
        {
            var escaped = search
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            return $"%{escaped}%";
        }

        private static bool IsUniqueViolation(DbUpdateException exception) =>
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

        private static string GetEventName(string eventCode) => eventCode switch
        {
            RegPriorityReservationHistory.EventCreated => "Reservasi dibuat",
            RegPriorityReservationHistory.EventArrived => "Pasien tiba",
            RegPriorityReservationHistory.EventArrivalReverted => "Dikembalikan ke Belum Tiba",
            RegPriorityReservationHistory.EventScreened => "Skrining awal disimpan",
            RegPriorityReservationHistory.EventCodeReissued => "Link konfirmasi dikirim ulang",
            RegPriorityReservationHistory.EventRescheduled => "Tanggal kunjungan diganti",
            RegPriorityReservationHistory.EventConverted => "Menjadi kunjungan",
            RegPriorityReservationHistory.EventCancelled => "Dibatalkan",
            RegPriorityReservationHistory.EventNoShow => "Tidak datang",
            RegPriorityReservationHistory.EventRegisteredElsewhere => "Didaftarkan lewat jalur lain",
            _ => eventCode
        };

        private static string GetEnumDisplayName<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }

        private static readonly System.Linq.Expressions.Expression<Func<RegPriorityReservation, PriorityReservationListItemResponse>>
            ListItemProjection = x => new PriorityReservationListItemResponse
            {
                Id = x.Id,
                ReservationNumber = x.ReservationNumber,
                PatientId = x.PatientId,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                BirthDate = x.Patient != null ? x.Patient.BirthDate : null,
                VisitDate = x.VisitDate,
                Status = x.Status,
                ArrivedAt = x.ArrivedAt,
                ArrivalSource = x.ArrivalSource,
                IsScreened = x.ScreenedAt != null,
                MembershipTierCodeSnapshot = x.MembershipTierCodeSnapshot,
                IsPriorityRevoked = x.PatientMembershipSnapshot != null &&
                                    x.PatientMembershipSnapshot.MembershipStatus == MembershipStatus.Cancelled,
                EncounterId = x.EncounterId,
                RowVersion = x.RowVersion,
                CreatedAt = x.CreateDateTime,
                UpdatedAt = x.UpdateDateTime
            };
    }
}
