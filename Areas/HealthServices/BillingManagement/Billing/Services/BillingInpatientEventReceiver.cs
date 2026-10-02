using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Penerima ketukan pintu Rawat Inap di dalam aplikasi — <c>BE-RWI-150</c>, kontrak
/// <c>integrasi-billing</c> <c>1.1.0</c> API 3.10 dan integrasi 4.2 (<c>INT-RWF-01</c>).
/// </summary>
/// <remarks>
/// <para>
/// <b>Bukan endpoint HTTP</b> (kontrak 9.12). Dipanggil <c>InpatientIntegrationOutboxWorker</c> dalam
/// proses yang sama, sehingga tidak ada pintu publik pengubah data yang dapat dipanggil dari luar.
/// </para>
/// <para>
/// <b>Satu transaksi Billing per pesan.</b> Pembukaan invoice <c>RANAP</c>, versi hitungan baru,
/// dan tanda terima ditulis bersama. Bila salah satu gagal, seluruhnya batal dan <b>tidak ada</b>
/// tanda terima yang tersimpan, sehingga worker mencoba ulang (kontrak integrasi 4.2 baris
/// "Billing melempar kesalahan").
/// </para>
/// <para>
/// <b>Idempotensi.</b> Kunci idempotensi pesan disimpan unik pada <c>BilInpatientEventReceipt</c>.
/// Pesan yang sama dikirim ulang dijawab <c>DUPLICATE</c> dengan <c>Accepted = true</c> tanpa efek
/// kedua (<c>INV-RWF-06</c>).
/// </para>
/// <para>
/// <b>Urutan pengiriman tidak dijamin.</b> Event tempat tidur yang datang sebelum
/// <c>ADMISSION_CONFIRMED</c> tetap membuka invoice lebih dulu.
/// </para>
/// </remarks>
public sealed class BillingInpatientEventReceiver
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing.InpatientEventReceiver";

    private readonly ApplicationDbContext _dbContext;
    private readonly BillingInvoiceService _invoiceService;
    private readonly BillingCalculationService _calculationService;
    private readonly LoggerService _loggerService;

    public BillingInpatientEventReceiver(
        ApplicationDbContext dbContext,
        BillingInvoiceService invoiceService,
        BillingCalculationService calculationService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _invoiceService = invoiceService;
        _calculationService = calculationService;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Memproses satu ketukan pintu dan mengembalikan tanda terima.
    /// </summary>
    /// <exception cref="ArgumentException">Isi pesan tidak lengkap atau jenis event tidak dikenal.</exception>
    /// <remarks>
    /// Kegagalan teknis (database, kalkulasi) dilempar ke pemanggil apa adanya supaya worker
    /// mencatatnya <c>Failed</c> dan mencoba ulang dengan backoff.
    /// </remarks>
    public async Task<InpatientEventReceipt> ReceiveAsync(
        InpatientBillingEventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        Validate(envelope);

        var idempotencyKey = envelope.IdempotencyKey.Trim();
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction is null)
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted, cancellationToken);
            }

            if (_dbContext.Database.IsRelational())
            {
                // Pesan kembar yang diproses dua worker bersamaan menunggu di sini, lalu yang
                // kedua menemukan tanda terima milik yang pertama.
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));",
                    [$"BIL_INP_EVENT_{idempotencyKey}"],
                    cancellationToken);
            }

            var prior = await _dbContext.BilInpatientEventReceipts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey && !x.IsDelete, cancellationToken);
            if (prior is not null)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return new InpatientEventReceipt
                {
                    Accepted = true,
                    ReceiptId = prior.Id,
                    Outcome = BillingInpatientEventOutcomes.Duplicate,
                    Message = $"Pesan sudah diterima sebelumnya dengan hasil {prior.Outcome}."
                };
            }

            var encounterExists = await _dbContext.RegPatientEncounters.AsNoTracking()
                .AnyAsync(x => x.Id == envelope.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken);
            if (!encounterExists)
            {
                // Tidak ada tanda terima yang disimpan: pesan ditolak, worker mencatatnya Failed
                // lalu DeadLetter (kontrak state 5.6).
                if (transaction is not null) await transaction.RollbackAsync(cancellationToken);
                await _loggerService.WarningAsync(LogCategory, "InpatientEvent.Rejected",
                    "Ketukan pintu Rawat Inap ditolak: kunjungan tidak dikenal Billing.", new
                    {
                        envelope.EventType,
                        envelope.EpisodeId,
                        envelope.EncounterId,
                        IdempotencyKey = idempotencyKey
                    });
                return new InpatientEventReceipt
                {
                    Accepted = false,
                    ReceiptId = Guid.Empty,
                    Outcome = BillingInpatientEventOutcomes.RejectedUnknownEncounter,
                    Message = "Kunjungan tidak dikenal Billing."
                };
            }

            var (invoice, created) = await _invoiceService.OpenInpatientInvoiceAsync(
                envelope.EncounterId, Guid.Empty, cancellationToken);

            string outcome;
            int? calculationVersionNo = null;
            string? message = null;

            if (string.Equals(envelope.EventType, InpatientBillingEventTypes.AdmissionConfirmed, StringComparison.Ordinal))
            {
                outcome = created
                    ? BillingInpatientEventOutcomes.InvoiceOpened
                    : BillingInpatientEventOutcomes.InvoiceAlreadyOpen;
            }
            else
            {
                outcome = BillingInpatientEventOutcomes.Recalculated;
                if (string.Equals(invoice.Status, BillingInvoiceStatuses.Open, StringComparison.Ordinal))
                {
                    var calculation = await _calculationService.RecalculateAsync(
                        invoice.Id,
                        new RecalculateInvoiceRequest
                        {
                            ExpectedRowVersion = invoice.RowVersion,
                            Reason = $"Hitung ulang tarif kamar dari ketukan pintu Rawat Inap {envelope.EventType}."
                        },
                        Guid.Empty,
                        cancellationToken);
                    calculationVersionNo = calculation.VersionNo;
                    if (created)
                        message = "Invoice RANAP dibuka lebih dulu karena event tempat tidur tiba sebelum ADMISSION_CONFIRMED.";
                }
                else
                {
                    // Invoice yang sudah final atau ditutup tidak dihitung ulang dari sini;
                    // perubahan sesudah final memakai adjustment Billing (RWI-DEC-195 butir 4).
                    message = $"Invoice berstatus {invoice.Status}; hitung ulang tarif kamar tidak dijalankan.";
                }
            }

            var receipt = new BilInpatientEventReceipt
            {
                IdempotencyKey = idempotencyKey,
                EventType = envelope.EventType,
                EpisodeId = envelope.EpisodeId,
                EncounterId = envelope.EncounterId,
                SourceId = envelope.SourceId,
                SourceVersion = envelope.Version,
                OccurredAtUtc = DateTime.SpecifyKind(envelope.OccurredAtUtc, DateTimeKind.Utc),
                ReceivedAtUtc = DateTime.UtcNow,
                Outcome = outcome,
                InvoiceId = invoice.Id,
                CalculationVersionNo = calculationVersionNo,
                Message = message,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Guid.Empty
            };
            _dbContext.BilInpatientEventReceipts.Add(receipt);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "InpatientEvent.Received",
                "Ketukan pintu Rawat Inap diterima Billing.", new
                {
                    ReceiptId = receipt.Id,
                    receipt.EventType,
                    receipt.Outcome,
                    receipt.InvoiceId,
                    receipt.CalculationVersionNo,
                    IdempotencyKey = idempotencyKey
                });

            return new InpatientEventReceipt
            {
                Accepted = true,
                ReceiptId = receipt.Id,
                Outcome = outcome,
                Message = message
            };
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private static void Validate(InpatientBillingEventEnvelope envelope)
    {
        if (string.IsNullOrWhiteSpace(envelope.IdempotencyKey))
            throw new ArgumentException("Kunci idempotensi ketukan pintu wajib diisi.");
        if (!InpatientBillingEventTypes.All.Contains(envelope.EventType ?? string.Empty))
            throw new ArgumentException($"Jenis event Rawat Inap tidak dikenal: {envelope.EventType}.");
        if (!InpatientBillingEventSourceTypes.All.Contains(envelope.SourceType ?? string.Empty))
            throw new ArgumentException($"SourceType ketukan pintu tidak dikenal: {envelope.SourceType}.");
        if (envelope.EncounterId == Guid.Empty || envelope.EpisodeId == Guid.Empty)
            throw new ArgumentException("EpisodeId dan EncounterId ketukan pintu wajib diisi.");
        if (string.IsNullOrWhiteSpace(envelope.SourceId))
            throw new ArgumentException("SourceId ketukan pintu wajib diisi.");
    }
}
