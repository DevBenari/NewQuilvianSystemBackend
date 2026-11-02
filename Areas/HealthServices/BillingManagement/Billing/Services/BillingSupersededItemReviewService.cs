using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Layanan pengelolaan peninjauan void biaya administrasi yang keliru (BE-BKC-088, BKC-DEC-158, BKC-DEC-162, BKC-DES-089).
/// </summary>
public sealed class BillingSupersededItemReviewService : IBillingSupersededItemReviewService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private const string SupersededVoidReason = "SUPERSEDED_BY_INPATIENT_ADMISSION";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly BillingCalculationService? _calculationService;

    /// <param name="calculationService">
    /// Mesin kalkulasi untuk memperbarui versi kalkulasi saat item dipulihkan (REINSTATE). Opsional agar konstruksi
    /// lama tetap sah; DI selalu menyediakannya. Bila kosong, perhitungan tidak diperbarui di sini dan invoice
    /// terbaca "belum terkini" oleh penjaga finalisasi — tidak pernah terbaca terkini secara keliru.
    /// </param>
    public BillingSupersededItemReviewService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        BillingCalculationService? calculationService = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        _calculationService = calculationService;
    }

    /// <inheritdoc />
    public async Task<PagedResult<SupersededItemReviewCandidateResponse>> GetCandidatesAsync(
        SupersededItemReviewListQuery query,
        CancellationToken cancellationToken = default)
    {
        query ??= new SupersededItemReviewListQuery();
        var pageNumber = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 20 : query.PageSize, 1, 100);

        var reviewState = (query.ReviewState ?? "PENDING").Trim().ToUpperInvariant();

        // 1. Query dasar kandidat item: VOIDED dengan kode SUPERSEDED_BY_INPATIENT_ADMISSION
        var baseQuery = _dbContext.BilInvoiceItems.AsNoTracking()
            .Include(x => x.Invoice)
            .Include(x => x.Category)
            .Where(x => !x.IsDelete
                && x.Status == BillingInvoiceItemStatuses.Voided
                && x.VoidReason == SupersededVoidReason
                && !x.Invoice.IsDelete);

        // 2. Filter status invoice
        if (!string.IsNullOrWhiteSpace(query.InvoiceStatus))
        {
            var invoiceStatus = query.InvoiceStatus.Trim().ToUpperInvariant();
            baseQuery = baseQuery.Where(x => x.Invoice.Status == invoiceStatus);
        }

        // 3. Filter nomor invoice
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(x => x.Invoice.InvoiceNumber.Contains(search));
        }

        // 4. Filter status tinjauan (PENDING vs DECIDED vs ALL)
        if (reviewState == "PENDING")
        {
            baseQuery = baseQuery.Where(item => !_dbContext.BilSupersededItemReviews
                .Any(r => r.InvoiceItemId == item.Id && !r.IsDelete));
        }
        else if (reviewState == "DECIDED")
        {
            baseQuery = baseQuery.Where(item => _dbContext.BilSupersededItemReviews
                .Any(r => r.InvoiceItemId == item.Id && !r.IsDelete));
        }

        var totalData = await baseQuery.CountAsync(cancellationToken);
        var totalPage = (int)Math.Ceiling(totalData / (double)pageSize);

        var pagedItems = await baseQuery
            .OrderByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (pagedItems.Count == 0)
        {
            return new PagedResult<SupersededItemReviewCandidateResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = totalPage,
                Items = []
            };
        }

        // 5. Muat data pendukung batch: Encounter, Pasien, Tautan Transfer, dan Review
        var itemIds = pagedItems.Select(x => x.Id).ToList();
        var invoiceIds = pagedItems.Select(x => x.InvoiceId).Distinct().ToList();
        var encounterIds = pagedItems.Select(x => x.Invoice.EncounterId).Distinct().ToList();

        // 5a. Kunjungan & Pasien
        var encounterPatientMap = await (
            from encounter in _dbContext.RegPatientEncounters.AsNoTracking()
            where encounterIds.Contains(encounter.Id) && !encounter.IsDelete
            join patient in _dbContext.MstPatients.AsNoTracking()
                on encounter.PatientId equals patient.Id into pg
            from patient in pg.DefaultIfEmpty()
            select new
            {
                EncounterId = encounter.Id,
                EncounterDate = (DateTime?)encounter.EncounterDate,
                PatientName = patient != null ? patient.FullName : string.Empty,
                MedicalRecordNumber = patient != null ? patient.MedicalRecordNumber : string.Empty
            }
        ).ToDictionaryAsync(x => x.EncounterId, cancellationToken);

        // 5b. Tautan Transfer Aktif ke Rawat Inap
        var linkedEncounterSet = (await _dbContext.BilInvoiceEncounterLinks.AsNoTracking()
            .Where(l => encounterIds.Contains(l.LinkedEncounterId) && !l.IsDelete && !l.IsCancel)
            .Select(l => l.LinkedEncounterId)
            .Distinct()
            .ToListAsync(cancellationToken))
            .ToHashSet();

        // 5c. Review yang sudah ada
        var existingReviews = await _dbContext.BilSupersededItemReviews.AsNoTracking()
            .Where(r => itemIds.Contains(r.InvoiceItemId) && !r.IsDelete)
            .ToListAsync(cancellationToken);

        var reviewerIds = existingReviews.Select(r => r.DecidedBy).Distinct().ToList();
        var userNamesMap = reviewerIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.Set<ApplicationUser>().AsNoTracking()
                .Where(u => reviewerIds.Contains(u.Id))
                .ToDictionaryAsync(
                    u => u.Id,
                    u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : (u.UserName ?? u.Id.ToString()),
                    cancellationToken);

        var reviewMap = existingReviews.ToDictionary(
            r => r.InvoiceItemId,
            r => MapReviewResponse(r, userNamesMap.TryGetValue(r.DecidedBy, out var name) ? name : r.DecidedBy.ToString(), isReplay: false));

        // 6. Proyeksikan respons per item
        var candidateResponses = new List<SupersededItemReviewCandidateResponse>(pagedItems.Count);
        foreach (var item in pagedItems)
        {
            encounterPatientMap.TryGetValue(item.Invoice.EncounterId, out var encPatient);
            var isLinked = linkedEncounterSet.Contains(item.Invoice.EncounterId);
            reviewMap.TryGetValue(item.Id, out var reviewResponse);

            var allowedDecisions = ResolveAllowedDecisions(item.Invoice.Status);

            candidateResponses.Add(new SupersededItemReviewCandidateResponse
            {
                InvoiceItemId = item.Id,
                InvoiceId = item.InvoiceId,
                InvoiceNumber = item.Invoice.InvoiceNumber,
                InvoiceStatus = item.Invoice.Status,
                InvoiceRowVersion = item.Invoice.RowVersion,
                EncounterId = item.Invoice.EncounterId,
                EncounterDate = encPatient?.EncounterDate,
                PatientName = encPatient?.PatientName ?? string.Empty,
                MedicalRecordNumber = encPatient?.MedicalRecordNumber ?? string.Empty,
                ItemDescription = item.DescriptionSnapshot,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = item.Quantity * item.UnitPrice,
                VoidedAt = item.UpdateDateTime.HasValue ? new DateTimeOffset(item.UpdateDateTime.Value, TimeSpan.Zero) : null,
                IsLinkedToInpatient = isLinked,
                IsAdministrationFeeCategory = item.Category?.IsAdministrationFee ?? false,
                AllowedDecisions = allowedDecisions,
                Review = reviewResponse
            });
        }

        return new PagedResult<SupersededItemReviewCandidateResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = totalPage,
            Items = candidateResponses
        };
    }

    /// <inheritdoc />
    public async Task<SupersededItemReviewCandidateResponse> GetCandidateByIdAsync(
        Guid invoiceItemId,
        CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.BilInvoiceItems.AsNoTracking()
            .Include(x => x.Invoice)
            .Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.Id == invoiceItemId && !x.IsDelete, cancellationToken);

        // BIL-VAL-152: Item harus VOIDED dengan VoidReason = 'SUPERSEDED_BY_INPATIENT_ADMISSION' dan tidak dihapus
        if (item is null || item.Status != BillingInvoiceItemStatuses.Voided || item.VoidReason != SupersededVoidReason || item.Invoice.IsDelete)
        {
            throw new KeyNotFoundException("Item tidak termasuk daftar tinjauan void biaya administrasi.");
        }

        var encPatient = await (
            from encounter in _dbContext.RegPatientEncounters.AsNoTracking()
            where encounter.Id == item.Invoice.EncounterId && !encounter.IsDelete
            join patient in _dbContext.MstPatients.AsNoTracking()
                on encounter.PatientId equals patient.Id into pg
            from patient in pg.DefaultIfEmpty()
            select new
            {
                EncounterDate = (DateTime?)encounter.EncounterDate,
                PatientName = patient != null ? patient.FullName : string.Empty,
                MedicalRecordNumber = patient != null ? patient.MedicalRecordNumber : string.Empty
            }
        ).FirstOrDefaultAsync(cancellationToken);

        var isLinked = await _dbContext.BilInvoiceEncounterLinks.AsNoTracking()
            .AnyAsync(l => l.LinkedEncounterId == item.Invoice.EncounterId && !l.IsDelete && !l.IsCancel, cancellationToken);

        var review = await _dbContext.BilSupersededItemReviews.AsNoTracking()
            .SingleOrDefaultAsync(r => r.InvoiceItemId == item.Id && !r.IsDelete, cancellationToken);

        SupersededItemReviewResponse? reviewResponse = null;
        if (review is not null)
        {
            var reviewer = await _dbContext.Set<ApplicationUser>().AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == review.DecidedBy, cancellationToken);
            var reviewerName = reviewer != null && !string.IsNullOrWhiteSpace(reviewer.DisplayName)
                ? reviewer.DisplayName
                : (reviewer?.UserName ?? review.DecidedBy.ToString());
            reviewResponse = MapReviewResponse(review, reviewerName, isReplay: false);
        }

        return new SupersededItemReviewCandidateResponse
        {
            InvoiceItemId = item.Id,
            InvoiceId = item.InvoiceId,
            InvoiceNumber = item.Invoice.InvoiceNumber,
            InvoiceStatus = item.Invoice.Status,
            InvoiceRowVersion = item.Invoice.RowVersion,
            EncounterId = item.Invoice.EncounterId,
            EncounterDate = encPatient?.EncounterDate,
            PatientName = encPatient?.PatientName ?? string.Empty,
            MedicalRecordNumber = encPatient?.MedicalRecordNumber ?? string.Empty,
            ItemDescription = item.DescriptionSnapshot,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Amount = item.Quantity * item.UnitPrice,
            VoidedAt = item.UpdateDateTime.HasValue ? new DateTimeOffset(item.UpdateDateTime.Value, TimeSpan.Zero) : null,
            IsLinkedToInpatient = isLinked,
            IsAdministrationFeeCategory = item.Category?.IsAdministrationFee ?? false,
            AllowedDecisions = ResolveAllowedDecisions(item.Invoice.Status),
            Review = reviewResponse
        };
    }

    /// <inheritdoc />
    public async Task<SupersededItemReviewResponse> DecideAsync(
        Guid invoiceItemId,
        Guid idempotencyKey,
        DecideSupersededItemRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        // 1. Validasi masukan dasar
        if (idempotencyKey == Guid.Empty)
        {
            throw new BillingSupersededItemReviewBadRequestException("Idempotency-Key wajib diisi.");
        }

        if (request is null)
        {
            throw new BillingSupersededItemReviewBadRequestException("Data permohonan keputusan tidak boleh kosong.");
        }

        var decision = (request.Decision ?? string.Empty).Trim().ToUpperInvariant();
        if (!BilSupersededItemReviewDecisions.All.Contains(decision))
        {
            throw new BillingSupersededItemReviewBadRequestException(
                "Keputusan tidak dikenal; gunakan REINSTATE, KEEP, atau NEEDS_POST_CLOSE_CORRECTION.");
        }

        var trimmedReason = (request.Reason ?? string.Empty).Trim();
        // BIL-VAL-151: Alasan wajib diisi minimal 10 karakter, maksimal 500 karakter sesudah dipangkas
        if (trimmedReason.Length < 10)
        {
            throw new BillingSupersededItemReviewBadRequestException("Alasan wajib diisi minimal 10 karakter.");
        }
        if (trimmedReason.Length > 500)
        {
            throw new BillingSupersededItemReviewBadRequestException("Alasan tidak boleh lebih dari 500 karakter.");
        }

        if (request.ExpectedInvoiceRowVersion == Guid.Empty)
        {
            throw new BillingSupersededItemReviewBadRequestException("ExpectedInvoiceRowVersion wajib diisi.");
        }

        var payloadHash = ComputePayloadHash(invoiceItemId, decision, trimmedReason, request.ExpectedInvoiceRowVersion);

        // 2. Transaksi Serializable & Penguncian Ledger
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
            }

            // 3. Periksa Idempotensi replay (BIL-VAL-158)
            var prior = await _dbContext.BilSupersededItemReviews
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey && !x.IsDelete, cancellationToken);

            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash)
                {
                    throw new BillingSupersededItemReviewConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                }

                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditDecideAsync(prior, actorUserId, isReplay: true);

                var priorReviewer = await _dbContext.Set<ApplicationUser>().AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == prior.DecidedBy, cancellationToken);
                var reviewerName = priorReviewer != null && !string.IsNullOrWhiteSpace(priorReviewer.DisplayName)
                    ? priorReviewer.DisplayName
                    : (priorReviewer?.UserName ?? prior.DecidedBy.ToString());

                return MapReviewResponse(prior, reviewerName, isReplay: true);
            }

            // 4. Muat item kandidat
            var item = await _dbContext.BilInvoiceItems
                .Include(x => x.Category)
                .SingleOrDefaultAsync(x => x.Id == invoiceItemId && !x.IsDelete, cancellationToken);

            // BIL-VAL-152: Item yang ditinjau harus VOIDED dengan kode SUPERSEDED_BY_INPATIENT_ADMISSION
            if (item is null || item.Status != BillingInvoiceItemStatuses.Voided || item.VoidReason != SupersededVoidReason)
            {
                throw new KeyNotFoundException("Item tidak termasuk daftar tinjauan void biaya administrasi.");
            }

            // Kunci ledger invoice untuk serialisasi transaksi keuangan
            await AcquireInvoiceLedgerLockAsync(item.InvoiceId, cancellationToken);

            // BIL-VAL-153: Satu keputusan per item, final
            var existingDecision = await _dbContext.BilSupersededItemReviews
                .SingleOrDefaultAsync(x => x.InvoiceItemId == invoiceItemId && !x.IsDelete, cancellationToken);
            if (existingDecision is not null)
            {
                throw new BillingSupersededItemReviewConflictException(
                    "Item ini sudah ditinjau dan keputusannya bersifat final.");
            }

            // 5. Muat invoice
            var invoice = await _dbContext.BilInvoices
                .SingleOrDefaultAsync(x => x.Id == item.InvoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");

            // BIL-VAL-157: expectedInvoiceRowVersion harus sama dengan RowVersion invoice
            if (invoice.RowVersion != request.ExpectedInvoiceRowVersion)
            {
                throw new BillingSupersededItemReviewConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");
            }

            // BIL-VAL-154: REINSTATE hanya untuk invoice OPEN
            if (decision == BilSupersededItemReviewDecisions.Reinstate && invoice.Status != BillingInvoiceStatuses.Open)
            {
                throw new BillingSupersededItemReviewValidationException(
                    "Tagihan sudah difinalisasi; pilih Perlu Koreksi Pasca-Tutup atau Biarkan.");
            }

            // BIL-VAL-155: NEEDS_POST_CLOSE_CORRECTION hanya untuk invoice yang tidak OPEN
            if (decision == BilSupersededItemReviewDecisions.NeedsPostCloseCorrection && invoice.Status == BillingInvoiceStatuses.Open)
            {
                throw new BillingSupersededItemReviewValidationException(
                    "Tagihan masih terbuka; pilih Pulihkan atau Biarkan.");
            }

            // BIL-VAL-156: REINSTATE ditolak bila sudah ada item ACTIVE dengan kunci sumber yang sama
            if (decision == BilSupersededItemReviewDecisions.Reinstate)
            {
                var hasActiveItem = await _dbContext.BilInvoiceItems
                    .AnyAsync(x => x.InvoiceId == invoice.Id
                        && x.SourceDomain == item.SourceDomain
                        && x.SourceDetailId == item.SourceDetailId
                        && x.Status == BillingInvoiceItemStatuses.Active
                        && !x.IsDelete, cancellationToken);

                if (hasActiveItem)
                {
                    throw new BillingSupersededItemReviewConflictException(
                        "Sudah ada item aktif untuk layanan yang sama pada tagihan ini.");
                }
            }

            // 6. Eksekusi Keputusan
            var reviewId = Guid.NewGuid();
            Guid? reinstatedInvoiceItemId = null;

            if (decision == BilSupersededItemReviewDecisions.Reinstate)
            {
                // Bagian 5.2: Item lama tetap VOIDED tanpa perubahan kolom. Item baru dibuat sebagai salinan ACTIVE.
                var newItem = new BilInvoiceItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = item.InvoiceId,
                    SourceDomain = item.SourceDomain,
                    SourceDetailId = item.SourceDetailId,
                    SourceVersion = item.SourceVersion + 1,
                    SourceContractVersion = item.SourceContractVersion,
                    SourceStatus = item.SourceStatus,
                    SourceOccurredAt = item.SourceOccurredAt,
                    CategoryId = item.CategoryId,
                    TariffId = item.TariffId,
                    DescriptionSnapshot = item.DescriptionSnapshot,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DoctorShare = item.DoctorShare,
                    Status = BillingInvoiceItemStatuses.Active,
                    VoidReason = null,
                    LastIdempotencyKey = idempotencyKey,
                    LastCorrelationId = reviewId,
                    LastCausationId = reviewId,
                    SourcePayloadHash = item.SourcePayloadHash,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.BilInvoiceItems.Add(newItem);
                reinstatedInvoiceItemId = newItem.Id;

                // Perbarui RowVersion invoice agar kalkulasi berikutnya memuat item ini
                invoice.RowVersion = Guid.NewGuid();
                invoice.UpdateDateTime = DateTime.UtcNow;
                invoice.UpdateBy = actorUserId;
            }

            // Simpan baris keputusan tinjauan
            var review = new BilSupersededItemReview
            {
                Id = reviewId,
                InvoiceItemId = item.Id,
                InvoiceId = invoice.Id,
                Decision = decision,
                InvoiceStatusAtDecision = invoice.Status,
                ReinstatedInvoiceItemId = reinstatedInvoiceItemId,
                Reason = trimmedReason,
                DecidedBy = actorUserId,
                DecidedAt = DateTimeOffset.UtcNow,
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.BilSupersededItemReviews.Add(review);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Item yang dipulihkan menambah kewajiban invoice OPEN: versi kalkulasi dibuat di transaksi yang sama
            // (commit atau rollback bersama keputusan tinjauan), bukan dibiarkan tertinggal untuk dihitung manual.
            if (reinstatedInvoiceItemId.HasValue)
                await RecalculateAfterReinstateAsync(invoice, actorUserId, cancellationToken);

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            // 7. Audit log resmi tanpa PII (BIL-AT-224)
            await AuditDecideAsync(review, actorUserId, isReplay: false);

            var actorUser = await _dbContext.Set<ApplicationUser>().AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);
            var actorName = actorUser != null && !string.IsNullOrWhiteSpace(actorUser.DisplayName)
                ? actorUser.DisplayName
                : (actorUser?.UserName ?? actorUserId.ToString());

            return MapReviewResponse(review, actorName, isReplay: false);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task RecalculateAfterReinstateAsync(
        BilInvoice invoice,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (_calculationService is null) return;

        try
        {
            await _calculationService.RecalculateAsync(
                invoice.Id,
                new RecalculateInvoiceRequest
                {
                    ExpectedRowVersion = invoice.RowVersion,
                    Reason = "Perhitungan otomatis: item biaya yang di-void keliru dipulihkan."
                },
                actorUserId,
                cancellationToken);
        }
        catch (BillingCalculationConflictException exception)
        {
            throw new BillingSupersededItemReviewConflictException(
                $"Keputusan dibatalkan karena perhitungan tagihan gagal diperbarui: {exception.Message} "
                + "Muat ulang tagihan lalu ulangi, atau hubungi petugas yang berwenang.",
                exception);
        }
        catch (BillingCalculationValidationException exception)
        {
            throw new BillingSupersededItemReviewValidationException(
                $"Keputusan dibatalkan karena perhitungan tagihan gagal diperbarui: {exception.Message} "
                + "Periksa kembali data tagihan atau hubungi petugas yang berwenang.");
        }
    }

    private static List<string> ResolveAllowedDecisions(string invoiceStatus)
    {
        if (invoiceStatus == BillingInvoiceStatuses.Open)
        {
            return [BilSupersededItemReviewDecisions.Reinstate, BilSupersededItemReviewDecisions.Keep];
        }

        return [BilSupersededItemReviewDecisions.NeedsPostCloseCorrection, BilSupersededItemReviewDecisions.Keep];
    }

    private static string ComputePayloadHash(
        Guid invoiceItemId,
        string decision,
        string reason,
        Guid expectedInvoiceRowVersion)
    {
        var canonical = string.Join('|',
            invoiceItemId.ToString("N"),
            decision.Trim().ToUpperInvariant(),
            reason.Trim(),
            expectedInvoiceRowVersion.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private Task AcquireInvoiceLedgerLockAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        return _dbContext.Database.IsRelational() && _dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite"
            ? _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"BIL_INVOICE_LEDGER_{invoiceId:N}"],
                cancellationToken)
            : Task.CompletedTask;
    }

    private Task AuditDecideAsync(BilSupersededItemReview review, Guid actorUserId, bool isReplay)
    {
        // BIL-AT-224: Payload MUST NOT memuat nama pasien, NIK, nomor rekam medis, maupun diagnosis
        return _loggerService.AuditAsync(
            LogCategory,
            "BillingSupersededItemReview.Decide",
            $"Keputusan tinjauan void item: {review.Decision}.",
            new
            {
                ReviewId = review.Id,
                InvoiceItemId = review.InvoiceItemId,
                InvoiceId = review.InvoiceId,
                Decision = review.Decision,
                ReinstatedInvoiceItemId = review.ReinstatedInvoiceItemId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });
    }

    private static SupersededItemReviewResponse MapReviewResponse(
        BilSupersededItemReview review,
        string decidedByName,
        bool isReplay)
    {
        return new SupersededItemReviewResponse
        {
            ReviewId = review.Id,
            InvoiceItemId = review.InvoiceItemId,
            InvoiceId = review.InvoiceId,
            Decision = review.Decision,
            InvoiceStatusAtDecision = review.InvoiceStatusAtDecision,
            ReinstatedInvoiceItemId = review.ReinstatedInvoiceItemId,
            Reason = review.Reason,
            DecidedBy = review.DecidedBy,
            DecidedByName = decidedByName,
            DecidedAt = review.DecidedAt,
            IsReplay = isReplay
        };
    }
}

/// <summary>
/// Kelas dasar exception untuk modul peninjauan void biaya administrasi.
/// </summary>
public abstract class BillingSupersededItemReviewException : Exception
{
    protected BillingSupersededItemReviewException(string message) : base(message) { }
    protected BillingSupersededItemReviewException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BillingSupersededItemReviewBadRequestException(string message)
    : BillingSupersededItemReviewException(message);

public sealed class BillingSupersededItemReviewValidationException(string message)
    : BillingSupersededItemReviewException(message);

public sealed class BillingSupersededItemReviewConflictException : BillingSupersededItemReviewException
{
    public BillingSupersededItemReviewConflictException(string message) : base(message) { }
    public BillingSupersededItemReviewConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
