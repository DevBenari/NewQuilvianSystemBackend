using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Repositories;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Worker pengiriman baris <see cref="FinAccountingEventOutbox"/> berstatus PENDING ke kotak masuk
/// Accounting (BE-FIN-071, FIN-DES-078, FR-FIN-150..153).
///
/// KESELAMATAN PENGIRIMAN — DUA LAPIS GERBANG (FIN-DES-078):
///
///   Lapis 1 — konfigurasi: <see cref="FinanceAccountingDispatchWorkerOptions.Enabled"/> harus true.
///             Nilai bawaannya FALSE — dibangun mati. Tanpa pengubahan eksplisit konfigurasi,
///             nol baris dikirim, nol thread HTTP dibuka, dan nol percobaan dicatat.
///
///   Lapis 2 — kode: kode tertentu dilewati WALAUPUN Lapis 1 sudah aktif:
///             * Ketiga penanda shift (PENUTUPAN-, PEMBALIKAN-PENUTUPAN-, PEMBUKAAN-SHIFT-KASIR)
///               — menunggu FIN-OQ-035 dan FIN-OQ-047 turun dari Accounting.
///             * Kode yang belum diratifikasi: PPN-MASUKAN-PEMBELIAN (FIN-OQ-020),
///               POTONGAN-PPH23/BIAYA-BANK (FIN-OQ-028).
///             Baris yang dilewati TETAP PENDING — AttemptCount tidak bertambah dan
///             LastAttemptAt tidak diperbarui — sehingga tidak akan ditandai FAILED oleh Lapis 1.
///
/// KREDENSIAL — MUST dari konfigurasi (G3, FIN-DES-078). MUST NOT ditanamkan di source.
///
/// IDEMPOTENSI — satu baris outbox diproses satu percobaan per siklus. Bila respons 200 atau 201,
/// baris ditandai ACKNOWLEDGED (evidence/16 bagian 5). Balasan lain dicatat sebagai percobaan
/// gagal; bila AttemptCount sudah mencapai MaxAttempts, baris ditandai FAILED.
///
/// TRANSAKSI — satu baris = satu transaksi = satu percobaan. Kegagalan satu baris TIDAK membatalkan
/// siklus; baris berikutnya tetap diproses (FIN-DEC-093).
/// </summary>
public sealed class FinanceAccountingDispatchWorker : BackgroundService
{
    private const int MinimumPollIntervalSeconds = 10;

    // Daftar tertutup kode yang DILEWATI worker ini (FIN-DES-078, lapis kedua).
    // MUST NOT dihapus ketika Lapis 1 dinyalakan.
    //
    // Penanda shift: menunggu G6 siap (FIN-OQ-035, FIN-OQ-047).
    // Kode belum diratifikasi: menunggu gerbang Accounting masing-masing.
    private static readonly IReadOnlySet<string> GatedEventTypeCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // Ketiga penanda shift (FIN-OQ-035, FIN-OQ-047)
        FinAccountingEventTypeCodes.PenutupanShiftKasir,
        FinAccountingEventTypeCodes.PembalikanPenutupanShiftKasir,
        FinAccountingEventTypeCodes.PembukaanShiftKasir,

        // Kode belum diratifikasi (FIN-OQ-020)
        FinAccountingEventTypeCodes.PpnMasukanPembelian,

        // Kode belum diratifikasi (FIN-OQ-028)
        FinAccountingEventTypeCodes.PotonganPph23Piutang,
        FinAccountingEventTypeCodes.PembalikanPotonganPph23Piutang,
        FinAccountingEventTypeCodes.PotonganBiayaBankPiutang,
        FinAccountingEventTypeCodes.PembalikanPotonganBiayaBankPiutang,
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FinanceAccountingDispatchWorkerOptions _options;
    private readonly ILogger<FinanceAccountingDispatchWorker> _logger;

    public FinanceAccountingDispatchWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<FinanceAccountingDispatchWorkerOptions> options,
        ILogger<FinanceAccountingDispatchWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Lapis 1: gerbang konfigurasi — nilai bawaan false = dibangun mati.
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Worker pengiriman kejadian Finance ke Accounting dinonaktifkan melalui konfigurasi " +
                "(Finance:AccountingDispatch:Enabled = false). Nol baris dikirim.");
            return;
        }

        // Validasi konfigurasi minimal sebelum mulai loop.
        if (string.IsNullOrWhiteSpace(_options.AccountingInboxUrl))
        {
            _logger.LogError(
                "Worker pengiriman Finance-Accounting berhenti: Finance:AccountingDispatch:AccountingInboxUrl " +
                "belum dikonfigurasi. MUST diisi dari konfigurasi lingkungan, BUKAN dari source code (G3).");
            return;
        }

        var jeda = Math.Max(MinimumPollIntervalSeconds, _options.PollIntervalSeconds);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(jeda));

        _logger.LogInformation(
            "Worker pengiriman Finance-Accounting aktif. Poll={Jeda}s, BatchSize={Batch}, MaxAttempts={MaxAttempts}, " +
            "GatedCodes={GatedCount} kode dilewati.",
            jeda, _options.BatchSize, _options.MaxAttempts, GatedEventTypeCodes.Count);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await JalankanSatuSiklusAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siklus pengiriman Finance-Accounting gagal secara keseluruhan.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task JalankanSatuSiklusAsync(CancellationToken ct)
    {
        // Baca kandidat pengiriman siklus ini (PENDING) — di luar transaksi, baca saja.
        List<Guid> candidateIds;
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            candidateIds = await db.FinAccountingEventOutboxes
                .AsNoTracking()
                .Where(x => !x.IsDelete
                    && x.DeliveryStatus == FinAccountingEventDeliveryStatuses.Pending
                    && !GatedEventTypeCodes.Contains(x.EventTypeCode))
                .OrderBy(x => x.EventOccurredAt)
                .Take(_options.BatchSize)
                .Select(x => x.Id)
                .ToListAsync(ct);
        }

        if (candidateIds.Count == 0) return;

        _logger.LogInformation(
            "Siklus pengiriman Finance-Accounting: {Count} baris kandidat.", candidateIds.Count);

        var sent = 0;
        var failed = 0;
        var skipped = 0;
        var errors = new List<string>();

        using var httpClient = BuatHttpClient();

        foreach (var outboxId in candidateIds)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                var hasilBaris = await ProsessSatuBarisAsync(outboxId, httpClient, ct);
                switch (hasilBaris)
                {
                    case HasilPrososBaris.Terkirim: sent++; break;
                    case HasilPrososBaris.Gagal: failed++; break;
                    case HasilPrososBaris.Dilewati: skipped++; break;
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                errors.Add($"OutboxId={outboxId}: {ex.Message}");
                _logger.LogWarning(ex, "Pengiriman baris outbox {OutboxId} melempar exception.", outboxId);
            }
        }

        if (sent + failed + skipped > 0)
        {
            _logger.LogInformation(
                "Siklus pengiriman Finance-Accounting selesai. Terkirim={Sent}, Gagal={Failed}, Dilewati={Skipped}, Galat={Errors}.",
                sent, failed, skipped, errors.Count);
        }

        foreach (var err in errors)
            _logger.LogWarning("Galat pengiriman baris: {Err}", err);
    }

    private async Task<HasilPrososBaris> ProsessSatuBarisAsync(
        Guid outboxId, HttpClient httpClient, CancellationToken ct)
    {
        // Setiap baris punya transaksinya sendiri agar kegagalan satu baris tidak membatalkan
        // yang lain (FIN-DEC-093). Scope baru per baris mengikuti pola AccAccountingEventSchedulerHostedService.
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Re-baca baris dalam transaksi — status bisa berubah sejak kandidat dibaca.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var outbox = await db.FinAccountingEventOutboxes
                .SingleOrDefaultAsync(x => x.Id == outboxId && !x.IsDelete, ct);

            // Baris tidak ada atau sudah diproses proses lain (race condition antar pod).
            if (outbox is null || outbox.DeliveryStatus != FinAccountingEventDeliveryStatuses.Pending)
                return HasilPrososBaris.Dilewati;

            // Cek ulang lapis dua: gerbang kode. Meskipun query kandidat sudah menyaring,
            // daftar GatedEventTypeCodes bisa berubah di kemudian hari; pemeriksaan di sini
            // menjaga invariant tanpa bergantung pada query kandidat.
            if (GatedEventTypeCodes.Contains(outbox.EventTypeCode))
                return HasilPrososBaris.Dilewati;

            // Sudah mencapai batas coba — tandai FAILED tanpa mengirim.
            if (outbox.AttemptCount >= _options.MaxAttempts)
            {
                outbox.DeliveryStatus = FinAccountingEventDeliveryStatuses.Failed;
                outbox.RowVersion = Guid.NewGuid();
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                _logger.LogWarning(
                    "Baris outbox {OutboxId} ({EventTypeCode}) ditandai FAILED setelah {MaxAttempts} percobaan.",
                    outboxId, outbox.EventTypeCode, _options.MaxAttempts);
                return HasilPrososBaris.Gagal;
            }

            // Kirim ke Accounting.
            var sw = Stopwatch.StartNew();
            int? responseCode = null;
            string? responseBody = null;
            string? errorMessage = null;
            bool sukses;

            try
            {
                var (code, body) = await KirimAsync(outbox, httpClient, ct);
                responseCode = code;
                responseBody = body?.Length > 2000 ? body[..2000] : body;
                // evidence/16 bagian 5: balasan 200 dan 201 sama-sama sukses.
                sukses = code is 200 or 201;
                if (!sukses) errorMessage = $"HTTP {code}";
            }
            catch (Exception ex)
            {
                sukses = false;
                errorMessage = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
            }

            sw.Stop();
            var durasiMs = (int)sw.ElapsedMilliseconds;

            // Catat percobaan.
            var percobaan = new FinAccountingEventAttempt
            {
                OutboxId = outboxId,
                AttemptNumber = outbox.AttemptCount + 1,
                AttemptedAt = DateTimeOffset.UtcNow,
                ResponseCode = responseCode,
                ResponseBody = responseBody,
                DurationMs = durasiMs,
                ErrorMessage = errorMessage
            };
            db.Set<FinAccountingEventAttempt>().Add(percobaan);

            // Perbarui status outbox.
            outbox.AttemptCount++;
            outbox.LastAttemptAt = percobaan.AttemptedAt;
            outbox.LastResponseCode = responseCode;
            outbox.RowVersion = Guid.NewGuid();

            if (sukses)
            {
                outbox.DeliveryStatus = FinAccountingEventDeliveryStatuses.Acknowledged;
                // evidence/16 bagian 5: AccountingReceiptNumber diisi dari AccountingEventId yang
                // dikembalikan Accounting dalam body respons (bila tersedia).
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(responseBody);
                        if (doc.RootElement.TryGetProperty("accountingEventId", out var idProp) ||
                            doc.RootElement.TryGetProperty("AccountingEventId", out idProp))
                        {
                            outbox.AccountingReceiptNumber = idProp.GetString();
                        }
                    }
                    catch
                    {
                        // Body tidak valid JSON atau tidak punya properti yang diharapkan — tidak masalah.
                    }
                }
            }
            else if (outbox.AttemptCount >= _options.MaxAttempts)
            {
                outbox.DeliveryStatus = FinAccountingEventDeliveryStatuses.Failed;
            }
            // else: tetap PENDING, akan dicoba siklus berikutnya.

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            if (sukses)
            {
                _logger.LogInformation(
                    "Baris outbox {OutboxId} ({EventTypeCode}) berhasil dikirim dalam {DurasiMs}ms. " +
                    "ReceiptNumber={ReceiptNumber}.",
                    outboxId, outbox.EventTypeCode, durasiMs, outbox.AccountingReceiptNumber ?? "(kosong)");
                return HasilPrososBaris.Terkirim;
            }

            _logger.LogWarning(
                "Baris outbox {OutboxId} ({EventTypeCode}) gagal percobaan ke-{Attempt}/{MaxAttempts}. " +
                "Galat={Galat}. Status={Status}.",
                outboxId, outbox.EventTypeCode, outbox.AttemptCount, _options.MaxAttempts,
                errorMessage, outbox.DeliveryStatus);

            return HasilPrososBaris.Gagal;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task<(int StatusCode, string? Body)> KirimAsync(
        FinAccountingEventOutbox outbox, HttpClient httpClient, CancellationToken ct)
    {
        // Payload yang dikirim adalah PayloadJson yang sudah disimpan di waktu penulisan
        // (FIN-DES-017: salinan persis pesan yang akan dikirim). Tidak ada transformasi di sini.
        var content = new StringContent(outbox.PayloadJson, Encoding.UTF8, "application/json");
        using var response = await httpClient.PostAsync(string.Empty, content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        return ((int)response.StatusCode, body);
    }

    private HttpClient BuatHttpClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(_options.AccountingInboxUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(Math.Max(5, _options.HttpTimeoutSeconds))
        };

        if (!string.IsNullOrWhiteSpace(_options.AccountingInboxApiKey))
        {
            // Mengirim sebagai Bearer token mengikuti pola yang sudah ada pada modul lain.
            // Bila Accounting menuntut header lain (misal X-Api-Key), sesuaikan di sini —
            // JANGAN menaruh nilai kuncinya di source; ambil dari _options.
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.AccountingInboxApiKey);
        }

        return client;
    }

    private enum HasilPrososBaris { Terkirim, Gagal, Dilewati }
}
