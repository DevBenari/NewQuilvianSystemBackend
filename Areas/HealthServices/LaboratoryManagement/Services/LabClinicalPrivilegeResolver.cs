using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Lapis orang kewenangan validasi dan rilis (<c>LAB-DEC-142</c>, <c>INT-07</c>): menjawab
    /// <i>"apakah orang ini ditunjuk memvalidasi atau merilis hasil disiplin ini pada hari
    /// tindakan, dan bila tidak, kenapa"</i> dari kredensial Human Resource.
    ///
    /// <para>
    /// <b>Fail-closed, tanpa jalur pintas</b> (A5.10). Akun tanpa data tenaga kerja, nol baris
    /// berkode sesuai, atau baris yang tidak berlaku → <b>ditolak</b>. Pembacaan yang gagal →
    /// <see cref="LabPrivilegeReadException"/>, yang dipetakan <c>503</c> oleh pemanggil, sehingga
    /// tindakannya tidak dilakukan.
    /// </para>
    ///
    /// <para>
    /// <b>Sengaja berbeda dari <c>OperatingRoomCredentialResolver</c>.</b> Pola membacanya
    /// ditiru — <c>WfpClinicalPrivilege</c>, <c>AsNoTracking</c>, status, masa berlaku — tetapi
    /// cara memutuskannya tidak. Resolver Kamar Operasi tidak mencocokkan kode dan melaporkan
    /// tenaga tanpa data sebagai <c>NotAvailable</c> yang <b>tidak memblokir</b>. Di sini kode
    /// wajib cocok (<c>LAB-DEC-148</c>) dan data kosong wajib menolak.
    /// </para>
    ///
    /// <para>
    /// <b>Baca saja, tanpa salinan dan tanpa cache.</b> Nol <c>Add</c>/<c>Update</c>/<c>Remove</c>
    /// atas entity Human Resource (<c>AC-232</c>). Penunjukan yang ditangguhkan pukul 10.00
    /// menolak validasi pukul 10.01.
    /// </para>
    /// </summary>
    public class LabClinicalPrivilegeResolver
    {
        /// <summary>Resource hak akses tempat aksi <c>Validate</c> dan <c>Release</c> tinggal.</summary>
        private const string ResultResourceName = "LabExaminationResult";

        private static readonly string[] NamaBulan =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };

        private readonly ApplicationDbContext _dbContext;

        public LabClinicalPrivilegeResolver(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menilai penunjukan pengguna untuk satu disiplin dan satu jenis tindakan pada saat
        /// <paramref name="at"/>.
        ///
        /// <para>
        /// <b>Aturan menilai baris</b> (<c>02-backend-architecture.md</c> 20.4, 20.10 butir 8):
        /// hanya baris berkode sama persis, <c>IsDelete = false</c>, dan <c>IsActive = true</c>.
        /// Dari baris itu, hanya yang masa berlakunya mencakup <b>hari tindakan menurut zona waktu
        /// rumah sakit</b> yang dinilai, dengan tanggal akhir <b>inklusif</b>. Satu saja yang
        /// <c>Revoked</c>, <c>Suspended</c>, atau <c>IsClinicalServiceBlocked</c> menolak; bila
        /// tidak ada, satu yang <c>Active</c> menerima.
        /// </para>
        ///
        /// <para>
        /// <b>Kenapa tanggal, bukan jam.</b> Human Resource menyimpan tanggal mulai dan akhir
        /// sebagai tanggal kalender berlabel tengah malam UTC
        /// (<c>WfpClinicalPrivilegeController.NormalizeUtcDate</c>). Penunjukan yang berakhir
        /// 30 September tersimpan <c>2026-09-30T00:00Z</c>; membandingkannya dengan jam tindakan
        /// akan menutupnya pukul 07.00 WIB tanggal 30 — tujuh belas jam terlalu cepat. Yang
        /// dibandingkan karena itu tanggal kalender tersimpan dengan tanggal WIB saat bertindak.
        /// </para>
        /// </summary>
        /// <param name="userId">Pengguna yang menekan tindakan.</param>
        /// <param name="discipline">Disiplin order pemeriksaan — bukan jabatan pelaku.</param>
        /// <param name="kind">Validasi atau rilis.</param>
        /// <param name="at">
        /// Saat tindakan. Nilai tanpa zona dibaca sebagai jam dinding WIB
        /// (<see cref="AppDateTimeHelper.ToUtc"/>).
        /// </param>
        /// <param name="cancellationToken">Pembatalan permintaan; tidak pernah dibungkus menjadi galat pembacaan.</param>
        /// <exception cref="LabPrivilegeReadException">Data kewenangan tidak dapat dibaca.</exception>
        public async Task<LabPrivilegeCheck> ResolveAsync(
            Guid userId,
            LabDiscipline discipline,
            LabPrivilegeKind kind,
            DateTime at,
            CancellationToken cancellationToken = default)
        {
            var hari = HariRumahSakit(at);
            var disiplin = LabelDisiplin(discipline);

            Guid? workforceProfileId;
            try
            {
                workforceProfileId = await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId)
                    .Select(x => x.WorkforceProfileId)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new LabPrivilegeReadException(ex);
            }

            if (workforceProfileId is null || workforceProfileId == Guid.Empty)
            {
                return Tolak(LabPrivilegeDenial.NoWorkforceProfile, null, kind, disiplin);
            }

            var code = LabClinicalPrivilegeCodes.For(discipline, kind);
            if (code is null)
            {
                return Tolak(LabPrivilegeDenial.NotAppointed, null, kind, disiplin);
            }

            List<BarisPenunjukan> rows;
            try
            {
                rows = await _dbContext.WfpClinicalPrivileges
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId.Value &&
                        x.PrivilegeCode == code &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => new BarisPenunjukan(
                        x.Id,
                        x.PrivilegeStatus,
                        x.EffectiveStartDate,
                        x.EffectiveEndDate,
                        x.IsClinicalServiceBlocked))
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new LabPrivilegeReadException(ex);
            }

            if (rows.Count == 0)
            {
                return Tolak(LabPrivilegeDenial.NotAppointed, null, kind, disiplin);
            }

            var berlaku = rows
                .Where(x =>
                    x.EffectiveStartDate.Date <= hari &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= hari))
                .ToList();

            if (berlaku.Count == 0)
            {
                // Tidak ada baris yang berlaku hari itu. Penunjukan yang akan datang lebih berguna
                // disebut daripada yang sudah lewat: pengguna tahu kapan ia dapat bertindak.
                var akanBerlaku = rows
                    .Where(x => x.EffectiveStartDate.Date > hari)
                    .OrderBy(x => x.EffectiveStartDate)
                    .FirstOrDefault();

                if (akanBerlaku is not null)
                {
                    return Tolak(
                        LabPrivilegeDenial.NotYetEffective,
                        akanBerlaku.EffectiveStartDate.Date,
                        kind,
                        disiplin);
                }

                var terakhirBerakhir = rows
                    .Where(x => x.EffectiveEndDate.HasValue)
                    .Max(x => x.EffectiveEndDate!.Value.Date);

                return Tolak(LabPrivilegeDenial.Expired, terakhirBerakhir, kind, disiplin);
            }

            // Satu baris yang buruk menolak, walau ada baris Active lain yang berlaku. Urutan
            // sebabnya dari yang paling berat.
            if (berlaku.Any(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Revoked))
            {
                return Tolak(LabPrivilegeDenial.Revoked, null, kind, disiplin);
            }

            if (berlaku.Any(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Suspended))
            {
                return Tolak(LabPrivilegeDenial.Suspended, null, kind, disiplin);
            }

            if (berlaku.Any(x => x.IsClinicalServiceBlocked))
            {
                return Tolak(LabPrivilegeDenial.ClinicalServiceBlocked, null, kind, disiplin);
            }

            var aktif = berlaku
                .Where(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Active)
                .OrderByDescending(x => x.EffectiveStartDate)
                .ThenBy(x => x.Id)
                .FirstOrDefault();

            if (aktif is not null)
            {
                return LabPrivilegeCheck.Granted(aktif.Id);
            }

            if (berlaku.Any(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Expired))
            {
                // Human Resource menandai kedaluwarsa walau tanggal akhirnya belum lewat. Tanggal
                // akhir tersimpan tidak disebut, sebab pesannya akan menyebut tanggal yang belum tiba.
                return Tolak(LabPrivilegeDenial.Expired, null, kind, disiplin);
            }

            if (berlaku.Any(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Pending))
            {
                return Tolak(LabPrivilegeDenial.PendingApproval, null, kind, disiplin);
            }

            // Sisanya berstatus NotApplicable — baris itu tidak memberi kewenangan apa pun.
            return Tolak(LabPrivilegeDenial.NotAppointed, null, kind, disiplin);
        }

        /// <summary>
        /// Penempatan jabatan yang memberi pengguna aksi <c>Validate</c> atau <c>Release</c>, untuk
        /// snapshot peran pada hasil (<c>ValidatedByPositionId</c>, <c>ReleasedByPositionId</c>).
        ///
        /// <para>
        /// Syarat kelayakan penempatan <b>disalin</b> dari
        /// <c>AccessPermissionService.HasAccessAsync</c>: aktif, tidak terhapus, tidak batal, dan
        /// dalam masa berlaku terhadap jam UTC saat ini. <b>Risiko penyimpangan:</b> bila platform
        /// mengubah syarat itu, snapshot dapat menunjuk penempatan yang berbeda dari yang
        /// benar-benar memberi izin — <c>AC-239</c> mengujinya.
        /// </para>
        ///
        /// <para>
        /// Bila lebih dari satu penempatan memegang aksi itu, penempatan utama didahulukan, lalu
        /// nama jabatan secara abjad. Penempatan utama yang <b>tidak</b> memegang aksinya tidak
        /// pernah terpilih.
        /// </para>
        /// </summary>
        /// <returns>
        /// <c>null</c> bila tidak ada penempatan yang memegang aksinya — misalnya superadmin, yang
        /// melewati pemeriksaan hak akses tanpa penempatan apa pun.
        /// </returns>
        /// <exception cref="LabPrivilegeReadException">Data penempatan tidak dapat dibaca.</exception>
        public async Task<LabActingPosition?> ResolveActingPositionAsync(
            Guid userId,
            LabPrivilegeKind kind,
            CancellationToken cancellationToken = default)
        {
            var actionName = kind == LabPrivilegeKind.Release ? "Release" : "Validate";

            try
            {
                var actionAccess = await _dbContext.SysActionAccesses
                    .AsNoTracking()
                    .Where(x =>
                        x.ActionName == actionName &&
                        x.IsActive &&
                        !x.IsDelete &&
                        !x.IsSystemOnly &&
                        x.ControllerAccess != null &&
                        x.ControllerAccess.ControllerName == ResultResourceName &&
                        x.ControllerAccess.IsActive &&
                        !x.ControllerAccess.IsDelete &&
                        !x.ControllerAccess.IsSystemOnly)
                    .Select(x => new
                    {
                        ActionAccessId = x.Id,
                        x.ControllerAccessId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (actionAccess is null)
                {
                    return null;
                }

                var now = DateTime.UtcNow;

                return await (
                    from organization in _dbContext.ApplicationUserOrganizations.AsNoTracking()
                    join policy in _dbContext.SysAccessPolicies.AsNoTracking()
                        on new { organization.DepartmentId, organization.PositionId }
                        equals new { policy.DepartmentId, policy.PositionId }
                    where organization.UserId == userId
                          && organization.IsActive
                          && !organization.IsDelete
                          && !organization.IsCancel
                          && (!organization.EffectiveStartDate.HasValue ||
                              organization.EffectiveStartDate.Value <= now)
                          && (!organization.EffectiveEndDate.HasValue ||
                              organization.EffectiveEndDate.Value >= now)
                          && policy.ControllerAccessId == actionAccess.ControllerAccessId
                          && policy.ActionAccessId == actionAccess.ActionAccessId
                          && policy.IsAllowed
                          && policy.IsActive
                          && !policy.IsDelete
                    orderby organization.IsPrimary descending,
                        organization.Position!.PositionName,
                        organization.PositionId
                    select new LabActingPosition(
                        organization.DepartmentId,
                        organization.PositionId,
                        organization.Position!.PositionName)
                ).FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new LabPrivilegeReadException(ex);
            }
        }

        /// <summary>
        /// Tanggal kalender WIB dari saat tindakan. Dihitung dari batas tengah malam WIB yang
        /// dipakai seluruh aplikasi (<see cref="AppDateTimeHelper.OperationalDateToUtc"/>),
        /// sehingga zona waktunya tidak ditulis ulang di sini.
        /// </summary>
        private static DateTime HariRumahSakit(DateTime at)
        {
            var atUtc = AppDateTimeHelper.ToUtc(at);
            var hari = atUtc.Date;

            if (AppDateTimeHelper.OperationalDateToUtc(hari.AddDays(1)) <= atUtc)
            {
                return hari.AddDays(1);
            }

            if (AppDateTimeHelper.OperationalDateToUtc(hari) > atUtc)
            {
                return hari.AddDays(-1);
            }

            return hari;
        }

        private static LabPrivilegeCheck Tolak(
            LabPrivilegeDenial denial,
            DateTime? tanggal,
            LabPrivilegeKind kind,
            string disiplin) =>
            LabPrivilegeCheck.Denied(denial, tanggal, BuatPesan(denial, tanggal, kind, disiplin));

        /// <summary>
        /// Pesan <c>VAL-128</c> per sebab — <c>LAB-VAL-v1</c> <c>r12</c> 14.2. Kata
        /// <i>validasi</i> diganti <i>rilis</i> pada tindakan rilis.
        /// </summary>
        private static string BuatPesan(
            LabPrivilegeDenial denial,
            DateTime? tanggal,
            LabPrivilegeKind kind,
            string disiplin)
        {
            var tindakan = kind == LabPrivilegeKind.Release ? "rilis" : "validasi";

            return denial switch
            {
                LabPrivilegeDenial.NoWorkforceProfile =>
                    $"Akun Anda belum terhubung dengan data tenaga kerja, sehingga kewenangan {tindakan} tidak dapat diperiksa. Hubungi bagian SDM.",
                LabPrivilegeDenial.NotAppointed =>
                    $"Anda belum ditunjuk sebagai pemegang kewenangan {tindakan} {disiplin}.",
                LabPrivilegeDenial.PendingApproval =>
                    $"Penunjukan {tindakan} {disiplin} Anda masih menunggu persetujuan.",
                LabPrivilegeDenial.NotYetEffective => tanggal.HasValue
                    ? $"Penunjukan {tindakan} {disiplin} Anda baru berlaku mulai {TulisTanggal(tanggal.Value)}."
                    : $"Penunjukan {tindakan} {disiplin} Anda belum berlaku.",
                LabPrivilegeDenial.Expired => tanggal.HasValue
                    ? $"Masa berlaku penunjukan {tindakan} {disiplin} Anda sudah habis pada {TulisTanggal(tanggal.Value)}."
                    : $"Masa berlaku penunjukan {tindakan} {disiplin} Anda sudah habis.",
                LabPrivilegeDenial.Suspended =>
                    $"Penunjukan {tindakan} {disiplin} Anda sedang ditangguhkan.",
                LabPrivilegeDenial.Revoked =>
                    $"Penunjukan {tindakan} {disiplin} Anda sudah dicabut.",
                LabPrivilegeDenial.ClinicalServiceBlocked =>
                    "Layanan klinis Anda sedang diblokir pada data kredensial.",
                _ => $"Anda belum ditunjuk sebagai pemegang kewenangan {tindakan} {disiplin}."
            };
        }

        /// <summary>
        /// "30 September 2026". Nama bulan ditulis sendiri supaya tidak bergantung pada data
        /// budaya <c>id-ID</c> di mesin server.
        /// </summary>
        private static string TulisTanggal(DateTime tanggal) =>
            $"{tanggal.Day} {NamaBulan[tanggal.Month - 1]} {tanggal.Year}";

        private static string LabelDisiplin(LabDiscipline discipline) => discipline switch
        {
            LabDiscipline.ClinicalPathology => "Patologi Klinik",
            LabDiscipline.AnatomicalPathology => "Patologi Anatomi",
            LabDiscipline.Microbiology => "Mikrobiologi",
            _ => discipline.ToString()
        };

        private sealed record BarisPenunjukan(
            Guid Id,
            ClinicalPrivilegeStatus PrivilegeStatus,
            DateTime EffectiveStartDate,
            DateTime? EffectiveEndDate,
            bool IsClinicalServiceBlocked);
    }

    /// <summary>
    /// Hasil <see cref="LabClinicalPrivilegeResolver.ResolveAsync"/>: diterima beserta
    /// <see cref="PrivilegeId"/> sebagai dasar kewenangan (<c>ValidatedByPrivilegeId</c>,
    /// <c>ReleasedByPrivilegeId</c>), atau ditolak beserta <b>satu</b> sebab dan pesannya
    /// (<c>AC-233</c>).
    /// </summary>
    public sealed record LabPrivilegeCheck(
        bool IsGranted,
        Guid? PrivilegeId,
        LabPrivilegeDenial? Denial,
        DateTime? DenialDate,
        string? Message)
    {
        public static LabPrivilegeCheck Granted(Guid privilegeId) =>
            new(true, privilegeId, null, null, null);

        public static LabPrivilegeCheck Denied(LabPrivilegeDenial denial, DateTime? date, string message) =>
            new(false, null, denial, date, message);
    }

    /// <summary>
    /// Penempatan yang memberi aksi — sumber snapshot peran pelaku (<c>AC-239</c>).
    /// </summary>
    public sealed record LabActingPosition(Guid DepartmentId, Guid PositionId, string PositionName);

    /// <summary>
    /// Data kewenangan atau penempatan tidak dapat dibaca. Pemanggil memetakannya menjadi
    /// <c>503</c> dan <b>tidak</b> melakukan tindakannya — pembacaan yang gagal tidak pernah
    /// dianggap sebagai izin.
    /// </summary>
    public sealed class LabPrivilegeReadException(Exception inner)
        : Exception("Data kewenangan klinis tidak dapat dibaca saat ini. Tindakan tidak dilakukan; coba lagi.", inner);
}
