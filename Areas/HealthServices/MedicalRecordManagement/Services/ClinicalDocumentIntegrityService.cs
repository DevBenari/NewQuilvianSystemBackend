using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Hasil pemeriksaan aturan keutuhan.
    ///
    /// Bentuk ini dipilih, bukan melempar exception, karena setiap penolakan punya pesan dan
    /// kode status yang sudah ditetapkan pada validation matrix. Controller tinggal
    /// meneruskannya ke pengguna tanpa menerjemahkan ulang. Pola yang sama sudah dipakai
    /// `ValidateMergedPatientReferenceAsync` pada PatientController.
    /// </summary>
    public sealed record IntegrityGuardResult(bool IsAllowed, int StatusCode, string? ErrorMessage)
    {
        public static IntegrityGuardResult Allowed() => new(true, StatusCodes.Status200OK, null);

        public static IntegrityGuardResult Denied(int statusCode, string message)
            => new(false, statusCode, message);
    }

    /// <summary>
    /// Satu-satunya tempat aturan keutuhan dokumen klinis ditegakkan.
    ///
    /// Sebelum modul ini ada, aturan seperti "catatan yang sudah final tidak boleh diubah"
    /// harus ditulis di setiap controller yang menyentuh dokumen klinis. Itu temuan
    /// `RM-CAP-010`: aturan tersebar dan mudah terlewat. Service ini memusatkannya.
    ///
    /// ATURAN PEMAKAIAN TRANSAKSI — perhatikan bedanya, karena keliru di sini berakibat data
    /// setengah tersimpan:
    ///
    /// <list type="bullet">
    /// <item><see cref="RegisterAsync"/> TIDAK menyimpan. Pemanggil wajib menjalankannya di
    /// dalam transaksi yang sama dengan pembuatan dokumennya. Bila pendaftaran gagal,
    /// pembuatan dokumen harus ikut dibatalkan — dokumen tanpa baris keutuhan akan luput dari
    /// seluruh aturan penguncian.</item>
    /// <item><see cref="LockOpenDocumentsForEncounterAsync"/> TIDAK menyimpan. Pemanggil wajib
    /// menjalankannya di dalam transaksi penutupan kunjungan.</item>
    /// <item><see cref="SignAsync"/> menyimpan sendiri, karena hanya menyentuh satu baris
    /// keutuhan dan tidak mengubah isi dokumen apa pun.</item>
    /// </list>
    /// </summary>
    public class ClinicalDocumentIntegrityService
    {
        private readonly ApplicationDbContext _dbContext;

        public ClinicalDocumentIntegrityService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Jenis dokumen yang tunduk aturan keutuhan pada rilis pertama.
        ///
        /// Sengaja dibatasi satu jenis sesuai `RM-DEC-019`. Karena status keutuhan disimpan di
        /// tabel terpisah, penegakannya bergantung pada service ini benar-benar dipanggil —
        /// dan cakupan yang sempit adalah cara paling kuat memastikan tidak ada yang terlewat.
        ///
        /// Dua belas jenis lain sudah punya nomor pada <see cref="ClinicalDocumentKind"/>,
        /// tetapi belum ditegakkan. Keadaan itu WAJIB dinyatakan terbuka di layar (`RM-FE-009`),
        /// bukan didiamkan.
        /// </summary>
        private static readonly HashSet<ClinicalDocumentKind> JenisYangDitegakkan =
        [
            ClinicalDocumentKind.ProgressNote
        ];

        /// <summary>
        /// Apakah jenis dokumen ini sudah tunduk aturan keutuhan pada rilis sekarang.
        /// </summary>
        public static bool DitegakkanUntuk(ClinicalDocumentKind kind)
            => JenisYangDitegakkan.Contains(kind);

        /// <summary>
        /// Mendaftarkan satu dokumen klinis ke daftar keutuhan, berstatus draf.
        ///
        /// Aman dipanggil berulang: bila dokumen sudah terdaftar, baris yang sudah ada
        /// dikembalikan tanpa membuat baris kedua. Keunikan tetap dijamin index basis data
        /// sebagai lapis terakhir.
        ///
        /// TIDAK menyimpan. Lihat aturan pemakaian transaksi pada keterangan kelas.
        /// </summary>
        public async Task<TrxClinicalDocumentIntegrity> RegisterAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid patientId,
            Guid encounterId,
            Guid authorUserId,
            bool isAuthorKnown = true,
            CancellationToken cancellationToken = default)
        {
            if (documentId == Guid.Empty)
                throw new InvalidOperationException("Id dokumen klinis tidak valid.");

            if (patientId == Guid.Empty)
                throw new InvalidOperationException("Id pasien tidak valid.");

            if (encounterId == Guid.Empty)
                throw new InvalidOperationException("Id kunjungan tidak valid.");

            var sudahAda = await FindAsync(documentKind, documentId, cancellationToken);
            if (sudahAda != null)
                return sudahAda;

            var keutuhan = new TrxClinicalDocumentIntegrity
            {
                DocumentKind = documentKind,
                DocumentId = documentId,
                PatientId = patientId,
                EncounterId = encounterId,
                AuthorUserId = authorUserId,
                IsAuthorKnown = isAuthorKnown,
                IntegrityStatus = ClinicalDocumentIntegrityStatus.Draft,
                CreateBy = authorUserId
            };

            await _dbContext.Set<TrxClinicalDocumentIntegrity>()
                .AddAsync(keutuhan, cancellationToken);

            return keutuhan;
        }

        /// <summary>
        /// Memeriksa apakah sebuah dokumen masih boleh diubah isinya.
        ///
        /// Wajib dipanggil setiap controller sebelum mengubah dokumen klinis. Bila metode ini
        /// tidak dipanggil, aturan penguncian tidak berlaku sama sekali untuk jalur tersebut.
        /// </summary>
        public async Task<IntegrityGuardResult> EnsureMutableAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            // Jenis yang belum ditegakkan dibiarkan lewat. Menolaknya justru akan memblokir
            // alur yang berjalan sekarang, padahal aturannya memang belum berlaku untuk jenis
            // itu. Keadaan ini dinyatakan terbuka di layar, bukan disembunyikan.
            if (!DitegakkanUntuk(documentKind))
                return IntegrityGuardResult.Allowed();

            var keutuhan = await FindAsync(documentKind, documentId, cancellationToken);

            // Dokumen yang belum terdaftar diperlakukan sebagai masih boleh diubah. Ini terjadi
            // pada dokumen lama yang belum tersentuh pengisian data lama.
            if (keutuhan == null)
                return IntegrityGuardResult.Allowed();

            return keutuhan.IntegrityStatus switch
            {
                ClinicalDocumentIntegrityStatus.Draft
                    => IntegrityGuardResult.Allowed(),

                ClinicalDocumentIntegrityStatus.Signed or
                ClinicalDocumentIntegrityStatus.LockedUnsigned
                    => IntegrityGuardResult.Denied(
                        StatusCodes.Status400BadRequest,
                        "Catatan ini sudah ditandatangani dan tidak dapat diubah. " +
                        "Gunakan addendum untuk membetulkan."),

                ClinicalDocumentIntegrityStatus.Cancelled
                    => IntegrityGuardResult.Denied(
                        StatusCodes.Status400BadRequest,
                        "Catatan ini sudah dibatalkan dan tidak dapat diubah."),

                _ => IntegrityGuardResult.Denied(
                        StatusCodes.Status400BadRequest,
                        "Status keutuhan catatan tidak dikenali.")
            };
        }

        /// <summary>
        /// Menandatangani dokumen, sekaligus menguncinya.
        ///
        /// Hanya penulis dokumen yang boleh menandatangani. Perangkat dan alamat jaringan
        /// diambil pemanggil dari permintaan HTTP, bukan dari kiriman klien — bila dikirim
        /// klien nilainya dapat dipalsukan dan kehilangan makna sebagai bukti (`RM-DEC-021`).
        ///
        /// Menyimpan sendiri, karena hanya menyentuh satu baris keutuhan.
        /// </summary>
        public async Task<(IntegrityGuardResult Result, TrxClinicalDocumentIntegrity? Integrity)> SignAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            var keutuhan = await FindAsync(documentKind, documentId, cancellationToken);

            if (keutuhan == null)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status404NotFound,
                    "Catatan tidak ditemukan pada daftar keutuhan."), null);
            }

            if (keutuhan.AuthorUserId != actorUserId)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status403Forbidden,
                    "Hanya penulis catatan yang dapat menandatanganinya."), null);
            }

            if (keutuhan.IntegrityStatus != ClinicalDocumentIntegrityStatus.Draft)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Catatan ini sudah terkunci. Gunakan addendum bila perlu melengkapi."), null);
            }

            keutuhan.IntegrityStatus = ClinicalDocumentIntegrityStatus.Signed;
            keutuhan.SignedAt = nowUtc;
            keutuhan.SignedByUserId = actorUserId;
            keutuhan.SignatureDeviceInfo = Potong(deviceInfo, 250);
            keutuhan.SignatureIpAddress = Potong(ipAddress, 64);
            keutuhan.LockedAt = nowUtc;
            keutuhan.LockTrigger = ClinicalDocumentLockTrigger.AuthorSigned;
            keutuhan.UpdateDateTime = nowUtc;
            keutuhan.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (IntegrityGuardResult.Allowed(), keutuhan);
        }

        /// <summary>
        /// Mengunci seluruh dokumen yang masih berstatus draf pada satu kunjungan.
        ///
        /// Dipicu ketika kunjungan berpindah menuju status selesai. Ini lapis kedua
        /// `RM-DEC-003`: jaring pengaman supaya tidak ada catatan yang menggantung terbuka
        /// selamanya karena penulisnya lupa menandatangani.
        ///
        /// Aman dipanggil berulang — dokumen yang sudah terkunci dilewati.
        ///
        /// TIDAK menyimpan. Pemanggil wajib menjalankannya di dalam transaksi penutupan
        /// kunjungan, supaya penutupan ikut dibatalkan bila penguncian gagal.
        /// </summary>
        /// <param name="batchSize">
        /// Banyaknya dokumen yang diproses sekali ambil. Kunjungan rawat inap yang panjang
        /// dapat memuat sangat banyak dokumen; mengambilnya sekaligus membuat transaksi
        /// menahan tabel terlalu lama.
        /// </param>
        /// <returns>Jumlah dokumen yang terkunci oleh pemanggilan ini.</returns>
        public async Task<int> LockOpenDocumentsForEncounterAsync(
            Guid encounterId,
            Guid actorUserId,
            DateTime nowUtc,
            DateTime? encounterClosedAtUtc = null,
            int batchSize = 200,
            CancellationToken cancellationToken = default)
        {
            if (encounterId == Guid.Empty)
                throw new InvalidOperationException("Id kunjungan tidak valid.");

            if (batchSize <= 0)
                throw new InvalidOperationException("Ukuran potongan penguncian harus lebih dari nol.");

            var jumlahTerkunci = 0;

            while (true)
            {
                var potongan = await _dbContext.Set<TrxClinicalDocumentIntegrity>()
                    .Where(x => x.EncounterId == encounterId
                                && x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Draft
                                && !x.IsDelete)
                    .OrderBy(x => x.CreateDateTime)
                    .Skip(jumlahTerkunci)
                    .Take(batchSize)
                    .ToListAsync(cancellationToken);

                if (potongan.Count == 0)
                    break;

                foreach (var keutuhan in potongan)
                {
                    keutuhan.IntegrityStatus = ClinicalDocumentIntegrityStatus.LockedUnsigned;
                    keutuhan.LockedAt = nowUtc;
                    keutuhan.LockTrigger = ClinicalDocumentLockTrigger.EncounterClosed;
                    keutuhan.LockedEncounterClosedAt = encounterClosedAtUtc ?? nowUtc;
                    keutuhan.UpdateDateTime = nowUtc;
                    keutuhan.UpdateBy = actorUserId;
                }

                jumlahTerkunci += potongan.Count;

                if (potongan.Count < batchSize)
                    break;
            }

            return jumlahTerkunci;
        }

        /// <summary>
        /// Mengambil baris keutuhan sebuah dokumen, atau null bila belum terdaftar.
        /// </summary>
        public Task<TrxClinicalDocumentIntegrity?> FindAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken = default)
            => _dbContext.Set<TrxClinicalDocumentIntegrity>()
                .FirstOrDefaultAsync(
                    x => x.DocumentKind == documentKind
                         && x.DocumentId == documentId
                         && !x.IsDelete,
                    cancellationToken);

        private static string? Potong(string? nilai, int panjangMaksimum)
        {
            if (string.IsNullOrWhiteSpace(nilai))
                return null;

            var bersih = nilai.Trim();
            return bersih.Length <= panjangMaksimum
                ? bersih
                : bersih[..panjangMaksimum];
        }
    }
}
