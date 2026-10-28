using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingDepositService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private const decimal MaxMoneyAmount = 9999999999999999.99m;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingNumberSeriesService _numberSeries;
    private readonly CashierShiftService _cashierShiftService;
    private readonly LoggerService _loggerService;

    public BillingDepositService(
        ApplicationDbContext dbContext,
        BillingNumberSeriesService numberSeries,
        CashierShiftService cashierShiftService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _numberSeries = numberSeries;
        _cashierShiftService = cashierShiftService;
        _loggerService = loggerService;
    }

    public async Task<DepositResponse> GetByEncounterAsync(
        Guid encounterId,
        CancellationToken cancellationToken)
    {
        if (encounterId == Guid.Empty)
            throw new BillingDepositValidationException("EncounterId wajib diisi.");

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
            ?? throw new KeyNotFoundException("Encounter tidak ditemukan.");

        var account = await _dbContext.BilDepositAccounts.AsNoTracking()
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Deposit rawat inap belum tersedia.");

        var (isTopUpEligible, topUpIneligibleReason) = await ResolveTopUpEligibilityAsync(encounter, account, cancellationToken);
        var response = MapDeposit(account);
        response.IsTopUpEligible = isTopUpEligible;
        response.TopUpIneligibleReason = topUpIneligibleReason;
        return response;
    }

    public async Task<DepositWorkspaceResponse> GetDepositWorkspaceAsync(
        Guid encounterId,
        int pageNumber,
        int pageSize,
        string? movementType,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        string? search,
        CancellationToken cancellationToken,
        string? status = null,
        string? sortDirection = null)
    {
        if (encounterId == Guid.Empty)
            throw new BillingDepositValidationException("EncounterId wajib diisi.");

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.ServiceUnit)
            .SingleOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
            ?? throw new KeyNotFoundException("Encounter tidak ditemukan.");

        if (encounter.EncounterType != EncounterType.Inpatient)
            throw new BillingDepositValidationException("Deposit hanya tersedia untuk encounter rawat inap.");

        var account = await _dbContext.BilDepositAccounts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

        var topUpEligibility = await ResolveTopUpEligibilityAsync(encounter, account, cancellationToken);

        if (account is null)
        {
            return new DepositWorkspaceResponse
            {
                EncounterId = encounter.Id,
                PatientId = encounter.PatientId,
                PatientName = encounter.Patient?.FullName ?? string.Empty,
                MedicalRecordNumber = encounter.Patient?.MedicalRecordNumber ?? string.Empty,
                EncounterNumber = encounter.EncounterNumber,
                EncounterType = encounter.EncounterType,
                EncounterTypeName = BuildEnumLabel(encounter.EncounterType),
                EncounterStatus = encounter.EncounterStatus,
                EncounterStatusName = BuildEnumLabel(encounter.EncounterStatus),
                ServiceUnitName = encounter.ServiceUnit?.ServiceUnitName ?? string.Empty,
                EncounterDate = encounter.EncounterDate,

                HasDepositAccount = false,
                DepositAccountId = null,
                AccountNumber = string.Empty,
                AccountStatus = string.Empty,
                AvailableBalance = 0m,
                RowVersion = null,
                TotalTopUpGross = 0m,
                TotalTopUpReversed = 0m,
                TotalAllocated = 0m,
                TotalReleasedOrRefunded = 0m,

                IsTopUpEligible = topUpEligibility.IsEligible,
                TopUpIneligibleReason = topUpEligibility.Reason,

                Items = [],
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = 0,
                TotalPage = 0
            };
        }

        // Hitung agregat mutasi deposit di database tanpa memuat seluruh entity ke memori
        var aggregates = await _dbContext.BilDepositMovements.AsNoTracking()
            .Where(x => x.DepositAccountId == account.Id && !x.IsDelete)
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalTopUpGross = g.Where(x => x.MovementType == BillingDepositMovementTypes.TopUp).Sum(x => (decimal?)x.Amount) ?? 0m,
                TotalTopUpReversed = g.Where(x => x.MovementType == BillingDepositMovementTypes.Reversal).Sum(x => (decimal?)x.Amount) ?? 0m,
                TotalAllocated = g.Where(x => x.MovementType == BillingDepositMovementTypes.Allocation).Sum(x => (decimal?)x.Amount) ?? 0m,
                // BKC-DEC-134: RELEASE dengan ReversesMovementId adalah pemulihan alokasi (bukan uang keluar),
                // RELEASE tanpa ReversesMovementId adalah refund kas murni.
                TotalAllocationRestored = g.Where(x => x.MovementType == BillingDepositMovementTypes.Release && x.ReversesMovementId != null).Sum(x => (decimal?)x.Amount) ?? 0m,
                TotalReleasedOrRefunded = g.Where(x => x.MovementType == BillingDepositMovementTypes.Release && x.ReversesMovementId == null).Sum(x => (decimal?)x.Amount) ?? 0m
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalTopUpGross = aggregates?.TotalTopUpGross ?? 0m;
        var totalTopUpReversed = aggregates?.TotalTopUpReversed ?? 0m;
        // Deposit terpakai = alokasi ke invoice dikurangi alokasi yang dipulihkan, sehingga
        // Masuk(net) - Terpakai - Pengembalian = Saldo tersedia.
        var totalAllocated = Math.Max(0m, (aggregates?.TotalAllocated ?? 0m) - (aggregates?.TotalAllocationRestored ?? 0m));
        var totalReleasedOrRefunded = aggregates?.TotalReleasedOrRefunded ?? 0m;

        // Proyeksi deterministik ringan untuk running balance yang konsisten terhadap ledger existing
        var allMovementsLedger = await _dbContext.BilDepositMovements.AsNoTracking()
            .Where(x => x.DepositAccountId == account.Id && !x.IsDelete)
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.CreateDateTime)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.MovementType,
                x.Amount,
                x.ReversesMovementId
            })
            .ToListAsync(cancellationToken);

        decimal runningBalance = 0;
        var balanceMap = new Dictionary<Guid, (decimal Effect, decimal BalanceAfter)>(allMovementsLedger.Count);
        foreach (var m in allMovementsLedger)
        {
            var effect = (m.MovementType == BillingDepositMovementTypes.TopUp ||
                          (m.MovementType == BillingDepositMovementTypes.Release && m.ReversesMovementId.HasValue))
                ? m.Amount
                : -m.Amount;
            runningBalance += effect;
            balanceMap[m.Id] = (effect, runningBalance);
        }

        // Top-up yang sudah dibalik oleh mutasi REVERSAL (status tampilan REVERSED).
        var reversedMovementIds = allMovementsLedger
            .Where(m => m.MovementType == BillingDepositMovementTypes.Reversal && m.ReversesMovementId.HasValue)
            .Select(m => m.ReversesMovementId!.Value)
            .Distinct()
            .ToList();

        // Query terfilter dan terpaginasi
        var filteredQuery = _dbContext.BilDepositMovements.AsNoTracking()
            .Where(x => x.DepositAccountId == account.Id && !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(movementType))
        {
            var normalizedType = movementType.Trim().ToUpperInvariant();
            filteredQuery = filteredQuery.Where(x => x.MovementType == normalizedType);
        }

        var rangeStartUtc = ResolveRangeStart(startDate);
        if (rangeStartUtc.HasValue)
        {
            filteredQuery = filteredQuery.Where(x => x.OccurredAt >= rangeStartUtc.Value);
        }

        var rangeEndExclusiveUtc = ResolveRangeEndExclusive(endDate);
        if (rangeEndExclusiveUtc.HasValue)
        {
            filteredQuery = filteredQuery.Where(x => x.OccurredAt < rangeEndExclusiveUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            // Status diturunkan dari ledger (append-only): REVERSED bila sudah ada REVERSAL yang merujuknya.
            var normalizedStatus = status.Trim().ToUpperInvariant();
            if (normalizedStatus == DepositMovementStatuses.Reversed)
            {
                filteredQuery = filteredQuery.Where(x => reversedMovementIds.Contains(x.Id));
            }
            else if (normalizedStatus == DepositMovementStatuses.Posted)
            {
                filteredQuery = filteredQuery.Where(x => !reversedMovementIds.Contains(x.Id));
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            // Nomor transaksi diturunkan dari Id sehingga dicocokkan di memori terhadap ledger akun
            // (jumlah mutasi per kunjungan kecil); hasilnya diterapkan sebagai himpunan Id ke query.
            var matchedByTransactionNumber = allMovementsLedger
                .Where(m => BuildTransactionNumber(m.Id).ToLower().Contains(keyword))
                .Select(m => m.Id)
                .ToList();

            filteredQuery = filteredQuery.Where(x =>
                (x.ReferenceNumber != null && x.ReferenceNumber.ToLower().Contains(keyword)) ||
                x.Reason.ToLower().Contains(keyword) ||
                matchedByTransactionNumber.Contains(x.Id));
        }

        var totalData = await filteredQuery.CountAsync(cancellationToken);
        var totalPage = totalData == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize);

        // Urutan deterministik. Default "asc" (urutan buku besar) dipertahankan untuk konsumen lama;
        // "desc" dipakai layar riwayat agar mutasi terbaru tampil di halaman pertama.
        var descending = string.Equals(sortDirection?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
        var orderedQuery = descending
            ? filteredQuery.OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.CreateDateTime).ThenByDescending(x => x.Id)
            : filteredQuery.OrderBy(x => x.OccurredAt).ThenBy(x => x.CreateDateTime).ThenBy(x => x.Id);

        var pageEntities = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Jenis order asal top-up (INITIAL_DEPOSIT → "Deposit Awal"); top-up tanpa order tetap "Top Up".
        var pageMovementIds = pageEntities.Select(x => x.Id).ToList();
        var orderTypeRows = await (from payment in _dbContext.BilDepositOrderPayments.AsNoTracking()
                                   join order in _dbContext.BilDepositPaymentOrders.AsNoTracking()
                                       on payment.DepositOrderId equals order.Id
                                   where pageMovementIds.Contains(payment.DepositMovementId)
                                         && !payment.IsDelete
                                         && payment.Status == DepositOrderPaymentStatuses.Success
                                   select new { payment.DepositMovementId, order.OrderType })
            .ToListAsync(cancellationToken);
        var orderTypeByMovement = orderTypeRows
            .GroupBy(x => x.DepositMovementId)
            .ToDictionary(g => g.Key, g => g.First().OrderType);

        var paymentMethodIds = pageEntities
            .Where(x => x.PaymentMethodId.HasValue)
            .Select(x => x.PaymentMethodId!.Value)
            .Distinct()
            .ToList();

        var paymentMethods = paymentMethodIds.Count > 0
            ? await _dbContext.MstPaymentMethods.AsNoTracking()
                .Where(x => paymentMethodIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, MstPaymentMethod>();

        var accountIds = pageEntities
            .Where(x => x.PaymentMethodAccountId.HasValue)
            .Select(x => x.PaymentMethodAccountId!.Value)
            .Distinct()
            .ToList();

        var paymentMethodAccounts = accountIds.Count > 0
            ? await _dbContext.MstPaymentMethodAccounts.AsNoTracking()
                .Where(x => accountIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, MstPaymentMethodAccount>();

        var cashierIds = pageEntities
            .Where(x => x.CreateBy != Guid.Empty)
            .Select(x => x.CreateBy)
            .Distinct()
            .ToList();

        var cashierNames = cashierIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(x => cashierIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.UserName ?? "Kasir", cancellationToken)
            : new Dictionary<Guid, string>();

        var items = pageEntities.Select(x =>
        {
            balanceMap.TryGetValue(x.Id, out var balanceInfo);

            string? pmName = null;
            string? bankName = null;
            string? accNum = null;

            if (x.PaymentMethodAccountId.HasValue && paymentMethodAccounts.TryGetValue(x.PaymentMethodAccountId.Value, out var pma))
            {
                bankName = pma.BankName;
                accNum = pma.AccountNumber;
            }

            if (x.PaymentMethodId.HasValue && paymentMethods.TryGetValue(x.PaymentMethodId.Value, out var pm))
            {
                pmName = pm.PaymentMethodName;
                bankName ??= pm.BankName;
                accNum ??= pm.BankAccountNumber;
            }
            else if (x.MovementType == BillingDepositMovementTypes.Allocation)
            {
                pmName = "Deposit";
            }

            cashierNames.TryGetValue(x.CreateBy, out var cashierName);

            orderTypeByMovement.TryGetValue(x.Id, out var sourceOrderType);
            var movementStatus = reversedMovementIds.Contains(x.Id)
                ? DepositMovementStatuses.Reversed
                : DepositMovementStatuses.Posted;

            return new DepositMovementResponse
            {
                Id = x.Id,
                TransactionNumber = BuildTransactionNumber(x.Id),
                MovementType = x.MovementType,
                MovementTypeName = BuildMovementTypeName(x.MovementType, x.ReversesMovementId.HasValue, sourceOrderType),
                Status = movementStatus,
                StatusName = BuildMovementStatusName(movementStatus),
                Amount = x.Amount,
                BalanceEffect = balanceInfo.Effect,
                BalanceAfter = balanceInfo.BalanceAfter,
                SettlementId = x.SettlementId,
                PaymentMethodId = x.PaymentMethodId,
                PaymentMethodAccountId = x.PaymentMethodAccountId,
                PaymentMethodName = pmName,
                BankName = bankName,
                AccountNumber = accNum,
                ReferenceNumber = x.ReferenceNumber ?? (x.SettlementId.HasValue ? $"INV-SETTLE-{x.SettlementId.Value:N}"[..15] : null),
                CashierName = cashierName ?? "Kasir",
                CashierShiftId = x.CashierShiftId,
                CorrelationId = x.CorrelationId,
                OccurredAt = x.OccurredAt,
                Reason = x.Reason,
                ReversesMovementId = x.ReversesMovementId
            };
        }).ToList();

        return new DepositWorkspaceResponse
        {
            EncounterId = encounter.Id,
            PatientId = encounter.PatientId,
            PatientName = encounter.Patient?.FullName ?? string.Empty,
            MedicalRecordNumber = encounter.Patient?.MedicalRecordNumber ?? string.Empty,
            EncounterNumber = encounter.EncounterNumber,
            EncounterType = encounter.EncounterType,
            EncounterTypeName = BuildEnumLabel(encounter.EncounterType),
            EncounterStatus = encounter.EncounterStatus,
            EncounterStatusName = BuildEnumLabel(encounter.EncounterStatus),
            ServiceUnitName = encounter.ServiceUnit?.ServiceUnitName ?? string.Empty,
            EncounterDate = encounter.EncounterDate,

            HasDepositAccount = true,
            DepositAccountId = account.Id,
            AccountNumber = account.AccountNumber,
            AccountStatus = account.Status,
            AvailableBalance = account.AvailableBalance,
            RowVersion = account.RowVersion,
            TotalTopUpGross = totalTopUpGross,
            TotalTopUpReversed = totalTopUpReversed,
            TotalAllocated = totalAllocated,
            TotalReleasedOrRefunded = totalReleasedOrRefunded,

            IsTopUpEligible = topUpEligibility.IsEligible,
            TopUpIneligibleReason = topUpEligibility.Reason,

            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = totalPage
        };
    }

    public async Task<DepositPolicyResponse> GetDepositPolicyAsync(
        Guid? guarantorId,
        Guid? patientClassId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var activePolicies = await _dbContext.MstDepositPolicies.AsNoTracking()
            .Where(x => x.IsActive
                && !x.IsDelete
                && !x.IsCancel
                && x.EffectiveFrom <= now
                && (x.EffectiveTo == null || x.EffectiveTo > now))
            .ToListAsync(cancellationToken);

        var policy = (guarantorId.HasValue && patientClassId.HasValue
                ? activePolicies.FirstOrDefault(x => x.GuarantorId == guarantorId.Value && x.PatientClassId == patientClassId.Value)
                : null)
            ?? (guarantorId.HasValue
                ? activePolicies.FirstOrDefault(x => x.GuarantorId == guarantorId.Value && x.PatientClassId == null)
                : null)
            ?? (patientClassId.HasValue
                ? activePolicies.FirstOrDefault(x => x.GuarantorId == null && x.PatientClassId == patientClassId.Value)
                : null)
            ?? activePolicies.FirstOrDefault(x => x.GuarantorId == null && x.PatientClassId == null);

        if (policy is not null)
        {
            return new DepositPolicyResponse
            {
                IsRequired = policy.IsRequired,
                MinimumAmount = policy.MinimumAmount,
                FollowUpIntervalDays = policy.FollowUpIntervalDays,
                GuarantorId = policy.GuarantorId,
                PatientClassId = policy.PatientClassId,
                PolicyId = policy.Id,
                PolicyCode = policy.Code,
                PolicyName = policy.Name,
                Description = policy.Description,
                EffectiveFrom = policy.EffectiveFrom,
                EffectiveTo = policy.EffectiveTo
            };
        }

        return new DepositPolicyResponse
        {
            IsRequired = true,
            MinimumAmount = 10000000m,
            FollowUpIntervalDays = 3,
            GuarantorId = guarantorId,
            PatientClassId = patientClassId,
            PolicyId = null,
            PolicyCode = "DEP-POLICY-DEFAULT",
            PolicyName = "Kebijakan Deposit Rawat Inap Standar",
            Description = "Kebijakan deposit rawat inap minimum Rp 10.000.000 berlaku di semua kelas.",
            EffectiveFrom = null,
            EffectiveTo = null
        };
    }

    public async Task<EpisodeDepositSummaryResponse> GetEpisodeDepositSummaryAsync(
        Guid episodeId,
        CancellationToken cancellationToken)
    {
        if (episodeId == Guid.Empty)
            throw new BillingDepositValidationException("EpisodeId wajib diisi.");

        var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Episode rawat inap tidak ditemukan.");

        var guarantor = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
            .Where(x => x.EncounterId == episode.EncounterId && x.IsActive && !x.IsDelete && !x.IsCancel)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Priority)
            .FirstOrDefaultAsync(cancellationToken);

        var guarantorId = guarantor?.InsuranceProviderId ?? guarantor?.CompanyGuarantorId;
        var patientClassId = episode.PatientClassId != Guid.Empty ? (Guid?)episode.PatientClassId : null;

        var policy = await GetDepositPolicyAsync(guarantorId, patientClassId, cancellationToken);

        var account = await _dbContext.BilDepositAccounts.AsNoTracking()
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(x => x.EncounterId == episode.EncounterId && !x.IsDelete, cancellationToken);

        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => x.EncounterId == episode.EncounterId && !x.IsDelete)
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        decimal finalBillAmount = 0m;
        if (invoice != null)
        {
            var calc = await _dbContext.BilCalculationVersions.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete)
                .FirstOrDefaultAsync(cancellationToken);
            finalBillAmount = calc?.PatientAmount ?? 0m;
        }

        decimal totalReceived = 0m;
        decimal totalAllocated = 0m;
        decimal totalRefunded = 0m;
        decimal availableBalance = 0m;

        if (account != null)
        {
            var activeMovements = account.Movements.Where(x => !x.IsDelete).ToList();
            var totalTopUp = activeMovements
                .Where(x => x.MovementType == BillingDepositMovementTypes.TopUp)
                .Sum(x => x.Amount);
            var totalReversal = activeMovements
                .Where(x => x.MovementType == BillingDepositMovementTypes.Reversal)
                .Sum(x => x.Amount);

            totalReceived = Math.Max(0m, totalTopUp - totalReversal);
            totalAllocated = activeMovements
                .Where(x => x.MovementType == BillingDepositMovementTypes.Allocation)
                .Sum(x => x.Amount);
            // BKC-DEC-134 / BKC-DES-056: totalRefunded hanya menghitung mutasi RELEASE yang merupakan refund kas murni
            // (ReversesMovementId == null). Mutasi RELEASE pembatalan alokasi (ReversesMovementId != null) dikecualikan
            // karena tidak ada pergerakan kas keluar dari kasir/rumah sakit.
            totalRefunded = activeMovements
                .Where(x => x.MovementType == BillingDepositMovementTypes.Release && !x.ReversesMovementId.HasValue)
                .Sum(x => x.Amount);
            availableBalance = account.AvailableBalance;
        }

        var policyShortfallAmount = policy.IsRequired && policy.MinimumAmount > 0
            ? Math.Max(0m, policy.MinimumAmount - totalReceived)
            : 0m;
        var finalBillShortfallAmount = Math.Max(0m, finalBillAmount - totalAllocated - availableBalance);
        var outstandingTopUp = policyShortfallAmount;

        return new EpisodeDepositSummaryResponse
        {
            EpisodeId = episode.Id,
            EncounterId = episode.EncounterId,
            DepositAccountId = account?.Id,
            AccountNumber = account?.AccountNumber ?? string.Empty,
            DepositStatus = account?.Status ?? string.Empty,
            HasDepositAccount = account != null,
            IsPolicyRequired = policy.IsRequired,
            MinimumPolicyAmount = policy.MinimumAmount,
            TotalReceived = totalReceived,
            TotalAllocated = totalAllocated,
            TotalRefunded = totalRefunded,
            AvailableBalance = availableBalance,
            PolicyShortfallAmount = policyShortfallAmount,
            FinalBillAmount = finalBillAmount,
            FinalBillShortfallAmount = finalBillShortfallAmount,
            OutstandingTopUp = outstandingTopUp,
            FollowUpIntervalDays = policy.FollowUpIntervalDays,
            GuarantorId = guarantorId,
            PatientClassId = patientClassId
        };
    }

    public async Task<SettlementResponse> TopUpAsync(
        Guid encounterId,
        DepositTopUpRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken,
        bool deferAudit = false)
    {
        ValidateTopUpRequest(encounterId, request, idempotencyKey);
        var payloadHash = ComputeTopUpPayloadHash(encounterId, request);
        var legacyPayloadHash = ComputeLegacyTopUpPayloadHash(encounterId, request);
        var cashReceiptApplied = false;
        Guid? cashReceiptSourceId = null;
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                // Admisi transfer menyertakan ledger dalam transaksi bed yang sama.
                if (_dbContext.Database.CurrentTransaction is null)
                    transaction = await _dbContext.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_DEPOSIT_{encounterId:N}", cancellationToken);
            }

            var priorMovement = await _dbContext.BilDepositMovements.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (priorMovement is not null)
            {
                var isPayloadMatch = priorMovement.PayloadHash == payloadHash
                    || priorMovement.PayloadHash == legacyPayloadHash;

                if (priorMovement.MovementType != BillingDepositMovementTypes.TopUp
                    || !isPayloadMatch)
                    throw new BillingDepositConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");

                var replayAccount = await LoadAccountAsync(priorMovement.DepositAccountId, cancellationToken);
                if (replayAccount.EncounterId != encounterId)
                    throw new BillingDepositConflictException(
                        "Permintaan idempotent tidak sesuai dengan encounter deposit.");

                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                if (!deferAudit) await AuditTopUpAsync(replayAccount, priorMovement, actorUserId, true);
                return MapSettlement(replayAccount, priorMovement, true);
            }
            if (await _dbContext.BilDepositMovements.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingDepositConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == encounterId && !x.IsDelete && !x.IsCancel,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Encounter tidak ditemukan.");
            if (encounter.EncounterType != EncounterType.Inpatient)
                throw new BillingDepositValidationException(
                    "Deposit hanya tersedia untuk encounter rawat inap.");
            if (encounter.EncounterStatus is EncounterStatus.Cancelled or EncounterStatus.NoShow)
                throw new BillingDepositValidationException(
                    "Encounter yang dibatalkan atau tidak hadir tidak dapat menerima deposit.");

            var paymentMethod = await _dbContext.MstPaymentMethods.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == request.PaymentMethodId && !x.IsDelete && !x.IsCancel,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Metode pembayaran tidak ditemukan.");
            ValidatePaymentMethod(paymentMethod.IsActive, paymentMethod.IsAvailableForBilling,
                paymentMethod.IsInsurance, paymentMethod.IsCompanyGuarantor,
                paymentMethod.IsMembership, paymentMethod.IsNeedReferenceNumber,
                paymentMethod.IsNeedApproval, paymentMethod.IsNeedAttachment,
                paymentMethod.PaymentMethodName, request.ReferenceNumber);
            BilCashierShift? cashierShift = null;
            if (paymentMethod.IsCash)
            {
                try
                {
                    cashierShift = await _cashierShiftService.RequireActiveShiftAsync(
                        actorUserId, cancellationToken);
                }
                catch (CashierShiftValidationException exception)
                {
                    throw new BillingDepositValidationException(exception.Message);
                }
                catch (CashierShiftForbiddenException exception)
                {
                    throw new BillingDepositValidationException(exception.Message);
                }
            }

            var account = await _dbContext.BilDepositAccounts
                .Include(x => x.Movements)
                .SingleOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

            var (isTopUpEligible, topUpIneligibleReason) = await ResolveTopUpEligibilityAsync(encounter, account, cancellationToken);
            if (!isTopUpEligible)
            {
                throw new BillingDepositValidationException(topUpIneligibleReason ?? "Tagihan sudah lunas, top-up deposit tidak tersedia.");
            }

            var now = DateTimeOffset.UtcNow;

            if (account is null)
            {
                if (request.ExpectedRowVersion.HasValue)
                    throw new BillingDepositConflictException(
                        "Data telah berubah. Muat ulang sebelum melanjutkan.");

                account = new BilDepositAccount
                {
                    EncounterId = encounterId,
                    AccountNumber = await _numberSeries.AllocateDepositAccountNumberAsync(
                        actorUserId, now, cancellationToken),
                    AvailableBalance = 0,
                    Status = BillingDepositAccountStatuses.Active,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.BilDepositAccounts.Add(account);
            }
            else
            {
                EnsureActiveAndCurrent(account, request.ExpectedRowVersion);
            }

            var beforeBalance = account.AvailableBalance;
            var afterBalance = checked(beforeBalance + request.Amount);
            if (afterBalance > MaxMoneyAmount)
                throw new BillingDepositValidationException(
                    "Saldo deposit melebihi batas nominal yang didukung.");
            var movement = new BilDepositMovement
            {
                DepositAccountId = account.Id,
                DepositAccount = account,
                MovementType = BillingDepositMovementTypes.TopUp,
                Amount = request.Amount,
                PaymentMethodId = request.PaymentMethodId,
                PaymentMethodAccountId = request.PaymentMethodAccountId,
                ReferenceNumber = string.IsNullOrWhiteSpace(request.ReferenceNumber) ? null : request.ReferenceNumber.Trim(),
                CashierShiftId = cashierShift?.Id,
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                OccurredAt = now,
                Reason = request.Reason.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            account.Movements.Add(movement);
            _dbContext.BilDepositMovements.Add(movement);
            if (cashierShift is not null)
            {
                cashReceiptApplied = await _cashierShiftService.ApplyCashReceiptAsync(
                    cashierShift,
                    "DEPOSIT_MOVEMENT",
                    movement.Id,
                    movement.Amount,
                    actorUserId,
                    movement.CorrelationId,
                    movement.CausationId,
                    movement.OccurredAt,
                    cancellationToken);
                cashReceiptSourceId = movement.Id;
            }
            account.AvailableBalance = afterBalance;
            account.RowVersion = Guid.NewGuid();
            account.UpdateDateTime = DateTime.UtcNow;
            account.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            if (!deferAudit) await AuditTopUpAsync(account, movement, actorUserId, false, beforeBalance);
            if (!deferAudit && cashReceiptApplied && cashReceiptSourceId.HasValue)
                await _cashierShiftService.AuditCashReceiptAsync(
                    cashReceiptSourceId.Value, cancellationToken);
            return MapSettlement(account, movement, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDepositConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDepositConflictException(
                "Top-up tidak dapat disimpan karena account, correlation, atau idempotency key sudah diproses.",
                exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    /// <summary>Audit ledger penerimaan yang sudah di-commit oleh orchestrator admisi.</summary>
    public async Task AuditAdmissionTopUpAsync(Guid encounterId, Guid idempotencyKey,
        Guid actorUserId, CancellationToken cancellationToken)
    {
        var movement = await _dbContext.BilDepositMovements.AsNoTracking().SingleAsync(
            x => x.IdempotencyKey == idempotencyKey && x.MovementType == BillingDepositMovementTypes.TopUp,
            cancellationToken);
        var account = await LoadAccountAsync(movement.DepositAccountId, cancellationToken);
        if (account.EncounterId != encounterId)
            throw new BillingDepositConflictException("Audit penerimaan tidak sesuai kunjungan admisi.");
        await AuditTopUpAsync(account, movement, actorUserId, false, account.AvailableBalance - movement.Amount);
        if (movement.CashierShiftId.HasValue)
            await _cashierShiftService.AuditCashReceiptAsync(movement.Id, cancellationToken);
    }

    public async Task<DepositResponse> ReverseTopUpAsync(
        Guid encounterId,
        Guid movementId,
        ReverseDepositMovementRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateReversalRequest(encounterId, movementId, request, idempotencyKey);
        var payloadHash = ComputeReversalPayloadHash(encounterId, movementId, request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_DEPOSIT_{encounterId:N}", cancellationToken);
            }

            var priorReversal = await _dbContext.BilDepositMovements.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (priorReversal is not null)
            {
                if (priorReversal.MovementType != BillingDepositMovementTypes.Reversal
                    || priorReversal.ReversesMovementId != movementId
                    || priorReversal.PayloadHash != payloadHash)
                    throw new BillingDepositConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");

                var replayAccount = await LoadAccountAsync(priorReversal.DepositAccountId, cancellationToken);
                if (replayAccount.EncounterId != encounterId)
                    throw new BillingDepositConflictException(
                        "Permintaan idempotent tidak sesuai dengan encounter deposit.");

                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditReversalAsync(replayAccount, priorReversal, actorUserId, true);
                return MapDeposit(replayAccount);
            }
            if (await _dbContext.BilDepositMovements.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingDepositConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var account = await _dbContext.BilDepositAccounts
                .Include(x => x.Movements)
                .SingleOrDefaultAsync(
                    x => x.EncounterId == encounterId && !x.IsDelete,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Deposit rawat inap belum tersedia.");
            EnsureActiveAndCurrent(account, request.ExpectedRowVersion);

            var original = account.Movements.SingleOrDefault(
                x => x.Id == movementId && !x.IsDelete)
                ?? throw new KeyNotFoundException("Movement deposit tidak ditemukan.");
            if (original.MovementType != BillingDepositMovementTypes.TopUp)
                throw new BillingDepositValidationException(
                    "Hanya movement top-up yang dapat dibalik melalui operasi ini.");
            if (account.Movements.Any(x => x.ReversesMovementId == movementId && !x.IsDelete))
                throw new BillingDepositConflictException("Movement top-up sudah pernah dibalik.");
            if (account.AvailableBalance < original.Amount)
                throw new BillingDepositValidationException(
                    "Dana deposit atau saldo tagihan tidak mencukupi.");

            var beforeBalance = account.AvailableBalance;
            var now = DateTimeOffset.UtcNow;
            var reversal = new BilDepositMovement
            {
                DepositAccountId = account.Id,
                DepositAccount = account,
                MovementType = BillingDepositMovementTypes.Reversal,
                Amount = original.Amount,
                SettlementId = original.SettlementId,
                PaymentMethodId = original.PaymentMethodId,
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                OccurredAt = now,
                Reason = request.Reason.Trim(),
                ReversesMovementId = original.Id,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            account.Movements.Add(reversal);
            _dbContext.BilDepositMovements.Add(reversal);
            account.AvailableBalance = beforeBalance - original.Amount;
            account.RowVersion = Guid.NewGuid();
            account.UpdateDateTime = DateTime.UtcNow;
            account.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditReversalAsync(account, reversal, actorUserId, false, beforeBalance);
            return MapDeposit(account);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDepositConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDepositConflictException(
                "Reversal tidak dapat disimpan karena account, movement, correlation, atau idempotency key sudah diproses.",
                exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private async Task<BilDepositAccount> LoadAccountAsync(
        Guid accountId,
        CancellationToken cancellationToken) =>
        await _dbContext.BilDepositAccounts.AsNoTracking()
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.Id == accountId && !x.IsDelete, cancellationToken)
        ?? throw new BillingDepositConflictException(
            "Receipt idempotency tidak memiliki account deposit yang valid.");

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditTopUpAsync(
        BilDepositAccount account,
        BilDepositMovement movement,
        Guid actorUserId,
        bool isReplay,
        decimal? beforeBalance = null)
    {
        var resolvedBefore = beforeBalance ?? account.AvailableBalance - movement.Amount;
        return _loggerService.AuditAsync(
            LogCategory,
            "BillingDeposit.TopUp",
            "Top-up dana rawat inap dicatat sebagai movement deposit yang belum dialokasikan.",
            new
            {
                DepositAccountId = account.Id,
                DepositMovementId = movement.Id,
                movement.PaymentMethodId,
                movement.Amount,
                BeforeBalance = resolvedBefore,
                AfterBalance = resolvedBefore + movement.Amount,
                movement.IdempotencyKey,
                movement.CorrelationId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });
    }

    private Task AuditReversalAsync(
        BilDepositAccount account,
        BilDepositMovement reversal,
        Guid actorUserId,
        bool isReplay,
        decimal? beforeBalance = null)
    {
        var resolvedBefore = beforeBalance ?? account.AvailableBalance + reversal.Amount;
        return _loggerService.AuditAsync(
            LogCategory,
            "BillingDeposit.ReverseTopUp",
            "Top-up deposit dibalik melalui movement kompensasi baru.",
            new
            {
                DepositAccountId = account.Id,
                DepositMovementId = reversal.Id,
                reversal.ReversesMovementId,
                reversal.Amount,
                BeforeBalance = resolvedBefore,
                AfterBalance = resolvedBefore - reversal.Amount,
                reversal.IdempotencyKey,
                reversal.CorrelationId,
                Reason = reversal.Reason,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });
    }

    private static void EnsureActiveAndCurrent(
        BilDepositAccount account,
        Guid? expectedRowVersion)
    {
        if (account.Status != BillingDepositAccountStatuses.Active)
            throw new BillingDepositValidationException("Account deposit sudah ditutup.");
        if (!expectedRowVersion.HasValue || account.RowVersion != expectedRowVersion.Value)
            throw new BillingDepositConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.");
    }

    private static void ValidatePaymentMethod(
        bool isActive,
        bool isAvailableForBilling,
        bool isInsurance,
        bool isCompanyGuarantor,
        bool isMembership,
        bool needsReference,
        bool needsApproval,
        bool needsAttachment,
        string paymentMethodName,
        string? referenceNumber)
    {
        if (!isActive || !isAvailableForBilling)
            throw new BillingDepositValidationException(
                "Metode pembayaran tidak aktif atau tidak tersedia untuk Billing.");
        if (isInsurance || isCompanyGuarantor || isMembership)
            throw new BillingDepositValidationException(
                "Metode penjamin tidak dapat digunakan sebagai top-up deposit.");
        // Persetujuan dan lampiran butuh alur settlement/tender yang belum tersedia untuk top-up deposit.
        if (needsApproval || needsAttachment)
            throw new BillingDepositValidationException(
                "Metode pembayaran ini memerlukan settlement/tender yang belum tersedia.");
        // Metode yang mewajibkan nomor referensi (mis. QRIS manual, transfer) cukup disertai referensi
        // dari bukti transaksi; tanpa referensi penerimaan tetap ditolak. Referensi tidak pernah dibuat sistem.
        if (needsReference && string.IsNullOrWhiteSpace(referenceNumber))
            throw new BillingDepositValidationException(
                $"Metode {paymentMethodName} mewajibkan nomor referensi / nomor transaksi.");
    }

    private async Task<(bool IsEligible, string? Reason)> ResolveTopUpEligibilityAsync(
        RegPatientEncounter encounter,
        BilDepositAccount? account,
        CancellationToken cancellationToken)
    {
        if (encounter.EncounterStatus is EncounterStatus.Cancelled or EncounterStatus.NoShow)
            return (false, "Kunjungan yang dibatalkan atau tidak hadir tidak dapat menerima deposit.");
        if (account is not null && account.Status != BillingDepositAccountStatuses.Active)
            return (false, "Akun deposit sudah ditutup dan tidak dapat menerima top-up baru.");

        // Validasi posisi tagihan pasien: jika tagihan pasien sudah lunas/terselesaikan, top-up dilarang
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => x.EncounterId == encounter.Id && !x.IsDelete)
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (invoice != null)
        {
            if (invoice.Status == BillingInvoiceStatuses.Closed)
            {
                return (false, "Tagihan sudah lunas, top-up deposit tidak tersedia.");
            }

            var calc = await _dbContext.BilCalculationVersions.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && !x.IsDelete)
                .OrderByDescending(x => x.VersionNo)
                .FirstOrDefaultAsync(cancellationToken);

            var closureService = new BillingInvoiceClosureService(_dbContext);
            decimal outstanding;
            if (calc != null)
            {
                outstanding = await closureService.CalculateOutstandingAsync(invoice, calc, cancellationToken);
            }
            else
            {
                var totalPaid = await _dbContext.BilPaymentAllocations.AsNoTracking()
                    .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                    .SumAsync(x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount), cancellationToken) ?? 0m;
                var totalCharges = invoice.Items?
                    .Where(x => x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
                    .Sum(x => x.Quantity * x.UnitPrice) ?? 0m;
                outstanding = Math.Max(0m, totalCharges - totalPaid);
            }

            if (outstanding <= 0m)
            {
                return (false, "Tagihan sudah lunas, top-up deposit tidak tersedia.");
            }
        }

        return (true, null);
    }

    /// <summary>Nomor transaksi tampilan yang stabil, diturunkan dari Id mutasi (tidak disimpan di database).</summary>
    internal static string BuildTransactionNumber(Guid movementId) =>
        $"DEP-{movementId:N}"[..12].ToUpperInvariant();

    private static string BuildMovementTypeName(string movementType, bool reversesAnotherMovement, string? sourceOrderType) =>
        movementType switch
        {
            BillingDepositMovementTypes.TopUp when sourceOrderType == DepositOrderTypes.InitialDeposit => "Deposit Awal",
            BillingDepositMovementTypes.TopUp => "Top Up",
            BillingDepositMovementTypes.Allocation => "Pemakaian Deposit",
            BillingDepositMovementTypes.Release when reversesAnotherMovement => "Pemulihan Alokasi",
            BillingDepositMovementTypes.Release => "Refund",
            BillingDepositMovementTypes.Reversal => "Koreksi (Pembalikan Top Up)",
            _ => movementType
        };

    private static string BuildMovementStatusName(string status) =>
        status == DepositMovementStatuses.Reversed ? "Dibalik" : "Berhasil";

    /// <summary>
    /// Mengubah batas awal rentang tanggal menjadi instan UTC. Nilai berjam 00:00 dibaca sebagai
    /// tanggal operasional (WIB), sehingga "2026-10-19" berarti awal hari 19 Oktober waktu setempat.
    /// </summary>
    internal static DateTimeOffset? ResolveRangeStart(DateTimeOffset? value)
    {
        if (!value.HasValue) return null;
        var v = value.Value;
        return v.TimeOfDay == TimeSpan.Zero
            ? new DateTimeOffset(AppDateTimeHelper.OperationalDateToUtc(v.DateTime), TimeSpan.Zero)
            : v;
    }

    /// <summary>
    /// Batas akhir rentang tanggal sebagai batas eksklusif UTC. Tanggal tanpa jam mencakup seluruh
    /// hari tersebut (awal hari berikutnya), bukan berhenti pada pukul 00:00.
    /// </summary>
    internal static DateTimeOffset? ResolveRangeEndExclusive(DateTimeOffset? value)
    {
        if (!value.HasValue) return null;
        var v = value.Value;
        return v.TimeOfDay == TimeSpan.Zero
            ? new DateTimeOffset(AppDateTimeHelper.OperationalDateToUtc(v.DateTime.Date.AddDays(1)), TimeSpan.Zero)
            : v.AddTicks(1);
    }

    private static void ValidateTopUpRequest(
        Guid encounterId,
        DepositTopUpRequest request,
        Guid idempotencyKey)
    {
        if (encounterId == Guid.Empty)
            throw new BillingDepositValidationException("EncounterId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingDepositValidationException("Idempotency-Key wajib diisi.");
        if (request.PaymentMethodId == Guid.Empty)
            throw new BillingDepositValidationException("PaymentMethodId wajib diisi.");
        if (request.Amount <= 0 || request.Amount > MaxMoneyAmount
            || decimal.Round(request.Amount, 2) != request.Amount)
            throw new BillingDepositValidationException(
                "Nominal top-up harus positif dan maksimal memiliki dua angka desimal.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingDepositValidationException(
                "ExpectedRowVersion tidak boleh Guid.Empty.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BillingDepositValidationException("Alasan top-up wajib diisi.");
        if (request.Reason.Trim().Length > 500)
            throw new BillingDepositValidationException("Alasan top-up maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingDepositValidationException(
                "CorrelationId dan CausationId wajib diisi.");
    }

    private static void ValidateReversalRequest(
        Guid encounterId,
        Guid movementId,
        ReverseDepositMovementRequest request,
        Guid idempotencyKey)
    {
        if (encounterId == Guid.Empty || movementId == Guid.Empty)
            throw new BillingDepositValidationException(
                "EncounterId dan MovementId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingDepositValidationException("Idempotency-Key wajib diisi.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingDepositValidationException("ExpectedRowVersion wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BillingDepositValidationException("Alasan reversal wajib diisi.");
        if (request.Reason.Trim().Length > 500)
            throw new BillingDepositValidationException("Alasan reversal maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingDepositValidationException(
                "CorrelationId dan CausationId wajib diisi.");
    }

    private static string ComputeTopUpPayloadHash(
        Guid encounterId,
        DepositTopUpRequest request)
    {
        var canonical = string.Join('|',
            encounterId.ToString("N"),
            request.PaymentMethodId.ToString("N"),
            request.PaymentMethodAccountId?.ToString("N") ?? string.Empty,
            request.ReferenceNumber?.Trim() ?? string.Empty,
            request.Amount.ToString(CultureInfo.InvariantCulture),
            request.ExpectedRowVersion?.ToString("N") ?? string.Empty,
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static string ComputeLegacyTopUpPayloadHash(
        Guid encounterId,
        DepositTopUpRequest request)
    {
        var canonical = string.Join('|',
            encounterId.ToString("N"),
            request.PaymentMethodId.ToString("N"),
            request.Amount.ToString(CultureInfo.InvariantCulture),
            request.ExpectedRowVersion?.ToString("N") ?? string.Empty,
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static string BuildEnumLabel<TEnum>(TEnum value) where TEnum : Enum
    {
        var memberInfo = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
        var displayAttribute = memberInfo?
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .OfType<DisplayAttribute>()
            .FirstOrDefault();

        var displayName = displayAttribute?.GetName();
        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName;

        return string.Concat(value.ToString().Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
    }

    private static string ComputeReversalPayloadHash(
        Guid encounterId,
        Guid movementId,
        ReverseDepositMovementRequest request)
    {
        var canonical = string.Join('|',
            encounterId.ToString("N"),
            movementId.ToString("N"),
            request.ExpectedRowVersion.ToString("N"),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private SettlementResponse MapSettlement(
        BilDepositAccount account,
        BilDepositMovement movement,
        bool isReplay) => new()
    {
        RequestedAmount = movement.Amount,
        SuccessfulAmount = movement.Amount,
        AllocatedAmount = 0,
        OutstandingAmount = 0,
        CollectibleAmount = 0,
        Status = BillingSettlementStatuses.Settled,
        IsReplay = isReplay,
        CorrelationId = movement.CorrelationId,
        DepositMovementId = movement.Id,
        Deposit = MapDeposit(account)
    };

    private DepositResponse MapDeposit(BilDepositAccount account)
    {
        var paymentMethodIds = account.Movements
            .Where(x => x.PaymentMethodId.HasValue)
            .Select(x => x.PaymentMethodId!.Value)
            .Distinct()
            .ToList();

        var paymentMethods = paymentMethodIds.Count > 0
            ? _dbContext.MstPaymentMethods.AsNoTracking()
                .Where(x => paymentMethodIds.Contains(x.Id))
                .ToDictionary(x => x.Id)
            : new Dictionary<Guid, MstPaymentMethod>();

        var accountIds = account.Movements
            .Where(x => x.PaymentMethodAccountId.HasValue)
            .Select(x => x.PaymentMethodAccountId!.Value)
            .Distinct()
            .ToList();

        var paymentMethodAccounts = accountIds.Count > 0
            ? _dbContext.MstPaymentMethodAccounts.AsNoTracking()
                .Where(x => accountIds.Contains(x.Id))
                .ToDictionary(x => x.Id)
            : new Dictionary<Guid, MstPaymentMethodAccount>();

        var cashierIds = account.Movements
            .Where(x => x.CreateBy != Guid.Empty)
            .Select(x => x.CreateBy)
            .Distinct()
            .ToList();

        var cashierNames = cashierIds.Count > 0
            ? _dbContext.Users.AsNoTracking()
                .Where(x => cashierIds.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x.DisplayName ?? x.UserName ?? "Kasir")
            : new Dictionary<Guid, string>();

        var reversedMovementIds = account.Movements
            .Where(x => !x.IsDelete && x.MovementType == BillingDepositMovementTypes.Reversal && x.ReversesMovementId.HasValue)
            .Select(x => x.ReversesMovementId!.Value)
            .ToHashSet();

        decimal runningBalance = 0;
        var movements = account.Movements
            .Where(x => !x.IsDelete)
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.CreateDateTime)
            .ThenBy(x => x.Id)
            .Select(x =>
            {
                // BKC-DEC-134 / BKC-DES-056:
                // TopUp selalu menambah saldo deposit (+Amount).
                // Mutasi RELEASE pembatalan alokasi (ReversesMovementId != null) memulihkan saldo deposit (+Amount).
                // Reversal, Allocation, dan Release refund murni (ReversesMovementId == null) mengurangi saldo deposit (-Amount).
                var effect = (x.MovementType == BillingDepositMovementTypes.TopUp ||
                              (x.MovementType == BillingDepositMovementTypes.Release && x.ReversesMovementId.HasValue))
                    ? x.Amount
                    : -x.Amount;
                runningBalance += effect;

                string? pmName = null;
                string? bankName = null;
                string? accNum = null;

                if (x.PaymentMethodAccountId.HasValue && paymentMethodAccounts.TryGetValue(x.PaymentMethodAccountId.Value, out var pma))
                {
                    bankName = pma.BankName;
                    accNum = pma.AccountNumber;
                }

                if (x.PaymentMethodId.HasValue && paymentMethods.TryGetValue(x.PaymentMethodId.Value, out var pm))
                {
                    pmName = pm.PaymentMethodName;
                    bankName ??= pm.BankName;
                    accNum ??= pm.BankAccountNumber;
                }
                else if (x.MovementType == BillingDepositMovementTypes.Allocation)
                {
                    pmName = "Deposit";
                }

                cashierNames.TryGetValue(x.CreateBy, out var cashierName);
                var movementStatus = reversedMovementIds.Contains(x.Id)
                    ? DepositMovementStatuses.Reversed
                    : DepositMovementStatuses.Posted;

                return new DepositMovementResponse
                {
                    Id = x.Id,
                    TransactionNumber = BuildTransactionNumber(x.Id),
                    MovementType = x.MovementType,
                    MovementTypeName = BuildMovementTypeName(x.MovementType, x.ReversesMovementId.HasValue, null),
                    Status = movementStatus,
                    StatusName = BuildMovementStatusName(movementStatus),
                    Amount = x.Amount,
                    BalanceEffect = effect,
                    BalanceAfter = runningBalance,
                    SettlementId = x.SettlementId,
                    PaymentMethodId = x.PaymentMethodId,
                    PaymentMethodAccountId = x.PaymentMethodAccountId,
                    PaymentMethodName = pmName,
                    BankName = bankName,
                    AccountNumber = accNum,
                    ReferenceNumber = x.ReferenceNumber ?? (x.SettlementId.HasValue ? $"INV-SETTLE-{x.SettlementId.Value:N}"[..15] : null),
                    CashierName = cashierName ?? "Kasir",
                    CashierShiftId = x.CashierShiftId,
                    CorrelationId = x.CorrelationId,
                    OccurredAt = x.OccurredAt,
                    Reason = x.Reason,
                    ReversesMovementId = x.ReversesMovementId
                };
            })
            .ToList();

        return new DepositResponse
        {
            Id = account.Id,
            EncounterId = account.EncounterId,
            AccountNumber = account.AccountNumber,
            AvailableBalance = account.AvailableBalance,
            Status = account.Status,
            RowVersion = account.RowVersion,
            Movements = movements
        };
    }

    public async Task<DepositRefundEligibilityResponse> GetDepositRefundEligibilityAsync(
        Guid encounterId,
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        if (encounterId == Guid.Empty)
            throw new BillingDepositValidationException("EncounterId wajib diisi.");
        if (invoiceId == Guid.Empty)
            throw new BillingDepositValidationException("InvoiceId wajib diisi.");

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kunjungan pasien tidak ditemukan.");

        var patient = await _dbContext.MstPatients.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == encounter.PatientId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Data pasien tidak ditemukan.");

        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");

        var response = new DepositRefundEligibilityResponse
        {
            EncounterId = encounterId,
            InvoiceId = invoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            PatientId = patient.Id,
            MedicalRecordNumber = patient.MedicalRecordNumber,
            PatientName = patient.FullName,
            IsEligible = false
        };

        if (invoice.EncounterId != encounterId)
        {
            response.IneligibilityReason = "Invoice tidak terhubung dengan kunjungan ini.";
            return response;
        }

        if (!string.Equals(invoice.ServiceType, "RANAP", StringComparison.OrdinalIgnoreCase))
        {
            response.IneligibilityReason = "Refund sisa deposit hanya berlaku untuk pasien rawat inap (RANAP).";
            return response;
        }

        var account = await _dbContext.BilDepositAccounts.AsNoTracking()
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

        if (account == null)
        {
            response.IneligibilityReason = "Akun deposit rawat inap belum dibuat untuk kunjungan ini.";
            return response;
        }

        response.DepositRowVersion = account.RowVersion;
        response.DepositAvailableBalance = account.AvailableBalance;

        var activeMovements = account.Movements.Where(x => !x.IsDelete).ToList();
        response.TotalDepositReceived = activeMovements
            .Where(x => x.MovementType == BillingDepositMovementTypes.TopUp)
            .Sum(x => x.Amount);

        var allocations = await _dbContext.BilPaymentAllocations.AsNoTracking()
            .Include(x => x.Settlement)
            .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice
                && x.TargetId == invoiceId
                && !x.IsDelete
                && x.ReversesAllocationId == null)
            .ToListAsync(cancellationToken);

        var depositAllocated = allocations
            .Where(x => x.Settlement != null && x.Settlement.Purpose == BillingSettlementPurposes.InvoicePayment)
            .Sum(x => x.Amount);

        var movementAllocated = activeMovements
            .Where(x => x.MovementType == BillingDepositMovementTypes.Allocation)
            .Sum(x => x.Amount);

        var totalAllocated = Math.Max(depositAllocated, movementAllocated);
        response.TotalDepositAllocated = totalAllocated;

        if (totalAllocated <= 0)
        {
            response.IneligibilityReason = "Deposit belum digunakan untuk membayar tagihan. Pilihan pengajuan hanya terbuka setelah deposit berhasil digunakan membayar tagihan rawat inap.";
            return response;
        }

        var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
            .OrderByDescending(x => x.VersionNo)
            .FirstOrDefaultAsync(cancellationToken);

        decimal outstanding;
        if (calculation != null)
        {
            var closureService = new BillingInvoiceClosureService(_dbContext);
            outstanding = await closureService.CalculateOutstandingAsync(invoice, calculation, cancellationToken);
        }
        else
        {
            var itemsTotal = await _dbContext.BilInvoiceItems.AsNoTracking()
                .Where(x => x.InvoiceId == invoiceId && x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
                .SumAsync(x => x.Quantity * x.UnitPrice, cancellationToken);
            var netAllocated = allocations.Sum(x => x.Amount);
            outstanding = Math.Max(0m, itemsTotal - netAllocated);
        }

        if (outstanding > 0)
        {
            response.IneligibilityReason = $"Tagihan pasien belum lunas (sisa tagihan Rp {outstanding:N2}). Sisa deposit hanya dapat direfund setelah tagihan terselesaikan.";
            return response;
        }

        if (account.AvailableBalance <= 0)
        {
            response.IneligibilityReason = "Tidak ada sisa saldo deposit yang tersedia untuk direfund.";
            return response;
        }

        var activeRequests = await _dbContext.FinPatientRefundRequests.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId
                && !x.IsDelete
                && (x.Status == FinPatientRefundStatuses.Pending
                    || x.Status == FinPatientRefundStatuses.Approved
                    || x.Status == FinPatientRefundStatuses.PartiallyPaid))
            .ToListAsync(cancellationToken);

        var reserved = activeRequests.Sum(x => (x.ApprovedAmount ?? x.RequestedAmount) - x.PaidAmount);
        response.ReservedRefundAmount = reserved;

        var maxRefundable = Math.Max(0m, account.AvailableBalance - reserved);
        response.MaxRefundableAmount = maxRefundable;

        if (maxRefundable <= 0)
        {
            response.IneligibilityReason = "Seluruh sisa saldo deposit telah dicadangkan pada antrean pengajuan refund Finance yang sedang berjalan.";
            return response;
        }

        var activeBillingRefund = await _dbContext.BilRefundCases.AsNoTracking()
            .AnyAsync(x => !x.IsDelete
                && x.InvoiceId == invoiceId
                && x.RefundCategory == "DEPOSITO"
                && x.Status != BillingRefundCaseStatuses.Rejected
                && x.Status != BillingRefundCaseStatuses.Executed,
                cancellationToken);

        if (activeBillingRefund)
        {
            response.IneligibilityReason = "Invoice ini masih memiliki kasus refund deposito aktif di loket kasir.";
            return response;
        }

        response.IsEligible = true;
        return response;
    }

    public async Task<DepositRefundResponse> CreateDepositRefundRequestAsync(
        Guid encounterId,
        CreateDepositRefundRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (encounterId == Guid.Empty)
            throw new BillingDepositValidationException("EncounterId wajib diisi.");
        if (request.InvoiceId == Guid.Empty)
            throw new BillingDepositValidationException("InvoiceId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingDepositValidationException("IdempotencyKey wajib diisi.");
        if (request.RequestedAmount <= 0)
            throw new BillingDepositValidationException("Nominal pengajuan refund harus lebih besar dari nol.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BillingDepositValidationException("Alasan pengajuan refund wajib diisi.");

        // Pengembalian sisa deposit selalu Transfer Bank; aturannya sama dengan sisi Finance
        // (PatientRefundTransferPolicy) supaya kedua pintu pengajuan tidak berbeda pendapat.
        if (!PatientRefundTransferPolicy.TryNormalizeMethod(request.PaymentMethod, out var method))
            throw new BillingDepositValidationException(PatientRefundTransferPolicy.TransferOnlyMessage);

        if (!request.BankAccountId.HasValue || request.BankAccountId.Value == Guid.Empty)
            throw new BillingDepositValidationException("Rekening pengirim wajib dipilih dari master rekening bank rumah sakit.");
        if (string.IsNullOrWhiteSpace(request.PayeeBankName))
            throw new BillingDepositValidationException("Nama bank tujuan pasien wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.PayeeAccountNumber))
            throw new BillingDepositValidationException("Nomor rekening tujuan pasien wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.PayeeAccountName))
            throw new BillingDepositValidationException("Nama penerima (pemilik rekening tujuan) wajib diisi.");

        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_DEPOSIT_{encounterId:N}", cancellationToken);
                await AcquireLockAsync($"BIL_INVOICE_{request.InvoiceId:N}", cancellationToken);
            }

            var priorRequest = await _dbContext.FinPatientRefundRequests.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey && !x.IsDelete, cancellationToken);

            if (priorRequest != null)
            {
                var priorInvoice = await _dbContext.BilInvoices.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == priorRequest.InvoiceId, cancellationToken);
                var priorPatient = await _dbContext.MstPatients.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == priorRequest.PatientId, cancellationToken);
                var priorAccount = await _dbContext.BilDepositAccounts.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

                if (transaction != null) await transaction.CommitAsync(cancellationToken);

                return new DepositRefundResponse
                {
                    RefundRequestId = priorRequest.Id,
                    RefundableCreditId = priorRequest.RefundableCreditId,
                    EncounterId = encounterId,
                    InvoiceId = priorRequest.InvoiceId,
                    InvoiceNumber = priorInvoice?.InvoiceNumber ?? string.Empty,
                    MedicalRecordNumber = priorPatient?.MedicalRecordNumber ?? string.Empty,
                    PatientName = priorPatient?.FullName ?? string.Empty,
                    RequestedAmount = priorRequest.RequestedAmount,
                    Status = priorRequest.Status,
                    PaymentMethod = priorRequest.PaymentMethod ?? method,
                    PayeeBankName = priorRequest.PayeeBankName,
                    PayeeAccountNumber = priorRequest.PayeeAccountNumber,
                    PayeeAccountName = priorRequest.PayeeAccountName,
                    Reason = priorRequest.Reason,
                    RemainingDepositBalance = priorAccount?.AvailableBalance ?? 0m,
                    IsReplay = true,
                    SubmittedAt = priorRequest.SubmittedAt
                };
            }

            // Dilakukan SETELAH kiriman ulang (idempotency) dikembalikan di atas: pengajuan yang sudah
            // sah tidak boleh gagal hanya karena rekening kemudian dinonaktifkan.
            var source = await PatientRefundTransferPolicy.ResolveSourceAccountAsync(
                _dbContext, request.BankAccountId, request.SenderBankId, cancellationToken);
            if (!source.IsValid)
                throw new BillingDepositValidationException(source.Error!);

            var payeeError = PatientRefundTransferPolicy.ValidatePayee(
                source.Account!, request.PayeeBankName, request.PayeeAccountNumber, request.PayeeAccountName);
            if (payeeError is not null)
                throw new BillingDepositValidationException(payeeError);

            var eligibility = await GetDepositRefundEligibilityAsync(encounterId, request.InvoiceId, cancellationToken);
            if (!eligibility.IsEligible)
            {
                throw new BillingDepositValidationException(eligibility.IneligibilityReason ?? "Deposit tidak memenuhi syarat untuk diajukan refund.");
            }

            if (request.RequestedAmount > eligibility.MaxRefundableAmount)
            {
                throw new BillingDepositValidationException(
                    $"Nominal pengajuan (Rp {request.RequestedAmount:N2}) melebihi maksimum saldo yang dapat direfund (Rp {eligibility.MaxRefundableAmount:N2}).");
            }

            var account = await _dbContext.BilDepositAccounts
                .SingleOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Akun deposit rawat inap tidak ditemukan.");

            if (request.ExpectedDepositRowVersion != Guid.Empty && account.RowVersion != request.ExpectedDepositRowVersion)
            {
                throw new BillingDepositConflictException("Data saldo deposit telah berubah. Muat ulang sebelum melanjutkan.");
            }

            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == request.InvoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");

            var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Kunjungan tidak ditemukan.");

            var patient = await _dbContext.MstPatients.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == encounter.PatientId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pasien tidak ditemukan.");

            var now = DateTimeOffset.UtcNow;
            var correlationId = request.CorrelationId != Guid.Empty ? request.CorrelationId : Guid.NewGuid();
            var causationId = request.CausationId != Guid.Empty ? request.CausationId : correlationId;

            account.AvailableBalance -= request.RequestedAmount;
            account.RowVersion = Guid.NewGuid();
            account.UpdateDateTime = DateTime.UtcNow;
            account.UpdateBy = actorUserId;

            var movement = new BilDepositMovement
            {
                DepositAccountId = account.Id,
                MovementType = BillingDepositMovementTypes.Release,
                Amount = request.RequestedAmount,
                Reason = $"Pencadangan pengajuan refund sisa deposit ke Finance: {request.Reason.Trim()}",
                OccurredAt = now,
                IdempotencyKey = idempotencyKey,
                CorrelationId = correlationId,
                CausationId = causationId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilDepositMovements.Add(movement);

            var credit = new BilRefundableCredit
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                SourceType = BillingRefundableCreditSourceTypes.Settlement,
                SourceId = movement.Id,
                OriginalAmount = request.RequestedAmount,
                AvailableAmount = request.RequestedAmount,
                Status = BillingRefundableCreditStatuses.Available,
                RecognizedAt = now,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilRefundableCredits.Add(credit);

            var refundRequest = new FinPatientRefundRequest
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                InvoiceId = invoice.Id,
                RefundableCreditId = credit.Id,
                RequestedAmount = request.RequestedAmount,
                ApprovedAmount = null,
                PaidAmount = 0m,
                Status = FinPatientRefundStatuses.Pending,
                Reason = request.Reason.Trim(),
                PaymentMethod = method,
                BankAccountId = source.Account!.BankAccountId,
                PayeeBankName = request.PayeeBankName?.Trim(),
                PayeeAccountNumber = request.PayeeAccountNumber?.Trim(),
                PayeeAccountName = request.PayeeAccountName?.Trim(),
                RequestedBy = actorUserId,
                SubmittedAt = now,
                IdempotencyKey = idempotencyKey,
                CorrelationId = correlationId,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.FinPatientRefundRequests.Add(refundRequest);

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "BillingDeposit.RefundRequested",
                "Kasir mengajukan refund sisa deposit rawat inap ke antrean persetujuan Finance.",
                new
                {
                    RefundRequestId = refundRequest.Id,
                    refundRequest.InvoiceId,
                    refundRequest.PatientId,
                    refundRequest.RequestedAmount,
                    RemainingBalance = account.AvailableBalance,
                    refundRequest.PaymentMethod,
                    ActorUserId = actorUserId
                });

            return new DepositRefundResponse
            {
                RefundRequestId = refundRequest.Id,
                RefundableCreditId = credit.Id,
                EncounterId = encounterId,
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                MedicalRecordNumber = patient.MedicalRecordNumber,
                PatientName = patient.FullName,
                RequestedAmount = refundRequest.RequestedAmount,
                Status = refundRequest.Status,
                PaymentMethod = method,
                PayeeBankName = refundRequest.PayeeBankName,
                PayeeAccountNumber = refundRequest.PayeeAccountNumber,
                PayeeAccountName = refundRequest.PayeeAccountName,
                Reason = refundRequest.Reason,
                RemainingDepositBalance = account.AvailableBalance,
                IsReplay = false,
                SubmittedAt = refundRequest.SubmittedAt
            };
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction != null) await transaction.DisposeAsync();
        }
    }

}

public sealed class BillingDepositValidationException(string message) : Exception(message);

public sealed class BillingDepositConflictException : Exception
{
    public BillingDepositConflictException(string message) : base(message) { }
    public BillingDepositConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
