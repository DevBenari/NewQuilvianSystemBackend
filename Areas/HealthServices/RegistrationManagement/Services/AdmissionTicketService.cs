using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    public enum AdmissionTicketOutcome
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        Conflict = 3
    }

    public sealed class AdmissionTicketCommandResult
    {
        public AdmissionTicketOutcome Outcome { get; init; }

        public string Message { get; init; } = string.Empty;

        public RegAdmissionTicket? Ticket { get; init; }

        /// <summary>Tiket yang sudah terbit dipulangkan ulang karena kunci idempotensi sama.</summary>
        public bool IsExisting { get; init; }

        public QueueVoiceGenerateResponse? Voice { get; init; }

        public bool IsSuccess => Outcome == AdmissionTicketOutcome.Success;

        public static AdmissionTicketCommandResult Fail(AdmissionTicketOutcome outcome, string message) =>
            new() { Outcome = outcome, Message = message };
    }

    /// <summary>
    /// Tiket admisi tidak dapat ditautkan ke kunjungan yang sedang disimpan. Dilempar di dalam
    /// transaksi kunjungan sehingga kunjungan dan antrean poli internal ikut dibatalkan.
    /// </summary>
    public class AdmissionTicketConflictException : Exception
    {
        public AdmissionTicketConflictException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Tiket antrean Admisi Rawat Jalan (blueprint Cetak Antrean Admisi v1.1): penerbitan anonim
    /// dari kiosk, cetak ulang nomor yang sama, Panggil Berikutnya multi-loket, transisi status
    /// loket, display, dan penautan ke kunjungan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nomor admisi</b> diterbitkan <see cref="NumberSeriesAllocator"/> (QBE-CODE-006) dengan
    /// deret per kelompok dan pengulangan harian (bagian 6), lalu dibentuk ulang menjadi
    /// <c>A-015</c>. Unique index <c>(ServiceDate, QueueGroupCode, SequenceNumber)</c> menjadi
    /// pengaman terakhir. Nomor yang hangus karena penyimpanan gagal tidak dipakai ulang.
    /// </para>
    /// <para>
    /// <b>Nomor antrean poliklinik internal tidak dibentuk di sini.</b> Ia tetap lahir dari
    /// <see cref="OutpatientQueueNumberAllocator"/> di transaksi simpan kunjungan yang sama
    /// dengan penautan tiket (<see cref="LinkRegistrationInTransactionAsync"/>), sehingga
    /// registrasi gagal tidak pernah meninggalkan antrean poli maupun tiket yang tertaut.
    /// </para>
    /// </remarks>
    public class AdmissionTicketService
    {
        private const string SequenceKeyPrefix = "REG_ADMISSION_TICKET_";
        private const int AllocatorSequenceDigits = 4;
        private const int DisplayRecentCallCount = 5;
        private const int DisplayWaitingCount = 8;

        public const string ActionCall = "call";
        public const string ActionRecall = "recall";
        public const string ActionStartService = "start-service";
        public const string ActionSkip = "skip";
        public const string ActionOpenRegistration = "open-registration";
        public const string ActionComplete = "complete";
        public const string ActionCancel = "cancel";

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly AdmissionQueueOptions _options;
        private readonly QueueRealtimeService _queueRealtimeService;
        private readonly QueueVoiceService? _queueVoiceService;

        public AdmissionTicketService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            IOptions<AdmissionQueueOptions> options,
            QueueRealtimeService queueRealtimeService,
            QueueVoiceService? queueVoiceService = null)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _options = options.Value;
            _queueRealtimeService = queueRealtimeService;
            _queueVoiceService = queueVoiceService;
        }

        // ── Kiosk ───────────────────────────────────────────────────────────────────

        public KioskAdmissionQueueConfigResponse GetKioskConfig()
        {
            var groups = _options.ResolveActiveGroups();

            return new KioskAdmissionQueueConfigResponse
            {
                Enabled = _options.Enabled,
                RequiresGroupSelection = groups.Count > 1,
                HospitalName = string.IsNullOrWhiteSpace(_options.HospitalName) ? null : _options.HospitalName.Trim(),
                WaitingAreaName = _options.ResolveWaitingAreaName(),
                Groups = groups.Select(MapGroup).ToList()
            };
        }

        /// <summary>
        /// Menerbitkan satu tiket admisi. Request dengan kunci idempotensi yang sama dari perangkat
        /// yang sama memulangkan tiket yang sudah terbit (bagian 6 dan 12).
        /// </summary>
        public async Task<AdmissionTicketCommandResult> IssueAsync(
            KioskAdmissionTicketIssueRequest request,
            Guid deviceUserId,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Cetak antrean admisi belum diaktifkan.");

            if (deviceUserId == Guid.Empty)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Perangkat kiosk tidak dikenali.");

            var idempotencyKey = request.IdempotencyKey?.Trim();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Kunci permintaan cetak wajib diisi.");

            var existing = await FindByIdempotencyKeyAsync(deviceUserId, idempotencyKey, cancellationToken);
            if (existing != null)
                return new AdmissionTicketCommandResult { Outcome = AdmissionTicketOutcome.Success, Ticket = existing, IsExisting = true };

            var groups = _options.ResolveActiveGroups();
            AdmissionQueueGroupOptions? group;

            if (string.IsNullOrWhiteSpace(request.QueueGroupCode))
            {
                if (groups.Count > 1)
                    return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Pilih kelompok antrean admisi terlebih dahulu.");

                group = groups[0];
            }
            else
            {
                group = _options.FindActiveGroup(request.QueueGroupCode);
                if (group == null)
                    return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Kelompok antrean admisi tidak tersedia.");
            }

            var nowUtc = DateTime.UtcNow;
            var serviceDate = ToServiceDate(nowUtc);
            var groupCode = group.Code.Trim().ToUpperInvariant();
            var prefix = group.Prefix.Trim().ToUpperInvariant();

            int sequenceNumber;
            try
            {
                // Nomor diminta paling akhir: seluruh penolakan sudah selesai di atas (INV-PLT-002).
                var allocated = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: SequenceKeyPrefix + groupCode,
                        Prefix: prefix,
                        ResetPolicy: NumberSeriesResetPolicies.Daily,
                        SequenceDigits: AllocatorSequenceDigits,
                        ActorUserId: deviceUserId,
                        Instant: new DateTimeOffset(nowUtc, TimeSpan.Zero),
                        AllowSequenceOverflow: true),
                    cancellationToken);

                sequenceNumber = ParseSequence(allocated);
            }
            catch (NumberSeriesAllocationException)
            {
                // Pesan asli alokator menyebut deret dan digit yang tidak berarti bagi pasien.
                return AdmissionTicketCommandResult.Fail(
                    AdmissionTicketOutcome.Invalid,
                    "Nomor antrean admisi gagal diterbitkan. Silakan hubungi petugas.");
            }

            var ticket = new RegAdmissionTicket
            {
                Id = Guid.NewGuid(),
                ServiceDate = serviceDate,
                QueueGroupCode = groupCode,
                QueueGroupName = string.IsNullOrWhiteSpace(group.Name) ? AdmissionQueueOptions.DefaultGroupName : group.Name.Trim(),
                Prefix = prefix,
                SequenceNumber = sequenceNumber,
                DisplayNumber = BuildDisplayNumber(prefix, sequenceNumber),
                IssuedAt = nowUtc,
                IssuedByUserId = deviceUserId,
                IdempotencyKey = idempotencyKey,
                Status = AdmissionTicketStatus.Waiting,
                PrintStatus = AdmissionTicketPrintStatus.Unconfirmed,
                CreateDateTime = nowUtc,
                CreateBy = deviceUserId
            };

            _dbContext.RegAdmissionTickets.Add(ticket);
            AddHistory(ticket, RegAdmissionTicketHistory.EventIssued, null, deviceUserId, nowUtc);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                // Dua request bersamaan dengan kunci yang sama: yang kalah memulangkan tiket pemenang.
                _dbContext.ChangeTracker.Clear();
                var winner = await FindByIdempotencyKeyAsync(deviceUserId, idempotencyKey, cancellationToken);
                if (winner != null)
                    return new AdmissionTicketCommandResult { Outcome = AdmissionTicketOutcome.Success, Ticket = winner, IsExisting = true };

                return AdmissionTicketCommandResult.Fail(
                    AdmissionTicketOutcome.Conflict,
                    "Nomor antrean admisi bentrok. Silakan tekan Cetak Antrean sekali lagi.");
            }

            await _queueRealtimeService.NotifyAdmissionTicketChangedAsync(ticket, RegAdmissionTicketHistory.EventIssued, null, nowUtc);

            return new AdmissionTicketCommandResult { Outcome = AdmissionTicketOutcome.Success, Ticket = ticket };
        }

        /// <summary>Cetak ulang nomor yang sama (bagian 6 dan 12, UAT-04); tidak menerbitkan nomor baru.</summary>
        public async Task<AdmissionTicketCommandResult> ReprintAsync(
            Guid ticketId,
            Guid deviceUserId,
            CancellationToken cancellationToken = default)
        {
            var ticket = await _dbContext.RegAdmissionTickets
                .FirstOrDefaultAsync(x => x.Id == ticketId && !x.IsDelete, cancellationToken);

            if (ticket == null || ticket.IssuedByUserId != deviceUserId)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.NotFound, "Tiket antrean admisi tidak ditemukan pada kiosk ini.");

            if (ticket.ServiceDate != ToServiceDate(DateTime.UtcNow))
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Tiket antrean admisi sudah tidak berlaku untuk hari ini.");

            if (ticket.Status is AdmissionTicketStatus.Cancelled or AdmissionTicketStatus.Completed)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Conflict, "Tiket antrean admisi sudah selesai atau dibatalkan.");

            var nowUtc = DateTime.UtcNow;
            AddHistory(ticket, RegAdmissionTicketHistory.EventReprinted, ticket.Status, deviceUserId, nowUtc);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AdmissionTicketCommandResult { Outcome = AdmissionTicketOutcome.Success, Ticket = ticket, IsExisting = true };
        }

        /// <summary>Kiosk melaporkan hasil cetak; status cetak terpisah dari status pelayanan.</summary>
        public async Task<AdmissionTicketCommandResult> ReportPrintResultAsync(
            Guid ticketId,
            Guid deviceUserId,
            KioskAdmissionTicketPrintResultRequest request,
            CancellationToken cancellationToken = default)
        {
            var ticket = await _dbContext.RegAdmissionTickets
                .FirstOrDefaultAsync(x => x.Id == ticketId && !x.IsDelete, cancellationToken);

            if (ticket == null || ticket.IssuedByUserId != deviceUserId)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.NotFound, "Tiket antrean admisi tidak ditemukan pada kiosk ini.");

            var nowUtc = DateTime.UtcNow;
            ticket.PrintAttemptCount += 1;
            ticket.LastPrintedAt = nowUtc;
            ticket.PrintStatus = request.Succeeded ? AdmissionTicketPrintStatus.Succeeded : AdmissionTicketPrintStatus.Failed;
            ticket.PrintFailureReason = request.Succeeded ? null : NormalizeText(request.FailureReason, 250);
            ticket.UpdateDateTime = nowUtc;
            ticket.UpdateBy = deviceUserId;

            AddHistory(
                ticket,
                request.Succeeded ? RegAdmissionTicketHistory.EventPrinted : RegAdmissionTicketHistory.EventPrintFailed,
                ticket.Status,
                deviceUserId,
                nowUtc,
                reason: ticket.PrintFailureReason);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AdmissionTicketCommandResult { Outcome = AdmissionTicketOutcome.Success, Ticket = ticket };
        }

        // ── Petugas: daftar ─────────────────────────────────────────────────────────

        public AdmissionQueueFilterMetadataResponse GetFilterMetadata() =>
            new()
            {
                Groups = _options.ResolveActiveGroups().Select(MapGroup).ToList(),
                Counters = _options.ResolveActiveCounters().Select(x => new AdmissionQueueCounterResponse
                {
                    Code = x.Code.Trim(),
                    Name = string.IsNullOrWhiteSpace(x.Name) ? x.Code.Trim() : x.Name.Trim(),
                    GroupCodes = x.GroupCodes.Select(code => code.Trim().ToUpperInvariant()).ToList()
                }).ToList(),
                StatusOptions = Enum.GetValues<AdmissionTicketStatus>()
                    .Select(x => new AdmissionQueueEnumOption { Value = (int)x, Name = x.ToString(), Label = GetEnumLabel(x) })
                    .ToList()
            };

        public async Task<AdmissionQueueSummaryResponse> GetSummaryAsync(
            DateTime? serviceDate,
            DateTime? serviceDateFrom,
            DateTime? serviceDateTo,
            string? queueGroupCode,
            CancellationToken cancellationToken = default)
        {
            var (from, to) = ResolveServiceDateRange(serviceDate, serviceDateFrom, serviceDateTo);
            var query = _dbContext.RegAdmissionTickets.AsNoTracking()
                .Where(x => !x.IsDelete && x.ServiceDate >= from && x.ServiceDate <= to);

            if (!string.IsNullOrWhiteSpace(queueGroupCode))
            {
                var code = queueGroupCode.Trim().ToUpperInvariant();
                query = query.Where(x => x.QueueGroupCode == code);
            }

            var counts = await query
                .GroupBy(x => new { x.Status, IsLinked = x.EncounterId != null })
                .Select(x => new { x.Key.Status, x.Key.IsLinked, Count = x.Count() })
                .ToListAsync(cancellationToken);

            int CountOf(AdmissionTicketStatus status) => counts.Where(x => x.Status == status).Sum(x => x.Count);

            return new AdmissionQueueSummaryResponse
            {
                ServiceDate = from,
                ServiceDateTo = to,
                Total = counts.Sum(x => x.Count),
                Waiting = CountOf(AdmissionTicketStatus.Waiting),
                Called = CountOf(AdmissionTicketStatus.Called),
                Serving = CountOf(AdmissionTicketStatus.Serving),
                Completed = CountOf(AdmissionTicketStatus.Completed),
                Skipped = CountOf(AdmissionTicketStatus.Skipped),
                Cancelled = CountOf(AdmissionTicketStatus.Cancelled),
                Registered = counts.Where(x => x.IsLinked).Sum(x => x.Count)
            };
        }

        public async Task<PagedResult<AdmissionTicketListItemResponse>> GetPagedAsync(
            AdmissionQueueListQuery query,
            CancellationToken cancellationToken = default)
        {
            var (from, to) = ResolveServiceDateRange(query.ServiceDate, query.ServiceDateFrom, query.ServiceDateTo);
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var source = _dbContext.RegAdmissionTickets.AsNoTracking()
                .Where(x => !x.IsDelete && x.ServiceDate >= from && x.ServiceDate <= to);

            if (!string.IsNullOrWhiteSpace(query.QueueGroupCode))
            {
                var code = query.QueueGroupCode.Trim().ToUpperInvariant();
                source = source.Where(x => x.QueueGroupCode == code);
            }

            if (query.Status.HasValue)
                source = source.Where(x => x.Status == query.Status.Value);

            if (!string.IsNullOrWhiteSpace(query.CounterCode))
            {
                var counter = query.CounterCode.Trim();
                source = source.Where(x => x.CounterCode == counter);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                source = source.Where(x =>
                    x.DisplayNumber.ToLower().Contains(keyword) ||
                    (x.Encounter != null && (
                        x.Encounter.EncounterNumber.ToLower().Contains(keyword) ||
                        (x.Encounter.Patient != null && (
                            x.Encounter.Patient.FullName.ToLower().Contains(keyword) ||
                            x.Encounter.Patient.MedicalRecordNumber.ToLower().Contains(keyword))))));
            }

            var totalData = await source.CountAsync(cancellationToken);

            // Hari terbaru di atas; di dalam satu hari mengikuti urutan nomor terbit.
            var items = await ProjectListItems(source
                    .OrderByDescending(x => x.ServiceDate)
                    .ThenBy(x => x.QueueGroupCode)
                    .ThenBy(x => x.SequenceNumber)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize))
                .ToListAsync(cancellationToken);

            items.ForEach(ApplyListLabels);

            return new PagedResult<AdmissionTicketListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<AdmissionTicketDetailResponse?> GetDetailAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            var ticket = await _dbContext.RegAdmissionTickets.AsNoTracking()
                .Include(x => x.Encounter!).ThenInclude(x => x.Patient)
                .FirstOrDefaultAsync(x => x.Id == ticketId && !x.IsDelete, cancellationToken);

            if (ticket == null)
                return null;

            var histories = await _dbContext.RegAdmissionTicketHistories.AsNoTracking()
                .Where(x => x.AdmissionTicketId == ticketId && !x.IsDelete)
                .OrderBy(x => x.OccurredAt)
                .ToListAsync(cancellationToken);

            var actorIds = histories.Where(x => x.ActorUserId.HasValue).Select(x => x.ActorUserId!.Value).Distinct().ToList();
            var actorNames = await _dbContext.Set<ApplicationUser>().AsNoTracking()
                .Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName, x.UserName })
                .ToDictionaryAsync(x => x.Id, x => string.IsNullOrWhiteSpace(x.DisplayName) ? x.UserName : x.DisplayName, cancellationToken);

            var detail = new AdmissionTicketDetailResponse
            {
                CalledByUserId = ticket.CalledByUserId,
                SkippedAt = ticket.SkippedAt,
                CancelledAt = ticket.CancelledAt,
                RegistrationLinkedAt = ticket.RegistrationLinkedAt,
                PrintAttemptCount = ticket.PrintAttemptCount,
                LastPrintedAt = ticket.LastPrintedAt,
                PrintFailureReason = ticket.PrintFailureReason,
                Histories = histories.Select(x => new AdmissionTicketHistoryResponse
                {
                    EventCode = x.EventCode,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    ToStatusName = GetEnumLabel(x.ToStatus),
                    CounterCode = x.CounterCode,
                    Reason = x.Reason,
                    ActorUserId = x.ActorUserId,
                    ActorName = x.ActorUserId.HasValue && actorNames.TryGetValue(x.ActorUserId.Value, out var name) ? name : null,
                    OccurredAt = x.OccurredAt
                }).ToList()
            };

            CopyListFields(MapListItem(ticket), detail);
            return detail;
        }

        // ── Petugas: perintah loket ─────────────────────────────────────────────────

        /// <summary>
        /// Panggil Berikutnya (bagian 5 dan 12, UAT-05): tiket menunggu tertua pada kelompok yang
        /// dilayani loket diklaim dengan <c>FOR UPDATE SKIP LOCKED</c>, sehingga dua loket yang
        /// menekan bersamaan tidak pernah mendapat tiket yang sama.
        /// </summary>
        public async Task<AdmissionTicketCommandResult> CallNextAsync(
            AdmissionQueueCallNextRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var counter = _options.FindActiveCounter(request.CounterCode);
            if (counter == null)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Loket admisi tidak tersedia.");

            var groupCodes = _options.ResolveActiveGroups()
                .Select(x => x.Code.Trim().ToUpperInvariant())
                .Where(counter.Serves)
                .ToList();

            if (!string.IsNullOrWhiteSpace(request.QueueGroupCode))
            {
                var requested = request.QueueGroupCode.Trim().ToUpperInvariant();
                if (!groupCodes.Contains(requested))
                    return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Kelompok antrean ini tidak dilayani loket yang dipilih.");

                groupCodes = new List<string> { requested };
            }

            if (groupCodes.Count == 0)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Loket ini belum melayani kelompok antrean mana pun.");

            var serviceDate = ToServiceDate(DateTime.UtcNow);
            var counterCode = counter.Code.Trim();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var heldError = await ResolveCounterHeldErrorAsync(counter, null, cancellationToken);
            if (heldError != null)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Conflict, heldError);

            RegAdmissionTicket? ticket;

            if (_dbContext.Database.IsNpgsql())
            {
                var codes = groupCodes.ToArray();
                var waiting = (int)AdmissionTicketStatus.Waiting;
                // DateOnly supaya dibandingkan sebagai date, bukan timestamptz ber-zona sesi.
                var serviceDay = DateOnly.FromDateTime(serviceDate);
                ticket = (await _dbContext.RegAdmissionTickets
                        .FromSqlInterpolated($@"SELECT * FROM public.""RegAdmissionTicket""
                            WHERE ""ServiceDate"" = {serviceDay}
                              AND ""Status"" = {waiting}
                              AND ""QueueGroupCode"" = ANY({codes})
                              AND NOT ""IsDelete""
                            ORDER BY ""IssuedAt"", ""SequenceNumber""
                            LIMIT 1
                            FOR UPDATE SKIP LOCKED")
                        .ToListAsync(cancellationToken))
                    .FirstOrDefault();
            }
            else
            {
                ticket = await _dbContext.RegAdmissionTickets
                    .Where(x => !x.IsDelete &&
                                x.ServiceDate == serviceDate &&
                                x.Status == AdmissionTicketStatus.Waiting &&
                                groupCodes.Contains(x.QueueGroupCode))
                    .OrderBy(x => x.IssuedAt)
                    .ThenBy(x => x.SequenceNumber)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (ticket == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new AdmissionTicketCommandResult
                {
                    Outcome = AdmissionTicketOutcome.Success,
                    Message = "Tidak ada antrean admisi yang menunggu."
                };
            }

            var nowUtc = DateTime.UtcNow;
            var fromStatus = ticket.Status;
            ApplyCall(ticket, counter, actorUserId, nowUtc);
            AddHistory(ticket, RegAdmissionTicketHistory.EventCalled, fromStatus, actorUserId, nowUtc, counterCode: counterCode);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await FinishCallAsync(ticket, RegAdmissionTicketHistory.EventCalled, nowUtc, $"Nomor {ticket.DisplayNumber} dipanggil ke {ticket.CounterName}.");
        }

        /// <summary>
        /// Panggil satu tiket Menunggu yang dipilih petugas dari daftar ke loketnya. Loket yang
        /// masih memegang nomor Dipanggil/Dilayani ditolak, sama seperti Panggil Berikutnya.
        /// </summary>
        public async Task<AdmissionTicketCommandResult> CallAsync(
            Guid ticketId,
            AdmissionTicketCounterRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var counter = _options.FindActiveCounter(request.CounterCode);
            if (counter == null)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Invalid, "Pilih loket admisi yang aktif sebelum memanggil.");

            var heldError = await ResolveCounterHeldErrorAsync(counter, ticketId, cancellationToken);
            if (heldError != null)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Conflict, heldError);

            return await MutateAsync(
                ticketId,
                actorUserId,
                RegAdmissionTicketHistory.EventCalled,
                cancellationToken,
                (ticket, nowUtc) =>
                {
                    if (ticket.Status != AdmissionTicketStatus.Waiting)
                        return Invalid(ticket, "dipanggil");

                    if (!counter.Serves(ticket.QueueGroupCode))
                        return "Kelompok antrean tiket ini tidak dilayani loket yang dipilih.";

                    ApplyCall(ticket, counter, actorUserId, nowUtc);
                    return null;
                },
                isCall: true);
        }

        /// <summary>Panggil Ulang tiket Dipanggil, atau panggil kembali tiket Dilewati (UAT-06).</summary>
        public Task<AdmissionTicketCommandResult> RecallAsync(
            Guid ticketId,
            AdmissionTicketCounterRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            MutateAsync(
                ticketId,
                actorUserId,
                RegAdmissionTicketHistory.EventRecalled,
                cancellationToken,
                (ticket, nowUtc) =>
                {
                    if (ticket.Status is not (AdmissionTicketStatus.Called or AdmissionTicketStatus.Skipped))
                        return Invalid(ticket, "dipanggil ulang");

                    var counter = _options.FindActiveCounter(
                        string.IsNullOrWhiteSpace(request.CounterCode) ? ticket.CounterCode : request.CounterCode);

                    if (counter == null)
                        return "Pilih loket admisi yang aktif untuk memanggil ulang.";

                    if (!counter.Serves(ticket.QueueGroupCode))
                        return "Kelompok antrean tiket ini tidak dilayani loket yang dipilih.";

                    ApplyCall(ticket, counter, actorUserId, nowUtc);
                    return null;
                },
                isCall: true);

        public Task<AdmissionTicketCommandResult> StartServiceAsync(
            Guid ticketId,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            MutateAsync(
                ticketId,
                actorUserId,
                RegAdmissionTicketHistory.EventServiceStarted,
                cancellationToken,
                (ticket, nowUtc) =>
                {
                    if (ticket.Status != AdmissionTicketStatus.Called)
                        return Invalid(ticket, "mulai dilayani");

                    ticket.Status = AdmissionTicketStatus.Serving;
                    ticket.ServiceStartedAt = nowUtc;
                    ticket.ServiceStartedByUserId = actorUserId;
                    return null;
                });

        public Task<AdmissionTicketCommandResult> SkipAsync(
            Guid ticketId,
            AdmissionTicketReasonRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            MutateAsync(
                ticketId,
                actorUserId,
                RegAdmissionTicketHistory.EventSkipped,
                cancellationToken,
                (ticket, nowUtc) =>
                {
                    if (ticket.Status != AdmissionTicketStatus.Called)
                        return Invalid(ticket, "dilewati");

                    var reason = NormalizeText(request.Reason, 250);
                    if (reason == null)
                        return "Alasan melewati nomor wajib diisi.";

                    ticket.Status = AdmissionTicketStatus.Skipped;
                    ticket.SkipCount += 1;
                    ticket.SkippedAt = nowUtc;
                    ticket.SkippedByUserId = actorUserId;
                    ticket.SkipReason = reason;
                    return null;
                },
                reasonSelector: ticket => ticket.SkipReason);

        /// <summary>
        /// Menutup pelayanan loket. Tiket yang belum tertaut kunjungan (misalnya registrasi gagal)
        /// wajib diberi catatan; tidak ada antrean poli yang dibuat dari penutupan ini (UAT-07).
        /// </summary>
        public Task<AdmissionTicketCommandResult> CompleteAsync(
            Guid ticketId,
            AdmissionTicketCompleteRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            MutateAsync(
                ticketId,
                actorUserId,
                RegAdmissionTicketHistory.EventCompleted,
                cancellationToken,
                (ticket, nowUtc) =>
                {
                    // Dipanggil juga boleh ditutup langsung, misalnya pasien hanya bertanya di loket.
                    if (ticket.Status is not (AdmissionTicketStatus.Called or AdmissionTicketStatus.Serving))
                        return Invalid(ticket, "diselesaikan");

                    var note = NormalizeText(request.Note, 500);
                    if (ticket.EncounterId == null && note == null)
                        return "Catatan penutupan wajib diisi karena tiket belum tertaut ke kunjungan.";

                    ticket.Status = AdmissionTicketStatus.Completed;
                    ticket.ServiceEndedAt = nowUtc;
                    ticket.ServiceEndedByUserId = actorUserId;
                    ticket.CompletionNote = note;
                    return null;
                },
                reasonSelector: ticket => ticket.CompletionNote);

        public Task<AdmissionTicketCommandResult> CancelAsync(
            Guid ticketId,
            AdmissionTicketReasonRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            MutateAsync(
                ticketId,
                actorUserId,
                RegAdmissionTicketHistory.EventCancelled,
                cancellationToken,
                (ticket, nowUtc) =>
                {
                    if (ticket.Status is AdmissionTicketStatus.Completed or AdmissionTicketStatus.Cancelled)
                        return Invalid(ticket, "dibatalkan");

                    if (ticket.EncounterId != null)
                        return "Tiket sudah tertaut ke kunjungan dan tidak dapat dibatalkan.";

                    var reason = NormalizeText(request.Reason, 250);
                    if (reason == null)
                        return "Alasan pembatalan wajib diisi.";

                    ticket.Status = AdmissionTicketStatus.Cancelled;
                    ticket.CancelledAt = nowUtc;
                    ticket.CancelledByUserId = actorUserId;
                    ticket.CancelReason = reason;
                    ticket.IsCancel = true;
                    ticket.CancelDateTime = nowUtc;
                    ticket.CancelBy = actorUserId;
                    return null;
                },
                reasonSelector: ticket => ticket.CancelReason);

        // ── Registrasi kunjungan ────────────────────────────────────────────────────

        /// <summary>
        /// Pemeriksaan sebelum transaksi kunjungan dibuka: tiket ada, hari ini, sedang Dilayani,
        /// dan belum tertaut. Pemeriksaan ulang di bawah kunci baris tetap dilakukan di dalam
        /// transaksi (<see cref="LinkRegistrationInTransactionAsync"/>).
        /// </summary>
        public async Task<string?> ValidateForRegistrationAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            var ticket = await _dbContext.RegAdmissionTickets.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == ticketId && !x.IsDelete, cancellationToken);

            return ticket == null
                ? "Tiket antrean admisi tidak ditemukan."
                : ResolveRegistrationBlocker(ticket);
        }

        /// <summary>
        /// Menautkan tiket ke kunjungan baru di dalam transaksi simpan kunjungan (bagian 8.3 dan
        /// 10 butir 2): tiket dikunci, diperiksa ulang, lalu ditandai Selesai. Kegagalan melempar
        /// <see cref="AdmissionTicketConflictException"/> sehingga kunjungan dan antrean poli
        /// internal ikut di-rollback — retry atau Simpan ganda tidak melahirkan kunjungan kedua
        /// (UAT-12).
        /// </summary>
        public async Task<RegAdmissionTicket> LinkRegistrationInTransactionAsync(
            Guid ticketId,
            Guid encounterId,
            Guid actorUserId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            if (_dbContext.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Penautan tiket admisi harus berjalan di dalam transaksi kunjungan.");

            var ticket = await LockTicketAsync(ticketId, cancellationToken)
                ?? throw new AdmissionTicketConflictException("Tiket antrean admisi tidak ditemukan.");

            var blocker = ResolveRegistrationBlocker(ticket);
            if (blocker != null)
                throw new AdmissionTicketConflictException(blocker);

            var fromStatus = ticket.Status;
            ticket.EncounterId = encounterId;
            ticket.RegistrationLinkedAt = nowUtc;
            ticket.Status = AdmissionTicketStatus.Completed;
            ticket.ServiceEndedAt = nowUtc;
            ticket.ServiceEndedByUserId = actorUserId;
            ticket.UpdateDateTime = nowUtc;
            ticket.UpdateBy = actorUserId;

            AddHistory(ticket, RegAdmissionTicketHistory.EventRegistrationLinked, fromStatus, actorUserId, nowUtc,
                counterCode: ticket.CounterCode, encounterId: encounterId);

            return ticket;
        }

        /// <summary>Kabar realtime sesudah transaksi kunjungan commit; upaya terbaik.</summary>
        public Task NotifyRegistrationLinkedAsync(RegAdmissionTicket ticket, DateTime nowUtc) =>
            _queueRealtimeService.NotifyAdmissionTicketChangedAsync(ticket, RegAdmissionTicketHistory.EventRegistrationLinked, null, nowUtc);

        // ── Display ─────────────────────────────────────────────────────────────────

        /// <summary>Display admisi (bagian 8.5, UAT-15): nomor dan loket saja, tanpa identitas pasien.</summary>
        public async Task<AdmissionQueueDisplayResponse> GetDisplayAsync(
            string? queueGroupCode,
            CancellationToken cancellationToken = default)
        {
            var serviceDate = ToServiceDate(DateTime.UtcNow);
            var query = _dbContext.RegAdmissionTickets.AsNoTracking()
                .Where(x => !x.IsDelete && x.ServiceDate == serviceDate);

            if (!string.IsNullOrWhiteSpace(queueGroupCode))
            {
                var code = queueGroupCode.Trim().ToUpperInvariant();
                query = query.Where(x => x.QueueGroupCode == code);
            }

            var calls = await query
                .Where(x => x.CalledAt != null &&
                            (x.Status == AdmissionTicketStatus.Called || x.Status == AdmissionTicketStatus.Serving))
                .OrderByDescending(x => x.CalledAt)
                .Take(DisplayRecentCallCount + 1)
                .Select(x => new AdmissionQueueDisplayCallResponse
                {
                    TicketId = x.Id,
                    DisplayNumber = x.DisplayNumber,
                    QueueGroupName = x.QueueGroupName,
                    CounterName = x.CounterName,
                    Status = x.Status,
                    CalledAt = x.CalledAt,
                    CallAttemptCount = x.CallAttemptCount
                })
                .ToListAsync(cancellationToken);

            var waitingCount = await query.CountAsync(x => x.Status == AdmissionTicketStatus.Waiting, cancellationToken);

            // Menunggu lebih dulu sesuai urutan terbit; Dilewati di belakang karena menunggu panggil ulang.
            var waitingTickets = await query
                .Where(x => x.Status == AdmissionTicketStatus.Waiting || x.Status == AdmissionTicketStatus.Skipped)
                .OrderBy(x => x.Status == AdmissionTicketStatus.Skipped)
                .ThenBy(x => x.IssuedAt)
                .ThenBy(x => x.SequenceNumber)
                .Take(DisplayWaitingCount)
                .Select(x => new AdmissionQueueDisplayWaitingResponse
                {
                    TicketId = x.Id,
                    DisplayNumber = x.DisplayNumber,
                    QueueGroupName = x.QueueGroupName,
                    Status = x.Status,
                    IssuedAt = x.IssuedAt
                })
                .ToListAsync(cancellationToken);

            var current = calls.FirstOrDefault();
            if (current != null)
            {
                // Audio dibuat saat petugas memanggil; di sini hampir selalu diambil dari cache.
                current.AudioUrl = (await TryBuildVoiceAsync(current.TicketId, current.DisplayNumber, current.CounterName))?.AudioUrl;
            }

            return new AdmissionQueueDisplayResponse
            {
                ServiceDate = serviceDate,
                Current = current,
                RecentCalls = calls.Skip(1).ToList(),
                WaitingCount = waitingCount,
                WaitingTickets = waitingTickets
            };
        }

        /// <summary>
        /// Menandai perangkat display admisi sedang online (kolom Terakhir Online di master display),
        /// sama seperti endpoint runtime display poli. Hanya perangkat aktif ber-layout Admisi.
        /// </summary>
        public async Task TouchDisplayDeviceAsync(Guid displayDeviceId, CancellationToken cancellationToken = default)
        {
            if (displayDeviceId == Guid.Empty)
                return;

            var nowUtc = DateTime.UtcNow;

            await _dbContext.Set<MstQueueDisplayDevice>()
                .Where(x => x.Id == displayDeviceId &&
                            !x.IsDelete &&
                            x.IsActive &&
                            x.LayoutType == QueueDisplayLayoutType.AdmissionBoard)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.LastOnlineDateTime, nowUtc)
                        .SetProperty(x => x.LastErrorMessage, (string?)null),
                    cancellationToken);
        }

        // ── Pemetaan publik ─────────────────────────────────────────────────────────

        public KioskAdmissionTicketResponse MapKioskTicket(RegAdmissionTicket ticket, bool isExisting) =>
            new()
            {
                Id = ticket.Id,
                DisplayNumber = ticket.DisplayNumber,
                QueueGroupCode = ticket.QueueGroupCode,
                QueueGroupName = ticket.QueueGroupName,
                ServiceDate = ticket.ServiceDate,
                IssuedAt = ticket.IssuedAt,
                HospitalName = string.IsNullOrWhiteSpace(_options.HospitalName) ? null : _options.HospitalName.Trim(),
                WaitingAreaName = _options.ResolveWaitingAreaName(),
                PrintStatus = ticket.PrintStatus,
                PrintStatusName = GetEnumLabel(ticket.PrintStatus),
                PrintAttemptCount = ticket.PrintAttemptCount,
                IsExisting = isExisting
            };

        public AdmissionTicketListItemResponse MapListItem(RegAdmissionTicket ticket)
        {
            var item = new AdmissionTicketListItemResponse
            {
                Id = ticket.Id,
                ServiceDate = ticket.ServiceDate,
                DisplayNumber = ticket.DisplayNumber,
                SequenceNumber = ticket.SequenceNumber,
                QueueGroupCode = ticket.QueueGroupCode,
                QueueGroupName = ticket.QueueGroupName,
                IssuedAt = ticket.IssuedAt,
                Status = ticket.Status,
                PrintStatus = ticket.PrintStatus,
                CounterCode = ticket.CounterCode,
                CounterName = ticket.CounterName,
                CalledAt = ticket.CalledAt,
                CallAttemptCount = ticket.CallAttemptCount,
                ServiceStartedAt = ticket.ServiceStartedAt,
                ServiceEndedAt = ticket.ServiceEndedAt,
                SkipCount = ticket.SkipCount,
                SkipReason = ticket.SkipReason,
                CancelReason = ticket.CancelReason,
                CompletionNote = ticket.CompletionNote,
                EncounterId = ticket.EncounterId,
                EncounterNumber = ticket.Encounter?.EncounterNumber,
                PatientName = ticket.Encounter?.Patient?.FullName,
                MedicalRecordNumber = ticket.Encounter?.Patient?.MedicalRecordNumber
            };

            ApplyListLabels(item);
            return item;
        }

        // ── Internal ────────────────────────────────────────────────────────────────

        private delegate string? TicketMutation(RegAdmissionTicket ticket, DateTime nowUtc);

        private async Task<AdmissionTicketCommandResult> MutateAsync(
            Guid ticketId,
            Guid actorUserId,
            string eventCode,
            CancellationToken cancellationToken,
            TicketMutation mutation,
            bool isCall = false,
            Func<RegAdmissionTicket, string?>? reasonSelector = null)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var ticket = await LockTicketAsync(ticketId, cancellationToken);
            if (ticket == null)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.NotFound, "Tiket antrean admisi tidak ditemukan.");

            if (ticket.ServiceDate != ToServiceDate(DateTime.UtcNow))
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Conflict, "Tiket antrean admisi bukan untuk hari ini.");

            var nowUtc = DateTime.UtcNow;
            var fromStatus = ticket.Status;
            var error = mutation(ticket, nowUtc);

            if (error != null)
                return AdmissionTicketCommandResult.Fail(AdmissionTicketOutcome.Conflict, error);

            ticket.UpdateDateTime = nowUtc;
            ticket.UpdateBy = actorUserId;

            AddHistory(ticket, eventCode, fromStatus, actorUserId, nowUtc,
                counterCode: ticket.CounterCode,
                reason: reasonSelector?.Invoke(ticket));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            if (isCall)
                return await FinishCallAsync(ticket, eventCode, nowUtc, $"Nomor {ticket.DisplayNumber} dipanggil ulang ke {ticket.CounterName}.");

            await _queueRealtimeService.NotifyAdmissionTicketChangedAsync(ticket, eventCode, null, nowUtc);
            return new AdmissionTicketCommandResult { Outcome = AdmissionTicketOutcome.Success, Ticket = ticket };
        }

        private async Task<AdmissionTicketCommandResult> FinishCallAsync(
            RegAdmissionTicket ticket,
            string eventCode,
            DateTime nowUtc,
            string message)
        {
            var voice = await TryBuildVoiceAsync(ticket.Id, ticket.DisplayNumber, ticket.CounterName);
            await _queueRealtimeService.NotifyAdmissionTicketChangedAsync(ticket, eventCode, voice?.AudioUrl, nowUtc);

            return new AdmissionTicketCommandResult
            {
                Outcome = AdmissionTicketOutcome.Success,
                Ticket = ticket,
                Voice = voice,
                Message = message
            };
        }

        /// <summary>Satu loket hanya memegang satu nomor Dipanggil/Dilayani pada satu waktu.</summary>
        private async Task<string?> ResolveCounterHeldErrorAsync(
            AdmissionQueueCounterOptions counter,
            Guid? exceptTicketId,
            CancellationToken cancellationToken)
        {
            var serviceDate = ToServiceDate(DateTime.UtcNow);
            var counterCode = counter.Code.Trim();

            var held = await _dbContext.RegAdmissionTickets.AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.ServiceDate == serviceDate &&
                            x.CounterCode == counterCode &&
                            x.Id != exceptTicketId &&
                            (x.Status == AdmissionTicketStatus.Called || x.Status == AdmissionTicketStatus.Serving))
                .Select(x => x.DisplayNumber)
                .FirstOrDefaultAsync(cancellationToken);

            return held == null
                ? null
                : $"{ResolveCounterName(counter)} masih memegang nomor {held}. Mulai pelayanan, lewati, atau selesaikan nomor itu terlebih dahulu.";
        }

        private static void ApplyCall(RegAdmissionTicket ticket, AdmissionQueueCounterOptions counter, Guid actorUserId, DateTime nowUtc)
        {
            ticket.Status = AdmissionTicketStatus.Called;
            ticket.CounterCode = counter.Code.Trim();
            ticket.CounterName = ResolveCounterName(counter);
            ticket.CalledAt = nowUtc;
            ticket.CalledByUserId = actorUserId;
            ticket.CallAttemptCount += 1;
            ticket.UpdateDateTime = nowUtc;
            ticket.UpdateBy = actorUserId;
        }

        private string? ResolveRegistrationBlocker(RegAdmissionTicket ticket)
        {
            if (ticket.EncounterId != null)
                return $"Tiket {ticket.DisplayNumber} sudah tertaut ke kunjungan lain. Buka Daftar Pasien Rawat Jalan untuk melihat kunjungannya.";

            if (ticket.ServiceDate != ToServiceDate(DateTime.UtcNow))
                return $"Tiket {ticket.DisplayNumber} bukan untuk hari ini.";

            if (ticket.Status != AdmissionTicketStatus.Serving)
                return $"Tiket {ticket.DisplayNumber} berstatus {GetEnumLabel(ticket.Status)}. Registrasi hanya dari tiket yang sedang Dilayani.";

            return null;
        }

        private async Task<RegAdmissionTicket?> LockTicketAsync(Guid ticketId, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
            {
                return await _dbContext.RegAdmissionTickets
                    .FirstOrDefaultAsync(x => x.Id == ticketId && !x.IsDelete, cancellationToken);
            }

            return (await _dbContext.RegAdmissionTickets
                    .FromSqlInterpolated($@"SELECT * FROM public.""RegAdmissionTicket""
                        WHERE ""Id"" = {ticketId} AND NOT ""IsDelete""
                        FOR UPDATE")
                    .ToListAsync(cancellationToken))
                .FirstOrDefault();
        }

        private Task<RegAdmissionTicket?> FindByIdempotencyKeyAsync(Guid deviceUserId, string idempotencyKey, CancellationToken cancellationToken) =>
            _dbContext.RegAdmissionTickets
                .FirstOrDefaultAsync(x =>
                    x.IssuedByUserId == deviceUserId &&
                    x.IdempotencyKey == idempotencyKey &&
                    !x.IsDelete,
                    cancellationToken);

        private void AddHistory(
            RegAdmissionTicket ticket,
            string eventCode,
            AdmissionTicketStatus? fromStatus,
            Guid actorUserId,
            DateTime nowUtc,
            string? counterCode = null,
            string? reason = null,
            Guid? encounterId = null)
        {
            _dbContext.RegAdmissionTicketHistories.Add(new RegAdmissionTicketHistory
            {
                Id = Guid.NewGuid(),
                AdmissionTicketId = ticket.Id,
                EventCode = eventCode,
                FromStatus = fromStatus,
                ToStatus = ticket.Status,
                ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
                CounterCode = counterCode,
                Reason = reason,
                EncounterId = encounterId,
                OccurredAt = nowUtc,
                CreateDateTime = nowUtc,
                CreateBy = actorUserId
            });
        }

        private async Task<QueueVoiceGenerateResponse?> TryBuildVoiceAsync(Guid ticketId, string displayNumber, string? counterName)
        {
            if (_queueVoiceService == null)
                return null;

            try
            {
                // RegQueue sementara hanya membawa Id untuk nama berkas cache; tidak disimpan.
                var voiceKey = new RegQueue { Id = ticketId, QueueCode = displayNumber };
                var text = $"Nomor antrean {SpellDisplayNumber(displayNumber)}. Silakan menuju {counterName ?? "loket admisi"}.";

                return await _queueVoiceService.GetOrCreateQueueCallAudioAsync(
                    voiceKey,
                    QueueVoiceCallTypes.Display,
                    forceRegenerate: false,
                    overrideText: text);
            }
            catch
            {
                // Suara hanya pelengkap; panggilan tetap sah tanpa audio.
                return null;
            }
        }

        /// <summary><c>A-015</c> dibaca "A, 15" agar mesin suara tidak mengeja tanda hubung.</summary>
        private static string SpellDisplayNumber(string displayNumber)
        {
            var parts = displayNumber.Split('-', 2);
            return parts.Length == 2 && int.TryParse(parts[1], out var number)
                ? $"{string.Join(" ", parts[0].ToCharArray())}, {number}"
                : displayNumber;
        }

        private static IQueryable<AdmissionTicketListItemResponse> ProjectListItems(IQueryable<RegAdmissionTicket> source) =>
            source.Select(x => new AdmissionTicketListItemResponse
            {
                Id = x.Id,
                ServiceDate = x.ServiceDate,
                DisplayNumber = x.DisplayNumber,
                SequenceNumber = x.SequenceNumber,
                QueueGroupCode = x.QueueGroupCode,
                QueueGroupName = x.QueueGroupName,
                IssuedAt = x.IssuedAt,
                Status = x.Status,
                PrintStatus = x.PrintStatus,
                CounterCode = x.CounterCode,
                CounterName = x.CounterName,
                CalledAt = x.CalledAt,
                CallAttemptCount = x.CallAttemptCount,
                ServiceStartedAt = x.ServiceStartedAt,
                ServiceEndedAt = x.ServiceEndedAt,
                SkipCount = x.SkipCount,
                SkipReason = x.SkipReason,
                CancelReason = x.CancelReason,
                CompletionNote = x.CompletionNote,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                PatientName = x.Encounter != null && x.Encounter.Patient != null ? x.Encounter.Patient.FullName : null,
                MedicalRecordNumber = x.Encounter != null && x.Encounter.Patient != null ? x.Encounter.Patient.MedicalRecordNumber : null
            });

        private static void ApplyListLabels(AdmissionTicketListItemResponse item)
        {
            item.StatusName = GetEnumLabel(item.Status);
            item.PrintStatusName = GetEnumLabel(item.PrintStatus);
            item.AvailableActions = ResolveAvailableActions(item.Status, item.EncounterId);
        }

        /// <summary>Aksi sah per status (bagian 7 dan 8.2); tombol layar mengikuti daftar ini.</summary>
        private static List<string> ResolveAvailableActions(AdmissionTicketStatus status, Guid? encounterId) =>
            status switch
            {
                AdmissionTicketStatus.Waiting => new List<string> { ActionCall, ActionCancel },
                AdmissionTicketStatus.Called =>
                    new List<string> { ActionRecall, ActionSkip, ActionStartService, ActionComplete, ActionCancel },
                AdmissionTicketStatus.Serving when encounterId == null =>
                    new List<string> { ActionOpenRegistration, ActionComplete, ActionCancel },
                AdmissionTicketStatus.Serving => new List<string> { ActionComplete },
                AdmissionTicketStatus.Skipped => new List<string> { ActionRecall, ActionCancel },
                _ => new List<string>()
            };

        private static void CopyListFields(AdmissionTicketListItemResponse source, AdmissionTicketListItemResponse target)
        {
            foreach (var property in typeof(AdmissionTicketListItemResponse).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanWrite)
                    property.SetValue(target, property.GetValue(source));
            }
        }

        private static AdmissionQueueGroupResponse MapGroup(AdmissionQueueGroupOptions group) =>
            new()
            {
                Code = group.Code.Trim().ToUpperInvariant(),
                Name = string.IsNullOrWhiteSpace(group.Name) ? group.Code.Trim() : group.Name.Trim(),
                Prefix = group.Prefix.Trim().ToUpperInvariant(),
                Description = string.IsNullOrWhiteSpace(group.Description) ? null : group.Description.Trim()
            };

        private static string ResolveCounterName(AdmissionQueueCounterOptions counter) =>
            string.IsNullOrWhiteSpace(counter.Name) ? counter.Code.Trim() : counter.Name.Trim();

        private static string Invalid(RegAdmissionTicket ticket, string action) =>
            $"Nomor {ticket.DisplayNumber} berstatus {GetEnumLabel(ticket.Status)} dan tidak dapat {action}.";

        /// <summary>Bagian urut dari nomor alokator <c>A-20261028-0015</c>.</summary>
        private static int ParseSequence(string allocated)
        {
            var lastDash = allocated.LastIndexOf('-');
            var sequence = lastDash >= 0 ? allocated[(lastDash + 1)..] : allocated;

            if (!int.TryParse(sequence, out var value) || value <= 0)
                throw new NumberSeriesAllocationException("Bentuk nomor antrean admisi tidak dikenali.");

            return value;
        }

        internal static string BuildDisplayNumber(string prefix, int sequenceNumber) =>
            $"{prefix}-{sequenceNumber.ToString($"D{AdmissionQueueOptions.DisplaySequenceDigits}")}";

        /// <summary>
        /// Rentang tanggal pelayanan inklusif. Rentang menang atas tanggal tunggal; sisi yang kosong
        /// diisi sisi lainnya; tanpa isian sama sekali berarti hari ini. Awal &gt; akhir ditukar.
        /// </summary>
        private static (DateTime From, DateTime To) ResolveServiceDateRange(
            DateTime? serviceDate,
            DateTime? serviceDateFrom,
            DateTime? serviceDateTo)
        {
            static DateTime Normalize(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

            var from = serviceDateFrom ?? serviceDateTo ?? serviceDate;
            var to = serviceDateTo ?? serviceDateFrom ?? serviceDate;

            if (!from.HasValue || !to.HasValue)
            {
                var today = ToServiceDate(DateTime.UtcNow);
                return (today, today);
            }

            var start = Normalize(from.Value);
            var end = Normalize(to.Value);
            return start <= end ? (start, end) : (end, start);
        }

        /// <summary>Tanggal pelayanan WIB, disimpan sebagai <c>date</c>.</summary>
        private static DateTime ToServiceDate(DateTime utc)
        {
            TimeZoneInfo zone;
            try
            {
                zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch (TimeZoneNotFoundException)
            {
                zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }

            return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utc, zone).Date, DateTimeKind.Utc);
        }

        private static string? NormalizeText(string? value, int maxLength)
        {
            var text = value?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            return text.Length > maxLength ? text[..maxLength] : text;
        }

        private static bool IsUniqueViolation(DbUpdateException exception) =>
            exception.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

        private static string GetEnumLabel<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }
}
