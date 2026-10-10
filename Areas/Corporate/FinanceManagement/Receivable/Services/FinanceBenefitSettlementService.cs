using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Data;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// BE-FIN-099, FIN-DES-102, FIN-DEC-177/178. Penutupan berkala piutang porsi benefit yang
/// ditanggung rumah sakit (DebtorType = PAYER atas penjamin internal "RS Benefit") — hitung awal
/// non-mutasi, draf, penerbitan atomic (seluruh kartu tertutup atau nol sama sekali), pembatalan
/// dengan mutasi pembalik. BUKAN FinReceipt (nol uang diterima) dan BUKAN penghapusan buku
/// (FIN-DEC-178) — satu-satunya jalur penutupan adalah mutasi PELUNASAN-INTERNAL.
/// </summary>
public sealed class FinanceBenefitSettlementService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.BenefitSettlement";
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceSubledgerMovementService _subledgerMovementService;

    public FinanceBenefitSettlementService(ApplicationDbContext dbContext, FinanceSubledgerMovementService subledgerMovementService)
    {
        _dbContext = dbContext;
        _subledgerMovementService = subledgerMovementService;
    }

    // ------------------------------------------------------------------------------------
    // Hitung awal — murni baca, nol transaksi, nol perubahan data (L.5.1, L.5.2, L.5.13).
    // ------------------------------------------------------------------------------------

    public async Task<BenefitSettlementPreviewResponse> PreviewAsync(PreviewBenefitSettlementRequest request, CancellationToken cancellationToken)
    {
        var debtorName = await GetDebtorNameAsync(request.DebtorReferenceId, cancellationToken);
        var items = await GetEligibleItemsAsync(request.AccountingPeriodCode, request.DebtorReferenceId, cancellationToken);

        return new BenefitSettlementPreviewResponse
        {
            AccountingPeriodCode = request.AccountingPeriodCode,
            DebtorReferenceId = request.DebtorReferenceId,
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
        var items = await GetEligibleItemsAsync(request.AccountingPeriodCode, request.DebtorReferenceId, cancellationToken);
        if (items.Count == 0)
            throw new BenefitSettlementValidationException(
                $"Tidak ada piutang porsi benefit yang perlu ditutup untuk periode {request.AccountingPeriodCode}."); // FIN-VAL-242

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_BENEFIT_SETTLEMENT_{request.AccountingPeriodCode}_{request.DebtorReferenceId:N}", cancellationToken);

            // FIN-VAL-241 lapis service — lapis keras IX_FinBenefitSettlement_Period_Debtor_NotCancelled.
            var hasActive = await _dbContext.FinBenefitSettlements.AsNoTracking().AnyAsync(
                x => !x.IsDelete && x.AccountingPeriodCode == request.AccountingPeriodCode
                    && x.DebtorReferenceId == request.DebtorReferenceId && x.Status != FinBenefitSettlementStatuses.Dibatalkan,
                cancellationToken);
            if (hasActive)
            {
                var existingNumber = await _dbContext.FinBenefitSettlements.AsNoTracking()
                    .Where(x => !x.IsDelete && x.AccountingPeriodCode == request.AccountingPeriodCode
                        && x.DebtorReferenceId == request.DebtorReferenceId && x.Status != FinBenefitSettlementStatuses.Dibatalkan)
                    .Select(x => x.SettlementNumber).FirstOrDefaultAsync(cancellationToken);
                throw new BenefitSettlementConflictException(
                    $"Periode {request.AccountingPeriodCode} untuk penjamin ini sudah pernah ditutup pada {existingNumber}.");
            }

            var now = DateTimeOffset.UtcNow;
            var settlement = new FinBenefitSettlement
            {
                SettlementNumber = GenerateSettlementNumber(),
                AccountingPeriodCode = request.AccountingPeriodCode,
                DebtorReferenceId = request.DebtorReferenceId,
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
                    $"Periode {request.AccountingPeriodCode} untuk penjamin ini sudah pernah ditutup.", exception);
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

    private async Task<List<BenefitSettlementItemResponse>> GetEligibleItemsAsync(
        string periodCode, Guid debtorReferenceId, CancellationToken cancellationToken)
    {
        var (periodStart, periodEnd) = ParsePeriod(periodCode);

        // FIN-VAL-243: piutang yang sudah ditutup pelunasan lain (TIDAK dibatalkan) dikeluarkan.
        // Catatan (BE-FIN-092 §3): tidak dapat ditegakkan DB constraint lintas tabel — murni
        // lapis service, SATU-SATUNYA penegak invarian ini.
        var closedReceivableIds = _dbContext.FinBenefitSettlementItems.AsNoTracking()
            .Where(i => i.Settlement!.Status != FinBenefitSettlementStatuses.Dibatalkan)
            .Select(i => i.ReceivableId);

        var query =
            from receivable in _dbContext.FinReceivables.AsNoTracking()
            where !receivable.IsDelete
                && receivable.DebtorType == FinReceivableDebtorTypes.Payer
                && receivable.DebtorReferenceId == debtorReferenceId
                && (receivable.Status == FinReceivableStatuses.Outstanding || receivable.Status == FinReceivableStatuses.Partial)
                && receivable.RecognizedAt >= periodStart && receivable.RecognizedAt < periodEnd
                && !closedReceivableIds.Contains(receivable.Id)
            select receivable;

        var receivables = await query.ToListAsync(cancellationToken);
        if (receivables.Count == 0) return [];

        var receivableIds = receivables.Select(x => x.Id).ToList();
        var invoiceIds = receivables.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value).ToList();
        var invoiceNumbers = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => invoiceIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.InvoiceNumber, cancellationToken);
        var patientNames = await (
            from item in _dbContext.FinReceivableItems.AsNoTracking()
            join patient in _dbContext.MstPatients.AsNoTracking() on item.PatientId equals patient.Id
            where receivableIds.Contains(item.ReceivableId) && item.PatientId.HasValue
            select new { item.ReceivableId, patient.FullName })
            .ToDictionaryAsync(x => x.ReceivableId, x => x.FullName, cancellationToken);

        return receivables.Select(r => new BenefitSettlementItemResponse
        {
            ReceivableId = r.Id,
            ReceivableNumber = r.ReceivableNumber,
            InvoiceNumber = r.InvoiceId.HasValue ? invoiceNumbers.GetValueOrDefault(r.InvoiceId.Value) : null,
            PatientName = patientNames.GetValueOrDefault(r.Id),
            Amount = r.OutstandingAmount
        }).ToList();
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
        var patientNames = await (
            from item in _dbContext.FinReceivableItems.AsNoTracking()
            join patient in _dbContext.MstPatients.AsNoTracking() on item.PatientId equals patient.Id
            where receivableIds.Contains(item.ReceivableId) && item.PatientId.HasValue
            select new { item.ReceivableId, patient.FullName })
            .ToDictionaryAsync(x => x.ReceivableId, x => x.FullName, cancellationToken);

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
            Items = settlement.Items.Select(i => new BenefitSettlementItemResponse
            {
                ReceivableId = i.ReceivableId,
                ReceivableNumber = receivableNumbers.GetValueOrDefault(i.ReceivableId),
                InvoiceNumber = invoiceByReceivable.TryGetValue(i.ReceivableId, out var invId) ? invoiceNumbers.GetValueOrDefault(invId) : null,
                PatientName = patientNames.GetValueOrDefault(i.ReceivableId),
                Amount = i.Amount
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
