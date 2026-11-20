using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Penahan Finance Handoff produksi, dipisahkan dari urusan iuran BPJS (PAY-BE-FIX-007).
///
/// Sebelumnya penahan ini bernama "mode pengujian tanpa BPJS" dan diturunkan dari saklar <c>EnableBpjsInPayroll</c>. Fitur iuran BPJS
/// pada Payroll kini dihapus dari jalur aktif sehingga saklar itu tidak ada lagi, tetapi keputusan pemilik proses untuk MELEPAS penahan
/// belum ada. Karena itu penahan dipertahankan apa adanya sebagai aturan tersendiri: setiap payroll run yang dihitung mesin ini
/// ditandai <c>financeHandoffHeld = true</c> pada snapshot kebijakannya, dan tetap tidak dapat dikirim ke Finance Handoff produksi
/// sampai pemilik proses menetapkan aturan penggantinya (NEEDS BUSINESS DECISION). Penghapusan fitur BPJS BUKAN izin kirim ke Finance.
///
/// Pembaca tetap mengenali dua kunci lama pada snapshot run historis (<c>testingModeWithoutBpjs</c> dan <c>enableBpjsInPayroll = false</c>)
/// supaya run yang sudah ditahan sebelumnya tidak terbuka dengan sendirinya. Kunci lama itu tidak lagi ditulis pada snapshot baru.
/// </summary>
public static class PayrollFinanceHandoffGuard
{
    /// <summary>Alasan penahan yang ditulis pada snapshot kebijakan run baru.</summary>
    public const string HoldReasonPendingOwnerDecision = "PENDING_OWNER_DECISION";

    /// <summary>Pesan penolakan pada setiap jalur Finance Handoff produksi untuk run yang ditahan.</summary>
    public const string HeldMessage =
        "Payroll run ini ditahan dari Finance Handoff produksi sampai pemilik proses menetapkan aturan pengiriman Payroll "
        + "tanpa iuran BPJS. Hubungi pemilik proses Payroll.";

    /// <summary>Label ringkas untuk layar Payroll Run.</summary>
    public const string HeldLabel = "Finance Handoff produksi ditahan menunggu keputusan pemilik proses";

    /// <summary>
    /// True bila snapshot kebijakan run menandai penahan Finance Handoff produksi, baik lewat kunci baru maupun kunci lama pada run
    /// historis. Snapshot kosong atau tidak dapat dibaca = tidak ditahan (perilaku sama dengan sebelumnya).
    /// </summary>
    public static bool IsHeld(string? policySnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(policySnapshotJson)) return false;
        try
        {
            using var document = JsonDocument.Parse(policySnapshotJson);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return false;

            if (root.TryGetProperty("financeHandoffHeld", out var held) && held.ValueKind == JsonValueKind.True) return true;

            // Kunci historis (run yang dihitung saat saklar BPJS nonaktif, 18-20 Nov 2026): tetap ditahan.
            if (root.TryGetProperty("testingModeWithoutBpjs", out var legacyTesting) && legacyTesting.ValueKind == JsonValueKind.True) return true;
            if (root.TryGetProperty("enableBpjsInPayroll", out var legacyEnable) && legacyEnable.ValueKind == JsonValueKind.False) return true;
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>Alasan penahan untuk tampilan: run yang belum dihitung selalu dianggap ditahan karena run baru akan ditandai saat dihitung.</summary>
    public static string? ReasonOf(string? policySnapshotJson, bool calculated)
    {
        if (!calculated) return HoldReasonPendingOwnerDecision;
        return IsHeld(policySnapshotJson) ? HoldReasonPendingOwnerDecision : null;
    }
}
