using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

/// <summary>
/// Ringkasan operasi baca-saja untuk bangsal dan dokter (<c>BE-RWI-180</c>, API 11.5.1
/// <c>GET cases/{id}/post-operative-summary</c>, <c>RWI-DEC-197</c>).
/// </summary>
/// <remarks>
/// Tidak ada field rupiah (<c>RWI-DEC-160</c>). Selama laporan operasi belum final,
/// <see cref="ReportFinal"/> = <c>false</c>, seluruh isian klinis <c>null</c>, dan
/// <see cref="Message"/> berbunyi "Laporan operasi belum final".
/// </remarks>
public class PostOperativeSummaryResponse
{
    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public List<string> ProcedureNames { get; set; } = [];
    public string PrimarySurgeonName { get; set; } = string.Empty;
    public OprCaseStatus CaseStatus { get; set; }

    /// <summary>Laporan operasi sudah final; hanya bila benar, isian klinis di bawah terisi.</summary>
    public bool ReportFinal { get; set; }

    /// <summary>Terisi "Laporan operasi belum final" bila <see cref="ReportFinal"/> = <c>false</c>.</summary>
    public string? Message { get; set; }

    // Laporan operasi final.
    public string? PostDiagnosis { get; set; }
    public string? Findings { get; set; }
    public string? Complications { get; set; }
    public decimal? BloodLossMl { get; set; }
    public string? ImplantDrainNote { get; set; }
    public string? PostPlan { get; set; }
    public DateTime? FinishedAt { get; set; }

    // Anestesi.
    public string? AnesthesiaTechnique { get; set; }
    public OprPlannedAnesthesiaType? PlannedAnesthesiaType { get; set; }

    // Kamar pulih.
    public string? RecoveryScoreSystem { get; set; }
    public decimal? RecoveryScoreValue { get; set; }
    public OprRecoveryDecision? RecoveryDecision { get; set; }

    // Serah terima terakhir.
    public string? HandoverInstructionSummary { get; set; }
    public OprHandoverStatus? HandoverStatus { get; set; }
}
