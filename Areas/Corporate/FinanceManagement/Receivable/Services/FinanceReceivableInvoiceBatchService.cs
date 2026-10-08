using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Layanan Batch Tagihan AR (BE-FIN-039, FIN-DEC-048, 054; 02-backend-architecture.md,
/// erd/data-dictionary.md §C.13-C.14). Menggabungkan beberapa FinReceivable milik satu penjamin
/// menjadi satu dokumen tagihan resmi. BUKAN pengganti FinReceivable — status batch murni
/// mengikuti/meringkas status anggotanya (FIN-STATE-1.2 §B.7); FinanceReceivableService TETAP
/// satu-satunya penulis OutstandingAmount. Service ini HANYA MEMBACA FinReceivable, tidak pernah
/// menulis kolomnya.
/// BE-FIN-052 (FIN-DEC-097, FIN-DES-070/071): sumbu klaim penjamin (ClaimStatus) adalah sumbu
/// KEDUA yang berdiri di samping Status — bukan perluasan enum Status. VerifyClaimAsync,
/// ApproveClaimAsync, CloseClaimAsync TIDAK PERNAH menulis Status/IssuedAt/OutstandingAmount;
/// ApproveClaimAsync TIDAK PERNAH memanggil FinanceReceivableService. Selisih klaim
/// (ClaimVarianceAmount) dihitung di Map(), TIDAK disimpan.
/// </summary>
public sealed class FinanceReceivableInvoiceBatchService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly BillingCompanyGuarantorInvoiceDocumentService _documentService;
    private readonly NumberSeriesAllocator _numberSeriesAllocator;
    private readonly FinanceReceivableService _receivableService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;

    public FinanceReceivableInvoiceBatchService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        BillingCompanyGuarantorInvoiceDocumentService documentService,
        NumberSeriesAllocator numberSeriesAllocator,
        FinanceReceivableService receivableService,
        FinanceAccountingOutboxService accountingOutboxService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _documentService = documentService;
        _numberSeriesAllocator = numberSeriesAllocator;
        _receivableService = receivableService;
        _accountingOutboxService = accountingOutboxService;
    }

    // ------------------------------------------------------------------------------------
    // Daftar batch — GET /
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<ReceivableInvoiceBatchResponse>> GetPagedAsync(
        ReceivableInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking().Where(x => !x.IsDelete);
        query = ApplyFilter(query, request);

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "batchnumber" or "invoicenumber" => descending ? query.OrderByDescending(x => x.BatchNumber) : query.OrderBy(x => x.BatchNumber),
            "totalamount" => descending ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
            "totaldiscount" => descending ? query.OrderByDescending(x => x.TotalDiscount) : query.OrderBy(x => x.TotalDiscount),
            "netamount" => descending ? query.OrderByDescending(x => x.TotalAmount - x.TotalDiscount) : query.OrderBy(x => x.TotalAmount - x.TotalDiscount),
            "invoicedate" => descending ? query.OrderByDescending(x => x.InvoiceDate) : query.OrderBy(x => x.InvoiceDate),
            "duedate" => descending ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var pagedRows = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new
            {
                Batch = x,
                MemberCount = x.Items.Count(i => !i.IsDelete)
            })
            .ToListAsync(cancellationToken);

        var debtorIds = pagedRows.Select(r => r.Batch.DebtorReferenceId).Distinct().ToList();
        var companyNames = debtorIds.Count > 0
            ? await _dbContext.MstCompanyGuarantors.AsNoTracking()
                .Where(c => debtorIds.Contains(c.Id) && !c.IsDelete)
                .ToDictionaryAsync(c => c.Id, c => c.CompanyGuarantorName, cancellationToken)
            : new Dictionary<Guid, string>();
        var insuranceNames = debtorIds.Count > 0
            ? await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(p => debtorIds.Contains(p.Id) && !p.IsDelete)
                .ToDictionaryAsync(p => p.Id, p => p.InsuranceProviderName, cancellationToken)
            : new Dictionary<Guid, string>();

        var items = pagedRows.Select(r =>
        {
            var b = r.Batch;
            var mapped = Map(b);
            string? debtorName = null;
            if (companyNames.TryGetValue(b.DebtorReferenceId, out var cName)) debtorName = cName;
            else if (insuranceNames.TryGetValue(b.DebtorReferenceId, out var iName)) debtorName = iName;

            var visitCode = ResolveBatchVisitCode(b);
            mapped.DebtorName = debtorName;
            mapped.PayerName = debtorName;
            mapped.CompanyId = b.DebtorReferenceId;
            mapped.CompanyName = debtorName;
            mapped.VisitCode = visitCode;
            mapped.VisitLabel = ResolveVisitLabel(visitCode);
            mapped.SentDate = b.IssuedAt;
            mapped.TotalDiscount = b.TotalDiscount;
            mapped.NetAmount = b.TotalAmount - b.TotalDiscount;
            mapped.TransactionCount = r.MemberCount;
            mapped.MemberCount = r.MemberCount;
            return mapped;
        }).ToList();

        return new PagedResult<ReceivableInvoiceBatchResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    // ------------------------------------------------------------------------------------
    // Ringkasan batch — GET /summary
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableInvoiceBatchSummaryResponse> GetSummaryAsync(
        ReceivableInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking().Where(x => !x.IsDelete);
        query = ApplyFilter(query, request);

        var aggregate = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(x => x.TotalAmount),
                TotalDiscount = g.Sum(x => x.TotalDiscount),
                TransactionCount = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new ReceivableInvoiceBatchSummaryResponse
        {
            TotalAmount = aggregate?.TotalAmount ?? 0m,
            TotalDiscount = aggregate?.TotalDiscount ?? 0m,
            NetAmount = (aggregate?.TotalAmount ?? 0m) - (aggregate?.TotalDiscount ?? 0m),
            TransactionCount = aggregate?.TransactionCount ?? 0
        };
    }

    // ------------------------------------------------------------------------------------
    // Rincian batch — GET /{id}. Status disegarkan di sini (FIN-STATE-1.2 §B.7: "murni
    // mengikuti/meringkas status anggotanya") sebelum dikembalikan ke pemanggil.
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableInvoiceBatchDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        await RefreshStatusAsync(batch, cancellationToken);

        var mapped = Map(batch);

        // Resolve Debtor Name
        var company = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == batch.DebtorReferenceId && !x.IsDelete)
            .Select(x => x.CompanyGuarantorName)
            .FirstOrDefaultAsync(cancellationToken);
        var debtorName = company ?? await _dbContext.MstInsuranceProviders.AsNoTracking()
            .Where(x => x.Id == batch.DebtorReferenceId && !x.IsDelete)
            .Select(x => x.InsuranceProviderName)
            .FirstOrDefaultAsync(cancellationToken);

        var visitCode = ResolveBatchVisitCode(batch);

        // Members & Patient Context
        var memberReceivables = batch.Items
            .Where(x => !x.IsDelete && x.Receivable is not null)
            .Select(x => x.Receivable!)
            .ToList();

        var receivableIds = memberReceivables.Select(r => r.Id).ToList();

        var receivableItems = await _dbContext.FinReceivableItems.AsNoTracking()
            .Where(x => !x.IsDelete && receivableIds.Contains(x.ReceivableId))
            .ToListAsync(cancellationToken);

        var invoiceIds = memberReceivables.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value)
            .Concat(receivableItems.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value))
            .Distinct()
            .ToList();

        var invoices = invoiceIds.Count > 0
            ? await _dbContext.BilInvoices.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, BilInvoice>();

        var encounterIds = invoices.Values.Select(x => x.EncounterId)
            .Concat(receivableItems.Where(x => x.EncounterId.HasValue).Select(x => x.EncounterId!.Value))
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        var encounters = encounterIds.Count > 0
            ? await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(x => encounterIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, RegPatientEncounter>();

        var patientIds = encounters.Values.Select(x => x.PatientId)
            .Concat(receivableItems.Where(x => x.PatientId.HasValue).Select(x => x.PatientId!.Value))
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        var patients = patientIds.Count > 0
            ? await _dbContext.MstPatients.AsNoTracking()
                .Where(x => patientIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, MstPatient>();

        var members = batch.Items
            .Where(x => !x.IsDelete && x.Receivable is not null)
            .Select(x =>
            {
                var r = x.Receivable!;
                var itemsForRec = receivableItems.Where(i => i.ReceivableId == r.Id).ToList();
                Guid? invId = r.InvoiceId ?? itemsForRec.FirstOrDefault(i => i.InvoiceId.HasValue)?.InvoiceId;
                BilInvoice? inv = invId.HasValue && invoices.TryGetValue(invId.Value, out var foundInv) ? foundInv : null;

                Guid? encId = inv?.EncounterId ?? itemsForRec.FirstOrDefault(i => i.EncounterId.HasValue)?.EncounterId;
                RegPatientEncounter? enc = encId.HasValue && encounters.TryGetValue(encId.Value, out var foundEnc) ? foundEnc : null;

                Guid? patId = enc?.PatientId ?? itemsForRec.FirstOrDefault(i => i.PatientId.HasValue)?.PatientId;
                MstPatient? pat = patId.HasValue && patients.TryGetValue(patId.Value, out var foundPat) ? foundPat : null;

                var billingDate = inv?.InvoiceDate.HasValue == true
                    ? DateOnly.FromDateTime(inv.InvoiceDate.Value.LocalDateTime)
                    : r.DueDate.AddDays(-30);

                return new ReceivableInvoiceBatchMemberResponse
                {
                    Id = x.Id,
                    ReceivableId = r.Id,
                    ReceivableNumber = r.ReceivableNumber,
                    InvoiceId = invId,
                    BillingNumber = inv?.InvoiceNumber ?? "-",
                    BillingDate = billingDate,
                    DueDate = r.DueDate,
                    DueDatePeriod = $"{r.DueDate:dd/MM/yyyy}",
                    PatientId = patId,
                    PatientName = pat?.FullName ?? "-",
                    MedicalRecordNumber = pat?.MedicalRecordNumber ?? "-",
                    OriginalAmount = r.OriginalAmount,
                    AllocatedAmount = r.AllocatedAmount,
                    AdjustedAmount = r.AdjustedAmount,
                    WrittenOffAmount = r.WrittenOffAmount,
                    OutstandingAmount = r.OutstandingAmount,
                    ReceivableStatus = r.Status,
                    IsSettled = r.Status == FinReceivableStatuses.Settled,
                    Note = null
                };
            })
            .ToList();

        // Payment History
        var allocations = await _dbContext.FinReceiptAllocations
            .Include(a => a.Receipt)
                .ThenInclude(r => r!.BankAccount)
                    .ThenInclude(b => b!.Bank)
            .Include(a => a.Receivable)
            .Where(a => receivableIds.Contains(a.ReceivableId!.Value) && !a.IsDelete)
            .OrderByDescending(a => a.AllocatedAt)
            .ToListAsync(cancellationToken);

        var allocationIds = allocations.Select(a => a.Id).ToList();
        var deductions = allocationIds.Count > 0
            ? await _dbContext.FinReceiptDeductions
                .Where(d => allocationIds.Contains(d.ReceiptAllocationId) && !d.IsDelete)
                .ToListAsync(cancellationToken)
            : new List<FinReceiptDeduction>();

        var payments = allocations
            .GroupBy(a => a.ReceiptId)
            .Select(g =>
            {
                var first = g.First();
                var rc = first.Receipt;
                var gAllocationIds = g.Select(a => a.Id).ToHashSet();
                var gDeductions = deductions.Where(d => gAllocationIds.Contains(d.ReceiptAllocationId)).ToList();
                var pph = gDeductions.Where(d => d.DeductionType == FinReceiptDeductionTypes.Pph23).Sum(d => d.Amount);
                var bankFee = gDeductions.Where(d => d.DeductionType == FinReceiptDeductionTypes.BankAdminFee).Sum(d => d.Amount);

                return new ReceivableInvoiceBatchPaymentHistoryResponse
                {
                    ReceiptId = g.Key,
                    ReceiptNumber = rc?.ReceiptNumber ?? "-",
                    PaymentDate = rc?.OccurredAt ?? first.AllocatedAt,
                    PaymentMethodName = rc?.BankAccount?.Bank != null ? $"Bank - {rc.BankAccount.Bank.BankName}" : (rc?.BankAccount != null ? $"Bank - {rc.BankAccount.AccountName}" : "Transfer Bank"),
                    BankAccountId = rc?.BankAccountId,
                    BankAccountNumber = rc?.BankAccount?.AccountNumber,
                    BankName = rc?.BankAccount?.Bank?.BankName ?? rc?.BankAccount?.AccountName,
                    TotalAmount = rc?.Amount ?? g.Sum(a => a.Amount),
                    AllocatedAmount = g.Sum(a => a.Amount),
                    Pph23Amount = pph,
                    BankAdminFeeAmount = bankFee,
                    Status = rc?.Status ?? FinReceiptStatuses.Allocated,
                    Allocations = g.Select(a => new ReceivableInvoiceBatchPaymentHistoryAllocationResponse
                    {
                        AllocationId = a.Id,
                        ReceivableId = a.ReceivableId!.Value,
                        ReceivableNumber = a.Receivable?.ReceivableNumber ?? "-",
                        Amount = a.Amount,
                        IsReversal = a.IsReversal,
                        AllocatedAt = a.AllocatedAt
                    }).ToList()
                };
            })
            .OrderByDescending(p => p.PaymentDate)
            .ToList();

        var totalReceivable = members.Sum(m => m.OriginalAmount);
        var totalAllocated = members.Sum(m => m.AllocatedAmount);
        var totalOutstanding = members.Sum(m => m.OutstandingAmount);
        var totalAdjusted = members.Sum(m => m.AdjustedAmount);
        var totalWrittenOff = members.Sum(m => m.WrittenOffAmount);
        var totalPph23 = deductions.Where(d => d.DeductionType == FinReceiptDeductionTypes.Pph23).Sum(d => d.Amount);
        var totalBankAdminFee = deductions.Where(d => d.DeductionType == FinReceiptDeductionTypes.BankAdminFee).Sum(d => d.Amount);

        return new ReceivableInvoiceBatchDetailResponse
        {
            Id = mapped.Id,
            BatchNumber = mapped.BatchNumber,
            DebtorType = mapped.DebtorType,
            DebtorReferenceId = mapped.DebtorReferenceId,
            CompanyId = mapped.DebtorReferenceId,
            DebtorName = debtorName,
            PayerName = debtorName,
            CompanyName = debtorName,
            VisitCode = visitCode,
            VisitLabel = ResolveVisitLabel(visitCode),
            PeriodStart = mapped.PeriodStart,
            PeriodEnd = mapped.PeriodEnd,
            InvoiceDate = mapped.InvoiceDate,
            DueDate = mapped.DueDate,
            PaymentTermDays = mapped.PaymentTermDays,
            Note = mapped.Note,
            TotalAmount = mapped.TotalAmount,
            TotalReceivable = totalReceivable,
            TotalAllocated = totalAllocated,
            TotalOutstanding = totalOutstanding,
            TotalAdjusted = totalAdjusted,
            TotalWrittenOff = totalWrittenOff,
            TotalDiscount = batch.TotalDiscount,
            DiscountPercent = batch.DiscountPercent ?? 0m,
            DiscountNote = batch.DiscountNote,
            OtherReceiptAmount = batch.OtherReceiptAmount,
            OtherReceiptNote = batch.OtherReceiptNote,
            Pph23Amount = totalPph23,
            BankAdminFeeAmount = totalBankAdminFee,
            NetInvoiceAmount = batch.TotalAmount - batch.TotalDiscount,
            Status = mapped.Status,
            IssuedAt = mapped.IssuedAt,
            SentDate = mapped.IssuedAt,
            RowVersion = mapped.RowVersion,
            ClaimStatus = mapped.ClaimStatus,
            ApprovedAmount = mapped.ApprovedAmount,
            ClaimVarianceAmount = mapped.ClaimVarianceAmount,
            PayerClaimReference = mapped.PayerClaimReference,
            ClaimNote = mapped.ClaimNote,
            PayerVerifiedAt = mapped.PayerVerifiedAt,
            ClaimApprovedAt = mapped.ClaimApprovedAt,
            ClaimClosedAt = mapped.ClaimClosedAt,
            Members = members,
            Payments = payments
        };
    }

    // ------------------------------------------------------------------------------------
    // Konteks pembuatan batch — GET /create-context. Menyediakan metadata penjamin,
    // default tanggal dokumen, Terms of Payment authoritative, dan estimasi jatuh tempo
    // tanpa menduplikasi pembacaan daftar piutang (/billing-data).
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableInvoiceBatchCreateContextResponse> GetCreateContextAsync(
        Guid debtorReferenceId, string? category, CancellationToken cancellationToken)
    {
        // 1. Cek apakah ini Company Guarantor
        var company = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == debtorReferenceId && !x.IsDelete)
            .Select(x => new { x.Id, x.CompanyGuarantorName })
            .FirstOrDefaultAsync(cancellationToken);

        string payerName;
        string payerKind;

        if (company != null)
        {
            payerName = company.CompanyGuarantorName;
            payerKind = BillingDataPayerKinds.Company;
        }
        else
        {
            // 2. Cek apakah ini Insurance Provider
            var insurance = await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(x => x.Id == debtorReferenceId && !x.IsDelete)
                .Select(x => new { x.Id, x.InsuranceProviderName })
                .FirstOrDefaultAsync(cancellationToken);

            if (insurance != null)
            {
                payerName = insurance.InsuranceProviderName;
                payerKind = BillingDataPayerKinds.Insurance;
            }
            else
            {
                throw new KeyNotFoundException($"Penjamin dengan Id '{debtorReferenceId}' tidak ditemukan.");
            }
        }

        // Business rule baru AR Invoice: Due Date selalu 30 hari kalender dari Tanggal Invoice
        var defaultInvoiceDate = FinanceBusinessDate.Today();
        const int paymentTermDays = 30;
        var dueDatePreview = defaultInvoiceDate.AddDays(paymentTermDays);

        return new ReceivableInvoiceBatchCreateContextResponse
        {
            DebtorReferenceId = debtorReferenceId,
            PayerName = payerName,
            PayerKind = payerKind,
            DefaultInvoiceDate = defaultInvoiceDate,
            PaymentTermDays = paymentTermDays,
            DueDatePreview = dueDatePreview,
            DueDateSource = ReceivableDueDateSources.SystemDefault30Days,
            IsTermConfigured = true
        };
    }

    // ------------------------------------------------------------------------------------
    // Piutang yang memenuhi syarat digabung — GET /eligible-receivables. "Memenuhi syarat"
    // ditafsirkan sebagai: penjamin sama, DebtorType PAYER (satu-satunya yang didukung batch,
    // FIN-DEC-048), belum tergabung batch aktif, dan masih punya sisa tagihan (OUTSTANDING/
    // PARTIAL) — piutang yang sudah SETTLED/WRITTEN_OFF/CANCELLED tidak ada gunanya ditagih
    // ulang. Bukan FIN-VAL bernomor; murni definisi teknis "layak digabung".
    // ------------------------------------------------------------------------------------

    public async Task<List<EligibleReceivableResponse>> GetEligibleReceivablesAsync(
        Guid debtorReferenceId, CancellationToken cancellationToken)
    {
        var activeBatchMemberIds = _dbContext.FinReceivableInvoiceBatchItems
            .Where(x => !x.IsDelete && x.IsActiveMembership)
            .Select(x => x.ReceivableId);

        return await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => !x.IsDelete
                && x.DebtorType == FinReceivableDebtorTypes.Payer
                && x.DebtorReferenceId == debtorReferenceId
                && (x.Status == FinReceivableStatuses.Outstanding || x.Status == FinReceivableStatuses.Partial)
                && !activeBatchMemberIds.Contains(x.Id))
            .OrderBy(x => x.DueDate)
            .Select(x => new EligibleReceivableResponse
            {
                Id = x.Id,
                ReceivableNumber = x.ReceivableNumber,
                InvoiceId = x.InvoiceId,
                OriginalAmount = x.OriginalAmount,
                OutstandingAmount = x.OutstandingAmount,
                DueDate = x.DueDate,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Pembuatan batch (DRAFT) — FIN-VAL-114, FIN-VAL-115, state-transition-matrix.md §B.7
    // ------------------------------------------------------------------------------------

    public async Task<FinReceivableInvoiceBatch> CreateAsync(
        DateOnly periodStart, DateOnly periodEnd, IReadOnlyList<Guid> receivableIds,
        Guid actorUserId, CancellationToken cancellationToken,
        DateOnly? invoiceDate = null, string? note = null)
    {
        var distinctIds = receivableIds.Distinct().ToList();
        var receivables = await _dbContext.FinReceivables
            .Where(x => distinctIds.Contains(x.Id) && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var missing = distinctIds.Except(receivables.Select(x => x.Id)).ToList();
        if (missing.Count > 0)
            throw new KeyNotFoundException($"Piutang tidak ditemukan: {string.Join(", ", missing)}.");

        // Bukan FIN-VAL bernomor — batas skema §C.13: DebtorType batch dipatok PAYER (FIN-DEC-048).
        var nonPayer = receivables.Where(x => x.DebtorType != FinReceivableDebtorTypes.Payer).ToList();
        if (nonPayer.Count > 0)
            throw new ReceivableInvoiceBatchBadRequestException(
                $"Batch Tagihan AR hanya menerima piutang berjenis PAYER. Piutang {nonPayer[0].ReceivableNumber} berjenis {nonPayer[0].DebtorType}.");

        // Hanya piutang yang masih bisa ditagihkan (OUTSTANDING/PARTIAL) ATAU sudah lunas dibayar (SETTLED, mis.
        // gabungan asuransi + excess tunai) dan bernilai positif yang boleh ditagihkan. Status pembayaran (SETTLED)
        // dan status pembuatan AR/Invoice (keanggotaan batch) adalah dua hal terpisah: piutang lunas yang belum
        // pernah digabung tetap perlu dibuatkan AR/Invoice-nya untuk keperluan dokumentasi/klaim. Piutang
        // CANCELLED atau WRITTEN_OFF tetap ditolak di server, tidak hanya disembunyikan dari daftar.
        var notBillable = receivables.Where(x =>
            (x.Status != FinReceivableStatuses.Outstanding
                && x.Status != FinReceivableStatuses.Partial
                && x.Status != FinReceivableStatuses.Settled)
            || x.OriginalAmount <= 0).ToList();
        if (notBillable.Count > 0)
            throw new ReceivableInvoiceBatchValidationException(
                $"Piutang {notBillable[0].ReceivableNumber} berstatus {notBillable[0].Status} dan tidak dapat ditagihkan. Hanya piutang OUTSTANDING, PARTIAL, atau SETTLED dengan nilai lebih dari nol yang dapat digabung.");

        // FIN-VAL-115: seluruh piutang dalam satu batch harus milik penjamin yang sama.
        var debtorReferenceIds = receivables.Select(x => x.DebtorReferenceId).Distinct().ToList();
        if (debtorReferenceIds.Count > 1 || debtorReferenceIds[0] is null)
            throw new ReceivableInvoiceBatchBadRequestException("Seluruh piutang dalam satu batch harus milik penjamin yang sama.");
        var debtorReferenceId = debtorReferenceIds[0]!.Value;

        // FIN-VAL-114: piutang yang sudah tergabung batch aktif tidak boleh digabung lagi.
        var alreadyBatched = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
            .Where(x => !x.IsDelete
                && distinctIds.Contains(x.ReceivableId)
                && x.IsActiveMembership)
            .Select(x => x.ReceivableId)
            .ToListAsync(cancellationToken);
        if (alreadyBatched.Count > 0)
            throw new ReceivableInvoiceBatchConflictException(
                $"Piutang {string.Join(", ", alreadyBatched)} sudah tergabung dalam batch tagihan lain.");

        // 1. Load seluruh item piutang, invoice terkait, dan encounter terkait untuk klasifikasi VisitCode & ServiceType
        var receivableItems = await _dbContext.FinReceivableItems.AsNoTracking()
            .Where(x => !x.IsDelete && distinctIds.Contains(x.ReceivableId))
            .ToListAsync(cancellationToken);

        var invoiceIds = receivables.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value)
            .Concat(receivableItems.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value))
            .Distinct()
            .ToList();

        var invoices = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => invoiceIds.Contains(x.Id) && !x.IsDelete)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var encounterIds = invoices.Values.Select(x => x.EncounterId)
            .Concat(receivableItems.Where(x => x.EncounterId.HasValue).Select(x => x.EncounterId!.Value))
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        var encounters = await _dbContext.RegPatientEncounters.AsNoTracking()
            .Where(x => encounterIds.Contains(x.Id) && !x.IsDelete)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        // 2. Resolve seluruh VisitCode dan pastikan hanya ada satu distinct VisitCode dalam batch
        var visitCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var receivable in receivables)
        {
            var itemsForRec = receivableItems.Where(x => x.ReceivableId == receivable.Id).ToList();
            var code = ResolveReceivableVisitCode(receivable, itemsForRec, invoices, encounters);
            visitCodes.Add(code);
        }

        if (visitCodes.Count > 1)
        {
            throw new ReceivableInvoiceBatchValidationException(
                "Tagihan Rawat Inap dan Rawat Jalan/IGD/OTC tidak dapat digabung dalam satu Invoice AR.");
        }

        var visitCode = visitCodes.First();

        // 2b. Snapshot ServiceType dari sumber authoritative BilInvoice atau fallback visitCode
        string? batchServiceType = null;
        var invoiceServiceTypes = invoices.Values
            .Where(x => !string.IsNullOrWhiteSpace(x.ServiceType))
            .Select(x => x.ServiceType.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();
        if (invoiceServiceTypes.Count > 0)
        {
            batchServiceType = invoiceServiceTypes[0];
        }
        else
        {
            batchServiceType = visitCode == "IP" ? "RANAP" : "RAJAL";
        }

        // 3. Due Date selalu otomatis 30 hari kalender dari Tanggal Pembuatan Invoice
        var effectiveInvoiceDate = invoiceDate ?? FinanceBusinessDate.Today();
        const int paymentTermDays = 30;
        var batchDueDate = effectiveInvoiceDate.AddDays(paymentTermDays);

        // 4. Nomor Invoice AR resmi menggunakan format {SEQUENCE}/{VISIT_CODE}/RSMMC/{ROMAN_MONTH}/{YEAR}
        var invoiceNumber = await GenerateArInvoiceNumberAsync(
            visitCode,
            effectiveInvoiceDate,
            actorUserId,
            cancellationToken);

        var batch = new FinReceivableInvoiceBatch
        {
            Id = Guid.NewGuid(),
            BatchNumber = invoiceNumber,
            DebtorType = FinReceivableDebtorTypes.Payer,
            DebtorReferenceId = debtorReferenceId,
            VisitCode = visitCode,
            ServiceType = batchServiceType,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            InvoiceDate = effectiveInvoiceDate,
            DueDate = batchDueDate,
            PaymentTermDays = paymentTermDays,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            TotalAmount = receivables.Sum(x => x.OriginalAmount),
            Status = FinReceivableInvoiceBatchStatuses.Draft,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            UpdateBy = actorUserId,
            DeleteBy = actorUserId,
            CancelBy = actorUserId
        };

        foreach (var receivable in receivables)
        {
            batch.Items.Add(new FinReceivableInvoiceBatchItem
            {
                Id = Guid.NewGuid(),
                BatchId = batch.Id,
                ReceivableId = receivable.Id,
                IsActiveMembership = true,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                DeleteBy = actorUserId,
                CancelBy = actorUserId
            });
        }

        _dbContext.FinReceivableInvoiceBatches.Add(batch);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Dua permintaan bersamaan melewati pemeriksaan di atas; indeks unik ActiveReceivable menolak yang
            // kedua. Hasilnya 409, bukan batch ganda dan bukan kesalahan server.
            throw new ReceivableInvoiceBatchConflictException(
                "Sebagian piutang sudah tergabung dalam batch tagihan lain oleh permintaan lain. Muat ulang daftar lalu coba lagi.",
                exception);
        }

        await AuditAsync("Create", batch.Id, actorUserId);
        return batch;
    }

    // ------------------------------------------------------------------------------------
    // Penerbitan (DRAFT -> ISSUED) — FIN-VAL-116
    // ------------------------------------------------------------------------------------

    public async Task<FinReceivableInvoiceBatch> IssueAsync(
        Guid batchId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Draft)
            throw new ReceivableInvoiceBatchValidationException($"Hanya batch berstatus DRAFT yang dapat diterbitkan. Status saat ini: {batch.Status}.");

        // FIN-VAL-116: batch kosong tidak boleh diterbitkan.
        if (batch.Items.Count(x => !x.IsDelete) == 0)
            throw new ReceivableInvoiceBatchValidationException("Batch tidak memiliki anggota piutang untuk diterbitkan.");

        batch.Status = FinReceivableInvoiceBatchStatuses.Issued;
        batch.IssuedAt = DateTimeOffset.UtcNow;
        // FIN-DES-070: sumbu klaim dimulai otomatis saat dokumen terbit — tidak menunggu aksi petugas.
        batch.ClaimStatus = FinReceivableInvoiceBatchClaimStatuses.Submitted;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Issue", batch.Id, actorUserId);
        return batch;
    }

    // ------------------------------------------------------------------------------------
    // Pembatalan (DRAFT -> CANCELLED) — anggotanya otomatis bebas digabung batch lain karena
    // FIN-VAL-114/eligible-query menyaring berdasarkan Batch.Status <> CANCELLED.
    // ------------------------------------------------------------------------------------

    public Task<FinReceivableInvoiceBatch> CancelAsync(
        Guid batchId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken) =>
        CancelAsync(batchId, expectedRowVersion, "Pembatalan oleh pengguna", actorUserId, cancellationToken);

    public async Task<FinReceivableInvoiceBatch> CancelAsync(
        Guid batchId, Guid expectedRowVersion, string reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ReceivableInvoiceBatchValidationException("Alasan pembatalan wajib diisi.");

        var trimmedReason = reason.Trim();
        if (trimmedReason.Length > 500)
            throw new ReceivableInvoiceBatchValidationException("Alasan pembatalan tidak boleh melebihi 500 karakter.");

        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        if (batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
            throw new ReceivableInvoiceBatchValidationException("Batch Tagihan AR sudah dibatalkan sebelumnya.");

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Draft && batch.Status != FinReceivableInvoiceBatchStatuses.Issued)
            throw new ReceivableInvoiceBatchValidationException($"Hanya batch berstatus DRAFT atau ISSUED yang dapat dibatalkan. Status saat ini: {batch.Status}.");

        // Periksa apakah batch/piutang anggota sudah memiliki alokasi pembayaran
        var memberReceivableIds = batch.Items.Where(x => !x.IsDelete).Select(x => x.ReceivableId).ToList();
        if (memberReceivableIds.Count > 0)
        {
            var hasPayments = await _dbContext.FinReceiptAllocations.AsNoTracking()
                .AnyAsync(a => !a.IsDelete && !a.IsReversal && memberReceivableIds.Contains(a.ReceivableId!.Value) && a.Amount > 0, cancellationToken);
            if (hasPayments)
                throw new ReceivableInvoiceBatchValidationException("Batch tagihan tidak dapat dibatalkan karena sudah memiliki alokasi pembayaran.");
        }

        batch.Status = FinReceivableInvoiceBatchStatuses.Cancelled;
        batch.CancelReason = trimmedReason;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.CancelDateTime = DateTime.UtcNow;
        batch.CancelBy = actorUserId;
        batch.IsCancel = true;
        batch.RowVersion = Guid.NewGuid();

        // Nonaktifkan keanggotaan batch item agar piutang dapat digabung ulang / di-reissue
        foreach (var item in batch.Items.Where(x => !x.IsDelete))
        {
            item.IsActiveMembership = false;
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = actorUserId;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Cancel", batch.Id, actorUserId);
        return batch;
    }

    // ------------------------------------------------------------------------------------
    // Sumbu klaim penjamin (BE-FIN-052, FIN-DEC-097, FIN-DES-070/071) — state-transition-matrix.md
    // §D.1. Sumbu INI tidak pernah menulis Status/IssuedAt/OutstandingAmount; RefreshStatusAsync dan
    // seluruh jalur pelunasan di atas tidak pernah menulis kolom klaim. Keduanya MUST tetap terpisah.
    // ------------------------------------------------------------------------------------

    /// <summary>FIN-VAL-147: klaim hanya ada pada batch yang sudah terbit (Status ISSUED/PARTIALLY_PAID/PAID).
    /// Batch lama yang terbit sebelum kolom ini ada (ClaimStatus masih kosong) diperlakukan seolah
    /// SUBMITTED — tanpa backfill, sesuai rencana migration (02-backend-architecture.md §J.6).</summary>
    private static string? ResolveEffectiveClaimStatus(FinReceivableInvoiceBatch batch)
    {
        if (!string.IsNullOrEmpty(batch.ClaimStatus)) return batch.ClaimStatus;

        var alreadyIssued = batch.Status is FinReceivableInvoiceBatchStatuses.Issued
            or FinReceivableInvoiceBatchStatuses.PartiallyPaid or FinReceivableInvoiceBatchStatuses.Paid;
        return alreadyIssued ? FinReceivableInvoiceBatchClaimStatuses.Submitted : null;
    }

    public async Task<FinReceivableInvoiceBatch> VerifyClaimAsync(
        Guid batchId, Guid expectedRowVersion, string? payerClaimReference, string? claimNote,
        Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        var effective = ResolveEffectiveClaimStatus(batch);
        if (effective is null)
            throw new ReceivableInvoiceBatchValidationException(
                "Tagihan ini belum diterbitkan ke penjamin, jadi belum ada klaim yang bisa ditindaklanjuti. Terbitkan tagihannya lebih dulu.");
        if (effective != FinReceivableInvoiceBatchClaimStatuses.Submitted)
            throw new ReceivableInvoiceBatchValidationException("Langkah ini tidak bisa dilakukan dari status klaim saat ini.");

        batch.ClaimStatus = FinReceivableInvoiceBatchClaimStatuses.PayerVerified;
        if (payerClaimReference is not null) batch.PayerClaimReference = payerClaimReference;
        if (claimNote is not null) batch.ClaimNote = claimNote;
        batch.PayerVerifiedAt = DateTimeOffset.UtcNow;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.RowVersion = Guid.NewGuid();

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException ex) { throw Stale(ex); }

        await AuditAsync("ClaimVerify", batch.Id, actorUserId);
        return batch;
    }

    public async Task<FinReceivableInvoiceBatch> ApproveClaimAsync(
        Guid batchId, Guid expectedRowVersion, decimal approvedAmount, string? payerClaimReference,
        string? claimNote, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        var effective = ResolveEffectiveClaimStatus(batch);
        if (effective is null)
            throw new ReceivableInvoiceBatchValidationException(
                "Tagihan ini belum diterbitkan ke penjamin, jadi belum ada klaim yang bisa ditindaklanjuti. Terbitkan tagihannya lebih dulu.");
        // FIN-DES-070: Approve sah dari SUBMITTED (lompat verifikasi), PAYER_VERIFIED, maupun APPROVED (mencatat ulang).
        var validFrom = effective is FinReceivableInvoiceBatchClaimStatuses.Submitted
            or FinReceivableInvoiceBatchClaimStatuses.PayerVerified or FinReceivableInvoiceBatchClaimStatuses.Approved;
        if (!validFrom)
            throw new ReceivableInvoiceBatchValidationException("Langkah ini tidak bisa dilakukan dari status klaim saat ini.");

        // FIN-VAL-150: nominal disetujui tidak boleh melebihi total tagihan.
        if (approvedAmount > batch.TotalAmount)
            throw new ReceivableInvoiceBatchValidationException("Nominal yang disetujui penjamin tidak boleh melebihi total tagihan.");
        // FIN-VAL-151: nominal lebih kecil dari tagihan wajib disertai alasan.
        if (approvedAmount < batch.TotalAmount && string.IsNullOrWhiteSpace(claimNote))
            throw new ReceivableInvoiceBatchBadRequestException(
                "Karena penjamin menyetujui lebih kecil dari tagihan, tuliskan alasannya supaya selisihnya bisa ditindaklanjuti.");

        batch.ClaimStatus = FinReceivableInvoiceBatchClaimStatuses.Approved;
        batch.ApprovedAmount = approvedAmount;
        if (payerClaimReference is not null) batch.PayerClaimReference = payerClaimReference;
        if (claimNote is not null) batch.ClaimNote = claimNote;
        batch.ClaimApprovedAt = DateTimeOffset.UtcNow;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.RowVersion = Guid.NewGuid();

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException ex) { throw Stale(ex); }

        await AuditAsync("ClaimApprove", batch.Id, actorUserId);
        return batch;
    }

    public async Task<FinReceivableInvoiceBatch> CloseClaimAsync(
        Guid batchId, Guid expectedRowVersion, string? claimNote, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        var effective = ResolveEffectiveClaimStatus(batch);
        if (effective is null)
            throw new ReceivableInvoiceBatchValidationException(
                "Tagihan ini belum diterbitkan ke penjamin, jadi belum ada klaim yang bisa ditindaklanjuti. Terbitkan tagihannya lebih dulu.");
        if (effective != FinReceivableInvoiceBatchClaimStatuses.Approved)
            throw new ReceivableInvoiceBatchValidationException("Langkah ini tidak bisa dilakukan dari status klaim saat ini.");

        batch.ClaimStatus = FinReceivableInvoiceBatchClaimStatuses.Closed;
        if (claimNote is not null) batch.ClaimNote = claimNote;
        batch.ClaimClosedAt = DateTimeOffset.UtcNow;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.RowVersion = Guid.NewGuid();

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException ex) { throw Stale(ex); }

        await AuditAsync("ClaimClose", batch.Id, actorUserId);
        return batch;
    }

    // ------------------------------------------------------------------------------------
    // Dokumen tagihan gabungan — GET /{id}/document. Memanggil
    // BillingCompanyGuarantorInvoiceDocumentService (FIN-CAP-030) sekali per FinReceivable
    // anggota (lewat InvoiceId-nya), tidak menyalin logikanya.
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableInvoiceBatchDocumentResponse> GetDocumentAsync(
        Guid batchId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");
        var company = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == batch.DebtorReferenceId && !x.IsDelete)
            .Select(x => x.CompanyGuarantorName)
            .FirstOrDefaultAsync(cancellationToken);
        var debtorName = company ?? await _dbContext.MstInsuranceProviders.AsNoTracking()
            .Where(x => x.Id == batch.DebtorReferenceId && !x.IsDelete)
            .Select(x => x.InsuranceProviderName)
            .FirstOrDefaultAsync(cancellationToken);

        var response = new ReceivableInvoiceBatchDocumentResponse
        {
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            DebtorReferenceId = batch.DebtorReferenceId,
            CompanyId = batch.DebtorReferenceId,
            CompanyName = debtorName,
            PeriodStart = batch.PeriodStart,
            PeriodEnd = batch.PeriodEnd,
            InvoiceDate = batch.InvoiceDate,
            DueDate = batch.DueDate
        };

        foreach (var item in batch.Items.Where(x => !x.IsDelete && x.Receivable is not null))
        {
            // BE-FIN-079: item migrasi tagihan lama tidak punya InvoiceId (asalnya bukan Billing) —
            // tidak ada dokumen tagihan Billing untuk diambil, sehingga dilewati dengan peringatan
            // alih-alih memanggil _documentService dengan Guid kosong.
            if (item.Receivable!.InvoiceId is not { } invoiceId)
            {
                response.Warnings.Add($"Anggota {item.Receivable.ReceivableNumber} adalah item migrasi tagihan lama — nol dokumen Billing untuk diambil.");
                continue;
            }

            var invoiceDocument = await _documentService.GetDocumentAsync(invoiceId, actorUserId, cancellationToken);
            response.Invoices.Add(invoiceDocument);
            response.Warnings.AddRange(invoiceDocument.Warnings);
        }

        response.GrandTotalCoveredAmount = response.Invoices.Sum(x => x.Totals.TotalCoveredAmount);
        return response;
    }

    // ------------------------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// FIN-STATE-1.2 §B.7: "Status FinReceivableInvoiceBatch tidak pernah mengubah status
    /// FinReceivable anggotanya secara langsung. Pelunasan tetap tercatat lewat alokasi
    /// penerimaan pada FinReceivable masing-masing — status batch murni mengikuti/meringkas
    /// status anggotanya." Disegarkan lazy setiap batch dibaca satu-satu (GetByIdAsync),
    /// bukan lewat hook di FinanceReceivableService — nol perubahan pada service itu.
    /// </summary>
    private async Task RefreshStatusAsync(FinReceivableInvoiceBatch batch, CancellationToken cancellationToken)
    {
        if (batch.Status != FinReceivableInvoiceBatchStatuses.Issued
            && batch.Status != FinReceivableInvoiceBatchStatuses.PartiallyPaid)
        {
            return;
        }

        var members = batch.Items.Where(x => !x.IsDelete && x.Receivable is not null).ToList();
        if (members.Count == 0) return;

        var settledCount = members.Count(x => x.Receivable!.Status == FinReceivableStatuses.Settled);
        var newStatus = settledCount == members.Count
            ? FinReceivableInvoiceBatchStatuses.Paid
            : settledCount > 0
                ? FinReceivableInvoiceBatchStatuses.PartiallyPaid
                : FinReceivableInvoiceBatchStatuses.Issued;

        if (newStatus == batch.Status) return;

        batch.Status = newStatus;
        batch.UpdateDateTime = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ReceivableInvoiceBatchResponse Map(FinReceivableInvoiceBatch batch)
    {
        var visitCode = ResolveBatchVisitCode(batch);
        return new()
        {
            Id = batch.Id,
            BatchNumber = batch.BatchNumber,
            DebtorType = batch.DebtorType,
            DebtorReferenceId = batch.DebtorReferenceId,
            CompanyId = batch.DebtorReferenceId,
            VisitCode = visitCode,
            VisitLabel = ResolveVisitLabel(visitCode),
            PeriodStart = batch.PeriodStart,
            PeriodEnd = batch.PeriodEnd,
            InvoiceDate = batch.InvoiceDate,
            DueDate = batch.DueDate,
            PaymentTermDays = batch.PaymentTermDays,
            Note = batch.Note,
            TotalAmount = batch.TotalAmount,
            TotalDiscount = batch.TotalDiscount,
            NetAmount = batch.TotalAmount - batch.TotalDiscount,
            Status = batch.Status,
            IssuedAt = batch.IssuedAt,
            SentDate = batch.IssuedAt,
            RowVersion = batch.RowVersion,
            ClaimStatus = batch.ClaimStatus,
            ApprovedAmount = batch.ApprovedAmount,
            // FIN-DES-071: dihitung di sini, TIDAK disimpan. Null selama belum APPROVED.
            ClaimVarianceAmount = batch.ApprovedAmount.HasValue ? batch.TotalAmount - batch.ApprovedAmount.Value : null,
            PayerClaimReference = batch.PayerClaimReference,
            ClaimNote = batch.ClaimNote,
            PayerVerifiedAt = batch.PayerVerifiedAt,
            ClaimApprovedAt = batch.ClaimApprovedAt,
            ClaimClosedAt = batch.ClaimClosedAt
        };
    }

    public async Task<string> GenerateArInvoiceNumberAsync(
        string visitCode,
        DateOnly invoiceDate,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var allocationRequest = new NumberAllocationRequest(
            SequenceKey: "FIN_AR_INVOICE",
            Prefix: "AR",
            ResetPolicy: NumberSeriesResetPolicies.Never,
            SequenceDigits: 4,
            ActorUserId: actorUserId,
            Instant: DateTimeOffset.UtcNow);

        var allocated = await _numberSeriesAllocator.AllocateAsync(allocationRequest, cancellationToken);
        var sequencePart = allocated.Split('-').Last();
        if (!long.TryParse(sequencePart, out var sequenceNumber))
        {
            sequenceNumber = 1;
        }

        var monthRoman = ToRomanMonth(invoiceDate.Month);
        return $"{sequenceNumber:D4}/{visitCode}/RSMMC/{monthRoman}/{invoiceDate.Year}";
    }

    public static string ToRomanMonth(int month) => month switch
    {
        1 => "I",
        2 => "II",
        3 => "III",
        4 => "IV",
        5 => "V",
        6 => "VI",
        7 => "VII",
        8 => "VIII",
        9 => "IX",
        10 => "X",
        11 => "XI",
        12 => "XII",
        _ => throw new ArgumentOutOfRangeException(nameof(month), "Bulan harus antara 1 dan 12.")
    };

    public static string ResolveReceivableVisitCode(
        FinReceivable receivable,
        List<FinReceivableItem> items,
        Dictionary<Guid, BilInvoice> invoices,
        Dictionary<Guid, RegPatientEncounter> encounters)
    {
        // 1. Cek dari BilInvoice jika ada
        Guid? invoiceId = receivable.InvoiceId ?? items.FirstOrDefault(x => x.InvoiceId.HasValue)?.InvoiceId;
        BilInvoice? invoice = null;
        if (invoiceId.HasValue && invoices.TryGetValue(invoiceId.Value, out invoice))
        {
            if (string.Equals(invoice.ServiceType, "OTC", StringComparison.OrdinalIgnoreCase))
            {
                return "OP";
            }
            if (string.Equals(invoice.ServiceType, "RANAP", StringComparison.OrdinalIgnoreCase))
            {
                return "IP";
            }
            if (string.Equals(invoice.ServiceType, "RAJAL", StringComparison.OrdinalIgnoreCase)
                || string.Equals(invoice.ServiceType, "IGD", StringComparison.OrdinalIgnoreCase))
            {
                return "OP";
            }
        }

        // 2. Cek dari RegPatientEncounter jika ada
        Guid? encounterId = invoice?.EncounterId;
        if (!encounterId.HasValue || encounterId == Guid.Empty)
        {
            encounterId = items.FirstOrDefault(x => x.EncounterId.HasValue)?.EncounterId;
        }

        if (encounterId.HasValue && encounters.TryGetValue(encounterId.Value, out var encounter))
        {
            return encounter.EncounterType switch
            {
                EncounterType.Inpatient => "IP",
                EncounterType.Outpatient => "OP",
                EncounterType.Emergency => "OP",
                _ => throw new ReceivableInvoiceBatchValidationException(
                    $"Piutang {receivable.ReceivableNumber} dengan jenis kunjungan '{encounter.EncounterType}' tidak dapat diklasifikasikan secara aman ke IP atau OP.")
            };
        }

        throw new ReceivableInvoiceBatchValidationException(
            $"Piutang {receivable.ReceivableNumber} tidak memiliki data invoice atau encounter yang valid untuk klasifikasi IP/OP.");
    }

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion) throw Stale();
    }

    private static ReceivableInvoiceBatchConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private Task AuditAsync(string action, Guid batchId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceReceivableInvoiceBatch.{action}",
            $"Perubahan Batch Tagihan AR dicatat. BatchId={batchId}",
            new { BatchId = batchId, ActorUserId = actorUserId });

    // ------------------------------------------------------------------------------------
    // Pelunasan Batch Tagihan AR — POST /{id}/payments
    // ------------------------------------------------------------------------------------

    public async Task<PostReceivableInvoiceBatchPaymentResponse> PostPaymentAsync(
        Guid batchId,
        PostReceivableInvoiceBatchPaymentRequest request,
        string? idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, request.ExpectedRowVersion);

        if (batch.Status == FinReceivableInvoiceBatchStatuses.Draft)
            throw new ReceivableInvoiceBatchValidationException("Batch Tagihan AR masih berstatus DRAFT. Terbitkan (Issue) batch terlebih dahulu sebelum memproses pembayaran.");

        if (batch.Status == FinReceivableInvoiceBatchStatuses.Paid)
            throw new ReceivableInvoiceBatchValidationException("Batch Tagihan AR sudah lunas (PAID).");

        if (batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
            throw new ReceivableInvoiceBatchValidationException("Batch Tagihan AR sudah dibatalkan (CANCELLED).");

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existingReceipt = await _dbContext.FinReceipts.AsNoTracking()
                .FirstOrDefaultAsync(r => r.ProviderEventId == idempotencyKey && !r.IsDelete, cancellationToken);
            if (existingReceipt != null)
                throw new ReceivableInvoiceBatchConflictException("Pembayaran dengan kunci idempotensi ini sudah pernah diproses.");
        }

        // Validasi Rekening Bank
        var bankExists = await _dbContext.MstBankAccounts.AsNoTracking()
            .AnyAsync(b => b.Id == request.BankAccountId && !b.IsDelete && b.IsActive, cancellationToken);
        if (!bankExists)
            throw new ReceivableInvoiceBatchBadRequestException("Rekening bank rumah sakit tidak valid atau tidak aktif.");

        // Validasi Akun COA
        if (request.DiscountAmount > 0)
        {
            if (!request.DiscountChartOfAccountId.HasValue)
                throw new ReceivableInvoiceBatchBadRequestException("Akun COA Diskon wajib dipilih bila nominal diskon lebih besar dari nol.");

            var coaValid = await _dbContext.AccChartOfAccounts.AsNoTracking()
                .AnyAsync(c => c.Id == request.DiscountChartOfAccountId.Value && !c.IsDelete && c.IsActive && c.IsPostable, cancellationToken);
            if (!coaValid)
                throw new ReceivableInvoiceBatchBadRequestException("Akun COA Diskon tidak valid, tidak aktif, atau bukan akun postable.");
        }

        if (request.OtherReceiptAmount > 0)
        {
            if (!request.OtherReceiptChartOfAccountId.HasValue)
                throw new ReceivableInvoiceBatchBadRequestException("Akun COA Penerimaan Lainnya wajib dipilih bila nominal penerimaan lainnya lebih besar dari nol.");

            var coaValid = await _dbContext.AccChartOfAccounts.AsNoTracking()
                .AnyAsync(c => c.Id == request.OtherReceiptChartOfAccountId.Value && !c.IsDelete && c.IsActive && c.IsPostable, cancellationToken);
            if (!coaValid)
                throw new ReceivableInvoiceBatchBadRequestException("Akun COA Penerimaan Lainnya tidak valid, tidak aktif, atau bukan akun postable.");
        }

        if (request.Pph23Amount > 0)
        {
            if (!request.Pph23ChartOfAccountId.HasValue)
                throw new ReceivableInvoiceBatchBadRequestException("Akun COA PPh 23 wajib dipilih bila nominal PPh 23 lebih besar dari nol.");

            var coaValid = await _dbContext.AccChartOfAccounts.AsNoTracking()
                .AnyAsync(c => c.Id == request.Pph23ChartOfAccountId.Value && !c.IsDelete && c.IsActive && c.IsPostable, cancellationToken);
            if (!coaValid)
                throw new ReceivableInvoiceBatchBadRequestException("Akun COA PPh 23 tidak valid, tidak aktif, atau bukan akun postable.");
        }

        if (request.BankAdminFeeAmount > 0)
        {
            if (!request.BankAdminFeeChartOfAccountId.HasValue)
                throw new ReceivableInvoiceBatchBadRequestException("Akun COA Biaya Administrasi Bank wajib dipilih bila nominal biaya admin lebih besar dari nol.");

            var coaValid = await _dbContext.AccChartOfAccounts.AsNoTracking()
                .AnyAsync(c => c.Id == request.BankAdminFeeChartOfAccountId.Value && !c.IsDelete && c.IsActive && c.IsPostable, cancellationToken);
            if (!coaValid)
                throw new ReceivableInvoiceBatchBadRequestException("Akun COA Biaya Administrasi Bank tidak valid, tidak aktif, atau bukan akun postable.");
        }

        // Validasi alokasi anggota
        if (request.MemberAllocations == null || request.MemberAllocations.Count == 0)
            throw new ReceivableInvoiceBatchBadRequestException("Rincian alokasi pembayaran untuk anggota batch wajib diisi.");

        var totalAllocated = request.MemberAllocations.Sum(x => x.Amount);
        if (totalAllocated != request.ReceiptAmount)
            throw new ReceivableInvoiceBatchValidationException($"Total alokasi pembayaran anggota (Rp {totalAllocated:N0}) harus sama dengan nominal penerimaan bank (Rp {request.ReceiptAmount:N0}).");

        if (request.DiscountAmount > 0)
        {
            var totalAllocDiscount = request.MemberAllocations.Sum(x => x.DiscountAmount);
            if (totalAllocDiscount != request.DiscountAmount)
                throw new ReceivableInvoiceBatchValidationException($"Total alokasi diskon anggota (Rp {totalAllocDiscount:N0}) harus sama dengan total nominal diskon (Rp {request.DiscountAmount:N0}).");
        }

        if (request.Pph23Amount > 0)
        {
            var totalAllocPph = request.MemberAllocations.Sum(x => x.Pph23Amount);
            if (totalAllocPph != request.Pph23Amount)
                throw new ReceivableInvoiceBatchValidationException($"Total alokasi PPh 23 anggota (Rp {totalAllocPph:N0}) harus sama dengan total nominal PPh 23 (Rp {request.Pph23Amount:N0}).");
        }

        if (request.BankAdminFeeAmount > 0)
        {
            var totalAllocFee = request.MemberAllocations.Sum(x => x.BankAdminFeeAmount);
            if (totalAllocFee != request.BankAdminFeeAmount)
                throw new ReceivableInvoiceBatchValidationException($"Total alokasi biaya administrasi anggota (Rp {totalAllocFee:N0}) harus sama dengan total nominal biaya admin (Rp {request.BankAdminFeeAmount:N0}).");
        }

        var memberMap = batch.Items
            .Where(x => !x.IsDelete && x.Receivable is not null)
            .ToDictionary(x => x.ReceivableId, x => x.Receivable!);

        foreach (var alloc in request.MemberAllocations)
        {
            if (!memberMap.TryGetValue(alloc.ReceivableId, out var rec))
                throw new ReceivableInvoiceBatchBadRequestException($"Piutang {alloc.ReceivableId} bukan merupakan anggota dari batch tagihan ini.");

            var totalDeductionForRec = alloc.Amount + alloc.DiscountAmount + alloc.Pph23Amount + alloc.BankAdminFeeAmount;
            if (totalDeductionForRec > rec.OutstandingAmount)
                throw new ReceivableInvoiceBatchValidationException(
                    $"Total alokasi dan potongan untuk piutang {rec.ReceivableNumber} (Rp {totalDeductionForRec:N0}) melebihi sisa piutang saat ini (Rp {rec.OutstandingAmount:N0}).");
        }

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);

            await AcquireLockAsync($"FIN_AR_BATCH_{batchId:N}", cancellationToken);
            foreach (var recId in request.MemberAllocations.Select(m => m.ReceivableId).Distinct().OrderBy(id => id))
            {
                await AcquireLockAsync($"FIN_RECEIVABLE_{recId:N}", cancellationToken);
            }

            var receiptNumber = GenerateReceiptNumber();
            var paymentDate = request.PaymentDate ?? DateTimeOffset.UtcNow;

            var receipt = new FinReceipt
            {
                Id = Guid.NewGuid(),
                ReceiptNumber = receiptNumber,
                SourceType = FinReceiptSourceTypes.ArCollection,
                BankAccountId = request.BankAccountId,
                Amount = request.ReceiptAmount,
                AllocatedAmount = request.ReceiptAmount,
                UnallocatedAmount = 0m,
                ProviderReference = request.ReferenceNumber,
                ProviderEventId = idempotencyKey,
                OccurredAt = paymentDate,
                Status = FinReceiptStatuses.Allocated,
                CorrelationId = batch.Id,
                CausationId = batch.Id,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.FinReceipts.Add(receipt);

            var stagedDeductions = new List<FinReceiptDeduction>();

            foreach (var line in request.MemberAllocations)
            {
                var rec = memberMap[line.ReceivableId];

                var allocation = new FinReceiptAllocation
                {
                    Id = Guid.NewGuid(),
                    ReceiptId = receipt.Id,
                    ReceivableId = line.ReceivableId,
                    TargetType = FinReceiptAllocationTargetTypes.Receivable,
                    Amount = line.Amount,
                    IsReversal = false,
                    AllocatedBy = actorUserId,
                    AllocatedAt = paymentDate,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.FinReceiptAllocations.Add(allocation);

                // 1. Alokasi pembayaran kas/bank
                if (line.Amount > 0)
                {
                    await _receivableService.ApplyAllocationAsync(
                        line.ReceivableId,
                        line.Amount,
                        actorUserId,
                        cancellationToken,
                        movementType: FinReceivableMovementTypes.AlokasiPenerimaan,
                        sourceAllocationId: allocation.Id,
                        referenceNumber: receipt.ReceiptNumber,
                        notes: request.Note);
                }

                // 2. Alokasi diskon invoice
                if (line.DiscountAmount > 0)
                {
                    await _receivableService.ApplyAllocationAsync(
                        line.ReceivableId,
                        line.DiscountAmount,
                        actorUserId,
                        cancellationToken,
                        movementType: FinReceivableMovementTypes.Potongan,
                        sourceAllocationId: allocation.Id,
                        referenceNumber: receipt.ReceiptNumber,
                        notes: $"Diskon AR Invoice: {request.DiscountNote}");
                }

                // 3. Potongan PPh 23
                if (line.Pph23Amount > 0)
                {
                    var pphDeduction = new FinReceiptDeduction
                    {
                        Id = Guid.NewGuid(),
                        DeductionNumber = GenerateDeductionNumber(),
                        ReceiptId = receipt.Id,
                        ReceiptAllocationId = allocation.Id,
                        DeductionType = FinReceiptDeductionTypes.Pph23,
                        ChartOfAccountId = request.Pph23ChartOfAccountId,
                        Amount = line.Pph23Amount,
                        Reason = "PPh 23 Pelunasan AR Invoice",
                        ReferenceNumber = request.ReferenceNumber,
                        IsReversal = false,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = actorUserId
                    };
                    _dbContext.FinReceiptDeductions.Add(pphDeduction);
                    stagedDeductions.Add(pphDeduction);

                    await _receivableService.ApplyAllocationAsync(
                        line.ReceivableId,
                        line.Pph23Amount,
                        actorUserId,
                        cancellationToken,
                        movementType: FinReceivableMovementTypes.Potongan,
                        sourceAllocationId: allocation.Id,
                        referenceNumber: pphDeduction.DeductionNumber,
                        notes: $"{FinReceiptDeductionTypes.Pph23}: PPh 23 Pelunasan AR Invoice");
                }

                // 4. Potongan Biaya Admin Bank
                if (line.BankAdminFeeAmount > 0)
                {
                    var adminDeduction = new FinReceiptDeduction
                    {
                        Id = Guid.NewGuid(),
                        DeductionNumber = GenerateDeductionNumber(),
                        ReceiptId = receipt.Id,
                        ReceiptAllocationId = allocation.Id,
                        DeductionType = FinReceiptDeductionTypes.BankAdminFee,
                        ChartOfAccountId = request.BankAdminFeeChartOfAccountId,
                        Amount = line.BankAdminFeeAmount,
                        Reason = "Biaya Administrasi Bank Pelunasan AR Invoice",
                        ReferenceNumber = request.ReferenceNumber,
                        IsReversal = false,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = actorUserId
                    };
                    _dbContext.FinReceiptDeductions.Add(adminDeduction);
                    stagedDeductions.Add(adminDeduction);

                    await _receivableService.ApplyAllocationAsync(
                        line.ReceivableId,
                        line.BankAdminFeeAmount,
                        actorUserId,
                        cancellationToken,
                        movementType: FinReceivableMovementTypes.Potongan,
                        sourceAllocationId: allocation.Id,
                        referenceNumber: adminDeduction.DeductionNumber,
                        notes: $"{FinReceiptDeductionTypes.BankAdminFee}: Biaya Administrasi Bank");
                }
            }

            // Stage Outbox Events untuk akuntansi
            await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
            {
                EventTypeCode = FinAccountingEventTypeCodes.PenerimaanPiutang,
                SourceTransactionId = receipt.ReceiptNumber,
                EventOccurredAt = paymentDate,
                AccountingDate = FinanceBusinessDate.ToDateOnly(paymentDate),
                Amount = receipt.Amount,
                CorrelationId = batch.Id,
                CausationId = receipt.Id,
                ActorUserId = actorUserId
            }, cancellationToken);

            foreach (var ded in stagedDeductions)
            {
                var eventTypeCode = ded.DeductionType == FinReceiptDeductionTypes.Pph23
                    ? FinAccountingEventTypeCodes.PotonganPph23Piutang
                    : FinAccountingEventTypeCodes.PotonganBiayaBankPiutang;

                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = eventTypeCode,
                    SourceTransactionId = ded.DeductionNumber,
                    EventOccurredAt = paymentDate,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(paymentDate),
                    Amount = ded.Amount,
                    CorrelationId = batch.Id,
                    CausationId = ded.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            // Perbarui header batch
            if (request.DiscountAmount > 0)
            {
                batch.TotalDiscount += request.DiscountAmount;
                batch.DiscountChartOfAccountId = request.DiscountChartOfAccountId;
                batch.DiscountNote = request.DiscountNote;
            }

            if (request.OtherReceiptAmount > 0)
            {
                batch.OtherReceiptAmount += request.OtherReceiptAmount;
                batch.OtherReceiptChartOfAccountId = request.OtherReceiptChartOfAccountId;
                batch.OtherReceiptNote = request.OtherReceiptNote;
            }

            // Cek status pelunasan seluruh anggota batch
            var settledCount = batch.Items.Count(x => !x.IsDelete && x.Receivable != null && x.Receivable.Status == FinReceivableStatuses.Settled);
            var totalActiveCount = batch.Items.Count(x => !x.IsDelete);

            batch.Status = settledCount == totalActiveCount
                ? FinReceivableInvoiceBatchStatuses.Paid
                : FinReceivableInvoiceBatchStatuses.PartiallyPaid;

            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = actorUserId;
            batch.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            await AuditAsync("PostPayment", batch.Id, actorUserId);

            return new PostReceivableInvoiceBatchPaymentResponse
            {
                BatchId = batch.Id,
                ReceiptId = receipt.Id,
                ReceiptNumber = receipt.ReceiptNumber,
                Status = batch.Status,
                TotalReceiptAmount = receipt.Amount,
                TotalDiscountAmount = request.DiscountAmount,
                TotalPph23Amount = request.Pph23Amount,
                TotalBankAdminFeeAmount = request.BankAdminFeeAmount,
                TotalOtherReceiptAmount = request.OtherReceiptAmount,
                NewRowVersion = batch.RowVersion,
                AllocationsCount = request.MemberAllocations.Count
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(transaction);
            throw Stale(ex);
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
    // Database Transaction & Locking Helpers
    // ------------------------------------------------------------------------------------

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    private static string GenerateReceiptNumber()
    {
        var candidate = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string GenerateDeductionNumber()
    {
        var candidate = $"DED-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private IQueryable<FinReceivableInvoiceBatch> ApplyFilter(
        IQueryable<FinReceivableInvoiceBatch> query,
        ReceivableInvoiceBatchQuery request)
    {
        var debtorId = request.CompanyId ?? request.DebtorReferenceId;
        if (debtorId.HasValue)
            query = query.Where(x => x.DebtorReferenceId == debtorId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
        {
            var invPattern = $"%{request.InvoiceNumber.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.BatchNumber, invPattern));
        }

        if (!string.IsNullOrWhiteSpace(request.VisitCode))
        {
            var vc = request.VisitCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.VisitCode == vc || (x.VisitCode == null && EF.Functions.ILike(x.BatchNumber, $"%/{vc}/%")));
        }

        if (request.StartDate.HasValue)
            query = query.Where(x => x.InvoiceDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(x => x.InvoiceDate <= request.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.BatchNumber, pattern)
                || (x.Note != null && EF.Functions.ILike(x.Note, pattern))
                || _dbContext.MstCompanyGuarantors.Any(c => !c.IsDelete && c.Id == x.DebtorReferenceId && EF.Functions.ILike(c.CompanyGuarantorName, pattern))
                || _dbContext.MstInsuranceProviders.Any(p => !p.IsDelete && p.Id == x.DebtorReferenceId && EF.Functions.ILike(p.InsuranceProviderName, pattern)));
        }

        return query;
    }

    private static string? ResolveBatchVisitCode(FinReceivableInvoiceBatch b)
    {
        if (!string.IsNullOrWhiteSpace(b.VisitCode)) return b.VisitCode;
        if (b.BatchNumber.Contains("/IP/", StringComparison.OrdinalIgnoreCase)) return "IP";
        if (b.BatchNumber.Contains("/OP/", StringComparison.OrdinalIgnoreCase)) return "OP";
        return null;
    }

    private static string? ResolveVisitLabel(string? visitCode) => visitCode switch
    {
        "IP" => "Rawat Inap",
        "OP" => "Rawat Jalan",
        _ => null
    };

    // ====================================================================================
    // CANCELED INVOICE WORKFLOW
    // ====================================================================================

    public async Task<CanceledInvoicePagedResponse> GetCanceledPagedAsync(
        CanceledInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == FinReceivableInvoiceBatchStatuses.Cancelled);

        query = ApplyCanceledFilter(query, request);

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "batchnumber" or "invoicenumber" => descending ? query.OrderByDescending(x => x.BatchNumber) : query.OrderBy(x => x.BatchNumber),
            "totalamount" => descending ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
            "invoicedate" => descending ? query.OrderByDescending(x => x.InvoiceDate) : query.OrderBy(x => x.InvoiceDate),
            "duedate" => descending ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate),
            "canceldatetime" or "cancelledat" => descending ? query.OrderByDescending(x => x.CancelDateTime) : query.OrderBy(x => x.CancelDateTime),
            "servicetype" => descending ? query.OrderByDescending(x => x.ServiceType) : query.OrderBy(x => x.ServiceType),
            _ => descending ? query.OrderByDescending(x => x.CancelDateTime ?? x.CreateDateTime) : query.OrderBy(x => x.CancelDateTime ?? x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;

        var pagedRows = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Resolve Debtor Names & User Names
        var debtorIds = pagedRows.Select(r => r.DebtorReferenceId).Distinct().ToList();
        var companyNames = debtorIds.Count > 0
            ? await _dbContext.MstCompanyGuarantors.AsNoTracking()
                .Where(c => debtorIds.Contains(c.Id) && !c.IsDelete)
                .ToDictionaryAsync(c => c.Id, c => c.CompanyGuarantorName, cancellationToken)
            : new Dictionary<Guid, string>();
        var insuranceNames = debtorIds.Count > 0
            ? await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(p => debtorIds.Contains(p.Id) && !p.IsDelete)
                .ToDictionaryAsync(p => p.Id, p => p.InsuranceProviderName, cancellationToken)
            : new Dictionary<Guid, string>();

        var userIds = pagedRows.Select(r => r.CancelBy).Concat(pagedRows.Select(r => r.CreateBy)).Where(u => u != Guid.Empty).Distinct().ToList();
        var userNames = userIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName ?? "-", cancellationToken)
            : new Dictionary<Guid, string>();

        // Check active receipts/tender for the paged batch rows
        var batchIds = pagedRows.Select(b => b.Id).ToList();
        var batchMemberReceivableIds = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
            .Where(i => batchIds.Contains(i.BatchId) && !i.IsDelete)
            .Select(i => new { i.BatchId, i.ReceivableId })
            .ToListAsync(cancellationToken);

        var allRecIds = batchMemberReceivableIds.Select(x => x.ReceivableId).Distinct().ToList();
        var paidReceivableIds = allRecIds.Count > 0
            ? await _dbContext.FinReceiptAllocations.AsNoTracking()
                .Where(a => allRecIds.Contains(a.ReceivableId!.Value) && !a.IsDelete && !a.IsReversal && a.Amount > 0)
                .Select(a => a.ReceivableId!.Value)
                .Distinct()
                .ToHashSetAsync(cancellationToken)
            : new HashSet<Guid>();

        var items = pagedRows.Select(b =>
        {
            string? debtorName = null;
            string? debtorKind = null;
            if (companyNames.TryGetValue(b.DebtorReferenceId, out var cName))
            {
                debtorName = cName;
                debtorKind = "COMPANY";
            }
            else if (insuranceNames.TryGetValue(b.DebtorReferenceId, out var iName))
            {
                debtorName = iName;
                debtorKind = "INSURANCE";
            }

            var serviceType = !string.IsNullOrWhiteSpace(b.ServiceType) ? b.ServiceType : (b.VisitCode == "IP" ? "RANAP" : (b.VisitCode == "OP" ? "RAJAL" : null));
            var serviceTypeName = ResolveServiceTypeName(serviceType, b.VisitCode);

            var recsForBatch = batchMemberReceivableIds.Where(x => x.BatchId == b.Id).Select(x => x.ReceivableId).ToList();
            var hasReceipt = recsForBatch.Any(r => paidReceivableIds.Contains(r));

            userNames.TryGetValue(b.CancelBy, out var cancelledByName);

            return new CanceledInvoiceRowResponse
            {
                Id = b.Id,
                InvoiceNumber = b.BatchNumber,
                DebtorReferenceId = b.DebtorReferenceId,
                DebtorName = debtorName,
                DebtorKind = debtorKind,
                ServiceType = serviceType,
                ServiceTypeName = serviceTypeName,
                InvoiceDate = b.InvoiceDate,
                CancelledAt = b.CancelDateTime,
                DueDate = b.DueDate,
                TotalAmount = b.TotalAmount,
                CancelledBy = b.CancelBy,
                CancelledByName = cancelledByName,
                CancelReason = b.CancelReason,
                ReissuedToBatchId = b.ReissuedToBatchId,
                ReissuedFromBatchId = b.ReissuedFromBatchId,
                RowVersion = b.RowVersion,
                DocumentCapabilities = new CanceledInvoiceDocumentCapabilities
                {
                    Invoice = new CanceledInvoiceCapabilityItem { Available = true },
                    ReceiptAcknowledgement = new CanceledInvoiceCapabilityItem { Available = true },
                    CashierReceipt = new CanceledInvoiceCapabilityItem
                    {
                        Available = hasReceipt,
                        Reason = hasReceipt ? null : "Belum ada pembayaran/tender yang dapat dibuat Kwitansi."
                    },
                    BillingRecap = new CanceledInvoiceCapabilityItem { Available = true }
                }
            };
        }).ToList();

        // Summary calculated over all filtered records
        var summary = await GetCanceledSummaryAsync(request, cancellationToken);

        return new CanceledInvoicePagedResponse
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items,
            Summary = summary
        };
    }

    public async Task<CanceledInvoiceSummaryResponse> GetCanceledSummaryAsync(
        CanceledInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == FinReceivableInvoiceBatchStatuses.Cancelled);

        query = ApplyCanceledFilter(query, request);

        var agg = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(x => x.TotalAmount),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        string targetName = "Semua Perusahaan";
        if (request.DebtorReferenceId.HasValue && request.DebtorReferenceId.Value != Guid.Empty)
        {
            var cName = await _dbContext.MstCompanyGuarantors.AsNoTracking()
                .Where(c => c.Id == request.DebtorReferenceId.Value && !c.IsDelete)
                .Select(c => c.CompanyGuarantorName)
                .FirstOrDefaultAsync(cancellationToken);

            targetName = cName ?? await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(p => p.Id == request.DebtorReferenceId.Value && !p.IsDelete)
                .Select(p => p.InsuranceProviderName)
                .FirstOrDefaultAsync(cancellationToken) ?? "Semua Perusahaan";
        }

        var totalAmount = agg?.TotalAmount ?? 0m;
        return new CanceledInvoiceSummaryResponse
        {
            TotalAmount = totalAmount,
            InvoiceCount = agg?.Count ?? 0,
            TargetAmount = totalAmount,
            TargetName = targetName
        };
    }

    public async Task<CanceledInvoiceDetailResponse> GetCanceledDetailAsync(
        Guid id, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Canceled Invoice tidak ditemukan.");

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            throw new ReceivableInvoiceBatchValidationException("Batch tagihan bukan berstatus CANCELLED.");

        // Debtor Name
        string? debtorName = null;
        string? debtorKind = null;
        var comp = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == batch.DebtorReferenceId && !x.IsDelete)
            .Select(x => x.CompanyGuarantorName)
            .FirstOrDefaultAsync(cancellationToken);
        if (comp != null)
        {
            debtorName = comp;
            debtorKind = "COMPANY";
        }
        else
        {
            var ins = await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(x => x.Id == batch.DebtorReferenceId && !x.IsDelete)
                .Select(x => x.InsuranceProviderName)
                .FirstOrDefaultAsync(cancellationToken);
            if (ins != null)
            {
                debtorName = ins;
                debtorKind = "INSURANCE";
            }
        }

        // Cancelled by User
        string? cancelledByName = null;
        if (batch.CancelBy != Guid.Empty)
        {
            cancelledByName = await _dbContext.Users.AsNoTracking()
                .Where(u => u.Id == batch.CancelBy)
                .Select(u => u.FullName ?? u.UserName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var serviceType = !string.IsNullOrWhiteSpace(batch.ServiceType) ? batch.ServiceType : (batch.VisitCode == "IP" ? "RANAP" : (batch.VisitCode == "OP" ? "RAJAL" : null));
        var serviceTypeName = ResolveServiceTypeName(serviceType, batch.VisitCode);

        // Members & Entities
        var memberReceivables = batch.Items
            .Where(x => !x.IsDelete && x.Receivable != null)
            .Select(x => x.Receivable!)
            .ToList();
        var receivableIds = memberReceivables.Select(r => r.Id).ToList();

        var receivableItems = await _dbContext.FinReceivableItems.AsNoTracking()
            .Where(x => !x.IsDelete && receivableIds.Contains(x.ReceivableId))
            .ToListAsync(cancellationToken);

        var invoiceIds = memberReceivables.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value)
            .Concat(receivableItems.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value))
            .Distinct()
            .ToList();

        var invoices = invoiceIds.Count > 0
            ? await _dbContext.BilInvoices.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, BilInvoice>();

        var encounterIds = invoices.Values.Select(x => x.EncounterId)
            .Concat(receivableItems.Where(x => x.EncounterId.HasValue).Select(x => x.EncounterId!.Value))
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        var encounters = encounterIds.Count > 0
            ? await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(x => encounterIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, RegPatientEncounter>();

        var patientIds = encounters.Values.Select(x => x.PatientId)
            .Concat(receivableItems.Where(x => x.PatientId.HasValue).Select(x => x.PatientId!.Value))
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        var patients = patientIds.Count > 0
            ? await _dbContext.MstPatients.AsNoTracking()
                .Where(x => patientIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, MstPatient>();

        // Pending adjustments
        var pendingAdjustments = await _dbContext.FinReceivableAdjustments.AsNoTracking()
            .Where(a => !a.IsDelete && receivableIds.Contains(a.ReceivableId) && a.Status == FinReceivableApprovalStatuses.Requested)
            .Select(a => a.ReceivableId)
            .ToHashSetAsync(cancellationToken);

        // Check payments/tender
        var hasPaymentAllocations = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .AnyAsync(a => !a.IsDelete && !a.IsReversal && receivableIds.Contains(a.ReceivableId!.Value) && a.Amount > 0, cancellationToken);

        var memberResponses = batch.Items
            .Where(x => !x.IsDelete && x.Receivable != null)
            .Select(x =>
            {
                var r = x.Receivable!;
                var itemsForRec = receivableItems.Where(i => i.ReceivableId == r.Id).ToList();
                Guid? invId = r.InvoiceId ?? itemsForRec.FirstOrDefault(i => i.InvoiceId.HasValue)?.InvoiceId;
                BilInvoice? inv = invId.HasValue && invoices.TryGetValue(invId.Value, out var foundInv) ? foundInv : null;

                Guid? encId = inv?.EncounterId ?? itemsForRec.FirstOrDefault(i => i.EncounterId.HasValue)?.EncounterId;
                RegPatientEncounter? enc = encId.HasValue && encounters.TryGetValue(encId.Value, out var foundEnc) ? foundEnc : null;

                Guid? patId = enc?.PatientId ?? itemsForRec.FirstOrDefault(i => i.PatientId.HasValue)?.PatientId;
                MstPatient? pat = patId.HasValue && patients.TryGetValue(patId.Value, out var foundPat) ? foundPat : null;

                bool hasPendingAdj = pendingAdjustments.Contains(r.Id);
                bool isReissued = batch.ReissuedToBatchId.HasValue;

                bool canEdit = true;
                string? editBlockedReason = null;

                if (isReissued)
                {
                    canEdit = false;
                    editBlockedReason = "Batch tagihan telah dibuat ulang.";
                }
                else if (hasPendingAdj)
                {
                    canEdit = false;
                    editBlockedReason = "Terdapat pengajuan koreksi piutang yang masih menunggu persetujuan.";
                }
                else if (r.AllocatedAmount > 0)
                {
                    canEdit = false;
                    editBlockedReason = "Nilai tagihan tidak dapat diubah karena piutang telah memiliki alokasi pembayaran. Gunakan proses koreksi piutang.";
                }
                else if (r.WrittenOffAmount > 0)
                {
                    canEdit = false;
                    editBlockedReason = "Nilai tagihan tidak dapat diubah karena piutang memiliki penghapusan (write-off).";
                }

                var effectiveAmount = r.OriginalAmount - r.AdjustedAmount;

                return new CanceledInvoiceMemberDetailResponse
                {
                    BatchItemId = x.Id,
                    ReceivableId = r.Id,
                    InvoiceId = invId,
                    EncounterId = encId,
                    PatientId = patId,
                    PatientName = pat?.FullName ?? "-",
                    MedicalRecordNumber = pat?.MedicalRecordNumber ?? "-",
                    BillingNumber = inv?.InvoiceNumber ?? "-",
                    RegistrationNumber = enc?.EncounterNumber ?? "-",
                    OriginalAmount = r.OriginalAmount,
                    OutstandingAmount = r.OutstandingAmount,
                    AllocatedAmount = r.AllocatedAmount,
                    AdjustedAmount = r.AdjustedAmount,
                    WrittenOffAmount = r.WrittenOffAmount,
                    EditableTotalAmount = effectiveAmount,
                    DiscountAmount = 0m,
                    FinalAmount = effectiveAmount,
                    ReceivableStatus = r.Status,
                    CanEdit = canEdit,
                    EditBlockedReason = editBlockedReason,
                    ActionCapabilities = new CanceledInvoiceMemberActionCapabilities
                    {
                        Print = new CanceledInvoiceCapabilityItem { Available = true },
                        CareBill = new CanceledInvoiceCapabilityItem
                        {
                            Available = invId.HasValue,
                            Reason = invId.HasValue ? null : "Data migrasi tidak memiliki invoice Billing sumber."
                        },
                        CostDetail = new CanceledInvoiceCapabilityItem
                        {
                            Available = invId.HasValue,
                            Reason = invId.HasValue ? null : "Data migrasi tidak memiliki invoice Billing sumber."
                        }
                    }
                };
            }).ToList();

        bool canReissue = !batch.ReissuedToBatchId.HasValue && pendingAdjustments.Count == 0;
        string? reissueBlockedReason = null;
        if (batch.ReissuedToBatchId.HasValue)
        {
            reissueBlockedReason = "Batch tagihan ini sudah pernah dibuat ulang.";
        }
        else if (pendingAdjustments.Count > 0)
        {
            reissueBlockedReason = "Perubahan nominal masih menunggu persetujuan koreksi piutang.";
        }

        return new CanceledInvoiceDetailResponse
        {
            Id = batch.Id,
            InvoiceNumber = batch.BatchNumber,
            DebtorReferenceId = batch.DebtorReferenceId,
            DebtorName = debtorName,
            DebtorKind = debtorKind,
            ServiceType = serviceType,
            ServiceTypeName = serviceTypeName,
            InvoiceDate = batch.InvoiceDate,
            SentDate = batch.IssuedAt,
            DueDate = batch.DueDate,
            CancelledAt = batch.CancelDateTime,
            CancelReason = batch.CancelReason,
            CancelledBy = batch.CancelBy,
            CancelledByName = cancelledByName,
            TotalAmount = batch.TotalAmount,
            RowVersion = batch.RowVersion,
            ReissuedToBatchId = batch.ReissuedToBatchId,
            ReissuedFromBatchId = batch.ReissuedFromBatchId,
            CanReissue = canReissue,
            ReissueBlockedReason = reissueBlockedReason,
            DocumentCapabilities = new CanceledInvoiceDocumentCapabilities
            {
                Invoice = new CanceledInvoiceCapabilityItem { Available = true },
                ReceiptAcknowledgement = new CanceledInvoiceCapabilityItem { Available = true },
                CashierReceipt = new CanceledInvoiceCapabilityItem
                {
                    Available = hasPaymentAllocations,
                    Reason = hasPaymentAllocations ? null : "Belum ada pembayaran/tender yang dapat dibuat Kwitansi."
                },
                BillingRecap = new CanceledInvoiceCapabilityItem { Available = true }
            },
            Items = memberResponses
        };
    }

    public async Task<EditCanceledInvoiceMemberAmountResponse> EditCanceledMemberAmountAsync(
        Guid batchId, EditCanceledInvoiceMemberAmountRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Canceled Invoice tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, request.ExpectedRowVersion);

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            throw new ReceivableInvoiceBatchValidationException("Hanya batch tagihan berstatus CANCELLED yang dapat diedit nominalnya.");

        if (batch.ReissuedToBatchId.HasValue)
            throw new ReceivableInvoiceBatchConflictException("Batch tagihan telah dibuat ulang dan tidak dapat diedit lagi.");

        if (request.TotalTagihan < 0m)
            throw new ReceivableInvoiceBatchBadRequestException("Total Tagihan tidak boleh kurang dari nol.");

        if (request.DiscountAmount < 0m)
            throw new ReceivableInvoiceBatchBadRequestException("Nilai Diskon tidak boleh kurang dari nol.");

        if (request.DiscountAmount > request.TotalTagihan)
            throw new ReceivableInvoiceBatchValidationException("Nilai Diskon tidak boleh melebihi Total Tagihan.");

        var finalAmount = request.TotalTagihan - request.DiscountAmount;
        if (finalAmount < 0m)
            throw new ReceivableInvoiceBatchBadRequestException("Nilai akhir tagihan tidak boleh kurang dari nol.");

        var item = batch.Items.FirstOrDefault(x => x.Id == request.BatchItemId && !x.IsDelete)
            ?? throw new KeyNotFoundException("Item anggota batch tidak ditemukan.");

        var receivable = item.Receivable
            ?? await _dbContext.FinReceivables.SingleOrDefaultAsync(x => x.Id == item.ReceivableId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Piutang anggota tidak ditemukan.");

        if (receivable.AllocatedAmount > 0m)
            throw new ReceivableInvoiceBatchValidationException("Nilai tagihan tidak dapat diubah karena piutang telah memiliki alokasi pembayaran. Gunakan proses koreksi piutang.");

        if (receivable.WrittenOffAmount > 0m)
            throw new ReceivableInvoiceBatchValidationException("Nilai tagihan tidak dapat diubah karena piutang memiliki penghapusan (write-off).");

        var hasPendingAdj = await _dbContext.FinReceivableAdjustments.AsNoTracking()
            .AnyAsync(a => !a.IsDelete && a.ReceivableId == receivable.Id && a.Status == FinReceivableApprovalStatuses.Requested, cancellationToken);
        if (hasPendingAdj)
            throw new ReceivableInvoiceBatchValidationException("Terdapat pengajuan koreksi piutang yang masih menunggu persetujuan pada piutang ini.");

        var currentEffective = receivable.OriginalAmount - receivable.AdjustedAmount;
        var diff = finalAmount - currentEffective;

        if (diff == 0m)
        {
            return new EditCanceledInvoiceMemberAmountResponse
            {
                BatchItemId = item.Id,
                ReceivableId = receivable.Id,
                NewTotalTagihan = request.TotalTagihan,
                NewDiscountAmount = request.DiscountAmount,
                NewFinalAmount = finalAmount,
                Message = "Tidak ada perubahan nilai finansial pada piutang."
            };
        }

        var direction = diff < 0m ? FinReceivableAdjustmentDirections.Credit : FinReceivableAdjustmentDirections.Debit;
        var adjAmount = Math.Abs(diff);
        var reasonText = string.IsNullOrWhiteSpace(request.Reason)
            ? $"Penyesuaian tagihan Canceled Invoice {batch.BatchNumber}"
            : request.Reason.Trim();

        var adjustment = await _receivableService.RequestAdjustmentAsync(
            receivable.Id, direction, adjAmount, reasonText, actorUserId, cancellationToken);

        return new EditCanceledInvoiceMemberAmountResponse
        {
            BatchItemId = item.Id,
            ReceivableId = receivable.Id,
            NewTotalTagihan = request.TotalTagihan,
            NewDiscountAmount = request.DiscountAmount,
            NewFinalAmount = finalAmount,
            AdjustmentId = adjustment.Id,
            AdjustmentNumber = adjustment.AdjustmentNumber,
            AdjustmentStatus = adjustment.Status,
            Message = $"Pengajuan koreksi piutang ({direction}) berhasil dibuat dan menunggu persetujuan."
        };
    }

    public async Task<FinReceivableInvoiceBatch> ReissueAsync(
        Guid batchId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Canceled Invoice tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            throw new ReceivableInvoiceBatchValidationException("Hanya batch tagihan berstatus CANCELLED yang dapat dibuat ulang (reissue).");

        if (batch.ReissuedToBatchId.HasValue)
            throw new ReceivableInvoiceBatchConflictException("Batch tagihan ini sudah pernah dibuat ulang sebelumnya.");

        var memberReceivableIds = batch.Items.Where(x => !x.IsDelete).Select(x => x.ReceivableId).Distinct().ToList();
        if (memberReceivableIds.Count == 0)
            throw new ReceivableInvoiceBatchValidationException("Batch tagihan tidak memiliki anggota piutang.");

        // Cek apakah ada adjustment yang masih REQUESTED
        var hasPendingAdjustment = await _dbContext.FinReceivableAdjustments.AsNoTracking()
            .AnyAsync(a => !a.IsDelete && memberReceivableIds.Contains(a.ReceivableId) && a.Status == FinReceivableApprovalStatuses.Requested, cancellationToken);
        if (hasPendingAdjustment)
            throw new ReceivableInvoiceBatchValidationException("Perubahan nominal masih menunggu persetujuan koreksi piutang.");

        // Cek apakah receivable sudah tergabung di batch aktif lain
        var alreadyInActiveBatch = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActiveMembership && memberReceivableIds.Contains(x.ReceivableId))
            .Select(x => x.Receivable!.ReceivableNumber)
            .ToListAsync(cancellationToken);
        if (alreadyInActiveBatch.Count > 0)
            throw new ReceivableInvoiceBatchConflictException($"Piutang {string.Join(", ", alreadyInActiveBatch)} sudah tergabung dalam batch tagihan aktif lain.");

        var receivables = await _dbContext.FinReceivables
            .Where(r => !r.IsDelete && memberReceivableIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        var effectiveInvoiceDate = FinanceBusinessDate.Today();
        var paymentTermDays = batch.PaymentTermDays ?? 30;
        var batchDueDate = effectiveInvoiceDate.AddDays(paymentTermDays);

        var visitCode = batch.VisitCode ?? "OP";
        var newInvoiceNumber = await GenerateArInvoiceNumberAsync(
            visitCode,
            effectiveInvoiceDate,
            actorUserId,
            cancellationToken);

        var newTotalAmount = receivables.Sum(r => r.OriginalAmount - r.AdjustedAmount);

        var newBatch = new FinReceivableInvoiceBatch
        {
            Id = Guid.NewGuid(),
            BatchNumber = newInvoiceNumber,
            DebtorType = batch.DebtorType,
            DebtorReferenceId = batch.DebtorReferenceId,
            VisitCode = visitCode,
            ServiceType = batch.ServiceType,
            PeriodStart = batch.PeriodStart,
            PeriodEnd = batch.PeriodEnd,
            InvoiceDate = effectiveInvoiceDate,
            DueDate = batchDueDate,
            PaymentTermDays = paymentTermDays,
            Note = string.IsNullOrWhiteSpace(batch.Note) ? $"Buat ulang dari {batch.BatchNumber}" : $"Buat ulang dari {batch.BatchNumber}. {batch.Note}".Trim(),
            TotalAmount = newTotalAmount,
            Status = FinReceivableInvoiceBatchStatuses.Draft,
            ReissuedFromBatchId = batch.Id,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            UpdateBy = actorUserId,
            DeleteBy = actorUserId,
            CancelBy = actorUserId
        };

        foreach (var r in receivables)
        {
            newBatch.Items.Add(new FinReceivableInvoiceBatchItem
            {
                Id = Guid.NewGuid(),
                BatchId = newBatch.Id,
                ReceivableId = r.Id,
                IsActiveMembership = true,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                DeleteBy = actorUserId,
                CancelBy = actorUserId
            });
        }

        batch.ReissuedToBatchId = newBatch.Id;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.RowVersion = Guid.NewGuid();

        _dbContext.FinReceivableInvoiceBatches.Add(newBatch);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Reissue", batch.Id, actorUserId);
        return newBatch;
    }

    private IQueryable<FinReceivableInvoiceBatch> ApplyCanceledFilter(
        IQueryable<FinReceivableInvoiceBatch> query, CanceledInvoiceBatchQuery request)
    {
        if (request.DebtorReferenceId.HasValue && request.DebtorReferenceId.Value != Guid.Empty)
            query = query.Where(x => x.DebtorReferenceId == request.DebtorReferenceId.Value);

        if (!string.IsNullOrWhiteSpace(request.ServiceType))
        {
            var st = request.ServiceType.Trim().ToUpperInvariant();
            if (st is "RANAP" or "IP")
                query = query.Where(x => x.ServiceType == "RANAP" || x.VisitCode == "IP");
            else if (st is "RAJAL" or "OP")
                query = query.Where(x => x.ServiceType == "RAJAL" || x.VisitCode == "OP");
            else if (st is "IGD")
                query = query.Where(x => x.ServiceType == "IGD");
            else
                query = query.Where(x => x.ServiceType == st);
        }

        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            var isCancelDate = string.Equals(request.DateType, "CANCEL_DATE", StringComparison.OrdinalIgnoreCase);
            if (isCancelDate)
            {
                if (request.StartDate.HasValue)
                {
                    var startDt = request.StartDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                    query = query.Where(x => x.CancelDateTime >= startDt);
                }
                if (request.EndDate.HasValue)
                {
                    var endDt = request.EndDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
                    query = query.Where(x => x.CancelDateTime <= endDt);
                }
            }
            else // INVOICE_DATE
            {
                if (request.StartDate.HasValue)
                    query = query.Where(x => x.InvoiceDate >= request.StartDate.Value);
                if (request.EndDate.HasValue)
                    query = query.Where(x => x.InvoiceDate <= request.EndDate.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var cat = request.Category.Trim().ToUpperInvariant();
            if (cat is "PATIENT" or "EMPLOYEE" or "PASIEN" or "KARYAWAN")
            {
                // Current V2 FinReceivableInvoiceBatch invariant is DebtorType = PAYER.
                query = query.Where(x => false);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.BatchNumber, pattern)
                || (x.Note != null && EF.Functions.ILike(x.Note, pattern))
                || (x.CancelReason != null && EF.Functions.ILike(x.CancelReason, pattern))
                || _dbContext.MstCompanyGuarantors.Any(c => !c.IsDelete && c.Id == x.DebtorReferenceId && EF.Functions.ILike(c.CompanyGuarantorName, pattern))
                || _dbContext.MstInsuranceProviders.Any(p => !p.IsDelete && p.Id == x.DebtorReferenceId && EF.Functions.ILike(p.InsuranceProviderName, pattern)));
        }

        return query;
    }

    private static string ResolveServiceTypeName(string? serviceType, string? visitCode)
    {
        if (!string.IsNullOrWhiteSpace(serviceType))
        {
            return serviceType.ToUpperInvariant() switch
            {
                "RANAP" => "Rawat Inap",
                "RAJAL" => "Rawat Jalan",
                "IGD" => "IGD",
                "OTC" => "OTC",
                "IP" => "Rawat Inap",
                "OP" => "Rawat Jalan",
                _ => serviceType
            };
        }

        return visitCode?.ToUpperInvariant() switch
        {
            "IP" => "Rawat Inap",
            "OP" => "Rawat Jalan",
            _ => "-"
        };
    }
}

public sealed class ReceivableInvoiceBatchBadRequestException(string message) : Exception(message);
public sealed class ReceivableInvoiceBatchValidationException(string message) : Exception(message);
public sealed class ReceivableInvoiceBatchConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
