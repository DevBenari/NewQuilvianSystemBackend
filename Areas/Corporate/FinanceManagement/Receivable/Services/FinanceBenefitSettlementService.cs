using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Data;
using System.Globalization;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// BE-FIN-099, FIN-DES-102, FIN-DEC-177/178. Penutupan berkala piutang porsi benefit yang
/// ditanggung rumah sakit (DebtorType = PAYER atas penjamin internal "RS Benefit") — hitung awal
/// non-mutasi, draf, penerbitan atomic (seluruh kartu tertutup atau nol sama sekali), pembatalan
/// dengan mutasi pembalik. BUKAN FinReceipt (nol uang diterima) dan BUKAN penghapusan buku
/// (FIN-DEC-178) — satu-satunya jalur penutupan adalah mutasi PELUNASAN-INTERNAL.
/// Plafon benefit pegawai dipotong di HR saat Finance menerbitkan (PostAsync) persetujuan penutupan benefit.
/// </summary>
public sealed class FinanceBenefitSettlementService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.BenefitSettlement";

    private sealed class BenefitSettlementScope
    {
        public string? AccountingPeriodCode { get; set; }
        public Guid? DebtorReferenceId { get; set; }
        public Guid? PatientId { get; set; }
        public string? PatientType { get; set; }
        public string? ResolutionStatus { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Period { get; set; }
        public string? Search { get; set; }
        public List<Guid>? ReceivableIds { get; set; }
    }
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceSubledgerMovementService _subledgerMovementService;
    private readonly EmployeeBenefitBalanceService? _balanceService;

    public FinanceBenefitSettlementService(
        ApplicationDbContext dbContext,
        FinanceSubledgerMovementService subledgerMovementService,
        EmployeeBenefitBalanceService? balanceService = null)
    {
        _dbContext = dbContext;
        _subledgerMovementService = subledgerMovementService;
        _balanceService = balanceService;
    }

    // ------------------------------------------------------------------------------------
    // Hitung awal — murni baca, nol transaksi, nol perubahan data (L.5.1, L.5.2, L.5.13).
    // ------------------------------------------------------------------------------------

    public async Task<BenefitSettlementPreviewResponse> PreviewAsync(PreviewBenefitSettlementRequest request, CancellationToken cancellationToken)
    {
        var scope = new BenefitSettlementScope
        {
            AccountingPeriodCode = request.AccountingPeriodCode,
            DebtorReferenceId = request.DebtorReferenceId,
            PatientId = request.PatientId,
            PatientType = request.PatientType,
            ResolutionStatus = request.ResolutionStatus,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Period = request.Period,
            Search = request.Search,
            ReceivableIds = request.ReceivableIds
        };

        var (items, periodCode, debtorReferenceId) = await GetEligibleItemsAsync(scope, cancellationToken);
        var debtorName = debtorReferenceId.HasValue ? await GetDebtorNameAsync(debtorReferenceId.Value, cancellationToken) : null;

        return new BenefitSettlementPreviewResponse
        {
            AccountingPeriodCode = periodCode,
            DebtorReferenceId = debtorReferenceId ?? Guid.Empty,
            DebtorName = debtorName,
            TotalAmount = items.Sum(x => x.Amount),
            ItemCount = items.Count,
            Items = items
        };
    }

    // ------------------------------------------------------------------------------------
    // Buat draf — snapshot piutang layak tutup saat ini. Saldo piutang BELUM bergerak (L.5.3).
    // ------------------------------------------------------------------------------------

    public async Task<BenefitSettlementDetailResponse> CreateAsync(
        CreateBenefitSettlementRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var scope = new BenefitSettlementScope
        {
            AccountingPeriodCode = request.AccountingPeriodCode,
            DebtorReferenceId = request.DebtorReferenceId,
            PatientId = request.PatientId,
            PatientType = request.PatientType,
            ResolutionStatus = request.ResolutionStatus,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Period = request.Period,
            Search = request.Search,
            ReceivableIds = request.ReceivableIds
        };

        var (items, periodCode, debtorReferenceId) = await GetEligibleItemsAsync(scope, cancellationToken);
        if (items.Count == 0)
            throw new BenefitSettlementValidationException(
                $"Tidak ada piutang porsi benefit yang perlu ditutup untuk kriteria/periode ini."); // FIN-VAL-242

        if (!debtorReferenceId.HasValue || debtorReferenceId.Value == Guid.Empty)
            throw new BenefitSettlementValidationException("Penjamin internal untuk penutupan benefit tidak dapat ditentukan.");

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BENEFIT_SETTLEMENT_{periodCode}_{debtorReferenceId.Value:N}", cancellationToken);

            // FIN-VAL-241 lapis service — lapis keras IX_FinBenefitSettlement_Period_Debtor_NotCancelled.
            var hasActive = await _dbContext.FinBenefitSettlements.AsNoTracking().AnyAsync(
                x => !x.IsDelete && x.AccountingPeriodCode == periodCode
                    && x.DebtorReferenceId == debtorReferenceId.Value && x.Status != FinBenefitSettlementStatuses.Dibatalkan,
                cancellationToken);
            if (hasActive)
            {
                var existingNumber = await _dbContext.FinBenefitSettlements.AsNoTracking()
                    .Where(x => !x.IsDelete && x.AccountingPeriodCode == periodCode
                        && x.DebtorReferenceId == debtorReferenceId.Value && x.Status != FinBenefitSettlementStatuses.Dibatalkan)
                    .Select(x => x.SettlementNumber).FirstOrDefaultAsync(cancellationToken);
                throw new BenefitSettlementConflictException(
                    $"Periode {periodCode} untuk penjamin ini sudah pernah ditutup pada {existingNumber}.");
            }

            var now = DateTimeOffset.UtcNow;
            var settlement = new FinBenefitSettlement
            {
                SettlementNumber = GenerateSettlementNumber(),
                AccountingPeriodCode = periodCode,
                DebtorReferenceId = debtorReferenceId.Value,
                TotalAmount = items.Sum(x => x.Amount),
                ItemCount = items.Count,
                Status = FinBenefitSettlementStatuses.Draf,
                Notes = request.Notes,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            foreach (var item in items)
            {
                settlement.Items.Add(new FinBenefitSettlementItem
                {
                    SettlementId = settlement.Id,
                    ReceivableId = item.ReceivableId,
                    Amount = item.Amount,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }
            _dbContext.FinBenefitSettlements.Add(settlement);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                throw new BenefitSettlementConflictException(
                    $"Periode {periodCode} untuk penjamin ini sudah pernah ditutup.", exception);
            }

            await CommitAsync(transaction, cancellationToken);
            return await MapDetailAsync(settlement.Id, cancellationToken);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Terbitkan — atomic: seluruh kartu ditutup atau nol sama sekali (L.5.4, L.5.5).
    // ------------------------------------------------------------------------------------

    public async Task<BenefitSettlementDetailResponse> PostAsync(
        Guid id, PostBenefitSettlementRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BENEFIT_SETTLEMENT_{id:N}", cancellationToken);

            var settlement = await _dbContext.FinBenefitSettlements
                .Include(x => x.Items)
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pelunasan internal tidak ditemukan.");

            if (settlement.Status != FinBenefitSettlementStatuses.Draf)
                throw new BenefitSettlementConflictException(
                    $"Pelunasan berstatus {settlement.Status} tidak dapat diterbitkan. Hanya DRAF yang dapat diterbitkan.");
            if (settlement.RowVersion != request.RowVersion)
                throw new BenefitSettlementConflictException("Data pelunasan sudah berubah sejak terakhir dibaca. Muat ulang sebelum mencoba lagi.");

            if (settlement.Items.Count == 0)
                throw new BenefitSettlementValidationException(
                    $"Tidak ada piutang porsi benefit yang perlu ditutup untuk periode {settlement.AccountingPeriodCode}."); // FIN-VAL-242

            // FIN-VAL-243, L.5.5: validasi ULANG seluruh baris sebelum menyentuh satu pun — bila
            // satu saja sudah tidak sah, NOL kartu ditutup (atomic, bukan "dua dari tiga").
            var receivableIds = settlement.Items.Select(x => x.ReceivableId).ToList();
            var receivables = await _dbContext.FinReceivables
                .Where(x => receivableIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            foreach (var item in settlement.Items)
            {
                if (!receivables.TryGetValue(item.ReceivableId, out var receivable) || receivable.IsDelete)
                    throw new BenefitSettlementConflictException($"Piutang pada daftar tidak lagi ditemukan — nol kartu diterbitkan.");
                if (receivable.Status is not (FinReceivableStatuses.Outstanding or FinReceivableStatuses.Partial))
                    throw new BenefitSettlementConflictException(
                        $"Piutang {receivable.ReceivableNumber} sudah ditutup atau sudah lunas, sehingga dikeluarkan dari daftar — nol kartu diterbitkan.");
            }

            // FIN-DEC-202 / Persetujuan Finance memotong plafon benefit di HR secara atomic.
            // Plafon dipotong sebesar nominal kartu piutang yang disetujui untuk ditutup.
            // Bila salah satu gagal (saldo tidak cukup, tidak aktif, atau konflik), transaksi dibatalkan (nol kartu ditutup).
            if (_balanceService != null)
            {
                foreach (var item in settlement.Items)
                {
                    var receivable = receivables[item.ReceivableId];
                    if (item.Amount <= 0) continue;

                    var handoffKey = receivable.SourceHandoffKey ?? receivable.Id;
                    var invoiceId = receivable.InvoiceId ?? Guid.Empty;

                    var (personKind, personId) = await ResolveBenefitPersonAsync(receivable, cancellationToken);
                    if (string.IsNullOrWhiteSpace(personKind) || !personId.HasValue)
                    {
                        throw new BenefitSettlementValidationException(
                            $"Identitas kepesertaan benefit pegawai tidak lengkap untuk pemotongan plafon pada piutang {receivable.ReceivableNumber}.");
                    }

                    var useResult = await _balanceService.UseAsync(new BenefitBalanceUseRequest
                    {
                        SourceHandoffKey = handoffKey,
                        InvoiceId = invoiceId,
                        BenefitPersonKind = personKind,
                        BenefitPersonId = personId.Value,
                        Amount = item.Amount,
                        ActorUserId = actorUserId
                    }, cancellationToken);

                    if (string.Equals(useResult.Result, BenefitUseResults.Insufficient, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BenefitSettlementValidationException(
                            useResult.Message ?? $"Sisa plafon tidak cukup untuk {receivable.ReceivableNumber}. Sisa saat ini Rp {useResult.RemainingAmount:N0}.");
                    }

                    if (string.Equals(useResult.Result, BenefitUseResults.KeyConflict, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BenefitSettlementConflictException(
                            useResult.Message ?? $"Konflik kunci serah terima saat pemotongan plafon benefit untuk {receivable.ReceivableNumber}.");
                    }

                    if (!string.Equals(useResult.Result, BenefitUseResults.Applied, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(useResult.Result, BenefitUseResults.AlreadyApplied, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BenefitSettlementValidationException(
                            useResult.Message ?? $"Gagal memotong plafon benefit pegawai untuk {receivable.ReceivableNumber}: {useResult.Result}.");
                    }
                }
            }

            var now = DateTimeOffset.UtcNow;
            foreach (var item in settlement.Items)
            {
                var receivable = receivables[item.ReceivableId];
                var balanceBefore = receivable.OutstandingAmount;
                receivable.OutstandingAmount = 0m;
                receivable.AllocatedAmount += balanceBefore; // CK_FinReceivable_Balance
                receivable.Status = FinReceivableStatuses.Settled;
                receivable.UpdateDateTime = DateTime.UtcNow;
                receivable.UpdateBy = actorUserId;
                receivable.RowVersion = Guid.NewGuid();

                if (balanceBefore != 0)
                {
                    await _subledgerMovementService.RecordReceivableMovementAsync(
                        receivable: receivable,
                        movementType: FinReceivableMovementTypes.PelunasanInternal,
                        deltaAmount: -balanceBefore,
                        balanceBefore: balanceBefore,
                        occurredAt: now,
                        actorUserId: actorUserId,
                        correlationId: settlement.Id,
                        causationId: item.Id,
                        referenceNumber: settlement.SettlementNumber,
                        notes: $"Pelunasan internal porsi benefit — {settlement.SettlementNumber} periode {settlement.AccountingPeriodCode}",
                        cancellationToken: cancellationToken);
                }
            }

            settlement.Status = FinBenefitSettlementStatuses.Diterbitkan;
            settlement.PostedBy = actorUserId;
            settlement.PostedAt = now;
            if (!string.IsNullOrWhiteSpace(request.Notes)) settlement.Notes = request.Notes;
            settlement.UpdateDateTime = DateTime.UtcNow;
            settlement.UpdateBy = actorUserId;
            settlement.RowVersion = Guid.NewGuid();
            // AccountingEventId SENGAJA tetap kosong — kontraknya tertahan FIN-OQ-103 (L.5.12).
            // Penutupan piutang tetap sah tanpanya.

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            return await MapDetailAsync(settlement.Id, cancellationToken);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Batalkan — dari DRAF (nol akibat saldo, L.5.8) atau DITERBITKAN (mutasi pembalik, L.5.9).
    // ------------------------------------------------------------------------------------

    public async Task<BenefitSettlementResponse> CancelAsync(
        Guid id, CancelBenefitSettlementRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BENEFIT_SETTLEMENT_{id:N}", cancellationToken);

            var settlement = await _dbContext.FinBenefitSettlements
                .Include(x => x.Items)
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pelunasan internal tidak ditemukan.");

            if (settlement.Status is not (FinBenefitSettlementStatuses.Draf or FinBenefitSettlementStatuses.Diterbitkan))
                throw new BenefitSettlementConflictException($"Pelunasan berstatus {settlement.Status} tidak dapat dibatalkan.");
            if (settlement.RowVersion != request.RowVersion)
                throw new BenefitSettlementConflictException("Data pelunasan sudah berubah sejak terakhir dibaca. Muat ulang sebelum mencoba lagi.");

            var now = DateTimeOffset.UtcNow;

            if (settlement.Status == FinBenefitSettlementStatuses.Diterbitkan)
            {
                var receivableIds = settlement.Items.Select(x => x.ReceivableId).ToList();
                var receivables = await _dbContext.FinReceivables
                    .Where(x => receivableIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

                foreach (var item in settlement.Items)
                {
                    if (!receivables.TryGetValue(item.ReceivableId, out var receivable)) continue;

                    var balanceBefore = receivable.OutstandingAmount;
                    receivable.OutstandingAmount += item.Amount;
                    receivable.AllocatedAmount -= item.Amount; // pembalik CK_FinReceivable_Balance
                    receivable.Status = receivable.OutstandingAmount >= receivable.OriginalAmount
                        ? FinReceivableStatuses.Outstanding
                        : FinReceivableStatuses.Partial;
                    receivable.UpdateDateTime = DateTime.UtcNow;
                    receivable.UpdateBy = actorUserId;
                    receivable.RowVersion = Guid.NewGuid();

                    await _subledgerMovementService.RecordReceivableMovementAsync(
                        receivable: receivable,
                        movementType: FinReceivableMovementTypes.PelunasanInternal,
                        deltaAmount: item.Amount,
                        balanceBefore: balanceBefore,
                        occurredAt: now,
                        actorUserId: actorUserId,
                        correlationId: settlement.Id,
                        causationId: item.Id,
                        referenceNumber: settlement.SettlementNumber,
                        notes: $"Pembalik pelunasan internal — {settlement.SettlementNumber} dibatalkan: {request.CancelReason}",
                        cancellationToken: cancellationToken);

                    // Kembalikan pemotongan plafon benefit di HR secara simetris
                    if (_balanceService != null)
                    {
                        var handoffKey = receivable.SourceHandoffKey ?? receivable.Id;
                        await _balanceService.ReturnAsync(new BenefitBalanceReturnRequest
                        {
                            SourceHandoffKey = handoffKey,
                            Reason = $"Pembalik pelunasan benefit {settlement.SettlementNumber}: {request.CancelReason}",
                            ActorUserId = actorUserId
                        }, cancellationToken);
                    }
                }
            }

            settlement.Status = FinBenefitSettlementStatuses.Dibatalkan;
            settlement.CancelReason = request.CancelReason;
            settlement.UpdateDateTime = DateTime.UtcNow;
            settlement.UpdateBy = actorUserId;
            settlement.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            return await MapAsync(settlement, cancellationToken);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<BenefitSettlementListResponse>> GetPagedAsync(BenefitSettlementQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinBenefitSettlements.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(request.AccountingPeriodCode)) query = query.Where(x => x.AccountingPeriodCode == request.AccountingPeriodCode);
        if (request.DebtorReferenceId.HasValue) query = query.Where(x => x.DebtorReferenceId == request.DebtorReferenceId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "settlementnumber" => descending ? query.OrderByDescending(x => x.SettlementNumber) : query.OrderBy(x => x.SettlementNumber),
            _ => descending ? query.OrderByDescending(x => x.AccountingPeriodCode) : query.OrderBy(x => x.AccountingPeriodCode)
        };

        var total = await query.CountAsync(cancellationToken);
        var page = await query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        var names = await GetUserNamesAsync(page.Where(x => x.PostedBy.HasValue).Select(x => x.PostedBy!.Value), cancellationToken);
        var debtorNames = await GetDebtorNamesAsync(page.Select(x => x.DebtorReferenceId), cancellationToken);

        return new PagedResult<BenefitSettlementListResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = page.Select(x => MapList(x, names, debtorNames)).ToList()
        };
    }

    public Task<BenefitSettlementDetailResponse> GetDetailAsync(Guid id, CancellationToken cancellationToken) =>
        MapDetailAsync(id, cancellationToken);

    // ------------------------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------------------------

    private Task<(List<BenefitSettlementItemResponse> Items, string PeriodCode, Guid? DebtorReferenceId)> GetEligibleItemsAsync(
        string periodCode, Guid debtorReferenceId, CancellationToken cancellationToken) =>
        GetEligibleItemsAsync(new BenefitSettlementScope { AccountingPeriodCode = periodCode, DebtorReferenceId = debtorReferenceId }, cancellationToken);

    private async Task<(List<BenefitSettlementItemResponse> Items, string PeriodCode, Guid? DebtorReferenceId)> GetEligibleItemsAsync(
        BenefitSettlementScope scope, CancellationToken cancellationToken)
    {
        DateTimeOffset? rangeFrom = null;
        DateTimeOffset? rangeToExclusive = null;
        string? determinedPeriodCode = !string.IsNullOrWhiteSpace(scope.AccountingPeriodCode)
            ? scope.AccountingPeriodCode.Trim()
            : null;

        if (!string.IsNullOrWhiteSpace(scope.StartDate) && DateOnly.TryParse(scope.StartDate.Trim(), CultureInfo.InvariantCulture, out var parsedStart))
        {
            rangeFrom = FinanceBusinessDate.GetStartOfDayUtc(parsedStart);
            determinedPeriodCode ??= $"{parsedStart.Year:D4}-{parsedStart.Month:D2}";
        }

        if (!string.IsNullOrWhiteSpace(scope.EndDate) && DateOnly.TryParse(scope.EndDate.Trim(), CultureInfo.InvariantCulture, out var parsedEnd))
        {
            rangeToExclusive = FinanceBusinessDate.GetEndOfDayUtc(parsedEnd);
            determinedPeriodCode ??= $"{parsedEnd.Year:D4}-{parsedEnd.Month:D2}";
        }

        if (!rangeFrom.HasValue && !rangeToExclusive.HasValue && !string.IsNullOrWhiteSpace(determinedPeriodCode))
        {
            var (start, end) = ParsePeriod(determinedPeriodCode);
            rangeFrom = start;
            rangeToExclusive = end;
        }

        determinedPeriodCode ??= $"{DateTime.UtcNow.Year:D4}-{DateTime.UtcNow.Month:D2}";

        var closedReceivableIds = _dbContext.FinBenefitSettlementItems.AsNoTracking()
            .Where(i => !i.IsDelete && i.Settlement != null && !i.Settlement.IsDelete && i.Settlement.Status != FinBenefitSettlementStatuses.Dibatalkan)
            .Select(i => i.ReceivableId);

        var query =
            from item in _dbContext.FinReceivableItems.AsNoTracking()
            join receivable in _dbContext.FinReceivables.AsNoTracking()
                on item.ReceivableId equals receivable.Id
            join invoiceJoin in _dbContext.BilInvoices.AsNoTracking()
                on item.InvoiceId equals (Guid?)invoiceJoin.Id into invoiceGroup
            from invoice in invoiceGroup.DefaultIfEmpty()
            join encounterJoin in _dbContext.RegPatientEncounters.AsNoTracking()
                on item.EncounterId equals (Guid?)encounterJoin.Id into encounterGroup
            from encounter in encounterGroup.DefaultIfEmpty()
            join patientJoin in _dbContext.MstPatients.AsNoTracking()
                on item.PatientId equals (Guid?)patientJoin.Id into patientGroup
            from patient in patientGroup.DefaultIfEmpty()
            where !item.IsDelete
                && !receivable.IsDelete
                && receivable.DebtorType == FinReceivableDebtorTypes.Payer
                && receivable.IsInternalBenefit
                && (receivable.Status == FinReceivableStatuses.Outstanding || receivable.Status == FinReceivableStatuses.Partial)
                && !closedReceivableIds.Contains(receivable.Id)
            select new { item, receivable, invoice, encounter, patient };

        if (scope.DebtorReferenceId.HasValue && scope.DebtorReferenceId.Value != Guid.Empty)
        {
            query = query.Where(x => x.receivable.DebtorReferenceId == scope.DebtorReferenceId.Value);
        }

        if (scope.PatientId.HasValue && scope.PatientId.Value != Guid.Empty)
        {
            query = query.Where(x => x.item.PatientId == scope.PatientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(scope.PatientType) && !string.Equals(scope.PatientType, "all", StringComparison.OrdinalIgnoreCase))
        {
            var encType = scope.PatientType.Trim().ToLowerInvariant() switch
            {
                "outpatient" => EncounterType.Outpatient,
                "inpatient" => EncounterType.Inpatient,
                "emergency" => EncounterType.Emergency,
                _ => (EncounterType?)null
            };
            if (encType.HasValue)
            {
                query = query.Where(x => x.encounter != null && x.encounter.EncounterType == encType.Value);
            }
        }

        if (rangeFrom.HasValue)
        {
            var fromOffset = rangeFrom.Value;
            var fromUtc = fromOffset.UtcDateTime;
            query = query.Where(x =>
                (x.encounter != null && x.encounter.EncounterDate >= fromUtc)
                || (x.encounter == null && x.receivable.RecognizedAt >= fromOffset));
        }

        if (rangeToExclusive.HasValue)
        {
            var toOffset = rangeToExclusive.Value;
            var toUtc = toOffset.UtcDateTime;
            query = query.Where(x =>
                (x.encounter != null && x.encounter.EncounterDate < toUtc)
                || (x.encounter == null && x.receivable.RecognizedAt < toOffset));
        }

        if (!string.IsNullOrWhiteSpace(scope.Search))
        {
            var pattern = $"%{scope.Search.Trim()}%";
            query = query.Where(x =>
                (x.invoice != null && EF.Functions.ILike(x.invoice.InvoiceNumber, pattern, "\\"))
                || (x.encounter != null && EF.Functions.ILike(x.encounter.EncounterNumber, pattern, "\\"))
                || (x.patient != null && EF.Functions.ILike(x.patient.FullName, pattern, "\\"))
                || (x.patient != null && EF.Functions.ILike(x.patient.MedicalRecordNumber, pattern, "\\")));
        }

        if (scope.ReceivableIds != null && scope.ReceivableIds.Count > 0)
        {
            query = query.Where(x => scope.ReceivableIds.Contains(x.receivable.Id));
        }

        var rows = await query
            .Select(x => new
            {
                ReceivableId = x.receivable.Id,
                x.receivable.ReceivableNumber,
                x.receivable.OutstandingAmount,
                x.receivable.DebtorReferenceId,
                InvoiceNumber = x.invoice != null ? x.invoice.InvoiceNumber : null,
                PatientName = x.patient != null ? x.patient.FullName : null,
                MedicalRecordNumber = x.patient != null ? x.patient.MedicalRecordNumber : null,
                PatientType = x.encounter != null ? x.encounter.EncounterType.ToString() : null,
                TransactionDate = (DateTimeOffset?)(x.invoice != null && x.invoice.InvoiceDate.HasValue
                    ? x.invoice.InvoiceDate.Value
                    : (x.encounter != null ? new DateTimeOffset(x.encounter.EncounterDate, TimeSpan.Zero) : x.receivable.RecognizedAt))
            })
            .ToListAsync(cancellationToken);

        var distinctReceivables = rows
            .GroupBy(x => x.ReceivableId)
            .Select(g => g.First())
            .ToList();

        var resultItems = distinctReceivables.Select(r => new BenefitSettlementItemResponse
        {
            ReceivableId = r.ReceivableId,
            ReceivableNumber = r.ReceivableNumber,
            InvoiceNumber = r.InvoiceNumber,
            PatientName = r.PatientName,
            Amount = r.OutstandingAmount,
            MedicalRecordNumber = r.MedicalRecordNumber,
            PatientType = r.PatientType,
            TransactionDate = r.TransactionDate
        }).ToList();

        var debtorReferenceId = scope.DebtorReferenceId.HasValue && scope.DebtorReferenceId.Value != Guid.Empty
            ? scope.DebtorReferenceId.Value
            : (distinctReceivables.Count > 0 ? distinctReceivables.First().DebtorReferenceId : (Guid?)null);

        if (!debtorReferenceId.HasValue)
        {
            var defaultGuarantorId = await _dbContext.FinReceivables.AsNoTracking()
                .Where(r => !r.IsDelete && r.IsInternalBenefit && r.DebtorReferenceId.HasValue)
                .Select(r => r.DebtorReferenceId)
                .FirstOrDefaultAsync(cancellationToken);
            if (defaultGuarantorId.HasValue) debtorReferenceId = defaultGuarantorId.Value;
        }

        return (resultItems, determinedPeriodCode, debtorReferenceId);
    }

    private static (DateTimeOffset Start, DateTimeOffset End) ParsePeriod(string periodCode)
    {
        var year = int.Parse(periodCode[..4]);
        var month = int.Parse(periodCode[5..7]);
        var start = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        return (start, start.AddMonths(1));
    }

    private async Task<string?> GetDebtorNameAsync(Guid debtorReferenceId, CancellationToken cancellationToken) =>
        await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == debtorReferenceId).Select(x => x.CompanyGuarantorName).FirstOrDefaultAsync(cancellationToken);

    private async Task<Dictionary<Guid, string?>> GetDebtorNamesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var distinct = ids.Distinct().ToList();
        if (distinct.Count == 0) return new Dictionary<Guid, string?>();
        return await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => distinct.Contains(x.Id))
            .Select(x => new { x.Id, Name = (string?)x.CompanyGuarantorName })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    private async Task<Dictionary<Guid, string?>> GetUserNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string?>();
        return await _dbContext.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    private async Task<BenefitSettlementDetailResponse> MapDetailAsync(Guid settlementId, CancellationToken cancellationToken)
    {
        var settlement = await _dbContext.FinBenefitSettlements.AsNoTracking()
            .Include(x => x.Items)
            .SingleAsync(x => x.Id == settlementId, cancellationToken);

        var names = await GetUserNamesAsync(settlement.PostedBy.HasValue ? [settlement.PostedBy.Value] : [], cancellationToken);
        var debtorName = await GetDebtorNameAsync(settlement.DebtorReferenceId, cancellationToken);

        var receivableIds = settlement.Items.Select(x => x.ReceivableId).ToList();
        var receivableNumbers = await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => receivableIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.ReceivableNumber, cancellationToken);
        var invoiceIds = await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => receivableIds.Contains(x.Id) && x.InvoiceId.HasValue)
            .Select(x => new { x.Id, x.InvoiceId }).ToListAsync(cancellationToken);
        var invoiceByReceivable = invoiceIds.ToDictionary(x => x.Id, x => x.InvoiceId!.Value);
        var invoiceNumbers = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => invoiceByReceivable.Values.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.InvoiceNumber, cancellationToken);
        var patientDetails = await (
            from item in _dbContext.FinReceivableItems.AsNoTracking()
            join patient in _dbContext.MstPatients.AsNoTracking() on item.PatientId equals patient.Id into pGroup
            from patient in pGroup.DefaultIfEmpty()
            join enc in _dbContext.RegPatientEncounters.AsNoTracking() on item.EncounterId equals enc.Id into eGroup
            from enc in eGroup.DefaultIfEmpty()
            where receivableIds.Contains(item.ReceivableId)
            select new
            {
                item.ReceivableId,
                FullName = patient != null ? patient.FullName : null,
                MedicalRecordNumber = patient != null ? patient.MedicalRecordNumber : null,
                PatientType = enc != null ? enc.EncounterType.ToString() : null,
                EncounterDate = enc != null ? (DateTimeOffset?)new DateTimeOffset(enc.EncounterDate, TimeSpan.Zero) : null
            })
            .ToListAsync(cancellationToken);

        var patientDataByReceivable = patientDetails
            .GroupBy(x => x.ReceivableId)
            .ToDictionary(g => g.Key, g => g.First());

        return new BenefitSettlementDetailResponse
        {
            Id = settlement.Id,
            SettlementNumber = settlement.SettlementNumber,
            AccountingPeriodCode = settlement.AccountingPeriodCode,
            DebtorReferenceId = settlement.DebtorReferenceId,
            DebtorName = debtorName,
            TotalAmount = settlement.TotalAmount,
            ItemCount = settlement.ItemCount,
            Status = settlement.Status,
            PostedByName = settlement.PostedBy.HasValue ? names.GetValueOrDefault(settlement.PostedBy.Value) : null,
            PostedAt = settlement.PostedAt,
            CancelReason = settlement.CancelReason,
            AccountingEventId = settlement.AccountingEventId,
            Notes = settlement.Notes,
            RowVersion = settlement.RowVersion,
            Items = settlement.Items.Select(i =>
            {
                var pData = patientDataByReceivable.GetValueOrDefault(i.ReceivableId);
                return new BenefitSettlementItemResponse
                {
                    ReceivableId = i.ReceivableId,
                    ReceivableNumber = receivableNumbers.GetValueOrDefault(i.ReceivableId),
                    InvoiceNumber = invoiceByReceivable.TryGetValue(i.ReceivableId, out var invId) ? invoiceNumbers.GetValueOrDefault(invId) : null,
                    PatientName = pData?.FullName,
                    MedicalRecordNumber = pData?.MedicalRecordNumber,
                    PatientType = pData?.PatientType,
                    TransactionDate = pData?.EncounterDate,
                    Amount = i.Amount
                };
            }).ToList()
        };
    }

    private async Task<BenefitSettlementResponse> MapAsync(FinBenefitSettlement settlement, CancellationToken cancellationToken)
    {
        var names = await GetUserNamesAsync(settlement.PostedBy.HasValue ? [settlement.PostedBy.Value] : [], cancellationToken);
        var debtorName = await GetDebtorNameAsync(settlement.DebtorReferenceId, cancellationToken);
        return MapList(settlement, names, new Dictionary<Guid, string?> { [settlement.DebtorReferenceId] = debtorName });
    }

    private static BenefitSettlementListResponse MapList(
        FinBenefitSettlement x, Dictionary<Guid, string?> userNames, Dictionary<Guid, string?> debtorNames) => new()
    {
        Id = x.Id,
        SettlementNumber = x.SettlementNumber,
        AccountingPeriodCode = x.AccountingPeriodCode,
        DebtorReferenceId = x.DebtorReferenceId,
        DebtorName = debtorNames.GetValueOrDefault(x.DebtorReferenceId),
        TotalAmount = x.TotalAmount,
        ItemCount = x.ItemCount,
        Status = x.Status,
        PostedByName = x.PostedBy.HasValue ? userNames.GetValueOrDefault(x.PostedBy.Value) : null,
        PostedAt = x.PostedAt,
        CancelReason = x.CancelReason,
        AccountingEventId = x.AccountingEventId,
        Notes = x.Notes,
        RowVersion = x.RowVersion
    };

    private async Task<(string? Kind, Guid? Id)> ResolveBenefitPersonAsync(FinReceivable receivable, CancellationToken cancellationToken)
    {
        var handoffKey = receivable.SourceHandoffKey ?? receivable.Id;

        // 1. Cek riwayat mutasi USE yang sudah ada untuk kunci handoff ini (idempotensi / histori)
        var existingMovement = await _dbContext.Set<HrdBenefitBalanceMovement>().AsNoTracking()
            .Where(x => x.SourceHandoffKey == handoffKey && x.MovementType == BenefitMovementTypes.Use)
            .FirstOrDefaultAsync(cancellationToken);
        if (existingMovement != null)
        {
            var personId = existingMovement.BenefitPersonKind == BenefitPersonKinds.Enrollment
                ? existingMovement.EmployeeBenefitEnrollmentId
                : existingMovement.EmployeeBenefitDependentId ?? existingMovement.EmployeeBenefitEnrollmentId;
            return (existingMovement.BenefitPersonKind, personId);
        }

        // 2. Telusuri dari invoice asal -> encounter -> penjamin internal
        if (receivable.InvoiceId.HasValue && _balanceService != null)
        {
            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .Where(x => x.Id == receivable.InvoiceId.Value)
                .Select(x => new { x.Id, x.EncounterId, x.InvoiceDate })
                .FirstOrDefaultAsync(cancellationToken);

            if (invoice != null)
            {
                var guarantors = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                    .Include(x => x.CompanyGuarantor)
                    .Where(x => x.EncounterId == invoice.EncounterId && x.IsActive && !x.IsDelete && !x.IsCancel)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Priority)
                    .ToListAsync(cancellationToken);

                var benefitGuarantor = guarantors.FirstOrDefault(x =>
                    (x.CompanyGuarantor != null && x.CompanyGuarantor.IsEmployeeBenefitGuarantor)
                    || x.EmployeeIdSnapshot.HasValue
                    || x.WorkforceProfileIdSnapshot.HasValue);

                if (benefitGuarantor != null)
                {
                    var ownerProfileId = benefitGuarantor.WorkforceProfileIdSnapshot
                        ?? (benefitGuarantor.EmployeeIdSnapshot.HasValue
                            ? await _dbContext.MstEmployees.AsNoTracking()
                                .Where(x => x.Id == benefitGuarantor.EmployeeIdSnapshot.Value)
                                .Select(x => (Guid?)x.WorkforceProfileId)
                                .FirstOrDefaultAsync(cancellationToken)
                            : null);

                    var asOfDate = (invoice.InvoiceDate ?? receivable.RecognizedAt).UtcDateTime;
                    var balanceResult = await _balanceService.ReadAsync(new BenefitBalanceReadRequest
                    {
                        WorkforceProfileId = ownerProfileId,
                        EmployeeId = benefitGuarantor.EmployeeIdSnapshot ?? Guid.Empty,
                        FamilyMemberId = benefitGuarantor.FamilyMemberIdSnapshot,
                        AsOfDate = asOfDate
                    }, cancellationToken);

                    if (balanceResult.IsUsable && !string.IsNullOrWhiteSpace(balanceResult.BenefitPersonKind) && balanceResult.BenefitPersonId.HasValue)
                    {
                        return (balanceResult.BenefitPersonKind, balanceResult.BenefitPersonId);
                    }
                }
            }
        }

        // 3. Cadangan: dari metadata pemilik pada receivable
        if ((receivable.BenefitOwnerWorkforceProfileId.HasValue || receivable.BenefitOwnerId.HasValue) && _balanceService != null)
        {
            var asOfDate = receivable.RecognizedAt.UtcDateTime;
            var balanceResult = await _balanceService.ReadAsync(new BenefitBalanceReadRequest
            {
                WorkforceProfileId = receivable.BenefitOwnerWorkforceProfileId,
                EmployeeId = receivable.BenefitOwnerId ?? Guid.Empty,
                AsOfDate = asOfDate
            }, cancellationToken);

            if (balanceResult.IsUsable && !string.IsNullOrWhiteSpace(balanceResult.BenefitPersonKind) && balanceResult.BenefitPersonId.HasValue)
            {
                return (balanceResult.BenefitPersonKind, balanceResult.BenefitPersonId);
            }
        }

        return (null, null);
    }

    private static string GenerateSettlementNumber()
    {
        var candidate = $"PLB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);
}

public sealed class BenefitSettlementValidationException(string message) : Exception(message);
public sealed class BenefitSettlementConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
