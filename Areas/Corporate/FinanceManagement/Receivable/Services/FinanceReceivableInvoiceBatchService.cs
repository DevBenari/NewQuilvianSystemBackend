using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;
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
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using System.Globalization;

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
    private static readonly CultureInfo IdCulture = new("id-ID");
    private const string LogCategory = "Corporate.FinanceManagement.Receivable";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly BillingCompanyGuarantorInvoiceDocumentService _documentService;
    private readonly NumberSeriesAllocator _numberSeriesAllocator;
    private readonly FinanceReceivableService _receivableService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;
    private readonly FinanceReceiptService _receiptService;

    public FinanceReceivableInvoiceBatchService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        BillingCompanyGuarantorInvoiceDocumentService documentService,
        NumberSeriesAllocator numberSeriesAllocator,
        FinanceReceivableService receivableService,
        FinanceAccountingOutboxService accountingOutboxService,
        FinanceReceiptService receiptService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _documentService = documentService;
        _numberSeriesAllocator = numberSeriesAllocator;
        _receivableService = receivableService;
        _accountingOutboxService = accountingOutboxService;
        _receiptService = receiptService;
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
            mapped.VisitCode = visitCode ?? string.Empty;
            mapped.VisitLabel = ResolveVisitLabel(visitCode) ?? string.Empty;
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
                .Select(x => new BatchMemberEncounterRow
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    DoctorId = x.DoctorId,
                    PatientClassId = x.PatientClassId,
                    EncounterNumber = x.EncounterNumber,
                    EncounterType = x.EncounterType,
                    EncounterDate = x.EncounterDate
                })
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, BatchMemberEncounterRow>();

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

        var doctorIds = encounters.Values.Where(e => e.DoctorId.HasValue).Select(e => e.DoctorId!.Value).Distinct().ToList();
        var doctors = doctorIds.Count > 0
            ? await _dbContext.MstDoctors.AsNoTracking()
                .Where(d => doctorIds.Contains(d.Id) && !d.IsDelete)
                .ToDictionaryAsync(d => d.Id, d => d.FullName, cancellationToken)
            : new Dictionary<Guid, string>();

        var patientClassIds = encounters.Values.Where(e => e.PatientClassId.HasValue).Select(e => e.PatientClassId!.Value).Distinct().ToList();
        var patientClasses = patientClassIds.Count > 0
            ? await _dbContext.MstPatientClasses.AsNoTracking()
                .Where(c => patientClassIds.Contains(c.Id) && !c.IsDelete)
                .ToDictionaryAsync(c => c.Id, c => c.PatientClassName, cancellationToken)
            : new Dictionary<Guid, string>();

        var encounterDiagnoses = encounterIds.Count > 0
            ? await _dbContext.TrxPatientDiagnoses.AsNoTracking()
                .Where(d => encounterIds.Contains(d.EncounterId) && !d.IsDelete)
                .OrderByDescending(d => d.IsPrimary)
                .ThenBy(d => d.SortOrder)
                .ToListAsync(cancellationToken)
            : new List<TrxPatientDiagnosis>();

        var members = batch.Items
            .Where(x => !x.IsDelete && x.Receivable is not null)
            .Select(x =>
            {
                var r = x.Receivable!;
                var itemsForRec = receivableItems.Where(i => i.ReceivableId == r.Id).ToList();
                Guid? invId = r.InvoiceId ?? itemsForRec.FirstOrDefault(i => i.InvoiceId.HasValue)?.InvoiceId;
                BilInvoice? inv = invId.HasValue && invoices.TryGetValue(invId.Value, out var foundInv) ? foundInv : null;

                Guid? encId = inv?.EncounterId ?? itemsForRec.FirstOrDefault(i => i.EncounterId.HasValue)?.EncounterId;
                BatchMemberEncounterRow? enc = encId.HasValue && encounters.TryGetValue(encId.Value, out var foundEnc) ? foundEnc : null;

                Guid? patId = enc?.PatientId ?? itemsForRec.FirstOrDefault(i => i.PatientId.HasValue)?.PatientId;
                MstPatient? pat = patId.HasValue && patients.TryGetValue(patId.Value, out var foundPat) ? foundPat : null;

                var billingDate = inv?.InvoiceDate.HasValue == true
                    ? DateOnly.FromDateTime(inv.InvoiceDate.Value.LocalDateTime)
                    : r.DueDate.AddDays(-30);

                var encDiags = encId.HasValue ? encounterDiagnoses.Where(d => d.EncounterId == encId.Value).ToList() : new List<TrxPatientDiagnosis>();
                var primaryDiag = encDiags.FirstOrDefault(d => d.IsPrimary) ?? encDiags.FirstOrDefault();
                var secondaryDiags = encDiags.Where(d => d != primaryDiag).Select(d => $"{d.DiagnosisCode} - {d.DiagnosisName}").ToList();

                string? doctorName = enc?.DoctorId.HasValue == true && doctors.TryGetValue(enc.DoctorId.Value, out var doc) ? doc : null;
                string? className = enc?.PatientClassId.HasValue == true && patientClasses.TryGetValue(enc.PatientClassId.Value, out var cls) ? cls : null;

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
                    Note = null,
                    EncounterNumber = enc?.EncounterNumber ?? "-",
                    ServiceType = enc != null ? (enc.EncounterType == EncounterType.Inpatient ? "RANAP" : enc.EncounterType == EncounterType.Emergency ? "IGD" : "RAJAL") : (visitCode == "IP" ? "RANAP" : "RAJAL"),
                    ServiceLabel = enc != null ? (enc.EncounterType == EncounterType.Inpatient ? "Rawat Inap" : enc.EncounterType == EncounterType.Emergency ? "IGD" : "Rawat Jalan") : (visitCode == "IP" ? "Rawat Inap" : "Rawat Jalan"),
                    ServiceDate = enc != null ? DateOnly.FromDateTime(enc.EncounterDate) : billingDate,
                    PatientClassName = className ?? "-",
                    DoctorName = doctorName ?? "-",
                    PrimaryDiagnosisCode = primaryDiag?.DiagnosisCode,
                    PrimaryDiagnosisName = primaryDiag?.DiagnosisName,
                    SecondaryDiagnoses = secondaryDiags.Count > 0 ? string.Join(", ", secondaryDiags) : null
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
                        ReversalOfAllocationId = a.ReversalOfAllocationId,
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

        var reversedAllocationIds = allocations
            .Where(a => a.IsReversal && a.ReversalOfAllocationId.HasValue)
            .Select(a => a.ReversalOfAllocationId!.Value)
            .ToHashSet();

        var activeAllocations = allocations
            .Where(a => !a.IsReversal && a.Amount > 0 && !reversedAllocationIds.Contains(a.Id))
            .ToList();

        var hasActivePayment = activeAllocations.Count > 0;
        var activePaymentCount = activeAllocations.Count;

        bool isCancelled = mapped.Status == FinReceivableInvoiceBatchStatuses.Cancelled;
        bool isPaid = mapped.Status == FinReceivableInvoiceBatchStatuses.Paid;
        bool isPartiallyPaid = mapped.Status == FinReceivableInvoiceBatchStatuses.PartiallyPaid;
        bool isIssued = mapped.Status == FinReceivableInvoiceBatchStatuses.Issued;
        bool isDraft = mapped.Status == FinReceivableInvoiceBatchStatuses.Draft;

        string invoiceStatusText = isCancelled ? "Dibatalkan" : "Aktif";
        string paymentStatus = isPaid ? "Lunas" : (isPartiallyPaid ? "Sebagian Lunas" : "Belum Lunas");

        bool canProcessPayment = !isCancelled && !isPaid && !hasActivePayment;
        bool canCancel = !isCancelled && (isDraft || isIssued || isPartiallyPaid || isPaid);
        bool canRevive = isCancelled;

        string? cancelWarning = hasActivePayment
            ? "Invoice ini sudah memiliki pembayaran aktif. Seluruh pembayaran aktif akan dibalik dan saldo piutang akan dikembalikan."
            : null;

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
            VisitCode = visitCode ?? string.Empty,
            VisitLabel = ResolveVisitLabel(visitCode) ?? string.Empty,
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
            CanProcessPayment = canProcessPayment,
            CanCancel = canCancel,
            HasActivePayment = hasActivePayment,
            ActivePaymentCount = activePaymentCount,
            CanRevive = canRevive,
            CancelWarning = cancelWarning,
            PaymentStatus = paymentStatus,
            InvoiceStatusText = invoiceStatusText,
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
                // Porsi Benefit RS internal bukan tagihan ke pihak luar: diselesaikan lewat Penutupan Benefit.
                && !x.IsInternalBenefit
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

        // Porsi Benefit Karyawan RS (PAYER dengan IsInternalBenefit) adalah biaya rumah sakit sendiri, bukan tagihan ke
        // perusahaan/asuransi; ditolak di server, bukan hanya disembunyikan dari daftar.
        var internalBenefit = receivables.Where(x => x.IsInternalBenefit).ToList();
        if (internalBenefit.Count > 0)
            throw new ReceivableInvoiceBatchBadRequestException(
                $"Piutang {internalBenefit[0].ReceivableNumber} adalah porsi Benefit RS internal dan tidak dapat digabung ke Batch Tagihan AR. " +
                "Selesaikan lewat Penutupan Benefit.");

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
            .Select(x => new { x.Id, x.EncounterType })
            .ToDictionaryAsync(x => x.Id, x => x.EncounterType, cancellationToken);

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
            Status = FinReceivableInvoiceBatchStatuses.Issued,
            IssuedAt = DateTimeOffset.UtcNow,
            ClaimStatus = null,
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

        // Jika batch sudah ISSUED, kembalikan idempotent untuk backward compatibility
        if (batch.Status == FinReceivableInvoiceBatchStatuses.Issued)
            return batch;

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Draft)
            throw new ReceivableInvoiceBatchValidationException($"Hanya batch berstatus DRAFT yang dapat diterbitkan. Status saat ini: {batch.Status}.");

        // FIN-VAL-116: batch kosong tidak boleh diterbitkan.
        if (batch.Items.Count(x => !x.IsDelete) == 0)
            throw new ReceivableInvoiceBatchValidationException("Batch tidak memiliki anggota piutang untuk diterbitkan.");

        batch.Status = FinReceivableInvoiceBatchStatuses.Issued;
        batch.IssuedAt = DateTimeOffset.UtcNow;
        batch.ClaimStatus = null;
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

    /// <summary>
    /// Membatalkan Batch Tagihan AR dan secara atomik membalik seluruh alokasi pembayaran aktif
    /// yang melekat pada piutang-piutang anggotanya (auto-reversal).
    /// Menerima status: DRAFT, ISSUED, PARTIALLY_PAID, PAID.
    /// Menolak status: CANCELLED.
    /// Seluruh alokasi pembayaran aktif (baik dari Cash/Bank maupun Ayat Silang) dibalik lewat
    /// FinanceReceiptService.ReverseAllocationCoreAsync tanpa menghapus riwayat receipt original.
    /// Subledger movement PEMBALIKAN-ALOKASI dan PEMBALIKAN-POTONGAN dicatat canonical,
    /// saldo Ayat Silang dipulihkan, status batch akhir menjadi CANCELLED, dan keanggotaan batch item dinonaktifkan.
    /// </summary>
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

        var allowedStatuses = new[]
        {
            FinReceivableInvoiceBatchStatuses.Draft,
            FinReceivableInvoiceBatchStatuses.Issued,
            FinReceivableInvoiceBatchStatuses.PartiallyPaid,
            FinReceivableInvoiceBatchStatuses.Paid
        };

        if (!allowedStatuses.Contains(batch.Status))
            throw new ReceivableInvoiceBatchValidationException($"Batch berstatus {batch.Status} tidak dapat dibatalkan.");

        var memberReceivableIds = batch.Items
            .Where(x => !x.IsDelete)
            .Select(x => x.ReceivableId)
            .Distinct()
            .ToList();

        // Cari seluruh alokasi pembayaran aktif pada piutang anggota
        var activeAllocations = await GetActiveAllocationsForReceivablesAsync(memberReceivableIds, cancellationToken);

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);

            // Deterministic distributed locking order: Batch -> Receipts (sorted) -> Receivables (sorted)
            await AcquireLockAsync($"FIN_AR_BATCH_{batch.Id:N}", cancellationToken);

            var receiptIdsToLock = activeAllocations
                .Select(a => a.ReceiptId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();
            foreach (var rId in receiptIdsToLock)
            {
                await AcquireLockAsync($"FIN_RECEIPT_{rId:N}", cancellationToken);
            }

            foreach (var recId in memberReceivableIds.OrderBy(id => id))
            {
                await AcquireLockAsync($"FIN_RECEIVABLE_{recId:N}", cancellationToken);
            }

            // Reverse setiap alokasi aktif secara atomik
            foreach (var activeAlloc in activeAllocations)
            {
                await _receiptService.ReverseAllocationCoreAsync(
                    activeAlloc.Id,
                    actorUserId,
                    cancellationToken,
                    acquireLocks: false,
                    synchronizeBatch: false);
            }

            // Nonaktifkan keanggotaan batch item agar piutang dapat digabung ulang / di-reissue
            foreach (var item in batch.Items.Where(x => !x.IsDelete))
            {
                item.IsActiveMembership = false;
                item.UpdateDateTime = DateTime.UtcNow;
                item.UpdateBy = actorUserId;
            }

            // Final state batch: CANCELLED
            var now = DateTime.UtcNow;
            batch.Status = FinReceivableInvoiceBatchStatuses.Cancelled;
            batch.CancelReason = trimmedReason;
            batch.CancelDateTime = now;
            batch.CancelBy = actorUserId;
            batch.IsCancel = true;
            batch.UpdateDateTime = now;
            batch.UpdateBy = actorUserId;
            batch.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
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

        await AuditAsync("Cancel", batch.Id, actorUserId);
        return batch;
    }

    /// <summary>
    /// Buat Ulang AR / Revive (Restore) — mengaktifkan kembali batch tagihan existing dengan
    /// Batch Id dan BatchNumber yang PERSIS SAMA (tanpa membuat GUID baru atau generate nomor invoice baru).
    ///
    /// Memeriksa bahwa tidak terdapat alokasi pembayaran aktif (unreversed) pada piutang anggotanya.
    /// Riwayat alokasi lama dan pembalikannya tetap utuh sebagai audit history.
    /// Riwayat pembatalan (CancelDateTime, CancelBy, CancelReason) SENGAJA dipertahankan sebagai bukti audit.
    ///
    /// Tanggal operasional diperbarui:
    /// InvoiceDate = FinanceBusinessDate.Today(), DueDate = InvoiceDate + PaymentTermDays, IssuedAt = UtcNow.
    /// Total tagihan dihitung ulang dari nilai piutang terkini.
    /// Status batch kembali ke ISSUED dan IsActiveMembership kembali ke true.
    /// </summary>
    public async Task<FinReceivableInvoiceBatch> ReviveAsync(
        Guid batchId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (actorUserId == Guid.Empty)
            throw new ReceivableInvoiceBatchBadRequestException("Identitas pengguna tidak valid untuk transaksi keuangan.");

        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            throw new ReceivableInvoiceBatchValidationException("Hanya batch tagihan berstatus CANCELLED yang dapat dihidupkan kembali (Buat Ulang AR).");

        if (batch.ReissuedToBatchId.HasValue)
            throw new ReceivableInvoiceBatchConflictException(
                "Batch tagihan ini sudah pernah dibuat ulang (reissue) ke batch baru dan tidak dapat dihidupkan kembali.");

        var memberReceivableIds = batch.Items
            .Where(x => !x.IsDelete)
            .Select(x => x.ReceivableId)
            .Distinct()
            .ToList();

        if (memberReceivableIds.Count == 0)
            throw new ReceivableInvoiceBatchValidationException("Batch tagihan tidak memiliki anggota piutang.");

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);

            await AcquireLockAsync($"FIN_AR_BATCH_{batch.Id:N}", cancellationToken);
            foreach (var recId in memberReceivableIds.OrderBy(id => id))
            {
                await AcquireLockAsync($"FIN_RECEIVABLE_{recId:N}", cancellationToken);
            }

            // 1. Periksa alokasi pembayaran aktif:
            // Invoice yang pernah dibayar lalu dibatalkan telah membalik alokasinya.
            // Revive HANYA ditolak bila masih ada alokasi pembayaran AKTIF yang belum dibalik!
            var hasActivePayments = await HasActiveAllocationsForReceivablesAsync(memberReceivableIds, cancellationToken);
            if (hasActivePayments)
                throw new ReceivableInvoiceBatchConflictException(
                    "Batch tagihan ini masih memiliki alokasi pembayaran aktif yang belum dibalik.");

            // 2. Periksa apakah ada koreksi piutang (FinReceivableAdjustment) yang masih pending (Requested)
            var hasPendingAdjustments = await _dbContext.FinReceivableAdjustments.AsNoTracking()
                .AnyAsync(a => memberReceivableIds.Contains(a.ReceivableId) && a.Status == FinReceivableApprovalStatuses.Requested && !a.IsDelete, cancellationToken);
            if (hasPendingAdjustments)
                throw new ReceivableInvoiceBatchConflictException(
                    "Perubahan nominal masih menunggu persetujuan koreksi piutang.");

            // 3. Periksa konflik: piutang anggota tidak boleh tergabung di batch aktif lain
            var conflictingNumbers = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActiveMembership && x.BatchId != batch.Id && memberReceivableIds.Contains(x.ReceivableId))
                .Select(x => x.Receivable!.ReceivableNumber)
                .ToListAsync(cancellationToken);
            if (conflictingNumbers.Count > 0)
                throw new ReceivableInvoiceBatchConflictException(
                    $"Piutang {string.Join(", ", conflictingNumbers)} sudah tergabung dalam batch tagihan aktif lain dan tidak dapat dikembalikan ke batch ini.");

            // 4. Perbarui tanggal operasional (InvoiceDate, DueDate, IssuedAt) dengan mempertahankan ID & Nomor Invoice
            var today = FinanceBusinessDate.Today();
            int paymentTermDays = batch.PaymentTermDays is > 0 ? batch.PaymentTermDays.Value : 30;
            batch.InvoiceDate = today;
            batch.DueDate = today.AddDays(paymentTermDays);
            batch.IssuedAt = DateTimeOffset.UtcNow;

            // 5. Hitung ulang total dari nilai finansial piutang terkini (nilai efektif = OriginalAmount - AdjustedAmount - WrittenOffAmount)
            var memberReceivables = await _dbContext.FinReceivables
                .Where(x => memberReceivableIds.Contains(x.Id) && !x.IsDelete)
                .ToListAsync(cancellationToken);

            if (memberReceivables.Count == 0)
                throw new ReceivableInvoiceBatchValidationException("Anggota piutang batch tagihan tidak ditemukan.");

            foreach (var r in memberReceivables)
            {
                if (r.Status is FinReceivableStatuses.Cancelled or FinReceivableStatuses.WrittenOff)
                {
                    throw new ReceivableInvoiceBatchValidationException(
                        $"Piutang {r.ReceivableNumber} berstatus {r.Status} dan tidak dapat diaktifkan kembali.");
                }

                var effectiveAmount = r.OriginalAmount - r.AdjustedAmount - r.WrittenOffAmount;
                if (r.OutstandingAmount > effectiveAmount)
                {
                    throw new ReceivableInvoiceBatchValidationException(
                        $"Sisa piutang {r.ReceivableNumber} (Rp {r.OutstandingAmount:N0}) melebihi nilai efektif (Rp {effectiveAmount:N0}).");
                }
            }

            batch.TotalAmount = memberReceivables.Sum(r => r.OriginalAmount - r.AdjustedAmount - r.WrittenOffAmount);

            // Reset nominal diskon dan penerimaan lain dari riwayat pembayaran sebelumnya yang sudah dibalik
            batch.TotalDiscount = 0m;
            batch.DiscountChartOfAccountId = null;
            batch.DiscountType = null;
            batch.DiscountPercent = null;
            batch.DiscountNote = null;
            batch.OtherReceiptAmount = 0m;
            batch.OtherReceiptChartOfAccountId = null;
            batch.OtherReceiptNote = null;

            // 6. Set status ISSUED dan aktifkan kembali membership
            batch.Status = FinReceivableInvoiceBatchStatuses.Issued;
            batch.IsCancel = false;
            batch.ClaimStatus = null;
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = actorUserId;
            batch.RowVersion = Guid.NewGuid();
            // CATATAN AUDIT: CancelDateTime, CancelBy, CancelReason SENGAJA dipertahankan untuk riwayat audit.

            foreach (var item in batch.Items.Where(x => !x.IsDelete))
            {
                item.IsActiveMembership = true;
                item.UpdateDateTime = DateTime.UtcNow;
                item.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
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

        await AuditAsync("Revive", batch.Id, actorUserId);
        return batch;
    }

    /// <summary>
    /// Mengembalikan seluruh alokasi pembayaran aktif (unreversed) untuk piutang-piutang anggota batch.
    /// Alokasi aktif adalah alokasi original (!IsDelete dan !IsReversal dan Amount > 0)
    /// yang belum pernah dibalik oleh alokasi pembalik (!IsDelete dan IsReversal dan ReversalOfAllocationId == original.Id).
    /// </summary>
    public async Task<List<FinReceiptAllocation>> GetActiveAllocationsForReceivablesAsync(
        IEnumerable<Guid> receivableIds, CancellationToken cancellationToken)
    {
        var idList = receivableIds.Distinct().ToList();
        if (idList.Count == 0) return new List<FinReceiptAllocation>();

        var originalAllocations = await _dbContext.FinReceiptAllocations
            .Include(a => a.Receipt)
            .Where(a => a.ReceivableId.HasValue && idList.Contains(a.ReceivableId.Value) && !a.IsDelete && !a.IsReversal && a.Amount > 0)
            .ToListAsync(cancellationToken);

        if (originalAllocations.Count == 0) return new List<FinReceiptAllocation>();

        var originalIds = originalAllocations.Select(a => a.Id).ToList();

        var reversedTargetIds = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .Where(a => a.IsReversal && a.ReversalOfAllocationId.HasValue && originalIds.Contains(a.ReversalOfAllocationId.Value) && !a.IsDelete)
            .Select(a => a.ReversalOfAllocationId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var reversedSet = reversedTargetIds.ToHashSet();

        return originalAllocations
            .Where(a => !reversedSet.Contains(a.Id))
            .OrderBy(a => a.AllocatedAt)
            .ToList();
    }

    /// <summary>
    /// Memeriksa apakah piutang-piutang anggota memiliki setidaknya satu alokasi pembayaran aktif (unreversed).
    /// </summary>
    public async Task<bool> HasActiveAllocationsForReceivablesAsync(
        IEnumerable<Guid> receivableIds, CancellationToken cancellationToken)
    {
        var idList = receivableIds.Distinct().ToList();
        if (idList.Count == 0) return false;

        var originalAllocations = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .Where(a => a.ReceivableId.HasValue && idList.Contains(a.ReceivableId.Value) && !a.IsDelete && !a.IsReversal && a.Amount > 0)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        if (originalAllocations.Count == 0) return false;

        var reversedTargetIds = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .Where(a => a.IsReversal && a.ReversalOfAllocationId.HasValue && originalAllocations.Contains(a.ReversalOfAllocationId.Value) && !a.IsDelete)
            .Select(a => a.ReversalOfAllocationId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var reversedSet = reversedTargetIds.ToHashSet();
        return originalAllocations.Any(id => !reversedSet.Contains(id));
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

    [Obsolete("Workflow operasional klaim ditiadakan. Dipertahankan hanya untuk backward compatibility legacy consumer.")]
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

    [Obsolete("Workflow operasional klaim ditiadakan. Dipertahankan hanya untuk backward compatibility legacy consumer.")]
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

    [Obsolete("Workflow operasional klaim ditiadakan. Dipertahankan hanya untuk backward compatibility legacy consumer.")]
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

        // 1. Resolve Debtor (Company Guarantor or Insurance Provider)
        var company = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == batch.DebtorReferenceId && !x.IsDelete, cancellationToken);
        var insurance = company is null
            ? await _dbContext.MstInsuranceProviders.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == batch.DebtorReferenceId && !x.IsDelete, cancellationToken)
            : null;

        var debtorName = company?.CompanyGuarantorName ?? insurance?.InsuranceProviderName ?? "-";
        var officeAddress = company?.OfficeAddress ?? insurance?.OfficeAddress ?? "-";
        var picName = company?.PicName ?? insurance?.PicName ?? "-";
        var picPhone = company?.PicPhoneNumber ?? insurance?.PicPhoneNumber;
        var picEmail = company?.PicEmail ?? insurance?.PicEmail;
        var contractNumber = company?.ContractNumber ?? insurance?.ContractNumber;
        var debtorType = company is not null ? "CompanyGuarantor" : (insurance is not null ? "InsuranceProvider" : "Corporate");

        // 2. Resolve Active Official Bank Accounts from V2 Master Data
        var bankAccounts = await _dbContext.MstBankAccounts
            .Include(x => x.Bank)
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && (x.AccountType == "COLLECTION" || x.AccountType == "OPERATIONAL"))
            .OrderByDescending(x => x.AccountType == "COLLECTION")
            .ThenBy(x => x.Bank != null ? x.Bank.SortOrder : 0)
            .ThenBy(x => x.Bank != null ? x.Bank.BankName : "")
            .Select(x => new ReceivableInvoiceBatchBankAccountResponse
            {
                BankAccountId = x.Id,
                BankName = x.Bank != null ? x.Bank.BankName : "Bank",
                BankCode = x.Bank != null ? x.Bank.BankCode : "",
                AccountNumber = x.AccountNumber,
                AccountName = x.AccountName,
                Branch = x.Bank != null ? x.Bank.Description : null,
                CurrencyCode = x.CurrencyCode,
                AccountType = x.AccountType
            })
            .ToListAsync(cancellationToken);

        // 3. Resolve Actual Payments Received from FinReceipt & FinReceiptAllocation
        var receivableIds = batch.Items.Where(x => !x.IsDelete && x.Receivable is not null)
            .Select(x => x.ReceivableId)
            .Distinct()
            .ToList();

        var receipts = await _dbContext.FinReceipts
            .Include(r => r.BankAccount).ThenInclude(b => b!.Bank)
            .AsNoTracking()
            .Where(r => !r.IsDelete
                && r.Status != FinReceiptStatuses.Reversed
                && r.ReversalOfReceiptId == null
                && (r.CorrelationId == batch.Id
                    || _dbContext.FinReceiptAllocations.Any(a =>
                        !a.IsDelete && !a.IsReversal && a.ReceiptId == r.Id && a.ReceivableId.HasValue && receivableIds.Contains(a.ReceivableId.Value))))
            .OrderByDescending(r => r.OccurredAt)
            .Select(r => new ReceivableInvoiceBatchReceiptResponse
            {
                ReceiptId = r.Id,
                ReceiptNumber = r.ReceiptNumber,
                KwitansiNumber = r.KwitansiNumber ?? r.ReceiptNumber,
                Amount = r.Amount,
                OccurredAt = r.OccurredAt,
                ProviderReference = r.ProviderReference,
                BankAccountId = r.BankAccountId,
                BankName = r.BankAccount != null && r.BankAccount.Bank != null ? r.BankAccount.Bank.BankName : null,
                AccountNumber = r.BankAccount != null ? r.BankAccount.AccountNumber : null,
                AccountName = r.BankAccount != null ? r.BankAccount.AccountName : null,
                Status = r.Status
            })
            .ToListAsync(cancellationToken);

        var response = new ReceivableInvoiceBatchDocumentResponse
        {
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            DebtorReferenceId = batch.DebtorReferenceId,
            CompanyId = batch.DebtorReferenceId,
            CompanyName = debtorName,
            CompanyAddress = officeAddress,
            PicName = picName,
            PicPhone = picPhone,
            PicEmail = picEmail,
            ContractNumber = contractNumber,
            DebtorType = debtorType,
            PeriodStart = batch.PeriodStart,
            PeriodEnd = batch.PeriodEnd,
            InvoiceDate = batch.InvoiceDate,
            DueDate = batch.DueDate,
            IsBankAccountConfigured = bankAccounts.Count > 0,
            BankAccounts = bankAccounts,
            Receipts = receipts
        };

        if (bankAccounts.Count == 0)
        {
            response.Warnings.Add("Rekening pembayaran rumah sakit belum dikonfigurasi pada master data.");
        }

        // 4. Resolve Member Invoices and Patient Recap with Coverage Items Breakdown
        int seq = 1;
        foreach (var item in batch.Items.Where(x => !x.IsDelete && x.Receivable is not null))
        {
            var rec = item.Receivable!;
            if (rec.InvoiceId is not { } invoiceId)
            {
                response.Warnings.Add($"Anggota {rec.ReceivableNumber} adalah item migrasi tagihan lama — nol dokumen Billing untuk diambil.");
                response.Patients.Add(new ReceivableInvoiceBatchRecapPatientResponse
                {
                    SequenceNumber = seq++,
                    InvoiceId = null,
                    ReceivableId = rec.Id,
                    TransactionDate = DateOnly.FromDateTime(rec.CreateDateTime),
                    MedicalRecordNumber = "-",
                    PatientName = "Item Migrasi Tagihan Lama",
                    BillingNumber = rec.ReceivableNumber,
                    VisitCode = batch.VisitCode ?? string.Empty,
                    VisitType = batch.VisitCode == "IP" ? "Rawat Inap" : "Rawat Jalan",
                    Note = "Item Migrasi",
                    TotalAmount = rec.OriginalAmount,
                    PatientPaidAmount = 0m,
                    GuarantorCoveredAmount = rec.OriginalAmount
                });
                continue;
            }

            CompanyGuarantorInvoiceDocumentResponse? invoiceDocument = null;
            try
            {
                invoiceDocument = await _documentService.GetDocumentAsync(invoiceId, actorUserId, cancellationToken);
                if (invoiceDocument != null)
                {
                    response.Invoices.Add(invoiceDocument);
                    response.Warnings.AddRange(invoiceDocument.Warnings);
                }
            }
            catch (Exception ex)
            {
                response.Warnings.Add($"Dokumen billing {invoiceId} dilewati: {ex.Message}");
            }

            var patientName = invoiceDocument?.Patient?.FullName ?? "-";
            var mrn = invoiceDocument?.Patient?.MedicalRecordNumber ?? "-";
            var billingNum = invoiceDocument?.InvoiceNumber ?? rec.ReceivableNumber;
            var txDate = invoiceDocument?.InvoiceDate.HasValue == true
                ? DateOnly.FromDateTime(invoiceDocument.InvoiceDate.Value.DateTime)
                : DateOnly.FromDateTime(rec.CreateDateTime);
            var coveredAmt = invoiceDocument?.Totals?.TotalCoveredAmount ?? rec.OriginalAmount;
            var patientAmt = invoiceDocument?.Totals?.PatientAmount ?? 0m;
            var totalAmt = coveredAmt + patientAmt;

            var coverageItems = new List<ReceivableInvoiceBatchRecapItemResponse>();
            if (invoiceDocument?.Items != null && invoiceDocument.Items.Count > 0)
            {
                foreach (var itm in invoiceDocument.Items)
                {
                    coverageItems.Add(new ReceivableInvoiceBatchRecapItemResponse
                    {
                        InvoiceItemId = itm.InvoiceItemId,
                        Description = itm.Description,
                        CategoryCode = itm.CategoryCode,
                        CategoryName = itm.CategoryName,
                        Quantity = itm.Quantity,
                        UnitPrice = itm.UnitPrice,
                        GrossAmount = itm.GrossAmount,
                        ItemDiscount = itm.ItemDiscount,
                        NetAmount = itm.NetAmount,
                        CoveredNetAmount = itm.CoveredNetAmount,
                        CoveredTaxAmount = itm.CoveredTaxAmount,
                        CoveredAmount = itm.CoveredAmount,
                        PatientAmount = itm.PatientAmount
                    });
                }
            }
            else
            {
                coverageItems.Add(new ReceivableInvoiceBatchRecapItemResponse
                {
                    Description = "Pelayanan Kesehatan " + (batch.VisitCode == "IP" ? "Rawat Inap" : "Rawat Jalan"),
                    Quantity = 1,
                    UnitPrice = coveredAmt,
                    GrossAmount = totalAmt,
                    NetAmount = totalAmt,
                    CoveredNetAmount = coveredAmt,
                    CoveredAmount = coveredAmt,
                    PatientAmount = patientAmt
                });
            }

            response.Patients.Add(new ReceivableInvoiceBatchRecapPatientResponse
            {
                SequenceNumber = seq++,
                InvoiceId = invoiceId,
                ReceivableId = rec.Id,
                TransactionDate = txDate,
                MedicalRecordNumber = mrn,
                PatientName = patientName,
                BillingNumber = billingNum,
                VisitCode = batch.VisitCode ?? string.Empty,
                VisitType = batch.VisitCode == "IP" ? "Rawat Inap" : "Rawat Jalan",
                Note = invoiceDocument?.Patient?.RoomName ?? invoiceDocument?.Patient?.ServiceUnitName ?? "-",
                TotalAmount = totalAmt,
                PatientPaidAmount = patientAmt,
                GuarantorCoveredAmount = coveredAmt,
                CoverageItems = coverageItems
            });
        }

        response.GrandTotalCoveredAmount = response.Patients.Sum(x => x.GuarantorCoveredAmount);
        response.TotalBillAmount = response.Patients.Sum(x => x.TotalAmount);
        response.TotalPatientAmount = response.Patients.Sum(x => x.PatientPaidAmount);
        response.PatientCount = response.Patients.Count;

        return response;
    }

    public async Task<ReceivableInvoiceBatchSingleDocumentResponse> GetSpecificDocumentAsync(
        Guid batchId,
        string documentType,
        string? language,
        Guid? receiptId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var doc = await GetDocumentAsync(batchId, actorUserId, cancellationToken);
        var normType = (documentType ?? "").Trim().ToUpperInvariant() switch
        {
            "INVOICE" or "LETTER" or "BERKAS_INVOICE" or "INVOICE_LETTER" => ReceivableInvoiceBatchDocumentTypes.InvoiceLetter,
            "TANDA_TERIMA" or "RECEIPT_ACK" or "ACKNOWLEDGEMENT" => ReceivableInvoiceBatchDocumentTypes.Acknowledgement,
            "KWITANSI" or "PAYMENT" or "PAYMENT_RECEIPT" => ReceivableInvoiceBatchDocumentTypes.PaymentReceipt,
            "REKAP" or "REKAP_TAGIHAN" or "BILLING_RECAP" => ReceivableInvoiceBatchDocumentTypes.BillingRecap,
            _ => documentType?.ToUpperInvariant() ?? ReceivableInvoiceBatchDocumentTypes.InvoiceLetter
        };

        var lang = (language ?? "id").Trim().ToLowerInvariant();
        if (lang != "id" && lang != "en") lang = "id";

        var meta = new ReceivableInvoiceBatchDocumentMetadataResponse
        {
            DocumentType = normType,
            DocumentNumber = doc.BatchNumber,
            Language = lang,
            RelatedInvoiceId = doc.BatchId,
            Printable = true,
            Downloadable = true,
            Available = true
        };

        if (normType == ReceivableInvoiceBatchDocumentTypes.PaymentReceipt)
        {
            if (doc.Receipts.Count == 0)
            {
                meta.Available = false;
                meta.Message = lang == "en"
                    ? "Not available — payment has not been received yet."
                    : "Belum tersedia — pembayaran belum diterima.";
                meta.Printable = false;
                meta.Downloadable = false;
            }
            else
            {
                if (receiptId.HasValue)
                {
                    var selected = doc.Receipts.FirstOrDefault(r => r.ReceiptId == receiptId.Value);
                    if (selected != null)
                    {
                        meta.RelatedReceiptId = selected.ReceiptId;
                        meta.DocumentNumber = selected.KwitansiNumber ?? selected.ReceiptNumber;
                    }
                }
                else
                {
                    var latest = doc.Receipts.First();
                    meta.RelatedReceiptId = latest.ReceiptId;
                    meta.DocumentNumber = latest.KwitansiNumber ?? latest.ReceiptNumber;
                }
            }
        }
        else if (normType == ReceivableInvoiceBatchDocumentTypes.Acknowledgement)
        {
            meta.DocumentNumber = $"TT/{doc.BatchNumber}";
        }

        return new ReceivableInvoiceBatchSingleDocumentResponse
        {
            Metadata = meta,
            Document = doc
        };
    }

    public async Task<byte[]> ExportRecapExcelAsync(Guid batchId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var doc = await GetDocumentAsync(batchId, actorUserId, cancellationToken);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Rekap Tagihan AR");

        // Title Block
        ws.Cell(1, 1).Value = "RUMAH SAKIT METROPOLITAN MEDICAL CENTRE";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(2, 1).Value = "REKAPITULASI TAGIHAN PELAYANAN KESEHATAN";
        ws.Cell(2, 1).Style.Font.Bold = true;
        ws.Cell(2, 1).Style.Font.FontSize = 12;

        ws.Cell(4, 1).Value = "No. Invoice AR:";
        ws.Cell(4, 2).Value = doc.BatchNumber;
        ws.Cell(4, 4).Value = "Penjamin / Asuransi:";
        ws.Cell(4, 5).Value = doc.CompanyName ?? "-";

        ws.Cell(5, 1).Value = "Periode Pelayanan:";
        ws.Cell(5, 2).Value = $"{doc.PeriodStart:dd/MM/yyyy} s/d {doc.PeriodEnd:dd/MM/yyyy}";
        ws.Cell(5, 4).Value = "Alamat:";
        ws.Cell(5, 5).Value = doc.CompanyAddress ?? "-";

        ws.Cell(6, 1).Value = "Tanggal Invoice:";
        ws.Cell(6, 2).Value = doc.InvoiceDate.HasValue ? doc.InvoiceDate.Value.ToString("dd/MM/yyyy") : "-";
        ws.Cell(6, 4).Value = "Jatuh Tempo:";
        ws.Cell(6, 5).Value = doc.DueDate.HasValue ? doc.DueDate.Value.ToString("dd/MM/yyyy") : "-";

        int row = 8;
        // Table 1 Header: Ringkasan Pasien
        ws.Cell(row, 1).Value = "TABEL 1. DAFTAR TAGIHAN PASIEN";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        var headers = new[]
        {
            "No.", "Tanggal Transaksi", "No. Rekam Medis", "Nama Pasien",
            "No. Billing", "Jenis Kunjungan", "Keterangan", "Total Biaya", "Dibayar", "Tagihan Penjamin"
        };

        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(row, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E2E8F0");
            cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = (c >= 7)
                ? ClosedXML.Excel.XLAlignmentHorizontalValues.Right
                : ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
        }
        row++;

        int startPatientRow = row;
        foreach (var p in doc.Patients)
        {
            ws.Cell(row, 1).Value = p.SequenceNumber;
            ws.Cell(row, 2).Value = p.TransactionDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 3).Value = p.MedicalRecordNumber;
            ws.Cell(row, 4).Value = p.PatientName;
            ws.Cell(row, 5).Value = p.BillingNumber;
            ws.Cell(row, 6).Value = p.VisitType;
            ws.Cell(row, 7).Value = p.Note ?? "-";
            ws.Cell(row, 8).Value = p.TotalAmount;
            ws.Cell(row, 9).Value = p.PatientPaidAmount;
            ws.Cell(row, 10).Value = p.GuarantorCoveredAmount;

            for (int c = 1; c <= 10; c++)
            {
                var cell = ws.Cell(row, c);
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                if (c >= 8)
                {
                    cell.Style.NumberFormat.Format = "#,##0";
                    cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                }
            }
            row++;
        }

        // Total Row Table 1
        ws.Cell(row, 1).Value = "GRAND TOTAL";
        ws.Range(row, 1, row, 7).Merge();
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

        ws.Cell(row, 8).FormulaA1 = $"SUM(H{startPatientRow}:H{row - 1})";
        ws.Cell(row, 9).FormulaA1 = $"SUM(I{startPatientRow}:I{row - 1})";
        ws.Cell(row, 10).FormulaA1 = $"SUM(J{startPatientRow}:J{row - 1})";

        for (int c = 1; c <= 10; c++)
        {
            var cell = ws.Cell(row, c);
            cell.Style.Font.Bold = true;
            cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#F1F5F9");
            if (c >= 8)
            {
                cell.Style.NumberFormat.Format = "#,##0";
                cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            }
        }
        row += 3;

        // Table 2 Header: Rincian Item Coverage Per Pasien
        ws.Cell(row, 1).Value = "TABEL 2. RINCIAN ITEM PELAYANAN YANG DICOVER PENJAMIN/ASURANSI";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        foreach (var p in doc.Patients)
        {
            ws.Cell(row, 1).Value = $"Pasien: {p.PatientName} (RM: {p.MedicalRecordNumber}) | No. Billing: {p.BillingNumber} | {p.VisitType}";
            ws.Range(row, 1, row, 9).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E0F2FE");
            ws.Cell(row, 1).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            row++;

            var itemHeaders = new[]
            {
                "No.", "Nama Layanan / Tindakan", "Kategori", "Qty", "Tarif Satuan",
                "Total Biaya", "Diskon", "Ditanggung Penjamin", "Porsi Pasien"
            };

            for (int c = 0; c < itemHeaders.Length; c++)
            {
                var cell = ws.Cell(row, c + 1);
                cell.Value = itemHeaders[c];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#F8FAFC");
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                cell.Style.Alignment.Horizontal = (c >= 3)
                    ? ClosedXML.Excel.XLAlignmentHorizontalValues.Right
                    : ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            }
            row++;

            int startItemRow = row;
            int itemNo = 1;
            foreach (var itm in p.CoverageItems)
            {
                ws.Cell(row, 1).Value = itemNo++;
                ws.Cell(row, 2).Value = itm.Description;
                ws.Cell(row, 3).Value = itm.CategoryName ?? itm.CategoryCode ?? "-";
                ws.Cell(row, 4).Value = itm.Quantity;
                ws.Cell(row, 5).Value = itm.UnitPrice;
                ws.Cell(row, 6).Value = itm.NetAmount > 0 ? itm.NetAmount : itm.GrossAmount;
                ws.Cell(row, 7).Value = itm.ItemDiscount;
                ws.Cell(row, 8).Value = itm.CoveredAmount;
                ws.Cell(row, 9).Value = itm.PatientAmount;

                for (int c = 1; c <= 9; c++)
                {
                    var cell = ws.Cell(row, c);
                    cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    if (c >= 4)
                    {
                        cell.Style.NumberFormat.Format = "#,##0";
                        cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                    }
                }
                row++;
            }

            // Subtotal Row for this patient
            ws.Cell(row, 1).Value = $"Subtotal {p.PatientName}";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

            ws.Cell(row, 6).FormulaA1 = $"SUM(F{startItemRow}:F{row - 1})";
            ws.Cell(row, 7).FormulaA1 = $"SUM(G{startItemRow}:G{row - 1})";
            ws.Cell(row, 8).FormulaA1 = $"SUM(H{startItemRow}:H{row - 1})";
            ws.Cell(row, 9).FormulaA1 = $"SUM(I{startItemRow}:I{row - 1})";

            for (int c = 1; c <= 9; c++)
            {
                var cell = ws.Cell(row, c);
                cell.Style.Font.Bold = true;
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#F1F5F9");
                if (c >= 6)
                {
                    cell.Style.NumberFormat.Format = "#,##0";
                    cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                }
            }
            row += 2;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
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
        if (batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
        {
            return;
        }

        var members = batch.Items.Where(x => !x.IsDelete && x.Receivable is not null && !x.Receivable.IsDelete).ToList();
        if (members.Count == 0) return;

        var totalAllocated = members.Sum(x => x.Receivable!.AllocatedAmount);
        var totalOutstanding = members.Sum(x => x.Receivable!.OutstandingAmount);
        var totalOriginal = members.Sum(x => x.Receivable!.OriginalAmount);
        var totalAdjusted = members.Sum(x => x.Receivable!.AdjustedAmount);
        var totalWrittenOff = members.Sum(x => x.Receivable!.WrittenOffAmount);
        var totalEffective = totalOriginal - totalAdjusted - totalWrittenOff;

        var allSettled = members.All(x => x.Receivable!.OutstandingAmount <= 0);
        var hasPayment = totalAllocated > 0;

        var newStatus = allSettled && (hasPayment || totalEffective <= 0)
            ? FinReceivableInvoiceBatchStatuses.Paid
            : (hasPayment && totalOutstanding > 0)
                ? FinReceivableInvoiceBatchStatuses.PartiallyPaid
                : FinReceivableInvoiceBatchStatuses.Issued;

        bool changed = false;

        // Normalisasi IssuedAt jika masih null (misal legacy DRAFT yang sudah punya nomor invoice)
        if (batch.IssuedAt == null)
        {
            DateTimeOffset effectiveIssuedAt;
            if (batch.InvoiceDate.HasValue)
            {
                effectiveIssuedAt = new DateTimeOffset(batch.InvoiceDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            }
            else
            {
                effectiveIssuedAt = new DateTimeOffset(batch.CreateDateTime, TimeSpan.Zero);
            }
            batch.IssuedAt = effectiveIssuedAt;
            changed = true;
        }

        if (newStatus != batch.Status)
        {
            batch.Status = newStatus;
            changed = true;
        }

        if (changed)
        {
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.RowVersion = Guid.NewGuid();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
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
            VisitCode = visitCode ?? string.Empty,
            VisitLabel = ResolveVisitLabel(visitCode) ?? string.Empty,
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
        Dictionary<Guid, EncounterType> encounters)
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

        if (encounterId.HasValue && encounters.TryGetValue(encounterId.Value, out var encounterType))
        {
            return encounterType switch
            {
                EncounterType.Inpatient => "IP",
                EncounterType.Outpatient => "OP",
                EncounterType.Emergency => "OP",
                _ => throw new ReceivableInvoiceBatchValidationException(
                    $"Piutang {receivable.ReceivableNumber} dengan jenis kunjungan '{encounterType}' tidak dapat diklasifikasikan secara aman ke IP atau OP.")
            };
        }

        throw new ReceivableInvoiceBatchValidationException(
            $"Piutang {receivable.ReceivableNumber} tidak memiliki data invoice atau encounter yang valid untuk klasifikasi IP/OP.");
    }

    public static string ResolveReceivableVisitCode(
        FinReceivable receivable,
        List<FinReceivableItem> items,
        Dictionary<Guid, BilInvoice> invoices,
        Dictionary<Guid, RegPatientEncounter> encounters)
    {
        return ResolveReceivableVisitCode(
            receivable,
            items,
            invoices,
            encounters.ToDictionary(k => k.Key, v => v.Value.EncounterType));
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
        if (actorUserId == Guid.Empty)
            throw new ReceivableInvoiceBatchBadRequestException("Identitas pengguna tidak valid.");

        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, request.ExpectedRowVersion);

        // Transisi kompatibilitas: legacy batch berstatus DRAFT otomatis dinormalisasi ke ISSUED saat menerima pembayaran pertama
        if (batch.Status == FinReceivableInvoiceBatchStatuses.Draft)
        {
            batch.Status = FinReceivableInvoiceBatchStatuses.Issued;
            batch.IssuedAt ??= DateTimeOffset.UtcNow;
            batch.ClaimStatus = null;
        }

        if (batch.Status == FinReceivableInvoiceBatchStatuses.Paid)
            throw new ReceivableInvoiceBatchValidationException("Batch Tagihan AR sudah lunas (PAID).");

        if (batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
            throw new ReceivableInvoiceBatchValidationException("Batch Tagihan AR sudah dibatalkan (CANCELLED).");

        // Periksa apakah batch ini sudah memiliki pembayaran aktif yang belum dibalik (Section 7)
        var memberRecIds = batch.Items
            .Where(x => !x.IsDelete && x.IsActiveMembership)
            .Select(x => x.ReceivableId)
            .Distinct()
            .ToList();
        if (memberRecIds.Count > 0 && await HasActiveAllocationsForReceivablesAsync(memberRecIds, cancellationToken))
        {
            throw new ReceivableInvoiceBatchValidationException("Invoice ini sudah memiliki pembayaran aktif. Gunakan koreksi pembayaran atau batalkan invoice jika diperlukan.");
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existingReceipt = await _dbContext.FinReceipts.AsNoTracking()
                .FirstOrDefaultAsync(r => r.ProviderEventId == idempotencyKey && !r.IsDelete, cancellationToken);
            if (existingReceipt != null)
                throw new ReceivableInvoiceBatchConflictException("Pembayaran dengan kunci idempotensi ini sudah pernah diproses.");
        }

        // Validasi Nominal Penerimaan Bank
        if (request.ReceiptAmount <= 0)
            throw new ReceivableInvoiceBatchBadRequestException("Nominal penerimaan bank (ReceiptAmount) harus lebih besar dari nol.");

        // Validasi Rekening Bank
        if (!request.BankAccountId.HasValue || request.BankAccountId.Value == Guid.Empty)
            throw new ReceivableInvoiceBatchBadRequestException("Rekening bank rumah sakit wajib dipilih.");

        var bankExists = await _dbContext.MstBankAccounts.AsNoTracking()
            .AnyAsync(b => b.Id == request.BankAccountId.Value && !b.IsDelete && b.IsActive, cancellationToken);
        if (!bankExists)
            throw new ReceivableInvoiceBatchBadRequestException("Rekening bank rumah sakit tidak valid atau tidak aktif.");

        // Validasi Non-Negatif untuk Diskon dan Potongan
        if (request.DiscountAmount < 0 || request.OtherReceiptAmount < 0 || request.Pph23Amount < 0 || request.BankAdminFeeAmount < 0)
            throw new ReceivableInvoiceBatchBadRequestException("Nominal diskon, penerimaan lain, PPh 23, dan biaya administrasi bank tidak boleh bernilai negatif.");

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

        if (request.MemberAllocations.Select(m => m.ReceivableId).Distinct().Count() != request.MemberAllocations.Count)
            throw new ReceivableInvoiceBatchBadRequestException("Rincian alokasi anggota tidak boleh memiliki piutang duplikat.");

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

        var activeItems = batch.Items.Where(x => !x.IsDelete && x.IsActiveMembership).ToList();
        var missingRecIds = activeItems.Where(x => x.Receivable == null).Select(x => x.ReceivableId).ToList();
        if (missingRecIds.Count > 0)
        {
            var loadedRecs = await _dbContext.FinReceivables
                .Where(r => missingRecIds.Contains(r.Id) && !r.IsDelete)
                .ToDictionaryAsync(r => r.Id, cancellationToken);
            foreach (var itm in activeItems.Where(x => x.Receivable == null))
            {
                if (loadedRecs.TryGetValue(itm.ReceivableId, out var r))
                {
                    itm.Receivable = r;
                }
            }
        }

        var memberMap = activeItems
            .Where(x => x.Receivable is not null)
            .ToDictionary(x => x.ReceivableId, x => x.Receivable!);

        foreach (var alloc in request.MemberAllocations)
        {
            if (alloc.Amount < 0 || alloc.DiscountAmount < 0 || alloc.Pph23Amount < 0 || alloc.BankAdminFeeAmount < 0)
                throw new ReceivableInvoiceBatchBadRequestException("Nominal alokasi dan potongan tidak boleh bernilai negatif.");

            if (!memberMap.TryGetValue(alloc.ReceivableId, out var rec))
                throw new ReceivableInvoiceBatchBadRequestException($"Piutang {alloc.ReceivableId} bukan merupakan anggota aktif dari batch tagihan ini.");

            if (rec.Status is FinReceivableStatuses.Cancelled or FinReceivableStatuses.WrittenOff)
                throw new ReceivableInvoiceBatchValidationException(
                    $"Piutang {rec.ReceivableNumber} berstatus {rec.Status} dan tidak dapat menerima pembayaran.");

            if (rec.OutstandingAmount <= 0)
                throw new ReceivableInvoiceBatchValidationException(
                    $"Piutang {rec.ReceivableNumber} tidak memiliki sisa tagihan (OutstandingAmount <= 0).");

            var totalDeductionForRec = alloc.Amount + alloc.DiscountAmount + alloc.Pph23Amount + alloc.BankAdminFeeAmount;
            if (totalDeductionForRec <= 0)
                throw new ReceivableInvoiceBatchValidationException(
                    $"Total alokasi dan potongan untuk piutang {rec.ReceivableNumber} harus lebih besar dari nol.");

            if (totalDeductionForRec > rec.OutstandingAmount)
                throw new ReceivableInvoiceBatchValidationException(
                    $"Total alokasi dan potongan untuk piutang {rec.ReceivableNumber} (Rp {totalDeductionForRec:N0}) melebihi sisa piutang saat ini (Rp {rec.OutstandingAmount:N0}).");

            if ((alloc.Pph23Amount > 0 || alloc.BankAdminFeeAmount > 0) && alloc.Amount <= 0)
                throw new ReceivableInvoiceBatchValidationException(
                    $"Potongan PPh 23 atau biaya admin pada piutang {rec.ReceivableNumber} memerlukan alokasi penerimaan kas/bank (Amount > 0).");
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

            // Validasi ulang di bawah lock untuk mencegah race conditions
            EnsureCurrent(batch.RowVersion, request.ExpectedRowVersion);

            if (batch.Status == FinReceivableInvoiceBatchStatuses.Paid)
                throw new ReceivableInvoiceBatchValidationException("Batch Tagihan AR sudah lunas (PAID).");

            if (batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
                throw new ReceivableInvoiceBatchValidationException("Batch Tagihan AR sudah dibatalkan (CANCELLED).");

            if (memberRecIds.Count > 0 && await HasActiveAllocationsForReceivablesAsync(memberRecIds, cancellationToken))
            {
                throw new ReceivableInvoiceBatchValidationException("Invoice ini sudah memiliki pembayaran aktif. Gunakan koreksi pembayaran atau batalkan invoice jika diperlukan.");
            }

            // Re-check idempotency key under transaction lock
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existingReceiptUnderLock = await _dbContext.FinReceipts.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ProviderEventId == idempotencyKey && !r.IsDelete, cancellationToken);
                if (existingReceiptUnderLock != null)
                    throw new ReceivableInvoiceBatchConflictException("Pembayaran dengan kunci idempotensi ini sudah pernah diproses.");
            }

            var receiptNumber = GenerateReceiptNumber();
            var paymentDate = request.PaymentDate.HasValue
                ? request.PaymentDate.Value.ToUniversalTime()
                : DateTimeOffset.UtcNow;
            var accountingDate = request.PaymentDate.HasValue
                ? FinanceBusinessDate.ToDateOnly(request.PaymentDate.Value)
                : FinanceBusinessDate.Today();

            var receipt = new FinReceipt
            {
                Id = Guid.NewGuid(),
                ReceiptNumber = receiptNumber,
                SourceType = FinReceiptSourceTypes.ArCollection,
                BankAccountId = request.BankAccountId.Value,
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
                FinReceiptAllocation? allocation = null;

                // 1. Alokasi penerimaan kas/bank (HANYA bila line.Amount > 0, agar memenuhi CK_FinReceiptAllocation_Amount > 0)
                if (line.Amount > 0)
                {
                    allocation = new FinReceiptAllocation
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
                        sourceAllocationId: allocation?.Id,
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
                        ReceiptAllocationId = allocation!.Id,
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
                        ReceiptAllocationId = allocation!.Id,
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
                AccountingDate = accountingDate,
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
                    AccountingDate = accountingDate,
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

            // Cek status pelunasan seluruh anggota batch (hanya keanggotaan aktif)
            var allSettled = activeItems.Count > 0 && activeItems.All(x => x.Receivable != null && (x.Receivable.OutstandingAmount <= 0 || x.Receivable.Status == FinReceivableStatuses.Settled));

            batch.Status = allSettled
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
        catch (Exception ex)
        {
            await RollbackAsync(transaction);

            var pgEx = ex as PostgresException ?? ex.InnerException as PostgresException;
            var memberIds = request.MemberAllocations?.Select(m => m.ReceivableId).ToList() ?? new List<Guid>();
            var outstandingAmount = memberMap?.Values.Sum(r => r.OutstandingAmount) ?? 0m;

            await _loggerService.ErrorAsync(
                LogCategory,
                "FinanceReceivableInvoiceBatch.PostPayment",
                $"Gagal memproses pembayaran batch tagihan AR {batchId}: {ex.Message}",
                ex,
                new
                {
                    BatchId = batchId,
                    ActorUserId = actorUserId,
                    BatchNumber = batch.BatchNumber,
                    BatchStatus = batch.Status,
                    ExpectedRowVersion = request.ExpectedRowVersion,
                    ActualRowVersion = batch.RowVersion,
                    ReceiptAmount = request.ReceiptAmount,
                    MemberReceivableIds = memberIds,
                    OutstandingAmount = outstandingAmount,
                    BankAccountId = request.BankAccountId,
                    PaymentDate = request.PaymentDate,
                    ExceptionType = ex.GetType().FullName,
                    InnerExceptionType = ex.InnerException?.GetType().FullName,
                    SqlState = pgEx?.SqlState,
                    ConstraintName = pgEx?.ConstraintName
                });

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

    private static async Task RollbackAsync(IDbContextTransaction? transaction)
    {
        if (transaction is null) return;
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch
        {
            // Abaikan kegagalan sekunder saat rollback untuk menjaga exception primer
        }
    }

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

    private static string ResolveVisitLabel(string? visitCode) => visitCode switch
    {
        "IP" => "Rawat Inap",
        "OP" => "Rawat Jalan",
        _ => string.Empty
    };

    // ====================================================================================
    // CANCELED INVOICE WORKFLOW
    // ====================================================================================

    private IQueryable<FinReceivableInvoiceBatch> BuildCancellationHistoryQuery()
    {
        return _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
            .Where(x => !x.IsDelete &&
                (x.CancelDateTime != null
                 || x.Status == FinReceivableInvoiceBatchStatuses.Cancelled
                 || x.ReissuedToBatchId != null
                 || x.IsCancel));
    }

    private static bool HasCancellationHistory(FinReceivableInvoiceBatch batch)
    {
        return batch.CancelDateTime != null
            || batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled
            || batch.ReissuedToBatchId != null
            || batch.IsCancel;
    }

    private static string ResolveCancellationReportStatus(FinReceivableInvoiceBatch batch)
    {
        // 1. Jika sudah di-reissue ke batch pengganti (ReissuedToBatchId != null),
        // invoice asal telah berhasil dibuat ulang menjadi batch baru.
        if (batch.ReissuedToBatchId.HasValue)
            return "RECREATED";

        // 2. Jika status operasional saat ini CANCELLED (dan belum di-reissue),
        // invoice masih dalam kondisi dibatalkan dan belum dibuat ulang.
        if (batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
            return "CANCELLED";

        // 3. Jika status operasional aktif (misalnya ISSUED, PARTIALLY_PAID, PAID)
        // dan memiliki riwayat pembatalan, berarti batch ini telah di-revive (diaktifkan kembali).
        if (batch.CancelDateTime.HasValue || batch.IsCancel)
            return "RECREATED";

        return "CANCELLED";
    }

    public async Task<CanceledInvoicePagedResponse> GetCanceledPagedAsync(
        CanceledInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var query = BuildCancellationHistoryQuery();

        query = ApplyCanceledFilter(query, request);
        query = ApplyCanceledSort(query, request);

        var total = await query.CountAsync(cancellationToken);
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;

        var pagedRows = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = await BuildCanceledRowsAsync(pagedRows, cancellationToken);

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

    private static IQueryable<FinReceivableInvoiceBatch> ApplyCanceledSort(
        IQueryable<FinReceivableInvoiceBatch> query, CanceledInvoiceBatchQuery request)
    {
        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "canceldatetime" : request.SortBy.Trim().ToLowerInvariant();

        return sortBy switch
        {
            "batchnumber" or "invoicenumber" => descending
                ? query.OrderByDescending(x => x.BatchNumber).ThenBy(x => x.Id)
                : query.OrderBy(x => x.BatchNumber).ThenBy(x => x.Id),
            "totalamount" => descending
                ? query.OrderByDescending(x => x.TotalAmount).ThenBy(x => x.Id)
                : query.OrderBy(x => x.TotalAmount).ThenBy(x => x.Id),
            "invoicedate" => descending
                ? query.OrderByDescending(x => x.InvoiceDate).ThenBy(x => x.Id)
                : query.OrderBy(x => x.InvoiceDate).ThenBy(x => x.Id),
            "duedate" => descending
                ? query.OrderByDescending(x => x.DueDate).ThenBy(x => x.Id)
                : query.OrderBy(x => x.DueDate).ThenBy(x => x.Id),
            "canceldatetime" or "cancelledat" => descending
                ? query.OrderByDescending(x => x.CancelDateTime).ThenBy(x => x.Id)
                : query.OrderBy(x => x.CancelDateTime).ThenBy(x => x.Id),
            "servicetype" => descending
                ? query.OrderByDescending(x => x.ServiceType).ThenBy(x => x.Id)
                : query.OrderBy(x => x.ServiceType).ThenBy(x => x.Id),
            _ => descending
                ? query.OrderByDescending(x => x.CancelDateTime ?? x.CreateDateTime).ThenBy(x => x.Id)
                : query.OrderBy(x => x.CancelDateTime ?? x.CreateDateTime).ThenBy(x => x.Id)
        };
    }

    /// <summary>Mengubah baris FinReceivableInvoiceBatch mentah menjadi CanceledInvoiceRowResponse,
    /// termasuk resolusi nama debitur/operator dan status kapabilitas dokumen. Dipakai bersama oleh
    /// GetCanceledPagedAsync (halaman saat ini) dan ExportCanceledListAsync (seluruh hasil filter),
    /// supaya kedua jalur selalu menghasilkan proyeksi baris yang identik.</summary>
    private async Task<List<CanceledInvoiceRowResponse>> BuildCanceledRowsAsync(
        List<FinReceivableInvoiceBatch> rows, CancellationToken cancellationToken)
    {
        var debtorIds = rows.Select(r => r.DebtorReferenceId).Distinct().ToList();
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

        var userIds = rows.Select(r => r.CancelBy).Concat(rows.Select(r => r.CreateBy)).Where(u => u != Guid.Empty).Distinct().ToList();
        var userNames = userIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? "-", cancellationToken)
            : new Dictionary<Guid, string>();

        var batchIds = rows.Select(b => b.Id).ToList();
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

        var reissuedToIds = rows
            .Where(r => r.ReissuedToBatchId.HasValue)
            .Select(r => r.ReissuedToBatchId!.Value)
            .Distinct()
            .ToList();
        var replacementNumbers = reissuedToIds.Count > 0
            ? await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
                .Where(b => reissuedToIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id, b => b.BatchNumber, cancellationToken)
            : new Dictionary<Guid, string>();

        return rows.Select(b =>
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
            var reportStatus = ResolveCancellationReportStatus(b);

            string? replNumber = null;
            if (b.ReissuedToBatchId.HasValue)
            {
                replacementNumbers.TryGetValue(b.ReissuedToBatchId.Value, out replNumber);
            }

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
                TotalDiscount = b.TotalDiscount,
                NetAmount = b.TotalAmount - b.TotalDiscount,
                ReportStatus = reportStatus,
                CurrentStatus = b.Status,
                Status = reportStatus,
                CancelledBy = b.CancelBy,
                CancelledByName = cancelledByName,
                CancelReason = b.CancelReason,
                ReissuedToBatchId = b.ReissuedToBatchId,
                ReissuedFromBatchId = b.ReissuedFromBatchId,
                ReplacementInvoiceNumber = replNumber,
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
    }

    /// <summary>Mengambil SELURUH baris Canceled Invoice yang cocok dengan filter (tanpa pagination) —
    /// dipakai khusus oleh export Excel/PDF, karena ekspor wajib merepresentasikan seluruh hasil
    /// filter, bukan hanya halaman yang sedang ditampilkan FE (lihat bagian G prompt task).</summary>
    private async Task<List<CanceledInvoiceRowResponse>> GetAllCanceledRowsForExportAsync(
        CanceledInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var query = BuildCancellationHistoryQuery();

        query = ApplyCanceledFilter(query, request);
        query = ApplyCanceledSort(query, request);

        var rows = await query.ToListAsync(cancellationToken);
        return await BuildCanceledRowsAsync(rows, cancellationToken);
    }

    public async Task<byte[]> ExportCanceledListExcelAsync(
        CanceledInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var rows = await GetAllCanceledRowsForExportAsync(request, cancellationToken);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Canceled Invoice");

        string[] headers =
        [
            "No", "No. Invoice", "Perusahaan/Penjamin", "Jenis Layanan", "Tanggal Invoice",
            "Tanggal Cancel", "Oleh", "Jatuh Tempo", "Total Tagihan", "Diskon", "Total Akhir",
            "Alasan Cancel", "Status"
        ];
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        var headerRange = ws.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

        var r = 2;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = r - 1;
            ws.Cell(r, 2).Value = row.InvoiceNumber;
            ws.Cell(r, 3).Value = row.DebtorName ?? "-";
            ws.Cell(r, 4).Value = row.ServiceTypeName ?? "-";
            ws.Cell(r, 5).Value = row.InvoiceDate.HasValue ? row.InvoiceDate.Value.ToString("dd/MM/yyyy") : "-";
            ws.Cell(r, 6).Value = row.CancelledAt.HasValue
                ? FinanceBusinessDate.ToBusinessTime(row.CancelledAt.Value).ToString("dd/MM/yyyy HH:mm")
                : "-";
            ws.Cell(r, 7).Value = row.CancelledByName ?? "-";
            ws.Cell(r, 8).Value = row.DueDate.HasValue ? row.DueDate.Value.ToString("dd/MM/yyyy") : "-";
            ws.Cell(r, 9).Value = row.TotalAmount;
            ws.Cell(r, 10).Value = row.TotalDiscount;
            ws.Cell(r, 11).Value = row.NetAmount;
            ws.Cell(r, 12).Value = row.CancelReason ?? "-";
            ws.Cell(r, 13).Value = row.ReportStatus == "RECREATED" ? "Sudah Dibuat Ulang" : "Cancelled";
            r++;
        }

        if (r > 2)
            ws.Range(2, 9, r - 1, 11).Style.NumberFormat.Format = "#,##0";

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportCanceledListPdfAsync(
        CanceledInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var rows = await GetAllCanceledRowsForExportAsync(request, cancellationToken);
        var summary = await GetCanceledSummaryAsync(request, cancellationToken);

        return CanceledInvoiceReportDocuments.BuildListPdf(rows, summary);
    }

    public async Task<byte[]> ExportCanceledDetailExcelAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await GetCanceledDetailAsync(id, cancellationToken);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Canceled Invoice Detail");

        ws.Cell(1, 1).Value = "No. Invoice";
        ws.Cell(1, 2).Value = detail.InvoiceNumber;
        ws.Cell(2, 1).Value = "Perusahaan/Penjamin";
        ws.Cell(2, 2).Value = detail.DebtorName ?? "-";
        ws.Cell(3, 1).Value = "Tanggal Cancel";
        ws.Cell(3, 2).Value = detail.CancelledAt.HasValue
            ? FinanceBusinessDate.ToBusinessTime(detail.CancelledAt.Value).ToString("dd/MM/yyyy HH:mm")
            : "-";
        ws.Cell(4, 1).Value = "Oleh";
        ws.Cell(4, 2).Value = detail.CancelledByName ?? "-";
        ws.Cell(5, 1).Value = "Alasan Cancel";
        ws.Cell(5, 2).Value = detail.CancelReason ?? "-";
        ws.Cell(6, 1).Value = "Tanggal Invoice";
        ws.Cell(6, 2).Value = detail.InvoiceDate.HasValue ? detail.InvoiceDate.Value.ToString("dd/MM/yyyy") : "-";
        ws.Cell(7, 1).Value = "Tanggal Jatuh Tempo";
        ws.Cell(7, 2).Value = detail.DueDate.HasValue ? detail.DueDate.Value.ToString("dd/MM/yyyy") : "-";
        ws.Cell(8, 1).Value = "Jenis Invoice";
        ws.Cell(8, 2).Value = detail.ServiceTypeName ?? "-";
        ws.Range(1, 1, 8, 1).Style.Font.Bold = true;

        var headerRow = 10;
        string[] headers = ["No", "Nama Pasien", "No. RM", "No. Bill", "No. Reg", "Tanggal Kunjungan", "Total Tagihan", "Diskon", "Total Akhir"];
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(headerRow, i + 1).Value = headers[i];
        var headerRange = ws.Range(headerRow, 1, headerRow, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

        var r = headerRow + 1;
        foreach (var m in detail.Items)
        {
            ws.Cell(r, 1).Value = r - headerRow;
            ws.Cell(r, 2).Value = m.PatientName;
            ws.Cell(r, 3).Value = m.MedicalRecordNumber;
            ws.Cell(r, 4).Value = m.BillingNumber;
            ws.Cell(r, 5).Value = m.RegistrationNumber;
            ws.Cell(r, 6).Value = m.VisitDate.HasValue ? m.VisitDate.Value.ToString("dd/MM/yyyy") : "-";
            ws.Cell(r, 7).Value = m.EditableTotalAmount;
            ws.Cell(r, 8).Value = m.DiscountAmount;
            ws.Cell(r, 9).Value = m.FinalAmount;
            r++;
        }
        if (r > headerRow + 1)
            ws.Range(headerRow + 1, 7, r - 1, 9).Style.NumberFormat.Format = "#,##0";

        var summaryRow = r + 1;
        ws.Cell(summaryRow, 1).Value = "Jumlah Detail";
        ws.Cell(summaryRow, 2).Value = detail.Summary.ItemCount;
        ws.Cell(summaryRow + 1, 1).Value = "Total Piutang";
        ws.Cell(summaryRow + 1, 2).Value = detail.Summary.TotalAmount;
        ws.Cell(summaryRow + 2, 1).Value = "Total Diskon";
        ws.Cell(summaryRow + 2, 2).Value = detail.Summary.TotalDiscount;
        ws.Cell(summaryRow + 3, 1).Value = "Total Akhir";
        ws.Cell(summaryRow + 3, 2).Value = detail.Summary.NetAmount;
        ws.Range(summaryRow, 1, summaryRow + 3, 1).Style.Font.Bold = true;
        ws.Range(summaryRow, 2, summaryRow + 3, 2).Style.NumberFormat.Format = "#,##0";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportCanceledDetailPdfAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await GetCanceledDetailAsync(id, cancellationToken);
        return CanceledInvoiceReportDocuments.BuildDetailPdf(detail);
    }

    public async Task<CanceledInvoiceSummaryResponse> GetCanceledSummaryAsync(
        CanceledInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var categorySupported = IsCanceledCategorySupported(request.Category, out var categoryLimitationReason);

        // Total Filter: agregat atas seluruh filter aktif (search, rentang tanggal, jenis layanan,
        // DebtorReferenceId, kategori). Merepresentasikan persis isi tabel yang sedang ditampilkan FE.
        var query = BuildCancellationHistoryQuery();
        query = ApplyCanceledFilter(query, request);

        var agg = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(x => x.TotalAmount),
                TotalDiscount = g.Sum(x => x.TotalDiscount),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        string targetName = "Semua Perusahaan";
        decimal targetAmount = 0m;
        if (categorySupported)
        {
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

                // Total Target: akumulasi sepanjang masa untuk target yang dipilih, TANPA rentang
                // tanggal/search/jenis layanan — definisi bisnisnya "berapa total yang pernah dibatalkan
                // untuk perusahaan ini", bukan "berapa yang cocok dengan filter saat ini" (itu Total Filter).
                targetAmount = await BuildCancellationHistoryQuery()
                    .Where(x => x.DebtorReferenceId == request.DebtorReferenceId.Value)
                    .SumAsync(x => x.TotalAmount, cancellationToken);
            }
            else
            {
                // Tidak ada target spesifik dipilih: Total Target = total seluruh invoice dengan riwayat pembatalan
                // sepanjang masa (tanpa filter apa pun), konsisten dengan TargetName "Semua Perusahaan".
                targetAmount = await BuildCancellationHistoryQuery()
                    .SumAsync(x => x.TotalAmount, cancellationToken);
            }
        }

        var totalAmount = agg?.TotalAmount ?? 0m;
        var totalDiscount = agg?.TotalDiscount ?? 0m;
        return new CanceledInvoiceSummaryResponse
        {
            TotalAmount = totalAmount,
            TotalDiscount = totalDiscount,
            NetAmount = totalAmount - totalDiscount,
            InvoiceCount = agg?.Count ?? 0,
            TargetAmount = targetAmount,
            TargetName = targetName,
            CategorySupported = categorySupported,
            CategoryLimitationReason = categoryLimitationReason
        };
    }

    public async Task<CanceledInvoiceDetailResponse> GetCanceledDetailAsync(
        Guid id, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Canceled Invoice tidak ditemukan.");

        if (!HasCancellationHistory(batch))
            throw new ReceivableInvoiceBatchValidationException("Batch tagihan tidak memiliki riwayat pembatalan.");

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
                .Select(u => u.DisplayName ?? u.UserName)
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
                .Select(x => new BatchMemberEncounterRow
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    DoctorId = x.DoctorId,
                    PatientClassId = x.PatientClassId,
                    EncounterNumber = x.EncounterNumber,
                    EncounterType = x.EncounterType,
                    EncounterDate = x.EncounterDate
                })
                .ToDictionaryAsync(x => x.Id, cancellationToken)
            : new Dictionary<Guid, BatchMemberEncounterRow>();

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
                BatchMemberEncounterRow? enc = encId.HasValue && encounters.TryGetValue(encId.Value, out var foundEnc) ? foundEnc : null;
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
                    VisitDate = enc != null ? DateOnly.FromDateTime(enc.EncounterDate) : null,
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

        var reportStatus = ResolveCancellationReportStatus(batch);
        bool canReissue = batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled
            && !batch.ReissuedToBatchId.HasValue
            && pendingAdjustments.Count == 0;
        string? reissueBlockedReason = null;
        if (batch.Status != FinReceivableInvoiceBatchStatuses.Cancelled || batch.ReissuedToBatchId.HasValue)
        {
            reissueBlockedReason = "Batch tagihan ini sudah pernah dibuat ulang / dipulihkan.";
        }
        else if (pendingAdjustments.Count > 0)
        {
            reissueBlockedReason = "Perubahan nominal masih menunggu persetujuan koreksi piutang.";
        }

        string? replacementInvoiceNumber = null;
        if (batch.ReissuedToBatchId.HasValue)
        {
            replacementInvoiceNumber = await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
                .Where(b => b.Id == batch.ReissuedToBatchId.Value)
                .Select(b => b.BatchNumber)
                .FirstOrDefaultAsync(cancellationToken);
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
            ReportStatus = reportStatus,
            CurrentStatus = batch.Status,
            Status = reportStatus,
            WasCanceled = true,
            WasRecreated = reportStatus == "RECREATED",
            TotalAmount = batch.TotalAmount,
            TotalDiscount = batch.TotalDiscount,
            NetAmount = batch.TotalAmount - batch.TotalDiscount,
            RowVersion = batch.RowVersion,
            ReissuedToBatchId = batch.ReissuedToBatchId,
            ReissuedFromBatchId = batch.ReissuedFromBatchId,
            ReplacementInvoiceNumber = replacementInvoiceNumber,
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
            Summary = new CanceledInvoiceDetailSummary
            {
                ItemCount = memberResponses.Count,
                TotalAmount = batch.TotalAmount,
                TotalDiscount = batch.TotalDiscount,
                NetAmount = batch.TotalAmount - batch.TotalDiscount
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

        var newTotalAmount = receivables.Sum(r => r.OriginalAmount - r.AdjustedAmount - r.WrittenOffAmount);

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
            Status = FinReceivableInvoiceBatchStatuses.Issued,
            IssuedAt = DateTimeOffset.UtcNow,
            ClaimStatus = null,
            ReissuedFromBatchId = batch.Id,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            UpdateBy = actorUserId
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
                UpdateBy = actorUserId
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

    /// <summary>
    /// Report Canceled Invoice hanya bersumber dari FinReceivableInvoiceBatch, yang menurut FIN-DEC-048
    /// (lihat CreateAsync) HANYA PERNAH menampung piutang DebtorType PAYER ("Perusahaan"). Kategori
    /// "Pasien Umum" (FinReceivableDebtorTypes.PatientGuarantor) dan "Karyawan"
    /// (FinReceivableDebtorTypes.EmployeeBenefit) memang terdaftar sebagai DebtorType yang sah pada
    /// FinReceivable, dan FinReceivableStatuses.Cancelled juga terdaftar sebagai status yang sah — tetapi
    /// tidak ada satu pun service di backend ini yang pernah menulis Status=CANCELLED untuk piutang
    /// non-PAYER (diverifikasi: seluruh referensi FinReceivableStatuses.Cancelled di codebase adalah
    /// pembacaan guard-clause, tidak ada assignment). FinReceivable juga tidak memiliki kolom audit
    /// pembatalan (CancelDateTime/CancelBy/CancelReason) seperti FinReceivableInvoiceBatch. Jalur
    /// pembatalan untuk dua kategori ini belum pernah dibangun di V2 — bukan sekadar belum di-query.
    /// Mengembalikan false (bukan melempar exception) agar caller bisa memilih perilaku aman: hasil
    /// kosong plus alasan eksplisit, bukan data fiktif.
    /// </summary>
    private static bool IsCanceledCategorySupported(string? category, out string? limitationReason)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            limitationReason = null;
            return true;
        }

        var cat = category.Trim().ToUpperInvariant();
        if (cat is "PATIENT" or "EMPLOYEE" or "PASIEN" or "KARYAWAN" or "PATIENT_GUARANTOR" or "EMPLOYEE_BENEFIT")
        {
            limitationReason =
                "Kategori ini belum didukung Report Canceled Invoice V2. Batch Tagihan AR (FinReceivableInvoiceBatch) " +
                "hanya pernah dibuat untuk piutang berjenis Perusahaan (PAYER, FIN-DEC-048). Piutang Pasien Umum/Karyawan " +
                "tidak pernah memiliki jalur pembatalan di backend saat ini, sehingga tidak ada data yang bisa ditampilkan.";
            return false;
        }

        limitationReason = null;
        return true;
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
                // FinanceBusinessDate adalah satu-satunya tempat konversi zona waktu di Finance
                // (FIN-DES-082, FIN-DEC-116) — batas hari dihitung menurut kalender WIB, bukan UTC
                // mentah, agar tanggal awal/akhir filter tetap inklusif sesuai hari bisnis pengguna.
                if (request.StartDate.HasValue)
                {
                    var startDt = FinanceBusinessDate.GetStartOfDayUtc(request.StartDate.Value);
                    query = query.Where(x => x.CancelDateTime >= startDt);
                }
                if (request.EndDate.HasValue)
                {
                    var endDt = FinanceBusinessDate.GetEndOfPeriodUtc(request.EndDate.Value);
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

        if (!IsCanceledCategorySupported(request.Category, out _))
        {
            // FinReceivableInvoiceBatch hanya pernah menampung piutang DebtorType PAYER (FIN-DEC-048);
            // tidak ada data CANCELLED untuk kategori ini di skema V2 saat ini. Lihat
            // IsCanceledCategorySupported untuk penjelasan lengkap. Mengembalikan kosong secara sengaja
            // alih-alih error 400, agar FE tetap bisa merender halaman kosong + pesan keterbatasan dari
            // CategorySupported/CategoryLimitationReason pada summary.
            query = query.Where(x => false);
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

    // ====================================================================================
    // REPORT AR CREATED (Laporan Pembuatan AR)
    // ====================================================================================

    private sealed class BatchMemberEncounterRow
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? PatientClassId { get; set; }
        public string? EncounterNumber { get; set; }
        public EncounterType EncounterType { get; set; }
        public DateTime EncounterDate { get; set; }
    }

    private sealed class ClaimSingleEncounterRow
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? EncounterNumber { get; set; }
    }

    private sealed class ReportArCreatedInternalRow
    {
        public FinReceivableInvoiceBatchItem Bi { get; set; } = null!;
        public FinReceivableInvoiceBatch Batch { get; set; } = null!;
        public FinReceivable Rec { get; set; } = null!;
        public BilInvoice? Invoice { get; set; }
        public EncounterType? EncounterType { get; set; }
        public DateTimeOffset? EncounterDate { get; set; }
        public string? EncounterNumber { get; set; }
        public MstPatient? Patient { get; set; }
        public string? CompanyGuarantorName { get; set; }
        public string? InsuranceProviderName { get; set; }
    }

    private static (DateOnly? StartDate, DateOnly? EndDate, string PeriodLabel) ResolveReportArCreatedPeriod(
        string? periodType, DateOnly? customStart, DateOnly? customEnd)
    {
        if (customStart.HasValue || customEnd.HasValue)
        {
            var sLabel = customStart?.ToString("dd/MM/yyyy") ?? "";
            var eLabel = customEnd?.ToString("dd/MM/yyyy") ?? "";
            var label = customStart.HasValue && customEnd.HasValue ? $"{sLabel} - {eLabel}"
                : customStart.HasValue ? $"Mulai {sLabel}" : $"Sampai {eLabel}";
            return (customStart, customEnd, label);
        }

        var today = FinanceBusinessDate.Today();
        var period = (periodType ?? "ALL").Trim().ToUpperInvariant();

        return period switch
        {
            "TODAY" => (today, today, "Hari Ini"),
            "THIS_WEEK" => (today.AddDays(-(int)today.DayOfWeek), today.AddDays(6 - (int)today.DayOfWeek), "Minggu Ini"),
            "THIS_MONTH" => (new DateOnly(today.Year, today.Month, 1), FinanceBusinessDate.GetPeriodEndDate(today.Year, today.Month), "Bulan Ini"),
            "LAST_MONTH" =>
                today.Month == 1
                    ? (new DateOnly(today.Year - 1, 12, 1), new DateOnly(today.Year - 1, 12, 31), "Bulan Lalu")
                    : (new DateOnly(today.Year, today.Month - 1, 1), FinanceBusinessDate.GetPeriodEndDate(today.Year, today.Month - 1), "Bulan Lalu"),
            "THIS_YEAR" => (new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31), "Tahun Ini"),
            _ => (null, null, "Semua Periode")
        };
    }

    private IQueryable<ReportArCreatedInternalRow> BuildReportArCreatedBaseQuery(ReportArCreatedQuery request)
    {
        var (startDate, endDate, _) = ResolveReportArCreatedPeriod(request.PeriodType, request.StartDate, request.EndDate);

        var query = from bi in _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                    where !bi.IsDelete && bi.IsActiveMembership
                    join b in _dbContext.FinReceivableInvoiceBatches.AsNoTracking() on bi.BatchId equals b.Id
                    where !b.IsDelete
                    join r in _dbContext.FinReceivables.AsNoTracking() on bi.ReceivableId equals r.Id
                    where !r.IsDelete
                    join inv in _dbContext.BilInvoices.AsNoTracking() on r.InvoiceId equals (Guid?)inv.Id into invGroup
                    from inv in invGroup.DefaultIfEmpty()
                    join enc in _dbContext.RegPatientEncounters.AsNoTracking() on (inv != null ? (Guid?)inv.EncounterId : null) equals (Guid?)enc.Id into encGroup
                    from enc in encGroup.DefaultIfEmpty()
                    join pat in _dbContext.MstPatients.AsNoTracking() on (enc != null ? (Guid?)enc.PatientId : null) equals (Guid?)pat.Id into patGroup
                    from pat in patGroup.DefaultIfEmpty()
                    join c in _dbContext.MstCompanyGuarantors.AsNoTracking() on b.DebtorReferenceId equals c.Id into cGroup
                    from c in cGroup.DefaultIfEmpty()
                    join p in _dbContext.MstInsuranceProviders.AsNoTracking() on b.DebtorReferenceId equals p.Id into pGroup
                    from p in pGroup.DefaultIfEmpty()
                    select new ReportArCreatedInternalRow
                    {
                        Bi = bi,
                        Batch = b,
                        Rec = r,
                        Invoice = inv,
                        EncounterType = enc != null ? (EncounterType?)enc.EncounterType : null,
                        EncounterDate = enc != null ? (DateTimeOffset?)enc.EncounterDate : null,
                        EncounterNumber = enc != null ? enc.EncounterNumber : null,
                        Patient = pat,
                        CompanyGuarantorName = c != null ? c.CompanyGuarantorName : null,
                        InsuranceProviderName = p != null ? p.InsuranceProviderName : null
                    };

        // 1. Status Filter (default: exclude CANCELLED)
        if (!string.IsNullOrWhiteSpace(request.Status) && !request.Status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var reqStatus = request.Status.Trim();
            query = query.Where(x => x.Batch.Status == reqStatus);
        }
        else
        {
            query = query.Where(x => x.Batch.Status != FinReceivableInvoiceBatchStatuses.Cancelled);
        }

        // 2. Filter Penjamin (DebtorReferenceId)
        if (request.DebtorReferenceId.HasValue && request.DebtorReferenceId.Value != Guid.Empty)
        {
            query = query.Where(x => x.Batch.DebtorReferenceId == request.DebtorReferenceId.Value);
        }

        // 3. Filter Tanggal
        var dateType = (request.DateType ?? "INVOICE_DATE").Trim().ToUpperInvariant();
        if (dateType == "REGISTRATION_DATE")
        {
            if (startDate.HasValue)
            {
                var startUtc = FinanceBusinessDate.GetStartOfDayUtc(startDate.Value);
                query = query.Where(x => x.EncounterDate != null && x.EncounterDate >= startUtc);
            }
            if (endDate.HasValue)
            {
                var endUtc = FinanceBusinessDate.GetEndOfPeriodUtc(endDate.Value);
                query = query.Where(x => x.EncounterDate != null && x.EncounterDate <= endUtc);
            }
        }
        else // INVOICE_DATE (Default)
        {
            if (startDate.HasValue)
            {
                query = query.Where(x => x.Batch.InvoiceDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(x => x.Batch.InvoiceDate <= endDate.Value);
            }
        }

        // 4. Filter Jenis Kunjungan (VisitType)
        if (!string.IsNullOrWhiteSpace(request.VisitType) && !request.VisitType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var vt = request.VisitType.Trim().ToUpperInvariant();
            if (vt == "RAJAL" || vt == "OP" || vt == "OUTPATIENT")
            {
                query = query.Where(x =>
                    (x.EncounterType != null && x.EncounterType == EncounterType.Outpatient)
                    || (x.Batch.VisitCode == "OP" || EF.Functions.ILike(x.Batch.BatchNumber, "%/OP/%")));
            }
            else if (vt == "RANAP" || vt == "IP" || vt == "INPATIENT")
            {
                query = query.Where(x =>
                    (x.EncounterType != null && x.EncounterType == EncounterType.Inpatient)
                    || (x.Batch.VisitCode == "IP" || EF.Functions.ILike(x.Batch.BatchNumber, "%/IP/%")));
            }
            else if (vt == "IGD" || vt == "EMERGENCY")
            {
                query = query.Where(x =>
                    (x.EncounterType != null && x.EncounterType == EncounterType.Emergency)
                    || (x.Batch.VisitCode == "IGD" || EF.Functions.ILike(x.Batch.BatchNumber, "%/IGD/%")));
            }
        }

        // 5. Filter Pencarian (Search)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Batch.BatchNumber, pattern)
                || (x.Invoice != null && EF.Functions.ILike(x.Invoice.InvoiceNumber, pattern))
                || (x.EncounterNumber != null && EF.Functions.ILike(x.EncounterNumber, pattern))
                || (x.Patient != null && (
                    EF.Functions.ILike(x.Patient.MedicalRecordNumber, pattern)
                    || EF.Functions.ILike(x.Patient.FullName, pattern)))
                || (x.CompanyGuarantorName != null && EF.Functions.ILike(x.CompanyGuarantorName, pattern))
                || (x.InsuranceProviderName != null && EF.Functions.ILike(x.InsuranceProviderName, pattern)));
        }

        return query;
    }

    private static IQueryable<ReportArCreatedInternalRow> ApplyReportArCreatedSort(
        IQueryable<ReportArCreatedInternalRow> query, ReportArCreatedQuery request)
    {
        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (request.SortBy ?? "company").Trim().ToLowerInvariant() switch
        {
            "invoicenumber" => descending
                ? query.OrderByDescending(x => x.Batch.BatchNumber).ThenBy(x => x.Bi.Id)
                : query.OrderBy(x => x.Batch.BatchNumber).ThenBy(x => x.Bi.Id),
            "billingnumber" => descending
                ? query.OrderByDescending(x => x.Invoice != null ? x.Invoice.InvoiceNumber : "").ThenBy(x => x.Bi.Id)
                : query.OrderBy(x => x.Invoice != null ? x.Invoice.InvoiceNumber : "").ThenBy(x => x.Bi.Id),
            "patientname" => descending
                ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : "").ThenBy(x => x.Bi.Id)
                : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : "").ThenBy(x => x.Bi.Id),
            "medicalrecordnumber" => descending
                ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : "").ThenBy(x => x.Bi.Id)
                : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : "").ThenBy(x => x.Bi.Id),
            "billingamount" or "amount" => descending
                ? query.OrderByDescending(x => x.Rec.OriginalAmount).ThenBy(x => x.Bi.Id)
                : query.OrderBy(x => x.Rec.OriginalAmount).ThenBy(x => x.Bi.Id),
            "invoicedate" => descending
                ? query.OrderByDescending(x => x.Batch.InvoiceDate).ThenBy(x => x.Batch.BatchNumber).ThenBy(x => x.Bi.Id)
                : query.OrderBy(x => x.Batch.InvoiceDate).ThenBy(x => x.Batch.BatchNumber).ThenBy(x => x.Bi.Id),
            "registrationdate" => descending
                ? query.OrderByDescending(x => x.EncounterDate).ThenBy(x => x.Bi.Id)
                : query.OrderBy(x => x.EncounterDate).ThenBy(x => x.Bi.Id),
            _ => descending
                ? query.OrderByDescending(x => x.CompanyGuarantorName != null ? x.CompanyGuarantorName : (x.InsuranceProviderName != null ? x.InsuranceProviderName : ""))
                       .ThenByDescending(x => x.Batch.InvoiceDate)
                       .ThenBy(x => x.Batch.BatchNumber)
                       .ThenBy(x => x.Bi.Id)
                : query.OrderBy(x => x.CompanyGuarantorName != null ? x.CompanyGuarantorName : (x.InsuranceProviderName != null ? x.InsuranceProviderName : ""))
                       .ThenByDescending(x => x.Batch.InvoiceDate)
                       .ThenBy(x => x.Batch.BatchNumber)
                       .ThenBy(x => x.Bi.Id)
        };
    }

    private static ReportArCreatedItemResponse MapReportArCreatedItem(ReportArCreatedInternalRow row)
    {
        var companyName = row.CompanyGuarantorName ?? row.InsuranceProviderName ?? "-";
        var visitType = row.EncounterType != null
            ? (row.EncounterType == EncounterType.Inpatient ? "RANAP"
               : row.EncounterType == EncounterType.Emergency ? "IGD"
               : "RAJAL")
            : (!string.IsNullOrWhiteSpace(row.Batch.VisitCode) ? (row.Batch.VisitCode == "IP" ? "RANAP" : row.Batch.VisitCode == "OP" ? "RAJAL" : row.Batch.VisitCode)
               : (!string.IsNullOrWhiteSpace(row.Batch.ServiceType) ? row.Batch.ServiceType : "RAJAL"));

        var visitTypeName = visitType switch
        {
            "RANAP" => "Rawat Inap",
            "RAJAL" => "Rawat Jalan",
            "IGD" => "IGD",
            _ => visitType
        };

        DateTimeOffset? invoiceDateTime = row.Batch.InvoiceDate.HasValue
            ? FinanceBusinessDate.GetStartOfDayUtc(row.Batch.InvoiceDate.Value)
            : row.Batch.CreateDateTime;

        return new ReportArCreatedItemResponse
        {
            Id = row.Bi.Id,
            BatchId = row.Batch.Id,
            BatchItemId = row.Bi.Id,
            ReceivableId = row.Rec.Id,
            InvoiceId = row.Invoice?.Id,
            InvoiceNumber = row.Batch.BatchNumber,
            CompanyId = row.Batch.DebtorReferenceId,
            CompanyName = companyName,
            InvoiceDate = invoiceDateTime,
            RegistrationDate = row.EncounterDate ?? row.Invoice?.InvoiceDate,
            BillingNumber = row.Invoice?.InvoiceNumber ?? "-",
            RegistrationNumber = row.EncounterNumber ?? "-",
            MedicalRecordNumber = row.Patient?.MedicalRecordNumber ?? "-",
            PatientName = row.Patient?.FullName ?? "-",
            VisitType = visitType,
            VisitTypeName = visitTypeName,
            BillingAmount = row.Rec.OriginalAmount,
            BatchStatus = row.Batch.Status
        };
    }

    private async Task<string> ResolveDebtorFilterLabelAsync(Guid? debtorId, CancellationToken cancellationToken)
    {
        if (!debtorId.HasValue || debtorId.Value == Guid.Empty) return "Semua Perusahaan";
        var c = await _dbContext.MstCompanyGuarantors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == debtorId.Value && !x.IsDelete, cancellationToken);
        if (c != null) return c.CompanyGuarantorName;
        var p = await _dbContext.MstInsuranceProviders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == debtorId.Value && !x.IsDelete, cancellationToken);
        if (p != null) return p.InsuranceProviderName;
        return "Penjamin Terpilih";
    }

    private static List<ReportArCreatedGroupResponse> BuildReportArCreatedGroups(
        List<ReportArCreatedItemResponse> items,
        Dictionary<Guid, decimal>? companyGlobalTotals = null)
    {
        var groups = new List<ReportArCreatedGroupResponse>();
        var companyGroups = items.GroupBy(x => new { x.CompanyId, x.CompanyName });

        foreach (var cg in companyGroups)
        {
            var groupItems = cg.ToList();
            var pageSubtotal = groupItems.Sum(x => x.BillingAmount);
            var totalCompany = (companyGlobalTotals != null && companyGlobalTotals.TryGetValue(cg.Key.CompanyId, out var globalTotal))
                ? globalTotal
                : pageSubtotal;

            var dateSubtotals = groupItems
                .GroupBy(x => x.InvoiceDate.HasValue
                    ? FinanceBusinessDate.ToBusinessTime(x.InvoiceDate.Value).ToString("yyyy-MM-dd")
                    : "NO_DATE")
                .Select(dg =>
                {
                    var dFormatted = dg.Key == "NO_DATE"
                        ? "Tanpa Tanggal"
                        : (dg.First().InvoiceDate.HasValue
                            ? FinanceBusinessDate.ToBusinessTime(dg.First().InvoiceDate!.Value).ToString("dd MMMM yyyy")
                            : dg.Key);
                    return new ReportArCreatedDateSubtotal
                    {
                        Date = dg.Key,
                        DateFormatted = dFormatted,
                        SubtotalAmount = dg.Sum(x => x.BillingAmount),
                        RowCount = dg.Count()
                    };
                })
                .ToList();

            groups.Add(new ReportArCreatedGroupResponse
            {
                CompanyId = cg.Key.CompanyId,
                CompanyName = cg.Key.CompanyName,
                TotalCompanyAmount = totalCompany,
                PageSubtotalAmount = pageSubtotal,
                DateSubtotals = dateSubtotals,
                Items = groupItems
            });
        }

        return groups;
    }

    public async Task<ReportArCreatedSummaryResponse> GetReportArCreatedSummaryAsync(
        ReportArCreatedQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = BuildReportArCreatedBaseQuery(request);

        var count = await baseQuery.CountAsync(cancellationToken);
        var (_, _, periodLabel) = ResolveReportArCreatedPeriod(request.PeriodType, request.StartDate, request.EndDate);
        var selectedCompanyName = await ResolveDebtorFilterLabelAsync(request.DebtorReferenceId, cancellationToken);

        if (count == 0)
        {
            return new ReportArCreatedSummaryResponse
            {
                PeriodLabel = periodLabel,
                SelectedCompanyName = selectedCompanyName
            };
        }

        var totalBillingAmount = await baseQuery.SumAsync(x => x.Rec.OriginalAmount, cancellationToken);
        var totalInvoiceCount = await baseQuery.Select(x => x.Batch.Id).Distinct().CountAsync(cancellationToken);

        var companyGroupData = await baseQuery
            .GroupBy(x => new
            {
                CompanyId = x.Batch.DebtorReferenceId,
                CompanyName = x.CompanyGuarantorName != null ? x.CompanyGuarantorName : (x.InsuranceProviderName != null ? x.InsuranceProviderName : "-")
            })
            .Select(g => new ReportArCreatedCompanySummary
            {
                CompanyId = g.Key.CompanyId,
                CompanyName = g.Key.CompanyName,
                TotalAmount = g.Sum(x => x.Rec.OriginalAmount),
                RowCount = g.Count(),
                InvoiceCount = g.Select(x => x.Batch.Id).Distinct().Count()
            })
            .OrderByDescending(g => g.TotalAmount)
            .ToListAsync(cancellationToken);

        var visitTypeGroupData = await baseQuery
            .GroupBy(x => x.EncounterType ?? EncounterType.Unknown)
            .Select(g => new
            {
                EncounterType = g.Key,
                Count = g.Count(),
                Amount = g.Sum(x => x.Rec.OriginalAmount)
            })
            .ToListAsync(cancellationToken);

        var visitTypeSummary = visitTypeGroupData.Select(v => new ReportArCreatedVisitTypeSummary
        {
            VisitType = v.EncounterType == EncounterType.Inpatient ? "RANAP"
                : v.EncounterType == EncounterType.Emergency ? "IGD"
                : "RAJAL",
            VisitTypeName = v.EncounterType == EncounterType.Inpatient ? "Rawat Inap"
                : v.EncounterType == EncounterType.Emergency ? "IGD"
                : "Rawat Jalan",
            Count = v.Count,
            Amount = v.Amount
        }).ToList();

        return new ReportArCreatedSummaryResponse
        {
            TotalInvoiceCount = totalInvoiceCount,
            TotalDetailRows = count,
            TotalBillingAmount = totalBillingAmount,
            PeriodLabel = periodLabel,
            SelectedCompanyName = selectedCompanyName,
            TopCompanySummary = companyGroupData.FirstOrDefault(),
            TotalByCompany = companyGroupData,
            VisitTypeSummary = visitTypeSummary
        };
    }

    public async Task<ReportArCreatedPagedResponse> GetReportArCreatedPagedAsync(
        ReportArCreatedQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 30); // Strictly max 30 enforced
        var pageNumber = Math.Max(1, request.PageNumber);

        var baseQuery = BuildReportArCreatedBaseQuery(request);
        var sortedQuery = ApplyReportArCreatedSort(baseQuery, request);

        var summary = await GetReportArCreatedSummaryAsync(request, cancellationToken);

        var pagedRows = await sortedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = pagedRows.Select(MapReportArCreatedItem).ToList();

        var companyTotalsDict = summary.TotalByCompany.ToDictionary(c => c.CompanyId, c => c.TotalAmount);
        var groups = BuildReportArCreatedGroups(items, companyTotalsDict);

        var totalData = summary.TotalDetailRows;
        var totalPage = totalData == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize);

        return new ReportArCreatedPagedResponse
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = totalPage,
            Items = items,
            Groups = groups,
            Summary = summary
        };
    }

    public async Task<byte[]> ExportReportArCreatedExcelAsync(
        ReportArCreatedQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = BuildReportArCreatedBaseQuery(request);
        var sortedQuery = ApplyReportArCreatedSort(baseQuery, request);

        var allRows = await sortedQuery.ToListAsync(cancellationToken);
        var items = allRows.Select(MapReportArCreatedItem).ToList();
        var summary = await GetReportArCreatedSummaryAsync(request, cancellationToken);
        var groups = BuildReportArCreatedGroups(items);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Laporan Pembuatan AR");

        // Title
        ws.Cell(1, 1).Value = "LAPORAN PEMBUATAN AR (REPORT AR CREATED)";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#087d91");

        // Metadata
        ws.Cell(2, 1).Value = $"Periode: {summary.PeriodLabel}  |  Penjamin: {summary.SelectedCompanyName}";
        ws.Cell(2, 1).Style.Font.FontSize = 10;
        ws.Cell(2, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#475569");

        var printTimeWib = FinanceBusinessDate.ToBusinessTime(DateTimeOffset.UtcNow);
        ws.Cell(3, 1).Value = $"Dicetak pada: {printTimeWib:dd/MM/yyyy HH:mm} WIB  |  Total: {summary.TotalDetailRows} Tagihan ({summary.TotalInvoiceCount} Invoice AR)  |  Rp {summary.TotalBillingAmount:N0}";
        ws.Cell(3, 1).Style.Font.FontSize = 9;
        ws.Cell(3, 1).Style.Font.Italic = true;
        ws.Cell(3, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#64748b");

        // Table Header (Row 5)
        var headerRow = 5;
        string[] headers =
        [
            "NO", "TANGGAL INVOICE", "TANGGAL REGISTRASI", "NO. BILL",
            "NO. REG", "NO. RM", "NAMA PASIEN", "NO. INVOICE", "JUMLAH TAGIHAN"
        ];

        for (var c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 9;
            cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#087d91");
            cell.Style.Alignment.Horizontal = (c == 0 || c == 1 || c == 2 || c == 4 || c == 5)
                ? ClosedXML.Excel.XLAlignmentHorizontalValues.Center
                : c == 8 ? ClosedXML.Excel.XLAlignmentHorizontalValues.Right
                : ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#065f6d");
        }

        var r = headerRow + 1;
        var globalIdx = 1;

        foreach (var group in groups)
        {
            // Group Header (Rowspan/Colspan 9)
            var grpRange = ws.Range(r, 1, r, 9);
            grpRange.Merge();
            grpRange.Value = group.CompanyName.ToUpperInvariant();
            grpRange.Style.Font.Bold = true;
            grpRange.Style.Font.FontSize = 9.5;
            grpRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#d9d9d9");
            grpRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#071323");
            grpRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            r++;

            // Items grouped by Date
            var dateItemsMap = group.Items
                .GroupBy(x => x.InvoiceDate.HasValue
                    ? FinanceBusinessDate.ToBusinessTime(x.InvoiceDate.Value).ToString("yyyy-MM-dd")
                    : "NO_DATE")
                .ToList();

            foreach (var dg in dateItemsMap)
            {
                decimal dateSubtotal = 0m;
                string dateLabel = dg.Key == "NO_DATE" ? "Tanpa Tanggal" : dg.Key;

                foreach (var item in dg)
                {
                    dateSubtotal += item.BillingAmount;

                    ws.Cell(r, 1).Value = globalIdx++;
                    ws.Cell(r, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    var invDateStr = item.InvoiceDate.HasValue
                        ? FinanceBusinessDate.ToBusinessTime(item.InvoiceDate.Value).ToString("dd/MM/yyyy HH:mm")
                        : "-";
                    ws.Cell(r, 2).Value = invDateStr;
                    ws.Cell(r, 2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    var regDateStr = item.RegistrationDate.HasValue
                        ? FinanceBusinessDate.ToBusinessTime(item.RegistrationDate.Value).ToString("dd/MM/yyyy HH:mm")
                        : "-";
                    ws.Cell(r, 3).Value = regDateStr;
                    ws.Cell(r, 3).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    ws.Cell(r, 4).Value = item.BillingNumber;
                    ws.Cell(r, 5).Value = item.RegistrationNumber;
                    ws.Cell(r, 5).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    ws.Cell(r, 6).Value = item.MedicalRecordNumber;
                    ws.Cell(r, 6).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    ws.Cell(r, 7).Value = item.PatientName;
                    ws.Cell(r, 7).Style.Font.Bold = true;
                    ws.Cell(r, 8).Value = item.InvoiceNumber;

                    ws.Cell(r, 9).Value = item.BillingAmount;
                    ws.Cell(r, 9).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(r, 9).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

                    for (var col = 1; col <= 9; col++)
                    {
                        ws.Cell(r, col).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        ws.Cell(r, col).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#e2e8f0");
                    }
                    r++;

                    if (item.InvoiceDate.HasValue)
                    {
                        dateLabel = FinanceBusinessDate.ToBusinessTime(item.InvoiceDate.Value).ToString("dd MMMM yyyy");
                    }
                }

                // Date Subtotal Row
                var subtotalLabelRange = ws.Range(r, 1, r, 8);
                subtotalLabelRange.Merge();
                subtotalLabelRange.Value = $"Total {dateLabel} :";
                subtotalLabelRange.Style.Font.Bold = true;
                subtotalLabelRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                subtotalLabelRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#1e4620");
                subtotalLabelRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#e6f4ea");

                var subtotalValCell = ws.Cell(r, 9);
                subtotalValCell.Value = dateSubtotal;
                subtotalValCell.Style.Font.Bold = true;
                subtotalValCell.Style.NumberFormat.Format = "#,##0";
                subtotalValCell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                subtotalValCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#1e4620");
                subtotalValCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#e6f4ea");

                for (var col = 1; col <= 9; col++)
                {
                    ws.Cell(r, col).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    ws.Cell(r, col).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#bbf7d0");
                }
                r++;
            }

            // Company Total Row
            var compTotalLabelRange = ws.Range(r, 1, r, 8);
            compTotalLabelRange.Merge();
            compTotalLabelRange.Value = $"Total {group.CompanyName} :";
            compTotalLabelRange.Style.Font.Bold = true;
            compTotalLabelRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            compTotalLabelRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#0369a1");
            compTotalLabelRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#e0f2fe");

            var compTotalValCell = ws.Cell(r, 9);
            compTotalValCell.Value = group.PageSubtotalAmount;
            compTotalValCell.Style.Font.Bold = true;
            compTotalValCell.Style.NumberFormat.Format = "#,##0";
            compTotalValCell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            compTotalValCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#0369a1");
            compTotalValCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#e0f2fe");

            for (var col = 1; col <= 9; col++)
            {
                ws.Cell(r, col).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Cell(r, col).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#7dd3fc");
            }
            r++;
        }

        // Grand Total Row
        if (items.Count > 0)
        {
            var gtLabelRange = ws.Range(r, 1, r, 8);
            gtLabelRange.Merge();
            gtLabelRange.Value = "GRAND TOTAL :";
            gtLabelRange.Style.Font.Bold = true;
            gtLabelRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            gtLabelRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            gtLabelRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0f172a");

            var gtValCell = ws.Cell(r, 9);
            gtValCell.Value = summary.TotalBillingAmount;
            gtValCell.Style.Font.Bold = true;
            gtValCell.Style.NumberFormat.Format = "#,##0";
            gtValCell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            gtValCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            gtValCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0f172a");
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(5);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportReportArCreatedPdfAsync(
        ReportArCreatedQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = BuildReportArCreatedBaseQuery(request);
        var sortedQuery = ApplyReportArCreatedSort(baseQuery, request);

        var allRows = await sortedQuery.ToListAsync(cancellationToken);
        var items = allRows.Select(MapReportArCreatedItem).ToList();
        var summary = await GetReportArCreatedSummaryAsync(request, cancellationToken);

        return ReportArCreatedReportDocuments.BuildPdf(items, summary);
    }

    // ------------------------------------------------------------------------------------
    // REVAMP MANAJEMEN KLAIM PENJAMIN V2 (Read-Model, Filter Server-Side, Summary, Export)
    // ------------------------------------------------------------------------------------

    public async Task<ClaimManagementPagedResponse> GetClaimsPagedAsync(
        ClaimManagementBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking().Where(x => !x.IsDelete);
        query = ApplyClaimFilter(query, request);

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var sortBy = (string.IsNullOrWhiteSpace(request.SortBy) ? "createdatetime" : request.SortBy).Trim().ToLowerInvariant();
        query = sortBy switch
        {
            "batchnumber" or "claimnumber" => descending ? query.OrderByDescending(x => x.BatchNumber) : query.OrderBy(x => x.BatchNumber),
            "totalamount" => descending ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
            "approvedamount" => descending ? query.OrderByDescending(x => x.ApprovedAmount) : query.OrderBy(x => x.ApprovedAmount),
            "invoicedate" => descending ? query.OrderByDescending(x => x.InvoiceDate) : query.OrderBy(x => x.InvoiceDate),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "claimstatus" => descending ? query.OrderByDescending(x => x.ClaimStatus) : query.OrderBy(x => x.ClaimStatus),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(request.PageSize, 1, 30);
        var pageNumber = Math.Max(request.PageNumber, 1);

        var pagedBatches = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var batchIds = pagedBatches.Select(b => b.Id).ToList();

        // 1. Resolve Debtor Names
        var debtorIds = pagedBatches.Select(b => b.DebtorReferenceId).Distinct().ToList();
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

        // 2. Resolve Active Member Items and Member Receivables
        var batchItems = batchIds.Count > 0
            ? await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                .Where(i => batchIds.Contains(i.BatchId) && !i.IsDelete && i.IsActiveMembership)
                .ToListAsync(cancellationToken)
            : new List<FinReceivableInvoiceBatchItem>();

        var receivableIds = batchItems.Select(i => i.ReceivableId).Distinct().ToList();
        var memberReceivables = receivableIds.Count > 0
            ? await _dbContext.FinReceivables.AsNoTracking()
                .Where(r => receivableIds.Contains(r.Id) && !r.IsDelete)
                .ToDictionaryAsync(r => r.Id, cancellationToken)
            : new Dictionary<Guid, FinReceivable>();

        // 3. For single-member batches (memberCount == 1), resolve patient, encounter, billing, and diagnosis
        var singleBatches = pagedBatches
            .Where(b => batchItems.Count(i => i.BatchId == b.Id) == 1)
            .ToList();

        var singleReceivableIds = singleBatches
            .SelectMany(b => batchItems.Where(i => i.BatchId == b.Id).Select(i => i.ReceivableId))
            .Distinct()
            .ToList();

        var singleReceivableItems = singleReceivableIds.Count > 0
            ? await _dbContext.FinReceivableItems.AsNoTracking()
                .Where(i => singleReceivableIds.Contains(i.ReceivableId) && !i.IsDelete)
                .ToListAsync(cancellationToken)
            : new List<FinReceivableItem>();

        var singleInvoiceIds = singleReceivableIds
            .Select(id => memberReceivables.TryGetValue(id, out var r) ? r.InvoiceId : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Concat(singleReceivableItems.Where(i => i.InvoiceId.HasValue).Select(i => i.InvoiceId!.Value))
            .Distinct()
            .ToList();

        var singleInvoices = singleInvoiceIds.Count > 0
            ? await _dbContext.BilInvoices.AsNoTracking()
                .Where(inv => singleInvoiceIds.Contains(inv.Id) && !inv.IsDelete)
                .ToDictionaryAsync(inv => inv.Id, cancellationToken)
            : new Dictionary<Guid, BilInvoice>();

        var singleEncounterIds = singleInvoices.Values.Select(inv => inv.EncounterId)
            .Concat(singleReceivableItems.Where(i => i.EncounterId.HasValue).Select(i => i.EncounterId!.Value))
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var singleEncounters = singleEncounterIds.Count > 0
            ? await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(enc => singleEncounterIds.Contains(enc.Id) && !enc.IsDelete)
                .Select(enc => new ClaimSingleEncounterRow
                {
                    Id = enc.Id,
                    PatientId = enc.PatientId,
                    EncounterNumber = enc.EncounterNumber
                })
                .ToDictionaryAsync(enc => enc.Id, cancellationToken)
            : new Dictionary<Guid, ClaimSingleEncounterRow>();

        var singlePatientIds = singleEncounters.Values.Select(enc => enc.PatientId)
            .Concat(singleReceivableItems.Where(i => i.PatientId.HasValue).Select(i => i.PatientId!.Value))
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var singlePatients = singlePatientIds.Count > 0
            ? await _dbContext.MstPatients.AsNoTracking()
                .Where(p => singlePatientIds.Contains(p.Id) && !p.IsDelete)
                .ToDictionaryAsync(p => p.Id, cancellationToken)
            : new Dictionary<Guid, MstPatient>();

        var singleDiagnoses = singleEncounterIds.Count > 0
            ? await _dbContext.TrxPatientDiagnoses.AsNoTracking()
                .Where(d => singleEncounterIds.Contains(d.EncounterId) && !d.IsDelete)
                .OrderByDescending(d => d.IsPrimary)
                .ThenBy(d => d.SortOrder)
                .ToListAsync(cancellationToken)
            : new List<TrxPatientDiagnosis>();

        var items = pagedBatches.Select(b =>
        {
            string payerName = "-";
            if (companyNames.TryGetValue(b.DebtorReferenceId, out var cName)) payerName = cName;
            else if (insuranceNames.TryGetValue(b.DebtorReferenceId, out var iName)) payerName = iName;

            var bItems = batchItems.Where(i => i.BatchId == b.Id).ToList();
            var memberCount = bItems.Count;

            decimal paidAmount = 0m;
            decimal outstandingAmount = b.TotalAmount;
            if (memberCount > 0)
            {
                paidAmount = bItems.Sum(i => memberReceivables.TryGetValue(i.ReceivableId, out var r) ? r.AllocatedAmount : 0m);
                outstandingAmount = bItems.Sum(i => memberReceivables.TryGetValue(i.ReceivableId, out var r) ? r.OutstandingAmount : 0m);
            }

            var visitCode = ResolveBatchVisitCode(b) ?? "OP";
            var visitLabel = ResolveVisitLabel(visitCode);

            string? singlePatientName = null;
            string? singleMrn = null;
            string? singleBillingNumber = null;
            string? singleEncounterNumber = null;
            string? diagnosisSummary = null;

            if (memberCount == 1)
            {
                var rId = bItems[0].ReceivableId;
                memberReceivables.TryGetValue(rId, out var r);
                var itemsForRec = singleReceivableItems.Where(it => it.ReceivableId == rId).ToList();

                Guid? invId = r?.InvoiceId ?? itemsForRec.FirstOrDefault(it => it.InvoiceId.HasValue)?.InvoiceId;
                BilInvoice? inv = invId.HasValue && singleInvoices.TryGetValue(invId.Value, out var foundInv) ? foundInv : null;

                Guid? encId = inv?.EncounterId ?? itemsForRec.FirstOrDefault(it => it.EncounterId.HasValue)?.EncounterId;
                ClaimSingleEncounterRow? enc = encId.HasValue && singleEncounters.TryGetValue(encId.Value, out var foundEnc) ? foundEnc : null;

                Guid? patId = enc?.PatientId ?? itemsForRec.FirstOrDefault(it => it.PatientId.HasValue)?.PatientId;
                MstPatient? pat = patId.HasValue && singlePatients.TryGetValue(patId.Value, out var foundPat) ? foundPat : null;

                singlePatientName = pat?.FullName;
                singleMrn = pat?.MedicalRecordNumber;
                singleBillingNumber = inv?.InvoiceNumber;
                singleEncounterNumber = enc?.EncounterNumber;

                var diag = encId.HasValue ? singleDiagnoses.FirstOrDefault(d => d.EncounterId == encId.Value) : null;
                if (diag != null)
                {
                    diagnosisSummary = !string.IsNullOrWhiteSpace(diag.DiagnosisCode)
                        ? $"{diag.DiagnosisCode} - {diag.DiagnosisName}"
                        : diag.DiagnosisName;
                }
            }

            var effectiveClaim = ResolveEffectiveClaimStatus(b);

            return new ClaimManagementBatchItemResponse
            {
                Id = b.Id,
                BatchNumber = b.BatchNumber,
                PayerClaimReference = b.PayerClaimReference,
                DebtorReferenceId = b.DebtorReferenceId,
                PayerName = payerName,
                MemberCount = memberCount,
                SinglePatientName = singlePatientName,
                SingleMedicalRecordNumber = singleMrn,
                SingleBillingNumber = singleBillingNumber,
                SingleEncounterNumber = singleEncounterNumber,
                VisitCode = visitCode,
                VisitLabel = visitLabel ?? string.Empty,
                DiagnosisSummary = diagnosisSummary ?? (memberCount > 1 ? $"{memberCount} Pasien" : "-"),
                TotalAmount = b.TotalAmount,
                ApprovedAmount = b.ApprovedAmount,
                ClaimVarianceAmount = b.ApprovedAmount.HasValue ? b.TotalAmount - b.ApprovedAmount.Value : null,
                PaidAmount = paidAmount,
                OutstandingAmount = outstandingAmount,
                ClaimStatus = effectiveClaim,
                PaymentStatus = b.Status == FinReceivableInvoiceBatchStatuses.Cancelled
                    ? FinReceivableInvoiceBatchStatuses.Cancelled
                    : outstandingAmount <= 0 && b.TotalAmount > 0
                        ? FinReceivableInvoiceBatchStatuses.Paid
                        : paidAmount > 0 && outstandingAmount > 0
                            ? FinReceivableInvoiceBatchStatuses.PartiallyPaid
                            : b.Status == FinReceivableInvoiceBatchStatuses.Paid
                                ? FinReceivableInvoiceBatchStatuses.Paid
                                : b.Status == FinReceivableInvoiceBatchStatuses.PartiallyPaid
                                    ? FinReceivableInvoiceBatchStatuses.PartiallyPaid
                                    : "UNPAID",
                InvoiceDate = b.InvoiceDate,
                PeriodStart = b.PeriodStart,
                PeriodEnd = b.PeriodEnd,
                IssuedAt = b.IssuedAt,
                PayerVerifiedAt = b.PayerVerifiedAt,
                ClaimApprovedAt = b.ClaimApprovedAt,
                ClaimClosedAt = b.ClaimClosedAt,
                ClaimNote = b.ClaimNote,
                CancelReason = b.CancelReason,
                CancelDateTime = b.CancelDateTime,
                RowVersion = b.RowVersion
            };
        }).ToList();

        var summary = await GetClaimsSummaryAsync(request, cancellationToken);

        return new ClaimManagementPagedResponse
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items,
            Summary = summary
        };
    }

    public async Task<ClaimManagementSummaryResponse> GetClaimsSummaryAsync(
        ClaimManagementBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking().Where(x => !x.IsDelete);
        query = ApplyClaimFilter(query, request);

        var totalClaims = await query.CountAsync(cancellationToken);
        if (totalClaims == 0)
        {
            return new ClaimManagementSummaryResponse();
        }

        var pendingVerificationCount = await query.CountAsync(x =>
            x.ClaimStatus == "SUBMITTED" || (x.ClaimStatus == null && x.Status != "DRAFT" && x.Status != "CANCELLED"), cancellationToken);

        var pendingApprovalCount = await query.CountAsync(x => x.ClaimStatus == "PAYER_VERIFIED", cancellationToken);
        var approvedCount = await query.CountAsync(x => x.ClaimStatus == "APPROVED", cancellationToken);
        var closedCount = await query.CountAsync(x => x.ClaimStatus == "CLOSED", cancellationToken);

        var unpaidCount = await query.CountAsync(x =>
            x.Status == FinReceivableInvoiceBatchStatuses.Issued || x.Status == FinReceivableInvoiceBatchStatuses.Draft, cancellationToken);
        var partiallyPaidCount = await query.CountAsync(x => x.Status == FinReceivableInvoiceBatchStatuses.PartiallyPaid, cancellationToken);
        var paidCount = await query.CountAsync(x => x.Status == FinReceivableInvoiceBatchStatuses.Paid, cancellationToken);
        var today = FinanceBusinessDate.Today();
        var overdueCount = await query.CountAsync(x =>
            (x.Status == FinReceivableInvoiceBatchStatuses.Issued || x.Status == FinReceivableInvoiceBatchStatuses.PartiallyPaid)
            && x.DueDate < today, cancellationToken);

        var totalClaimAmount = await query.SumAsync(x => x.TotalAmount, cancellationToken);
        var totalApprovedAmount = await query.SumAsync(x => x.ApprovedAmount ?? 0m, cancellationToken);

        var batchIdsQuery = query.Select(x => x.Id);
        var receivableTotals = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
            .Where(i => batchIdsQuery.Contains(i.BatchId) && !i.IsDelete && i.IsActiveMembership)
            .Join(_dbContext.FinReceivables.AsNoTracking().Where(r => !r.IsDelete),
                i => i.ReceivableId,
                r => r.Id,
                (i, r) => new { r.AllocatedAmount, r.OutstandingAmount })
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalPaid = g.Sum(x => x.AllocatedAmount),
                TotalOutstanding = g.Sum(x => x.OutstandingAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new ClaimManagementSummaryResponse
        {
            TotalClaims = totalClaims,
            TotalInvoices = totalClaims,
            UnpaidCount = unpaidCount,
            PartiallyPaidCount = partiallyPaidCount,
            PaidCount = paidCount,
            OverdueCount = overdueCount,
            PendingVerificationCount = pendingVerificationCount,
            PendingApprovalCount = pendingApprovalCount,
            ApprovedCount = approvedCount,
            ClosedCount = closedCount,
            TotalClaimAmount = totalClaimAmount,
            TotalApprovedAmount = totalApprovedAmount,
            TotalPaidAmount = receivableTotals?.TotalPaid ?? 0m,
            TotalOutstandingAmount = receivableTotals?.TotalOutstanding ?? totalClaimAmount
        };
    }

    public async Task<byte[]> ExportClaimsExcelAsync(
        ClaimManagementBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking().Where(x => !x.IsDelete);
        query = ApplyClaimFilter(query, request).OrderByDescending(x => x.CreateDateTime);
        var batches = await query.Take(1000).ToListAsync(cancellationToken);
        var batchIds = batches.Select(b => b.Id).ToList();

        var debtorIds = batches.Select(b => b.DebtorReferenceId).Distinct().ToList();
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

        var batchItems = batchIds.Count > 0
            ? await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                .Where(i => batchIds.Contains(i.BatchId) && !i.IsDelete && i.IsActiveMembership)
                .ToListAsync(cancellationToken)
            : new List<FinReceivableInvoiceBatchItem>();

        var receivableIds = batchItems.Select(i => i.ReceivableId).Distinct().ToList();
        var memberReceivables = receivableIds.Count > 0
            ? await _dbContext.FinReceivables.AsNoTracking()
                .Where(r => receivableIds.Contains(r.Id) && !r.IsDelete)
                .ToDictionaryAsync(r => r.Id, cancellationToken)
            : new Dictionary<Guid, FinReceivable>();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Monitoring Tagihan");

        string[] headers =
        [
            "No", "No. Invoice AR", "Ref. Penjamin", "Penjamin", "Pasien / Jumlah",
            "Jenis Kunjungan", "Tanggal Invoice", "Jatuh Tempo", "Total Tagihan",
            "Terbayar", "Sisa Piutang", "Status Pembayaran"
        ];
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        var headerRange = ws.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0d7f89");
        headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

        var r = 2;
        foreach (var b in batches)
        {
            string payerName = "-";
            if (companyNames.TryGetValue(b.DebtorReferenceId, out var cName)) payerName = cName;
            else if (insuranceNames.TryGetValue(b.DebtorReferenceId, out var iName)) payerName = iName;

            var bItems = batchItems.Where(i => i.BatchId == b.Id).ToList();
            var memberCount = bItems.Count;
            decimal paidAmount = memberCount > 0
                ? bItems.Sum(i => memberReceivables.TryGetValue(i.ReceivableId, out var rec) ? rec.AllocatedAmount : 0m)
                : 0m;
            decimal outstandingAmount = memberCount > 0
                ? bItems.Sum(i => memberReceivables.TryGetValue(i.ReceivableId, out var rec) ? rec.OutstandingAmount : 0m)
                : b.TotalAmount;

            var visitCode = ResolveBatchVisitCode(b) ?? "OP";
            var visitLabel = ResolveVisitLabel(visitCode);

            ws.Cell(r, 1).Value = r - 1;
            ws.Cell(r, 2).Value = b.BatchNumber;
            ws.Cell(r, 3).Value = b.PayerClaimReference ?? "-";
            ws.Cell(r, 4).Value = payerName;
            ws.Cell(r, 5).Value = memberCount > 1 ? $"{memberCount} Pasien" : "1 Pasien";
            ws.Cell(r, 6).Value = visitLabel;
            ws.Cell(r, 7).Value = b.InvoiceDate.HasValue ? b.InvoiceDate.Value.ToString("dd/MM/yyyy") : "-";
            ws.Cell(r, 8).Value = b.DueDate.HasValue ? b.DueDate.Value.ToString("dd/MM/yyyy") : "-";
            ws.Cell(r, 9).Value = b.TotalAmount;
            ws.Cell(r, 10).Value = paidAmount;
            ws.Cell(r, 11).Value = outstandingAmount;
            ws.Cell(r, 12).Value = b.Status;

            for (var col = 1; col <= headers.Length; col++)
            {
                ws.Cell(r, col).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Cell(r, col).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#e2e8f0");
            }

            r++;
        }

        if (r > 2)
        {
            ws.Range(2, 9, r - 1, 11).Style.NumberFormat.Format = "#,##0";
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportClaimsPdfAsync(
        ClaimManagementBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking().Where(x => !x.IsDelete);
        query = ApplyClaimFilter(query, request).OrderByDescending(x => x.CreateDateTime);
        var batches = await query.Take(500).ToListAsync(cancellationToken);
        var batchIds = batches.Select(b => b.Id).ToList();

        var debtorIds = batches.Select(b => b.DebtorReferenceId).Distinct().ToList();
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

        var batchItems = batchIds.Count > 0
            ? await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                .Where(i => batchIds.Contains(i.BatchId) && !i.IsDelete && i.IsActiveMembership)
                .ToListAsync(cancellationToken)
            : new List<FinReceivableInvoiceBatchItem>();

        var receivableIds = batchItems.Select(i => i.ReceivableId).Distinct().ToList();
        var memberReceivables = receivableIds.Count > 0
            ? await _dbContext.FinReceivables.AsNoTracking()
                .Where(r => receivableIds.Contains(r.Id) && !r.IsDelete)
                .ToDictionaryAsync(r => r.Id, cancellationToken)
            : new Dictionary<Guid, FinReceivable>();

        var summary = await GetClaimsSummaryAsync(request, cancellationToken);

        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("RUMAH SAKIT METROPOLITAN MEDICAL CENTRE").Bold().FontSize(12).FontColor("#0d7f89");
                            c.Item().Text("LAPORAN MONITORING TAGIHAN PENJAMIN (AR)").Bold().FontSize(10).FontColor("#1f2937");
                            c.Item().Text($"Dicetak pada: {DateTime.Now:dd/MM/yyyy HH:mm} WIB").FontSize(7.5f).FontColor("#6b7280");
                        });
                    });
                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor("#0d7f89");
                });

                page.Content().PaddingVertical(5).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25);  // No
                            columns.RelativeColumn(2);   // No. Invoice AR
                            columns.RelativeColumn(2.5f);// Penjamin
                            columns.RelativeColumn(1.2f);// Pasien
                            columns.RelativeColumn(1f);  // Jenis
                            columns.RelativeColumn(1.2f);// Tgl Invoice
                            columns.RelativeColumn(1.2f);// Jatuh Tempo
                            columns.RelativeColumn(1.5f);// Total Tagihan
                            columns.RelativeColumn(1.5f);// Terbayar
                            columns.RelativeColumn(1.5f);// Sisa Piutang
                            columns.RelativeColumn(1.2f);// Status Bayar
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#0d7f89").Padding(3).AlignCenter().Text("NO").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).Text("NO. INVOICE AR").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).Text("PENJAMIN").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).AlignCenter().Text("PASIEN").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).AlignCenter().Text("JENIS").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).AlignCenter().Text("TGL INV").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).AlignCenter().Text("JATUH TEMPO").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("TOTAL TAGIHAN").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("TERBAYAR").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("SISA PIUTANG").Bold().FontColor("#ffffff");
                            header.Cell().Background("#0d7f89").Padding(3).AlignCenter().Text("STATUS").Bold().FontColor("#ffffff");
                        });

                        var idx = 1;
                        foreach (var b in batches)
                        {
                            string payerName = "-";
                            if (companyNames.TryGetValue(b.DebtorReferenceId, out var cName)) payerName = cName;
                            else if (insuranceNames.TryGetValue(b.DebtorReferenceId, out var iName)) payerName = iName;

                            var bItems = batchItems.Where(i => i.BatchId == b.Id).ToList();
                            var memberCount = bItems.Count;
                            decimal paidAmount = memberCount > 0
                                ? bItems.Sum(i => memberReceivables.TryGetValue(i.ReceivableId, out var rec) ? rec.AllocatedAmount : 0m)
                                : 0m;
                            decimal outstandingAmount = memberCount > 0
                                ? bItems.Sum(i => memberReceivables.TryGetValue(i.ReceivableId, out var rec) ? rec.OutstandingAmount : 0m)
                                : b.TotalAmount;

                            var visitCode = ResolveBatchVisitCode(b) ?? "OP";
                            var visitLabel = ResolveVisitLabel(visitCode);

                            var isEven = idx % 2 == 0;
                            var bg = isEven ? "#f9fafb" : "#ffffff";

                            table.Cell().Background(bg).Padding(2).AlignCenter().Text(idx.ToString());
                            table.Cell().Background(bg).Padding(2).Text(b.BatchNumber).Bold();
                            table.Cell().Background(bg).Padding(2).Text(payerName);
                            table.Cell().Background(bg).Padding(2).AlignCenter().Text(memberCount > 1 ? $"{memberCount} Pasien" : "1 Pasien");
                            table.Cell().Background(bg).Padding(2).AlignCenter().Text(visitLabel);
                            table.Cell().Background(bg).Padding(2).AlignCenter().Text(b.InvoiceDate.HasValue ? b.InvoiceDate.Value.ToString("dd/MM/yyyy") : "-");
                            table.Cell().Background(bg).Padding(2).AlignCenter().Text(b.DueDate.HasValue ? b.DueDate.Value.ToString("dd/MM/yyyy") : "-");
                            table.Cell().Background(bg).Padding(2).AlignRight().Text(b.TotalAmount.ToString("N0", IdCulture));
                            table.Cell().Background(bg).Padding(2).AlignRight().Text(paidAmount.ToString("N0", IdCulture));
                            table.Cell().Background(bg).Padding(2).AlignRight().Text(outstandingAmount.ToString("N0", IdCulture));
                            table.Cell().Background(bg).Padding(2).AlignCenter().Text(b.Status);

                            idx++;
                        }

                        // Summary Footer row
                        table.Cell().ColumnSpan(7).Background("#e0f2fe").Padding(3).AlignRight().Text("TOTAL TAGIHAN PENJAMIN :").Bold().FontColor("#0369a1");
                        table.Cell().Background("#e0f2fe").Padding(3).AlignRight().Text(summary.TotalClaimAmount.ToString("N0", IdCulture)).Bold().FontColor("#0369a1");
                        table.Cell().Background("#e0f2fe").Padding(3).AlignRight().Text(summary.TotalPaidAmount.ToString("N0", IdCulture)).Bold().FontColor("#0369a1");
                        table.Cell().Background("#e0f2fe").Padding(3).AlignRight().Text(summary.TotalOutstandingAmount.ToString("N0", IdCulture)).Bold().FontColor("#0369a1");
                        table.Cell().Background("#e0f2fe").Padding(3).Text("");
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Halaman ");
                    x.CurrentPageNumber();
                    x.Span(" dari ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private IQueryable<FinReceivableInvoiceBatch> ApplyClaimFilter(
        IQueryable<FinReceivableInvoiceBatch> query,
        ClaimManagementBatchQuery request)
    {
        var debtorId = request.PayerId ?? request.DebtorReferenceId;
        if (debtorId.HasValue && debtorId.Value != Guid.Empty)
            query = query.Where(x => x.DebtorReferenceId == debtorId.Value);

        if (!string.IsNullOrWhiteSpace(request.VisitCode) && !request.VisitCode.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var vc = request.VisitCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.VisitCode == vc || (x.VisitCode == null && EF.Functions.ILike(x.BatchNumber, $"%/{vc}/%")));
        }

        DateOnly? start = request.StartDate;
        DateOnly? end = request.EndDate;
        if (!string.IsNullOrWhiteSpace(request.PeriodPreset))
        {
            var today = FinanceBusinessDate.Today();
            var preset = request.PeriodPreset.Trim().ToUpperInvariant();
            if (preset == "TODAY") { start = today; end = today; }
            else if (preset == "THIS_WEEK")
            {
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                start = today.AddDays(-diff);
                end = start.Value.AddDays(6);
            }
            else if (preset == "THIS_MONTH")
            {
                start = new DateOnly(today.Year, today.Month, 1);
                end = start.Value.AddMonths(1).AddDays(-1);
            }
            else if (preset == "LAST_MONTH")
            {
                var firstThisMonth = new DateOnly(today.Year, today.Month, 1);
                end = firstThisMonth.AddDays(-1);
                start = new DateOnly(end.Value.Year, end.Value.Month, 1);
            }
            else if (preset == "THIS_YEAR")
            {
                start = new DateOnly(today.Year, 1, 1);
                end = new DateOnly(today.Year, 12, 31);
            }
        }

        if (start.HasValue)
            query = query.Where(x => (x.InvoiceDate ?? DateOnly.FromDateTime(x.CreateDateTime)) >= start.Value);

        if (end.HasValue)
            query = query.Where(x => (x.InvoiceDate ?? DateOnly.FromDateTime(x.CreateDateTime)) <= end.Value);

        if (!string.IsNullOrWhiteSpace(request.ClaimStatus) && !request.ClaimStatus.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var cs = request.ClaimStatus.Trim().ToUpperInvariant();
            if (cs == "SUBMITTED")
            {
                query = query.Where(x => x.ClaimStatus == "SUBMITTED" || (x.ClaimStatus == null && x.Status != "DRAFT" && x.Status != "CANCELLED"));
            }
            else if (cs == "PAYER_VERIFIED")
            {
                query = query.Where(x => x.ClaimStatus == "PAYER_VERIFIED");
            }
            else if (cs == "APPROVED")
            {
                query = query.Where(x => x.ClaimStatus == "APPROVED");
            }
            else if (cs == "CLOSED")
            {
                query = query.Where(x => x.ClaimStatus == "CLOSED");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) && !request.PaymentStatus.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var ps = request.PaymentStatus.Trim().ToUpperInvariant();
            query = query.Where(x => x.Status == ps);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.BatchNumber, pattern)
                || (x.PayerClaimReference != null && EF.Functions.ILike(x.PayerClaimReference, pattern))
                || (x.Note != null && EF.Functions.ILike(x.Note, pattern))
                || _dbContext.MstCompanyGuarantors.Any(c => !c.IsDelete && c.Id == x.DebtorReferenceId && EF.Functions.ILike(c.CompanyGuarantorName, pattern))
                || _dbContext.MstInsuranceProviders.Any(p => !p.IsDelete && p.Id == x.DebtorReferenceId && EF.Functions.ILike(p.InsuranceProviderName, pattern))
                || x.Items.Any(i => !i.IsDelete && i.Receivable != null && (
                    EF.Functions.ILike(i.Receivable.ReceivableNumber, pattern)
                    || _dbContext.BilInvoices.Any(inv => !inv.IsDelete && inv.Id == i.Receivable.InvoiceId && (
                        EF.Functions.ILike(inv.InvoiceNumber, pattern)
                        || _dbContext.RegPatientEncounters.Any(enc => !enc.IsDelete && enc.Id == inv.EncounterId && (
                            EF.Functions.ILike(enc.EncounterNumber, pattern)
                            || _dbContext.MstPatients.Any(pat => !pat.IsDelete && pat.Id == enc.PatientId && (
                                EF.Functions.ILike(pat.FullName, pattern) || EF.Functions.ILike(pat.MedicalRecordNumber, pattern)
                            ))
                        ))
                    ))
                ))
            );
        }

        return query;
    }
}

public sealed class ReceivableInvoiceBatchBadRequestException(string message) : Exception(message);
public sealed class ReceivableInvoiceBatchValidationException(string message) : Exception(message);
public sealed class ReceivableInvoiceBatchConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
