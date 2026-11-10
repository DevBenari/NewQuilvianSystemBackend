using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Kewenangan profesi untuk dokumen disiplin rehab (<c>BE-RHB-006</c>, RHB-INV-08,
    /// <c>RHB-VAL-031</c>): penulis asesmen, program, dan sesi suatu disiplin harus memegang profesi
    /// klinis yang dipetakan admin ke disiplin itu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Sumber profesi adalah relasi akun</b>, mengikuti
    /// <c>InpatientClinicalContextService.ResolveCpptAuthorProfessionAsync</c>: baris dokter,
    /// pegawai, dan pengguna eksternal yang terhubung ke akun langsung atau lewat profil tenaga
    /// kerja. Isian request tidak pernah dipakai.
    /// </para>
    /// <para>
    /// <b>Pencocokan memakai <c>MstProfession.Id</c></b>, bukan nama atau kode profesi. Berbeda dengan
    /// layanan CPPT yang hanya mengambil satu profesi menurut urutan, di sini seluruh profesi klinis
    /// aktif milik akun dikumpulkan, karena satu profil dapat punya baris dokter sekaligus pegawai.
    /// </para>
    /// <para>
    /// Tidak ada nama peran, departemen, atau jabatan yang dibandingkan. Hak akses endpoint tetap
    /// ditentukan admin di Akses Role; kelas ini hanya memeriksa kewenangan yang melekat pada data.
    /// </para>
    /// </remarks>
    public class RhbProfessionAuthority
    {
        private readonly ApplicationDbContext _dbContext;

        public RhbProfessionAuthority(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Seluruh profesi klinis aktif milik akun yang sedang masuk. Kosong bila akun tidak aktif
        /// atau tidak terhubung ke tenaga kerja berprofesi klinis.
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> ResolveProfessionIdsAsync(
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (actorUserId == Guid.Empty)
                return Array.Empty<Guid>();

            var account = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == actorUserId && x.IsActive)
                .Select(x => new { x.DoctorId, x.EmployeeId, x.ExternalUserId, x.WorkforceProfileId })
                .FirstOrDefaultAsync(cancellationToken);

            if (account == null)
                return Array.Empty<Guid>();

            var clinicalProfessions = _dbContext.Set<MstProfession>()
                .AsNoTracking()
                .Where(p => !p.IsDelete && p.IsActive && p.IsClinicalProfession)
                .Select(p => p.Id);

            var fromDoctor = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(d =>
                    !d.IsDelete && d.IsActive &&
                    ((account.DoctorId.HasValue && d.Id == account.DoctorId.Value) ||
                     (account.WorkforceProfileId.HasValue && d.WorkforceProfileId == account.WorkforceProfileId.Value)) &&
                    clinicalProfessions.Contains(d.ProfessionId))
                .Select(d => d.ProfessionId)
                .ToListAsync(cancellationToken);

            var fromEmployee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(e =>
                    !e.IsDelete && e.IsActive &&
                    ((account.EmployeeId.HasValue && e.Id == account.EmployeeId.Value) ||
                     (account.WorkforceProfileId.HasValue && e.WorkforceProfileId == account.WorkforceProfileId.Value)) &&
                    e.ProfessionId.HasValue &&
                    clinicalProfessions.Contains(e.ProfessionId.Value))
                .Select(e => e.ProfessionId!.Value)
                .ToListAsync(cancellationToken);

            var fromExternal = await _dbContext.Set<MstExternalUser>()
                .AsNoTracking()
                .Where(u =>
                    !u.IsDelete && u.IsActive &&
                    ((account.ExternalUserId.HasValue && u.Id == account.ExternalUserId.Value) ||
                     (account.WorkforceProfileId.HasValue && u.WorkforceProfileId == account.WorkforceProfileId.Value)) &&
                    u.ProfessionId.HasValue &&
                    clinicalProfessions.Contains(u.ProfessionId.Value))
                .Select(u => u.ProfessionId!.Value)
                .ToListAsync(cancellationToken);

            return fromDoctor.Concat(fromEmployee).Concat(fromExternal).Distinct().ToList();
        }

        /// <summary>
        /// <c>true</c> bila akun memegang profesi yang dipetakan ke disiplin aktif itu. Disiplin
        /// yang tidak aktif atau belum punya profesi selalu <c>false</c>.
        /// </summary>
        public async Task<bool> CanActForDisciplineAsync(
            Guid actorUserId,
            Guid disciplineId,
            CancellationToken cancellationToken)
        {
            var disciplineProfessionId = await _dbContext.RhbDisciplines
                .AsNoTracking()
                .Where(x => x.Id == disciplineId && !x.IsDelete && x.IsActive)
                .Select(x => x.ProfessionId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!disciplineProfessionId.HasValue)
                return false;

            var professions = await ResolveProfessionIdsAsync(actorUserId, cancellationToken);
            return professions.Contains(disciplineProfessionId.Value);
        }
    }
}
