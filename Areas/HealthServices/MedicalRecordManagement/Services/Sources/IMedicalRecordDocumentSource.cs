using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources
{
    /// <summary>
    /// Pembaca dokumen final milik satu modul untuk berkas rekam medis (`INT-RM-10`).
    ///
    /// DIIMPLEMENTASIKAN MODUL PEMILIK, BUKAN REKAM MEDIS. `RM-DEC-038` menetapkan bahwa rekam
    /// medis meminta dengan id pasien, dan modul pemilik yang menerjemahkannya menjadi
    /// kunjungan, episode, atau kasusnya sendiri. Karena itu implementasinya tinggal di folder
    /// <c>Services/</c> modul pemilik dan ditulis dengan persetujuan pemilik modul itu.
    ///
    /// Interface dipakai — menyimpang dari kebiasaan service tanpa interface — karena rekam
    /// medis membaca banyak modul lewat <c>IEnumerable&lt;IMedicalRecordDocumentSource&gt;</c>
    /// tanpa bergantung pada service internal masing-masing. Preseden yang sama:
    /// <c>IBillingChargeSourceAdapter</c>, <c>IOpeningItemFileReader</c>.
    ///
    /// Kewajiban implementasi:
    /// <list type="bullet">
    /// <item>hanya dokumen final menurut modul itu (`RM-DEC-041`);</item>
    /// <item>baca saja — tidak menulis, tidak melacak entity;</item>
    /// <item>tidak memeriksa izin modulnya sendiri — izin berkas sudah diperiksa rekam medis
    /// (`RM-DEC-040`);</item>
    /// <item>menghormati <c>CancellationToken</c>: rekam medis membatasi waktu setiap pembaca.</item>
    /// </list>
    /// </summary>
    public interface IMedicalRecordDocumentSource
    {
        /// <summary>Nama modul pemilik untuk pembaca berkas, mis. "Laboratorium".</summary>
        string SourceModule { get; }

        /// <summary>Jenis dokumen yang dibaca pembaca ini. Satu jenis hanya punya satu pembaca.</summary>
        IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; }

        Task<MedicalRecordSourcePage> ReadAsync(
            MedicalRecordSourceQuery query,
            CancellationToken cancellationToken);

        /// <summary>
        /// Isi satu dokumen final untuk layar rincian berkas. Wajib mengembalikan <c>null</c>
        /// bila dokumen tidak ada, belum final, atau BUKAN milik pasien yang diminta — tanpa
        /// pemeriksaan itu, pemegang izin berkas satu pasien dapat membaca dokumen pasien lain
        /// dengan menebak id-nya.
        /// </summary>
        Task<MedicalRecordSourceDocumentDetail?> ReadDetailAsync(
            ClinicalDocumentKind kind,
            Guid documentId,
            Guid patientId,
            CancellationToken cancellationToken);
    }
}
