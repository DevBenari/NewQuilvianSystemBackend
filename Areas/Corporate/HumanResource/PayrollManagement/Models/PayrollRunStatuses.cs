namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

/// <summary>
/// Status siklus TrxPayrollRun.RunStatus. Kolom bertipe string, jadi tidak ada perubahan skema.
/// Alur normal: Draft (run dibuka HRD) -> Calculated (gaji dan potongan terhitung) -> Submitted (diajukan untuk review)
/// -> Approved (disahkan; hasil potongan dikonfirmasi ke Finance dan run terkunci). Dibatalkan hanya sebelum disahkan; koreksi
/// sesudah disahkan lewat pembalikan/penyesuaian bertanda tangan (bukan menjalankan ulang potongan).
/// </summary>
public static class PayrollRunStatuses
{
    public const string Draft = "Draft";
    public const string CollectingInput = "CollectingInput";
    public const string Calculated = "Calculated";
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";
    public const string Posted = "Posted";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";
    public const string Reversed = "Reversed";

    /// <summary>Run yang masih boleh menerima input baru (mis. jadwal angsuran dari Finance) dan dihitung ulang.</summary>
    public static bool AcceptsInputs(string? status) =>
        string.Equals(status, Draft, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, CollectingInput, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Calculated, StringComparison.OrdinalIgnoreCase);

    /// <summary>Run yang sudah disahkan atau lebih jauh: hasil potongan sudah final dan tidak boleh dihitung ulang.</summary>
    public static bool IsFinalized(string? status) =>
        string.Equals(status, Approved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Posted, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Closed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Reversed, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Status pembalikan potongan cicilan atas payroll run yang sudah disahkan (TrxPayrollReversal.ReversalStatus; kolom string, tanpa
/// perubahan skema). Requested -> Approved (saldo piutang dipulihkan di Finance) atau Rejected. Kosakata "Approved" sama dengan
/// alur restitusi yang sudah ada.
/// </summary>
public static class PayrollReversalStatuses
{
    public const string Requested = "Requested";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
