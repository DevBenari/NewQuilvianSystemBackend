using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services
{
    /// <summary>Hak seorang pengguna atas satu dokumen Pengadaan, dilihat dari sisi lampiran.</summary>
    public enum ProcurementDocumentAccessLevel
    {
        /// <summary>Dokumen tidak ada atau di luar cakupan pengguna → 404.</summary>
        None = 0,

        /// <summary>Boleh melihat dan mengunduh lampiran, tetapi dokumen sudah final atau bukan haknya untuk diubah.</summary>
        Read = 1,

        /// <summary>Boleh menambah dan menarik lampiran (dokumen masih dapat diubah pengguna ini).</summary>
        Edit = 2
    }

    /// <summary>
    /// Penentu hak atas dokumen induk lampiran (BE-PBJ-004). Setiap jenis dokumen
    /// (<c>ProcurementDocumentTypes</c>) mendaftarkan satu penyedia lewat DI pada task yang membuat
    /// dokumennya (BE-PBJ-006 untuk PR, BE-PBJ-009 untuk rencana, dst.).
    /// </summary>
    /// <remarks>
    /// Interface ini titik perluasan, bukan abstraksi service biasa: lampiran tidak boleh mengenal
    /// aturan tiap dokumen. <b>Fail-closed:</b> jenis dokumen tanpa penyedia dijawab
    /// <see cref="ProcurementDocumentAccessLevel.None"/>, sehingga lampirannya tidak dapat dibaca
    /// maupun ditambah sampai dokumen pemiliknya ada.
    /// </remarks>
    public interface IProcurementDocumentAccessProvider
    {
        /// <summary>Jenis dokumen yang dilayani, contoh <c>PBJ_PR</c>.</summary>
        string DocumentType { get; }

        Task<ProcurementDocumentAccessLevel> GetAccessAsync(
            ClaimsPrincipal user,
            Guid documentId,
            CancellationToken cancellationToken);
    }
}
