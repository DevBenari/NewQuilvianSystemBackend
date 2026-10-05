namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;

/// <summary>
/// Konfigurasi dua kunci bukti pembayaran langsung (BE-FIN-074, FIN-DES-092). Dibaca lewat
/// IOptions&lt;FinanceTransactionProofOptions&gt;; diisi bagian "FinanceManagement:TransactionProof"
/// pada appsettings.json atau konfigurasi lingkungan — pola <c>&lt;Modul&gt;:&lt;Fitur&gt;:AllowedExtensions</c>
/// yang sama dengan <c>HumanResource:WorkflowAttachment:AllowedExtensions</c>
/// (<c>WorkflowFileStorageService.ResolveAllowedExtensions</c>).
///
/// Kedua kunci ini SENGAJA berperilaku berbeda ketika kosong (FIN-DES-092) — bukan ketidakkonsistenan:
/// <list type="bullet">
/// <item><see cref="AllowedExtensions"/> memakai daftar bawaan bila kuncinya tidak ada, karena
/// daftarnya sudah diputuskan FIN-DEC-139 — konfigurasi hanya tempat mengubahnya kelak.</item>
/// <item><see cref="MaxFileSizeBytes"/> TIDAK diberi nilai bawaan (tetap <c>null</c>) karena
/// angkanya belum pernah diputuskan (FIN-OQ-082). Pemanggil (FinanceTransactionProofService,
/// BE-FIN-075) MUST menolak unggah fail-closed (503) selama nilainya <c>null</c> — memberi bawaan di
/// sini berarti mengarang keputusan owner.</item>
/// </list>
///
/// Pemetaan ekstensi ke tipe media (pemeriksaan #6 FIN-DES-092 M.3) SENGAJA TIDAK digambar di sini —
/// itu milik validasi unggah BE-FIN-075, bukan pembacaan konfigurasi task ini.
/// </summary>
public sealed class FinanceTransactionProofOptions
{
    private static readonly string[] DefaultAllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];

    /// <summary>
    /// Ekstensi berkas bukti yang diizinkan. Nilai bawaan (bila konfigurasi tidak ada):
    /// .pdf, .jpg, .jpeg, .png (FIN-DEC-139).
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = [.. DefaultAllowedExtensions];

    /// <summary>
    /// Batas ukuran berkas bukti dalam bytes. <c>null</c> berarti BELUM DITETAPKAN (FIN-OQ-082) —
    /// pemanggil MUST menolak unggah fail-closed, BUKAN memperlakukannya sebagai tak terbatas.
    /// </summary>
    public long? MaxFileSizeBytes { get; set; }
}
