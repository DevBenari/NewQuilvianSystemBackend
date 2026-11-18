using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Permintaan stok barang dan obat: pembuatan, perubahan, pengiriman, dan riwayatnya.
/// </summary>
/// <remarks>
/// Seluruh perintah yang mengubah data bersifat idempoten lewat <c>IdempotencyKey</c>, dan
/// memakai <c>ExpectedVersion</c> agar dua petugas tidak saling menimpa tanpa sadar.
/// </remarks>
public sealed class StockRequestService
{
    private const string LogCategory = "PharmacyManagement";

    private const string CreateAction = "CreateStockRequest";
    private const string UpdateAction = "UpdateStockRequest";
    private const string SubmitAction = "SubmitStockRequest";
    private const string CancelAction = "CancelStockRequest";
    private const string FulfillAction = "FulfillStockRequest";
    private const string ApproveAction = "ApproveStockRequest";
    private const string RejectAction = "RejectStockRequest";
    private const string AuthorizeEmergencyAction = "AuthorizeEmergencyStockRequest";

    // Verifikasi susulan CITO (LGS-DEC-005). Hasilnya dibedakan lewat nama aksi, sehingga
    // terstruktur tanpa kolom atau tabel baru.
    private const string VerifyEmergencyConfirmedAction = "VerifyEmergencyStockRequestConfirmed";
    private const string VerifyEmergencyDeviationAction = "VerifyEmergencyStockRequestDeviation";

    /// <summary>
    /// Batas aktivasi (UTC) persetujuan atasan untuk penyerahan gudang, per lingkungan
    /// (<c>LGS-DEC-006</c>). Aturannya ada pada <see cref="StockRequestApprovalCutover"/>.
    /// </summary>
    /// <remarks>
    /// Diisi lewat konfigurasi lingkungan saat aktivasi (mis. variabel lingkungan
    /// <c>PharmacyManagement__StockRequest__ApprovalEnforcedFromUtc</c>), bukan lewat API, sehingga
    /// pengguna aplikasi tidak dapat mengubahnya. Lihat
    /// <c>docs/module-blueprints/logistik/evidence/05-aktivasi-batas-persetujuan.md</c>.
    /// </remarks>
    public const string ApprovalEnforcedFromConfigKey =
        "PharmacyManagement:StockRequest:ApprovalEnforcedFromUtc";

    // Penanda batas terakhir yang tercatat di log, supaya setiap perubahan nilai konfigurasi
    // meninggalkan jejak operasional tanpa menulis log pada setiap permintaan.
    private static readonly object CutoverObservationLock = new();
    private static string? _lastCutoverObservation;

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly DrugStockService? _drugStockService;
    private readonly StockRequestApprovalAuthority _approvalAuthority;
    private readonly StockRequestApprovalCutover _approvalCutover;

    /// <summary>
    /// <paramref name="drugStockService"/> boleh kosong agar pengujian yang hanya memeriksa
    /// alur permintaan tidak perlu menyiapkan seluruh persediaan. Bila kosong, penyerahan
    /// hanya mencatat jumlah dan tidak menyentuh stok. <paramref name="configuration"/> hanya
    /// dibaca untuk <see cref="ApprovalEnforcedFromConfigKey"/>.
    /// </summary>
    public StockRequestService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        DrugStockService? drugStockService = null, IConfiguration? configuration = null)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _drugStockService = drugStockService;
        _approvalAuthority = new StockRequestApprovalAuthority(dbContext);
        _approvalCutover = StockRequestApprovalCutover.Resolve(
            configuration?[ApprovalEnforcedFromConfigKey], DateTime.UtcNow);
        ObserveCutover(_approvalCutover);
    }

    /// <summary>
    /// Mencatat batas aktivasi ke log sekali per nilai yang berbeda: pertama kali terlihat,
    /// setiap kali berubah, dan selalu sebagai galat bila nilainya tidak sah atau di masa depan.
    /// Nilai yang tidak sah tidak menggagalkan service — yang terjadi justru sebaliknya: seluruh
    /// permintaan wajib disetujui, sehingga kekeliruan konfigurasi tidak pernah membuka jalan
    /// pintas dan gudang tetap dapat bekerja.
    /// </summary>
    private void ObserveCutover(StockRequestApprovalCutover cutover)
    {
        var observation = $"{cutover.State}|{cutover.RawValue}";
        string? previous;

        lock (CutoverObservationLock)
        {
            if (string.Equals(_lastCutoverObservation, observation, StringComparison.Ordinal)) return;
            previous = _lastCutoverObservation;
            _lastCutoverObservation = observation;
        }

        var data = new
        {
            ConfigKey = ApprovalEnforcedFromConfigKey,
            cutover.State,
            cutover.RawValue,
            cutover.EffectiveFromUtc,
            PreviousObservation = previous
        };

        _ = cutover.State is StockRequestApprovalCutoverState.Invalid
                or StockRequestApprovalCutoverState.FutureRejected
            ? _loggerService.ErrorAsync(LogCategory, "StockRequest.ApprovalCutover", cutover.Message,
                data: data)
            : previous == null
                ? _loggerService.AuditAsync(LogCategory, "StockRequest.ApprovalCutover",
                    cutover.Message, data)
                : _loggerService.WarningAsync(LogCategory, "StockRequest.ApprovalCutoverChanged",
                    $"Batas aktivasi persetujuan berubah. {cutover.Message}", data);
    }

    /// <summary>
    /// Permintaan Submitted yang diajukan sebelum batas aktivasi (aturan peralihan).
    /// </summary>
    private bool IsLegacySubmission(PhmStockRequest entity) =>
        entity.Status == StockRequestStatus.Submitted &&
        _approvalCutover.IsBeforeCutover(entity.SubmittedAt);

    // ============================================ 1. riwayat permintaan obat

    public async Task<PagedResult<StockRequestSummaryResponse>> GetPagedAsync(
        StockRequestPagedQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PhmStockRequests.AsNoTracking().Where(x => !x.IsDelete);

        // Sama dengan StockRequestApprovalCutover.IsBeforeCutover: hanya batas yang aktif, dan
        // permintaan tanpa waktu pengajuan tidak pernah dianggap permintaan lama.
        DateTime? cutover = _approvalCutover.AllowsLegacyTransition
            ? _approvalCutover.EffectiveFromUtc
            : null;

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);

        // Antrean gudang memakai aturan yang sama dengan CanFulfill di bawah, supaya permintaan
        // lama yang boleh diserahkan langsung tidak hilang dari antrean.
        if (request.FulfillableOnly)
            query = query.Where(x => x.Status == StockRequestStatus.Approved ||
                                     (cutover != null && x.Status == StockRequestStatus.Submitted &&
                                      x.SubmittedAt != null && x.SubmittedAt < cutover));

        // Verifikasi susulan CITO dihitung dari riwayat, bukan kolom status (LGS-DEC-005).
        if (request.EmergencyReviewState.HasValue)
        {
            query = request.EmergencyReviewState.Value switch
            {
                StockRequestEmergencyReviewState.Pending => query.Where(x =>
                    x.Histories.Any(h => !h.IsDelete && h.Action == AuthorizeEmergencyAction) &&
                    !x.Histories.Any(h => !h.IsDelete &&
                        (h.Action == VerifyEmergencyConfirmedAction || h.Action == VerifyEmergencyDeviationAction))),
                StockRequestEmergencyReviewState.Confirmed => query.Where(x =>
                    x.Histories.Any(h => !h.IsDelete && h.Action == VerifyEmergencyConfirmedAction)),
                StockRequestEmergencyReviewState.Deviation => query.Where(x =>
                    x.Histories.Any(h => !h.IsDelete && h.Action == VerifyEmergencyDeviationAction)),
                _ => query.Where(x =>
                    !x.Histories.Any(h => !h.IsDelete && h.Action == AuthorizeEmergencyAction))
            };
        }
        if (request.Priority.HasValue) query = query.Where(x => x.Priority == request.Priority);
        if (request.RequestingServiceUnitId.HasValue)
            query = query.Where(x => x.RequestingServiceUnitId == request.RequestingServiceUnitId);
        if (request.StorageLocationId.HasValue)
            query = query.Where(x => x.StorageLocationId == request.StorageLocationId);

        // Menyaring menurut obat berarti menyaring permintaan yang MEMUAT obat itu,
        // bukan permintaan yang seluruhnya obat itu.
        if (request.DrugId.HasValue)
            query = query.Where(x => x.Items.Any(i => i.DrugId == request.DrugId && !i.IsDelete));

        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.Date.ToUniversalTime();
            query = query.Where(x => x.RequestedAt >= start);
        }

        if (request.EndDate.HasValue)
        {
            // Batas akhir dibuat eksklusif pada hari berikutnya, supaya permintaan yang
            // dibuat sore hari pada tanggal itu tetap ikut terbawa.
            var endExclusive = request.EndDate.Value.Date.ToUniversalTime().AddDays(1);
            query = query.Where(x => x.RequestedAt < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.RequestNumber.ToLower().Contains(keyword) ||
                (x.RequestingServiceUnit != null &&
                 x.RequestingServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                x.Items.Any(i => !i.IsDelete &&
                    (i.DrugNameSnapshot.ToLower().Contains(keyword) ||
                     i.DrugCodeSnapshot.ToLower().Contains(keyword))));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var total = await query.CountAsync(cancellationToken);

        // Antrean gudang: CITO menyalip seluruh antrean, lalu yang paling lama menunggu.
        // Riwayat biasa tetap terbaru lebih dahulu.
        var ordered = request.OrderByPriority
            ? query.OrderByDescending(x => x.Priority).ThenBy(x => x.RequestedAt)
            : query.OrderByDescending(x => x.RequestedAt);

        var items = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockRequestSummaryResponse
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                RequestingServiceUnitId = x.RequestingServiceUnitId,
                RequestingServiceUnitName = x.RequestingServiceUnit != null
                    ? x.RequestingServiceUnit.ServiceUnitName : string.Empty,
                StorageLocationId = x.StorageLocationId,
                StorageLocationName = x.StorageLocation != null
                    ? x.StorageLocation.StorageLocationName : string.Empty,
                RequestedByName = x.RequestedByWorkforce != null
                    ? x.RequestedByWorkforce.DisplayName : string.Empty,
                Status = x.Status,
                Priority = x.Priority,
                RequestedAt = x.RequestedAt,
                NeededAt = x.NeededAt,
                SubmittedAt = x.SubmittedAt,
                ItemCount = x.ItemCount,
                Version = x.Version,
                IsEditable = x.Status == StockRequestStatus.Draft,
                CanFulfill = x.Status == StockRequestStatus.Approved ||
                             (cutover != null && x.Status == StockRequestStatus.Submitted &&
                              x.SubmittedAt != null && x.SubmittedAt < cutover),
                IsLegacySubmission = cutover != null && x.Status == StockRequestStatus.Submitted &&
                                     x.SubmittedAt != null && x.SubmittedAt < cutover,
                EmergencyReviewState =
                    x.Histories.Any(h => !h.IsDelete && h.Action == VerifyEmergencyDeviationAction)
                        ? StockRequestEmergencyReviewState.Deviation
                        : x.Histories.Any(h => !h.IsDelete && h.Action == VerifyEmergencyConfirmedAction)
                            ? StockRequestEmergencyReviewState.Confirmed
                            : x.Histories.Any(h => !h.IsDelete && h.Action == AuthorizeEmergencyAction)
                                ? StockRequestEmergencyReviewState.Pending
                                : StockRequestEmergencyReviewState.NotRequired,
                CanCancel = x.Status == StockRequestStatus.Draft ||
                            x.Status == StockRequestStatus.Submitted ||
                            x.Status == StockRequestStatus.Approved,
                CanApprove = x.Status == StockRequestStatus.Submitted,
                CanAuthorizeEmergency = x.Status == StockRequestStatus.Submitted &&
                                        x.Priority == StockRequestPriority.Emergency,
                IsPartiallyFulfilled = x.Status == StockRequestStatus.Completed &&
                                       x.Items.Any(i => !i.IsDelete && i.FulfilledQuantity != null &&
                                                        i.FulfilledQuantity < i.RequestedQuantity)
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<StockRequestSummaryResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ============================================ 2. detail permintaan

    public async Task<StockRequestDetailResponse?> GetDetailAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(id, tracking: false, cancellationToken);
        if (entity == null) return null;

        var detail = MapDetail(entity);

        // Nama petugas pada riwayat dibaca dari akun penggunanya, supaya setiap keputusan
        // menunjukkan siapa yang memutuskan, bukan sekadar kapan.
        var actorIds = detail.Histories.Select(h => h.ActorUserId).Distinct().ToList();
        if (actorIds.Count > 0)
        {
            var actors = await _dbContext.Users.AsNoTracking()
                .Where(u => actorIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    Name = u.WorkforceProfile != null ? u.WorkforceProfile.DisplayName : u.UserName
                })
                .ToListAsync(cancellationToken);

            var names = actors.ToDictionary(a => a.Id, a => a.Name);
            foreach (var history in detail.Histories)
                history.ActorName = names.GetValueOrDefault(history.ActorUserId);
        }

        // Aturan peralihan: permintaan lama yang diajukan sebelum persetujuan berlaku masih
        // boleh diserahkan langsung (lihat ApprovalEnforcedFromConfigKey).
        detail.IsLegacySubmission = IsLegacySubmission(entity);
        if (detail.IsLegacySubmission) detail.CanFulfill = true;

        // Otorisasi CITO dan verifikasi susulannya dibaca dari riwayat (LGS-DEC-005).
        var authorization = FindEmergencyAuthorization(entity);
        var review = FindEmergencyReview(entity);
        detail.EmergencyReviewState = ResolveEmergencyReviewState(authorization, review);

        if (authorization != null)
            detail.EmergencyAuthorization = new StockRequestDecisionRecordResponse
            {
                ActorUserId = authorization.ActorUserId,
                ActorName = detail.Histories.FirstOrDefault(h => h.Id == authorization.Id)?.ActorName,
                OccurredAt = authorization.OccurredAt,
                Reason = authorization.Reason
            };

        if (review != null)
            detail.EmergencyReview = new StockRequestEmergencyReviewResponse
            {
                ActorUserId = review.ActorUserId,
                ActorName = detail.Histories.FirstOrDefault(h => h.Id == review.Id)?.ActorName,
                OccurredAt = review.OccurredAt,
                Reason = review.Reason,
                Outcome = review.Action == VerifyEmergencyDeviationAction
                    ? StockRequestEmergencyReviewOutcome.Deviation
                    : StockRequestEmergencyReviewOutcome.Confirmed
            };

        var currentUserId = TryGetCurrentUserId();
        if (currentUserId.HasValue)
        {
            var userId = currentUserId.Value;
            detail.IsOwnRequest = await IsRequesterAsync(entity, userId, cancellationToken);

            // Kewenangan dihitung dengan aturan yang sama dengan DecideAsync dan
            // VerifyEmergencyAsync, supaya layar menjelaskan sebab penolakan sebelum petugas
            // menekan tombol — bukan sesudahnya. Layar tidak pernah menjadi sumber kewenangan.
            var awaitingDecision = entity.Status == StockRequestStatus.Submitted;
            var awaitingReview = detail.EmergencyReviewState == StockRequestEmergencyReviewState.Pending;

            if (awaitingDecision || awaitingReview)
            {
                detail.HasApproveAuthority = await _approvalAuthority.HasApproveGrantAsync(userId,
                    cancellationToken);

                var authority = await _approvalAuthority.CheckUnitAuthorityAsync(userId,
                    entity.RequestingServiceUnitId, cancellationToken);
                detail.HasUnitAuthority = authority.IsAuthorized;
                detail.UnitAuthorityMessage = authority.Message;
            }

            if (awaitingDecision && entity.Priority == StockRequestPriority.Emergency)
                detail.HasEmergencyAuthority = await _approvalAuthority.HasEmergencyGrantAsync(
                    userId, cancellationToken);

            if (awaitingReview)
            {
                var verdict = await EvaluateEmergencyVerifierAsync(entity, authorization!, userId,
                    cancellationToken);
                detail.CanVerifyEmergency = verdict.Allowed;
                detail.VerifyEmergencyMessage = verdict.Message;
            }
        }

        return detail;
    }

    // ============================================ kewenangan keputusan (domain)

    private sealed record AuthorityVerdict(bool Allowed, string ReasonCode, string? Message)
    {
        public static readonly AuthorityVerdict Granted = new(true, "Granted", null);
        public static AuthorityVerdict Denied(string code, string message) => new(false, code, message);
    }

    /// <summary>
    /// Kewenangan setuju atau tolak: bukan pemohon, izin <c>StockRequest:Approve</c> eksplisit,
    /// dan atasan unit peminta. Ketiganya diperiksa di sini, tidak bergantung pada filter
    /// controller, peran SuperAdmin, maupun saklar pengembangan.
    /// </summary>
    private async Task<AuthorityVerdict> EvaluateDeciderAsync(PhmStockRequest entity, Guid userId,
        CancellationToken cancellationToken)
    {
        if (await IsRequesterAsync(entity, userId, cancellationToken))
            return AuthorityVerdict.Denied("SelfDecision",
                "Pemohon tidak boleh menyetujui, menolak, maupun mengotorisasi permintaannya sendiri.");

        if (!await _approvalAuthority.HasApproveGrantAsync(userId, cancellationToken))
            return AuthorityVerdict.Denied("NoApprovePermission",
                "Persetujuan dan penolakan hanya dapat dilakukan petugas yang penempatan jabatannya " +
                "diberi izin StockRequest:Approve secara eksplisit.");

        var unit = await _approvalAuthority.CheckUnitAuthorityAsync(userId,
            entity.RequestingServiceUnitId, cancellationToken);
        return unit.IsAuthorized
            ? AuthorityVerdict.Granted
            : AuthorityVerdict.Denied($"Unit{unit.Reason}", unit.Message
                ?? "Anda tidak berwenang memutus permintaan unit ini.");
    }

    /// <summary>
    /// Kewenangan otorisasi CITO: bukan pemohon dan izin <c>StockRequest:AuthorizeEmergency</c>
    /// eksplisit pada penempatan jabatan yang aktif (<c>LGS-DEC-004</c>).
    /// </summary>
    private async Task<AuthorityVerdict> EvaluateEmergencyAuthorizerAsync(PhmStockRequest entity,
        Guid userId, CancellationToken cancellationToken)
    {
        if (await IsRequesterAsync(entity, userId, cancellationToken))
            return AuthorityVerdict.Denied("SelfDecision",
                "Pemohon tidak boleh menyetujui, menolak, maupun mengotorisasi permintaannya sendiri.");

        return await _approvalAuthority.HasEmergencyGrantAsync(userId, cancellationToken)
            ? AuthorityVerdict.Granted
            : AuthorityVerdict.Denied("NoEmergencyPermission",
                "Otorisasi CITO hanya dapat dilakukan petugas yang penempatan jabatannya diberi " +
                "izin StockRequest:AuthorizeEmergency secara eksplisit. Peran SuperAdmin saja " +
                "tidak cukup.");
    }

    /// <summary>
    /// Kewenangan verifikasi susulan CITO (<c>LGS-DEC-005</c>): bukan pemohon, bukan petugas yang
    /// mengotorisasi CITO-nya, izin <c>StockRequest:Approve</c> eksplisit, dan atasan unit peminta.
    /// </summary>
    private async Task<AuthorityVerdict> EvaluateEmergencyVerifierAsync(PhmStockRequest entity,
        PhmStockRequestHistory authorization, Guid userId, CancellationToken cancellationToken)
    {
        if (await IsRequesterAsync(entity, userId, cancellationToken))
            return AuthorityVerdict.Denied("SelfReview",
                "Pemohon tidak boleh memverifikasi otorisasi CITO atas permintaannya sendiri.");

        if (authorization.ActorUserId == userId)
            return AuthorityVerdict.Denied("AuthorizerReview",
                "Petugas yang mengotorisasi CITO tidak boleh memverifikasi otorisasinya sendiri.");

        if (!await _approvalAuthority.HasApproveGrantAsync(userId, cancellationToken))
            return AuthorityVerdict.Denied("NoApprovePermission",
                "Verifikasi susulan hanya dapat dilakukan atasan unit yang penempatan jabatannya " +
                "diberi izin StockRequest:Approve secara eksplisit.");

        var unit = await _approvalAuthority.CheckUnitAuthorityAsync(userId,
            entity.RequestingServiceUnitId, cancellationToken);
        return unit.IsAuthorized
            ? AuthorityVerdict.Granted
            : AuthorityVerdict.Denied($"Unit{unit.Reason}", unit.Message
                ?? "Anda tidak berwenang memverifikasi permintaan unit ini.");
    }

    /// <summary>
    /// Mencatat percobaan yang ditolak ke log audit — siapa, atas permintaan apa, aksi apa, dan
    /// mengapa — lalu mengembalikan pengecualian untuk dilempar pemanggil.
    /// </summary>
    private async Task<StockRequestForbiddenException> DeniedAsync(PhmStockRequest entity,
        string action, Guid actorUserId, AuthorityVerdict verdict)
    {
        await _loggerService.WarningAsync(LogCategory, "StockRequest.AuthorityDenied",
            verdict.Message ?? "Tindakan atas permintaan stok ditolak karena kewenangan.",
            new
            {
                entity.Id,
                entity.RequestNumber,
                entity.RequestingServiceUnitId,
                Action = action,
                verdict.ReasonCode,
                ActorUserId = actorUserId
            });

        return new StockRequestForbiddenException(verdict.Message
            ?? "Anda tidak berwenang melakukan tindakan ini.");
    }

    private static PhmStockRequestHistory? FindEmergencyAuthorization(PhmStockRequest entity) =>
        entity.Histories
            .Where(h => !h.IsDelete && h.Action == AuthorizeEmergencyAction)
            .OrderBy(h => h.OccurredAt)
            .FirstOrDefault();

    private static PhmStockRequestHistory? FindEmergencyReview(PhmStockRequest entity) =>
        entity.Histories
            .Where(h => !h.IsDelete &&
                        (h.Action == VerifyEmergencyConfirmedAction || h.Action == VerifyEmergencyDeviationAction))
            .OrderBy(h => h.OccurredAt)
            .FirstOrDefault();

    private static StockRequestEmergencyReviewState ResolveEmergencyReviewState(
        PhmStockRequestHistory? authorization, PhmStockRequestHistory? review) =>
        review != null
            ? review.Action == VerifyEmergencyDeviationAction
                ? StockRequestEmergencyReviewState.Deviation
                : StockRequestEmergencyReviewState.Confirmed
            : authorization != null
                ? StockRequestEmergencyReviewState.Pending
                : StockRequestEmergencyReviewState.NotRequired;

    // ============================================ 3. buat permintaan

    public async Task<StockRequestDetailResponse> CreateAsync(CreateStockRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        var actorUserId = GetCurrentUserId();
        var fingerprint = BuildFingerprint(request.StorageLocationId, request.Priority,
            request.NeededAt, request.Notes, request.Items);

        var prior = await FindIdempotentAsync(CreateAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(prior.StockRequestId, cancellationToken))!;
        }

        await ValidateReferencesAsync(request.RequestingServiceUnitId, request.StorageLocationId,
            request.RequestedByWorkforceId, cancellationToken);

        var drugs = await ResolveDrugsAsync(request.Items, cancellationToken);
        var now = DateTime.UtcNow;
        var id = DeterministicId(request.IdempotencyKey);

        var entity = new PhmStockRequest
        {
            Id = id,
            RequestNumber = $"REQ-{now:yyyyMMdd}-{id.ToString("N")[..6].ToUpperInvariant()}",
            RequestingServiceUnitId = request.RequestingServiceUnitId,
            StorageLocationId = request.StorageLocationId,
            RequestedByWorkforceId = request.RequestedByWorkforceId,
            Status = StockRequestStatus.Draft,
            Priority = request.Priority,
            NeededAt = request.NeededAt?.ToUniversalTime(),
            Notes = Normalize(request.Notes),
            RequestedAt = now,
            ItemCount = request.Items.Count,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.PhmStockRequests.Add(entity);
        AddItems(entity.Id, request.Items, drugs, actorUserId, now);

        _dbContext.PhmStockRequestHistories.Add(NewHistory(entity.Id, StockRequestStatus.Draft,
            null, CreateAction, null, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "StockRequest.Create",
            "Membuat permintaan stok barang atau obat.",
            new { entity.Id, entity.RequestNumber, entity.ItemCount, ActorUserId = actorUserId });

        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    // ============================================ 4. edit permintaan

    /// <summary>
    /// Mengubah permintaan yang masih berstatus <c>Draft</c>.
    /// </summary>
    /// <remarks>
    /// Setelah dikirim, permintaan tidak boleh diubah lagi. Gudang sudah melihatnya dan
    /// mungkin sudah mulai menyiapkan; mengubah isinya diam-diam membuat yang disiapkan
    /// tidak lagi cocok dengan yang diminta. Bila memang perlu berubah, permintaan
    /// dibatalkan lalu dibuat baru — sehingga jejaknya tetap terbaca.
    /// </remarks>
    public async Task<StockRequestDetailResponse> UpdateAsync(Guid id,
        UpdateStockRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        var actorUserId = GetCurrentUserId();
        var fingerprint = BuildFingerprint(request.StorageLocationId, request.Priority,
            request.NeededAt, request.Notes, request.Items);

        var prior = await FindIdempotentAsync(UpdateAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            if (prior.StockRequestId != id)
                throw new StockRequestConflictException("PHM013",
                    "Idempotency key sudah dipakai untuk permintaan lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Permintaan stok tidak ditemukan.");

        if (entity.Status != StockRequestStatus.Draft)
            throw new StockRequestConflictException("PHM004",
                "Permintaan yang sudah dikirim tidak dapat diubah. Batalkan lalu buat permintaan baru.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var storageValid = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .AnyAsync(x => x.Id == request.StorageLocationId && !x.IsDelete, cancellationToken);
        if (!storageValid)
            throw new StockRequestUnprocessableException("PHM001",
                "Lokasi penyimpanan tidak ditemukan.");

        var drugs = await ResolveDrugsAsync(request.Items, cancellationToken);
        var now = DateTime.UtcNow;

        // Baris lama ditandai terhapus, bukan dibuang dari basis data, supaya jejak
        // penyuntingan tetap dapat ditelusuri bila kelak dipersoalkan.
        foreach (var existing in entity.Items.Where(x => !x.IsDelete))
        {
            existing.IsDelete = true;
            existing.DeleteDateTime = now;
            existing.DeleteBy = actorUserId;
        }

        AddItems(entity.Id, request.Items, drugs, actorUserId, now);

        entity.StorageLocationId = request.StorageLocationId;
        entity.Priority = request.Priority;
        entity.NeededAt = request.NeededAt?.ToUniversalTime();
        entity.Notes = Normalize(request.Notes);
        entity.ItemCount = request.Items.Count;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockRequestHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status,
            UpdateAction, null, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ============================================ perintah status

    /// <summary>
    /// Mengajukan permintaan untuk disetujui atasan unit; sesudah ini isinya terkunci.
    /// </summary>
    /// <remarks>
    /// Permintaan CITO (<c>Emergency</c>) wajib menyertakan alasan darurat. Memilih CITO tidak
    /// melewati persetujuan: permintaan tetap menunggu, hanya saja dapat diotorisasi lewat
    /// jalur darurat oleh petugas berwenang (<c>LGS-DEC-002</c>).
    /// </remarks>
    public async Task<StockRequestDetailResponse> SubmitAsync(Guid id,
        SubmitStockRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();
        var emergencyReason = Normalize(request.EmergencyReason);

        // Sidik jari lama dipertahankan untuk pengajuan tanpa alasan darurat, supaya kunci
        // idempotensi yang tercatat sebelum perubahan ini tetap cocok saat dikirim ulang.
        var fingerprint = emergencyReason == null
            ? Hash($"submit:{id}")
            : Hash($"submit:{id}|{emergencyReason}");

        var prior = await FindIdempotentAsync(SubmitAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Permintaan stok tidak ditemukan.");

        if (entity.Status != StockRequestStatus.Draft)
            throw new StockRequestConflictException("PHM004",
                "Hanya permintaan berstatus Draft yang dapat dikirim.");

        if (!entity.Items.Any(x => !x.IsDelete))
            throw new StockRequestUnprocessableException("PHM002",
                "Permintaan tanpa satu pun item tidak dapat dikirim.");

        if (entity.Priority == StockRequestPriority.Emergency && emergencyReason == null)
            throw new StockRequestUnprocessableException("PHM014",
                "Alasan CITO wajib diisi untuk permintaan darurat.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var now = DateTime.UtcNow;
        entity.Status = StockRequestStatus.Submitted;
        entity.SubmittedAt = now;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockRequestHistories.Add(NewHistory(entity.Id, StockRequestStatus.Submitted,
            StockRequestStatus.Draft, SubmitAction,
            entity.Priority == StockRequestPriority.Emergency ? emergencyReason : null,
            request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "StockRequest.Submit",
            "Mengajukan permintaan stok untuk persetujuan atasan unit.",
            new { entity.Id, entity.RequestNumber, entity.Priority, ActorUserId = actorUserId });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    public async Task<StockRequestDetailResponse> CancelAsync(Guid id,
        CancelStockRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new StockRequestUnprocessableException("PHM009",
                "Alasan pembatalan wajib diisi.");

        var actorUserId = GetCurrentUserId();
        var reason = request.Reason.Trim();
        var fingerprint = Hash(reason);

        var prior = await FindIdempotentAsync(CancelAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Permintaan stok tidak ditemukan.");

        // Selama barangnya belum diserahkan, permintaan masih boleh dibatalkan peminta:
        // kebutuhan unit dapat hilang setelah permintaan dikirim, juga setelah disetujui.
        // Permintaan yang ditolak sudah tertutup dan tidak dibatalkan lagi.
        if (entity.Status is not (StockRequestStatus.Draft or StockRequestStatus.Submitted
            or StockRequestStatus.Approved))
            throw new StockRequestConflictException("PHM004",
                "Permintaan yang sudah diserahkan, ditolak, atau sudah ditutup tidak dapat dibatalkan.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var now = DateTime.UtcNow;
        var from = entity.Status;
        entity.Status = StockRequestStatus.Cancelled;
        entity.DecidedAt = now;
        entity.DecisionReason = reason;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockRequestHistories.Add(NewHistory(entity.Id, StockRequestStatus.Cancelled,
            from, CancelAction, reason, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ============================================ keputusan atasan dan jalur CITO

    /// <summary>
    /// Persetujuan atasan unit atas permintaan yang menunggu persetujuan (<c>LGS-DEC-002</c>).
    /// </summary>
    /// <remarks>
    /// Izin <c>StockRequest:Approve</c> diperiksa di controller menurut kebijakan platform, lalu
    /// diperiksa ulang di sini secara eksplisit terhadap penempatan jabatan (tidak ikut jalan
    /// pintas SuperAdmin maupun saklar pengembangan). Di sini juga dijaga keadaan permintaan,
    /// versi data, larangan menyetujui permintaan sendiri, dan kewenangan atas unit peminta:
    /// hanya atasan unit (penempatan manajerial aktif pada unit organisasi yang ditautkan ke unit
    /// layanan peminta, <c>LGS-DEC-003</c>) yang boleh memutus. Persetujuan tidak menyentuh stok;
    /// saldo baru berubah saat gudang menyerahkan.
    /// </remarks>
    public Task<StockRequestDetailResponse> ApproveAsync(Guid id,
        ApproveStockRequestRequest request, CancellationToken cancellationToken = default) =>
        DecideAsync(id, ApproveAction, StockRequestStatus.Approved, Normalize(request.Note),
            reasonRequired: false, emergencyOnly: false, request.ExpectedVersion,
            request.IdempotencyKey, "StockRequest.Approve",
            "Atasan unit menyetujui permintaan stok.", cancellationToken);

    /// <summary>Penolakan atasan unit; alasannya wajib dan permintaan tidak dapat diproses lagi.</summary>
    public Task<StockRequestDetailResponse> RejectAsync(Guid id,
        RejectStockRequestRequest request, CancellationToken cancellationToken = default) =>
        DecideAsync(id, RejectAction, StockRequestStatus.Rejected, Normalize(request.Reason),
            reasonRequired: true, emergencyOnly: false, request.ExpectedVersion,
            request.IdempotencyKey, "StockRequest.Reject",
            "Atasan unit menolak permintaan stok.", cancellationToken);

    /// <summary>
    /// Otorisasi jalur CITO oleh petugas berwenang atas permintaan darurat yang menunggu.
    /// </summary>
    /// <remarks>
    /// Izin <c>StockRequest:AuthorizeEmergency</c> diperiksa dua kali. Controller memeriksanya
    /// menurut kebijakan platform, yang meloloskan SuperAdmin. Domain memeriksanya lagi tanpa
    /// pengecualian: izin itu harus diberikan eksplisit kepada penempatan jabatan petugas
    /// (<see cref="StockRequestApprovalAuthority.HasEmergencyGrantAsync"/>). Izin ini hanya
    /// diberikan kepada jabatan operasional yang ditunjuk manajemen (<c>LGS-DEC-004</c>); sampai
    /// ditunjuk, jalur CITO tertutup. Pemohon tetap tidak boleh mengotorisasi permintaannya
    /// sendiri. Alasan wajib; identitas dan waktu otorisasi tercatat di riwayat. Otorisasi yang
    /// sah membuat permintaan segera dapat diproses dan menunggu verifikasi susulan atasan unit
    /// (<see cref="VerifyEmergencyAsync"/>).
    /// </remarks>
    public Task<StockRequestDetailResponse> AuthorizeEmergencyAsync(Guid id,
        AuthorizeEmergencyStockRequestRequest request, CancellationToken cancellationToken = default) =>
        DecideAsync(id, AuthorizeEmergencyAction, StockRequestStatus.Approved, Normalize(request.Reason),
            reasonRequired: true, emergencyOnly: true, request.ExpectedVersion,
            request.IdempotencyKey, "StockRequest.AuthorizeEmergency",
            "Petugas berwenang mengotorisasi permintaan stok CITO.", cancellationToken);

    private async Task<StockRequestDetailResponse> DecideAsync(Guid id, string action,
        StockRequestStatus target, string? reason, bool reasonRequired, bool emergencyOnly,
        int expectedVersion, string idempotencyKey, string auditEvent, string auditMessage,
        CancellationToken cancellationToken)
    {
        EnsureIdempotencyKey(idempotencyKey);

        if (reasonRequired && reason == null)
            throw new StockRequestUnprocessableException("PHM015",
                target == StockRequestStatus.Rejected
                    ? "Alasan penolakan wajib diisi."
                    : "Alasan otorisasi CITO wajib diisi.");

        var actorUserId = GetCurrentUserId();
        var fingerprint = Hash($"{action}:{id}|{reason}");

        var prior = await FindIdempotentAsync(action, idempotencyKey, cancellationToken);
        if (prior != null)
        {
            if (prior.StockRequestId != id)
                throw new StockRequestConflictException("PHM013",
                    "Idempotency key sudah dipakai untuk permintaan lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Permintaan stok tidak ditemukan.");

        // Keputusan hanya sah sekali, dan hanya atas permintaan yang sedang menunggu. Upaya
        // kedua — misalnya dua atasan menekan Setujui hampir bersamaan — ditolak di sini atau
        // oleh token versi saat menyimpan.
        if (entity.Status is StockRequestStatus.Approved or StockRequestStatus.Rejected)
            throw new StockRequestConflictException("PHM018",
                "Permintaan ini sudah diputuskan dan tidak dapat diputuskan ulang.");

        if (entity.Status != StockRequestStatus.Submitted)
            throw new StockRequestConflictException("PHM004",
                "Hanya permintaan yang menunggu persetujuan yang dapat diputuskan.");

        if (emergencyOnly && entity.Priority != StockRequestPriority.Emergency)
            throw new StockRequestUnprocessableException("PHM017",
                "Jalur otorisasi CITO hanya untuk permintaan berprioritas darurat.");

        EnsureVersion(entity.Version, expectedVersion);

        // Kewenangan domain, di atas izin controller. Tidak ikut jalan pintas SuperAdmin maupun
        // saklar pengembangan (lihat StockRequestApprovalAuthority). Sumbernya hanya data server:
        // akun yang login, penempatan jabatan, dan struktur organisasi — bukan isi permintaan.
        var verdict = emergencyOnly
            ? await EvaluateEmergencyAuthorizerAsync(entity, actorUserId, cancellationToken)
            : await EvaluateDeciderAsync(entity, actorUserId, cancellationToken);

        if (!verdict.Allowed)
            throw await DeniedAsync(entity, action, actorUserId, verdict);

        var now = DateTime.UtcNow;
        var from = entity.Status;
        entity.Status = target;
        entity.DecidedAt = now;
        entity.DecisionReason = reason;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockRequestHistories.Add(NewHistory(entity.Id, target, from, action,
            reason, idempotencyKey, fingerprint, actorUserId, now));

        // Satu simpanan: perubahan status dan riwayat masuk bersama, dijaga token versi. Bila
        // keputusan lain tersimpan lebih dulu, simpanan ini gagal utuh (PHM012) dan tidak ada
        // dua keputusan sah atas satu permintaan.
        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, auditEvent, auditMessage,
            new
            {
                entity.Id, entity.RequestNumber, entity.Priority, FromStatus = from, ToStatus = target,
                Action = action, Reason = reason, OccurredAt = now, ActorUserId = actorUserId
            });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ============================================ verifikasi susulan CITO

    /// <summary>
    /// Verifikasi administratif susulan atas permintaan CITO yang diotorisasi lewat jalur
    /// darurat (<c>LGS-DEC-005</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dilakukan atasan unit peminta dengan izin <c>StockRequest:Approve</c> eksplisit. Pemohon
    /// dan petugas yang mengotorisasi CITO tidak boleh memverifikasi permintaan yang sama.
    /// </para>
    /// <para>
    /// Verifikasi <b>tidak mengubah status dan tidak menyentuh stok</b>, apa pun hasilnya. Temuan
    /// penyimpangan tidak membatalkan atau membalik transaksi fisik yang sudah terjadi; ia
    /// tercatat di riwayat dan log audit sebagai bahan tindak lanjut. Penelusuran tindak lanjut
    /// (penanggung jawab, penyelesaian) belum memiliki tempat penyimpanan dan dicatat sebagai
    /// blocker persistence.
    /// </para>
    /// <para>
    /// Verifikasi hanya sah sekali. Versi permintaan ikut dinaikkan, sehingga dua verifikasi yang
    /// berlangsung bersamaan tidak dapat sama-sama tersimpan.
    /// </para>
    /// </remarks>
    public async Task<StockRequestDetailResponse> VerifyEmergencyAsync(Guid id,
        VerifyEmergencyStockRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        if (!Enum.IsDefined(request.Outcome))
            throw new StockRequestUnprocessableException("PHM011",
                "Hasil verifikasi susulan tidak dikenal.");

        var note = Normalize(request.Note);
        if (request.Outcome == StockRequestEmergencyReviewOutcome.Deviation && note == null)
            throw new StockRequestUnprocessableException("PHM008",
                "Temuan penyimpangan wajib disertai catatan pemeriksaan.");

        var action = request.Outcome == StockRequestEmergencyReviewOutcome.Deviation
            ? VerifyEmergencyDeviationAction
            : VerifyEmergencyConfirmedAction;

        var actorUserId = GetCurrentUserId();
        var fingerprint = Hash($"verify-emergency:{id}|{(int)request.Outcome}|{note}");

        // Kunci yang sama dicari pada kedua hasil, supaya kunci yang dipakai ulang dengan hasil
        // berbeda ditolak sebagai benturan, bukan diperlakukan sebagai verifikasi baru.
        var prior = await FindIdempotentAsync(VerifyEmergencyConfirmedAction, request.IdempotencyKey,
                        cancellationToken)
                    ?? await FindIdempotentAsync(VerifyEmergencyDeviationAction, request.IdempotencyKey,
                        cancellationToken);
        if (prior != null)
        {
            if (prior.StockRequestId != id)
                throw new StockRequestConflictException("PHM013",
                    "Idempotency key sudah dipakai untuk permintaan lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Permintaan stok tidak ditemukan.");

        var authorization = FindEmergencyAuthorization(entity)
            ?? throw new StockRequestConflictException("PHM010",
                "Permintaan ini tidak diotorisasi lewat jalur CITO, sehingga tidak memerlukan " +
                "verifikasi susulan.");

        if (FindEmergencyReview(entity) != null)
            throw new StockRequestConflictException("PHM019",
                "Verifikasi susulan untuk permintaan ini sudah tercatat dan tidak dapat diulang.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var verdict = await EvaluateEmergencyVerifierAsync(entity, authorization, actorUserId,
            cancellationToken);
        if (!verdict.Allowed)
            throw await DeniedAsync(entity, action, actorUserId, verdict);

        var now = DateTime.UtcNow;

        // Status sengaja tidak disentuh; yang naik hanya versi, sebagai penjaga verifikasi ganda.
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockRequestHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status,
            action, note, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);

        var auditData = new
        {
            entity.Id, entity.RequestNumber, entity.RequestingServiceUnitId, Status = entity.Status,
            Outcome = request.Outcome, Note = note, OccurredAt = now, ActorUserId = actorUserId,
            AuthorizedByUserId = authorization.ActorUserId, AuthorizedAt = authorization.OccurredAt
        };

        await _loggerService.AuditAsync(LogCategory, "StockRequest.VerifyEmergency",
            request.Outcome == StockRequestEmergencyReviewOutcome.Deviation
                ? "Atasan unit mencatat temuan penyimpangan pada otorisasi CITO."
                : "Atasan unit membenarkan otorisasi CITO.",
            auditData);

        // Temuan penyimpangan dinaikkan juga sebagai peringatan, supaya tertangkap pemantauan
        // dan ditindaklanjuti — transaksinya sendiri tidak dihapus maupun dibalik.
        if (request.Outcome == StockRequestEmergencyReviewOutcome.Deviation)
            await _loggerService.WarningAsync(LogCategory, "StockRequest.EmergencyDeviation",
                "Temuan penyimpangan pada otorisasi CITO memerlukan tindak lanjut.", auditData);

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    /// <summary>
    /// Benar bila pengguna adalah pemohon permintaan: pembuatnya, pengirimnya, atau pemilik
    /// akun dari profil petugas yang tercatat sebagai peminta.
    /// </summary>
    private async Task<bool> IsRequesterAsync(PhmStockRequest entity, Guid userId,
        CancellationToken cancellationToken)
    {
        if (entity.CreateBy == userId) return true;

        if (entity.Histories.Any(h => !h.IsDelete && h.ActorUserId == userId &&
                                      (h.Action == CreateAction || h.Action == SubmitAction)))
            return true;

        return await _dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.WorkforceProfileId == entity.RequestedByWorkforceId,
                cancellationToken);
    }

    // ============================================ perintah gudang

    /// <summary>
    /// Mencatat penyerahan barang oleh gudang, baris per baris.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ini satu-satunya perintah yang dimiliki gudang. Gudang tidak menyetujui dan tidak
    /// menolak permintaan; ia melihat apa yang diminta, lalu mencatat berapa yang benar-benar
    /// diserahkan ketika depo mengambil. Sejak <c>LGS-DEC-002</c> penyerahan hanya sah atas
    /// permintaan berstatus Disetujui. Satu-satunya pengecualian adalah aturan peralihan:
    /// permintaan Submitted yang diajukan sebelum <see cref="ApprovalEnforcedFromConfigKey"/>
    /// masih boleh diserahkan langsung, dan riwayatnya mencatat perpindahan Submitted →
    /// Completed sehingga jalur itu selalu dapat ditelusuri.
    /// </para>
    /// <para>
    /// Perintah ini <b>mengurangi saldo gudang asal</b> dan menulis kartu stok, mengambil
    /// batch yang paling dekat kedaluwarsa lebih dahulu. Bila stoknya tidak mencukupi,
    /// seluruh penyerahan dibatalkan dan status permintaan tidak berubah — tidak ada
    /// permintaan yang tertutup padahal barangnya tidak pernah keluar.
    /// </para>
    /// <para>
    /// Penyerahan sebagian tetap menutup permintaan. Kekurangannya terbaca dari selisih
    /// jumlah diminta dan jumlah diserahkan pada tiap baris; bila unit masih membutuhkan
    /// sisanya, ia membuat permintaan baru — sama seperti permintaan yang perlu diubah
    /// setelah dikirim.
    /// </para>
    /// </remarks>
    public async Task<StockRequestDetailResponse> FulfillAsync(Guid id,
        FulfillStockRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        if (request.Items.Count == 0)
            throw new StockRequestUnprocessableException("PHM002",
                "Penyerahan harus menyebut sekurang-kurangnya satu baris.");

        var actorUserId = GetCurrentUserId();
        var note = Normalize(request.Note);
        var fingerprint = Hash($"fulfill:{id}|{note}|" + string.Join(',',
            request.Items.OrderBy(x => x.StockRequestItemId)
                .Select(x => $"{x.StockRequestItemId:N}:{x.FulfilledQuantity}")));

        var prior = await FindIdempotentAsync(FulfillAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Permintaan stok tidak ditemukan.");

        // LGS-DEC-002: gudang hanya menyerahkan permintaan yang sudah disetujui atasan unit
        // atau diotorisasi lewat jalur CITO. Penjagaan ini di backend, bukan sekadar tombol
        // yang dinonaktifkan di layar. Permintaan lama yang diajukan sebelum aturan ini
        // berlaku tidak diblokir diam-diam: ia mengikuti aturan peralihan yang dikonfigurasi.
        var legacySubmission = IsLegacySubmission(entity);

        if (entity.Status == StockRequestStatus.Submitted && !legacySubmission)
            throw new StockRequestConflictException("PHM016",
                "Permintaan belum disetujui atasan unit, sehingga belum dapat diserahkan gudang.");

        if (entity.Status != StockRequestStatus.Approved && !legacySubmission)
            throw new StockRequestConflictException("PHM004",
                "Hanya permintaan yang sudah disetujui yang dapat diserahkan.");

        var fromStatus = entity.Status;

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var activeItems = entity.Items.Where(x => !x.IsDelete).ToList();
        EnsureFulfillmentCoversEveryLine(activeItems, request.Items);

        var byId = request.Items.ToDictionary(x => x.StockRequestItemId, x => x.FulfilledQuantity);
        var now = DateTime.UtcNow;

        foreach (var item in activeItems)
        {
            var quantity = byId[item.Id];

            if (quantity < 0)
                throw new StockRequestUnprocessableException("PHM006",
                    "Jumlah yang diserahkan tidak boleh kurang dari nol.");

            // Menyerahkan lebih dari yang diminta bukan pemenuhan permintaan ini. Bila
            // gudang memang perlu mengeluarkan lebih, itu pengeluaran atas dasar lain.
            if (quantity > item.RequestedQuantity)
                throw new StockRequestUnprocessableException("PHM006",
                    $"Jumlah diserahkan pada baris {item.LineNumber} melebihi jumlah yang diminta.");

            item.FulfilledQuantity = quantity;
            item.UpdateDateTime = now;
            item.UpdateBy = actorUserId;
        }

        // Stok gudang benar-benar berkurang di sini, bukan sekadar dicatat angkanya.
        //
        // Pengurangan dijalankan sebelum permintaan ditutup, supaya kegagalan karena stok
        // tidak mencukupi membatalkan seluruh penyerahan alih-alih meninggalkan permintaan
        // yang berstatus Selesai padahal barangnya tidak pernah keluar.
        //
        // Yang berkurang hanya stok gudang asal. Ke mana barangnya mendarat adalah urusan
        // kapabilitas transfer, yang masih menunggu penetapan unit mana saja yang berstatus
        // depo dengan saldo sendiri.
        if (_drugStockService != null)
        {
            foreach (var item in activeItems.Where(x => byId[x.Id] > 0))
            {
                try
                {
                    // Perencanaan dan pengeluaran dipisah supaya seluruh baris tersimpan
                    // dalam satu simpanan di akhir. Memakai pengeluaran yang menyimpan
                    // sendiri membuat baris pertama sudah terlanjur mengurangi stok ketika
                    // baris kedua gagal — obat hilang dari gudang tanpa satu pun dokumen
                    // yang menutupinya.
                    var plan = await _drugStockService.PlanFefoAsync(item.DrugId,
                        entity.StorageLocationId, byId[item.Id], cancellationToken);

                    var sequence = 1;
                    foreach (var (batchId, quantity) in plan)
                    {
                        await _drugStockService.IssueBatchAsync(batchId,
                            entity.StorageLocationId, quantity, consumeReservation: false,
                            DrugStockSourceDocumentTypes.StockRequest, entity.Id,
                            $"Penyerahan permintaan {entity.RequestNumber} baris {item.LineNumber}.",
                            $"srq-{item.Id:N}-{sequence++}", cancellationToken);
                    }
                }
                // Kegagalan stok diterjemahkan ke bahasa permintaan supaya petugas gudang
                // membaca sebab yang sebenarnya — obat mana yang kurang — bukan kegagalan
                // umum yang tidak memberi tahu apa pun.
                catch (DrugStockUnprocessableException ex)
                {
                    throw new StockRequestUnprocessableException(ex.Code,
                        $"{item.DrugNameSnapshot}: {ex.Message}");
                }
                catch (DrugStockConflictException ex)
                {
                    throw new StockRequestConflictException(ex.Code,
                        $"{item.DrugNameSnapshot}: {ex.Message}");
                }
            }
        }

        entity.Status = StockRequestStatus.Completed;
        entity.DecidedAt = now;
        entity.DecisionReason = note;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockRequestHistories.Add(NewHistory(entity.Id, StockRequestStatus.Completed,
            fromStatus, FulfillAction, note, request.IdempotencyKey,
            fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "StockRequest.Fulfill",
            legacySubmission
                ? "Mencatat penyerahan barang atas permintaan stok lama dengan aturan peralihan."
                : "Mencatat penyerahan barang atas permintaan stok.",
            new
            {
                entity.Id, entity.RequestNumber, entity.ItemCount, FromStatus = fromStatus,
                LegacyTransition = legacySubmission, ActorUserId = actorUserId
            });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    /// <summary>
    /// Penyerahan harus menyebut setiap baris permintaan, tidak boleh ada yang terlewat.
    /// </summary>
    /// <remarks>
    /// Baris yang tidak disebut sengaja tidak dianggap nol. Diam bisa berarti "tidak
    /// diserahkan" atau "lupa dicatat", dan keduanya berbeda akibatnya bagi unit peminta.
    /// </remarks>
    private static void EnsureFulfillmentCoversEveryLine(List<PhmStockRequestItem> activeItems,
        List<FulfillStockRequestItemInput> inputs)
    {
        if (inputs.GroupBy(x => x.StockRequestItemId).Any(g => g.Count() > 1))
            throw new StockRequestUnprocessableException("PHM007",
                "Satu baris permintaan hanya boleh disebut satu kali pada penyerahan.");

        var activeIds = activeItems.Select(x => x.Id).ToHashSet();
        var inputIds = inputs.Select(x => x.StockRequestItemId).ToHashSet();

        if (inputIds.Except(activeIds).Any())
            throw new StockRequestUnprocessableException("PHM007",
                "Ada baris penyerahan yang bukan bagian dari permintaan ini.");

        if (activeIds.Except(inputIds).Any())
            throw new StockRequestUnprocessableException("PHM007",
                "Setiap baris permintaan harus disebut jumlah penyerahannya, walaupun nol.");
    }

    // ============================================================== penolong

    private Task<PhmStockRequest?> LoadAsync(Guid id, bool tracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.PhmStockRequests
            .Include(x => x.RequestingServiceUnit)
            .Include(x => x.StorageLocation)
            .Include(x => x.RequestedByWorkforce)
            .Include(x => x.Items)
            .Include(x => x.Histories.Where(h => !h.IsDelete))
            .Where(x => x.Id == id && !x.IsDelete);

        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    private void AddItems(Guid requestId, List<StockRequestItemInput> inputs,
        Dictionary<Guid, DrugSnapshot> drugs, Guid actorUserId, DateTime now)
    {
        var line = 1;
        foreach (var input in inputs)
        {
            var drug = drugs[input.DrugId];

            // Ditambahkan lewat DbSet, bukan lewat navigasi induk yang sudah dilacak, agar
            // entity baru pasti berstatus Added walaupun kuncinya diisi dari aplikasi.
            _dbContext.PhmStockRequestItems.Add(new PhmStockRequestItem
            {
                StockRequestId = requestId,
                DrugId = input.DrugId,
                MeasurementId = input.MeasurementId,
                DrugCodeSnapshot = drug.Code,
                DrugNameSnapshot = drug.Name,
                MeasurementNameSnapshot = drug.MeasurementNames.GetValueOrDefault(input.MeasurementId),
                RequestedQuantity = input.RequestedQuantity,
                Note = Normalize(input.Note),
                LineNumber = line++,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
    }

    private sealed record DrugSnapshot(string Code, string Name,
        Dictionary<Guid, string> MeasurementNames);

    /// <summary>
    /// Memastikan seluruh obat dan satuan yang diminta benar-benar ada dan aktif, lalu
    /// mengambil nama serta kodenya sekali saja untuk disalin ke setiap baris.
    /// </summary>
    private async Task<Dictionary<Guid, DrugSnapshot>> ResolveDrugsAsync(
        List<StockRequestItemInput> items, CancellationToken cancellationToken)
    {
        var drugIds = items.Select(x => x.DrugId).Distinct().ToList();
        var measurementIds = items.Select(x => x.MeasurementId).Distinct().ToList();

        var drugs = await _dbContext.MstDrugs.AsNoTracking()
            .Where(x => drugIds.Contains(x.Id) && x.IsActive && !x.IsDelete)
            .Select(x => new { x.Id, x.DrugCode, x.DrugName })
            .ToListAsync(cancellationToken);

        if (drugs.Count != drugIds.Count)
            throw new StockRequestUnprocessableException("PHM001",
                "Ada obat yang tidak ditemukan atau tidak aktif pada master farmasi.");

        var measurements = await _dbContext.MstMeasurements.AsNoTracking()
            .Where(x => measurementIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.MeasurementName })
            .ToListAsync(cancellationToken);

        if (measurements.Count != measurementIds.Count)
            throw new StockRequestUnprocessableException("PHM001",
                "Ada satuan yang tidak ditemukan pada master.");

        var measurementNames = measurements.ToDictionary(x => x.Id, x => x.MeasurementName);

        return drugs.ToDictionary(x => x.Id,
            x => new DrugSnapshot(x.DrugCode, x.DrugName, measurementNames));
    }

    private async Task ValidateReferencesAsync(Guid serviceUnitId, Guid storageLocationId,
        Guid workforceId, CancellationToken cancellationToken)
    {
        var unitValid = await _dbContext.MstServiceUnits.AsNoTracking()
            .AnyAsync(x => x.Id == serviceUnitId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!unitValid)
            throw new StockRequestUnprocessableException("PHM001",
                "Unit peminta tidak ditemukan atau tidak aktif.");

        var storageValid = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .AnyAsync(x => x.Id == storageLocationId && !x.IsDelete, cancellationToken);
        if (!storageValid)
            throw new StockRequestUnprocessableException("PHM001",
                "Lokasi penyimpanan tidak ditemukan.");

        var workforceValid = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!workforceValid)
            throw new StockRequestUnprocessableException("PHM001",
                "Petugas peminta tidak ditemukan atau tidak aktif.");
    }

    /// <summary>
    /// Satu obat hanya boleh muncul sekali. Dua baris obat yang sama membuat gudang
    /// menyiapkan dua kali untuk kebutuhan yang satu.
    /// </summary>
    private static void EnsureItemsValid(List<StockRequestItemInput> items)
    {
        if (items.Count == 0)
            throw new StockRequestUnprocessableException("PHM002",
                "Permintaan harus memuat sekurang-kurangnya satu item.");

        if (items.Any(x => x.RequestedQuantity <= 0))
            throw new StockRequestUnprocessableException("PHM003",
                "Jumlah permintaan setiap item harus lebih dari nol.");

        var duplicate = items.GroupBy(x => x.DrugId).Any(g => g.Count() > 1);
        if (duplicate)
            throw new StockRequestUnprocessableException("PHM005",
                "Satu obat hanya boleh muncul satu kali. Gabungkan jumlahnya menjadi satu baris.");
    }

    private static void EnsureVersion(int current, int expected)
    {
        if (current != expected)
            throw new StockRequestConflictException("PHM012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
    }

    private static void EnsureIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key wajib diisi.");
    }

    private static void EnsureSameFingerprint(string source, string fingerprint)
    {
        if (!string.Equals(source, BuildSource(fingerprint), StringComparison.Ordinal))
            throw new StockRequestConflictException("PHM013",
                "Idempotency key dipakai dengan isi permintaan yang berbeda.");
    }

    private Task<PhmStockRequestHistory?> FindIdempotentAsync(string action, string key,
        CancellationToken cancellationToken) =>
        _dbContext.PhmStockRequestHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Action == action && x.CorrelationId == key.Trim() &&
                                      !x.IsDelete, cancellationToken);

    private static PhmStockRequestHistory NewHistory(Guid requestId, StockRequestStatus to,
        StockRequestStatus? from, string action, string? reason, string idempotencyKey,
        string fingerprint, Guid actorUserId, DateTime now) => new()
        {
            StockRequestId = requestId,
            FromStatus = from,
            ToStatus = to,
            Action = action,
            Reason = reason,
            ActorUserId = actorUserId,
            OccurredAt = now,
            Source = BuildSource(fingerprint),
            CorrelationId = idempotencyKey.Trim(),
            CreateDateTime = now,
            CreateBy = actorUserId
        };

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new StockRequestConflictException("PHM012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private Guid GetCurrentUserId() =>
        TryGetCurrentUserId()
        ?? throw new StockRequestForbiddenException("Identitas pengguna tidak valid.");

    private Guid? TryGetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
    }

    private static string BuildFingerprint(Guid storageLocationId, StockRequestPriority priority,
        DateTime? neededAt, string? notes, List<StockRequestItemInput> items) =>
        Hash(string.Join('|', storageLocationId, (int)priority,
            neededAt?.ToUniversalTime().Ticks, Normalize(notes),
            string.Join(',', items.OrderBy(x => x.DrugId)
                .Select(x => $"{x.DrugId:N}:{x.MeasurementId:N}:{x.RequestedQuantity}"))));

    private static string BuildSource(string fingerprint) => $"API:{fingerprint[..46]}";
    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"StockRequest:{key.Trim()}"))[..16]);

    private static StockRequestDetailResponse MapDetail(PhmStockRequest x) => new()
    {
        Id = x.Id,
        RequestNumber = x.RequestNumber,
        RequestingServiceUnitId = x.RequestingServiceUnitId,
        RequestingServiceUnitName = x.RequestingServiceUnit?.ServiceUnitName ?? string.Empty,
        StorageLocationId = x.StorageLocationId,
        StorageLocationName = x.StorageLocation?.StorageLocationName ?? string.Empty,
        RequestedByWorkforceId = x.RequestedByWorkforceId,
        RequestedByName = x.RequestedByWorkforce?.DisplayName ?? string.Empty,
        Status = x.Status,
        Priority = x.Priority,
        RequestedAt = x.RequestedAt,
        NeededAt = x.NeededAt,
        SubmittedAt = x.SubmittedAt,
        DecidedAt = x.DecidedAt,
        DecisionReason = x.DecisionReason,
        Notes = x.Notes,
        ItemCount = x.ItemCount,
        Version = x.Version,
        IsEditable = x.Status == StockRequestStatus.Draft,
        CanFulfill = x.Status == StockRequestStatus.Approved,
        CanCancel = x.Status is StockRequestStatus.Draft or StockRequestStatus.Submitted
            or StockRequestStatus.Approved,
        CanApprove = x.Status == StockRequestStatus.Submitted,
        CanAuthorizeEmergency = x.Status == StockRequestStatus.Submitted &&
                                x.Priority == StockRequestPriority.Emergency,
        IsPartiallyFulfilled = x.Status == StockRequestStatus.Completed &&
                               x.Items.Any(i => !i.IsDelete && i.FulfilledQuantity != null &&
                                                i.FulfilledQuantity < i.RequestedQuantity),
        Items = [.. x.Items.Where(i => !i.IsDelete).OrderBy(i => i.LineNumber)
            .Select(i => new StockRequestItemResponse
            {
                Id = i.Id,
                DrugId = i.DrugId,
                DrugCode = i.DrugCodeSnapshot,
                DrugName = i.DrugNameSnapshot,
                MeasurementId = i.MeasurementId,
                MeasurementName = i.MeasurementNameSnapshot,
                RequestedQuantity = i.RequestedQuantity,
                FulfilledQuantity = i.FulfilledQuantity,
                Note = i.Note,
                LineNumber = i.LineNumber
            })],
        Histories = [.. x.Histories.Where(h => !h.IsDelete).OrderByDescending(h => h.OccurredAt)
            .Select(h => new StockRequestHistoryResponse
            {
                Id = h.Id,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                Action = h.Action,
                Reason = h.Reason,
                OccurredAt = h.OccurredAt,
                ActorUserId = h.ActorUserId
            })]
    };
}

/// <summary>Benturan data atau transisi status yang tidak sah; dipetakan ke `409`.</summary>
public sealed class StockRequestConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Pengguna tidak berwenang atas tindakan ini; dipetakan ke `403`.</summary>
public sealed class StockRequestForbiddenException(string message) : Exception(message);

/// <summary>Prasyarat aturan permintaan stok belum terpenuhi; dipetakan ke `422`.</summary>
public sealed class StockRequestUnprocessableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
