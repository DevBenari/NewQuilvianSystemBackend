using System.Security.Cryptography;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Tata letak folder dan path publik QR pasien, sama dengan yang dipakai pembuatan pasien.
    /// </summary>
    public sealed record RsmmcPilotQrArtifactLayout(
        string PublicRequestPath,
        string QrFolderName,
        string QrFileName);

    /// <summary>
    /// Fungsi format QR milik <c>PatientController</c> yang dipakai ulang apa adanya, supaya
    /// isi QR, nama folder, dan path publiknya tidak punya dua sumber.
    /// </summary>
    public sealed record RsmmcPilotQrArtifactFormat(
        Func<string, string> SanitizePathSegment,
        Func<string, string> BuildPayload,
        Func<string, string?, string?, byte[]> RenderPng,
        Func<string[], string> CombineUrlPath);

    /// <summary>
    /// Artefak QR yang dibuat oleh satu pemanggilan <see cref="RsmmcPilotQrArtifactStore.CreateNew"/>,
    /// beserta bukti kepemilikannya.
    /// </summary>
    /// <remarks>
    /// Bukti kepemilikan terdiri atas tiga hal sekaligus: penanda
    /// <see cref="CreatedByThisInvocation"/>, panjang berkas yang ditulis, dan digest SHA-256 dari
    /// byte yang ditulis. Ukuran saja bukan bukti: berkas lain dapat kebetulan sama panjangnya.
    /// </remarks>
    public sealed record RsmmcPilotCreatedQrArtifact(
        string PublicPath,
        string PhysicalPath,
        string AbsoluteFolder,
        bool CreatedFolder,
        bool CreatedByThisInvocation,
        long Length,
        string Sha256);

    public sealed record RsmmcPilotQrCreateResult(
        RsmmcPilotCreatedQrArtifact? Created,
        bool IsConflict,
        bool IsRaceConflict,
        string PublicPath)
    {
        public static RsmmcPilotQrCreateResult Conflict(string publicPath, bool race) =>
            new(null, true, race, publicPath);

        public static RsmmcPilotQrCreateResult Success(RsmmcPilotCreatedQrArtifact created) =>
            new(created, false, false, created.PublicPath);
    }

    public enum RsmmcPilotQrCleanupOutcome
    {
        /// <summary>Kepemilikan terbukti dan berkas dihapus.</summary>
        Deleted,

        /// <summary>Artefak tidak dibuat oleh pemanggilan ini. Tidak dihapus.</summary>
        NotCreatedByThisInvocation,

        /// <summary>Berkas sudah tidak ada. Tidak ada yang dihapus.</summary>
        AlreadyMissing,

        /// <summary>Panjang atau digest tidak cocok. Kepemilikan tidak terbukti, tidak dihapus.</summary>
        OwnershipUnproven,

        /// <summary>Pemeriksaan atau penghapusan gagal karena galat filesystem. Tidak dianggap terhapus.</summary>
        Failed
    }

    public sealed record RsmmcPilotQrCleanupResult(RsmmcPilotQrCleanupOutcome Outcome, Exception? Error = null)
    {
        public bool Deleted => Outcome == RsmmcPilotQrCleanupOutcome.Deleted;
    }

    /// <summary>
    /// Membuat QR pasien untuk MRN kanonik tanpa pernah menimpa artefak yang sudah ada, dan hanya
    /// membersihkan artefak yang terbukti dibuatnya sendiri.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Helper <c>SavePatientQrCodeFile</c> milik pembuatan pasien menulis dengan
    /// <c>File.WriteAllBytes</c>, yang menimpa file yang sudah ada. Memeriksa keberadaan file lalu
    /// memanggil helper itu masih menyisakan jeda: proses lain dapat membuat file yang sama di
    /// antara pemeriksaan dan penulisan. Karena itu batas penulisan terakhir di sini memakai
    /// <see cref="FileMode.CreateNew"/>, yang ditolak sistem operasi bila file sudah ada. Pemeriksaan
    /// awal hanya mempercepat penolakan; yang menjamin tidak ada penimpaan adalah <c>CreateNew</c>.
    /// </para>
    /// <para>
    /// Path dan isi QR dihitung lewat fungsi format milik <c>PatientController</c>. Lokasi
    /// penyimpanan diminta secara malas lewat <c>resolveStorage</c>, sehingga simulasi yang hanya
    /// memanggil <see cref="BuildPublicPath"/> tidak menyentuh filesystem sama sekali.
    /// </para>
    /// </remarks>
    public sealed class RsmmcPilotQrArtifactStore
    {
        private readonly RsmmcPilotQrArtifactLayout _layout;
        private readonly Func<(string RootPath, string? LogoPath)> _resolveStorage;
        private readonly RsmmcPilotQrArtifactFormat _format;

        public RsmmcPilotQrArtifactStore(
            RsmmcPilotQrArtifactLayout layout,
            Func<(string RootPath, string? LogoPath)> resolveStorage,
            RsmmcPilotQrArtifactFormat format)
        {
            _layout = layout;
            _resolveStorage = resolveStorage;
            _format = format;
        }

        /// <summary>
        /// Path publik QR untuk sebuah MRN. Murni perhitungan string, tanpa akses filesystem.
        /// </summary>
        public string BuildPublicPath(string medicalRecordNumber)
        {
            var relativeFolder = BuildRelativeFolder(medicalRecordNumber);

            return _format.CombineUrlPath(
            [
                _layout.PublicRequestPath,
                relativeFolder.Replace("\\", "/"),
                _layout.QrFileName
            ]);
        }

        /// <summary>
        /// Membuat QR baru untuk MRN ini. Mengembalikan konflik bila artefak tujuan sudah ada,
        /// termasuk bila ia muncul di antara pemeriksaan awal dan penulisan.
        /// </summary>
        /// <exception cref="Exception">
        /// Kegagalan membuat QR — misalnya PNG gagal dibentuk atau folder tidak dapat ditulis.
        /// Bila penulisan gagal sesudah berkas tujuan dibuat, berkas sebagian itu tidak lagi
        /// cocok dengan digest-nya dan sengaja tidak dihapus; pesannya menyebut path-nya.
        /// </exception>
        public RsmmcPilotQrCreateResult CreateNew(string medicalRecordNumber, string? traceId)
        {
            var publicPath = BuildPublicPath(medicalRecordNumber);
            var storage = _resolveStorage();
            var absoluteFolder = Path.Combine(storage.RootPath, BuildRelativeFolder(medicalRecordNumber));
            var physicalPath = Path.Combine(absoluteFolder, _layout.QrFileName);

            if (File.Exists(physicalPath))
            {
                return RsmmcPilotQrCreateResult.Conflict(publicPath, race: false);
            }

            // PNG dan digest-nya dibentuk sebelum apa pun ditulis, supaya kegagalannya tidak
            // meninggalkan berkas, dan supaya bukti kepemilikan berasal dari byte yang memang ditulis.
            var payload = _format.BuildPayload(medicalRecordNumber);
            var pngBytes = _format.RenderPng(payload, storage.LogoPath, traceId);
            var sha256 = ComputeSha256(pngBytes);

            var folderExisted = Directory.Exists(absoluteFolder);
            Directory.CreateDirectory(absoluteFolder);

            FileStream stream;

            try
            {
                stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            }
            catch (IOException) when (File.Exists(physicalPath))
            {
                // Artefak muncul sesudah pemeriksaan awal. CreateNew menolak, jadi tidak ada byte
                // yang ditulis ke file itu, dan file itu bukan milik pemanggilan ini.
                return RsmmcPilotQrCreateResult.Conflict(publicPath, race: true);
            }
            catch
            {
                TryDeleteEmptyFolder(absoluteFolder, createdByThisCall: !folderExisted);
                throw;
            }

            try
            {
                using (stream)
                {
                    stream.Write(pngBytes, 0, pngBytes.Length);
                    stream.Flush(flushToDisk: true);
                }
            }
            catch (Exception ex)
            {
                // Isi berkas sebagian tidak lagi cocok dengan digest, sehingga kepemilikannya tidak
                // dapat dibuktikan dengan cara yang sama seperti artefak utuh. Berkas itu sengaja
                // tidak dihapus; operator yang memutuskan.
                throw new IOException(
                    $"Penulisan QR baru gagal sesudah berkas tujuan dibuat. Berkas sebagian mungkin tertinggal di {publicPath} dan tidak dihapus otomatis.",
                    ex);
            }

            return RsmmcPilotQrCreateResult.Success(new RsmmcPilotCreatedQrArtifact(
                publicPath,
                physicalPath,
                absoluteFolder,
                CreatedFolder: !folderExisted,
                CreatedByThisInvocation: true,
                Length: pngBytes.LongLength,
                Sha256: sha256));
        }

        /// <summary>
        /// Menghapus artefak yang dibuat <see cref="CreateNew"/> hanya bila kepemilikannya terbukti:
        /// dibuat pemanggilan ini, berkasnya masih ada, panjangnya sama, dan digest SHA-256 isinya
        /// sama. Selain itu tidak ada yang dihapus. Tidak pernah rekursif; foldernya hanya dihapus
        /// bila dibuat pemanggilan yang sama dan sudah kosong.
        /// </summary>
        public RsmmcPilotQrCleanupResult TryDeleteCreated(RsmmcPilotCreatedQrArtifact artifact)
        {
            if (!artifact.CreatedByThisInvocation)
            {
                return new(RsmmcPilotQrCleanupOutcome.NotCreatedByThisInvocation);
            }

            try
            {
                var file = new FileInfo(artifact.PhysicalPath);

                if (!file.Exists)
                {
                    return new(RsmmcPilotQrCleanupOutcome.AlreadyMissing);
                }

                if (file.Length != artifact.Length)
                {
                    return new(RsmmcPilotQrCleanupOutcome.OwnershipUnproven);
                }

                string actualSha256;

                using (var reader = new FileStream(artifact.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    actualSha256 = Convert.ToHexString(SHA256.HashData(reader)).ToLowerInvariant();
                }

                if (!string.Equals(actualSha256, artifact.Sha256, StringComparison.Ordinal))
                {
                    return new(RsmmcPilotQrCleanupOutcome.OwnershipUnproven);
                }

                file.Delete();
                TryDeleteEmptyFolder(artifact.AbsoluteFolder, artifact.CreatedFolder);

                return new(RsmmcPilotQrCleanupOutcome.Deleted);
            }
            catch (Exception ex)
            {
                return new(RsmmcPilotQrCleanupOutcome.Failed, ex);
            }
        }

        private static string ComputeSha256(byte[] bytes) =>
            Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        private string BuildRelativeFolder(string medicalRecordNumber)
        {
            var qrFolderName = _format.SanitizePathSegment(medicalRecordNumber);

            if (string.IsNullOrWhiteSpace(qrFolderName))
            {
                throw new InvalidOperationException("Nomor rekam medis tidak menghasilkan nama folder QR yang sah.");
            }

            return Path.Combine(_layout.QrFolderName, qrFolderName);
        }

        private static void TryDeleteEmptyFolder(string absoluteFolder, bool createdByThisCall)
        {
            if (!createdByThisCall || !Directory.Exists(absoluteFolder))
            {
                return;
            }

            try
            {
                if (!Directory.EnumerateFileSystemEntries(absoluteFolder).Any())
                {
                    Directory.Delete(absoluteFolder, recursive: false);
                }
            }
            catch (IOException)
            {
                // Folder kosong yang tertinggal tidak berbahaya; jangan menutupi galat aslinya.
            }
            catch (UnauthorizedAccessException)
            {
                // Sama seperti di atas.
            }
        }
    }
}
