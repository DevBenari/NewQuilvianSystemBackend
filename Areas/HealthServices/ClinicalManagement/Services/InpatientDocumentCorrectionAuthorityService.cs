using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Sebab penolakan koreksi atas nama penulis lain, beserta kode HTTP-nya.
    /// </summary>
    public enum InpatientCorrectionAuthorityOutcome
    {
        /// <summary>Boleh mengoreksi atas nama penulis lain.</summary>
        Allowed = 0,

        /// <summary>
        /// Dokumen tidak berada di bawah perawatan rawat inap mana pun, sehingga aturan ini
        /// memang tidak berlaku dan pemeriksaan diserahkan kembali ke mesin koreksi.
        /// </summary>
        NotInpatientDocument = 1,

        /// <summary>Pengguna tidak terhubung ke dokter mana pun.</summary>
        ActorNotDoctor = 2,

        /// <summary>Pengguna dokter, tetapi bukan DPJP perawatan itu.</summary>
        ActorNotAttendingDoctor = 3
    }

    /// <summary>
    /// Hasil pemeriksaan kewenangan koreksi atas nama penulis lain.
    /// </summary>
    public sealed class InpatientCorrectionAuthorityResult
    {
        public InpatientCorrectionAuthorityOutcome Outcome { get; init; }

        public int StatusCode { get; init; }

        public string? ErrorMessage { get; init; }

        /// <summary>Perawatan yang menaungi dokumen, bila memang ada.</summary>
        public Guid? EpisodeId { get; init; }

        /// <summary>Dokter yang melekat pada pengguna, bila dapat ditentukan.</summary>
        public Guid? ActorDoctorId { get; init; }

        public bool IsAllowed =>
            Outcome == InpatientCorrectionAuthorityOutcome.Allowed ||
            Outcome == InpatientCorrectionAuthorityOutcome.NotInpatientDocument;

        internal static InpatientCorrectionAuthorityResult Ok(
            InpatientCorrectionAuthorityOutcome outcome,
            Guid? episodeId = null,
            Guid? actorDoctorId = null) => new()
            {
                Outcome = outcome,
                StatusCode = StatusCodes.Status200OK,
                EpisodeId = episodeId,
                ActorDoctorId = actorDoctorId
            };

        internal static InpatientCorrectionAuthorityResult Denied(
            InpatientCorrectionAuthorityOutcome outcome,
            string message,
            Guid? episodeId = null) => new()
            {
                Outcome = outcome,
                StatusCode = StatusCodes.Status403Forbidden,
                ErrorMessage = message,
                EpisodeId = episodeId
            };
    }

    /// <summary>
    /// Penjaga <c>VAL-DOK-35</c>: koreksi atas nama dokter lain hanya boleh dilakukan DPJP yang
    /// bertanggung jawab atas pasien itu — <c>RWI-DEC-088</c>, <c>INV-DOK-13</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa penjaga ini harus ada, padahal hak aksesnya sudah ada.</b> Penetapan berhalangan
    /// milik <c>MedicalRecordManagement</c> menyatakan "dokter ini berhalangan" — dan berhenti di
    /// situ. Ia <b>tidak menyebut siapa penggantinya</b>. Begitu satu penetapan berlaku, setiap
    /// pemegang butir hak akses <c>ClinicalNoteAddendum : CreateAsSubstitute</c> dapat mengoreksi
    /// catatan dokter itu — termasuk untuk pasien yang sama sekali bukan tanggung jawabnya.
    /// Mesin hak akses tidak dapat menutupnya karena ia mengenal peran, bukan pasien.
    /// </para>
    /// <para>
    /// <b>Karena itu penjaganya berada di sini, bukan di mesin koreksi.</b> Aturan "hanya DPJP
    /// perawatan itu" adalah aturan Rawat Inap, dan sumber kebenarannya adalah penugasan dokter
    /// berperiode <c>InpDoctorAssignment</c>. Uji yang hanya menguji hak akses tidak akan
    /// menangkap celah ini — seluruh pemeriksaan hak akses memang lolos, dan penolakannya datang
    /// dari aturan bisnis.
    /// </para>
    /// <para>
    /// <b>Dokumen di luar rawat inap dilewatkan.</b> Catatan poliklinik dan IGD tidak punya
    /// perawatan rawat inap, sehingga aturan ini tidak berlaku bagi keduanya. Menolaknya di sini
    /// akan mematikan jalur koreksi yang hari ini sudah berjalan pada modul lain.
    /// </para>
    /// <para>
    /// <b>Perawatan yang sudah ditutup tetap dilayani.</b> Koreksi justru paling sering
    /// dibutuhkan setelah pasien pulang — <c>FR-DOK-047</c>. Karena penugasan dokter biasanya
    /// diakhiri saat perawatan ditutup, kewenangan pada perawatan tertutup dinilai dari
    /// penugasan terakhir yang pernah berlaku, bukan dari penugasan yang berlaku hari ini.
    /// </para>
    /// <para>
    /// Tidak memakai interface, mengikuti pola service pada repository ini.
    /// </para>
    /// </remarks>
    public class InpatientDocumentCorrectionAuthorityService
    {
        /// <summary>
        /// Kalimat penolakan <c>VAL-DOK-35</c>, apa adanya seperti pada validation matrix.
        /// </summary>
        public const string PenolakanBukanDpjpAktif =
            "Koreksi atas nama dokter lain hanya dapat dilakukan DPJP yang sedang bertanggung " +
            "jawab atas pasien ini.";

        /// <summary>
        /// Kalimat penolakan bagi pengguna yang tidak terhubung ke dokter mana pun.
        /// </summary>
        public const string PenolakanBukanDokter =
            "Koreksi atas nama dokter lain hanya dapat dilakukan dokter.";

        private readonly ApplicationDbContext _dbContext;

        public InpatientDocumentCorrectionAuthorityService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menjawab apakah pengguna boleh mengoreksi sebuah dokumen atas nama penulis lain.
        /// </summary>
        /// <param name="documentKind">Jenis dokumen yang hendak dikoreksi.</param>
        /// <param name="documentId">Id dokumen pada tabel asalnya.</param>
        /// <param name="user">Identitas pengguna yang sedang masuk.</param>
        /// <param name="actorUserId">Id pengguna yang sedang masuk.</param>
        /// <param name="atUtc">Saat yang dipakai menilai periode penugasan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<InpatientCorrectionAuthorityResult> EnsureMaySubstituteAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            ClaimsPrincipal? user,
            Guid actorUserId,
            DateTime atUtc,
            CancellationToken cancellationToken = default)
        {
            // Baris keutuhan adalah satu-satunya tempat yang mengetahui kunjungan sebuah dokumen
            // tanpa harus mengenal tiga belas tabel klinis satu per satu.
            var keutuhan = await _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.DocumentKind == documentKind
                            && x.DocumentId == documentId
                            && !x.IsDelete)
                .Select(x => new { x.EncounterId })
                .FirstOrDefaultAsync(cancellationToken);

            if (keutuhan == null)
                return InpatientCorrectionAuthorityResult.Ok(
                    InpatientCorrectionAuthorityOutcome.NotInpatientDocument);

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.EncounterId == keutuhan.EncounterId && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.EpisodeStatus })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
                return InpatientCorrectionAuthorityResult.Ok(
                    InpatientCorrectionAuthorityOutcome.NotInpatientDocument);

            var doctorId = await ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);

            if (doctorId == null)
            {
                return InpatientCorrectionAuthorityResult.Denied(
                    InpatientCorrectionAuthorityOutcome.ActorNotDoctor,
                    PenolakanBukanDokter,
                    episode.Id);
            }

            var berwenang = await IsAttendingDoctorAsync(
                episode.Id, doctorId.Value, episode.EpisodeStatus, atUtc, cancellationToken);

            if (!berwenang)
            {
                return InpatientCorrectionAuthorityResult.Denied(
                    InpatientCorrectionAuthorityOutcome.ActorNotAttendingDoctor,
                    PenolakanBukanDpjpAktif,
                    episode.Id);
            }

            return InpatientCorrectionAuthorityResult.Ok(
                InpatientCorrectionAuthorityOutcome.Allowed, episode.Id, doctorId);
        }

        /// <summary>
        /// Menjawab apakah seorang dokter adalah penanggung jawab perawatan itu.
        /// </summary>
        /// <remarks>
        /// Pada perawatan yang masih berjalan, yang dinilai adalah penugasan yang periodenya
        /// memuat saat ini. Pada perawatan yang sudah ditutup atau dibatalkan, penugasan
        /// biasanya sudah diakhiri, sehingga yang dinilai adalah penugasan <b>terakhir</b> yang
        /// pernah berlaku pada perawatan itu. Tanpa keringanan ini, koreksi setelah pasien
        /// pulang menjadi mustahil bagi siapa pun — persis kebalikan dari yang diminta
        /// <c>FR-DOK-047</c>.
        /// </remarks>
        private async Task<bool> IsAttendingDoctorAsync(
            Guid episodeId,
            Guid doctorId,
            InpEpisodeStatus episodeStatus,
            DateTime atUtc,
            CancellationToken cancellationToken)
        {
            var berlakuSekarang = await _dbContext.Set<InpDoctorAssignment>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EpisodeId == episodeId &&
                    x.DoctorId == doctorId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.StartDateTime <= atUtc &&
                    (x.EndDateTime == null || x.EndDateTime > atUtc),
                    cancellationToken);

            if (berlakuSekarang)
                return true;

            var masihBerjalan =
                episodeStatus == InpEpisodeStatus.Admitted ||
                episodeStatus == InpEpisodeStatus.DischargePending;

            if (masihBerjalan)
                return false;

            var penugasanTerakhir = await _dbContext.Set<InpDoctorAssignment>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderByDescending(x => x.StartDateTime)
                .Select(x => (Guid?)x.DoctorId)
                .FirstOrDefaultAsync(cancellationToken);

            return penugasanTerakhir == doctorId;
        }

        /// <summary>
        /// Menemukan baris dokter yang melekat pada pengguna yang sedang masuk.
        /// </summary>
        /// <remarks>
        /// Urutannya mengikuti pola yang sudah dipakai
        /// <c>PatientAssessmentController.ResolveCurrentDoctorIdAsync</c> dan
        /// <c>DoctorQueueController.ResolveAllowedDoctorIdAsync</c>: klaim identitas dokter
        /// lebih dulu, lalu penautan lewat profil tenaga kerja, lalu surel. Ketiganya bersandar
        /// pada <b>data</b>; tidak satu pun membaca nama peran, nama jabatan, maupun
        /// <c>UserType</c>.
        /// </remarks>
        private async Task<Guid?> ResolveActorDoctorIdAsync(
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var doctorIdClaim = user?.FindFirstValue("doctor_id") ?? user?.FindFirstValue("DoctorId");

            if (Guid.TryParse(doctorIdClaim, out var dariKlaimDokter) && dariKlaimDokter != Guid.Empty)
            {
                var adaDokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == dariKlaimDokter && !x.IsDelete && x.IsActive,
                              cancellationToken);

                if (adaDokter)
                    return dariKlaimDokter;
            }

            var workforceClaim = user?.FindFirstValue("workforce_profile_id")
                                 ?? user?.FindFirstValue("WorkforceProfileId");

            Guid? workforceProfileId =
                Guid.TryParse(workforceClaim, out var dariKlaimProfil) && dariKlaimProfil != Guid.Empty
                    ? dariKlaimProfil
                    : null;

            var pengguna = actorUserId == Guid.Empty
                ? null
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => x.Id == actorUserId)
                    .Select(x => new { x.WorkforceProfileId, x.Email })
                    .FirstOrDefaultAsync(cancellationToken);

            workforceProfileId ??= pengguna?.WorkforceProfileId;

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                var dokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId.Value &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (dokter.HasValue)
                    return dokter;
            }

            if (!string.IsNullOrWhiteSpace(pengguna?.Email))
            {
                var surel = pengguna.Email.ToLower();

                var dokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x =>
                        x.Email != null &&
                        x.Email.ToLower() == surel &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (dokter.HasValue)
                    return dokter;
            }

            return null;
        }
    }
}
