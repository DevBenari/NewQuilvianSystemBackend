using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
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

        public DateTime NoteDateTime { get; init; }

        public CpptVerificationStatus VerificationStatus { get; init; }

        public DateTime? VerificationDueAt { get; init; }

        /// <summary>Benar bila catatan sudah melewati batas waktu verifikasinya.</summary>
        public bool IsOverdue { get; init; }
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
                    StatusCodes.Status403Forbidden, PenolakanBukanDpjp);
            }

            // VAL-DOK-07, RWI-RULE-030. Yang diperiksa adalah penugasan yang berlaku SEKARANG.
            var berwenang = await _contextService.IsDoctorAssignedAsync(
                episodeId.Value, actorDoctorId.Value, nowUtc, cancellationToken);

            if (!berwenang)
            {
                return CpptVerificationResult.Fail(
                    StatusCodes.Status403Forbidden, PenolakanBukanDpjp);
            }

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
            note.VerificationStatus = CpptVerificationStatus.Verified;
            note.VerifiedAt = nowUtc;
            note.VerifiedByUserId = actorUserId;
            note.UpdateDateTime = nowUtc;
            note.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return CpptVerificationResult.Ok(note);
        }

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
                    x.VerificationDueAt
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
