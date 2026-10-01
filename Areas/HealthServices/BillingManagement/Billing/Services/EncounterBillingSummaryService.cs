using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Ringkasan tagihan satu kunjungan untuk workspace dokter (<c>BE-RJE-014</c>,
/// <c>RJ-E2E-DEC-008</c>).
/// </summary>
/// <remarks>
/// Total invoice <c>OPEN</c> dihitung <see cref="BillingCalculationService.PreviewCalculationAsync"/> —
/// mesin yang sama dengan <c>calculation-preview</c> kasir; invoice yang sudah final memakai versi
/// kalkulasi yang dikunci. Angka dokter dan kasir karena itu tidak pernah berbeda.
/// Service ini tidak pernah mengembalikan harga per item, dan tidak membuka invoice lain selain
/// milik kunjungan yang diminta. Seluruhnya baca saja.
/// </remarks>
public sealed class EncounterBillingSummaryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingCalculationService _calculationService;

    public EncounterBillingSummaryService(ApplicationDbContext dbContext, BillingCalculationService calculationService)
    {
        _dbContext = dbContext;
        _calculationService = calculationService;
    }

    /// <summary>Mengembalikan <c>null</c> bila kunjungan tidak ditemukan.</summary>
    public async Task<EncounterBillingSummaryResponse?> GetByEncounterAsync(
        Guid encounterId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .Where(x => x.Id == encounterId && !x.IsDelete)
            .Select(x => new { x.Id, x.PaymentType })
            .FirstOrDefaultAsync(cancellationToken);
        if (encounter is null)
            return null;

        var response = new EncounterBillingSummaryResponse
        {
            EncounterId = encounter.Id,
            PaymentSourceLabel = await BuildPaymentSourceLabelAsync(encounter.Id, encounter.PaymentType, cancellationToken)
        };

        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .Select(x => new { x.Id, x.InvoiceNumber, x.Status, x.CurrentCalculationVersion, x.CreateDateTime, x.UpdateDateTime })
            .FirstOrDefaultAsync(cancellationToken);
        if (invoice is not null)
        {
            var items = await _dbContext.BilInvoiceItems.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && x.Status != BillingInvoiceItemStatuses.Voided && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .Select(x => new { x.DescriptionSnapshot, x.Quantity, x.SourceDomain, x.SourceStatus })
                .ToListAsync(cancellationToken);

            var (gross, payer, patient) = await ReadAmountsAsync(
                invoice.Id, invoice.Status, invoice.CurrentCalculationVersion, actorUserId, cancellationToken);

            response.BillingStatus = invoice.Status;
            response.InvoiceId = invoice.Id;
            response.InvoiceNumber = invoice.InvoiceNumber;
            response.ServiceCount = items.Count;
            response.GrossAmount = gross;
            response.PayerAmount = payer;
            response.PatientAmount = patient;
            response.LastUpdatedAt = new DateTimeOffset(
                DateTime.SpecifyKind(invoice.UpdateDateTime ?? invoice.CreateDateTime, DateTimeKind.Utc));
            response.Services.AddRange(items.Select(x => new EncounterBillingSummaryServiceResponse
            {
                Description = x.DescriptionSnapshot,
                Quantity = x.Quantity,
                ServiceStatusLabel = ServiceStatusLabel(x.SourceDomain, x.SourceStatus),
                BillingState = EncounterBillingSummaryBillingStates.Recorded
            }));
        }

        await AppendUnbilledServicesAsync(encounterId, response, cancellationToken);
        return response;
    }

    /// <summary>
    /// Invoice <c>OPEN</c>: pratinjau kalkulasi, sama dengan <c>calculation-preview</c> kasir.
    /// Invoice yang sudah final tidak dapat dihitung ulang, sehingga angkanya dibaca dari versi
    /// kalkulasi terkini yang dikunci saat finalisasi — angka yang sama dengan yang dilihat kasir.
    /// </summary>
    private async Task<(decimal Gross, decimal Payer, decimal Patient)> ReadAmountsAsync(
        Guid invoiceId,
        string invoiceStatus,
        int currentCalculationVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (invoiceStatus == BillingInvoiceStatuses.Open)
        {
            // BillingCalculationValidationException sengaja diteruskan: pemanggil menjawab 422.
            var preview = await _calculationService.PreviewCalculationAsync(invoiceId, actorUserId, cancellationToken);
            return (preview.GrossAmount, preview.PrimaryAmount, preview.PatientAmount);
        }

        var stored = await _dbContext.BilCalculationVersions.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId && x.VersionNo == currentCalculationVersion && !x.IsDelete)
            .Select(x => new { x.GrossAmount, x.PrimaryAmount, x.PatientAmount })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BillingCalculationValidationException("Invoice belum memiliki hasil perhitungan terkini.");
        return (stored.GrossAmount, stored.PrimaryAmount, stored.PatientAmount);
    }

    /// <summary>
    /// Pelayanan yang belum (atau tidak) menjadi item invoice, dari revisi terakhir setiap fakta pada
    /// folio kunjungan, ditambah fakta klinis yang tidak pernah sampai ke folio.
    /// </summary>
    private async Task AppendUnbilledServicesAsync(
        Guid encounterId,
        EncounterBillingSummaryResponse response,
        CancellationToken cancellationToken)
    {
        var effects = await (
                from effect in _dbContext.Set<BilProcessingEffect>().AsNoTracking()
                join folio in _dbContext.Set<BilFolio>().AsNoTracking() on effect.FolioId equals folio.Id
                where folio.EncounterId == encounterId && !effect.IsDelete
                select new
                {
                    effect.MilestoneFactId,
                    effect.SourceContext,
                    effect.MilestoneFactVersion,
                    effect.InvoiceSyncStatus,
                    effect.InvoiceSyncErrorCode
                })
            .ToListAsync(cancellationToken);

        var latest = effects
            .GroupBy(x => new { x.MilestoneFactId, x.SourceContext })
            .Select(x => x.OrderByDescending(e => e.MilestoneFactVersion).First());
        foreach (var effect in latest)
        {
            var (state, label) = effect.InvoiceSyncStatus switch
            {
                BillingInvoiceSyncStatus.Pending or BillingInvoiceSyncStatus.Failed =>
                    (EncounterBillingSummaryBillingStates.Pending, "Menunggu masuk tagihan"),
                BillingInvoiceSyncStatus.ReconciliationRequired =>
                    (EncounterBillingSummaryBillingStates.Reconciliation, "Perlu ditinjau Billing"),
                BillingInvoiceSyncStatus.NotApplicable
                    when effect.InvoiceSyncErrorCode is BillingBridgeCodes.NotBillable or BillingBridgeCodes.RepeatInternalError =>
                    (EncounterBillingSummaryBillingStates.NotBilled, "Tidak ditagihkan"),
                _ => (null, null)
            };
            if (state is null) continue;

            if (state == EncounterBillingSummaryBillingStates.Pending) response.PendingSyncCount++;
            if (state == EncounterBillingSummaryBillingStates.Reconciliation) response.ReconciliationCount++;
            response.Services.Add(new EncounterBillingSummaryServiceResponse
            {
                Description = SourceLabel(effect.SourceContext),
                Quantity = 1,
                ServiceStatusLabel = label!,
                BillingState = state
            });
        }

        // Fakta yang gagal diserahkan ke folio sama sekali tidak punya efek; tetap wajib terlihat.
        var undeliveredFacts = await _dbContext.Set<CliClinicalMilestoneFact>().AsNoTracking()
            .Where(x => x.EncounterId == encounterId
                && x.ReconciliationRequiredAt != null
                && x.ReconciliationResolvedAt == null
                && x.BillingProcessingEffectId == null
                && !x.IsDelete)
            .Select(x => x.SourceContext)
            .ToListAsync(cancellationToken);
        foreach (var sourceContext in undeliveredFacts)
        {
            response.ReconciliationCount++;
            response.Services.Add(new EncounterBillingSummaryServiceResponse
            {
                Description = SourceLabel(sourceContext),
                Quantity = 1,
                ServiceStatusLabel = "Perlu ditinjau Billing",
                BillingState = EncounterBillingSummaryBillingStates.Reconciliation
            });
        }
    }

    private async Task<string> BuildPaymentSourceLabelAsync(
        Guid encounterId,
        EncounterPaymentType encounterPaymentType,
        CancellationToken cancellationToken)
    {
        // Sama dengan ringkasan tagihan rawat inap: penjamin utama kunjungan menentukan label.
        var guarantor = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
            .Where(x => x.EncounterId == encounterId && x.IsActive && !x.IsDelete && !x.IsCancel)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Priority)
            .Select(x => new { x.PaymentType, x.PaymentSourceNameSnapshot })
            .FirstOrDefaultAsync(cancellationToken);

        var paymentType = guarantor?.PaymentType ?? encounterPaymentType;
        var typeLabel = paymentType switch
        {
            EncounterPaymentType.Cash => "Tunai",
            EncounterPaymentType.Insurance => "Asuransi",
            EncounterPaymentType.CompanyGuarantor => "Penjamin Perusahaan",
            _ => paymentType.ToString()
        };
        var name = guarantor?.PaymentSourceNameSnapshot?.Trim();
        return paymentType == EncounterPaymentType.Cash || string.IsNullOrEmpty(name)
            ? typeLabel
            : $"{typeLabel} — {name}";
    }

    private static string SourceLabel(string sourceContext) => sourceContext switch
    {
        BillingSourceContract.ProcedureSourceContext => "Tindakan",
        BillingSourceContract.LaboratorySourceContext => "Pemeriksaan laboratorium",
        BillingSourceContract.RadiologySourceContext => "Pemeriksaan radiologi",
        BillingSourceContract.PrescriptionSourceContext => "Resep",
        BillingSourceContract.ConsultationSourceContext => "Jasa konsultasi",
        _ => "Pelayanan"
    };

    private static string ServiceStatusLabel(string sourceDomain, string sourceStatus) => sourceStatus switch
    {
        "PERFORMED" => "Dikerjakan",
        "ACCEPTED" => sourceDomain == BillingBridgeSourceDomains.Laboratory ? "Diterima Lab" : "Diterima",
        "CONFIRMED" => "Dikonfirmasi",
        "COMPLETED" => "Selesai",
        "PRESCRIBED" => "Diresepkan",
        "DISPENSED" => "Diserahkan",
        "ADDED" => "Ditambahkan kasir",
        _ => sourceStatus
    };
}
