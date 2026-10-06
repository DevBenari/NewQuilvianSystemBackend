using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

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

    public FinanceReceivableInvoiceBatchService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        BillingCompanyGuarantorInvoiceDocumentService documentService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _documentService = documentService;
    }

    // ------------------------------------------------------------------------------------
    // Daftar batch — GET /
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<ReceivableInvoiceBatchResponse>> GetPagedAsync(
        ReceivableInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking().Where(x => !x.IsDelete);

        if (request.DebtorReferenceId.HasValue)
            query = query.Where(x => x.DebtorReferenceId == request.DebtorReferenceId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "batchnumber" => descending ? query.OrderByDescending(x => x.BatchNumber) : query.OrderBy(x => x.BatchNumber),
            "totalamount" => descending ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);

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
        return new ReceivableInvoiceBatchDetailResponse
        {
            Id = mapped.Id,
            BatchNumber = mapped.BatchNumber,
            DebtorType = mapped.DebtorType,
            DebtorReferenceId = mapped.DebtorReferenceId,
            PeriodStart = mapped.PeriodStart,
            PeriodEnd = mapped.PeriodEnd,
            InvoiceDate = mapped.InvoiceDate,
            DueDate = mapped.DueDate,
            PaymentTermDays = mapped.PaymentTermDays,
            Note = mapped.Note,
            TotalAmount = mapped.TotalAmount,
            Status = mapped.Status,
            IssuedAt = mapped.IssuedAt,
            RowVersion = mapped.RowVersion,
            ClaimStatus = mapped.ClaimStatus,
            ApprovedAmount = mapped.ApprovedAmount,
            ClaimVarianceAmount = mapped.ClaimVarianceAmount,
            PayerClaimReference = mapped.PayerClaimReference,
            ClaimNote = mapped.ClaimNote,
            PayerVerifiedAt = mapped.PayerVerifiedAt,
            ClaimApprovedAt = mapped.ClaimApprovedAt,
            ClaimClosedAt = mapped.ClaimClosedAt,
            Members = batch.Items
                .Where(x => !x.IsDelete && x.Receivable is not null)
                .Select(x => new ReceivableInvoiceBatchMemberResponse
                {
                    Id = x.Id,
                    ReceivableId = x.ReceivableId,
                    ReceivableNumber = x.Receivable!.ReceivableNumber,
                    InvoiceId = x.Receivable.InvoiceId,
                    OriginalAmount = x.Receivable.OriginalAmount,
                    OutstandingAmount = x.Receivable.OutstandingAmount,
                    ReceivableStatus = x.Receivable.Status
                })
                .ToList()
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
            .Select(x => new { x.Id, x.CompanyGuarantorName, x.PaymentDueDays })
            .FirstOrDefaultAsync(cancellationToken);

        string payerName;
        string payerKind;
        int? paymentTermDays = null;
        string dueDateSource;

        if (company != null)
        {
            payerName = company.CompanyGuarantorName;
            payerKind = BillingDataPayerKinds.Company;
            paymentTermDays = company.PaymentDueDays;
            dueDateSource = ReceivableDueDateSources.CompanyGuarantorTerm;
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

                // Asuransi belum memiliki kolom PaymentDueDays di master data.
                // Audit apakah piutang aktif milik penjamin ini memiliki DueDate yang dapat ditarik selisihnya terhadap tanggal pengakuan.
                var sampleReceivable = await _dbContext.FinReceivables.AsNoTracking()
                    .Where(x => !x.IsDelete
                        && x.DebtorType == FinReceivableDebtorTypes.Payer
                        && x.DebtorReferenceId == debtorReferenceId)
                    .OrderByDescending(x => x.RecognizedAt)
                    .Select(x => new { x.RecognizedAt, x.DueDate })
                    .FirstOrDefaultAsync(cancellationToken);

                if (sampleReceivable != null)
                {
                    var recDate = FinanceBusinessDate.ToDateOnly(sampleReceivable.RecognizedAt);
                    var days = sampleReceivable.DueDate.DayNumber - recDate.DayNumber;
                    if (days > 0)
                    {
                        paymentTermDays = days;
                        dueDateSource = ReceivableDueDateSources.ReceivableDueDate;
                    }
                    else
                    {
                        dueDateSource = ReceivableDueDateSources.NotConfigured;
                    }
                }
                else
                {
                    dueDateSource = ReceivableDueDateSources.NotConfigured;
                }
            }
            else
            {
                throw new KeyNotFoundException($"Penjamin dengan Id '{debtorReferenceId}' tidak ditemukan.");
            }
        }

        var defaultInvoiceDate = FinanceBusinessDate.Today();
        DateOnly? dueDatePreview = paymentTermDays.HasValue
            ? defaultInvoiceDate.AddDays(paymentTermDays.Value)
            : null;

        return new ReceivableInvoiceBatchCreateContextResponse
        {
            DebtorReferenceId = debtorReferenceId,
            PayerName = payerName,
            PayerKind = payerKind,
            DefaultInvoiceDate = defaultInvoiceDate,
            PaymentTermDays = paymentTermDays,
            DueDatePreview = dueDatePreview,
            DueDateSource = dueDateSource,
            IsTermConfigured = paymentTermDays.HasValue
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
            .Where(x => !x.IsDelete && x.Batch!.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
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
                && x.Batch!.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            .Select(x => x.ReceivableId)
            .ToListAsync(cancellationToken);
        if (alreadyBatched.Count > 0)
            throw new ReceivableInvoiceBatchConflictException(
                $"Piutang {string.Join(", ", alreadyBatched)} sudah tergabung dalam batch tagihan lain.");

        // Batch yang dibatalkan tidak menghapus baris anggotanya, dan indeks unik
        // IX_FinReceivableInvoiceBatchItem_ActiveReceivable (IsDelete = false) tetap menahan piutang itu.
        // Dikenali di sini supaya pengguna mendapat penjelasan yang benar, bukan kesalahan database.
        var stuckInCancelled = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
            .Where(x => !x.IsDelete
                && distinctIds.Contains(x.ReceivableId)
                && x.Batch!.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
            .Select(x => x.Receivable!.ReceivableNumber)
            .ToListAsync(cancellationToken);
        if (stuckInCancelled.Count > 0)
            throw new ReceivableInvoiceBatchConflictException(
                $"Piutang {string.Join(", ", stuckInCancelled)} masih tercatat pada batch tagihan yang sudah dibatalkan, sehingga belum dapat digabung ulang. Pembebasan piutang dari batch yang dibatalkan belum tersedia.");

        // Snapshot authoritative Terms of Payment dan kalkulasi Due Date
        int? paymentTermDays = null;
        var companyTerm = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == debtorReferenceId && !x.IsDelete)
            .Select(x => (int?)x.PaymentDueDays)
            .FirstOrDefaultAsync(cancellationToken);

        if (companyTerm.HasValue)
        {
            paymentTermDays = companyTerm.Value;
        }
        else
        {
            var sampleReceivable = receivables.FirstOrDefault();
            if (sampleReceivable != null)
            {
                var recDate = FinanceBusinessDate.ToDateOnly(sampleReceivable.RecognizedAt);
                var days = sampleReceivable.DueDate.DayNumber - recDate.DayNumber;
                if (days > 0)
                {
                    paymentTermDays = days;
                }
            }
        }

        var effectiveInvoiceDate = invoiceDate ?? FinanceBusinessDate.Today();
        DateOnly? batchDueDate = paymentTermDays.HasValue
            ? effectiveInvoiceDate.AddDays(paymentTermDays.Value)
            : null;

        var batch = new FinReceivableInvoiceBatch
        {
            Id = Guid.NewGuid(),
            BatchNumber = GenerateBatchNumber(),
            DebtorType = FinReceivableDebtorTypes.Payer,
            DebtorReferenceId = debtorReferenceId,
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

    public async Task<FinReceivableInvoiceBatch> CancelAsync(
        Guid batchId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Draft)
            throw new ReceivableInvoiceBatchValidationException($"Hanya batch berstatus DRAFT yang dapat dibatalkan. Status saat ini: {batch.Status}.");

        batch.Status = FinReceivableInvoiceBatchStatuses.Cancelled;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.CancelDateTime = DateTime.UtcNow;
        batch.CancelBy = actorUserId;
        batch.IsCancel = true;
        batch.RowVersion = Guid.NewGuid();

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

        var response = new ReceivableInvoiceBatchDocumentResponse
        {
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            DebtorReferenceId = batch.DebtorReferenceId,
            PeriodStart = batch.PeriodStart,
            PeriodEnd = batch.PeriodEnd
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

    private static ReceivableInvoiceBatchResponse Map(FinReceivableInvoiceBatch batch) => new()
    {
        Id = batch.Id,
        BatchNumber = batch.BatchNumber,
        DebtorType = batch.DebtorType,
        DebtorReferenceId = batch.DebtorReferenceId,
        PeriodStart = batch.PeriodStart,
        PeriodEnd = batch.PeriodEnd,
        InvoiceDate = batch.InvoiceDate,
        DueDate = batch.DueDate,
        PaymentTermDays = batch.PaymentTermDays,
        Note = batch.Note,
        TotalAmount = batch.TotalAmount,
        Status = batch.Status,
        IssuedAt = batch.IssuedAt,
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

    private static string GenerateBatchNumber()
    {
        // KNOWN ISSUE (bersama seluruh generator nomor rumpun ini): belum memakai provider
        // number-series atomik (QBE-CODE-001..006).
        var candidate = $"BAR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
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
}

public sealed class ReceivableInvoiceBatchBadRequestException(string message) : Exception(message);
public sealed class ReceivableInvoiceBatchValidationException(string message) : Exception(message);
public sealed class ReceivableInvoiceBatchConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
