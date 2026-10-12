using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
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
    private readonly FinanceReceivableInvoiceBatchStatusSynchronizer _batchStatusSynchronizer;

    public FinanceReceiptService(
        ApplicationDbContext dbContext,
        FinanceAccountingOutboxService accountingOutboxService,
        FinanceReceivableService receivableService,
        FinanceReceivableInvoiceBatchStatusSynchronizer batchStatusSynchronizer)
    {
        _dbContext = dbContext;
        _accountingOutboxService = accountingOutboxService;
        _receivableService = receivableService;
        _batchStatusSynchronizer = batchStatusSynchronizer;
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

        // BE-FIN-069, FIN-INTEGRATION-1.7 §5.12.1: dimensi shift dan metode pembayaran kejadian ini.
        var cashierShiftNumber = await ResolveCashierShiftNumberAsync(handoff.CashierShiftId, cancellationToken);

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

        // FR-FIN-034: tagihan belum final -> PENERIMAAN-UANG-MUKA; tagihan final -> PENERIMAAN-KASIR (FIN-DEC-030, FIN-DES-033, BE-FIN-024).
        var eventTypeCode = string.Equals(handoff.SourceInvoiceStatus, BillingInvoiceStatuses.Open, StringComparison.OrdinalIgnoreCase)
            ? FinAccountingEventTypeCodes.PenerimaanUangMuka
            : FinAccountingEventTypeCodes.PenerimaanKasir;

        await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
        {
            EventTypeCode = eventTypeCode,
            SourceTransactionId = receipt.ReceiptNumber,
            EventOccurredAt = handoff.OccurredAt,
            AccountingDate = FinanceBusinessDate.ToDateOnly(handoff.OccurredAt),
            Amount = receipt.Amount,
            CorrelationId = receipt.CorrelationId,
            CausationId = receipt.CausationId,
            ActorUserId = actorUserId,
            CashierShiftId = handoff.CashierShiftId,
            CashierShiftNumber = cashierShiftNumber,
            PaymentMethodCode = paymentMethod.PaymentMethodCode,
            PaymentMethodAccountId = handoff.PaymentMethodAccountId
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

        // BE-FIN-069, FIN-INTEGRATION-1.7 §5.12.2: kuitansi pembalik membawa shift SAAT PEMBALIKAN
        // terjadi (handoff ini), bukan shift kuitansi asli — FIN-DEC-120. Metode pembayaran
        // pembalik juga diambil dari handoff ini, bukan disalin dari metode penerimaan asli.
        var reversalPaymentMethodCode = await _dbContext.MstPaymentMethods.AsNoTracking()
            .Where(x => x.Id == handoff.PaymentMethodId && !x.IsDelete)
            .Select(x => x.PaymentMethodCode)
            .SingleOrDefaultAsync(cancellationToken);
        var reversalCashierShiftNumber = await ResolveCashierShiftNumberAsync(handoff.CashierShiftId, cancellationToken);

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

        // FR-FIN-034, FIN-DES-034, FIN-DEC-044, FIN-VAL-085 (BE-FIN-024): kode pembalikan penerimaan
        // diturunkan dari SourceInvoiceStatus milik baris penerimaan ASLI (yang ditunjuk ReversalOfReceiptId),
        // bukan dari status tagihan saat pembalikan terjadi:
        // - Bila penerimaan asli berstatus OPEN -> PEMBALIKAN-PENERIMAAN-UANG-MUKA (walau tagihan sekarang sudah FINAL).
        // - Bila penerimaan asli berstatus FINAL (atau bukan OPEN) -> PEMBALIKAN-PENERIMAAN-KASIR.
        var reversalEventTypeCode = string.Equals(original.SourceInvoiceStatus, BillingInvoiceStatuses.Open, StringComparison.OrdinalIgnoreCase)
            ? FinAccountingEventTypeCodes.PembalikanPenerimaanUangMuka
            : FinAccountingEventTypeCodes.PembalikanPenerimaanKasir;

        await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
        {
            EventTypeCode = reversalEventTypeCode,
            SourceTransactionId = reversal.ReceiptNumber,
            EventOccurredAt = handoff.OccurredAt,
            AccountingDate = FinanceBusinessDate.ToDateOnly(handoff.OccurredAt),
            Amount = reversal.Amount,
            CorrelationId = reversal.CorrelationId,
            CausationId = reversal.CausationId,
            ActorUserId = actorUserId,
            CashierShiftId = handoff.CashierShiftId,
            CashierShiftNumber = reversalCashierShiftNumber,
            PaymentMethodCode = reversalPaymentMethodCode,
            PaymentMethodAccountId = handoff.PaymentMethodAccountId,
            ReversalOfSourceTransactionId = original.ReceiptNumber
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

    // Nomor potongan (FIN-DES-048 "Nomor") — pola yang sama dengan GenerateReceiptNumber, bukan
    // Count/Max/Last+1. Wajib berbeda per baris potongan supaya tidak berbagi kunci kejadian
    // (SourceTransactionId) dengan potongan lain pada penerimaan yang sama.
    private static string GenerateDeductionNumber()
    {
        var candidate = $"DED-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    // BE-FIN-069, FIN-INTEGRATION-1.7 §5.12.1: rujukan shift yang terbaca manusia untuk payload
    // Accounting — null bila penerimaan tidak melibatkan shift kasir (non-tunai).
    private async Task<string?> ResolveCashierShiftNumberAsync(Guid? cashierShiftId, CancellationToken cancellationToken)
    {
        if (!cashierShiftId.HasValue) return null;
        return await _dbContext.Set<BilCashierShift>().AsNoTracking()
            .Where(x => x.Id == cashierShiftId.Value)
            .Select(x => x.ShiftNumber)
            .SingleOrDefaultAsync(cancellationToken);
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

    /// <summary>BE-FIN-040, `GET /receipts/{id}/deductions` (FIN-API-1.2 B.8) — baca saja.</summary>
    public async Task<List<FinReceiptDeduction>> GetDeductionsAsync(Guid receiptId, CancellationToken cancellationToken)
    {
        if (!await _dbContext.FinReceipts.AsNoTracking().AnyAsync(x => x.Id == receiptId && !x.IsDelete, cancellationToken))
            throw new KeyNotFoundException("Penerimaan tidak ditemukan.");

        return await _dbContext.FinReceiptDeductions.AsNoTracking()
            .Where(x => x.ReceiptId == receiptId && !x.IsDelete)
            .OrderBy(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);
    }

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
    // Buku register dan rekonsiliasi shift (BE-FIN-050, FIN-API-1.0) — baca saja, menutup gap
    // yang eksplisit dikecualikan BE-FIN-018 ("di luar cakupan literal roadmap task ini").
    // ------------------------------------------------------------------------------------

    /// <summary>`GET /receipts/register` — buku penerimaan kasir per tanggal, menelusur ke
    /// tender dan kwitansi asalnya (FIN-API-1.0).</summary>
    public async Task<PagedResult<ReceiptRegisterResponse>> GetRegisterAsync(ReceiptRegisterQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceipts.AsNoTracking().Where(x => !x.IsDelete);

        if (request.StartDate.HasValue)
            query = query.Where(x => x.OccurredAt >= request.StartDate.Value);
        if (request.EndDate.HasValue)
            query = query.Where(x => x.OccurredAt <= request.EndDate.Value);
        if (request.CashierShiftId.HasValue)
            query = query.Where(x => x.CashierShiftId == request.CashierShiftId.Value);

        query = query.OrderByDescending(x => x.OccurredAt);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ReceiptRegisterResponse
            {
                Id = x.Id,
                ReceiptNumber = x.ReceiptNumber,
                KwitansiNumber = x.KwitansiNumber,
                SourceTenderId = x.SourceTenderId,
                CashierShiftId = x.CashierShiftId,
                PaymentMethodId = x.PaymentMethodId,
                Amount = x.Amount,
                Status = x.Status,
                OccurredAt = x.OccurredAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ReceiptRegisterResponse>
        {
            Items = items,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>BE-FIN-054 (FIN-DES-073). `GET /receipts/reversed-allocations` — daftar baris
    /// pembalik alokasi (IsReversal = true), baca saja. Pembalikan tetap satu-satunya lewat
    /// ReverseAllocationAsync di bawah; method ini TIDAK PERNAH menulis FinReceiptAllocation.</summary>
    public async Task<PagedResult<ReversedAllocationRowResponse>> GetReversedAllocationsAsync(
        ReversedAllocationQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceiptAllocations.AsNoTracking()
            .Include(x => x.Receipt)
            .Include(x => x.Receivable)
            .Where(x => !x.IsDelete && x.IsReversal);

        if (request.StartDate.HasValue) query = query.Where(x => x.AllocatedAt >= request.StartDate.Value);
        if (request.EndDate.HasValue) query = query.Where(x => x.AllocatedAt <= request.EndDate.Value);
        if (request.ReceiptId.HasValue) query = query.Where(x => x.ReceiptId == request.ReceiptId.Value);

        query = query.OrderByDescending(x => x.AllocatedAt);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ReversedAllocationRowResponse
            {
                AllocationId = x.Id,
                ReceiptId = x.ReceiptId,
                ReceiptNumber = x.Receipt!.ReceiptNumber,
                ReceivableId = x.ReceivableId,
                ReceivableNumber = x.Receivable != null ? x.Receivable.ReceivableNumber : null,
                Amount = x.Amount,
                ReversalOfAllocationId = x.ReversalOfAllocationId!.Value,
                ReversedAt = x.AllocatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ReversedAllocationRowResponse>
        {
            Items = items,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>`GET /receipts/shift-reconciliation` — membandingkan total penerimaan tunai
    /// Finance dengan `BilCashierShift.SystemCash` milik Billing untuk satu shift (FIN-API-1.0).
    /// Baca saja: nol tulisan ke `FinReceipt` maupun `BilCashierShift`, sama seperti
    /// `GetInvoiceBreakdownAsync` (BE-FIN-017) — kedua sisi tetap dimiliki service masing-masing.</summary>
    public async Task<ShiftReconciliationResponse> GetShiftReconciliationAsync(Guid cashierShiftId, CancellationToken cancellationToken)
    {
        var shift = await _dbContext.BilCashierShifts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == cashierShiftId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Shift kasir tidak ditemukan.");

        var receipts = await _dbContext.FinReceipts.AsNoTracking()
            .Where(x => !x.IsDelete && x.CashierShiftId == cashierShiftId)
            .Select(x => new { x.Amount, x.Status, x.PaymentMethodId })
            .ToListAsync(cancellationToken);

        var cashPaymentMethodIds = await _dbContext.MstPaymentMethods.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsCash)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var cashReceipts = receipts.Where(x => x.PaymentMethodId.HasValue && cashPaymentMethodIds.Contains(x.PaymentMethodId.Value)).ToList();

        // Baris pembalik (Status = REVERSED) menetralkan baris aslinya — dijumlah bersih, pola
        // sama dengan GetInvoiceBreakdownAsync.
        var netCashAmount = cashReceipts.Where(x => x.Status != FinReceiptStatuses.Reversed).Sum(x => x.Amount)
            - cashReceipts.Where(x => x.Status == FinReceiptStatuses.Reversed).Sum(x => x.Amount);

        return new ShiftReconciliationResponse
        {
            CashierShiftId = shift.Id,
            ShiftNumber = shift.ShiftNumber,
            CashierShiftStatus = shift.Status,
            SystemCashAmount = shift.SystemCash,
            FinanceNetCashReceiptAmount = netCashAmount,
            Variance = shift.SystemCash - netCashAmount,
            CashReceiptCount = cashReceipts.Count
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
    ///
    /// BE-FIN-040, FIN-DES-048/052: setiap baris boleh membawa `Deductions[]` (PPh 23 / biaya
    /// admin bank). Potongan MENGURANGI SISA PIUTANG lewat `ApplyAllocationAsync` yang sama —
    /// bukan mengurangi `UnallocatedAmount` penerimaan (FIN-VAL-121, uangnya tidak pernah
    /// bergerak untuk potongan). Kode kejadian dipilih dari `DeductionType`; `OTHER` ditolak
    /// fail-closed (FIN-VAL-137) sampai Accounting meratifikasi kode ketiga (FIN-OQ-033).
    /// </summary>
    public async Task<FinReceipt> AllocateAsync(
        Guid receiptId, IReadOnlyList<AllocationLineRequest> lines, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (lines is not { Count: > 0 })
            throw new ReceivableBadRequestException("Daftar alokasi wajib berisi minimal satu baris.");
        foreach (var line in lines)
        {
            if (line.Amount <= 0) throw new ReceivableBadRequestException("Nominal setiap baris alokasi harus lebih dari nol.");

            if (line.Deductions is not { Count: > 0 }) continue;

            // FIN-VAL-128: potongan hanya sah pada baris ber-ReceivableId terisi (TargetType = RECEIVABLE).
            if (!line.ReceivableId.HasValue)
                throw new ReceivableBadRequestException("Potongan hanya dapat dicatat pada pelunasan piutang.");

            foreach (var deduction in line.Deductions)
            {
                // FIN-VAL-118.
                if (deduction.Amount <= 0)
                    throw new ReceivableBadRequestException("Nominal potongan harus lebih dari nol.");

                // FIN-VAL-137 (FIN-DES-052): OTHER ditolak fail-closed — belum ada akun debit yang sah.
                if (deduction.DeductionType == FinReceiptDeductionTypes.Other)
                    throw new ReceivableBadRequestException(
                        "Jenis potongan ini belum dapat dicatat karena perlakuan akuntansinya belum ditetapkan. Pakai PPh 23 atau biaya administrasi bank, atau hubungi bagian akuntansi.");

                if (deduction.DeductionType != FinReceiptDeductionTypes.Pph23 && deduction.DeductionType != FinReceiptDeductionTypes.BankAdminFee)
                    throw new ReceivableBadRequestException($"Jenis potongan '{deduction.DeductionType}' tidak dikenal.");
            }
        }

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

            // FR-FIN-041: hanya uang alokasi yang dibandingkan ke sisa penerimaan — potongan
            // BUKAN uang penerimaan (FIN-VAL-121), sehingga tidak ikut dijumlahkan di sini.
            var totalRequested = lines.Sum(l => l.Amount);
            if (totalRequested > receipt.UnallocatedAmount)
                throw new ReceivableValidationException(
                    $"Alokasi melebihi sisa penerimaan. Sisa saat ini Rp {receipt.UnallocatedAmount:N0}.");

            // Cek integrasi Ayat Silang (FinCrossEntry)
            var crossEntry = await _dbContext.FinCrossEntries
                .SingleOrDefaultAsync(x => x.ReceiptId == receiptId && !x.IsDelete, cancellationToken);
            if (crossEntry != null && crossEntry.Status == FinCrossEntryStatuses.Cancelled)
                throw new ReceivableValidationException("Dokumen Ayat Silang telah dibatalkan dan tidak dapat dialokasikan.");

            decimal crossEntryRunningBalance = receipt.UnallocatedAmount;

            var now = DateTimeOffset.UtcNow;
            foreach (var line in lines)
            {
                var allocation = new FinReceiptAllocation
                {
                    Id = Guid.NewGuid(),
                    ReceiptId = receiptId,
                    ReceivableId = line.ReceivableId,
                    TargetType = line.ReceivableId.HasValue ? FinReceiptAllocationTargetTypes.Receivable : FinReceiptAllocationTargetTypes.InvoiceDirect,
                    Amount = line.Amount,
                    IsReversal = false,
                    AllocatedBy = actorUserId,
                    AllocatedAt = now,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.Set<FinReceiptAllocation>().Add(allocation);

                // FR-FIN-042 ditegakkan di dalam ApplyAllocationAsync — satu-satunya penulis OutstandingAmount.
                // BE-FIN-060, FIN-DES-079: Meneruskan rujukan alokasi dan nomor kuitansi ke mutasi subledger (Jalur 2).
                if (line.ReceivableId.HasValue)
                {
                    await _receivableService.ApplyAllocationAsync(
                        line.ReceivableId.Value,
                        line.Amount,
                        actorUserId,
                        cancellationToken,
                        movementType: FinReceivableMovementTypes.AlokasiPenerimaan,
                        sourceAllocationId: allocation.Id,
                        referenceNumber: receipt.ReceiptNumber);
                }

                // Integrasi Ayat Silang: buat mutasi AR_ALLOCATION (DEBIT) jika receipt terhubung dengan FinCrossEntry
                if (crossEntry != null)
                {
                    var duplicateExists = await _dbContext.FinCrossEntryTransactions
                        .AnyAsync(t => t.CrossEntryId == crossEntry.Id && t.ReceiptAllocationId == allocation.Id && t.TransactionType == FinCrossEntryTransactionTypes.ArAllocation && !t.IsDelete, cancellationToken);

                    if (!duplicateExists)
                    {
                        string? resolvedBatchNumber = null;
                        string? resolvedReceivableNumber = null;
                        if (line.ReceivableId.HasValue)
                        {
                            resolvedBatchNumber = await (from item in _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                                                         join b in _dbContext.FinReceivableInvoiceBatches.AsNoTracking() on item.BatchId equals b.Id
                                                         where item.ReceivableId == line.ReceivableId.Value && !item.IsDelete && item.IsActiveMembership && !b.IsDelete
                                                         select b.BatchNumber).FirstOrDefaultAsync(cancellationToken);

                            if (string.IsNullOrWhiteSpace(resolvedBatchNumber))
                            {
                                resolvedReceivableNumber = await _dbContext.FinReceivables.AsNoTracking()
                                    .Where(r => r.Id == line.ReceivableId.Value)
                                    .Select(r => r.ReceivableNumber)
                                    .FirstOrDefaultAsync(cancellationToken);
                            }
                        }

                        string description = !string.IsNullOrWhiteSpace(resolvedBatchNumber)
                            ? $"Alokasi Ayat Silang ke Invoice AR {resolvedBatchNumber}"
                            : !string.IsNullOrWhiteSpace(resolvedReceivableNumber)
                                ? $"Alokasi Ayat Silang ke Piutang AR {resolvedReceivableNumber}"
                                : "Alokasi Ayat Silang ke Invoice AR";

                        crossEntryRunningBalance -= line.Amount;
                        var crossEntryTx = new FinCrossEntryTransaction
                        {
                            Id = Guid.NewGuid(),
                            CrossEntryId = crossEntry.Id,
                            TransactionType = FinCrossEntryTransactionTypes.ArAllocation,
                            Direction = FinCrossEntryDirections.Debit,
                            Amount = line.Amount,
                            BalanceAfterTransaction = crossEntryRunningBalance,
                            OccurredAt = now,
                            ReceiptAllocationId = allocation.Id,
                            Description = description,
                            CreateBy = actorUserId,
                            CreateDateTime = DateTime.UtcNow
                        };
                        _dbContext.Set<FinCrossEntryTransaction>().Add(crossEntryTx);
                    }
                }

                if (line.Deductions is not { Count: > 0 }) continue;

                foreach (var deductionLine in line.Deductions)
                {
                    var deduction = new FinReceiptDeduction
                    {
                        Id = Guid.NewGuid(),
                        DeductionNumber = GenerateDeductionNumber(),
                        ReceiptId = receiptId,
                        ReceiptAllocationId = allocation.Id,
                        DeductionType = deductionLine.DeductionType,
                        Amount = deductionLine.Amount,
                        Reason = deductionLine.Reason,
                        ReferenceNumber = deductionLine.ReferenceNumber,
                        IsReversal = false,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = actorUserId
                    };
                    _dbContext.Set<FinReceiptDeduction>().Add(deduction);

                    // FIN-VAL-033 diperluas (FIN-DES-048): uang alokasi + seluruh potongan ≤ sisa piutang.
                    // Dipanggil beruntun terhadap FinReceivable yang sama (tracked oleh DbContext yang
                    // sama), sehingga setiap panggilan memeriksa sisa TERKINI, bukan sisa sebelum baris ini.
                    // BE-FIN-060, FIN-DES-079: Mutasi subledger POTONGAN (Jalur 4).
                    var receivable = await _receivableService.ApplyAllocationAsync(
                        line.ReceivableId!.Value,
                        deductionLine.Amount,
                        actorUserId,
                        cancellationToken,
                        movementType: FinReceivableMovementTypes.Potongan,
                        sourceAllocationId: allocation.Id,
                        referenceNumber: deduction.DeductionNumber,
                        notes: $"{deductionLine.DeductionType}: {deductionLine.Reason}");

                    // FIN-DES-050/052: kode dipilih dari DeductionType — POTONGAN-PIUTANG-NON-TUNAI
                    // MUST NOT ditulis lagi (superseded, AMENDMENT REVISI 6).
                    var eventTypeCode = deductionLine.DeductionType == FinReceiptDeductionTypes.Pph23
                        ? FinAccountingEventTypeCodes.PotonganPph23Piutang
                        : FinAccountingEventTypeCodes.PotonganBiayaBankPiutang;

                    await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                    {
                        EventTypeCode = eventTypeCode,
                        SourceTransactionId = deduction.DeductionNumber,
                        EventOccurredAt = now,
                        AccountingDate = FinanceBusinessDate.ToDateOnly(now),
                        Amount = deduction.Amount,
                        CorrelationId = receivable.CorrelationId,
                        CausationId = deduction.Id,
                        ActorUserId = actorUserId
                    }, cancellationToken);
                }
            }

            // Pembaruan status Ayat Silang
            if (crossEntry != null && crossEntry.Status != FinCrossEntryStatuses.Cancelled)
            {
                var finalUnallocated = receipt.UnallocatedAmount - totalRequested;
                crossEntry.Status = finalUnallocated == 0m
                    ? FinCrossEntryStatuses.FullyUsed
                    : finalUnallocated < receipt.Amount
                        ? FinCrossEntryStatuses.PartiallyUsed
                        : FinCrossEntryStatuses.Open;
                crossEntry.UpdateDateTime = DateTime.UtcNow;
                crossEntry.UpdateBy = actorUserId;
                crossEntry.RowVersion = Guid.NewGuid();
            }

            // Sinkronisasi status batch tagihan AR untuk seluruh piutang terdampak
            var affectedReceivableIds = lines
                .Where(l => l.ReceivableId.HasValue)
                .Select(l => l.ReceivableId!.Value)
                .Distinct()
                .ToList();
            if (affectedReceivableIds.Count > 0)
            {
                await _batchStatusSynchronizer.SynchronizeByReceivableIdsAsync(affectedReceivableIds, actorUserId, cancellationToken);
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
            var reversal = await ReverseAllocationCoreAsync(allocationId, actorUserId, cancellationToken, acquireLocks: true, synchronizeBatch: true);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            return reversal;
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
    }

    /// <summary>
    /// Inti pembalikan satu FinReceiptAllocation tanpa membuka transaksi mandiri.
    /// Digunakan baik oleh ReverseAllocationAsync maupun orchestrasi pembatalan batch (CancelAsync)
    /// yang membalik beberapa alokasi sekaligus dalam satu transaksi atomik.
    /// </summary>
    public async Task<FinReceiptAllocation> ReverseAllocationCoreAsync(
        Guid allocationId,
        Guid actorUserId,
        CancellationToken cancellationToken,
        bool acquireLocks = true,
        bool synchronizeBatch = true)
    {
        var original = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == allocationId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Alokasi tidak ditemukan.");
        if (original.IsReversal)
            throw new ReceivableValidationException("Baris pembalik tidak dapat dibalik lagi.");
        if (await _dbContext.FinReceiptAllocations.AnyAsync(x => x.ReversalOfAllocationId == original.Id && !x.IsDelete, cancellationToken))
            throw new ReceivableValidationException("Alokasi ini sudah pernah dibalik sebelumnya.");

        if (acquireLocks)
        {
            await AcquireLockAsync($"FIN_RECEIPT_{original.ReceiptId:N}", cancellationToken);
            if (original.ReceivableId.HasValue)
                await AcquireLockAsync($"FIN_RECEIVABLE_{original.ReceivableId.Value:N}", cancellationToken);
        }

        var receipt = await _dbContext.FinReceipts.SingleAsync(x => x.Id == original.ReceiptId, cancellationToken);

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
        {
            // BE-FIN-060, FIN-DES-079: Mutasi subledger PEMBALIKAN-ALOKASI (Jalur 3).
            await _receivableService.ReverseAllocationAsync(
                original.ReceivableId.Value,
                original.Amount,
                actorUserId,
                cancellationToken,
                movementType: FinReceivableMovementTypes.PembalikanAlokasi,
                sourceAllocationId: reversal.Id,
                referenceNumber: receipt.ReceiptNumber);

            // BE-FIN-040, FIN-DES-049: seluruh potongan pada alokasi ini ikut dibalik dalam
            // transaksi yang sama — potongan tidak dapat dibalik sendirian tanpa alokasinya.
            var deductions = await _dbContext.FinReceiptDeductions.AsNoTracking()
                .Where(x => x.ReceiptAllocationId == original.Id && !x.IsReversal && !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var deduction in deductions)
            {
                var deductionReversal = new FinReceiptDeduction
                {
                    DeductionNumber = GenerateDeductionNumber(),
                    ReceiptId = deduction.ReceiptId,
                    ReceiptAllocationId = reversal.Id,
                    DeductionType = deduction.DeductionType,
                    Amount = deduction.Amount,
                    Reason = deduction.Reason,
                    ReferenceNumber = deduction.ReferenceNumber,
                    IsReversal = true,
                    ReversalOfDeductionId = deduction.Id,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.Set<FinReceiptDeduction>().Add(deductionReversal);

                // BE-FIN-060, FIN-DES-079: Mutasi subledger PEMBALIKAN-POTONGAN (Jalur 4).
                var receivable = await _receivableService.ReverseAllocationAsync(
                    original.ReceivableId.Value,
                    deduction.Amount,
                    actorUserId,
                    cancellationToken,
                    movementType: FinReceivableMovementTypes.PembalikanPotongan,
                    sourceAllocationId: reversal.Id,
                    referenceNumber: deductionReversal.DeductionNumber,
                    notes: $"Pembalikan {deduction.DeductionType}: {deduction.Reason}");

                // FIN-DES-050/052: kebalikan kode 32/34 — nilai baris pembalik SELALU positif.
                var reversalEventTypeCode = deduction.DeductionType == FinReceiptDeductionTypes.Pph23
                    ? FinAccountingEventTypeCodes.PembalikanPotonganPph23Piutang
                    : FinAccountingEventTypeCodes.PembalikanPotonganBiayaBankPiutang;

                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = reversalEventTypeCode,
                    SourceTransactionId = deductionReversal.DeductionNumber,
                    EventOccurredAt = now,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(now),
                    Amount = deduction.Amount,
                    CorrelationId = receivable.CorrelationId,
                    CausationId = deductionReversal.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            // Pembalikan potongan non-deduction (Diskon Invoice AR) yang melekat pada alokasi ini.
            // Sesuai audit Task Revamp Receivable AR Canceled: PostPaymentAsync membuat diskon via
            // FinReceivableMovementTypes.Potongan dengan SourceAllocationId = original.Id tanpa FinReceiptDeduction.
            // Saat alokasi dibalik, efek finansial diskon ini dinetralkan melalui PEMBALIKAN-POTONGAN.
            var discountMovements = await _dbContext.FinReceivableMovements.AsNoTracking()
                .Where(x => x.ReceivableId == original.ReceivableId.Value
                    && x.SourceAllocationId == original.Id
                    && x.MovementType == FinReceivableMovementTypes.Potongan
                    && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var deductionNumbers = deductions.Select(d => d.DeductionNumber).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var nonDeductionDiscountMovements = discountMovements
                .Where(m => string.IsNullOrEmpty(m.ReferenceNumber) || !deductionNumbers.Contains(m.ReferenceNumber))
                .ToList();

            foreach (var discMovement in nonDeductionDiscountMovements)
            {
                var discAmount = Math.Abs(discMovement.Amount);
                if (discAmount > 0)
                {
                    await _receivableService.ReverseAllocationAsync(
                        original.ReceivableId.Value,
                        discAmount,
                        actorUserId,
                        cancellationToken,
                        movementType: FinReceivableMovementTypes.PembalikanPotongan,
                        sourceAllocationId: reversal.Id,
                        referenceNumber: receipt.ReceiptNumber,
                        notes: $"Pembalikan {discMovement.Notes ?? "Diskon AR"}");
                }
            }
        }
        receipt.AllocatedAmount -= original.Amount;
        receipt.UnallocatedAmount += original.Amount;
        receipt.Status = FinReceiptStatuses.Received; // tidak lagi ALLOCATED penuh setelah sebagian dibalik
        receipt.UpdateDateTime = DateTime.UtcNow;
        receipt.UpdateBy = actorUserId;
        receipt.RowVersion = Guid.NewGuid();

        // Cek integrasi Ayat Silang (FinCrossEntry) untuk pembalikan alokasi
        var crossEntry = await _dbContext.FinCrossEntries
            .SingleOrDefaultAsync(x => x.ReceiptId == original.ReceiptId && !x.IsDelete, cancellationToken);

        if (crossEntry != null)
        {
            var origTx = await _dbContext.FinCrossEntryTransactions
                .FirstOrDefaultAsync(x => x.CrossEntryId == crossEntry.Id && x.ReceiptAllocationId == original.Id && x.TransactionType == FinCrossEntryTransactionTypes.ArAllocation && !x.IsDelete, cancellationToken);

            string? resolvedBatchNumber = null;
            if (original.ReceivableId.HasValue)
            {
                resolvedBatchNumber = await (from item in _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                                             join b in _dbContext.FinReceivableInvoiceBatches.AsNoTracking() on item.BatchId equals b.Id
                                             where item.ReceivableId == original.ReceivableId.Value && !item.IsDelete && item.IsActiveMembership && !b.IsDelete
                                             select b.BatchNumber).FirstOrDefaultAsync(cancellationToken);
            }

            string description = !string.IsNullOrWhiteSpace(resolvedBatchNumber)
                ? $"Pembalikan alokasi Ayat Silang ke Invoice AR {resolvedBatchNumber}"
                : "Pembalikan alokasi Ayat Silang";

            var reversalBalance = receipt.UnallocatedAmount;
            var reversalTx = new FinCrossEntryTransaction
            {
                Id = Guid.NewGuid(),
                CrossEntryId = crossEntry.Id,
                TransactionType = FinCrossEntryTransactionTypes.ArAllocationReversal,
                Direction = FinCrossEntryDirections.Credit,
                Amount = original.Amount,
                BalanceAfterTransaction = reversalBalance,
                OccurredAt = now,
                ReceiptAllocationId = reversal.Id,
                ReversalOfTransactionId = origTx?.Id,
                Description = description,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };
            _dbContext.Set<FinCrossEntryTransaction>().Add(reversalTx);

            if (crossEntry.Status != FinCrossEntryStatuses.Cancelled)
            {
                crossEntry.Status = reversalBalance == receipt.Amount
                    ? FinCrossEntryStatuses.Open
                    : reversalBalance > 0m
                        ? FinCrossEntryStatuses.PartiallyUsed
                        : FinCrossEntryStatuses.FullyUsed;
                crossEntry.UpdateDateTime = DateTime.UtcNow;
                crossEntry.UpdateBy = actorUserId;
                crossEntry.RowVersion = Guid.NewGuid();
            }
        }

        // Sinkronisasi status batch tagihan AR untuk piutang yang alokasinya dibalik
        if (synchronizeBatch && original.ReceivableId.HasValue)
        {
            await _batchStatusSynchronizer.SynchronizeByReceivableIdsAsync(new[] { original.ReceivableId.Value }, actorUserId, cancellationToken);
        }

        return reversal;
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

    // ====================================================================================
    // READ-MODEL KHUSUS: RECEIVABLE AR CANCELED (PAYMENT SESSION GRAIN)
    // ====================================================================================

    /// <summary>
    /// Memuat dan memfilter daftar sesi pembayaran AR yang dibatalkan.
    /// Grain: 1 payment session = 1 row (dikelompokkan berdasarkan ReceiptId + BatchId + OriginalPaymentAllocatedAt).
    /// </summary>
    private async Task<List<CanceledArPaymentRowResponse>> LoadFilteredCanceledSessionsAsync(
        CanceledArPaymentQuery request, CancellationToken cancellationToken)
    {
        var baseReversals = await (
            from rev in _dbContext.FinReceiptAllocations.AsNoTracking()
            join orig in _dbContext.FinReceiptAllocations.AsNoTracking() on rev.ReversalOfAllocationId equals orig.Id
            join receipt in _dbContext.FinReceipts.AsNoTracking() on orig.ReceiptId equals receipt.Id
            where !rev.IsDelete && rev.IsReversal && !orig.IsDelete && !receipt.IsDelete
            select new
            {
                ReversalId = rev.Id,
                ReversedAt = rev.AllocatedAt,
                ReversedBy = rev.AllocatedBy,
                OriginalId = orig.Id,
                ReceiptId = receipt.Id,
                ReceiptNumber = receipt.ReceiptNumber,
                ReceiptOccurredAt = receipt.OccurredAt,
                ReceiptCorrelationId = receipt.CorrelationId,
                ReceiptAmount = receipt.Amount,
                ReceiptUnallocatedAmount = receipt.UnallocatedAmount,
                ReceiptStatus = receipt.Status,
                ReceiptSourceType = receipt.SourceType,
                ReceiptBankAccountId = receipt.BankAccountId,
                ReceiptProviderReference = receipt.ProviderReference,
                ReceiptRowVersion = receipt.RowVersion,
                ReceivableId = orig.ReceivableId,
                AllocatedAmount = orig.Amount,
                OriginalAllocatedAt = orig.AllocatedAt
            }
        ).ToListAsync(cancellationToken);

        var distinctReceivableIds = baseReversals
            .Where(x => x.ReceivableId.HasValue)
            .Select(x => x.ReceivableId!.Value)
            .Distinct()
            .ToList();

        var batchItems = distinctReceivableIds.Count > 0
            ? await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                .Where(bi => distinctReceivableIds.Contains(bi.ReceivableId) && !bi.IsDelete)
                .Select(bi => new { bi.ReceivableId, bi.BatchId, bi.IsActiveMembership })
                .ToListAsync(cancellationToken)
            : [];

        var candidateBatchIds = batchItems.Select(x => x.BatchId)
            .Concat(baseReversals.Select(x => x.ReceiptCorrelationId))
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var batches = candidateBatchIds.Count > 0
            ? await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
                .Where(b => candidateBatchIds.Contains(b.Id) && !b.IsDelete)
                .ToDictionaryAsync(b => b.Id, cancellationToken)
            : new Dictionary<Guid, FinReceivableInvoiceBatch>();

        var groupedSessions = baseReversals
            .Select(r =>
            {
                Guid? resolvedBatchId = null;
                if (batches.ContainsKey(r.ReceiptCorrelationId))
                {
                    resolvedBatchId = r.ReceiptCorrelationId;
                }
                else if (r.ReceivableId.HasValue)
                {
                    var match = batchItems
                        .Where(bi => bi.ReceivableId == r.ReceivableId.Value && batches.ContainsKey(bi.BatchId))
                        .OrderByDescending(bi => bi.IsActiveMembership)
                        .FirstOrDefault();
                    if (match != null) resolvedBatchId = match.BatchId;
                }

                return new { Reversal = r, ResolvedBatchId = resolvedBatchId };
            })
            .GroupBy(x => new
            {
                ReceiptId = x.Reversal.ReceiptId,
                BatchId = x.ResolvedBatchId,
                AllocatedAt = x.Reversal.OriginalAllocatedAt
            })
            .ToList();

        var debtorIds = batches.Values.Select(b => b.DebtorReferenceId).Where(id => id != Guid.Empty).Distinct().ToList();
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

        var userIds = baseReversals.Select(r => r.ReversedBy).Where(u => u != Guid.Empty).Distinct().ToList();
        var userNames = userIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? "-", cancellationToken)
            : new Dictionary<Guid, string>();

        var originalAllocationIds = baseReversals.Select(r => r.OriginalId).Distinct().ToList();
        var movements = originalAllocationIds.Count > 0
            ? await _dbContext.FinReceivableMovements.AsNoTracking()
                .Where(m => m.SourceAllocationId.HasValue && originalAllocationIds.Contains(m.SourceAllocationId.Value) && !m.IsDelete)
                .Select(m => new { m.SourceAllocationId, m.ReceivableId, m.BalanceAfter, m.MovementType, m.Notes, m.OccurredAt, m.Id })
                .ToListAsync(cancellationToken)
            : [];

        var receivablesDict = distinctReceivableIds.Count > 0
            ? await _dbContext.FinReceivables.AsNoTracking()
                .Where(r => distinctReceivableIds.Contains(r.Id) && !r.IsDelete)
                .ToDictionaryAsync(r => r.Id, cancellationToken)
            : new Dictionary<Guid, FinReceivable>();

        var activeReplacements = await (
            from a in _dbContext.FinReceiptAllocations.AsNoTracking()
            where !a.IsDelete && !a.IsReversal && a.Amount > 0
                  && !_dbContext.FinReceiptAllocations.Any(rev => rev.ReversalOfAllocationId == a.Id && !rev.IsDelete)
            select new { a.ReceiptId, a.ReceivableId, a.AllocatedAt }
        ).ToListAsync(cancellationToken);

        var allRows = new List<CanceledArPaymentRowResponse>();
        foreach (var group in groupedSessions)
        {
            var representative = group.OrderBy(x => x.Reversal.ReversalId).First().Reversal;
            var batch = group.Key.BatchId.HasValue && batches.TryGetValue(group.Key.BatchId.Value, out var b) ? b : null;

            string companyName = "-";
            if (batch != null && batch.DebtorReferenceId != Guid.Empty)
            {
                if (companyNames.TryGetValue(batch.DebtorReferenceId, out var cName))
                    companyName = cName;
                else if (insuranceNames.TryGetValue(batch.DebtorReferenceId, out var insName))
                    companyName = insName;
            }

            string? visitType = null;
            if (batch != null)
            {
                var st = batch.ServiceType?.Trim().ToUpperInvariant() ?? "";
                visitType = st switch
                {
                    "OP" or "RAJAL" or "RAWAT JALAN" => "Rawat Jalan",
                    "IP" or "RANAP" or "RAWAT INAP" => "Rawat Inap",
                    "IGD" => "IGD",
                    _ => batch.ServiceType
                };
            }

            var paymentDate = representative.ReceiptOccurredAt != default
                ? representative.ReceiptOccurredAt
                : representative.OriginalAllocatedAt;

            decimal totalDiterima = group.Sum(x => x.Reversal.AllocatedAmount);
            decimal totalTagihan = batch?.TotalAmount ?? group.Sum(x =>
                x.Reversal.ReceivableId.HasValue && receivablesDict.TryGetValue(x.Reversal.ReceivableId.Value, out var rec)
                    ? rec.OriginalAmount
                    : x.Reversal.AllocatedAmount);

            var groupOrigIds = group.Select(x => x.Reversal.OriginalId).ToHashSet();
            var groupRecIds = group.Where(x => x.Reversal.ReceivableId.HasValue).Select(x => x.Reversal.ReceivableId!.Value).Distinct().ToList();

            decimal historicalRemaining = 0m;
            bool hasSubledger = false;
            foreach (var recId in groupRecIds)
            {
                var recMovements = movements.Where(m => m.ReceivableId == recId && m.SourceAllocationId.HasValue && groupOrigIds.Contains(m.SourceAllocationId.Value)).ToList();
                if (recMovements.Count > 0)
                {
                    hasSubledger = true;
                    var lastMovement = recMovements.OrderByDescending(m => m.OccurredAt).ThenByDescending(m => m.Id).First();
                    historicalRemaining += lastMovement.BalanceAfter;
                }
            }
            if (!hasSubledger)
            {
                historicalRemaining = Math.Max(0m, totalTagihan - totalDiterima);
            }

            string keterangan = "-";
            var movementNote = movements.FirstOrDefault(m => m.SourceAllocationId.HasValue && groupOrigIds.Contains(m.SourceAllocationId.Value) && !string.IsNullOrWhiteSpace(m.Notes))?.Notes;
            if (!string.IsNullOrWhiteSpace(movementNote))
                keterangan = movementNote;
            else if (!string.IsNullOrWhiteSpace(batch?.Note))
                keterangan = batch.Note;
            else if (!string.IsNullOrWhiteSpace(representative.ReceiptProviderReference))
                keterangan = representative.ReceiptProviderReference;

            var reversedAt = group.Max(x => x.Reversal.ReversedAt);
            var reversedBy = representative.ReversedBy;
            var reversedByName = userNames.TryGetValue(reversedBy, out var un) ? un : "-";

            var isReplaced = activeReplacements.Any(ar =>
                ar.ReceiptId == group.Key.ReceiptId &&
                groupRecIds.Contains(ar.ReceivableId ?? Guid.Empty) &&
                ar.AllocatedAt > reversedAt);

            allRows.Add(new CanceledArPaymentRowResponse
            {
                SessionId = representative.ReversalId,
                ReceiptId = representative.ReceiptId,
                ReceiptNumber = representative.ReceiptNumber,
                BatchId = group.Key.BatchId,
                InvoiceNumber = batch?.BatchNumber ?? "-",
                DebtorReferenceId = batch?.DebtorReferenceId,
                CompanyName = companyName,
                VisitType = visitType,
                PaymentDate = paymentDate,
                TotalTagihan = totalTagihan,
                TotalDiterima = totalDiterima,
                HistoricalRemaining = historicalRemaining,
                Keterangan = keterangan,
                Status = "Canceled",
                ReversedAt = reversedAt,
                ReversedBy = reversedBy,
                ReversedByName = reversedByName,
                FollowUpStatus = isReplaced ? "Sudah Diposting Ulang" : "Belum Diposting Ulang"
            });
        }

        var query = allRows.AsEnumerable();

        if (request.StartDate.HasValue)
        {
            var startDt = request.StartDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.PaymentDate.Date >= startDt.Date);
        }
        if (request.EndDate.HasValue)
        {
            var endDt = request.EndDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(x => x.PaymentDate.Date <= endDt.Date);
        }
        if (request.DebtorReferenceId.HasValue && request.DebtorReferenceId.Value != Guid.Empty)
        {
            query = query.Where(x => x.DebtorReferenceId == request.DebtorReferenceId.Value);
        }
        if (!string.IsNullOrWhiteSpace(request.VisitType))
        {
            var vt = request.VisitType.Trim();
            query = query.Where(x => !string.IsNullOrEmpty(x.VisitType) && (
                x.VisitType.Equals(vt, StringComparison.OrdinalIgnoreCase) ||
                (vt.Equals("Rawat Jalan", StringComparison.OrdinalIgnoreCase) && (x.VisitType.Contains("Jalan", StringComparison.OrdinalIgnoreCase) || x.VisitType.Equals("OP", StringComparison.OrdinalIgnoreCase))) ||
                (vt.Equals("Rawat Inap", StringComparison.OrdinalIgnoreCase) && (x.VisitType.Contains("Inap", StringComparison.OrdinalIgnoreCase) || x.VisitType.Equals("IP", StringComparison.OrdinalIgnoreCase))) ||
                (vt.Equals("IGD", StringComparison.OrdinalIgnoreCase) && x.VisitType.Equals("IGD", StringComparison.OrdinalIgnoreCase))
            ));
        }
        if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
        {
            var invPattern = request.InvoiceNumber.Trim();
            query = query.Where(x => x.InvoiceNumber.Contains(invPattern, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(x =>
                x.InvoiceNumber.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                x.CompanyName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                x.ReceiptNumber.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                x.Keterangan.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var sortBy = request.SortBy?.Trim().ToLowerInvariant() ?? "";
        query = sortBy switch
        {
            "invoicenumber" or "batchnumber" => descending ? query.OrderByDescending(x => x.InvoiceNumber) : query.OrderBy(x => x.InvoiceNumber),
            "companyname" or "debtorname" => descending ? query.OrderByDescending(x => x.CompanyName) : query.OrderBy(x => x.CompanyName),
            "totaltagihan" or "totalamount" => descending ? query.OrderByDescending(x => x.TotalTagihan) : query.OrderBy(x => x.TotalTagihan),
            "totalditerima" or "totalbayar" => descending ? query.OrderByDescending(x => x.TotalDiterima) : query.OrderBy(x => x.TotalDiterima),
            "historicalremaining" or "totalsisa" => descending ? query.OrderByDescending(x => x.HistoricalRemaining) : query.OrderBy(x => x.HistoricalRemaining),
            "reversedat" => descending ? query.OrderByDescending(x => x.ReversedAt) : query.OrderBy(x => x.ReversedAt),
            _ => descending ? query.OrderByDescending(x => x.PaymentDate) : query.OrderBy(x => x.PaymentDate)
        };

        return query.ToList();
    }

    /// <summary>
    /// Mengambil daftar Canceled AR Payment Sessions dengan paginasi server-side.
    /// </summary>
    public async Task<PagedResult<CanceledArPaymentRowResponse>> GetCanceledArPaymentSessionsAsync(
        CanceledArPaymentQuery request, CancellationToken cancellationToken)
    {
        var allSessions = await LoadFilteredCanceledSessionsAsync(request, cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var total = allSessions.Count;

        var items = allSessions
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<CanceledArPaymentRowResponse>
        {
            Items = items,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Mengambil ringkasan 4 metrik (Transaksi, Total Tagihan, Total Bayar, Total Sisa)
    /// yang dihitung berdasarkan SELURUH hasil filter aktif.
    /// </summary>
    public async Task<CanceledArPaymentSummaryResponse> GetCanceledArPaymentSummaryAsync(
        CanceledArPaymentQuery request, CancellationToken cancellationToken)
    {
        var allSessions = await LoadFilteredCanceledSessionsAsync(request, cancellationToken);
        return new CanceledArPaymentSummaryResponse
        {
            TransactionCount = allSessions.Count,
            TotalTagihan = allSessions.Sum(x => x.TotalTagihan),
            TotalBayar = allSessions.Sum(x => x.TotalDiterima),
            TotalSisa = allSessions.Sum(x => x.HistoricalRemaining)
        };
    }

    /// <summary>
    /// Mengambil rincian lengkap satu Canceled AR Payment Session berdasarkan sessionId (representative ReversalAllocationId).
    /// </summary>
    public async Task<CanceledArPaymentDetailResponse> GetCanceledArPaymentDetailAsync(
        Guid sessionId, CancellationToken cancellationToken)
    {
        var reversal = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .Include(x => x.Receipt)
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.IsReversal && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Sesi pembatalan pembayaran tidak ditemukan.");

        if (!reversal.ReversalOfAllocationId.HasValue)
            throw new KeyNotFoundException("Alokasi asal tidak ditemukan.");

        var origAllocation = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == reversal.ReversalOfAllocationId.Value && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Alokasi original tidak ditemukan.");

        var receipt = await _dbContext.FinReceipts.AsNoTracking()
            .Include(x => x.BankAccount)
            .FirstOrDefaultAsync(x => x.Id == origAllocation.ReceiptId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Penerimaan (Receipt) tidak ditemukan.");

        var siblingOriginals = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .Where(x => x.ReceiptId == origAllocation.ReceiptId
                     && x.AllocatedAt == origAllocation.AllocatedAt
                     && !x.IsReversal
                     && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var siblingOrigIds = siblingOriginals.Select(x => x.Id).ToList();
        var siblingReversals = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .Where(x => x.ReversalOfAllocationId.HasValue && siblingOrigIds.Contains(x.ReversalOfAllocationId.Value) && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var receivableIds = siblingOriginals.Where(x => x.ReceivableId.HasValue).Select(x => x.ReceivableId!.Value).Distinct().ToList();
        FinReceivableInvoiceBatch? batch = null;

        if (receipt.CorrelationId != Guid.Empty)
        {
            batch = await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == receipt.CorrelationId && !b.IsDelete, cancellationToken);
        }

        if (batch == null && receivableIds.Count > 0)
        {
            var batchItem = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                .Where(bi => receivableIds.Contains(bi.ReceivableId) && !bi.IsDelete)
                .OrderByDescending(bi => bi.IsActiveMembership)
                .FirstOrDefaultAsync(cancellationToken);

            if (batchItem != null)
            {
                batch = await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == batchItem.BatchId && !b.IsDelete, cancellationToken);
            }
        }

        string companyName = "-";
        if (batch != null && batch.DebtorReferenceId != Guid.Empty)
        {
            var comp = await _dbContext.MstCompanyGuarantors.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == batch.DebtorReferenceId && !c.IsDelete, cancellationToken);
            if (comp != null)
            {
                companyName = comp.CompanyGuarantorName;
            }
            else
            {
                var ins = await _dbContext.MstInsuranceProviders.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == batch.DebtorReferenceId && !p.IsDelete, cancellationToken);
                if (ins != null) companyName = ins.InsuranceProviderName;
            }
        }

        string reversedByName = "-";
        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == reversal.AllocatedBy, cancellationToken);
        if (user != null)
        {
            reversedByName = user.DisplayName ?? user.UserName ?? "-";
        }

        var crossEntry = await _dbContext.FinCrossEntries.AsNoTracking()
            .Include(x => x.BankAccount)
            .FirstOrDefaultAsync(x => x.ReceiptId == receipt.Id && !x.IsDelete, cancellationToken);

        string paymentSource = crossEntry != null ? "AYAT_SILANG" : "CASH_BANK";

        // Bank info resolution
        Guid? bankAccountId = null;
        string? bankAccountNumber = null;
        string? bankAccountName = null;
        string? bankName = null;

        if (crossEntry != null)
        {
            bankAccountId = crossEntry.BankAccountId;
            bankAccountNumber = crossEntry.BankAccount?.AccountNumber;
            bankAccountName = crossEntry.BankAccount?.AccountName;
            if (crossEntry.BankAccount != null && crossEntry.BankAccount.BankId != Guid.Empty)
            {
                var bank = await _dbContext.MstBanks.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == crossEntry.BankAccount.BankId && !b.IsDelete, cancellationToken);
                bankName = bank?.BankName;
            }
        }
        else if (receipt.BankAccountId.HasValue)
        {
            bankAccountId = receipt.BankAccountId.Value;
            bankAccountNumber = receipt.BankAccount?.AccountNumber;
            bankAccountName = receipt.BankAccount?.AccountName;
            if (receipt.BankAccount != null && receipt.BankAccount.BankId != Guid.Empty)
            {
                var bank = await _dbContext.MstBanks.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == receipt.BankAccount.BankId && !b.IsDelete, cancellationToken);
                bankName = bank?.BankName;
            }
        }

        decimal totalDiterima = siblingOriginals.Sum(x => x.Amount);
        decimal totalTagihan = batch?.TotalAmount ?? 0m;

        int sequenceNumber = 1;
        if (batch != null && receivableIds.Count > 0)
        {
            var allBatchItemReceivableIds = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                .Where(bi => bi.BatchId == batch.Id && !bi.IsDelete)
                .Select(bi => bi.ReceivableId)
                .ToListAsync(cancellationToken);

            var allAllocations = await _dbContext.FinReceiptAllocations.AsNoTracking()
                .Where(a => a.ReceivableId.HasValue && allBatchItemReceivableIds.Contains(a.ReceivableId.Value) && !a.IsDelete)
                .Select(a => new { a.ReceiptId, a.AllocatedAt })
                .Distinct()
                .ToListAsync(cancellationToken);

            var distinctSessions = allAllocations
                .GroupBy(a => new { a.ReceiptId, a.AllocatedAt })
                .OrderBy(g => g.Key.AllocatedAt)
                .ToList();

            var currentIdx = distinctSessions.FindIndex(g => g.Key.ReceiptId == receipt.Id && g.Key.AllocatedAt == origAllocation.AllocatedAt);
            sequenceNumber = currentIdx >= 0 ? currentIdx + 1 : 1;
        }

        var movements = await _dbContext.FinReceivableMovements.AsNoTracking()
            .Where(m => m.SourceAllocationId.HasValue && siblingOrigIds.Contains(m.SourceAllocationId.Value) && !m.IsDelete)
            .ToListAsync(cancellationToken);

        decimal historicalRemaining = 0m;
        bool hasMovements = false;
        foreach (var recId in receivableIds)
        {
            var recMovements = movements.Where(m => m.ReceivableId == recId).ToList();
            if (recMovements.Count > 0)
            {
                hasMovements = true;
                var lastMovement = recMovements.OrderByDescending(m => m.OccurredAt).ThenByDescending(m => m.Id).First();
                historicalRemaining += lastMovement.BalanceAfter;
            }
        }
        if (!hasMovements)
        {
            historicalRemaining = Math.Max(0m, (batch?.TotalAmount ?? totalDiterima) - totalDiterima);
        }

        var originalDeductions = await _dbContext.FinReceiptDeductions.AsNoTracking()
            .Where(d => siblingOrigIds.Contains(d.ReceiptAllocationId) && !d.IsReversal && !d.IsDelete)
            .ToListAsync(cancellationToken);

        var deductionResponses = originalDeductions
            .GroupBy(d => d.DeductionType)
            .Select(g => new CanceledArPaymentDeductionResponse
            {
                DeductionType = g.Key,
                Amount = g.Sum(x => x.Amount),
                Reason = g.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Reason))?.Reason,
                ReferenceNumber = g.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ReferenceNumber))?.ReferenceNumber
            })
            .ToList();

        var activeReplacements = await (
            from a in _dbContext.FinReceiptAllocations.AsNoTracking()
            where a.ReceiptId == receipt.Id
                  && a.ReceivableId.HasValue
                  && receivableIds.Contains(a.ReceivableId.Value)
                  && !a.IsDelete
                  && !a.IsReversal
                  && a.Amount > 0
                  && a.AllocatedAt > reversal.AllocatedAt
                  && !_dbContext.FinReceiptAllocations.Any(rev => rev.ReversalOfAllocationId == a.Id && !rev.IsDelete)
            select a
        ).AnyAsync(cancellationToken);

        string followUpStatus = activeReplacements ? "Sudah Diposting Ulang" : "Belum Diposting Ulang";

        bool canReapply = true;
        string? reapplyBlockedReason = null;

        if (batch != null && batch.Status == "CANCELLED")
        {
            canReapply = false;
            reapplyBlockedReason = "Invoice AR ini sedang dibatalkan. Buat Ulang AR dari halaman Pembatalan Invoice AR terlebih dahulu sebelum pembayaran dapat diposting ulang.";
        }
        else if (receipt.Status == FinReceiptStatuses.Reversed)
        {
            canReapply = false;
            reapplyBlockedReason = "Penerimaan (Receipt) berstatus REVERSED.";
        }
        else if (receipt.UnallocatedAmount < totalDiterima)
        {
            canReapply = false;
            reapplyBlockedReason = $"Saldo penerimaan yang belum dialokasikan tidak mencukupi (Tersedia: Rp {receipt.UnallocatedAmount:N0}, Dibutuhkan: Rp {totalDiterima:N0}).";
        }
        else if (crossEntry != null && crossEntry.Status == FinCrossEntryStatuses.Cancelled)
        {
            canReapply = false;
            reapplyBlockedReason = "Ayat Silang terkait dalam status CANCELLED.";
        }
        else if (activeReplacements)
        {
            canReapply = false;
            reapplyBlockedReason = "Pembayaran ini sudah mempunyai alokasi pengganti.";
        }
        else if (receivableIds.Count == 0)
        {
            canReapply = false;
            reapplyBlockedReason = "Data historis tidak memiliki referensi piutang/invoice yang lengkap.";
        }

        var memberReceivables = await _dbContext.FinReceivables.AsNoTracking()
            .Where(r => receivableIds.Contains(r.Id) && !r.IsDelete)
            .ToListAsync(cancellationToken);

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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var memberResponses = new List<CanceledArPaymentMemberDetailResponse>();

        foreach (var orig in siblingOriginals)
        {
            if (!orig.ReceivableId.HasValue) continue;
            var r = memberReceivables.FirstOrDefault(x => x.Id == orig.ReceivableId.Value);
            if (r == null) continue;

            var itemsForRec = receivableItems.Where(i => i.ReceivableId == r.Id).ToList();
            Guid? invId = r.InvoiceId ?? itemsForRec.FirstOrDefault(i => i.InvoiceId.HasValue)?.InvoiceId;
            BilInvoice? inv = invId.HasValue && invoices.TryGetValue(invId.Value, out var foundInv) ? foundInv : null;

            Guid? encId = inv?.EncounterId ?? itemsForRec.FirstOrDefault(i => i.EncounterId.HasValue)?.EncounterId;
            RegPatientEncounter? enc = encId.HasValue && encounters.TryGetValue(encId.Value, out var foundEnc) ? foundEnc : null;

            Guid? patId = enc?.PatientId ?? itemsForRec.FirstOrDefault(i => i.PatientId.HasValue)?.PatientId;
            MstPatient? pat = patId.HasValue && patients.TryGetValue(patId.Value, out var foundPat) ? foundPat : null;

            decimal histRem = 0m;
            var recMovements = movements.Where(m => m.ReceivableId == r.Id && m.SourceAllocationId == orig.Id).ToList();
            if (recMovements.Count > 0)
            {
                var lastMovement = recMovements.OrderByDescending(m => m.OccurredAt).ThenByDescending(m => m.Id).First();
                histRem = lastMovement.BalanceAfter;
            }
            else
            {
                histRem = Math.Max(0m, r.OriginalAmount - orig.Amount);
            }

            string? dueDatePeriod = null;
            if (r.DueDate != default)
            {
                int dayDiff = today.DayNumber - r.DueDate.DayNumber;
                dueDatePeriod = dayDiff > 0 ? $"{dayDiff} hari lewat jatuh tempo" : $"{Math.Abs(dayDiff)} hari sebelum jatuh tempo";
            }

            DateOnly? invoiceDate = inv?.InvoiceDate.HasValue == true
                ? DateOnly.FromDateTime(inv.InvoiceDate.Value.DateTime)
                : (r.RecognizedAt != default ? DateOnly.FromDateTime(r.RecognizedAt.DateTime) : null);

            memberResponses.Add(new CanceledArPaymentMemberDetailResponse
            {
                ReceivableId = r.Id,
                ReceivableNumber = r.ReceivableNumber,
                PatientName = pat?.FullName ?? "-",
                MedicalRecordNumber = pat?.MedicalRecordNumber ?? "-",
                BillingNumber = inv?.InvoiceNumber ?? "-",
                RegistrationNumber = enc?.EncounterNumber ?? "-",
                InvoiceDate = invoiceDate,
                DueDate = r.DueDate != default ? r.DueDate : null,
                DueDatePeriod = dueDatePeriod,
                OriginalAmount = r.OriginalAmount,
                OriginalPaidAmount = orig.Amount,
                HistoricalRemainingAmount = histRem,
                CurrentOutstandingAmount = r.OutstandingAmount,
                CurrentAllocatedAmount = r.AllocatedAmount,
                WasFullyPaid = histRem <= 0m,
                Note = recMovements.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.Notes))?.Notes ?? batch?.Note
            });
        }

        string? visitTypeDetail = null;
        if (batch != null)
        {
            var st = batch.ServiceType?.Trim().ToUpperInvariant() ?? "";
            visitTypeDetail = st switch
            {
                "OP" or "RAJAL" or "RAWAT JALAN" => "Rawat Jalan",
                "IP" or "RANAP" or "RAWAT INAP" => "Rawat Inap",
                "IGD" => "IGD",
                _ => batch.ServiceType
            };
        }

        var paymentDateDetail = receipt.OccurredAt != default ? receipt.OccurredAt : origAllocation.AllocatedAt;
        var reversedAtDetail = siblingReversals.Count > 0 ? siblingReversals.Max(x => x.AllocatedAt) : reversal.AllocatedAt;

        return new CanceledArPaymentDetailResponse
        {
            SessionId = reversal.Id,
            ReceiptId = receipt.Id,
            ReceiptNumber = receipt.ReceiptNumber,
            BatchId = batch?.Id,
            InvoiceNumber = batch?.BatchNumber ?? "-",
            DebtorReferenceId = batch?.DebtorReferenceId,
            CompanyName = companyName,
            VisitType = visitTypeDetail,
            PaymentDate = paymentDateDetail,
            PaymentSequenceNumber = sequenceNumber,
            ReversedAt = reversedAtDetail,
            ReversedBy = reversal.AllocatedBy,
            ReversedByName = reversedByName,
            PaymentSource = paymentSource,
            BankAccountId = bankAccountId,
            BankAccountNumber = bankAccountNumber,
            BankAccountName = bankAccountName,
            BankName = bankName,
            CrossEntryId = crossEntry?.Id,
            CrossEntryNumber = crossEntry?.CrossEntryNumber,
            CrossEntryStatus = crossEntry?.Status,
            TotalTagihan = totalTagihan,
            TotalDiterima = totalDiterima,
            HistoricalRemaining = historicalRemaining,
            CurrentBatchStatus = batch?.Status,
            BatchCancelReason = batch?.CancelReason,
            CanReapply = canReapply,
            ReapplyBlockedReason = reapplyBlockedReason,
            ReceiptRowVersion = receipt.RowVersion,
            BatchRowVersion = batch?.RowVersion,
            Note = movements.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.Notes))?.Notes ?? batch?.Note ?? receipt.ProviderReference,
            ReceiptAmount = receipt.Amount,
            ReceiptUnallocatedAmount = receipt.UnallocatedAmount,
            TotalDiscount = batch?.TotalDiscount ?? 0m,
            OtherReceiptAmount = batch?.OtherReceiptAmount ?? 0m,
            FollowUpStatus = followUpStatus,
            Members = memberResponses,
            Deductions = deductionResponses
        };
    }

    /// <summary>
    /// Ekspor daftar Canceled AR Payment Sessions ke format file Excel (.xlsx).
    /// </summary>
    public async Task<byte[]> ExportCanceledArPaymentsExcelAsync(
        CanceledArPaymentQuery request, CancellationToken cancellationToken)
    {
        var rows = await LoadFilteredCanceledSessionsAsync(request, cancellationToken);
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Canceled AR Payments");

        worksheet.Cell(1, 1).Value = "Daftar Pembatalan Receivable AR";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        worksheet.Cell(2, 1).Value = $"Tanggal Ekspor: {DateTime.UtcNow:dd/MM/yyyy HH:mm} WIB | Total: {rows.Count} sesi";
        worksheet.Cell(2, 1).Style.Font.FontSize = 9;

        string[] headers = [
            "No", "No. Invoice", "Nama Perusahaan", "Tgl Pembayaran",
            "Total Tagihan", "Total Diterima", "Sisa Pembayaran", "Keterangan", "Status", "Dibatalkan Oleh"
        ];

        for (int col = 0; col < headers.Length; col++)
        {
            var cell = worksheet.Cell(4, col + 1);
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2E8F0");
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            int rowIdx = 5 + i;
            worksheet.Cell(rowIdx, 1).Value = i + 1;
            worksheet.Cell(rowIdx, 2).Value = r.InvoiceNumber;
            worksheet.Cell(rowIdx, 3).Value = r.CompanyName;
            worksheet.Cell(rowIdx, 4).Value = FinanceBusinessDate.ToBusinessTime(r.PaymentDate).ToString("dd/MM/yyyy");
            worksheet.Cell(rowIdx, 5).Value = r.TotalTagihan;
            worksheet.Cell(rowIdx, 5).Style.NumberFormat.Format = "#,##0";
            worksheet.Cell(rowIdx, 6).Value = r.TotalDiterima;
            worksheet.Cell(rowIdx, 6).Style.NumberFormat.Format = "#,##0";
            worksheet.Cell(rowIdx, 7).Value = r.HistoricalRemaining;
            worksheet.Cell(rowIdx, 7).Style.NumberFormat.Format = "#,##0";
            worksheet.Cell(rowIdx, 8).Value = r.Keterangan;
            worksheet.Cell(rowIdx, 9).Value = r.Status;
            worksheet.Cell(rowIdx, 10).Value = r.ReversedByName;

            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cell(rowIdx, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Ekspor daftar Canceled AR Payment Sessions ke format PDF (QuestPDF A4 Landscape).
    /// </summary>
    public async Task<byte[]> ExportCanceledArPaymentsPdfAsync(
        CanceledArPaymentQuery request, CancellationToken cancellationToken)
    {
        var rows = await LoadFilteredCanceledSessionsAsync(request, cancellationToken);
        var summary = new CanceledArPaymentSummaryResponse
        {
            TransactionCount = rows.Count,
            TotalTagihan = rows.Sum(x => x.TotalTagihan),
            TotalBayar = rows.Sum(x => x.TotalDiterima),
            TotalSisa = rows.Sum(x => x.HistoricalRemaining)
        };

        return CanceledArPaymentReportDocuments.BuildListPdf(rows, summary);
    }
}

/// <summary>Satu baris permintaan alokasi (BE-FIN-018, FR-FIN-040). ReceivableId kosong = INVOICE_DIRECT.
/// Deductions kosong/null = perilaku persis sebelum BE-FIN-040 (regresi nol, FIN-DES-048).</summary>
public sealed record AllocationLineRequest(Guid? ReceivableId, decimal Amount, IReadOnlyList<ReceiptDeductionLineRequest>? Deductions = null);

/// <summary>Satu baris potongan sisi penerimaan (BE-FIN-040, FIN-DES-048). DeductionType salah satu
/// dari FinReceiptDeductionTypes; OTHER selalu ditolak (FIN-VAL-137).</summary>
public sealed record ReceiptDeductionLineRequest(string DeductionType, decimal Amount, string? Reason, string? ReferenceNumber);

/// <summary>Hasil GetInvoiceBreakdownAsync (FR-FIN-035) — dua sisi Finance untuk satu tagihan.</summary>
public sealed record InvoiceCollectionBreakdown(
    Guid InvoiceId,
    int ReceiptCount,
    decimal NetReceiptAmount,
    int ReceivableCount,
    decimal TotalReceivableOriginalAmount,
    decimal TotalReceivableOutstandingAmount);
