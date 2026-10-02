using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Hasil satu perintah verifikasi catatan terpadu.
    /// </summary>
    public sealed class CpptVerificationResult
    {
        public bool IsSuccess { get; init; }

        public int StatusCode { get; init; }

        public string? ErrorMessage { get; init; }

        public TrxPatientIntegratedProgressNote? Note { get; init; }

        internal static CpptVerificationResult Ok(TrxPatientIntegratedProgressNote note) => new()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
            Note = note
        };

        internal static CpptVerificationResult Fail(int statusCode, string message) => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = message
        };
    }

    /// <summary>
    /// Satu baris pada daftar pantau verifikasi.
    /// </summary>
    public sealed class CpptVerificationWatchItem
    {
        public Guid NoteId { get; init; }

        public string ProgressNoteNumber { get; init; } = string.Empty;

        public string ProfessionType { get; init; } = string.Empty;

        public Guid? ProviderUserId { get; init; }

        /// <summary>
        /// Nama penulis catatan - <c>BE-RWI-067</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Kosong berarti penulisnya tidak dapat dikenali sama sekali. Nomor pengguna
        /// <b>sengaja tidak</b> dipakai sebagai penggantinya: deretan angka dan huruf tidak
        /// menolong supervisor yang sedang mencari siapa yang perlu diingatkan, dan menampilkan
        /// nomor di kolom bernama "Penulis" hanya memindahkan pekerjaan menebak ke layar.
        /// </para>
        /// <para>
        /// Diambil <b>snapshot lebih dulu</b>, baru relasi pengguna. Daftar pantau adalah catatan
        /// historis: akun yang berganti nama tidak boleh mengubah nama penulis pada catatan lama.
        /// </para>
        /// </remarks>
        public string? ProviderName { get; init; }

        public DateTime NoteDateTime { get; init; }

        public CpptVerificationStatus VerificationStatus { get; init; }

        public DateTime? VerificationDueAt { get; init; }

        /// <summary>Benar bila catatan sudah melewati batas waktu verifikasinya.</summary>
        public bool IsOverdue { get; init; }
    }

    /// <summary>
    /// Satu baris pada daftar tunggu verifikasi milik seorang DPJP — <c>BE-RWI-096</c>,
    /// <c>FR-DOK-084</c>, <c>api-contract.md</c> `0.6.0` bagian 12.4.
    /// </summary>
    /// <remarks>
    /// Barisnya <b>satu per perawatan</b>, bukan satu per catatan. DPJP yang membuka daftar ini
    /// sedang bertanya "pasien siapa yang masih menunggu saya", bukan "catatan nomor berapa";
    /// daftar per catatan pada pasien yang sama hanya memanjangkan layar tanpa menambah
    /// keputusan yang dapat diambil.
    /// </remarks>
    public sealed class CpptVerificationWorklistItem
    {
        public Guid EpisodeId { get; init; }

        public string EpisodeNumber { get; init; } = string.Empty;

        public Guid PatientId { get; init; }

        public string? PatientName { get; init; }

        public string? MedicalRecordNumber { get; init; }

        public InpEpisodeStatus EpisodeStatus { get; init; }

        /// <summary>Label status perawatan yang siap ditampilkan.</summary>
        public string EpisodeStatusName { get; init; } = string.Empty;

        /// <summary>
        /// Benar bila perawatan sudah ditutup dan barisnya muncul lewat pengecualian
        /// <c>RWI-DEC-126</c>. Dipakai layar untuk menandainya, supaya DPJP tahu ia sedang
        /// menyelesaikan entri tertinggal — bukan mengerjakan pasien yang masih dirawat.
        /// </summary>
        public bool IsClosedEpisodeException { get; init; }

        /// <summary>Jumlah entri yang masih menunggu verifikasi pada perawatan ini.</summary>
        public int PendingCount { get; init; }

        /// <summary>Waktu klinis entri tertunda yang paling lama.</summary>
        public DateTime? OldestPendingNoteDateTime { get; init; }

        /// <summary>
        /// Benar bila ada setidaknya satu entri yang sudah melewati batas waktu verifikasinya.
        /// </summary>
        /// <remarks>
        /// Diturunkan dari <c>VerificationDueAt</c>, bukan disimpan. Selama kebijakan
        /// <c>RWI-RULE-021</c> belum disahkan, tidak satu pun entri punya batas waktu, dan
        /// nilai ini selalu <c>false</c> — bukan <c>true</c> berdasarkan angka bawaan yang
        /// dikarang.
        /// </remarks>
        public bool IsOverdue { get; init; }

        /// <summary>
        /// Lama keterlambatan entri paling terlambat, dalam menit. Kosong bila tidak ada entri
        /// yang melewati batas, atau ketika kebijakan batas waktu masih kosong.
        /// </summary>
        public int? LateByMinutes { get; init; }
    }

    /// <summary>
    /// Keadaan verifikasi seluruh catatan terpadu pada satu perawatan.
    /// </summary>
    public sealed class CpptVerificationStatusSummary
    {
        public Guid EpisodeId { get; init; }

        public int TotalNoteCount { get; init; }

        public int NotRequiredCount { get; init; }

        public int PendingCount { get; init; }

        public int VerifiedCount { get; init; }

        public int OverdueCount { get; init; }

        /// <summary>
        /// Benar ketika tidak satu pun catatan pada perawatan ini diwajibkan diverifikasi.
        /// </summary>
        /// <remarks>
        /// Inilah keadaan hari ini pada seluruh rumah sakit: nilai batas waktu verifikasi
        /// <c>RWI-RULE-021</c> belum disahkan, sehingga tidak satu pun catatan diberi batas dan
        /// daftar pantau selalu kosong. Penandanya dikembalikan apa adanya supaya layar dapat
        /// menyatakan "kebijakan verifikasi belum aktif", bukan menampilkan daftar kosong yang
        /// tampak seperti semuanya sudah beres.
        /// </remarks>
        public bool IsVerificationPolicyEmpty { get; init; }

        /// <summary>Catatan yang menunggu verifikasi, termasuk yang sudah lewat batas.</summary>
        public List<CpptVerificationWatchItem> WatchList { get; init; } = new();
    }

    /// <summary>
    /// Verifikasi DPJP atas catatan profesi lain pada lembar terpadu — <c>CAP-021</c>,
    /// <c>BE-RWI-053</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Verifikasi memantau, ia tidak menahan.</b> Catatan yang belum diverifikasi tetap sah,
    /// tetap terbaca, dan tidak menahan penulisan catatan berikutnya — <c>RWI-RULE-021</c>.
    /// Menjadikannya gerbang pelayanan akan menghentikan pelayanan setiap kali DPJP sedang di
    /// kamar operasi, dan itu bahaya yang jauh lebih besar daripada catatan yang belum terbaca.
    /// </para>
    /// <para>
    /// <b>Tidak satu angka batas waktu pun ditanam di sini.</b> Nilai batasnya
    /// <c>RWI-RULE-021</c> belum disahkan karena pemilik klinisnya belum ditunjuk. Mekanismenya
    /// dibangun penuh dan berjalan dengan <b>kebijakan kosong</b>: selama tidak ada catatan yang
    /// diberi batas waktu, seluruh catatan berstatus tidak-diwajibkan dan daftar pantau kosong.
    /// Menanam angka bawaan berarti mengarang kebijakan klinis.
    /// </para>
    /// <para>
    /// <b>Verifikator bukan penulis, dan itu inti aturannya.</b> Verifikasi tidak pernah menulis
    /// ulang penulis catatan — <c>INV-DOK-11</c>. Yang tersimpan adalah dua nama pada dua kolom
    /// berbeda: penulis tetap perawat atau profesi lain yang menulisnya, verifikator adalah DPJP
    /// yang menyatakan sudah membacanya.
    /// </para>
    /// <para>
    /// Tidak memakai interface, mengikuti pola service pada repository ini.
    /// </para>
    /// </remarks>
    public class CpptVerificationService
    {
        /// <summary>
        /// Kalimat penolakan <c>VAL-DOK-07</c>, apa adanya seperti pada validation matrix.
        /// </summary>
        public const string PenolakanBukanDpjp = "Verifikasi hanya dapat dilakukan DPJP pasien ini.";

        /// <summary>
        /// Kalimat penolakan <c>INV-DOK-16</c> pada bentuk yang menjelaskan sebabnya —
        /// <c>BE-RWI-089</c>, <c>FR-DOK-082</c>, <c>api-contract.md</c> `0.6.0` bagian 12.4.
        /// </summary>
        /// <remarks>
        /// Dipakai untuk konsulen, dokter jaga, <b>dan</b> dokter yang dulu DPJP tetapi sudah
        /// digantikan. Ketiganya menerima kalimat yang sama dan sengaja: membedakannya akan
        /// membocorkan siapa DPJP pasien itu sekarang kepada dokter yang tidak lagi berwenang.
        /// </remarks>
        public const string PenolakanVerifikasiBukanDpjpAktif =
            "Hanya DPJP yang sedang bertugas atas pasien ini yang dapat memverifikasi catatan " +
            "profesi lain.";

        /// <summary>
        /// Kalimat penolakan <c>BE-RWI-095</c> kriteria 5: entri yang ditulis <b>sesudah</b>
        /// perawatan ditutup tidak termasuk pengecualian DPJP terakhir.
        /// </summary>
        public const string PenolakanEntriSesudahPenutupan =
            "Catatan ini tercatat setelah perawatan pasien ditutup, sehingga tidak termasuk " +
            "entri tertinggal yang masih dapat diverifikasi.";

        /// <summary>
        /// Episode berstatus Closed tanpa waktu penutupan tidak menyediakan batas yang cukup
        /// untuk membuktikan sebuah entri benar-benar ditulis sebelum penutupan.
        /// </summary>
        public const string PenolakanWaktuPenutupanTidakTercatat =
            "Waktu penutupan perawatan tidak tercatat, sehingga sistem tidak dapat memastikan " +
            "catatan ini dibuat sebelum perawatan ditutup.";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _contextService;

        public CpptVerificationService(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService contextService)
        {
            _dbContext = dbContext;
            _contextService = contextService;
        }

        /// <summary>
        /// DPJP menyatakan sudah membaca satu catatan profesi lain.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Kewenangannya dinilai pada <b>saat verifikasi</b>, bukan pada saat catatan ditulis —
        /// <c>RWI-RULE-030</c>. DPJP yang menerima alih rawat hari ini bertanggung jawab atas
        /// catatan pasiennya, termasuk catatan yang ditulis sebelum ia mengambil alih; DPJP lama
        /// justru sudah tidak lagi berwenang.
        /// </para>
        /// <para>
        /// Verifikator tidak boleh sama dengan penulis catatan. Menandatangani bacaan atas
        /// tulisan sendiri bukan verifikasi.
        /// </para>
        /// </remarks>
        /// <param name="noteId">Catatan terpadu yang diverifikasi.</param>
        /// <param name="actorUserId">Pengguna yang memverifikasi.</param>
        /// <param name="actorDoctorId">
        /// Baris dokter yang melekat pada pengguna itu. Kosong berarti pengguna tidak terhubung
        /// ke dokter mana pun, dan permintaannya ditolak <c>403</c>.
        /// </param>
        /// <param name="nowUtc">Saat verifikasi.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<CpptVerificationResult> VerifyAsync(
            Guid noteId,
            Guid actorUserId,
            Guid? actorDoctorId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            var note = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .FirstOrDefaultAsync(x => x.Id == noteId && !x.IsDelete, cancellationToken);

            if (note == null)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Catatan terpadu tidak ditemukan.");
            }

            if (note.CancelledAt.HasValue)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Catatan yang sudah dibatalkan tidak dapat diverifikasi.");
            }

            var episodeId = await ResolveEpisodeIdAsync(note, cancellationToken);

            if (episodeId == null)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Catatan ini tidak berada di bawah perawatan rawat inap, sehingga " +
                    "verifikasi DPJP tidak berlaku untuknya.");
            }

            if (actorDoctorId == null || actorDoctorId.Value == Guid.Empty)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status403Forbidden, PenolakanVerifikasiBukanDpjpAktif);
            }

            // BE-RWI-089, BE-RWI-095. Kewenangan verifikasi dinilai lewat satu pembantu, supaya
            // perawatan berjalan dan perawatan yang sudah ditutup tidak dapat berbeda aturan
            // tanpa disengaja.
            var kewenangan = await NilaiKewenanganVerifikasiAsync(
                episodeId.Value, actorDoctorId.Value, note, nowUtc, cancellationToken);

            if (kewenangan != null)
                return kewenangan;

            if (note.ProviderUserId.HasValue && note.ProviderUserId.Value == actorUserId)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status403Forbidden,
                    "Catatan Anda sendiri tidak dapat Anda verifikasi.");
            }

            if (note.VerificationStatus == CpptVerificationStatus.Verified)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status409Conflict,
                    "Catatan ini sudah diverifikasi sebelumnya.");
            }

            // Penulis catatan SENGAJA tidak disentuh. Yang berubah hanya kolom verifikasi.
            //
            // BE-RWI-095 kriteria 3. Batas waktu verifikasi pada VerificationDueAt SENGAJA
            // tidak dikosongkan dan tidak digeser. Verifikasi yang terlambat tetap terbaca
            // terlambat sesudahnya, karena keterlambatannya diturunkan dari selisih
            // VerifiedAt terhadap VerificationDueAt — bukan dari status yang ditimpa. Menghapus
            // batasnya akan membuat verifikasi yang datang 19 jam terlambat terlihat tepat
            // waktu, dan itu menghapus satu-satunya angka yang dipakai menilai kepatuhan.
            note.VerificationStatus = CpptVerificationStatus.Verified;
            note.VerifiedAt = nowUtc;
            note.VerifiedByUserId = actorUserId;
            note.UpdateDateTime = nowUtc;
            note.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return CpptVerificationResult.Ok(note);
        }

        /// <summary>
        /// Menilai kewenangan verifikasi satu catatan, pada perawatan yang masih berjalan
        /// maupun yang sudah ditutup — <c>BE-RWI-089</c>, <c>BE-RWI-095</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Dua aturan, satu tempat.</b> Selama perawatan berjalan, yang berwenang adalah
        /// dokter dengan penugasan berperan <b>DPJP</b> yang aktif pada detik verifikasi
        /// (<c>INV-DOK-16</c>). Sesudah perawatan ditutup, yang berwenang adalah <b>DPJP
        /// terakhir</b> perawatan itu, dan hanya atas entri yang ditulis sebelum penutupan
        /// (<c>RWI-DEC-126</c>).
        /// </para>
        /// <para>
        /// <b>Kenapa pengecualian perawatan tertutup dibutuhkan.</b> Perawatan ditutup, tetapi
        /// ada entri CPPT yang belum sempat diverifikasi. Kalau verifikasi ikut tertutup, entri
        /// itu menggantung selamanya dan tidak ada orang yang dapat menyelesaikannya. Pengecualian
        /// ini hanya untuk <b>verifikasi</b>; ia tidak membuka penulisan catatan baru pada
        /// perawatan tertutup — jalur penulisan tetap dijaga
        /// <c>InpatientClinicalContextService</c> dengan <c>forNewDocument: true</c>.
        /// </para>
        /// <para>
        /// <b>Contoh berangka.</b> Entri perawat Sabtu 21.00 belum diverifikasi ketika perawatan
        /// ditutup Minggu 10.00. Senin 16.00 DPJP terakhir memverifikasinya: <b>diterima</b>,
        /// karena entri ditulis sebelum penutupan. Entri lain yang tercatat Minggu 12.00 — dua
        /// jam <b>setelah</b> penutupan — ditolak <c>422</c>. Dokter yang menjadi DPJP pada
        /// minggu pertama lalu digantikan tetap ditolak <c>403</c>.
        /// </para>
        /// </remarks>
        /// <returns>
        /// <c>null</c> bila verifikasi boleh dilanjutkan, atau hasil penolakan yang siap
        /// dikembalikan.
        /// </returns>
        private async Task<CpptVerificationResult?> NilaiKewenanganVerifikasiAsync(
            Guid episodeId,
            Guid actorDoctorId,
            TrxPatientIntegratedProgressNote note,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.EpisodeStatus, x.ClosedAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Perawatan rawat inap catatan ini tidak ditemukan.");
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Cancelled)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Perawatan yang dibatalkan tidak termasuk pengecualian verifikasi episode tertutup.");
            }

            var sudahDitutup = episode.EpisodeStatus == InpEpisodeStatus.Closed;

            if (!sudahDitutup)
            {
                // BE-RWI-089 / INV-DOK-16, FR-DOK-082. Saringan perannya inilah perubahan
                // task itu: sebelumnya penjaga memakai IsDoctorAssignedAsync, yang terbuka
                // bagi konsulen dan dokter jaga juga.
                var dpjpAktif = await _contextService.IsDpjpAssignedAsync(
                    episodeId, actorDoctorId, nowUtc, cancellationToken);

                return dpjpAktif
                    ? null
                    : CpptVerificationResult.Fail(
                        StatusCodes.Status403Forbidden, PenolakanVerifikasiBukanDpjpAktif);
            }

            // BE-RWI-095 kriteria 1 dan 2. Hanya DPJP TERAKHIR, bukan DPJP mana pun yang pernah
            // memegang perawatan ini.
            var dpjpTerakhir = await _contextService.FindLastAttendingDoctorIdAsync(
                episodeId, cancellationToken);

            if (!dpjpTerakhir.HasValue || dpjpTerakhir.Value != actorDoctorId)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status403Forbidden, PenolakanVerifikasiBukanDpjpAktif);
            }

            // BE-RWI-095 kriteria 5. Entri yang tercatat sesudah penutupan bukan "entri
            // tertinggal", dan pengecualian ini tidak dibuat untuknya. Waktu klinis yang
            // dipakai adalah NoteDateTime, bukan waktu penyimpanan — itulah saat kejadian
            // klinisnya menurut penulisnya sendiri.
            //
            // Fail closed bila waktu penutupan tidak ada. Tanpa batas itu sistem tidak dapat
            // membuktikan kriteria "ditulis sebelum penutupan" dan tidak boleh menebaknya.
            if (!episode.ClosedAt.HasValue)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    PenolakanWaktuPenutupanTidakTercatat);
            }

            if (note.NoteDateTime > episode.ClosedAt.Value)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status422UnprocessableEntity, PenolakanEntriSesudahPenutupan);
            }

            return null;
        }

        /// <summary>
        /// Daftar tunggu verifikasi milik seorang dokter — <c>BE-RWI-096</c>,
        /// <c>FR-DOK-084</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Kenapa daftar ini ada.</b> Pengecualian <c>BE-RWI-095</c> memberi DPJP terakhir
        /// hak memverifikasi entri tertinggal pada perawatan yang sudah ditutup. Hak itu tidak
        /// ada gunanya kalau DPJP tidak tahu entri mana yang tertinggal: perawatan yang ditutup
        /// hilang dari daftar pasiennya, dan entri yang menggantung ikut hilang bersamanya.
        /// Daftar ini karena itu <b>ikut memuat</b> perawatan <c>Closed</c> miliknya, dan baru
        /// melepasnya setelah seluruh entri di dalamnya terverifikasi.
        /// </para>
        /// <para>
        /// <b>Dua sumber kewenangan, satu daftar.</b> Perawatan berjalan masuk lewat penugasan
        /// DPJP yang aktif sekarang. Perawatan tertutup masuk lewat penugasan DPJP
        /// <b>terakhir</b> — dan hanya terakhir. Dokter yang dulu DPJP lalu digantikan tidak
        /// melihat satu pun perawatan tertutup milik penggantinya, sesuai
        /// <c>BE-RWI-095</c> kriteria 2.
        /// </para>
        /// <para>
        /// <b>Contoh berangka.</b> dr. Ahmad DPJP tiga pasien. Dua masih dirawat, dan
        /// masing-masing punya satu catatan perawat yang menunggu. Satu sudah pulang Minggu
        /// dengan satu entri Sabtu 21.00 yang belum sempat diverifikasi. Daftarnya memuat
        /// <b>tiga</b> baris; baris ketiga bertanda perawatan tertutup. Setelah dr. Ahmad
        /// memverifikasi entri Sabtu itu, baris ketiga hilang dan daftarnya menjadi dua.
        /// </para>
        /// <para>
        /// <b>Satu pembacaan, bukan satu per perawatan.</b> Entri tertunda dibaca sekali lalu
        /// dikelompokkan di memori. Membaca per perawatan akan menghasilkan sebanyak-perawatan
        /// pembacaan basis data pada daftar yang dibuka setiap kali dokter membuka ruang
        /// kerjanya.
        /// </para>
        /// </remarks>
        /// <param name="actorDoctorId">Dokter yang membuka daftar.</param>
        /// <param name="nowUtc">Saat yang dipakai menilai keaktifan penugasan dan keterlambatan.</param>
        /// <param name="includeClosedEpisodes">
        /// Benar berarti perawatan tertutup milik DPJP terakhir ikut dimuat — bawaan kontrak.
        /// Salah dipakai layar yang memang hanya ingin pasien yang masih dirawat.
        /// </param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<List<CpptVerificationWorklistItem>> GetVerificationWorklistAsync(
            Guid actorDoctorId,
            DateTime nowUtc,
            bool includeClosedEpisodes = true,
            CancellationToken cancellationToken = default)
        {
            if (actorDoctorId == Guid.Empty)
                return new List<CpptVerificationWorklistItem>();

            // Perawatan yang dokter ini DPJP-nya sekarang. Peran disaring di sini, bukan di
            // memori: penugasan konsulen dan dokter jaga tidak memberi kewenangan verifikasi.
            var perawatanBerjalan = await (
                    from assignment in _dbContext.Set<InpDoctorAssignment>().AsNoTracking()
                    join episodeBerjalan in _dbContext.Set<InpEpisode>().AsNoTracking()
                        on assignment.EpisodeId equals episodeBerjalan.Id
                    where assignment.DoctorId == actorDoctorId &&
                          assignment.AssignmentRole == InpDoctorAssignmentRole.Dpjp &&
                          !assignment.IsDelete &&
                          !assignment.IsCancel &&
                          assignment.IsActive &&
                          assignment.StartDateTime <= nowUtc &&
                          (assignment.EndDateTime == null || assignment.EndDateTime > nowUtc) &&
                          !episodeBerjalan.IsDelete &&
                          (episodeBerjalan.EpisodeStatus == InpEpisodeStatus.Admitted ||
                           episodeBerjalan.EpisodeStatus == InpEpisodeStatus.DischargePending)
                    select assignment.EpisodeId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var perawatanTertutup = new List<Guid>();

            if (includeClosedEpisodes)
            {
                // Perawatan tertutup yang dokter ini pernah menjadi DPJP-nya. Penyaring "DPJP
                // TERAKHIR" belum dapat dinyatakan pada query ini, karena ia menuntut
                // perbandingan antar-baris penugasan; ia ditegakkan sesudahnya, per perawatan.
                var kandidat = await _dbContext.Set<InpDoctorAssignment>()
                    .AsNoTracking()
                    .Where(x =>
                        x.DoctorId == actorDoctorId &&
                        x.AssignmentRole == InpDoctorAssignmentRole.Dpjp &&
                        !x.IsDelete &&
                        !x.IsCancel)
                    .Select(x => x.EpisodeId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var kandidatTertutup = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .Where(x =>
                        kandidat.Contains(x.Id) &&
                        !x.IsDelete &&
                        x.EpisodeStatus == InpEpisodeStatus.Closed)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                // BE-RWI-095 kriteria 2, BE-RWI-096 kriteria 3. Hanya perawatan yang DPJP
                // TERAKHIRNYA dokter ini. Perawatan yang DPJP-nya sudah berganti milik
                // penggantinya, bukan miliknya.
                foreach (var episodeId in kandidatTertutup)
                {
                    var dpjpTerakhir = await _contextService.FindLastAttendingDoctorIdAsync(
                        episodeId, cancellationToken);

                    if (dpjpTerakhir.HasValue && dpjpTerakhir.Value == actorDoctorId)
                        perawatanTertutup.Add(episodeId);
                }
            }

            var seluruhEpisode = perawatanBerjalan
                .Concat(perawatanTertutup)
                .Distinct()
                .ToList();

            if (seluruhEpisode.Count == 0)
                return new List<CpptVerificationWorklistItem>();

            var batasEpisodeTertutup = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x =>
                    seluruhEpisode.Contains(x.Id) &&
                    !x.IsDelete &&
                    x.EpisodeStatus == InpEpisodeStatus.Closed)
                .Select(x => new { x.Id, x.ClosedAt })
                .ToDictionaryAsync(x => x.Id, x => x.ClosedAt, cancellationToken);

            // BE-RWI-096 kriteria 2. Perawatan tanpa satu pun entri tertunda TIDAK ikut
            // terbentuk di bawah, dan itulah cara ia keluar dari daftar - bukan lewat penanda
            // yang perlu dimatikan seseorang.
            var entriTertunda = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking()
                .Where(x =>
                    x.InpEpisodeId.HasValue &&
                    seluruhEpisode.Contains(x.InpEpisodeId.Value) &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    (x.VerificationStatus == CpptVerificationStatus.Pending ||
                     x.VerificationStatus == CpptVerificationStatus.Overdue))
                .Select(x => new
                {
                    EpisodeId = x.InpEpisodeId!.Value,
                    x.NoteDateTime,
                    x.VerificationDueAt
                })
                .ToListAsync(cancellationToken);

            // BE-RWI-095 kriteria 5. Daftar kerja hanya memuat pekerjaan yang benar-benar dapat
            // diselesaikan lewat pengecualian episode Closed. Baris pascapenutupan tetap berada
            // di basis data untuk audit, tetapi tidak ditawarkan sebagai aksi yang pasti 422.
            entriTertunda = entriTertunda
                .Where(x =>
                    !batasEpisodeTertutup.TryGetValue(x.EpisodeId, out var closedAt) ||
                    (closedAt.HasValue && x.NoteDateTime <= closedAt.Value))
                .ToList();

            if (entriTertunda.Count == 0)
                return new List<CpptVerificationWorklistItem>();

            var episodeYangTerpakai = entriTertunda
                .Select(x => x.EpisodeId)
                .Distinct()
                .ToList();

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => episodeYangTerpakai.Contains(x.Id) && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.EpisodeNumber,
                    x.PatientId,
                    x.EpisodeStatus
                })
                .ToListAsync(cancellationToken);

            var patientIds = episode.Select(x => x.PatientId).Distinct().ToList();

            // RWI-DEC-127 butir (2). Identitas pasien yang dibaca MINIMUM: nama dan nomor rekam
            // medis. Daftar tunggu tidak membuka satu pun kolom klinis pasien.
            var pasien = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => patientIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.MedicalRecordNumber })
                .ToListAsync(cancellationToken);

            var hasil = new List<CpptVerificationWorklistItem>();

            foreach (var e in episode)
            {
                var entri = entriTertunda.Where(x => x.EpisodeId == e.Id).ToList();

                if (entri.Count == 0)
                    continue;

                var p = pasien.FirstOrDefault(x => x.Id == e.PatientId);

                var terlambat = entri
                    .Where(x => x.VerificationDueAt.HasValue && x.VerificationDueAt.Value < nowUtc)
                    .ToList();

                int? lamaTerlambat = null;

                if (terlambat.Count > 0)
                {
                    var batasPalingAwal = terlambat.Min(x => x.VerificationDueAt!.Value);
                    lamaTerlambat = (int)Math.Floor((nowUtc - batasPalingAwal).TotalMinutes);
                }

                var sudahDitutup = e.EpisodeStatus == InpEpisodeStatus.Closed;

                hasil.Add(new CpptVerificationWorklistItem
                {
                    EpisodeId = e.Id,
                    EpisodeNumber = e.EpisodeNumber,
                    PatientId = e.PatientId,
                    PatientName = p?.FullName,
                    MedicalRecordNumber = p?.MedicalRecordNumber,
                    EpisodeStatus = e.EpisodeStatus,
                    EpisodeStatusName = NamaStatusPerawatan(e.EpisodeStatus),
                    IsClosedEpisodeException = sudahDitutup,
                    PendingCount = entri.Count,
                    OldestPendingNoteDateTime = entri.Min(x => x.NoteDateTime),
                    IsOverdue = terlambat.Count > 0,
                    LateByMinutes = lamaTerlambat
                });
            }

            // Yang paling lama menunggu berada di atas. Perawatan tertutup umumnya jatuh ke
            // atas dengan sendirinya, karena entrinya memang yang paling lama menggantung.
            return hasil
                .OrderBy(x => x.OldestPendingNoteDateTime ?? DateTime.MaxValue)
                .ToList();
        }

        /// <summary>
        /// Label status perawatan yang siap ditampilkan pada daftar tunggu verifikasi.
        /// </summary>
        private static string NamaStatusPerawatan(InpEpisodeStatus status) => status switch
        {
            InpEpisodeStatus.Draft => "Draf",
            InpEpisodeStatus.Admitted => "Dirawat",
            InpEpisodeStatus.DischargePending => "Menunggu Pulang",
            InpEpisodeStatus.Closed => "Selesai",
            InpEpisodeStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        /// <summary>
        /// Keadaan verifikasi seluruh catatan terpadu pada satu perawatan, beserta daftar
        /// pantaunya.
        /// </summary>
        /// <remarks>
        /// Status <c>Overdue</c> <b>diturunkan</b> dari batas waktu, bukan disimpan. Menyimpannya
        /// menuntut ada yang menjalankan pekerjaan latar setiap menit hanya untuk menaikkan
        /// status, dan hasilnya tetap basi di antara dua jalannya.
        /// </remarks>
        /// <param name="episodeId">Perawatan yang dibaca.</param>
        /// <param name="nowUtc">Saat yang dipakai menilai keterlambatan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<CpptVerificationStatusSummary> GetStatusByEpisodeAsync(
            Guid episodeId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            var catatan = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.NoteDateTime)
                .Select(x => new
                {
                    x.Id,
                    x.ProgressNoteNumber,
                    x.ProfessionType,
                    x.ProviderUserId,
                    x.NoteDateTime,
                    x.VerificationStatus,
                    x.VerificationDueAt,

                    // BE-RWI-067. Kedua sumber nama ikut terbaca pada query yang sama; tidak ada
                    // pembacaan tambahan per baris, dan tidak satu pun kolom isi catatan diambil.
                    NamaSnapshot = x.ProviderDisplayNameSnapshot,
                    NamaAkun = x.ProviderUser != null ? x.ProviderUser.DisplayName : null
                })
                .ToListAsync(cancellationToken);

            var daftarPantau = catatan
                .Where(x => x.VerificationStatus == CpptVerificationStatus.Pending
                            || x.VerificationStatus == CpptVerificationStatus.Overdue)
                .Select(x => new CpptVerificationWatchItem
                {
                    NoteId = x.Id,
                    ProgressNoteNumber = x.ProgressNoteNumber,
                    ProfessionType = x.ProfessionType,
                    ProviderUserId = x.ProviderUserId,
                    ProviderName = NamaPenulis(x.NamaSnapshot, x.NamaAkun),
                    NoteDateTime = x.NoteDateTime,
                    VerificationStatus = x.VerificationStatus,
                    VerificationDueAt = x.VerificationDueAt,
                    IsOverdue = x.VerificationDueAt.HasValue && x.VerificationDueAt.Value < nowUtc
                })
                .ToList();

            return new CpptVerificationStatusSummary
            {
                EpisodeId = episodeId,
                TotalNoteCount = catatan.Count,
                NotRequiredCount = catatan.Count(
                    x => x.VerificationStatus == CpptVerificationStatus.NotRequired),
                PendingCount = catatan.Count(
                    x => x.VerificationStatus == CpptVerificationStatus.Pending),
                VerifiedCount = catatan.Count(
                    x => x.VerificationStatus == CpptVerificationStatus.Verified),
                OverdueCount = daftarPantau.Count(x => x.IsOverdue),

                // Kebijakan dianggap kosong selama tidak satu pun catatan diberi batas waktu.
                // Tidak ada tabel kebijakan yang dibaca di sini, dan itu disengaja: kebijakannya
                // memang belum disahkan, dan membuat tabelnya sekarang berarti menebak bentuknya.
                IsVerificationPolicyEmpty = catatan.TrueForAll(x => !x.VerificationDueAt.HasValue),
                WatchList = daftarPantau
            };
        }

        /// <summary>
        /// Mengembalikan catatan terverifikasi ke keadaan menunggu verifikasi setelah dikoreksi.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-053</c> kriteria 6, <c>state-transition-matrix.md</c> bagian 3. Verifikasi
        /// menyatakan "saya sudah membaca isi ini". Begitu isinya bertambah lewat koreksi,
        /// pernyataan itu berhenti berlaku, dan membiarkannya tetap <c>Verified</c> berarti
        /// menampilkan tanda tangan atas isi yang belum pernah dibaca.
        /// </para>
        /// <para>
        /// Catatan berstatus tidak-diwajibkan <b>tidak</b> dinaikkan menjadi menunggu. Rumah
        /// sakit yang tidak mewajibkan verifikasi tidak boleh tiba-tiba punya daftar pantau
        /// hanya karena ada koreksi.
        /// </para>
        /// </remarks>
        /// <param name="documentKind">Jenis dokumen yang dikoreksi.</param>
        /// <param name="documentId">Id dokumen pada tabel asalnya.</param>
        /// <param name="actorUserId">Pengguna yang membuat koreksi.</param>
        /// <param name="nowUtc">Saat koreksi dibuat.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        /// <returns>Benar bila ada catatan yang dikembalikan ke keadaan menunggu.</returns>
        public async Task<bool> ResetVerificationOnCorrectionAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid actorUserId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            if (documentKind != ClinicalDocumentKind.ProgressNote)
                return false;

            var note = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .FirstOrDefaultAsync(x => x.Id == documentId && !x.IsDelete, cancellationToken);

            if (note == null || note.VerificationStatus != CpptVerificationStatus.Verified)
                return false;

            note.VerificationStatus = CpptVerificationStatus.Pending;
            note.VerifiedAt = null;
            note.VerifiedByUserId = null;
            note.UpdateDateTime = nowUtc;
            note.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        /// <summary>
        /// Nama penulis sebuah catatan pada daftar pantau, atau <c>null</c> bila ia tidak dapat
        /// disebutkan - <c>BE-RWI-067</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Snapshot lebih dulu, dan urutan itu mengikat.</b> Daftar pantau menyebut siapa yang
        /// menulis sebuah catatan pada saat catatan itu ditulis. Ns. Sari yang menulis catatan
        /// pada 1 September tetap Ns. Sari, walaupun akunnya berganti nama menjadi Ns. Sari
        /// Wijaya pada 5 September. Membaca nama akun lebih dulu akan menulis ulang riwayat
        /// setiap kali seseorang menikah, bergelar baru, atau namanya diperbaiki.
        /// </para>
        /// <para>
        /// Penulis yang tidak dapat dikenali sama sekali menghasilkan <c>null</c>, bukan nomor
        /// pengguna dan bukan nama tebakan.
        /// </para>
        /// </remarks>
        private static string? NamaPenulis(string? snapshot, string? namaAkun)
        {
            if (!string.IsNullOrWhiteSpace(snapshot))
                return snapshot.Trim();

            if (!string.IsNullOrWhiteSpace(namaAkun))
                return namaAkun.Trim();

            return null;
        }

        /// <summary>
        /// Menemukan perawatan rawat inap yang menaungi sebuah catatan terpadu.
        /// </summary>
        /// <remarks>
        /// Kolom penanda perawatan dipakai lebih dulu; bila kosong, perawatan diturunkan dari
        /// kunjungannya. Catatan lama yang ditulis sebelum kolom penanda ada tetap terjawab
        /// dengan benar.
        /// </remarks>
        private async Task<Guid?> ResolveEpisodeIdAsync(
            TrxPatientIntegratedProgressNote note,
            CancellationToken cancellationToken)
        {
            if (note.InpEpisodeId.HasValue && note.InpEpisodeId.Value != Guid.Empty)
                return note.InpEpisodeId;

            if (!note.EncounterId.HasValue || note.EncounterId.Value == Guid.Empty)
                return null;

            var konteks = await _contextService.ResolveAsync(
                note.EncounterId.Value,
                forNewDocument: false,
                cancellationToken: cancellationToken);

            return konteks.IsResolved ? konteks.Context!.EpisodeId : null;
        }
    }
}
