using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class RetirementService
    {
        /// <summary>Kode kebijakan yang dikembalikan bila usia pensiun normal RS belum ditetapkan (BE-HRD-368).</summary>
        public const string PolicyNotConfiguredCode = "POLICY_NOT_CONFIGURED";

        // Jendela tampilan "mendekati pensiun": tiga tahun sebelum tanggal pensiun proyeksi, sama dengan
        // perilaku lama (usia 55 untuk pensiun 58). Ini rentang tampilan, bukan usia pensiun.
        private const int ApproachingWindowYears = 3;

        private const string RetirementSequenceKey = "HRD_LC_RETIREMENT";
        private const string RetirementPrefix = "RET";
        private const string ExitClearanceSequenceKey = "HRD_LC_EXIT_CLEARANCE";
        private const string ExitClearancePrefix = "EXC";
        private const int SequenceDigits = 4;

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        public RetirementService(ApplicationDbContext dbContext, NumberSeriesAllocator numberSeriesAllocator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _numberSeriesAllocator = numberSeriesAllocator ?? throw new ArgumentNullException(nameof(numberSeriesAllocator));
        }

        /// <summary>
        /// Usia pensiun normal per rumah sakit dari pengaturan siklus kepegawaian (BE-HRD-368).
        /// Kosong bila belum ada satu pun RS yang menetapkannya.
        /// </summary>
        private async Task<Dictionary<Guid, int>> LoadRetirementPolicyBySiteAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.HrdEmploymentLifecycleSettings
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.NormalRetirementAge != null)
                .ToDictionaryAsync(x => x.HospitalSiteId, x => x.NormalRetirementAge!.Value, cancellationToken);
        }

        /// <summary>
        /// Usia pensiun yang berlaku untuk satu orang: RS penempatan utamanya; bila penempatannya tidak
        /// diketahui dan hanya ada satu nilai kebijakan, nilai itu yang dipakai. Selain itu <c>null</c>.
        /// </summary>
        private static int? ResolveAgeForSite(Dictionary<Guid, int> policyBySite, Guid? hospitalSiteId)
        {
            if (hospitalSiteId.HasValue && policyBySite.TryGetValue(hospitalSiteId.Value, out var siteAge))
            {
                return siteAge;
            }

            var distinctAges = policyBySite.Values.Distinct().ToList();
            return distinctAges.Count == 1 ? distinctAges[0] : null;
        }

        private async Task<Dictionary<Guid, Guid?>> LoadPrimarySiteByProfileAsync(
            IReadOnlyCollection<Guid> workforceProfileIds,
            CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;
            var rows = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x => workforceProfileIds.Contains(x.WorkforceProfileId) &&
                            x.IsActive && !x.IsDelete &&
                            x.EffectiveStartDate <= today &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Select(x => new { x.WorkforceProfileId, x.HospitalSiteId })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(g => g.Key, g => (Guid?)g.First().HospitalSiteId);
        }

        private Task<string> AllocateNumberAsync(string sequenceKey, string prefix, string resetPolicy, Guid actorUserId, CancellationToken cancellationToken)
        {
            return _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: sequenceKey,
                    Prefix: prefix,
                    ResetPolicy: resetPolicy,
                    SequenceDigits: SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);
        }

        public RetirementFilterMetadataResponse GetFilterMetadata()
        {
            return new RetirementFilterMetadataResponse();
        }

        public async Task<RetirementSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var all = await _dbContext.TrxRetirements
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .Select(x => x.RetirementStatus)
                .ToListAsync(cancellationToken);

            // BE-HRD-368: hitungan "mendekati pensiun" memakai usia pensiun kebijakan RS, bukan angka tertanam.
            var policyBySite = await LoadRetirementPolicyBySiteAsync(cancellationToken);
            var approachingCount = 0;
            int? singlePolicyAge = null;
            if (policyBySite.Count > 0)
            {
                var eligible = await ComputeApproachingAsync(policyBySite, null, int.MaxValue, cancellationToken);
                approachingCount = eligible.Count;
                var distinctAges = policyBySite.Values.Distinct().ToList();
                singlePolicyAge = distinctAges.Count == 1 ? distinctAges[0] : null;
            }

            return new RetirementSummaryResponse
            {
                TotalData = all.Count,
                Planned = all.Count(x => string.Equals(x, "Planned", StringComparison.OrdinalIgnoreCase)),
                Verified = all.Count(x => string.Equals(x, "Verified", StringComparison.OrdinalIgnoreCase)),
                Approved = all.Count(x => string.Equals(x, "Approved", StringComparison.OrdinalIgnoreCase)),
                HandoffCompleted = all.Count(x => string.Equals(x, "HandoffCompleted", StringComparison.OrdinalIgnoreCase)),
                Cancelled = all.Count(x => string.Equals(x, "Cancelled", StringComparison.OrdinalIgnoreCase)),
                ApproachingRetirementCount = approachingCount,
                RetirementPolicyStatus = policyBySite.Count > 0 ? "CONFIGURED" : PolicyNotConfiguredCode,
                NormalRetirementAge = singlePolicyAge
            };
        }

        public async Task<PagedResult<RetirementListResponse>> GetPagedAsync(
            string? status,
            string? search,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber = 1,
            int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxRetirements
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryPosition)
                .Include(x => x.ApprovedByUser)
                .Where(x => !x.IsDelete && x.IsActive);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.RetirementStatus.ToLower() == status.Trim().ToLower());
            }

            if (startDate.HasValue)
            {
                query = query.Where(x => x.NormalRetirementDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.NormalRetirementDate <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RetirementNumber.ToLower().Contains(term) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(term)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.ProfileCode.ToLower().Contains(term)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.Employee != null && x.WorkforceProfile.Employee.EmployeeNumber.ToLower().Contains(term)));
            }

            var totalItems = await query.CountAsync(cancellationToken);
            var safePageNumber = Math.Max(1, pageNumber);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .Select(x => new RetirementListResponse
                {
                    Id = x.Id,
                    RetirementNumber = x.RetirementNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeId = x.EmployeeId ?? (x.WorkforceProfile != null && x.WorkforceProfile.Employee != null ? x.WorkforceProfile.Employee.Id : null),
                    EmployeeName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    EmployeeNip = x.WorkforceProfile != null && x.WorkforceProfile.Employee != null ? x.WorkforceProfile.Employee.EmployeeNumber : (x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty),
                    DepartmentName = x.WorkforceProfile != null && x.WorkforceProfile.PrimaryDepartment != null ? x.WorkforceProfile.PrimaryDepartment.DepartmentName : string.Empty,
                    PositionName = x.WorkforceProfile != null && x.WorkforceProfile.PrimaryPosition != null ? x.WorkforceProfile.PrimaryPosition.PositionName : string.Empty,
                    RetirementType = x.RetirementType,
                    NormalRetirementDate = x.NormalRetirementDate,
                    ActualRetirementDate = x.ActualRetirementDate,
                    RetirementAge = x.RetirementAge,
                    RetirementStatus = x.RetirementStatus,
                    EmployeeSeparationId = x.EmployeeSeparationId,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                    ApprovedAt = x.ApprovedAt,
                    Notes = x.Notes,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RetirementListResponse>
            {
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalData = totalItems,
                TotalPage = (int)Math.Ceiling(totalItems / (double)safePageSize),
                Items = items
            };
        }

        /// <summary>
        /// Kandidat pensiun dari master pegawai aktif (BE-HRD-368). Mengembalikan <c>null</c> bila usia
        /// pensiun normal RS belum ditetapkan (<see cref="PolicyNotConfiguredCode"/>): penetapan kandidat
        /// otomatis diblokir, tetapi kasus pensiun yang sudah ada tetap dapat dibaca.
        /// </summary>
        /// <param name="minAge">Penyaring tambahan usia saat ini (opsional), bukan usia pensiun.</param>
        /// <param name="cancellationToken">Token pembatalan.</param>
        public async Task<List<EligibleRetirementEmployeeResponse>?> GetEligibleEmployeesAsync(
            int? minAge = null,
            CancellationToken cancellationToken = default)
        {
            var policyBySite = await LoadRetirementPolicyBySiteAsync(cancellationToken);
            if (policyBySite.Count == 0)
            {
                return null;
            }

            return await ComputeApproachingAsync(policyBySite, minAge, 50, cancellationToken);
        }

        private async Task<List<EligibleRetirementEmployeeResponse>> ComputeApproachingAsync(
            Dictionary<Guid, int> policyBySite,
            int? minAge,
            int take,
            CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;

            // Prasaring di database memakai usia kebijakan terkecil supaya tidak ada kandidat yang terlewat;
            // usia per orang dihitung sesudahnya dari RS penempatannya.
            var youngestPolicyAge = policyBySite.Values.Min();
            var earliestQualifyingAge = Math.Max(0, youngestPolicyAge - ApproachingWindowYears);
            if (minAge.HasValue)
            {
                earliestQualifyingAge = Math.Max(earliestQualifyingAge, minAge.Value);
            }
            var maxBirthDate = today.AddYears(-earliestQualifyingAge);

            var employees = await _dbContext.MstEmployees
                .AsNoTracking()
                .Include(e => e.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(e => e.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryPosition)
                .Where(x => !x.IsDelete && x.IsActive && x.BirthDate <= maxBirthDate)
                .OrderBy(x => x.BirthDate)
                .ToListAsync(cancellationToken);

            var siteByProfile = await LoadPrimarySiteByProfileAsync(
                employees.Select(x => x.WorkforceProfileId).Distinct().ToList(),
                cancellationToken);

            var windowEnd = today.AddYears(ApproachingWindowYears);
            var result = new List<EligibleRetirementEmployeeResponse>();
            foreach (var emp in employees)
            {
                siteByProfile.TryGetValue(emp.WorkforceProfileId, out var siteId);
                var policyAge = ResolveAgeForSite(policyBySite, siteId);
                if (!policyAge.HasValue)
                {
                    continue;
                }

                var birthDate = emp.BirthDate;
                var age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age)) age--;

                if (minAge.HasValue && age < minAge.Value)
                {
                    continue;
                }

                var projectedDate = birthDate.AddYears(policyAge.Value);
                if (projectedDate.Date > windowEnd)
                {
                    continue;
                }

                var remainingMonths = ((projectedDate.Year - today.Year) * 12) + projectedDate.Month - today.Month;

                result.Add(new EligibleRetirementEmployeeResponse
                {
                    WorkforceProfileId = emp.WorkforceProfileId,
                    EmployeeId = emp.Id,
                    EmployeeName = emp.FullName,
                    EmployeeNip = emp.EmployeeNumber,
                    DepartmentName = emp.WorkforceProfile?.PrimaryDepartment?.DepartmentName ?? string.Empty,
                    PositionName = emp.WorkforceProfile?.PrimaryPosition?.PositionName ?? string.Empty,
                    BirthDate = birthDate,
                    CurrentAge = age,
                    ProjectedRetirementDate = projectedDate,
                    RemainingMonths = remainingMonths
                });
            }

            return result;
        }

        public async Task<RetirementDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var x = await _dbContext.TrxRetirements
                .AsNoTracking()
                .Include(r => r.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .Include(r => r.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(r => r.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryPosition)
                .Include(r => r.BenefitPlan)
                .Include(r => r.EmployeeSeparation)
                .Include(r => r.ApprovedByUser)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDelete, cancellationToken);

            if (x == null) return null;

            var profile = x.WorkforceProfile;
            var emp = profile?.Employee;
            int? serviceYears = null;
            if (emp != null)
            {
                var refDate = x.ActualRetirementDate ?? x.NormalRetirementDate;
                serviceYears = Math.Max(0, refDate.Year - emp.JoinDate.Year);
            }

            return new RetirementDetailResponse
            {
                Id = x.Id,
                RetirementNumber = x.RetirementNumber,
                WorkforceProfileId = x.WorkforceProfileId,
                EmployeeId = x.EmployeeId ?? emp?.Id,
                EmployeeName = profile?.DisplayName ?? string.Empty,
                EmployeeNip = emp?.EmployeeNumber ?? profile?.ProfileCode ?? string.Empty,
                DepartmentName = profile?.PrimaryDepartment?.DepartmentName ?? string.Empty,
                PositionName = profile?.PrimaryPosition?.PositionName ?? string.Empty,
                RetirementType = x.RetirementType,
                NormalRetirementDate = x.NormalRetirementDate,
                ActualRetirementDate = x.ActualRetirementDate,
                RetirementAge = x.RetirementAge,
                RetirementStatus = x.RetirementStatus,
                EmployeeSeparationId = x.EmployeeSeparationId,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByName = x.ApprovedByUser?.DisplayName,
                ApprovedAt = x.ApprovedAt,
                Notes = x.Notes,
                CreateDateTime = x.CreateDateTime,
                BenefitPlanId = x.BenefitPlanId,
                BenefitPlanName = x.BenefitPlan?.BenefitPlanName,
                BirthDate = emp?.BirthDate,
                HireDate = emp?.JoinDate,
                ServiceYears = serviceYears,
                SeparationNumber = x.EmployeeSeparation?.SeparationNumber
            };
        }

        public async Task<RetirementDetailResponse> CreateAsync(
            CreateRetirementRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var profile = await _dbContext.MstWorkforceProfiles
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == request.WorkforceProfileId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Workforce Profile pegawai tidak ditemukan.");

            var now = DateTime.UtcNow;
            // BE-HRD-368: nomor kasus dari number series bersama (RET-YYYYMM-NNNN), bukan cap waktu + GUID.
            var number = await AllocateNumberAsync(RetirementSequenceKey, RetirementPrefix, NumberSeriesResetPolicies.Monthly, actorUserId, cancellationToken);

            // Usia pensiun kasus: nilai yang diisi petugas, atau kebijakan RS bila tidak diisi.
            var retirementAge = request.RetirementAge;
            if (!retirementAge.HasValue)
            {
                var policyBySite = await LoadRetirementPolicyBySiteAsync(cancellationToken);
                if (policyBySite.Count > 0)
                {
                    var siteByProfile = await LoadPrimarySiteByProfileAsync(new[] { request.WorkforceProfileId }, cancellationToken);
                    siteByProfile.TryGetValue(request.WorkforceProfileId, out var siteId);
                    retirementAge = ResolveAgeForSite(policyBySite, siteId);
                }
            }

            var retirement = new TrxRetirement
            {
                Id = Guid.NewGuid(),
                RetirementNumber = number,
                WorkforceProfileId = request.WorkforceProfileId,
                EmployeeId = profile.Employee?.Id,
                RetirementType = string.IsNullOrWhiteSpace(request.RetirementType) ? "Normal" : request.RetirementType.Trim(),
                NormalRetirementDate = request.NormalRetirementDate,
                ActualRetirementDate = request.ActualRetirementDate ?? request.NormalRetirementDate,
                RetirementAge = retirementAge,
                BenefitPlanId = request.BenefitPlanId,
                RetirementStatus = "Planned",
                Notes = request.Notes?.Trim(),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.TrxRetirements.Add(retirement);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (await GetByIdAsync(retirement.Id, cancellationToken))!;
        }

        public async Task<RetirementDetailResponse> UpdateAsync(
            Guid id,
            UpdateRetirementRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var retirement = await _dbContext.TrxRetirements
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Kasus pensiun tidak ditemukan.");

            if (retirement.RetirementStatus == "Approved" || retirement.RetirementStatus == "HandoffCompleted")
            {
                throw new InvalidOperationException("Kasus pensiun yang sudah disetujui atau di-handoff tidak dapat diubah.");
            }

            if (!string.IsNullOrWhiteSpace(request.RetirementType))
            {
                retirement.RetirementType = request.RetirementType.Trim();
            }

            if (request.ActualRetirementDate.HasValue)
            {
                retirement.ActualRetirementDate = request.ActualRetirementDate.Value;
            }

            if (request.RetirementAge.HasValue)
            {
                retirement.RetirementAge = request.RetirementAge.Value;
            }

            if (request.BenefitPlanId.HasValue)
            {
                retirement.BenefitPlanId = request.BenefitPlanId.Value;
            }

            if (request.Notes != null)
            {
                retirement.Notes = request.Notes.Trim();
            }

            retirement.UpdateDateTime = DateTime.UtcNow;
            retirement.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return (await GetByIdAsync(retirement.Id, cancellationToken))!;
        }

        public async Task<RetirementDetailResponse> ApproveAsync(
            Guid id,
            ApproveRetirementRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var retirement = await _dbContext.TrxRetirements
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Kasus pensiun tidak ditemukan.");

            if (retirement.RetirementStatus == "Approved" || retirement.RetirementStatus == "HandoffCompleted")
            {
                return (await GetByIdAsync(retirement.Id, cancellationToken))!;
            }

            var now = DateTime.UtcNow;
            retirement.RetirementStatus = "Approved";
            retirement.ApprovedAt = now;
            retirement.ApprovedByUserId = actorUserId;
            if (!string.IsNullOrWhiteSpace(request?.Notes))
            {
                retirement.Notes = string.IsNullOrWhiteSpace(retirement.Notes)
                    ? request.Notes.Trim()
                    : $"{retirement.Notes}\n[Catatan Persetujuan: {request.Notes.Trim()}]";
            }
            retirement.UpdateDateTime = now;
            retirement.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return (await GetByIdAsync(retirement.Id, cancellationToken))!;
        }

        public async Task<RetirementDetailResponse> HandoffAsync(
            Guid id,
            RetirementHandoffRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var retirement = await _dbContext.TrxRetirements
                .Include(r => r.WorkforceProfile)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Kasus pensiun tidak ditemukan.");

            // Idempoten: jika sudah pernah handoff, langsung kembalikan data yang ada
            if (retirement.EmployeeSeparationId.HasValue)
            {
                await tx.RollbackAsync(cancellationToken);
                return (await GetByIdAsync(retirement.Id, cancellationToken))!;
            }

            // Validasi P0: status wajib Approved sebelum handoff ke pemisahan/offboarding
            if (!string.Equals(retirement.RetirementStatus, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                await tx.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"Kasus pensiun belum disetujui (status saat ini: {retirement.RetirementStatus}). Hanya kasus pensiun yang berstatus 'Approved' yang dapat dilakukan serah terima (handoff).");
            }

            var now = DateTime.UtcNow;
            // BE-HRD-368: nomor separation memakai deret SEP bersama milik modul hubungan kerja (BE-HRD-366),
            // nomor clearance dari deret EXC; keduanya atomik dan unik.
            var separationNumber = await AllocateNumberAsync(
                EmployeeRelationNumberSeries.Keys.Separation,
                EmployeeRelationNumberSeries.Prefixes.Separation,
                EmployeeRelationNumberSeries.ResetPolicies.Separation,
                actorUserId,
                cancellationToken);
            var clearanceNumber = await AllocateNumberAsync(ExitClearanceSequenceKey, ExitClearancePrefix, NumberSeriesResetPolicies.Monthly, actorUserId, cancellationToken);
            var effectiveDate = retirement.ActualRetirementDate ?? retirement.NormalRetirementDate;

            var separation = new TrxEmployeeSeparation
            {
                Id = Guid.NewGuid(),
                SeparationNumber = separationNumber,
                WorkforceProfileId = retirement.WorkforceProfileId,
                EmployeeId = retirement.EmployeeId,
                SeparationType = "Retirement",
                RequestDate = retirement.CreateDateTime,
                NoticeDate = retirement.CreateDateTime,
                ApprovedDate = retirement.ApprovedAt ?? now,
                EffectiveSeparationDate = effectiveDate,
                LastWorkingDate = effectiveDate,
                SeparationStatus = "Approved",
                ApprovalStatus = "Approved",
                IsEligibleForRehire = false,
                ApprovedByUserId = retirement.ApprovedByUserId ?? actorUserId,
                ReasonText = $"Pensiun ({retirement.RetirementType})",
                Notes = request.Notes?.Trim() ?? retirement.Notes,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.TrxEmployeeSeparations.Add(separation);

            // Inisiasi Exit Clearance jika belum ada
            var exitClearance = new TrxExitClearance
            {
                Id = Guid.NewGuid(),
                ClearanceNumber = clearanceNumber,
                EmployeeSeparationId = separation.Id,
                WorkforceProfileId = retirement.WorkforceProfileId,
                EmployeeId = retirement.EmployeeId,
                StartDate = now,
                DueDate = effectiveDate,
                ClearanceStatus = "Draft",
                IsFinanceCleared = false,
                IsDepartmentCleared = false,
                IsAssetCleared = false,
                IsPayrollCleared = false,
                IsItAccessCleared = false,
                IsHrCleared = false,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.TrxExitClearances.Add(exitClearance);

            // Hubungkan kembali ke TrxRetirement
            retirement.EmployeeSeparationId = separation.Id;
            retirement.RetirementStatus = "HandoffCompleted";
            retirement.UpdateDateTime = now;
            retirement.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return (await GetByIdAsync(retirement.Id, cancellationToken))!;
        }
    }
}
