using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Pemilik tagihan iuran membership non-kunjungan (<see cref="BilMembershipFeeCharge"/>). Billing memiliki tagihan
/// dan kewajiban bayar; pembayaran memakai mesin settlement/tender Kasir yang sudah ada
/// (<c>Purpose = MEMBERSHIP_FEE</c>), dan CRM tetap memiliki siklus hidup membership
/// (<see cref="PatientMembershipFeeLinkService"/>). Aktivasi membership hanya terjadi dari settlement yang
/// berhasil diverifikasi server; tidak ada jalan pintas untuk mengaktifkan membership dari sini.
/// </summary>
public sealed class BillingMembershipFeeService : IMembershipFeeChargePort
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing.MembershipFee";
    private const int RetryBatchSize = 100;
    private const int ReconciliationLimit = 200;

    /// <summary>
    /// Kebijakan pembayaran bertahap (cicilan) iuran belum ditetapkan owner (DEC-CRM-001), sehingga iuran
    /// wajib dibayar penuh dalam satu settlement. Backend menolak nominal lain secara eksplisit.
    /// </summary>
    public const bool AllowPartialPayment = false;

    private readonly ApplicationDbContext _dbContext;
    private readonly BillingNumberSeriesService _numberSeries;
    private readonly PatientMembershipFeeLinkService _membershipLink;
    private readonly LoggerService _loggerService;
    private readonly TimeProvider _timeProvider;

    public BillingMembershipFeeService(
        ApplicationDbContext dbContext,
        BillingNumberSeriesService numberSeries,
        PatientMembershipFeeLinkService membershipLink,
        LoggerService loggerService,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _numberSeries = numberSeries;
        _membershipLink = membershipLink;
        _loggerService = loggerService;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    // ------------------------------------------------------------------------------------------------
    // Pembuatan tagihan (INT-06)
    // ------------------------------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<MembershipFeeChargeRef> EnsureChargeAsync(
        MembershipFeeChargeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.MembershipId == Guid.Empty || request.PatientId == Guid.Empty)
            throw new BillingMembershipFeeValidationException("Membership dan pasien wajib diisi.");
        if (request.FeeAmount <= 0)
            throw new BillingMembershipFeeValidationException("Nominal iuran harus lebih besar dari nol.");

        // Idempotent: membership yang sama selalu mendapat tagihan yang sama, nominal tidak dihitung ulang.
        var existing = await _dbContext.BilMembershipFeeCharges.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MembershipId == request.MembershipId && !x.IsDelete, cancellationToken);
        if (existing is not null)
            return new MembershipFeeChargeRef(existing.Id, existing.ChargeNumber, false);

        var now = _timeProvider.GetUtcNow();
        var chargeNumber = await _numberSeries.AllocateMembershipFeeChargeNumberAsync(
            request.ActorUserId, now, cancellationToken);

        var charge = new BilMembershipFeeCharge
        {
            Id = Guid.NewGuid(),
            ChargeNumber = chargeNumber,
            MembershipId = request.MembershipId,
            PatientId = request.PatientId,
            FeeAmount = request.FeeAmount,
            PaidAmount = 0m,
            Status = BillingMembershipFeeChargeStatuses.Unpaid,
            PaymentDueAt = request.PaymentDueAt,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = now.UtcDateTime,
            CreateBy = request.ActorUserId
        };
        _dbContext.BilMembershipFeeCharges.Add(charge);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(
            LogCategory,
            "MembershipFeeCharge.Created",
            $"Tagihan iuran {chargeNumber} dibuat untuk membership {request.MemberNumber}.",
            new
            {
                ChargeId = charge.Id,
                charge.ChargeNumber,
                charge.MembershipId,
                charge.FeeAmount,
                charge.PaymentDueAt,
                ActorUserId = request.ActorUserId
            });

        return new MembershipFeeChargeRef(charge.Id, chargeNumber, true);
    }

    /// <summary>
    /// Membuat atau mengambil tagihan untuk membership yang sudah terdaftar Menunggu Pembayaran tetapi belum
    /// bertagihan (mis. didaftarkan sebelum fitur ini). Idempotent; nominal dari snapshot membership.
    /// </summary>
    public async Task<MembershipFeeChargeDetailResponse> EnsureForMembershipAsync(
        Guid membershipId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (membershipId == Guid.Empty)
            throw new BillingMembershipFeeValidationException("MembershipId wajib diisi.");

        var context = await _membershipLink.GetFeeContextAsync(membershipId, cancellationToken)
            ?? throw new KeyNotFoundException("Membership tidak ditemukan.");
        if (context.FeeChargeReferenceId.HasValue)
            return await GetDetailAsync(context.FeeChargeReferenceId.Value, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await AcquireLockAsync($"BIL_MEMBERSHIP_FEE_MEMBERSHIP_{membershipId:N}", cancellationToken);

        context = await _membershipLink.GetFeeContextAsync(membershipId, cancellationToken)
            ?? throw new KeyNotFoundException("Membership tidak ditemukan.");
        if (context.FeeChargeReferenceId.HasValue)
        {
            await transaction.CommitAsync(cancellationToken);
            return await GetDetailAsync(context.FeeChargeReferenceId.Value, cancellationToken);
        }

        if (context.Status != MembershipStatus.PendingPayment)
            throw new BillingMembershipFeeValidationException(
                "Tagihan iuran hanya dibuat untuk membership yang menunggu pembayaran.");
        if (!context.PaymentDueAt.HasValue || context.PaymentDueAt.Value <= _timeProvider.GetUtcNow())
            throw new BillingMembershipFeeValidationException(
                "Batas pembayaran membership sudah lewat; tagihan tidak dapat dibuat. Daftarkan ulang membership.");

        var chargeRef = await EnsureChargeAsync(
            new MembershipFeeChargeRequest(
                context.MembershipId, context.PatientId, context.MemberNumber, context.FeeAmountSnapshot,
                context.PaymentDueAt.Value, actorUserId),
            cancellationToken);

        await _membershipLink.AttachChargeAsync(context.MembershipId, chargeRef.ChargeId, actorUserId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetDetailAsync(chargeRef.ChargeId, cancellationToken);
    }

    // ------------------------------------------------------------------------------------------------
    // Pembacaan
    // ------------------------------------------------------------------------------------------------

    public async Task<PagedResult<MembershipFeeChargeResponse>> GetPagedAsync(
        MembershipFeeChargeQuery query,
        CancellationToken cancellationToken)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, 100);
        var now = _timeProvider.GetUtcNow();

        var rows = QueryRows();

        if (!string.IsNullOrWhiteSpace(query.Status)
            && BillingMembershipFeeChargeStatuses.All.Contains(query.Status.Trim()))
        {
            var status = query.Status.Trim().ToUpperInvariant();
            rows = rows.Where(r => r.Charge.Status == status);
        }

        if (query.PayableOnly)
        {
            rows = rows.Where(r =>
                (r.Charge.Status == BillingMembershipFeeChargeStatuses.Unpaid
                    || r.Charge.Status == BillingMembershipFeeChargeStatuses.PartiallyPaid)
                && r.Charge.PaymentDueAt > now
                && r.MembershipStatus == MembershipStatus.PendingPayment);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();
            rows = rows.Where(r =>
                r.Charge.ChargeNumber.ToLower().Contains(keyword)
                || r.MemberNumber.ToLower().Contains(keyword)
                || r.MedicalRecordNumber.ToLower().Contains(keyword)
                || r.PatientName.ToLower().Contains(keyword));
        }

        var totalData = await rows.CountAsync(cancellationToken);
        var page = await rows
            .OrderByDescending(r => r.Charge.CreateDateTime)
            .ThenByDescending(r => r.Charge.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<MembershipFeeChargeResponse>
        {
            Items = page.Select(r => Map(r, now)).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = totalData == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize)
        };
    }

    public async Task<MembershipFeeChargeDetailResponse> GetDetailAsync(Guid chargeId, CancellationToken cancellationToken)
    {
        if (chargeId == Guid.Empty)
            throw new BillingMembershipFeeValidationException("ChargeId wajib diisi.");

        var row = await QueryRows().FirstOrDefaultAsync(r => r.Charge.Id == chargeId, cancellationToken)
            ?? throw new KeyNotFoundException("Tagihan iuran tidak ditemukan.");
        return await BuildDetailAsync(row, cancellationToken);
    }

    public async Task<MembershipFeeChargeDetailResponse> GetByMembershipAsync(Guid membershipId, CancellationToken cancellationToken)
    {
        if (membershipId == Guid.Empty)
            throw new BillingMembershipFeeValidationException("MembershipId wajib diisi.");

        var row = await QueryRows().FirstOrDefaultAsync(r => r.Charge.MembershipId == membershipId, cancellationToken)
            ?? throw new KeyNotFoundException("Membership ini belum memiliki tagihan iuran.");
        return await BuildDetailAsync(row, cancellationToken);
    }

    public async Task<IReadOnlyList<MembershipFeePaymentItem>> GetPaymentsAsync(Guid chargeId, CancellationToken cancellationToken)
    {
        if (chargeId == Guid.Empty)
            throw new BillingMembershipFeeValidationException("ChargeId wajib diisi.");

        if (!await _dbContext.BilMembershipFeeCharges.AsNoTracking().AnyAsync(x => x.Id == chargeId && !x.IsDelete, cancellationToken))
            throw new KeyNotFoundException("Tagihan iuran tidak ditemukan.");

        var rows = await (
            from t in _dbContext.BilTenders.AsNoTracking()
            join s in _dbContext.BilSettlements.AsNoTracking() on t.SettlementId equals s.Id
            join pm in _dbContext.MstPaymentMethods.AsNoTracking() on t.PaymentMethodId equals pm.Id
            where s.MembershipFeeChargeId == chargeId && !s.IsDelete && !t.IsDelete
            orderby t.AttemptedAt descending, t.Id descending
            select new
            {
                t.Id,
                t.SettlementId,
                t.KwitansiNumber,
                t.PaymentMethodId,
                pm.PaymentMethodName,
                t.Amount,
                t.Status,
                t.CashierReferenceNote,
                t.ProviderReference,
                t.AttemptedAt,
                t.SettledAt
            }).ToListAsync(cancellationToken);

        return rows.Select(x => new MembershipFeePaymentItem
        {
            TenderId = x.Id,
            SettlementId = x.SettlementId,
            KwitansiNumber = x.KwitansiNumber,
            PaymentMethodId = x.PaymentMethodId,
            PaymentMethodName = x.PaymentMethodName,
            Amount = x.Amount,
            Status = x.Status,
            CashierReferenceNote = x.CashierReferenceNote,
            ProviderReferenceMasked = MaskReference(x.ProviderReference),
            AttemptedAt = x.AttemptedAt,
            SettledAt = x.SettledAt
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, MembershipFeeChargeSummary>> GetSummariesAsync(
        IReadOnlyCollection<Guid> chargeIds,
        CancellationToken cancellationToken)
    {
        var ids = chargeIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, MembershipFeeChargeSummary>();

        var now = _timeProvider.GetUtcNow();
        var rows = await QueryRows().Where(r => ids.Contains(r.Charge.Id)).ToListAsync(cancellationToken);
        var lastPayments = await LoadLastPaymentsAsync(ids, cancellationToken);

        return rows.ToDictionary(
            r => r.Charge.Id,
            r =>
            {
                var mapped = Map(r, now);
                lastPayments.TryGetValue(r.Charge.Id, out var last);
                return new MembershipFeeChargeSummary(
                    mapped.Id,
                    mapped.ChargeNumber,
                    mapped.FeeAmount,
                    mapped.PaidAmount,
                    mapped.OutstandingAmount,
                    mapped.Status,
                    mapped.StatusName,
                    mapped.PaymentDueAt,
                    mapped.SettledAt,
                    mapped.MembershipConfirmedAt,
                    last?.PaymentMethodName,
                    last?.Reference,
                    last?.KwitansiNumber,
                    mapped.IsOverdue,
                    mapped.NeedsSync,
                    mapped.RequiresReview,
                    mapped.ReviewReason);
            });
    }

    public async Task<MembershipFeeReconciliationResponse> GetReconciliationAsync(CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();

        // (1) Tagihan lunas yang belum diterapkan ke membership, dan tagihan yang perlu ditinjau manual.
        var chargeRows = await QueryRows()
            .Where(r => (r.Charge.Status == BillingMembershipFeeChargeStatuses.Paid && r.Charge.MembershipConfirmedAt == null)
                || (r.Charge.Status == BillingMembershipFeeChargeStatuses.Cancelled && r.Charge.PaidAmount > 0)
                || (r.Charge.MembershipConfirmedAt != null && r.Charge.Status != BillingMembershipFeeChargeStatuses.Paid)
                || (r.Charge.Status == BillingMembershipFeeChargeStatuses.Paid && r.MembershipStatus == MembershipStatus.Cancelled))
            .OrderByDescending(r => r.Charge.CreateDateTime)
            .Take(ReconciliationLimit)
            .ToListAsync(cancellationToken);

        var paidButNotActivated = new List<MembershipFeeReconciliationItem>();
        var needsReview = new List<MembershipFeeReconciliationItem>();
        foreach (var row in chargeRows)
        {
            var review = EvaluateReview(row.Charge, row.MembershipStatus);
            if (review is not null)
                needsReview.Add(ToReconciliationItem(row, review));
            else if (row.Charge.Status == BillingMembershipFeeChargeStatuses.Paid && row.Charge.MembershipConfirmedAt is null)
                paidButNotActivated.Add(ToReconciliationItem(
                    row, "Tagihan lunas tetapi belum diterapkan ke membership; sinkronisasi akan diulang."));
        }

        // (2) Membership Aktif/Terjadwal tanpa tagihan lunas, kecuali data lama yang diakui lewat verifikasi.
        var withoutPaid = await (
            from m in _dbContext.Set<MstPatientMembership>().AsNoTracking()
            join c in _dbContext.BilMembershipFeeCharges.AsNoTracking().Where(x => !x.IsDelete)
                on m.Id equals c.MembershipId into chargeGroup
            from c in chargeGroup.DefaultIfEmpty()
            where !m.IsDelete
                && (m.MembershipStatus == MembershipStatus.Active || m.MembershipStatus == MembershipStatus.Scheduled)
                && m.RegistrationType != MembershipRegistrationType.LegacyImport
                && (c == null || c.Status != BillingMembershipFeeChargeStatuses.Paid)
            orderby m.CreateDateTime descending
            select new
            {
                m.Id,
                m.MemberNumber,
                m.MembershipStatus,
                PatientName = m.Patient!.FullName,
                m.Patient.MedicalRecordNumber,
                ChargeId = c == null ? (Guid?)null : c.Id,
                ChargeNumber = c == null ? null : c.ChargeNumber,
                ChargeStatus = c == null ? null : c.Status,
                FeeAmount = c == null ? m.FeeAmountSnapshot : c.FeeAmount,
                PaidAmount = c == null ? 0m : c.PaidAmount
            }).Take(ReconciliationLimit).ToListAsync(cancellationToken);

        return new MembershipFeeReconciliationResponse
        {
            PaidButNotActivated = paidButNotActivated,
            NeedsReview = needsReview,
            ActivatedWithoutPaidCharge = withoutPaid.Select(x => new MembershipFeeReconciliationItem
            {
                MembershipId = x.Id,
                MemberNumber = x.MemberNumber,
                PatientName = x.PatientName,
                MedicalRecordNumber = x.MedicalRecordNumber,
                MembershipStatusName = MembershipStatusName(x.MembershipStatus),
                ChargeId = x.ChargeId,
                ChargeNumber = x.ChargeNumber,
                ChargeStatusName = x.ChargeStatus is null ? null : ChargeStatusName(x.ChargeStatus),
                FeeAmount = x.FeeAmount,
                PaidAmount = x.PaidAmount,
                Reason = "Membership aktif/terjadwal tanpa tagihan iuran lunas."
            }).ToList()
        };
    }

    // ------------------------------------------------------------------------------------------------
    // Pembayaran lewat settlement Kasir
    // ------------------------------------------------------------------------------------------------

    /// <summary>
    /// Memvalidasi tagihan sebagai target settlement baru: belum lunas/dibatalkan, belum lewat batas bayar,
    /// membership masih Menunggu Pembayaran, dan nominal sama dengan sisa tagihan (pembayaran penuh).
    /// </summary>
    internal async Task ValidateSettlementTargetAsync(
        Guid chargeId,
        decimal requestedAmount,
        CancellationToken cancellationToken)
    {
        var charge = await ValidatePayableAsync(chargeId, cancellationToken);
        var outstanding = charge.FeeAmount - charge.PaidAmount;

        if (!AllowPartialPayment && requestedAmount != outstanding)
            throw new BillingMembershipFeeValidationException(
                $"Iuran membership harus dibayar penuh sebesar Rp {outstanding:N0}; pembayaran sebagian tidak diperbolehkan.");
        if (requestedAmount > outstanding)
            throw new BillingMembershipFeeValidationException("Nominal melebihi sisa tagihan iuran.");
    }

    /// <summary>
    /// Dipanggil sebelum tender baru dibuat. Mengambil kunci tagihan yang sama dengan pembatalan, sehingga
    /// tagihan yang dibatalkan tidak menerima tender dan pembatalan menolak tagihan yang sedang diproses.
    /// </summary>
    internal async Task EnsureSettlementPayableAsync(BilSettlement settlement, CancellationToken cancellationToken)
    {
        if (settlement.Purpose != BillingSettlementPurposes.MembershipFee || !settlement.MembershipFeeChargeId.HasValue)
            return;

        await ValidatePayableAsync(settlement.MembershipFeeChargeId.Value, cancellationToken);
    }

    /// <summary>
    /// Menerapkan dana settlement yang berhasil ke tagihan (alokasi bersih, dengan pembalikan bila tender dibalik)
    /// dan memperbarui status tagihan. Berjalan di transaksi rekonsiliasi tender. Tidak mengaktifkan membership;
    /// aktivasi dilakukan setelah commit lewat <see cref="ConfirmChargeAsync"/>.
    /// </summary>
    internal async Task ReconcileSettlementAsync(
        BilSettlement settlement,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        if (settlement.Purpose != BillingSettlementPurposes.MembershipFee || !settlement.MembershipFeeChargeId.HasValue)
            return;

        var chargeId = settlement.MembershipFeeChargeId.Value;
        await AcquireLockAsync(ChargeLockKey(chargeId), cancellationToken);

        var charge = await _dbContext.BilMembershipFeeCharges
            .FirstOrDefaultAsync(x => x.Id == chargeId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Tagihan iuran tidak ditemukan.");

        var allocations = await _dbContext.BilMembershipFeeAllocations
            .Where(x => x.MembershipFeeChargeId == chargeId && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var chargeNet = Net(allocations);
        var settlementAllocations = allocations.Where(x => x.SettlementId == settlement.Id).ToList();
        var settlementNet = Net(settlementAllocations);

        if (settlement.SuccessfulAmount > settlementNet)
        {
            var newFunds = settlement.SuccessfulAmount - settlementNet;
            var outstanding = Math.Max(charge.FeeAmount - chargeNet, 0m);
            var amount = Math.Min(newFunds, outstanding);
            if (amount > 0)
            {
                var allocation = NewAllocation(chargeId, settlement.Id, amount, occurredAt, actorUserId, null);
                charge.Allocations.Add(allocation);
                _dbContext.BilMembershipFeeAllocations.Add(allocation);
                chargeNet += amount;
                settlementNet += amount;
            }

            if (newFunds > amount)
                await _loggerService.WarningAsync(
                    LogCategory,
                    "MembershipFeeCharge.UnallocatedFunds",
                    $"Dana settlement {settlement.Id} melebihi sisa tagihan {charge.ChargeNumber}; kelebihan tidak dialokasikan.",
                    new { ChargeId = chargeId, SettlementId = settlement.Id, Excess = newFunds - amount });
        }
        else if (settlement.SuccessfulAmount < settlementNet)
        {
            // Tender dibalik: alokasi dibalik dengan baris baru (append-only), terbaru lebih dulu.
            var reduction = settlementNet - settlement.SuccessfulAmount;
            var reversedIds = settlementAllocations
                .Where(x => x.ReversesAllocationId.HasValue)
                .Select(x => x.ReversesAllocationId!.Value)
                .ToHashSet();

            foreach (var original in settlementAllocations
                .Where(x => !x.ReversesAllocationId.HasValue && !reversedIds.Contains(x.Id))
                .OrderByDescending(x => x.AllocatedAt)
                .ThenByDescending(x => x.Id))
            {
                if (reduction <= 0) break;
                var amount = Math.Min(original.Amount, reduction);
                var reversal = NewAllocation(chargeId, settlement.Id, amount, occurredAt, actorUserId, original.Id);
                charge.Allocations.Add(reversal);
                _dbContext.BilMembershipFeeAllocations.Add(reversal);
                chargeNet -= amount;
                settlementNet -= amount;
                reduction -= amount;
            }
        }

        settlement.AllocatedAmount = Math.Max(settlementNet, 0m);

        var previousStatus = charge.Status;
        charge.PaidAmount = Math.Clamp(chargeNet, 0m, charge.FeeAmount);
        if (charge.Status != BillingMembershipFeeChargeStatuses.Cancelled)
        {
            charge.Status = charge.PaidAmount >= charge.FeeAmount
                ? BillingMembershipFeeChargeStatuses.Paid
                : charge.PaidAmount > 0
                    ? BillingMembershipFeeChargeStatuses.PartiallyPaid
                    : BillingMembershipFeeChargeStatuses.Unpaid;
        }

        if (charge.Status == BillingMembershipFeeChargeStatuses.Paid)
        {
            if (previousStatus != BillingMembershipFeeChargeStatuses.Paid)
                charge.SettledAt = occurredAt;
        }
        else
        {
            charge.SettledAt = null;
        }

        charge.RowVersion = Guid.NewGuid();
        charge.UpdateDateTime = DateTime.UtcNow;
        charge.UpdateBy = actorUserId;

        await _loggerService.AuditAsync(
            LogCategory,
            "MembershipFeeCharge.Reconciled",
            $"Dana settlement dialokasikan ke tagihan iuran {charge.ChargeNumber}.",
            new
            {
                ChargeId = chargeId,
                SettlementId = settlement.Id,
                charge.PaidAmount,
                StatusBefore = previousStatus,
                StatusAfter = charge.Status,
                ActorUserId = actorUserId
            });
    }

    /// <summary>
    /// Setelah pembayaran tersimpan (di luar transaksi tender): bila tagihan lunas, kabari CRM untuk
    /// mengaktifkan membership. Best-effort dan idempotent; kegagalan tidak membatalkan uang yang sudah
    /// diterima, dan diulang oleh <see cref="RetryUnconfirmedAsync"/> serta proses harian.
    /// </summary>
    public async Task ConfirmPaidChargeForSettlementAsync(Guid settlementId, Guid actorUserId, CancellationToken cancellationToken)
    {
        try
        {
            var chargeId = await _dbContext.BilSettlements.AsNoTracking()
                .Where(x => x.Id == settlementId && !x.IsDelete && x.Purpose == BillingSettlementPurposes.MembershipFee)
                .Select(x => x.MembershipFeeChargeId)
                .FirstOrDefaultAsync(cancellationToken);
            if (!chargeId.HasValue) return;

            await ConfirmChargeAsync(chargeId.Value, actorUserId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await _loggerService.ErrorAsync(
                LogCategory,
                "MembershipFeeCharge.ConfirmFailed",
                "Konfirmasi pelunasan iuran ke CRM gagal; akan diulang oleh rekonsiliasi.",
                ex,
                new { SettlementId = settlementId });
        }
    }

    /// <summary>
    /// `ConfirmFeePaidAsync` sisi Billing: memanggil CRM untuk tagihan yang sudah lunas dan menandai hasilnya.
    /// Idempotent; tagihan yang belum lunas atau sudah terkonfirmasi diabaikan.
    /// </summary>
    public async Task<MembershipFeeConfirmationResult?> ConfirmChargeAsync(
        Guid chargeId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var charge = await _dbContext.BilMembershipFeeCharges.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == chargeId && !x.IsDelete, cancellationToken);
        if (charge is null
            || charge.Status != BillingMembershipFeeChargeStatuses.Paid
            || charge.MembershipConfirmedAt.HasValue
            || !charge.SettledAt.HasValue)
            return null;

        var result = await _membershipLink.ConfirmFeePaidAsync(
            charge.MembershipId, charge.Id, charge.SettledAt.Value, actorUserId, cancellationToken);

        if (!result.IsSuccess)
        {
            await _loggerService.WarningAsync(
                LogCategory,
                "MembershipFeeCharge.ConfirmRejected",
                $"CRM tidak menerapkan pelunasan tagihan {charge.ChargeNumber}: {result.Outcome}. {result.Message}",
                new { ChargeId = chargeId, charge.MembershipId, result.Outcome });
            return result;
        }

        // Dimuat ulang: layanan CRM dapat membersihkan change tracker pada jalur penolakan.
        var tracked = await _dbContext.BilMembershipFeeCharges
            .FirstOrDefaultAsync(x => x.Id == chargeId && !x.IsDelete, cancellationToken);
        if (tracked is not null && !tracked.MembershipConfirmedAt.HasValue)
        {
            tracked.MembershipConfirmedAt = _timeProvider.GetUtcNow();
            tracked.RowVersion = Guid.NewGuid();
            tracked.UpdateDateTime = DateTime.UtcNow;
            tracked.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await _loggerService.AuditAsync(
            LogCategory,
            "MembershipFeeCharge.MembershipConfirmed",
            $"Pelunasan tagihan {charge.ChargeNumber} diterapkan ke membership ({result.Outcome}).",
            new { ChargeId = chargeId, charge.MembershipId, result.Outcome, ActorUserId = actorUserId });

        return result;
    }

    /// <inheritdoc />
    public async Task<int> RetryUnconfirmedAsync(CancellationToken cancellationToken)
    {
        // Hanya tagihan yang masih mungkin berhasil: lunas sebelum batas bayar dan membership masih Menunggu
        // Pembayaran. Sisanya (dibayar terlambat, membership sudah dibatalkan) ditinjau manual lewat rekonsiliasi.
        var candidateIds = await (
            from c in _dbContext.BilMembershipFeeCharges.AsNoTracking()
            join m in _dbContext.Set<MstPatientMembership>().AsNoTracking() on c.MembershipId equals m.Id
            where !c.IsDelete
                && c.Status == BillingMembershipFeeChargeStatuses.Paid
                && c.MembershipConfirmedAt == null
                && c.SettledAt != null
                && c.SettledAt <= c.PaymentDueAt
                && m.MembershipStatus == MembershipStatus.PendingPayment
                && !m.IsDelete
            orderby c.SettledAt
            select c.Id).Take(RetryBatchSize).ToListAsync(cancellationToken);

        var synced = 0;
        foreach (var chargeId in candidateIds)
        {
            var result = await ConfirmChargeAsync(chargeId, Guid.Empty, cancellationToken);
            if (result?.IsSuccess == true) synced++;
        }

        return synced;
    }

    // ------------------------------------------------------------------------------------------------
    // Pembatalan (CRM-DEC-014, CRM-DEC-029)
    // ------------------------------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<MembershipFeeCancelOutcome> CancelChargeAsync(
        Guid membershipId,
        string reason,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var chargeId = await _dbContext.BilMembershipFeeCharges.AsNoTracking()
            .Where(x => x.MembershipId == membershipId && !x.IsDelete)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (!chargeId.HasValue)
            return MembershipFeeCancelOutcome.NoCharge;

        await AcquireLockAsync(ChargeLockKey(chargeId.Value), cancellationToken);

        var charge = await _dbContext.BilMembershipFeeCharges
            .FirstOrDefaultAsync(x => x.Id == chargeId.Value && !x.IsDelete, cancellationToken);
        if (charge is null)
            return MembershipFeeCancelOutcome.NoCharge;
        if (charge.Status == BillingMembershipFeeChargeStatuses.Cancelled)
            return MembershipFeeCancelOutcome.Cancelled;

        // Uang yang sudah diterima menang atas pembatalan: tidak ada refund otomatis dan tidak ada
        // pengubahan PaidAmount menjadi nol (kebijakan penyetuju refund belum ditetapkan).
        if (charge.PaidAmount > 0 || charge.Status == BillingMembershipFeeChargeStatuses.Paid)
            return MembershipFeeCancelOutcome.PaymentReceived;

        var settlementIds = _dbContext.BilSettlements.AsNoTracking()
            .Where(s => s.MembershipFeeChargeId == charge.Id && !s.IsDelete)
            .Select(s => s.Id);
        var hasInFlightTender = await _dbContext.BilTenders.AsNoTracking()
            .AnyAsync(t => !t.IsDelete
                && settlementIds.Contains(t.SettlementId)
                && (t.Status == BillingTenderStatuses.Created || t.Status == BillingTenderStatuses.Pending),
                cancellationToken);
        if (hasInFlightTender)
            return MembershipFeeCancelOutcome.PaymentInProgress;

        var now = _timeProvider.GetUtcNow();
        charge.Status = BillingMembershipFeeChargeStatuses.Cancelled;
        charge.CancelledAt = now;
        charge.CancellationReason = reason is { Length: > 500 } ? reason[..500] : reason;
        charge.RowVersion = Guid.NewGuid();
        charge.UpdateDateTime = now.UtcDateTime;
        charge.UpdateBy = actorUserId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(
            LogCategory,
            "MembershipFeeCharge.Cancelled",
            $"Tagihan iuran {charge.ChargeNumber} dibatalkan mengikuti siklus hidup membership.",
            new { ChargeId = charge.Id, charge.MembershipId, charge.CancellationReason, ActorUserId = actorUserId });

        return MembershipFeeCancelOutcome.Cancelled;
    }

    /// <summary>
    /// Membatalkan tagihan dari Billing hanya bila membership-nya sudah dibatalkan di CRM. Billing tidak
    /// membatalkan tagihan sendiri karena itu membuat tagihan dan membership tidak sinkron.
    /// </summary>
    public async Task<MembershipFeeChargeDetailResponse> CancelForCancelledMembershipAsync(
        Guid chargeId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var row = await QueryRows().FirstOrDefaultAsync(r => r.Charge.Id == chargeId, cancellationToken)
            ?? throw new KeyNotFoundException("Tagihan iuran tidak ditemukan.");

        if (row.MembershipStatus != MembershipStatus.Cancelled)
            throw new BillingMembershipFeeConflictException(
                "Tagihan hanya dapat dibatalkan setelah membership-nya dibatalkan di CRM. Batalkan dari halaman membership.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var outcome = await CancelChargeAsync(row.Charge.MembershipId, "Membership dibatalkan di CRM.", actorUserId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return outcome switch
        {
            MembershipFeeCancelOutcome.PaymentReceived => throw new BillingMembershipFeeConflictException(
                "Uang untuk tagihan ini sudah diterima; pembatalan memerlukan proses refund kasir yang disetujui."),
            MembershipFeeCancelOutcome.PaymentInProgress => throw new BillingMembershipFeeConflictException(
                "Ada pembayaran yang sedang diproses; tunggu hingga selesai."),
            _ => await GetDetailAsync(chargeId, cancellationToken)
        };
    }

    // ------------------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------------------

    private async Task<BilMembershipFeeCharge> ValidatePayableAsync(Guid chargeId, CancellationToken cancellationToken)
    {
        await AcquireLockAsync(ChargeLockKey(chargeId), cancellationToken);

        var charge = await _dbContext.BilMembershipFeeCharges.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == chargeId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Tagihan iuran tidak ditemukan.");

        if (charge.Status == BillingMembershipFeeChargeStatuses.Cancelled)
            throw new BillingMembershipFeeValidationException("Tagihan iuran sudah dibatalkan dan tidak dapat dibayar.");
        if (charge.Status == BillingMembershipFeeChargeStatuses.Paid)
            throw new BillingMembershipFeeValidationException("Tagihan iuran sudah lunas.");

        if (_timeProvider.GetUtcNow() >= charge.PaymentDueAt)
            throw new BillingMembershipFeeValidationException(
                "Batas pembayaran iuran sudah lewat; pembayaran baru tidak diterima. Pasien perlu mendaftar ulang.");

        var membership = await _membershipLink.GetFeeContextAsync(charge.MembershipId, cancellationToken)
            ?? throw new KeyNotFoundException("Membership untuk tagihan ini tidak ditemukan.");
        if (membership.Status != MembershipStatus.PendingPayment)
            throw new BillingMembershipFeeValidationException(
                "Membership tidak lagi menunggu pembayaran; tagihan tidak dapat dibayar.");
        if (membership.FeeChargeReferenceId != charge.Id)
            throw new BillingMembershipFeeConflictException("Tagihan tidak sesuai dengan membership.");

        return charge;
    }

    private IQueryable<ChargeRow> QueryRows() =>
        from c in _dbContext.BilMembershipFeeCharges.AsNoTracking()
        join m in _dbContext.Set<MstPatientMembership>().AsNoTracking() on c.MembershipId equals m.Id
        where !c.IsDelete
        select new ChargeRow
        {
            Charge = c,
            MemberNumber = m.MemberNumber,
            MembershipStatus = m.MembershipStatus,
            RegistrationType = m.RegistrationType,
            TierName = m.MembershipTier!.TierName,
            PatientName = m.Patient!.FullName,
            MedicalRecordNumber = m.Patient.MedicalRecordNumber
        };

    private async Task<MembershipFeeChargeDetailResponse> BuildDetailAsync(ChargeRow row, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var mapped = Map(row, now);

        var openSettlement = await _dbContext.BilSettlements.AsNoTracking()
            .Include(x => x.Tenders)
            .Where(x => x.MembershipFeeChargeId == row.Charge.Id
                && !x.IsDelete
                && x.Status != BillingSettlementStatuses.Settled)
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        var lastPayments = await LoadLastPaymentsAsync([row.Charge.Id], cancellationToken);
        lastPayments.TryGetValue(row.Charge.Id, out var last);

        var pending = openSettlement?.Tenders
            .Where(t => !t.IsDelete && (t.Status == BillingTenderStatuses.Created || t.Status == BillingTenderStatuses.Pending))
            .Sum(t => t.Amount) ?? 0m;

        return new MembershipFeeChargeDetailResponse
        {
            Id = mapped.Id,
            ChargeNumber = mapped.ChargeNumber,
            MembershipId = mapped.MembershipId,
            MemberNumber = mapped.MemberNumber,
            MembershipStatus = mapped.MembershipStatus,
            MembershipStatusName = mapped.MembershipStatusName,
            RegistrationType = mapped.RegistrationType,
            TierName = mapped.TierName,
            PatientId = mapped.PatientId,
            MedicalRecordNumber = mapped.MedicalRecordNumber,
            PatientName = mapped.PatientName,
            FeeAmount = mapped.FeeAmount,
            PaidAmount = mapped.PaidAmount,
            OutstandingAmount = mapped.OutstandingAmount,
            Status = mapped.Status,
            StatusName = mapped.StatusName,
            PaymentDueAt = mapped.PaymentDueAt,
            IsOverdue = mapped.IsOverdue,
            SettledAt = mapped.SettledAt,
            MembershipConfirmedAt = mapped.MembershipConfirmedAt,
            CancelledAt = mapped.CancelledAt,
            CanPay = mapped.CanPay,
            NeedsSync = mapped.NeedsSync,
            RequiresReview = mapped.RequiresReview,
            ReviewReason = mapped.ReviewReason,
            CreatedAt = mapped.CreatedAt,
            RowVersion = mapped.RowVersion,
            CancellationReason = row.Charge.CancellationReason,
            OpenSettlementId = openSettlement?.Id,
            OpenSettlementRowVersion = openSettlement?.RowVersion,
            OpenSettlementStatus = openSettlement?.Status,
            OpenSettlementCollectibleAmount = openSettlement is null
                ? 0m
                : Math.Max(openSettlement.RequestedAmount - openSettlement.SuccessfulAmount - pending, 0m),
            LastPaymentMethodName = last?.PaymentMethodName,
            LastPaymentReference = last?.Reference,
            LastKwitansiNumber = last?.KwitansiNumber
        };
    }

    private async Task<Dictionary<Guid, LastPayment>> LoadLastPaymentsAsync(
        List<Guid> chargeIds,
        CancellationToken cancellationToken)
    {
        if (chargeIds.Count == 0) return new Dictionary<Guid, LastPayment>();

        var rows = await (
            from t in _dbContext.BilTenders.AsNoTracking()
            join s in _dbContext.BilSettlements.AsNoTracking() on t.SettlementId equals s.Id
            join pm in _dbContext.MstPaymentMethods.AsNoTracking() on t.PaymentMethodId equals pm.Id
            where s.MembershipFeeChargeId != null
                && chargeIds.Contains(s.MembershipFeeChargeId.Value)
                && !s.IsDelete && !t.IsDelete
                && t.Status == BillingTenderStatuses.Succeeded
            select new
            {
                ChargeId = s.MembershipFeeChargeId!.Value,
                pm.PaymentMethodName,
                t.CashierReferenceNote,
                t.ProviderReference,
                t.KwitansiNumber,
                t.SettledAt,
                t.AttemptedAt
            }).ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.ChargeId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var latest = g.OrderByDescending(x => x.SettledAt ?? x.AttemptedAt).First();
                    return new LastPayment(
                        latest.PaymentMethodName,
                        !string.IsNullOrWhiteSpace(latest.CashierReferenceNote)
                            ? latest.CashierReferenceNote
                            : MaskReference(latest.ProviderReference),
                        latest.KwitansiNumber);
                });
    }

    private static MembershipFeeChargeResponse Map(ChargeRow row, DateTimeOffset now)
    {
        var charge = row.Charge;
        var isOpenStatus = charge.Status is BillingMembershipFeeChargeStatuses.Unpaid
            or BillingMembershipFeeChargeStatuses.PartiallyPaid;
        var review = EvaluateReview(charge, row.MembershipStatus);

        return new MembershipFeeChargeResponse
        {
            Id = charge.Id,
            ChargeNumber = charge.ChargeNumber,
            MembershipId = charge.MembershipId,
            MemberNumber = row.MemberNumber,
            MembershipStatus = row.MembershipStatus.ToString(),
            MembershipStatusName = MembershipStatusName(row.MembershipStatus),
            RegistrationType = row.RegistrationType.ToString(),
            TierName = row.TierName,
            PatientId = charge.PatientId,
            MedicalRecordNumber = row.MedicalRecordNumber,
            PatientName = row.PatientName,
            FeeAmount = charge.FeeAmount,
            PaidAmount = charge.PaidAmount,
            OutstandingAmount = charge.Status == BillingMembershipFeeChargeStatuses.Cancelled
                ? 0m
                : Math.Max(charge.FeeAmount - charge.PaidAmount, 0m),
            Status = charge.Status,
            StatusName = ChargeStatusName(charge.Status),
            PaymentDueAt = charge.PaymentDueAt,
            IsOverdue = isOpenStatus && now >= charge.PaymentDueAt,
            SettledAt = charge.SettledAt,
            MembershipConfirmedAt = charge.MembershipConfirmedAt,
            CancelledAt = charge.CancelledAt,
            CanPay = isOpenStatus && now < charge.PaymentDueAt && row.MembershipStatus == MembershipStatus.PendingPayment,
            NeedsSync = charge.Status == BillingMembershipFeeChargeStatuses.Paid && !charge.MembershipConfirmedAt.HasValue,
            RequiresReview = review is not null,
            ReviewReason = review,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(charge.CreateDateTime, DateTimeKind.Utc)),
            RowVersion = charge.RowVersion
        };
    }

    /// <summary>
    /// Keadaan yang tidak boleh diputuskan otomatis tanpa aturan bisnis yang disetujui; ditampilkan sebagai
    /// antrean peninjauan, bukan diperbaiki diam-diam.
    /// </summary>
    private static string? EvaluateReview(BilMembershipFeeCharge charge, MembershipStatus membershipStatus)
    {
        if (charge.Status == BillingMembershipFeeChargeStatuses.Paid && !charge.MembershipConfirmedAt.HasValue)
        {
            if (membershipStatus != MembershipStatus.PendingPayment)
                return "Uang sudah diterima tetapi membership sudah tidak menunggu pembayaran (mis. dibatalkan); perlu ditinjau.";
            if (charge.SettledAt.HasValue && charge.SettledAt.Value > charge.PaymentDueAt)
                return "Dibayar setelah batas bayar; membership tidak diaktifkan dan pengembalian dana perlu ditinjau.";
            return null;
        }

        if (charge.Status == BillingMembershipFeeChargeStatuses.Cancelled && charge.PaidAmount > 0)
            return "Tagihan dibatalkan tetapi uang sudah diterima; perlu proses refund yang disetujui.";

        if (charge.MembershipConfirmedAt.HasValue && charge.Status != BillingMembershipFeeChargeStatuses.Paid)
            return "Pembayaran dibalik setelah membership diaktifkan; status manfaat perlu ditinjau.";

        if (charge.Status == BillingMembershipFeeChargeStatuses.Paid && membershipStatus == MembershipStatus.Cancelled)
            return "Membership dibatalkan setelah iuran lunas; refund mengikuti kebijakan yang belum ditetapkan.";

        return null;
    }

    private static MembershipFeeReconciliationItem ToReconciliationItem(ChargeRow row, string reason) => new()
    {
        MembershipId = row.Charge.MembershipId,
        MemberNumber = row.MemberNumber,
        PatientName = row.PatientName,
        MedicalRecordNumber = row.MedicalRecordNumber,
        MembershipStatusName = MembershipStatusName(row.MembershipStatus),
        ChargeId = row.Charge.Id,
        ChargeNumber = row.Charge.ChargeNumber,
        ChargeStatusName = ChargeStatusName(row.Charge.Status),
        FeeAmount = row.Charge.FeeAmount,
        PaidAmount = row.Charge.PaidAmount,
        Reason = reason
    };

    private static BilMembershipFeeAllocation NewAllocation(
        Guid chargeId,
        Guid settlementId,
        decimal amount,
        DateTimeOffset allocatedAt,
        Guid actorUserId,
        Guid? reversesAllocationId) => new()
    {
        Id = Guid.NewGuid(),
        MembershipFeeChargeId = chargeId,
        SettlementId = settlementId,
        Amount = amount,
        AllocatedAt = allocatedAt,
        ReversesAllocationId = reversesAllocationId,
        CreateDateTime = DateTime.UtcNow,
        CreateBy = actorUserId
    };

    private static decimal Net(IEnumerable<BilMembershipFeeAllocation> allocations) =>
        allocations.Sum(x => x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount);

    private static string ChargeLockKey(Guid chargeId) => $"BIL_MEMBERSHIP_FEE_CHARGE_{chargeId:N}";

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return Task.CompletedTask;
        return _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);
    }

    private static string ChargeStatusName(string status) => status switch
    {
        BillingMembershipFeeChargeStatuses.Unpaid => "Belum Dibayar",
        BillingMembershipFeeChargeStatuses.PartiallyPaid => "Dibayar Sebagian",
        BillingMembershipFeeChargeStatuses.Paid => "Lunas",
        BillingMembershipFeeChargeStatuses.Cancelled => "Dibatalkan",
        _ => status
    };

    private static string MembershipStatusName(MembershipStatus status) => status switch
    {
        MembershipStatus.PendingPayment => "Menunggu Pembayaran",
        MembershipStatus.Active => "Aktif",
        MembershipStatus.Scheduled => "Terjadwal",
        MembershipStatus.Expired => "Kedaluwarsa",
        MembershipStatus.Cancelled => "Dibatalkan",
        MembershipStatus.Upgraded => "Di-upgrade",
        MembershipStatus.NeedsVerification => "Perlu Verifikasi",
        _ => status.ToString()
    };

    private static string? MaskReference(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) return null;
        var value = reference.Trim();
        return value.Length <= 4 ? new string('•', value.Length) : new string('•', value.Length - 4) + value[^4..];
    }

    private sealed record LastPayment(string PaymentMethodName, string? Reference, string? KwitansiNumber);

    private sealed class ChargeRow
    {
        public BilMembershipFeeCharge Charge { get; set; } = null!;
        public string MemberNumber { get; set; } = string.Empty;
        public MembershipStatus MembershipStatus { get; set; }
        public MembershipRegistrationType RegistrationType { get; set; }
        public string TierName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
    }
}

public sealed class BillingMembershipFeeValidationException(string message) : Exception(message);

public sealed class BillingMembershipFeeConflictException(string message) : Exception(message);
