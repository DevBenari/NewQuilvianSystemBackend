using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Riwayat pembayaran GABUNGAN satu kunjungan: tender pada seluruh invoice kunjungan, top-up deposit,
/// dan pemakaian deposit ke invoice. Seluruhnya baca saja; tidak ada angka baru yang dihitung selain
/// klasifikasi tender, semua nominal dibaca apa adanya dari ledger.
/// </summary>
/// <remarks>
/// Pemakaian deposit lahir sebagai <c>BilSettlement</c> berstatus <c>SETTLED</c> TANPA tender
/// (<see cref="BillingAllocationService"/>), sehingga daftar yang hanya membaca tender tidak pernah
/// menampilkannya. Karena itu pemakaian deposit dibaca dari mutasi deposit (<c>ALLOCATION</c>), dan
/// settlement tanpa tender tidak menghasilkan baris tender.
/// </remarks>
public sealed class BillingEncounterPaymentHistoryService
{
    private readonly ApplicationDbContext _dbContext;

    public BillingEncounterPaymentHistoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Mengembalikan <c>null</c> bila kunjungan tidak ditemukan.</summary>
    public async Task<EncounterPaymentHistoryResponse?> GetAsync(
        Guid encounterId,
        CancellationToken cancellationToken)
    {
        if (encounterId == Guid.Empty)
            throw new BillingInvoiceValidationException("EncounterId wajib diisi.");

        var encounterExists = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
            .AnyAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);
        if (!encounterExists) return null;

        var invoices = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .Select(x => new { x.Id, x.InvoiceNumber })
            .ToListAsync(cancellationToken);
        var invoiceNumberById = invoices.ToDictionary(x => x.Id, x => x.InvoiceNumber);
        var invoiceIds = invoiceNumberById.Keys.ToList();

        // Seluruh settlement invoice kunjungan ini. Tender dimuat bersamanya; baris yang dihapus lunak
        // disaring di memori (pola yang sama dengan BillingSettlementService.GetByInvoiceAsync).
        var settlements = invoiceIds.Count == 0
            ? new List<BilSettlement>()
            : await _dbContext.BilSettlements.AsNoTracking()
                .Include(x => x.Tenders)
                .Where(x => x.InvoiceId != null && invoiceIds.Contains(x.InvoiceId.Value) && !x.IsDelete)
                .ToListAsync(cancellationToken);
        var settlementById = settlements.ToDictionary(x => x.Id);

        var account = await _dbContext.BilDepositAccounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);

        var movements = account is null
            ? new List<BilDepositMovement>()
            : await _dbContext.BilDepositMovements.AsNoTracking()
                .Where(x => x.DepositAccountId == account.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

        // Ledger append-only: top-up yang dibalik (REVERSAL) dan alokasi yang dipulihkan (RELEASE yang
        // merujuk alokasinya) menandai mutasi asalnya sebagai REVERSED, bukan menghapusnya.
        var reversedMovementIds = movements
            .Where(x => (x.MovementType == BillingDepositMovementTypes.Reversal
                    || x.MovementType == BillingDepositMovementTypes.Release)
                && x.ReversesMovementId.HasValue)
            .Select(x => x.ReversesMovementId!.Value)
            .ToHashSet();

        var depositMovements = movements
            .Where(x => x.MovementType == BillingDepositMovementTypes.TopUp
                || x.MovementType == BillingDepositMovementTypes.Allocation)
            .ToList();

        var tenders = settlements
            .SelectMany(settlement => settlement.Tenders
                .Where(tender => !tender.IsDelete)
                .Select(tender => (Settlement: settlement, Tender: tender)))
            .ToList();

        // Jenis order asal top-up (INITIAL_DEPOSIT -> "Deposit Awal"); top-up tanpa order tetap "Top Up".
        var topUpIds = depositMovements
            .Where(x => x.MovementType == BillingDepositMovementTypes.TopUp)
            .Select(x => x.Id)
            .ToList();
        var orderTypeRows = await (from payment in _dbContext.BilDepositOrderPayments.AsNoTracking()
                                   join order in _dbContext.BilDepositPaymentOrders.AsNoTracking()
                                       on payment.DepositOrderId equals order.Id
                                   where topUpIds.Contains(payment.DepositMovementId)
                                       && !payment.IsDelete
                                       && payment.Status == DepositOrderPaymentStatuses.Success
                                   select new { payment.DepositMovementId, order.OrderType })
            .ToListAsync(cancellationToken);
        var orderTypeByMovement = orderTypeRows
            .GroupBy(x => x.DepositMovementId)
            .ToDictionary(g => g.Key, g => g.First().OrderType);

        // Metode pembayaran, rekening, dan nama kasir dimuat sekali untuk seluruh baris.
        var paymentMethodIds = tenders.Select(x => x.Tender.PaymentMethodId)
            .Concat(depositMovements.Where(x => x.PaymentMethodId.HasValue).Select(x => x.PaymentMethodId!.Value))
            .Distinct()
            .ToList();
        var paymentMethods = paymentMethodIds.Count == 0
            ? new Dictionary<Guid, MstPaymentMethod>()
            : await _dbContext.MstPaymentMethods.AsNoTracking()
                .Where(x => paymentMethodIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var accountIds = tenders.Where(x => x.Tender.PaymentMethodAccountId.HasValue)
            .Select(x => x.Tender.PaymentMethodAccountId!.Value)
            .Concat(depositMovements.Where(x => x.PaymentMethodAccountId.HasValue).Select(x => x.PaymentMethodAccountId!.Value))
            .Distinct()
            .ToList();
        var paymentMethodAccounts = accountIds.Count == 0
            ? new Dictionary<Guid, MstPaymentMethodAccount>()
            : await _dbContext.MstPaymentMethodAccounts.AsNoTracking()
                .Where(x => accountIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var cashierIds = tenders.Select(x => x.Tender.CreateBy)
            .Concat(depositMovements.Select(x => x.CreateBy))
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();
        var cashierNames = cashierIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.Users.AsNoTracking()
                .Where(x => cashierIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => !string.IsNullOrEmpty(x.DisplayName) ? x.DisplayName : (x.UserName ?? "Kasir"),
                    cancellationToken);

        string? CashierName(Guid userId) =>
            userId != Guid.Empty && cashierNames.TryGetValue(userId, out var name) ? name : null;

        string MethodName(Guid? methodId, string fallback) =>
            methodId.HasValue && paymentMethods.TryGetValue(methodId.Value, out var method)
                ? method.PaymentMethodName
                : fallback;

        string? Channel(Guid? methodId, Guid? accountId)
        {
            if (accountId.HasValue && paymentMethodAccounts.TryGetValue(accountId.Value, out var methodAccount))
                return FormatChannel(methodAccount.BankName, methodAccount.AccountNumber, methodAccount.AccountHolderName);

            if (methodId.HasValue && paymentMethods.TryGetValue(methodId.Value, out var method))
                return FormatChannel(method.BankName, method.BankAccountNumber, method.BankAccountName);

            return null;
        }

        var entries = new List<EncounterPaymentHistoryEntryResponse>();

        // --- Tender, dikelompokkan per settlement supaya klasifikasi memakai total berjalan settlement itu.
        foreach (var group in tenders.GroupBy(x => x.Settlement.Id))
        {
            var settlement = group.First().Settlement;
            var target = settlement.RequestedAmount;
            var roundedTarget = Math.Round(target, 0, MidpointRounding.AwayFromZero);
            var runningTotal = 0m;

            foreach (var (_, tender) in group.OrderBy(x => x.Tender.AttemptedAt).ThenBy(x => x.Tender.Id))
            {
                var succeeded = tender.Status == BillingTenderStatuses.Succeeded;
                if (succeeded) runningTotal += tender.Amount;

                var settling = succeeded && target > 0
                    && (runningTotal >= target
                        || (Math.Abs(runningTotal - roundedTarget) == 0 && Math.Abs(runningTotal - target) < 1.00m));

                entries.Add(new EncounterPaymentHistoryEntryResponse
                {
                    Id = tender.Id,
                    EntryType = EncounterPaymentHistoryEntryTypes.TenderPayment,
                    EntryTypeName = "Pembayaran",
                    OccurredAt = tender.AttemptedAt,
                    DocumentNumber = tender.KwitansiNumber ?? string.Empty,
                    HasKwitansi = !string.IsNullOrWhiteSpace(tender.KwitansiNumber),
                    InvoiceId = settlement.InvoiceId,
                    InvoiceNumber = settlement.InvoiceId.HasValue
                        && invoiceNumberById.TryGetValue(settlement.InvoiceId.Value, out var invoiceNumber)
                            ? invoiceNumber
                            : null,
                    SettlementId = settlement.Id,
                    PaymentMethodId = tender.PaymentMethodId,
                    PaymentMethodName = MethodName(tender.PaymentMethodId, "-"),
                    PaymentChannel = Channel(tender.PaymentMethodId, tender.PaymentMethodAccountId),
                    ProviderReferenceMasked = MaskReference(tender.ProviderReference),
                    Amount = tender.Amount,
                    Status = tender.Status,
                    StatusName = TenderStatusName(tender.Status),
                    Note = tender.CashierReferenceNote,
                    CashierName = CashierName(tender.CreateBy),
                    TransactionClassification = !succeeded ? "PENDING" : (settling ? "PELUNASAN" : "CICILAN"),
                    PostTransactionStatus = !succeeded ? "BELUM_LUNAS" : (settling ? "LUNAS" : "SEBAGIAN"),
                    RemainingOutstanding = settling ? 0m : Math.Max(target - runningTotal, 0m)
                });
            }
        }

        // --- Top-up dan pemakaian deposit.
        foreach (var movement in depositMovements)
        {
            var isTopUp = movement.MovementType == BillingDepositMovementTypes.TopUp;
            var reversed = reversedMovementIds.Contains(movement.Id);

            BilSettlement? settlement = null;
            if (!isTopUp && movement.SettlementId.HasValue)
                settlementById.TryGetValue(movement.SettlementId.Value, out settlement);

            var typeName = isTopUp
                ? (orderTypeByMovement.TryGetValue(movement.Id, out var orderType)
                    && orderType == DepositOrderTypes.InitialDeposit
                        ? "Deposit Awal"
                        : "Top Up Deposit")
                : "Pemakaian Deposit";

            entries.Add(new EncounterPaymentHistoryEntryResponse
            {
                Id = movement.Id,
                EntryType = isTopUp
                    ? EncounterPaymentHistoryEntryTypes.DepositTopUp
                    : EncounterPaymentHistoryEntryTypes.DepositAllocation,
                EntryTypeName = typeName,
                OccurredAt = movement.OccurredAt,
                DocumentNumber = BillingDepositService.BuildTransactionNumber(movement.Id),
                HasKwitansi = false,
                InvoiceId = settlement?.InvoiceId,
                InvoiceNumber = settlement?.InvoiceId is Guid invoiceId
                    && invoiceNumberById.TryGetValue(invoiceId, out var allocatedInvoiceNumber)
                        ? allocatedInvoiceNumber
                        : null,
                SettlementId = movement.SettlementId,
                PaymentMethodId = movement.PaymentMethodId,
                PaymentMethodName = isTopUp ? MethodName(movement.PaymentMethodId, "-") : "Deposit Pasien",
                PaymentChannel = isTopUp ? Channel(movement.PaymentMethodId, movement.PaymentMethodAccountId) : null,
                Amount = movement.Amount,
                Status = reversed ? "REVERSED" : "SUCCEEDED",
                StatusName = reversed ? "Dibalik" : "Berhasil",
                Note = !string.IsNullOrWhiteSpace(movement.ReferenceNumber) ? movement.ReferenceNumber : movement.Reason,
                CashierName = CashierName(movement.CreateBy)
            });
        }

        var ordered = entries
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.EntryType, StringComparer.Ordinal)
            .ThenBy(x => x.Id)
            .ToList();

        return new EncounterPaymentHistoryResponse
        {
            EncounterId = encounterId,
            Entries = ordered,
            Summary = new EncounterPaymentHistorySummaryResponse
            {
                TotalTenderReceived = ordered
                    .Where(x => x.EntryType == EncounterPaymentHistoryEntryTypes.TenderPayment
                        && x.Status == BillingTenderStatuses.Succeeded)
                    .Sum(x => x.Amount),
                TotalDepositTopUp = ordered
                    .Where(x => x.EntryType == EncounterPaymentHistoryEntryTypes.DepositTopUp && x.Status != "REVERSED")
                    .Sum(x => x.Amount),
                TotalDepositUsed = ordered
                    .Where(x => x.EntryType == EncounterPaymentHistoryEntryTypes.DepositAllocation && x.Status != "REVERSED")
                    .Sum(x => x.Amount),
                EntryCount = ordered.Count
            }
        };
    }

    private static string? FormatChannel(string? bankName, string? accountNumber, string? holderName)
    {
        var head = string.Join(" - ", new[] { bankName, accountNumber }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (head.Length == 0) return null;
        return string.IsNullOrWhiteSpace(holderName) ? head : $"{head} (a.n. {holderName})";
    }

    private static string TenderStatusName(string status) => status switch
    {
        BillingTenderStatuses.Succeeded => "Berhasil",
        BillingTenderStatuses.Pending => "Pending",
        BillingTenderStatuses.Failed => "Gagal",
        BillingTenderStatuses.Expired => "Kedaluwarsa",
        BillingTenderStatuses.Reversed => "Dibalik",
        BillingTenderStatuses.Created => "Dibuat",
        _ => status
    };

    // Sama dengan BillingSettlementService.MaskProviderReference: referensi penyedia tidak pernah
    // dikembalikan utuh.
    private static string? MaskReference(string? providerReference)
    {
        if (string.IsNullOrWhiteSpace(providerReference)) return null;
        return providerReference.Length <= 4 ? "****" : $"****{providerReference[^4..]}";
    }
}
