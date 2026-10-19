using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
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
                // Replay juga menutup tautan yang terlewat sebelum I6, memakai receipt asli.
                if (prior.EventType == InpatientBillingEventTypes.AdmissionConfirmed && prior.InvoiceId.HasValue)
                {
                    var targetEncounterId = prior.EncounterId != Guid.Empty ? prior.EncounterId : envelope.EncounterId;
                    await EnsureEncounterLinksAsync(prior.EpisodeId, targetEncounterId, prior.InvoiceId.Value, prior.Id, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
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
            if (envelope.EventType == InpatientBillingEventTypes.AdmissionConfirmed)
                await EnsureEncounterLinksAsync(envelope.EpisodeId, envelope.EncounterId, invoice.Id, receipt.Id, cancellationToken);
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
        if (!Guid.TryParse(envelope.SourceId, out var sourceId) || sourceId == Guid.Empty || envelope.Version < 1
            || envelope.IdempotencyKey != $"INPATIENT:{envelope.SourceType}:{sourceId}:{envelope.Version}")
            throw new ArgumentException("Identitas atau kunci idempotensi ketukan pintu tidak sesuai kontrak.");
    }

    private async Task EnsureEncounterLinksAsync(Guid episodeId, Guid encounterId, Guid invoiceId, Guid receiptId, CancellationToken ct)
    {
        if (_dbContext.Database.IsRelational())
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"BIL_INP_LINK_{invoiceId:N}"], ct);

        var referrals = await _dbContext.InpAdmissionReferrals.AsNoTracking()
            .Where(x => x.CompletedEpisodeId == episodeId && x.Status == InpAdmissionReferralStatus.Completed && !x.IsDelete)
            .Select(x => new { x.Id, x.SourceEncounterId }).ToListAsync(ct);

        var linkedEncounterIds = await _dbContext.BilInvoiceEncounterLinks.Where(x => x.RanapInvoiceId == invoiceId && !x.IsDelete)
            .Select(x => x.LinkedEncounterId).ToListAsync(ct);

        foreach (var referral in referrals)
        {
            if (linkedEncounterIds.Contains(referral.SourceEncounterId)) continue;
            linkedEncounterIds.Add(referral.SourceEncounterId);
            _dbContext.BilInvoiceEncounterLinks.Add(new BilInvoiceEncounterLink
            {
                RanapInvoiceId = invoiceId,
                LinkedEncounterId = referral.SourceEncounterId,
                LinkReason = BillingEncounterLinkReasons.SurgeryOrigin,
                SourceReferralId = referral.Id,
                ReceiptId = receiptId,
                LinkedAt = DateTimeOffset.UtcNow,
                CreateBy = Guid.Empty,
                CreateDateTime = DateTime.UtcNow
            });
        }

        // BKC-DES-077 / BKC-DES-078 / BKC-DES-079 / BIL-INT-021: Tautan kunjungan asal transfer IGD/Poli
        var ranapEncounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .Where(x => x.Id == encounterId && !x.IsDelete && !x.IsCancel)
            .Select(x => new
            {
                x.Id,
                x.PatientId,
                x.SourceEncounterId,
                x.AdmissionTransferSourceType
            })
            .FirstOrDefaultAsync(ct);

        if (ranapEncounter == null || !ranapEncounter.SourceEncounterId.HasValue || ranapEncounter.SourceEncounterId.Value == Guid.Empty)
        {
            return;
        }

        var sourceEncounterId = ranapEncounter.SourceEncounterId.Value;

        // Jika kunjungan asal sudah bertautan — termasuk lewat rujukan operasi — lewati (sebab pertama dipertahankan)
        if (linkedEncounterIds.Contains(sourceEncounterId))
        {
            return;
        }

        string linkReason;
        if (string.Equals(ranapEncounter.AdmissionTransferSourceType, "Emergency", StringComparison.OrdinalIgnoreCase))
        {
            linkReason = BillingEncounterLinkReasons.EmergencyOrigin;
        }
        else if (string.Equals(ranapEncounter.AdmissionTransferSourceType, "Outpatient", StringComparison.OrdinalIgnoreCase))
        {
            linkReason = BillingEncounterLinkReasons.OutpatientOrigin;
        }
        else
        {
            await _loggerService.WarningAsync(LogCategory, "InpatientEvent.TransferLinkSkipped",
                "Tautan asal transfer rawat inap dilewati: tipe sumber transfer tidak dikenal.", new
                {
                    EpisodeId = episodeId,
                    EncounterId = encounterId,
                    SourceEncounterId = sourceEncounterId,
                    ReasonCode = "INVALID_TRANSFER_SOURCE_TYPE"
                });
            return;
        }

        var originEncounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .Where(x => x.Id == sourceEncounterId)
            .Select(x => new
            {
                x.Id,
                x.PatientId,
                x.IsDelete,
                x.IsCancel
            })
            .FirstOrDefaultAsync(ct);

        if (originEncounter == null)
        {
            await _loggerService.WarningAsync(LogCategory, "InpatientEvent.TransferLinkSkipped",
                "Tautan asal transfer rawat inap dilewati: kunjungan asal tidak ditemukan.", new
                {
                    EpisodeId = episodeId,
                    EncounterId = encounterId,
                    SourceEncounterId = sourceEncounterId,
                    ReasonCode = "ORIGIN_NOT_FOUND"
                });
            return;
        }

        if (originEncounter.IsDelete)
        {
            await _loggerService.WarningAsync(LogCategory, "InpatientEvent.TransferLinkSkipped",
                "Tautan asal transfer rawat inap dilewati: kunjungan asal telah dihapus.", new
                {
                    EpisodeId = episodeId,
                    EncounterId = encounterId,
                    SourceEncounterId = sourceEncounterId,
                    ReasonCode = "DELETED_ORIGIN"
                });
            return;
        }

        if (originEncounter.IsCancel)
        {
            await _loggerService.WarningAsync(LogCategory, "InpatientEvent.TransferLinkSkipped",
                "Tautan asal transfer rawat inap dilewati: kunjungan asal telah dibatalkan.", new
                {
                    EpisodeId = episodeId,
                    EncounterId = encounterId,
                    SourceEncounterId = sourceEncounterId,
                    ReasonCode = "CANCELLED_ORIGIN"
                });
            return;
        }

        if (originEncounter.PatientId != ranapEncounter.PatientId)
        {
            await _loggerService.WarningAsync(LogCategory, "InpatientEvent.TransferLinkSkipped",
                "Tautan asal transfer rawat inap dilewati: kunjungan asal milik pasien lain.", new
                {
                    EpisodeId = episodeId,
                    EncounterId = encounterId,
                    SourceEncounterId = sourceEncounterId,
                    ReasonCode = "PATIENT_MISMATCH"
                });
            return;
        }

        linkedEncounterIds.Add(originEncounter.Id);
        _dbContext.BilInvoiceEncounterLinks.Add(new BilInvoiceEncounterLink
        {
            RanapInvoiceId = invoiceId,
            LinkedEncounterId = originEncounter.Id,
            LinkReason = linkReason,
            SourceReferralId = null,
            ReceiptId = receiptId,
            LinkedAt = DateTimeOffset.UtcNow,
            CreateBy = Guid.Empty,
            CreateDateTime = DateTime.UtcNow
        });
    }
}
