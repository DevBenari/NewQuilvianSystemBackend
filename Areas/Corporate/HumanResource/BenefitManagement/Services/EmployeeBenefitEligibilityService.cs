using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Pemeriksaan kelayakan penjamin internal (FIN-INTEGRATION-1.9 R.2.3, PeriksaKelayakan).
    /// BE-FIN-104 membuat operasi ini karena BacaPlafon (R.3.1) wajib menjalankannya ulang;
    /// CariPegawai dan DaftarKeluarga beserta adapter Registrasi tetap milik BE-FIN-106.
    /// Hanya membaca; tidak membuka transaksi.
    /// </summary>
    public sealed class EmployeeBenefitEligibilityService
    {
        private static readonly TimeZoneInfo OperationalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");

        private readonly ApplicationDbContext _dbContext;
        private readonly WorkforceBenefitOwnerResolver _ownerResolver;

        // Parameter resolver bersifat opsional agar pembuatan manual `new EmployeeBenefitEligibilityService(dbContext)`
        // (dipakai beberapa controller sebagai cadangan) tetap berjalan dengan resolver yang sama.
        public EmployeeBenefitEligibilityService(ApplicationDbContext dbContext, WorkforceBenefitOwnerResolver? ownerResolver = null)
        {
            _dbContext = dbContext;
            _ownerResolver = ownerResolver ?? new WorkforceBenefitOwnerResolver(dbContext);
        }

        public WorkforceBenefitOwnerResolver OwnerResolver => _ownerResolver;

        /// <summary>Urutan R.2.3; berhenti pada kegagalan pertama.</summary>
        public async Task<BenefitEligibilityResult> CheckAsync(BenefitEligibilityRequest request, CancellationToken cancellationToken)
        {
            var serviceDay = ToOperationalDayUtc(request.ServiceDate);

            // 1. Pemilik benefit (karyawan atau dokter tetap) ada, aktif, dan berjenis yang diizinkan.
            //    WorkforceProfileId adalah identitas utama; EmployeeId lama diterjemahkan ke Workforce.
            var resolution = await _ownerResolver.ResolveRequestAsync(
                request.WorkforceProfileId,
                request.EmployeeId == Guid.Empty ? null : request.EmployeeId,
                cancellationToken);
            if (!resolution.IsResolved)
                return Fail(resolution.ReasonCode ?? BenefitEligibilityReasonCodes.OwnerNotFound);
            var owner = resolution.Owner!;
            var employee = new { WorkforceProfileId = owner.WorkforceProfileId };

            // 2. Tepat satu enrollment layanan RS Active yang masa berlakunya mencakup tanggal layanan.
            var enrollments = await (from e in _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                                     join plan in _dbContext.MstBenefitPlans.AsNoTracking() on e.BenefitPlanId equals plan.Id
                                     join type in _dbContext.MstBenefitTypes.AsNoTracking() on plan.BenefitTypeId equals type.Id
                                     where !e.IsDelete
                                           && e.WorkforceProfileId == employee.WorkforceProfileId
                                           && e.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active
                                           && type.BenefitCategory == BenefitServiceCategories.Medical
                                           && !type.IsClaimBased
                                     select new { Enrollment = e, plan.BenefitPlanCode, plan.BenefitPlanName })
                                    .ToListAsync(cancellationToken);
            var covering = enrollments
                .Where(x => Covers(x.Enrollment.EffectiveStartDate, x.Enrollment.EffectiveEndDate, serviceDay))
                .ToList();
            if (covering.Count == 0) return Fail(BenefitEligibilityReasonCodes.EnrollmentNotActive);
            if (covering.Count > 1) return Fail(BenefitEligibilityReasonCodes.EnrollmentAmbiguous);

            var active = covering[0];
            var enrollment = active.Enrollment;
            var result = new BenefitEligibilityResult
            {
                ReasonCode = BenefitEligibilityReasonCodes.Ok,
                WorkforceProfileId = owner.WorkforceProfileId,
                OwnerType = owner.OwnerType,
                EmployeeId = owner.EmployeeId,
                DoctorId = owner.DoctorId,
                StaffNumber = owner.StaffNumber,
                StaffName = owner.StaffName,
                // Nama lama dipertahankan untuk pemanggil yang belum pindah ke StaffNumber/StaffName.
                EmployeeNumber = owner.StaffNumber,
                EmployeeName = owner.StaffName,
                EnrollmentId = enrollment.Id,
                PlanCode = active.BenefitPlanCode,
                PlanName = active.BenefitPlanName,
                CoverageType = enrollment.CoverageLevel
            };

            // 3. Pegawai sendiri: saldo ada di enrollment.
            if (!request.FamilyMemberId.HasValue)
            {
                result.RelationshipCode = BenefitRelationshipCodes.Self;
                result.BenefitPersonKind = BenefitPersonKinds.Enrollment;
                result.BenefitPersonId = enrollment.Id;
                result.EffectiveStartDate = enrollment.EffectiveStartDate;
                result.EffectiveEndDate = enrollment.EffectiveEndDate;
                result.TargetPersonNationalId = owner.NationalId;
                result.TargetPersonFullName = owner.StaffName;
                result.TargetPersonBirthDate = await GetBirthDateAsync(owner, cancellationToken);
                return result;
            }

            // 4. Anggota keluarga ada, aktif, dan milik pegawai yang sama.
            var member = await _dbContext.WfpFamilyMembers.AsNoTracking()
                .Where(x => x.Id == request.FamilyMemberId.Value && !x.IsDelete && x.WorkforceProfileId == employee.WorkforceProfileId)
                .Select(x => new { x.Id, x.IsActive, x.Relationship, x.IdentityNumber, x.FullName, x.BirthDate })
                .SingleOrDefaultAsync(cancellationToken);
            if (member is null) return Fail(BenefitEligibilityReasonCodes.FamilyMemberNotFound);
            if (!member.IsActive) return Fail(BenefitEligibilityReasonCodes.FamilyMemberInactive);

            // 5. Hubungan dibolehkan CoverageType plan (R.1.2).
            var relationship = BenefitRelationshipCodes.FromFamilyRelationship(member.Relationship);
            if (!BenefitCoverageTypes.AllowsRelationship(enrollment.CoverageLevel, relationship))
                return Fail(BenefitEligibilityReasonCodes.RelationshipNotCovered);

            // 6. Tanggungan Verified, layak, ditanggung, dan dalam masa berlaku.
            var dependents = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                .Where(x => !x.IsDelete && x.EmployeeBenefitEnrollmentId == enrollment.Id && x.FamilyMemberId == member.Id)
                .ToListAsync(cancellationToken);

            var verified = dependents.FirstOrDefault(x => x.DependentStatus == EmployeeBenefitDependentStatuses.Verified);
            if (verified is null)
            {
                if (dependents.Any(x => x.DependentStatus is EmployeeBenefitDependentStatuses.Draft or EmployeeBenefitDependentStatuses.Requested))
                    return Fail(BenefitEligibilityReasonCodes.DependentNotVerified);
                return dependents.Count > 0
                    ? Fail(BenefitEligibilityReasonCodes.DependentNotEligible)
                    : Fail(BenefitEligibilityReasonCodes.DependentNotRegistered);
            }

            if (!verified.IsEligible || !verified.IsCovered || !verified.IsActive)
                return Fail(BenefitEligibilityReasonCodes.DependentNotEligible);
            if (!Covers(verified.EffectiveStartDate, verified.EffectiveEndDate, serviceDay))
                return Fail(BenefitEligibilityReasonCodes.DependentOutOfPeriod);

            result.FamilyMemberId = member.Id;
            result.RelationshipCode = relationship;
            result.BenefitPersonKind = BenefitPersonKinds.Dependent;
            result.BenefitPersonId = verified.Id;
            result.EffectiveStartDate = verified.EffectiveStartDate;
            result.EffectiveEndDate = verified.EffectiveEndDate;
            result.TargetPersonNationalId = member.IdentityNumber;
            result.TargetPersonFullName = member.FullName;
            result.TargetPersonBirthDate = member.BirthDate;
            return result;
        }

        /// <summary>
        /// CariPegawai (R.2.1): pegawai aktif yang NIP, NIK, atau namanya memuat kata kunci. Paling banyak
        /// 20 baris; bila lebih, IsTruncated true dan petugas diminta mengetik lebih spesifik (FIN-VAL-269).
        /// Hanya NIP, NIK tersamar, nama, unit, dan penanda benefit aktif yang keluar dari HR.
        /// </summary>
        public async Task<EmployeeSearchResult> SearchEmployeesAsync(string keyword, CancellationToken cancellationToken)
        {
            var term = (keyword ?? string.Empty).Trim().ToLower();
            if (term.Length < EmployeeLookupLimits.MinimumKeywordLength)
                throw new ArgumentException("Kata kunci pencarian pegawai minimal 3 karakter.", nameof(keyword));

            var rows = await (from e in _dbContext.MstEmployees.AsNoTracking()
                              join d in _dbContext.MstDepartments.AsNoTracking() on e.PrimaryDepartmentId equals d.Id into departmentJoin
                              from d in departmentJoin.DefaultIfEmpty()
                              where !e.IsDelete && e.IsActive
                                    && (e.EmployeeNumber.ToLower().Contains(term)
                                        || e.IdentityNumber.Contains(term)
                                        || e.FullName.ToLower().Contains(term))
                              orderby e.FullName, e.EmployeeNumber
                              select new
                              {
                                  e.Id,
                                  e.WorkforceProfileId,
                                  e.EmployeeNumber,
                                  e.IdentityNumber,
                                  e.FullName,
                                  DepartmentName = d != null ? d.DepartmentName : null
                              })
                             .Take(EmployeeLookupLimits.MaximumResults + 1)
                             .ToListAsync(cancellationToken);

            var result = new EmployeeSearchResult { IsTruncated = rows.Count > EmployeeLookupLimits.MaximumResults };
            var page = rows.Take(EmployeeLookupLimits.MaximumResults).ToList();
            if (page.Count == 0) return result;

            var today = ToOperationalDayUtc(AppDateTimeHelper.OperationalDate());
            var profileIds = page.Select(x => x.WorkforceProfileId).Distinct().ToList();
            var enrollmentPeriods = await ActiveMedicalEnrollments()
                .Where(x => profileIds.Contains(x.WorkforceProfileId))
                .Select(x => new { x.WorkforceProfileId, x.EffectiveStartDate, x.EffectiveEndDate })
                .ToListAsync(cancellationToken);
            var withActiveEnrollment = enrollmentPeriods
                .Where(x => Covers(x.EffectiveStartDate, x.EffectiveEndDate, today))
                .Select(x => x.WorkforceProfileId)
                .ToHashSet();

            result.Items = page.Select(x => new EmployeeSearchItem
            {
                EmployeeId = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                EmployeeNumber = x.EmployeeNumber,
                NationalIdMasked = MaskIdentity(x.IdentityNumber) ?? string.Empty,
                FullName = x.FullName,
                OrganizationUnitName = x.DepartmentName,
                HasActiveMedicalEnrollment = withActiveEnrollment.Contains(x.WorkforceProfileId)
            }).ToList();
            return result;
        }

        /// <summary>
        /// Pencarian pemilik benefit gabungan: karyawan (jenis SDM Employee) dan dokter tetap (PermanentDoctor)
        /// aktif yang NIP/nomor dokter, NIK, atau namanya memuat kata kunci. Paling banyak 20 baris
        /// (IsTruncated bila lebih). Memakai kueri tetap: satu untuk karyawan, satu untuk dokter, dua untuk peran
        /// lain SDM berperan ganda, dan satu untuk penanda enrollment Medical aktif; tidak ada kueri per baris.
        /// NIK keluar tersamar.
        /// </summary>
        public async Task<WorkforceSearchResult> SearchWorkforceAsync(string keyword, CancellationToken cancellationToken)
        {
            var term = (keyword ?? string.Empty).Trim().ToLower();
            if (term.Length < EmployeeLookupLimits.MinimumKeywordLength)
                throw new ArgumentException("Kata kunci pencarian SDM minimal 3 karakter.", nameof(keyword));

            var take = EmployeeLookupLimits.MaximumResults + 1;
            var today = ToOperationalDayUtc(AppDateTimeHelper.OperationalDate());

            var employeeRows = await (from e in _dbContext.MstEmployees.AsNoTracking()
                                      join p in _dbContext.MstWorkforceProfiles.AsNoTracking() on e.WorkforceProfileId equals p.Id into pGroup
                                      from p in pGroup.DefaultIfEmpty()
                                      join dept in _dbContext.MstDepartments.AsNoTracking() on e.PrimaryDepartmentId equals dept.Id into deptGroup
                                      from dept in deptGroup.DefaultIfEmpty()
                                      join et in _dbContext.MstEmploymentTypes.AsNoTracking() on e.EmploymentTypeId equals et.Id into etGroup
                                      from et in etGroup.DefaultIfEmpty()
                                      where !e.IsDelete && e.IsActive && (!e.ResignDate.HasValue || e.ResignDate > today)
                                            && (p == null || (!p.IsDelete && p.IsActive))
                                            && (((e.EmployeeNumber ?? string.Empty).ToLower().Contains(term))
                                                || ((e.IdentityNumber ?? string.Empty).ToLower().Contains(term))
                                                || ((e.FullName ?? string.Empty).ToLower().Contains(term)))
                                      orderby e.FullName, e.EmployeeNumber
                                      select new WorkforceSearchRow
                                      {
                                          WorkforceProfileId = (p != null && p.Id != Guid.Empty)
                                              ? p.Id
                                              : (e.WorkforceProfileId != Guid.Empty ? e.WorkforceProfileId : e.Id),
                                          OwnerType = WorkforceBenefitOwnerTypes.Employee,
                                          EmployeeId = e.Id,
                                          Number = e.EmployeeNumber ?? string.Empty,
                                          FullName = e.FullName ?? string.Empty,
                                          NationalId = e.IdentityNumber,
                                          DepartmentName = dept != null ? dept.DepartmentName : null,
                                          EmploymentTypeName = et != null ? et.EmploymentTypeName : null
                                      })
                                     .Take(take)
                                     .ToListAsync(cancellationToken);

            var doctorRows = await (from d in _dbContext.MstDoctors.AsNoTracking()
                                    join p in _dbContext.MstWorkforceProfiles.AsNoTracking() on d.WorkforceProfileId equals p.Id into pGroup
                                    from p in pGroup.DefaultIfEmpty()
                                    join dept in _dbContext.MstDepartments.AsNoTracking() on d.PrimaryDepartmentId equals (Guid?)dept.Id into deptGroup
                                    from dept in deptGroup.DefaultIfEmpty()
                                    join et in _dbContext.MstEmploymentTypes.AsNoTracking() on d.EmploymentTypeId equals et.Id into etGroup
                                    from et in etGroup.DefaultIfEmpty()
                                    where !d.IsDelete && d.IsActive && (!d.ResignDate.HasValue || d.ResignDate > today)
                                          && (p == null || (!p.IsDelete && p.IsActive))
                                          && (((d.DoctorNumber ?? string.Empty).ToLower().Contains(term))
                                              || ((d.IdentityNumber ?? string.Empty).ToLower().Contains(term))
                                              || ((d.FullName ?? string.Empty).ToLower().Contains(term)))
                                    orderby d.FullName, d.DoctorNumber
                                    select new WorkforceSearchRow
                                    {
                                        WorkforceProfileId = (p != null && p.Id != Guid.Empty)
                                            ? p.Id
                                            : (d.WorkforceProfileId != Guid.Empty ? d.WorkforceProfileId : d.Id),
                                        OwnerType = WorkforceBenefitOwnerTypes.Doctor,
                                        DoctorId = d.Id,
                                        Number = d.DoctorNumber ?? string.Empty,
                                        FullName = d.FullName ?? string.Empty,
                                        NationalId = d.IdentityNumber,
                                        DepartmentName = dept != null ? dept.DepartmentName : null,
                                        EmploymentTypeName = et != null ? et.EmploymentTypeName : null
                                    })
                                   .Take(take)
                                   .ToListAsync(cancellationToken);

            // Satu SDM berperan ganda tetap satu baris: peran yang sesuai jenis SDM pada profil menang.
            var merged = employeeRows.Concat(doctorRows)
                .GroupBy(x => x.WorkforceProfileId)
                .Select(g => g.First())
                .OrderBy(x => x.FullName).ThenBy(x => x.Number)
                .ToList();

            var result = new WorkforceSearchResult { IsTruncated = merged.Count > EmployeeLookupLimits.MaximumResults };
            var page = merged.Take(EmployeeLookupLimits.MaximumResults).ToList();
            if (page.Count == 0) return result;

            var profileIds = page.Select(x => x.WorkforceProfileId).ToList();
            var employeeIds = await _dbContext.MstEmployees.AsNoTracking()
                .Where(x => profileIds.Contains(x.WorkforceProfileId) && !x.IsDelete && !x.IsCancel)
                .Select(x => new { x.WorkforceProfileId, x.Id })
                .ToListAsync(cancellationToken);
            var doctorIds = await _dbContext.MstDoctors.AsNoTracking()
                .Where(x => profileIds.Contains(x.WorkforceProfileId) && !x.IsDelete && !x.IsCancel)
                .Select(x => new { x.WorkforceProfileId, x.Id })
                .ToListAsync(cancellationToken);

            var enrollmentPeriods = await ActiveMedicalEnrollments()
                .Where(x => profileIds.Contains(x.WorkforceProfileId))
                .Select(x => new { x.WorkforceProfileId, x.EffectiveStartDate, x.EffectiveEndDate })
                .ToListAsync(cancellationToken);
            var withActiveEnrollment = enrollmentPeriods
                .Where(x => Covers(x.EffectiveStartDate, x.EffectiveEndDate, today))
                .Select(x => x.WorkforceProfileId)
                .ToHashSet();

            result.Items = page.Select(x => new WorkforceSearchItem
            {
                WorkforceProfileId = x.WorkforceProfileId,
                OwnerType = x.OwnerType,
                EmployeeId = x.EmployeeId ?? employeeIds.Where(r => r.WorkforceProfileId == x.WorkforceProfileId).Select(r => (Guid?)r.Id).FirstOrDefault(),
                DoctorId = x.DoctorId ?? doctorIds.Where(r => r.WorkforceProfileId == x.WorkforceProfileId).Select(r => (Guid?)r.Id).FirstOrDefault(),
                StaffNumber = x.Number,
                FullName = x.FullName,
                NationalIdMasked = MaskIdentity(x.NationalId) ?? string.Empty,
                OrganizationUnitName = x.DepartmentName,
                EmploymentType = x.EmploymentTypeName,
                IsActive = true,
                HasActiveMedicalEnrollment = withActiveEnrollment.Contains(x.WorkforceProfileId)
            }).ToList();
            return result;
        }

        private sealed class WorkforceSearchRow
        {
            public Guid WorkforceProfileId { get; set; }
            public string OwnerType { get; set; } = string.Empty;
            public Guid? EmployeeId { get; set; }
            public Guid? DoctorId { get; set; }
            public string Number { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string? NationalId { get; set; }
            public string? DepartmentName { get; set; }
            public string? EmploymentTypeName { get; set; }
        }

        /// <summary>Tanggal lahir pemilik dari baris peran utamanya (karyawan atau dokter), untuk pencocokan identitas.</summary>
        private async Task<DateTime?> GetBirthDateAsync(WorkforceBenefitOwner owner, CancellationToken cancellationToken)
        {
            if (owner.OwnerType == WorkforceBenefitOwnerTypes.Doctor && owner.DoctorId.HasValue)
                return await _dbContext.MstDoctors.AsNoTracking()
                    .Where(x => x.Id == owner.DoctorId.Value)
                    .Select(x => x.BirthDate)
                    .FirstOrDefaultAsync(cancellationToken);

            if (owner.EmployeeId.HasValue)
                return await _dbContext.MstEmployees.AsNoTracking()
                    .Where(x => x.Id == owner.EmployeeId.Value)
                    .Select(x => (DateTime?)x.BirthDate)
                    .FirstOrDefaultAsync(cancellationToken);

            return null;
        }

        /// <summary>
        /// DaftarKeluarga (R.2.2): anggota keluarga aktif satu pegawai beserta status verifikasi tanggungan
        /// pada enrollment layanan RS yang berlaku hari ini dan apakah plan mencakup hubungannya.
        /// </summary>
        public async Task<List<FamilyMemberEligibilityItem>> ListFamilyMembersAsync(Guid employeeId, CancellationToken cancellationToken)
        {
            var resolution = await _ownerResolver.ResolveByEmployeeAsync(employeeId, cancellationToken);
            if (!resolution.IsResolved) throw new KeyNotFoundException(MessageFor(BenefitEligibilityReasonCodes.EmployeeNotFound));
            return await ListFamilyMembersByWorkforceAsync(resolution.Owner!.WorkforceProfileId, cancellationToken);
        }

        /// <summary>DaftarKeluarga untuk pemilik benefit mana pun (karyawan atau dokter tetap) berdasarkan WorkforceProfileId.</summary>
        public async Task<List<FamilyMemberEligibilityItem>> ListFamilyMembersByWorkforceAsync(
            Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var resolution = await _ownerResolver.ResolveByWorkforceProfileAsync(workforceProfileId, cancellationToken);
            if (!resolution.IsResolved) throw new KeyNotFoundException(MessageFor(resolution.ReasonCode ?? BenefitEligibilityReasonCodes.OwnerNotFound));
            var employee = new { WorkforceProfileId = resolution.Owner!.WorkforceProfileId };

            var members = await _dbContext.WfpFamilyMembers.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.WorkforceProfileId == employee.WorkforceProfileId)
                .OrderBy(x => x.FullName)
                .ToListAsync(cancellationToken);

            var today = ToOperationalDayUtc(AppDateTimeHelper.OperationalDate());
            var covering = (await ActiveMedicalEnrollments()
                    .Where(x => x.WorkforceProfileId == employee.WorkforceProfileId)
                    .ToListAsync(cancellationToken))
                .Where(x => Covers(x.EffectiveStartDate, x.EffectiveEndDate, today))
                .ToList();
            // Lebih dari satu enrollment berarti data ganda (ENROLLMENT_AMBIGUOUS): tidak ada yang dianggap berlaku.
            var enrollment = covering.Count == 1 ? covering[0] : null;

            var dependents = enrollment is null
                ? new List<TrxEmployeeBenefitDependent>()
                : await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .Where(x => !x.IsDelete && x.EmployeeBenefitEnrollmentId == enrollment.Id && x.FamilyMemberId != null)
                    .ToListAsync(cancellationToken);

            var localToday = AppDateTimeHelper.OperationalDate();
            return members.Select(member =>
            {
                var relationship = BenefitRelationshipCodes.FromFamilyRelationship(member.Relationship);
                var statuses = dependents.Where(d => d.FamilyMemberId == member.Id).Select(d => d.DependentStatus).ToList();
                return new FamilyMemberEligibilityItem
                {
                    FamilyMemberId = member.Id,
                    FullName = member.FullName,
                    RelationshipCode = relationship,
                    AgeYears = AgeInYears(member.BirthDate, localToday),
                    IdentityMasked = MaskIdentity(member.IdentityNumber),
                    DependentVerification = VerificationCodeOf(statuses),
                    CoveredByPlan = enrollment is not null
                        && BenefitCoverageTypes.AllowsRelationship(enrollment.CoverageLevel, relationship)
                };
            }).ToList();
        }

        /// <summary>Enrollment layanan RS berstatus Active, bukan berbasis klaim; masa berlaku dinilai pemanggil.</summary>
        private IQueryable<TrxEmployeeBenefitEnrollment> ActiveMedicalEnrollments() =>
            from e in _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
            join plan in _dbContext.MstBenefitPlans.AsNoTracking() on e.BenefitPlanId equals plan.Id
            join type in _dbContext.MstBenefitTypes.AsNoTracking() on plan.BenefitTypeId equals type.Id
            where !e.IsDelete
                  && e.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active
                  && type.BenefitCategory == BenefitServiceCategories.Medical
                  && !type.IsClaimBased
            select e;

        /// <summary>Verified menang atas pengajuan; pengajuan menang atas penolakan; penolakan atas berakhir.</summary>
        private static string VerificationCodeOf(IReadOnlyCollection<string> statuses)
        {
            if (statuses.Contains(EmployeeBenefitDependentStatuses.Verified)) return BenefitDependentVerificationCodes.Verified;
            if (statuses.Contains(EmployeeBenefitDependentStatuses.Requested) || statuses.Contains(EmployeeBenefitDependentStatuses.Draft))
                return BenefitDependentVerificationCodes.Requested;
            if (statuses.Contains(EmployeeBenefitDependentStatuses.Rejected)) return BenefitDependentVerificationCodes.Rejected;
            if (statuses.Contains(EmployeeBenefitDependentStatuses.Ended)) return BenefitDependentVerificationCodes.Ended;
            return BenefitDependentVerificationCodes.NotRegistered;
        }

        private static int? AgeInYears(DateTime? birthDate, DateTime today)
        {
            if (!birthDate.HasValue) return null;
            var born = birthDate.Value.Date;
            var age = today.Year - born.Year;
            if (born > today.AddYears(-age)) age--;
            return age < 0 ? null : age;
        }

        /// <summary>Empat digit pertama dan empat terakhir tampil; sisanya "•". Nomor pendek tertutup seluruhnya.</summary>
        internal static string? MaskIdentity(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var digits = value.Trim();
            if (digits.Length <= 8) return new string('•', digits.Length);
            return string.Concat(digits.AsSpan(0, 4), new string('•', digits.Length - 8), digits.AsSpan(digits.Length - 4));
        }

        /// <summary>
        /// Status orang yang dapat dipakai saldonya, tanpa tanggal (dipakai PakaiPlafon; tanggal sudah
        /// diperiksa BacaPlafon pada transaksi finalisasi yang sama, R.3.4 langkah 2).
        /// </summary>
        internal static bool IsEnrollmentUsable(TrxEmployeeBenefitEnrollment e) =>
            !e.IsDelete && e.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active;

        internal static bool IsDependentUsable(TrxEmployeeBenefitDependent d) =>
            !d.IsDelete && d.IsActive && d.IsEligible && d.IsCovered
            && d.DependentStatus == EmployeeBenefitDependentStatuses.Verified;

        public static string MessageFor(string reasonCode) => reasonCode switch
        {
            BenefitEligibilityReasonCodes.EmployeeNotFound => "Pegawai tidak ditemukan atau sudah tidak aktif di HR.",
            BenefitEligibilityReasonCodes.OwnerNotFound => "SDM tidak ditemukan atau sudah tidak aktif di HR.",
            BenefitEligibilityReasonCodes.OwnerTypeNotAllowed => "Jenis SDM ini tidak termasuk penerima benefit kesehatan menurut kebijakan HR.",
            BenefitEligibilityReasonCodes.OwnerIdentityConflict => "Data HR SDM ini bertentangan atau ganda. Hubungi HR sebelum melanjutkan.",
            BenefitEligibilityReasonCodes.OwnerMismatch => "EmployeeId dan WorkforceProfileId tidak menunjuk orang yang sama.",
            BenefitEligibilityReasonCodes.EnrollmentNotActive => "Pegawai belum memiliki benefit layanan RS yang aktif. Daftarkan sebagai Pasien Umum atau hubungi HR.",
            BenefitEligibilityReasonCodes.EnrollmentAmbiguous => "Data benefit pegawai ganda di HR. Hubungi HR sebelum melanjutkan.",
            BenefitEligibilityReasonCodes.FamilyMemberNotFound => "Anggota keluarga tidak terdaftar pada pegawai ini di HR.",
            BenefitEligibilityReasonCodes.FamilyMemberInactive => "Anggota keluarga ini sudah tidak aktif di HR.",
            BenefitEligibilityReasonCodes.RelationshipNotCovered => "Jenis hubungan keluarga ini tidak ditanggung oleh plan benefit pegawai.",
            BenefitEligibilityReasonCodes.DependentNotRegistered => "Pasien belum terdaftar sebagai tanggungan benefit pegawai. Daftarkan di HR terlebih dahulu atau daftarkan sebagai Pasien Umum.",
            BenefitEligibilityReasonCodes.DependentNotVerified => "Pendaftaran tanggungan pasien belum diverifikasi HR.",
            BenefitEligibilityReasonCodes.DependentNotEligible => "Pasien tidak ditanggung benefit pegawai menurut HR.",
            BenefitEligibilityReasonCodes.DependentOutOfPeriod => "Masa berlaku tanggungan pasien tidak mencakup tanggal layanan.",
            BenefitEligibilityReasonCodes.HrUnavailable => "Data HR belum dapat dibaca. Coba lagi, atau daftarkan sebagai Pasien Umum.",
            _ => string.Empty
        };

        /// <summary>
        /// Tanggal layanan dinormalkan ke awal hari operasional dalam UTC — bentuk yang sama dengan
        /// EffectiveStartDate/EffectiveEndDate yang disimpan BE-FIN-103 — sehingga perbandingan
        /// berlaku per tanggal, termasuk tanggal akhir.
        /// </summary>
        internal static DateTime ToOperationalDayUtc(DateTime value)
        {
            var localDate = value.Kind == DateTimeKind.Utc
                ? TimeZoneInfo.ConvertTimeFromUtc(value, OperationalTimeZone).Date
                : value.Date;
            return AppDateTimeHelper.OperationalDateToUtc(localDate);
        }

        private static bool Covers(DateTime? start, DateTime? end, DateTime dayUtc) =>
            (!start.HasValue || start.Value <= dayUtc) && (!end.HasValue || end.Value >= dayUtc);

        private static BenefitEligibilityResult Fail(string reasonCode) =>
            new() { ReasonCode = reasonCode, Message = MessageFor(reasonCode) };
    }
}
