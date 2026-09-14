using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services
{
    /// <summary>
    /// Adapter baca status kunjungan milik modul hulu. <c>BD-DOM-16</c>, ditetapkan
    /// <c>DEC-BD-014</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Menjawab tepat satu pertanyaan: apakah kunjungan ini sudah berakhir.</b> Kelas ini
    /// <b>tidak pernah</b> menulis satu baris pun ke Registration maupun InPatient — Bank Darah
    /// membaca lifecycle dari modul pemiliknya, tidak ikut mengaturnya.
    /// </para>
    ///
    /// <para>
    /// <b>Dua penyesuai berbeda, karena dua jenis kunjungan memang berakhir dengan cara yang
    /// berbeda</b> (<c>DEC-BD-014</c>, <c>CONF-BD-004</c>):
    /// </para>
    /// <list type="number">
    /// <item>
    /// <b>Rawat Jalan dan IGD</b> memakai status akhir kunjungan: <c>Completed</c>,
    /// <c>Cancelled</c>, atau <c>NoShow</c>.
    /// </item>
    /// <item>
    /// <b>Rawat Inap</b> memakai <c>InpEpisode.PhysicallyLeftAt</c> — waktu pasien
    /// <b>benar-benar meninggalkan rumah sakit</b> — dan <b>bukan</b> penutupan administratif
    /// episode (<c>InpEpisode.ClosedAt</c>).
    /// </item>
    /// </list>
    ///
    /// <para>
    /// <b>Kenapa perbedaan kedua itu penting, dengan angka.</b> Keputusan pulang turun Senin
    /// pagi, pasien benar-benar pulang Senin siang, episodenya baru ditutup administratif Rabu.
    /// Order Bank Darah berhenti aktif <b>Senin siang</b>, bukan Rabu (<c>AC-BD-017</c>).
    /// Memakai <c>ClosedAt</c> akan membuat order menahan order baru selama dua hari setelah
    /// pasiennya tidak ada lagi di rumah sakit — dan penahanan itu jatuh pada pasien berikutnya
    /// yang benar-benar membutuhkan darah.
    /// </para>
    ///
    /// <para>
    /// <b>Tidak membuka transaksi.</b> Seluruh method di sini murni membaca
    /// (<c>02-backend-architecture.md</c> §F.4).
    /// </para>
    /// </remarks>
    public class BbkEncounterStatusReader
    {
        /// <summary>
        /// Ketiga status yang menyatakan kunjungan Rawat Jalan atau IGD sudah berakhir
        /// (<c>DEC-BD-014</c>).
        /// </summary>
        private static readonly EncounterStatus[] ClosedEncounterStatuses =
        {
            EncounterStatus.Completed,
            EncounterStatus.Cancelled,
            EncounterStatus.NoShow
        };

        private readonly ApplicationDbContext _dbContext;

        public BbkEncounterStatusReader(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menyatakan apakah satu kunjungan sudah berakhir menurut sumber lifecycle jenisnya
        /// masing-masing.
        /// </summary>
        /// <remarks>
        /// Kunjungan yang tidak ditemukan dijawab <c>true</c> — <b>dianggap berakhir</b>.
        /// Pilihan ini sengaja berat sebelah ke arah aman: order yang menggantung pada
        /// kunjungan yang tidak dapat dibaca tidak boleh terus menahan order baru bagi pasien
        /// yang sama.
        /// </remarks>
        public async Task<bool> IsEncounterClosedAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var state = await ReadAsync(encounterId, cancellationToken);

            return state.IsClosed;
        }

        /// <summary>
        /// Membaca keadaan satu kunjungan beserta waktu dan sumber sinyal berakhirnya.
        /// </summary>
        public async Task<EncounterClosureState> ReadAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == encounterId && !x.IsDelete)
                .Select(x => new { x.Id, x.EncounterStatus, x.EncounterType })
                .FirstOrDefaultAsync(cancellationToken);

            if (encounter == null)
            {
                return new EncounterClosureState(
                    Exists: false,
                    IsClosed: true,
                    ClosedAt: null,
                    Signal: EncounterClosureSignal.EncounterNotFound);
            }

            // Rawat Inap: waktu pasien benar-benar pulang, bukan penutupan administratif.
            if (encounter.EncounterType == EncounterType.Inpatient)
            {
                var physicallyLeftAt = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                    .OrderByDescending(x => x.CreateDateTime)
                    .Select(x => x.PhysicallyLeftAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (physicallyLeftAt.HasValue)
                {
                    return new EncounterClosureState(
                        Exists: true,
                        IsClosed: true,
                        ClosedAt: physicallyLeftAt,
                        Signal: EncounterClosureSignal.InpatientPhysicallyLeft);
                }

                // Episode belum menyatakan pasien pulang. Status kunjungan yang sudah berakhir
                // tetap dihormati, supaya kunjungan yang dibatalkan sebelum pasien masuk tidak
                // menggantung selamanya.
                return ClosedEncounterStatuses.Contains(encounter.EncounterStatus)
                    ? new EncounterClosureState(
                        Exists: true,
                        IsClosed: true,
                        ClosedAt: null,
                        Signal: EncounterClosureSignal.EncounterStatusClosed)
                    : new EncounterClosureState(
                        Exists: true,
                        IsClosed: false,
                        ClosedAt: null,
                        Signal: EncounterClosureSignal.Open);
            }

            // Rawat Jalan, IGD, dan jenis lain: status akhir kunjungan.
            return ClosedEncounterStatuses.Contains(encounter.EncounterStatus)
                ? new EncounterClosureState(
                    Exists: true,
                    IsClosed: true,
                    ClosedAt: null,
                    Signal: EncounterClosureSignal.EncounterStatusClosed)
                : new EncounterClosureState(
                    Exists: true,
                    IsClosed: false,
                    ClosedAt: null,
                    Signal: EncounterClosureSignal.Open);
        }
    }

    /// <summary>Keadaan satu kunjungan sebagaimana dibaca Bank Darah.</summary>
    /// <param name="Exists">Kunjungannya ditemukan.</param>
    /// <param name="IsClosed">Kunjungannya sudah berakhir menurut sumber lifecycle jenisnya.</param>
    /// <param name="ClosedAt">
    /// Waktu berakhirnya bila sumbernya memang menyimpan waktu — hanya terisi pada Rawat Inap,
    /// karena status kunjungan Rawat Jalan tidak membawa stempel waktu tersendiri.
    /// </param>
    /// <param name="Signal">Sinyal mana yang menyatakan kunjungan berakhir.</param>
    public sealed record EncounterClosureState(
        bool Exists,
        bool IsClosed,
        DateTime? ClosedAt,
        EncounterClosureSignal Signal);

    /// <summary>Sumber sinyal berakhirnya kunjungan (<c>DEC-BD-014</c>).</summary>
    public enum EncounterClosureSignal
    {
        /// <summary>Kunjungan masih berjalan.</summary>
        Open = 0,

        /// <summary>Status kunjungan sudah <c>Completed</c>, <c>Cancelled</c>, atau <c>NoShow</c>.</summary>
        EncounterStatusClosed = 1,

        /// <summary>Pasien Rawat Inap benar-benar sudah meninggalkan rumah sakit.</summary>
        InpatientPhysicallyLeft = 2,

        /// <summary>Kunjungannya tidak ditemukan; diperlakukan sebagai berakhir.</summary>
        EncounterNotFound = 3
    }
}
