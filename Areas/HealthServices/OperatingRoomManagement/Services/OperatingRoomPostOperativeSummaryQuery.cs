using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Bacaan gabungan ringkasan operasi (<c>BE-RWI-180</c>, <c>FR-RWF-081</c>, <c>FR-RWF-082</c>,
/// backend 12.7). Menggabungkan empat sumber OK — laporan operasi final, catatan anestesi, kamar
/// pulih, dan serah terima terakhir — dalam satu bacaan dengan satu permission
/// <c>OperatingRoomCase : Read</c>. Tidak menulis apa pun.
/// </summary>
/// <remarks>
/// Contoh <c>UAT-RWF-17</c>: dokter DPJP Budi membuka ringkasan sore hari sesudah laparotomi —
/// diagnosis pasca bedah, temuan, perdarahan 150 ml, drain, rencana, teknik anestesi umum, skor
/// Aldrete 9 dengan keputusan "Rawat inap", dan instruksi serah terima tampil sekaligus. Bila
/// dibuka saat laporan masih draft, yang tampil hanya identitas kasus dan "Laporan operasi belum
/// final".
/// </remarks>
public sealed class OperatingRoomPostOperativeSummaryQuery
{
    public const string ReportNotFinalMessage = "Laporan operasi belum final";

    private readonly ApplicationDbContext _dbContext;

    public OperatingRoomPostOperativeSummaryQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <returns><c>null</c> bila kasus tidak ditemukan.</returns>
    public async Task<PostOperativeSummaryResponse?> GetAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var opCase = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new
            {
                x.Id, x.CaseNumber, x.Status, x.PlannedAnesthesiaType,
                PrimarySurgeonName = x.PrimarySurgeon != null ? x.PrimarySurgeon.FullName : string.Empty,
                ProcedureNames = x.Procedures.Where(p => !p.IsDelete).OrderBy(p => p.Sequence)
                    .Select(p => p.PatientProcedure != null ? p.PatientProcedure.ProcedureNameSnapshot : string.Empty)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (opCase == null) return null;

        var response = new PostOperativeSummaryResponse
        {
            CaseId = opCase.Id,
            CaseNumber = opCase.CaseNumber,
            CaseStatus = opCase.Status,
            PrimarySurgeonName = opCase.PrimarySurgeonName,
            ProcedureNames = [.. opCase.ProcedureNames.Where(x => !string.IsNullOrWhiteSpace(x))]
        };

        var report = await _dbContext.OprExecutionRecords.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete && x.Status == OprRecordStatus.Final)
            .Select(x => new
            {
                x.PostDiagnosis, x.Findings, x.Complications, x.BloodLossMl, x.ImplantDrainNote, x.PostPlan, x.FinishedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        // FR-RWF-082: laporan draft → tidak ada isi klinis sama sekali.
        if (report == null)
        {
            response.ReportFinal = false;
            response.Message = ReportNotFinalMessage;
            return response;
        }

        response.ReportFinal = true;
        response.PostDiagnosis = report.PostDiagnosis;
        response.Findings = report.Findings;
        response.Complications = report.Complications;
        response.BloodLossMl = report.BloodLossMl;
        response.ImplantDrainNote = report.ImplantDrainNote;
        response.PostPlan = report.PostPlan;
        response.FinishedAt = report.FinishedAt;
        response.PlannedAnesthesiaType = opCase.PlannedAnesthesiaType;

        response.AnesthesiaTechnique = await _dbContext.OprAnesthesiaRecords.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete && x.Status == OprRecordStatus.Final)
            .Select(x => x.Technique)
            .FirstOrDefaultAsync(cancellationToken);

        var recovery = await _dbContext.OprRecoveries.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .Select(x => new { x.ScoreSystem, x.ScoreValue, x.Decision, x.Status })
            .FirstOrDefaultAsync(cancellationToken);
        if (recovery != null)
        {
            response.RecoveryScoreSystem = recovery.ScoreSystem;
            response.RecoveryScoreValue = recovery.ScoreValue;
            // Keputusan baru bermakna setelah pasien dinyatakan siap keluar atau keluar kamar pulih.
            response.RecoveryDecision = recovery.Status == OprRecoveryStatus.Monitoring ? null : recovery.Decision;
        }

        var handover = await _dbContext.OprHandovers.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .OrderByDescending(x => x.Revision)
            .Select(x => new { x.InstructionSummary, x.Status })
            .FirstOrDefaultAsync(cancellationToken);
        if (handover != null)
        {
            response.HandoverInstructionSummary = handover.InstructionSummary;
            response.HandoverStatus = handover.Status;
        }

        return response;
    }
}
