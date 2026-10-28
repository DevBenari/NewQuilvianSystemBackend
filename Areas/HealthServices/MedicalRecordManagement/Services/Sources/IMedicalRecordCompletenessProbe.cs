using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources
{
    /// <summary>
    /// Pembaca uji kelengkapan satu kunjungan, diimplementasikan modul pemilik dokumen
    /// (`INT-RM-11`, `INT-RM-13`).
    ///
    /// Menjawab tiga hal untuk satu kunjungan: kriteria dokumen wajib mana yang terpenuhi,
    /// kejadian bersyarat mana yang ada (operasi, sesi hemodialisa, transfusi), dan pemeriksaan
    /// mana yang masih menunggu hasil. Kewajiban yang sama dengan
    /// <see cref="IMedicalRecordDocumentSource"/> berlaku: baca saja, arti final milik modul
    /// pemilik, tanpa pemeriksaan izin modul asal.
    ///
    /// Kontrak ini sudah tersedia sejak `BE-21` supaya pembaca sumber (`BE-23` dan seterusnya)
    /// dapat mengimplementasikannya; pemakainya, penilaian kelengkapan, menyusul pada `BE-39`.
    /// </summary>
    public interface IMedicalRecordCompletenessProbe
    {
        string SourceModule { get; }

        IReadOnlyCollection<CompletenessCriterion> Criteria { get; }

        IReadOnlyCollection<CompletenessCondition> Conditions { get; }

        /// <summary>Apakah pembaca ini melaporkan pemeriksaan tertunda (syarat 2).</summary>
        bool ReportsPendingResults { get; }

        Task<MedicalRecordCompletenessProbeResult> ProbeAsync(
            MedicalRecordCompletenessProbeQuery query,
            CancellationToken cancellationToken);
    }
}
