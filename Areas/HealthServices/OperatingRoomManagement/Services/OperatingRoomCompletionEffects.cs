using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Efek kasus OK <c>Completed</c> (<c>BE-RWI-179</c>, <c>RWI-DEC-196</c>, <c>INV-RWF-29</c>,
/// <c>INV-RWF-30</c>, backend 12.5 dan 12.7). Dijalankan <b>sesudah</b> commit penerimaan serah
/// terima yang membuat kasus selesai.
/// </summary>
/// <remarks>
/// <para>
/// Urutannya: (1) setiap order tindakan yang dirujuk kasus diselesaikan lewat
/// <see cref="PatientProcedureExecutionService.ExecuteFromOperatingRoomAsync"/> — itulah satu-satunya
/// pengirim baris tindakan operasi; (2) komponen OK disiapkan sebagai delivery Billing: anestesi
/// (bila catatan anestesi final), sewa kamar operasi (durasi menit), dan setiap bahan
/// <c>Used</c>; (3) delivery dikirim ke Billing.
/// </para>
/// <para>
/// <b>Idempoten.</b> Order yang sudah selesai dilewati dan delivery berkunci
/// <c>case:charge:component:revision</c> tidak pernah disiapkan dua kali, sehingga efek boleh
/// dijalankan ulang tanpa baris tagihan ganda. Kasus yang tidak <c>Completed</c> — termasuk
/// <c>Cancelled</c> dan <c>Rejected</c> — tidak menimbulkan biaya apa pun.
/// </para>
/// <para>
/// Contoh <c>UAT-RWF-05</c>: SC Ny. Ani dipesan 1 Okt — belum ada biaya. Kasus selesai 2 Okt
/// 11.40 saat serah terima diterima: order "Sectio Caesarea" menjadi <c>Completed</c> dan satu
/// baris tindakan masuk invoice; anestesi, sewa kamar operasi 95 menit, dan dua bahan masing-masing
/// satu baris.
/// </para>
/// </remarks>
public sealed class OperatingRoomCompletionEffects
{
    private const string CompleteAction = "CompleteCase";

    private readonly ApplicationDbContext _dbContext;
    private readonly PatientProcedureExecutionService _procedureExecution;
    private readonly OperatingRoomIntegrationService _integrationService;
    private readonly LoggerService _loggerService;

    public OperatingRoomCompletionEffects(ApplicationDbContext dbContext,
        PatientProcedureExecutionService procedureExecution, OperatingRoomIntegrationService integrationService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _procedureExecution = procedureExecution;
        _integrationService = integrationService;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Menjalankan efek kasus selesai. Tidak pernah melempar ke pemanggil: setiap langkah yang
    /// gagal dicatat dan dapat diulang, karena penyelesaian kasus yang sudah tersimpan tidak boleh
    /// ikut gagal (backend 12.5).
    /// </summary>
    public async Task<OprCompletionEffectsResult> ApplyAsync(Guid caseId, Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var result = new OprCompletionEffectsResult();

        var opCase = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new
            {
                x.Id, x.CaseNumber, x.Status, x.PrimarySurgeonId, x.UpdateDateTime,
                ProcedureIds = x.Procedures.Where(p => !p.IsDelete).OrderBy(p => p.Sequence)
                    .Select(p => p.PatientProcedureId).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        // INV-RWF-30: hanya kasus Completed yang menimbulkan biaya.
        if (opCase == null || opCase.Status != OprCaseStatus.Completed)
        {
            result.Skipped = true;
            return result;
        }

        var completedAt = await _dbContext.OprStatusHistories.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && x.Action == CompleteAction && !x.IsDelete)
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => (DateTime?)x.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken) ?? opCase.UpdateDateTime ?? DateTime.UtcNow;

        // (1) Order tindakan — satu baris tindakan, satu pengirim (INV-RWF-29).
        foreach (var procedureId in opCase.ProcedureIds)
        {
            try
            {
                var execution = await _procedureExecution.ExecuteFromOperatingRoomAsync(
                    procedureId, opCase.PrimarySurgeonId, completedAt, actorUserId, cancellationToken);
                if (execution.IsSuccess) result.ProceduresCompleted++;
                else result.Failures.Add($"Order {procedureId:N}: {execution.Message}");
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                result.Failures.Add($"Order {procedureId:N}: {exception.GetType().Name}");
                _dbContext.ChangeTracker.Clear();
            }
        }

        // (2) Komponen OK sebagai delivery Billing.
        try
        {
            var now = DateTime.UtcNow;

            var anesthesia = await _dbContext.OprAnesthesiaRecords.AsNoTracking()
                .Where(x => x.OprCaseId == caseId && !x.IsDelete && x.Status == OprRecordStatus.Final)
                .Select(x => new { x.Version })
                .FirstOrDefaultAsync(cancellationToken);
            if (anesthesia != null)
                await _integrationService.StageChargeDeliveryAsync(caseId,
                    OperatingRoomIntegrationService.AnesthesiaComponent, anesthesia.Version, actorUserId, now, cancellationToken);

            var execution = await _dbContext.OprExecutionRecords.AsNoTracking()
                .Where(x => x.OprCaseId == caseId && !x.IsDelete && x.FinishedAt != null)
                .Select(x => new { x.Version })
                .FirstOrDefaultAsync(cancellationToken);
            if (execution != null)
                await _integrationService.StageChargeDeliveryAsync(caseId,
                    OperatingRoomIntegrationService.OperatingRoomRentComponent, execution.Version, actorUserId, now, cancellationToken);

            var usages = await _dbContext.OprMaterialUsages.AsNoTracking()
                .Where(x => x.OprCaseId == caseId && !x.IsDelete && x.Outcome == OprMaterialOutcome.Used)
                .Select(x => new { x.Id, x.Revision })
                .ToListAsync(cancellationToken);
            foreach (var usage in usages)
                await _integrationService.StageChargeDeliveryAsync(caseId,
                    $"{OperatingRoomIntegrationService.MaterialComponentPrefix}{usage.Id:N}", usage.Revision,
                    actorUserId, now, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            result.Failures.Add($"Penyiapan komponen: {exception.GetType().Name}");
            _dbContext.ChangeTracker.Clear();
        }

        // (3) Kirim ke Billing. Yang gagal tetap Failed dan diantrekan ulang lewat RetryAsync.
        try
        {
            result.ComponentsDelivered = await _integrationService.DeliverPendingBillingAsync(
                caseId, actorUserId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            result.Failures.Add($"Pengiriman Billing: {exception.GetType().Name}");
            _dbContext.ChangeTracker.Clear();
        }

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomCompletion.ApplyEffects",
            "Menjalankan efek kasus operasi selesai: order tindakan dan komponen biaya.",
            new
            {
                CaseId = caseId, opCase.CaseNumber, ActorUserId = actorUserId, result.ProceduresCompleted,
                result.ComponentsDelivered, FailureCount = result.Failures.Count
            });
        return result;
    }
}

/// <summary>Ringkasan efek kasus selesai, untuk log dan pemanggil.</summary>
public sealed class OprCompletionEffectsResult
{
    /// <summary>Kasus tidak <c>Completed</c>, sehingga tidak ada efek (<c>INV-RWF-30</c>).</summary>
    public bool Skipped { get; set; }

    public int ProceduresCompleted { get; set; }
    public int ComponentsDelivered { get; set; }
    public List<string> Failures { get; } = [];
}
