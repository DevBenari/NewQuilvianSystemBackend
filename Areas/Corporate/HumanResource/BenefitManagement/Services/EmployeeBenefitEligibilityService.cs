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

        public EmployeeBenefitEligibilityService(ApplicationDbContext dbContext) => _dbContext = dbContext;

        /// <summary>Urutan R.2.3; berhenti pada kegagalan pertama.</summary>
        public async Task<BenefitEligibilityResult> CheckAsync(BenefitEligibilityRequest request, CancellationToken cancellationToken)
        {
            var serviceDay = ToOperationalDayUtc(request.ServiceDate);

            // 1. Pegawai ada dan aktif.
            var employee = await _dbContext.MstEmployees.AsNoTracking()
                .Where(x => x.Id == request.EmployeeId && !x.IsDelete && x.IsActive)
                .Select(x => new { x.Id, x.WorkforceProfileId })
                .SingleOrDefaultAsync(cancellationToken);
            if (employee is null) return Fail(BenefitEligibilityReasonCodes.EmployeeNotFound);

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
                EmployeeId = employee.Id,
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
                return result;
            }

            // 4. Anggota keluarga ada, aktif, dan milik pegawai yang sama.
            var member = await _dbContext.WfpFamilyMembers.AsNoTracking()
                .Where(x => x.Id == request.FamilyMemberId.Value && !x.IsDelete && x.WorkforceProfileId == employee.WorkforceProfileId)
                .Select(x => new { x.Id, x.IsActive, x.Relationship })
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
            return result;
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
