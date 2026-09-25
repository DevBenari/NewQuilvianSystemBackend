using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Kontrak antarmuka evaluasi kelayakan finansial rawat inap, validasi deposit tindakan besar,
/// dan penegakan Auto-Reblock (BKC-DEC-114, BKC-DEC-115, BKC-DEC-116, BKC-DES-045, BKC-DES-046, BKC-DES-047).
/// </summary>
public interface IInpatientClearanceService
{
    /// <summary>
    /// Evaluasi status kelayakan pemulangan rawat inap (Single Source of Truth).
    /// Menerbitkan baris baru BilInpatientClearanceHandoff dengan nomor FinancialVersion yang naik monoton (BKC-DEC-115, BKC-DES-045).
    /// </summary>
    Task<BilInpatientClearanceHandoff> EvaluateClearanceAsync(
        Guid encounterId,
        string reasonCode,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken,
        string? revocationReason = null);

    /// <summary>
    /// Validasi deposit 100% dari ekses/tanggung jawab pasien atas tindakan/operasi besar (BKC-DEC-114, BKC-DES-047, BIL-VAL-121).
    /// </summary>
    Task<MajorProcedureDepositValidationResult> ValidateMajorProcedureDepositAsync(
        MajorProcedureDepositValidationRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Menegakkan Auto-Reblock (REVOKED) seketika saat tagihan susulan tiba pada invoice berstatus OPEN (BKC-DEC-116, BKC-DES-046, BIL-VAL-123).
    /// </summary>
    Task<BilInpatientClearanceHandoff?> TriggerAutoReblockIfApplicableAsync(
        Guid invoiceId,
        string triggerReason,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Mengakui penerimaan surat handoff kelayakan oleh bangsal rawat inap secara idempoten (BIL-VAL-116, BIL-VAL-125).
    /// </summary>
    Task<BilInpatientClearanceHandoff> AcknowledgeClearanceHandoffAsync(
        Guid handoffId,
        Guid actorUserId,
        DateTimeOffset acknowledgedAt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Mengambil surat kelayakan pemulangan aktif terakhir untuk encounter.
    /// </summary>
    Task<BilInpatientClearanceHandoff?> GetLatestClearanceForEncounterAsync(
        Guid encounterId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Menyajikan ringkasan kelayakan finansial rawat inap untuk bangsal (BIL-API-1.4).
    /// </summary>
    Task<InpatientBillingSummaryResponse> GetInpatientBillingSummaryAsync(
        Guid encounterId,
        bool includeFinancialDetails,
        CancellationToken cancellationToken);
}
