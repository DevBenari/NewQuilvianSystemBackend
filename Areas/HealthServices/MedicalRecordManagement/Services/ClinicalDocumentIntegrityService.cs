using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

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
    /// <item><see cref="RegisterSignedAsync"/> TIDAK menyimpan, dengan alasan yang sama.
    /// Dipakai pada saat dokumen difinalkan.</item>
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
        /// Jenis dokumen yang tunduk aturan keutuhan pada rilis sekarang.
        ///
        /// Sengaja dibatasi. Karena status keutuhan disimpan di tabel terpisah, penegakannya
        /// bergantung pada service ini benar-benar dipanggil — dan cakupan yang sempit adalah
        /// cara paling kuat memastikan tidak ada yang terlewat.
        ///
        /// Rilis pertama hanya memuat catatan terpadu sesuai `RM-DEC-019`. `BE-RWI-038`
        /// menambahkan tiga jenis yang finalisasinya kini mendaftarkan dokumen sebagai
        /// tertanda tangan: catatan dokter, kajian medis, dan tindakan. Alasannya bukan
        /// kelengkapan, melainkan `RWI-FACT-014`: sebelum ini hanya catatan terpadu yang
        /// terdaftar, sehingga catatan dokter yang sudah diselesaikan tidak dapat disunting
        /// **maupun** dikoreksi — satu-satunya jalan membetulkan salah ketik adalah menulis
        /// catatan baru yang membantah catatan lama.
        ///
        /// Sembilan jenis lain sudah punya nomor pada <see cref="ClinicalDocumentKind"/>,
        /// tetapi belum ditegakkan. Keadaan itu WAJIB dinyatakan terbuka di layar (`RM-FE-009`),
        /// bukan didiamkan.
        /// </summary>
        private static readonly HashSet<ClinicalDocumentKind> JenisYangDitegakkan =
        [
            ClinicalDocumentKind.ProgressNote,
            ClinicalDocumentKind.Consultation,
            ClinicalDocumentKind.Assessment,
            ClinicalDocumentKind.Procedure
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
        public async Task<MrcClinicalDocumentIntegrity> RegisterAsync(
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

            // BE-RWI-091 kriteria 2. Idempotensi lapis kedua: baris yang sudah DITAMBAHKAN pada
            // unit kerja ini tetapi BELUM tersimpan.
            //
            // Kenapa dibutuhkan. FindAsync di atas menjalankan pembacaan basis data, dan
            // pembacaan basis data tidak melihat baris yang baru di-Add dan belum
            // SaveChanges. Sejak registrasi berpindah ke "saat konsep pertama kali disimpan",
            // satu permintaan dapat memanggil pendaftaran lebih dari sekali — misalnya
            // pembuatan yang langsung diselesaikan lewat CompleteImmediately, yang memanggil
            // pendaftaran draf lalu pendaftaran tertanda tangan pada SaveChanges yang sama.
            // Tanpa pemeriksaan ini, keduanya menambah baris dan penyimpanan gagal pada index
            // unique — permintaan sah berakhir sebagai galat sistem.
            //
            // Index unique (DocumentKind, DocumentId) tetap menjadi lapis terakhir bagi dua
            // permintaan yang tiba bersamaan; pemeriksaan di dalam aplikasi tidak dapat
            // menggantikannya.
            var barisLokal = _dbContext.ChangeTracker
                .Entries<MrcClinicalDocumentIntegrity>()
                .Select(x => x.Entity)
                .FirstOrDefault(x =>
                    x.DocumentKind == documentKind &&
                    x.DocumentId == documentId &&
                    !x.IsDelete);

            if (barisLokal != null)
                return barisLokal;

            var keutuhan = new MrcClinicalDocumentIntegrity
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

            await _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AddAsync(keutuhan, cancellationToken);

            return keutuhan;
        }

        /// <summary>
        /// Mendaftarkan satu dokumen klinis ke daftar keutuhan sekaligus menandainya
        /// tertanda tangan oleh penulisnya — `BE-RWI-038`, `RWI-AC-157`.
        ///
        /// Dipakai pada saat dokumen difinalkan. Pendaftaran dan penandatanganan dilakukan
        /// sekaligus karena finalisasi memang sudah menyatakan "dokumen ini selesai dan
        /// menjadi tanggung jawab penulisnya"; memisahkannya menjadi dua langkah membuka
        /// jendela waktu di mana dokumen sudah final tetapi belum terkunci.
        ///
        /// TIDAK menyimpan. Pemanggil WAJIB menjalankannya di dalam transaksi atau
        /// `SaveChanges` yang sama dengan finalisasi dokumennya. Bila pendaftaran gagal,
        /// finalisasi harus ikut batal — dokumen final tanpa baris keutuhan adalah dokumen
        /// yang tidak dapat dikoreksi selamanya, dan itu persis keadaan yang sedang ditutup.
        ///
        /// Aman dipanggil berulang: dokumen yang sudah terkunci dikembalikan apa adanya tanpa
        /// tanda tangan kedua, dan dokumen yang masih draf dinaikkan menjadi tertanda tangan.
        /// </summary>
        /// <param name="documentKind">Jenis dokumen yang difinalkan.</param>
        /// <param name="documentId">Id dokumen pada tabel asalnya.</param>
        /// <param name="patientId">Pasien pemilik dokumen.</param>
        /// <param name="encounterId">Kunjungan yang menaungi dokumen.</param>
        /// <param name="authorUserId">
        /// Penulis dokumen. Ia sekaligus menjadi penanda tangan — `RWI-AC-157`. Bila
        /// penulisnya tidak dapat ditentukan, pendaftaran ditolak, karena dokumen bertanda
        /// tangan tanpa penanda tangan bukan bukti apa pun.
        /// </param>
        /// <param name="deviceInfo">Perangkat pemanggil, diambil dari permintaan HTTP.</param>
        /// <param name="ipAddress">Alamat jaringan pemanggil, diambil dari permintaan HTTP.</param>
        /// <param name="nowUtc">Saat finalisasi; dipakai sebagai waktu tanda tangan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<MrcClinicalDocumentIntegrity> RegisterSignedAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid patientId,
            Guid encounterId,
            Guid authorUserId,
            string? deviceInfo,
            string? ipAddress,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            if (authorUserId == Guid.Empty)
                throw new InvalidOperationException("Penulis dokumen klinis tidak dapat ditentukan.");

            var keutuhan = await RegisterAsync(
                documentKind, documentId, patientId, encounterId, authorUserId,
                isAuthorKnown: true, cancellationToken);

            // Dokumen yang sudah terkunci tidak ditandatangani ulang. Tanda tangan kedua akan
            // menimpa waktu dan perangkat tanda tangan pertama, dan itu menghapus bukti.
            if (keutuhan.IntegrityStatus != ClinicalDocumentIntegrityStatus.Draft)
                return keutuhan;

            keutuhan.IntegrityStatus = ClinicalDocumentIntegrityStatus.Signed;
            keutuhan.SignedAt = nowUtc;
            keutuhan.SignedByUserId = authorUserId;
            keutuhan.SignatureDeviceInfo = Potong(deviceInfo, 250);
            keutuhan.SignatureIpAddress = Potong(ipAddress, 64);
            keutuhan.LockedAt = nowUtc;
            keutuhan.LockTrigger = ClinicalDocumentLockTrigger.AuthorSigned;
            keutuhan.UpdateDateTime = nowUtc;
            keutuhan.UpdateBy = authorUserId;

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
        /// Memeriksa apakah sebuah <b>konsep</b> masih boleh disunting <b>atau diselesaikan</b> —
        /// <c>BE-RWI-091</c>, <c>VAL-DOK-43</c>, <c>RWI-DEC-138</c> butir (2).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Bedanya dengan <see cref="EnsureMutableAsync"/>.</b> Sejak konsep catatan dokter dan
        /// kajian medis rawat inap terdaftar <c>Draft</c> sejak simpan pertama, konsep yang
        /// ditinggal penulisnya akan dikunci <c>LockedUnsigned</c> ketika perawatannya ditutup.
        /// Menyelesaikan konsep seperti itu belakangan sama dengan menandatangani mundur, dan itu
        /// menghapus fakta bahwa catatan tersebut <b>tidak</b> ditandatangani saat perawatan
        /// berakhir. Karena itu jalur Selesai wajib menolaknya — bukan hanya jalur sunting.
        /// </para>
        /// <para>
        /// <b>Kode dan kalimatnya sengaja berbeda.</b> Keadaan ini bukan kesalahan isian, melainkan
        /// benturan dengan keadaan dokumen, sehingga dijawab <c>409</c> beserta arahan memakai
        /// addendum dari Catatan Saya. Keadaan lain diteruskan apa adanya ke
        /// <see cref="EnsureMutableAsync"/>, sehingga pesan yang sudah dikenal pengguna tidak
        /// bergeser.
        /// </para>
        /// <para>
        /// <b>Contoh.</b> dr. Yoga menyimpan konsep SOAP Joko pukul 06.30. Episode Joko ditutup
        /// pukul 13.00 dan konsep itu terkunci. Pukul 15.00 dr. Yoga menekan Selesai → <c>409</c>
        /// "Catatan ini terkunci karena perawatan pasien sudah ditutup. Lengkapi lewat addendum
        /// dari Catatan Saya."
        /// </para>
        /// </remarks>
        public async Task<IntegrityGuardResult> EnsureDraftStillOpenAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            if (!DitegakkanUntuk(documentKind))
                return IntegrityGuardResult.Allowed();

            var keutuhan = await FindAsync(documentKind, documentId, cancellationToken);

            if (keutuhan?.IntegrityStatus == ClinicalDocumentIntegrityStatus.LockedUnsigned)
            {
                return IntegrityGuardResult.Denied(
                    StatusCodes.Status409Conflict,
                    "Catatan ini terkunci karena perawatan pasien sudah ditutup. " +
                    "Lengkapi lewat addendum dari Catatan Saya.");
            }

            return await EnsureMutableAsync(documentKind, documentId, cancellationToken);
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
        public async Task<(IntegrityGuardResult Result, MrcClinicalDocumentIntegrity? Integrity)> SignAsync(
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
        /// Menandai baris keutuhan sebuah dokumen sebagai <b>dibatalkan</b> — `BE-RWI-091`,
        /// `RWI-AC-217`.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Barisnya tidak dihapus, dan itu inti aturannya.</b> Konsep yang dibatalkan tetap
        /// pernah ada, dan riwayat dokumen yang pernah ada tidak boleh menghilang tanpa jejak.
        /// Yang berubah hanya statusnya menjadi <see cref="ClinicalDocumentIntegrityStatus.Cancelled"/>
        /// beserta alasannya, sehingga laporan kelengkapan rekam medis tetap dapat menyebutkan
        /// berapa konsep yang dibatalkan dan karena apa.
        /// </para>
        /// <para>
        /// <b>Dokumen yang sudah terkunci tidak dapat dibatalkan lewat jalur ini.</b> Catatan
        /// yang sudah ditandatangani dibetulkan lewat addendum, bukan disembunyikan lewat
        /// pembatalan — pembatalan atas catatan tertanda tangan akan menghapus tanda tangan
        /// yang sudah diberikan.
        /// </para>
        /// <para>
        /// <b>Aman dipanggil berulang.</b> Baris yang sudah dibatalkan dikembalikan apa adanya,
        /// tanpa menimpa alasan dan waktu pembatalan pertama.
        /// </para>
        /// <para>
        /// TIDAK menyimpan. Pemanggil wajib menjalankannya di dalam `SaveChanges` yang sama
        /// dengan pembatalan dokumennya, supaya tidak pernah ada dokumen yang dibatalkan tetapi
        /// baris keutuhannya masih menyatakan draf — dokumen seperti itu akan muncul pada
        /// daftar "konsep saya" selamanya.
        /// </para>
        /// </remarks>
        /// <param name="documentKind">Jenis dokumen yang dibatalkan.</param>
        /// <param name="documentId">Id dokumen pada tabel asalnya.</param>
        /// <param name="actorUserId">Pengguna yang membatalkan.</param>
        /// <param name="reason">Alasan pembatalan. SENSITIF — dapat memuat keterangan klinis.</param>
        /// <param name="nowUtc">Saat pembatalan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        /// <returns>
        /// Baris keutuhan yang ditandai, atau <c>null</c> bila dokumennya belum terdaftar —
        /// keadaan yang wajar pada dokumen lama, dan sengaja tidak dianggap galat.
        /// </returns>
        public async Task<MrcClinicalDocumentIntegrity?> MarkCancelledAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid actorUserId,
            string? reason,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            if (!DitegakkanUntuk(documentKind))
                return null;

            var keutuhan = await FindAsync(documentKind, documentId, cancellationToken);

            if (keutuhan == null)
                return null;

            if (keutuhan.IntegrityStatus == ClinicalDocumentIntegrityStatus.Cancelled)
                return keutuhan;

            // Dokumen yang sudah terkunci — tertanda tangan maupun terkunci tanpa tanda tangan —
            // tidak dibatalkan di sini. Penjaga pembatalan pada controller sudah menolaknya
            // lebih dulu lewat EnsureMutableAsync; baris ini menjaga agar jalur baru yang lupa
            // memanggil penjaga itu tidak diam-diam menghapus tanda tangan.
            if (keutuhan.IntegrityStatus != ClinicalDocumentIntegrityStatus.Draft)
                return keutuhan;

            keutuhan.IntegrityStatus = ClinicalDocumentIntegrityStatus.Cancelled;
            keutuhan.CancelledReason = Potong(reason, 250);
            keutuhan.LockedAt = nowUtc;
            keutuhan.LockTrigger = ClinicalDocumentLockTrigger.DocumentCancelled;
            keutuhan.IsActive = false;
            keutuhan.UpdateDateTime = nowUtc;
            keutuhan.UpdateBy = actorUserId;

            return keutuhan;
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
                var potongan = await _dbContext.Set<MrcClinicalDocumentIntegrity>()
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
        public Task<MrcClinicalDocumentIntegrity?> FindAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken = default)
            => _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .FirstOrDefaultAsync(
                    x => x.DocumentKind == documentKind
                         && x.DocumentId == documentId
                         && !x.IsDelete,
                    cancellationToken);

        /// <summary>
        /// Membaca konsep milik penulis yang sedang masuk. Penyaring layanan dan metadata
        /// rawat inap ditambahkan oleh <c>BE-RWI-092</c> tanpa mengubah perilaku bawaan layar
        /// rekam medis yang tetap membaca seluruh layanan.
        /// </summary>
        public async Task<PagedResult<UnsignedDocumentResponse>> GetAuthoredDraftsAsync(
            Guid authorUserId,
            ClinicalDocumentServiceContext serviceContext,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.AuthorUserId == authorUserId
                            && x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Draft
                            && !x.IsDelete);

            query = ApplyServiceContext(query, serviceContext);

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UnsignedDocumentResponse
                {
                    IntegrityId = x.Id,
                    DocumentKind = x.DocumentKind,
                    DocumentKindName = x.DocumentKind.ToString(),
                    DocumentId = x.DocumentId,
                    PatientId = x.PatientId,
                    EncounterId = x.EncounterId,
                    CreatedAt = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            await CompleteDraftMetadataAsync(items, cancellationToken);

            return new PagedResult<UnsignedDocumentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Membaca catatan terkunci milik penulis yang sedang masuk untuk halaman
        /// "Catatan Saya" â€” <c>BE-RWI-092</c>, <c>RWI-DEC-127</c>, dan
        /// <c>RWI-DEC-151</c>.
        /// </summary>
        /// <remarks>
        /// Query selalu dibatasi oleh <paramref name="authorUserId"/> sebelum identitas pasien
        /// dilengkapi. Response hanya membawa nama, nomor rekam medis, nomor kunjungan, nomor
        /// perawatan, dan metadata dokumen; tidak ada diagnosis, resep, hasil pemeriksaan, atau
        /// isi catatan pasien.
        /// </remarks>
        public async Task<PagedResult<AuthoredDocumentItem>> GetAuthoredDocumentsAsync(
            Guid authorUserId,
            ClinicalDocumentIntegrityStatus? status,
            ClinicalDocumentServiceContext serviceContext,
            DateTime? from,
            DateTime? to,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.AuthorUserId == authorUserId && !x.IsDelete);

            if (status.HasValue)
            {
                query = query.Where(x => x.IntegrityStatus == status.Value);
            }
            else
            {
                // Konsep tetap dibaca dari my-unsigned. Endpoint ini menjadi jalan masuk
                // addendum, sehingga bawaan kontrak 0.6.0 hanya catatan yang sudah terkunci.
                query = query.Where(x =>
                    x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Signed ||
                    x.IntegrityStatus == ClinicalDocumentIntegrityStatus.LockedUnsigned);
            }

            query = ApplyServiceContext(query, serviceContext);

            if (from.HasValue)
                query = query.Where(x => x.CreateDateTime >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.CreateDateTime <= to.Value);

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.DocumentKind,
                    x.DocumentId,
                    x.IntegrityStatus,
                    x.PatientId,
                    x.EncounterId,
                    x.SignedAt,
                    x.LockedAt,
                    x.LockTrigger,
                    x.AddendumCount,
                    x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            var items = rows.Select(x => new AuthoredDocumentItem
            {
                IntegrityId = x.Id,
                DocumentKind = x.DocumentKind,
                DocumentKindName = x.DocumentKind.ToString(),
                DocumentId = x.DocumentId,
                IntegrityStatus = x.IntegrityStatus,
                IntegrityStatusName = IntegrityStatusName(x.IntegrityStatus),
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                SignedAt = x.SignedAt,
                LockedAt = x.LockedAt,
                LockTrigger = x.LockTrigger,
                LockTriggerName = x.LockTrigger.HasValue
                    ? LockTriggerName(x.LockTrigger.Value)
                    : null,
                AddendumCount = x.AddendumCount,
                CanAddAddendum =
                    x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Signed ||
                    x.IntegrityStatus == ClinicalDocumentIntegrityStatus.LockedUnsigned,
                CreatedAt = x.CreateDateTime
            }).ToList();

            await CompleteAuthoredMetadataAsync(items, cancellationToken);

            return new PagedResult<AuthoredDocumentItem>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        private IQueryable<MrcClinicalDocumentIntegrity> ApplyServiceContext(
            IQueryable<MrcClinicalDocumentIntegrity> query,
            ClinicalDocumentServiceContext serviceContext)
        {
            if (serviceContext == ClinicalDocumentServiceContext.All)
                return query;

            var inpatientEncounters = _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.EncounterId);

            return serviceContext == ClinicalDocumentServiceContext.Inpatient
                ? query.Where(x => inpatientEncounters.Contains(x.EncounterId))
                : query.Where(x => !inpatientEncounters.Contains(x.EncounterId));
        }

        private async Task CompleteDraftMetadataAsync(
            List<UnsignedDocumentResponse> items,
            CancellationToken cancellationToken)
        {
            if (items.Count == 0)
                return;

            var patientIds = items.Select(x => x.PatientId).Distinct().ToList();
            var encounterIds = items.Select(x => x.EncounterId).Distinct().ToList();

            var patients = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => patientIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.MedicalRecordNumber })
                .ToListAsync(cancellationToken);

            var encounters = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => encounterIds.Contains(x.Id))
                .Select(x => new { x.Id, x.EncounterNumber })
                .ToListAsync(cancellationToken);

            var episodes = await ReadEpisodesByEncounterAsync(encounterIds, cancellationToken);
            var clinicalTimes = await ReadClinicalTimesAsync(
                items.Select(x => (x.DocumentKind, x.DocumentId)).ToList(),
                cancellationToken);

            foreach (var item in items)
            {
                var patient = patients.FirstOrDefault(x => x.Id == item.PatientId);
                item.PatientName = patient?.FullName;
                item.MedicalRecordNumber = patient?.MedicalRecordNumber;
                item.EncounterNumber = encounters
                    .FirstOrDefault(x => x.Id == item.EncounterId)?.EncounterNumber;

                if (episodes.TryGetValue(item.EncounterId, out var episode))
                    item.EpisodeNumber = episode.EpisodeNumber;

                if (clinicalTimes.TryGetValue((item.DocumentKind, item.DocumentId), out var time))
                    item.ClinicalDateTime = time;
            }
        }

        private async Task CompleteAuthoredMetadataAsync(
            List<AuthoredDocumentItem> items,
            CancellationToken cancellationToken)
        {
            if (items.Count == 0)
                return;

            var patientIds = items.Select(x => x.PatientId).Distinct().ToList();
            var encounterIds = items.Select(x => x.EncounterId).Distinct().ToList();

            var patients = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => patientIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.MedicalRecordNumber })
                .ToListAsync(cancellationToken);

            var encounters = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => encounterIds.Contains(x.Id))
                .Select(x => new { x.Id, x.EncounterNumber })
                .ToListAsync(cancellationToken);

            var episodes = await ReadEpisodesByEncounterAsync(encounterIds, cancellationToken);
            var clinicalTimes = await ReadClinicalTimesAsync(
                items.Select(x => (x.DocumentKind, x.DocumentId)).ToList(),
                cancellationToken);

            foreach (var item in items)
            {
                var patient = patients.FirstOrDefault(x => x.Id == item.PatientId);
                item.PatientName = patient?.FullName;
                item.MedicalRecordNumber = patient?.MedicalRecordNumber;
                item.EncounterNumber = encounters
                    .FirstOrDefault(x => x.Id == item.EncounterId)?.EncounterNumber;

                if (episodes.TryGetValue(item.EncounterId, out var episode))
                {
                    item.InpEpisodeId = episode.Id;
                    item.EpisodeNumber = episode.EpisodeNumber;
                }

                if (clinicalTimes.TryGetValue((item.DocumentKind, item.DocumentId), out var time))
                    item.ClinicalDateTime = time;
            }
        }

        private async Task<Dictionary<Guid, EpisodeIdentity>> ReadEpisodesByEncounterAsync(
            List<Guid> encounterIds,
            CancellationToken cancellationToken)
        {
            if (encounterIds.Count == 0)
                return new Dictionary<Guid, EpisodeIdentity>();

            var rows = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => encounterIds.Contains(x.EncounterId) && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.EncounterId, x.EpisodeNumber })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.EncounterId)
                .ToDictionary(
                    x => x.Key,
                    x =>
                    {
                        var row = x.First();
                        return new EpisodeIdentity(row.Id, row.EncounterId, row.EpisodeNumber);
                    });
        }

        private async Task<Dictionary<(ClinicalDocumentKind, Guid), DateTime?>> ReadClinicalTimesAsync(
            List<(ClinicalDocumentKind Kind, Guid Id)> references,
            CancellationToken cancellationToken)
        {
            var result = new Dictionary<(ClinicalDocumentKind, Guid), DateTime?>();

            var consultationIds = references
                .Where(x => x.Kind == ClinicalDocumentKind.Consultation)
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            if (consultationIds.Count > 0)
            {
                var rows = await _dbContext.Set<TrxDoctorConsultation>()
                    .AsNoTracking()
                    .Where(x => consultationIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.ClinicalDateTime, x.ConsultationDateTime })
                    .ToListAsync(cancellationToken);

                foreach (var row in rows)
                    result[(ClinicalDocumentKind.Consultation, row.Id)] =
                        row.ClinicalDateTime ?? row.ConsultationDateTime;
            }

            var assessmentIds = references
                .Where(x => x.Kind == ClinicalDocumentKind.Assessment)
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            if (assessmentIds.Count > 0)
            {
                var rows = await _dbContext.Set<TrxPatientAssessment>()
                    .AsNoTracking()
                    .Where(x => assessmentIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.AssessmentDateTime })
                    .ToListAsync(cancellationToken);

                foreach (var row in rows)
                    result[(ClinicalDocumentKind.Assessment, row.Id)] = row.AssessmentDateTime;
            }

            var progressNoteIds = references
                .Where(x => x.Kind == ClinicalDocumentKind.ProgressNote)
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            if (progressNoteIds.Count > 0)
            {
                var rows = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                    .AsNoTracking()
                    .Where(x => progressNoteIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.NoteDateTime })
                    .ToListAsync(cancellationToken);

                foreach (var row in rows)
                    result[(ClinicalDocumentKind.ProgressNote, row.Id)] = row.NoteDateTime;
            }

            var procedureIds = references
                .Where(x => x.Kind == ClinicalDocumentKind.Procedure)
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            if (procedureIds.Count > 0)
            {
                var rows = await _dbContext.Set<TrxPatientProcedure>()
                    .AsNoTracking()
                    .Where(x => procedureIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.ProcedureDateTime })
                    .ToListAsync(cancellationToken);

                foreach (var row in rows)
                    result[(ClinicalDocumentKind.Procedure, row.Id)] = row.ProcedureDateTime;
            }

            return result;
        }

        private static string IntegrityStatusName(ClinicalDocumentIntegrityStatus status) => status switch
        {
            ClinicalDocumentIntegrityStatus.Draft => "Draf",
            ClinicalDocumentIntegrityStatus.Signed => "Ditandatangani",
            ClinicalDocumentIntegrityStatus.LockedUnsigned => "Terkunci, Tidak Ditandatangani",
            ClinicalDocumentIntegrityStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        private static string LockTriggerName(ClinicalDocumentLockTrigger trigger) => trigger switch
        {
            ClinicalDocumentLockTrigger.AuthorSigned => "Ditandatangani Penulis",
            ClinicalDocumentLockTrigger.EncounterClosed => "Kunjungan Ditutup",
            ClinicalDocumentLockTrigger.BackfillEncounterClosed => "Pengisian Data Lama",
            ClinicalDocumentLockTrigger.DocumentCancelled => "Dokumen Dibatalkan",
            _ => trigger.ToString()
        };

        private sealed record EpisodeIdentity(Guid Id, Guid EncounterId, string EpisodeNumber);

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
