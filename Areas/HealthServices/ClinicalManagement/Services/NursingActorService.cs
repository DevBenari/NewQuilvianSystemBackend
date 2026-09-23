using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Menemukan baris pegawai yang melekat pada pengguna yang sedang masuk, untuk pencatatan
    /// dokumentasi keperawatan — <c>BE-RWI-059</c>, <c>BE-RWI-061</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa harus pegawai, bukan sekadar pengguna.</b> Rencana asuhan dan catatan tindakan
    /// menyimpan <b>siapa perawatnya</b>, bukan siapa akunnya. Akun dapat dinonaktifkan,
    /// diganti, atau dipakai lewat perangkat bersama; yang harus tetap terbaca pada rekam medis
    /// bertahun-tahun kemudian adalah nama pegawainya. Karena itu kolom pelakunya menunjuk
    /// <c>MstEmployee</c>, sementara jejak akun tetap tersimpan pada <c>CreateBy</c>.
    /// </para>
    /// <para>
    /// <b>Urutan pencariannya bersandar pada data, bukan pada nama peran.</b> Klaim identitas
    /// pegawai lebih dulu, lalu penautan lewat profil tenaga kerja, lalu surel. Tidak satu pun
    /// membaca nama peran, nama departemen, nama jabatan, maupun <c>UserType</c> — hak akses
    /// tetap ditentukan admin lewat layar Akses Role. Urutan yang sama sudah dipakai
    /// <c>NurseStationQueueController.ResolveCurrentEmployeeAsync</c> dan
    /// <c>InpatientDocumentCorrectionAuthorityService.ResolveActorDoctorIdAsync</c>.
    /// </para>
    /// <para>
    /// Tidak memakai interface, mengikuti pola service pada repository ini.
    /// </para>
    /// </remarks>
    public class NursingActorService
    {
        /// <summary>
        /// Kalimat penolakan bagi pengguna yang belum tertaut ke data pegawai mana pun.
        /// </summary>
        /// <remarks>
        /// Penolakan ini <b>bukan</b> soal hak akses melainkan soal kelengkapan data induk:
        /// dokumentasi keperawatan wajib menyebut perawatnya, dan akun tanpa pegawai tidak dapat
        /// menyebut siapa pun. Pesannya karena itu mengarahkan ke perbaikan datanya, bukan ke
        /// permintaan hak akses baru.
        /// </remarks>
        public const string PenolakanTanpaPegawai =
            "Akun Anda belum tertaut ke data pegawai, sehingga catatan keperawatan tidak dapat " +
            "menyebut siapa perawatnya. Hubungi bagian kepegawaian untuk menautkannya.";

        /// <summary>
        /// Kalimat penolakan <c>GUARD-INP-07</c>: permintaan menyebut perawat lain sebagai
        /// penulis atau pelaksana - <c>BE-RWI-078</c>, <c>AC-KEP-047</c>.
        /// </summary>
        /// <remarks>
        /// Penolakan ini soal kewenangan, bukan soal kelengkapan data, sehingga jawabannya
        /// <c>403</c> dan kalimatnya menyebut siapa yang seharusnya menulis.
        /// </remarks>
        public const string PenolakanPegawaiPihakLain =
            "Dokumentasi keperawatan dicatat atas nama perawat yang sedang masuk. Catatan atas " +
            "nama perawat lain tidak dapat disimpan.";

        private readonly ApplicationDbContext _dbContext;

        public NursingActorService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menemukan penanda pegawai milik pengguna yang sedang masuk, atau kosong bila tidak ada.
        /// </summary>
        /// <param name="user">Identitas pengguna yang sedang masuk.</param>
        /// <param name="actorUserId">Id pengguna yang sedang masuk.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<Guid?> ResolveEmployeeIdAsync(
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var employeeIdClaim = user?.FindFirstValue("employee_id") ?? user?.FindFirstValue("EmployeeId");

            if (Guid.TryParse(employeeIdClaim, out var dariKlaimPegawai) && dariKlaimPegawai != Guid.Empty)
            {
                var adaPegawai = await _dbContext.Set<MstEmployee>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == dariKlaimPegawai && !x.IsDelete && x.IsActive, cancellationToken);

                if (adaPegawai)
                    return dariKlaimPegawai;
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
                    .Select(x => new { x.EmployeeId, x.WorkforceProfileId, x.Email })
                    .FirstOrDefaultAsync(cancellationToken);

            // GUARD-INP-07, BE-RWI-078, RWI-DEC-100. Penautan langsung pada baris pengguna
            // dibaca sebelum penautan lewat profil tenaga kerja dan sebelum surel: ia adalah
            // rantai identitas yang disebut kontrak 0.4.0 bagian 0.A.2 secara bernama, dan ia
            // satu-satunya yang tidak bergantung pada kesamaan alamat surel.
            if (pengguna?.EmployeeId is Guid dariPengguna && dariPengguna != Guid.Empty)
            {
                var adaPegawai = await _dbContext.Set<MstEmployee>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == dariPengguna && !x.IsDelete && x.IsActive, cancellationToken);

                if (adaPegawai)
                    return dariPengguna;
            }

            workforceProfileId ??= pengguna?.WorkforceProfileId;

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                var pegawai = await _dbContext.Set<MstEmployee>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId.Value &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (pegawai.HasValue)
                    return pegawai;
            }

            if (!string.IsNullOrWhiteSpace(pengguna?.Email))
            {
                var surel = pengguna.Email.ToLower();

                var pegawai = await _dbContext.Set<MstEmployee>()
                    .AsNoTracking()
                    .Where(x =>
                        x.Email.ToLower() == surel &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (pegawai.HasValue)
                    return pegawai;
            }

            return null;
        }

        /// <summary>
        /// Menjawab apakah sebuah penanda pegawai menunjuk pegawai yang benar-benar ada dan aktif.
        /// </summary>
        /// <remarks>
        /// Dipakai ketika pelaku tindakan berbeda dari pencatatnya — perawat yang mencatatkan
        /// tindakan rekannya. Yang diperiksa keberadaan datanya, bukan nama perannya.
        /// </remarks>
        public async Task<bool> IsActiveEmployeeAsync(
            Guid employeeId,
            CancellationToken cancellationToken = default)
        {
            if (employeeId == Guid.Empty)
                return false;

            return await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == employeeId && !x.IsDelete && x.IsActive, cancellationToken);
        }

        /// <summary>Nama lengkap beberapa pegawai sekaligus, untuk tampilan daftar.</summary>
        public async Task<Dictionary<Guid, string>> GetEmployeeNamesAsync(
            IReadOnlyCollection<Guid> employeeIds,
            CancellationToken cancellationToken = default)
        {
            var ids = employeeIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (ids.Count == 0)
                return [];

            return await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName })
                .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);
        }
    }
}
