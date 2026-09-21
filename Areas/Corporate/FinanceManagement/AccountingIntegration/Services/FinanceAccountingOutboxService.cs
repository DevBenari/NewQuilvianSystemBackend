using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

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
/// ACC-XMOD-0.2 + Components saja — desain ini sengaja mengunci FR-FIN-073 (data pasien/DoctorId
/// tidak pernah ikut) di level tipe: pemanggil tidak diberi jalan untuk menyisipkan field bebas
/// ke payload sama sekali.
/// </summary>
public sealed class FinanceAccountingOutboxService
{
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
        var payloadJson = BuildPayloadJson(eventNumber, request, sourceVersion, legalEntityId.Value);
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
            LegalEntityId = legalEntityId.Value,
            CorrelationId = request.CorrelationId,
            CausationId = request.CausationId,
            ComponentsJson = componentsJson,
            PayloadJson = payloadJson,
            // HELD_FOR_FINALIZATION hanya berlaku untuk kejadian penerimaan sebelum tagihan final
            // (FIN-DES-018, accounting-integration.md §4) — belum ada pemanggil nyata pada task ini
            // karena FinReceipt/Collection masih BLOCKED (roadmap MVP-2/3). Parameter tetap
            // disediakan supaya pemanggil di masa depan tidak perlu mengubah tanda tangan ini.
            DeliveryStatus = request.RequiresFinalization
                ? FinAccountingEventDeliveryStatuses.HeldForFinalization
                : FinAccountingEventDeliveryStatuses.Pending,
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
        // Persis 12 field wajib ACC-XMOD-0.2 (integration-contract.md §5.2) + Components opsional.
        // Sengaja tidak menerima field bebas dari pemanggil — lihat ringkasan kelas.
        var payload = new
        {
            EventNumber = eventNumber,
            request.EventTypeCode,
            SourceModule = FinAccountingEventSourceModules.Finance,
            request.SourceTransactionId,
            SourceVersion = sourceVersion,
            request.EventOccurredAt,
            request.AccountingDate,
            request.Amount,
            CurrencyCode = "IDR",
            LegalEntityId = legalEntityId,
            request.CorrelationId,
            request.CausationId,
            Components = request.Components
        };
        return JsonSerializer.Serialize(payload);
    }

    private static void ValidateRequest(AccountingOutboxEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventTypeCode)) throw new AccountingOutboxException("EventTypeCode wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.SourceTransactionId)) throw new AccountingOutboxException("SourceTransactionId wajib diisi.");
        if (request.Amount <= 0) throw new AccountingOutboxException("Amount kejadian harus lebih dari nol.");
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
    public bool RequiresFinalization { get; init; }
    public Guid ActorUserId { get; init; }
}

public sealed record AccountingEventComponent(string ComponentCode, decimal Amount);

public sealed class AccountingOutboxException(string message) : Exception(message);
