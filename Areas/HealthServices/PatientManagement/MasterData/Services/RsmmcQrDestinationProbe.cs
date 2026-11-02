using System.Security.Cryptography;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>Hasil pemeriksaan QR tujuan dan QR historis satu MRN, tanpa menulis apa pun.</summary>
    public sealed record RsmmcQrDestinationProbeResult(
        bool PlannedExists,
        bool HistoricalExists,
        long? HistoricalLength,
        string? HistoricalSha256);

    /// <summary>
    /// Pemeriksa baca-saja keberadaan berkas QR untuk worker migrasi massal (`PAT-DEC-046`).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hanya membaca keberadaan, panjang, dan SHA-256 berkas. Tidak pernah membuat folder, membuat,
    /// mengubah, memindah, atau menghapus berkas — termasuk tidak memanggil penentu lokasi milik
    /// pembuatan pasien yang membuat folder (`PAT-SRC-015`). Karena itu simulasi aman dijalankan.
    /// </para>
    /// <para>
    /// Path rencana memakai namespace <c>patient-qrcodes/rsmmc-bulk/&lt;MRN&gt;/qrcode.png</c>; path bawaan
    /// historis <c>patient-qrcodes/&lt;MRN&gt;/qrcode.png</c> hanya dilaporkan sebagai informasi, bukan
    /// konflik (`PAT-DEC-052`).
    /// </para>
    /// </remarks>
    public sealed class RsmmcQrDestinationProbe
    {
        private readonly Func<string> _resolveStorageRootWithoutCreating;
        private readonly RsmmcPilotQrArtifactLayout _plannedLayout;
        private readonly RsmmcPilotQrArtifactLayout _historicalLayout;
        private readonly Func<string, string> _sanitizePathSegment;

        public RsmmcQrDestinationProbe(
            Func<string> resolveStorageRootWithoutCreating,
            RsmmcPilotQrArtifactLayout plannedLayout,
            RsmmcPilotQrArtifactLayout historicalLayout,
            Func<string, string> sanitizePathSegment)
        {
            _resolveStorageRootWithoutCreating = resolveStorageRootWithoutCreating;
            _plannedLayout = plannedLayout;
            _historicalLayout = historicalLayout;
            _sanitizePathSegment = sanitizePathSegment;
        }

        /// <summary>Apakah path rencana <c>rsmmc-bulk</c> untuk MRN ini sudah terisi.</summary>
        public bool PlannedExists(string medicalRecordNumber) =>
            File.Exists(PhysicalPath(_plannedLayout, medicalRecordNumber));

        /// <summary>Keadaan path rencana dan path historis, termasuk panjang dan SHA-256 berkas historis.</summary>
        public RsmmcQrDestinationProbeResult Probe(string medicalRecordNumber)
        {
            var plannedExists = PlannedExists(medicalRecordNumber);
            var historical = new FileInfo(PhysicalPath(_historicalLayout, medicalRecordNumber));

            if (!historical.Exists)
            {
                return new RsmmcQrDestinationProbeResult(plannedExists, false, null, null);
            }

            string sha256;

            using (var reader = new FileStream(historical.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                sha256 = Convert.ToHexString(SHA256.HashData(reader)).ToLowerInvariant();
            }

            return new RsmmcQrDestinationProbeResult(plannedExists, true, historical.Length, sha256);
        }

        private string PhysicalPath(RsmmcPilotQrArtifactLayout layout, string medicalRecordNumber)
        {
            var folder = _sanitizePathSegment(medicalRecordNumber);

            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new InvalidOperationException("Nomor rekam medis tidak menghasilkan nama folder QR yang sah.");
            }

            return Path.Combine(_resolveStorageRootWithoutCreating(), layout.QrFolderName, folder, layout.QrFileName);
        }
    }
}
