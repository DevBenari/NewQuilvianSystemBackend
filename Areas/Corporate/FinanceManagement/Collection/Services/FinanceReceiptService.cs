using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;

/// <summary>
/// Pemilik logika penerimaan Finance (02-backend-architecture.md §4.7/4.22): membuat penerimaan
/// dari tender Billing, dan — sesuai lingkup literal BE-FIN-017 — membuktikan tidak ada dobel
/// hitung antara penerimaan dan piutang untuk satu tagihan (FR-FIN-035).
///
/// Riwayat: `CreateFromTenderIntakeAsync`/`CreateSucceededReceiptAsync`/`CreateReversalReceiptAsync`
/// awalnya ditulis langsung di dalam `FinanceBillingIntakeService` (BE-FIN-016), sebelum
/// ditemukan bahwa `02-backend-architecture.md` §4.22 justru menetapkan `FinanceReceiptService`
/// sebagai pemilik logika ini — dengan `FinanceBillingIntakeService` sebagai PEMANGGIL, persis
/// seperti `FinanceAccountingOutboxService`. Dipindahkan ke sini pada BE-FIN-017 atas otorisasi
/// eksplisit pemilik repository (22 September 2026, "Refactor now").
///
/// MUST NOT membuka, commit, atau rollback transaksi sendiri pada `CreateFromTenderIntakeAsync` —
/// pemanggil (`FinanceBillingIntakeService.ProcessCollectionIntakeAsync`) sudah berada di dalam
/// transaksi `Serializable` miliknya sendiri. Ini sengaja identik dengan kontrak
/// `FinanceAccountingOutboxService.StageEventAsync` (FIN-DES-017) — method ini hanya `Add()`.
///
/// Alokasi manual penerimaan ke piutang (BE-FIN-018, FIN-DES-013, FR-FIN-040..042/045):
/// `AllocateAsync`/`ReverseAllocationAsync` di bagian bawah kelas ini mengorkestrasi validasi dan
/// mutasi `FinReceipt`/`FinReceiptAllocation`, tetapi **TIDAK PERNAH** menulis kolom `FinReceivable`
/// secara langsung — setiap perubahan `OutstandingAmount` didelegasikan ke
/// `FinanceReceivableService.ApplyAllocationAsync`/`ReverseAllocationAsync`, yang tetap
/// satu-satunya penulis (BE-FIN-006 bagian "Yang mudah salah"; kontradiksi dengan komentar lama
/// kelas ini yang sempat menyebut alokasi "milik FinanceReceiptService" tanpa merinci batasnya —
/// diluruskan di sini: kepemilikan ORKESTRASI ada di sini, kepemilikan TULIS tetap di sana).
///
/// FIN-DES-013: alokasi manual penuh, TIDAK ADA pencocokan otomatis (FIFO atau lainnya) — staf
/// mengirim daftar alokasi eksplisit lewat `AllocateAsync`. Alokasi (state-transition-matrix.md
/// §2/§4) adalah aksi LANGSUNG oleh Petugas AR, BUKAN maker-checker — berbeda dari
/// `FinReceivableAdjustment`/`WriteOff` (§3, sudah ada sejak BE-FIN-008) yang memang
/// REQUESTED→APPROVED/REJECTED. Belum ada controller yang memanggil method-method ini — pola yang
/// sama dengan `FinanceReceivableService` (BE-FIN-008) yang selesai penuh sebelum
/// `FinanceReceivablesController` (BE-FIN-009) dibangun.
/// </summary>
public sealed class FinanceReceiptService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;
    private readonly FinanceReceivableService _receivableService;

    public FinanceReceiptService(
        ApplicationDbContext dbContext,
        FinanceAccountingOutboxService accountingOutboxService,
        FinanceReceivableService receivableService)
    {
        _dbContext = dbContext;
        _accountingOutboxService = accountingOutboxService;
        _receivableService = receivableService;
    }

    // ------------------------------------------------------------------------------------
    // Pembuatan penerimaan dari tender Billing (BE-FIN-016, FR-FIN-030..034). Dipanggil DI DALAM
    // transaksi FinanceBillingIntakeService.ProcessCollectionIntakeAsync — lihat ringkasan kelas.
    // ------------------------------------------------------------------------------------

    public Task<FinReceipt> CreateFromTenderIntakeAsync(BilCollectionHandoff handoff, Guid actorUserId, CancellationToken cancellationToken) =>
        handoff.TenderStatus switch
        {
            BillingTenderStatuses.Succeeded => CreateSucceededReceiptAsync(handoff, actorUserId, cancellationToken),
            BillingTenderStatuses.Reversed => CreateReversalReceiptAsync(handoff, actorUserId, cancellationToken),
            _ => throw new InvalidOperationException($"TenderStatus '{handoff.TenderStatus}' pada fakta penerimaan tidak dikenal.")
        };

    // FR-FIN-030/032: satu tender berhasil menghasilkan satu penerimaan, nominal disalin apa
    // adanya. FR-FIN-033: tunai wajib menyebut shift kasirnya — Billing sudah menegakkan ini
    // sebelum menerbitkan handoff (BIL-VAL-111), tapi diperiksa ulang di sini sebagai lapis kedua
    // sesuai FR-FIN-033 sendiri, bukan mempercayai buta sisi pengirim.
    private async Task<FinReceipt> CreateSucceededReceiptAsync(BilCollectionHandoff handoff, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (await _dbContext.FinReceipts.AnyAsync(x => !x.IsDelete && x.SourceTenderId == handoff.TenderId, cancellationToken))
            throw new InvalidOperationException("Penerimaan untuk tender ini sudah pernah dibuat.");

        var paymentMethod = await _dbContext.MstPaymentMethods.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == handoff.PaymentMethodId && !x.IsDelete, cancellationToken)
            ?? throw new InvalidOperationException("Metode pembayaran pada fakta penerimaan tidak ditemukan.");
        var isCash = paymentMethod.IsCash || string.Equals(paymentMethod.PaymentMethodType, "Cash", StringComparison.OrdinalIgnoreCase);
        if (isCash && !handoff.CashierShiftId.HasValue)
            throw new BillingIntakeValidationException(
                "Penerimaan tunai tidak dapat diproses karena shift kasirnya tidak diketahui.");

        var receipt = new FinReceipt
        {
            ReceiptNumber = GenerateReceiptNumber(),
            SourceType = FinReceiptSourceTypes.BillingTender,
            SourceTenderId = handoff.TenderId,
            SourceCollectionHandoffId = handoff.Id,
            SettlementId = handoff.SettlementId,
            InvoiceId = handoff.InvoiceId,
            PaymentMethodId = handoff.PaymentMethodId,
            PaymentMethodAccountId = handoff.PaymentMethodAccountId,
            Amount = handoff.Amount,
            AllocatedAmount = 0m,
            UnallocatedAmount = handoff.Amount,
            KwitansiNumber = handoff.KwitansiNumber,
            CashierShiftId = handoff.CashierShiftId,
            ProviderReference = handoff.ProviderReference,
            ProviderEventId = handoff.ProviderEventId,
            OccurredAt = handoff.OccurredAt,
            SourceInvoiceStatus = handoff.SourceInvoiceStatus,
            Status = FinReceiptStatuses.Received,
            CorrelationId = handoff.CorrelationId,
            CausationId = handoff.CausationId,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.Set<FinReceipt>().Add(receipt);

        // FR-FIN-034: tagihan belum final -> kejadian ditahan (HELD_FOR_FINALIZATION). Pelepasan
        // saat tagihan kelak final adalah mekanisme terpisah, di luar lingkup BE-FIN-016/017
        // (accounting-integration.md §4, dicatat sebagai OPEN dependency di laporan task).
        var requiresFinalization = string.Equals(handoff.SourceInvoiceStatus, BillingInvoiceStatuses.Open, StringComparison.OrdinalIgnoreCase);
        await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
        {
            EventTypeCode = FinAccountingEventTypeCodes.PenerimaanKasir,
            SourceTransactionId = receipt.ReceiptNumber,
            EventOccurredAt = handoff.OccurredAt,
            AccountingDate = DateOnly.FromDateTime(handoff.OccurredAt.UtcDateTime),
            Amount = receipt.Amount,
            CorrelationId = receipt.CorrelationId,
            CausationId = receipt.CausationId,
            RequiresFinalization = requiresFinalization,
            ActorUserId = actorUserId
        }, cancellationToken);

        return receipt;
    }

    // FR-FIN-034 area (evidence/02-permintaan-kontrak-untuk-owner-billing.md §2.5): tender yang
    // dibalik menerbitkan BARIS BARU (Status = REVERSED), baris asli TIDAK PERNAH diubah/dihapus
    // ("riwayatnya hilang di kedua sisi" bila diubah). Diperbaiki 23 September 2026 — versi
    // sebelumnya memblokir jalur ini karena CK_FinReceipt_TenderRequired belum mengizinkan
    // SourceTenderId kosong pada baris pembalik (lihat FinReceiptConfiguration.cs, dan laporan
    // BE-FIN-016 bagian 1.5 untuk riwayat lengkap konflik constraint-nya). Baris pembalik
    // mengosongkan SourceTenderId dan menunjuk baris asli lewat ReversalOfReceiptId — persis
    // seperti sudah didokumentasikan FinReceipt.cs sejak awal — supaya identitas idempotensi
    // tender asli (IX_FinReceipt_SourceTenderId) tidak pernah dipakai ulang.
    private async Task<FinReceipt> CreateReversalReceiptAsync(BilCollectionHandoff handoff, Guid actorUserId, CancellationToken cancellationToken)
    {
        var original = await _dbContext.FinReceipts
            .SingleOrDefaultAsync(x => !x.IsDelete && x.SourceTenderId == handoff.TenderId && x.Status != FinReceiptStatuses.Reversed, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Penerimaan asli untuk tender {handoff.TenderId:N} tidak ditemukan — pembalikan tidak dapat diproses sebelum penerimaannya sendiri tercatat.");

        // Idempotensi: satu penerimaan asli hanya boleh dibalik sekali (baris ERROR tersimpan bila
        // fakta pembalikan yang sama disinkron ulang, bukan membuat baris pembalik kedua).
        if (await _dbContext.FinReceipts.AnyAsync(x => !x.IsDelete && x.ReversalOfReceiptId == original.Id, cancellationToken))
            throw new InvalidOperationException("Penerimaan ini sudah pernah dibalik.");

        var reversal = new FinReceipt
        {
            ReceiptNumber = GenerateReceiptNumber(),
            SourceType = FinReceiptSourceTypes.BillingTender,
            SourceTenderId = null,
            ReversalOfReceiptId = original.Id,
            SourceCollectionHandoffId = handoff.Id,
            SettlementId = handoff.SettlementId,
            InvoiceId = handoff.InvoiceId,
            PaymentMethodId = handoff.PaymentMethodId,
            PaymentMethodAccountId = handoff.PaymentMethodAccountId,
            Amount = handoff.Amount,
            AllocatedAmount = 0m,
            UnallocatedAmount = handoff.Amount,
            KwitansiNumber = handoff.KwitansiNumber,
            CashierShiftId = handoff.CashierShiftId,
            ProviderReference = handoff.ProviderReference,
            ProviderEventId = handoff.ProviderEventId,
            OccurredAt = handoff.OccurredAt,
            SourceInvoiceStatus = handoff.SourceInvoiceStatus,
            Status = FinReceiptStatuses.Reversed,
            CorrelationId = handoff.CorrelationId,
            CausationId = handoff.CausationId,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.Set<FinReceipt>().Add(reversal);

        // Kejadian pembalikan mengikuti kebijakan pra-finalisasi yang sama dengan penerimaan asli
        // (FR-FIN-034) — bila tagihan sumber belum final saat pembalikan terjadi, kejadian ditahan.
        var requiresFinalization = string.Equals(handoff.SourceInvoiceStatus, BillingInvoiceStatuses.Open, StringComparison.OrdinalIgnoreCase);
        await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
        {
            EventTypeCode = FinAccountingEventTypeCodes.PembalikanPenerimaanKasir,
            SourceTransactionId = reversal.ReceiptNumber,
            EventOccurredAt = handoff.OccurredAt,
            AccountingDate = DateOnly.FromDateTime(handoff.OccurredAt.UtcDateTime),
            Amount = reversal.Amount,
            CorrelationId = reversal.CorrelationId,
            CausationId = reversal.CausationId,
            RequiresFinalization = requiresFinalization,
            ActorUserId = actorUserId
        }, cancellationToken);

        return reversal;
    }

    // ------------------------------------------------------------------------------------
    // Pembuktian tidak dobel-hitung (BE-FIN-017, FR-FIN-035) — baca saja, nol tulisan.
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// FR-FIN-035: "Sistem dapat menampilkan penerimaan kasir dan piutang penjamin berdampingan
    /// untuk satu tagihan." Mengembalikan total penerimaan (bersih dari pembalikan) dan total
    /// piutang (asli maupun sisa) untuk satu InvoiceId yang sama — dua sumber yang, bila
    /// digabung, MUST NOT pernah melebihi nilai tagihan itu sendiri. Nilai tagihan Billing sendiri
    /// sengaja TIDAK diikutkan di sini — itu perhitungan milik Billing, bukan Finance (aturan
    /// bisnis #1, FIN-OOS-001..004); dua angka Finance ini sudah cukup untuk membuktikan sisi
    /// Finance tidak dobel-mencatat.
    /// </summary>
    public async Task<InvoiceCollectionBreakdown> GetInvoiceBreakdownAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var receipts = await _dbContext.FinReceipts.AsNoTracking()
            .Where(x => !x.IsDelete && x.InvoiceId == invoiceId)
            .Select(x => new { x.Amount, x.Status })
            .ToListAsync(cancellationToken);
        var receivables = await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => !x.IsDelete && x.InvoiceId == invoiceId)
            .Select(x => new { x.OriginalAmount, x.OutstandingAmount })
            .ToListAsync(cancellationToken);

        // Baris pembalik (Status = REVERSED) menetralkan baris aslinya — dijumlah bersih, bukan
        // dijumlah sebagai dua penerimaan positif terpisah (lihat CreateReversalReceiptAsync).
        var netReceiptAmount = receipts.Where(x => x.Status != FinReceiptStatuses.Reversed).Sum(x => x.Amount)
            - receipts.Where(x => x.Status == FinReceiptStatuses.Reversed).Sum(x => x.Amount);

        return new InvoiceCollectionBreakdown(
            InvoiceId: invoiceId,
            ReceiptCount: receipts.Count,
            NetReceiptAmount: netReceiptAmount,
            ReceivableCount: receivables.Count,
            TotalReceivableOriginalAmount: receivables.Sum(x => x.OriginalAmount),
            TotalReceivableOutstandingAmount: receivables.Sum(x => x.OutstandingAmount));
    }

    // Nomor penerimaan tidak punya penomor seri resmi — dibuat unik lewat Guid, bukan
    // Count/Max/Last+1 (QBE-CODE-002/003), sampai ada keputusan skema penomoran resmi.
    private static string GenerateReceiptNumber()
    {
        var candidate = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    // ------------------------------------------------------------------------------------
    // Pembacaan (BE-FIN-018) — nol tulisan. Belum mencakup daftar berpaging/register/rekonsiliasi
    // shift dari FIN-PERM-1.0 (permission-audit-matrix.md baris 77-78) — service-nya belum ada,
    // di luar cakupan literal roadmap task ini ("Alokasi, koreksi, penghapusan"). Dicatat sebagai
    // gap terbuka, bukan diam-diam dilewatkan; lihat laporan task BE-FIN-018 pembaruan 23 September 2026.
    // ------------------------------------------------------------------------------------

    public async Task<FinReceipt> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.FinReceipts.AsNoTracking()
            .Include(x => x.Allocations)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Penerimaan tidak ditemukan.");

    public async Task<PagedResult<FinReceiptResponse>> GetPagedAsync(FinReceiptQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceipts.AsNoTracking().Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.SourceType))
            query = query.Where(x => x.SourceType == request.SourceType);

        if (request.PaymentMethodId.HasValue)
            query = query.Where(x => x.PaymentMethodId == request.PaymentMethodId.Value);

        if (request.StartDate.HasValue)
            query = query.Where(x => x.OccurredAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(x => x.OccurredAt <= request.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                x.ReceiptNumber.ToUpper().Contains(search) ||
                (x.KwitansiNumber != null && x.KwitansiNumber.ToUpper().Contains(search)));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "receiptnumber" => descending ? query.OrderByDescending(x => x.ReceiptNumber) : query.OrderBy(x => x.ReceiptNumber),
            "amount" => descending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "unallocatedamount" => descending ? query.OrderByDescending(x => x.UnallocatedAmount) : query.OrderBy(x => x.UnallocatedAmount),
            _ => descending ? query.OrderByDescending(x => x.OccurredAt) : query.OrderBy(x => x.OccurredAt)
        };

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FinReceiptResponse
            {
                Id = x.Id,
                ReceiptNumber = x.ReceiptNumber,
                SourceType = x.SourceType,
                SourceTenderId = x.SourceTenderId,
                InvoiceId = x.InvoiceId,
                PaymentMethodId = x.PaymentMethodId,
                CashierShiftId = x.CashierShiftId,
                Amount = x.Amount,
                AllocatedAmount = x.AllocatedAmount,
                UnallocatedAmount = x.UnallocatedAmount,
                OccurredAt = x.OccurredAt,
                Status = x.Status,
                ReversalOfReceiptId = x.ReversalOfReceiptId,
                RowVersion = x.RowVersion
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<FinReceiptResponse>
        {
            Items = items,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // ------------------------------------------------------------------------------------
    // Alokasi manual (BE-FIN-018, FR-FIN-040..042/045) — membuka transaksi Serializable sendiri,
    // berbeda dari CreateFromTenderIntakeAsync, karena belum ada pemanggil lain yang sudah berada
    // di dalam transaksi (tidak ada controller yang memanggil ini, lihat ringkasan kelas).
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// FR-FIN-040: petugas menentukan sendiri daftar alokasi (satu penerimaan boleh dipecah ke
    /// beberapa piutang sekaligus, atau ke `INVOICE_DIRECT` bila `ReceivableId` kosong — pasien
    /// bayar lunas tanpa piutang, FIN-DES-011). FR-FIN-041: jumlah seluruh baris tidak boleh
    /// melebihi sisa penerimaan. FR-FIN-042 ditegakkan per baris di dalam
    /// `FinanceReceivableService.ApplyAllocationAsync`.
    /// </summary>
    public async Task<FinReceipt> AllocateAsync(
        Guid receiptId, IReadOnlyList<AllocationLineRequest> lines, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (lines is not { Count: > 0 })
            throw new ReceivableBadRequestException("Daftar alokasi wajib berisi minimal satu baris.");
        foreach (var line in lines)
            if (line.Amount <= 0) throw new ReceivableBadRequestException("Nominal setiap baris alokasi harus lebih dari nol.");

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_RECEIPT_{receiptId:N}", cancellationToken);
            // Urutan lock piutang diseragamkan (menaik) supaya dua alokasi bersamaan yang
            // menyentuh piutang yang tumpang tindih tidak saling deadlock.
            foreach (var receivableId in lines.Where(l => l.ReceivableId.HasValue).Select(l => l.ReceivableId!.Value).Distinct().OrderBy(id => id))
                await AcquireLockAsync($"FIN_RECEIVABLE_{receivableId:N}", cancellationToken);

            var receipt = await _dbContext.FinReceipts
                .SingleOrDefaultAsync(x => x.Id == receiptId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Penerimaan tidak ditemukan.");
            // state-transition-matrix.md §4 baris 83: penerimaan yang sudah dibalik tidak bisa dialokasikan.
            if (receipt.Status == FinReceiptStatuses.Reversed)
                throw new ReceivableValidationException("Penerimaan yang sudah dibalik tidak dapat dialokasikan — uangnya sudah tidak ada.");

            var totalRequested = lines.Sum(l => l.Amount);
            // FR-FIN-041.
            if (totalRequested > receipt.UnallocatedAmount)
                throw new ReceivableValidationException(
                    $"Alokasi melebihi sisa penerimaan. Sisa saat ini Rp {receipt.UnallocatedAmount:N0}.");

            var now = DateTimeOffset.UtcNow;
            foreach (var line in lines)
            {
                // FR-FIN-042 ditegakkan di dalam ApplyAllocationAsync — satu-satunya penulis OutstandingAmount.
                if (line.ReceivableId.HasValue)
                    await _receivableService.ApplyAllocationAsync(line.ReceivableId.Value, line.Amount, actorUserId, cancellationToken);

                _dbContext.Set<FinReceiptAllocation>().Add(new FinReceiptAllocation
                {
                    ReceiptId = receiptId,
                    ReceivableId = line.ReceivableId,
                    TargetType = line.ReceivableId.HasValue ? FinReceiptAllocationTargetTypes.Receivable : FinReceiptAllocationTargetTypes.InvoiceDirect,
                    Amount = line.Amount,
                    IsReversal = false,
                    AllocatedBy = actorUserId,
                    AllocatedAt = now,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }

            receipt.AllocatedAmount += totalRequested;
            receipt.UnallocatedAmount -= totalRequested;
            // state-transition-matrix.md §4: RECEIVED tetap RECEIVED bila masih ada sisa, ALLOCATED bila habis.
            receipt.Status = receipt.UnallocatedAmount == 0m ? FinReceiptStatuses.Allocated : FinReceiptStatuses.Received;
            receipt.UpdateDateTime = DateTime.UtcNow;
            receipt.UpdateBy = actorUserId;
            receipt.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            return receipt;
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

    /// <summary>
    /// FR-FIN-045: pembalikan tidak menghapus riwayat — baris alokasi lama tetap utuh, baris baru
    /// (`IsReversal = true`) yang menetralkan. Piutang (bila ada) dan penerimaan sama-sama
    /// dikembalikan ke keadaan sebelum alokasi ini.
    /// </summary>
    public async Task<FinReceiptAllocation> ReverseAllocationAsync(Guid allocationId, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);

            var original = await _dbContext.FinReceiptAllocations.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == allocationId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Alokasi tidak ditemukan.");
            if (original.IsReversal)
                throw new ReceivableValidationException("Baris pembalik tidak dapat dibalik lagi.");
            if (await _dbContext.FinReceiptAllocations.AnyAsync(x => x.ReversalOfAllocationId == original.Id, cancellationToken))
                throw new ReceivableValidationException("Alokasi ini sudah pernah dibalik sebelumnya.");

            await AcquireLockAsync($"FIN_RECEIPT_{original.ReceiptId:N}", cancellationToken);
            if (original.ReceivableId.HasValue)
                await AcquireLockAsync($"FIN_RECEIVABLE_{original.ReceivableId.Value:N}", cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var reversal = new FinReceiptAllocation
            {
                ReceiptId = original.ReceiptId,
                ReceivableId = original.ReceivableId,
                TargetType = original.TargetType,
                Amount = original.Amount,
                IsReversal = true,
                ReversalOfAllocationId = original.Id,
                AllocatedBy = actorUserId,
                AllocatedAt = now,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.Set<FinReceiptAllocation>().Add(reversal);

            if (original.ReceivableId.HasValue)
                await _receivableService.ReverseAllocationAsync(original.ReceivableId.Value, original.Amount, actorUserId, cancellationToken);

            var receipt = await _dbContext.FinReceipts.SingleAsync(x => x.Id == original.ReceiptId, cancellationToken);
            receipt.AllocatedAmount -= original.Amount;
            receipt.UnallocatedAmount += original.Amount;
            receipt.Status = FinReceiptStatuses.Received; // tidak lagi ALLOCATED penuh setelah sebagian dibalik
            receipt.UpdateDateTime = DateTime.UtcNow;
            receipt.UpdateBy = actorUserId;
            receipt.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            return reversal;
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

/// <summary>Satu baris permintaan alokasi (BE-FIN-018, FR-FIN-040). ReceivableId kosong = INVOICE_DIRECT.</summary>
public sealed record AllocationLineRequest(Guid? ReceivableId, decimal Amount);

/// <summary>Hasil GetInvoiceBreakdownAsync (FR-FIN-035) — dua sisi Finance untuk satu tagihan.</summary>
public sealed record InvoiceCollectionBreakdown(
    Guid InvoiceId,
    int ReceiptCount,
    decimal NetReceiptAmount,
    int ReceivableCount,
    decimal TotalReceivableOriginalAmount,
    decimal TotalReceivableOutstandingAmount);
