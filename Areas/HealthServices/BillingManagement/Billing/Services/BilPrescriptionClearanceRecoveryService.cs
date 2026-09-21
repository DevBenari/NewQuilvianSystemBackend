using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// BKC-DEC-111, BIL-INTEGRATION-1.1:
/// Layanan pemulihan resep yang terlanjur tertahan sebelum jalur handoff berdiri.
/// Memanggil pemeriksaan ulang keadaan clearance (BE-BKC-068) untuk resep yang masih menunggu pembayaran,
/// lalu menerbitkan surat pertamanya bila memang tagihannya sudah lunas.
/// Menjamin idempotensi penuh (dapat dijalankan dua kali tanpa melahirkan surat ganda)
/// dan membaca dari model Billing resmi tanpa skrip pemutakhiran data langsung.
/// </summary>
public sealed class BilPrescriptionClearanceRecoveryService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly BilConsumerHandoffService _handoffService;
    private readonly LoggerService _loggerService;

    public BilPrescriptionClearanceRecoveryService(
        ApplicationDbContext dbContext,
        BilConsumerHandoffService handoffService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _handoffService = handoffService;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Menjalankan pekerjaan pemulihan resep tertahan secara idempoten (BKC-DEC-111).
    /// </summary>
    public async Task<PrescriptionClearanceRecoveryResult> RecoverStuckPrescriptionsAsync(
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        // 1. Ambil seluruh item invoice farmasi aktif yang terikat ke invoice
        var pharmacyItems = await _dbContext.BilInvoiceItems
            .AsNoTracking()
            .Include(x => x.Invoice)
            .Where(x => x.SourceDomain == "PHARMACY"
                && x.Status != BillingInvoiceItemStatuses.Voided
                && !x.IsDelete)
            .ToListAsync(cancellationToken);

        // Kumpulkan distinct PrescriptionId beserta Invoicenya
        var prescriptionInvoiceMap = new Dictionary<Guid, BilInvoice>();
        foreach (var item in pharmacyItems)
        {
            if (Guid.TryParse(item.SourceDetailId, out var presId) && presId != Guid.Empty && item.Invoice != null)
            {
                if (!prescriptionInvoiceMap.ContainsKey(presId))
                {
                    prescriptionInvoiceMap[presId] = item.Invoice;
                }
            }
        }

        var totalInvoicesEvaluated = pharmacyItems.Select(x => x.InvoiceId).Distinct().Count();
        var totalPrescriptionsEvaluated = prescriptionInvoiceMap.Count;
        var recoveredPrescriptionIds = new List<Guid>();
        var skippedAlreadyCleared = 0;
        var skippedUnpaid = 0;
        var notes = new List<string>();

        foreach (var (prescriptionId, invoice) in prescriptionInvoiceMap)
        {
            // 2. Acceptance Criteria 2: Resep yang tagihannya belum lunas tidak memperoleh apa pun.
            var isSettled = invoice.Status is BillingInvoiceStatuses.Closed
                or BillingInvoiceStatuses.SettledByWriteOff;

            if (!isSettled)
            {
                skippedUnpaid++;
                continue;
            }

            // 3. Panggil pemeriksaan ulang keadaan clearance terkini via BE-BKC-068 (ReadPrescriptionClearanceAsync)
            var currentStatus = await _handoffService.ReadPrescriptionClearanceAsync(
                prescriptionId, cancellationToken);

            // 4. Acceptance Criteria 3: Idempotensi. Resep yang sudah memiliki surat clearance dilewati.
            if (currentStatus.IsKnown)
            {
                skippedAlreadyCleared++;
                continue;
            }

            // 5. Acceptance Criteria 1: Terbitkan surat pertamanya bila memang sudah lunas.
            var reasonCode = invoice.Status == BillingInvoiceStatuses.SettledByWriteOff
                ? PrescriptionClearanceReasonCodes.InvoiceWrittenOff
                : PrescriptionClearanceReasonCodes.InvoiceSettled;

            var occurredAt = invoice.ClosedAt
                ?? invoice.UpdateDateTime
                ?? DateTimeOffset.UtcNow;

            var correlationId = Guid.NewGuid();
            var causationId = invoice.Id;

            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
            }

            try
            {
                var handoffs = await _handoffService.PublishForClearanceChangeAsync(
                    invoiceId: invoice.Id,
                    reasonCode: reasonCode,
                    actorUserId: actorUserId,
                    occurredAt: occurredAt,
                    correlationId: correlationId,
                    causationId: causationId,
                    cancellationToken: cancellationToken,
                    specificPrescriptionId: prescriptionId);

                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                if (handoffs.Count > 0)
                {
                    recoveredPrescriptionIds.Add(prescriptionId);
                }
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                notes.Add($"Gagal memulihkan resep {prescriptionId} pada invoice {invoice.InvoiceNumber}: {ex.Message}");
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        await _loggerService.AuditAsync(
            LogCategory,
            "BillingPrescriptionClearanceRecovery.Completed",
            "Pekerjaan pemulihan resep tertahan selesai dijalankan.",
            new
            {
                ActorUserId = actorUserId,
                TotalInvoicesEvaluated = totalInvoicesEvaluated,
                TotalPrescriptionsEvaluated = totalPrescriptionsEvaluated,
                TotalPrescriptionsRecovered = recoveredPrescriptionIds.Count,
                TotalPrescriptionsSkippedAlreadyCleared = skippedAlreadyCleared,
                TotalPrescriptionsSkippedUnpaid = skippedUnpaid
            });

        notes.Add($"Pemulihan selesai: {recoveredPrescriptionIds.Count} resep berhasil dipulihkan, {skippedAlreadyCleared} dilewati karena sudah memiliki surat, {skippedUnpaid} dilewati karena tagihan belum lunas.");

        return new PrescriptionClearanceRecoveryResult(
            TotalInvoicesEvaluated: totalInvoicesEvaluated,
            TotalPrescriptionsEvaluated: totalPrescriptionsEvaluated,
            TotalPrescriptionsRecovered: recoveredPrescriptionIds.Count,
            TotalPrescriptionsSkippedAlreadyCleared: skippedAlreadyCleared,
            TotalPrescriptionsSkippedUnpaid: skippedUnpaid,
            RecoveredPrescriptionIds: recoveredPrescriptionIds,
            SummaryNotes: notes);
    }
}
