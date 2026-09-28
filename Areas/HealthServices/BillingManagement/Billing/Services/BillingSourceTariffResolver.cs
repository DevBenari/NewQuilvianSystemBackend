using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Menetapkan harga item invoice dari katalog tarif untuk fakta pelayanan Rawat Jalan
/// (<c>RJ-E2E-DEC-006</c>, <c>02-backend-architecture.md</c> V2.7.4).
///
/// Jumlah berasal dari fakta klinis (kebenaran klinis); harga berasal dari <see cref="MstTariff"/>
/// yang berlaku pada waktu pelayanan (kebenaran finansial). Harga yang dikirim modul klinis di
/// <c>TariffSnapshot</c> sengaja tidak dibaca. Bila tarif tidak ditemukan, resolver tidak pernah
/// mengarang Rp0 — hasilnya penolakan yang diteruskan ke antrean rekonsiliasi.
/// </summary>
public sealed class BillingSourceTariffResolver
{
    private readonly ApplicationDbContext _dbContext;

    public BillingSourceTariffResolver(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<BillingTariffResolution> ResolveAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        return request.SourceDomain switch
        {
            BillingBridgeSourceDomains.Procedure => await ResolveProcedureAsync(request, cancellationToken),
            BillingBridgeSourceDomains.Laboratory => await ResolveLaboratoryAsync(request, cancellationToken),
            BillingBridgeSourceDomains.Radiology => await ResolveRadiologyAsync(request, cancellationToken),
            _ => BillingTariffResolution.Pending(
                BillingBridgeCodes.SourcePendingSupport,
                $"Penetapan tarif untuk {request.SourceDomain} belum tersedia pada jembatan.")
        };
    }

    private async Task<BillingTariffResolution> ResolveProcedureAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        var procedure = await _dbContext.Set<TrxPatientProcedure>().AsNoTracking()
            .Where(x => x.Id == request.SourceAggregateId)
            .Select(x => new { x.TariffId, x.ProcedureId, x.ClinicId, x.Quantity, x.IsFreeOfCharge, x.IsBillable })
            .FirstOrDefaultAsync(cancellationToken);
        if (procedure is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Tindakan sumber tidak ditemukan.");

        // Tindakan gratis atau yang ditandai tidak ditagihkan memang tidak masuk invoice.
        if (procedure.IsFreeOfCharge || !procedure.IsBillable)
            return BillingTariffResolution.NotBillable(BillingBridgeCodes.NotBillable, "Tindakan ditandai gratis atau tidak ditagihkan.");

        var quantity = request.FactQuantity is > 0 ? request.FactQuantity.Value : procedure.Quantity;
        var tariff = await FindTariffAsync(
            procedure.TariffId, procedure.ProcedureId, procedure.ClinicId ?? request.EncounterClinicId,
            request.PatientClassId, request.OccurredAt, cancellationToken);
        return Build(tariff, quantity);
    }

    private async Task<BillingTariffResolution> ResolveLaboratoryAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        var examination = await _dbContext.Set<LabExamination>().AsNoTracking()
            .Where(x => x.Id == request.SourceItemId)
            .Select(x => new { x.TariffId, x.ProcedureId })
            .FirstOrDefaultAsync(cancellationToken);
        if (examination is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Pemeriksaan laboratorium sumber tidak ditemukan.");

        var tariff = await FindTariffAsync(
            examination.TariffId, examination.ProcedureId, request.EncounterClinicId,
            request.PatientClassId, request.OccurredAt, cancellationToken);
        return Build(tariff, 1m);
    }

    private async Task<BillingTariffResolution> ResolveRadiologyAsync(
        BillingTariffResolutionRequest request,
        CancellationToken cancellationToken)
    {
        var study = await _dbContext.Set<RadStudy>().AsNoTracking()
            .Where(x => x.Id == request.SourceItemId)
            .Select(x => new { x.RadOrderId, x.RepeatCause })
            .FirstOrDefaultAsync(cancellationToken);
        if (study is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Study radiologi sumber tidak ditemukan.");

        // RJ-BIL-GATE-DEC-004: pengulangan karena kesalahan rumah sakit tidak menambah tagihan pasien.
        if (study.RepeatCause == RadRepeatCause.InternalHospitalError)
            return BillingTariffResolution.NotBillable(
                BillingBridgeCodes.RepeatInternalError,
                "Pengulangan pemeriksaan karena kesalahan rumah sakit tidak ditagihkan.");

        var procedureId = await _dbContext.Set<RadOrder>().AsNoTracking()
            .Where(x => x.Id == study.RadOrderId)
            .Select(x => (Guid?)x.ProcedureId)
            .FirstOrDefaultAsync(cancellationToken);
        if (procedureId is null)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceNotFound, "Order radiologi sumber tidak ditemukan.");

        var tariff = await FindTariffAsync(
            null, procedureId.Value, request.EncounterClinicId, request.PatientClassId,
            request.OccurredAt, cancellationToken);
        return Build(tariff, 1m);
    }

    /// <summary>
    /// Tarif langsung (bila sumber sudah menyimpannya) menang; bila tidak ada atau tidak berlaku,
    /// dicari berdasarkan tindakan dengan urutan paling spesifik: klinik dan kelas pasien, lalu
    /// salah satunya, lalu umum. Seri diputus dengan kode tarif supaya hasilnya deterministik.
    /// </summary>
    private async Task<MstTariff?> FindTariffAsync(
        Guid? directTariffId,
        Guid procedureId,
        Guid? clinicId,
        Guid? patientClassId,
        DateTime occurredAt,
        CancellationToken cancellationToken)
    {
        var effective = EffectiveTariffs(occurredAt);

        if (directTariffId is { } tariffId && tariffId != Guid.Empty)
        {
            var direct = await effective.FirstOrDefaultAsync(x => x.Id == tariffId, cancellationToken);
            if (direct is not null) return direct;
        }

        var candidates = await effective
            .Where(x => x.ProcedureId == procedureId
                && (x.ClinicId == null || x.ClinicId == clinicId)
                && (x.PatientClassId == null || x.PatientClassId == patientClassId))
            .ToListAsync(cancellationToken);

        return candidates
            .OrderByDescending(x => (x.ClinicId != null ? 2 : 0) + (x.PatientClassId != null ? 1 : 0))
            .ThenBy(x => x.TariffCode, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private IQueryable<MstTariff> EffectiveTariffs(DateTime occurredAt) =>
        _dbContext.Set<MstTariff>().AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= occurredAt)
                && (x.EffectiveEndDate == null || occurredAt < x.EffectiveEndDate));

    private static BillingTariffResolution Build(MstTariff? tariff, decimal quantity)
    {
        if (tariff is null)
            return BillingTariffResolution.Rejected(
                BillingBridgeCodes.TariffNotFound,
                "Tarif untuk pelayanan ini belum tersedia pada tanggal pelayanan. Lengkapi tarif, lalu kirim ulang.");
        if (quantity <= 0)
            return BillingTariffResolution.Rejected(BillingBridgeCodes.SourceRejected, "Jumlah pelayanan tidak sah.");

        var description = tariff.TariffName.Trim();
        return BillingTariffResolution.Resolved(
            tariff.Id,
            tariff.TariffCategoryId,
            description.Length > 250 ? description[..250] : description,
            quantity,
            tariff.NormalPrice);
    }
}

public sealed record BillingTariffResolutionRequest(
    string SourceDomain,
    Guid SourceAggregateId,
    Guid? SourceItemId,
    decimal? FactQuantity,
    Guid? EncounterClinicId,
    Guid? PatientClassId,
    DateTime OccurredAt);

public enum BillingTariffResolutionKind
{
    Resolved = 1,
    NotBillable = 2,
    Rejected = 3,
    Pending = 4
}

public sealed record BillingTariffResolution(
    BillingTariffResolutionKind Kind,
    Guid? TariffId,
    Guid CategoryId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    string? Code,
    string? Message)
{
    public static BillingTariffResolution Resolved(Guid tariffId, Guid categoryId, string description, decimal quantity, decimal unitPrice) =>
        new(BillingTariffResolutionKind.Resolved, tariffId, categoryId, description, quantity, unitPrice, null, null);

    public static BillingTariffResolution NotBillable(string code, string message) =>
        new(BillingTariffResolutionKind.NotBillable, null, Guid.Empty, string.Empty, 0, 0, code, message);

    public static BillingTariffResolution Rejected(string code, string message) =>
        new(BillingTariffResolutionKind.Rejected, null, Guid.Empty, string.Empty, 0, 0, code, message);

    public static BillingTariffResolution Pending(string code, string message) =>
        new(BillingTariffResolutionKind.Pending, null, Guid.Empty, string.Empty, 0, 0, code, message);
}
