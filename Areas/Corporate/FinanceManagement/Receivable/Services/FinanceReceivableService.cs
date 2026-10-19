using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.DTOs;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Satu-satunya penulis FinReceivable.OutstandingAmount (FIN-DES-011, konsekuensi kolom di
/// erd/data-dictionary.md §2.1) — TERMASUK saat dipanggil dari service lain. Mengurus empat hal:
/// umur piutang (aging, FR-FIN-022), pengajuan/keputusan koreksi (FinReceivableAdjustment),
/// pengajuan/keputusan penghapusan buku (FinReceivableWriteOff), dan sejak BE-FIN-018 —
/// ApplyAllocationAsync/ReverseAllocationAsync yang dipanggil FinanceReceiptService.AllocateAsync/
/// ReverseAllocationAsync (Collection/Services). Orkestrasi alokasi (validasi FR-FIN-041,
/// pembuatan baris FinReceiptAllocation, mutasi FinReceipt) tetap milik FinanceReceiptService —
/// tetapi mutasi FinReceivable itu sendiri MUST selalu lewat dua method di bagian bawah kelas
/// ini, tidak pernah ditulis langsung oleh service lain.
///
/// MUST NOT dipakai untuk membuat FinReceivable baru dari fakta Billing — itu tanggung jawab
/// FinanceBillingIntakeService (02-backend-architecture.md, belum ada task pemilik eksplisit,
/// lihat laporan BE-FIN-005 bagian 1).
///
/// BE-FIN-011: koreksi/penghapusan yang DISETUJUI menulis kejadian ke FinAccountingEventOutbox
/// lewat FinanceAccountingOutboxService, di dalam transaksi Decide*Async yang sama (FIN-DES-017).
/// </summary>
public sealed class FinanceReceivableService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;
    private readonly FinanceSubledgerMovementService _subledgerMovementService;

    public FinanceReceivableService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceAccountingOutboxService accountingOutboxService,
        FinanceSubledgerMovementService subledgerMovementService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _accountingOutboxService = accountingOutboxService;
        _subledgerMovementService = subledgerMovementService;
    }

    // ------------------------------------------------------------------------------------
    // Baca (BE-FIN-009: daftar, rincian, ringkasan) — ditambahkan saat controller dibangun,
    // tidak mengubah perilaku tulis yang sudah ada dari BE-FIN-008.
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<ReceivableResponse>> GetPagedAsync(ReceivableQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivables.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.DebtorType)) query = query.Where(x => x.DebtorType == request.DebtorType);
        if (request.DueDateFrom.HasValue) query = query.Where(x => x.DueDate >= request.DueDateFrom.Value);
        if (request.DueDateTo.HasValue) query = query.Where(x => x.DueDate <= request.DueDateTo.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.ReceivableNumber.ToUpper().Contains(search));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "receivablenumber" => descending ? query.OrderByDescending(x => x.ReceivableNumber) : query.OrderBy(x => x.ReceivableNumber),
            "outstandingamount" => descending ? query.OrderByDescending(x => x.OutstandingAmount) : query.OrderBy(x => x.OutstandingAmount),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate)
        };

        var total = await query.CountAsync(cancellationToken);
        var pagedEntities = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Resolve Debtor Names & Invoice Numbers in batch untuk daftar
        var debtorRefIds = pagedEntities.Where(x => x.DebtorReferenceId.HasValue).Select(x => x.DebtorReferenceId!.Value).Distinct().ToList();
        var companyNames = debtorRefIds.Count > 0
            ? await _dbContext.MstCompanyGuarantors.AsNoTracking()
                .Where(c => !c.IsDelete && debtorRefIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CompanyGuarantorName, cancellationToken)
            : new Dictionary<Guid, string>();

        var missingCompanyIds = debtorRefIds.Where(id => !companyNames.ContainsKey(id)).ToList();
        if (missingCompanyIds.Count > 0)
        {
            var insuranceNames = await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(p => !p.IsDelete && missingCompanyIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.InsuranceProviderName, cancellationToken);
            foreach (var kvp in insuranceNames)
            {
                companyNames[kvp.Key] = kvp.Value;
            }
        }

        // Pemilik manfaat lewat identitas Workforce (karyawan maupun dokter tetap), tanpa N+1.
        var ownerLookup = await FinanceBenefitOwnerDirectory.LoadAsync(
            _dbContext,
            pagedEntities
                .Where(x => x.BenefitOwnerId.HasValue || x.BenefitOwnerWorkforceProfileId.HasValue)
                .Select(x => ((Guid?)x.BenefitOwnerId, x.BenefitOwnerWorkforceProfileId)),
            cancellationToken);

        var invoiceIds = pagedEntities.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value).Distinct().ToList();
        var invoiceNumbers = invoiceIds.Count > 0
            ? await _dbContext.BilInvoices.AsNoTracking()
                .Where(i => invoiceIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => i.InvoiceNumber, cancellationToken)
            : new Dictionary<Guid, string>();

        var items = pagedEntities.Select(x =>
        {
            var res = Map(x);
            if (x.DebtorReferenceId.HasValue && companyNames.TryGetValue(x.DebtorReferenceId.Value, out var debtorName))
            {
                res.DebtorName = debtorName;
                res.CompanyName = debtorName;
                res.DebtorDisplayName = debtorName;
            }
            var benefitOwner = ownerLookup.Find(x.BenefitOwnerId, x.BenefitOwnerWorkforceProfileId);
            if (benefitOwner is not null)
            {
                res.BenefitOwnerWorkforceProfileId = benefitOwner.WorkforceProfileId;
                res.BenefitOwnerType = benefitOwner.OwnerType;
                res.BenefitOwnerName = benefitOwner.StaffName;
                res.BenefitOwnerEmployeeNumber = benefitOwner.StaffNumber;
                if (string.IsNullOrWhiteSpace(res.DebtorName))
                {
                    res.DebtorName = benefitOwner.StaffName;
                    res.CompanyName = "Internal RS";
                    res.DebtorDisplayName = $"{benefitOwner.StaffName} ({benefitOwner.StaffNumber})";
                    res.DebtorDisplayCategory = BenefitOwnerCategoryLabel(benefitOwner.OwnerType);
                }
            }
            if (x.InvoiceId.HasValue && invoiceNumbers.TryGetValue(x.InvoiceId.Value, out var invNum))
            {
                res.InvoiceNumber = invNum;
            }
            return res;
        }).ToList();

        return new PagedResult<ReceivableResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<ReceivableDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var receivable = await _dbContext.FinReceivables.AsNoTracking()
            .Include(x => x.Items).Include(x => x.Documents).Include(x => x.Adjustments).Include(x => x.WriteOffs)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

        var resReceivable = Map(receivable);

        string? debtorDisplayName = null;
        string? debtorDisplayCategory = null;

        // Resolve debtor name (Company Guarantor / Insurance Provider)
        if (receivable.DebtorReferenceId.HasValue)
        {
            var company = await _dbContext.MstCompanyGuarantors.AsNoTracking()
                .Where(c => !c.IsDelete && c.Id == receivable.DebtorReferenceId.Value)
                .Select(c => c.CompanyGuarantorName)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(company))
            {
                debtorDisplayName = company;
                debtorDisplayCategory = "Perusahaan penjamin";
                resReceivable.DebtorName = company;
                resReceivable.CompanyName = company;
            }
            else
            {
                var insurance = await _dbContext.MstInsuranceProviders.AsNoTracking()
                    .Where(p => !p.IsDelete && p.Id == receivable.DebtorReferenceId.Value)
                    .Select(p => p.InsuranceProviderName)
                    .FirstOrDefaultAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(insurance))
                {
                    debtorDisplayName = insurance;
                    debtorDisplayCategory = "Asuransi rekanan";
                    resReceivable.DebtorName = insurance;
                    resReceivable.CompanyName = insurance;
                }
            }
        }

        string? benefitOwnerName = null;
        string? benefitOwnerEmpNo = null;
        string? benefitOwnerType = null;
        if (receivable.BenefitOwnerId.HasValue || receivable.BenefitOwnerWorkforceProfileId.HasValue)
        {
            var ownerLookup = await FinanceBenefitOwnerDirectory.LoadAsync(
                _dbContext,
                new[] { ((Guid?)receivable.BenefitOwnerId, receivable.BenefitOwnerWorkforceProfileId) },
                cancellationToken);
            var emp = ownerLookup.Find(receivable.BenefitOwnerId, receivable.BenefitOwnerWorkforceProfileId);
            if (emp != null)
            {
                benefitOwnerName = emp.StaffName;
                benefitOwnerEmpNo = emp.StaffNumber;
                benefitOwnerType = emp.OwnerType;
                debtorDisplayName = $"{emp.StaffName} ({emp.StaffNumber})";
                debtorDisplayCategory = BenefitOwnerCategoryLabel(emp.OwnerType);
                resReceivable.DebtorName = emp.StaffName;
                resReceivable.CompanyName = "Internal RS";
                resReceivable.BenefitOwnerWorkforceProfileId = emp.WorkforceProfileId;
                resReceivable.BenefitOwnerType = emp.OwnerType;
                resReceivable.BenefitOwnerName = emp.StaffName;
                resReceivable.BenefitOwnerEmployeeNumber = emp.StaffNumber;
            }
        }

        var patientIds = receivable.Items.Where(x => x.PatientId.HasValue).Select(x => x.PatientId!.Value).Distinct().ToList();
        var patientsDict = patientIds.Count > 0
            ? await _dbContext.MstPatients.AsNoTracking()
                .Where(p => !p.IsDelete && patientIds.Contains(p.Id))
                .Select(p => new { p.Id, p.FullName, p.MedicalRecordNumber })
                .ToDictionaryAsync(p => p.Id, p => new { p.FullName, p.MedicalRecordNumber }, cancellationToken)
            : [];

        var firstPatientId = receivable.Items.FirstOrDefault(x => x.PatientId.HasValue)?.PatientId;
        string? resolvedPatientName = null;
        string? resolvedPatientMrn = null;
        if (firstPatientId.HasValue && patientsDict.TryGetValue(firstPatientId.Value, out var patientInfo))
        {
            resolvedPatientName = patientInfo.FullName;
            resolvedPatientMrn = patientInfo.MedicalRecordNumber;
            resReceivable.PatientId = firstPatientId;
            resReceivable.PatientName = resolvedPatientName;
            resReceivable.PatientMedicalRecordNumber = resolvedPatientMrn;
        }
        resReceivable.BenefitRelationship = receivable.BenefitRelationship;

        resReceivable.DebtorDisplayName = debtorDisplayName;
        resReceivable.DebtorDisplayCategory = debtorDisplayCategory;

        string? resolvedInvoiceNumber = null;
        string? invoiceServiceType = null;
        DateTimeOffset? invoiceDate = null;

        // Resolve invoice number asal (Billing)
        if (receivable.InvoiceId.HasValue)
        {
            var bilInvoice = await _dbContext.BilInvoices.AsNoTracking()
                .Where(i => i.Id == receivable.InvoiceId.Value)
                .Select(i => new { i.InvoiceNumber, i.ServiceType, i.InvoiceDate })
                .FirstOrDefaultAsync(cancellationToken);
            if (bilInvoice != null)
            {
                resolvedInvoiceNumber = bilInvoice.InvoiceNumber;
                invoiceServiceType = bilInvoice.ServiceType;
                invoiceDate = bilInvoice.InvoiceDate;
                resReceivable.InvoiceNumber = bilInvoice.InvoiceNumber;
            }
        }

        // Resolve batch invoice number (Finance AR Batch)
        var batchNumber = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
            .Where(b => b.ReceivableId == receivable.Id && b.IsActiveMembership && b.Batch!.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            .Select(b => b.Batch!.BatchNumber)
            .FirstOrDefaultAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(batchNumber))
        {
            resReceivable.BatchInvoiceNumber = batchNumber;
            if (string.IsNullOrWhiteSpace(resReceivable.InvoiceNumber))
            {
                resReceivable.InvoiceNumber = batchNumber;
                resolvedInvoiceNumber = batchNumber;
            }
        }

        resReceivable.InvoiceServiceType = invoiceServiceType;
        resReceivable.InvoiceDate = invoiceDate;

        return new ReceivableDetailResponse
        {
            Receivable = resReceivable,
            DebtorDisplayName = debtorDisplayName,
            DebtorDisplayCategory = debtorDisplayCategory,
            InvoiceNumber = resolvedInvoiceNumber,
            InvoiceServiceType = invoiceServiceType,
            InvoiceDate = invoiceDate,
            BenefitOwnerName = benefitOwnerName,
            BenefitOwnerEmployeeNumber = benefitOwnerEmpNo,
            BenefitOwnerType = benefitOwnerType,
            BenefitRelationship = receivable.BenefitRelationship,
            PatientId = firstPatientId,
            PatientName = resolvedPatientName,
            PatientMedicalRecordNumber = resolvedPatientMrn,
            Items = receivable.Items.Select(x =>
            {
                string? pName = null;
                string? pMrn = null;
                if (x.PatientId.HasValue && patientsDict.TryGetValue(x.PatientId.Value, out var pi))
                {
                    pName = pi.FullName;
                    pMrn = pi.MedicalRecordNumber;
                }
                return new ReceivableItemResponse
                {
                    Id = x.Id,
                    EncounterId = x.EncounterId,
                    InvoiceId = x.InvoiceId,
                    PatientId = x.PatientId,
                    PatientName = pName,
                    PatientMedicalRecordNumber = pMrn,
                    Description = x.Description,
                    Amount = x.Amount
                };
            }).ToList(),
            Documents = receivable.Documents.Select(x => new ReceivableDocumentResponse
            {
                Id = x.Id, DocumentType = x.DocumentType, DocumentNumber = x.DocumentNumber, IsReceived = x.IsReceived, ReceivedAt = x.ReceivedAt
            }).ToList(),
            Adjustments = receivable.Adjustments.Select(MapAdjustment).ToList(),
            WriteOffs = receivable.WriteOffs.Select(MapWriteOff).ToList()
        };
    }

    /// <summary>
    /// Agregasi dokumen asli terkait piutang (Rekap Tagihan Pelayanan Billing, Kwitansi Pembayaran Kasir, Invoice AR Batch Finance).
    /// Read-only, tidak memicu transaksi baru, dan mengembalikan referensi nyata untuk preview/download.
    /// </summary>
    public async Task<ReceivableDocumentsResponse> GetDocumentsAsync(Guid id, CancellationToken cancellationToken)
    {
        var receivable = await _dbContext.FinReceivables.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

        var response = new ReceivableDocumentsResponse
        {
            ReceivableId = receivable.Id,
            ReceivableNumber = receivable.ReceivableNumber
        };

        // 1. Rekap Tagihan Pelayanan Kesehatan (Billing)
        BilInvoice? bilInvoice = null;
        if (receivable.InvoiceId.HasValue)
        {
            bilInvoice = await _dbContext.BilInvoices.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == receivable.InvoiceId.Value, cancellationToken);
        }

        var hasBillingRecap = bilInvoice != null;
        response.Documents.Add(new ReceivableDocumentItemResponse
        {
            Id = receivable.InvoiceId?.ToString() ?? $"recap-{receivable.Id}",
            DocumentType = "BILLING_RECAP",
            DocumentName = "Rekap Tagihan Pelayanan Kesehatan",
            SourceModule = "Billing",
            ReferenceNumber = hasBillingRecap ? bilInvoice!.InvoiceNumber : "-",
            FileFormat = "PDF",
            Available = hasBillingRecap,
            Status = hasBillingRecap ? "Tersedia" : "Belum tersedia",
            ViewUrl = hasBillingRecap ? $"/api/v1/health-services/billing-management/billing/invoices/{bilInvoice!.Id}/company-guarantor-invoice-document" : null,
            Amount = receivable.OriginalAmount,
            DocumentDate = bilInvoice?.InvoiceDate
        });

        // 2. Kwitansi Pembayaran (Billing / Kasir)
        var kwitansiList = new List<ReceivableDocumentItemResponse>();
        if (receivable.InvoiceId.HasValue)
        {
            var tenders = await (from s in _dbContext.BilSettlements.AsNoTracking()
                                 join t in _dbContext.BilTenders.AsNoTracking() on s.Id equals t.SettlementId
                                 where s.InvoiceId == receivable.InvoiceId.Value && !t.IsDelete && !string.IsNullOrWhiteSpace(t.KwitansiNumber)
                                 select new { t.Id, t.KwitansiNumber, t.Amount, t.AttemptedAt })
                                 .ToListAsync(cancellationToken);

            foreach (var tender in tenders)
            {
                kwitansiList.Add(new ReceivableDocumentItemResponse
                {
                    Id = tender.Id.ToString(),
                    DocumentType = "CASHIER_RECEIPT",
                    DocumentName = $"Kwitansi Pembayaran - {tender.KwitansiNumber}",
                    SourceModule = "Billing/Kasir",
                    ReferenceNumber = tender.KwitansiNumber!,
                    FileFormat = "PDF",
                    Available = true,
                    Status = "Tersedia",
                    Amount = tender.Amount,
                    DocumentDate = tender.AttemptedAt
                });
            }
        }

        // Periksa juga penerimaan Finance jika ada receipt allocations
        var financeReceipts = await (from a in _dbContext.FinReceiptAllocations.AsNoTracking()
                                     join r in _dbContext.FinReceipts.AsNoTracking() on a.ReceiptId equals r.Id
                                     where a.ReceivableId == receivable.Id && !a.IsDelete && !r.IsDelete && !string.IsNullOrWhiteSpace(r.KwitansiNumber)
                                     select new { r.Id, r.KwitansiNumber, r.Amount, r.CreateDateTime })
                                     .ToListAsync(cancellationToken);

        foreach (var fr in financeReceipts)
        {
            if (!kwitansiList.Any(k => k.ReferenceNumber == fr.KwitansiNumber))
            {
                kwitansiList.Add(new ReceivableDocumentItemResponse
                {
                    Id = fr.Id.ToString(),
                    DocumentType = "CASHIER_RECEIPT",
                    DocumentName = $"Kwitansi Pembayaran - {fr.KwitansiNumber}",
                    SourceModule = "Billing/Kasir",
                    ReferenceNumber = fr.KwitansiNumber!,
                    FileFormat = "PDF",
                    Available = true,
                    Status = "Tersedia",
                    Amount = fr.Amount,
                    DocumentDate = fr.CreateDateTime
                });
            }
        }

        if (kwitansiList.Count > 0)
        {
            response.Documents.AddRange(kwitansiList);
        }
        else
        {
            response.Documents.Add(new ReceivableDocumentItemResponse
            {
                Id = $"kwitansi-{receivable.Id}",
                DocumentType = "CASHIER_RECEIPT",
                DocumentName = "Kwitansi Pembayaran",
                SourceModule = "Billing/Kasir",
                ReferenceNumber = "-",
                FileFormat = "PDF",
                Available = false,
                Status = "Belum tersedia",
                Amount = receivable.AllocatedAmount
            });
        }

        // 3. Invoice AR (Finance)
        var batchItem = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
            .Include(x => x.Batch)
            .Where(x => x.ReceivableId == receivable.Id && x.IsActiveMembership && x.Batch!.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            .FirstOrDefaultAsync(cancellationToken);

        if (batchItem?.Batch != null)
        {
            response.Documents.Add(new ReceivableDocumentItemResponse
            {
                Id = batchItem.Batch.Id.ToString(),
                DocumentType = "AR_INVOICE",
                DocumentName = $"Invoice AR - {batchItem.Batch.BatchNumber}",
                SourceModule = "Finance",
                ReferenceNumber = batchItem.Batch.BatchNumber,
                FileFormat = "PDF",
                Available = true,
                Status = "Tersedia",
                ViewUrl = $"/api/v1/corporate/finance-management/receivable-invoice-batches/{batchItem.Batch.Id}/document",
                Amount = batchItem.Batch.TotalAmount,
                DocumentDate = batchItem.Batch.IssuedAt ?? batchItem.Batch.CreateDateTime
            });
        }
        else
        {
            response.Documents.Add(new ReceivableDocumentItemResponse
            {
                Id = $"ar-invoice-{receivable.Id}",
                DocumentType = "AR_INVOICE",
                DocumentName = "Invoice AR",
                SourceModule = "Finance",
                ReferenceNumber = "-",
                FileFormat = "PDF",
                Available = false,
                Status = "Belum dibuat"
            });
        }

        return response;
    }

    // BE-FIN-100 (FR-FIN-219, FIN-DEC-176): parameter debtorType opsional, mengikuti pola
    // GetAgingSummaryAsync (BE-FIN-055) — null/kosong berarti seluruh FinReceivable tanpa
    // saringan, perilaku bawaan sebelum task ini TIDAK berubah bila dipanggil tanpa argumen ini.
    public async Task<ReceivableSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken, string? debtorType = null)
    {
        var query = _dbContext.FinReceivables.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(debtorType)) query = query.Where(x => x.DebtorType == debtorType);
        var counts = await query.GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        int Count(string status) => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;

        return new ReceivableSummaryResponse
        {
            TotalReceivable = counts.Sum(c => c.Count),
            OutstandingCount = Count(FinReceivableStatuses.Outstanding),
            PartialCount = Count(FinReceivableStatuses.Partial),
            SettledCount = Count(FinReceivableStatuses.Settled),
            WrittenOffCount = Count(FinReceivableStatuses.WrittenOff),
            TotalOutstandingAmount = await query.SumAsync(x => x.OutstandingAmount, cancellationToken)
        };
    }

    public Task<ReceivableFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new ReceivableFilterMetadataResponse
        {
            PageSizeOptions = [10, 25, 50, 100],
            SortableFields = ["dueDate", "receivableNumber", "outstandingAmount", "status"],
            StatusOptions =
            [
                FinReceivableStatuses.Outstanding, FinReceivableStatuses.Partial, FinReceivableStatuses.Settled,
                FinReceivableStatuses.WrittenOff, FinReceivableStatuses.Cancelled
            ]
        });

    private static string BenefitOwnerCategoryLabel(string? ownerType) =>
        ownerType == QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants.WorkforceBenefitOwnerTypes.Doctor
            ? "Dokter RS (Benefit)"
            : "Pegawai RS (Benefit)";

    public static ReceivableResponse Map(FinReceivable x) => new()
    {
        Id = x.Id,
        ReceivableNumber = x.ReceivableNumber,
        InvoiceId = x.InvoiceId,
        DebtorType = x.DebtorType,
        DebtorReferenceId = x.DebtorReferenceId,
        BenefitOwnerId = x.BenefitOwnerId,
        BenefitOwnerWorkforceProfileId = x.BenefitOwnerWorkforceProfileId,
        BenefitRelationship = x.BenefitRelationship,
        OriginalAmount = x.OriginalAmount,
        OutstandingAmount = x.OutstandingAmount,
        AllocatedAmount = x.AllocatedAmount,
        AdjustedAmount = x.AdjustedAmount,
        WrittenOffAmount = x.WrittenOffAmount,
        DueDate = x.DueDate,
        Status = x.Status,
        ClaimStatus = x.ClaimStatus,
        RecognizedAt = x.RecognizedAt,
        RowVersion = x.RowVersion
    };

    public static ReceivableAdjustmentResponse MapAdjustment(FinReceivableAdjustment x) => new()
    {
        Id = x.Id,
        AdjustmentNumber = x.AdjustmentNumber,
        Direction = x.Direction,
        Amount = x.Amount,
        Reason = x.Reason,
        Status = x.Status,
        RequestedBy = x.RequestedBy,
        RequestedAt = x.RequestedAt,
        ApprovedBy = x.ApprovedBy,
        ApprovedAt = x.ApprovedAt,
        RejectionReason = x.RejectionReason,
        RowVersion = x.RowVersion
    };

    public static ReceivableWriteOffResponse MapWriteOff(FinReceivableWriteOff x) => new()
    {
        Id = x.Id,
        WriteOffNumber = x.WriteOffNumber,
        Amount = x.Amount,
        Reason = x.Reason,
        Status = x.Status,
        RequestedBy = x.RequestedBy,
        RequestedAt = x.RequestedAt,
        ApprovedBy = x.ApprovedBy,
        ApprovedAt = x.ApprovedAt,
        RejectionReason = x.RejectionReason,
        RowVersion = x.RowVersion
    };

    // ------------------------------------------------------------------------------------
    // Umur piutang (FR-FIN-022, FIN-DEC-010) — dihitung saat query, tidak disimpan
    // (02-backend-architecture.md §10: menyimpannya berisiko basi).
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Empat kelompok tetap: "0-30", "31-60", "61-90", "di atas 90 hari" (FIN-DEC-010).
    /// Piutang yang belum jatuh tempo (DueDate di masa depan relatif asOfDate) tidak diatur
    /// eksplisit oleh FIN-DEC-010/FR-FIN-022 manapun — diperlakukan sebagai "0-30" (hari
    /// terlambat dianggap nol), bukan keputusan bisnis baru, sekadar nilai aman untuk kasus
    /// yang tidak diatur.
    /// </summary>
    /// <summary>BE-FIN-055: parameter debtorType opsional, nilai PAYER/PATIENT_GUARANTOR/
    /// EMPLOYEE_BENEFIT (FinReceivableDebtorTypes). Null/kosong berarti seluruh FinReceivable
    /// tanpa saringan — perilaku bawaan sebelum task ini, TIDAK berubah bila dipanggil tanpa
    /// argumen ini (dipakai GetSummaryAsync di bawah).</summary>
    public async Task<List<ReceivableAgingBucketResult>> GetAgingSummaryAsync(
        DateOnly? asOfDate, CancellationToken cancellationToken, string? debtorType = null)
    {
        var referenceDate = asOfDate ?? FinanceBusinessDate.Today();
        var query = _dbContext.FinReceivables.AsNoTracking()
            .Where(x => !x.IsDelete && x.OutstandingAmount > 0);
        if (!string.IsNullOrWhiteSpace(debtorType))
            query = query.Where(x => x.DebtorType == debtorType);

        var receivables = await query
            .Select(x => new { x.DueDate, x.OutstandingAmount })
            .ToListAsync(cancellationToken);

        var buckets = ReceivableAgingBuckets.Labels.ToDictionary(label => label, label => new ReceivableAgingBucketResult { BucketLabel = label });

        foreach (var receivable in receivables)
        {
            var daysPastDue = Math.Max(0, referenceDate.DayNumber - receivable.DueDate.DayNumber);
            var bucket = buckets[ReceivableAgingBuckets.Resolve(daysPastDue)];
            bucket.ReceivableCount++;
            bucket.TotalOutstandingAmount += receivable.OutstandingAmount;
        }

        return ReceivableAgingBuckets.Labels.Select(label => buckets[label]).ToList();
    }

    // ------------------------------------------------------------------------------------
    // Koreksi (FinReceivableAdjustment) — FIN-DES-014, FIN-VAL-020..025
    // ------------------------------------------------------------------------------------

    public async Task<FinReceivableAdjustment> RequestAdjustmentAsync(
        Guid receivableId, string direction, decimal amount, string reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        var normalizedDirection = ValidateDirection(direction);
        ValidateAmount(amount);
        var reasonText = ValidateReason(reason, "Alasan koreksi");

        await FindReceivableAsync(receivableId, cancellationToken);

        var adjustment = new FinReceivableAdjustment
        {
            AdjustmentNumber = GenerateNumber("ADJ"),
            ReceivableId = receivableId,
            Direction = normalizedDirection,
            Amount = amount,
            Reason = reasonText,
            Status = FinReceivableApprovalStatuses.Requested,
            RequestedBy = actorUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.FinReceivableAdjustments.Add(adjustment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Adjustment.Request", adjustment.Id, receivableId, actorUserId);
        return adjustment;
    }

    public Task<FinReceivableAdjustment> ApproveAdjustmentAsync(
        Guid adjustmentId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideAdjustmentAsync(adjustmentId, expectedRowVersion, actorUserId, approve: true, rejectionReason: null, cancellationToken);

    public Task<FinReceivableAdjustment> RejectAdjustmentAsync(
        Guid adjustmentId, Guid expectedRowVersion, string rejectionReason, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideAdjustmentAsync(adjustmentId, expectedRowVersion, actorUserId, approve: false,
            ValidateReason(rejectionReason, "Alasan penolakan"), cancellationToken);

    private async Task<FinReceivableAdjustment> DecideAdjustmentAsync(
        Guid adjustmentId, Guid expectedRowVersion, Guid actorUserId, bool approve, string? rejectionReason, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            var adjustment = await _dbContext.FinReceivableAdjustments
                .SingleOrDefaultAsync(x => x.Id == adjustmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pengajuan koreksi piutang tidak ditemukan.");
            EnsureCurrent(adjustment.RowVersion, expectedRowVersion);

            // FIN-VAL-025: keputusan hanya boleh sekali.
            if (adjustment.Status != FinReceivableApprovalStatuses.Requested)
                throw new ReceivableValidationException("Permohonan ini sudah diputuskan sebelumnya.");
            // FIN-VAL-020 / FIN-DES-014: pengaju tidak boleh menyetujui/menolak permohonannya sendiri.
            if (actorUserId == adjustment.RequestedBy)
                throw new ReceivableValidationException(
                    "Pengaju tidak boleh menyetujui permohonannya sendiri. Mintalah persetujuan pengguna lain yang berwenang.");

            await AcquireLockAsync($"FIN_RECEIVABLE_{adjustment.ReceivableId:N}", cancellationToken);
            var receivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => x.Id == adjustment.ReceivableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

            var balanceBefore = receivable.OutstandingAmount;
            decimal deltaAmount = 0m;

            if (approve)
            {
                // FIN-VAL-024: koreksi pengurang (CREDIT) tidak boleh melebihi sisa piutang.
                if (adjustment.Direction == FinReceivableAdjustmentDirections.Credit && adjustment.Amount > receivable.OutstandingAmount)
                    throw new ReceivableValidationException(
                        $"Nilai koreksi melebihi sisa piutang. Sisa saat ini Rp {receivable.OutstandingAmount:N0}.");

                // Invariant (FIN-VAL-011): OriginalAmount = Outstanding + Allocated + Adjusted + WrittenOff.
                // CREDIT mengurangi sisa dan menambah AdjustedAmount; DEBIT sebaliknya — status
                // FinReceivable TIDAK berubah oleh koreksi (state-transition-matrix.md §2 hanya
                // memetakan perubahan status dari alokasi, write-off, dan pembatalan).
                if (adjustment.Direction == FinReceivableAdjustmentDirections.Credit)
                {
                    receivable.OutstandingAmount -= adjustment.Amount;
                    receivable.AdjustedAmount += adjustment.Amount;
                    deltaAmount = -adjustment.Amount;
                }
                else
                {
                    receivable.OutstandingAmount += adjustment.Amount;
                    receivable.AdjustedAmount -= adjustment.Amount;
                    deltaAmount = +adjustment.Amount;
                }
                receivable.UpdateDateTime = DateTime.UtcNow;
                receivable.UpdateBy = actorUserId;
                receivable.RowVersion = Guid.NewGuid();

                adjustment.Status = FinReceivableApprovalStatuses.Approved;
            }
            else
            {
                adjustment.Status = FinReceivableApprovalStatuses.Rejected;
                adjustment.RejectionReason = rejectionReason;
            }
            adjustment.ApprovedBy = actorUserId;
            adjustment.ApprovedAt = DateTimeOffset.UtcNow;
            adjustment.UpdateDateTime = DateTime.UtcNow;
            adjustment.UpdateBy = actorUserId;
            adjustment.RowVersion = Guid.NewGuid();

            // BE-FIN-011: kejadian hanya lahir dari koreksi yang DISETUJUI (katalog 17 kode,
            // "Koreksi piutang disetujui") — penolakan tidak mengubah apa pun secara finansial.
            if (approve)
            {
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PenyesuaianPiutang,
                    SourceTransactionId = receivable.ReceivableNumber,
                    EventOccurredAt = adjustment.ApprovedAt!.Value,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(adjustment.ApprovedAt!.Value),
                    Amount = adjustment.Amount,
                    CorrelationId = receivable.CorrelationId,
                    CausationId = adjustment.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);

                // BE-FIN-060, FIN-DES-079: Mutasi subledger PENYESUAIAN piutang disetujui (Jalur 5)
                await _subledgerMovementService.RecordReceivableMovementAsync(
                    receivable: receivable,
                    movementType: FinReceivableMovementTypes.Penyesuaian,
                    deltaAmount: deltaAmount,
                    balanceBefore: balanceBefore,
                    occurredAt: adjustment.ApprovedAt!.Value,
                    actorUserId: actorUserId,
                    correlationId: receivable.CorrelationId,
                    causationId: adjustment.Id,
                    referenceNumber: adjustment.AdjustmentNumber,
                    notes: adjustment.Reason,
                    cancellationToken: cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync(approve ? "Adjustment.Approve" : "Adjustment.Reject", adjustment.Id, adjustment.ReceivableId, actorUserId);
            return adjustment;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
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
    // Penghapusan buku (FinReceivableWriteOff) — FIN-VAL-014, FIN-DES-014
    // ------------------------------------------------------------------------------------

    /// <summary>BE-FIN-053 (FIN-DES-073) — daftar LINTAS piutang, baca saja. Pembuatan dan
    /// keputusan write-off tetap lewat RequestWriteOffAsync/DecideWriteOffAsync di bawah.</summary>
    public async Task<PagedResult<ReceivableWriteOffRowResponse>> GetWriteOffsAsync(
        ReceivableWriteOffQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableWriteOffs.AsNoTracking()
            .Include(x => x.Receivable)
            .Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (request.DebtorReferenceId.HasValue) query = query.Where(x => x.Receivable!.DebtorReferenceId == request.DebtorReferenceId.Value);
        if (request.StartDate.HasValue) query = query.Where(x => x.RequestedAt >= request.StartDate.Value.ToDateTime(TimeOnly.MinValue));
        if (request.EndDate.HasValue) query = query.Where(x => x.RequestedAt < request.EndDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "amount" => descending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.RequestedAt) : query.OrderBy(x => x.RequestedAt)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new ReceivableWriteOffRowResponse
            {
                Id = x.Id,
                WriteOffNumber = x.WriteOffNumber,
                ReceivableId = x.ReceivableId,
                ReceivableNumber = x.Receivable!.ReceivableNumber,
                DebtorReferenceId = x.Receivable.DebtorReferenceId,
                Amount = x.Amount,
                Reason = x.Reason,
                Status = x.Status,
                RequestedAt = x.RequestedAt,
                DecidedAt = x.ApprovedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ReceivableWriteOffRowResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<FinReceivableWriteOff> RequestWriteOffAsync(
        Guid receivableId, decimal amount, string reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        ValidateAmount(amount);
        var reasonText = ValidateReason(reason, "Alasan penghapusan");

        var receivable = await FindReceivableAsync(receivableId, cancellationToken);
        // FIN-VAL-014: piutang lunas tidak boleh dihapusbukukan (diperiksa pada permintaan,
        // sesuai endpoint yang dirujuk kontrak: POST /receivables/{id}/write-offs).
        if (receivable.Status == FinReceivableStatuses.Settled)
            throw new ReceivableValidationException("Piutang yang sudah lunas tidak dapat dihapusbukukan.");

        var writeOff = new FinReceivableWriteOff
        {
            WriteOffNumber = GenerateNumber("WO"),
            ReceivableId = receivableId,
            Amount = amount,
            Reason = reasonText,
            Status = FinReceivableApprovalStatuses.Requested,
            RequestedBy = actorUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.FinReceivableWriteOffs.Add(writeOff);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("WriteOff.Request", writeOff.Id, receivableId, actorUserId);
        return writeOff;
    }

    public Task<FinReceivableWriteOff> ApproveWriteOffAsync(
        Guid writeOffId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideWriteOffAsync(writeOffId, expectedRowVersion, actorUserId, approve: true, rejectionReason: null, cancellationToken);

    public Task<FinReceivableWriteOff> RejectWriteOffAsync(
        Guid writeOffId, Guid expectedRowVersion, string rejectionReason, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideWriteOffAsync(writeOffId, expectedRowVersion, actorUserId, approve: false,
            ValidateReason(rejectionReason, "Alasan penolakan"), cancellationToken);

    private async Task<FinReceivableWriteOff> DecideWriteOffAsync(
        Guid writeOffId, Guid expectedRowVersion, Guid actorUserId, bool approve, string? rejectionReason, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            var writeOff = await _dbContext.FinReceivableWriteOffs
                .SingleOrDefaultAsync(x => x.Id == writeOffId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pengajuan penghapusan piutang tidak ditemukan.");
            EnsureCurrent(writeOff.RowVersion, expectedRowVersion);

            if (writeOff.Status != FinReceivableApprovalStatuses.Requested)
                throw new ReceivableValidationException("Permohonan ini sudah diputuskan sebelumnya.");
            if (actorUserId == writeOff.RequestedBy)
                throw new ReceivableValidationException(
                    "Pengaju tidak boleh menyetujui permohonannya sendiri. Mintalah persetujuan pengguna lain yang berwenang.");

            await AcquireLockAsync($"FIN_RECEIVABLE_{writeOff.ReceivableId:N}", cancellationToken);
            var receivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => x.Id == writeOff.ReceivableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

            var balanceBefore = receivable.OutstandingAmount;

            if (approve)
            {
                if (receivable.Status == FinReceivableStatuses.Settled)
                    throw new ReceivableValidationException("Piutang yang sudah lunas tidak dapat dihapusbukukan.");
                if (writeOff.Amount > receivable.OutstandingAmount)
                    throw new ReceivableValidationException(
                        $"Nilai penghapusan melebihi sisa piutang. Sisa saat ini Rp {receivable.OutstandingAmount:N0}.");

                // state-transition-matrix.md §2: OUTSTANDING atau PARTIAL -> WRITTEN_OFF pada setiap
                // penghapusan yang disetujui, penuh maupun sebagian — tidak bersyarat sisa jadi nol.
                // Dicatat apa adanya sesuai kontrak terkunci; lihat laporan BE-FIN-008 untuk catatan
                // bahwa ini terasa ganjil untuk penghapusan sebagian dan perlu diratifikasi ulang.
                receivable.OutstandingAmount -= writeOff.Amount;
                receivable.WrittenOffAmount += writeOff.Amount;
                receivable.Status = FinReceivableStatuses.WrittenOff;
                receivable.UpdateDateTime = DateTime.UtcNow;
                receivable.UpdateBy = actorUserId;
                receivable.RowVersion = Guid.NewGuid();

                writeOff.Status = FinReceivableApprovalStatuses.Approved;
            }
            else
            {
                writeOff.Status = FinReceivableApprovalStatuses.Rejected;
                writeOff.RejectionReason = rejectionReason;
            }
            writeOff.ApprovedBy = actorUserId;
            writeOff.ApprovedAt = DateTimeOffset.UtcNow;
            writeOff.UpdateDateTime = DateTime.UtcNow;
            writeOff.UpdateBy = actorUserId;
            writeOff.RowVersion = Guid.NewGuid();

            // BE-FIN-011: kejadian hanya lahir dari penghapusan yang DISETUJUI (katalog 17 kode,
            // "Penghapusan piutang disetujui") — penolakan tidak mengubah apa pun secara finansial.
            if (approve)
            {
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PemutihanPiutang,
                    SourceTransactionId = receivable.ReceivableNumber,
                    EventOccurredAt = writeOff.ApprovedAt!.Value,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(writeOff.ApprovedAt!.Value),
                    Amount = writeOff.Amount,
                    CorrelationId = receivable.CorrelationId,
                    CausationId = writeOff.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);

                // BE-FIN-060, FIN-DES-079: Mutasi subledger PENGHAPUSAN piutang disetujui (Jalur 6A)
                await _subledgerMovementService.RecordReceivableMovementAsync(
                    receivable: receivable,
                    movementType: FinReceivableMovementTypes.Penghapusan,
                    deltaAmount: -writeOff.Amount,
                    balanceBefore: balanceBefore,
                    occurredAt: writeOff.ApprovedAt!.Value,
                    actorUserId: actorUserId,
                    correlationId: receivable.CorrelationId,
                    causationId: writeOff.Id,
                    referenceNumber: writeOff.WriteOffNumber,
                    notes: writeOff.Reason,
                    cancellationToken: cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync(approve ? "WriteOff.Approve" : "WriteOff.Reject", writeOff.Id, writeOff.ReceivableId, actorUserId);
            return writeOff;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
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
    // Alokasi penerimaan (BE-FIN-018, FR-FIN-040..042/045, state-transition-matrix.md §2) —
    // dipanggil OLEH FinanceReceiptService.AllocateAsync/ReverseAllocationAsync, BUKAN lewat
    // controller sendiri. Kedua method ini MUST NOT membuka/commit/rollback transaksi sendiri —
    // pemanggil sudah berada di dalam transaksi Serializable miliknya sendiri (pola yang sama
    // dengan FinanceAccountingOutboxService.StageEventAsync). Ini SATU-SATUNYA titik selain
    // Decide*Async di atas yang menulis FinReceivable.OutstandingAmount — menjaga invariant
    // "satu-satunya penulis" tetap benar meski dipanggil dari service lain (Collection), karena
    // pemanggil TIDAK PERNAH menulis kolom FinReceivable secara langsung, hanya lewat sini.
    // ------------------------------------------------------------------------------------

    /// <summary>FR-FIN-042: alokasi tidak boleh melebihi sisa piutang.</summary>
    public async Task<FinReceivable> ApplyAllocationAsync(
        Guid receivableId,
        decimal amount,
        Guid actorUserId,
        CancellationToken cancellationToken,
        string movementType = FinReceivableMovementTypes.AlokasiPenerimaan,
        Guid? sourceAllocationId = null,
        string? referenceNumber = null,
        string? notes = null)
    {
        var receivable = await _dbContext.FinReceivables
            .SingleOrDefaultAsync(x => x.Id == receivableId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

        // state-transition-matrix.md §2: WRITTEN_OFF/CANCELLED adalah status akhir bagi alokasi.
        if (receivable.Status is FinReceivableStatuses.WrittenOff or FinReceivableStatuses.Cancelled)
            throw new ReceivableValidationException(
                $"Piutang berstatus {receivable.Status} tidak dapat menerima alokasi penerimaan.");
        if (amount > receivable.OutstandingAmount)
            throw new ReceivableValidationException(
                $"Alokasi melebihi sisa piutang. Sisa saat ini Rp {receivable.OutstandingAmount:N0}.");

        var balanceBefore = receivable.OutstandingAmount;
        var now = DateTimeOffset.UtcNow;

        receivable.OutstandingAmount -= amount;
        receivable.AllocatedAmount += amount;
        receivable.Status = receivable.OutstandingAmount == 0m ? FinReceivableStatuses.Settled : FinReceivableStatuses.Partial;
        receivable.UpdateDateTime = DateTime.UtcNow;
        receivable.UpdateBy = actorUserId;
        receivable.RowVersion = Guid.NewGuid();

        // BE-FIN-060, FIN-DES-079: Mutasi subledger alokasi penerimaan / potongan piutang (Jalur 2 & 4)
        await _subledgerMovementService.RecordReceivableMovementAsync(
            receivable: receivable,
            movementType: movementType,
            deltaAmount: -amount,
            balanceBefore: balanceBefore,
            occurredAt: now,
            actorUserId: actorUserId,
            correlationId: receivable.CorrelationId,
            causationId: sourceAllocationId ?? receivable.Id,
            sourceAllocationId: sourceAllocationId,
            referenceNumber: referenceNumber,
            notes: notes,
            cancellationToken: cancellationToken);

        return receivable;
    }

    /// <summary>
    /// FR-FIN-045: pembalikan tidak menghapus riwayat — dipanggil setelah pemanggil membuat baris
    /// FinReceiptAllocation pembalik sendiri (Collection tidak dikenal di sini). Method ini murni
    /// mengembalikan nilai piutang, persis kebalikan ApplyAllocationAsync.
    /// </summary>
    public async Task<FinReceivable> ReverseAllocationAsync(
        Guid receivableId,
        decimal amount,
        Guid actorUserId,
        CancellationToken cancellationToken,
        string movementType = FinReceivableMovementTypes.PembalikanAlokasi,
        Guid? sourceAllocationId = null,
        string? referenceNumber = null,
        string? notes = null)
    {
        var receivable = await _dbContext.FinReceivables
            .SingleOrDefaultAsync(x => x.Id == receivableId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

        var balanceBefore = receivable.OutstandingAmount;
        var now = DateTimeOffset.UtcNow;

        receivable.OutstandingAmount += amount;
        receivable.AllocatedAmount -= amount;
        // state-transition-matrix.md §2 baris 39/40: PARTIAL/SETTLED -> OUTSTANDING bila seluruh
        // alokasi sudah dibalik (AllocatedAmount kembali nol); selain itu tetap PARTIAL.
        receivable.Status = receivable.AllocatedAmount <= 0m
            ? FinReceivableStatuses.Outstanding
            : (receivable.OutstandingAmount <= 0m ? FinReceivableStatuses.Settled : FinReceivableStatuses.Partial);
        receivable.UpdateDateTime = DateTime.UtcNow;
        receivable.UpdateBy = actorUserId;
        receivable.RowVersion = Guid.NewGuid();

        // BE-FIN-060, FIN-DES-079: Mutasi subledger pembalikan alokasi / pembalikan potongan piutang (Jalur 3 & 4)
        await _subledgerMovementService.RecordReceivableMovementAsync(
            receivable: receivable,
            movementType: movementType,
            deltaAmount: +amount,
            balanceBefore: balanceBefore,
            occurredAt: now,
            actorUserId: actorUserId,
            correlationId: receivable.CorrelationId,
            causationId: sourceAllocationId ?? receivable.Id,
            sourceAllocationId: sourceAllocationId,
            referenceNumber: referenceNumber,
            notes: notes,
            cancellationToken: cancellationToken);

        return receivable;
    }

    // ------------------------------------------------------------------------------------
    // Endpoint V2: Pembayaran Langsung, Penghapusan Langsung, dan Laporan Agregat Piutang
    // ------------------------------------------------------------------------------------

    // BE-FIN-077, FIN-DES-085, FIN-DEC-126/132/134: metode, sumber dana, nomor rujukan, dan ProofId
    // kini WAJIB dan benar-benar tersimpan di baris mutasi — sebelumnya BankAccountId sudah diterima
    // RecordReceivablePaymentRequest tetapi tidak pernah diteruskan ke sini (temuan F1/F3,
    // 00-interview-decisions.md). PERUBAHAN MEMUTUS pada kontrak POST api/finance/receivable/payment
    // — lihat laporan task BE-FIN-077 bagian 7 untuk urutan rilis yang MUST dijaga bersama FE-FIN-030.
    public async Task<ReceivablePaymentResponse> RecordPaymentAsync(
        Guid receivableId,
        decimal amount,
        string paymentMethod,
        Guid? fundingSourceId,
        string? referenceNumber,
        Guid proofId,
        string? notes,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateAmount(amount);

        // FIN-VAL-199: PaymentMethod kosong atau di luar TRANSFER/CASH (400).
        var normalizedPaymentMethod = paymentMethod?.Trim().ToUpperInvariant();
        if (normalizedPaymentMethod is not ("CASH" or "TRANSFER"))
        {
            throw new ReceivableBadRequestException("Metode pembayaran wajib dipilih.");
        }

        // FIN-VAL-200: TRANSFER tanpa rekening sumber (422).
        if (normalizedPaymentMethod == "TRANSFER" && !fundingSourceId.HasValue)
        {
            throw new ReceivableValidationException("Rekening sumber dana wajib dipilih untuk pembayaran transfer.");
        }

        // FIN-VAL-201: CASH tetapi rekening bank diisi (400).
        if (normalizedPaymentMethod == "CASH" && fundingSourceId.HasValue)
        {
            throw new ReceivableBadRequestException("Pembayaran tunai tidak memakai rekening bank.");
        }

        // FIN-VAL-202: ProofId kosong (422).
        if (proofId == Guid.Empty)
        {
            throw new ReceivableValidationException("Bukti pembayaran wajib dilampirkan.");
        }

        // FIN-VAL-197: tanpa baris ambang aktif, jalur ini belum dapat dipakai — fail-closed (404),
        // BUKAN dianggap tak terbatas (FIN-DES-086).
        var threshold = await _dbContext.MstDirectPaymentThresholds.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException(
                "Ambang pembayaran langsung belum ditetapkan, sehingga jalur ini belum dapat dipakai.");

        // FIN-VAL-198: nominal melewati ambang aktif (422) — diarahkan ke jalur FinPayment berjenjang.
        if (amount > threshold.Amount)
        {
            throw new ReceivableValidationException(
                "Nominal melewati batas pembayaran langsung. Gunakan jalur pembayaran berjenjang.");
        }

        // FIN-VAL-223: ProofId tidak ditemukan, atau sudah bertanda IsDelete (404).
        var proofExists = await _dbContext.FinTransactionProofs.AsNoTracking()
            .AnyAsync(x => x.Id == proofId && !x.IsDelete, cancellationToken);
        if (!proofExists)
        {
            throw new KeyNotFoundException("Bukti tidak ditemukan. Unggah ulang buktinya.");
        }

        // FIN-VAL-203: ProofId sudah dipakai mutasi lain (409) — diperiksa di KEDUA tabel mutasi
        // (piutang dan utang supplier), karena FIN-DES-087 menetapkan satu bukti hanya untuk tepat
        // satu pembayaran, bukan hanya satu pembayaran per jenis buku. Pre-check praktik baik; jaring
        // pengaman sebenarnya tetap unique index IX_FinReceivableMovement_ProofId/IX_FinSupplierPayableMovement_ProofId.
        var proofAlreadyUsed =
            await _dbContext.FinReceivableMovements.AnyAsync(x => x.ProofId == proofId && !x.IsDelete, cancellationToken) ||
            await _dbContext.FinSupplierPayableMovements.AnyAsync(x => x.ProofId == proofId && !x.IsDelete, cancellationToken);
        if (proofAlreadyUsed)
        {
            throw new ReceivableConflictException("Bukti ini sudah dipakai pada pembayaran lain. Unggah bukti baru.");
        }

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_RECEIVABLE_{receivableId:N}", cancellationToken);

            var receivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => x.Id == receivableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

            if (receivable.Status is FinReceivableStatuses.WrittenOff or FinReceivableStatuses.Cancelled)
                throw new ReceivableValidationException($"Piutang berstatus {receivable.Status} tidak dapat menerima pembayaran.");

            if (amount > receivable.OutstandingAmount)
                throw new ReceivableValidationException($"Nilai pembayaran Rp {amount:N0} melebihi sisa piutang Rp {receivable.OutstandingAmount:N0}.");

            var prevOutstanding = receivable.OutstandingAmount;
            receivable.OutstandingAmount -= amount;
            receivable.AllocatedAmount += amount;
            receivable.Status = receivable.OutstandingAmount <= 0m ? FinReceivableStatuses.Settled : FinReceivableStatuses.Partial;
            receivable.UpdateDateTime = DateTime.UtcNow;
            receivable.UpdateBy = actorUserId;
            receivable.RowVersion = Guid.NewGuid();

            // Stage event PENERIMAAN-PIUTANG ke Accounting Integration Outbox
            var eventOccurredAt = DateTimeOffset.UtcNow;
            await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
            {
                EventTypeCode = FinAccountingEventTypeCodes.PenerimaanPiutang,
                SourceTransactionId = receivable.ReceivableNumber,
                EventOccurredAt = eventOccurredAt,
                AccountingDate = FinanceBusinessDate.ToDateOnly(eventOccurredAt),
                Amount = amount,
                CorrelationId = receivable.CorrelationId,
                CausationId = receivable.Id,
                ActorUserId = actorUserId
            }, cancellationToken);

            var fundingSourceType = normalizedPaymentMethod == "CASH" ? "CASH" : "BANK_ACCOUNT";

            // BE-FIN-060, FIN-DES-079: Mutasi subledger PEMBAYARAN-LANGSUNG piutang (Jalur 7).
            // BE-FIN-077, FIN-DES-085: fundingSourceId dan proofId kini diteruskan — RecordReceivableMovementAsync
            // sudah mendukung keduanya sejak BE-FIN-058, hanya belum pernah dipanggil dengan nilainya.
            var recMovement = await _subledgerMovementService.RecordReceivableMovementAsync(
                receivable: receivable,
                movementType: FinReceivableMovementTypes.PembayaranLangsung,
                deltaAmount: -amount,
                balanceBefore: prevOutstanding,
                occurredAt: eventOccurredAt,
                actorUserId: actorUserId,
                correlationId: receivable.CorrelationId,
                causationId: receivable.Id,
                paymentMethodCode: normalizedPaymentMethod,
                fundingSourceType: fundingSourceType,
                fundingSourceId: fundingSourceId,
                referenceNumber: referenceNumber,
                proofId: proofId,
                notes: notes,
                cancellationToken: cancellationToken);

            // BE-FIN-062, FIN-DES-081: Mutasi kas masuk PENERIMAAN-TUNAI-LANGSUNG (Sumber 2).
            // FIN-DES-085: anggaran kas kecil TIDAK PERNAH tersentuh jalur ini — hanya Kas Kasir.
            if (normalizedPaymentMethod == "CASH")
            {
                await _subledgerMovementService.RecordCashMovementAsync(
                    movementType: FinCashMovementTypes.PenerimaanTunaiLangsung,
                    direction: FinCashMovementDirections.In,
                    amount: amount,
                    businessDate: FinanceBusinessDate.ToDateOnly(eventOccurredAt),
                    occurredAt: eventOccurredAt,
                    sourceReferenceType: FinCashMovementSourceReferenceTypes.ReceivableMovement,
                    sourceReferenceId: recMovement.Id.ToString(),
                    actorUserId: actorUserId,
                    correlationId: receivable.CorrelationId,
                    causationId: receivable.Id,
                    paymentMethodCode: "CASH",
                    notes: notes,
                    cancellationToken: cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("RecordPayment", receivable.Id, receivable.Id, actorUserId);

            return new ReceivablePaymentResponse
            {
                ReceivableId = receivable.Id,
                ReceivableNumber = receivable.ReceivableNumber,
                PaymentAmount = amount,
                PreviousOutstanding = prevOutstanding,
                CurrentOutstanding = receivable.OutstandingAmount,
                TotalAllocated = receivable.AllocatedAmount,
                Status = receivable.Status,
                PaymentDate = eventOccurredAt.UtcDateTime,
                ReferenceNumber = referenceNumber,
                PaymentMethod = normalizedPaymentMethod,
                FundingSourceId = fundingSourceId,
                ProofId = proofId
            };
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
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

    public async Task<FinReceivableWriteOff> DirectWriteOffAsync(
        Guid receivableId,
        decimal amount,
        string reason,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateAmount(amount);
        var reasonText = ValidateReason(reason, "Alasan penghapusan");

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_RECEIVABLE_{receivableId:N}", cancellationToken);

            var receivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => x.Id == receivableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

            if (receivable.Status is FinReceivableStatuses.Settled or FinReceivableStatuses.WrittenOff or FinReceivableStatuses.Cancelled)
                throw new ReceivableValidationException($"Piutang berstatus {receivable.Status} tidak dapat dihapus.");

            if (amount > receivable.OutstandingAmount)
                throw new ReceivableValidationException($"Nominal penghapusan Rp {amount:N0} melebihi sisa piutang Rp {receivable.OutstandingAmount:N0}.");

            var writeOff = new FinReceivableWriteOff
            {
                ReceivableId = receivableId,
                WriteOffNumber = GenerateNumber("WO"),
                Amount = amount,
                Reason = reasonText,
                Status = FinReceivableApprovalStatuses.Approved,
                ApprovedBy = actorUserId,
                ApprovedAt = DateTimeOffset.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            var balanceBefore = receivable.OutstandingAmount;
            receivable.OutstandingAmount -= amount;
            receivable.WrittenOffAmount += amount;
            receivable.Status = receivable.OutstandingAmount <= 0m ? FinReceivableStatuses.WrittenOff : FinReceivableStatuses.Partial;
            receivable.UpdateDateTime = DateTime.UtcNow;
            receivable.UpdateBy = actorUserId;
            receivable.RowVersion = Guid.NewGuid();

            _dbContext.FinReceivableWriteOffs.Add(writeOff);

            // Stage event PEMUTIHAN-PIUTANG ke Accounting Outbox
            await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
            {
                EventTypeCode = FinAccountingEventTypeCodes.PemutihanPiutang,
                SourceTransactionId = receivable.ReceivableNumber,
                EventOccurredAt = writeOff.ApprovedAt.Value,
                AccountingDate = FinanceBusinessDate.ToDateOnly(writeOff.ApprovedAt.Value),
                Amount = writeOff.Amount,
                CorrelationId = receivable.CorrelationId,
                CausationId = writeOff.Id,
                ActorUserId = actorUserId
            }, cancellationToken);

            // BE-FIN-060, FIN-DES-079: Mutasi subledger PENGHAPUSAN piutang langsung (Jalur 6B)
            await _subledgerMovementService.RecordReceivableMovementAsync(
                receivable: receivable,
                movementType: FinReceivableMovementTypes.Penghapusan,
                deltaAmount: -amount,
                balanceBefore: balanceBefore,
                occurredAt: writeOff.ApprovedAt.Value,
                actorUserId: actorUserId,
                correlationId: receivable.CorrelationId,
                causationId: writeOff.Id,
                referenceNumber: writeOff.WriteOffNumber,
                notes: writeOff.Reason,
                cancellationToken: cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("DirectWriteOff", writeOff.Id, receivableId, actorUserId);
            return writeOff;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
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

    // BE-FIN-100 (FR-FIN-219, FIN-DEC-176): parameter debtorType opsional, pola yang sama dengan
    // GetAgingSummaryAsync/GetSummaryAsync — null/kosong berarti seluruh FinReceivable tanpa
    // saringan (perilaku bawaan sebelum task ini). Diteruskan juga ke GetAgingSummaryAsync di
    // bawah supaya AgingBuckets pada laporan tetap konsisten dengan saringan yang sama.
    public async Task<ReceivableReportResponse> GetReportSummaryAsync(
        DateOnly? asOfDate, CancellationToken cancellationToken, string? debtorType = null)
    {
        var date = asOfDate ?? FinanceBusinessDate.Today();
        var query = _dbContext.FinReceivables.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(debtorType)) query = query.Where(x => x.DebtorType == debtorType);
        var receivables = await query.ToListAsync(cancellationToken);

        var agingBuckets = await GetAgingSummaryAsync(date, cancellationToken, debtorType);

        var statusBreakdown = receivables
            .GroupBy(x => x.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var debtorBreakdown = receivables
            .GroupBy(x => string.IsNullOrWhiteSpace(x.DebtorType) ? "LAINNYA" : x.DebtorType)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.OutstandingAmount));

        return new ReceivableReportResponse
        {
            AsOfDate = date,
            TotalOriginalAmount = receivables.Sum(x => x.OriginalAmount),
            TotalOutstandingAmount = receivables.Sum(x => x.OutstandingAmount),
            TotalAllocatedAmount = receivables.Sum(x => x.AllocatedAmount),
            TotalAdjustedAmount = receivables.Sum(x => x.AdjustedAmount),
            TotalWrittenOffAmount = receivables.Sum(x => x.WrittenOffAmount),
            TotalReceivableCount = receivables.Count,
            StatusBreakdown = statusBreakdown,
            DebtorTypeBreakdown = debtorBreakdown,
            AgingBuckets = agingBuckets
        };
    }

    // ------------------------------------------------------------------------------------
    // Infrastruktur bersama
    // ------------------------------------------------------------------------------------

    private async Task<FinReceivable> FindReceivableAsync(Guid receivableId, CancellationToken cancellationToken) =>
        await _dbContext.FinReceivables.AsNoTracking().SingleOrDefaultAsync(x => x.Id == receivableId && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

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

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion) throw Stale();
    }

    private static ReceivableConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private static string ValidateDirection(string? direction)
    {
        var normalized = (direction ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized is not (FinReceivableAdjustmentDirections.Debit or FinReceivableAdjustmentDirections.Credit))
            throw new ReceivableBadRequestException("Arah koreksi harus DEBIT atau CREDIT.");
        return normalized;
    }

    // FIN-VAL-023 (dan analoginya untuk write-off): nominal harus lebih dari nol.
    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0) throw new ReceivableBadRequestException("Nominal harus lebih dari nol.");
    }

    // FIN-VAL-021/022: alasan wajib diisi.
    private static string ValidateReason(string? value, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ReceivableBadRequestException($"{label} wajib diisi.");
        var trimmed = value.Trim();
        if (trimmed.Length > 500) throw new ReceivableBadRequestException($"{label} maksimal 500 karakter.");
        return trimmed;
    }

    // Nomor koreksi/penghapusan tidak punya format baku pada kontrak yang terkunci (berbeda
    // dari ReceivableNumber yang punya contoh "AR-2026-09-00871"); dibuat unik lewat Guid,
    // bukan Count/Max/Last+1 (QBE-CODE-002/003), sampai ada keputusan skema penomoran resmi.
    private static string GenerateNumber(string prefix)
    {
        var candidate = $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    // Privasi: Reason/RejectionReason bisa memuat keterangan bebas dan MUST NOT masuk log
    // aplikasi, mengikuti pola PettyCashVoucherService.
    private Task AuditAsync(string action, Guid entityId, Guid receivableId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceReceivable.{action}",
            $"Perubahan piutang dicatat. EntityId={entityId} ReceivableId={receivableId}",
            new { EntityId = entityId, ReceivableId = receivableId, ActorUserId = actorUserId });

    /// <summary>
    /// Membaca buku mutasi satu piutang secara berpaging dan tersaring (BE-FIN-063, FR-FIN-140, FIN-API-1.5 F.5).
    /// </summary>
    public async Task<PagedResult<ReceivableMovementResponse>> GetMovementsPagedAsync(
        Guid receivableId, ReceivableMovementQuery query, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.FinReceivables.AsNoTracking()
            .AnyAsync(x => x.Id == receivableId && !x.IsDelete, cancellationToken);
        if (!exists) throw new KeyNotFoundException($"Piutang dengan ID '{receivableId}' tidak ditemukan.");

        var q = _dbContext.FinReceivableMovements.AsNoTracking()
            .Where(x => x.ReceivableId == receivableId && !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(query.MovementType))
        {
            var movementType = query.MovementType.Trim().ToUpperInvariant();
            q = q.Where(x => x.MovementType == movementType);
        }

        if (query.DateFrom.HasValue)
            q = q.Where(x => x.BusinessDate >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            q = q.Where(x => x.BusinessDate <= query.DateTo.Value);

        var total = await q.CountAsync(cancellationToken);

        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        q = descending
            ? q.OrderByDescending(x => x.BusinessDate).ThenByDescending(x => x.OccurredAt).ThenByDescending(x => x.CreateDateTime)
            : q.OrderBy(x => x.BusinessDate).ThenBy(x => x.OccurredAt).ThenBy(x => x.CreateDateTime);

        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 25 : query.PageSize;

        var items = await q
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ReceivableMovementResponse
            {
                Id = x.Id,
                ReceivableId = x.ReceivableId,
                MovementType = x.MovementType,
                Amount = x.Amount,
                BalanceBefore = x.BalanceBefore,
                BalanceAfter = x.BalanceAfter,
                BusinessDate = x.BusinessDate,
                OccurredAt = x.OccurredAt,
                SourceAllocationId = x.SourceAllocationId,
                PaymentMethodCode = x.PaymentMethodCode,
                FundingSourceType = x.FundingSourceType,
                FundingSourceId = x.FundingSourceId,
                ReferenceNumber = x.ReferenceNumber,
                ProofId = x.ProofId,
                OpeningItemBatchId = x.OpeningItemBatchId,
                Notes = x.Notes,
                CorrelationId = x.CorrelationId,
                CausationId = x.CausationId
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ReceivableMovementResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }
}

/// <summary>Nama kelompok umur tetap sesuai FIN-DEC-010 — bukan enum karena dipakai sebagai label tampilan langsung.</summary>
public static class ReceivableAgingBuckets
{
    public const string Days0To30 = "0-30";
    public const string Days31To60 = "31-60";
    public const string Days61To90 = "61-90";
    public const string Over90Days = "di atas 90 hari";

    public static readonly string[] Labels = [Days0To30, Days31To60, Days61To90, Over90Days];

    public static string Resolve(int daysPastDue) => daysPastDue switch
    {
        <= 30 => Days0To30,
        <= 60 => Days31To60,
        <= 90 => Days61To90,
        _ => Over90Days
    };
}

public sealed class ReceivableAgingBucketResult
{
    public string BucketLabel { get; set; } = string.Empty;
    public int ReceivableCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
}

public sealed class ReceivableBadRequestException(string message) : Exception(message);
public sealed class ReceivableValidationException(string message) : Exception(message);
public sealed class ReceivableConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
