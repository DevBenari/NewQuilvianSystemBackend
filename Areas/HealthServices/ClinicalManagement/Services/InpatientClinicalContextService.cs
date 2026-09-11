using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Sebab penolakan konteks klinis rawat inap. Setiap nilai memetakan tepat satu kode HTTP,
    /// sehingga pemanggil tidak perlu menerjemahkannya sendiri-sendiri.
    /// </summary>
    public enum InpatientClinicalContextOutcome
    {
        /// <summary>Konteks terbentuk; dokumen boleh dibuat.</summary>
        Resolved = 0,

        /// <summary>Kunjungannya sendiri tidak ada atau sudah dihapus.</summary>
        EncounterNotFound = 1,

        /// <summary>Kunjungan tidak memiliki perawatan rawat inap sama sekali.</summary>
        NoInpatientEpisode = 2,

        /// <summary>Perawatan masih Draft; pasien belum benar-benar dirawat.</summary>
        EpisodeNotAdmitted = 3,

        /// <summary>Perawatan Closed atau Cancelled; dokumen baru ditolak.</summary>
        EpisodeClosed = 4,

        /// <summary>Pasien pada dokumen tidak sama dengan pasien pada perawatan.</summary>
        PatientMismatch = 5,

        /// <summary>Dokter tidak berwenang atas pasien pada perawatan itu.</summary>
        DoctorNotAuthorized = 6,

        /// <summary>Penanda perawatan yang dikirim tidak cocok dengan perawatan milik kunjungan.</summary>
        EpisodeMismatch = 7,

        /// <summary>
        /// Perawat tidak bertugas di unit tempat perawatan itu berada - <c>GUARD-INP-08</c>,
        /// <c>BE-RWI-078</c>.
        /// </summary>
        NurseNotAuthorized = 8,

        /// <summary>
        /// Pengguna yang menulis tidak tertaut ke satu baris pegawai pun, sehingga penulisnya
        /// tidak dapat disebut - <c>GUARD-INP-07</c>, <c>BE-RWI-078</c>.
        /// </summary>
        NurseNotIdentified = 9,

        /// <summary>
        /// Pengguna yang menulis tidak tertaut ke satu baris dokter pun, sehingga penulis
        /// klinisnya tidak dapat disebut — <c>GUARD-INP-05</c>, <c>BE-RWI-076</c>.
        /// </summary>
        DoctorNotIdentified = 10,

        /// <summary>
        /// Penulis yang disebut payload berbeda dari dokter milik pengguna terautentikasi —
        /// <c>GUARD-INP-05</c>, <c>BE-RWI-076</c>.
        /// </summary>
        DoctorImpersonation = 11
    }

    /// <summary>
    /// Jawaban atas pertanyaan "dokumen ini milik perawatan yang mana".
    /// </summary>
    public sealed class InpatientClinicalContext
    {
        public Guid EpisodeId { get; init; }

        public string EpisodeNumber { get; init; } = string.Empty;

        public Guid EncounterId { get; init; }

        public Guid PatientId { get; init; }

        public Guid ServiceUnitId { get; init; }

        public InpEpisodeStatus EpisodeStatus { get; init; }

        /// <summary>
        /// Saat pasien benar-benar masuk kamar. Kosong selama perawatan masih <c>Draft</c>.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-046</c>, <c>VAL-DOK-14</c>. Waktu pemeriksaan yang lebih awal daripada saat
        /// ini menyatakan pemeriksaan yang terjadi sebelum pasien berada di kamar, dan itu
        /// membuat lini masa perkembangan pasien menyesatkan.
        /// </remarks>
        public DateTime? AdmittedAt { get; init; }

        /// <summary>
        /// Perawatan yang masih berjalan, yaitu <c>Admitted</c> atau <c>DischargePending</c>.
        /// Dokumen baru hanya boleh lahir di atas perawatan berjalan.
        /// </summary>
        public bool IsEpisodeOpen { get; init; }

        /// <summary>
        /// DPJP yang berwenang pada saat yang ditanyakan. Kosong bila tidak ada penugasan yang
        /// berlaku pada saat itu.
        /// </summary>
        public Guid? AttendingDoctorId { get; init; }

        /// <summary>
        /// Benar bila dokter yang ditanyakan memiliki penugasan berlaku pada perawatan itu.
        /// Bernilai benar juga ketika pemanggil memang tidak menanyakan dokter mana pun.
        /// </summary>
        public bool IsDoctorAuthorized { get; init; }

        /// <summary>
        /// Benar bila perawat yang ditanyakan bertugas di unit tempat perawatan itu berada.
        /// Bernilai benar juga ketika pemanggil memang tidak menanyakan perawat mana pun.
        /// </summary>
        /// <remarks><c>GUARD-INP-08</c>, <c>BE-RWI-078</c>.</remarks>
        public bool IsNurseAuthorized { get; init; }

        /// <summary>
        /// Dokter milik pengguna yang sedang menulis, hasil <c>GUARD-INP-05</c>. Kosong pada
        /// jalur yang memang tidak menulis sebagai dokter.
        /// </summary>
        /// <remarks>
        /// Inilah satu-satunya nilai yang boleh dipakai sebagai penulis dokter pada baris yang
        /// disimpan. Penanda dokter dari payload, dari antrean, maupun dari kunjungan
        /// <b>tidak</b> boleh dipakai menggantikannya — <c>BE-RWI-076</c>.
        /// </remarks>
        public Guid? ActorDoctorId { get; init; }
    }

    /// <summary>
    /// Hasil pemanggilan konteks: berhasil beserta isinya, atau gagal beserta kode dan kalimat
    /// penolakannya.
    /// </summary>
    public sealed class InpatientClinicalContextResult
    {
        public InpatientClinicalContextOutcome Outcome { get; init; }

        public int StatusCode { get; init; }

        public string? ErrorMessage { get; init; }

        public InpatientClinicalContext? Context { get; init; }

        public bool IsResolved => Outcome == InpatientClinicalContextOutcome.Resolved;

        internal static InpatientClinicalContextResult Ok(InpatientClinicalContext context) => new()
        {
            Outcome = InpatientClinicalContextOutcome.Resolved,
            StatusCode = StatusCodes.Status200OK,
            Context = context
        };

        internal static InpatientClinicalContextResult Fail(
            InpatientClinicalContextOutcome outcome,
            int statusCode,
            string message) => new()
            {
                Outcome = outcome,
                StatusCode = statusCode,
                ErrorMessage = message
            };
    }

    /// <summary>
    /// Satu tempat yang menjawab "dokumen klinis ini milik perawatan rawat inap yang mana, dan
    /// siapa yang berwenang menulisnya" — <c>CON-INP-015</c>, <c>INT-DOK-01</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa service ini ada.</b> Sebelum ini, pembuatan catatan dokter dan pengkajian hanya
    /// mengenal dua keadaan: kunjungan berantre, atau kunjungan IGD. Pasien rawat inap tidak
    /// termasuk keduanya, sehingga dokumentasinya tidak punya pintu masuk sama sekali —
    /// <c>DOK-TRC-INT-01</c>. Menambal setiap controller sendiri-sendiri akan melahirkan dua
    /// salinan aturan yang sama, dan sub-modul keperawatan membutuhkan aturan yang sama persis
    /// lewat <c>INT-KEP-01</c>. Karena itu jawabannya dikumpulkan di sini dan dipakai bersama —
    /// <c>INT-DOK-09</c>.
    /// </para>
    /// <para>
    /// <b>Nol baris antrean.</b> Service ini hanya membaca dan tidak pernah menyentuh
    /// <c>TrxQueue</c>. Jalan pintas berupa "membuatkan antrean semu supaya jalur lama terpakai"
    /// ditolak dengan sadar: antrean semu akan muncul pada layar antrean poliklinik dan ikut
    /// terhitung pada laporan kunjungan.
    /// </para>
    /// <para>
    /// <b>Kewenangan diturunkan dari data, bukan dari nama peran.</b> Yang diperiksa adalah
    /// penugasan dokter berperiode <c>InpDoctorAssignment</c> pada perawatan yang bersangkutan,
    /// sesuai keadaan pada saat yang ditanyakan. Tidak ada pemeriksaan nama peran, nama jabatan,
    /// maupun <c>UserType</c> di sini; hak akses tetap ditentukan admin lewat layar Akses Role.
    /// </para>
    /// <para>
    /// Tidak memakai interface, mengikuti pola service pada repository ini.
    /// </para>
    /// </remarks>
    public class InpatientClinicalContextService
    {
        /// <summary>
        /// Kalimat penolakan <c>GUARD-INP-08</c>: perawat menulis untuk pasien yang dirawat di
        /// unit lain.
        /// </summary>
        /// <remarks>
        /// Kalimatnya menyebut <b>unit</b>, bukan penugasan, karena itulah yang menentukan.
        /// Perawat yang ditolak di sini biasanya benar-benar sedang memegang pasien itu di
        /// layarnya, dan penjelasan yang menyebut "bukan pasien Anda" akan menyesatkan.
        /// </remarks>
        public const string PenolakanPerawatUnitLain =
            "Pasien ini dirawat di unit lain, sehingga dokumentasi keperawatannya tidak dapat " +
            "ditulis dari unit tempat Anda bertugas.";

        /// <summary>
        /// Kalimat penolakan <c>GUARD-INP-07</c>: pengguna tidak tertaut ke data pegawai mana pun.
        /// </summary>
        public const string PenolakanPerawatTanpaPegawai =
            "Akun Anda belum tertaut ke data pegawai, sehingga unit tempat Anda bertugas tidak " +
            "dapat ditentukan. Hubungi bagian kepegawaian untuk menautkannya.";

        /// <summary>
        /// Kalimat penolakan <c>GUARD-INP-05</c>: pengguna tidak tertaut ke baris dokter mana pun.
        /// </summary>
        public const string PenolakanBukanDokter =
            "Akun Anda belum tertaut ke data dokter, sehingga catatan klinis ini tidak dapat " +
            "disimpan atas nama siapa pun. Hubungi bagian kepegawaian untuk menautkannya.";

        /// <summary>
        /// Kalimat penolakan <c>GUARD-INP-05</c>: payload menyebut dokter lain sebagai penulis.
        /// </summary>
        public const string PenolakanMenulisAtasNamaDokterLain =
            "Catatan klinis hanya dapat disimpan atas nama dokter yang menuliskannya sendiri.";

        private readonly ApplicationDbContext _dbContext;

        public InpatientClinicalContextService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menjawab apakah sebuah kunjungan sedang menaungi perawatan rawat inap yang berjalan.
        /// </summary>
        /// <remarks>
        /// Dipakai penjaga yang hanya perlu membedakan rawat inap dari rawat jalan — misalnya
        /// pelonggaran jumlah catatan dan resep — tanpa memerlukan seluruh isi konteks.
        /// Mengembalikan identitas perawatan berjalan, atau kosong bila tidak ada.
        /// </remarks>
        public async Task<Guid?> FindOpenEpisodeIdAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            if (encounterId == Guid.Empty)
                return null;

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.EpisodeStatus })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
                return null;

            return IsOpen(episode.EpisodeStatus) ? episode.Id : null;
        }

        /// <summary>
        /// Membentuk konteks klinis rawat inap untuk satu kunjungan.
        /// </summary>
        /// <param name="encounterId">Kunjungan yang menaungi dokumen.</param>
        /// <param name="expectedPatientId">
        /// Pasien yang tertulis pada dokumen. Bila terisi dan berbeda dari pasien perawatan,
        /// permintaan ditolak <c>400</c> — penjaga salah pasien.
        /// </param>
        /// <param name="expectedEpisodeId">
        /// Penanda perawatan yang ikut dikirim pemanggil. Bila terisi dan tidak cocok dengan
        /// perawatan milik kunjungan itu, permintaan ditolak <c>400</c> — <c>VAL-DOK-26</c>.
        /// </param>
        /// <param name="doctorId">
        /// Dokter yang hendak menulis. Bila terisi dan tidak memiliki penugasan berlaku pada
        /// perawatan itu, permintaan ditolak <c>403</c>.
        /// </param>
        /// <param name="nurseEmployeeId">
        /// Perawat yang hendak menulis, dalam bentuk penanda <c>MstEmployee</c> milik pengguna
        /// terautentikasi. Bila terisi dan perawat itu tidak bertugas di unit tempat perawatan
        /// berada, permintaan ditolak <c>403</c> — <c>GUARD-INP-08</c>, <c>BE-RWI-078</c>.
        /// Kosong berarti pemanggil memang tidak sedang menulis sebagai perawat, misalnya jalur
        /// dokter dan jalur pembacaan.
        /// </param>
        /// <param name="forNewDocument">
        /// Benar ketika yang diminta adalah dokumen <b>baru</b>. Perawatan yang sudah
        /// <c>Closed</c> atau <c>Cancelled</c> hanya menolak dokumen baru; koreksi atas dokumen
        /// lama tetap boleh, sehingga pemanggil koreksi mengirim nilai salah.
        /// </param>
        /// <param name="atUtc">
        /// Saat yang dipakai memeriksa penugasan dokter. Kosong berarti sekarang. Diisi ketika
        /// dokumen dituliskan untuk waktu klinis yang berbeda dari waktu penulisannya.
        /// </param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<InpatientClinicalContextResult> ResolveAsync(
            Guid encounterId,
            Guid? expectedPatientId = null,
            Guid? expectedEpisodeId = null,
            Guid? doctorId = null,
            Guid? nurseEmployeeId = null,
            bool forNewDocument = true,
            DateTime? atUtc = null,
            CancellationToken cancellationToken = default)
        {
            var encounter = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == encounterId && !x.IsDelete)
                .Select(x => new { x.Id, x.PatientId })
                .FirstOrDefaultAsync(cancellationToken);

            if (encounter == null)
            {
                return InpatientClinicalContextResult.Fail(
                    InpatientClinicalContextOutcome.EncounterNotFound,
                    StatusCodes.Status404NotFound,
                    "Kunjungan tidak ditemukan.");
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new
                {
                    x.Id,
                    x.EpisodeNumber,
                    x.EncounterId,
                    x.PatientId,
                    x.ServiceUnitId,
                    x.EpisodeStatus,
                    x.AdmittedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
            {
                return InpatientClinicalContextResult.Fail(
                    InpatientClinicalContextOutcome.NoInpatientEpisode,
                    StatusCodes.Status422UnprocessableEntity,
                    "Kunjungan ini tidak memiliki perawatan rawat inap.");
            }

            if (expectedEpisodeId.HasValue &&
                expectedEpisodeId.Value != Guid.Empty &&
                expectedEpisodeId.Value != episode.Id)
            {
                return InpatientClinicalContextResult.Fail(
                    InpatientClinicalContextOutcome.EpisodeMismatch,
                    StatusCodes.Status400BadRequest,
                    "Perawatan rawat inap tidak sesuai dengan kunjungannya.");
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Draft)
            {
                return InpatientClinicalContextResult.Fail(
                    InpatientClinicalContextOutcome.EpisodeNotAdmitted,
                    StatusCodes.Status422UnprocessableEntity,
                    "Perawatan rawat inap belum dimulai; pasien belum masuk kamar.");
            }

            var isOpen = IsOpen(episode.EpisodeStatus);

            if (!isOpen && forNewDocument)
            {
                return InpatientClinicalContextResult.Fail(
                    InpatientClinicalContextOutcome.EpisodeClosed,
                    StatusCodes.Status422UnprocessableEntity,
                    "Perawatan rawat inap sudah ditutup; dokumen baru tidak dapat dibuat. " +
                    "Gunakan koreksi untuk membetulkan dokumen yang sudah ada.");
            }

            if (expectedPatientId.HasValue &&
                expectedPatientId.Value != Guid.Empty &&
                expectedPatientId.Value != episode.PatientId)
            {
                return InpatientClinicalContextResult.Fail(
                    InpatientClinicalContextOutcome.PatientMismatch,
                    StatusCodes.Status400BadRequest,
                    "Pasien pada dokumen tidak sesuai dengan pasien pada perawatan rawat inap.");
            }

            var instant = atUtc ?? DateTime.UtcNow;

            var attendingDoctorId = await FindAttendingDoctorIdAsync(
                episode.Id, instant, cancellationToken);

            var isDoctorAuthorized = true;

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
            {
                isDoctorAuthorized = await IsDoctorAssignedAsync(
                    episode.Id, doctorId.Value, instant, cancellationToken);

                if (!isDoctorAuthorized)
                {
                    return InpatientClinicalContextResult.Fail(
                        InpatientClinicalContextOutcome.DoctorNotAuthorized,
                        StatusCodes.Status403Forbidden,
                        "Dokter tidak berwenang atas pasien pada perawatan rawat inap ini.");
                }
            }

            var isNurseAuthorized = true;

            // GUARD-INP-08. Diperiksa setelah penjaga episode dan penjaga pasien, sehingga
            // perawat unit lain tetap menerima sebab yang paling menjelaskan keadaannya -
            // episode yang sudah ditutup dijawab 422 lebih dulu, bukan 403.
            if (nurseEmployeeId.HasValue && nurseEmployeeId.Value != Guid.Empty)
            {
                isNurseAuthorized = await IsNurseOnDutyAtUnitAsync(
                    episode.ServiceUnitId, nurseEmployeeId.Value, instant, cancellationToken);

                if (!isNurseAuthorized)
                {
                    return InpatientClinicalContextResult.Fail(
                        InpatientClinicalContextOutcome.NurseNotAuthorized,
                        StatusCodes.Status403Forbidden,
                        PenolakanPerawatUnitLain);
                }
            }

            return InpatientClinicalContextResult.Ok(new InpatientClinicalContext
            {
                EpisodeId = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                EncounterId = episode.EncounterId,
                PatientId = episode.PatientId,
                ServiceUnitId = episode.ServiceUnitId,
                EpisodeStatus = episode.EpisodeStatus,
                AdmittedAt = episode.AdmittedAt,
                IsEpisodeOpen = isOpen,
                AttendingDoctorId = attendingDoctorId,
                IsDoctorAuthorized = isDoctorAuthorized,
                IsNurseAuthorized = isNurseAuthorized
            });
        }

        /// <summary>
        /// Membentuk konteks perawatan untuk <b>jalur tulis dokter</b>, sekaligus menegakkan
        /// <c>GUARD-INP-05</c> dan <c>GUARD-INP-06</c> — <c>BE-RWI-076</c>, <c>RWI-DEC-099</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Kenapa penjagaannya dikumpulkan di sini.</b> Sebelum <c>BE-RWI-076</c>, penjaga
        /// kewenangan pada <see cref="ResolveAsync"/> sudah benar tetapi <b>mati</b>: ia hanya
        /// diuji ketika pemanggil mengirimkan dokter pelaku, dan dari sembilan titik panggil
        /// hanya <c>PhysicianVisitController</c> yang mengirimkannya. Menyalin penjagaannya ke
        /// lima controller berarti lima salinan yang dapat berbeda isi. Karena itu jalur tulis
        /// dokter memanggil method ini, bukan <see cref="ResolveAsync"/> langsung.
        /// </para>
        /// <para>
        /// <b>Batas yang dijaga ketat: rawat jalan tidak ikut berubah.</b> Penjagaan penulis
        /// hanya menyala ketika kunjungan itu benar-benar menaungi perawatan rawat inap. Bila
        /// tidak ada perawatan, <see cref="ResolveAsync"/> menjawab
        /// <see cref="InpatientClinicalContextOutcome.NoInpatientEpisode"/> dan pemanggil
        /// meneruskannya ke jalur rawat jalannya masing-masing, persis seperti sebelum task ini.
        /// Poliklinik, medical check-up, dan IGD karena itu <b>tidak</b> mendadak menuntut
        /// penautan akun ke baris dokter.
        /// </para>
        /// <para>
        /// <b>Urutan penolakan disengaja.</b> Kelayakan perawatan diperiksa lebih dulu, baru
        /// identitas penulis. Dokter yang menulis pada perawatan yang sudah ditutup karena itu
        /// menerima <c>422</c> yang menjelaskan keadaan perawatannya, bukan <c>403</c> yang
        /// membuatnya mengira akunnya bermasalah.
        /// </para>
        /// </remarks>
        /// <param name="user">Pengguna terautentikasi, sumber klaim identitas dokter.</param>
        /// <param name="actorUserId">Penanda <c>ApplicationUser</c> pengguna itu.</param>
        /// <param name="requestedDoctorId">
        /// Penanda dokter yang disebut payload. Bila terisi dan berbeda dari dokter pengguna,
        /// permintaan ditolak <c>403</c> — ia <b>tidak pernah</b> dipakai sebagai penulis.
        /// </param>
        /// <param name="atUtc">
        /// Waktu klinis dokumen. Inilah <c>GUARD-INP-06</c>: kewenangan dinilai pada waktu
        /// klinis, bukan waktu penyimpanan, sehingga backdating tidak dapat dipakai melewati
        /// periode penugasan.
        /// </param>
        public async Task<InpatientClinicalContextResult> ResolveForDoctorWriteAsync(
            ClaimsPrincipal? user,
            Guid actorUserId,
            Guid encounterId,
            Guid? expectedPatientId = null,
            Guid? expectedEpisodeId = null,
            Guid? requestedDoctorId = null,
            bool forNewDocument = true,
            DateTime? atUtc = null,
            CancellationToken cancellationToken = default)
        {
            var actorDoctorId = await ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);

            var hasil = await ResolveAsync(
                encounterId,
                expectedPatientId: expectedPatientId,
                expectedEpisodeId: expectedEpisodeId,
                doctorId: actorDoctorId,
                nurseEmployeeId: null,
                forNewDocument: forNewDocument,
                atUtc: atUtc,
                cancellationToken: cancellationToken);

            // Kunjungan tanpa perawatan rawat inap, perawatan yang belum dimulai, dan perawatan
            // yang sudah ditutup dijawab apa adanya. Jalur rawat jalan pemanggil hidup di sini.
            if (!hasil.IsResolved)
                return hasil;

            // GUARD-INP-05 bagian satu. Sampai di sini kunjungan terbukti menaungi perawatan
            // rawat inap yang berjalan, sehingga penulis klinisnya wajib dapat disebut.
            if (actorDoctorId == null || actorDoctorId.Value == Guid.Empty)
            {
                return InpatientClinicalContextResult.Fail(
                    InpatientClinicalContextOutcome.DoctorNotIdentified,
                    StatusCodes.Status403Forbidden,
                    PenolakanBukanDokter);
            }

            // GUARD-INP-05 bagian dua. Penanda dokter pada payload tidak menentukan penulis.
            // Bila ia menyebut orang lain, permintaan ditolak alih-alih diam-diam dipakai.
            if (requestedDoctorId.HasValue &&
                requestedDoctorId.Value != Guid.Empty &&
                requestedDoctorId.Value != actorDoctorId.Value)
            {
                return InpatientClinicalContextResult.Fail(
                    InpatientClinicalContextOutcome.DoctorImpersonation,
                    StatusCodes.Status403Forbidden,
                    PenolakanMenulisAtasNamaDokterLain);
            }

            return InpatientClinicalContextResult.Ok(new InpatientClinicalContext
            {
                EpisodeId = hasil.Context!.EpisodeId,
                EpisodeNumber = hasil.Context.EpisodeNumber,
                EncounterId = hasil.Context.EncounterId,
                PatientId = hasil.Context.PatientId,
                ServiceUnitId = hasil.Context.ServiceUnitId,
                EpisodeStatus = hasil.Context.EpisodeStatus,
                AdmittedAt = hasil.Context.AdmittedAt,
                IsEpisodeOpen = hasil.Context.IsEpisodeOpen,
                AttendingDoctorId = hasil.Context.AttendingDoctorId,
                IsDoctorAuthorized = hasil.Context.IsDoctorAuthorized,
                IsNurseAuthorized = hasil.Context.IsNurseAuthorized,
                ActorDoctorId = actorDoctorId
            });
        }

        /// <summary>
        /// Menemukan baris dokter yang melekat pada pengguna yang sedang masuk —
        /// <c>GUARD-INP-05</c>.
        /// </summary>
        /// <remarks>
        /// Urutannya mengikuti pola yang sudah dipakai
        /// <c>PhysicianVisitController.ResolveCurrentDoctorIdAsync</c> dan
        /// <c>PatientAssessmentController.ResolveCurrentDoctorIdAsync</c>, dengan satu tambahan
        /// di paling depan: kolom <c>ApplicationUser.DoctorId</c>, yang disebut
        /// <c>api-contract.md</c> <c>0.5.0</c> bagian 0.A.2 sebagai sumber identitas penulis.
        ///
        /// <para>
        /// Keempat langkahnya bersandar pada <b>data</b>. Tidak satu pun membaca nama peran,
        /// nama jabatan, nama departemen, maupun <c>UserType</c>; hak akses tetap ditentukan
        /// admin lewat layar Akses Role.
        /// </para>
        /// </remarks>
        public async Task<Guid?> ResolveActorDoctorIdAsync(
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var pengguna = actorUserId == Guid.Empty
                ? null
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => x.Id == actorUserId)
                    .Select(x => new { x.DoctorId, x.WorkforceProfileId, x.Email })
                    .FirstOrDefaultAsync(cancellationToken);

            if (pengguna?.DoctorId is Guid dariAkun && dariAkun != Guid.Empty &&
                await DokterAktifAsync(dariAkun, cancellationToken))
            {
                return dariAkun;
            }

            var klaimDokter = user?.FindFirst("doctor_id")?.Value
                              ?? user?.FindFirst("DoctorId")?.Value;

            if (Guid.TryParse(klaimDokter, out var dariKlaim) && dariKlaim != Guid.Empty &&
                await DokterAktifAsync(dariKlaim, cancellationToken))
            {
                return dariKlaim;
            }

            var klaimProfil = user?.FindFirst("workforce_profile_id")?.Value
                              ?? user?.FindFirst("WorkforceProfileId")?.Value;

            Guid? workforceProfileId =
                Guid.TryParse(klaimProfil, out var dariKlaimProfil) && dariKlaimProfil != Guid.Empty
                    ? dariKlaimProfil
                    : pengguna?.WorkforceProfileId;

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
                var surel = pengguna.Email!.ToLower();

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

        /// <summary>Benar bila baris dokter itu ada, aktif, dan belum dihapus.</summary>
        private Task<bool> DokterAktifAsync(Guid doctorId, CancellationToken cancellationToken)
        {
            return _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == doctorId && !x.IsDelete && x.IsActive, cancellationToken);
        }

        /// <summary>
        /// Menemukan DPJP yang penugasannya berlaku pada saat tertentu.
        /// </summary>
        /// <remarks>
        /// Penugasan bersifat berperiode. Yang dicari adalah penugasan yang periodenya memuat
        /// saat itu, bukan penugasan terkini — catatan yang ditulis untuk pemeriksaan kemarin
        /// dinilai dengan DPJP yang berwenang kemarin.
        ///
        /// <para>
        /// <b>Hanya baris berperan DPJP yang dibaca, sejak <c>BE-RWI-074</c>.</b> Satu episode
        /// kini dapat punya konsulen dan dokter jaga yang aktif bersamaan. Tanpa saringan
        /// peran, salah satu dari mereka akan terbaca sebagai penanggung jawab pelayanan pada
        /// setiap dokumen klinis yang dibuat. Kewenangan <b>menulis</b> tetap terbuka bagi
        /// ketiga peran; yang disaring di sini hanya pertanyaan "siapa DPJP-nya" —
        /// <c>permission-audit-matrix.md</c> bagian 4-A.1 dan 4-A.3.
        /// </para>
        /// </remarks>
        /// <param name="episodeId">Perawatan yang ditanyakan.</param>
        /// <param name="atUtc">Saat yang dipakai memeriksa periode penugasan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<Guid?> FindAttendingDoctorIdAsync(
            Guid episodeId,
            DateTime atUtc,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<InpDoctorAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.EpisodeId == episodeId &&
                    x.AssignmentRole == InpDoctorAssignmentRole.Dpjp &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.StartDateTime <= atUtc &&
                    (x.EndDateTime == null || x.EndDateTime > atUtc))
                .OrderByDescending(x => x.StartDateTime)
                .Select(x => (Guid?)x.DoctorId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Menjawab apakah seorang dokter memiliki penugasan yang berlaku pada perawatan itu.
        /// </summary>
        /// <remarks>
        /// Satu episode dapat memiliki lebih dari satu dokter berwenang pada saat yang sama,
        /// misalnya setelah pendelegasian. Karena itu yang diperiksa adalah keberadaan penugasan
        /// miliknya, bukan kesamaan dengan satu DPJP terpilih.
        ///
        /// <para>
        /// <b>Saringan peran sengaja tidak dipasang di sini.</b> Konsulen dan dokter jaga
        /// memang <b>boleh</b> menulis dokumen klinis; yang tertutup bagi mereka adalah
        /// keputusan atas arah perawatan, dan itu dijaga <c>GUARD-INP-01</c> sampai
        /// <c>GUARD-INP-04</c> di dalam <c>InpEpisodeService</c>, bukan di sini. Menambahkan
        /// <c>AssignmentRole = Dpjp</c> pada pemeriksaan ini akan menutup pencatatan visite
        /// konsulen — persis kebalikan dari yang diminta
        /// <c>permission-audit-matrix.md</c> bagian 4-A.3.
        /// </para>
        /// </remarks>
        /// <param name="episodeId">Perawatan yang ditanyakan.</param>
        /// <param name="doctorId">Dokter yang kewenangannya diperiksa.</param>
        /// <param name="atUtc">Saat yang dipakai memeriksa periode penugasan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<bool> IsDoctorAssignedAsync(
            Guid episodeId,
            Guid doctorId,
            DateTime atUtc,
            CancellationToken cancellationToken = default)
        {
            if (episodeId == Guid.Empty || doctorId == Guid.Empty)
                return false;

            return await _dbContext.Set<InpDoctorAssignment>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EpisodeId == episodeId &&
                    x.DoctorId == doctorId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.StartDateTime <= atUtc &&
                    (x.EndDateTime == null || x.EndDateTime > atUtc),
                    cancellationToken);
        }

        /// <summary>
        /// Menjawab apakah seorang perawat bertugas di unit tempat sebuah perawatan berada -
        /// <c>GUARD-INP-08</c>, <c>BE-RWI-078</c>.
        /// </summary>
        /// <remarks>
        /// Unitnya dibaca dari <c>InpEpisode.ServiceUnitId</c> <b>saat ini</b>, bukan dari
        /// salinan mana pun. Itulah yang membuat kewenangan ikut berpindah begitu pasien
        /// dipindahkan ke bangsal lain - <c>AC-KEP-049</c>.
        /// </remarks>
        /// <param name="episodeId">Perawatan yang ditanyakan.</param>
        /// <param name="nurseEmployeeId">Perawat yang kewenangannya diperiksa.</param>
        /// <param name="atUtc">Saat yang dipakai memeriksa periode penempatan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<bool> IsNurseOnDutyAtEpisodeAsync(
            Guid episodeId,
            Guid nurseEmployeeId,
            DateTime atUtc,
            CancellationToken cancellationToken = default)
        {
            if (episodeId == Guid.Empty || nurseEmployeeId == Guid.Empty)
                return false;

            var serviceUnitId = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => (Guid?)x.ServiceUnitId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!serviceUnitId.HasValue || serviceUnitId.Value == Guid.Empty)
                return false;

            return await IsNurseOnDutyAtUnitAsync(
                serviceUnitId.Value, nurseEmployeeId, atUtc, cancellationToken);
        }

        /// <summary>
        /// Menjawab apakah seorang perawat bertugas di sebuah unit pelayanan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Sumber data "unit tempat perawat bertugas", beserta alasan pemilihannya.</b>
        /// <c>RWI-DEC-100</c> menetapkan gerbangnya tetapi sengaja menyerahkan pemilihan
        /// sumbernya ke task implementasi, karena <c>InpNurseAssignment</c> memang tidak
        /// menyimpannya. Yang dipakai di sini adalah rantai penempatan organisasi milik
        /// kepegawaian:
        /// </para>
        /// <para>
        /// sisi pasien - <c>InpEpisode.ServiceUnitId</c> lalu
        /// <c>MstServiceUnit.OrganizationUnitId</c> lalu
        /// <c>MstOrganizationUnit.DepartmentId</c>;
        /// sisi perawat - <c>MstEmployee.WorkforceProfileId</c> lalu baris
        /// <c>WfpOrganizationAssignment</c> yang periodenya memuat saat itu.
        /// </para>
        /// <para>
        /// <b>Kenapa rantai ini, bukan yang lain.</b> Tiga alasan, dan ketiganya berdasar pada
        /// data yang memang sudah ada. Pertama, <c>WfpOrganizationAssignment</c> sudah
        /// berperiode lewat <c>EffectiveStartDate</c> dan <c>EffectiveEndDate</c>, sehingga
        /// mutasi perawat antarbangsal terbaca dengan sendirinya tanpa tabel baru. Kedua, ia
        /// sudah menjadi sumber penempatan yang dipakai modul kepegawaian lain - kehadiran,
        /// tunjangan, dan roster membacanya - sehingga tidak melahirkan sumber kebenaran kedua.
        /// Ketiga, ia <b>tidak</b> menyalin unit ke baris penugasan pasien, dan justru itulah
        /// yang dilarang <c>permission-audit-matrix.md</c> bagian 3A.5: unit episode berubah saat
        /// pasien dipindahkan, sedangkan salinan tidak.
        /// </para>
        /// <para>
        /// <b>Roster shift sengaja tidak dipakai.</b> <c>TrxShiftAssignment</c> memang menyimpan
        /// unit, tetapi <c>RWI-DEC-100</c> mencatat bahwa rumah sakit ini belum menjalankan
        /// konsep shift perawat sama sekali. Menggantungkan hak tulis pada roster yang belum
        /// terisi berarti menolak seluruh dokumentasi keperawatan pada hari pertama.
        /// </para>
        /// <para>
        /// <b>Dua kelonggaran yang disengaja.</b> Pencocokan jatuh ke departemen ketika unit
        /// pelayanan hanya terpetakan sampai sana, dan jatuh ke
        /// <c>MstEmployee.PrimaryDepartmentId</c> ketika perawat belum punya satu pun baris
        /// penempatan yang berlaku. Keduanya menjaga perawat yang datanya belum lengkap tetap
        /// dapat mendokumentasikan pasien di bangsalnya; keduanya tetap menolak perawat dari
        /// departemen lain.
        /// </para>
        /// <para>
        /// <b>Unit pelayanan yang belum terpetakan ditolak, dan itu disengaja.</b> Bila
        /// <c>MstServiceUnit</c> tidak menyebut organisasi maupun departemen mana pun, tidak ada
        /// yang dapat dibandingkan, dan melewatkannya berarti mengembalikan persis lubang yang
        /// sedang ditutup - siapa pun menulis untuk siapa pun. Pemetaannya karena itu menjadi
        /// syarat data induk sebelum penerapan.
        /// </para>
        /// <para>
        /// Tidak ada nama peran, nama jabatan, maupun <c>UserType</c> yang dibaca di sini. Hak
        /// akses tetap ditentukan admin lewat layar Akses Role; yang diputuskan metode ini hanya
        /// kewenangan yang melekat pada data.
        /// </para>
        /// </remarks>
        /// <param name="serviceUnitId">Unit pelayanan tempat perawatan berada.</param>
        /// <param name="nurseEmployeeId">Perawat yang kewenangannya diperiksa.</param>
        /// <param name="atUtc">Saat yang dipakai memeriksa periode penempatan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<bool> IsNurseOnDutyAtUnitAsync(
            Guid serviceUnitId,
            Guid nurseEmployeeId,
            DateTime atUtc,
            CancellationToken cancellationToken = default)
        {
            if (serviceUnitId == Guid.Empty || nurseEmployeeId == Guid.Empty)
                return false;

            var unitPelayanan = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => x.Id == serviceUnitId && !x.IsDelete)
                .Select(x => new { x.OrganizationUnitId })
                .FirstOrDefaultAsync(cancellationToken);

            if (unitPelayanan == null)
                return false;

            Guid? organisasiTujuan = unitPelayanan.OrganizationUnitId;
            Guid? departemenTujuan = null;

            if (organisasiTujuan.HasValue && organisasiTujuan.Value != Guid.Empty)
            {
                departemenTujuan = await _dbContext.Set<MstOrganizationUnit>()
                    .AsNoTracking()
                    .Where(x => x.Id == organisasiTujuan.Value && !x.IsDelete)
                    .Select(x => x.DepartmentId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var adaTujuan =
                (organisasiTujuan.HasValue && organisasiTujuan.Value != Guid.Empty) ||
                (departemenTujuan.HasValue && departemenTujuan.Value != Guid.Empty);

            if (!adaTujuan)
                return false;

            var pegawai = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x => x.Id == nurseEmployeeId && !x.IsDelete && x.IsActive)
                .Select(x => new { x.WorkforceProfileId, x.PrimaryDepartmentId })
                .FirstOrDefaultAsync(cancellationToken);

            if (pegawai == null)
                return false;

            var penempatan = await _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == pegawai.WorkforceProfileId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.EffectiveStartDate <= atUtc &&
                    (x.EffectiveEndDate == null || x.EffectiveEndDate > atUtc))
                .Select(x => new { x.OrganizationUnitId, x.DepartmentId })
                .ToListAsync(cancellationToken);

            if (organisasiTujuan.HasValue &&
                organisasiTujuan.Value != Guid.Empty &&
                penempatan.Any(x => x.OrganizationUnitId == organisasiTujuan.Value))
            {
                return true;
            }

            if (departemenTujuan.HasValue && departemenTujuan.Value != Guid.Empty)
            {
                if (penempatan.Any(x => x.DepartmentId == departemenTujuan.Value))
                    return true;

                // Perawat yang belum punya satu pun baris penempatan berlaku dinilai dari
                // departemen induknya. Baris penempatan yang ada tidak dilewati begitu saja:
                // perawat yang sudah dimutasi dinilai dari mutasinya, bukan dari kolom lama.
                if (penempatan.Count == 0 && pegawai.PrimaryDepartmentId == departemenTujuan.Value)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Perawatan yang masih berjalan. <c>DischargePending</c> ikut dihitung berjalan: pasien
        /// masih berada di kamar sampai ia benar-benar meninggalkan rumah sakit, dan dokumentasi
        /// pada masa itu tetap sah.
        /// </summary>
        private static bool IsOpen(InpEpisodeStatus status) =>
            status == InpEpisodeStatus.Admitted ||
            status == InpEpisodeStatus.DischargePending;
    }
}
