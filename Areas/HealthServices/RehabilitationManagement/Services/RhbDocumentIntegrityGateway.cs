using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Pintu tunggal dokumen klinis Rehab Medik ke mesin keutuhan Rekam Medis —
    /// <c>02-backend-architecture.md</c> bagian 4.2, <c>integration-contract.md</c> INT-RHB-05,
    /// RHB-DEC-007/019.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rehab tidak membuat kunci, tanda tangan, maupun addendum sendiri. Setiap service dokumen
    /// rehab wajib lewat sini: daftarkan saat draft dibuat, periksa sebelum setiap perubahan,
    /// tanda tangani saat final, tandai batal saat draft dibatalkan.
    /// </para>
    /// <para>
    /// <b>Aturan transaksi</b> mengikuti <see cref="ClinicalDocumentIntegrityService"/>:
    /// <see cref="RegisterDraftAsync"/> dan <see cref="MarkCancelledAsync"/> tidak menyimpan dan
    /// harus berada di transaksi pemanggil; <see cref="SignAsync"/> memanggil
    /// <c>SaveChanges</c> sendiri, sehingga pemanggil yang mengubah status dokumen rehab bersamaan
    /// dengan tanda tangan wajib membuka transaksi basis data lebih dulu agar keduanya tersimpan
    /// atau gagal bersama.
    /// </para>
    /// <para>
    /// <b>Akibat penutupan kunjungan.</b> Mesin Rekam Medis mengunci draft yang masih terbuka pada
    /// sebuah kunjungan saat kunjungan itu ditutup (<c>LockOpenDocumentsForEncounterAsync</c>,
    /// status <c>LockedUnsigned</c>). Draft rehab yang belum ditandatangani sebelum kunjungan RJ
    /// ditutup tidak bisa lagi disunting; penyelesaiannya lewat addendum atestasi Rekam Medis.
    /// </para>
    /// </remarks>
    public class RhbDocumentIntegrityGateway
    {
        private readonly ClinicalDocumentIntegrityService _integrityService;

        public RhbDocumentIntegrityGateway(ClinicalDocumentIntegrityService integrityService)
        {
            _integrityService = integrityService;
        }

        /// <summary>
        /// Jenis dokumen yang dimiliki Rehab Medik. Dipakai pembaca berkas rekam medis rehab
        /// (<c>BE-RHB-015</c>) dan pemeriksaan kelengkapan episode.
        /// </summary>
        public static readonly IReadOnlyCollection<ClinicalDocumentKind> RehabDocumentKinds =
        [
            ClinicalDocumentKind.RehabAssessment,
            ClinicalDocumentKind.RehabProgram,
            ClinicalDocumentKind.RehabSession,
            ClinicalDocumentKind.RehabProgressReview,
            ClinicalDocumentKind.RehabDischargeSummary,
            ClinicalDocumentKind.RehabEducation
        ];

        /// <summary>
        /// Mendaftarkan dokumen rehab berstatus draf. Tidak menyimpan; jalankan di transaksi yang
        /// sama dengan pembuatan dokumennya.
        /// </summary>
        public Task<MrcClinicalDocumentIntegrity> RegisterDraftAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid patientId,
            Guid encounterId,
            Guid authorUserId,
            CancellationToken cancellationToken)
        {
            EnsureRehabKind(documentKind);
            return _integrityService.RegisterAsync(
                documentKind,
                documentId,
                patientId,
                encounterId,
                authorUserId,
                isAuthorKnown: true,
                cancellationToken);
        }

        /// <summary>
        /// Memeriksa apakah isi dokumen masih boleh diubah. Wajib dipanggil sebelum setiap
        /// perubahan isi dokumen rehab.
        /// </summary>
        public Task<IntegrityGuardResult> EnsureMutableAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            EnsureRehabKind(documentKind);
            return _integrityService.EnsureMutableAsync(documentKind, documentId, cancellationToken);
        }

        /// <summary>
        /// Menandatangani sekaligus mengunci dokumen. Hanya penulis yang boleh; perangkat dan
        /// alamat jaringan diambil pemanggil dari permintaan HTTP, bukan dari kiriman klien.
        /// Menyimpan sendiri — lihat aturan transaksi pada keterangan kelas.
        /// </summary>
        public Task<(IntegrityGuardResult Result, MrcClinicalDocumentIntegrity? Integrity)> SignAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            EnsureRehabKind(documentKind);
            return _integrityService.SignAsync(
                documentKind,
                documentId,
                actorUserId,
                deviceInfo,
                ipAddress,
                nowUtc,
                cancellationToken);
        }

        /// <summary>Menandai dokumen draf dibatalkan. Tidak menyimpan; jalankan di transaksi pemanggil.</summary>
        public Task<MrcClinicalDocumentIntegrity?> MarkCancelledAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid actorUserId,
            string? reason,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            EnsureRehabKind(documentKind);
            return _integrityService.MarkCancelledAsync(
                documentKind,
                documentId,
                actorUserId,
                reason,
                nowUtc,
                cancellationToken);
        }

        /// <summary>Membaca baris keutuhan sebuah dokumen rehab, atau <c>null</c> bila belum terdaftar.</summary>
        public Task<MrcClinicalDocumentIntegrity?> FindAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            EnsureRehabKind(documentKind);
            return _integrityService.FindAsync(documentKind, documentId, cancellationToken);
        }

        /// <summary>
        /// Mencegah gateway ini dipakai untuk dokumen milik modul lain, dan memastikan jenis rehab
        /// benar-benar ditegakkan Rekam Medis — tanpa itu dokumen tampil ditandatangani tetapi
        /// tetap dapat disunting (Temuan Kritis HD).
        /// </summary>
        private static void EnsureRehabKind(ClinicalDocumentKind documentKind)
        {
            if (!RehabDocumentKinds.Contains(documentKind))
                throw new InvalidOperationException("Jenis dokumen bukan milik Rehab Medik.");

            if (!ClinicalDocumentIntegrityService.DitegakkanUntuk(documentKind))
                throw new InvalidOperationException("Jenis dokumen Rehab Medik belum ditegakkan aturan keutuhan Rekam Medis.");
        }
    }
}
