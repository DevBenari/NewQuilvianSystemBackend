using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services
{
    /// <summary>
    /// Aturan kategori lampiran untuk jenis dokumen tertentu (BE-PBJ-PS-006; PBJ-CAP-004, PBJ-DEC-079, PBJ-DEC-095).
    /// Didaftarkan lewat DI oleh sub-modul pemilik dokumen, mengikuti pola <see cref="IProcurementDocumentAccessProvider"/>.
    /// </summary>
    /// <remarks>
    /// <para>Titik perluasan opsional: jenis dokumen tanpa aturan ini tetap memakai perilaku lama (kategori bebas, semua
    /// lampiran terlihat oleh pemegang hak dokumen). Contoh: lampiran PR <c>PBJ_PR</c> tidak berubah.</para>
    /// <para>Contoh pemakai: paket <c>PBJ_PACKAGE</c> mewajibkan kategori = kode jenis dokumen paket yang aktif (KAK/TOR,
    /// Dasar HPS, …), dan menyembunyikan lampiran berkategori jenis dokumen <c>IsPriceSensitive</c> dari pengguna tanpa
    /// <c>ProcurementPackage : ReadPrice</c>.</para>
    /// </remarks>
    public interface IProcurementAttachmentCategoryPolicy
    {
        /// <summary>Apakah aturan ini berlaku untuk jenis dokumen tersebut, contoh <c>PBJ_PACKAGE</c>.</summary>
        bool Handles(string documentType);

        /// <summary>
        /// Memeriksa kategori lampiran baru dan mengembalikan bentuk bakunya. Melempar
        /// <see cref="ProcurementAttachmentValidationException"/> (422) bila kategori tidak sah.
        /// </summary>
        Task<string> NormalizeCategoryAsync(string documentType, string? category, CancellationToken cancellationToken);

        /// <summary>
        /// Kategori yang boleh dilihat pengguna ini; <c>null</c> = semua kategori terlihat. Lampiran di luar himpunan ini
        /// tidak didaftar, dan unduh/ganti/tariknya dijawab 404 (keberadaannya tidak dibocorkan).
        /// </summary>
        Task<IReadOnlySet<string>?> GetVisibleCategoriesAsync(
            ClaimsPrincipal user, string documentType, CancellationToken cancellationToken);
    }
}
