using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Penjaga status episode untuk Workspace PPRI — <c>BE-RWI-193</c>, <c>INV-RWA-08</c>,
    /// <c>VAL-RWA-01</c>, <c>02</c>, state matrix 10.6.
    /// </summary>
    /// <remarks>
    /// Ditiru dari <c>NursingEpisodeWriteGuard</c> tanpa pemeriksaan unit perawat, karena dokumen
    /// admisi dikerjakan petugas admisi, CRO, dan perawat dari unit mana pun. Tidak ada nama peran
    /// yang dibaca; yang dijaga di sini hanya kelayakan status episode.
    ///
    /// <para>
    /// Contoh: episode Ny. Wati sudah <c>Closed</c>. Mengubah Nilai Kepercayaannya lewat panggilan
    /// langsung ditolak <c>409 INP-ADM-DOC-001</c>; membaca dan mencetak ulang beralasan tetap boleh.
    /// Episode yang admisinya belum dikonfirmasi (<c>Draft</c>) belum membuka Workspace PPRI sama
    /// sekali, kecuali ringkasan dan kop surat (G-30).
    /// </para>
    /// </remarks>
    public sealed class InpAdmissionWriteGuard
    {
        public const string NotWritableMessage =
            "Episode sudah ditutup atau dibatalkan. Dokumen admisi hanya dapat dibaca dan dicetak ulang.";

        public const string NotAdmittedMessage =
            "Admisi belum dikonfirmasi. Selesaikan alur Admisi Rawat Inap lebih dulu.";

        public const string EpisodeNotFoundMessage = "Episode rawat inap tidak ditemukan.";

        private readonly ApplicationDbContext _dbContext;

        public InpAdmissionWriteGuard(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Episode apa pun statusnya; kosong bila tidak ditemukan.</summary>
        public Task<InpAdmissionEpisodeGate?> LoadAsync(Guid episodeId, CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new InpAdmissionEpisodeGate(x.Id, x.PatientId, x.EncounterId, x.EpisodeStatus))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>Bacaan selain ringkasan dan kop: episode <c>Draft</c> ditolak <c>409 INP-ADM-DOC-002</c>.</summary>
        public async Task<InpAdmissionResult<InpAdmissionEpisodeGate>> EnsureReadableAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gate = await LoadAsync(episodeId, cancellationToken);

            if (gate == null)
            {
                return InpAdmissionResult<InpAdmissionEpisodeGate>.Fail(
                    StatusCodes.Status404NotFound, EpisodeNotFoundMessage);
            }

            if (gate.Status == InpEpisodeStatus.Draft)
            {
                return InpAdmissionResult<InpAdmissionEpisodeGate>.Fail(
                    StatusCodes.Status409Conflict, NotAdmittedMessage, InpAdmissionCodes.EpisodeNotAdmitted);
            }

            return InpAdmissionResult<InpAdmissionEpisodeGate>.Ok(gate, string.Empty);
        }

        /// <summary>Penulisan hanya pada episode <c>Admitted</c> atau <c>DischargePending</c>.</summary>
        public async Task<InpAdmissionResult<InpAdmissionEpisodeGate>> EnsureWritableAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var readable = await EnsureReadableAsync(episodeId, cancellationToken);

            if (!readable.IsSuccess)
            {
                return readable;
            }

            if (!readable.Data!.IsWritable)
            {
                return InpAdmissionResult<InpAdmissionEpisodeGate>.Fail(
                    StatusCodes.Status409Conflict, NotWritableMessage, InpAdmissionCodes.EpisodeNotWritable);
            }

            return readable;
        }
    }

    /// <summary>Keadaan episode yang menentukan apa yang boleh dilakukan di Workspace PPRI.</summary>
    public sealed record InpAdmissionEpisodeGate(
        Guid EpisodeId,
        Guid PatientId,
        Guid EncounterId,
        InpEpisodeStatus Status)
    {
        /// <summary>Episode berjalan: <c>Admitted</c> atau <c>DischargePending</c>.</summary>
        public bool IsWritable => Status is InpEpisodeStatus.Admitted or InpEpisodeStatus.DischargePending;

        /// <summary>Episode selesai: setiap cetak wajib beralasan (<c>RWI-DEC-240</c> butir 7).</summary>
        public bool IsClosedOrCancelled => Status is InpEpisodeStatus.Closed or InpEpisodeStatus.Cancelled;
    }
}
