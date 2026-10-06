using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Satu-satunya penulis FinAccountingEventOutbox (BE-FIN-011, FIN-DES-017). MUST NOT membuka,
/// commit, atau rollback transaksi sendiri — pemanggil WAJIB sudah berada di dalam transaksi
/// eksplisitnya sendiri (BeginTransactionAsync milik service pemanggil) dan memanggil
/// SaveChangesAsync-nya sendiri SESUDAH StageEventAsync, supaya fakta bisnis dan baris kejadian
/// tersimpan atau batal bersama dalam satu SaveChangesAsync/commit (FR-FIN-070). Melanggar ini
/// (memanggil BeginTransactionAsync/SaveChangesAsync di sini) adalah kesalahan yang paling mudah
/// terjadi menurut roadmap MVP-5.
///
/// PayloadJson dibangun DI SINI, bukan diterima mentah dari pemanggil, dari 12 field wajib
/// ACC-XMOD-0.2 + Components &amp; SubledgerBalance opsional — desain ini sengaja mengunci FR-FIN-073 (data pasien/DoctorId
/// tidak pernah ikut) di level tipe: pemanggil tidak diberi jalan untuk menyisipkan field bebas
/// ke payload sama sekali.
/// </summary>
public sealed class FinanceAccountingOutboxService
{
    private static readonly Regex AccountingPeriodRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);
    private readonly ApplicationDbContext _dbContext;

    public FinanceAccountingOutboxService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FinAccountingEventOutbox> StageEventAsync(AccountingOutboxEventRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        // LegalEntityId "Rujukan shared" (erd/data-dictionary.md §7.1) — Finance tidak punya
        // sumber badan hukum sendiri. Dipakai ulang, bukan diduplikasi: mekanisme MVP yang sama
        // yang sudah dipakai seluruh service Accounting (ACC-DEC-041/043) untuk kasus yang identik
        // ("badan hukum mana yang sedang disentuh" saat sumbernya sendiri tidak menyimpannya).
        var legalEntityId = await AccountingLegalEntityGuard.AmbilBadanHukumUtamaAsync(_dbContext, cancellationToken)
            ?? throw new AccountingOutboxException(
                "Badan hukum utama (IsDefault) tidak dapat ditentukan — kejadian Accounting tidak dapat dibuat. " +
                "Tetapkan tepat satu badan hukum utama pada master badan hukum sebelum fakta ini dapat diakui.");

        var sourceVersion = request.SourceVersion
            ?? await ResolveNextSourceVersionAsync(request.SourceTransactionId, request.EventTypeCode, cancellationToken);

        var eventNumber = GenerateEventNumber();
        var payloadJson = BuildPayloadJson(eventNumber, request, sourceVersion, legalEntityId);
        var componentsJson = request.Components is { Count: > 0 } ? JsonSerializer.Serialize(request.Components) : null;

        var entity = new FinAccountingEventOutbox
        {
            EventNumber = eventNumber,
            EventTypeCode = request.EventTypeCode,
            SourceModule = FinAccountingEventSourceModules.Finance,
            SourceTransactionId = request.SourceTransactionId,
            SourceVersion = sourceVersion,
            EventOccurredAt = request.EventOccurredAt,
            AccountingDate = request.AccountingDate,
            Amount = request.Amount,
            CurrencyCode = "IDR",
            LegalEntityId = legalEntityId,
            CorrelationId = request.CorrelationId,
            CausationId = request.CausationId,
            ComponentsJson = componentsJson,
            PayloadJson = payloadJson,
            // HELD_FOR_FINALIZATION dicabut oleh FIN-DEC-030 dan FIN-DES-033 (FIN-VAL-076 dicabut).
            // Penentuan perlakuan pra-finalisasi berpindah ke pemilihan EventTypeCode (BE-FIN-024).
            // Seluruh kejadian baru masuk dengan status PENDING.
            DeliveryStatus = FinAccountingEventDeliveryStatuses.Pending,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = request.ActorUserId
        };

        // Add saja — TIDAK SaveChangesAsync. Lihat ringkasan kelas.
        _dbContext.Set<FinAccountingEventOutbox>().Add(entity);
        return entity;
    }

    // FR-FIN-072: koreksi MUST memakai versi baru, bukan menimpa. Dihitung dari baris yang
    // sudah tersimpan untuk pasangan (SourceTransactionId, EventTypeCode) yang sama — aman dari
    // race karena pemanggil (mis. FinanceReceivableService.DecideAdjustmentAsync) sudah memegang
    // advisory lock atas piutang yang sama sebelum memanggil StageEventAsync. Bukan Count/Max/
    // Last+1 untuk NOMOR tampilan (itu tetap dilarang QBE-CODE-002/003); ini versi internal yang
    // baru terlihat pemanggil setelah dikunci.
    private async Task<string> ResolveNextSourceVersionAsync(string sourceTransactionId, string eventTypeCode, CancellationToken cancellationToken)
    {
        var versions = await _dbContext.Set<FinAccountingEventOutbox>().AsNoTracking()
            .Where(x => !x.IsDelete && x.SourceModule == FinAccountingEventSourceModules.Finance
                && x.SourceTransactionId == sourceTransactionId && x.EventTypeCode == eventTypeCode)
            .Select(x => x.SourceVersion)
            .ToListAsync(cancellationToken);
        var maxVersion = versions.Select(v => int.TryParse(v, out var parsed) ? parsed : 0).DefaultIfEmpty(0).Max();
        return (maxVersion + 1).ToString();
    }

    private static string BuildPayloadJson(string eventNumber, AccountingOutboxEventRequest request, string sourceVersion, Guid legalEntityId)
    {
        // Persis 12 field wajib ACC-XMOD-0.2 (integration-contract.md §5.2) + Components & SubledgerBalance opsional.
        // Sengaja tidak menerima field bebas dari pemanggil — lihat ringkasan kelas.
        // Pelurusan 1 (FIN-DES-058, FIN-VAL-139): bila tidak ada komponen, properti Components TIDAK BOLEH
        // muncul sama sekali di payload (bukan "Components": null).
        var payload = new Dictionary<string, object?>
        {
            ["EventNumber"] = eventNumber,
            ["EventTypeCode"] = request.EventTypeCode,
            ["SourceModule"] = FinAccountingEventSourceModules.Finance,
            ["SourceTransactionId"] = request.SourceTransactionId,
            ["SourceVersion"] = sourceVersion,
            ["EventOccurredAt"] = request.EventOccurredAt,
            ["AccountingDate"] = request.AccountingDate,
            ["Amount"] = request.Amount,
            ["CurrencyCode"] = "IDR",
            ["LegalEntityId"] = legalEntityId,
            ["CorrelationId"] = request.CorrelationId,
            ["CausationId"] = request.CausationId,
            // BE-FIN-069, FIN-INTEGRATION-1.7 §5.12.1-5.12.2: lima ruas dimensi, selalu hadir
            // (termasuk null) — bukan conditional seperti Components/SubledgerBalance di bawah,
            // karena Accounting membacanya lewat AdditionalFields [JsonExtensionData] yang tidak
            // mensyaratkan field ini pernah absen.
            ["CashierShiftId"] = request.CashierShiftId,
            ["CashierShiftNumber"] = request.CashierShiftNumber,
            ["PaymentMethodCode"] = request.PaymentMethodCode,
            ["PaymentMethodAccountId"] = request.PaymentMethodAccountId,
            ["ReversalOfSourceTransactionId"] = request.ReversalOfSourceTransactionId
        };

        if (request.Components is { Count: > 0 })
        {
            payload["Components"] = request.Components;
        }

        if (request.SubledgerBalance != null)
        {
            payload["SubledgerBalance"] = new
            {
                request.SubledgerBalance.AccountingPeriodCode,
                request.SubledgerBalance.ControlAccountCode
            };
        }

        return JsonSerializer.Serialize(payload);
    }

    private static void ValidateRequest(AccountingOutboxEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventTypeCode))
            throw new AccountingOutboxException("EventTypeCode wajib diisi.");

        if (string.IsNullOrWhiteSpace(request.SourceTransactionId))
            throw new AccountingOutboxException("SourceTransactionId wajib diisi.");

        var isSaldoSubledger = string.Equals(request.EventTypeCode, FinAccountingEventTypeCodes.SaldoSubledger, StringComparison.OrdinalIgnoreCase);

        // Aturan Nilai (FIN-VAL-079, FIN-VAL-138, FIN-DES-054, FIN-DES-058, FIN-DEC-091, ACC-DEC-109)
        if (request.Amount < 0)
        {
            throw new AccountingOutboxException("Nominal kejadian harus lebih dari nol.");
        }

        if (request.Amount == 0)
        {
            if (!FinAccountingEventTypeCodes.ZeroAmountAllowedEventTypes.Contains(request.EventTypeCode))
            {
                throw new AccountingOutboxException("Nominal kejadian harus lebih dari nol.");
            }
        }

        // Aturan SubledgerBalance (FIN-VAL-081, FIN-VAL-082, FIN-VAL-083, FIN-VAL-084, FIN-DEC-092, ACC-DEC-110)
        if (!isSaldoSubledger)
        {
            if (request.SubledgerBalance != null)
            {
                throw new AccountingOutboxException("Rincian saldo subledger hanya berlaku untuk pesan saldo.");
            }
        }
        else
        {
            if (request.SubledgerBalance == null ||
                string.IsNullOrWhiteSpace(request.SubledgerBalance.AccountingPeriodCode) ||
                string.IsNullOrWhiteSpace(request.SubledgerBalance.ControlAccountCode))
            {
                throw new AccountingOutboxException("Pesan saldo subledger wajib menyebutkan periode dan akun kontrolnya.");
            }

            if (request.SubledgerBalance.AccountingPeriodCode.Length > 7 ||
                !AccountingPeriodRegex.IsMatch(request.SubledgerBalance.AccountingPeriodCode))
            {
                throw new AccountingOutboxException("Kode periode harus berbentuk tahun-bulan, contoh 2026-11.");
            }

            var periodParts = request.SubledgerBalance.AccountingPeriodCode.Split('-');
            var periodYear = int.Parse(periodParts[0]);
            var periodMonth = int.Parse(periodParts[1]);
            var expectedLastDate = FinanceBusinessDate.GetPeriodEndDate(periodYear, periodMonth);

            if (request.AccountingDate != expectedLastDate)
            {
                throw new AccountingOutboxException(
                    $"AccountingDate untuk pesan saldo periode {request.SubledgerBalance.AccountingPeriodCode} " +
                    $"wajib tanggal akhir periode ({expectedLastDate:yyyy-MM-dd}).");
            }

            if (request.SubledgerBalance.ControlAccountCode.Length > 50)
            {
                throw new AccountingOutboxException("Kode akun kontrol melebihi panjang maksimal 50 karakter.");
            }

            if (request.SourceVersion != null && (!int.TryParse(request.SourceVersion, out var version) || version <= 0))
            {
                throw new AccountingOutboxException("Pernyataan ulang saldo harus memakai versi yang lebih baru.");
            }
        }
    }

    // EventNumber tidak punya format baku pada kontrak yang terkunci — dibuat unik lewat Guid,
    // bukan Count/Max/Last+1 (QBE-CODE-002/003), sampai ada keputusan skema penomoran resmi.
    private static string GenerateEventNumber()
    {
        var candidate = $"EVT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }
}

/// <summary>Parameter StageEventAsync. SourceVersion null berarti "hitung otomatis" (lihat
/// ResolveNextSourceVersionAsync) — dipakai pemanggil yang tidak melacak versi sendiri.</summary>
public sealed class AccountingOutboxEventRequest
{
    public required string EventTypeCode { get; init; }
    public required string SourceTransactionId { get; init; }
    public string? SourceVersion { get; init; }
    public DateTimeOffset EventOccurredAt { get; init; }
    public DateOnly AccountingDate { get; init; }
    public decimal Amount { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid CausationId { get; init; }
    public IReadOnlyList<AccountingEventComponent>? Components { get; init; }
    public SubledgerBalanceRequest? SubledgerBalance { get; init; }
    public Guid ActorUserId { get; init; }

    /// <summary>BE-FIN-069, FIN-INTEGRATION-1.7 §5.12.1: terisi pada kejadian penerimaan kasir dan pembaliknya.</summary>
    public Guid? CashierShiftId { get; init; }

    /// <summary>BE-FIN-069, FIN-INTEGRATION-1.7 §5.12.1: rujukan shift yang terbaca manusia, pasangan <see cref="CashierShiftId"/>.</summary>
    public string? CashierShiftNumber { get; init; }

    /// <summary>BE-FIN-069, FIN-INTEGRATION-1.7 §5.12.1: terisi pada penerimaan kasir, penerimaan piutang, dan pembayaran utang supplier.</summary>
    public string? PaymentMethodCode { get; init; }

    /// <summary>BE-FIN-069, FIN-INTEGRATION-1.7 §5.12.1: rekening sumber/tujuan dana non-tunai, pasangan <see cref="PaymentMethodCode"/>.</summary>
    public Guid? PaymentMethodAccountId { get; init; }

    /// <summary>BE-FIN-069, FIN-INTEGRATION-1.7 §5.12.2: diisi hanya pada kejadian pembalik — SourceTransactionId kuitansi ASLI, bukan shift asli.</summary>
    public string? ReversalOfSourceTransactionId { get; init; }
}

/// <summary>
/// Parameter rincian saldo subledger khusus kejadian SALDO-SUBLEDGER (FIN-DES-032, FIN-VAL-081, FIN-VAL-082).
/// </summary>
public sealed class SubledgerBalanceRequest
{
    /// <summary>
    /// Periode akuntansi berbentuk YYYY-MM (maksimal 7 karakter).
    /// </summary>
    public required string AccountingPeriodCode { get; init; }

    /// <summary>
    /// Kode akun kontrol milik Accounting (maksimal 50 karakter).
    /// </summary>
    public required string ControlAccountCode { get; init; }
}

public sealed record AccountingEventComponent(string ComponentCode, decimal Amount);

public sealed class AccountingOutboxException(string message) : Exception(message);
