using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.DTOs;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Layanan Pengajuan Pemutihan Tagihan AR V2 (Header dan Detail).
/// Mengintegrasikan Invoice AR V2 (FinReceivableInvoiceBatch -&gt; FinReceivableInvoiceBatchItem -&gt; FinReceivable),
/// maker-checker approval, mutasi saldo piutang canonical, subledger movement, dan Accounting Outbox.
/// </summary>
public sealed class FinanceReceivableWriteOffProposalService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.WriteOffProposal";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceSubledgerMovementService _subledgerMovementService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;

    public FinanceReceivableWriteOffProposalService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceSubledgerMovementService subledgerMovementService,
        FinanceAccountingOutboxService accountingOutboxService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _subledgerMovementService = subledgerMovementService;
        _accountingOutboxService = accountingOutboxService;
    }

    // ------------------------------------------------------------------------------------
    // Master Alasan & Metadata
    // ------------------------------------------------------------------------------------

    public List<EligibleWriteOffReasonResponse> GetReasonCodes()
    {
        return FinReceivableWriteOffReasonCodes.DisplayLabels
            .Select(kv => new EligibleWriteOffReasonResponse { Code = kv.Key, Label = kv.Value })
            .ToList();
    }

    // ------------------------------------------------------------------------------------
    // Invoice AR Eligible — GET /eligible-invoices
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<EligibleWriteOffInvoiceResponse>> GetEligibleInvoicesAsync(
        EligibleWriteOffInvoiceQuery request, CancellationToken cancellationToken)
    {
        // 1. Ambil ID piutang yang sedang berada dalam proposal aktif (SUBMITTED / APPROVED)
        var pendingReceivableIds = await _dbContext.FinReceivableWriteOffProposalItems.AsNoTracking()
            .Where(pi => !pi.IsDelete
                && (pi.Proposal!.Status == FinReceivableWriteOffProposalStatuses.Submitted
                    || pi.Proposal.Status == FinReceivableWriteOffProposalStatuses.Approved)
                && !pi.Proposal.IsDelete
                && !pi.Proposal.IsCancel)
            .Select(pi => pi.ReceivableId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var pendingReceivableSet = pendingReceivableIds.ToHashSet();

        // 2. Query Invoice AR (FinReceivableInvoiceBatch) yang aktif
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
            .Where(b => !b.IsDelete && !b.IsCancel
                && (b.Status == FinReceivableInvoiceBatchStatuses.Issued || b.Status == FinReceivableInvoiceBatchStatuses.PartiallyPaid)
                && b.ReissuedToBatchId == null);

        if (request.DebtorReferenceId.HasValue && request.DebtorReferenceId.Value != Guid.Empty)
            query = query.Where(b => b.DebtorReferenceId == request.DebtorReferenceId.Value);

        if (!string.IsNullOrWhiteSpace(request.ServiceType) && !request.ServiceType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var st = request.ServiceType.Trim().ToUpperInvariant();
            query = query.Where(b => b.ServiceType == st || (b.ServiceType == null && b.VisitCode == st));
        }

        if (request.StartDate.HasValue)
            query = query.Where(b => (b.InvoiceDate ?? DateOnly.FromDateTime(b.CreateDateTime)) >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(b => (b.InvoiceDate ?? DateOnly.FromDateTime(b.CreateDateTime)) <= request.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(b =>
                EF.Functions.ILike(b.BatchNumber, pattern)
                || (b.Note != null && EF.Functions.ILike(b.Note, pattern))
                || _dbContext.MstCompanyGuarantors.Any(c => !c.IsDelete && c.Id == b.DebtorReferenceId && EF.Functions.ILike(c.CompanyGuarantorName, pattern))
                || _dbContext.MstInsuranceProviders.Any(p => !p.IsDelete && p.Id == b.DebtorReferenceId && EF.Functions.ILike(p.InsuranceProviderName, pattern)));
        }

        query = query.OrderByDescending(b => b.InvoiceDate ?? DateOnly.FromDateTime(b.CreateDateTime));

        var allBatches = await query.ToListAsync(cancellationToken);
        if (allBatches.Count == 0)
        {
            return new PagedResult<EligibleWriteOffInvoiceResponse>
            {
                PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = 0, TotalPage = 0, Items = new()
            };
        }

        var batchIds = allBatches.Select(b => b.Id).ToList();

        // 3. Ambil data batch items dan piutang anggota
        var batchItems = await _dbContext.FinReceivableInvoiceBatchItems
            .Include(i => i.Receivable)
            .AsNoTracking()
            .Where(i => batchIds.Contains(i.BatchId) && !i.IsDelete && i.IsActiveMembership && i.Receivable != null && !i.Receivable.IsDelete)
            .ToListAsync(cancellationToken);

        // Filter batch yang masih memiliki piutang dengan sisa saldo (> 0) dan tidak pending di proposal lain
        var validBatchItems = batchItems
            .Where(i => i.Receivable!.OutstandingAmount > 0 && !pendingReceivableSet.Contains(i.ReceivableId))
            .ToList();

        var validBatchIdSet = validBatchItems.Select(i => i.BatchId).ToHashSet();
        var eligibleBatches = allBatches.Where(b => validBatchIdSet.Contains(b.Id)).ToList();

        var total = eligibleBatches.Count;
        var pagedBatches = eligibleBatches
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var pagedBatchIds = pagedBatches.Select(b => b.Id).ToHashSet();
        var pagedBatchItems = validBatchItems.Where(i => pagedBatchIds.Contains(i.BatchId)).ToList();
        var pagedReceivables = pagedBatchItems.Select(i => i.Receivable!).ToList();
        var pagedReceivableIds = pagedReceivables.Select(r => r.Id).ToList();

        // 4. Resolve konteks penjamin, billing, encounter, pasien, dokter
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

        var receivableItems = pagedReceivableIds.Count > 0
            ? await _dbContext.FinReceivableItems.AsNoTracking()
                .Where(i => pagedReceivableIds.Contains(i.ReceivableId) && !i.IsDelete)
                .ToListAsync(cancellationToken)
            : new List<FinReceivableItem>();

        var invoiceIds = pagedReceivables.Where(r => r.InvoiceId.HasValue).Select(r => r.InvoiceId!.Value)
            .Concat(receivableItems.Where(i => i.InvoiceId.HasValue).Select(i => i.InvoiceId!.Value))
            .Distinct()
            .ToList();

        var billingInvoices = invoiceIds.Count > 0
            ? await _dbContext.BilInvoices.AsNoTracking()
                .Where(inv => invoiceIds.Contains(inv.Id) && !inv.IsDelete)
                .ToDictionaryAsync(inv => inv.Id, cancellationToken)
            : new Dictionary<Guid, BilInvoice>();

        var encounterIds = billingInvoices.Values.Select(inv => inv.EncounterId)
            .Concat(receivableItems.Where(i => i.EncounterId.HasValue).Select(i => i.EncounterId!.Value))
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var encounters = encounterIds.Count > 0
            ? await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(e => encounterIds.Contains(e.Id) && !e.IsDelete)
                .Select(e => new
                {
                    e.Id,
                    e.EncounterNumber,
                    e.EncounterType,
                    e.EncounterDate,
                    e.PatientId,
                    e.DoctorId,
                    e.PatientClassId
                })
                .ToDictionaryAsync(e => e.Id, cancellationToken)
            : new();

        var patientIds = encounters.Values.Select(e => e.PatientId)
            .Concat(receivableItems.Where(i => i.PatientId.HasValue).Select(i => i.PatientId!.Value))
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var patients = patientIds.Count > 0
            ? await _dbContext.MstPatients.AsNoTracking()
                .Where(p => patientIds.Contains(p.Id) && !p.IsDelete)
                .ToDictionaryAsync(p => p.Id, cancellationToken)
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

        // 5. Build response list
        var items = pagedBatches.Select(b =>
        {
            var bItems = pagedBatchItems.Where(i => i.BatchId == b.Id).ToList();
            string debtorName = companyNames.TryGetValue(b.DebtorReferenceId, out var cName)
                ? cName
                : (insuranceNames.TryGetValue(b.DebtorReferenceId, out var iName) ? iName : "-");

            var memberResponses = bItems.Select(bi =>
            {
                var r = bi.Receivable!;
                var itemsForRec = receivableItems.Where(it => it.ReceivableId == r.Id).ToList();
                Guid? invId = r.InvoiceId ?? itemsForRec.FirstOrDefault(it => it.InvoiceId.HasValue)?.InvoiceId;
                BilInvoice? inv = invId.HasValue && billingInvoices.TryGetValue(invId.Value, out var foundInv) ? foundInv : null;

                Guid? encId = inv?.EncounterId ?? itemsForRec.FirstOrDefault(it => it.EncounterId.HasValue)?.EncounterId;
                var enc = encId.HasValue && encounters.TryGetValue(encId.Value, out var foundEnc) ? foundEnc : null;

                Guid? patId = enc?.PatientId ?? itemsForRec.FirstOrDefault(it => it.PatientId.HasValue)?.PatientId;
                MstPatient? pat = patId.HasValue && patients.TryGetValue(patId.Value, out var foundPat) ? foundPat : null;

                var billingDate = inv?.InvoiceDate.HasValue == true
                    ? DateOnly.FromDateTime(inv.InvoiceDate.Value.LocalDateTime)
                    : r.DueDate.AddDays(-30);

                string? docName = enc?.DoctorId.HasValue == true && doctors.TryGetValue(enc.DoctorId.Value, out var d) ? d : null;
                string? className = enc?.PatientClassId.HasValue == true && patientClasses.TryGetValue(enc.PatientClassId.Value, out var cl) ? cl : null;

                var serviceType = enc != null
                    ? (enc.EncounterType == EncounterType.Inpatient ? "RANAP" : enc.EncounterType == EncounterType.Emergency ? "IGD" : "RAJAL")
                    : (b.ServiceType ?? (b.VisitCode == "IP" ? "RANAP" : "RAJAL"));

                return new EligibleWriteOffInvoiceMemberResponse
                {
                    ReceivableId = r.Id,
                    ReceivableNumber = r.ReceivableNumber,
                    OriginalAmount = r.OriginalAmount,
                    AllocatedAmount = r.AllocatedAmount,
                    WrittenOffAmount = r.WrittenOffAmount,
                    OutstandingAmount = r.OutstandingAmount,
                    Status = r.Status,
                    InvoiceId = invId,
                    BillingNumber = inv?.InvoiceNumber ?? "-",
                    BillingDate = billingDate,
                    EncounterNumber = enc?.EncounterNumber ?? "-",
                    PatientName = pat?.FullName ?? "-",
                    MedicalRecordNumber = pat?.MedicalRecordNumber ?? "-",
                    DoctorName = docName ?? "-",
                    PatientClassName = className ?? "-",
                    ServiceType = serviceType,
                    ServiceDate = enc != null ? DateOnly.FromDateTime(enc.EncounterDate) : billingDate
                };
            }).ToList();

            var totalOutstanding = memberResponses.Sum(m => m.OutstandingAmount);
            var totalAllocated = memberResponses.Sum(m => m.AllocatedAmount);

            return new EligibleWriteOffInvoiceResponse
            {
                Id = b.Id,
                BatchNumber = b.BatchNumber,
                InvoiceNumber = b.BatchNumber,
                DebtorReferenceId = b.DebtorReferenceId,
                DebtorName = debtorName,
                InvoiceDate = b.InvoiceDate,
                DueDate = b.DueDate,
                ServiceType = b.ServiceType ?? (b.VisitCode == "IP" ? "RANAP" : "RAJAL"),
                VisitCode = b.VisitCode,
                TotalAmount = b.TotalAmount,
                AllocatedAmount = totalAllocated,
                OutstandingAmount = totalOutstanding,
                Status = b.Status,
                MemberCount = memberResponses.Count,
                Members = memberResponses
            };
        }).ToList();

        return new PagedResult<EligibleWriteOffInvoiceResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    // ------------------------------------------------------------------------------------
    // Daftar Proposal — GET /write-off-proposals
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<ReceivableWriteOffProposalResponse>> GetPagedAsync(
        ReceivableWriteOffProposalQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableWriteOffProposals.AsNoTracking().Where(p => !p.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(p => p.Status == request.Status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(request.ProposalNumber))
            query = query.Where(p => EF.Functions.ILike(p.ProposalNumber, $"%{request.ProposalNumber.Trim()}%"));

        if (!string.IsNullOrWhiteSpace(request.ServiceType) && !request.ServiceType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            query = query.Where(p => p.ServiceType == request.ServiceType.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(request.ReasonCode))
            query = query.Where(p => p.ReasonCode == request.ReasonCode.Trim().ToUpperInvariant());

        if (request.DebtorReferenceId.HasValue && request.DebtorReferenceId.Value != Guid.Empty)
            query = query.Where(p => p.DebtorReferenceId == request.DebtorReferenceId.Value);

        if (request.StartDate.HasValue)
            query = query.Where(p => p.ProposalDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(p => p.ProposalDate <= request.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
        {
            var invPattern = $"%{request.InvoiceNumber.Trim()}%";
            query = query.Where(p => p.Items.Any(i => !i.IsDelete && EF.Functions.ILike(i.InvoiceBatch!.BatchNumber, invPattern)));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.ProposalNumber, pattern)
                || EF.Functions.ILike(p.ReasonDescription, pattern)
                || (p.DebtorReferenceId.HasValue && (
                    _dbContext.MstCompanyGuarantors.Any(c => !c.IsDelete && c.Id == p.DebtorReferenceId.Value && EF.Functions.ILike(c.CompanyGuarantorName, pattern))
                    || _dbContext.MstInsuranceProviders.Any(i => !i.IsDelete && i.Id == p.DebtorReferenceId.Value && EF.Functions.ILike(i.InsuranceProviderName, pattern))
                ))
                || p.Items.Any(i => !i.IsDelete && (
                    EF.Functions.ILike(i.InvoiceBatch!.BatchNumber, pattern)
                    || EF.Functions.ILike(i.Receivable!.ReceivableNumber, pattern)
                )));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "proposalnumber" => descending ? query.OrderByDescending(p => p.ProposalNumber) : query.OrderBy(p => p.ProposalNumber),
            "proposaldate" => descending ? query.OrderByDescending(p => p.ProposalDate) : query.OrderBy(p => p.ProposalDate),
            "totalproposedamount" => descending ? query.OrderByDescending(p => p.TotalProposedAmount) : query.OrderBy(p => p.TotalProposedAmount),
            "totalapprovedamount" => descending ? query.OrderByDescending(p => p.TotalApprovedAmount) : query.OrderBy(p => p.TotalApprovedAmount),
            "status" => descending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            _ => descending ? query.OrderByDescending(p => p.CreateDateTime) : query.OrderBy(p => p.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var pagedList = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                Proposal = p,
                InvoiceCount = p.Items.Where(i => !i.IsDelete).Select(i => i.InvoiceBatchId).Distinct().Count(),
                ItemCount = p.Items.Count(i => !i.IsDelete)
            })
            .ToListAsync(cancellationToken);

        var debtorIds = pagedList.Where(x => x.Proposal.DebtorReferenceId.HasValue)
            .Select(x => x.Proposal.DebtorReferenceId!.Value).Distinct().ToList();

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

        var userIds = pagedList.SelectMany(x => new[]
        {
            x.Proposal.RequestedBy,
            x.Proposal.ApprovedBy ?? Guid.Empty,
            x.Proposal.PostedBy ?? Guid.Empty,
            x.Proposal.RejectedBy ?? Guid.Empty
        }).Where(id => id != Guid.Empty).Distinct().ToList();

        var userNames = userIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? "-", cancellationToken)
            : new Dictionary<Guid, string>();

        var items = pagedList.Select(x =>
        {
            var p = x.Proposal;
            string? debtorName = null;
            if (p.DebtorReferenceId.HasValue)
            {
                if (companyNames.TryGetValue(p.DebtorReferenceId.Value, out var cName)) debtorName = cName;
                else if (insuranceNames.TryGetValue(p.DebtorReferenceId.Value, out var iName)) debtorName = iName;
            }

            return new ReceivableWriteOffProposalResponse
            {
                Id = p.Id,
                ProposalNumber = p.ProposalNumber,
                ProposalDate = p.ProposalDate,
                ProposalType = p.ProposalType,
                ServiceType = p.ServiceType,
                ServiceLabel = ResolveServiceLabel(p.ServiceType),
                DebtorReferenceId = p.DebtorReferenceId,
                DebtorName = debtorName,
                ReasonCode = p.ReasonCode,
                ReasonLabel = FinReceivableWriteOffReasonCodes.DisplayLabels.GetValueOrDefault(p.ReasonCode, p.ReasonCode),
                ReasonDescription = p.ReasonDescription,
                Status = p.Status,
                TotalInvoiceAmount = p.TotalInvoiceAmount,
                TotalOutstandingAmount = p.TotalOutstandingAmount,
                TotalProposedAmount = p.TotalProposedAmount,
                TotalApprovedAmount = p.TotalApprovedAmount,
                InvoiceCount = x.InvoiceCount,
                ItemCount = x.ItemCount,
                RequestedBy = p.RequestedBy,
                RequestedByName = userNames.GetValueOrDefault(p.RequestedBy, "-"),
                RequestedAt = p.RequestedAt,
                ApprovedBy = p.ApprovedBy,
                ApprovedByName = p.ApprovedBy.HasValue ? userNames.GetValueOrDefault(p.ApprovedBy.Value, "-") : null,
                ApprovedAt = p.ApprovedAt,
                PostedBy = p.PostedBy,
                PostedByName = p.PostedBy.HasValue ? userNames.GetValueOrDefault(p.PostedBy.Value, "-") : null,
                PostedAt = p.PostedAt,
                RejectedBy = p.RejectedBy,
                RejectedByName = p.RejectedBy.HasValue ? userNames.GetValueOrDefault(p.RejectedBy.Value, "-") : null,
                RejectedAt = p.RejectedAt,
                RejectionReason = p.RejectionReason,
                RowVersion = p.RowVersion
            };
        }).ToList();

        return new PagedResult<ReceivableWriteOffProposalResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    // ------------------------------------------------------------------------------------
    // Ringkasan Proposal — GET /write-off-proposals/summary
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableWriteOffProposalSummaryResponse> GetSummaryAsync(
        ReceivableWriteOffProposalQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableWriteOffProposals.AsNoTracking().Where(p => !p.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.ServiceType) && !request.ServiceType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            query = query.Where(p => p.ServiceType == request.ServiceType.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(request.ReasonCode))
            query = query.Where(p => p.ReasonCode == request.ReasonCode.Trim().ToUpperInvariant());

        if (request.DebtorReferenceId.HasValue && request.DebtorReferenceId.Value != Guid.Empty)
            query = query.Where(p => p.DebtorReferenceId == request.DebtorReferenceId.Value);

        if (request.StartDate.HasValue)
            query = query.Where(p => p.ProposalDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(p => p.ProposalDate <= request.EndDate.Value);

        var counts = await query
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), TotalProposed = g.Sum(x => x.TotalProposedAmount), TotalApproved = g.Sum(x => x.TotalApprovedAmount) })
            .ToListAsync(cancellationToken);

        int totalProposals = counts.Sum(c => c.Count);
        int totalDraft = counts.FirstOrDefault(c => c.Status == FinReceivableWriteOffProposalStatuses.Draft)?.Count ?? 0;
        int totalSubmitted = counts.FirstOrDefault(c => c.Status == FinReceivableWriteOffProposalStatuses.Submitted)?.Count ?? 0;
        int totalApproved = counts.FirstOrDefault(c => c.Status == FinReceivableWriteOffProposalStatuses.Approved)?.Count ?? 0;
        int totalPosted = counts.FirstOrDefault(c => c.Status == FinReceivableWriteOffProposalStatuses.Posted)?.Count ?? 0;
        int totalRejected = counts.FirstOrDefault(c => c.Status == FinReceivableWriteOffProposalStatuses.Rejected)?.Count ?? 0;

        decimal totalProposed = counts.Sum(c => c.TotalProposed);
        decimal totalApprovedAmt = counts.Where(c => c.Status == FinReceivableWriteOffProposalStatuses.Approved || c.Status == FinReceivableWriteOffProposalStatuses.Posted).Sum(c => c.TotalApproved);
        decimal totalPostedAmt = counts.FirstOrDefault(c => c.Status == FinReceivableWriteOffProposalStatuses.Posted)?.TotalApproved ?? 0m;

        return new ReceivableWriteOffProposalSummaryResponse
        {
            TotalProposals = totalProposals,
            TotalDraft = totalDraft,
            TotalSubmitted = totalSubmitted,
            TotalApproved = totalApproved,
            TotalPosted = totalPosted,
            TotalRejected = totalRejected,
            TotalProposedAmount = totalProposed,
            TotalApprovedAmount = totalApprovedAmt,
            TotalPostedAmount = totalPostedAmt
        };
    }

    // ------------------------------------------------------------------------------------
    // Rincian Proposal — GET /write-off-proposals/{id}
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableWriteOffProposalDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var proposal = await _dbContext.FinReceivableWriteOffProposals
            .Include(p => p.Items.Where(i => !i.IsDelete))
                .ThenInclude(i => i.InvoiceBatch)
            .Include(p => p.Items.Where(i => !i.IsDelete))
                .ThenInclude(i => i.Receivable)
            .Include(p => p.Items.Where(i => !i.IsDelete))
                .ThenInclude(i => i.WriteOff)
            .SingleOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan pemutihan tagihan tidak ditemukan.");

        string? debtorName = null;
        if (proposal.DebtorReferenceId.HasValue)
        {
            var company = await _dbContext.MstCompanyGuarantors.AsNoTracking()
                .Where(c => c.Id == proposal.DebtorReferenceId.Value && !c.IsDelete)
                .Select(c => c.CompanyGuarantorName)
                .FirstOrDefaultAsync(cancellationToken);
            debtorName = company ?? await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(i => i.Id == proposal.DebtorReferenceId.Value && !i.IsDelete)
                .Select(i => i.InsuranceProviderName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var userIds = new[]
        {
            proposal.RequestedBy,
            proposal.ApprovedBy ?? Guid.Empty,
            proposal.PostedBy ?? Guid.Empty,
            proposal.RejectedBy ?? Guid.Empty
        }.Where(u => u != Guid.Empty).Distinct().ToList();

        var userNames = userIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? "-", cancellationToken)
            : new Dictionary<Guid, string>();

        // Resolusi info pasien dan billing per item
        var recIds = proposal.Items.Select(i => i.ReceivableId).Distinct().ToList();
        var receivableItems = recIds.Count > 0
            ? await _dbContext.FinReceivableItems.AsNoTracking()
                .Where(it => recIds.Contains(it.ReceivableId) && !it.IsDelete)
                .ToListAsync(cancellationToken)
            : new List<FinReceivableItem>();

        var invIds = proposal.Items.Where(i => i.Receivable?.InvoiceId != null).Select(i => i.Receivable!.InvoiceId!.Value)
            .Concat(receivableItems.Where(it => it.InvoiceId.HasValue).Select(it => it.InvoiceId!.Value))
            .Distinct()
            .ToList();

        var billingInvoices = invIds.Count > 0
            ? await _dbContext.BilInvoices.AsNoTracking()
                .Where(inv => invIds.Contains(inv.Id) && !inv.IsDelete)
                .ToDictionaryAsync(inv => inv.Id, cancellationToken)
            : new Dictionary<Guid, BilInvoice>();

        var encIds = billingInvoices.Values.Select(inv => inv.EncounterId)
            .Concat(receivableItems.Where(it => it.EncounterId.HasValue).Select(it => it.EncounterId!.Value))
            .Where(eId => eId != Guid.Empty)
            .Distinct()
            .ToList();

        var encounters = encIds.Count > 0
            ? await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(e => encIds.Contains(e.Id) && !e.IsDelete)
                .Select(e => new
                {
                    e.Id,
                    e.EncounterNumber,
                    e.EncounterType,
                    e.EncounterDate,
                    e.PatientId,
                    e.DoctorId
                })
                .ToDictionaryAsync(e => e.Id, cancellationToken)
            : new();

        var patIds = encounters.Values.Select(e => e.PatientId)
            .Concat(receivableItems.Where(it => it.PatientId.HasValue).Select(it => it.PatientId!.Value))
            .Where(pId => pId != Guid.Empty)
            .Distinct()
            .ToList();

        var patients = patIds.Count > 0
            ? await _dbContext.MstPatients.AsNoTracking()
                .Where(p => patIds.Contains(p.Id) && !p.IsDelete)
                .ToDictionaryAsync(p => p.Id, cancellationToken)
            : new Dictionary<Guid, MstPatient>();

        var docIds = encounters.Values.Where(e => e.DoctorId.HasValue).Select(e => e.DoctorId!.Value).Distinct().ToList();
        var doctors = docIds.Count > 0
            ? await _dbContext.MstDoctors.AsNoTracking()
                .Where(d => docIds.Contains(d.Id) && !d.IsDelete)
                .ToDictionaryAsync(d => d.Id, d => d.FullName, cancellationToken)
            : new Dictionary<Guid, string>();

        var mappedItems = proposal.Items.Select(item =>
        {
            var r = item.Receivable;
            var itemsForRec = receivableItems.Where(it => it.ReceivableId == item.ReceivableId).ToList();
            Guid? invId = r?.InvoiceId ?? itemsForRec.FirstOrDefault(it => it.InvoiceId.HasValue)?.InvoiceId;
            BilInvoice? inv = invId.HasValue && billingInvoices.TryGetValue(invId.Value, out var foundInv) ? foundInv : null;

            Guid? encId = inv?.EncounterId ?? itemsForRec.FirstOrDefault(it => it.EncounterId.HasValue)?.EncounterId;
            var enc = encId.HasValue && encounters.TryGetValue(encId.Value, out var foundEnc) ? foundEnc : null;

            Guid? patId = enc?.PatientId ?? itemsForRec.FirstOrDefault(it => it.PatientId.HasValue)?.PatientId;
            MstPatient? pat = patId.HasValue && patients.TryGetValue(patId.Value, out var foundPat) ? foundPat : null;

            string? docName = enc?.DoctorId.HasValue == true && doctors.TryGetValue(enc.DoctorId.Value, out var d) ? d : null;

            var serviceType = enc != null
                ? (enc.EncounterType == EncounterType.Inpatient ? "RANAP" : enc.EncounterType == EncounterType.Emergency ? "IGD" : "RAJAL")
                : (item.InvoiceBatch?.ServiceType ?? (item.InvoiceBatch?.VisitCode == "IP" ? "RANAP" : "RAJAL"));

            DateOnly? billingDate = null;
            if (inv?.InvoiceDate.HasValue == true)
            {
                billingDate = DateOnly.FromDateTime(inv.InvoiceDate.Value.LocalDateTime);
            }
            else if (r != null)
            {
                billingDate = r.DueDate.AddDays(-30);
            }

            DateOnly? serviceDate = enc != null
                ? DateOnly.FromDateTime(enc.EncounterDate)
                : billingDate;

            return new ReceivableWriteOffProposalItemResponse
            {
                Id = item.Id,
                ProposalId = item.ProposalId,
                InvoiceBatchId = item.InvoiceBatchId,
                InvoiceNumber = item.InvoiceBatch?.BatchNumber ?? "-",
                ReceivableId = item.ReceivableId,
                ReceivableNumber = r?.ReceivableNumber ?? "-",
                PatientName = pat?.FullName ?? "-",
                MedicalRecordNumber = pat?.MedicalRecordNumber ?? "-",
                BillingNumber = inv?.InvoiceNumber ?? "-",
                EncounterNumber = enc?.EncounterNumber ?? "-",
                ServiceDate = serviceDate,
                ServiceType = serviceType,
                DoctorName = docName ?? "-",
                OutstandingSnapshot = item.OutstandingSnapshot,
                CurrentOutstandingAmount = r?.OutstandingAmount ?? 0m,
                ProposedAmount = item.ProposedAmount,
                ApprovedAmount = item.ApprovedAmount,
                Note = item.Note,
                WriteOffId = item.WriteOffId,
                WriteOffNumber = item.WriteOff?.WriteOffNumber
            };
        }).ToList();

        var invoiceSummaries = mappedItems
            .GroupBy(i => new { i.InvoiceBatchId, i.InvoiceNumber })
            .Select(g => new ReceivableWriteOffProposalInvoiceSummaryResponse
            {
                InvoiceBatchId = g.Key.InvoiceBatchId,
                InvoiceNumber = g.Key.InvoiceNumber,
                InvoiceDate = proposal.Items.FirstOrDefault(it => it.InvoiceBatchId == g.Key.InvoiceBatchId)?.InvoiceBatch?.InvoiceDate,
                TotalInvoiceAmount = proposal.Items.Where(it => it.InvoiceBatchId == g.Key.InvoiceBatchId).Select(it => it.Receivable?.OriginalAmount ?? 0m).Sum(),
                TotalOutstandingAmount = g.Sum(x => x.OutstandingSnapshot),
                TotalProposedAmount = g.Sum(x => x.ProposedAmount),
                TotalApprovedAmount = g.Sum(x => x.ApprovedAmount),
                MemberCount = g.Count()
            })
            .ToList();

        return new ReceivableWriteOffProposalDetailResponse
        {
            Id = proposal.Id,
            ProposalNumber = proposal.ProposalNumber,
            ProposalDate = proposal.ProposalDate,
            ProposalType = proposal.ProposalType,
            ServiceType = proposal.ServiceType,
            ServiceLabel = ResolveServiceLabel(proposal.ServiceType),
            DebtorReferenceId = proposal.DebtorReferenceId,
            DebtorName = debtorName,
            ReasonCode = proposal.ReasonCode,
            ReasonLabel = FinReceivableWriteOffReasonCodes.DisplayLabels.GetValueOrDefault(proposal.ReasonCode, proposal.ReasonCode),
            ReasonDescription = proposal.ReasonDescription,
            Status = proposal.Status,
            TotalInvoiceAmount = proposal.TotalInvoiceAmount,
            TotalOutstandingAmount = proposal.TotalOutstandingAmount,
            TotalProposedAmount = proposal.TotalProposedAmount,
            TotalApprovedAmount = proposal.TotalApprovedAmount,
            InvoiceCount = invoiceSummaries.Count,
            ItemCount = mappedItems.Count,
            RequestedBy = proposal.RequestedBy,
            RequestedByName = userNames.GetValueOrDefault(proposal.RequestedBy, "-"),
            RequestedAt = proposal.RequestedAt,
            ApprovedBy = proposal.ApprovedBy,
            ApprovedByName = proposal.ApprovedBy.HasValue ? userNames.GetValueOrDefault(proposal.ApprovedBy.Value, "-") : null,
            ApprovedAt = proposal.ApprovedAt,
            PostedBy = proposal.PostedBy,
            PostedByName = proposal.PostedBy.HasValue ? userNames.GetValueOrDefault(proposal.PostedBy.Value, "-") : null,
            PostedAt = proposal.PostedAt,
            RejectedBy = proposal.RejectedBy,
            RejectedByName = proposal.RejectedBy.HasValue ? userNames.GetValueOrDefault(proposal.RejectedBy.Value, "-") : null,
            RejectedAt = proposal.RejectedAt,
            RejectionReason = proposal.RejectionReason,
            RowVersion = proposal.RowVersion,
            Items = mappedItems,
            Invoices = invoiceSummaries
        };
    }

    // ------------------------------------------------------------------------------------
    // Buat Draft Proposal — POST /write-off-proposals
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableWriteOffProposalDetailResponse> CreateDraftAsync(
        CreateReceivableWriteOffProposalRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        ValidateProposalInput(request.ProposalType, request.ReasonCode, request.ReasonDescription, request.Items);

        var distinctItemRecIds = request.Items.Select(i => i.ReceivableId).Distinct().ToList();
        if (distinctItemRecIds.Count != request.Items.Count)
            throw new ReceivableValidationException("Terdapat duplikasi piutang anggota dalam pengajuan pemutihan.");

        var invoiceBatchIds = request.Items.Select(i => i.InvoiceBatchId).Distinct().ToList();
        if (request.ProposalType == FinReceivableWriteOffProposalTypes.PerInvoice && invoiceBatchIds.Count > 1)
            throw new ReceivableValidationException("Pengajuan berjenis PER_INVOICE hanya boleh mencakup satu Invoice AR.");

        // Validasi batch exist dan aktif
        var batches = await _dbContext.FinReceivableInvoiceBatches
            .Include(b => b.Items)
            .Where(b => invoiceBatchIds.Contains(b.Id) && !b.IsDelete)
            .ToListAsync(cancellationToken);

        if (batches.Count != invoiceBatchIds.Count)
            throw new KeyNotFoundException("Satu atau lebih Invoice AR yang dipilih tidak ditemukan.");

        foreach (var b in batches)
        {
            if (b.Status == FinReceivableInvoiceBatchStatuses.Cancelled || b.IsCancel)
                throw new ReceivableValidationException($"Invoice AR {b.BatchNumber} telah dibatalkan.");
            if (b.ReissuedToBatchId.HasValue)
                throw new ReceivableValidationException($"Invoice AR {b.BatchNumber} sudah digantikan oleh pembuatan ulang baru (reissue).");
        }

        // Jika BATCH: penjamin harus sama
        var debtorReferenceIds = batches.Select(b => b.DebtorReferenceId).Distinct().ToList();
        if (debtorReferenceIds.Count > 1)
            throw new ReceivableValidationException("Seluruh Invoice AR dalam satu pengajuan BATCH harus memiliki penjamin yang sama.");

        var debtorRefId = debtorReferenceIds[0];

        // Validasi receviables
        var receivables = await _dbContext.FinReceivables
            .Where(r => distinctItemRecIds.Contains(r.Id) && !r.IsDelete)
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        if (receivables.Count != distinctItemRecIds.Count)
            throw new KeyNotFoundException("Satu atau lebih piutang anggota tidak ditemukan.");

        // Cek overlapping proposal aktif
        var pendingInOtherProposal = await _dbContext.FinReceivableWriteOffProposalItems.AsNoTracking()
            .Where(pi => !pi.IsDelete
                && distinctItemRecIds.Contains(pi.ReceivableId)
                && (pi.Proposal!.Status == FinReceivableWriteOffProposalStatuses.Submitted
                    || pi.Proposal.Status == FinReceivableWriteOffProposalStatuses.Approved)
                && !pi.Proposal.IsDelete
                && !pi.Proposal.IsCancel)
            .Select(pi => pi.Receivable!.ReceivableNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (pendingInOtherProposal != null)
            throw new ReceivableConflictException($"Piutang {pendingInOtherProposal} sedang dalam pengajuan pemutihan lain yang menunggu persetujuan.");

        var proposalId = Guid.NewGuid();
        var proposalNumber = GenerateProposalNumber();
        var today = FinanceBusinessDate.Today();

        var proposal = new FinReceivableWriteOffProposal
        {
            Id = proposalId,
            ProposalNumber = proposalNumber,
            ProposalDate = today,
            ProposalType = request.ProposalType.Trim().ToUpperInvariant(),
            ServiceType = request.ServiceType?.Trim().ToUpperInvariant() ?? batches[0].ServiceType,
            DebtorReferenceId = debtorRefId,
            ReasonCode = request.ReasonCode.Trim().ToUpperInvariant(),
            ReasonDescription = request.ReasonDescription.Trim(),
            Status = FinReceivableWriteOffProposalStatuses.Draft,
            RequestedBy = actorUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            CreateDateTime = DateTime.UtcNow
        };

        decimal totalInvoiceAmount = 0m;
        decimal totalOutstandingAmount = 0m;
        decimal totalProposedAmount = 0m;

        foreach (var itemReq in request.Items)
        {
            var batch = batches.First(b => b.Id == itemReq.InvoiceBatchId);
            var isMember = batch.Items.Any(i => i.ReceivableId == itemReq.ReceivableId && !i.IsDelete && i.IsActiveMembership);
            if (!isMember)
                throw new ReceivableValidationException($"Piutang {itemReq.ReceivableId} bukan anggota aktif dari Invoice AR {batch.BatchNumber}.");

            var rec = receivables[itemReq.ReceivableId];
            if (rec.Status is FinReceivableStatuses.Settled or FinReceivableStatuses.WrittenOff or FinReceivableStatuses.Cancelled)
                throw new ReceivableValidationException($"Piutang {rec.ReceivableNumber} berstatus {rec.Status} dan tidak dapat diajukan pemutihan.");

            if (itemReq.ProposedAmount <= 0)
                throw new ReceivableValidationException($"Nominal pemutihan untuk piutang {rec.ReceivableNumber} harus lebih besar dari nol.");

            if (itemReq.ProposedAmount > rec.OutstandingAmount)
                throw new ReceivableValidationException($"Nominal pemutihan untuk piutang {rec.ReceivableNumber} (Rp {itemReq.ProposedAmount:N0}) melebihi sisa piutang (Rp {rec.OutstandingAmount:N0}).");

            totalInvoiceAmount += rec.OriginalAmount;
            totalOutstandingAmount += rec.OutstandingAmount;
            totalProposedAmount += itemReq.ProposedAmount;

            proposal.Items.Add(new FinReceivableWriteOffProposalItem
            {
                Id = Guid.NewGuid(),
                ProposalId = proposalId,
                InvoiceBatchId = itemReq.InvoiceBatchId,
                ReceivableId = itemReq.ReceivableId,
                OutstandingSnapshot = rec.OutstandingAmount,
                ProposedAmount = itemReq.ProposedAmount,
                ApprovedAmount = itemReq.ProposedAmount,
                Note = itemReq.Note,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            });
        }

        proposal.TotalInvoiceAmount = totalInvoiceAmount;
        proposal.TotalOutstandingAmount = totalOutstandingAmount;
        proposal.TotalProposedAmount = totalProposedAmount;
        proposal.TotalApprovedAmount = totalProposedAmount;

        _dbContext.FinReceivableWriteOffProposals.Add(proposal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("CreateDraft", proposal.Id, actorUserId);
        return await GetByIdAsync(proposal.Id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Perbarui Draft Proposal — PUT /write-off-proposals/{id}
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableWriteOffProposalDetailResponse> UpdateDraftAsync(
        Guid id, UpdateReceivableWriteOffProposalRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var proposal = await _dbContext.FinReceivableWriteOffProposals
            .Include(p => p.Items)
            .SingleOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan pemutihan tidak ditemukan.");

        EnsureCurrent(proposal.RowVersion, request.ExpectedRowVersion);

        if (proposal.Status != FinReceivableWriteOffProposalStatuses.Draft)
            throw new ReceivableValidationException($"Hanya pengajuan berstatus DRAFT yang dapat disunting. Status saat ini: {proposal.Status}.");

        ValidateProposalInput(request.ProposalType, request.ReasonCode, request.ReasonDescription, request.Items);

        var distinctItemRecIds = request.Items.Select(i => i.ReceivableId).Distinct().ToList();
        if (distinctItemRecIds.Count != request.Items.Count)
            throw new ReceivableValidationException("Terdapat duplikasi piutang anggota dalam pengajuan pemutihan.");

        var invoiceBatchIds = request.Items.Select(i => i.InvoiceBatchId).Distinct().ToList();
        if (request.ProposalType == FinReceivableWriteOffProposalTypes.PerInvoice && invoiceBatchIds.Count > 1)
            throw new ReceivableValidationException("Pengajuan berjenis PER_INVOICE hanya boleh mencakup satu Invoice AR.");

        var batches = await _dbContext.FinReceivableInvoiceBatches
            .Include(b => b.Items)
            .Where(b => invoiceBatchIds.Contains(b.Id) && !b.IsDelete)
            .ToListAsync(cancellationToken);

        if (batches.Count != invoiceBatchIds.Count)
            throw new KeyNotFoundException("Satu atau lebih Invoice AR yang dipilih tidak ditemukan.");

        foreach (var b in batches)
        {
            if (b.Status == FinReceivableInvoiceBatchStatuses.Cancelled || b.IsCancel)
                throw new ReceivableValidationException($"Invoice AR {b.BatchNumber} telah dibatalkan.");
        }

        var debtorReferenceIds = batches.Select(b => b.DebtorReferenceId).Distinct().ToList();
        if (debtorReferenceIds.Count > 1)
            throw new ReceivableValidationException("Seluruh Invoice AR dalam satu pengajuan BATCH harus memiliki penjamin yang sama.");

        var receivables = await _dbContext.FinReceivables
            .Where(r => distinctItemRecIds.Contains(r.Id) && !r.IsDelete)
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        // Hapus items lama
        _dbContext.FinReceivableWriteOffProposalItems.RemoveRange(proposal.Items);
        proposal.Items.Clear();

        decimal totalInvoiceAmount = 0m;
        decimal totalOutstandingAmount = 0m;
        decimal totalProposedAmount = 0m;

        foreach (var itemReq in request.Items)
        {
            var batch = batches.First(b => b.Id == itemReq.InvoiceBatchId);
            var isMember = batch.Items.Any(i => i.ReceivableId == itemReq.ReceivableId && !i.IsDelete && i.IsActiveMembership);
            if (!isMember)
                throw new ReceivableValidationException($"Piutang {itemReq.ReceivableId} bukan anggota aktif dari Invoice AR {batch.BatchNumber}.");

            var rec = receivables[itemReq.ReceivableId];
            if (rec.Status is FinReceivableStatuses.Settled or FinReceivableStatuses.WrittenOff or FinReceivableStatuses.Cancelled)
                throw new ReceivableValidationException($"Piutang {rec.ReceivableNumber} berstatus {rec.Status} dan tidak dapat diajukan pemutihan.");

            if (itemReq.ProposedAmount <= 0)
                throw new ReceivableValidationException($"Nominal pemutihan untuk piutang {rec.ReceivableNumber} harus lebih besar dari nol.");

            if (itemReq.ProposedAmount > rec.OutstandingAmount)
                throw new ReceivableValidationException($"Nominal pemutihan untuk piutang {rec.ReceivableNumber} (Rp {itemReq.ProposedAmount:N0}) melebihi sisa piutang (Rp {rec.OutstandingAmount:N0}).");

            totalInvoiceAmount += rec.OriginalAmount;
            totalOutstandingAmount += rec.OutstandingAmount;
            totalProposedAmount += itemReq.ProposedAmount;

            proposal.Items.Add(new FinReceivableWriteOffProposalItem
            {
                Id = Guid.NewGuid(),
                ProposalId = proposal.Id,
                InvoiceBatchId = itemReq.InvoiceBatchId,
                ReceivableId = itemReq.ReceivableId,
                OutstandingSnapshot = rec.OutstandingAmount,
                ProposedAmount = itemReq.ProposedAmount,
                ApprovedAmount = itemReq.ProposedAmount,
                Note = itemReq.Note,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            });
        }

        proposal.ProposalType = request.ProposalType.Trim().ToUpperInvariant();
        proposal.ServiceType = request.ServiceType?.Trim().ToUpperInvariant() ?? batches[0].ServiceType;
        proposal.DebtorReferenceId = debtorReferenceIds[0];
        proposal.ReasonCode = request.ReasonCode.Trim().ToUpperInvariant();
        proposal.ReasonDescription = request.ReasonDescription.Trim();
        proposal.TotalInvoiceAmount = totalInvoiceAmount;
        proposal.TotalOutstandingAmount = totalOutstandingAmount;
        proposal.TotalProposedAmount = totalProposedAmount;
        proposal.TotalApprovedAmount = totalProposedAmount;
        proposal.UpdateBy = actorUserId;
        proposal.UpdateDateTime = DateTime.UtcNow;
        proposal.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("UpdateDraft", proposal.Id, actorUserId);
        return await GetByIdAsync(proposal.Id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Hapus Draft Proposal — DELETE /write-off-proposals/{id}
    // ------------------------------------------------------------------------------------

    public async Task DeleteDraftAsync(Guid id, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var proposal = await _dbContext.FinReceivableWriteOffProposals
            .Include(p => p.Items)
            .SingleOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan pemutihan tidak ditemukan.");

        EnsureCurrent(proposal.RowVersion, expectedRowVersion);

        if (proposal.Status != FinReceivableWriteOffProposalStatuses.Draft)
            throw new ReceivableValidationException($"Hanya pengajuan berstatus DRAFT yang dapat dihapus. Status saat ini: {proposal.Status}.");

        var now = DateTime.UtcNow;
        proposal.IsDelete = true;
        proposal.DeleteBy = actorUserId;
        proposal.DeleteDateTime = now;

        foreach (var item in proposal.Items)
        {
            item.IsDelete = true;
            item.DeleteBy = actorUserId;
            item.DeleteDateTime = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("DeleteDraft", proposal.Id, actorUserId);
    }

    // ------------------------------------------------------------------------------------
    // Submit Proposal (DRAFT -> SUBMITTED) — POST /write-off-proposals/{id}/submit
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableWriteOffProposalDetailResponse> SubmitAsync(
        Guid id, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var proposal = await _dbContext.FinReceivableWriteOffProposals
            .Include(p => p.Items).ThenInclude(i => i.Receivable)
            .Include(p => p.Items).ThenInclude(i => i.InvoiceBatch)
            .SingleOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan pemutihan tidak ditemukan.");

        EnsureCurrent(proposal.RowVersion, expectedRowVersion);

        if (proposal.Status != FinReceivableWriteOffProposalStatuses.Draft)
            throw new ReceivableValidationException($"Hanya proposal berstatus DRAFT yang dapat diajukan (SUBMIT). Status saat ini: {proposal.Status}.");

        if (proposal.Items.Count(i => !i.IsDelete) == 0)
            throw new ReceivableValidationException("Pengajuan pemutihan tidak memiliki rincian piutang anggota.");

        // Validasi ulang kelayakan piutang saat submit
        var itemRecIds = proposal.Items.Where(i => !i.IsDelete).Select(i => i.ReceivableId).ToList();

        var overlapping = await _dbContext.FinReceivableWriteOffProposalItems.AsNoTracking()
            .Where(pi => pi.ProposalId != proposal.Id
                && !pi.IsDelete
                && itemRecIds.Contains(pi.ReceivableId)
                && (pi.Proposal!.Status == FinReceivableWriteOffProposalStatuses.Submitted
                    || pi.Proposal.Status == FinReceivableWriteOffProposalStatuses.Approved)
                && !pi.Proposal.IsDelete
                && !pi.Proposal.IsCancel)
            .Select(pi => pi.Receivable!.ReceivableNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (overlapping != null)
            throw new ReceivableConflictException($"Piutang {overlapping} sedang dalam pengajuan pemutihan lain yang menunggu persetujuan/posting.");

        foreach (var item in proposal.Items.Where(i => !i.IsDelete))
        {
            var r = item.Receivable;
            if (r == null || r.IsDelete || r.Status is FinReceivableStatuses.Settled or FinReceivableStatuses.WrittenOff or FinReceivableStatuses.Cancelled)
                throw new ReceivableValidationException($"Piutang {r?.ReceivableNumber ?? item.ReceivableId.ToString()} sudah tidak aktif atau lunas.");

            if (item.ProposedAmount > r.OutstandingAmount)
                throw new ReceivableValidationException($"Nominal pemutihan piutang {r.ReceivableNumber} (Rp {item.ProposedAmount:N0}) melebihi sisa piutang saat ini (Rp {r.OutstandingAmount:N0}).");

            if (item.InvoiceBatch == null || item.InvoiceBatch.IsDelete || item.InvoiceBatch.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
                throw new ReceivableValidationException($"Invoice AR {item.InvoiceBatch?.BatchNumber} telah dibatalkan.");
        }

        proposal.Status = FinReceivableWriteOffProposalStatuses.Submitted;
        proposal.RequestedBy = actorUserId;
        proposal.RequestedAt = DateTimeOffset.UtcNow;
        proposal.UpdateBy = actorUserId;
        proposal.UpdateDateTime = DateTime.UtcNow;
        proposal.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Submit", proposal.Id, actorUserId);
        return await GetByIdAsync(proposal.Id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Setujui Proposal (SUBMITTED -> APPROVED) — POST /write-off-proposals/{id}/approve
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableWriteOffProposalDetailResponse> ApproveAsync(
        Guid id, ApproveReceivableWriteOffProposalRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var proposal = await _dbContext.FinReceivableWriteOffProposals
            .Include(p => p.Items)
            .SingleOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan pemutihan tidak ditemukan.");

        EnsureCurrent(proposal.RowVersion, request.ExpectedRowVersion);

        if (proposal.Status != FinReceivableWriteOffProposalStatuses.Submitted)
            throw new ReceivableValidationException($"Hanya proposal berstatus SUBMITTED yang dapat disetujui. Status saat ini: {proposal.Status}.");

        // MAKER-CHECKER ENFORCEMENT
        if (actorUserId == proposal.RequestedBy)
            throw new ReceivableValidationException("Pengaju tidak boleh menyetujui pengajuannya sendiri. Mintalah persetujuan pengguna lain yang berwenang (maker-checker).");

        if (request.ItemApprovals != null && request.ItemApprovals.Count > 0)
        {
            var approvalMap = request.ItemApprovals.ToDictionary(a => a.ItemId, a => a.ApprovedAmount);
            foreach (var item in proposal.Items.Where(i => !i.IsDelete))
            {
                if (approvalMap.TryGetValue(item.Id, out var approvedAmt))
                {
                    if (approvedAmt < 0)
                        throw new ReceivableValidationException("Nominal pemutihan yang disetujui tidak boleh bernilai negatif.");
                    if (approvedAmt > item.ProposedAmount)
                        throw new ReceivableValidationException($"Nominal pemutihan disetujui (Rp {approvedAmt:N0}) melebihi nominal yang diajukan (Rp {item.ProposedAmount:N0}).");

                    item.ApprovedAmount = approvedAmt;
                }
            }
        }
        else
        {
            // Default: ApprovedAmount = ProposedAmount
            foreach (var item in proposal.Items.Where(i => !i.IsDelete))
            {
                item.ApprovedAmount = item.ProposedAmount;
            }
        }

        proposal.TotalApprovedAmount = proposal.Items.Where(i => !i.IsDelete).Sum(i => i.ApprovedAmount);
        proposal.Status = FinReceivableWriteOffProposalStatuses.Approved;
        proposal.ApprovedBy = actorUserId;
        proposal.ApprovedAt = DateTimeOffset.UtcNow;
        proposal.UpdateBy = actorUserId;
        proposal.UpdateDateTime = DateTime.UtcNow;
        proposal.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Approve", proposal.Id, actorUserId);
        return await GetByIdAsync(proposal.Id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Tolak Proposal (SUBMITTED -> REJECTED) — POST /write-off-proposals/{id}/reject
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableWriteOffProposalDetailResponse> RejectAsync(
        Guid id, RejectReceivableWriteOffProposalRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            throw new ReceivableBadRequestException("Alasan penolakan pengajuan pemutihan wajib diisi.");

        var proposal = await _dbContext.FinReceivableWriteOffProposals
            .SingleOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan pemutihan tidak ditemukan.");

        EnsureCurrent(proposal.RowVersion, request.ExpectedRowVersion);

        if (proposal.Status != FinReceivableWriteOffProposalStatuses.Submitted)
            throw new ReceivableValidationException($"Hanya proposal berstatus SUBMITTED yang dapat ditolak. Status saat ini: {proposal.Status}.");

        proposal.Status = FinReceivableWriteOffProposalStatuses.Rejected;
        proposal.RejectedBy = actorUserId;
        proposal.RejectedAt = DateTimeOffset.UtcNow;
        proposal.RejectionReason = request.RejectionReason.Trim();
        proposal.UpdateBy = actorUserId;
        proposal.UpdateDateTime = DateTime.UtcNow;
        proposal.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Reject", proposal.Id, actorUserId);
        return await GetByIdAsync(proposal.Id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Posting Pemutihan (APPROVED -> POSTED) — POST /write-off-proposals/{id}/post
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableWriteOffProposalDetailResponse> PostAsync(
        Guid id, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var proposal = await _dbContext.FinReceivableWriteOffProposals
            .Include(p => p.Items)
            .SingleOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan pemutihan tidak ditemukan.");

        EnsureCurrent(proposal.RowVersion, expectedRowVersion);

        // IDEMPOTENCY CHECK
        if (proposal.Status == FinReceivableWriteOffProposalStatuses.Posted)
        {
            return await GetByIdAsync(proposal.Id, cancellationToken);
        }

        if (proposal.Status != FinReceivableWriteOffProposalStatuses.Approved)
            throw new ReceivableValidationException($"Hanya proposal berstatus APPROVED yang dapat diposting. Status saat ini: {proposal.Status}.");

        var activeItems = proposal.Items.Where(i => !i.IsDelete && i.ApprovedAmount > 0).ToList();
        if (activeItems.Count == 0)
            throw new ReceivableValidationException("Tidak terdapat nominal pemutihan yang disetujui untuk diposting.");

        var batchIdsToLock = activeItems.Select(i => i.InvoiceBatchId).Distinct().OrderBy(bId => bId).ToList();
        var recIdsToLock = activeItems.Select(i => i.ReceivableId).Distinct().OrderBy(rId => rId).ToList();

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);

            // Distributed transaction locks secara berurutan dan deterministik
            await AcquireLockAsync($"FIN_WRITE_OFF_PROPOSAL_{proposal.Id:N}", cancellationToken);

            foreach (var bId in batchIdsToLock)
                await AcquireLockAsync($"FIN_AR_BATCH_{bId:N}", cancellationToken);

            foreach (var rId in recIdsToLock)
                await AcquireLockAsync($"FIN_RECEIVABLE_{rId:N}", cancellationToken);

            // Re-validate status proposal di bawah lock
            var lockedProposal = await _dbContext.FinReceivableWriteOffProposals
                .Include(p => p.Items)
                .SingleOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pengajuan pemutihan tidak ditemukan.");

            if (lockedProposal.Status == FinReceivableWriteOffProposalStatuses.Posted)
            {
                await RollbackAsync(transaction);
                return await GetByIdAsync(proposal.Id, cancellationToken);
            }

            if (lockedProposal.Status != FinReceivableWriteOffProposalStatuses.Approved)
                throw new ReceivableValidationException($"Proposal berstatus {lockedProposal.Status} dan tidak dapat diposting.");

            var now = DateTimeOffset.UtcNow;
            var accountingDate = FinanceBusinessDate.ToDateOnly(now);

            // Load receivables dan batches under lock
            var lockedReceivables = await _dbContext.FinReceivables
                .Where(r => recIdsToLock.Contains(r.Id) && !r.IsDelete)
                .ToDictionaryAsync(r => r.Id, cancellationToken);

            var lockedBatches = await _dbContext.FinReceivableInvoiceBatches
                .Include(b => b.Items)
                .Where(b => batchIdsToLock.Contains(b.Id) && !b.IsDelete)
                .ToDictionaryAsync(b => b.Id, cancellationToken);

            foreach (var item in lockedProposal.Items.Where(i => !i.IsDelete && i.ApprovedAmount > 0))
            {
                if (!lockedReceivables.TryGetValue(item.ReceivableId, out var rec))
                    throw new KeyNotFoundException($"Piutang {item.ReceivableId} tidak ditemukan.");

                if (rec.Status is FinReceivableStatuses.Settled or FinReceivableStatuses.WrittenOff or FinReceivableStatuses.Cancelled)
                    throw new ReceivableValidationException($"Piutang {rec.ReceivableNumber} berstatus {rec.Status} dan tidak dapat dihapusbukukan.");

                if (item.ApprovedAmount > rec.OutstandingAmount)
                    throw new ReceivableValidationException($"Nominal pemutihan yang disetujui (Rp {item.ApprovedAmount:N0}) melebihi sisa piutang {rec.ReceivableNumber} saat ini (Rp {rec.OutstandingAmount:N0}) akibat pembayaran atau transaksi lain.");

                if (!lockedBatches.TryGetValue(item.InvoiceBatchId, out var batch))
                    throw new KeyNotFoundException($"Invoice AR {item.InvoiceBatchId} tidak ditemukan.");

                if (batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled || batch.IsCancel)
                    throw new ReceivableValidationException($"Invoice AR {batch.BatchNumber} telah dibatalkan sebelum posting pemutihan.");

                // 1. Buat record FinReceivableWriteOff
                var writeOff = new FinReceivableWriteOff
                {
                    Id = Guid.NewGuid(),
                    WriteOffNumber = GenerateWriteOffNumber(),
                    ReceivableId = rec.Id,
                    Amount = item.ApprovedAmount,
                    Reason = $"{lockedProposal.ReasonCode}: {lockedProposal.ReasonDescription}",
                    Status = FinReceivableApprovalStatuses.Approved,
                    RequestedBy = lockedProposal.RequestedBy,
                    RequestedAt = lockedProposal.RequestedAt,
                    ApprovedBy = lockedProposal.ApprovedBy,
                    ApprovedAt = lockedProposal.ApprovedAt,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateBy = actorUserId,
                    UpdateDateTime = DateTime.UtcNow,
                    RowVersion = Guid.NewGuid()
                };

                _dbContext.FinReceivableWriteOffs.Add(writeOff);
                item.WriteOffId = writeOff.Id;

                // 2. Mutasi saldo piutang
                var balanceBefore = rec.OutstandingAmount;
                rec.OutstandingAmount -= item.ApprovedAmount;
                rec.WrittenOffAmount += item.ApprovedAmount;
                rec.Status = rec.OutstandingAmount <= 0m ? FinReceivableStatuses.WrittenOff : FinReceivableStatuses.Partial;
                rec.UpdateDateTime = DateTime.UtcNow;
                rec.UpdateBy = actorUserId;
                rec.RowVersion = Guid.NewGuid();

                // 3. Stage Accounting Outbox Event
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PemutihanPiutang,
                    SourceTransactionId = rec.ReceivableNumber,
                    EventOccurredAt = now,
                    AccountingDate = accountingDate,
                    Amount = item.ApprovedAmount,
                    CorrelationId = rec.CorrelationId,
                    CausationId = lockedProposal.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);

                // 4. Catat mutasi Subledger PENGHAPUSAN
                await _subledgerMovementService.RecordReceivableMovementAsync(
                    receivable: rec,
                    movementType: FinReceivableMovementTypes.Penghapusan,
                    deltaAmount: -item.ApprovedAmount,
                    balanceBefore: balanceBefore,
                    occurredAt: now,
                    actorUserId: actorUserId,
                    correlationId: rec.CorrelationId,
                    causationId: lockedProposal.Id,
                    referenceNumber: writeOff.WriteOffNumber,
                    notes: $"{lockedProposal.ReasonCode}: {lockedProposal.ReasonDescription}",
                    cancellationToken: cancellationToken);
            }

            // 5. Perbarui status setiap Invoice AR yang terpengaruh
            foreach (var batch in lockedBatches.Values)
            {
                var memberRecIds = batch.Items
                    .Where(i => !i.IsDelete && i.IsActiveMembership)
                    .Select(i => i.ReceivableId)
                    .Distinct()
                    .ToList();

                var allMembers = await _dbContext.FinReceivables
                    .Where(r => memberRecIds.Contains(r.Id) && !r.IsDelete)
                    .ToListAsync(cancellationToken);

                var allSettled = allMembers.Count > 0 && allMembers.All(r => r.OutstandingAmount <= 0m);
                var totalOutstanding = allMembers.Sum(r => r.OutstandingAmount);
                var totalOriginal = allMembers.Sum(r => r.OriginalAmount);

                if (batch.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
                {
                    if (allSettled && totalOriginal > 0)
                    {
                        batch.Status = FinReceivableInvoiceBatchStatuses.Paid;
                    }
                    else if (totalOutstanding < totalOriginal)
                    {
                        batch.Status = FinReceivableInvoiceBatchStatuses.PartiallyPaid;
                    }

                    batch.UpdateDateTime = DateTime.UtcNow;
                    batch.UpdateBy = actorUserId;
                    batch.RowVersion = Guid.NewGuid();
                }
            }

            // 6. Tandai Proposal sebagai POSTED
            lockedProposal.Status = FinReceivableWriteOffProposalStatuses.Posted;
            lockedProposal.PostedBy = actorUserId;
            lockedProposal.PostedAt = now;
            lockedProposal.UpdateDateTime = DateTime.UtcNow;
            lockedProposal.UpdateBy = actorUserId;
            lockedProposal.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            await AuditAsync("Post", lockedProposal.Id, actorUserId);
            return await GetByIdAsync(lockedProposal.Id, cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(transaction);
            throw new ReceivableConflictException("Terjadi konflik konkurensi saat memposting pemutihan piutang. Coba lagi.", ex);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction != null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Helper Methods
    // ------------------------------------------------------------------------------------

    private static void ValidateProposalInput(
        string proposalType, string reasonCode, string reasonDescription, List<CreateReceivableWriteOffProposalItemRequest>? items)
    {
        if (string.IsNullOrWhiteSpace(proposalType))
            throw new ReceivableBadRequestException("Jenis pengajuan (ProposalType) wajib diisi.");

        var pt = proposalType.Trim().ToUpperInvariant();
        if (pt != FinReceivableWriteOffProposalTypes.PerInvoice && pt != FinReceivableWriteOffProposalTypes.Batch)
            throw new ReceivableValidationException($"Jenis pengajuan '{proposalType}' tidak valid. Gunakan PER_INVOICE atau BATCH.");

        if (string.IsNullOrWhiteSpace(reasonCode))
            throw new ReceivableBadRequestException("Kode alasan pemutihan (ReasonCode) wajib diisi.");

        var rc = reasonCode.Trim().ToUpperInvariant();
        if (!FinReceivableWriteOffReasonCodes.DisplayLabels.ContainsKey(rc))
            throw new ReceivableValidationException($"Kode alasan pemutihan '{reasonCode}' tidak valid.");

        if (string.IsNullOrWhiteSpace(reasonDescription))
            throw new ReceivableBadRequestException("Deskripsi/alasan pemutihan wajib diisi.");

        if (items == null || items.Count == 0)
            throw new ReceivableValidationException("Pengajuan pemutihan harus memiliki setidaknya satu rincian piutang anggota.");
    }

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion)
            throw new ReceivableConflictException("Data telah berubah oleh pengguna atau proses lain. Muat ulang sebelum melanjutkan.");
    }

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
        }
    }

    private static string GenerateProposalNumber()
    {
        var candidate = $"PROP-WO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string GenerateWriteOffNumber()
    {
        var candidate = $"WO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string ResolveServiceLabel(string? serviceType) => (serviceType ?? string.Empty).ToUpperInvariant() switch
    {
        "RANAP" => "Rawat Inap",
        "RAJAL" => "Rawat Jalan",
        "IGD" => "IGD",
        "ODC" => "ODC",
        "RAD" => "Radiologi",
        "LAB" => "Laboratorium",
        _ => serviceType ?? "-"
    };

    private Task AuditAsync(string action, Guid proposalId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"WriteOffProposal.{action}",
            $"Aktivitas Pengajuan Pemutihan Tagihan AR dicatat. ProposalId={proposalId}",
            new { ProposalId = proposalId, ActorUserId = actorUserId });
}
