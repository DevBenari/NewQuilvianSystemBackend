using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;

/// <summary>
/// Alur Batas Pembayaran Langsung (FIN-BE-DPT-001): Pengajuan → Menunggu → Disetujui/Ditolak → Aktifkan.
///
/// Satu-satunya tempat nominal yang berlaku bagi transaksi (<see cref="MstDirectPaymentThreshold"/>, dibaca AR/AP dengan
/// <c>IsActive &amp;&amp; !IsDelete</c>) diubah adalah <see cref="ActivateAsync"/>. <see cref="CreateAsync"/>,
/// <see cref="ApproveAsync"/>, dan <see cref="RejectAsync"/> TIDAK menyentuh baris itu. Tanpa batas aktif, AR/AP tetap fail-closed.
///
/// Setiap tindakan tulis berjalan dalam satu transaksi database di bawah advisory lock yang sama, jadi dua penyetuju, aktivasi
/// ganda, atau dua pengajuan serentak tidak dapat saling menimpa; penanda versi (<c>RowVersion</c>) menolak versi basi dengan 409.
/// SuperAdmin hanya DIKECUALIKAN dari larangan menyetujui pengajuannya sendiri — validasi nominal, status, versi, dan atomisitas
/// tetap berlaku, dan pengecualiannya dicatat pada kejadian (<c>ActorIsSuperAdmin</c>, <c>IsSelfApproval</c>).
/// </summary>
public sealed class DirectPaymentThresholdProposalService
{
    private const string LogCategory = "Corporate.FinanceManagement.MasterData";
    private const string WorkflowLockKey = "FIN_DIRECT_PAYMENT_THRESHOLD_WORKFLOW";
    private const decimal MaxAmount = 9_999_999_999_999_999.99m; // decimal(18,2)
    private const int MaxTextLength = 500;
    private const string StaleMessage = "Pengajuan sudah diubah oleh orang lain. Muat ulang sebelum melanjutkan.";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public DirectPaymentThresholdProposalService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------ pembacaan

    public async Task<DirectPaymentThresholdProposalSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var countRows = await _dbContext.MstDirectPaymentThresholdProposals.AsNoTracking()
            .Where(x => !x.IsDelete)
            .GroupBy(x => new { x.ApprovalStatus, x.ActivationStatus })
            .Select(g => new { g.Key.ApprovalStatus, g.Key.ActivationStatus, Total = g.Count() })
            .ToListAsync(cancellationToken);

        var activeMaster = await _dbContext.MstDirectPaymentThresholds.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
        var activeProposal = await _dbContext.MstDirectPaymentThresholdProposals.AsNoTracking()
            .Where(x => !x.IsDelete && x.ActivationStatus == DirectPaymentThresholdActivationStatuses.Active)
            .FirstOrDefaultAsync(cancellationToken);

        var activeVersion = new DirectPaymentThresholdActiveVersionResponse();
        if (activeMaster is null)
        {
            activeVersion.Source = DirectPaymentThresholdActiveSources.None;
        }
        else if (activeProposal is not null && activeProposal.ProposedAmount == activeMaster.Amount)
        {
            var names = await ResolveNamesAsync(
                [activeProposal.ActivatedBy ?? Guid.Empty, activeProposal.ApprovedBy ?? Guid.Empty], cancellationToken);
            activeVersion.Source = DirectPaymentThresholdActiveSources.Proposal;
            activeVersion.ProposalNumber = activeProposal.ProposalNumber;
            activeVersion.Amount = activeMaster.Amount;
            activeVersion.EffectiveFrom = activeProposal.EffectiveFrom;
            activeVersion.ActivatedAt = activeProposal.ActivatedAt;
            activeVersion.ActivatedByName = NameOf(names, activeProposal.ActivatedBy);
            activeVersion.ApprovedAt = activeProposal.ApprovedAt;
            activeVersion.ApprovedByName = NameOf(names, activeProposal.ApprovedBy);
        }
        else
        {
            // Batas yang aktif sebelum alur persetujuan ada (atau tidak cocok dengan pengajuan mana pun): tanggal berlaku dan
            // persetujuannya TIDAK dapat dibuktikan, jadi tidak dikarang dari CreateDateTime.
            activeVersion.Source = DirectPaymentThresholdActiveSources.Legacy;
            activeVersion.Amount = activeMaster.Amount;
        }

        return new DirectPaymentThresholdProposalSummaryResponse
        {
            TotalProposals = countRows.Sum(x => x.Total),
            WaitingProposals = countRows
                .Where(x => x.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Waiting).Sum(x => x.Total),
            ApprovedNotActiveProposals = countRows
                .Where(x => x.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Approved
                            && x.ActivationStatus == DirectPaymentThresholdActivationStatuses.NotActive)
                .Sum(x => x.Total),
            RejectedProposals = countRows
                .Where(x => x.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Rejected).Sum(x => x.Total),
            ActiveVersion = activeVersion
        };
    }

    public async Task<PagedResult<DirectPaymentThresholdProposalResponse>> GetProposalsAsync(
        DirectPaymentThresholdProposalQuery request, Guid viewerUserId, bool viewerIsSuperAdmin, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstDirectPaymentThresholdProposals.AsNoTracking().Where(x => !x.IsDelete);

        var approvalStatus = NormalizeFilter(request.ApprovalStatus, DirectPaymentThresholdApprovalStatuses.All, "status pengajuan");
        if (approvalStatus is not null) query = query.Where(x => x.ApprovalStatus == approvalStatus);

        var activationStatus = NormalizeFilter(request.ActivationStatus, DirectPaymentThresholdActivationStatuses.All, "status aktivasi");
        if (activationStatus is not null) query = query.Where(x => x.ActivationStatus == activationStatus);

        var (fromUtc, toExclusiveUtc) = ResolveRange(request.StartDate, request.EndDate);
        if (fromUtc.HasValue) query = query.Where(x => x.RequestedAt >= fromUtc.Value);
        if (toExclusiveUtc.HasValue) query = query.Where(x => x.RequestedAt < toExclusiveUtc.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = ContainsPattern(request.Search);
            query = query.Where(x => EF.Functions.ILike(x.ProposalNumber, pattern, "!"));
        }

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(x => x.RequestedAt).ThenByDescending(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DirectPaymentThresholdProposalResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = await BuildResponsesAsync(entities, viewerUserId, viewerIsSuperAdmin, cancellationToken)
        };
    }

    public async Task<DirectPaymentThresholdProposalResponse> GetProposalAsync(
        Guid id, Guid viewerUserId, bool viewerIsSuperAdmin, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.MstDirectPaymentThresholdProposals.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan batas pembayaran langsung tidak ditemukan.");

        return (await BuildResponsesAsync(new List<MstDirectPaymentThresholdProposal> { entity }, viewerUserId, viewerIsSuperAdmin, cancellationToken))[0];
    }

    public async Task<PagedResult<DirectPaymentThresholdHistoryResponse>> GetHistoryAsync(
        DirectPaymentThresholdHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstDirectPaymentThresholdEvents.AsNoTracking().Where(x => !x.IsDelete);

        var eventType = NormalizeFilter(request.EventType, DirectPaymentThresholdEventTypes.All, "jenis kejadian");
        if (eventType is not null) query = query.Where(x => x.EventType == eventType);

        var (fromUtc, toExclusiveUtc) = ResolveRange(request.StartDate, request.EndDate);
        if (fromUtc.HasValue) query = query.Where(x => x.OccurredAt >= fromUtc.Value);
        if (toExclusiveUtc.HasValue) query = query.Where(x => x.OccurredAt < toExclusiveUtc.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = ContainsPattern(request.Search);
            query = query.Where(x => EF.Functions.ILike(x.Proposal!.ProposalNumber, pattern, "!"));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new { Event = x, ProposalNumber = x.Proposal!.ProposalNumber })
            .ToListAsync(cancellationToken);

        var names = await ResolveNamesAsync(rows.Select(x => x.Event.ActorUserId), cancellationToken);

        var relatedIds = rows.Where(x => x.Event.RelatedProposalId.HasValue)
            .Select(x => x.Event.RelatedProposalId!.Value).Distinct().ToList();
        var relatedNumbers = relatedIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.MstDirectPaymentThresholdProposals.AsNoTracking()
                .Where(x => relatedIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.ProposalNumber, cancellationToken);

        return new PagedResult<DirectPaymentThresholdHistoryResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = rows.Select(x => new DirectPaymentThresholdHistoryResponse
            {
                Id = x.Event.Id,
                ProposalId = x.Event.ProposalId,
                ProposalNumber = x.ProposalNumber,
                RelatedProposalNumber = x.Event.RelatedProposalId.HasValue
                    && relatedNumbers.TryGetValue(x.Event.RelatedProposalId.Value, out var related) ? related : null,
                EventType = x.Event.EventType,
                PreviousAmount = x.Event.PreviousAmount,
                NewAmount = x.Event.NewAmount,
                ActorName = names.TryGetValue(x.Event.ActorUserId, out var actor) ? actor : null,
                OccurredAt = x.Event.OccurredAt,
                StatusBefore = x.Event.StatusBefore,
                StatusAfter = x.Event.StatusAfter,
                Reason = x.Event.Reason,
                ActorIsSuperAdmin = x.Event.ActorIsSuperAdmin,
                IsSelfApproval = x.Event.IsSelfApproval,
                IsLegacyBaseline = x.Event.IsLegacyBaseline
            }).ToList()
        };
    }

    // ------------------------------------------------------------------ tindakan tulis

    /// <summary>
    /// Membuat pengajuan berstatus MENUNGGU + BELUM_AKTIF. TIDAK mengubah batas aktif. Hanya satu pengajuan terbuka (Menunggu atau
    /// Disetujui-belum-aktif) pada satu waktu supaya dua pengajuan tidak saling menimpa saat aktivasi.
    /// </summary>
    public async Task<DirectPaymentThresholdProposalResponse> CreateAsync(
        CreateDirectPaymentThresholdProposalRequest request, Guid actorUserId, bool actorIsSuperAdmin, CancellationToken cancellationToken)
    {
        // Urutan pemeriksaan mengikuti kontrak lama: alasan kosong (422) → nominal tidak sah (400) → benturan (409).
        var reason = NormalizeText(request.ChangeReason, "Alasan pengajuan perubahan batas wajib diisi.");
        if (request.Amount <= 0 || request.Amount > MaxAmount || decimal.Round(request.Amount, 2) != request.Amount)
        {
            throw new DirectPaymentThresholdBadRequestException(
                "Batas harus berupa angka lebih besar dari nol dengan maksimal dua digit desimal.");
        }

        var response = await InWorkflowTransactionAsync(async () =>
        {
            var open = await _dbContext.MstDirectPaymentThresholdProposals.AsNoTracking()
                .Where(x => !x.IsDelete
                    && (x.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Waiting
                        || (x.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Approved
                            && x.ActivationStatus == DirectPaymentThresholdActivationStatuses.NotActive)))
                .Select(x => new { x.ProposalNumber, x.ApprovalStatus })
                .FirstOrDefaultAsync(cancellationToken);
            if (open is not null)
            {
                var stage = open.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Waiting
                    ? "menunggu persetujuan"
                    : "sudah disetujui dan menunggu aktivasi";
                // Pelanggaran aturan bisnis (422), BUKAN benturan versi (409): 409 hanya berarti "data sudah diubah pihak lain",
                // dan layar menerjemahkannya menjadi pesan tetap yang menyembunyikan alasan ini.
                throw new DirectPaymentThresholdValidationException(
                    $"Masih ada pengajuan {open.ProposalNumber} yang {stage}. Selesaikan atau tolak pengajuan itu sebelum membuat pengajuan baru.");
            }

            var active = await _dbContext.MstDirectPaymentThresholds.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
            if (active is not null && active.Amount == request.Amount)
            {
                throw new DirectPaymentThresholdBadRequestException(
                    "Nominal yang diajukan sama dengan batas pembayaran langsung yang sedang berlaku.");
            }

            var now = DateTime.UtcNow;
            var proposal = new MstDirectPaymentThresholdProposal
            {
                ProposedAmount = request.Amount,
                PreviousActiveAmount = active?.Amount,
                ChangeReason = reason,
                ApprovalStatus = DirectPaymentThresholdApprovalStatuses.Waiting,
                ActivationStatus = DirectPaymentThresholdActivationStatuses.NotActive,
                RequestedBy = actorUserId,
                RequestedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            proposal.ProposalNumber = BuildProposalNumber(proposal.Id, now);

            _dbContext.MstDirectPaymentThresholdProposals.Add(proposal);
            AddEvent(proposal, DirectPaymentThresholdEventTypes.ProposalCreated, active?.Amount, proposal.ProposedAmount,
                actorUserId, actorIsSuperAdmin, false, null, StatusText(proposal), reason, now);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (await BuildResponsesAsync(new List<MstDirectPaymentThresholdProposal> { proposal }, actorUserId, actorIsSuperAdmin, cancellationToken))[0];
        }, cancellationToken);

        await AuditAsync(DirectPaymentThresholdEventTypes.ProposalCreated, response, actorUserId, actorIsSuperAdmin);
        return response;
    }

    /// <summary>MENUNGGU → DISETUJUI. TIDAK mengaktifkan nominal. Pengaju non-SuperAdmin tidak boleh menyetujui pengajuannya sendiri.</summary>
    public async Task<DirectPaymentThresholdProposalResponse> ApproveAsync(
        Guid id, ApproveDirectPaymentThresholdProposalRequest request, Guid actorUserId, bool actorIsSuperAdmin, CancellationToken cancellationToken)
    {
        var response = await InWorkflowTransactionAsync(async () =>
        {
            var proposal = await FindTrackedAsync(id, cancellationToken);
            EnsureVersion(proposal, request.ExpectedRowVersion);

            if (proposal.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Approved)
                throw new DirectPaymentThresholdBadRequestException("Pengajuan ini sudah disetujui.");
            if (proposal.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Rejected)
                throw new DirectPaymentThresholdBadRequestException("Pengajuan yang sudah ditolak tidak dapat disetujui.");

            var isSelf = proposal.RequestedBy == actorUserId;
            // Kewenangan yang melekat pada data (bukan izin Role Management, jadi bukan 403): pengaju non-SuperAdmin tidak boleh
            // menyetujui pengajuannya sendiri. Pengecualian SuperAdmin khusus fitur ini dan tercatat pada kejadian.
            if (isSelf && !actorIsSuperAdmin)
                throw new DirectPaymentThresholdValidationException("Pengaju tidak dapat menyetujui pengajuannya sendiri.");

            var before = StatusText(proposal);
            var now = DateTime.UtcNow;
            proposal.ApprovalStatus = DirectPaymentThresholdApprovalStatuses.Approved;
            proposal.ApprovedBy = actorUserId;
            proposal.ApprovedAt = now;
            Touch(proposal, actorUserId, now);

            AddEvent(proposal, DirectPaymentThresholdEventTypes.ProposalApproved, proposal.PreviousActiveAmount, proposal.ProposedAmount,
                actorUserId, actorIsSuperAdmin, isSelf, before, StatusText(proposal), null, now);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (await BuildResponsesAsync(new List<MstDirectPaymentThresholdProposal> { proposal }, actorUserId, actorIsSuperAdmin, cancellationToken))[0];
        }, cancellationToken);

        await AuditAsync(DirectPaymentThresholdEventTypes.ProposalApproved, response, actorUserId, actorIsSuperAdmin);
        return response;
    }

    /// <summary>MENUNGGU → DITOLAK. Alasan penolakan wajib. Pengaju boleh menolak pengajuannya sendiri (menarik pengajuan).</summary>
    public async Task<DirectPaymentThresholdProposalResponse> RejectAsync(
        Guid id, RejectDirectPaymentThresholdProposalRequest request, Guid actorUserId, bool actorIsSuperAdmin, CancellationToken cancellationToken)
    {
        var rejectionReason = NormalizeText(request.RejectionReason, "Alasan penolakan wajib diisi.");

        var response = await InWorkflowTransactionAsync(async () =>
        {
            var proposal = await FindTrackedAsync(id, cancellationToken);
            EnsureVersion(proposal, request.ExpectedRowVersion);

            if (proposal.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Rejected)
                throw new DirectPaymentThresholdBadRequestException("Pengajuan ini sudah ditolak.");
            if (proposal.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Approved)
                throw new DirectPaymentThresholdBadRequestException(
                    "Pengajuan yang sudah disetujui tidak dapat ditolak. Aktifkan, atau biarkan digantikan oleh pengajuan berikutnya.");

            var before = StatusText(proposal);
            var now = DateTime.UtcNow;
            proposal.ApprovalStatus = DirectPaymentThresholdApprovalStatuses.Rejected;
            proposal.RejectedBy = actorUserId;
            proposal.RejectedAt = now;
            proposal.RejectionReason = rejectionReason;
            Touch(proposal, actorUserId, now);

            AddEvent(proposal, DirectPaymentThresholdEventTypes.ProposalRejected, proposal.PreviousActiveAmount, proposal.ProposedAmount,
                actorUserId, actorIsSuperAdmin, false, before, StatusText(proposal), rejectionReason, now);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (await BuildResponsesAsync(new List<MstDirectPaymentThresholdProposal> { proposal }, actorUserId, actorIsSuperAdmin, cancellationToken))[0];
        }, cancellationToken);

        await AuditAsync(DirectPaymentThresholdEventTypes.ProposalRejected, response, actorUserId, actorIsSuperAdmin);
        return response;
    }

    /// <summary>
    /// DISETUJUI + BELUM_AKTIF → DISETUJUI + AKTIF. SATU-SATUNYA tindakan yang mengubah <see cref="MstDirectPaymentThreshold"/>.
    /// Dalam satu transaksi: batas lama diperbarui di tempat (atau dibuat bila belum ada), pengajuan lama yang aktif menjadi
    /// DIGANTIKAN, pengajuan ini menjadi AKTIF dengan Tanggal Berlaku = waktu aktivasi, dan kejadian ditulis. Gagal di tengah
    /// berarti tidak ada yang berubah.
    /// </summary>
    public async Task<DirectPaymentThresholdProposalResponse> ActivateAsync(
        Guid id, ActivateDirectPaymentThresholdProposalRequest request, Guid actorUserId, bool actorIsSuperAdmin, CancellationToken cancellationToken)
    {
        var response = await InWorkflowTransactionAsync(async () =>
        {
            var proposal = await FindTrackedAsync(id, cancellationToken);
            EnsureVersion(proposal, request.ExpectedRowVersion);

            if (proposal.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Waiting)
                throw new DirectPaymentThresholdBadRequestException("Pengajuan ini masih menunggu persetujuan dan belum dapat diaktifkan.");
            if (proposal.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Rejected)
                throw new DirectPaymentThresholdBadRequestException("Pengajuan yang ditolak tidak dapat diaktifkan.");
            if (proposal.ActivationStatus == DirectPaymentThresholdActivationStatuses.Active)
                throw new DirectPaymentThresholdBadRequestException("Pengajuan ini sudah aktif.");
            if (proposal.ActivationStatus == DirectPaymentThresholdActivationStatuses.Superseded)
                throw new DirectPaymentThresholdBadRequestException("Pengajuan ini sudah digantikan dan tidak dapat diaktifkan lagi.");

            var now = DateTime.UtcNow;
            var before = StatusText(proposal);

            var master = await _dbContext.MstDirectPaymentThresholds
                .SingleOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
            var replacedProposal = await _dbContext.MstDirectPaymentThresholdProposals
                .FirstOrDefaultAsync(x => !x.IsDelete && x.Id != proposal.Id
                    && x.ActivationStatus == DirectPaymentThresholdActivationStatuses.Active, cancellationToken);

            var previousAmount = master?.Amount;

            if (master is null)
            {
                // Penetapan PERTAMA: belum ada baris aktif. Baris dibuat di sini, bukan saat pengajuan atau persetujuan.
                master = new MstDirectPaymentThreshold
                {
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                _dbContext.MstDirectPaymentThresholds.Add(master);
            }
            else
            {
                master.UpdateDateTime = now;
                master.UpdateBy = actorUserId;
            }

            master.Amount = proposal.ProposedAmount;
            master.ChangeReason = proposal.ChangeReason;
            master.RowVersion = Guid.NewGuid();

            if (replacedProposal is not null)
            {
                replacedProposal.ActivationStatus = DirectPaymentThresholdActivationStatuses.Superseded;
                replacedProposal.SupersededAt = now;
                replacedProposal.SupersededBy = actorUserId;
                replacedProposal.SupersededByProposalId = proposal.Id;
                Touch(replacedProposal, actorUserId, now);
            }

            proposal.ActivationStatus = DirectPaymentThresholdActivationStatuses.Active;
            proposal.ActivatedBy = actorUserId;
            proposal.ActivatedAt = now;
            proposal.EffectiveFrom = now;
            proposal.ActivatedPreviousAmount = previousAmount;
            Touch(proposal, actorUserId, now);

            AddEvent(proposal, DirectPaymentThresholdEventTypes.ThresholdActivated, previousAmount, proposal.ProposedAmount,
                actorUserId, actorIsSuperAdmin, false, before, StatusText(proposal), null, now,
                relatedProposalId: replacedProposal?.Id);

            if (previousAmount.HasValue)
            {
                // Batas lama tetap tercatat sebagai versi historis: pengajuan yang digantikan, atau — bila tidak ada pengajuan
                // (batas aktif sebelum alur ini ada) — baseline legacy tanpa persetujuan atau tanggal berlaku fiktif.
                var superseded = replacedProposal ?? proposal;
                AddEvent(superseded, DirectPaymentThresholdEventTypes.ThresholdSuperseded, previousAmount, proposal.ProposedAmount,
                    actorUserId, actorIsSuperAdmin, false,
                    replacedProposal is null ? null : $"{replacedProposal.ApprovalStatus}/{DirectPaymentThresholdActivationStatuses.Active}",
                    replacedProposal is null ? StatusText(proposal) : StatusText(replacedProposal), null, now,
                    relatedProposalId: replacedProposal is null ? null : proposal.Id,
                    isLegacyBaseline: replacedProposal is null);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (await BuildResponsesAsync(new List<MstDirectPaymentThresholdProposal> { proposal }, actorUserId, actorIsSuperAdmin, cancellationToken))[0];
        }, cancellationToken);

        await AuditAsync(DirectPaymentThresholdEventTypes.ThresholdActivated, response, actorUserId, actorIsSuperAdmin);
        return response;
    }

    // ------------------------------------------------------------------ pembantu

    /// <summary>
    /// Satu transaksi + satu advisory lock untuk SELURUH tindakan tulis alur ini (pola kunci yang sama dengan modul Finance lain).
    /// Lock lepas saat transaksi selesai; keluar tanpa Commit (mis. karena penolakan domain) berarti rollback otomatis.
    /// </summary>
    private async Task<T> InWorkflowTransactionAsync<T>(Func<Task<T>> body, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [WorkflowLockKey], cancellationToken);

        try
        {
            var result = await body();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DirectPaymentThresholdConflictException(StaleMessage);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new DirectPaymentThresholdConflictException(StaleMessage);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        (exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation;

    private async Task<MstDirectPaymentThresholdProposal> FindTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstDirectPaymentThresholdProposals.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Pengajuan batas pembayaran langsung tidak ditemukan.");

    // Basi ATAU tidak dikirim → 409, bukan ditimpa. Dibaca pertama supaya klik ganda mendapat jawaban "sudah diubah", bukan galat status.
    private static void EnsureVersion(MstDirectPaymentThresholdProposal proposal, Guid? expectedRowVersion)
    {
        if (expectedRowVersion is null || expectedRowVersion == Guid.Empty || expectedRowVersion != proposal.RowVersion)
            throw new DirectPaymentThresholdConflictException(StaleMessage);
    }

    private static void Touch(MstDirectPaymentThresholdProposal proposal, Guid actorUserId, DateTime now)
    {
        proposal.UpdateDateTime = now;
        proposal.UpdateBy = actorUserId;
        // Putar penanda versi: nilai lama tetap dipakai EF pada klausa WHERE, nilai baru menjadi versi berikutnya.
        proposal.RowVersion = Guid.NewGuid();
    }

    private static string StatusText(MstDirectPaymentThresholdProposal proposal) =>
        $"{proposal.ApprovalStatus}/{proposal.ActivationStatus}";

    private static string BuildProposalNumber(Guid id, DateTime utcNow) =>
        $"BPL-{AppDateTimeHelper.UtcToLocal(utcNow):yyyyMMdd}-{id.ToString("N")[..8].ToUpperInvariant()}";

    private static string NormalizeText(string? value, string requiredMessage)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DirectPaymentThresholdValidationException(requiredMessage);
        var text = value.Trim();
        if (text.Length > MaxTextLength)
            throw new DirectPaymentThresholdValidationException($"Teks maksimal {MaxTextLength} karakter.");
        return text;
    }

    private static string? NormalizeFilter(string? value, IReadOnlyCollection<string> allowed, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToUpperInvariant();
        if (!allowed.Contains(normalized))
            throw new DirectPaymentThresholdBadRequestException($"Filter {label} tidak dikenali: {value.Trim()}.");
        return normalized;
    }

    // Tanggal operasional (WIB) → batas UTC: awal hari pertama (inklusif) dan awal hari setelah tanggal akhir (eksklusif).
    private static (DateTime? FromUtc, DateTime? ToExclusiveUtc) ResolveRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new DirectPaymentThresholdBadRequestException("Tanggal akhir tidak boleh sebelum tanggal awal.");

        DateTime? rangeStartUtc = startDate.HasValue
            ? AppDateTimeHelper.OperationalDateToUtc(startDate.Value.ToDateTime(TimeOnly.MinValue))
            : null;
        DateTime? rangeEndExclusiveUtc = endDate.HasValue
            ? AppDateTimeHelper.OperationalDateToUtc(endDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue))
            : null;
        return (rangeStartUtc, rangeEndExclusiveUtc);
    }

    // Karakter pelarian "!" (bukan backslash) agar aman pada konfigurasi Npgsql; %, _, dan ! pada input diperlakukan harfiah.
    private static string ContainsPattern(string text)
    {
        var escaped = text.Trim().Replace("!", "!!").Replace("%", "!%").Replace("_", "!_");
        return $"%{escaped}%";
    }

    private void AddEvent(
        MstDirectPaymentThresholdProposal proposal, string eventType, decimal? previousAmount, decimal? newAmount,
        Guid actorUserId, bool actorIsSuperAdmin, bool isSelfApproval, string? statusBefore, string statusAfter,
        string? reason, DateTime now, Guid? relatedProposalId = null, bool isLegacyBaseline = false)
    {
        _dbContext.MstDirectPaymentThresholdEvents.Add(new MstDirectPaymentThresholdEvent
        {
            ProposalId = proposal.Id,
            RelatedProposalId = relatedProposalId,
            EventType = eventType,
            PreviousAmount = previousAmount,
            NewAmount = newAmount,
            ActorUserId = actorUserId,
            OccurredAt = now,
            StatusBefore = statusBefore,
            StatusAfter = statusAfter,
            Reason = reason,
            ActorIsSuperAdmin = actorIsSuperAdmin,
            IsSelfApproval = isSelfApproval,
            IsLegacyBaseline = isLegacyBaseline,
            CreateDateTime = now,
            CreateBy = actorUserId
        });
    }

    private Task AuditAsync(string eventType, DirectPaymentThresholdProposalResponse response, Guid actorUserId, bool actorIsSuperAdmin) =>
        _loggerService.AuditAsync(LogCategory, $"DirectPaymentThreshold.{eventType}", "Alur Batas Pembayaran Langsung.", new
        {
            ProposalId = response.Id,
            response.ProposalNumber,
            response.PreviousActiveAmount,
            response.ProposedAmount,
            response.ApprovalStatus,
            response.ActivationStatus,
            ActorUserId = actorUserId,
            ActorIsSuperAdmin = actorIsSuperAdmin
        });

    private async Task<Dictionary<Guid, string>> ResolveNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Where(x => x != Guid.Empty).Distinct().ToList();
        var names = new Dictionary<Guid, string>();
        if (ids.Count == 0) return names;

        // Nama DIBACA saat menyusun respons dari audit (RequestedBy/ApprovedBy/…), tidak disalin ke tabel Finance dan tidak diganti
        // nama pengguna yang sedang login. Tidak ditemukan → tidak ada entri; pemanggil MUST NOT menggantinya dengan ID mentah.
        var users = await _dbContext.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName, u.UserName, u.UserCode })
            .ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            var name = !string.IsNullOrWhiteSpace(user.DisplayName) ? user.DisplayName
                : !string.IsNullOrWhiteSpace(user.UserName) ? user.UserName
                : user.UserCode;
            if (!string.IsNullOrWhiteSpace(name)) names[user.Id] = name;
        }

        return names;
    }

    private static string? NameOf(Dictionary<Guid, string> names, Guid? userId) =>
        userId.HasValue && names.TryGetValue(userId.Value, out var name) ? name : null;

    private async Task<List<DirectPaymentThresholdProposalResponse>> BuildResponsesAsync(
        IReadOnlyCollection<MstDirectPaymentThresholdProposal> proposals, Guid viewerUserId, bool viewerIsSuperAdmin,
        CancellationToken cancellationToken)
    {
        if (proposals.Count == 0) return new List<DirectPaymentThresholdProposalResponse>();

        var actorIds = new List<Guid>();
        foreach (var p in proposals)
        {
            actorIds.Add(p.RequestedBy);
            if (p.ApprovedBy.HasValue) actorIds.Add(p.ApprovedBy.Value);
            if (p.RejectedBy.HasValue) actorIds.Add(p.RejectedBy.Value);
            if (p.ActivatedBy.HasValue) actorIds.Add(p.ActivatedBy.Value);
            if (p.SupersededBy.HasValue) actorIds.Add(p.SupersededBy.Value);
        }

        var names = await ResolveNamesAsync(actorIds, cancellationToken);

        var supersederIds = proposals.Where(p => p.SupersededByProposalId.HasValue)
            .Select(p => p.SupersededByProposalId!.Value).Distinct().ToList();
        var supersederNumbers = supersederIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.MstDirectPaymentThresholdProposals.AsNoTracking()
                .Where(x => supersederIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.ProposalNumber, cancellationToken);

        return proposals
            .Select(p => MapProposal(p, names, supersederNumbers, viewerUserId, viewerIsSuperAdmin))
            .ToList();
    }

    private static DirectPaymentThresholdProposalResponse MapProposal(
        MstDirectPaymentThresholdProposal p, Dictionary<Guid, string> names, Dictionary<Guid, string> supersederNumbers,
        Guid viewerUserId, bool viewerIsSuperAdmin)
    {
        // Update Terakhir = tahap terbaru pada pengajuan ini. RequestedAt/ApprovedAt/ActivatedAt/… tidak dicampur:
        // masing-masing tetap tampil sebagai kolomnya sendiri; hanya yang paling baru menjadi "terakhir".
        var stages = new List<(DateTime At, Guid? By, string Action)>
        {
            (p.RequestedAt, p.RequestedBy, DirectPaymentThresholdEventTypes.ProposalCreated)
        };
        if (p.ApprovedAt.HasValue) stages.Add((p.ApprovedAt.Value, p.ApprovedBy, DirectPaymentThresholdEventTypes.ProposalApproved));
        if (p.RejectedAt.HasValue) stages.Add((p.RejectedAt.Value, p.RejectedBy, DirectPaymentThresholdEventTypes.ProposalRejected));
        if (p.ActivatedAt.HasValue) stages.Add((p.ActivatedAt.Value, p.ActivatedBy, DirectPaymentThresholdEventTypes.ThresholdActivated));
        if (p.SupersededAt.HasValue) stages.Add((p.SupersededAt.Value, p.SupersededBy, DirectPaymentThresholdEventTypes.ThresholdSuperseded));
        var last = stages.OrderByDescending(x => x.At).First();

        var isOwn = p.RequestedBy == viewerUserId;
        var actions = new List<string>();
        if (p.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Waiting)
        {
            if (!isOwn || viewerIsSuperAdmin) actions.Add("Approve");
            actions.Add("Reject");
        }
        else if (p.ApprovalStatus == DirectPaymentThresholdApprovalStatuses.Approved
                 && p.ActivationStatus == DirectPaymentThresholdActivationStatuses.NotActive)
        {
            actions.Add("Activate");
        }

        return new DirectPaymentThresholdProposalResponse
        {
            Id = p.Id,
            ProposalNumber = p.ProposalNumber,
            ProposedAmount = p.ProposedAmount,
            PreviousActiveAmount = p.PreviousActiveAmount,
            AmountDifference = p.PreviousActiveAmount.HasValue ? p.ProposedAmount - p.PreviousActiveAmount.Value : null,
            ChangeReason = p.ChangeReason,
            ApprovalStatus = p.ApprovalStatus,
            ActivationStatus = p.ActivationStatus,
            RequestedByName = NameOf(names, p.RequestedBy),
            RequestedAt = p.RequestedAt,
            ApprovedByName = NameOf(names, p.ApprovedBy),
            ApprovedAt = p.ApprovedAt,
            RejectedByName = NameOf(names, p.RejectedBy),
            RejectedAt = p.RejectedAt,
            RejectionReason = p.RejectionReason,
            ActivatedByName = NameOf(names, p.ActivatedBy),
            ActivatedAt = p.ActivatedAt,
            EffectiveFrom = p.EffectiveFrom,
            ActivatedPreviousAmount = p.ActivatedPreviousAmount,
            SupersededAt = p.SupersededAt,
            SupersededByName = NameOf(names, p.SupersededBy),
            SupersededByProposalNumber = p.SupersededByProposalId.HasValue
                && supersederNumbers.TryGetValue(p.SupersededByProposalId.Value, out var number) ? number : null,
            LastUpdatedAt = last.At,
            LastUpdatedByName = NameOf(names, last.By),
            LastUpdatedAction = last.Action,
            RowVersion = p.RowVersion,
            AvailableActions = actions,
            IsOwnProposal = isOwn
        };
    }
}
