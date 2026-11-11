namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;

/// <summary>
/// Status siklus PrlPayrollRun.RunStatus. Kolom bertipe string, jadi tidak ada perubahan skema.
/// Alur normal: Draft (run dibuka) -> CollectingInput (data HRD diimpor) -> Calculated (gaji dan potongan terhitung) -> Submitted
/// (diajukan untuk review) -> Approved (disahkan; hasil potongan dikonfirmasi ke Finance dan run terkunci) -> SentToFinance
/// (instruksi pembayaran dikirim) -> PartiallyPaid / Paid (hasil bank dari Finance) -> Closed. Dibatalkan hanya sebelum disahkan;
/// koreksi sesudah disahkan lewat pembalikan/penyesuaian bertanda tangan (bukan menjalankan ulang potongan).
/// Pemetaan ke kosakata usulan prompt: INPUT_READY=CollectingInput, REVIEWED=Submitted, APPROVED+LOCKED=Approved (IsLocked=true).
/// </summary>
public static class PayrollRunStatuses
{
    public const string Draft = "Draft";
    public const string CollectingInput = "CollectingInput";
    public const string Calculated = "Calculated";
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";
    public const string SentToFinance = "SentToFinance";
    public const string PartiallyPaid = "PartiallyPaid";
    public const string Paid = "Paid";
    public const string Posted = "Posted";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";
    public const string Reversed = "Reversed";

    /// <summary>Run yang masih boleh menerima input baru (mis. jadwal angsuran dari Finance) dan dihitung ulang.</summary>
    public static bool AcceptsInputs(string? status) =>
        Is(status, Draft) || Is(status, CollectingInput) || Is(status, Calculated);

    /// <summary>Run yang sudah disahkan atau lebih jauh: hasil potongan sudah final dan tidak boleh dihitung ulang.</summary>
    public static bool IsFinalized(string? status) =>
        Is(status, Approved) || Is(status, SentToFinance) || Is(status, PartiallyPaid) || Is(status, Paid)
        || Is(status, Posted) || Is(status, Closed) || Is(status, Reversed);

    /// <summary>Instruksi pembayaran hanya boleh dikirim ke Finance dari run yang sudah disahkan dan belum dikirim.</summary>
    public static bool AllowsFinanceHandoff(string? status) => Is(status, Approved);

    /// <summary>Tahap pembayaran: instruksi sudah dikirim ke Finance dan hasil bank mulai/masih masuk.</summary>
    public static bool IsPaymentStage(string? status) =>
        Is(status, SentToFinance) || Is(status, PartiallyPaid) || Is(status, Paid);

    /// <summary>
    /// Pembalikan potongan cicilan boleh diajukan selama potongan sudah dikonfirmasi ke Finance, termasuk sesudah pembayaran
    /// (salah potong biasanya baru ketahuan kemudian). Tidak boleh pada run yang sudah ditutup, dibatalkan, atau dibalik.
    /// </summary>
    public static bool AllowsDeductionReversal(string? status) =>
        Is(status, Approved) || Is(status, SentToFinance) || Is(status, PartiallyPaid) || Is(status, Paid);

    /// <summary>Slip dapat dibuat dari snapshot run yang sudah dibekukan (disahkan atau lebih jauh, kecuali dibalik).</summary>
    public static bool AllowsPayslipGeneration(string? status) => IsFinalized(status) && !Is(status, Reversed);

    private static bool Is(string? status, string expected) =>
        string.Equals(status, expected, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Status pembalikan potongan cicilan atas payroll run yang sudah disahkan (PrlPayrollReversal.ReversalStatus; kolom string, tanpa
/// perubahan skema). Requested -> Approved (saldo piutang dipulihkan di Finance) atau Rejected. Kosakata "Approved" sama dengan
/// alur restitusi yang sudah ada.
/// </summary>
public static class PayrollReversalStatuses
{
    public const string Requested = "Requested";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
