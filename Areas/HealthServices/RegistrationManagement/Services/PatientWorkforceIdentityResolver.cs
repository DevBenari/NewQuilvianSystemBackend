using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Layanan read-only untuk mendeteksi secara otomatis status workforce (Karyawan/Dokter/Keluarga/Umum)
    /// dan memeriksa kelayakan Benefit Karyawan berdasarkan PatientId yang dipilih dari No. RM.
    /// Memisahkan status identitas HR dari status kelayakan benefit.
    /// </summary>
    public sealed class PatientWorkforceIdentityResolver : IPatientWorkforceIdentityResolver
    {
        private const string LogCategory = "HealthServices.RegistrationManagement.WorkforceIdentityResolver";

        private readonly ApplicationDbContext _dbContext;
        private readonly RegistrationEmployeeBenefitLookupService _lookupService;
        private readonly LoggerService _loggerService;
        private readonly WorkforceBenefitOwnerResolver _ownerResolver;

        public PatientWorkforceIdentityResolver(
            ApplicationDbContext dbContext,
            RegistrationEmployeeBenefitLookupService lookupService,
            LoggerService loggerService,
            WorkforceBenefitOwnerResolver? ownerResolver = null)
        {
            _dbContext = dbContext;
            _lookupService = lookupService;
            _loggerService = loggerService;
            _ownerResolver = ownerResolver ?? new WorkforceBenefitOwnerResolver(dbContext);
        }

        public async Task<PatientWorkforceStatusResponse> ResolveStatusAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            // 1. Validasi keberadaan pasien
            var patient = await _dbContext.MstPatients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == patientId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Data pasien tidak ditemukan.");

            // 2. Evaluasi canonical patient bila pasien pernah di-merge
            var canonicalPatient = patient;
            var mergeDepth = 0;
            while (canonicalPatient.MergedToPatientId.HasValue &&
                   canonicalPatient.MergedToPatientId.Value != Guid.Empty &&
                   canonicalPatient.MergedToPatientId.Value != canonicalPatient.Id &&
                   mergeDepth < 5)
            {
                var target = await _dbContext.MstPatients.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == canonicalPatient.MergedToPatientId.Value && !x.IsDelete, cancellationToken);
                if (target == null) break;
                canonicalPatient = target;
                mergeDepth++;
            }

            var response = new PatientWorkforceStatusResponse
            {
                PatientId = patient.Id,
                MedicalRecordNumber = canonicalPatient.MedicalRecordNumber,
                PatientName = canonicalPatient.FullName,
                NationalIdMasked = MaskNik(canonicalPatient.IdentityNumber),
                PersonCategory = PersonCategories.General,
                IdentityStatus = WorkforceIdentityStatuses.NotLinked,
                IsIdentityVerified = false,
                IsActiveWorkforce = false,
                BenefitStatus = "NOT_ELIGIBLE",
                IsBenefitEligible = false,
                RequiresReview = false
            };

            // 3. Periksa kepemilikan kartu penjamin internal aktif (MstPatientCompanyGuarantor)
            var internalCards = await (
                from cg in _dbContext.MstPatientCompanyGuarantors.AsNoTracking()
                join comp in _dbContext.MstCompanyGuarantors.AsNoTracking()
                    on cg.CompanyGuarantorId equals comp.Id
                where cg.PatientId == canonicalPatient.Id
                    && !cg.IsDelete && !cg.IsCancel && cg.IsActive
                    && comp.IsEmployeeBenefitGuarantor
                    && !comp.IsDelete && !comp.IsCancel && comp.IsActive
                select new { Card = cg, Company = comp }
            ).ToListAsync(cancellationToken);

            var primaryInternalCard = internalCards.FirstOrDefault(x => x.Card.IsPrimary) ?? internalCards.FirstOrDefault();
            if (primaryInternalCard != null)
            {
                response.InternalGuarantorCardId = primaryInternalCard.Card.Id;
            }

            // 4. Deteksi apakah pasien terdaftar sebagai Anggota Keluarga Pegawai (EMPLOYEE_FAMILY)
            var familyCard = internalCards.FirstOrDefault(x =>
                x.Card.FamilyMemberId.HasValue ||
                (!string.IsNullOrWhiteSpace(x.Card.BenefitRelationship) &&
                 !string.Equals(x.Card.BenefitRelationship, BenefitRelationshipCodes.Self, StringComparison.OrdinalIgnoreCase)));

            if (familyCard != null && (familyCard.Card.BenefitOwnerWorkforceProfileId.HasValue || familyCard.Card.EmployeeId.HasValue))
            {
                // Pemilik benefit keluarga: WorkforceProfileId pada kartu baru; kartu lama hanya membawa EmployeeId.
                var sponsorProfileId = familyCard.Card.BenefitOwnerWorkforceProfileId
                    ?? await _dbContext.MstEmployees.AsNoTracking()
                        .Where(x => x.Id == familyCard.Card.EmployeeId!.Value && !x.IsDelete && !x.IsCancel)
                        .Select(x => (Guid?)x.WorkforceProfileId)
                        .FirstOrDefaultAsync(cancellationToken);

                var sponsors = sponsorProfileId.HasValue
                    ? await _ownerResolver.ResolveManyAsync(new[] { sponsorProfileId.Value }, cancellationToken)
                    : new Dictionary<Guid, QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs.WorkforceBenefitOwner>();

                if (sponsorProfileId.HasValue && sponsors.TryGetValue(sponsorProfileId.Value, out var sponsor))
                {
                    var isSponsorActive = sponsor.IsActive;
                    response.PersonCategory = PersonCategories.EmployeeFamily;
                    response.IdentityStatus = isSponsorActive ? WorkforceIdentityStatuses.Active : WorkforceIdentityStatuses.Inactive;
                    response.IsIdentityVerified = true;
                    response.IsActiveWorkforce = isSponsorActive;
                    response.StaffNumber = sponsor.StaffNumber;
                    response.EmployeeId = sponsor.EmployeeId;
                    response.DoctorId = sponsor.DoctorId;
                    response.BenefitOwnerWorkforceProfileId = sponsor.WorkforceProfileId;
                    response.BenefitOwnerType = sponsor.OwnerType;
                    response.BenefitOwnerName = sponsor.StaffName;
                    response.BenefitRelationship = familyCard.Card.BenefitRelationship ?? "FAMILY";
                    response.OrganizationUnitName = sponsor.OrganizationUnitName;
                    response.PositionName = await GetPositionNameAsync(sponsor, cancellationToken);

                    if (isSponsorActive)
                    {
                        await CheckEligibilitySafeAsync(response, sponsor.WorkforceProfileId, null, familyCard.Card.FamilyMemberId, cancellationToken);
                    }
                    else
                    {
                        response.IsBenefitEligible = false;
                        response.BenefitStatus = "INACTIVE";
                        response.Message = "Pegawai penanggung benefit sudah tidak aktif / resign di HR.";
                    }

                    return response;
                }
            }

            // 5. Pencocokan langsung terhadap MstEmployee dan MstDoctor (NIK atau tautan kartu)
            var normalizedNik = NormalizeNik(canonicalPatient.IdentityNumber);
            List<MstEmployee> matchedEmployees = new();
            List<MstDoctor> matchedDoctors = new();

            if (!string.IsNullOrWhiteSpace(normalizedNik) && normalizedNik.Length >= 8)
            {
                matchedEmployees = await _dbContext.MstEmployees.AsNoTracking()
                    .Include(x => x.PrimaryDepartment)
                    .Include(x => x.PrimaryPosition)
                    .Where(x => !x.IsDelete && !x.IsCancel && x.IdentityNumber == normalizedNik)
                    .ToListAsync(cancellationToken);

                matchedDoctors = await _dbContext.MstDoctors.AsNoTracking()
                    .Include(x => x.PrimaryDepartment)
                    .Include(x => x.PrimaryPosition)
                    .Where(x => !x.IsDelete && !x.IsCancel && x.IdentityNumber == normalizedNik)
                    .ToListAsync(cancellationToken);
            }

            // Bila NIK belum cocok, tetapi ada kartu internal yang menunjuk ke EmployeeId pasien sendiri
            if (matchedEmployees.Count == 0 && primaryInternalCard?.Card.EmployeeId.HasValue == true)
            {
                var cardEmp = await _dbContext.MstEmployees.AsNoTracking()
                    .Include(x => x.PrimaryDepartment)
                    .Include(x => x.PrimaryPosition)
                    .FirstOrDefaultAsync(x => x.Id == primaryInternalCard.Card.EmployeeId.Value && !x.IsDelete && !x.IsCancel, cancellationToken);

                if (cardEmp != null && !matchedEmployees.Any(x => x.Id == cardEmp.Id))
                {
                    matchedEmployees.Add(cardEmp);
                }
            }

            // Hubungkan silang dokter dan pegawai melalui WorkforceProfileId bila salah satu ditemukan
            if (matchedEmployees.Count > 0 && matchedDoctors.Count == 0)
            {
                var wfpIds = matchedEmployees.Select(x => x.WorkforceProfileId).Distinct().ToList();
                var crossDoctors = await _dbContext.MstDoctors.AsNoTracking()
                    .Include(x => x.PrimaryDepartment)
                    .Include(x => x.PrimaryPosition)
                    .Where(x => !x.IsDelete && !x.IsCancel && wfpIds.Contains(x.WorkforceProfileId))
                    .ToListAsync(cancellationToken);
                matchedDoctors.AddRange(crossDoctors);
            }
            else if (matchedDoctors.Count > 0 && matchedEmployees.Count == 0)
            {
                var wfpIds = matchedDoctors.Select(x => x.WorkforceProfileId).Distinct().ToList();
                var crossEmployees = await _dbContext.MstEmployees.AsNoTracking()
                    .Include(x => x.PrimaryDepartment)
                    .Include(x => x.PrimaryPosition)
                    .Where(x => !x.IsDelete && !x.IsCancel && wfpIds.Contains(x.WorkforceProfileId))
                    .ToListAsync(cancellationToken);
                matchedEmployees.AddRange(crossEmployees);
            }

            // 6. Deteksi konflik identitas atau data ganda
            if (matchedEmployees.Count > 1)
            {
                response.PersonCategory = PersonCategories.ReviewRequired;
                response.IdentityStatus = WorkforceIdentityStatuses.Conflict;
                response.RequiresReview = true;
                response.Message = "Ditemukan lebih dari satu catatan pegawai aktif dengan NIK yang sama di HR. Perlu verifikasi.";
                return response;
            }

            if (matchedDoctors.Count > 1)
            {
                response.PersonCategory = PersonCategories.ReviewRequired;
                response.IdentityStatus = WorkforceIdentityStatuses.Conflict;
                response.RequiresReview = true;
                response.Message = "Ditemukan lebih dari satu catatan dokter dengan NIK yang sama di HR. Perlu verifikasi.";
                return response;
            }

            // 7. Pengelompokan Kategori: Karyawan, Dokter, Dokter Karyawan, atau Umum
            var singleEmployee = matchedEmployees.FirstOrDefault();
            var singleDoctor = matchedDoctors.FirstOrDefault();

            // Kasus A: Pegawai sekaligus Dokter
            if (singleEmployee != null && singleDoctor != null)
            {
                var isEmpActive = singleEmployee.IsActive && !singleEmployee.ResignDate.HasValue;
                var isDocActive = singleDoctor.IsActive && !singleDoctor.ResignDate.HasValue;
                var isActive = isEmpActive || isDocActive;

                response.PersonCategory = PersonCategories.DoctorEmployee;
                response.IdentityStatus = isActive ? WorkforceIdentityStatuses.Active : WorkforceIdentityStatuses.Inactive;
                response.IsIdentityVerified = true;
                response.IsActiveWorkforce = isActive;
                response.EmployeeId = singleEmployee.Id;
                response.DoctorId = singleDoctor.Id;
                response.BenefitOwnerWorkforceProfileId = singleEmployee.WorkforceProfileId;
                response.StaffNumber = $"{singleEmployee.EmployeeNumber} / {singleDoctor.DoctorNumber}";
                response.DoctorKind = FormatDoctorKind(singleDoctor.PracticeType);
                response.OrganizationUnitName = singleEmployee.PrimaryDepartment?.DepartmentName;
                response.PositionName = singleEmployee.PrimaryPosition?.PositionName;
                response.BenefitRelationship = BenefitRelationshipCodes.Self;

                // Satu SDM berperan ganda adalah satu pemilik benefit (satu WorkforceProfileId); jenis pemilik
                // mengikuti jenis SDM pada profil, bukan peran yang kebetulan ditemukan lebih dulu.
                if (isActive)
                {
                    await CheckEligibilitySafeAsync(response, singleEmployee.WorkforceProfileId, null, null, cancellationToken);
                }
                else
                {
                    response.IsBenefitEligible = false;
                    response.BenefitStatus = "INACTIVE";
                    response.Message = "Status kepegawaian HR sudah tidak aktif.";
                }

                return response;
            }

            // Kasus B: Dokter Saja (Bukan Karyawan)
            if (singleDoctor != null && singleEmployee == null)
            {
                var isDocActive = singleDoctor.IsActive && !singleDoctor.ResignDate.HasValue;
                response.PersonCategory = PersonCategories.Doctor;
                response.IdentityStatus = isDocActive ? WorkforceIdentityStatuses.Active : WorkforceIdentityStatuses.Inactive;
                response.IsIdentityVerified = true;
                response.IsActiveWorkforce = isDocActive;
                response.DoctorId = singleDoctor.Id;
                response.StaffNumber = singleDoctor.DoctorNumber;
                response.DoctorKind = FormatDoctorKind(singleDoctor.PracticeType);
                response.OrganizationUnitName = singleDoctor.PrimaryDepartment?.DepartmentName;
                response.PositionName = singleDoctor.SpecialistName ?? singleDoctor.PrimaryPosition?.PositionName;
                response.BenefitRelationship = BenefitRelationshipCodes.Self;
                response.BenefitOwnerWorkforceProfileId = singleDoctor.WorkforceProfileId;
                response.BenefitOwnerName = singleDoctor.FullName;

                if (!isDocActive)
                {
                    response.IsBenefitEligible = false;
                    response.BenefitStatus = "INACTIVE";
                    response.Message = "Dokter sudah tidak aktif di HR.";
                    return response;
                }

                // Dokter tetap berhak benefit hanya bila punya enrollment Medical aktif (kategori pasien tetap DOCTOR,
                // tidak dijadikan karyawan). Dokter tamu/mitra tidak otomatis berhak: kebijakan eligibility HR yang
                // menentukan, dan pemeriksaan di bawah menolaknya dengan alasan OWNER_TYPE_NOT_ALLOWED.
                await CheckEligibilitySafeAsync(response, singleDoctor.WorkforceProfileId, null, null, cancellationToken);
                if (!response.IsBenefitEligible && response.BenefitStatus == BenefitEligibilityReasonCodes.OwnerTypeNotAllowed)
                {
                    response.BenefitStatus = "NOT_ELIGIBLE";
                    response.Message = $"{response.DoctorKind} tidak memiliki kepesertaan benefit karyawan HR.";
                }
                else if (!response.IsBenefitEligible && response.BenefitStatus == BenefitEligibilityReasonCodes.EnrollmentNotActive)
                {
                    response.Message = "Enrollment benefit Medical dokter ini belum aktif di HR. Benefit tidak dapat dipakai sampai HR mengaktifkannya.";
                }
                return response;
            }

            // Kasus C: Karyawan Saja (Bukan Dokter)
            if (singleEmployee != null && singleDoctor == null)
            {
                var isEmpActive = singleEmployee.IsActive && !singleEmployee.ResignDate.HasValue;
                response.PersonCategory = PersonCategories.Employee;
                response.IdentityStatus = isEmpActive ? WorkforceIdentityStatuses.Active : WorkforceIdentityStatuses.Inactive;
                response.IsIdentityVerified = true;
                response.IsActiveWorkforce = isEmpActive;
                response.EmployeeId = singleEmployee.Id;
                response.StaffNumber = singleEmployee.EmployeeNumber;
                response.OrganizationUnitName = singleEmployee.PrimaryDepartment?.DepartmentName;
                response.PositionName = singleEmployee.PrimaryPosition?.PositionName;
                response.BenefitRelationship = BenefitRelationshipCodes.Self;
                response.BenefitOwnerWorkforceProfileId = singleEmployee.WorkforceProfileId;

                if (isEmpActive)
                {
                    await CheckEligibilitySafeAsync(response, singleEmployee.WorkforceProfileId, singleEmployee.Id, null, cancellationToken);
                }
                else
                {
                    response.IsBenefitEligible = false;
                    response.BenefitStatus = "INACTIVE";
                    response.Message = "Karyawan sudah tidak aktif / resign di HR.";
                }

                return response;
            }

            // Kasus D: Pasien Umum
            response.PersonCategory = PersonCategories.General;
            response.IdentityStatus = WorkforceIdentityStatuses.NotLinked;
            response.IsIdentityVerified = false;
            response.IsActiveWorkforce = false;
            response.IsBenefitEligible = false;
            response.BenefitStatus = "NOT_ELIGIBLE";
            response.Message = "Pasien umum (tidak terdaftar sebagai karyawan atau dokter rumah sakit).";
            return response;
        }

        private async Task CheckEligibilitySafeAsync(
            PatientWorkforceStatusResponse response,
            Guid? workforceProfileId,
            Guid? employeeId,
            Guid? familyMemberId,
            CancellationToken cancellationToken)
        {
            try
            {
                var operationalDate = AppDateTimeHelper.OperationalDate();
                var result = await _lookupService.CheckEligibilityAsync(
                    workforceProfileId, employeeId, familyMemberId, operationalDate, cancellationToken);

                response.IsBenefitEligible = result.IsEligible;
                response.BenefitStatus = result.IsEligible ? "ELIGIBLE" : (result.ReasonCode ?? "NOT_ELIGIBLE");
                response.Message = result.Message;
                if (result.IsEligible)
                {
                    response.BenefitPlanName = result.PlanName;
                    response.BenefitOwnerWorkforceProfileId = result.WorkforceProfileId ?? response.BenefitOwnerWorkforceProfileId;
                    response.BenefitOwnerType = result.OwnerType;
                    response.BenefitOwnerName ??= result.OwnerName;
                }
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync(LogCategory, "CheckEligibility",
                    $"Gagal memeriksa kelayakan HR untuk SDM {workforceProfileId ?? employeeId}", ex);
                response.BenefitStatus = WorkforceIdentityStatuses.Unavailable;
                response.IsBenefitEligible = false;
                response.Message = "Layanan HR tidak dapat diakses saat ini. Jalur pembayaran umum dapat tetap digunakan.";
            }
        }

        private async Task<string?> GetPositionNameAsync(
            QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs.WorkforceBenefitOwner owner, CancellationToken cancellationToken)
        {
            if (owner.EmployeeId.HasValue)
                return await _dbContext.MstEmployees.AsNoTracking()
                    .Where(x => x.Id == owner.EmployeeId.Value)
                    .Select(x => x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null)
                    .FirstOrDefaultAsync(cancellationToken);
            if (owner.DoctorId.HasValue)
                return await _dbContext.MstDoctors.AsNoTracking()
                    .Where(x => x.Id == owner.DoctorId.Value)
                    .Select(x => x.SpecialistName ?? (x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null))
                    .FirstOrDefaultAsync(cancellationToken);
            return null;
        }

        private static string FormatDoctorKind(DoctorPracticeType practiceType)
        {
            return practiceType switch
            {
                DoctorPracticeType.FullTime => "Dokter Tetap",
                DoctorPracticeType.PartTime => "Dokter Paruh Waktu",
                DoctorPracticeType.Visiting => "Dokter Tamu",
                DoctorPracticeType.Guest => "Dokter Tamu",
                DoctorPracticeType.Consultant => "Dokter Konsultan",
                DoctorPracticeType.OnCall => "Dokter On-Call",
                DoctorPracticeType.Telemedicine => "Dokter Telemedicine",
                _ => "Dokter Mitra"
            };
        }

        private static string? NormalizeNik(string? nik)
        {
            if (string.IsNullOrWhiteSpace(nik)) return null;
            var clean = Regex.Replace(nik.Trim(), @"\D", "");
            return clean.Length > 0 ? clean : null;
        }

        private static string MaskNik(string? nik)
        {
            if (string.IsNullOrWhiteSpace(nik)) return "-";
            var clean = nik.Trim();
            if (clean.Length <= 8) return clean;
            return $"{clean[..4]}••••••••{clean[^4..]}";
        }
    }
}
