using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

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
        EpisodeMismatch = 7
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
                    x.EpisodeStatus
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

            return InpatientClinicalContextResult.Ok(new InpatientClinicalContext
            {
                EpisodeId = episode.Id,
                EpisodeNumber = episode.EpisodeNumber,
                EncounterId = episode.EncounterId,
                PatientId = episode.PatientId,
                ServiceUnitId = episode.ServiceUnitId,
                EpisodeStatus = episode.EpisodeStatus,
                IsEpisodeOpen = isOpen,
                AttendingDoctorId = attendingDoctorId,
                IsDoctorAuthorized = isDoctorAuthorized
            });
        }

        /// <summary>
        /// Menemukan DPJP yang penugasannya berlaku pada saat tertentu.
        /// </summary>
        /// <remarks>
        /// Penugasan bersifat berperiode. Yang dicari adalah penugasan yang periodenya memuat
        /// saat itu, bukan penugasan terkini — catatan yang ditulis untuk pemeriksaan kemarin
        /// dinilai dengan DPJP yang berwenang kemarin.
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
        /// Perawatan yang masih berjalan. <c>DischargePending</c> ikut dihitung berjalan: pasien
        /// masih berada di kamar sampai ia benar-benar meninggalkan rumah sakit, dan dokumentasi
        /// pada masa itu tetap sah.
        /// </summary>
        private static bool IsOpen(InpEpisodeStatus status) =>
            status == InpEpisodeStatus.Admitted ||
            status == InpEpisodeStatus.DischargePending;
    }
}
