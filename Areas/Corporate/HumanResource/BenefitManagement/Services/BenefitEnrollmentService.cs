using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Layanan Administrasi Kepesertaan Asuransi & Tanggungan Keluarga ESS Pegawai RS (Slice S-D3).
    /// Mengimplementasikan penegakan aturan bisnis BE-HRD-181 s.d. BE-HRD-184:
    /// - BEN-B1 & VR-BEN-11..13: Aktivasi cepat BPJS TK H+0 Onboarding & aktivasi bertahap pasca-lulus probation.
    /// - BEN-B2 & VR-BEN-01..02: Kuota keluarga inti sah (maksimal 1 pasangan sah + 3 anak sah).
    /// - BEN-B3 & VR-BEN-05..08: Batas usia anak non-kuliah <= 21 th dan kuliah <= 25 th ber-surat dekanat.
    /// - VR-BEN-03..04: Validasi format 16 digit NIK & Nomor Kartu Keluarga.
    /// - VR-BEN-09..10: Validasi format 13 digit BPJS Kesehatan & 11 digit BPJS Ketenagakerjaan.
    /// - VR-BEN-25 & AUDIT-BEN-05: Meja kerja verifikasi berkas fisik kependudukan HR & alasan penolakan min 15 char.
    /// </summary>
    public sealed class BenefitEnrollmentService : IBenefitEnrollmentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<BenefitEnrollmentService> _logger;
        private readonly NumberSeriesAllocator _numberAllocator;

        private static readonly Regex NikRegex = new(@"^[0-9]{16}$", RegexOptions.Compiled);
        private static readonly Regex BpjsKesRegex = new(@"^[0-9]{13}$", RegexOptions.Compiled);
        private static readonly Regex BpjsTkRegex = new(@"^[0-9]{11}$", RegexOptions.Compiled);

        public BenefitEnrollmentService(ApplicationDbContext dbContext, ILogger<BenefitEnrollmentService> logger, NumberSeriesAllocator numberAllocator)
        {
            _dbContext = dbContext;
            _logger = logger;
            _numberAllocator = numberAllocator;
        }

        // =========================================================================
        // 1. ADMINISTRASI KEPESERTAAN ASURANSI PEGAWAI RS (BE-HRD-182)
        // =========================================================================

        public async Task<PagedResult<EmployeeEnrollmentListItemDto>> GetEnrollmentsPagedAsync(
            EmployeeEnrollmentListQuery query, CancellationToken cancellationToken)
        {
            var baseQuery = _dbContext.TrxEmployeeInsuranceEnrollments.AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.BenefitPlan)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                baseQuery = baseQuery.Where(x => x.EnrollmentStatus == query.Status);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchKeyword))
            {
                var kw = query.SearchKeyword.Trim().ToLower();
                baseQuery = baseQuery.Where(x =>
                    (x.Employee != null && x.Employee.FullName.ToLower().Contains(kw)) ||
                    (x.Employee != null && x.Employee.EmployeeNumber.ToLower().Contains(kw)) ||
                    x.InsuranceEnrollmentNumber.ToLower().Contains(kw) ||
                    (x.BpjsKesehatanNumber != null && x.BpjsKesehatanNumber.Contains(kw)) ||
                    (x.BpjsKetenagakerjaanNumber != null && x.BpjsKetenagakerjaanNumber.Contains(kw)) ||
                    (x.PrivateInsurancePolicyNumber != null && x.PrivateInsurancePolicyNumber.ToLower().Contains(kw)));
            }

            var totalItems = await baseQuery.CountAsync(cancellationToken);

            var items = await baseQuery
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new EmployeeEnrollmentListItemDto
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeId = x.EmployeeId,
                    EmployeeCode = x.Employee != null ? x.Employee.EmployeeNumber : (x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : "-"),
                    EmployeeName = x.Employee != null ? x.Employee.FullName : (x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : "-"),
                    JobTitle = "-",
                    DepartmentName = "-",
                    EmploymentStatus = x.Employee != null ? (x.Employee.GradeLevel ?? "Permanent") : "Permanent",
                    JoinDate = x.Employee != null ? x.Employee.JoinDate : null,
                    BenefitPlanName = x.BenefitPlan != null ? x.BenefitPlan.BenefitPlanName : x.ProviderName,
                    CoverageTier = x.BenefitPlan != null ? x.BenefitPlan.CoverageTier : "Standard",
                    BpjsKetenagakerjaanNumber = x.BpjsKetenagakerjaanNumber,
                    BpjsKesehatanNumber = x.BpjsKesehatanNumber,
                    PrivateInsurancePolicyNumber = x.PrivateInsurancePolicyNumber ?? x.PolicyNumber,
                    EnrollmentStatus = x.EnrollmentStatus,
                    EffectiveStartDate = x.EffectiveStartDate,
                    ActiveDependentCount = x.CoveredDependentCount,
                    IsProbation = x.EnrollmentStatus == "PendingActivation"
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<EmployeeEnrollmentListItemDto>
            {
                PageNumber = query.Page,
                PageSize = query.PageSize,
                TotalData = totalItems,
                TotalPage = (int)Math.Ceiling((double)totalItems / query.PageSize),
                Items = items
            };
        }

        public async Task<BenefitEnrollmentDetailDto> GetEnrollmentDetailAsync(Guid id, CancellationToken cancellationToken)
        {
            var enrollment = await _dbContext.TrxEmployeeInsuranceEnrollments.AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.BenefitPlan)
                .Include(x => x.EmployeeBenefitEnrollment)
                    .ThenInclude(e => e.Dependents.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Kepesertaan asuransi dengan ID '{id}' tidak ditemukan.");
            }

            var dependents = enrollment.EmployeeBenefitEnrollment?.Dependents
                .Select(d => MapDependentDetail(d))
                .ToList() ?? new List<BenefitDependentDetailDto>();

            return new BenefitEnrollmentDetailDto
            {
                Id = enrollment.Id,
                EmployeeBenefitEnrollmentId = enrollment.EmployeeBenefitEnrollmentId,
                WorkforceProfileId = enrollment.WorkforceProfileId,
                EmployeeId = enrollment.EmployeeId,
                EmployeeCode = enrollment.Employee?.EmployeeNumber ?? enrollment.WorkforceProfile?.ProfileCode ?? "-",
                EmployeeName = enrollment.Employee?.FullName ?? enrollment.WorkforceProfile?.DisplayName ?? "-",
                DepartmentName = "-",
                JobTitle = "-",
                BenefitPlanId = enrollment.BenefitPlanId,
                BenefitPlanCode = enrollment.BenefitPlan?.BenefitPlanCode ?? "-",
                BenefitPlanName = enrollment.BenefitPlan?.BenefitPlanName ?? enrollment.ProviderName,
                BenefitCategory = enrollment.BenefitPlan?.BenefitCategory ?? "BPJS_Mandatory",
                CoverageTier = enrollment.BenefitPlan?.CoverageTier ?? "Basic",
                BpjsKetenagakerjaanNumber = enrollment.BpjsKetenagakerjaanNumber,
                BpjsKesehatanNumber = enrollment.BpjsKesehatanNumber,
                PrivateInsurancePolicyNumber = enrollment.PrivateInsurancePolicyNumber ?? enrollment.PolicyNumber,
                ProviderName = enrollment.ProviderName,
                EnrollmentStatus = enrollment.EnrollmentStatus,
                EffectiveStartDate = enrollment.EffectiveStartDate,
                EffectiveEndDate = enrollment.EffectiveEndDate,
                ProbationEvaluationId = enrollment.ProbationEvaluationId,
                CoveredDependentCount = dependents.Count(d => d.VerificationStatus == "Verified"),
                Dependents = dependents
            };
        }

        public async Task<BenefitEnrollmentDetailDto> ActivateProbationEnrollmentAsync(
            Guid id, ActivateProbationRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            if (request.ProbationEvaluationId == Guid.Empty)
            {
                throw new InvalidOperationException("Aktivasi paket benefit ditolak. SK pengangkatan atau kelulusan masa percobaan 3 bulan tidak ditemukan (VR-BEN-13).");
            }

            var enrollment = await _dbContext.TrxEmployeeInsuranceEnrollments
                .Include(x => x.EmployeeBenefitEnrollment)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Kepesertaan asuransi dengan ID '{id}' tidak ditemukan.");
            }

            var effectiveDate = request.EffectiveDate ?? DateTime.UtcNow;

            // Aktivasi paket kesehatan pasca-lulus masa percobaan 3 bulan (BEN-B1, VR-BEN-13)
            enrollment.EnrollmentStatus = "Active";
            enrollment.EffectiveStartDate = effectiveDate;
            enrollment.ProbationEvaluationId = request.ProbationEvaluationId;
            enrollment.Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? enrollment.Notes
                : (enrollment.Notes + " | " + request.Notes);
            enrollment.VerifiedAt = DateTime.UtcNow;
            enrollment.VerifiedByUserId = actorUserId;
            enrollment.UpdateDateTime = DateTime.UtcNow;
            enrollment.UpdateBy = actorUserId;

            // Sinkronkan ke parent enrollment bila ada
            if (enrollment.EmployeeBenefitEnrollment != null)
            {
                enrollment.EmployeeBenefitEnrollment.EnrollmentStatus = "Active";
                enrollment.EmployeeBenefitEnrollment.ActivatedAt = DateTime.UtcNow;
                enrollment.EmployeeBenefitEnrollment.ActivatedByUserId = actorUserId;
                enrollment.EmployeeBenefitEnrollment.EffectiveStartDate = effectiveDate;
                enrollment.EmployeeBenefitEnrollment.UpdateDateTime = DateTime.UtcNow;
                enrollment.EmployeeBenefitEnrollment.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT-BEN-03] Paket asuransi kesehatan staf ID '{WorkforceProfileId}' diaktifkan pasca-evaluasi '{ProbationEvaluationId}' oleh user '{UserId}'.",
                enrollment.WorkforceProfileId, request.ProbationEvaluationId, actorUserId);

            return await GetEnrollmentDetailAsync(id, cancellationToken);
        }

        public async Task<BenefitEnrollmentDetailDto> UpdatePolicyNumbersAsync(
            Guid id, UpdatePolicyNumbersRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var enrollment = await _dbContext.TrxEmployeeInsuranceEnrollments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Kepesertaan asuransi dengan ID '{id}' tidak ditemukan.");
            }

            // Validasi format nomor BPJS Kesehatan (VR-BEN-09: 13 digit)
            if (!string.IsNullOrWhiteSpace(request.BpjsKesehatanNumber))
            {
                var bpjsKes = request.BpjsKesehatanNumber.Trim();
                if (!BpjsKesRegex.IsMatch(bpjsKes))
                {
                    throw new InvalidOperationException("Nomor kartu BPJS Kesehatan tidak valid. Nomor kepesertaan BPJS Kesehatan wajib 13 digit angka (VR-BEN-09).");
                }
                enrollment.BpjsKesehatanNumber = bpjsKes;
            }

            // Validasi format nomor BPJS Ketenagakerjaan (VR-BEN-10: 11 digit)
            if (!string.IsNullOrWhiteSpace(request.BpjsKetenagakerjaanNumber))
            {
                var bpjsTk = request.BpjsKetenagakerjaanNumber.Trim();
                if (!BpjsTkRegex.IsMatch(bpjsTk))
                {
                    throw new InvalidOperationException("Nomor kartu BPJS Ketenagakerjaan tidak valid. Nomor KPJ BPJS TK wajib 11 digit angka (VR-BEN-10).");
                }
                enrollment.BpjsKetenagakerjaanNumber = bpjsTk;
            }

            if (!string.IsNullOrWhiteSpace(request.PrivateInsurancePolicyNumber))
            {
                enrollment.PrivateInsurancePolicyNumber = request.PrivateInsurancePolicyNumber.Trim();
                enrollment.PolicyNumber = request.PrivateInsurancePolicyNumber.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.PrivateInsuranceProviderName))
            {
                enrollment.ProviderName = request.PrivateInsuranceProviderName.Trim();
            }

            enrollment.UpdateDateTime = DateTime.UtcNow;
            enrollment.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT-BEN-04] Pembaruan nomor polis BPJS/Swasta enrollment '{Id}' oleh user '{UserId}'.",
                id, actorUserId);

            return await GetEnrollmentDetailAsync(id, cancellationToken);
        }

        // =========================================================================
        // 2. PORTAL ESS PENDAFTARAN TANGGUNGAN & KUOTA KELUARGA (BE-HRD-181, 183)
        // =========================================================================

        public async Task<List<EmployeeDependentSummaryDto>> GetMyFamilyDependentsAsync(
            Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var dependents = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            return dependents.Select(MapDependentSummary).ToList();
        }

        public async Task<EmployeeFamilyQuotaSummaryDto> GetMyFamilyQuotaSummaryAsync(
            Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var dependents = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.VerificationStatus != "Rejected")
                .ToListAsync(cancellationToken);

            var spouseCount = dependents.Count(x => string.Equals(x.RelationshipType, "Spouse", StringComparison.OrdinalIgnoreCase));
            var childCount = dependents.Count(x => string.Equals(x.RelationshipType, "Child", StringComparison.OrdinalIgnoreCase));
            var verifiedCount = dependents.Count(x => x.VerificationStatus == "Verified");
            var pendingCount = dependents.Count(x => x.VerificationStatus == "Submitted" || x.VerificationStatus == "Draft");

            return new EmployeeFamilyQuotaSummaryDto
            {
                MaxSpouse = 1,
                CurrentSpouseCount = spouseCount,
                CanAddSpouse = spouseCount < 1,

                MaxChild = 3,
                CurrentChildCount = childCount,
                CanAddChild = childCount < 3,

                TotalDependents = dependents.Count,
                VerifiedDependents = verifiedCount,
                PendingVerificationCount = pendingCount
            };
        }

        public async Task<EmployeeDependentSummaryDto> CreateEssDependentAsync(
            Guid workforceProfileId, CreateEssDependentRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            // 1. Validasi relasi keluarga (Hanya Spouse atau Child)
            var rel = request.RelationshipType.Trim();
            if (!string.Equals(rel, "Spouse", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(rel, "Child", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Pendaftaran anggota keluarga ditolak. Hanya pasangan sah (Spouse) dan anak sah (Child) yang berhak didaftarkan sebagai tanggungan (BEN-B2).");
            }

            var normalizedRel = string.Equals(rel, "Spouse", StringComparison.OrdinalIgnoreCase) ? "Spouse" : "Child";

            // 2. Validasi format NIK & Nomor Kartu Keluarga (VR-BEN-03, VR-BEN-04: tepat 16 digit angka)
            var nik = request.NationalIdNumber.Trim();
            if (!NikRegex.IsMatch(nik))
            {
                throw new InvalidOperationException("Format NIK tidak valid. NIK wajib terdiri dari tepat 16 digit angka kependudukan (VR-BEN-03).");
            }

            var noKk = request.FamilyCardNumber.Trim();
            if (!NikRegex.IsMatch(noKk))
            {
                throw new InvalidOperationException("Nomor Kartu Keluarga tidak valid. Nomor KK wajib terdiri dari tepat 16 digit angka (VR-BEN-04).");
            }

            // 3. Cek duplikasi NIK pada tanggungan aktif/terbuka
            var existingNik = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                .AnyAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete
                    && (x.NationalIdNumber == nik || x.IdentityNumber == nik)
                    && x.VerificationStatus != "Rejected", cancellationToken);
            if (existingNik)
            {
                throw new InvalidOperationException($"Anggota keluarga dengan NIK '{nik}' sudah terdaftar dalam sistem.");
            }

            // 4. Penegakan Kuota Pasangan (VR-BEN-01: Maks 1 orang)
            if (normalizedRel == "Spouse")
            {
                var existingSpouseCount = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete
                        && x.RelationshipType == "Spouse" && x.VerificationStatus != "Rejected", cancellationToken);

                if (existingSpouseCount >= 1)
                {
                    throw new InvalidOperationException("Pendaftaran pasangan ditolak. Pegawai hanya berhak mendaftarkan maksimal 1 pasangan sah (VR-BEN-01, BEN-B2).");
                }
            }

            // 5. Penegakan Kuota Anak (VR-BEN-02: Maks 3 orang)
            if (normalizedRel == "Child")
            {
                var existingChildCount = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .CountAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete
                        && x.RelationshipType == "Child" && x.VerificationStatus != "Rejected", cancellationToken);

                if (existingChildCount >= 3)
                {
                    throw new InvalidOperationException("Pendaftaran anak ditolak. Kuota pertanggungan anak sah maksimal adalah 3 orang (VR-BEN-02, BEN-B2).");
                }
            }

            // 6. Penegakan Batas Usia Anak (BEN-B3, VR-BEN-05..08)
            var birthDate = request.BirthDate;
            var today = DateTime.UtcNow.Date;
            var ageYears = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-ageYears)) ageYears--;

            if (normalizedRel == "Child")
            {
                // VR-BEN-05: Non-mahasiswa maksimal 21 tahun
                if (!request.IsActiveStudent && ageYears > 21)
                {
                    throw new InvalidOperationException("Pendaftaran anak ditolak. Batas usia tanggungan anak non-mahasiswa adalah maksimal 21 tahun (VR-BEN-05, BEN-B3).");
                }

                // VR-BEN-06: Mahasiswa aktif maksimal 25 tahun 0 bulan
                if (request.IsActiveStudent && ageYears > 25)
                {
                    throw new InvalidOperationException("Pendaftaran anak ditolak. Batas usia maksimal anak berstatus mahasiswa aktif adalah 25 tahun (VR-BEN-06, BEN-B3).");
                }

                // VR-BEN-07: Mandatory surat dekanat perguruan tinggi jika usia > 21 tahun
                if (ageYears > 21 && string.IsNullOrWhiteSpace(request.StudentVerificationLetterPath))
                {
                    throw new InvalidOperationException("Surat Keterangan Mahasiswa Aktif dari perguruan tinggi wajib dilampirkan untuk anak usia di atas 21 tahun (VR-BEN-07).");
                }

                // VR-BEN-08: Masa berlaku surat kuliah tidak boleh kedaluwarsa (maks 31 Agustus tahun berjalan)
                if (request.StudentVerificationExpiryDate.HasValue)
                {
                    if (request.StudentVerificationExpiryDate.Value.Date < today)
                    {
                        throw new InvalidOperationException("Masa berlaku surat keterangan kuliah telah kedaluwarsa. Harap unggah surat keterangan mahasiswa baru (VR-BEN-08).");
                    }
                }
            }

            // Cari atau kaitkan dengan enrollment induk aktif pegawai
            var parentEnrollment = await _dbContext.TrxEmployeeBenefitEnrollments
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            var enrollmentId = parentEnrollment?.Id ?? Guid.NewGuid();

            if (parentEnrollment == null)
            {
                // Buat placeholder parent enrollment jika belum ada. Nomor kepesertaan memakai deret yang sama
                // dengan EmployeeBenefitEnrollmentService (BEN-000001), bukan angka acak (QBE-CODE-003).
                var enrollmentNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                    SequenceKey: BenefitNumberSeries.EnrollmentKey,
                    Prefix: BenefitNumberSeries.EnrollmentPrefix,
                    ResetPolicy: BenefitNumberSeries.EnrollmentResetPolicy,
                    SequenceDigits: BenefitNumberSeries.EnrollmentDigits,
                    ActorUserId: actorUserId == Guid.Empty ? BenefitNumberSeries.SystemFallbackUserId : actorUserId,
                    Instant: DateTimeOffset.UtcNow), cancellationToken);

                parentEnrollment = new TrxEmployeeBenefitEnrollment
                {
                    Id = enrollmentId,
                    WorkforceProfileId = workforceProfileId,
                    EnrollmentNumber = enrollmentNumber,
                    EnrollmentStatus = "Submitted",
                    EnrollmentDate = DateTime.UtcNow,
                    EffectiveStartDate = DateTime.UtcNow,
                    CoverageLevel = "Family",
                    MaximumDependents = 4,
                    CurrencyCode = "IDR",
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.TrxEmployeeBenefitEnrollments.Add(parentEnrollment);
            }

            var dependent = new TrxEmployeeBenefitDependent
            {
                Id = Guid.NewGuid(),
                EmployeeBenefitEnrollmentId = enrollmentId,
                WorkforceProfileId = workforceProfileId,
                DependentName = request.DependentName.Trim(),
                NationalIdNumber = nik,
                IdentityNumber = nik,
                FamilyCardNumber = noKk,
                RelationshipType = normalizedRel,
                BirthDate = request.BirthDate,
                GenderSnapshot = request.Gender ?? "Unknown",
                IsActiveStudent = request.IsActiveStudent,
                StudentVerificationLetterPath = request.StudentVerificationLetterPath,
                StudentVerificationExpiryDate = request.StudentVerificationExpiryDate,
                VerificationStatus = "Submitted", // Masuk antrean verifikasi HR
                DependentStatus = "Requested",
                RequestedAt = DateTime.UtcNow,
                RequestedByUserId = actorUserId,
                VerificationNotes = request.Notes,
                IsEligible = false, // Belum diverifikasi
                IsCovered = false,
                CoverageLimitAmount = 0m,
                RemainingAmount = 0m,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.TrxEmployeeBenefitDependents.Add(dependent);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT-BEN-01] Pendaftaran tanggungan ESS '{DependentName}' ({Relationship}) berhasil diajukan oleh staf '{WorkforceProfileId}'.",
                dependent.DependentName, normalizedRel, workforceProfileId);

            return MapDependentSummary(dependent);
        }

        // =========================================================================
        // 3. MEJA KERJA VERIFIKASI BERKAS FISIK KEPENDUDUKAN HR (BE-HRD-184)
        // =========================================================================

        public async Task<PagedResult<PendingDependentVerificationDto>> GetPendingDependentVerificationsAsync(
            PendingDependentVerificationQuery query, CancellationToken cancellationToken)
        {
            var baseQuery = _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x => !x.IsDelete && (x.VerificationStatus == "Submitted" || x.DependentStatus == "Requested"));

            if (!string.IsNullOrWhiteSpace(query.SearchKeyword))
            {
                var kw = query.SearchKeyword.Trim().ToLower();
                baseQuery = baseQuery.Where(x =>
                    x.DependentName.ToLower().Contains(kw) ||
                    (x.NationalIdNumber != null && x.NationalIdNumber.Contains(kw)) ||
                    (x.FamilyCardNumber != null && x.FamilyCardNumber.Contains(kw)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(kw)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.ProfileCode.ToLower().Contains(kw)));
            }

            var totalItems = await baseQuery.CountAsync(cancellationToken);

            var items = await baseQuery
                .OrderBy(x => x.RequestedAt ?? x.CreateDateTime)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new PendingDependentVerificationDto
                {
                    Id = x.Id,
                    EmployeeBenefitEnrollmentId = x.EmployeeBenefitEnrollmentId,
                    EmployeeName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : "-",
                    EmployeeCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : "-",
                    DepartmentName = "-",
                    DependentName = x.DependentName,
                    NationalIdNumber = x.NationalIdNumber ?? x.IdentityNumber ?? "-",
                    FamilyCardNumber = x.FamilyCardNumber ?? "-",
                    RelationshipType = x.RelationshipType,
                    BirthDate = x.BirthDate,
                    AgeYears = x.BirthDate.HasValue ? (DateTime.UtcNow.Year - x.BirthDate.Value.Year) : 0,
                    IsActiveStudent = x.IsActiveStudent,
                    StudentVerificationLetterPath = x.StudentVerificationLetterPath,
                    StudentVerificationExpiryDate = x.StudentVerificationExpiryDate,
                    VerificationStatus = x.VerificationStatus ?? x.DependentStatus,
                    RequestedAt = x.RequestedAt ?? x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<PendingDependentVerificationDto>
            {
                PageNumber = query.Page,
                PageSize = query.PageSize,
                TotalData = totalItems,
                TotalPage = (int)Math.Ceiling((double)totalItems / query.PageSize),
                Items = items
            };
        }

        public async Task<BenefitDependentDetailDto> VerifyDependentAsync(
            Guid dependentId, VerifyDependentRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var dependent = await _dbContext.TrxEmployeeBenefitDependents
                .Include(x => x.EmployeeBenefitEnrollment)
                .FirstOrDefaultAsync(x => x.Id == dependentId && !x.IsDelete, cancellationToken);

            if (dependent == null)
            {
                throw new KeyNotFoundException($"Berkas tanggungan dengan ID '{dependentId}' tidak ditemukan.");
            }

            var isApproval = string.Equals(request.VerificationStatus, "Verified", StringComparison.OrdinalIgnoreCase);
            var isRejection = string.Equals(request.VerificationStatus, "Rejected", StringComparison.OrdinalIgnoreCase);

            if (!isApproval && !isRejection)
            {
                throw new InvalidOperationException("Status verifikasi tidak valid. Pilihan hanya 'Verified' atau 'Rejected'.");
            }

            if (isRejection)
            {
                // VR-BEN-25: Alasan penolakan minimal 15 karakter
                var reason = (request.RejectionReason ?? string.Empty).Trim();
                if (reason.Length < 15)
                {
                    throw new InvalidOperationException("Alasan penolakan dokumen kependudukan wajib diisi secara rinci agar pegawai dapat melakukan perbaikan (minimal 15 karakter, VR-BEN-25).");
                }

                dependent.VerificationStatus = "Rejected";
                dependent.DependentStatus = "Rejected";
                dependent.RejectionReason = reason;
                dependent.IsEligible = false;
                dependent.IsCovered = false;
                dependent.VerifiedAt = DateTime.UtcNow;
                dependent.VerifiedByUserId = actorUserId;
                dependent.VerificationNotes = request.VerificationNotes;
                dependent.UpdateDateTime = DateTime.UtcNow;
                dependent.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "[AUDIT-BEN-05] Verifikasi tanggungan '{DependentId}' DITOLAK oleh user '{UserId}'. Alasan: {Reason}",
                    dependentId, actorUserId, reason);

                return MapDependentDetail(dependent);
            }

            // Jalur Persetujuan (Verified)
            var limit = request.CoverageLimitAmount ?? 10000000m; // Default pagu Rp 10.000.000
            dependent.VerificationStatus = "Verified";
            dependent.DependentStatus = "Verified";
            dependent.IsEligible = true;
            dependent.IsCovered = true;
            dependent.CoverageLimitAmount = limit;
            dependent.RemainingAmount = limit;
            dependent.UsedAmount = 0m;
            dependent.EffectiveStartDate = DateTime.UtcNow;
            dependent.VerifiedAt = DateTime.UtcNow;
            dependent.VerifiedByUserId = actorUserId;
            dependent.VerificationNotes = request.VerificationNotes;
            dependent.RejectionReason = null;
            dependent.UpdateDateTime = DateTime.UtcNow;
            dependent.UpdateBy = actorUserId;

            // Update counter tanggungan aktif di parent enrollment
            if (dependent.EmployeeBenefitEnrollment != null)
            {
                var activeCount = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .CountAsync(x => x.EmployeeBenefitEnrollmentId == dependent.EmployeeBenefitEnrollmentId
                        && !x.IsDelete && (x.VerificationStatus == "Verified" || x.Id == dependent.Id), cancellationToken);

                dependent.EmployeeBenefitEnrollment.ActiveDependentCount = activeCount;
                dependent.EmployeeBenefitEnrollment.UpdateDateTime = DateTime.UtcNow;
                dependent.EmployeeBenefitEnrollment.UpdateBy = actorUserId;
            }

            // Update juga counter di insurance enrollment jika ada
            var insEnrollment = await _dbContext.TrxEmployeeInsuranceEnrollments
                .FirstOrDefaultAsync(x => x.EmployeeBenefitEnrollmentId == dependent.EmployeeBenefitEnrollmentId && !x.IsDelete, cancellationToken);
            if (insEnrollment != null)
            {
                insEnrollment.CoveredDependentCount += 1;
                insEnrollment.UpdateDateTime = DateTime.UtcNow;
                insEnrollment.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT-BEN-05] Verifikasi tanggungan '{DependentName}' ({Relationship}) DISETUJUI oleh user '{UserId}' dengan pagu Rp {Limit:N0}.",
                dependent.DependentName, dependent.RelationshipType, actorUserId, limit);

            return MapDependentDetail(dependent);
        }

        // =========================================================================
        // 4. PEMETAAN DTOS
        // =========================================================================

        private static EmployeeDependentSummaryDto MapDependentSummary(TrxEmployeeBenefitDependent d)
        {
            var today = DateTime.UtcNow.Date;
            var age = 0;
            if (d.BirthDate.HasValue)
            {
                age = today.Year - d.BirthDate.Value.Year;
                if (d.BirthDate.Value.Date > today.AddYears(-age)) age--;
            }

            return new EmployeeDependentSummaryDto
            {
                Id = d.Id,
                DependentName = d.DependentName,
                NationalIdNumber = d.NationalIdNumber ?? d.IdentityNumber ?? string.Empty,
                FamilyCardNumber = d.FamilyCardNumber ?? string.Empty,
                RelationshipType = d.RelationshipType,
                BirthDate = d.BirthDate,
                AgeYears = age,
                Gender = d.GenderSnapshot,
                IsActiveStudent = d.IsActiveStudent,
                StudentVerificationLetterPath = d.StudentVerificationLetterPath,
                StudentVerificationExpiryDate = d.StudentVerificationExpiryDate,
                VerificationStatus = d.VerificationStatus ?? d.DependentStatus,
                CoverageLimitAmount = d.CoverageLimitAmount,
                RemainingAmount = d.RemainingAmount,
                RejectionReason = d.RejectionReason
            };
        }

        private static BenefitDependentDetailDto MapDependentDetail(TrxEmployeeBenefitDependent d)
        {
            var today = DateTime.UtcNow.Date;
            var age = 0;
            if (d.BirthDate.HasValue)
            {
                age = today.Year - d.BirthDate.Value.Year;
                if (d.BirthDate.Value.Date > today.AddYears(-age)) age--;
            }

            return new BenefitDependentDetailDto
            {
                Id = d.Id,
                EmployeeBenefitEnrollmentId = d.EmployeeBenefitEnrollmentId,
                WorkforceProfileId = d.WorkforceProfileId,
                DependentName = d.DependentName,
                NationalIdNumber = d.NationalIdNumber ?? d.IdentityNumber ?? string.Empty,
                FamilyCardNumber = d.FamilyCardNumber ?? string.Empty,
                RelationshipType = d.RelationshipType,
                BirthDate = d.BirthDate,
                AgeYears = age,
                GenderSnapshot = d.GenderSnapshot,
                IsActiveStudent = d.IsActiveStudent,
                StudentVerificationLetterPath = d.StudentVerificationLetterPath,
                StudentVerificationExpiryDate = d.StudentVerificationExpiryDate,
                VerificationStatus = d.VerificationStatus ?? d.DependentStatus,
                CoverageLimitAmount = d.CoverageLimitAmount,
                RemainingAmount = d.RemainingAmount,
                IsEligible = d.IsEligible,
                IsCovered = d.IsCovered,
                VerifiedAt = d.VerifiedAt,
                VerifiedBy = d.VerifiedByUser?.UserName,
                RejectionReason = d.RejectionReason,
                VerificationNotes = d.VerificationNotes
            };
        }
    }
}
