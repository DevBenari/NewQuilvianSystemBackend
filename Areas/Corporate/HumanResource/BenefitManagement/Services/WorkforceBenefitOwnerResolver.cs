using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Satu-satunya tempat yang menerjemahkan identitas SDM menjadi pemilik benefit. Enrollment, kelayakan,
    /// registrasi, Billing, dan Finance memakai hasil yang sama sehingga tidak ada modul yang mencocokkan
    /// identitas dengan aturannya sendiri.
    ///
    /// Prinsip: <c>MstWorkforceProfile.Id</c> adalah identitas pemilik; <c>EmployeeId</c> dan <c>DoctorId</c> adalah
    /// identitas peran. Resolver tidak pernah menaruh DoctorId di EmployeeId, tidak membuat data karyawan atau
    /// pasien, dan hanya membaca HR (tanpa SaveChanges). Satu panggilan "banyak" memakai tiga kueri tetap,
    /// bukan satu kueri per orang.
    /// </summary>
    public sealed class WorkforceBenefitOwnerResolver
    {
        private readonly ApplicationDbContext _dbContext;

        public WorkforceBenefitOwnerResolver(ApplicationDbContext dbContext) => _dbContext = dbContext;

        // ------------------------------------------------------------------------------------
        // Resolusi tunggal
        // ------------------------------------------------------------------------------------

        /// <summary>Identitas utama. Profil harus ada, tidak dihapus, aktif, dan berjenis yang diizinkan kebijakan.</summary>
        public async Task<WorkforceOwnerResolution> ResolveByWorkforceProfileAsync(
            Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (workforceProfileId == Guid.Empty) return NotFound();
            var all = await LoadAsync(new[] { workforceProfileId }, legacyEmployeeCompat: false, cancellationToken);
            return RequireActive(all.GetValueOrDefault(workforceProfileId) ?? NotFound());
        }

        /// <summary>
        /// Adapter kompatibilitas untuk kontrak lama berbasis EmployeeId. Baris MstEmployee tetap dianggap peran
        /// karyawan seperti sebelumnya, terlepas dari jenis SDM pada profilnya.
        /// </summary>
        public async Task<WorkforceOwnerResolution> ResolveByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken)
        {
            if (employeeId == Guid.Empty) return NotFound();
            var profileId = await _dbContext.MstEmployees.AsNoTracking()
                .Where(x => x.Id == employeeId && !x.IsDelete && !x.IsCancel)
                .Select(x => (Guid?)x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);
            if (!profileId.HasValue) return NotFound();

            var all = await LoadAsync(new[] { profileId.Value }, legacyEmployeeCompat: true, cancellationToken);
            return RequireActive(all.GetValueOrDefault(profileId.Value) ?? NotFound());
        }

        /// <summary>Pencarian melalui DoctorId (peran dokter) menuju pemilik yang sama dengan jalur Workforce.</summary>
        public async Task<WorkforceOwnerResolution> ResolveByDoctorAsync(Guid doctorId, CancellationToken cancellationToken)
        {
            if (doctorId == Guid.Empty) return NotFound();
            var profileId = await _dbContext.MstDoctors.AsNoTracking()
                .Where(x => x.Id == doctorId && !x.IsDelete && !x.IsCancel)
                .Select(x => (Guid?)x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);
            if (!profileId.HasValue) return NotFound();
            return await ResolveByWorkforceProfileAsync(profileId.Value, cancellationToken);
        }

        /// <summary>
        /// Memastikan EmployeeId dan WorkforceProfileId dalam satu permintaan menunjuk orang yang sama, lalu
        /// meresolusi. Bila hanya EmployeeId yang dikirim (kontrak lama), EmployeeId diterjemahkan ke Workforce.
        /// </summary>
        public async Task<WorkforceOwnerResolution> ResolveRequestAsync(
            Guid? workforceProfileId, Guid? employeeId, CancellationToken cancellationToken)
        {
            var hasProfile = workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty;
            var hasEmployee = employeeId.HasValue && employeeId.Value != Guid.Empty;
            if (!hasProfile && !hasEmployee) return NotFound();

            if (hasProfile)
            {
                var byProfile = hasEmployee
                    ? await ResolveWithEmployeeCompatAsync(workforceProfileId!.Value, cancellationToken)
                    : await ResolveByWorkforceProfileAsync(workforceProfileId!.Value, cancellationToken);
                if (!byProfile.IsResolved || !hasEmployee) return byProfile;

                if (byProfile.Owner!.EmployeeId != employeeId!.Value)
                    return WorkforceOwnerResolution.Fail(
                        BenefitEligibilityReasonCodes.OwnerMismatch,
                        "EmployeeId dan WorkforceProfileId tidak menunjuk orang yang sama.");
                return byProfile;
            }

            return await ResolveByEmployeeAsync(employeeId!.Value, cancellationToken);
        }

        // ------------------------------------------------------------------------------------
        // Resolusi banyak (tanpa N+1), untuk tampilan dan agregasi
        // ------------------------------------------------------------------------------------

        /// <summary>
        /// Pemilik untuk banyak profil sekaligus (tiga kueri tetap). Yang aktif maupun tidak aktif ikut dikembalikan
        /// supaya nama tetap tampil pada riwayat; profil yang tidak ditemukan atau tidak sah tidak ada pada hasil.
        /// </summary>
        public async Task<Dictionary<Guid, WorkforceBenefitOwner>> ResolveManyAsync(
            IEnumerable<Guid> workforceProfileIds, CancellationToken cancellationToken)
        {
            var ids = workforceProfileIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, WorkforceBenefitOwner>();

            var all = await LoadAsync(ids, legacyEmployeeCompat: true, cancellationToken);
            return all.Where(x => x.Value.IsResolved).ToDictionary(x => x.Key, x => x.Value.Owner!);
        }

        // ------------------------------------------------------------------------------------
        // Inti
        // ------------------------------------------------------------------------------------

        private async Task<WorkforceOwnerResolution> ResolveWithEmployeeCompatAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var all = await LoadAsync(new[] { workforceProfileId }, legacyEmployeeCompat: true, cancellationToken);
            return RequireActive(all.GetValueOrDefault(workforceProfileId) ?? NotFound());
        }

        private async Task<Dictionary<Guid, WorkforceOwnerResolution>> LoadAsync(
            IReadOnlyCollection<Guid> profileIds, bool legacyEmployeeCompat, CancellationToken cancellationToken)
        {
            var profiles = await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Where(x => profileIds.Contains(x.Id) && !x.IsDelete)
                .Select(x => new { x.Id, x.UserType, x.DisplayName, x.IsActive, x.IsCancel })
                .ToListAsync(cancellationToken);
            if (profiles.Count == 0) return new Dictionary<Guid, WorkforceOwnerResolution>();

            var loadedIds = profiles.Select(x => x.Id).ToList();

            var employees = await _dbContext.MstEmployees.AsNoTracking()
                .Where(x => loadedIds.Contains(x.WorkforceProfileId) && !x.IsDelete && !x.IsCancel)
                .Select(x => new RoleRow
                {
                    WorkforceProfileId = x.WorkforceProfileId,
                    Id = x.Id,
                    Number = x.EmployeeNumber,
                    Name = x.FullName,
                    NationalId = x.IdentityNumber,
                    EmployeeCategoryId = x.EmployeeCategoryId,
                    EmploymentTypeId = x.EmploymentTypeId,
                    EmploymentTypeName = x.EmploymentType != null ? x.EmploymentType.EmploymentTypeName : null,
                    DepartmentName = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : null,
                    JoinDate = x.JoinDate,
                    IsActive = x.IsActive && (!x.ResignDate.HasValue || x.ResignDate.Value.Date > DateTime.UtcNow.Date)
                })
                .ToListAsync(cancellationToken);

            var doctors = await _dbContext.MstDoctors.AsNoTracking()
                .Where(x => loadedIds.Contains(x.WorkforceProfileId) && !x.IsDelete && !x.IsCancel)
                .Select(x => new RoleRow
                {
                    WorkforceProfileId = x.WorkforceProfileId,
                    Id = x.Id,
                    Number = x.DoctorNumber,
                    Name = x.FullName,
                    NationalId = x.IdentityNumber,
                    EmployeeCategoryId = x.EmployeeCategoryId,
                    EmploymentTypeId = x.EmploymentTypeId,
                    EmploymentTypeName = x.EmploymentType != null ? x.EmploymentType.EmploymentTypeName : null,
                    DepartmentName = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : null,
                    JoinDate = x.JoinDate,
                    IsActive = x.IsActive && (!x.ResignDate.HasValue || x.ResignDate.Value.Date > DateTime.UtcNow.Date)
                })
                .ToListAsync(cancellationToken);

            var employeesByProfile = employees.ToLookup(x => x.WorkforceProfileId);
            var doctorsByProfile = doctors.ToLookup(x => x.WorkforceProfileId);

            var result = new Dictionary<Guid, WorkforceOwnerResolution>(profiles.Count);
            foreach (var profile in profiles)
            {
                var employeeRows = employeesByProfile[profile.Id].ToList();
                var doctorRows = doctorsByProfile[profile.Id].ToList();

                // Identitas harus tegas: lebih dari satu baris peran yang sama berarti data HR ganda.
                if (employeeRows.Count > 1 || doctorRows.Count > 1)
                {
                    result[profile.Id] = WorkforceOwnerResolution.Fail(
                        BenefitEligibilityReasonCodes.OwnerIdentityConflict,
                        "Data HR ganda untuk SDM ini. Hubungi HR sebelum melanjutkan.");
                    continue;
                }

                var employee = employeeRows.FirstOrDefault();
                var doctor = doctorRows.FirstOrDefault();

                string ownerType;
                RoleRow primary;
                if (profile.UserType == UserType.Employee)
                {
                    if (employee is null)
                    {
                        result[profile.Id] = ConflictFor(profile.UserType);
                        continue;
                    }
                    ownerType = WorkforceBenefitOwnerTypes.Employee;
                    primary = employee;
                }
                else if (profile.UserType == UserType.PermanentDoctor || profile.UserType == UserType.GuestDoctor)
                {
                    if (doctor is null)
                    {
                        result[profile.Id] = ConflictFor(profile.UserType);
                        continue;
                    }
                    ownerType = WorkforceBenefitOwnerTypes.Doctor;
                    primary = doctor;
                }
                else if (legacyEmployeeCompat && employee is not null)
                {
                    // Kontrak lama berbasis EmployeeId: baris MstEmployee tetap peran karyawan (perilaku sebelumnya).
                    ownerType = WorkforceBenefitOwnerTypes.Employee;
                    primary = employee;
                }
                else if (employee is not null)
                {
                    ownerType = WorkforceBenefitOwnerTypes.Employee;
                    primary = employee;
                }
                else if (doctor is not null)
                {
                    ownerType = WorkforceBenefitOwnerTypes.Doctor;
                    primary = doctor;
                }
                else
                {
                    result[profile.Id] = WorkforceOwnerResolution.Fail(
                        BenefitEligibilityReasonCodes.OwnerTypeNotAllowed,
                        "Jenis SDM ini tidak termasuk penerima benefit kesehatan menurut kebijakan HR.");
                    continue;
                }

                result[profile.Id] = WorkforceOwnerResolution.Ok(new WorkforceBenefitOwner
                {
                    WorkforceProfileId = profile.Id,
                    OwnerType = ownerType,
                    EmployeeId = employee?.Id,
                    DoctorId = doctor?.Id,
                    StaffNumber = primary.Number,
                    StaffName = string.IsNullOrWhiteSpace(primary.Name) ? profile.DisplayName : primary.Name,
                    NationalId = primary.NationalId,
                    EmployeeCategoryId = primary.EmployeeCategoryId,
                    EmploymentTypeId = primary.EmploymentTypeId,
                    EmploymentTypeName = primary.EmploymentTypeName,
                    OrganizationUnitName = primary.DepartmentName,
                    JoinDate = primary.JoinDate,
                    IsActive = profile.IsActive && !profile.IsCancel && primary.IsActive,
                    UserType = profile.UserType
                });
            }

            return result;
        }

        private static WorkforceOwnerResolution RequireActive(WorkforceOwnerResolution resolution) =>
            resolution.IsResolved && !resolution.Owner!.IsActive ? NotFound() : resolution;

        private static WorkforceOwnerResolution NotFound() => WorkforceOwnerResolution.Fail(
            BenefitEligibilityReasonCodes.OwnerNotFound,
            "SDM tidak ditemukan atau sudah tidak aktif di HR.");

        private static WorkforceOwnerResolution ConflictFor(UserType userType) => WorkforceOwnerResolution.Fail(
            BenefitEligibilityReasonCodes.OwnerIdentityConflict,
            (userType == UserType.PermanentDoctor || userType == UserType.GuestDoctor)
                ? "Profil SDM berjenis dokter tetapi data dokternya tidak ditemukan di HR."
                : "Profil SDM berjenis karyawan tetapi data karyawannya tidak ditemukan di HR.");

        private sealed class RoleRow
        {
            public Guid WorkforceProfileId { get; set; }
            public Guid Id { get; set; }
            public string Number { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string? NationalId { get; set; }
            public Guid? EmployeeCategoryId { get; set; }
            public Guid? EmploymentTypeId { get; set; }
            public string? EmploymentTypeName { get; set; }
            public string? DepartmentName { get; set; }
            public DateTime? JoinDate { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
